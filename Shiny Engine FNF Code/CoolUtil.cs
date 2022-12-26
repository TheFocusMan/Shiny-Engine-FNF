namespace Shiny_Engine_FNF.Code
{
    static class CoolUtil
    {
        public static string[] defaultDifficulties = new string[] { "Easy", "Normal", "Hard" };

        public static string defaultDifficulty = "Normal"; //The chart that has no suffix and starting difficulty on Freeplay/Story Mode

        public static string DifficultyFromInt(int difficulty)
        {
            return defaultDifficulties[difficulty];
        }

        public static string GetDifficultyFilePath(int num = -1)
        {
            if (num == -1) num = PlayState.storyDifficulty;
            var fileSuffix = defaultDifficulties[num];
            if (fileSuffix != defaultDifficulty)
            {
                fileSuffix = '-' + fileSuffix;
            }
            else
            {
                fileSuffix = "";
            }
            return fileSuffix.ToLower().Replace(' ', '-');
        }
    }
}
