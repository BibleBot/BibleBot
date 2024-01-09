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
    /// A convenient way to distinguish sections of the Bible
    /// without hardcoded strings.
    /// </summary>
    public enum BookCategories
    {
        /// <summary>
        /// An Old Testament book, more specifically the whole OT from a Protestant POV.
        /// </summary>
        OldTestament,

        /// <summary>
        /// A New Testament book.
        /// </summary>
        NewTestament,

        /// <summary>
        /// A Deuterocanonical book, found standard in the OT canon of Apostolic churches.
        /// </summary>
        Deuterocanon
    }
}