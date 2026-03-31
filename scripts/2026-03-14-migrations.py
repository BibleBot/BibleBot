"""
Copyright (C) 2016-2026 Kerygma Digital Co.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this file,
You can obtain one at https://mozilla.org/MPL/2.0/.
"""

# use frontend venv or perish

import os
import re
import json
from concurrent.futures import ThreadPoolExecutor

from tqdm import tqdm
import pymongo
import psycopg

BATCH_SIZE = 10000


# List of versions that follow the Septuagint (LXX) numbering for Psalms
# This typically involves combining Psalms 9 and 10, and 114 and 115,
# resulting in a numbering that is generally one behind the Masoretic Text (MT)
# from Psalm 10 to 147.
versionsWithLXXPsalms = [
    "LXX",      # Septuagint
    "ELXX",     # Brenton's Septuagint
    "VULGATE",  # Biblia Sacra Vulgata
    "DRA",      # Douay-Rheims
    # "RUSV",   # Russian Synodal Version -- this one is technically LXX-numbered, but BG reworks the numbering to fit the Hebrew
    # "BG1940", # 1940 Bulgarian Bible -- this one is technically LXX-numbered, but BG reworks the numbering to fit the Hebrew
    "BOB",      # Bulgarian Synodal
]

experimentsToImport = [
    {
        "Name": "FormattingCmdEmojiExperiment",
        "Variants": [
            "Control",
            "NewEmojis"
        ],
        "Weights": [
            50,
            50
        ],
        "Description": "New emojis for /formatting toggle indicators.",
        "Type": "Backend",
        "Sphere": "User"
    }
]

postgresSchemas = {
    "frontend_stats": """
        CREATE TABLE frontend_stats (
            id SERIAL PRIMARY KEY,
            shard_count INTEGER NOT NULL,
            server_count INTEGER NOT NULL,
            user_count INTEGER NOT NULL,
            channel_count INTEGER NOT NULL,
            commit_hash TEXT NOT NULL,
            user_install_count INTEGER NOT NULL
        );
    """,
    "languages": """
        CREATE TABLE languages (
            id TEXT PRIMARY KEY,
            name TEXT NOT NULL
        );
    """,
    "versions": """
        CREATE TABLE versions (
            id TEXT PRIMARY KEY,
            is_active BOOLEAN NOT NULL,
            alias_of_id TEXT REFERENCES versions(id) ON DELETE SET NULL,
            name TEXT NOT NULL,
            source TEXT NOT NULL,
            publisher TEXT,
            locale TEXT,
            internal_id TEXT,
            supports_ot BOOLEAN NOT NULL,
            supports_nt BOOLEAN NOT NULL,
            supports_deu BOOLEAN NOT NULL,
            follows_septuagint BOOLEAN NOT NULL,

            CONSTRAINT cannot_alias_self CHECK (id <> alias_of_id)
        );
    """,
    "books": """
        CREATE TABLE books (
            id SERIAL PRIMARY KEY,
            version_id TEXT REFERENCES versions(id) ON DELETE CASCADE,
            name TEXT NOT NULL,
            proper_name TEXT NOT NULL,
            internal_name TEXT NOT NULL,
            preferred_name TEXT NOT NULL,
            UNIQUE (version_id, name)
        );
    """,
    "chapters": """
        CREATE TABLE chapters (
            id SERIAL PRIMARY KEY,
            book_id INTEGER REFERENCES books(id) ON DELETE CASCADE,
            number INTEGER NOT NULL,
            titles JSONB DEFAULT '[]'::jsonb,
            UNIQUE (book_id, number)
        );
    """,
    "verses": """
        CREATE TABLE verses (
            id SERIAL PRIMARY KEY,
            chapter_id INTEGER REFERENCES chapters(id) ON DELETE CASCADE,
            number INTEGER NOT NULL,
            content TEXT NOT NULL,
            UNIQUE (chapter_id, number)
        );
    """,
    "guilds": """
        CREATE TABLE guilds (
            id BIGINT PRIMARY KEY,
            version TEXT REFERENCES versions(id) ON DELETE SET NULL,
            language TEXT REFERENCES languages(id) ON DELETE SET NULL,
            display_style TEXT,
            ignoring_brackets TEXT,
            dv_channel_id BIGINT,
            dv_webhook TEXT,
            dv_time TEXT,
            dv_timezone TEXT,
            dv_last_sent TEXT,
            dv_role_id BIGINT,
            dv_is_thread BOOLEAN,
            dv_last_status SMALLINT,
            is_dm BOOLEAN NOT NULL
        );
    """,
    "users": """
        CREATE TABLE users (
            id BIGINT PRIMARY KEY,
            version TEXT REFERENCES versions(id) ON DELETE SET NULL,
            language TEXT REFERENCES languages(id) ON DELETE SET NULL,
            display_style TEXT,
            input_method TEXT,
            titles_enabled BOOLEAN NOT NULL DEFAULT TRUE,
            verse_numbers_enabled BOOLEAN NOT NULL DEFAULT TRUE,
            pagination_enabled BOOLEAN NOT NULL DEFAULT FALSE
        );
    """,
    "opt_out_users": """
        CREATE TABLE opt_out_users (
            id BIGINT PRIMARY KEY
        );
    """,
    "opt_out_guilds": """
        CREATE TABLE opt_out_guilds (
            id BIGINT PRIMARY KEY
        );
    """,
    "experiments": """
        CREATE TABLE experiments (
            id TEXT PRIMARY KEY,
            description TEXT,
            type TEXT NOT NULL DEFAULT 'Backend',
            sphere TEXT NOT NULL DEFAULT 'User',
            variants JSONB DEFAULT '[]'::jsonb,
            weights JSONB DEFAULT '[]'::jsonb,
            feedback JSONB DEFAULT '{}'::jsonb
        );
    """
}

postgresPostSchemas = {
    "languages_fk": """
        ALTER TABLE languages ADD COLUMN default_version TEXT REFERENCES versions(id) ON DELETE SET NULL;
    """,
    "indexes": """
        CREATE INDEX idx_versions_id_lower ON versions (lower(id));
        CREATE INDEX idx_users_version ON users (version);
        CREATE INDEX idx_users_language ON users (language);
        CREATE INDEX idx_guilds_daily_verse ON guilds (dv_time, dv_timezone) WHERE dv_webhook IS NOT NULL;
        CREATE INDEX idx_guilds_dv_last_sent ON guilds (dv_last_sent);
        CREATE INDEX idx_verse_metrics_guild_book_chapter ON verse_metrics (guild_id, book, chapter);
        CREATE INDEX idx_verse_metrics_user_book_chapter ON verse_metrics (user_id, book, chapter);
        CREATE INDEX idx_verse_metrics_time_generated ON verse_metrics (time_generated DESC);
        CREATE INDEX idx_appended_verse_metric_id ON appended_verses (verse_metric_id);
    """
}



def setSeptuagintFlag(mongo_db):
    versions = mongo_db.Versions
    for abbv in tqdm(versionsWithLXXPsalms, desc="Setting LXX Flag", bar_format="{desc:<30}: |{bar:20}| {percentage:3.0f}% ({n_fmt}/{total_fmt}) [{remaining}, {rate_fmt}]"):
        result = versions.update_one(
            {"Abbreviation": abbv}, {"$set": {"FollowsSeptuagintNumbering": True}}
        )


def importExperiments(mongo_db):
    experiments = mongo_db.Experiments
    for experiment in tqdm(experimentsToImport, desc="Updating Experiments", bar_format="{desc:<30}: |{bar:20}| {percentage:3.0f}% ({n_fmt}/{total_fmt}) [{remaining}, {rate_fmt}]"):
        result = experiments.update_one(
            {"Name": experiment["Name"]},
            {"$set": experiment},
            upsert=True
        )

def migrateExperimentsToPostgres(mongo_db, pg_conn):
    mongo_experiments = mongo_db.Experiments
    doc_count = mongo_experiments.count_documents({})

    experiment_params = []
    seen_experiments = set()

    for experiment in tqdm(mongo_experiments.find(), total=doc_count, desc="Migrating Experiments", bar_format="{desc:<30}: |{bar:20}| {percentage:3.0f}% ({n_fmt}/{total_fmt}) [{remaining}, {rate_fmt}]"):
        name = experiment.get('Name')
        if not name or name in seen_experiments:
            continue
        seen_experiments.add(name)

        # Map Variants and Weights directly as lists
        variants = experiment.get('Variants', [])
        weights = experiment.get('Weights', [])

        # Map Feedback (Helped/DidNotHelp lists of User IDs)
        helped = []
        for user_id_str in experiment.get('Helped', []):
            try:
                helped.append(int(user_id_str))
            except (ValueError, TypeError):
                continue

        did_not_help = []
        for user_id_str in experiment.get('DidNotHelp', []):
            try:
                did_not_help.append(int(user_id_str))
            except (ValueError, TypeError):
                continue

        feedback_data = {
            "Helped": helped,
            "DidNotHelp": did_not_help
        }

        experiment_params.append((
            name,
            experiment.get('Description'),
            experiment.get('Type') or 'Backend',
            experiment.get('Sphere') or 'User',
            json.dumps(variants),
            json.dumps(weights),
            json.dumps(feedback_data)
        ))

    with pg_conn.cursor() as cur:
        if experiment_params:
            cur.executemany("""
                            INSERT INTO experiments (id, description, type, sphere, variants, weights, feedback)
                            VALUES (%s, %s, %s, %s, %s, %s, %s)
                                ON CONFLICT (id) DO UPDATE SET
                                description = EXCLUDED.description,
                                                        type = EXCLUDED.type,
                                                        sphere = EXCLUDED.sphere,
                                                        variants = EXCLUDED.variants,
                                                        weights = EXCLUDED.weights,
                                                        feedback = EXCLUDED.feedback;
                            """, experiment_params)

    # Note: If MongoDB contains duplicate 'Name' entries, the count might differ due to 'seen_experiments' filtering.
    actual_count = getRowCounts('experiments', pg_conn)
    if len(seen_experiments) != actual_count:
        print(f"experiments processed: {len(seen_experiments)}, experiments in postgres: {actual_count}")
        raise Exception("Experiment count mismatch")

def getRowCounts(table_name, pg_conn):
    with pg_conn.cursor() as cur:
        cur.execute(f"SELECT COUNT(*) FROM {table_name};")
        return cur.fetchone()[0]

def validateTime(time_str):
    if not time_str or not isinstance(time_str, str):
        return None

    # Matches HH:MM or H:MM
    match = re.match(r'^(\d{1,2}):(\d{2})$', time_str)
    if match:
        hours = int(match.group(1))
        minutes = int(match.group(2))
        if 0 <= hours <= 23 and 0 <= minutes <= 59:
            return f"{hours:02d}:{minutes:02d}"

    # If it's something like "808:00", it's probably meant to be "08:00"
    # Let's try to take the last 5 characters if they fit HH:MM
    if len(time_str) > 5:
        sub_time = time_str[-5:]
        match = re.match(r'^(\d{2}):(\d{2})$', sub_time)
        if match:
            hours = int(match.group(1))
            minutes = int(match.group(2))
            if 0 <= hours <= 23 and 0 <= minutes <= 59:
                return sub_time

    return None

def parseDate(date_str):
    if not date_str or not isinstance(date_str, str):
        return None

    # Expected format: 'MM/DD/YYYY'
    match = re.match(r'^(\d{1,2})/(\d{1,2})/(\d{4})$', date_str)
    if match:
        month = int(match.group(1))
        day = int(match.group(2))
        year = int(match.group(3))
        try:
            from datetime import datetime
            return datetime(year, month, day)
        except ValueError:
            return None

    return None

def migrateStatsToPostgres(mongo_db, pg_conn):
    mongo_stats = mongo_db.FrontendStats
    doc_count = mongo_stats.count_documents({})

    if doc_count == 0:
        return

    params = []

    for stat in tqdm(mongo_stats.find(), total=doc_count, desc="Migrating Stats", bar_format="{desc:<30}: |{bar:20}| {percentage:3.0f}% ({n_fmt}/{total_fmt}) [{remaining}, {rate_fmt}]"):
        params.append((
            stat.get('ShardCount'),
            stat.get('ServerCount'),
            stat.get('UserCount'),
            stat.get('ChannelCount'),
            stat.get('FrontendRepoCommitHash'),
            stat.get('UserInstallCount')
        ))

    if params:
        with pg_conn.cursor() as cur:
            cur.executemany("""
                INSERT INTO frontend_stats (
                    shard_count,
                    server_count,
                    user_count,
                    channel_count,
                    commit_hash,
                    user_install_count
                ) VALUES (%s, %s, %s, %s, %s, %s)
                ON CONFLICT DO NOTHING;
            """, params)

def migrateLanguagesToPostgres(mongo_db, pg_conn):
    mongo_languages = mongo_db.Languages
    doc_count = mongo_languages.count_documents({})

    if doc_count == 0:
        return

    params = []
    valid_languages = set()
    for language in tqdm(mongo_languages.find(), total=doc_count, desc="Migrating Languages", bar_format="{desc:<30}: |{bar:20}| {percentage:3.0f}% ({n_fmt}/{total_fmt}) [{remaining}, {rate_fmt}]"):
        culture = language.get('Culture')
        params.append((culture, language.get('Name')))
        valid_languages.add(culture)

    with pg_conn.cursor() as cur:
        for i in range(0, len(params), BATCH_SIZE):
            cur.executemany("""
                INSERT INTO languages (
                    id,
                    name
                ) VALUES (%s, %s)
                ON CONFLICT (id) DO UPDATE SET name = EXCLUDED.name;
            """, params[i:i + BATCH_SIZE])

    if doc_count != getRowCounts('languages', pg_conn):
        print(f"languages in mongodb: {doc_count}, languages in postgres: {getRowCounts('languages', pg_conn)}")
        raise Exception("Language count mismatch")

    return valid_languages

def migrateVersionsToPostgres(mongo_db, pg_conn):
    mongo_versions = mongo_db.Versions
    doc_count = mongo_versions.count_documents({})

    if doc_count == 0:
        return

    version_params = []
    alias_params = []
    versions_data = []
    valid_versions = set()

    for version in tqdm(mongo_versions.find(), total=doc_count, desc="Migrating Versions (Metadata)", bar_format="{desc:<30}: |{bar:20}| {percentage:3.0f}% ({n_fmt}/{total_fmt}) [{remaining}, {rate_fmt}]"):
        versions_data.append(version)
        abbv = version.get('Abbreviation')
        valid_versions.add(abbv)
        version_params.append((
            abbv, version.get('Active'), version.get('Name'),
            version.get('Source'), version.get('Publisher'), version.get('Locale'), version.get('InternalId'), version.get('SupportsOldTestament'),
            version.get('SupportsNewTestament'), version.get('SupportsDeuterocanon'), version.get('FollowsSeptuagintNumbering') or False
        ))
        if version.get('AliasOf') is not None:
            alias_params.append((version.get('AliasOf'), abbv))

    with pg_conn.cursor() as cur:
        for i in range(0, len(version_params), BATCH_SIZE):
            cur.executemany("""
                INSERT INTO versions (
                    id,
                    is_active,
                    name,
                    source,
                    publisher,
                    locale,
                    internal_id,
                    supports_ot,
                    supports_nt,
                    supports_deu,
                    follows_septuagint
                ) VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s)
                ON CONFLICT (id) DO UPDATE SET
                    is_active = EXCLUDED.is_active,
                    name = EXCLUDED.name,
                    source = EXCLUDED.source,
                    publisher = EXCLUDED.publisher,
                    locale = EXCLUDED.locale,
                    internal_id = EXCLUDED.internal_id,
                    supports_ot = EXCLUDED.supports_ot,
                    supports_nt = EXCLUDED.supports_nt,
                    supports_deu = EXCLUDED.supports_deu,
                    follows_septuagint = EXCLUDED.follows_septuagint;
            """, version_params[i:i + BATCH_SIZE])

        if alias_params:
            for i in range(0, len(alias_params), BATCH_SIZE):
                cur.executemany("""
                    UPDATE versions SET alias_of_id = %s WHERE id = %s;
                """, alias_params[i:i + BATCH_SIZE])

    for version in tqdm(versions_data, desc="Migrating Books/Chapters", bar_format="{desc:<30}: |{bar:20}| {percentage:3.0f}% ({n_fmt}/{total_fmt}) [{remaining}, {rate_fmt}]"):
        if version['Books'] is None:
            continue

        book_count = len(version['Books'])
        before_book_count = getRowCounts('books', pg_conn)

        book_params = []
        for book in version.get('Books'):
            book_params.append((
                version.get('Abbreviation'),
                book.get('Name'),
                book.get('ProperName'),
                book.get('InternalName'),
                book.get('PreferredName')
            ))

        book_ids = []
        if book_params:
            with pg_conn.cursor() as cur:
                for params in book_params:
                    cur.execute("""
                        INSERT INTO books (
                            version_id,
                            name,
                            proper_name,
                            internal_name,
                            preferred_name
                        ) VALUES (%s, %s, %s, %s, %s)
                        ON CONFLICT (version_id, name) DO UPDATE SET
                            proper_name = EXCLUDED.proper_name,
                            internal_name = EXCLUDED.internal_name,
                            preferred_name = EXCLUDED.preferred_name
                        RETURNING id;
                    """, params)
                    book_ids.append(cur.fetchone()[0])

        before_chapter_count = getRowCounts('chapters', pg_conn)
        chapter_params = []
        total_chapters_in_mongo = 0

        for i, book in enumerate(version.get('Books')):
            book_id = book_ids[i]
            if book['Chapters'] is None:
                continue

            total_chapters_in_mongo += len(book['Chapters'])
            for chapter in book.get('Chapters'):
                titles_json = json.dumps(chapter.get('Titles') or [])
                chapter_params.append((book_id, chapter.get('Number'), titles_json))

        if chapter_params:
            with pg_conn.cursor() as cur:
                for i in range(0, len(chapter_params), BATCH_SIZE):
                    cur.executemany("""
                        INSERT INTO chapters (
                            book_id,
                            number,
                            titles
                        ) VALUES (%s, %s, %s)
                        ON CONFLICT (book_id, number) DO UPDATE SET
                            titles = EXCLUDED.titles;
                    """, chapter_params[i:i + BATCH_SIZE])

        # Migrate Verses
        before_verse_count = getRowCounts('verses', pg_conn)
        total_verses_in_mongo = 0

        # Fetch chapter IDs from PG for this version's books
        pg_chapters = {} # (book_id, chapter_number) -> chapter_id
        if book_ids:
            with pg_conn.cursor() as cur:
                cur.execute("SELECT id, book_id, number FROM chapters WHERE book_id = ANY(%s)", (book_ids,))
                for cid, bid, num in cur.fetchall():
                    pg_chapters[(bid, num)] = cid

        verse_params = []
        for i, book in enumerate(version.get('Books')):
            book_id = book_ids[i]
            if book['Chapters'] is None:
                continue

            for chapter in book.get('Chapters'):
                chapter_num = chapter.get('Number')
                chapter_id = pg_chapters.get((book_id, chapter_num))
                if not chapter_id:
                    continue

                if chapter.get('Verses') is None:
                    continue

                total_verses_in_mongo += len(chapter.get('Verses'))
                for verse in chapter.get('Verses'):
                    verse_params.append((chapter_id, verse.get('Number'), verse.get('Content')))

                    if len(verse_params) >= BATCH_SIZE:
                        with pg_conn.cursor() as cur:
                            cur.executemany("""
                                INSERT INTO verses (chapter_id, number, content)
                                VALUES (%s, %s, %s)
                                ON CONFLICT (chapter_id, number) DO UPDATE SET
                                    content = EXCLUDED.content;
                            """, verse_params)
                        verse_params = []

        if verse_params:
            with pg_conn.cursor() as cur:
                cur.executemany("""
                    INSERT INTO verses (chapter_id, number, content)
                    VALUES (%s, %s, %s)
                    ON CONFLICT (chapter_id, number) DO UPDATE SET
                        content = EXCLUDED.content;
                """, verse_params)

        if total_chapters_in_mongo != (getRowCounts('chapters', pg_conn) - before_chapter_count):
            print(f"chapters for {version.get('Abbreviation')} in mongodb: {total_chapters_in_mongo}, in postgres: {getRowCounts('chapters', pg_conn) - before_chapter_count}")

        if total_verses_in_mongo != (getRowCounts('verses', pg_conn) - before_verse_count):
            print(f"verses for {version.get('Abbreviation')} in mongodb: {total_verses_in_mongo}, in postgres: {getRowCounts('verses', pg_conn) - before_verse_count}")

        if book_count != (getRowCounts('books', pg_conn) - before_book_count):
            print(f"books for {version.get('Abbreviation')} in mongodb: {book_count}, in postgres: {getRowCounts('books', pg_conn) - before_book_count}")

    if doc_count != getRowCounts('versions', pg_conn):
        print(f"versions in mongodb: {doc_count}, versions in postgres: {getRowCounts('versions', pg_conn)}")
        raise Exception("Version count mismatch")

    return valid_versions

def migrateGuildsToPostgres(mongo_db, pg_conn, valid_languages, valid_versions):
    mongo_guilds = mongo_db.Guilds
    doc_count = mongo_guilds.count_documents({})

    if doc_count == 0:
        return

    params = []
    for guild in tqdm(mongo_guilds.find(), total=doc_count, desc="Migrating Guilds", bar_format="{desc:<30}: |{bar:20}| {percentage:3.0f}% ({n_fmt}/{total_fmt}) [{remaining}, {rate_fmt}]"):
        version = guild.get('Version')
        if version not in valid_versions:
            version = 'RSV'

        language = guild.get('Language')
        if language not in valid_languages:
            language = 'en-US'

        dv_time = validateTime(guild.get('DailyVerseTime'))
        dv_last_sent = parseDate(guild.get('DailyVerseLastSentDate'))

        params.append((
            guild.get('GuildId'),
            version,
            language,
            guild.get('DisplayStyle') or "embed",
            guild.get('IgnoringBrackets') or "<>",
            guild.get('DailyVerseChannelId'),
            guild.get('DailyVerseWebhook'),
            dv_time,
            guild.get('DailyVerseTimeZone'),
            dv_last_sent,
            guild.get('DailyVerseRoleId'),
            guild.get('DailyVerseIsThread'),
            guild.get('DailyVerseLastStatusCode'),
            guild.get('IsDM') or False
        ))

        if len(params) >= BATCH_SIZE:
            with pg_conn.cursor() as cur:
                try:
                    cur.executemany("""
                        INSERT INTO guilds (
                            id, version, language, display_style, ignoring_brackets, dv_channel_id, dv_webhook, dv_time, dv_timezone, dv_last_sent, dv_role_id, dv_is_thread, dv_last_status, is_dm
                        ) VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s)
                        ON CONFLICT (id) DO UPDATE SET
                            version = EXCLUDED.version,
                            language = EXCLUDED.language,
                            display_style = EXCLUDED.display_style,
                            ignoring_brackets = EXCLUDED.ignoring_brackets,
                            dv_channel_id = EXCLUDED.dv_channel_id,
                            dv_webhook = EXCLUDED.dv_webhook,
                            dv_time = EXCLUDED.dv_time,
                            dv_timezone = EXCLUDED.dv_timezone,
                            dv_last_sent = EXCLUDED.dv_last_sent,
                            dv_role_id = EXCLUDED.dv_role_id,
                            dv_is_thread = EXCLUDED.dv_is_thread,
                            dv_last_status = EXCLUDED.dv_last_status,
                            is_dm = EXCLUDED.is_dm;
                    """, params)
                except Exception as e:
                    print(f"Failed to migrate guilds: {e}")
            params = []

    if params:
        with pg_conn.cursor() as cur:
            try:
                cur.executemany("""
                    INSERT INTO guilds (
                        id, version, language, display_style, ignoring_brackets, dv_channel_id, dv_webhook, dv_time, dv_timezone, dv_last_sent, dv_role_id, dv_is_thread, dv_last_status, is_dm
                    ) VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s)
                    ON CONFLICT (id) DO UPDATE SET
                        version = EXCLUDED.version,
                        language = EXCLUDED.language,
                        display_style = EXCLUDED.display_style,
                        ignoring_brackets = EXCLUDED.ignoring_brackets,
                        dv_channel_id = EXCLUDED.dv_channel_id,
                        dv_webhook = EXCLUDED.dv_webhook,
                        dv_time = EXCLUDED.dv_time,
                        dv_timezone = EXCLUDED.dv_timezone,
                        dv_last_sent = EXCLUDED.dv_last_sent,
                        dv_role_id = EXCLUDED.dv_role_id,
                        dv_is_thread = EXCLUDED.dv_is_thread,
                        dv_last_status = EXCLUDED.dv_last_status,
                        is_dm = EXCLUDED.is_dm;
                """, params)
            except Exception as e:
                print(f"Failed to migrate guilds: {e}")

    if doc_count != getRowCounts('guilds', pg_conn):
        print(f"guilds in mongodb: {doc_count}, guilds in postgres: {getRowCounts('guilds', pg_conn)}")
        raise Exception("Guild count mismatch")

def migrateUsersToPostgres(mongo_db, pg_conn, valid_languages, valid_versions):
    mongo_users = mongo_db.Users
    doc_count = mongo_users.count_documents({})

    if doc_count == 0:
        return

    params = []
    for user in tqdm(mongo_users.find(), total=doc_count, desc="Migrating Users", bar_format="{desc:<30}: |{bar:20}| {percentage:3.0f}% ({n_fmt}/{total_fmt}) [{remaining}, {rate_fmt}]"):
        version = user.get('Version')
        if version not in valid_versions:
            version = 'RSV'

        language = user.get('Language')
        if language not in valid_languages:
            language = 'en-US'

        params.append((
            user.get('UserId'),
            version,
            language,
            user.get('DisplayStyle') or "embed",
            user.get('InputMethod') or "default",
            user.get('TitlesEnabled') if user.get('TitlesEnabled') is not None else True,
            user.get('VerseNumbersEnabled') if user.get('VerseNumbersEnabled') is not None else True,
            user.get('PaginationEnabled') if user.get('PaginationEnabled') is not None else False,
        ))

        if len(params) >= BATCH_SIZE:
            with pg_conn.cursor() as cur:
                try:
                    cur.executemany("""
                        INSERT INTO users (
                            id, version, language, display_style, input_method, titles_enabled, verse_numbers_enabled, pagination_enabled
                        ) VALUES (%s, %s, %s, %s, %s, %s, %s, %s)
                        ON CONFLICT (id) DO UPDATE SET
                            version = EXCLUDED.version,
                            language = EXCLUDED.language,
                            display_style = EXCLUDED.display_style,
                            input_method = EXCLUDED.input_method,
                            titles_enabled = EXCLUDED.titles_enabled,
                            verse_numbers_enabled = EXCLUDED.verse_numbers_enabled,
                            pagination_enabled = EXCLUDED.pagination_enabled;
                    """, params)
                except Exception as e:
                    print(f"Failed to migrate users: {e}")
            params = []

    if params:
        with pg_conn.cursor() as cur:
            try:
                cur.executemany("""
                    INSERT INTO users (
                        id, version, language, display_style, input_method, titles_enabled, verse_numbers_enabled, pagination_enabled
                    ) VALUES (%s, %s, %s, %s, %s, %s, %s, %s)
                    ON CONFLICT (id) DO UPDATE SET
                        version = EXCLUDED.version,
                        language = EXCLUDED.language,
                        display_style = EXCLUDED.display_style,
                        input_method = EXCLUDED.input_method,
                        titles_enabled = EXCLUDED.titles_enabled,
                        verse_numbers_enabled = EXCLUDED.verse_numbers_enabled,
                        pagination_enabled = EXCLUDED.pagination_enabled;
                """, params)
            except Exception as e:
                print(f"Failed to migrate users: {e}")

    if doc_count != getRowCounts('users', pg_conn):
        print(f"users in mongodb: {doc_count}, users in postgres: {getRowCounts('users', pg_conn)}")
        raise Exception("User count mismatch")

def migrateOptOutUsersToPostgres(mongo_db, pg_conn):
    mongo_optoutusers = mongo_db.OptOutUsers
    doc_count = mongo_optoutusers.count_documents({})

    if doc_count == 0:
        return

    params = []
    for user in tqdm(mongo_optoutusers.find(), total=doc_count, desc="Migrating Opt-out Users", bar_format="{desc:<30}: |{bar:20}| {percentage:3.0f}% ({n_fmt}/{total_fmt}) [{remaining}, {rate_fmt}]"):
        params.append((
            user.get('UserId'),
        ))

        if len(params) >= BATCH_SIZE:
            with pg_conn.cursor() as cur:
                cur.executemany("""
                    INSERT INTO opt_out_users (id) VALUES (%s)
                    ON CONFLICT (id) DO NOTHING;
                """, params)
            params = []

    if params:
        with pg_conn.cursor() as cur:
            cur.executemany("""
                INSERT INTO opt_out_users (id) VALUES (%s)
                ON CONFLICT (id) DO NOTHING;
            """, params)

    if doc_count != getRowCounts('opt_out_users', pg_conn):
        print(f"optoutusers in mongodb: {doc_count}, optoutusers in postgres: {getRowCounts('opt_out_users', pg_conn)}")
        raise Exception("Opt-out user count mismatch")

def migrateOptOutGuildsToPostgres(mongo_db, pg_conn):
    mongo_optoutguilds = mongo_db.OptOutGuilds
    doc_count = mongo_optoutguilds.count_documents({})

    if doc_count == 0:
        return

    params = []
    for guild in tqdm(mongo_optoutguilds.find(), total=doc_count, desc="Migrating Opt-out Guilds", bar_format="{desc:<30}: |{bar:20}| {percentage:3.0f}% ({n_fmt}/{total_fmt}) [{remaining}, {rate_fmt}]"):
        params.append((
            guild.get('GuildId'),
        ))

        if len(params) >= BATCH_SIZE:
            with pg_conn.cursor() as cur:
                cur.executemany("""
                    INSERT INTO opt_out_guilds (id) VALUES (%s)
                    ON CONFLICT (id) DO NOTHING;
                """, params)
            params = []

    if params:
        with pg_conn.cursor() as cur:
            cur.executemany("""
                INSERT INTO opt_out_guilds (id) VALUES (%s)
                ON CONFLICT (id) DO NOTHING;
            """, params)

    if doc_count != getRowCounts('opt_out_guilds', pg_conn):
        print(f"optoutguilds in mongodb: {doc_count}, optoutguilds in postgres: {getRowCounts('opt_out_guilds', pg_conn)}")
        raise Exception("Opt-out guild count mismatch")


def migrateMongoToPostgres():
    mongo_conn_str = os.environ.get("MONGODB_CONN")
    pg_conn_str = os.environ.get("POSTGRES_CONN")

    mongo_client = pymongo.MongoClient(mongo_conn_str)
    mongo_db = mongo_client.BibleBotBackend

    # Main connection for schema and initial setup
    with psycopg.connect(pg_conn_str) as pg_conn:
        print(f"Connected to {pg_conn.info.dbname}")
        try:
            with pg_conn.cursor() as cur:
                for name, schema in postgresSchemas.items():
                    print(f"Creating postgres table: {name}")
                    try:
                        cur.execute(schema)
                    except Exception as e:
                        print(f"Failed to create '{name}' table: {e}")

                for name, schema in postgresPostSchemas.items():
                    print(f"Applying post-schema: {name}")
                    try:
                        cur.execute(schema)
                    except Exception as e:
                        print(f"Failed to apply '{name}' post-schema: {e}")
            try:
                pg_conn.commit()
            except Exception as e:
                print(f"failed to commit initial setup")

            setSeptuagintFlag(mongo_db)
            importExperiments(mongo_db)

            # Parallel tasks using separate connections for thread safety
            def run_task(func, *args):
                inner_mongo_client = pymongo.MongoClient(mongo_conn_str)
                inner_mongo_db = inner_mongo_client.BibleBotBackend
                try:
                    with psycopg.connect(pg_conn_str) as inner_pg_conn:
                        result = func(inner_mongo_db, inner_pg_conn, *args)
                        inner_pg_conn.commit()
                        return result
                finally:
                    inner_mongo_client.close()

            with ThreadPoolExecutor() as executor:
                future_stats = executor.submit(run_task, migrateStatsToPostgres)
                future_experiments = executor.submit(run_task, migrateExperimentsToPostgres)
                future_optout_users = executor.submit(run_task, migrateOptOutUsersToPostgres)
                future_optout_guilds = executor.submit(run_task, migrateOptOutGuildsToPostgres)
                future_langs = executor.submit(run_task, migrateLanguagesToPostgres)
                future_versions = executor.submit(run_task, migrateVersionsToPostgres)

                future_stats.result()
                future_experiments.result()
                future_optout_users.result()
                future_optout_guilds.result()
                valid_languages = future_langs.result()
                valid_versions = future_versions.result()

            with ThreadPoolExecutor() as executor:
                future_guilds = executor.submit(run_task, migrateGuildsToPostgres, valid_languages, valid_versions)
                future_users = executor.submit(run_task, migrateUsersToPostgres, valid_languages, valid_versions)

                future_guilds.result()
                future_users.result()
        except Exception as e:
            print(f"Migration failed: {e}")
            pg_conn.rollback()
            raise
        finally:
            print("Migration complete")
            mongo_client.close()

if __name__ == "__main__":
    migrateMongoToPostgres()
