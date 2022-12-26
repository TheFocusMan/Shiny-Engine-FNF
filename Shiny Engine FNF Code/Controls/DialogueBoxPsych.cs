using Newtonsoft.Json;
using Shiny_Engine_FNF.Code.FridayNightFunkinDataBase;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using WpfGame;
using WpfGame.Controls;

namespace Shiny_Engine_FNF.Code.Controls
{
    class DialogueCharacterFile
    {
        public string image = null;
        public string dialogue_pos = null;

        public DialogueAnimArray[] animations = Array.Empty<DialogueAnimArray>();
        public double[] position = Array.Empty<double>();
        public double scale = 0;
    }

    class DialogueAnimArray
    {
        public string anim = null;
        public string loop_name = null;
        public int[] loop_offsets = Array.Empty<int>();
        public string idle_name = null;
        public int[] idle_offsets = Array.Empty<int>();
    }

    // Gonna try to kind of make it compatible to Forever Engine,
    // love u Shubs no homo :flushedh4:
    public class DialogueFile
    {
        public DialogueLine[] dialogue;
    }

    public class DialogueLine
    {
        public string portrait;
        public string expression;
        public string text;
        public string boxState;
        public double speed;
        //var skipdelay:Null<Int>;
        //var append:Null<Bool>; //thinkin bout having some rpg type text shit.
    }
    class DialogueCharacter : Sprite
    {
        private static string IDLE_SUFFIX = "-IDLE";
        public static string DEFAULT_CHARACTER = "bf";
        public static double DEFAULT_SCALE = 0.7;

        public DialogueCharacterFile jsonFile = null;
        public Dictionary<string, DialogueAnimArray> dialogueAnimations = new Dictionary<string, DialogueAnimArray>();


        public double startingPos = 0; //For center characters, it works as the starting Y, for everything else it works as starting X
        public bool isGhost = false; //For the editor
        public string curCharacter = "bf";
        public int skiptimer = 0;
        public int skipping = 0;
        public DialogueCharacter(double x = 0, double y = 0, string character = null)
        {
            X = x;
            Y = y;

            if (character == null) character = DEFAULT_CHARACTER;
            curCharacter = character;

            ReloadCharacterJson(character);
            Frames = Paths.GetSparrowAtlas("dialogue\\" + jsonFile.image, this, null, 24);
            reloadAnimations();
        }

        public void ReloadCharacterJson(string character)
        {
            var characterPath = "images\\dialogue\\" + character + ".json";
            var path = Paths.ModFolders(characterPath);
            if (!File.Exists(path))
            {
                path = Paths.GetPreloadPath(characterPath);
            }

            if (!File.Exists(path))
            {
                path = Paths.GetPreloadPath("images\\dialogue\\" + DEFAULT_CHARACTER + ".json");
            }
            string rawJson = File.ReadAllText(path);
            jsonFile = JsonConvert.DeserializeObject<DialogueCharacterFile>(rawJson);
        }

        public void reloadAnimations()
        {
            dialogueAnimations.Clear();
            if (jsonFile.animations != null && jsonFile.animations.Length > 0)
            {
                foreach (var anim in jsonFile.animations)
                {
                    Frames.AddByPrefix(anim.anim, anim.loop_name);
                    Frames.AddByPrefix(anim.anim + IDLE_SUFFIX, anim.idle_name);
                    dialogueAnimations.Add(anim.anim, anim);
                }
            }
        }

        public void PlayAnim(string animName = null, bool playIdle = false)
        {
            var leAnim = animName;
            if (animName == null || !dialogueAnimations.ContainsKey(animName))
            { //Anim is null, get a random animation
                var arrayAnims = dialogueAnimations.Select(x => x.Key).ToArray();
                if (arrayAnims.Length > 0)
                    leAnim = arrayAnims[new Random().Next(0, arrayAnims.Length - 1)];
            }

            if (dialogueAnimations.ContainsKey(leAnim) &&
            (dialogueAnimations[leAnim].loop_name == null ||
            dialogueAnimations[leAnim].loop_name.Length < 1 ||
            dialogueAnimations[leAnim].loop_name == dialogueAnimations[leAnim].idle_name))
            {
                playIdle = true;
            }
            Frames.PlayAnimation2(playIdle ? leAnim + IDLE_SUFFIX : leAnim, false);

            if (dialogueAnimations.ContainsKey(leAnim))
            {
                var anim = dialogueAnimations[leAnim];
                if (playIdle)
                {
                    Offset = new(anim.idle_offsets[0], anim.idle_offsets[1]);
                    //trace("Setting idle offsets: " + anim.idle_offsets);
                }
                else
                {
                    Offset = new(anim.loop_offsets[0], anim.loop_offsets[1]);
                    //trace("Setting loop offsets: " + anim.loop_offsets);
                }
            }
            else
            {
                Offset = new();
                Trace.WriteLine("Offsets not found! Dialogue character is badly formatted, anim: " + leAnim + ", " + (playIdle ? "idle anim" : "loop anim"));
            }
        }

        public bool animationIsLoop()
        {
            if (Frames.CurrentAnimationName == null) return false;
            return !Frames.CurrentAnimationName.EndsWith(IDLE_SUFFIX);
        }
    }
    public class DialogueBoxPsych : CanvasWithMargarin, IUpdatable
    {
        //Alphabet dialogue;
        DialogueFile dialogueList = null;

        public event EventHandler finishThing;
        public event EventHandler NextDialogueThing;
        public event EventHandler SkipDialogueThing;
        Sprite bgFade = null;
        Sprite box;
        string textToType = "";

        List<DialogueCharacter> arrayCharacters = new();

        int currentText = 0;
        double offsetPos = -600;

        string[] textBoxTypes = new[] { "normal", "angry" };
        //var charPositionList:Array<String> = ["left", "center", "right"];

        public DialogueBoxPsych(DialogueFile dialogueList, string song = null) : base()
        {
            if (song != null && song != "") Sound.Play(Paths.Music(song));
            bgFade = new()
            {
                X = -500,
                Y = -500,
                Width = Display.DefaultWidth * 2,
                Height = Display.DefaultHeight * 2,
                Background = Brushes.White,
                Opacity = 0
            };
            Children.Add(bgFade);

            this.dialogueList = dialogueList;
            spawnCharacters();

            box = new() { X = 70, Y = 370 };
            box.Frames = Paths.GetSparrowAtlas("speech_bubble", box);
            box.Frames.Framerate = 24;
            box.Frames.AddByPrefix("normal", "speech bubble normal");
            box.Frames.AddByPrefix("normalOpen", "Speech Bubble Normal Open");
            box.Frames.AddByPrefix("angry", "AHH speech bubble");
            box.Frames.AddByPrefix("angryOpen", "speech bubble loud open");
            box.Frames.AddByPrefix("center-normal", "speech bubble middle");
            box.Frames.AddByPrefix("center-normalOpen", "Speech Bubble Middle Open");
            box.Frames.AddByPrefix("center-angry", "AHH Speech Bubble middle");
            box.Frames.AddByPrefix("center-angryOpen", "speech bubble Middle loud open");
            box.Frames.PlayAnimation2("normal", true, false);
            box.Visibility = Visibility.Hidden;
            box.Scale = new(0.9, 0.9);
            Children.Add(box);

            StartNextDialog();
        }

        //bool dialogueStarted = false;
        bool dialogueEnded = false;

        public static double LEFT_CHAR_X = -60;
        public static double RIGHT_CHAR_X = -100;
        public static double DEFAULT_CHAR_Y = 60;

        void spawnCharacters()
        {
            Dictionary<string, bool> charsMap = new();
            for (int i = 0; i < dialogueList.dialogue.Length; i++)
            {
                if (dialogueList.dialogue[i] != null)
                {
                    var charToAdd = dialogueList.dialogue[i].portrait;
                    if (!charsMap.ContainsKey(charToAdd) || !charsMap[charToAdd])
                    {
                        charsMap.Add(charToAdd, true);
                    }
                }
            }

            foreach (var individualChar in charsMap.Keys)
            {
                var x = LEFT_CHAR_X;
                var y = DEFAULT_CHAR_Y;
                DialogueCharacter char1 = new(x + offsetPos, y, individualChar);
                char1.ScaleX = char1.ScaleY = char1.Width * DialogueCharacter.DEFAULT_SCALE * char1.jsonFile.scale;
                char1.Opacity = 0.00001;
                Children.Add(char1);

                bool saveY = false;
                switch (char1.jsonFile.dialogue_pos)
                {
                    case "center":
                        char1.X = Display.DefaultWidth / 2;
                        char1.X -= char1.Width / 2;
                        y = char1.Y;
                        char1.Y = Display.DefaultHeight + 50;
                        saveY = true;
                        break;
                    case "right":
                        x = Display.DefaultWidth - char1.Width + RIGHT_CHAR_X;
                        char1.X = x - offsetPos;
                        break;
                }
                x += char1.jsonFile.position[0];
                y += char1.jsonFile.position[1];
                char1.X += char1.jsonFile.position[0];
                char1.Y += char1.jsonFile.position[1];
                char1.startingPos = saveY ? y : x;
                arrayCharacters.Add(char1);
            }
        }

        public const int DEFAULT_TEXT_X = 90;
        public const int DEFAULT_TEXT_Y = 430;
        double scrollSpeed = 4500;
        Alphabet daText = null;
        bool ignoreThisFrame = true; //First frame is reserved for loading dialogue images

        void IUpdatable.Update()
        {
            Update();
        }

        void Update()
        {
            if (ignoreThisFrame)
            {
                ignoreThisFrame = false;
                return;
            }

            if (!dialogueEnded)
            {
                bgFade.Opacity += 0.5 * StaticTimer.DeltaSaconds;
                if (bgFade.Opacity > 0.5) bgFade.Opacity = 0.5;

                if (PlayerSttings.KeyAcceptPressed())
                {
                    if (!daText.finishedText)
                    {
                        if (daText != null)
                            Children.Remove(daText);
                        daText = new Alphabet(new(DEFAULT_TEXT_X, DEFAULT_TEXT_Y), textToType, false, true, 0.0, 0.7);
                        Children.Add(daText);

                        SkipDialogueThing?.Invoke(this, EventArgs.Empty);
                    }
                    else if (currentText >= dialogueList.dialogue.Length)
                    {
                        dialogueEnded = true;
                        for (int i = 0; i < textBoxTypes.Length; i++)
                        {
                            var checkArray = new string[] { "", "center-" };
                            var animName = box.Frames.CurrentAnimationName;
                            for (int j = 0; j < checkArray.Length; j++)
                            {
                                if (animName == checkArray[j] + textBoxTypes[i] || animName == checkArray[j] + textBoxTypes[i] + "Open")
                                {
                                    box.Frames.PlayAnimation2(checkArray[j] + textBoxTypes[i] + "Open", true);
                                }
                            }
                        }

                        box.Frames.CurrentFrame = box.Frames.GetAnimationFramesCountByName(box.Frames.CurrentAnimationName) - 1;
                        box.Frames.ReverseAnimation(box.Frames.CurrentAnimationName);
                        Children.Remove(daText);
                        daText = null;
                        UpdateBoxOffsets(box);
                    }
                    else StartNextDialog();
                    Sound.Play(Paths.Sound("dialogueClose"));
                }
                else if (daText.finishedText)
                {
                    DialogueCharacter char1 = arrayCharacters[lastCharacter];
                    if (char1 != null && char1.Frames.CurrentAnimationName != null && char1.animationIsLoop() && !char1.Frames.IsPlayingAmimation)
                    {
                        char1.PlayAnim(char1.Frames.CurrentAnimationName, true);
                    }
                }
                else
                {
                    DialogueCharacter char1 = arrayCharacters[lastCharacter];
                    if (char1 != null && char1.Frames.CurrentAnimationName != null && !char1.Frames.IsPlayingAmimation)
                    {
                        char1.Frames.PlayAnimation2(char1.Frames.CurrentAnimationName, true, char1.Frames.Looping);
                    }
                }

                if (!box.Frames.IsPlayingAmimation)
                {
                    for (int i = 0; i < textBoxTypes.Length; i++)
                    {
                        var checkArray = new string[] { "", "center-" };
                        var animName = box.Frames.CurrentAnimationName;
                        for (int j = 0; j < checkArray.Length; j++)
                        {
                            if (animName == checkArray[j] + textBoxTypes[i] || animName == checkArray[j] + textBoxTypes[i] + "Open")
                                box.Frames.PlayAnimation2(checkArray[j] + textBoxTypes[i], true);
                        }
                    }
                    UpdateBoxOffsets(box);
                }

                if (lastCharacter != -1 && arrayCharacters.Count > 0)
                {
                    for (int i = 0; i < arrayCharacters.Count; i++)
                    {
                        var char1 = arrayCharacters[i];
                        if (char1 != null)
                        {
                            if (i != lastCharacter)
                            {
                                switch (char1.jsonFile.dialogue_pos)
                                {
                                    case "left":
                                        char1.X -= scrollSpeed * StaticTimer.DeltaSaconds;
                                        if (char1.X < char1.startingPos + offsetPos) char1.X = char1.startingPos + offsetPos;
                                        break;
                                    case "center":
                                        char1.Y += scrollSpeed * StaticTimer.DeltaSaconds;
                                        if (char1.Y > char1.startingPos + Display.DefaultHeight) char1.Y = char1.startingPos + Display.DefaultHeight;
                                        break;
                                    case "right":
                                        char1.X += scrollSpeed * StaticTimer.DeltaSaconds;
                                        if (char1.X > char1.startingPos - offsetPos) char1.X = char1.startingPos - offsetPos;
                                        break;
                                }
                                char1.Opacity -= 3 * StaticTimer.DeltaSaconds;
                                if (char1.Opacity < 0.00001) char1.Opacity = 0.00001;
                            }
                            else
                            {
                                switch (char1.jsonFile.dialogue_pos)
                                {
                                    case "left":
                                        char1.X += scrollSpeed * StaticTimer.DeltaSaconds;
                                        if (char1.X > char1.startingPos) char1.X = char1.startingPos;
                                        break;
                                    case "center":
                                        char1.Y -= scrollSpeed * StaticTimer.DeltaSaconds;
                                        if (char1.Y < char1.startingPos) char1.Y = char1.startingPos;
                                        break;
                                    case "right":
                                        char1.X -= scrollSpeed * StaticTimer.DeltaSaconds;
                                        if (char1.X < char1.startingPos) char1.X = char1.startingPos;
                                        break;
                                }
                                char1.Opacity += 3 * StaticTimer.DeltaSaconds;
                            }
                        }
                    }
                }
            }
            else
            { //Dialogue ending
                if (box != null && box.Frames.CurrentFrame <= 0)
                {
                    Children.Remove(box);
                    box = null;
                }

                if (bgFade != null)
                {
                    bgFade.Opacity -= 0.5 * scrollSpeed * StaticTimer.DeltaSaconds;
                    if (bgFade.Opacity <= 0)
                    {
                        Children.Remove(bgFade);
                        bgFade = null;
                    }
                }

                for (int i = 0; i < arrayCharacters.Count; i++)
                {
                    var leChar = arrayCharacters[i];
                    if (leChar != null)
                    {
                        switch (arrayCharacters[i].jsonFile.dialogue_pos)
                        {
                            case "left":
                                leChar.X -= scrollSpeed * StaticTimer.DeltaSaconds;
                                break;
                            case "center":
                                leChar.Y += scrollSpeed * StaticTimer.DeltaSaconds;
                                break;
                            case "right":
                                leChar.X += scrollSpeed * StaticTimer.DeltaSaconds;
                                break;
                        }
                        leChar.Opacity -= scrollSpeed * StaticTimer.DeltaSaconds * 10;
                    }
                }

                if (box == null && bgFade == null)
                {
                    for (int i = 0; i < arrayCharacters.Count; i++)
                    {
                        DialogueCharacter leChar = arrayCharacters[0];
                        if (leChar != null)
                        {
                            arrayCharacters.Remove(leChar);
                            Children.Remove(leChar);
                        }
                    }
                    finishThing(this, EventArgs.Empty);
                }
            }
        }

        int lastCharacter = -1;
        string lastBoxType = "";
        void StartNextDialog()
        {
            DialogueLine curDialogue;
            do
            {
                curDialogue = dialogueList.dialogue[currentText];
            } while (curDialogue == null);

            if (curDialogue.text == null || curDialogue.text.Length < 1) curDialogue.text = " ";
            if (curDialogue.boxState == null) curDialogue.boxState = "normal";
            //if (curDialogue.speed == null || Math.isNaN(curDialogue.speed)) curDialogue.speed = 0.05;

            var animName = curDialogue.boxState;
            var boxType = textBoxTypes[0];
            for (int i = 0; i < textBoxTypes.Length; i++)
            {
                if (textBoxTypes[i] == animName)
                {
                    boxType = animName;
                }
            }

            int character = 0;
            box.Visibility = Visibility.Visible;
            for (int i = 0; i < arrayCharacters.Count; i++)
            {
                if (arrayCharacters[i].curCharacter == curDialogue.portrait)
                {
                    character = i;
                    break;
                }
            }
            var centerPrefix = "";
            var lePosition = arrayCharacters[character].jsonFile.dialogue_pos;
            if (lePosition == "center") centerPrefix = "center-";

            if (character != lastCharacter)
            {
                box.Frames.PlayAnimation2(centerPrefix + boxType + "Open", true);
                UpdateBoxOffsets(box);
                box.FlipX = lePosition == "left";
            }
            else if (boxType != lastBoxType)
            {
                box.Frames.PlayAnimation2(centerPrefix + boxType, true);
                UpdateBoxOffsets(box);
            }
            lastCharacter = character;
            lastBoxType = boxType;

            if (daText != null)
                Children.Remove(daText);

            textToType = curDialogue.text;
            daText = new Alphabet(new(DEFAULT_TEXT_X, DEFAULT_TEXT_Y), textToType, false, true, curDialogue.speed, 0.7);
            Children.Add(daText);

            DialogueCharacter char1 = arrayCharacters[character];
            if (char1 != null)
            {
                char1.PlayAnim(curDialogue.expression, daText.finishedText);
                if (char1.Frames.CurrentAnimationName != null)
                {
                    double rate = 24 - (curDialogue.speed - 0.05) / 5 * 480;
                    if (rate < 12) rate = 12;
                    else if (rate > 48) rate = 48;
                    char1.Frames.Framerate = rate;
                }
            }
            currentText++;

            NextDialogueThing?.Invoke(this, EventArgs.Empty);
        }

        public static DialogueFile ParseDialogue(string path)
        {
            if (File.Exists(path))
            {
                return JsonConvert.DeserializeObject<DialogueFile>(File.ReadAllText(path));
            }
            return null;
        }

        public static void UpdateBoxOffsets(Sprite box)
        {
            //Had to make it static because of the editors
            if (box.Frames.CurrentAnimationName.StartsWith("angry")) box.Offset = new(50, 65);
            else if (box.Frames.CurrentAnimationName.StartsWith("center-angry")) box.Offset = new(50, 30);
            else box.Offset = new(10, 0);
            if (!box.FlipX) box.OffsetY += 10;
        }
    }
}
