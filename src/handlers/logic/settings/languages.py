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

import os
import sys

import aiotinydb
import tinydb

dir_path = os.path.dirname(os.path.realpath(__file__))
sys.path.append(f"{dir_path}/../../..")

import central  # noqa: E402


async def set_language(user, language):
    # noinspection PyBroadException
    try:
        if getattr(central.languages, language) is not None:
            ideal_user = tinydb.Query()

            async with aiotinydb.AIOTinyDB(central.db_path) as db:
                results = db.search(ideal_user.id == user.id)

                if len(results) > 0:
                    db.update({"language": language}, ideal_user.id == user.id)
                else:
                    db.insert({"id": user.id, "language": language})

            return True
    except Exception:
        return False


async def set_guild_language(guild, language):
    # noinspection PyBroadException
    try:
        if getattr(central.languages, language) is not None:
            ideal_guild = tinydb.Query()

            async with aiotinydb.AIOTinyDB(central.guildDB_path) as guildDB:
                results = guildDB.search(ideal_guild.id == guild.id)

                if len(results) > 0:
                    guildDB.update({"language": language}, ideal_guild.id == guild.id)
                else:
                    guildDB.insert({"id": guild.id, "language": language})

                return True
    except Exception:
        return False


async def get_language(user):
    ideal_user = tinydb.Query()

    async with aiotinydb.AIOTinyDB(central.db_path) as db:
        results = db.search(ideal_user.id == user.id)

        languages = get_languages()

        if len(results) > 0:
            if "language" in results[0]:
                for item in languages:
                    if item["object_name"] == results[0]["language"]:
                        if results[0]["language"] in ["english_us", "english_uk"]:
                            return "english"

                        return results[0]["language"]

        return None


async def get_guild_language(guild):
    if guild is not None:
        ideal_guild = tinydb.Query()

        async with aiotinydb.AIOTinyDB(central.guildDB_path) as guildDB:
            results = guildDB.search(ideal_guild.id == guild.id)

            if len(results) > 0:
                if "language" in results[0]:
                    return results[0]["language"]

            return "english"


def get_languages():
    languages = []

    for lang in [a for a in dir(central.languages) if not a.startswith('__')]:
        name = getattr(central.languages, lang).name
        object_name = getattr(central.languages, lang).object_name

        languages.append({"name": name, "object_name": object_name})

    return languages
