/*
* Copyright (C) 2016-2023 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using BibleBot.Backend.Models;

namespace BibleBot.Backend.Tests.Mocks
{
    public class MockRSV : Version
    {
        public MockRSV()
        {
            this.Name = "Revised Standard Version (RSV)";
            this.Abbreviation = "RSV";
            this.Source = "bg";
            this.SupportsOldTestament = true;
            this.SupportsNewTestament = true;
            this.SupportsDeuterocanon = true;
        }
    }

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

    public class MockLXX : Version
    {
        public MockLXX()
        {
            this.Name = "Septuagint (LXX)";
            this.Abbreviation = "LXX";
            this.Source = "ab";
            this.SupportsOldTestament = true;
            this.SupportsNewTestament = false;
            this.SupportsDeuterocanon = true;
        }
    }

    public class MockELXX : Version
    {
        public MockELXX()
        {
            this.Name = "Brenton's Septuagint (ELXX)";
            this.Abbreviation = "ELXX";
            this.Source = "ab";
            this.SupportsOldTestament = true;
            this.SupportsNewTestament = false;
            this.SupportsDeuterocanon = true;
        }
    }

    public class MockPAT1904 : Version
    {
        public MockPAT1904()
        {
            this.Name = "Patriarchal Text of 1904 (PAT1904)";
            this.Abbreviation = "PAT1904";
            this.Source = "ab";
            this.SupportsOldTestament = false;
            this.SupportsNewTestament = true;
            this.SupportsDeuterocanon = false;
        }
    }

    public class MockISV : Version
    {
        public MockISV()
        {
            this.Name = "International Standard Version (ISV)";
            this.Abbreviation = "ISV";
            this.Source = "bg";
            this.SupportsOldTestament = true;
            this.SupportsNewTestament = true;
            this.SupportsDeuterocanon = false;
        }
    }

    public class MockNIV : Version
    {
        public MockNIV()
        {
            this.Name = "New International Version (NIV)";
            this.Abbreviation = "NIV";
            this.Source = "bg";
            this.SupportsOldTestament = true;
            this.SupportsNewTestament = true;
            this.SupportsDeuterocanon = false;
        }
    }
}