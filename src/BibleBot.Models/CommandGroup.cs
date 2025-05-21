/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;

namespace BibleBot.Models
{
    /// <summary>
    /// The base implementation of a CommandGroup.
    /// </summary>
    public abstract class CommandGroup
    {
        /// <summary>
        /// The name of the command group.
        /// </summary>
        public abstract string Name { get; set; }

        /// <summary>
        /// Indicates whether the commands within this group are only for BibleBot staff.
        /// </summary>
        public virtual bool IsStaffOnly { get; set; } = false;

        /// <summary>
        /// A list of the <see cref="Command"/>s within this group.
        /// </summary>
        public abstract List<Command> Commands { get; set; }

        /// <summary>
        /// The <see cref="Command"/> that should be used as a fallback if this group is invoked by itself.
        /// </summary>
        public abstract Command DefaultCommand { get; set; }
    }
}
