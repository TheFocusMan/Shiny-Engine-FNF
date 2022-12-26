using Shiny_Engine_FNF.Code;
using Shiny_Engine_FNF.Code.FridayNightFunkinDataBase;
using System.Linq;
using System.Windows.Input;
using WpfGame;
using WpfGame.Controls;

namespace Shiny_Engine_FNF.Code
{
    /// <summary>
    /// Interaction logic for GitarooPause.xaml
    /// </summary>
    public partial class GitarooPause : MusicBeatState
    {
        bool replaySelect = false;
        public GitarooPause()
        {
            InitializeComponent();

            Sound.StopAll();

            bg.Source = Sprite.CreateGoodImage(Paths.Image("pauseAlt\\pauseBG"));
            bg.ResyncSize();

            bf.Frames = Paths.GetSparrowAtlas("pauseAlt\\bfLol", bf, null, 13);
            bf.Frames.AddByPrefix("lol", "funnyThing");
            bf.Frames.PlayAnimation2("lol");
            bf.ScreenCenter();
            bf.Y = 30;

            replayButton.Position = new(Display.DefaultWidth * 0.28, Display.DefaultHeight * 0.7);
            replayButton.Frames = Paths.GetSparrowAtlas("pauseAlt\\pauseUI", replayButton,null,0);
            replayButton.Frames.AppendByPrefix("selected", "bluereplay", "yellowreplay");
            replayButton.Frames.PlayAnimation2("selected",false,false);

            cancelButton.Position = new(Display.DefaultWidth  *0.58, replayButton.Y);
            cancelButton.Frames = Paths.GetSparrowAtlas("pauseAlt\\pauseUI", cancelButton, null, 0);
            cancelButton.Frames.AppendByPrefix("selected", "cancelyellow", "bluecancel");
            cancelButton.Frames.PlayAnimation2("selected", false, false);
        }

        private void replayButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            replaySelect = true;
            ChangeThing();
        }

        private void cancelButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            replaySelect = false;
            ChangeThing();
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (PlayerSttings.UiKeys[0].Contains(e.Key) || PlayerSttings.UiKeys[3].Contains(e.Key))
            {
                replaySelect = !replaySelect;
                ChangeThing();
            }
            if (PlayerSttings.Accept.Contains(e.Key))
            {
                if (replaySelect)
                {
                    SwichState(new PlayState());
                }
                else
                {
                    //PlayState.usedPractice = false;
                    PlayState.changedDifficulty = false;
                    PlayState.seenCutscene = false;
                    PlayState.deathCounter = 0;
                    //PlayState.cpuControlled = false;
                    SwichState(new MainMenuState());
                    Sound.Play(Paths.Music("freakyMenu"));
                }
            }
            base.OnKeyUp(e);
        }

        void ChangeThing()
        {
            if (replaySelect)
            {
                cancelButton.Frames.CurrentFrame = 0;
                replayButton.Frames.CurrentFrame = 1;
            }
            else
            {
                cancelButton.Frames.CurrentFrame = 1;
                replayButton.Frames.CurrentFrame = 0;
            }
        }
    }
}
