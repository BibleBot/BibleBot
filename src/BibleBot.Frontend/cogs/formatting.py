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
from utils import backend, sending, checks, views, containers
from utils.i18n import i18n as i18n_class

i18n = i18n_class()
logger = VyLogger("default")


class Formatting(commands.Cog):
    def __init__(self, bot):
        self.bot = bot

    @commands.slash_command(description=Localized(key="CMD_FORMATTING_DESC"))
    async def formatting(self, inter: CommandInteraction):
        await inter.response.defer(ephemeral=checks.inter_is_user(inter))
        resp = await backend.submit_command(inter.channel, inter.author, "+formatting")
        await sending.safe_send_interaction(inter.followup, components=resp)

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

        await sending.safe_send_interaction(inter.followup, components=resp)

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

        await sending.safe_send_interaction(inter.followup, components=resp)

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

        await sending.safe_send_interaction(inter.followup, components=resp)

    @commands.slash_command(description=Localized(key="CMD_SETDISPLAY_DESC"))
    async def setdisplay(self, inter: CommandInteraction, style: str = ""):
        await inter.response.defer(ephemeral=checks.inter_is_user(inter))
        localization = i18n.get_i18n_or_default(inter.locale.name)

        if style == "":
            await sending.safe_send_interaction(
                inter.followup,
                content=localization["SELECT_DISPLAY_STYLE"],
                view=views.DisplayStyleView(inter.author.id, localization, False),
            )
        else:
            resp = await backend.submit_command(
                inter.channel, inter.author, f"+formatting setdisplay {style}"
            )

            await sending.safe_send_interaction(inter.followup, components=resp)

    @commands.slash_command(description=Localized(key="CMD_SETSERVERDISPLAY_DESC"))
    @commands.install_types(guild=True)
    @commands.contexts(guild=True, bot_dm=False, private_channel=False)
    async def setserverdisplay(self, inter: CommandInteraction, style: str = ""):
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

            if style == "":
                await sending.safe_send_interaction(
                    inter.followup,
                    content=localization["SELECT_DISPLAY_STYLE"],
                    view=views.DisplayStyleView(inter.author.id, localization, True),
                )
            else:
                resp = await backend.submit_command(
                    inter.channel, inter.author, f"+formatting setserverdisplay {style}"
                )

                await sending.safe_send_interaction(inter.followup, components=resp)
        else:
            await sending.safe_send_interaction(
                inter.followup,
                components=containers.create_error_container(
                    localization["PERMS_ERROR_LABEL"],
                    localization["CMD_NODMS"],
                    localization,
                ),
                ephemeral=True,
            )
            return

    @commands.slash_command(description=Localized(key="CMD_SETBRACKETS_DESC"))
    @commands.install_types(guild=True)
    @commands.contexts(guild=True, bot_dm=False, private_channel=False)
    async def setbrackets(self, inter: CommandInteraction, brackets: str = ""):
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

            if brackets == "":
                await sending.safe_send_interaction(
                    inter.followup,
                    content=localization["SELECT_BRACKETS"],
                    view=views.BracketsView(inter.author.id, localization),
                )
            else:
                resp = await backend.submit_command(
                    inter.channel, inter.author, f"+formatting setbrackets {brackets}"
                )

                await sending.safe_send_interaction(inter.followup, components=resp)
        else:
            await sending.safe_send_interaction(
                inter.followup,
                components=containers.create_error_container(
                    "/setbrackets", localization["CMD_NODMS"], localization
                ),
                ephemeral=True,
            )
            return
