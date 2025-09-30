SELECT *
FROM
    (SELECT id,
            user_id,
            guild_id,
            time_generated,
            book,
            chapter,
            generate_series(lower(verse_range), upper(verse_range) - 1) AS verse,
            version,
            publisher,
            is_ot,
            is_nt,
            is_deu
     FROM verse_metrics
     UNION ALL SELECT m.id,
                      m.user_id,
                      m.guild_id,
                      m.time_generated,
                      m.book,
                      m.chapter,
                      generate_series(lower(a.verse_range), upper(a.verse_range) - 1) AS verse,
                      m.version,
                      m.publisher,
                      m.is_ot,
                      m.is_nt,
                      m.is_deu
     FROM appended_verses a
     JOIN verse_metrics m ON a.verse_metric_id = m.id) all_verses
ORDER BY id DESC;