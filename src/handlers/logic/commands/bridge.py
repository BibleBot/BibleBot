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

import os
import sys
import ast

import discord
import tinydb

from .information import biblebot, creeds, catechisms, special, paged_commands
from handlers.logic.settings import versions
from handlers.logic.settings import languages, misc, formatting
from . import utils

dir_path = os.path.dirname(os.path.realpath(__file__))
sys.path.append(f"{dir_path}/../..")

from vytypes.version import Version  # noqa: E402
from bible_modules import bibleutils  # noqa: E402
import central  # noqa: E402


async def run_command(ctx, command, remainder):
    lang = ctx["language"]
    user = ctx["author"]
    guild = ctx["guild"]
    channel = ctx["channel"]
    args = remainder.split(" ")

    if command == "biblebot":
        pages = biblebot.create_biblebot_embeds(lang)

        return {
            "level": "info",
            "paged": True,
            "pages": pages
        }

    elif command == "qrandom":
        verse = await bibleutils.get_quantum_random_verse()
        mode = formatting.get_mode(user)
        version = utils.get_version(user, guild)
        headings = formatting.get_headings(user)
        verse_numbers = formatting.get_verse_numbers(user)

        return await utils.get_bible_verse(verse, mode, version, headings, verse_numbers)
    elif command == "search":
        version = utils.get_version(user, guild)
        return await paged_commands.search(version, remainder, lang)
    elif command == "versions":
        return paged_commands.get_versions(lang)
    elif command == "setversion":
        if versions.set_version(user, args[0]):
            embed = utils.create_embed(lang["commands"]["setversion"], lang["setversionsuccess"])

            return {
                "level": "info",
                "message": embed
            }
        else:
            embed = utils.create_embed(lang["commands"]["setversion"],
                                       lang["setversionfail"].replace("<versions>", lang["commands"]["versions"]),
                                       error=True)

            return {
                "level": "err",
                "message": embed
            }
    elif command == "setguildversion":
        perms = user.guild_permissions
        
        if not perms:
            embed = utils.create_embed(lang["commands"]["setguildversion"], lang["setguildversionnoperm"],
                                           error=True)
            
            return {
                    "level": "err",
                    "message": embed
            }

        if str(user.id) != central.config["BibleBot"]["owner"]:
            if not perms.manage_guild:
                embed = utils.create_embed(lang["commands"]["setguildversion"], lang["setguildversionnoperm"],
                                           error=True)

                return {
                    "level": "err",
                    "message": embed
                }

        if versions.set_guild_version(guild, args[0]):
            embed = utils.create_embed(lang["commands"]["setguildversion"], lang["setguildversionsuccess"])

            return {
                "level": "info",
                "message": embed
            }
        else:
            embed = utils.create_embed(lang["commands"]["setguildversion"],
                                       lang["setguildversionfail"].replace("<versions>", lang["commands"]["versions"]),
                                       error=True)

            return {
                "level": "err",
                "message": embed
            }
    elif command == "version":
        version = versions.get_version(user)

        if version is not None:
            response = lang["versionused"]

            response = response.replace("<version>", version)
            response = response.replace("<setversion>", lang["commands"]["setversion"])

            embed = utils.create_embed(lang["commands"]["version"], response)

            return {
                "level": "info",
                "message": embed
            }
        else:
            response = lang["noversionused"]
            response = response.replace("<setversion>", lang["commands"]["setversion"])

            embed = utils.create_embed(lang["commands"]["version"], response, error=True)

            return {
                "level": "err",
                "message": embed
            }
    elif command == "guildversion":
        version = versions.get_guild_version(guild)

        if version is not None:
            response = lang["guildversionused"]

            response = response.replace("<version>", version)
            response = response.replace("<setguildversion>", lang["commands"]["setguildversion"])

            embed = utils.create_embed(lang["commands"]["guildversion"], response)

            return {
                "level": "info",
                "message": embed
            }
        else:
            response = lang["noguildversionused"]
            response = response.replace("<setguildversion>", lang["commands"]["setguildversion"])

            embed = utils.create_embed(lang["commands"]["guildversion"], response, error=True)

            return {
                "level": "err",
                "message": embed
            }
    elif command == "versioninfo":
        ideal_version = tinydb.Query()
        results = central.versionDB.search(ideal_version["abbv"] == args[0])

        if len(results) > 0:
            response = lang["versioninfo"]

            response = response.replace("<versionname>", results[0]["name"])

            def check_validity(section):
                if results[0]["has" + section]:
                    return lang["arguments"]["yes"]
                else:
                    return lang["arguments"]["no"]

            for category in ["OT", "NT", "DEU"]:
                response = response.replace(f"<has{category}>", check_validity(category))

            embed = utils.create_embed(lang["commands"]["versioninfo"], response)

            return {
                "level": "info",
                "message": embed
            }
        else:
            embed = utils.create_embed(lang["commands"]["versioninfo"], lang["versioninfofailed"], error=True)

            return {
                "level": "err",
                "message": embed
            }
    elif command == "setlanguage":
        if languages.set_language(user, args[0]):
            embed = utils.create_embed(lang["commands"]["setlanguage"], lang["setlanguagesuccess"])

            return {
                "level": "info",
                "message": embed
            }
        else:
            embed = utils.create_embed(lang["commands"]["setlanguage"],
                                       lang["setlanguagefail"].replace("<languages>", lang["commands"]["languages"]),
                                       error=True)

            return {
                "level": "err",
                "message": embed
            }
    elif command == "setguildlanguage":
        perms = user.guild_permissions

        if not perms:
            embed = utils.create_embed(lang["commands"]["setguildlanguage"], lang["setguildlanguagenoperm"],
                                           error=True)
            
            return {
                    "level": "err",
                    "message": embed
            }
        
        if str(user.id) != central.config["BibleBot"]["owner"]:
            if not perms.manage_guild:
                embed = utils.create_embed(lang["commands"]["setguildlanguage"], lang["setguildlanguagenoperm"],
                                           error=True)

                return {
                    "level": "err",
                    "message": embed
                }

        if languages.set_guild_language(guild, args[0]):
            embed = utils.create_embed(lang["commands"]["setguildlanguage"], lang["setguildlanguagesuccess"])

            return {
                "level": "info",
                "message": embed
            }
        else:
            description = lang["setguildlanguagefail"].replace("<languages>", lang["commands"]["languages"])

            embed = utils.create_embed(lang["commands"]["setguildlanguage"], description, error=True)

            return {
                "level": "err",
                "message": embed
            }
    elif command == "language":
        response = lang["languageused"]
        response = response.replace("<setlanguage>", lang["commands"]["setlanguage"])

        embed = utils.create_embed(lang["commands"]["language"], response)

        return {
            "level": "info",
            "message": embed
        }
    elif command == "guildlanguage":
        glang = languages.get_guild_language(guild)
        glang = central.get_raw_language(glang)

        response = glang["guildlanguageused"]
        response = response.replace("<setguildlanguage>", glang["commands"]["setguildlanguage"])

        embed = utils.create_embed(glang["commands"]["guildlanguage"], response)

        return {
            "level": "info",
            "message": embed
        }
    elif command == "languages":
        available_languages = languages.get_languages()

        string = ""

        for item in available_languages:
            if item["object_name"] != "default":
                string += item["name"] + " [`" + item["object_name"] + "`]\n"

        embed = utils.create_embed(lang["commands"]["languages"], string)

        return {
            "level": "info",
            "message": embed
        }
    elif command == "setguildbrackets":
        perms = user.guild_permissions

        if not perms:
            embed = utils.create_embed(lang["commands"]["setguildbrackets"], lang["setguildbracketsnoperm"],
                                           error=True)
            
            return {
                    "level": "err",
                    "message": embed
            }
        
        if str(user.id) != central.config["BibleBot"]["owner"]:
            if not perms.manage_guild:
                embed = utils.create_embed(lang["commands"]["setguildbrackets"], lang["setguildbracketsnoperm"],
                                           error=True)

                return {
                    "level": "err",
                    "message": embed
                }

        if formatting.set_guild_brackets(guild, args[0]):
            embed = utils.create_embed(lang["commands"]["setguildbrackets"], lang["setguildbracketssuccess"])

            return {
                "level": "info",
                "message": embed
            }
        else:
            embed = utils.create_embed(lang["commands"]["setguildbrackets"], lang["setguildbracketsfail"], error=True)

            return {
                "level": "err",
                "message": embed
            }
    elif command == "guildbrackets":
        brackets_dict = formatting.get_guild_brackets(guild)
        brackets = brackets_dict["first"] + brackets_dict["second"]

        response = lang["guildbracketsused"]

        response = response.replace("<brackets>", brackets)
        response = response.replace("<setguildbrackets>", lang["commands"]["setguildbrackets"])

        embed = utils.create_embed(lang["commands"]["guildbrackets"], response)

        return {
            "level": "info",
            "message": embed
        }
    elif command == "setvotdtime":
        perms = user.guild_permissions
        
        if not perms:
            embed = utils.create_embed(lang["commands"]["setvotdtime"], lang["setvotdtimenoperm"],
                                           error=True)
            
            return {
                    "level": "err",
                    "message": embed
            }

        if str(user.id) != central.config["BibleBot"]["owner"]:
            if not perms.manage_guild:
                embed = utils.create_embed(lang["commands"]["setvotdtime"], lang["setvotdtimenoperm"], error=True)

                return {
                    "level": "err",
                    "message": embed
                }

        if misc.set_guild_votd_time(guild, channel, args[0]):
            embed = utils.create_embed(lang["commands"]["setvotdtime"], lang["setvotdtimesuccess"])

            return {
                "level": "info",
                "message": embed
            }
        else:
            embed = utils.create_embed(lang["commands"]["setvotdtime"], lang["setvotdtimefail"], error=True)

            return {
                "level": "err",
                "message": embed
            }
    elif command == "clearvotdtime":
        perms = user.guild_permissions
        
        if not perms:
            embed = utils.create_embed(lang["commands"]["clearvotdtime"], lang["clearvotdtimenoperm"],
                                           error=True)
            
            return {
                    "level": "err",
                    "message": embed
            }

        if str(user.id) != central.config["BibleBot"]["owner"]:
            if not perms.manage_guild:
                embed = utils.create_embed(lang["commands"]["clearvotdtime"], lang["clearvotdtimenoperm"], error=True)

                return {
                    "level": "err",
                    "message": embed
                }

        if misc.set_guild_votd_time(guild, channel, "clear"):
            embed = utils.create_embed(lang["commands"]["clearvotdtime"], lang["clearvotdtimesuccess"])

            return {
                "level": "info",
                "message": embed
            }
    elif command == "votdtime":
        time_tuple = misc.get_guild_votd_time(guild)

        if time_tuple is not None:
            channel, time = time_tuple

            response = lang["votdtimeused"]

            response = response.replace("<time>", time + " UTC")
            response = response.replace("<channel>", channel)
            response = response.replace("<setvotdtime>", lang["commands"]["setvotdtime"])
            response = response.replace("<clearvotdtime>", lang["commands"]["clearvotdtime"])

            embed = utils.create_embed(lang["commands"]["votdtime"], response)

            return {
                "level": "info",
                "message": embed
            }
        else:
            response = lang["novotdtimeused"]
            response = response.replace("<setvotdtime>", lang["commands"]["setvotdtime"])

            embed = utils.create_embed(lang["commands"]["votdtime"], response, error=True)

            return {
                "level": "err",
                "message": embed
            }
    elif command in ["votd", "verseoftheday"]:
        verse = await bibleutils.get_votd()
        mode = formatting.get_mode(user)
        version = utils.get_version(user, guild)
        headings = formatting.get_headings(user)
        verse_numbers = formatting.get_verse_numbers(user)

        return await utils.get_bible_verse(verse, mode, version, headings, verse_numbers)
    elif command == "random":
        verse = await bibleutils.get_random_verse()
        mode = formatting.get_mode(user)
        version = utils.get_version(user, guild)
        headings = formatting.get_headings(user)
        verse_numbers = formatting.get_verse_numbers(user)

        return await utils.get_bible_verse(verse, mode, version, headings, verse_numbers)
    elif command == "setheadings":
        if formatting.set_headings(user, args[0]):
            embed = utils.create_embed(lang["commands"]["setheadings"], lang["headingssuccess"])

            return {
                "level": "info",
                "message": embed
            }
        else:
            response = lang["headingsfail"]

            response = response.replace("<setheadings>", lang["commands"]["setheadings"])
            response = response.replace("<enable>", lang["arguments"]["enable"])
            response = response.replace("<disable>", lang["arguments"]["disable"])

            embed = utils.create_embed(lang["commands"]["setheadings"], response, error=True)

            return {
                "level": "err",
                "message": embed
            }
    elif command == "headings":
        headings = formatting.get_headings(user)

        if headings == "enable":
            response = lang["headings"].replace("<enabled/disabled>", lang["enabled"])
            response = response.replace("<setheadings>", lang["commands"]["setheadings"])

            embed = utils.create_embed(lang["commands"]["headings"], response)

            return {
                "level": "info",
                "message": embed
            }
        else:
            response = lang["headings"].replace("<enabled/disabled>", lang["disabled"])
            response = response.replace("<setheadings>", lang["commands"]["setheadings"])

            embed = utils.create_embed(lang["commands"]["headings"], response)

            return {
                "level": "info",
                "message": embed
            }
    elif command == "setmode":
        if formatting.set_mode(user, args[0]):
            embed = utils.create_embed(lang["commands"]["setmode"], lang["modesuccess"])

            return {
                "level": "info",
                "message": embed
            }
        else:
            response = lang["modefail"]

            response = response.replace("<setmode>", lang["commands"]["setmode"])

            embed = utils.create_embed(lang["commands"]["setmode"], response, error=True)

            return {
                "level": "err",
                "message": embed
            }
    elif command == "mode":
        mode = formatting.get_mode(user)
        modes = ["default", "embed", "blockquote", "code"]

        if mode in modes:
            response = lang["mode"].replace("<value>", mode)
            response = response.replace("<setmode>", lang["commands"]["setmode"])

            embed = utils.create_embed(lang["commands"]["mode"], response)

            return {
                "level": "info",
                "message": embed
            }
    elif command == "setversenumbers":
        if formatting.set_verse_numbers(user, args[0]):
            embed = utils.create_embed(lang["commands"]["setversenumbers"], lang["versenumberssuccess"])

            return {
                "level": "info",
                "message": embed
            }
        else:
            response = lang["versenumbersfail"].replace("<setversenumbers>", lang["commands"]["setversenumbers"])
            response = response.replace("<enable>", lang["arguments"]["enable"])
            response = response.replace("<disable>", lang["arguments"]["disable"])

            embed = utils.create_embed(lang["commands"]["setversenumbers"], response, error=True)

            return {
                "level": "err",
                "message": embed
            }
    elif command == "versenumbers":
        verse_numbers = formatting.get_verse_numbers(user)
        response = lang["versenumbers"].replace("<setversenumbers>", lang["commands"]["setversenumbers"])

        if verse_numbers == "enable":
            response = response.replace("<enabled/disabled>", lang["enabled"])
        else:
            response = response.replace("<enabled/disabled>", lang["disabled"])

        embed = utils.create_embed(lang["commands"]["versenumbers"], response)

        return {
            "level": "info",
            "message": embed
        }
    elif command == "setannouncements":
        perms = user.guild_permissions
        
        if not perms:
            embed = utils.create_embed(lang["commands"]["setannouncements"], lang["setannouncementsnoperm"],
                                       error=True)
            
            return {
                    "level": "err",
                    "message": embed
            }

        if not perms.manage_guild:
            embed = utils.create_embed(lang["commands"]["setannouncements"], lang["setannouncementsnoperm"], error=True)

            return {
                "level": "err",
                "message": embed
            }

        if misc.set_guild_announcements(guild, channel, args[0]):
            embed = utils.create_embed(lang["commands"]["setannouncements"], lang["setannouncementssuccess"])

            return {
                "level": "info",
                "message": embed
            }
        else:
            response = lang["setannouncementsfail"]

            response = response.replace("<setannouncements>", lang["commands"]["setannouncements"])
            response = response.replace("<enable>", lang["arguments"]["enable"])
            response = response.replace("<disable>", lang["arguments"]["disable"])

            embed = utils.create_embed(lang["commands"]["setannouncements"], response, error=True)

            return {
                "level": "err",
                "message": embed
            }
    elif command == "announcements":
        announce_tuple = misc.get_guild_announcements(guild, True)

        if announce_tuple is not None:
            channel, setting = announce_tuple

            if setting:
                response = lang["announcementsenabled"]
            else:
                response = lang["announcementsdisabled"]

            response = response.replace("<channel>", channel)
            response = response.replace("<setannouncements>", lang["commands"]["setannouncements"])

            embed = utils.create_embed(lang["commands"]["announcements"], response)

            return {
                "level": "info",
                "message": embed
            }
        else:
            response = lang["noannouncements"]
            response = response.replace("<setannouncements>", lang["commands"]["setannouncements"])

            embed = utils.create_embed(lang["commands"]["announcements"], response, error=True)

            return {
                "level": "err",
                "message": embed
            }
    elif command == "users":
        processed = len(ctx["self"].users)

        embed = utils.create_embed(lang["commands"]["users"], lang["users"] + ": " + str(processed))

        return {
            "level": "info",
            "message": embed
        }
    elif command == "servers":
        processed = len(ctx["self"].guilds)
        embed = utils.create_embed(lang["commands"]["servers"], lang["servers"].replace("<count>", str(processed)))

        return {
            "level": "info",
            "message": embed
        }
    elif command == "jepekula":
        version = utils.get_version(user, guild)
        mode = formatting.get_mode(user)
        headings = formatting.get_headings(user)
        verse_numbers = formatting.get_verse_numbers(user)

        return await utils.get_bible_verse("Mark 9:23-24", mode, version, headings, verse_numbers)
    elif command in special.cm_commands:
        return special.get_custom_message(command)
    elif command == "supporters":
        return special.get_supporters(lang)
    elif command == "creeds":
        return creeds.get_creeds(lang)
    elif command in creeds.creeds:
        return creeds.get_creed(command, lang)
    elif command == "catechisms":
        return catechisms.get_catechisms(lang)
    elif command == "invite":
        bot_id = ctx["self"].user.id

        return {
            "level": "info",
            "text": True,
            "message": f"https://discordapp.com/oauth2/authorize?client_id={bot_id}&scope=bot&permissions=93248"
        }


async def run_owner_command(ctx, command, remainder):
    embed = discord.Embed()
    lang = ctx["language"]
    user = ctx["author"]
    guild = ctx["guild"]
    channel = ctx["channel"]
    args = remainder.split(" ")

    if command == "puppet":
        message = ""

        for item in args:
            message += f"{item} "

        if message == " " or message == "":
            return

        try:
            await ctx["raw"].delete()
        except (discord.errors.Forbidden, discord.errors.HTTPException):
            pass

        await channel.send(message[0:-1])

        return None
    elif command == "eval":
        cmd = " ".join(args)
        fn_name = "_eval_expr"
        cmd = cmd.strip("` ")

        # add a layer of indentation
        cmd = "\n".join(f"    {i}" for i in cmd.splitlines())

        # wrap in def body
        body = f"async def {fn_name}():\n{cmd}"
        parsed = ast.parse(body)
        body = parsed.body[0].body

        utils.insert_returns(body)

        env = {
            'bot': ctx["self"],
            'discord': discord,
            '__import__': __import__,
            'channel': channel,
            'central': central
        }

        exec(compile(parsed, filename="<ast>", mode="exec"), env)

        result = (await eval(f"{fn_name}()", env))

        return {
            "level": "info",
            "text": True,
            "message": result
        }
    elif command == "announce":
        message = ""

        for item in args:
            message += f"{item} "

        embed = utils.create_embed("Announcement", message[0:-1], custom_title=True)

        return {
            "level": "info",
            "announcement": True,
            "message": embed
        }
    elif command == "addversion":
        argc = len(args)
        name = ""

        for i in range(0, (argc - 4)):
            name += f"{args[i]} "

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

        embed = utils.create_embed(lang["commands"]["addversion"], lang["addversionsuccess"])

        return {
            "level": "info",
            "message": embed
        }
    elif command == "rmversion":
        query = tinydb.Query()

        result = central.versionDB.remove(query.abbv == args[0])

        if result:
            return {
                "level": "info",
                "text": True,
                "message": f"Removed {result}."
            }
        else:
            return {
                "level": "err",
                "text": True,
                "message": f"No version available to remove."
            }
    elif command == "userid":
        arg = ""

        for item in args:
            arg += f"{item} "

        split = arg[0:-1].split("#")
        results = "IDs matching: "

        if len(split) == 2:
            users = [x for x in ctx["bot"].users if x.name == split[0] and x.discriminator == split[1]]

            for item in users:
                results += f"{str(item.id)}, "

            results = results[0:-2]
        else:
            results += "None"

        return {
            "level": "info",
            "text": True,
            "message": results
        }
    elif command == "optout":
        if central.is_snowflake(args[0]):
            if central.add_optout(args[0]):
                return {
                    "level": "info",
                    "text": True,
                    "message": f"Opt out {args[0]}."
                }
            else:
                return {
                    "level": "err",
                    "text": True,
                    "message": f"{args[0]} is already opt out."
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
                    "message": f"Unoptout {args[0]}."
                }
            else:
                return {
                    "level": "err",
                    "text": True,
                    "message": f"{args[0]} is not opt out."
                }
        else:
            return {
                "level": "err",
                "text": True,
                "message": "This is not an ID."
            }
    elif command == "leave":
        if len(args) > 0:
            server_name = ""

            for arg in args:
                server_name += arg + " "

            for item in ctx["bot"].guilds:
                if item.name == server_name[0:-1]:
                    await item.leave()
                    return {"level": "info", "leave": True}

            await channel.send("Server does not exist.")
            return None
        else:
            await guild.leave()
            return None
