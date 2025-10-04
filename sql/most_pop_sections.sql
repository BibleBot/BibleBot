SELECT 'is_ot' AS category,
       COUNT(*) AS reference_count
FROM verse_metrics
WHERE is_ot
UNION ALL
SELECT 'is_nt',
       COUNT(*)
FROM verse_metrics
WHERE is_nt
UNION ALL
SELECT 'is_deu',
       COUNT(*)
FROM verse_metrics
WHERE is_deu
ORDER BY reference_count DESC;