/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using BibleBot.Models;
using MDBookMap = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, string>>;
using MDBookNames = System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>>;

namespace BibleBot.Backend.Services
{
    public partial class ParsingService
    {
        private readonly MDBookMap _bookMap = JsonSerializer.Deserialize<MDBookMap>(File.ReadAllText("./Data/book_map.json"));
        private readonly List<string> _overlappingBookNames =
        [
            "EZR", // Ezra
            "JER", // Jeremiah
            "EST", // Esther
            "JHN", // John
            "SNG" // Song of Songs (conflicts with Song of the Three Holy Youths)
        ];

        public System.Tuple<string, List<BookSearchResult>> GetBooksInString(MDBookNames bookNames, List<string> defaultNames, string str)
        {
            List<BookSearchResult> results = [];

            // We want to iterate twice through the booknames
            // in order to skip Ezra in the first iteration,
            // otherwise 1/2/3 Esdras will collide with Ezra.
            for (int i = 1; i < 3; i++)
            {
                foreach (KeyValuePair<string, List<string>> bookName in bookNames)
                {
                    foreach (string item in bookName.Value.Where(item => IsValueInString(str, item.ToLowerInvariant())).Where(_ => !(i == 1 && _overlappingBookNames.Contains(bookName.Key))))
                    {
                        str = str.Replace(item.ToLowerInvariant(), bookName.Key);
                    }
                }
            }

            string[] tokens = str.Split(" ");
            foreach (string bookName in defaultNames)
            {
                for (int i = 0; i < tokens.Length; i++)
                {
                    if (tokens[i] == bookName)
                    {
                        results.Add(new BookSearchResult { Name = bookName, Index = i });
                    }
                }
            }

            // Sort results by input.
            results.Sort((x, y) => x.Index.CompareTo(y.Index));
            return new System.Tuple<string, List<BookSearchResult>>(str, results);
        }

        [GeneratedRegex(@"[A-Z]{1,8}[\-]{0,1}[A-Z]{0,4}[0-9]{0,5}", RegexOptions.Compiled)]
        private static partial Regex VersionAcronymRegex();

        public Reference GenerateReference(string str, BookSearchResult bookSearchResult, Version prefVersion, List<Version> versions)
        {
            string bookName = bookSearchResult.Name;
            int startingChapter = 0, startingVerse = 0, endingChapter = 0, endingVerse = 0;
            List<System.Tuple<int, int>> appendedVerses = [];
            bool expandoVerseUsed = false;

            string[] tokens = str.Split(' ');

            int relevantIndex = bookSearchResult.Index + 1;
            if (relevantIndex >= tokens.Length)
            {
                return null;
            }

            string relevantToken = tokens[relevantIndex];
            char[] colonChars = [':', '：'];

            List<int> colonIndices = [];
            for (int i = 0; i < relevantToken.Length; i++)
            {
                if (relevantToken[i] is ':' or '：')
                {
                    colonIndices.Add(i);
                }
            }

            if (colonIndices.Count == 0)
            {
                return null;
            }

            if (relevantIndex + 1 <= tokens.Length)
            {
                int tokenIdxAfterSpan = relevantIndex + 1;

                if (tokens.Length > tokenIdxAfterSpan)
                {
                    string lastToken = tokens[tokenIdxAfterSpan].ToUpperInvariant();
                    Match versionAcronymRegexMatch = VersionAcronymRegex().Match(lastToken);

                    while (!versionAcronymRegexMatch.Success && tokenIdxAfterSpan < tokens.Length - 1)
                    {
                        tokenIdxAfterSpan += 1;
                        lastToken = tokens[tokenIdxAfterSpan].ToUpperInvariant();
                        versionAcronymRegexMatch = VersionAcronymRegex().Match(lastToken);
                    }

                    Version potentialVersion = versions.FirstOrDefault(version => string.Equals(version.Abbreviation, versionAcronymRegexMatch.Value, System.StringComparison.OrdinalIgnoreCase));

                    if (potentialVersion != null)
                    {
                        prefVersion = potentialVersion;
                    }
                }

                switch (colonIndices.Count)
                {
                    case 2:
                        {
                            string[] span = relevantToken.Split('-');

                            foreach (string pairString in span)
                            {
                                int colonIdx = pairString.IndexOfAny(colonChars);
                                if (colonIdx == -1)
                                {
                                    return null;
                                }

                                string left = pairString.Substring(0, colonIdx);
                                string right = pairString.Substring(colonIdx + 1);

                                left = RemovePunctuation(left);
                                right = RemovePunctuation(right);

                                if (!int.TryParse(left, out int firstNum) || !int.TryParse(right, out int secondNum))
                                {
                                    return null;
                                }

                                if (startingChapter == 0)
                                {
                                    startingChapter = firstNum;
                                    startingVerse = secondNum;
                                }
                                else
                                {
                                    endingChapter = firstNum;
                                    endingVerse = secondNum;
                                }
                            }
                            break;
                        }
                    case 1:
                        {
                            int colonIdx = relevantToken.IndexOfAny(colonChars);
                            if (colonIdx == -1)
                            {
                                return null;
                            }

                            string left = relevantToken.Substring(0, colonIdx);
                            string right = relevantToken.Substring(colonIdx + 1);

                            left = RemovePunctuation(left);
                            if (!int.TryParse(left, out int chapterNum))
                            {
                                return null;
                            }

                            startingChapter = chapterNum;
                            endingChapter = chapterNum;

                            int spanQuantity = right.Count(c => c == '-');

                            string[] spanSplit = right.Split('-');
                            foreach (string pairValue in spanSplit)
                            {
                                string pairValueCopy = RemovePunctuation(pairValue);

                                try
                                {
                                    int verseNum = int.Parse(pairValueCopy);

                                    switch (System.Array.IndexOf(spanSplit, pairValue))
                                    {
                                        case 0:
                                            startingVerse = verseNum;
                                            break;
                                        case 1:
                                            endingVerse = verseNum;
                                            break;
                                        default:
                                            return null;
                                    }
                                }
                                catch
                                {
                                    if (pairValueCopy.Contains(','))
                                    {
                                        string[] commaSplit = pairValueCopy.Split(",");

                                        if (commaSplit[0] == "")
                                        {
                                            return null;
                                        }

                                        int tokenIndexPtr = 2;

                                        while (commaSplit[^1] == "")
                                        {
                                            int nextTokenIdx = bookSearchResult.Index + tokenIndexPtr;

                                            if (nextTokenIdx >= tokens.Length)
                                            {
                                                break;
                                            }

                                            pairValueCopy += tokens[bookSearchResult.Index + tokenIndexPtr];
                                            commaSplit = pairValueCopy.Split(',');

                                            tokenIndexPtr++;
                                        }

                                        foreach (string commaValue in commaSplit)
                                        {
                                            if (commaValue.Contains('-'))
                                            {
                                                string[] commaSpanSplit = commaValue.Split("-");
                                                List<int> pairArray = [];

                                                foreach (string commaSpanPairValue in commaSpanSplit)
                                                {
                                                    if (!int.TryParse(commaSpanPairValue, out int commaSpanPairNum))
                                                    {
                                                        return null;
                                                    }
                                                    pairArray.Add(commaSpanPairNum);
                                                }

                                                appendedVerses.Add(new System.Tuple<int, int>(pairArray[0], pairArray[1]));
                                            }
                                            else
                                            {
                                                if (!int.TryParse(commaValue, out int singleNum))
                                                {
                                                    return null;
                                                }
                                                appendedVerses.Add(new System.Tuple<int, int>(singleNum, singleNum));
                                            }
                                        }

                                        // Early exit for too many commas to avoid unnecessary processing
                                        if (commaSplit.Length > 5)
                                        {
                                            throw new VerseLimitationException("too many commas");
                                        }

                                        if (appendedVerses.Count > 0)
                                        {
                                            switch (System.Array.IndexOf(spanSplit, pairValue))
                                            {
                                                case 0:
                                                    startingVerse = appendedVerses[0].Item1;
                                                    appendedVerses = [.. appendedVerses.TakeLast(appendedVerses.Count - 1)];
                                                    break;
                                                case 1:
                                                    endingVerse = appendedVerses[0].Item1;
                                                    appendedVerses = [.. appendedVerses.Skip(1).TakeLast(appendedVerses.Count - 1)];
                                                    break;
                                                default:
                                                    return null;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // Instead of returning null here, we'll break out of the loop
                                        // in the event that the span exists to extend to the end of a chapter.
                                        expandoVerseUsed = true;

                                        // These providers don't treat the unfinished span like BibleGateway, thus a workaround.
                                        if (prefVersion.Source is "ab" or "nlt")
                                        {
                                            // Largest verse count in any chapter is 176. No harm in rounding up.
                                            endingVerse = 200;
                                        }

                                        break;
                                    }
                                }
                            }

                            // We set a toggle if we think expando verses are being used, otherwise
                            // references like "Genesis 1:1-1" act like expando verses.
                            if (appendedVerses.Count == 0 && endingVerse == 0 && (spanQuantity == 0 || !expandoVerseUsed))
                            {
                                endingVerse = startingVerse;
                            }
                            break;
                        }
                    default:
                        return null;
                }
            }
            else
            {
                return null;
            }

            // Without this, the parsing thinks of any verse like "Genesis 1: 1-5" as "Genesis 1:0-".
            // We wouldn't want users trying to start verse referencing with a 0-based index anyway.
            if (startingVerse == 0)
            {
                return null;
            }

            bool isOT = false;
            bool isNT = false;
            bool isDEU = false;

            // Cache dictionary lookups to avoid repeated key access
            Dictionary<string, string> otMap = _bookMap["ot"];
            Dictionary<string, string> ntMap = _bookMap["nt"];
            Dictionary<string, string> deuMap = _bookMap["deu"];

            if (otMap.TryGetValue(bookName, out string otName))
            {
                isOT = true;
                bookName = otName;
            }
            else if (ntMap.TryGetValue(bookName, out string ntName))
            {
                isNT = true;
                bookName = ntName;
            }
            else if (deuMap.TryGetValue(bookName, out string deuName))
            {
                isDEU = true;
                bookName = deuName;
            }

            // Use Ordinal comparison for exact string matches (faster than default)
            if (string.Equals(prefVersion.Abbreviation, "NRSV", System.StringComparison.Ordinal))
            {
                prefVersion = versions.FirstOrDefault(version => string.Equals(version.Abbreviation, "NRSVA", System.StringComparison.OrdinalIgnoreCase));
            }

            Book book = prefVersion.Books?.FirstOrDefault(book => book.ProperName == bookName, new Book { Name = bookSearchResult.Name, ProperName = bookName });

            if (bookName == "Psalm" && startingChapter == 151)
            {
                isOT = false;
                isDEU = true;

                if (prefVersion.Source == "bg")
                {
                    book.ProperName = "Psalm 151";
                    startingChapter = 1;
                    endingChapter -= 150;
                }
            }

            return new Reference
            {
                Book = book,
                StartingChapter = startingChapter,
                StartingVerse = startingVerse,
                EndingChapter = endingChapter,
                EndingVerse = endingVerse,
                AppendedVerses = appendedVerses,
                Version = prefVersion,
                IsExpandoVerse = expandoVerseUsed,

                IsOT = isOT,
                IsNT = isNT,
                IsDEU = isDEU
            };
        }

        public static string PurifyBody(List<string> ignoringBrackets, string str)
        {
            str = str.ToLowerInvariant().Replace("\r", " ").Replace("\n", " ");
            str = ignoringBrackets.Aggregate(str, (current, brackets) => new Regex(@"\" + brackets[0] + @"[^\" + brackets[1] + @"]*\" + brackets[1]).Replace(current, ""));
            str = VariantDashesRegex().Replace(str, "-");

            const string punctuationToIgnore = "!\"#$%&'()*+./;<=>?@[\\]^_`{|}~";
            return punctuationToIgnore.Aggregate(str, (current, character) => current.Replace(character, ' '));
        }

        private static bool IsValueInString(string str, string val) => $" {str} ".Contains($" {val} ");

        [GeneratedRegex(@"[\u2013\u2014\u2012\uFF0D]")]
        private static partial Regex VariantDashesRegex();

        [GeneratedRegex(@"[^,\w\s]|_")]
        private static partial Regex NoPunctuationRegex();

        [GeneratedRegex(@"\s+")]
        private static partial Regex MinimizeWhitespaceRegex();

        private static string RemovePunctuation(string str) => MinimizeWhitespaceRegex().Replace(NoPunctuationRegex().Replace(str, ""), " ");
    }
}
