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

from . import utils

catechisms = ["lsc", "heidelberg", "ccc"]


async def run_command(ctx, command, remainder):
    lang = ctx["language"]
    args = remainder.split(" ")

    if command in catechisms:
        if len(args) == 2:
            return utils.create_embeds(lang, command, section=args[0], page=args[1], guild=ctx["guild"])
        elif len(args) == 1:
            if args[0] == "":
                return utils.create_embeds(lang, command, guild=ctx["guild"])

            return utils.create_embeds(lang, command, section=args[0], guild=ctx["guild"])

