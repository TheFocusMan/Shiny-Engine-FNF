using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfGame.Controls
{
    public partial class FlxObject : Border, IUpdatable
    {
        Point _position;
        Point _offset;
        Point _origin;
        Point _velocity = new Point();

        public FlxObject()
        {
            RenderTransformOrigin = new Point(0.5, 0.5);
            HorizontalAlignment = HorizontalAlignment.Left;
            VerticalAlignment = VerticalAlignment.Top;
            RenderTransform = new TransformGroup()
            {
                Children = new TransformCollection()
                {
                    new ScaleTransform(1,1,0.5,0.5),
                    new SkewTransform(0,0,0.5,0.5),
                    new RotateTransform(0,0.5,0.5),
                    new TranslateTransform()
                }
            };
        }

        protected virtual void Update()
        {
            if (Moves)
                UpdateMotion(StaticTimer.DeltaSaconds);
        }

        void IUpdatable.Update()
        {
            Update();
        }


        #region FlxObject.hx
        public virtual Point GetMidPoint() =>new Point(X + Width * 0.5, Y + Height * 0.5);

        /// <summary>
        /// Internal function for updating the position and speed of this object.
        ///  Useful for cases when you need to update this but are buried down in too many supers.
        /// Does a slightly fancier-than-normal integration to help with higher fidelity framerate-independent motion.
        /// </summary>
        /// <param name="elapsed"></param>
        void UpdateMotion(double elapsed)
        {
            var velocityDelta = 0.5 * (Mathf.ComputeVelocity(AngularVelocity, AngularAcceleration, AngularDrag, MaxAngular, elapsed) - AngularVelocity);
            AngularVelocity += velocityDelta;
            Angle += AngularVelocity * elapsed;
            AngularVelocity += velocityDelta;

            velocityDelta = 0.5 * (Mathf.ComputeVelocity(_velocity.X, Acceleration.X, Drag.X, MaxVelocity.X, elapsed) - _velocity.X);
            _velocity.X += velocityDelta;
            var delta = _velocity.X * elapsed;
            _velocity.X += velocityDelta;
            X += delta;

            velocityDelta = 0.5 * (Mathf.ComputeVelocity(_velocity.Y, Acceleration.Y, Drag.Y, MaxVelocity.Y, elapsed) - _velocity.Y);
            _velocity.Y += velocityDelta;
            delta = Velocity.Y * elapsed;
            _velocity.Y += velocityDelta;
            Y += delta;
        }

        public void ScreenCenter()
        {
            double left = (Display.DefaultWidth - Width) / 2;
            double top = (Display.DefaultHeight - Height) / 2;
            Position = new Point(left, top);
        }


        bool _isrefresh;
        protected void RefreshPosition()
        {
            if (!_isrefresh)
            {
                _isrefresh = true;
                var pospoint = new Point(Position.X - Offset.X - Origin.X, Position.Y - Offset.Y - Origin.Y);
                //var cam = GetCamera();
               /* if (cam != null && ScrollFactor != new Point())
                    pospoint = Point.Subtract(pospoint, new Vector(-(ScrollFactor.X * cam.Offset.X), -(ScrollFactor.Y * cam.Offset.Y)));*/
                Margin = new Thickness(pospoint.X, pospoint.Y, pospoint.X, pospoint.Y);
                //RenderTransformOrigin = ScrollFactor;
                InvalidateVisual();
                //InvalidateArrange();
                _isrefresh = false;
            }
        }

        public Camera2D GetCamera()
        {
            var cam = this as FrameworkElement;
            // משיג את המצלמה
            while (cam is not Camera2D && cam != null)
                cam = cam.Parent as FrameworkElement;
            return cam as Camera2D;
        }
        #endregion
    }
}
