/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using BibleBot.Models;

namespace BibleBot.Backend.Tests.Mocks
{
    public class MockRequest : Request
    {
        public MockRequest(string body)
        {
            UserId = "000000";
            GuildId = "000000";
            IsDM = false;
            Body = body;
        }

        public MockRequest()
        {
            UserId = "000000";
            GuildId = "000000";
            IsDM = false;
            Body = "";
        }
    }
}