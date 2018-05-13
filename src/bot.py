"""
    Copyright (c) 2018 Elliott Pardee <me [at] vypr [dot] xyz>
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
import time

import discord

import central
from data.BGBookNames import start as bg_book_names
from handlers.commandlogic.settings import languages
from handlers.commands import CommandHandler
from handlers.verses import VerseHandler

dir_path = os.path.dirname(os.path.realpath(__file__))

config = configparser.ConfigParser()
config.read(dir_path + "/config.ini")

configVersion = configparser.ConfigParser()
configVersion.read(dir_path + "/config.example.ini")


class BibleBot(discord.AutoShardedClient):
    def __init__(self, *args, loop=None, **kwargs):
        super().__init__(self, *args, loop, **kwargs)

        self.shard = None
        self.total_shards = self.shard_count - 1
        self.current_page = None
        self.total_pages = None

    async def on_ready(self):
        if self.shard_id is None:
            self.shard = 1
        else:
            self.shard = self.shard_id

        mod_time = os.path.getmtime(
            dir_path + "/data/BGBookNames/books.json")

        now = time.time()
        one_week_ago = now - 60 * 60 * 24 * 7  # Number of seconds in two days

        if mod_time < one_week_ago:
            bg_book_names.getBooks()

        central.logMessage("info", self.shard_id,
                           "global", "global", "connected")

        await self.change_presence(status=discord.Status.online,
                                   activity=discord.Game(
                                       config["meta"]["name"] + " v" +
                                       configVersion["meta"]["version"] +
                                       " | Shard: " + str(self.shard) + " / " +
                                       str(self.total_shards)))

    async def on_message(self, raw):
        sender = raw.author
        identifier = sender.name + "#" + sender.discriminator
        channel = raw.channel
        message = raw.content
        guild = None

        if config["BibleBot"]["devMode"] == "True":
            if str(sender.id) != config["BibleBot"]["owner"]:
                return

        if sender == self.user:
            return

        language = languages.getLanguage(sender)

        if language is None:
            language = "english_us"

        if hasattr(channel, "guild"):
            guild = channel.guild
            if hasattr(channel.guild, "name"):
                source = channel.guild.name + "#" + channel.name
            else:
                source = "unknown (direct messages?)"

            if "Discord Bot" in channel.guild.name:
                if sender.id != config["BibleBot"]["owner"]:
                    return
        else:
            source = "unknown (direct messages?)"

        embed_or_reaction_not_allowed = False

        if guild is not None:
            perms = channel.permissions_for(guild.me)

            if perms is not None:
                if not perms.send_messages or not perms.read_messages:
                    return

                if not perms.embed_links:
                    embed_or_reaction_not_allowed = True

                if not perms.add_reactions:
                    embed_or_reaction_not_allowed = True

        if message.startswith(config["BibleBot"]["commandPrefix"]):
            if embed_or_reaction_not_allowed:
                await channel.send("I need 'Embed Links'" +
                                   " and 'Add Reactions' permissions!")
                return

            command = message[1:].split(" ")[0]
            args = message.split(" ")

            if not isinstance(args.pop(0), str):
                args = None

            raw_language = eval(
                "central.languages." + str(language))
            raw_language = raw_language.rawObject

            cmd_handler = CommandHandler()

            res = cmd_handler.processCommand(
                bot, command, language, sender, args)

            original_command = ""
            self.current_page = 1

            if res is None:
                return

            if res is not None:
                if guild is not None:
                    if central.isBanned(str(guild.id)):
                        await channel.send("This server has been banned" +
                                           " from using BibleBot.")
                        await channel.send("If this is invalid, the server " +
                                           "owner may appeal by contacting " +
                                           "vypr#9944.")
                        central.logMessage(
                            "err", self.shard, identifier, source, "Server is " +
                            "banned.")
                        return

            if central.isBanned(str(sender.id)):
                await channel.send(sender.mention +
                                   " You have been banned from " +
                                   "using BibleBot.")
                await channel.send("You may appeal by " +
                                   "contacting vypr#9944.")
                central.logMessage("err", self.shard, identifier,
                                   source, "User is banned.")
                return

            if "leave" in res:
                if res["leave"] == "this":
                    if guild is not None:
                        await guild.leave()
                else:
                    for item in bot.guilds:
                        if str(item.id) == res["leave"]:
                            await item.leave()
                            await channel.send("Left " + str(item.name))

                central.logMessage("info", self.shard, identifier, source, "+leave")
                return

            if "isError" not in res:
                if "announcement" not in res:
                    if "twoMessages" in res:
                        await channel.send(res["firstMessage"])
                        await channel.send(res["secondMessage"])
                    elif "paged" in res:
                        self.total_pages = len(res["pages"])

                        msg = await channel.send(
                            embed=res["pages"][0])
                        await msg.add_reaction("⬅")
                        await msg.add_reaction("➡")

                        def check(r, u):
                            if r.message.id == msg.id:
                                if str(r.emoji) == "⬅":
                                    if u.id != bot.user.id:
                                        if self.current_page != 1:
                                            self.current_page -= 1
                                            return True
                                elif str(reaction.emoji) == "➡":
                                    if u.id != bot.user.id:
                                        if self.current_page != self.total_pages:
                                            self.current_page += 1
                                            return True

                        continue_paging = True

                        try:
                            while continue_paging:
                                reaction, user = await bot.wait_for(
                                    'reaction_add', timeout=120.0, check=check)
                                await reaction.message.edit(
                                    embed=res["pages"][self.current_page - 1])
                                reaction, user = await bot.wait_for(
                                    'reaction_remove', timeout=120.0,
                                    check=check)
                                await reaction.message.edit(
                                    embed=res["pages"][self.current_page - 1])

                        except (asyncio.TimeoutError, IndexError):
                            msg.clear_reactions()
                    else:
                        if "reference" not in res and "text" not in res:
                            await channel.send(embed=res["message"])
                        else:
                            if res["message"] is not None:
                                await channel.send(res["message"])
                            else:
                                await channel.send("Done.")

                    for original_command_name in raw_language["commands"].keys():
                        if raw_language["commands"][original_command_name] == command:  # noqa: E501
                            original_command = original_command_name
                        elif command == "setlanguage":
                            original_command = "setlanguage"
                        elif command == "ban":
                            original_command = "ban"
                        elif command == "unban":
                            original_command = "unban"
                        elif command == "eval":
                            original_command = "eval"
                        elif command == "jepekula":
                            original_command = "jepekula"
                        elif command == "joseph":
                            original_command = "joseph"
                        elif command == "tiger":
                            original_command = "tiger"
                else:
                    for original_command_name in raw_language["commands"].keys():
                        if raw_language["commands"][original_command_name] == command:  # noqa: E501
                            original_command = original_command_name

                    count = 1
                    total = len(bot.guilds)

                    for item in bot.guilds:
                        if "Discord Bot" not in item.name:
                            if str(item.id) != "362503610006765568":
                                sent = False

                                preferred = ["misc", "bots", "meta", "hangout",
                                             "fellowship", "lounge",
                                             "congregation", "general",
                                             "taffer", "family_text", "staff"]

                                for ch in item.text_channels:
                                    try:
                                        if not sent:
                                            for name in preferred:
                                                if ch.name == name:
                                                    if not sent:
                                                        perm = ch.permissions_for(  # noqa: E501
                                                            item.me)

                                                        if perm.read_messages:
                                                            if perm.send_messages:  # noqa: E501
                                                                await channel.send(str(count) +  # noqa: E501
                                                                                "/" + str(total) + " - " +  # noqa: E501
                                                                                item.name + " :white_check_mark:")  # noqa: E501
                                                                if perm.embed_links:  # noqa: E501
                                                                    await ch.send(  # noqa: E501
                                                                        embed=res["message"])  # noqa: E501
                                                                else:
                                                                    await ch.send(res["message"].fields[0].value)  # noqa: E501
                                                            else:
                                                                await channel.send(str(count) +  # noqa: E501
                                                                                "/" + str(total) + " - " +  # noqa: E501
                                                                                item.name + " :regional_indicator_x:")  # noqa: E501
                                                            count += 1
                                                            sent = True
                                    except Exception:
                                        sent = False
                            else:
                                for ch in item.text_channels:
                                    if ch.name == "announcements":
                                        await ch.send(embed=res["message"])

                    await channel.send("Done.")

                clean_args = str(args).replace(
                    ",", " ").replace("[", "").replace(
                        "]", "").replace("\"", "").replace(
                            "'", "").replace(
                                "  ", " ")

                if original_command == "puppet":
                    clean_args = ""
                elif original_command == "eval":
                    clean_args = ""
                elif original_command == "announce":
                    clean_args = ""

                central.logMessage(res["level"], self.shard, identifier,
                                   source, "+" + original_command +
                                   " " + clean_args)
            else:
                await channel.send(embed=res["return"])
        else:
            verse_handler = VerseHandler()

            result = verse_handler.process_raw_message(raw, sender, language)

            if result is not None:
                if guild is not None:
                    if central.isBanned(str(guild.id)):
                        await channel.send("This server has been banned " +
                                           "from using BibleBot.")
                        await channel.send("If this is invalid, the server " +
                                           "owner may appeal by contacting " +
                                           "vypr#9944.")
                        central.logMessage(
                            "err", self.shard, identifier,
                            source, "Server is banned.")
                        return

                if central.isBanned(str(sender.id)):
                    await channel.send(sender.mention +
                                       " You have been banned from " +
                                       "using BibleBot.")
                    await channel.send("You may appeal by " +
                                       "contacting vypr#9944.")
                    central.logMessage(
                        "err", self.shard, identifier, source, "User is banned.")
                    return

                if "invalid" not in result and "spam" not in result:
                    if embed_or_reaction_not_allowed:
                        await channel.send("I need 'Embed Links'" +
                                           " and 'Add Reactions' permissions!")
                        return

                    for item in result:
                        try:
                            if "twoMessages" in item:
                                await channel.send(item["firstMessage"])
                                await channel.send(item["secondMessage"])
                            elif "message" in item:
                                await channel.send(item["message"])
                        except KeyError:
                            item = item

                        if "reference" in item:
                            central.logMessage(item["level"], self.shard,
                                               identifier, source,
                                               item["reference"])
                else:
                    if "spam" in result:
                        await channel.send(result["spam"])


bot = BibleBot()
central.logMessage("info", 0,
                   "global", "global", "BibleBot v" +
                   configVersion["meta"]["version"] +
                   " by Elliott Pardee (vypr)")
bot.run(config["BibleBot"]["token"])
