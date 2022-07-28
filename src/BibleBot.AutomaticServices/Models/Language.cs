/*
* Copyright (C) 2016-2022 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace BibleBot.AutomaticServices.Models
{
    public class Language
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("Name")]
        public string Name { get; set; }

        [BsonElement("ObjectName")]
        public string ObjectName { get; set; }

        [BsonElement("DefaultVersion")]
        public string DefaultVersion { get; set; }

        [BsonElement("RawLanguage")]
        public RawLanguage RawLanguage { get; set; }

        public string GetString(string key)
        {
            try
            {
                return (string)RawLanguage.GetType().GetProperty(key).GetValue(RawLanguage, null);
            }
            catch
            {
                throw new StringNotFoundException();
            }
        }

        public string GetCommand(string key)
        {
            var commands = RawLanguage.GetType().GetProperty("Commands").GetValue(RawLanguage.Commands, null);
            var possibleCommand = (string)commands.GetType().GetProperty(key).GetValue(commands, null);

            if (possibleCommand == null)
            {
                throw new StringNotFoundException();
            }

            return possibleCommand;
        }

        public string GetArgument(string key)
        {
            var arguments = RawLanguage.GetType().GetProperty("Arguments").GetValue(RawLanguage.Arguments, null);
            var possibleArgument = (string)arguments.GetType().GetProperty(key).GetValue(arguments, null);

            if (possibleArgument == null)
            {
                throw new StringNotFoundException();
            }

            return possibleArgument;
        }

        public string GetCommandKey(string value)
        {
            var commands = RawLanguage.GetType().GetProperty("Commands").GetValue(RawLanguage.Commands, null);

            foreach (var possibleCommandKey in commands.GetType().GetProperties())
            {
                var commandValue = (string)possibleCommandKey.GetValue(commands);

                if (value == commandValue)
                {
                    return (string)possibleCommandKey.Name;
                }
            }

            throw new StringNotFoundException();
        }
    }

    public class RawLanguage
    {
        [BsonElement("BibleBot")]
        public string BibleBot { get; set; }

        [BsonElement("Credit")]
        public string Credit { get; set; }


        public RawCommands Commands { get; set; }
        public RawArguments Arguments { get; set; }
    }

    public class RawCommands
    {

    }

    public class RawArguments
    {

    }
}
