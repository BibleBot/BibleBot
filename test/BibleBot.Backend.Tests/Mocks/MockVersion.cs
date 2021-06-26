/*
* Copyright (C) 2016-2021 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using BibleBot.Lib;

namespace BibleBot.Backend.Tests.Mocks
{
    public class MockVersion : Version
    {
        public MockVersion()
        {
            this.Name = "Revised Standard Version (RSV)";
            this.Abbreviation = "RSV";
            this.Source = "bg";
            this.SupportsOldTestament = true;
            this.SupportsNewTestament = true;
            this.SupportsDeuterocanon = true;
        }
    }
}