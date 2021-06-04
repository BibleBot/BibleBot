using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
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
        private readonly FrontendStatsService _frontendStatsService;

        private readonly BibleGatewayProvider _bgProvider;

        private readonly List<ICommandGroup> _commandGroups;

        public CommandsController(UserService userService, GuildService guildService, VersionService versionService,
                                  FrontendStatsService frontendStatsService, BibleGatewayProvider bibleGatewayProvider)
        {
            _userService = userService;
            _guildService = guildService;
            _versionService = versionService;
            _frontendStatsService = frontendStatsService;

            _bgProvider = bibleGatewayProvider;

            _commandGroups = new List<ICommandGroup>
            {
                new CommandGroups.Information.InformationCommandGroup(_userService, _guildService, _versionService, _frontendStatsService),
                new CommandGroups.Resources.DailyVerseCommandGroup(_userService, _guildService, _versionService, _bgProvider),
                new CommandGroups.Resources.RandomVerseCommandGroup(_userService, _guildService, _versionService, _bgProvider),
                new CommandGroups.Settings.VersionCommandGroup(_userService, _guildService, _versionService),
                new CommandGroups.Settings.FormattingCommandGroup(_userService, _guildService)
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
        public IResponse ProcessMessage([FromBody] Request req)
        {
            if (req.Token != Environment.GetEnvironmentVariable("ENDPOINT_TOKEN"))
            {
                return new CommandResponse
                {
                    OK = false
                };
            }

            var tokenizedBody = req.Body.Split(" ");
            
            if (tokenizedBody.Length > 0)
            {
                var potentialCommand = tokenizedBody[0];

                var idealGuild = _guildService.Get(req.GuildId);
                var prefix = "+";

                if (idealGuild != null)
                {
                    prefix = idealGuild.Prefix;
                }

                if (potentialCommand.StartsWith(prefix))
                {
                    var grp = _commandGroups.Where(grp => grp.Name == potentialCommand.Substring(1)).FirstOrDefault();

                    if (grp != null)
                    {
                        if (grp.IsOwnerOnly && req.UserId != "186046294286925824")
                        {
                            return new CommandResponse
                            {
                                OK = false,
                                Pages = new List<InternalEmbed>
                                {
                                    new Utils().Embedify("Permissions Error", "You do not have the required permissions to use this command.", true)
                                }
                            };
                        }

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
                                                Pages = new List<InternalEmbed>
                                                {
                                                    new Utils().Embedify("Insufficient Permissions", "You do not have the required permissions to use this command.", true)
                                                },
                                                LogStatement = $"Insufficient permissions on +{grp.Name} {idealCommand.Name}."
                                            };
                                        }
                                    }
                                }

                                var commandArgs = tokenizedBody.Skip(2).ToList();

                                if (commandArgs.Count() < idealCommand.ExpectedArguments)
                                {
                                    return new CommandResponse
                                    {
                                        OK = false,
                                        Pages = new List<InternalEmbed>
                                        {
                                            new Utils().Embedify("Insufficient Parameters", idealCommand.ArgumentsError, true)
                                        },
                                        LogStatement = $"Insufficient parameters on +{grp.Name} {idealCommand.Name}."
                                    };
                                }
                                
                                return idealCommand.ProcessCommand(req, tokenizedBody.Skip(2).ToList());
                            }
                        }

                        return grp.DefaultCommand.ProcessCommand(req, null);
                    }
                    else
                    {
                        var cmd = _commandGroups.Where(grp => grp.Name == "info").FirstOrDefault().Commands.Where(cmd => cmd.Name == potentialCommand.Substring(1)).FirstOrDefault();
                        
                        if (cmd != null)
                        {
                            if (cmd.Name == "biblebot")
                            {
                                var args = tokenizedBody.Skip(1).ToList();

                                if (args.Count < 1) {
                                    return cmd.ProcessCommand(req, null);
                                }

                                var potentialRescue = _commandGroups.Where(grp => grp.Name == args[0]).FirstOrDefault();

                                if (potentialRescue != null)
                                {
                                    if (args.Count > 1) {
                                        var idealCommand = potentialRescue.Commands.Where(cmd => cmd.Name == args[1]).FirstOrDefault();

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
                                                            Pages = new List<InternalEmbed>
                                                            {
                                                                new Utils().Embedify("Insufficient Permissions", "You do not have the required permissions to use this command.", true)
                                                            },
                                                            LogStatement = $"Insufficient permissions on +{grp.Name} {idealCommand.Name}."
                                                        };
                                                    }
                                                }
                                            }

                                            var commandArgs = args.Skip(1).ToList();

                                            if (commandArgs.Count() < idealCommand.ExpectedArguments)
                                            {
                                                return new CommandResponse
                                                {
                                                    OK = false,
                                                    Pages = new List<InternalEmbed>
                                                    {
                                                        new Utils().Embedify("Insufficient Parameters", idealCommand.ArgumentsError, true)
                                                    },
                                                    LogStatement = $"Insufficient parameters on +{grp.Name} {idealCommand.Name}."
                                                };
                                            }
                                            
                                            return idealCommand.ProcessCommand(req, args.Skip(1).ToList());
                                        }
                                    }
                                    else
                                    {
                                        return potentialRescue.DefaultCommand.ProcessCommand(req, null);
                                    }
                                }

                                return cmd.ProcessCommand(req, null);
                            }
                            else
                            {
                                return cmd.ProcessCommand(req, null);
                            }
                        }
                    }
                }
            }

            return new CommandResponse
            {
                OK = false,
                Pages = null,
                LogStatement = null
            };
        }
    }
}