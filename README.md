<p align="center">
    <a alt="BibleBot" href="https://biblebot.xyz">
        <img alt="BibleBot" width="400px" src="https://i.imgur.com/JVBY24z.png">
    </a>
    <br>
    <br>
    <a href="https://github.com/BibleBot/BibleBot/actions?query=workflow%3A%22docker+%28dev%29%22">
        <img alt="GitHub Workflow Status" src="https://github.com/BibleBot/BibleBot/workflows/docker%20(dev)/badge.svg">
    </a>
    <a href="https://github.com/BibleBot/BibleBot/actions?query=workflow%3Atests">
        <img alt="GitHub Test Status" src="https://github.com/BibleBot/BibleBot/workflows/tests/badge.svg">
    </a>
    <a href="https://github.com/BibleBot/BibleBot/actions?query=workflow%3A%22docker+%28prod%29%22">
        <img alt="GitHub Workflow Status" src="https://github.com/BibleBot/BibleBot/workflows/docker%20(prod)/badge.svg">
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
- `BibleBot.Frontend` will eventually exist as a .NET Core Discord bot utilizing DSharpPlus.

## Prerequisites

- .NET Core 5.0 (SDK/Runtime)
- ASP.NET Core 5.0 (Runtime)
- Docker

## Self-Host Setup
This is from the old Golang backend which we'll derive from later. **This does not work.**

```bash
git clone https://github.com/BibleBot/backend && cd backend
cp config.example.yml && $EDITOR config.yml

# build production container
# the build-arg is optional if you're wanting localhost *without* HTTPS
docker build --build-arg DOMAIN=<domain> -t backend .
docker run -dp 443:443 backend
```

## Special Thanks

To our financial supporters to help us keep this project's lights on.  
To our volunteer translators helping BibleBot be more accessible to everyone.  
To our licensing coordinators for helping us sift through all the darn permissions requests.  
To our support team for helping others use BibleBot.
