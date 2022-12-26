using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace WpfGame
{
    public class CustomLimitTimer
    {
        int _counters;
        int _current;
        Action _timer;
        bool _active = true;
        public CustomLimitTimer(TimeSpan invertal, int times, Action action)
        {
            _counters = times;
            _timer = async delegate
            {
                if (_counters > -1)
                {
                    if (_current < _counters) _current++;
                    else return;
                }
                await Task.Delay(invertal);
                action();
                Start();
            };
            Start();
        }

        public int CurrentNumber => _current;

        public bool Finished => _current >= _counters;

        public bool Active
        {
            get => _active; set
            {
                _active = value;
                if (value) Start();
            }
        }

        public void Start()
        {
            if (Active)
                Dispatcher.CurrentDispatcher.BeginInvoke(_timer);
        }
    }
}
