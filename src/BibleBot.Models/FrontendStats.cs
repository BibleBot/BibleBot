/*
* Copyright (C) 2016-2026 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BibleBot.Models
{
    /// <summary>
    /// The model for frontend statistics.
    /// </summary>
    public class FrontendStats
    {
        /// <summary>
        /// The internal database ID.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        /// <summary>
        /// A shard is a logical process (or thread) of a Discord bot
        /// which, for bots that are in more than 2,500 guilds, handles
        /// a portion of guilds instead of one logical process handling
        /// them all. <seealso href="https://anidiots.guide/understanding/sharding/"/>
        /// </summary>
        [BsonElement("ShardCount")]
        public int ShardCount { get; set; }

        /// <summary>
        /// The amount of guilds the frontend is in.
        /// </summary>
        [BsonElement("ServerCount")]
        public int ServerCount { get; set; }

        /// <summary>
        /// The collective amount of users in each guild the frontend is in.
        /// </summary>
        /// <remarks>
        /// Discord provides only an estimate for each guild. We cannot get a perfectly
        /// accurate count without having the GUILD_MEMBERS Privileged Intent (see <seealso href="https://discord.com/developers/docs/topics/gateway#privileged-intents"/>).
        /// Therefore, this does not account for duplicate members.
        /// </remarks>
        [BsonElement("UserCount")]
        public int UserCount { get; set; }

        /// <summary>
        /// The collective amount of channels in each guild the frontend is in.
        /// </summary>
        [BsonElement("ChannelCount")]
        public int ChannelCount { get; set; }

        /// <summary>
        /// The approximate number of users who have installed the bot as a user app.
        /// </summary>
        [BsonElement("UserInstallCount")]
        public int UserInstallCount { get; set; }

        /// <summary>
        /// The current commit hash of the repo that frontend is running from.
        /// </summary>
        /// <remarks>
        /// This does not necessarily differ from backend's current commit hash, but there have been
        /// scenarios in the past where frontend runs from a different clone of the repo.
        /// </remarks>
        [BsonElement("FrontendRepoCommitHash")]
        public string FrontendRepoCommitHash { get; set; }
    }
}
