using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Windows;
using Shiny_Engine_FNF.Code;

namespace Shiny_Engine_FNF.Code.FridayNightFunkinDataBase
{
    class StageFile
    {
        public string directory;
        public double defaultZoom;
        public bool isPixelStage;

        public int[] boyfriend;
        public int[] girlfriend;
        public int[] opponent;

        public bool hide_girlfriend;

        public double[] camera_boyfriend;
        public double[] camera_opponent;
	    public double[] camera_girlfriend;
	    public double? camera_speed;
    }
    internal static class StageData
    {
        public static string forceNextDirectory = null;
        public static void loadDirectory(SwagSong SONG)
        {
            string stage = SONG.stage != null ? SONG.stage : SONG.song != null ? SONG.song.ToLower().Replace(" ", "-") switch
            {
                "spookeez" or "south" or "monster" => "spooky",
                "pico" or "blammed" or "philly" or "philly-nice" => "philly",
                "milf" or "satin-panties" or "high" => "limo",
                "cocoa" or "eggnog" => "mall",
                "winter-horrorland" => "mallEvil",
                "senpai" or "roses" => "school",
                "thorns" => "schoolEvil",
                _ => "stage",
            } : "stage";

            StageFile stageFile = GetStageFile(stage);
            forceNextDirectory = stageFile == null ? "" : stageFile.directory; 
        }

        public static StageFile GetStageFile(string stage)
        {
            string path = Paths.GetPreloadPath("stages\\" + stage + ".json");
            var modPath = Paths.ModFolders("stages\\" + stage + ".json");

            string rawJson = File.Exists(modPath) ? File.ReadAllText(modPath) : File.Exists(path) ? File.ReadAllText(path) : null;

            return JsonConvert.DeserializeObject<StageFile>(rawJson);
        }
    }
}
