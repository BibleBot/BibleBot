import os

version = "undefined"
verse_footer = "BibleBot <v> by Kerygma Digital"

logo_emoji = "<:biblebot:1438705100515184762>"

if os.environ.get("ASPNETCORE_ENVIRONMENT") == "Development":
    logo_emoji = "<:biblebot:1438598262234808381>"

publisher_to_url = {
    "biblica": {"name": "Biblica", "url": "https://biblica.com"},
    "lockman": {"name": "The Lockman Foundation", "url": "https://www.lockman.org"},
}

try:
    config = (
        "Release"
        if os.environ.get("ASPNETCORE_ENVIRONMENT") == "Production"
        else "Debug"
    )

    __assembly_info_file__ = open(
        f"../../src/BibleBot.Backend/obj/{config}/net9.0/GitInfo.cache",
        "r",
    ).readlines()

    for line in __assembly_info_file__:
        if "GitBaseVersion=" in line:
            split = line.split("=")

            if split:
                version = "v" + split[1][0:-2]
                verse_footer = verse_footer.replace("<v>", version)
except Exception:
    pass
