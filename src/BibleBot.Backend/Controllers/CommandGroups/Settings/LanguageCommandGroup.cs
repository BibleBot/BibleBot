/*
* Copyright (C) 2016-2021 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BibleBot.Backend.Models;
using BibleBot.Backend.Services;
using BibleBot.Lib;

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
            DefaultCommand = Commands.Where(cmd => cmd.Name == "usage").FirstOrDefault();
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

            public IResponse ProcessCommand(Request req, List<string> args)
            {
                var idealUser = _userService.Get(req.UserId);
                var idealGuild = _guildService.Get(req.GuildId);

                var defaultLanguage = _languageService.Get("english");

                var response = "Your preferred language is set to **<language>**.\n" +
                               "The server's preferred language is set to **<glanguage>**.\n\n" +
                               "__**Subcommands**__\n" +
                               "**set** - set your preferred language\n" +
                               "**setserver** - set the server's default language (staff only)\n" +
                               "**list** - list all available languages";

                if (idealUser != null)
                {
                    var idealUserLanguage = _languageService.Get(idealUser.Language);

                    if (idealUserLanguage != null)
                    {
                        response = response.Replace("<language>", idealUserLanguage.Name);
                    }
                }

                if (idealGuild != null)
                {
                    var idealGuildLanguage = _languageService.Get(idealGuild.Language);

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
                        new Utils().Embedify("+language", response, false)
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

            public IResponse ProcessCommand(Request req, List<string> args)
            {
                var newLanguage = args[0].ToUpperInvariant();
                var idealLanguage = _languageService.Get(newLanguage);

                if (idealLanguage != null)
                {
                    var idealUser = _userService.Get(req.UserId);

                    if (idealUser != null)
                    {
                        idealUser.Language = idealLanguage.ObjectName;
                        _userService.Update(req.UserId, idealUser);
                    }
                    else
                    {
                        _userService.Create(new User
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
                            new Utils().Embedify("+language set", "Set language successfully.", false)
                        },
                        LogStatement = $"+language set {args[0]}"
                    };
                }

                return new CommandResponse
                {
                    OK = false,
                    Pages = new List<InternalEmbed>
                    {
                        new Utils().Embedify("+language set", "Failed to set language, see `+language list`.", true)
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

            public IResponse ProcessCommand(Request req, List<string> args)
            {
                var newLanguage = args[0].ToUpperInvariant();
                var idealLanguage = _languageService.Get(newLanguage);

                if (idealLanguage != null)
                {
                    var idealGuild = _guildService.Get(req.GuildId);

                    if (idealGuild != null)
                    {
                        idealGuild.Language = idealLanguage.ObjectName;
                        _guildService.Update(req.GuildId, idealGuild);
                    }
                    else
                    {
                        _guildService.Create(new Guild
                        {
                            GuildId = req.GuildId,
                            Version = "RSV",
                            Language = idealLanguage.ObjectName,
                            Prefix = "+",
                            IgnoringBrackets = "<>",
                            IsDM = req.IsDM
                        });
                    }

                    return new CommandResponse
                    {
                        OK = true,
                        Pages = new List<InternalEmbed>
                        {
                            new Utils().Embedify("+language setserver", "Set server language successfully.", false)
                        },
                        LogStatement = $"+language setserver {args[0]}"
                    };
                }

                return new CommandResponse
                {
                    OK = false,
                    Pages = new List<InternalEmbed>
                    {
                        new Utils().Embedify("+language setserver", "Failed to set server language, see `+language list`.", true)
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

            public IResponse ProcessCommand(Request req, List<string> args)
            {
                var languages = _languageService.Get();
                languages.Sort((x, y) => x.Name.CompareTo(y.Name));

                var content = "";

                foreach (Language lang in languages)
                {
                    content += $"{lang.Name} `[{lang.ObjectName}]`\n";
                }

                return new CommandResponse
                {
                    OK = true,
                    Pages = new List<InternalEmbed>
                    {
                        new Utils().Embedify("+language list", content, false)
                    },
                    LogStatement = "+language list"
                };
            }
        }
    }
}
