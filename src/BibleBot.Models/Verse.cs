/*
* Copyright (C) 2016-2024 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

namespace BibleBot.Models
{
    /// <summary>
    /// The model for verse results.
    /// </summary>
    public class Verse
    {
        /// <summary>
        /// The <see cref="Reference"/> that this Verse was made from.
        /// </summary>
        public Reference Reference { get; set; }

        /// <summary>
        /// The title belonging to the section of the verse.
        /// </summary>
        /// <remarks>
        /// This is not available in all versions.
        /// </remarks>
        public string Title { get; set; }

        /// <summary>
        /// The title for a Psalm, if applicable.
        /// </summary>
        /// <remarks>
        /// This is currently unused by frontend, as there isn't a clean way to display it.
        /// </remarks>
        public string PsalmTitle { get; set; }

        /// <summary>
        /// The content of the verse.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Determines whether the specified object is equal to the Verse.
        /// </summary>
        /// <remarks>
        /// This is used for caching purposes.
        /// </remarks>
        /// <param name="obj"></param>
        /// <returns>true if the specified object is equal to the Verse; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (obj is null or not Verse)
            {
                return false;
            }

            Verse verse = obj as Verse;

            return Reference.Equals(verse.Reference) && Title.Equals(verse.Title) &&
                   PsalmTitle.Equals(verse.PsalmTitle) && Text.Equals(verse.Text);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <remarks>
        /// This is used for caching purposes.
        /// </remarks>
        /// <returns>A hash code created by XOR'ing the properties' hash codes.</returns>
        public override int GetHashCode() => Reference.GetHashCode() ^ Title.GetHashCode() ^ PsalmTitle.GetHashCode() ^ Text.GetHashCode();
    }
}
