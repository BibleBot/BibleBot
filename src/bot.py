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

from handlers.commandlogic.settings import languages, versions, formatting
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

        totalShards = self.shard_count

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

        if (config["BibleBot"]["devMode"] == "True"):
            if str(sender.id) != config["BibleBot"]["owner"]:
                return

        if sender == self.user:
            return

        language = languages.getLanguage(sender)

        if isinstance(channel.guild.name, str):
            if isinstance(channel.name, str):
                source = channel.guild.name + "#" + channel.name
            else:
                source = "unknown (direct messages?)"

            if "Discord Bot" in channel.guild.name:
                if sender.id != config["BibleBot"]["owner"]:
                    return
        else:
            source = "unknown (direct messages?)"

        if message[0] == "+":
            command = message[1:].split(" ")[0]
            args = message.split(" ")

            if isinstance(args.pop(0), str) is False:
                args = None

            rawLanguage = eval("central.languages." + language)
            rawLanguage = rawLanguage.rawObject

            cmdHandler = CommandHandler()

            res = cmdHandler.processCommand(
                bot, command, language, sender, args)

            originalCommand = ""

            if res["isError"] is False:
                if res["announcement"] is False:
                    if res["twoMessages"]:
                        channel.send(res.first)
                        channel.send(res.second)
                    elif res["paged"] and len(res["pages"]) != 0:
                        currentPage = 1
                        totalPages = len(res["pages"])

                        msg = channel.send(embed=res["pages"][currentPage - 1])
                        await msg.add_reaction("⬅")
                        await msg.add_reaction("➡")

                        def check(reaction, user):
                            if reaction.message.id == msg.id:
                                if str(reaction.emoji) == "⬅":
                                    if user.id != bot.user.id:
                                        if currentPage == 1:
                                            return
                                        else:
                                            currentPage -= 1
                                            msg.edit(
                                                res.pages[currentPage - 1])
                                elif str(reaction.emoji) == "➡":
                                    if user.id != bot.user.id:
                                        if currentPage == totalPages:
                                            return
                                        else:
                                            currentPage += 1
                                            msg.edit(
                                                res.pages[currentPage - 1])

                        try:
                            reaction, user = await bot.wait_for('reaction_add', timeout=120.0, check=check)  # noqa: E501
                        except asyncio.TimeoutError:
                            await msg.clear_reactions()
                    else:
                        channel.send(res.message)

                    for originalCommandName in rawLanguage.commands.keys():
                        if rawLanguage.commands[originalCommandName] == command:  # noqa: E501
                            originalCommand = originalCommandName
                        elif command == "eval":
                            originalCommand = "eval"
                        elif command == "jepekula":
                            originalCommand = "jepekula"
                        elif command == "joseph":
                            originalCommand = "joseph"
                else:
                    for originalCommandName in rawLanguage.commands.keys():
                        if rawLanguage.commands[originalCommandName] == command:  # noqa: E501
                            originalCommand = originalCommandName

                    for guild in bot.guilds:
                        embed = discord.Embed()

                        embed.color = 303102
                        embed.set_footer("BibleBot v" + central.config["meta"]["version"],  # noqa: E501
                                         "https://cdn.discordapp.com/avatars/" +  # noqa: E501
                                         "361033318273384449/" +
                                         "5aad77425546f9baa5e4b5112696e10a.png")  # noqa: E501

                        embed.add_field("Announcement", res.message)

                        if "Discord Bot" in guild.name:
                            return

                        if guild.id != "362503610006765568":
                            sent = False
                            ch = [i for i, x in enumerate(
                                guild.channels) if isinstance(i, discord.TextChannel)]  # noqa: E501

                            preferred = ["misc", "bots", "meta", "hangout",
                                         "fellowship", "lounge",
                                         "congregation", "general",
                                         "taffer", "family_text", "staff"]

                            for i in range(0, len(preferred)):
                                if sent is False:
                                    receiver = [
                                        j for j, x in ch if x.name == preferred[i]]  # noqa: E501

                                    if receiver:
                                        receiver.send(embed=embed)
                                        sent = True
                        else:
                            ch = [i for i, x in enumerate(
                                guild.channels) if isinstance(i, discord.TextChannel)]  # noqa: E501

                            receiver = [j for j, x in ch if x.name == "announcements"]  # noqa: E501

                            if receiver:
                                receiver.send(embed=embed)
                                sent = True

                cleanArgs = str(args).replace(",", " ")

                if originalCommand == "puppet":
                    cleanArgs = ""
                elif originalCommand == "eval":
                    cleanArgs = ""
                elif originalCommand == "announce":
                    cleanArgs = ""

                central.logMessage(res.level, shard, identifier,
                                   source, "+" + originalCommand +
                                   " " + cleanArgs)
            else:
                channel.send(res["return"])
        else:
            verseHandler = VerseHandler()

            result = verseHandler.processRawMessage(
                shard, raw, sender, language)

            if result.invalid is False:
                if result.twoMessages:
                    channel.send(result.firstMessage)
                    channel.send(result.secondMessage)
                else:
                    channel.send(result.message)

                if result.reference:
                    central.logMessage(result.level, shard,
                                       identifier, source, result.reference)


bot = BibleBot()
central.logMessage("info", 0,
                   "global", "global", "BibleBot v" +
                   config["meta"]["version"] +
                   " by Elliott Pardee (vypr)")
bot.run(config["BibleBot"]["token"])
