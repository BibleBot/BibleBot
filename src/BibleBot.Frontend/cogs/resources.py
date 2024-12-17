"""
    Copyright (C) 2016-2024 Kerygma Digital Co.

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

Resource = commands.option_enum(
    {
        "Canon Law - Code of Canon Law (1983)": "cic",
        "Canon Law - Code of Canons of the Eastern Churches (1990)": "cceo",
        "Catechism - of the Catholic Church (1993)": "ccc",
        "Catechism - Luther's Small (1529)": "lsc",
        "Creed - Apostles'": "apostles",
        "Creed - Chalcedonian Definition (451)": "chalcedon",
        "Creed - Nicene (325)": "nicene325",
        "Creed - Nicene-Constantinopolitan (381)": "nicene",
    }
)


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
        self,
        inter: CommandInteraction,
        resource: str = commands.Param(
            choices={
                "Canon Law - Code of Canon Law (1983)": "cic",
                "Canon Law - Code of Canons of the Eastern Churches (1990)": "cceo",
                "Catechism - of the Catholic Church (1993)": "ccc",
                "Catechism - Luther's Small (1529)": "lsc",
                "Creed - Apostles'": "apostles",
                "Creed - Chalcedonian Definition (451)": "chalcedon",
                "Creed - Nicene (325)": "nicene325",
                "Creed - Nicene-Constantinopolitan (381)": "nicene",
            }
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
