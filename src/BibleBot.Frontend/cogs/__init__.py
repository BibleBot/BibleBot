"""
Copyright (C) 2016-2026 Kerygma Digital Co.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this file,
You can obtain one at https://mozilla.org/MPL/2.0/.
"""

from disnake.ext import commands

from cogs import (
    events,
    formatting,
    info,
    languages,
    resources,
    staff,
    tasks,
    verse_cmds,
    versions,
)


def setup(bot: commands.Bot):
    bot.add_cog(events.EventListeners(bot))
    bot.add_cog(versions.Versions(bot))
    bot.add_cog(formatting.Formatting(bot))
    bot.add_cog(languages.Languages(bot))
    bot.add_cog(info.Information(bot))
    bot.add_cog(verse_cmds.VerseCommands(bot))
    bot.add_cog(resources.Resources(bot))
    bot.add_cog(tasks.Tasks(bot))
    bot.add_cog(staff.Staff(bot))
