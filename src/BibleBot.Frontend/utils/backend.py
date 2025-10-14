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
from utils import sending
from utils import statics
from utils import webhooks
from utils.views import CreatePaginator
from utils.i18n import i18n as i18n_class
from utils import channels

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
                                return convert_embed(resp_body["pages"][0])
                except disnake.errors.Forbidden:
                    await sending.safe_send_channel(
                        ctx.channel,
                        embed=create_error_embed(
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
                                return convert_embed(resp_body["pages"][0])
                except disnake.errors.Forbidden:
                    try:
                        await sending.safe_send_channel(
                            ctx.channel,
                            embed=create_error_embed(
                                "/dailyverseset",
                                localization["WEBHOOK_CREATION_FAILURE"],
                                localization,
                            ),
                        )
                    except disnake.errors.Forbidden:
                        logger.error(
                            f"unable to add webhook for <{user.id}@{ctx.guild_id}#{ctx.channel_id}>"
                        )

            return convert_embed(resp_body["pages"][0])
        else:
            return create_pagination_embeds(resp_body["pages"], localization)
    elif resp_body["type"] == "verse":
        if "does not support the" in resp_body["logStatement"]:
            return create_error_embed(
                "Verse Error", resp_body["logStatement"], localization
            )
        elif "too many verses" in resp_body["logStatement"]:
            return convert_embed(resp_body["pages"][0])

        display_style = resp_body["displayStyle"]
        if display_style == "embed":
            for verse in resp_body["verses"]:
                return create_embed_from_verse(
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
            resp_text = resp_text.replace("\\\\", "\\")

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

    if isinstance(resp_body, disnake.Embed):
        await sending.safe_send_channel(ctx.channel, embed=resp_body)
    elif isinstance(resp_body, CreatePaginator):
        await sending.safe_send_channel(
            ctx.channel, embed=resp_body.embeds[0], view=resp_body
        )
    elif isinstance(resp_body, list):
        if isinstance(resp_body[0], disnake.Embed):
            await sending.safe_send_channel(ctx.channel, embeds=resp_body)
        elif isinstance(resp_body[0], str):
            for item in resp_body:
                await sending.safe_send_channel(ctx.channel, item)

    return req_body, resp_body


async def submit_verse_raw(
    endpoint: str, req_body: dict, is_command: bool = False
) -> Optional[Union[disnake.Embed, list[str], list[disnake.Embed], CreatePaginator]]:
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
            return create_error_embed(
                "Verse Error", resp_body["logStatement"], localization
            )
        elif "too many verses" in resp_body["logStatement"]:
            return convert_embed(resp_body["pages"][0])

    if "verses" not in resp_body:
        if "pages" in resp_body:
            return convert_embed(resp_body["pages"][0])
        return

    verses = resp_body["verses"]
    processed_verses = []

    display_style = resp_body["displayStyle"]
    if display_style == "embed":
        if resp_body["paginate"] and len(verses) > 1:
            embeds = create_pagination_embeds(
                verses,
                (
                    resp_body["cultureFooter"]
                    if resp_body["cultureFooter"] is not None
                    else statics.verse_footer
                ),
                is_verses=True,
            )
            return CreatePaginator(embeds, int(req_body["user_id"]), 180)
        else:
            for verse in verses:
                processed_verses.append(
                    create_embed_from_verse(
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


def convert_embed(internal_embed):
    embed = disnake.Embed()

    embed.title = internal_embed["title"]
    embed.description = internal_embed["description"]
    embed.url = internal_embed["url"]
    embed.color = internal_embed["color"]

    if internal_embed["fields"] is not None:
        for field in internal_embed["fields"]:
            embed.add_field(
                name=field["name"], value=field["value"], inline=field["inline"]
            )

    embed.set_footer(
        text=internal_embed["footer"]["text"],
        icon_url=internal_embed["footer"]["icon_url"],
    )

    return embed


def create_embed_from_verse(verse, localization):
    embed = disnake.Embed()

    reference_title = (
        verse["reference"]["asString"] + " - " + verse["reference"]["version"]["name"]
    )

    if verse["reference"]["version"]["publisher"] == "biblica":
        embed.set_author(name=reference_title + " (Biblica)", url="https://biblica.com")
    else:
        embed.set_author(name=reference_title)

    embed.title = verse["title"]
    embed.description = verse["text"]
    embed.color = 6709986

    embed.set_footer(
        text=localization.replace("{0}", statics.version),
        icon_url="https://i.imgur.com/hr4RXpy.png",
    )

    return embed


def create_error_embed(title, description, localization):
    embed = disnake.Embed()

    embed.title = title
    embed.description = description
    embed.color = 16723502

    embed.set_footer(
        text=localization["EMBED_FOOTER"].replace("<v>", statics.version),
        icon_url="https://i.imgur.com/hr4RXpy.png",
    )

    return embed


def create_pagination_embeds(pages, localization, is_verses=False):
    embeds = []

    for page in pages:
        if is_verses:
            embeds.append(create_embed_from_verse(page, localization))
        else:
            embeds.append(convert_embed(page))

    return embeds
