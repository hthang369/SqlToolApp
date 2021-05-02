using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SQLToolApp.ViewModels
{
    public class BasePopupViewModel : BaseViewModel
    {
        private string _header;
        public string Header
        {
            get => _header;
            set => SetProperty(ref _header, value);
        }
        private Visibility _isTabControl;
        public Visibility isTabControl
        {
            get => _isTabControl;
            set => SetProperty(ref _isTabControl, value);
        }
        private Visibility _isNoTabControl;
        public Visibility isNoTabControl
        {
            get => _isNoTabControl;
            set => SetProperty(ref _isNoTabControl, value);
        }
        public ICommand KeyBindingCommand { get; set; }

        public BasePopupViewModel()
        {
            KeyBindingCommand = new RelayCommand<object>((x) => CanExecute(), (x) => KeyBindingActionCommand(x));
        }

        protected virtual void KeyBindingActionCommand(object x)
        {
        }
    }
}
