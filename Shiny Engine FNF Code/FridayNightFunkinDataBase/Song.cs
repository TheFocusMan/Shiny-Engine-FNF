using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shiny_Engine_FNF.Code;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Shiny_Engine_FNF.Code.FridayNightFunkinDataBase
{
    [Serializable]
    public class FNFEvent
    {
        public string name;
        public TimeSpan position;
        public object value;
        public string type;

        public FNFEvent(string name, TimeSpan pos, object value, string type)
        {
            this.name = name;
            position = pos;
            this.value = value;
            this.type = type;
        }
    }
    [Serializable]
    public class SwagSong
    {
        public string chartVersion;
        public string song;
        public SwagSection[] notes;
        public JArray events;
        public double bpm;
        public bool needsVoices;
        public double speed;

        public string player1;
        public string player2;
        public string player3; //deprecated, now replaced by gfVersion
        public string gfVersion;
        public string stage;

        public string arrowSkin;
        public string splashSkin;
        public bool validScore;
    }

    public class FNFSong
    {
        public string chartVersion;
        public string song;
        public SwagSection[] notes;
        public JArray events;
        public double bpm;
        public bool needsVoices = true;
        public string arrowSkin = "";
        public string splashSkin;
        public double speed = 1;
        public string stage;

        public string player1 = "bf";
        public string player2 = "dad";
        public string gfVersion = "";
        public string player3 = "gf"; //deprecated

        private static void ConvertToPsycEngineFormat(SwagSong songJson) // Convert old charts to newest format
        {
            if (songJson.gfVersion == null)
            {
                songJson.gfVersion = songJson.player3;
                songJson.player3 = null;
            }

            if (songJson.events == null)
            {
                songJson.events = new();
                for (var secNum = 0; secNum < songJson.notes.Length; secNum++)
                {
                    var sec = songJson.notes[secNum];

                    int i = 0;
                    var notes = sec.sectionNotes.ToList();
                    int len = notes.Count;
                    while (i < len)
                    {
                        var note = notes[i];
                        if ((double)note[1] < 0)
                        {
                            var list = songJson.events;
                            list.Add(new JArray(note[0], new JArray() { new JArray(note[2], note[3], note[4]) }));
                            notes.Remove(note);
                            songJson.events = list;
                            len = notes.Count;
                        }
                        else i++;
                    }
                    sec.sectionNotes = notes.ToArray();
                }
            }
        }

        public FNFSong(string song, SwagSection[] notes, double bpm)
        {
            this.song = song;
            this.notes = notes;
            this.bpm = bpm;
        }

        public static SwagSong LoadFromJsonRAW(string rawJson)
        {
            while (!rawJson.EndsWith("}"))
            {
                rawJson = rawJson[0..^1];
                // LOL GOING THROUGH THE BULLSHIT TO CLEAN IDK WHATS STRANGE
            }

            return ParseJSONshit(rawJson);
        }

        public static SwagSong LoadFromJson(string jsonInput, string folder = null)
        {
            // pre lowercasing the folder name
            var formattedFolder = Paths.FormatToSongPath(folder);
            var formattedSong = Paths.FormatToSongPath(jsonInput);
            string rawJson = null;

            var moddyFile = Paths.ModFolders(formattedFolder + "\\" + formattedSong);
            if (File.Exists(moddyFile))
            {
                rawJson = File.ReadAllText(moddyFile).Trim();
            }
            if (rawJson == null)
                rawJson = File.ReadAllText(Paths.Json(formattedFolder + "\\" + formattedSong)).Trim();

            while (!rawJson.EndsWith("}"))
            {
                rawJson = rawJson[0..^1];
                // LOL GOING THROUGH THE BULLSHIT TO CLEAN IDK WHATS STRANGE
            }

            // FIX THE CASTING ON WINDOWS/NATIVE
            // Windows???
            // trace(songData);

            // trace("LOADED FROM JSON: " + songData.notes);
            /* 
                for (i in 0...songData.notes.length)
                {
                    trace("LOADED FROM JSON: " + songData.notes[i].sectionNotes);
                    // songData.notes[i].sectionNotes = songData.notes[i].sectionNotes
                }

                    daNotes = songData.notes;
                    daSong = songData.song;
                    daBpm = songData.bpm; */
            SwagSong songJson = ParseJSONshit(rawJson);
            if (jsonInput != "events") StageData.loadDirectory(songJson);
            ConvertToPsycEngineFormat(songJson);
            return songJson;
        }

        public static SwagSong ParseJSONshit(string rawJson)
        {
            var swagShit = ((JObject)JsonConvert.DeserializeObject(rawJson))["song"].ToObject<SwagSong>();
            swagShit.validScore = true;
            return swagShit;
        }
    }
}
