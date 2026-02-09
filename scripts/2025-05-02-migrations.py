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


def set_all_versions_active():
    versions = db.Versions
    result = versions.update_many({}, {"$set": {"Active": True}})
    print(f"updated {result.modified_count} of {result.matched_count}")


def migrate_apibibleid_to_internalid():
    versions = db.Versions
    result = versions.update_many(
        {"Source": "ab"}, {"$rename": {"ApiBibleId": "InternalId"}}
    )
    print(f"updated {result.modified_count} of {result.matched_count}")


print("setting Active = True on all versions")
set_all_versions_active()

print("renaming ApiBibleId to InternalId")
migrate_apibibleid_to_internalid()
