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
using System.Windows.Shapes;
using DevExpress.Xpf.Core;
using SQLToolApp.Util;
using SQLToolApp.ViewModels;

namespace SQLToolApp.Views
{
    /// <summary>
    /// Interaction logic for ResultView.xaml
    /// </summary>
    public partial class ResultView : UserControl
    {
        public ResultView()
        {
            InitializeComponent();
        }

        private void DgvDataResult_AutoGeneratingColumn(object sender, DevExpress.Xpf.Grid.AutoGeneratingColumnEventArgs e)
        {
            
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //if(this.DataContext != null)
            //{
            //    try
            //    {
            //        ResultViewModel resultView = this.DataContext as ResultViewModel;
            //        if (resultView.isTabControl == Visibility.Hidden && resultView.DataResults.Count > 0)
            //        {
            //            foreach (DataResults item in resultView.DataResults)
            //            {
            //                DXTabItem tabItem = new DXTabItem();
            //                tabItem.Header = item.Title;
                            
                            
            //                tabControl.Items.Add(tabItem);
            //                //tabControl.InsertTabItem(tabItem, resultView.DataResults.IndexOf(item));
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {

            //    }
            //}
        }
    }
}
