import disnake
from disnake.ext import commands
from logger import VyLogger
import os

logger = VyLogger("default")

intents = disnake.Intents.default()
intents.message_content = True
bot = commands.AutoShardedBot(
    command_prefix=commands.when_mentioned,
    intents=intents,
    test_guilds=[362503610006765568],
    sync_commands_debug=True,
)

bot.load_extension("cogs")

bot.run(os.environ.get("DISCORD_TOKEN"))
