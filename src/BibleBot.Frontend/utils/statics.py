import os

version = "undefined"
verse_footer = "BibleBot <v> by Kerygma Digital"

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
                version = "v" + split[1].split(";")[0]
                verse_footer = verse_footer.replace("<v>", version)
except:
    pass
