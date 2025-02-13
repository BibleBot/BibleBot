"""
    Copyright (C) 2016-2025 Kerygma Digital Co.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this file,
    You can obtain one at https://mozilla.org/MPL/2.0/.
"""

import os
import aiohttp
import disnake
from disnake.ext import commands
from logger import VyLogger
from utils import sending
from utils import statics

from utils.paginator import CreatePaginator

logger = VyLogger("default")


async def submit_command(
    rch: disnake.abc.Messageable,
    user: disnake.abc.User,
    body: str,
):
    try:
        ch = await rch._get_channel()
    except AttributeError:
        # In this scenario, we've got something that
        # should inherit Messageable in disnake but
        # has not been implemented.
        #
        # May seem silly to have this, but we've been
        # fooled before.
        return

    isDM = ch.type == disnake.ChannelType.private
    isThread = (
        True
        if ch.type
        in [
            disnake.ChannelType.news_thread,
            disnake.ChannelType.public_thread,
            disnake.ChannelType.private_thread,
        ]
        else False
    )

    guildId = ch.id if isDM else ch.guild.id
    channelId = ch.id

    if isThread:
        channelId = ch.parent.id

    reqbody = {
        "UserId": str(user.id),
        "GuildId": str(guildId),
        "ChannelId": str(channelId),
        "ThreadId": str(ch.id),
        "IsThread": isThread,
        "IsBot": user.bot,
        "IsDM": isDM,
        "Body": body,
    }

    endpoint = os.environ.get("ENDPOINT")

    async with aiohttp.ClientSession() as session:
        async with session.post(
            f"{endpoint}/commands/process",
            json=reqbody,
            headers={"Authorization": os.environ.get("ENDPOINT_TOKEN")},
        ) as resp:
            respBody = await resp.json()

            if respBody["ok"]:
                logger.info(
                    f"<{user.id}@{guildId}#{ch.id}> " + respBody["logStatement"]
                )
            else:
                logger.error(
                    f"<{user.id}@{guildId}#{ch.id}> " + respBody["logStatement"]
                )

            if respBody["type"] == "cmd":
                if len(respBody["pages"]) == 1:
                    # todo: webhook stuff should not be dailyverse-specific
                    if respBody["removeWebhook"] and not isDM:
                        try:
                            webhooks = await ch.guild.webhooks()

                            for webhook in webhooks:
                                if webhook.user is not None:
                                    if webhook.user.id == ch.guild.me.id:
                                        await webhook.delete(
                                            reason=f"User ID {user.id} performed a command that removes BibleBot-related webhooks."
                                        )
                        except disnake.errors.Forbidden:
                            await sending.safe_send_channel(
                                ch,
                                embed=create_error_embed(
                                    "Permissions Error",
                                    "I was unable to remove our existing webhooks for this server. I need the **`Manage Webhooks`** permission to manage automatic daily verses.",
                                ),
                            )

                    if respBody["createWebhook"] and not isDM:
                        try:
                            webhook_service_body = None
                            # Unlike other libraries, we have to convert an
                            # image into bytes to pass as the webhook avatar.
                            with open("./data/avatar.png", "rb") as image:
                                if isThread:
                                    webhook = await ch.parent.create_webhook(  # type: ignore
                                        name="BibleBot Automatic Daily Verses",
                                        avatar=bytearray(image.read()),
                                        reason="For automatic daily verses from BibleBot.",
                                    )
                                    webhook_service_body = f"{webhook.id}/{webhook.token}?thread_id={ch.id}"
                                else:
                                    webhook = await ch.create_webhook(  # type: ignore
                                        name="BibleBot Automatic Daily Verses",
                                        avatar=bytearray(image.read()),
                                        reason="For automatic daily verses from BibleBot.",
                                    )
                                    webhook_service_body = (
                                        f"{webhook.id}/{webhook.token}"
                                    )

                            # Send a request to the webhook controller, which will update the DB.
                            reqbody["Body"] = webhook_service_body
                            async with aiohttp.ClientSession() as subsession:
                                async with subsession.post(
                                    f"{endpoint}/webhooks/process",
                                    json=reqbody,
                                    headers={
                                        "Authorization": os.environ.get(
                                            "ENDPOINT_TOKEN"
                                        )
                                    },
                                ) as subresp:
                                    if subresp.status != 200:
                                        logger.error("couldn't submit webhook")
                                    else:
                                        return convert_embed(respBody["pages"][0])
                        except disnake.errors.Forbidden:
                            try:
                                await sending.safe_send_channel(
                                    ch,
                                    embed=create_error_embed(
                                        "/dailyverseset",
                                        "I was unable to create a webhook for this channel. I need the **`Manage Webhooks`** permission to enable automatic daily verses.",
                                    ),
                                )
                            except disnake.errors.Forbidden:
                                logger.error(
                                    f"unable to add webhook for <{user.id}@{guildId}#{ch.id}>"
                                )

                    return convert_embed(respBody["pages"][0])
                else:
                    return create_pagination_embeds(respBody["pages"])
            elif respBody["type"] == "verse":
                if "does not support the" in respBody["logStatement"]:
                    return create_error_embed("Verse Error", respBody["logStatement"])
                elif "too many verses" in respBody["logStatement"]:
                    return convert_embed(respBody["pages"][0])

                display_style = respBody["displayStyle"]
                if display_style == "embed":
                    for verse in respBody["verses"]:
                        return create_embed_from_verse(verse)
                elif display_style == "blockquote":
                    for verse in respBody["verses"]:
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
                elif display_style == "code":
                    for verse in respBody["verses"]:
                        reference_title = (
                            verse["reference"]["asString"]
                            + " - "
                            + verse["reference"]["version"]["name"]
                        )
                        verse_title = (
                            (verse["title"] + "\n\n") if len(verse["title"]) > 0 else ""
                        )
                        verse_text = verse["text"].replace("*", "")

                        return f"**{reference_title}**\n\n```json\n{verse_title} {verse_text}```"


async def submit_command_raw(
    rch: disnake.abc.Messageable,
    user: disnake.abc.User,
    body: str,
):
    try:
        ch = await rch._get_channel()
    except AttributeError:
        # In this scenario, we've got something that
        # should inherit Messageable in disnake but
        # has not been implemented.
        #
        # May seem silly to have this, but we've been
        # fooled before.
        return

    isDM = ch.type == disnake.ChannelType.private
    isThread = (
        True
        if ch.type
        in [
            disnake.ChannelType.news_thread,
            disnake.ChannelType.public_thread,
            disnake.ChannelType.private_thread,
        ]
        else False
    )

    guildId = ch.id if isDM else ch.guild.id
    channelId = ch.id

    if isThread:
        channelId = ch.parent.id

    reqbody = {
        "UserId": str(user.id),
        "GuildId": str(guildId),
        "ChannelId": str(channelId),
        "ThreadId": str(ch.id),
        "IsThread": isThread,
        "IsBot": user.bot,
        "IsDM": isDM,
        "Body": body,
    }

    endpoint = os.environ.get("ENDPOINT")

    async with aiohttp.ClientSession() as session:
        async with session.post(
            f"{endpoint}/commands/process",
            json=reqbody,
            headers={"Authorization": os.environ.get("ENDPOINT_TOKEN")},
        ) as resp:
            respBody = await resp.json()
            return respBody


async def submit_verse(
    rch: disnake.abc.Messageable,
    user: disnake.abc.User,
    body: str,
):
    try:
        ch = await rch._get_channel()
    except AttributeError:
        # In this scenario, we've got something that
        # should inherit Messageable in disnake but
        # has not been implemented.
        #
        # May seem silly to have this, but we've been
        # fooled before.
        return None, None

    isDM = ch.type == disnake.ChannelType.private
    isThread = (
        True
        if ch.type
        in [
            disnake.ChannelType.news_thread,
            disnake.ChannelType.public_thread,
            disnake.ChannelType.private_thread,
        ]
        else False
    )

    guildId = ch.id if isDM else ch.guild.id
    channelId = ch.parent_id if isThread else ch.id

    reqbody = {
        "UserId": str(user.id),
        "GuildId": str(guildId),
        "ChannelId": str(channelId),
        "ThreadId": str(ch.id),
        "IsThread": isThread,
        "IsBot": user.bot,
        "IsDM": isDM,
        "Body": body,
    }

    endpoint = os.environ.get("ENDPOINT")

    async with aiohttp.ClientSession() as session:
        async with session.post(
            f"{endpoint}/verses/process",
            json=reqbody,
            headers={"Authorization": os.environ.get("ENDPOINT_TOKEN")},
        ) as resp:
            respBody = await resp.json()

            if respBody["logStatement"]:
                logger.info(
                    f"<{user.id}@{guildId}#{ch.id}> " + respBody["logStatement"]
                )

            if respBody["logStatement"]:
                if "does not support the" in respBody["logStatement"]:
                    await sending.safe_send_channel(
                        ch,
                        embed=create_error_embed(
                            "Verse Error", respBody["logStatement"]
                        ),
                    )
                    return (reqbody, respBody)
                elif "too many verses" in respBody["logStatement"]:
                    await sending.safe_send_channel(
                        ch, embed=convert_embed(respBody["pages"][0])
                    )
                    return (reqbody, respBody)

            if "verses" not in respBody:
                if "pages" in respBody:
                    await sending.safe_send_channel(
                        ch, embed=convert_embed(respBody["pages"][0])
                    )
                return (reqbody, respBody)

            verses = respBody["verses"]  # todo: remove duplicate verses

            display_style = respBody["displayStyle"]
            if display_style == "embed":
                if respBody["paginate"] and len(verses) > 1:
                    embeds = create_pagination_embeds(verses, is_verses=True)
                    paginator = CreatePaginator(embeds, user.id, 180)

                    await sending.safe_send_channel(ch, embed=embeds[0], view=paginator)
                else:
                    for verse in verses:
                        await sending.safe_send_channel(
                            ch, embed=create_embed_from_verse(verse)
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

                    await sending.safe_send_channel(
                        ch,
                        f"**{reference_title}**\n\n> {verse_title}{verse_text}",
                    )
            elif display_style == "code":
                for verse in verses:
                    reference_title = (
                        verse["reference"]["asString"]
                        + " - "
                        + verse["reference"]["version"]["name"]
                    )
                    verse_title = (
                        (verse["title"] + "\n\n") if len(verse["title"]) > 0 else ""
                    )
                    verse_text = verse["text"].replace("*", "")

                    await sending.safe_send_channel(
                        ch,
                        f"**{reference_title}**\n\n```json\n{verse_title} {verse_text}```",
                    )

            return (reqbody, respBody)


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


def create_embed_from_verse(verse):
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
        text=f"BibleBot v{statics.version} by Kerygma Digital",
        icon_url="https://i.imgur.com/hr4RXpy.png",
    )

    return embed


def create_error_embed(title, description):
    embed = disnake.Embed()

    embed.title = title
    embed.description = description
    embed.color = 16723502

    embed.set_footer(
        text=f"BibleBot v{statics.version} by Kerygma Digital",
        icon_url="https://i.imgur.com/hr4RXpy.png",
    )

    return embed


def create_pagination_embeds(pages, is_verses=False):
    embeds = []

    for page in pages:
        if is_verses:
            embeds.append(create_embed_from_verse(page))
        else:
            embeds.append(convert_embed(page))

    return embeds
