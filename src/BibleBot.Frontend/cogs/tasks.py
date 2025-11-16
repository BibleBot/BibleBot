"""
Copyright (C) 2016-2025 Kerygma Digital Co.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this file,
You can obtain one at https://mozilla.org/MPL/2.0/.
"""

import disnake
from disnake.ext import commands, tasks
import aiohttp, subprocess
from logger import VyLogger
import os

logger = VyLogger("default")


class Tasks(commands.Cog):
    def __init__(self, bot):
        self.bot = bot
        self.run_tasks.start()

    def cog_unload(self):
        self.run_tasks.cancel()

    @tasks.loop(minutes=15)
    async def run_tasks(self):
        await self.send_stats(self.bot)
        await self.update_topgg(self.bot)
        await self.update_discordbotlist(self.bot)

    @run_tasks.before_loop
    async def before_run_tasks(self):
        await self.bot.wait_until_ready()

    async def update_topgg(self, bot: disnake.AutoShardedClient):
        topgg_auth = os.environ.get("TOPGG_TOKEN")

        if topgg_auth:
            body = {"server_count": len(bot.guilds)}
            async with aiohttp.ClientSession() as session:
                async with session.post(
                    f"https://top.gg/api/bots/{bot.user.id}/stats",
                    json=body,
                    headers={"Authorization": topgg_auth},
                ) as resp:
                    if resp.status != 200:
                        if resp.status != 429:
                            logger.warning(
                                "couldn't submit stats to top.gg, it may be offline"
                            )
                    else:
                        logger.info("submitted stats to top.gg")

    async def update_discordbotlist(self, bot: disnake.AutoShardedClient):
        discordbotlist_auth = os.environ.get("DISCORDBOTLIST_TOKEN")

        if discordbotlist_auth:
            body = {
                "users": sum([x.member_count for x in bot.guilds]),
                "guilds": len(bot.guilds),
            }
            async with aiohttp.ClientSession() as session:
                async with session.post(
                    f"https://discordbotlist.com/api/v1/bots/{bot.user.id}/stats",
                    json=body,
                    headers={"Authorization": discordbotlist_auth},
                ) as resp:
                    if resp.status != 200:
                        if resp.status != 429:
                            logger.warning(
                                "couldn't submit stats to discordbotlist.com, it may be offline"
                            )
                    else:
                        logger.info("submitted stats to discordbotlist.com")

    async def send_stats(self, bot: disnake.AutoShardedClient):
        endpoint = os.environ.get("ENDPOINT")
        token = os.environ.get("ENDPOINT_TOKEN", "")

        shard_count = bot.shard_count
        guild_count = len(bot.guilds)
        user_count = sum([x.member_count for x in bot.guilds])
        channel_count = sum([len(x.channels) for x in bot.guilds])

        repo_sha = (
            subprocess.check_output(["git", "rev-parse", "HEAD"])
            .decode("ascii")
            .strip()
        )

        async with aiohttp.ClientSession() as session:
            async with session.post(
                f"{endpoint}/stats/process",
                json={
                    "Body": f"{shard_count}||{guild_count}||{user_count}||{channel_count}||{repo_sha}",
                },
                headers={"Authorization": token},
            ) as resp:
                if resp.status != 200:
                    logger.error("couldn't submit stats to backend")
