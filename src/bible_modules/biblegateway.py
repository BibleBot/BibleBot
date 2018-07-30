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

import html
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


def remove_bible_title_in_search(string):
    return re.sub(r"<[^>]*>", "", string)


def search(version, query):
    query = html.escape(query)

    url = "https://www.biblegateway.com/quicksearch/?search=" + query + \
          "&version=" + version + "&searchtype=all&limit=50000&interface=print"

    search_results = {}
    length = 0

    resp = requests.get(url)

    if resp is not None:
        soup = BeautifulSoup(resp.text, "html.parser")

        for row in soup.find_all(True, {"class": "row"}):
            result = {}

            for extra in row.find_all(True, {"class": "bible-item-extras"}):
                extra.decompose()

            result["title"] = row.find(True, {"class": "bible-item-title"})
            result["text"] = row.find(True, {"class": "bible-item-text"})

            if result["title"] is not None:
                if result["text"] is not None:
                    result["title"] = result["title"].getText()
                    result["text"] = remove_bible_title_in_search(
                        bibleutils.purify_text(result["text"].get_text()[0:-1]))

                    length += 1
                    search_results["result" + str(length)] = result
    return search_results


def get_result(query, version, headings, verse_numbers):
    if ":" not in query:
        book = query.split("|")[0]
        chapter = query.split("|")[1]
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

    url = "https://www.biblegateway.com/passage/?search=" + query + \
        "&version=" + version + "&interface=print"

    resp = requests.get(url)

    if resp is not None:
        soup = BeautifulSoup(resp.text, "lxml")

        for div in soup.find_all("div", {"class": "result-text-style-normal"}):
            text = ""
            title = ""

            if headings == "disable":
                for heading in div.find_all("h3"):
                    heading.decompose()

                for heading in div.find_all(True, {"class": "inline-h3"}):
                    heading.decompose()
            else:
                for heading in div.find_all("h3"):
                    title += heading.get_text() + " / "

            for note in div.find_all(True, {"class": "first-line-none"}):
                note.decompose()

            for inline in div.find_all(True, {"class": "inline-h3"}):
                inline.decompose()

            for footnote in div.find_all(True, {"class": "footnotes"}):
                footnote.decompose()

            if verse_numbers == "disable":
                for num in div.find_all(True, {"class": ["chapternum", "versenum"]}):
                    num.string = " "
            else:
                # turn all chapter numbers into "1" otherwise the verse numbers look strange
                for num in div.find_all(True, {"class": "chapternum"}):
                    num.string = "<1> "

                for num in div.find_all(True, {"class": "versenum"}):
                    num.string = "<" + num.string[0:-1] + "> "

            for meta in div.find_all(True, {"class": ["crossreference", "footnote"]}):
                meta.decompose()

            for paragraph in div.find_all("p"):
                text += paragraph.get_text()

            verse_object = {
                "passage": div.find(True, {"class": "passage-display-bcv"}).string,
                "version": div.find(True, {"class": "passage-display-version"}).string,
                "title": title[0:-3],
                "text": bibleutils.purify_text(text)
            }

            return verse_object
