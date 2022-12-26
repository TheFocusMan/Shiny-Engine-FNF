using Shiny_Engine_FNF.Code;
using WpfGame.Controls;

namespace Shiny_Engine_FNF.Code.Controls
{
    internal class BackgroundDancer : Sprite
    {
        public BackgroundDancer(double x, double y)
        {
            X = x;
            Y = y;

            Frames = Paths.GetSparrowAtlas("limo\\limoDancer", this, null, 24);
            Frames.AddByIndices("danceLeft", "bg dancer sketch PINK", new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 });
            Frames.AddByIndices("danceRight", "bg dancer sketch PINK", new int[] { 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29 });
            Frames.PlayAnimation2("danceLeft", true, false);
        }

        bool danceDir = false;

        public void Dance()
        {
            danceDir = !danceDir;

            if (danceDir)
                Frames.PlayAnimation2("danceRight", false, false);
            else
                Frames.PlayAnimation2("danceLeft", false, false);
        }
    }
}
