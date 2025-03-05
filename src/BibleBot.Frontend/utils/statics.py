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
        f"../../src/BibleBot.Backend/obj/{config}/net9.0/BibleBot.Backend.AssemblyInfo.cs",
        "r",
    ).readlines()

    for line in __assembly_info_file__:
        if "AssemblyInformationalVersionAttribute" in line:
            quotation_split = line.split('"')

            if quotation_split:
                version = "v" + quotation_split[1].split("+")[0]
                verse_footer = verse_footer.replace("<v>", version)
except:
    pass
