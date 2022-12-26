using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfGame.AnimationsSheet;

namespace WpfGame.Controls
{
    /// <summary>
    /// The Sprite it can be a character with animation or a animated picture
    /// </summary>
    public partial class Sprite : FlxObject, IUpdatable
    {
        private AnimationSheet _sheet;

        #region Initalize (Microsoft is dumb)
        internal Canvas image;
        ImageBrush bruscl;

        public Sprite()
        {
            bruscl = new ImageBrush()
            {
                RelativeTransform = new TransformGroup()
                {
                    Children = new TransformCollection()
                    {
                        new ScaleTransform() {CenterX=0.5,CenterY=0.5},
                        new SkewTransform() {CenterX = 0.5,CenterY=0.5},
                        new RotateTransform(0,0.5,0.5),
                        new TranslateTransform()
                    }
                }
            };
            image = new Canvas()
            {
                ClipToBounds = true,
                Background = bruscl
            };
            ClipToBounds = true;
            Child = image;
            Unloaded += OnDestroy;
        }

        private void OnDestroy(object sender, RoutedEventArgs e)
        {
            if (Source != null)
            {
                if (DisposeImageOnUnload && Source is BitmapImage bitmap)
                    CacheKiller.GetImageChache().Remove(bitmap.UriSource); // השם ישמור זה לקח זמן
                CacheKiller.DestroyImage(Source); // למנוע באג
                Source = null;
            }
            if (Effect != null)
            {
                CacheKiller.DestroyObject(Effect); // למנוע באג
                Effect = null;
            }
        }
        #endregion

        public Size AvgFrameSize()
        {
            var fr = Frames.GetAnimationSubTexturesByName(Frames.CurrentAnimationName)
                .Where(x => x.Frame.Size != new Size());
            if (fr.Any())
                return new Size(fr.Average(x => x.Frame.Width), fr.Average(y => y.Frame.Height));
            else
                return new();
        }

        protected override void Update()
        {
            Frames?.Update();
            base.Update();
        }

        public void ResyncSize()
        {
            Height = image.Height;
            Width = image.Width;
        }

        protected virtual void OnFrameChangingPosition()
        {

        }

        internal void InvokeFrameChangingPosition()
        {
            OnFrameChangingPosition();
        }


        /// <summary>
        /// This is do a better texture load
        /// </summary>
        public static BitmapSource CreateGoodImage(string uri)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            bitmap.UriSource = new Uri(uri);
            bitmap.EndInit();
            return bitmap;
        }

        #region FlxSprite.hx
        public void CenterOrigin()
        {
            Origin = new Point(FrameSize.Width *0.5 + SacondOffsetPoint.X, FrameSize.Height * 0.5 + SacondOffsetPoint.Y);
            var scaledOrigin = new Point(Origin.X * Scale.X, Origin.Y * Scale.Y);
            Origin = new Point(Origin.X- scaledOrigin.X, Origin.Y- scaledOrigin.Y);
            RenderTransformOrigin = new Point(0.5, 0.5);
        }

        public void CenterOffsets()
        {
            Offset = new Point((FrameSize.Width - Width) * 0.5, (FrameSize.Height - Height) * 0.5);
        }

        public override Point GetMidPoint() => new(X + (Width * ScaleX) * 0.5, Y + (Height * ScaleY) * 0.5);

        public Point GetGraphicMidpoint()
        {
            var size = FrameSize == new Size() ? AvgFrameSize() : FrameSize;
            return new(X + (size.Width * ScaleX) * 0.5, Y + (size.Height * ScaleY) * 0.5);
        }
        #endregion
    }
}
