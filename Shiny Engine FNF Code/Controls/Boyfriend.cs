namespace Shiny_Engine_FNF.Code.Controls
{
    public class Boyfriend : Character
    {
        public Boyfriend() : this("bf")
        {

        }

        protected override void Update()
        {
            if (GetAnimationName(Frames.CurrentAnimationName).StartsWith("sing"))
            {
                holdTimer += 1.0 / 24;
            }
            else holdTimer = 0;

            if (GetAnimationName(Frames.CurrentAnimationName).EndsWith("miss") && !Frames.IsPlayingAmimation)
            {
                PlayAnim("idle", true, false, 10);
            }

            if (GetAnimationName(Frames.CurrentAnimationName) == "firstDeath" && !Frames.IsPlayingAmimation)
            {
                PlayAnim("deathLoop");
            }
            base.Update();
        }

        public Boyfriend(string chart) : base(chart, true)
        {
            CurrentCharcter = chart;
        }
    }
}
