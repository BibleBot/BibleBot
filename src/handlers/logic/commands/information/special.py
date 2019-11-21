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

from handlers.logic.commands import utils

custom_messages = {
    "joseph": "Jesus never consecrated peanut butter and jelly sandwiches and Coca-Cola!",
    "tiger": "Our favorite Tiger lives by Ephesians 4:29,31-32, Matthew 16:26, James 4:6, and lastly, his calling from God, Matthew 28:19-20. He tells everyone that because of grace in faith (Ephesians 2:8-10) he was saved, and not of works. Christ Jesus has made him a new creation (2 Corinthians 5:17). He asks you make God preeminent not prominent.",  # noqa
    "rose": "I am a rose of Sharon, a lily of the valleys.  Because, if you confess with your mouth that Jesus is Lord and believe in your heart that God raised him from the dead, you will be saved.  For everyone who calls on the name of the Lord will be saved.  The desert and the parched land will be glad..."
}

cm_commands = list(custom_messages.keys())


def get_custom_message(name):
    if name not in cm_commands:
        raise IndexError(f"Not a valid custom message. Valid custom messages: {str(cm_commands)}")

    return {
        "level": "info",
        "text": True,
        "message": custom_messages[name]
    }


supporters = ["<@169500883825065985>", "Jepekula", "Joseph", "<@199366508139249665>", "<@384202884553768961>",
              "<@288825133731414017>"]


def get_supporters(lang):
    title = lang["commands"]["supporters"]
    description = "**" + lang["supporters"] + "**\n\n"

    for supporter in supporters:
        description += f"- {supporter}\n"

    description += "- " + lang["anonymousDonors"] + "\n\n" + lang["donorsNotListed"]

    embed = utils.create_embed(title, description)

    return {
        "level": "info",
        "message": embed
    }
