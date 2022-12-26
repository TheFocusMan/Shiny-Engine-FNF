using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows;

namespace WpfGame.AnimationsSheet
{
    public class PackerAnimationSheet : AnimationSheet
    {
        string _txt;
        public PackerAnimationSheet(string texture, string txt, double framerate = 30)
            : base(texture, framerate)
        {
            _txt = txt;
            BeginInit();
            EndInit();
        }

        internal protected override void ReadAmimation()
        {
            using var reader = new StreamReader(_txt);
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var slited = line.Split('=');
                var name = slited[0].Trim();
                var namesplit = name.Split("_");
                int num = int.Parse(namesplit[1]);
                name = namesplit[0];
                var values = slited[1].Trim().Split(' ');
                if (!_rect.ContainsKey(name))
                    _rect.Add(name, new AnimationSubTextureDataCollection());
                _rect[name].Add(new AnimationSubTextureData(
                    new Rect(double.Parse(values[0]), double.Parse(values[1]), double.Parse(values[2]), double.Parse(values[3])),
                    new Rect(0,0, double.Parse(values[2]), double.Parse(values[3])), num));
            }
        }
    }
}
