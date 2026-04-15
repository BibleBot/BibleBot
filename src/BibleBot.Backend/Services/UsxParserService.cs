/*
 * Copyright (C) 2016-2026 Kerygma Digital Co.
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using BibleBot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BibleBot.Backend.Services
{
    /// <summary>
    /// The result of parsing a single USX file.
    /// </summary>
    public class UsxBookResult
    {
        /// <summary>
        /// The book code from the USX file (e.g., "JHN", "PSA").
        /// </summary>
        public string BookCode { get; set; }

        /// <summary>
        /// Parsed chapters with their verses and titles.
        /// </summary>
        public List<UsxChapterResult> Chapters { get; set; } = [];
    }

    /// <summary>
    /// The result of parsing a single chapter from a USX file.
    /// </summary>
    public class UsxChapterResult
    {
        /// <summary>
        /// The chapter number.
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Individual verses extracted from the chapter.
        /// </summary>
        public List<UsxVerseResult> Verses { get; set; } = [];

        /// <summary>
        /// Section titles found in the chapter, mapped to the verse they precede.
        /// </summary>
        public List<ChapterTitle> Titles { get; set; } = [];
    }

    /// <summary>
    /// A single verse extracted from a USX file.
    /// </summary>
    public class UsxVerseResult
    {
        /// <summary>
        /// The verse number.
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// The plain-text content of the verse.
        /// </summary>
        public string Content { get; set; }
    }

    /// <summary>
    /// Service for parsing USX files and importing verse content into the database.
    /// </summary>
    public partial class UsxParserService(IServiceScopeFactory scopeFactory, ILogger<UsxParserService> logger)
    {
        [GeneratedRegex(@"\s{2,}", RegexOptions.Compiled)]
        private static partial Regex MultipleSpacesRegex();

        /// <summary>
        /// Parses a single USX file and returns the structured book data.
        /// </summary>
        /// <param name="filePath">The path to the .usx file.</param>
        /// <returns>The parsed book data, or null if the file could not be parsed.</returns>
        public UsxBookResult ParseFile(string filePath)
        {
            XmlDocument doc = new();
            doc.Load(filePath);

            XmlNode bookNode = doc.SelectSingleNode("//book");
            if (bookNode == null)
            {
                logger.LogWarning("No <book> element found in {FilePath}", filePath);
                return null;
            }

            string bookCode = bookNode.Attributes?["code"]?.Value;
            if (string.IsNullOrEmpty(bookCode))
            {
                logger.LogWarning("No book code attribute found in {FilePath}", filePath);
                return null;
            }

            UsxBookResult result = new() { BookCode = bookCode };

            XmlNode usxRoot = doc.SelectSingleNode("//usx");
            if (usxRoot == null)
            {
                logger.LogWarning("No <usx> root element found in {FilePath}", filePath);
                return null;
            }

            int currentChapter = 0;
            UsxChapterResult currentChapterResult = null;

            // State for tracking verse content
            int currentVerseNumber = 0;
            StringBuilder currentVerseContent = new();
            bool inVerse = false;

            // State for section titles
            string pendingTitle = null;

            foreach (XmlNode node in usxRoot.ChildNodes)
            {
                switch (node.Name)
                {
                    case "chapter":
                        // Flush any in-progress verse
                        if (inVerse && currentChapterResult != null)
                        {
                            FlushVerse(currentChapterResult, currentVerseNumber, currentVerseContent);
                            inVerse = false;
                        }

                        // Handle chapter start (sid) vs end (eid)
                        if (node.Attributes?["sid"] != null)
                        {
                            string numberStr = node.Attributes["number"]?.Value;
                            if (int.TryParse(numberStr, out int chapterNum))
                            {
                                currentChapter = chapterNum;
                                currentChapterResult = new UsxChapterResult { Number = chapterNum };
                                result.Chapters.Add(currentChapterResult);
                            }
                        }
                        break;

                    case "para":
                        if (currentChapterResult == null)
                        {
                            break;
                        }

                        ProcessParagraph(node, currentChapterResult, ref currentVerseNumber,
                            ref currentVerseContent, ref inVerse, ref pendingTitle);
                        break;
                    default:
                        logger.LogWarning("Unknown node type: {NodeType}", node.Name);
                        break;
                }
            }

            // Flush any remaining verse from the last chapter
            if (inVerse && currentChapterResult != null)
            {
                FlushVerse(currentChapterResult, currentVerseNumber, currentVerseContent);
            }

            return result;
        }

        private void ProcessParagraph(XmlNode paraNode, UsxChapterResult chapter,
            ref int currentVerseNumber, ref StringBuilder currentVerseContent,
            ref bool inVerse, ref string pendingTitle)
        {
            string style = paraNode.Attributes?["style"]?.Value ?? "";

            switch (style)
            {
                // Section headings
                case "s1" or "s2" or "s3":
                    pendingTitle = GetPlainTextContent(paraNode);
                    return;

                // Psalm superscription
                case "d":
                    pendingTitle = GetPlainTextContent(paraNode);
                    return;

                // Chapter label, main title, introductory content — skip
                case "cl" or "mt1" or "mt2" or "mt3" or "ms1" or "ms2"
                    or "mr" or "imt" or "im" or "ip" or "ie" or "rem"
                    or "h" or "toc1" or "toc2" or "toc3":
                    return;

                // Blank line — skip
                case "b":
                    // A blank para might have a vid attribute (continuing a verse through a stanza break)
                    // Just skip it - the verse content continues in the next para
                    return;
                default:
                    break;
            }

            // Content paragraphs (p, q1, q2, m, pi, etc.)
            // These contain verse content with <verse> elements

            // Check if this para continues a verse (vid attribute)
            string vid = paraNode.Attributes?["vid"]?.Value;
            bool isContinuation = !string.IsNullOrEmpty(vid);

            if (isContinuation && inVerse)
            {
                // Continuation of current verse — add a space separator then content
                currentVerseContent.Append(' ');
            }

            foreach (XmlNode child in paraNode.ChildNodes)
            {
                switch (child.Name)
                {
                    case "verse":
                        if (child.Attributes?["sid"] != null)
                        {
                            // Verse start — flush previous verse
                            if (inVerse)
                            {
                                FlushVerse(chapter, currentVerseNumber, currentVerseContent);
                            }

                            string numberStr = child.Attributes["number"]?.Value;
                            if (int.TryParse(numberStr, out int verseNum))
                            {
                                currentVerseNumber = verseNum;
                                currentVerseContent.Clear();
                                inVerse = true;

                                // Attach any pending title to this verse
                                if (pendingTitle != null)
                                {
                                    chapter.Titles.Add(new ChapterTitle
                                    {
                                        StartVerse = verseNum,
                                        EndVerse = verseNum,
                                        Title = pendingTitle
                                    });
                                    pendingTitle = null;
                                }
                            }
                        }
                        // eid attributes just mark the end boundary — we flush on the next sid
                        break;

                    case "char":
                        // Strip the tag, keep inner text (for wj, nd, tl, sc, qt, etc.)
                        currentVerseContent.Append(GetPlainTextContent(child));
                        break;

                    case "note":
                        // Strip entirely — footnotes, cross-references
                        break;

                    case "#text":
                        if (inVerse)
                        {
                            currentVerseContent.Append(child.Value);
                        }
                        break;

                    case "ref":
                        // Strip cross-reference elements
                        break;

                    default:
                        // Any other inline element — try to get its text content
                        if (inVerse)
                        {
                            currentVerseContent.Append(GetPlainTextContent(child));
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Extracts plain text from an XML node, recursively stripping all markup
        /// except note elements (which are removed entirely).
        /// </summary>
        private static string GetPlainTextContent(XmlNode node)
        {
            if (node.NodeType == XmlNodeType.Text)
            {
                return node.Value ?? "";
            }

            // Skip note elements entirely (footnotes, cross-references)
            if (node.Name is "note" or "ref")
            {
                return "";
            }

            StringBuilder sb = new();
            foreach (XmlNode child in node.ChildNodes)
            {
                sb.Append(GetPlainTextContent(child));
            }
            return sb.ToString();
        }

        private void FlushVerse(UsxChapterResult chapter, int verseNumber, StringBuilder content)
        {
            string text = CleanVerseText(content.ToString());
            if (!string.IsNullOrWhiteSpace(text))
            {
                chapter.Verses.Add(new UsxVerseResult
                {
                    Number = verseNumber,
                    Content = text
                });
            }
            content.Clear();
        }

        /// <summary>
        /// Cleans up verse text: trims whitespace, collapses multiple spaces, removes leading/trailing issues.
        /// </summary>
        private static string CleanVerseText(string text)
        {
            // Collapse multiple whitespace characters into single spaces
            text = MultipleSpacesRegex().Replace(text, " ");
            return text.Trim();
        }

        /// <summary>
        /// Imports all USX files from a directory into the database for a given version.
        /// </summary>
        /// <param name="directoryPath">The path to the directory containing .usx files.</param>
        /// <param name="versionId">The version abbreviation (e.g., "NIV").</param>
        /// <returns>A summary of the import operation.</returns>
        public async Task<UsxImportSummary> ImportDirectory(string directoryPath, string versionId)
        {
            UsxImportSummary summary = new();

            if (!Directory.Exists(directoryPath))
            {
                logger.LogError("Directory not found: {DirectoryPath}", directoryPath);
                summary.Errors.Add($"Directory not found: {directoryPath}");
                return summary;
            }

            string[] usxFiles = Directory.GetFiles(directoryPath, "*.usx");
            if (usxFiles.Length == 0)
            {
                logger.LogError("No .usx files found in {DirectoryPath}", directoryPath);
                summary.Errors.Add($"No .usx files found in {directoryPath}");
                return summary;
            }

            logger.LogInformation("Found {FileCount} USX files in {DirectoryPath}", usxFiles.Length, directoryPath);

            using IServiceScope scope = scopeFactory.CreateScope();
            PgContext context = scope.ServiceProvider.GetRequiredService<PgContext>();

            // Verify the version exists
            BibleBot.Models.Version version = await context.Versions.FirstOrDefaultAsync(v => v.Id == versionId);
            if (version == null)
            {
                logger.LogError("Version '{VersionId}' not found in database", versionId);
                summary.Errors.Add($"Version '{versionId}' not found in database");
                return summary;
            }

            // Pre-load all books for this version with their chapters
            List<Book> books = await context.Books
                .Where(b => b.VersionId == versionId)
                .Include(b => b.Chapters)
                .ToListAsync();

            if (books.Count == 0)
            {
                logger.LogError("No books found for version '{VersionId}'", versionId);
                summary.Errors.Add($"No books found for version '{versionId}'. Run metadata fetching first.");
                return summary;
            }

            logger.LogInformation("Loaded {BookCount} books for version '{VersionId}'", books.Count, versionId);

            foreach (string filePath in usxFiles)
            {
                try
                {
                    await ImportSingleFile(context, books, filePath, summary);
                }
                catch (Exception ex)
                {
                    string fileName = Path.GetFileName(filePath);
                    logger.LogError(ex, "Error importing {FileName}", fileName);
                    summary.Errors.Add($"Error importing {fileName}: {ex.Message}");
                }
            }

            logger.LogInformation("Import complete: {BooksProcessed} books, {VersesInserted} verses inserted, {VersesUpdated} verses updated, {TitlesUpdated} titles updated",
                summary.BooksProcessed, summary.VersesInserted, summary.VersesUpdated, summary.TitlesUpdated);

            return summary;
        }

        private async Task ImportSingleFile(PgContext context, List<Book> books, string filePath, UsxImportSummary summary)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            UsxBookResult parsed = ParseFile(filePath);
            if (parsed == null)
            {
                summary.BooksSkipped.Add(fileName);
                return;
            }

            // Match the USX book code to a database book
            Book book = books.FirstOrDefault(b =>
                string.Equals(b.Name, parsed.BookCode, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(b.InternalName, parsed.BookCode, StringComparison.OrdinalIgnoreCase));

            if (book == null)
            {
                logger.LogWarning("No matching book found for code '{BookCode}' in the database (file: {FileName})", parsed.BookCode, fileName);
                summary.BooksSkipped.Add($"{fileName} ({parsed.BookCode})");
                return;
            }

            logger.LogInformation("Importing {BookCode} ({ProperName}) — {ChapterCount} chapters",
                parsed.BookCode, book.ProperName, parsed.Chapters.Count);

            foreach (UsxChapterResult usxChapter in parsed.Chapters)
            {
                Chapter dbChapter = book.Chapters.FirstOrDefault(c => c.Number == usxChapter.Number);
                if (dbChapter == null)
                {
                    logger.LogWarning("No matching chapter {ChapterNum} for {BookCode}", usxChapter.Number, parsed.BookCode);
                    continue;
                }

                // Load existing verses for this chapter
                List<Verse> existingVerses = await context.Verses
                    .Where(v => v.ChapterId == dbChapter.Id)
                    .ToListAsync();

                foreach (UsxVerseResult usxVerse in usxChapter.Verses)
                {
                    Verse existing = existingVerses.FirstOrDefault(v => v.Number == usxVerse.Number);
                    if (existing != null)
                    {
                        existing.Content = usxVerse.Content;
                        existing.Source = "usx";
                        existing.FetchedAt = DateTime.UtcNow;
                        summary.VersesUpdated++;
                    }
                    else
                    {
                        context.Verses.Add(new Verse
                        {
                            ChapterId = dbChapter.Id,
                            Number = usxVerse.Number,
                            Content = usxVerse.Content,
                            Source = "usx",
                            FetchedAt = DateTime.UtcNow
                        });
                        summary.VersesInserted++;
                    }
                }

                // Update chapter titles if we extracted any
                if (usxChapter.Titles.Count > 0)
                {
                    foreach (ChapterTitle title in usxChapter.Titles.Where(title => title.StartVerse == title.EndVerse))
                    {
                        title.EndVerse = usxChapter.Verses.Max(v => v.Number);
                    }

                    dbChapter.Titles = usxChapter.Titles;
                    summary.TitlesUpdated++;
                }

                await context.SaveChangesAsync();
            }

            summary.BooksProcessed++;
        }
    }

    /// <summary>
    /// Summary of a USX import operation.
    /// </summary>
    public class UsxImportSummary
    {
        public int BooksProcessed { get; set; }
        public int VersesInserted { get; set; }
        public int VersesUpdated { get; set; }
        public int TitlesUpdated { get; set; }
        public List<string> BooksSkipped { get; set; } = [];
        public List<string> Errors { get; set; } = [];
    }
}
