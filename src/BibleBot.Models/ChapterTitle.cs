/*
 * Copyright (C) 2016-2026 Kerygma Digital Co.
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BibleBot.Models
{
    /// <summary>
    /// A title heading for a chapter, indicating a section title that spans a range of verses.
    /// </summary>
    [JsonConverter(typeof(ChapterTitleConverter))]
    public class ChapterTitle
    {
        /// <summary>
        /// The beginning verse number where the title exists.
        /// </summary>
        public int StartVerse { get; set; }

        /// <summary>
        /// The ending verse number where the title exists.
        /// </summary>
        public int EndVerse { get; set; }

        /// <summary>
        /// The title text itself.
        /// </summary>
        public string Title { get; set; }
    }

    /// <summary>
    /// A JSON converter for <see cref="ChapterTitle"/> that can read both
    /// the legacy tuple-array format (<c>[1, 5, "Title"]</c>) and the
    /// new object format (<c>{"StartVerse":1, "EndVerse":5, "Title":"Title"}</c>).
    /// </summary>
    public class ChapterTitleConverter : JsonConverter<ChapterTitle>
    {
        /// <inheritdoc/>
        public override ChapterTitle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                // Legacy tuple-array format: [int, int, string]
                reader.Read();
                int startVerse = reader.GetInt32();
                reader.Read();
                int endVerse = reader.GetInt32();
                reader.Read();
                string title = reader.GetString();
                reader.Read(); // EndArray

                if (reader.TokenType != JsonTokenType.EndArray)
                {
                    throw new JsonException("Expected end of tuple array for ChapterTitle.");
                }

                return new ChapterTitle
                {
                    StartVerse = startVerse,
                    EndVerse = endVerse,
                    Title = title
                };
            }

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                // Object format: {"StartVerse": int, "EndVerse": int, "Title": string}
                // Also handles {"Item1": int, "Item2": int, "Item3": string} from
                // System.Text.Json serialization of System.Tuple.
                int startVerse = 0;
                int endVerse = 0;
                string title = null;

                while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                {
                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        string propertyName = reader.GetString();
                        reader.Read();

                        switch (propertyName)
                        {
                            case "StartVerse":
                            case "Item1":
                                startVerse = reader.GetInt32();
                                break;
                            case "EndVerse":
                            case "Item2":
                                endVerse = reader.GetInt32();
                                break;
                            case "Title":
                            case "Item3":
                                title = reader.GetString();
                                break;
                        }
                    }
                }

                return new ChapterTitle
                {
                    StartVerse = startVerse,
                    EndVerse = endVerse,
                    Title = title
                };
            }

            throw new JsonException($"Unexpected token {reader.TokenType} when reading ChapterTitle.");
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, ChapterTitle value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("StartVerse", value.StartVerse);
            writer.WriteNumber("EndVerse", value.EndVerse);
            writer.WriteString("Title", value.Title);
            writer.WriteEndObject();
        }
    }
}
