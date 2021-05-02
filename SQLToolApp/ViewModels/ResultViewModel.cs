using DevExpress.Xpf.Core;
using SQLToolApp.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SQLToolApp.ViewModels
{
    public class ResultViewModel : BasePopupViewModel
    {
        private Dictionary<string, string> _dataResults;
        public Dictionary<string, string> DataResults
        {
            get => _dataResults;
            set => SetProperty(ref _dataResults, value);
        }

        private ObservableCollection<DXTabItem> _lstTabItems;
        public ObservableCollection<DXTabItem> lstTabItems
        {
            get => _lstTabItems;
            set => SetProperty(ref _lstTabItems, value);
        }

    }
    public class DataResults
    {
        public string strName { get; set; }
        public string strQuery { get; set; }
    }
}
