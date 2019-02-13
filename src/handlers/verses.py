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

import json
import math
import os
import random
import sys

import tinydb

from handlers.logic.verses import utils
from handlers.logic.settings import versions, formatting

__dir_path = os.path.dirname(os.path.realpath(__file__))
sys.path.append(f"{__dir_path}/..")

from bible_modules import apibible, bibleserver, biblehub, biblegateway, rev  # noqa: E402
from data.BGBookNames.books import item_to_book  # noqa: E402
import central  # noqa: E402

books = open(f"{__dir_path}/../data/BGBookNames/books.json")
books = json.loads(books.read())


class VerseHandler:
    @classmethod
    def process_raw_message(cls, raw_message, sender, lang, guild):
        available_versions = versions.get_versions_by_acronym()
        brackets = formatting.get_guild_brackets(guild)
        msg = raw_message.content
        msg = " ".join(msg.splitlines())

        if brackets is None:
            brackets = central.brackets

        if " " in msg:
            verses = []

            msg = utils.purify(msg.title())

            results = utils.get_books(msg)
            results.sort(key=lambda item: item[1])  # sort the results based on the index that they were found

            for book, index in results:
                verse = utils.create_verse_object(book, index, msg, available_versions, brackets)

                if verse != "invalid" and verse is not None:
                    skip = False

                    for key, val in verse.items():
                        if key is "book" or key is "chapter":
                            if val is None or val == "None":
                                skip = True

                    if not skip:
                        verses.append(verse)

            if len(verses) > 6:
                responses = ["spamming me, really?", "no spam pls",
                             "be nice to me", "such verses, many spam",
                             "＼(º □ º l|l)/ SO MANY VERSES",
                             "don't spam me, i'm a good bot",
                             "hey buddy, get your own bot to spam",
                             "i'm a little robot, short and stout\n"
                             "stop spamming me or i'll claw your eyes out!"]

                random_index = int(math.floor(random.random() * len(responses)))
                return [{"spam": responses[random_index]}]

            references = []

            for i, verse in enumerate(verses):
                reference = utils.create_reference_string(verse)

                if reference is not None:
                    references.append(reference)

            return_list = []

            for reference in references:
                version = versions.get_version(sender)

                if version is None:
                    version = versions.get_guild_version(guild)

                    if version is None:
                        version = "RSV"

                headings = formatting.get_headings(sender)
                verse_numbers = formatting.get_verse_numbers(sender)

                ref_split = reference.split(" | v: ")

                if len(ref_split) == 2:
                    reference = ref_split[0]
                    version = ref_split[1]

                if version == "REV":
                    version = versions.get_version(sender)

                    if version is None:
                        version = versions.get_guild_version(guild)

                        if version is None:
                            version = "RSV"

                ideal_version = tinydb.Query()
                results = central.versionDB.search(ideal_version.abbv == version)

                if len(results) > 0:
                    for verse in verses:
                        for index in item_to_book["ot"]:
                            is_ot = (index == verse["book"])

                            if not results[0]["hasOT"] and is_ot:
                                response = lang["otnotsupported"]
                                response = response.replace("<version>", results[0]["name"])

                                response2 = lang["otnotsupported2"]
                                response2 = response2.replace("<setversion>", lang["commands"]["setversion"])

                                reference = reference.replace("|", " ")

                                return [{
                                    "level": "err",
                                    "twoMessages": True,
                                    "reference": f"{reference} {version}",
                                    "firstMessage": response,
                                    "secondMessage": response2
                                }]

                        for index in item_to_book["nt"]:
                            is_nt = (index == verse["book"])

                            if not results[0]["hasNT"] and is_nt:
                                response = lang["ntnotsupported"]
                                response = response.replace("<version>", results[0]["name"])

                                response2 = lang["ntnotsupported2"]
                                response2 = response2.replace("<setversion>", lang["commands"]["setversion"])

                                reference = reference.replace("|", " ")

                                return [{
                                    "level": "err",
                                    "twoMessages": True,
                                    "reference": f"{reference} {version}",
                                    "firstMessage": response,
                                    "secondMessage": response2
                                }]

                        for index in item_to_book["deu"]:
                            is_deu = (index == verse["book"])

                            if not results[0]["hasDEU"] and is_deu:
                                response = lang["deunotsupported"]
                                response = response.replace("<version>", results[0]["name"])

                                response2 = lang["deunotsupported2"]
                                response2 = response2.replace("<setversion>", lang["commands"]["setversion"])

                                reference = reference.replace("|", " ")

                                return [{
                                    "level": "err",
                                    "twoMessages": True,
                                    "reference": f"{reference} {version}",
                                    "firstMessage": response,
                                    "secondMessage": response2
                                }]

                    biblehub_versions = ["BSB", "NHEB", "WBT"]
                    bibleserver_versions = ["LUT", "LXX", "SLT"]
                    apibible_versions = ["KJVA"]
                    other_versions = ["REV"]
                    non_bible_gateway = other_versions + biblehub_versions + apibible_versions + bibleserver_versions

                    if version not in non_bible_gateway:
                        result = biblegateway.get_result(reference, version, headings, verse_numbers)

                        if result is not None:
                            if result["text"][0] != " ":
                                result["text"] = " " + result["text"]

                            content = "```Dust\n" + result["title"] + "\n\n" + result["text"] + "```"
                            response_string = "**" + result["passage"] + " - " + result["version"] + "**\n\n" + content

                            reference = reference.replace("|", " ")

                            if len(response_string) < 2000:
                                return_list.append({
                                    "level": "info",
                                    "reference": reference + " " + version,
                                    "message": response_string
                                })
                            elif 2000 < len(response_string) < 3500:
                                split_text = central.halve_string(result["text"])

                                content1 = "```Dust\n" + result["title"] + "\n\n" + split_text["first"] + "```"
                                response_string1 = "**" + result["passage"] + " - " + result["version"] + "**" + \
                                                   "\n\n" + content1

                                content2 = "```Dust\n" + split_text["second"] + "```"

                                return_list.append({
                                    "level": "info",
                                    "twoMessages": True,
                                    "reference": f"{reference} {version}",
                                    "firstMessage": response_string1,
                                    "secondMessage": content2
                                })
                            else:
                                return_list.append({
                                    "level": "err",
                                    "reference": f"{reference} {version}",
                                    "message": lang["passagetoolong"]
                                })
                    elif version == "REV":
                        result = rev.get_result(reference, verse_numbers)

                        if result["text"][0] != " ":
                            result["text"] = " " + result["text"]

                        content = "```Dust\n" + result["text"] + "```"
                        response_string = "**" + result["passage"] + " - " + result["version"] + "**\n\n" + content

                        reference = reference.replace("|", " ")

                        if len(response_string) < 2000:
                            return_list.append({
                                "level": "info",
                                "reference": f"{reference} {version}",
                                "message": response_string
                            })
                        elif 2000 < len(response_string) < 3500:
                            split_text = central.halve_string(result["text"])

                            content1 = "```Dust\n" + split_text["first"] + "```"
                            response_string1 = "**" + result["passage"] + " - " + result["version"] + "**" + \
                                               "\n\n" + content1

                            content2 = "```Dust\n" + split_text["second"] + "```"

                            return_list.append({
                                "level": "info",
                                "twoMessages": True,
                                "reference": f"{reference} {version}",
                                "firstMessage": response_string1,
                                "secondMessage": content2
                            })
                        else:
                            return_list.append({
                                "level": "err",
                                "reference": f"{reference} {version}",
                                "message": lang["passagetoolong"]
                            })
                    elif version in apibible_versions:
                        result = apibible.get_result(reference, version, headings, verse_numbers)

                        if result is not None:
                            content = "```Dust\n" + result["title"] + "\n\n" + result["text"] + "```"
                            response_string = "**" + result["passage"] + " - " + result["version"] + "**\n\n" + content
    
                            reference = reference.replace("|", " ")

                            if len(response_string) < 2000:
                                return_list.append({
                                    "level": "info",
                                    "reference": f"{reference} {version}",
                                    "message": response_string
                                })
                            elif len(response_string) > 2000:
                                if len(response_string) < 3500:
                                    split_text = central.halve_string(result["text"])

                                    content1 = "```Dust\n" + result["title"] + "\n\n" + split_text["first"] + "```"
                                    response_string1 = "**" + result["passage"] + " - " + result["version"] + "**" + \
                                                       "\n\n" + content1

                                    content2 = "```Dust\n" + split_text["second"] + "```"

                                    return_list.append({
                                        "level": "info",
                                        "twoMessages": True,
                                        "reference": f"{reference} {version}",
                                        "firstMessage": response_string1,
                                        "secondMessage": content2
                                    })
                                else:
                                    return_list.append({
                                        "level": "err",
                                        "reference": f"{reference} {version}",
                                        "message": lang["passagetoolong"]
                                    })
                    elif version in biblehub_versions:
                        result = biblehub.get_result(reference, version, verse_numbers)

                        if result is not None:
                            content = "```Dust\n" + result["title"] + "\n\n" + result["text"] + "```"
                            response_string = "**" + result["passage"] + " - " + result["version"] + "**\n\n" + content

                            reference = reference.replace("|", " ")

                            if len(response_string) < 2000:
                                return_list.append({
                                    "level": "info",
                                    "reference": f"{reference} {version}",
                                    "message": response_string
                                })
                            elif len(response_string) > 2000:
                                if len(response_string) < 3500:
                                    split_text = central.halve_string(result["text"])

                                    content1 = "```Dust\n" + result["title"] + "\n\n" + split_text["first"] + "```"
                                    response_string1 = "**" + result["passage"] + " - " + result["version"] + "**" + \
                                                       "\n\n" + content1

                                    content2 = "```Dust\n" + split_text["second"] + "```"

                                    return_list.append({
                                        "level": "info",
                                        "twoMessages": True,
                                        "reference": f"{reference} {version}",
                                        "firstMessage": response_string1,
                                        "secondMessage": content2
                                    })
                                else:
                                    return_list.append({
                                        "level": "err",
                                        "reference": f"{reference} {version}",
                                        "message": lang["passagetoolong"]
                                    })
                    elif version in bibleserver_versions:
                        result = bibleserver.get_result(reference, version, verse_numbers)

                        if result is not None:
                            if result["text"][0] != " ":
                                result["text"] = " " + result["text"]

                            content = "```Dust\n" + result["title"] + "\n\n" + result["text"] + "```"
                            response_string = "**" + result["passage"] + " - " + result["version"] + "**\n\n" + content

                            reference = reference.replace("|", " ")

                            if len(response_string) < 2000:
                                return_list.append({
                                    "level": "info",
                                    "reference": f"{reference} {version}",
                                    "message": response_string
                                })
                            elif len(response_string) > 2000:
                                if len(response_string) < 3500:
                                    split_text = central.halve_string(result["text"])

                                    content1 = "```Dust\n" + result["title"] + "\n\n" + split_text["first"] + "```"
                                    response_string1 = "**" + result["passage"] + " - " + result["version"] + "**" + \
                                                       "\n\n" + content1

                                    content2 = "```Dust\n" + split_text["second"] + "```"

                                    return_list.append({
                                        "level": "info",
                                        "twoMessages": True,
                                        "reference": f"{reference} {version}",
                                        "firstMessage": response_string1,
                                        "secondMessage": content2
                                    })
                                else:
                                    return_list.append({
                                        "level": "err",
                                        "reference": f"{reference} {version}",
                                        "message": lang["passagetoolong"]
                                    })

            return return_list
