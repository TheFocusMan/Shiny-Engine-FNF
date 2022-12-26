using Shiny_Engine_FNF.Code;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfGame.Controls;

namespace Shiny_Engine_FNF.Code.Controls
{
    public class AttachedSprite : Sprite
    {
        public FrameworkElement sprTracker;
        public double xAdd = 0;
        public double yAdd = 0;
        public double angleAdd = 0;
        public double alphaMult = 1;

        public bool copyAngle = true;
        public bool copyAlpha = true;
        public bool copyVisible = false;

        public AttachedSprite() : this(null, null, null, false)
        {

        }

        public AttachedSprite(string file, string anim, string library, bool loop) : base()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                if (anim != null)
                {
                    Frames = Paths.GetSparrowAtlas(file, this, library, 24);
                    Frames.AddByPrefix("idle", anim);
                    Frames.PlayAnimation2("idle", false, loop);
                }
                else if (file != null)
                {
                    Source = CreateGoodImage(Paths.Image(file));
                    ResyncSize();
                }
            }
        }

        protected override void Update()
        {
            if (sprTracker != null)
            {
                Position = new Point(sprTracker.Margin.Left + xAdd, sprTracker.Margin.Top + yAdd);

                if (copyAngle)
                    Angle = (((sprTracker.RenderTransform as TransformGroup)?.Children[3] as RotateTransform)?.Angle).GetValueOrDefault() + angleAdd;

                if (copyAlpha)
                    Opacity = sprTracker.Opacity * alphaMult;

                if (copyVisible)
                    Visibility = sprTracker.Visibility;
            }
        }
    }
}
