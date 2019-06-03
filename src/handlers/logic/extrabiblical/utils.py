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
    "heidelberg": open_data_file(f"catechisms/heidelberg_catechism.bin")
}


def create_embed(title, description, image=None, _copyright=None, custom_title=False, error=False):
    embed = discord.Embed()

    if error:
        embed.color = 16723502
    else:
        embed.color = 303102

    if image:
        embed.set_thumbnail(url=f"https://i.imgur.com/{image}.png")

    if _copyright:
        embed.set_footer(text=f"{_copyright[1:]} // BibleBot {central.version}", icon_url=central.icon)
    else:
        embed.set_footer(text=f"Public Domain // BibleBot {central.version}", icon_url=central.icon)

    if custom_title:
        embed.title = title
    else:
        embed.title = central.config["BibleBot"]["commandPrefix"] + title

    embed.description = description

    return embed


def create_embeds(lang, resource, section=None, page=None):
    if resource in resources.keys():
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
        elif not page:
            if section_is_index:
                section_obj = sections[int(section) - 1]
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
        else:
            if section_is_index:
                section_obj = sections[int(section) - 1]
            else:
                section_objs = [sectionx for sectionx in sections if section in sectionx["slugs"]]

                if section_objs:
                    section_obj = section_objs[0]
                else:
                    section_obj = None

            if section_obj:
                return {
                    "level": "info",
                    "message": create_section_page(lang, title, section_obj, page - 1, _copyright)
                }
            else:
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


def create_title_page(lang, title, author, _copyright, category, sections=None, section=None, image=None):
    category_translated = lang["category"]
    sections_translated = lang["sections"]
    section_translated = lang["section"]

    description = lang["author"].replace("<author>", author)
    description += f"{category_translated}{category}\n\n"

    if not sections:
        if section:
            description += f"{section_translated}"

            i_title = section["title"]
            slugs = section["slugs"]

            if len(section["pages"]) > 1:
                page_count = lang["pages"].replace("<num>", str(len(section["pages"])))
            else:
                page_count = lang["singlePage"]  # "1 page"

            description += f"{slugs[0]}. {i_title} - {page_count} `{slugs}`\n"
    else:
        description += f"{sections_translated}"

        for item in sections:
            i_title = item["title"]
            slugs = item["slugs"]

            if len(item["pages"]) > 1:
                page_count = lang["pages"].replace("<num>", str(len(item["pages"])))
            else:
                page_count = lang["singlePage"]  # "1 page"

            description += f"{slugs[0]}. {i_title} ({page_count}) `{slugs}`\n"

    return create_embed(title, description, image=image, _copyright=_copyright, custom_title=True)


def create_section_page(lang, title, section, page_num, _copyright):
    section_title = section["title"]
    section_pages = section["pages"]
    page_count = len(section_pages)
    page_counter = lang["pageOf"].replace("<num>", str(page_num + 1)).replace("<total>", str(page_count))

    title = f"{section_title} ({page_counter}) // {title}"
    description = section_pages[page_num]

    return create_embed(title, description, _copyright=_copyright, custom_title=True)


def parse_category(lang, category):
    return_str = ""

    if "." in category:
        categories = category.split(".")

        for item in categories:
            translated = lang[item]
            return_str += f"{translated} > "

        return_str = return_str[0:-3]
    else:
        return_str = lang["category"]

    return return_str
