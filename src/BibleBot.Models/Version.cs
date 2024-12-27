/*
* Copyright (C) 2016-2024 Kerygma Digital Co.
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
    /// The model for Bible versions.
    /// </summary>
    public class Version
    {
        /// <summary>
        /// The internal database ID.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        /// <summary>
        /// The name of the version.
        /// </summary>
        [BsonElement("Name")]
        public string Name { get; set; }

        /// <summary>
        /// The abbreviated name of the version.
        /// </summary>
        /// <remarks>
        /// In hindsight, maybe this should have been "Acronym" given that's what most of these are
        /// but this was a decision made by December 2016 Seraphim, and we don't question December 2016 Seraphim.
        /// </remarks>
        [BsonElement("Abbreviation")]
        public string Abbreviation { get; set; }

        /// <summary>
        /// The source of the version, correlating to a <see cref="IBibleProvider.Name"/>.
        /// </summary>
        [BsonElement("Source")]
        public string Source { get; set; }

        /// <summary>
        /// The publisher of the version.
        /// </summary>
        /// <remarks>
        /// This is currently only planned for use in the frontend to fulfill license agreement obligations.
        /// </remarks>
        [BsonElement("Publisher")]
        public string Publisher { get; set; }

        /// <summary>
        /// Indicates whether the version supports Old Testament books.
        /// </summary>
        [BsonElement("SupportsOldTestament")]
        public bool SupportsOldTestament { get; set; }

        /// <summary>
        /// Indicates whether the version supports New Testament books.
        /// </summary>
        [BsonElement("SupportsNewTestament")]
        public bool SupportsNewTestament { get; set; }

        /// <summary>
        /// Indicates whether the version supports Deuterocanon books.
        /// </summary>
        [BsonElement("SupportsDeuterocanon")]
        public bool SupportsDeuterocanon { get; set; }
    }
}
