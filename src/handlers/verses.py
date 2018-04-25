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

import os
import sys
import math
import random
import tinydb
from handlers.verselogic import utils

__dir_path = os.path.dirname(os.path.realpath(__file__))
sys.path.append(__dir_path + "/..")

from vytypes.handler import Handler  # noqa: E402
import handlers.commandlogic.settings as settings  # noqa: E402
from bible_modules import biblegateway as biblegateway  # noqa: E402
from bible_modules import rev as rev  # noqa: E402
from data import books  # noqa: E402
import central  # noqa: E402


class VerseHandler(Handler):
    def processRawMessage(self, shard, rawMessage, sender, lang):
        availableVersions = settings.versions.getVersionsByAcronym()
        msg = rawMessage.content

        if ":" in msg and " " in msg:
            split = utils.tokenize(msg)
            bookIndexes = []
            bookNames = []
            verses = {}
            verseCount = 0

            for i in range(0, len(split)):
                try:
                    split[i] = utils.purify(split[i])
                except Exception:
                    split[i] = split[i]

                split[i] = utils.parseSpacedBookName(split[i], split, i)

                book = utils.purgeBrackets(split[i])
                difference = utils.getDifference(book, split[i])

                if isinstance(books.ot[book.lower()], str):
                    bookNames.append(books.ot[book.lower()])
                    split[i] = difference + books.ot[book.lower()]
                    bookIndexes.append(i)

                if isinstance(books.nt[book.lower()], str):
                    bookNames.append(books.nt[book.lower()])
                    split[i] = difference + books.nt[book.lower()]
                    bookIndexes.append(i)

                if isinstance(books.apo[book.lower()], str):
                    bookNames.append(books.apo[book.lower()])
                    split[i] = difference + books.apo[book.lower()]
                    bookIndexes.append(i)

            for index in bookIndexes:
                verse = []
                invalid = False

                verse = utils.createVerseObject(
                    split, index, availableVersions)

                if isinstance(verse, str):
                    if verse.startswith("invalid"):
                        invalid = True
                        return {"invalid": invalid}

                if invalid is False:
                    verses[verseCount] = verse
                    verseCount += 1

                if verseCount > 6:
                    responses = ["spamming me, really?", "no spam pls",
                                 "no spam, am good bot", "be nice to me",
                                 "don't spam me, i'm a good bot",
                                 "hey buddy, get your own " + "bot to spam"]

                    randomIndex = math.floor(random.random() * 4)
                    return responses[randomIndex]

                for i in range(0, len(verses.keys())):
                    verse = verses[i]
                    reference = utils.createReferenceString(verse)

                    version = settings.versions.getVersion(sender)

                    if version is None or version is "HWP":
                        version = lang.defaultVersion

                    headings = settings.formatting.getHeadings(sender)
                    verseNumbers = settings.formatting.getVerseNumbers(sender)

                    refSplit = reference.split(" | v: ")

                    if len(refSplit) == 2:
                        reference = refSplit[0]
                        version = refSplit[1]

                    idealVersion = tinydb.Query()
                    results = central.versionDB.search(
                        idealVersion.abbv == version)

                    if len(results) > 0:
                        continueProcessing = True

                        for name in bookNames:
                            isOT = False
                            isNT = False
                            isDEU = False

                            for index in books.ot:
                                if books.ot[index] == name:
                                    isOT = True

                            if results[0].hasOT is False and isOT:
                                response = lang.otnotsupported
                                response = response.replace(
                                    "<version>", results[0].name)

                                response2 = lang.otnotsupported2
                                response2 = response2.replace(
                                    "<setversion>", lang.commands.setversion)

                                continueProcessing = False

                                return {
                                    "level": "err",
                                    "twoMessages": True,
                                    "reference": reference,
                                    "firstMessage": response,
                                    "secondMessage": response2
                                }

                            for index in books.nt:
                                if books.nt[index] == name:
                                    isNT = True

                            if results[0].hasNT is False and isNT:
                                response = lang.ntnotsupported
                                response = response.replace(
                                    "<version>", results[0].name)

                                response2 = lang.ntnotsupported2
                                response2 = response2.replace(
                                    "<setversion>", lang.commands.setversion)

                                continueProcessing = False

                                return {
                                    "level": "err",
                                    "twoMessages": True,
                                    "reference": reference,
                                    "firstMessage": response,
                                    "secondMessage": response2
                                }

                            for index in books.deu:
                                if books.deu[index] == name:
                                    isDEU = True

                            if results[0].hasDEU is False and isDEU:
                                response = lang.deunotsupported
                                response = response.replace(
                                    "<version>", results[0].name)

                                response2 = lang.deunotsupported2
                                response2 = response2.replace(
                                    "<setversion>", lang.commands.setversion)

                                continueProcessing = False

                                return {
                                    "level": "err",
                                    "twoMessages": True,
                                    "reference": reference,
                                    "firstMessage": response,
                                    "secondMessage": response2
                                }

                        if continueProcessing:
                            if version != "REV":
                                result = biblegateway.getResult(
                                    reference, version, headings, verseNumbers)

                                content = "```Dust\n" + result.title + \
                                    "\n\n" + result.text + "```"

                                responseString = "**" + result.passage + \
                                    " - " + result.version + "**\n\n" + content

                                if len(responseString) < 2000:
                                    return {
                                        "level": "info",
                                        "twoMessages": "false",
                                        "reference": reference,
                                        "message": responseString
                                    }
                                elif len(responseString) > 2000:
                                    if len(responseString) < 3500:
                                        splitText = central.splitter(
                                            result.text)

                                        content1 = "```Dust\n" + \
                                            result.title + "\n\n" + \
                                            splitText["first"] + "```"
                                        responseString1 = "**" + \
                                            result.passage + " - " + \
                                            result.version + "**\n\n" + \
                                            content1
                                        content2 = "```Dust\n " + \
                                            splitText["second"] + "```"

                                        return {
                                            "level": "info",
                                            "twoMessages": True,
                                            "reference": reference,
                                            "firstMessage": responseString1,
                                            "secondMessage": content2
                                        }
                                    else:
                                        return {
                                            "level": "err",
                                            "twoMessages": False,
                                            "reference": reference,
                                            "message": lang.passagetoolong
                                        }
                            else:
                                result = rev.getResult(
                                    reference, version, headings, verseNumbers)

                                content = "```Dust\n" + result.title + \
                                    "\n\n" + result.text + "```"

                                responseString = "**" + result.passage + \
                                    " - " + result.version + "**\n\n" + content

                                if len(responseString) < 2000:
                                    return {
                                        "level": "info",
                                        "twoMessages": "false",
                                        "reference": reference,
                                        "message": responseString
                                    }
                                elif len(responseString) > 2000:
                                    if len(responseString) < 3500:
                                        splitText = central.splitter(
                                            result.text)

                                        content1 = "```Dust\n" + \
                                            result.title + "\n\n" + \
                                            splitText["first"] + "```"
                                        responseString1 = "**" + \
                                            result.passage + " - " + \
                                            result.version + "**\n\n" + \
                                            content1
                                        content2 = "```Dust\n " + \
                                            splitText["second"] + "```"

                                        return {
                                            "level": "info",
                                            "twoMessages": True,
                                            "reference": reference,
                                            "firstMessage": responseString1,
                                            "secondMessage": content2
                                        }
                                    else:
                                        return {
                                            "level": "err",
                                            "twoMessages": False,
                                            "reference": reference,
                                            "message": lang.passagetoolong
                                        }
