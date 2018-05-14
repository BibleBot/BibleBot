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
import numbers
import os
import re
import sys

__dir_path = os.path.dirname(os.path.realpath(__file__))
sys.path.append(__dir_path + "/../..")

import data.BGBookNames.start as bgbooknames  # noqa: E402
import central  # noqa: E402

books = None

try:
    books = open(__dir_path + "/../../data/BGBookNames/books.json")
    books = json.loads(books.read())
except FileNotFoundError:
    bgbooknames.getBooks()
    books = open(__dir_path + "/../../data/BGBookNames/books.json")
    books = json.loads(books.read())


def tokenize(msg):
    array = []

    dashes = ["-", "—", "–"]
    dash_in_message = any(dash in msg for dash in dashes)
    dash_used = [dash for dash in dashes if dash in msg]

    if dash_in_message:
        for item in msg.split(dash_used[0]):
            split = item.split(":")

            for subitem in split:
                tmp_split = subitem.split(" ")

                for subsubitem in tmp_split:
                    subsubitem = re.sub(r"[^a-zA-Z0-9:()\"'<>|\[\]{\}\\/ ;*&^%$#@!.+_?=]", "", subsubitem)

                    array.append(subsubitem)
    else:
        for item in msg.split(":"):
            split = item.split(" ")

            for subitem in split:
                array.append(subitem)

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
    msg = re.sub(r"[^a-zA-Z0-9 ()\[\]{}<>:-]", "", msg)
    return central.capitalize_first_letter(msg)


def purge_brackets(msg):
    msg = msg.replace("(", "")
    msg = msg.replace(")", "")
    msg = msg.replace("[", "")
    msg = msg.replace("]", "")
    msg = msg.replace("{", "")
    msg = msg.replace("}", "")
    msg = msg.replace("<", "")
    msg = msg.replace(">", "")
    return msg.replace(" ", "")


def get_difference(a, b):
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


def get_book(array, index):
    for key in books:
        for value in books[key]:
            if array[index] in value:
                try:
                    item = array[index] + " " + array[index + 1]

                    if item in value:
                        return (key, "+")
                    elif array[index] == value:
                        return (key, "none")
                except Exception:
                    try:
                        item = array[index - 1] + " " + array[index]

                        if item in value:
                            return (key, "-")
                        elif array[index] == value:
                            return (key, "none")
                    except Exception:
                        if array[index] == value:
                            return (key, "none")


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


def create_verse_object(array, book_index, available_versions):
    # TODO: See what's up with array length.
    # Compare it to bookIndex and see if there's more values
    # that could compose a verse?
    # This function might need a rewrite.
    book_index = int(book_index)

    # check if there are numbers after the book
    try:
        if isinstance(array[book_index + 1], numbers.Number):
            return "invalid - NaN"

        if isinstance(purge_brackets(array[book_index + 2]), numbers.Number):
            return "invalid - NaN"
    except Exception:
        return "invalid"

    # find various indexes for brackets and see
    # if our verse is being surrounded by them
    bracket_indexes = []
    for i, j in enumerate(array):
        if i <= book_index:
            if central.dividers["first"] in j:
                is_instance = isinstance(j.index(central.dividers["first"]), numbers.Number)

                if is_instance:
                    bracket_indexes.append(i)

        if i > book_index:
            if central.dividers["second"] in j:
                is_instance = isinstance(j.index(central.dividers["second"]), numbers.Number)

                if is_instance:
                    bracket_indexes.append(i)

    if len(bracket_indexes) == 2:
        if bracket_indexes[0] <= book_index <= bracket_indexes[1]:
            return "invalid - brackets surrounding"

    verse = {
        "book": purge_brackets(array[book_index]),
        "chapter": array[book_index + 1],
        "startingVerse": array[book_index + 2]
    }

    if len(array) > book_index + 3:
        array[book_index + 3] = purge_brackets(array[book_index + 3])

        try:
            if isinstance(int(array[book_index + 3]), numbers.Number):
                if isinstance(int(array[book_index + 2]), numbers.Number):
                    if int(array[book_index + 3]) >= int(array[book_index + 2]):
                        ending_verse = array[book_index + 3]
                        verse["endingVerse"] = ending_verse
        except Exception:
            if array[book_index + 3].upper() in available_versions:
                version = array[book_index + 3]
                verse["version"] = version

    if len(array) > book_index + 4:
        array[book_index + 4] = purge_brackets(array[book_index + 4])

        if array[book_index + 4].upper() in available_versions:
            version = array[book_index + 4]
            verse["version"] = version

    return verse


def create_reference_string(verse):
    reference = None

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
                ":" + verse[2] + " | v: " + verse[3][4:]
        elif len(verse) == 5:
            if verse[4].startswith("v"):
                reference = verse[0] + " " + verse[1] + ":" + \
                    verse[2] + "-" + verse[3] + " | v: " + verse[4][4:]
        else:
            if verse[3].startswith("v"):
                reference = verse[0] + " " + verse[1] + \
                    ":" + verse[2] + " | v: " + verse[3][4:]
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
