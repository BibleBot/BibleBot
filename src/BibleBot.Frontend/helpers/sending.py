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
    """A wrapper around interaction responses to ensure proper error handling."""
    try:
        await receiver.send(*args, **kwargs)
    except disnake.errors.Forbidden:
        message = "unable to send response to previous interaction"

        if "components" in kwargs.keys():
            if hasattr(kwargs["components"], "accent_color"):
                if str(kwargs["components"].accent_color) == "#ff2e2e":
                    message += " - this was an error embed"

        logger.error(message)
    except disnake.errors.HTTPException as ex:
        if (
            "Components displayable text size exceeds maximum size of 4000" in ex.text
            and isinstance(kwargs["components"], list)
        ):
            components_list = kwargs["components"]
            del kwargs["components"]

            for component in components_list:
                kwargs["components"] = component
                await receiver.send(*args, **kwargs)
        else:
            message = "unable to send response to channel"

            if "components" in kwargs.keys():
                if hasattr(kwargs["components"], "accent_color"):
                    if str(kwargs["components"].accent_color) == "#ff2e2e":
                        message += " - this was an error embed"

            logger.error(message)


async def safe_send_interaction_ephemeral(
    resp: disnake.InteractionResponse, *args, **kwargs
):
    """A wrapper around ephemeral interaction responses to ensure proper error handling."""
    try:
        await resp.send_message(*args, **kwargs)
    except disnake.errors.Forbidden:
        message = "unable to send response to previous interaction"

        if "components" in kwargs.keys():
            if hasattr(kwargs["components"], "accent_color"):
                if str(kwargs["components"].accent_color) == "#ff2e2e":
                    message += " - this was an error embed"

        logger.error(message)
    except disnake.errors.HTTPException as ex:
        if (
            "Components displayable text size exceeds maximum size of 4000" in ex.text
            and isinstance(kwargs["components"], list)
        ):
            components_list = kwargs["components"]
            del kwargs["components"]

            for component in components_list:
                kwargs["components"] = component
                await resp.send_message(*args, **kwargs)
        else:
            message = "unable to send response to channel"

            if "components" in kwargs.keys():
                if hasattr(kwargs["components"], "accent_color"):
                    if str(kwargs["components"].accent_color) == "#ff2e2e":
                        message += " - this was an error embed"

            logger.error(message)


async def safe_send_channel(receiver: abc.Messageable, *args, **kwargs):
    """A wrapper around channel responses to ensure proper error handling."""
    try:
        await receiver.send(*args, **kwargs)
    except disnake.errors.Forbidden:
        message = "unable to send response to channel"

        if "components" in kwargs.keys():
            if hasattr(kwargs["components"], "accent_color"):
                if str(kwargs["components"].accent_color) == "#ff2e2e":
                    message += " - this was an error embed"

        logger.error(message)
    except disnake.errors.HTTPException as ex:
        if (
            "Components displayable text size exceeds maximum size of 4000" in ex.text
            and isinstance(kwargs["components"], list)
        ):
            components_list = kwargs["components"]
            del kwargs["components"]

            for component in components_list:
                kwargs["components"] = component
                await receiver.send(*args, **kwargs)
        else:
            message = "unable to send response to channel"

            if "components" in kwargs.keys():
                if hasattr(kwargs["components"], "accent_color"):
                    if str(kwargs["components"].accent_color) == "#ff2e2e":
                        message += " - this was an error embed"

            logger.error(message)
