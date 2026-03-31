/*
* Copyright (C) 2016-2026 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BibleBot.Models
{
    /// <summary>
    /// The model for Bible versions.
    /// </summary>
    public class Version
    {
        /// <summary>
        /// The abbreviated name of the version.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Whether the version is active/live on the service.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// The version that this version is an alias for. (ex. NRSV -> NRSVA)
        /// </summary>
        public string AliasOfId { get; set; }

        /// <summary>
        /// The name of the version.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The source of the version, correlating to a <see cref="IContentProvider.Name"/>.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// The publisher of the version.
        /// </summary>
        /// <remarks>
        /// This is currently only intended for use in the frontend to fulfill license agreement obligations.
        /// </remarks>
        public string Publisher { get; set; }

        /// <summary>
        /// The locale of the version.
        /// </summary>
        public string Locale { get; set; }

        /// <summary>
        /// The source's internal ID for the version.
        /// </summary>
        public string InternalId { get; set; }

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
        /// Indicates whether the OT follows Septuagint numberings where divergent from the Masoretic.
        /// </summary>
        /// <remarks>
        /// This may be prone to errors in divergences outside the Psalms, as some versions are loose with Septuagint numbering.
        /// <br/><br/>
        /// This is also intended primarily for ensuring consistency in Septuagint-numbered versions for special verses.
        /// </remarks>
        public bool FollowsSeptuagintNumbering { get; set; }

        /// <summary>
        /// An array of books, which contains an array of chapters along with other metadata.
        /// </summary>
        public List<Book> Books { get; set; } = [];
    }

    /// <summary>
    /// A representation of a book data object.
    /// </summary>
    public class Book
    {
        /// <summary>
        /// The internal database ID of the book.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The version ID this book belongs to.
        /// </summary>
        public string VersionId { get; set; }

        /// <summary>
        /// The data name of the book, like "1TI".
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The proper English name of the book, like "1 Timothy".
        /// </summary>
        public string ProperName { get; set; }

        /// <summary>
        /// The internal data name of the book, according to its source.
        /// </summary>
        public string InternalName { get; set; }

        /// <summary>
        /// The preferred name according to the version's source.
        /// For API.Bible, this corresponds to <see cref="ABBook.Name"/>.
        /// </summary>
        public string PreferredName { get; set; }

        /// <summary>
        /// An array of chapters, which contains an array of verse data.
        /// </summary>
        public List<Chapter> Chapters { get; set; } = [];

        /// <inheritdoc/>
        public override string ToString() => ProperName;
    }

    /// <summary>
    /// A representation of a chapter data object.
    /// </summary>
    public class Chapter
    {
        /// <summary>
        /// The internal database ID of the chapter.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The book ID this chapter belongs to.
        /// </summary>
        public int BookId { get; set; }

        /// <summary>
        /// The chapter number. This field exists primarily to aid LINQ queries, as this is ordinarily part of a sorted list.
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// An array of title headings for the chapter.
        /// </summary>
        [JsonConverter(typeof(ChapterTitleListConverter))]
        public List<ChapterTitle> Titles { get; set; } = [];

        /// <summary>
        /// An array of verse data.
        /// </summary>
        public List<Verse> Verses { get; set; } = [];

        /// <inheritdoc/>
        public override string ToString() => Number.ToString();
    }

    /// <summary>
    /// A representation of a verse data object.
    /// </summary>
    public class Verse
    {
        /// <summary>
        /// The internal database ID of the verse.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The chapter ID this verse belongs to.
        /// </summary>
        public int ChapterId { get; set; }

        /// <summary>
        /// The verse number. This field exists primarily to aid LINQ queries, as this is ordinarily part of a sorted list.
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// The content of a verse.
        /// </summary>
        public string Content { get; set; }
    }
}
