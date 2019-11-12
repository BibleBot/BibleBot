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

import discord
import central

import re

from handlers.logic.commands import utils


def create_biblebot_embeds(lang):
    pages = [discord.Embed(), discord.Embed()]

    command_list = utils.divide_list(lang["commandlist"].split("* ")[1:], 6)

    for page in pages:
        page.title = lang["biblebot"].replace(
            "<version>", f"{central.version}")
        page.description = lang["code"].replace(
            "repositoryLink", "https://github.com/BibleBot/BibleBot")

        page.color = 303102
        page.set_footer(text=page.title, icon_url=central.icon)

    responses = command_list + [lang["commandlist2"], lang["guildcommandlist"]]

    for response in responses:
        if response == responses[-1] or response == responses[-2]:
            for placeholder in re.findall(r"<[a-zA-Z0-9]*>", response):
                placeholder = placeholder[1:-1]

                if placeholder == "biblebotversion":
                    response = response.replace(
                        f"<{placeholder}>", central.version)
                elif placeholder in ["enable", "disable"]:
                    response = response.replace(
                        f"<{placeholder}>", lang["arguments"][placeholder])
                else:
                    response = response.replace(
                        f"<{placeholder}>", lang["commands"][placeholder])
        else:
            for chunk in response:
                for command in chunk:
                    for placeholder in re.findall(r"<[a-zA-Z0-9]*>", command):
                        placeholder = placeholder[1:-1]

                        if placeholder == "biblebotversion":
                            command = command.replace(
                                f"<{placeholder}>", central.version)
                        elif placeholder in ["enable", "disable"]:
                            command = command.replace(
                                f"<{placeholder}>", lang["arguments"][placeholder])
                        else:
                            command = command.replace(
                                f"<{placeholder}>", lang["commands"][placeholder])

    command_list_count = len(command_list)
    pages[0].add_field(name=lang["commandlistName"],
                       value="".join(responses[0]), inline=False)

    for i in range(1, command_list_count):
        if i != command_list_count - 1:
            pages[0].add_field(name=u"\u200B",
                               value="".join(responses[i]), inline=False)
        else:
            pages[0].add_field(name=u"\u200B",
                               value="".join(responses[i]) + "\n" + u"\u200B",
                               inline=False)

    # pages[0].add_field(name=u"\u200B", value=u"\u200B", inline=False)

    pages[1].add_field(name=lang["extrabiblicalcommandlistName"],
                       value=responses[-2].replace("* ", ""), inline=False)
    pages[1].add_field(name=u"\u200B", value=u"\u200B", inline=False)

    guild_commands = responses[-1].replace("* ", "") + "\n" + u"\u200B"
    pages[1].add_field(name=lang["guildcommandlistName"], value=guild_commands,
                       inline=False)
    # pages[1].add_field(name=u"\u200B", value=u"\u200B", inline=False)

    website = lang["website"].replace("websiteLink", "https://biblebot.xyz")
    server_invite = lang["joinserver"].replace("inviteLink",
                                               "https://discord.gg/H7ZyHqE")
    terms = lang["terms"].replace("termsLink", "https://biblebot.xyz/terms")
    usage = lang["usage"]

    links = f"{website}\n{server_invite}\n{terms}\n\n**{usage}**"
    for page in pages:
        page.add_field(name=lang["links"], value=links)

    return pages
