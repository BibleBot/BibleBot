from disnake.ext import commands
from cogs import events
from cogs import versions
from cogs import formatting
from cogs import info
from cogs import verse_cmds


def setup(bot: commands.Bot):
    bot.add_cog(events.EventListeners(bot))
    bot.add_cog(versions.Versions(bot))
    bot.add_cog(formatting.Formatting(bot))
    bot.add_cog(info.Information(bot))
    bot.add_cog(verse_cmds.VerseCommands(bot))
