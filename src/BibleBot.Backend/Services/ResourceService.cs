using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

using BibleBot.Backend.Models;

namespace BibleBot.Backend.Services
{

    public class ResourceService
    {
        private readonly Dictionary<string, Tuple<ResourceStyle, string>> _catechismData;
        private readonly List<string> _creeds;

        private readonly List<IResource> _resources;

        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public ResourceService()
        {
            _catechismData = new Dictionary<string, Tuple<ResourceStyle, string>>
            {
                { "ccc", new Tuple<ResourceStyle, string>(ResourceStyle.PARAGRAPHED, "catechism_of_the_catholic_church") },
                { "lsc" , new Tuple<ResourceStyle, string>(ResourceStyle.SECTIONED,  "luthers_small_catechism") }
            };

            _creeds = new List<string>
            {
                "apostles",
                "nicene325",
                "nicene",
                "chalcedon"
            };

            _resources = new List<IResource>();

            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                IgnoreNullValues = true
            };
        }

        public List<IResource> GetAllResources()
        {
            if (_resources.Count == 0)
            {
                foreach (var catechismData in _catechismData)
                {
                    _resources.Add(GetResource(ResourceType.CATECHISM, catechismData.Key));
                }

                foreach (var creed in _creeds)
                {
                    _resources.Add(GetResource(ResourceType.CREED, creed));
                }

                return _resources;
            }

            return _resources;
        }

        public IResource GetResource(ResourceType type, string name)
        {

            if (type == ResourceType.CREED)
            {
                if (_creeds.Contains(name))
                {
                    var creedFile = File.ReadAllText($"./Data/Creeds/english.json");
                    
                    var creedFileObj = JsonSerializer.Deserialize<CreedFile>(creedFile, _jsonSerializerOptions);

                    CreedResource resource;

                    switch (name)
                    {
                        case "apostles":
                            resource = creedFileObj.Apostles;
                            break;
                        case "nicene325":
                            resource = creedFileObj.Nicene325;
                            break;
                        case "nicene":
                            resource = creedFileObj.Nicene;
                            break;
                        case "chalcedon":
                            resource = creedFileObj.Chalcedon;
                            break;
                        default:
                            throw new KeyNotFoundException();
                    }

                    resource.CommandReference = name;
                    resource.Type = ResourceType.CREED;
                    resource.Style = ResourceStyle.FULL_TEXT;

                    return resource;
                }
            }
            else if (type == ResourceType.CATECHISM)
            {
                if (_catechismData.ContainsKey(name))
                {
                    var idealCatechism = _catechismData[name];

                    var catechismFile = File.ReadAllText($"./Data/Catechisms/{idealCatechism.Item2}.json");

                    if (idealCatechism.Item1 == ResourceStyle.PARAGRAPHED)
                    {
                        var resource = JsonSerializer.Deserialize<ParagraphedResource>(catechismFile, _jsonSerializerOptions);
                        resource.CommandReference = name;
                        resource.Type = ResourceType.CATECHISM;
                        resource.Style = ResourceStyle.PARAGRAPHED;
                        return resource;
                    }
                    else if (idealCatechism.Item1 == ResourceStyle.SECTIONED)
                    {
                        var resource = JsonSerializer.Deserialize<SectionedResource>(catechismFile, _jsonSerializerOptions);
                        resource.CommandReference = name;
                        resource.Type = ResourceType.CATECHISM;
                        resource.Style = ResourceStyle.SECTIONED;
                        return resource;
                    }
                }
            }

            throw new KeyNotFoundException();
        }
    }
}