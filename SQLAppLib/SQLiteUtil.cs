using SqlKata;
using SqlKata.Compilers;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SQLAppLib
{
    public class SQLiteUtil : SqlDatabaseHelper
    {
        static SQLiteUtil()
        {
            ChangeConnection(SqlDbConnectionType.Sqlite, Application.StartupPath + "\\sqlconfig.db", "", "");
            //new 
           
        }

        private static Query GetQueryConfig(SqlDbConnectionType strType)
        {
            return new Query("query_configs").Where("type", strType.ToString().ToLower());
        }

        public static string GetCurrentDatabaseName(SqlDbConnectionType strType)
        {
            SqlResult result = new SqliteCompiler().Compile(GetQueryConfig(strType).Select("query_string").Where("name", "current_db"));
            //return RunQueryTable("SELECT  FROM query_configs WHERE name = '' AND ");

            return "";
        }
    }
}
