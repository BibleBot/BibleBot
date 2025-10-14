"""
Copyright (C) 2016-2025 Kerygma Digital Co.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this file,
You can obtain one at https://mozilla.org/MPL/2.0/.
"""

from disnake import CommandInteraction


def inter_is_user(inter: CommandInteraction) -> bool:
    """Returns whether an interaction is triggered in a user app context."""
    return (
        getattr(inter.authorizing_integration_owners, "user_id", None) is not None
        and getattr(inter.authorizing_integration_owners, "guild_id", None) is None
    )
