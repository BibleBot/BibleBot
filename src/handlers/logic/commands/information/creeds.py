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

from handlers.logic.commands import utils
import central

creeds = ["apostles", "nicene325", "nicene", "chalcedon"]


def get_creeds(lang):
    title = lang["creeds"]
    description = lang["creeds_text"]

    for creed in creeds:
        creed_title = lang[f"{creed}_name"]
        command_name = lang["commands"][creed]

        description += f"`{central.cmd_prefix}{command_name}` - **{creed_title}**\n"

    embed = utils.create_embed(title, description, custom_title=True)

    return {
        "level": "info",
        "message": embed
    }


def get_creed(name, lang):
    if name not in creeds:
        raise IndexError(f"Not a valid creed. Valid creeds: {str(creeds)}")

    title = lang[f"{name}_name"]
    description = lang[f"{name}_text"]

    embed = utils.create_embed(title, description, custom_title=True)

    return {
        "level": "info",
        "message": embed
    }
