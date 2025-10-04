SELECT version,
       publisher,
       COUNT(*) AS reference_count
FROM
    (SELECT version,
            publisher
     FROM verse_metrics
     UNION ALL SELECT m.version, m.publisher
     FROM appended_verses a
     JOIN verse_metrics m ON a.verse_metric_id = m.id) AS all_verses
GROUP BY version, publisher
ORDER BY reference_count DESC