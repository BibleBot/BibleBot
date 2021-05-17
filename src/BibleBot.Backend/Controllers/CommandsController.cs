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
    [Route("api/commands")]
    [ApiController]
    public class CommandsController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly GuildService _guildService;

        private readonly BibleGatewayProvider _bgpProvider;

        private readonly List<ICommandGroup> _commandGroups;

        public CommandsController(UserService userService, GuildService guildService, BibleGatewayProvider bibleGatewayProvider)
        {
            _userService = userService;
            _guildService = guildService;
            _bgpProvider = bibleGatewayProvider;

            _commandGroups = new List<ICommandGroup>
            {
                new CommandGroups.Settings.VersionsGroup(_userService, _guildService)
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
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<CommandResponse> ProcessMessage([FromBody] Request req)
        {
            return new CommandResponse();
        }
    }
}