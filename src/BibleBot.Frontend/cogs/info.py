"""
    Copyright (C) 2016-2023 Kerygma Digital Co.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this file,
    You can obtain one at https://mozilla.org/MPL/2.0/.
"""

import os
import aiohttp
from disnake import CommandInteraction
import disnake
from disnake.ext import commands
from logger import VyLogger
from utils import backend, sending
import patreon
import subprocess

logger = VyLogger("default")
patreon_api = patreon.API(os.environ.get("PATREON_TOKEN"))


class Information(commands.Cog):
    def __init__(self, bot):
        self.bot = bot

    @commands.slash_command(description="The help command.")
    async def biblebot(self, inter: CommandInteraction):
        await inter.response.defer()
        resp = await backend.submit_command(inter.channel, inter.author, "+biblebot")
        await sending.safe_send_interaction(inter.followup, embed=resp)

    @commands.slash_command(description="Statistics on the bot.")
    async def stats(self, inter: CommandInteraction):
        await inter.response.defer()
        await send_stats(self.bot)
        resp = await backend.submit_command(inter.channel, inter.author, "+stats")

        await sending.safe_send_interaction(inter.followup, embed=resp)

    @commands.slash_command(description="See bot and support server invites.")
    async def invite(self, inter: CommandInteraction):
        await inter.response.defer()
        resp = await backend.submit_command(inter.channel, inter.author, "+invite")
        await sending.safe_send_interaction(inter.followup, embed=resp)

    @commands.slash_command(description="View all Patreon supporters.")
    async def supporters(self, inter: CommandInteraction):
        await inter.response.defer()
        campaigns = patreon_api.fetch_campaign()
        print(campaigns)
        campaign = campaigns.data()[0].id()
        pledges = []
        names = []
        cursor = None

        while True:
            pledges_resp = patreon_api.fetch_page_of_pledges(
                campaign, 25, cursor=cursor
            )
            pledges += pledges_resp.data()
            cursor = patreon_api.extract_cursor(pledges_resp)
            if not cursor:
                break

        names = [
            x.relationship("patron").attribute("full_name").strip() for x in pledges
        ]

        embed = disnake.Embed()

        embed.title = "Patreon Supporters"
        embed.description = (
            "Many thanks to our Patreon supporters:\n\n**" + "**\n**".join(names) + "**"
        )
        embed.color = 6709986

        embed.set_footer(
            text="BibleBot v9.2-beta by Kerygma Digital",
            icon_url="https://i.imgur.com/hr4RXpy.png",
        )
        await sending.safe_send_interaction(inter.followup, embed=embed)


async def send_stats(bot: disnake.AutoShardedClient):
    endpoint = os.environ.get("ENDPOINT")
    token = os.environ.get("ENDPOINT_TOKEN")

    shard_count = bot.shard_count
    guild_count = len(bot.guilds)
    user_count = sum([x.member_count for x in bot.guilds])
    channel_count = sum([len(x.channels) for x in bot.guilds])
    repo_sha = (
        subprocess.check_output(["git", "rev-parse", "HEAD"]).decode("ascii").strip()
    )

    async with aiohttp.ClientSession() as session:
        async with session.post(
            f"{endpoint}/stats/process",
            json={
                "Token": token,
                "Body": f"{shard_count}||{guild_count}||{user_count}||{channel_count}||{repo_sha}",
            },
        ) as resp:
            if resp.status != 200:
                logger.error("couldn't submit stats to backend")
