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

import tinydb

dir_path = os.path.dirname(os.path.realpath(__file__))
sys.path.append(f"{dir_path}/../../..")

import central  # noqa: E402


def set_version(user, version):
    version = version.upper()

    ideal_version = tinydb.Query()
    version_results = central.versionDB.search(ideal_version.abbv == version)

    if len(version_results) > 0:
        ideal_user = tinydb.Query()
        results = central.db.search(ideal_user.id == user.id)

        if len(results) > 0:
            central.db.update({"version": version}, ideal_user.id == user.id)
        else:
            central.db.insert({"id": user.id, "version": version})

        return True

    return False


def set_guild_version(guild, version):
    version = version.upper()

    ideal_version = tinydb.Query()
    version_results = central.versionDB.search(ideal_version.abbv == version)

    if len(version_results) > 0:
        ideal_guild = tinydb.Query()
        results = central.guildDB.search(ideal_guild.id == guild.id)

        if len(results) > 0:
            central.guildDB.update({"version": version}, ideal_guild.id == guild.id)
        else:
            central.guildDB.insert({"id": guild.id, "version": version})

        return True

    return False


def get_version(user):
    ideal_user = tinydb.Query()
    results = central.db.search(ideal_user.id == user.id)

    if len(results) > 0:
        if "version" in results[0]:
            return results[0]["version"]

    return None


def get_guild_version(guild):
    if guild is not None:
        ideal_guild = tinydb.Query()
        results = central.guildDB.search(ideal_guild.id == guild.id)

        if len(results) > 0:
            if "version" in results[0]:
                return results[0]["version"]

        return None


def get_versions():
    results = central.versionDB.all()
    versions = []

    for result in results:
        versions.append(result["name"])

    return sorted(versions)


def get_versions_by_acronym():
    results = central.versionDB.all()
    versions = []

    for result in results:
        versions.append(result["abbv"])

    return sorted(versions)
