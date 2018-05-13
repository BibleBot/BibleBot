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

dir_path = os.path.dirname(os.path.realpath(__file__))
sys.path.append(dir_path + "/../../..")

import central  # noqa: E402


def setVersion(user, version):
    version = version.upper()

    idealVersion = tinydb.Query()
    versionResults = central.versionDB.search(idealVersion.abbv == version)

    if len(versionResults) > 0:
        idealUser = tinydb.Query()
        results = central.db.search(idealUser.id == user.id)

        if len(results) > 0:
            central.db.update({"version": version}, idealUser.id == user.id)
        else:
            central.db.insert({"id": user.id, "version": version})

        return True
    else:
        return False


def getVersion(user):
    idealUser = tinydb.Query()
    results = central.db.search(idealUser.id == user.id)

    if len(results) > 0:
        if "version" in results[0]:
            return results[0]["version"]
    else:
        return None


def getVersions():
    results = central.versionDB.all()
    versions = []

    for result in results:
        versions.append(result["name"])

    return sorted(versions)


def getVersionsByAcronym():
    results = central.versionDB.all()
    versions = []

    for result in results:
        versions.append(result["abbv"])

    return sorted(versions)
