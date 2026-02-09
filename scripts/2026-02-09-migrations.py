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

# List of versions that follow the Septuagint (LXX) numbering for Psalms
# This typically involves combining Psalms 9 and 10, and 114 and 115,
# resulting in a numbering that is generally one behind the Masoretic Text (MT)
# from Psalm 10 to 147.
versionsWithLXXPsalms = [
    "LXX",  # Septuagint
    "ELXX",  # Brenton's Septuagint
    "VULGATE",  # Biblia Sacra Vulgata
    "DRA",  # Douay-Rheims
    # "RUSV",  # Russian Synodal Version-- this one is technically LXX-numbered, but BG reworks the numbering to fit the Hebrew
    # "BG1940",  # 1940 Bulgarian Bible -- this one is technically LXX-numbered, but BG reworks the numbering to fit the Hebrew
    "BOB",  # Bulgarian Synodal
]


def setSeptuagintFlag():
    versions = db.Versions
    for abbv in versionsWithLXXPsalms:
        result = versions.update_one(
            {"Abbreviation": abbv}, {"$set": {"FollowsSeptuagintNumbering": True}}
        )
        if result.matched_count > 0:
            print(f"Updated {abbv}: set FollowsSeptuagintNumbering=True")
        else:
            print(f"Warning: Version {abbv} not found in database")


setSeptuagintFlag()
