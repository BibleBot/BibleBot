/*
* Copyright (C) 2016-2022 Kerygma Digital Co.
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
using BibleBot.Backend.Models;
using BibleBot.Backend.Services;
using BibleBot.Backend.Services.Providers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BibleBot.Backend.Controllers
{
    [Produces("application/json")]
    [Route("api/verses")]
    [ApiController]
    public class VersesController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly GuildService _guildService;
        private readonly ParsingService _parsingService;
        private readonly VersionService _versionService;
        private readonly NameFetchingService _nameFetchingService;

        private readonly List<IBibleProvider> _bibleProviders;

        public VersesController(UserService userService, GuildService guildService, ParsingService parsingService, VersionService versionService, NameFetchingService nameFetchingService,
                                BibleGatewayProvider bgProvider, APIBibleProvider abProvider)
        {
            _userService = userService;
            _guildService = guildService;
            _parsingService = parsingService;
            _versionService = versionService;
            _nameFetchingService = nameFetchingService;

            _bibleProviders = new List<IBibleProvider>
            {
                bgProvider,
                abProvider
            };
        }

        /// <summary>
        /// Processes a message to locate verse references, outputting
        /// the corresponding text.
        /// </summary>
        /// <param name="req">A BibleBot.Lib.Request object</param>
        /// <response code="200">Returns the corresponding text</response>
        /// <response code="400">If req is invalid</response>
        [Route("process")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IResponse> ProcessMessage([FromBody] Request req)
        {
            if (req.Token != Environment.GetEnvironmentVariable("ENDPOINT_TOKEN"))
            {
                return new VerseResponse
                {
                    OK = false,
                    Verses = null,
                    LogStatement = null
                };
            }

            var displayStyle = "embed";
            var ignoringBrackets = "<>";
            var paginateVerses = false;

            var idealGuild = _guildService.Get(req.GuildId);
            if (idealGuild != null)
            {
                displayStyle = idealGuild.DisplayStyle == null ? displayStyle : idealGuild.DisplayStyle;
                ignoringBrackets = idealGuild.IgnoringBrackets == null ? ignoringBrackets : idealGuild.IgnoringBrackets;
            }

            var body = _parsingService.PurifyBody(ignoringBrackets, req.Body);
            var tuple = _parsingService.GetBooksInString(_nameFetchingService.GetBookNames(), _nameFetchingService.GetDefaultBookNames(), body);

            List<Verse> results = new List<Verse>();

            foreach (var bsr in tuple.Item2)
            {
                var version = "RSV";
                var verseNumbersEnabled = true;
                var titlesEnabled = true;

                var idealUser = _userService.Get(req.UserId);

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

                var idealVersion = _versionService.Get(version);
                var reference = _parsingService.GenerateReference(tuple.Item1, bsr, idealVersion);

                if (reference != null)
                {
                    if (reference.IsOT && !reference.Version.SupportsOldTestament)
                    {
                        return new VerseResponse
                        {
                            OK = false,
                            LogStatement = $"{reference.Version.Name} does not support the Old Testament."
                        };
                    }
                    else if (reference.IsNT && !reference.Version.SupportsNewTestament)
                    {
                        return new VerseResponse
                        {
                            OK = false,
                            LogStatement = $"{reference.Version.Name} does not support the New Testament."
                        };
                    }
                    else if (reference.IsDEU && !reference.Version.SupportsDeuterocanon)
                    {
                        return new VerseResponse
                        {
                            OK = false,
                            LogStatement = $"{reference.Version.Name} does not support the Apocrypha/Deuterocanon."
                        };
                    }

                    Verse result = new Verse();
                    IBibleProvider provider = _bibleProviders.Where(pv => pv.Name == reference.Version.Source).FirstOrDefault();

                    if (provider == null)
                    {
                        throw new ProviderNotFoundException();
                    }

                    result = await provider.GetVerse(reference, titlesEnabled, verseNumbersEnabled);

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
                        result.Text = $"{String.Join("", result.Text.SkipLast(result.Text.Length - 2044))}...";
                        result.Text = Regex.Replace(result.Text, @"(\.*\s*<*\**\d*\**>*\.\.\.)$", "...");
                    }
                    else if (displayStyle != "embed")
                    {
                        var combinedTextLength = result.Title.Length + result.PsalmTitle.Length + result.Text.Length;

                        if (combinedTextLength > 2000)
                        {
                            result.Text = $"{String.Join("", result.Text.SkipLast(combinedTextLength - 1919))}...";
                            result.Text = Regex.Replace(result.Text, @"(\.*\s*<*\**\d*\**>*\.\.\.)$", "...");
                        }
                    }

                    results.Add(result);
                }
            }

            results = results.Distinct().ToList();

            if (results.Count() > 6)
            {
                return new CommandResponse
                {
                    OK = false,
                    Pages = new List<InternalEmbed>
                    {
                        new Utils().Embedify("Too Many References", "There are too many references, the maximum amount of references you can do in one message is 6.", true)
                    },
                    LogStatement = "too many verses"
                };
            }
            else if (results.Count() > 0)
            {
                return new VerseResponse
                {
                    OK = true,
                    Verses = results,
                    DisplayStyle = displayStyle,
                    Paginate = paginateVerses,
                    LogStatement = String.Join(" / ", results.Select(verse => verse.Reference.ToString()))
                };
            }
            else
            {
                return new VerseResponse
                {
                    OK = false,
                    Verses = null,
                    LogStatement = null
                };
            }
        }
    }
}
