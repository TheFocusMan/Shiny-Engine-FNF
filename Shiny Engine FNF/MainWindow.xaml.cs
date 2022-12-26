using Shiny_Engine_FNF.Code;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using WpfGame;

namespace Shiny_Engine_FNF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Show();

            contentC.Content = new StatePreloader();
        }


        protected override void OnClosed(EventArgs e)
        {
            Application.Current.Shutdown();
            Sound.StopAll();
            base.OnClosed(e);
        }

        // אני לא יודע אנגלית טוב אז יהיה קצב הערות
        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.SystemKey == Key.F10 && !(WindowState == WindowState.Maximized && WindowStyle == WindowStyle.None))
            {
                Width = 1282;
                Height = 752;
            }
            if (e.Key == Key.F11) // fullscreen
            {
                if (WindowState == WindowState.Maximized && WindowStyle == WindowStyle.None)
                {
                    WindowState = WindowState.Normal;
                    WindowStyle = WindowStyle.SingleBorderWindow;
                    this.ResizeMode = ResizeMode.CanResize;
                }
                else
                {
                    this.ResizeMode = ResizeMode.NoResize;
                    //Topmost = true;
                    WindowStyle = WindowStyle.None;
                    WindowState = WindowState.Maximized;
                }
            }
            if (contentC.Content is StatePreloader musicBeatState)
            {
                musicBeatState.FireKeyUp(e);
            }
            base.OnKeyUp(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (contentC.Content is StatePreloader musicBeatState)
            {
                musicBeatState.FireKeyDown(e);
            }
            base.OnKeyDown(e);
        }
       
    }
}
