using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Debugging;
using Shiny_Engine_FNF.Code.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;
using WpfGame;
using Shiny_Engine_FNF.Code.FridayNightFunkinDataBase;

namespace Shiny_Engine_FNF.Code
{
    public sealed class FunkinLua
    {
        Script luaGlobal;
        DynValue luaFunction;
        public string scriptName = "";
        public static int Function_Stop = 1;
        public static int Function_Continue = 0;
        BreakAfterManyInstructionsDebugger _debugger;

        public FunkinLua(string script)
        {
            luaGlobal = new Script();
            luaFunction = luaGlobal.LoadFile(script);
            scriptName = script;

            Trace.WriteLine("Lua file loaded succesfully:" + script);

            // Lua shit
            Set("Function_Stop", Function_Stop);
            Set("Function_Continue", Function_Continue);
            Set("luaDebugMode", false);
            Set("luaDeprecatedWarnings", true);
            Set("inChartEditor", false);

            // Song/Week shit
            Set("curBpm", Conductor.bpm);
            Set("bpm", PlayState.SONG.bpm);
            Set("scrollSpeed", PlayState.SONG.speed);
            Set("crochet", Conductor.crochet);
            Set("stepCrochet", Conductor.stepCrochet);
            Set("songLength", 0);
            Set("songName", PlayState.SONG.song);
            Set("startedCountdown", false);

            Set("isStoryMode", PlayState.isStoryMode);
            Set("difficulty", PlayState.storyDifficulty);
            Set("difficultyName", CoolUtil.defaultDifficulties[PlayState.storyDifficulty]);
            Set("weekRaw", PlayState.storyWeek);
            Set("week", WeekData.weeksList[PlayState.storyWeek]);
            Set("seenCutscene", PlayState.seenCutscene);

            // Block require and os, Should probably have a proper function but this should be good enough for now until someone smarter comes along and recreates a safe version of the OS library
            Set("require", false);
            Set("os", false);

            // Camera poo
            Set("cameraX", PlayState.Instance.camGame.Margin.Left);
            Set("cameraY", PlayState.Instance.camGame.Margin.Top);

            // Screen stuff
            Set("screenWidth", Display.DefaultWidth);
            Set("screenHeight", Display.DefaultHeight);

            // PlayState cringe ass nae nae bullcrap
            Set("curBeat", 0);
            Set("curStep", 0);

            Set("score", 0);
            Set("misses", 0);
            Set("hits", 0);

            Set("rating", 0);
            Set("ratingName", "");
            Set("ratingFC", "");
            Set("version", "0");

            Set("inGameOver", false);
            Set("mustHitSection", false);
            Set("altAnim", false);
            Set("gfSection", false);

            // Gameplay settings
            Set("healthGainMult", PlayState.Instance.healthGain);
            Set("healthLossMult", PlayState.Instance.healthLoss);
            Set("instakillOnMiss", PlayState.Instance.instakillOnMiss);
            Set("botPlay", PlayState.Instance.cpuControlled);
            Set("practice", PlayState.Instance.practiceMode);

            for (int i = 0; i < 4; i++)
            {
                Set("defaultPlayerStrumX" + i, 0);
                Set("defaultPlayerStrumY" + i, 0);
                Set("defaultOpponentStrumX" + i, 0);
                Set("defaultOpponentStrumY" + i, 0);
            }

            // Default character positions woooo
            Set("defaultBoyfriendX", PlayState.Instance.BF_X);
            Set("defaultBoyfriendY", PlayState.Instance.BF_Y);
            Set("defaultOpponentX", PlayState.Instance.DAD_X);
            Set("defaultOpponentY", PlayState.Instance.DAD_Y);
            Set("defaultGirlfriendX", PlayState.Instance.GF_X);
            Set("defaultGirlfriendY", PlayState.Instance.GF_Y);

            // Character shit
            Set("boyfriendName", PlayState.SONG.player1);
            Set("dadName", PlayState.SONG.player2);
            Set("gfName", PlayState.SONG.player3);

            // Some settings, no jokes
            Set("downscroll", Settings.Default.DownScroll);
            Set("middlescroll", Settings.Default.MiddleScroll);
            Set("framerate", Settings.Default.Framerate);
            Set("ghostTapping", Settings.Default.GhostTapping);
            Set("hideHud", Settings.Default.HideHud);
            Set("timeBarType", Settings.Default.TimeBarType);
            Set("scoreZoom", Settings.Default.ScoreZoom);
            Set("cameraZoomOnBeat", Settings.Default.CamZooms);
            Set("flashingLights", Settings.Default.Flashing);
            Set("noteOffset", Settings.Default.NoteOffset);
            Set("healthBarAlpha", Settings.Default.HealthBarAlpha);
            Set("noResetButton", Settings.Default.NoReset);
            Set("lowQuality", Settings.Default.LowQuality);

            var addluascript = delegate (string luaFile, bool ignoreAlreadyRunning)
            {
                var cervix = luaFile + ".lua";
                var doPush = false;
                if (File.Exists(Paths.ModFolders(cervix)))
                {
                    cervix = Paths.ModFolders(cervix);
                    doPush = true;
                }
                else
                {
                    cervix = Paths.GetPreloadPath(cervix);
                    if (File.Exists(cervix))
                    {
                        doPush = true;
                    }
                }

                if (doPush)
                {
                    if (!ignoreAlreadyRunning)
                    {
                        foreach (var luaInstance in PlayState.Instance.luaArray)
                        {
                            if (luaInstance.scriptName == cervix)
                            {
                                luaTrace("The script \"" + cervix + "\" is already running!");
                                return;
                            }
                        }
                    }
                    PlayState.Instance.luaArray.Add(new FunkinLua(cervix));
                    return;
                }
                luaTrace("Script doesnt exist!");
            };

            AddCallback("addLuaScript", (string luaFile) => addluascript(luaFile, false));
            AddCallback("addLuaScript", addluascript);
        }

        public void luaTrace(string text, bool ignoreCheck = false, bool deprecated = false)
        {
            if (ignoreCheck || getBool("luaDebugMode"))
            {
                if (deprecated && !getBool("luaDeprecatedWarnings"))
                {
                    return;
                }
                PlayState.Instance.AddTextToDebug(text);
                Trace.WriteLine(text);
            }
        }

        public bool getBool(string variable)
        {
            string result = (string)luaGlobal.Globals[variable];

            if (result == null)
            {
                return false;
            }

            // YES! FINALLY IT WORKS
            //trace('variable: ' + variable + ', ' + result);
            return result == "true";
        }

        public void Set(string variable, object data)
        {
            if (luaGlobal == null) return;
            luaGlobal.Globals.Set(variable, UserData.Create(data));
        }

        private void AddCallback(string name, Delegate clallback)
        {
            luaGlobal.Globals[name] = clallback;
        }

        public object Call(string event1, params object[] args)
        {
            try
            {
                if (luaGlobal == null) return Function_Continue;

                luaGlobal.AttachDebugger(_debugger);

                var function = luaGlobal.Globals.Get(event1);
                var result = luaGlobal.Call(function, args);

                if (result != null && resultIsAllowed(result))
                    return result.ToObject();
            }
            catch
            {
                Trace.WriteLine("Script stopped");
            }
            return Function_Continue;
        }

        bool resultIsAllowed(DynValue value)
        { //Makes it ignore warnings
            return value.Type switch
            {
                DataType.Nil | DataType.Boolean | DataType.Number | DataType.String | DataType.Table | DataType.Void => true,
                _ => false,
            };
        }

        public void Stop()
        {
            if (luaGlobal == null) return;

            //if (accessedProps != null) accessedProps.clear();

            _debugger.Stop();
            luaGlobal = null;
            _debugger = null;
        }
    }
    public class ModchartSprite : Border
    {
        public ModchartSprite()
        {
            VerticalAlignment = System.Windows.VerticalAlignment.Top;
            HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
        }
        public bool wasAdded = false;
        //public var isInFront:Bool = false;
    }
    public class MyException : Exception
    {

    }

    class BreakAfterManyInstructionsDebugger : IDebugger
    {
        int m_InstructionCounter = 0;
        readonly List<DynamicExpression> m_Dynamics = new();
        bool _stop;

        public void Stop()
        {
            _stop = true;
        }

        public void SetSourceCode(SourceCode sourceCode)
        {
        }

        public void SetByteCode(string[] byteCode)
        {
        }

        public bool IsPauseRequested()
        {
            return true;
        }

        public bool SignalRuntimeException(ScriptRuntimeException ex)
        {
            return false;
        }

        public DebuggerAction GetAction(int ip, SourceRef sourceref)
        {
            m_InstructionCounter += 1;

            if (m_InstructionCounter % 1000 == 0)
                Console.Write(".");

            if (m_InstructionCounter > 50000 || _stop)
                throw new MyException();

            return new DebuggerAction()
            {
                Action = DebuggerAction.ActionType.StepIn,
            };
        }

        public void SignalExecutionEnded()
        {
        }

        public void Update(WatchType watchType, IEnumerable<WatchItem> items)
        {
        }

        public List<DynamicExpression> GetWatchItems()
        {
            return m_Dynamics;
        }

        public void RefreshBreakpoints(IEnumerable<SourceRef> refs)
        {
        }

        public DebuggerCaps GetDebuggerCaps()
        {
            return DebuggerCaps.CanDebugSourceCode;
        }

        public void SetDebugService(DebugService debugService)
        {

        }
    }
}
