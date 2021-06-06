using System.Collections.Generic;

namespace BibleBot.Backend.Models
{
    public class ParagraphedResource : IResource
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string ImageRef { get; set; }
        public string Copyright { get; set; }
        public string Category { get; set; }
        public List<IParagraph> Paragraphs { get; set; }
    }

    public class SectionedResource : IResource
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string ImageRef { get; set; }
        public string Copyright { get; set; }
        public string Category { get; set; }
        public List<ISection> Sections { get; set; }
    }

    public class CreedResource : IResource
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string ImageRef { get; set; }
        public string Copyright { get; set; }
        public string Category { get; set; }
        public string Text { get; set; }
    }

    public interface IResource
    {
        string Title { get; set; }
        string Author { get; set; }
        string ImageRef { get; set; }
        string Copyright { get; set; }
        string Category { get; set; }
    }

    public interface IParagraph
    {
        string Text { get; set; }
    }

    public interface ISection
    {
        string Title { get; set; }
        List<string> Slugs { get; set; }
        List<string> Pages { get; set; }
    }
}