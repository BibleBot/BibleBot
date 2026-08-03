"""
Copyright (C) 2016-2026 Kerygma Digital Co.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this file,
You can obtain one at https://mozilla.org/MPL/2.0/.
"""

import asyncio
import json
import os
from typing import Optional, Union

import aiohttp
import disnake
from core import constants
from core.i18n import bb_i18n
from helpers import channels, sending
from logger import VyLogger
from services import http, webhooks
from ui import renderers as containers
from ui.paginator import ComponentPaginator

i18n = bb_i18n()
logger = VyLogger("default")
aiohttp_headers = {"Authorization": os.environ.get("ENDPOINT_TOKEN", "")}


async def submit_command(
    rch: disnake.abc.Messageable, user: disnake.abc.User, body: str
):
    """Submits a command to the backend and processes the result."""

    ctx = await channels.get_channel_context_from_messageable(rch)

    if ctx is None or ctx.channel is None:
        return None

    req_body = {
        "UserId": user.id,
        "GuildId": ctx.guild_id,
        "ChannelId": ctx.channel_id,
        "ThreadId": ctx.thread_id,
        "IsThread": ctx.is_thread,
        "IsBot": user.bot,
        "IsDM": ctx.is_dm,
        "Body": body,
    }

    endpoint = os.environ.get("ENDPOINT", "")

    resp_body = await submit_command_raw(endpoint, req_body)

    if resp_body.get("transportFailure"):
        logger.error(
            f"<{user.id}@{ctx.guild_id}#{ctx.channel_id}> " + resp_body["logStatement"]
        )
        return containers.create_error_container(
            "Backend Error",
            "BibleBot couldn't reach its backend in time, please try again in a moment.",
            i18n.get_i18n_or_default("en_US"),
        )

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
                    await webhooks.remove_webhooks(user, ctx.guild)
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
                    subsession = await http.get_session()
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
                        else constants.verse_footer
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

                returning_text = f"**{reference_title}**\n\n> {verse_title}{verse_text}\n\n-# {constants.logo_emoji} {resp_body['cultureFooter']}"

                if verse["reference"]["version"]["publisher"] is not None:
                    publisher_info = constants.publisher_to_url[
                        verse["reference"]["version"]["publisher"]
                    ]

                    if publisher_info is not None:
                        returning_text += (
                            f" ∙ [{publisher_info['name']}]({publisher_info['url']})"
                        )

                return returning_text
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

                returning_text = f"**{reference_title}**\n\n```json\n{verse_title} {verse_text}```\n\n-# {constants.logo_emoji} {resp_body['cultureFooter']}"

                if verse["reference"]["version"]["publisher"] is not None:
                    publisher_info = constants.publisher_to_url[
                        verse["reference"]["version"]["publisher"]
                    ]

                    if publisher_info is not None:
                        returning_text += (
                            f" ∙ [{publisher_info['name']}]({publisher_info['url']})"
                        )

                return returning_text
            return None
        return None
    return None


def _transport_failure(log_statement: str) -> dict:
    """A minimally-shaped response so callers don't KeyError on the failure path."""

    return {
        "ok": False,
        "culture": None,
        "type": None,
        "logStatement": log_statement,
        "transportFailure": True,
    }


async def submit_command_raw(endpoint: str, req_body: dict):
    """Submits a command to the backend and returns the result."""

    session = await http.get_session()

    try:
        async with session.post(
            f"{endpoint}/commands/process",
            json=req_body,
            headers=aiohttp_headers,
        ) as resp:
            resp_text = await resp.text()
            resp_text = resp_text.replace("\\\\n", "\\n")
    except (asyncio.TimeoutError, aiohttp.ClientError) as err:
        logger.error(
            f"couldn't reach backend at /commands/process: {err.__class__.__name__}: {err}"
        )
        return _transport_failure("backend unreachable")

    try:
        return json.loads(resp_text)
    except json.JSONDecodeError:
        logger.error(f"couldn't parse response from backend: {resp_text}")
        return _transport_failure("couldn't parse response")


async def submit_verse(rch: disnake.abc.Messageable, user: disnake.abc.User, body: str):
    """Submits a verse to the backend and processes the result."""
    ctx = await channels.get_channel_context_from_messageable(rch)

    if ctx is None or ctx.channel is None:
        return None

    req_body = {
        "UserId": user.id,
        "GuildId": ctx.guild_id,
        "ChannelId": ctx.channel_id,
        "ThreadId": ctx.thread_id,
        "IsThread": ctx.is_thread,
        "IsBot": user.bot,
        "IsDM": ctx.is_dm,
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
    elif isinstance(resp_body, list) and len(resp_body) > 0:
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

    session = await http.get_session()

    try:
        async with session.post(
            f"{endpoint}/verses/process",
            json=req_body,
            headers=aiohttp_headers,
        ) as resp:
            resp_body = await resp.json()
    except (asyncio.TimeoutError, aiohttp.ClientError, json.JSONDecodeError) as err:
        logger.error(
            f"<{req_body['UserId']}@{req_body['GuildId']}#{req_body['ChannelId']}> "
            f"couldn't reach backend at /verses/process: {err.__class__.__name__}: {err}"
        )
        # /verse renders None as CMD_VERSE_FAIL, and the on_message path stays
        # quiet - which is what we want when the backend is having a bad day.
        return None

    if resp_body["culture"] is not None:
        localization = i18n.get_i18n_or_default(resp_body["culture"].replace("-", "_"))
    else:
        localization = i18n.get_i18n_or_default("en_US")

    if resp_body["logStatement"]:
        logger.info(
            f"<{req_body['UserId']}@{req_body['GuildId']}#{req_body['ChannelId']}> "
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
                    else constants.verse_footer
                ),
                is_verses=True,
            )
            return ComponentPaginator(components, int(req_body["UserId"]))
        else:
            for verse in verses:
                processed_verses.append(
                    containers.convert_verse_to_container(
                        verse,
                        (
                            resp_body["cultureFooter"]
                            if resp_body["cultureFooter"] is not None
                            else constants.verse_footer
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
                f"**{reference_title}**\n\n> {verse_title}{verse_text}\n\n-# {constants.logo_emoji}  {resp_body['cultureFooter']}"
                + (
                    f" ∙ [{constants.publisher_to_url[verse['reference']['version']['publisher']]['name']}](<{constants.publisher_to_url[verse['reference']['version']['publisher']]['url']}>)"
                    if verse["reference"]["version"]["publisher"] is not None
                    else ""
                )
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
                f"**{reference_title}**\n\n```json\n{verse_title} {verse_text}```\n\n-# {constants.logo_emoji}  {resp_body['cultureFooter']}"
                + (
                    f" ∙ [{constants.publisher_to_url[verse['reference']['version']['publisher']]['name']}](<{constants.publisher_to_url[verse['reference']['version']['publisher']]['url']}>)"
                    if verse["reference"]["version"]["publisher"] is not None
                    else ""
                )
            )

    return processed_verses


async def check_if_staff(user_id: int) -> tuple[int, Optional[dict]]:
    """Returns the backend's staff-check status, and its body if it sent one.

    The body has to be read inside the response context - the connection is
    released on exit, so handing the response object back to the caller left
    them with something they couldn't read.
    """

    endpoint = os.environ.get("ENDPOINT", "")

    req_body = {
        "UserId": user_id,
        "GuildId": 0,
        "ChannelId": 0,
        "ThreadId": 0,
        "IsThread": False,
        "IsBot": False,
        "IsDM": True,
        "Body": "",
    }

    session = await http.get_session()

    try:
        async with session.post(
            f"{endpoint}/commands/staff_check",
            json=req_body,
            headers=aiohttp_headers,
        ) as resp:
            try:
                body = await resp.json()
            except (aiohttp.ClientError, json.JSONDecodeError):
                body = None

            return resp.status, body
    except (asyncio.TimeoutError, aiohttp.ClientError) as err:
        logger.error(
            f"couldn't reach backend at /commands/staff_check: {err.__class__.__name__}: {err}"
        )
        return 0, None
