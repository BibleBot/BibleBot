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

import os
import sys
import re
import numbers

__dir_path = os.path.dirname(os.path.realpath(__file__))
sys.path.append(__dir_path + "/../..")

import central  # noqa: E402


def tokenize(msg):
    array = []

    dashes = ["-", "—", "–"]
    dashInMessage = any(dash in msg for dash in dashes)
    dashUsed = [dash for dash in dashes if dash in msg]

    if dashInMessage:
        for item in msg.split(dashUsed[0]):
            split = item.split(":")

            for item in split:
                tmpSplit = item.split(" ")

                for item in tmpSplit:
                    item = re.sub(
                        r"[^a-zA-Z0-9:()\"'<>" +
                        r"|\[\]\{\}\\/ ; *&^%$  # @!.+_?=]", "", item)

                    array.append(item)
    else:
        for item in msg.split(":"):
            split = item.split(" ")

            for item in split:
                array.append(item)

    return array


def purify(msg):
    msg = msg.replace("(", " ( ")
    msg = msg.replace(")", " ) ")
    msg = msg.replace("[", " [ ")
    msg = msg.replace("]", " ] ")
    msg = msg.replace("{", " { ")
    msg = msg.replace("}", " } ")
    msg = msg.replace("<", " < ")
    msg = msg.replace(">", " > ")
    msg = re.sub(r"[^a-zA-Z0-9 \(\)\[\]{} <>: -]", "", msg)
    return central.capitalizeFirstLetter(msg)


def purgeBrackets(msg):
    msg = msg.replace("(", "")
    msg = msg.replace(")", "")
    msg = msg.replace("[", "")
    msg = msg.replace("]", "")
    msg = msg.replace("{", "")
    msg = msg.replace("}", "")
    msg = msg.replace("<", "")
    msg = msg.replace(">", "")
    return msg.replace(" ", "")


def getDifference(a, b):
    i = 0
    j = 0
    result = ""

    while j < len(b):
        try:
            if a[i] != b[j] or i == len(a):
                result += b[j]
            else:
                i += 1
        except Exception:
            result += b[j]

        j += 1

    return result.strip()


def parseSpacedBookName(item, array, index):
    singleSpacedBooks = ["Sam", "Sm", "Shmuel", "Kgs", "Melachim", "Chron",
                         "Chr", "Cor", "Thess", "Thes", "Tim", "Tm", "Pet",
                         "Pt", "Macc", "Mac", "Esd", "Samuel", "Kings",
                         "Chronicles", "Esdras", "Maccabees", "Corinthians",
                         "Thessalonians", "Timothy", "Peter", "151"]

    doubleSpacedBooks = ["Azariah", "Manasses", "Manasseh", "Solomon", "Songs"]

    thatOneSongInTheDeuterocanon = ["Men", "Youths", "Children"]

    if item in singleSpacedBooks:
        array[index] = array[index - 1] + item
    elif item == "Esther":
        if index > 0:
            if array[index - 1] == "Greek":
                array[index] = array[index - 1] + item
            else:
                array[index] = "Esther"
        else:
            array[index] = "Esther"
    elif item == "Jeremiah":
        isLetter = ((array[index - 2] + array[index - 1]) == "LetterOf")

        if isLetter:
            array[index] = array[index - 2] + array[index - 1] + item
        else:
            array[index] = "Jeremiah"
    elif item == "Dragon":
        array[index] = array[index - 3] + \
            array[index - 2] + array[index - 1] + item
    elif item in thatOneSongInTheDeuterocanon:
        array[index] = array[index - 5] + array[index - 4] + \
            array[index - 3] + array[index - 2] + array[index - 1] + item
    elif item in doubleSpacedBooks:
        array[index] = array[index - 2] + array[index - 1] + item
    elif item in ["John", "Jn"]:
        num = 0
        try:
            if array[index - 1] is not None and index != 0:
                num = int(array[index - 1])

                if num > 0 and num < 4:
                    array[index] = array[index - 1] + item
        except (ValueError, TypeError):
            array[index] = array[index]  # this is so the linter shuts up

    return array[index]


def createVerseObject(array, bookIndex, availableVersions):
    verse = []
    bookIndex = int(bookIndex)

    try:
        if isinstance(array[bookIndex + 1], numbers.Number):
            return "invalid - NaN"

        if isinstance(purgeBrackets(array[bookIndex + 2]), numbers.Number):
            return "invalid - NaN"
    except Exception:
        return "invalid"

    # if array[bookIndex].index(central.dividers["first"]):
    #    return "invalid - found bracket at beginning"

    bracketIndexes = []
    for i, j in enumerate(array):
        if i <= bookIndex:
            if central.dividers["first"] in j:
                isInstance = isinstance(j.index(
                    central.dividers["first"]), numbers.Number)

                if isInstance:
                    bracketIndexes.append(i)

        if i > bookIndex:
            if central.dividers["second"] in j:
                isInstance = isinstance(j.index(
                    central.dividers["second"]), numbers.Number)

                if isInstance:
                    bracketIndexes.append(i)

    if len(bracketIndexes) == 2:
        if bracketIndexes[0] <= bookIndex and bracketIndexes[1] > bookIndex:
            return "invalid - brackets surrounding"

    book = purgeBrackets(array[bookIndex])
    chapter = array[bookIndex + 1]
    startingVerse = array[bookIndex + 2]

    verse.append(book)
    verse.append(chapter)
    verse.append(startingVerse)

    if len(array) > bookIndex + 3:
        if central.dividers["second"] in array[bookIndex + 3]:
            return "invalid - ending bracket found"

        array[bookIndex + 3] = purgeBrackets(array[bookIndex + 3])

        try:
            if isinstance(int(array[bookIndex + 3]), numbers.Number):
                if isinstance(int(array[bookIndex + 2]), numbers.Number):
                    if int(array[bookIndex + 3]) > int(array[bookIndex + 2]):
                        endingVerse = array[bookIndex + 3].replace(
                            central.dividers["first"], "")
                        endingVerse = endingVerse.replace(
                            central.dividers["second"], "")
                        verse.append(purgeBrackets(endingVerse))
        except Exception:
            if array[bookIndex + 3].upper() in availableVersions:
                version = array[bookIndex + 3].replace(
                    central.dividers["first"], "")
                version = version.replace(central.dividers["second"], "")
                verse.append("v - " + version.upper())
            else:
                verse.append(re.sub(
                    r"[a-zA-Z]", "-", array[bookIndex + 3]))

    if len(array) > bookIndex + 4:
        if central.dividers["second"] in array[bookIndex + 4]:
            return "invalid - ending bracket found"

        array[bookIndex + 4] = purgeBrackets(array[bookIndex + 4])

        if array[bookIndex + 4].upper() in availableVersions:
            version = array[bookIndex + 4].replace(
                central.dividers["first"], "")
            version = version.replace(central.dividers["second"], "")
            verse.append("v - " + version.upper())

    return verse


def createReferenceString(verse):
    reference = None

    for v in verse:
        if isinstance(v, str):
            v = re.sub(r"[^a-zA-Z0-9\-]", "", v)

    try:
        if not isinstance(int(verse[1]), numbers.Number):
            return

        if not isinstance(int(verse[2]), numbers.Number):
            return

        if len(verse) == 4:
            if not isinstance(int(verse[3]), numbers.Number):
                return
    except Exception:
        verse = verse

    if len(verse) <= 3:
        reference = verse[0] + " " + verse[1] + ":" + verse[2]

    if len(verse) >= 4:
        if verse[3].startswith("v"):
            reference = verse[0] + " " + verse[1] + \
                ":" + verse[2] + " | v: " + verse[3][1:]
        elif len(verse) == 5:
            if verse[4].startswith("v"):
                reference = verse[0] + " " + verse[1] + ":" + \
                    verse[2] + "-" + verse[3] + " | v: " + verse[4][1:]
        else:
            if verse[3].startswith("v"):
                reference = verse[0] + " " + verse[1] + \
                    ":" + verse[2] + " | v: " + verse[3][1:]
            elif "-" in verse[3]:
                reference = verse[0] + " " + verse[1] + \
                    ":" + verse[2]
            elif verse[3] == "":
                reference = verse[0] + " " + verse[1] + \
                    ":" + verse[2] + "-"
            else:
                reference = verse[0] + " " + verse[1] + \
                    ":" + verse[2] + "-" + verse[3]

        if not isinstance(reference, str):
            reference = verse[0] + " " + verse[1] + \
                ":" + verse[2] + "-" + verse[3]

    return reference
