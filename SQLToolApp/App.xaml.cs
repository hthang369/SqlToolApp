using DevExpress.Xpf.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SQLToolApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            SQLAppLib.SQLAppWaitingDialog.ShowDialog();
            using(Util.FnFunctionList FnFuncs = new Util.FnFunctionList())
            {
                ApplicationThemeHelper.ApplicationThemeName = FnFuncs.GetThemeName();
            }
            base.OnStartup(e);
        }
    }
}
