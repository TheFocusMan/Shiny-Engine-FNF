using Shiny_Engine_FNF.Code;
using Shiny_Engine_FNF.Code.FridayNightFunkinDataBase;
using Shiny_Engine_FNF.Code.Properties;
using Shiny_Engine_FNF.States.Editors;
using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfGame;
using WpfGame.AnimationsSheet;
using WpfGame.Controls;

namespace Shiny_Engine_FNF.Code.Controls
{
    public class Note : Sprite
    {
        public double strumTime = 0;

        public bool mustPress = false;
        public int noteData = 0;
        public bool canBeHit = false;
        public bool tooLate = false;
        public bool wasGoodHit = false;
        public bool ignoreNote = false;
        public bool hitByOpponent = false;
        public bool noteWasHit = false;
        public Note prevNote;

        public double sustainLength = 0;
        public bool isSustainNote = false;

        private string _noteType;

        public string NoteType
        {
            get => _noteType; set
            {
                noteSplashTexture = PlayState.SONG.splashSkin;
#if DO_SHADERS
                colorSwap.Color = DataFile.Data.arrowHSV[noteData % 4];
#endif
                if (noteData > -1 && _noteType != value)
                {
                    switch (value)
                    {
                        case "Hurt Note":
                            ignoreNote = mustPress;
                            ReloadNote("HURT");
                            noteSplashTexture = "HURTnoteSplashes";
#if DO_SHADERS
                            colorSwap.Color = Colors.Black;
#endif
                            if (isSustainNote)
                            {
                                missHealth = 0.1;
                            }
                            else
                            {
                                missHealth = 0.3;
                            }
                            hitCausesMiss = true;
                            break;
                        case "No Animation":
                            noAnimation = true;
                            break;
                        case "GF Sing":
                            gfNote = true;
                            break;
                    }
                    _noteType = value;
                }
#if DO_SHADERS
                noteSplashColor = colorSwap.Color;
#endif
            }
        }

        public string eventName = "";
        public int eventLength = 0;
        public string eventVal1 = "";
        public string eventVal2 = "";

        public ColorSwap colorSwap;
        public bool inEditor = false;
        public bool gfNote = false;
        private double earlyHitMult = 0.5;

        public const double swagWidth = 160 * 0.7;
        public const int PURP_NOTE = 0;
        public const int GREEN_NOTE = 2;
        public const int BLUE_NOTE = 1;
        public const int RED_NOTE = 3;

        // Lua shit
        public bool noteSplashDisabled = false;
        public string noteSplashTexture = null;
        public HsbColor noteSplashColor = Colors.Black;

        public double offsetX = 0;
        public double offsetY = 0;
        public double offsetAngle = 0;
        public double multAlpha = 1;

        public bool copyX = true;
        public bool copyY = true;
        public bool copyAngle = true;
        public bool copyAlpha = true;

        public double hitHealth = 0.023;
        public double missHealth = 0.0475;

        private string _texture;
        public string Texture
        {
            get => _texture;
            set
            {
                if (_texture != value)
                    ReloadNote("", value);
                _texture = value;
            }
        }

        public bool noAnimation = false;
        public bool hitCausesMiss = false;
        public double distance = 2000;//plan on doing scroll directions soon -bb

        public Note(double strumTime, int noteData, Note prevNote, bool sustainNote = false, bool inEditor = false) : base()
        {
            Width = 0;
            Height = 0;
            if (prevNote == null)
                prevNote = this;

            this.prevNote = prevNote;
            isSustainNote = sustainNote;
            this.inEditor = inEditor;

            X += (Settings.Default.MiddleScroll ? PlayState.STRUM_X_MIDDLESCROLL : PlayState.STRUM_X) + 50;
            // MAKE SURE ITS DEFINITELY OFF SCREEN?
            Y -= 2000;
            this.strumTime = strumTime;
            if (!inEditor) this.strumTime += Settings.Default.NoteOffset;

            this.noteData = noteData;

            if (noteData > -1)
            {
                Texture = "";
#if DO_SHADERS
                colorSwap = new ColorSwap();
                Effect = colorSwap;
#endif

                X += swagWidth * (noteData % 4);
                if (!isSustainNote)
                { //Doing this 'if' check to fix the warnings on Senpai songs
                    var animToPlay = "";
                    switch (noteData % 4)
                    {
                        case 0:
                            animToPlay = "purple";
                            break;
                        case 1:
                            animToPlay = "blue";
                            break;
                        case 2:
                            animToPlay = "green";
                            break;
                        case 3:
                            animToPlay = "red";
                            break;
                    }
                    Frames.PlayAnimation2(animToPlay + "Scroll", true);
                }
            }

            // trace(prevNote);
            if (PlayState.isPixelStage)
                this.UpdateHitbox(); // באגים זנותיים

            if (isSustainNote && prevNote != null)
            {
                Opacity = 0.6;
                multAlpha = 0.6;
                if (Settings.Default.DownScroll)
                    FlipY = true;

                offsetX += Width / 2;
                copyAngle = false;

                switch (noteData)
                {
                    case 0:
                        Frames.PlayAnimation2("purpleholdend", true);
                        break;
                    case 1:
                        Frames.PlayAnimation2("blueholdend", true);
                        break;
                    case 2:
                        Frames.PlayAnimation2("greenholdend", true);
                        break;
                    case 3:
                        Frames.PlayAnimation2("redholdend", true);
                        break;
                }
                offsetX -= Width / 2;

                this.UpdateHitbox();

                if (PlayState.isPixelStage)
                    offsetX += 30;

                if (prevNote.isSustainNote)
                {
                    switch (prevNote.noteData)
                    {
                        case 0:
                            prevNote.Frames.PlayAnimation2("purplehold", true);
                            break;
                        case 1:
                            prevNote.Frames.PlayAnimation2("bluehold", true);
                            break;
                        case 2:
                            prevNote.Frames.PlayAnimation2("greenhold", true);
                            break;
                        case 3:
                            prevNote.Frames.PlayAnimation2("redhold", true);
                            break;
                    }
                    prevNote.ScaleY *= Conductor.stepCrochet / 100 * 1.05;
                    if (PlayState.Instance != null)
                        prevNote.ScaleY *= PlayState.Instance.SongSpeed;

                    if (PlayState.isPixelStage)
                        prevNote.ScaleY *= 1.19;
                    //prevNote.UpdateHitbox();
                    // prevNote.setGraphicSize();
                }

                if (PlayState.isPixelStage)
                    ScaleY *= PlayState.daPixelZoom;
            }
            else if (!isSustainNote)
                earlyHitMult = 1;
            X += offsetX;
            //Unloaded += Note_Unloaded;
        }

        private void Note_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            prevNote = null;
        }

        void ReloadNote(string prefix = "", string texture = "", string suffix = "")
        {
            if (prefix == null) prefix = "";
            if (texture == null) texture = "";
            if (suffix == null) suffix = "";

            var skin = texture;
            if (texture.Length < 1)
            {
                skin = PlayState.SONG.arrowSkin;
                if (skin == null || skin.Length < 1)
                {
                    skin = "NOTE_assets";
                }
            }

            string animName = null;
            if (Frames?.CurrentAnimationName != null)
            {
                animName = Frames.CurrentAnimationName;
            }

            var arraySkin = skin.Split('\\');
            arraySkin[^1] = prefix + arraySkin[^1] + suffix;

            var lastScaleY = ScaleY;
            var blahblah = string.Join('\\', arraySkin);
            if (PlayState.isPixelStage)
            {
                if (isSustainNote)
                    TextureAnimationSheet.Create(Paths.Image("pixelUI\\" + blahblah + "ENDS"),2,4,this);
                else
                    TextureAnimationSheet.Create(Paths.Image("pixelUI\\" + blahblah),5,4,this);
                Scale = new(PlayState.daPixelZoom, PlayState.daPixelZoom);
                LoadPixelNoteAnims();
            }
            else
            {
                Frames = Paths.GetSparrowAtlas(blahblah, this);
                LoadNoteAnims();
            }
            if (isSustainNote)
                ScaleY = lastScaleY;
            if (animName != null)
                Frames.PlayAnimation2(animName, true);

            if (inEditor)
            {
                Width = Height = ChartingState.GRID_SIZE;
            }
        }

        void LoadNoteAnims()
        {
            Frames.AddByIndices("greenScroll", "green", new int[] { 0 });
            Frames.AddByIndices("redScroll", "red", new int[] { 0 });
            Frames.AddByIndices("blueScroll", "blue", new int[] { 0 });
            Frames.AddByIndices("purpleScroll", "purple", new int[] { 0 });

            if (isSustainNote)
            {
                Frames.AddByPrefix("purpleholdend", "pruple end hold");
                Frames.AddByPrefix("greenholdend", "green hold end");
                Frames.AddByPrefix("redholdend", "red hold end");
                Frames.AddByPrefix("blueholdend", "blue hold end");

                Frames.AddByPrefix("purplehold", "purple hold piece");
                Frames.AddByPrefix("greenhold", "green hold piece");
                Frames.AddByPrefix("redhold", "red hold piece");
                Frames.AddByPrefix("bluehold", "blue hold piece");
            }

            Scale = new System.Windows.Vector(0.7, 0.7);
        }

        void LoadPixelNoteAnims()
        {
            if (isSustainNote)
            {
                Frames.Add("purpleholdend", PURP_NOTE + 4);
                Frames.Add("greenholdend", GREEN_NOTE + 4);
                Frames.Add("redholdend", RED_NOTE + 4);
                Frames.Add("blueholdend", BLUE_NOTE + 4);

                Frames.Add("purplehold", PURP_NOTE);
                Frames.Add("greenhold", GREEN_NOTE);
                Frames.Add("redhold", RED_NOTE);
                Frames.Add("bluehold", BLUE_NOTE);
            }
            else
            {
                Frames.Add("greenScroll", GREEN_NOTE + 4);
                Frames.Add("redScroll", RED_NOTE + 4);
                Frames.Add("blueScroll", BLUE_NOTE + 4);
                Frames.Add("purpleScroll", PURP_NOTE + 4);
            }
        }

        protected override void Update()
        {
            base.Update();
            if (mustPress)
            {
                // ok river
                if (strumTime > Conductor.songPosition.TotalMilliseconds - Conductor.safeZoneOffset
                    && strumTime < Conductor.songPosition.TotalMilliseconds + Conductor.safeZoneOffset * earlyHitMult)
                    canBeHit = true;
                else
                    canBeHit = false;

                if (strumTime < Conductor.songPosition.TotalMilliseconds - Conductor.safeZoneOffset && !wasGoodHit)
                    tooLate = true;
            }
            else
            {
                canBeHit = false;

                if (strumTime < Conductor.songPosition.TotalMilliseconds + Conductor.safeZoneOffset * earlyHitMult)
                {
                    if (isSustainNote && prevNote.wasGoodHit || strumTime <= Conductor.songPosition.TotalMilliseconds)
                        wasGoodHit = true;
                }
            }

            if (tooLate && !inEditor)
            {
                if (Opacity > 0.3)
                    Opacity = 0.3;
            }
        }
    }
}
