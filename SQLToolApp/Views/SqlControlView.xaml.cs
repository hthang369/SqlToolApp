using SQLToolApp.ViewModels;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for SqlControlView.xaml
    /// </summary>
    public partial class SqlControlView : UserControl
    {
        private SqlControlViewModel userControlViewModel;
        public SqlControlView()
        {
            InitializeComponent();
            DataContext = userControlViewModel = new SqlControlViewModel(this);
        }
    }
}
