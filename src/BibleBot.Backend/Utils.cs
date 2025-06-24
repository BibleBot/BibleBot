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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using BibleBot.Backend.InternalModels;
using BibleBot.Models;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Sentry;
using Serilog;
using Serilog.Extensions.Logging;

namespace BibleBot.Backend
{
    public class Utils
    {
        private static Utils _instance;
        private static readonly Lock _lock = new();
        private readonly IStringLocalizer _localizer;

        private Utils()
        {
            ResourceManagerStringLocalizerFactory localizerFactory = new(Options.Create(new LocalizationOptions { ResourcesPath = "Resources" }), new SerilogLoggerFactory());
            _localizer = localizerFactory.Create(typeof(SharedResource));

            _instance = this;
        }

        public static Utils GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new Utils();
                }
            }

            return _instance;
        }

        private static readonly string[] _creeds = ["apostles", "nicene"];

        private enum Colors
        {
            NORMAL_COLOR = 6709986,
            ERROR_COLOR = 16723502
        }

        private static readonly string _buildConfiguration = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production" ? "Release" : "Debug";
        private static readonly string _gitInfoPath = Path.GetFullPath($"{Directory.GetCurrentDirectory()}/obj/{_buildConfiguration}/net9.0/GitInfo.cache");
        private static StreamReader _gitInfoReader;
        private static string _cachedVersion;

        public static string GetVersion()
        {
            if (_cachedVersion != null)
            {
                return _cachedVersion;
            }

            try
            {
                _gitInfoReader = new StreamReader(_gitInfoPath);

                while (_gitInfoReader.ReadLine() is { } line)
                {
                    if (!line.Contains("GitBaseVersion="))
                    {
                        continue;
                    }

                    string version = line.Split("=")[1];
                    _cachedVersion = $"v{version[..^1]}";
                }


                return _cachedVersion;
            }
            catch
            {
                Log.Warning("unable to fetch GitInfo.cache for version information, version is now undefined");
                return "undefined";
            }
        }

        public static readonly string Version = GetVersion();

        public InternalEmbed Embedify(string title, string description, bool isError) => Embedify(null, title, description, isError, null);

        public InternalEmbed Embedify(string author, string title, string description, bool isError, string copyright)
        {
            string footerText = string.Format(_localizer["GlobalFooter"], Version);

            InternalEmbed embed = new()
            {
                Title = title,
                Color = isError ? (uint)Colors.ERROR_COLOR : (uint)Colors.NORMAL_COLOR,

                Footer = new Footer
                {
                    Text = copyright != null ? $"{copyright}\n{footerText}" : footerText,
                    IconURL = "https://i.imgur.com/hr4RXpy.png"
                }
            };

            if (description != null)
            {
                embed.Description = description;
            }

            if (author != null)
            {
                embed.Author = new Author
                {
                    Name = author
                };
            }

            return embed;
        }

        public List<InternalEmbed> EmbedifyResource(IResource resource, string section)
        {
            switch (resource.Style)
            {
                case ResourceStyle.FULL_TEXT when resource.Type == ResourceType.CREED:
                    {
                        var creedResource = resource as CreedResource;
                        string copyright = null;

                        if (_creeds.Contains(creedResource!.CommandReference))
                        {
                            copyright = "© 1998 English Language Liturgical Consultation (ELLC)";
                        }

                        return
                        [
                            Embedify(null, creedResource.Title, creedResource.Text, false, copyright)
                        ];
                    }
                case ResourceStyle.PARAGRAPHED:
                    {
                        var pgResource = resource as ParagraphedResource;
                        int sectionAsIndex;

                        try
                        {
                            sectionAsIndex = int.Parse(section);
                        }
                        catch
                        {
                            sectionAsIndex = 0;
                        }

                        if (sectionAsIndex == 0)
                        {
                            if (!section.Contains('-'))
                            {
                                return
                                [
                                    CreateTitlePage(pgResource!.Author, pgResource.Title, pgResource.Category, pgResource.Copyright, pgResource.ImageRef, null)
                                ];
                            }

                            string[] sectionRange = [.. section.Split("-").Where(item => item.Length > 0)];
                            int firstPart = 0;
                            int secondPart = 0;

                            List<InternalEmbed> results = [];

                            switch (sectionRange.Length)
                            {
                                case 2:
                                    try
                                    {
                                        firstPart = int.Parse(sectionRange[0]);
                                        secondPart = int.Parse(sectionRange[1]);
                                    }
                                    catch
                                    {
                                        firstPart = 0;
                                        secondPart = 0;
                                    }
                                    break;
                                case 1:
                                    try
                                    {
                                        firstPart = int.Parse(sectionRange[0]);
                                        secondPart = firstPart;
                                    }
                                    catch
                                    {
                                        firstPart = 0;
                                        secondPart = 0;
                                    }
                                    break;
                                default:
                                    break;
                            }

                            if (firstPart > 0 && secondPart > 0)
                            {
                                if (firstPart < secondPart)
                                {
                                    for (int i = firstPart; i < secondPart + 1; i++)
                                    {
                                        // TODO: should follow verse numbers preference?
                                        results.Add(Embedify(null, $"{pgResource!.Title} - Paragraph {i}", $"<**{i}**> {pgResource.Paragraphs.ElementAt(i - 1).Text}", false, pgResource.Copyright));
                                    }
                                }
                                else if (firstPart == secondPart)
                                {
                                    results.Add(Embedify(null, $"{pgResource!.Title} - Paragraph {firstPart}", $"<**{firstPart}**> {pgResource.Paragraphs.ElementAt(firstPart - 1).Text}", false, pgResource.Copyright));
                                }
                                else
                                {
                                    results.Add(CreateTitlePage(pgResource!.Author, pgResource.Title, pgResource.Category, pgResource.Copyright, pgResource.ImageRef, null));
                                }
                            }
                            else
                            {
                                results.Add(CreateTitlePage(pgResource!.Author, pgResource.Title, pgResource.Category, pgResource.Copyright, pgResource.ImageRef, null));
                            }

                            return results;

                        }

                        return pgResource.Paragraphs.Count >= sectionAsIndex
                            ? [Embedify(null, $"{pgResource!.Title} - Paragraph {sectionAsIndex}", $"<**{sectionAsIndex}**> {pgResource.Paragraphs.ElementAt(sectionAsIndex - 1).Text}", false, pgResource.Copyright)]
                            : null;
                    }
                case ResourceStyle.SECTIONED:
                    {
                        var sResource = resource as SectionedResource;
                        int sectionAsIndex;

                        try
                        {
                            sectionAsIndex = int.Parse(section);
                        }
                        catch
                        {
                            sectionAsIndex = 0;
                        }

                        if (sectionAsIndex == 0)
                        {
                            Section matchingSection = sResource!.Sections.FirstOrDefault(sct => sct.Slugs.Contains(section));

                            if (matchingSection == null)
                            {
                                return
                                [
                                    CreateTitlePage(sResource.Author, sResource.Title, sResource.Category, sResource.Copyright, sResource.ImageRef, sResource.Sections)
                                ];
                            }

                            string title = $"{matchingSection.Title}";

                            List<InternalEmbed> results = [];
                            results.AddRange(matchingSection.Pages.Select((t, i) => Embedify(sResource.Title, $"{title} (Page {i + 1} of {matchingSection.Pages.Count})", matchingSection.Pages.ElementAt(i), false, sResource.Copyright)));

                            return results;

                        }
                        else
                        {
                            Section matchingSection = sResource!.Sections.ElementAtOrDefault(sectionAsIndex - 1);

                            if (matchingSection == null)
                            {
                                return
                                [
                                    CreateTitlePage(sResource.Author, sResource.Title, sResource.Category, sResource.Copyright, sResource.ImageRef, sResource.Sections)
                                ];
                            }

                            string title = $"{matchingSection.Title}";

                            List<InternalEmbed> results = [];
                            results.AddRange(matchingSection.Pages.Select((t, i) => Embedify(sResource.Title, $"{title} (Page {i + 1} of {matchingSection.Pages.Count})", matchingSection.Pages.ElementAt(i), false, sResource.Copyright)));

                            return results;

                        }
                    }
                case ResourceStyle.FULL_TEXT:
                default:
                    return null;
            }

        }

        private InternalEmbed CreateTitlePage(string author, string title, string category, string copyright, string imageRef, List<Section> sections)
        {
            StringBuilder categoryText = new();

            foreach (string cat in category.Split("."))
            {
                categoryText.Append($"{CultureInfo.InvariantCulture.TextInfo.ToTitleCase(cat)} > ");
            }

            InternalEmbed embed = Embedify(null, title, null, false, copyright);

            embed.Fields =
            [
                new EmbedField
                {
                    Name = "Author",
                    Value = author,
                    Inline = false
                },
                new EmbedField
                {
                    Name = "Category",
                    Value = categoryText.ToString()[..(categoryText.Length - 3)],
                    Inline = false
                }
            ];


            if (imageRef != null)
            {
                embed.Thumbnail = new Media
                {
                    URL = $"https://i.imgur.com/{imageRef}.png"
                };
            }

            if (sections is not { Count: > 0 })
            {
                return embed;
            }

            StringBuilder sectionList = new();

            for (int i = 0; i < sections.Count; i++)
            {
                Section section = sections[i];

                sectionList.Append($"{i + 1}. {section.Title} ({(section.Pages.Count > 1 ? $"{section.Pages.Count} pages" : $"{section.Pages.Count} page")})" +
                $" [`{section.Slugs[0]}`]\n");
            }

            embed.Fields.Add(new EmbedField
            {
                Name = "Sections",
                Value = sectionList.ToString(),
                Inline = false
            });

            return embed;
        }

        public StringBuilder[] PermissionsChecker(long selfPermissionsInChannel, long rolePermissionsInChannel, long rolePermissionsInGuild)
        {
            Permissions[] permissionsToCheck = [
                Permissions.VIEW_CHANNEL,
                Permissions.SEND_MESSAGES,
                Permissions.SEND_MESSAGES_IN_THREADS,
                Permissions.ADD_REACTIONS,
                Permissions.EMBED_LINKS,
                Permissions.READ_MESSAGE_HISTORY,
                Permissions.MANAGE_MESSAGES,
                Permissions.MANAGE_WEBHOOKS,
                Permissions.USE_APPLICATION_COMMANDS,
                Permissions.USE_EXTERNAL_EMOJIS
            ];

            StringBuilder selfChannelPermissionsList = new();
            StringBuilder roleChannelPermissionsList = new();
            StringBuilder roleGuildPermissionsList = new();
            foreach (Permissions perm in permissionsToCheck)
            {
                string permName = Enum.GetName(perm);

                if ((selfPermissionsInChannel & (long)perm) == (long)perm)
                {
                    selfChannelPermissionsList.Append($"{permName}: :white_check_mark:\n");
                }
                else
                {
                    selfChannelPermissionsList.Append($"{permName}: :x:\n");
                }

                if ((rolePermissionsInChannel & (long)perm) == (long)perm)
                {
                    roleChannelPermissionsList.Append($"{permName}: :white_check_mark:\n");
                }
                else
                {
                    roleChannelPermissionsList.Append($"{permName}: :x:\n");
                }

                if (permName == "VIEW_CHANNEL")
                {
                    continue;
                }

                if ((rolePermissionsInGuild & (long)perm) == (long)perm)
                {
                    roleGuildPermissionsList.Append($"{permName}: :white_check_mark:\n");
                }
                else
                {
                    roleGuildPermissionsList.Append($"{permName}: :x:\n");
                }
            }

            return [selfChannelPermissionsList, roleChannelPermissionsList, roleGuildPermissionsList];
        }

        public Reference ConvertReferenceToSeptuagintNumbering(Reference reference) => ConvertReferencesToSeptuagintNumbering([reference]).FirstOrDefault();
        public List<Reference> ConvertReferencesToSeptuagintNumbering(List<Reference> references)
        {
            // Just a note about this function. This only really concerns itself with Psalms with references that are contained within one Masoretic-numbered chapter.
            // This makes no provisions for references like 1:1-2:2.
            List<Reference> referencesToReturn = [];

            foreach (Reference reference in references)
            {
                if (reference.Book.Name != "PSA")
                {
                    // We currently only support conversions for the Psalms.
                    continue;
                }

                int startingChapter = reference.StartingChapter switch
                {
                    9 or 10 => 9,
                    (>= 11 and <= 113) or (>= 117 and <= 146) => reference.StartingChapter - 1,
                    114 or 115 => 113,
                    // 116:1-9, 116:10-19, 147:1-11, 147:12-20 are handled outside of this
                    _ => reference.StartingChapter
                };

                int endingChapter = reference.EndingChapter switch
                {
                    9 or 10 => 9,
                    (>= 11 and <= 113) or (>= 117 and <= 146) => reference.EndingChapter - 1,
                    114 or 115 => 113,
                    // 116:1-9, 116:10-19, 147:1-11, 147:12-20 are handled outside of this
                    _ => reference.EndingChapter
                };

                int startingVerse = reference.StartingVerse;
                int endingVerse = reference.EndingVerse;

                if (reference.StartingChapter == startingChapter && startingChapter != 9 && startingChapter < 148)
                {
                    // This is one of the special cases we can't easily fulfill in the switch cases.
                    if (reference.StartingChapter == 116 && reference.EndingChapter == 116)
                    {
                        if (reference.StartingVerse is >= 1 and <= 9 && reference.EndingVerse is >= 1 and <= 9)
                        {
                            startingChapter = 114;
                            endingChapter = 114;
                        }
                        else if (reference.StartingVerse is >= 10 and <= 19 && reference.EndingVerse is >= 10 and <= 19)
                        {
                            startingChapter = 115;
                            endingChapter = 115;

                            startingVerse -= 9;
                            endingVerse -= 9;
                        }
                    }
                    else if (reference.StartingChapter == 147 && reference.EndingChapter == 147)
                    {
                        if (reference.StartingVerse is >= 1 and <= 11 && reference.EndingVerse is >= 1 and <= 11)
                        {
                            startingChapter = 146;
                            endingChapter = 146;
                        }
                        else if (reference.StartingVerse is >= 10 and <= 19 && reference.EndingVerse is >= 10 and <= 19)
                        {
                            startingChapter = 147;
                            endingChapter = 147;

                            startingVerse -= 11;
                            endingVerse -= 11;
                        }
                    }
                    else
                    {
                        try
                        {
                            throw new NotImplementedException($"ConvertReferencesToSeptuagintNumbering: did not account for chapter {reference.StartingChapter}");
                        }
                        catch (NotImplementedException ex)
                        {
                            SentrySdk.CaptureException(ex);
                            continue;
                        }
                    }
                }

                reference.StartingChapter = startingChapter;
                reference.EndingChapter = endingChapter;

                reference.StartingVerse = startingVerse;
                reference.EndingVerse = endingVerse;

                referencesToReturn.Add(reference);
            }

            return referencesToReturn;
        }
    }
}
