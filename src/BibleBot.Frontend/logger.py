"""
    Copyright (C) 2016-2023 Kerygma Digital Co.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this file,
    You can obtain one at https://mozilla.org/MPL/2.0/.
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
    "DEFAULT": Fore.RESET,
}

Levels = {
    "WARNING": Colors["YELLOW"],
    "INFO": Colors["CYAN"],
    "DEBUG": Colors["GREY"],
    "ERROR": Colors["RED"],
}

Format = {"WARNING": "warn", "INFO": "info", "DEBUG": "dbug", "ERROR": "erro"}


class VyFormatter(logging.Formatter):
    def __init__(self, msg, use_color=True):
        logging.Formatter.__init__(self, msg)
        self.use_color = use_color

    def format(self, record):
        levelname = record.levelname

        if self.use_color:
            levelname_color = None

            if levelname in Levels:
                levelname_color = (
                    Levels[levelname]
                    + "["
                    + Format[levelname]
                    + "] "
                    + Colors["DEFAULT"]
                )

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
