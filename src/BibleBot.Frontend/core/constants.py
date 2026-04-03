import os
import disnake
from logger import VyLogger

logger = VyLogger("default")

version = "undefined"
verse_footer = "BibleBot <v> by Kerygma Digital"

logo_emoji = "<:biblebot:1438705100515184762>"

config = "Release"

if os.environ.get("ASPNETCORE_ENVIRONMENT") == "Development":
    config = "Debug"
    logo_emoji = "<:biblebot:1438598262234808381>"

publisher_to_url = {
    "biblica": {"name": "Biblica", "url": "https://biblica.com"},
    "lockman": {"name": "The Lockman Foundation", "url": "https://www.lockman.org"},
}

try:
    __assembly_info_file__ = open(
        f"../../src/BibleBot.Backend/obj/{config}/net10.0/GitInfo.cache",
        "r",
    ).readlines()

    for line in __assembly_info_file__:
        if "GitBaseVersion=" in line:
            split = line.split("=")

            if split:
                version = "v" + split[1][0:-2]
                verse_footer = verse_footer.replace("<v>", version)
except Exception as err:
    logger.error(f"couldn't fetch and set latest version: {err}")
    pass

async def check_version_changes(bot: disnake.AutoShardedClient):
    global version
    global verse_footer

    try:
        __assembly_info_file__ = open(
            f"../../src/BibleBot.Backend/obj/{config}/net10.0/GitInfo.cache",
            "r",
        ).readlines()

        for line in __assembly_info_file__:
            if "GitBaseVersion=" in line:
                split = line.split("=")

                if split:
                    version = "v" + split[1][0:-2]
                    verse_footer = verse_footer.replace("<v>", version)

                    if bot._connection.shard_ids is not None:
                        for shard_id in bot._connection.shard_ids:
                            await bot.change_presence(
                                status=disnake.Status.online,
                                activity=disnake.Game(
                                    f"/biblebot {version} - shard {shard_id + 1}"
                                ),
                                shard_id=shard_id,
                            )

                        logger.info("updated shard versions")
    except Exception as err:
        logger.error(f"couldn't fetch and set latest version: {err}")
        pass
