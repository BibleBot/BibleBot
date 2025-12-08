"""
Copyright (C) 2016-2025 Kerygma Digital Co.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this file,
You can obtain one at https://mozilla.org/MPL/2.0/.
"""

import os

import disnake
import patreon
from core import checks, constants
from core.i18n import bb_i18n
from disnake import Localized
from disnake.ext import commands
from disnake.interactions import ApplicationCommandInteraction
from helpers import sending
from logger import VyLogger
from services import backend

i18n = bb_i18n()

logger = VyLogger("default")
patreon_api = patreon.API(os.environ.get("PATREON_TOKEN"))


class Information(commands.Cog):
    def __init__(self, bot):
        self.bot = bot

    @commands.slash_command(description=Localized(key="CMD_BIBLEBOT_DESC"))
    async def biblebot(self, inter: ApplicationCommandInteraction):
        await inter.response.defer(ephemeral=checks.inter_is_user(inter))
        resp = await backend.submit_command(inter.channel, inter.author, "+biblebot")
        await sending.safe_send_interaction(inter.followup, components=resp)

    @commands.slash_command(description=Localized(key="CMD_STATS_DESC"))
    async def stats(self, inter: ApplicationCommandInteraction):
        await inter.response.defer(ephemeral=checks.inter_is_user(inter))
        resp = await backend.submit_command(inter.channel, inter.author, "+stats")
        await sending.safe_send_interaction(inter.followup, components=resp)

    @commands.slash_command(description=Localized(key="CMD_INVITE_DESC"))
    async def invite(self, inter: ApplicationCommandInteraction):
        await inter.response.defer(ephemeral=checks.inter_is_user(inter))
        resp = await backend.submit_command(inter.channel, inter.author, "+invite")
        await sending.safe_send_interaction(inter.followup, components=resp)

    @commands.slash_command(description=Localized(key="CMD_SUPPORTERS_DESC"))
    async def supporters(self, inter: ApplicationCommandInteraction):
        # Patreon's development utilities are pretty garbage.
        # The way they've positioned the types and how they actually work are not the same thing.
        # Thus, we ignore type checking in some lines to compensate for their... stupidity.
        # TODO: Replace with direct API calls, since Patreon hardly maintains this library.

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

        container = disnake.ui.Container()
        container.accent_color = 6709986

        container.children.append(
            disnake.ui.TextDisplay(f"### {localization['SUPPORTERS_TITLE']}")
        )
        container.children.append(
            disnake.ui.TextDisplay(
                f"{localization['SUPPORTERS_LEADIN'] + ':\n\n**' + '**\n**'.join(names) + '**'}"
            )
        )

        container.children.append(
            disnake.ui.Separator(divider=True, spacing=disnake.SeparatorSpacing.large)
        )

        container.children.append(
            disnake.ui.TextDisplay(
                f"-# {constants.logo_emoji}  **{localization['EMBED_FOOTER'].replace('<v>', constants.version)}**"
            )
        )

        await sending.safe_send_interaction(inter.followup, components=container)

    @commands.slash_command(description=Localized(key="CMD_EXPERIMENTS_DESC"))
    async def experiments(self, inter: ApplicationCommandInteraction):
        await inter.response.defer(ephemeral=checks.inter_is_user(inter))
        resp = await backend.submit_command(inter.channel, inter.author, "+experiments")
        await sending.safe_send_interaction(inter.followup, components=resp)
