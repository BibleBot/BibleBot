"""
Copyright (C) 2016-2025 Kerygma Digital Co.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this file,
You can obtain one at https://mozilla.org/MPL/2.0/.
"""

import os

import disnake
import sentry_sdk
from disnake.ext import commands
from logger import VyLogger
from sentry_sdk.integrations.argv import ArgvIntegration
from sentry_sdk.integrations.atexit import AtexitIntegration
from sentry_sdk.integrations.dedupe import DedupeIntegration
from sentry_sdk.integrations.excepthook import ExcepthookIntegration
from sentry_sdk.integrations.modules import ModulesIntegration
from sentry_sdk.integrations.stdlib import StdlibIntegration
from sentry_sdk.integrations.threading import ThreadingIntegration

sentry_sdk.init(
    dsn=os.environ.get("SENTRY_DSN", ""),
    default_integrations=False,
    integrations=[
        AtexitIntegration(),
        ArgvIntegration(),
        DedupeIntegration(),
        ExcepthookIntegration(),
        StdlibIntegration(),
        ModulesIntegration(),
        ThreadingIntegration(),
    ],
)

logger = VyLogger("default")

intents = disnake.Intents.default()
intents.message_content = True

command_sync_flags = commands.CommandSyncFlags.default()
command_sync_flags.sync_commands_debug = False

bot = commands.AutoShardedInteractionBot(
    intents=intents,
    command_sync_flags=command_sync_flags,
    default_install_types=disnake.ApplicationInstallTypes.all(),
    default_contexts=disnake.InteractionContextTypes.all(),
)

bot.load_extension("cogs")
bot.i18n.load("locale/")
bot.run(os.environ.get("DISCORD_TOKEN"))
