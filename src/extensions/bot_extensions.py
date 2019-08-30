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

from handlers.logic.settings import versions
from handlers.logic.settings import languages, misc
from handlers.logic.commands import utils
from bible_modules import bibleutils


async def run_timed_votds(self):
    await self.wait_until_ready()

    while not self.is_closed():
        results = [x for x in central.guildDB.all() if "channel" in x]

        for item in results:
            if "channel" in item and "time" in item:
                channel = self.get_channel(item["channel"])
                votd_time = item["time"]

                try:
                    version = versions.get_guild_version(channel.guild)
                    lang = languages.get_guild_language(channel.guild)
                except AttributeError:
                    version = "RSV"
                    lang = "default"

                lang = central.get_raw_language(lang)

                current_time = datetime.datetime.utcnow().strftime("%H:%M")

                if votd_time == current_time:
                    await channel.send(lang["votd"])

                    reference = bibleutils.get_votd()

                    # noinspection PyBroadException
                    try:
                        result = utils.get_bible_verse(reference, version, "enable", "enable")
                        await channel.send(result["message"])
                    except Exception:
                        pass

        # central.log_message("info", 0, "votd_sched", "global", "Sending VOTDs...")
        await asyncio.sleep(60)


async def send_server_count(bot):
    dbl_token = central.config["apis"]["discordbots"]

    if dbl_token:
        headers = {"Authorization": dbl_token}
        data = {"server_count": len(bot.guilds)}
        url = f"https://discordbots.org/api/bots/{bot.user.id}/stats"

        async with aiohttp.ClientSession() as session:
            await session.post(url, data=data, headers=headers)

        central.log_message("info", 0, "global", "global", "Server count sent to Discordbots.org.")


async def send_announcement(ctx, res):
    count = 1
    total = len(ctx["bot"].guilds)

    for guild in ctx["bot"].guilds:
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
                ch = ctx["bot"].get_channel(chan)
                perm = ch.permissions_for(guild.me)

                if perm.read_messages and perm.send_messages:
                    if perm.embed_links:
                        msg = await ch.send(embed=res["message"])
                    else:
                        msg = await ch.send(res["message"].fields[0].value)

                    if msg:
                        await ctx["channel"].send(
                            f"`{str(count)} / {str(total)} - {guild.name} - {msg.id}` " +
                            ":white_check_mark:")
                    else:
                        await ctx["channel"].send(
                            f"`{str(count)} / {str(total)} - {guild.name}` " +
                            ":regional_indicator_x:")
                else:
                    await ctx["channel"].send(
                        f"`{str(count)} / {str(total)} - {guild.name}` " +
                        ":regional_indicator_x:")

                count += 1
            elif chan == "preferred" and setting:
                sent = False

                for ch in guild.text_channels:
                    try:
                        if not sent and ch.name in preferred:
                            perm = ch.permissions_for(guild.me)

                            if perm.read_messages and perm.send_messages:
                                if perm.embed_links:
                                    msg = await ch.send(embed=res["message"])
                                else:
                                    msg = await ch.send(res["message"].fields[0].value)

                                if msg:
                                    await ctx["channel"].send(
                                        f"`{str(count)} / {str(total)} - {guild.name} - {msg.id}` " +
                                        ":white_check_mark:")
                                else:
                                    await ctx["channel"].send(
                                        f"`{str(count)} / {str(total)} - {guild.name}` " +
                                        ":regional_indicator_x:")

                            else:
                                await ctx["channel"].send(
                                    f"`{str(count)} / {str(total)} - {guild.name}` " +
                                    ":regional_indicator_x:")

                            count += 1
                            sent = True
                    except (AttributeError, IndexError):
                        sent = True
            else:
                await ctx["channel"].send(
                    f"`{str(count)} / {str(total)} - {guild.name}` " +
                    ":regional_indicator_x:")

                count += 1

    await ctx["channel"].send(ctx["author"].mention + ": Announcements completed.")
