"""
    aiotinydb - asyncio compatibility shim for tinydb

    Copyright 2017 Pavel Pletenev <cpp.create@gmail.com>
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
"""
# pylint: disable=too-few-public-methods, no-self-use
from tinydb.middlewares import Middleware
from tinydb.middlewares import CachingMiddleware as VanillaCachingMiddleware
from .exceptions import NotOverridableError


class AIOMiddleware(Middleware):
    """
        Asyncronous middleware base class
    """
    async def __aenter__(self):
        """
            Initialize middleware here
        """
        await self.storage.__aenter__()
        return self

    async def __aexit__(self, exc_type, exc, traceback):
        """
            Finalize middleware here
        """
        await self.storage.__aexit__(exc_type, exc, traceback)

    def close(self):
        """
        This is not called and should NOT be used
        """
        raise NotOverridableError('NOT to be overridden or called, use __aexit__!')


class AIOMiddlewareMixin(AIOMiddleware):
    """
        Mixin class to enable usage of non-async Middlewares
    """
    async def __aexit__(self, exc_type, exc, traceback):
        try:
            self.close()
        except NotOverridableError:
            pass
        await self.storage.__aexit__(exc_type, exc, traceback)


class CachingMiddleware(VanillaCachingMiddleware, AIOMiddlewareMixin):
    """
        Async-aware CachingMiddleware. For more info read
        docstring for `tinydb.middlewares.CachingMiddleware`
    """
    pass
