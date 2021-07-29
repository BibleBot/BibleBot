/*
* Copyright (C) 2016-2021 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

namespace BibleBot.Lib
{
    public class Verse
    {
        public Reference Reference { get; set; }
        public string Title { get; set; }
        public string PsalmTitle { get; set; }
        public string Text { get; set; }
    }
}
