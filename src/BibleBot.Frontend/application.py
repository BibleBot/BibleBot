"""
Copyright (C) 2016-2026 Kerygma Digital Co.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this file,
You can obtain one at https://mozilla.org/MPL/2.0/.
"""

import os

import disnake
import sentry_sdk
from aiohttp import web
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
    environment=os.environ.get("SENTRY_ENVIRONMENT"),
)

logger = VyLogger("default")

intents = disnake.Intents.default()
# noinspection PyDunderSlots, PyUnresolvedReferences
intents.message_content = True

command_sync_flags = commands.CommandSyncFlags.default()
# noinspection PyDunderSlots, PyUnresolvedReferences
command_sync_flags.sync_commands_debug = False

bot = commands.AutoShardedInteractionBot(
    intents=intents,
    command_sync_flags=command_sync_flags,
    default_install_types=disnake.ApplicationInstallTypes.all(),
    default_contexts=disnake.InteractionContextTypes.all(),
)


async def health_check(request):
    return web.Response(
        text="<html><body><h1>BibleBot Frontend is Healthy</h1></body></html>",
        content_type="text/html",
    )


async def start_health_server():
    app = web.Application()
    app.router.add_get("/health", health_check)
    app.router.add_get("/", health_check)
    runner = web.AppRunner(app)
    await runner.setup()
    site = web.TCPSite(runner, "0.0.0.0", 5054)
    await site.start()
    logger.info("Health check server started on port 5054")


health_server_started = False


@bot.listen()
async def on_ready():
    global health_server_started
    if not health_server_started:
        await start_health_server()
        health_server_started = True


bot.load_extension("cogs")
bot.i18n.load("locale/")
bot.run(os.environ.get("DISCORD_TOKEN"))
