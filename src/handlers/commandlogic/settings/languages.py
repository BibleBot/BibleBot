'''
    Copyright (c) 2018 Elliott Pardee <vypr [at] vypr [dot] space>
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


def setLanguage(user, language):
    if getattr(central.languages, language) is not None:
        idealUser = tinydb.Query()
        userResult = central.db.search(idealUser.id == user.id)

        if len(userResult) > 0:
            central.db.update({"id": user.id}, idealUser.language == language)
        else:
            central.db.insert({"id": user.id, "language": language})

        return True
    else:
        return False


def getLanguage(user):
    idealUser = tinydb.Query()
    results = central.db.search(idealUser.id == user.id)

    if len(results) > 0:
        return results[0].language
    else:
        return central.languages.default.objectName


def getLanguages():
    languages = []

    for lang in [a for a in dir(central.languages) if not a.startswith('__')]:
        name = getattr(central.languages, lang).name
        objectName = getattr(central.languages, lang).objectName

        languages.append({"name": name, "objectName": objectName})

    return languages
