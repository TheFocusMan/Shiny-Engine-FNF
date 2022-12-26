using Shiny_Engine_FNF.Code.Controls;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace Shiny_Engine_FNF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }
        protected override void OnLoadCompleted(NavigationEventArgs e)
        {
            //DiscordClient.Initalize(); 
            base.OnLoadCompleted(e);
        }
        protected override void OnExit(ExitEventArgs e)
        {
            DiscordClient.Shutdown();
            base.OnExit(e);
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
    }
}
