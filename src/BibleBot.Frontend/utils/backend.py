"""
    Copyright (C) 2016-2022 Kerygma Digital Co.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this file,
    You can obtain one at https://mozilla.org/MPL/2.0/.
"""

import os
import requests
import disnake
from logger import VyLogger

logger = VyLogger("default")


async def submit_command(
    rch: disnake.abc.Messageable, user: disnake.abc.User, body: str
):
    ch = await rch._get_channel()

    isDM = ch.type == disnake.ChannelType.private
    guildId = ch.id if isDM else user.id

    reqbody = {
        "UserId": str(user.id),
        "GuildId": str(guildId),
        "IsDM": isDM,
        "Body": body,
        "Token": os.environ.get("ENDPOINT_TOKEN"),
    }

    endpoint = os.environ.get("ENDPOINT")
    resp = requests.post(f"{endpoint}/commands/process", json=reqbody)

    print(reqbody)
    print(resp.json())

    if resp.json()["type"] == "cmd":
        if len(resp.json()["pages"]) == 1:
            # todo: webhook stuff should not be dailyverse-specific
            if resp.json()["removeWebhook"] and not isDM:
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
                            "/autodailyverse",
                            "I was unable to remove our existing webhooks for this server. I need the **`Manage Webhooks`** permission to manage automatic daily verses.",
                        )
                    )

            if resp.json()["createWebhook"] and not isDM:
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
                    requests.post(f"{endpoint}/webhooks/process", json=reqbody)

                    return convert_embed(resp.json()["pages"][0])
                except disnake.errors.Forbidden:
                    await ch.send(
                        embed=create_error_embed(
                            "/autodailyverse",
                            "I was unable to create a webhook for this channel. I need the **`Manage Webhooks`** permission to enable automatic daily verses.",
                        )
                    )

            return convert_embed(resp.json()["pages"][0])
        else:
            return create_pagination_embeds_from_pages(resp.json()["pages"])
    elif resp.json()["type"] == "verse":
        if not resp.json()["paginate"] and resp.json()["displayStyle"] == "embed":
            if len(resp.json()["verses"]) == 1:
                return create_embed_from_verse(resp.json()["verses"][0])
            else:
                # todo
                pass


async def submit_command_raw(
    rch: disnake.abc.Messageable, user: disnake.abc.User, body: str
):
    ch = await rch._get_channel()

    isDM = ch.type == disnake.ChannelType.private
    guildId = ch.id if isDM else user.id

    reqbody = {
        "UserId": str(user.id),
        "GuildId": str(guildId),
        "IsDM": isDM,
        "Body": body,
        "Token": os.environ.get("ENDPOINT_TOKEN"),
    }

    endpoint = os.environ.get("ENDPOINT")
    resp = requests.post(f"{endpoint}/commands/process", json=reqbody)

    return resp.json()


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

    # For whatever reason, the paginator library has the buttons
    # performing the opposite effect, "next" goes to the previous
    # page and vice versa. This reverses the array and makes sure
    # the first page is properly the first embed, which is still a
    # requirement despite the paginator working backwards.
    #
    # I could fix this myself by forking the library
    # (it's a two-line fix), but I'm too lazy for that.
    for page in pages[::-1]:
        page_embed = convert_embed(page)

        if f"Page 1 of" in page_embed.title:
            starting_page = page_embed
        else:
            embeds.append(page_embed)

    embeds.insert(0, starting_page)

    return embeds
