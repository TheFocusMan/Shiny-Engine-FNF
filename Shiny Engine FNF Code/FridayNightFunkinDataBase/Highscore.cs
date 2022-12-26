using Shiny_Engine_FNF.Code;
using System;
using System.Collections.Generic;

namespace Shiny_Engine_FNF.Code.FridayNightFunkinDataBase
{
    class Highscore
    {
        public static Dictionary<string, int> weekScores = new();
        public static Dictionary<string, int> songScores = new();
        public static Dictionary<string, double> songRating = new();


        public static void ResetSong(string song, int diff = 0)
        {
            var daSong = FormatSong(song, diff);
            SetScore(daSong, 0);
            SetRating(daSong, 0);
        }

        public static void ResetWeek(string week, int diff = 0)
        {
            var daWeek = FormatSong(week, diff);
            SetWeekScore(daWeek, 0);
        }

        public static double FloorDecimal(double value, int decimals)
        {
            if (decimals < 1)
                return Math.Floor(value);

            var tempMult = 1;
            for (int i = 0; i < decimals; i++) tempMult *= 10;
            var newValue = Math.Floor(value * tempMult);
            return newValue / tempMult;
        }

        public static void SaveScore(string song, int score = 0, int diff = 0, double rating = -1)
        {
            var daSong = FormatSong(song, diff);

            if (songScores.ContainsKey(daSong))
            {
                if (songScores[daSong] < score)
                {
                    SetScore(daSong, score);
                    if (rating >= 0) SetRating(daSong, rating);
                }
            }
            else
            {
                SetScore(daSong, score);
                if (rating >= 0) SetRating(daSong, rating);
            }
        }

        public static void SaveWeekScore(string week, int score = 0, int diff = 0)
        {
            var daWeek = FormatSong(week, diff);

            if (weekScores.ContainsKey(daWeek))
            {
                if (weekScores[daWeek] < score)
                    SetWeekScore(daWeek, score);
            }
            else
                SetWeekScore(daWeek, score);
        }

        /**
         * YOU SHOULD FORMAT SONG WITH formatSong() BEFORE TOSSING IN SONG VARIABLE
         */
        static void SetScore(string song, int score)
        {
            // Reminder that I don't need to format this song, it should come formatted!
            if (songScores.ContainsKey(song))
                songScores[song] = score;
            else songScores.Add(song, score);
            DataFile.Data.SongsScores = songScores;
            DataFile.Data.Save();
        }
        static void SetWeekScore(string week, int score)
        {
            // Reminder that I don't need to format this song, it should come formatted!
            if (weekScores.ContainsKey(week))
                weekScores[week] = score;
            else weekScores.Add(week, score);
            DataFile.Data.WeekScores = weekScores;
            DataFile.Data.Save();
        }

        static void SetRating(string song, double rating)
        {
            // Reminder that I don't need to format this song, it should come formatted!
            if (songRating.ContainsKey(song))
                songRating[song] = rating;
            else songRating.Add(song, rating);
            DataFile.Data.SongRating = songRating;
            DataFile.Data.Save();
        }

        public static string FormatSong(string song, int diff)
        {
            return Paths.FormatToSongPath(song) + CoolUtil.GetDifficultyFilePath(diff);
        }

        public static int GetScore(string song, int diff)
        {
            var daSong = FormatSong(song, diff);
            if (!songScores.ContainsKey(daSong))
                SetScore(daSong, 0);

            return songScores[daSong];
        }

        public static double GetRating(string song, int diff)
        {
            var daSong = FormatSong(song, diff);
            if (!songRating.ContainsKey(daSong))
                SetRating(daSong, 0);

            return songRating[daSong];
        }

        public static int GetWeekScore(string week, int diff)
        {
            var daWeek = FormatSong(week, diff);
            if (!weekScores.ContainsKey(daWeek))
                SetWeekScore(daWeek, 0);

            return weekScores[daWeek];
        }

        static Highscore()
        {
            if (DataFile.Data.WeekScores != null)
                weekScores = DataFile.Data.WeekScores;
            if (DataFile.Data.SongsScores != null)
                songScores = DataFile.Data.SongsScores;
            if (DataFile.Data.SongRating != null)
                songRating = DataFile.Data.SongRating;
        }
    }

    [Serializable]
    public class SongHighscore
    {
        public int _score = 0;
        public int _combo = 0;
        public int _misses = 0;
        public double _accuracy = 0;
    }
}
