SELECT guild_id,
       book,
       chapter,
       verse,
       COUNT(*) AS reference_count
FROM
    (SELECT guild_id,
            book,
            chapter,
            generate_series(lower(verse_range), upper(verse_range) - 1) AS verse
     FROM verse_metrics
     UNION ALL SELECT m.guild_id,
                      m.book,
                      chapter,
                      generate_series(lower(a.verse_range), upper(a.verse_range) - 1) AS verse
     FROM appended_verses a
     JOIN verse_metrics m ON a.verse_metric_id = m.id) AS all_verses
WHERE guild_id='xxxxxxxx'
GROUP BY guild_id,
         book,
         chapter,
         verse
ORDER BY reference_count DESC