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

import math
import os
import random
import sys

import tinydb

from handlers.logic.verses import utils
from handlers.logic.settings import versions, formatting
from name_scraper import client

__dir_path = os.path.dirname(os.path.realpath(__file__))
sys.path.append(f"{__dir_path}/..")

from bible_modules import apibible, bibleserver, biblehub, biblegateway  # noqa: E402

import central  # noqa: E402

books = client.get_books()


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

                        if version is None or version == "REV":
                            version = "RSV"

                ideal_version = tinydb.Query()
                results = central.versionDB.search(ideal_version.abbv == version)

                if len(results) > 0:
                    for verse in verses:
                        for section in ["ot", "nt", "deu"]:
                            support = utils.check_section_support(results[0], verse, reference, section, lang)

                            if "ok" not in support.keys():
                                return [support]

                    biblehub_versions = ["BSB", "NHEB", "WBT"]
                    bibleserver_versions = ["LUT", "LXX", "SLT"]
                    apibible_versions = ["KJVA"]

                    non_bible_gateway = biblehub_versions + apibible_versions + bibleserver_versions

                    if version not in non_bible_gateway:
                        result = biblegateway.get_result(reference, version, headings, verse_numbers)
                        return_list.append(utils.process_result(result, reference, version, lang))
                    elif version in apibible_versions:
                        result = apibible.get_result(reference, version, headings, verse_numbers)
                        return_list.append(utils.process_result(result, reference, version, lang))
                    elif version in biblehub_versions:
                        result = biblehub.get_result(reference, version, verse_numbers)
                        return_list.append(utils.process_result(result, reference, version, lang))
                    elif version in bibleserver_versions:
                        result = bibleserver.get_result(reference, version, verse_numbers)
                        return_list.append(utils.process_result(result, reference, version, lang))

            return return_list
