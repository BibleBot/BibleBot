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
import numbers
import math

import discord
import tinydb
from handlers.commandlogic.settings import languages, versions, formatting

dir_path = os.path.dirname(os.path.realpath(__file__))
sys.path.append(dir_path + "/../..")

from vytypes.version import Version  # noqa: E402
from bible_modules import bibleutils  # noqa: E402
from bible_modules import biblegateway  # noqa: E402
from bible_modules import rev  # noqa: E402
import central  # noqa: E402


def runCommand(command, args, lang, user):
    embed = discord.Embed()

    if command == "biblebot":
        embed.title = lang["biblebot"].replace(
            "<biblebotversion>", central.config["meta"]["version"])
        embed.description = lang["code"]
        embed.color = 303102
        embed.set_footer(text="BibleBot v" + central.config["meta"]["version"],
                         icon_url="https://cdn.discordapp.com/avatars/" +
                         "361033318273384449/" +
                         "5aad77425546f9baa5e4b5112696e10a.png")

        response = lang["commandlist"]
        response = response.replace(
            "<biblebotversion>", central.config["meta"]["version"])
        response = response.replace(
            "<search>", lang["commands"]["search"])
        response = response.replace(
            "<setversion>", lang["commands"]["setversion"])
        response = response.replace(
            "<version>", lang["commands"]["version"])
        response = response.replace(
            "<versions>", lang["commands"]["versions"])
        response = response.replace(
            "<versioninfo>", lang["commands"]["versioninfo"])
        response = response.replace(
            "<votd>", lang["commands"]["votd"])
        response = response.replace(
            "<verseoftheday>", lang["commands"]["verseoftheday"])
        response = response.replace(
            "<random>", lang["commands"]["random"])
        response = response.replace(
            "<versenumbers>", lang["commands"]["versenumbers"])
        response = response.replace(
            "<headings>", lang["commands"]["headings"])
        response = response.replace(
            "<setlanguage>", lang["commands"]["setlanguage"])
        response = response.replace(
            "<language>", lang["commands"]["language"])
        response = response.replace(
            "<languages>", lang["commands"]["languages"])
        response = response.replace(
            "<enable>", lang["arguments"]["enable"])
        response = response.replace(
            "<disable>", lang["arguments"]["disable"])
        response = response.replace(
            "<users>", lang["commands"]["users"])
        response = response.replace(
            "<servers>", lang["commands"]["servers"])
        response = response.replace(
            "<invite>", lang["commands"]["invite"])
        response = response.replace(
            "<supporters>", lang["commands"]["supporters"])
        response = response.replace(
            "* ", "")

        embed.add_field(name=lang["commandlistName"],
                        value=response + "\n\n**" + lang["usage"] + "**")
        embed.add_field(name=u"\u200B", value=u"\u200B")
        embed.add_field(name=lang["links"], value=lang["patreon"] +
                        "\n" + lang["joinserver"] + "\n" + lang["copyright"])

        return {
            "level": "info",
            "message": embed
        }
    elif command == "search":
        availableVersions = versions.getVersionsByAcronym()
        version = versions.getVersion(user)

        query = ""

        if version is None or version is "HWP":
            version = lang.defaultVersion

        if isinstance(availableVersions.index(args[0]), numbers.Number):
            version = args[0]

            for i in args:
                if i != 0:
                    query += args[i] + " "
        else:
            for i in args:
                query += args[i] + " "

        if version != "REV":
            results = biblegateway.search(version, query)

            if results is not None:
                query.replace("\"", "")

                pages = []
                maxResultsPerPage = 6
                totalPages = math.ceil(len(results.keys()) / maxResultsPerPage)

                if totalPages == 0:
                    totalPages += 1

                for i in range(0, totalPages):
                    embed = discord.Embed()

                    embed.title(lang.searchResults + " \"" +
                                query[0:-1] + "\"")
                    embed.description(
                        lang.page + " " + (len(pages) + 1) +
                        " " + lang.of + " " + totalPages)
                    embed.color = 303102
                    embed.set_footer(text="BibleBot v" + central.config["meta"]["version"],
                                     icon_url="https://cdn.discordapp.com/avatars/" +
                                     "361033318273384449/" +
                                     "5aad77425546f9baa5e4b5112696e10a.png")

                    if len(results.keys()) > 0:
                        count = 0

                        for key in results.keys():
                            if (count < maxResultsPerPage):
                                embed.add_field(
                                    results[key]["title"],
                                    results[key]["text"], True)
                                del results[key]
                                count += 1
                    else:
                        embed.title(lang.nothingFound.replace(
                            "<query>", query[0:-1]))

                    pages.append(embed)

                if len(pages) > 1:
                    return {
                        "level": "info",
                        "paged": True,
                        "pages": pages
                    }
                else:
                    return {
                        "level": "info",
                        "message": pages[0]
                    }
        else:
            return {
                "level": "err",
                "message": lang.searchNotSupported.replace(
                    "<search>", lang["commands"].search)
            }
    elif command == "setversion":
        if versions.setVersion(user, args[0]):
            embed.color = 303102
            embed.set_footer(text="BibleBot v" + central.config["meta"]["version"],
                             icon_url="https://cdn.discordapp.com/avatars/" +
                             "361033318273384449/" +
                             "5aad77425546f9baa5e4b5112696e10a.png")

            embed.add_field("+" + lang["commands"].setversion,
                            lang.setversionsuccess)

            return {
                "level": "info",
                "message": embed
            }
        else:
            embed.color = "#ff2e2e"
            embed.add_field("+" + lang["commands"].setversion,
                            lang.setversionfail.replace(
                                "<versions>", lang["commands"].versions))

            return {
                "level": "err",
                "message": embed
            }
    elif command == "version":
        version = versions.getVersion(user)

        embed.color = 303102
        embed.set_footer(text="BibleBot v" + central.config["meta"]["version"],
                         icon_url="https://cdn.discordapp.com/avatars/" +
                         "361033318273384449/" +
                         "5aad77425546f9baa5e4b5112696e10a.png")

        if version is not None:
            if version == "HWP":
                version = lang.defaultVersion

            response = lang.versionused

            response = response.replace(
                "<version>", version)
            response = response.replace(
                "<setversion>", lang["commands"].setversion)

            embed.add_field("+" + lang["commands"].version, response)

            return {
                "level": "info",
                "message": embed
            }
        else:
            response = lang.noversionused

            response = response.replace(
                "<setversion>", lang["commands"].setversion)

            embed.color = "#ff2e2e"
            embed.add_field("+" + lang["commands"].version, response)

            return {
                "level": "err",
                "message": embed
            }
    elif command == "versions":
        availableVersions = versions.getVersions
        maxResultsPerPage = 25

        totalPages = math.ceil(len(availableVersions) / maxResultsPerPage)

        if totalPages == 0:
            totalPages += 1

        for i in range(0, totalPages):
            embed = discord.Embed()

            embed.color = 303102
            embed.set_footer(text="BibleBot v" + central.config["meta"]["version"],
                             icon_url="https://cdn.discordapp.com/avatars/" +
                             "361033318273384449/" +
                             "5aad77425546f9baa5e4b5112696e10a.png")

            if len(availableVersions) > 0:
                count = 0
                versionList = ""

                for index in availableVersions:
                    if count < maxResultsPerPage:
                        versionList += availableVersions[index] + "\n"
                        del availableVersions[index]
                        count += 1

                embed.add_field("+" + lang["commands"].versions + " - " +
                                lang.page + " " + (len(pages) + 1) + " " +
                                lang.of + " " + totalPages, versionList)

                pages.append(embed)

        return {
            "level": "info",
            "paged": True,
            "pages": pages
        }
    elif command == "versioninfo":
        idealVersion = tinydb.Query()
        results = central.versionDB.search(idealVersion.abbv == args[0])

        if len(results) > 0:
            embed.color = 303102
            embed.set_footer(text="BibleBot v" + central.config["meta"]["version"],
                             icon_url="https://cdn.discordapp.com/avatars/" +
                             "361033318273384449/" +
                             "5aad77425546f9baa5e4b5112696e10a.png")

            response = lang.versioninfo

            response = response.replace("<versionname>", results[0].name)

            if results[0].hasOT:
                response = response.replace(
                    "<hasOT>", lang.arguments.yes)
            else:
                response = response.replace(
                    "<hasOT>", lang.arguments.no)

            if results[0].hasNT:
                response = response.replace(
                    "<hasNT>", lang.arguments.yes)
            else:
                response = response.replace(
                    "<hasNT>", lang.arguments.no)

            if results[0].hasDEU:
                response = response.replace(
                    "<hasDEU>", lang.arguments.yes)
            else:
                response = response.replace(
                    "<hasDEU>", lang.arguments.no)

            embed.add_field("+" + lang["commands"].versioninfo, response)

            return {
                "level": "info",
                "message": embed
            }
        else:
            embed.color = "#ff2e2e"
            embed.add_field("+" + lang["commands"].versioninfo,
                            lang.versioninfofailed)

            return {
                "level": "err",
                "message": embed
            }
    elif command == "setlanguage":
        if languages.setLanguage(user, args[0]):
            embed.color = 303102
            embed.set_footer(text="BibleBot v" + central.config["meta"]["version"],
                             icon_url="https://cdn.discordapp.com/avatars/" +
                             "361033318273384449/" +
                             "5aad77425546f9baa5e4b5112696e10a.png")

            embed.add_field("+" + lang["commands"].setlanguage,
                            lang.setlanguagesuccess)

            return {
                "level": "info",
                "message": embed
            }
        else:
            embed.color = "#ff2e2e"
            embed.add_field("+" + lang["commands"].setlanguage,
                            lang.setlanguagefail.replace(
                                "<languages>", lang["commands"].languages))

            return {
                "level": "err",
                "message": embed
            }
    elif command == "language":
        embed.color = 303102
        embed.set_footer(text="BibleBot v" + central.config["meta"]["version"],
                         icon_url="https://cdn.discordapp.com/avatars/" +
                         "361033318273384449/" +
                         "5aad77425546f9baa5e4b5112696e10a.png")

        response = lang.languageused

        response = response.replace(
            "<setlanguage>", lang["commands"].setlanguage)

        return {
            "level": "info",
            "message": embed
        }
    elif "languages":
        availableLanguages = languages.getLanguages()

        embed.color = 303102
        embed.set_footer(text="BibleBot v" + central.config["meta"]["version"],
                         icon_url="https://cdn.discordapp.com/avatars/" +
                         "361033318273384449/" +
                         "5aad77425546f9baa5e4b5112696e10a.png")

        string = ""

        for index in availableLanguages:
            string += availableLanguages[i].name + \
                " [`" + availableLanguages[i].objectName + "`]\n"

        embed.add_field("+" + lang["commands"].languages, string)

        return {
            "level": "info",
            "message": embed
        }
    elif command == "votd" or command == "verseoftheday":
        version = versions.getVersion(user)
        headings = formatting.getHeadings(user)
        verseNumbers = formatting.getVerseNumbers(user)

        if version is None or version is "HWP":
            version = lang.defaultVersion

        if version != "REV":
            verse = bibleutils.getVOTD()
            result = biblegateway.getResult(
                verse, version, headings, verseNumbers)

            content = "```Dust\n" + result.title + \
                "\n\n" + result.text + "```"

            responseString = "**" + result.passage + \
                " - " + result.version + "**\n\n" + content

            if len(responseString) < 2000:
                return {
                    "level": "info",
                    "twoMessages": False,
                    "reference": verse,
                    "message": responseString
                }
            elif len(responseString) > 2000:
                if len(responseString) < 3500:
                    splitText = central.splitter(
                        result.text)

                    content1 = "```Dust\n" + \
                        result.title + "\n\n" + \
                        splitText["first"] + "```"
                    responseString1 = "**" + \
                        result.passage + " - " + \
                        result.version + "**\n\n" + \
                        content1
                    content2 = "```Dust\n " + \
                        splitText["second"] + "```"

                    return {
                        "level": "info",
                        "twoMessages": True,
                        "reference": verse,
                        "firstMessage": responseString1,
                        "secondMessage": content2
                    }
                else:
                    return {
                        "level": "err",
                        "twoMessages": False,
                        "reference": verse,
                        "message": lang.passagetoolong
                    }
        else:
            verse = bibleutils.getVOTD()
            result = rev.getResult(
                verse, version, headings, verseNumbers)

            content = "```Dust\n" + result.title + \
                "\n\n" + result.text + "```"

            responseString = "**" + result.passage + \
                " - " + result.version + "**\n\n" + content

            if len(responseString) < 2000:
                return {
                    "level": "info",
                    "twoMessages": False,
                    "reference": verse,
                    "message": responseString
                }
            elif len(responseString) > 2000:
                if len(responseString) < 3500:
                    splitText = central.splitter(
                        result.text)

                    content1 = "```Dust\n" + \
                        result.title + "\n\n" + \
                        splitText["first"] + "```"
                    responseString1 = "**" + \
                        result.passage + " - " + \
                        result.version + "**\n\n" + \
                        content1
                    content2 = "```Dust\n " + \
                        splitText["second"] + "```"

                    return {
                        "level": "info",
                        "twoMessages": True,
                        "reference": verse,
                        "firstMessage": responseString1,
                        "secondMessage": content2
                    }
                else:
                    return {
                        "level": "err",
                        "twoMessages": False,
                        "reference": verse,
                        "message": lang.passagetoolong
                    }
    elif command == "random":
        version = versions.getVersion(user)
        headings = formatting.getHeadings(user)
        verseNumbers = formatting.getVerseNumbers(user)

        if version is None or version is "HWP":
            version = lang.defaultVersion

        if version != "REV":
            verse = bibleutils.getRandomVerse()
            result = biblegateway.getResult(
                verse, version, headings, verseNumbers)

            content = "```Dust\n" + result.title + \
                "\n\n" + result.text + "```"

            responseString = "**" + result.passage + \
                " - " + result.version + "**\n\n" + content

            if len(responseString) < 2000:
                return {
                    "level": "info",
                    "twoMessages": False,
                    "reference": verse,
                    "message": responseString
                }
            elif len(responseString) > 2000:
                if len(responseString) < 3500:
                    splitText = central.splitter(
                        result.text)

                    content1 = "```Dust\n" + \
                        result.title + "\n\n" + \
                        splitText["first"] + "```"
                    responseString1 = "**" + \
                        result.passage + " - " + \
                        result.version + "**\n\n" + \
                        content1
                    content2 = "```Dust\n " + \
                        splitText["second"] + "```"

                    return {
                        "level": "info",
                        "twoMessages": True,
                        "reference": verse,
                        "firstMessage": responseString1,
                        "secondMessage": content2
                    }
                else:
                    return {
                        "level": "err",
                        "twoMessages": False,
                        "reference": verse,
                        "message": lang.passagetoolong
                    }
        else:
            verse = bibleutils.getRandomVerse()
            result = rev.getResult(
                verse, version, headings, verseNumbers)

            content = "```Dust\n" + result.title + \
                "\n\n" + result.text + "```"

            responseString = "**" + result.passage + \
                " - " + result.version + "**\n\n" + content

            if len(responseString) < 2000:
                return {
                    "level": "info",
                    "twoMessages": False,
                    "reference": verse,
                    "message": responseString
                }
            elif len(responseString) > 2000:
                if len(responseString) < 3500:
                    splitText = central.splitter(
                        result.text)

                    content1 = "```Dust\n" + \
                        result.title + "\n\n" + \
                        splitText["first"] + "```"
                    responseString1 = "**" + \
                        result.passage + " - " + \
                        result.version + "**\n\n" + \
                        content1
                    content2 = "```Dust\n " + \
                        splitText["second"] + "```"

                    return {
                        "level": "info",
                        "twoMessages": True,
                        "reference": verse,
                        "firstMessage": responseString1,
                        "secondMessage": content2
                    }
                else:
                    return {
                        "level": "err",
                        "twoMessages": False,
                        "reference": verse,
                        "message": lang.passagetoolong
                    }
    elif command == "headings":
        if len(args) == 1:
            embed.color = 303102
            embed.set_footer(text="BibleBot v" + central.config["meta"]["version"],
                             icon_url="https://cdn.discordapp.com/avatars/" +
                             "361033318273384449/" +
                             "5aad77425546f9baa5e4b5112696e10a.png")

            if formatting.setHeadings(user, args[0]):
                embed.add_field("+" + lang["commands"].headings,
                                lang.headingssuccess)

                return {
                    "level": "info",
                    "message": embed
                }
            else:
                embed.color = "#ff2e2e"

                response = lang.headingsfail.replace(
                    "<headings>", lang["commands"].headings).replace(
                        "<enable>", lang.arguments.enable).replace(
                            "<disable>", lang.arguments.disable)

                embed.add_field("+" + lang["commands"].headings, response)

                return {
                    "level": "err",
                    "message": embed
                }
        else:
            headings = formatting.getHeadings(user)

            embed.color = 303102
            embed.set_footer(text="BibleBot v" + central.config["meta"]["version"],
                             icon_url="https://cdn.discordapp.com/avatars/" +
                             "361033318273384449/" +
                             "5aad77425546f9baa5e4b5112696e10a.png")

            if headings == "enable":
                response = lang.headings.replace(
                    "<enabled/disabled>", lang.enabled)
                embed.add_field("+" + lang["commands"].headings, response)

                return {
                    "level": "info",
                    "message": embed
                }
            else:
                response = lang.headings.replace(
                    "<enabled/disabled>", lang.disabled)
                embed.add_field("+" + lang["commands"].headings, response)

                return {
                    "level": "info",
                    "message": embed
                }
    elif command == "versenumbers":
        if len(args) == 1:
            embed.color = 303102
            embed.set_footer(text="BibleBot v" + central.config["meta"]["version"],
                             icon_url="https://cdn.discordapp.com/avatars/" +
                             "361033318273384449/" +
                             "5aad77425546f9baa5e4b5112696e10a.png")

            if formatting.setVerseNumbers(user, args[0]):
                embed.add_field("+" + lang["commands"].versenumbers,
                                lang.versenumberssuccess)

                return {
                    "level": "info",
                    "message": embed
                }
            else:
                embed.color = "#ff2e2e"

                response = lang.versenumbersfail.replace(
                    "<headings>", lang["commands"].versenumbers).replace(
                        "<enable>", lang.arguments.enable).replace(
                            "<disable>", lang.arguments.disable)

                embed.add_field("+" + lang["commands"].versenumbers, response)

                return {
                    "level": "err",
                    "message": embed
                }
        else:
            verseNumbers = formatting.getVerseNumbers(user)

            embed.color = 303102
            embed.set_footer(text="BibleBot v" + central.config["meta"]["version"],
                             icon_url="https://cdn.discordapp.com/avatars/" +
                             "361033318273384449/" +
                             "5aad77425546f9baa5e4b5112696e10a.png")

            if verseNumbers == "enable":
                response = lang.versenumbers.replace(
                    "<enabled/disabled>", lang.enabled)
                embed.add_field("+" + lang["commands"].versenumbers, response)

                return {
                    "level": "info",
                    "message": embed
                }
            else:
                response = lang.versenumbers.replace(
                    "<enabled/disabled>", lang.disabled)
                embed.add_field("+" + lang["commands"].versenumbers, response)

                return {
                    "level": "info",
                    "message": embed
                }
    elif command == "users":
        embed.color = 303102
        embed.set_footer(text="BibleBot v" + central.config["meta"]["version"],
                         icon_url="https://cdn.discordapp.com/avatars/" +
                         "361033318273384449/" +
                         "5aad77425546f9baa5e4b5112696e10a.png")

        processed = len(args[0].users)

        embed.add_field("+" + lang["commands"].users,
                        lang.users + ": " + str(processed))

        return {
            "level": "info",
            "message": embed
        }
    elif command == "servers":
        embed.color = 303102
        embed.set_footer(text="BibleBot v" + central.config["meta"]["version"],
                         icon_url="https://cdn.discordapp.com/avatars/" +
                         "361033318273384449/" +
                         "5aad77425546f9baa5e4b5112696e10a.png")

        processed = len(args[0].guilds)

        embed.add_field("+" + lang["commands"].servers,
                        lang.servers.replace("<count>", str(processed)))

        return {
            "level": "info",
            "message": embed
        }
    elif command == "jepekula":
        version = versions.getVersion(user)
        headings = formatting.getHeadings(user)
        verseNumbers = formatting.getVerseNumbers(user)

        if version is None or version is "HWP":
            version = lang.defaultVersion

        verse = "Mark 9:23-24"

        if version != "REV":
            result = biblegateway.getResult(
                verse, version, headings, verseNumbers)

            content = "```Dust\n" + result.title + \
                "\n\n" + result.text + "```"

            responseString = "**" + result.passage + \
                " - " + result.version + "**\n\n" + content

            if len(responseString) < 2000:
                return {
                    "level": "info",
                    "twoMessages": False,
                    "reference": verse,
                    "message": responseString
                }
        else:
            result = rev.getResult(
                verse, version, headings, verseNumbers)

            content = "```Dust\n" + result.title + \
                "\n\n" + result.text + "```"

            responseString = "**" + result.passage + \
                " - " + result.version + "**\n\n" + content

            if len(responseString) < 2000:
                return {
                    "level": "info",
                    "twoMessages": False,
                    "reference": verse,
                    "message": responseString
                }
    elif command == "joseph":
        return {
            "level": "info",
            "message": "Jesus never consecrated peanut butter " +
            "and jelly sandwiches and Coca-Cola!"
        }
    elif command == "supporters":
        embed.color = 303102
        embed.set_footer(text="BibleBot v" + central.config["meta"]["version"],
                         icon_url="https://cdn.discordapp.com/avatars/" +
                         "361033318273384449/" +
                         "5aad77425546f9baa5e4b5112696e10a.png")

        embed.add_field("+" + lang["commands"].supporters, "**" +
                        lang.supporters + "**\n\nCHAZER2222\nJepekula" +
                        "\nJoseph\nSoku\n" + lang.anonymousDonors +
                        "\n" + lang.donorsNotListed)

        return {
            "level": "info",
            "message": embed
        }
    elif command == "invite":
        return {
            "level": "info",
            "message": "<https://discordapp.com/oauth2/authorize?" +
            "client_id=361033318273384449&scope=bot&permissions=0>"
        }


def runOwnerCommand(command, args, lang):
    embed = discord.Embed()

    if command == "puppet":
        message = ""

        for index in args:
            message += args[index] + " "

        return {
            "level": "info",
            "message": message[0:-1]
        }
    elif command == "eval":
        message = ""

        for index in args:
            message += args[index] + " "

        try:
            return {
                "level": "info",
                "message": eval(message)
            }
        except Exception as e:
            return {
                "level": "err",
                "message": "[err] " + e
            }
    elif command == "announce":
        embed.color = 303102
        embed.set_footer(text="BibleBot v" + central.config["meta"]["version"],
                         icon_url="https://cdn.discordapp.com/avatars/" +
                         "361033318273384449/" +
                         "5aad77425546f9baa5e4b5112696e10a.png")

        message = ""

        for index in args:
            message += args[index] + " "

        embed.add_field("Announcement", message[0:-1])

        return {
            "level": "info",
            "announcement": True,
            "message": embed
        }
    elif command == "addversion":
        embed.color = 303102
        embed.set_footer(text="BibleBot v" + central.config["meta"]["version"],
                         icon_url="https://cdn.discordapp.com/avatars/" +
                         "361033318273384449/" +
                         "5aad77425546f9baa5e4b5112696e10a.png")

        argc = len(args)
        name = ""

        for i in range(0, (argc - 4)):
            name += args[i] + " "

        name = name[0:-1]
        abbv = args[argc - 4]

        hasOT = False
        hasNT = False
        hasDEU = False

        if args[argc - 3] == "yes":
            hasOT = True

        if args[argc - 2] == "yes":
            hasNT = True

        if args[argc - 1] == "yes":
            hasDEU = True

        newVersion = Version(name, abbv, hasOT, hasNT, hasDEU)
        central.versionDB.insert(newVersion.toObject())

        embed.add_field(
            "+" + lang["commands"].addversion, lang.addversionsuccess)

        return {
            "level": "info",
            "message": embed
        }
