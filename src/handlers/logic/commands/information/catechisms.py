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

catechisms = ["lsc", "heidelberg", "ccc"]

catechism_titles = {
    "lsc": "Luther's Small Catechism (1529)",
    "heidelberg": "Heidelberg Catechism (1563)",
    "ccc": "Catechism of the Catholic Church (1992)"
}


def get_catechisms(lang):
    title = lang["catechisms"]
    description = lang["catechisms_text"]

    for catechism in catechisms:
        command_name = catechism
        description += f"`{central.cmd_prefix}{command_name}` - **{catechism_titles[catechism]}**\n"

    embed = utils.create_embed(title, description, custom_title=True)

    return {
        "level": "info",
        "message": embed
    }
