using System.Linq;
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
        private readonly VersionService _versionService;

        private readonly BibleGatewayProvider _bgpProvider;

        private readonly List<ICommandGroup> _commandGroups;

        public CommandsController(UserService userService, GuildService guildService, VersionService versionService,
                                  BibleGatewayProvider bibleGatewayProvider)
        {
            _userService = userService;
            _guildService = guildService;
            _versionService = versionService;
            _bgpProvider = bibleGatewayProvider;

            _commandGroups = new List<ICommandGroup>
            {
                new CommandGroups.Settings.VersionCommandGroup(_userService, _guildService, _versionService)
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
        public CommandResponse ProcessMessage([FromBody] Request req)
        {
            var tokenizedBody = req.Body.Split(" ");
            
            if (tokenizedBody.Length > 0)
            {
                var potentialCommand = tokenizedBody[0];

                if (potentialCommand.StartsWith("+"))
                {
                    var grp = _commandGroups.Where(grp => grp.Name == potentialCommand.Substring(1)).FirstOrDefault();

                    if (grp != null)
                    {
                        // TODO: Implement OwnerOnly check.

                        if (tokenizedBody.Length > 1)
                        {
                            var idealCommand = grp.Commands.Where(cmd => cmd.Name == tokenizedBody[1]).FirstOrDefault();

                            if (idealCommand != null)
                            {
                                if (idealCommand.PermissionsRequired != null)
                                {
                                    foreach (var permission in idealCommand.PermissionsRequired)
                                    {
                                        if ((req.UserPermissions & (long) permission) != (long) permission)
                                        {
                                            return new CommandResponse
                                            {
                                                OK = false,
                                                Pages = new List<DiscordEmbed>
                                                {
                                                    new Utils().Embedify("Permissions Error", "You do not have the required permissions to use this command.", true)
                                                }
                                            };
                                        }
                                    }
                                }
                                
                                return idealCommand.ProcessCommand(req, tokenizedBody.Skip(2).ToList());
                            }
                        }

                        // TODO: At the moment, non-grouped commands CANNOT take an argument, which could be problematic later.
                        return grp.DefaultCommand.ProcessCommand(req, null);
                    }
                }
            }

            return new CommandResponse
            {
                OK = false,
                Pages = null
            };
        }
    }
}