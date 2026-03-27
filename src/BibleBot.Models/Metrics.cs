/*
* Copyright (C) 2016-2026 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NpgsqlTypes;

namespace BibleBot.Models
{
    /// <summary>
    /// A verse metric class, which outlines the verse referenced and the version used.
    /// </summary>
    /// <remarks>
    /// This class is designed to log references within a single chapter, other
    /// chapters in a reference should be added separately. There is no real alternative that
    /// would avoid verses being erroneously accounted in multiple chapters.
    /// </remarks>
    [Table("verse_metrics")]
    public class VerseMetric()
    {
        /// <summary>
        /// The ID of the row.
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// The time that the object was generated.
        /// </summary>
        [Column("time_generated")]
        public Instant TimeGenerated { get; set; }

        /// <summary>
        /// The ID of the user that invoked the reference.
        /// </summary>
        [Required]
        [Column("user_id")]
        public long UserId { get; set; }

        /// <summary>
        /// The ID of the guild that invoked the reference.
        /// </summary>
        [Required]
        [Column("guild_id")]
        public long GuildId { get; set; }

        /// <summary>
        /// The internal name of the book.
        /// </summary>
        [Required]
        [Column("book")]
        public string Book { get; set; }

        /// <summary>
        /// The chapter referenced.
        /// </summary>
        [Required]
        [Column("chapter")]
        public int Chapter { get; set; }

        /// <summary>
        /// The range of verses referenced.
        /// </summary>
        [Required]
        [Column("verse_range", TypeName = "int4range")]
        public NpgsqlRange<int> VerseRange { get; set; }

        /// <summary>
        /// The abbreviation of the version used.
        /// </summary>
        [Required]
        [Column("version")]
        public string Version { get; set; }

        /// <summary>
        /// The publisher of the version, if applicable.
        /// </summary>
        [Column("publisher")]
        public string Publisher { get; set; }

        /// <summary>
        /// Indicates whether the reference is of the Old Testament.
        /// </summary>
        /// <remarks>
        /// If this is true, <see cref="IsNT"/> and <see cref="IsDEU"/> must be false.
        /// </remarks>
        [Required]
        [Column("is_ot")]
        public bool IsOT { get; set; }

        /// <summary>
        /// Indicates whether the reference is of the New Testament.
        /// </summary>
        /// <remarks>
        /// If this is true, <see cref="IsOT"/> and <see cref="IsDEU"/> must be false.
        /// </remarks>
        [Required]
        [Column("is_nt")]
        public bool IsNT { get; set; }

        /// <summary>
        /// Indicates whether the reference is of the Deuterocanon.
        /// </summary>
        /// <remarks>
        /// If this is true, <see cref="IsOT"/> and <see cref="IsNT"/> must be false.
        /// </remarks>
        [Required]
        [Column("is_deu")]
        public bool IsDEU { get; set; }

        /// <summary>
        /// The appended verse ranges, if applicable.
        /// </summary>
        public List<AppendedVerse> AppendedVerses { get; set; } = [];
    }

    /// <summary>
    /// The table for appended verse ranges.
    /// </summary>
    [Table("appended_verses")]
    public class AppendedVerse()
    {
        /// <summary>
        /// The ID of the row.
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// The ID of the verse metric entry this belongs to.
        /// </summary>
        [Column("verse_metric_id")]
        [ForeignKey(nameof(VerseMetric))]
        public int VerseMetricId { get; set; }

        /// <summary>
        /// The verse range
        /// </summary>
        [Column("verse_range", TypeName = "int4range")]
        public NpgsqlRange<int> VerseRange { get; set; }

        /// <summary>
        /// The verse metric entry this belongs to.
        /// </summary>
        public VerseMetric VerseMetric { get; set; }
    }
}
