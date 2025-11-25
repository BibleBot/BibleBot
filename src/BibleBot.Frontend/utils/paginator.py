"""
Copyright (C) 2016-2025 Kerygma Digital Co.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this file,
You can obtain one at https://mozilla.org/MPL/2.0/.
"""

import time
import asyncio
from dataclasses import dataclass, field
from disnake import (
    ButtonStyle,
    DMChannel,
    GroupChannel,
    Message,
    TextChannel,
    Thread,
    VoiceChannel,
    StageChannel,
)
from disnake.interactions import MessageInteraction, ApplicationCommandInteraction
from disnake.ui._types import MessageComponents
from disnake.ui import Container, Button, ActionRow
from utils.i18n import i18n as i18n_class
from typing import List, Dict, Optional

i18n = i18n_class()


@dataclass
class PaginationState:
    author_id: int
    message: Message
    index: int = 0
    pages: List[Container] = field(default_factory=list)
    last_interacted: float = field(default_factory=time.time)
    task: Optional[asyncio.Task] = field(default=None)


class ComponentPaginator:
    """
    Paginator for containers.
    Parameters:
    ----------
    pages: List[Container]
        List of containers which are in the paginator. Paginator starts from first container.
    author: int
        The ID of the author who can interact with the buttons.
    """

    _registry: Dict[int, PaginationState] = {}  # message_id -> pagination state
    _timeout = 180.0

    def __init__(self, pages: List[Container], author_id: int):
        self.pages = pages
        self.author_id = author_id

    def _render(self, index: int, disabled: bool = False) -> MessageComponents:
        if disabled:
            return [self.pages[index]]

        previous_button = Button(
            emoji="⬅️",
            style=ButtonStyle.secondary,
            custom_id="pagination:prev",
        )
        next_button = Button(
            emoji="➡️",
            style=ButtonStyle.secondary,
            custom_id="pagination:next",
        )

        row = ActionRow(previous_button, next_button)
        return [self.pages[index], row]

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
        components = self._render(0)

        if isinstance(sendable, (MessageInteraction, ApplicationCommandInteraction)):
            msg = await sendable.followup.send(components=components, wait=True)
        else:
            msg = await sendable.send(components=components)

        self._registry[msg.id] = PaginationState(
            author_id=self.author_id,
            pages=self.pages,
            index=0,
            message=msg,
            task=asyncio.create_task(self._auto_disable_runner(msg.id)),
        )

        return msg

    @classmethod
    async def _handle_click(cls, inter: MessageInteraction):
        localization = i18n.get_i18n_or_default(inter.locale.name)

        state = cls._registry.get(inter.message.id)
        if state is None:
            return

        if inter.author.id != state.author_id:
            await inter.send(localization["PAGINATOR_FORBIDDEN"], ephemeral=True)
            return

        state.last_interacted = time.time()
        if state.task and not state.task.done():
            state.task.cancel()
        state.task = asyncio.create_task(cls._auto_disable_runner(inter.message.id))

        custom_id = inter.data.custom_id
        if custom_id == "pagination:prev":
            state.index = (state.index - 1) % len(state.pages)
        elif custom_id == "pagination:next":
            state.index = (state.index + 1) % len(state.pages)
        else:
            return

        await inter.followup.edit_message(
            inter.message.id,
            components=cls(state.pages, state.author_id)._render(state.index),
        )

    @classmethod
    async def _auto_disable_runner(cls, message_id: int):
        try:
            state = cls._registry.get(message_id)
            if not state:
                return

            while True:
                now = time.time()
                remaining = cls._timeout - (now - state.last_interacted)

                if remaining <= 0:
                    break

                await asyncio.sleep(min(remaining, 5))

            disabled = cls(state.pages, state.author_id)._render(
                state.index, disabled=True
            )

            try:
                await state.message.edit(components=disabled)
            except Exception:
                pass

            cls._registry.pop(message_id, None)
        except asyncio.CancelledError:
            return
