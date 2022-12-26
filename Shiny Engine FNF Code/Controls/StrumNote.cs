using Shiny_Engine_FNF.Code;
using Shiny_Engine_FNF.Code.FridayNightFunkinDataBase;
using System;
using System.Windows;
using WpfGame.Controls;
using WpfGame.AnimationsSheet;
using WpfGame;

namespace Shiny_Engine_FNF.Code.Controls
{
    public class StrumNote : Sprite
    {
        private ColorSwap colorSwap;
        public double resetAnim = 0;
        private int noteData = 0;
        public double direction = 90;//plan on doing scroll directions soon -bb
        public bool downScroll = false;//plan on doing scroll directions soon -bb
        public bool sustainReduce = true;

        private int player;
        private string _texture;
        public string Texture
        {
            get => _texture;
            set
            {
                if (_texture != value)
                {
                    _texture = value;
                    ReloadNote();
                }
            }
        }

        public StrumNote(double x, double y, int leData, int player)
        {
#if DO_SHADERS
            colorSwap = new ColorSwap();
            Effect = colorSwap;
#endif
            X = x;
            Y = y;
            noteData = leData;
            this.player = player;
            noteData = leData;

            var skin = "NOTE_assets";
            if (PlayState.SONG.arrowSkin != null && PlayState.SONG.arrowSkin.Length > 1) skin = PlayState.SONG.arrowSkin;
            Texture = skin; //Load texture and anims

        }

        public void ReloadNote()
        {
            string lastAnim = null;
            if (Frames?.CurrentAnimationName != null) lastAnim = Frames?.CurrentAnimationName;

            if (PlayState.isPixelStage)
            {
                TextureAnimationSheet.Create(Paths.Image("pixelUI\\" + Texture),5,4,this);
                //ResyncSize();
                this.SetZoom(PlayState.daPixelZoom);

                Frames.Framerate = 24;
                Frames.Add("green", 6);
                Frames.Add("red", 7);
                Frames.Add("blue", 5);
                Frames.Add("purple", 4);
                switch (Math.Abs(noteData))
                {
                    case 0:
                        Frames.Add("static", 0);
                        Frames.Add("pressed", 4, 8);
                        Frames.Add("confirm", 12, 16);
                        break;
                    case 1:
                        Frames.Add("static", 1);
                        Frames.Add("pressed", 5, 9);
                        Frames.Add("confirm", 13, 17);
                        break;
                    case 2:
                        Frames.Add("static", 2);
                        Frames.Add("pressed", 6, 10);
                        Frames.Add("confirm", 14, 18);
                        break;
                    case 3:
                        Frames.Add("static", 3);
                        Frames.Add("pressed", 7, 11);
                        Frames.Add("confirm", 15, 19);
                        break;
                }

                this.UpdateHitbox(); // באגים זנותיים
            }
            else
            {
                Frames = Paths.GetSparrowAtlas(Texture, this, null, 24);
                Frames.AddByPrefix("green", "arrowUP");
                Frames.AddByPrefix("blue", "arrowDOWN");
                Frames.AddByPrefix("purple", "arrowLEFT");
                Frames.AddByPrefix("red", "arrowRIGHT");
                Scale = new Vector(0.7, 0.7);
                //this.setGraphicSize(Width * 0.7, Height);

                switch (Math.Abs(noteData))
                {
                    case 0:
                        Frames.AddByPrefix("static", "arrowLEFT");
                        Frames.AddByPrefix("pressed", "left press");
                        Frames.AddByPrefix("confirm", "left confirm");
                        break;
                    case 1:
                        Frames.AddByPrefix("static", "arrowDOWN");
                        Frames.AddByPrefix("pressed", "down press");
                        Frames.AddByPrefix("confirm", "down confirm");
                        break;
                    case 2:
                        Frames.AddByPrefix("static", "arrowUP");
                        Frames.AddByPrefix("pressed", "up press");
                        Frames.AddByPrefix("confirm", "up confirm");
                        break;
                    case 3:
                        Frames.AddByPrefix("static", "arrowRIGHT");
                        Frames.AddByPrefix("pressed", "right press");
                        Frames.AddByPrefix("confirm", "right confirm");
                        break;
                }
            }
       

            if (lastAnim != null)
            {
                PlayAnim(lastAnim, true);
            }
        }

        public void postAddedToGroup()
        {
            PlayAnim("static", true);
            X += Note.swagWidth * noteData;
            X += 50;
            X += Display.DefaultWidth / 2 * player;
        }

        protected override void Update()
        {
            if (resetAnim > 0)
            {
                resetAnim -= StaticTimer.DeltaSaconds;
                if (resetAnim <= 0)
                {
                    PlayAnim("static", true);
                    resetAnim = 0;
                }
            }
            base.Update();
            RianunNekudot();
        }

        protected override void OnFrameChangingPosition()
        {
            RianunNekudot();
        }

        private void RianunNekudot()
        {
            if (Frames != null)
            {
                if (Frames.CurrentFriendlyAnimationName == "confirm" && !PlayState.isPixelStage)
                {
                    var framename = Frames.CurrentAnimationName;
                    if (framename != null)
                    {
                        if (FrameSize == new Size()) FrameSize = AvgFrameSize();
                        if (FrameSize.Width > 0 && FrameSize.Height > 0)
                            CenterOrigin(); // negative frame point
                        //this.UpdateHitbox();
                    }
                }
                else
                {
                    Origin = new();
                }
            }
        }


        public void PlayAnim(string anim, bool force = false)
        {
            Origin = new();
            Frames.PlayAnimation2(anim, force, false);
            //CenterOffsets();
            if (anim != "confirm") CenterOrigin();
            if (Frames.CurrentAnimationName == null || Frames.CurrentFriendlyAnimationName == "static")
            {
#if DO_SHADERS
                colorSwap.Hue = 0;
                colorSwap.Saturation = 0;
                colorSwap.Brightness = 0;
#endif
            }
            else
            {
#if DO_SHADERS
                colorSwap.Hue = DataFile.Data.arrowHSV[noteData % 4].PreciseHue;
                colorSwap.Saturation = DataFile.Data.arrowHSV[noteData % 4].PreciseSaturation;
                colorSwap.Brightness = DataFile.Data.arrowHSV[noteData % 4].PreciseBrightness;
#endif
                RianunNekudot();
            }
        }
    }
}
