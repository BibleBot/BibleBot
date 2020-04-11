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

import central
import asyncio
import aiohttp
import datetime
import discord
import traceback

from discord.ext import tasks
from handlers.logic.settings import versions
from handlers.logic.settings import languages, misc
from handlers.logic.commands import utils
from bible_modules import bibleutils

@tasks.loop(seconds=60)
async def run_timed_votds(bot):
    await bot.wait_until_ready()
    
    current_time = datetime.datetime.utcnow().strftime("%H:%M")
    results = [x for x in central.guildDB.all() if "channel" in x and x["time"] == current_time]

    count = 0

    for item in results:
        if "channel" in item and "time" in item:
            channel = bot.get_channel(item["channel"])
                                                       
            try:
                version = versions.get_guild_version(channel.guild)
                lang = languages.get_guild_language(channel.guild)
            except AttributeError:
                version = "RSV"
                lang = "english"

            if not version:
                version = "RSV"

            if not lang:
                lang = "english"

            lang = central.get_raw_language(lang)

            if channel:
                reference = await bibleutils.get_votd()
                result = await utils.get_bible_verse(reference, "embed", version, "enable", "enable")
                
                if result:
                    try:
                        await channel.send(lang["votd"])
                        await channel.send(embed=result["embed"])
                    except discord.errors.Forbidden:
                        pass
                    count += 1
    
    if count > 0:
        central.log_message("info", 0, "votd_sched", "global", f"Sending {str(count)} VOTDs at {current_time}...")


async def send_server_count(bot):
    dbl_token = central.config["apis"]["topgg"]

    if dbl_token:
        headers = {"Authorization": dbl_token}
        data = {"server_count": len(bot.guilds)}
        url = f"https://top.gg/api/bots/{bot.user.id}/stats"

        async with aiohttp.ClientSession() as session:
            await session.post(url, data=data, headers=headers)

        central.log_message("info", 0, "global", "global", "Server count sent to top.gg.")


async def send_announcement(ctx, res):
    count = 1
    total = len(ctx["self"].guilds)
    message_counter = None

    for guild in ctx["self"].guilds:
        announce_tuple = misc.get_guild_announcements(guild, False)

        if "Discord Bot" not in guild.name:
            if announce_tuple is not None:
                chan, setting = announce_tuple
            else:
                chan = "preferred"
                setting = True

            preferred = ["misc", "bots", "meta", "hangout", "fellowship", "lounge",
                         "congregation", "general", "bot-spam", "staff"]

            if chan != "preferred" and setting:
                try:
                    ch = ctx["self"].get_channel(chan)
                    perm = ch.permissions_for(guild.me)

                    if perm.read_messages and perm.send_messages:
                        if perm.embed_links:
                            await ch.send(embed=res["message"])
                        else:
                            await ch.send(res["message"].fields[0].value)

                    count += 1
                except (AttributeError, IndexError):
                    count += 1
            elif chan == "preferred" and setting:
                sent = False

                for ch in guild.text_channels:
                    try:
                        if not sent and ch.name in preferred:
                            perm = ch.permissions_for(guild.me)

                            if perm.read_messages and perm.send_messages:
                                if perm.embed_links:
                                    await ch.send(embed=res["message"])
                                else:
                                    await ch.send(res["message"].fields[0].value)

                            count += 1
                            sent = True
                    except (AttributeError, IndexError):
                        count += 1
                        sent = True
            else:
                count += 1

        message_counter = await update_counter(message_counter, ctx, count, total)


def craft_counting_embed(count, total, done=None):
    embed = discord.Embed()

    embed.color = 303102
    embed.set_footer(text=f"BibleBot {central.version}", icon_url=central.icon)

    embed.title = "Announcements"

    percentage = "{:.1%}".format(count / total)
    progress = f"Progress: {percentage} ({str(count)}/{str(total)})"

    if done:
        embed.description = f"Announcements completed.\n\n"
    else:
        embed.description = progress

    return embed


async def update_counter(message_counter, ctx, count, total):
    if count == 1:
        embed = craft_counting_embed(count, total)
        message_counter = await ctx["channel"].send(embed=embed)
    elif message_counter:
        embed = craft_counting_embed(count, total)
        message_counter = await message_counter.edit(embed=embed)

    return message_counter
