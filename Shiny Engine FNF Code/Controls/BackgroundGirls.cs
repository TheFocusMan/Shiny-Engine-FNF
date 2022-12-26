using Shiny_Engine_FNF.Code;
using System;
using System.Linq;
using WpfGame.Controls;

namespace Shiny_Engine_FNF.Code.Controls
{
    internal class BackgroundGirls : Sprite
    {
        bool isPissed = true;
        public BackgroundGirls(double x, double y)
        {
            X = x;
            Y = y;


            // BG fangirls dissuaded
            Frames = Paths.GetSparrowAtlas("weeb\\bgFreaks", this);

            SwapDanceType();

            Frames.PlayAnimation2("danceLeft");
        }

        protected override void Update()
        {
            RefreshPosition();
            base.Update();
        }

        bool danceDir = false;

        public void SwapDanceType()
        {
            isPissed = !isPissed;
            if (Frames != null)
            {
                Frames.AnimationChase.Clear();
                Frames.AnimationSortChase.Clear(); // dont throw expect
                if (!isPissed)
                {
                    //Gets unpissed
                    Frames.AddByIndices("danceLeft", "BG girls group", Enumerable.Range(0, 14).ToArray());
                    Frames.AddByIndices("danceRight", "BG girls group", Enumerable.Range(15, 15).ToArray());
                }
                else
                { //Pisses
                    Frames.AddByIndices("danceLeft", "BG fangirls dissuaded", Enumerable.Range(0, 14).ToArray());
                    Frames.AddByIndices("danceRight", "BG fangirls dissuaded", Enumerable.Range(15, 15).ToArray());
                }
                Dance();
            }
        }

        public void Dance()
        {
            danceDir = !danceDir;

            if (danceDir)
                Frames.PlayAnimation2("danceRight", true, false);
            else
                Frames.PlayAnimation2("danceLeft", true, false);
        }
    }
}
