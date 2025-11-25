"""
Copyright (C) 2016-2025 Kerygma Digital Co.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this file,
You can obtain one at https://mozilla.org/MPL/2.0/.
"""

from disnake import Localized
from disnake.interactions import ApplicationCommandInteraction
import disnake
from disnake.ext import commands
from logger import VyLogger
from utils import backend, sending, checks, containers
from utils.paginator import ComponentPaginator
from utils.i18n import i18n as i18n_class

i18n = i18n_class()
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
    async def version(self, inter: ApplicationCommandInteraction):
        await inter.response.defer(ephemeral=checks.inter_is_user(inter))
        resp = await backend.submit_command(inter.channel, inter.author, "+version")
        await sending.safe_send_interaction(inter.followup, components=resp)

    @commands.slash_command(description=Localized(key="CMD_SETVERSION_DESC"))
    async def setversion(
        self,
        inter: ApplicationCommandInteraction,
        acronym: str = commands.Param(description=Localized(key="VERSION_PARAM")),
    ):
        await inter.response.defer(ephemeral=checks.inter_is_user(inter))
        resp = await backend.submit_command(
            inter.channel, inter.author, f"+version set {acronym}"
        )
        await sending.safe_send_interaction(inter.followup, components=resp)

    @commands.slash_command(description=Localized(key="CMD_SETSERVERVERSION_DESC"))
    @commands.install_types(guild=True)
    @commands.contexts(guild=True, bot_dm=False, private_channel=False)
    async def setserverversion(
        self,
        inter: ApplicationCommandInteraction,
        acronym: str = commands.Param(description=Localized(key="VERSION_PARAM")),
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
                inter.channel, inter.author, f"+version setserver {acronym}"
            )
            await sending.safe_send_interaction(inter.followup, components=resp)
        else:
            await sending.safe_send_interaction(
                inter.followup,
                components=containers.create_error_container(
                    "/setserverversion", localization["CMD_NODMS"], localization
                ),
                ephemeral=True,
            )
            return

    @commands.slash_command(description=Localized(key="CMD_VERSIONINFO_DESC"))
    async def versioninfo(
        self,
        inter: ApplicationCommandInteraction,
        acronym: str = commands.Param(description=Localized(key="VERSION_PARAM")),
    ):
        await inter.response.defer(ephemeral=checks.inter_is_user(inter))

        command = "+version info"

        if acronym:
            command += f" {acronym}"

        resp = await backend.submit_command(inter.channel, inter.author, command)
        await sending.safe_send_interaction(inter.followup, components=resp)

    @commands.slash_command(description=Localized(key="CMD_LISTVERSIONS_DESC"))
    async def listversions(
        self,
        inter: ApplicationCommandInteraction,
        sort_by_language: bool = commands.Param(
            description=Localized(key="LISTVERSIONS_LANGUAGE_PARAM"), default=False
        ),
    ):
        await inter.response.defer(ephemeral=checks.inter_is_user(inter))
        resp = await backend.submit_command(
            inter.channel,
            inter.author,
            "+version list" + (" language" if sort_by_language else ""),
        )

        if isinstance(resp, list):
            paginator = ComponentPaginator(resp, inter.author.id)
            await paginator.send(inter)
        else:
            await sending.safe_send_interaction(inter.followup, components=resp)

    @commands.slash_command(description=Localized(key="CMD_BOOKLIST_DESC"))
    async def booklist(
        self,
        inter: ApplicationCommandInteraction,
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
            paginator = ComponentPaginator(resp, inter.author.id)
            await paginator.send(inter)
        else:
            await sending.safe_send_interaction(inter.followup, components=resp)
