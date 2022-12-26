using Shiny_Engine_FNF.Code.Controls;
using Shiny_Engine_FNF.Code.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Shiny_Engine_FNF.Code.FridayNightFunkinDataBase
{
    struct BPMChangeEvent
    {
        public int stepTime;
        public TimeSpan songTime;
        public double bpm;
    }
    class Conductor
    {
        public static double bpm = 100;
        public static double crochet = 60.0 / bpm * 1000; // beats in milliseconds
        public static double stepCrochet = crochet / 4; // steps in milliseconds
        public static TimeSpan songPosition;
        public static TimeSpan lastSongPos = TimeSpan.Zero;
        public static double offset = 0;

        //public static int safeFrames = 10;
        public static double safeZoneOffset = Math.Floor(Settings.Default.SafeFrames / 60.0 * 1000.0); // is calculated in create(), is safeFrames in milliseconds
        public static List<BPMChangeEvent> bpmChangeMap = new();


        public static void MapBPMChanges(SwagSong song)
        {
            bpmChangeMap = new List<BPMChangeEvent>();

            double curBPM = song.bpm;
            int totalSteps = 0;
            double totalPos = 0;
            for (int i = 0; i < song.notes.Length; i++)
            {
                if (song.notes[i].changeBPM && song.notes[i].bpm != curBPM)
                {
                    curBPM = song.notes[i].bpm;
                    BPMChangeEvent j = new()
                    {
                        stepTime = totalSteps,
                        songTime = TimeSpan.FromSeconds(totalPos),
                        bpm = curBPM
                    };
                    bpmChangeMap.Add(j);
                }

                int deltaSteps = song.notes[i].lengthInSteps;
                totalSteps += deltaSteps;
                totalPos += 60 / curBPM * 1000 / 4 * deltaSteps;
            }
            Debug.WriteLine("new BPM map BUDDY " + bpmChangeMap);
        }

        /* public static void RecalculateTimingStruct(Song SONG)
         {
             foreach (var i in SONG.eventObjects)
             {
                 /*TimingStruct.addTiming(beat,bpm,endBeat, Std.parseFloat(OFFSET));

                 if (changeEvents.length != 0)
                 {
                     var data = TimingStruct.AllTimings[currentIndex - 1];
                     data.endBeat = beat;
                     data.length = (data.endBeat - data.startBeat) / (data.bpm / 60);
                     TimingStruct.AllTimings[currentIndex].startTime = data.startTime + data.length;
                 }
             }
         }*/

        public static string JudgeNote(Note note, double diff = 0) //STOLEN FROM KADE ENGINE (bbpanzu) - I had to rewrite it later anyway after i added the custom hit windows lmao (Shadow Mario)
        {
            //tryna do MS based judgment due to popular demand
            int[] timingWindows = new int[] { Settings.Default.SickWindow, Settings.Default.GoodWindow, Settings.Default.BadWindow };
            var windowNames = new string[] { "sick", "good", "bad" };

            // var diff = Math.abs(note.strumTime - Conductor.songPosition) / (PlayState.songMultiplier >= 1 ? PlayState.songMultiplier : 1);
            for (int i = 0; i < timingWindows.Length; i++) // based on 4 timing windows, will break with anything else
            {
                if (diff <= timingWindows[Math.Min(i, timingWindows.Length - 1)])
                {
                    return windowNames[i];
                }
            }
            return "shit";
        }

        public static void ChangeBPM(double newBpm)
        {
            bpm = newBpm;

            crochet = 60.0 / bpm * 1000;
            stepCrochet = crochet / 4;
        }
    }
}
