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

import math
import os
import sys

import discord
import tinydb

from handlers.commandlogic.settings import languages, versions, formatting, misc

dir_path = os.path.dirname(os.path.realpath(__file__))
sys.path.append(dir_path + "/../..")

from vytypes.version import Version  # noqa: E402
from bible_modules import bibleutils  # noqa: E402
from bible_modules import biblegateway  # noqa: E402
from bible_modules import rev  # noqa: E402
import central  # noqa: E402


def run_command(command, args, lang, user, guild, channel):
    embed = discord.Embed()

    if command == "biblebot":
        embed.title = lang["biblebot"].replace("<biblebotversion>", central.version.split("v")[1])
        embed.description = lang["code"].replace("repositoryLink", "https://github.com/BibleBot/BibleBot")

        embed.color = 303102
        embed.set_footer(text=central.version, icon_url=central.icon)

        response = lang["commandlist"]
        response2 = lang["commandlist2"]
        response3 = lang["guildcommandlist"]

        response = response.replace("<biblebotversion>", central.version)
        response = response.replace("<search>", lang["commands"]["search"])
        response = response.replace("<setversion>", lang["commands"]["setversion"])
        response = response.replace("<version>", lang["commands"]["version"])
        response = response.replace("<versions>", lang["commands"]["versions"])
        response = response.replace("<versioninfo>", lang["commands"]["versioninfo"])
        response = response.replace("<votd>", lang["commands"]["votd"])
        response = response.replace("<verseoftheday>", lang["commands"]["verseoftheday"])
        response = response.replace("<random>", lang["commands"]["random"])
        response = response.replace("<setversenumbers>", lang["commands"]["setversenumbers"])
        response = response.replace("<versenumbers>", lang["commands"]["versenumbers"])
        response = response.replace("<setheadings>", lang["commands"]["setheadings"])
        response = response.replace("<headings>", lang["commands"]["headings"])
        response = response.replace("<setlanguage>", lang["commands"]["setlanguage"])
        response = response.replace("<language>", lang["commands"]["language"])
        response = response.replace("<languages>", lang["commands"]["languages"])
        response = response.replace("<enable>", lang["arguments"]["enable"])
        response = response.replace("<disable>", lang["arguments"]["disable"])
        response = response.replace("<users>", lang["commands"]["users"])
        response = response.replace("<servers>", lang["commands"]["servers"])
        response = response.replace("<invite>", lang["commands"]["invite"])
        response = response.replace("<supporters>", lang["commands"]["supporters"])
        response = response.replace("* ", "")
        response = response.replace("+", central.config["BibleBot"]["commandPrefix"])

        response2 = response2.replace("<creeds>", lang["commands"]["creeds"])
        response2 = response2.replace("* ", "")
        response2 = response2.replace("+", central.config["BibleBot"]["commandPrefix"])

        response3 = response3.replace("<setguildversion>", lang["commands"]["setguildversion"])
        response3 = response3.replace("<guildversion>", lang["commands"]["guildversion"])
        response3 = response3.replace("<setguildlanguage>", lang["commands"]["setguildlanguage"])
        response3 = response3.replace("<guildlanguage>", lang["commands"]["guildlanguage"])
        response3 = response3.replace("<setvotdtime>", lang["commands"]["setvotdtime"])
        response3 = response3.replace("<clearvotdtime>", lang["commands"]["clearvotdtime"])
        response3 = response3.replace("<votdtime>", lang["commands"]["votdtime"])
        response3 = response3.replace("<setannouncements>", lang["commands"]["setannouncements"])
        response3 = response3.replace("<announcements>", lang["commands"]["announcements"])
        response3 = response3.replace("<enable>", lang["arguments"]["enable"])
        response3 = response3.replace("<disable>", lang["arguments"]["disable"])
        response3 = response3.replace("* ", "")
        response3 = response3.replace("+", central.config["BibleBot"]["commandPrefix"])

        embed.add_field(name=lang["commandlistName"], value=response, inline=False)
        embed.add_field(name=u"\u200B", value=u"\u200B", inline=False)
        embed.add_field(name=lang["extrabiblicalcommandlistName"], value=response2, inline=False)
        embed.add_field(name=u"\u200B", value=u"\u200B", inline=False)
        embed.add_field(name=lang["guildcommandlistName"], value=response3, inline=False)
        embed.add_field(name=u"\u200B", value=u"\u200B", inline=False)

        links = lang["website"].replace("websiteLink", "https://biblebot.xyz") + "\n" + lang["joinserver"].replace(
            "inviteLink", "https://discord.gg/seKEJUn") + "\n" + \
                lang["terms"].replace("termsLink", "https://biblebot.xyz/terms") + "\n\n**" + lang["usage"] + "**"

        embed.add_field(name=lang["links"], value=links)

        return {
            "level": "info",
            "message": embed
        }
    elif command == "search":
        available_versions = versions.get_versions_by_acronym()
        version = versions.get_version(user)

        if version is None:
            version = versions.get_guild_version(guild)

            if version is None:
                version = "NRSV"

        query = ""

        if args[0] in available_versions:
            version = args[0]

            for i, arg in enumerate(args):
                if i != 0:
                    query += arg + " "
        else:
            for arg in args:
                query += arg + " "

        if version != "REV":
            results = biblegateway.search(version, query[0:-1])

            if results is not None:
                query.replace("\"", "")

                pages = []
                max_results_per_page = 6
                total_pages = int(math.ceil(len(results.keys()) / max_results_per_page))

                if total_pages == 0:
                    total_pages += 1
                elif total_pages > 100:
                    total_pages = 100

                for i in range(total_pages):
                    embed = discord.Embed()

                    embed.title = lang["searchResults"] + " \"" + query[0:-1] + "\""

                    page_counter = lang["pageOf"].replace("<num>", str(i + 1)).replace("<total>", str(total_pages))
                    embed.description = page_counter

                    embed.color = 303102
                    embed.set_footer(text=central.version, icon_url=central.icon)

                    if len(results.keys()) > 0:
                        count = 0

                        for key in list(results.keys()):
                            if len(results[key]["text"]) < 700:
                                if count < max_results_per_page:
                                    title = results[key]["title"]
                                    text = results[key]["text"]

                                    embed.add_field(name=title, value=text, inline=False)

                                    del results[key]
                                    count += 1
                    else:
                        embed.title = lang["nothingFound"].replace("<query>", query[0:-1])
                        embed.description = ""

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
                "message": lang["searchNotSupported"].replace("<search>", lang["commands"]["search"])
            }
    elif command == "setversion":
        if versions.set_version(user, args[0]):
            embed.color = 303102
            embed.set_footer(text=central.version, icon_url=central.icon)

            embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["setversion"],
                            value=lang["setversionsuccess"])

            return {
                "level": "info",
                "message": embed
            }
        else:
            embed.color = 16723502
            embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["setversion"],
                            value=lang["setversionfail"].replace("<versions>", lang["commands"]["versions"]))

            return {
                "level": "err",
                "message": embed
            }
    elif command == "setguildversion":
        perms = user.guild_permissions

        if str(user.id) != central.config["BibleBot"]["owner"]:
            if not perms.manage_guild:
                embed.color = 16723502
                embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["setguildversion"],
                                value=lang["setguildversionnoperm"])

                return {
                    "level": "err",
                    "message": embed
                }

        if versions.set_guild_version(guild, args[0]):
            embed.color = 303102
            embed.set_footer(text=central.version, icon_url=central.icon)

            embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["setguildversion"],
                            value=lang["setguildversionsuccess"])

            return {
                "level": "info",
                "message": embed
            }
        else:
            embed.color = 16723502
            embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["setguildversion"],
                            value=lang["setguildversionfail"].replace("<versions>", lang["commands"]["versions"]))

            return {
                "level": "err",
                "message": embed
            }
    elif command == "version":
        version = versions.get_version(user)

        embed.color = 303102
        embed.set_footer(text=central.version, icon_url=central.icon)

        if version is not None:
            response = lang["versionused"]

            response = response.replace("<version>", version)
            response = response.replace("<setversion>", lang["commands"]["setversion"])

            embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["version"],
                            value=response)

            return {
                "level": "info",
                "message": embed
            }
        else:
            response = lang["noversionused"]

            response = response.replace("<setversion>", lang["commands"]["setversion"])

            embed.color = 16723502
            embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["version"],
                            value=response)

            return {
                "level": "err",
                "message": embed
            }
    elif command == "guildversion":
        version = versions.get_guild_version(guild)

        embed.color = 303102
        embed.set_footer(text=central.version, icon_url=central.icon)

        if version is not None:
            response = lang["guildversionused"]

            response = response.replace("<version>", version)
            response = response.replace("<setguildversion>", lang["commands"]["setguildversion"])

            embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["guildversion"],
                            value=response)

            return {
                "level": "info",
                "message": embed
            }
        else:
            response = lang["noguildversionused"]

            response = response.replace("<setguildversion>", lang["commands"]["setguildversion"])

            embed.color = 16723502
            embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["guildversion"],
                            value=response)

            return {
                "level": "err",
                "message": embed
            }
    elif command == "versions":
        pages = []
        available_versions = versions.get_versions()
        max_results_per_page = 25

        total_pages = int(math.ceil(len(available_versions) / max_results_per_page))

        if total_pages == 0:
            total_pages += 1

        for i in range(total_pages):
            embed = discord.Embed()

            embed.color = 303102
            embed.set_footer(text=central.version, icon_url=central.icon)

            if len(available_versions) > 0:
                count = 0
                version_list = ""

                available_versions_copy = available_versions[:]
                for item in available_versions_copy:
                    if count < max_results_per_page:
                        version_list += item + "\n"
                        count += 1

                        available_versions.remove(item)
                    else:
                        break

                page_counter = lang["pageOf"].replace("<num>", str(i + 1)).replace("<total>", str(total_pages))
                embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["versions"] +
                                     " - " + page_counter, value=version_list)

                pages.append(embed)

        return {
            "level": "info",
            "paged": True,
            "pages": pages
        }
    elif command == "versioninfo":
        ideal_version = tinydb.Query()
        results = central.versionDB.search(ideal_version["abbv"] == args[0])

        if len(results) > 0:
            embed.color = 303102
            embed.set_footer(text=central.version, icon_url=central.icon)

            response = lang["versioninfo"]

            response = response.replace("<versionname>", results[0]["name"])

            if results[0]["hasOT"]:
                response = response.replace("<hasOT>", lang["arguments"]["yes"])
            else:
                response = response.replace("<hasOT>", lang["arguments"]["no"])

            if results[0]["hasNT"]:
                response = response.replace("<hasNT>", lang["arguments"]["yes"])
            else:
                response = response.replace("<hasNT>", lang["arguments"]["no"])

            if results[0]["hasDEU"]:
                response = response.replace("<hasDEU>", lang["arguments"]["yes"])
            else:
                response = response.replace("<hasDEU>", lang["arguments"]["no"])

            embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["versioninfo"],
                            value=response)

            return {
                "level": "info",
                "message": embed
            }
        else:
            embed.color = 16723502
            embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["versioninfo"],
                            value=lang["versioninfofailed"])

            return {
                "level": "err",
                "message": embed
            }
    elif command == "setlanguage":
        if languages.set_language(user, args[0]):
            embed.color = 303102
            embed.set_footer(text=central.version, icon_url=central.icon)

            embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["setlanguage"],
                            value=lang["setlanguagesuccess"])

            return {
                "level": "info",
                "message": embed
            }
        else:
            embed.color = 16723502
            embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["setlanguage"],
                            value=lang["setlanguagefail"].replace("<languages>", lang["commands"]["languages"]))

            return {
                "level": "err",
                "message": embed
            }
    elif command == "setguildlanguage":
        perms = user.guild_permissions

        if str(user.id) != central.config["BibleBot"]["owner"]:
            if not perms.manage_guild:
                embed.color = 16723502
                embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["setguildlanguage"],
                                value=lang["setguildlanguagenoperm"])

                return {
                    "level": "err",
                    "message": embed
                }

        if languages.set_guild_language(guild, args[0]):
            embed.color = 303102
            embed.set_footer(text=central.version, icon_url=central.icon)

            embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["setguildlanguage"],
                            value=lang["setguildlanguagesuccess"])

            return {
                "level": "info",
                "message": embed
            }
        else:
            embed.color = 16723502
            embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["setguildlanguage"],
                            value=lang["setguildlanguagefail"].replace("<languages>", lang["commands"]["languages"]))

            return {
                "level": "err",
                "message": embed
            }
    elif command == "language":
        embed.color = 303102
        embed.set_footer(text=central.version, icon_url=central.icon)

        response = lang["languageused"]

        response = response.replace("<setlanguage>", lang["commands"]["setlanguage"])

        embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["language"],
                        value=response)

        return {
            "level": "info",
            "message": embed
        }
    elif command == "guildlanguage":
        glang = languages.get_guild_language(guild)
        glang = getattr(central.languages, glang).raw_object

        embed.color = 303102
        embed.set_footer(text=central.version, icon_url=central.icon)

        response = glang["guildlanguageused"]

        response = response.replace("<setguildlanguage>", glang["commands"]["setguildlanguage"])

        embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + glang["commands"]["guildlanguage"],
                        value=response)

        return {
            "level": "info",
            "message": embed
        }
    elif command == "languages":
        available_languages = languages.get_languages()

        embed.color = 303102
        embed.set_footer(text=central.version, icon_url=central.icon)

        string = ""

        for item in available_languages:
            string += item["name"] + " [`" + item["object_name"] + "`]\n"

        embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["languages"],
                        value=string)

        return {
            "level": "info",
            "message": embed
        }
    elif command == "setguildbrackets":
        perms = user.guild_permissions

        if str(user.id) != central.config["BibleBot"]["owner"]:
            if not perms.manage_guild:
                embed.color = 16723502
                embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["setguildbrackets"],
                                value=lang["setguildbracketsnoperm"])

                return {
                    "level": "err",
                    "message": embed
                }

        if formatting.set_guild_brackets(guild, args[0]):
            embed.color = 303102
            embed.set_footer(text=central.version, icon_url=central.icon)

            embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["setguildbrackets"],
                            value=lang["setguildbracketssuccess"])

            return {
                "level": "info",
                "message": embed
            }
        else:
            embed.color = 16723502
            embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["setguildbrackets"],
                            value=lang["setguildbracketsfail"])

            return {
                "level": "err",
                "message": embed
            }
    elif command == "guildbrackets":
        brackets_dict = formatting.get_guild_brackets(guild)
        brackets = brackets_dict["first"] + brackets_dict["second"]

        embed.color = 303102
        embed.set_footer(text=central.version, icon_url=central.icon)

        response = lang["guildbracketsused"]

        response = response.replace("<brackets>", brackets)
        response = response.replace("<setguildbrackets>", lang["commands"]["setguildbrackets"])

        embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["guildbrackets"],
                        value=response)

        return {
            "level": "info",
            "message": embed
        }
    elif command == "setvotdtime":
        perms = user.guild_permissions

        if str(user.id) != central.config["BibleBot"]["owner"]:
            if not perms.manage_guild:
                embed.color = 16723502
                embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["setvotdtime"],
                                value=lang["setvotdtimenoperm"])

                return {
                    "level": "err",
                    "message": embed
                }

        if misc.set_guild_votd_time(guild, channel, args[0]):
            embed.color = 303102
            embed.set_footer(text=central.version, icon_url=central.icon)

            embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["setvotdtime"],
                            value=lang["setvotdtimesuccess"])

            return {
                "level": "info",
                "message": embed
            }
        else:
            embed.color = 16723502
            embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["setvotdtime"],
                            value=lang["setvotdtimefail"])

            return {
                "level": "err",
                "message": embed
            }
    elif command == "clearvotdtime":
        perms = user.guild_permissions

        if str(user.id) != central.config["BibleBot"]["owner"]:
            if not perms.manage_guild:
                embed.color = 16723502
                embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["clearvotdtime"],
                                value=lang["clearvotdtimenoperm"])

                return {
                    "level": "err",
                    "message": embed
                }

        if misc.set_guild_votd_time(guild, channel, "clear"):
            embed.color = 303102
            embed.set_footer(text=central.version, icon_url=central.icon)

            embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["clearvotdtime"],
                            value=lang["clearvotdtimesuccess"])

            return {
                "level": "info",
                "message": embed
            }
    elif command == "votdtime":
        time_tuple = misc.get_guild_votd_time(guild)

        if time_tuple is not None:
            channel, time = time_tuple

            embed.color = 303102
            embed.set_footer(text=central.version, icon_url=central.icon)

            response = lang["votdtimeused"]

            response = response.replace("<time>", time + " UTC")
            response = response.replace("<channel>", channel)
            response = response.replace("<setvotdtime>", lang["commands"]["setvotdtime"])
            response = response.replace("<clearvotdtime>", lang["commands"]["clearvotdtime"])

            embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["votdtime"],
                            value=response)

            return {
                "level": "info",
                "message": embed
            }
        else:
            response = lang["novotdtimeused"]

            response = response.replace("<setvotdtime>", lang["commands"]["setvotdtime"])

            embed.color = 16723502
            embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["votdtime"],
                            value=response)

            return {
                "level": "err",
                "message": embed
            }
    elif command == "votd" or command == "verseoftheday":
        version = versions.get_version(user)
        headings = formatting.get_headings(user)
        verse_numbers = formatting.get_verse_numbers(user)

        if version is None:
            version = versions.get_guild_version(guild)

            if version is None:
                version = "NRSV"

        if version != "REV":
            verse = bibleutils.get_votd()
            result = biblegateway.get_result(verse, version, headings, verse_numbers)

            content = "```Dust\n" + result["title"] + "\n\n" + result["text"] + "```"
            response_string = "**" + result["passage"] + " - " + result["version"] + "**\n\n" + content

            if len(response_string) < 2000:
                return {
                    "level": "info",
                    "reference": verse,
                    "message": response_string
                }
            elif len(response_string) > 2000:
                if len(response_string) < 3500:
                    split_text = central.splitter(result["text"])

                    content1 = "```Dust\n" + result["title"] + "\n\n" + split_text["first"] + "```"
                    response_string1 = "**" + result["passage"] + " - " + result["version"] + "**\n\n" + content1

                    content2 = "```Dust\n " + split_text["second"] + "```"

                    return {
                        "level": "info",
                        "twoMessages": True,
                        "reference": verse,
                        "firstMessage": response_string1,
                        "secondMessage": content2
                    }
                else:
                    return {
                        "level": "err",
                        "reference": verse,
                        "message": lang["passagetoolong"]
                    }
        else:
            verse = bibleutils.get_votd()
            result = rev.get_result(verse, verse_numbers)

            content = "```Dust\n" + result["title"] + "\n\n" + result["text"] + "```"
            response_string = "**" + result["passage"] + " - " + result["version"] + "**\n\n" + content

            if len(response_string) < 2000:
                return {
                    "level": "info",
                    "reference": verse,
                    "message": response_string
                }
            elif len(response_string) > 2000:
                if len(response_string) < 3500:
                    split_text = central.splitter(
                        result["text"])

                    content1 = "```Dust\n" + result["title"] + "\n\n" + split_text["first"] + "```"
                    response_string1 = "**" + result["passage"] + " - " + result["version"] + "**\n\n" + content1

                    content2 = "```Dust\n " + split_text["second"] + "```"

                    return {
                        "level": "info",
                        "twoMessages": True,
                        "reference": verse,
                        "firstMessage": response_string1,
                        "secondMessage": content2
                    }
                else:
                    return {
                        "level": "err",
                        "reference": verse,
                        "message": lang["passagetoolong"]
                    }
    elif command == "random":
        version = versions.get_version(user)
        headings = formatting.get_headings(user)
        verse_numbers = formatting.get_verse_numbers(user)

        if version is None:
            version = versions.get_guild_version(guild)

            if version is None:
                version = "NRSV"

        if version != "REV":
            verse = bibleutils.get_random_verse()
            result = biblegateway.get_result(verse, version, headings, verse_numbers)
            counter = 10

            while result is None and counter != 10:
                verse = bibleutils.get_random_verse()
                result = biblegateway.get_result(verse, version, headings, verse_numbers)
                counter += 1

            if counter == 10 and result is None:
                return

            content = "```Dust\n" + result["title"] + "\n\n" + result["text"] + "```"
            response_string = "**" + result["passage"] + " - " + result["version"] + "**\n\n" + content

            if len(response_string) < 2000:
                return {
                    "level": "info",
                    "reference": verse,
                    "message": response_string
                }
            elif len(response_string) > 2000:
                if len(response_string) < 3500:
                    split_text = central.splitter(result["text"])

                    content1 = "```Dust\n" + result["title"] + "\n\n" + split_text["first"] + "```"
                    response_string1 = "**" + result["passage"] + " - " + result["version"] + "**\n\n" + content1

                    content2 = "```Dust\n " + split_text["second"] + "```"

                    return {
                        "level": "info",
                        "twoMessages": True,
                        "reference": verse,
                        "firstMessage": response_string1,
                        "secondMessage": content2
                    }
                else:
                    return {
                        "level": "err",
                        "reference": verse,
                        "message": lang["passagetoolong"]
                    }
        else:
            verse = bibleutils.get_random_verse()
            result = rev.get_result(verse, verse_numbers)

            content = "```Dust\n" + result["title"] + "\n\n" + result["text"] + "```"

            response_string = "**" + result["passage"] + " - " + result["version"] + "**\n\n" + content

            if len(response_string) < 2000:
                return {
                    "level": "info",
                    "reference": verse,
                    "message": response_string
                }
            elif len(response_string) > 2000:
                if len(response_string) < 3500:
                    split_text = central.splitter(result["text"])

                    content1 = "```Dust\n" + result["title"] + "\n\n" + split_text["first"] + "```"
                    response_string1 = "**" + result["passage"] + " - " + result["version"] + "**\n\n" + content1

                    content2 = "```Dust\n " + split_text["second"] + "```"

                    return {
                        "level": "info",
                        "twoMessages": True,
                        "reference": verse,
                        "firstMessage": response_string1,
                        "secondMessage": content2
                    }
                else:
                    return {
                        "level": "err",
                        "reference": verse,
                        "message": lang["passagetoolong"]
                    }
    elif command == "setheadings":
        embed.color = 303102
        embed.set_footer(text=central.version, icon_url=central.icon)

        if formatting.set_headings(user, args[0]):
            embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["setheadings"],
                            value=lang["headingssuccess"])

            return {
                "level": "info",
                "message": embed
            }
        else:
            embed.color = 16723502

            response = lang["headingsfail"]

            response = response.replace("<setheadings>", lang["commands"]["setheadings"])
            response = response.replace("<enable>", lang["arguments"]["enable"])
            response = response.replace("<disable>", lang["arguments"]["disable"])

            embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["setheadings"],
                            value=response)

            return {
                "level": "err",
                "message": embed
            }
    elif command == "headings":
        headings = formatting.get_headings(user)

        embed.color = 303102
        embed.set_footer(text=central.version, icon_url=central.icon)

        if headings == "enable":
            response = lang["headings"].replace("<enabled/disabled>", lang["enabled"])
            response = response.replace("<setheadings>", lang["commands"]["setheadings"])

            embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["headings"],
                            value=response)

            return {
                "level": "info",
                "message": embed
            }
        else:
            response = lang["headings"].replace("<enabled/disabled>", lang["disabled"])
            response = response.replace("<setheadings>", lang["commands"]["setheadings"])

            embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["headings"],
                            value=response)

            return {
                "level": "info",
                "message": embed
            }
    elif command == "setversenumbers":
        embed.color = 303102
        embed.set_footer(text=central.version, icon_url=central.icon)

        if formatting.set_verse_numbers(user, args[0]):
            embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["setversenumbers"],
                            value=lang["versenumberssuccess"])

            return {
                "level": "info",
                "message": embed
            }
        else:
            embed.color = 16723502

            response = lang["versenumbersfail"].replace("<setversenumbers>", lang["commands"]["setversenumbers"])
            response = response.replace("<enable>", lang["arguments"]["enable"])
            response = response.replace("<disable>", lang["arguments"]["disable"])

            embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["setversenumbers"],
                            value=response)

            return {
                "level": "err",
                "message": embed
            }
    elif command == "versenumbers":
        verse_numbers = formatting.get_verse_numbers(user)

        embed.color = 303102
        embed.set_footer(text=central.version, icon_url=central.icon)

        if verse_numbers == "enable":
            response = lang["versenumbers"].replace("<enabled/disabled>", lang["enabled"])
            response = response.replace("<setversenumbers>", lang["commands"]["setversenumbers"])
            embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["versenumbers"],
                            value=response)

            return {
                "level": "info",
                "message": embed
            }
        else:
            response = lang["versenumbers"].replace("<enabled/disabled>", lang["disabled"])
            response = response.replace("<setversenumbers>", lang["commands"]["setversenumbers"])
            embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["versenumbers"],
                            value=response)

            return {
                "level": "info",
                "message": embed
            }
    elif command == "setannouncements":
        perms = user.guild_permissions

        if str(user.id) != central.config["BibleBot"]["owner"]:
            if not perms.manage_guild:
                embed.color = 16723502
                embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["setannouncements"],
                                value=lang["setannouncementsnoperm"])

                return {
                    "level": "err",
                    "message": embed
                }

        if misc.set_guild_announcements(guild, channel, args[0]):
            embed.color = 303102
            embed.set_footer(text=central.version, icon_url=central.icon)

            embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["setannouncements"],
                            value=lang["setannouncementssuccess"])

            return {
                "level": "info",
                "message": embed
            }
        else:
            response = lang["setannouncementsfail"]

            response = response.replace("<setannouncements>", lang["commands"]["setannouncements"])
            response = response.replace("<enable>", lang["arguments"]["enable"])
            response = response.replace("<disable>", lang["arguments"]["disable"])

            embed.color = 16723502
            embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["setannouncements"],
                            value=response)

            return {
                "level": "err",
                "message": embed
            }
    elif command == "announcements":
        announce_tuple = misc.get_guild_announcements(guild, True)

        if announce_tuple is not None:
            channel, setting = announce_tuple

            embed.color = 303102
            embed.set_footer(text=central.version, icon_url=central.icon)

            if setting:
                response = lang["announcementsenabled"]
            else:
                response = lang["announcementsdisabled"]

            response = response.replace("<channel>", channel)
            response = response.replace("<setannouncements>", lang["commands"]["setannouncements"])

            embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["announcements"],
                            value=response)

            return {
                "level": "info",
                "message": embed
            }
        else:
            response = lang["noannouncements"]

            response = response.replace("<setannouncements>", lang["commands"]["setannouncements"])

            embed.color = 16723502
            embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["announcements"],
                            value=response)

            return {
                "level": "err",
                "message": embed
            }
    elif command == "users":
        embed.color = 303102
        embed.set_footer(text=central.version, icon_url=central.icon)

        processed = len(args[0].users)

        embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["users"],
                        value=lang["users"] + ": " + str(processed))

        return {
            "level": "info",
            "message": embed
        }
    elif command == "servers":
        embed.color = 303102
        embed.set_footer(text=central.version, icon_url=central.icon)

        processed = len(args[0].guilds)

        embed.add_field(name=central.config["BibleBot"]["commandPrefix"] + lang["commands"]["servers"],
                        value=lang["servers"].replace("<count>", str(processed)))

        return {
            "level": "info",
            "message": embed
        }
    elif command == "jepekula":
        version = versions.get_version(user)
        headings = formatting.get_headings(user)
        verse_numbers = formatting.get_verse_numbers(user)

        if version is None:
            version = versions.get_guild_version(guild)

            if version is None:
                version = "NRSV"

        verse = "Mark 9:23-24"

        if version != "REV":
            result = biblegateway.get_result(verse, version, headings, verse_numbers)

            content = "```Dust\n" + result["title"] + "\n\n" + result["text"] + "```"
            response_string = "**" + result["passage"] + " - " + result["version"] + "**\n\n" + content

            if len(response_string) < 2000:
                return {
                    "level": "info",
                    "reference": verse,
                    "message": response_string
                }
        else:
            result = rev.get_result(verse, verse_numbers)

            content = "```Dust\n" + result["title"] + "\n\n" + result["text"] + "```"
            response_string = "**" + result["passage"] + " - " + result["version"] + "**\n\n" + content

            if len(response_string) < 2000:
                return {
                    "level": "info",
                    "reference": verse,
                    "message": response_string
                }
    elif command == "joseph":
        return {
            "level": "info",
            "text": True,
            "message": "Jesus never consecrated peanut butter " +
                       "and jelly sandwiches and Coca-Cola!"
        }
    elif command == "tiger":
        return {
            "level": "info",
            "text": True,
            "message": "Our favorite Tiger lives by Ephesians 4:29,31-32, " +
                       "Matthew 16:26, James 4:6, and lastly, his calling from God, " +
                       "1 Peter 5:8. He tells everyone that because of grace in faith " +
                       "(Ephesians 2:8-10) he was saved, and not of works. Christ " +
                       "Jesus has made him a new creation (2 Corinthians 5:17)."
        }
    elif command == "supporters":
        embed.color = 303102
        embed.set_footer(text=central.version, icon_url=central.icon)

        supporters = "**" + lang["supporters"] + "**\n\n- CHAZER2222\n- Jepekula" + "\n- Joseph\n- Soku\n- " + \
                     lang["anonymousDonors"] + "\n\n" + lang["donorsNotListed"]

        embed.add_field(name="+" + lang["commands"]["supporters"], value=supporters)

        return {
            "level": "info",
            "message": embed
        }
    elif command == "creeds":
        embed.color = 303102
        embed.set_footer(text=central.version, icon_url=central.icon)

        response = lang["creedstext"]

        response = response.replace("<apostles>", lang["commands"]["apostles"])
        response = response.replace("<nicene>", lang["commands"]["nicene"])
        response = response.replace("<chalcedonian>", lang["commands"]["chalcedonian"])
        response = response.replace("<athanasian>", lang["commands"]["athanasian"])

        embed.add_field(name=lang["creeds"], value=response)

        return {
            "level": "info",
            "message": embed
        }
    elif command == "apostles":
        embed.color = 303102
        embed.set_footer(text=central.version, icon_url=central.icon)

        embed.add_field(name=lang["apostlescreed"], value=lang["apostlestext1"])

        return {
            "level": "info",
            "message": embed
        }
    elif command == "nicene":
        embed.color = 303102
        embed.set_footer(text=central.version, icon_url=central.icon)

        embed.add_field(name=lang["nicenecreed"], value=lang["nicenetext1"])
        embed.add_field(name=u"\u200B", value=lang["nicenetext2"])
        embed.add_field(name=u"\u200B", value=lang["nicenetext3"])
        embed.add_field(name=u"\u200B", value=lang["nicenetext4"])

        return {
            "level": "info",
            "message": embed
        }
    elif command == "chalcedonian":
        embed.color = 303102
        embed.set_footer(text=central.version, icon_url=central.icon)

        embed.add_field(name=lang["chalcedoniancreed"], value=lang["chalcedoniantext1"])
        embed.add_field(name=u"\u200B", value=lang["chalcedoniantext2"])

        return {
            "level": "info",
            "message": embed
        }
    elif command == "athanasian":
        embed.color = 303102
        embed.set_footer(text=central.version, icon_url=central.icon)

        embed.add_field(name=lang["pseudoathanasiancreed"],
                        value=lang["pseudoathanasiantext1"] + "https://www.ccel.org/creeds/athanasian.creed.html")

        return {
            "level": "info",
            "message": embed
        }
    elif command == "invite":
        return {
            "level": "info",
            "text": True,
            "message": "<https://discordapp.com/oauth2/authorize?" +
                       "client_id=361033318273384449&scope=bot&permissions=19520>"
        }


def run_owner_command(bot, command, args, lang):
    embed = discord.Embed()

    if command == "puppet":
        message = ""

        for item in args:
            message += item + " "

        if message == " " or message == "":
            return

        return {
            "level": "info",
            "text": True,
            "message": message[0:-1]
        }
    elif command == "eval":
        message = ""

        for item in args:
            message += item + " "

        try:
            return {
                "level": "info",
                "text": True,
                "message": exec(message[0:-1])
            }
        except Exception as e:
            return {
                "level": "err",
                "text": True,
                "message": "[err] " + str(e)
            }
    elif command == "announce":
        embed.color = 303102
        embed.set_footer(text=central.version, icon_url=central.icon)

        message = ""

        for item in args:
            message += item + " "

        embed.add_field(name="Announcement", value=message[0:-1])

        return {
            "level": "info",
            "announcement": True,
            "message": embed
        }
    elif command == "addversion":
        embed.color = 303102
        embed.set_footer(text=central.version, icon_url=central.icon)

        argc = len(args)
        name = ""

        for i in range(0, (argc - 4)):
            name += args[i] + " "

        name = name[0:-1]
        abbv = args[argc - 4]

        has_ot = False
        has_nt = False
        has_deu = False

        if args[argc - 3] == "yes":
            has_ot = True

        if args[argc - 2] == "yes":
            has_nt = True

        if args[argc - 1] == "yes":
            has_deu = True

        new_version = Version(name, abbv, has_ot, has_nt, has_deu)
        central.versionDB.insert(new_version.to_object())

        embed.add_field(name="+" + lang["commands"]["addversion"], value=lang["addversionsuccess"])

        return {
            "level": "info",
            "message": embed
        }
    elif command == "userid":
        arg = ""

        for item in args:
            arg += item + " "

        split = arg[0:-1].split("#")
        results = "IDs matching: "

        if len(split) == 2:
            users = [x for x in bot.users if x.name == split[0] and x.discriminator == split[1]]

            for item in users:
                results += str(item.id) + ", "

            results = results[0:-2]
        else:
            results += "None"

        return {
            "level": "info",
            "text": True,
            "message": results
        }
    elif command == "ban":
        ban_reason = ""

        for index in range(0, len(args)):
            if index != 0:
                ban_reason += args[index] + " "

        if central.is_snowflake(args[0]):
            if central.add_ban(args[0], ban_reason[0:-1]):
                return {
                    "level": "info",
                    "text": True,
                    "message": "Banned " + args[0] + " for " + ban_reason[0:-1] + "."
                }
            else:
                return {
                    "level": "err",
                    "text": True,
                    "message": args[0] + " is already banned."
                }
        else:
            return {
                "level": "err",
                "text": True,
                "message": "This is not an ID."
            }
    elif command == "unban":
        if central.is_snowflake(args[0]):
            if central.remove_ban(args[0]):
                return {
                    "level": "info",
                    "text": True,
                    "message": "Unbanned " + args[0] + "."
                }
            else:
                return {
                    "level": "err",
                    "text": True,
                    "message": args[0] + " is not banned."
                }
        else:
            return {
                "level": "err",
                "text": True,
                "message": "This is not an ID."
            }
    elif command == "reason":
        if central.is_snowflake(args[0]):
            is_banned, reason = central.is_banned(args[0])
            if is_banned:
                if reason is not None:
                    return {
                        "level": "info",
                        "text": True,
                        "message": args[0] + " is banned for `" + reason + "`."
                    }
                else:
                    return {
                        "level": "info",
                        "text": True,
                        "message": args[0] + " is banned for an unknown reason."
                    }
            else:
                return {
                    "level": "err",
                    "text": True,
                    "message": args[0] + " is not banned."
                }
        else:
            return {
                "level": "err",
                "text": True,
                "message": "This is not an ID."
            }
    elif command == "optout":
        if central.is_snowflake(args[0]):
            if central.add_optout(args[0]):
                return {
                    "level": "info",
                    "text": True,
                    "message": "Opt out " + args[0] + "."
                }
            else:
                return {
                    "level": "err",
                    "text": True,
                    "message": args[0] + " is already opt out."
                }
        else:
            return {
                "level": "err",
                "text": True,
                "message": "This is not an ID."
            }
    elif command == "unoptout":
        if central.is_snowflake(args[0]):
            if central.remove_optout(args[0]):
                return {
                    "level": "info",
                    "text": True,
                    "message": "Unoptout " + args[0] + "."
                }
            else:
                return {
                    "level": "err",
                    "text": True,
                    "message": args[0] + " is not opt out."
                }
        else:
            return {
                "level": "err",
                "text": True,
                "message": "This is not an ID."
            }
    elif command == "leave":
        if len(args) > 0:
            exists = False
            server_id = None
            server_name = ""

            for arg in args:
                server_name += arg + " "

            for item in bot.guilds:
                if item.name == server_name[0:-1]:
                    exists = True
                    server_id = item.id

            if exists:
                return {
                    "level": "info",
                    "leave": str(server_id)
                }
            else:
                return {
                    "level": "err",
                    "text": True,
                    "message": "Server does not exist!"
                }
        else:
            return {
                "level": "info",
                "leave": "this"
            }
