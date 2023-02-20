/*
* Copyright (C) 2016-2023 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

namespace BibleBot.Backend.Models
{
    public interface IResponse
    {
        bool OK { get; set; }
        string LogStatement { get; set; }
        string Type { get; }
    }
}
