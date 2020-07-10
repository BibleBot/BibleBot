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
# pylint: disable=super-init-not-called
import os
import io
from abc import abstractmethod

import aiofiles
from tinydb.storages import Storage, JSONStorage, json
from .exceptions import NotOverridableError, ReadonlyStorageError


class AIOStorage(Storage):
    """
    Abstract asyncio Storage class
    """
    @abstractmethod
    async def __aenter__(self):
        """
        Initialize storage in async manner (open files, connections, etc...)
        """
        raise NotImplementedError('To be overridden!')

    @abstractmethod
    async def __aexit__(self, exc_type, exc, traceback):
        """
        Finalize storage in async manner (close files, connections, etc...)
        """
        raise NotImplementedError('To be overridden!')

    def close(self):
        """
        This is not called and should NOT be used
        """
        raise NotOverridableError('NOT to be overridden or called, use __aexit__!')


class AIOJSONStorage(AIOStorage, JSONStorage):
    """
    Asyncronous JSON Storage for AIOTinyDB
    """
    def __init__(self, filename, *args, **kwargs):
        self.args = args
        self.kwargs = kwargs
        self._filename = filename
        self._lock = None
        self._handle = None
        self._aio_handle = None

    async def __aenter__(self):
        if self._handle is None:
            try:
                async with aiofiles.open(self._filename, 'r+') as in_file:
                    payload = await in_file.read()
            except FileNotFoundError:
                dirname = os.path.dirname(self._filename)
                if dirname:
                    os.makedirs(dirname, exist_ok=True)

                async with aiofiles.open(self._filename, 'w+') as in_file:
                    payload = await in_file.read()

            self._handle = io.StringIO(payload)
        return self

    def write(self, data):
        self._handle.seek(0)
        serialized = json.dumps(data, **self.kwargs)
        self._handle.write(serialized)
        self._handle.flush()
        self._handle.truncate()

    async def __aexit__(self, exc_type, exc, traceback):
        if self._handle is not None:
            async with aiofiles.open(self._filename, 'w') as out_file:
                await out_file.write(self._handle.getvalue())

            self._handle.close()


class AIOImmutableJSONStorage(AIOJSONStorage):
    """
    Asyncronous readonly JSON Storage for AIOTinyDB
    """
    async def __aenter__(self):
        if self._handle is None:
            async with aiofiles.open(self._filename) as in_file:
                payload = await in_file.read()
            self._handle = io.StringIO(payload)
        return self

    async def __aexit__(self, exc_type, exc, traceback):
        if self._handle is not None:
            self._handle.close()

    def write(self, data):
        raise ReadonlyStorageError('AIOImmutableJSONStorage cannot be written to')
