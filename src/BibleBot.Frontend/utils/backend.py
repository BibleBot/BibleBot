"""
Copyright (C) 2016-2025 Kerygma Digital Co.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this file,
You can obtain one at https://mozilla.org/MPL/2.0/.
"""

import os
import json
from typing import Optional, Union
import aiohttp
import disnake
from logger import VyLogger
from . import sending, statics, webhooks, containers, channels
from .paginator import ComponentPaginator
from .i18n import i18n as i18n_class

i18n = i18n_class()
logger = VyLogger("default")
aiohttp_headers = {"Authorization": os.environ.get("ENDPOINT_TOKEN", "")}


async def submit_command(
    rch: disnake.abc.Messageable,
    user: disnake.abc.User,
    body: str,
):
    """Submits a command to the backend and processes the result."""

    ctx = await channels.get_channel_context_from_messageable(rch)

    if ctx is None or ctx.channel is None:
        return None

    req_body = {
        "UserId": str(user.id),
        "GuildId": ctx.guild_id,
        "ChannelId": ctx.channel_id,
        "ThreadId": ctx.thread_id,
        "IsThread": ctx.is_thread,
        "IsBot": user.bot,
        "IsDM": ctx.is_thread,
        "Body": body,
    }

    endpoint = os.environ.get("ENDPOINT", "")

    resp_body = await submit_command_raw(endpoint, req_body)

    if resp_body["culture"] is not None:
        localization = i18n.get_i18n_or_default(resp_body["culture"].replace("-", "_"))
    else:
        localization = i18n.get_i18n_or_default("en_US")

    if resp_body["ok"]:
        logger.info(
            f"<{user.id}@{ctx.guild_id}#{ctx.channel_id}> " + resp_body["logStatement"]
        )
    else:
        logger.error(
            f"<{user.id}@{ctx.guild_id}#{ctx.channel_id}> " + resp_body["logStatement"]
        )

    if resp_body["type"] == "cmd":
        if len(resp_body["pages"]) == 1:
            # todo: webhook stuff should not be dailyverse-specific
            if resp_body["removeWebhook"] and ctx.guild is not None:
                try:
                    webhook_service_body = await webhooks.remove_webhooks(
                        user, ctx.guild
                    )

                    req_body["Body"] = webhook_service_body
                    async with aiohttp.ClientSession() as subsession:
                        async with subsession.post(
                            f"{endpoint}/webhooks/process",
                            json=req_body,
                            headers=aiohttp_headers,
                        ) as subresp:
                            if subresp.status != 200:
                                logger.error("couldn't submit webhook")
                            else:
                                return containers.convert_embed_to_container(
                                    resp_body["pages"][0]
                                )
                except disnake.errors.Forbidden:
                    await sending.safe_send_channel(
                        ctx.channel,
                        components=containers.create_error_container(
                            localization["PERMS_ERROR_LABEL"],
                            localization["WEBHOOK_REMOVAL_FAILURE"],
                            localization,
                        ),
                    )

            if resp_body["createWebhook"] and ctx.guild is not None:
                try:
                    webhook_service_body = await webhooks.create_webhook(ctx)

                    # Send a request to the webhook controller, which will update the DB.
                    req_body["Body"] = webhook_service_body
                    async with aiohttp.ClientSession() as subsession:
                        async with subsession.post(
                            f"{endpoint}/webhooks/process",
                            json=req_body,
                            headers=aiohttp_headers,
                        ) as subresp:
                            if subresp.status != 200:
                                logger.error("couldn't submit webhook")
                            else:
                                return containers.convert_embed_to_container(
                                    resp_body["pages"][0]
                                )
                except disnake.errors.Forbidden:
                    try:
                        await sending.safe_send_channel(
                            ctx.channel,
                            components=containers.create_error_container(
                                "/dailyverseset",
                                localization["WEBHOOK_CREATION_FAILURE"],
                                localization,
                            ),
                        )
                    except disnake.errors.Forbidden:
                        logger.error(
                            f"unable to add webhook for <{user.id}@{ctx.guild_id}#{ctx.channel_id}>"
                        )

            return containers.convert_embed_to_container(resp_body["pages"][0])
        else:
            return containers.mass_create_containers(resp_body["pages"], localization)
    elif resp_body["type"] == "verse":
        if "does not support the" in resp_body["logStatement"]:
            return containers.create_error_container(
                "Verse Error", resp_body["logStatement"], localization
            )
        elif "too many verses" in resp_body["logStatement"]:
            return containers.convert_embed_to_container(resp_body["pages"][0])

        display_style = resp_body["displayStyle"]
        if display_style == "embed":
            for verse in resp_body["verses"]:
                return containers.convert_verse_to_container(
                    verse,
                    (
                        resp_body["cultureFooter"]
                        if resp_body["cultureFooter"] is not None
                        else statics.verse_footer
                    ),
                )
            return None
        elif display_style == "blockquote":
            for verse in resp_body["verses"]:
                reference_title = (
                    verse["reference"]["asString"]
                    + " - "
                    + verse["reference"]["version"]["name"]
                )
                verse_title = (
                    ("**" + verse["title"] + "**\n> \n> ")
                    if len(verse["title"]) > 0
                    else ""
                )
                verse_text = verse["text"]

                return f"**{reference_title}**\n\n> {verse_title}{verse_text}"
            return None
        elif display_style == "code":
            for verse in resp_body["verses"]:
                reference_title = (
                    verse["reference"]["asString"]
                    + " - "
                    + verse["reference"]["version"]["name"]
                )
                verse_title = (
                    (verse["title"] + "\n\n") if len(verse["title"]) > 0 else ""
                )
                verse_text = verse["text"].replace("*", "")

                return (
                    f"**{reference_title}**\n\n```json\n{verse_title} {verse_text}```"
                )
            return None
        return None
    return None


async def submit_command_raw(endpoint: str, req_body: dict):
    """Submits a command to the backend and returns the result."""

    async with aiohttp.ClientSession() as session:
        async with session.post(
            f"{endpoint}/commands/process",
            json=req_body,
            headers=aiohttp_headers,
        ) as resp:
            resp_text = await resp.text()
            resp_text = resp_text.replace("\\\\n", "\\n")

            resp_body = json.loads(resp_text)
            return resp_body


async def submit_verse(
    rch: disnake.abc.Messageable,
    user: disnake.abc.User,
    body: str,
):
    """Submits a verse to the backend and processes the result."""
    ctx = await channels.get_channel_context_from_messageable(rch)

    if ctx is None or ctx.channel is None:
        return None

    req_body = {
        "UserId": str(user.id),
        "GuildId": ctx.guild_id,
        "ChannelId": ctx.channel_id,
        "ThreadId": ctx.thread_id,
        "IsThread": ctx.is_thread,
        "IsBot": user.bot,
        "IsDM": ctx.is_thread,
        "Body": body,
    }

    endpoint = os.environ.get("ENDPOINT", "")

    resp_body = await submit_verse_raw(endpoint, req_body)

    if isinstance(resp_body, disnake.ui.Container):
        await sending.safe_send_channel(
            ctx.channel,
            components=resp_body,
        )
    elif isinstance(resp_body, ComponentPaginator):
        await resp_body.send(ctx.channel)
    elif isinstance(resp_body, list):
        if isinstance(resp_body[0], disnake.ui.Container):
            await sending.safe_send_channel(
                ctx.channel,
                components=resp_body,
            )
        elif isinstance(resp_body[0], str):
            for item in resp_body:
                await sending.safe_send_channel(ctx.channel, item)

    return req_body, resp_body


async def submit_verse_raw(
    endpoint: str, req_body: dict, is_command: bool = False
) -> Optional[
    Union[
        disnake.ui.Container, list[str], list[disnake.ui.Container], ComponentPaginator
    ]
]:
    """Submits a verse to the backend and returns the result."""

    resp_body = None

    async with aiohttp.ClientSession() as session:
        async with session.post(
            f"{endpoint}/verses/process",
            json=req_body,
            headers=aiohttp_headers,
        ) as resp:
            resp_body = await resp.json()

    if resp_body["culture"] is not None:
        localization = i18n.get_i18n_or_default(resp_body["culture"].replace("-", "_"))
    else:
        localization = i18n.get_i18n_or_default("en_US")

    if resp_body["logStatement"]:
        logger.info(
            f"<{req_body["UserId"]}@{req_body["GuildId"]}#{req_body["ChannelId"]}> "
            + resp_body["logStatement"]
        )

    if resp_body["logStatement"]:
        if "does not support the" in resp_body["logStatement"]:
            return containers.create_error_container(
                "Verse Error", resp_body["logStatement"], localization
            )
        elif "too many verses" in resp_body["logStatement"]:
            return containers.convert_embed_to_container(resp_body["pages"][0])

    if "verses" not in resp_body:
        if "pages" in resp_body:
            return containers.convert_embed_to_container(resp_body["pages"][0])
        return

    verses = resp_body["verses"]
    processed_verses = []

    display_style = resp_body["displayStyle"]
    if display_style == "embed":
        if resp_body["paginate"] and len(verses) > 1:
            components = containers.mass_create_containers(
                verses,
                (
                    resp_body["cultureFooter"]
                    if resp_body["cultureFooter"] is not None
                    else statics.verse_footer
                ),
                is_verses=True,
            )
            return ComponentPaginator(components, int(req_body["user_id"]))
        else:
            for verse in verses:
                processed_verses.append(
                    containers.convert_verse_to_container(
                        verse,
                        (
                            resp_body["cultureFooter"]
                            if resp_body["cultureFooter"] is not None
                            else statics.verse_footer
                        ),
                    ),
                )
    elif display_style == "blockquote":
        for verse in verses:
            reference_title = (
                verse["reference"]["asString"]
                + " - "
                + verse["reference"]["version"]["name"]
            )
            verse_title = (
                ("**" + verse["title"] + "**\n> \n> ")
                if len(verse["title"]) > 0
                else ""
            )
            verse_text = verse["text"]

            processed_verses.append(
                f"**{reference_title}**\n\n> {verse_title}{verse_text}"
            )
    elif display_style == "code":
        for verse in verses:
            reference_title = (
                verse["reference"]["asString"]
                + " - "
                + verse["reference"]["version"]["name"]
            )
            verse_title = (verse["title"] + "\n\n") if len(verse["title"]) > 0 else ""
            verse_text = verse["text"].replace("*", "")

            processed_verses.append(
                f"**{reference_title}**\n\n```json\n{verse_title} {verse_text}```"
            )

    return processed_verses
