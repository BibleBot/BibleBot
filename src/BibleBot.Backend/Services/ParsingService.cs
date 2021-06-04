using System.Linq;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using BibleBot.Lib;
using BibleBot.Backend.Models;

namespace BibleBot.Backend.Services
{
    public class ParsingService
    {
        private readonly VersionService _versionService;
        
        public ParsingService(VersionService versionService)
        {
            _versionService = versionService;
        }

        public System.Tuple<string, List<BookSearchResult>> GetBooksInString(Dictionary<string, List<string>> bookNames, List<string> defaultNames, string str)
        {
            List<BookSearchResult> results = new List<BookSearchResult>();

            foreach (var bookName in bookNames)
            {
                foreach (string item in bookName.Value)
                {
                    if (IsValueInString(str, item.ToLowerInvariant()))
                    {
                        str = str.Replace(item.ToLowerInvariant(), bookName.Key);
                    }
                }
            }

            var tokens = str.Split(" ");
            foreach (var bookName in defaultNames)
            {
                foreach (var token in tokens)
                {
                    if (token == bookName)
                    {
                        results.Add(new BookSearchResult{ Name = bookName, Index = System.Array.IndexOf(tokens, token) });
                    }
                }
            }

            // Sort results by input.
            results.Sort((x, y) => x.Index.CompareTo(y.Index));
            return new System.Tuple<string, List<BookSearchResult>>(str, results);
        }

        public Reference GenerateReference(string str, BookSearchResult bookSearchResult, Version version)
        {
            string book = bookSearchResult.Name;
            int startingChapter = 0;
            int startingVerse = 0;
            int endingChapter = 0;
            int endingVerse = 0;
            int tokenIdxAfterSpan = 0;

            var tokens = str.Split(" ");

            if (bookSearchResult.Index + 2 <= tokens.Length)
            {
                var relevantToken = tokens.Skip(bookSearchResult.Index + 1).First();

                if (relevantToken.Contains(":"))
                {
                    tokenIdxAfterSpan = bookSearchResult.Index + 2;

                    var colonRegex = new Regex(@":", RegexOptions.Compiled);
                    var colonQuantity = colonRegex.Matches(relevantToken).Count();

                    if (colonQuantity == 2)
                    {
                        var span = relevantToken.Split("-");

                        foreach (var pairString in span)
                        {
                            var pairStringSplit = pairString.Split(":");

                            foreach (var pairValue in pairStringSplit)
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
                    else if (colonQuantity == 1)
                    {
                        var pair = relevantToken.Split(":");

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

                        var spanRegex = new Regex(@"-", RegexOptions.Compiled);
                        var spanQuantity = spanRegex.Matches(relevantToken).Count();

                        var spanSplit = pair[1].Split("-");
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
                                // We know that BibleGateway will extend to the end of a chapter with this syntax,
                                // but for other sources this is likely not available.
                                if (version.Source == "bg")
                                {
                                    // Instead of returning nil, we'll break out of the loop
                                    // in the event that the span exists to extend to the end of a chapter.
                                    break;
                                }
                            }
                        }

                        if (endingVerse == 0 && spanQuantity == 0)
                        {
                            endingVerse = startingVerse;
                        }
                    }
                    else
                    {
                        return null;
                    }

                    if (tokens.Length > tokenIdxAfterSpan)
                    {
                        string lastToken = tokens[tokenIdxAfterSpan].ToUpper();

                        var idealVersion = _versionService.Get(lastToken);

                        if (idealVersion != null)
                        {
                            if (idealVersion.Abbreviation == lastToken)
                            {
                                version = idealVersion;
                            }
                        }
                    }
                }
            }
            else
            {
                return null;
            }

            bool isOT = false;
            bool isNT = false;
            bool isDEU = false;

            string bookMapString = File.ReadAllText("./Data/book_map.json");
            var bookMap = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(bookMapString);

            if (bookMap["ot"].ContainsKey(book))
            {
                isOT = true;
                book = bookMap["ot"][book];
            }
            else if (bookMap["nt"].ContainsKey(book))
            {
                isNT = true;
                book = bookMap["nt"][book];
            } 
            else if (bookMap["deu"].ContainsKey(book))
            {
                isDEU = true;
                book = bookMap["deu"][book];
            }

            return new Reference
            {
                Book = book,
                StartingChapter = startingChapter,
                StartingVerse = startingVerse,
                EndingChapter = endingChapter,
                EndingVerse = endingVerse,
                Version = version,

                IsOT = isOT,
                IsNT = isNT,
                IsDEU = isDEU
            };
        }

        public bool IsSurroundedByBrackets(string brackets, BookSearchResult result, string str)
        {
            var tokens = str.Split(" ");
            var pureIndexOfResult = str.IndexOf(tokens[result.Index]);

            var bracketCheckRegex = new Regex(@"\" + brackets[0] + @"(.*?)\" + brackets[1]);

            foreach (Match match in bracketCheckRegex.Matches(str))
            {
                var matchStr = match.Value.Trim();

                if (!matchStr.Contains(brackets[0]) || !matchStr.Contains(brackets[1]))
                {
                    return str.IndexOf(matchStr) == pureIndexOfResult;
                }
            }

            return false;
        }

        private bool IsValueInString(string str, string val)
        {
            return $" {str} ".Contains($" {val} ");
        }

        private string RemovePunctuation(string str)
        {
            var noPunctuationRegex = new Regex(@"[^\w\s]|_", RegexOptions.Compiled);
            var minimizeWhitespaceRegex = new Regex(@"\s+", RegexOptions.Compiled);

            return minimizeWhitespaceRegex.Replace(noPunctuationRegex.Replace(str, ""), " ");
        }
    }
}