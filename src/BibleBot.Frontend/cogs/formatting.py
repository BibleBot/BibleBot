"""
Copyright (C) 2016-2025 Kerygma Digital Co.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this file,
You can obtain one at https://mozilla.org/MPL/2.0/.
"""

import disnake
from disnake import CommandInteraction, Localized
from disnake.ext import commands
from logger import VyLogger
from utils import backend, sending, checks
from utils.i18n import i18n as i18n_class

i18n = i18n_class()

logger = VyLogger("default")


class DisplayStyleSelect(disnake.ui.StringSelect):
    def __init__(self, author_id: int, is_server: bool, loc) -> None:
        self.author_id = author_id
        self.custom_id = "formatting " + (
            "setserverdisplay" if is_server else "setdisplay"
        )
        self.is_ephemeral = False

        options = [
            disnake.SelectOption(
                label=loc["EMBED_BLOCKS_LABEL"],
                value="embed",
                description=loc["EMBED_BLOCKS_DESC"],
            ),
            disnake.SelectOption(
                label=loc["CODE_BLOCKS_LABEL"],
                value="code",
                description=loc["CODE_BLOCKS_DESC"],
            ),
            disnake.SelectOption(
                label=loc["BLOCKQUOTES_LABEL"],
                value="blockquote",
                description=loc["BLOCKQUOTES_DESC"],
            ),
        ]
        super().__init__(
            custom_id=self.custom_id,
            placeholder=loc["SELECT_DISPLAY_STYLE"],
            options=options,
        )

    async def callback(self, inter: disnake.MessageInteraction) -> None:
        if inter.author.id != self.author_id:
            return

        value = inter.values[0]

        resp = await backend.submit_command(
            inter.channel, inter.author, f"+{self.custom_id} {value}"
        )

        await inter.response.edit_message(embed=resp, components=None, content=None)  # type: ignore

    async def on_error(
        self, error: Exception, inter: disnake.MessageInteraction
    ) -> None:
        localization = i18n.get_i18n_or_default(inter.locale.name)

        await inter.response.edit_message(
            content=localization["DISPLAY_STYLE_FAILURE"],
            components=None,
        )


class BracketsSelect(disnake.ui.StringSelect):
    def __init__(self, author_id: int, loc: dict[str, str]) -> None:
        self.author_id = author_id
        self.custom_id = "formatting setbrackets"
        options = [
            disnake.SelectOption(
                label=loc["ANGLE_BRACKETS_LABEL"],
                value="<>",
                description=loc["ANGLE_BRACKETS_DESC"],
            ),
            disnake.SelectOption(
                label=loc["SQUARE_BRACKETS_LABEL"],
                value="[]",
                description=loc["SQUARE_BRACKETS_DESC"],
            ),
            disnake.SelectOption(
                label=loc["CURLY_BRACKETS_LABEL"],
                value="{}",
                description=loc["CURLY_BRACKETS_DESC"],
            ),
            disnake.SelectOption(
                label=loc["PARENTHESIS_LABEL"],
                value="()",
                description=loc["PARENTHESIS_DESC"],
            ),
        ]
        super().__init__(
            custom_id=self.custom_id,
            placeholder=loc["SELECT_BRACKETS"],
            options=options,
        )

    async def callback(self, inter: disnake.MessageInteraction) -> None:
        if inter.author.id != self.author_id:
            return

        value = inter.values[0]

        resp = await backend.submit_command(
            inter.channel, inter.author, f"+{self.custom_id} {value}"
        )

        await inter.response.edit_message(embed=resp, components=None, content=None)  # type: ignore

    async def on_error(
        self, error: Exception, inter: disnake.MessageInteraction
    ) -> None:
        localization = i18n.get_i18n_or_default(inter.locale.name)

        await inter.response.edit_message(
            content=localization["BRACKETS_FAILURE"],
            components=None,
        )


class Formatting(commands.Cog):
    def __init__(self, bot):
        self.bot = bot

    @commands.slash_command(description=Localized(key="CMD_FORMATTING_DESC"))
    async def formatting(self, inter: CommandInteraction):
        await inter.response.defer(ephemeral=checks.inter_is_user(inter))
        resp = await backend.submit_command(inter.channel, inter.author, "+formatting")
        await sending.safe_send_interaction(inter.followup, embed=resp)

    @commands.slash_command(description=Localized(key="CMD_SETVERSENUMBERS_DESC"))
    async def setversenumbers(
        self,
        inter: CommandInteraction,
        val: str = commands.Param(
            choices=[
                disnake.OptionChoice(
                    Localized("TOGGLE_PARAM_ENABLE", key="TOGGLE_PARAM_ENABLE"),
                    "enable",
                ),
                disnake.OptionChoice(
                    Localized("TOGGLE_PARAM_DISABLE", key="TOGGLE_PARAM_DISABLE"),
                    "disable",
                ),
            ]
        ),
    ):
        await inter.response.defer(ephemeral=checks.inter_is_user(inter))
        resp = await backend.submit_command(
            inter.channel, inter.author, f"+formatting setversenumbers {val}"
        )

        await sending.safe_send_interaction(inter.followup, embed=resp)

    @commands.slash_command(description=Localized(key="CMD_SETTITLES_DESC"))
    async def settitles(
        self,
        inter: CommandInteraction,
        val: str = commands.Param(
            choices=[
                disnake.OptionChoice(
                    Localized("TOGGLE_PARAM_ENABLE", key="TOGGLE_PARAM_ENABLE"),
                    "enable",
                ),
                disnake.OptionChoice(
                    Localized("TOGGLE_PARAM_DISABLE", key="TOGGLE_PARAM_DISABLE"),
                    "disable",
                ),
            ]
        ),
    ):
        await inter.response.defer(ephemeral=checks.inter_is_user(inter))
        resp = await backend.submit_command(
            inter.channel, inter.author, f"+formatting settitles {val}"
        )

        await sending.safe_send_interaction(inter.followup, embed=resp)

    @commands.slash_command(description=Localized(key="CMD_SETPAGINATION_DESC"))
    async def setpagination(
        self,
        inter: CommandInteraction,
        val: str = commands.Param(
            choices=[
                disnake.OptionChoice(
                    Localized("TOGGLE_PARAM_ENABLE", key="TOGGLE_PARAM_ENABLE"),
                    "enable",
                ),
                disnake.OptionChoice(
                    Localized("TOGGLE_PARAM_DISABLE", key="TOGGLE_PARAM_DISABLE"),
                    "disable",
                ),
            ]
        ),
    ):
        await inter.response.defer(ephemeral=checks.inter_is_user(inter))
        resp = await backend.submit_command(
            inter.channel, inter.author, f"+formatting setpagination {val}"
        )

        await sending.safe_send_interaction(inter.followup, embed=resp)

    @commands.slash_command(description=Localized(key="CMD_SETDISPLAY_DESC"))
    async def setdisplay(self, inter: CommandInteraction, style: str = ""):
        await inter.response.defer(ephemeral=checks.inter_is_user(inter))
        localization = i18n.get_i18n_or_default(inter.locale.name)

        if style == "":
            select_menu_view = disnake.ui.View(timeout=180)
            select_menu_view.add_item(
                DisplayStyleSelect(inter.author.id, False, localization)
            )

            await sending.safe_send_interaction(
                inter.followup,
                content=localization["SELECT_DISPLAY_STYLE"],
                view=select_menu_view,
            )
        else:
            resp = await backend.submit_command(
                inter.channel, inter.author, f"+formatting setdisplay {style}"
            )

            await sending.safe_send_interaction(inter.followup, embed=resp)

    @commands.slash_command(description=Localized(key="CMD_SETSERVERDISPLAY_DESC"))
    @commands.install_types(guild=True)
    async def setserverdisplay(self, inter: CommandInteraction, style: str = ""):
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

            if style == "":
                select_menu_view = disnake.ui.View(timeout=180)
                select_menu_view.add_item(
                    DisplayStyleSelect(inter.author.id, True, localization)
                )

                await sending.safe_send_interaction(
                    inter.followup,
                    content=localization["SELECT_DISPLAY_STYLE"],
                    view=select_menu_view,
                )
            else:
                resp = await backend.submit_command(
                    inter.channel, inter.author, f"+formatting setserverdisplay {style}"
                )

                await sending.safe_send_interaction(inter.followup, embed=resp)
        else:
            await sending.safe_send_interaction(
                inter.followup,
                embed=backend.create_error_embed(
                    localization["PERMS_ERROR_LABEL"],
                    localization["CMD_NODMS"],
                    localization,
                ),
                ephemeral=True,
            )
            return

    @commands.slash_command(description=Localized(key="CMD_SETBRACKETS_DESC"))
    @commands.install_types(guild=True)
    async def setbrackets(self, inter: CommandInteraction, brackets: str = ""):
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

            if brackets == "":
                select_menu_view = disnake.ui.View(timeout=180)
                select_menu_view.add_item(BracketsSelect(inter.author.id, localization))

                await sending.safe_send_interaction(
                    inter.followup,
                    content=localization["SELECT_BRACKETS"],
                    view=select_menu_view,
                )
            else:
                resp = await backend.submit_command(
                    inter.channel, inter.author, f"+formatting setbrackets {brackets}"
                )

                await sending.safe_send_interaction(inter.followup, embed=resp)
        else:
            await sending.safe_send_interaction(
                inter.followup,
                embed=backend.create_error_embed(
                    "/setbrackets", localization["CMD_NODMS"], localization
                ),
                ephemeral=True,
            )
            return
