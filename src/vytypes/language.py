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

import json


class Language:
    def __init__(self, name, objectName, rawObject, defaultVersion):
        self.name = name
        self.objectName = objectName
        self.rawObject = rawObject
        self.defaultVersion = defaultVersion

    def toObject(self):
        return {
            "name": self.name,
            "objectName": self.objectName,
            "rawObject": self.rawObject,
            "defaultVersion": self.defaultVersion
        }

    def toString(self):
        return json.dumps(self.toObject())
