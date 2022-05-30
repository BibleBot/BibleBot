"""
    Copyright (C) 2016-2022 Kerygma Digital Co.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this file,
    You can obtain one at https://mozilla.org/MPL/2.0/.
"""

from disnake import CommandInteraction
from disnake.ext import commands
from setuptools import Command
from logger import VyLogger
from utils import backend
from utils.paginator import CreatePaginator

logger = VyLogger("default")


class VerseCommands(commands.Cog):
    def __init__(self, bot):
        self.bot = bot

    @commands.slash_command(description="Search for verses by keyword.")
    async def search(self, inter: CommandInteraction, query: str):
        resp = await backend.submit_command(
            inter.channel, inter.author, f"+search {query}"
        )

        if isinstance(resp, list):
            await inter.response.send_message(
                embed=resp[0], view=CreatePaginator(resp, inter.author.id, 180)
            )
        else:
            await inter.response.send_message(embed=resp)

    @commands.slash_command(
        description="Display a random verse from a predetermined pool."
    )
    async def random(self, inter: CommandInteraction):
        # /r/Catholicism has personally requested that random commands be used in DMs.
        if inter.guild_id == 238001909716353025:
            err = backend.create_error_embed(
                "/random",
                "This server has personally requested that this command be only used in DMs to avoid spam.",
            )
            await inter.response.send_message(embed=err)
            return

        resp = await backend.submit_command(inter.channel, inter.author, "+random")

        if isinstance(resp, str):
            await inter.response.send_message(content=resp)
        else:
            await inter.response.send_message(embed=resp)

    @commands.slash_command(
        description="Display a random verse based on random number generation."
    )
    async def truerandom(self, inter: CommandInteraction):
        # /r/Catholicism has personally requested that random commands be used in DMs.
        if inter.guild_id == 238001909716353025:
            err = backend.create_error_embed(
                "/random",
                "This server has personally requested that this command be only used in DMs to avoid spam.",
            )
            await inter.response.send_message(embed=err)
            return

        resp = await backend.submit_command(inter.channel, inter.author, "+random true")

        if isinstance(resp, str):
            await inter.response.send_message(content=resp)
        else:
            await inter.response.send_message(embed=resp)

    @commands.slash_command(description="Display the verse of the day.")
    async def dailyverse(self, inter: CommandInteraction):
        resp = await backend.submit_command(inter.channel, inter.author, "+dailyverse")

        if isinstance(resp, str):
            await inter.response.send_message(content=resp)
        else:
            await inter.response.send_message(embed=resp)

    @commands.slash_command(description="Setup automatic daily verses on this channel.")
    async def dailyverseset(
        self, inter: CommandInteraction, time: str = None, tz: str = None
    ):
        if not inter.channel.permissions_for(inter.author).manage_guild:
            await inter.response.send_message(
                embed=backend.create_error_embed(
                    "Permissions Error",
                    "You must have the `Manage Server` permission to use this command.",
                ),
                ephemeral=True,
            )
            return

        resp = None
        if time is None or tz is None:
            resp = await backend.submit_command(
                inter.channel, inter.author, "+dailyverse set"
            )
        else:
            resp = await backend.submit_command(
                inter.channel, inter.author, f"+dailyverse set {time} {tz}"
            )

        await inter.response.send_message(embed=resp)

    @commands.slash_command(
        description="See automatic daily verse status for this server."
    )
    async def dailyversestatus(self, inter: CommandInteraction):
        resp = await backend.submit_command(
            inter.channel, inter.author, "+dailyverse status"
        )

        await inter.response.send_message(embed=resp)

    @commands.slash_command(
        description="Clear all automatic daily verse preferences for this server."
    )
    async def dailyverseclear(self, inter: CommandInteraction):
        if not inter.channel.permissions_for(inter.author).manage_guild:
            await inter.response.send_message(
                embed=backend.create_error_embed(
                    "Permissions Error",
                    "You must have the `Manage Server` permission to use this command.",
                ),
                ephemeral=True,
            )
            return

        resp = await backend.submit_command(
            inter.channel, inter.author, "+dailyverse clear"
        )

        await inter.response.send_message(embed=resp)
