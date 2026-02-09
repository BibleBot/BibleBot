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
    /// An interface describing the implementation of a Response.
    /// </summary>
    public interface IResponse
    {
        /// <summary>
        /// Indicates whether the operation was performed successfully.
        /// </summary>
        bool OK { get; set; }

        /// <summary>
        /// The message that frontend should log.
        /// </summary>
        string LogStatement { get; set; }

        /// <summary>
        /// The IETF BCP 47 tag of the localization used.
        /// </summary>
        string Culture { get; set; }

        /// <summary>
        /// The ordinary footer, but localized.
        /// </summary>
        string CultureFooter { get; set; }

        /// <summary>
        /// The type of response this is, to indicate to frontend how to handle it.
        /// </summary>
        string Type { get; }
    }
}
