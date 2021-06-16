using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Generic;

using BibleBot.Lib;

using BibleBot.Backend.Models;
using BibleBot.Backend.Services;
using BibleBot.Backend.Services.Providers;

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
        public async Task<VerseResponse> ProcessMessage([FromBody] Request req)
        {
            if (req.Token != Environment.GetEnvironmentVariable("ENDPOINT_TOKEN"))
            {
                return new VerseResponse
                {
                    OK = false,
                    LogStatement = null
                };
            }

            var body = req.Body.ToLower().Replace("\r", " ").Replace("\n", " ");
            var tuple = _parsingService.GetBooksInString(_nameFetchingService.GetBookNames(), _nameFetchingService.GetDefaultBookNames(), body);

            List<Verse> results = new List<Verse>();
            var displayStyle = "embed";

            foreach (var bsr in tuple.Item2)
            {
                var idealUser = _userService.Get(req.UserId);
                var idealGuild = _guildService.Get(req.GuildId);

                var version = "RSV";
                var verseNumbersEnabled = true;
                var titlesEnabled = true;
                var ignoringBrackets = "<>";

                if (idealUser != null)
                {
                    version = idealUser.Version;
                    verseNumbersEnabled = idealUser.VerseNumbersEnabled;
                    titlesEnabled = idealUser.TitlesEnabled;
                    displayStyle = idealUser.DisplayStyle;
                }
                else if (idealGuild != null)
                {
                    // As much as I hate the duplication, we have to check independently of the previous
                    // otherwise the guild default won't be a default.

                    version = idealGuild.Version;
                }
                
                if (idealGuild != null)
                {
                    ignoringBrackets = idealGuild.IgnoringBrackets;
                }

                var idealVersion = _versionService.Get(version);

                if (!_parsingService.IsSurroundedByBrackets(ignoringBrackets, bsr, tuple.Item1))
                {
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
                        IBibleProvider provider = _bibleProviders.Where(pv => pv.Name == idealVersion.Source).FirstOrDefault();

                        if (provider == null)
                        {
                            throw new ProviderNotFoundException();
                        }

                        result = await provider.GetVerse(reference, titlesEnabled, verseNumbersEnabled);

                        if (result.Text == null)
                        {
                            break;
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
            }

            if (results.Count() > 0)
            {
                return new VerseResponse
                {
                    OK = true,
                    Verses = results,
                    DisplayStyle = displayStyle,
                    LogStatement = String.Join(" / ", results.Select(verse => verse.Reference.ToString()))
                };
            }
            else
            {
               return new VerseResponse
                {
                    OK = false,
                    LogStatement = null
                }; 
            }
        }
    }
}