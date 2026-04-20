"""
Copyright (C) 2016-2026 Kerygma Digital Co.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this file,
You can obtain one at https://mozilla.org/MPL/2.0/.
"""

import os
import subprocess

import aiohttp
import disnake
import sentry_sdk
from disnake.ext import commands, tasks
from logger import VyLogger
from core import constants

logger = VyLogger("default")


class Tasks(commands.Cog):
    def __init__(self, bot):
        self.bot = bot
        self.run_tasks.start()

    def cog_unload(self):
        self.run_tasks.cancel()

    @tasks.loop(minutes=15)
    async def run_tasks(self):
        await constants.check_version_changes(self.bot)
        await self.update_shards(self.bot)
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

        try:
            shard_count = bot.shard_count
            guild_count = len(bot.guilds)
            user_count = sum([x.member_count for x in bot.guilds])
            channel_count = sum([len(x.channels) for x in bot.guilds])
            user_install_count = (
                await bot.application_info()
            ).approximate_user_install_count
        except Exception as e:
            sentry_sdk.capture_exception(e)
            logger.error(f"couldn't get stats, caused by {e.__class__.__name__}, bailing out")
            return

        repo_sha = (
            subprocess.check_output(["git", "rev-parse", "HEAD"])
            .decode("ascii")
            .strip()
        )

        async with aiohttp.ClientSession() as session:
            async with session.post(
                f"{endpoint}/stats/process",
                json={
                    "Body": f"{shard_count}||{guild_count}||{user_count}||{channel_count}||{user_install_count}||{repo_sha}",
                },
                headers={"Authorization": token},
            ) as resp:
                if resp.status != 200:
                    logger.error("couldn't submit stats to backend")
                elif resp.status == 200:
                    logger.info("submitted stats to backend")

    async def update_shards(self, bot: disnake.AutoShardedClient):
        if bot._connection.shard_count is None:
            logger.error("couldn't get shard count to potentially update")
            return

        if bot._connection.shard_ids is None:
            logger.error("couldn't get shard ids to potentially update")
            return

        old_shard_count = bot._connection.shard_count
        old_shard_ids = bot._connection.shard_ids

        shard_count, gateway, session_start_limit = await bot.http.get_bot_gateway(
            encoding=bot.gateway_params.encoding,
            zlib=bot.gateway_params.zlib,
        )

        if old_shard_count >= shard_count:
            logger.info("no shards to launch")
            return

        logger.info(f"launching {shard_count - old_shard_count} shards")

        bot.session_start_limit = disnake.client.SessionStartLimit(session_start_limit)

        bot.shard_count = shard_count

        bot._connection.shard_count = shard_count
        bot._connection.shard_ids = range(shard_count)

        if bot.session_start_limit is not None and bot._connection.shard_count is not None:
            if bot.session_start_limit.remaining < (bot.shard_count - old_shard_count):
                raise disnake.errors.SessionStartLimitReached(bot.session_start_limit, requested=bot._connection.shard_count)

        new_shard_ids = set(bot._connection.shard_ids) - set(old_shard_ids)

        for shard_id in new_shard_ids:
            await bot.launch_shard(gateway, shard_id, initial=False)
            logger.info(f"launched new shard {shard_id}")

        bot._connection.shards_launched.set()

