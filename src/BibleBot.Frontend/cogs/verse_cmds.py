"""
    Copyright (C) 2016-2024 Kerygma Digital Co.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this file,
    You can obtain one at https://mozilla.org/MPL/2.0/.
"""

from disnake import CommandInteraction
from disnake.ext import commands
import disnake
from setuptools import Command
from logger import VyLogger
from utils import backend, sending
from utils.paginator import CreatePaginator

logger = VyLogger("default")


class VerseCommands(commands.Cog):
    def __init__(self, bot):
        self.bot = bot

    @commands.slash_command(description="Search for verses by keyword.")
    async def search(self, inter: CommandInteraction, query: str):
        await inter.response.defer()
        resp = await backend.submit_command(
            inter.channel, inter.author, f"+search {query}"
        )

        if isinstance(resp, list):
            await sending.safe_send_interaction(
                inter.followup,
                embed=resp[0],
                view=CreatePaginator(resp, inter.author.id, 180),
            )
        else:
            await sending.safe_send_interaction(inter.followup, embed=resp)

    @commands.slash_command(
        description="Display a random verse from a predetermined pool."
    )
    async def random(self, inter: CommandInteraction):
        await inter.response.defer()

        # /r/Catholicism has personally requested that random commands be used in DMs.
        if inter.guild_id == 238001909716353025:
            err = backend.create_error_embed(
                "/random",
                "This server has personally requested that this command be only used in DMs to avoid spam.",
            )
            await sending.safe_send_interaction(inter.followup, embed=err)
            return

        resp = await backend.submit_command(inter.channel, inter.author, "+random")

        if isinstance(resp, str):
            await sending.safe_send_interaction(inter.followup, content=resp)
        else:
            await sending.safe_send_interaction(inter.followup, embed=resp)

    @commands.slash_command(
        description="Display a random verse based on random number generation."
    )
    async def truerandom(self, inter: CommandInteraction):
        await inter.response.defer()

        # /r/Catholicism has personally requested that random commands be used in DMs.
        if inter.guild_id == 238001909716353025:
            err = backend.create_error_embed(
                "/random",
                "This server has personally requested that this command be only used in DMs to avoid spam.",
            )
            await sending.safe_send_interaction(inter.followup, embed=err)
            return

        resp = await backend.submit_command(inter.channel, inter.author, "+random true")

        if isinstance(resp, str):
            await sending.safe_send_interaction(inter.followup, content=resp)
        else:
            await sending.safe_send_interaction(inter.followup, embed=resp)

    @commands.slash_command(description="Display the verse of the day.")
    async def dailyverse(self, inter: CommandInteraction):
        await inter.response.defer()
        resp = await backend.submit_command(inter.channel, inter.author, "+dailyverse")

        if isinstance(resp, str):
            await sending.safe_send_interaction(inter.followup, content=resp)
        else:
            await sending.safe_send_interaction(inter.followup, embed=resp)

    @commands.slash_command(description="Setup automatic daily verses on this channel.")
    async def dailyverseset(
        self, inter: CommandInteraction, time: str = "", tz: str = ""
    ):
        await inter.response.defer()

        if not inter.channel.permissions_for(inter.author).manage_guild:
            await sending.safe_send_interaction(
                inter.followup,
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

        await sending.safe_send_interaction(inter.followup, embed=resp)

    @commands.slash_command(
        description="See automatic daily verse status for this server."
    )
    async def dailyversestatus(self, inter: CommandInteraction):
        await inter.response.defer()
        resp = await backend.submit_command(
            inter.channel, inter.author, "+dailyverse status"
        )

        await sending.safe_send_interaction(inter.followup, embed=resp)

    @commands.slash_command(
        description="Clear all automatic daily verse preferences for this server."
    )
    async def dailyverseclear(self, inter: CommandInteraction):
        await inter.response.defer()

        if not inter.channel.permissions_for(inter.author).manage_guild:
            await sending.safe_send_interaction(
                inter.followup,
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

        await sending.safe_send_interaction(inter.followup, embed=resp)

    @commands.slash_command(
        description="Set a role to be @mention'd with every automatic daily verse."
    )
    async def dailyverserole(self, inter: CommandInteraction, role: disnake.Role):
        await inter.response.defer()

        if not inter.channel.permissions_for(inter.author).manage_guild:
            await sending.safe_send_interaction(
                inter.followup,
                embed=backend.create_error_embed(
                    "Permissions Error",
                    "You must have the `Manage Server` permission to use this command.",
                ),
                ephemeral=True,
            )
            return

        if not role.mentionable:
            await sending.safe_send_interaction(
                inter.followup,
                embed=backend.create_error_embed(
                    "/dailyverserole",
                    "This role is unmentionable. Please enable `Allow anyone to @mention this role` within the role's permissions.",
                ),
                ephemeral=True,
            )

        resp = await backend.submit_command(
            inter.channel, inter.author, f"+dailyverse role {role.id}"
        )

        await sending.safe_send_interaction(inter.followup, embed=resp)
