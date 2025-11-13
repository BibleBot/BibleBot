"""
Copyright (C) 2016-2025 Kerygma Digital Co.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this file,
You can obtain one at https://mozilla.org/MPL/2.0/.
"""

from disnake import CommandInteraction, Localized
from disnake.ext import commands
from logger import VyLogger
from utils import backend, sending, checks, containers
from utils.paginator import ComponentPaginator
from utils.i18n import i18n as i18n_class

i18n = i18n_class()

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
    async def language(self, inter: CommandInteraction):
        await inter.response.defer(ephemeral=checks.inter_is_user(inter))
        resp = await backend.submit_command(inter.channel, inter.author, "+language")
        await sending.safe_send_interaction(inter.followup, components=resp)

    @commands.slash_command(description=Localized(key="CMD_SETLANGUAGE_DESC"))
    async def setlanguage(
        self,
        inter: CommandInteraction,
        language: str = Language,
    ):
        await inter.response.defer(ephemeral=checks.inter_is_user(inter))
        resp = await backend.submit_command(
            inter.channel, inter.author, f"+language set {language}"
        )
        await sending.safe_send_interaction(inter.followup, components=resp)

    @commands.slash_command(description=Localized(key="CMD_SETSERVERLANGUAGE_DESC"))
    @commands.install_types(guild=True)
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
    async def listlanguages(self, inter: CommandInteraction):
        await inter.response.defer(ephemeral=checks.inter_is_user(inter))
        resp = await backend.submit_command(
            inter.channel, inter.author, "+language list"
        )

        if isinstance(resp, list):
            paginator = ComponentPaginator(resp, inter.author.id)
            await paginator.send(inter)
        else:
            await sending.safe_send_interaction(inter.followup, components=resp)
