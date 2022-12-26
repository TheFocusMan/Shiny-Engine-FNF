using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace WpfGame
{
    /// <summary>
    /// Represents a HSV (=HSB) color space.
    /// http://en.wikipedia.org/wiki/HSV_color_space
    /// </summary>
    [Serializable]
    public struct HsbColor
    {
        public HsbColor(
            double hue,
            double saturation,
            double brightness,
            int alpha)
        {
            PreciseHue = hue;
            PreciseSaturation = saturation;
            PreciseBrightness = brightness;
            Alpha = alpha;
        }

        /// <summary>
        /// Gets or sets the hue. Values from 0 to 360.
        /// </summary>

        public double PreciseHue { get; set; }

        /// <summary>
        /// Gets or sets the saturation. Values from 0 to 100.
        /// </summary>

        public double PreciseSaturation { get; set; }

        /// <summary>
        /// Gets or sets the brightness. Values from 0 to 100.
        /// </summary>

        public double PreciseBrightness { get; set; }


        public int Hue => Convert.ToInt32(PreciseHue);

        public int Saturation => Convert.ToInt32(PreciseSaturation);


        public int Brightness => Convert.ToInt32(PreciseBrightness);

        /// <summary>
        /// Gets or sets the alpha. Values from 0 to 255.
        /// </summary>

        public int Alpha { get; set; }

        public static implicit operator Color(HsbColor color)
        {
            return color.HsbToRgb();
        }

        public static implicit operator HsbColor(Color color)
        {
            return color.RgbToHsb();
        }

        public override string ToString()
        {
            return $@"Hue: {Hue}; saturation: {Saturation}; brightness: {Brightness}.";
        }

        public override bool Equals(
            object obj)
        {
            var equal = false;

            if (obj is HsbColor color)
            {
                var hsb = color;

                if (Math.Abs(PreciseHue - hsb.PreciseHue) < 0.001 &&
                    Math.Abs(PreciseSaturation - hsb.PreciseSaturation) < 0.001 &&
                    Math.Abs(PreciseBrightness - hsb.PreciseBrightness) < 0.001)
                {
                    equal = true;
                }
            }

            return equal;
        }

        public override int GetHashCode()
        {
            return $@"H:{Hue}-S:{Saturation}-B:{Brightness}-A:{Alpha}".GetHashCode();
        }
    }
}
