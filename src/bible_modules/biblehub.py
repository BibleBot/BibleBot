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
from http.client import HTTPConnection

import requests
from bs4 import BeautifulSoup

import bible_modules.bibleutils as bibleutils

HTTPConnection.debuglevel = 0

logging.getLogger("requests").setLevel(logging.WARNING)
logging.getLogger("urllib3").setLevel(logging.WARNING)
logging.getLogger("urllib3.connectionpool").setLevel(logging.WARNING)

version_names = {
    "BSB": "Berean Study Bible (BSB)",
    "NHEB": "New Heart Study Bible (NHEB)",
    "WBT": "Webster's Bible Translation (WBT)"
}


# def remove_bible_title_in_search(string):
#     return re.sub(r"<[^>]*>", "", string)


# def search(version, query):
#     query = html.escape(query)
#
#     url = "https://www.biblegateway.com/quicksearch/?search=" + query + \
#           "&version=" + version + "&searchtype=all&limit=50000&interface=print"
#
#     search_results = {}
#     length = 0
#
#     resp = requests.get(url)
#
#     if resp is not None:
#         soup = BeautifulSoup(resp.text, "html.parser")
#
#         for row in soup.find_all(True, {"class": "row"}):
#             result = {}
#
#             for extra in row.find_all(True, {"class": "bible-item-extras"}):
#                 extra.decompose()
#
#             result["title"] = row.find(True, {"class": "bible-item-title"})
#             result["text"] = row.find(True, {"class": "bible-item-text"})
#
#             if result["title"] is not None:
#                 if result["text"] is not None:
#                     result["title"] = result["title"].getText()
#                     result["text"] = remove_bible_title_in_search(
#                         bibleutils.purify_text(result["text"].get_text()[0:-1]))
#
#                     length += 1
#                     search_results["result" + str(length)] = result
#     return search_results


def get_result(query, version, verse_numbers):
    book = query.split("|")[0]
    chapter = query.split("|")[1].split(":")[0]
    starting_verse = query.split("|")[1].split(":")[1]
    ending_verse = starting_verse

    if "-" in starting_verse:
        temp = starting_verse.split("-")

        if len(temp[1]) != 0:
            starting_verse = temp[0]
            ending_verse = temp[1]
        else:
            starting_verse = temp[0]
            ending_verse = "-"

    url = f"http://biblehub.com/{version.lower()}/{book.lower()}/{chapter}.htm"

    resp = requests.get(url)
    soup = BeautifulSoup(resp.text, "lxml")

    text = None

    for div in soup.find_all("div", {"class": "chap"}):
        for p in div.find_all("p", {"class": "cross"}):
            p.decompose()

        for heading in div.find_all("p", {"class": "hdg"}):
            heading.decompose()

        if ending_verse == "-":
            ending_verse = div.find_all("span", {"class": "reftext"})[-1].get_text()

        for sup in div.find_all("span", {"class": "reftext"}):
            if verse_numbers == "enable":
                sup.replace_with("<" + sup.get_text() + "> ")
            else:
                sup.replace_with(" ")

        text = div.get_text()

        text = text.split(f"<{int(ending_verse) + 1}>")[0]

        if int(starting_verse) != 1:
            text = f" <{starting_verse}>" + text.split(f"<{int(starting_verse)}>")[1]

        if text is None:
            return

        verse_object = {
            "passage": query.replace("|", " "),
            "version": version_names[version],
            "title": "",
            "text": bibleutils.purify_text(text)
        }

        return verse_object

