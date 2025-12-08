/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BibleBot.Models
{
    /// <summary>
    /// A convenient way to distinguish sections of the Bible
    /// without hardcoded strings.
    /// </summary>
    public enum BookCategories
    {
        /// <summary>
        /// An Old Testament book, more specifically the whole OT from a Protestant POV.
        /// </summary>
        OldTestament,

        /// <summary>
        /// A New Testament book.
        /// </summary>
        NewTestament,

        /// <summary>
        /// A Deuterocanonical book, found standard in the OT canon of Apostolic churches.
        /// </summary>
        Deuterocanon
    }

    /// <summary>
    /// An experiment, used in A/B testing.
    /// </summary>
    public class Experiment
    {
        /// <summary>
        /// The internal database ID.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        /// <summary>
        /// The name of the experiment.
        /// </summary>
        [BsonElement("Name")]
        public string Name { get; set; }

        /// <summary>
        /// The description of the experiment.
        /// </summary>
        [BsonElement("Description")]
        public string Description { get; set; }

        /// <summary>
        /// The variants of the experiment.
        /// </summary>
        [BsonElement("Variants")]
        public List<string> Variants { get; set; }

        /// <summary>
        /// The weights of the variants.
        /// </summary>
        [BsonElement("Weights")]
        public List<int> Weights { get; set; } = [50, 50];

        /// <summary>
        /// The type of the experiment, e.g. "Backend" or "Frontend" or "Universal" or "AutoServ".
        /// </summary>
        [BsonElement("Type")]
        public string Type { get; set; } = "Backend";

        /// <summary>
        /// The sphere of the experiment, e.g. "User" or "Guild" or "Universal".
        /// </summary>
        [BsonElement("Sphere")]
        public string Sphere { get; set; } = "User";

        /// <summary>
        /// The users who found the experiment helpful.
        /// </summary>
        [BsonElement("Helped")]
        public HashSet<string> Helped { get; set; } = [];

        /// <summary>
        /// The users who found the experiment unhelpful.
        /// </summary>
        [BsonElement("DidNotHelp")]
        public HashSet<string> DidNotHelp { get; set; } = [];
    }
}