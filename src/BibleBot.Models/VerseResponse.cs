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
    /// The model for standard verse responses.
    /// </summary>
    public class VerseResponse : IResponse
    {
        /// <inheritdoc/>
        public bool OK { get; set; }
        /// <inheritdoc/>
        public string LogStatement { get; set; }
        /// <inheritdoc/>
        public string Type => "verse";

        /// <summary>
        /// The content of the response.
        /// </summary>
        public List<Verse> Verses { get; set; }

        /// <summary>
        /// The display style that the content should be displayed in.
        /// </summary>
        public string DisplayStyle { get; set; }

        /// <summary>
        /// Indicates whether to paginate if there are multiple verse results.
        /// </summary>
        /// <remarks>
        /// This is only relevant if <see cref="DisplayStyle"/> is <c>embed</c>.
        /// </remarks>
        public bool Paginate { get; set; }
    }
}
