"""
Copyright (C) 2016-2026 Kerygma Digital Co.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this file,
You can obtain one at https://mozilla.org/MPL/2.0/.
"""

from disnake import Guild, Thread
from disnake.abc import User
from disnake.channel import StageChannel, TextChannel, VoiceChannel
from helpers.channels import ChannelContext


async def remove_webhooks(user: User, guild: Guild):
    webhooks = await guild.webhooks()

    for webhook in webhooks:
        if webhook.user is not None:
            if webhook.user.id == guild.me.id:
                await webhook.delete(
                    reason=f"User ID {user.id} performed a command that removes BibleBot-related webhooks."
                )


async def create_webhook(ctx: ChannelContext):
    webhook = None
    webhook_service_body = None

    with open("./data/avatar.png", "rb") as image:
        if isinstance(ctx.channel, Thread) and ctx.channel.parent is not None:
            webhook = await ctx.channel.parent.create_webhook(
                name="BibleBot Automatic Daily Verses",
            )
            webhook_service_body = (
                f"{webhook.id}/{webhook.token}?thread_id={ctx.thread_id}"
            )
        elif isinstance(ctx.channel, (TextChannel, VoiceChannel, StageChannel)):
            webhook = await ctx.channel.create_webhook(
                name="BibleBot Automatic Daily Verses",
                avatar=bytearray(image.read()),
                reason="For automatic daily verses from BibleBot.",
            )
            webhook_service_body = f"{webhook.id}/{webhook.token}"

    return webhook_service_body
