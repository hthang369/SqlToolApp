using SQLToolApp.Util;
using SQLToolApp.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace SQLToolApp.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        protected MainWindow mainWindow;
        private Util.FnFunctionList _FnFuncs;
        private Util.EnterFunctionList _EnterFuncs;
        private Util.OrderFunctionList _OrderFuncs;
        bool isBusy = false;
        public bool IsBusy
        {
            get { return isBusy; }
            set { SetProperty(ref isBusy, value); }
        }

        string title = string.Empty;
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        public FnFunctionList FnFuncs
        {
            get
            {
                if (_FnFuncs == null)
                    _FnFuncs = new FnFunctionList();
                return _FnFuncs;
            }
            set => _FnFuncs = value;
        }
        public EnterFunctionList EnterFuncs
        {
            get
            {
                if (_EnterFuncs == null)
                    _EnterFuncs = new EnterFunctionList();
                return _EnterFuncs;
            }
            set => _EnterFuncs = value;
        }
        public OrderFunctionList OrderFuncs
        {
            get
            {
                if (_OrderFuncs == null)
                    _OrderFuncs = new OrderFunctionList();
                return _OrderFuncs;
            }
            set => _OrderFuncs = value;
        }

        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName]string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        public BaseViewModel()
        {

        }

        protected FrameworkElement GetFrameworkElement(FrameworkElement w)
        {
            if (w.Parent != null) w = GetFrameworkElement(w.Parent as FrameworkElement);
            return w;
        }

        protected void GetMainWindow(FrameworkElement ctrl)
        {
            if (mainWindow == null)
                mainWindow = GetFrameworkElement(ctrl) as MainWindow;
        }

        protected void ShowWaitIndicator(FrameworkElement ctrl)
        {
            GetMainWindow(ctrl);
            if (mainWindow != null)
            {
                if (mainWindow._waitIndicatorService != null)
                    mainWindow._waitIndicatorService.ShowSplashScreen(mainWindow.Name);
                else
                    SQLAppLib.SQLAppWaitingDialog.ShowDialog();
            }

        }

        protected void HideWaitIndicator(FrameworkElement ctrl)
        {
            GetMainWindow(ctrl);
            if (mainWindow != null)
            {
                if (mainWindow._waitIndicatorService != null)
                    mainWindow._waitIndicatorService.HideSplashScreen();
                else
                    SQLAppLib.SQLAppWaitingDialog.HideDialog();
            }

        }

        protected virtual bool CanExecute()
        {
            return true;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
    class RelayCommand<T> : ICommand
    {
        private readonly Predicate<T> _canExecute;
        private readonly Action<T> _execute;

        public RelayCommand(Predicate<T> canExecute, Action<T> execute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");
            _canExecute = canExecute;
            _execute = execute;

        }

        public bool CanExecute(object parameter)
        {
            try
            {
                return _canExecute == null ? true : _canExecute((T)parameter);
            }
            catch
            {
                return true;
            }
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        public event EventHandler CanExecuteChanged;
    }
    public class ElementObject : INotifyPropertyChanged
    {
        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName]string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
