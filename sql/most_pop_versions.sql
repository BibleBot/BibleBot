SELECT version,
       COUNT(*) AS reference_count
FROM
    (SELECT version
     FROM verse_metrics
     UNION ALL SELECT m.version
     FROM appended_verses a
     JOIN verse_metrics m ON a.verse_metric_id = m.id) AS all_verses
GROUP BY version
ORDER BY reference_count DESC