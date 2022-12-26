using Shiny_Engine_FNF.FridayNightFunkinDataBase;
using System.Windows.Input;
using WpfGame;

namespace Shiny_Engine_FNF.Code.FridayNightFunkinDataBase
{
    internal static class PlayerSttings
    {
        static Key[] _accept;
        static Key[] _pause;
        static Key[][] _notesKeyBinds;
        static Key[][] _uiKeys;

        public static bool KeyAcceptPressed()
        {
            _acceptKeyPressed.Check();
            return _acceptKeyPressed.IsPressed && _acceptKeyPressed.CountedTimes == 1;
        }

        internal static KeyNotifyEvent _acceptKeyPressed;
        internal static KeyNotifyEvent _uiKeydownPress;
        internal static KeyNotifyEvent _uiKeyupPress;


        public static bool KeyPressed(KeyNotifyEvent keyNotify)
        {
            keyNotify.Check();
            return keyNotify.IsPressed && keyNotify.CountedTimes == 1;
        }

        public static Key[] Accept { get => _accept; set => _accept = value; }
        public static Key[] Pause { get => _pause; set => _pause = value; }
        /// <summary>
        /// keys
        /// 0 left
        /// 1 down
        /// 2 up
        /// 3 right
        /// </summary>
        public static Key[][] NotesKeyBinds { get => _notesKeyBinds; set => _notesKeyBinds = value; }
        /// <summary>
        /// keys
        /// 0 left
        /// 1 down
        /// 2 up
        /// 3 right
        /// </summary>
        public static Key[][] UiKeys { get => _uiKeys; set => _uiKeys = value; }

        static PlayerSttings()
        {
            _accept = ConvertToKeyArray(KeybindsSettings.Default.KeyBindsAccept);
            _notesKeyBinds = new Key[][] {
                ConvertToKeyArray(KeybindsSettings.Default.KeyBindsLeft),
                ConvertToKeyArray(KeybindsSettings.Default.KeyBindsDown),
                ConvertToKeyArray(KeybindsSettings.Default.KeyBindsUp),
                ConvertToKeyArray(KeybindsSettings.Default.KeyBindsRight)
            };
            _uiKeys = new Key[][] {
                 ConvertToKeyArray(KeybindsSettings.Default.KeyBindsUILeft),
                ConvertToKeyArray(KeybindsSettings.Default.KeyBindsUIDown),
                ConvertToKeyArray(KeybindsSettings.Default.KeyBindsUIUp),
                ConvertToKeyArray(KeybindsSettings.Default.KeyBindsUIRight)
            };
            _pause = ConvertToKeyArray(KeybindsSettings.Default.KeyBindsPause);
            _acceptKeyPressed = new(_accept);
            _uiKeydownPress = new(_uiKeys[1]);
            _uiKeyupPress = new(_uiKeys[2]);
        }

        private static Key[] ConvertToKeyArray(long arr)
        {
            var arr1 = Mathf.ConvertToHighLow(arr);
            return new Key[] { (Key)arr1[0], (Key)arr1[1] };
        }
    }

    internal class KeyNotifyEvent
    {
        public bool IsPressed { get; private set; }

        public int CountedTimes { get; private set; }

        private Key[] _keys;

        public KeyNotifyEvent(Key[] keys)
        {
            _keys = keys;
        }

        public void Press()
        {
            IsPressed = true;
            CountedTimes++;
        }

        public void Unpress()
        {
            IsPressed = false;
            CountedTimes = 0;
        }


        public void Check()
        {
            for (int i = 0; i < _keys.Length; i++)
            {
                if (Keyboard.IsKeyDown(_keys[i]))
                {
                    Press();
                    return;
                }
            }
            Unpress();
        }
    }
}
