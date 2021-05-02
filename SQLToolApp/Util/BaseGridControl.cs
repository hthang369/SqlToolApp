using DevExpress.Xpf.Core;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using SQLToolApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SQLToolApp.Util
{
    public class BaseGridControl : GridControl
    {
        public BaseGridControl() : base()
        {
            this.Margin = new Thickness(0);
            this.AutoGenerateColumns = AutoGenerateColumnsMode.AddNew;
            this.AutoGeneratingColumn += BaseGridControl_AutoGeneratingColumn;
            ICommand commandKey = new RelayCommand<object>((x) => true, (x) => KeyBindingActionCommand(x));
            AddInputHotKey(commandKey, Key.C, ModifierKeys.Alt);
            AddInputHotKey(commandKey, Key.M, ModifierKeys.Control);
            AddInputHotKey(commandKey, Key.H, ModifierKeys.Control);
            AddInputHotKey(commandKey, Key.I, ModifierKeys.Control);
        }

        private void KeyBindingActionCommand(object x)
        {
            HotKeyInfo hotKey = HotKeyGenerate.GenerateHotKeyByString(Convert.ToString(x));
            object focusVal = this.GetFocusedValue();
            switch (hotKey.modifierKey)
            {
                case ModifierKeys.Alt:
                    switch (hotKey.key)
                    {
                        case Key.C:
                            Clipboard.SetDataObject(focusVal);
                            break;
                    }
                    break;
                case ModifierKeys.Control:
                    TableView objTableView = (this.View as TableView);
                    switch (hotKey.key)
                    {
                        case Key.M:
                            string strColumn = string.Empty;
                            PromptForm.ShowText("Find Column", "Find Column", ref strColumn);
                            if (!string.IsNullOrEmpty(strColumn))
                            {
                                this.Columns.ToList().ForEach(col =>
                                {
                                    if (!col.FieldName.Equals("id"))
                                        col.Visible = false;
                                    strColumn.Split(',').ToList().ForEach(c =>
                                    {
                                        if (col.FieldName.Equals(c))
                                            col.Visible = true;
                                    });
                                });
                            }
                            break;
                        case Key.H:
                            int idx = this.Columns.ToList().FindIndex(c => c.FieldName.Equals(objTableView.FocusedColumn.FieldName));
                            DevExpress.Xpf.Grid.GridColumn column = this.Columns.ToList().ElementAtOrDefault(idx + 1);
                            objTableView.FocusedColumn.Visible = false;
                            if (column != null)
                                objTableView.FocusedColumn = column;
                            break;
                        case Key.I:
                            objTableView.ShowColumnChooser();
                            break;
                    }
                    break;
                case ModifierKeys.Shift:
                    break;
            }
        }

        private void AddInputHotKey(ICommand commandKey, Key key, ModifierKeys modifierKeys)
        {
            KeyBinding keyBinding = new KeyBinding(commandKey, key, modifierKeys);
            keyBinding.CommandParameter = string.Format("{0}+{1}", modifierKeys, key);
            InputBindings.Add(keyBinding);
        }

        private void BaseGridControl_AutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            if (e.Column.FieldType.Equals(typeof(String)))
                e.Column.AutoFilterCriteria = DevExpress.Data.Filtering.Helpers.ClauseType.Contains;
            else if (e.Column.FieldType.Equals(typeof(Int32)) ||
                e.Column.FieldType.Equals(typeof(UInt32)) ||
                e.Column.FieldType.Equals(typeof(Byte)))
                e.Column.AutoFilterCriteria = DevExpress.Data.Filtering.Helpers.ClauseType.Equals;
            else if (e.Column.FieldType.Equals(typeof(DateTime)) ||
                e.Column.FieldType.Equals(typeof(DateTime?)))
            {
                e.Column.EditSettings = new DateEditSettings();
                e.Column.EditSettings.DisplayFormat = "dd/MM/yyyy hh:mm:ss";
            }
            if (e.Column.FieldName == "id")
            {
                e.Column.Fixed = DevExpress.Xpf.Grid.FixedStyle.Left;
                e.Column.BestFitArea = BestFitArea.Rows;
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            object objFocusVal = this.GetFocusedValue();
            TableView objTableView = (this.View as TableView);
            switch (e.Key)
            {
                case Key.F:
                    objTableView.FocusedColumn.AutoFilterValue = objFocusVal;
                    break;
                case Key.Escape:
                    objTableView.FocusedColumn.AutoFilterValue = string.Empty;
                    break;
                case Key.F5:
                    DXTabItem tabItem = this.Parent as DXTabItem;
                    ViewModels.ResultViewModel resultView = tabItem.DataContext as ViewModels.ResultViewModel;
                    string strQuery = resultView.DataResults[tabItem.Header.ToString()];
                    SQLAppLib.SQLAppWaitingDialog.ShowDialog();
                    Task.Factory.StartNew(() =>
                    {
                        return SQLAppLib.SQLDBUtil.GetDataTable(strQuery);
                    }).ContinueWith(r => RefeshDataSource(r.Result), TaskScheduler.FromCurrentSynchronizationContext());
                    break;
            }
        }

        protected void RefeshDataSource(DataTable dtSource)
        {
            this.ItemsSource = dtSource;
            SQLAppLib.SQLAppWaitingDialog.HideDialog();
        }
    }
}
