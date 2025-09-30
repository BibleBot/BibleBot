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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BibleBot.Backend.Models;
using BibleBot.Backend.Services;
using BibleBot.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sentry;
using Version = BibleBot.Models.Version;

namespace BibleBot.Backend.Controllers
{
    [Produces("application/json")]
    [Route("api/verses")]
    [ApiController]
    public partial class VersesController(UserService userService, GuildService guildService, ParsingService parsingService, VerseMetricsService verseMetricsService,
                                          VersionService versionService, LanguageService languageService, MetadataFetchingService metadataFetchingService,
                                          List<IContentProvider> bibleProviders, IStringLocalizer<VersesController> localizer, IStringLocalizer<SharedResource> sharedLocalizer) : ControllerBase
    {
        private readonly List<IContentProvider> _bibleProviders = bibleProviders;
        private readonly IStringLocalizer _localizer = localizer;
        private readonly IStringLocalizer _sharedLocalizer = sharedLocalizer;

        [GeneratedRegex(@"(\.*\s*<*\**\d*\**>*\.\.\.)$", RegexOptions.Compiled)]
        private static partial Regex TruncatedTextRegex();

        [GeneratedRegex(@"<\*\*(.*?)\*\*>", RegexOptions.Compiled)]
        private static partial Regex VerseNumberRegex();

        /// <summary>
        /// Processes a message to locate verse references, outputting
        /// the corresponding text.
        /// </summary>
        /// <param name="req">A <see cref="Request" /> object</param>
        /// <response code="200">Returns the corresponding text</response>
        /// <response code="400">If <paramref name="req"/> is invalid</response>
        [Route("process")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IResponse>> ProcessMessage([FromBody] Request req)
        {
            SentrySdk.ConfigureScope(scope =>
            {
                scope.Contexts["request"] = req;
            });

            string displayStyle = "embed";
            List<string> ignoringBrackets = ["<>"];
            bool paginateVerses = false;

            Guild idealGuild = await guildService.Get(req.GuildId);
            if (idealGuild != null)
            {
                displayStyle = idealGuild.DisplayStyle ?? displayStyle;

                if (idealGuild.IgnoringBrackets != null)
                {
                    ignoringBrackets.Add(idealGuild.IgnoringBrackets);
                }
            }

            string body = ParsingService.PurifyBody(ignoringBrackets, req.Body);
            Tuple<string, List<BookSearchResult>> tuple = parsingService.GetBooksInString(metadataFetchingService.GetBookNames(), metadataFetchingService.GetDefaultBookNames(), body);

            bool verseNumbersEnabled = true;
            bool titlesEnabled = true;

            User idealUser = await userService.Get(req.UserId);

            if (idealUser != null && !req.IsBot)
            {
                verseNumbersEnabled = idealUser.VerseNumbersEnabled;
                titlesEnabled = idealUser.TitlesEnabled;
                displayStyle = idealUser.DisplayStyle;
                paginateVerses = idealUser.PaginationEnabled;

                if (idealUser.IsOptOut)
                {
                    return null;
                }
            }

            Language language = await languageService.GetPreferenceOrDefault(idealUser, idealGuild, req.IsBot);
            CultureInfo.CurrentUICulture = new CultureInfo(language.Culture);

            Version idealVersion = await versionService.GetPreferenceOrDefault(idealUser, idealGuild, req.IsBot);

            List<Version> versions = await versionService.Get();
            HashSet<Reference> references = [];

            foreach (BookSearchResult bsr in tuple.Item2)
            {
                Reference reference = null;

                try
                {
                    reference = parsingService.GenerateReference(tuple.Item1, bsr, idealVersion, versions);
                }
                catch (VerseLimitationException ex)
                {
                    if (ex.Message == "too many commas")
                    {
                        return BadRequest(new CommandResponse
                        {
                            OK = false,
                            Pages =
                            [
                                Utils.GetInstance().Embedify(_localizer["CommaLimitTitle"], _localizer["CommaLimitDescription"], true)
                            ],
                            LogStatement = "too many commas",
                            Culture = CultureInfo.CurrentUICulture.Name
                        });
                    }
                }
                catch (Exception err)
                {
                    SentrySdk.CaptureException(err);
                }

                if (reference == null)
                {
                    continue;
                }

                if (reference.IsOT && !reference.Version.SupportsOldTestament)
                {
                    return BadRequest(new VerseResponse
                    {
                        OK = false,
                        LogStatement = string.Format(_localizer["VersionNoSupportOT"], reference.Version.Name),
                        Culture = CultureInfo.CurrentUICulture.Name
                    });
                }

                if (reference.IsNT && !reference.Version.SupportsNewTestament)
                {
                    return BadRequest(new VerseResponse
                    {
                        OK = false,
                        LogStatement = string.Format(_localizer["VersionNoSupportNT"], reference.Version.Name),
                        Culture = CultureInfo.CurrentUICulture.Name
                    });
                }

                if (reference.IsDEU && !reference.Version.SupportsDeuterocanon)
                {
                    return BadRequest(new VerseResponse
                    {
                        OK = false,
                        LogStatement = string.Format(_localizer["VersionNoSupportDEU"], reference.Version.Name),
                        Culture = CultureInfo.CurrentUICulture.Name
                    });
                }

                if (reference.Book is { InternalName: "addesth" or "praz" or "epjer" })
                {
                    reference.Book.ProperName = reference.Book.PreferredName;
                }

                references.Add(reference);
            }

            SentrySdk.ConfigureScope(scope =>
            {
                scope.Contexts["verseReferences"] = references;
            });

            HashSet<VerseResult> results = [];

            if (references.Count == 0)
            {
                return BadRequest(new VerseResponse
                {
                    OK = false,
                    Verses = null,
                    LogStatement = null,
                    Culture = CultureInfo.CurrentUICulture.Name
                });
            }

            if (references.Count > 6)
            {
                return BadRequest(new CommandResponse
                {
                    OK = false,
                    Pages =
                    [
                        Utils.GetInstance().Embedify(_localizer["ReferenceLimitTitle"], _localizer["ReferenceLimitDescription"], true)
                    ],
                    LogStatement = "too many verses",
                    Culture = CultureInfo.CurrentUICulture.Name
                });
            }

            // Create provider lookup dictionary once
            Dictionary<string, IContentProvider> providerLookup = _bibleProviders.ToDictionary(p => p.Name);

            foreach (Reference reference in references)
            {
                if (reference.Version?.Source == null || !providerLookup.TryGetValue(reference.Version.Source, out IContentProvider provider))
                {
                    throw new ProviderNotFoundException();
                }

                VerseResult result = await provider.GetVerse(reference, titlesEnabled, verseNumbersEnabled);

                if (result?.Text == null)
                {
                    continue;
                }

                if (string.Equals(displayStyle, "embed", StringComparison.Ordinal))
                {
                    const int MAX_TEXT_LENGTH = 4096;
                    const int MAX_TITLE_LENGTH = 256;

                    if (result.Text.Length > MAX_TEXT_LENGTH)
                    {
                        result.Text = string.Concat(result.Text.AsSpan(0, Math.Min(MAX_TEXT_LENGTH - 4, result.Text.Length)), "...");
                        result.Text = TruncatedTextRegex().Replace(result.Text, "...");
                    }

                    if (result.Title.Length > MAX_TITLE_LENGTH)
                    {
                        result.Title = string.Concat(result.Title.AsSpan(0, Math.Min(MAX_TITLE_LENGTH - 4, result.Title.Length)), "...");
                    }
                }
                else if (!string.Equals(displayStyle, "embed", StringComparison.Ordinal))
                {
                    const int MAX_TEXT_LENGTH = 2000;

                    int combinedLength = result.Title.Length + result.PsalmTitle.Length + result.Text.Length;
                    int remainingTextLength = MAX_TEXT_LENGTH - result.Title.Length - result.PsalmTitle.Length;

                    if (combinedLength > MAX_TEXT_LENGTH)
                    {
                        result.Text = string.Concat(result.Text.AsSpan(0, Math.Min(remainingTextLength - 70, result.Text.Length)), "..."); // 30 as a buffer for any formatting marks
                        result.Text = TruncatedTextRegex().Replace(result.Text, "...");
                    }
                }

                results.Add(result);
            }

            SentrySdk.ConfigureScope(scope =>
            {
                scope.Contexts["verseResults"] = results;
            });

            if (results.Count == 0)
            {
                return BadRequest(new VerseResponse
                {
                    OK = false,
                    Verses = null,
                    LogStatement = null,
                    Culture = CultureInfo.CurrentUICulture.Name
                });
            }

            // Could use StringBuilder for better performance:
            var logBuilder = new StringBuilder();
            foreach (VerseResult verse in results)
            {
                if (logBuilder.Length > 0)
                {
                    logBuilder.Append(" / ");
                }

                logBuilder.Append(verse.Reference.ToString(true)).Append(' ').Append(verse.Reference.Version.Abbreviation);

                int startingChapterEndingVerse = 0;
                if (verse.Reference.IsExpandoVerse)
                {
                    MatchCollection verseNumbers = VerseNumberRegex().Matches(verse.Text);
                    startingChapterEndingVerse = int.Parse(verseNumbers[verseNumbers.Count - 1].Groups[1].Value);
                }
                else if (verse.Reference.StartingChapter != verse.Reference.EndingChapter)
                {
                    string firstChapterText = verse.Text.Split($"<**{verse.Reference.EndingChapter}:")[0];

                    MatchCollection verseNumbers = VerseNumberRegex().Matches(firstChapterText);
                    startingChapterEndingVerse = int.Parse(verseNumbers[verseNumbers.Count - 1].Groups[1].Value);
                }

                await verseMetricsService.Create(req.UserId, req.GuildId, verse.Reference, startingChapterEndingVerse);
            }

            string logStatement = logBuilder.ToString();
            if (logStatement.Contains("Psalm 151"))
            {
                logStatement = logStatement.Replace("Psalm 151 1", "Psalm 151");
            }

            return Ok(new VerseResponse
            {
                OK = true,
                Verses = [.. results],
                DisplayStyle = displayStyle,
                Paginate = paginateVerses,
                LogStatement = logStatement,
                Culture = CultureInfo.CurrentUICulture.Name,
                CultureFooter = string.Format(_sharedLocalizer["GlobalFooter"], Utils.Version)
            });

        }
    }
}
