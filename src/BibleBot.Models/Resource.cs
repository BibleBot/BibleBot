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
    /// The model for paragraphed-style resources, like canon laws and some catechisms.
    /// </summary>
    public class ParagraphedResource : IResource
    {
        /// <inheritdoc/>
        public ResourceType Type { get; set; }
        /// <inheritdoc/>
        public ResourceStyle Style { get; set; } = ResourceStyle.PARAGRAPHED;
        /// <inheritdoc/>
        public string CommandReference { get; set; }
        /// <inheritdoc/>
        public string Title { get; set; }
        /// <inheritdoc/>
        public string Author { get; set; }
        /// <inheritdoc/>
        public string ImageRef { get; set; }
        /// <inheritdoc/>
        public string Copyright { get; set; }
        /// <inheritdoc/>
        public string Category { get; set; }

        /// <summary>
        /// The content of the resource.
        /// </summary>
        public List<Paragraph> Paragraphs { get; set; }
    }

    /// <summary>
    /// The model for sectioned-style resources, like some catechisms and standard book-like resources.
    /// </summary>
    public class SectionedResource : IResource
    {
        /// <inheritdoc/>
        public ResourceType Type { get; set; }
        /// <inheritdoc/>
        public ResourceStyle Style { get; set; }
        /// <inheritdoc/>
        public string CommandReference { get; set; }
        /// <inheritdoc/>
        public string Title { get; set; }
        /// <inheritdoc/>
        public string Author { get; set; }
        /// <inheritdoc/>
        public string ImageRef { get; set; }
        /// <inheritdoc/>
        public string Copyright { get; set; }
        /// <inheritdoc/>
        public string Category { get; set; }

        /// <summary>
        /// The content of the resource.
        /// </summary>
        public List<Section> Sections { get; set; }
    }

    /// <summary>
    /// The model for creed resources.
    /// </summary>
    public class CreedResource : IResource
    {
        /// <inheritdoc/>
        public ResourceType Type { get; set; }
        /// <inheritdoc/>
        public ResourceStyle Style { get; set; }
        /// <inheritdoc/>
        public string CommandReference { get; set; }
        /// <inheritdoc/>
        public string Title { get; set; }
        /// <inheritdoc/>
        public string Author { get; set; }
        /// <inheritdoc/>
        public string ImageRef { get; set; }
        /// <inheritdoc/>
        public string Copyright { get; set; }
        /// <inheritdoc/>
        public string Category { get; set; }

        /// <summary>
        /// The content of the resource.
        /// </summary>
        public string Text { get; set; }
    }

    /// <summary>
    /// The model representing a creed file.
    /// </summary>
    public class CreedFile
    {
        /// <inheritdoc/>
        public CreedResource Apostles { get; set; }
        /// <inheritdoc/>
        public CreedResource Nicene325 { get; set; }
        /// <inheritdoc/>
        public CreedResource Nicene { get; set; }
        /// <inheritdoc/>
        public CreedResource Chalcedon { get; set; }
    }

    /// <summary>
    /// An interface describing the implementation of a Resource.
    /// </summary>
    public interface IResource
    {
        /// <summary>
        /// The type of the resource. BibleBot currently supports canon laws, catechisms, and creeds.
        /// </summary>
        ResourceType Type { get; set; }

        /// <summary>
        /// The style of the resource.
        /// </summary>
        ResourceStyle Style { get; set; }

        /// <summary>
        /// The slug of the resource.
        /// </summary>
        string CommandReference { get; set; }

        /// <summary>
        /// The title of the resource.
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// The author of the resource.
        /// </summary>
        string Author { get; set; }

        /// <summary>
        /// The reference for the cover image.
        /// </summary>
        /// <remarks>
        /// This is currently the ID given to an Imgur upload.
        /// </remarks>
        string ImageRef { get; set; }

        /// <summary>
        /// The copyright notice of the resource.
        /// </summary>
        string Copyright { get; set; }

        /// <summary>
        /// The category of the resource.
        /// </summary>
        /// <remarks>
        /// This is hardly useful and may be removed in a later update.
        /// </remarks>
        string Category { get; set; }
    }

    /// <summary>
    /// The model representing an individual paragraph.
    /// </summary>
    public class Paragraph
    {
        /// <summary>
        /// The content of the paragraph.
        /// </summary>
        public string Text { get; set; }
    }

    /// <summary>
    /// The model representing an individual section.
    /// </summary>
    public class Section
    {
        /// <summary>
        /// The title of the section.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The slug(s) of the content.
        /// </summary>
        public List<string> Slugs { get; set; }

        /// <summary>
        /// The content of the section.
        /// </summary>
        public List<string> Pages { get; set; }
    }

    /// <summary>
    /// An enum of resource types, for easier handling.
    /// </summary>
    public enum ResourceType
    {
        /// <inheritdoc/>
        CREED = 0,
        /// <inheritdoc/>
        CATECHISM = 1,
        /// <inheritdoc/>
        CANONS = 2
    }

    /// <summary>
    /// An enum of resource styles, for easier handling.
    /// </summary>
    public enum ResourceStyle
    {
        /// <inheritdoc/>
        PARAGRAPHED = 0,
        /// <inheritdoc/>
        SECTIONED = 1,
        /// <inheritdoc/>
        FULL_TEXT = 2
    }
}
