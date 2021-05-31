<p align="center">
    <a alt="BibleBot" href="https://biblebot.xyz">
        <img alt="BibleBot" width="400px" src="https://i.imgur.com/JVBY24z.png">
    </a>
    <br>
    <br>
    <a href="https://coveralls.io/github/BibleBot/BibleBot?branch=master">
        <img src="https://coveralls.io/repos/github/BibleBot/BibleBot/badge.svg?branch=master" alt="Coverage Status">
    </a>
    <a href="https://ci.appveyor.com/project/SeraphimRP/biblebot">
        <img alt="AppVeyor Build Status" src="https://ci.appveyor.com/api/projects/status/x6pdy1e2aw1vstru?svg=true">
    </a>
    <a href="https://www.codacy.com/gh/BibleBot/BibleBot/dashboard?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=BibleBot/BibleBot&amp;utm_campaign=Badge_Grade">
        <img src="https://app.codacy.com/project/badge/Grade/0ebeb56c612a4643851d9beb1003a1de">
    </a>
    <br>
    <a alt="Discord" href="https://discord.gg/H7ZyHqE">
        <img alt="Discord" src="https://img.shields.io/discord/362503610006765568?label=discord">
    </a>
    <a href="https://github.com/BibleBot/BibleBot/blob/master/LICENSE.txt">
        <img alt="MPL-2.0" src="https://img.shields.io/github/license/BibleBot/BibleBot">
    </a>
    <br>
</p>
<p align="center">
    The premier Discord bot for Christians.
</p>

## Internal Organization

This repository is a monolith containing subprojects in `src/`. These various projects do as follows:

- `BibleBot.Backend` is our ASP.NET Core backend API, it does most of the heavy lifting of the project.
- `BibleBot.Lib` is a class library containing shared models between Backend and Frontend.
- `BibleBot.Frontend` is a .NET Core Discord bot utilizing DSharpPlus that acts as a middleman between Discord and BibleBot.Backend.

## Prerequisites

- .NET Core 5.0 (SDK/Runtime)
- ASP.NET Core 5.0 (Runtime)

## Special Thanks

To our financial supporters to help us keep this project's lights on.  
To our volunteer translators helping BibleBot be more accessible to everyone.  
To our licensing coordinators for helping us sift through all the darn permissions requests.  
To our support team for helping others use BibleBot.
