SELECT book,
       COUNT(*) AS reference_count
FROM
    (SELECT book
     FROM verse_metrics
     UNION ALL SELECT m.book
     FROM appended_verses a
     JOIN verse_metrics m ON a.verse_metric_id = m.id) AS all_verses
GROUP BY book
ORDER BY reference_count DESC