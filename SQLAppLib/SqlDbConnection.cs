using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
