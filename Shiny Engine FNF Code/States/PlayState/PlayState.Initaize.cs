using Shiny_Engine_FNF.Code;
using Shiny_Engine_FNF.Code.Controls;
using Shiny_Engine_FNF.Code.FridayNightFunkinDataBase;
using Shiny_Engine_FNF.Code.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WpfGame;
using WpfGame.Controls;
using WpfGame.Particles;

namespace Shiny_Engine_FNF.Code
{
    public partial class PlayState
    {
        public const int STRUM_X = 42;
        public const int STRUM_X_MIDDLESCROLL = -278;

        public double defaultCamZoom = 1.05;
        public PlayState()
        {
            Paths.DisposeAll();
            Paths.SetCurrentLevelForWeek();

            InitializeComponent();

            InvalidateVisual();
            Instance = this;


            // For the "Just the Two of Us" achievement
            for (int i = 0; i < PlayerSttings.NotesKeyBinds.Length; i++)
            {
                keysPressed.Add(false);
            }

            Conductor.MapBPMChanges(SONG);
            Conductor.ChangeBPM(SONG.bpm);

            storyDifficultyText = CoolUtil.DifficultyFromInt(storyDifficulty);

            // String that contains the mode defined here so it isn"t necessary to call changePresence for each mode
            if (isStoryMode) detailsText = "Story Mode: " + WeekData.getCurrentWeek().weekName;
            else detailsText = "Freeplay";

            // String for when the game is paused
            detailsPausedText = "Paused - " + detailsText;

            //GameOverSubstate.resetVariables();
            var songName = SONG.song.ToLower().Replace(" ", "-");

            curStage = SONG.stage;

            if (PlayState.SONG.stage == null || PlayState.SONG.stage.Length < 1)
            {
                curStage = songName switch
                {
                    "spookeez" or "south" or "monster" => "spooky",
                    "pico" or "blammed" or "philly" or "philly-nice" => "philly",
                    "milf" or "satin-panties" or "high" => "limo",
                    "cocoa" or "eggnog" => "mall",
                    "winter-horrorland" => "mallEvil",
                    "senpai" or "roses" => "school",
                    "thorns" => "schoolEvil",
                    _ => "stage",
                };
            }

            var stageData = StageData.GetStageFile(curStage);
            if (stageData == null)
            {
                //Stage couldn"t be found, create a dummy stage for preventing a crash
                stageData = new()
                {
                    directory = "",
                    defaultZoom = 0.9,
                    isPixelStage = false,

                    boyfriend = new int[] { 770, 100 },
                    girlfriend = new int[] { 400, 130 },
                    opponent = new int[] { 100, 100 },
                    hide_girlfriend = false,

                    camera_boyfriend = new double[] { 0, 0 },
                    camera_opponent = new double[] { 0, 0 },
                    camera_girlfriend = new double[] { 0, 0 },
                    camera_speed = 1
                };
            }
            // set positions
            defaultCamZoom = stageData.defaultZoom;
            isPixelStage = stageData.isPixelStage;
            BF_X = stageData.boyfriend[0];
            BF_Y = stageData.boyfriend[1];
            GF_X = stageData.girlfriend[0];
            GF_Y = stageData.girlfriend[1];
            DAD_X = stageData.opponent[0];
            DAD_Y = stageData.opponent[1];

            if (stageData.camera_speed != null)
                cameraSpeed = stageData.camera_speed.Value;

            boyfriendCameraOffset = new(stageData.camera_boyfriend[0], stageData.camera_boyfriend[1]);
            opponentCameraOffset = new(stageData.camera_opponent[0], stageData.camera_opponent[1]);
            girlfriendCameraOffset = new(stageData.camera_girlfriend[0], stageData.camera_girlfriend[1]);

            boyfriendGroup = new() { VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Left };
            gfGroup = new() { VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Left };
            dadGroup = new() { VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Left };
            boyfriendGroup.Margin = new Thickness(BF_X, BF_Y, 0, 0);
            dadGroup.Margin = new Thickness(DAD_X, DAD_Y, 0, 0);
            gfGroup.Margin = new Thickness(GF_X, GF_Y, 0, 0);

            switch (curStage)
            {
                case "stage": //Week 1
                    {
                        var bg = new BGSprite("stageback", -600, -200, 0.9, 0.9);
                        camGame.Children.Add(bg);

                        var stageFront = new BGSprite("stagefront", -650, 600, 0.9, 0.9)
                        {
                            Scale = new(1.1, 1.1)
                        };
                        camGame.Children.Add(stageFront);

                        if (!Settings.Default.LowQuality)
                        {
                            BGSprite stageLight = new("stage_light", -125, -100, 0.9, 0.9);
                            stageLight.Scale = new(1.1, 1.1);
                            camGame.Children.Add(stageLight);
                            stageLight = new("stage_light", 1225, -100, 0.9, 0.9)
                            {
                                Scale = new(1.1, 1.1),
                                FlipX = true
                            };
                            camGame.Children.Add(stageLight);

                            var stageCurtains = new BGSprite("stagecurtains", -500, -300, 1.3, 1.3)
                            {
                                Scale = new(0.9, 0.9)
                            };
                            camGame.Children.Add(stageCurtains);
                        }
                    }
                    break;
                case "spooky": //Week 2
                    if (!Settings.Default.LowQuality) halloweenBG = new BGSprite("halloween_bg", -200, -100, new string[] { "halloweem bg", "halloweem bg lightning strike" });
                    else halloweenBG = new BGSprite("halloween_bg_low", -200, -100, null);
                    camGame.Children.Add(halloweenBG);

                    halloweenWhite = new BGSprite(null, -Width, -Height, 0, 0)
                    {
                        Background = new SolidColorBrush(Colors.White),
                        Opacity = 0,
                    };

                    //PRECACHE SOUNDS NO Asset loading in WPF
                    break;
                case "philly": //Week 3
                    {
                        if (!Settings.Default.LowQuality)
                        {
                            var bg = new BGSprite("philly\\sky", -100, 0, 0.1, 0.1);
                            camGame.Children.Add(bg);
                        }

                        //camGame.Children.AddShaderToCamera("game", chromAb);
                        //chromAb.setChrome(0.01);

                        var city = new BGSprite("philly\\city", -10, 0, 0.3, 0.3);
                        city.setGraphicSize((city.Width * 0.85));
                        camGame.Children.Add(city);

                        phillyCityLights = new CanvasWithMargarin();
                        camGame.Children.Add(phillyCityLights);

                        for (int i = 0; i < 5; i++)
                        {
                            var light = new BGSprite("philly\\win" + i, city.X, city.Y, 0.3, 0.3)
                            {
                                Visibility = Visibility.Hidden
                            };
                            light.setGraphicSize(light.Width * 0.85);
                            phillyCityLights.Children.Add(light);
                        }

                        if (!Settings.Default.LowQuality)
                        {
                            var streetBehind = new BGSprite("philly\\behindTrain", -40, 50, null);
                            camGame.Children.Add(streetBehind);
                        }

                        phillyTrain = new BGSprite("philly\\train", 2000, 360, null);
                        camGame.Children.Add(phillyTrain);

                        trainSound = Sound.LoadOnly(Paths.Sound("train_passes"));
                        var street = new BGSprite("philly\\street", -40, 50, null);
                        camGame.Children.Add(street);
                    }
                    break;
                case "limo": //Week 4
                    var skyBG = new BGSprite("limo\\limoSunset", -120, -50, 0.1, 0.1);
                    camGame.Children.Add(skyBG);

                    if (!Settings.Default.LowQuality)
                    {
                        limoMetalPole = new BGSprite("gore\\metalPole", -500, 220, 0.4, 0.4);
                        camGame.Children.Add(limoMetalPole);

                        bgLimo = new BGSprite("limo\\bgLimo", -150, 480, 0.4, 0.4, new string[] { "background limo pink" }, true);
                        camGame.Children.Add(bgLimo);

                        limoCorpse = new BGSprite("gore\\noooooo", -500, limoMetalPole.Y - 130, 0.4, 0.4, new string[] { "Henchmen on rail PINK" }, true);
                        camGame.Children.Add(limoCorpse);

                        limoCorpseTwo = new BGSprite("gore\\noooooo", -500, limoMetalPole.Y, 0.4, 0.4, new string[] { "henchmen death2 PINK" }, true);
                        camGame.Children.Add(limoCorpseTwo);

                        grpLimoDancers = new CanvasWithMargarin();
                        camGame.Children.Add(grpLimoDancers);

                        for (int i = 0; i < 5; i++)
                        {
                            BackgroundDancer dancer = new((370 * i) + 130, bgLimo.Y - 400);
                            dancer.X -= 0.4;
                            dancer.Y -= 0.4;
                            grpLimoDancers.Children.Add(dancer);
                        }

                        limoLight = new BGSprite("gore\\coldHeartKiller", limoMetalPole.X - 180, limoMetalPole.Y - 80, 0.4, 0.4);
                        camGame.Children.Add(limoLight);

                        grpLimoParticles = new CanvasWithMargarin();
                        camGame.Children.Add(grpLimoParticles);

                        //PRECACHE BLOOD
                        var particle = new BGSprite("gore\\stupidBlood", -400, -400, 0.4, 0.4, new string[] { "blood 1" }, false)
                        {
                            Opacity = 0.01
                        };
                        grpLimoParticles.Children.Add(particle);
                        ResetLimoKill();
                    }

                    limo = new BGSprite("limo\\limoDrive", -120, 550, 1, 1, new string[] { "Limo stage" }, true);

                    fastCar = new BGSprite("limo\\fastCarLol", -300, 160);
                    limoKillingState = 0;
                    break;
                case "mall": //Week 5 - Cocoa, Eggnog
                    {
                        var bg = new BGSprite("christmas\\bgWalls", -1000, -500, 0.2, 0.2);
                        bg.setGraphicSize((bg.Width * 0.8));
                        camGame.Children.Add(bg);

                        if (!Settings.Default.LowQuality)
                        {
                            upperBoppers = new BGSprite("christmas\\upperBop", -240, -90, 0.33, 0.33, new string[] { "Upper Crowd Bob" });
                            upperBoppers.setGraphicSize((upperBoppers.Width * 0.85));
                            camGame.Children.Add(upperBoppers);

                            var bgEscalator = new BGSprite("christmas\\bgEscalator", -1100, -600, 0.3, 0.3);
                            bgEscalator.setGraphicSize((bgEscalator.Width * 0.9));
                            camGame.Children.Add(bgEscalator);
                        }

                        var tree = new BGSprite("christmas\\christmasTree", 370, -250, 0.40, 0.40);
                        camGame.Children.Add(tree);

                        bottomBoppers = new BGSprite("christmas\\bottomBop", -300, 140, 0.9, 0.9, new string[] { "Bottom Level Boppers Idle" });
                        bottomBoppers.Frames.AddByPrefix("hey", "Bottom Level Boppers HEY");
                        bottomBoppers.setGraphicSize((bottomBoppers.Width * 1));
                        camGame.Children.Add(bottomBoppers);

                        var fgSnow = new BGSprite("christmas\\fgSnow", -600, 700);
                        camGame.Children.Add(fgSnow);

                        santa = new BGSprite("christmas\\santa", -840, 150, 1, 1, new string[] { "santa idle in fear" });
                        camGame.Children.Add(santa);
                    }
                    break;
                case "mallEvil": //Week 5 - Winter Horrorland
                    {
                        var bg = new BGSprite("christmas\\evilBG", -400, -500, 0.2, 0.2);
                        bg.setGraphicSize((bg.Width * 0.8));
                        camGame.Children.Add(bg);

                        var evilTree = new BGSprite("christmas\\evilTree", 300, -300, 0.2, 0.2);
                        camGame.Children.Add(evilTree);

                        var evilSnow = new BGSprite("christmas\\evilSnow", -200, 700);
                        camGame.Children.Add(evilSnow);
                    }
                    break;
                case "school": //Week 6 - Senpai, Roses
                    GameOverSubstate.deathSoundName = "fnf_loss_sfx-pixel";
                    GameOverSubstate.loopSoundName = "gameOver-pixel";
                    GameOverSubstate.endSoundName = "gameOverEnd-pixel";
                    GameOverSubstate.characterName = "bf-pixel-dead";

                    var bgSky = new BGSprite("weeb\\weebSky", 0, 0, 0.1, 0.1) { Antialiasing = false };
                    camGame.Children.Add(bgSky);

                    var repositionShit = -200;

                    var bgSchool = new BGSprite("weeb\\weebSchool", repositionShit, 0, 0.6, 0.90) { Antialiasing = false };
                    camGame.Children.Add(bgSchool);

                    var bgStreet = new BGSprite("weeb\\weebStreet", repositionShit, 0, 0.95, 0.95) { Antialiasing = false };
                    camGame.Children.Add(bgStreet);

                    double widShit = 6;
                    if (!Settings.Default.LowQuality)
                    {
                        var fgTrees = new BGSprite("weeb\\weebTreesBack", repositionShit + 170, 130, 0.9, 0.9) { Antialiasing = false };
                        fgTrees.SetZoom((widShit * 0.8));
                        camGame.Children.Add(fgTrees);
                    }

                    Sprite bgTrees = new() { X = repositionShit -380, Y =-800, Antialiasing = false};
                    bgTrees.Frames = Paths.GetPackerAtlas("weeb\\weebTrees", bgTrees, null, 12);
                    bgTrees.Frames.PlayAnimation2("trees",false,true);
                    camGame.Children.Add(bgTrees);

                    bgTrees.FrameSize = bgTrees.AvgFrameSize();
                    //bgTrees.UpdateHitbox();
                    bgTrees.RenderTransformOrigin = new(0.225, 0.185);

                    if (!Settings.Default.LowQuality)
                    {
                        var treeLeaves = new BGSprite("weeb\\petals", repositionShit, -40, 0.85, 0.85, new string[] { "PETALS ALL" }, true) { Antialiasing = false };
                        treeLeaves.SetZoom(widShit);
                        camGame.Children.Add(treeLeaves);
                    }

                    bgSky.SetZoom(widShit);
                    bgSchool.SetZoom(widShit);
                    bgStreet.SetZoom(widShit);
                    bgTrees.SetZoom(widShit * 1.4);

                    if (!Settings.Default.LowQuality)
                    {
                        bgGirls = new(-100, 190)
                        {
                            Antialiasing = false,
                            VerticalAlignment = VerticalAlignment.Top
                        };

                        bgGirls.SetZoom((daPixelZoom));
                        bgGirls.UpdateHitbox();
                        bgGirls.RenderTransformOrigin = new(0.9, 0.5);
                        camGame.Children.Add(bgGirls);
                    }
                    break;
                case "schoolEvil": //Week 6 - Thorns
                    GameOverSubstate.deathSoundName = "fnf_loss_sfx-pixel";
                    GameOverSubstate.loopSoundName = "gameOver-pixel";
                    GameOverSubstate.endSoundName = "gameOverEnd-pixel";
                    GameOverSubstate.characterName = "bf-pixel-dead";

                    /*if(!Settings.Default.LowQuality) { //Does this even do something?
						var waveEffectBG = new FlxWaveEffect(FlxWaveMode.ALL, 2, -1, 3, 2);
						var waveEffectFG = new FlxWaveEffect(FlxWaveMode.ALL, 2, -1, 5, 2);
					}*/
                    var posX = 400;
                    var posY = 200;
                    if (!Settings.Default.LowQuality)
                    {
                        var bg = new BGSprite("weeb\\animatedEvilSchool", posX, posY, 0.8, 0.9, new string[] { "background 2" }, true) { Antialiasing = false };
                        var scale = ((bg.RenderTransform as TransformGroup).Children[0] as ScaleTransform);
                        scale.ScaleX = scale.ScaleY = 6;
                        camGame.Children.Add(bg);

                        bgGhouls = new BGSprite("weeb\\bgGhouls", -100, 190, 0.9, 0.9, new string[] { "BG freaks glitch instance" }, false) { Antialiasing = false };
                        bgGhouls.SetZoom((daPixelZoom));
                        bgGhouls.Visibility = Visibility.Hidden;
                        camGame.Children.Add(bgGhouls);
                    }
                    else
                    {
                        var bg = new BGSprite("weeb\\animatedEvilSchool_low", posX, posY, 0.8, 0.9);
                        var scale = ((bg.RenderTransform as TransformGroup).Children[0] as ScaleTransform);
                        scale.ScaleX = scale.ScaleY = 6;
                        camGame.Children.Add(bg);
                    }
                    break;
            }

            if (isPixelStage) introSoundsSuffix = "-pixel";

            camGame.Children.Add(gfGroup);

            // Shitty layering but whatev it works LOL
            if (curStage == "limo")
                camGame.Children.Add(limo);

            camGame.Children.Add(dadGroup);
            camGame.Children.Add(boyfriendGroup);

            if (curStage == "spooky") camGame.Children.Add(halloweenWhite);

            if (curStage == "philly")
            {
                phillyCityLightsEvent = new();
                for (int i = 0; i < 5; i++)
                {
                    var light = new BGSprite("philly\\win" + i, -10, 0, 0.3, 0.3)
                    {
                        Visibility = Visibility.Hidden
                    };
                    light.setGraphicSize(light.Width * 0.85);
                    phillyCityLightsEvent.Children.Add(light);
                }
            }


            List<string> filesPushed = new();
            List<string> foldersToCheck = new() { Paths.GetPreloadPath("scripts\\") };

            foldersToCheck.Insert(0, Paths.ModFolders("scripts\\"));
            if (Paths.CurrentModDirectory != null && Paths.CurrentModDirectory.Length > 0)
                foldersToCheck.Insert(0, Paths.ModFolders(Paths.CurrentModDirectory + "\\scripts\\"));

            foreach (var folder in foldersToCheck)
            {
                if (File.Exists(folder))
                {
                    foreach (string file in Directory.GetDirectories(folder))
                    {
                        if (file.EndsWith(".lua") && !filesPushed.Contains(file))
                        {
                            luaArray.Add(new FunkinLua(folder + file));
                            filesPushed.Add(file);
                        }
                    }
                }
            }

            bool doPush = false;
            var luaFile = "stages\\" + curStage + ".lua";
            if (File.Exists(Paths.ModFolders(luaFile)))
            {
                luaFile = Paths.ModFolders(luaFile);
                doPush = true;
            }
            else
            {
                luaFile = Paths.GetPreloadPath(luaFile);
                if (File.Exists(luaFile)) doPush = true;
            }

            if (doPush)
                luaArray.Add(new FunkinLua(luaFile));

            if (!modchartSprites.ContainsKey("blammedLightsBlack"))
            {
                //Creates blammed light black fade in case you didn"t make your own
                blammedLightsBlack = new ModchartSprite()
                {
                    Margin = new Thickness(Display.DefaultWidth * -0.5, Display.DefaultHeight * -0.5, 0, 0),
                    Background = Brushes.Black,
                    Width = Display.DefaultWidth * 2,
                    Height = Display.DefaultHeight * 2
                };
                int position = camGame.Children.IndexOf(gfGroup);
                if (camGame.Children.IndexOf(boyfriendGroup) < position)
                {
                    position = camGame.Children.IndexOf(boyfriendGroup);
                }
                else if (camGame.Children.IndexOf(dadGroup) < position)
                {
                    position = camGame.Children.IndexOf(dadGroup);
                }
                camGame.Children.Insert(position, blammedLightsBlack);

                blammedLightsBlack.wasAdded = true;
                modchartSprites.Add("blammedLightsBlack", blammedLightsBlack);
            }
            if (curStage == "philly") camGame.Children.Insert(camGame.Children.IndexOf(blammedLightsBlack) + 1, phillyCityLightsEvent);
            blammedLightsBlack = modchartSprites["blammedLightsBlack"];
            blammedLightsBlack.Opacity = 0.0;

            var gfVersion = SONG.gfVersion;
            if (gfVersion == null || gfVersion.Length < 1)
            {
                gfVersion = curStage switch
                {
                    "limo" => "gf-car",
                    "mall" or "mallEvil" => "gf-christmas",
                    "school" or "schoolEvil" => "gf-pixel",
                    _ => "gf",
                };
                SONG.gfVersion = gfVersion; //Fix for the Chart Editor
            }

            if (!stageData.hide_girlfriend)
            {
                gf = new Character(gfVersion);
                StartCharacterPos(gf);
                //gf.scrollFactor.set(0.95, 0.95);
                gfGroup.Children.Add(gf);
                StartCharacterLua(gf.CurrentCharcter);
            }

            dad = new Character(SONG.player2);
            StartCharacterPos(dad, true);
            dadGroup.Children.Add(dad);
            StartCharacterLua(dad.CurrentCharcter);


            boyfriend = new Boyfriend(SONG.player1);
            StartCharacterPos(boyfriend);
            boyfriendGroup.Children.Add(boyfriend);
            StartCharacterLua(boyfriend.CurrentCharcter);

            Point camPos = girlfriendCameraOffset;
            camPos.X += gf.cameraPosition.X + gf.GetGraphicMidpoint().X + gfGroup.GetPosition().X;
            camPos.Y += gf.cameraPosition.Y + gf.GetGraphicMidpoint().Y + gfGroup.GetPosition().Y;

            if (dad.CurrentCharcter.StartsWith("gf"))
            {
                dad.X = GF_X - (dadGroup.GetPosition().X);
                dad.Y = GF_Y - (dadGroup.GetPosition().Y);
                gf.Visibility = Visibility.Hidden;
            }
            switch (curStage)
            {
                case "limo":
                    ResetFastCar();
                    camGame.Children.Insert(camGame.Children.IndexOf(gfGroup) - 1, fastCar);
                    break;
                case "schoolEvil":
                    {
                        var evilTrail = new TrailParticle(dad, 4, 24, 0.3, 0.069); //nice
                        camGame.Children.Insert(camGame.Children.IndexOf(dadGroup) - 1, evilTrail);
                    }
                    break;
            }

            if (Settings.Default.DownScroll)
                stumLineY = Display.DefaultHeight - 150;

            string file1 = Paths.Json(songName + "\\dialogue"); //Checks for json/Psych Engine dialogue
            if (File.Exists(file1)) dialogueJson = DialogueBoxPsych.ParseDialogue(file1);

            file1 = Paths.Txt(songName + "\\" + songName + "Dialogue"); //Checks for vanilla/Senpai dialogue
            if (File.Exists(file1))
            {
                dialogue = File.ReadAllLines(file1);
            }
            DialogueBox doof = new(false, dialogue);
            // doof.x += 70;
            // doof.y =             Display.DefaultHeight * 0.5;
            doof.FinishThing += StartCountdown;
            doof.NextDialogueThing += StartNextDialogue;
            doof.SkipDialogueThing += SkipDialogue;

            Conductor.songPosition = TimeSpan.FromMilliseconds(-5000);
            var showTime = (Settings.Default.TimeBarType != "Disabled");
            timeTxt.Margin = new(STRUM_X + (Display.DefaultWidth / 2) - 248, 19, 0, 0);
            timeTxt.Opacity = 0;
            timeTxt.StrokeThickness = 2;
            timeTxt.Visibility = showTime ? Visibility.Visible : Visibility.Hidden;

            if (Settings.Default.DownScroll) timeTxt.SetPosition(timeTxt.Margin.Left, Display.DefaultHeight - 44);

            if (Settings.Default.TimeBarType == "Song Name") timeTxt.Text = SONG.song;
            updateTime = showTime;

            timeBarBG = new AttachedSprite("timeBar", null, null, false)
            {
                X = timeTxt.Margin.Left,
                Y = timeTxt.Margin.Top + (32 / 4),
                Opacity = 0,
                Visibility = showTime ? Visibility.Visible : Visibility.Hidden,
                Background = Brushes.Black,
                xAdd = -4,
                yAdd = -4
            };
            camHUD.Children.Insert(camHUD.Children.IndexOf(timeBar), timeBarBG);
            timeBar.Margin = new(timeBarBG.X + 4, timeBarBG.Y + 4, 0, 0);
            timeBar.Opacity = 0;
            timeBar.Width = (timeBarBG.Width - 8);
            timeBar.Height = timeBarBG.Height - 8;
            timeBar.Visibility = showTime ? Visibility.Visible : Visibility.Hidden;
            timeBarBG.sprTracker = timeBar;

            if (Settings.Default.TimeBarType == "Song Name")
            {
                timeTxt.Width = 24;
                timeTxt.Height = 24;
                timeTxt.Margin = new(timeTxt.Margin.Left, timeTxt.Margin.Top + 3, 0, 0);
            }

            var splash = new NoteSplash(100, 100, 0);
            grpNoteSplashes.Children.Add(splash);
            splash.Opacity = 0.0;

            opponentStrums = new();
            playerStrums = new();

            // startCountdown();

            GenerateSong(SONG.song);
            foreach (var notetype in noteTypeMap.Keys)
            {
                var luaToLoad = Paths.ModFolders("custom_notetypes/" + notetype + ".lua");
                if (File.Exists(luaToLoad))
                    luaArray.Add(new FunkinLua(luaToLoad));
                else
                {
                    luaToLoad = Paths.GetPreloadPath("custom_notetypes/" + notetype + ".lua");
                    if (File.Exists(luaToLoad))
                        luaArray.Add(new FunkinLua(luaToLoad));
                }
            }
            foreach (var event1 in eventPushedMap.Keys)
            {
                var luaToLoad = Paths.ModFolders("custom_events/" + event1 + ".lua");
                if (File.Exists(luaToLoad))
                    luaArray.Add(new FunkinLua(luaToLoad));
                else
                {
                    luaToLoad = Paths.GetPreloadPath("custom_events/" + event1 + ".lua");
                    if (File.Exists(luaToLoad))
                        luaArray.Add(new FunkinLua(luaToLoad));
                }
            }

            noteTypeMap.Clear();
            noteTypeMap = null;
            eventPushedMap.Clear();
            eventPushedMap = null;

            // After all characters being loaded, it makes then invisible 0.01s later so that the player won"t freeze when you change characters
            // add(strumLine);
            //camGame.FollowLearp = 1;
            camGame.Follow(camPos);

            MoveCameraSection(0);

            healthBarBG = new AttachedSprite("healthBar", null, null, false)
            {
                Visibility = !Settings.Default.HideHud ? Visibility.Visible : Visibility.Hidden,
                xAdd = -4,
                yAdd = -4
            };
            camHUD.Children.Insert(camHUD.Children.IndexOf(healthBar), healthBarBG);
            healthBarBG.ScreenCenter();
            healthBarBG.Y = Display.DefaultHeight * 0.89;
            if (Settings.Default.DownScroll) healthBarBG.Y = 0.11 * Display.DefaultHeight;

            healthBar.SetPosition(healthBarBG.X + 4, healthBarBG.Y + 4);
            healthBar.Width = healthBarBG.Width - 8;
            healthBar.Height = healthBarBG.Height - 8;
            // healthBar
            healthBar.Visibility = !Settings.Default.HideHud ? Visibility.Visible : Visibility.Hidden;
            healthBar.Opacity = Settings.Default.HealthBarAlpha;
            healthBarBG.sprTracker = healthBar;


            iconP1 = new HealthIcon(boyfriend.healthIcon, true)
            {
                Y = healthBar.Margin.Top - 75,
                Visibility = !Settings.Default.HideHud ? Visibility.Visible : Visibility.Hidden,
                Opacity = Settings.Default.HealthBarAlpha
            };
            camHUD.Children.Add(iconP1);

            iconP2 = new HealthIcon(dad.healthIcon, false)
            {
                Y = healthBar.Margin.Top - 75,
                Visibility = !Settings.Default.HideHud ? Visibility.Visible : Visibility.Hidden,
                Opacity = Settings.Default.HealthBarAlpha
            };
            camHUD.Children.Add(iconP2);
            ReloadHealthBarColors();

            // scoreTxt.ScreenCenter();
            scoreTxt.Width = Display.DefaultWidth;
            scoreTxt.SetPosition(0, healthBarBG.Y + 36);
            scoreTxt.Visibility = !Settings.Default.HideHud ? Visibility.Visible : Visibility.Hidden;

            botplayTxt.SetPosition(400, timeBarBG.Y + 55);
            botplayTxt.Visibility = cpuControlled ? Visibility.Visible : Visibility.Hidden;
            if (Settings.Default.DownScroll) botplayTxt.SetPosition(400, timeBarBG.Y - 78);
            startingSong = true;

            filesPushed = new List<string>();
            foldersToCheck = new List<string>() { Paths.GetPreloadPath("data\\" + Paths.FormatToSongPath(SONG.song) + "\\") };

            foldersToCheck.Insert(0, Paths.ModFolders("data\\" + Paths.FormatToSongPath(SONG.song) + "/"));

            if (!string.IsNullOrEmpty(Paths.CurrentModDirectory))
                foldersToCheck.Insert(0, Paths.ModFolders(Paths.CurrentModDirectory + "\\data\\" + Paths.FormatToSongPath(SONG.song) + "\\"));


            foreach (var folder in foldersToCheck)
            {
                if (Directory.Exists(folder))
                {
                    foreach (var file in Directory.GetFiles(folder))
                    {
                        if (file.EndsWith(".lua") && !filesPushed.Contains(file))
                        {
                            luaArray.Add(new FunkinLua(folder + file));
                            filesPushed.Add(file);
                        }
                    }
                }
            }

            this.InvalidateVisual();
            Dispatcher.BeginInvoke(delegate () { }, System.Windows.Threading.DispatcherPriority.Render);
            var daSong = Paths.FormatToSongPath(curSong);
            if (isStoryMode && !seenCutscene)
            {
                switch (daSong)
                {
                    case "monster":
                        var whiteScreen = new Sprite()
                        {
                            Position = new(),
                            Width = Display.DefaultWidth * 2,
                            Height = Display.DefaultHeight * 2,
                            Background = Brushes.White
                        };
                        camGame.Children.Add(whiteScreen);
                        camHUD.Visibility = Visibility.Hidden;
                        SnapCamFollowToPos(dad.GetMidPoint().X + 150, dad.GetMidPoint().Y - 100);
                        inCutscene = true;

                        Tween.Start(whiteScreen.Opacity, 0, 1, new CubicEase(),
                            (s, e) => whiteScreen.Opacity = e,
                            (s, e) =>
                            {
                                camHUD.Visibility = Visibility.Visible;
                                camGame.Children.Remove(whiteScreen);
                                StartCountdown(null, null);
                            });
                        Sound.Play(Paths.SoundRandom("thunder_", 1, 2));
                        gf.PlayAnim("scared", true);
                        boyfriend.PlayAnim("scared", true);
                        break;
                    case "winter-horrorland":
                        var blackScreen = new Sprite()
                        {
                            Width = Display.DefaultWidth * 2,
                            Height = Display.DefaultHeight * 2,
                            Background = Brushes.Black
                        };
                        camGame.Children.Add(blackScreen);
                        camHUD.Visibility = Visibility.Hidden;
                        inCutscene = true;

                        Tween.Start(blackScreen.Opacity, 0, 0.7, new CubicEase(),
                            (s, e) => blackScreen.Opacity = e,
                            (s, e) => camGame.Children.Remove(blackScreen));

                        Sound.Play(Paths.Sound("Lights_Turn_On"));
                        SnapCamFollowToPos(400, -2050);
                        camGame.SetZoom(1.5);

                        var fun = async delegate ()
                        {
                            await Task.Delay(800);
                            camHUD.Visibility = Visibility.Visible;
                            camGame.Children.Remove(blackScreen);
                            Tween.Start(camGame.GetZoom(), defaultCamZoom, 2.5, new QuadraticEase() { EasingMode = EasingMode.EaseInOut },
                                (s, e) => camGame.SetZoom(e),
                                (s, e) => StartCountdown(s, e));
                        };

                        fun();
                        break;
                    case "senpai" or "roses" or "thorns":
                        if (daSong == "roses") Sound.Play(Paths.Sound("ANGRY"));
                        SchoolIntro(doof);
                        break;
                    default:
                        StartCountdown(null, null);
                        break;
                }
                seenCutscene = true;
            }
            else
            {
                StartCountdown(null, null);
            }
            RecalculateRating();

            /*
            //PRECACHING MISS SOUNDS BECAUSE I THINK THEY CAN LAG PEOPLE AND FUCK THEM UP IDK HOW HAXE WORKS
            CoolUtil.precacheSound("missnote1");
            CoolUtil.precacheSound("missnote2");
            CoolUtil.precacheSound("missnote3");
            BRO THATS LITERLY C# */

            // Updating Discord Rich Presence.
            DiscordClient.ChangePresence(detailsText, SONG.song + " (" + storyDifficultyText + ")", iconP2.getCharacter());

            Conductor.safeZoneOffset = (Settings.Default.SafeFrames / 60.0) * 1000;
            CallOnLuas("onCreatePost");

            //Paths.clearUnusedMemory();
            //CustomFadeTransition.nextCamera = camOther;

            Unloaded += PlayState_Unloaded;
        }

        private bool preventLuaRemove = false;
        private void PlayState_Unloaded(object sender, RoutedEventArgs e)
        {
            preventLuaRemove = true;
            for (int i = 0;i< luaArray.Count;i++)
            {
                luaArray[i].Call("onDestroy");
                luaArray[i].Stop();
            }
            luaArray.Clear();
            foreach (var notes in unspawnNotes)
            {
                camGame.Children.Add(notes);
                camGame.Children.Remove(notes);
            }
            unspawnNotes.Clear();
            panel.Children.Clear();
            camGame.Children.Clear();
            Trace.WriteLine($"Num of Vars:{typeof(PlayState).GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Length} Holy Shit");
        }
    }
}
