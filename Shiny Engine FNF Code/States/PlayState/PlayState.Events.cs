using Newtonsoft.Json.Linq;
using Shiny_Engine_FNF.Code;
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
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using WpfGame;
using WpfGame.Controls;

namespace Shiny_Engine_FNF.Code
{
    public partial class PlayState
    {
        public void TriggerEventNote(string eventName, string value1, string value2)
        {
            switch (eventName)
            {
                case "Hey!":
                    {
                        int value = value1.ToLower().Trim() switch
                        {
                            "bf" or "boyfriend" or "0" => 0,
                            "gf" or "girlfriend" or "1" => 1,
                            _ => 2,
                        };
                        _ = double.TryParse(value2, out double time);
                        if (time <= 0) time = 0.6;

                        if (value != 0)
                        {
                            if (dad.CurrentCharcter.StartsWith("gf"))
                            { //Tutorial GF is actually Dad! The GF is an imposter!! ding ding ding ding ding ding ding, dindinding, end my suffering
                                dad.PlayAnim("cheer", true);
                                dad.specialAnim = true;
                                dad.heyTimer = time;
                            }
                            else
                            {
                                gf.PlayAnim("cheer", true);
                                gf.specialAnim = true;
                                gf.heyTimer = time;
                            }

                            if (curStage == "mall")
                            {
                                bottomBoppers.Frames.PlayAnimation2("hey", true, false);
                                heyTimer = time;
                            }
                        }
                        if (value != 1)
                        {
                            boyfriend.PlayAnim("hey", true);
                            boyfriend.specialAnim = true;
                            boyfriend.heyTimer = time;
                        }
                    }
                    break;

                case "Set GF Speed":
                    {
                        _ = int.TryParse(value1, out int value);
                        gfSpeed = value;
                    }
                    break;

                case "Blammed Lights":
                    {
                        _ = int.TryParse(value1, out int lightId);

                        if (lightId > 0 && curLightEvent != lightId)
                        {
                            if (lightId > 5) lightId = RandomWithExludes(1, 5, curLightEvent);

                            uint color = lightId switch
                            {
                                //Blue
                                1 => 0xff31a2fd,
                                //Green
                                2 => 0xff31fd8c,
                                //Pink
                                3 => 0xfff794f7,
                                //Red
                                4 => 0xfff96d63,
                                //Orange
                                5 => 0xfffba633,
                                _ => 0xffffffff,
                            };
                            curLightEvent = lightId;

                            if (blammedLightsBlack.Opacity == 0)
                            {
                                if (blammedLightsBlackTween != null)
                                    blammedLightsBlackTween.Cancel();
                                blammedLightsBlackTween = new Tween(blammedLightsBlack.Opacity, 1, new QuadraticEase() { EasingMode = EasingMode.EaseInOut }, 1);
                                blammedLightsBlackTween.UpdateValue += (s, e) => blammedLightsBlack.Opacity = e;
                                blammedLightsBlackTween.Complite += (s, e) => blammedLightsBlackTween = null;
                                blammedLightsBlackTween.Start();

                                var chars = new Character[] { boyfriend, gf, dad };
                                for (int i = 0; i < chars.Length-1; i++)
                                {
                                    if (chars[i].colorTween != null) chars[i].colorTween.Cancel();
                                    chars[i].colorTween = new TweenColor(Colors.White, color.ToColor(), new QuadraticEase() { EasingMode = EasingMode.EaseInOut }, 1);
                                    chars[i].colorTween.UpdateValue += (s, e) => chars[i].Color = e;
                                    chars[i].colorTween.Complite += (s, e) => chars[i].colorTween = null;
                                    chars[i].colorTween.Start();
                                }
                            }
                            else
                            {
                                if (blammedLightsBlackTween != null)
                                    blammedLightsBlackTween.Cancel();
                                blammedLightsBlackTween = null;
                                blammedLightsBlack.Opacity = 1;

                                Character[] chars = new[] { boyfriend, gf, dad };
                                for (int i = 0; i < chars.Length-1; i++)
                                {
                                    if (chars[i].colorTween != null)
                                        chars[i].colorTween.Cancel();
                                    chars[i].colorTween = null;
                                }
                                dad.Color = color.ToColor();
                                boyfriend.Color = color.ToColor();
                                gf.Color = color.ToColor();


                                if (curStage == "philly")
                                {
                                    if (phillyCityLightsEvent != null)
                                    {
                                        foreach (FrameworkElement element in phillyCityLightsEvent.Children) element.Visibility = Visibility.Hidden;
                                        phillyCityLightsEvent.Children[lightId - 1].Visibility = Visibility.Visible;
                                        phillyCityLightsEvent.Children[lightId - 1].Opacity = 1;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (blammedLightsBlack.Opacity != 0)
                            {
                                if (blammedLightsBlackTween != null) blammedLightsBlackTween.Cancel();
                                blammedLightsBlackTween = new Tween(blammedLightsBlack.Opacity, 0, new QuadraticEase() { EasingMode = EasingMode.EaseInOut }, 1);
                                blammedLightsBlackTween.UpdateValue += (s, e) => blammedLightsBlack.Opacity = e;
                                blammedLightsBlackTween.Complite += (s, e) => blammedLightsBlackTween = null;
                                blammedLightsBlackTween.Start();
                            }

                            if (curStage == "philly")
                            {
                                foreach (FrameworkElement element in phillyCityLights.Children) element.Visibility = Visibility.Hidden;
                                foreach (FrameworkElement element in phillyCityLightsEvent.Children) element.Visibility = Visibility.Hidden;

                                FrameworkElement memb = (FrameworkElement)phillyCityLightsEvent.Children[curLightEvent - 1];
                                if (memb != null)
                                {
                                    memb.Visibility = Visibility.Visible;
                                    memb.Opacity = 1;
                                    if (phillyCityLightsEventTween != null)
                                        phillyCityLightsEventTween.Cancel();

                                    phillyCityLightsEventTween = new Tween(memb.Opacity, 0, new QuadraticEase() { EasingMode = EasingMode.EaseInOut }, 1);
                                    blammedLightsBlackTween.UpdateValue += (s, e) => memb.Opacity = e;
                                    phillyCityLightsEventTween.Complite += (s, e) => phillyCityLightsEventTween = null;
                                    blammedLightsBlackTween.Start();
                                }
                            }

                            var chars = new Character[] { boyfriend, gf, dad };
                            for (int i = 0; i < chars.Length-1; i++)
                            {
                                if (chars[i].colorTween != null)
                                    chars[i].colorTween.Cancel();
                                chars[i].colorTween = new TweenColor(chars[i].Color, Colors.White, new QuadraticEase() { EasingMode = EasingMode.EaseInOut }, 1);
                                chars[i].colorTween.UpdateValue += (s, e) => chars[i].Color = e;
                                chars[i].colorTween.Complite += (s, e) => chars[i].colorTween = null;
                                chars[i].colorTween.Start();
                            }

                            curLight = 0;
                            curLightEvent = 0;
                        }
                    }
                    break;

                case "Kill Henchmen":
                    KillHenchmen();
                    break;
                case "Add Camera Zoom":
                    if (Settings.Default.CamZooms && camGame.GetZoom() < 1.35)
                    {
                        _ = double.TryParse(value1, out double camZoom);
                        _ = double.TryParse(value2, out double hudZoom);

                        camGame.SetZoom(camZoom);
                        camHUD.SetZoom(hudZoom);
                    }
                    break;

                case "Trigger BG Ghouls":
                    if (curStage == "schoolEvil" && !Settings.Default.LowQuality)
                    {
                        bgGhouls.Dance(true);
                        bgGhouls.Visibility = Visibility.Visible;
                    }
                    break;

                case "Play Animation":
                    { //trace("Anim to play: " + value1);
                        var char1 = dad;
                        switch (value2.ToLower().Trim())
                        {
                            case "bf" or "boyfriend":
                                char1 = boyfriend;
                                break;
                            case "gf" or "girlfriend":
                                char1 = gf;
                                break;
                            default:
                                int val2 = 0;
                                _ = int.TryParse(value2, out val2);

                                switch (val2)
                                {
                                    case 1:
                                        char1 = boyfriend;
                                        break;
                                    case 2:
                                        char1 = gf;
                                        break;
                                }
                                break;
                        }
                        char1.PlayAnim(value1, true);
                        char1.specialAnim = true;
                    }
                    break;
                case "Camera Follow Pos":
                    {
                        double val2 = 0;
                        isCameraOnForcedPos = false;
                        if (!double.TryParse(value1, out double val1) || !double.TryParse(value2, out val2))
                        {
                            SnapCamFollowToPos(val1, val2);
                            isCameraOnForcedPos = true;
                        }
                    }
                    break;
                case "Alt Idle Animation":
                    {
                        var char1 = dad;
                        switch (value1.ToLower())
                        {
                            case "gf" or "girlfriend":
                                char1 = gf;
                                break;
                            case "boyfriend" or "bf":
                                char1 = boyfriend;
                                break;
                            default:
                                int val = 0;
                                _ = int.TryParse(value1, out val);

                                switch (val)
                                {
                                    case 1:
                                        char1 = boyfriend;
                                        break;
                                    case 2:
                                        char1 = gf;
                                        break;
                                }
                                break;
                        }
                        char1.idleSuffix = value2;
                        char1.RecalculateDanceIdle();
                    }
                    break;
                case "Screen Shake":
                    string[] valuesArray = new[] { value1, value2 };
                    Camera2D[] targetsArray = new[] { camGame, camHUD };
                    for (int i = 0; i < targetsArray.Length; i++)
                    {
                        var split = valuesArray[i].Split(",");
                        double duration = 0;
                        double intensity = 0;
                        if (split[0] != null) duration = double.Parse(split[0].Trim());
                        if (split[1] != null) intensity = double.Parse(split[1].Trim());

                        if (duration > 0 && intensity != 0)
                        {
                            targetsArray[i].Shake(intensity, duration);
                        }
                    }
                    break;

                case "Change Character":
                    {
                        var charType = 0;
                        switch (value1)
                        {
                            case "gf" or "girlfriend":
                                charType = 2;
                                break;
                            case "dad" or "opponent":
                                charType = 1;
                                break;
                            default:
                                _ = int.TryParse(value1, out charType);
                                break;
                        }

                        switch (charType)
                        {
                            case 0:
                                if (boyfriend.CurrentCharcter != value2)
                                {
                                    if (!boyfriendMap.ContainsKey(value2))
                                        AddCharacterToList(value2, charType);

                                    var lastAlpha = boyfriend.Opacity;
                                    boyfriend.Opacity = 0.00001;
                                    boyfriend = boyfriendMap[value2];
                                    boyfriend.Opacity = lastAlpha;
                                    iconP1.ChangeIcon(boyfriend.healthIcon);
                                }
                                SetOnLuas("boyfriendName", boyfriend.CurrentCharcter);
                                break;
                            case 1:
                                if (dad.CurrentCharcter != value2)
                                {
                                    if (!dadMap.ContainsKey(value2))
                                        AddCharacterToList(value2, charType);

                                    var wasGf = dad.CurrentCharcter.StartsWith("gf");
                                    var lastAlpha = dad.Opacity;
                                    dad.Opacity = 0.00001;
                                    dad = dadMap[value2];
                                    if (!dad.CurrentCharcter.StartsWith("gf"))
                                        if (wasGf) gf.Visibility = Visibility.Visible;
                                        else gf.Visibility = Visibility.Hidden;
                                    dad.Opacity = lastAlpha;
                                    iconP2.ChangeIcon(dad.healthIcon);
                                }
                                SetOnLuas("dadName", dad.CurrentCharcter);
                                break;
                            case 2:
                                if (gf.CurrentCharcter != value2)
                                {
                                    if (!gfMap.ContainsKey(value2))
                                        AddCharacterToList(value2, charType);

                                    var lastAlpha = gf.Opacity;
                                    gf.Opacity = 0.00001;
                                    gf = gfMap[value2];
                                    gf.Opacity = lastAlpha;
                                }
                                SetOnLuas("gfName", gf.CurrentCharcter);
                                break;
                        }
                        ReloadHealthBarColors();
                    }
                    break;
                case "BG Freaks Expression":
                    if (bgGirls != null) bgGirls.SwapDanceType();
                    break;
                case "Change Scroll Speed":
                    {
                        if (songSpeedType == "constant")
                            return;
                        _ = double.TryParse(value1, out double val1);
                        _ = double.TryParse(value2, out double val2);

                        var newValue = SONG.speed * Settings.Default.ScrollSpeed * val1;

                        if (val2 <= 0)
                            SongSpeed = newValue;
                        else
                        {
                            songSpeedTween = new Tween(SongSpeed, newValue, new CubicEase(), val2);
                            songSpeedTween.UpdateValue += (s, e) => SongSpeed = e;
                            songSpeedTween.Complite += (s, e) => songSpeedTween = null;
                        }
                    }
                    break;
            }
            CallOnLuas("onEvent", eventName, value1, value2);
        }

        void KillHenchmen()
        {
            if (!Settings.Default.LowQuality && Settings.Default.Violence && curStage == "limo")
            {
                if (limoKillingState < 1)
                {
                    limoMetalPole.X = -400;
                    limoMetalPole.Visibility = Visibility.Visible;
                    limoLight.Visibility = Visibility.Visible;
                    limoCorpse.Visibility = Visibility.Hidden;
                    limoCorpseTwo.Visibility = Visibility.Hidden;
                    limoKillingState = 1;

#if ACHIEVEMENTS_ALLOWED
				Achievements.henchmenDeath++;
				FlxG.save.data.henchmenDeath = Achievements.henchmenDeath;
				var achieve:String = checkForAchievement(['roadkill_enthusiast']);
				if (achieve != null) {
					startAchievement(achieve);
				} else {
					FlxG.save.flush();
				}
				FlxG.log.add('Deaths: ' + Achievements.henchmenDeath);
#endif
                }
            }
        }

        protected override void BeatHit()
        {
            base.BeatHit();
            if (lastBeatHit >= CurBeat)
            {
                //trace("BEAT HIT: " + curBeat + ", LAST HIT: " + lastBeatHit);
                return;
            }

            if ((int)Math.Floor(CurStep / 16.0) < SONG.notes.Length)
            {
                if (SONG.notes[(int)Math.Floor(CurStep / 16.0)].changeBPM)
                {
                    Conductor.ChangeBPM(SONG.notes[(int)Math.Floor(CurStep / 16.0)].bpm);
                    //FlxG.log.add("CHANGED BPM!");
                    SetOnLuas("curBpm", Conductor.bpm);
                    SetOnLuas("crochet", Conductor.crochet);
                    SetOnLuas("stepCrochet", Conductor.stepCrochet);
                }
                SetOnLuas("mustHitSection", SONG.notes[(int)Math.Floor(CurStep / 16.0)].mustHitSection);
                SetOnLuas("altAnim", SONG.notes[(int)Math.Floor(CurStep / 16.0)].altAnim);
                SetOnLuas("gfSection", SONG.notes[(int)Math.Floor(CurStep / 16.0)].gfSection);
                // else
                // Conductor.changeBPM(SONG.bpm);
            }
            // FlxG.log.add("change bpm" + SONG.notes[Std.int(curStep / 16)].changeBPM);

            if (generatedMusic && (CurStep / 16) < SONG.notes.Length && !endingSong && !isCameraOnForcedPos)
            {
                MoveCameraSection(CurStep / 16);
            }
            if (camZooming && camGame.GetZoom() < 1.35 && Settings.Default.CamZooms && CurBeat % 4 == 0)
            {
                camGame.SetZoom(camGame.GetZoom() + 0.015);
                camHUD.SetZoom(camHUD.GetZoom() + 0.03);
            }

            iconP1.Scale = new(1.2, 1.2);
            iconP2.Scale = new(1.2, 1.2);

            // שיט של אנימציות
            if (CurBeat % gfSpeed == 0 && !gf.stunned && (!gf.Frames.IsPlayingAmimation)) gf.Dance();

            if (CurBeat % 2 == 0)
            {
                if (!boyfriend.Frames.IsPlayingAmimation)
                    boyfriend.Dance();
                if (!dad.Frames.IsPlayingAmimation && !dad.stunned)
                    dad.Dance();
            }
            else if (dad.danceIdle && !dad.Frames.IsPlayingAmimation && !dad.stunned)
               dad.Dance();

            switch (curStage)
            {
                case "school":
                    if (!Settings.Default.LowQuality) bgGirls.Dance();
                    break;

                case "mall":
                    if (!Settings.Default.LowQuality) upperBoppers.Dance(true);

                    if (heyTimer <= 0) bottomBoppers.Dance(true);
                    santa.Dance(true);
                    break;
                case "limo":
                    if (!Settings.Default.LowQuality)
                    {
                        foreach (BackgroundDancer Dancer in grpLimoDancers.Children)
                        {
                            Dancer.Dance();
                        }
                    }
                    if ((Random.Shared.Next(10) % 2) == 0 && fastCarCanDrive) FastCarDrive();
                    break;
                case "philly":
                    if (!trainMoving)
                        trainCooldown += 1;

                    if (CurBeat % 4 == 0)
                    {
                        foreach (BGSprite light in phillyCityLights.Children) light.Visibility = Visibility.Hidden;

                        curLight = Random.Shared.Next(0, phillyCityLights.Children.Count - 1);

                        phillyCityLights.Children[curLight].Visibility = Visibility.Visible;
                        phillyCityLights.Children[curLight].Opacity = 1;
                    }

                    if (CurBeat % 8 == 4 && (Random.Shared.Next(30) % 2) == 0 && !trainMoving && trainCooldown > 8)
                    {
                        trainCooldown = Random.Shared.Next(-4, 0);
                        TrainStart();
                    }
                    break;
            }

            if (curStage == "spooky" && (Random.Shared.Next(10) % 2) == 0 && CurBeat > lightningStrikeBeat + lightningOffset) LightningStrikeShit();
            lastBeatHit = CurBeat;

            SetOnLuas("curBeat", CurBeat);//DAWGG?????
            CallOnLuas("onBeatHit");
        }

        void SnapCamFollowToPos(double x, double y)
        {
            camGame.Follow(new Point(x, y));
        }


        void EventPushed(JArray event1)
        {
            switch ((string)event1[1])
            {
                case "Change Character":
                    var charType = ((string)event1[2]).ToLower() switch
                    {
                        "gf" or "girlfriend" or "1" => 2,
                        "dad" or "opponent" or "0" => 1,
                        _ => double.IsNaN((double)event1[2]) ? 0 : (double)event1[2],
                    };
                    var newCharacter = (string)event1[3];
                    AddCharacterToList(newCharacter, (int)charType);
                    break;
            }

            if (!eventPushedMap.ContainsKey((string)event1[1]))
                eventPushedMap.Add((string)event1[1], true);
        }

        double EventNoteEarlyTrigger(JArray event1)
        {
            double returnedValue = Convert.ToDouble(CallOnLuas("eventEarlyTrigger", (string)event1[1]));
            if (returnedValue != 0)
            {
                return returnedValue;
            }

            return (string)event1[1] switch
            {
                //Better timing so that the kill sound matches the beat intended
                "Kill Henchmen" => 280,//Plays 280ms before the actual position
                _ => 0,
            };
        }

        int lightningStrikeBeat = 0;
        int lightningOffset = 8;
        async void LightningStrikeShit()
        {
            Sound.Play(Paths.SoundRandom("thunder_", 1, 2));
            if (!Settings.Default.LowQuality) halloweenBG.Frames.PlayAnimation2("halloweem bg lightning strike");

            lightningStrikeBeat = CurBeat;
            lightningOffset = Random.Shared.Next(8, 24);

            if (boyfriend.animOffsets.ContainsKey("scared")) boyfriend.PlayAnim("scared", true);
            if (gf.animOffsets.ContainsKey("scared")) gf.PlayAnim("scared", true);

            if (Settings.Default.CamZooms)
            {
                camGame.SetZoom(camGame.GetZoom() + 0.015);
                camHUD.SetZoom(camHUD.GetZoom() + 0.03);

                if (!camZooming)
                { //Just a way for preventing it to be permanently zoomed until Skid & Pump hits a note
                    Tween.Start(camGame.GetZoom(), defaultCamZoom, 0.5, new CubicEase(),
                        (s, e) => camGame.SetZoom(e), (s, e) => { });
                    Tween.Start(camHUD.GetZoom(), 1, 0.5, new CubicEase(),
                        (s, e) => camHUD.SetZoom(e), (s, e) => { });
                }
            }

            if (Settings.Default.Flashing)
            {
                halloweenWhite.Opacity = 0.4;
                Tween.Start(halloweenWhite.Opacity, 0.5, 0.075, new CubicEase(),
                    (s, e) => halloweenWhite.Opacity = e, (s, e) => { });
                await Task.Delay(150);
                Tween.Start(halloweenWhite.Opacity, 0, 0.25, new CubicEase(),
                    (s, e) => halloweenWhite.Opacity = e, (s, e) => { });
            }
        }

        public void CheckEventNote()
        {
            while (eventNotes.Count > 0)
            {
                double leStrumTime = (double)eventNotes[0][0];
                if (Conductor.songPosition.TotalMilliseconds < leStrumTime)
                {
                    break;
                }

                var value1 = "";
                if (eventNotes[0][2] != null)
                    value1 = (string)eventNotes[0][2];

                var value2 = "";
                if (eventNotes[0][3] != null)
                    value2 = (string)eventNotes[0][3];

                TriggerEventNote((string)eventNotes[0][1], value1, value2);
                eventNotes.RemoveAt(0);
            }
        }

        private static int RandomWithExludes(int min, int max, params int[] exludes)
        {
            var range = Enumerable.Range(min, max).Where(i => !exludes.Contains(i));

            var rand = new System.Random();
            int index = rand.Next(min, max - exludes.Length);
            return range.ElementAt(index);
        }

        CustomLimitTimer carTimer;
        async void FastCarDrive()
        {
            //trace("Car drive");
            Sound.Play(Paths.SoundRandom("carPass", 0, 1));
            await Task.Delay(700);

            fastCar.Velocity =new Point((Random.Shared.Next(170, 220) / StaticTimer.DeltaSaconds) *3,fastCar.Velocity.Y);
            fastCarCanDrive = false;
            carTimer = new CustomLimitTimer(TimeSpan.FromSeconds(2),1, delegate ()
            {
                ResetFastCar();
                carTimer = null;
            });
        }

        bool trainMoving = false;
        void TrainStart()
        {
            trainMoving = true;
            if (trainSound.PlaybackState != NAudio.Wave.PlaybackState.Playing)
                trainSound.Play();
        }

        int curLight = 0;
        int curLightEvent = 0;
    }
}
