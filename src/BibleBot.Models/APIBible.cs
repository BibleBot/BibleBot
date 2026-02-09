/*
* Copyright (C) 2016-2026 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;

namespace BibleBot.Models
{

    /// <summary>
    /// Generic response wrapper for API.Bible list endpoints.
    /// </summary>
    /// <typeparam name="T">The type of data items in the response.</typeparam>
    public class ABListResponse<T>
    {
        /// <summary>
        /// The list of data items returned by the API.
        /// </summary>
        public List<T> Data { get; set; }
    }

    /// <summary>
    /// Response wrapper for API.Bible Bibles endpoint.
    /// </summary>
    public class ABBibleResponse : ABListResponse<ABBible> { }

    /// <summary>
    /// Response wrapper for API.Bible Books endpoint.
    /// </summary>
    public class ABBooksResponse : ABListResponse<ABBook> { }

    /// <summary>
    /// Response wrapper for API.Bible Chapters endpoint.
    /// </summary>
    public class ABChaptersResponse : ABListResponse<ABChapter> { }

    /// <summary>
    /// Response wrapper for API.Bible Verses endpoint.
    /// </summary>
    public class ABVersesResponse : ABListResponse<ABVerse> { }

    /// <summary>
    /// Response wrapper for API.Bible search endpoint.
    /// </summary>
    public class ABSearchResponse
    {
        /// <summary>
        /// The search query that was executed.
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// The search results data.
        /// </summary>
        public ABSearch Data { get; set; }

        /// <summary>
        /// Metadata associated with the search response.
        /// </summary>
        public ABMetadata Metadata { get; set; }
    }

    /// <summary>
    /// Represents a book in the Bible according to API.Bible.
    /// </summary>
    public class ABBook
    {
        /// <summary>
        /// Unique identifier for the book.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Identifier of the Bible version this book belongs to.
        /// </summary>
        public string BibleId { get; set; }

        /// <summary>
        /// Short abbreviation for the book (e.g., "Gen", "Matt").
        /// </summary>
        public string Abbreviation { get; set; }

        /// <summary>
        /// Standard name of the book (e.g., "Genesis", "Matthew").
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Full/long name of the book (e.g., "The Book of Genesis", "The Gospel According to Matthew").
        /// </summary>
        public string NameLong { get; set; }

        /// <summary>
        /// List of chapters in this book.
        /// </summary>
        public List<ABChapter> Chapters { get; set; }
    }

    /// <summary>
    /// Represents a chapter in a Bible book according to API.Bible.
    /// </summary>
    public class ABChapter
    {
        /// <summary>
        /// Unique identifier for the chapter.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Identifier of the Bible version this chapter belongs to.
        /// </summary>
        public string BibleId { get; set; }

        /// <summary>
        /// Chapter number as a string (e.g., "1", "2", "3").
        /// Note: This is stored as a string by API.Bible, not an integer.
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// Identifier of the book this chapter belongs to.
        /// </summary>
        public string BookId { get; set; }

        /// <summary>
        /// Human-readable reference for this chapter (e.g., "Genesis 1").
        /// </summary>
        public string Reference { get; set; }

        /// <summary>
        /// List of sections within this chapter.
        /// </summary>
        public List<ABSection> Sections { get; set; }

        // Note: There is an optional `int Position` property that can exist depending on the API response,
        // but it can be misleading because some versions have an intro "chapter" at position 0
        // which is technically incorrect. However, chapters are sorted properly in the result, so
        // we don't have to worry about position for ordering purposes.
    }

    /// <summary>
    /// Represents a Bible version/translation according to API.Bible.
    /// </summary>
    public class ABBible
    {
        /// <summary>
        /// Unique identifier for the Bible version.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Digital Bible Library identifier for this version.
        /// </summary>
        public string DBLId { get; set; }

        /// <summary>
        /// Standard abbreviation for the Bible version (e.g., "KJV", "NIV").
        /// </summary>
        public string Abbreviation { get; set; }

        /// <summary>
        /// Localized abbreviation for the Bible version.
        /// </summary>
        public string AbbreviationLocal { get; set; }

        /// <summary>
        /// Language information for this Bible version.
        /// </summary>
        public ABLanguage Language { get; set; }

        /// <summary>
        /// List of countries where this Bible version is used.
        /// </summary>
        public List<ABCountry> Countries { get; set; }

        /// <summary>
        /// Standard name of the Bible version (e.g., "King James Version").
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Localized name of the Bible version.
        /// </summary>
        public string NameLocal { get; set; }

        /// <summary>
        /// Description of the Bible version.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Localized description of the Bible version.
        /// </summary>
        public string DescriptionLocal { get; set; }

        /// <summary>
        /// Related Digital Bible Library reference.
        /// </summary>
        public string RelatedDBL { get; set; }

        /// <summary>
        /// Type of Bible version (e.g., "text", "audio").
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Timestamp of when this Bible version was last updated.
        /// </summary>
        public string UpdatedAt { get; set; }

        /// <summary>
        /// List of audio Bible versions associated with this text version.
        /// </summary>
        public List<ABAudioBible> AudioBibles { get; set; }
    }

    /// <summary>
    /// Represents language information for a Bible version according to API.Bible.
    /// </summary>
    public class ABLanguage
    {
        /// <summary>
        /// Unique identifier for the language.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Standard name of the language (e.g., "English").
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Localized name of the language in its native script.
        /// </summary>
        public string NameLocal { get; set; }

        /// <summary>
        /// Script system used by the language (e.g., "Latin", "Cyrillic").
        /// </summary>
        public string Script { get; set; }

        /// <summary>
        /// Text direction for the language (e.g., "ltr" for left-to-right, "rtl" for right-to-left).
        /// </summary>
        public string ScriptDirection { get; set; }
    }

    /// <summary>
    /// Represents country information for a Bible version according to API.Bible.
    /// </summary>
    public class ABCountry
    {
        /// <summary>
        /// Unique identifier for the country.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Standard name of the country (e.g., "United States").
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Localized name of the country in its native language.
        /// </summary>
        public string NameLocal { get; set; }
    }

    /// <summary>
    /// Represents an audio Bible version according to API.Bible.
    /// </summary>
    public class ABAudioBible
    {
        /// <summary>
        /// Unique identifier for the audio Bible version.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Standard name of the audio Bible version.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Localized name of the audio Bible version.
        /// </summary>
        public string NameLocal { get; set; }

        /// <summary>
        /// Description of the audio Bible version.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Localized description of the audio Bible version.
        /// </summary>
        public string DescriptionLocal { get; set; }
    }

    /// <summary>
    /// Represents search results from API.Bible search endpoints.
    /// </summary>
    public class ABSearch
    {
        /// <summary>
        /// The search query that was executed.
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// Maximum number of results requested.
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// Number of results skipped for pagination.
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// Total number of results available for the search query.
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Number of verses found in the search results.
        /// </summary>
        public int VerseCount { get; set; }

        /// <summary>
        /// List of individual verses found in the search.
        /// </summary>
        public List<ABVerse> Verses { get; set; }

        /// <summary>
        /// List of passages found in the search (grouped verses).
        /// </summary>
        public List<ABPassage> Passages { get; set; }
    }

    /// <summary>
    /// Represents a single verse according to API.Bible.
    /// </summary>
    public class ABVerse
    {
        /// <summary>
        /// Unique identifier for the verse.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Original identifier for the verse (may differ from Id in some contexts).
        /// </summary>
        public string OrgId { get; set; }

        /// <summary>
        /// Identifier of the Bible version this verse belongs to.
        /// </summary>
        public string BibleId { get; set; }

        /// <summary>
        /// Identifier of the book this verse belongs to.
        /// </summary>
        public string BookId { get; set; }

        /// <summary>
        /// Identifier of the chapter this verse belongs to.
        /// </summary>
        public string ChapterId { get; set; }

        /// <summary>
        /// The actual text content of the verse.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Human-readable reference for this verse (e.g., "John 3:16").
        /// </summary>
        public string Reference { get; set; }
    }

    /// <summary>
    /// Represents a passage (group of verses) according to API.Bible.
    /// </summary>
    public class ABPassage
    {
        /// <summary>
        /// Unique identifier for the passage.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Identifier of the Bible version this passage belongs to.
        /// </summary>
        public string BibleId { get; set; }

        /// <summary>
        /// Identifier of the book this passage belongs to.
        /// </summary>
        public string BookId { get; set; }

        /// <summary>
        /// Original identifier for the passage (may differ from Id in some contexts).
        /// </summary>
        public string OrgId { get; set; }

        /// <summary>
        /// The complete text content of the passage (all verses combined).
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Human-readable reference for this passage (e.g., "John 3:16-18").
        /// </summary>
        public string Reference { get; set; }

        /// <summary>
        /// Number of verses included in this passage.
        /// </summary>
        public int VerseCount { get; set; }

        /// <summary>
        /// Copyright information for the passage content.
        /// </summary>
        public string Copyright { get; set; }
    }

    /// <summary>
    /// Represents metadata for API.Bible responses, particularly for search results.
    /// Contains FUMS (Footnotes, User Markings, and Study Notes) related information.
    /// </summary>
    public class ABMetadata
    {
        /// <summary>
        /// FUMS (Footnotes, User Markings, and Study Notes) content.
        /// </summary>
        public string FUMS { get; set; }

        /// <summary>
        /// FUMS identifier.
        /// </summary>
        public string FUMSId { get; set; }

        /// <summary>
        /// JavaScript include for FUMS functionality.
        /// </summary>
        public string FUMSJSInclude { get; set; }

        /// <summary>
        /// JavaScript code for FUMS functionality.
        /// </summary>
        public string FUMSJS { get; set; }

        /// <summary>
        /// FUMS content for environments without JavaScript.
        /// </summary>
        public string FUMSNoScript { get; set; }
    }

    /// <summary>
    /// Represents a section within a chapter according to API.Bible.
    /// Sections are logical divisions within chapters (e.g., headings, paragraphs).
    /// </summary>
    public class ABSection
    {
        /// <summary>
        /// Unique identifier for the section.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Identifier of the Bible version this section belongs to.
        /// </summary>
        public string BibleId { get; set; }

        /// <summary>
        /// Identifier of the book this section belongs to.
        /// </summary>
        public string BookId { get; set; }

        /// <summary>
        /// Title or heading for this section.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Identifier of the first verse in this section.
        /// </summary>
        public string FirstVerseId { get; set; }

        /// <summary>
        /// Identifier of the last verse in this section.
        /// </summary>
        public string LastVerseId { get; set; }

        /// <summary>
        /// Original identifier of the first verse in this section.
        /// </summary>
        public string FirstVerseOrgId { get; set; }

        /// <summary>
        /// Original identifier of the last verse in this section.
        /// </summary>
        public string LastVerseOrgId { get; set; }
    }
}
