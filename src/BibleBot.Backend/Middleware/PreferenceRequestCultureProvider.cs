/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BibleBot.Backend.Services;
using BibleBot.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

namespace BibleBot.Backend.Middleware
{
    public class PreferenceRequestCultureProvider(UserService userService, GuildService guildService, LanguageService languageService) : RequestCultureProvider
    {
        private readonly UserService _userService = userService;
        private readonly GuildService _guildService = guildService;
        private readonly LanguageService _languageService = languageService;
        private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

        public override async Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
        {
            ArgumentNullException.ThrowIfNull(httpContext);

            HttpRequest request = httpContext.Request;
            if (request.Method == HttpMethods.Post && request.ContentLength > 0)
            {
                request.EnableBuffering();
                byte[] buffer = new byte[Convert.ToInt32(request.ContentLength)];
                await request.Body.ReadExactlyAsync(buffer);

                Request req = JsonSerializer.Deserialize<Request>(Encoding.UTF8.GetString(buffer), _jsonSerializerOptions);

                User idealUser = await _userService.Get(req.UserId);
                string culture = "en-US";

                if (idealUser != null)
                {
                    culture = idealUser.Language;
                }
                else
                {
                    Guild idealGuild = await _guildService.Get(req.GuildId);

                    if (idealGuild != null)
                    {
                        culture = idealGuild.Language;
                    }
                }

                request.Body.Position = 0;

                Language language = await _languageService.Get(culture);

                if (language != null)
                {
                    return new ProviderCultureResult(culture);
                }
            }

            return null;
        }
    }
}