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

import discord
import central
import ast

from bible_modules import biblehub, bibleserver, apibible, biblegateway, rev
from handlers.logic.settings import versions
from handlers.logic.verses import utils as verseutils


def divide_list(dividend, divisor):
    # tl;dr for every n (divisor) entries, separate and place into a new array
    # divide_list([1,2,3,4], 2) = [[1,2],[3,4]]
    return [dividend[i:i + divisor] for i in range(0, len(dividend), divisor)]


def insert_returns(body):  # for +eval, thanks to nitros12 on github for the code
    # insert return stmt if the last expression is a expression statement
    if isinstance(body[-1], ast.Expr):
        body[-1] = ast.Return(body[-1].value)
        ast.fix_missing_locations(body[-1])

    # for if statements, we insert returns into the body and the orelse
    if isinstance(body[-1], ast.If):
        insert_returns(body[-1].body)
        insert_returns(body[-1].orelse)

    # for with blocks, again we insert returns into the body
    if isinstance(body[-1], ast.With):
        insert_returns(body[-1].body)


def create_embed(title, description, custom_title=False, error=False):
    embed = discord.Embed()

    if error:
        embed.color = 16723502
    else:
        embed.color = 303102

    embed.set_footer(text=f"BibleBot {central.version}", icon_url=central.icon)

    if custom_title:
        embed.title = title
    else:
        embed.title = central.config["BibleBot"]["commandPrefix"] + title

    embed.description = description

    return embed


def get_version(user, guild):
    version = versions.get_version(user)

    if version is None:
        version = versions.get_guild_version(guild)

        if version is None:
            version = "RSV"

    return version


def get_bible_verse(reference, version, headings, verse_numbers):
    biblehub_versions = ["BSB", "NHEB", "WBT"]
    bibleserver_versions = ["LUT", "LXX", "SLT"]
    apibible_versions = ["KJVA"]
    other_versions = ["REV"]

    non_bg = other_versions + biblehub_versions + apibible_versions + bibleserver_versions

    if version not in non_bg:
        result = biblegateway.get_result(reference, version, headings, verse_numbers)

        print(verseutils)
        processed_results = verseutils.process_result(result, reference, version, None)
        print(processed_results)

        return utils.process_result(result, reference, version, None)[0]
    elif version == "REV":
        result = rev.get_result(reference, verse_numbers)
        return utils.process_result(result, reference, version, None)[0]
    elif version in biblesorg_versions:
        result = apibible.get_result(reference, version, headings, verse_numbers)
        return utils.process_result(result, reference, version, None)[0]
    elif version in biblehub_versions:
        result = biblehub.get_result(reference, version, verse_numbers)
        return utils.process_result(result, reference, version, None)[0]
    elif version in bibleserver_versions:
        result = bibleserver.get_result(reference, version, verse_numbers)
        return utils.process_result(result, reference, version, None)[0]
