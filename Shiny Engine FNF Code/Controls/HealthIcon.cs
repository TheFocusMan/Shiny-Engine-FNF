using Shiny_Engine_FNF.Code;
using System.ComponentModel;
using System.IO;
using System.Windows;
using WpfGame.AnimationsSheet;
using WpfGame.Controls;

namespace Shiny_Engine_FNF.Code.Controls
{
    public class HealthIcon : Sprite
    {
        public Sprite sprTracker;
        private bool isOldIcon = false;
        private bool isPlayer = false;
        private string char1 = "";

        public HealthIcon() : this("bf", false)
        {

        }

        public HealthIcon(string char1, bool isPlayer = false) : base()
        {
            isOldIcon = char1 == "bf-old";
            this.isPlayer = isPlayer;
            if (!DesignerProperties.GetIsInDesignMode(this)) ChangeIcon(char1);
        }
        protected override void Update()
        {
            base.Update();
            if (sprTracker != null)
                Position = new Point(sprTracker.X + sprTracker.Width + 10, sprTracker.Y - 30);
        }

        public void swapOldIcon()
        {
            if (isOldIcon = !isOldIcon) ChangeIcon("bf-old");
            else ChangeIcon("bf");
        }

        private Point iconOffsets = new();
        public void ChangeIcon(string char1)
        {
            if (this.char1 != char1)
            {
                var name = "icons\\" + char1;
                if (!Paths.FileExists("images\\" + name + ".png")) name = "icons\\icon-" + char1; //Older versions of psych engine's support
                if (!Paths.FileExists("images\\" + name + ".png")) name = "icons\\icon-face"; //Prevents crash from missing icon
                var file = Paths.Image(name);
                var frames = new TextureAnimationSheet(file);
                Source = frames.ImageSource;
                FlipX = isPlayer;
                Width = 150;
                Height = 150;
                frames.RawSetControl(this);
                frames.Framerate = 1;
                frames.FillTable(this, 1, 2);
                frames.Add(char1, 0, 1);
                frames.Control = this;
                iconOffsets = new((Width - 150) / 2, (Width - 150) / 2);
                Position += new Vector(iconOffsets.X, iconOffsets.Y);
                Frames.PlayAnimation2(char1, false, false);
                Frames.CurrentFrame = 0;
                this.char1 = char1;
            }
        }

        public string getCharacter()
        {
            return char1;
        }
    }
}
