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
    /// Not to be confused with <see cref="BookSearchResult"/>, SearchResult represents an individual
    /// search result from a BibleProvider.
    /// </summary>
    public class SearchResult
    {
        /// <summary>
        /// The verse reference string of the result.
        /// </summary>
        public string Reference { get; set; }

        /// <summary>
        /// The text or an excerpt thereof of the result.
        /// </summary>
        public string Text { get; set; }
    }
}
