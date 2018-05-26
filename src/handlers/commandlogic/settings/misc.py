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

import os
import sys

import tinydb
import tinydb.operations

dir_path = os.path.dirname(os.path.realpath(__file__))
sys.path.append(dir_path + "/../../..")

import central  # noqa: E402


def set_guild_votd_time(guild, channel, time):
    if len(time) != 5:
        return False

    ideal_guild = tinydb.Query()
    results = central.guildDB.search(ideal_guild.id == guild.id)

    if len(results) > 0:
        if time != "clear":
            central.guildDB.update({"time": time, "channel": channel.id, "channel_name": channel.name},
                                   ideal_guild.id == guild.id)
        else:
            central.guildDB.update(tinydb.operations.delete("time"), ideal_guild.id == guild.id)
            central.guildDB.update(tinydb.operations.delete("channel"), ideal_guild.id == guild.id)
            central.guildDB.update(tinydb.operations.delete("channel_name"), ideal_guild.id == guild.id)
    else:
        central.guildDB.insert({"id": guild.id, "time": time, "channel": channel.id, "channel_name": channel.name})

    return True


def get_guild_votd_time(guild):
    if guild is not None:
        ideal_guild = tinydb.Query()
        results = central.guildDB.search(ideal_guild.id == guild.id)

        if len(results) > 0:
            if "channel_name" in results[0] and "time" in results[0]:
                return results[0]["channel_name"], results[0]["time"]

        return None


def set_guild_announcements(guild, channel, setting):
    if setting == "enable":
        setting = True
    else:
        setting = False

    if guild is not None:
        ideal_guild = tinydb.Query()
        results = central.guildDB.search(ideal_guild.id == guild.id)

        item = {"announce": setting, "announcechannel": channel.id, "announcechannelname": channel.name}

        if len(results) > 0:
            central.guildDB.update(item, ideal_guild.id == guild.id)
        else:
            item["id"] = guild.id
            central.guildDB.insert(item)

        return True

    return False


def get_guild_announcements(guild, notice):
    if guild is not None:
        ideal_guild = tinydb.Query()
        results = central.guildDB.search(ideal_guild.id == guild.id)

        if len(results) > 0:
            if "announce" in results[0]:
                if not notice:
                    return results[0]["announcechannel"], results[0]["announce"]
                else:
                    return results[0]["announcechannelname"], results[0]["announce"]

        return None
