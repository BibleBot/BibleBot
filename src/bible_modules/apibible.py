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

import logging
import os
import sys
import configparser
from http.client import HTTPConnection

import aiohttp
from bs4 import BeautifulSoup

import bible_modules.bibleutils as bibleutils

dir_path = os.path.dirname(os.path.realpath(__file__))
sys.path.append(f"{dir_path}/..")

import central  # noqa: E402

config = configparser.ConfigParser()
config.read(f"{dir_path}/../config.ini")

HTTPConnection.debuglevel = 0

logging.getLogger("aiohttp").setLevel(logging.WARNING)
logging.getLogger("urllib3").setLevel(logging.WARNING)
logging.getLogger("urllib3.connectionpool").setLevel(logging.WARNING)

versions = {
    "KJVA": "de4e12af7f28f599-01",
}

version_names = {
    "KJVA": "King James Version with Apocrypha (KJVA)",
}

# def remove_bible_title_in_search(string):
#     return re.sub(r"<[^>]*>", "", string)


# def search(version, query):
#     query = html.escape(query)
#
#     url = "https://www.biblegateway.com/quicksearch/?search=" + query + \
#           "&version=" + version + \
#           "&searchtype=all&limit=50000&interface=print"
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


async def get_result(query, ver, headings, verse_numbers):
    query = query.replace("|", " ")

    url = f"https://api.scripture.api.bible/v1/bibles/{versions[ver]}/search"
    headers = {"api-key": config["apis"]["apibible"]}
    params = {"query": query, "limit": "1"}

    async with aiohttp.ClientSession() as session:
        async with session.get(url, params=params, headers=headers) as resp:
            if resp is not None:
                data = await resp.json()
                data = data["data"]["passages"]
                text = None

                if data[0]["bibleId"] != versions[ver]:
                    central.log_message("err", 0, "apibible", "global",
                                        f"{version} is no longer able to be used.")
                    return

                if len(data) > 0:
                    text = data[0]["content"]

                if text is None:
                    return

                soup = BeautifulSoup(text, "lxml")

                title = ""
                text = ""

                for heading in soup.find_all("h3"):
                    title += f"{heading.get_text()} / "
                    heading.decompose()

                for span in soup.find_all("span", {"class": "v"}):
                    if verse_numbers == "enable":
                        span.replace_with(f"<**{span.get_text()}**> ")
                    else:
                        span.replace_with(" ")

                    span.decompose()

                for p in soup.find_all("p", {"class": "p"}):
                    text += p.get_text()

                if headings == "disable":
                    title = ""

                text = f" {bibleutils.purify_text(text).rstrip()}"

                verse_object = {
                    "passage": query,
                    "version": version_names[ver],
                    "title": title[0:-3],
                    "text": text
                }

                return verse_object
