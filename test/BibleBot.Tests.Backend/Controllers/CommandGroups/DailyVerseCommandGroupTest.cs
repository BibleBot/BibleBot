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
    public class DailyVerseCommandGroupTest : TestBaseClass
    {
        [Test]
        public async Task ShouldRejectSetupInDM()
        {
            MockRequest req = new("+dailyverse set 12:00 America/Detroit") { IsDM = true };
            ObjectResult result = (await _commandsController.ProcessMessage(req)).Result as ObjectResult;
            CommandResponse resp = result.Value as CommandResponse;

            result.StatusCode.Should().Be(400);
            resp.OK.Should().BeFalse();
            resp.LogStatement.Should().Be("/setdailyverse");
            resp.Pages.Should().NotBeNullOrEmpty();
            resp.Pages[0].Description.Should().Be("AutomaticDailyVerseNoDMs");
        }
    }
}