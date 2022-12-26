using System;
using System.Windows;
using System.Windows.Controls;
using WpfGame;

namespace WpfGame.Controls
{
    public class TypeTextBlock : TextBlock, IUpdatable
    {
        bool _istyping;
        bool _iserasing;
        bool _autoErase;
        double _delay;
        double _eraseDelay;
        FlxSound _sound;

        /**
	 * This is incremented every frame by elapsed, and when greater than delay, adds the next letter.
	 */
        double _timer = 0.0;

        /**
		 * A timer that is used while waiting between typing and erasing.
		 */
        double _waitTimer = 0.0;

        /**
         * Internal tracker for current string length, not counting the prefix.
         */
        int _length = 0;

        bool _waiting = false;


        public TypeTextBlock()
        {

        }

        void IUpdatable.Update()
        {
            Update();
        }

        private void Update()
        {
            if (FinalText == null) FinalText = "";
            if (_waiting && !IsPaused)
            {
                _waitTimer -= StaticTimer.DeltaSaconds;

                if (_waitTimer <= 0)
                {
                    _waiting = false;
                    _iserasing = true;
                }
            }

            // So long as we should be animating, increment the timer by time elapsed.
            if (!_waiting && !IsPaused)
            {
                if (_length < FinalText.Length && IsTyping) _timer += StaticTimer.DeltaSaconds;
                if (_length > 0 && _iserasing) _timer += StaticTimer.DeltaSaconds;
            }

            // If the timer value is higher than the rate at which we should be changing letters, increase or decrease desired string length.

            if (IsTyping || _iserasing)
            {
                if (IsTyping && _timer >= _delay)
                {
                    _length += (int)(_timer / _delay);
                    if (_length > FinalText.Length)
                        _length = FinalText.Length;
                }

                if (_iserasing && _timer >= _eraseDelay)
                {
                    _length -= (int)(_timer / _eraseDelay);
                    if (_length < 0)
                        _length = 0;
                }

                if (IsTyping && _timer >= _delay || _iserasing && _timer >= _eraseDelay)
                {
                    _timer %= _delay;
                    _sound = Sound.Play(SourceSound.OriginalString);
                }
            }
            // Update the helper string with what could potentially be the new FinalText.
            var helperString = FinalText[.._length];


            // If the FinalText changed, update it.
            Text = helperString;

            // If we're done typing, call the onComplete() function
            if (_length >= FinalText.Length && IsTyping && !_waiting && !_iserasing) OnComplete();

            // If we're done erasing, call the onErased() function
            if (_length == 0 && _iserasing && !IsTyping && !_waiting) OnErased();

        }

        #region Public Functions

        ///<summary>
        ///Reset the FinalText with a new FinalText string. Automatically cancels typing, and erasing.
        /// </summary>
        /// <param name="FinalText">The FinalText that will ultimately be displayed.</param>
        public void ResetText(string text)
        {
            Text = "";
            FinalText = text;
            _istyping = false;
            _iserasing = false;
            IsPaused = false;
            _waiting = false;
            _length = 0;
        }

        ///<summary>
        ///Immediately finishes the animation. Called if any of the skipKeys is pressed.
        /// Handy for custom skipping behaviour(for example with different inputs like mouse or gamepad).
        /// </summary>
        public void Skip()
        {
            if (IsErasing || _waiting)
            {
                _length = 0;
                _waiting = false;
            }
            else if (IsTyping)
            {
                _length = FinalText.Length;
            }
        }


        /// <summary>
        /// Start the FinalText animation.
        /// </summary>
        /// <param name="delay">Optionally, set the delay between characters. Can also be set separately.</param>
        /// <param name="forceRestart">Whether or not to start this animation over if currently animating; false by default.</param>
        /// <param name="autoErase">Whether or not to begin the erase animation when the typing animation is complete. Can also be set separately.</param>
        /// <param name="callback">An optional callback function, to be called when the typing animation is complete.</param>

        public void Start(double delay, bool forceRestart = false, bool autoErase = false, EventHandler callback = null)
        {
            _delay = delay;

            _istyping = true;
            _iserasing = false;
            IsPaused = false;
            _waiting = false;

            if (forceRestart)
            {
                Text = "";
                _length = 0;
            }

            _autoErase = autoErase;
            void handler(object sender, EventArgs e)
            {
                callback?.Invoke(sender, e);
                TypeComplete -= handler;
            }
            TypeComplete += handler;

            InsertBreakLines();

        }


        /// <summary>
        /// Begin an animated erase of this text.
        /// </summary>
        /// <param name="delay">Optionally, set the delay between characters. Can also be set separately.</param>
        /// <param name="forceRestart">Whether or not to start this animation over if currently animating; false by default.</param>
        /// <param name="callback">
        /// An optional callback function, to be called when the erasing animation is complete.
        /// Optional parameters to pass to the callback function.
        /// </param>
        public void Erase(double delay, bool forceRestart = false, EventHandler callback = null)
        {
            _iserasing = true;
            _istyping = false;
            IsPaused = false;
            _waiting = false;
            _eraseDelay = delay;

            if (forceRestart)
            {
                _length = FinalText.Length;
                Text = FinalText;
            }

            void handler(object sender, EventArgs e)
            {
                callback(sender, e);
                EraseComplete -= handler;
            }
            EraseComplete += handler;

        }

        #endregion

        ///<summary>
        ///Internal function that replace last space in a line for a line break.
        /// To prevent a word start typing in a line and jump to next.
        /// </summary>
        void InsertBreakLines()
        {
            var saveText = Text;

            var last = FinalText.Length;
            while (true)
            {
                last = FinalText[..last].LastIndexOf(" ");

                if (last <= 0)
                    break;

                Text = FinalText;
                int n0 = Text.Split('\n').Length;

                var nextText = FinalText[..last] + "\n" + FinalText[(last + 1)..];

                Text = nextText;
                int n1 = Text.Split('\n').Length;
                if (n0 == n1)
                {
                    FinalText = nextText;
                }
            }

            Text = saveText;
        }

        ///<summary>
        ///Internal function that is called when typing is complete.
        ///</summary>
        void OnComplete()
        {
            _timer = 0;
            _istyping = false;


            _sound?.Player.Stop();

            TypeComplete?.Invoke(this, EventArgs.Empty);

            if (_autoErase && WaitTime <= 0)
            {
                _iserasing = true;
            }
            else if (_autoErase)
            {
                _waitTimer = WaitTime;
                _waiting = true;
            }
        }

        void OnErased()
        {
            _timer = 0;
            _iserasing = false;
            EraseComplete?.Invoke(this, EventArgs.Empty);
        }

        #region Public Properies
        public bool IsTyping => _istyping;

        public bool IsErasing => _iserasing;

        public string FinalText { get; set; }
        /// <summary>
        /// How long to pause after finishing the FinalText before erasing it. Only used if autoErase is true.
        /// </summary>
        public double WaitTime { get; set; } = 1;

        public bool IsPaused { get; set; }

        public Uri SourceSound
        {
            get { return (Uri)GetValue(SourceSoundProperty); }
            set { SetValue(SourceSoundProperty, value); }
        }
        #endregion

        #region Public Events
        public event EventHandler TypeComplete;
        public event EventHandler EraseComplete;
        #endregion

        // Using a DependencyProperty as the backing store for SourceSound.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceSoundProperty =
            DependencyProperty.Register("SourceSound", typeof(Uri), typeof(TypeTextBlock), new PropertyMetadata(null));
    }
}
