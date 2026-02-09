/*
* Copyright (C) 2016-2026 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BibleBot.Models
{
    /// <summary>
    /// The model for Bible versions.
    /// </summary>
    public class Version
    {
        /// <summary>
        /// The internal database ID.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        /// <summary>
        /// Whether the version is active/live on the service.
        /// </summary>
        [BsonElement("Active")]
        public bool Active { get; set; }

        /// <summary>
        /// The version that this version is an alias for. (ex. NRSV -> NRSVA)
        /// </summary>
        [BsonElement("AliasOf")]
        public string AliasOf { get; set; }

        /// <summary>
        /// The name of the version.
        /// </summary>
        [BsonElement("Name")]
        public string Name { get; set; }

        /// <summary>
        /// The abbreviated name of the version.
        /// </summary>
        /// <remarks>
        /// In hindsight, maybe this should have been "Acronym" given that's what most of these are
        /// but this was a decision made by December 2016 Seraphim, and we don't question December 2016 Seraphim.
        /// </remarks>
        [BsonElement("Abbreviation")]
        public string Abbreviation { get; set; }

        /// <summary>
        /// The source of the version, correlating to a <see cref="IContentProvider.Name"/>.
        /// </summary>
        [BsonElement("Source")]
        public string Source { get; set; }

        /// <summary>
        /// The publisher of the version.
        /// </summary>
        /// <remarks>
        /// This is currently only intended for use in the frontend to fulfill license agreement obligations.
        /// </remarks>
        [BsonElement("Publisher")]
        public string Publisher { get; set; }

        /// <summary>
        /// The locale of the version.
        /// </summary>
        [BsonElement("Locale")]
        public string Locale { get; set; }

        /// <summary>
        /// The source's internal ID for the version.
        /// </summary>
        [BsonElement("InternalId")]
        public string InternalId { get; set; }

        /// <summary>
        /// Indicates whether the version supports Old Testament books.
        /// </summary>
        [BsonElement("SupportsOldTestament")]
        public bool SupportsOldTestament { get; set; }

        /// <summary>
        /// Indicates whether the version supports New Testament books.
        /// </summary>
        [BsonElement("SupportsNewTestament")]
        public bool SupportsNewTestament { get; set; }

        /// <summary>
        /// Indicates whether the version supports Deuterocanon books.
        /// </summary>
        [BsonElement("SupportsDeuterocanon")]
        public bool SupportsDeuterocanon { get; set; }

        /// <summary>
        /// Indicates whether the OT follows Septuagint numberings where divergent from the Masoretic.
        /// </summary>
        /// <remarks>
        /// This may be prone to errors in divergences outside the Psalms, as some versions are loose with Septuagint numbering.
        /// <br/><br/>
        /// This is also intended primarily for ensuring consistency in Septuagint-numbered versions for special verses.
        /// </remarks>
        [BsonElement("FollowsSeptuagintNumbering")]
        public bool FollowsSeptuagintNumbering { get; set; }

        /// <summary>
        /// An array of books, which contains an array of chapters along with other metadata.
        /// </summary>
        [BsonElement("Books")]
        public List<Book> Books { get; set; }
    }

    /// <summary>
    /// A representation of a book data object.
    /// </summary>
    public class Book
    {
        /// <summary>
        /// The data name of the book, like "1TI".
        /// </summary>
        [BsonElement("Name")]
        public string Name { get; set; }

        /// <summary>
        /// The proper English name of the book, like "1 Timothy".
        /// </summary>
        [BsonElement("ProperName")]
        public string ProperName { get; set; }

        /// <summary>
        /// The internal data name of the book, according to its source.
        /// </summary>
        [BsonElement("InternalName")]
        public string InternalName { get; set; }

        /// <summary>
        /// The preferred name according to the version's source.
        /// For API.Bible, this corresponds to <see cref="ABBook.Name"/>.
        /// </summary>
        [BsonElement("PreferredName")]
        public string PreferredName { get; set; }

        /// <summary>
        /// An array of chapters, which contains an array of verse data.
        /// </summary>
        [BsonElement("Chapters")]
        public List<Chapter> Chapters { get; set; }

        /// <inheritdoc/>
        public override string ToString() => ProperName;
    }

    /// <summary>
    /// A representation of a chapter data object.
    /// </summary>
    public class Chapter
    {
        /// <summary>
        /// The chapter number. This field exists primarily to aid LINQ queries, as this is ordinarily part of a sorted list.
        /// </summary>
        [BsonElement("Number")]
        public int Number { get; set; }

        /// <summary>
        /// An array of tuples for title headings, where Item1 is the beginning verse number the title exists, Item2 is the ending verse number, and Item3 is the title itself.
        /// </summary>
        [BsonElement("Titles")]
        public List<System.Tuple<int, int, string>> Titles { get; set; }

        /// <summary>
        /// An array of verse data.
        /// </summary>
        [BsonElement("Verses")]
        public List<Verse> Verses { get; set; }

        /// <inheritdoc/>
        public override string ToString() => Number.ToString();
    }

    /// <summary>
    /// A representation of a verse data object.
    /// </summary>
    public class Verse
    {
        /// <summary>
        /// The verse number. This field exists primarily to aid LINQ queries, as this is ordinarily part of a sorted list.
        /// </summary>
        [BsonElement("Number")]
        public int Number { get; set; }

        /// <summary>
        /// The content of a verse.
        /// </summary>
        [BsonElement("Content")]
        public string Content { get; set; }
    }
}
