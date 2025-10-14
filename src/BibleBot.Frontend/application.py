"""
Copyright (C) 2016-2025 Kerygma Digital Co.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this file,
You can obtain one at https://mozilla.org/MPL/2.0/.
"""

import disnake
from disnake.ext import commands
from logger import VyLogger
import os
import asyncio

if os.name == "nt":
    asyncio.set_event_loop_policy(asyncio.WindowsSelectorEventLoopPolicy())

logger = VyLogger("default")

intents = disnake.Intents.default()
intents.message_content = True

command_sync_flags = commands.CommandSyncFlags.default()
command_sync_flags.sync_commands_debug = True

bot = commands.AutoShardedInteractionBot(
    intents=intents,
    command_sync_flags=command_sync_flags,
    default_install_types=disnake.ApplicationInstallTypes.all(),
)

bot.load_extension("cogs")
bot.i18n.load("locale/")

bot.run(os.environ.get("DISCORD_TOKEN"))
