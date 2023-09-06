/*
* Copyright (C) 2016-2023 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;

namespace BibleBot.Models
{
    public class CommandResponse : IResponse
    {
        public bool OK { get; set; }
        public string LogStatement { get; set; }
        public string Type => "cmd";
        public List<InternalEmbed> Pages { get; set; }
        public bool CreateWebhook { get; set; }
        public bool RemoveWebhook { get; set; }
        public bool SendAnnouncement { get; set; }
    }
}
