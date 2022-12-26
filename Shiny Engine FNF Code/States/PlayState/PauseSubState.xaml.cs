using Shiny_Engine_FNF.Code.Controls;
using Shiny_Engine_FNF.Code.FridayNightFunkinDataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using WpfGame;

namespace Shiny_Engine_FNF.Code
{
    /// <summary>
    /// Interaction logic for PauseSubState.xaml
    /// </summary>
    public partial class PauseSubState : MusicBeatSubstate
    {
        string[] menuItems;
        string[] menuItemsOG = new string[] { "Resume", "Restart Song", "Toggle Botplay", "Memorty Bomb Lags AHHH", "Change Difficulty", "Exit to menu" };
        List<string> difficultyChoices = new();
        int curSelected = 0;

        FlxSound pauseMusic;

        private PauseSubState()
        {
            InitializeComponent();
            Loaded += PauseSubState_Loaded;
            menuItems = menuItemsOG;

            levelInfo.Text = PlayState.SONG.song;
            levelDifficulty.Text = CoolUtil.DifficultyFromInt(PlayState.storyDifficulty);
            blueballedTxt.Text = "Blueballed: " + PlayState.deathCounter;
            for (int i = 0; i < CoolUtil.defaultDifficulties.Length; i++)
            {
                var diff = "" + CoolUtil.defaultDifficulties[i];
                difficultyChoices.Add(diff);
            }
            difficultyChoices.Add("BACK");

            for (int i = 0; i < menuItems.Length; i++)
            {
                Alphabet songText = new(new(0, (70 * i) + 30), menuItems[i], true, false)
                {
                    isMenuItem = true,
                    targetY = i
                };
                songText.MouseLeftButtonUp += (s, e) =>
                {
                    if (grpMenuShit.Children.IndexOf(songText) == curSelected)
                        OnKeyUp(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromDependencyObject(this), 0, PlayerSttings.Accept[0]));
                    else ChangeSelection(grpMenuShit.Children.IndexOf(songText) - curSelected);
                };
                grpMenuShit.Children.Add(songText);
            }
            pauseMusic = Sound.LoadOnly(Paths.Music("breakfast"));
            ChangeSelection();
            Unloaded += PauseSubState_Unloaded;
        }

        private void PauseSubState_Unloaded(object sender, RoutedEventArgs e)
        {
            pauseMusic.Dispose();
        }

        private void PauseSubState_Loaded(object sender, RoutedEventArgs e)
        {
            InvalidateVisual();
            BeginStoryboard(Resources["Storyboard1"] as Storyboard);
            practiceText.Visibility = PlayState.Instance.practiceMode ? Visibility.Visible : Visibility.Hidden;
            chartingText.Visibility = PlayState.chartingMode ? Visibility.Visible : Visibility.Hidden;
        }

        public PauseSubState(double x, double y) : this()
        {

        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                ChangeSelection(-1);
            else ChangeSelection(1);
            base.OnMouseWheel(e);
        }

        protected override void Update()
        {
            if (pauseMusic.Volume < 0.5)
                pauseMusic.Volume += 0.01f * (float)StaticTimer.DeltaSaconds;
            if (pauseMusic.PlaybackState == NAudio.Wave.PlaybackState.Stopped && !pauseMusic.IsDisposed)
            {
                pauseMusic.Position = TimeSpan.Zero;
                pauseMusic.Play();
            }
            base.Update();
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (PlayerSttings.Accept.Contains(e.Key))
            {
                var daSelected = menuItems[curSelected];
                if (daSelected != "BACK" && difficultyChoices.Contains(daSelected))
                {
                    var name = PlayState.SONG.song.ToLower();
                    var poop = Highscore.FormatSong(name, curSelected);
                    PlayState.SONG = FNFSong.LoadFromJson(poop, name);
                    PlayState.storyDifficulty = curSelected;
                    MusicBeatState.ResetState(CurrentState.NavigationService);
                    PlayState.changedDifficulty = true;
                    PlayState.chartingMode = false;
                    return;
                }

                switch (daSelected)
                {
                    case "Resume":
                        Close();
                        break;
                    case "Change Difficulty":
                        menuItems = difficultyChoices.ToArray();
                        RegenMenu();
                        break;
                    case "Toggle Practice Mode":
                        PlayState.Instance.practiceMode = !PlayState.Instance.practiceMode;
                        PlayState.changedDifficulty = true;
                        practiceText.Visibility = PlayState.Instance.practiceMode ? Visibility.Visible : Visibility.Hidden;
                        break;
                    case "Restart Song":
                        RestartSong(CurrentState);
                        break;
                    case "Toggle Botplay":
                        PlayState.Instance.cpuControlled = !PlayState.Instance.cpuControlled;
                        PlayState.changedDifficulty = true;
                        PlayState.Instance.botplayTxt.Visibility = PlayState.Instance.cpuControlled ? Visibility.Visible : Visibility.Hidden;
                        PlayState.Instance.botplayTxt.Opacity = 1;
                        PlayState.Instance.botplaySine = 0;
                        break;
                    case "Exit to menu":
                        PlayState.deathCounter = 0;
                        PlayState.seenCutscene = false;
                        foreach (Note note in PlayState.Instance.unspawnNotes)
                            CacheKiller.DestroyObject(note.Effect);
                        MusicBeatState.SwitchToNullState(CurrentState.NavigationService);
                        Paths.DisposeAll();
                        if (PlayState.isStoryMode)
                        {
                            MusicBeatState.SwichState(CurrentState.NavigationService, CurrentState.Content as Panel, new StoryMenuState());
                        }
                        else
                        {
                            MusicBeatState.SwichState(CurrentState.NavigationService, CurrentState.Content as Panel, new FreeplayState());
                        }
                        Sound.Play(Paths.Music("freakyMenu"));
                        PlayState.changedDifficulty = false;
                        PlayState.chartingMode = false;
                        break;
                    case "BACK":
                        menuItems = menuItemsOG;
                        RegenMenu();
                        break;
                    case "Memorty Bomb Lags AHHH":
                        Paths.RemoveUnusedMemory();
                        GC.WaitForPendingFinalizers();
                        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                        break;
                }
            }
            if (PlayerSttings.UiKeys[1].Contains(e.Key))
            {
                ChangeSelection(1);
            }
            if (PlayerSttings.UiKeys[2].Contains(e.Key))
            {
                ChangeSelection(-1);
            }
            base.OnKeyUp(e);
        }

        public static void RestartSong(State state, bool noTrans = false)
        {
            PlayState.Instance.paused = true; // For lua
            PlayState.Instance.instumental.Volume = 0;
            if (PlayState.Instance.vocals != null)
                PlayState.Instance.vocals.Volume = 0;

            StageData.loadDirectory(PlayState.SONG);//תיקון באג
            MusicBeatState.SwichState(state.NavigationService, state.Content as Panel, Activator.CreateInstance(state.GetType()) as State, !noTrans);
        }

        void ChangeSelection(int change = 0)
        {
            curSelected += change;

            Sound.Play(Paths.Sound("scrollMenu"));

            if (curSelected < 0)
                curSelected = menuItems.Length - 1;
            if (curSelected >= menuItems.Length)
                curSelected = 0;

            var bullShit = 0;

            foreach (Alphabet item in grpMenuShit.Children)
            {
                item.targetY = bullShit - curSelected;
                bullShit++;

                item.Opacity = 0.6;
                // item.setGraphicSize(Std.int(item.width * 0.8));

                if (item.targetY == 0)
                {
                    item.Opacity = 1;
                    // item.setGraphicSize(Std.int(item.width));
                }
            }
        }

        void RegenMenu()
        {
            this.grpMenuShit.Children.Clear();
            for (int i = 0; i < menuItems.Length; i++)
            {
                var item = new Alphabet(new Point(0, 70 * i + 30), menuItems[i], true, false);
                item.isMenuItem = true;
                item.targetY = i;
                grpMenuShit.Children.Add(item);
            }
            curSelected = 0;
            ChangeSelection();
        }
    }
}
