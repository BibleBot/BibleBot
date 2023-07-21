/*
* Copyright (C) 2016-2023 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BibleBot.Models;

namespace BibleBot.Backend
{
    public class Utils
    {

        private Utils() { }
        private static Utils _instance;
        private static readonly object _lock = new object();

        public static Utils GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new Utils();
                    }
                }
            }

            return _instance;
        }


        public enum Colors
        {
            NORMAL_COLOR = 6709986,
            ERROR_COLOR = 16723502
        }

        public static string Version = "9.2-beta";

        public InternalEmbed Embedify(string title, string description, bool isError)
        {
            return Embedify(null, title, description, isError, null);
        }

        public InternalEmbed Embedify(string author, string title, string description, bool isError, string copyright)
        {
            string footerText = $"BibleBot v{Utils.Version} by Kerygma Digital";

            var embed = new InternalEmbed();
            embed.Title = title;
            embed.Color = isError ? (uint)Colors.ERROR_COLOR : (uint)Colors.NORMAL_COLOR;

            embed.Footer = new Footer();
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

                    if ((new string[] { "apostles", "nicene" }).Contains(creedResource.CommandReference))
                    {
                        copyright = "© 1998 English Language Liturgical Consultation (ELLC)";
                    }


                    return new List<InternalEmbed>
                    {
                        Embedify(null, creedResource.Title, creedResource.Text, false, copyright)
                    };
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
                    if (section.Contains("-"))
                    {
                        var sectionRange = section.Split("-").Where(item => item.Length > 0).ToArray();
                        int firstPart = 0;
                        int secondPart = 0;

                        var results = new List<InternalEmbed>();

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
                                    var title = $"{pgResource.Title} - Paragraph {i}";
                                    results.Add(Embedify(null, title, $"<**{i}**> {pgResource.Paragraphs.ElementAt(i - 1).Text}", false, pgResource.Copyright));
                                }
                            }
                            else if (firstPart == secondPart)
                            {
                                var title = $"{pgResource.Title} - Paragraph {firstPart}";
                                results.Add(Embedify(null, title, pgResource.Paragraphs.ElementAt(firstPart - 1).Text, false, pgResource.Copyright));
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
                        return new List<InternalEmbed>
                        {
                            CreateTitlePage(pgResource.Author, pgResource.Title, pgResource.Category, pgResource.Copyright, pgResource.ImageRef, null)
                        };
                    }
                }
                else
                {
                    var title = $"{pgResource.Title} - Paragraph {sectionAsIndex}";

                    return new List<InternalEmbed>
                    {
                        Embedify(null, title, pgResource.Paragraphs.ElementAt(sectionAsIndex - 1).Text, false, pgResource.Copyright)
                    };
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
                    var matchingSection = sResource.Sections.Where(sct => sct.Slugs.Contains(section)).FirstOrDefault();

                    if (matchingSection != null)
                    {
                        var title = $"{matchingSection.Title}";
                        var results = new List<InternalEmbed>();

                        for (int i = 0; i < matchingSection.Pages.Count; i++)
                        {
                            results.Add(Embedify(sResource.Title, $"{title} (Page {i + 1} of {matchingSection.Pages.Count})", matchingSection.Pages.ElementAt(i), false, sResource.Copyright));
                        }

                        return results;
                    }
                    else
                    {
                        return new List<InternalEmbed>
                        {
                            CreateTitlePage(sResource.Author, sResource.Title, sResource.Category, sResource.Copyright, sResource.ImageRef, sResource.Sections)
                        };
                    }
                }
                else
                {
                    var matchingSection = sResource.Sections.ElementAtOrDefault(sectionAsIndex - 1);

                    if (matchingSection != null)
                    {
                        var title = $"{matchingSection.Title}";
                        var results = new List<InternalEmbed>();

                        for (int i = 0; i < matchingSection.Pages.Count; i++)
                        {
                            results.Add(Embedify(sResource.Title, $"{title} (Page {i + 1} of {matchingSection.Pages.Count})", matchingSection.Pages.ElementAt(i), false, sResource.Copyright));
                        }

                        return results;
                    }
                    else
                    {
                        return new List<InternalEmbed>
                        {
                            CreateTitlePage(sResource.Author, sResource.Title, sResource.Category, sResource.Copyright, sResource.ImageRef, sResource.Sections)
                        };
                    }
                }
            }

            return null;
        }

        private InternalEmbed CreateTitlePage(string author, string title, string category, string copyright, string imageRef, List<Section> sections)
        {
            var categoryText = "";

            foreach (var cat in category.Split("."))
            {
                categoryText += $"{CultureInfo.InvariantCulture.TextInfo.ToTitleCase(cat)} > ";
            }

            var embed = Embedify(null, title, null, false, copyright);

            embed.Fields = new List<EmbedField>
            {
                new EmbedField
                {
                    Name = "Author",
                    Value = author,
                    Inline = false
                },
                new EmbedField
                {
                    Name = "Category",
                    Value = categoryText.Substring(0, categoryText.Length - 3),
                    Inline = false
                }
            };


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
                    string sectionList = "";

                    for (int i = 0; i < sections.Count; i++)
                    {
                        Section section = sections[i];

                        sectionList += $"{i + 1}. {section.Title} ({(section.Pages.Count > 1 ? $"{section.Pages.Count} pages" : $"{section.Pages.Count} page")})" +
                                       $" [`{section.Slugs[0]}`]\n";
                    }

                    embed.Fields.Add(new EmbedField
                    {
                        Name = "Sections",
                        Value = sectionList,
                        Inline = false
                    });
                }
            }

            return embed;
        }
    }
}
