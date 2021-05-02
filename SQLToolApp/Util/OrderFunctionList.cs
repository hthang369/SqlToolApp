using DevExpress.Xpf.Core;
using SQLAppLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SQLToolApp.Util
{
    public class OrderFunctionList : BaseFunctionList
    {
        #region config Connect
        public void GetConfigConnectSQL(string status, int idx = 0)
        {
            string cnt = SQLApp.GetIniFile(strFileName, section, serverCnt);
            int index = Convert.ToInt32(cnt);
            string strDesc = SQLApp.GetIniFile(strFileName, section, ServerDesc + idx);
            MessageBoxResult result = PromptForm.ShowText("Description", "Description", ref strDesc);
            if (result == MessageBoxResult.Cancel) return;
            string strServer = SQLApp.GetIniFile(strFileName, section, ServerName + idx);
            result = PromptForm.ShowText("Server", "Server", ref strServer);
            if (result == MessageBoxResult.Cancel) return;
            string strUser = SQLApp.GetIniFile(strFileName, section, ServerUID + idx);
            result = PromptForm.ShowText("User", "User", ref strUser);
            if (result == MessageBoxResult.Cancel) return;
            string strPass = SQLApp.GetIniFile(strFileName, section, ServerPWD + idx);
            result = PromptForm.ShowText("Pass", "Pass", ref strPass);
            if (result == MessageBoxResult.Cancel) return;
            if (status == "Add")
            {
                if (!string.IsNullOrEmpty(strDesc) && !string.IsNullOrEmpty(strServer) && !string.IsNullOrEmpty(strUser) && !string.IsNullOrEmpty(strPass))
                {
                    index += 1;
                    SQLApp.SetIniFile(strFileName, section, serverCnt, index.ToString());
                    SQLApp.SetIniFile(strFileName, section, ServerDesc + index, strDesc);
                    SQLApp.SetIniFile(strFileName, section, ServerName + index, strServer);
                    SQLApp.SetIniFile(strFileName, section, ServerUID + index, strUser);
                    SQLApp.SetIniFile(strFileName, section, ServerPWD + index, strPass);
                    SQLApp.SetIniFile(strFileName, section, ServerDBOld + index, "");
                }
            }
            else if (status == "edit")
            {
                if (!string.IsNullOrEmpty(strDesc) && !string.IsNullOrEmpty(strServer) && !string.IsNullOrEmpty(strUser) && !string.IsNullOrEmpty(strPass))
                {
                    SQLApp.SetIniFile(strFileName, section, ServerDesc + idx, strDesc);
                    SQLApp.SetIniFile(strFileName, section, ServerName + idx, strServer);
                    SQLApp.SetIniFile(strFileName, section, ServerUID + idx, strUser);
                    SQLApp.SetIniFile(strFileName, section, ServerPWD + idx, strPass);
                }
            }
            else
            {
                SQLApp.SetIniFile(strFileName, section, serverCnt, (index - 1).ToString());
                SQLApp.SetIniFile(strFileName, section, ServerDesc + idx, null);
                SQLApp.SetIniFile(strFileName, section, ServerName + idx, null);
                SQLApp.SetIniFile(strFileName, section, ServerUID + idx, null);
                SQLApp.SetIniFile(strFileName, section, ServerPWD + idx, null);
                SQLApp.SetIniFile(strFileName, section, ServerDBOld + idx, null);
            }
        }

        public DataTable LoadDatabaseByServer(string keySection, int idx)
        {
            if (idx == -1) return new DataTable();
            strServer = GetServerConfig(keySection, idx);
            strUserName = GetUserNameConfig(keySection, idx);
            strPassWord = GetPassWordConfig(keySection, idx);
            //strDBOld = SQLApp.GetIniFile(strFileName, section, _serverDBOld + (cboServer.SelectedIndex + 1));

            SQLDBUtil.ChangeConnection((SqlDbConnectionType)Enum.Parse(typeof(SqlDbConnectionType), keySection), strServer, strUserName, strPassWord);
            return SQLDBUtil.GetDataTableByDataSet(SQLDBUtil.GetAllDatabases());
        }
        #endregion

        #region Load config
        public void GetDatabaseVersion(SqlDbConnectionType connectionType)
        {
            string strVersion = SQLDBUtil.GetDatabaseVersion(connectionType);
            ShowMessengeInfo(strVersion);
        }
        public List<string> LoadConfigInitToList(string keySection = null)
        {
            List<string> lst = new List<string>();
            if (string.IsNullOrEmpty(keySection)) keySection = section;
            string count = SQLApp.GetIniFile(strFileName, keySection, serverCnt);
            if (!string.IsNullOrEmpty(count))
            {
                for (int i = 0; i < Convert.ToInt32(count); i++)
                {
                    lst.Add(SQLApp.GetIniFile(strFileName, keySection, ServerDesc + (i + 1)));
                }
            }
            return lst;
        }
        public string GetItemConfig(string keySection, string keyPrefix, int idx)
        {
            if (string.IsNullOrEmpty(keySection)) keySection = section;
            return SQLApp.GetIniFile(strFileName, keySection, keyPrefix + (idx + 1));
        }
        public string GetServerConfig(string keySection, int idx)
        {
            return GetItemConfig(keySection, ServerName, idx);
        }
        public string GetUserNameConfig(string keySection, int idx)
        {
            return GetItemConfig(keySection, ServerUID, idx);
        }
        public string GetPassWordConfig(string keySection, int idx)
        {
            return GetItemConfig(keySection, ServerPWD, idx);
        }
        public string GetDescriptionConfig(string keySection, int idx)
        {
            return GetItemConfig(keySection, ServerDesc, idx);
        }
        public string GetServerConfig(int idx)
        {
            return SQLApp.GetIniFile(strFileName, section, ServerName + (idx + 1));
        }
        public string GetUserNameConfig(int idx)
        {
            return SQLApp.GetIniFile(strFileName, section, ServerUID + (idx + 1));
        }
        public string GetPassWordConfig(int idx)
        {
            return SQLApp.GetIniFile(strFileName, section, ServerPWD + (idx + 1));
        }
        public string GetDescriptionConfig(int idx)
        {
            return SQLApp.GetIniFile(strFileName, section, ServerDesc + (idx + 1));
        }
        public string GetDBHistoryConfig(int idx)
        {
            return SQLApp.GetIniFile(strFileName, section, ServerDBOld + (idx + 1));
        }
        public void SetDBHistoryConfig(int idx, string strDBOld)
        {
            SQLApp.SetIniFile(strFileName, section, ServerDBOld + (idx + 1), strDBOld);
        }
        public void SetServerHistoryConfig(string strSVOld)
        {
            SQLApp.SetIniFile(strFileName, "LoginHistory", "ServerOld", strSVOld);
        }
        public string GetServerHistoryConfig()
        {
            return SQLApp.GetIniFile(strFileName, "LoginHistory", "ServerOld");
        }
        public int GetAnimateWindowTime()
        {
            int dwTime = 5000;
            dwTime = Convert.ToInt32(SQLApp.GetIniFile(strFileName, "AnimateWindow", "AnimateTime"));
            return dwTime;
        }
        #endregion

        #region Compare DB
        public void CompareDatabase(SqlDbConnectionType consType, Dictionary<string, SqlDbConnection> lstCons)
        {
            if (!CheckSelectedDB()) return;
            switch (consType)
            {
                case SqlDbConnectionType.MySql:
                    CompareDBMySql(lstCons);
                    break;
                case SqlDbConnectionType.SqlServer:
                    //(new TSQL.Tokens.
                    break;
            }
        }
        protected void CompareDBMySql(Dictionary<string, SqlDbConnection> lstCons)
        {
            string strQuery = SQLDBUtil.GenerateQuery("information_schema.TABLES", "TABLE_SCHEMA = '{0}'", "TABLE_NAME");
            CompareDifferrentData(lstCons, strQuery, "Compare Same Tables");

            strQuery = SQLDBUtil.GenerateQuery("information_schema.COLUMNS", "TABLE_SCHEMA = '{0}'", "TABLE_NAME,COLUMN_NAME,DATA_TYPE");
            CompareDifferrentData(lstCons, strQuery, "Compare Same Columns");

            strQuery = SQLDBUtil.GenerateQuery("information_schema.ROUTINES", "ROUTINE_SCHEMA = '{0}'", "ROUTINE_NAME,ROUTINE_TYPE,DATA_TYPE");
            CompareDifferrentData(lstCons, strQuery, "Compare Same Routines");

            strQuery = SQLDBUtil.GenerateQuery("information_schema.TRIGGERS", "TRIGGER_SCHEMA = '{0}'", "TRIGGER_NAME,EVENT_MANIPULATION,EVENT_OBJECT_TABLE,ACTION_TIMING");
            CompareDifferrentData(lstCons, strQuery, "Compare Same Triggers");

            strQuery = SQLDBUtil.GenerateQuery("information_schema.STATISTICS", "TABLE_SCHEMA = '{0}'", "TABLE_NAME,COLUMN_NAME");
            CompareDifferrentData(lstCons, strQuery, "Compare Same User key");
        }
        protected void CompareDifferrentData(Dictionary<string, SqlDbConnection> lstCons, string strQuery, string tblName)
        {
            SQLDBUtil.CurrentDatabase = lstCons[CtrlFrom];
            DataTable dtSource = SQLDBUtil.GetDataTable(string.Format(strQuery, SQLDBUtil.CurrentDatabase.Connection.Database));
            SQLDBUtil.CurrentDatabase = lstCons[CtrlTo];
            DataTable dtTarget = SQLDBUtil.GetDataTable(string.Format(strQuery, SQLDBUtil.CurrentDatabase.Connection.Database));
            IEnumerable<DataRow> lstSameTable = dtSource.AsEnumerable().Except(dtTarget.AsEnumerable(), DataRowComparer.Default);
            DataTable dtSame = ConvertDataRowToTable(lstSameTable, tblName);
            ShowCompareResultView(dtSame);
        }
        #endregion

        public void ShowCompareResultView(DataTable dtSource)
        {
            if (dtSource != null)
            {
                ViewModels.CompareResultViewModel popupView = GetCompareResultPopupView();
                popupView.Title = "Compare";
                popupView.Header = "Compare Result";
                Task.Factory.StartNew(() =>
                {
                    return dtSource;
                }).ContinueWith(r => AddControlsToGrid(popupView, r.Result, ""), TaskScheduler.FromCurrentSynchronizationContext());
                ShowPopupViewModal(popupView, new Views.CompareResult());
            }
        }
    }
}