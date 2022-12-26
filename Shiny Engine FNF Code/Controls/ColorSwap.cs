using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Effects;
using System.Reflection;
using WpfGame;

namespace Shiny_Engine_FNF.Code.Controls
{
    public class ColorSwap : ShaderEffect
    {
        static ColorSwap()
        {
            // Associate _pixelShader with our compiled pixel shader
            _pixelShader.UriSource = MakePackUri("Controls/ColorSwapShader.ps");
        }

        // MakePackUri is a utility method for computing a pack uri
        // for the given resource. 
        public static Uri MakePackUri(string relativeFile)
        {
            Assembly a = typeof(ColorSwap).Assembly;

            // Extract the short name.
            string assemblyShortName = a.ToString().Split(',')[0];

            string uriString = "pack://application:,,,/" +
                assemblyShortName +
                ";component/" +
                relativeFile;

            return new Uri(uriString);
        }

        private static PixelShader _pixelShader = new PixelShader();

        public ColorSwap()
        {
            PixelShader = _pixelShader;
            UpdateShaderValue(InputProperty);
            Input = ImplicitInput;
        }

        public Brush Input
        {
            get { return (Brush)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }

        public double Hue
        {
            get => ((HsbColor)_color).PreciseHue;
            set
            {
                var color = (HsbColor)_color;
                color.PreciseHue = value;
                Color = color;
            }
        }

        public double Saturation
        {
            get => ((HsbColor)_color).PreciseSaturation;
            set
            {
                var color = (HsbColor)_color;
                color.PreciseSaturation = value;
                Color = color;
            }
        }

        public double Brightness
        {
            get => ((HsbColor)_color).PreciseBrightness;
            set
            {
                var color = (HsbColor)_color;
                color.PreciseBrightness = value;
                Color = color;
            }
        }

        private Color _color;

        public Color Color
        {
            get => _color;
            set
            {
                _color = value;
                // Input = new SolidColorBrush(_color);
            }
        }

        public static readonly DependencyProperty InputProperty =
            RegisterPixelShaderSamplerProperty("Input", typeof(ColorSwap), 0);
    }
}
