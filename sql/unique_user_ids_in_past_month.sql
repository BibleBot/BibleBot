SELECT COUNT(DISTINCT user_id) AS unique_user_count
FROM verse_metrics
WHERE time_generated >= NOW() - INTERVAL '1 month';