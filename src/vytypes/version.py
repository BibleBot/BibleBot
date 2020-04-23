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

import json


class Version:
    def __init__(self, name, abbv, has_ot, has_nt, has_deu):
        self.name = name
        self.abbv = abbv

        self.has_ot = has_ot
        self.has_nt = has_nt
        self.has_deu = has_deu

    def to_object(self):
        return {
            "name": self.name,
            "abbv": self.abbv,
            "hasOT": self.has_ot,
            "hasNT": self.has_nt,
            "hasDEU": self.has_deu
        }

    def to_string(self):
        return json.dumps(self.to_object())
