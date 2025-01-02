/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace BibleBot.Models
{
    /// <summary>
    /// This should thrown when no BibleProvider corresponds to a particular condition.
    /// </summary>
    public class ProviderNotFoundException : System.Exception
    {
        public ProviderNotFoundException() { }
        public ProviderNotFoundException(string message) : base(message) { }
        public ProviderNotFoundException(string message, System.Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// This is/was planned for usage with <see cref="Language"/>s, where this
    /// exception would be thrown if a particular string didn't exist.
    /// </summary>
    public class StringNotFoundException : System.Exception
    {
        public StringNotFoundException() { }
        public StringNotFoundException(string message) : base(message) { }
        public StringNotFoundException(string message, System.Exception inner) : base(message, inner) { }
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
