﻿SELECT
    col.RDB$FIELD_NAME          AS COLUMN_NAME,
    col.RDB$FIELD_POSITION + 1  AS ORDINAL_POSITION,
    fld.RDB$FIELD_TYPE          AS DATA_TYPE,
    fld.RDB$FIELD_LENGTH        AS CHARACTER_MAXIMUM_LENGTH,
    fld.RDB$FIELD_PRECISION     AS NUMERIC_PRECISION,
    fld.RDB$FIELD_SCALE         AS NUMERIC_SCALE,
    CASE WHEN col.RDB$NULL_FLAG IS NULL THEN 'YES' ELSE 'NO' END
                                AS IS_NULLABLE,
    (SELECT FIRST 1 idx.RDB$UNIQUE_FLAG
     FROM RDB$INDICES idx
     INNER JOIN RDB$INDEX_SEGMENTS idxs ON idxs.RDB$INDEX_NAME = idx.RDB$INDEX_NAME
     WHERE idx.RDB$RELATION_NAME = col.RDB$RELATION_NAME
     AND   idxs.RDB$FIELD_NAME = col.RDB$FIELD_NAME)
                                AS IS_IDENTITY,
    (SELECT FIRST 1 con.RDB$CONSTRAINT_TYPE
     FROM RDB$RELATION_CONSTRAINTS con
     INNER JOIN RDB$INDEX_SEGMENTS idxs ON idxs.RDB$INDEX_NAME = con.RDB$INDEX_NAME
     WHERE con.RDB$RELATION_NAME = col.RDB$RELATION_NAME
     AND   idxs.RDB$FIELD_NAME = col.RDB$FIELD_NAME)
                                AS CONSTRAINT_TYPE
FROM
    RDB$RELATION_FIELDS col
LEFT JOIN
    RDB$FIELDS fld ON fld.RDB$FIELD_NAME = col.RDB$FIELD_NAME
WHERE
    col.RDB$RELATION_NAME = '@TABLE_NAME'
ORDER BY
    col.RDB$FIELD_POSITION
