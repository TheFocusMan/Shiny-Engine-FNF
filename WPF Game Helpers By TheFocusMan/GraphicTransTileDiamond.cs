using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WpfGame
{
    public class GraphicTransTileDiamond : Canvas
    {
        readonly bool _flash;
        public GraphicTransTileDiamond(bool flash) : base()
        {
            _flash = flash;
        }

        public void Start(double time, Color color, bool reversed)
        {
            if (!_flash)
            {
                Background = new LinearGradientBrush(new GradientStopCollection()
                {
                    new GradientStop(Color.FromArgb(0,0,0,0),0),
                    new GradientStop(color,0)
                })
                {
                    StartPoint = new Point(0.5, reversed ? 1 : 0),
                    EndPoint = new Point(0.5, reversed ? 0 : 1)
                };

                Tween tr = new Tween(0, 1, new CubicEase()
                {
                    EasingMode = EasingMode.EaseInOut
                }, time);
                tr.UpdateValue += (sender, e) =>
                {
                    var r = (Background as LinearGradientBrush).GradientStops[1];
                    r.Offset = e;
                    InvalidateVisual();
                };
                tr.Complite += (s, e) => Complited?.Invoke(s, e);
                tr.Start();
            }
            else
            {
                Background = new SolidColorBrush(color);
                Tween tr = new Tween(255, 0, new SineEase() { EasingMode = EasingMode.EaseIn }, time);
                tr.UpdateValue += (sender, e) =>
                {
                    (Background as SolidColorBrush).Color = Color.FromArgb((byte)e, color.R, color.G, color.B);
                };
                tr.Complite += (s, e) => Complited?.Invoke(s, e);
                tr.Start();
            }
        }

        public event EventHandler Complited;
    }
}
