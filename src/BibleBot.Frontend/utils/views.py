"""
Copyright (C) 2016-2025 Kerygma Digital Co.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this file,
You can obtain one at https://mozilla.org/MPL/2.0/.
"""

from disnake import ui, ButtonStyle
from logger import VyLogger
from .i18n import i18n as i18n_class
from . import backend, components, containers, statics
import disnake
import asyncio

i18n = i18n_class()
logger = VyLogger("default")


class DisplayStyleView(ui.View):
    def __init__(
        self,
        author_id: int,
        localization,
        is_server: bool = False,
        timeout: float = 180.0,
    ):
        if not timeout:
            super().__init__()
        else:
            super().__init__(timeout=timeout)

        self.add_item(components.DisplayStyleSelect(author_id, is_server, localization))


class BracketsView(ui.View):
    def __init__(
        self,
        author_id: int,
        localization,
        timeout: float = 180.0,
    ):
        if not timeout:
            super().__init__()
        else:
            super().__init__(timeout=timeout)

        self.add_item(components.BracketsSelect(author_id, localization))


# TODO: rewrite this into components
class ConfirmationPromptView(ui.View):
    def __init__(
        self,
        on_confirm_command: str,
        author: disnake.User | disnake.Member,
        localization,
        timeout: float = 0.0,
    ):
        if not timeout:
            super().__init__()
        else:
            super().__init__(timeout=timeout)

        self.author = author
        self.on_confirm_command = on_confirm_command

        container = ui.Container()
        container.accent_colour = 16776960

        container.children.append(
            ui.TextDisplay(f"### {localization["CONFIRMATION_REQUIRED_TITLE"]}")
        )

        container.children.append(
            ui.TextDisplay(
                f"{localization[
                    "CONFIRMATION_REQUIRED_SETDAILYVERSEROLE_EVERYONE"
                ]}"
            )
        )

        container.children.append(
            ui.Separator(divider=True, spacing=disnake.SeparatorSpacing.large)
        )

        container.children.append(
            ui.TextDisplay(
                f"-# {statics.logo_emoji}  **{localization["EMBED_FOOTER"].replace("<v>", statics.version)}**"
            )
        )

        self.container = container

    @ui.button(emoji="✅", style=ButtonStyle.green)
    async def yes(self, button, inter):
        localization = i18n.get_i18n_or_default(inter.locale.name)
        try:
            if inter.author.id != self.author.id:
                return await inter.send(
                    localization["PAGINATOR_FORBIDDEN"], ephemeral=True
                )

            resp = await backend.submit_command(
                inter.channel, self.author, self.on_confirm_command
            )
            await inter.response.edit_message(components=resp, view=None)
        except:
            pass

    @ui.button(emoji="✖️", style=ButtonStyle.red)
    async def no(self, button, inter):
        localization = i18n.get_i18n_or_default(inter.locale.name)
        try:
            if inter.author.id != self.author.id:
                return await inter.send(
                    localization["PAGINATOR_FORBIDDEN"], ephemeral=True
                )

            resp = containers.create_error_container(
                localization["CONFIRMATION_REJECTED_TITLE"],
                localization["CONFIRMATION_REJECTED_DESC"],
                localization,
            )
            await inter.response.edit_message(components=resp, view=None)
        except:
            pass
