"""
Copyright (C) 2016-2025 Kerygma Digital Co.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this file,
You can obtain one at https://mozilla.org/MPL/2.0/.
"""

from disnake import CommandInteraction, Localized, OptionChoice
from disnake.ext import commands
from setuptools import Command
from logger import VyLogger
from utils import backend, sending, checks
from utils.views import CreatePaginator

logger = VyLogger("default")


class Resources(commands.Cog):
    def __init__(self, bot):
        self.bot = bot

    @commands.slash_command(description=Localized(key="CMD_LISTRESOURCES_DESC"))
    async def listresources(self, inter: CommandInteraction):
        await inter.response.defer(ephemeral=checks.inter_is_user(inter))
        resp = await backend.submit_command(inter.channel, inter.author, f"+resource")
        await sending.safe_send_interaction(inter.followup, embed=resp)

    @commands.slash_command(description=Localized(key="CMD_RESOURCE_DESC"))
    async def resource(
        self,
        inter: CommandInteraction,
        resource: str = commands.Param(
            choices=[
                OptionChoice(
                    Localized("RESOURCE_CIC_TITLE", key="RESOURCE_CIC_TITLE"), "cic"
                ),
                OptionChoice(
                    Localized("RESOURCE_CCEO_TITLE", key="RESOURCE_CCEO_TITLE"), "cceo"
                ),
                OptionChoice(
                    Localized("RESOURCE_CCC_TITLE", key="RESOURCE_CCC_TITLE"), "ccc"
                ),
                OptionChoice(
                    Localized("RESOURCE_LSC_TITLE", key="RESOURCE_LSC_TITLE"), "lsc"
                ),
                OptionChoice(
                    Localized("RESOURCE_APOSTLES_TITLE", key="RESOURCE_APOSTLES_TITLE"),
                    "apostles",
                ),
                OptionChoice(
                    Localized(
                        "RESOURCE_CHALCEDON_TITLE", key="RESOURCE_CHALCEDON_TITLE"
                    ),
                    "chalcedon",
                ),
                OptionChoice(
                    Localized(
                        "RESOURCE_NICENE325_TITLE", key="RESOURCE_NICENE325_TITLE"
                    ),
                    "nicene325",
                ),
                OptionChoice(
                    Localized("RESOURCE_NICENE_TITLE", key="RESOURCE_NICENE_TITLE"),
                    "nicene",
                ),
            ]
        ),
        input: str = "",
    ):
        await inter.response.defer()
        cmd = f"+resource {resource}"

        if input:
            cmd += f" {input}"

        resp = await backend.submit_command(inter.channel, inter.author, cmd)

        if isinstance(resp, list):
            if resource in ["lsc"] or len(resp) > 3:
                await sending.safe_send_interaction(
                    inter.followup,
                    embed=resp[0],
                    view=CreatePaginator(resp, inter.author.id, 180),
                )
            else:
                await sending.safe_send_interaction(inter.followup, embeds=resp)
        else:
            await sending.safe_send_interaction(inter.followup, embed=resp)
