/*
* Copyright (C) 2016-2023 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

namespace BibleBot.Models
{
    public class Verse
    {
        public Reference Reference { get; set; }
        public string Title { get; set; }
        public string PsalmTitle { get; set; }
        public string Text { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Verse))
            {
                return false;
            }

            Verse verse = (Verse)obj;

            return (Reference.Equals(verse.Reference)) && (Title.Equals(verse.Title)) &&
                   (PsalmTitle.Equals(verse.PsalmTitle)) && (Text.Equals(verse.Text));
        }

        public override int GetHashCode()
        {
            return Reference.GetHashCode() ^ Title.GetHashCode() ^ PsalmTitle.GetHashCode() ^ Text.GetHashCode();
        }
    }
}
