SELECT user_id,
       COUNT(*) AS reference_count
FROM
    (SELECT user_id
     FROM verse_metrics
     UNION ALL SELECT m.user_id
     FROM appended_verses a
     JOIN verse_metrics m ON a.verse_metric_id = m.id) AS all_verses
GROUP BY user_id
ORDER BY reference_count DESC