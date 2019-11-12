"""
    Copyright (c) 2018-2019 Elliott Pardee <me [at] vypr [dot] xyz>
    This file is part of BibleBot.

    BibleBot is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    BibleBot is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with BibleBot.  If not, see <http://www.gnu.org/licenses/>.
"""

import asyncio
import configparser
import os

import discord

import central
from name_scraper import client as name_scraper
from handlers.logic.settings import languages
from handlers.commands import CommandHandler
from handlers.verses import VerseHandler
from extensions import bot_extensions, compile_extrabiblical

dir_path = os.path.dirname(os.path.realpath(__file__))

config = configparser.ConfigParser()
config.read(f"{dir_path}/config.ini")

configVersion = configparser.ConfigParser()
configVersion.read(f"{dir_path}/config.example.ini")


class BibleBot(discord.AutoShardedClient):
    def __init__(self, *args, loop=None, **kwargs):
        super().__init__(*args, loop=loop, **kwargs)

        self.bg_task = self.loop.create_task(bot_extensions.run_timed_votds(self))
        self.current_page = None
        self.total_pages = None

    async def on_ready(self):
        if int(config["BibleBot"]["shards"]) < 2:
            status = f"+biblebot {central.version} | Shard: 1 / 1"
            activity = discord.Game(status)

            await self.change_presence(status=discord.Status.online, activity=activity)

            central.log_message("info", 1, "global", "global", "connected")

        await bot_extensions.send_server_count(self)

    async def on_shard_ready(self, shard_id):
        shard_count = str(config["BibleBot"]["shards"])
        s_id = str(shard_id + 1)
        status = f"+biblebot {central.version} | Shard {s_id} / {shard_count}"

        activity = discord.Game(status)
        await self.change_presence(status=discord.Status.online, activity=activity, shard_id=shard_id)

        central.log_message("info", shard_id + 1, "global", "global", "shard connected")

    async def on_guild_join(self):
        await bot_extensions.send_server_count(self)

    async def on_guild_remove(self):
        await bot_extensions.send_server_count(self)

    async def on_message(self, raw):
        owner_id = config["BibleBot"]["owner"]
        await self.wait_until_ready()

        if ":" not in raw.content:
            if config["BibleBot"]["commandPrefix"] not in raw.content:
                return

        ctx = {
            "self": bot,
            "author": raw.author,
            "identifier": f"{raw.author.id}",
            "channel": raw.channel,
            "message": raw.content,
            "raw": raw,
            "guild": None,
            "language": None
        }

        is_self = ctx["author"] == self.user
        is_optout = central.is_optout(str(ctx["author"].id))
        if is_self or is_optout:
            return

        is_owner = ctx["identifier"] == owner_id
        if is_owner:
            ctx["identifier"] = "owner"

        language = languages.get_language(ctx["author"])

        if hasattr(ctx["channel"], "guild"):
            ctx["guild"] = ctx["channel"].guild

            if language is None:
                language = languages.get_guild_language(ctx["guild"])

            guild_id = str(ctx["channel"].guild.id)
            chan_id = str(ctx["channel"].id)

            source = f"{guild_id}#{chan_id}"

            if "Discord Bot" in ctx["channel"].guild.name:
                if not is_owner:
                    return
        else:
            source = "unknown (direct messages?)"

        if ctx["guild"] is None:
            shard = 1
        else:
            shard = ctx["guild"].shard_id + 1

        if language is None:
            language = "english"

        if config["BibleBot"]["devMode"] == "True":
            # more often than not, new things are added that aren't filtered
            # through crowdin yet so we do this to avoid having to deal with
            # missing values
            language = "default"

            if not is_owner:
                return

        embed_or_reaction_not_allowed = False

        if ctx["guild"] is not None:
            try:
                perms = ctx["channel"].permissions_for(ctx["guild"].me)

                if perms is not None:
                    if not perms.send_messages or not perms.read_messages:
                        return

                    if not perms.embed_links:
                        embed_or_reaction_not_allowed = True

                    if not perms.add_reactions:
                        embed_or_reaction_not_allowed = True

                    no_managing = not perms.manage_messages
                    no_history = not perms.read_message_history

                    if no_managing or no_history:
                        embed_or_reaction_not_allowed = True
            except AttributeError:
                pass

        ctx["language"] = central.get_raw_language(language)

        if ctx["message"].startswith(config["BibleBot"]["commandPrefix"]):
            command = ctx["message"][1:].split(" ")[0]
            remainder = " ".join(ctx["message"].split(" ")[1:])

            cmd_handler = CommandHandler()

            res = await cmd_handler.process_command(ctx, command, remainder)

            original_command = ""
            self.current_page = 1

            if res is None:
                return

            if "announcement" in res:
                await bot_extensions.send_announcement(ctx, res)
                return

            if "isError" not in res:
                if embed_or_reaction_not_allowed:
                    ch = ctx["channel"]

                    await ch.send("Permissions are not properly configured.")
                    await ch.send("Please check https://biblebot.xyz/permissions for more information.")
                    return

                if "twoMessages" in res:
                    await ctx["channel"].send(res["firstMessage"])
                    await ctx["channel"].send(res["secondMessage"])
                elif "paged" in res:
                    self.total_pages = len(res["pages"])

                    msg = await ctx["channel"].send(embed=res["pages"][0])

                    await msg.add_reaction("⬅")
                    await msg.add_reaction("➡")

                    def check(r, u):
                        if r.message.id == msg.id:
                            if str(r.emoji) == "⬅":
                                if u.id != bot.user.id:
                                    if self.current_page != 1:
                                        self.current_page -= 1
                                        return True
                            elif str(r.emoji) == "➡":
                                if u.id != bot.user.id:
                                    if self.current_page != self.total_pages:
                                        self.current_page += 1
                                        return True

                    continue_paging = True

                    try:
                        while continue_paging:
                            reaction, user = await bot.wait_for('reaction_add', timeout=60.0, check=check)
                            await reaction.message.edit(embed=res["pages"][self.current_page - 1])

                            reaction, user = await bot.wait_for('reaction_remove', timeout=60.0, check=check)
                            await reaction.message.edit(embed=res["pages"][self.current_page - 1])
                    except (asyncio.TimeoutError, IndexError):
                        try:
                            await msg.clear_reactions()
                        except (discord.errors.Forbidden, discord.errors.NotFound):
                            pass
                else:
                    if "reference" not in res and "text" not in res:
                        await ctx["channel"].send(embed=res["message"])

                lang = central.get_raw_language(language)
                for original_command_name in lang["commands"].keys():
                    untranslated = ["setlanguage", "userid", "ban", "unban",
                                    "reason", "optout", "unoptout", "eval",
                                    "jepekula", "joseph", "tiger",
                                    "lsc", "heidelberg", "ccc"]

                    if lang["commands"][original_command_name] == command:
                        original_command = original_command_name
                    elif command in untranslated:
                        original_command = command

                clean_args = remainder.replace("\"", "").replace("'", "").replace("  ", " ")
                clean_args = clean_args.replace("\n", "").strip()

                ignore_arg_commands = ["puppet", "eval", "announce"]

                if original_command in ignore_arg_commands:
                    clean_args = ""

                central.log_message(res["level"], shard, ctx["identifier"], source, f"+{original_command} {clean_args}")
            else:
                await ctx["channel"].send(embed=res["message"])
        else:
            verse_handler = VerseHandler()

            result = verse_handler.process_raw_message(raw, ctx["author"], ctx["language"], ctx["guild"])

            if result is not None:
                if embed_or_reaction_not_allowed:
                    ch = ctx["channel"]

                    await ch.send("Permissions are not properly configured.")
                    await ch.send("Please check https://biblebot.xyz/permissions for more information.")
                    return

                if "invalid" not in result and "spam" not in result:
                    for item in result:
                        try:
                            if "twoMessages" in item:
                                await ctx["channel"].send(item["firstMessage"])
                                await ctx["channel"].send(item["secondMessage"])  # noqa: E501
                            elif "message" in item:
                                await ctx["channel"].send(item["message"])
                            elif "embed" in item:
                                await ctx["channel"].send(embed=item["embed"])
                        except KeyError:
                            pass

                        if "reference" in item:
                            central.log_message(item["level"], shard, ctx["identifier"],  source, item["reference"])
                elif "spam" in result:
                    central.log_message("warn", shard,
                                        ctx["identifier"], source,
                                        "Too many verses at once.")
                    await ctx["channel"].send(result["spam"])


if int(config["BibleBot"]["shards"]) > 1:
    bot = BibleBot(shard_count=int(config["BibleBot"]["shards"]))
else:
    bot = BibleBot()

name_scraper.update_books(config["apis"]["apibible"])

if config["BibleBot"]["devMode"] == "True":
    compile_extrabiblical.compile_resources()

central.log_message("info", 0, "global", "global",
                    f"BibleBot {central.version} by Elliott Pardee (vypr)")
bot.run(config["BibleBot"]["token"])
