using Newtonsoft.Json;
using Shiny_Engine_FNF.Code;
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
    public class CharacterFile
    {
        public AnimArray[] animations;
        public string image;
        public double scale;
        public double sing_duration;
        public string healthicon;

        public double[] position;
        public double[] camera_position;

        public bool flip_x;
        public bool no_antialiasing;
        public int[] healthbar_colors;
    }

    public class AnimArray
    {
        public string anim;
        public string name;
        public int fps;
        public bool loop;
        public int[] indices;
        public int[] offsets;
    }

    public class Character : Sprite
    {
        public Dictionary<string, Point> animOffsets;
        private Dictionary<string, int[]> _sortanim;

        public bool isPlayer = false;
        public bool specialAnim = false;
        string curCharacter = "bf";

        public double singDuration = 4; //Multiplier of how long a character holds the sing pose
        public string idleSuffix = "";
        public bool danceIdle = false; //Character use "danceLeft" and "danceRight" instead of "idle"
        public bool stunned = false;

        public const string DEFAULT_CHARACTER = "bf";

        public bool hasMissAnimations = false;

        //Used on Character Editor
        public string imageFile = "";
        public double jsonScale = 1;
        public bool noAntialiasing = false;
        public bool originalFlipX = false;
        public int[] healthColorArray = new int[] { 255, 0, 0 };

        public string healthIcon = "face";
        public AnimArray[] animationsArray = Array.Empty<AnimArray>();
        public TweenColor colorTween;

        public Point positionArray = new();
        public Point cameraPosition = new();

        public bool debugMode = false;


        public double holdTimer = 0;
        public double heyTimer = 0;

        public Character(string character, bool isPlayer = false) : base()
        {
            animOffsets = new Dictionary<string, Point>();
            _sortanim = new Dictionary<string, int[]>();
            IsPlayer = isPlayer;
            CurrentCharcter = character;
            //OpacityMask = Brushes.Black;
        }

        protected override void Update()
        {
            if (!debugMode && Frames.CurrentFriendlyAnimationName != null)
            {
                if (heyTimer > 0)
                {
                    heyTimer -= 1 / StaticTimer.MaxFrameRate;
                    if (heyTimer <= 0)
                    {
                        if (specialAnim && Frames.CurrentFriendlyAnimationName == "hey" || Frames.CurrentFriendlyAnimationName == "cheer")
                        {
                            specialAnim = false;
                            Dance();
                        }
                        heyTimer = 0;
                    }
                }
                else if (specialAnim && !Frames.IsPlayingAmimation)
                {
                    specialAnim = false;
                    Dance();
                }

                if (!isPlayer)
                {
                    if (Frames.CurrentFriendlyAnimationName.StartsWith("sing"))
                        holdTimer += StaticTimer.DeltaSaconds;

                    if (holdTimer >= Conductor.stepCrochet * 0.001 * singDuration)
                    {
                        Dance();
                        holdTimer = 0;
                    }
                }

                if (!Frames.IsPlayingAmimation && Frames.AnimationChase.ContainsKey(Frames.CurrentFriendlyAnimationName + "-loop"))
                {
                    Frames.PlayAnimation2(Frames.CurrentFriendlyAnimationName + "-loop", false, false);
                }
            }
            base.Update();
        }

        public Character() : this("bf", false)
        {

        }
        public string CurrentCharcter
        {
            get => curCharacter;
            set
            {
                curCharacter = value;
                _sortanim.Clear();

                switch (curCharacter)
                {
                    //case "your character name in case you want to hardcode them instead":

                    default:
                        string characterPath = "characters\\" + curCharacter + ".json";

                        var path = Paths.ModFolders(characterPath);
                        if (!File.Exists(path))
                        {
                            path = Paths.GetPreloadPath(characterPath);
                        }

                        if (!File.Exists(path))
                        {
                            path = Paths.GetPreloadPath("characters\\" + DEFAULT_CHARACTER + ".json"); //If a character couldn"t be found, change him to BF just to prevent a crash
                        }
                        var rawJson = File.ReadAllText(path);

                        CharacterFile json = JsonConvert.DeserializeObject<CharacterFile>(rawJson);
                        var spriteType = "sparrow";
                        //sparrow
                        //packer
                        //texture
                        var escpedfile = json.image.Replace('/', '\\'); // url errors
                        var modTxtToFind = Paths.ModFolders(escpedfile + ".txt");
                        var txtToFind = Paths.GetPath("images\\" + escpedfile + ".txt", null);

                        //var modTextureToFind:String = Paths.modFolders("images/"+json.image);
                        //var textureToFind:String = Paths.getPath("images/" + json.image, new AssetType();

                        if (File.Exists(modTxtToFind) || File.Exists(txtToFind)) spriteType = "packer";


                        var modAnimToFind = Paths.ModFolders("images\\" + escpedfile + "\\Animation.json");
                        var animToFind = Paths.GetPath("images\\" + escpedfile + "\\Animation.json", null);

                        //var modTextureToFind:String = Paths.modFolders("images/"+json.image);
                        //var textureToFind:String = Paths.getPath("images/" + json.image, new AssetType();

                        if (File.Exists(modAnimToFind) || File.Exists(animToFind))
                        {
                            spriteType = "texture";
                        }

                        switch (spriteType)
                        {

                            case "packer":
                                Frames = Paths.GetPackerAtlas(escpedfile, this);
                                break;

                            case "sparrow":
                                Frames = Paths.GetSparrowAtlas(escpedfile, this);
                                break;

                            case "texture":
                                //Frames = AtlasFrameMaker.construct(escpedfile);
                                break;
                        }

                        imageFile = escpedfile;

                        if (json.scale != 1)
                        {
                            jsonScale = json.scale;
                            Scale = new Vector(jsonScale, jsonScale);
                        }

                        positionArray = new Point(json.position[0], json.position[1]);
                        cameraPosition = new Point(json.camera_position[0], json.camera_position[1]);

                        healthIcon = json.healthicon;
                        singDuration = json.sing_duration;
                        FlipX = json.flip_x;
                        if (json.no_antialiasing)
                            noAntialiasing = true;

                        if (json.healthbar_colors != null && json.healthbar_colors.Length > 2)
                            healthColorArray = json.healthbar_colors;

                        animationsArray = json.animations;
                        /*foreach(var animation in json.animations) // bug fix
                        { 
                            if (animation.name.EndsWith('0'))
                                animation.name = animation.name.TrimEnd('0');
                        }*/
                        if (animationsArray != null && animationsArray.Length > 0)
                        {
                            foreach (var anim in animationsArray)
                            {
                                string animAnim = anim.anim;
                                string animName = anim.name;
                                int animFps = anim.fps;
                                bool animLoop = anim.loop;
                                int[] animIndices = anim.indices;
                                if (animIndices != null && animIndices.Any())
                                    Frames.AddByIndices(animAnim, animName, animIndices);
                                else Frames.AddByPrefix(animAnim, animName);

                                if (anim.offsets != null && anim.offsets.Length > 1)
                                {
                                    if (animOffsets.ContainsKey(anim.anim))
                                        animOffsets[animAnim] = new(anim.offsets[0], anim.offsets[1]);
                                    else animOffsets.Add(animAnim, new(anim.offsets[0], anim.offsets[1]));
                                }
                            }
                        }
                        else Frames.AddByPrefix("idle", "BF idle dance");
                        //trace("Loaded file to character " + curCharacter);
                        break;
                }
                originalFlipX = FlipX;

                if (animOffsets.ContainsKey("singLEFTmiss") || animOffsets.ContainsKey("singDOWNmiss") || animOffsets.ContainsKey("singUPmiss") || animOffsets.ContainsKey("singRIGHTmiss")) hasMissAnimations = true;
                RecalculateDanceIdle();
                Dance();

                if (isPlayer)
                {
                    FlipX = !originalFlipX;

                    /*// Doesn"t flip for BF, since his are already in the right place???
                    if (!curCharacter.startsWith("bf"))
                    {
                        // var animArray
                        if(animation.getByName("singLEFT") != null && animation.getByName("singRIGHT") != null)
                        {
                            var oldRight = animation.getByName("singRIGHT").frames;
                            animation.getByName("singRIGHT").frames = animation.getByName("singLEFT").frames;
                            animation.getByName("singLEFT").frames = oldRight;
                        }

                        // IF THEY HAVE MISS ANIMATIONS??
                        if (animation.getByName("singLEFTmiss") != null && animation.getByName("singRIGHTmiss") != null)
                        {
                            var oldMiss = animation.getByName("singRIGHTmiss").frames;
                            animation.getByName("singRIGHTmiss").frames = animation.getByName("singLEFTmiss").frames;
                            animation.getByName("singLEFTmiss").frames = oldMiss;
                        }
                    }*/
                }
            }
        }

        public bool IsPlayer
        {
            get => isPlayer;
            set
            {
                isPlayer = value;
                FlipX = !originalFlipX;
            }
        }


        public bool danced = false;

        ///<summary>
        ///FOR GF DANCING SHIT
        /// </summary>
        public void Dance()
        {
            if (!debugMode && !specialAnim && !Frames.IsPlayingAmimation)
            {
                if (danceIdle)
                {
                    danced = !danced;
                    var animname = (danced ? "danceRight" : "danceLeft") + idleSuffix;
                    var anim = animationsArray.Where(x => x.name == Frames.AnimationChase[animname]).FirstOrDefault();
                    Frames.Framerate = anim.fps;

                    Frames.PlayAnimation2(animname, true, false);
                }
                else if (Frames.AnimationChase.ContainsKey("idle" + idleSuffix))
                {
                    var anim = GetAnimBy("idle" + idleSuffix);
                    Frames.Framerate = anim.fps;
                    Frames.PlayAnimation2("idle" + idleSuffix, true, false);
                }
            }
        }

        private AnimArray GetAnimBy(string str)
        {
            var animstr = animationsArray.Select(x => x.name)
                .FindCloseString(Frames.AnimationChase[str]);
            return animationsArray.SingleOrDefault(x => x.name == animstr && x.anim == str); // עשייה
        }

        public void PlayAnim(string AnimName, bool Force = false, bool Reversed = false, int Frame = 0)
        {
            specialAnim = false;

            var anim = GetAnimBy(AnimName);
            if (Frame <= 0) Frames.Framerate = anim.fps;
            else Frames.Framerate = Frame;
            if (Reversed) Frames.ReverseAnimation2(AnimName);
            Frames.PlayAnimation2(AnimName, Force, false);
            animOffsets.TryGetValue(AnimName, out Point daOffset);
            Offset = daOffset;
            if (curCharacter.StartsWith("gf"))
            {
                if (AnimName == "singLEFT") danced = true;
                else if (AnimName == "singRIGHT") danced = false;

                if (AnimName == "singUP" || AnimName == "singDOWN") danced = !danced;
            }
        }

        public void RecalculateDanceIdle()
        {
            danceIdle = Frames.AnimationChase.ContainsKey("danceLeft" + idleSuffix) && Frames.AnimationChase.ContainsKey("danceRight" + idleSuffix);
        }

        public void AddOffset(string name, double x = 0, double y = 0)
        {
            animOffsets[name] = new(x, y);
        }

        public string GetAnimationName(string realanimname)
        {
            int inm = 0;
            _ = Frames.AnimationChase.Values.Where((x, i) =>
              {
                  if (x == realanimname)
                      inm = i;
                  return false;
              });
            return Frames.AnimationChase.Keys.ToArray()[inm];
        }
    }
}
