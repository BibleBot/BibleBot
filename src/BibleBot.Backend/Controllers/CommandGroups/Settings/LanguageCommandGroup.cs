/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BibleBot.Backend.Services;
using BibleBot.Models;
using MongoDB.Driver;

namespace BibleBot.Backend.Controllers.CommandGroups.Settings
{
    public class LanguageCommandGroup(UserService userService, GuildService guildService, LanguageService languageService) : CommandGroup
    {
        public override string Name { get => "language"; set => throw new NotImplementedException(); }
        public override Command DefaultCommand { get => Commands.FirstOrDefault(cmd => cmd.Name == "usage"); set => throw new NotImplementedException(); }
        public override List<Command> Commands
        {
            get => [
                new LanguageUsage(userService, guildService, languageService),
                new LanguageSet(userService, languageService),
                new LanguageSetServer( guildService, languageService),
                new LanguageList(languageService)
            ]; set => throw new NotImplementedException();
        }

        public class LanguageUsage(UserService userService, GuildService guildService, LanguageService languageService) : Command
        {
            public override string Name { get => "usage"; set => throw new NotImplementedException(); }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                User idealUser = await userService.Get(req.UserId);
                Guild idealGuild = await guildService.Get(req.GuildId);

                Language defaultLanguage = await languageService.Get("english");

                string response = "Your preferred language is set to **<language>**.\n" +
                               "The server's preferred language is set to **<glanguage>**.\n\n" +
                               "__**Subcommands**__\n" +
                               "**set** - set your preferred language\n" +
                               "**setserver** - set the server's default language (staff only)\n" +
                               "**list** - list all available languages";

                if (idealUser != null)
                {
                    Language idealUserLanguage = await languageService.Get(idealUser.Language);

                    if (idealUserLanguage != null)
                    {
                        response = response.Replace("<language>", idealUserLanguage.Name);
                    }
                }

                if (idealGuild != null)
                {
                    Language idealGuildLanguage = await languageService.Get(idealGuild.Language);

                    if (idealGuildLanguage != null)
                    {
                        response = response.Replace("<glanguage>", idealGuildLanguage.Name);
                    }
                }

                response = response.Replace("<language>", defaultLanguage.Name);
                response = response.Replace("<glanguage>", defaultLanguage.Name);

                return new CommandResponse
                {
                    OK = true,
                    Pages =
                    [
                        Utils.GetInstance().Embedify("+language", response, false)
                    ],
                    LogStatement = "+language"
                };
            }
        }

        public class LanguageSet(UserService userService, LanguageService languageService) : Command
        {
            public override string Name { get => "set"; set => throw new NotImplementedException(); }
            public override string ArgumentsError { get => "Expected a language parameter, like `english` or `german`."; set => throw new NotImplementedException(); }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                string newLanguage = args[0].ToUpperInvariant();
                Language idealLanguage = await languageService.Get(newLanguage);

                if (idealLanguage != null)
                {
                    User idealUser = await userService.Get(req.UserId);

                    if (idealUser != null)
                    {
                        UpdateDefinition<User> update = Builders<User>.Update
                                     .Set(user => user.Language, idealLanguage.ObjectName);

                        await userService.Update(req.UserId, update);
                    }
                    else
                    {
                        User newUser = new()
                        {
                            UserId = req.UserId,
                            Language = idealLanguage.ObjectName
                        };

                        await userService.Create(newUser);
                    }

                    return new CommandResponse
                    {
                        OK = true,
                        Pages =
                        [
                            Utils.GetInstance().Embedify("+language set", "Set language successfully.", false)
                        ],
                        LogStatement = $"+language set {args[0]}"
                    };
                }

                return new CommandResponse
                {
                    OK = false,
                    Pages =
                    [
                        Utils.GetInstance().Embedify("+language set", "Failed to set language, see `+language list`.", true)
                    ],
                    LogStatement = "+language set"
                };
            }
        }

        public class LanguageSetServer(GuildService guildService, LanguageService languageService) : Command
        {
            public override string Name { get => "setserver"; set => throw new NotImplementedException(); }
            public override string ArgumentsError { get => "Expected a language parameter, like `english` or `german`."; set => throw new NotImplementedException(); }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                string newLanguage = args[0].ToUpperInvariant();
                Language idealLanguage = await languageService.Get(newLanguage);

                if (idealLanguage != null)
                {
                    Guild idealGuild = await guildService.Get(req.GuildId);

                    if (idealGuild != null)
                    {
                        UpdateDefinition<Guild> update = Builders<Guild>.Update
                                     .Set(guild => guild.Language, idealLanguage.ObjectName);

                        await guildService.Update(req.GuildId, update);
                    }
                    else
                    {
                        Guild newGuild = new()
                        {
                            GuildId = req.GuildId,
                            Language = idealLanguage.ObjectName,
                            IsDM = req.IsDM
                        };

                        await guildService.Create(newGuild);
                    }

                    return new CommandResponse
                    {
                        OK = true,
                        Pages =
                        [
                            Utils.GetInstance().Embedify("+language setserver", "Set server language successfully.", false)
                        ],
                        LogStatement = $"+language setserver {args[0]}"
                    };
                }

                return new CommandResponse
                {
                    OK = false,
                    Pages =
                    [
                        Utils.GetInstance().Embedify("+language setserver", "Failed to set server language, see `+language list`.", true)
                    ],
                    LogStatement = "+language setserver"
                };
            }
        }

        public class LanguageList(LanguageService languageService) : Command
        {
            public override string Name { get => "list"; set => throw new NotImplementedException(); }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                List<Language> languages = await languageService.Get();
                languages.Sort((x, y) => x.Name.CompareTo(y.Name));

                StringBuilder content = new();

                foreach (Language lang in languages)
                {
                    content.Append($"{lang.Name} `[{lang.ObjectName}]`\n");
                }

                return new CommandResponse
                {
                    OK = true,
                    Pages =
                    [
                        Utils.GetInstance().Embedify("+language list", content.ToString(), false)
                    ],
                    LogStatement = "+language list"
                };
            }
        }
    }
}
