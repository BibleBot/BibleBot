/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Threading.Tasks;
using BibleBot.Models;
using BibleBot.Tests.Backend.Mocks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace BibleBot.Tests.Backend.Controllers.Verses
{
    [TestFixture, Category("Verses")]
    public class BasicVersesTest : TestBaseClass
    {
        [Test]
        public void ShouldProcessBibleGatewayReference()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Matthew 1:1")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Matthew 1:1 RSV",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
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
                            Version = _defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "Matthew 1:1"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessAPIBibleReference()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Genesis 1:1 KJVA")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Genesis 1:1 KJVA",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
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
                            Version = _defaultAPIBibleVersion,
                            IsOT = true,
                            IsNT = false,
                            IsDEU = false,
                            AsString = "Genesis 1:1"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldFailWhenReferencingDeuterocanonInProtestantBible()
        {
            _ = _versionService.Get("NTFE") ?? _versionService.Create(new MockNTFE());
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Sirach 1:1 NTFE")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = false,
                LogStatement = "New Testament for Everyone (NTFE) does not support the Apocrypha/Deuterocanon."
            };

            result.StatusCode.Should().Be(400);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldFailWhenReferencingOldTestamentInNewTestamentOnlyBible()
        {
            _ = _versionService.Get("NTFE") ?? _versionService.Create(new MockNTFE());
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Genesis 1:1 NTFE")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = false,
                LogStatement = "New Testament for Everyone (NTFE) does not support the Old Testament."
            };

            result.StatusCode.Should().Be(400);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldIgnoreMultipleVerseReferencesInIgnoringBrackets()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("lorem < Genesis 1:1 NTFE / Matthew 1:1 NTFE / Acts 1:1 NTFE > ipsum John 1:1 dolor < Genesis 1:1 NTFE > sit")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "John 1:1 RSV",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
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
                            Version = _defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "John 1:1"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessVerseInNonIgnoringBrackets()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("(John 1:1)")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "John 1:1 RSV",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
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
                            Version = _defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "John 1:1"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessBibleGatewaySpannedReference()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Matthew 1:1-2")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Matthew 1:1-2 RSV",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
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
                            Version = _defaultBibleGatewayVersion,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "Matthew 1:1-2"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessAPIBibleSpannedReference()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Genesis 1:1-2 KJVA")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Genesis 1:1-2 KJVA",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
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
                            Version = _defaultAPIBibleVersion,
                            IsOT = true,
                            IsNT = false,
                            IsDEU = false,
                            AsString = "Genesis 1:1-2"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessBibleGatewaySpannedChapterReference()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Matthew 1:25-2:1")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Matthew 1:25-2:1 RSV",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
                    {
                        Title = "The Visit of the Wise Men",
                        PsalmTitle = "",
                        Text = "<**25**> but knew her not until she had borne a son; and he called his name Jesus. <**2:1**> Now when Jesus was born in Bethlehem of Judea in the days of Herod the king, behold, wise men from the East came to Jerusalem, saying,",
                        Reference = new Reference
                        {
                            Book = "Matthew",
                            StartingChapter = 1,
                            StartingVerse = 25,
                            EndingChapter = 2,
                            EndingVerse = 1,
                            Version = _defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "Matthew 1:25-2:1"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessAPIBibleSpannedChapterReference()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Genesis 1:31-2:1 KJVA")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Genesis 1:31-2:1 KJVA",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
                    {
                        Title = "",
                        PsalmTitle = "",
                        Text = "<**31**> And God saw every thing that he had made, and, behold, it was very good. And the evening and the morning were the sixth day. <**2:1**> Thus the heavens and the earth were finished, and all the host of them.",
                        Reference = new Reference
                        {
                            Book = "Genesis",
                            StartingChapter = 1,
                            StartingVerse = 31,
                            EndingChapter = 2,
                            EndingVerse = 1,
                            Version = _defaultAPIBibleVersion,
                            IsOT = true,
                            IsNT = false,
                            IsDEU = false,
                            AsString = "Genesis 1:31-2:1"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessBibleGatewayExpandedReference()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Matthew 1:24-")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Matthew 1:24- RSV",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
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
                            Version = _defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "Matthew 1:24-25"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessAPIBibleExpandedReference()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Matthew 1:24- KJVA")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Matthew 1:24- KJVA",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
                    {
                        Title = "",
                        PsalmTitle = "",
                        Text = "<**24**> Then Joseph being raised from sleep did as the angel of the Lord had bidden him, and took unto him his wife: <**25**> And knew her not till she had brought forth her firstborn son: and he called his name JESUS.",
                        Reference = new Reference
                        {
                            Book = "Matthew",
                            StartingChapter = 1,
                            StartingVerse = 24,
                            EndingChapter = 1,
                            EndingVerse = 200,
                            Version = _defaultAPIBibleVersion,
                            IsOT = false,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "Matthew 1:24-25"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldNotProcessReferenceStartingWithVerseZero()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Genesis 1:0")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = false,
                LogStatement = null
            };

            result.StatusCode.Should().Be(400);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldNotProcessReferenceWithSpaceBetweenColonAndVerseNumbers()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Genesis 1: 1-5")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = false,
                LogStatement = null
            };

            result.StatusCode.Should().Be(400);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessReferenceWithFullWidthColon()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Matthew 1：1-2")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Matthew 1:1-2 RSV",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
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
                            Version = _defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "Matthew 1:1-2"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldNotProcessSameVerseSpannedReferenceAsExpando()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Matthew 1:1-1")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Matthew 1:1 RSV",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
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
                            Version = _defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "Matthew 1:1"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldNotProcessEsdrasBooksAsEzra()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("1 Esdras 1:1")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "1 Esdras 1:1 RSV",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
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
                            Version = _defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = false,
                            IsDEU = true,
                            AsString = "1 Esdras 1:1"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldNotProcessLetterOfJeremiahAsJeremiah()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Letter of Jeremiah 1:1")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Letter of Jeremiah 1:1 RSV",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
                    {
                        Title = "",
                        PsalmTitle = "",
                        Text = "<**6:1**> A copy of a letter which Jeremiah sent to those who were to be taken to Babylon as captives by the king of the Babylonians, to give them the message which God had commanded him.",
                        Reference = new Reference
                        {
                            Book = "Letter of Jeremiah",
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 1,
                            Version = _defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = false,
                            IsDEU = true,
                            AsString = "Letter of Jeremiah 1"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessJeremiah()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Jeremiah 1:1")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Jeremiah 1:1 RSV",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
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
                            Version = _defaultBibleGatewayVersion,
                            IsOT = true,
                            IsNT = false,
                            IsDEU = false,
                            AsString = "Jeremiah 1:1"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        // TODO: test 1 Esdras + Ezra, Letter of Jeremiah + Jeremiah in same request

        [Test]
        public void ShouldProcessPsalm151Properly()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Psalm 151:1")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Psalm 151:1 RSV",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
                    {
                        Title = "",
                        PsalmTitle = "This psalm is ascribed to David as his own composition (though it is outside the number), after he had fought in single combat with Goliath.",
                        Text = "<**1**> I was small among my brothers, and youngest in my father's house; I tended my father's sheep.",
                        Reference = new Reference
                        {
                            Book = "Psalm 151",
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 1,
                            Version = _defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = false,
                            IsDEU = true,
                            AsString = "Psalm 151 1"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task ShouldThrowProviderNotFoundException()
        {
            Version testVersion = await _versionService.Create(new Version
            {
                Name = "A Test Version (TEST)",
                Abbreviation = "TEST",
                Source = "test",
                SupportsOldTestament = true,
                SupportsNewTestament = true,
                SupportsDeuterocanon = false
            });

            _ = _versesController
                .Invoking(c => c.ProcessMessage(new MockRequest("Genesis 1:1 TEST")).GetAwaiter().GetResult())
                .Should()
                .Throw<ProviderNotFoundException>();

            await _versionService.Remove(testVersion);
        }


        [Test]
        public void ShouldNotReturnDuplicates()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("John 1:1 / John 1:1")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "John 1:1 RSV",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
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
                            Version = _defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "John 1:1"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task ShouldReturnDuplicatesWhenVersionsDiffer()
        {
            Version testVersion = await _versionService.Get("NTFE") ?? await _versionService.Create(new MockNTFE());

            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Philippians 1:6 / Philippians 1:6 NTFE")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Philippians 1:6 RSV / Philippians 1:6 NTFE",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
                    {
                        Title = "",
                        PsalmTitle = "",
                        Text = "<**6**> And I am sure that he who began a good work in you will bring it to completion at the day of Jesus Christ.",
                        Reference = new Reference
                        {
                            Book = "Philippians",
                            StartingChapter = 1,
                            StartingVerse = 6,
                            EndingChapter = 1,
                            EndingVerse = 6,
                            Version = _defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "Philippians 1:6"
                        }
                    },
                    new()
                    {
                        Title = "",
                        PsalmTitle = "",
                        Text = "<**6**> Of this I'm convinced: the one who began a good work in you will thoroughly complete it by the day of Messiah Jesus.",
                        Reference = new Reference
                        {
                            Book = "Philippians",
                            StartingChapter = 1,
                            StartingVerse = 6,
                            EndingChapter = 1,
                            EndingVerse = 6,
                            Version = testVersion,
                            IsOT = false,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "Philippians 1:6"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldNotProcessGreekEstherAsEsther()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Greek Esther 1:1")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Greek Esther 1:1 RSV",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
                    {
                        Title = "King Ahasu-erus Deposes Queen Vashti",
                        PsalmTitle = "",
                        Text = "<**1**> In the days of Ahasu-e'rus, the Ahasu-e'rus who reigned from India to Ethiopia over one hundred and twenty-seven provinces,",
                        Reference = new Reference
                        {
                            Book = "Greek Esther",
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 1,
                            Version = _defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = false,
                            IsDEU = true,
                            AsString = "Greek Esther 1:1"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task ShouldRemoveH4HeadingsInVerseContent()
        {
            Version testVersion = await _versionService.Get("NKJV") ?? await _versionService.Create(new MockNKJV());

            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Song of Songs 7:9-10 NKJV")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Song of Songs 7:9-10 NKJV",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
                    {
                        Title = "",
                        PsalmTitle = "",
                        Text = "<**9**> And the roof of your mouth like the best wine. The wine goes down smoothly for my beloved, Moving gently the lips of sleepers. <**10**> I am my beloved's, And his desire is toward me.",
                        Reference = new Reference
                        {
                            Book = "Song of Songs",
                            StartingChapter = 7,
                            StartingVerse = 9,
                            EndingChapter = 7,
                            EndingVerse = 10,
                            Version = testVersion,
                            IsOT = true,
                            IsNT = false,
                            IsDEU = false,
                            AsString = "Song of Solomon 7:9-10"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void BGShouldHandleSpanlessCommaNotation()
        {
            ObjectResult spacelessResult = _versesController.ProcessMessage(new MockRequest("Matthew 1:1,3,9")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse spacelessResp = spacelessResult.Value as VerseResponse;

            ObjectResult spacedResult = _versesController.ProcessMessage(new MockRequest("Matthew 1:1, 3, 9")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse spacedResp = spacedResult.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Matthew 1:1, 3, 9 RSV",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
                    {
                        Title = "The Genealogy of Jesus the Messiah",
                        PsalmTitle = "",
                        Text = "<**1**> The book of the genealogy of Jesus Christ, the son of David, the son of Abraham. <**3**> and Judah the father of Perez and Zerah by Tamar, and Perez the father of Hezron, and Hezron the father of Ram, <**9**> and Uzzi'ah the father of Jotham, and Jotham the father of Ahaz, and Ahaz the father of Hezeki'ah,",
                        Reference = new Reference
                        {
                            Book = "Matthew",
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 1,
                            AppendedVerses = [new System.Tuple<int, int>(3, 3), new System.Tuple<int, int>(9, 9)],
                            Version = _defaultBibleGatewayVersion,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "Matthew 1:1, 3, 9"
                        }
                    }
                ]
            };

            spacelessResult.StatusCode.Should().Be(200);
            spacedResult.StatusCode.Should().Be(200);
            spacelessResp.Should().BeEquivalentTo(expected);
            spacedResp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void BGShouldHandleSpannedCommaNotation()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Matthew 1:1-3, 5-7, 9-11")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Matthew 1:1-3, 5-7, 9-11 RSV",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
                    {
                        Title = "The Genealogy of Jesus the Messiah",
                        PsalmTitle = "",
                        Text = "<**1**> The book of the genealogy of Jesus Christ, the son of David, the son of Abraham. <**2**> Abraham was the father of Isaac, and Isaac the father of Jacob, and Jacob the father of Judah and his brothers, <**3**> and Judah the father of Perez and Zerah by Tamar, and Perez the father of Hezron, and Hezron the father of Ram, <**5**> and Salmon the father of Bo'az by Rahab, and Bo'az the father of Obed by Ruth, and Obed the father of Jesse, <**6**> and Jesse the father of David the king. And David was the father of Solomon by the wife of Uri'ah, <**7**> and Solomon the father of Rehobo'am, and Rehobo'am the father of Abi'jah, and Abi'jah the father of Asa, <**9**> and Uzzi'ah the father of Jotham, and Jotham the father of Ahaz, and Ahaz the father of Hezeki'ah, <**10**> and Hezeki'ah the father of Manas'seh, and Manas'seh the father of Amos, and Amos the father of Josi'ah, <**11**> and Josi'ah the father of Jechoni'ah and his brothers, at the time of the deportation to Babylon.",
                        Reference = new Reference
                        {
                            Book = "Matthew",
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 3,
                            AppendedVerses = [new System.Tuple<int, int>(5, 7), new System.Tuple<int, int>(9, 11)],
                            Version = _defaultBibleGatewayVersion,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "Matthew 1:1-3, 5-7, 9-11"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }
    }
}