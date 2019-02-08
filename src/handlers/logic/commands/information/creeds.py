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
