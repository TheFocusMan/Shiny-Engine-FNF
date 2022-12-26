using Shiny_Engine_FNF.Code;
using Shiny_Engine_FNF.Code.Controls;
using Shiny_Engine_FNF.Code.FridayNightFunkinDataBase;
using Shiny_Engine_FNF.Code.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using WpfGame;
using WpfGame.Controls;

namespace Shiny_Engine_FNF.Code
{
    public partial class PlayState
    {
        void SpawnNoteSplashOnNote(Note note)
        {
            if (Settings.Default.NoteSplashes && note != null)
            {
                var strum = playerStrums[note.noteData];
                if (strum != null)
                {
                    SpawnNoteSplash(strum.X, strum.Y, note.noteData, note);
                }
            }
        }

        public void SpawnNoteSplash(double x, double y, int data, Note note = null)
        {
            var skin = "noteSplashes";
            if (PlayState.SONG.splashSkin != null && PlayState.SONG.splashSkin.Length > 0) skin = PlayState.SONG.splashSkin;

            var hue = DataFile.Data.arrowHSV[data % 4].PreciseHue / 360;
            var sat = DataFile.Data.arrowHSV[data % 4].PreciseSaturation / 100;
            var brt = DataFile.Data.arrowHSV[data % 4].PreciseBrightness / 100;
            if (note != null)
            {
                skin = note.noteSplashTexture;
                hue = note.noteSplashColor.PreciseHue;
                sat = note.noteSplashColor.PreciseSaturation;
                brt = note.noteSplashColor.PreciseBrightness;
            }

            NoteSplash splash = new(100, 100, 0);
            grpNoteSplashes.Children.Add(splash);
            splash.SetupNoteSplash(x, y, data, skin, hue, sat, brt);
        }

        void NoteMiss(Note daNote)
        {
#if !DEBUG
            //You didn"t hit the key and let it go offscreen, also used by Hurt Notes
          //Dupe note remove
            foreach (Note note in notes.Children.OfType<Note>().ToArray())
            {
                if (daNote != note && daNote.mustPress && daNote.noteData == note.noteData && daNote.isSustainNote == note.isSustainNote && Math.Abs(daNote.strumTime - note.strumTime) < 1)
                    notes.Children.Remove(note);
            }
            combo = 0;

            health -= daNote.missHealth * healthLoss;
            if (instakillOnMiss)
            {
                ChangeVocalVolume(0);
                DoDeathCheck(true);
            }

            //For testing purposes
            //trace(daNote.missHealth);
            songMisses++;
            if (vocals != null) ChangeVocalVolume(0);
            if (!practiceMode) songScore -= 10;

            totalPlayed++;
            RecalculateRating();

            Character char1 = boyfriend;
            if (daNote.gfNote) char1 = gf;

            if (char1.hasMissAnimations)
            {
                var daAlt = "";
                if (daNote.NoteType == "Alt Animation") daAlt = "-alt";

                var animToPlay = singAnimations[Math.Abs(daNote.noteData)] + "miss" + daAlt;
                char1.PlayAnim(animToPlay, true);
            }

            CallOnLuas("noteMiss", notes.Children.IndexOf(daNote), daNote.noteData, daNote.NoteType, daNote.isSustainNote);
#endif
        }

        void StrumPlayAnim(bool isDad, int id, double time)
        {
            StrumNote spr;
            if (isDad)
                spr = (StrumNote)strumLineNotes.Children[id];
            else
                spr = playerStrums[id];

            if (spr != null)
            {
                spr.PlayAnim("confirm", true);
                spr.resetAnim = time;
            }
        }

        private void PopUpScore(Note note = null)
        {
            var noteDiff = Math.Abs(note.strumTime - Conductor.songPosition.TotalMilliseconds + Settings.Default.RatingOffset) - StaticTimer.DeltaSaconds * 1300;
            //trace(noteDiff, " " + Math.abs(note.strumTime - Conductor.songPosition));

            // boyfriend.playAnim("hey");
            ChangeVocalVolume(1);

            var placement = combo.ToString();

            OutlinedTextBlock coolText = new() { Text = placement, FontSize = 32 };
            coolText.ScreenCenter();
            coolText.SetPosition(Display.DefaultWidth * 0.35, 0);
            //

            Sprite rating = new();
            var score = 350;

            //tryna do MS based judgment due to popular demand
            var daRating = Conductor.JudgeNote(note, noteDiff);

            switch (daRating)
            {
                case "shit": // shit
                    totalNotesHit += 0;
                    score = 50;
                    shits++;
                    break;
                case "bad": // bad
                    totalNotesHit += 0.5;
                    score = 100;
                    bads++;
                    break;
                case "good": // good
                    totalNotesHit += 0.75;
                    score = 200;
                    goods++;
                    break;
                case "sick": // sick
                    totalNotesHit += 1;
                    sicks++;
                    break;
            }


            if (daRating == "sick" && !note.noteSplashDisabled)
            {
                SpawnNoteSplashOnNote(note);
            }

            if (!practiceMode && !cpuControlled)
            {
                songScore += score;
                songHits++;
                totalPlayed++;
                RecalculateRating();

                if (Settings.Default.ScoreZoom)
                {
                    if (scoreTxtTween != null)
                        scoreTxtTween.Cancel();
                    scoreTxt.SetZoom(1.075);

                    scoreTxtTween = new Tween(scoreTxt.GetZoom(), 1, new CubicEase(), 0.2);
                    scoreTxtTween.UpdateValue += (s, e) => scoreTxt.SetZoom(e);
                    scoreTxtTween.Complite += (s, e) => scoreTxtTween = null;
                    scoreTxtTween.Start();
                }
            }

            /* if (combo > 60)
                    daRating = "sick";
                else if (combo > 12)
                    daRating = "good"
                else if (combo > 4)
                    daRating = "bad";
             */

            var pixelShitPart1 = "";
            var pixelShitPart2 = "";

            if (PlayState.isPixelStage)
            {
                pixelShitPart1 = "pixelUI\\";
                pixelShitPart2 = "-pixel";
            }

            rating.Source = Sprite.CreateGoodImage(Paths.Image(pixelShitPart1 + daRating + pixelShitPart2));
            rating.ResyncSize();
            rating.ScreenCenter();
            rating.X = coolText.GetPosition().X - 40;
            rating.Y -= 60;
            rating.Acceleration = new(rating.Acceleration.X, 550);
            rating.Velocity -= new Vector(Random.Shared.Next(140, 175), Random.Shared.Next(0, 10));
            rating.Visibility = !Settings.Default.HideHud ? Visibility.Visible : Visibility.Hidden;
            rating.X += Settings.Default.ComboOffset[0];
            rating.Y -= Settings.Default.ComboOffset[1];
            rating.Moves = true;

            var comboSpr = new Sprite
            {
                Source = Sprite.CreateGoodImage(Paths.Image(pixelShitPart1 + "combo" + pixelShitPart2)),
            };
            comboSpr.ResyncSize();
            comboSpr.ScreenCenter();
            comboSpr.X = coolText.GetPosition().X;
            comboSpr.Acceleration = new(comboSpr.Acceleration.X, 600);
            comboSpr.Velocity = new(comboSpr.Velocity.X, comboSpr.Velocity.Y - 150);
            comboSpr.Visibility = !Settings.Default.HideHud ? Visibility.Visible : Visibility.Hidden;
            comboSpr.X += Settings.Default.ComboOffset[0];
            comboSpr.Y -= Settings.Default.ComboOffset[1];
            comboSpr.Moves = true;
            camHUD.Children.Add(comboSpr);

            comboSpr.SacondOffsetPoint = new(Random.Shared.Next(1, 10), comboSpr.SacondOffsetPoint.Y);
            camHUD.Children.Insert(camHUD.Children.IndexOf(strumLineNotes), rating);

            if (!PlayState.isPixelStage)
            {
                rating.Scale = new(0.7, 0.7);
                rating.Antialiasing = Settings.Default.Antialiasing;
                comboSpr.Scale = new(0.7, 0.7);
                comboSpr.Antialiasing = Settings.Default.Antialiasing;
            }
            else
            {
                rating.Scale = new(daPixelZoom * 0.85, daPixelZoom * 0.85);
                comboSpr.Scale = new(daPixelZoom * 0.85, daPixelZoom * 0.85);
            }

            List<int> seperatedScore = new();

            if (combo >= 1000)
            {
                seperatedScore.Add((int)Math.Floor(combo / 1000.0) % 10);
            }
            seperatedScore.Add((int)Math.Floor(combo / 100.0) % 10);
            seperatedScore.Add((int)Math.Floor(combo / 10.0) % 10);
            seperatedScore.Add(combo % 10);

            var daLoop = 0;
            foreach (var i in seperatedScore)
            {
                var numScore = new Sprite
                {
                    Source = Sprite.CreateGoodImage(Paths.Image(pixelShitPart1 + "num" + i + pixelShitPart2)),
                };
                numScore.ResyncSize();
                numScore.ScreenCenter();
                numScore.X = coolText.GetPosition().X + (43 * daLoop) - 90;
                numScore.Y += 80;

                numScore.X += Settings.Default.ComboOffset[2];
                numScore.Y -= Settings.Default.ComboOffset[3];

                if (!PlayState.isPixelStage)
                {
                    numScore.SetZoom(0.5);
                    numScore.Antialiasing = Settings.Default.Antialiasing;
                }
                else numScore.SetZoom(daPixelZoom);

                numScore.Acceleration = new(numScore.Acceleration.X, Random.Shared.Next(200, 300));
                numScore.Velocity = new(Extentions.GetRandomNumber(-5, 5), numScore.Velocity.Y - Random.Shared.Next(140, 160));
                numScore.Visibility = !Settings.Default.HideHud ? Visibility.Visible : Visibility.Hidden;
                numScore.Moves = true;
                //if (combo >= 10 || combo == 0)
                camHUD.Children.Insert(camHUD.Children.IndexOf(strumLineNotes), numScore);
                Tween.Start(numScore.Opacity, 0, 0.2, new CubicEase(),
                    (s, e) => numScore.Opacity = e,
                    (s, e) => camHUD.Children.Remove(numScore),
                    Conductor.crochet * 0.001);

                daLoop++;
            }
            /* 
                trace(combo);
                trace(seperatedScore);
             */

            coolText.Text = string.Join(",", (seperatedScore));
            // add(coolText);

            Tween.Start(rating.Opacity, 0, 0.2, new CubicEase(),
                (s, e) => rating.Opacity = e,
                 (s, e) => { }, Conductor.crochet * 0.001);
            Tween.Start(comboSpr.Opacity, 0, 0.2, new CubicEase(),
                (s, e) => comboSpr.Opacity = e,
                 (s, e) =>
                 {
                     camHUD.Children.Remove(coolText);
                     camHUD.Children.Remove(comboSpr);

                     camHUD.Children.Remove(rating);
                 }, Conductor.crochet * 0.001);
        }

        void GoodNoteHit(Note note)
        {
            if (!note.wasGoodHit)
            {
                if (cpuControlled && (note.ignoreNote || note.hitCausesMiss)) return;

                if (note.hitCausesMiss)
                {
                    NoteMiss(note);
                    if (!note.noteSplashDisabled && !note.isSustainNote)
                    {
                        SpawnNoteSplashOnNote(note);
                    }

                    switch (note.NoteType)
                    {
                        case "Hurt Note": //Hurt note
                            if (boyfriend.Frames.GetAnimationFramesCountByName("hurt") > 0)
                            {
                                boyfriend.PlayAnim("hurt", true);
                                boyfriend.specialAnim = true;
                            }
                            break;
                    }

                    note.wasGoodHit = true;
                    //if (!note.isSustainNote)
                    notes.Children.Remove(note);
                    return;
                }

                if (!note.isSustainNote)
                {
                    combo += 1;
                    PopUpScore(note);
                    if (combo > 9999) combo = 9999;
                }
                health += note.hitHealth * healthGain;

                if (!note.noAnimation)
                {
                    var daAlt = "";
                    if (note.NoteType == "Alt Animation") daAlt = "-alt";

                    var animToPlay = singAnimations[Math.Abs(note.noteData)];

                    //if (note.isSustainNote){ wouldn"t this be fun : P. i think it would be swell

                    //if(note.gfNote) {
                    //  var anim = animToPlay +"-hold" + daAlt;
                    //	if(gf.animation.getByName(anim) == null)anim = animToPlay + daAlt;
                    //	gf.PlayAnim(anim, true);
                    //	gf.holdTimer = 0;
                    //} else {
                    //  var anim = animToPlay +"-hold" + daAlt;
                    //	if(boyfriend.animation.getByName(anim) == null)anim = animToPlay + daAlt;
                    //	boyfriend.PlayAnim(anim, true);
                    //	boyfriend.holdTimer = 0;
                    //}
                    //}else{
                    if (note.gfNote)
                    {
                        gf.PlayAnim(animToPlay + daAlt, true);
                        gf.holdTimer = 0;
                    }
                    else
                    {
                        boyfriend.PlayAnim(animToPlay + daAlt, true);
                        boyfriend.holdTimer = 0;
                    }
                    //}
                    if (note.NoteType == "Hey!")
                    {
                        if (boyfriend.animOffsets.ContainsKey("hey"))
                        {
                            boyfriend.PlayAnim("hey", true);
                            boyfriend.specialAnim = true;
                            boyfriend.heyTimer = 0.6;
                        }

                        if (gf.animOffsets.ContainsKey("cheer"))
                        {
                            gf.PlayAnim("cheer", true);
                            gf.specialAnim = true;
                            gf.heyTimer = 0.6;
                        }
                    }
                }

                if (cpuControlled)
                {
                    var time = 0.15;
                    if (note.isSustainNote && !note.Frames.CurrentAnimationName.EndsWith("end"))
                    {
                        time += 0.15;
                    }
                    StrumPlayAnim(false, Math.Abs(note.noteData) % 4, time);
                }
                else
                {
                    foreach (var spr in playerStrums)
                    {
                        if (Math.Abs(note.noteData) == playerStrums.IndexOf(spr))
                        {
                            spr.PlayAnim("confirm", true);
                        }
                    }
                }
                note.wasGoodHit = true;
                ChangeVocalVolume(1);

                var isSus = note.isSustainNote; //GET OUT OF MY HEAD, GET OUT OF MY HEAD, GET OUT OF MY HEAD
                var leData = Math.Abs(note.noteData);
                var leType = note.NoteType;
                CallOnLuas("goodNoteHit", notes.Children.IndexOf(note), leData, leType, isSus);

                //if (!note.isSustainNote)
                notes.Children.Remove(note);
            }
        }

        // Hold notes
        private void KeyShit()
        {
            // HOLDING
            var array = GetKeyFromEvent(true);
            var controlHoldArray = new bool[] { array.Contains(0), array.Contains(1), array.Contains(2), array.Contains(3) };


            // FlxG.watch.addQuick("asdfa", upP);
            if (!boyfriend.stunned && generatedMusic)
            {
                // rewritten inputs???
                foreach (Note daNote in notes.Children.OfType<Note>().ToArray())
                {
                    // hold note functions
                    if (daNote.isSustainNote && controlHoldArray[daNote.noteData] && daNote.canBeHit
                    && daNote.mustPress && !daNote.tooLate && !daNote.wasGoodHit)
                    {
                        GoodNoteHit(daNote);
                    }
                };

                if (controlHoldArray.Contains(true) && !endingSong)
                {
#if ACHIEVEMENTS_ALLOWED
				var achieve:String = checkForAchievement(["oversinging"]);
				if (achieve != null) {
					startAchievement(achieve);
				}
#endif
                }
                else if (boyfriend.holdTimer > Conductor.stepCrochet * 0.001 * boyfriend.singDuration &&
                    boyfriend.Frames.CurrentFriendlyAnimationName.StartsWith("sing")
                    && !boyfriend.Frames.CurrentFriendlyAnimationName.EndsWith("miss"))
                    boyfriend.Dance();
            }

        }

        private void OpponentNoteHit(Note note)
        {
            if (Paths.FormatToSongPath(SONG.song) != "tutorial")
                camZooming = true;

            if (note.NoteType == "Hey!" && dad.animOffsets.ContainsKey("hey"))
            {
                dad.PlayAnim("hey", true);
                dad.specialAnim = true;
                dad.heyTimer = 0.6;
            }
            else if (!note.noAnimation)
            {
                var altAnim = "";

                var curSection = (int)Math.Floor(CurStep / 16.0);
                if (SONG.notes.Length > curSection)
                {
                    if (SONG.notes[curSection].altAnim || note.NoteType == "Alt Animation")
                    {
                        altAnim = "-alt";
                    }
                }

                Character char1 = dad;
                String animToPlay = singAnimations[Math.Abs(note.noteData)] + altAnim;
                if (note.gfNote)
                {
                    char1 = gf;
                }

                char1.PlayAnim(animToPlay, true);
                char1.holdTimer = 0;
            }

            if (SONG.needsVoices)
                ChangeVocalVolume(1);

            var time = 0.15;
            if (note.isSustainNote && !note.Frames.CurrentAnimationName.EndsWith("end"))
            {
                time += 0.15;
            }
            StrumPlayAnim(true, Math.Abs(note.noteData) % 4, time);
            note.hitByOpponent = true;

            CallOnLuas("opponentNoteHit", notes.Children.IndexOf(note), Math.Abs(note.noteData), note.NoteType, note.isSustainNote);

            //if (!note.isSustainNote)
            notes.Children.Remove(note);
        }

        public void RecalculateRating()
        {
            SetOnLuas("score", songScore);
            SetOnLuas("misses", songMisses);
            SetOnLuas("hits", songHits);

            var ret = CallOnLuas("onRecalculateRating");
            if (!ret.Equals(FunkinLua.Function_Stop))
            {
                if (totalPlayed < 1) //Prevent divide by 0
                    ratingName = "?";
                else
                {
                    // Rating Percent
                    ratingPercent = Math.Min(1, Math.Max(0, totalNotesHit / totalPlayed));
                    //trace((totalNotesHit / totalPlayed) + ', Total: ' + totalPlayed + ', notes hit: ' + totalNotesHit);

                    // Rating Name
                    if (ratingPercent >= 1)
                        ratingName = ratingStuff[^1].Item1;
                    else
                    {
                        for (int i = 0; i < ratingStuff.Length - 1; i++)
                        {
                            if (ratingPercent < ratingStuff[i].Item2)
                            {
                                ratingName = ratingStuff[i].Item1;
                                break;
                            }
                        }
                    }
                }

                // Rating FC
                ratingFC = "";
                if (sicks > 0) ratingFC = "SFC";
                if (goods > 0) ratingFC = "GFC";
                if (bads > 0 || shits > 0) ratingFC = "FC";
                if (songMisses > 0 && songMisses < 10) ratingFC = "SDCB";
                else if (songMisses >= 10) ratingFC = "Clear";
            }
            SetOnLuas("rating", ratingPercent);
            SetOnLuas("ratingName", ratingName);
            SetOnLuas("ratingFC", ratingFC);
        }

        private int[] GetKeyFromEvent(bool isdown)
        {
            List<int> keys = new();
            for (int i = 0; i < PlayerSttings.NotesKeyBinds.Length; i++)
            {
                bool wasDown = false;
                for (int j = 0; j < PlayerSttings.NotesKeyBinds[i].Length; j++)
                {
                    if (!wasDown)
                        wasDown = Keyboard.IsKeyDown(PlayerSttings.NotesKeyBinds[i][j]);
                }
                if (isdown)
                {
                    if (wasDown) keys.Add(i);
                }
                else if (!wasDown) keys.Add(i);
            }
            return keys.ToArray();
        }

        void NoteMissPress(int direction = 1) //You pressed a key when there was no notes to press for this key
        {
            if (!boyfriend.stunned)
            {
                health -= 0.05 * healthLoss;
                if (instakillOnMiss)
                {
                    ChangeVocalVolume(0);
                    DoDeathCheck(true);
                }

                if (Settings.Default.GhostTapping) return;

                if (combo > 5 && gf.animOffsets.ContainsKey("sad"))
                {
                    gf.PlayAnim("sad");
                }
                combo = 0;

                if (!practiceMode) songScore -= 10;
                if (!endingSong)
                {
                    songMisses++;
                }
                totalPlayed++;
                RecalculateRating();

                Sound.Play(Paths.SoundRandom("missnote", 1, 3));
                // FlxG.sound.play(Paths.sound('missnote1'), 1, false);
                // FlxG.log.add('played imss note');

                /*boyfriend.stunned = true;

                // get stunned for 1/60 of a second, makes you able to
                new FlxTimer().start(1 / 60, function(tmr:FlxTimer)
                {
                    boyfriend.stunned = false;
                });*/

                if (boyfriend.hasMissAnimations)
                    boyfriend.PlayAnim(singAnimations[(Math.Abs(direction))] + "miss", true);
                 ChangeVocalVolume(0);
            }
        }
    }
}
