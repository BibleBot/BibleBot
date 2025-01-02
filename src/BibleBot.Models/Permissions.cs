/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace BibleBot.Models
{
    /// <summary>
    /// A enum of Discord's permission bitfields (see <seealso href="https://discord.com/developers/docs/topics/permissions"/>).
    /// </summary>
    [Flags]
    public enum Permissions : long
    {
        CREATE_INSTANT_INVITE = (long)1 << 0,
        KICK_MEMBERS = (long)1 << 1,
        BAN_MEMBERS = (long)1 << 2,
        ADMINISTRATOR = (long)1 << 3,
        MANAGE_CHANNELS = (long)1 << 4,
        MANAGE_GUILD = (long)1 << 5,
        ADD_REACTIONS = (long)1 << 6,
        VIEW_AUDIT_LOG = (long)1 << 7,
        PRIORITY_SPEAKER = (long)1 << 8,
        STREAM = (long)1 << 9,
        VIEW_CHANNEL = (long)1 << 10,
        SEND_MESSAGES = (long)1 << 11,
        SEND_TTS_MESSAGES = (long)1 << 12,
        MANAGE_MESSAGES = (long)1 << 13,
        EMBED_LINKS = (long)1 << 14,
        ATTACH_FILES = (long)1 << 15,
        READ_MESSAGE_HISTORY = (long)1 << 16,
        MENTION_EVERYONE = (long)1 << 17,
        USE_EXTERNAL_EMOJIS = (long)1 << 18,
        VIEW_GUILD_INSIGHTS = (long)1 << 19,
        CONNECT = (long)1 << 20,
        SPEAK = (long)1 << 21,
        MUTE_MEMBERS = (long)1 << 22,
        DEAFEN_MEMBERS = (long)1 << 23,
        MOVE_MEMBERS = (long)1 << 24,
        USE_VAD = (long)1 << 25,
        CHANGE_NICKNAME = (long)1 << 26,
        MANAGE_NICKNAMES = (long)1 << 27,
        MANAGE_ROLES = (long)1 << 28,
        MANAGE_WEBHOOKS = (long)1 << 29,
        MANAGE_GUILD_EXPRESSIONS = (long)1 << 30,
        USE_APPLICATION_COMMANDS = (long)1 << 31,
        REQUEST_TO_SPEAK = (long)1 << 32,
        MANAGE_EVENTS = (long)1 << 33,
        MANAGE_THREADS = (long)1 << 34,
        CREATE_PUBLIC_THREADS = (long)1 << 35,
        CREATE_PRIVATE_THREADS = (long)1 << 36,
        USE_EXTERNAL_STICKERS = (long)1 << 37,
        SEND_MESSAGES_IN_THREADS = (long)1 << 38,
        USE_EMBEDDED_ACTIVITIES = (long)1 << 39,
        MODERATE_MEMBERS = (long)1 << 40,
        VIEW_CREATOR_MONETIZATION_ANALYTICS = (long)1 << 41,
        USE_SOUNDBOARD = (long)1 << 42,
        USE_EXTERNAL_SOUNDS = (long)1 << 45,
        SEND_VOICE_MESSAGES = (long)1 << 46
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member