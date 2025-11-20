"""
Copyright (C) 2016-2025 Kerygma Digital Co.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this file,
You can obtain one at https://mozilla.org/MPL/2.0/.
"""

import disnake
from disnake import abc
from logger import VyLogger

logger = VyLogger("default")


async def safe_send_interaction(receiver: disnake.Webhook, *args, **kwargs):
    try:
        await receiver.send(*args, **kwargs)
    except disnake.errors.Forbidden:
        message = "unable to send response to previous interaction"

        if "components" in kwargs.keys():
            # TODO: the following doesn't work, need to clearly identify error embeds
            if str(kwargs["components"].accent_color) == "#ff2e2e":
                message += " - this was an error embed"

        logger.error(message)


async def safe_send_interaction_ephemeral(
    resp: disnake.InteractionResponse, *args, **kwargs
):
    try:
        await resp.send_message(*args, **kwargs)
    except disnake.errors.Forbidden:
        message = "unable to send response to previous interaction"

        if "components" in kwargs.keys():
            if str(kwargs["components"].accent_color) == "#ff2e2e":
                message += " - this was an error"

        logger.error(message)


async def safe_send_channel(receiver: abc.Messageable, *args, **kwargs):
    try:
        await receiver.send(*args, **kwargs)
    except disnake.errors.Forbidden:
        message = "unable to send response to channel"

        if "components" in kwargs.keys():
            if str(kwargs["components"].accent_color) == "#ff2e2e":
                message += " - this was an error"

        logger.error(message)
