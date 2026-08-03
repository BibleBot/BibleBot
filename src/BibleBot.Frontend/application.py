"""
Copyright (C) 2016-2026 Kerygma Digital Co.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this file,
You can obtain one at https://mozilla.org/MPL/2.0/.
"""

import logging
import os

import aiohttp
import disnake
import sentry_sdk
from aiohttp import web
from disnake.ext import commands
from logger import VyLogger
from services import http
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

# disnake's own loggers were never attached to a handler, so nothing they emit
# has ever reached the journal - including "heartbeat blocked for more than N
# seconds", which is the definitive signal that the event loop is stalling, and
# the websocket close codes behind shard disconnects. WARNING keeps the volume
# low; set DISNAKE_LOG_LEVEL=INFO to see gateway chatter while debugging.
_disnake_logger = logging.getLogger("disnake")
_disnake_logger.setLevel(
    getattr(
        logging,
        os.environ.get("DISNAKE_LOG_LEVEL", "WARNING").upper(),
        logging.WARNING,
    )
)
if not _disnake_logger.handlers:
    _disnake_handler = logging.StreamHandler()
    _disnake_handler.setFormatter(
        logging.Formatter("[%(levelname)s] %(name)s: %(message)s")
    )
    _disnake_logger.addHandler(_disnake_handler)

intents = disnake.Intents.default()
# noinspection PyDunderSlots, PyUnresolvedReferences
intents.message_content = True

command_sync_flags = commands.CommandSyncFlags.default()
# noinspection PyDunderSlots, PyUnresolvedReferences
command_sync_flags.sync_commands_debug = False

class BibleBot(commands.AutoShardedInteractionBot):
    async def login(self, token: str) -> None:
        # disnake builds ONE aiohttp session for both REST and the gateway
        # websockets (HTTPClient.ws_connect uses the same session), from this
        # connector. Left as None it defaults to TCPConnector(limit=100) - and
        # every shard's websocket permanently occupies one of those 100 slots.
        # At 99 shards that leaves a single slot for all REST traffic, so
        # interaction defer()s queue behind whatever else is in flight and blow
        # Discord's three-second window. Must be built here: login() runs inside
        # the loop, and aiohttp refuses to construct a connector without one.
        if self.http.connector is None:
            self.http.connector = aiohttp.TCPConnector(
                limit=int(os.environ.get("DISCORD_CONN_LIMIT", 0)),
                ttl_dns_cache=300,
                enable_cleanup_closed=True,
            )
            logger.info(
                f"discord connector limit set to "
                f"{self.http.connector.limit or 'unlimited'}"
            )

        await super().login(token)

    async def close(self):
        await http.close_session()
        await super().close()


bot = BibleBot(
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
