using System;
using System.Collections.Generic;
using System.Text;
using WpfGame.Controls;

namespace WpfGame.AnimationsSheet
{
    public class TextureAnimationSheet : AnimationSheet
    {
        internal AnimationSubTextureDataCollection _globalcollection;

        public static TextureAnimationSheet Create(string path,int rows, int colmons,Sprite sprite,double fps=30)
        {
            var texture = new TextureAnimationSheet(path, fps);
            sprite.Source = texture.ImageSource;
            sprite.Width = texture.ImageSource.PixelWidth;
            sprite.Height = texture.ImageSource.PixelHeight;

            sprite.Width /= colmons;
            sprite.Height /= rows;

            texture.FillTable(sprite,rows, colmons);
            texture.RawSetControl(sprite);
            sprite.RawSetFrame(texture);
            return texture;
        }

        public TextureAnimationSheet(string texture, double framerate = 30)
            : base(texture, framerate)
        {
            _globalcollection = new AnimationSubTextureDataCollection();
        }


        internal protected override void ReadAmimation()
        {
            // texture need to be empty
        }

        public void FillTable(Sprite control,int rows, int colmons)
        {
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < colmons; j++)
                {
                    _globalcollection.Add(new AnimationSubTextureData(
                        new System.Windows.Rect(j * control.Width, i * control.Height, control.Width, control.Height),
                        new System.Windows.Rect(0, 0, control.Width, control.Height), i + j));
                }
            }
        }
    }
}
