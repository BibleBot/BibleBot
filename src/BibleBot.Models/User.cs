/*
 * Copyright (C) 2016-2026 Kerygma Digital Co.
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Numerics;
using Microsoft.EntityFrameworkCore;

namespace BibleBot.Models
{
    /// <summary>
    /// The model for user preferences.
    /// </summary>
    public class User : IPreference
    {
        /// <summary>
        /// The Discord Snowflake identifier of the user.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// The default input method of the user.
        /// </summary>
        public string InputMethod { get; set; } = "default";

        /// <summary>
        /// The default language of the user.
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// The default version of the user.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Indicates whether the user prefers titles and other headings to be displayed in verse results.
        /// </summary>
        public bool TitlesEnabled { get; set; } = true;

        /// <summary>
        /// Indicates whether the user prefers chapter and verse numbers to be displayed in verse results.
        /// </summary>
        public bool VerseNumbersEnabled { get; set; } = true;

        /// <summary>
        /// Indicates whether the user prefers pagination when multiple verse results are sent in one response.
        /// </summary>
        public bool PaginationEnabled { get; set; }

        /// <summary>
        /// The default display style for verses from the user.
        /// </summary>
        public string DisplayStyle { get; set; } = "embed";

        /// <summary>
        /// Indicates whether the user is opt out of the service.
        /// </summary>
        public bool IsOptOut { get; set; } = false;
    }
}
