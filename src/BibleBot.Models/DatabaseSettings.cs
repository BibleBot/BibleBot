/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace BibleBot.Models
{
    public class DatabaseSettings : IDatabaseSettings
    {
        public string UserCollectionName { get; set; }
        public string GuildCollectionName { get; set; }
        public string VersionCollectionName { get; set; }
        public string LanguageCollectionName { get; set; }
        public string FrontendStatsCollectionName { get; set; }
        public string OptOutUserCollectionName { get; set; }
        public string DatabaseName { get; set; }
    }

    public interface IDatabaseSettings
    {
        string UserCollectionName { get; set; }
        string GuildCollectionName { get; set; }
        string VersionCollectionName { get; set; }
        string LanguageCollectionName { get; set; }
        string FrontendStatsCollectionName { get; set; }
        string OptOutUserCollectionName { get; set; }
        string DatabaseName { get; set; }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member