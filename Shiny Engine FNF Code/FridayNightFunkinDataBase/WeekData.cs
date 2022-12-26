using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;

namespace Shiny_Engine_FNF.Code.FridayNightFunkinDataBase
{
    [Serializable]
    class WeekFile
    {
        // JSON variables
        public JArray songs;
        public string[] weekCharacters;
        public string weekBackground;
        public string weekBefore;
        public string storyName;
        public string weekName;
        public int[] freeplayColor;
        public bool startUnlocked;
        public bool hideStoryMode;
        public bool hideFreeplay;
        public string difficulties;
    }


    class WeekData
    {
        public static Dictionary<string, WeekData> weeksLoaded = new();
        public static List<string> weeksList = new();
        public string folder = "";

        // JSON variables
        public JArray songs;
        public string[] weekCharacters;
        public string weekBackground;
        public string weekBefore;
        public string storyName;
        public string weekName;
        public int[] freeplayColor;
        public bool startUnlocked;
        public bool hideStoryMode;
        public bool hideFreeplay;
        public string difficulties;

        public static WeekFile createWeekFile()
        {
            var weekFile = new WeekFile()
            {
                songs = new JArray
                {
                    new JArray("Bopeebo", "dad", new int[]{ 146, 113, 253}),
                    new JArray("Fresh", "dad", new int[]{ 146, 113, 253}),
                    new JArray("Dad Battle", "dad", new int[]{ 146, 113, 253 })
                },
                weekCharacters = new string[] { "dad", "bf", "gf" },
                weekBackground = "stage",
                weekBefore = "tutorial",
                storyName = "Your New Week",
                weekName = "Custom Week",
                freeplayColor = new int[] { 146, 113, 253 },
                startUnlocked = true,
                hideStoryMode = false,
                hideFreeplay = false,
                difficulties = ""

            };
            return weekFile;
        }

        // HELP: Is there any way to convert a WeekFile to WeekData without having to put all variables there manually? I"m kind of a noob in haxe lmao
        public WeekData(WeekFile weekFile)
        {
            songs = weekFile.songs;
            weekCharacters = weekFile.weekCharacters;
            weekBackground = weekFile.weekBackground;
            weekBefore = weekFile.weekBefore;
            storyName = weekFile.storyName;
            weekName = weekFile.weekName;
            freeplayColor = weekFile.freeplayColor;
            startUnlocked = weekFile.startUnlocked;
            hideStoryMode = weekFile.hideStoryMode;
            hideFreeplay = weekFile.hideFreeplay;
            difficulties = weekFile.difficulties;
        }

        public static void reloadWeekFiles(bool isStoryMode = false)
        {
            weeksList.Clear();
            weeksLoaded.Clear();

            var disabledMods = new List<string>();
            var modsListPath = "modsList.txt";
            var directories = new List<string> { Paths.ModFolders(), Paths.GetPreloadPath() };
            var originalLength = directories.Count;
            if (File.Exists(modsListPath))
            {
                var stuff = File.ReadAllLines(modsListPath);
                for (int i = 0; i < stuff.Length; i++)
                {
                    var splitName = stuff[i].Trim().Split("|");
                    if (splitName[1] == "0") // Disable mod
                    {
                        disabledMods.Add(splitName[0]);
                    }
                    else // Sort mod loading order based on modsList.txt file
                    {
                        var path = Path.Combine(Paths.ModFolders(), splitName[0]);
                        //trace("trying to push: " + splitName[0]);
                        if (Directory.Exists(path) && !Paths.IgnoreModFolders.Contains(splitName[0]) && !disabledMods.Contains(splitName[0]) && !directories.Contains(path + "/"))
                        {
                            directories.Add(path + "\\");
                            //trace("pushed Directory: " + splitName[0]);
                        }
                    }
                }
            }

            string[] modsDirectories = Directory.GetDirectories(Paths.ModFolders());
            foreach (string folder in modsDirectories)
            {
                var pathThing = Path.Combine(Paths.ModFolders(), folder) + "/";
                if (!disabledMods.Contains(folder) && !directories.Contains(pathThing))
                {
                    directories.Add(pathThing);
                    //trace("pushed Directory: " + folder);
                }
            }

            string[] sexList = File.ReadAllLines(Paths.GetPreloadPath("weeks\\weekList.txt"));
            for (int i = 0; i < sexList.Length; i++)
            {
                for (int j = 0; j < directories.Count; j++)
                {
                    var fileToCheck = directories[j] + "weeks\\" + sexList[i] + ".json";
                    if (!weeksLoaded.ContainsKey(sexList[i]))
                    {
                        var week = getWeekFile(fileToCheck);
                        if (week != null)
                        {
                            var weekFile = new WeekData(week);

                            if (j >= originalLength)
                            {
                                weekFile.folder = directories[j].Substring(Paths.ModFolders().Length, directories[j].Length - 1);
                            }

                            if (weekFile != null && (isStoryMode && !weekFile.hideStoryMode || !isStoryMode && !weekFile.hideFreeplay))
                            {
                                weeksLoaded.Add(sexList[i], weekFile);
                                weeksList.Add(sexList[i]);
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < directories.Count; i++)
            {
                var directory = directories[i] + "weeks\\";
                if (File.Exists(directory))
                {
                    var listOfWeeks = File.ReadAllLines(directory + "weekList.txt");
                    foreach (var daWeek in listOfWeeks)
                    {
                        var path = directory + daWeek + ".json";
                        if (File.Exists(path))
                        {
                            addWeek(daWeek, path, directories[i], i, originalLength);
                        }
                    }

                    foreach (var file in Directory.GetDirectories(directory))
                    {
                        var path = Path.Combine(directory, file);
                        if (!Directory.Exists(path) && file.EndsWith(".json"))
                        {
                            addWeek(file.Substring(0, file.Length - 5), path, directories[i], i, originalLength);
                        }
                    }
                }
            }

        }

        private static void addWeek(string weekToCheck, string path, string directory, int i, int originalLength)
        {
            if (!weeksLoaded.ContainsKey(weekToCheck))
            {
                var week = getWeekFile(path);
                if (week != null)
                {
                    var weekFile = new WeekData(week);
                    if (i >= originalLength)
                    {
                        weekFile.folder = directory.Substring(Paths.ModFolders().Length, directory.Length - 1);
                    }
                    if (PlayState.isStoryMode && !weekFile.hideStoryMode || !PlayState.isStoryMode && !weekFile.hideFreeplay)
                    {
                        weeksLoaded[weekToCheck] = weekFile;
                        weeksList.Add(weekToCheck);
                    }
                }
            }
        }

        private static WeekFile getWeekFile(string path)
        {
            string rawJson = null;
            if (File.Exists(path))
            {
                rawJson = File.ReadAllText(path);
            }

            if (rawJson != null && rawJson.Length > 0)
            {
                return JsonConvert.DeserializeObject<WeekFile>(rawJson);
            }
            return null;
        }

        //   FUNCTIONS YOU WILL PROBABLY NEVER NEED TO USE

        //To use on PlayState.hx or Highscore stuff
        public static string getWeekFileName()
        {
            return weeksList[PlayState.storyWeek];
        }

        //Used on LoadingState, nothing really too relevant
        public static WeekData getCurrentWeek()
        {
            return weeksLoaded[weeksList[PlayState.storyWeek]];
        }

        public static void setDirectoryFromWeek(WeekData data = null)
        {
            Paths.CurrentModDirectory = "";
            if (data != null && data.folder != null && data.folder.Length > 0)
            {
                Paths.CurrentModDirectory = data.folder;
            }
        }
    }
}
