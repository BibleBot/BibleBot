/*
* Copyright (C) 2016-2021 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using BibleBot.Lib;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace BibleBot.Backend.Models
{
    public class Language
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("Name")]
        public string Name { get; set; }

        [BsonElement("ObjectName")]
        public string ObjectName { get; set; }

        [BsonElement("DefaultVersion")]
        public string DefaultVersion { get; set; }

        [BsonElement("RawLanguage")]
        public RawLanguage RawLanguage { get; set; }
    }

    /*
     *  !----------- THE FOLLOWING CLASSES ARE AUTO-GENERATED -----------!
     *    These classes were auto-generated with QuickType.io using the
     *    default.json language file. They have been slightly modified
     *    to make more logical sense, however quirks may still be abound.
     */

    public partial class RawLanguage
    {
        [BsonElement("versionString")]
        public string VersionString { get; set; }

        [BsonElement("relatedCommands")]
        public string RelatedCommands { get; set; }

        [BsonElement("permissionsError")]
        public string PermissionsError { get; set; }

        [BsonElement("permissionsErrorUser")]
        public string PermissionsErrorUser { get; set; }

        [BsonElement("permissionsErrorBot")]
        public string PermissionsErrorBot { get; set; }

        [BsonElement("needsManageServer")]
        public string NeedsManageServer { get; set; }

        [BsonElement("argumentsError")]
        public string ArgumentsError { get; set; }

        [BsonElement("versionNoOT")]
        public string VersionNoOT { get; set; }

        [BsonElement("versionNoNT")]
        public string VersionNoNT { get; set; }

        [BsonElement("versionNoDEU")]
        public string VersionNoDEU { get; set; }

        [BsonElement("tmrError")]
        public string TmrError { get; set; }

        [BsonElement("tmrErrorExp")]
        public string TmrErrorExp { get; set; }

        [BsonElement("verseError")]
        public string VerseError { get; set; }

        [BsonElement("buttonFail")]
        public string ButtonFail { get; set; }

        [BsonElement("enable")]
        public string Enable { get; set; }

        [BsonElement("disable")]
        public string Disable { get; set; }

        [BsonElement("enabled")]
        public string Enabled { get; set; }

        [BsonElement("disabled")]
        public string Disabled { get; set; }

        [BsonElement("setDisplayView")]
        public SetDisplayView SetDisplayView { get; set; }

        [BsonElement("bracketsView")]
        public BracketsView BracketsView { get; set; }

        [BsonElement("commands")]
        public Commands Commands { get; set; }
    }

    public partial class BracketsView
    {
        [BsonElement("selections")]
        public BracketsViewSelections Selections { get; set; }

        [BsonElement("placeholder")]
        public string Placeholder { get; set; }

        [BsonElement("discordError")]
        public string DiscordError { get; set; }
    }

    public partial class BracketsViewSelections
    {
        [BsonElement("angleBrackets")]
        public BracketSelection AngleBrackets { get; set; }

        [BsonElement("squareBrackets")]
        public BracketSelection SquareBrackets { get; set; }

        [BsonElement("curlyBrackets")]
        public BracketSelection CurlyBrackets { get; set; }

        [BsonElement("parentheses")]
        public BracketSelection Parentheses { get; set; }
    }

    public partial class BracketSelection
    {
        [BsonElement("label")]
        public string Label { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }
    }

    public partial class Commands
    {
        [BsonElement("biblebot")]
        public BibleBot Biblebot { get; set; }

        [BsonElement("stats")]
        public Stats Stats { get; set; }

        [BsonElement("invite")]
        public Invite Invite { get; set; }

        [BsonElement("announce")]
        public Announce Announce { get; set; }

        [BsonElement("supporters")]
        public PlainResp Supporters { get; set; }

        [BsonElement("resources")]
        public ComplexResp Resources { get; set; }

        [BsonElement("resource")]
        public Resource Resource { get; set; }

        [BsonElement("formatting")]
        public Formatting Formatting { get; set; }

        [BsonElement("setversenumbers")]
        public ToggleResp SetVerseNumbers { get; set; }

        [BsonElement("settitles")]
        public ToggleResp SetTitles { get; set; }

        [BsonElement("setpagination")]
        public ToggleResp SetPagination { get; set; }

        [BsonElement("setdisplay")]
        public CustomInputResp SetDisplay { get; set; }

        [BsonElement("setserverdisplay")]
        public CustomInputResp SetServerDisplay { get; set; }

        [BsonElement("setbrackets")]
        public CustomInputResp SetBrackets { get; set; }

        [BsonElement("language")]
        public DailyVerseStatus Language { get; set; }

        [BsonElement("setlanguage")]
        public SetLanguageResp SetLanguage { get; set; }

        [BsonElement("setserverlanguage")]
        public SetLanguageResp SetServerLanguage { get; set; }

        [BsonElement("listlanguages")]
        public ComplexResp ListLanguages { get; set; }

        [BsonElement("version")]
        public DailyVerseStatus Version { get; set; }

        [BsonElement("setversion")]
        public SetVersionResp SetVersion { get; set; }

        [BsonElement("setserverversion")]
        public SetVersionResp SetServerVersion { get; set; }

        [BsonElement("versioninfo")]
        public Versioninfo VersionInfo { get; set; }

        [BsonElement("listversions")]
        public PlainResp ListVersions { get; set; }

        [BsonElement("dailyverse")]
        public ComplexResp DailyVerse { get; set; }

        [BsonElement("dailyverseset")]
        public DailyVerseSet DailyVerseSet { get; set; }

        [BsonElement("dailyversestatus")]
        public DailyVerseStatus DailyVerseStatus { get; set; }

        [BsonElement("dailyverseclear")]
        public ToggleResp DailyVerseClear { get; set; }

        [BsonElement("random")]
        public Random Random { get; set; }

        [BsonElement("truerandom")]
        public Random TrueRandom { get; set; }

        [BsonElement("search")]
        public Search Search { get; set; }
    }

    public partial class Announce
    {
        [BsonElement("cmd")]
        public string Command { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("announcementHeader")]
        public string AnnouncementHeader { get; set; }

        [BsonElement("parameters")]
        public AnnounceParameters Parameters { get; set; }
    }

    public partial class AnnounceParameters
    {
        [BsonElement("content")]
        public string Content { get; set; }

        [BsonElement("contentDesc")]
        public string ContentDesc { get; set; }
    }

    public partial class BibleBot
    {
        [BsonElement("cmd")]
        public string Command { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("biblebot")]
        public string VersionString { get; set; }

        [BsonElement("slogan")]
        public string Slogan { get; set; }

        [BsonElement("credit")]
        public string Credit { get; set; }

        [BsonElement("commands")]
        public string Commands { get; set; }

        [BsonElement("commandList")]
        public string CommandList { get; set; }

        [BsonElement("links")]
        public string Links { get; set; }

        [BsonElement("website")]
        public string Website { get; set; }

        [BsonElement("copyrights")]
        public string Copyrights { get; set; }

        [BsonElement("code")]
        public string Code { get; set; }

        [BsonElement("terms")]
        public string Terms { get; set; }

        [BsonElement("news")]
        public string News { get; set; }
    }

    public partial class ComplexResp
    {
        [BsonElement("cmd")]
        public string Command { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }
    }

    public partial class ToggleResp
    {
        [BsonElement("cmd")]
        public string Command { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("success")]
        public string Success { get; set; }

        [BsonElement("argumentsError")]
        public string ArgumentsError { get; set; }
    }

    public partial class DailyVerseSet
    {
        [BsonElement("cmd")]
        public string Command { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("success")]
        public string Success { get; set; }

        [BsonElement("fail")]
        public string Fail { get; set; }

        [BsonElement("failDM")]
        public string FailDM { get; set; }
    }

    public partial class DailyVerseStatus
    {
        [BsonElement("cmd")]
        public string Command { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("status")]
        public string Status { get; set; }

        [BsonElement("relatedCommands")]
        public string RelatedCommands { get; set; }

        [BsonElement("fail")]
        public string Fail { get; set; }

        [BsonElement("serverStatus")]
        public string ServerStatus { get; set; }
    }

    public partial class Formatting
    {
        [BsonElement("cmd")]
        public string Command { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("verseNumbersStatus")]
        public string VerseNumbersStatus { get; set; }

        [BsonElement("titlesStatus")]
        public string TitlesStatus { get; set; }

        [BsonElement("versePaginationStatus")]
        public string VersePaginationStatus { get; set; }

        [BsonElement("displayStyleStatus")]
        public string DisplayStyleStatus { get; set; }

        [BsonElement("serverDisplayStyleStatus")]
        public string ServerDisplayStyleStatus { get; set; }

        [BsonElement("bracketsStatus")]
        public string BracketsStatus { get; set; }

        [BsonElement("relatedCommands")]
        public string RelatedCommands { get; set; }
    }

    public partial class Invite
    {
        [BsonElement("cmd")]
        public string Command { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("usage")]
        public string Usage { get; set; }
    }

    public partial class PlainResp
    {
        [BsonElement("cmd")]
        public string Command { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("title")]
        public string Title { get; set; }

        [BsonElement("listStart")]
        public string ListStart { get; set; }
    }

    public partial class Random
    {
        [BsonElement("cmd")]
        public string Command { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("bibliomancyWarning")]
        public string BibliomancyWarning { get; set; }

        [BsonElement("useInDMs")]
        public string UseInDMs { get; set; }
    }

    public partial class Resource
    {
        [BsonElement("cmd")]
        public string Command { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("creeds")]
        public string Creeds { get; set; }

        [BsonElement("catechisms")]
        public string Catechisms { get; set; }

        [BsonElement("canonLaws")]
        public string CanonLaws { get; set; }

        [BsonElement("usage")]
        public string Usage { get; set; }

        [BsonElement("parameters")]
        public ResourceParameters Parameters { get; set; }
    }

    public partial class ResourceParameters
    {
        [BsonElement("resource")]
        public string Resource { get; set; }

        [BsonElement("resourceDesc")]
        public string ResourceDesc { get; set; }

        [BsonElement("range")]
        public string Range { get; set; }

        [BsonElement("rangeDesc")]
        public string RangeDesc { get; set; }
    }

    public partial class Search
    {
        [BsonElement("cmd")]
        public string Command { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("queryTooShort")]
        public string QueryTooShort { get; set; }

        [BsonElement("noResults")]
        public string NoResults { get; set; }

        [BsonElement("searchHeader")]
        public string SearchHeader { get; set; }

        [BsonElement("paginator")]
        public string Paginator { get; set; }
    }

    public partial class CustomInputResp
    {
        [BsonElement("cmd")]
        public string Command { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("cta")]
        public string CallToAction { get; set; }

        [BsonElement("argumentsError")]
        public string ArgumentsError { get; set; }

        [BsonElement("argumentsError2")]
        public string ArgumentsError2 { get; set; }

        [BsonElement("characterIssue")]
        public string CharacterIssue { get; set; }

        [BsonElement("success")]
        public string Success { get; set; }
    }

    public partial class SetVersionResp
    {
        [BsonElement("cmd")]
        public string Command { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("argumentsError")]
        public string ArgumentsError { get; set; }

        [BsonElement("success")]
        public string Success { get; set; }

        [BsonElement("fail")]
        public string Fail { get; set; }

        [BsonElement("parameters")]
        public SetVersionParameters Parameters { get; set; }
    }

    public partial class SetLanguageResp
    {
        [BsonElement("cmd")]
        public string Command { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("argumentsError")]
        public string ArgumentsError { get; set; }

        [BsonElement("success")]
        public string Success { get; set; }

        [BsonElement("fail")]
        public string Fail { get; set; }

        [BsonElement("parameters")]
        public SetLanguageParameters Parameters { get; set; }
    }

    public partial class SetVersionParameters
    {
        [BsonElement("abbreviation")]
        public string Abbreviation { get; set; }

        [BsonElement("abbreviationDesc")]
        public string AbbreviationDesc { get; set; }
    }

    public partial class SetLanguageParameters
    {
        [BsonElement("objectName")]
        public string ObjectName { get; set; }

        [BsonElement("objectNameDesc")]
        public string ObjectNameDesc { get; set; }
    }

    public partial class Stats
    {
        [BsonElement("cmd")]
        public string Command { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("frontendStats")]
        public string FrontendStats { get; set; }

        [BsonElement("shardCount")]
        public string ShardCount { get; set; }

        [BsonElement("serverCount")]
        public string ServerCount { get; set; }

        [BsonElement("userCount")]
        public string UserCount { get; set; }

        [BsonElement("channelCount")]
        public string ChannelCount { get; set; }

        [BsonElement("backendStats")]
        public string BackendStats { get; set; }

        [BsonElement("userPrefsCount")]
        public string UserPrefsCount { get; set; }

        [BsonElement("guildPrefsCount")]
        public string GuildPrefsCount { get; set; }

        [BsonElement("versionCount")]
        public string VersionCount { get; set; }

        [BsonElement("metadata")]
        public string Metadata { get; set; }
    }

    public partial class Versioninfo
    {
        [BsonElement("cmd")]
        public string Command { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("argumentsError")]
        public string ArgumentsError { get; set; }

        [BsonElement("containsOT")]
        public string ContainsOT { get; set; }

        [BsonElement("containtsNT")]
        public string ContaintsNT { get; set; }

        [BsonElement("containsDEU")]
        public string ContainsDEU { get; set; }

        [BsonElement("fail")]
        public string Fail { get; set; }

        [BsonElement("parameters")]
        public SetVersionParameters Parameters { get; set; }
    }

    public partial class SetDisplayView
    {
        [BsonElement("selections")]
        public SetDisplayViewSelections Selections { get; set; }

        [BsonElement("placeholder")]
        public string Placeholder { get; set; }

        [BsonElement("discordError")]
        public string DiscordError { get; set; }
    }

    public partial class SetDisplayViewSelections
    {
        [BsonElement("embed")]
        public DisplayStyleSelection Embed { get; set; }

        [BsonElement("code")]
        public DisplayStyleSelection Code { get; set; }

        [BsonElement("blockquote")]
        public DisplayStyleSelection Blockquote { get; set; }
    }

    public partial class DisplayStyleSelection
    {
        [BsonElement("value")]
        public string Value { get; set; }

        [BsonElement("label")]
        public string Label { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }
    }
}
