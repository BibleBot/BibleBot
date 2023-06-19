/*
* Copyright (C) 2016-2023 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using System.Threading.Tasks;

namespace BibleBot.Backend.Models
{
    public interface ICommandGroup
    {
        string Name { get; set; }
        bool IsOwnerOnly { get; set; }
        List<ICommandable> Commands { get; set; }
        ICommandable DefaultCommand { get; set; }
    }
}
