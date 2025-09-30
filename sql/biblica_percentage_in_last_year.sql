WITH LastYearReferences AS
    (SELECT *
     FROM verse_metrics
     WHERE time_generated >= now() - interval '1 year' )
SELECT COUNT(*) AS total_references,
       COUNT(CASE
                 WHEN publisher = 'biblica' THEN 1
             END) AS biblica_references,
       ROUND(CAST(COUNT(CASE
                            WHEN publisher = 'biblica' THEN 1
                        END) AS numeric) * 100.0 / NULLIF(COUNT(*), 0), 2) AS biblica_references
FROM LastYearReferences;