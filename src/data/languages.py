"""
    Copyright (c) 2018-2019 Elliott Pardee <me [at] vypr [dot] xyz>
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

import json as __json
import os as __os
import sys as __sys

__dir_path = __os.path.dirname(__os.path.realpath(__file__))
__sys.path.append(__dir_path + "/..")

from vytypes.language import Language as __Language  # noqa: E402

__defaultLang = open(__dir_path + "/../../i18n/default/default.json", encoding="utf-8")
__defaultLang = __json.loads(__defaultLang.read())

# __english = open(__dir_path + "/../../i18n/english/english.json", encoding="utf-8")
# __english = __json.loads(__english_us.read())

# __esperanto = open(__dir_path + "/../../i18n/esperanto/esperanto.json", encoding="utf-8")
# __esperanto = __json.loads(__esperanto.read())

# __finnish = open(__dir_path + "/../../i18n/finnish/finnish.json", encoding="utf-8")
# __finnish = __json.loads(__finnish.read())

# __french = open(__dir_path + "/../../i18n/french/french.json", encoding="utf-8")
# __french = __json.loads(__french.read())

# __french_qc = open(__dir_path + "/../../i18n/french_qc/french_qc.json", encoding="utf-8")
# __french_qc = __json.loads(__french_qc.read())

# __greek = open(__dir_path + "/../../i18n/greek/greek.json", encoding="utf-8")
# __greek = __json.loads(__greek.read())

# __italian = open(__dir_path + "/../../i18n/italian/italian.json", encoding="utf-8")
# __italian = __json.loads(__italian.read())

# __welsh = open(__dir_path + "/../../i18n/welsh/welsh.json", encoding="utf-8")
# __welsh = __json.loads(__welsh.read())

default = __Language("Default", "default", __defaultLang, "RSV")
# english = __Language("English", "english", __english, "RSV")
# esperanto = __Language("Esperanto", "esperanto", __esperanto, "RSV")
# finnish = __Language("Finnish", "finnish", __finnish, "R1933")
# french = __Language("French", "french", __french, "BDS")
# french_qc = __Language("French (QC)", "french_qc", __french_qc, "BDS")
# greek = __Language("Greek", "greek", __greek, "RSV")
# italian = __Language("Italian", "italian", __italian, "CEI")
# welsh = __Language("Welsh", "welsh", __welsh, "BWM")
