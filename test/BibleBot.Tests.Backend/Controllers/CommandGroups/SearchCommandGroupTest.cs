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

namespace BibleBot.Tests.Backend.Controllers.CommandGroups
{
    [TestFixture, Category("Commands")]
    public class SearchCommandGroupTest : TestBaseClass
    {
        [Test]
        public async Task ShouldProcessQueryWithLargeResults()
        {
            ObjectResult result = (await _commandsController.ProcessMessage(new MockRequest("+search faith"))).Result as ObjectResult;
            CommandResponse resp = result.Value as CommandResponse;

            result.StatusCode.Should().Be(200);
            resp.OK.Should().BeTrue();
            resp.LogStatement.Should().Be("/search faith");
            resp.Pages.Should().NotBeNullOrEmpty();


            resp.Pages[1].Description.Should().Contain("Page 2 of"); // Ensure correct order.
        }

        [Test]
        public async Task ShouldProcessQueryWithNoResults()
        {
            ObjectResult result = (await _commandsController.ProcessMessage(new MockRequest("+search aStringOfRandomNonsense"))).Result as ObjectResult;
            CommandResponse resp = result.Value as CommandResponse;

            result.StatusCode.Should().Be(400);
            resp.OK.Should().BeFalse();
            resp.LogStatement.Should().Be("/search");
            resp.Pages.Should().NotBeNullOrEmpty();

            resp.Pages[0].Description.Should().Be("Your search query produced no results."); // Ensure correct order.
        }

        [Test]
        public async Task ShouldNotAllowNonBibleGatewayVersionForSubsets()
        {
            ObjectResult result = (await _commandsController.ProcessMessage(new MockRequest("+search subset:2 version:KJVA Sadducees"))).Result as ObjectResult;
            CommandResponse resp = result.Value as CommandResponse;

            result.StatusCode.Should().Be(400);
            resp.OK.Should().BeFalse();
            resp.LogStatement.Should().Be("/search");
            resp.Pages.Should().NotBeNullOrEmpty();

            resp.Pages[0].Description.Should().Be("This version is not eligible for search subsets yet. Make sure you're using a version of source `bg` (see `/versioninfo`).");
        }

        [Test]
        public async Task ShouldNotAllowDeuterocanonSubsetInProtestantBible()
        {
            ObjectResult result = (await _commandsController.ProcessMessage(new MockRequest("+search subset:3 Sadducees"))).Result as ObjectResult;
            CommandResponse resp = result.Value as CommandResponse;

            result.StatusCode.Should().Be(400);
            resp.OK.Should().BeFalse();
            resp.LogStatement.Should().Be("/search");
            resp.Pages.Should().NotBeNullOrEmpty();

            resp.Pages[0].Description.Should().Be("Your search query produced no results. Does your version support the subset you are searching? (`/versioninfo`)");
        }
    }
}