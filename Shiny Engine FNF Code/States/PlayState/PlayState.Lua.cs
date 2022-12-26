using Shiny_Engine_FNF.Code;
using Shiny_Engine_FNF.Code.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfGame.Controls;

namespace Shiny_Engine_FNF.Code
{
    public partial class PlayState
    {
        private void SkipDialogue(object sender, EventArgs e)
        {
            CallOnLuas("onSkipDialogue", dialogueCount);
        }

        void StartCharacterLua(string name)
        {
            var doPush = false;
            var luaFile = "characters\\" + name + ".lua";
            if (File.Exists(Paths.ModFolders(luaFile)))
            {
                luaFile = Paths.ModFolders(luaFile);
                doPush = true;
            }
            else
            {
                luaFile = Paths.GetPreloadPath(luaFile);
                if (File.Exists(luaFile))
                    doPush = true;
            }

            if (doPush)
            {
                foreach (var lua in luaArray)
                    if (lua.scriptName == luaFile) return;
                luaArray.Add(new FunkinLua(luaFile));
            }

        }

        public void AddCharacterToList(string newCharacter, int type)
        {
            switch (type)
            {
                case 0:
                    if (!boyfriendMap.ContainsKey(newCharacter))
                    {
                        var newBoyfriend = new Boyfriend() { X = 0, Y = 0, CurrentCharcter = newCharacter };
                        boyfriendMap.TryAdd(newCharacter, newBoyfriend);
                        boyfriendGroup.Children.Add(newBoyfriend);
                        StartCharacterPos(newBoyfriend);
                        newBoyfriend.Opacity = 0.00001;
                        StartCharacterLua(newBoyfriend.CurrentCharcter);
                    }
                    break;
                case 1:
                    if (!dadMap.ContainsKey(newCharacter))
                    {
                        var newDad = new Character(newCharacter) { X = 0, Y = 0 };
                        dadMap.TryAdd(newCharacter, newDad);
                        dadGroup.Children.Add(newDad);
                        StartCharacterPos(newDad, true);
                        newDad.Opacity = 0.00001;
                        StartCharacterLua(newDad.CurrentCharcter);
                    }
                    break;
                case 2:
                    if (!gfMap.ContainsKey(newCharacter))
                    {
                        var newGf = new Character(newCharacter) { X = 0, Y = 0 };
                        //newGf.scrollFactor.set(0.95, 0.95);
                        gfMap.TryAdd(newCharacter, newGf);
                        gfGroup.Children.Add(newGf);
                        StartCharacterPos(newGf);
                        newGf.Opacity = 0.00001;
                        StartCharacterLua(newGf.CurrentCharcter);
                    }
                    break;
            }
        }

        public void RemoveLua(FunkinLua lua)
        {
            if (luaArray != null && !preventLuaRemove)
            {
                luaArray.Remove(lua);
            }
        }

        public List<FunkinLua> closeLuas = new();
        public object CallOnLuas(string event1, params object[] args)
        {
            object returnVal = FunkinLua.Function_Continue;
            for (int i = 0; i < luaArray.Count; i++)
            {
                var ret = luaArray[i].Call(event1, args);
                if (!ret.Equals(FunkinLua.Function_Continue))
                    returnVal = ret;
            }

            for (int i = 0; i < closeLuas.Count; i++)
            {
                luaArray.Remove(closeLuas[i]);
                closeLuas[i].Stop();
            }
            return returnVal;
        }

        private void StartNextDialogue(object sender, EventArgs e)
        {
            dialogueCount++;
            CallOnLuas("onSkipDialogue", dialogueCount);
        }

        public void AddTextToDebug(string text)
        {
            if (luaDebugGroup.Children.Count > 34)
                luaDebugGroup.Children.Remove(luaDebugGroup.Children[34]);
            luaDebugGroup.Children.Insert(0, new OutlinedTextBlock { Text = (text), FontSize = 20 });
        }

        public void SetOnLuas(string variable, object arg)
        {
            for (int i = 0; i < luaArray.Count; i++)
            {
                luaArray[i].Set(variable, arg);
            }
        }
    }
}
