'''
    Copyright (c) 2018 BibleBot <vypr [at] vypr [dot] space>
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

import tinydb

dir_path = os.path.dirname(os.path.realpath(__file__))
sys.path.append(dir_path + "/../../..")

import central  # noqa: E402


def setHeadings(user, headings):
    headings = headings.lower()

    if headings != "enable" and headings != "disable":
        return False

    idealUser = tinydb.Query()
    results = central.db.search(idealUser.id == user.id)

    if len(results) > 0:
        central.db.update({"headings": headings}, idealUser.id == user.id)
    else:
        central.db.insert({"id": user.id, "headings": headings})

    return True


def getHeadings(user):
    idealUser = tinydb.Query()
    results = central.db.search(idealUser.id == user.id)

    if len(results) > 0:
        if "headings" in results[0]:
            return results[0]["headings"]
        else:
            return "enable"
    else:
        return "enable"


def setVerseNumbers(user, verseNumbers):
    verseNumbers = verseNumbers.lower()

    if verseNumbers != "enable" and verseNumbers != "disable":
        return False

    idealUser = tinydb.Query()
    results = central.db.search(idealUser.id == user.id)

    if len(results) > 0:
        central.db.update({"verseNumbers": verseNumbers},
                          idealUser.id == user.id)
    else:
        central.db.insert({"id": user.id, "verseNumbers": verseNumbers})

    return True


def getVerseNumbers(user):
    idealUser = tinydb.Query()
    results = central.db.search(idealUser.id == user.id)

    if len(results) > 0:
        if "verseNumbers" in results[0]:
            return results[0]["verseNumbers"]
        else:
            return "enable"
    else:
        return "enable"
