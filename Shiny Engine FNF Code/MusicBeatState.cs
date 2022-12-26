using Shiny_Engine_FNF.Code.FridayNightFunkinDataBase;
using Shiny_Engine_FNF.Code.Properties;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using WpfGame;

namespace Shiny_Engine_FNF.Code
{
    public class MusicBeatState : State
    {
        protected double lastBeat = 0;
        protected double lastStep = 0;

        protected int CurStep { get; set; } = 0;
        protected int CurBeat { get; set; } = 0;

        public MusicBeatState()
        {

        }

        protected override void Update()
        {
            //everyStep();
            //PlayerSttings.Update();
            var oldStep = CurStep;

            UpdateCurStep();
            UpdateBeat();

            if (oldStep != CurStep && CurStep > 0)
                StepHit();

            base.Update();
        }

        private void UpdateBeat()
        {
            lastBeat = CurBeat;
            CurBeat = (int)Math.Floor(CurStep / 4.0);
        }

        private void UpdateCurStep()
        {
            var lastChange = new BPMChangeEvent()
            {
                stepTime = 0,
                songTime = TimeSpan.Zero,
                bpm = 0
            };
            for (int i = 0; i < Conductor.bpmChangeMap.Count; i++)
            {
                if (Conductor.songPosition >= Conductor.bpmChangeMap[i].songTime)
                    lastChange = Conductor.bpmChangeMap[i];
            }

            CurStep = lastChange.stepTime + (int)Math.Floor((Conductor.songPosition.TotalMilliseconds - Settings.Default.NoteOffset
                - lastChange.songTime.TotalMilliseconds) / Conductor.stepCrochet);
        }

        public virtual void StepHit()
        {
            if (CurStep % 4 == 0)
                BeatHit();
        }

        protected virtual void BeatHit()
        {

        }

        public static void SwitchToNullState(NavigationService service)
        {
            service.Navigate(null);
        }

        public static void SwichState(NavigationService service, Panel target, State state, bool doeffect = true)
        {
            SwichState(service, target, (object)state, doeffect);
        }

        public void SwichState(State state, bool doeffect = true)
        {
            SwichState(NavigationService, state.Content as Panel, (object)state, doeffect);
        }

        private static async void SwichState(NavigationService service, Panel target, object state, bool doeffect = true)
        {
            service.Navigate(state);
            if (doeffect)
            {
                DoEffectTransition(0.7, Colors.Black, target, false);
                await Task.Delay(700);
            }
            //NavigationService.RemoveBackEntry();
            service.RemoveBackEntry();
        }

        public void DoEffectTransition(double time, Color color, bool isflash = false, bool reversed = false, Action oncomplite = null)
        {
            DoEffectTransition(time, color, Content as Panel, isflash, reversed, oncomplite);
        }

        public static void DoEffectTransition(double time, Color color, Panel target = null, bool isflash = false, bool reversed = false, Action oncomplite = null)
        {
            var diamondc = new GraphicTransTileDiamond(isflash); // from psyc engine filters
            target.Children.Add(diamondc);
            target.InvalidateVisual();
            diamondc.Start(time, color, reversed);
            diamondc.Complited += (sender, e) =>
            {
                target.Children.Remove(diamondc);
                oncomplite?.Invoke();
            };
        }

        public static void ResetState(NavigationService navigation)
        {
            navigation.Navigate(navigation.CurrentSource);
        }
    }
}
