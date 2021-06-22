/*
* Copyright (C) 2016-2021 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Text.Json.Serialization;

namespace BibleBot.Lib
{
    public class Request
    {
        [JsonPropertyName("userId")]
        public string UserId { get; set; }

        [JsonPropertyName("userPermissions")]
        public long UserPermissions { get; set; }

        [JsonPropertyName("guildId")]
        public string GuildId { get; set; }

        [JsonPropertyName("isDM")]
        public bool IsDM { get; set; }

        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("body")]
        public string Body { get; set; }
    }
}
