using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Threading;

namespace WpfGame
{
    public class StaticTimer
    {
        internal static readonly DispatcherTimer Timer;
        internal static readonly Stopwatch Stopwatch;

        // debug only
        public static int TimerCounterEvents { get; private set; } = 0;

        public static double DeltaSaconds { get => _delta; }

        public static double CurrentFrameRate { get => 1 / _delta; }

        public static double MaxFrameRate
        {
            get => _maxFps;
            set
            {
                _maxFps = value;
                Timer.Interval = TimeSpan.FromSeconds(1 / value);
            }
        }

        private static double _maxFps;

        private static double _delta;
        private static long _lasttime;

        // For better viewing
        static EventHandlerList _list;
        static object _updateKey = new object();

        static StaticTimer()
        {
            _list = new EventHandlerList();
            _lasttime = DateTime.UtcNow.Ticks;
            Timer = new DispatcherTimer(DispatcherPriority.Render);
            MaxFrameRate = 60;
            Timer.Start();
            Timer.Tick += (sender, e) =>
            {
                _delta = MathDeltaTime();
                _lasttime = DateTime.UtcNow.Ticks;
                _list[_updateKey]?.DynamicInvoke();
            };
            Stopwatch = Stopwatch.StartNew();
        }
        /// <summary>
        /// for Better debug
        /// </summary>
        private static double MathDeltaTime()
        {
            var time = TimeSpan.FromTicks(Math.Abs(DateTime.UtcNow.Ticks - _lasttime)); // Stolen from Unity
            if (time.TotalSeconds > 1) return 0; // timer fix
            return time.TotalSeconds;
        }

        public static void AddEvent(Action handler)
        {
            TimerCounterEvents++;
            _list.AddHandler(_updateKey, handler);
        }

        public static void PutFavorite(Action action)
        {
            var array = _list[_updateKey].GetInvocationList();
            foreach (var v in array)
                _list.RemoveHandler(_updateKey, v);
            _list.AddHandler(_updateKey, action);
            foreach (var v in array)
                _list.AddHandler(_updateKey, v);
        }


        public static void RemoveEvent(Action handler)
        {
            TimerCounterEvents--;
            _list.RemoveHandler(_updateKey, handler);
        }
    }
}
