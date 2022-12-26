using Shiny_Engine_FNF.Code.FridayNightFunkinDataBase;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using WpfGame;
using WpfGame.AnimationsSheet;
using WpfGame.Controls;
using FileSystem = System.IO.File;

namespace Shiny_Engine_FNF.Code
{
    static class Paths
    {
        public const string SOUND_EXT = "ogg";
        static string _currentLevel;

        public static string[] IgnoreModFolders = Array.Empty<string>();

        public static string CurrentModDirectory = "";
        public static void SetCurrentLevel(string name) => _currentLevel = name.ToLower();


        public static bool FileExists(string key, string libary = null)
        {
            if (FileSystem.Exists(ModFolders(CurrentModDirectory + '\\' + key)) || FileSystem.Exists(ModFolders(key)))
                return true;
            if (FileSystem.Exists(GetPath(key, libary)))
                return true;
            return false;
        }
        public static string GetPath(string File, string library)
        {
            if (library != null)
                return GetLibraryPath(File, library);

            if (_currentLevel != null)
            {
                var levelPath = GetLibraryPathForce(File, _currentLevel);
                if (FileSystem.Exists(levelPath))
                    return levelPath;

                levelPath = GetLibraryPathForce(File, "shared");
                if (FileSystem.Exists(levelPath))
                    return levelPath;
            }

            return GetPreloadPath(File);
        }

        public static string GetLibraryPath(string File, string library = "preload") =>
            library == "preload" || library == "default" ? GetPreloadPath(File) : GetLibraryPathForce(File, library);

        public static string GetLibraryPathForce(string File, string library) =>
            ReturnCurrentFolder($"assets\\{library}\\{File}");

        public static string GetPreloadPath(string File) =>
            ReturnCurrentFolder($"assets\\{File}");

        public static string GetPreloadPath() =>
            ReturnCurrentFolder("assets\\");

        private static string ReturnCurrentFolder(string key)
        {
            if (BrowserInteropHelper.IsBrowserHosted)
                return BrowserInteropHelper.Source.Host + key.Replace("\\", "/");
            else return AppDomain.CurrentDomain.BaseDirectory + key;
        }

        public static string File(string File, string library = null) => GetPath(File, library);

        public static string Lua(string key, string library = null) => GetPath($"data\\{key}.lua", library);

        public static string LuaImage(string key, string library = null) => GetPath($"data\\{key}.png", library);

        public static string Txt(string key, string library = null) => GetPath($"data\\{key}.txt", library);

        public static string Xml(string key, string library = null) => GetPath($"data\\{key}.xml", library);

        public static string Json(string key, string library = null) => GetPath($"data\\{key}.json", library);

        public static string SoundRandom(string key, int min, int max, string library = null)
        {
            Random rnd = new();
            return Sound(key + rnd.Next(min, max), library);
        }

        public static string Music(string key, string library = null) => ReturnSound("music", key, library);

        public static string Voices(string song)
        {
            return ReturnSound("songs", $"{song.ToLower().Replace(' ', '-')}\\Voices");
        }

        public static string FormatToSongPath(string path)
        {
            return path.ToLower().Replace(" ", "-");
        }

        public static string ReturnSound(string path, string key, string library = null)
        {
            var file = ModFolders(path + "\\" + key + "." + SOUND_EXT);
            if (FileSystem.Exists(file))
            {
                // localTrackedAssets.push(key);
                return file;
            }
            // I hate this so god damn much
            var gottenPath = GetPath($"{path}\\{key}.{SOUND_EXT}", library);
            // gottenPath = gottenPath[(gottenPath.IndexOf(':') + 1)..];
            // trace(gottenPath);
            return gottenPath;
        }
        static public string Sound(string key, string library = null)
        {
            var sound = ReturnSound("sounds", key, library);
            return sound;
        }

        public static string Inst(string song)
        {
            return ReturnSound("songs", $"{song.ToLower().Replace(' ', '-')}\\Inst");
        }

        public static string Image(string key, string library = null) => ReturnGraphic(key, library);

        public static string Font(string key) => GetPreloadPath($"fonts\\{key}");

        public static string ModFolders(string key)
        {
            if (CurrentModDirectory != null && CurrentModDirectory.Length > 0)
            {
                var fileToCheck = "mods\\" + CurrentModDirectory + "\\" + key;
                if (FileSystem.Exists(fileToCheck))
                {
                    return fileToCheck;
                }
            }
            return ReturnCurrentFolder("mods\\" + key);
        }

        public static string ModsJson(string key)
        {
            return ModFolders("data\\" + key + ".json");
        }
        public static string ModFolders()
        {
            return ReturnCurrentFolder("mods\\");
        }

        public static void SetCurrentLevelForWeek()
        {
            var directory = "shared";
            var weekDir = StageData.forceNextDirectory;
            StageData.forceNextDirectory = null;

            if (weekDir != null && weekDir.Length > 0 && weekDir != "") directory = weekDir;

            SetCurrentLevel(directory);
        }
        public static Sparrow2AnimationSheet GetSparrowAtlas(string key, Sprite control, string library = null, double framerate = 30)
        {
            var file = Image(key, library);
            if (file == null) return null;
            var xmlExists = FileSystem.Exists(ModFolders($"images\\{key}.xml"));
            return new Sparrow2AnimationSheet(file, xmlExists ? ModFolders($"images\\{key}.xml") : File($"images\\{key}.xml", library), framerate) { Control = control };
        }

        static public PackerAnimationSheet GetPackerAtlas(string key, Sprite control, string library = null, double framerate = 24)
        {
            var imageLoaded = ReturnGraphic(key);
            var txtExists = FileSystem.Exists(ModFolders($"images\\{key}.txt"));

            return new PackerAnimationSheet(imageLoaded ?? Image(key, library),
                txtExists ? ModFolders($"images\\{key}.txt") : File($"images\\{key}.txt", library), framerate)
            { Control = control };
        }

        public static string ReturnGraphic(string key, string library = null)
        {
            if (FileSystem.Exists(ModFolders($"images\\{key}.png")))
                return ModFolders($"images\\{key}.png");
            var path = GetPath($"images\\{key}.png", library);
            if (FileSystem.Exists(path))
                return path;
            Trace.WriteLine("oh no its returning null NOOOO");
            return null;
        }
        public static void DisposeAll()
        {
            WpfGame.Sound.StopAll();
            CacheKiller.AggresseveDispose();

            for (int i = 0; i < 15; i++)
            {
                RemoveUnusedMemory();
                GC.WaitForPendingFinalizers();
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
            }
            GC.WaitForPendingFinalizers();
        }

        public static void RemoveUnusedMemory()
        {
            Marshal.CleanupUnusedObjectsInCurrentContext();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
        }

        public static BitmapSource ImageCached(string key)
        {
            var data = new BitmapImage(new Uri(key));
            Debug.WriteLine($"finding {key} - {data}");
            return data;
        }
    }
}
