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
using BibleBot.Backend.Services;
using BibleBot.Models;
using Microsoft.Extensions.Localization;
using MongoDB.Driver;

namespace BibleBot.Backend.Controllers.CommandGroups.Settings
{
    public class VersionCommandGroup(UserService userService, GuildService guildService, VersionService versionService, NameFetchingService nameFetchingService, IStringLocalizerFactory localizerFactory) : CommandGroup
    {
        private readonly IStringLocalizer _localizer = localizerFactory.Create(typeof(VersionCommandGroup));

        public override string Name { get => "version"; set => throw new NotImplementedException(); }
        public override Command DefaultCommand { get => Commands.FirstOrDefault(cmd => cmd.Name == "usage"); set => throw new NotImplementedException(); }
        public override List<Command> Commands
        {
            get => [
                new VersionUsage(userService, guildService, versionService, _localizer),
                new VersionSet(userService, versionService, _localizer),
                new VersionSetServer(guildService, versionService, _localizer),
                new VersionInfo(userService, guildService, versionService, _localizer),
                new VersionList(versionService, _localizer),
                new VersionBookList(userService, guildService, versionService, nameFetchingService, _localizer)
            ]; set => throw new NotImplementedException();
        }

        public class VersionUsage(UserService userService, GuildService guildService, VersionService versionService, IStringLocalizer localizer) : Command
        {
            public override string Name { get => "usage"; set => throw new NotImplementedException(); }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                User idealUser = await userService.Get(req.UserId);
                Guild idealGuild = await guildService.Get(req.GuildId);

                Models.Version defaultVersion = await versionService.Get("RSV");

                string response = "Your preferred version is set to **<version>**.\n" +
                               "The server's preferred version is set to **<gversion>**.\n\n" +
                               "__**Related Commands**__\n" +
                               "**/setversion** - set your preferred version\n" +
                               "**/setserverversion** - set the server's default version (staff only)\n" +
                               "**/versioninfo** - get information on a version\n" +
                               "**/listversions** - list all available versions\n" +
                               "**/booklist** - list all available books";

                if (idealUser != null)
                {
                    Models.Version idealUserVersion = await versionService.Get(idealUser.Version);

                    if (idealUserVersion != null)
                    {
                        response = response.Replace("<version>", idealUserVersion.Name);
                    }
                }

                if (idealGuild != null)
                {
                    Models.Version idealGuildVersion = await versionService.Get(idealGuild.Version);

                    if (idealGuildVersion != null)
                    {
                        response = response.Replace("<gversion>", idealGuildVersion.Name);
                    }
                }

                response = response.Replace("<version>", defaultVersion.Name);
                response = response.Replace("<gversion>", defaultVersion.Name);

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
                            Utils.GetInstance().Embedify("/setversion", "Set version successfully.", false)
                        ],
                        LogStatement = $"/setversion {args[0]}"
                    };
                }

                return new CommandResponse
                {
                    OK = false,
                    Pages =
                    [
                        Utils.GetInstance().Embedify("/setversion", "Failed to set version, see `/listversions`.", true)
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

                    string message = "Set server version successfully.";
                    if (!idealVersion.SupportsOldTestament || !idealVersion.SupportsNewTestament)
                    {
                        message += "\n\n" +
                        ":warning: This version will not work with automatic daily verses as it does not support both the Old and New Testaments. If this is a concern to you, use `/versioninfo` to help you identify versions that support both.";
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
                        Utils.GetInstance().Embedify("/setserverversion", "Failed to set server version, see `/listversions`.", true)
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
                                $"Contains Old Testament: {(idealVersion.SupportsOldTestament ? ":white_check_mark:" : ":x:")}\n" +
                                $"Contains New Testament: {(idealVersion.SupportsNewTestament ? ":white_check_mark:" : ":x:")}\n" +
                                $"Contains Apocrypha/Deuterocanon: {(idealVersion.SupportsDeuterocanon ? ":white_check_mark:" : ":x:")}\n" +
                                $"Source (mainly for developers): `{idealVersion.Source}`";


                    if (!idealVersion.SupportsOldTestament || !idealVersion.SupportsNewTestament)
                    {
                        response += "\n\n:warning: This version will not work with automatic daily verses.";
                    }

                    if (idealVersion.Source != "bg")
                    {
                        response += "\n\nSee `/booklist` for a list of available books.";
                    }

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
                        Utils.GetInstance().Embedify("/versioninfo", "I couldn't find that version, are you sure you used the right acronym?", true)
                    ],
                    LogStatement = "/versioninfo"
                };
            }
        }

        public class VersionList(VersionService versionService, IStringLocalizer localizer) : Command
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

                    InternalEmbed embed = Utils.GetInstance().Embedify($"/listversions - Page {i + 1} of {totalPages}", versionList.ToString(), false);
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
                    if (idealVersion.Source != "bg")
                    {
                        return new CommandResponse
                        {
                            OK = false,
                            Pages =
                            [
                                Utils.GetInstance().Embedify("/booklist", "This version is not eligible for this command yet. Make sure you're using a version of source `bg` (see `/versioninfo`).", true)
                            ],
                            LogStatement = "/booklist - non-bg source"
                        };
                    }

                    Dictionary<BookCategories, Dictionary<string, string>> names = await nameFetchingService.GetBibleGatewayVersionBookList(idealVersion);

                    if (names != null)
                    {
                        List<InternalEmbed> pages = [];

                        if (names.ContainsKey(BookCategories.OldTestament))
                        {
                            pages.Add(Utils.GetInstance().Embedify($"/booklist - {idealVersion.Name}", "### Old Testament\n* " + string.Join("\n* ", names[BookCategories.OldTestament].Values).Replace("<151>", "*(contains Psalm 151)*"), false));
                        }

                        if (names.ContainsKey(BookCategories.NewTestament))
                        {
                            pages.Add(Utils.GetInstance().Embedify($"/booklist - {idealVersion.Name}", "### New Testament\n* " + string.Join("\n* ", names[BookCategories.NewTestament].Values), false));
                        }

                        if (names.ContainsKey(BookCategories.Deuterocanon))
                        {
                            pages.Add(Utils.GetInstance().Embedify($"/booklist - {idealVersion.Name}", "### Apocrypha/Deuterocanon\n* " + string.Join("\n* ", names[BookCategories.Deuterocanon].Values), false));
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
                        string message = "We encountered an internal error. " +
                        "Please report this to the support server (https://biblebot.xyz/discord) or make a bug report (https://biblebot.xyz/bugreport) with the following information:\n\n" +
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
                        Utils.GetInstance().Embedify("/booklist", "I couldn't find that version, are you sure you used the right acronym?", true)
                    ],
                    LogStatement = "/booklist - invalid version"
                };
            }
        }
    }
}