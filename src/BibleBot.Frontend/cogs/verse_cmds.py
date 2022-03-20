from select import select
from turtle import back
import disnake
from disnake import AppCommandInteraction, CommandInter, CommandInteraction
from disnake.ext import commands
from logger import VyLogger
from utils import backend
import asyncio

logger = VyLogger("default")


class VerseCommands(commands.Cog):
    def __init__(self, bot):
        self.bot = bot

    # todo: all of these commands need to account for display style

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
