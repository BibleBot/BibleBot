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
    public class VersionSpecificVersesTest : TestBaseClass
    {
        [Test]
        public async Task ShouldProcessDanielInEnglishSeptuagint()
        {
            Version testVersion = await _versionService.Get("ELXX") ?? await _versionService.Create(new MockELXX());

            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Daniel 1:1-2 ELXX")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result!.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Daniel 1:1-2 ELXX",
                DisplayStyle = "embed",
                Verses =
                [
                    new VerseResult
                    {
                        Title = "",
                        PsalmTitle = "",
                        Text = "<**1**> In the third year of the reign of Joakim king of Judah, came Nebuchadnezzar king of Babylon to Jerusalem, and besieged it. <**2**> And the Lord gave into his hand Joakim king of Judah, and part of the vessels of the house of God: and he brought them into the land of Shinar to the house of his god; and he brought the vessels into the treasure house of his god.",
                        Reference = new Reference
                        {
                            Book = new Book {
                                ProperName = "Daniel"
                            },
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
                ],
                Culture = "en-US",
                CultureFooter = $"BibleBot {Utils.Version} by Kerygma Digital"
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task ShouldProcessDanielInSeptuagint()
        {
            Version testVersion = await _versionService.Get("LXX") ?? await _versionService.Create(new MockLXX());

            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Daniel 1:1-2 LXX")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result!.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Daniel 1:1-2 LXX",
                DisplayStyle = "embed",
                Verses =
                [
                    new VerseResult
                    {
                        Title = "",
                        PsalmTitle = "",
                        Text = "<**1**> ἘΝ ἔτει τρίτῳ τῆς βασιλείας Ἰωακεὶμ βασιλέως Ἰούδα, ἦλθε Ναβουχοδονόσορ ὁ βασιλεὺς Βαβυλῶνος εἰς Ἱερουσαλὴμ, καὶ ἐπολιόρκει αὐτήν. <**2**> Καὶ ἔδωκε Κύριος ἐν χειρὶ αὐτοῦ τὸν Ἰωακεὶμ βασιλέα Ἰούδα, καὶ ἀπὸ μέρους τῶν σκευῶν οἴκου τοῦ Θεοῦ· καὶ ἤνεγκεν αὐτὰ εἰς γῆν Σενναὰρ οἴκου τοῦ θεοῦ αὐτοῦ, καὶ τὰ σκεύη εἰσήνεγκεν εἰς τὸν οἶκον θησαυροῦ τοῦ θεοῦ αὐτοῦ.",
                        Reference = new Reference
                        {
                            Book = new Book {
                                ProperName = "Daniel"
                            },
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
                ],
                Culture = "en-US",
                CultureFooter = $"BibleBot {Utils.Version} by Kerygma Digital"
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task ShouldProcessJohnInPatriarchalText()
        {
            Version testVersion = await _versionService.Get("PAT1904") ?? await _versionService.Create(new MockPAT1904());

            ObjectResult result = _versesController.ProcessMessage(new MockRequest("John 1:1-2 PAT1904")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result!.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "John 1:1-2 PAT1904",
                DisplayStyle = "embed",
                Verses =
                [
                    new VerseResult
                    {
                        Title = "",
                        PsalmTitle = "",
                        Text = "<**1**> Ἐν ἀρχῇ ἦν ὁ Λόγος, καὶ ὁ Λόγος ἦν πρὸς τὸν Θεόν, καὶ Θεὸς ἦν ὁ Λόγος. <**2**> Οὗτος ἦν ἐν ἀρχῇ πρὸς τὸν Θεόν.",
                        Reference = new Reference
                        {
                            Book = new Book {
                                ProperName = "John"
                            },
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 2,
                            Version = testVersion,
                            IsOT = false,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "John 1:1-2"
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
        public async Task ShouldNotMishandleMultipleSpansInProverbsInNIV()
        {
            Version testVersion = await _versionService.Get("NIV") ?? await _versionService.Create(new MockNIV());

            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Proverbs 25:1-12 NIV")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result!.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Proverbs 25:1-12 NIV",
                DisplayStyle = "embed",
                Verses =
                [
                    new VerseResult
                    {
                        Title = resp!.Verses[0]?.Title, // TODO: remove, is workaround with impending switch of source for NIV
                        PsalmTitle = "",
                        Text = "<**1**> These are more proverbs of Solomon, compiled by the men of Hezekiah king of Judah: <**2**> It is the glory of God to conceal a matter; to search out a matter is the glory of kings. <**3**> As the heavens are high and the earth is deep, so the hearts of kings are unsearchable. <**4**> Remove the dross from the silver, and a silversmith can produce a vessel; <**5**> remove wicked officials from the king's presence, and his throne will be established through righteousness. <**6**> Do not exalt yourself in the king's presence, and do not claim a place among his great men; <**7**> it is better for him to say to you, \"Come up here,\" than for him to humiliate you before his nobles. What you have seen with your eyes <**8**> do not bring hastily to court, for what will you do in the end if your neighbor puts you to shame? <**9**> If you take your neighbor to court, do not betray another's confidence, <**10**> or the one who hears it may shame you and the charge against you will stand. <**11**> Like apples of gold in settings of silver is a ruling rightly given. <**12**> Like an earring of gold or an ornament of fine gold is the rebuke of a wise judge to a listening ear.",
                        Reference = new Reference
                        {
                            Book = new Book {
                                ProperName = "Proverbs"
                            },
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
                ],
                Culture = "en-US",
                CultureFooter = $"BibleBot {Utils.Version} by Kerygma Digital"
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task ShouldWorkAroundHebrewVerseNumbersInExodusInISVPartOne()
        {
            Version testVersion = await _versionService.Get("ISV") ?? await _versionService.Create(new MockISV());

            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Exodus 20:1-7 ISV")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result!.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Exodus 20:1-7 ISV",
                DisplayStyle = "embed",
                Verses =
                [
                    new VerseResult
                    {
                        Title = "The Ten Commandments",
                        PsalmTitle = "",
                        Text = "<**1**> Then God spoke all these words: <**2**> \"I am the Lord your God, who brought you out of the land of Egypt— from the house of slavery. <**3**> You are to have no other gods as a substitute for me. <**4**> \"You are not to craft for yourselves an idol or anything resembling what is in the skies above, or on earth beneath, or in the water sources under the earth. <**5**> You are not to bow down to them in worship or serve them, because I, the Lord your God, am a jealous God, visiting the guilt of parents on children, to the third and fourth generation of those who hate me, <**6**> but showing gracious love to the thousands of those who love me and keep my commandments. <**7**> \"You are not to misuse the name of the Lord your God, because the Lord will not leave unpunished the one who misuses his name.",
                        Reference = new Reference
                        {
                            Book = new Book {
                                ProperName = "Exodus"
                            },
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
                ],
                Culture = "en-US",
                CultureFooter = $"BibleBot {Utils.Version} by Kerygma Digital"
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task ShouldWorkAroundHebrewVerseNumbersInExodusInISVPartTwo()
        {
            Version testVersion = await _versionService.Get("ISV") ?? await _versionService.Create(new MockISV());

            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Exodus 20:8-17 ISV")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result!.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Exodus 20:8-17 ISV",
                DisplayStyle = "embed",
                Verses =
                [
                    new VerseResult
                    {
                        Title = "",
                        PsalmTitle = "",
                        Text = "<**8**> \"Remember the Sabbath day, maintaining its holiness. <**9**> Six days you are to labor and do all your work, <**10**> but the seventh day is a Sabbath to the Lord your God. You are not to do any work—neither you, nor your son, nor your daughter, nor your male or female servant, nor your livestock, nor any foreigner who lives among you— <**11**> because the Lord made the heavens, the earth, the sea, and everything that is in them in six days. Then he rested on the seventh day. Therefore, the Lord blessed the Sabbath day and made it holy. <**12**> \"Honor your father and your mother, so that you may live long in the land that the Lord your God is giving you. <**13**> \"You are not to commit murder. <**14**> \"You are not to commit adultery. <**15**> \"You are not to steal. <**16**> \"You are not to give false testimony against your neighbor. <**17**> \"You are not to desire your neighbor's house, nor your neighbor's wife, his male or female servant, his ox, his donkey, nor anything else that pertains to your neighbor.\"",
                        Reference = new Reference
                        {
                            Book = new Book {
                                ProperName = "Exodus"
                            },
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
                ],
                Culture = "en-US",
                CultureFooter = $"BibleBot {Utils.Version} by Kerygma Digital"
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task ShouldHaveCorrectVerseNumbersInNIV()
        {
            Version testVersion = await _versionService.Get("NIV") ?? await _versionService.Create(new MockNIV());

            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Hebrews 11:1 NIV")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result!.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Hebrews 11:1 NIV",
                DisplayStyle = "embed",
                Verses =
                [
                    new VerseResult
                    {
                        Title = "Faith in Action",
                        PsalmTitle = "",
                        Text = "<**1**> Now faith is confidence in what we hope for and assurance about what we do not see.",
                        Reference = new Reference
                        {
                            Book = new Book {
                                ProperName = "Hebrews"
                            },
                            StartingChapter = 11,
                            StartingVerse = 1,
                            EndingChapter = 11,
                            EndingVerse = 1,
                            Version = testVersion,
                            IsOT = false,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "Hebrews 11:1"
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
        public async Task ShouldProcessWithVersionAlias()
        {
            Version testVersion = await _versionService.Get("NRSVA") ?? await _versionService.Create(new MockNRSVA());

            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Genesis 1:1 NRSV")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result!.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Genesis 1:1 NRSVA",
                DisplayStyle = "embed",
                Verses =
                [
                    new VerseResult
                    {
                        Title = "Six Days of Creation and the Sabbath",
                        PsalmTitle = "",
                        Text = "<**1**> In the beginning when God created the heavens and the earth,",
                        Reference = new Reference
                        {
                            Book = new Book {
                                ProperName = "Genesis"
                            },
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 1,
                            Version = testVersion,
                            IsOT = true,
                            IsNT = false,
                            IsDEU = false,
                            AsString = "Genesis 1:1"
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
