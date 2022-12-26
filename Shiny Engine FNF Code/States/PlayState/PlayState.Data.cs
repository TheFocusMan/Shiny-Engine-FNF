using Newtonsoft.Json.Linq;
using Shiny_Engine_FNF.Code.Controls;
using Shiny_Engine_FNF.Code.FridayNightFunkinDataBase;
using System;
using System.Collections.Generic;
using System.Windows;
using WpfGame;
using WpfGame.Controls;

namespace Shiny_Engine_FNF.Code
{
    public partial class PlayState
    {
        public static readonly ValueTuple<string, double>[] ratingStuff = new ValueTuple<string, double>[]
        {
            new ValueTuple<string,double>("You Suck!", 0.2), //From 0% to 19%
            new ValueTuple<string,double>("Shit", 0.4), //From 20% to 39%
            new ValueTuple<string,double>("Bad", 0.5), //From 40% to 49%
            new ValueTuple<string,double>("Bruh", 0.6), //From 50% to 59%
            new ValueTuple<string,double>("Meh", 0.69), //From 60% to 68%
            new ValueTuple<string,double>("Nice", 0.7), //69%
            new ValueTuple<string,double>("Good", 0.8), //From 70% to 79%
            new ValueTuple<string,double>("Great", 0.9), //From 80% to 89%
            new ValueTuple<string,double>("Sick!", 1), //From 90% to 99%
            new ValueTuple<string,double>("Perfect!!", 1) //The value on this one isn"t used actually, since Perfect is always "1"
        };

        #region Lua Shit
        public static PlayState Instance;
        public List<FunkinLua> luaArray = new();
        public String introSoundsSuffix = "";
        #endregion

        #region Song Data
        public static string curStage = "";
        public static bool isPixelStage = false;
        public static SwagSong SONG = default;
        public static bool isStoryMode = false;
        public static int storyWeek = 0;
        public static List<string> storyPlaylist = new();
        public static int storyDifficulty = 1;
        #endregion

        #region Story Mode Data
        public static int campaignScore = 0;
        public static int campaignMisses = 0;
        public static bool seenCutscene = false;
        public static int deathCounter = 0;
        #endregion

        #region  Discord RPC variables
        string storyDifficultyText = "";
        string detailsText = "";
        string detailsPausedText = "";
        #endregion

        #region Week 2
        BGSprite halloweenBG;
        BGSprite halloweenWhite;
        #endregion

        #region Week 3
        CanvasWithMargarin phillyCityLights;
        BGSprite phillyTrain;
        ModchartSprite blammedLightsBlack;
        Tween blammedLightsBlackTween;
        CanvasWithMargarin phillyCityLightsEvent;
        Tween phillyCityLightsEventTween;
        FlxSound trainSound;
        #endregion

        #region Week 4
        int limoKillingState = 0;
        BGSprite limo;
        BGSprite limoMetalPole;
        BGSprite limoLight;
        BGSprite limoCorpse;
        BGSprite limoCorpseTwo;
        BGSprite bgLimo;
        CanvasWithMargarin grpLimoParticles;
        CanvasWithMargarin grpLimoDancers;
        BGSprite fastCar;
        #endregion

        #region Week 5
        BGSprite upperBoppers;
        BGSprite bottomBoppers;
        BGSprite santa;
        double heyTimer;
        #endregion

        #region Week 6
        BackgroundGirls bgGirls;
        //WiggleEffect wiggleShit = new WiggleEffect();
        BGSprite bgGhouls;
        #endregion

        #region Defualt Charcter Positions
        public double BF_X = 770;
        public double BF_Y = 100;
        public double DAD_X = 100;
        public double DAD_Y = 100;
        public double GF_X = 400;
        public double GF_Y = 130;
        #endregion

        #region ModChart Data
        public Dictionary<string, ModchartSprite> modchartSprites = new();
        public Dictionary<string, Tween> modchartTweens = new();
        public Dictionary<string, CustomLimitTimer> modchartTimers = new();
        #endregion

        #region Gameplay settings
        public double healthGain = 1;
        public double healthLoss = 1;
        public bool instakillOnMiss = false;
        public bool cpuControlled = false;
        public bool practiceMode = false;
        #endregion

        #region Charchters 
        public Character dad;
        public Character gf;
        public Boyfriend boyfriend;
        CanvasWithMargarin dadGroup;
        CanvasWithMargarin gfGroup;
        CanvasWithMargarin boyfriendGroup;
        #endregion

        #region Dialog
        String[] dialogue = { "blah blah blah", "coolswag" };
        DialogueFile dialogueJson = null;
        #endregion

        #region Defualt Controls
        AttachedSprite timeBarBG;
        private AttachedSprite healthBarBG;
        double songPercent = 0;
        #endregion

        #region Song Actions value
        private bool generatedMusic = false;
        public bool endingSong = false;
        private bool startingSong = false;
        private bool updateTime = true;
        public static bool changedDifficulty = false;
        public static bool chartingMode = false;
        #endregion

        #region Other Stats
        public int gfSpeed = 1;
        public double health = 1;
        public int combo = 0;
        #endregion

        #region Song Speed vars
        public Tween songSpeedTween;
        private double _songSpeed = 1;
        public double SongSpeed
        {
            get => _songSpeed;
            set
            {
                if (generatedMusic)
                {
                    var ratio = value / _songSpeed; //funny word huh
                    foreach (Note note in notes.Children)
                    {
                        if (note.isSustainNote && !note.Frames.CurrentAnimationName.EndsWith("end"))
                        {
                            note.ScaleY *= ratio;
                            //note.UpdateHitbox();
                        }
                    }
                    foreach (Note note in unspawnNotes)
                    {
                        if (note.isSustainNote && !note.Frames.CurrentAnimationName.EndsWith("end"))
                        {
                            note.ScaleY *= ratio;
                            //note.UpdateHitbox();
                        }
                    }
                }
                _songSpeed = value;
                noteKillOffset = 350 / _songSpeed;
            }
        }
        public string songSpeedType = "multiplicative";
        public double noteKillOffset = 350;
        #endregion

        #region Notes Var
        public CanvasWithMargarin notes;
        public List<Note> unspawnNotes = new();
        public JArray eventNotes = new();
        #endregion

        #region event variables
        private bool isCameraOnForcedPos = false;
        public Dictionary<string, Boyfriend> boyfriendMap = new();
        public Dictionary<string, Character> dadMap = new();
        public Dictionary<string, Character> gfMap = new();
        #endregion

        #region Notes Status
        public List<StrumNote> opponentStrums;
        public List<StrumNote> playerStrums;
        #endregion

        public FlxSound vocals;
        public FlxSound instumental;

        #region Visual Vars
        public static double daPixelZoom = 6;
        private string[] singAnimations = new string[] { "singLEFT", "singDOWN", "singUP", "singRIGHT" };

        public bool inCutscene = false;
        public bool skipCountdown = false;
        double songLength = 0;

        public Point boyfriendCameraOffset =new(0,0);
	    public Point opponentCameraOffset =new(0,0);
	    public Point girlfriendCameraOffset = new(0,0);
        #endregion

        #region Health Bar Vars
        public HealthIcon iconP1;
        public HealthIcon iconP2;
        #endregion

        #region On Time Vars
        public bool paused = false;
        bool startedCountdown = false;
        bool canPause = false;
        double limoSpeed = 0;
        #endregion

        #region Song Stats
        public int songScore = 0;
        public int songHits = 0;
        public int songMisses = 0;
        #endregion

        #region Rating Vars
        public string ratingName = "?";
        public double ratingPercent;
        public string ratingFC;
        #endregion

        #region Player Play Counter
        public int totalPlayed = 0;
        public double totalNotesHit = 0.0;
        #endregion

        #region Gameplay Stats Player
        public int sicks = 0;
        public int goods = 0;
        public int bads = 0;
        public int shits = 0;
        #endregion

        #region Achievement shit
        List<bool> keysPressed = new();
        double boyfriendIdleTime = 0.0;
        protected bool boyfriendIdled = false;
        #endregion

        #region Update Shit
        int previousFrameTime = 0;
        protected int lastReportedPlayheadPosition = 0;
        double songTime = 0;
        #endregion

        Tween scoreTxtTween;

        public double botplaySine = 0;

        public double cameraSpeed = 1;

        public bool camZooming = false;
        private string curSong = "";
    }
}
