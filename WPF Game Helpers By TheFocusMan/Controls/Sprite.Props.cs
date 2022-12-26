using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfGame.AnimationsSheet;

namespace WpfGame.Controls
{
    public partial class Sprite
    {

        internal void RawSetFrame(AnimationSheet sheet)
            => _sheet = sheet;

        #region Public Properties
        #region FlxSprite.hx

        public AnimationSheet Frames
        {
            get => _sheet;
            set
            {
                _sheet = value;
                try
                {
                    if (_sheet != null)
                    {
                        var data = _sheet.GetAnimationSubTextures()[0];
                        Width = data.Bounds.Width;
                        Height = data.Bounds.Height;
                    }
                }
                catch { }
            }
        }

        public BitmapSource Source
        {
            get => (BitmapSource)bruscl.ImageSource;
            set
            {
                bruscl.ImageSource = value;
                if (value != null)
                {
                    image.Height = value.PixelHeight;
                    image.Width = value.PixelWidth;
                }
            }
        }

        public Vector Scale
        {
            get
            {
                var f = new Point(((RenderTransform as TransformGroup).Children[0] as ScaleTransform).ScaleX, ((RenderTransform as TransformGroup).Children[0] as ScaleTransform).ScaleY);
                return new Vector(FlipX ? -f.X : f.X, FlipY ? -f.Y : f.Y);
            }
            set
            {
                ((RenderTransform as TransformGroup).Children[0] as ScaleTransform).ScaleX = FlipX ? -value.X : value.X;
                ((RenderTransform as TransformGroup).Children[0] as ScaleTransform).ScaleY = FlipY ? -value.Y : value.Y;
            }
        }

        public Point SacondOffsetPoint
        {
            get
            {
                var tr = (RenderTransform as TransformGroup).Children[3] as TranslateTransform;
                return new Point(tr.X, tr.Y);
            }
            set
            {
                var tr = (RenderTransform as TransformGroup).Children[3] as TranslateTransform;
                tr.X = value.X;
                tr.Y = value.Y;
            }
        }

        public Color Color
        {
            get
            {
                return Colors.Black;
            }
            set
            {

            }
        }

        public double ScaleX
        {
            get => Scale.X;
            set => Scale = new Vector(value, ScaleY);
        }

        public double ScaleY
        {
            get => Scale.Y;
            set => Scale = new Vector(ScaleX, value);
        }

        private bool _flipX;
        private bool _flipY;
        public bool FlipX
        {
            get => _flipX;
            set
            {
                var scale = Scale;
                _flipX = value;
                Scale = scale; // reload
            }
        }
        public bool FlipY
        {
            get => _flipY; 
            set
            {
                var scale = Scale;
                _flipY = value;
                Scale = scale; // reload
            }
        }

        public bool Antialiasing
        {
            get
            {
                var scalemode = RenderOptions.GetBitmapScalingMode(image);
                return !(scalemode == BitmapScalingMode.Linear && SnapsToDevicePixels);
            }
            set
            {
                if (value)
                {
                    image.SnapsToDevicePixels =SnapsToDevicePixels = false;
                    RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.Unspecified);
                }
                else
                {
                    image.SnapsToDevicePixels = SnapsToDevicePixels = true;
                    RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.Linear);
                }
            }
        }

        public Size FrameSize { get; set; }
        #endregion
        /// <summary>
        /// Dispose image when unloaded used when ur on games
        /// </summary>
        public bool DisposeImageOnUnload { get; set; } = true;
        #endregion


        internal Canvas ControlAnim { get => image; }

        internal Thickness ControlMargarin { get => image.Margin; set => image.Margin = value; }
    }
}
