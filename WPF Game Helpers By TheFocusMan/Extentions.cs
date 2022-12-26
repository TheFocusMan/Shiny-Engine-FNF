using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using WpfGame.Controls;

namespace WpfGame
{
    public static class Extentions
    {
        public static double GetRandomNumber(double minimum, double maximum)
        {
            Random random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }

        public static DispatcherOperation RestartOperation(this DispatcherOperation operation)
        {
            var type = operation.GetType();
            var delgate = (Delegate)type.GetField("_method", CacheKiller.DefualtFuck).GetValue(operation);
            var args = (object[])type.GetField("_args", CacheKiller.DefualtFuck).GetValue(operation);
            var ponity = (DispatcherPriority)type.GetField("_priority", CacheKiller.DefualtFuck).GetValue(operation);
            return operation.Dispatcher.BeginInvoke(delgate, ponity, args);
        }

        public static async Task<Stream> CreateStreamFromUri(string url)
        {
            if (BrowserInteropHelper.IsBrowserHosted)
            {
                using HttpClient client = new HttpClient();
                var ret = await client.GetStreamAsync(url);
                return ret;
            }
            else return new FileStream(url, FileMode.Open, FileAccess.Read);
        }

        public static List<int> AllIndexesOf(this string str, string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("the string to find may not be empty", "value");
            List<int> indexes = new List<int>();
            for (int index = 0; ; index += value.Length)
            {
                index = str.IndexOf(value, index);
                if (index == -1)
                    return indexes;
                indexes.Add(index);
            }
        }

        public static void RegisterTimer(this FrameworkElement element, Action handler)
        {
            StaticTimer.AddEvent(handler);
            element.Unloaded += (sender, e) => StaticTimer.RemoveEvent(handler);
        }

        public static void UpdateHitbox(this Sprite sprite)
        {
            double width = double.IsNaN(sprite.Width) ? sprite.ActualWidth : sprite.Width;
            double height = double.IsNaN(sprite.Height) ? sprite.ActualHeight : sprite.Height;
            sprite.Offset = new Point(-0.5 * (width * sprite.Scale.X - sprite.FrameSize.Width * sprite.Scale.X), -0.5 * (height * sprite.Scale.Y - sprite.FrameSize.Height * sprite.Scale.Y));
            sprite.CenterOrigin();
        }

        public static void ScreenCenter(this FrameworkElement element)
        {
            double left = (Display.DefaultWidth - element.ActualWidth) / 2;
            double top = (Display.DefaultHeight - element.ActualHeight) / 2;
            element.Margin = new Thickness(left, top, 0, 0);
        }

        public static void SetPosition(this FrameworkElement element, double x, double y)
        {
            element.Margin = new Thickness(x, y, 0, 0);
        }
        public static Point GetScreenCenter(this FrameworkElement element)
        {
            double left = element.ActualWidth / 2;
            double top = element.ActualHeight / 2;
            return new Point(left, top);
        }

        public static void setGraphicSize(this FrameworkElement padding, double width = 0, double hieght = 0)
        {
            var scale = new Point(width / padding.Width, hieght / padding.Height);
            padding.Width = Math.Abs(scale.X) * padding.Width;
            padding.Height = Math.Abs(scale.Y) * padding.Height;
        }

        public static double GetZoom(this FrameworkElement padding)
        {
            var tr = (padding.RenderTransform as TransformGroup).Children[0] as ScaleTransform;
            return tr.ScaleX;
        }

        public static Vector GetScale(this FrameworkElement padding)
        {
            var tr = (padding.RenderTransform as TransformGroup).Children[0] as ScaleTransform;
            return new Vector(tr.ScaleX, tr.ScaleY);
        }

        public static Point GetPosition(this FrameworkElement element)
        {
            return new Point(element.Margin.Left, element.Margin.Top);
        }

        public static void SetZoom(this FrameworkElement padding, double zoom = 0)
        {
            var tr = (padding.RenderTransform as TransformGroup).Children[0] as ScaleTransform;
            tr.CenterY = tr.CenterX = 0.5;
            tr.ScaleX = zoom;
            tr.ScaleY = zoom;
        }


        public static void SetZoom(this Sprite padding, double zoom = 0)
        {
            var tr = (padding.RenderTransform as TransformGroup).Children[0] as ScaleTransform;
            tr.ScaleX = zoom;
            tr.ScaleY = zoom;
        }

        public static HsbColor RgbToHsb(this Color rgb)
        {
            // _NOTE #1: Even though we're dealing with a very small range of
            // numbers, the accuracy of all calculations is fairly important.
            // For this reason, I've opted to use double data types instead
            // of float, which gives us a little bit extra precision (recall
            // that precision is the number of significant digits with which
            // the result is expressed).

            var r = rgb.R / 255d;
            var g = rgb.G / 255d;
            var b = rgb.B / 255d;

            var minValue = GetMinimumValue(r, g, b);
            var maxValue = GetMaximumValue(r, g, b);
            var delta = maxValue - minValue;

            double hue = 0;
            double saturation;
            var brightness = maxValue * 100;

            if (Math.Abs(maxValue - 0) < double.Epsilon || Math.Abs(delta - 0) < double.Epsilon)
            {
                hue = 0;
                saturation = 0;
            }
            else
            {
                // _NOTE #2: FXCop insists that we avoid testing for floating 
                // point equality (CA1902). Instead, we'll perform a series of
                // tests with the help of Double.Epsilon that will provide 
                // a more accurate equality evaluation.

                if (Math.Abs(minValue - 0) < double.Epsilon)
                {
                    saturation = 100;
                }
                else
                {
                    saturation = delta / maxValue * 100;
                }

                if (Math.Abs(r - maxValue) < double.Epsilon)
                {
                    hue = (g - b) / delta;
                }
                else if (Math.Abs(g - maxValue) < double.Epsilon)
                {
                    hue = 2 + (b - r) / delta;
                }
                else if (Math.Abs(b - maxValue) < double.Epsilon)
                {
                    hue = 4 + (r - g) / delta;
                }
            }

            hue *= 60;
            if (hue < 0)
            {
                hue += 360;
            }

            return new HsbColor(
                hue,
                saturation,
                brightness,
                rgb.A);
        }

        public static Color HsbToRgb(this HsbColor hsb)
        {
            double red = 0, green = 0, blue = 0;

            double h = hsb.Hue;
            var s = (double)hsb.Saturation / 100;
            var b = (double)hsb.Brightness / 100;

            if (Math.Abs(s - 0) < double.Epsilon)
            {
                red = b;
                green = b;
                blue = b;
            }
            else
            {
                // the color wheel has six sectors.

                var sectorPosition = h / 60;
                var sectorNumber = (int)Math.Floor(sectorPosition);
                var fractionalSector = sectorPosition - sectorNumber;

                var p = b * (1 - s);
                var q = b * (1 - s * fractionalSector);
                var t = b * (1 - s * (1 - fractionalSector));

                // Assign the fractional colors to r, g, and b
                // based on the sector the angle is in.
                switch (sectorNumber)
                {
                    case 0:
                        red = b;
                        green = t;
                        blue = p;
                        break;

                    case 1:
                        red = q;
                        green = b;
                        blue = p;
                        break;

                    case 2:
                        red = p;
                        green = b;
                        blue = t;
                        break;

                    case 3:
                        red = p;
                        green = q;
                        blue = b;
                        break;

                    case 4:
                        red = t;
                        green = p;
                        blue = b;
                        break;

                    case 5:
                        red = b;
                        green = p;
                        blue = q;
                        break;
                }
            }

            var nRed = Convert.ToInt32(red * 255);
            var nGreen = Convert.ToInt32(green * 255);
            var nBlue = Convert.ToInt32(blue * 255);

            return Color.FromArgb((byte)hsb.Alpha, (byte)nRed, (byte)nGreen, (byte)nBlue);
        }

        /// <summary>
        /// Determines the maximum value of all of the numbers provided in the
        /// variable argument list.
        /// </summary>
        private static double GetMaximumValue(
            params double[] values)
        {
            var maxValue = values[0];

            if (values.Length >= 2)
            {
                for (var i = 1; i < values.Length; i++)
                {
                    var num = values[i];
                    maxValue = Math.Max(maxValue, num);
                }
            }

            return maxValue;
        }

        /// <summary>
        /// Determines the minimum value of all of the numbers provided in the
        /// variable argument list.
        /// </summary>
        private static double GetMinimumValue(
            params double[] values)
        {
            var minValue = values[0];

            if (values.Length >= 2)
            {
                for (var i = 1; i < values.Length; i++)
                {
                    var num = values[i];
                    minValue = Math.Min(minValue, num);
                }
            }

            return minValue;
        }

        /// <summary>
        /// For WPF
        /// </summary>
        /// <param name="hexvalue"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public static Color ToColor(this uint hexvalue)
        {
            byte[] bytes = BitConverter.GetBytes(hexvalue);
            return Color.FromArgb(bytes[3], bytes[2], bytes[1], bytes[0]);
        }
    }
}
