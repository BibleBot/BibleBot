"""
Copyright (C) 2016-2025 Kerygma Digital Co.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this file,
You can obtain one at https://mozilla.org/MPL/2.0/.
"""

import os
from disnake import CommandInteraction, Localized, OptionChoice
from disnake.ext import commands
import disnake
from logger import VyLogger
from utils import backend, sending, statics, channels, checks
from utils.views import CreatePaginator, CreateConfirmationPrompt
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
        await inter.response.defer(ephemeral=checks.inter_is_user(inter))
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

    @commands.slash_command(description=Localized(key="CMD_VERSE_DESC"))
    @commands.install_types(user=True)
    async def verse(self, inter: CommandInteraction, reference: str):
        await inter.response.defer()

        ctx = await channels.get_channel_context_from_interaction(inter)

        if ctx is None:
            return None

        req_body = {
            "UserId": str(inter.author.id),
            "GuildId": ctx.guild_id,
            "ChannelId": ctx.channel_id,
            "ThreadId": ctx.thread_id,
            "IsThread": ctx.is_thread,
            "IsBot": inter.author.bot,
            "IsDM": ctx.is_thread,
            "Body": reference,
        }

        endpoint = os.environ.get("ENDPOINT", "")

        resp = await backend.submit_verse_raw(endpoint, req_body)

        localization = i18n.get_i18n_or_default(inter.locale.name)

        if resp is None:
            await sending.safe_send_interaction(
                inter.followup, localization["CMD_VERSE_FAIL"], ephemeral=True
            )
        elif isinstance(resp, disnake.Embed):
            await sending.safe_send_interaction(inter.followup, embed=resp)
        elif isinstance(resp, CreatePaginator):
            await sending.safe_send_interaction(
                inter.followup, embed=resp.embeds[0], view=resp
            )
        elif isinstance(resp, list):
            if len(resp) == 0:
                await sending.safe_send_interaction(
                    inter.followup, localization["CMD_VERSE_FAIL"], ephemeral=True
                )
            elif isinstance(resp[0], disnake.Embed):
                await sending.safe_send_interaction(inter.followup, embeds=resp)
            elif isinstance(resp[0], str):
                for item in resp:
                    await sending.safe_send_interaction(inter.followup, item)

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
    @commands.install_types(guild=True)
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
    @commands.install_types(guild=True)
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
    @commands.install_types(guild=True)
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
    @commands.install_types(guild=True)
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

            if not role.is_default() and not role.mentionable:
                await sending.safe_send_interaction(
                    inter.followup,
                    embed=backend.create_error_embed(
                        "/setdailyverserole",
                        localization["SETDAILYVERSEROLE_UNMENTIONABLE"],
                        localization,
                    ),
                    ephemeral=True,
                )

            if role.is_default():
                embed = disnake.Embed()
                embed.title = localization["CONFIRMATION_REQUIRED_TITLE"]
                embed.description = localization[
                    "CONFIRMATION_REQUIRED_SETDAILYVERSEROLE_EVERYONE"
                ]

                embed.color = 16776960

                embed.set_footer(
                    text=localization["EMBED_FOOTER"].replace("<v>", statics.version),
                    icon_url="https://i.imgur.com/hr4RXpy.png",
                )
                await sending.safe_send_interaction(
                    inter.followup,
                    embed=embed,
                    view=CreateConfirmationPrompt(
                        f"+dailyverse role {role.id}", inter.author, 180
                    ),
                )
            else:
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

    @commands.slash_command(description=Localized(key="CMD_CLEARDAILYVERSEROLE_DESC"))
    @commands.install_types(guild=True)
    async def cleardailyverserole(self, inter: CommandInteraction):
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
                inter.channel, inter.author, "+dailyverse clearrole"
            )

            await sending.safe_send_interaction(inter.followup, embed=resp)
        else:
            await sending.safe_send_interaction(
                inter.followup,
                embed=backend.create_error_embed(
                    "/cleardailyverserole",
                    localization["AUTOMATIC_DAILY_VERSE_NODMS"],
                    localization,
                ),
                ephemeral=True,
            )
            return
