DECLARE @ModuleName nvarchar(max)
SET @ModuleName = N'@ModuleName@'
EXEC('SELECT a.STModuleID,STModuleName,STModuleDescriptionDescription,STModuleType
FROM STModules a 
	JOIN STModuleDescriptions b ON a.STModuleID = b.STModuleID
WHERE (STModuleName like N''%' + @ModuleName + '%'' OR STModuleDescriptionDescription like N''%' + @ModuleName + '%'')')