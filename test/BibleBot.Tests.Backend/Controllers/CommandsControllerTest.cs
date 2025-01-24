/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using BibleBot.Tests.Backend.Mocks;
using BibleBot.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using System.Threading.Tasks;

namespace BibleBot.Tests.Backend.Controllers
{
    [TestFixture, Category("Commands")]
    public class CommandsControllerTest : TestBaseClass
    {
        [Test]
        public async Task ShouldFailWhenBodyIsEmpty()
        {
            ObjectResult result = (await _commandsController.ProcessMessage(new MockRequest())).Result as ObjectResult;
            CommandResponse resp = result.Value as CommandResponse;

            CommandResponse expected = new()
            {
                OK = false,
                LogStatement = null,
                Pages = null,
                CreateWebhook = false,
                RemoveWebhook = false,
                SendAnnouncement = false
            };

            result.StatusCode.Should().Be(400);
            resp.Should().BeEquivalentTo(expected);
        }
    }
}