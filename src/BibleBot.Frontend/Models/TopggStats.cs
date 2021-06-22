/*
* Copyright (C) 2016-2021 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BibleBot.Frontend.Models
{
    public class TopggStats
    {
        [JsonPropertyName("server_count")]
        public int ServerCount { get; set; }

        // Until DSharpPlus implements a way to see how many guilds are in
        // an individual shard, these are useless.
        /*[JsonPropertyName("shards")]
        public List<int> Shards { get; set; }

        [JsonPropertyName("shard_id")]
        public int ShardID { get; set; }

        [JsonPropertyName("shard_count")]
        public int ShardCount { get; set; }*/
    }
}