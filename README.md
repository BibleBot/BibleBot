# BibleBot
[![Help Translate on Crowdin](https://d322cqt584bo4o.cloudfront.net/biblebot/localized.svg)](https://crowdin.com/project/biblebot) [![Join us on Discord](https://img.shields.io/discord/362503610006765568.svg)](https://discord.gg/seKEJUn) [![Discord Bots](https://discordbots.org/api/widget/lib/361033318273384449.png)](https://discordbots.org/bot/361033318273384449)

A Discord bot for Bible verses.

To use it, just say a Bible verse.

---

Installation:

```bash
git clone https://github.com/BibleBot/BibleBot.git
cd BibleBot
git submodule update --init
python3 -m venv venv
source venv/bin/activate
cp src/config.example.ini src/config.ini
$EDITOR src/config.ini
pip install -U "https://github.com/Rapptz/discord.py/archive/rewrite.zip#egg=discord.py[voice]"
pip install -U tinydb colorama requests bs4 lxml
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
* `+ban id reason` - ban an id from using the bot (works for both users and guilds)
* `+unban id reason` - unban an id from using the bot (works for both users and guilds)
* `+reason id` - get ban reason for an id
* `+optout id` - optout an id from using the bot (works for users)
* `+unoptout id` - unoptout an id from using the bot (works for users)
* `+leave NAME` - leave the current or NAME server (argument optional)

Invite BibleBot to your server! https://discordapp.com/oauth2/authorize?client_id=361033318273384449&scope=bot&permissions=93248

---

Versioning:

Every commit, add 1 to the last number of the version, if the result is 10,
add 1 to the second number of the version. If the result of the second number is 10,
add 1 to the first number of the version.

Examples:  
2.8.9 --> Commit --> 2.9.0  
2.9.8 --> Commit --> 2.9.9  
2.9.9 --> Commit --> 3.0.0  

Every commit done involving the code itself must have the version number updated.   
Commits done to the README, the package.json file (except when adding dependencies),   
and the dotfiles do not need to have the version number updated.   

---

### Special Thanks

**adfizz, apocz, audiovideodisco, Blubb, BonaventureSissokovitch, Buggyrcobra, Coal, DeadPixels, jznsamuel, Koockies, Mark Nunberg, Manelic, Raven Melodie, omeratagun, Sezess, sunray.steemit, SwedishMeatball, Tuonela, TySpeedy, Viva98, xnkmevaou, Zyxl and many more** for their hard work on helping BibleBot reach the world by translating languages :heart:
