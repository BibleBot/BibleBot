"""
    Copyright (c) 2018-2020 Elliott Pardee <me [at] thevypr [dot] com>
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

import logging

from colorama import Fore

Colors = {
    "GREEN": Fore.LIGHTGREEN_EX,
    "YELLOW": Fore.LIGHTYELLOW_EX,
    "BLUE": Fore.LIGHTBLUE_EX,
    "CYAN": Fore.CYAN,
    "RED": Fore.LIGHTRED_EX,
    "GREY": Fore.LIGHTBLACK_EX,
    "DEFAULT": Fore.RESET
}

Levels = {
    "WARNING": Colors["YELLOW"],
    "INFO": Colors["CYAN"],
    "DEBUG": Colors["GREY"],
    "ERROR": Colors["RED"]
}

Format = {
    "WARNING": "warn",
    "INFO": "info",
    "DEBUG": "dbug",
    "ERROR": "erro"
}


class VyFormatter(logging.Formatter):
    def __init__(self, msg, use_color=True):
        logging.Formatter.__init__(self, msg)
        self.use_color = use_color

    def format(self, record):
        levelname = record.levelname

        if self.use_color:
            levelname_color = None

            if levelname in Levels:
                levelname_color = Levels[levelname] + "[" + \
                    Format[levelname] + "] " + Colors["DEFAULT"]

                record.levelname = levelname_color

        return logging.Formatter.format(self, record)


class VyLogger(logging.Logger):
    FORMAT = "%(levelname)s %(message)s"

    def __init__(self, name):
        logging.Logger.__init__(self, name, logging.DEBUG)

        color_formatter = VyFormatter(self.FORMAT)
        console = logging.StreamHandler()

        console.setFormatter(color_formatter)
        self.addHandler(console)
        return


logging.setLoggerClass(VyLogger)
