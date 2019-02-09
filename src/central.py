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

import configparser
import math
import os
import time
import numbers

import tinydb

from data import languages
from extensions.vylogger import VyLogger

dir_path = os.path.dirname(os.path.realpath(__file__))

config = configparser.ConfigParser()
config.read(f"{dir_path}/config.ini")

configVersion = configparser.ConfigParser()
configVersion.read(f"{dir_path}/config.example.ini")

version = configVersion["meta"]["version"]
icon = "https://cdn.discordapp.com/avatars/361033318273384449/cc2758488d104770c9630e4c21ad1e4a.png"  # noqa: E501
cmd_prefix = config["BibleBot"]["commandPrefix"]

logger = VyLogger("default")

db = tinydb.TinyDB(f"{dir_path}/../databases/db")
guildDB = tinydb.TinyDB(f"{dir_path}/../databases/guilddb")
versionDB = tinydb.TinyDB(f"{dir_path}/../databases/versiondb")
optoutDB = tinydb.TinyDB(f"{dir_path}/../databases/optoutdb")

languages = languages

brackets = {
    "first": config["BibleBot"]["dividingBrackets"][0],
    "second": config["BibleBot"]["dividingBrackets"][1]
}


def capitalize_first_letter(string):
    return string[0].upper() + string[1:]


def halve_string(s):
    """
    This function works by finding the space nearest the middle
    of 's' and cutting the string into two separate strings on that space.
    :param s: string; The input.
    :return: dict; A dict with the first half at dict["first"] and the second at dict["second"].
    """

    middle = math.floor(len(s) / 2)
    before = s.rfind(" ", middle)
    after = s.index(" ", middle + 1)

    if (middle - before) < (after - middle):
        middle = after
    else:
        middle = before

    return {
        "first": s[0:middle],
        "second": s[middle + 1:]
    }


def log_message(level, shard, sender, source, msg):
    message = f"[shard {str(shard)}] <{sender}@{source}> {msg}"

    if level == "warn":
        logger.warning(message)
    elif level == "err":
        logger.error(message)
    elif level == "info":
        logger.info(message)
    elif level == "debug":
        logger.debug(message)


def get_raw_language(obj):
    return getattr(languages, obj).raw_object


def add_optout(entryid):
    ideal_entry = tinydb.Query()
    result = optoutDB.search(ideal_entry.id == entryid)

    if len(result) > 0:
        return False
    else:
        db.remove(ideal_entry.id == entryid)
        guildDB.remove(ideal_entry.id == entryid)

        optoutDB.insert({"id": entryid})
        return True


def remove_optout(entryid):
    ideal_entry = tinydb.Query()
    result = optoutDB.search(ideal_entry.id == entryid)

    if len(result) > 0:
        optoutDB.remove(ideal_entry.id == entryid)
        return True
    else:
        return False


def is_optout(entryid):
    ideal_entry = tinydb.Query()
    result = optoutDB.search(ideal_entry.id == entryid)

    if len(result) > 0:
        return True
    else:
        return False


def is_snowflake(snowflake):
    if isinstance(snowflake, int):
        if snowflake > 1.4200704e+16:
            return True
    else:
        try:
            if isinstance(int(snowflake), int):
                if int(snowflake) > 1.4200704e+16:
                    return True
        except ValueError:
            return False


def sleep(milliseconds):
    time.sleep(milliseconds / 1000.0)
