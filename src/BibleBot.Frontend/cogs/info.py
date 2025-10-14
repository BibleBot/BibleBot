"""
Copyright (C) 2016-2025 Kerygma Digital Co.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this file,
You can obtain one at https://mozilla.org/MPL/2.0/.
"""

import os
import aiohttp
from disnake import CommandInteraction, Localized
import disnake
from disnake.ext import commands
from logger import VyLogger
from utils import backend, sending, statics, checks
from utils.i18n import i18n as i18n_class
import patreon
import subprocess

i18n = i18n_class()

logger = VyLogger("default")
patreon_api = patreon.API(os.environ.get("PATREON_TOKEN"))


class Information(commands.Cog):
    def __init__(self, bot):
        self.bot = bot

    @commands.slash_command(description=Localized(key="CMD_BIBLEBOT_DESC"))
    async def biblebot(self, inter: CommandInteraction):
        await inter.response.defer(ephemeral=checks.inter_is_user(inter))
        resp = await backend.submit_command(inter.channel, inter.author, "+biblebot")
        await sending.safe_send_interaction(inter.followup, embed=resp)

    @commands.slash_command(description=Localized(key="CMD_STATS_DESC"))
    async def stats(self, inter: CommandInteraction):
        await inter.response.defer(ephemeral=checks.inter_is_user(inter))
        if not inter.author.bot:
            await send_stats(self.bot)

        resp = await backend.submit_command(inter.channel, inter.author, "+stats")
        await sending.safe_send_interaction(inter.followup, embed=resp)

    @commands.slash_command(description=Localized(key="CMD_INVITE_DESC"))
    async def invite(self, inter: CommandInteraction):
        await inter.response.defer(ephemeral=checks.inter_is_user(inter))
        resp = await backend.submit_command(inter.channel, inter.author, "+invite")
        await sending.safe_send_interaction(inter.followup, embed=resp)

    @commands.slash_command(description=Localized(key="CMD_PERMSCHECK_DESC"))
    @commands.install_types(guild=True)
    async def permscheck(
        self,
        inter: CommandInteraction,
        channel_id: str = commands.Param(
            default=None,
            description="The ID of the channel (optional)",
        ),
    ):
        await inter.response.defer()

        localization = i18n.get_i18n_or_default(inter.locale.name)

        channel = inter.channel
        guild = inter.guild

        if channel_id is not None:
            try:
                channel_to_be = await self.bot.fetch_channel(channel_id)

                if not isinstance(channel_to_be, disnake.abc.PrivateChannel):
                    channel = channel_to_be
                    guild = channel_to_be.guild
                else:
                    await sending.safe_send_interaction(
                        inter.followup,
                        embed=backend.create_error_embed(
                            localization["PERMSCHECK_ERROR_LABEL"],
                            localization["PERMSCHECK_ERROR_DM"],
                            localization,
                        ),
                    )
            except (disnake.NotFound, disnake.Forbidden):
                await sending.safe_send_interaction(
                    inter.followup,
                    embed=backend.create_error_embed(
                        localization["PERMSCHECK_ERROR_LABEL"],
                        localization["PERMSCHECK_ERROR_NOCHAN"],
                        localization,
                    ),
                )
                return
            except:
                await sending.safe_send_interaction(
                    inter.followup,
                    embed=backend.create_error_embed(
                        localization["PERMSCHECK_ERROR_LABEL"],
                        localization["PERMSCHECK_ERROR_UNKNOWN"],
                        localization,
                    ),
                )
                return

        integrated_role = [
            x
            for x in guild.me.roles
            if x.is_bot_managed and x.is_integration and x.name != "@everyone"
        ][0]

        channel_perms_for_self = channel.permissions_for(guild.me).value
        channel_perms_for_role = channel.permissions_for(integrated_role).value
        guild_perms = integrated_role.permissions.value

        resp = await backend.submit_command(
            inter.channel,
            inter.author,
            f"+staff permscheck {channel.id} {guild.id} {channel_perms_for_self} {channel_perms_for_role} {guild_perms} {integrated_role.name} {integrated_role.id}",
        )

        await sending.safe_send_interaction(inter.followup, embed=resp)

    @commands.slash_command(description=Localized(key="CMD_SUPPORTERS_DESC"))
    async def supporters(self, inter: CommandInteraction):
        # Patreon's development utilities are pretty garbage.
        # The way they've positioned the types and how they actually work are not the same thing.
        # Thus, we ignore type checking in some lines to compensate for their... [redacted].

        await inter.response.defer(ephemeral=checks.inter_is_user(inter))

        localization = i18n.get_i18n_or_default(inter.locale.name)

        campaigns = patreon_api.fetch_campaign().data()
        campaign = campaigns[0].id()  # type: ignore
        pledges = []
        names = []
        cursor = None

        while True:
            pledges_resp = patreon_api.fetch_page_of_pledges(
                campaign, 25, cursor=cursor
            )
            pledges += pledges_resp.data()  # type: ignore
            cursor = patreon_api.extract_cursor(pledges_resp)
            if not cursor:
                break

        names = [
            x.relationship("patron").attribute("full_name").strip() for x in pledges
        ]

        embed = disnake.Embed()

        embed.title = localization["SUPPORTERS_TITLE"]
        embed.description = (
            localization["SUPPORTERS_LEADIN"] + f":\n\n**" + "**\n**".join(names) + "**"
        )
        embed.color = 6709986

        embed.set_footer(
            text=localization["EMBED_FOOTER"].replace("<v>", statics.version),
            icon_url="https://i.imgur.com/hr4RXpy.png",
        )
        await sending.safe_send_interaction(inter.followup, embed=embed)


async def send_stats(bot: disnake.AutoShardedClient):
    endpoint = os.environ.get("ENDPOINT")
    token = os.environ.get("ENDPOINT_TOKEN")

    shard_count = bot.shard_count
    guild_count = len(bot.guilds)
    user_count = sum([x.member_count for x in bot.guilds])
    channel_count = sum([len(x.channels) for x in bot.guilds])

    repo_sha = (
        subprocess.check_output(["git", "rev-parse", "HEAD"]).decode("ascii").strip()
    )

    async with aiohttp.ClientSession() as session:
        async with session.post(
            f"{endpoint}/stats/process",
            json={
                "Body": f"{shard_count}||{guild_count}||{user_count}||{channel_count}||{repo_sha}",
            },
            headers={"Authorization": token},
        ) as resp:
            if resp.status != 200:
                logger.error("couldn't submit stats to backend")
