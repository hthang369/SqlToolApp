using System;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm;
using System.Collections.Generic;
using System.Windows.Input;
using System.Data;
using System.Windows.Controls;

namespace SQLToolApp.ViewModels
{
    [POCOViewModel]
    public class SqlControlViewModel : BaseViewModel
    {
        Control Parent;
        private List<string> _lstSqlType;
        public List<string> lstSqlType
        {
            get => _lstSqlType;
            set => SetProperty<List<string>>(ref _lstSqlType, value);
        }
        private List<string> _lstServers;
        public List<string> lstServers
        {
            get => _lstServers;
            set => SetProperty<List<string>>(ref _lstServers, value);
        }
        private DataTable _lstDatabase;
        public DataTable lstDatabase
        {
            get => _lstDatabase;
            set => SetProperty(ref _lstDatabase, value);
        }
        public ICommand selectedItemChangeCommand { get; set; }
        public ICommand serverSelectedItemChangeCommand { get; set; }
        public ICommand selectedDatabaseChangeCommand { get; set; }
        Util.OrderFunctionList OrderFuncs;
        public SqlControlViewModel(Control ctrl)
        {
            Parent = ctrl;
            selectedItemChangeCommand = new RelayCommand<object>((x) => { return true; }, x => ChangeServerBySqlType(x));
            serverSelectedItemChangeCommand = new RelayCommand<object>((x) => { return true; }, x => ChangeDatabaseBySqlType(x));
            selectedDatabaseChangeCommand = new RelayCommand<object>((x) => { return true; }, x => ChangeDatabaseByServer(x));
            lstSqlType = new List<string>()
            {
                SQLAppLib.SqlDbConnectionType.MySql.ToString(),
                SQLAppLib.SqlDbConnectionType.SqlServer.ToString(),
            };
            lstServers = new List<string>();
            lstDatabase = new DataTable();
            OrderFuncs = new Util.OrderFunctionList();
        }

        private void ChangeDatabaseByServer(object key)
        {
            string keySection = Convert.ToString((Parent as Views.SqlControlView).cboSqlType.SelectedItem);
            SQLAppLib.SQLDBUtil.ChangeDatabase(((SQLAppLib.SqlDbConnectionType)Enum.Parse(typeof(SQLAppLib.SqlDbConnectionType), keySection)), Convert.ToString(key));
            if(mainWindow.DataContext is MainViewModel)
                (mainWindow.DataContext as MainViewModel).lstSqlDbConnections.Add(Parent.Name, SQLAppLib.SQLDBUtil.CurrentDatabase.Clone() as SQLAppLib.SqlDbConnection);
        }

        private void ChangeServerBySqlType(object key)
        {
            ShowWaitIndicator(Parent);
            if (string.IsNullOrEmpty(Convert.ToString(key))) return;
            (Parent as Views.SqlControlView).cboServer.SelectedItem = null;
            (Parent as Views.SqlControlView).cboDatabase.SelectedItem = null;
            if(lstDatabase != null)
                lstDatabase.Clear();
            lstServers = OrderFuncs.LoadConfigInitToList(Convert.ToString(key));
            GetMainWindow(Parent);
            if (mainWindow.DataContext is MainViewModel)
            {
                (mainWindow.DataContext as MainViewModel).LoadMainMenu(Convert.ToString(key));
                mainWindow.lstFunction.RefreshData();
            }

            HideWaitIndicator(Parent);
        }

        private void ChangeDatabaseBySqlType(object idx)
        {
            ShowWaitIndicator(Parent);
            try
            {
                string keySection = Convert.ToString((Parent as Views.SqlControlView).cboSqlType.SelectedItem);
                if(lstDatabase != null)
                    lstDatabase.Reset();
                lstDatabase = OrderFuncs.LoadDatabaseByServer(keySection, (int)idx);
                //lstServers = Util.FunctionList.LoadConfigInitToList(Convert.ToString(key));
            }
            catch(Exception ex) { }
            HideWaitIndicator(Parent);
        }

        public void RefeshConfigServerByType(string strType)
        {
            ChangeServerBySqlType(strType);
        }
    }
}