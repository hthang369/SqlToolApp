using DevExpress.XtraEditors;
using SQLAppLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace SQLAppLib
{
    public class SQLUtil
    {
        private static string section = "SqlServer";
        private static string serverCnt = "ServerCount";
        private static string _serverDesc = "Descreiption";
        private static string _serverName = "Server";
        private static string _serverUID = "UID";
        private static string _serverPWD = "PWD";
        private static string _serverDBOld = "DB_Old";
        private static string strFileName = Application.StartupPath + "\\config.ini";
        private static string strServer;
        private static string strDatabase;
        private static string strUserName;
        private static string strPassWord;
        private static string strDBOld;
        public static string strPath = Application.StartupPath + "\\Scripts\\";
        public static string strFileCfgScript = strPath + "scripts.ini";
        public static Dictionary<string, string> lstFuncLst;
        public static string strDynPara = "DynPara";

        #region config Connect
        //public static void GetConfigConnectSQL(string status, int idx = 0)
        //{
        //    string cnt = SQLApp.GetIniFile(strFileName, section, serverCnt);
        //    int index = Convert.ToInt32(cnt);
        //    string strDesc = SQLApp.GetIniFile(strFileName, section, _serverDesc + idx);
        //    DialogResult result = PromptForm.ShowText("Description", "Description", ref strDesc);
        //    if (result == DialogResult.Cancel) return;
        //    string strServer = SQLApp.GetIniFile(strFileName, section, _serverName + idx);
        //    result = PromptForm.ShowText("Server", "Server", ref strServer);
        //    if (result == DialogResult.Cancel) return;
        //    string strUser = SQLApp.GetIniFile(strFileName, section, _serverUID + idx);
        //    result = PromptForm.ShowText("User", "User", ref strUser);
        //    if (result == DialogResult.Cancel) return;
        //    string strPass = SQLApp.GetIniFile(strFileName, section, _serverPWD + idx);
        //    result = PromptForm.ShowText("Pass", "Pass", ref strPass);
        //    if (result == DialogResult.Cancel) return;
        //    if (status == "add")
        //    {
        //        if (!string.IsNullOrEmpty(strDesc) && !string.IsNullOrEmpty(strServer) && !string.IsNullOrEmpty(strUser) && !string.IsNullOrEmpty(strPass))
        //        {
        //            index += 1;
        //            SQLApp.SetIniFile(strFileName, section, serverCnt, index.ToString());
        //            SQLApp.SetIniFile(strFileName, section, _serverDesc + index, strDesc);
        //            SQLApp.SetIniFile(strFileName, section, _serverName + index, strServer);
        //            SQLApp.SetIniFile(strFileName, section, _serverUID + index, strUser);
        //            SQLApp.SetIniFile(strFileName, section, _serverPWD + index, strPass);
        //            SQLApp.SetIniFile(strFileName, section, _serverDBOld + index, "");
        //        }
        //    }
        //    else if (status == "edit")
        //    {
        //        if (!string.IsNullOrEmpty(strDesc) && !string.IsNullOrEmpty(strServer) && !string.IsNullOrEmpty(strUser) && !string.IsNullOrEmpty(strPass))
        //        {
        //            SQLApp.SetIniFile(strFileName, section, _serverDesc + idx, strDesc);
        //            SQLApp.SetIniFile(strFileName, section, _serverName + idx, strServer);
        //            SQLApp.SetIniFile(strFileName, section, _serverUID + idx, strUser);
        //            SQLApp.SetIniFile(strFileName, section, _serverPWD + idx, strPass);
        //        }
        //    }
        //    else
        //    {
        //        SQLApp.SetIniFile(strFileName, section, serverCnt, (index - 1).ToString());
        //        SQLApp.SetIniFile(strFileName, section, _serverDesc + idx, null);
        //        SQLApp.SetIniFile(strFileName, section, _serverName + idx, null);
        //        SQLApp.SetIniFile(strFileName, section, _serverUID + idx, null);
        //        SQLApp.SetIniFile(strFileName, section, _serverPWD + idx, null);
        //        SQLApp.SetIniFile(strFileName, section, _serverDBOld + idx, null);
        //    }
        //}
        #endregion

        #region Load config
        public static void LoadConfigIniFromServer(Control cbbData)
        {
            try
            {
                #region Code mới lấy thông tin từ file config

                string count = SQLApp.GetIniFile(strFileName, section, serverCnt);
                List<string> lst = new List<string>();
                for (int i = 0; i < Convert.ToInt32(count); i++)
                {
                    lst.Add(SQLApp.GetIniFile(strFileName, section, _serverDesc + (i + 1)));
                }
                if (cbbData is System.Windows.Forms.ComboBox)
                {
                    ((System.Windows.Forms.ComboBox)cbbData).Items.Clear();
                    ((System.Windows.Forms.ComboBox)cbbData).Items.AddRange(lst.ToArray());
                }
                else if (cbbData is DevExpress.XtraEditors.ComboBox)
                {
                    ((DevExpress.XtraEditors.ComboBox)cbbData).Properties.Items.Clear();
                    ((DevExpress.XtraEditors.ComboBox)cbbData).Properties.Items.AddRange(lst);
                }
                else if (cbbData is ComboBoxEdit)
                {
                    ((ComboBoxEdit)cbbData).Properties.Items.Clear();
                    ((ComboBoxEdit)cbbData).Properties.Items.AddRange(lst);
                }
                #endregion
            }
            catch
            {
            }
        }
        public static string[] LoadDatabaseByServer(int idx = -1)
        {
            List<string> lst = new List<string>();
            try
            {
                if (idx != -1)
                {
                    strServer = SQLApp.GetIniFile(strFileName, section, _serverName + (idx + 1));
                    strUserName = SQLApp.GetIniFile(strFileName, section, _serverUID + (idx + 1));
                    strPassWord = SQLApp.GetIniFile(strFileName, section, _serverPWD + (idx + 1));
                    strDBOld = SQLApp.GetIniFile(strFileName, section, _serverDBOld + (idx + 1));

                    SQLDBUtil.SwitchConnection(SQLDBUtil.ConnectionType, strServer, strDBOld, strUserName, strPassWord);
                    DataTable dtSource = SQLDBUtil.GetDataTableByDataSet(SQLDBUtil.GetAllDatabases());
                    if (dtSource != null)
                        lst = dtSource.Select().Select(x => x["name"].ToString()).ToList();
                }
            }
            catch
            {
                return null;
            }
            return lst.ToArray();
        }
        public static string GetServerConfig(int idx)
        {
            return SQLApp.GetIniFile(strFileName, section, _serverName + (idx + 1));
        }
        public static string GetUserNameConfig(int idx)
        {
            return SQLApp.GetIniFile(strFileName, section, _serverUID + (idx + 1));
        }
        public static string GetPassWordConfig(int idx)
        {
            return SQLApp.GetIniFile(strFileName, section, _serverPWD + (idx + 1));
        }
        public static string GetDescriptionConfig(int idx)
        {
            return SQLApp.GetIniFile(strFileName, section, _serverDesc + (idx + 1));
        }
        public static string GetDBHistoryConfig(int idx)
        {
            return SQLApp.GetIniFile(strFileName, section, _serverDBOld + (idx + 1));
        }
        public static void SetDBHistoryConfig(int idx, string strDBOld)
        {
            SQLApp.SetIniFile(strFileName, section, _serverDBOld + (idx + 1), strDBOld);
        }
        public static void SetServerHistoryConfig(string strSVOld)
        {
            SQLApp.SetIniFile(strFileName, "LoginHistory", "ServerOld", strSVOld);
        }
        public static string GetServerHistoryConfig()
        {
            return SQLApp.GetIniFile(strFileName, "LoginHistory", "ServerOld");
        }
        public static int GetAnimateWindowTime()
        {
            int dwTime = 5000;
            dwTime = Convert.ToInt32(SQLApp.GetIniFile(strFileName, "AnimateWindow", "AnimateTime"));
            return dwTime;
        }
        #endregion
    }
    public class TextBoxUtil : TextBox
    {
        public TextBoxUtil()
        {
            this.KeyDown += textBox_KeyDown;
        }
        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
                this.SelectAll();
            if(e.Control && e.Shift && e.KeyCode == Keys.S)
            {
                string str = Clipboard.GetText();
                str = str.TrimEnd(Environment.NewLine.ToCharArray());
                string[] lst = str.Split('\n');
                this.ResetText();
                this.Text = string.Join(",", lst);
            }
        }
    }
}
