"""
    Copyright (C) 2016-2025 Kerygma Digital Co.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this file,
    You can obtain one at https://mozilla.org/MPL/2.0/.
"""

from disnake import CommandInteraction, Localized
import disnake
from disnake.ext import commands
from logger import VyLogger
from utils import backend, sending
from utils.paginator import CreatePaginator
from utils.i18n import i18n as i18n_class

i18n = i18n_class()

logger = VyLogger("default")

Language = commands.Param(
    choices={
        "English (UK)": "en-GB",
        "English (US)": "en-US",
        "Esperanto": "eo",
        "Polski": "pl-PL",
    }
)


class Languages(commands.Cog):
    def __init__(self, bot):
        self.bot = bot

    @commands.slash_command(description=Localized(key="CMD_LANGUAGE_DESC"))
    async def language(self, inter: CommandInteraction):
        await inter.response.defer()
        resp = await backend.submit_command(inter.channel, inter.author, "+language")
        await sending.safe_send_interaction(inter.followup, embed=resp)

    @commands.slash_command(description=Localized(key="CMD_SETLANGUAGE_DESC"))
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

    @commands.slash_command(description=Localized(key="CMD_SETSERVERLANGUAGE_DESC"))
    async def setserverlanguage(
        self,
        inter: CommandInteraction,
        language: str = Language,
    ):
        await inter.response.defer()

        localization = i18n.get_i18n_or_default(inter.locale.name)

        if hasattr(inter.channel, "permissions_for") and callable(
            inter.channel.permissions_for
        ):
            if not inter.channel.permissions_for(inter.author).manage_guild:
                await sending.safe_send_interaction(
                    inter.followup,
                    embed=backend.create_error_embed(
                        localization["PERMS_ERROR_LABEL"],
                        localization["PERMS_ERROR_DESC"],
                        localization,
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
                    "/setserverlanguage", localization["CMD_NODMS"], localization
                ),
                ephemeral=True,
            )
            return

    @commands.slash_command(description=Localized(key="CMD_LISTLANGUAGES_DESC"))
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
