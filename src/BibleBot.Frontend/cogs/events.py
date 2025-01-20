"""
    Copyright (C) 2016-2025 Kerygma Digital Co.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this file,
    You can obtain one at https://mozilla.org/MPL/2.0/.
"""

import os
import disnake
from utils import backend, sending
import aiohttp
from disnake.ext import commands
from logger import VyLogger
from utils.paginator import CreatePaginator
import re
import time
import json

logger = VyLogger("default")


class EventListeners(commands.Cog):
    def __init__(self, bot):
        self.bot = bot

    @commands.Cog.listener()
    async def on_shard_connect(self, shard_id):
        logger.info(f"shard {shard_id + 1} connected")

    @commands.Cog.listener()
    async def on_shard_disconnect(self, shard_id):
        logger.info(f"shard {shard_id + 1} disconnected")

    @commands.Cog.listener()
    async def on_shard_resumed(self, shard_id):
        logger.info(f"shard {shard_id + 1} resumed")

    @commands.Cog.listener()
    async def on_shard_ready(self, shard_id):
        await self.bot.change_presence(
            status=disnake.Status.online,
            activity=disnake.Game(f"/biblebot v9.2-beta - shard {shard_id + 1}"),
            shard_id=shard_id,
        )
        logger.info(f"shard {shard_id + 1} ready")

    @commands.Cog.listener()
    async def on_ready(self):
        logger.info("biblebot ready")

    # this, in theory, should work - but for some reason when dailyverseset is done initially
    # this gets triggered by us adding the webhook, yet the webhook added is not in the array
    # causing us to delete the webhook from the database as soon as we make it
    #
    # @commands.Cog.listener()
    # async def on_webhooks_update(self, ch: disnake.abc.GuildChannel):
    #     try:
    #         webhooks = await ch.webhooks()
    #         biblebot_webhooks = [x for x in webhooks if x.user.id == self.bot.user.id]

    #         # from frontend, we have no way of knowing if there was a daily verse webhook here
    #         # thus we inform backend just in case
    #         if len(biblebot_webhooks) == 0:
    #             # yeet the webhook from the database, if applicable
    #             reqbody = {
    #                 "GuildId": str(ch.guild.id),
    #                 "ChannelId": str(ch.id),
    #                 "Body": "delete",
    #                 "Token": os.environ.get("ENDPOINT_TOKEN"),
    #             }

    #             endpoint = os.environ.get("ENDPOINT")

    #             async with aiohttp.ClientSession() as session:
    #                 async with session.post(
    #                     f"{endpoint}/webhooks/process", json=reqbody
    #                 ) as resp:
    #                     if resp.status == 200:
    #                         logger.info(
    #                             f"<global@{ch.guild.id}#{ch.id}> detected removed webhook, deleting..."
    #                         )
    #     except disnake.errors.Forbidden:
    #         # this is likely triggered by on_guild_remove, if not
    #         # then the server is on their own in deleting the webhook.
    #         # the only other scenario here is that they've removed Manage Webhooks perms
    #         pass

    @commands.Cog.listener()
    async def on_guild_join(self, guild: disnake.Guild):
        await update_topgg(self.bot)
        await update_discordbotlist(self.bot)

    @commands.Cog.listener()
    async def on_guild_remove(self, guild: disnake.Guild):
        await update_topgg(self.bot)
        await update_discordbotlist(self.bot)

        # yeet the webhook from the database, if applicable
        reqbody = {"GuildId": str(guild.id), "Body": "delete"}

        endpoint = os.environ.get("ENDPOINT")

        async with aiohttp.ClientSession() as session:
            async with session.post(
                f"{endpoint}/webhooks/process",
                json=reqbody,
                headers={"Authorization": os.environ.get("ENDPOINT_TOKEN")},
            ) as resp:
                if resp.status == 200:
                    logger.info(
                        f"<global@{guild.id}#global> we've left this server, deleting webhook..."
                    )

    @commands.Cog.listener()
    async def on_message(self, msg: disnake.Message):
        if msg.author == self.bot.user:
            return

        clean_msg = msg.content.replace("://", "")
        verse_regex = re.compile(
            r"\ [0-9]{1,3}:[0-9]{1,3}(-)?([0-9]{1,3})?(:[0-9]{1,3})?"
        )

        if verse_regex.search(clean_msg):
            start_time = time.time()
            req, resp = await backend.submit_verse(msg.channel, msg.author, clean_msg)
            end_time = time.time()

            seconds_to_execute = end_time - start_time

            if seconds_to_execute > 2:
                logger.info(
                    f"<{msg.author.id}@{msg.guild.id if msg.guild is not None else msg.channel.id}#{msg.channel.id}> this response took {seconds_to_execute} seconds to receive, logging message to file"
                )
                with open("heavy_queries.json", "a", encoding="utf-8") as heavy_queries:
                    json_string = json.dumps(
                        {"req": req, "resp": resp, "time_seconds": seconds_to_execute}
                    )
                    heavy_queries.write(f"{json_string}\n")

        elif "ccc" in clean_msg.lower() and msg.guild:
            if msg.guild.id in [
                238001909716353025,
                769709969796628500,
                362503610006765568,
                636984073226813449,
            ]:
                reference_regex = re.compile(r"ccc [0-9]+(-[0-9]+)?")
                reference_regex_match = reference_regex.search(clean_msg.lower())
                if reference_regex_match:
                    resp = await backend.submit_command(
                        msg.channel,
                        msg.author,
                        f"+resource {reference_regex_match[0]}",
                    )

                    if isinstance(resp, list):
                        if len(resp) > 3:
                            await sending.safe_send_channel(
                                msg.channel,
                                embed=resp[0],
                                view=CreatePaginator(resp, msg.author.id, 180),
                            )
                        else:
                            await sending.safe_send_channel(msg.channel, embeds=resp)
                    else:
                        await sending.safe_send_channel(msg.channel, embed=resp)


async def update_topgg(bot: disnake.AutoShardedClient):
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


async def update_discordbotlist(bot: disnake.AutoShardedClient):
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
