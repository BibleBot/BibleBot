"""
Copyright (C) 2016-2025 Kerygma Digital Co.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this file,
You can obtain one at https://mozilla.org/MPL/2.0/.
"""

from disnake.ui.text_display import TextDisplay
from disnake.ui import Container
import disnake
from disnake import Localized
from disnake.interactions import ApplicationCommandInteraction
from utils import backend, sending, statics, checks, containers
from utils.i18n import i18n as i18n_class
from disnake.ext import commands
from disnake.abc import GuildChannel, PrivateChannel
import experiments

i18n = i18n_class()


class Staff(commands.Cog):
    def __init__(self, bot: commands.Bot):
        self.bot = bot

    @commands.slash_command(description=Localized(key="CMD_PERMSCHECK_DESC"))
    @commands.install_types(guild=True)
    @commands.contexts(guild=True, bot_dm=False, private_channel=False)
    async def permscheck(
        self,
        inter: ApplicationCommandInteraction,
        channel_id: int = commands.Param(
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

                if not isinstance(channel_to_be, PrivateChannel):
                    channel = channel_to_be
                    guild = channel_to_be.guild
                else:
                    await sending.safe_send_interaction(
                        inter.followup,
                        components=containers.create_error_container(
                            localization["PERMSCHECK_ERROR_LABEL"],
                            localization["PERMSCHECK_ERROR_DM"],
                            localization,
                        ),
                    )
                    return
            except (disnake.errors.NotFound, disnake.errors.Forbidden):
                await sending.safe_send_interaction(
                    inter.followup,
                    components=containers.create_error_container(
                        localization["PERMSCHECK_ERROR_LABEL"],
                        localization["PERMSCHECK_ERROR_NOCHAN"],
                        localization,
                    ),
                )
                return
            except:
                await sending.safe_send_interaction(
                    inter.followup,
                    components=containers.create_error_container(
                        localization["PERMSCHECK_ERROR_LABEL"],
                        localization["PERMSCHECK_ERROR_UNKNOWN"],
                        localization,
                    ),
                )
                return

        if guild is None:
            # This case should ideally not be reached due to context decorators
            # and channel fetching logic, but as a safeguard:
            await sending.safe_send_interaction(
                inter.followup,
                components=containers.create_error_container(
                    localization["PERMSCHECK_ERROR_LABEL"],
                    "Could not determine the guild for this check.",
                    localization,
                ),
            )
            return

        integrated_roles = [
            x
            for x in guild.me.roles
            if x.is_bot_managed and x.is_integration and x.name != "@everyone"
        ]

        if not integrated_roles:
            # Handle case where bot has no integration role.
            # This might involve sending an error or using a default.
            # For now, we can assume the first role is the one we want if it exists.
            # Or we can send an error message.
            await sending.safe_send_interaction(
                inter.followup,
                components=containers.create_error_container(
                    localization["PERMSCHECK_ERROR_LABEL"],
                    "Bot integration role not found.",
                    localization,
                ),
            )
            return

        integrated_role = integrated_roles[0]

        if not isinstance(channel, GuildChannel):
            await sending.safe_send_interaction(
                inter.followup,
                components=containers.create_error_container(
                    localization["PERMSCHECK_ERROR_LABEL"],
                    "Permissions check cannot be performed on this channel type.",
                    localization,
                ),
            )
            return

        channel_perms_for_self = channel.permissions_for(guild.me).value
        channel_perms_for_role = channel.permissions_for(integrated_role).value
        guild_perms = integrated_role.permissions.value

        resp = await backend.submit_command(
            inter.channel,
            inter.author,
            f"+staff permscheck {channel.id} {guild.id} {channel_perms_for_self} {channel_perms_for_role} {guild_perms} {integrated_role.name} {integrated_role.id}",
        )

        await sending.safe_send_interaction(inter.followup, components=resp)

    @commands.slash_command(description=Localized(key="CMD_RELOAD_VERSIONS_DESC"))
    async def reload_versions(self, inter: ApplicationCommandInteraction):
        await inter.response.defer()
        resp = await backend.submit_command(
            inter.channel, inter.author, "+staff reload_versions"
        )
        await sending.safe_send_interaction(inter.followup, components=resp)

    @commands.slash_command(description=Localized(key="CMD_RELOAD_LANGUAGES_DESC"))
    async def reload_languages(self, inter: ApplicationCommandInteraction):
        await inter.response.defer()
        resp = await backend.submit_command(
            inter.channel, inter.author, "+staff reload_languages"
        )
        await sending.safe_send_interaction(inter.followup, components=resp)

    @commands.slash_command(description=Localized(key="CMD_RELOAD_EXPERIMENTS_DESC"))
    async def reload_experiments(self, inter: ApplicationCommandInteraction):
        await inter.response.defer()

        resp = await backend.submit_command(
            inter.channel, inter.author, "+staff reload_experiments"
        )

        if resp is not None and isinstance(resp, Container):
            if isinstance(resp.children[1], TextDisplay):
                if resp.children[1].content == "Experiments have been reloaded.":
                    self.bot.remove_cog("Experiments")
                    self.bot.add_cog(experiments.Experiments(self.bot))

            await sending.safe_send_interaction(inter.followup, components=resp)
