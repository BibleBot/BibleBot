import os
from sys import intern
import requests
import disnake
from logger import VyLogger

logger = VyLogger("default")


async def submit_command(
    rch: disnake.abc.Messageable, user: disnake.abc.User, body: str
):
    ch = await rch._get_channel()

    isDM = ch.type == disnake.ChannelType.private
    guildId = ch.id if isDM else user.id

    reqbody = {
        "UserId": str(user.id),
        "GuildId": str(guildId),
        "IsDM": isDM,
        "Body": body,
        "Token": os.environ.get("ENDPOINT_TOKEN"),
    }

    endpoint = os.environ.get("ENDPOINT")
    resp = requests.post(f"{endpoint}/commands/process", json=reqbody)

    print(reqbody)
    print(resp.json())

    if resp.json()["type"] == "cmd":
        if len(resp.json()["pages"]) == 1:
            return convert_embed(resp.json()["pages"][0])
        else:
            # todo
            pass
    elif resp.json()["type"] == "verse":
        if not resp.json()["paginate"] and resp.json()["displayStyle"] == "embed":
            if len(resp.json()["verses"]) == 1:
                return create_embed_from_verse(resp.json()["verses"][0])
            else:
                # todo
                pass


async def submit_command_raw(
    rch: disnake.abc.Messageable, user: disnake.abc.User, body: str
):
    ch = await rch._get_channel()

    isDM = ch.type == disnake.ChannelType.private
    guildId = ch.id if isDM else user.id

    reqbody = {
        "UserId": str(user.id),
        "GuildId": str(guildId),
        "IsDM": isDM,
        "Body": body,
        "Token": os.environ.get("ENDPOINT_TOKEN"),
    }

    endpoint = os.environ.get("ENDPOINT")
    resp = requests.post(f"{endpoint}/commands/process", json=reqbody)

    return resp.json()


def convert_embed(internal_embed):
    embed = disnake.Embed()

    embed.title = internal_embed["title"]
    embed.description = internal_embed["description"]
    embed.url = internal_embed["url"]
    embed.color = internal_embed["color"]

    if internal_embed["fields"] is not None:
        for field in internal_embed["fields"]:
            embed.add_field(
                name=field["name"], value=field["value"], inline=field["inline"]
            )

    embed.set_footer(
        text=internal_embed["footer"]["text"],
        icon_url=internal_embed["footer"]["icon_url"],
    )

    return embed


def create_embed_from_verse(verse):
    embed = disnake.Embed()

    reference_title = (
        verse["reference"]["asString"] + " - " + verse["reference"]["version"]["name"]
    )

    embed.set_author(name=reference_title)
    embed.title = verse["title"]
    embed.description = verse["text"]
    embed.color = 6709986

    embed.set_footer(
        text="BibleBot v9.2-beta by Kerygma Digital",
        icon_url="https://i.imgur.com/hr4RXpy.png",
    )

    return embed
