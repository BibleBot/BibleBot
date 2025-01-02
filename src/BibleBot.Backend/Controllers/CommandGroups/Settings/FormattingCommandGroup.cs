/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
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
    public class FormattingCommandGroup : ICommandGroup
    {
        public string Name { get; set; }
        public bool IsStaffOnly { get; set; }
        public ICommand DefaultCommand { get; set; }
        public List<ICommand> Commands { get; set; }

        private readonly UserService _userService;
        private readonly GuildService _guildService;

        public FormattingCommandGroup(UserService userService, GuildService guildService)
        {
            _userService = userService;
            _guildService = guildService;

            Name = "formatting";
            IsStaffOnly = false;
            Commands =
            [
                new FormattingUsage(_userService, _guildService),
                new FormattingSetVerseNumbers(_userService),
                new FormattingSetTitles(_userService),
                new FormattingSetPagination(_userService),
                new FormattingSetDisplayStyle(_userService),
                new FormattingSetServerDisplayStyle(_guildService),
                new FormattingSetIgnoringBrackets(_guildService)
            ];
            DefaultCommand = Commands.FirstOrDefault(cmd => cmd.Name == "usage");
        }

        public class FormattingUsage(UserService userService, GuildService guildService) : ICommand
        {
            public string Name { get; set; } = "usage";
            public string ArgumentsError { get; set; } = null;
            public int ExpectedArguments { get; set; } = 0;
            public List<Permissions> PermissionsRequired { get; set; } = null;
            public bool BotAllowed { get; set; } = true;

            private readonly UserService _userService = userService;
            private readonly GuildService _guildService = guildService;

            public async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                User idealUser = await _userService.Get(req.UserId);
                Guild idealGuild = await _guildService.Get(req.GuildId);

                string response = "<vncheck> Verse numbers are **<verseNumbers>**.\n" +
                               "<titlecheck> Titles are **<titles>**.\n" +
                               "<vpcheck> Verse pagination is **<pagination>**.\n" +
                               "Your preferred display style is set to **`<displayStyle>`**.\n\n" +
                               "The server's preferred display style is set to **`<serverDisplayStyle>`**.\n" +
                               "This bot will ignore verses in this server surrounded by **`<>`**<ignoringBrackets>.\n\n" +
                               "__**Related Commands**__\n" +
                               "**/setversenumbers** - enable or disable verse numbers\n" +
                               "**/settitles** - enable or disable titles\n" +
                               "**/setpagination** - enable or disable verse pagination\n" +
                               "**/setdisplay** - set your preferred display style\n" +
                               "**/setserverdisplay** - set the server's preferred display style (staff only)\n" +
                               "**/setbrackets** - set the bot's ignoring brackets for this server (staff only)";

                if (idealUser != null)
                {
                    response = idealUser.VerseNumbersEnabled
                        ? response.Replace("<vncheck>", "<:checkmark:1132080313854603295>").Replace("<verseNumbers>", "enabled")
                        : response.Replace("<vncheck>", "<:xmark:1132080327557398599>").Replace("<verseNumbers>", "disabled");

                    response = idealUser.TitlesEnabled
                        ? response.Replace("<titlecheck>", "<:checkmark:1132080313854603295>").Replace("<titles>", "enabled")
                        : response.Replace("<titlecheck>", "<:xmark:1132080327557398599>").Replace("<titles>", "disabled");

                    response = idealUser.PaginationEnabled
                        ? response.Replace("<vpcheck>", "<:checkmark:1132080313854603295>").Replace("<pagination>", "enabled")
                        : response.Replace("<vpcheck>", "<:xmark:1132080327557398599>").Replace("<pagination>", "disabled");

                    response = response.Replace("<displayStyle>", idealUser.DisplayStyle);
                }

                if (idealGuild != null)
                {
                    response = response.Replace("<serverDisplayStyle>", idealGuild.DisplayStyle ?? "embed");
                    response = response.Replace("<ignoringBrackets>", idealGuild.IgnoringBrackets != "<>" ? $" or **`{idealGuild.IgnoringBrackets}`**" : "");
                }

                response = response.Replace("<vncheck>", "<:checkmark:1132080313854603295>").Replace("<verseNumbers>", "enabled");
                response = response.Replace("<titlecheck>", "<:checkmark:1132080313854603295>").Replace("<titles>", "enabled");
                response = response.Replace("<vpcheck>", "<:checkmark:1132080313854603295>").Replace("<pagination>", "enabled");
                response = response.Replace("<displayStyle>", "embed");
                response = response.Replace("<serverDisplayStyle>", "embed");
                response = response.Replace("<ignoringBrackets>", "");

                return new CommandResponse
                {
                    OK = true,
                    Pages =
                    [
                        Utils.GetInstance().Embedify("/formatting", response, false)
                    ],
                    LogStatement = "/formatting"
                };
            }
        }

        public class FormattingSetVerseNumbers(UserService userService) : ICommand
        {
            public string Name { get; set; } = "setversenumbers";
            public string ArgumentsError { get; set; } = "Expected an `enable` or `disable` parameter.";
            public int ExpectedArguments { get; set; } = 1;
            public List<Permissions> PermissionsRequired { get; set; } = null;
            public bool BotAllowed { get; set; } = false;

            private readonly UserService _userService = userService;

            public async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                if (args[0] is not "enable" and not "disable")
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages =
                        [
                            Utils.GetInstance().Embedify("/setversenumbers", "Expected an `enable` or `disable` parameter.", true)
                        ],
                        LogStatement = "/setversenumbers"
                    };
                }

                User idealUser = await _userService.Get(req.UserId);

                if (idealUser != null)
                {
                    UpdateDefinition<User> update = Builders<User>.Update
                                 .Set(user => user.VerseNumbersEnabled, args[0] is "enable" and not "disable");

                    await _userService.Update(req.UserId, update);
                }
                else
                {
                    User newUser = new()
                    {
                        UserId = req.UserId,
                        VerseNumbersEnabled = args[0] is "enable" and not "disable"
                    };

                    await _userService.Create(newUser);
                }

                return new CommandResponse
                {
                    OK = true,
                    Pages =
                    [
                        Utils.GetInstance().Embedify("/setversenumbers", "Set verse numbers successfully.", false)
                    ],
                    LogStatement = $"/setversenumbers {args[0]}"
                };
            }
        }

        public class FormattingSetTitles(UserService userService) : ICommand
        {
            public string Name { get; set; } = "settitles";
            public string ArgumentsError { get; set; } = "Expected an `enable` or `disable` parameter.";
            public int ExpectedArguments { get; set; } = 1;
            public List<Permissions> PermissionsRequired { get; set; } = null;
            public bool BotAllowed { get; set; } = false;

            private readonly UserService _userService = userService;

            public async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                if (args[0] is not "enable" and not "disable")
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages =
                        [
                            Utils.GetInstance().Embedify("/settitles", "Expected an `enable` or `disable` parameter.", true)
                        ],
                        LogStatement = "/settitles"
                    };
                }

                User idealUser = await _userService.Get(req.UserId);

                if (idealUser != null)
                {
                    UpdateDefinition<User> update = Builders<User>.Update
                                 .Set(user => user.TitlesEnabled, args[0] is "enable" and not "disable");

                    await _userService.Update(req.UserId, update);
                }
                else
                {
                    User newUser = new()
                    {
                        UserId = req.UserId,
                        TitlesEnabled = args[0] is "enable" and not "disable"
                    };

                    await _userService.Create(newUser);
                }

                return new CommandResponse
                {
                    OK = true,
                    Pages =
                    [
                        Utils.GetInstance().Embedify("/settitles", "Set titles successfully.", false)
                    ],
                    LogStatement = $"/settitles {args[0]}"
                };
            }
        }

        public class FormattingSetPagination(UserService userService) : ICommand
        {
            public string Name { get; set; } = "setpagination";
            public string ArgumentsError { get; set; } = "Expected an `enable` or `disable` parameter.";
            public int ExpectedArguments { get; set; } = 1;
            public List<Permissions> PermissionsRequired { get; set; } = null;
            public bool BotAllowed { get; set; } = false;

            private readonly UserService _userService = userService;

            public async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                if (args[0] is not "enable" and not "disable")
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages =
                        [
                            Utils.GetInstance().Embedify("/setpagination", "Expected an `enable` or `disable` parameter.", true)
                        ],
                        LogStatement = "/setpagination"
                    };
                }

                User idealUser = await _userService.Get(req.UserId);

                if (idealUser != null)
                {
                    UpdateDefinition<User> update = Builders<User>.Update
                                 .Set(user => user.PaginationEnabled, args[0] is "enable" and not "disable");

                    await _userService.Update(req.UserId, update);
                }
                else
                {
                    User newUser = new()
                    {
                        UserId = req.UserId,
                        PaginationEnabled = args[0] is "enable" and not "disable"
                    };

                    await _userService.Create(newUser);
                }

                return new CommandResponse
                {
                    OK = true,
                    Pages =
                    [
                        Utils.GetInstance().Embedify("/setpagination", "Set pagination successfully.", false)
                    ],
                    LogStatement = $"/setpagination {args[0]}"
                };
            }
        }

        public class FormattingSetDisplayStyle(UserService userService) : ICommand
        {
            public string Name { get; set; } = "setdisplay";
            public string ArgumentsError { get; set; } = "Expected a parameter of `embed`, `code`, or `blockquote`.";
            public int ExpectedArguments { get; set; } = 1;
            public List<Permissions> PermissionsRequired { get; set; } = null;
            public bool BotAllowed { get; set; } = false;

            private readonly UserService _userService = userService;

            public async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                if (args[0] is not ("embed" or "code" or "blockquote"))
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages =
                        [
                            Utils.GetInstance().Embedify("/setdisplay", "You may choose between `embed`, `code`, or `blockquote`.", true)
                        ],
                        LogStatement = "/setdisplay"
                    };
                }

                User idealUser = await _userService.Get(req.UserId);

                if (idealUser != null)
                {
                    UpdateDefinition<User> update = Builders<User>.Update
                                 .Set(user => user.DisplayStyle, args[0]);

                    await _userService.Update(req.UserId, update);
                }
                else
                {
                    User newUser = new()
                    {
                        UserId = req.UserId,
                        DisplayStyle = args[0]
                    };

                    await _userService.Create(newUser);
                }

                return new CommandResponse
                {
                    OK = true,
                    Pages =
                    [
                        Utils.GetInstance().Embedify("/setdisplay", "Set display style successfully.", false)
                    ],
                    LogStatement = $"/setdisplay {args[0]}"
                };
            }
        }

        public class FormattingSetServerDisplayStyle(GuildService guildService) : ICommand
        {
            public string Name { get; set; } = "setserverdisplay";
            public string ArgumentsError { get; set; } = "Expected a parameter of `embed`, `code`, or `blockquote`.";
            public int ExpectedArguments { get; set; } = 1;
            public List<Permissions> PermissionsRequired { get; set; } =
                [
                    Permissions.MANAGE_GUILD
                ];
            public bool BotAllowed { get; set; } = false;

            private readonly GuildService _guildService = guildService;

            public async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                if (args[0] is not ("embed" or "code" or "blockquote"))
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages =
                        [
                            Utils.GetInstance().Embedify("/setserverdisplay", "You may choose between `embed`, `code`, or `blockquote`.", true)
                        ],
                        LogStatement = "/setserverdisplay"
                    };
                }

                Guild idealGuild = await _guildService.Get(req.GuildId);

                if (idealGuild != null)
                {
                    UpdateDefinition<Guild> update = Builders<Guild>.Update
                                 .Set(guild => guild.DisplayStyle, args[0]);

                    await _guildService.Update(req.GuildId, update);
                }
                else
                {
                    Guild newGuild = new()
                    {
                        GuildId = req.GuildId,
                        DisplayStyle = args[0],
                        IsDM = req.IsDM
                    };

                    await _guildService.Create(newGuild);
                }

                return new CommandResponse
                {
                    OK = true,
                    Pages =
                    [
                        Utils.GetInstance().Embedify("/setserverdisplay", "Set server display style successfully.", false)
                    ],
                    LogStatement = $"/setserverdisplay {args[0]}"
                };
            }
        }

        public class FormattingSetIgnoringBrackets(GuildService guildService) : ICommand
        {
            public string Name { get; set; } = "setbrackets";
            public string ArgumentsError { get; set; } = "Expected a parameter with two characters, that must be `<>`, `[]`, `{}`, or `()`.";
            public int ExpectedArguments { get; set; } = 1;
            public List<Permissions> PermissionsRequired { get; set; } =
                [
                    Permissions.MANAGE_GUILD
                ];
            public bool BotAllowed { get; set; } = false;

            private readonly GuildService _guildService = guildService;

            public async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                List<string> acceptableBrackets = ["<>", "[]", "{}", "()"];

                if (args[0].Length != 2)
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages =
                        [
                            Utils.GetInstance().Embedify("/setbrackets", "The brackets can only be two characters.", true)
                        ],
                        LogStatement = "/setbrackets"
                    };
                }
                else if (!acceptableBrackets.Contains(args[0]))
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages =
                        [
                            Utils.GetInstance().Embedify("/setbrackets", "The brackets can only be set to `<>`, `[]`, `{}`, or `()`.", true)
                        ],
                        LogStatement = "/setbrackets"
                    };
                }


                Guild idealGuild = await _guildService.Get(req.GuildId);

                if (idealGuild != null)
                {
                    UpdateDefinition<Guild> update = Builders<Guild>.Update
                                 .Set(guild => guild.IgnoringBrackets, args[0]);

                    await _guildService.Update(req.GuildId, update);
                }
                else
                {
                    Guild newGuild = new()
                    {
                        GuildId = req.GuildId,
                        IgnoringBrackets = args[0],
                        IsDM = req.IsDM
                    };

                    await _guildService.Create(newGuild);
                }

                return new CommandResponse
                {
                    OK = true,
                    Pages =
                    [
                        Utils.GetInstance().Embedify("/setbrackets", "Set brackets successfully.", false)
                    ],
                    LogStatement = $"/setbrackets {args[0]}"
                };
            }
        }
    }
}
