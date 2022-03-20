from select import select
from turtle import back
import disnake
from disnake import AppCommandInteraction, CommandInteraction
from disnake.ext import commands
from logger import VyLogger
from utils import backend
import asyncio

logger = VyLogger("default")


class Information(commands.Cog):
    def __init__(self, bot):
        self.bot = bot

    @commands.slash_command(description="The help command.")
    async def biblebot(self, inter: CommandInteraction):
        resp = await backend.submit_command(inter.channel, inter.author, "+biblebot")
        await inter.response.send_message(embed=resp)

    @commands.slash_command(description="Statistics on the bot.")
    async def stats(self, inter: CommandInteraction):
        # todo: send stats before fetching result
        resp = await backend.submit_command(inter.channel, inter.author, "+stats")

        await inter.response.send_message(embed=resp)

    @commands.slash_command(description="See bot and support server invites.")
    async def invite(self, inter: CommandInteraction):
        resp = await backend.submit_command(inter.channel, inter.author, "+invite")
        await inter.response.send_message(embed=resp)
