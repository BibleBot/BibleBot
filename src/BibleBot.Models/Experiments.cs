/*
* Copyright (C) 2016-2026 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;

namespace BibleBot.Models
{
    /// <summary>
    /// An experiment, used in A/B testing.
    /// </summary>
    public class Experiment
    {
        /// <summary>
        /// The name of the experiment.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The description of the experiment.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The variants of the experiment.
        /// </summary>
        public List<string> Variants { get; set; } = [];

        /// <summary>
        /// The weights of the variants.
        /// </summary>
        public List<int> Weights { get; set; } = [];


        /// <summary>
        /// The type of the experiment, e.g. "Backend" or "Frontend" or "Universal" or "AutoServ".
        /// </summary>
        public string Type { get; set; } = "Backend";

        /// <summary>
        /// The sphere of the experiment, e.g. "User" or "Guild" or "Universal".
        /// </summary>
        public string Sphere { get; set; } = "User";

        /// <summary>
        /// The feedback of the experiment.
        /// </summary>
        public ExperimentFeedback Feedback { get; set; }
    }

    /// <summary>
    /// Feedback for an experiment variant.
    /// </summary>
    public class ExperimentFeedback
    {
        /// <summary>
        /// Whether the user think the variant helped.
        /// </summary>
        public List<long> Helped { get; set; } = [];

        /// <summary>
        /// Whether the user think the variant did not help.
        /// </summary>
        public List<long> DidNotHelp { get; set; } = [];
    }
}
