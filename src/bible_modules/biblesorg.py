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
import os
import sys
import configparser
from http.client import HTTPConnection

import requests
from bs4 import BeautifulSoup

import bible_modules.bibleutils as bibleutils

dir_path = os.path.dirname(os.path.realpath(__file__))
sys.path.append(dir_path + "/..")

config = configparser.ConfigParser()
config.read(dir_path + "/../config.ini")

HTTPConnection.debuglevel = 0

logging.getLogger("requests").setLevel(logging.WARNING)
logging.getLogger("urllib3").setLevel(logging.WARNING)
logging.getLogger("urllib3.connectionpool").setLevel(logging.WARNING)

versions = {
    "KJVA": "eng-KJVA"
}

version_names = {
    "KJVA": "King James Version with Apocrypha (KJVA)"
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

    query = query.replace("|", " ")

    url = "https://bibles.org/v2/passages.js?q[]=" + query + "&version=" + versions[version]

    resp = requests.get(url, auth=(config["apis"]["biblesorg"], "X"))

    if resp is not None:
        data = resp.json()
        data = data["response"]["search"]["result"]["passages"]
        text = None

        if len(data) > 0:
            text = data[0]["text"]

        if text is None:
            return

        soup = BeautifulSoup(text, "lxml")

        title = ""
        text = ""

        for heading in soup.find_all("h3"):
            title += heading.get_text() + " / "
            heading.decompose()

        for sup in soup.find_all("sup", {"class": "v"}):
            if verse_numbers == "enable":
                sup.replace_with("<" + sup.get_text() + "> ")
            else:
                sup.replace_with(" ")

        for p in soup.find_all("p", {"class": "p"}):
            text += p.get_text()

        if headings == "disable":
            title = ""

        verse_object = {
            "passage": query,
            "version": version_names[version],
            "title": title[0:-3],
            "text": bibleutils.purify_text(text)
        }

        return verse_object
