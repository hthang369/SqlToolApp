using DevExpress.Xpf.Core;
using SQLAppLib;
using SQLToolApp.ViewModels;
using SQLToolApp.Views;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace SQLToolApp.Util
{
    public class BaseFunctionList : IDisposable
    {
        private string _section = "SqlServer";
        private string _serverCnt = "ServerCount";
        private string _serverDesc = "Descreiption";
        private string _serverName = "Server";
        private string _serverUID = "UID";
        private string _serverPWD = "PWD";
        private string _serverDBOld = "DB_Old";
        private string _startUpPath = System.Windows.Forms.Application.StartupPath;
        private string _strFileName = "";
        private string _strServer;
        private string _strDatabase;
        private string _strUserName;
        private string _strPassWord;
        private string _strDBOld;
        private string _strPath = "";
        private string _strFileCfgScript = "";
        private string _strCfgScriptName = "";
        public Dictionary<string, string> lstFuncLst;
        public string strDynPara = "DynPara";
        private ViewModels.ResultViewModel popupView;
        private ViewModels.CompareResultViewModel comparePopupView;
        private Views.BasePopupWindow popupWindow;
        private string _strThemeApp = "ThemeApp";
        private string _strThemeName = "ThemeName";
        private string _strFontApp = "FontApp";
        private string _ctrlFrom = "ctrlFrom";
        private string _ctrlTo = "ctrlTo";
        private MainWindow mainWindow;

        public string section
        {
            get => _section;
            set => _section = value;
        }
        public string strFileName
        {
            get => string.IsNullOrEmpty(StrFileName) ? _startUpPath + "\\config.ini" : StrFileName;
            set => StrFileName = value;
        }
        public string strServer { get => _strServer; set => _strServer = value; }
        public string strDatabase { get => _strDatabase; set => _strDatabase = value; }
        public string strUserName { get => _strUserName; set => _strUserName = value; }
        public string strPassWord { get => _strPassWord; set => _strPassWord = value; }
        public string strDBOld { get => _strDBOld; set => _strDBOld = value; }
        public string strPath
        {
            get => !string.IsNullOrEmpty(_strPath) ? _strPath : _startUpPath + "\\Scripts\\";
            set => _strPath = value;
        }
        public string strFileCfgScript
        {
            get => !string.IsNullOrEmpty(_strFileCfgScript) ? _strFileCfgScript : strPath + "scripts.ini";
            set => _strFileCfgScript = value;
        }
        public string strCfgScriptName
        {
            get => !string.IsNullOrEmpty(_strCfgScriptName) ? _strCfgScriptName : strPath + "config.ini";
            set => _strCfgScriptName = value;
        }
        public string serverCnt { get => _serverCnt; set => _serverCnt = value; }
        public string ServerDesc { get => _serverDesc; set => _serverDesc = value; }
        public string ServerName { get => _serverName; set => _serverName = value; }
        public string ServerUID { get => _serverUID; set => _serverUID = value; }
        public string ServerPWD { get => _serverPWD; set => _serverPWD = value; }
        public string StrFileName { get => _strFileName; set => _strFileName = value; }
        public string StrThemeApp { get => _strThemeApp; set => _strThemeApp = value; }
        public string StrThemeName { get => _strThemeName; set => _strThemeName = value; }
        public string StrFontApp { get => _strFontApp; set => _strFontApp = value; }
        public string ServerDBOld { get => _serverDBOld; set => _serverDBOld = value; }
        public string CtrlFrom { get => _ctrlFrom; set => _ctrlFrom = value; }
        public string CtrlTo { get => _ctrlTo; set => _ctrlTo = value; }

        public void SetWindowParent(Window frm)
        {
            mainWindow = frm as MainWindow;
        }

        public string GetSourceCodePath()
        {
            return SQLApp.GetIniFile(strFileName, "SourceCode", "SourceUrl");
        }

        public bool CheckSelectedDB()
        {
            if (mainWindow != null && string.IsNullOrEmpty(Convert.ToString(mainWindow.ctrlFrom.cboDatabase.SelectedItem)))
            {
                ShowMessenge("Vui lòng chọn DB", "Thông báo", MessageBoxImage.Warning);
                return false;
            }
            return true;
        }
        public void ShowMessenger(int idx)
        {
            if (idx != 0)
                ShowMessengeInfo("Thành công!");
            else
                ShowMessengeWarning("Thất bại!");
        }
        public void ShowMessenge(string strContent, string strTitle, MessageBoxImage messageImage)
        {
            DXMessageBox.Show(strContent, strTitle, MessageBoxButton.OK, messageImage);
        }
        public void ShowMessengeInfo(string strContent, string strTitle = "Thông báo")
        {
            ShowMessenge(strContent, strTitle, MessageBoxImage.Information);
        }
        public void ShowMessengeWarning(string strContent, string strTitle = "Thông báo")
        {
            ShowMessenge(strContent, strTitle, MessageBoxImage.Warning);
        }

        protected void AddEventByView(System.Windows.Controls.Control view, BasePopupViewModel viewModel, Key _key, ModifierKeys _modifierKeys)
        {
            InputBinding inputBinding = new KeyBinding(viewModel.KeyBindingCommand, _key, _modifierKeys);
            inputBinding.CommandParameter = string.Format("{0}+{1}", _modifierKeys.ToString(), _key.ToString());
            view.InputBindings.Add(inputBinding);
        }
        protected static DataTable ConvertDataRowToTable(IEnumerable<DataRow> lstRows, string strTableName)
        {
            if (lstRows.Count() > 0)
            {
                using (DataTable dtSame = lstRows.CopyToDataTable())
                {
                    dtSame.TableName = strTableName;
                    return dtSame;
                }
            }
            else
            {
                using (DataTable dtSame = new DataTable())
                {
                    dtSame.TableName = strTableName;
                    return dtSame;
                }
            }
        }

        #region CallMethod
        public void CallMethodName(string strFuncName, Window frmParent)
        {
            MethodInfo method = this.GetType().GetMethod(strFuncName, (((BindingFlags)BindingFlags.Public) | ((BindingFlags)BindingFlags.Static)));
            if (method != null)
            {
                if (method.GetParameters().Length == 0)
                    method.Invoke(null, (object[])null);
                else
                    method.Invoke(null, new Window[] { frmParent });
            }
            else
            {
                method = this.GetType().GetMethod(strFuncName, (((BindingFlags)BindingFlags.Public) | ((BindingFlags)BindingFlags.Instance)));
                if (method != null)
                {
                    if (method.GetParameters().Length == 0)
                        method.Invoke(this, (object[])null);
                    else
                        method.Invoke(this, new Window[] { frmParent });
                }
            }
        }
        protected string GenerateScriptWithParameters(FunctionListObject functionObj, string strScript, Window frmParent)
        {
            string strCnt = SQLApp.GetIniFile(strFileCfgScript, strDynPara, string.Concat(functionObj.Name, "Cnt"));
            int iCnt = string.IsNullOrEmpty(strCnt) ? 0 : Convert.ToInt32(strCnt);
            if (iCnt > 0)
            {
                PromptForm._frmParent = frmParent;
                string strValue = "";
                for (int i = 1; i <= iCnt; i++)
                {
                    string param = Convert.ToString(SQLApp.GetIniFile(strFileCfgScript, strDynPara, string.Concat(functionObj.Name, "Name", i)));
                    string strParamDesc = Convert.ToString(SQLApp.GetIniFile(strFileCfgScript, strDynPara, string.Concat(functionObj.Name, "Desc", i)));
                    strValue = Convert.ToString(SQLApp.GetIniFile(strFileCfgScript, strDynPara, string.Concat(functionObj.Name, "Val", i)));
                    string strValList = Convert.ToString(SQLApp.GetIniFile(strFileCfgScript, strDynPara, string.Concat(functionObj.Name, "ValList", i)));
                    string strListFolder = Convert.ToString(SQLApp.GetIniFile(strFileCfgScript, strDynPara, string.Concat(functionObj.Name, "ListFolder", i)));
                    string strListFile = Convert.ToString(SQLApp.GetIniFile(strFileCfgScript, strDynPara, string.Concat(functionObj.Name, "ListFile", i)));
                    string strDefaultValue = Convert.ToString(SQLApp.GetIniFile(strFileCfgScript, strDynPara, string.Concat(functionObj.Name, "DefaultValue", i)));
                    MessageBoxResult result;
                    string strTitle = string.IsNullOrEmpty(strParamDesc) ? param : strParamDesc;
                    if (!string.IsNullOrEmpty(strListFolder))
                    {
                        string sourceUrl = SQLApp.GetIniFile(strFileName, "SourceCode", "SourceUrl");
                        string[] lstModules = Directory.GetDirectories(string.Concat(sourceUrl, "\\", strListFolder));
                        string[] lstModuleNames = lstModules.ToList().Select(x => new DirectoryInfo(x).Name).ToArray();
                        result = PromptForm.ShowCombobox("Dynamic parameter for script: " + functionObj.Text, strTitle, lstModuleNames, ref strValue);
                    }
                    else if (!string.IsNullOrEmpty(strListFile))
                    {
                        string sourceUrl = SQLApp.GetIniFile(strFileName, "SourceCode", "SourceUrl");
                        string[] lstFiles = Directory.GetFiles(string.Concat(sourceUrl, "\\", strListFile));
                        string[] lstFileNames = lstFiles.ToList().Select(x => new DirectoryInfo(x).Name).ToArray();
                        result = PromptForm.ShowCombobox("Dynamic parameter for script: " + functionObj.Text, strTitle, lstFileNames, ref strValue);
                    }
                    else if (!string.IsNullOrEmpty(strValList))
                    {
                        string[] arrSource = strValList.Split('|');
                        if ((strValList.StartsWith("SHOW") || strValList.StartsWith("SELECT")) && arrSource.Length == 1)
                        {
                            List<string> lstDBSystems = (new string[] { "mysql", "information_schema", "performance_schema" }).ToList();
                            arrSource = SQLDBUtil.GetDataTable(strValList).Rows.Cast<DataRow>().Where(x =>
                            {
                                return (lstDBSystems.IndexOf(x[0].ToString()) == -1);
                            }).Select(x => x[0].ToString()).ToArray();
                        }
                        result = PromptForm.ShowCombobox("Dynamic parameter for script: " + functionObj.Text, strTitle, arrSource, ref strValue);
                    }
                    else if (string.IsNullOrEmpty(strDefaultValue))
                        result = PromptForm.ShowText("Dynamic parameter for script: " + functionObj.Text, strTitle, ref strValue);
                    else
                        result = MessageBoxResult.OK;

                    if (result == MessageBoxResult.Cancel)
                    {
                        return (string.IsNullOrEmpty(param)) ? strScript : string.Empty;
                    }
                    if (!string.IsNullOrEmpty(param) && string.IsNullOrEmpty(strDefaultValue))
                        strScript = strScript.Replace(param, strValue);
                    if (!string.IsNullOrEmpty(strValList) && !string.IsNullOrEmpty(strValue))
                        strScript = string.Concat(strScript, " ", strValue);
                    if (!string.IsNullOrEmpty(strDefaultValue))
                        strScript = strScript.Replace(param, strDefaultValue);
                }
            }
            return strScript;
        }

        private string getValue(object strVal)
        {
            if (strVal is String)
                return string.Format("'{0}'", strVal);
            else if (strVal is DateTime)
                return string.Format("'{0}'", strVal);
            else
                return strVal.ToString();
        }

        public ViewModels.ResultViewModel GetResultPopupView()
        {
            if (popupView == null)
                popupView = new ResultViewModel();
            return popupView;
        }
        public Views.BasePopupWindow GetPopupWindow()
        {
            if (popupWindow == null)
                popupWindow = new Views.BasePopupWindow();
            return popupWindow;
        }
        public ViewModels.CompareResultViewModel GetCompareResultPopupView()
        {
            if (comparePopupView == null)
                comparePopupView = new CompareResultViewModel();
            return comparePopupView;
        }
        
        protected void AddControlsToGrid(BasePopupViewModel viewModel, DataTable dtSource, string strQuery)
        {
            if (viewModel is ResultViewModel)
            {
                ResultViewModel result = (viewModel as ResultViewModel);
                if (result.lstTabItems == null)
                    result.lstTabItems = new System.Collections.ObjectModel.ObservableCollection<DXTabItem>();
                if (result.DataResults == null)
                    result.DataResults = new Dictionary<string, string>();
                DXTabItem tabItem = new DXTabItem();
                tabItem.FontSize = Convert.ToDouble(SQLApp.GetIniFile(strFileName, StrFontApp, "FontSize"));
                BaseGridControl gridControl = new BaseGridControl();
                BaseTableView tableView = new BaseTableView();
                gridControl.View = tableView;
                gridControl.ItemsSource = dtSource;
                tabItem.Header = dtSource.TableName;
                tabItem.Content = gridControl;
                result.lstTabItems.Add(tabItem);
                result.DataResults.Add(dtSource.TableName, strQuery);
            }
        }
        protected void ShowPopupViewModal(BasePopupViewModel viewModel, System.Windows.Controls.UserControl view)
        {
            Views.BasePopupWindow popupWindow = GetPopupWindow();
            popupWindow.DataContext = viewModel;
            viewModel.isNoTabControl = Visibility.Visible;
            viewModel.isTabControl = Visibility.Hidden;
            popupWindow.waitLoadView.LoadingChild = view;
            popupWindow.Closed += PopupWindow_Closed;
            //(view as Views.ResultView).tabControl.KeyUp += TabControl_KeyUp;
            //ICommand commandKey = new RelayCommand<object>((x) => true, (x) => KeyBindingActionCommand(x));
            //InputBinding input = new KeyBinding(commandKey, Key.V, ModifierKeys.Alt);
            //input.CommandParameter = "Alt+V";
            //(view as Views.ResultView).tabControl.InputBindings.Add(input);
            if (view is Views.ResultView)
            {
                (view as Views.ResultView).tabControl.TabContentCacheMode = DevExpress.Xpf.Core.TabContentCacheMode.None;
                (view as Views.ResultView).tabControl.TabRemoved += TabControl_TabRemoved;
                (view as Views.ResultView).tabControl.TabIndex = (view as Views.ResultView).tabControl.Items.Count - 1;
            }
            popupWindow.Show();
        }

        protected void ShowResultData(Window frmParent, DataTable dtSource, string strQuery = "")
        {
            if (dtSource != null)
            {
                ViewModels.ResultViewModel popupView = GetResultPopupView();
                popupView.Title = "T-SQL";
                popupView.Header = "T-SQL Result";
                Task.Factory.StartNew(() =>
                {
                    return dtSource;
                }).ContinueWith(r => AddControlsToGrid(popupView, r.Result, strQuery), TaskScheduler.FromCurrentSynchronizationContext());
                ShowPopupViewModal(popupView, new Views.ResultView());
            }
            else if (!string.IsNullOrEmpty(strQuery))
            {
                ShowResultDataView(strQuery);
            }
        }

        public void ShowResultDataView(string strQuery)
        {
            ViewModels.ResultViewModel popupView = GetResultPopupView();
            popupView.Title = "T-SQL";
            popupView.Header = "T-SQL Result";

            Task.Factory.StartNew(() =>
            {
                return SQLAppLib.SQLDBUtil.GetDataTable(strQuery);
            }).ContinueWith(r => AddControlsToGrid(popupView, r.Result, strQuery), TaskScheduler.FromCurrentSynchronizationContext());
            ShowPopupViewModal(popupView, new Views.ResultView());
        }

        private void PopupWindow_Closed(object sender, EventArgs e)
        {
            popupWindow = null;
            popupView = null;
        }

        private void TabControl_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.SystemKey == Key.LeftAlt || e.SystemKey == Key.RightAlt)
            {
                if (e.Key == Key.V)
                {
                    DevExpress.Xpf.Core.DXTabItem tabItem = ((sender as DevExpress.Xpf.Core.DXTabControl).SelectedTabItem as DevExpress.Xpf.Core.DXTabItem);

                }
            }
        }

        private void TabControl_TabRemoved(object sender, DevExpress.Xpf.Core.TabControlTabRemovedEventArgs e)
        {
            if ((sender as DevExpress.Xpf.Core.DXTabControl).Items.Count == 0)
            {
                popupWindow.Close();
                popupWindow = null;
            }
        }
        #endregion

        #region Get Config
        public string GetItemConfig(string keySection, string keyPrefix, int idx)
        {
            if (string.IsNullOrEmpty(keySection)) keySection = section;
            return SQLApp.GetIniFile(strFileName, keySection, keyPrefix + (idx + 1));
        }
        public string GetServerConfig(string keySection, int idx)
        {
            return GetItemConfig(keySection, _serverName, idx);
        }
        public string GetUserNameConfig(string keySection, int idx)
        {
            return GetItemConfig(keySection, _serverUID, idx);
        }
        public string GetPassWordConfig(string keySection, int idx)
        {
            return GetItemConfig(keySection, _serverPWD, idx);
        }
        public string GetDescriptionConfig(string keySection, int idx)
        {
            return GetItemConfig(keySection, ServerDesc, idx);
        }

        public string GetServerConfig(int idx)
        {
            return SQLApp.GetIniFile(strFileName, section, _serverName + (idx + 1));
        }
        public string GetUserNameConfig(int idx)
        {
            return SQLApp.GetIniFile(strFileName, section, _serverUID + (idx + 1));
        }
        public string GetPassWordConfig(int idx)
        {
            return SQLApp.GetIniFile(strFileName, section, _serverPWD + (idx + 1));
        }
        public string GetDescriptionConfig(int idx)
        {
            return SQLApp.GetIniFile(strFileName, section, ServerDesc + (idx + 1));
        }

        public void Dispose()
        {
        }
        #endregion
    }


}
