"""
Copyright (C) 2016-2025 Kerygma Digital Co.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this file,
You can obtain one at https://mozilla.org/MPL/2.0/.
"""

from disnake.abc import Messageable, GuildChannel
from disnake.channel import (
    TextChannel,
    VoiceChannel,
    DMChannel,
    StageChannel,
    GroupChannel,
    PartialMessageable,
)
from disnake import Thread, ChannelType, Guild, CommandInteraction
from typing import NamedTuple, Optional


# remind me to get away from this accursed language
class ChannelContext(NamedTuple):
    """A channel and its pertinent information."""

    channel: (
        TextChannel
        | VoiceChannel
        | DMChannel
        | StageChannel
        | Thread
        | GroupChannel
        | None
    )
    guild: Guild | None
    guild_id: str
    channel_id: str
    thread_id: str
    is_dm: bool = False
    is_thread: bool = False


async def get_channel_context_from_messageable(
    messageable: Messageable,
) -> Optional[ChannelContext]:
    """Get a ChannelContext object from a disnake.abc.Messageable object."""

    channel = None
    guild = None
    guild_id = None
    channel_id = None
    thread_id = None
    is_dm = False
    is_thread = False

    try:
        # pylint: disable=protected-access
        # <insert ron swanson "i know more than you" gif here>
        channel = await messageable._get_channel()
        # pylint: enable=protected-access
    except AttributeError:
        # In this scenario, we've got something that
        # should inherit Messageable in disnake but
        # has not been implemented. Meaning it's a
        # Messageable channel according to the Discord
        # API, but disnake has no clue for whatever reason.
        #
        # May seem silly to have this, but we've been
        # fooled before.
        return None

    if isinstance(channel, PartialMessageable):
        return None

    # this value shouldn't get used unless is_thread is True, so it's fine to set it
    # i forget if i made this optional in the C# request model
    thread_id = channel.id

    match channel.type:
        case ChannelType.private:
            is_dm = True
            guild_id = channel.id
            channel_id = channel.id
        case (
            ChannelType.news_thread
            | ChannelType.public_thread
            | ChannelType.private_thread
        ):
            is_thread = True
            guild = channel.guild
            guild_id = channel.guild.id
            channel_id = channel.parent_id
            thread_id = channel.id
        case _:
            guild = channel.guild
            guild_id = channel.guild.id
            channel_id = channel.id

    guild_id = str(guild_id)
    channel_id = str(channel_id)
    thread_id = str(thread_id)

    return ChannelContext(
        channel, guild, guild_id, channel_id, thread_id, is_dm, is_thread
    )


async def get_channel_context_from_interaction(
    interaction: CommandInteraction,
) -> Optional[ChannelContext]:
    """Get a ChannelContext object from a disnake.CommandInteraction object."""

    channel = (
        interaction.channel
        if isinstance(interaction.channel, (GuildChannel, Thread))
        else None
    )
    guild = interaction.guild
    guild_id = (
        interaction.guild.id
        if interaction.guild is not None
        else interaction.channel.id
    )
    channel_id = (
        interaction.channel.id
        if not isinstance(interaction.channel, Thread)
        else interaction.channel.parent_id
    )
    thread_id = interaction.channel.id
    is_dm = isinstance(interaction.channel, PartialMessageable)
    is_thread = isinstance(interaction.channel, Thread)

    guild_id = str(guild_id)
    channel_id = str(channel_id)
    thread_id = str(thread_id)

    return ChannelContext(
        channel, guild, guild_id, channel_id, thread_id, is_dm, is_thread
    )
