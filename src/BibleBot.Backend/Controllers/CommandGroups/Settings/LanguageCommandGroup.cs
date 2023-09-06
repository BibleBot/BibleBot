/*
* Copyright (C) 2016-2023 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BibleBot.Backend.Services;
using BibleBot.Models;
using MongoDB.Driver;

namespace BibleBot.Backend.Controllers.CommandGroups.Settings
{
    public class LanguageCommandGroup : ICommandGroup
    {
        public string Name { get; set; }
        public bool IsOwnerOnly { get; set; }
        public ICommand DefaultCommand { get; set; }
        public List<ICommand> Commands { get; set; }

        private readonly UserService _userService;
        private readonly GuildService _guildService;
        private readonly LanguageService _languageService;

        public LanguageCommandGroup(UserService userService, GuildService guildService, LanguageService languageService)
        {
            _userService = userService;
            _guildService = guildService;
            _languageService = languageService;

            Name = "language";
            IsOwnerOnly = false;
            Commands = new List<ICommand>
            {
                new LanguageUsage(_userService, _guildService, _languageService),
                new LanguageSet(_userService, _guildService, _languageService),
                new LanguageSetServer(_userService, _guildService, _languageService),
                new LanguageList(_userService, _guildService, _languageService)
            };
            DefaultCommand = Commands.FirstOrDefault(cmd => cmd.Name == "usage");
        }

        public class LanguageUsage : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }
            public bool BotAllowed { get; set; }

            private readonly UserService _userService;
            private readonly GuildService _guildService;
            private readonly LanguageService _languageService;

            public LanguageUsage(UserService userService, GuildService guildService, LanguageService languageService)
            {
                Name = "usage";
                ArgumentsError = null;
                ExpectedArguments = 0;
                PermissionsRequired = null;
                BotAllowed = true;

                _userService = userService;
                _guildService = guildService;
                _languageService = languageService;
            }

            public async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                User idealUser = await _userService.Get(req.UserId);
                Guild idealGuild = await _guildService.Get(req.GuildId);

                Language defaultLanguage = await _languageService.Get("english");

                string response = "Your preferred language is set to **<language>**.\n" +
                               "The server's preferred language is set to **<glanguage>**.\n\n" +
                               "__**Subcommands**__\n" +
                               "**set** - set your preferred language\n" +
                               "**setserver** - set the server's default language (staff only)\n" +
                               "**list** - list all available languages";

                if (idealUser != null)
                {
                    Language idealUserLanguage = await _languageService.Get(idealUser.Language);

                    if (idealUserLanguage != null)
                    {
                        response = response.Replace("<language>", idealUserLanguage.Name);
                    }
                }

                if (idealGuild != null)
                {
                    Language idealGuildLanguage = await _languageService.Get(idealGuild.Language);

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
                    Pages = new List<InternalEmbed>
                    {
                        Utils.GetInstance().Embedify("+language", response, false)
                    },
                    LogStatement = "+language"
                };
            }
        }

        public class LanguageSet : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }
            public bool BotAllowed { get; set; }

            private readonly UserService _userService;
            private readonly GuildService _guildService;
            private readonly LanguageService _languageService;

            public LanguageSet(UserService userService, GuildService guildService, LanguageService languageService)
            {
                Name = "set";
                ArgumentsError = "Expected a language parameter, like `english` or `german`.";
                ExpectedArguments = 1;
                PermissionsRequired = null;
                BotAllowed = false;

                _userService = userService;
                _guildService = guildService;
                _languageService = languageService;
            }

            public async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                string newLanguage = args[0].ToUpperInvariant();
                Language idealLanguage = await _languageService.Get(newLanguage);

                if (idealLanguage != null)
                {
                    User idealUser = await _userService.Get(req.UserId);

                    if (idealUser != null)
                    {
                        UpdateDefinition<User> update = Builders<User>.Update
                                     .Set(user => user.Language, idealLanguage.ObjectName);

                        await _userService.Update(req.UserId, update);
                    }
                    else
                    {
                        await _userService.Create(new User
                        {
                            UserId = req.UserId,
                            Version = "RSV",
                            InputMethod = "default",
                            Language = idealLanguage.ObjectName,
                            TitlesEnabled = true,
                            VerseNumbersEnabled = true,
                            PaginationEnabled = false,
                            DisplayStyle = "embed"
                        });
                    }

                    return new CommandResponse
                    {
                        OK = true,
                        Pages = new List<InternalEmbed>
                        {
                            Utils.GetInstance().Embedify("+language set", "Set language successfully.", false)
                        },
                        LogStatement = $"+language set {args[0]}"
                    };
                }

                return new CommandResponse
                {
                    OK = false,
                    Pages = new List<InternalEmbed>
                    {
                        Utils.GetInstance().Embedify("+language set", "Failed to set language, see `+language list`.", true)
                    },
                    LogStatement = "+language set"
                };
            }
        }

        public class LanguageSetServer : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }
            public bool BotAllowed { get; set; }

            private readonly UserService _userService;
            private readonly GuildService _guildService;
            private readonly LanguageService _languageService;

            public LanguageSetServer(UserService userService, GuildService guildService, LanguageService languageService)
            {
                Name = "setserver";
                ArgumentsError = "Expected a language parameter, like `english` or `german`.";
                ExpectedArguments = 1;
                PermissionsRequired = new List<Permissions>
                {
                    Permissions.MANAGE_GUILD
                };
                BotAllowed = false;

                _userService = userService;
                _guildService = guildService;
                _languageService = languageService;
            }

            public async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                string newLanguage = args[0].ToUpperInvariant();
                Language idealLanguage = await _languageService.Get(newLanguage);

                if (idealLanguage != null)
                {
                    Guild idealGuild = await _guildService.Get(req.GuildId);

                    if (idealGuild != null)
                    {
                        UpdateDefinition<Guild> update = Builders<Guild>.Update
                                     .Set(guild => guild.Language, idealLanguage.ObjectName);

                        await _guildService.Update(req.GuildId, update);
                    }
                    else
                    {
                        await _guildService.Create(new Guild
                        {
                            GuildId = req.GuildId,
                            Version = "RSV",
                            Language = idealLanguage.ObjectName,
                            Prefix = "+",
                            DisplayStyle = "embed",
                            IgnoringBrackets = "<>",
                            IsDM = req.IsDM
                        });
                    }

                    return new CommandResponse
                    {
                        OK = true,
                        Pages = new List<InternalEmbed>
                        {
                            Utils.GetInstance().Embedify("+language setserver", "Set server language successfully.", false)
                        },
                        LogStatement = $"+language setserver {args[0]}"
                    };
                }

                return new CommandResponse
                {
                    OK = false,
                    Pages = new List<InternalEmbed>
                    {
                        Utils.GetInstance().Embedify("+language setserver", "Failed to set server language, see `+language list`.", true)
                    },
                    LogStatement = "+language setserver"
                };
            }
        }

        public class LanguageList : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }
            public bool BotAllowed { get; set; }

            private readonly UserService _userService;
            private readonly GuildService _guildService;
            private readonly LanguageService _languageService;

            public LanguageList(UserService userService, GuildService guildService, LanguageService languageService)
            {
                Name = "list";
                ArgumentsError = null;
                ExpectedArguments = 0;
                PermissionsRequired = null;
                BotAllowed = false;

                _userService = userService;
                _guildService = guildService;
                _languageService = languageService;
            }

            public async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                List<Language> languages = await _languageService.Get();
                languages.Sort((x, y) => x.Name.CompareTo(y.Name));

                string content = "";

                foreach (Language lang in languages)
                {
                    content += $"{lang.Name} `[{lang.ObjectName}]`\n";
                }

                return new CommandResponse
                {
                    OK = true,
                    Pages = new List<InternalEmbed>
                    {
                        Utils.GetInstance().Embedify("+language list", content, false)
                    },
                    LogStatement = "+language list"
                };
            }
        }
    }
}
