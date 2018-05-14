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

import json


class Language:
    def __init__(self, name, object_name, raw_object, default_version):
        self.name = name
        self.object_name = object_name
        self.raw_object = raw_object
        self.default_version = default_version

    def to_object(self):
        return {
            "name": self.name,
            "object_name": self.object_name,
            "raw_object": self.raw_object,
            "default_version": self.default_version
        }

    def to_string(self):
        return json.dumps(self.to_object())
