"""
Copyright (C) 2016-2025 Kerygma Digital Co.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this file,
You can obtain one at https://mozilla.org/MPL/2.0/.
"""

import os

import aiohttp
from services import backend

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


async def get_user_frontend_experiments(user_id: int):
    return await backend.get_user_frontend_experiments(user_id)


async def get_active_experiments_for_guild(guild_id: int) -> dict[str, str]:
    async with aiohttp.ClientSession() as session:
        async with session.get(
            f"{endpoint}/experiments/active_guild",
            headers=aiohttp_headers,
            params={"guild_id": guild_id, "frontend": True},
        ) as response:
            return await response.json()


async def experiment_helped(experiment_name: str, user_id: int):
    async with aiohttp.ClientSession() as session:
        await session.post(
            f"{endpoint}/experiments/helped",
            headers=aiohttp_headers,
            params={"experiment_name": experiment_name, "user_id": user_id},
        )


async def experiment_did_not_help(experiment_name: str, user_id: int):
    async with aiohttp.ClientSession() as session:
        await session.post(
            f"{endpoint}/experiments/did_not_help",
            headers=aiohttp_headers,
            params={"experiment_name": experiment_name, "user_id": user_id},
        )


async def feedback_exists(experiment_name: str, user_id: int):
    async with aiohttp.ClientSession() as session:
        async with session.get(
            f"{endpoint}/experiments/feedback_exists",
            headers=aiohttp_headers,
            params={"experiment_name": experiment_name, "user_id": user_id},
        ) as response:
            return await response.text() == "true"
