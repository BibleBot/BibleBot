<p align="center">
    <a alt="BibleBot" href="https://biblebot.xyz">
        <img alt="BibleBot" width="400px" src="https://i.imgur.com/JVBY24z.png">
    </a>
    <br>
    <br>
    <a href="https://codecov.io/gh/BibleBot/BibleBot">
        <img alt="Codecov Coverage Status" src="https://img.shields.io/codecov/c/github/BibleBot/BibleBot?token=Z0bU0yHqEN&style=flat-square">
    </a>
    <a href="https://www.codacy.com/gh/BibleBot/BibleBot/dashboard">
        <img alt="Codacy Quality Status" src="https://img.shields.io/codacy/grade/0ebeb56c612a4643851d9beb1003a1de?style=flat-square">
    </a>
    <a href="https://ci.appveyor.com/project/SeraphimRP/biblebot">
        <img alt="AppVeyor Build Status" src="https://img.shields.io/appveyor/build/SeraphimRP/BibleBot?style=flat-square">
    </a>
    <a href="https://ci.appveyor.com/project/SeraphimRP/biblebot/build/tests">
        <img alt="AppVeyor Test Status" src="https://img.shields.io/appveyor/tests/SeraphimRP/BibleBot?style=flat-square">
    </a>
    <br>
    <a alt="Discord" href="https://discord.gg/H7ZyHqE">
        <img alt="Discord" src="https://img.shields.io/discord/362503610006765568?label=discord&style=flat-square">
    </a>
    <a href="https://github.com/BibleBot/BibleBot/blob/master/LICENSE">
        <img alt="MPL-2.0" src="https://img.shields.io/github/license/BibleBot/BibleBot?style=flat-square">
    </a>
    <img alt="Total Lines of Code" src="https://img.shields.io/tokei/lines/github/BibleBot/BibleBot?style=flat-square">
    <br>
</p>
<p align="center">
    Scripture from your Discord client to your heart.
</p>

## Internal Organization

This repository is a monolith containing subprojects in `src/`. These various projects do as follows:

- `BibleBot.Backend` is our ASP.NET Core backend API, it does most of the heavy lifting of the project.
- `BibleBot.Lib` is a class library containing shared models between Backend and Frontend.
- `BibleBot.Frontend` is a .NET Core Discord bot utilizing DSharpPlus that acts as a middleman between Discord and BibleBot.Backend.

## Special Thanks

To our financial supporters to help us keep this project's lights on.  
To our volunteer translators helping BibleBot be more accessible to everyone.  
To our licensing coordinators for helping us sift through all the darn permissions requests.  
To our support team for helping others use BibleBot.
