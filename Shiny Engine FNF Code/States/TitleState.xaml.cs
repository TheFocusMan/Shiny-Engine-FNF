using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shiny_Engine_FNF.Code;
using Shiny_Engine_FNF.Code.Controls;
using Shiny_Engine_FNF.Code.FridayNightFunkinDataBase;
using Shiny_Engine_FNF.Code.Properties;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using WpfGame;
using WpfGame.AnimationsSheet;
using WpfGame.Controls;

namespace Shiny_Engine_FNF.Code
{
    class TitleData
    {
        public float titlex = 0;
        public float titley = 0;
        public float startx = 0;
        public float starty = 0;
        public float gfx = 0;
        public float gfy = 0;
        public string backgroundSprite = "";
        public int bpm = 0;
    }
    /// <summary>
    /// Interaction logic for TitleState.xaml
    /// </summary>

    public partial class TitleState : MusicBeatState
    {

        bool danceLeft = false;
        readonly Grid _bgtextp = new() { Background = Brushes.Black };
        readonly StackPanel textGroup = new() { VerticalAlignment = VerticalAlignment.Center, Background = Brushes.Black };
        readonly Sprite ngSpr = new() { Height = 374, Width = 374,Antialiasing = Settings.Default.Antialiasing ,HorizontalAlignment = HorizontalAlignment.Center};
        string[] curWacky = Array.Empty<string>();
        static bool initialized = false;
        bool transitioning = false;
        FlxSound _musicobj;
        readonly TitleData titleJSON;

        public TitleState()
        {
            InitializeComponent();

            if (File.Exists("modsList.txt"))
            {

                var list = File.ReadAllLines("modsList.txt");
                var foundTheTop = false;
                foreach (var i in list)
                {
                    var dat = i.Split('|');
                    if (dat[1] == "1" && !foundTheTop)
                    {
                        foundTheTop = true;
                        Paths.CurrentModDirectory = dat[0];
                    }

                }
            }

            var path = Path.Combine("mods", Paths.CurrentModDirectory, "images\\gfDanceTitle.json");
            //trace(path, FileSystem.exists(path));
            if (!File.Exists(path))
                path = "mods\\images\\gfDanceTitle.json";
            //trace(path, FileSystem.exists(path));
            if (!File.Exists(path))
                path = "assets\\images\\gfDanceTitle.json";
            //trace(path, FileSystem.exists(path));
            titleJSON = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(path)).ToObject<TitleData>();
            logoBl.Margin = new Thickness(titleJSON.titlex, titleJSON.titley, 0, 0);
            gfDance.Margin = new Thickness(titleJSON.gfx, titleJSON.gfy, 0, 0);
            titleText.Margin = new Thickness(titleJSON.startx, titleJSON.starty, 0, 0);
            StartIntro();
        }

        private void StartIntro()
        {
            // persistentUpdate = true;
            gfDance.Frames = Paths.GetSparrowAtlas("gfDanceTitle", gfDance, "preload",24);
            titleText.Frames = Paths.GetSparrowAtlas("titleEnter", titleText,null, 24);
            logoBl.Frames = Paths.GetSparrowAtlas("logoBumpin", logoBl, null, 24);

            logoBl.Antialiasing = Settings.Default.Antialiasing;
            titleText.Antialiasing = Settings.Default.Antialiasing;
            gfDance.Antialiasing = Settings.Default.Antialiasing;

            logoBl.Frames.PlayAnimation2("logo bumpin");

            curWacky = File.ReadAllLines(Paths.Txt("introText"));

            titleText.Frames.PlayAnimation2("Press Enter to Begin");
            this.controlgrid.Children.Add(_bgtextp);
            _bgtextp.Children.Add(textGroup);
            ngSpr.Source = Sprite.CreateGoodImage(Paths.Image("newgrounds_logo"));
            var credTextShit = new Alphabet(new Point(0, 0), "ninjamuffin99\\nPhantomArcade\\nkawaisprite\\nevilsk8er", true)
            {
                HorizontalAlignment = HorizontalAlignment.Center
            };
            //credGroup.VerticalAlignment = VerticalAlignment.Center;
            this.controlgrid.Children.Add(credTextShit);

            // credTextShit.alignment = CENTER;

            credTextShit.Visibility = Visibility.Hidden;

            var tw = new Tween(credTextShit.Margin.Top, credTextShit.Margin.Top + 20, new QuadraticEase() { EasingMode = EasingMode.EaseInOut }, 2.9);
            tw.UpdateValue += (sender, e) => credTextShit.Margin = new Thickness(credTextShit.Margin.Left, e, 0, 0);
            tw.Start();
            //FlxG.mouse.visible = false;

            if (initialized)
                SkipIntro();
            else
            {
                //MainWindow.DoEffectTransition(0.7, Colors.Black,false);
                _musicobj = Sound.Play(Paths.Music("freakyMenu"), delegate { });

                Conductor.ChangeBPM(titleJSON.bpm);
                initialized = true;
            }

            // credGroup.add(credTextShit);
        }

        private void CreateCoolText(string[] textArray)
        {
            for (int i = 0; i < textArray.Length; i++)
            {
                Alphabet money = new(new Point(0, 0), textArray[i], true, false)
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                money.Margin = new Thickness(money.Margin.Left, 0, 0, 0);
                //credGroup.Children.Add(money);
                textGroup.Children.Add(money);
            }
        }

        private void AddMoreText(string text)
        {
            Alphabet coolText = new(new Point(0, 0), text, true, false)
            {
                HorizontalAlignment = HorizontalAlignment.Center
            };
            coolText.Margin = new Thickness(coolText.Margin.Left, 0, 0, 0);
            //credGroup.Children.Add(coolText);
            textGroup.Children.Add(coolText);
        }

        private void DeleteCoolText()
        {
            while (textGroup.Children.Count > 0)
            {
                //credGroup.Children.Remove(credGroup.Children[0]);
                textGroup.Children.Remove(textGroup.Children[0]);
            }
        }

        protected override void BeatHit()
        {
            base.BeatHit();
            danceLeft = !danceLeft;
            gfDance.Frames.SortAnimation("gfDance", danceLeft ? new int[] { 29, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 } : new int[] { 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29 });
            if (!skippedIntro)
            {
                switch (CurBeat)
                {
                    case 0:
                        DeleteCoolText();
                        break;
                    case 1:
                        CreateCoolText(new string[] { "ninjamuffin99", "phantomArcade", "kawaisprite", "evilsk8er" });
                        break;
                    // credTextShit.visible = true;
                    case 3:
                        AddMoreText("present");
                        break;
                    // credTextShit.text += "\npresent...";
                    // credTextShit.addText();
                    case 4:
                        DeleteCoolText();
                        break;
                    // credTextShit.visible = false;
                    // credTextShit.text = "In association \nwith";
                    // credTextShit.screenCenter();
                    case 5:
                        CreateCoolText(new string[] { "In Partnership", "with" });
                        break;
                    case 7:
                        AddMoreText("Newgrounds");
                        textGroup.Children.Add(ngSpr);
                        ngSpr.Visibility = Visibility.Visible;
                        break;
                    case 8:
                        DeleteCoolText();
                        ngSpr.Visibility = Visibility.Hidden;
                        break;
                    case 9:
                        CreateCoolText(new string[] { curWacky[0] });
                        break;
                    // credTextShit.visible = true;
                    case 11:
                        AddMoreText(curWacky[1]);
                        break;
                    // credTextShit.text += "\nlmao";
                    case 12:
                        DeleteCoolText();
                        break;
                    // credTextShit.visible = false;
                    // credTextShit.text = "Friday";
                    // credTextShit.screenCenter();
                    case 13:
                        AddMoreText("Friday");
                        break;
                    // credTextShit.visible = true;
                    case 14:
                        AddMoreText("Night");
                        break;
                    // credTextShit.text += "\nNight";
                    case 15:
                        AddMoreText("Funkin"); // credTextShit.text += "\nFunkin";
                        break;
                    case 16:
                        SkipIntro();
                        break;
                }
            }
        }
        bool _finished;
        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!transitioning && skippedIntro && !_finished)
                {
                    _finished = true;

                    Sound.Play(Paths.Sound("confirmMenu"), delegate { });
                    titleText.Frames.PlayAnimation2("ENTER PRESSED", true,true);
                    Task.Factory.StartNew(() =>
                    {
                        Task.Delay(600).Wait();
                        transitioning = true;
                        // FlxG.sound.music.stop();
                        MainMenuState.firstStart = true;
                        MainMenuState.finishedFunnyMove = false;
                        Dispatcher.Invoke(() => SwichState(new MainMenuState()));
                    });

                    // Sound.Play(Paths.Music("titleShoot"),delegate { });
                }
                if (!skippedIntro && initialized)
                    SkipIntro();
            }
            if (e.Key == Key.F)
            {

            }
            base.OnKeyUp(e);
        }

        protected override void Update()
        {
            if (_musicobj != null && !_musicobj.IsDisposed)
                Conductor.songPosition = _musicobj.Position;


            gfDance.Frames.PlayAnimation2("gfDance",false,false);
            if (!_finished) titleText.Frames.PlayAnimation2("Press Enter to Begin",false,false);
            // FlxG.watch.addQuick('amp', FlxG.sound.music.amplitude);

            /* if (FlxG.keys.justPressed.F)
             {
                 FlxG.fullscreen = !FlxG.fullscreen;
             }*/

            base.Update();
        }

        bool skippedIntro = false;

        private void SkipIntro()
        {
            if (!skippedIntro)
            {
                DoEffectTransition(2, Colors.White, true);
                //this.controlgrid.Children.Remove(credGroup);
                var logotr = new Tween(logoBl.Margin.Top, -100, new ExponentialEase() { EasingMode = EasingMode.EaseInOut }, 1.4);
                logotr.UpdateValue += (s, e) => logoBl.Margin = new Thickness(logoBl.Margin.Left, e, logoBl.Margin.Right, logoBl.Margin.Bottom);
                logotr.Start();
                textGroup.Children.Clear();
                this.controlgrid.Children.Remove(_bgtextp);
                skippedIntro = true;
            }
        }
    }
}
