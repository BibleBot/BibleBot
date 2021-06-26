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
    public class MockNTE : Version
    {
        public MockNTE()
        {
            this.Name = "New Testament for Everyone (NTE)";
            this.Abbreviation = "NTE";
            this.Source = "bg";
            this.SupportsOldTestament = false;
            this.SupportsNewTestament = true;
            this.SupportsDeuterocanon = false;
        }
    }

    public class MockKJVA : Version
    {
        public MockKJVA()
        {
            this.Name = "King James Version with Apocrypha (KJVA)";
            this.Abbreviation = "KJVA";
            this.Source = "ab";
            this.SupportsOldTestament = true;
            this.SupportsNewTestament = true;
            this.SupportsDeuterocanon = true;
        }
    }

}