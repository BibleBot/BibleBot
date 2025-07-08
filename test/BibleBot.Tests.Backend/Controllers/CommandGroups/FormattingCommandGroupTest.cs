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
    public class FormattingCommandGroupTest : TestBaseClass
    {
        [Test]
        public async Task ShouldFailWithInvalidBracketLength()
        {
            ObjectResult result = (await _commandsController.ProcessMessage(new MockRequest("+formatting setbrackets abc"))).Result as ObjectResult;
            CommandResponse resp = result.Value as CommandResponse;

            result.StatusCode.Should().Be(400);
            resp.OK.Should().BeFalse();
            resp.LogStatement.Should().Be("/setbrackets");
            resp.Pages.Should().NotBeNullOrEmpty();
            resp.Pages[0].Description.Should().Be("The brackets can only be two characters.");
        }

        [Test]
        public async Task ShouldFailWithInvalidDisplayStyle()
        {
            ObjectResult result = (await _commandsController.ProcessMessage(new MockRequest("+formatting setdisplay invalid"))).Result as ObjectResult;
            CommandResponse resp = result.Value as CommandResponse;

            result.StatusCode.Should().Be(400);
            resp.OK.Should().BeFalse();
            resp.LogStatement.Should().Be("/setdisplay");
            resp.Pages.Should().NotBeNullOrEmpty();
            resp.Pages[0].Description.Should().Be("You may choose between `embed`, `code`, or `blockquote`.");
        }
    }
}