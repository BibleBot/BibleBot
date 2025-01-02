/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;

namespace BibleBot.Models
{
    /// <summary>
    /// The model for standard command responses.
    /// </summary>
    public class CommandResponse : IResponse
    {
        /// <inheritdoc/>
        public bool OK { get; set; }
        /// <inheritdoc/>
        public string LogStatement { get; set; }
        /// <inheritdoc/>
        public string Type => "cmd";

        /// <summary>
        /// The content of the response.
        /// </summary>
        public List<InternalEmbed> Pages { get; set; }

        /// <summary>
        /// Indicates whether frontend should create a new webhook and inform the backend of the webhook's URL.
        /// </summary>
        public bool CreateWebhook { get; set; }

        /// <summary>
        /// Indicates whether frontend should remove all BibleBot-created webhooks.
        /// </summary>
        /// <remarks>
        /// If both <see cref="CreateWebhook"/> and this property are true, frontend will remove existing webhooks first.
        /// </remarks>
        public bool RemoveWebhook { get; set; }

        /// <summary>
        /// Indicates whether the contents of <see cref="Pages"/> are meant to be a bot-wide announcement.
        /// </summary>
        public bool SendAnnouncement { get; set; }
    }
}
