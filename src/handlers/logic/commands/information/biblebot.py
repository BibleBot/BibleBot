import discord
import central



def create_biblebot_embeds(lang):
    pages = [discord.Embed(), discord.Embed()]

    command_list = divide_list(lang["commandlist"].split("* ")[1:], 6)

    for page in pages:
        page.title = lang["biblebot"].replace("<biblebotversion>", central.version.split("v")[1])
        page.description = lang["code"].replace("repositoryLink", "https://github.com/BibleBot/BibleBot")

        page.color = 303102
        page.set_footer(text=central.version, icon_url=central.icon)

    responses = command_list + [lang["commandlist2"], lang["guildcommandlist"]]

    for response in responses:
        if response == responses[-1] or response == responses[-2]:
            for placeholder in re.findall(r"<[a-zA-Z0-9]*>", response):
                placeholder = placeholder[1:-1]

                if placeholder == "biblebotversion":
                    response = response.replace(f"<{placeholder}>", central.version)
                elif placeholder in ["enable", "disable"]:
                    response = response.replace(f"<{placeholder}>", lang["arguments"][placeholder])
                else:
                    response = response.replace(f"<{placeholder}>", lang["commands"][placeholder])
        else:
            for chunk in response:
                for command in chunk:
                    for placeholder in re.findall(r"<[a-zA-Z0-9]*>", command):
                        placeholder = placeholder[1:-1]

                        if placeholder == "biblebotversion":
                            command = command.replace(f"<{placeholder}>", central.version)
                        elif placeholder in ["enable", "disable"]:
                            command = command.replace(f"<{placeholder}>", lang["arguments"][placeholder])
                        else:
                            command = command.replace(f"<{placeholder}>", lang["commands"][placeholder])

    command_list_count = len(command_list)
    pages[0].add_field(name=lang["commandlistName"], value="".join(responses[0]), inline=False)

    for i in range(1, command_list_count):
        pages[0].add_field(name=u"\u200B", value="".join(responses[i]), inline=False)

    pages[0].add_field(name=u"\u200B", value=u"\u200B", inline=False)

    pages[1].add_field(name=lang["extrabiblicalcommandlistName"], value=responses[-2].replace("* ", ""), inline=False)
    pages[1].add_field(name=u"\u200B", value=u"\u200B", inline=False)

    pages[1].add_field(name=lang["guildcommandlistName"], value=responses[-1].replace("* ", ""), inline=False)
    pages[1].add_field(name=u"\u200B", value=u"\u200B", inline=False)

    website = lang["website"].replace("websiteLink", "https://biblebot.xyz")
    server_invite = lang["joinserver"].replace("inviteLink", "https://discord.gg/H7ZyHqE")
    terms = lang["terms"].replace("termsLink", "https://biblebot.xyz/terms")
    usage = lang["usage"]

    links = f"{website}\n{server_invite}\n{terms}\n\n**{usage}**"
    for page in pages:
        page.add_field(name=lang["links"], value=links)

    return pages