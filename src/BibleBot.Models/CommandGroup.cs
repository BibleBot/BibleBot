/*
* Copyright (C) 2016-2024 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;

namespace BibleBot.Models
{
    /// <summary>
    /// An interface that describes the implementation of a CommandGroup.
    /// </summary>
    public interface ICommandGroup
    {
        /// <summary>
        /// The name of the command group.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Indicates whether the commands within this group are only for BibleBot staff.
        /// </summary>
        bool IsStaffOnly { get; set; }

        /// <summary>
        /// A list of the <see cref="ICommand"/>s within this group.
        /// </summary>
        List<ICommand> Commands { get; set; }

        /// <summary>
        /// The <see cref="ICommand"/> that should be used as a fallback if this group is invoked by itself.
        /// </summary>
        ICommand DefaultCommand { get; set; }
    }
}
