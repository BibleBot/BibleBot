/*
* Copyright (C) 2016-2023 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;
using CacheCow.Client;

namespace BibleBot.Models
{
    public static class CachingClient
    {
        public static readonly int expiryMins = 120; // Change when data expires & is freed from CacheCow.InMemoryCacheStore, min is 60
        public static readonly int staleMins = 60; // Change when data becomes stale and needs revalidation

        public static HttpClient GetCachingClient(string baseURL)
        {
            HttpClient client = HttpClientFactory.Create(
                new CachingHandler(new InMemoryCacheStore(System.TimeSpan.FromMinutes(expiryMins))),
                new CacheControlHandler());

            client.BaseAddress = new System.Uri(baseURL);

            return client;
        }

        public static HttpClient GetTrimmedCachingClient(string baseURL, bool isHtml)
        {
            HttpClient client;

            if (isHtml)
            {
                client = HttpClientFactory.Create(
                new CachingHandler(new InMemoryCacheStore(System.TimeSpan.FromMinutes(expiryMins))),
                new CacheControlHandler(),
                new HtmlTrimHandler());
            }
            else
            {
                client = HttpClientFactory.Create(
                new CachingHandler(new InMemoryCacheStore(System.TimeSpan.FromMinutes(expiryMins))),
                new CacheControlHandler(),
                new JsonTrimHandler());
            }
            client.BaseAddress = new System.Uri(baseURL);

            return client;
        }


        public static async Task<T> GetJsonContentAs<T>(this HttpClient client, string url, JsonSerializerOptions op)
        {
            var resp = await client.GetAsync(url);

            // Benchmarking/Debugging
            // try
            // {
            //     System.Console.WriteLine("[{0}]", string.Join(", ", resp.Headers.GetValues("x-cachecow-client")));
            // }
            // catch (System.Exception) { }

            if (resp.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(await resp.Content.ReadAsStringAsync(), op);
        }
    }

    public class CacheControlHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                response.Headers.CacheControl = new()
                {
                    MaxAge = System.TimeSpan.FromMinutes(CachingClient.staleMins)
                };
            }

            return response;
        }
    }

    public class HtmlTrimHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // In testing, trimming reduces response time by ~20%
            // Reduces cached response size by ~130kb (~99% less on Phil 4:6-7) which reduces need for request splitting

            var response = await base.SendAsync(request, cancellationToken);
            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(await response.Content.ReadAsStreamAsync());

            response.Content = new StringContent(
                document.GetElementsByClassName("dropdown-display").FirstOrDefault().InnerHtml + // Verse reference
                document.QuerySelector(".result-text-style-normal").InnerHtml); // Verse body

            return response;
        }
    }

    public class JsonTrimHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // In testing, trimming's effect is negligible on response time (~10%, <100ms)
            // Reduces cache response size by ~1kb (~39% less on Phil 4:6-7)

            var response = await base.SendAsync(request, cancellationToken);

            JsonNode json = JsonNode.Parse(await response.Content.ReadAsStringAsync());
            response.Content = json["data"] != null ? new StringContent(json["data"].ToJsonString()) : null;

            return response;
        }
    }
}