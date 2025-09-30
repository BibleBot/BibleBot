SELECT *
FROM
    (SELECT id,
            user_id,
            guild_id,
            time_generated,
            book,
            chapter,
            version,
            verse_range,
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
                      m.version,
                      a.verse_range,
                      m.publisher,
                      m.is_ot,
                      m.is_nt,
                      m.is_deu
     FROM appended_verses a
     JOIN verse_metrics m ON a.verse_metric_id = m.id) all_verses
ORDER BY id DESC;