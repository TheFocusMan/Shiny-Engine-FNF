using Shiny_Engine_FNF.Code;
using System;
using System.Windows.Controls;
using WpfGame.Controls;

namespace Shiny_Engine_FNF.Code.Controls
{
    public class NoteSplash : Sprite
    {
        public ColorSwap colorSwap = null;
        //private string idleAnim;
        private string textureLoaded = null;

        public NoteSplash(double x = 0, double y = 0, int note = 0)
        {
            Position = new(x, y);

            var skin = "noteSplashes";
            if (PlayState.SONG.splashSkin != null && PlayState.SONG.splashSkin.Length > 0) skin = PlayState.SONG.splashSkin;

            loadAnims(skin);

            colorSwap = new ColorSwap();
            Effect = colorSwap;
            SetupNoteSplash(x, y, note);
        }

        public void SetupNoteSplash(double x, double y, int note = 0, string texture = null, double hueColor = 0, double satColor = 0, double brtColor = 0)
        {
            Position = new(x - Note.swagWidth * 0.95, y - Note.swagWidth);
            Opacity = 0.6;

            if (texture == null)
            {
                texture = "noteSplashes";
                if (PlayState.SONG.splashSkin != null && PlayState.SONG.splashSkin.Length > 0) texture = PlayState.SONG.splashSkin;
            }

            if (textureLoaded != texture)
            {
                loadAnims(texture);
            }
            colorSwap.Hue = hueColor;
            colorSwap.Saturation = satColor;
            colorSwap.Brightness = brtColor;
            Offset = new System.Windows.Point(10, 10);

            var animNum = new Random().Next(1, 2);
            Frames.PlayAnimation2("note" + note + "-" + animNum, true, false);
            if (Frames.CurrentAnimationName != null) Frames.Framerate = 24 + new Random().Next(-2, 2);
            _played = true;
        }

        private bool _played;

        void loadAnims(string skin)
        {
            Frames = Paths.GetSparrowAtlas(skin, this);
            for (int i = 1; i < 3; i++)
            {
                Frames.AddByPrefix("note1-" + i, "note splash blue " + i);
                Frames.AddByPrefix("note2-" + i, "note splash green " + i);
                Frames.AddByPrefix("note0-" + i, "note splash purple " + i);
                Frames.AddByPrefix("note3-" + i, "note splash red " + i);
            }
        }

        protected override void Update()
        {
            if (_played)
                if (!Frames.IsPlayingAmimation)
                    (Parent as Panel).Children.Remove(this);
            base.Update();
        }

    }
}
