using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

using BibleBot.Backend.Models;

namespace BibleBot.Backend.Services
{
    public enum ResourceType
    {
        CREED = 0,
        CATECHISM = 1,
        PARAGRAPHED = 2,
        SECTIONED = 3
    }

    public class ResourceService
    {
        private readonly Dictionary<string, Tuple<ResourceType, string>> _catechisms;
        private readonly List<string> _creedLanguages;

        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public ResourceService()
        {
            _catechisms = new Dictionary<string, Tuple<ResourceType, string>>
            {
                { "ccc", new Tuple<ResourceType, string>(ResourceType.PARAGRAPHED, "catechism_of_the_catholic_church") },
                { "lsc" , new Tuple<ResourceType, string>(ResourceType.SECTIONED,  "luthers_small_catechism") }
            };

            _creedLanguages = new List<string>
            {
                "english"
            };

            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                IgnoreNullValues = true
            };
        }

        public IResource GetResource(ResourceType type, string name)
        {
            if (type == ResourceType.CREED)
            {
                if (_creedLanguages.Contains(name))
                {
                    var creedFile = File.ReadAllText($"./Data/Creeds/{name}.json");
                    return JsonSerializer.Deserialize<CreedResource>(creedFile, _jsonSerializerOptions);
                }
            }
            else if (type == ResourceType.CATECHISM)
            {
                if (_catechisms.ContainsKey(name))
                {
                    var idealCatechism = _catechisms[name];

                    var catechismFile = File.ReadAllText($"./Data/Catechisms/{idealCatechism.Item2}.json");

                    if (idealCatechism.Item1 == ResourceType.PARAGRAPHED)
                    {
                        return JsonSerializer.Deserialize<ParagraphedResource>(catechismFile, _jsonSerializerOptions);
                    }
                    else if (idealCatechism.Item1 == ResourceType.SECTIONED)
                    {
                        return JsonSerializer.Deserialize<SectionedResource>(catechismFile, _jsonSerializerOptions);
                    }
                }
            }

            return null;
        }
    }
}