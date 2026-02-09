"""
Copyright (C) 2016-2026 Kerygma Digital Co.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this file,
You can obtain one at https://mozilla.org/MPL/2.0/.
"""

from core import checks
from core.i18n import bb_i18n
from disnake import Localized
from disnake.ext import commands
from disnake.interactions import ApplicationCommandInteraction
from helpers import sending
from logger import VyLogger
from services import backend
from ui import renderers as containers
from ui.paginator import ComponentPaginator

i18n = bb_i18n()

logger = VyLogger("default")

Language = commands.Param(
    choices={
        "English (UK)": "en-GB",
        "English (US)": "en-US",
        "Esperanto": "eo-UY",
        "Polski": "pl-PL",
    }
)


class Languages(commands.Cog):
    def __init__(self, bot):
        self.bot = bot

    @commands.slash_command(description=Localized(key="CMD_LANGUAGE_DESC"))
    async def language(self, inter: ApplicationCommandInteraction):
        await inter.response.defer(ephemeral=checks.inter_is_user(inter))
        resp = await backend.submit_command(inter.channel, inter.author, "+language")
        await sending.safe_send_interaction(inter.followup, components=resp)

    @commands.slash_command(description=Localized(key="CMD_SETLANGUAGE_DESC"))
    async def setlanguage(
        self,
        inter: ApplicationCommandInteraction,
        language: str = Language,  # TODO: add description to param
    ):
        await inter.response.defer(ephemeral=checks.inter_is_user(inter))
        resp = await backend.submit_command(
            inter.channel, inter.author, f"+language set {language}"
        )
        await sending.safe_send_interaction(inter.followup, components=resp)

    @commands.slash_command(description=Localized(key="CMD_SETSERVERLANGUAGE_DESC"))
    @commands.install_types(guild=True)
    @commands.contexts(guild=True, bot_dm=False, private_channel=False)
    async def setserverlanguage(
        self,
        inter: ApplicationCommandInteraction,
        language: str = Language,  # TODO: add description to param
    ):
        await inter.response.defer()

        localization = i18n.get_i18n_or_default(inter.locale.name)

        if checks.inter_is_not_dm(inter):
            if not checks.author_has_manage_server_permission(inter):
                await sending.safe_send_interaction(
                    inter.followup,
                    components=containers.create_error_container(
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
            await sending.safe_send_interaction(inter.followup, components=resp)
        else:
            await sending.safe_send_interaction(
                inter.followup,
                components=containers.create_error_container(
                    "/setserverlanguage", localization["CMD_NODMS"], localization
                ),
                ephemeral=True,
            )
            return

    @commands.slash_command(description=Localized(key="CMD_LISTLANGUAGES_DESC"))
    async def listlanguages(self, inter: ApplicationCommandInteraction):
        await inter.response.defer(ephemeral=checks.inter_is_user(inter))
        resp = await backend.submit_command(
            inter.channel, inter.author, "+language list"
        )

        if isinstance(resp, list):
            paginator = ComponentPaginator(resp, inter.author.id)
            await paginator.send(inter)
        else:
            await sending.safe_send_interaction(inter.followup, components=resp)
