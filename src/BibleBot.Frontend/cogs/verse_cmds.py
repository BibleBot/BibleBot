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


class VerseCommands(commands.Cog):
    def __init__(self, bot):
        self.bot = bot

    # todo: all of these commands need to account for display style

    @commands.slash_command(description="Search for verses by keyword.")
    async def search(self, inter: CommandInteraction, query: str):
        resp = await backend.submit_command_raw(
            inter.channel, inter.author, f"+search {query}"
        )

        embeds = []
        starting_page = None

        # For whatever reason, the paginator library has the buttons
        # performing the opposite effect, "next" goes to the previous
        # page and vice versa. This reverses the array and makes sure
        # the first page is properly the first embed, which is still a
        # requirement despite the paginator working backwards.
        #
        # I could fix this myself by forking the library
        # (it's a two-line fix), but I'm too lazy for that.
        for page in resp["pages"][::-1]:
            page_embed = backend.convert_embed(page)

            if f"Page 1 of" in page_embed.description:
                starting_page = page_embed
            else:
                embeds.append(page_embed)

        embeds.insert(0, starting_page)

        await inter.response.send_message(
            embed=embeds[0], view=CreatePaginator(embeds, inter.author.id, 180)
        )

    @commands.slash_command(
        description="Display a random verse from a predetermined pool."
    )
    async def random(self, inter: CommandInteraction):
        resp = await backend.submit_command(inter.channel, inter.author, "+random")
        await inter.response.send_message(embed=resp)

    @commands.slash_command(
        description="Display a random verse based on random number generation."
    )
    async def truerandom(self, inter: CommandInteraction):
        resp = await backend.submit_command(inter.channel, inter.author, "+random true")
        await inter.response.send_message(embed=resp)

    @commands.slash_command(description="Display the verse of the day.")
    async def dailyverse(self, inter: CommandInteraction):
        resp = await backend.submit_command(inter.channel, inter.author, "+dailyverse")
        await inter.response.send_message(embed=resp)

    @commands.slash_command(description="Setup automatic daily verses on this channel.")
    @commands.has_permissions(manage_guild=True)
    async def setautodailyverse(
        self, inter: CommandInteraction, time: str = None, tz: str = None
    ):
        resp = None
        if time is None or tz is None:
            resp = await backend.submit_command(
                inter.channel, inter.author, "+dailyverse set"
            )
        else:
            # todo: webhooks et al
            resp = await backend.submit_command(
                inter.channel, inter.author, f"+dailyverse set {time} {tz}"
            )

        await inter.response.send_message(embed=resp)

    @commands.slash_command(
        description="See automatic daily verse status for this server."
    )
    async def autodailyversestatus(self, inter: CommandInteraction):
        resp = await backend.submit_command(
            inter.channel, inter.author, "+dailyverse status"
        )

        await inter.response.send_message(embed=resp)

    @commands.slash_command(
        description="Clear all automatic daily verse preferences for this server."
    )
    @commands.has_permissions(manage_guild=True)
    async def clearautodailyverse(self, inter: CommandInteraction):
        # todo: webhooks et al
        resp = await backend.submit_command(
            inter.channel, inter.author, "+dailyverse clear"
        )

        await inter.response.send_message(embed=resp)
