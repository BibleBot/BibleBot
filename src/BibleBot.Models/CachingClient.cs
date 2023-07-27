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
using System.Threading;
using System.Threading.Tasks;
using CacheCow.Client;

namespace BibleBot.Models
{
    public class CachingClient
    {
        public static HttpClient GetCachingClient()
        {
            return HttpClientFactory.Create(new CachingHandler(), new CacheControlHandler());
        }
    }


    class CacheControlHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            int ttlMins = 60; // Set how long to keep data in cache

            CacheControlHeaderValue cacheControl = new CacheControlHeaderValue();
            cacheControl.MaxAge = System.TimeSpan.FromMinutes(ttlMins);

            response.Headers.CacheControl = cacheControl;
            // response.Content.Headers.Expires = time // If expiry is needed, but CacheControl header should suffice

            return response;
        }
    }
}