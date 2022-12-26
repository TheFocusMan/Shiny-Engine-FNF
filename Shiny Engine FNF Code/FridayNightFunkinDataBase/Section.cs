using Newtonsoft.Json.Linq;
using System;

namespace Shiny_Engine_FNF.Code.FridayNightFunkinDataBase
{
    [Serializable]
    public class SwagSection
    {
        public JArray[] sectionNotes;
        public int lengthInSteps;
        public int typeOfSection;
        public bool mustHitSection;
        public bool gfSection;
        public double bpm;
        public bool changeBPM;
        public bool altAnim;
    }

    class Section
    {
        public double[][] sectionNotes = Array.Empty<double[]>();

        public int lengthInSteps = 16;
        public int typeOfSection = 0;
        public bool mustHitSection = true;
        public bool gfSection = false;

        /**
         *	Copies the first section into the second section!
         */
        public static int COPYCAT = 0;

        public Section(int lengthInSteps = 16)
        {
            this.lengthInSteps = lengthInSteps;
        }
    }
}
