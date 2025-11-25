"""
Copyright (C) 2016-2025 Kerygma Digital Co.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this file,
You can obtain one at https://mozilla.org/MPL/2.0/.
"""

from utils import backend, sending, statics, checks, containers
from utils.i18n import i18n as i18n_class
from disnake.ext import commands
from logger import VyLogger

i18n = i18n_class()
logger = VyLogger("default")


class Experiments(commands.Cog):
    def __init__(self, bot):
        self.bot = bot

    async def cog_load(self):
        logger.info("loaded experiments cog")

    def cog_unload(self):
        logger.info("unloaded experiments cog")
