/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using BibleBot.Models;

namespace BibleBot.Backend.Tests.Mocks
{
    public class MockRSV : Version
    {
        public MockRSV()
        {
            Name = "Revised Standard Version (RSV)";
            Abbreviation = "RSV";
            Source = "bg";
            SupportsOldTestament = true;
            SupportsNewTestament = true;
            SupportsDeuterocanon = true;
        }
    }

    public class MockNTFE : Version
    {
        public MockNTFE()
        {
            Name = "New Testament for Everyone (NTFE)";
            Abbreviation = "NTFE";
            Source = "bg";
            SupportsOldTestament = false;
            SupportsNewTestament = true;
            SupportsDeuterocanon = false;
        }
    }

    public class MockKJVA : Version
    {
        public MockKJVA()
        {
            Name = "King James Version with Apocrypha (KJVA)";
            Abbreviation = "KJVA";
            Source = "ab";
            SupportsOldTestament = true;
            SupportsNewTestament = true;
            SupportsDeuterocanon = true;
        }
    }

    public class MockLXX : Version
    {
        public MockLXX()
        {
            Name = "Septuagint (LXX)";
            Abbreviation = "LXX";
            Source = "ab";
            SupportsOldTestament = true;
            SupportsNewTestament = false;
            SupportsDeuterocanon = true;
        }
    }

    public class MockELXX : Version
    {
        public MockELXX()
        {
            Name = "Brenton's Septuagint (ELXX)";
            Abbreviation = "ELXX";
            Source = "ab";
            SupportsOldTestament = true;
            SupportsNewTestament = false;
            SupportsDeuterocanon = true;
        }
    }

    public class MockPAT1904 : Version
    {
        public MockPAT1904()
        {
            Name = "Patriarchal Text of 1904 (PAT1904)";
            Abbreviation = "PAT1904";
            Source = "ab";
            SupportsOldTestament = false;
            SupportsNewTestament = true;
            SupportsDeuterocanon = false;
        }
    }

    public class MockISV : Version
    {
        public MockISV()
        {
            Name = "International Standard Version (ISV)";
            Abbreviation = "ISV";
            Source = "bg";
            SupportsOldTestament = true;
            SupportsNewTestament = true;
            SupportsDeuterocanon = false;
        }
    }

    public class MockNIV : Version
    {
        public MockNIV()
        {
            Name = "New International Version (NIV)";
            Abbreviation = "NIV";
            Source = "bg";
            SupportsOldTestament = true;
            SupportsNewTestament = true;
            SupportsDeuterocanon = false;
        }
    }

    public class MockWYC : Version
    {
        public MockWYC()
        {
            Name = "Wycliffe Bible (WYC)";
            Abbreviation = "WYC";
            Source = "bg";
            SupportsOldTestament = true;
            SupportsNewTestament = true;
            SupportsDeuterocanon = true;
        }
    }

    public class MockCEB : Version
    {
        public MockCEB()
        {
            Name = "Common English Bible (CEB)";
            Abbreviation = "CEB";
            Source = "bg";
            SupportsOldTestament = true;
            SupportsNewTestament = true;
            SupportsDeuterocanon = true;
        }
    }

    public class MockNRSVA : Version
    {
        public MockNRSVA()
        {
            Name = "New Revised Standard Version, Anglicised (NRSVA)";
            Abbreviation = "NRSVA";
            Source = "bg";
            SupportsOldTestament = true;
            SupportsNewTestament = true;
            SupportsDeuterocanon = true;
        }
    }

    public class MockNKJV : Version
    {
        public MockNKJV()
        {
            Name = "New King James Version";
            Abbreviation = "NKJV";
            Source = "bg";
            SupportsOldTestament = true;
            SupportsNewTestament = true;
            SupportsDeuterocanon = false;
        }
    }

    public class MockERVAR : Version
    {
        public MockERVAR()
        {
            Name = "Arabic Bible: Easy-to-Read Version";
            Abbreviation = "ERV-AR";
            Source = "bg";
            SupportsOldTestament = true;
            SupportsNewTestament = true;
            SupportsDeuterocanon = false;
        }
    }
}