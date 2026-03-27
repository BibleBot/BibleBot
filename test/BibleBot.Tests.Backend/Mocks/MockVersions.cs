/*
* Copyright (C) 2016-2026 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using BibleBot.Models;
using Version = BibleBot.Models.Version;

namespace BibleBot.Tests.Backend.Mocks
{
    public static class MockVersionHelper
    {
        private static readonly Dictionary<string, Dictionary<string, string>> _bookMap;

        static MockVersionHelper()
        {
            string[] paths =
            [
                "src/BibleBot.Backend/Data/book_map.json",
                Path.Combine(AppContext.BaseDirectory, "../../../../../src/BibleBot.Backend/Data/book_map.json")
            ];

            foreach (string path in paths)
            {
                if (!File.Exists(path))
                {
                    continue;
                }

                string json = File.ReadAllText(path);
                _bookMap = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json);
                break;
            }
        }

        public static void PopulateMockBooks(Version version)
        {
            if (_bookMap == null) return;

            if (version.SupportsOldTestament && _bookMap.TryGetValue("ot", out var otBooks))
            {
                AddBooks(version, otBooks);
            }

            if (version.SupportsNewTestament && _bookMap.TryGetValue("nt", out var ntBooks))
            {
                AddBooks(version, ntBooks);
            }

            if (version.SupportsDeuterocanon && _bookMap.TryGetValue("deu", out var deuBooks))
            {
                AddBooks(version, deuBooks);
            }
        }

        private static void AddBooks(Version version, Dictionary<string, string> books)
        {
            foreach (KeyValuePair<string, string> kvp in books)
            {
                if (kvp.Key == "PS2")
                {
                    version.Books.Find(b => b.Name == "PSA").Chapters.Add(new Chapter { Number = 151 });
                    continue;
                }

                var book = new Book
                {
                    Name = kvp.Key,
                    ProperName = kvp.Value,
                    VersionId = version.Id
                };

                if (version.Source == "bg")
                {
                    if (book.Name == "AZA")
                    {
                        book.InternalName = "praz";
                        book.PreferredName = "Prayer of Azariah";
                    }
                    else if (book.Name == "ESA")
                    {
                        book.InternalName = "addesth";
                        book.PreferredName = "Additions to Esther";
                    }
                }

                for (int i = 1; i <= 150; i++)
                {
                    book.Chapters.Add(new Chapter { Number = i });
                }

                version.Books.Add(book);
            }
        }
    }

    public class MockRSV : Version
    {
        public MockRSV()
        {
            Name = "Revised Standard Version (RSV)";
            Id = "RSV";
            Source = "bg";
            SupportsOldTestament = true;
            SupportsNewTestament = true;
            SupportsDeuterocanon = true;

            MockVersionHelper.PopulateMockBooks(this);
        }
    }

    public class MockNTFE : Version
    {
        public MockNTFE()
        {
            Name = "New Testament for Everyone (NTFE)";
            Id = "NTFE";
            Source = "bg";
            SupportsOldTestament = false;
            SupportsNewTestament = true;
            SupportsDeuterocanon = false;

            MockVersionHelper.PopulateMockBooks(this);
        }
    }

    public class MockKJV : Version
    {
        public MockKJV()
        {
            Name = "King James Version with Apocrypha (KJV)";
            Id = "KJV";
            Source = "ab";
            SupportsOldTestament = true;
            SupportsNewTestament = true;
            SupportsDeuterocanon = true;
            InternalId = "de4e12af7f28f599-01";

            MockVersionHelper.PopulateMockBooks(this);
        }
    }

    public class MockLXX : Version
    {
        public MockLXX()
        {
            Name = "Septuagint (LXX)";
            Id = "LXX";
            Source = "ab";
            SupportsOldTestament = true;
            SupportsNewTestament = false;
            SupportsDeuterocanon = true;
            InternalId = "c114c33098c4fef1-01";

            MockVersionHelper.PopulateMockBooks(this);
        }
    }

    public class MockELXX : Version
    {
        public MockELXX()
        {
            Name = "Brenton's Septuagint (ELXX)";
            Id = "ELXX";
            Source = "ab";
            SupportsOldTestament = true;
            SupportsNewTestament = false;
            SupportsDeuterocanon = true;
            InternalId = "65bfdebd704a8324-01";

            MockVersionHelper.PopulateMockBooks(this);
        }
    }

    public class MockPAT1904 : Version
    {
        public MockPAT1904()
        {
            Name = "Patriarchal Text of 1904 (PAT1904)";
            Id = "PAT1904";
            Source = "ab";
            SupportsOldTestament = false;
            SupportsNewTestament = true;
            SupportsDeuterocanon = false;
            InternalId = "901dcd9744e1bf69-01";

            MockVersionHelper.PopulateMockBooks(this);
        }
    }

    public class MockISV : Version
    {
        public MockISV()
        {
            Name = "International Standard Version (ISV)";
            Id = "ISV";
            Source = "bg";
            SupportsOldTestament = true;
            SupportsNewTestament = true;
            SupportsDeuterocanon = false;

            MockVersionHelper.PopulateMockBooks(this);
        }
    }

    public class MockNIV : Version
    {
        public MockNIV()
        {
            Name = "New International Version (NIV)";
            Id = "NIV";
            Source = "ab";
            SupportsOldTestament = true;
            SupportsNewTestament = true;
            SupportsDeuterocanon = false;
            InternalId = "78a9f6124f344018-01";

            MockVersionHelper.PopulateMockBooks(this);
        }
    }

    public class MockWYC : Version
    {
        public MockWYC()
        {
            Name = "Wycliffe Bible (WYC)";
            Id = "WYC";
            Source = "bg";
            SupportsOldTestament = true;
            SupportsNewTestament = true;
            SupportsDeuterocanon = true;

            MockVersionHelper.PopulateMockBooks(this);
        }
    }

    public class MockCEB : Version
    {
        public MockCEB()
        {
            Name = "Common English Bible (CEB)";
            Id = "CEB";
            Source = "bg";
            SupportsOldTestament = true;
            SupportsNewTestament = true;
            SupportsDeuterocanon = true;

            MockVersionHelper.PopulateMockBooks(this);
        }
    }

    public class MockNRSV : Version
    {
        public MockNRSV()
        {
            Name = "New Revised Standard Version (NRSV)";
            Id = "NRSV";
            Source = "bg";
            SupportsOldTestament = true;
            SupportsNewTestament = true;
            SupportsDeuterocanon = true;
            AliasOfId = "NRSVA";

            MockVersionHelper.PopulateMockBooks(this);
        }
    }

    public class MockNRSVA : Version
    {
        public MockNRSVA()
        {
            Name = "New Revised Standard Version, Anglicised (NRSVA)";
            Id = "NRSVA";
            Source = "bg";
            SupportsOldTestament = true;
            SupportsNewTestament = true;
            SupportsDeuterocanon = true;

            MockVersionHelper.PopulateMockBooks(this);
        }
    }

    public class MockNKJV : Version
    {
        public MockNKJV()
        {
            Name = "New King James Version";
            Id = "NKJV";
            Source = "bg";
            SupportsOldTestament = true;
            SupportsNewTestament = true;
            SupportsDeuterocanon = false;

            MockVersionHelper.PopulateMockBooks(this);
        }
    }

    public class MockERVAR : Version
    {
        public MockERVAR()
        {
            Name = "Arabic Bible: Easy-to-Read Version";
            Id = "ERV-AR";
            Source = "bg";
            SupportsOldTestament = true;
            SupportsNewTestament = true;
            SupportsDeuterocanon = false;

            MockVersionHelper.PopulateMockBooks(this);
        }
    }

    public class MockWLC : Version
    {
        public MockWLC()
        {
            Name = "Westminister Leningrad Codex (WLC)";
            Id = "WLC";
            Source = "ab";
            SupportsOldTestament = true;
            SupportsNewTestament = false;
            SupportsDeuterocanon = false;
            InternalId = "0b262f1ed7f084a6-01";

            MockVersionHelper.PopulateMockBooks(this);
        }
    }
}
