"""
    Copyright (C) 2016-2025 Kerygma Digital Co.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this file,
    You can obtain one at https://mozilla.org/MPL/2.0/.
"""

# use frontend venv or perish

import pymongo
import os

client = pymongo.MongoClient(os.environ.get("MONGODB_CONN"))
db = client.BibleBotBackend

versionsToMigrateToAB = {
    "ASV": "06125adad2d5898a-01",
    "NIV": "78a9f6124f344018-01",
    "NVI": "01c25b8715dbb632-01",
    "BDS": "6f26e199139ea7f1-01",
    "CARS": "d16ede2395debe86-01",
    "CARST": "50999ebfe23c23f7-01",
    "CARSA": "8b5beb01e227434e-01",
    "GW": "9ea7245a669aa0a0-01",
    "OJB": "c89622d31b60c444-02",
}

versionsToAdd = [
    {
        "Name": "Patriarchal Text of 1904 (PAT1904)",
        "Abbreviation": "PAT1904",
        "Source": "ab",
        "ApiBibleId": "901dcd9744e1bf69-01",
        "SupportsOldTestament": False,
        "SupportsNewTestament": True,
        "SupportsDeuterocanon": False,
    },
    {
        "Name": "Evangelical Heritage Version (EHV)",
        "Abbreviation": "EHV",
        "Source": "ab",
        "ApiBibleId": "931578ed851c8c36-01",
        "SupportsOldTestament": True,
        "SupportsNewTestament": True,
        "SupportsDeuterocanon": False,
    },
    {
        "Name": "Literal Standard Version (LSV)",
        "Abbreviation": "LSV",
        "Source": "ab",
        "ApiBibleId": "01b29f4b342acc35-01",
        "SupportsOldTestament": True,
        "SupportsNewTestament": True,
        "SupportsDeuterocanon": False,
    },
    {
        "Name": "Berean Standard Bible (BSB)",
        "Abbreviation": "BSB",
        "Source": "ab",
        "ApiBibleId": "bba9f40183526463-01",
        "SupportsOldTestament": True,
        "SupportsNewTestament": True,
        "SupportsDeuterocanon": False,
    },
    {
        "Name": "Canisiusvertaling 1939 (NLD1939)",
        "Abbreviation": "NLD1939",
        "Source": "ab",
        "ApiBibleId": "ead7b4cc5007389c-01",
        "SupportsOldTestament": True,
        "SupportsNewTestament": True,
        "SupportsDeuterocanon": True,
    },
]

versionsToConsolidate = {
    "KJVA": "KJV",
}

versionsToUpdate = {
    "KJV": {
        "Name": "King James Version (KJV)",
        "Abbreviation": "KJV",
        "Source": "ab",
        "ApiBibleId": "de4e12af7f28f599-01",
        "SupportsOldTestament": True,
        "SupportsNewTestament": True,
        "SupportsDeuterocanon": True,
    }
}

languagesToAdd = [
    {"Name": "English (US)", "Culture": "en-US", "DefaultVersion": "RSV"},
    {"Name": "English (UK)", "Culture": "en-GB", "DefaultVersion": "RSV"},
    {"Name": "Esperanto", "Culture": "eo", "DefaultVersion": "RSV"},
]

languagesToConsolidate = {
    "english": "en-US",
    "english_us": "en-US",
    "english_uk": "en-GB",
    "default": "en-US",
    "esperanto": "eo",
}


def migrateVersionsToAB():
    versions = db.Versions
    for abbv, bibleId in versionsToMigrateToAB.items():
        versions.update_one(
            {"Abbreviation": abbv}, {"$set": {"Source": "ab", "ApiBibleId": bibleId}}
        )
        print(f"migrated version {abbv} to AB")


def addVersions():
    versions = db.Versions
    for version in versionsToAdd:
        versions.insert_one(version)
        print(f"added version {version["Name"]}")


def consolidateVersions():
    guilds = db.Guilds
    users = db.Users
    versions = db.Versions

    for oldVersion, newVersion in versionsToConsolidate.items():
        guild_changes = guilds.update_many(
            {"Version": oldVersion}, {"$set": {"Version": newVersion}}
        )
        print(
            f"changed {guild_changes.modified_count} guilds using version {oldVersion} to {newVersion}"
        )
        user_changes = users.update_many(
            {"Version": oldVersion}, {"$set": {"Version": newVersion}}
        )
        print(
            f"changed {user_changes.modified_count} users using version {oldVersion} to {newVersion}"
        )
        versions.delete_one({"Abbreviation": oldVersion})
        print(f"removed version {oldVersion}")


def updateVersions():
    versions = db.Versions

    for abbv, version in versionsToUpdate.items():
        versions.update_one({"Abbreviation": abbv}, {"$set": version})
        print(f"updated version {abbv}")


def addLanguages():
    languages = db.Languages

    for language in languagesToAdd:
        languages.insert_one(language)
        print(f"added language {language["Culture"]}")


def consolidateLanguages():
    guilds = db.Guilds
    users = db.Users

    for oldLanguage, newLanguage in languagesToConsolidate.items():
        guild_changes = guilds.update_many(
            {"Language": oldLanguage}, {"$set": {"Language": newLanguage}}
        )
        print(
            f"changed {guild_changes.modified_count} guilds using language {oldLanguage} to {newLanguage}"
        )
        user_changes = users.update_many(
            {"Language": oldLanguage}, {"$set": {"Language": newLanguage}}
        )
        print(
            f"changed {user_changes.modified_count} users using language {oldLanguage} to {newLanguage}"
        )


migrateVersionsToAB()
addVersions()
consolidateVersions()
updateVersions()
addLanguages()
consolidateLanguages()
