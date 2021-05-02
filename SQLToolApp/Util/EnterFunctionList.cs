using Newtonsoft.Json;
using SQLAppLib;
using SQLToolApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace SQLToolApp.Util
{
    public class EnterFunctionList : BaseFunctionList
    {
        public void LoadQueryPath(FunctionListObject obj, Window frmParent)
        {
            string strQuery = SQLApp.GetFile(obj.Path);
            strQuery = GenerateScriptWithParameters(obj, strQuery, frmParent);
            if (string.IsNullOrEmpty(strQuery)) return;
            DataTable dt = SQLDBUtil.GetDataTable(strQuery);
            if (dt == null) return;
            ShowResultData(frmParent, dt, strQuery);
        }
        #region function list

        public void LoadDataByTable(Window frmParent)
        {
            PromptForm._frmParent = frmParent;
            string moduleName = "";
            MessageBoxResult result = PromptForm.ShowText("Find Module", "ModuleName", ref moduleName);
            if (result == MessageBoxResult.Cancel) return;
            string strQuery = SQLApp.GetFile(strPath + "FindModule.sql");
            strQuery = strQuery.Replace("@ModuleName@", moduleName);
            DataTable dt = SQLDBUtil.GetDataTable(strQuery);
            if (dt == null) return;
            dt.TableName = "STModules";
            ShowResultData(frmParent, dt, strQuery);
        }
        //Ctrl + 1 tìm module
        public void FindModule(Window frmParent)
        {
            PromptForm._frmParent = frmParent;
            string moduleName = "";
            MessageBoxResult result = PromptForm.ShowText("Find Module", "ModuleName", ref moduleName);
            if (result == MessageBoxResult.Cancel) return;
            string strQuery = SQLApp.GetFile(strPath + "FindModule.sql");
            strQuery = strQuery.Replace("@ModuleName@", moduleName);
            DataTable dt = SQLDBUtil.GetDataTable(strQuery);
            if (dt == null) return;
            dt.TableName = "STModules";
            ShowResultData(frmParent, dt, strQuery);
        }
        //Ctrl + F Find Column
        public void FindColumn(Window frmParent)
        {
            PromptForm._frmParent = frmParent;
            string tableName = "";
            MessageBoxResult result = PromptForm.ShowCombobox("FindColumn", "Table Name", ref tableName);
            if (result == MessageBoxResult.Cancel) return;
            string colName = "";
            result = PromptForm.ShowText("FindColumn", "Column Name", ref colName);
            if (result == MessageBoxResult.Cancel) return;
            DataSet ds = SQLDBUtil.GetAllTableColumns(tableName, colName);
            DataTable dtData = SQLDBUtil.GetDataTableByDataSet(ds);
            if (dtData == null) return;
            dtData.TableName = tableName;
            ShowResultData(frmParent, dtData, "");
        }
        //Alt + 1 View Data by No
        public void GetViewDataByNo(Window frmParent)
        {
            PromptForm._frmParent = frmParent;
            string tableName = "";

            MessageBoxResult result = PromptForm.ShowCombobox("ViewDataByNo", "Table Name", ref tableName);
            if (result == MessageBoxResult.Cancel) return;
            string colName = "";
            result = PromptForm.ShowText("ViewDataByNo", "Column Name", ref colName);
            if (result == MessageBoxResult.Cancel) return;
            if (string.IsNullOrEmpty(colName)) colName = "*";
            string strWhere = string.Empty;
            if (SQLDBUtil.ColumnIsExistInTable(tableName, "AAStatus")) strWhere = "AAStatus = 'Alive'";
            string strQuery = SQLDBUtil.GenerateQuery(tableName, strWhere, colName);
            //DataTable dtData = SQLDBUtil.GetDataByTable(tableName, strWhere, colName);
            //if (dtData == null) return;
            //dtData.TableName = tableName;
            ShowResultData(frmParent, null, strQuery);
        }

        //Ctrl + 0 View Connect Sql
        public void GetViewConnectToSQL(Window frmParent)
        {
            string strQuery = SQLApp.GetFile(strPath + "ViewConnectSql.sql");
            DataTable dtSource = SQLDBUtil.GetDataTable(strQuery);
            ShowResultData(frmParent, dtSource, strQuery);
        }
        //Ctrl + Shift + A : Create Module
        public void CreateMoudle()
        {
            string str = MethodInfo.GetCurrentMethod().Name;
            string strModuleName = "";
            MessageBoxResult result = PromptForm.ShowText(str, "Module Name", ref strModuleName);
            if (result == MessageBoxResult.Cancel) return;
            string strModuleDesc = "";
            result = PromptForm.ShowText(str, "Module Descreiption", ref strModuleDesc);
            if (result == MessageBoxResult.Cancel) return;
            string strModuleCode = "";
            result = PromptForm.ShowText(str, "Module Code", ref strModuleCode);
            if (result == MessageBoxResult.Cancel) return;
            if (!string.IsNullOrEmpty(strModuleName) && !string.IsNullOrEmpty(strModuleDesc) && !string.IsNullOrEmpty(strModuleCode))
            {
                string strQuery = SQLApp.GetFile(strPath + "CreateModule.sql");
                strQuery = strQuery.Replace("@ModuleName@", strModuleName);
                strQuery = strQuery.Replace("@ModuleCode@", strModuleCode);
                strQuery = strQuery.Replace("@ModuleDesc@", strModuleDesc);
                int iResult = SQLDBUtil.ExecuteNonQuery(strQuery);
                ShowMessenger(iResult);
            }
        }
        //Ctrl + Alt + T : Gen Script Create Table
        public void GenScriptCreateTable(Window frmParent)
        {
            string str = MethodInfo.GetCurrentMethod().Name;
            string strTableName = "";
            MessageBoxResult result = PromptForm.ShowCombobox(str, "Table Name", ref strTableName);
            if (result == MessageBoxResult.Cancel) return;
            string strQuery = SQLApp.GetFile(strPath + "GenCreateTable.sql");
            strQuery = strQuery.Replace("@TableName@", strTableName);
            DataTable dt = SQLDBUtil.GetDataTable(strQuery);
            ShowResultData(frmParent, dt, strQuery);
        }
        //Ctrl + Alt + T : Gen Script Create Table
        public void GenScriptCreateColumn(Window frmParent)
        {
            string str = MethodInfo.GetCurrentMethod().Name;
            string strTableName = "";
            MessageBoxResult result = PromptForm.ShowCombobox(str, "Table Name", ref strTableName);
            if (result == MessageBoxResult.Cancel) return;
            string strColumnName = "";
            result = PromptForm.ShowCombobox(str, "Column Name", ref strTableName);
            if (result == MessageBoxResult.Cancel) return;
            string strQuery = SQLApp.GetFile(strPath + "GenCreateTable.sql");
            strQuery = strQuery.Replace("@TableName@", strTableName);
            DataTable dt = SQLDBUtil.GetDataTable(strQuery);
            ShowResultData(frmParent, dt, strQuery);
        }
        //Ctrl + 6: Gen Info / Controller
        public void GenInfoController()
        {
            string str = MethodInfo.GetCurrentMethod().Name;
            string strTableName = "";
            MessageBoxResult result = PromptForm.ShowCombobox(str, "Table Name", ref strTableName);
            if (result == MessageBoxResult.Cancel) return;
            string strType = string.Empty;
            result = PromptForm.ShowCombobox(str, "Gen Controller", new string[] { "YES", "NO" }, ref strType);
            if (result == MessageBoxResult.Cancel) return;
            string strQuery = SQLApp.GetFile(strPath + "GenInfo.sql");
            strQuery = strQuery.Replace("@TableName@", strTableName);
            strQuery = strQuery.Replace("@Version@", System.Windows.Forms.Application.ProductName + " - " + System.Windows.Forms.Application.ProductVersion);
            strQuery = strQuery.Replace("@CreatedDate@", DateTime.Now.ToShortDateString());
            DataTable dt = SQLDBUtil.GetDataTable(strQuery);
            if (dt != null)
            {
                string strContent = Convert.ToString(dt.Rows[0][0]);
                SQLApp.WriteFile("D:\\" + strTableName + "Info.cs", strContent);
                //NotifycationAler aler = new NotifycationAler();
                //aler.ShowDialog();
            }
            if (strType == "YES")
            {
                strQuery = SQLApp.GetFile(strPath + "GenController.sql");
                strQuery = strQuery.Replace("@TableName@", strTableName);
                strQuery = strQuery.Replace("@Version@", System.Windows.Forms.Application.ProductName + " - " + System.Windows.Forms.Application.ProductVersion);
                strQuery = strQuery.Replace("@CreatedDate@", DateTime.Now.ToShortDateString());
                dt = SQLDBUtil.GetDataTable(strQuery);
                if (dt != null)
                {
                    string strContent = Convert.ToString(dt.Rows[0][0]);
                    SQLApp.WriteFile("D:\\" + strTableName + "Controller.cs", strContent);
                }
            }
        }
        //public void LoadFunction(frmMain frmParent)
        //{
        //    ListViewItem obj = frmParent.lstFunction.SelectedItems.Cast<ListViewItem>().FirstOrDefault();
        //    if (obj == null) return;
        //    string strCnt = SQLApp.GetIniFile(strFileCfgScript, strDynPara, obj.Text + "Cnt");
        //    if (string.IsNullOrEmpty(strCnt)) return;
        //    int idxCnt = Convert.ToInt32(strCnt);
        //    Dictionary<string, string> dicPara = new Dictionary<string, string>();
        //    for (int i = 1; i <= idxCnt; i++)
        //    {
        //        string strName = SQLApp.GetIniFile(strFileCfgScript, strDynPara, obj.Text + "Name" + i);
        //        string strVal = SQLApp.GetIniFile(strFileCfgScript, strDynPara, obj.Text + "Val" + i);
        //        string strValList = SQLApp.GetIniFile(strFileCfgScript, strDynPara, obj.Text + "ValList" + i);
        //        MessageBoxResult result = MessageBoxResult.Cancel;
        //        if (strName.Contains("TableName"))
        //            result = PromptForm.ShowCombobox(obj.Text, strName, ref strVal);
        //        else if(!string.IsNullOrEmpty(strValList))
        //            result = PromptForm.ShowCombobox(obj.Text, strName, strValList.Split('|'), ref strVal);
        //        else
        //            result = PromptForm.ShowText(obj.Text, strName, ref strVal);
        //        if (result == MessageBoxResult.Cancel) return;
        //        dicPara.AddItem(strName, strVal);
        //    }
        //    string strQuery = SQLApp.GetFile(strPath + obj.Text + ".sql");
        //    foreach (KeyValuePair<string,string> item in dicPara)
        //    {
        //        strQuery = strQuery.Replace(item.Key, item.Value);
        //    }
        //    SQLDBUtil.GetDataSet(strQuery);
        //}
        #region Sync DB
        public DataSet SynchronizeTable(int indexFrom, int indexTo, string strDBFrom, string strDBTo)
        {
            int iResult = CreateLinkServer(indexFrom, indexTo, strDBFrom, strDBTo);
            string strSrvName = GetDescriptionConfig(indexFrom);
            string strSrvAdd = GetServerConfig(indexFrom);
            string strUser = GetUserNameConfig(indexFrom);
            string strPassWord = GetPassWordConfig(indexFrom);

            string strSrvNameTo = GetDescriptionConfig(indexTo);
            string strSrvAddTo = GetServerConfig(indexFrom);
            string strUserTo = GetUserNameConfig(indexTo);
            string strPassWordTo = GetPassWordConfig(indexTo);

            string strQuery = SQLApp.GetFile(strPath + "SyncDB.sql");
            strQuery = strQuery.Replace("@serverName@", strSrvName);
            strQuery = strQuery.Replace("@serverAddress@", strSrvAdd);
            strQuery = strQuery.Replace("@serverUser@", strUser);
            strQuery = strQuery.Replace("@serverPass@", strPassWord);
            strQuery = strQuery.Replace("@serverDB@", strDBFrom);

            strQuery = strQuery.Replace("@serverNameTo@", strSrvNameTo);
            strQuery = strQuery.Replace("@serverAddressTo@", strSrvAddTo);
            strQuery = strQuery.Replace("@serverUserTo@", strUserTo);
            strQuery = strQuery.Replace("@serverPassTo@", strPassWordTo);
            strQuery = strQuery.Replace("@serverDBTo@", strDBTo);

            return SQLDBUtil.GetDataSet(strQuery);
        }
        public int CreateLinkServer(int indexFrom, int indexTo, string strDBFrom, string strDBTo)
        {
            string strSrvName = GetDescriptionConfig(indexFrom);
            string strSrvAdd = GetServerConfig(indexFrom);
            string strUser = GetUserNameConfig(indexFrom);
            string strPassWord = GetPassWordConfig(indexFrom);

            string strSrvNameTo = GetDescriptionConfig(indexTo);
            string strSrvAddTo = GetServerConfig(indexFrom);
            string strUserTo = GetUserNameConfig(indexTo);
            string strPassWordTo = GetPassWordConfig(indexTo);

            string strQuery = SQLApp.GetFile(strPath + "LinkServer.sql");
            strQuery = strQuery.Replace("@serverName@", strSrvName);
            strQuery = strQuery.Replace("@serverAddress@", strSrvAdd);
            strQuery = strQuery.Replace("@serverUser@", strUser);
            strQuery = strQuery.Replace("@serverPass@", strPassWord);
            strQuery = strQuery.Replace("@serverDB@", strDBFrom);

            strQuery = strQuery.Replace("@serverNameTo@", strSrvNameTo);
            strQuery = strQuery.Replace("@serverAddressTo@", strSrvAddTo);
            strQuery = strQuery.Replace("@serverUserTo@", strUserTo);
            strQuery = strQuery.Replace("@serverPassTo@", strPassWordTo);
            strQuery = strQuery.Replace("@serverDBTo@", strDBTo);

            return SQLDBUtil.ExecuteNonQuery(strQuery);
        }
        public string GetScriptDropColumn(string strTable, string strColumn)
        {
            return string.Format("ALTER TABLE [{0}] DROP COLUMN [{1}]", strTable, strColumn);
        }
        public string GetScriptDropTable(string strTable)
        {
            return string.Format("DROP TABLE [{0}]", strTable);
        }
        public string GetScriptCreateTable(string strDBName, string strTable)
        {
            string strQuery = SQLApp.GetFile(strPath + "CreateTable.sql");
            strQuery = strQuery.Replace("@tablename@", strTable);
            strQuery = strQuery.Replace("@schemaname@", strDBName);
            DataTable dt = SQLDBUtil.GetDataTable(strQuery);
            if (dt != null && dt.Rows.Count > 0)
            {
                return string.Format("{0}", dt.Rows[0][0]);
            }
            return "";
        }
        public string GetScriptCreateColumn(string strDBName, string strTable, string strColName)
        {
            string strQuery = SQLApp.GetFile(strPath + "CreateColumn.sql");
            strQuery = strQuery.Replace("@TableName@", strTable);
            strQuery = strQuery.Replace("@ColumnName@", strColName);
            strQuery = strQuery.Replace("@schemaname@", strDBName);
            DataTable dt = SQLDBUtil.GetDataTable(strQuery);
            if (dt != null && dt.Rows.Count > 0)
            {
                return string.Format("{0}", dt.Rows[0][0]);
            }
            return "";
        }
        
        private void AddReaderToGrid(BasePopupViewModel viewModel, System.Data.Common.DbDataReader dataReader)
        {
            DataTable dtSource = new DataTable();
            dtSource.TableName = "Data";

            for (int i = 0; i < dataReader.FieldCount; i++)
            {
                DataColumn col = new DataColumn(dataReader.GetName(i), dataReader.GetFieldType(i));
                dtSource.Columns.Add(col);
            }
            while (dataReader.Read())
            {
                object[] values = new object[dataReader.FieldCount];
                dataReader.GetValues(values);
                dtSource.LoadDataRow(values, false);
            }
            dataReader.Close();

            AddControlsToGrid(viewModel, dtSource, "");
        }
        #endregion

        #region func list F10
        public void GetFunctionList()
        {
            lstFuncLst = new Dictionary<string, string>()
            {
                { "COL RENAME","RenameColumn" }, { "COL REPLACENAME","replacename" }, { "COL ADD","AddColumn" }, { "COL ALTER","AlterColumn" }, { "COL DROP","DropColumn" },
                { "INDEX ADD","AddIndex" }, { "INDEX DROP","DropIndex" }, { "PK ADD","AddPrimaryKey" }, { "PK DROP","DropPrimaryKey"}, { "FK ADD","AddForeignKey" }, { "FK DROP","DropForeignKey" },
                { "TABLE RENAME","RenameTable" }, { "TABLE ADD","CreateTable" }, { "TABLE DROP","DropTable" }, { "TABLE ZAP","ZapTable" }, {"DB CREATE","CreateDatabase" }, {"DB DROP","DropDatabase" },
                { "DB BACKUP","BackupDatabase" }, { "DB SHRINK","ShrinkDatabase" }, {"DB RESTORE","RestoreDatabase" }, {"DB RESTART","" }
            };
        }
        public void ShowFunctionList()
        {
            GetFunctionList();
            string[] lstSource = lstFuncLst.Keys.ToArray();
            string value = "";
            MessageBoxResult result = PromptForm.ShowCombobox("Action", "Action", lstSource, ref value);
            if (result == MessageBoxResult.Cancel) return;
            //switch (value)
            //{
            EnterFunctionList lstThis = new EnterFunctionList();
            MethodInfo mi = lstThis.GetType().GetMethod(lstFuncLst[value]);
            mi.Invoke(lstThis, null);
            //MethodInfo miConstructed = mi.MakeGenericMethod(type[0]);
            //}
        }
        public void RenameColumn()
        {
            string str = MethodInfo.GetCurrentMethod().Name;
            string strTblName = "";
            MessageBoxResult result = PromptForm.ShowText(str, "Table Name", ref strTblName);
            if (result == MessageBoxResult.Cancel) return;
            string strColOld = "";
            result = PromptForm.ShowText(str, "Column Name", ref strColOld);
            if (result == MessageBoxResult.Cancel) return;
            string strColNew = "";
            result = PromptForm.ShowText(str, "New Column Name", ref strColNew);
            if (result == MessageBoxResult.Cancel) return;
            int iResult = SQLDBUtil.ExecuteNonQuery(string.Format("ALTER TABLE {0} RENAME COLUMN {1} TO {2}", strTblName, strColOld, strColNew));
            ShowMessenger(iResult);
        }
        public void DropColumn()
        {
            string str = MethodInfo.GetCurrentMethod().Name;
            string strTblName = "";
            MessageBoxResult result = PromptForm.ShowText(str, "Table Name", ref strTblName);
            if (result == MessageBoxResult.Cancel) return;
            string strColName = "";
            result = PromptForm.ShowText(str, "Column Name", ref strColName);
            if (result == MessageBoxResult.Cancel) return;
            int iResult = SQLDBUtil.ExecuteNonQuery(string.Format("ALTER TABLE {0} DROP COLUMN {1}", strTblName, strColName));
            ShowMessenger(iResult);
        }
        public void AlterColumn()
        {
            string str = MethodInfo.GetCurrentMethod().Name;
            string strTblName = "";
            MessageBoxResult result = PromptForm.ShowText("Table Name", "Table Name", ref strTblName);
            if (result == MessageBoxResult.Cancel) return;
            string strColName = "";
            result = PromptForm.ShowText("Column Name", "Column Name", ref strColName);
            if (result == MessageBoxResult.Cancel) return;
            string strColType = SQLDBUtil.GetColumnDBType(strTblName, strColName);
            result = PromptForm.ShowText("Column Type", "Column Type", ref strColType);
            if (result == MessageBoxResult.Cancel) return;
            int iResult = SQLDBUtil.ExecuteNonQuery(string.Format("ALTER TABLE {0} ALTER COLUMN {1} {2}", strTblName, strColName, strColType));
            ShowMessenger(iResult);
        }
        public void AddColumn()
        {
            string str = MethodInfo.GetCurrentMethod().Name;
            string strTblName = "";
            MessageBoxResult result = PromptForm.ShowCombobox(str, "Table Name", ref strTblName);
            if (result == MessageBoxResult.Cancel) return;
            string strColName = "";
            result = PromptForm.ShowText(str, "Column Name", ref strColName);
            if (result == MessageBoxResult.Cancel) return;
            int iResult = SQLDBUtil.ExecuteNonQuery(string.Format("ALTER TABLE {0} ADD {1}", strTblName, strColName));
            ShowMessenger(iResult);
        }
        public void AddPrimaryKey()
        {
            string str = MethodInfo.GetCurrentMethod().Name;
            string strTblName = "";
            MessageBoxResult result = PromptForm.ShowText("Table Name", "Table Name", ref strTblName);
            if (result == MessageBoxResult.Cancel) return;
            string strColName = "";
            result = PromptForm.ShowText("Column Name", "Column Name", ref strColName);
            if (result == MessageBoxResult.Cancel) return;
            int iResult = SQLDBUtil.ExecuteNonQuery(string.Format("ALTER TABLE {0} ADD CONSTRAINT PK_{0} PRIMARY KEY({1})", strTblName, strColName));
            ShowMessenger(iResult);
        }
        public void AddIndex()
        {
            string str = MethodInfo.GetCurrentMethod().Name;
            string strTblName = "";
            MessageBoxResult result = PromptForm.ShowCombobox(str, "Table Name", ref strTblName);
            if (result == MessageBoxResult.Cancel) return;
            string strColName = "";
            result = PromptForm.ShowText(str, "Column Name", ref strColName);
            if (result == MessageBoxResult.Cancel) return;
            string strIdxName = "";
            result = PromptForm.ShowText(str, "Index Name", ref strIdxName);
            if (result == MessageBoxResult.Cancel) return;
            int iResult = SQLDBUtil.ExecuteNonQuery(string.Format("CREATE INDEX {0} ON {1}({2})", strIdxName, strTblName, strColName));
            ShowMessenger(iResult);
        }
        public void DropPrimaryKey()
        {
            string str = MethodInfo.GetCurrentMethod().Name;
            string strTblName = "";
            MessageBoxResult result = PromptForm.ShowText("Table Name", "Table Name", ref strTblName);
            if (result == MessageBoxResult.Cancel) return;
            string strColName = "";
            result = PromptForm.ShowText("Column Name", "Column Name", ref strColName);
            if (result == MessageBoxResult.Cancel) return;
            int iResult = SQLDBUtil.ExecuteNonQuery(string.Format("ALTER TABLE {0} DROP CONSTRAINT PK_{1}", strTblName, strColName));
            ShowMessenger(iResult);
        }
        public void DropIndex()
        {
            string str = MethodInfo.GetCurrentMethod().Name;
            string strTblName = "";
            MessageBoxResult result = PromptForm.ShowText("Table Name", "Table Name", ref strTblName);
            if (result == MessageBoxResult.Cancel) return;
            string strIdxName = "";
            result = PromptForm.ShowText("Index Name", "Index Name", ref strIdxName);
            if (result == MessageBoxResult.Cancel) return;
            int iResult = SQLDBUtil.ExecuteNonQuery(string.Format("DROP INDEX {0}.{1}", strTblName, strIdxName));
            ShowMessenger(iResult);
        }
        public void DropTable()
        {
            string str = MethodInfo.GetCurrentMethod().Name;
            string strTblName = "";
            MessageBoxResult result = PromptForm.ShowText("Table Name", "Table Name", ref strTblName);
            if (result == MessageBoxResult.Cancel) return;
            int iResult = SQLDBUtil.ExecuteNonQuery(string.Format("DROP TABLE {0}", strTblName));
            ShowMessenger(iResult);
        }
        public void DropDatabase()
        {
            string str = MethodInfo.GetCurrentMethod().Name;
            string strDBName = "";
            MessageBoxResult result = PromptForm.ShowText("Database Name", "Database Name", ref strDBName);
            if (result == MessageBoxResult.Cancel) return;
            int iResult = SQLDBUtil.ExecuteNonQuery(string.Format("DROP DATABASE {0}", strDBName));
            ShowMessenger(iResult);
        }
        public void AddForeignKey(string strTblName, string strColName, string strReferenTblName, string strReferenColName)
        {
            string str = MethodInfo.GetCurrentMethod().Name;
            int iResult = SQLDBUtil.ExecuteNonQuery(string.Format("ALTER TABLE {0} ADD CONSTRAINT FK_{0}_{2}_{1} FOREIGN KEY {1} REFERENCES {2} ({3})", strTblName, strColName, strReferenTblName, strReferenColName));
            ShowMessenger(iResult);
        }
        public void DropForeignKey()
        {
            string str = MethodInfo.GetCurrentMethod().Name;
            string strTblName = "";
            MessageBoxResult result = PromptForm.ShowText("Table Name", "Table Name", ref strTblName);
            if (result == MessageBoxResult.Cancel) return;
            int iResult = SQLDBUtil.ExecuteNonQuery(string.Format("ALTER TABLE {0} DROP CONSTRAINT FK_{1}", strTblName));
            ShowMessenger(iResult);
        }
        public void RenameTable()
        {
            string str = MethodInfo.GetCurrentMethod().Name;
            string strTblName = "";
            MessageBoxResult result = PromptForm.ShowText(str, "Table Name", ref strTblName);
            if (result == MessageBoxResult.Cancel) return;
            string strToTblName = "";
            result = PromptForm.ShowText(str, "New Table Name", ref strToTblName);
            if (result == MessageBoxResult.Cancel) return;
            int iResult = SQLDBUtil.ExecuteNonQuery(string.Format("RENAME TABLE {0} TO {1}", strTblName, strToTblName));
            ShowMessenger(iResult);
        }
        public void CreateTable()
        {

        }
        public void CreateDatabase()
        {

        }
        public void ZapTable()
        {

        }
        public void BackupDatabase()
        {

        }
        public void ShrinkDatabase()
        {

        }
        public void RestoreDatabase()
        {

        }
        
        #endregion

        #region Function Extra
        public void FindAllProcessLockedFile(Window frmParent)
        {
            string strFilePath = "";
            using (OpenFileDialog open = new OpenFileDialog())
            {
                if (open.ShowDialog() == DialogResult.OK)
                    strFilePath = open.FileName;
            }
            List<Process> lstProcess = Win32Processes.GetProcessesLockingFile(strFilePath);
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("name"));
            foreach (Process item in lstProcess)
            {
                DataRow dr = dt.NewRow();
                dr["name"] = item.ProcessName;
                dt.Rows.Add(dr);
                item.Kill();
            }
            ShowResultData(frmParent, dt, "");
        }
        
        public void ShowYoutubeView()
        {
            //ViewModels.YoutubeViewModel popupView = new YoutubeViewModel();
            //ShowPopupViewModal(popupView, new Views.YoutubeView());
        }
        public void ShowFlashDealView()
        {
            //Views.FlashDealView view = new Views.FlashDealView();
            //ViewModels.FlashDealViewModel popupView = new FlashDealViewModel(view);
            //popupView.Header = "Flash sale sendo";
            //ShowPopupViewModal(popupView, view);
        }
        
        public void GetAllLanguage(Window frmParent)
        {
            try
            {
                

                string sourceUrl = SQLApp.GetIniFile(strFileName, "SourceCode", "SourceUrl");
                string[] listFiles = Directory.GetFiles(string.Format("{0}\\database\\seeds\\lang", sourceUrl), "*_lang.php", SearchOption.AllDirectories);
                DataTable dtLang = new DataTable("Language");
                DataColumn col = new DataColumn("Group");
                dtLang.Columns.Add(col);
                col = new DataColumn("Key");
                dtLang.Columns.Add(col);
                col = new DataColumn("Text_vi");
                dtLang.Columns.Add(col);
                col = new DataColumn("Text_en");
                dtLang.Columns.Add(col);
                col = new DataColumn("Text_ja");
                dtLang.Columns.Add(col);
                foreach (string item in listFiles)
                {
                    string strContent = SQLApp.ExecutedPowerShell(string.Format("php -r \"echo json_encode(require '{0}');\"", item));
                    object obj = JsonConvert.DeserializeObject(strContent);
                    string objGroup = (obj as Newtonsoft.Json.Linq.JObject).GetValue("group").ToString();
                    Newtonsoft.Json.Linq.JArray objLang = (obj as Newtonsoft.Json.Linq.JObject).GetValue("lang") as Newtonsoft.Json.Linq.JArray;
                    foreach (object objItem in objLang)
                    {
                        DataRow dr = dtLang.NewRow();
                        dr["Group"] = objGroup;
                        dr["Key"] = (objItem as Newtonsoft.Json.Linq.JObject).GetValue("key").ToString();
                        dr["Text_vi"] = (objItem as Newtonsoft.Json.Linq.JObject).SelectToken("text.vi").ToString();
                        dr["Text_en"] = (objItem as Newtonsoft.Json.Linq.JObject).SelectToken("text.en").ToString();
                        dr["Text_ja"] = (objItem as Newtonsoft.Json.Linq.JObject).SelectToken("text.ja").ToString();
                        dtLang.Rows.Add(dr);
                    }
                }
                ShowResultData(frmParent, dtLang);
            }
            catch (Exception ex)
            {

            }
        }
        #endregion
        #endregion
    }
}