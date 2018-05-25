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

import json
import math
import os
import random
import sys

import tinydb

from handlers.verselogic import utils

__dir_path = os.path.dirname(os.path.realpath(__file__))
sys.path.append(__dir_path + "/..")

import handlers.commandlogic.settings as settings  # noqa: E402
from bible_modules import biblegateway as biblegateway  # noqa: E402
from bible_modules import rev as rev  # noqa: E402
from data.BGBookNames.books import itemToBook  # noqa: E402
import central  # noqa: E402

books = open(__dir_path + "/../data/BGBookNames/books.json")
books = json.loads(books.read())

'''
TODO: I'm expecting the formula to go something like this:
1. Check book name.
2. Iterate through each key in books to see if book name is in there.
3. If so, grab the value in books[key].
4. Do the thing with parseSpacedBooks.
5. ???
6. Profit?
'''


class VerseHandler:
    @classmethod
    def process_raw_message(cls, raw_message, sender, lang, guild):
        lang = getattr(central.languages, lang).raw_object
        available_versions = settings.versions.get_versions_by_acronym()
        brackets = settings.formatting.get_guild_brackets(guild)
        msg = raw_message.content

        if brackets is None:
            brackets = central.config["BibleBot"]["dividingBrackets"]

        try:
            brackets["first"] = brackets["first"]
            brackets["second"] = brackets["second"]
        except TypeError:
            brackets = {
                "first": central.config["BibleBot"]["dividingBrackets"][0],
                "second": central.config["BibleBot"]["dividingBrackets"][1]
            }

        if " " in msg:
            verses = []

            msg = utils.purify(msg)

            results = utils.get_books(msg)
            results.sort(key=lambda item: item[1])  # sort the results based on the index that they were found

            for book, index in results:
                verse = utils.create_verse_object(book, index, msg, available_versions, brackets)

                if verse != "invalid" and verse is not None:
                    verses.append(verse)

            if len(verses) > 6:
                responses = ["spamming me, really?", "no spam pls",
                             "be nice to me", "such verses, many spam",
                             "＼(º □ º l|l)/ SO MANY VERSES",
                             "don't spam me, i'm a good bot",
                             "hey buddy, get your own bot to spam"]

                random_index = int(math.floor(random.random() * len(responses)))
                return [{"spam": responses[random_index]}]

            references = []

            for i, verse in enumerate(verses):
                reference = utils.create_reference_string(verse)

                if reference is not None:
                    references.append(reference)

            return_list = []

            for reference in references:
                version = settings.versions.get_version(sender)

                if version is None:
                    version = settings.versions.get_guild_version(guild)

                    if version is None or version == "REV":
                        version = "NRSV"

                headings = settings.formatting.get_headings(sender)
                verse_numbers = settings.formatting.get_verse_numbers(sender)

                ref_split = reference.split(" | v: ")

                if len(ref_split) == 2:
                    reference = ref_split[0]
                    version = ref_split[1]

                ideal_version = tinydb.Query()
                results = central.versionDB.search(ideal_version.abbv == version)

                if len(results) > 0:
                    for verse in verses:
                        is_ot = False
                        is_nt = False
                        is_deu = False

                        for index in itemToBook["ot"]:
                            if index == verse["book"]:
                                is_ot = True

                            if not results[0]["hasOT"] and is_ot:
                                response = lang["otnotsupported"]
                                response = response.replace("<version>", results[0]["name"])

                                response2 = lang["otnotsupported2"]
                                response2 = response2.replace("<setversion>", lang["commands"]["setversion"])

                                return [{
                                    "level": "err",
                                    "twoMessages": True,
                                    "reference": reference + " " + version,
                                    "firstMessage": response,
                                    "secondMessage": response2
                                }]

                        for index in itemToBook["nt"]:
                            if index == verse["book"]:
                                is_nt = True

                            if not results[0]["hasNT"] and is_nt:
                                response = lang["ntnotsupported"]
                                response = response.replace("<version>", results[0]["name"])

                                response2 = lang.rawObject["ntnotsupported2"]
                                response2 = response2.replace("<setversion>", lang["commands"]["setversion"])

                                return [{
                                    "level": "err",
                                    "twoMessages": True,
                                    "reference": reference + " " + version,
                                    "firstMessage": response,
                                    "secondMessage": response2
                                }]

                        for index in itemToBook["deu"]:
                            if index == verse["book"]:
                                is_deu = True

                            if not results[0]["hasDEU"] and is_deu:
                                response = lang["deunotsupported"]
                                response = response.replace("<version>", results[0]["name"])

                                response2 = lang["deunotsupported2"]
                                response2 = response2.replace("<setversion>", lang["commands"]["setversion"])

                                return [{
                                    "level": "err",
                                    "twoMessages": True,
                                    "reference": reference + " " + version,
                                    "firstMessage": response,
                                    "secondMessage": response2
                                }]

                    if version != "REV":
                        result = biblegateway.get_result(reference, version, headings, verse_numbers)

                        if result is not None:
                            if result["text"][0] != " ":
                                result["text"] = " " + result["text"]

                            content = "```Dust\n" + result["title"] + "\n\n" + result["text"] + "```"
                            response_string = "**" + result["passage"] + " - " + result["version"] + "**\n\n" + content

                            if len(response_string) < 2000:
                                return_list.append({
                                    "level": "info",
                                    "reference": reference + " " + version,
                                    "message": response_string
                                })
                            elif len(response_string) > 2000:
                                if len(response_string) < 3500:
                                    split_text = central.splitter(result["text"])

                                    content1 = "```Dust\n" + result["title"] + "\n\n" + split_text["first"] + "```"
                                    response_string1 = "**" + result["passage"] + " - " + result["version"] + "**" + \
                                                       "\n\n" + content1

                                    content2 = "```Dust\n" + split_text["second"] + "```"

                                    return_list.append({
                                        "level": "info",
                                        "twoMessages": True,
                                        "reference": reference + " " + version,
                                        "firstMessage": response_string1,
                                        "secondMessage": content2
                                    })
                                else:
                                    return_list.append({
                                        "level": "err",
                                        "reference": reference + " " + version,
                                        "message": lang["passagetoolong"]
                                    })
                    else:
                        result = rev.get_result(reference, verse_numbers)

                        if result["text"][0] != " ":
                            result["text"] = " " + result["text"]

                        content = "```Dust\n" + result["text"] + "```"
                        response_string = "**" + result["passage"] + " - " + result["version"] + "**\n\n" + content

                        if len(response_string) < 2000:
                            return_list.append({
                                "level": "info",
                                "reference": reference + " " + version,
                                "message": response_string
                            })
                        elif len(response_string) > 2000:
                            if len(response_string) < 3500:
                                split_text = central.splitter(result["text"])

                                content1 = "```Dust\n" + split_text["first"] + "```"
                                response_string1 = "**" + result["passage"] + " - " + result["version"] + "**" + \
                                                   "\n\n" + content1

                                content2 = "```Dust\n" + split_text["second"] + "```"

                                return_list.append({
                                    "level": "info",
                                    "twoMessages": True,
                                    "reference": reference + " " + version,
                                    "firstMessage": response_string1,
                                    "secondMessage": content2
                                })
                            else:
                                return_list.append({
                                    "level": "err",
                                    "reference": reference + " " + version,
                                    "message": lang["passagetoolong"]
                                })
            return return_list
