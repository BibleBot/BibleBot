"""
    Copyright (c) 2018-2019 Elliott Pardee <me [at] vypr [dot] xyz>
    This file is part of BibleBot.

    BibleBot is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    BibleBot is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with BibleBot.  If not, see <http://www.gnu.org/licenses/>.
"""

import zlib
import os
import central

dir_path = os.path.dirname(os.path.realpath(__file__))
extrabiblical_path = f"{dir_path}/../data/extrabiblical/"

paths = [
    "catechisms/luthers_small_catechism.raw.json",
    "catechisms/heidelberg_catechism.raw.json"
]


def compile_resources():
    for file_location in paths:
        temp_file = open(f"{extrabiblical_path}{file_location}").read()
        output = f"{extrabiblical_path}" + file_location.replace(".raw.json", ".bin")

        with open(f"{output}", "wb") as fl:
            temp_out = zlib.compress(bytearray(temp_file, 'utf-8'))
            fl.write(temp_out)

            name = file_location.replace(".raw.json", "")

            if "/" in name:
                name = name.split("/")[-1]

            central.log_message("info", 0, "compile_extrabiblical", "global", f"compiled '{name}'")


if __name__ == '__main__':
    compile_resources()
