/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using BibleBot.Models;

namespace BibleBot.Tests.Backend.Mocks
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

    public class MockKJV : Version
    {
        public MockKJV()
        {
            Name = "King James Version with Apocrypha (KJV)";
            Abbreviation = "KJV";
            Source = "ab";
            SupportsOldTestament = true;
            SupportsNewTestament = true;
            SupportsDeuterocanon = true;
            InternalId = "de4e12af7f28f599-01";
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
            InternalId = "c114c33098c4fef1-01";
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
            InternalId = "65bfdebd704a8324-01";
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
            InternalId = "901dcd9744e1bf69-01";
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
            Source = "ab";
            SupportsOldTestament = true;
            SupportsNewTestament = true;
            SupportsDeuterocanon = false;
            InternalId = "78a9f6124f344018-01";
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