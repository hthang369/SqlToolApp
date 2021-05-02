using SQLAppLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLToolApp.Util
{
    class BaseUtil
    {
        // Get table name by column
        public static String GetForeignTableNameByForeignColum(String strColumn)
        {
            string strTableName_Foreign = "";
            try
            {
                if (strColumn.Contains("FK_"))
                {
                    String[] strForeignColumn = strColumn.Split('_');
                    if (strForeignColumn.Length > 0)
                    {
                        strTableName_Foreign = (strForeignColumn[strForeignColumn.Length - 1].ToString()).Replace("ID", "s");
                    }
                }
            }
            catch { }

            return strTableName_Foreign;
        }

        public static SqlDbConnectionType GetConnectionType(string consType)
        {
            return (SqlDbConnectionType)Enum.Parse(typeof(SqlDbConnectionType), consType);
        }
    }

}
