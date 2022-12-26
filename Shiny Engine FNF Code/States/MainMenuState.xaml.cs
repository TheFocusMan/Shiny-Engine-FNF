using Shiny_Engine_FNF.Code;
using Shiny_Engine_FNF.Code.Controls;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using WpfGame;
using WpfGame.AnimationsSheet;
using WpfGame.Controls;

namespace Shiny_Engine_FNF.Code
{
    /// <summary>
    /// Interaction logic for MainMenuState.xaml
    /// </summary>
    public partial class MainMenuState : MusicBeatState
    {
        public static bool firstStart = true;
        public static bool finishedFunnyMove = false;
        int curSelected = 0;

        string[] optionShit = new string[] { "story_mode", "freeplay", "donate", "options" };

        public const string nightly = "";

        public const string kadeEngineVer = "1.6";
        public const string gameVer = "0.2.7.1";

        public MainMenuState()
        {
            InitializeComponent();

            DiscordClient.ChangePresence("In the Menus", null);

            //Sound.Play(Paths.Music("freakyMenu"), delegate { });

            //persistentUpdate = persistentDraw = true;
            image.Source = new BitmapImage(new Uri(Paths.Image("menuBG"))); // הרקעים
            magenta.Source = new BitmapImage(new Uri(Paths.Image("menuBGMagenta")));

            //image.RenderTransform = magenta.RenderTransform = new ScaleTransform() { ScaleX = optionShit.Length / 2.7, ScaleY = optionShit.Length / 2.7 };

            for (int i = 0; i < optionShit.Length; i++)
            {
                var menuItem = new Sprite();
                var tex = Paths.GetSparrowAtlas("mainmenu\\menu_"+optionShit[i], menuItem);
                menuItem.Tag = tex;
                menuItem.MouseDown += (o, e) =>
                { 
                    if (e.ButtonState == Mouse.LeftButton)
                    {
                        var tag = menuItem.Tag as Sparrow2AnimationSheet;
                        if (tag.CurrentAnimationName == tag.AnimationChase["selected"])
                            OnKeyUp(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromDependencyObject(this), 0, Key.Enter));
                        else
                        {
                            curSelected = menuItems.Children.IndexOf(o as UIElement)-1;
                            ChangeItem(1); // כדי שיהיה צלילים
                        }
                        e.Handled = true;
                    }
                };
                menuItem.VerticalAlignment = VerticalAlignment.Top;
                menuItem.HorizontalAlignment = HorizontalAlignment.Center;
                menuItem.Margin = new Thickness(0, 2000, 0, 0);
                tex.AnimationChase.Add("idle", optionShit[i] + " basic");
                tex.AnimationChase.Add("selected", optionShit[i] + " white");
                tex.PlayAnimation2("idle");
                menuItems.Children.Add(menuItem);
                // menuItem.scrollFactor.set();
                if (firstStart)
                {
                    Tween tween = new(menuItem.Margin.Top, 60 + (i * 160), new ExponentialEase() { EasingMode = EasingMode.EaseInOut }, 1 + (i * 0.25));
                    tween.UpdateValue += (sender, e) => menuItem.Margin = new Thickness(0, e, 0, 0);
                    tween.Complite += async (sender, e) =>
                    {
                        await Task.Delay(100);
                        ChangeItem();
                        finishedFunnyMove = true;
                    };
                    tween.Start();
                }
                else menuItem.Margin = new Thickness(0, 60 + (i * 160), 0, 0);
            }
            var yScroll = Math.Max(0.25 - (0.05 * (optionShit.Length - 4)), 0.1);
            image.Width = image.Source.Width;
            image.Height = image.Source.Height;
            image.setGraphicSize(image.Width * 1.175,image.Height + image.Height*yScroll);
            magenta.Width = image.Width;
            magenta.Height = image.Height;
            firstStart = false;

            //FlxG.camera.follow(camFollow, null, 0.60 * (60 / FlxG.save.data.fpsCap));
            versionShit.Text = "FNF: " + gameVer + " Shiny Engine";

            // NG.core.calls.event.logEvent('swag').send();


            /* if (FlxG.save.data.dfjk)
                 controls.setKeyboardScheme(KeyboardScheme.Solo, true);
             else
                 controls.setKeyboardScheme(KeyboardScheme.Duo(true), true);
            */
            ChangeItem();

        }

        bool selectedSomethin = false;

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (!selectedSomethin && finishedFunnyMove)
            {
                if (e.Key == Key.PageUp || e.Key == Key.Up) ChangeItem(-1);
                if (e.Key == Key.PageDown || e.Key == Key.Down) ChangeItem(1);

                if (e.Key == Key.Back)
                    SwichState(new TitleState(),false);

                if (e.Key == Key.Enter)
                {
                    if (optionShit[curSelected] == "donate")
                        OpenUrl("https://ninja-muffin24.itch.io/funkin");
                    else
                    {
                        selectedSomethin = true;
                        Sound.Play(Paths.Sound("confirmMenu"));
                        Flicker.StartFlicker(new FrameworkElement[] { magenta , menuItems.Children[curSelected] as FrameworkElement}, 1.1, 0.15); // אפקט שיש במשחק המקורי
                        foreach (FrameworkElement element in menuItems.Children)
                        {
                            if (curSelected != menuItems.Children.IndexOf(element))
                            {
                                Tween tween = new(element.Opacity, 0, new QuarticEase() { EasingMode = EasingMode.EaseOut }, 1.3);
                                tween.UpdateValue += (sender, val) => element.Opacity = 0;
                                tween.Complite += (sender, val) => {/*spr.kill();*/};
                                tween.Start();
                            }
                            else GoToState();
                        }
                    }
                }
            }
            base.OnKeyUp(e);
        }

        private static void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

        protected override void Update()
        {
            if (Sound.PlayersCount == 0)
                Sound.Play(Paths.Music("freakyMenu"), delegate { });

            /*if (FlxG.Sound.music.volume < 0.8)
            {
                FlxG.Sound.music.volume += 0.5 * FlxG.elapsed;
            }*/

            base.Update();
        }

        private void GoToState()
        {
            var dis = this.Dispatcher;
            Task.Factory.StartNew(delegate
            {
                Thread.Sleep(1500);
                dis.BeginInvoke(new Action(() =>
                {
                    var daChoice = optionShit[curSelected];
                    switch (daChoice)
                    {
                        case "story_mode":
                            SwichState(new StoryMenuState());
                            Debug.WriteLine("Story Menu Selected");
                            break;
                        case "freeplay":
                            SwichState(new FreeplayState());
                            Debug.WriteLine("Freeplay Menu Selected");
                            break;

                        case "options":
                            SwichState(new OptionsMenu());
                            break;
                    }
                }));
            });
        }

        private void ChangeItem(int huh = 0)
        {
            if (huh != 0) Sound.Play(Paths.Sound("scrollMenu"));
            if (finishedFunnyMove)
            {
                curSelected += huh;

                if (curSelected >= menuItems.Children.Count)
                    curSelected = 0;
                if (curSelected < 0)
                    curSelected = menuItems.Children.Count - 1;
            }
            var tween = new Tween(magenta.Margin.Top, -(curSelected * 5),new QuadraticEase(),0.45);
            tween.UpdateValue += (sender, e) =>
            {
                image.Margin = magenta.Margin = new Thickness(magenta.Margin.Left, e, magenta.Margin.Right, magenta.Margin.Bottom);
                menuItems.Margin= new Thickness(magenta.Margin.Left, e*10, magenta.Margin.Right, magenta.Margin.Bottom);
            };
            foreach (FrameworkElement spr in menuItems.Children) // לא להוריד את הלולאה
            {
                var anim = spr.Tag as Sparrow2AnimationSheet;
                if (menuItems.Children.IndexOf(spr) == curSelected && finishedFunnyMove)
                    anim.PlayAnimation(anim.AnimationChase["selected"], anim.CurrentAnimationName != anim.AnimationChase["selected"], true);
                else anim.PlayAnimation(anim.AnimationChase["idle"], true);
            }
            tween.Start();
        }

        private void MusicBeatState_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) ChangeItem(-1);
            else ChangeItem(1);
        }
    }
}
