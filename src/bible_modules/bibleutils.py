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

import re

import requests
from bs4 import BeautifulSoup


def purify_text(text):
    result = text.replace("“", " \"")
    result = result.replace("[", " <")
    result = result.replace("]", "> ")
    result = result.replace("”", "\" ")
    result = result.replace("‘", "'")
    result = result.replace("’", "'")
    result = result.replace(",", ", ")
    result = result.replace(".", ". ")
    result = result.replace(". \"", ".\"")
    result = result.replace(". '", ".'")
    result = result.replace(" .", ".")
    result = result.replace(", \"", ",\"")
    result = result.replace(", '", ",'")
    result = result.replace("!", "! ")
    result = result.replace("! \"", "!\"")
    result = result.replace("! '", "!'")
    result = result.replace("?", "? ")
    result = result.replace("? \"", "?\"")
    result = result.replace("? '", "?'")
    result = result.replace(":", ": ")
    return re.sub(r"\s+", " ", result)


def get_random_verse():
    url = "https://dailyverses.net/random-bible-verse"

    resp = requests.get(url)

    if resp is not None:
        soup = BeautifulSoup(resp.text, "html.parser")
        verse = soup.find("div", {"class": "bibleChapter"}).find("a").get_text()

        return verse


def get_votd():
    url = "https://www.biblegateway.com/reading-plans/verse-of-the-day/next"

    resp = requests.get(url)

    if resp is not None:
        soup = BeautifulSoup(resp.text, "html.parser")
        verse = soup.find(True, {"class": "rp-passage-display"}).getText()

        return verse
