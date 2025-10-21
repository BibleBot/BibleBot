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
    /// This should be thrown when no BibleProvider corresponds to a particular condition.
    /// </summary>
    public class ProviderNotFoundException : System.Exception
    {
        public ProviderNotFoundException() { }
        public ProviderNotFoundException(string message) : base(message) { }
        public ProviderNotFoundException(string message, System.Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// This should be thrown when a limitation is encountered and should be made known to the user.
    /// </summary>
    public class VerseLimitationException : System.Exception
    {
        public VerseLimitationException() { }
        public VerseLimitationException(string message) : base(message) { }
        public VerseLimitationException(string message, System.Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// This should be thrown when a limitation is encountered and should be made known to the user.
    /// </summary>
    public class VersionUnavailableException : System.Exception
    {
        public VersionUnavailableException() { }
        public VersionUnavailableException(string message) : base(message) { }
        public VersionUnavailableException(string message, System.Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// This should be thrown when a verse range is illogical in sequence.
    /// </summary>
    public class VerseRangeInvalidException : System.Exception
    {
        public VerseRangeInvalidException() { }
        public VerseRangeInvalidException(string message) : base(message) { }
        public VerseRangeInvalidException(string message, System.Exception inner) : base(message, inner) { }
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
