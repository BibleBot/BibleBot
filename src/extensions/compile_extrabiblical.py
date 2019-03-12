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

dir_path = os.path.dirname(os.path.realpath(__file__))
extrabiblical_path = f"{dir_path}/../data/extrabiblical/"


def main():
    done = False

    while not done:
        file_location = input("Your current directory is src/data/extrabiblical/, what is the file you're compiling? ")

        if not file_location.endswith(".raw.json"):
            print("Your file must end with .raw.json.")
            exit(1)

        temp_file = open(f"{extrabiblical_path}{file_location}").read()
        output = f"{extrabiblical_path}" + file_location.replace(f".raw.json", ".bin")

        with open(f"{output}", "wb") as fl:
            temp_out = zlib.compress(bytearray(temp_file, 'utf-8'))
            fl.write(temp_out)

        print("If there are no errors, consider it done.")

        done_input = input("More? [y/n] ")

        if done_input.lower() == "y":
            done = True


if __name__ == '__main__':
    main()
