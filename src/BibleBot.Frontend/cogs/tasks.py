"""
Copyright (C) 2016-2026 Kerygma Digital Co.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this file,
You can obtain one at https://mozilla.org/MPL/2.0/.
"""

import asyncio
import os
import subprocess

import aiohttp
import disnake
import sentry_sdk
from core import constants
from disnake.ext import commands, tasks
from logger import VyLogger

logger = VyLogger("default")


class Tasks(commands.Cog):
    def __init__(self, bot):
        self.bot = bot
        self.run_tasks.start()

    def cog_unload(self):
        self.run_tasks.cancel()

    async def _run_safe(self, coro, name: str):
        try:
            await coro
        except Exception as e:
            sentry_sdk.capture_exception(e)
            logger.error(f"task '{name}' failed with {e.__class__.__name__}: {e}")

    @tasks.loop(minutes=15)
    async def run_tasks(self):
        await self._run_safe(
            constants.check_version_changes(self.bot), "check_version_changes"
        )
        await self._run_safe(self.update_shards(self.bot), "update_shards")
        await self._run_safe(self.send_stats(self.bot), "send_stats")
        await self._run_safe(self.update_topgg(self.bot), "update_topgg")
        await self._run_safe(
            self.update_discordbotlist(self.bot), "update_discordbotlist"
        )

    @run_tasks.error
    async def run_tasks_error(self, error: BaseException):
        sentry_sdk.capture_exception(error)
        logger.error(
            f"run_tasks loop died with {error.__class__.__name__}: {error}, restarting in 60s"
        )
        await asyncio.sleep(60)
        self.run_tasks.restart()

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
            logger.error(
                f"couldn't get stats, caused by {e.__class__.__name__}, bailing out"
            )
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

        old_shard_count = bot._connection.shard_count
        old_shard_ids = (
            set(bot._connection.shard_ids)
            if bot._connection.shard_ids is not None
            else set()
        )

        shard_count, gateway, session_start_limit = await bot.http.get_bot_gateway(
            encoding=bot.gateway_params.encoding,
            zlib=bot.gateway_params.zlib,
        )

        if old_shard_count >= shard_count:
            logger.info("no shards to launch")
            return

        bot.session_start_limit = disnake.client.SessionStartLimit(session_start_limit)

        # All existing shards must re-IDENTIFY with the new shard_count,
        # plus we need to IDENTIFY new shards.
        total_identifies_needed = shard_count
        if bot.session_start_limit.remaining < total_identifies_needed:
            logger.error(
                f"insufficient session starts for resharding: "
                f"need {total_identifies_needed}, have {bot.session_start_limit.remaining}, "
                f"resets at {bot.session_start_limit.reset_time}"
            )
            return

        logger.warning(
            f"shard count changed from {old_shard_count} to {shard_count}, "
            f"beginning rolling reconnect of all shards"
        )

        # Update shard_count BEFORE reconnecting so that
        # DiscordWebSocket.from_client() reads the new count for IDENTIFY payloads.
        bot.shard_count = shard_count
        bot._connection.shard_count = shard_count
        bot._connection.shard_ids = range(shard_count)

        # Reconnect all existing shards sequentially.
        # Each reconnect calls from_client() → identify() → before_identify_hook(),
        # which sleeps 5s between non-initial IDENTIFYs automatically.
        for shard_id in sorted(old_shard_ids):
            shard_info = bot.get_shard(shard_id)
            if shard_info is None:
                logger.warning(
                    f"shard {shard_id + 1} not found during resharding, skipping"
                )
                continue

            try:
                await shard_info.reconnect()
                logger.info(
                    f"reconnected shard {shard_id + 1} with new count {shard_count}"
                )
            except Exception as e:
                sentry_sdk.capture_exception(e)
                logger.error(f"failed to reconnect shard {shard_id + 1}: {e}")

        # Launch brand-new shards.
        new_shard_ids = sorted(set(range(shard_count)) - old_shard_ids)
        for shard_id in new_shard_ids:
            try:
                await bot.launch_shard(gateway, shard_id, initial=False)
                logger.info(f"launched new shard {shard_id + 1}")
            except Exception as e:
                sentry_sdk.capture_exception(e)
                logger.error(f"failed to launch new shard {shard_id + 1}: {e}")

        bot._connection.shards_launched.set()
        logger.warning(
            f"rolling reconnect complete: {len(old_shard_ids)} reconnected, "
            f"{len(new_shard_ids)} new shards launched"
        )
