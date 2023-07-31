/*
* Copyright (C) 2016-2023 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CacheCow.Client;

namespace BibleBot.Models
{
    public static class CachingClient
    {
        public static HttpClient GetCachingClient()
        {
            return HttpClientFactory.Create(new CachingHandler(), new CacheControlHandler());
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
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            int ttlMins = 60; // Set how long to keep data fresh in cache

            CacheControlHeaderValue cacheControl = new CacheControlHeaderValue();
            cacheControl.MaxAge = System.TimeSpan.FromMinutes(ttlMins);

            response.Headers.CacheControl = cacheControl;
            // response.Content.Headers.Expires = time // If expiry is needed, but CacheControl header should suffice

            // TODO: may be able to cut down cache size by trimming excess data out (all the html stuff)

            return response;
        }
    }
}