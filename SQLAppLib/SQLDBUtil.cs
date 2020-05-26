using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;
using System.Windows.Forms;
using System.Reflection;
using System.Diagnostics;
using MySql.Data.MySqlClient;

namespace SQLAppLib
{
    public enum SqlDbConnectionType
    {
        SqlServer,
        MySql
    }
    public class SqlDbConnection : ICloneable
    {
        private DbConnection _dbConnection;
        private SqlDbConnectionType _connectionType;
        public SqlDbConnection(SqlDbConnectionType connectionType, string connectionString)
        {
            _connectionType = connectionType;
            switch (connectionType)
            {
                case SqlDbConnectionType.MySql:
                    _dbConnection = new MySqlConnection(connectionString);
                    break;
                default:
                    _dbConnection = new SqlConnection(connectionString);
                    break;
            }
        }
        public DbConnection Connection
        {
            get { return _dbConnection; }
        }
        public DbDataAdapter CreateDataAdapter()
        {
            DbDataAdapter dataAdapter = null;
            switch (_connectionType)
            {
                case SqlDbConnectionType.MySql:
                    dataAdapter = new MySqlDataAdapter();
                    break;
                default:
                    dataAdapter = new SqlDataAdapter();
                    break;
            }
            return dataAdapter;
        }
        public DbCommand CreateDbCommand(string strQueryCommand)
        {
            DbCommand command = null;
            switch (_connectionType)
            {
                case SqlDbConnectionType.MySql:
                    command = new MySqlCommand(strQueryCommand, Connection as MySqlConnection);
                    break;
                default:
                    command = new SqlCommand(strQueryCommand, Connection as SqlConnection);
                    break;
            }
            return command;
        }
        public void DeriveParameters(DbCommand command)
        {
            switch (_connectionType)
            {
                case SqlDbConnectionType.MySql:
                    MySqlCommandBuilder.DeriveParameters(command as MySqlCommand);
                    break;
                default:
                    SqlCommandBuilder.DeriveParameters(command as SqlCommand);
                    break;
            }
        }
        public DbTransaction BeginTransaction()
        {
            if (Connection is MySqlConnection)
                return (Connection as MySqlConnection).BeginTransaction();
            return (Connection as SqlConnection).BeginTransaction();
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
    public class SqlDatabaseHelper
    {
        private static string _companyName = string.Empty;
        private static string _connectionString = string.Empty;
        public static string AAStatusColumn = "AAStatus";
        private static Timer autoTestConTimer;
        public static SqlDbConnection CurrentDatabase = null;
        private static Hashtable GMCDatabaseCollection = new Hashtable();
        public static DbTransaction Transaction = null;
        private static Dictionary<string, DataSet> lstForeignColumns = new Dictionary<string, DataSet>();
        private static Dictionary<string, string> lstPrimaryColumns = new Dictionary<string, string>();
        private static Dictionary<string, DbCommand> MaxIDDbCommandList = new Dictionary<string, DbCommand>();
        public static Dictionary<string, DbCommand> StoredProCommandList = new Dictionary<string, DbCommand>();
        public static Dictionary<string, DataTable> TableColumnList = new Dictionary<string, DataTable>();
        public static string TableName = string.Empty;
        public static string _strServer = string.Empty;
        public static string _strDatabase = string.Empty;
        public static string _strUser = string.Empty;
        public static string _strPass = string.Empty;
        private static SqlDbConnectionType _connectionType;

        static SqlDatabaseHelper()
        {
            try
            {
                _connectionType = SqlDbConnectionType.SqlServer;
                CurrentDatabase = new SqlDbConnection(_connectionType, GetConnectionString(_connectionType, _strServer, _strDatabase, _strUser, _strPass));
            }
            catch (Exception exception)
            {
                MessageBox.Show("Can not connect to server - " + _strServer, "Messenger", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public static void SetSqlDbConnectionType(SqlDbConnectionType connectionType)
        {
            _connectionType = connectionType;
        }
        private static void AutoTestConnectionTimer_Tick(object sender, EventArgs e)
        {
            Application.DoEvents();
            //if (TestConnection())
            //{
            //    //GMCWaitingDialog.Close();
            //    autoTestConTimer.Enabled = false;
            //}
        }
        protected static DbTransaction BeginTransaction()
        {
            Transaction = CurrentDatabase.BeginTransaction();
            return Transaction;
        }
        protected static void CommitTransaction(DbTransaction transaction)
        {
            if (transaction != null)
                transaction.Commit();
        }
        public static string GetConnectionString()
        {
            return _connectionString;
        }
        public static string GetConnectionString(SqlDbConnectionType connectionType, String strServer, String strDatabase, String strUsername, String strPassword)
        {
            //return string.Format("Server={0};Database={1};User Id={2};Password={3};", strServer, strDatabase, strUsername, strPassword);
            switch (connectionType)
            {
                case SqlDbConnectionType.MySql:
                    string[] lstServer = strServer.Split(':');
                    _connectionString = string.Format("Server={0};port={1};Database={2};User Id={3};password={4};Convert Zero Datetime=True", lstServer.FirstOrDefault(), lstServer.LastOrDefault(), strDatabase, strUsername, strPassword);
                    break;
                default:
                    _connectionString = string.Format("data source={0};database={1};uid={2};pwd={3}", strServer, strDatabase, strUsername, strPassword);
                    break;
            }
            return _connectionString;
        }
        protected static string[] GetParameters(string strQueryCommand)
        {
            string[] array = new string[0];
            string whereClause = GetWhereClause(strQueryCommand);
            if (!string.IsNullOrEmpty(whereClause))
            {
                do
                {
                    whereClause = whereClause.Substring(whereClause.IndexOf("@"));
                    string str2 = whereClause.Substring(1, whereClause.IndexOf(")") - 1);
                    Array.Resize<string>(ref array, array.Length + 1);
                    array[array.Length - 1] = str2;
                    whereClause = whereClause.Substring(str2.Length + 1);
                    if (whereClause.StartsWith(")"))
                    {
                        whereClause = whereClause.Substring(1);
                    }
                }
                while (whereClause.Contains("@"));
                return array;
            }
            return null;
        }
        protected static DbCommand GetQuery(string strQueryCommand)
        {
            try
            {
                return CurrentDatabase.CreateDbCommand(strQueryCommand);
            }
            catch
            {
                return null;
            }
        }
        protected static DbCommand GetStoredProcedure(string spName, params object[] values)
        {
            try
            {
                DbCommand command = CurrentDatabase.CreateDbCommand(spName);
                command.CommandType = CommandType.StoredProcedure;

                CurrentDatabase.DeriveParameters(command);
                command.Parameters.Remove(command.Parameters["@RETURN_VALUE"]);

                // Add the input parameter and set value
                for (int iIndex = 0; iIndex < command.Parameters.Count; iIndex++)
                {
                    if (values.Length > iIndex)
                    {
                        command.Parameters[iIndex].Value = values[iIndex];
                    }
                }
                return command;
            }
            catch (Exception exception)
            {
                if (!(exception is SqlException) || ((((SqlException)exception).ErrorCode != -2146232060) || !exception.Message.Contains("TCP Provider")))
                {
                    MessageBox.Show(exception.Message);
                }
                return null;
            }
        }
        private static string GetWhereClause(string strQueryCommand)
        {
            if (strQueryCommand.Contains("WHERE"))
            {
                return strQueryCommand.Substring(strQueryCommand.IndexOf("WHERE"));
            }
            return string.Empty;
        }
        private static void GMCWaitingDialog_StopDialogEvent()
        {
            Process.GetCurrentProcess().Kill();
        }
        protected static void RollbackTransaction(DbTransaction transaction)
        {
            if (transaction != null)
                transaction.Rollback();
        }
        protected static DataSet RunQuery(string strQuery)
        {
            return RunQuery(GetQuery(strQuery));
        }
        protected static DataTable RunQueryTable(string strQuery)
        {
            return RunQueryTable(GetQuery(strQuery));
        }
        protected static DataSet RunQuery(DbCommand cmd)
        {
            try
            {
                if (CurrentDatabase == null)
                    SwitchConnection(_connectionType, _connectionString);
                if (CurrentDatabase.Connection.State != ConnectionState.Open)
                    CurrentDatabase.Connection.Open();
                Transaction = BeginTransaction();
                cmd.Transaction = Transaction;
                DataSet ds = new DataSet();
                DbDataAdapter adapter = CurrentDatabase.CreateDataAdapter();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds);
                CommitTransaction(Transaction);
                CurrentDatabase.Connection.Close();
                return ds;
            }
            catch (Exception exception)
            {
                RollbackTransaction(Transaction);
                CurrentDatabase.Connection.Close();
                MessageBox.Show(exception.Message);
                return null;
            }
        }

        protected static DataTable RunQueryTable(DbCommand cmd)
        {
            try
            {
                if (CurrentDatabase == null)
                    SwitchConnection(_connectionType, _connectionString);
                if (CurrentDatabase.Connection.State != ConnectionState.Open)
                    CurrentDatabase.Connection.Open();
                Transaction = BeginTransaction();
                cmd.Transaction = Transaction;
                DataTable dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
                //Task.Run(delegate
                //{
                //    using (DbDataReader read = cmd.ExecuteReader())
                //    {
                //        for (int i = 0; i < read.FieldCount; i++)
                //        {
                //            DataColumn col = new DataColumn(read.GetName(i), read.GetFieldType(i));
                //            dt.Columns.Add(col);
                //        }
                //        while (read.Read())
                //        {
                //            object[] values = new object[read.FieldCount];
                //            read.GetValues(values);
                //            dt.LoadDataRow(values, false);
                //        }
                //    }

                //        return dt;
                //}).Wait();
                CommitTransaction(Transaction);
                CurrentDatabase.Connection.Close();
                return dt;
            }
            catch (Exception exception)
            {
                RollbackTransaction(Transaction);
                CurrentDatabase.Connection.Close();
                MessageBox.Show(exception.Message);
                return null;
            }
        }

        protected static int RunQueryNonDataSet(DbCommand cmd)
        {
            try
            {
                if (CurrentDatabase == null)
                    SwitchConnection(_connectionType, _connectionString);
                CurrentDatabase.Connection.Open();
                Transaction = BeginTransaction();
                cmd.Transaction = Transaction;
                int idx = cmd.ExecuteNonQuery();
                CommitTransaction(Transaction);
                CurrentDatabase.Connection.Close();
                return idx;
            }
            catch (Exception exception)
            {
                RollbackTransaction(Transaction);
                CurrentDatabase.Connection.Close();
                if (!(exception is SqlException) || ((((SqlException)exception).ErrorCode != -2146232060) || !exception.Message.Contains("TCP Provider")))
                {
                    MessageBox.Show(exception.Message);
                }
                else if (!(exception is MySqlException) || (!exception.Message.Contains("TCP Provider")))
                {
                    MessageBox.Show(exception.Message);
                }
                return 0;
            }
        }
        protected static DataSet RunStoredProcedure(DbCommand cmd)
        {
            try
            {
                return RunQuery(cmd);
            }
            catch (Exception exception)
            {
                if ((exception is SqlException) && ((((SqlException)exception).ErrorCode == -2146232060) && exception.Message.Contains("TCP Provider")))
                {
                    return null;
                }
                else if (!(exception is MySqlException) || (!exception.Message.Contains("TCP Provider")))
                {
                    MessageBox.Show(exception.Message);
                }
                return null;
            }
        }
        protected static DataSet RunStoredProcedure(string spName)
        {
            try
            {
                DbCommand storedProcCommand = GetStoredProcedure(spName);
                return RunQuery(storedProcCommand);
            }
            catch (Exception exception)
            {
                if (!(exception is SqlException) || ((((SqlException)exception).ErrorCode != -2146232060) || !exception.Message.Contains("TCP Provider")))
                {
                    MessageBox.Show(exception.Message);
                }
                else if (!(exception is MySqlException) || (!exception.Message.Contains("TCP Provider")))
                {
                    MessageBox.Show(exception.Message);
                }
                return null;
            }
        }
        protected static DataSet RunStoredProcedure(string spName, params object[] values)
        {
            try
            {
                return RunQuery(GetStoredProcedure(spName, values));
            }
            catch (Exception exception)
            {
                if (!(exception is SqlException) || ((((SqlException)exception).ErrorCode != -2146232060) || !exception.Message.Contains("TCP Provider")))
                {
                    MessageBox.Show(exception.Message);
                }
                else if (!(exception is MySqlException) || (!exception.Message.Contains("TCP Provider")))
                {
                    MessageBox.Show(exception.Message);
                }
                return null;
            }
        }
        public static void SwitchConnection(SqlDbConnectionType connectionType, string strConnectionString)
        {
            if (CurrentDatabase == null)
                CurrentDatabase = new SqlDbConnection(connectionType, strConnectionString);
            else
                CurrentDatabase.Connection.ConnectionString = strConnectionString;
        }
        public static void SwitchConnection(SqlDbConnectionType connectionType, String strServers, String strDatabases, String strUsernames, String strPasswords)
        {
            try
            {
                _connectionType = connectionType;
                if (!string.IsNullOrEmpty(strServers) || strServers != _strServer) _strServer = strServers;
                if (!string.IsNullOrEmpty(strDatabases) || strDatabases != _strDatabase) _strDatabase = strDatabases;
                if (!string.IsNullOrEmpty(strUsernames) || strUsernames != _strUser) _strUser = strUsernames;
                if (!string.IsNullOrEmpty(strPasswords) || strPasswords != _strPass) _strPass = strPasswords;
                if (CurrentDatabase == null)
                    CurrentDatabase = new SqlDbConnection(connectionType, GetConnectionString(connectionType, _strServer, _strDatabase, _strUser, _strPass));
                else if(!CurrentDatabase.Connection.GetType().Name.StartsWith(SqlDbConnectionType.MySql.ToString()))
                    CurrentDatabase = new SqlDbConnection(connectionType, GetConnectionString(connectionType, _strServer, _strDatabase, _strUser, _strPass));
                else if (!CurrentDatabase.Connection.GetType().Name.StartsWith("Sql"))
                    CurrentDatabase = new SqlDbConnection(connectionType, GetConnectionString(connectionType, _strServer, _strDatabase, _strUser, _strPass));
                else if (CurrentDatabase.Connection.State == ConnectionState.Closed)
                    CurrentDatabase.Connection.ConnectionString = GetConnectionString(connectionType, _strServer, _strDatabase, _strUser, _strPass);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can't not change connection!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public static void ChangeDatabase(SqlDbConnectionType connectionType, string strDBName)
        {
            try
            {
                _strDatabase = strDBName;
                SwitchConnection(connectionType, _strServer, strDBName, _strUser, _strPass);
                if (CurrentDatabase.Connection.State != ConnectionState.Open)
                    CurrentDatabase.Connection.Open();
                CurrentDatabase.Connection.ChangeDatabase(strDBName);
            }
            catch (Exception e) { }
        }
        public static Dictionary<string, DataSet> ForeignColumnsList
        {
            get { return lstForeignColumns; }
        }
        public static Dictionary<string, string> PrimaryColumnsList
        {
            get { return lstPrimaryColumns; }
        }
        public static SqlDbConnectionType ConnectionType
        {
            get => _connectionType;
        }
        public static void ChangeConnection(SqlDbConnectionType connectionType, String strServers, String strUsernames, String strPasswords)
        {
            _strDatabase = string.Empty;
            _connectionType = connectionType;
            if (CurrentDatabase != null && CurrentDatabase.Connection.State == ConnectionState.Open)
                CurrentDatabase.Connection.Close();
            SwitchConnection(connectionType, strServers, "", strUsernames, strPasswords);
        }
    }
    
    public class SQLDBUtil : SqlDatabaseHelper
    {
        private static Dictionary<string, DataTable> TableColumnCaptionList = new Dictionary<string, DataTable>();
        public static int ExecuteNonQuery(string strQuery)
        {
            return RunQueryNonDataSet(GetQuery(strQuery));
        }
        public static DataTable GetDataTable(string strQuery)
        {
            DataTable dt = RunQueryTable(strQuery);
            return dt;
        }
        public static DataSet GetDataSet(string strQuery)
        {
            return RunQuery(strQuery);
        }
        public static string GetCurrentDatabaseName()
        {
            DataTable dt = GetDataTable("SELECT DB_NAME()");
            if (dt != null && dt.Rows.Count > 0)
                return Convert.ToString(dt.Rows[0][0]);
            return string.Empty;
        }
        public static DataSet GetAllDatabases()
        {
            string strQuery = string.Empty;
            string strTable = string.Empty;
            switch (ConnectionType)
            {
                case SqlDbConnectionType.MySql:
                    strTable = "mysql";
                    strQuery = @"SELECT SCHEMA_NAME AS name FROM information_schema.SCHEMATA WHERE SCHEMA_NAME NOT IN('information_schema','mysql','performance_schema')";
                    break;
                default:
                    strTable = "master";
                    strQuery = @"SELECT name FROM sys.databases WHERE name NOT IN('master','model','msdb','tempdb') ORDER BY 1";
                    break;
            }
            ChangeDatabase(ConnectionType, strTable);
            return RunQuery(strQuery);
        }
        public static DataSet GetAllTables()
        {
            string strQuery = string.Empty;
            switch (ConnectionType)
            {
                case SqlDbConnectionType.MySql:
                    strQuery = @"SHOW TABLES";
                    break;
                default:
                    strQuery = @"SELECT name AS TableName FROM sys.tables ORDER BY name";
                    break;
            }
            return RunQuery(strQuery);
        }
        public static DataSet GetAllStoreProcedures()
        {
            string strQuery = string.Empty;
            switch (ConnectionType)
            {
                case SqlDbConnectionType.MySql:
                    strQuery = string.Format(@"SHOW FUNCTION STATUS WHERE Db = '{0}'", CurrentDatabase.Connection.Database);
                    break;
                default:
                    strQuery = @"SELECT name AS STORENAME FROM sys.objects WHERE type = 'P' ORDER BY name";
                    break;
            }
            return RunQuery(strQuery);
        }
        public static DataSet GetAllTableColumns(string strTableName, string strColumnName = "")
        {
            try
            {
                string strSub = "";
                if (!string.IsNullOrEmpty(strColumnName))
                    strSub = string.Format("and c.name = '{0}'", strColumnName);

                string strQuery = string.Format(@"select t.name TableName,c.name ColumnName,tp.name + CASE WHEN tp.name IN ('varchar', 'char', 'varbinary', 'binary', 'text')
							            THEN '(' + CASE WHEN c.max_length = -1 THEN 'MAX' ELSE CAST(c.max_length AS VARCHAR(5)) END + ')'
							         WHEN tp.name IN ('nvarchar', 'nchar', 'ntext')
							           THEN '(' + CASE WHEN c.max_length = -1 THEN 'MAX' ELSE CAST(c.max_length / 2 AS VARCHAR(5)) END + ')'
							         WHEN tp.name IN ('datetime2', 'time2', 'datetimeoffset') 
							           THEN '(' + CAST(c.scale AS VARCHAR(5)) + ')'
							         WHEN tp.name = 'decimal' 
							           THEN '(' + CAST(c.[precision] AS VARCHAR(5)) + ',' + CAST(c.scale AS VARCHAR(5)) + ')'
							        ELSE ''
						        END Type,c.max_length length,c.is_nullable Isnullable,c.column_id colorder,
						        ReferenceTableName FKTableName,ReferenceColumnName FKCplumnName,
						        isnull(ic.index_id,0) PK,delete_referential_action_desc
                                from sys.tables t 
	                                join sys.columns c on t.object_id = c.object_id
	                                join sys.types tp on c.user_type_id = tp.user_type_id
	                                left join(SELECT t.name TableName,c.name ColumnName,tr.name ReferenceTableName,cs.name ReferenceColumnName,f.delete_referential_action_desc
				                                FROM sys.tables t JOIN sys.foreign_keys f ON t.OBJECT_ID = f.parent_object_id
				                                JOIN sys.tables tr ON tr.OBJECT_ID = f.referenced_object_id
				                                JOIN sys.foreign_key_columns fc ON f.OBJECT_ID = fc.constraint_object_id
				                                JOIN sys.columns c ON c.OBJECT_ID = fc.parent_object_id AND c.column_id = fc.parent_column_id
				                                JOIN sys.columns cs ON cs.OBJECT_ID = fc.referenced_object_id AND cs.column_id = fc.referenced_column_id
				                                ) s on s.TableName = t.name and s.ColumnName = c.name
	                                left join sys.key_constraints k on k.parent_object_id = t.object_id and k.type = 'PK'
	                                left join sys.index_columns ic on c.object_id = ic.object_id and c.column_id = ic.column_id and ic.is_included_column = 0
                                where t.name = '{0}' {1}", strTableName, strSub);
                return RunQuery(strQuery);
            }
            catch (Exception exception)
            {
                if (!(exception is SqlException) || ((((SqlException)exception).ErrorCode != -2146232060) || !exception.Message.Contains("TCP Provider")))
                {
                    MessageBox.Show(exception.Message);
                }
                return null;
            }
        }
        public static string GetPrimaryKeyColumn(string strTableName)
        {
            string strQuery = string.Format(@"select c.name 
                                                from sys.tables t 
                                                join sys.columns c on t.object_id = c.object_id
                                                join sys.key_constraints k on k.parent_object_id = t.object_id and k.type = 'PK'
                                                where t.name = '{0}'", strTableName);
            DataSet ds = RunQuery(strQuery);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                return Convert.ToString(ds.Tables[0].Rows[0]["name"]);
            return string.Empty;
        }
        public static Dictionary<string, string> GetAllTablePrimaryColumns()
        {
            try
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                string str = "SELECT tc.TABLE_NAME,kcu.COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu,INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc WHERE";
                DataSet set = RunQuery(GetQuery(str + "(kcu.TABLE_NAME IN (SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES\tWHERE(TABLE_TYPE='BASE TABLE')AND(TABLE_NAME<>'sysdiagrams')))" + " AND(kcu.CONSTRAINT_NAME=tc.CONSTRAINT_NAME)AND(tc.CONSTRAINT_TYPE='PRIMARY KEY')"));
                if ((set != null) && (set.Tables.Count > 0))
                {
                    foreach (DataRow row in set.Tables[0].Rows)
                    {
                        string key = row["TABLE_NAME"].ToString();
                        string str3 = row["COLUMN_NAME"].ToString();
                        dictionary.Add(key, str3);
                    }
                }
                return dictionary;
            }
            catch (Exception exception)
            {
                if (!(exception is SqlException) || ((((SqlException)exception).ErrorCode != -2146232060) || !exception.Message.Contains("TCP Provider")))
                {
                    MessageBox.Show(exception.Message);
                }
                return null;
            }
        }
        public static bool ColumnIsExistInTable(string strTableName, string strColumnName)
        {
            try
            {
                TableName = strTableName;
                string strQuery = string.Empty;
                switch (ConnectionType)
                {
                    case SqlDbConnectionType.MySql:
                        strQuery = @"select c.column_name 
                                    from information_schema.tables t 
                                    join information_schema.columns c on t.table_name = c.table_name
                                    where t.table_schema = DATABASE() and t.table_name = '{0}' and c.column_name = '{1}'";
                        
                        break;
                    default:
                        strQuery = @"select c.name 
                                    from sys.tables t 
                                    join sys.columns c on t.object_id = c.object_id
                                    where t.name = '{0}' and c.name = '{1}'";
                        break;
                }
                DataSet set = RunQuery(string.Format(strQuery, strTableName, strColumnName));
                return ((set != null) && (set.Tables.Count > 0) && (set.Tables[0].Rows.Count > 0));
            }
            catch (Exception exception)
            {
                if (!(exception is SqlException) || ((((SqlException)exception).ErrorCode != -2146232060) || !exception.Message.Contains("TCP Provider")))
                {
                    MessageBox.Show(exception.Message);
                }
                return true;
            }
        }
        public static DataSet GetPrimaryTableWhichForeignKeyTable(string tblName, string colName = "")
        {
            string subQuery = string.Empty;
            if (!string.IsNullOrEmpty(colName))
                subQuery = string.Format(@"AND c.name like '{0}'", colName);

            string strQuery = string.Format(@"SELECT t.name TableName,c.name ColumnName,tr.name ReferenceTableName,cs.name ReferenceColumnName
                                            FROM sys.tables t JOIN sys.foreign_keys f ON t.OBJECT_ID = f.parent_object_id
                                            JOIN sys.tables tr ON tr.OBJECT_ID = f.referenced_object_id
                                            JOIN sys.foreign_key_columns fc ON f.OBJECT_ID = fc.constraint_object_id
                                            JOIN sys.columns c ON c.OBJECT_ID = fc.parent_object_id AND c.column_id = fc.parent_column_id
                                            JOIN sys.columns cs ON cs.OBJECT_ID = fc.referenced_object_id AND cs.column_id = fc.referenced_column_id
                                            WHERE t.name like '{0}' {1}", tblName, subQuery);

            return RunQuery(strQuery);
        }
        public static bool ColumnIsForeignKey(string strTableName, string strColumnName)
        {
            try
            {
                TableName = strTableName;
                DataSet set = GetPrimaryTableWhichForeignKeyTable(strTableName, strColumnName);
                return (((set != null) && (set.Tables.Count > 0)) && (set.Tables[0].Rows.Count > 0));
            }
            catch (Exception exception)
            {
                if (!(exception is SqlException) || ((((SqlException)exception).ErrorCode != -2146232060) || !exception.Message.Contains("TCP Provider")))
                {
                    MessageBox.Show(exception.Message);
                }
                return true;
            }
        }
        public static bool ColumnIsPrimaryKey(string strTableName, string strColumnName)
        {
            try
            {
                TableName = strTableName;
                DataSet set = RunQuery(string.Format(@"select t.name,c.name 
                                                from sys.tables t 
                                                join sys.columns c on t.object_id = c.object_id
                                                join sys.index_columns ic on c.object_id = ic.object_id and c.column_id = ic.column_id and ic.is_included_column = 0
                                                where t.name = '{0}' and c.name = '{1}'", strTableName, strColumnName));
                return (((set != null) && (set.Tables.Count > 0)) && (set.Tables[0].Rows.Count > 0));
            }
            catch (Exception exception)
            {
                if (!(exception is SqlException) || ((((SqlException)exception).ErrorCode != -2146232060) || !exception.Message.Contains("TCP Provider")))
                {
                    MessageBox.Show(exception.Message);
                }
                return false;
            }
        }
        public static bool IsColumnAllowNull(string strTableName, string strColumnName)
        {
            try
            {
                TableName = strTableName;
                DataSet set = RunQuery(string.Format(@"select c.name,c.is_nullable
                                                from sys.tables t 
                                                join sys.columns c on t.object_id = c.object_id
                                                join sys.key_constraints k on k.parent_object_id = t.object_id and k.type = 'PK'
                                                where t.name = '{0}'", new object[] { strTableName, strColumnName }));
                if (((set != null) && (set.Tables.Count > 0)) && (set.Tables[0].Rows.Count > 0))
                {
                    return (set.Tables[0].Rows[0]["is_nullable"].ToString() == "1");
                }
                return true;
            }
            catch (Exception exception)
            {
                if (!(exception is SqlException) || ((((SqlException)exception).ErrorCode != -2146232060) || !exception.Message.Contains("TCP Provider")))
                {
                    MessageBox.Show(exception.Message);
                }
                return false;
            }
        }

        public static string GetDatabaseVersion(SqlDbConnectionType connectionType)
        {
            string strQuery = string.Empty;
            switch (connectionType)
            {
                case SqlDbConnectionType.MySql:
                    strQuery = "SELECT VERSION()";
                    break;
                case SqlDbConnectionType.SqlServer:
                    strQuery = "";
                    break;
            }
            DataTable dtData = RunQueryTable(strQuery);
            return Convert.ToString(dtData.Rows[0][0]);
        }

        #region QQuyen - Get Object By ID AND Table
        public static DataInfo GetObjectByIDAndTableName(int iID, String strTableName)
        {
            DataInfo obj = new DataInfo();

            if (CheckTableNameIsExixst(strTableName))
            {
                String StrColumnNameByID = strTableName.Substring(0, strTableName.Length - 1) + "ID";
                String StrColumnNameByNo = strTableName.Substring(0, strTableName.Length - 1) + "No";
                String StrColumnNameByName = strTableName.Substring(0, strTableName.Length - 1) + "Name";
                String StrColumnNameByDate = strTableName.Substring(0, strTableName.Length - 1) + "Date";

                String str = String.Format(@"select * from {0} Where {1} = {2}", strTableName, StrColumnNameByID, iID);

                DataSet dsDataRow = RunQuery(String.Format(@"select * from {0} Where {1} = {2}", strTableName, StrColumnNameByID, iID));
                if (dsDataRow != null && dsDataRow.Tables.Count > 0 && dsDataRow.Tables[0].Rows.Count > 0)
                {
                    try
                    {
                        obj.strNo = dsDataRow.Tables[0].Rows[0][StrColumnNameByNo].ToString();
                        obj.strName = dsDataRow.Tables[0].Rows[0][StrColumnNameByName].ToString();
                        obj.dtDate = Convert.ToDateTime(dsDataRow.Tables[0].Rows[0][StrColumnNameByDate]);
                    }
                    catch
                    {

                    }
                }
            }

            return obj;
        }

        public static int GetIDObjectByNoAndTableName(String strNo, String strTableName)
        {
            DataInfo obj = new DataInfo();
            int iID = 0;

            if (CheckTableNameIsExixst(strTableName))
            {
                String StrColumnNameByID = strTableName.Substring(0, strTableName.Length - 1) + "ID";
                String StrColumnNameByNo = strTableName.Substring(0, strTableName.Length - 1) + "No";

                String str = String.Format(@"select {0} from {1} Where {2} = '{3}'", StrColumnNameByID, strTableName, StrColumnNameByNo, strNo);

                DataSet dsDataRow = RunQuery(str);
                if (dsDataRow != null && dsDataRow.Tables.Count > 0 && dsDataRow.Tables[0].Rows.Count > 0)
                {
                    try
                    {
                        iID = Convert.ToInt32(dsDataRow.Tables[0].Rows[0][StrColumnNameByID]);
                    }
                    catch
                    { }
                }
            }

            return iID;
        }

        public static bool CheckTableNameIsExixst(String strTableName)
        {
            String strQueryAllTableName = String.Format(@"SELECT name AS TableName FROM sys.tables WHERE name = '{0}'", strTableName);
            DataSet dsTableName = RunQuery(strQueryAllTableName);
            if (dsTableName != null && dsTableName.Tables.Count > 0 && dsTableName.Tables[0].Rows.Count > 0)
            {
                return true;
            }

            return false;
        }

        #endregion
        public static string GenerateQuery(string strTblName, string strWhere = "", string strCol = "*")
        {
            string strSubWhere = (!string.IsNullOrEmpty(strWhere)) ? "WHERE " + strWhere : "";
            return string.Format("SELECT {0} FROM {1} {2}", strCol, strTblName, strSubWhere);
        }
        public static DataTable GetDataTableByDataSet(DataSet ds)
        {
            DataTable dt = null;
            if (ds != null && ds.Tables.Count > 0)
                dt = ds.Tables[0];
            return dt;
        }
        public static DataTable GetDataByTable(string strTblName, string strWhere = "", string strCol = "*")
        {
            return RunQueryTable(GenerateQuery(strTblName, strWhere, strCol));
        }
        public static DataTable GetTableColumns(string strTblName)
        {
            string strQuery = string.Format(@"SELECT TABLE_NAME,COLUMN_NAME,DATA_TYPE,CHARACTER_MAXIMUM_LENGTH,ORDINAL_POSITION,IS_NULLABLE,CHARACTER_OCTET_LENGTH 
                                                FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{0}'", strTblName);
            DataSet ds = GetDataSet(strQuery);
            if (!TableColumnList.ContainsKey(strTblName))
                TableColumnList.Add(strTblName, GetDataTableByDataSet(ds));
            return GetDataTableByDataSet(ds);
        }
        public static DataRow GetTableColumn(string strTblName, string strColumnName)
        {
            if (!TableColumnList.ContainsKey(strTblName))
                GetTableColumns(strTblName);

            if (TableColumnList.ContainsKey(strTblName))
            {
                DataRow[] rows = TableColumnList[strTblName].Select(string.Format("COLUMN_NAME = '{0}'", strColumnName));
                if (rows.Length > 0)
                {
                    return rows[0];
                }
            }
            return null;
        }
        public static string GetColumnDataType(string strTblName, string strColumnName)
        {
            string str = string.Empty;
            DataRow tableColumn = GetTableColumn(strTblName, strColumnName);
            if (tableColumn != null)
                str = tableColumn["DATA_TYPE"].ToString();
            return str;
        }
        public static string GetColumnDBType(string strTblName, string strColumnName)
        {
            string str = string.Empty;
            DataRow tableColumn = GetTableColumn(strTblName, strColumnName);
            if (tableColumn != null)
            {
                str = tableColumn["DATA_TYPE"].ToString();
                str += "(" + tableColumn["CHARACTER_MAXIMUM_LENGTH"].ToString() + ")";
            }
            return str;
        }
        public static DataTable GetTableColumnCaptions(string strTableName)
        {
            if (!TableColumnCaptionList.ContainsKey(strTableName))
            {
                string strQuery = string.Format(@"  SELECT  TABLE_NAME,COLUMN_NAME , DATA_TYPE,                                                    
                                                            ISNULL(MAX(AAColumnAliasCaption), COLUMN_NAME) CAPTION_NAME
                                                    FROM    INFORMATION_SCHEMA.COLUMNS c
                                                    LEFT JOIN AAColumnAlias a ON TABLE_NAME = AATableName AND COLUMN_NAME = AAColumnAliasName AND a.AAStatus = 'Alive'
                                                    WHERE   TABLE_NAME = '{0}'
                                                    GROUP BY TABLE_NAME,COLUMN_NAME,ORDINAL_POSITION,DATA_TYPE
                                                    ORDER BY ORDINAL_POSITION
                                                ", strTableName);
                DataTable dt = GetDataTableByDataSet(RunQuery(strQuery));
                TableColumnCaptionList.AddItem(strTableName, dt);
            }
            return TableColumnCaptionList[strTableName];
        }
    }
    public class DataInfo
    {
        public String strNo;
        public String strName;
        public DateTime dtDate;
    }
}
