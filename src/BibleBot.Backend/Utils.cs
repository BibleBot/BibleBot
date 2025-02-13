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
using System.Threading;
using BibleBot.Backend.InternalModels;
using BibleBot.Models;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Serilog.Extensions.Logging;

namespace BibleBot.Backend
{
    public class Utils
    {
        private static Utils _instance;
        private static readonly Lock _lock = new();
        private readonly IStringLocalizer _localizer;
        private readonly ResourceManagerStringLocalizerFactory _localizerFactory;

        public Utils()
        {
            _localizerFactory = new ResourceManagerStringLocalizerFactory(Options.Create(new LocalizationOptions { ResourcesPath = "Resources" }), new SerilogLoggerFactory());
            _localizer = _localizerFactory.Create(typeof(SharedResource));

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

        public enum Colors
        {
            NORMAL_COLOR = 6709986,
            ERROR_COLOR = 16723502
        }

        public static readonly string Version = ThisAssembly.Info.InformationalVersion.Split('+')[0];

        public InternalEmbed Embedify(string title, string description, bool isError) => Embedify(null, title, description, isError, null);

        public InternalEmbed Embedify(string author, string title, string description, bool isError, string copyright)
        {
            string footerText = string.Format(_localizer["GlobalFooter"], Version);

            InternalEmbed embed = new()
            {
                Title = title,
                Color = isError ? (uint)Colors.ERROR_COLOR : (uint)Colors.NORMAL_COLOR,

                Footer = new Footer()
            };

            embed.Footer.Text = copyright != null ? $"{copyright}\n{footerText}" : footerText;
            embed.Footer.IconURL = "https://i.imgur.com/hr4RXpy.png";

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
            if (resource.Style == ResourceStyle.FULL_TEXT)
            {
                if (resource.Type == ResourceType.CREED)
                {
                    var creedResource = resource as CreedResource;
                    string copyright = null;

                    if (_creeds.Contains(creedResource.CommandReference))
                    {
                        copyright = "© 1998 English Language Liturgical Consultation (ELLC)";
                    }


                    return
                    [
                        Embedify(null, creedResource.Title, creedResource.Text, false, copyright)
                    ];
                }
            }
            else if (resource.Style == ResourceStyle.PARAGRAPHED)
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
                    if (section.Contains('-'))
                    {
                        string[] sectionRange = [.. section.Split("-").Where(item => item.Length > 0)];
                        int firstPart = 0;
                        int secondPart = 0;

                        List<InternalEmbed> results = [];

                        if (sectionRange.Length == 2)
                        {
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
                        }
                        else if (sectionRange.Length == 1)
                        {
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
                        }

                        if (firstPart > 0 && secondPart > 0)
                        {
                            if (firstPart < secondPart)
                            {
                                for (int i = firstPart; i < secondPart + 1; i++)
                                {
                                    string title = $"{pgResource.Title} - Paragraph {i}";
                                    // TODO: should follow verse numbers preference?
                                    results.Add(Embedify(null, title, $"<**{i}**> {pgResource.Paragraphs.ElementAt(i - 1).Text}", false, pgResource.Copyright));
                                }
                            }
                            else if (firstPart == secondPart)
                            {
                                string title = $"{pgResource.Title} - Paragraph {firstPart}";
                                results.Add(Embedify(null, title, $"<**{firstPart}**> {pgResource.Paragraphs.ElementAt(firstPart - 1).Text}", false, pgResource.Copyright));
                            }
                            else
                            {
                                results.Add(CreateTitlePage(pgResource.Author, pgResource.Title, pgResource.Category, pgResource.Copyright, pgResource.ImageRef, null));
                            }
                        }
                        else
                        {
                            results.Add(CreateTitlePage(pgResource.Author, pgResource.Title, pgResource.Category, pgResource.Copyright, pgResource.ImageRef, null));
                        }

                        return results;
                    }
                    else
                    {
                        return
                        [
                            CreateTitlePage(pgResource.Author, pgResource.Title, pgResource.Category, pgResource.Copyright, pgResource.ImageRef, null)
                        ];
                    }
                }
                else
                {
                    string title = $"{pgResource.Title} - Paragraph {sectionAsIndex}";

                    return
                    [
                        Embedify(null, title, $"<**{sectionAsIndex}**> {pgResource.Paragraphs.ElementAt(sectionAsIndex - 1).Text}", false, pgResource.Copyright)
                    ];
                }
            }
            else if (resource.Style == ResourceStyle.SECTIONED)
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
                    Section matchingSection = sResource.Sections.FirstOrDefault(sct => sct.Slugs.Contains(section));

                    if (matchingSection != null)
                    {
                        string title = $"{matchingSection.Title}";
                        List<InternalEmbed> results = [];

                        for (int i = 0; i < matchingSection.Pages.Count; i++)
                        {
                            results.Add(Embedify(sResource.Title, $"{title} (Page {i + 1} of {matchingSection.Pages.Count})", matchingSection.Pages.ElementAt(i), false, sResource.Copyright));
                        }

                        return results;
                    }
                    else
                    {
                        return
                        [
                            CreateTitlePage(sResource.Author, sResource.Title, sResource.Category, sResource.Copyright, sResource.ImageRef, sResource.Sections)
                        ];
                    }
                }
                else
                {
                    Section matchingSection = sResource.Sections.ElementAtOrDefault(sectionAsIndex - 1);

                    if (matchingSection != null)
                    {
                        string title = $"{matchingSection.Title}";
                        List<InternalEmbed> results = [];

                        for (int i = 0; i < matchingSection.Pages.Count; i++)
                        {
                            results.Add(Embedify(sResource.Title, $"{title} (Page {i + 1} of {matchingSection.Pages.Count})", matchingSection.Pages.ElementAt(i), false, sResource.Copyright));
                        }

                        return results;
                    }
                    else
                    {
                        return
                        [
                            CreateTitlePage(sResource.Author, sResource.Title, sResource.Category, sResource.Copyright, sResource.ImageRef, sResource.Sections)
                        ];
                    }
                }
            }

            return null;
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
                new()
                {
                    Name = "Author",
                    Value = author,
                    Inline = false
                },
                new()
                {
                    Name = "Category",
                    Value = categoryText.ToString().Substring(0, categoryText.Length - 3),
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

            if (sections != null)
            {
                if (sections.Count > 0)
                {
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
                }
            }

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

                if (permName != "VIEW_CHANNEL")
                {
                    if ((rolePermissionsInGuild & (long)perm) == (long)perm)
                    {
                        roleGuildPermissionsList.Append($"{permName}: :white_check_mark:\n");
                    }
                    else
                    {
                        roleGuildPermissionsList.Append($"{permName}: :x:\n");
                    }
                }
            }

            return [selfChannelPermissionsList, roleChannelPermissionsList, roleGuildPermissionsList];
        }
    }
}
