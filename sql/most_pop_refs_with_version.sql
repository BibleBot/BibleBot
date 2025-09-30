SELECT book,
       chapter,
       verse,
       version,
       COUNT(*) AS reference_count
FROM
    (SELECT book,
            chapter,
            generate_series(lower(verse_range), upper(verse_range) - 1) AS verse,
            version
     FROM verse_metrics
     UNION ALL SELECT m.book,
                      chapter,
                      generate_series(lower(a.verse_range), upper(a.verse_range) - 1) AS verse,
                      m.version
     FROM appended_verses a
     JOIN verse_metrics m ON a.verse_metric_id = m.id) AS all_verses
GROUP BY book,
         chapter,
         verse,
         version
ORDER BY reference_count DESC