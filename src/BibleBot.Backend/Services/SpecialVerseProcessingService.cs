/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BibleBot.Backend.Services.Providers;
using BibleBot.Models;
using Version = BibleBot.Models.Version;

namespace BibleBot.Backend.Services
{
    public class SpecialVerseProcessingService(ParsingService parsingService, MetadataFetchingService metadataFetchingService,
        VersionService versionService, SpecialVerseProvider specialVerseProvider, List<IContentProvider> bibleProviders)
    {

        /// <summary>
        /// Gets a daily verse using the proper parsing pipeline, similar to VersesController
        /// </summary>
        /// <param name="version">The version to use for the verse</param>
        /// <param name="titlesEnabled">Whether titles should be included</param>
        /// <param name="verseNumbersEnabled">Whether verse numbers should be included</param>
        /// <returns>A VerseResult with the daily verse, or null if processing failed</returns>
        public async Task<VerseResult> GetDailyVerse(Version version, bool titlesEnabled, bool verseNumbersEnabled)
        {
            string votdRef = await specialVerseProvider.GetDailyVerse();
            return await ProcessSpecialVerseReference(votdRef, version, titlesEnabled, verseNumbersEnabled);
        }

        /// <summary>
        /// Gets a random verse using the proper parsing pipeline, similar to VersesController
        /// </summary>
        /// <param name="version">The version to use for the verse</param>
        /// <param name="titlesEnabled">Whether titles should be included</param>
        /// <param name="verseNumbersEnabled">Whether verse numbers should be included</param>
        /// <returns>A VerseResult with the random verse, or null if processing failed</returns>
        public async Task<VerseResult> GetRandomVerse(Version version, bool titlesEnabled, bool verseNumbersEnabled)
        {
            string randomRef = await specialVerseProvider.GetRandomVerse();
            return await ProcessSpecialVerseReference(randomRef, version, titlesEnabled, verseNumbersEnabled);
        }

        /// <summary>
        /// Gets a truly random verse using the proper parsing pipeline, similar to VersesController
        /// </summary>
        /// <param name="version">The version to use for the verse</param>
        /// <param name="titlesEnabled">Whether titles should be included</param>
        /// <param name="verseNumbersEnabled">Whether verse numbers should be included</param>
        /// <returns>A VerseResult with the truly random verse, or null if processing failed</returns>
        public async Task<VerseResult> GetTrulyRandomVerse(Version version, bool titlesEnabled, bool verseNumbersEnabled)
        {
            string trulyRandomRef = await specialVerseProvider.GetTrulyRandomVerse();
            return await ProcessSpecialVerseReference(trulyRandomRef, version, titlesEnabled, verseNumbersEnabled);
        }

        /// <summary>
        /// Processes a special verse reference using the same pipeline as VersesController
        /// </summary>
        /// <param name="referenceString">The reference string to process</param>
        /// <param name="version">The version to use for the verse</param>
        /// <param name="titlesEnabled">Whether titles should be included</param>
        /// <param name="verseNumbersEnabled">Whether verse numbers should be included</param>
        /// <returns>A VerseResult with the processed verse, or null if processing failed</returns>
        private async Task<VerseResult> ProcessSpecialVerseReference(string referenceString, Version version, bool titlesEnabled, bool verseNumbersEnabled)
        {
            // Get all available versions for parsing
            List<Version> versions = await versionService.Get();

            // Get book names for parsing
            Dictionary<string, List<string>> bookNames = metadataFetchingService.GetBookNames();
            List<string> defaultNames = metadataFetchingService.GetDefaultBookNames();

            // Parse the reference string using the same pipeline as VersesController
            System.Tuple<string, List<BookSearchResult>> tuple = parsingService.GetBooksInString(bookNames, defaultNames, referenceString);

            if (tuple.Item2.Count == 0)
            {
                return null;
            }

            // Generate the reference using the first book found
            Reference reference = parsingService.GenerateReference(tuple.Item1, tuple.Item2[0], version, versions);

            if (reference == null)
            {
                return null;
            }

            // Validate version support
            if (reference.IsOT && !reference.Version.SupportsOldTestament)
            {
                return null;
            }

            if (reference.IsNT && !reference.Version.SupportsNewTestament)
            {
                return null;
            }

            if (reference.IsDEU && !reference.Version.SupportsDeuterocanon)
            {
                return null;
            }

            // Handle special book cases
            if (reference.Book is { InternalName: "addesth" or "praz" or "epjer" })
            {
                reference.Book.ProperName = reference.Book.PreferredName;
            }

            // Find the appropriate provider
            IContentProvider provider = bibleProviders.FirstOrDefault(p => p.Name == reference.Version.Source);
            if (provider == null)
            {
                return null;
            }

            // Get the verse using the proper Reference object
            return await provider.GetVerse(reference, titlesEnabled, verseNumbersEnabled);
        }
    }
}