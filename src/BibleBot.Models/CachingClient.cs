/*
* Copyright (C) 2016-2023 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;
using CacheCow.Client;

namespace BibleBot.Models
{
    public static class CachingClient
    {
        public static readonly int expiryMins = 120; // Change when data expires & is freed from CacheCow.InMemoryCacheStore
        public static readonly int staleMins = 60; // Change when data becomes stale and needs revalidation

        public static HttpClient GetCachingClient()
        {
            return HttpClientFactory.Create(
                new CachingHandler(new InMemoryCacheStore(System.TimeSpan.FromMinutes(expiryMins))),
                new CacheControlHandler());
        }

        public static HttpClient GetTrimmedCachingClient()
        {
            return HttpClientFactory.Create(
                new CachingHandler(new InMemoryCacheStore(System.TimeSpan.FromMinutes(expiryMins))),
                new CacheControlHandler(),
                new HtmlTrimHandler());
        }

        public static async Task<T> GetJsonContentAs<T>(this HttpClient client, string url, JsonSerializerOptions op)
        {
            var resp = await client.GetAsync(url);

            // Benchmarking/Debugging, TODO: remove when ready
            try
            {
                System.Console.WriteLine("[{0}]", string.Join(", ", resp.Headers.GetValues("x-cachecow-client")));
            }
            catch (System.Exception) { }

            return JsonSerializer.Deserialize<T>(await resp.Content.ReadAsStringAsync(), op);
        }
    }

    public class CacheControlHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            response.Headers.CacheControl = new()
            {
                MaxAge = System.TimeSpan.FromMinutes(CachingClient.staleMins)
            };

            return response;
        }
    }

    public class HtmlTrimHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // In testing, trimming reduces response time by ~20%
            // Also reduces each cached response size by ~99% (~130kb less per) which reduces need for request splitting

            var response = await base.SendAsync(request, cancellationToken);
            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(await response.Content.ReadAsStreamAsync());

            response.Content = new StringContent(
                document.GetElementsByClassName("dropdown-display").FirstOrDefault().InnerHtml + // Verse reference
                document.QuerySelector(".result-text-style-normal p").InnerHtml); // Verse body

            return response;
        }
    }

    public class JsonTrimHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            // TODO: Implement (also need to change APIBibleProvider to accept trimmed)

            return response;
        }
    }
}