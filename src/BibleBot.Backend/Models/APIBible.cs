using System.Collections.Generic;

namespace BibleBot.Backend.Models
{
    public interface IABBibleResponse
    {
        List<IABBibleData> Data { get; set; }
    }

    public interface IABSearchResponse
    {
        string Query { get; set; }
        IABSearchData Data { get; set; }
        IABMetadata Metadata { get; set; }
    }

    public interface IABBookData
    {
        string Id { get; set; }
        string BibleId { get; set; }
        string Abbreviation { get; set; }
        string Name { get; set; }
        string NameLong { get; set; }
        List<IABChapter> Chapters { get; set; }
    }

    public interface IABChapter
    {
        string Id { get; set; }
        string BibleId { get; set; }
        string Number { get; set; } // no, really, it's a string
        string BookId { get; set; }
        string Reference { get; set; }
    }

    public interface IABBibleData
    {
        string Id { get; set; }
        string DBLId { get; set; }
        string Abbreviation { get; set; }
        string AbbreviationLocal { get; set; }
        IABLanguage Language { get; set; }
        List<IABCountry> Countries { get; set; }
        string Name { get; set; }
        string NameLocal { get; set; }
        string Description { get; set; }
        string DescriptionLocal { get; set; }
        string RelatedDBL { get; set; }
        string Type { get; set; }
        string UpdatedAt { get; set; }
        List<IABAudioBible> AudioBibles { get; set; }
    }

    public interface IABLanguage
    {
        string Id { get; set; }
        string Name { get; set; }
        string NameLocal { get; set; }
        string Script { get; set; }
        string ScriptDirection { get; set; }
    }

    public interface IABCountry
    {
        string Id { get; set; }
        string Name { get; set; }
        string NameLocal { get; set; }
    }

    public interface IABAudioBible
    {
        string Id { get; set; }
        string Name { get; set; }
        string NameLocal { get; set; }
        string Description { get; set; }
        string DescriptionLocal { get; set; }
    }

    public interface IABSearchData
    {
        string Query { get; set; }
        int Limit { get; set; }
        int Offset { get; set; }
        int Total { get; set; }
        int VerseCount { get; set; }
        List<IABVerse> Verses { get; set; }
        List<IABPassage> Passages { get; set; }
    }

    public interface IABVerse
    {
        string Id { get; set; }
        string OrgId { get; set; }
        string BibleId { get; set; }
        string BookId { get; set; }
        string ChapterId { get; set; }
        string Text { get; set; }
        string Reference { get; set; }
    }

    public interface IABPassage
    {
        string Id { get; set; }
        string BibleId { get; set; }
        string OrgId { get; set; }
        string Content { get; set; }
        string Reference { get; set; }
        int VerseCount { get; set; }
        string Copyright { get; set; }
    }

    public interface IABMetadata
    {
        string FUMS { get; set; }
        string FUMSId { get; set; }
        string FUMSJSInclude { get; set; }
        string FUMSJS { get; set; }
        string FUMSNoScript { get; set; }
    }
}