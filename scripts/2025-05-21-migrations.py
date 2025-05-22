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


def fix_net_internal_id():
    versions = db.Versions
    result = versions.update_one(
        {"Abbreviation": "NET"},
        {"$set": {"InternalId": "New-English-Translation-NET-Bible"}},
    )
    print(f"updated {result.modified_count} of {result.matched_count}")


def rename_rvr1977():
    versions = db.Versions
    result = versions.update_one(
        {"Abbreviation": "RVR1977"},
        {"$set": {"Name": "Reina Valera Revisada (RVR1977)"}},
    )
    print(f"updated {result.modified_count} of {result.matched_count}")


def remove_unavailable_version():
    versions = db.Versions
    result = versions.delete_one({"Abbreviation": "VIET"})
    print(f"deleted {result.deleted_count}")


print("fixing NET's internal ID")
fix_net_internal_id()

print("renaming RVR1977")
rename_rvr1977()

print("remove unavailable versions")
remove_unavailable_version()
