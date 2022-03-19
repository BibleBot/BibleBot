/*
* Copyright (C) 2016-2021 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using BibleBot.Lib;

namespace BibleBot.Backend.Tests.Mocks
{
    public class MockRequest : Request
    {
        public MockRequest(string body)
        {
            this.UserId = "000000";
            this.GuildId = "000000";
            this.IsDM = false;
            this.Token = Environment.GetEnvironmentVariable("ENDPOINT_TOKEN");
            this.Body = body;
        }

        public MockRequest()
        {
            this.UserId = "000000";
            this.GuildId = "000000";
            this.IsDM = false;
            this.Token = Environment.GetEnvironmentVariable("ENDPOINT_TOKEN");
            this.Body = "";
        }
    }
}