/*
* Copyright (C) 2016-2024 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

namespace BibleBot.Models
{
    /// <summary>
    /// Not to be confused with <see cref="SearchResult"/>, BookSearchResult
    /// is used to mark when a book name is mentioned in a string.
    /// </summary>
    public class BookSearchResult
    {
        /// <summary>
        /// The book name found in a string, represented by its data name in the book map.
        /// </summary>
        /// <value>
        /// For example, if "Greek Esther" was found in a string, this would be "gkest".
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// The index that the book name was found in a tokenized version of the string.
        /// </summary>
        /// <value>
        /// For example, if "Greek Esther" was found in a string, this would be the index
        /// of "Greek" in the tokenized string.
        /// </value>
        public int Index { get; set; }
    }
}
