/*
* Copyright (C) 2016-2023 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;

namespace BibleBot.Models
{
    public class Reference
    {
        public string Book { get; set; }

        public int StartingChapter { get; set; }
        public int StartingVerse { get; set; }
        public int EndingChapter { get; set; }
        public int EndingVerse { get; set; }

        public Version Version { get; set; }

        public bool IsOT { get; set; }
        public bool IsNT { get; set; }
        public bool IsDEU { get; set; }

        public string AsString { get; set; }

        public override string ToString()
        {
            string result = $"{this.Book} {this.StartingChapter}:{this.StartingVerse}";

            if (this.EndingChapter > 0 && this.EndingChapter != this.StartingChapter)
            {
                result += $"-{this.EndingChapter}:{this.EndingVerse}";
            }
            else if (this.EndingVerse > 0 && this.EndingVerse != this.StartingVerse)
            {
                result += $"-{this.EndingVerse}";
            }
            else if (this.EndingChapter > 0 && this.EndingVerse == 0)
            {
                result += "-";
            }

            return result;
        }

        public string ToAPIBibleString()
        {
            var nameToId = new Dictionary<string, string>
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
                { "Habukkuk", "HAB" },
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

            string result = $"{nameToId[this.Book]}.{this.StartingChapter}.{this.StartingVerse}";

            if (this.EndingChapter > 0 && this.EndingChapter != this.StartingChapter)
            {
                result += $"-{nameToId[this.Book]}.{this.EndingChapter}.{this.EndingVerse}";
            }
            else if (this.EndingVerse > 0 && this.EndingVerse != this.StartingVerse)
            {
                result += $"-{nameToId[this.Book]}.{this.StartingChapter}.{this.EndingVerse}";
            }

            return result;
        }

        // NOTE: Equals() and GetHashCode() presume version DOES NOT MATTER

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Reference))
            {
                return false;
            }



            return this.ToString() == ((obj as Reference).ToString()) &&
                   this.Version.Abbreviation == (obj as Reference).Version.Abbreviation;
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }
    }
}
