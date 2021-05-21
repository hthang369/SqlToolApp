using SQLToolApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SQLToolApp.Views
{
    /// <summary>
    /// Interaction logic for DatabaseConfigView.xaml
    /// </summary>
    public partial class DatabaseConfigView : UserControl
    {
        public DatabaseConfigView()
        {
            InitializeComponent();
        }

        private void TableView_CellValueChanged(object sender, DevExpress.Xpf.Grid.CellValueChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Cell.Value.ToString()) && e.Column.FieldName.EndsWith("Table"))
            {
                string[] lstAllColumns = SQLAppLib.SQLDBUtil.GetTableColumns(e.Cell.Value.ToString()).Rows.Cast<DataRow>().Select(x => x.Field<string>("COLUMN_NAME")).ToArray();
                if (e.Column.FieldName.Contains("Primary"))
                    (DataContext as DatabaseConfigViewModel).listColumnByTable = lstAllColumns;
                else
                    (DataContext as DatabaseConfigViewModel).listColumnByForeignTable = lstAllColumns;
            }
        }

        private void TableView_RowUpdated(object sender, DevExpress.Xpf.Grid.RowEventArgs e)
        {

        }
    }
}
