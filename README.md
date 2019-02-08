# BibleBot
[![Codacy Badge](https://api.codacy.com/project/badge/Grade/3c19643f9d964f3daa0e2e70c1ad83f4)](https://app.codacy.com/app/vypr/BibleBot?utm_source=github.com&utm_medium=referral&utm_content=BibleBot/BibleBot&utm_campaign=Badge_Grade_Dashboard)
[![Help Translate on Crowdin](https://d322cqt584bo4o.cloudfront.net/biblebot/localized.svg)](https://crowdin.com/project/biblebot)
[![Join us on Discord](https://img.shields.io/discord/362503610006765568.svg)](https://discord.gg/H7ZyHqE)
[![Discord Bots](https://discordbots.org/api/widget/lib/361033318273384449.png)](https://discordbots.org/bot/361033318273384449)


The premier Discord bot for Bible verses.

To use it, just say a Bible verse.

## Self-Host Installation

### Linux/MacOS
```bash
git clone https://github.com/BibleBot/BibleBot.git
cd BibleBot
git submodule update --init
git submodule foreach git pull origin master
python3 -m venv venv
source venv/bin/activate
cp src/config.example.ini src/config.ini
$EDITOR src/config.ini
pip install -U "https://github.com/Rapptz/discord.py/archive/rewrite.zip#egg=discord.py[voice]"
pip install -U bs4 colorama lxml requests tinydb
python src/bot.py
```

### Windows (requires the GitHub client)

#### Step 1. Git Bash
```
git clone https://github.com/BibleBot/BibleBot.git
cd BibleBot
git submodule update --init
git submodule foreach git pull origin master
```

#### Step 2. Command Prompt
```
python -m venv venv
.\venv\Scripts\activate
copy src\config.example.ini src\config.ini
notepad src\config.ini
pip install -U "https://github.com/Rapptz/discord.py/archive/rewrite.zip#egg=discord.py[voice]"
pip install -U bs4 colorama lxml requests tinydb
python src/bot.py
```

---

Commands:

* `+biblebot` - the help command
* `+versions` - show all Bible translations you can set
* `+setversion VER` - set a preferred version
* `+version` - display your current version
* `+versioninfo VER` - read information about a version, using the acronym
* `+random` - get a random Bible verse
* `+verseoftheday` (`+votd`) - get the verse of the day
* `+headings enable/disable` - enable or disable the headings that display on certain verses
* `+versenumbers enable/disable` - enable or disable verse numbers from showing on each line
* `+languages` - show all available language translations you can set
* `+setlanguage LANG` - set a preferred language
* `+language` - display your current language
* `+users` - list all users throughout all servers (not counting duplicates)
* `+servers` - list all servers BibleBot is in
* `+invite` - get the invite link for BibleBot

Extra-Biblical Commands:

* `+creeds` - the ecumenical creeds (contains further commands)

Guild Commands:

* `+setguildversion ABBV` - set the guild's default version to ABBV
* `+guildversion` - see the default version for this guild
* `+setguildlanguage LANG` - set the guild's default language to LANG
* `+guildlanguage` - see the guild's default language, if one is set (default: english)
* `+setvotdtime TIME` - set the VOTD scheduler time to TIME (TIME must be 24h format and in UTC)
* `+clearvotdtime` - clear the VOTD scheduler time
* `+votdtime` - see the VOTD scheduler time and channel

Bot Owner Commands:

* `+addversion versionname abbv hasOT hasNT hasDEU` (`+av`) - add a version
* `+puppet message` - say something as the bot (requires 'Manage Messages' perms in order to fully function)
* `+eval python` - execute python code (it's an exec() wrapper)
* `+userid name#discriminator` - grab a user id by name and discriminator
* `+optout id` - optout an id from using the bot (works for users)
* `+unoptout id` - unoptout an id from using the bot (works for users)
* `+leave NAME` - leave the current or NAME server (argument optional)

Invite BibleBot to your server!  
https://discordapp.com/oauth2/authorize?client_id=361033318273384449&scope=bot&permissions=93248

## Permissions

BibleBot requires the following permissions in order to function properly:

- Read Messages, Send Messages - Obviously.
- Embed Links - This is for BibleBot to use the Discord `embed` object, as BibleBot uses these for everything besides verses. 
  - Example:  
  ![](https://i.imgur.com/3XT6Md0.png)
  
- Add Reactions, Manage Messages (to clear reactions after timeout), Read Message History - To use reactions properly on things like +search and +versions.
  - Example:  
  ![](https://i.imgur.com/DosRFtd.gif)
  
## Special Thanks

- My translators on Crowdin for their hard work on helping BibleBot reach the world.
- My Patreon supporters for helping fund development and keep BibleBot running.