"""
    Copyright (C) 2016-2022 Kerygma Digital Co.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this file,
    You can obtain one at https://mozilla.org/MPL/2.0/.
"""

import disnake
from disnake import AppCommandInteraction, CommandInteraction
from disnake.ext import commands
from logger import VyLogger
from utils import backend
import asyncio

logger = VyLogger("default")


class VersionListSelect(disnake.ui.Select):
    def __init__(self, author_id: int, is_server: bool) -> None:
        self.author_id = author_id
        self.custom_id = "version " + ("setserver" if is_server else "set")
        options = [
            # todo: populate this programatically
            disnake.SelectOption(label="King James Version (KJV)", value="KJV"),
            disnake.SelectOption(
                label="Revised Standard Version (RSV)",
                value="RSV",
            ),
        ]
        super().__init__(
            custom_id=self.custom_id, placeholder="Select a version...", options=options
        )

    async def callback(self, inter: disnake.MessageInteraction) -> None:
        if inter.author.id != self.author_id:
            return

        value = inter.values[0]

        resp = await backend.submit_command(
            inter.channel, inter.author, f"+{self.custom_id} {value}"
        )

        await inter.message.edit(embed=resp, components=None, content=None)

    async def on_error(
        self, error: Exception, inter: disnake.MessageInteraction
    ) -> None:
        await inter.message.edit(
            content="Error occurred, version settings have not changed.",
            components=None,
        )


class Versions(commands.Cog):
    def __init__(self, bot):
        self.bot = bot

    @commands.slash_command(description="See your version preferences.")
    async def version(self, inter: CommandInteraction):
        resp = await backend.submit_command(inter.channel, inter.author, "+version")
        await inter.response.send_message(embed=resp)

    @commands.slash_command(description="Set your preferred version.")
    async def setversion(self, inter: CommandInteraction):
        select_menu_view = disnake.ui.View(timeout=180)
        select_menu_view.add_item(VersionListSelect(inter.author.id, False))

        await inter.response.send_message(
            content="Select your preferred version.",
            view=select_menu_view,
        )

    @commands.slash_command(description="Set your server's preferred version.")
    @commands.has_permissions(manage_guild=True)
    async def setserverversion(self, inter: CommandInteraction):
        select_menu_view = disnake.ui.View(timeout=180)
        select_menu_view.add_item(VersionListSelect(inter.author.id, True))

        await inter.response.send_message(
            content="Select your server's preferred version.",
            view=select_menu_view,
        )

    @commands.slash_command(description="See information on a version.")
    async def versioninfo(
        self,
        inter: CommandInteraction,
        abbv: str = commands.Param(description="The abbreviation of the version."),
    ):
        resp = await backend.submit_command(
            inter.channel, inter.author, f"+version info {abbv}"
        )
        await inter.response.send_message(embed=resp)

    @commands.slash_command(description="List all available versions.")
    async def listversions(self, inter: CommandInteraction):
        # todo: get all versions and format them
        await inter.response.send_message(content="<version list here>")
