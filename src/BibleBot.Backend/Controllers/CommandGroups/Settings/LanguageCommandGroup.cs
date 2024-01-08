/*
* Copyright (C) 2016-2023 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BibleBot.Backend.Services;
using BibleBot.Models;
using MongoDB.Driver;

namespace BibleBot.Backend.Controllers.CommandGroups.Settings
{
    public class LanguageCommandGroup : ICommandGroup
    {
        public string Name { get; set; }
        public bool IsStaffOnly { get; set; }
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
            IsStaffOnly = false;
            Commands = new List<ICommand>
            {
                new LanguageUsage(_userService, _guildService, _languageService),
                new LanguageSet(_userService, _languageService),
                new LanguageSetServer( _guildService, _languageService),
                new LanguageList(_languageService)
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
            private readonly LanguageService _languageService;

            public LanguageSet(UserService userService, LanguageService languageService)
            {
                Name = "set";
                ArgumentsError = "Expected a language parameter, like `english` or `german`.";
                ExpectedArguments = 1;
                PermissionsRequired = null;
                BotAllowed = false;

                _userService = userService;
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
                        User newUser = new()
                        {
                            UserId = req.UserId,
                            Language = idealLanguage.ObjectName
                        };

                        await _userService.Create(newUser);
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

            private readonly GuildService _guildService;
            private readonly LanguageService _languageService;

            public LanguageSetServer(GuildService guildService, LanguageService languageService)
            {
                Name = "setserver";
                ArgumentsError = "Expected a language parameter, like `english` or `german`.";
                ExpectedArguments = 1;
                PermissionsRequired = new List<Permissions>
                {
                    Permissions.MANAGE_GUILD
                };
                BotAllowed = false;

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
                        Guild newGuild = new()
                        {
                            GuildId = req.GuildId,
                            Language = idealLanguage.ObjectName,
                            IsDM = req.IsDM
                        };

                        await _guildService.Create(newGuild);
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

            private readonly LanguageService _languageService;

            public LanguageList(LanguageService languageService)
            {
                Name = "list";
                ArgumentsError = null;
                ExpectedArguments = 0;
                PermissionsRequired = null;
                BotAllowed = false;

                _languageService = languageService;
            }

            public async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                List<Language> languages = await _languageService.Get();
                languages.Sort((x, y) => x.Name.CompareTo(y.Name));

                StringBuilder content = new();

                foreach (Language lang in languages)
                {
                    content.Append($"{lang.Name} `[{lang.ObjectName}]`\n");
                }

                return new CommandResponse
                {
                    OK = true,
                    Pages = new List<InternalEmbed>
                    {
                        Utils.GetInstance().Embedify("+language list", content.ToString(), false)
                    },
                    LogStatement = "+language list"
                };
            }
        }
    }
}
