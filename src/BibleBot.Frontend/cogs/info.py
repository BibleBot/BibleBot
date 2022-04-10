"""
    Copyright (C) 2016-2022 Kerygma Digital Co.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this file,
    You can obtain one at https://mozilla.org/MPL/2.0/.
"""

import os
import requests
from disnake import CommandInteraction
import disnake
from disnake.ext import commands
from logger import VyLogger
from utils import backend

logger = VyLogger("default")


class Information(commands.Cog):
    def __init__(self, bot):
        self.bot = bot

    @commands.slash_command(description="The help command.")
    async def biblebot(self, inter: CommandInteraction):
        resp = await backend.submit_command(inter.channel, inter.author, "+biblebot")
        await inter.response.send_message(embed=resp)

    @commands.slash_command(description="Statistics on the bot.")
    async def stats(self, inter: CommandInteraction):
        send_stats(self.bot)
        resp = await backend.submit_command(inter.channel, inter.author, "+stats")

        await inter.response.send_message(embed=resp)

    @commands.slash_command(description="See bot and support server invites.")
    async def invite(self, inter: CommandInteraction):
        resp = await backend.submit_command(inter.channel, inter.author, "+invite")
        await inter.response.send_message(embed=resp)


def send_stats(bot: disnake.AutoShardedClient):
    endpoint = os.environ.get("ENDPOINT")
    token = os.environ.get("ENDPOINT_TOKEN")

    shard_count = bot.shard_count
    guild_count = len(bot.guilds)
    user_count = sum([x.member_count for x in bot.guilds])
    channel_count = sum([len(x.channels) for x in bot.guilds])

    requests.post(
        f"{endpoint}/stats/process",
        json={
            "Token": token,
            "Body": f"{shard_count}||{guild_count}||{user_count}||{channel_count}",
        },
    )
