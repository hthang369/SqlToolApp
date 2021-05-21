using System;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm;
using SQLToolApp.Views;
using System.Linq;
using System.Data;
using System.Collections.Generic;
using SQLToolApp.Util;

namespace SQLToolApp.ViewModels
{
    [POCOViewModel]
    public class DatabaseConfigViewModel : BasePopupViewModel
    {
        DatabaseConfigView view;
        Util.FnFunctionList FnFuncs;

        private object _listPrimaryTables;
        public object listPrimaryTables
        {
            get => _listPrimaryTables;
            set => SetProperty(ref _listPrimaryTables, value);
        }

        private object _listColumnByTable;
        public object listColumnByTable
        {
            get => _listColumnByTable;
            set => SetProperty(ref _listColumnByTable, value);
        }

        private object _listForeignTables;
        public object listForeignTables
        {
            get => _listForeignTables;
            set => SetProperty(ref _listForeignTables, value);
        }

        private object _listColumnByForeignTable;
        public object listColumnByForeignTable
        {
            get => _listColumnByForeignTable;
            set => SetProperty(ref _listColumnByForeignTable, value);
        }

        private List<DatabaseConfigInfo> _listDataSourceConfig;
        public List<DatabaseConfigInfo> listDataSourceConfig
        {
            get => _listDataSourceConfig;
            set => SetProperty(ref _listDataSourceConfig, value);
        }

        public DatabaseConfigViewModel(DatabaseConfigView _view)
        {
            view = _view;
            FnFuncs = new Util.FnFunctionList();
            listDataSourceConfig = new List<DatabaseConfigInfo>();

            listPrimaryTables = SQLAppLib.SQLDBUtil.GetDataTableByDataSet(SQLAppLib.SQLDBUtil.GetAllTables()).Rows.Cast<DataRow>().Select(x => x[0].ToString());
            listForeignTables = SQLAppLib.SQLDBUtil.GetDataTableByDataSet(SQLAppLib.SQLDBUtil.GetAllTables()).Rows.Cast<DataRow>().Select(x => x[0].ToString());
        }
        protected override void KeyBindingActionCommand(object x)
        {
            //FnFuncs.ShowResultDataView(view.reditData.Text);
        }
    }
}