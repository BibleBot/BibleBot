/*
* Copyright (C) 2016-2024 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BibleBot.Backend.Services;
using BibleBot.Backend.Services.Providers;
using BibleBot.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BibleBot.Backend.Controllers
{
    [Produces("application/json")]
    [Route("api/verses")]
    [ApiController]
    public class VersesController(UserService userService, GuildService guildService, ParsingService parsingService, VersionService versionService, NameFetchingService nameFetchingService,
                            BibleGatewayProvider bgProvider, APIBibleProvider abProvider) : ControllerBase
    {
        private readonly UserService _userService = userService;
        private readonly GuildService _guildService = guildService;
        private readonly ParsingService _parsingService = parsingService;
        private readonly VersionService _versionService = versionService;
        private readonly NameFetchingService _nameFetchingService = nameFetchingService;

        private readonly List<IBibleProvider> _bibleProviders =
            [
                bgProvider,
                abProvider
            ];

        /// <summary>
        /// Processes a message to locate verse references, outputting
        /// the corresponding text.
        /// </summary>
        /// <param name="req">A <see cref="Request" /> object</param>
        /// <response code="200">Returns the corresponding text</response>
        /// <response code="400">If <paramref name="req"/> is invalid</response>
        /// <response code="403">If <paramref name="req"/>.Token is invalid</response>
        [Route("process")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IResponse>> ProcessMessage([FromBody] Request req)
        {
            if (req.Token != Environment.GetEnvironmentVariable("ENDPOINT_TOKEN"))
            {
                return new ObjectResult(new VerseResponse
                {
                    OK = false,
                    Verses = null,
                    LogStatement = null
                })
                {
                    StatusCode = 403
                };
            }

            string displayStyle = "embed";
            List<string> ignoringBrackets = ["<>"];
            bool paginateVerses = false;

            Guild idealGuild = await _guildService.Get(req.GuildId);
            if (idealGuild != null)
            {
                displayStyle = idealGuild.DisplayStyle ?? displayStyle;

                if (idealGuild.IgnoringBrackets != null)
                {
                    ignoringBrackets.Add(idealGuild.IgnoringBrackets);
                }
            }

            string body = _parsingService.PurifyBody(ignoringBrackets, req.Body);
            Tuple<string, List<BookSearchResult>> tuple = _parsingService.GetBooksInString(_nameFetchingService.GetBookNames(), _nameFetchingService.GetDefaultBookNames(), body);

            string version = "RSV";
            bool verseNumbersEnabled = true;
            bool titlesEnabled = true;

            User idealUser = await _userService.Get(req.UserId);

            if (idealUser != null && !req.IsBot)
            {
                version = idealUser.Version;
                verseNumbersEnabled = idealUser.VerseNumbersEnabled;
                titlesEnabled = idealUser.TitlesEnabled;
                displayStyle = idealUser.DisplayStyle;
                paginateVerses = idealUser.PaginationEnabled;
            }
            else if (idealGuild != null)
            {
                // As much as I hate the if-duplication, we have to check independently of the previous
                // otherwise the guild default won't be a default.

                version = idealGuild.Version;
            }


            Models.Version idealVersion = await _versionService.Get(version) ?? await _versionService.Get("RSV");

            List<Reference> references = [];

            foreach (BookSearchResult bsr in tuple.Item2)
            {
                Reference reference = await _parsingService.GenerateReference(tuple.Item1, bsr, idealVersion);

                if (reference == null)
                {
                    continue;
                }

                if (reference.IsOT && !reference.Version.SupportsOldTestament)
                {
                    return BadRequest(new VerseResponse
                    {
                        OK = false,
                        LogStatement = $"{reference.Version.Name} does not support the Old Testament."
                    });
                }
                else if (reference.IsNT && !reference.Version.SupportsNewTestament)
                {
                    return BadRequest(new VerseResponse
                    {
                        OK = false,
                        LogStatement = $"{reference.Version.Name} does not support the New Testament."
                    });
                }
                else if (reference.IsDEU && !reference.Version.SupportsDeuterocanon)
                {
                    return BadRequest(new VerseResponse
                    {
                        OK = false,
                        LogStatement = $"{reference.Version.Name} does not support the Apocrypha/Deuterocanon."
                    });
                }

                if (!references.Contains(reference))
                {
                    references.Add(reference);
                }
            }

            List<Verse> results = [];

            foreach (Reference reference in references)
            {
                IBibleProvider provider = _bibleProviders.FirstOrDefault(pv =>
                {
                    if (reference != null)
                    {
                        if (reference.Version != null)
                        {
                            return pv.Name == reference.Version.Source;
                        }
                    }

                    return false;
                }) ?? throw new ProviderNotFoundException();

                Verse result = await provider.GetVerse(reference, titlesEnabled, verseNumbersEnabled);

                if (result == null)
                {
                    continue;
                }

                if (result.Text == null)
                {
                    continue;
                }

                if (displayStyle == "embed" && result.Text.Length > 2048)
                {
                    result.Text = $"{string.Join("", result.Text.SkipLast(result.Text.Length - 2044))}...";
                    result.Text = Regex.Replace(result.Text, @"(\.*\s*<*\**\d*\**>*\.\.\.)$", "...");
                }
                else if (displayStyle != "embed")
                {
                    int combinedTextLength = result.Title.Length + result.PsalmTitle.Length + result.Text.Length;

                    if (combinedTextLength > 2000)
                    {
                        result.Text = $"{string.Join("", result.Text.SkipLast(combinedTextLength - 1919))}...";
                        result.Text = Regex.Replace(result.Text, @"(\.*\s*<*\**\d*\**>*\.\.\.)$", "...");
                    }
                }

                if (!results.Contains(result))
                {
                    results.Add(result);
                }
            }

            if (results.Count > 6)
            {
                return BadRequest(new CommandResponse
                {
                    OK = false,
                    Pages =
                    [
                        Utils.GetInstance().Embedify("Too Many References", "There are too many references, the maximum amount of references you can do in one message is 6.", true)
                    ],
                    LogStatement = "too many verses"
                });
            }
            else if (results.Count > 0)
            {
                string logStatement = string.Join(" / ", results.Select(verse => $"{verse.Reference} {verse.Reference.Version.Abbreviation}"));

                if (logStatement.Contains("Psalm 151"))
                {
                    logStatement = logStatement.Replace("Psalm 151 1", "Psalm 151");
                }

                return Ok(new VerseResponse
                {
                    OK = true,
                    Verses = results,
                    DisplayStyle = displayStyle,
                    Paginate = paginateVerses,
                    LogStatement = logStatement
                });
            }
            else
            {
                return BadRequest(new VerseResponse
                {
                    OK = false,
                    Verses = null,
                    LogStatement = null
                });
            }
        }
    }
}
