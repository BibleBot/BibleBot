/*
 * Copyright (C) 2016-2025 Kerygma Digital Co.
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
    /// The model for user preferences.
    /// </summary>
    [BsonIgnoreExtraElements]
    public class User : IPreference
    {
        /// <summary>
        /// The internal database ID.
        /// <br/><br/>
        /// <b>DO NOT USE THIS AS IF IT IS THE DISCORD ID OF THE USER.</b>
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        /// <summary>
        /// The Discord Snowflake identifier of the user.
        /// </summary>
        [BsonElement("UserId")]
        public string UserId { get; init; }

        /// <summary>
        /// The default input method of the user.
        /// </summary>
        [BsonElement("InputMethod")]
        public string InputMethod { get; init; } = "default";

        /// <summary>
        /// The default language of the user.
        /// </summary>
        [BsonElement("Language")]
        public string Language { get; set; }

        /// <summary>
        /// The default version of the user.
        /// </summary>
        [BsonElement("Version")]
        public string Version { get; set; }

        /// <summary>
        /// Indicates whether the user prefers titles and other headings to be displayed in verse results.
        /// </summary>
        [BsonElement("TitlesEnabled")]
        public bool TitlesEnabled { get; init; } = true;

        /// <summary>
        /// Indicates whether the user prefers chapter and verse numbers to be displayed in verse results.
        /// </summary>
        [BsonElement("VerseNumbersEnabled")]
        public bool VerseNumbersEnabled { get; set; } = true;

        /// <summary>
        /// Indicates whether the user prefers pagination when multiple verse results are sent in one response.
        /// </summary>
        [BsonElement("PaginationEnabled")]
        public bool PaginationEnabled { get; set; }

        /// <summary>
        /// The default display style for verses from the user.
        /// </summary>
        [BsonElement("DisplayStyle")]
        public string DisplayStyle { get; set; } = "embed";
    }
}
