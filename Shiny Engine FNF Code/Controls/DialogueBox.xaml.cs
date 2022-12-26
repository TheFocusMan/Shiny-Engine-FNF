using Shiny_Engine_FNF.Code;
using Shiny_Engine_FNF.Code.Controls;
using Shiny_Engine_FNF.Code.FridayNightFunkinDataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using WpfGame;
using WpfGame.Controls;

namespace Shiny_Engine_FNF
{
    /// <summary>
    /// Interaction logic for DialogueBox.xaml
    /// </summary>
    public partial class DialogueBox : UserControl, IUpdatable
    {
        Alphabet dialogue;
        List<string> dialogueList = new();
        string curCharacter = "";

        public event EventHandler NextDialogueThing;
        public event EventHandler SkipDialogueThing;
        public event EventHandler FinishThing;

        public DialogueBox() : this(false, Array.Empty<string>())
        {

        }
        public DialogueBox(bool talkingRight, string[] dialogueList)
        {
            InitializeComponent();
            switch (PlayState.SONG.song.ToLower())
            {
                case "senpai":
                    Sound.Play(Paths.Music("Lunchbox"));
                    break;
                case "thorns":
                    Sound.Play(Paths.Music("LunchboxScary"));
                    break;
            }

            Dispatcher.BeginInvoke(async delegate ()
            {
                for (int i = 0; i < 5; i++)
                {
                    bgFade.Opacity += (1 / 5) * 0.7;
                    if (bgFade.Opacity > 0.7)
                        bgFade.Opacity = 0.7;
                    await Task.Delay(830);
                }
            });

            bool hasDialog = false;
            switch (PlayState.SONG.song.ToLower())
            {
                case "senpai":
                    hasDialog = true;
                    box.Frames = Paths.GetSparrowAtlas("weeb\\pixelUI\\dialogueBox-pixel", box);
                    box.Frames.Framerate = 24;
                    box.Frames.AddByPrefix("normalOpen", "Text Box Appear instance 1");
                    box.Frames.AddByIndices("normal", "Text Box Appear instance 1", new int[] { 4 });
                    break;
                case "roses":
                    hasDialog = true;
                    Sound.Play(Paths.Sound("ANGRY_TEXT_BOX"));

                    box.Frames = Paths.GetSparrowAtlas("weeb\\pixelUI\\dialogueBox-senpaiMad", box);
                    box.Frames.Framerate = 24;
                    box.Frames.AddByPrefix("normalOpen", "SENPAI ANGRY IMPACT SPEECH instance 1");
                    box.Frames.AddByIndices("normal", "SENPAI ANGRY IMPACT SPEECH instance 1", new int[] { 4 });
                    break;
                case "thorns":
                    hasDialog = true;
                    box.Frames = Paths.GetSparrowAtlas("weeb\\pixelUI\\dialogueBox-evil", box);
                    box.Frames.Framerate = 24;
                    box.Frames.AddByPrefix("normalOpen", "Spirit Textbox spawn instance 1");
                    box.Frames.AddByIndices("normal", "Spirit Textbox spawn instance 1", new int[] { 11 });

                    var face = new Sprite() { X = 320, Y = 170, Source = Sprite.CreateGoodImage(Paths.Image("weeb\\spiritFaceForward")) };
                    face.ResyncSize();
                    face.SetZoom(6);
                    panel.Children.Add(face);
                    break;
            }

            this.dialogueList = dialogueList.ToList();

            if (!hasDialog)
                return;

            portraitLeft.Frames = Paths.GetSparrowAtlas("weeb\\senpaiPortrait", portraitLeft, null, 24);
            portraitLeft.Frames.AddByPrefix("enter", "Senpai Portrait Enter instance 1");
            portraitLeft.SetZoom(PlayState.daPixelZoom * 0.9);
            portraitLeft.Visibility = System.Windows.Visibility.Hidden;
            portraitLeft.UpdateHitbox();
            portraitRight.Frames = Paths.GetSparrowAtlas("weeb\\bfPortrait", portraitRight, null, 24);
            portraitRight.Frames.AddByPrefix("enter", "Boyfriend portrait enter instance 1");
            portraitRight.SetZoom(PlayState.daPixelZoom * 0.9);
            portraitRight.Visibility = System.Windows.Visibility.Hidden;
            portraitRight.UpdateHitbox();

            box.Frames.PlayAnimation2("normalOpen", false, false);
            box.SetZoom(PlayState.daPixelZoom * 0.9);
            box.UpdateHitbox();

            handSelect.Source = Sprite.CreateGoodImage(Paths.Image("weeb\\pixelUI\\hand_textbox"));
            handSelect.ResyncSize();
            handSelect.SetZoom(PlayState.daPixelZoom * 0.9);
            handSelect.Visibility = Visibility.Hidden;

            if (!talkingRight)
            {
                // box.flipX = true;
            }

            dropText.Width = Display.DefaultWidth * 0.6;

            swagDialogue.Width = Display.DefaultWidth * 0.6;
            swagDialogue.SourceSound = new(Paths.Sound("pixelText"));

            dialogue = new Alphabet(new Point(0, 80), "", false, true);
            // dialogue.x = 90;
            // add(dialogue);
        }

        bool dialogueOpened = false;
        bool dialogueStarted = false;
        bool dialogueEnded = false;

        bool isEnding = false;

        void IUpdatable.Update()
        {
            Update();
        }

        private void Update()
        {
            // HARD CODING CUZ IM STUPDI
            if (PlayState.SONG.song.ToLower() == "roses")
                portraitLeft.Visibility = Visibility.Hidden;
            if (PlayState.SONG.song.ToLower() == "thorns")
            {
                portraitLeft.Visibility = Visibility.Hidden;
                swagDialogue.Foreground = Brushes.White;
                dropText.Foreground = Brushes.Black;
            }

            dropText.Text = swagDialogue.Text;

            if (box.Frames.CurrentFriendlyAnimationName != null)
            {
                if (box.Frames.CurrentFriendlyAnimationName == "normalOpen" && box.Frames.IsPlayingAmimation)
                {
                    box.Frames.PlayAnimation2("normal", false, false);
                    dialogueOpened = true;
                }
            }

            if (dialogueOpened && !dialogueStarted)
            {
                StartDialogue();
                dialogueStarted = true;
            }

            if (PlayerSttings.KeyAcceptPressed())
            {
                if (dialogueEnded)
                {
                    panel.Children.Remove(dialogue);
                    if (dialogueList.Count == 1)
                    {
                        if (!isEnding)
                        {
                            isEnding = true;
                            Sound.Play(Paths.Sound("clickText"));

                            Sound.StopAll();
                            Dispatcher.BeginInvoke(async delegate ()
                            {
                                for (int i = 0; i < 5; i++)
                                {
                                    await Task.Delay(200);
                                    box.Opacity -= 1 / 5;
                                    bgFade.Opacity -= 1 / 5 * 0.7;
                                    portraitLeft.Visibility = Visibility.Hidden;
                                    portraitRight.Visibility = Visibility.Hidden;
                                    swagDialogue.Opacity -= 1 / 5;
                                    handSelect.Opacity -= 1 / 5;
                                    dropText.Opacity = swagDialogue.Opacity;
                                }
                            });

                            Dispatcher.BeginInvoke(async delegate ()
                            {
                                await Task.Delay(1500);
                                FinishThing?.Invoke(this, EventArgs.Empty);
                                (Parent as Panel).Children.Remove(this);
                            }, DispatcherPriority.Render);
                        }
                    }
                    else
                    {
                        dialogueList.Remove(dialogueList[0]);
                        StartDialogue();
                        Sound.Play(Paths.Sound("clickText"));
                    }
                }
                else if (dialogueStarted)
                {
                    Sound.Play(Paths.Sound("clickText"));
                    swagDialogue.Skip();
                    SkipDialogueThing?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        void StartDialogue()
        {
            CleanDialog();
            // var theDialog:Alphabet = new Alphabet(0, 70, dialogueList[0], false, true);
            // dialogue = theDialog;
            // add(theDialog);

            // swagDialogue.text = ;
            swagDialogue.ResetText(dialogueList[0]);
            swagDialogue.Start(0.04, true);
            swagDialogue.TypeComplete += (s, e) =>
            {
                handSelect.Visibility = Visibility.Visible;
                dialogueEnded = true;
            };

            handSelect.Visibility = Visibility.Hidden;
            dialogueEnded = false;
            switch (curCharacter)
            {
                case "dad":
                    portraitRight.Visibility = Visibility.Hidden;
                    if (!portraitLeft.IsVisible)
                    {
                        if (PlayState.SONG.song.ToLower() == "senpai")
                            portraitLeft.Visibility = Visibility.Visible;
                        portraitLeft.Frames.PlayAnimation2("enter", false, false);
                    }
                    break;
                case "bf":
                    portraitLeft.Visibility = Visibility.Hidden;
                    if (!portraitRight.IsVisible)
                    {
                        portraitRight.Visibility = Visibility.Visible;
                        portraitRight.Frames.PlayAnimation2("enter", false, false);
                    }
                    break;
            }
            NextDialogueThing?.Invoke(this, EventArgs.Empty);
        }

        void CleanDialog()
        {
            var splitName = dialogueList[0].Split(":");
            curCharacter = splitName[1];
            dialogueList[0] = dialogueList[0][(splitName[1].Length + 2)..].Trim();
        }
    }
}