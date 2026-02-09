"""
Copyright (C) 2016-2026 Kerygma Digital Co.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this file,
You can obtain one at https://mozilla.org/MPL/2.0/.
"""

import asyncio
import time
from dataclasses import dataclass, field
from typing import Dict, Optional

from core import constants
from core.i18n import bb_i18n
from disnake import (
    ButtonStyle,
    DMChannel,
    GroupChannel,
    Member,
    Message,
    SeparatorSpacing,
    StageChannel,
    TextChannel,
    Thread,
    User,
    VoiceChannel,
    ui,
)
from disnake.interactions import ApplicationCommandInteraction, MessageInteraction
from disnake.ui import ActionRow, Button, Container
from disnake.ui._types import MessageComponents
from services import experiments
from ui import renderers

i18n = bb_i18n()


@dataclass
class HelpfulnessPromptState:
    author: User | Member
    message: Message
    experiment_name: str
    title: str
    description: str
    created_at: float = field(default_factory=time.time)
    task: Optional[asyncio.Task] = field(default=None)
    localization: Dict[str, str] = field(default_factory=Dict[str, str])


class HelpfulnessPrompt:
    _registry: Dict[int, HelpfulnessPromptState] = {}
    _timeout = 20.0

    def __init__(
        self,
        author: User | Member,
        experiment_name: str,
        title: str,
        description: str,
        localization,
    ):
        self.author = author
        self.experiment_name = experiment_name
        self.localization = localization
        self.title = title
        self.description = description

    def _render(self, response=None) -> MessageComponents:
        if response is not None:
            return [response]

        container = Container()
        container.accent_color = 16776960

        container.children.append(ui.TextDisplay(f"### {self.title}"))
        container.children.append(ui.TextDisplay(f"{self.description}"))

        container.children.append(
            ui.Separator(divider=True, spacing=SeparatorSpacing.large)
        )

        container.children.append(
            ui.TextDisplay(
                f"-# {constants.logo_emoji}  **{self.localization['EMBED_FOOTER'].replace('<v>', constants.version)}**"
            )
        )

        yes_button = Button(
            emoji="✅",
            style=ButtonStyle.green,
            custom_id="helpfulness:yes",
        )
        no_button = Button(
            emoji="✖️",
            style=ButtonStyle.red,
            custom_id="helpfulness:no",
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

        self._registry[msg.id] = HelpfulnessPromptState(
            author=self.author,
            message=msg,
            experiment_name=self.experiment_name,
            title=self.title,
            description=self.description,
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

        components_to_send = renderers.create_success_container(
            "Thanks!",
            "Your feedback has been submitted.",
            localization,
        )

        custom_id = inter.data.custom_id
        if custom_id == "helpfulness:yes":
            await experiments.experiment_helped(state.experiment_name, state.author.id)
        elif custom_id == "helpfulness:no":
            await experiments.experiment_did_not_help(
                state.experiment_name, state.author.id
            )
        else:
            return

        await inter.followup.edit_message(
            inter.message.id,
            components=cls(
                state.author,
                state.experiment_name,
                state.title,
                state.description,
                localization,
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

            try:
                await state.message.delete()
            except Exception:
                pass

            cls._registry.pop(message_id, None)
        except asyncio.CancelledError:
            return
