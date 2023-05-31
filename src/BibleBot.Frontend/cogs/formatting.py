"""
    Copyright (C) 2016-2023 Kerygma Digital Co.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this file,
    You can obtain one at https://mozilla.org/MPL/2.0/.
"""

import disnake
from disnake import CommandInteraction, Permissions
from disnake.ext import commands
from logger import VyLogger
from utils import backend, sending

logger = VyLogger("default")


class DisplayStyleSelect(disnake.ui.Select):
    def __init__(self, author_id: int, is_server: bool) -> None:
        self.author_id = author_id
        self.custom_id = "formatting " + (
            "setserverdisplay" if is_server else "setdisplay"
        )
        options = [
            disnake.SelectOption(
                label="Embed Blocks",
                value="embed",
                description="The fancy blocks that our command output comes in.",
            ),
            disnake.SelectOption(
                label="Code Blocks",
                value="code",
                description="See an example by saying ``` test ```.",
            ),
            disnake.SelectOption(
                label="Blockquotes",
                value="blockquote",
                description="See an example by saying `> test`.",
            ),
        ]
        super().__init__(
            custom_id=self.custom_id,
            placeholder="Select a display style...",
            options=options,
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
            content="Error occurred, display style settings have not changed.",
            components=None,
        )


class BracketsSelect(disnake.ui.Select):
    def __init__(self, author_id: int) -> None:
        self.author_id = author_id
        self.custom_id = "formatting setbrackets"
        options = [
            disnake.SelectOption(
                label="Angle Brackets <>",
                value="<>",
                description="References like <Genesis 1:1> will be ignored.",
            ),
            disnake.SelectOption(
                label="Square Brackets []",
                value="[]",
                description="References like [Genesis 1:1] will be ignored.",
            ),
            disnake.SelectOption(
                label="Curly Brackets {}",
                value="{}",
                description="References like {Genesis 1:1} will be ignored.",
            ),
            disnake.SelectOption(
                label="Parentheses ()",
                value="()",
                description="References like (Genesis 1:1) will be ignored.",
            ),
        ]
        super().__init__(
            custom_id=self.custom_id,
            placeholder="Select a pair of brackets...",
            options=options,
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
            content="Error occurred, bracket settings have not changed.",
            components=None,
        )


class Formatting(commands.Cog):
    def __init__(self, bot):
        self.bot = bot

    @commands.slash_command(description="See your formatting preferences.")
    async def formatting(self, inter: CommandInteraction):
        await inter.response.defer()
        resp = await backend.submit_command(inter.channel, inter.author, "+formatting")
        await sending.safe_send_interaction(inter.followup, embed=resp)

    @commands.slash_command(description="Enable or disable verse numbers.")
    async def setversenumbers(
        self,
        inter: CommandInteraction,
        val: str = commands.Param(choices={"Enable": "enable", "Disable": "disable"}),
    ):
        await inter.response.defer()
        resp = await backend.submit_command(
            inter.channel, inter.author, f"+formatting setversenumbers {val}"
        )

        await sending.safe_send_interaction(inter.followup, embed=resp)

    @commands.slash_command(description="Enable or disable headings.")
    async def settitles(
        self,
        inter: CommandInteraction,
        val: str = commands.Param(choices={"Enable": "enable", "Disable": "disable"}),
    ):
        await inter.response.defer()
        resp = await backend.submit_command(
            inter.channel, inter.author, f"+formatting settitles {val}"
        )

        await sending.safe_send_interaction(inter.followup, embed=resp)

    @commands.slash_command(description="Enable or disable verse pagination.")
    async def setpagination(
        self,
        inter: CommandInteraction,
        val: str = commands.Param(choices={"Enable": "enable", "Disable": "disable"}),
    ):
        await inter.response.defer()
        resp = await backend.submit_command(
            inter.channel, inter.author, f"+formatting setpagination {val}"
        )

        await sending.safe_send_interaction(inter.followup, embed=resp)

    @commands.slash_command(description="Set your preferred display style.")
    async def setdisplay(self, inter: CommandInteraction):
        await inter.response.defer()
        select_menu_view = disnake.ui.View(timeout=180)
        select_menu_view.add_item(DisplayStyleSelect(inter.author.id, False))

        await sending.safe_send_interaction(
            inter.followup,
            content="Select your preferred display style.",
            view=select_menu_view,
        )

    @commands.slash_command(description="Set your server's preferred display style.")
    async def setserverdisplay(self, inter: CommandInteraction):
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

        select_menu_view = disnake.ui.View(timeout=180)
        select_menu_view.add_item(DisplayStyleSelect(inter.author.id, True))

        await sending.safe_send_interaction(
            inter.followup,
            content="Select your server's preferred display style.",
            view=select_menu_view,
        )

    @commands.slash_command(
        description="Set the bot's ignoring brackets for this server."
    )
    async def setbrackets(self, inter: CommandInteraction):
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

        select_menu_view = disnake.ui.View(timeout=180)
        select_menu_view.add_item(BracketsSelect(inter.author.id))

        await sending.safe_send_interaction(
            inter.followup,
            content="Select a pair of brackets.",
            view=select_menu_view,
        )
