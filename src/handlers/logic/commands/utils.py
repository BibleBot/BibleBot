import discord
import central
import ast
from bible_modules import biblehub, bibleserver, biblesorg, biblegateway, rev


def divide_list(dividend, divisor):
    # tl;dr for every n (divisor) entries, separate and place into a new array
    # divide_list([1,2,3,4], 2) = [[1,2],[3,4]]
    return [dividend[i:i + divisor] for i in range(0, len(dividend), divisor)]


def insert_returns(body):  # for +eval, thanks to nitros12 on github for the code
    # insert return stmt if the last expression is a expression statement
    if isinstance(body[-1], ast.Expr):
        body[-1] = ast.Return(body[-1].value)
        ast.fix_missing_locations(body[-1])

    # for if statements, we insert returns into the body and the orelse
    if isinstance(body[-1], ast.If):
        insert_returns(body[-1].body)
        insert_returns(body[-1].orelse)

    # for with blocks, again we insert returns into the body
    if isinstance(body[-1], ast.With):
        insert_returns(body[-1].body)


def create_embed(title, description, custom_title=False, error=False):
    embed = discord.Embed()

    if error:
        embed.color = 16723502
    else:
        embed.color = 303102

    embed.set_footer(text=f"BibleBot {central.version}", icon_url=central.icon)

    if custom_title:
        embed.title = title
    else:
        embed.title = central.config["BibleBot"]["commandPrefix"] + title

    embed.description = description

    return embed


def get_bible_verse(reference, version, headings, verse_numbers):
    biblehub_versions = ["BSB", "NHEB", "WBT"]
    bibleserver_versions = ["LUT", "LXX", "SLT"]
    biblesorg_versions = ["KJVA"]
    other_versions = ["REV"]

    non_bg = other_versions + biblehub_versions + biblesorg_versions + bibleserver_versions

    if version not in non_bg:
        result = biblegateway.get_result(reference, version, headings, verse_numbers)

        if result is not None:
            if result["text"][0] != " ":
                result["text"] = " " + result["text"]

            content = "```Dust\n" + result["title"] + "\n\n" + result["text"] + "```"
            response_string = "**" + result["passage"] + " - " + result[
                "version"] + "**\n\n" + content

            if len(response_string) < 2000:
                return {
                    "level": "info",
                    "reference": reference,
                    "message": response_string
                }
    elif version == "REV":
        result = rev.get_result(reference, verse_numbers)

        if result["text"][0] != " ":
            result["text"] = " " + result["text"]

        content = "```Dust\n" + result["text"] + "```"
        response_string = "**" + result["passage"] + " - " + result["version"] + "**\n\n" + content

        if len(response_string) < 2000:
            return {
                "level": "info",
                "reference": reference,
                "message": response_string
            }
    elif version in biblesorg_versions:
        result = biblesorg.get_result(reference, version, headings, verse_numbers)

        if result is not None:
            if result["text"][0] != " ":
                result["text"] = " " + result["text"]

            content = "```Dust\n" + result["title"] + "\n\n" + result["text"] + "```"
            response_string = "**" + result["passage"] + " - " + result[
                "version"] + "**\n\n" + content

            if len(response_string) < 2000:
                return {
                    "level": "info",
                    "reference": reference,
                    "message": response_string
                }
    elif version in biblehub_versions:
        result = biblehub.get_result(reference, version, verse_numbers)

        if result is not None:
            if result["text"][0] != " ":
                result["text"] = " " + result["text"]

            content = "```Dust\n" + result["title"] + "\n\n" + result["text"] + "```"
            response_string = "**" + result["passage"] + " - " + result[
                "version"] + "**\n\n" + content

            if len(response_string) < 2000:
                return {
                    "level": "info",
                    "reference": reference,
                    "message": response_string
                }
    elif version in bibleserver_versions:
        result = bibleserver.get_result(reference, version, verse_numbers)

        if result is not None:
            if result["text"][0] != " ":
                result["text"] = " " + result["text"]

            content = "```Dust\n" + result["title"] + "\n\n" + result["text"] + "```"
            response_string = "**" + result["passage"] + " - " + result[
                "version"] + "**\n\n" + content

            if len(response_string) < 2000:
                return {
                    "level": "info",
                    "reference": reference,
                    "message": response_string
                }
