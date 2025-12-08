"""
Copyright (C) 2016-2025 Kerygma Digital Co.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this file,
You can obtain one at https://mozilla.org/MPL/2.0/.
"""

from core.i18n import bb_i18n
from disnake import ui
from logger import VyLogger
from ui import components

i18n = bb_i18n()
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
