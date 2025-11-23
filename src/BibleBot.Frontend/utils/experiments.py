"""
Copyright (C) 2016-2025 Kerygma Digital Co.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this file,
You can obtain one at https://mozilla.org/MPL/2.0/.
"""

import aiohttp

endpoint = os.environ.get("ENDPOINT", "")
aiohttp_headers = {"Authorization": os.environ.get("ENDPOINT_TOKEN", "")}


async def get_active_experiments_for_user(user_id: int) -> dict[str, str]:
    async with aiohttp.ClientSession() as session:
        async with session.get(
            f"{endpoint}/experiments/active_user",
            headers=aiohttp_headers,
            params={"user_id": user_id, "frontend": True},
        ) as response:
            return await response.json()


async def get_active_experiments_for_guild(guild_id: int) -> dict[str, str]:
    async with aiohttp.ClientSession() as session:
        async with session.get(
            f"{endpoint}/experiments/active_guild",
            headers=aiohttp_headers,
            params={"guild_id": guild_id, "frontend": True},
        ) as response:
            return await response.json()
