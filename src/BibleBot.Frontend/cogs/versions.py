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
from utils import backend, sending, checks
from utils.views import CreatePaginator
from utils.i18n import i18n as i18n_class

i18n = i18n_class()

import os

logger = VyLogger("default")

# -- This is all commented out since select menus can only have 25 options, sadly. -- #
#
# import pymongo
# _mongocli = pymongo.MongoClient(os.environ.get("MONGODB_CONN"))
# _vdb = _mongocli.BibleBotBackend
# _versions = _vdb.Versions.find().sort("Name", pymongo.ASCENDING)
# versions = [
#     disnake.SelectOption(label=x["Name"], value=x["Abbreviation"]) for x in _versions
# ]
#
#
# class VersionListSelect(disnake.ui.Select):
#     def __init__(self, author_id: int, is_server: bool) -> None:
#         self.author_id = author_id
#         self.custom_id = "version " + ("setserver" if is_server else "set")
#
#         super().__init__(
#             custom_id=self.custom_id,
#             placeholder="Select a version...",
#             options=versions,
#         )
#
#     async def callback(self, inter: disnake.MessageInteraction) -> None:
#         if inter.author.id != self.author_id:
#             return
#
#         value = inter.values[0]
#
#         resp = await backend.submit_command(
#             inter.channel, inter.author, f"+{self.custom_id} {value}"
#         )
#
#         await inter.message.edit(embed=resp, components=None, content=None)
#
#     async def on_error(
#         self, error: Exception, inter: disnake.MessageInteraction
#     ) -> None:
#         await inter.message.edit(
#             content="Error occurred, version settings have not changed.",
#             components=None,
#         )


class Versions(commands.Cog):
    def __init__(self, bot):
        self.bot = bot

    @commands.slash_command(description=Localized(key="CMD_VERSION_DESC"))
    async def version(self, inter: CommandInteraction):
        await inter.response.defer(ephemeral=checks.inter_is_user(inter))
        resp = await backend.submit_command(inter.channel, inter.author, "+version")
        await sending.safe_send_interaction(inter.followup, embed=resp)

    @commands.slash_command(description=Localized(key="CMD_SETVERSION_DESC"))
    async def setversion(
        self,
        inter: CommandInteraction,
        acronym: str = commands.Param(description=Localized(key="VERSION_PARAM")),
    ):
        await inter.response.defer(ephemeral=checks.inter_is_user(inter))
        resp = await backend.submit_command(
            inter.channel, inter.author, f"+version set {acronym}"
        )
        await sending.safe_send_interaction(inter.followup, embed=resp)

    @commands.slash_command(description=Localized(key="CMD_SETSERVERVERSION_DESC"))
    @commands.install_types(guild=True)
    async def setserverversion(
        self,
        inter: CommandInteraction,
        acronym: str = commands.Param(description=Localized(key="VERSION_PARAM")),
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
                inter.channel, inter.author, f"+version setserver {acronym}"
            )
            await sending.safe_send_interaction(inter.followup, embed=resp)
        else:
            await sending.safe_send_interaction(
                inter.followup,
                embed=backend.create_error_embed(
                    "/setserverversion", localization["CMD_NODMS"], localization
                ),
                ephemeral=True,
            )
            return

    @commands.slash_command(description=Localized(key="CMD_VERSIONINFO_DESC"))
    async def versioninfo(
        self,
        inter: CommandInteraction,
        acronym: str = commands.Param(description=Localized(key="VERSION_PARAM")),
    ):
        await inter.response.defer(ephemeral=checks.inter_is_user(inter))

        command = "+version info"

        if acronym:
            command += f" {acronym}"

        resp = await backend.submit_command(inter.channel, inter.author, command)
        await sending.safe_send_interaction(inter.followup, embed=resp)

    @commands.slash_command(description=Localized(key="CMD_LISTVERSIONS_DESC"))
    async def listversions(self, inter: CommandInteraction):
        await inter.response.defer(ephemeral=checks.inter_is_user(inter))
        resp = await backend.submit_command(
            inter.channel, inter.author, "+version list"
        )

        if isinstance(resp, list):
            await sending.safe_send_interaction(
                inter.followup,
                embed=resp[0],
                view=CreatePaginator(resp, inter.author.id, 180),
            )
        else:
            await sending.safe_send_interaction(inter.followup, embed=resp)

    @commands.slash_command(description=Localized(key="CMD_BOOKLIST_DESC"))
    async def booklist(
        self,
        inter: CommandInteraction,
        acronym: str = commands.Param(
            description=Localized(key="VERSION_PARAM"), default=None
        ),
    ):
        await inter.response.defer(ephemeral=checks.inter_is_user(inter))

        command = "+version booklist"

        if acronym:
            command += f" {acronym}"

        resp = await backend.submit_command(inter.channel, inter.author, command)
        if isinstance(resp, list):
            await sending.safe_send_interaction(
                inter.followup,
                embed=resp[0],
                view=CreatePaginator(resp, inter.author.id, 180),
            )
        else:
            await sending.safe_send_interaction(inter.followup, embed=resp)
