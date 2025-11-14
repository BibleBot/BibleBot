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
using BibleBot.Backend.Models;
using BibleBot.Backend.Services;
using BibleBot.Models;
using Microsoft.Extensions.Localization;
using MongoDB.Driver;
using Serilog;
using MDVersionBookList = System.Collections.Generic.Dictionary<BibleBot.Models.BookCategories, System.Collections.Generic.Dictionary<string, string>>;
using Version = BibleBot.Models.Version;

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
                (idealVersion.AliasOf != null ? $"{string.Format(localizer["VersionInfoAlias"], idealVersion.AliasOf)}\n\n" : "") +
                $"{localizer["VersionInfoContainsOT"]}: {(idealVersion.SupportsOldTestament ? Utils.GetInstance().emoji["check_emoji"] : Utils.GetInstance().emoji["xmark_emoji"])}\n" +
                $"{localizer["VersionInfoContainsNT"]}: {(idealVersion.SupportsNewTestament ? Utils.GetInstance().emoji["check_emoji"] : Utils.GetInstance().emoji["xmark_emoji"])}\n" +
                $"{localizer["VersionInfoContainsDEU"]}: {(idealVersion.SupportsDeuterocanon ? Utils.GetInstance().emoji["check_emoji"] : Utils.GetInstance().emoji["xmark_emoji"])}\n\n" +
                $"__**{localizer["VersionInfoDeveloperInfoHeader"]}**__\n" +
                $"{localizer["VersionInfoSource"]}: `{idealVersion.Source}`";

                if (idealVersion.Publisher != null)
                {
                    response += $"\n{localizer["VersionInfoPublisher"]}: `{idealVersion.Publisher}`";
                }

                if (idealVersion.InternalId != null)
                {
                    response += $"\n{localizer["VersionInfoInternalId"]}: `{idealVersion.InternalId}`";
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
                bool isByLanguage = args.Count == 1 && args[0] == "language";

                List<Version> versions = await versionService.Get();

                if (!isByLanguage)
                {
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
                else
                {
                    SortedDictionary<string, List<string>> localesToVersionBeforeEnglish = new(StringComparer.Ordinal);
                    List<string> englishVersions = [];
                    SortedDictionary<string, List<string>> localesToVersionAfterEnglish = new(StringComparer.Ordinal);

                    versions.ForEach((version) =>
                    {
                        string localeDisplayName = null;
                        bool shouldSkip = false;

                        try
                        {
                            localeDisplayName = CultureInfo.GetCultureInfo(version.Locale).DisplayName.Split(" (")[0];
                        }
                        catch (CultureNotFoundException)
                        {
                            localeDisplayName = version.Locale;
                        }
                        catch (ArgumentNullException)
                        {
                            Log.Error($"[err] {version.Abbreviation} does not have a locale");
                            shouldSkip = true;
                        }

                        if (!shouldSkip)
                        {
                            if (localeDisplayName == "cak")
                            {
                                localeDisplayName = "Kaqchikel";
                            }

                            if (localeDisplayName == "English")
                            {
                                englishVersions.Add(version.Name);
                            }

                            if (string.CompareOrdinal(localeDisplayName, "English") < 0)
                            {
                                if (localesToVersionBeforeEnglish.ContainsKey(localeDisplayName))
                                {
                                    if (!localesToVersionBeforeEnglish[localeDisplayName].Contains(version.Name))
                                    {
                                        localesToVersionBeforeEnglish[localeDisplayName].Add(version.Name);
                                    }
                                }
                                else
                                {
                                    localesToVersionBeforeEnglish[localeDisplayName] = [version.Name];
                                }
                            }
                            else if (string.CompareOrdinal(localeDisplayName, "English") > 0)
                            {
                                if (localesToVersionAfterEnglish.ContainsKey(localeDisplayName))
                                {
                                    if (!localesToVersionAfterEnglish[localeDisplayName].Contains(version.Name))
                                    {
                                        localesToVersionAfterEnglish[localeDisplayName].Add(version.Name);
                                    }
                                }
                                else
                                {
                                    localesToVersionAfterEnglish[localeDisplayName] = [version.Name];
                                }
                            }
                        }
                    });

                    List<string> localeDisplayNamesProcessed = [];
                    List<InternalEmbed> pages = [];

                    const int maxResultsPerPage = 5; // TODO: make this an appsettings param

                    // Separate page counts for before/after groups so English can be inserted between them
                    int beforeCount = localesToVersionBeforeEnglish.Keys.Count;
                    int afterCount = localesToVersionAfterEnglish.Keys.Count;

                    int pagesForBefore = (int)Math.Ceiling(decimal.Divide(beforeCount, maxResultsPerPage));
                    int pagesForAfter = (int)Math.Ceiling(decimal.Divide(afterCount, maxResultsPerPage));

                    // English versions are always present; reserve one dedicated page for English.
                    int totalPages = pagesForBefore + 1 + pagesForAfter;

                    // Build pages for the "before English" locale groups first.
                    for (int i = 0; i < pagesForBefore; i++)
                    {
                        int count = 0;
                        StringBuilder versionList = new();

                        foreach (KeyValuePair<string, List<string>> kvp in localesToVersionBeforeEnglish.Where(kvp => count < maxResultsPerPage).Where(kvp => !localeDisplayNamesProcessed.Contains(kvp.Key)))
                        {
                            kvp.Value.Sort(StringComparer.Ordinal);
                            versionList.Append($"__**{kvp.Key}**__\n{string.Join("\n", kvp.Value)}\n\n");
                            localeDisplayNamesProcessed.Add(kvp.Key);
                            count++;
                        }

                        int pageIndex = i + 1; // before pages start at 1
                        string pageCounter = string.Format(sharedLocalizer["PageCounter"], [pageIndex, totalPages]);
                        InternalEmbed embed = Utils.GetInstance().Embedify($"/listversions - {pageCounter}", versionList.ToString(), false);
                        pages.Add(embed);
                    }

                    // Insert a dedicated English page after the before-English pages (always its own page).
                    englishVersions.Sort(StringComparer.Ordinal);
                    int englishPageIndex = pagesForBefore + 1;
                    string englishPageCounter = string.Format(sharedLocalizer["PageCounter"], [englishPageIndex, totalPages]);
                    InternalEmbed englishEmbed = Utils.GetInstance().Embedify($"/listversions - {englishPageCounter}", $"__**English**__\n{string.Join("\n", englishVersions)}\n\n", false);
                    pages.Add(englishEmbed);

                    // Build pages for the "after English" locale groups.
                    for (int i = 0; i < pagesForAfter; i++)
                    {
                        int count = 0;
                        StringBuilder versionList = new();

                        foreach (KeyValuePair<string, List<string>> kvp in localesToVersionAfterEnglish.Where(kvp => count < maxResultsPerPage).Where(kvp => !localeDisplayNamesProcessed.Contains(kvp.Key)))
                        {
                            kvp.Value.Sort(StringComparer.Ordinal);
                            versionList.Append($"__**{kvp.Key}**__\n{string.Join("\n", kvp.Value)}\n\n");
                            localeDisplayNamesProcessed.Add(kvp.Key);
                            count++;
                        }

                        int pageIndex = pagesForBefore + i + 2;
                        string pageCounterAfter = string.Format(sharedLocalizer["PageCounter"], [pageIndex, totalPages]);
                        InternalEmbed embed = Utils.GetInstance().Embedify($"/listversions - {pageCounterAfter}", versionList.ToString(), false);
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

                if (idealVersion.AliasOf != null)
                {
                    idealVersion = await versionService.Get(idealVersion.AliasOf);
                }

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
