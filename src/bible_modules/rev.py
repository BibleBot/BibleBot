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
import requests
from bs4 import BeautifulSoup
import bible_modules.bibleutils as bibleutils
import re
import logging
from http.client import HTTPConnection
HTTPConnection.debuglevel = 0

logging.getLogger("requests").setLevel(logging.WARNING)
logging.getLogger("urllib3").setLevel(logging.WARNING)
logging.getLogger("urllib3.connectionpool").setLevel(logging.WARNING)


def getResult(query, version, verseNumbers):
    split = query.split(":")
    book = split[0].split(" ")[0]
    chapter = split[0].split(" ")[1]
    startingVerse = split[1].split("-")[0]
    endingVerse = 0

    if len(split[1].split("-")) > 1:
        endingVerse = split[1].split("-")[1]

    url = "https://www.revisedenglishversion.com/" + \
        book + "/" + chapter + "/"

    resp = requests.get(url)

    # i could've decided to modify the html,
    # but for the sake of not having to make another
    # variable, i decided not to
    if resp is not None:
        soup = BeautifulSoup(resp.text, "html.parser")

        for container in soup.findAll(True, {"class": "col1container"}):
            text = ""

            for num in container.findAll(True, {"class": ["versenum",
                                                          "versenumcomm"]}):
                num.replaceWith("[" + num.getText() + "] ")

            for meta in container.findAll(True, {"class": "fnmark"}):
                meta.decompose()

            if startingVerse > endingVerse:
                for heading in container.findAll(True,
                                                 {"class": ["heading",
                                                            "headingfirst"]}):
                    heading.string.replaceWith("")

                text = " [" + startingVerse + "]" + \
                    container.getText().split(
                        "[" + str(int(startingVerse) + 1) +
                        "]")[0].split("[" + startingVerse + "]")[1]

                text = re.sub(r"(\r\n|\n|\r)", " ", text,
                              0, re.MULTILINE)[1:-1]
            else:

                for heading in container.findAll(True,
                                                 {"class": ["heading",
                                                            "headingfirst"]}):
                    heading.string.replaceWith("")

                text = " [" + startingVerse + "]" + \
                    container.getText().split(
                        "[" + str(int(endingVerse) + 1) +
                        "]")[0].split("[" + startingVerse + "]")[1]

                text = re.sub(r"(\r\n|\n|\r)", " ", text,
                              0, re.MULTILINE)[1:-1]

            if verseNumbers == "disable":
                text = re.sub(r".?\[[0-9]\]", "")

            verseObject = {
                "passage": query,
                "version": "Revised English Version (REV)",
                "text": bibleutils.purifyText(text)
            }

            return verseObject
