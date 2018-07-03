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
import logging
import re
from http.client import HTTPConnection

import requests
from bs4 import BeautifulSoup

import bible_modules.bibleutils as bibleutils

HTTPConnection.debuglevel = 0

logging.getLogger("requests").setLevel(logging.WARNING)
logging.getLogger("urllib3").setLevel(logging.WARNING)
logging.getLogger("urllib3.connectionpool").setLevel(logging.WARNING)


def get_result(query, verse_numbers):
    if ":" not in query:
        book = query.split(" ")[0]
        chapter = query.split(" ")[1]
        starting_verse = "1"
        ending_verse = "5"

        unversed_books = ["Obadiah", "Philemon", "2 John", "3 John", "Jude"]
        is_unversed = False

        query = book + " " + chapter

        for i in unversed_books:
            if i in query:
                is_unversed = True

        if is_unversed:
            query += ":" + starting_verse
        else:
            query += ":" + starting_verse + "-" + ending_verse

    url = "https://www.revisedenglishversion.com/" + book + "/" + chapter + "/"
    resp = requests.get(url)

    # i could've decided to modify the html,
    # but for the sake of not having to make another
    # variable, i decided not to
    if resp is not None:
        soup = BeautifulSoup(resp.text, "lxml")

        for container in soup.find_all(True, {"class": "col1container"}):
            for num in container.find_all(True, {"class": ["versenum", "versenumcomm"]}):
                num.replace_with("[" + num.get_text() + "] ")

            for meta in container.find_all(True, {"class": "fnmark"}):
                meta.decompose()

            if starting_verse > ending_verse:
                for heading in container.find_all(True, {"class": ["heading", "headingfirst"]}):
                    heading.string.replace_with("")

                text = " [" + starting_verse + "]" + container.get_text().split(
                    "[" + str(int(starting_verse) + 1) + "]")[0].split("[" + starting_verse + "]")[1]

                text = re.sub(r"(\r\n|\n|\r)", " ", text, 0, re.MULTILINE)[1:-1]
            else:
                for heading in container.find_all(True, {"class": ["heading", "headingfirst"]}):
                    heading.string.replace_with("")

                text = " [" + starting_verse + "]" + container.get_text().split(
                    "[" + str(int(ending_verse) + 1) + "]")[0].split("[" + starting_verse + "]")[1]

                text = re.sub(r"(\r\n|\n|\r)", " ", text, 0, re.MULTILINE)[1:-1]

            if verse_numbers == "disable":
                text = re.sub(r".?\[[0-9]\]", "", text)

            verse_object = {
                "passage": query,
                "version": "Revised English Version (REV)",
                "text": bibleutils.purify_text(text)
            }

            return verse_object
