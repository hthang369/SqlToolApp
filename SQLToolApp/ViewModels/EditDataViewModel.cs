using System;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm;

namespace SQLToolApp.ViewModels
{
    [POCOViewModel]
    public class EditDataViewModel : BasePopupViewModel
    {
        private object _strQuery;
        public object strQuery
        {
            get => _strQuery;
            set => SetProperty(ref _strQuery, value);
        }
        Views.EditDataView view;
        Util.FnFunctionList FnFuncs;
        public EditDataViewModel(Views.EditDataView _view)
        {
            view = _view;
            FnFuncs = new Util.FnFunctionList();
        }
        protected override void KeyBindingActionCommand(object x)
        {
            FnFuncs.ShowResultDataView(view.reditData.Text);
        }
    }
}