"""
    Copyright (C) 2016-2022 Kerygma Digital Co.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this file,
    You can obtain one at https://mozilla.org/MPL/2.0/.
"""

import disnake
from disnake.ext import commands
from logger import VyLogger
import os

logger = VyLogger("default")

intents = disnake.Intents.default()
intents.message_content = True
bot = commands.AutoShardedBot(
    command_prefix=commands.when_mentioned,
    intents=intents,
)

bot.load_extension("cogs")

bot.run(os.environ.get("DISCORD_TOKEN"))
