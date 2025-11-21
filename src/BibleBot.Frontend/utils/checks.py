"""
Copyright (C) 2016-2025 Kerygma Digital Co.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this file,
You can obtain one at https://mozilla.org/MPL/2.0/.
"""

from disnake import CommandInteraction
import disnake


def inter_is_user(inter: CommandInteraction) -> bool:
    """Returns whether an interaction is triggered in a user app context."""
    return (
        getattr(inter.authorizing_integration_owners, "user_id", None) is not None
        and getattr(inter.authorizing_integration_owners, "guild_id", None) is None
    )


def author_has_manage_server_permission(inter: CommandInteraction) -> bool:
    if inter.guild and inter.author:
        member = inter.guild.get_member(inter.author.id)

        if member is not None:
            return member.guild_permissions.manage_guild

    return False


def inter_is_not_dm(inter: CommandInteraction) -> bool:
    return not isinstance(inter.channel, disnake.DMChannel)
