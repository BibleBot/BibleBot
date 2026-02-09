"""
Copyright (C) 2016-2026 Kerygma Digital Co.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this file,
You can obtain one at https://mozilla.org/MPL/2.0/.
"""

# use frontend venv or perish

import os

import pymongo

client = pymongo.MongoClient(os.environ.get("MONGODB_CONN"))
db = client.BibleBotBackend

versionsMissingPublisher = {
    "BDS": "biblica",
    "NASB": "lockman",
    "NASB1997": "lockman",
    "LBLA": "lockman",
    "AMP": "lockman",
    "AMPC": "lockman",
    "LSB": "lockman",
}

versionAliases = {"NRSV": "NRSVA"}


def addMissingPublishers():
    versions = db.Versions
    for abbv, publisher in versionsMissingPublisher.items():
        versions.update_one({"Abbreviation": abbv}, {"$set": {"Publisher": publisher}})
        print(f"added publisher '{publisher}' to {abbv}")


def addVersionAliases():
    versions = db.Versions
    for abbv, aliases in versionAliases.items():
        versions.update_one({"Abbreviation": abbv}, {"$set": {"AliasOf": aliases}})
        print(f"added aliasof '{aliases}' to {abbv}")


addMissingPublishers()
