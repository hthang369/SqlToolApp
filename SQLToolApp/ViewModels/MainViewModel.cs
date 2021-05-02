using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using SQLAppLib;
using SQLToolApp.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace SQLToolApp.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        Window frmMain;
        private List<FunctionListObject> _lstFunctions;
        public List<FunctionListObject> lstFunctions
        {
            get => _lstFunctions;
            set => SetProperty(ref _lstFunctions, value);
        }
        private Dictionary<string, SqlDbConnection> _lstSqlDbConnections;
        public Dictionary<string, SqlDbConnection> lstSqlDbConnections
        {
            get => _lstSqlDbConnections;
            set => SetProperty(ref _lstSqlDbConnections, value);
        }
        private double _iLvsWidth;
        public double iLvsWidth
        {
            get => _iLvsWidth;
            set => SetProperty(ref _iLvsWidth, value);
        }
        public ICommand btnAddCommand { get; set; }
        public ICommand btnEditCommand { get; set; }
        public ICommand btnDelCommand { get; set; }
        public ICommand btnVerCommand { get; set; }
        public ICommand btnCompareCommand { get; set; }
        public ICommand btnRefeshCommand { get; set; }
        public ICommand btnCompareDataCommand { get; set; }
        public ICommand KeyBindingCommand { get; set; }

        public MainViewModel(Window window)
        {
            frmMain = window;
            iLvsWidth = window.Width - 30;
            lstFunctions = new List<FunctionListObject>();
            lstSqlDbConnections = new Dictionary<string, SqlDbConnection>();

            btnAddCommand = new RelayCommand<object>((x) => CanExecute(), (x) => ActionCommand(x));
            btnEditCommand = new RelayCommand<object>((x) => CanExecute(), (x) => ActionCommand(x));
            btnDelCommand = new RelayCommand<object>((x) => CanExecute(), (x) => ActionCommand(x));
            btnVerCommand = new RelayCommand<object>((x) => CanExecute(), (x) => ActionCommand(x));

            btnCompareDataCommand = new RelayCommand<object>((x) => CanExecute(), (x) => ActionCommand(x));
            btnCompareCommand = new RelayCommand<object>((x) => CanExecute(), (x) => ActionCommand(x));
            btnRefeshCommand = new RelayCommand<object>((x) => CanExecute(), (x) => ActionCommand(x));

            KeyBindingCommand = new RelayCommand<object>((x) => CanExecute(), (x) => KeyBindingActionCommand(x));
            
            LoadMainMenu();
        }

        public void LoadMainMenu(string strFolder = "")
        {
            lstFunctions.Clear();
            string strPath = System.Windows.Forms.Application.StartupPath + "\\Scripts\\config.ini";
            string strSectionCaption = "Captions";
            string strSectionFunc = "Funcs";
            string strPathFunc = System.Windows.Forms.Application.StartupPath + "\\Scripts";
            if(Directory.Exists(strPathFunc))
            {
                List<string> arrFunctions = Directory.GetFiles(strPathFunc, "*.sql").ToList();
                if (!string.IsNullOrEmpty(strFolder))
                    arrFunctions.AddRange(Directory.GetFiles(strPathFunc + "\\" + strFolder, "*.sql"));

                List<string> funcCaptions = SQLApp.GetKeysIniFile(strPath, strSectionCaption);
                Parallel.ForEach(arrFunctions, (item) =>
                {
                    lock (lstFunctions)
                    {
                        string strName = Path.GetFileNameWithoutExtension(item);
                        string strKey = funcCaptions.Find(x => x.Equals(strName));
                        string strText = string.IsNullOrEmpty(strKey) ? strName : SQLApp.GetIniFile(strPath, strSectionCaption, strKey);
                        lstFunctions.Add(new FunctionListObject { Name = strName, Text = strText, Path = item });
                    }
                });
                List<string> funcKeysIni = SQLApp.GetKeysIniFile(strPath, strSectionFunc);
                Parallel.ForEach(funcKeysIni, (item) =>
                {
                    lock (lstFunctions)
                    {
                        string strName = item;
                        string strKey = funcCaptions.Find(x => x.Equals(strName));
                        string strText = SQLApp.GetIniFile(strPath, strSectionCaption, strKey);
                        string strFuncName = SQLApp.GetIniFile(strPath, strSectionFunc, strKey);
                        lstFunctions.Add(new FunctionListObject { Name = strName, Text = strText, FuncName = strFuncName });
                    }
                });
            }
        }

        private void ActionCommand(object item)
        {
            SimpleButton btn = item as SimpleButton;
            FrameworkElement window = GetFrameworkElement(btn);
            MainWindow mainWindow = (window as MainWindow);
            string strType = Convert.ToString(mainWindow.ctrlFrom.cboSqlType.SelectedItem);
            //ResultViewModel resultView = new ResultViewModel();
            //DataResults data = new DataResults();
            //data.Title = "APPOs";
            //data.PinMode = TabPinMode.Left;
            //Task.Run(() =>
            //{
            //    data.DataSource = SQLAppLib.SQLDBUtil.GetDataTable("select * from APPOs");
            //});
            //Views.ResultView result = new Views.ResultView();
            //result.DataContext = data;
            //result.ShowDialog();
            if (string.IsNullOrEmpty(strType))
            {
                DXMessageBox.Show(mainWindow, "Vui lòng chọn Loại Sql", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            OrderFuncs.SetWindowParent(mainWindow);
            switch (Convert.ToString(btn.Content).ToLower().Replace(' ','_'))
            {
                case "add":
                case "edit":
                case "del":
                    int idx = mainWindow.ctrlFrom.cboServer.SelectedIndex;
                    if (idx == -1) idx = 0;
                    OrderFuncs.section = strType;
                    OrderFuncs.GetConfigConnectSQL(Convert.ToString(btn.Content), idx + 1);
                    mainWindow.ctrlFrom.RefeshConfigConnection(strType);
                    break;
                case "ver":
                    SQLDBUtil.CurrentDatabase = lstSqlDbConnections["ctrlFrom"];
                    OrderFuncs.GetDatabaseVersion((SqlDbConnectionType)Enum.Parse(typeof(SqlDbConnectionType), strType));
                    break;
                case "compare":
                    OrderFuncs.CompareDatabase(Util.BaseUtil.GetConnectionType(strType), (mainWindow.DataContext as MainViewModel).lstSqlDbConnections);
                    break;
                case "refresh":
                    mainWindow.ctrlFrom.RefeshConfigConnection(strType);
                    break;
            }
        }

        internal void KeyViewActionCommand(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    if (sender is TableView)
                    {
                        FunctionListObject func = (sender as TableView).SelectedRows.Cast<FunctionListObject>().FirstOrDefault();
                        if (!string.IsNullOrEmpty(func.FuncName))
                            EnterFuncs.CallMethodName(func.FuncName, this.frmMain);
                        else if (!string.IsNullOrEmpty(func.Path))
                            EnterFuncs.LoadQueryPath(func, this.frmMain);
                    }
                    break;
            }
        }

        internal void KeyActionCommand(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F3:
                    FnFuncs.ShowChangeTheme();
                    break;
                case Key.F9:
                    FnFuncs.ShowEditDataView();
                    break;
                case Key.F10:
                    FnFuncs.ShowFunctionList("FuncsF10", this.frmMain);
                    break;
                case Key.F11:
                    FnFuncs.ShowFunctionList("FuncsF11", this.frmMain);
                    break;
                case Key.System:
                    if (e.SystemKey == Key.F10)
                        FnFuncs.ShowFunctionList("FuncsF10", this.frmMain);
                    if (e.KeyboardDevice.Modifiers == ModifierKeys.Alt)
                    {

                    }
                    break;
            }
        }

        private void ShowEditDataView()
        {
            ViewModels.PopupViewModel popupView = new PopupViewModel();
            Views.BasePopupWindow popupWindow = new Views.BasePopupWindow() { DataContext = popupView };
            popupView.isNoTabControl = Visibility.Visible;
            popupView.isTabControl = Visibility.Hidden;
            popupWindow.waitLoadView.LoadingChild = new Views.EditDataView();
            popupWindow.Show();
            //popupWindow.tabResults.AddNewTabItem();
        }

        private void KeyBindingActionCommand(object x)
        {
            string[] arr = Convert.ToString(x).Split('+');
            Key key = (Key)Enum.Parse(typeof(Key), arr.LastOrDefault());
            ModifierKeys modifier = (ModifierKeys)Enum.Parse(typeof(ModifierKeys), arr.FirstOrDefault());
            switch (modifier)
            {
                case ModifierKeys.Alt:
                    switch (key)
                    {
                        case Key.D1:
                            EnterFuncs.FindModule(frmMain);
                            break;
                    }
                    break;
                case ModifierKeys.Control:
                    switch (key)
                    {
                        case Key.D1:
                            EnterFuncs.FindModule(frmMain);
                            break;
                    }
                    break;
                case ModifierKeys.Shift:
                    break;
                case ModifierKeys.None:
                    if (key == Key.Enter)
                    {
                        //Util.FunctionList.FindAllProcessLockedFile(frmMain);
                        EnterFuncs.ShowFlashDealView();
                    }
                    break;
            }
            //DevExpress.Xpf.Editors.SearchControl
        }

        private void KeyModifierCommand(ModifierKeys modifier, Key key)
        {

        }
    }

    public class FunctionListObject
    {
        public string Name { get; set; }
        public string Text { get; set; }
        public string Path { get; set; }
        public string FuncName { get; set; }
    }
}
