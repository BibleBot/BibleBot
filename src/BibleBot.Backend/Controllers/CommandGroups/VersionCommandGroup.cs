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
using Version = BibleBot.Models.Version;
using MDVersionBookList = System.Collections.Generic.Dictionary<BibleBot.Models.BookCategories, System.Collections.Generic.Dictionary<string, string>>;

namespace BibleBot.Backend.Controllers.CommandGroups
{
    public class VersionCommandGroup(UserService userService, GuildService guildService, VersionService versionService, MetadataFetchingService metadataFetchingService, IStringLocalizerFactory localizerFactory) : CommandGroup
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
                new VersionBookList(userService, guildService, versionService, metadataFetchingService, _localizer)
            ]; set => throw new NotImplementedException();
        }

        private class VersionUsage(UserService userService, GuildService guildService, VersionService versionService, IStringLocalizer localizer, IStringLocalizer sharedLocalizer) : Command
        {
            public override string Name { get => "usage"; set => throw new NotImplementedException(); }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                User idealUser = await userService.Get(req.UserId);
                Guild idealGuild = await guildService.Get(req.GuildId);

                string response = $"{localizer["VersionStatusPreference"]}\n" +
                               $"{localizer["VersionStatusServerPreference"]}\n\n" +
                               $"__**{sharedLocalizer["RelatedCommands"]}**__\n" +
                               $"**/setversion** - {localizer["SetVersionCommandDescription"]}\n" +
                               $"**/setserverversion** - {localizer["SetServerVersionCommandDescription"]}\n" +
                               $"**/versioninfo** - {localizer["VersionInfoCommandDescription"]}\n" +
                               $"**/listversions** - {localizer["ListVersionsCommandDescription"]}\n" +
                               $"**/booklist** - {localizer["BookListCommandDescription"]}";

                List<string> replacements = [];

                Version idealUserVersion = await versionService.GetPreferenceOrDefault(idealUser, false);
                replacements.Add($"**{idealUserVersion.Name}**");

                Version idealGuildVersion = await versionService.GetPreferenceOrDefault(idealGuild, false);
                replacements.Add($"**{idealGuildVersion.Name}**");

                response = string.Format(response, [.. replacements]);

                return new CommandResponse
                {
                    OK = true,
                    Pages =
                    [
                        Utils.GetInstance().Embedify("/version", response, false)
                    ],
                    LogStatement = "/version",
                    Culture = CultureInfo.CurrentUICulture.Name
                };
            }
        }

        private class VersionSet(UserService userService, VersionService versionService, IStringLocalizer localizer) : Command
        {
            public override string Name { get => "set"; set => throw new NotImplementedException(); }
            public override string ArgumentsError { get => "Expected a version abbreviation parameter, like `RSV` or `KJV`."; set => throw new NotImplementedException(); }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                Version idealVersion = await versionService.Get(args[0]);

                if (idealVersion == null)
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages =
                        [
                            Utils.GetInstance().Embedify("/setversion", localizer["SetVersionFailure"], true)
                        ],
                        LogStatement = "/setversion",
                        Culture = CultureInfo.CurrentUICulture.Name
                    };
                }

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
                    LogStatement = $"/setversion {args[0]}",
                    Culture = CultureInfo.CurrentUICulture.Name
                };

            }
        }

        private class VersionSetServer(GuildService guildService, VersionService versionService, IStringLocalizer localizer) : Command
        {
            public override string Name { get => "setserver"; set => throw new NotImplementedException(); }
            public override string ArgumentsError { get => "Expected a version abbreviation parameter, like `RSV` or `KJV`."; set => throw new NotImplementedException(); }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                Version idealVersion = await versionService.Get(args[0]);

                if (idealVersion == null)
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages =
                        [
                            Utils.GetInstance().Embedify("/setserverversion", localizer["SetServerVersionFailure"], true)
                        ],
                        LogStatement = "/setserverversion",
                        Culture = CultureInfo.CurrentUICulture.Name
                    };
                }

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
                    LogStatement = $"/setserverversion {args[0]}",
                    Culture = CultureInfo.CurrentUICulture.Name
                };

            }
        }

        private class VersionInfo(UserService userService, GuildService guildService, VersionService versionService, IStringLocalizer localizer) : Command
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

                Version idealVersion = await versionService.Get(version) ?? await versionService.Get("RSV");

                if (idealVersion == null)
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages =
                        [
                            Utils.GetInstance().Embedify("/versioninfo", localizer["InvalidVersion"], true)
                        ],
                        LogStatement = "/versioninfo",
                        Culture = CultureInfo.CurrentUICulture.Name
                    };
                }

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

                if (idealVersion.InternalId != null)
                {
                    response += $"\nInternal ID: `{idealVersion.InternalId}`";
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
                    LogStatement = $"/versioninfo {version}",
                    Culture = CultureInfo.CurrentUICulture.Name
                };

            }
        }

        private class VersionList(VersionService versionService, IStringLocalizer sharedLocalizer) : Command
        {
            public override string Name { get => "list"; set => throw new NotImplementedException(); }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                List<Version> versions = await versionService.Get();
                versions.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));

                List<string> versionsUsed = [];
                List<InternalEmbed> pages = [];

                const int maxResultsPerPage = 25; // TODO: make this an appsettings param

                // We need to add a page here because the for loop won't hit the last one otherwise.
                // This also prevents situations where the Ceiling() result might equal 0.
                int totalPages = (int)Math.Ceiling((decimal)(versions.Count / maxResultsPerPage));

                for (int i = 0; i < totalPages; i++)
                {
                    int count = 0;
                    StringBuilder versionList = new();

                    foreach (Version version in versions.Where(version => count < maxResultsPerPage).Where(version => !versionsUsed.Contains(version.Name)))
                    {
                        versionList.Append($"{version.Name}\n");
                        versionsUsed.Add(version.Name);
                        count++;
                    }

                    string pageCounter = string.Format(sharedLocalizer["PageCounter"], [i + 1, totalPages]);
                    InternalEmbed embed = Utils.GetInstance().Embedify($"/listversions - {pageCounter}", versionList.ToString(), false);
                    pages.Add(embed);
                }

                return new CommandResponse
                {
                    OK = true,
                    Pages = pages,
                    LogStatement = "/listversions",
                    Culture = CultureInfo.CurrentUICulture.Name
                };
            }
        }

        private class VersionBookList(UserService userService, GuildService guildService, VersionService versionService, MetadataFetchingService metadataFetchingService, IStringLocalizer localizer) : Command
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

                Version idealVersion = await versionService.Get(version) ?? await versionService.Get("RSV");

                if (idealVersion == null)
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages =
                        [
                            Utils.GetInstance().Embedify("/booklist", localizer["InvalidVersion"], true)
                        ],
                        LogStatement = "/booklist - invalid version",
                        Culture = CultureInfo.CurrentUICulture.Name
                    };
                }

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

                MDVersionBookList names = metadataFetchingService.GetVersionBookList(idealVersion);

                if (names != null)
                {
                    List<InternalEmbed> pages = [];

                    if (names.TryGetValue(BookCategories.OldTestament, out Dictionary<string, string> otNames))
                    {
                        pages.Add(Utils.GetInstance().Embedify($"/booklist - {idealVersion.Name}", $"### {localizer["BookListOTHeader"]}\n* " + string.Join("\n* ", otNames.Values).Replace("<151>", $"*({localizer["BookListContainsPsalm151"]})*"), false));
                    }

                    if (names.TryGetValue(BookCategories.NewTestament, out Dictionary<string, string> ntNames))
                    {
                        pages.Add(Utils.GetInstance().Embedify($"/booklist - {idealVersion.Name}", $"### {localizer["BookListNTHeader"]}\n* " + string.Join("\n* ", ntNames.Values), false));
                    }

                    if (names.TryGetValue(BookCategories.Deuterocanon, out Dictionary<string, string> deuNames))
                    {
                        pages.Add(Utils.GetInstance().Embedify($"/booklist - {idealVersion.Name}", $"### {localizer["BookListDEUHeader"]}\n* " + string.Join("\n* ", deuNames.Values), false));
                    }

                    return new CommandResponse
                    {
                        OK = true,
                        Pages = pages,
                        LogStatement = "/booklist"
                    };
                }

                string message = $"{localizer["BookListInternalError"]}\n\n" +
                $"```\nVersion: {idealVersion.Abbreviation}\n```";

                return new CommandResponse
                {
                    OK = false,
                    Pages =
                    [
                        Utils.GetInstance().Embedify("/booklist", message, true)
                    ],
                    LogStatement = $"/booklist - internal error on {idealVersion.Abbreviation}",
                    Culture = CultureInfo.CurrentUICulture.Name
                };

            }
        }
    }
}
