/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using BibleBot.Models;
using BibleBot.Tests.Backend.Mocks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace BibleBot.Tests.Backend.Controllers
{
    [TestFixture, Category("Verses")]
    public class VersesControllerTest : TestBaseClass
    {
        [Test]
        public void ShouldFailWhenBodyIsEmpty()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest()).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result!.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = false,
                LogStatement = null,
                Culture = "en-US"
            };

            result.StatusCode.Should().Be(400);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldNotProcessInvalidVerse()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Genesis 1:125")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result!.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = false,
                LogStatement = null,
                Culture = "en-US"
            };

            result.StatusCode.Should().Be(400);
            resp.Should().BeEquivalentTo(expected);
        }
    }
}
