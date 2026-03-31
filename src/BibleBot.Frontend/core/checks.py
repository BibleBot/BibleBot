"""
Copyright (C) 2016-2026 Kerygma Digital Co.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this file,
You can obtain one at https://mozilla.org/MPL/2.0/.
"""

import disnake
from disnake.interactions import ApplicationCommandInteraction
# from os import environ
# import sentry_sdk

def inter_is_user(inter: ApplicationCommandInteraction) -> bool:
    """Returns whether an interaction is triggered in a user app context."""
    return (
        getattr(inter.authorizing_integration_owners, "user_id", None) is not None
        and getattr(inter.authorizing_integration_owners, "guild_id", None) is None
    )


def author_has_manage_server_permission(inter: ApplicationCommandInteraction) -> bool:
    """Returns whether the author of an interaction has the Manage Server permission in the guild of the interaction."""
    if (
        hasattr(inter.channel, "permissions_for")
        and callable(inter.channel.permissions_for)
        and isinstance(inter.author, disnake.Member)
    ):
        return inter.channel.permissions_for(inter.author).manage_guild
    return False


def inter_is_not_dm(inter: ApplicationCommandInteraction) -> bool:
    """Returns whether an interaction is triggered in a guild app context."""
    return not isinstance(inter.channel, disnake.DMChannel)


# This code is functional, but because we don't have/use
# the Guild Members verified intent, we have no way of using it.
# def user_is_bb_staff(inter: ApplicationCommandInteraction) -> bool:
#     try:
#         bb_guild = inter.bot.get_guild(int(environ.get('BB_GUILD_ID')))
#
#         if bb_guild is not None:
#             member = bb_guild.get_member(inter.author.id)
#
#             if member is not None:
#                 return member.get_role(int(environ.get('BB_STAFF_ROLE_ID'))) is not None
#     except Exception as e:
#         sentry_sdk.capture_exception(e)
#
#     return False
