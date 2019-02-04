import discord
import central
import ast


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

    embed.set_footer(text=central.version, icon_url=central.icon)

    if custom_title:
        embed.title = title
    else:
        embed.title = central.config["BibleBot"]["commandPrefix"] + title

    embed.description = description

    return embed
