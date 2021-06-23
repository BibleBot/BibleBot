/*
* Copyright (C) 2016-2021 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using System.Threading.Tasks;
using BibleBot.Lib;

namespace BibleBot.Backend.Models
{
    public interface IBibleProvider
    {
        string Name { get; set; }
        Task<Verse> GetVerse(Reference reference, bool titlesEnabled, bool verseNumbersEnabled);
        Task<Verse> GetVerse(string reference, bool titlesEnabled, bool verseNumbersEnabled, Version version);
        Task<List<SearchResult>> Search(string query, Version version);
    }
}
