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

# pylint: disable=super-init-not-called,arguments-differ
# pylint: disable=too-many-instance-attributes
from tinydb import TinyDB
from tinydb.database import Table, StorageProxy
from .exceptions import NotOverridableError, DatabaseNotReady
from .storage import AIOJSONStorage


class AIOTinyDB(TinyDB):
    """
    TinyDB for asyncio

    # Example
    ```
    import asyncio
    from aiotinydb import AIOTinyDB

    async def test():
        async with AIOTinyDB('test.json') as db:
            db.insert(dict(counter=1))

    loop = asyncio.new_event_loop()
    loop.run_until_complete(test())
    loop.close()
    ```
    """
    DEFAULT_STORAGE = AIOJSONStorage
    def __init__(self, *args, **kwargs):
        self._storage_cls = kwargs.pop('storage', self.DEFAULT_STORAGE)
        self._table_name = kwargs.pop('default_table', self.DEFAULT_TABLE)
        self._args = args
        self._kwargs = kwargs
        self._table_cache = {}
        self._storage = None
        self._table = None
        self._cls_table = kwargs.pop('table_class', self.table_class)
        self._cls_storage_proxy = kwargs.pop('storage_proxy_class',
                                             self.storage_proxy_class)

    def purge_table(self, name):
        if self._storage is None:
            raise DatabaseNotReady('File is not opened. Use with AIOTinyDB(...):')
        return super().purge_table(name)

    def purge_tables(self):
        if self._storage is None:
            raise DatabaseNotReady('File is not opened. Use with AIOTinyDB(...):')
        return super().purge_tables()

    def table(self, name=None, **options):
        if name is None:
            name = self.DEFAULT_TABLE
        if self._storage is None:
            raise DatabaseNotReady('File is not opened. Use with AIOTinyDB(...):')
        return super().table(name, **options)

    def tables(self):
        if self._storage is None:
            raise DatabaseNotReady('File is not opened. Use with AIOTinyDB(...):')
        return super().tables()

    def __getattr__(self, val):
        if self._storage is None:
            raise AttributeError('File is not opened. Use with AIOTinyDB(...):')
        return super().__getattr__(val)

    async def __aenter__(self):
        if self._storage is None:
            self._storage = self._storage_cls(*self._args, **self._kwargs)
            await self._storage.__aenter__()
            self._table = self.table(self._table_name)
        return self

    async def __aexit__(self, exc_type, exc, traceback):
        if self._storage:
            await self._storage.__aexit__(exc_type, exc, traceback)
            self._storage = None
            self._table_cache = {}

    def close(self):
        raise NotOverridableError('Usual methods will not work on async')

    def __enter__(self):
        raise NotOverridableError('Usual methods will not work on async')

    def __exit__(self, exc_type, exc, tb):
        raise NotOverridableError('Usual methods will not work on async')


# Set the default table class
AIOTinyDB.table_class = Table

# Set the default storage proxy class
AIOTinyDB.storage_proxy_class = StorageProxy
