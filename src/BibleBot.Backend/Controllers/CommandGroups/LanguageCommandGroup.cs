/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BibleBot.Backend.InternalModels;
using BibleBot.Backend.Services;
using BibleBot.Models;
using Microsoft.Extensions.Localization;
using MongoDB.Driver;

namespace BibleBot.Backend.Controllers.CommandGroups
{
    public class LanguageCommandGroup(UserService userService, GuildService guildService, LanguageService languageService, IStringLocalizerFactory localizerFactory) : CommandGroup
    {
        private readonly IStringLocalizer _localizer = localizerFactory.Create(typeof(LanguageCommandGroup));
        private readonly IStringLocalizer _sharedLocalizer = localizerFactory.Create(typeof(SharedResource));

        public override string Name { get => "language"; set => throw new NotImplementedException(); }
        public override Command DefaultCommand { get => Commands.FirstOrDefault(cmd => cmd.Name == "usage"); set => throw new NotImplementedException(); }
        public override List<Command> Commands
        {
            get => [
                new LanguageUsage(userService, guildService, languageService, _localizer, _sharedLocalizer),
                new LanguageSet(userService, languageService, _localizer),
                new LanguageSetServer( guildService, languageService, _localizer),
                new LanguageList(languageService)
            ]; set => throw new NotImplementedException();
        }

        public class LanguageUsage(UserService userService, GuildService guildService, LanguageService languageService, IStringLocalizer localizer, IStringLocalizer sharedLocalizer) : Command
        {
            public override string Name { get => "usage"; set => throw new NotImplementedException(); }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                User idealUser = await userService.Get(req.UserId);
                Guild idealGuild = await guildService.Get(req.GuildId);

                Language defaultLanguage = await languageService.Get("en-US");

                string response = $"{localizer["LanguageStatusPreference"]}\n" +
                               $"{localizer["LanguageStatusGuildPreference"]}\n\n" +
                               $"__**{sharedLocalizer["RelatedCommands"]}**__\n" +
                               $"**/setlanguage** - {localizer["SetLanguageCommandDescription"]}\n" +
                               $"**/setserverlanguage** - {localizer["SetServerLanguageCommandDescription"]}\n" +
                               $"**/listlanguages** - {localizer["ListLanguagesCommandDescription"]}";

                List<string> replacements = [];

                if (idealUser != null)
                {
                    Language idealUserLanguage = await languageService.Get(idealUser.Language);

                    if (idealUserLanguage != null)
                    {
                        replacements.Add($"**{idealUserLanguage.Name}**");
                    }
                }

                if (replacements.Count == 0)
                {
                    replacements.Add($"**{defaultLanguage.Name}**");
                }

                if (idealGuild != null)
                {
                    Language idealGuildLanguage = await languageService.Get(idealGuild.Language);

                    if (idealGuildLanguage != null)
                    {
                        replacements.Add($"**{idealGuildLanguage.Name}**");
                    }
                }

                if (replacements.Count == 1)
                {
                    replacements.Add($"**{defaultLanguage.Name}**");
                }

                response = string.Format(response, [.. replacements]);

                return new CommandResponse
                {
                    OK = true,
                    Pages =
                    [
                        Utils.GetInstance().Embedify("/language", response, false)
                    ],
                    LogStatement = "/language",
                    Culture = CultureInfo.CurrentUICulture.Name
                };
            }
        }

        public class LanguageSet(UserService userService, LanguageService languageService, IStringLocalizer localizer) : Command
        {
            public override string Name { get => "set"; set => throw new NotImplementedException(); }
            public override string ArgumentsError { get => "Expected a language parameter, like `english` or `german`."; set => throw new NotImplementedException(); }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                Language idealLanguage = await languageService.Get(args[0]);

                if (idealLanguage != null)
                {
                    User idealUser = await userService.Get(req.UserId);

                    if (idealUser != null)
                    {
                        UpdateDefinition<User> update = Builders<User>.Update
                                     .Set(user => user.Language, idealLanguage.Culture);

                        await userService.Update(req.UserId, update);
                    }
                    else
                    {
                        User newUser = new()
                        {
                            UserId = req.UserId,
                            Language = idealLanguage.Culture
                        };

                        await userService.Create(newUser);
                    }

                    CultureInfo.CurrentUICulture = new CultureInfo(idealLanguage.Culture);

                    return new CommandResponse
                    {
                        OK = true,
                        Pages =
                        [
                            Utils.GetInstance().Embedify("/setlanguage", localizer["SetLanguageSuccess"], false)
                        ],
                        LogStatement = $"/setlanguage {args[0]}",
                        Culture = CultureInfo.CurrentUICulture.Name
                    };
                }

                return new CommandResponse
                {
                    OK = false,
                    Pages =
                    [
                        Utils.GetInstance().Embedify("/setlanguage", localizer["SetLanguageFailure"], true)
                    ],
                    LogStatement = "/setlanguage",
                    Culture = CultureInfo.CurrentUICulture.Name
                };
            }
        }

        public class LanguageSetServer(GuildService guildService, LanguageService languageService, IStringLocalizer localizer) : Command
        {
            public override string Name { get => "setserver"; set => throw new NotImplementedException(); }
            public override string ArgumentsError { get => "Expected a language parameter, like `english` or `german`."; set => throw new NotImplementedException(); }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                Language idealLanguage = await languageService.Get(args[0]);

                if (idealLanguage != null)
                {
                    Guild idealGuild = await guildService.Get(req.GuildId);

                    if (idealGuild != null)
                    {
                        UpdateDefinition<Guild> update = Builders<Guild>.Update
                                     .Set(guild => guild.Language, idealLanguage.Culture);

                        await guildService.Update(req.GuildId, update);
                    }
                    else
                    {
                        Guild newGuild = new()
                        {
                            GuildId = req.GuildId,
                            Language = idealLanguage.Culture,
                            IsDM = req.IsDM
                        };

                        await guildService.Create(newGuild);
                    }

                    return new CommandResponse
                    {
                        OK = true,
                        Pages =
                        [
                            Utils.GetInstance().Embedify("/setserverlanguage", localizer["SetServerLanguageSuccess"], false)
                        ],
                        LogStatement = $"/setserverlanguage {args[0]}",
                        Culture = CultureInfo.CurrentUICulture.Name
                    };
                }

                return new CommandResponse
                {
                    OK = false,
                    Pages =
                    [
                        Utils.GetInstance().Embedify("/setserverlanguage", localizer["SetServerLanguageFailure"], true)
                    ],
                    LogStatement = "/setserverlanguage",
                    Culture = CultureInfo.CurrentUICulture.Name
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
                    content.Append($"{lang.Name} `[{lang.Culture}]`\n");
                }

                return new CommandResponse
                {
                    OK = true,
                    Pages =
                    [
                        Utils.GetInstance().Embedify("/listlanguages", content.ToString(), false)
                    ],
                    LogStatement = "/listlanguages",
                    Culture = CultureInfo.CurrentUICulture.Name
                };
            }
        }
    }
}
