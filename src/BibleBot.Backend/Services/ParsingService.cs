/*
* Copyright (C) 2016-2024 Kerygma Digital Co.
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

namespace BibleBot.Backend.Services
{
    public partial class ParsingService
    {
        public System.Tuple<string, List<BookSearchResult>> GetBooksInString(Dictionary<string, List<string>> bookNames, List<string> defaultNames, string str)
        {
            List<BookSearchResult> results = [];

            List<string> overlaps =
            [
                "ezra",
                "jer",
                "esth"
            ];

            // We want to iterate twice through the booknames
            // in order to skip Ezra in the first iteration,
            // otherwise 1/2/3 Esdras will collide with Ezra.
            // Ditto for Jeremiah + Letter/Epistle of Jeremiah.
            for (int i = 1; i < 3; i++)
            {
                foreach (KeyValuePair<string, List<string>> bookName in bookNames)
                {
                    foreach (string item in bookName.Value)
                    {
                        if (IsValueInString(str, item.ToLowerInvariant()))
                        {
                            if (!(i == 1 && overlaps.Contains(bookName.Key)))
                            {
                                str = str.Replace(item.ToLowerInvariant(), bookName.Key);
                            }
                        }
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

        [GeneratedRegex(@"[:：]")]
        private static partial Regex ContainsColonsRegex();

        [GeneratedRegex(@"-")]
        private static partial Regex ContainsSpanRegex();

        public Reference GenerateReference(string str, BookSearchResult bookSearchResult, Version prefVersion, List<Version> versions)
        {
            string book = bookSearchResult.Name;
            int startingChapter = 0;
            int startingVerse = 0;
            int endingChapter = 0;
            int endingVerse = 0;
            bool expandoVerseUsed = false;

            Dictionary<string, Dictionary<string, string>> _bookMap = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(File.ReadAllText("./Data/book_map.json"));

            string[] tokens = str.Split(" ");

            if (bookSearchResult.Index + 2 <= tokens.Length)
            {
                string relevantToken = tokens.Skip(bookSearchResult.Index + 1).First();
                MatchCollection colonMatches = ContainsColonsRegex().Matches(relevantToken);

                if (colonMatches.Count != 0)
                {
                    int tokenIdxAfterSpan = bookSearchResult.Index + 2;

                    if (tokens.Length > tokenIdxAfterSpan)
                    {
                        string lastToken = tokens[tokenIdxAfterSpan].ToUpperInvariant();
                        Version potentialVersion = versions.SingleOrDefault(version => string.Equals(version.Abbreviation, lastToken, System.StringComparison.OrdinalIgnoreCase));

                        if (potentialVersion != null)
                        {
                            prefVersion = potentialVersion;
                        }
                    }


                    if (colonMatches.Count == 2)
                    {
                        string[] span = relevantToken.Split("-");

                        foreach (string pairString in span)
                        {
                            string[] pairStringSplit = pairString.Split(colonMatches.First().Value);

                            foreach (string pairValue in pairStringSplit)
                            {
                                pairStringSplit[System.Array.IndexOf(pairStringSplit, pairValue)] = RemovePunctuation(pairValue);
                            }

                            try
                            {
                                int firstNum = int.Parse(pairStringSplit[0]);
                                int secondNum = int.Parse(pairStringSplit[1]);

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
                            catch
                            {
                                return null;
                            }
                        }
                    }
                    else if (colonMatches.Count == 1)
                    {
                        string[] pair = relevantToken.Split(colonMatches.First().Value);

                        try
                        {
                            int num = int.Parse(pair[0]);

                            startingChapter = num;
                            endingChapter = num;
                        }
                        catch
                        {
                            return null;
                        }

                        int spanQuantity = ContainsSpanRegex().Matches(relevantToken).Count;

                        string[] spanSplit = pair[1].Split("-");
                        foreach (string pairValue in spanSplit)
                        {
                            string pairValueCopy = RemovePunctuation(pairValue);

                            try
                            {
                                int num = int.Parse(pairValueCopy);

                                switch (System.Array.IndexOf(spanSplit, pairValue))
                                {
                                    case 0:
                                        startingVerse = num;
                                        break;
                                    case 1:
                                        endingVerse = num;
                                        break;
                                    default:
                                        return null;
                                }
                            }
                            catch
                            {
                                // We know that BibleGateway will extend to the end of a chapter with this 
                                // "Genesis 1:1-" syntax, but for other sources this is likely not available.
                                if (prefVersion.Source == "bg")
                                {
                                    // Instead of returning null here, we'll break out of the loop
                                    // in the event that the span exists to extend to the end of a chapter.
                                    expandoVerseUsed = true;
                                    break;
                                }

                                return null;
                            }
                        }

                        // We set a toggle if we think expando verses are being used, otherwise
                        // references like "Genesis 1:1-1" act like expando verses.
                        if (endingVerse == 0 && (spanQuantity == 0 || !expandoVerseUsed))
                        {
                            endingVerse = startingVerse;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
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

            if (_bookMap["ot"].ContainsKey(book))
            {
                isOT = true;
                book = _bookMap["ot"][book];
            }
            else if (_bookMap["nt"].ContainsKey(book))
            {
                isNT = true;
                book = _bookMap["nt"][book];
            }
            else if (_bookMap["deu"].ContainsKey(book))
            {
                isDEU = true;
                book = _bookMap["deu"][book];
            }

            if (book == "Psalm" && startingChapter == 151)
            {
                isOT = false;
                isDEU = true;
                book = _bookMap["deu"]["ps151"];
                startingChapter = 1;
                endingChapter -= 150;
            }

            return new Reference
            {
                Book = book,
                StartingChapter = startingChapter,
                StartingVerse = startingVerse,
                EndingChapter = endingChapter,
                EndingVerse = endingVerse,
                Version = prefVersion,

                IsOT = isOT,
                IsNT = isNT,
                IsDEU = isDEU
            };
        }

        public string PurifyBody(List<string> ignoringBrackets, string str)
        {
            str = str.ToLowerInvariant().Replace("\r", " ").Replace("\n", " ");

            foreach (string brackets in ignoringBrackets)
            {
                str = new Regex(@"\" + brackets[0] + @"[^\" + brackets[1] + @"]*\" + brackets[1]).Replace(str, "");
            }

            string punctuationToIgnore = "!\"#$%&'()*+,./;<=>?@[\\]^_`{|}~";
            foreach (char character in punctuationToIgnore)
            {
                str = str.Replace(character, ' ');
            }

            return str;
        }

        private static bool IsValueInString(string str, string val) => $" {str} ".Contains($" {val} ");

        [GeneratedRegex(@"[^\w\s]|_")]
        private static partial Regex NoPunctuationRegex();

        [GeneratedRegex(@"\s+")]
        private static partial Regex MinimizeWhitespaceRegex();

        private static string RemovePunctuation(string str) => MinimizeWhitespaceRegex().Replace(NoPunctuationRegex().Replace(str, ""), " ");
    }
}
