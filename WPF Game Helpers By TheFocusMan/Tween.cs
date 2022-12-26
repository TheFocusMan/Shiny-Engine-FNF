using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace WpfGame
{
    public class Tween : TweenBase<double>
    {
        public Tween(double start, double end, EasingFunctionBase function, double duration) : base(start, end, function, duration)
        {
        }

        protected override double MathValue(double start, double end, double easingInc)
        {
            return Mathf.LerpFloat(start, end, easingInc);
        }
    }

    public class TweenColor : TweenBase<Color>
    {
        public TweenColor(Color start, Color end, EasingFunctionBase function, double duration) : base(start, end, function, duration)
        {
        }

        protected override Color MathValue(Color start, Color end, double easingInc)
        {
            HsbColor color = start;
            HsbColor color2 = end;

            return new HsbColor(Mathf.LerpFloat(color.PreciseHue, color2.PreciseHue, easingInc),
                Mathf.LerpFloat(color.PreciseSaturation, color2.PreciseSaturation, easingInc),
                Mathf.LerpFloat(color.PreciseBrightness, color2.PreciseBrightness, easingInc),
                 (byte)Mathf.LerpFloat(color.Alpha, color2.Alpha, easingInc));
        }
    }

    public class TweenPoint : TweenBase<Point>
    {
        public TweenPoint(Point start, Point end, EasingFunctionBase function, double duration) : base(start, end, function, duration)
        {
        }
        protected override Point MathValue(Point start, Point end, double easingInc)
        {
            return new Point(Mathf.LerpFloat(start.X, end.X, easingInc), Mathf.LerpFloat(start.Y, end.Y, easingInc));
        }
    }
#if !DEBUG
    [Obsolete("Its easyer to use storyboard", UrlFormat = "https://docs.microsoft.com/en-us/dotnet/desktop/wpf/graphics-multimedia/easing-functions?view=netframeworkdesktop-4.8")]
#endif
    public abstract class TweenBase<T> where T : struct
    {
        EasingFunctionBase _function;
        private readonly T _start;
        private readonly T _end;
        private readonly double _duration;
        private double _currentTime;
        private T _value;

        public TweenBase(T start, T end, EasingFunctionBase function, double duration)
        {
            _start = start;
            _end = end;
            _function = function;
            _duration = duration;
        }

        public EasingFunctionBase Function
        {
            get => _function;
            set => _function = value;
        }

        public T StartValue { get => _start; }

        public T EndValue { get => _end; }

        public T CurrentValue { get => _value; }


        protected abstract T MathValue(T start, T end, double easingInc);

        public void Cancel()
        {
            _cancel = true;
        }

        private bool _cancel;
        private void Update()
        {
            if (_plying)
            {
                if (Math.Round(_currentTime / _duration, 1) <= 1 && !_cancel)
                {
                    var ease = _currentTime / _duration;
                    if (_function != null)
                        _function.Dispatcher.BeginInvoke(DispatcherPriority.Inactive,
                            new Action(() => ease = _function.Ease(_currentTime / _duration))); // prevent bugs
                    // Update the value
                    _value = MathValue(_start, _end, Mathf.Clamp(ease, 0, 1));
                    UpdateValue?.Invoke(this, _value);
                    _currentTime += 1.0 / 24;
                }
                else
                {
                    _plying = false;
                    StaticTimer.RemoveEvent(Update);
                    Complite?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public void Start()
        {
            _plying = true;
            StaticTimer.AddEvent(Update);
        }

        bool _plying;

        public event EventHandler<T> UpdateValue;

        public event EventHandler Complite;

        public static async void Start(double start, double end, double duration, EasingFunctionBase function, EventHandler<double> update, EventHandler complite, double startdelay = 0)
        {
            await Task.Delay(TimeSpan.FromSeconds(startdelay));
            Tween tween = new Tween(start, end, function, duration);
            tween.UpdateValue += update;
            tween.Complite += complite;
            tween.Start();
        }
    }
}
