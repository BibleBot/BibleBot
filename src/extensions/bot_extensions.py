import central
import asyncio
import datetime

from handlers.logic.commands.settings import versions, languages, misc
from bible_modules import biblegateway, biblehub, bibleserver, biblesorg, bibleutils, rev


async def run_timed_votds(self):
    await self.wait_until_ready()

    while not self.is_closed():
        # a nice list comprehension for getting all the servers with votd stuff set
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
                    lang = "english_us"

                lang = getattr(central.languages, lang).raw_object

                current_time = datetime.datetime.utcnow().strftime("%H:%M")

                if votd_time == current_time:
                    await channel.send(lang["votd"])

                    reference = bibleutils.get_votd()

                    biblehub_versions = ["BSB", "NHEB", "WBT"]
                    bibleserver_versions = ["LUT", "LXX", "SLT"]
                    biblesorg_versions = ["KJVA"]
                    other_versions = ["REV"]

                    non_bg = other_versions + biblehub_versions + biblesorg_versions + bibleserver_versions

                    if version not in non_bg:
                        result = biblegateway.get_result(reference, version, "enable", "enable")

                        if result is not None:
                            if result["text"][0] != " ":
                                result["text"] = " " + result["text"]

                            content = "```Dust\n" + result["title"] + "\n\n" + result["text"] + "```"
                            response_string = "**" + result["passage"] + " - " + result[
                                "version"] + "**\n\n" + content

                            if len(response_string) < 2000:
                                await channel.send(response_string)
                            elif 2000 < len(response_string) < 3500:
                                split_text = central.splitter(result["text"])

                                content1 = "```Dust\n" + result["title"] + "\n\n" + split_text["first"] + "```"
                                response_string1 = "**" + result["passage"] + " - " + result["version"] + "**" + \
                                                   "\n\n" + content1

                                content2 = "```Dust\n" + split_text["second"] + "```"

                                await channel.send(response_string1)
                                await channel.send(content2)
                            else:
                                await channel.send(lang["votdcantprocess"])
                    elif version == "REV":
                        result = rev.get_result(reference, "enable")

                        if result["text"][0] != " ":
                            result["text"] = " " + result["text"]

                        content = "```Dust\n" + result["text"] + "```"
                        response_string = "**" + result["passage"] + " - " + result["version"] + "**\n\n" + content

                        if len(response_string) < 2000:
                            await channel.send(response_string)
                        elif 2000 < len(response_string) < 3500:
                            split_text = central.splitter(result["text"])

                            content1 = "```Dust\n" + split_text["first"] + "```"
                            response_string1 = "**" + result["passage"] + " - " + result["version"] + "**" + \
                                               "\n\n" + content1

                            content2 = "```Dust\n" + split_text["second"] + "```"

                            await channel.send(response_string1)
                            await channel.send(content2)
                        else:
                            await channel.send(lang["votdcantprocess"])
                    elif version in biblesorg_versions:
                        result = biblesorg.get_result(reference, version, "enable", "enable")

                        if result is not None:
                            if result["text"][0] != " ":
                                result["text"] = " " + result["text"]

                            content = "```Dust\n" + result["title"] + "\n\n" + result["text"] + "```"
                            response_string = "**" + result["passage"] + " - " + result[
                                "version"] + "**\n\n" + content

                            if len(response_string) < 2000:
                                await channel.send(response_string)
                            elif len(response_string) > 2000:
                                if len(response_string) < 3500:
                                    split_text = central.splitter(result["text"])

                                    content1 = "```Dust\n" + result["title"] + "\n\n" + split_text["first"] + "```"
                                    response_string1 = "**" + result["passage"] + " - " + result["version"] + "**" + \
                                                       "\n\n" + content1

                                    content2 = "```Dust\n" + split_text["second"] + "```"

                                    await channel.send(response_string1)
                                    await channel.send(content2)
                                else:
                                    await channel.send(lang["votdcantprocess"])
                    elif version in biblehub_versions:
                        result = biblehub.get_result(reference, version, "enable")

                        if result is not None:
                            if result["text"][0] != " ":
                                result["text"] = " " + result["text"]

                            content = "```Dust\n" + result["title"] + "\n\n" + result["text"] + "```"
                            response_string = "**" + result["passage"] + " - " + result[
                                "version"] + "**\n\n" + content

                            if len(response_string) < 2000:
                                await channel.send(response_string)
                            elif len(response_string) > 2000:
                                if len(response_string) < 3500:
                                    split_text = central.splitter(result["text"])

                                    content1 = "```Dust\n" + result["title"] + "\n\n" + split_text["first"] + "```"
                                    response_string1 = "**" + result["passage"] + " - " + result["version"] + "**" + \
                                                       "\n\n" + content1

                                    content2 = "```Dust\n" + split_text["second"] + "```"

                                    await channel.send(response_string1)
                                    await channel.send(content2)
                                else:
                                    await channel.send(lang["votdcantprocess"])
                    elif version in bibleserver_versions:
                        result = bibleserver.get_result(reference, version, "enable")

                        if result is not None:
                            if result["text"][0] != " ":
                                result["text"] = " " + result["text"]

                            content = "```Dust\n" + result["title"] + "\n\n" + result["text"] + "```"
                            response_string = "**" + result["passage"] + " - " + result[
                                "version"] + "**\n\n" + content

                            if len(response_string) < 2000:
                                await channel.send(response_string)
                            elif len(response_string) > 2000:
                                if len(response_string) < 3500:
                                    split_text = central.splitter(result["text"])

                                    content1 = "```Dust\n" + result["title"] + "\n\n" + split_text["first"] + "```"
                                    response_string1 = "**" + result["passage"] + " - " + result["version"] + "**" + \
                                                       "\n\n" + content1

                                    content2 = "```Dust\n" + split_text["second"] + "```"

                                    await channel.send(response_string1)
                                    await channel.send(content2)
                                else:
                                    await channel.send(lang["votdcantprocess"])

        # central.log_message("info", 0, "votd_sched", "global", "Sending VOTDs...")
        await asyncio.sleep(60)


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
                    f"`{str(count)} / {str(total)} - {guild.name} - {msg.id}` " +
                    ":regional_indicator_x:")

                count += 1

    await ctx["channel"].send(ctx["author"].mention + ": Announcements completed.")
