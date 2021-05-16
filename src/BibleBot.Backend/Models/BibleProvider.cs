using System.Threading.Tasks;
using System.Collections.Generic;

using BibleBot.Lib;

namespace BibleBot.Backend.Models
{
    public interface IBibleProvider
    {
        string Name { get; set; }
        Task<Verse> GetVerse(Reference reference, bool titlesEnabled, bool verseNumbersEnabled);
        Task<Dictionary<string, string>> Search(string query, Version version);
    }
}