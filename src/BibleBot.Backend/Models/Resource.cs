/*
* Copyright (C) 2016-2021 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;

namespace BibleBot.Backend.Models
{
    public class ParagraphedResource : IResource
    {
        public ResourceType Type { get; set; }
        public ResourceStyle Style { get; set; }
        public string CommandReference { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string ImageRef { get; set; }
        public string Copyright { get; set; }
        public string Category { get; set; }
        public List<Paragraph> Paragraphs { get; set; }
    }

    public class SectionedResource : IResource
    {
        public ResourceType Type { get; set; }
        public ResourceStyle Style { get; set; }
        public string CommandReference { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string ImageRef { get; set; }
        public string Copyright { get; set; }
        public string Category { get; set; }
        public List<Section> Sections { get; set; }
    }

    public class CreedResource : IResource
    {
        public ResourceType Type { get; set; }
        public ResourceStyle Style { get; set; }
        public string CommandReference { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string ImageRef { get; set; }
        public string Copyright { get; set; }
        public string Category { get; set; }
        public string Text { get; set; }
    }

    public class CreedFile
    {
        public CreedResource Apostles { get; set; }
        public CreedResource Nicene325 { get; set; }
        public CreedResource Nicene { get; set; }
        public CreedResource Chalcedon { get; set; }
    }

    public interface IResource
    {
        ResourceType Type { get; set; }
        ResourceStyle Style { get; set; }
        string CommandReference { get; set; }
        string Title { get; set; }
        string Author { get; set; }
        string ImageRef { get; set; }
        string Copyright { get; set; }
        string Category { get; set; }
    }

    public class Paragraph
    {
        public string Text { get; set; }
    }

    public class Section
    {
        public string Title { get; set; }
        public List<string> Slugs { get; set; }
        public List<string> Pages { get; set; }
    }

    public enum ResourceType
    {
        CREED = 0,
        CATECHISM = 1,
    }

    public enum ResourceStyle
    {
        PARAGRAPHED = 0,
        SECTIONED = 1,
        FULL_TEXT = 2
    }
}
