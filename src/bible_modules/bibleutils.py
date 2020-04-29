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

import re

import aiohttp
from bs4 import BeautifulSoup

import quantumrandom

def remove_html(text):
    return re.sub(r"<[^<]+?>", "", text)


def purify_text(text):
    result = text.replace("“", "\"")
    result = result.replace("[", " <")
    result = result.replace("]", "> ")
    result = result.replace("”", "\"")
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
    result = result.replace(";", "; ")
    result = result.replace("¶ ", "")
    result = result.replace("â", "\"")  # biblehub beginning smart quote
    result = result.replace(" â", "\"")  # biblehub ending smart quote
    result = result.replace("â", "-")  # biblehub dash unicode
    return re.sub(r"\s+", " ", result)

@staticmethod
async def get_quantum_random_verse():
    return 3

async def get_random_verse():
    url = "https://dailyverses.net/random-bible-verse"

    async with aiohttp.ClientSession() as session:
        async with session.get(url) as resp:
            if resp is not None:
                soup = BeautifulSoup(await resp.text(), "lxml")
                verse = soup.find(
                    "div", {"class": "bibleChapter"}).find("a").get_text()

                return verse


async def get_votd():
    url = "https://www.biblegateway.com/reading-plans/verse-of-the-day/next"

    async with aiohttp.ClientSession() as session:
        async with session.get(url) as resp:
            if resp is not None:
                soup = BeautifulSoup(await resp.text(), "lxml")
                verse = soup.find(True, {"class": "rp-passage-display"}).get_text()

                return verse
