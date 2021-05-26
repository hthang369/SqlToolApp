--ALTER TABLE [dbo].[CSCompanys] DROP	CONSTRAINT [FK_CSCompanys_ICStocks]
DECLARE @table_name SYSNAME
DECLARE @column_name SYSNAME
DECLARE 
      @object_name SYSNAME
    , @object_id INT

SET @table_name = '@TableName@'
SET @column_name = '@TableName@'

IF (@table_name IS NOT NULL OR @table_name <> '') AND (@column_name IS NOT NULL OR @column_name <> '')
BEGIN
	SELECT 
			  @object_name = '[' + s.name + '].[' + t.name + ']'
			, @object_id = t.[object_id]
		FROM sys.objects t WITH (NOWAIT)
		JOIN sys.schemas s WITH (NOWAIT) ON t.[schema_id] = s.[schema_id]
		WHERE t.name LIKE @table_name
			AND t.[type] IN('U')

	DECLARE @SQLADD NVARCHAR(MAX) = ''
	DECLARE @SQLDEL NVARCHAR(MAX) = ''
	DECLARE @SQLFK NVARCHAR(MAX) = ''
	SELECT @SQLADD = 'ALTER TABLE ' + @object_name + CHAR(9) + 'AND' + CHAR(9) + STUFF((
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
							--CASE WHEN c.collation_name IS NOT NULL THEN ' COLLATE ' + c.collation_name ELSE '' END +
							CASE WHEN c.is_nullable = 1 THEN ' NULL' ELSE ' NOT NULL' END +
							CASE WHEN dc.[definition] IS NOT NULL THEN ' DEFAULT' + dc.[definition] ELSE '' END + 
							CASE WHEN ic.is_identity = 1 THEN ' IDENTITY(' + CAST(ISNULL(ic.seed_value, '0') AS CHAR(1)) + ',' + CAST(ISNULL(ic.increment_value, '1') AS CHAR(1)) + ')' ELSE '' END 
					END + CHAR(13)
				FROM sys.columns c WITH (NOWAIT)
				JOIN sys.types tp WITH (NOWAIT) ON c.user_type_id = tp.user_type_id
				LEFT JOIN sys.computed_columns cc WITH (NOWAIT) ON c.[object_id] = cc.[object_id] AND c.column_id = cc.column_id
				LEFT JOIN sys.default_constraints dc WITH (NOWAIT) ON c.default_object_id != 0 AND c.[object_id] = dc.parent_object_id AND c.column_id = dc.parent_column_id
				LEFT JOIN sys.identity_columns ic WITH (NOWAIT) ON c.is_identity = 1 AND c.[object_id] = ic.[object_id] AND c.column_id = ic.column_id
				WHERE c.[object_id] = @object_id AND c.[name] = @column_name
				ORDER BY c.column_id
				FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, CHAR(9) + ' ')

	SET @SQLDEL = 'ALTER TABLE ' + @object_name + CHAR(9) + 'DROP COLUMN' + CHAR(9) + QUOTENAME(@column_name)
	select @SQLADD,@SQLDEL
END
