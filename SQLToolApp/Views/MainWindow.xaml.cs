using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DevExpress.Mvvm;
using DevExpress.Xpf.Core;
using SQLAppLib;
using SQLToolApp.ViewModels;

namespace SQLToolApp.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ThemedWindow
    {
        MainViewModel mainViewModel;
        public readonly ISplashScreenService _waitIndicatorService;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = mainViewModel = new MainViewModel(this);
            this.ctrlFrom.RegisterName("svrFrom", true);
            this.ctrlTo.RegisterName("svrTo", true);
            //_waitIndicatorService = ServiceContainer.Default.GetService<ISplashScreenService>("WaitIndicatorService");
            //if (_waitIndicatorService != null)
            //    _waitIndicatorService.ShowSplashScreen(this.Name);
        }

        private void ThemedWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //DXSplashScreen.Close();
            //LoadingDecorator loading = new LoadingDecorator();
            ////loading.spl
            //loading.Visibility = Visibility.Visible;
            //SQLApp.ShowAnimate(this.Handle, Util.FunctionList.GetAnimateWindowTime());
            //InputGesture gesture = new KeyGesture(Key.D1, ModifierKeys.Alt, "ALT+D1");
            //string hex = "637E5FC15EZ8831";
            //hex = "A4167B221DFC2D3";
            //System.Security.Cryptography.SymmetricAlgorithm algorithm = System.Security.Cryptography.DES.Create();
            //System.Security.Cryptography.DESCryptoServiceProvider desCrypto = new System.Security.Cryptography.DESCryptoServiceProvider();
            //algorithm.Padding = System.Security.Cryptography.PaddingMode.Zeros;
            //System.Security.Cryptography.ICryptoTransform transform = algorithm.CreateDecryptor();
            //System.IO.MemoryStream stream = new System.IO.MemoryStream();
            //System.Security.Cryptography.CryptoStream stream2 = new System.Security.Cryptography.CryptoStream(stream, transform, System.Security.Cryptography.CryptoStreamMode.Write);
            //byte[] bytes = new ASCIIEncoding().GetBytes(hex);
            //stream2.Write(bytes, 0, bytes.Length);
            //stream2.FlushFinalBlock();
            //byte[] buffer = stream.ToArray();
            //stream2.Close();
            //List<string> funcHotKeysIni = SQLApp.GetKeysIniFile(System.Windows.Forms.Application.StartupPath + "\\config.ini", "HotKeyApp");
            //Parallel.ForEach(funcHotKeysIni, (item) =>
            //{
            //    InputBinding inputBinding = new KeyBinding(mainViewModel.KeyBindingCommand, Key.D1, ModifierKeys.Alt);
            //    inputBinding.CommandParameter = "Alt+D1";
            //    InputBindings.Add(inputBinding);
            //});
            //if (_waitIndicatorService != null)
            //    _waitIndicatorService.HideSplashScreen();
            SQLAppWaitingDialog.HideDialog();
            //InputBinding inputBinding = new KeyBinding(mainViewModel.KeyBindingCommand, Key.Enter, ModifierKeys.None);
            //inputBinding.CommandParameter = "None+Enter";
            //lstFunction.InputBindings.Add(inputBinding);
        }

        private void ThemedWindow_KeyDown(object sender, KeyEventArgs e)
        {
            mainViewModel.KeyActionCommand(sender, e);
        }

        private void LstFunction_KeyDown(object sender, KeyEventArgs e)
        {
            
            mainViewModel.KeyViewActionCommand(sender, e);
        }

        private void LstFunction_SelectedItemChanged(object sender, DevExpress.Xpf.Grid.SelectedItemChangedEventArgs e)
        {

        }

        private void FrmMain_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DXMessageBox.Show("Bạn có chắc muốn thoát?", "Thông báo", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }
    }
}
