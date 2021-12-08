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
    public class FormattingCommandGroup : ICommandGroup
    {
        public string Name { get; set; }
        public bool IsOwnerOnly { get; set; }
        public ICommand DefaultCommand { get; set; }
        public List<ICommand> Commands { get; set; }

        private readonly UserService _userService;
        private readonly GuildService _guildService;

        public FormattingCommandGroup(UserService userService, GuildService guildService)
        {
            _userService = userService;
            _guildService = guildService;

            Name = "formatting";
            IsOwnerOnly = false;
            Commands = new List<ICommand>
            {
                new FormattingUsage(_userService, _guildService),
                new FormattingSetVerseNumbers(_userService, _guildService),
                new FormattingSetTitles(_userService, _guildService),
                new FormattingSetPagination(_userService, _guildService),
                new FormattingSetDisplayStyle(_userService, _guildService),
                new FormattingSetPrefix(_userService, _guildService),
                new FormattingSetIgnoringBrackets(_userService, _guildService)
            };
            DefaultCommand = Commands.Where(cmd => cmd.Name == "usage").FirstOrDefault();
        }

        public class FormattingUsage : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }
            public bool BotAllowed { get; set; }

            private readonly UserService _userService;
            private readonly GuildService _guildService;

            public FormattingUsage(UserService userService, GuildService guildService)
            {
                Name = "usage";
                ArgumentsError = null;
                ExpectedArguments = 0;
                PermissionsRequired = null;
                BotAllowed = true;

                _userService = userService;
                _guildService = guildService;
            }

            public IResponse ProcessCommand(Request req, List<string> args)
            {
                var idealUser = _userService.Get(req.UserId);
                var idealGuild = _guildService.Get(req.GuildId);

                var response = "Verse numbers are **<verseNumbers>**.\n" +
                               "Titles are **<titles>**.\n" +
                               "Verse pagination is **<pagination>**.\n" +
                               "Your display style is set to **`<displayStyle>`**.\n\n" +
                               "The bot's prefix for this server is **`<prefix>`**.\n" +
                               "This bot will ignore verses in this server surrounded by **`<ignoringBrackets>`**.\n\n" +
                               "__**Subcommands**__\n" +
                               "**setversenumbers** - enable or disable verse numbers\n" +
                               "**settitles** - enable or disable titles\n" +
                               "**setpagination** - enable or disable verse pagination\n" +
                               "**setdisplay** - set your preferred display style\n" +
                               "**setprefix** - set the bot's command prefix for this server (staff only)\n" +
                               "**setbrackets** - set the bot's ignoring brackets for this server (staff only)";

                if (idealUser != null)
                {
                    response = response.Replace("<verseNumbers>", idealUser.VerseNumbersEnabled ? "enabled" : "disabled");
                    response = response.Replace("<titles>", idealUser.TitlesEnabled ? "enabled" : "disabled");
                    response = response.Replace("<pagination>", idealUser.PaginationEnabled ? "enabled" : "disabled");
                    response = response.Replace("<displayStyle>", idealUser.DisplayStyle);
                }

                if (idealGuild != null)
                {
                    response = response.Replace("<prefix>", idealGuild.Prefix);
                    response = response.Replace("<ignoringBrackets>", idealGuild.IgnoringBrackets);
                }

                response = response.Replace("<verseNumbers>", "enabled");
                response = response.Replace("<titles>", "enabled");
                response = response.Replace("<pagination>", "enabled");
                response = response.Replace("<displayStyle>", "embed");
                response = response.Replace("<prefix>", "+");
                response = response.Replace("<ignoringBrackets>", "<>");

                return new CommandResponse
                {
                    OK = true,
                    Pages = new List<InternalEmbed>
                    {
                        new Utils().Embedify("+formatting", response, false)
                    },
                    LogStatement = "+formatting"
                };
            }
        }

        public class FormattingSetVerseNumbers : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }
            public bool BotAllowed { get; set; }

            private readonly UserService _userService;
            private readonly GuildService _guildService;

            public FormattingSetVerseNumbers(UserService userService, GuildService guildService)
            {
                Name = "setversenumbers";
                ArgumentsError = "Expected an `enable` or `disable` parameter.";
                ExpectedArguments = 1;
                PermissionsRequired = null;
                BotAllowed = false;

                _userService = userService;
                _guildService = guildService;
            }

            public IResponse ProcessCommand(Request req, List<string> args)
            {
                if (args[0] != "enable" && args[0] != "disable")
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages = new List<InternalEmbed>
                        {
                            new Utils().Embedify("+formatting setversenumbers", "Expected an `enable` or `disable` parameter.", true)
                        },
                        LogStatement = "+formatting setversenumbers"
                    };
                }

                var idealUser = _userService.Get(req.UserId);

                if (idealUser != null)
                {
                    idealUser.VerseNumbersEnabled = (args[0] == "enable" && args[0] != "disable");
                    _userService.Update(req.UserId, idealUser);
                }
                else
                {
                    _userService.Create(new User
                    {
                        UserId = req.UserId,
                        Version = "RSV",
                        InputMethod = "default",
                        Language = "english",
                        TitlesEnabled = true,
                        VerseNumbersEnabled = (args[0] == "enable" && args[0] != "disable"),
                        PaginationEnabled = false,
                        DisplayStyle = "embed"
                    });
                }

                return new CommandResponse
                {
                    OK = true,
                    Pages = new List<InternalEmbed>
                    {
                        new Utils().Embedify("+formatting setversenumbers", "Set verse numbers successfully.", false)
                    },
                    LogStatement = $"+formatting setversenumbers {args[0]}"
                };
            }
        }

        public class FormattingSetTitles : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }
            public bool BotAllowed { get; set; }

            private readonly UserService _userService;
            private readonly GuildService _guildService;

            public FormattingSetTitles(UserService userService, GuildService guildService)
            {
                Name = "settitles";
                ArgumentsError = "Expected an `enable` or `disable` parameter.";
                ExpectedArguments = 1;
                PermissionsRequired = null;
                BotAllowed = false;

                _userService = userService;
                _guildService = guildService;
            }

            public IResponse ProcessCommand(Request req, List<string> args)
            {
                if (args[0] != "enable" && args[0] != "disable")
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages = new List<InternalEmbed>
                        {
                            new Utils().Embedify("+formatting settitles", "Expected an `enable` or `disable` parameter.", true)
                        },
                        LogStatement = "+formatting settitles"
                    };
                }

                var idealUser = _userService.Get(req.UserId);

                if (idealUser != null)
                {
                    idealUser.TitlesEnabled = (args[0] == "enable" && args[0] != "disable");
                    _userService.Update(req.UserId, idealUser);
                }
                else
                {
                    _userService.Create(new User
                    {
                        UserId = req.UserId,
                        Version = "RSV",
                        InputMethod = "default",
                        Language = "english",
                        TitlesEnabled = (args[0] == "enable" && args[0] != "disable"),
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
                        new Utils().Embedify("+formatting settitles", "Set titles successfully.", false)
                    },
                    LogStatement = $"+formatting settitles {args[0]}"
                };
            }
        }

        public class FormattingSetPagination : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }
            public bool BotAllowed { get; set; }

            private readonly UserService _userService;
            private readonly GuildService _guildService;

            public FormattingSetPagination(UserService userService, GuildService guildService)
            {
                Name = "setpagination";
                ArgumentsError = "Expected an `enable` or `disable` parameter.";
                ExpectedArguments = 1;
                PermissionsRequired = null;
                BotAllowed = false;

                _userService = userService;
                _guildService = guildService;
            }

            public IResponse ProcessCommand(Request req, List<string> args)
            {
                if (args[0] != "enable" && args[0] != "disable")
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages = new List<InternalEmbed>
                        {
                            new Utils().Embedify("+formatting setpagination", "Expected an `enable` or `disable` parameter.", true)
                        },
                        LogStatement = "+formatting setpagination"
                    };
                }

                var idealUser = _userService.Get(req.UserId);

                if (idealUser != null)
                {
                    idealUser.PaginationEnabled = (args[0] == "enable" && args[0] != "disable");
                    _userService.Update(req.UserId, idealUser);
                }
                else
                {
                    _userService.Create(new User
                    {
                        UserId = req.UserId,
                        Version = "RSV",
                        InputMethod = "default",
                        Language = "english",
                        TitlesEnabled = true,
                        VerseNumbersEnabled = true,
                        PaginationEnabled = (args[0] == "enable" && args[0] != "disable"),
                        DisplayStyle = "embed"
                    });
                }

                return new CommandResponse
                {
                    OK = true,
                    Pages = new List<InternalEmbed>
                    {
                        new Utils().Embedify("+formatting setpagination", "Set titles successfully.", false)
                    },
                    LogStatement = $"+formatting setpagination {args[0]}"
                };
            }
        }

        public class FormattingSetDisplayStyle : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }
            public bool BotAllowed { get; set; }

            private readonly UserService _userService;
            private readonly GuildService _guildService;

            public FormattingSetDisplayStyle(UserService userService, GuildService guildService)
            {
                Name = "setdisplay";
                ArgumentsError = "Expected a parameter of `embed`, `code`, or `blockquote`.";
                ExpectedArguments = 1;
                PermissionsRequired = null;
                BotAllowed = false;

                _userService = userService;
                _guildService = guildService;
            }

            public IResponse ProcessCommand(Request req, List<string> args)
            {
                if (!(args[0] == "embed" || args[0] == "code" || args[0] == "blockquote"))
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages = new List<InternalEmbed>
                        {
                            new Utils().Embedify("+formatting setdisplay", "You may choose between `embed`, `code`, or `blockquote`.", true)
                        },
                        LogStatement = "+formatting setdisplay"
                    };
                }

                var idealUser = _userService.Get(req.UserId);

                if (idealUser != null)
                {
                    idealUser.DisplayStyle = args[0];
                    _userService.Update(req.UserId, idealUser);
                }
                else
                {
                    _userService.Create(new User
                    {
                        UserId = req.UserId,
                        Version = "RSV",
                        InputMethod = "default",
                        Language = "english",
                        TitlesEnabled = true,
                        VerseNumbersEnabled = true,
                        PaginationEnabled = false,
                        DisplayStyle = args[0]
                    });
                }

                return new CommandResponse
                {
                    OK = true,
                    Pages = new List<InternalEmbed>
                    {
                        new Utils().Embedify("+formatting setdisplay", "Set display style successfully.", false)
                    },
                    LogStatement = $"+formatting setdisplay {args[0]}"
                };
            }
        }

        public class FormattingSetPrefix : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }
            public bool BotAllowed { get; set; }

            private readonly UserService _userService;
            private readonly GuildService _guildService;

            public FormattingSetPrefix(UserService userService, GuildService guildService)
            {
                Name = "setprefix";
                ArgumentsError = "Expected a parameter with a single character, that must be `+`, `-`, `!`, `=`, `$`, `%`, `^`, `*`, `.`, `,`, `?`, `~`, or `|`.";
                ExpectedArguments = 1;
                PermissionsRequired = new List<Permissions>
                {
                    Permissions.MANAGE_GUILD
                };
                BotAllowed = false;

                _userService = userService;
                _guildService = guildService;
            }

            public IResponse ProcessCommand(Request req, List<string> args)
            {
                var acceptablePrefixes = new List<string> { "+", "-", "!", "=", "$", "%", "^", "*", ".", ",", "?", "~", "|" };

                if (args[0].Length != 1)
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages = new List<InternalEmbed>
                        {
                            new Utils().Embedify("+formatting setprefix", "The prefix can only be one character.", true)
                        },
                        LogStatement = "+formatting setprefix"
                    };
                }
                else if (!acceptablePrefixes.Contains(args[0]))
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages = new List<InternalEmbed>
                        {
                            new Utils().Embedify("+formatting setprefix", "The prefix can only be set to `+`, `-`, `!`, `=`, `$`, `%`, `^`, `*`, `.`, `,`, `?`, `~`, or `|`.", true)
                        },
                        LogStatement = "+formatting setprefix"
                    };
                }


                var idealGuild = _guildService.Get(req.GuildId);

                if (idealGuild != null)
                {
                    idealGuild.Prefix = args[0];
                    _guildService.Update(req.GuildId, idealGuild);
                }
                else
                {
                    _guildService.Create(new Guild
                    {
                        GuildId = req.GuildId,
                        Version = "RSV",
                        Language = "english",
                        Prefix = args[0],
                        IgnoringBrackets = "<>",
                        IsDM = req.IsDM
                    });
                }

                return new CommandResponse
                {
                    OK = true,
                    Pages = new List<InternalEmbed>
                    {
                        new Utils().Embedify("+formatting setprefix", "Set prefix successfully.", false)
                    },
                    LogStatement = $"+formatting setprefix {args[0]}"
                };
            }
        }

        public class FormattingSetIgnoringBrackets : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }
            public bool BotAllowed { get; set; }

            private readonly UserService _userService;
            private readonly GuildService _guildService;

            public FormattingSetIgnoringBrackets(UserService userService, GuildService guildService)
            {
                Name = "setbrackets";
                ArgumentsError = "Expected a parameter with two characters, that must be `<>`, `[]`, `{}`, or `()`.";
                ExpectedArguments = 1;
                PermissionsRequired = new List<Permissions>
                {
                    Permissions.MANAGE_GUILD
                };
                BotAllowed = false;

                _userService = userService;
                _guildService = guildService;
            }

            public IResponse ProcessCommand(Request req, List<string> args)
            {
                var acceptableBrackets = new List<string> { "<>", "[]", "{}", "()" };

                if (args[0].Length != 2)
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages = new List<InternalEmbed>
                        {
                            new Utils().Embedify("+formatting setbrackets", "The brackets can only be two characters.", true)
                        },
                        LogStatement = "+formatting setbrackets"
                    };
                }
                else if (!acceptableBrackets.Contains(args[0]))
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages = new List<InternalEmbed>
                        {
                            new Utils().Embedify("+formatting setbrackets", "The brackets can only be set to `<>`, `[]`, `{}`, or `()`.", true)
                        },
                        LogStatement = "+formatting setbrackets"
                    };
                }


                var idealGuild = _guildService.Get(req.GuildId);

                if (idealGuild != null)
                {
                    idealGuild.IgnoringBrackets = args[0];
                    _guildService.Update(req.GuildId, idealGuild);
                }
                else
                {
                    _guildService.Create(new Guild
                    {
                        GuildId = req.GuildId,
                        Version = "RSV",
                        Language = "english",
                        Prefix = "+",
                        IgnoringBrackets = args[0],
                        IsDM = req.IsDM
                    });
                }

                return new CommandResponse
                {
                    OK = true,
                    Pages = new List<InternalEmbed>
                    {
                        new Utils().Embedify("+formatting setbrackets", "Set brackets successfully.", false)
                    },
                    LogStatement = $"+formatting setbrackets {args[0]}"
                };
            }
        }
    }
}
