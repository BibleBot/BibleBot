"""
    Copyright (c) 2018-2020 Elliott Pardee < me [at] thevypr [dot] com>
    This file is part of name_scraper.

    name_scraper is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    name_scraper is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with name_scraper.  If not, see <http://www.gnu.org/licenses/>.
"""

import aiohttp
import json
import os
import sys
import click
from bs4 import BeautifulSoup
import logging
from http.client import HTTPConnection
HTTPConnection.debuglevel = 0

logging.getLogger("aiohttp").setLevel(logging.WARNING)
logging.getLogger("urllib3").setLevel(logging.WARNING)
logging.getLogger("urllib3.connectionpool").setLevel(logging.WARNING)

dir_path = os.path.dirname(os.path.realpath(__file__))
sys.path.append(dir_path + "/")

from ext.vylogger import VyLogger  # noqa: E501

logger = VyLogger("default")

master_map = json.load(open(f"{dir_path}/mappings/master.json"))
apibible_map = json.load(open(f"{dir_path}/mappings/apibible.json"))


def log_message(level, source, msg):
    message = f"[shard 0] <{source}@name_scraper> {msg}"

    if level == "warn":
        logger.warning(message)
    elif level == "err":
        logger.error(message)
    elif level == "info":
        logger.info(message)
    elif level == "debug":
        logger.debug(message)


def is_nuisance(word):
    nuisances = ["pet", "in", "pet.", "in."]

    if word.lower() in nuisances:
        return True
    
    return False


async def get_bible_gateway_versions():
    versions = {}
    ignored = ["Arabic Bible: Easy-to-Read Version (ERV-AR)", "Ketab El Hayat (NAV)", "Farsi New Testament",
               "Farsi Ebook Bible", "Habrit Hakhadasha/Haderekh (HHH)", "The Westminster Leningrad Codex (WLC)",
               "Urdu Bible: Easy-to-Read Version (ERV-UR)", "Hawai‘i Pidgin (HWP)"]

    async with aiohttp.ClientSession() as session:
        async with session.get("https://www.biblegateway.com/versions/") as res:
            if res is not None:
                soup = BeautifulSoup(await res.text(), "lxml")

                with click.progressbar(soup.find_all("td", {"class": ["collapse", "translation-name"]})) as bar:
                    for version in bar:
                        for a in version.find_all("a", href=True):
                            version_name = a.text
                            link = a["href"]

                            if "#booklist" in link and version_name not in ignored:
                                versions[version_name] = {}
                                versions[version_name]["booklist"] = "https://www.biblegateway.com" + link

                return versions


async def get_bible_gateway_names(versions):
    global master_map
    
    if versions is not {}:
        with click.progressbar(versions) as bar:
            for item in bar:
                booklist_url = versions[item]["booklist"]
                async with aiohttp.ClientSession() as session:
                    async with session.get(booklist_url) as book_res:
                        if book_res is not None:
                            soup = BeautifulSoup(await book_res.text(), "lxml")

                            table = soup.find("table", {"class": "chapterlinks"})

                            for table_field in table.find_all("td"):
                                book = dict(table_field.attrs).get("data-target")

                                for chapter_numbers in table_field.find_all("span", {"class": "num-chapters"}):
                                    chapter_numbers.decompose()

                                if not str(book) == "None":
                                    book = book[1:-5]
                                    classes = dict(table_field.attrs).get("class")
                                    name = table_field.text.strip()

                                    try:
                                        if book in ["3macc", "4macc"]:
                                            book = book[0:-2]
                                        elif book in ["gkesth", "adest", "addesth"]:
                                            book = "gkest"
                                        elif book in ["sgthree", "sgthr", "prazar"]:
                                            book = "praz"

                                        if "book-name" in classes and not is_nuisance(name):
                                            if name not in master_map[book]:
                                                master_map[book].append(name)
                                    except KeyError:
                                        log_message("info", "bible_gateway", f"Inconsistency found: `{book}` in {item}, "
                                                    "please file an issue on GitHub.")


async def get_apibible_versions(api_key):
    headers = {"api-key": api_key}

    async with aiohttp.ClientSession() as session:
        async with session.get("https://api.scripture.api.bible/v1/bibles", headers=headers) as res:
            versions = []

            if res is not None:
                data = await res.json()
                data = data["data"]

                with click.progressbar(data) as bar:
                    for entry in bar:
                        versions.append({
                            "id": entry["id"],
                            "name": entry["name"]
                        })

            return versions


async def get_apibible_names(versions, api_key):
    global master_map
    global apibible_map

    if versions is not {}:
        with click.progressbar(versions) as bar:
            for version in bar:
                version_id = version["id"]
                version_name = version["name"]

                headers = {"api-key": api_key}
                async with aiohttp.ClientSession() as session:
                    async with session.get(f"https://api.scripture.api.bible/v1/bibles/{version_id}/books", headers=headers) as res:
                        if res is not None:
                            data = await res.json()
                            data = data["data"]

                            for entry in data:
                                apibible_id = entry["id"]
                                apibible_name = entry["name"]
                                apibible_abbv = entry["abbreviation"]

                                try:
                                    master_name = apibible_map[apibible_id]

                                    if apibible_name is not None:
                                        apibible_name = apibible_name.strip()

                                        if apibible_name not in master_map[master_name]:
                                            master_map[master_name].append(apibible_name)

                                    if apibible_abbv is not None:
                                        apibible_abbv = apibible_abbv.strip()

                                        if apibible_abbv not in master_map[master_name] and not is_nuisance(apibible_abbv):
                                            master_map[master_name].append(apibible_abbv)
                                except KeyError:
                                    if apibible_id != "DAG":
                                        log_message("info", "apibible",
                                                    f"Inconsistency found: `{apibible_id}` in {version_name}, "
                                                    f"book name: `{apibible_name}`.")


async def update_books(apibible_key=None, dry=False):
    global master_map

    if not dry:
        log_message("info", "bible_gateway", "Getting versions...")
        versions = await get_bible_gateway_versions()

        log_message("info", "bible_gateway", "Getting book names...")
        await get_bible_gateway_names(versions)

        if apibible_key:
            log_message("info", "apibible", "Getting versions...")
            versions = await get_apibible_versions(apibible_key)

            log_message("info", "apibible", "Getting book names...")
            await get_apibible_names(versions, apibible_key)

        if os.path.isfile(f"{dir_path}/mappings/combine.json"):
            log_message("info", "global", "Removing old names file...")
            os.remove(f"{dir_path}/mappings/combine.json")

        with open(f"{dir_path}/mappings/combine.json", "w") as file:
            log_message("info", "global", "Writing new names file...")
            file.write(json.dumps(master_map))
    else:
        log_message("info", "global", "Checking for existing names file...")

        if os.path.isfile(f"{dir_path}/mappings/combine.json"):
            log_message("info", "global", "Existing names file found, linking...")

            master_map = open(f"{dir_path}/mappings/combine.json")
            master_map = json.load(master_map)
        else:
            log_message("info", "global", "Names file not found, please run with dry=False to update.")

    log_message("info", "global", "Done.")


def get_books():
    return master_map
