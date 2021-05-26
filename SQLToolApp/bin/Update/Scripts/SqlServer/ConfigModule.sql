declare @module_name SYSNAME
declare @maxid int
set @module_name = '@ModuleName@'

select @maxid = isnull(max(CSCompanyModuleID), 0) from CSCompanyModules

insert into CSCompanyModules select @maxid + 1,STModuleID, STModuleName from STModules where STModuleName like @module_name

select * from CSCompanyModules