"""
    Copyright (C) 2016-2023 Kerygma Digital Co.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this file,
    You can obtain one at https://mozilla.org/MPL/2.0/.
"""

from disnake import CommandInteraction
from disnake.ext import commands
from setuptools import Command
from logger import VyLogger
from utils import backend, sending
from utils.paginator import CreatePaginator

logger = VyLogger("default")


class Resources(commands.Cog):
    def __init__(self, bot):
        self.bot = bot

    @commands.slash_command(description="See all available resources.")
    async def resources(self, inter: CommandInteraction):
        await inter.response.defer()
        resp = await backend.submit_command(inter.channel, inter.author, f"+resource")
        await sending.safe_send_interaction(inter.followup, embed=resp)

    @commands.slash_command(description="Use a resource.")
    async def resource(
        self, inter: CommandInteraction, resource: str, range: str = None
    ):
        await inter.response.defer()
        cmd = f"+resource {resource}"

        if range:
            cmd += f" {range}"

        resp = await backend.submit_command(inter.channel, inter.author, cmd)

        if isinstance(resp, list):
            await sending.safe_send_interaction(
                inter.followup,
                embed=resp[0],
                view=CreatePaginator(resp, inter.author.id, 180),
            )
        else:
            await sending.safe_send_interaction(inter.followup, embed=resp)
