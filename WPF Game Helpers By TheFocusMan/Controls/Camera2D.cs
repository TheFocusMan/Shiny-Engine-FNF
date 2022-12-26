using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfGame.Controls
{
    public class Camera2D : CanvasWithMargarin, IUpdatable
    {
        /// <summary>
        /// Internal, percentage of screen size representing the maximum distance that the screen can move while shaking.
        /// </summary>
        double _fxShakeIntensity = 0;

        /// <summary>
        /// Internal, duration of the `shake()` effect.
        /// </summary>
        double _fxShakeDuration = 0;

        Action _fxShakeComplete;
        FlxObject _targetfollow;

        public Camera2D()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                _targetfollow = new FlxObject() { Width=1,Height=1,X=0,Y=0 }; // set follow target
                FollowLearp = 60.0 / StaticTimer.MaxFrameRate;
            }
        }

        static Camera2D()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Camera2D), new FrameworkPropertyMetadata(typeof(Camera2D)));
            RenderTransformProperty.OverrideMetadata(typeof(Camera2D), new FrameworkPropertyMetadata(new TransformGroup()
            { Children = new TransformCollection() { new ScaleTransform(), new SkewTransform(), new RotateTransform(), new TranslateTransform() } }));
        }

        protected virtual void Update()
        {
            // follow the target, if there is one
            UpdateFollow();
            UpdateShake(StaticTimer.DeltaSaconds);
            //UpdateScroll();
            for (int i = 0; i < InternalChildren.Count; i++)
                UpdateChilds(InternalChildren[i] as FrameworkElement);
        }

        private void UpdateChilds(FrameworkElement element)
        {
            if (element == null) return;
            if (element is IUpdatable && element.IsLoaded)
                (element as IUpdatable).Update();
            if (element is ContentControl)
                UpdateChilds((element as ContentControl).Content as FrameworkElement);
            if (element is Panel panel)
            {
                for (int i = 0; i < panel.Children.Count; i++)
                    UpdateChilds(panel.Children[i] as FrameworkElement);
            }
        }

        void IUpdatable.Update()
        {
            Update();
        }


        Rect _deadzone;
        Vector _scrollTarget;
        void UpdateFollow()
        {
            _targetfollow.Position = FollowinTarget; // update follow pos

            double edge;
            var targetX = _targetfollow.X;
            var targetY = _targetfollow.Y;

            edge = targetX - _deadzone.X;
            if (_scrollTarget.X > edge)
            {
                _scrollTarget.X = edge;
            }
            edge = targetX + _targetfollow.Width - _deadzone.X - _deadzone.Width;
            if (_scrollTarget.X < edge)
            {
                _scrollTarget.X = edge;
            }

            edge = targetY - _deadzone.Y;
            if (_scrollTarget.Y > edge)
            {
                _scrollTarget.Y = edge;
            }
            edge = targetY + _targetfollow.Height - _deadzone.Y - _deadzone.Height;
            if (_scrollTarget.Y < edge)
            {
                _scrollTarget.Y = edge;
            }
            /* if (_followlearp >= 60 / StaticTimer.MaxFrameRate)
             {
                 var _target = Vector.Multiply(-this.GetZoom(), _scrollTarget);
                 Offset = (_target); // no easing
             }*/
            //else
            //{
            var off = Offset;
            //off = new Vector(off.X / this.GetZoom(), off.Y / this.GetZoom());
            off.X += (_scrollTarget.X - off.X) * _followlearp * StaticTimer.MaxFrameRate / 60.0;
            off.Y += (_scrollTarget.Y - off.Y) * _followlearp * StaticTimer.MaxFrameRate / 60.0;
            //off = Vector.Multiply(-this.GetZoom(), off); // for move best
            Offset = off;
            //}
        }
        /// <summary>
        /// Updates (bounds) the camera scroll.
        /// Called every frame by camera's <see cref="Update"/> method.
        /// </summary>
        /// <returns></returns>
        public void UpdateScroll()
        {
            // Adjust bounds to account for zoom
            var zoom = this.GetZoom();

            var off = Offset;
            off = new Vector(off.X / zoom, off.Y / zoom);
            // Make sure we didn't go outside the camera's bounds
            double w1 = double.IsNaN(Width) ? ActualWidth : Width;
            double h1 = double.IsNaN(Height) ? ActualHeight : Height;
            off.X = Mathf.Clamp(off.X, 0, w1);
            off.Y = Mathf.Clamp(off.Y, 0, h1);
            Offset = off;
        }

        Vector _offset;


        internal Vector Offset
        {
            get => _offset;
            set
            {
                _offset = value;
                double w1 = double.IsNaN(Width) ? ActualWidth : Width;
                double h1 = double.IsNaN(Height) ? ActualHeight : Height;
                Margin = new Thickness(_offset.X*-1,_offset.Y*-1,0,0); //- new Vector(ActualWidth,ActualHeight);
            }
        }

        private double _followlearp;
        public double FollowLearp
        {
            get => _followlearp;
            set => _followlearp = Mathf.Clamp(value, 0, 60.0 / StaticTimer.MaxFrameRate);
        }


        void UpdateShake(double elapsed)
        {
            if (_fxShakeDuration > 0)
            {
                _fxShakeDuration -= elapsed;
                if (_fxShakeDuration <= 0)
                {
                    _fxShakeComplete?.Invoke();
                    _fxShakeComplete = null;
                }
                else
                {
                    var fd = new Vector(Extentions.GetRandomNumber(-_fxShakeIntensity * Width, _fxShakeIntensity * Width) * this.GetZoom(),
                        Extentions.GetRandomNumber(-_fxShakeIntensity * Height, _fxShakeIntensity * Height) * this.GetZoom());
                }
            }
        }
        /// <summary>
        /// A simple screen-shake effect.
        /// </summary>
        /// <param name="intensity">Percentage of screen size representing the maximum distance 
        /// that the screen can move while shaking.</param>
        /// <param name="duration">The length in seconds that the shaking effect should last.</param>
        /// <param name="OnComplete">A function you want to run when the shake effect finishes.</param>
        /// <param name="force">Force the effect to reset (default = `true`, unlike `flash()` and `fade()`!).</param>
        public void Shake(double intensity = 0.05, double duration = 0.5, Action OnComplete = null, bool force = true)
        {
            if (!force && _fxShakeDuration > 0)
                return;

            _fxShakeIntensity = intensity;
            _fxShakeDuration = duration;
            _fxShakeComplete = OnComplete;
        }

        public void Follow(Point position,double? lerp=null)
        {
            if (lerp == null)
                lerp = 60.0 / StaticTimer.MaxFrameRate;


            double w = 0;
            double h = 0;
            double width = double.IsNaN(Width) ? ActualWidth : Width;
            double height = double.IsNaN(Height) ? ActualHeight : Height;

            if (_targetfollow != null)
            {
                w = _targetfollow.Width;
                h = _targetfollow.Height;
            }
            _deadzone = new Rect((width - w) / 2, (height - h) / 2 - h * 0.25, w, h);

            FollowinTarget = position;
            FollowLearp = lerp.Value;
        }

        private Point FollowinTarget
        {
            get { return (Point)GetValue(FollowinTargetProperty); }
            set { SetValue(FollowinTargetProperty, value); }
        }



        // Using a DependencyProperty as the backing store for FollowinTarget.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FollowinTargetProperty =
            DependencyProperty.Register("FollowinTarget", typeof(Point), typeof(Camera2D), new PropertyMetadata(new Point()));



    }
}
