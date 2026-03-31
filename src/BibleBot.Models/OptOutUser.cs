/*
 * Copyright (C) 2016-2026 Kerygma Digital Co.
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at https://mozilla.org/MPL/2.0/.
 */

namespace BibleBot.Models
{
    /// <summary>
    /// Represents an entry in the opt_out_users table.
    /// If a row exists for a given user ID, that user is opted out.
    /// </summary>
    public class OptOutUser
    {
        /// <summary>
        /// The Discord Snowflake identifier of the opted-out user.
        /// This is a foreign key to the users table.
        /// </summary>
        public long Id { get; set; }
    }
}
