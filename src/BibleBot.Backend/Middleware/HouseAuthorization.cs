/*
* Copyright (C) 2016-2026 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Threading.Tasks;
using BibleBot.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace BibleBot.Backend.Middleware
{
    public class HouseAuthorizationMiddleware(RequestDelegate next)
    {
        public async Task Invoke(HttpContext context)
        {
            string authHeader = context.Request.Headers.Authorization;
            if (authHeader != null)
            {
                if (authHeader == Environment.GetEnvironmentVariable("ENDPOINT_TOKEN"))
                {
                    await next(context);
                }
                else
                {
                    context.Response.StatusCode = 401;
                    if (context.Request.Path.Value!.Contains("verses"))
                    {
                        await context.Response.WriteAsJsonAsync(new VerseResponse
                        {
                            OK = false,
                            Verses = null,
                            LogStatement = "Unauthorized"
                        });
                    }
                    else
                    {
                        await context.Response.WriteAsJsonAsync(new CommandResponse
                        {
                            OK = false,
                            Pages = null,
                            LogStatement = "Unauthorized"
                        });
                    }
                }
            }
            else if (context.Request.Path.Value! == "/metrics")
            {
                await next(context);
            }
            else
            {
                context.Response.StatusCode = 401;
                if (context.Request.Path.Value!.Contains("verses"))
                {
                    await context.Response.WriteAsJsonAsync(new VerseResponse
                    {
                        OK = false,
                        Verses = null,
                        LogStatement = "Unauthorized"
                    });
                }
                else
                {
                    await context.Response.WriteAsJsonAsync(new CommandResponse
                    {
                        OK = false,
                        Pages = null,
                        LogStatement = "Unauthorized"
                    });
                }
            }
        }
    }

    public static class HouseAuthorizationMiddlewareExtensions
    {
        public static void UseHouseAuthorization(this IApplicationBuilder builder) => builder.UseMiddleware<HouseAuthorizationMiddleware>();
    }
}
