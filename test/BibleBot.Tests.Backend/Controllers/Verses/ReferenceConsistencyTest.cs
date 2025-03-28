// Copyright (C) 2016-2025 Kerygma Digital Co.
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this file,
// You can obtain one at https://mozilla.org/MPL/2.0/.

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
    public class ReferenceConsistencyTest : TestBaseClass
    {
        [Test]
        public async Task ShouldHaveConsistentReferenceInNLD1939PartOne()
        {
            ObjectResult result = (await _versesController.ProcessMessage(new MockRequest("Gen 14:15-15:1 NLD1939"))).Result as ObjectResult;
            VerseResponse resp = result!.Value as VerseResponse;

            const string expectedAsString = "Genesis 14:15-15:1";

            result.StatusCode.Should().Be(200);
            resp!.Verses.Should().HaveCount(1);
            resp!.Verses[0].Reference.AsString.Should().Be(expectedAsString);
        }

        [Test]
        public async Task ShouldHaveConsistentReferenceInNLD1939PartTwo()
        {
            ObjectResult result = (await _versesController.ProcessMessage(new MockRequest("Lev 1:2-2:2 NLD1939"))).Result as ObjectResult;
            VerseResponse resp = result!.Value as VerseResponse;

            const string expectedAsString = "Leviticus 1:2-2:2";

            result.StatusCode.Should().Be(200);
            resp!.Verses.Should().HaveCount(1);
            resp!.Verses[0].Reference.AsString.Should().Be(expectedAsString);
        }

        [Test]
        public async Task ShouldHaveConsistentReferenceInRSVPartOne()
        {
            ObjectResult result = (await _versesController.ProcessMessage(new MockRequest("Gen 14:15-15:1 RSV"))).Result as ObjectResult;
            VerseResponse resp = result!.Value as VerseResponse;

            const string expectedAsString = "Genesis 14:15-15:1";

            result.StatusCode.Should().Be(200);
            resp!.Verses.Should().HaveCount(1);
            resp!.Verses[0].Reference.AsString.Should().Be(expectedAsString);
        }

        [Test]
        public async Task ShouldHaveConsistentReferenceInRSVPartTwo()
        {
            ObjectResult result = (await _versesController.ProcessMessage(new MockRequest("Lev 1:2-2:2 RSV"))).Result as ObjectResult;
            VerseResponse resp = result!.Value as VerseResponse;

            const string expectedAsString = "Leviticus 1:2-2:2";

            result.StatusCode.Should().Be(200);
            resp!.Verses.Should().HaveCount(1);
            resp!.Verses[0].Reference.AsString.Should().Be(expectedAsString);
        }
    }
}
