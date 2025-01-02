/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;

// TODO(srp): Docstrings, eventually...
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace BibleBot.Models
{
    public class ABBibleResponse
    {
        public List<ABBibleData> Data { get; set; }
    }

    public class ABSearchResponse
    {
        public string Query { get; set; }
        public ABSearchData Data { get; set; }
        public ABMetadata Metadata { get; set; }
    }

    public class ABBookData
    {
        public string Id { get; set; }
        public string BibleId { get; set; }
        public string Abbreviation { get; set; }
        public string Name { get; set; }
        public string NameLong { get; set; }
        public List<ABChapter> Chapters { get; set; }
    }

    public class ABChapter
    {
        public string Id { get; set; }
        public string BibleId { get; set; }
        public string Number { get; set; } // no, really, it's a string
        public string BookId { get; set; }
        public string Reference { get; set; }
    }

    public class ABBibleData
    {
        public string Id { get; set; }
        public string DBLId { get; set; }
        public string Abbreviation { get; set; }
        public string AbbreviationLocal { get; set; }
        public ABLanguage Language { get; set; }
        public List<ABCountry> Countries { get; set; }
        public string Name { get; set; }
        public string NameLocal { get; set; }
        public string Description { get; set; }
        public string DescriptionLocal { get; set; }
        public string RelatedDBL { get; set; }
        public string Type { get; set; }
        public string UpdatedAt { get; set; }
        public List<ABAudioBible> AudioBibles { get; set; }
    }

    public class ABLanguage
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string NameLocal { get; set; }
        public string Script { get; set; }
        public string ScriptDirection { get; set; }
    }

    public class ABCountry
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string NameLocal { get; set; }
    }

    public class ABAudioBible
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string NameLocal { get; set; }
        public string Description { get; set; }
        public string DescriptionLocal { get; set; }
    }

    public class ABSearchData
    {
        public string Query { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public int Total { get; set; }
        public int VerseCount { get; set; }
        public List<ABVerse> Verses { get; set; }
        public List<ABPassage> Passages { get; set; }
    }

    public class ABVerse
    {
        public string Id { get; set; }
        public string OrgId { get; set; }
        public string BibleId { get; set; }
        public string BookId { get; set; }
        public string ChapterId { get; set; }
        public string Text { get; set; }
        public string Reference { get; set; }
    }

    public class ABPassage
    {
        public string Id { get; set; }
        public string BibleId { get; set; }
        public string OrgId { get; set; }
        public string Content { get; set; }
        public string Reference { get; set; }
        public int VerseCount { get; set; }
        public string Copyright { get; set; }
    }

    public class ABMetadata
    {
        public string FUMS { get; set; }
        public string FUMSId { get; set; }
        public string FUMSJSInclude { get; set; }
        public string FUMSJS { get; set; }
        public string FUMSNoScript { get; set; }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member