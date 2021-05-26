IF OBJECT_ID('tempdb..#View') IS NOT NULL drop table #View
SELECT t.name TableName,c.name ColumnName,tr.name ReferenceTableName,cs.name ReferenceColumnName
INTO #View
FROM sys.tables t JOIN sys.foreign_keys f ON t.OBJECT_ID = f.parent_object_id
JOIN sys.tables tr ON tr.OBJECT_ID = f.referenced_object_id
JOIN sys.foreign_key_columns fc ON f.OBJECT_ID = fc.constraint_object_id
JOIN sys.columns c ON c.OBJECT_ID = fc.parent_object_id AND c.column_id = fc.parent_column_id
JOIN sys.columns cs ON cs.OBJECT_ID = fc.referenced_object_id AND cs.column_id = fc.referenced_column_id

update STGridColumns set STGridColumnRepository = ReferenceTableName
from STGridColumns a
join #View b on a.STGridColumnTableName = b.TableName and a.STGridColumnName = b.ColumnName
where STGridColumnName like 'FK_%' and STGridColumnRepository = ''