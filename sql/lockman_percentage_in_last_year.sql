WITH LastYearReferences AS
    (SELECT *
     FROM verse_metrics
     WHERE time_generated >= now() - interval '1 year' )
SELECT COUNT(*) AS total_references,
       COUNT(CASE
                 WHEN publisher = 'lockman' THEN 1
             END) AS lockman_references,
       ROUND(CAST(COUNT(CASE
                            WHEN publisher = 'lockman' THEN 1
                        END) AS numeric) * 100.0 / NULLIF(COUNT(*), 0), 2) AS lockman_references
FROM LastYearReferences;