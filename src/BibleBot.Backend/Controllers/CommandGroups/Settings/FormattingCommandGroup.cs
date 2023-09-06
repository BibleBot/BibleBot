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
                new FormattingSetVerseNumbers(_userService),
                new FormattingSetTitles(_userService),
                new FormattingSetPagination(_userService),
                new FormattingSetDisplayStyle(_userService),
                new FormattingSetServerDisplayStyle(_guildService),
                new FormattingSetIgnoringBrackets(_guildService)
            };
            DefaultCommand = Commands.FirstOrDefault(cmd => cmd.Name == "usage");
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
                    Pages = new List<InternalEmbed>
                    {
                        Utils.GetInstance().Embedify("/formatting", response, false)
                    },
                    LogStatement = "/formatting"
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

            public FormattingSetVerseNumbers(UserService userService)
            {
                Name = "setversenumbers";
                ArgumentsError = "Expected an `enable` or `disable` parameter.";
                ExpectedArguments = 1;
                PermissionsRequired = null;
                BotAllowed = false;

                _userService = userService;
            }

            public async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                if (args[0] is not "enable" and not "disable")
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages = new List<InternalEmbed>
                        {
                            Utils.GetInstance().Embedify("/setversenumbers", "Expected an `enable` or `disable` parameter.", true)
                        },
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
                    await _userService.Create(new User
                    {
                        UserId = req.UserId,
                        Version = "RSV",
                        InputMethod = "default",
                        Language = "english_us",
                        TitlesEnabled = true,
                        VerseNumbersEnabled = args[0] is "enable" and not "disable",
                        PaginationEnabled = false,
                        DisplayStyle = "embed"
                    });
                }

                return new CommandResponse
                {
                    OK = true,
                    Pages = new List<InternalEmbed>
                    {
                        Utils.GetInstance().Embedify("/setversenumbers", "Set verse numbers successfully.", false)
                    },
                    LogStatement = $"/setversenumbers {args[0]}"
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

            public FormattingSetTitles(UserService userService)
            {
                Name = "settitles";
                ArgumentsError = "Expected an `enable` or `disable` parameter.";
                ExpectedArguments = 1;
                PermissionsRequired = null;
                BotAllowed = false;

                _userService = userService;
            }

            public async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                if (args[0] is not "enable" and not "disable")
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages = new List<InternalEmbed>
                        {
                            Utils.GetInstance().Embedify("/settitles", "Expected an `enable` or `disable` parameter.", true)
                        },
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
                    await _userService.Create(new User
                    {
                        UserId = req.UserId,
                        Version = "RSV",
                        InputMethod = "default",
                        Language = "english_us",
                        TitlesEnabled = args[0] is "enable" and not "disable",
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
                        Utils.GetInstance().Embedify("/settitles", "Set titles successfully.", false)
                    },
                    LogStatement = $"/settitles {args[0]}"
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

            public FormattingSetPagination(UserService userService)
            {
                Name = "setpagination";
                ArgumentsError = "Expected an `enable` or `disable` parameter.";
                ExpectedArguments = 1;
                PermissionsRequired = null;
                BotAllowed = false;

                _userService = userService;
            }

            public async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                if (args[0] is not "enable" and not "disable")
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages = new List<InternalEmbed>
                        {
                            Utils.GetInstance().Embedify("/setpagination", "Expected an `enable` or `disable` parameter.", true)
                        },
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
                    await _userService.Create(new User
                    {
                        UserId = req.UserId,
                        Version = "RSV",
                        InputMethod = "default",
                        Language = "english_us",
                        TitlesEnabled = true,
                        VerseNumbersEnabled = true,
                        PaginationEnabled = args[0] is "enable" and not "disable",
                        DisplayStyle = "embed"
                    });
                }

                return new CommandResponse
                {
                    OK = true,
                    Pages = new List<InternalEmbed>
                    {
                        Utils.GetInstance().Embedify("/setpagination", "Set pagination successfully.", false)
                    },
                    LogStatement = $"/setpagination {args[0]}"
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

            public FormattingSetDisplayStyle(UserService userService)
            {
                Name = "setdisplay";
                ArgumentsError = "Expected a parameter of `embed`, `code`, or `blockquote`.";
                ExpectedArguments = 1;
                PermissionsRequired = null;
                BotAllowed = false;

                _userService = userService;
            }

            public async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                if (args[0] is not ("embed" or "code" or "blockquote"))
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages = new List<InternalEmbed>
                        {
                            Utils.GetInstance().Embedify("/setdisplay", "You may choose between `embed`, `code`, or `blockquote`.", true)
                        },
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
                    await _userService.Create(new User
                    {
                        UserId = req.UserId,
                        Version = "RSV",
                        InputMethod = "default",
                        Language = "english_us",
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
                        Utils.GetInstance().Embedify("/setdisplay", "Set display style successfully.", false)
                    },
                    LogStatement = $"/setdisplay {args[0]}"
                };
            }
        }

        public class FormattingSetServerDisplayStyle : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }
            public bool BotAllowed { get; set; }

            private readonly GuildService _guildService;

            public FormattingSetServerDisplayStyle(GuildService guildService)
            {
                Name = "setserverdisplay";
                ArgumentsError = "Expected a parameter of `embed`, `code`, or `blockquote`.";
                ExpectedArguments = 1;
                PermissionsRequired = new List<Permissions>
                {
                    Permissions.MANAGE_GUILD
                };
                BotAllowed = false;

                _guildService = guildService;
            }

            public async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                if (args[0] is not ("embed" or "code" or "blockquote"))
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages = new List<InternalEmbed>
                        {
                            Utils.GetInstance().Embedify("/setserverdisplay", "You may choose between `embed`, `code`, or `blockquote`.", true)
                        },
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
                    await _guildService.Create(new Guild
                    {
                        GuildId = req.GuildId,
                        Version = "RSV",
                        Language = "english_us",
                        Prefix = "+",
                        DisplayStyle = args[0],
                        IgnoringBrackets = "<>",
                        IsDM = req.IsDM
                    });
                }

                return new CommandResponse
                {
                    OK = true,
                    Pages = new List<InternalEmbed>
                    {
                        Utils.GetInstance().Embedify("/setserverdisplay", "Set server display style successfully.", false)
                    },
                    LogStatement = $"/setserverdisplay {args[0]}"
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

            private readonly GuildService _guildService;

            public FormattingSetIgnoringBrackets(GuildService guildService)
            {
                Name = "setbrackets";
                ArgumentsError = "Expected a parameter with two characters, that must be `<>`, `[]`, `{}`, or `()`.";
                ExpectedArguments = 1;
                PermissionsRequired = new List<Permissions>
                {
                    Permissions.MANAGE_GUILD
                };
                BotAllowed = false;

                _guildService = guildService;
            }

            public async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                List<string> acceptableBrackets = new() { "<>", "[]", "{}", "()" };

                if (args[0].Length != 2)
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages = new List<InternalEmbed>
                        {
                            Utils.GetInstance().Embedify("/setbrackets", "The brackets can only be two characters.", true)
                        },
                        LogStatement = "/setbrackets"
                    };
                }
                else if (!acceptableBrackets.Contains(args[0]))
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages = new List<InternalEmbed>
                        {
                            Utils.GetInstance().Embedify("/setbrackets", "The brackets can only be set to `<>`, `[]`, `{}`, or `()`.", true)
                        },
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
                    await _guildService.Create(new Guild
                    {
                        GuildId = req.GuildId,
                        Version = "RSV",
                        Language = "english_us",
                        Prefix = "+",
                        DisplayStyle = "embed",
                        IgnoringBrackets = args[0],
                        IsDM = req.IsDM
                    });
                }

                return new CommandResponse
                {
                    OK = true,
                    Pages = new List<InternalEmbed>
                    {
                        Utils.GetInstance().Embedify("/setbrackets", "Set brackets successfully.", false)
                    },
                    LogStatement = $"/setbrackets {args[0]}"
                };
            }
        }
    }
}
