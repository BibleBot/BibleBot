/*
* Copyright (C) 2016-2021 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using System.Threading.Tasks;
using BibleBot.Backend.Services;
using BibleBot.Lib;
using Microsoft.Extensions.DependencyInjection;

namespace BibleBot.Backend.Models
{
    public interface ICommand
    {
        string Name { get; set; }
        string ArgumentsError { get; set; }
        int ExpectedArguments { get; set; }
        List<Permissions> PermissionsRequired { get; set; }
        bool BotAllowed { get; set; }
        IResponse ProcessCommand(Request req, List<string> args);
    }
}
