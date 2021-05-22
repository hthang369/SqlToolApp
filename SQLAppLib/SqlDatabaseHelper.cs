using MySqlConnector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SQLAppLib
{
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
            switch (connectionType)
            {
                case SqlDbConnectionType.MySql:
                    string[] lstServer = strServer.Split(':');
                    MySqlConnectionStringBuilder mysqlBuilder = new MySqlConnectionStringBuilder
                    {
                        Server = lstServer.FirstOrDefault(),
                        Port = Convert.ToUInt32(lstServer.LastOrDefault()),
                        Database = strDatabase,
                        UserID = strUsername,
                        Password = strPassword,
                        ConvertZeroDateTime = true,
                        AllowUserVariables = true
                    };

                    _connectionString = mysqlBuilder.ConnectionString;
                    break;
                case SqlDbConnectionType.Sqlite:
                    var builder = new SQLiteConnectionStringBuilder
                    {
                        DataSource = strServer,
                        Version = 3
                    };

                    _connectionString = builder.ConnectionString;
                    break;
                default:
                    SqlConnectionStringBuilder sqlBuilder = new SqlConnectionStringBuilder
                    {
                        DataSource = strServer,
                        UserID = strUsername,
                        Password = strPassword,
                        InitialCatalog = strDatabase
                    };
                    _connectionString = sqlBuilder.ConnectionString;
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
                else if (!CurrentDatabase.Connection.GetType().Name.StartsWith(SqlDbConnectionType.MySql.ToString()))
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
}
