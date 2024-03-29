"""
    Copyright (C) 2016-2022 Kerygma Digital Co.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this file,
    You can obtain one at https://mozilla.org/MPL/2.0/.
"""

import disnake
from disnake import CommandInteraction
from disnake.ext import commands
from logger import VyLogger
from utils import backend
from utils.paginator import CreatePaginator

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

    @commands.slash_command(description="See your version preferences.")
    async def version(self, inter: CommandInteraction):
        resp = await backend.submit_command(inter.channel, inter.author, "+version")
        await inter.response.send_message(embed=resp)

    @commands.slash_command(description="Set your preferred version.")
    async def setversion(
        self,
        inter: CommandInteraction,
        abbreviation: str = commands.Param(
            description="The abbreviation of the version."
        ),
    ):
        resp = await backend.submit_command(
            inter.channel, inter.author, f"+version set {abbreviation}"
        )
        await inter.response.send_message(embed=resp)

    @commands.slash_command(description="Set your server's preferred version.")
    async def setserverversion(
        self,
        inter: CommandInteraction,
        abbreviation: str = commands.Param(
            description="The abbreviation of the version."
        ),
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

        resp = await backend.submit_command(
            inter.channel, inter.author, f"+version setserver {abbreviation}"
        )
        await inter.response.send_message(embed=resp)

    @commands.slash_command(description="See information on a version.")
    async def versioninfo(
        self,
        inter: CommandInteraction,
        abbreviation: str = commands.Param(
            description="The abbreviation of the version."
        ),
    ):
        resp = await backend.submit_command(
            inter.channel, inter.author, f"+version info {abbreviation}"
        )
        await inter.response.send_message(embed=resp)

    @commands.slash_command(description="List all available versions.")
    async def listversions(self, inter: CommandInteraction):
        resp = await backend.submit_command(
            inter.channel, inter.author, "+version list"
        )

        await inter.response.send_message(
            embed=resp[0], view=CreatePaginator(resp, inter.author.id, 180)
        )
