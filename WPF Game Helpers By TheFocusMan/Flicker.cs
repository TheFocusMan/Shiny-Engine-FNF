using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace WpfGame
{
    public static class Flicker
    {
        public static void StartFlicker(FrameworkElement[] elements, double duration, double espaled) // אפקט של עמעום תצוגה
        {
            var times = TimeSpan.FromSeconds(duration);
            TimeSpan time = TimeSpan.Zero;
            DispatcherTimer timer = null;
            timer = new DispatcherTimer(TimeSpan.FromSeconds(espaled), DispatcherPriority.Render, (sender, e) =>
              {
                  if (time <= times)
                  {
                      foreach (var element in elements)
                          element.Visibility = element.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
                      time += TimeSpan.FromSeconds(espaled);
                  }
                  else timer.Stop();
              }, Dispatcher.CurrentDispatcher);
            timer.Start();
        }
    }
}
