using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
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
        private readonly ParsingService _parsingService;
        private readonly VersionService _versionService;
        private readonly NameFetchingService _nameFetchingService;

        private readonly BibleGatewayProvider _bgpProvider;

        public VersesController(UserService userService, ParsingService parsingService, VersionService versionService, NameFetchingService nameFetchingService,
                                BibleGatewayProvider bibleGatewayProvider)
        {
            _userService = userService;
            _parsingService = parsingService;
            _versionService = versionService;
            _nameFetchingService = nameFetchingService;

            _bgpProvider = bibleGatewayProvider;
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
            var tuple = _parsingService.GetBooksInString(_nameFetchingService.GetBookNames(), _nameFetchingService.GetDefaultBookNames(), req.Body.ToLower());

            List<Verse> results = new List<Verse>();

            foreach (var bsr in tuple.Item2)
            {
                var idealUser = _userService.Get(req.UserId);

                if (idealUser != null)
                {
                    var idealVersion = _versionService.Get(idealUser.Version);

                    if (idealVersion == null)
                    {
                        idealVersion = _versionService.Get("RSV");
                    }

                    var reference = _parsingService.GenerateReference(tuple.Item1, bsr, idealVersion);

                    if (reference != null)
                    {
                        switch (reference.Version.Source) 
                        {
                            case "bg":
                                Verse result = await _bgpProvider.GetVerse(reference, true, true);
                                results.Add(result);
                                break;
                        }
                    }
                }
                
            }

            return new VerseResponse
            {
                OK = true,
                Verses = results
            };
        }
    }
}