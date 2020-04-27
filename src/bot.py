"""
    Copyright (c) 2018-2020 Elliott Pardee <me [at] thevypr [dot] com>
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
import sys
import pathlib
import datetime
import math

import discord
from discord.ext import tasks

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

        self.current_page = {}
        self.total_pages = {}
        self.continue_paging = {}
        self.connection_counter = 1

    async def on_connect(self):
        central.log_message("info", self.connection_counter, "global", "global", "connected to discord")
        self.connection_counter += 1
        
        if self.connection_counter == int(config["BibleBot"]["shards"]):
            self.connection_counter = 1

    async def on_ready(self):
        if int(config["BibleBot"]["shards"]) < 2:
            status = f"+biblebot {central.version} | Shard: 1 / 1"
            activity = discord.Game(status)

            await self.change_presence(status=discord.Status.online, activity=activity)

            central.log_message("info", 1, "global", "global", "finished")

        self.heartbeat.start()
        await bot_extensions.send_server_count(self)

    @tasks.loop(seconds=1)
    async def heartbeat(self):
        output = ""

        for latency_item in self.latencies:
            shard, latency = latency_item
            output += f"{shard + 1}: {math.ceil(latency * 100)}ms, "

        central.log_message("info", 0, "global", "global", output[:-2])

    async def on_shard_ready(self, shard_id):
        shard_count = str(config["BibleBot"]["shards"])
        s_id = str(shard_id + 1)
        status = f"+biblebot {central.version} | Shard {s_id} / {shard_count}"

        activity = discord.Game(status)
        await self.change_presence(status=discord.Status.online, activity=activity, shard_id=shard_id)

        central.log_message("info", shard_id + 1, "global", "global", "prepared")

    async def on_disconnect(self):
        central.log_message("info", 0, "global", "global", "disconnected")

    async def on_error(self, event, *args, **kwargs):
        output = f"{event}\n\nargs: {args}\n\nkwargs: {kwargs}\n\nex: {sys.exc_info()}"
        
        pathlib.Path("./error_logs").mkdir(exist_ok=True)
        current_time = datetime.now().strftime("%Y-%m-%d_%H:%M:%S")
        output_file = open(f"./error_logs/log-{current_time}.txt", "w")

        output_file.write(output)
        
    async def on_guild_join(self, guild):
        await bot_extensions.send_server_count(self)

    async def on_guild_remove(self, guild):
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

        if language is None or language in ["english_us", "english_uk"]:
            language = "english"

        if config["BibleBot"]["devMode"] == "True":
            # more often than not, new things are added that aren't filtered
            # through crowdin yet so we do this to avoid having to deal with
            # missing values
            language = "default"

            if not is_owner:
                return

        ctx["language"] = central.get_raw_language(language)

        if ctx["message"].startswith(config["BibleBot"]["commandPrefix"]):
            command = ctx["message"][1:].split(" ")[0]
            remainder = " ".join(ctx["message"].split(" ")[1:])

            cmd_handler = CommandHandler()

            res = await cmd_handler.process_command(ctx, command, remainder)

            original_command = ""

            if res is None:
                return

            if "announcement" in res:
                central.log_message("info", 0, "owner", "global", "Announcement starting.")
                await bot_extensions.send_announcement(ctx, res)
                central.log_message("info", 0, "owner", "global", "Announcement finished.")
                return

            if "isError" not in res:
                if "twoMessages" in res:
                    await ctx["channel"].send(res["firstMessage"])
                    await ctx["channel"].send(res["secondMessage"])
                elif "paged" in res:
                    msg = await ctx["channel"].send(embed=res["pages"][0])

                    self.current_page[msg.id] = 1
                    self.total_pages[msg.id] = len(res["pages"])

                    await msg.add_reaction("⬅")
                    await msg.add_reaction("➡")

                    def check(r, u):
                        if r.message.id == msg.id:
                            if str(r.emoji) == "⬅":
                                if u.id != bot.user.id:
                                    if self.current_page[msg.id] != 1:
                                        self.current_page[msg.id] -= 1
                                        return True
                            elif str(r.emoji) == "➡":
                                if u.id != bot.user.id:
                                    if self.current_page[msg.id] != self.total_pages[msg.id]:
                                        self.current_page[msg.id] += 1
                                        return True

                    self.continue_paging[msg.id] = True

                    try:
                        while self.continue_paging[msg.id]:
                            reaction, _ = await asyncio.ensure_future(bot.wait_for('reaction_add', timeout=60.0, check=check))
                            await reaction.message.edit(embed=res["pages"][self.current_page[msg.id] - 1])

                            reaction, _ = await asyncio.ensure_future(bot.wait_for('reaction_remove', timeout=60.0, check=check))
                            await reaction.message.edit(embed=res["pages"][self.current_page[msg.id] - 1])
                    except (asyncio.TimeoutError, IndexError):
                        try:
                            self.current_page.pop(msg.id)
                            self.total_pages.pop(msg.id)
                            self.continue_paging.pop(msg.id)

                            await msg.clear_reactions()
                        except (discord.errors.Forbidden, discord.errors.NotFound):
                            pass
                elif "embed" in res:
                    try:
                        await ctx["channel"].send(embed=res["embed"])
                    except discord.errors.Forbidden:
                        await ctx["channel"].send("Unable to send embed, please verify permissions.")
                else:
                    if "reference" not in res and "text" not in res:
                        await ctx["channel"].send(embed=res["message"])
                    else:
                        await ctx["channel"].send(res["message"])

                lang = central.get_raw_language(language)
                for original_command_name in lang["commands"].keys():
                    untranslated = ["setlanguage", "userid", "ban", "unban",
                                    "reason", "optout", "unoptout", "eval",
                                    "jepekula", "joseph", "tiger", "rose",
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

            result = await verse_handler.process_raw_message(raw, ctx["author"], ctx["language"], ctx["guild"])

            if result is not None:
                if "invalid" not in result and "spam" not in result:
                    for item in result:
                        try:
                            if "twoMessages" in item:
                                await ctx["channel"].send(item["firstMessage"])
                                await ctx["channel"].send(item["secondMessage"])
                            elif "message" in item:
                                await ctx["channel"].send(item["message"])
                            elif "embed" in item:
                                await ctx["channel"].send(embed=item["embed"])

                            if "reference" in item:
                                central.log_message(item["level"], shard, ctx["identifier"],  source, item["reference"])
                        except (KeyError, TypeError):
                            pass
                elif "spam" in result:
                    central.log_message("warn", shard,
                                        ctx["identifier"], source,
                                        "Too many verses at once.")
                    await ctx["channel"].send(result["spam"])


if int(config["BibleBot"]["shards"]) > 1:
    bot = BibleBot(shard_count=int(config["BibleBot"]["shards"]))
else:
    bot = BibleBot()

if config["BibleBot"]["devMode"] == "True":
    compile_extrabiblical.compile_resources()
    asyncio.run(name_scraper.update_books(dry=True))
elif len(sys.argv) == 2 and any([sys.argv[1] == x for x in ["-d", "--dry"]]):
    asyncio.run(name_scraper.update_books(dry=True))
else:
    asyncio.run(name_scraper.update_books(config["apis"]["apibible"]))

central.log_message("info", 0, "global", "global", f"BibleBot {central.version} by Elliott Pardee (vypr)")

bot_extensions.run_timed_votds.start(bot)
bot.run(config["BibleBot"]["token"])
