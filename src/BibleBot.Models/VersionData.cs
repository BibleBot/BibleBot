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
    /// The model for the version data index.
    /// </summary>
    public class VersionData
    {
        /// <summary>
        /// The internal ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The name of the version.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The abbreviated name of the version.
        /// </summary>
        /// <remarks>
        /// In hindsight, maybe this should have been "Acronym" given that's what most of these are
        /// but this was a decision made by December 2016 Seraphim, and we don't question December 2016 Seraphim.
        /// </remarks>
        public string Abbreviation { get; set; }

        /// <summary>
        /// Indicates whether the version supports Old Testament books.
        /// </summary>
        public bool SupportsOldTestament { get; set; }

        /// <summary>
        /// Indicates whether the version supports New Testament books.
        /// </summary>
        public bool SupportsNewTestament { get; set; }

        /// <summary>
        /// Indicates whether the version supports Deuterocanon books.
        /// </summary>
        public bool SupportsDeuterocanon { get; set; }

        /// <summary>
        /// A list of book data objects, representing the various books.
        /// </summary>
        public List<BookData> bookData;
    }

    /// <summary>
    /// A representation of a book data object.
    /// </summary>
    public class BookData
    {
        /// <summary>
        /// The data name of the book, like "1tim".
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The proper name of the book, like "1 Timothy".
        /// </summary>
        public string ProperName { get; set; }

        /// <summary>
        /// An array of chapters, which contains an array of verse data.
        /// </summary>
        public List<VerseData[]> Chapters;
    }

    /// <summary>
    /// A representation of a verse data object.
    /// </summary>
    public class VerseData
    {
        /// <summary>
        /// The content of a verse.
        /// </summary>
        public string Content { get; set; }
    }
}
