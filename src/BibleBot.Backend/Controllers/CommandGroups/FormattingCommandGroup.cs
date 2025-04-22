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
using System.Threading.Tasks;
using BibleBot.Backend.InternalModels;
using BibleBot.Backend.Services;
using BibleBot.Models;
using Microsoft.Extensions.Localization;
using MongoDB.Driver;

namespace BibleBot.Backend.Controllers.CommandGroups
{
    public class FormattingCommandGroup(UserService userService, GuildService guildService, IStringLocalizerFactory localizerFactory) : CommandGroup
    {
        private readonly IStringLocalizer _localizer = localizerFactory.Create(typeof(FormattingCommandGroup));
        private readonly IStringLocalizer _sharedLocalizer = localizerFactory.Create(typeof(SharedResource));

        public override string Name { get => "formatting"; set => throw new NotImplementedException(); }
        public override Command DefaultCommand { get => Commands.FirstOrDefault(cmd => cmd.Name == "usage"); set => throw new NotImplementedException(); }
        public override List<Command> Commands
        {
            get => [
                new FormattingUsage(userService, guildService, _localizer, _sharedLocalizer),
                new FormattingSetVerseNumbers(userService, _localizer),
                new FormattingSetTitles(userService, _localizer),
                new FormattingSetPagination(userService, _localizer),
                new FormattingSetDisplayStyle(userService, _localizer),
                new FormattingSetServerDisplayStyle(guildService, _localizer),
                new FormattingSetIgnoringBrackets(guildService, _localizer)
            ]; set => throw new NotImplementedException();
        }

        public class FormattingUsage(UserService userService, GuildService guildService, IStringLocalizer localizer, IStringLocalizer sharedLocalizer) : Command
        {
            public override string Name { get => "usage"; set => throw new NotImplementedException(); }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                User idealUser = await userService.Get(req.UserId);
                Guild idealGuild = await guildService.Get(req.GuildId);

                string response = $"{localizer["FormattingStatusVerseNumbers"]}\n" +
                               $"{localizer["FormattingStatusTitles"]}\n" +
                               $"{localizer["FormattingStatusVersePagination"]}\n" +
                               $"{localizer["FormattingStatusDisplayStyle"]}\n\n" +
                               $"{localizer["FormattingStatusServerDisplayStyle"]}\n" +
                               $"{localizer["FormattingStatusIgnoringBrackets"]}\n\n" +
                               $"__**{sharedLocalizer["RelatedCommands"]}**__\n" +
                               $"**/setversenumbers** - {localizer["SetVerseNumbersCommandDescription"]}\n" +
                               $"**/settitles** - {localizer["SetTitlesCommandDescription"]}\n" +
                               $"**/setpagination** - {localizer["SetPaginationCommandDescription"]}\n" +
                               $"**/setdisplay** - {localizer["SetDisplayCommandDescription"]}\n" +
                               $"**/setserverdisplay** - {localizer["SetServerDisplayCommandDescription"]}\n" +
                               $"**/setbrackets** - {localizer["SetBracketsCommandDescription"]}";

                List<string> replacements = [];

                if (idealUser != null)
                {
                    replacements.AddRange(idealUser.VerseNumbersEnabled ? [":white_check_mark:", $"**{localizer["Enabled"]}**"] : [":no_entry_sign:", $"**{localizer["Disabled"]}**"]);
                    replacements.AddRange(idealUser.TitlesEnabled ? [":white_check_mark:", $"**{localizer["Enabled"]}**"] : [":no_entry_sign:", $"**{localizer["Disabled"]}**"]);
                    replacements.AddRange(idealUser.PaginationEnabled ? [":white_check_mark:", $"**{localizer["Enabled"]}**"] : [":no_entry_sign:", $"**{localizer["Disabled"]}**"]);
                    replacements.Add(idealUser.DisplayStyle != null ? $"**`{idealUser.DisplayStyle}`**" : "**`embed`**");
                }
                else
                {
                    replacements.AddRange([":white_check_mark:", $"**{localizer["Enabled"]}**"]);
                    replacements.AddRange([":white_check_mark:", $"**{localizer["Enabled"]}**"]);
                    replacements.AddRange([":white_check_mark:", $"**{localizer["Disabled"]}**"]);
                    replacements.Add("**`embed`**");
                }

                if (idealGuild != null)
                {
                    replacements.Add(idealGuild.DisplayStyle != null ? $"**`{idealUser.DisplayStyle}`**" : "**`embed`**");
                    replacements.Add(idealGuild.IgnoringBrackets != "<>" ? $" / **`{idealGuild.IgnoringBrackets}`**" : "");
                }
                else
                {
                    replacements.Add("**`embed`**");
                    replacements.Add("");
                }

                response = string.Format(response, [.. replacements]);

                return new CommandResponse
                {
                    OK = true,
                    Pages =
                    [
                        Utils.GetInstance().Embedify("/formatting", response, false)
                    ],
                    LogStatement = "/formatting",
                    Culture = CultureInfo.CurrentUICulture.Name
                };
            }
        }

        public class FormattingSetVerseNumbers(UserService userService, IStringLocalizer localizer) : Command
        {
            public override string Name { get => "setversenumbers"; set => throw new NotImplementedException(); }
            public override string ArgumentsError { get => "Expected an `enable` or `disable` parameter."; set => throw new NotImplementedException(); }

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
                        LogStatement = "/setversenumbers",
                        Culture = CultureInfo.CurrentUICulture.Name
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
                        Utils.GetInstance().Embedify("/setversenumbers", localizer["SetVerseNumbersSuccess"], false)
                    ],
                    LogStatement = $"/setversenumbers {args[0]}",
                    Culture = CultureInfo.CurrentUICulture.Name
                };
            }
        }

        public class FormattingSetTitles(UserService userService, IStringLocalizer localizer) : Command
        {
            public override string Name { get => "settitles"; set => throw new NotImplementedException(); }
            public override string ArgumentsError { get => "Expected an `enable` or `disable` parameter."; set => throw new NotImplementedException(); }

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
                        LogStatement = "/settitles",
                        Culture = CultureInfo.CurrentUICulture.Name
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
                        Utils.GetInstance().Embedify("/settitles", localizer["SetTitlesSuccess"], false)
                    ],
                    LogStatement = $"/settitles {args[0]}",
                    Culture = CultureInfo.CurrentUICulture.Name
                };
            }
        }

        public class FormattingSetPagination(UserService userService, IStringLocalizer localizer) : Command
        {
            public override string Name { get => "setpagination"; set => throw new NotImplementedException(); }
            public override string ArgumentsError { get => "Expected an `enable` or `disable` parameter."; set => throw new NotImplementedException(); }

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
                        LogStatement = "/setpagination",
                        Culture = CultureInfo.CurrentUICulture.Name
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
                        Utils.GetInstance().Embedify("/setpagination", localizer["SetPaginationSuccess"], false)
                    ],
                    LogStatement = $"/setpagination {args[0]}",
                    Culture = CultureInfo.CurrentUICulture.Name
                };
            }
        }

        public class FormattingSetDisplayStyle(UserService userService, IStringLocalizer localizer) : Command
        {
            public override string Name { get => "setdisplay"; set => throw new NotImplementedException(); }
            public override string ArgumentsError { get => "Expected a parameter of `embed`, `code`, or `blockquote`."; set => throw new NotImplementedException(); }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                if (args[0] is not ("embed" or "code" or "blockquote"))
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages =
                        [
                            Utils.GetInstance().Embedify("/setdisplay", localizer["SetDisplayFailure"], true)
                        ],
                        LogStatement = "/setdisplay",
                        Culture = CultureInfo.CurrentUICulture.Name
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
                        Utils.GetInstance().Embedify("/setdisplay", localizer["SetDisplaySuccess"], false)
                    ],
                    LogStatement = $"/setdisplay {args[0]}",
                    Culture = CultureInfo.CurrentUICulture.Name
                };
            }
        }

        public class FormattingSetServerDisplayStyle(GuildService guildService, IStringLocalizer localizer) : Command
        {
            public override string Name { get => "setserverdisplay"; set => throw new NotImplementedException(); }
            public override string ArgumentsError { get => "Expected a parameter of `embed`, `code`, or `blockquote`."; set => throw new NotImplementedException(); }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                if (args[0] is not ("embed" or "code" or "blockquote"))
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages =
                        [
                            Utils.GetInstance().Embedify("/setserverdisplay", localizer["SetDisplayFailure"], true)
                        ],
                        LogStatement = "/setserverdisplay",
                        Culture = CultureInfo.CurrentUICulture.Name
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
                        Utils.GetInstance().Embedify("/setserverdisplay", localizer["SetServerDisplaySuccess"], false)
                    ],
                    LogStatement = $"/setserverdisplay {args[0]}",
                    Culture = CultureInfo.CurrentUICulture.Name
                };
            }
        }

        public class FormattingSetIgnoringBrackets(GuildService guildService, IStringLocalizer localizer) : Command
        {
            public override string Name { get => "setbrackets"; set => throw new NotImplementedException(); }
            public override string ArgumentsError { get => "Expected a parameter with two characters, that must be `<>`, `[]`, `{}`, or `()`."; set => throw new NotImplementedException(); }

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
                            Utils.GetInstance().Embedify("/setbrackets", localizer["SetBracketsFailureLength"], true)
                        ],
                        LogStatement = "/setbrackets",
                        Culture = CultureInfo.CurrentUICulture.Name
                    };
                }
                else if (!acceptableBrackets.Contains(args[0]))
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages =
                        [
                            Utils.GetInstance().Embedify("/setbrackets", localizer["SetBracketsFailure"], true)
                        ],
                        LogStatement = "/setbrackets",
                        Culture = CultureInfo.CurrentUICulture.Name
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
                        Utils.GetInstance().Embedify("/setbrackets", localizer["SetBracketsSuccess"], false)
                    ],
                    LogStatement = $"/setbrackets {args[0]}",
                    Culture = CultureInfo.CurrentUICulture.Name
                };
            }
        }
    }
}
