"""
Copyright (C) 2016-2025 Kerygma Digital Co.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this file,
You can obtain one at https://mozilla.org/MPL/2.0/.
"""

from disnake import ui, User, Member
from disnake.ui import Container, TextDisplay, Button, ActionRow
from dataclasses import dataclass, field
from utils.i18n import i18n as i18n_class
from typing import List, Dict, Optional
from disnake.ui._types import MessageComponents
from disnake import (
    ButtonStyle,
    DMChannel,
    GroupChannel,
    Message,
    TextChannel,
    Thread,
    VoiceChannel,
    StageChannel,
    SeparatorSpacing,
)
from disnake.interactions import MessageInteraction, ApplicationCommandInteraction
import time
import asyncio
import backend, containers, statics

i18n = i18n_class()


@dataclass
class ConfirmationPromptState:
    author: User | Member
    message: Message
    on_confirm_command: str
    created_at: float = field(default_factory=time.time)
    task: Optional[asyncio.Task] = field(default=None)
    localization: Dict[str, str] = field(default_factory=Dict[str, str])


class ConfirmationPrompt:
    _registry: Dict[int, ConfirmationPromptState] = {}
    _timeout = 20.0

    def __init__(self, on_confirm_command: str, author: User | Member, localization):
        self.author = author
        self.on_confirm_command = on_confirm_command
        self.localization = localization

    def _render(self, response=None) -> MessageComponents:
        if response is not None:
            return [response]

        container = Container()
        container.accent_color = 16776960

        container.children.append(
            ui.TextDisplay(f"### {self.localization["CONFIRMATION_REQUIRED_TITLE"]}")
        )

        container.children.append(
            ui.TextDisplay(
                f"{self.localization[
                    "CONFIRMATION_REQUIRED_SETDAILYVERSEROLE_EVERYONE"
                ]}"
            )
        )

        container.children.append(
            ui.Separator(divider=True, spacing=SeparatorSpacing.large)
        )

        container.children.append(
            ui.TextDisplay(
                f"-# {statics.logo_emoji}  **{self.localization["EMBED_FOOTER"].replace("<v>", statics.version)}**"
            )
        )

        yes_button = Button(
            emoji="✅",
            style=ButtonStyle.green,
            custom_id="confirmation:yes",
        )
        no_button = Button(
            emoji="✖️",
            style=ButtonStyle.red,
            custom_id="confirmation:no",
        )

        row = ActionRow(yes_button, no_button)
        return [container, row]

    async def send(
        self,
        sendable: (
            TextChannel
            | Thread
            | VoiceChannel
            | StageChannel
            | DMChannel
            | GroupChannel
            | MessageInteraction
            | ApplicationCommandInteraction
        ),
    ):
        components = self._render()

        if isinstance(sendable, (MessageInteraction, ApplicationCommandInteraction)):
            msg = await sendable.followup.send(components=components, wait=True)
        else:
            msg = await sendable.send(components=components)

        self._registry[msg.id] = ConfirmationPromptState(
            author=self.author,
            on_confirm_command=self.on_confirm_command,
            message=msg,
            task=asyncio.create_task(self._auto_disable_runner(msg.id)),
            localization=self.localization,
        )

        return msg

    @classmethod
    async def _handle_click(cls, inter: MessageInteraction):
        state = cls._registry.get(inter.message.id)
        if state is None:
            return

        localization = state.localization

        if inter.author.id != state.author.id:
            await inter.send(localization["PAGINATOR_FORBIDDEN"], ephemeral=True)
            return

        if state.task and not state.task.done():
            state.task.cancel()

        components_to_send = None

        custom_id = inter.data.custom_id
        if custom_id == "confirmation:yes":
            components_to_send = await backend.submit_command(
                inter.channel, state.author, state.on_confirm_command
            )
        elif custom_id == "confirmation:no":
            components_to_send = containers.create_error_container(
                localization["CONFIRMATION_REJECTED_TITLE"],
                localization["CONFIRMATION_REJECTED_DESC"],
                localization,
            )
        else:
            return

        await inter.followup.edit_message(
            inter.message.id,
            components=cls(
                state.on_confirm_command, state.author, localization
            )._render(response=components_to_send),
        )

    @classmethod
    async def _auto_disable_runner(cls, message_id: int):
        try:
            state = cls._registry.get(message_id)
            if not state:
                return

            while True:
                now = time.time()
                remaining = cls._timeout - (now - state.created_at)

                if remaining <= 0:
                    break

                await asyncio.sleep(min(remaining, 5))

            components_to_send = containers.create_error_container(
                state.localization["CONFIRMATION_REJECTED_TITLE"],
                state.localization["CONFIRMATION_REJECTED_DESC"],
                state.localization,
            )

            disabled = cls(
                state.on_confirm_command, state.author, state.localization
            )._render(response=components_to_send)

            try:
                await state.message.edit(components=disabled)
            except Exception:
                pass

            cls._registry.pop(message_id, None)
        except asyncio.CancelledError:
            return
