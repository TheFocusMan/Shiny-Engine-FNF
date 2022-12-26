using Newtonsoft.Json.Linq;
using Shiny_Engine_FNF.Code.Controls;
using Shiny_Engine_FNF.Code.FridayNightFunkinDataBase;
using Shiny_Engine_FNF.Code.Properties;
using Shiny_Engine_FNF.States.Editors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using WpfGame;
using WpfGame.Controls;

namespace Shiny_Engine_FNF.Code
{
    /// <summary>
    /// Interaction logic for PlayState.xaml
    /// </summary>
    public partial class PlayState : MusicBeatState
    {
        void StartCharacterPos(Character character, bool gfCheck = false)
        {
            if (gfCheck && character.CurrentCharcter.StartsWith("gf"))
            { //IF DAD IS GIRLFRIEND, HE GOES TO HER POSITION
                character.X = GF_X - (dadGroup.GetPosition().X);
                character.Y = GF_Y - (dadGroup.GetPosition().Y);
            }
            character.X += character.positionArray.X;
            character.Y += character.positionArray.Y;
        }

        int lastBeatHit = -1;

        void ResetLimoKill()
        {
            if (curStage == "limo")
            {
                limoMetalPole.X = -500;
                limoMetalPole.Visibility = Visibility.Hidden;
                limoLight.X = -500;
                limoLight.Visibility = Visibility.Hidden;
                limoCorpse.X = -500;
                limoCorpse.Visibility = Visibility.Hidden;
                limoCorpseTwo.X = -500;
                limoCorpseTwo.Visibility = Visibility.Hidden;
            }
        }

        bool fastCarCanDrive = true;

        void ResetFastCar()
        {
            fastCar.X = -12600;
            fastCar.Y = new Random().Next(140, 250);
            fastCarCanDrive = true;
        }

        double trainFrameTiming = 0;

        int trainCars = 8;
        bool trainFinishing = false;
        int trainCooldown = 0;

        bool startedMoving = false;

        void UpdateTrainPos()
        {
            if (trainSound.Position.TotalMilliseconds >= 4700)
            {
                startedMoving = true;
                gf.PlayAnim("hairBlow");
                gf.specialAnim = true;
            }

            if (startedMoving)
            {
                phillyTrain.X -= 400;

                if (phillyTrain.X < -2000 && !trainFinishing)
                {
                    phillyTrain.X = -1150;
                    trainCars -= 1;

                    if (trainCars <= 0)
                        trainFinishing = true;
                }

                if (phillyTrain.X < -4000 && trainFinishing)
                    TrainReset();
            }
        }

        void TrainReset()
        {
            gf.danced = false; //Sets head to the correct position once the animation ends
            gf.PlayAnim("hairFall");
            gf.specialAnim = true;
            phillyTrain.X = Display.DefaultWidth + 200;
            trainMoving = false;
            // trainSound.stop();
            // trainSound.time = 0;
            trainCars = 8;
            trainFinishing = false;
            startedMoving = false;
        }

        int dialogueCount = 0;
        public DialogueBoxPsych psychDialogue;
        //You don't have to add a song, just saying. You can just do "startDialogue(dialogueJson);" and it should work
        public void StartDialogue(DialogueFile dialogueFile, string song = null)
        {
            // TO DO: Make this more flexible, maybe?
            if (psychDialogue != null) return;

            if (dialogueFile.dialogue.Length > 0)
            {
                inCutscene = true;
                psychDialogue = new DialogueBoxPsych(dialogueFile, song);
                if (endingSong)
                    psychDialogue.finishThing += (s, e) => { psychDialogue = null; EndSong(); };
                else
                {
                    psychDialogue.finishThing += (s, e) =>
                    {
                        psychDialogue = null;
                        StartCountdown(null, null);
                    };
                }
                psychDialogue.NextDialogueThing += StartNextDialogue;
                psychDialogue.SkipDialogueThing += SkipDialogue;
                camHUD.Children.Add(psychDialogue);
            }
            else
            {
                Trace.TraceWarning("Your dialogue file is badly formatted!");
                if (endingSong)
                    EndSong();
                else StartCountdown(null, null);
            }
        }

        public bool transitioning = false;
        public void EndSong()
        {
            //Should kill you if you tried to cheat
            if (!startingSong)
            {
                foreach (Note daNote in notes.Children)
                    if (daNote.strumTime < songLength - Conductor.safeZoneOffset)
                        health -= 0.05 * healthLoss;

                foreach (var daNote in unspawnNotes)
                    if (daNote.strumTime < songLength - Conductor.safeZoneOffset)
                        health -= 0.05 * healthLoss;

                if (DoDeathCheck()) return;
            }

            foreach (var item in unspawnNotes)
            {
                notes.Children.Add(item);
                notes.Children.Remove(item);
            }
            unspawnNotes.Clear();

            timeBarBG.Visibility = Visibility.Hidden;
            timeBar.Visibility = Visibility.Hidden;
            timeTxt.Visibility = Visibility.Hidden;
            canPause = false;
            endingSong = true;
            camZooming = false;
            inCutscene = false;
            updateTime = false;

            deathCounter = 0;
            seenCutscene = false;

#if ACHIEVEMENTS_ALLOWED
		if(achievementObj != null) {
			return;
		} else {
			var achieve:String = checkForAchievement();

			if(achieve != null) {
				startAchievement(achieve);
				return;
			}
		}
#endif


            var ret = CallOnLuas("onEndSong");
            if (!ret.Equals(FunkinLua.Function_Stop) && !transitioning)
            {
                Instance = null;
                Paths.DisposeAll();
                if (SONG.validScore)
                {
                    var percent = ratingPercent;
                    Highscore.SaveScore(SONG.song, songScore, storyDifficulty, percent);
                }

                if (chartingMode)
                {
                    // TODO:
                    // openChartEditor();
                    return;
                }

                if (isStoryMode)
                {
                    campaignScore += songScore;
                    campaignMisses += songMisses;

                    storyPlaylist.Remove(storyPlaylist[0]);

                    if (storyPlaylist.Count <= 0)
                    {
                        Sound.Play(Paths.Music("freakyMenu"));

                        // if ()
                        if (!Settings.Default.PracticeMode && !Settings.Default.BotPlay)
                        {
                            StoryMenuState.weekCompleted.TryAdd(WeekData.weeksList[storyWeek], true);

                            if (SONG.validScore)
                                Highscore.SaveWeekScore(WeekData.getWeekFileName(), campaignScore, storyDifficulty);

                            DataFile.Data.weekCompleted = StoryMenuState.weekCompleted;
                            DataFile.Data.Save();
                        }
                        changedDifficulty = false;
                        SwichState(new StoryMenuState());
                    }
                    else
                    {
                        var difficulty = CoolUtil.GetDifficultyFilePath();
                        Trace.WriteLine("LOADING NEXT SONG");
                        Trace.WriteLine(Paths.FormatToSongPath(PlayState.storyPlaylist[0]) + difficulty);

                        var winterHorrorlandNext = (Paths.FormatToSongPath(SONG.song) == "eggnog");
                        if (winterHorrorlandNext)
                        {
                            var blackShit = new Sprite()
                            {
                                Position = new(-Display.DefaultWidth * camGame.GetZoom(), -Display.DefaultHeight * camGame.GetZoom()),
                                Width = Display.DefaultWidth * 3,
                                Height = Display.DefaultHeight * 3,
                                Background = Brushes.Black
                            };
                            camGame.Children.Add(blackShit);
                            camHUD.Visibility = Visibility.Hidden;

                            Sound.Play(Paths.Sound("Lights_Shut_off"));
                        }
                        PlayState.SONG = FNFSong.LoadFromJson(PlayState.storyPlaylist[0] + difficulty, PlayState.storyPlaylist[0]);
                        SwichState(new PlayState());
                    }
                }
                else
                {
                    Trace.WriteLine("WENT BACK TO FREEPLAY??");
                    SwichState(new FreeplayState());
                    Sound.Play(Paths.Music("freakyMenu"));
                    changedDifficulty = false;
                }
                camGame.Children.Clear();
                transitioning = true;
            }
        }


        private double stumLineY = 50;

        CustomLimitTimer startTimer;
        CustomLimitTimer finishTimer = null;


        public Sprite countdownReady;
        public Sprite countdownSet;
        public Sprite countdownGo;
        private void StartCountdown(object sender, EventArgs e)
        {
            if (startedCountdown)
            {
                CallOnLuas("onStartCountdown");
                return;
            }

            inCutscene = false;
            var ret = CallOnLuas("onStartCountdown");
            if (!ret.Equals(FunkinLua.Function_Stop))
            {
                GenerateStaticArrows(0);
                GenerateStaticArrows(1);
                for (int i = 0; i < playerStrums.Count; i++)
                {
                    SetOnLuas("defaultPlayerStrumX" + i, playerStrums[i].X);
                    SetOnLuas("defaultPlayerStrumY" + i, playerStrums[i].Y);
                }
                for (int i = 0; i < opponentStrums.Count; i++)
                {
                    SetOnLuas("defaultOpponentStrumX" + i, opponentStrums[i].X);
                    SetOnLuas("defaultOpponentStrumY" + i, opponentStrums[i].Y);
                    //if(Settings.Default.middleScroll) opponentStrums.members[i].visible = false;
                }
                Dispatcher.BeginInvoke(delegate () { }, DispatcherPriority.Render);
                startedCountdown = true;
                Conductor.songPosition = TimeSpan.Zero;
                Conductor.songPosition -= TimeSpan.FromMilliseconds(Conductor.crochet * 5);
                SetOnLuas("startedCountdown", true);
                CallOnLuas("onCountdownStarted");

                var swagCounter = 0;

                if (skipCountdown)
                {
                    Conductor.songPosition = TimeSpan.Zero;
                    Conductor.songPosition -= TimeSpan.FromMilliseconds(Conductor.crochet);
                    swagCounter = 3;
                }
                // Conductor.crochet / 1000
                startTimer = new CustomLimitTimer(TimeSpan.FromMilliseconds(Conductor.crochet), 5, delegate
                 {
                     if (startTimer.CurrentNumber % gfSpeed == 0 && !gf.stunned && (!gf.Frames.IsPlayingAmimation)) gf.Dance();

                     if (startTimer.CurrentNumber % 2 == 0)
                     {
                         if (!boyfriend.Frames.IsPlayingAmimation && !boyfriend.Frames.CurrentFriendlyAnimationName.StartsWith("sing"))
                         {
                             boyfriend.Dance();
                         }
                         if ((!dad.Frames.IsPlayingAmimation && !(dad.Frames.CurrentFriendlyAnimationName?.StartsWith("sing")).GetValueOrDefault()) && !dad.stunned)
                         {
                             dad.Dance();
                         }
                     }
                     else if (dad.danceIdle && ((!dad.Frames.IsPlayingAmimation) && !dad.Frames.CurrentFriendlyAnimationName.StartsWith("sing")) && !dad.stunned)
                         dad.Dance();

                     Dictionary<string, string[]> introAssets = new()
                     {
                         { "default", new[] { "ready", "set", "go" } },
                         { "pixel", new[] { "pixelUI\\ready-pixel", "pixelUI\\set-pixel", "pixelUI\\date-pixel" } }
                     };

                     var introAlts = introAssets["default"];
                     var antialias = Settings.Default.Antialiasing;
                     if (isPixelStage)
                     { 
                         introAlts = introAssets["pixel"];
                         antialias = false;
                     }

                     // head bopping for bg characters on Mall
                     if (curStage == "mall")
                     {
                         if (!Settings.Default.LowQuality)
                             upperBoppers.Dance(true);

                         bottomBoppers.Dance(true);
                         santa.Dance(true);
                     }

                     switch (swagCounter)
                     {
                         case 0:
                             Sound.Play(Paths.Sound("intro3" + introSoundsSuffix));
                             break;
                         case 1:
                             countdownReady = new() {
                                 Source = Sprite.CreateGoodImage(Paths.Image(introAlts[0])),
                                 Antialiasing = antialias
                             };
                             countdownReady.ResyncSize();

                             if (PlayState.isPixelStage)
                                 countdownReady.setGraphicSize(countdownReady.Width * daPixelZoom, countdownReady.Height);

                             camHUD.Children.Add(countdownReady);
                             countdownReady.ScreenCenter();
                             Tween.Start(countdownReady.Opacity, 0, Conductor.crochet / 1000, new CubicEase() { EasingMode = EasingMode.EaseInOut },
                                (s, e) => countdownReady.Opacity = e,
                                (s, e) => camHUD.Children.Remove(countdownReady));
                             Sound.Play(Paths.Sound("intro2" + introSoundsSuffix));
                             break;
                         case 2:
                             countdownSet = new() { 
                                 Source = Sprite.CreateGoodImage(Paths.Image(introAlts[1])),
                                 Antialiasing = antialias
                             };
                             countdownSet.ResyncSize();

                             if (PlayState.isPixelStage)
                                 countdownSet.setGraphicSize(countdownSet.Width * daPixelZoom, countdownSet.Height);

                             camHUD.Children.Add(countdownSet);
                             countdownSet.ScreenCenter();
                             Tween.Start(countdownSet.Opacity, 0, Conductor.crochet / 1000, new CubicEase() { EasingMode = EasingMode.EaseInOut },
                            (s, e) => countdownSet.Opacity = e,
                            (s, e) => camHUD.Children.Remove(countdownSet));
                             Sound.Play(Paths.Sound("intro1" + introSoundsSuffix));
                             break;
                         case 3:
                             if (!skipCountdown)
                             {
                                 countdownGo = new() { 
                                     Source = Sprite.CreateGoodImage(Paths.Image(introAlts[2])),
                                     Antialiasing = antialias
                                 };
                                 countdownGo.ResyncSize();

                                 if (PlayState.isPixelStage)
                                     countdownGo.setGraphicSize(countdownGo.Width * daPixelZoom, countdownGo.Height);

                                 camHUD.Children.Add(countdownGo);
                                 countdownGo.ScreenCenter();
                                 Tween.Start(countdownGo.Opacity, 0, Conductor.crochet / 1000, new CubicEase() { EasingMode = EasingMode.EaseInOut },
                                    (s, e) => countdownSet.Opacity = e,
                                    (s, e) => camHUD.Children.Remove(countdownGo));
                                 Sound.Play(Paths.Sound("introGo" + introSoundsSuffix));
                                 canPause = true;
                             }
                             break;
                         case 4:
                             break;
                     }
                     foreach (Note note in notes.Children)
                     {
                         note.copyAlpha = false;
                         note.Opacity = note.multAlpha;
                         if (Settings.Default.MiddleScroll && !note.mustPress)
                             note.Opacity *= 0.5;
                     }
                     CallOnLuas("onCountdownTick", swagCounter);

                     swagCounter += 1;
                 });
            }
        }

        private async void GenerateStaticArrows(int player)
        {
            for (int i = 0; i < 4; i++)
            {
                // FlxG.log.add(i);
                double targetAlpha = 1;
                if (player < 1 && Settings.Default.MiddleScroll) targetAlpha = 0.35;

                var babyArrow = new StrumNote(Settings.Default.MiddleScroll ? STRUM_X_MIDDLESCROLL : STRUM_X, stumLineY, i, player)
                {
                    downScroll = Settings.Default.DownScroll
                };
                if (!isStoryMode)
                {
                    babyArrow.Y -= 10;
                    babyArrow.Opacity = 0;
                    await Task.Delay(TimeSpan.FromSeconds(0.5 + (0.2 * i)));
                    Tween.Start(babyArrow.Y, babyArrow.Y + 10, 1, new CircleEase() { EasingMode = EasingMode.EaseOut },
                        (s, e) => babyArrow.Y = e, delegate (object s, EventArgs e) { });
                    Tween.Start(babyArrow.Opacity, targetAlpha, 1, new CircleEase() { EasingMode = EasingMode.EaseOut },
                        (s, e) => babyArrow.Opacity = e, delegate (object s, EventArgs e) { });
                }
                else babyArrow.Opacity = targetAlpha;
                if (player == 1)
                    playerStrums.Add(babyArrow);
                else
                {
                    if (Settings.Default.MiddleScroll)
                    {
                        babyArrow.X += 310;
                        if (i > 1)
                            babyArrow.X += Display.DefaultWidth / 2 + 25;
                    }
                    opponentStrums.Add(babyArrow);
                }

                strumLineNotes.Children.Add(babyArrow);
                babyArrow.postAddedToGroup();
            }
        }

        private Dictionary<string, bool> noteTypeMap = new();
        private Dictionary<string, bool> eventPushedMap = new();
        private void GenerateSong(string dataPath)
        {
            // FlxG.log.add(ChartParser.parse());
            songSpeedType = Settings.Default.ScrollType;

            switch (songSpeedType)
            {
                case "multiplicative":
                    SongSpeed = SONG.speed * Settings.Default.ScrollSpeed;
                    break;
                case "constant":
                    SongSpeed = Settings.Default.ScrollSpeed;
                    break;
            }

            var songData = SONG;
            Conductor.ChangeBPM(songData.bpm);

            curSong = songData.song;

            //ReloadSounds(); week 6 fuck

            notes = new CanvasWithMargarin();
            camHUD.Children.Add(notes);

            SwagSection[] noteData = songData.notes;


            int daBeats = 0; // Not exactly representative of "daBeats" lol, just how much it has looped

            var songName = Paths.FormatToSongPath(SONG.song);
            var file = Paths.Json(songName + "\\events");
            if (File.Exists(Paths.ModsJson(songName + "\\events")) || File.Exists(file))
            {
                var dat = FNFSong.LoadFromJson("events", songName);
                var eventsData = dat.events;
                if (eventsData != null)
                {
                    foreach (var event1 in eventsData) //Event Notes
                    {
                        for (int i = 0; i < event1[1].Count(); i++)
                        {
                            var newEventNote = new JArray(event1[0], event1[1][i][0], event1[1][i][1], event1[1][i][2]);
                            var subEvent = new JArray((double)newEventNote[0] + Settings.Default.NoteOffset - EventNoteEarlyTrigger(newEventNote), newEventNote[1], newEventNote[2], newEventNote[3]);
                            eventNotes.Add(subEvent);
                            EventPushed(subEvent);
                        }
                    }
                }
            }

            foreach (var section in noteData)
            {
                foreach (var songNotes in section.sectionNotes)
                {
                    var daStrumTime = (double)songNotes[0];
                    var daNoteData = (int)((double)songNotes[1] % 4);

                    var gottaHitNote = section.mustHitSection;

                    if ((double)songNotes[1] > 3)
                        gottaHitNote = !section.mustHitSection;

                    Note oldNote;
                    if (unspawnNotes.Count > 0)
                        oldNote = unspawnNotes[^1];
                    else
                        oldNote = null;

                    string type = "";
                    if (songNotes.Count >= 4)
                        type = (string)songNotes[3];

                    var swagNote = new Note(daStrumTime, daNoteData, oldNote)
                    {
                        mustPress = gottaHitNote,
                        sustainLength = (double)songNotes[2],
                        gfNote = (section.gfSection && ((double)songNotes[1] < 4)),
                        NoteType = type
                    };
                    if (int.TryParse(type, out _))
                        swagNote.NoteType = ChartingState.noteTypeList[(int)songNotes[3]]; //Backward compatibility + compatibility with Week 7 charts

                    var susLength = swagNote.sustainLength;

                    susLength /= Conductor.stepCrochet;
                    unspawnNotes.Add(swagNote);

                    int floorSus = (int)Math.Floor(susLength);
                    if (floorSus > 0)
                    {
                        for (int susNote = 0; susNote < floorSus + 1; susNote++)
                        {
                            oldNote = unspawnNotes[^1];

                            var sustainNote = new Note(daStrumTime + (Conductor.stepCrochet * susNote) + (Conductor.stepCrochet / Math.Round(SongSpeed, 2)), daNoteData, oldNote, true)
                            {
                                mustPress = gottaHitNote,
                                gfNote = (section.gfSection && ((double)songNotes[1] < 4)),
                                NoteType = swagNote.NoteType
                            };
                            unspawnNotes.Add(sustainNote);

                            if (sustainNote.mustPress)
                                sustainNote.X += Display.DefaultWidth / 2;
                            else if (Settings.Default.MiddleScroll)
                            {
                                sustainNote.X += 310;
                                if (daNoteData > 1) //Up and Right
                                    sustainNote.X += Display.DefaultWidth / 2 + 25;
                            }
                        }
                    }

                    if (swagNote.mustPress)
                        swagNote.X += Display.DefaultWidth / 2;
                    else if (Settings.Default.MiddleScroll)
                    {
                        swagNote.X += 310;
                        if (daNoteData > 1) //Up and Right
                            swagNote.X += Display.DefaultWidth / 2 + 25;
                    }

                    if (!noteTypeMap.ContainsKey(swagNote.NoteType))
                        noteTypeMap.Add(swagNote.NoteType, true);
                }
                daBeats += 1;
            }
            if (songData.events != null)
            {
                foreach (var event1 in songData.events) //Event Notes
                {
                    for (int i = 0; i < event1[1].Count(); i++)
                    {
                        var newEventNote = new JArray(event1[0], event1[1][i][0], event1[1][i][1], event1[1][i][2]);
                        var subEvent = new JArray((double)newEventNote[0] + Settings.Default.NoteOffset - EventNoteEarlyTrigger(newEventNote), newEventNote[1], newEventNote[2], newEventNote[3]);
                        eventNotes.Add(subEvent);
                        EventPushed(subEvent);
                    }
                }
            }

            // trace(unspawnNotes.length);
            // playerCounter += 1;

            unspawnNotes.Sort((x, y) => x.strumTime.CompareTo(y.strumTime));
            if (eventNotes.Count > 1)
            { //No need to sort if there"s a single one or none at all
                var notes = eventNotes.ToList();
                notes.Sort((x, y) => ((double)x[0]).CompareTo((double)y[0]));
            }
            CheckEventNote();
            generatedMusic = true;
        }

        public void ReloadHealthBarColors()
        {
            healthBar.BorderBrush = healthBar.Background = new SolidColorBrush(Color.FromRgb((byte)dad.healthColorArray[0], (byte)dad.healthColorArray[1], (byte)dad.healthColorArray[2]));
            healthBar.Foreground = new SolidColorBrush(Color.FromRgb((byte)boyfriend.healthColorArray[0], (byte)boyfriend.healthColorArray[1], (byte)boyfriend.healthColorArray[2]));
        }

        Tween cameraTwn;
        public void MoveCamera(bool isDad)
        {
            if (isDad)
            {
                Point dadGroupPos = dadGroup.GetPosition();
                var dadMidPos = dad.GetMidPoint();
                var checkPos = new Point(dadMidPos.X+ 150 + dadGroupPos.X, dadMidPos.Y + dadGroupPos.Y - 100);
                checkPos.X += (dad.cameraPosition.X + opponentCameraOffset.X);
                checkPos.Y += (dad.cameraPosition.Y + opponentCameraOffset.Y);
                camGame.Follow(new(checkPos.X, checkPos.Y));
                TweenCamIn();
            }
            else
            {
                Point boyfriendGroupPos = boyfriendGroup.GetPosition();
                var boyfriendMidPos = boyfriend.GetMidPoint();
                Point camfollow = new(boyfriendMidPos.X + boyfriendGroupPos.X - 100, boyfriendMidPos.Y + boyfriendGroupPos.Y - 100);

                camfollow.X -= boyfriend.cameraPosition.X - boyfriendCameraOffset.X;
                camfollow.Y += boyfriend.cameraPosition.Y + boyfriendCameraOffset.Y;

                camGame.Follow(new(camfollow.X, camfollow.Y));

                if (Paths.FormatToSongPath(SONG.song) == "tutorial" && cameraTwn == null && camGame.GetZoom() != 1)
                {
                    cameraTwn = new Tween(camGame.GetZoom(), 1, new ElasticEase() { EasingMode = EasingMode.EaseInOut }, (Conductor.stepCrochet * 4 / 1000));
                    cameraTwn.UpdateValue += (s, e) => camGame.SetZoom(e);
                    cameraTwn.Complite += (s, e) => cameraTwn = null;
                    cameraTwn.Start();
                }
            }
        }

        void TweenCamIn()
        {
            if (Paths.FormatToSongPath(SONG.song) == "tutorial" && cameraTwn == null && camGame.GetZoom() != 1.3)
            {
                cameraTwn = new Tween(camGame.GetZoom(), 1.3, new ElasticEase() { EasingMode = EasingMode.EaseInOut }, (Conductor.stepCrochet * 4 / 1000));
                cameraTwn.UpdateValue += (s, e) => camGame.SetZoom(e);
                cameraTwn.Complite += (s, e) => cameraTwn = null;
                cameraTwn.Start();
            }
        }

        int _lastid = 0;
        void MoveCameraSection(int id = 0)
        {
            if (SONG.notes[id] == null || _lastid == id) return;
            _lastid = id;
            if (SONG.notes[id].gfSection)
            {
                camGame.Follow(new(gf.GetMidPoint().X + gf.cameraPosition.X, gf.GetMidPoint().Y + gf.cameraPosition.Y));
                TweenCamIn();
                CallOnLuas("onMoveCamera", "gf");
                return;
            }

            if (!SONG.notes[id].mustHitSection)
            {
                MoveCamera(true);
                CallOnLuas("onMoveCamera", "dad");
            }
            else
            {
                MoveCamera(false);
                CallOnLuas("onMoveCamera", "boyfriend");
            }
        }

        public bool isDead = false; //Don't mess with this on Lua!!!
        bool DoDeathCheck(bool skipHealthCheck = false)
        {
            if (((skipHealthCheck && instakillOnMiss) || health <= 0) && !practiceMode && !isDead)
            {
                var ret = CallOnLuas("onGameOver");
                if (!ret.Equals(FunkinLua.Function_Stop))
                {
                    boyfriend.stunned = true;
                    deathCounter++;

                    paused = true;
                    Sound.StopAll();
                    // TODO:
                    //openSubState(new GameOverSubstate(boyfriend.getScreenPosition().x - boyfriend.positionArray[0], boyfriend.getScreenPosition().y - boyfriend.positionArray[1], camFollowPos.x, camFollowPos.y));

                    // MusicBeatState.switchState(new GameOverState(boyfriend.getScreenPosition().x, boyfriend.getScreenPosition().y));
                    // Game Over doesn't get his own variable because it's only used here
                    DiscordClient.ChangePresence("Game Over - " + detailsText, SONG.song + " (" + storyDifficultyText + ")", iconP2.getCharacter());
                    isDead = true;
                    return true;
                }
            }
            return false;
        }


        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (!inCutscene)
            {
                var eventKey = e.Key;
                var keys = GetKeyFromEvent(true);
                //trace('Pressed: ' + eventKey);
                foreach (int key in keys)
                {
                    if (!cpuControlled && !paused && !e.IsRepeat)
                    {
                        if (!boyfriend.stunned && generatedMusic && !endingSong)
                        {
                            //more accurate hit time for the ratings?
                            var lastTime = Conductor.songPosition;
                            //Conductor.songPosition = instumental.Music.CurrentTime;

                            var canMiss = !Settings.Default.GhostTapping;

                            // heavily based on my own code LOL if it aint broke dont fix it
                            List<Note> pressNotes = new();
                            //var notesDatas:Array<Int> = [];
                            var notesStopped = false;

                            List<Note> sortedNotesList = new();
                            foreach (Note daNote in notes.Children)
                            {
                                if (daNote.canBeHit && daNote.mustPress && !daNote.tooLate && !daNote.wasGoodHit && !daNote.isSustainNote)
                                {
                                    if (daNote.noteData == key)
                                    {
                                        sortedNotesList.Add(daNote);
                                        //notesDatas.push(daNote.noteData);
                                    }
                                    canMiss = true;
                                }
                            };
                            sortedNotesList.Sort((a, b) => a.strumTime.CompareTo(b.strumTime));

                            if (sortedNotesList.Count > 0)
                            {
                                foreach (var epicNote in sortedNotesList)
                                {
                                    foreach (var doubleNote in pressNotes)
                                    {
                                        if (Math.Abs(doubleNote.strumTime - epicNote.strumTime) < 1)
                                            notes.Children.Remove(doubleNote);
                                        else
                                            notesStopped = true;
                                    }

                                    // eee jack detection before was not super good
                                    if (!notesStopped)
                                    {
                                        GoodNoteHit(epicNote);
                                        pressNotes.Add(epicNote);
                                    }

                                }
                            }
                            else if (canMiss)
                            {
                                NoteMissPress(key);
                                CallOnLuas("noteMissPress", key);
                            }

                            // I dunno what you need this for but here you go
                            //									- Shubs

                            // Shubs, this is for the "Just the Two of Us" achievement lol
                            //									- Shadow Mario
                            keysPressed[key] = true;

                            //more accurate hit time for the ratings? part 2 (Now that the calculations are done, go back to the time it was before for not causing a note stutter)
                            Conductor.songPosition = lastTime;
                        }

                        var spr = playerStrums[key];
                        if (spr != null && spr.Frames.CurrentFriendlyAnimationName != "confirm")
                        {
                            spr.PlayAnim("pressed", true);
                            spr.resetAnim = 0;
                        }
                        CallOnLuas("onKeyPress", key);
                    }
                }
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (!inCutscene)
            {
                if (e.Key == Key.D9)
                {
                    iconP1.swapOldIcon();
                }
                if (PlayerSttings.Pause.Contains(e.Key) && startedCountdown && canPause)
                {
                    var ret = CallOnLuas("onPause");
                    if (!ret.Equals(FunkinLua.Function_Stop))
                    {
                        paused = true;

                        // 1 / 1000 chance for Gitaroo Man easter egg
                        if (Random.Shared.NextDouble() == 1 / 1000.0)
                        {
                            // gitaroo man easter egg
                            SwichState(new GitarooPause());
                        }
                        else
                        {
                            if (instumental != null)
                            {
                                instumental.Pause();
                                vocals?.Pause();
                            }
                            OpenSubState(new PauseSubState(boyfriend.PointToScreen(new()).X, boyfriend.PointToScreen(new()).Y));
                        }
                        DiscordClient.ChangePresence(detailsPausedText, SONG.song + " (" + storyDifficultyText + ")", iconP2.getCharacter());
                    }
                }
                /*
                if (FlxG.keys.anyJustPressed(debugKeysCharacter) && !endingSong && !inCutscene)
                {
                    paused = true;
                    MusicBeatState.SwitchState(new CharacterEditorState(SONG.player2));
                }
                if (FlxG.keys.anyJustPressed(debugKeysChart) && !endingSong && !inCutscene)
                {
                    openChartEditor();
                }
                // RESET = Quick Game Over Screen
                if (!ClientPrefs.noReset && controls.RESET && !inCutscene && !endingSong)
                {
                    health = 0;
                    trace("RESET = True");
                }*/
                //trace('released: ' + controlArray);

                var keys = GetKeyFromEvent(false);
                foreach (int key in keys)
                {
                    if (!cpuControlled && !paused && key > -1)
                    {
                        var spr = playerStrums[key];
                        if (spr != null)
                        {
                            spr.PlayAnim("static", true);
                            spr.resetAnim = 0;
                        }
                        CallOnLuas("onKeyRelease", key);
                    }
                }
            }
        }

        async void SchoolIntro(DialogueBox dialogueBox)
        {
            inCutscene = true;
            var black = new Sprite()
            {
                Position = new(-100, -100),
                Width = Display.DefaultWidth * 2,
                Height = Display.DefaultHeight * 2,
                Background = Brushes.Black
            };
            camGame.Children.Add(black);

            var red = new Sprite()
            {
                Position = new(-100, -100),
                Width = Display.DefaultWidth * 2,
                Height = Display.DefaultHeight * 2,
                Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xff, 0x1b, 0x31))
            };

            var senpaiEvil = new Sprite();
            senpaiEvil.Frames = Paths.GetSparrowAtlas("weeb\\senpaiCrazy", senpaiEvil, null, 24);
            senpaiEvil.Frames.AddByPrefix("idle", "Senpai Pre Explosion");
            senpaiEvil.Scale = new(6, 6);
            senpaiEvil.Antialiasing = false;
            senpaiEvil.ScreenCenter();
            senpaiEvil.X += 300;

            var songName = Paths.FormatToSongPath(SONG.song);
            if (songName == "roses" || songName == "thorns")
            {
                camGame.Children.Remove(black);

                if (songName == "thorns")
                {
                    camGame.Children.Add(red);
                    camHUD.Visibility = Visibility.Hidden;
                }
            }
            while (black.Opacity > 0)
            {
                black.Opacity -= 0.15;
                await Task.Delay(300);
            }
            if (dialogueBox != null)
            {
                if (Paths.FormatToSongPath(SONG.song) == "thorns")
                {
                    camGame.Children.Add(senpaiEvil);
                    senpaiEvil.Opacity = 0;
                    while (senpaiEvil.Opacity < 1)
                    {
                        senpaiEvil.Opacity += 0.15;
                        await Task.Delay(300);
                    }
                    senpaiEvil.Frames.PlayAnimation2("idle");
                    Sound.Play(Paths.Sound("Senpai_Dies"), async delegate ()
                    {
                        await Task.Delay(1000);
                        camGame.Children.Remove(senpaiEvil);
                        camGame.Children.Remove(red);
                        camGame.Children.Add(dialogueBox);
                        camHUD.Visibility = Visibility.Visible;
                    });
                }
                else
                {
                    camGame.Children.Add(dialogueBox);
                }
            }
            else
                StartCountdown(null, null);

            camGame.Children.Remove(black);
        }

        protected override async void Update()
        {
            if (instumental is not null && !instumental.IsDisposed && instumental?.Position == instumental?.Length)
                instumental.Stop();

            CallOnLuas("onUpdate", StaticTimer.DeltaSaconds);

            switch (curStage)
            {
                case "schoolEvil":
                    if (!Settings.Default.LowQuality && bgGhouls != null && !bgGhouls.Frames.IsPlayingAmimation)
                        bgGhouls.Visibility = Visibility.Hidden;
                    break;
                case "philly":
                    if (trainMoving)
                    {
                        trainFrameTiming += StaticTimer.DeltaSaconds;

                        if (trainFrameTiming >= 1 / 24)
                        {
                            UpdateTrainPos();
                            trainFrameTiming = 0;
                        }
                    }
                    phillyCityLights.Children[curLight].Opacity -= (Conductor.crochet / 1000) * StaticTimer.DeltaSaconds * 1.5;
                    break;
                case "limo":
                    if (!Settings.Default.LowQuality)
                    {
                        foreach (Sprite sprite in grpLimoParticles.Children.OfType<Sprite>().ToArray())
                        {
                            if (!sprite.Frames.IsPlayingAmimation)
                                grpLimoParticles.Children.Remove(sprite);
                        }

                        switch (limoKillingState)
                        {
                            case 1:
                                limoMetalPole.X += 5000 * StaticTimer.DeltaSaconds;
                                limoLight.X = limoMetalPole.X - 180;
                                limoCorpse.X = limoLight.X - 50;
                                limoCorpseTwo.X = limoLight.X + 35;

                                var dancers = grpLimoDancers.Children.OfType<BackgroundDancer>().ToArray();
                                for (int i = 0; i < dancers.Length; i++)
                                {
                                    if (dancers[i].X < Display.DefaultWidth * 1.5 && limoLight.X > (370 * i) + 130)
                                    {
                                        switch (i)
                                        {
                                            case 0 or 3:
                                                if (i == 0)
                                                {
                                                    Sound.Play(Paths.Sound("dancerdeath"));
                                                    await Task.Delay(500);
                                                }

                                                var diffStr = i == 3 ? " 2 " : " ";
                                                var particle = new BGSprite("gore\\noooooo", dancers[i].X + 200, dancers[i].Y, 0.4, 0.4, new[] { "hench leg spin" + diffStr + "PINK" }, false);
                                                grpLimoParticles.Children.Add(particle);
                                                particle = new BGSprite("gore\\noooooo", dancers[i].X + 160, dancers[i].Y + 200, 0.4, 0.4, new[] { "hench arm spin" + diffStr + "PINK" }, false);
                                                grpLimoParticles.Children.Add(particle);
                                                particle = new BGSprite("gore\\noooooo", dancers[i].X, dancers[i].Y + 50, 0.4, 0.4, new[] { "hench head spin" + diffStr + "PINK" }, false);
                                                grpLimoParticles.Children.Add(particle);

                                                particle = new BGSprite("gore\\stupidBlood", dancers[i].X - 110, dancers[i].Y + 20, 0.4, 0.4, new[] { "blood 1" }, false)
                                                {
                                                    FlipX = true,
                                                    Angle = -57.5
                                                };
                                                grpLimoParticles.Children.Add(particle);
                                                break;
                                            case 1:
                                                limoCorpse.Visibility = Visibility.Visible;
                                                break;
                                            case 2:
                                                limoCorpseTwo.Visibility = Visibility.Visible;
                                                break;
                                        } //Note: Nobody cares about the fifth dancer because he is mostly hidden offscreen :(
                                        dancers[i].X += Display.DefaultWidth * 2;
                                    }
                                }

                                if (limoMetalPole.X > Display.DefaultWidth * 2)
                                {
                                    ResetLimoKill();
                                    limoSpeed = 800;
                                    limoKillingState = 2;
                                }
                                break;
                            case 2:
                                limoSpeed -= 4000 * StaticTimer.DeltaSaconds;
                                bgLimo.X -= limoSpeed * StaticTimer.DeltaSaconds;
                                if (bgLimo.X > Display.DefaultWidth * 1.5)
                                {
                                    limoSpeed = 3000;
                                    limoKillingState = 3;
                                }
                                break;
                            case 3:
                                limoSpeed -= 2000 * StaticTimer.DeltaSaconds;
                                if (limoSpeed < 1000) limoSpeed = 1000;

                                bgLimo.X -= limoSpeed * StaticTimer.DeltaSaconds;
                                if (bgLimo.X < -275)
                                {
                                    limoKillingState = 4;
                                    limoSpeed = 800;
                                }
                                break;
                            case 4:
                                bgLimo.X = Mathf.Lerp(bgLimo.X, -150, Mathf.Clamp(StaticTimer.DeltaSaconds * 9, 0, 1));
                                if (Math.Round(bgLimo.X) == -150)
                                {
                                    bgLimo.X = -150;
                                    limoKillingState = 0;
                                }
                                break;
                        }

                        if (limoKillingState > 2)
                        {
                            var dancers = grpLimoDancers.Children.OfType<BackgroundDancer>().ToArray();
                            for (int i = 0; i < dancers.Length; i++)
                                dancers[i].X = (370 * i) + bgLimo.X + 280;
                        }
                    }
                    break;
                case "mall":
                    if (heyTimer > 0)
                    {
                        heyTimer -= StaticTimer.DeltaSaconds;
                        if (heyTimer <= 0)
                        {
                            bottomBoppers.Dance(true);
                            heyTimer = 0;
                        }
                    }
                    break;
            }

            if (!inCutscene)
            {
                //var lerpVal = Mathf.Clamp(StaticTimer.DeltaSaconds * 2.4 * cameraSpeed, 0, 1);
                //camFollowPos.setPosition(Mathf.Lerp(camFollowPos.X, camFollow.X, lerpVal), Mathf.Lerp(camFollowPos.Y, camFollow.Y, lerpVal));
                if (!startingSong && !endingSong && (boyfriend?.Frames.CurrentFriendlyAnimationName ?? "").StartsWith("idle"))
                {
                    boyfriendIdleTime += StaticTimer.DeltaSaconds;
                    if (boyfriendIdleTime >= 0.15)
                    { // Kind of a mercy thing for making the achievement easier to get as it"s apparently frustrating to some playerss
                        boyfriendIdled = true;
                    }
                }
                else boyfriendIdleTime = 0;
            }
            base.Update();

            if (ratingName == "?")
            {
                scoreTxt.Text = "Score: " + songScore + " | Misses: " + songMisses + " | Rating: " + ratingName;
            }
            else
            {
                scoreTxt.Text = "Score: " + songScore + " | Misses: " + songMisses + " | Rating: " + ratingName + " (" + Highscore.FloorDecimal(ratingPercent * 100, 2) + "%)" + " - " + ratingFC;//peeps wanted no integer rating
            }

            if (botplayTxt.IsVisible)
            {
                botplaySine += 180 * StaticTimer.DeltaSaconds;
                botplayTxt.Opacity = 1 - Math.Sin((Math.PI * botplaySine) / 180);
            }

            // FlxG.watch.addQuick("VOL", vocals.amplitudeLeft);
            // FlxG.watch.addQuick("VOLRight", vocals.amplitudeRight);

            var mult = Mathf.Lerp(1, iconP1.Scale.X, Mathf.Clamp(1 - (StaticTimer.DeltaSaconds * 9), 0, 1));
            iconP1.Scale = new(mult, mult);

            mult = Mathf.Lerp(1, iconP2.Scale.X, Mathf.Clamp(1 - (StaticTimer.DeltaSaconds * 9), 0, 1));
            iconP2.Scale = new(mult, mult);

            var iconOffset = 26;

            iconP1.X = healthBar.Margin.Left + (healthBar.Width * (Mathf.RemapToRange(healthBar.Value, 0, 100, 100, 0) * 0.01)) + (150 * iconP1.Scale.X - 150) / 2 - iconOffset;
            iconP2.X = healthBar.Margin.Left + (healthBar.Width * (Mathf.RemapToRange(healthBar.Value, 0, 100, 100, 0) * 0.01)) - (150 * iconP2.Scale.X) / 2 - iconOffset * 2;

            if (health > 2)
                health = 2;

            if (healthBar.Value < 20)
                iconP1.Frames.CurrentFrame = 1;
            else
                iconP1.Frames.CurrentFrame = 0;

            if (healthBar.Value > 80)
                iconP2.Frames.CurrentFrame = 1;
            else
                iconP2.Frames.CurrentFrame = 0;

            healthBar.Value = health * 50;

            if (startingSong)
            {
                if (startedCountdown)
                {
                    Conductor.songPosition += TimeSpan.FromSeconds(StaticTimer.DeltaSaconds);
                    if (Conductor.songPosition >= TimeSpan.Zero)
                        StartSong();
                }
            }
            else
            {
                Conductor.songPosition += TimeSpan.FromSeconds(StaticTimer.DeltaSaconds);

                if (!paused)
                {
                    songTime += Environment.TickCount - previousFrameTime;
                    previousFrameTime = Environment.TickCount;

                    // Interpolation type beat
                    if (Conductor.lastSongPos != Conductor.songPosition)
                    {
                        songTime = (songTime + Conductor.songPosition.TotalMilliseconds) / 2;
                        Conductor.lastSongPos = Conductor.songPosition;
                        // Conductor.songPosition.TotalSeconds += FlxG.StaticTimer.DeltaSaconds * 1000;
                        // trace("MISSED FRAME");
                    }

                    if (updateTime)
                    {
                        var curTime = Conductor.songPosition.TotalMilliseconds - Settings.Default.NoteOffset;
                        if (curTime < 0) curTime = 0;
                        songPercent = (curTime / songLength) * timeBar.Maximum;
                        timeBar.Value = double.IsFinite(songPercent) ? songPercent : 0;

                        var songCalc = (songLength - curTime);
                        if (Settings.Default.TimeBarType == "Time Elapsed") songCalc = curTime;

                        var secondsTotal = Math.Floor(songCalc);
                        if (secondsTotal < 0) secondsTotal = 0;

                        if (Settings.Default.TimeBarType != "Song Name")
                            timeTxt.Text = TimeSpan.FromMilliseconds(secondsTotal).ToString(@"mm\:ss");
                    }
                }

                // Conductor.lastSongPos = FlxG.sound.music.time;
            }

            if (camZooming)
            {
                camGame.SetZoom(Mathf.Lerp(defaultCamZoom, camGame.GetZoom(), Mathf.Clamp(1 - (StaticTimer.DeltaSaconds * 3.125), 0, 1)));
                camHUD.SetZoom(Mathf.Lerp(1, camHUD.GetZoom(), Mathf.Clamp(1 - (StaticTimer.DeltaSaconds * 3.125), 0, 1)));
            }

            //FlxG.watch.addQuick("beatShit", curBeat);
            //FlxG.watch.addQuick("stepShit", curStep);
            DoDeathCheck();

            if (unspawnNotes.Count > 0)
            {
                double time = 3000;//shit be werid on 4:3
                if (SongSpeed < 1) time /= SongSpeed;

                while (unspawnNotes.Count > 0 && unspawnNotes[0].strumTime - Conductor.songPosition.TotalMilliseconds < time)
                {
                    var dunceNote = unspawnNotes[0];
                    notes.Children.Insert(0, dunceNote);

                    unspawnNotes.Remove(dunceNote);
                }
            }

            if (generatedMusic && songLength > 0)
            {
                var fakeCrochet = (60.0 / SONG.bpm) * 1000;
                foreach (Note daNote in notes.Children.OfType<Note>().ToArray())
                {
                    var strumGroup = playerStrums;
                    if (!daNote.mustPress) strumGroup = opponentStrums;

                    var strumX = strumGroup[daNote.noteData].X;
                    var strumY = strumGroup[daNote.noteData].Y;
                    var strumAngle = strumGroup[daNote.noteData].Angle;
                    var strumDirection = strumGroup[daNote.noteData].direction;
                    var strumAlpha = strumGroup[daNote.noteData].Opacity;
                    var strumScroll = strumGroup[daNote.noteData].downScroll;

                    strumX += daNote.offsetX;
                    strumY += daNote.offsetY;
                    strumAngle += daNote.offsetAngle;
                    strumAlpha *= daNote.multAlpha;

                    if (strumScroll) //Downscroll
                    {
                        //daNote.Y = (strumY + 0.45 * (Conductor.songPosition.TotalSeconds - daNote.strumTime) * SongSpeed);
                        daNote.distance = (0.45 * (Conductor.songPosition.TotalMilliseconds - daNote.strumTime) * SongSpeed);
                    }
                    else //Upscroll
                    {
                        //daNote.Y = (strumY - 0.45 * (Conductor.songPosition.TotalSeconds - daNote.strumTime) * SongSpeed);
                        daNote.distance = (-0.45 * (Conductor.songPosition.TotalMilliseconds - daNote.strumTime) * SongSpeed);
                    }

                    var angleDir = strumDirection * Math.PI / 180;
                    if (daNote.copyAngle)
                        daNote.Angle = strumDirection - 90 + strumAngle;

                    if (daNote.copyAlpha)
                        daNote.Opacity = strumAlpha;

                    if (daNote.copyX)
                        daNote.X = strumX + Math.Cos(angleDir) * daNote.distance;

                    if (daNote.copyY)
                    {
                        daNote.Y = strumY + Math.Sin(angleDir) * daNote.distance;

                        // השם ישמור זה לקח לי כל כך הרבה זמן
                        // bad Psyc Engine Code
                        if (daNote.isSustainNote)
                        {
                            double addY = 0;
                            if (daNote.Frames.CurrentFriendlyAnimationName.EndsWith("end"))
                            {
                                addY += 10.5 * (fakeCrochet / 400) * 1.5 * SongSpeed + (46 * (SongSpeed - 1));
                                addY -= 46 * (1 - (fakeCrochet / 600)) * SongSpeed;
                                if (PlayState.isPixelStage) addY += 8;
                                else addY -= 19;
                            }
                            if (strumScroll)
                            {
                                addY += (Note.swagWidth / 2) - (60.5 * (SongSpeed - 1));
                                addY += 27.5 * ((SONG.bpm / 100) - 1) * (SongSpeed - 1);
                            }
                            daNote.Y += strumScroll ? addY : -addY;
                        }
                    }

                    if (!daNote.mustPress && daNote.wasGoodHit && !daNote.hitByOpponent && !daNote.ignoreNote)
                        OpponentNoteHit(daNote);

                    if (daNote.mustPress && cpuControlled)
                    {
                        if (daNote.isSustainNote)
                        {
                            if (daNote.canBeHit)
                                GoodNoteHit(daNote);
                        }
                        else if (daNote.strumTime <= Conductor.songPosition.TotalMilliseconds || (daNote.isSustainNote && daNote.canBeHit && daNote.mustPress))
                            GoodNoteHit(daNote);
                    }

                    // Kill extremely late notes and cause misses
                    if (Conductor.songPosition.TotalMilliseconds > noteKillOffset + daNote.strumTime)
                    {
                        if (daNote.mustPress && !cpuControlled && !daNote.ignoreNote && !endingSong && (daNote.tooLate || !daNote.wasGoodHit))
                            NoteMiss(daNote);
                        notes.Children.Remove(daNote);
                    }
                }
            }
            CheckEventNote();

            if (!inCutscene)
            {
                if (!cpuControlled)
                    KeyShit();
                else if (boyfriend.holdTimer > Conductor.stepCrochet * 0.001 * boyfriend.singDuration &&
                    boyfriend.Frames.CurrentFriendlyAnimationName.StartsWith("sing") &&
                    !boyfriend.Frames.CurrentFriendlyAnimationName.EndsWith("miss"))
                    boyfriend.Dance();
            }

            SetOnLuas("cameraX", camGame.GetPosition().X);
            SetOnLuas("cameraY", camGame.GetPosition().Y);
            SetOnLuas("botPlay", cpuControlled);

            CallOnLuas("onUpdatePost", StaticTimer.DeltaSaconds);
            ShaderUpdates?.Invoke(StaticTimer.DeltaSaconds);
        }
        public event Action<double> ShaderUpdates;

        int lastStepHit = -1;

        void ResyncVocals()
        {
            vocals?.Pause();

            instumental.Play();
            if (Conductor.songPosition < instumental.Length && !instumental.IsDisposed)
                instumental.Position = Conductor.songPosition;
            if (vocals != null)
            {
                vocals.Position = instumental.Position;
                vocals?.Play();
            }
        }

        private void ReloadSounds()
        {
            if (SONG.needsVoices)
                vocals = Sound.LoadOnly(Paths.Voices(PlayState.SONG.song));
            instumental = Sound.LoadOnly(Paths.Inst(PlayState.SONG.song));
        }

        public void ChangeVocalVolume(float vol)
        {
            if (vocals != null) vocals.Volume = vol;
        }

        public override void StepHit()
        {
            base.StepHit();
            if (instumental != null && !instumental.IsDisposed)
            {
                double inst = instumental.Position.TotalMilliseconds;
                double gameplay = Conductor.songPosition.TotalMilliseconds;
                double voices = (vocals?.Position.TotalMilliseconds).GetValueOrDefault();
                if ((inst > gameplay + 500 || inst < gameplay - 500 || voices > gameplay + 500 || voices < gameplay - 500) && health > 0) ResyncVocals();
            }

            if (CurStep == lastStepHit)
            {
                return;
            }

            lastStepHit = CurStep;
            SetOnLuas("curStep", CurStep);
            CallOnLuas("onStepHit");
        }

        public override void OpenSubState(SubState SubState)
        {
            if (paused)
            {
                if (instumental != null)
                {
                    instumental.Pause();
                    vocals?.Pause();
                }
                if (!startTimer.Finished)
                    startTimer.Active = false;
                if (finishTimer != null && !finishTimer.Finished)
                    finishTimer.Active = false;
                if (songSpeedTween != null)
                    songSpeedTween.Cancel();

                if (blammedLightsBlackTween != null)
                    blammedLightsBlackTween.Cancel();
                if (phillyCityLightsEventTween != null)
                    phillyCityLightsEventTween.Cancel();

                if (carTimer != null) carTimer.Active = false;

                var chars = new Character[] { boyfriend, gf, dad };
                for (int i = 0; i < chars.Length; i++)
                {
                    lock (chars)
                    {
                        if (chars[i].colorTween != null)
                        {
                            chars[i].colorTween.Cancel();
                        }
                    }
                }

                foreach (var tween in modchartTweens.Values) tween.Cancel();
                foreach (var timer in modchartTimers.Values) timer.Active = false;
            }
            base.OpenSubState(SubState);
        }

        public override void CloseSubState()
        {
            if (paused)
            {
                if (instumental != null && !startingSong)
                {
                    ResyncVocals();
                }

                if (!startTimer.Finished)
                    startTimer.Active = true;
                if (finishTimer != null && !finishTimer.Finished)
                    finishTimer.Active = true;
                if (songSpeedTween != null)
                    songSpeedTween.Start();

                if (blammedLightsBlackTween != null)
                    blammedLightsBlackTween.Start();
                if (phillyCityLightsEventTween != null)
                    phillyCityLightsEventTween.Start();

                if (carTimer != null) carTimer.Active = true;

                Character[] chars = new[] { boyfriend, gf, dad };
                for (int i = 0; i < chars.Length; i++)
                {
                    if (chars[i].colorTween != null)
                    {
                        chars[i].colorTween.Start();
                    }
                }

                foreach (var tween in modchartTweens)
                {
                    tween.Value.Start();
                }
                foreach (var timer in modchartTimers.ToList())
                {
                    timer.Value.Active = true;
                }
                paused = false;
                CallOnLuas("onResume");

                if (startTimer.Finished)
                {
                    DiscordClient.ChangePresence(detailsText, SONG.song + " (" + storyDifficultyText + ")", iconP2.getCharacter(), true, new DateTime(0) + TimeSpan.FromMilliseconds(songLength - Conductor.songPosition.TotalMilliseconds - Settings.Default.NoteOffset));
                }
                else
                {
                    DiscordClient.ChangePresence(detailsText, SONG.song + " (" + storyDifficultyText + ")", iconP2.getCharacter());
                }
            }
            base.CloseSubState();
        }

        void StartSong()
        {
            startingSong = false;

            ReloadSounds(); // week 6 fuck

            previousFrameTime = Environment.TickCount;
            lastReportedPlayheadPosition = 0;
            instumental.Play();
            instumental.PlaybackStopped += (s, e) =>
            {
                if (instumental.PlaybackState == NAudio.Wave.PlaybackState.Stopped)
                    FinishSong();
            };
            vocals?.Play();

            if (paused)
            {
                //trace("Oopsie doopsie! Paused sound");
                instumental.Pause();
                vocals?.Pause();
            }

            // Song duration in a float, useful for the time left feature
            songLength = instumental.Length.TotalMilliseconds;
            Tween.Start(timeBar.Opacity, 1, 0.5, new CircleEase() { EasingMode = EasingMode.EaseInOut },
                (s, e) => timeBar.Opacity = e, delegate (object s, EventArgs e) { });
            Tween.Start(timeTxt.Opacity, 1, 0.5, new CircleEase() { EasingMode = EasingMode.EaseInOut },
                (s, e) => timeTxt.Opacity = e, delegate (object s, EventArgs e) { });

            // Updating Discord Rich Presence (with Time Left)
            DiscordClient.ChangePresence(detailsText, SONG.song + " (" + storyDifficultyText + ")", iconP2.getCharacter(), true,
                new(TimeSpan.FromMilliseconds(songLength).Ticks));
            SetOnLuas("songLength", songLength);
            CallOnLuas("onSongStart");
        }

        void FinishSong()
        {
            var finishCallback = EndSong; //In case you want to change it in a specific song.

            updateTime = false;
            instumental.Dispose();
            if (vocals != null)
            {
                ChangeVocalVolume(0);
                vocals.Dispose();
            }
            if (Settings.Default.NoteOffset <= 0)
            {
                finishCallback();
            }
            else
            {
                finishTimer = new CustomLimitTimer(TimeSpan.FromMilliseconds(Settings.Default.NoteOffset), 1, delegate ()
                {
                    finishCallback();
                });
            }
        }
    }
}
