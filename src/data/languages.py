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

import json as __json
import os as __os
import sys as __sys

__dir_path = __os.path.dirname(__os.path.realpath(__file__))
__sys.path.append(__dir_path + "/..")

from vytypes.language import Language as __Language  # noqa: E402

# __defaultLang = open(__dir_path + "/../../i18n/default/default.json")
# __defaultLang = __json.loads(__defaultLang.read())

__english_us = open(__dir_path + "/../../i18n/english_us/english_us.json")
__english_us = __json.loads(__english_us.read())

__english_uk = open(__dir_path + "/../../i18n/english_uk/english_uk.json")
__english_uk = __json.loads(__english_uk.read())

__esperanto = open(__dir_path + "/../../i18n/esperanto/esperanto.json")
__esperanto = __json.loads(__esperanto.read())

# __french = open(__dir_path + "/../../i18n/french/french.json")
# __french = __json.loads(__french.read())

# __french_qc = open(__dir_path + "/../../i18n/french_qc/french_qc.json")
# __french_qc = __json.loads(__french_qc.read())

# __greek = open(__dir_path + "/../../i18n/greek/greek.json")
# __greek = __json.loads(__greek.read())

# __welsh = open(__dir_path + "/../../i18n/welsh/welsh.json")
# __welsh = __json.loads(__welsh.read())

# default = __Language("Default", "default", __defaultLang, "NRSV")
english_us = __Language("English (US)", "english_us", __english_us, "NRSV")
english_uk = __Language("English (UK)", "english_uk", __english_uk, "NRSVA")
esperanto = __Language("Esperanto", "esperanto", __esperanto, "NRSV")
# french = __Language("French", "french", __french, "BDS")
# french_qc = __Language("French (QC)", "french_qc", __french_qc, "BDS")
# greek = __Language("Greek", "greek", __greek, "NRSV")
# welsh = __Language("Welsh", "welsh", __welsh, "BWM")
