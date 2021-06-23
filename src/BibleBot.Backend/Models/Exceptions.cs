/*
* Copyright (C) 2016-2021 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

namespace BibleBot.Backend.Models
{
    [System.Serializable]
    public class ProviderNotFoundException : System.Exception
    {
        public ProviderNotFoundException() { }
        public ProviderNotFoundException(string message) : base(message) { }
        public ProviderNotFoundException(string message, System.Exception inner) : base(message, inner) { }
        protected ProviderNotFoundException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [System.Serializable]
    public class StringNotFoundException : System.Exception
    {
        public StringNotFoundException() { }
        public StringNotFoundException(string message) : base(message) { }
        public StringNotFoundException(string message, System.Exception inner) : base(message, inner) { }
        protected StringNotFoundException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
