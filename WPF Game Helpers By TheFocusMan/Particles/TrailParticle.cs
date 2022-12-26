using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WpfGame.Controls;

namespace WpfGame.Particles
{
    public class TrailParticle : Sprite
    {
        private Sprite _parent;

        private int _times;
        private int _counttimes;
        double _opiticy;
        double _opiticycount;
        double _diff;
        DrawingGroup _group;

        public TrailParticle(Sprite target, int count = 10, int delay = 3, double alpha = 0.4, double diff = 0.05)
        {
            _parent = target;
            DispatcherTimer timer = new DispatcherTimer()
            { Interval = TimeSpan.FromSeconds(1.0 / delay) };
            _counttimes = _times = count;
            _opiticy = _opiticycount = alpha;
            _diff = diff;
            timer.Tick += Update;
            timer.Start();
            Unloaded += (s, e) => timer.Stop();
        }

        private void Update(object sender, EventArgs e)
        {
            X = _parent.X;
            Y = _parent.Y;
            Width = _parent.Width;
            Height = _parent.Height;
            if (_counttimes > 0)
            {
                _group = VisualTreeHelper.GetDrawing(_parent);
                InvalidateVisual();
                _counttimes--;
                _opiticycount -= _diff;
            }
            else
            {
                _opiticycount = _opiticy;
                _counttimes = _times;
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            dc.DrawDrawing(_group);
            dc.PushOpacity(_opiticycount);
            base.OnRender(dc);
        }
    }
}
