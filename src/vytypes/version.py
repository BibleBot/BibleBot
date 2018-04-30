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


class Version:
    def __init__(self, name, abbv, hasOT, hasNT, hasDEU):
        self.name = name
        self.abbv = abbv

        self.hasOT = False
        self.hasNT = False
        self.hasDEU = False

        if hasOT == "yes":
            self.hasOT = True

        if hasNT == "yes":
            self.hasNT = True

        if hasDEU == "yes":
            self.hasDEU = True

    def toObject(self):
        return {
            "name": self.name,
            "abbv": self.abbv,
            "hasOT": self.hasOT,
            "hasNT": self.hasNT,
            "hasDEU": self.hasDEU
        }

    def toString(self):
        return json.dumps(self.toObject())
