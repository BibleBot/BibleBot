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

import os
import sys

from handlers.logic.commands import bridge as command_bridge
from handlers.logic.extrabiblical import bridge as catechisms_bridge

dir_path = os.path.dirname(os.path.realpath(__file__))
sys.path.append(dir_path + "/..")

import central  # noqa: E402


def is_command(command, lang):
    commands = lang["commands"]
    untranslated_commands = ["biblebot", "setlanguage", "userid",
                             "ban", "unban", "reason",
                             "optout", "unoptout", "eval",
                             "jepekula", "joseph", "tiger", "lsc"]

    result = {
        "ok": False
    }

    if command in (untranslated_commands + list(commands.values())):
        result = {
            "ok": True,
            "orig": command
        }

    return result


def is_owner_command(command, lang):
    commands = lang["commands"]
    owner_commands = [commands["leave"], commands["puppet"], commands["announce"],
                      commands["addversion"], commands["rmversion"],
                      "userid", "ban", "unban", "reason",
                      "optout", "unoptout", "eval"]

    return command in owner_commands


def is_catechism_command(command, lang):
    commands = lang["commands"]
    catechism_commands = ["lsc"]

    return command in catechism_commands


class CommandHandler:
    @classmethod
    async def process_command(cls, ctx, command, remainder=None):
        proper_command = is_command(command, ctx["language"])

        if proper_command["ok"]:
            orig_cmd = proper_command["orig"]

            if not is_owner_command(orig_cmd, ctx["language"]):
                if not is_catechism_command(orig_cmd, ctx["language"]):
                    return await command_bridge.run_command(ctx, orig_cmd, remainder)
                else:
                    return await catechisms_bridge.run_command(ctx, orig_cmd, remainder)
            else:
                if str(ctx["author"].id) == central.config["BibleBot"]["owner"]:
                    return await command_bridge.run_owner_command(ctx, command, remainder)
