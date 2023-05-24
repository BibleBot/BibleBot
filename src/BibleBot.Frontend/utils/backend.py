"""
    Copyright (C) 2016-2023 Kerygma Digital Co.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this file,
    You can obtain one at https://mozilla.org/MPL/2.0/.
"""

import os
import aiohttp
import disnake
from logger import VyLogger

from utils.paginator import CreatePaginator

logger = VyLogger("default")


async def submit_command(
    rch: disnake.abc.Messageable, user: disnake.abc.User, body: str
):
    ch = await rch._get_channel()

    isDM = ch.type == disnake.ChannelType.private
    guildId = ch.id if isDM else ch.guild.id

    reqbody = {
        "UserId": str(user.id),
        "GuildId": str(guildId),
        "IsDM": isDM,
        "Body": body,
        "Token": os.environ.get("ENDPOINT_TOKEN"),
    }

    endpoint = os.environ.get("ENDPOINT")

    async with aiohttp.ClientSession() as session:
        async with session.post(f"{endpoint}/commands/process", json=reqbody) as resp:
            respBody = await resp.json()

            if respBody["ok"]:
                logger.info(
                    f"<{user.id}#{ch.id}@{guildId}> " + respBody["logStatement"]
                )
            else:
                logger.error(
                    f"<{user.id}#{ch.id}@{guildId}> " + respBody["logStatement"]
                )

            if respBody["type"] == "cmd":
                if len(respBody["pages"]) == 1:
                    # todo: webhook stuff should not be dailyverse-specific
                    if respBody["removeWebhook"] and not isDM:
                        try:
                            webhooks = await ch.guild.webhooks()

                            for webhook in webhooks:
                                if webhook.user.id == ch.guild.me.id:
                                    await webhook.delete(
                                        reason=f"User ID {user.id} performed a command that removes BibleBot-related webhooks."
                                    )
                        except disnake.errors.Forbidden:
                            await ch.send(
                                embed=create_error_embed(
                                    "/dailyverseset",
                                    "I was unable to remove our existing webhooks for this server. I need the **`Manage Webhooks`** permission to manage automatic daily verses.",
                                )
                            )

                    if respBody["createWebhook"] and not isDM:
                        try:
                            # Unlike other libraries, we have to convert an
                            # image into bytes to pass as the webhook avatar.
                            with open("./data/avatar.png", "rb") as image:
                                webhook = await ch.create_webhook(
                                    name="BibleBot Automatic Daily Verses",
                                    avatar=bytearray(image.read()),
                                    reason="For automatic daily verses from BibleBot.",
                                )

                            # Send a request to the webhook controller, which will update the DB.
                            reqbody["Body"] = f"{webhook.id}/{webhook.token}||{ch.id}"
                            async with aiohttp.ClientSession() as subsession:
                                async with subsession.post(
                                    f"{endpoint}/webhooks/process", json=reqbody
                                ) as subresp:
                                    if subresp.status != 200:
                                        logger.error("couldn't submit webhook")
                                    else:
                                        return convert_embed(respBody["pages"][0])
                        except disnake.errors.Forbidden:
                            await ch.send(
                                embed=create_error_embed(
                                    "/dailyverseset",
                                    "I was unable to create a webhook for this channel. I need the **`Manage Webhooks`** permission to enable automatic daily verses.",
                                )
                            )

                    return convert_embed(respBody["pages"][0])
                else:
                    return create_pagination_embeds_from_pages(respBody["pages"])
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
    rch: disnake.abc.Messageable, user: disnake.abc.User, body: str
):
    ch = await rch._get_channel()

    isDM = ch.type == disnake.ChannelType.private
    guildId = ch.id if isDM else ch.guild.id

    reqbody = {
        "UserId": str(user.id),
        "GuildId": str(guildId),
        "IsDM": isDM,
        "Body": body,
        "Token": os.environ.get("ENDPOINT_TOKEN"),
    }

    endpoint = os.environ.get("ENDPOINT")

    async with aiohttp.ClientSession() as session:
        async with session.post(f"{endpoint}/commands/process", json=reqbody) as resp:
            respBody = await resp.json()
            return respBody


async def submit_verse(rch: disnake.abc.Messageable, user: disnake.abc.User, body: str):
    ch = await rch._get_channel()

    isDM = ch.type == disnake.ChannelType.private
    guildId = ch.id if isDM else ch.guild.id

    reqbody = {
        "UserId": str(user.id),
        "GuildId": str(guildId),
        "IsDM": isDM,
        "IsBot": user.bot,
        "Body": body,
        "Token": os.environ.get("ENDPOINT_TOKEN"),
    }

    endpoint = os.environ.get("ENDPOINT")

    async with aiohttp.ClientSession() as session:
        async with session.post(f"{endpoint}/verses/process", json=reqbody) as resp:
            respBody = await resp.json()

            if respBody["logStatement"]:
                logger.info(
                    f"<{user.id}#{ch.id}@{guildId}> " + respBody["logStatement"]
                )

            if respBody["logStatement"]:
                if "does not support the" in respBody["logStatement"]:
                    await ch.send(
                        embed=create_error_embed(
                            "Verse Error", respBody["logStatement"]
                        )
                    )
                    return
                elif "too many verses" in respBody["logStatement"]:
                    await ch.send(embed=convert_embed(respBody["pages"][0]))
                    return

            if respBody["verses"] is None:
                return

            verses = respBody["verses"]  # todo: remove duplicate verses

            display_style = respBody["displayStyle"]
            if display_style == "embed":
                if respBody["paginate"] and len(verses) > 1:
                    embeds = create_pagination_embeds_from_verses(verses)
                    paginator = CreatePaginator(embeds, user.id, 180)

                    await ch.send(embed=embeds[0], view=paginator)
                else:
                    for verse in verses:
                        await ch.send(embed=create_embed_from_verse(verse))
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

                    await ch.send(
                        f"**{reference_title}**\n\n> {verse_title}{verse_text}"
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

                    await ch.send(
                        f"**{reference_title}**\n\n```json\n{verse_title} {verse_text}```"
                    )


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

    embed.set_author(name=reference_title)
    embed.title = verse["title"]
    embed.description = verse["text"]
    embed.color = 6709986

    embed.set_footer(
        text="BibleBot v9.2-beta by Kerygma Digital",
        icon_url="https://i.imgur.com/hr4RXpy.png",
    )

    return embed


def create_error_embed(title, description):
    embed = disnake.Embed()

    embed.title = title
    embed.description = description
    embed.color = 16723502

    embed.set_footer(
        text="BibleBot v9.2-beta by Kerygma Digital",
        icon_url="https://i.imgur.com/hr4RXpy.png",
    )

    return embed


def create_pagination_embeds_from_pages(pages):
    embeds = []
    starting_page = None

    for page in pages:
        page_embed = convert_embed(page)

        if f"Page 1 of" in page_embed.title:
            starting_page = page_embed
        else:
            embeds.append(page_embed)

    if starting_page == None:
        starting_page = embeds.pop()

    embeds.insert(0, starting_page)

    return embeds


def create_pagination_embeds_from_verses(verses):
    embeds = []
    starting_verse = None

    for verse in verses:
        verse_embed = create_embed_from_verse(verse)

        if verse == verses[0]:
            starting_verse = verse_embed
        else:
            embeds.append(verse_embed)

    embeds.insert(0, starting_verse)

    return embeds
