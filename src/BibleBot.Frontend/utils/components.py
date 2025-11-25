"""
Copyright (C) 2016-2025 Kerygma Digital Co.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this file,
You can obtain one at https://mozilla.org/MPL/2.0/.
"""

from disnake import SelectOption, MessageInteraction
from disnake.ui import StringSelect
from utils.i18n import i18n as i18n_class
from utils import backend

i18n = i18n_class()


class DisplayStyleSelect(StringSelect):
    def __init__(self, author_id: int, is_server: bool, loc) -> None:
        self.author_id = author_id
        self.custom_id = "formatting " + (
            "setserverdisplay" if is_server else "setdisplay"
        )
        self.is_ephemeral = False

        options = [
            SelectOption(
                label=loc["EMBED_BLOCKS_LABEL"],
                value="embed",
                description=loc["EMBED_BLOCKS_DESC"],
            ),
            SelectOption(
                label=loc["CODE_BLOCKS_LABEL"],
                value="code",
                description=loc["CODE_BLOCKS_DESC"],
            ),
            SelectOption(
                label=loc["BLOCKQUOTES_LABEL"],
                value="blockquote",
                description=loc["BLOCKQUOTES_DESC"],
            ),
        ]
        super().__init__(
            custom_id=self.custom_id,
            placeholder=loc["SELECT_DISPLAY_STYLE"],
            options=options,
        )

    async def callback(self, inter: MessageInteraction) -> None:
        if inter.author.id != self.author_id:
            return

        value = inter.values[0]

        resp = await backend.submit_command(
            inter.channel, inter.author, f"+{self.custom_id} {value}"
        )

        await inter.response.edit_message(embed=None, components=resp, content=None)  # type: ignore

    async def on_error(self, error: Exception, inter: MessageInteraction) -> None:
        localization = i18n.get_i18n_or_default(inter.locale.name)

        await inter.response.edit_message(
            content=localization["DISPLAY_STYLE_FAILURE"],
            components=None,
        )


class BracketsSelect(StringSelect):
    def __init__(self, author_id: int, loc: dict[str, str]) -> None:
        self.author_id = author_id
        self.custom_id = "formatting setbrackets"
        options = [
            SelectOption(
                label=loc["ANGLE_BRACKETS_LABEL"],
                value="<>",
                description=loc["ANGLE_BRACKETS_DESC"],
            ),
            SelectOption(
                label=loc["SQUARE_BRACKETS_LABEL"],
                value="[]",
                description=loc["SQUARE_BRACKETS_DESC"],
            ),
            SelectOption(
                label=loc["CURLY_BRACKETS_LABEL"],
                value="{}",
                description=loc["CURLY_BRACKETS_DESC"],
            ),
            SelectOption(
                label=loc["PARENTHESIS_LABEL"],
                value="()",
                description=loc["PARENTHESIS_DESC"],
            ),
        ]
        super().__init__(
            custom_id=self.custom_id,
            placeholder=loc["SELECT_BRACKETS"],
            options=options,
        )

    async def callback(self, inter: MessageInteraction) -> None:
        if inter.author.id != self.author_id:
            return

        value = inter.values[0]

        resp = await backend.submit_command(
            inter.channel, inter.author, f"+{self.custom_id} {value}"
        )

        await inter.response.edit_message(embed=None, components=resp, content=None)  # type: ignore

    async def on_error(self, error: Exception, inter: MessageInteraction) -> None:
        localization = i18n.get_i18n_or_default(inter.locale.name)

        await inter.response.edit_message(
            content=localization["BRACKETS_FAILURE"],
            components=None,
        )
