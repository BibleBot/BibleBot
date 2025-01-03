<p align="center">
    <a alt="BibleBot" href="https://biblebot.xyz">
        <img alt="BibleBot" width="400px" src="https://i.imgur.com/JVBY24z.png">
    </a>
    <br>
    <br>
    <a href="https://gitlab.com/kerygmadigital/biblebot/BibleBot/-/pipelines/latest">
        <img alt="GitLab Pipeline Status" src="https://img.shields.io/gitlab/pipeline-status/kerygmadigital%2Fbiblebot%2FBibleBot?branch=master&style=flat-square&logo=gitlab&label=pipeline">
    </a>
    </a>
    <a href="https://app.codacy.com/gl/kerygmadigital/BibleBot/dashboard">
        <img alt="Codacy Coverage Status" src="https://img.shields.io/codacy/coverage/2f77eda59dca4477b7b127f94c76db62?style=flat-square&logo=codacy">
    </a>
    <a href="https://app.codacy.com/gl/kerygmadigital/BibleBot/dashboard">
        <img alt="Codacy Quality Status" src="https://img.shields.io/codacy/grade/2f77eda59dca4477b7b127f94c76db62?style=flat-square&logo=codacy">
    </a>
    <br>
    <a alt="Discord" href="https://biblebot.xyz/discord">
        <img alt="Discord" src="https://img.shields.io/discord/362503610006765568?label=discord&style=flat-square">
    </a>
    <a href="https://gitlab.com/kerygmadigital/biblebot/BibleBot/-/blob/master/LICENSE">
        <img alt="MPL-2.0" src="https://img.shields.io/gitlab/license/kerygmadigital%2FBibleBot%2Fbiblebot?style=flat-square">
    </a>
    <br>
</p>
<p align="center">
    Scripture from your Discord client to your heart.
</p>

## Internal Organization

This repository is a monolith containing subprojects in `src/`. These various projects do as follows:

-   `BibleBot.Backend` is our ASP.NET Core backend API, it does most of the heavy lifting of the project.
-   `BibleBot.Frontend` is a Python Discord bot utilizing disnake that acts as a middleman between Discord and BibleBot.Backend.
-   `BibleBot.AutomaticServices` is a minimized version of BibleBot.Backend, to handle things like automatic daily verses and sending our version statistics to our community management.
-   `BibleBot.Models` is a class library to share models between the projects.

## Special Thanks

To our financial supporters to help us keep this project's lights on.  
To our volunteer translators helping BibleBot be more accessible to everyone.  
To our licensing coordinators for helping us sift through all the darn permissions requests.  
To our support team for helping others use BibleBot.
