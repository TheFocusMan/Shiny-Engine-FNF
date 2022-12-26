using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace WpfGame.Controls
{
    public partial class FlxObject
    {
        #region FlxObject.hx

        public Point ScrollFactor 
        {
            get => (Point)Point.Subtract(new Point(1, 1), RenderTransformOrigin);
            set => RenderTransformOrigin = (Point)Point.Subtract(new Point(1, 1), value);
        }

        public bool Moves { get; set; }

        /// <summary>
        ///  The basic speed of this object (in pixels per second).
        /// </summary>
        public Point Velocity { get => _velocity; set => _velocity = value; }

        /// <summary>
        /// If you are using <see cref="Acceleration"/>, you can use <see cref="MaxVelocity"/> with it
        /// to cap the speed automatically (very useful!).
        /// </summary>
        public Point MaxVelocity { get; set; } = new Point(10000, 10000);

        /// <summary>
        /// How fast the speed of this object is changing (in pixels per second).
        /// Useful for smooth movement and gravity.
        /// </summary>
        public Point Acceleration { get; set; } = new Point();

        /// <summary>
        /// This isn't drag exactly, more like deceleration that is only applied
        /// when <see cref="Acceleration"/> is not affecting the sprite.
        /// </summary>
        public Point Drag { get; set; } = new Point();

        /// <summary>
        /// This is how fast you want this sprite to spin (in degrees per second).
        /// </summary>
        public double AngularVelocity { get; set; } = 0;

        /// <summary>
        /// How fast the spin speed should change (in degrees per second).
        /// </summary>
        public double AngularAcceleration { get; set; } = 0;
        /// <summary>
        ///  Like drag but for spinning.
        /// </summary>
        public double AngularDrag { get; set; } = 0;

        /// <summary>
        /// Use in conjunction with angularAcceleration for fluid spin speed control.
        /// </summary>
        public double MaxAngular { get; set; } = 10000;

        public double X
        {
            get => Position.X;
            set => Position = new Point(value, Y);
        }

        public double Y
        {
            get => Position.Y;
            set => Position = new Point(X, value);
        }


        public Point Position
        {
            get => _position;
            set
            {
                _position = value;
                RefreshPosition();
            }
        }

        public double OffsetX
        {
            get => Offset.X;
            set => Offset = new Point(value, OffsetY);
        }

        public double OffsetY
        {
            get => Offset.Y;
            set => Offset = new Point(OffsetX, value);
        }


        public Point Offset
        {
            get => _offset;
            set
            {
                _offset = value;
                RefreshPosition();
            }
        }

        public Point Origin
        {
            get => _origin;
            set
            {
                _origin = value;
                RefreshPosition();
            }
        }

        public double Angle
        {
            get => ((RenderTransform as TransformGroup).Children[2] as RotateTransform).Angle;
            set => ((RenderTransform as TransformGroup).Children[2] as RotateTransform).Angle = value;
        }
        #endregion
    }
}
