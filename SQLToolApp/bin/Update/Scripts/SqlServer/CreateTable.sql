DECLARE @table_name SYSNAME
DECLARE 
      @object_name SYSNAME
    , @object_id INT

SELECT @table_name = '@tablename@'
SELECT 
		  @object_name = '[' + s.name + '].[' + t.name + ']'
		, @object_id = t.[object_id]
	FROM [@schemaname@].sys.objects t WITH (NOWAIT)
	JOIN [@schemaname@].sys.schemas s WITH (NOWAIT) ON t.[schema_id] = s.[schema_id]
	WHERE t.name like @table_name
		AND t.[type] IN('U')
DECLARE @SQL NVARCHAR(MAX) = ''
SELECT @SQL = 'CREATE TABLE ' + @object_name + CHAR(13) + '(' + CHAR(13) + STUFF((
			SELECT CHAR(9) + ', [' + c.name + '] ' + 
				CASE WHEN c.is_computed = 1
					THEN 'AS ' + cc.[definition] 
					ELSE UPPER(tp.name) + 
						CASE WHEN tp.name IN ('varchar', 'char', 'varbinary', 'binary', 'text')
							   THEN '(' + CASE WHEN c.max_length = -1 THEN 'MAX' ELSE CAST(c.max_length AS VARCHAR(5)) END + ')'
							 WHEN tp.name IN ('nvarchar', 'nchar', 'ntext')
							   THEN '(' + CASE WHEN c.max_length = -1 THEN 'MAX' ELSE CAST(c.max_length / 2 AS VARCHAR(5)) END + ')'
							 WHEN tp.name IN ('datetime2', 'time2', 'datetimeoffset') 
							   THEN '(' + CAST(c.scale AS VARCHAR(5)) + ')'
							 WHEN tp.name = 'decimal' 
							   THEN '(' + CAST(c.[precision] AS VARCHAR(5)) + ',' + CAST(c.scale AS VARCHAR(5)) + ')'
							ELSE ''
						END +
						CASE WHEN c.collation_name IS NOT NULL THEN ' COLLATE ' + c.collation_name ELSE '' END +
						CASE WHEN c.is_nullable = 1 THEN ' NULL' ELSE ' NOT NULL' END +
						CASE WHEN dc.[definition] IS NOT NULL THEN ' DEFAULT' + dc.[definition] ELSE '' END + 
						CASE WHEN ic.is_identity = 1 THEN ' IDENTITY(' + CAST(ISNULL(ic.seed_value, '0') AS CHAR(1)) + ',' + CAST(ISNULL(ic.increment_value, '1') AS CHAR(1)) + ')' ELSE '' END 
				END + CHAR(13)
			FROM [@schemaname@].sys.columns c WITH (NOWAIT)
			JOIN [@schemaname@].sys.types tp WITH (NOWAIT) ON c.user_type_id = tp.user_type_id
			LEFT JOIN [@schemaname@].sys.computed_columns cc WITH (NOWAIT) ON c.[object_id] = cc.[object_id] AND c.column_id = cc.column_id
			LEFT JOIN [@schemaname@].sys.default_constraints dc WITH (NOWAIT) ON c.default_object_id != 0 AND c.[object_id] = dc.parent_object_id AND c.column_id = dc.parent_column_id
			LEFT JOIN [@schemaname@].sys.identity_columns ic WITH (NOWAIT) ON c.is_identity = 1 AND c.[object_id] = ic.[object_id] AND c.column_id = ic.column_id
			WHERE c.[object_id] = @object_id
			ORDER BY c.column_id
			FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, CHAR(9) + ' ')
			+ ISNULL((SELECT CHAR(9) + ', CONSTRAINT [' + k.name + '] PRIMARY KEY (' + 
							(SELECT STUFF((
								 SELECT ', [' + c.name + '] ' + CASE WHEN ic.is_descending_key = 1 THEN 'DESC' ELSE 'ASC' END
								 FROM [@schemaname@].sys.index_columns ic WITH (NOWAIT)
								 JOIN [@schemaname@].sys.columns c WITH (NOWAIT) ON c.[object_id] = ic.[object_id] AND c.column_id = ic.column_id
								 WHERE ic.is_included_column = 0
									 AND ic.[object_id] = k.parent_object_id 
									 AND ic.index_id = k.unique_index_id     
								 FOR XML PATH(N''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, ''))
					+ ')' + CHAR(13)
					FROM [@schemaname@].sys.key_constraints k WITH (NOWAIT)
					WHERE k.parent_object_id = @object_id 
						AND k.[type] = 'PK'), '') + ')'  + CHAR(13)

						SELECT @SQL