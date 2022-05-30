"""
    Copyright (C) 2016-2022 Kerygma Digital Co.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this file,
    You can obtain one at https://mozilla.org/MPL/2.0/.
"""

import os
import disnake
from utils import backend
import requests
from disnake.ext import commands
from logger import VyLogger

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

    @commands.Cog.listener()
    async def on_guild_join(self, guild: disnake.Guild):
        update_topgg(self.bot)
        update_discordbotlist(self.bot)

    @commands.Cog.listener()
    async def on_guild_remove(self, guild: disnake.Guild):
        update_topgg(self.bot)
        update_discordbotlist(self.bot)

        # yeet the webhook from the database, if applicable
        reqbody = {
            "GuildId": str(guild.id),
            "Body": "delete",
            "Token": os.environ.get("ENDPOINT_TOKEN"),
        }

        endpoint = os.environ.get("ENDPOINT")
        requests.post(f"{endpoint}/webhooks/process", json=reqbody)

    @commands.Cog.listener()
    async def on_message(self, msg: disnake.Message):
        if msg.author == self.bot.user:
            return

        clean_msg = msg.content.replace("http:", "").replace("https:", "")

        if ":" in clean_msg:
            await backend.submit_verse(msg.channel, msg.author, clean_msg)


def update_topgg(bot: disnake.AutoShardedClient):
    topgg_auth = os.environ.get("TOPGG_TOKEN")

    if topgg_auth:
        body = {"server_count": len(bot.guilds)}
        requests.post(
            f"https://top.gg/api/bots/{bot.user.id}/stats",
            json=body,
            headers={"Authorization": topgg_auth},
        )

        logger.info("submitted stats to top.gg")


def update_discordbotlist(bot: disnake.AutoShardedClient):
    discordbotlist_auth = os.environ.get("DISCORDBOTLIST_TOKEN")

    if discordbotlist_auth:
        body = {
            "users": sum([x.member_count for x in bot.guilds]),
            "guilds": len(bot.guilds),
        }
        requests.post(
            f"https://discordbotlist.com/api/v1/bots/{bot.user.id}/stats",
            json=body,
            headers={"Authorization": discordbotlist_auth},
        )

        logger.info("submitted stats to discordbotlist.com")
