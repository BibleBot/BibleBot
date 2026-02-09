/*
* Copyright (C) 2016-2026 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Threading.Tasks;
using BibleBot.Backend;
using BibleBot.Models;
using BibleBot.Tests.Backend.Mocks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace BibleBot.Tests.Backend.Controllers.Verses
{
    [TestFixture, Category("Verses")]
    public class BookNameOverlapTest : TestBaseClass
    {
        [Test]
        public void ShouldProcessVerseWithDataNameSGTHR()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Song of the Three Young Men 1:1")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result!.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Song of the Three Young Men 1:1 RSV",
                DisplayStyle = "embed",
                Verses =
                [
                    new VerseResult
                    {
                        Title = "The Prayer of Azariah in the Furnace",
                        PsalmTitle = "",
                        Text = "<**1**> And they walked about in the midst of the flames, singing hymns to God and blessing the Lord.",
                        Reference = new Reference
                        {
                            Book = new Book {
                                ProperName = "Song of the Three Young Men"
                            },
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 1,
                            Version = _defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = false,
                            IsDEU = true,
                            AsString = "Song of the Three Young Men 1:1"
                        }
                    }
                ],
                Culture = "en-US",
                CultureFooter = $"BibleBot {Utils.Version} by Kerygma Digital"
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task ShouldProcessVerseWithActualDataNameSGTHREE()
        {
            Version testVersion = await _versionService.Get("WYC") ?? await _versionService.Create(new MockWYC());

            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Song of the Three Young Men 1:1 WYC")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result!.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Song of the Three Young Men 1:1 WYC",
                DisplayStyle = "embed",
                Verses =
                [
                    new VerseResult
                    {
                        Title = "",
                        PsalmTitle = "",
                        Text = "<**1**> And they walked in the midst of the flame, and praised God, and blessed the Lord. [And they walked in the middle of the flame, praising God, and blessing the Lord.]",
                        Reference = new Reference
                        {
                            Book = new Book {
                                ProperName = "Song of the Three Young Men"
                            },
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 1,
                            Version = testVersion,
                            IsOT = false,
                            IsNT = false,
                            IsDEU = true,
                            AsString = "Song of the Three Young Men 1:1"
                        }
                    }
                ],
                Culture = "en-US",
                CultureFooter = $"BibleBot {Utils.Version} by Kerygma Digital"
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task ShouldProcessVerseWithDataNamePRAZ()
        {
            Version testVersion = await _versionService.Get("NRSVA") ?? await _versionService.Create(new MockNRSVA());

            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Prayer of Azariah 1:1 NRSVA")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result!.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Prayer of Azariah 1:1 NRSVA",
                DisplayStyle = "embed",
                Verses =
                [
                    new VerseResult
                    {
                        Title = "The Prayer of Azariah in the Furnace",
                        PsalmTitle = "(Additions to Daniel, inserted between 3.23 and 3.24)",
                        Text = "<**1**> They walked around in the midst of the flames, singing hymns to God and blessing the Lord.",
                        Reference = new Reference
                        {
                            Book = new Book {
                                ProperName = "Prayer of Azariah"
                            },
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 1,
                            Version = testVersion,
                            IsOT = false,
                            IsNT = false,
                            IsDEU = true,
                            AsString = "Prayer of Azariah 1:1"
                        }
                    }
                ],
                Culture = "en-US",
                CultureFooter = $"BibleBot {Utils.Version} by Kerygma Digital"
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task ShouldProcessVerseWithActualDataNamePRAZAR()
        {
            Version testVersion = await _versionService.Get("CEB") ?? await _versionService.Create(new MockCEB());

            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Prayer of Azariah 1:1 CEB")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result!.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Prayer of Azariah 1:1 CEB",
                DisplayStyle = "embed",
                Verses =
                [
                    new VerseResult
                    {
                        Title = "Azariah's prayer for reconciliation",
                        PsalmTitle = "",
                        Text = "<**1**> Shadrach, Meshach, and Abednego walked around in the flames, singing hymns to God, blessing the Lord.",
                        Reference = new Reference
                        {
                            Book = new Book {
                                ProperName = "Prayer of Azariah"
                            },
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 1,
                            Version = testVersion,
                            IsOT = false,
                            IsNT = false,
                            IsDEU = true,
                            AsString = "Prayer of Azariah 1:1"
                        }
                    }
                ],
                Culture = "en-US",
                CultureFooter = $"BibleBot {Utils.Version} by Kerygma Digital"
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task ShouldProcessVerseWithDataNameADDESTH()
        {
            Version testVersion = await _versionService.Get("WYC") ?? await _versionService.Create(new MockWYC());

            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Additions to Esther 10:4 WYC")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result!.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Additions to Esther 10:4 WYC",
                DisplayStyle = "embed",
                Verses =
                [
                    new VerseResult
                    {
                        Title = "",
                        PsalmTitle = "",
                        Text = "<**4**> And Mordecai said, These things be done of (or by) God. [And Mordecai said, Of God these things be done.]",
                        Reference = new Reference
                        {
                            Book = new Book {
                                ProperName = "Additions to Esther"
                            },
                            StartingChapter = 10,
                            StartingVerse = 4,
                            EndingChapter = 10,
                            EndingVerse = 4,
                            Version = testVersion,
                            IsOT = false,
                            IsNT = false,
                            IsDEU = true,
                            AsString = "Additions to Esther 10:4"
                        }
                    }
                ],
                Culture = "en-US",
                CultureFooter = $"BibleBot {Utils.Version} by Kerygma Digital"
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldNotProcessGreekEstherAsEsther()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Greek Esther 1:1")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result!.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Greek Esther 1:1 RSV",
                DisplayStyle = "embed",
                Verses =
                [
                    new VerseResult
                    {
                        Title = "King Ahasu-erus Deposes Queen Vashti",
                        PsalmTitle = "",
                        Text = "<**1**> In the days of Ahasu-e'rus, the Ahasu-e'rus who reigned from India to Ethiopia over one hundred and twenty-seven provinces,",
                        Reference = new Reference
                        {
                            Book = new Book {
                                ProperName = "Greek Esther"
                            },
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
                ],
                Culture = "en-US",
                CultureFooter = $"BibleBot {Utils.Version} by Kerygma Digital"
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldNotProcessJohannineEpistlesAsJohn()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("1 John 1:1 KJV")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result!.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "1 John 1:1 KJV",
                DisplayStyle = "embed",
                Verses =
                [
                    new VerseResult
                    {
                        Title = "",
                        PsalmTitle = "",
                        Text = "<**1**> That which was from the beginning, which we have heard, which we have seen with our eyes, which we have looked upon, and our hands have handled, of the Word of life;",
                        Reference = new Reference
                        {
                            Book = new Book {
                                ProperName = "1 John"
                            },
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 1,
                            Version = _defaultAPIBibleVersion,
                            IsOT = false,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "1 John 1:1"
                        }
                    }
                ],
                Culture = "en-US",
                CultureFooter = $"BibleBot {Utils.Version} by Kerygma Digital"
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldNotProcessEsdrasBooksAsEzra()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("1 Esdras 1:1")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result!.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "1 Esdras 1:1 RSV",
                DisplayStyle = "embed",
                Verses =
                [
                    new VerseResult
                    {
                        Title = "Josiah Celebrates the Passover",
                        PsalmTitle = "",
                        Text = "<**1**> Josi'ah kept the passover to his Lord in Jerusalem; he killed the passover lamb on the fourteenth day of the first month,",
                        Reference = new Reference
                        {
                            Book = new Book {
                                ProperName = "1 Esdras"
                            },
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
                ],
                Culture = "en-US",
                CultureFooter = $"BibleBot {Utils.Version} by Kerygma Digital"
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldNotProcessLetterOfJeremiahAsJeremiah()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Letter of Jeremiah 1:1")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result!.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Letter of Jeremiah 1:1 RSV",
                DisplayStyle = "embed",
                Verses =
                [
                    new VerseResult
                    {
                        Title = "",
                        PsalmTitle = "",
                        Text = "<**6:1**> A copy of a letter which Jeremiah sent to those who were to be taken to Babylon as captives by the king of the Babylonians, to give them the message which God had commanded him.",
                        Reference = new Reference
                        {
                            Book = new Book {
                                ProperName = "Letter of Jeremiah"
                            },
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
                ],
                Culture = "en-US",
                CultureFooter = $"BibleBot {Utils.Version} by Kerygma Digital"
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessJeremiah()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Jeremiah 1:1")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result!.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Jeremiah 1:1 RSV",
                DisplayStyle = "embed",
                Verses =
                [
                    new VerseResult
                    {
                        Title = "",
                        PsalmTitle = "",
                        Text = "<**1**> The words of Jeremiah, the son of Hilki'ah, of the priests who were in An'athoth in the land of Benjamin,",
                        Reference = new Reference
                        {
                            Book = new Book {
                                ProperName = "Jeremiah"
                            },
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
                ],
                Culture = "en-US",
                CultureFooter = $"BibleBot {Utils.Version} by Kerygma Digital"
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessMultipleOverlappingBooks()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("1 Esdras 1:1 / Ezra 1:1 / Letter of Jeremiah 1:1 / Jeremiah 1:1")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result!.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "1 Esdras 1:1 RSV / Ezra 1:1 RSV / Letter of Jeremiah 1:1 RSV / Jeremiah 1:1 RSV",
                DisplayStyle = "embed",
                Verses =
                [
                    new VerseResult
                    {
                        Title = "Josiah Celebrates the Passover",
                        PsalmTitle = "",
                        Text = "<**1**> Josi'ah kept the passover to his Lord in Jerusalem; he killed the passover lamb on the fourteenth day of the first month,",
                        Reference = new Reference
                        {
                            Book = new Book {
                                ProperName = "1 Esdras"
                            },
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
                    },
                    new VerseResult
                    {
                        Title = "End of the Babylonian Captivity",
                        PsalmTitle = "",
                        Text = "<**1**> In the first year of Cyrus king of Persia, that the word of the Lord by the mouth of Jeremiah might be accomplished, the Lord stirred up the spirit of Cyrus king of Persia so that he made a proclamation throughout all his kingdom and also put it in writing:",
                        Reference = new Reference
                        {
                            Book = new Book {
                                ProperName = "Ezra"
                            },
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 1,
                            Version = _defaultBibleGatewayVersion,
                            IsOT = true,
                            IsNT = false,
                            IsDEU = false,
                            AsString = "Ezra 1:1"
                        }
                    },
                    new VerseResult
                    {
                        Title = "",
                        PsalmTitle = "",
                        Text = "<**6:1**> A copy of a letter which Jeremiah sent to those who were to be taken to Babylon as captives by the king of the Babylonians, to give them the message which God had commanded him.",
                        Reference = new Reference
                        {
                            Book = new Book {
                                ProperName = "Letter of Jeremiah"
                            },
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 1,
                            Version = _defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = false,
                            IsDEU = true,
                            AsString = "Letter of Jeremiah 1:1"
                        }
                    },
                    new VerseResult
                    {
                        Title = "",
                        PsalmTitle = "",
                        Text = "<**1**> The words of Jeremiah, the son of Hilki'ah, of the priests who were in An'athoth in the land of Benjamin,",
                        Reference = new Reference
                        {
                            Book = new Book {
                                ProperName = "Jeremiah"
                            },
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
                ],
                Culture = "en-US",
                CultureFooter = $"BibleBot {Utils.Version} by Kerygma Digital"
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessPsalm151Properly()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Psalm 151:1")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result!.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Psalm 151:1 RSV",
                DisplayStyle = "embed",
                Verses =
                [
                    new VerseResult
                    {
                        Title = "",
                        PsalmTitle = "This psalm is ascribed to David as his own composition (though it is outside the number), after he had fought in single combat with Goliath.",
                        Text = "<**1**> I was small among my brothers, and youngest in my father's house; I tended my father's sheep.",
                        Reference = new Reference
                        {
                            Book = new Book {
                                ProperName = "Psalm 151"
                            },
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
                ],
                Culture = "en-US",
                CultureFooter = $"BibleBot {Utils.Version} by Kerygma Digital"
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }
    }
}
