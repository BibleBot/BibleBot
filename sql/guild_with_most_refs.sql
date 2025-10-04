SELECT guild_id,
       COUNT(*) AS reference_count
FROM
    (SELECT guild_id
     FROM verse_metrics
     UNION ALL SELECT m.guild_id
     FROM appended_verses a
     JOIN verse_metrics m ON a.verse_metric_id = m.id) AS all_verses
GROUP BY guild_id
ORDER BY reference_count DESC