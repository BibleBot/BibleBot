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
using BibleBot.Backend.InternalModels;
using BibleBot.Backend.Services;
using BibleBot.Models;
using Microsoft.Extensions.Localization;
using MongoDB.Driver;

namespace BibleBot.Backend.Controllers.CommandGroups.Settings
{
    public class VersionCommandGroup(UserService userService, GuildService guildService, VersionService versionService, NameFetchingService nameFetchingService, IStringLocalizerFactory localizerFactory) : CommandGroup
    {
        private readonly IStringLocalizer _localizer = localizerFactory.Create(typeof(VersionCommandGroup));
        private readonly IStringLocalizer _sharedLocalizer = localizerFactory.Create(typeof(SharedResource));

        public override string Name { get => "version"; set => throw new NotImplementedException(); }
        public override Command DefaultCommand { get => Commands.FirstOrDefault(cmd => cmd.Name == "usage"); set => throw new NotImplementedException(); }
        public override List<Command> Commands
        {
            get => [
                new VersionUsage(userService, guildService, versionService, _localizer, _sharedLocalizer),
                new VersionSet(userService, versionService, _localizer),
                new VersionSetServer(guildService, versionService, _localizer),
                new VersionInfo(userService, guildService, versionService, _localizer),
                new VersionList(versionService, _sharedLocalizer),
                new VersionBookList(userService, guildService, versionService, nameFetchingService, _localizer)
            ]; set => throw new NotImplementedException();
        }

        public class VersionUsage(UserService userService, GuildService guildService, VersionService versionService, IStringLocalizer localizer, IStringLocalizer sharedLocalizer) : Command
        {
            public override string Name { get => "usage"; set => throw new NotImplementedException(); }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                User idealUser = await userService.Get(req.UserId);
                Guild idealGuild = await guildService.Get(req.GuildId);

                Models.Version defaultVersion = await versionService.Get("RSV");

                string response = $"{localizer["VersionStatusPreference"]}\n" +
                               $"{localizer["VersionStatusServerPreference"]}\n\n" +
                               $"__**{sharedLocalizer["RelatedCommands"]}**__\n" +
                               $"**/setversion** - {localizer["SetVersionCommandDescription"]}\n" +
                               $"**/setserverversion** - {localizer["SetServerVersionCommandDescription"]}\n" +
                               $"**/versioninfo** - {localizer["VersionInfoCommandDescription"]}\n" +
                               $"**/listversions** - {localizer["ListVersionsCommandDescription"]}\n" +
                               $"**/booklist** - {localizer["BookListCommandDescription"]}";

                List<string> replacements = [];

                if (idealUser != null)
                {
                    Models.Version idealUserVersion = await versionService.Get(idealUser.Version);

                    if (idealUserVersion != null)
                    {
                        replacements.Add($"**{idealUserVersion.Name}**");
                    }
                }

                if (replacements.Count == 0)
                {
                    replacements.Add($"**{defaultVersion.Name}**");
                }

                if (idealGuild != null)
                {
                    Models.Version idealGuildVersion = await versionService.Get(idealGuild.Version);

                    if (idealGuildVersion != null)
                    {
                        replacements.Add($"**{idealGuildVersion.Name}**");
                    }
                }

                if (replacements.Count == 1)
                {
                    replacements.Add($"**{defaultVersion.Name}**");
                }

                response = string.Format(response, [.. replacements]);

                return new CommandResponse
                {
                    OK = true,
                    Pages =
                    [
                        Utils.GetInstance().Embedify("/version", response, false)
                    ],
                    LogStatement = "/version"
                };
            }
        }

        public class VersionSet(UserService userService, VersionService versionService, IStringLocalizer localizer) : Command
        {
            public override string Name { get => "set"; set => throw new NotImplementedException(); }
            public override string ArgumentsError { get => "Expected a version abbreviation parameter, like `RSV` or `KJV`."; set => throw new NotImplementedException(); }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                Models.Version idealVersion = await versionService.Get(args[0]);

                if (idealVersion != null)
                {
                    User idealUser = await userService.Get(req.UserId);

                    if (idealUser != null)
                    {
                        UpdateDefinition<User> update = Builders<User>.Update
                                     .Set(user => user.Version, idealVersion.Abbreviation);

                        await userService.Update(req.UserId, update);
                    }
                    else
                    {
                        User newUser = new()
                        {
                            UserId = req.UserId,
                            Version = idealVersion.Abbreviation
                        };

                        await userService.Create(newUser);
                    }

                    return new CommandResponse
                    {
                        OK = true,
                        Pages =
                        [
                            Utils.GetInstance().Embedify("/setversion", localizer["SetVersionSuccess"], false)
                        ],
                        LogStatement = $"/setversion {args[0]}"
                    };
                }

                return new CommandResponse
                {
                    OK = false,
                    Pages =
                    [
                        Utils.GetInstance().Embedify("/setversion", localizer["SetVersionFailure"], true)
                    ],
                    LogStatement = "/setversion"
                };
            }
        }

        public class VersionSetServer(GuildService guildService, VersionService versionService, IStringLocalizer localizer) : Command
        {
            public override string Name { get => "setserver"; set => throw new NotImplementedException(); }
            public override string ArgumentsError { get => "Expected a version abbreviation parameter, like `RSV` or `KJV`."; set => throw new NotImplementedException(); }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                Models.Version idealVersion = await versionService.Get(args[0]);

                if (idealVersion != null)
                {
                    Guild idealGuild = await guildService.Get(req.GuildId);

                    if (idealGuild != null)
                    {
                        UpdateDefinition<Guild> update = Builders<Guild>.Update
                                     .Set(guild => guild.Version, idealVersion.Abbreviation);

                        await guildService.Update(req.GuildId, update);
                    }
                    else
                    {
                        Guild newGuild = new()
                        {
                            GuildId = req.GuildId,
                            Version = idealVersion.Abbreviation,
                            IsDM = req.IsDM
                        };

                        await guildService.Create(newGuild);
                    }

                    string message = localizer["SetServerVersionSuccess"];
                    if (!idealVersion.SupportsOldTestament || !idealVersion.SupportsNewTestament)
                    {
                        message += $"\n\n:warning: {localizer["SetServerVersionSuccessDailyVerseWarning"]}";
                    }

                    return new CommandResponse
                    {
                        OK = true,
                        Pages =
                        [
                            Utils.GetInstance().Embedify("/setserverversion", message, false)
                        ],
                        LogStatement = $"/setserverversion {args[0]}"
                    };
                }

                return new CommandResponse
                {
                    OK = false,
                    Pages =
                    [
                        Utils.GetInstance().Embedify("/setserverversion", localizer["SetServerVersionFailure"], true)
                    ],
                    LogStatement = "/setserverversion"
                };
            }
        }

        public class VersionInfo(UserService userService, GuildService guildService, VersionService versionService, IStringLocalizer localizer) : Command
        {
            public override string Name { get => "info"; set => throw new NotImplementedException(); }
            public override string ArgumentsError { get => "Expected a version abbreviation parameter, like `RSV` or `KJV`."; set => throw new NotImplementedException(); }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                User idealUser = await userService.Get(req.UserId);
                Guild idealGuild = await guildService.Get(req.GuildId);

                string version;

                if (args.Count > 0)
                {
                    version = args[0];
                }
                else
                {
                    version = "RSV";

                    if (idealUser != null && !req.IsBot)
                    {
                        version = idealUser.Version;
                    }
                    else if (idealGuild != null)
                    {
                        version = idealGuild.Version;
                    }
                }

                Models.Version idealVersion = await versionService.Get(version) ?? await versionService.Get("RSV");

                if (idealVersion != null)
                {
                    string response = $"### {idealVersion.Name}\n\n" +
                                $"{localizer["VersionInfoContainsOT"]}: {(idealVersion.SupportsOldTestament ? ":white_check_mark:" : ":x:")}\n" +
                                $"{localizer["VersionInfoContainsNT"]}: {(idealVersion.SupportsNewTestament ? ":white_check_mark:" : ":x:")}\n" +
                                $"{localizer["VersionInfoContainsDEU"]}: {(idealVersion.SupportsDeuterocanon ? ":white_check_mark:" : ":x:")}\n\n" +
                                $"__**{localizer["VersionInfoDeveloperInfoHeader"]}**__\n" +
                                $"{localizer["VersionInfoSource"]}: `{idealVersion.Source}`";

                    if (idealVersion.Publisher != null)
                    {
                        response += $"\n{localizer["VersionInfoPublisher"]}: `{idealVersion.Publisher}`";
                    }

                    if (idealVersion.ApiBibleId != null)
                    {
                        response += $"\nAPI.Bible ID: `{idealVersion.ApiBibleId}`";
                    }

                    if (!idealVersion.SupportsOldTestament || !idealVersion.SupportsNewTestament)
                    {
                        response += $"\n\n:warning: {localizer["VersionInfoDailyVerseWarning"]}";
                    }

                    response += $"\n\n{localizer["VersionInfoBookListNotice"]}";

                    return new CommandResponse
                    {
                        OK = true,
                        Pages =
                            [
                                Utils.GetInstance().Embedify("/versioninfo", response, false)
                            ],
                        LogStatement = $"/versioninfo {version}"
                    };
                }

                return new CommandResponse
                {
                    OK = false,
                    Pages =
                    [
                        Utils.GetInstance().Embedify("/versioninfo", localizer["InvalidVersion"], true)
                    ],
                    LogStatement = "/versioninfo"
                };
            }
        }

        public class VersionList(VersionService versionService, IStringLocalizer sharedLocalizer) : Command
        {
            public override string Name { get => "list"; set => throw new NotImplementedException(); }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                List<Models.Version> versions = await versionService.Get();
                versions.Sort((x, y) => x.Name.CompareTo(y.Name));

                List<string> versionsUsed = [];
                List<InternalEmbed> pages = [];

                int maxResultsPerPage = 25;

                // We need to add a page here because the for loop won't hit the last one otherwise.
                // This also prevents situations where the Ceiling() result might equal 0.
                int totalPages = (int)Math.Ceiling((decimal)(versions.Count / maxResultsPerPage)) + 1;

                for (int i = 0; i < totalPages; i++)
                {
                    int count = 0;
                    StringBuilder versionList = new();

                    foreach (Models.Version version in versions)
                    {
                        if (count < maxResultsPerPage)
                        {
                            if (!versionsUsed.Contains(version.Name))
                            {
                                versionList.Append($"{version.Name}\n");
                                versionsUsed.Add(version.Name);
                                count++;
                            }
                        }
                    }

                    string pageCounter = string.Format(sharedLocalizer["PageCounter"], [i + 1, totalPages]);
                    InternalEmbed embed = Utils.GetInstance().Embedify($"/listversions - {pageCounter}", versionList.ToString(), false);
                    pages.Add(embed);
                }

                return new CommandResponse
                {
                    OK = true,
                    Pages = pages,
                    LogStatement = "/listversions"
                };
            }
        }

        public class VersionBookList(UserService userService, GuildService guildService, VersionService versionService, NameFetchingService nameFetchingService, IStringLocalizer localizer) : Command
        {
            public override string Name { get => "booklist"; set => throw new NotImplementedException(); }
            public override string ArgumentsError { get => "Expected a version abbreviation parameter, like `RSV` or `KJV`."; set => throw new NotImplementedException(); }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                User idealUser = await userService.Get(req.UserId);
                Guild idealGuild = await guildService.Get(req.GuildId);

                string version;

                if (args.Count > 0)
                {
                    version = args[0];
                }
                else
                {
                    version = "RSV";

                    if (idealUser != null && !req.IsBot)
                    {
                        version = idealUser.Version;
                    }
                    else if (idealGuild != null)
                    {
                        version = idealGuild.Version;
                    }
                }

                Models.Version idealVersion = await versionService.Get(version) ?? await versionService.Get("RSV");

                if (idealVersion != null)
                {
                    // if (idealVersion.Source != "bg")
                    // {
                    //     return new CommandResponse
                    //     {
                    //         OK = false,
                    //         Pages =
                    //         [
                    //             Utils.GetInstance().Embedify("/booklist", localizer["BookListVersionIneligible"], true)
                    //         ],
                    //         LogStatement = "/booklist - non-bg source"
                    //     };
                    // }

                    Dictionary<BookCategories, Dictionary<string, string>> names = null;

                    if (idealVersion.Source == "bg")
                    {
                        names = await nameFetchingService.GetBibleGatewayVersionBookList(idealVersion);
                    }
                    else if (idealVersion.Source == "ab")
                    {
                        names = await nameFetchingService.GetAPIBibleVersionBookList(idealVersion);
                    }

                    if (names != null)
                    {
                        List<InternalEmbed> pages = [];

                        if (names.ContainsKey(BookCategories.OldTestament))
                        {
                            pages.Add(Utils.GetInstance().Embedify($"/booklist - {idealVersion.Name}", $"### {localizer["BookListOTHeader"]}\n* " + string.Join("\n* ", names[BookCategories.OldTestament].Values).Replace("<151>", $"*({localizer["BookListContainsPsalm151"]})*"), false));
                        }

                        if (names.ContainsKey(BookCategories.NewTestament))
                        {
                            pages.Add(Utils.GetInstance().Embedify($"/booklist - {idealVersion.Name}", $"### {localizer["BookListNTHeader"]}\n* " + string.Join("\n* ", names[BookCategories.NewTestament].Values), false));
                        }

                        if (names.ContainsKey(BookCategories.Deuterocanon))
                        {
                            pages.Add(Utils.GetInstance().Embedify($"/booklist - {idealVersion.Name}", $"### {localizer["BookListDEUHeader"]}\n* " + string.Join("\n* ", names[BookCategories.Deuterocanon].Values), false));
                        }

                        return new CommandResponse
                        {
                            OK = true,
                            Pages = pages,
                            LogStatement = "/booklist"
                        };
                    }
                    else
                    {
                        string message = $"{localizer["BookListInternalError"]}\n\n" +
                        $"```\nVersion: {idealVersion.Abbreviation}\n```";

                        return new CommandResponse
                        {
                            OK = false,
                            Pages =
                            [
                                Utils.GetInstance().Embedify("/booklist", message, true)
                            ],
                            LogStatement = $"/booklist - internal error on {idealVersion.Abbreviation}"
                        };
                    }
                }

                return new CommandResponse
                {
                    OK = false,
                    Pages =
                    [
                        Utils.GetInstance().Embedify("/booklist", localizer["InvalidVersion"], true)
                    ],
                    LogStatement = "/booklist - invalid version"
                };
            }
        }
    }
}