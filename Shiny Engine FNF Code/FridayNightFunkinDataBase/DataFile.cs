using Shiny_Engine_FNF.Code.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WpfGame;

namespace Shiny_Engine_FNF.Code.FridayNightFunkinDataBase
{
    [Serializable]
    class DataFile
    {
        public Dictionary<string, bool> weekCompleted = new Dictionary<string, bool>();

        public static DataFile Data;

        public Dictionary<string, int> SongsScores = new Dictionary<string, int>();

        public Dictionary<string, string> SongsCombos = new Dictionary<string, string>();

        public Dictionary<string, int> WeekScores = new Dictionary<string, int>();

        public Dictionary<string, double> SongRating = new Dictionary<string, double>();

        public HsbColor[] arrowHSV = new HsbColor[4];

        static DataFile()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ninjamuffin99", StatePreloader.ModName, "ninjamuffin99", "funkin.sol");
            Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ninjamuffin99", StatePreloader.ModName, "ninjamuffin99"));
            BinaryFormatter binder = new();
            if (Settings.Default.ComboOffset == null)
            {
                Settings.Default.ComboOffset = new int[4];
                Settings.Default.Save();
            }
            using FileStream file = new(path, FileMode.OpenOrCreate);
            Data = new DataFile();
            try
            {
                if (!File.Exists(path)) binder.Serialize(file, Data);
                else
                {
                    Data = (DataFile)binder.Deserialize(file);
                }
            }
            catch
            {
                Trace.WriteLine("File not loaded");
                binder.Serialize(file, Data);
            }
        }

        public void Save()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ninjamuffin99", StatePreloader.ModName, "ninjamuffin99", "funkin.sol");
            BinaryFormatter binder = new();
            using FileStream file = new(path, FileMode.OpenOrCreate);
            binder.Serialize(file, Data);
        }
    }
}
