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
import numbers
import os
import re
import sys

from name_scraper.books import item_to_book
from name_scraper import client

__dir_path = os.path.dirname(os.path.realpath(__file__))
sys.path.append(f"{__dir_path}/../..")

import central  # noqa: E402

books = client.get_books()

dashes = ["-", "—", "–"]


def list_duplicates_of(seq, item):
    start_at = -1
    locs = []

    while True:
        try:
            loc = seq.index(item, start_at + 1)
        except ValueError:
            break
        else:
            locs.append(loc)
            start_at = loc

    return locs


def purify(msg):
    msg = msg.replace("(", " ( ")
    msg = msg.replace(")", " ) ")
    msg = msg.replace("[", " [ ")
    msg = msg.replace("]", " ] ")
    msg = msg.replace("{", " { ")
    msg = msg.replace("}", " } ")
    msg = msg.replace("<", " < ")
    msg = msg.replace(">", " > ")
    msg = re.sub(r"[.,;'\"_=$#*&^%@!?]", "", msg)
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
        except IndexError:
            result += b[j]

        j += 1

    return result.strip()


def get_books(msg):
    results = []
    existing_indices = []

    for key, value in books.items():
        for item in value:
            valx = [item.title(), item.upper(), item.lower(), item]

            for val in valx:
                if val in msg:
                    numbered_johns = books["1john"] + books["2john"] + books["3john"]
                    numbered_esdras = books["1esd"] + books["2esd"]
                    psalm_151 = books["ps151"]

                    # tl;dr - if we find a "john", but "1/2/3 John" exists, add any non-numbered "john"
                    # etc for esdras and psalms
                    if key == "john" and any([True for x in numbered_johns if f" {x} " in f" {msg} "]):
                        last_item = item.split(" ")[-1]
                        msg_split = msg.split(" ")

                        indices = [i for i, x in enumerate(msg_split) if x == last_item]

                        for index in indices:
                            if f"{msg_split[index - 1]} {msg_split[index]}" not in numbered_johns:
                                results.append(("john", index))
                                existing_indices.append(index)
                    elif key == "ezra" and any([True for x in numbered_esdras if f" {x} " in f" {msg} "]):
                        last_item = item.split(" ")[-1]
                        msg_split = msg.split(" ")

                        indices = [i for i, x in enumerate(msg_split) if x == last_item]

                        for index in indices:
                            if f"{msg_split[index - 1]} {msg_split[index]}" not in numbered_esdras:
                                results.append(("ezra", index))
                                existing_indices.append(index)
                    elif key == "ps" and any([True for x in psalm_151 if f" {x} " in f" {msg} "]):
                        last_item = item.split(" ")[-1]
                        msg_split = msg.split(" ")

                        indices = [i for i, x in enumerate(msg_split) if x == last_item]

                        for index in indices:
                            if f"{msg_split[index]} {msg_split[index + 1]}" not in psalm_151:
                                results.append(("ps", index))
                                existing_indices.append(index)
                    else:
                        last_item = item.split(" ")[-1]
                        msg_split = msg.split(" ")

                        indices = [i for i, x in enumerate(msg_split) if x == last_item]

                        for index in indices:
                            if index not in existing_indices:
                                results.append((key, index))
                                existing_indices.append(index)
                                break

    return results


def create_verse_object(name, book_index, msg, available_versions, brackets):
    book_index = int(book_index)
    array = msg.split(" ")

    # find various indexes for brackets and see
    # if our verse is being surrounded by them
    bracket_indexes = []
    for i, j in enumerate(array):
        if i <= book_index:
            if brackets["first"] in j:
                is_instance = isinstance(j.index(brackets["first"]), numbers.Number)

                if is_instance:
                    bracket_indexes.append(i)

        if i > book_index:
            if brackets["second"] in j:
                is_instance = isinstance(j.index(brackets["second"]), numbers.Number)

                if is_instance:
                    bracket_indexes.append(i)

    if len(bracket_indexes) == 2:
        if bracket_indexes[0] <= book_index <= bracket_indexes[1]:
            return "invalid"

    try:
        number_split = array[book_index + 1].split(":")
    except IndexError:
        return "invalid"

    dash_split = None

    if len(number_split) > 1:
        dash_split = number_split[1].split("-")
    elif name in ["ps151", "obad", "phlm", "2john", "3john" "jude"]:
        number_split = [1]
        dash_split = array[book_index + 1].split("-")

    verse = {
        "book": name,
        "chapter": None,
        "startingVerse": None,
        "endingVerse": None
    }

    try:
        if isinstance(int(number_split[0]), numbers.Number):
            verse["chapter"] = int(number_split[0])

            if dash_split is not None:
                if isinstance(int(dash_split[0]), numbers.Number):
                    verse["startingVerse"] = int(dash_split[0])

                    if isinstance(int(dash_split[1]), numbers.Number):
                        verse["endingVerse"] = int(dash_split[1])

                        if verse["startingVerse"] > verse["endingVerse"]:
                            return "invalid"
    except (IndexError, TypeError, ValueError):
        pass

    try:
        if re.sub(r"[0-9]", "", dash_split[1]) == dash_split[1]:
            if dash_split[1] == "":
                verse["endingVerse"] = "-"
    except (IndexError, TypeError):
        pass

    try:
        if array[book_index + 2].upper() in available_versions:
            verse["version"] = array[book_index + 2].upper()
    except IndexError:
        pass

    if verse["startingVerse"] is None:
        return

    return verse


def create_reference_string(verse):
    reference = None

    try:
        if not isinstance(int(verse["chapter"]), numbers.Number):
            return
    except (ValueError, TypeError, KeyError):
        pass

    if verse is None:
        return

    if "startingVerse" in verse.keys():
        if verse["startingVerse"] is not None:
            if verse["book"] in item_to_book["ot"]:
                reference = item_to_book["ot"][verse["book"]] + "|" + \
                            str(verse["chapter"]) + ":" + str(verse["startingVerse"])
            elif verse["book"] in item_to_book["nt"]:
                reference = item_to_book["nt"][verse["book"]] + "|" + \
                            str(verse["chapter"]) + ":" + str(verse["startingVerse"])
            elif verse["book"] in item_to_book["deu"]:
                reference = item_to_book["deu"][verse["book"]] + "|" + \
                            str(verse["chapter"]) + ":" + str(verse["startingVerse"])
        else:
            if verse["book"] in item_to_book["ot"]:
                reference = item_to_book["ot"][verse["book"]] + "|" + str(verse["chapter"])
            elif verse["book"] in item_to_book["nt"]:
                reference = item_to_book["nt"][verse["book"]] + "|" + str(verse["chapter"])
            elif verse["book"] in item_to_book["deu"]:
                reference = item_to_book["deu"][verse["book"]] + "|" + str(verse["chapter"])

        if "endingVerse" in verse.keys():
            if verse["endingVerse"] is not None:
                try:
                    if verse["endingVerse"] != "-":
                        if int(verse["startingVerse"]) <= int(verse["endingVerse"]):
                            reference += "-" + str(verse["endingVerse"])
                    else:
                        reference += "-"
                except (ValueError, TypeError, KeyError):
                    reference = reference

    if "version" in verse.keys():
        reference = reference + " | v: " + verse["version"]

    return reference


def check_section_support(version, verse, reference, section, lang):
    for index in item_to_book[section]:
        has_section = (index == verse["book"])

        if not version[f"has{section.upper()}"] and has_section:
            response = lang[f"{section}notsupported"]
            response = response.replace("<version>", version["name"])

            response2 = lang[f"{section}notsupported2"]
            response2 = response2.replace("<setversion>", lang["commands"]["setversion"])

            reference = reference.replace("|", " ")

            return {
                "level": "err",
                "twoMessages": True,
                "reference": f"{reference} " + version["abbv"],
                "firstMessage": response,
                "secondMessage": response2
            }

    return {
        "ok": True
    }


def process_result(result, reference, version, lang):
    return_list = []

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
            if lang:
                return_list.append({
                    "level": "err",
                    "reference": f"{reference} {version}",
                    "message": lang["passagetoolong"]
                })

        return return_list
