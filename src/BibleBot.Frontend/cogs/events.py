"""
    Copyright (C) 2016-2022 Kerygma Digital Co.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this file,
    You can obtain one at https://mozilla.org/MPL/2.0/.
"""

import disnake
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
        logger.info(f"shard {shard_id + 1} ready")

    @commands.Cog.listener()
    async def on_ready(self):
        logger.info("biblebot ready")

    @commands.Cog.listener()
    async def on_message(self, msg: disnake.Message):
        if msg.author == self.bot.user:
            return

        if msg.author.id == 186046294286925824 and msg.content == "+ping":
            await msg.channel.send("pong")
