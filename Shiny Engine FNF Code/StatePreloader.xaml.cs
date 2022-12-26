using Shiny_Engine_FNF.Code;
using Shiny_Engine_FNF.Code.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfGame;

namespace Shiny_Engine_FNF.Code
{
    /// <summary>
    /// Interaction logic for StatePreloader.xaml
    /// </summary>
    public partial class StatePreloader : Page
    {
        public StatePreloader()
        {
            InitializeComponent();
        }


        public static Key[] muteKeys = { Key.D0 };
        public static Key[] volumeDownKeys = { Key.Add, Key.Subtract };
        public static Key[] volumeUpKeys = { Key.Add };

        public const string ModName = "ShinyEngine";
        long lastTime;

        private static string FileSizeToString(long size)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = size;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            string result = String.Format("{0:0.##} {1}", len, sizes[order]);
            return result;
        }
        private void UpdateFPS()
        {
            if ((System.DateTime.Now.Ticks - lastTime) >= TimeSpan.TicksPerSecond / 10)
            {
                framerateText.Text = string.Format("FPS:{0:.}", Math.Min(StaticTimer.MaxFrameRate, StaticTimer.CurrentFrameRate));
                lastTime = DateTime.Now.Ticks;
                memoryText.Text = String.Format("Memory: {0}", FileSizeToString(GC.GetTotalMemory(false) * 10));
            }
        }

        public void FireKeyUp(KeyEventArgs e)
        {
            OnKeyUp(e);
        }

        public void FireKeyDown(KeyEventArgs e)
        {
            OnKeyDown(e);
        }


        // אני לא יודע אנגלית טוב אז יהיה קצב הערות
        protected override void OnKeyUp(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.D0:
                case Key.NumPad0:
                    ButtonAutomationPeer peer = new(audiobutton);
                    IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider; // custom volume bar
                    invokeProv.Invoke();
                    PullTheVolBar();
                    break;
                case Key.Subtract:
                case Key.OemMinus:
                    _isonmute = false;
                    PullTheVolBar();
                    volumeSlider.Value -= 10;
                    break;
                case Key.Add:
                case Key.OemPlus:
                    _isonmute = false;
                    PullTheVolBar();
                    volumeSlider.Value += 10;
                    break;
            }
            if (contentC.Content is MusicBeatState musicBeatState)
            {
                musicBeatState.FireKeyUp(e);
            }
            base.OnKeyUp(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (contentC.Content is MessageState state)
            {
                if (e.Key == Key.Enter)
                {
                    if (Directory.Exists("Assets"))
                        MusicBeatState.SwichState(contentC.NavigationService, Content as Grid, new TitleState());
                    else
                    {
                        Flicker.StartFlicker(new FrameworkElement[] { state }, 1, 0.2);
                        System.Media.SystemSounds.Asterisk.Play();
                    }
                }
            }
            if (contentC.Content is MusicBeatState musicBeatState)
            {
                musicBeatState.FireKeyDown(e);
            }
            base.OnKeyDown(e);
        }

        private void PullTheVolBar()
        {
            _waitsex = 5;
            if (_task == null)
            {
                _task = new Task(async () =>
                {
                    Tween tween = new(-67, 0, new SineEase() { EasingMode = EasingMode.EaseIn }, 0.3);
                    tween.UpdateValue += (sender, e) => audiocpntroler.Margin =
                    new Thickness(audiocpntroler.Margin.Left, e, audiocpntroler.Margin.Right, audiocpntroler.Margin.Bottom);
                    tween.Start();
                    while (_waitsex > 0)
                    {
                        await Task.Delay(1000);
                        _waitsex--;
                    }
                    tween = new Tween(0, -67, new SineEase() { EasingMode = EasingMode.EaseIn }, 0.3);
                    tween.UpdateValue += (sender, e) => audiocpntroler.Margin =
                    new Thickness(audiocpntroler.Margin.Left, e, audiocpntroler.Margin.Right, audiocpntroler.Margin.Bottom);
                    tween.Start();
                    _task = null;
                });
                _task.Start();
            }
        }
        private Task _task;
        private int _waitsex = 0;

        private bool _isonmute = false;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _isonmute = _isonmute != true;
            volumeSlider.IsEnabled = !_isonmute;
            Slider_ValueChanged(this, new RoutedPropertyChangedEventArgs<double>(0, _isonmute ? 0 : volumeSlider.Value)); // להתריע על השתקה
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue > 65)
                audiobutton.Background = VerifyImageBrush("max_Audio_Icon");
            else if (e.NewValue > 32)
                audiobutton.Background = VerifyImageBrush("med_Audio_Icon");
            else if (e.NewValue > 0)
                audiobutton.Background = VerifyImageBrush("low_Audio_Icon");
            if (_isonmute || e.NewValue == 0)
                audiobutton.Background = VerifyImageBrush("mute_Audio_Icon");
            Sound.Volume = (float)(e.NewValue / 100);
            VolumeText.Text = ((int)e.NewValue).ToString();
            if (IsInitialized)
                PullTheVolBar();
        }

        private ImageBrush VerifyImageBrush(string key) // This is for if the dispose was not too agressive
        {
            var d = key;
            var brush = Resources[d] as ImageBrush;
            try
            {
                var h = (brush.ImageSource.Metadata as BitmapMetadata)?.Format;
            }
            catch (Exception ex)
            {
                if (ex is not NotSupportedException)
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.UriSource =ColorSwap.MakePackUri($"{key}.png");
                    image.CacheOption = BitmapCacheOption.OnLoad; // הורדת זיכרון מיותר
                    image.CreateOptions = BitmapCreateOptions.IgnoreImageCache; // ישות עצמעית
                    image.EndInit();
                    brush.ImageSource = image;
                }
            }
            return brush;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists("Assets"))
                MusicBeatState.SwichState(contentC.NavigationService, Content as Grid, new TitleState());
            else MusicBeatState.SwichState(contentC.NavigationService, Content as Grid, new MessageState("Assets Directory not exsist\n Press Enter to Continue"));

            this.RegisterTimer(UpdateFPS);
        }
    }
}
