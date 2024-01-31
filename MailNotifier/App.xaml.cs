using MailNotifier.Core;
using MailNotifier.MVVM.Model;
using MailNotifier.MVVM.ViewModel;
using MailNotifier.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Windows;
using Windows.Foundation.Collections;

namespace MailNotifier
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;

        public App()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddSingleton<MainWindow>(provider => new MainWindow
            {
                DataContext = provider.GetRequiredService<MainViewModel>()
            });

            services.AddSingleton<MainViewModel>();
            services.AddSingleton<Services.INavigationService, NavigationService>();

            services.AddSingleton<Func<Type, ViewModel>>(serviceProvider => viewModelType => (ViewModel)serviceProvider.GetRequiredService(viewModelType));

            _serviceProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            ToastNotificationManagerCompat.OnActivated += ToastNotificationManagerCompat_OnActivated;

            double screenHeight = SystemParameters.FullPrimaryScreenHeight;
            double screenWidth = SystemParameters.FullPrimaryScreenWidth;

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Top = (screenHeight - mainWindow.Height - 800) / 0x00000002;
            mainWindow.Left = (screenWidth - mainWindow.Width + 1500) / 0x00000002;
            mainWindow.Show();
            base.OnStartup(e);
        }

        private async void ToastNotificationManagerCompat_OnActivated(ToastNotificationActivatedEventArgsCompat e)
        {
            // Obtain the arguments from the notification
            ToastArguments args = ToastArguments.Parse(e.Argument);

            switch (args["action"])
            {
                case "view": 
                    {
                        NotificationHandler.View();
                        break;
                    }
                case "reply":
                    {
                        ValueSet userInput = e.UserInput;
                        await NotificationHandler.ReplyAsync(userInput, args);
                        break;
                    }
                default: break;
            } 
        }
    }
}
