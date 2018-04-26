'''
    Copyright (c) 2018 Elliott Pardee <vypr [at] vypr [dot] space>
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
'''

import asyncio
import discord
import os
import central
import configparser

from handlers.commandlogic.settings import languages
from handlers.verses import VerseHandler
from handlers.commands import CommandHandler

dir_path = os.path.dirname(os.path.realpath(__file__))

config = configparser.ConfigParser()
config.read(dir_path + "/config.ini")

shard = None
totalShards = 1

currentPage = None
totalPages = None


class BibleBot(discord.AutoShardedClient):
    async def on_ready(self):
        if self.shard_id is None:
            shard = 1
        else:
            shard = self.shard_id

        totalShards = self.shard_count - 1

        central.logMessage("info", self.shard_id,
                           "global", "global", "connected")

        await self.change_presence(status=discord.Status.online,
                                   activity=discord.Game(
                                       config["meta"]["name"] + " v" +
                                       config["meta"]["version"] +
                                       " | Shard: " + str(shard) + " / " +
                                       str(totalShards)))

    async def on_message(self, raw):
        sender = raw.author
        identifier = sender.name + "#" + sender.discriminator
        channel = raw.channel
        message = raw.content
        guild = None

        if (config["BibleBot"]["devMode"] == "True"):
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

        if guild is not None:
            perms = channel.permissions_for(guild.me)

            if perms.send_messages is False or perms.embed_links is False:
                return

        if message.startswith(config["BibleBot"]["commandPrefix"]):
            command = message[1:].split(" ")[0]
            args = message.split(" ")

            if isinstance(args.pop(0), str) is False:
                args = None

            rawLanguage = eval("central.languages." + str(language))
            rawLanguage = rawLanguage.rawObject

            cmdHandler = CommandHandler()

            res = cmdHandler.processCommand(
                bot, command, language, sender, args)

            originalCommand = ""
            self.currentPage = 1

            if res is None:
                return

            if "isError" not in res:
                if "announcement" not in res:
                    if "twoMessages" in res:
                        await channel.send(res["firstMessage"])
                        await channel.send(res["secondMessage"])
                    elif "paged" in res:
                        self.totalPages = len(res["pages"])

                        msg = await channel.send(
                            embed=res["pages"][0])
                        await msg.add_reaction("⬅")
                        await msg.add_reaction("➡")

                        def check(reaction, user):
                            if reaction.message.id == msg.id:
                                if str(reaction.emoji) == "⬅":
                                    if user.id != bot.user.id:
                                        if self.currentPage != 1:
                                            self.currentPage -= 1
                                            return True
                                elif str(reaction.emoji) == "➡":
                                    if user.id != bot.user.id:
                                        if self.currentPage != self.totalPages:
                                            self.currentPage += 1
                                            return True

                        continuePaging = True
                        reaction = None
                        user = None

                        try:
                            while continuePaging:
                                reaction, user = await bot.wait_for(
                                    'reaction_add', timeout=120.0, check=check)
                                await reaction.message.edit(
                                    embed=res["pages"][self.currentPage - 1])
                                reaction, user = await bot.wait_for(
                                    'reaction_remove', timeout=120.0,
                                    check=check)
                                await reaction.message.edit(
                                    embed=res["pages"][self.currentPage - 1])

                        except asyncio.TimeoutError:
                            continuePaging = False
                    else:
                        if "reference" not in res and "text" not in res:
                            await channel.send(embed=res["message"])
                        else:
                            if res["message"] is not None:
                                await channel.send(res["message"])
                            else:
                                await channel.send("Done.")

                    for originalCommandName in rawLanguage["commands"].keys():
                        if rawLanguage["commands"][originalCommandName] == command:  # noqa: E501
                            originalCommand = originalCommandName
                        elif command == "eval":
                            originalCommand = "eval"
                        elif command == "jepekula":
                            originalCommand = "jepekula"
                        elif command == "joseph":
                            originalCommand = "joseph"
                        elif command == "tiger":
                            originalCommand = "tiger"
                else:
                    for originalCommandName in rawLanguage["commands"].keys():
                        if rawLanguage["commands"][originalCommandName] == command:  # noqa: E501
                            originalCommand = originalCommandName

                    for item in bot.guilds:
                        if "Discord Bot" in item.name:
                            return

                        if str(item.id) != "362503610006765568":
                            sent = False

                            preferred = ["misc", "bots", "meta", "hangout",
                                         "fellowship", "lounge",
                                         "congregation", "general",
                                         "taffer", "family_text", "staff"]

                            for i in range(0, len(preferred)):
                                if sent is False:
                                    for ch in item.text_channels:
                                        if ch.name == preferred[i]:
                                            await ch.send(embed=res["message"])
                                            sent = True
                        else:
                            for ch in item.text_channels:
                                if ch.name == "announcements":
                                    await ch.send(embed=res["message"])

                    await channel.send("Done.")

                cleanArgs = str(args).replace(
                    ",", " ").replace("[", "").replace(
                        "]", "").replace("\"", "").replace(
                            "'", "").replace(
                                "  ", " ")

                if originalCommand == "puppet":
                    cleanArgs = ""
                elif originalCommand == "eval":
                    cleanArgs = ""
                elif originalCommand == "announce":
                    cleanArgs = ""

                central.logMessage(res["level"], shard, identifier,
                                   source, "+" + originalCommand +
                                   " " + cleanArgs)
            else:
                await channel.send(embed=res["return"])
        else:
            verseHandler = VerseHandler()

            result = verseHandler.processRawMessage(
                shard, raw, sender, language)

            if result is not None:
                if "invalid" not in result and "spam" not in result:
                    for item in result:
                        if "twoMessages" in item:
                            await channel.send(item["firstMessage"])
                            await channel.send(item["secondMessage"])
                        else:
                            await channel.send(item["message"])

                        if "reference" in item:
                            central.logMessage(item["level"], shard,
                                               identifier, source,
                                               item["reference"])
                else:
                    if "spam" in result:
                        await channel.send(result["spam"])


bot = BibleBot()
central.logMessage("info", 0,
                   "global", "global", "BibleBot v" +
                   config["meta"]["version"] +
                   " by Seraphim Pardee")
bot.run(config["BibleBot"]["token"])
