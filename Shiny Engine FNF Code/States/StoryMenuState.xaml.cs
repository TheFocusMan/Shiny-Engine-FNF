using Shiny_Engine_FNF.Code;
using Shiny_Engine_FNF.Code.Controls;
using Shiny_Engine_FNF.Code.FridayNightFunkinDataBase;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WpfGame;
using WpfGame.AnimationsSheet;
using WpfGame.Controls;

namespace Shiny_Engine_FNF.Code
{
    /// <summary>
    /// Interaction logic for StoryMenuState.xaml
    /// </summary>
    public partial class StoryMenuState : MusicBeatState
    {
        // Wether you have to beat the previous week for playing this one
        // Not recommended, as people usually download your mod for, you know,
        // playing just the modded week then delete it.
        // defaults to True
        public static Dictionary<string,bool> weekCompleted = new();
        int lerpScore = 0;
        int intendedScore = 0;
        //readonly string[] weekNames = File.ReadAllLines(Paths.Txt("data\\weekNames"));
        bool movedBack = false;
        bool selectedWeek = false;

        public StoryMenuState()
        {
            InitializeComponent();
            WeekData.reloadWeekFiles(true);
            weekCompleted = DataFile.Data.weekCompleted;
            if (weekCompleted == null) weekCompleted = new();
            // Updating Discord Rich Presence
            DiscordClient.ChangePresence("In the Story Mode Menu", null);

            for (var i = 0; i < WeekData.weeksLoaded.Count; i++)
            {
                MenuItem weekThing = new(WeekData.weeksList[i])
                {
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                weekThing.Margin = new Thickness(0, (weekThing.Height + 20) * i + 30, 0, 0);
                grpWeekText.Children.Add(weekThing);
                // weekThing.updateHitbox();

                // Needs an offset thingie
                if (weekIsLocked(i))
                {
                    Debug.WriteLine("locking week " + i);
                    Sprite lockcontrol = new() { Margin = new Thickness(weekThing.Margin.Left + ((FrameworkElement)weekThing.Children[0]).Width + 80, 0, 0, 0) };
                    var ui_tex = Paths.GetSparrowAtlas("campaign_menu_UI_assets", lockcontrol);
                    lockcontrol.Frames = ui_tex;
                    ui_tex.PlayAnimation("lock");
                    //lockcontrol.frames = ui_tex;
                    //lockcontrol.animation.addByPrefix("lock", "lock");
                    //lockcontrol.animation.play("lock");
                    // lockcontrol.ID = i;
                    weekThing.Children.Add(lockcontrol);
                }
            }

            //difficultySelectors = new FlxGroup();
            //add(difficultySelectors);
            leftArrow.Tag = Paths.GetSparrowAtlas("campaign_menu_UI_assets", leftArrow);
            (leftArrow.Tag as Sparrow2AnimationSheet).AnimationChase.Add("idle", "arrow left");
            (leftArrow.Tag as Sparrow2AnimationSheet).AnimationChase.Add("press", "arrow push left");
            (leftArrow.Tag as Sparrow2AnimationSheet).PlayAnimation((leftArrow.Tag as Sparrow2AnimationSheet).AnimationChase["idle"], false, false);
            ChangeDifficulty();

            rightArrow.Tag = Paths.GetSparrowAtlas("campaign_menu_UI_assets", rightArrow);
            (rightArrow.Tag as Sparrow2AnimationSheet).AnimationChase.Add("idle", "arrow right");
            (rightArrow.Tag as Sparrow2AnimationSheet).AnimationChase.Add("press", "arrow push right");
            (rightArrow.Tag as Sparrow2AnimationSheet).PlayAnimation((rightArrow.Tag as Sparrow2AnimationSheet).AnimationChase["idle"],false,false);
            // add(rankText);
            ChangeWeek();

            UpdateText();
        }
        int curWeek = 0;
        int curDifficulty = 1;

        void UpdateText()
        {
            foreach (MenuCharacter child in yellowBG.Children)
            {
                var i = yellowBG.Children.IndexOf(child);
                child.X = (Width * 0.25) * (1 + i) - 150;
                child.Y = 14;
            }
            WeekData.setDirectoryFromWeek(WeekData.weeksLoaded[WeekData.weeksList[curWeek]]);
            var weekc = WeekData.weeksLoaded[WeekData.weeksList[curWeek]];

            charchter1.ChangeCharacter(weekc.weekCharacters[0]);
            charchter2.ChangeCharacter(weekc.weekCharacters[1]);
            charchter3.ChangeCharacter(weekc.weekCharacters[2]);

            txtTracklist.Text = "Tracks\n";
            foreach (var i in weekc.songs.Select(x => x[0]))
                txtTracklist.Text += "\n" + i;

            txtTracklist.Text = txtTracklist.Text.ToUpper();

            //  txtTracklist.screenCenter(X);
            // txtTracklist.x -= FlxG.width * 0.35;

            txtTracklist.Text += "\n";
            intendedScore = Highscore.GetWeekScore(WeekData.weeksList[curWeek], curDifficulty);

        }

        private void ChangeDifficulty(int change = 0)
        {
            curDifficulty += change;

            if (curDifficulty < 0)
                curDifficulty = 2;
            if (curDifficulty > 2)
                curDifficulty = 0;

            //sprDifficulty.offset.x = 0;
            var tween = new Tween(-300, 0, new QuadraticEase(), 0.2); // אפקט
            tween.UpdateValue += (s, e) => sprDifficulty.Y = e;
            tween.Start();
            sprDifficulty.Source = Sprite.CreateGoodImage(Paths.Image($"menudifficulties\\{CoolUtil.DifficultyFromInt(curDifficulty).ToLower()}"));
            // USING THESE WEIRD VALUES SO THAT IT DOESNT FLOAT UP
            //sprDifficulty.Margin = new Thickness(0, leftArrow.Margin.Top - 15, 0, 0);

            intendedScore = Highscore.GetWeekScore(WeekData.weeksList[curWeek], curDifficulty);
        }
        protected override void BeatHit()
        {
            base.BeatHit();
        }

        private static bool IsMouseDown(FrameworkElement element)
        {
            var point = Mouse.GetPosition(element);
            return Mouse.LeftButton == MouseButtonState.Pressed && point.X >= 0 &&
                point.Y >= 0 && point.X <= element.ActualWidth && point.Y <= element.ActualHeight;
        }

        protected override void Update()
        {
            base.Update();
            lerpScore = (int)Math.Floor(Mathf.Lerp(lerpScore, intendedScore, Mathf.Clamp(StaticTimer.DeltaSaconds * 30, 0, 1)));
            if (Math.Abs(intendedScore - lerpScore) < 10) lerpScore = intendedScore;
            scoreText.Text = "WEEK SCORE:" + lerpScore;
            foreach (MenuCharacter c in yellowBG.Children)
                c.BopHead();

            if (Keyboard.IsKeyDown(Key.Right) || Keyboard.IsKeyDown(Key.Left) || Mouse.LeftButton == MouseButtonState.Pressed)
            {
                var arrow = Keyboard.IsKeyDown(Key.Right) || IsMouseDown(rightArrow) ? rightArrow :
                    Keyboard.IsKeyDown(Key.Left) || IsMouseDown(leftArrow) ? leftArrow : null;
                arrow?.Frames.PlayAnimation2("press", false, false);
            }
            else
            {
                leftArrow.Frames.PlayAnimation2("idle",false,false);
                rightArrow.Frames.PlayAnimation2("idle", false, false);
            }

            if (Sound.PlayersCount == 0 && !selectedWeek)
            {
                Sound.Play(Paths.Music("freakyMenu"));
                Conductor.ChangeBPM(102);
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (!movedBack)
            {
                if (!selectedWeek)
                {
                    if (e.Key == Key.PageUp || e.Key == Key.Up) ChangeWeek(-1);
                    if (e.Key == Key.PageDown || e.Key == Key.Down) ChangeWeek(1);
                    if (e.Key == Key.Right || e.Key == Key.Left) ChangeDifficulty(e.Key == Key.Right ? 1 : e.Key == Key.Left ? -1 : 0); // לאפקטים
                }

                if (e.Key == Key.Enter)SelectWeek();
            }

            if (e.Key == Key.Back && !movedBack && !selectedWeek)
            {
                Sound.Play(Paths.Sound("cancelMenu"));
                movedBack = true;
                SwichState(new MainMenuState());
            }
            if (Sound.PlayersCount != 0)
                Conductor.songPosition = Sound.GetSoundStrongCache()[0].Position;

            base.OnKeyUp(e);
        }
        bool weekIsLocked(int weekNum)
        {
            var leWeek = WeekData.weeksLoaded[WeekData.weeksList[weekNum]];
            var ret = (!weekCompleted.ContainsKey(leWeek.weekBefore) || !weekCompleted[leWeek.weekBefore]);
            return (!leWeek.startUnlocked && leWeek.weekBefore.Length > 0 && ret);
        }


        bool stopspamming = false;
        private void SelectWeek()
        {
            if (!weekIsLocked(curWeek))
            {
                if (stopspamming == false)
                {
                    Sound.Play(Paths.Sound("confirmMenu"));

                    // grpWeekText.members[curWeek].startFlashing();
                    Flicker.StartFlicker(new FrameworkElement[] { grpWeekText.Children[curWeek] as FrameworkElement },1,0.05);
                    charchter2.Frames.PlayAnimation2("confirm", true,false);
                    stopspamming = true;

                    // Nevermind that's stupid lmao
                    PlayState.storyPlaylist = WeekData.weeksLoaded[WeekData.weeksList[curWeek]].songs.Select(x => (string)x[0]).ToList();
                    PlayState.isStoryMode = true;
                    selectedWeek = true;

                    var diffic = CoolUtil.GetDifficultyFilePath(curDifficulty);
                    if (diffic == null) diffic = "";

                    PlayState.storyDifficulty = curDifficulty;

                    PlayState.SONG = FNFSong.LoadFromJson(PlayState.storyPlaylist[0].ToLower() + diffic, PlayState.storyPlaylist[0].ToLower());
                    PlayState.campaignScore = 0;
                    PlayState.campaignMisses = 0;
                    Task.Factory.StartNew(() =>
                    {
                        Task.Delay(1000).Wait();
                        Dispatcher.BeginInvoke(() => SwichState(new PlayState()));
                    });
                }
                //LoadingState.loadAndSwitchState(new PlayState(), true);
                //FreeplayState.destroyFreeplayVocals();
            }
            else Sound.Play(Paths.Sound("cancelMenu"));
        }
        private void ChangeWeek(int change = 0)
        {
            curWeek += change;

            if (curWeek >= WeekData.weeksList.Count)
                curWeek = 0;
            if (curWeek < 0)
                curWeek = WeekData.weeksList.Count - 1;

            var leWeek = WeekData.weeksLoaded[WeekData.weeksList[curWeek]];
            WeekData.setDirectoryFromWeek(leWeek);

            foreach (MenuItem item in grpWeekText.Children)
            {
                //item.targetY = bullShit - curWeek;
                if (!weekIsLocked(grpWeekText.Children.IndexOf(item)))
                    item.Children[0].Opacity = 1;
                else item.Children[0].Opacity = 0.6;
                item.Dispatcher.BeginInvoke(new Action(() => { }), DispatcherPriority.Render);
            }
            Tween tween = new(grpWeekText.Margin.Top, -((FrameworkElement)grpWeekText.Children[curWeek]).Margin.Top, null, 0.24);
            tween.UpdateValue += (sender, e) => grpWeekText.Margin = new Thickness(0, e, 0, 0);
            tween.Start();

            txtWeekTitle.Text = leWeek.storyName;
            difficultySelectors.Visibility = !weekIsLocked(curWeek) ? Visibility.Visible : Visibility.Hidden;

            Sound.Play(Paths.Sound("scrollMenu"));

            UpdateText();
        }

        private void MusicBeatState_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ChangeWeek(e.Delta > 0 ? -1 : 1);
        }

        private void LeftArrow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ChangeDifficulty(-1);
        }

        private void RightArrow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ChangeDifficulty(1);
        }
    }
}
