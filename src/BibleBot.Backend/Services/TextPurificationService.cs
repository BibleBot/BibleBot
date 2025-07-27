using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BibleBot.Backend.Services
{
    public static partial class TextPurificationService
    {
        private static readonly Dictionary<string, string> _nuisances = new()
        {
            { "“",     "\"" },
            { "”",     "\"" },
            { "\n",    " " },
            { "\t",    " " },
            { "\v",    " " },
            { "\f",    " " },
            { "\r",    " " },
            { "¶ ",    "" },
            { " , ",   ", " },
            { " .",    "." },
            { "′",     "'" },
            { "‘",     "'" },
            { "’",     "'" }, // Fonts may make it look like this is no different from the line above, but it's a different codepoint in Unicode.
            { "' s",   "'s" },
            { "' \"",  "'\""},
            { " . ",   " " },
            { "*",     "\\*" },
            { "_",     "\\_" },
            { "\\*\\*", "**" },
            { "\\_\\_", "__" },
            { "\\*(Selah)\\*", "*(Selah)*"}
        };

        [GeneratedRegex(@"\s+")]
        private static partial Regex MultipleWhitespacesGeneratedRegex();

        [GeneratedRegex(@"\s+(['""])(\w)", RegexOptions.Compiled)]
        private static partial Regex SpaceBeforeClosingQuoteRegex();

        [GeneratedRegex(@"Selah\.?", RegexOptions.Compiled)]
        private static partial Regex SelahRegex();

        [GeneratedRegex(@"[\u05D0-\u05D9]", RegexOptions.Compiled)]
        private static partial Regex HebrewCharsRegex();

        [GeneratedRegex(@"[\u2013\u2014\u2012\uFF0D]")]
        private static partial Regex VariantDashesRegex();

        [GeneratedRegex(@"[^,\w\s]|_")]
        private static partial Regex NoPunctuationRegex();

        [GeneratedRegex(@"\s+")]
        private static partial Regex MinimizeWhitespaceRegex();

        /// <summary>
        /// Purifies text by removing unwanted characters, fixing spacing, and handling special cases.
        /// </summary>
        /// <param name="text">The text to purify</param>
        /// <param name="isISV">Whether this is ISV text that needs Hebrew character removal</param>
        /// <returns>The purified text</returns>
        public static string PurifyText(string text, bool isISV = false)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            // Handle Selah with regex (combines both cases)
            text = SelahRegex().Replace(text, " *(Selah)* ");

            // Apply all nuisance replacements
            foreach ((string key, string value) in _nuisances)
            {
                text = text.Replace(key, value);
            }

            // Handle ISV-specific Hebrew character removal with regex
            if (isISV)
            {
                text = HebrewCharsRegex().Replace(text, "");
            }

            // Apply regex replacements
            text = MultipleWhitespacesGeneratedRegex().Replace(text, " ");
            text = SpaceBeforeClosingQuoteRegex().Replace(text, @"$1 $2");

            return text.Trim();
        }

        /// <summary>
        /// Purifies text with custom nuisance replacements in addition to the standard ones.
        /// </summary>
        /// <param name="text">The text to purify</param>
        /// <param name="customNuisances">Additional nuisance replacements specific to this provider</param>
        /// <param name="isISV">Whether this is ISV text that needs Hebrew character removal</param>
        /// <returns>The purified text</returns>
        public static string PurifyText(string text, Dictionary<string, string> customNuisances, bool isISV = false)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            // Handle Selah with regex (combines both cases)
            text = SelahRegex().Replace(text, " *(Selah)* ");

            // Apply standard nuisance replacements
            foreach ((string key, string value) in _nuisances)
            {
                text = text.Replace(key, value);
            }

            // Apply custom nuisance replacements
            if (customNuisances != null)
            {
                foreach ((string key, string value) in customNuisances)
                {
                    text = text.Replace(key, value);
                }
            }

            // Handle ISV-specific Hebrew character removal with regex
            if (isISV)
            {
                text = HebrewCharsRegex().Replace(text, "");
            }

            // Apply regex replacements
            text = MultipleWhitespacesGeneratedRegex().Replace(text, " ");
            text = SpaceBeforeClosingQuoteRegex().Replace(text, @"$1 $2");

            return text.Trim();
        }

        /// <summary>
        /// Purifies text for parsing by removing brackets, normalizing dashes, and removing punctuation.
        /// </summary>
        /// <param name="ignoringBrackets">List of bracket pairs to remove</param>
        /// <param name="str">The text to purify</param>
        /// <returns>The purified text for parsing</returns>
        public static string PurifyBody(List<string> ignoringBrackets, string str)
        {
            // Use StringBuilder for multiple string operations
            var sb = new System.Text.StringBuilder(str.ToLowerInvariant());
            sb.Replace("\r", " ").Replace("\n", " ");

            // Process brackets
            foreach (string brackets in ignoringBrackets)
            {
                string pattern = @"\" + brackets[0] + @"[^\" + brackets[1] + @"]*\" + brackets[1];
                string bracketResult = Regex.Replace(sb.ToString(), pattern, "");
                sb.Clear().Append(bracketResult);
            }

            // Replace variant dashes
            string result = VariantDashesRegex().Replace(sb.ToString(), "-");

            // Replace punctuation more efficiently
            const string punctuationToIgnore = "!\"#$%&'()*+./;<=>?@[\\]^_`{|}~";
            foreach (char character in punctuationToIgnore)
            {
                result = result.Replace(character, ' ');
            }

            return result;
        }

        /// <summary>
        /// Checks if a value exists in a string with proper word boundaries.
        /// </summary>
        /// <param name="str">The string to search in</param>
        /// <param name="val">The value to search for</param>
        /// <returns>True if the value is found with word boundaries</returns>
        public static bool IsValueInString(string str, string val)
        {
            // Avoid string allocations by using IndexOf with proper bounds checking
            int index = str.IndexOf(val, System.StringComparison.OrdinalIgnoreCase);
            if (index == -1)
            {
                return false;
            }

            // Check if it's a word boundary (space or start/end of string)
            bool startBoundary = index == 0 || char.IsWhiteSpace(str[index - 1]);
            bool endBoundary = index + val.Length == str.Length || char.IsWhiteSpace(str[index + val.Length]);

            return startBoundary && endBoundary;
        }

        /// <summary>
        /// Removes punctuation from a string and normalizes whitespace.
        /// </summary>
        /// <param name="str">The string to process</param>
        /// <returns>The string with punctuation removed and whitespace normalized</returns>
        public static string RemovePunctuation(string str) => MinimizeWhitespaceRegex().Replace(NoPunctuationRegex().Replace(str, ""), " ");
    }
}