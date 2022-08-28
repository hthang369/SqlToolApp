using DevExpress.Xpf.Core;
using SQLAppLib;
using SQLToolApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace SQLToolApp.Util
{
    public class FnFunctionList : BaseFunctionList
    {
        #region Theme
        public string GetThemeName()
        {
            string _themeName = SQLApp.GetIniFile(StrFileName, StrThemeApp, StrThemeName);
            if (string.IsNullOrEmpty(_themeName)) _themeName = Properties.Resources.ThemeName;
            return _themeName;
        }

        public void SetThemeName(string themeName)
        {
            SQLApp.SetIniFile(StrFileName, StrThemeApp, StrThemeName, themeName);
        }

        public void ShowChangeTheme()
        {
            string _themeName = ApplicationThemeHelper.ApplicationThemeName;
            if (PromptForm.ShowCombobox("Change Theme", "Theme Name", Theme.Themes.Select(x => x.Name).ToArray(), ref _themeName) == MessageBoxResult.Cancel)
                return;

            SetThemeName(_themeName);

            //System.Diagnostics.Process.Start(System.Windows.Forms.Application.ExecutablePath);
            //Environment.Exit(Environment.ExitCode);

            ApplicationThemeHelper.ApplicationThemeName = _themeName;
        }
        #endregion

        #region Function list cmd
        public void ShowFunctionList(string strFuncName, Window frmParent)
        {
            string sourceUrl = SQLApp.GetIniFile(strFileName, "SourceCode", "SourceUrl");
            bool isReturn = false;
            if (string.IsNullOrEmpty(sourceUrl) || !Directory.Exists(sourceUrl))
            {
                isReturn = true;
                FolderBrowserDialog folder = new FolderBrowserDialog();
                if (folder.ShowDialog() == DialogResult.OK)
                {
                    SQLApp.SetIniFile(strFileName, "SourceCode", "SourceUrl", folder.SelectedPath);
                    isReturn = false;
                }
            }
            if (isReturn) return;
            List<string> lstFuncs = SQLApp.GetKeysIniFile(strCfgScriptName, strFuncName, 3000);
            List<FunctionListObject> lstObjectFuncs = new List<FunctionListObject>();
            lstFuncs.ForEach(x =>
            {
                string caption = SQLApp.GetIniFile(strCfgScriptName, "Captions", x);
                if (string.IsNullOrEmpty(caption)) caption = string.Join(" ", x.ToUpper().Split('_'));
                lstObjectFuncs.Add(new FunctionListObject { Name = x, Text = caption });
            });
            PromptForm._frmParent = frmParent;
            string value = string.Empty;
            MessageBoxResult messageResult = PromptForm.ShowCombobox("Function List In Source", "Function Name", lstObjectFuncs.Select(x => x.Text).ToArray(), ref value);
            if (messageResult == MessageBoxResult.OK)
            {
                FunctionListObject functionObj = lstObjectFuncs.Find(x => x.Text.Equals(value));
                string strKey = (functionObj != null) ? functionObj.Name : string.Empty;
                string functionName = SQLApp.GetIniFile(strCfgScriptName, strFuncName, strKey);
                if (functionName.StartsWith("Cmd"))
                {
                    functionObj.FuncName = functionName;
                    ExecutedScriptCommand(functionObj, frmParent);
                }
                else
                {
                    CallMethodName(functionName, frmParent);
                }
            }
        }
        
        private void ExecutedScriptCommand(FunctionListObject functionObj, Window frmParent)
        {
            //string funcName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            //Lấy tên function hiện tại
            string strScript = GetScriptCommandByFuncName(functionObj.FuncName);
            strScript = GenerateScriptWithParameters(functionObj, strScript, frmParent);
            if (string.IsNullOrEmpty(strScript))
            {
                ShowMessengeInfo("Không có mã thực thi");
                return;
            }
            SQLAppWaitingDialog.ShowDialog();
            string sourceUrl = GetSourceCodePath();
            string output = SQLApp.ExecutedPowerShell(string.Format("cd {0} {1} {2}", sourceUrl, Environment.NewLine, strScript));
            string strType = SQLApp.GetIniFile(strFileCfgScript, strDynPara, functionObj.Name + "Show");
            if (!string.IsNullOrEmpty(strType))
            {
                ShowScriptCommand(output, strType);
                SQLAppWaitingDialog.HideDialog();
                return;
            }
            SQLAppWaitingDialog.HideDialog();
            ShowMessengeInfo(output);
        }
        public string GetScriptCommandByFuncName(string strFuncName)
        {
            return SQLApp.GetIniFile(string.Concat(strPath, "ScriptCommand.ini"), "Laravel", strFuncName);
        }
        protected void ShowScriptCommand(string output, string strType)
        {
            if (strType.Equals("table"))
            {
                string[] arr = output.Split('\n');
                DataTable dtSoure = new DataTable("Route List");
                int idx = 0;
                arr.ToList().ForEach(r =>
                {
                    string[] row = r.Split('|');
                    if (row.Length > 1)
                    {
                        if (idx == 0)
                        {
                            row.Where(c => !string.IsNullOrEmpty(c.Trim())).ToList().ForEach(c =>
                            {
                                DataColumn col = new DataColumn(c.Trim(), typeof(string));
                                dtSoure.Columns.Add(col);
                            });
                        }
                        else
                            dtSoure.LoadDataRow(row.Where(c => !string.IsNullOrEmpty(c.Trim())).Select(c => c.Trim()).ToArray(), false);
                        idx++;
                    }
                });
                ShowResultData(null, dtSoure);
            }
            else if (strType.Equals("table_json"))
            {
                DataTable dtSoure = new DataTable("Route List");
                typeof(RouteInfo).GetProperties().ToList().ForEach(c => dtSoure.Columns.Add(c.Name));
                List<RouteInfo> lstRoutes = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RouteInfo>>(output);
                lstRoutes.ForEach(r => dtSoure.Rows.Add(r.Domain, r.Method, r.URI, r.Middleware, r.Name, r.Action));
                ShowResultData(null, dtSoure);
            }
            else
            {
                SaveFileDialog saveFile = new SaveFileDialog();
                saveFile.Filter = "*." + strType.Replace("file", "");
                if (saveFile.ShowDialog() == DialogResult.OK)
                {
                    SQLApp.WriteFile(saveFile.FileName, output);
                }
            }
        }
        #endregion

        public void ChangeDbLaravel(Window frmParent)
        {
            if (!CheckSelectedDB()) return;

            string sourceUrl = GetSourceCodePath();
            string filePath = Directory.GetFiles(sourceUrl, "*.env").FirstOrDefault();
            string[] lines = File.ReadAllLines(filePath);
            string[] lstDBs = SQLDBUtil.GetDataTableByDataSet(SQLDBUtil.GetAllDatabases()).Select().Select(x => x[0].ToString()).ToArray();
            string DBVal = lines.ToList().Find(x => x.StartsWith("DB_DATABASE")).Split('=').LastOrDefault();
            if (PromptForm.ShowCombobox("Change Database Name", "Database Name", lstDBs, ref DBVal) == MessageBoxResult.OK)
            {
                SQLAppWaitingDialog.ShowDialog();
                int idx = lines.ToList().FindIndex(x => x.StartsWith("DB_DATABASE"));
                lines[idx] = string.Format("{0}={1}", "DB_DATABASE", DBVal);
                File.WriteAllLines(filePath, lines);
                FunctionListObject functionObj = new FunctionListObject();
                functionObj.FuncName = "CmdLaravelConfigCache";
                ExecutedScriptCommand(functionObj, frmParent);
                SQLAppWaitingDialog.HideDialog();
            }
        }

        public void MoveFileFolder(Window frmParent)
        {
            string sourceUrl = GetSourceCodePath();
            SQLAppWaitingDialog.ShowDialog();
            List<string> funcKeysIni = SQLApp.GetKeysIniFile(strPath, "FileToFolder");
            foreach (string item in funcKeysIni)
            {
                string strText = SQLApp.GetIniFile(strPath, "FileToFolder", item);

                string[] arr = item.Split('.');

                DirectoryInfo directory = Directory.CreateDirectory(sourceUrl + "/" + string.Join("/", arr));

                foreach (string file in strText.Split('|'))
                {
                    string[] lstFile = Directory.GetFiles(sourceUrl + "/" + arr.FirstOrDefault(), file, SearchOption.AllDirectories);

                    foreach (var filePath in lstFile)
                    {
                        string[] lines = File.ReadAllLines(filePath);

                        for (int i = 0; i < lines.Length - 1; i++)
                        {
                            foreach (string item1 in funcKeysIni)
                            {
                                string[] arr1 = item1.Split('.');
                                if (lines[i].Contains("App\\" + arr1.FirstOrDefault()))
                                {
                                    lines[i].Replace("App\\" + arr1.FirstOrDefault(), "App\\" + string.Join("\\", arr1));
                                }
                            }
                        }
                        File.WriteAllLines(filePath, lines);

                        File.Move(filePath, directory.FullName);
                    }
                }
            }
            SQLAppWaitingDialog.HideDialog();
        }

        public void ExecuteScriptFile(Window frmParent)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                string allLines = File.ReadAllText(openFile.FileName);
                string[] allChar = new string[] { ";" + Environment.NewLine };
                string[] arrLines = allLines.Split(allChar, StringSplitOptions.None);
                SQLAppWaitingDialog.ShowDialog();
                foreach (string line in arrLines)
                {
                    try
                    {
                        SQLDBUtil.ExecuteNonQuery(line);
                    }
                    catch (Exception ex)
                    {
                        SQLAppWaitingDialog.HideDialog();
                        ShowMessengeWarning("Có lỗi xảy ra");
                    }
                }
                SQLAppWaitingDialog.HideDialog();
                ShowMessengeInfo("Thành công");
            }
        }

        public void ShowEditDataView()
        {
            Views.EditDataView view = new Views.EditDataView();
            ViewModels.EditDataViewModel popupView = new EditDataViewModel(view);
            popupView.Title = "T-SQL";
            popupView.Header = "T-SQL";
            AddEventByView(view.reditData, popupView, Key.F9, ModifierKeys.None);
            AddEventByView(view.reditData, popupView, Key.G, ModifierKeys.Control);
            ShowPopupViewModal(popupView, view);
        }

        public void SettingDbConfig(Window frmParent)
        {
            Views.DatabaseConfigView view = new Views.DatabaseConfigView();
            DatabaseConfigViewModel popupView = new DatabaseConfigViewModel(view);
            ShowPopupViewModal(popupView, view);
        }
    }
}