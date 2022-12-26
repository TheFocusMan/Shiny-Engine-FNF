using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows;
using Shiny_Engine_FNF.Code;

namespace Shiny_Engine_FNF.Code.Controls
{
    class MenuItem : Grid
    {
        public Image week;
        public int flashingInt = 0;
        private string _weeknum;

        public MenuItem(string weekName = "") : this()
        {
            WeekName = weekName;
        }

        public MenuItem()
        {
            HorizontalAlignment = HorizontalAlignment.Center;
            week = new Image();
            Children.Add(week);
        }

        public string WeekName
        {
            get => _weeknum;
            set
            {
                _weeknum = value;
                week.Source = new BitmapImage(new Uri(Paths.Image("storymenu\\" + _weeknum)));
                week.Height = ((BitmapSource)week.Source).PixelHeight;
                week.Width = ((BitmapSource)week.Source).PixelWidth;
                week.Stretch = System.Windows.Media.Stretch.UniformToFill;
                Height = week.Height + 10;
            }
        }

        // if it runs at 60fps, fake framerate will be 6
        // if it runs at 144 fps, fake framerate will be like 14, and will update the graphic every 0.016666 * 3 seconds still???
        // so it runs basically every so many seconds, not dependant on framerate??
        // I'm still learning how math works thanks whoever is reading this lol

        // In This Engine has Infinity FPS lol

        //var fakeFramerate:Int = Math.round((1 / FlxG.elapsed) / 10);

        /*override function update(elapsed:Float)
        {
            super.update(elapsed);
            y = FlxMath.lerp(y, (targetY * 120) + 480, 0.17 * (60 / FlxG.save.data.fpsCap));

            if (isFlashing)
                flashingInt += 1;

            if (flashingInt % fakeFramerate >= Math.floor(fakeFramerate / 2))
                week.color = 0xFF33ffff;
            else if (FlxG.save.data.flashing)
                week.color = FlxColor.WHITE;
        }*/
    }
}
