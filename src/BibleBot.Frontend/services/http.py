"""
Copyright (C) 2016-2026 Kerygma Digital Co.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this file,
You can obtain one at https://mozilla.org/MPL/2.0/.
"""

import asyncio
import os

import aiohttp
from logger import VyLogger

logger = VyLogger("default")

# Every outbound request used to build its own ClientSession, which meant a new
# connector, a new TLS handshake and an uncached DNS lookup per verse - and, with
# aiohttp's five-minute default, a hung backend held its socket for five minutes.
# Enough of those at once starves the event loop and delays disnake's own HTTP,
# including the defer() that has to land inside Discord's three-second window.
DEFAULT_TIMEOUT = aiohttp.ClientTimeout(
    total=float(os.environ.get("HTTP_TIMEOUT_TOTAL", 15)),
    connect=float(os.environ.get("HTTP_TIMEOUT_CONNECT", 5)),
    sock_read=float(os.environ.get("HTTP_TIMEOUT_SOCK_READ", 15)),
)

_session: aiohttp.ClientSession | None = None
_lock = asyncio.Lock()


async def get_session() -> aiohttp.ClientSession:
    """Returns the process-wide aiohttp session, creating it on first use.

    Must be awaited from inside the running loop - aiohttp binds the connector to
    whichever loop is running when the session is constructed.
    """

    global _session

    if _session is not None and not _session.closed:
        return _session

    async with _lock:
        # Another coroutine may have built it while we waited for the lock.
        if _session is not None and not _session.closed:
            return _session

        connector = aiohttp.TCPConnector(
            limit=int(os.environ.get("HTTP_CONN_LIMIT", 100)),
            limit_per_host=int(os.environ.get("HTTP_CONN_LIMIT_PER_HOST", 50)),
            ttl_dns_cache=int(os.environ.get("HTTP_DNS_CACHE_TTL", 300)),
            enable_cleanup_closed=True,
        )

        _session = aiohttp.ClientSession(connector=connector, timeout=DEFAULT_TIMEOUT)
        logger.info(
            f"http session created (timeout={DEFAULT_TIMEOUT.total}s, "
            f"limit={connector.limit}, limit_per_host={connector.limit_per_host})"
        )

    return _session


async def close_session():
    """Closes the process-wide session, if one was ever opened."""

    global _session

    if _session is not None and not _session.closed:
        await _session.close()
        logger.info("http session closed")

    _session = None
