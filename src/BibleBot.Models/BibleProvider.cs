/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
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
    /// An interface that describes the implementation of a BibleProvider.
    /// </summary>
    public interface IBibleProvider
    {
        /// <summary>
        /// A simple reference of the source. The current convention is
        /// to use 2-char long names like "bg" (Bible Gateway) or "ab" (API.Bible)
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Fetches, formats, and returns the content belonging to a <see cref="Reference"/>.
        /// </summary>
        /// <param name="reference">The reference whose content we are fetching.</param>
        /// <param name="titlesEnabled">Whether the content should include titles and other headings.</param>
        /// <param name="verseNumbersEnabled">Whether the content should include chapter and verse numbers.</param>
        /// <returns>If the <see cref="Reference"/> is valid, a <see cref="VerseResult"/> object populated with content according to the parameters given. If not, null.</returns>
        Task<VerseResult> GetVerse(Reference reference, bool titlesEnabled, bool verseNumbersEnabled);

        /// <summary>
        /// This serves as a wrapper for <see cref="GetVerse(Reference, bool, bool)"/>, when we have a verse reference
        /// string, usually of trustworthy composition.
        /// </summary>
        /// <remarks>
        /// In practice, this function will create a <see cref="Reference"/> out of the string by
        /// setting the <see cref="Reference.Book"/> to "str", <see cref="Reference.Version"/> to
        /// the provided version, and <see cref="Reference.AsString"/> to the string provided.
        /// <see cref="GetVerse(Reference, bool, bool)"/> is expected to handle <see cref="Reference"/>s of this nature.
        /// This function should not be used for user-generated verse reference strings.
        /// </remarks>
        /// <param name="reference">The reference whose content we are fetching.</param>
        /// <param name="titlesEnabled">Whether the content should include titles and other headings.</param>
        /// <param name="verseNumbersEnabled">Whether the content should include chapter and verse numbers.</param>
        /// <param name="version">The version we should fetch the content from.</param>
        /// <returns>
        /// If the <see cref="Reference"/> is valid, a <see cref="VerseResult"/> object populated with
        /// content according to the parameters given. If not, null.
        /// </returns>
        Task<VerseResult> GetVerse(string reference, bool titlesEnabled, bool verseNumbersEnabled, Version version);

        /// <summary>
        /// Handles search functionality from the provider, if available.
        /// </summary>
        /// <param name="query">The keyword(s) to search with.</param>
        /// <param name="version">The version that the search results will display in.</param>
        /// <returns>
        /// If available, a list of <see cref="SearchResult"/>s from the provider.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// Thrown when search handling has not been implemented, but the provider offers the functionality.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// Thrown when the provider does not offer search functionality.
        /// </exception>
        Task<List<SearchResult>> Search(string query, Version version);
    }
}
