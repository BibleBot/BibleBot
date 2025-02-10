/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

namespace BibleBot.Models
{
    /// <summary>
    /// Represents an OptOutUser entry in the database.
    /// </summary>
    public class OptOutUser
    {
        /// <summary>
        /// The Discord Snowflake identifier of the user.
        /// </summary>
        public string UserId { get; set; }
    }
}