using Newtonsoft.Json;
using Shiny_Engine_FNF.Code;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using WpfGame.Controls;

namespace Shiny_Engine_FNF.Code.Controls
{
    class MenuCharacterFile
    {
        public string image = "";
        public float scale = 0;
        public int[] position = Array.Empty<int>();
        public string idle_anim = "";
        public string confirm_anim = "";
    }

    class MenuCharacter : Sprite
    {

        //questionable variable name lmfao
        private string character = "";
        readonly ScaleTransform _transform;
        private const string DEFAULT_CHARACTER = "bf";

        public MenuCharacter(double scale, bool flipped) : this()
        {
            Flipped = flipped;
            Scale = scale;
        }

        public MenuCharacter() : base()
        {
            _transform = (RenderTransform as TransformGroup).Children[0] as ScaleTransform;
        }
        public new double Scale
        {
            get => _transform.ScaleX;
            set
            {
                var isval = Convert.ToInt32(Flipped) == 0 ? -1 : 1;
                _transform.ScaleX = -isval * value;
                _transform.ScaleY = value;
            }
        }

        public bool Flipped
        {
            get => _flipped;
            set
            {
                _flipped = value;
                Scale = _transform.ScaleX; //update scale
            }
        }
        private bool _flipped;

        public void BopHead()
        {
            if (string.IsNullOrEmpty(character)) return;
            Frames.PlayAnimation2("idle", false, false);
            /*if (character == "gf" || character == "spooky")
            {
                danceLeft = !danceLeft;
                _sheet.SortAnimation(_sheet.AnimationChase[character], danceLeft ? _sortanimations[character + "-left"] : _sortanimations[character + "-right"]);
                _sheet.PlayAnimation(_sheet.AnimationChase[character]);
            }
            else
            {
                //no spooky nor girlfriend so we do da normal animation
                if (_sheet.CurrentAnimationName == "bfConfirm")
                    return;
                _sheet.PlayAnimation(_sheet.AnimationChase[character]);
            }
            if (LastFrame)
            {
                //   animation.finish();
            }*/
        }

        public void ChangeCharacter(string character = "bf")
        {
            if (character == null) character = "";
            if (character == this.character) return;

            this.character = character;
            Visibility = Visibility.Visible;
            Scale = 1;

            switch (character)
            {
                case "":
                    Visibility = Visibility.Hidden;
                    break;
                default:
                    string characterPath = "images\\menucharacters\\" + character + ".json";
                    string path = "mods\\" + characterPath;
                    if (!File.Exists(path))
                    {
                        path = Paths.GetPreloadPath(characterPath);
                    }

                    if (!File.Exists(path))
                    {
                        path = Paths.GetPreloadPath("images\\menucharacters\\" + DEFAULT_CHARACTER + ".json");
                    }
                    string rawJson = File.ReadAllText(path);
                    var charFile = JsonConvert.DeserializeObject<MenuCharacterFile>(rawJson);
                    Frames = Paths.GetSparrowAtlas("menucharacters\\" + charFile.image, this, null, 24);
                    Frames.AnimationChase.Add("idle", charFile.idle_anim);
                    Frames.AnimationChase.Add("confirm", charFile.confirm_anim);

                    if (charFile.scale != 1)
                    {
                        Scale = charFile.scale;
                    }
                    X -= charFile.position[0];
                    Y -= charFile.position[1];
                    Frames.PlayAnimation2("idle", false, false);
                    break;
            }
        }
    }
}
