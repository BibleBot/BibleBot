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

import discord
import central
import os
import sys
import json
import zlib

from extensions import compile_extrabiblical

dir_path = os.path.dirname(os.path.realpath(__file__))
sys.path.append(f"{dir_path}/../..")

data_path = f"{dir_path}/../../../data"


def open_data_file(path):
    try:
        return json.loads(zlib.decompress(open(f"{data_path}/extrabiblical/{path}", "rb").read()))
    except json.JSONDecodeError:
        compile_extrabiblical.compile_resources()
        return json.loads(zlib.decompress(open(f"{data_path}/extrabiblical/{path}", "rb").read()))


resources = {
    "lsc": open_data_file(f"catechisms/luthers_small_catechism.bin"),
    #"heidelberg": open_data_file(f"catechisms/heidelberg_catechism.bin"),
    "ccc": open_data_file(f"catechisms/catechism_of_the_catholic_church.bin")
}


def create_embed(lang, title, description, image=None, _copyright=None, error=False):
    embed = discord.Embed()

    if error:
        embed.color = 16723502
    else:
        embed.color = 303102

    if image:
        embed.set_thumbnail(url=f"https://i.imgur.com/{image}.png")

    if _copyright:
        embed.set_footer(text=f"{_copyright[1:]} // BibleBot {central.version}", icon_url=central.icon)
    elif error:
        embed.set_footer(text=f"BibleBot {central.version}", icon_url=central.icon)
    else:
        embed.set_footer(text=lang["pubdomain"] + f" // BibleBot {central.version}", icon_url=central.icon)

    embed.title = title
    embed.description = description

    return embed


def create_embeds(lang, resource, section=None, page=None, guild=None):
    if resource in resources.keys():
        if resource == "ccc":
            if guild:
                if guild.id == "238001909716353025":
                    return
                else:
                    return create_numbered_embed(lang, resource, paragraph=section)
        elif resource in ["heidelberg"]:
            return create_numbered_embed(lang, resource, paragraph=section)

        catechism_obj = resources[resource]
        title = catechism_obj["title"]
        author = catechism_obj["author"]
        image = catechism_obj["image"]
        _copyright = catechism_obj["copyright"]
        category = parse_category(lang, catechism_obj["category"])
        sections = catechism_obj["sections"]
        section_is_index = True

        try:
            if not isinstance(int(section), int):
                section_is_index = False
        except (ValueError, TypeError):
            section_is_index = False

        try:
            if isinstance(int(page), int):
                page = int(page)
        except (ValueError, TypeError):
            page = None

        if image == "":
            image = None

        if not section:
            return create_full_embed(lang, title, author, _copyright, category, sections, image)
        elif not page:
            if section_is_index:
                try:
                    section_obj = sections[int(section) - 1]
                except IndexError:
                    return
            else:
                section_objs = [x for x in sections if section in x["slugs"]]

                if section_objs:
                    section_obj = section_objs[0]
                else:
                    section_obj = None

            title_page = create_title_page(lang, title, author, _copyright, category, section=section_obj,
                                           image=image)

            if section_obj:
                pages = [title_page]

                for i in range(0, len(section_obj["pages"])):
                    pages.append(create_section_page(lang, title, section_obj, i, _copyright))

                return {
                    "level": "info",
                    "paged": True,
                    "pages": pages
                }
        else:
            if section_is_index:
                try:
                    section_obj = sections[int(section) - 1]
                except IndexError:
                    return
            else:
                section_objs = [sectionx for sectionx in sections if section in sectionx["slugs"]]

                if section_objs:
                    section_obj = section_objs[0]
                else:
                    section_obj = None

            if section_obj:
                message = create_section_page(lang, title, section_obj, page - 1, _copyright)

                if message is not None:
                    return {
                        "level": "info",
                        "message": create_section_page(lang, title, section_obj, page - 1, _copyright)
                    }
            else:
                return create_full_embed(lang, title, author, _copyright, category, sections, image)


def create_title_page(lang, title, author, _copyright, category, sections=None, section=None, image=None):
    category_translated = lang["category"]
    sections_translated = lang["sections"]
    section_translated = lang["section"]

    description = lang["author"].replace("<author>", author)
    description += f"{category_translated}{category}\n\n"

    if not sections:
        if section:
            description += f"{section_translated}{create_section_description(lang, section)}"
    else:
        description += f"{sections_translated}"

        for item in sections:
            description += create_section_description(lang, item)

    return create_embed(lang, title, description, image=image, _copyright=_copyright)


def create_section_description(lang, item):
    i_title = item["title"]
    slugs = item["slugs"]

    if len(item["pages"]) > 1:
        page_count = lang["pages"].replace("<num>", str(len(item["pages"])))
    else:
        page_count = lang["singlePage"]  # "1 page"

    return f"{slugs[0]}. {i_title} ({page_count}) `{slugs}`\n"


def create_section_page(lang, title, section, page_num, _copyright):
    section_title = section["title"]
    section_pages = section["pages"]
    page_count = len(section_pages)
    page_counter = lang["pageOf"].replace("<num>", str(page_num + 1)).replace("<total>", str(page_count))

    title = f"{section_title} ({page_counter}) // {title}"

    try:
        description = section_pages[page_num]
    except IndexError:
        return

    return create_embed(lang, title, description, _copyright=_copyright)


def parse_category(lang, category):
    return_str = ""

    if "." in category:
        categories = category.split(".")

        for item in categories:
            translated = lang[item]
            return_str += f"{translated} > "

        return_str = return_str[0:-3]
    else:
        return_str = lang[category]

    return return_str


def create_full_embed(lang, title, author, _copyright, category, sections, image):
    title_page = create_title_page(lang, title, author, _copyright, category, sections=sections,
                                   image=image)
    pages = [title_page]

    for item in sections:
        for i in range(0, len(item["pages"])):
            pages.append(create_section_page(lang, title, item, i, _copyright))

    return {
        "level": "info",
        "paged": True,
        "pages": pages
    }


def create_numbered_embed(lang, resource, paragraph=None):
    catechism_obj = resources[resource]
    title = catechism_obj["title"]
    author = catechism_obj["author"]
    image = catechism_obj["image"]
    _copyright = catechism_obj["copyright"]
    category = parse_category(lang, catechism_obj["category"])

    if paragraph is None:
        return {
            "level": "info",
            "message": create_title_page(lang, title, author, _copyright, category, image=image)
        }
    else:
        paragraphs = []

        if "-" not in paragraph:
            try:
                if isinstance(int(paragraph), int):
                    paragraphs.append(int(paragraph))
            except (ValueError, TypeError):
                pass
        else:
            sep_paragraphs = paragraph.split("-")

            for paragraph in sep_paragraphs:
                try:
                    if isinstance(int(paragraph), int):
                        paragraphs.append(int(paragraph))
                except (ValueError, TypeError):
                    pass

            if len(sep_paragraphs) > 2:
                return {
                    "level": "err",
                    "message": create_embed(lang, f"{title}", lang["rangeError"], error=True)
                }

            for index, paragraph in enumerate(paragraphs):
                if 0 < index < len(paragraphs):
                    if paragraphs[index - 1] > paragraph:
                        return {
                            "level": "err",
                            "message": create_embed(lang, f"{title}", lang["rangeError"], error=True)
                        }

        pages = []
        highest_num = 0

        if len(paragraphs) > 1:
            for i in paragraphs:
                if 0 < i < len(catechism_obj["paragraphs"]):
                    if i > highest_num:
                        highest_num = i

            if (highest_num - paragraphs[0]) > 14:
                return {
                    "level": "err",
                    "message": create_embed(lang, f"{title}", lang["rangeError"], error=True)
                }

            for i in range(paragraphs[0], highest_num + 1):
                pages.append(create_embed(lang, f"{title} - Paragraph {i}",
                                          catechism_obj["paragraphs"][i - 1]["text"],
                                          _copyright=_copyright))
        else:
            if 0 < paragraphs[0] < len(catechism_obj["paragraphs"]):
                pages.append(create_embed(lang, f"{title} - Paragraph {paragraphs[0]}",
                                          catechism_obj["paragraphs"][paragraphs[0] - 1]["text"],
                                          _copyright=_copyright))

        if len(pages) > 1:
            return {
                "level": "info",
                "paged": True,
                "pages": pages
            }
        elif len(pages) > 0:
            return {
                "level": "info",
                "message": pages[0]
            }
