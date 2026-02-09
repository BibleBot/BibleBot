/*
* Copyright (C) 2016-2026 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using System.Threading.Tasks;
using BibleBot.Models;

namespace BibleBot.Backend.Services.Providers.Content
{
    public partial class HouseProvider : IContentProvider
    {
        public string Name { get; set; }

        public HouseProvider() => Name = "bb";

        public async Task<VerseResult> GetVerse(Reference reference, bool titlesEnabled, bool verseNumbersEnabled) => null;

        public async Task<VerseResult> GetVerse(string reference, bool titlesEnabled, bool verseNumbersEnabled, Version version) => await GetVerse(new Reference { Book = null, Version = version, AsString = reference }, titlesEnabled, verseNumbersEnabled);

        public async Task<List<SearchResult>> Search(string query, Version version) => [];
    }
}
