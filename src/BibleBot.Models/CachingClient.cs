/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using CacheCow.Client;

// TODO(srp): Add documentation strings to this.
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace BibleBot.Models
{
    public static class CachingClient
    {
        private static readonly int _expiryMins = 120; // Change when data expires & is freed from CacheCow.InMemoryCacheStore, min is 60
        public static readonly int _staleMins = 60; // Change when data becomes stale and needs revalidation

        public static HttpClient GetCachingClient(string baseURL)
        {
            HttpClient client = HttpClientFactory.Create(
                new CachingHandler(new InMemoryCacheStore(System.TimeSpan.FromMinutes(_expiryMins))),
                new CacheControlHandler());

            client.BaseAddress = new System.Uri(baseURL);

            return client;
        }

        public static HttpClient GetTrimmedCachingClient(string baseURL, bool isHtml)
        {
            HttpClient client;

#pragma warning disable IDE0045
            if (isHtml)
            {
                client = HttpClientFactory.Create(
                new CachingHandler(new InMemoryCacheStore(System.TimeSpan.FromMinutes(_expiryMins))),
                new CacheControlHandler(),
                new HtmlTrimHandler());
            }
            else
            {
                client = HttpClientFactory.Create(
                new CachingHandler(new InMemoryCacheStore(System.TimeSpan.FromMinutes(_expiryMins))),
                new CacheControlHandler(),
                new JsonTrimHandler());
            }
#pragma warning restore IDE0045
            client.BaseAddress = new System.Uri(baseURL);

            return client;
        }

        public static HttpClient GetTrimmedCachingClient(bool isHtml)
        {
            HttpClient client;

#pragma warning disable IDE0045
            if (isHtml)
            {
                client = HttpClientFactory.Create(
                new CachingHandler(new InMemoryCacheStore(System.TimeSpan.FromMinutes(_expiryMins))),
                new CacheControlHandler(),
                new HtmlTrimHandler());
            }
            else
            {
                client = HttpClientFactory.Create(
                new CachingHandler(new InMemoryCacheStore(System.TimeSpan.FromMinutes(_expiryMins))),
                new CacheControlHandler(),
                new JsonTrimHandler());
            }
#pragma warning restore IDE0045

            return client;
        }


        public static async Task<T> GetJsonContentAs<T>(this HttpClient client, string url, JsonSerializerOptions op)
        {
            HttpResponseMessage resp = await client.GetAsync(url);
            return resp.StatusCode != System.Net.HttpStatusCode.OK ? default : JsonSerializer.Deserialize<T>(await resp.Content.ReadAsStringAsync(), op);
        }
    }

    public class CacheControlHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                response.Headers.CacheControl = new()
                {
                    MaxAge = System.TimeSpan.FromMinutes(CachingClient._staleMins)
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
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            HtmlParser parser = new();
            IHtmlDocument document = await parser.ParseDocumentAsync(await response.Content.ReadAsStreamAsync(cancellationToken));

            IElement respContent = document.QuerySelector(".passage-box");

            if (respContent == null)
            {
                return new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.BadRequest };
            }

            response.Content = new StringContent(respContent.InnerHtml);

            return response;
        }
    }

    public class JsonTrimHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // In testing, trimming's effect is negligible on response time (~10%, <100ms)
            // Reduces cache response size by ~1kb (~39% less on Phil 4:6-7)
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            try
            {
                JsonNode json = JsonNode.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
                response.Content = json?["data"] != null ? new StringContent(json["data"].ToJsonString()) : new StringContent("{}");
            }
            catch (JsonException)
            {
                response.Content = new StringContent("Unknown error");
            }


            return response;
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
