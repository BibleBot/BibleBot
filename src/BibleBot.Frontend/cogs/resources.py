"""
    Copyright (C) 2016-2022 Kerygma Digital Co.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this file,
    You can obtain one at https://mozilla.org/MPL/2.0/.
"""

from disnake import CommandInteraction
from disnake.ext import commands
from setuptools import Command
from logger import VyLogger
from utils import backend
from Paginator import CreatePaginator

logger = VyLogger("default")


class Resources(commands.Cog):
    def __init__(self, bot):
        self.bot = bot

    # todo: this

    @commands.slash_command(description="Search for verses by keyword.")
    async def search(self, inter: CommandInteraction, query: str):
        resp = await backend.submit_command(
            inter.channel, inter.author, f"+search {query}"
        )

        await inter.response.send_message(
            embed=resp[0], view=CreatePaginator(resp, inter.author.id, 180)
        )
