using MailNotifier.Core;
using MailNotifier.MVVM.Model;
using MailNotifier.Services;
using System.Windows;
using System.Windows.Input;

namespace MailNotifier.MVVM.ViewModel
{
    class MainViewModel : Core.ViewModel
    {
        public MainViewModel(INavigationService navigation)
        {
            Navigation = navigation;  
            CloseCommand = new RelayCommand(Close);
            MinimizeCommand = new RelayCommand(Minimize);

            StartCommand = new RelayCommand(Start);
            StopCommand = new RelayCommand(Stop);

            Monitor = new();
            TrayIcon = "MailNotifierIcon.ico";
        }

        #region [Properties]

        public MailMonitor Monitor { get; set; }

        private string _trayIcon;
        public string TrayIcon
        {
            get { return _trayIcon; }
            set { _trayIcon = value; OnPropertyChanged(); }
        }

        private INavigationService _navigation;
        public INavigationService Navigation
        {
            get { return _navigation; }
            set { _navigation = value; OnPropertyChanged(); }
        }

        #endregion

        #region [Commands]

        public ICommand CloseCommand { get; set; }
        public ICommand MinimizeCommand { get; set; }
        public ICommand StartCommand { get; set; }
        public ICommand StopCommand { get; set; }

        private async void Start(object obj)
        {
            await Monitor.StartMonitoringAsync();
            TrayIcon = "MailNotifierIndicatorIcon.ico";
        }
        private async void Stop(object obj)
        {
            await Monitor.StopMonitoringAsync();
            TrayIcon = "MailNotifierIcon.ico";
        }
        public void Close(object obj) => Application.Current.Shutdown(); 
        public void Minimize(object obj) => Application.Current.MainWindow.WindowState = WindowState.Minimized;

        #endregion 
    }
}
