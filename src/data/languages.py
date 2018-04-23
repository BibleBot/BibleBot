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

import os as __os
import sys as __sys
import json as __json

__dir_path = __os.path.dirname(__os.path.realpath(__file__))
__sys.path.append(__dir_path + "/../vytypes")

from language import Language as __Language  # noqa: E402

__defaultLang = open(__dir_path + "/../../i18n/default/default.json")
__defaultLang = __json.loads(__defaultLang.read())

default = __Language("Default", "default", __defaultLang, "NRSV")
