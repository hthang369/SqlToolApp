SELECT 
	table_schema as DatabaseName,
	table_name as TableName,
	ROUND(data_length / 1024 / 1024) as 'DataLength (Mb)',
	ROUND(data_free / 1024 / 1024) as 'DataFree (Mb)',
	ROUND(max_data_length / 1024 / 1024) as 'MaxDataLength (Mb)',
	ROUND(index_length / 1024 / 1024) as 'IndexLength (Mb)',
	table_rows as TableRows,
	auto_increment as AutoIncrement
FROM information_schema.tables
WHERE TABLE_SCHEMA NOT IN('information_schema','mysql','performance_schema')
	AND TABLE_SCHEMA = database()
ORDER BY data_length desc,data_free desc