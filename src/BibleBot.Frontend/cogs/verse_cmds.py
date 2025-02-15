"""
    Copyright (C) 2016-2025 Kerygma Digital Co.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this file,
    You can obtain one at https://mozilla.org/MPL/2.0/.
"""

from disnake import CommandInteraction, Localized, OptionChoice
from disnake.ext import commands
import disnake
from setuptools import Command
from logger import VyLogger
from utils import backend, sending
from utils.paginator import CreatePaginator
from utils.i18n import i18n as i18n_class

i18n = i18n_class()

logger = VyLogger("default")


class VerseCommands(commands.Cog):
    def __init__(self, bot):
        self.bot = bot

    @commands.slash_command(description=Localized(key="CMD_SEARCH_DESC"))
    async def search(
        self,
        inter: CommandInteraction,
        query: str,
        subset: str = commands.Param(
            choices=[
                OptionChoice(
                    Localized("SEARCH_SUBSET_OT", key="SEARCH_SUBSET_OT"), "1"
                ),
                OptionChoice(
                    Localized("SEARCH_SUBSET_NT", key="SEARCH_SUBSET_NT"), "2"
                ),
                OptionChoice(
                    Localized("SEARCH_SUBSET_DEU", key="SEARCH_SUBSET_DEU"), "3"
                ),
            ],
            default="0",
        ),
        version: str = "null",
    ):
        await inter.response.defer()
        resp = await backend.submit_command(
            inter.channel,
            inter.author,
            f"+search subset:{subset} version:{version} {query}",
        )

        if isinstance(resp, list):
            await sending.safe_send_interaction(
                inter.followup,
                embed=resp[0],
                view=CreatePaginator(resp, inter.author.id, 180),
            )
        else:
            await sending.safe_send_interaction(inter.followup, embed=resp)

    @commands.slash_command(description=Localized(key="CMD_RANDOM_DESC"))
    async def random(self, inter: CommandInteraction):
        await inter.response.defer()

        resp = await backend.submit_command(inter.channel, inter.author, "+random")

        if isinstance(resp, str):
            await sending.safe_send_interaction(inter.followup, content=resp)
        else:
            await sending.safe_send_interaction(inter.followup, embed=resp)

    @commands.slash_command(description=Localized(key="CMD_TRUERANDOM_DESC"))
    async def truerandom(self, inter: CommandInteraction):
        await inter.response.defer()

        resp = await backend.submit_command(inter.channel, inter.author, "+random true")

        if isinstance(resp, str):
            await sending.safe_send_interaction(inter.followup, content=resp)
        else:
            await sending.safe_send_interaction(inter.followup, embed=resp)

    @commands.slash_command(description=Localized(key="CMD_DAILYVERSE_DESC"))
    async def dailyverse(self, inter: CommandInteraction):
        await inter.response.defer()
        resp = await backend.submit_command(inter.channel, inter.author, "+dailyverse")

        if isinstance(resp, str):
            await sending.safe_send_interaction(inter.followup, content=resp)
        else:
            await sending.safe_send_interaction(inter.followup, embed=resp)

    @commands.slash_command(description=Localized(key="CMD_SETDAILYVERSE_DESC"))
    async def setdailyverse(
        self, inter: CommandInteraction, time: str = "", tz: str = ""
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

            resp = None
            if time is None or tz is None:
                resp = await backend.submit_command(
                    inter.channel, inter.author, "+dailyverse set"
                )
            else:
                resp = await backend.submit_command(
                    inter.channel, inter.author, f"+dailyverse set {time} {tz}"
                )

            await sending.safe_send_interaction(inter.followup, embed=resp)
        else:
            await sending.safe_send_interaction(
                inter.followup,
                embed=backend.create_error_embed(
                    "/setdailyverse",
                    localization["AUTOMATIC_DAILY_VERSE_NODMS"],
                    localization,
                ),
                ephemeral=True,
            )
            return

    @commands.slash_command(description=Localized(key="CMD_DAILYVERSESTATUS_DESC"))
    async def dailyversestatus(self, inter: CommandInteraction):
        await inter.response.defer()

        localization = i18n.get_i18n_or_default(inter.locale.name)

        if hasattr(inter.channel, "permissions_for") and callable(
            inter.channel.permissions_for
        ):
            resp = await backend.submit_command(
                inter.channel, inter.author, "+dailyverse status"
            )

            await sending.safe_send_interaction(inter.followup, embed=resp)
        else:
            await sending.safe_send_interaction(
                inter.followup,
                embed=backend.create_error_embed(
                    "/dailyversestatus",
                    localization["AUTOMATIC_DAILY_VERSE_NODMS"],
                    localization,
                ),
                ephemeral=True,
            )
            return

    @commands.slash_command(description=Localized(key="CMD_CLEARDAILYVERSE_DESC"))
    async def cleardailyverse(self, inter: CommandInteraction):
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
                inter.channel, inter.author, "+dailyverse clear"
            )

            await sending.safe_send_interaction(inter.followup, embed=resp)
        else:
            await sending.safe_send_interaction(
                inter.followup,
                embed=backend.create_error_embed(
                    "/cleardailyverse",
                    localization["AUTOMATIC_DAILY_VERSE_NODMS"],
                    localization,
                ),
                ephemeral=True,
            )
            return

    @commands.slash_command(description=Localized(key="CMD_SETDAILYVERSEROLE_DESC"))
    async def setdailyverserole(self, inter: CommandInteraction, role: disnake.Role):
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

            if not role.mentionable:
                await sending.safe_send_interaction(
                    inter.followup,
                    embed=backend.create_error_embed(
                        "/setdailyverserole",
                        localization["SETDAILYVERSEROLE_UNMENTIONABLE"],
                        localization,
                    ),
                    ephemeral=True,
                )

            resp = await backend.submit_command(
                inter.channel, inter.author, f"+dailyverse role {role.id}"
            )

            await sending.safe_send_interaction(inter.followup, embed=resp)
        else:
            await sending.safe_send_interaction(
                inter.followup,
                embed=backend.create_error_embed(
                    "/setdailyverserole",
                    localization["AUTOMATIC_DAILY_VERSE_NODMS"],
                    localization,
                ),
                ephemeral=True,
            )
            return
