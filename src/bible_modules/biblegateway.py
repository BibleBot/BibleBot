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

import requests
from bs4 import BeautifulSoup
import re
import cgi
import bible_modules.bibleutils as bibleutils
import logging
from http.client import HTTPConnection
HTTPConnection.debuglevel = 0

logging.getLogger("requests").setLevel(logging.WARNING)
logging.getLogger("urllib3").setLevel(logging.WARNING)
logging.getLogger("urllib3.connectionpool").setLevel(logging.WARNING)


def removeBibleTitleInSearch(string):
    return re.sub(r"<[^>]*>", "", string)


def search(version, query):
    query = cgi.escape(query)

    url = "https://www.biblegateway.com/quicksearch/?search=" + \
        query + "&version=" + \
        version + "&searchtype=all&limit=50000&interface=print"

    searchResults = {}
    length = 0

    resp = requests.get(url)

    if resp is not None:
        soup = BeautifulSoup(resp.text, "html.parser")

        for row in soup.findAll(True, {"class": "row"}):
            result = {}

            for extra in row.findAll(True, {"class": "bible-item-extras"}):
                extra.decompose()

            result["title"] = row.find(
                True, {"class": "bible-item-title"})

            result["text"] = row.find(True, {"class": "bible-item-text"})

            if result["title"] is not None:
                if result["text"] is not None:
                    result["title"] = result["title"].getText()
                    result["text"] = removeBibleTitleInSearch(
                        bibleutils.purifyText(result["text"].getText()[0:-1]))

                    length += 1
                    searchResults["result" + str(length)] = result
    return searchResults


def getResult(query, version, headings, verseNumbers):
    url = "https://www.biblegateway.com/passage/?search=" + query + \
        "&version=" + version + "&interface=print"

    resp = requests.get(url)

    if resp is not None:
        soup = BeautifulSoup(resp.text, "html.parser")

        for div in soup.findAll("div", {"class": "result-text-style-normal"}):
            title = ""

            if headings == "disable":
                for heading in div.findAll("h3"):
                    heading.decompose()

                for heading in div.findAll(True, {"class": "inline-h3"}):
                    heading.decompose()
            else:
                for heading in div.findAll("h3"):
                    title += heading.getText() + " / "

            if verseNumbers == "disable":
                for num in div.findAll(True, {"class": ["chapternum",
                                                        "versenum"]}):
                    num.string.replaceWith(" ")
            else:
                for num in div.findAll(True, {"class": ["chapternum",
                                                        "versenum"]}):
                    num.string.replaceWith("<" + num.string[0:-1] + "> ")

            for meta in div.findAll(True, {"class": ["crossreference",
                                                     "footnote"]}):
                meta.decompose()

            text = ""
            for paragraph in div.findAll("p"):
                text += paragraph.getText()

            verseObject = {
                "passage": div.find(True, {"class":
                                           "passage-display-bcv"}).string,
                "version": div.find(True, {"class":
                                           "passage-display-version"}).string,
                "title": title[0:-3],
                "text": bibleutils.purifyText(text)
            }

            return verseObject
