using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;
using WpfGame;
using WpfGame.Controls;

namespace Shiny_Engine_FNF.Code.Controls
{
    internal class BGSprite : Sprite
    {
        private string idleAnim;
        private bool _loop = false;
        public BGSprite(string image, double x = 0, double y = 0)
            : this(image, x, y, null)
        {

        }

        public BGSprite(string image, double x = 0, double y = 0, string[] animArray = null)
            : this(image, x, y, 1, 1, animArray)
        {

        }

        protected override void Update()
        {
            RefreshPosition();
            base.Update();
        }

        public BGSprite(string image, double x = 0, double y = 0, double scrollX = 1, double scrollY = 1, string[] animArray = null, bool loop = true)
        {
            X = x;
            Y = y;
            ScrollFactor = new(scrollX, scrollY);
            try
            {
                if (animArray != null)
                {
                    Frames = Paths.GetSparrowAtlas(image, this);
                    if (Frames != null)
                    {
                        for (int i = 0; i < animArray.Length; i++)
                        {
                            var anim = animArray[i];
                            if (idleAnim == null)
                            {
                                idleAnim = anim;
                                Frames.PlayAnimation2(anim,false, loop);
                            }
                        }
                    }
                }
                else
                {
                    if (image != null)
                    {
                        var file = Paths.Image(image);
                        if (File.Exists(file))
                            Source = CreateGoodImage(file);
                        ResyncSize();
                    }
                }
            }
            catch (Exception)
            {
                Trace.WriteLine("Fuck u PsycEngine");
            }
            RenderTransformOrigin = new System.Windows.Point();
            _loop = loop;
        }

        public void Dance(bool forceplay = false)
        {
            if (idleAnim != null)
                Frames.PlayAnimation2(idleAnim, forceplay, _loop);
        }
    }
}
