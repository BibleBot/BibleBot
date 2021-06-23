/*
* Copyright (C) 2016-2021 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;

namespace BibleBot.Lib
{
    public class VerseResponse : IResponse
    {
        public bool OK { get; set; }
        public string LogStatement { get; set; }
        public List<Verse> Verses { get; set; }
        public string DisplayStyle { get; set; }
    }
}
