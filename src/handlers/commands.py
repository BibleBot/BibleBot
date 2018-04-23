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

import sys
import os
import discord
from handlers.commandlogic import commandbridge as commandBridge

dir_path = os.path.dirname(os.path.realpath(__file__))
sys.path.append(dir_path + "/..")

import central  # noqa: E402
from vytypes.handler import Handler  # noqa: E402

commandMap = {
    "biblebot": 0,
    "search": 1,
    "versions": 0,
    "setversion": 1,
    "version": 0,
    "versioninfo": 1,
    "random": 0,
    "verseoftheday": 0,
    "votd": 0,
    "headings": 1,
    "versenumbers": 1,
    "languages": 0,
    "setlanguage": 1,
    "language": 0,
    "users": 0,
    "servers": 0,
    "invite": 0,

    "jepekula": 0,
    "joseph": 0,
    "supporters": 0,

    "addversion": 5
}


def isCommand(command, lang):
    commands = lang["commands"]

    result = {
        "ok": False
    }

    if command == "eval":
        result = {
            "ok": True,
            "orig": "eval"
        }
    elif command == "jepekula":
        result = {
            "ok": True,
            "orig": "jepekula"
        }
    elif command == "joseph":
        result = {
            "ok": True,
            "orig": "joseph"
        }
    else:
        for originalCommandName in commands.keys():
            if commands[originalCommandName] == command:
                result = {
                    "ok": True,
                    "orig": originalCommandName
                }

    return result


def isOwnerCommand(command, lang):
    commands = lang["commands"]

    if command == commands["leave"]:
        return True
    elif command == commands["puppet"]:
        return True
    elif command == commands["announce"]:
        return True
    elif command == commands["addversion"]:
        return True
    elif command == "eval":
        return True
    else:
        return False


class CommandHandler(Handler):
    def processCommand(self, bot, command, lang, sender, args=None):
        rawLanguage = eval("central.languages." + lang).rawObject
        commands = rawLanguage["commands"]

        properCommand = isCommand(command, rawLanguage)
        origCmd = properCommand["orig"]

        if properCommand["ok"]:
            if isOwnerCommand(origCmd, rawLanguage) is False:
                if origCmd != commands["search"]:
                    if origCmd != commands["headings"] and origCmd != commands["versenumbers"]:  # noqa: E501
                        if origCmd != commands["servers"] and origCmd != commands["users"]:  # noqa: E501
                            requiredArguments = commandMap[origCmd]

                            if args is None:
                                args = []

                            if len(args) != requiredArguments:
                                embed = discord.Embed()

                                embed.color = "#ff2e2e"
                                embed.set_footer("BibleBot v" + central.config["meta"]["version"],  # noqa: E501
                                                 "https://cdn.discordapp.com/avatars/" +  # noqa: E501
                                                 "361033318273384449/" +
                                                 "5aad77425546f9baa5e4b5112696e10a.png")  # noqa: E501

                                response = rawLanguage.argumentCountError.replace(  # noqa: E501
                                    "<command>", command).replace(
                                        "<count>", requiredArguments)

                                embed.add_field(rawLanguage.error, response)

                                return {
                                    "isError": True,
                                    "return": embed
                                }

                            return commandBridge.runCommand(origCmd, args,
                                                            rawLanguage,
                                                            sender)
                        else:
                            requiredArguments = commandMap[properCommand]

                            if args is None:
                                args = []

                            if len(args) != requiredArguments:
                                embed = discord.Embed()

                                embed.color = "#ff2e2e"
                                embed.set_footer("BibleBot v" + central.config["meta"]["version"],  # noqa: E501
                                                 "https://cdn.discordapp.com/avatars/" +  # noqa: E501
                                                 "361033318273384449/" +
                                                 "5aad77425546f9baa5e4b5112696e10a.png")  # noqa: E501

                                response = rawLanguage.argumentCountError.replace(  # noqa: E501
                                    "<command>", command).replace(
                                        "<count>", requiredArguments)

                                embed.add_field(rawLanguage.error, response)

                                return {
                                    "isError": True,
                                    "return": embed
                                }

                            return commandBridge.runCommand(origCmd, [bot],
                                                            rawLanguage,
                                                            sender)
                    else:
                        if args is None:
                            args = []

                        if len(args) == 0 or len(args) == 1:
                            return commandBridge.runCommand(origCmd, args, rawLanguage, sender)  # noqa: E501
                        else:
                            embed.color = "#ff2e2e"
                            embed.set_footer("BibleBot v" + central.config["meta"]["version"],  # noqa: E501
                                                 "https://cdn.discordapp.com/avatars/" +  # noqa: E501
                                                 "361033318273384449/" +
                                                 "5aad77425546f9baa5e4b5112696e10a.png")  # noqa: E501

                            response = rawLanguage.argumentCountError.replace(  # noqa: E501
                                "<command>", command).replace(
                                    "<count>", rawLanguage.zeroOrOne)

                            embed.add_field(rawLanguage.error, response)

                            return {
                                "isError": True,
                                "return": embed
                            }
                else:
                    if args is None:
                        args = []

                    if len(args) == 1 and len(args[0]) < 4:
                        embed.color = "#ff2e2e"
                        embed.set_footer("BibleBot v" + central.config["meta"]["version"],  # noqa: E501
                                                 "https://cdn.discordapp.com/avatars/" +  # noqa: E501
                                                 "361033318273384449/" +
                                                 "5aad77425546f9baa5e4b5112696e10a.png")  # noqa: E501

                        embed.add_field(rawLanguage.error,
                                        rawLanguage.queryTooShort)

                        return {
                            "isError": True,
                            "return": embed
                        }

                    if len(args) == 0:
                        embed.color = "#ff2e2e"
                        embed.set_footer("BibleBot v" + central.config["meta"]["version"],  # noqa: E501
                                                 "https://cdn.discordapp.com/avatars/" +  # noqa: E501
                                                 "361033318273384449/" +
                                                 "5aad77425546f9baa5e4b5112696e10a.png")  # noqa: E501

                        response = rawLanguage.argumentCountErrorAL.replace(
                            "<command>", command).replace("<count>", 1)

                        embed.add_field(rawLanguage.error, response)

                        return {
                            "isError": True,
                            "return": embed
                        }
                    else:
                        return commandBridge.runCommand(origCmd, args,
                                                        rawLanguage, sender)
            else:
                try:
                    if sender.id == central.config["BibleBot"]["owner"]:
                        return commandBridge.runOwnerCommand(command, args,
                                                             rawLanguage)
                except Exception:
                    embed.color = "#ff2e2e"
                    embed.set_footer("BibleBot v" + central.config["meta"]["version"],  # noqa: E501
                                                 "https://cdn.discordapp.com/avatars/" +  # noqa: E501
                                                 "361033318273384449/" +
                                                 "5aad77425546f9baa5e4b5112696e10a.png")  # noqa: E501

                    response = rawLanguage.commandNotFoundError.replace(
                        "<command>", command)

                    embed.add_field(rawLanguage.error, response)

                    return {
                        "isError": True,
                        "return": embed
                    }
        else:
            embed.color = "#ff2e2e"
            embed.set_footer("BibleBot v" + central.config["meta"]["version"],  # noqa: E501
                                                 "https://cdn.discordapp.com/avatars/" +  # noqa: E501
                                                 "361033318273384449/" +
                                                 "5aad77425546f9baa5e4b5112696e10a.png")  # noqa: E501

            response = rawLanguage.commandNotFoundError.replace(
                "<command>", command)

            embed.add_field(rawLanguage.error, response)

            return {
                "isError": True,
                "return": embed
            }
