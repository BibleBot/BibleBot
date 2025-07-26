/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace BibleBot.Models
{
    /// <summary>
    /// The model for verse references.
    /// </summary>
    public class Reference
    {
        internal readonly Dictionary<string, string> _nameToId = new()
        {
            { "Genesis", "GEN" },
            { "Exodus", "EXO" },
            { "Leviticus", "LEV" },
            { "Numbers", "NUM" },
            { "Deuteronomy", "DEU" },
            { "Joshua", "JOS" },
            { "Judges", "JDG" },
            { "Ruth", "RUT" },
            { "1 Samuel", "1SA" },
            { "2 Samuel", "2SA" },
            { "1 Kings", "1KI" },
            { "2 Kings", "2KI" },
            { "1 Chronicles", "1CH" },
            { "2 Chronicles", "2CH" },
            { "Ezra", "EZR" },
            { "Nehemiah", "NEH" },
            { "Esther", "EST" },
            { "Job", "JOB" },
            { "Psalms", "PSA" },
            { "Proverbs", "PRO" },
            { "Ecclesiastes", "ECC" },
            { "Song of Songs", "SNG" },
            { "Isaiah", "ISA" },
            { "Jeremiah", "JER" },
            { "Epistle of Jeremiah", "LJE" },
            { "Lamentations", "LAM" },
            { "Ezekiel", "EZK" },
            { "Daniel", "DAN" },
            { "Hosea", "HOS" },
            { "Joel", "JOL" },
            { "Amos", "AMO" },
            { "Obadiah", "OBA" },
            { "Jonah", "JON" },
            { "Micah", "MIC" },
            { "Nahum", "NAM" },
            { "Habakkuk", "HAB" },
            { "Zephaniah", "ZEP" },
            { "Haggai", "HAG" },
            { "Zechariah", "ZEC" },
            { "Malachi", "MAL" },
            { "1 Esdras", "1ES" },
            { "2 Esdras", "2ES" },
            { "Tobit", "TOB" },
            { "Judith", "JDT" },
            { "Greek Esther", "ESG" },
            { "Wisdom", "WIS" },
            { "Sirach", "SIR" },
            { "Baruch", "BAR" },
            { "Prayer of Azariah", "S3Y" },
            { "Susanna", "SUS" },
            { "Bel and the Dragon", "BEL" },
            { "Prayer of Manasseh", "MAN" },
            { "1 Maccabees", "1MA" },
            { "2 Maccabees", "2MA" },
            { "3 Maccabees", "3MA" },
            { "4 Maccabees", "4MA" },
            { "Matthew", "MAT" },
            { "Mark", "MRK" },
            { "Luke", "LUK" },
            { "John", "JHN" },
            { "Acts", "ACT" },
            { "Romans", "ROM" },
            { "1 Corinthians", "1CO" },
            { "2 Corinthians", "2CO" },
            { "Galatians", "GAL" },
            { "Ephesians", "EPH" },
            { "Philippians", "PHP" },
            { "Colossians", "COL" },
            { "1 Thessalonians", "1TH" },
            { "2 Thessalonians", "2TH" },
            { "1 Timothy", "1TI" },
            { "2 Timothy", "2TI" },
            { "Titus", "TIT" },
            { "Philemon", "PHM" },
            { "Hebrews", "HEB" },
            { "James", "JAS" },
            { "1 Peter", "1PE" },
            { "2 Peter", "2PE" },
            { "1 John", "1JN" },
            { "2 John", "2JN" },
            { "3 John", "3JN" },
            { "Jude", "JUD" },
            { "Revelation", "REV" }
        };

        /// <summary>
        /// The book being referenced.
        /// </summary>
        public Book Book { get; set; }

        /// <summary>
        /// The chapter the reference begins in.
        /// </summary>
        public int StartingChapter { get; set; }

        /// <summary>
        /// The verse of the chapter the reference begins in.
        /// </summary>
        public int StartingVerse { get; set; }

        /// <summary>
        /// The chapter the reference ends in.
        /// </summary>
        public int EndingChapter { get; set; }

        /// <summary>
        /// The verses of the chapter the reference ends in.
        /// </summary>
        public int EndingVerse { get; set; }

        /// <summary>
        /// The chapters used in this reference.
        /// </summary>
        public List<Chapter> Chapters { get; set; }

        /// <summary>
        /// The appended verses in a reference.
        /// </summary>
        public List<Tuple<int, int>> AppendedVerses { get; set; } = [];

        /// <summary>
        /// The <see cref="Version"/> that the reference requests.
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// Indicates whether the reference is of the Old Testament.
        /// </summary>
        /// <remarks>
        /// If this is true, <see cref="IsNT"/> and <see cref="IsDEU"/> must be false.
        /// </remarks>
        public bool IsOT { get; set; }

        /// <summary>
        /// Indicates whether the reference is of the New Testament.
        /// </summary>
        /// <remarks>
        /// If this is true, <see cref="IsOT"/> and <see cref="IsDEU"/> must be false.
        /// </remarks>
        public bool IsNT { get; set; }

        /// <summary>
        /// Indicates whether the reference is of the Deuterocanon.
        /// </summary>
        /// <remarks>
        /// If this is true, <see cref="IsOT"/> and <see cref="IsNT"/> must be false.
        /// </remarks>
        public bool IsDEU { get; set; }

        /// <summary>
        /// Indicates whether the expando notation was used, like Psalm 1:1-
        /// </summary>
        public bool IsExpandoVerse { get; set; }

        /// <summary>
        /// The string representation of the Reference.
        /// </summary>
        /// <remarks>
        /// In most cases, this will equal the value of <see cref="ToString()"/>, set by the BibleProvider at the end of
        /// fetching a verse. In certain cases, it is used to handle references where we skip the parsing process and
        /// trust the origin to have their references proper.
        /// </remarks>
        public string AsString { get; set; }

        /// <summary>
        /// Returns a string that represents the Reference. Data names of books differ in
        /// API.Bible, thus the distinct function.
        /// </summary>
        /// <returns>A string that represents the Reference.</returns>
        public string ToString(bool isLogStatement)
        {
            StringBuilder sb = new();
            sb.Append(Book).Append(' ').Append(StartingChapter).Append(':').Append(StartingVerse);

            // Handle different cases more efficiently
            if (EndingChapter > 0 && EndingChapter != StartingChapter)
            {
                sb.Append('-').Append(EndingChapter).Append(':').Append(EndingVerse);
            }
            else if (EndingVerse > 0 && EndingVerse != StartingVerse)
            {
                if (isLogStatement && IsExpandoVerse && Version.Source == "ab")
                {
                    sb.Append('-');
                }
                else
                {
                    sb.Append('-').Append(EndingVerse);
                }
            }
            else if (EndingChapter > 0 && AppendedVerses?.Count == 0 && EndingVerse == 0)
            {
                sb.Append('-');
            }

            // Handle appended verses
            if (AppendedVerses?.Count > 0)
            {
                foreach (Tuple<int, int> verse in AppendedVerses)
                {
                    sb.Append(", ");
                    if (verse.Item1 == verse.Item2)
                    {
                        sb.Append(verse.Item1);
                    }
                    else
                    {
                        sb.Append(verse.Item1).Append('-').Append(verse.Item2);
                    }
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Returns a string that represents the Reference. Data names of books differ in
        /// API.Bible, thus the distinct function.
        /// </summary>
        /// <returns>A string that represents the Reference.</returns>
        public override string ToString() => ToString(false);

        /// <summary>
        /// Returns a API.Bible-friendly string that represents the Reference. Data names of books differ in
        /// API.Bible, thus the distinct function.
        /// </summary>
        /// <returns>An API.Bible-friendly string that represents the Reference.</returns>
        public string ToAPIBibleString()
        {
            var sb = new StringBuilder();
            sb.Append(_nameToId[Book.ProperName]).Append('.').Append(StartingChapter).Append('.').Append(StartingVerse);

            if (EndingChapter > 0 && EndingChapter != StartingChapter)
            {
                sb.Append('-').Append(_nameToId[Book.ProperName]).Append('.').Append(EndingChapter).Append('.').Append(EndingVerse);
            }
            else if (EndingVerse > 0 && EndingVerse != StartingVerse)
            {
                sb.Append('-').Append(_nameToId[Book.ProperName]).Append('.').Append(StartingChapter).Append('.').Append(EndingVerse);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Determines whether the specified object is equal to the Reference.
        /// </summary>
        /// <remarks>
        /// This is used for caching purposes.
        /// </remarks>
        /// <param name="obj"></param>
        /// <returns>true if the specified object is equal to the Reference; otherwise, false.</returns>
        public override bool Equals(object obj) => obj is Reference other &&
                   Book.ProperName == other.Book.ProperName &&
                   StartingChapter == other.StartingChapter &&
                   StartingVerse == other.StartingVerse &&
                   EndingChapter == other.EndingChapter &&
                   EndingVerse == other.EndingVerse &&
                   Version.Abbreviation == other.Version.Abbreviation;

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <remarks>
        /// This is used for caching purposes.
        /// </remarks>
        /// <returns>A hash code for the string representing the reference.</returns>
        public override int GetHashCode() => HashCode.Combine(Book.ProperName, StartingChapter, StartingVerse,
                           EndingChapter, EndingVerse, Version.Abbreviation);
    }
}

