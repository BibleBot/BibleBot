"""
    Copyright (C) 2016-2025 Kerygma Digital Co.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this file,
    You can obtain one at https://mozilla.org/MPL/2.0/.
"""

from disnake import CommandInteraction
import disnake
from disnake.ext import commands
from logger import VyLogger
from utils import backend, sending
from utils.paginator import CreatePaginator

import os

logger = VyLogger("default")

Language = commands.Param(
    choices={
        "English (US)": "en-US",
        "Esperanto": "eo",
    }
)


class Languages(commands.Cog):
    def __init__(self, bot):
        self.bot = bot

    @commands.slash_command(description="See your language preferences.")
    async def language(self, inter: CommandInteraction):
        await inter.response.defer()
        resp = await backend.submit_command(inter.channel, inter.author, "+language")
        await sending.safe_send_interaction(inter.followup, embed=resp)

    @commands.slash_command(description="Set your preferred language.")
    async def setlanguage(
        self,
        inter: CommandInteraction,
        language: str = Language,
    ):
        await inter.response.defer()
        resp = await backend.submit_command(
            inter.channel, inter.author, f"+language set {language}"
        )
        await sending.safe_send_interaction(inter.followup, embed=resp)

    @commands.slash_command(description="Set your server's preferred language.")
    async def setserverlanguage(
        self,
        inter: CommandInteraction,
        language: str = Language,
    ):
        await inter.response.defer()

        if hasattr(inter.channel, "permissions_for") and callable(
            inter.channel.permissions_for
        ):
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
                inter.channel, inter.author, f"+language setserver {language}"
            )
            await sending.safe_send_interaction(inter.followup, embed=resp)
        else:
            await sending.safe_send_interaction(
                inter.followup,
                embed=backend.create_error_embed(
                    "/setserverlanguage",
                    "This command can only be used in a server.",
                ),
                ephemeral=True,
            )
            return

    @commands.slash_command(description="List all available languages.")
    async def listlanguages(self, inter: CommandInteraction):
        await inter.response.defer()
        resp = await backend.submit_command(
            inter.channel, inter.author, "+language list"
        )

        if isinstance(resp, list):
            await sending.safe_send_interaction(
                inter.followup,
                embed=resp[0],
                view=CreatePaginator(resp, inter.author.id, 180),
            )
        else:
            await sending.safe_send_interaction(inter.followup, embed=resp)
