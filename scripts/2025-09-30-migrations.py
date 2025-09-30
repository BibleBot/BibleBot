"""
Copyright (C) 2016-2025 Kerygma Digital Co.

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


def addMissingPublishers():
    versions = db.Versions
    for abbv, publisher in versionsMissingPublisher.items():
        versions.update_one({"Abbreviation": abbv}, {"$set": {"Publisher": publisher}})
        print(f"added publisher '{publisher}' to {abbv}")


addMissingPublishers()
