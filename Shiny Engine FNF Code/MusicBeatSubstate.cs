using Shiny_Engine_FNF.Code.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using Shiny_Engine_FNF.Code.FridayNightFunkinDataBase;
using WpfGame;

namespace Shiny_Engine_FNF.Code
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Shiny_Engine_FNF"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Shiny_Engine_FNF;assembly=Shiny_Engine_FNF"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:MusicBeatSubstate/>
    ///
    /// </summary>
    public class MusicBeatSubstate : SubState
    {
        protected double lastBeat = 0;
        protected double lastStep = 0;

        protected int CurStep { get; set; } = 0;
        protected int CurBeat { get; set; } = 0;

        public MusicBeatSubstate()
        {

        }

        protected override void Update()
        {
            //everyStep();
            var oldStep = CurStep;

            UpdateCurStep();

            if (oldStep != CurStep && CurStep > 0)
                StepHit();

            base.Update();
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

            CurStep = (int)(lastChange.stepTime + Math.Floor((Conductor.songPosition.TotalMilliseconds - lastChange.songTime.TotalMilliseconds) / Conductor.stepCrochet));
        }

        public virtual void StepHit()
        {
            if (CurStep % 4 == 0)
                BeatHit();
        }

        protected virtual void BeatHit()
        {

        }
    }
}
