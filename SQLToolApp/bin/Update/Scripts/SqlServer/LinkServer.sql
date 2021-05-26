---- create linked server
if EXISTS(select * from sys.servers where name = '@serverName@')
begin
--- xóa link srv login from
exec sp_droplinkedsrvlogin @rmtsrvname = '@serverName@', @locallogin=null
--- xóa link server from
exec sp_dropserver '@serverName@'
end

if NOT EXISTS(select * from sys.servers where name = '@serverName@')
begin
--- add link server from
EXEC master.dbo.sp_addlinkedserver @server = '@serverName@', @provider=N'SQLOLEDB', @datasrc = '@serverAddress@', @srvproduct = '@serverName@'
--- add link srv login from
EXEC master.dbo.sp_addlinkedsrvlogin @rmtsrvname = '@serverName@', @useself=N'False', @locallogin=NULL, @rmtuser='@serverUser@', @rmtpassword='@serverPass@'
end