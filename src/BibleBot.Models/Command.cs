/*
* Copyright (C) 2016-2024 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BibleBot.Models
{
    /// <summary>
    /// An interface that describes the implementation of a Command.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// The name of the command.
        /// </summary>
        string Name { get; set; }

        // TODO(srp): Should rename this to UsageText and expand its usage in error cases.
        /// <summary>
        /// The usage text, typically returned when arguments are invalid or the amount of arguments does not agree with <see cref="ExpectedArguments"/>.
        /// </summary>
        string ArgumentsError { get; set; }

        [Obsolete("Slash commands has made this obsolete. We expect frontend to handle argument counts and similar validation.")]
        int ExpectedArguments { get; set; }

        [Obsolete("Slash commands has made this obsolete. We expect frontend to handle permission checks.")]
        List<Permissions> PermissionsRequired { get; set; }

        [Obsolete("Slash commands has made this obsolete. Bots cannot run slash commands.")]
        bool BotAllowed { get; set; }

        /// <summary>
        /// The internal logic of the command, whatever that may be.
        /// </summary>
        /// <param name="req">The request that invoked the command.</param>
        /// <param name="args">The arguments of the command.</param>
        /// <returns>A <see cref="CommandResponse"/> in most cases, sometimes a <see cref="VerseResponse"/>.</returns>
        Task<IResponse> ProcessCommand(Request req, List<string> args);
    }
}
