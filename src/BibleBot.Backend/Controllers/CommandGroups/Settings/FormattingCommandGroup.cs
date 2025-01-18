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
    public class FormattingCommandGroup(UserService userService, GuildService guildService) : CommandGroup
    {
        public override string Name { get => "formatting"; set { } }
        public override Command DefaultCommand { get => Commands.FirstOrDefault(cmd => cmd.Name == "usage"); set { } }
        public override List<Command> Commands
        {
            get => [
                new FormattingUsage(userService, guildService),
                new FormattingSetVerseNumbers(userService),
                new FormattingSetTitles(userService),
                new FormattingSetPagination(userService),
                new FormattingSetDisplayStyle(userService),
                new FormattingSetServerDisplayStyle(guildService),
                new FormattingSetIgnoringBrackets(guildService)
            ]; set { }
        }

        public class FormattingUsage(UserService userService, GuildService guildService) : Command
        {
            public override string Name { get => "usage"; set { } }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                User idealUser = await userService.Get(req.UserId);
                Guild idealGuild = await guildService.Get(req.GuildId);

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

        public class FormattingSetVerseNumbers(UserService userService) : Command
        {
            public override string Name { get => "setversenumbers"; set { } }
            public override string ArgumentsError { get => "Expected an `enable` or `disable` parameter."; set { } }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
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

                User idealUser = await userService.Get(req.UserId);

                if (idealUser != null)
                {
                    UpdateDefinition<User> update = Builders<User>.Update
                                 .Set(user => user.VerseNumbersEnabled, args[0] is "enable" and not "disable");

                    await userService.Update(req.UserId, update);
                }
                else
                {
                    User newUser = new()
                    {
                        UserId = req.UserId,
                        VerseNumbersEnabled = args[0] is "enable" and not "disable"
                    };

                    await userService.Create(newUser);
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

        public class FormattingSetTitles(UserService userService) : Command
        {
            public override string Name { get => "settitles"; set { } }
            public override string ArgumentsError { get => "Expected an `enable` or `disable` parameter."; set { } }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
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

                User idealUser = await userService.Get(req.UserId);

                if (idealUser != null)
                {
                    UpdateDefinition<User> update = Builders<User>.Update
                                 .Set(user => user.TitlesEnabled, args[0] is "enable" and not "disable");

                    await userService.Update(req.UserId, update);
                }
                else
                {
                    User newUser = new()
                    {
                        UserId = req.UserId,
                        TitlesEnabled = args[0] is "enable" and not "disable"
                    };

                    await userService.Create(newUser);
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

        public class FormattingSetPagination(UserService userService) : Command
        {
            public override string Name { get => "setpagination"; set { } }
            public override string ArgumentsError { get => "Expected an `enable` or `disable` parameter."; set { } }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
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

                User idealUser = await userService.Get(req.UserId);

                if (idealUser != null)
                {
                    UpdateDefinition<User> update = Builders<User>.Update
                                 .Set(user => user.PaginationEnabled, args[0] is "enable" and not "disable");

                    await userService.Update(req.UserId, update);
                }
                else
                {
                    User newUser = new()
                    {
                        UserId = req.UserId,
                        PaginationEnabled = args[0] is "enable" and not "disable"
                    };

                    await userService.Create(newUser);
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

        public class FormattingSetDisplayStyle(UserService userService) : Command
        {
            public override string Name { get => "setdisplay"; set { } }
            public override string ArgumentsError { get => "Expected a parameter of `embed`, `code`, or `blockquote`."; set { } }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
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

                User idealUser = await userService.Get(req.UserId);

                if (idealUser != null)
                {
                    UpdateDefinition<User> update = Builders<User>.Update
                                 .Set(user => user.DisplayStyle, args[0]);

                    await userService.Update(req.UserId, update);
                }
                else
                {
                    User newUser = new()
                    {
                        UserId = req.UserId,
                        DisplayStyle = args[0]
                    };

                    await userService.Create(newUser);
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

        public class FormattingSetServerDisplayStyle(GuildService guildService) : Command
        {
            public override string Name { get => "setserverdisplay"; set { } }
            public override string ArgumentsError { get => "Expected a parameter of `embed`, `code`, or `blockquote`."; set { } }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
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

                Guild idealGuild = await guildService.Get(req.GuildId);

                if (idealGuild != null)
                {
                    UpdateDefinition<Guild> update = Builders<Guild>.Update
                                 .Set(guild => guild.DisplayStyle, args[0]);

                    await guildService.Update(req.GuildId, update);
                }
                else
                {
                    Guild newGuild = new()
                    {
                        GuildId = req.GuildId,
                        DisplayStyle = args[0],
                        IsDM = req.IsDM
                    };

                    await guildService.Create(newGuild);
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

        public class FormattingSetIgnoringBrackets(GuildService guildService) : Command
        {
            public override string Name { get => "setbrackets"; set { } }
            public override string ArgumentsError { get => "Expected a parameter with two characters, that must be `<>`, `[]`, `{}`, or `()`."; set { } }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
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


                Guild idealGuild = await guildService.Get(req.GuildId);

                if (idealGuild != null)
                {
                    UpdateDefinition<Guild> update = Builders<Guild>.Update
                                 .Set(guild => guild.IgnoringBrackets, args[0]);

                    await guildService.Update(req.GuildId, update);
                }
                else
                {
                    Guild newGuild = new()
                    {
                        GuildId = req.GuildId,
                        IgnoringBrackets = args[0],
                        IsDM = req.IsDM
                    };

                    await guildService.Create(newGuild);
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
