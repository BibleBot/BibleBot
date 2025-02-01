"""
    Copyright (C) 2016-2025 Kerygma Digital Co.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this file,
    You can obtain one at https://mozilla.org/MPL/2.0/.
"""

from disnake.ext import commands
from cogs import events
from cogs import versions
from cogs import formatting
from cogs import languages
from cogs import info
from cogs import verse_cmds
from cogs import resources


def setup(bot: commands.Bot):
    bot.add_cog(events.EventListeners(bot))
    bot.add_cog(versions.Versions(bot))
    bot.add_cog(formatting.Formatting(bot))
    bot.add_cog(languages.Languages(bot))
    bot.add_cog(info.Information(bot))
    bot.add_cog(verse_cmds.VerseCommands(bot))
    bot.add_cog(resources.Resources(bot))
