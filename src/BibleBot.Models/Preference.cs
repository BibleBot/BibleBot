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
    /// An interface describing the implementation of Preference models.
    /// </summary>
    public interface IPreference
    {
        /// <summary>
        /// The Discord Snowflake identifier of the preference.
        /// </summary>
        long Id { get; set; }

        /// <summary>
        /// The default version of the preference.
        /// </summary>
        string Version { get; set; }

        /// <summary>
        /// The default language of the preference.
        /// </summary>
        string Language { get; set; }
    }
}
