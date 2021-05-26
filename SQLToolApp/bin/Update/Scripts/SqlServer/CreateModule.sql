--select * from sys.tables where name like 'STModule%'

DECLARE @maxID int
DECLARE @maxDescID int
DECLARE @strModuleName NVARCHAR(150)
DECLARE @strModuleCode NVARCHAR(150)
DECLARE @strModuleDesc NVARCHAR(150)
SET @strModuleName = '@ModuleName@'
SET @strModuleCode = '@ModuleCode@'
SET @strModuleDesc = N'@ModuleDesc@'

IF @strModuleName = '' --OR @strModuleName = '@ModuleName@'
	RAISERROR( 'Vui lòng nhập tên module',16,1)
ELSE IF @strModuleCode = '' --OR @strModuleCode = '@ModuleCode@'
	RAISERROR( 'Vui lòng nhập module code',16,1)
ELSE IF @strModuleDesc = '' --OR @strModuleDesc = '@ModuleDesc@'
	RAISERROR('Vui lòng nhập diễn giải',16,1)

SELECT @maxID = MAX(STModuleID) FROM STModules
SELECT @maxDescID = MAX(STModuleDescriptionID) FROM STModuleDescriptions

--select * from STModuleDescriptions

INSERT INTO STModules(STModuleID,STModuleName,STModuleCode,STModuleMain,STModuleDesc) VALUES (@maxID + 1,@strModuleName,@strModuleCode,0,@strModuleDesc)
INSERT INTO STModuleDescriptions(STModuleDescriptionID,STModuleID,STLanguageID,STModuleDescriptionDescription) VALUES (@maxDescID + 1,@maxID + 1,1,@strModuleDesc)
