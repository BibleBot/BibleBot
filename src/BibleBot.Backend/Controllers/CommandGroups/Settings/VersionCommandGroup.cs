/*
* Copyright (C) 2016-2023 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BibleBot.Backend.Services;
using BibleBot.Models;
using MongoDB.Driver;

namespace BibleBot.Backend.Controllers.CommandGroups.Settings
{
    public class VersionCommandGroup : ICommandGroup
    {
        public string Name { get; set; }
        public bool IsStaffOnly { get; set; }
        public ICommand DefaultCommand { get; set; }
        public List<ICommand> Commands { get; set; }

        private readonly UserService _userService;
        private readonly GuildService _guildService;
        private readonly VersionService _versionService;
        private readonly NameFetchingService _nameFetchingService;

        public VersionCommandGroup(UserService userService, GuildService guildService, VersionService versionService, NameFetchingService nameFetchingService)
        {
            _userService = userService;
            _guildService = guildService;
            _versionService = versionService;
            _nameFetchingService = nameFetchingService;

            Name = "version";
            IsStaffOnly = false;
            Commands = new List<ICommand>
            {
                new VersionUsage(_userService, _guildService, _versionService),
                new VersionSet(_userService, _versionService),
                new VersionSetServer(_guildService, _versionService),
                new VersionInfo(_userService, _guildService, _versionService),
                new VersionList(_versionService),
                new VersionBookList(_userService, _guildService, _versionService, _nameFetchingService)
            };
            DefaultCommand = Commands.FirstOrDefault(cmd => cmd.Name == "usage");
        }

        public class VersionUsage : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }
            public bool BotAllowed { get; set; }

            private readonly UserService _userService;
            private readonly GuildService _guildService;
            private readonly VersionService _versionService;

            public VersionUsage(UserService userService, GuildService guildService, VersionService versionService)
            {
                Name = "usage";
                ArgumentsError = null;
                ExpectedArguments = 0;
                PermissionsRequired = null;
                BotAllowed = true;

                _userService = userService;
                _guildService = guildService;
                _versionService = versionService;
            }

            public async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                User idealUser = await _userService.Get(req.UserId);
                Guild idealGuild = await _guildService.Get(req.GuildId);

                Version defaultVersion = await _versionService.Get("RSV");

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
                    Version idealUserVersion = await _versionService.Get(idealUser.Version);

                    if (idealUserVersion != null)
                    {
                        response = response.Replace("<version>", idealUserVersion.Name);
                    }
                }

                if (idealGuild != null)
                {
                    Version idealGuildVersion = await _versionService.Get(idealGuild.Version);

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
                    Pages = new List<InternalEmbed>
                    {
                        Utils.GetInstance().Embedify("/version", response, false)
                    },
                    LogStatement = "/version"
                };
            }
        }

        public class VersionSet : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }
            public bool BotAllowed { get; set; }

            private readonly UserService _userService;
            private readonly VersionService _versionService;

            public VersionSet(UserService userService, VersionService versionService)
            {
                Name = "set";
                ArgumentsError = "Expected a version abbreviation parameter, like `RSV` or `KJV`.";
                ExpectedArguments = 1;
                PermissionsRequired = null;
                BotAllowed = false;

                _userService = userService;
                _versionService = versionService;
            }

            public async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                Version idealVersion = await _versionService.Get(args[0]);

                if (idealVersion != null)
                {
                    User idealUser = await _userService.Get(req.UserId);

                    if (idealUser != null)
                    {
                        UpdateDefinition<User> update = Builders<User>.Update
                                     .Set(user => user.Version, idealVersion.Abbreviation);

                        await _userService.Update(req.UserId, update);
                    }
                    else
                    {
                        await _userService.Create(new User
                        {
                            UserId = req.UserId,
                            Version = idealVersion.Abbreviation,
                            InputMethod = "default",
                            Language = "english_us",
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
                            Utils.GetInstance().Embedify("/setversion", "Set version successfully.", false)
                        },
                        LogStatement = $"/setversion {args[0]}"
                    };
                }

                return new CommandResponse
                {
                    OK = false,
                    Pages = new List<InternalEmbed>
                    {
                        Utils.GetInstance().Embedify("/setversion", "Failed to set version, see `/listversions`.", true)
                    },
                    LogStatement = "/setversion"
                };
            }
        }

        public class VersionSetServer : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }
            public bool BotAllowed { get; set; }

            private readonly GuildService _guildService;
            private readonly VersionService _versionService;

            public VersionSetServer(GuildService guildService, VersionService versionService)
            {
                Name = "setserver";
                ArgumentsError = "Expected a version abbreviation parameter, like `RSV` or `KJV`.";
                ExpectedArguments = 1;
                PermissionsRequired = new List<Permissions>
                {
                    Permissions.MANAGE_GUILD
                };
                BotAllowed = false;

                _guildService = guildService;
                _versionService = versionService;
            }

            public async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                Version idealVersion = await _versionService.Get(args[0]);

                if (idealVersion != null)
                {
                    Guild idealGuild = await _guildService.Get(req.GuildId);

                    if (idealGuild != null)
                    {
                        UpdateDefinition<Guild> update = Builders<Guild>.Update
                                     .Set(guild => guild.Version, idealVersion.Abbreviation);

                        await _guildService.Update(req.GuildId, update);
                    }
                    else
                    {
                        await _guildService.Create(new Guild
                        {
                            GuildId = req.GuildId,
                            Version = idealVersion.Abbreviation,
                            Language = "english_us",
                            Prefix = "+",
                            DisplayStyle = "embed",
                            IgnoringBrackets = "<>",
                            IsDM = req.IsDM
                        });
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
                        Pages = new List<InternalEmbed>
                        {
                            Utils.GetInstance().Embedify("/setserverversion", message, false)
                        },
                        LogStatement = $"/setserverversion {args[0]}"
                    };
                }

                return new CommandResponse
                {
                    OK = false,
                    Pages = new List<InternalEmbed>
                    {
                        Utils.GetInstance().Embedify("/setserverversion", "Failed to set server version, see `/listversions`.", true)
                    },
                    LogStatement = "/setserverversion"
                };
            }
        }

        public class VersionInfo : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }
            public bool BotAllowed { get; set; }

            private readonly UserService _userService;
            private readonly GuildService _guildService;
            private readonly VersionService _versionService;

            public VersionInfo(UserService userService, GuildService guildService, VersionService versionService)
            {
                Name = "info";
                ArgumentsError = "Expected a version abbreviation parameter, like `RSV` or `KJV`.";
                ExpectedArguments = 0;
                PermissionsRequired = null;
                BotAllowed = true;

                _userService = userService;
                _guildService = guildService;
                _versionService = versionService;
            }

            public async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                User idealUser = await _userService.Get(req.UserId);
                Guild idealGuild = await _guildService.Get(req.GuildId);

                string version;

                if (args.Count() > 0)
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

                Version idealVersion = await _versionService.Get(version);

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
                        Pages = new List<InternalEmbed>
                            {
                                Utils.GetInstance().Embedify("/versioninfo", response, false)
                            },
                        LogStatement = $"/versioninfo {version}"
                    };
                }

                return new CommandResponse
                {
                    OK = false,
                    Pages = new List<InternalEmbed>
                    {
                        Utils.GetInstance().Embedify("/versioninfo", "I couldn't find that version, are you sure you used the right acronym?", true)
                    },
                    LogStatement = "/versioninfo"
                };
            }
        }

        public class VersionList : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }
            public bool BotAllowed { get; set; }

            private readonly VersionService _versionService;

            public VersionList(VersionService versionService)
            {
                Name = "list";
                ArgumentsError = null;
                ExpectedArguments = 0;
                PermissionsRequired = null;
                BotAllowed = false;

                _versionService = versionService;
            }

            public async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                List<Version> versions = await _versionService.Get();
                versions.Sort((x, y) => x.Name.CompareTo(y.Name));

                List<string> versionsUsed = new();
                List<InternalEmbed> pages = new();

                int maxResultsPerPage = 25;

                // We need to add a page here because the for loop won't hit the last one otherwise.
                // This also prevents situations where the Ceiling() result might equal 0.
                int totalPages = (int)System.Math.Ceiling((decimal)(versions.Count / maxResultsPerPage)) + 1;

                for (int i = 0; i < totalPages; i++)
                {
                    int count = 0;
                    StringBuilder versionList = new();

                    foreach (Version version in versions)
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

        public class VersionBookList : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }
            public bool BotAllowed { get; set; }

            private readonly UserService _userService;
            private readonly GuildService _guildService;
            private readonly VersionService _versionService;
            private readonly NameFetchingService _nameFetchingService;

            public VersionBookList(UserService userService, GuildService guildService, VersionService versionService, NameFetchingService nameFetchingService)
            {
                Name = "booklist";
                ArgumentsError = "Expected a version abbreviation parameter, like `RSV` or `KJV`.";
                ExpectedArguments = 0;
                PermissionsRequired = null;
                BotAllowed = false;

                _userService = userService;
                _guildService = guildService;
                _versionService = versionService;
                _nameFetchingService = nameFetchingService;
            }

            public async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                User idealUser = await _userService.Get(req.UserId);
                Guild idealGuild = await _guildService.Get(req.GuildId);

                string version;

                if (args.Count() > 0)
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

                Version idealVersion = await _versionService.Get(version);

                if (idealVersion != null)
                {
                    if (idealVersion.Source != "bg")
                    {
                        return new CommandResponse
                        {
                            OK = false,
                            Pages = new List<InternalEmbed>
                            {
                                Utils.GetInstance().Embedify("/booklist", "This version is not eligible for this command yet. Make sure you're using a version of source `bg` (see `/versioninfo`).", true)
                            },
                            LogStatement = "/booklist - non-bg source"
                        };
                    }

                    Dictionary<BookCategories, Dictionary<string, string>> names = await _nameFetchingService.GetBibleGatewayVersionBookList(idealVersion);

                    if (names != null)
                    {
                        List<InternalEmbed> pages = new();

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
                            Pages = new List<InternalEmbed>
                            {
                                Utils.GetInstance().Embedify("/booklist", message, true)
                            },
                            LogStatement = $"/booklist - internal error on {idealVersion.Abbreviation}"
                        };
                    }
                }

                return new CommandResponse
                {
                    OK = false,
                    Pages = new List<InternalEmbed>
                    {
                        Utils.GetInstance().Embedify("/booklist", "I couldn't find that version, are you sure you used the right acronym?", true)
                    },
                    LogStatement = "/booklist - invalid version"
                };
            }
        }
    }
}