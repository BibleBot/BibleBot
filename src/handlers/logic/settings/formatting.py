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

import tinydb

dir_path = os.path.dirname(os.path.realpath(__file__))
sys.path.append(f"{dir_path}/../../..")

import central  # noqa: E402


def set_headings(user, headings):
    headings = headings.lower()

    if headings != "enable" and headings != "disable":
        return False

    ideal_user = tinydb.Query()
    results = central.db.search(ideal_user.id == user.id)

    if len(results) > 0:
        central.db.update({"headings": headings}, ideal_user.id == user.id)
    else:
        central.db.insert({"id": user.id, "headings": headings})

    return True


def get_headings(user):
    ideal_user = tinydb.Query()
    results = central.db.search(ideal_user.id == user.id)

    if len(results) > 0:
        if "headings" in results[0]:
            return results[0]["headings"]
        else:
            return "enable"
    else:
        return "enable"


def set_verse_numbers(user, verse_numbers):
    verse_numbers = verse_numbers.lower()

    if verse_numbers != "enable" and verse_numbers != "disable":
        return False

    ideal_user = tinydb.Query()
    results = central.db.search(ideal_user.id == user.id)

    if len(results) > 0:
        central.db.update({"verseNumbers": verse_numbers}, ideal_user.id == user.id)
    else:
        central.db.insert({"id": user.id, "verseNumbers": verse_numbers})

    return True


def get_verse_numbers(user):
    ideal_user = tinydb.Query()
    results = central.db.search(ideal_user.id == user.id)

    if len(results) > 0:
        if "verseNumbers" in results[0]:
            return results[0]["verseNumbers"]
        else:
            return "enable"
    else:
        return "enable"


def set_guild_brackets(guild, brackets):
    if len(brackets) != 2:
        return False

    if brackets not in ["<>", "()", "{}", "[]"]:
        return False

    ideal_guild = tinydb.Query()
    results = central.guildDB.search(ideal_guild.id == guild.id)

    item = {
        "first": brackets[0],
        "second": brackets[1]
    }

    if len(results) > 0:
        central.guildDB.update({"brackets": item}, ideal_guild.id == guild.id)
    else:
        central.guildDB.insert({"id": guild.id, "brackets": item})

    return True


def get_guild_brackets(guild):
    if guild is not None:
        ideal_guild = tinydb.Query()
        results = central.guildDB.search(ideal_guild.id == guild.id)

        if len(results) > 0:
            if "brackets" in results[0]:
                return results[0]["brackets"]

        return central.brackets
