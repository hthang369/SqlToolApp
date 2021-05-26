select client_net_address,local_net_address,connect_time,program_name,hostname,loginame,
db_name(dbid),status
from master.dbo.sysprocesses a
join  sys.dm_exec_connections b on spid = session_id order by spid