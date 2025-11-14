/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BibleBot.Models
{
    /// <summary>
    /// The model for Discord's Embed object (see <seealso href="https://discord.com/developers/docs/resources/channel#embed-object"/>).
    /// </summary>
    /// <remarks>
    /// Frontend casts this object to the <c>disnake.Embed</c> type when sending responses.
    /// <br/><br/>
    /// The sum of all characters within all embed structures in a single message must not exceed 6000 characters.
    /// </remarks>
    public class InternalEmbed
    {
        /// <summary>
        /// The title of the embed. Titles are limited to 256 characters.
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; }

        /// <summary>
        /// The type of embed.
        /// </summary>
        /// <remarks>
        /// Discord considers these essentially deprecated. There are other embed types, like images, videos, articles, etc., but
        /// I'm not sure if it was ever permissible to send embeds of different types. We use the <c>rich</c> type as it
        /// is the "standard" embed.
        /// </remarks>
        [JsonPropertyName("type")]
        public string Type = "rich";

        /// <summary>
        /// The description of the embed. Descriptions are limited to 4096 characters.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }

        /// <summary>
        /// The URL of the embed. This turns the <see cref="Title"/> into a link, with this as the reference.
        /// </summary>
        [JsonPropertyName("url")]
        public string URL { get; set; }

        /// <summary>
        /// The color code of the embed's accent.
        /// </summary>
        /// <remarks>
        /// This is a <i>decimal</i> representation of the color. For some reason, hexadecimal is just too much to ask.
        /// </remarks>
        [JsonPropertyName("color")]
        public uint Color { get; set; }

        /// <summary>
        /// The footer of the embed.
        /// </summary>
        [JsonPropertyName("footer")]
        public Footer Footer { get; set; }

        /// <summary>
        /// The image of the embed, spanning the width of the embed at the bottom of it.
        /// </summary>
        [JsonPropertyName("image")]
        public Media Image { get; set; }

        /// <summary>
        /// The thumbnail of the embed, placed in the upper right corner of it.
        /// </summary>
        [JsonPropertyName("thumbnail")]
        public Media Thumbnail { get; set; }

        /// <summary>
        /// The video of the embed, placed similarly to the <see cref="Image"/>.
        /// </summary>
        [JsonPropertyName("video")]
        public Media Video { get; set; }

        /// <summary>
        /// The provider of the embed. I am uncertain what this does.
        /// </summary>
        [JsonPropertyName("provider")]
        public Provider Provider { get; set; }

        /// <summary>
        /// The author of the embed. Setting this will create a little user display in
        /// the upper left corner of the embed.
        /// </summary>
        [JsonPropertyName("author")]
        public Author Author { get; set; }

        /// <summary>
        /// The fields of the embed. There can be a maximum of 25 fields in a single embed.
        /// </summary>
        [JsonPropertyName("fields")]
        public List<EmbedField> Fields { get; set; }
    }

    /// <summary>
    /// The footer model belonging to an embed.
    /// </summary>
    /// <remarks>
    /// We use this to display the running version of BibleBot in all embed responses.
    /// </remarks>
    public class Footer
    {
        /// <summary>
        /// The text of the footer. Footer text is limited to 2048 characters.
        /// </summary>
        [JsonPropertyName("text")]
        public string Text { get; set; }

        /// <summary>
        /// The URL of the icon to be used to the left of the footer text.
        /// </summary>
        [JsonPropertyName("icon_url")]
        public string IconURL { get; set; }

        /// <summary>
        /// The proxied URL of the icon. This is useless to us but relevant if we were reading embeds
        /// instead of writing them...
        /// </summary>
        [JsonPropertyName("proxy_icon_url")]
        public string ProxyIconURL { get; set; }
    }

    /// <summary>
    /// The image/thumbnail/video model.
    /// </summary>
    /// <remarks>
    /// We never use this. If we did, it would be exclusively for images.
    /// </remarks>
    public class Media
    {
        /// <summary>
        /// The URL of the embedded media.
        /// </summary>
        [JsonPropertyName("url")]
        public string URL { get; set; }

        /// <summary>
        /// The proxied URL of the embedded media.
        /// </summary>
        [JsonPropertyName("proxy_url")]
        public string ProxyURL { get; set; }

        /// <summary>
        /// The height of the embedded media.
        /// </summary>
        [JsonPropertyName("height")]
        public int Height { get; set; }

        /// <summary>
        /// The width of the embedded media.
        /// </summary>
        [JsonPropertyName("width")]
        public int Width { get; set; }
    }

    /// <summary>
    /// The provider model belonging to an embed.
    /// </summary>
    /// <remarks>
    /// No idea what this is used for in the years that embeds have been available.
    /// Some say the person who implemented this subobject spoke with cryptids and
    /// could summon The Luminous Elemental of Frosembyr at will.
    /// </remarks>
    public class Provider
    {
        /// <summary>
        /// T̷h̷e̴ ̵n̶a̶m̶e̶ ̵o̶f̵ ̴t̶h̶e̸ ̵p̴r̸o̵v̶i̸d̴e̸r̶.̸
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// T̵h̴e̴ ̵U̷R̸L̵ ̸o̶f̷ ̸t̵h̶e̸ ̷p̶r̸o̶v̶i̵d̸e̸r̵.̵
        /// </summary>
        [JsonPropertyName("url")]
        public string URL { get; set; }
    }

    /// <summary>
    /// The author model belonging to an embed.
    /// </summary>
    /// <remarks>
    /// This is one of the few subobjects that can be useful to us. We've flirted with
    /// using it in the past, but it never stuck around.
    /// </remarks>
    public class Author
    {
        /// <summary>
        /// The name of the author. Author names are limited to 256 characters.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// The URL of the author. This converts the <see cref="Name"/> into a link, with this as the reference.
        /// </summary>
        [JsonPropertyName("url")]
        public string URL { get; set; }

        /// <summary>
        /// The icon URL of the author.
        /// </summary>
        [JsonPropertyName("icon_url")]
        public string IconURL { get; set; }

        /// <summary>
        /// The proxied URL of the icon.
        /// </summary>
        [JsonPropertyName("proxy_icon_url")]
        public string ProxyIconURL { get; set; }
    }

    /// <summary>
    /// The field model belonging to an embed.
    /// </summary>
    public class EmbedField
    {
        /// <summary>
        /// The name of the field. Field names are limited to 256 characters.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// The value of the field. Field values are limited to 1024 characters.
        /// </summary>
        [JsonPropertyName("value")]
        public string Value { get; set; }

        /// <summary>
        /// Indicates whether this field can be displayed inline (side-by-side with another field).
        /// </summary>
        [JsonPropertyName("inline")]
        public bool Inline { get; set; }

        /// <summary>
        /// Indicates whether a separator should be added after this field.
        /// </summary>
        [JsonPropertyName("add_separator_after")]
        public bool AddSeparatorAfter { get; set; } = false;

    }

    /// <summary>
    /// The interface for Discord components (see <seealso href="https://discord.com/developers/docs/components/reference#anatomy-of-a-component"/>).
    /// </summary>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(InternalContainer), 17)]
    [JsonDerivedType(typeof(TextDisplayComponent), 10)]
    [JsonDerivedType(typeof(SeparatorComponent), 14)]
    public interface IDiscordComponent
    {
        /// <summary>
        /// The component type.
        /// </summary>
        int Type { get; set; }
    }

    /// <summary>
    /// The model for Discord's Container object (see <seealso href="https://discord.com/developers/docs/components/reference#container"/>).
    /// </summary>
    /// <remarks>
    /// Intended to be used exclusively for AutoServ.
    /// <br/><br/>
    /// The sum of all characters must not exceed 4000 characters.
    /// </remarks>
    public class InternalContainer : IDiscordComponent
    {
        /// <summary>
        /// The component type. For containers, this is always 17.
        /// </summary>
        [JsonIgnore]
        public int Type { get; set; } = 17;

        /// <summary>
        /// Child components contained within this container.
        /// </summary>
        [JsonPropertyName("components")]
        public List<IDiscordComponent> Components { get; set; } = [];

        /// <summary>
        /// The accent color of the container.
        /// </summary>
        [JsonPropertyName("accent_color")]
        public uint AccentColor { get; set; } = 0x000000;
    }

    /// <summary>
    /// The model for Discord's Text Display Component (see <seealso href="https://discord.com/developers/docs/components/reference#text-display"/>).
    /// </summary>
    public class TextDisplayComponent(string content) : IDiscordComponent
    {
        /// <summary>
        /// The component type. For text display components, this is always 10.
        /// </summary>
        [JsonIgnore]
        public int Type { get; set; } = 10;

        /// <summary>
        /// The text content of the component.
        /// </summary>
        [JsonPropertyName("content")]
        public string Content { get; set; } = content;
    }

    /// <summary>
    /// The model for Discord's Separator Component (see <seealso href="https://discord.com/developers/docs/components/reference#separator"/>).
    /// </summary>
    public class SeparatorComponent : IDiscordComponent
    {
        /// <summary>
        /// The component type. For separator components, this is always 14.
        /// </summary>
        [JsonIgnore]
        public int Type { get; set; } = 14;

        /// <summary>
        /// Whether a visual divider should be shown.
        /// </summary>
        [JsonPropertyName("divider")]
        public bool Divider { get; set; } = true;

        /// <summary>
        /// The spacing size around the separator. 1 for small, 2 for large.
        /// </summary>
        [JsonPropertyName("spacing")]
        public int Spacing { get; set; } = 1;
    }
}
