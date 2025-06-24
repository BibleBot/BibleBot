/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using BibleBot.Models;

namespace BibleBot.Backend.Services
{

    public class ResourceService
    {
        private readonly Dictionary<string, Tuple<ResourceStyle, string>> _canonData;
        private readonly Dictionary<string, Tuple<ResourceStyle, string>> _catechismData;
        private readonly List<string> _creeds;

        private readonly List<IResource> _resources;

        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public ResourceService()
        {
            _canonData = new Dictionary<string, Tuple<ResourceStyle, string>>
            {
                { "cic", new Tuple<ResourceStyle, string>(ResourceStyle.PARAGRAPHED, "code_of_canon_law") },
                { "cceo" , new Tuple<ResourceStyle, string>(ResourceStyle.PARAGRAPHED,  "eastern_code") }
            };

            _catechismData = new Dictionary<string, Tuple<ResourceStyle, string>>
            {
                { "ccc", new Tuple<ResourceStyle, string>(ResourceStyle.PARAGRAPHED, "catechism_of_the_catholic_church") },
                { "lsc" , new Tuple<ResourceStyle, string>(ResourceStyle.SECTIONED,  "luthers_small_catechism") }
            };

            _creeds =
            [
                "apostles",
                "nicene325",
                "nicene",
                "chalcedon"
            ];

            _resources = [];

            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            foreach (KeyValuePair<string, Tuple<ResourceStyle, string>> canonData in _canonData)
            {
                _resources.Add(GetResource(ResourceType.CANONS, canonData.Key));
            }

            foreach (KeyValuePair<string, Tuple<ResourceStyle, string>> catechismData in _catechismData)
            {
                _resources.Add(GetResource(ResourceType.CATECHISM, catechismData.Key));
            }

            foreach (string creed in _creeds)
            {
                _resources.Add(GetResource(ResourceType.CREED, creed));
            }
        }

        public List<IResource> GetAllResources() => _resources;

        private IResource GetResource(ResourceType type, string name)
        {

            switch (type)
            {
                case ResourceType.CREED when _creeds.Contains(name):
                    {
                        string creedFile = File.ReadAllText($"./Data/Creeds/english.json");

                        CreedFile creedFileObj = JsonSerializer.Deserialize<CreedFile>(creedFile, _jsonSerializerOptions);
                        CreedResource resource = name switch
                        {
                            "apostles" => creedFileObj.Apostles,
                            "nicene325" => creedFileObj.Nicene325,
                            "nicene" => creedFileObj.Nicene,
                            "chalcedon" => creedFileObj.Chalcedon,
                            _ => throw new KeyNotFoundException(),
                        };

                        resource.CommandReference = name;
                        resource.Type = ResourceType.CREED;
                        resource.Style = ResourceStyle.FULL_TEXT;

                        return resource;
                    }
                case ResourceType.CATECHISM:
                    {
                        if (_catechismData.TryGetValue(name, out Tuple<ResourceStyle, string> idealCatechism))
                        {
                            string catechismFile = File.ReadAllText($"./Data/Catechisms/{idealCatechism.Item2}.json");

                            switch (idealCatechism.Item1)
                            {
                                case ResourceStyle.PARAGRAPHED:
                                    {
                                        ParagraphedResource resource = JsonSerializer.Deserialize<ParagraphedResource>(catechismFile, _jsonSerializerOptions);
                                        resource.CommandReference = name;
                                        resource.Type = ResourceType.CATECHISM;
                                        resource.Style = ResourceStyle.PARAGRAPHED;
                                        return resource;
                                    }
                                case ResourceStyle.SECTIONED:
                                    {
                                        SectionedResource resource = JsonSerializer.Deserialize<SectionedResource>(catechismFile, _jsonSerializerOptions);
                                        resource.CommandReference = name;
                                        resource.Type = ResourceType.CATECHISM;
                                        resource.Style = ResourceStyle.SECTIONED;
                                        return resource;
                                    }
                                case ResourceStyle.FULL_TEXT:
                                default:
                                    throw new NotImplementedException();
                            }
                        }
                        break;
                    }
                case ResourceType.CANONS:
                    {
                        if (_canonData.TryGetValue(name, out Tuple<ResourceStyle, string> idealCanons))
                        {
                            string canonsFile = File.ReadAllText($"./Data/Canons/{idealCanons.Item2}.json");

                            switch (idealCanons.Item1)
                            {
                                case ResourceStyle.PARAGRAPHED:
                                    {
                                        ParagraphedResource resource = JsonSerializer.Deserialize<ParagraphedResource>(canonsFile, _jsonSerializerOptions);
                                        resource.CommandReference = name;
                                        resource.Type = ResourceType.CANONS;
                                        resource.Style = ResourceStyle.PARAGRAPHED;
                                        return resource;
                                    }
                                case ResourceStyle.SECTIONED:
                                    {
                                        SectionedResource resource = JsonSerializer.Deserialize<SectionedResource>(canonsFile, _jsonSerializerOptions);
                                        resource.CommandReference = name;
                                        resource.Type = ResourceType.CANONS;
                                        resource.Style = ResourceStyle.SECTIONED;
                                        return resource;
                                    }
                                case ResourceStyle.FULL_TEXT:
                                default:
                                    throw new NotImplementedException();
                            }
                        }
                        break;
                    }
                case ResourceType.CREED:
                default:
                    break;
            }

            throw new KeyNotFoundException();
        }
    }
}
