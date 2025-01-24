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
    public class VersionCommandGroupTest : TestBaseClass
    {
        [Test]
        public async Task ShouldSetVersionWithLowercaseVersionAbbreviation()
        {
            ObjectResult result = (await _commandsController.ProcessMessage(new MockRequest("+version set rsv"))).Result as ObjectResult;
            CommandResponse resp = result.Value as CommandResponse;

            result.StatusCode.Should().Be(200);
            resp.OK.Should().BeTrue();
            resp.LogStatement.Should().Be("/setversion rsv");
            resp.Pages.Should().NotBeNullOrEmpty();
            resp.Pages[0].Description.Should().Be("Set version successfully.");
        }
    }
}