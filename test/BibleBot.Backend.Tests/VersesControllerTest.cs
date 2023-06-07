/*
* Copyright (C) 2016-2023 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using BibleBot.Backend.Controllers;
using BibleBot.Backend.Models;
using BibleBot.Backend.Services;
using BibleBot.Backend.Services.Providers;
using BibleBot.Backend.Tests.Mocks;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace BibleBot.Backend.Tests
{
    [TestFixture, Category("VersesController")]
    public class VersesControllerTest
    {
        private VersesController versesController;

        private VersionService versionService;

        private Mock<UserService> userServiceMock;
        private Mock<GuildService> guildServiceMock;
        private Mock<ParsingService> parsingServiceMock;
        private Mock<NameFetchingService> nameFetchingServiceMock;

        private Mock<BibleGatewayProvider> bgProviderMock;
        private Mock<APIBibleProvider> abProviderMock;

        private IDatabaseSettings databaseSettings;

        private Models.Version defaultBibleGatewayVersion;
        private Models.Version defaultAPIBibleVersion;

        [SetUp]
        public void Setup()
        {
            databaseSettings = new DatabaseSettings
            {
                UserCollectionName = "Users",
                GuildCollectionName = "Guilds",
                VersionCollectionName = "Versions",
                LanguageCollectionName = "Languages",
                FrontendStatsCollectionName = "FrontendStats",
                DatabaseName = "BibleBotBackend"
            };

            userServiceMock = new Mock<UserService>(databaseSettings);
            guildServiceMock = new Mock<GuildService>(databaseSettings);
            versionService = new VersionService(databaseSettings);
            parsingServiceMock = new Mock<ParsingService>(versionService);
            nameFetchingServiceMock = new Mock<NameFetchingService>();

            bgProviderMock = new Mock<BibleGatewayProvider>();
            abProviderMock = new Mock<APIBibleProvider>();

            defaultBibleGatewayVersion = versionService.Get("RSV");
            if (defaultBibleGatewayVersion == null)
            {
                defaultBibleGatewayVersion = versionService.Create(new MockRSV());
            }

            defaultAPIBibleVersion = versionService.Get("KJVA");
            if (defaultAPIBibleVersion == null)
            {
                defaultAPIBibleVersion = versionService.Create(new MockKJVA());
            }

            versesController = new VersesController(userServiceMock.Object, guildServiceMock.Object,
                                                    parsingServiceMock.Object, versionService,
                                                    nameFetchingServiceMock.Object, bgProviderMock.Object,
                                                    abProviderMock.Object);
        }

        [Test]
        public void ShouldFailWhenTokenIsInvalid()
        {
            var req = new MockRequest();
            req.Token = "meowmix";

            var resp = versesController.ProcessMessage(req).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = false,
                LogStatement = null
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldFailWhenBodyIsEmpty()
        {
            var resp = versesController.ProcessMessage(new MockRequest()).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = false,
                LogStatement = null
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessBibleGatewayReference()
        {
            var resp = versesController.ProcessMessage(new MockRequest("Matthew 1:1")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = true,
                LogStatement = "Matthew 1:1 RSV",
                DisplayStyle = "embed",
                Verses = new List<Verse>
                {
                    new Verse
                    {
                        Title = "The Genealogy of Jesus the Messiah",
                        PsalmTitle = "",
                        Text = "<**1**> The book of the genealogy of Jesus Christ, the son of David, the son of Abraham.",
                        Reference = new Reference
                        {
                            Book = "Matthew",
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 1,
                            Version = defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "Matthew 1:1"
                        }
                    }
                }
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessAPIBibleReference()
        {
            var resp = versesController.ProcessMessage(new MockRequest("Genesis 1:1 KJVA")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = true,
                LogStatement = "Genesis 1:1 KJVA",
                DisplayStyle = "embed",
                Verses = new List<Verse>
                {
                    new Verse
                    {
                        Title = "",
                        PsalmTitle = "",
                        Text = "<**1**> In the beginning God created the heaven and the earth.",
                        Reference = new Reference
                        {
                            Book = "Genesis",
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 1,
                            Version = defaultAPIBibleVersion,
                            IsOT = true,
                            IsNT = false,
                            IsDEU = false,
                            AsString = "Genesis 1:1"
                        }
                    }
                }
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldFailWhenReferencingDeuterocanonInProtestantBible()
        {
            var testVersion = versionService.Get("NTE");

            if (testVersion == null)
            {
                testVersion = versionService.Create(new MockNTE());
            }

            var resp = versesController.ProcessMessage(new MockRequest("Sirach 1:1 NTE")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = false,
                LogStatement = "New Testament for Everyone (NTE) does not support the Apocrypha/Deuterocanon."
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldFailWhenReferencingOldTestamentInNewTestamentOnlyBible()
        {
            var testVersion = versionService.Get("NTE");

            if (testVersion == null)
            {
                testVersion = versionService.Create(new MockNTE());
            }

            var resp = versesController.ProcessMessage(new MockRequest("Genesis 1:1 NTE")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = false,
                LogStatement = "New Testament for Everyone (NTE) does not support the Old Testament."
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldIgnoreMultipleVerseReferencesInIgnoringBrackets()
        {
            var resp = versesController.ProcessMessage(new MockRequest("lorem < Genesis 1:1 NTE / Matthew 1:1 NTE / Acts 1:1 NTE > ipsum John 1:1 dolor < Genesis 1:1 NTE > sit")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = true,
                LogStatement = "John 1:1 RSV",
                DisplayStyle = "embed",
                Verses = new List<Verse>
                {
                    new Verse
                    {
                        Title = "The Word Became Flesh",
                        PsalmTitle = "",
                        Text = "<**1**> In the beginning was the Word, and the Word was with God, and the Word was God.",
                        Reference = new Reference
                        {
                            Book = "John",
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 1,
                            Version = defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "John 1:1"
                        }
                    }
                }
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessVerseInNonIgnoringBrackets()
        {
            var resp = versesController.ProcessMessage(new MockRequest("(John 1:1)")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = true,
                LogStatement = "John 1:1 RSV",
                DisplayStyle = "embed",
                Verses = new List<Verse>
                {
                    new Verse
                    {
                        Title = "The Word Became Flesh",
                        PsalmTitle = "",
                        Text = "<**1**> In the beginning was the Word, and the Word was with God, and the Word was God.",
                        Reference = new Reference
                        {
                            Book = "John",
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 1,
                            Version = defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "John 1:1"
                        }
                    }
                }
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessBibleGatewaySpannedReference()
        {
            var resp = versesController.ProcessMessage(new MockRequest("Matthew 1:1-2")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = true,
                LogStatement = "Matthew 1:1-2 RSV",
                DisplayStyle = "embed",
                Verses = new List<Verse>
                {
                    new Verse
                    {
                        Title = "The Genealogy of Jesus the Messiah",
                        PsalmTitle = "",
                        Text = "<**1**> The book of the genealogy of Jesus Christ, the son of David, the son of Abraham. <**2**> Abraham was the father of Isaac, and Isaac the father of Jacob, and Jacob the father of Judah and his brothers,",
                        Reference = new Reference
                        {
                            Book = "Matthew",
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 2,
                            Version = defaultBibleGatewayVersion,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "Matthew 1:1-2"
                        }
                    }
                }
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessAPIBibleSpannedReference()
        {
            var resp = versesController.ProcessMessage(new MockRequest("Genesis 1:1-2 KJVA")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = true,
                LogStatement = "Genesis 1:1-2 KJVA",
                DisplayStyle = "embed",
                Verses = new List<Verse>
                {
                    new Verse
                    {
                        Title = "",
                        PsalmTitle = "",
                        Text = "<**1**> In the beginning God created the heaven and the earth. <**2**> And the earth was without form, and void; and darkness was upon the face of the deep. And the Spirit of God moved upon the face of the waters.",
                        Reference = new Reference
                        {
                            Book = "Genesis",
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 2,
                            Version = defaultAPIBibleVersion,
                            IsOT = true,
                            IsNT = false,
                            IsDEU = false,
                            AsString = "Genesis 1:1-2"
                        }
                    }
                }
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessBibleGatewaySpannedChapterReference()
        {
            var resp = versesController.ProcessMessage(new MockRequest("Matthew 1:25-2:1")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = true,
                LogStatement = "Matthew 1:25-2:1 RSV",
                DisplayStyle = "embed",
                Verses = new List<Verse>
                {
                    new Verse
                    {
                        Title = "The Visit of the Wise Men",
                        PsalmTitle = "",
                        Text = "<**25**> but knew her not until she had borne a son; and he called his name Jesus. <**1**> Now when Jesus was born in Bethlehem of Judea in the days of Herod the king, behold, wise men from the East came to Jerusalem, saying,",
                        Reference = new Reference
                        {
                            Book = "Matthew",
                            StartingChapter = 1,
                            StartingVerse = 25,
                            EndingChapter = 2,
                            EndingVerse = 1,
                            Version = defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "Matthew 1:25-2:1"
                        }
                    }
                }
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessAPIBibleSpannedChapterReference()
        {
            var resp = versesController.ProcessMessage(new MockRequest("Genesis 1:31-2:1 KJVA")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = true,
                LogStatement = "Genesis 1:31-2:1 KJVA",
                DisplayStyle = "embed",
                Verses = new List<Verse>
                {
                    new Verse
                    {
                        Title = "",
                        PsalmTitle = "",
                        Text = "<**31**> And God saw every thing that he had made, and, behold, it was very good. And the evening and the morning were the sixth day. <**1**> Thus the heavens and the earth were finished, and all the host of them.",
                        Reference = new Reference
                        {
                            Book = "Genesis",
                            StartingChapter = 1,
                            StartingVerse = 31,
                            EndingChapter = 2,
                            EndingVerse = 1,
                            Version = defaultAPIBibleVersion,
                            IsOT = true,
                            IsNT = false,
                            IsDEU = false,
                            AsString = "Genesis 1:31-2:1"
                        }
                    }
                }
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessBibleGatewayExpandedReference()
        {
            var resp = versesController.ProcessMessage(new MockRequest("Matthew 1:24-")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = true,
                LogStatement = "Matthew 1:24- RSV",
                DisplayStyle = "embed",
                Verses = new List<Verse>
                {
                    new Verse
                    {
                        Title = "",
                        PsalmTitle = "",
                        Text = "<**24**> When Joseph woke from sleep, he did as the angel of the Lord commanded him; he took his wife, <**25**> but knew her not until she had borne a son; and he called his name Jesus.",
                        Reference = new Reference
                        {
                            Book = "Matthew",
                            StartingChapter = 1,
                            StartingVerse = 24,
                            EndingChapter = 1,
                            EndingVerse = 0,
                            Version = defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "Matthew 1:24-25"
                        }
                    }
                }
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldIgnoreAPIBibleExpandedReference()
        {
            var resp = versesController.ProcessMessage(new MockRequest("Matthew 1:24- KJVA")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = false,
                LogStatement = null
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldNotProcessReferenceStartingWithVerseZero()
        {
            var resp = versesController.ProcessMessage(new MockRequest("Genesis 1:0")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = false,
                LogStatement = null
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldNotProcessReferenceWithSpaceBetweenColonAndVerseNumbers()
        {
            var resp = versesController.ProcessMessage(new MockRequest("Genesis 1: 1-5")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = false,
                LogStatement = null
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessReferenceWithFullWidthColon()
        {
            var resp = versesController.ProcessMessage(new MockRequest("Matthew 1：1-2")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = true,
                LogStatement = "Matthew 1:1-2 RSV",
                DisplayStyle = "embed",
                Verses = new List<Verse>
                {
                    new Verse
                    {
                        Title = "The Genealogy of Jesus the Messiah",
                        PsalmTitle = "",
                        Text = "<**1**> The book of the genealogy of Jesus Christ, the son of David, the son of Abraham. <**2**> Abraham was the father of Isaac, and Isaac the father of Jacob, and Jacob the father of Judah and his brothers,",
                        Reference = new Reference
                        {
                            Book = "Matthew",
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 2,
                            Version = defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "Matthew 1:1-2"
                        }
                    }
                }
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldNotProcessSameVerseSpannedReferenceAsExpando()
        {
            var resp = versesController.ProcessMessage(new MockRequest("Matthew 1:1-1")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = true,
                LogStatement = "Matthew 1:1 RSV",
                DisplayStyle = "embed",
                Verses = new List<Verse>
                {
                    new Verse
                    {
                        Title = "The Genealogy of Jesus the Messiah",
                        PsalmTitle = "",
                        Text = "<**1**> The book of the genealogy of Jesus Christ, the son of David, the son of Abraham.",
                        Reference = new Reference
                        {
                            Book = "Matthew",
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 1,
                            Version = defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "Matthew 1:1"
                        }
                    }
                }
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessDanielInEnglishSeptuagint()
        {
            var testVersion = versionService.Get("ELXX");
            if (testVersion == null)
            {
                testVersion = versionService.Create(new MockELXX());
            }

            var resp = versesController.ProcessMessage(new MockRequest("Daniel 1:1-2 ELXX")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = true,
                LogStatement = "Daniel 1:1-2 ELXX",
                DisplayStyle = "embed",
                Verses = new List<Verse>
                {
                    new Verse
                    {
                        Title = "",
                        PsalmTitle = "",
                        Text = "<**1**> In the third year of the reign of Joakim king of Judah, came Nebuchadnezzar king of Babylon to Jerusalem, and besieged it. <**2**> And the Lord gave into his hand Joakim king of Judah, and part of the vessels of the house of God: and he brought them into the land of Shinar to the house of his god; and he brought the vessels into the treasure house of his god.",
                        Reference = new Reference
                        {
                            Book = "Daniel",
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 2,
                            Version = testVersion,
                            IsOT = true,
                            IsNT = false,
                            IsDEU = false,
                            AsString = "Daniel 1:1-2"
                        }
                    }
                }
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessDanielInSeptuagint()
        {
            var testVersion = versionService.Get("LXX");
            if (testVersion == null)
            {
                testVersion = versionService.Create(new MockLXX());
            }

            var resp = versesController.ProcessMessage(new MockRequest("Daniel 1:1-2 LXX")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = true,
                LogStatement = "Daniel 1:1-2 LXX",
                DisplayStyle = "embed",
                Verses = new List<Verse>
                {
                    new Verse
                    {
                        Title = "",
                        PsalmTitle = "",
                        Text = "<**1**> ἘΝ ἔτει τρίτῳ τῆς βασιλείας Ἰωακεὶμ βασιλέως Ἰούδα, ἦλθε Ναβουχοδονόσορ ὁ βασιλεὺς Βαβυλῶνος εἰς Ἱερουσαλὴμ, καὶ ἐπολιόρκει αὐτήν. <**2**> Καὶ ἔδωκε Κύριος ἐν χειρὶ αὐτοῦ τὸν Ἰωακεὶμ βασιλέα Ἰούδα, καὶ ἀπὸ μέρους τῶν σκευῶν οἴκου τοῦ Θεοῦ· καὶ ἤνεγκεν αὐτὰ εἰς γῆν Σενναὰρ οἴκου τοῦ θεοῦ αὐτοῦ, καὶ τὰ σκεύη εἰσήνεγκεν εἰς τὸν οἶκον θησαυροῦ τοῦ θεοῦ αὐτοῦ.",
                        Reference = new Reference
                        {
                            Book = "Daniel",
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 2,
                            Version = testVersion,
                            IsOT = true,
                            IsNT = false,
                            IsDEU = false,
                            AsString = "Daniel 1:1-2"
                        }
                    }
                }
            };

            resp.Should().BeEquivalentTo(expected);
        }

        // [Test]
        // public void ShouldProcessJohnInPatriarchalText()
        // {
        //     var testVersion = versionService.Get("PAT1904");
        //     if (testVersion == null)
        //     {
        //         testVersion = versionService.Create(new MockPAT1904());
        //     }
        //
        //     var resp = versesController.ProcessMessage(new MockRequest("John 1:1-2 PAT1904")).GetAwaiter().GetResult();
        //
        //     var expected = new VerseResponse
        //     {
        //         OK = true,
        //         LogStatement = "John 1:1-2 PAT1904",
        //         DisplayStyle = "embed",
        //         Verses = new List<Verse>
        //         {
        //             new Verse
        //             {
        //                 Title = "",
        //                 PsalmTitle = "",
        //                 Text = "<**1**> Ἐν ἀρχῇ ἦν ὁ Λόγος, καὶ ὁ Λόγος ἦν πρὸς τὸν Θεόν, καὶ Θεὸς ἦν ὁ Λόγος. <**2**> Οὗτος ἦν ἐν ἀρχῇ πρὸς τὸν Θεόν.",
        //                 Reference = new Reference
        //                 {
        //                     Book = "John",
        //                     StartingChapter = 1,
        //                     StartingVerse = 1,
        //                     EndingChapter = 1,
        //                     EndingVerse = 2,
        //                     Version = testVersion,
        //                     IsOT = false,
        //                     IsNT = true,
        //                     IsDEU = false,
        //                     AsString = "John 1:1-2"
        //                 }
        //             }
        //         }
        //     };

        //     resp.Should().BeEquivalentTo(expected);
        // }

        [Test]
        public void ShouldNotMishandleMultipleSpansInProverbsInNIV()
        {
            var testVersion = versionService.Get("NIV");
            if (testVersion == null)
            {
                testVersion = versionService.Create(new MockNIV());
            }

            var resp = versesController.ProcessMessage(new MockRequest("Proverbs 25:1-12 NIV")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = true,
                LogStatement = "Proverbs 25:1-12 NIV",
                DisplayStyle = "embed",
                Verses = new List<Verse>
                {
                    new Verse
                    {
                        Title = "More Proverbs of Solomon",
                        PsalmTitle = "",
                        Text = "<**1**> These are more proverbs of Solomon, compiled by the men of Hezekiah king of Judah: <**2**> It is the glory of God to conceal a matter; to search out a matter is the glory of kings. <**3**> As the heavens are high and the earth is deep, so the hearts of kings are unsearchable. <**4**> Remove the dross from the silver, and a silversmith can produce a vessel; <**5**> remove wicked officials from the king's presence, and his throne will be established through righteousness. <**6**> Do not exalt yourself in the king's presence, and do not claim a place among his great men; <**7**> it is better for him to say to you, \"Come up here,\" than for him to humiliate you before his nobles. What you have seen with your eyes <**8**> do not bring hastily to court, for what will you do in the end if your neighbor puts you to shame? <**9**> If you take your neighbor to court, do not betray another's confidence, <**10**> or the one who hears it may shame you and the charge against you will stand. <**11**> Like apples of gold in settings of silver is a ruling rightly given. <**12**> Like an earring of gold or an ornament of fine gold is the rebuke of a wise judge to a listening ear.",
                        Reference = new Reference
                        {
                            Book = "Proverbs",
                            StartingChapter = 25,
                            StartingVerse = 1,
                            EndingChapter = 25,
                            EndingVerse = 12,
                            Version = testVersion,
                            IsOT = true,
                            IsNT = false,
                            IsDEU = false,
                            AsString = "Proverbs 25:1-12"
                        }
                    }
                }
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldWorkAroundHebrewVerseNumbersInExodusInISVPartOne()
        {
            var testVersion = versionService.Get("ISV");
            if (testVersion == null)
            {
                testVersion = versionService.Create(new MockISV());
            }

            var resp = versesController.ProcessMessage(new MockRequest("Exodus 20:1-7 ISV")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = true,
                LogStatement = "Exodus 20:1-7 ISV",
                DisplayStyle = "embed",
                Verses = new List<Verse>
                {
                    new Verse
                    {
                        Title = "The Ten Commandments",
                        PsalmTitle = "",
                        Text = "<**1**> Then God spoke all these words: <**2**> \"I am the Lord your God, who brought you out of the land of Egypt— from the house of slavery. <**3**> You are to have no other gods as a substitute for me. <**4**> \"You are not to craft for yourselves an idol or anything resembling what is in the skies above, or on earth beneath, or in the water sources under the earth. <**5**> You are not to bow down to them in worship or serve them, because I, the Lord your God, am a jealous God, visiting the guilt of parents on children, to the third and fourth generation of those who hate me, <**6**> but showing gracious love to the thousands of those who love me and keep my commandments. <**7**> \"You are not to misuse the name of the Lord your God, because the Lord will not leave unpunished the one who misuses his name.",
                        Reference = new Reference
                        {
                            Book = "Exodus",
                            StartingChapter = 20,
                            StartingVerse = 1,
                            EndingChapter = 20,
                            EndingVerse = 7,
                            Version = testVersion,
                            IsOT = true,
                            IsNT = false,
                            IsDEU = false,
                            AsString = "Exodus 20:1-7"
                        }
                    }
                }
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldWorkAroundHebrewVerseNumbersInExodusInISVPartTwo()
        {
            var testVersion = versionService.Get("ISV");
            if (testVersion == null)
            {
                testVersion = versionService.Create(new MockISV());
            }

            var resp = versesController.ProcessMessage(new MockRequest("Exodus 20:8-17 ISV")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = true,
                LogStatement = "Exodus 20:8-17 ISV",
                DisplayStyle = "embed",
                Verses = new List<Verse>
                {
                    new Verse
                    {
                        Title = "",
                        PsalmTitle = "",
                        Text = "<**8**> \"Remember the Sabbath day, maintaining its holiness. <**9**> Six days you are to labor and do all your work, <**10**> but the seventh day is a Sabbath to the Lord your God. You are not to do any work—neither you, nor your son, nor your daughter, nor your male or female servant, nor your livestock, nor any foreigner who lives among you— <**11**> because the Lord made the heavens, the earth, the sea, and everything that is in them in six days. Then he rested on the seventh day. Therefore, the Lord blessed the Sabbath day and made it holy. <**12**> \"Honor your father and your mother, so that you may live long in the land that the Lord your God is giving you. <**13**> \"You are not to commit murder. <**14**> \"You are not to commit adultery. <**15**> \"You are not to steal. <**16**> \"You are not to give false testimony against your neighbor. <**17**> \"You are not to desire your neighbor's house, nor your neighbor's wife, his male or female servant, his ox, his donkey, nor anything else that pertains to your neighbor.\"",
                        Reference = new Reference
                        {
                            Book = "Exodus",
                            StartingChapter = 20,
                            StartingVerse = 8,
                            EndingChapter = 20,
                            EndingVerse = 17,
                            Version = testVersion,
                            IsOT = true,
                            IsNT = false,
                            IsDEU = false,
                            AsString = "Exodus 20:8-17"
                        }
                    }
                }
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldNotProcessEsdrasBooksAsEzra()
        {
            var resp = versesController.ProcessMessage(new MockRequest("1 Esdras 1:1")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = true,
                LogStatement = "1 Esdras 1:1 RSV",
                DisplayStyle = "embed",
                Verses = new List<Verse>
                {
                    new Verse
                    {
                        Title = "Josiah Celebrates the Passover",
                        PsalmTitle = "",
                        Text = "<**1**> Josi'ah kept the passover to his Lord in Jerusalem; he killed the passover lamb on the fourteenth day of the first month,",
                        Reference = new Reference
                        {
                            Book = "1 Esdras",
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 1,
                            Version = defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = false,
                            IsDEU = true,
                            AsString = "1 Esdras 1:1"
                        }
                    }
                }
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldNotProcessLetterOfJeremiahAsJeremiah()
        {
            var resp = versesController.ProcessMessage(new MockRequest("Letter of Jeremiah 1:1")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = true,
                LogStatement = "Letter of Jeremiah 1:1 RSV",
                DisplayStyle = "embed",
                Verses = new List<Verse>
                {
                    new Verse
                    {
                        Title = "",
                        PsalmTitle = "",
                        Text = "<**1**> A copy of a letter which Jeremiah sent to those who were to be taken to Babylon as captives by the king of the Babylonians, to give them the message which God had commanded him.",
                        Reference = new Reference
                        {
                            Book = "Letter of Jeremiah",
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 1,
                            Version = defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = false,
                            IsDEU = true,
                            AsString = "Letter of Jeremiah 1"
                        }
                    }
                }
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessJeremiah()
        {
            var resp = versesController.ProcessMessage(new MockRequest("Jeremiah 1:1")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = true,
                LogStatement = "Jeremiah 1:1 RSV",
                DisplayStyle = "embed",
                Verses = new List<Verse>
                {
                    new Verse
                    {
                        Title = "",
                        PsalmTitle = "",
                        Text = "<**1**> The words of Jeremiah, the son of Hilki'ah, of the priests who were in An'athoth in the land of Benjamin,",
                        Reference = new Reference
                        {
                            Book = "Jeremiah",
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 1,
                            Version = defaultBibleGatewayVersion,
                            IsOT = true,
                            IsNT = false,
                            IsDEU = false,
                            AsString = "Jeremiah 1:1"
                        }
                    }
                }
            };

            resp.Should().BeEquivalentTo(expected);
        }
    }
}