using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WpfGame;
using WpfGame.Controls;

namespace Shiny_Engine_FNF.Code.Controls
{
    class Alphabet : StackPanel, IUpdatable
    {

        public double delay = 0.05;
        public bool paused = false;

        // for menu shit
        public double forceX = double.NegativeInfinity;
        public double targetY = 0;
        public double yMult = 120;
        public double xAdd = 0;
        public double yAdd = 0;
        public bool isMenuItem = false;
        public double textSize = 1.0;

        public string text = "";

        string _finalText = "";
        double yMulti = 1;

        // custom shit
        // amp, backslash, question mark, apostrophy, comma, angry faic, period
        AlphaCharacter lastSprite;
        bool xPosResetted = false;

        char[] splitWords = Array.Empty<char>();

        bool isBold = false;
        public List<AlphaCharacter> lettersArray = new();

        public bool finishedText = false;
        public bool typed = false;

        public double typingSpeed = 0.05;

        bool _isBold = false;

        public string Text
        {
            get => text;
            set
            {
                text = value;
                ChangeText(text);
            }
        }

        public bool IsBold
        {
            get => _isBold;
            set
            {
                _isBold = value;
                ChangeText(Text);
            }
        }

        public Alphabet()
        {
            Orientation = Orientation.Horizontal;
            ClipToBounds = true;
        }

        public Alphabet(Point location, string text = "", bool bold = false, bool typed = false, double typingSpeed = 0.05, double textSize = 1) : this()
        {
            X = location.X;
            Y = location.Y;
            forceX = double.NegativeInfinity;
            this.textSize = textSize;

            _finalText = text;
            this.text = text;
            this.typed = typed;
            isBold = bold;

            if (text != "")
            {
                if (typed)
                {
                    StartTypedText(typingSpeed);
                }
                else
                {
                    AddText();
                }
            }
            else
            {
                finishedText = true;
            }

        }

        public double X
        {
            get => Margin.Left;
            set => Margin = new Thickness(value, Y, 0, 0);
        }

        public double Y
        {
            get => Margin.Top;
            set => Margin = new Thickness(X, value, 0, 0);
        }

        public void ChangeText(string newText, double newTypingSpeed = -1)
        {
            for (int i = 0; i < lettersArray.Count; i++)
            {
                var letter = lettersArray[0];
                Children.Remove(letter);
                lettersArray.Remove(letter);
            }
            lettersArray = new();
            splitWords = Array.Empty<char>();
            loopNum = 0;
            xPos = 0;
            curRow = 0;
            consecutiveSpaces = 0;
            xPosResetted = false;
            finishedText = false;
            lastSprite = null;

            var lastX = X;
            X = 0;
            _finalText = newText;
            text = newText;
            if (newTypingSpeed != -1) typingSpeed = newTypingSpeed;
            if (text != "")
            {
                if (typed) StartTypedText(typingSpeed);
                else AddText();
            }
            else finishedText = true;
            X = lastX;
        }



        public void AddText()
        {
            DoSplitWords();

            double xPos = 0;
            foreach (var character in splitWords)
            {
                // if (character.fastCodeAt() == " ")
                // {
                // }

                var spaceChar = character == ' ' || isBold && character == '_';
                if (spaceChar)
                {
                    consecutiveSpaces++;
                }

                var isNumber = AlphaCharacter.Numbers.IndexOf(character) != -1;
                var isSymbol = AlphaCharacter.Symbols.IndexOf(character) != -1;
                var isAlphabet = AlphaCharacter.Alphabet.IndexOf(char.ToLower(character)) != -1;
                if ((isAlphabet || isSymbol || isNumber) && (!isBold || !spaceChar))
                {
                    if (lastSprite != null)
                    {
                        xPos = lastSprite.X + lastSprite.Width;
                    }

                    if (consecutiveSpaces > 0)
                    {
                        xPos += 40 * consecutiveSpaces * textSize;
                    }
                    consecutiveSpaces = 0;

                    // var letter:AlphaCharacter = new AlphaCharacter(30 * loopNum, 0, textSize);
                    var letter = new AlphaCharacter(new(xPos, 0), textSize);

                    if (isBold)
                    {
                        if (isNumber)
                        {
                            letter.CreateBoldNumber(character.ToString());
                        }
                        else if (isSymbol)
                        {
                            letter.CreateBoldSymbol(character.ToString());
                        }
                        else letter.CreateBoldLetter(character.ToString());
                    }
                    else
                    {
                        if (isNumber) letter.CreateNumber(character.ToString());
                        else if (isSymbol) letter.CreateSymbol(character.ToString());
                        else letter.CreateLetter(character.ToString());
                    }

                    Children.Add(letter);
                    lettersArray.Add(letter);

                    lastSprite = letter;
                }

                // loopNum += 1;
            }
        }

        void DoSplitWords()
        {
            splitWords = _finalText.ToCharArray();
        }

        int loopNum = 0;
        double xPos = 0;
        public int curRow = 0;
        FlxSound dialogueSound = null;
        int consecutiveSpaces = 0;

        DispatcherTimer typeTimer = null;
        public void StartTypedText(double speed)
        {
            _finalText = text;
            DoSplitWords();

            // trace(arrayShit);

            if (speed <= 0)
            {
                while (!finishedText)
                    TimerCheck();

                dialogueSound?.Dispose();
                dialogueSound = Sound.Play(Paths.Sound("dialogue"));
            }
            else
            {
                typeTimer = new DispatcherTimer();
                typeTimer.Interval = TimeSpan.FromSeconds(speed);
                typeTimer.Tick += (s, e) =>
                {
                    TimerCheck(typeTimer);
                };
            }
        }

        const double LONG_TEXT_ADD = -24; //text is over 2 rows long, make it go up a bit
        public void TimerCheck(DispatcherTimer tmr = null)
        {
            var autoBreak = false;
            if (loopNum <= splitWords.Length - 2 && splitWords[loopNum] == '\\' && splitWords[loopNum + 1] == 'n' ||
                (autoBreak = true) && xPos >= Display.DefaultWidth * 0.65 && splitWords[loopNum] == ' ')
            {
                if (autoBreak)
                {
                    //if (tmr != null) tmr.Loops -= 1;
                    loopNum += 1;
                }
                else
                {
                    //if (tmr != null) tmr.loops -= 2;
                    loopNum += 2;
                }
                yMulti += 1;
                xPosResetted = true;
                xPos = 0;
                curRow += 1;
                if (curRow == 2) Y += LONG_TEXT_ADD;
            }

            if (loopNum <= splitWords.Length)
            {
                var spaceChar = splitWords[loopNum] == ' ' || isBold && splitWords[loopNum] == '_';
                if (spaceChar) consecutiveSpaces++;

                var isNumber = AlphaCharacter.Numbers.IndexOf(splitWords[loopNum]) != -1;
                var isSymbol = AlphaCharacter.Symbols.IndexOf(splitWords[loopNum]) != -1;
                var isAlphabet = AlphaCharacter.Alphabet.IndexOf(char.ToLower(splitWords[loopNum])) != -1;

                if ((isAlphabet || isSymbol || isNumber) && (!isBold || !spaceChar))
                {
                    if (lastSprite != null && !xPosResetted)
                    {
                        xPos += lastSprite.Width + 3;
                        // if (isBold)
                        // xPos -= 80;
                    }
                    else
                    {
                        xPosResetted = false;
                    }

                    if (consecutiveSpaces > 0)
                    {
                        xPos += 20 * consecutiveSpaces * textSize;
                    }
                    consecutiveSpaces = 0;

                    // var letter:AlphaCharacter = new AlphaCharacter(30 * loopNum, 0, textSize);
                    AlphaCharacter letter = new(new(xPos, 55 * yMulti), textSize);
                    letter.row = curRow;
                    if (isBold)
                    {
                        if (isNumber)
                        {
                            letter.CreateBoldNumber(splitWords[loopNum].ToString());
                        }
                        else if (isSymbol)
                        {
                            letter.CreateBoldSymbol(splitWords[loopNum].ToString());
                        }
                        else
                        {
                            letter.CreateBoldLetter(splitWords[loopNum].ToString());
                        }
                    }
                    else
                    {
                        if (isNumber) letter.CreateNumber(splitWords[loopNum].ToString());
                        else if (isSymbol) letter.CreateSymbol(splitWords[loopNum].ToString());
                        else letter.CreateLetter(splitWords[loopNum].ToString());
                    }
                    //letter.X += 90;

                    if (tmr != null)
                    {
                        if (dialogueSound != null) dialogueSound.Dispose();
                        dialogueSound = Sound.Play(Paths.Sound("dialogue"));
                    }

                    Children.Add(letter);

                    lastSprite = letter;
                }
            }

            loopNum++;
            if (loopNum >= splitWords.Length)
            {
                if (tmr != null)
                {
                    typeTimer = null;
                    tmr.Stop();
                }
                finishedText = true;
            }
        }
        void IUpdatable.Update()
        {
            Update();
        }
        void Update()
        {
            if (isMenuItem)
            {
                var scaledY = Mathf.RemapToRange(targetY, 0, 1, 0, 1.3);

                var lerpVal = Mathf.Clamp(StaticTimer.DeltaSaconds * 9.6, 0, 1);
                Point point = new(Margin.Left, Margin.Top);
                point.Y = Mathf.Lerp(point.Y, (scaledY * yMult) + (Display.DefaultHeight * 0.48) + yAdd, lerpVal);
                if (forceX != double.NegativeInfinity)
                {
                    point.X = forceX;
                }
                else
                {
                    point.X = Mathf.Lerp(point.X, (targetY * 20) + 90 + xAdd, lerpVal);
                }
                Margin = new Thickness(point.X, point.Y, 0, 0);
            }
        }
    }

    class AlphaCharacter : Sprite
    {
        public const string Alphabet = "abcdefghijklmnopqrstuvwxyz";

        public const string Numbers = "1234567890";

        public const string Symbols = "|~#$%()*+-:;<=>@[]^_.,'!?";

        public int row = 0;

        private double textSize = 1;

        public AlphaCharacter(Point location, double textSize) : base()
        {
            Margin = new Thickness(location.X, location.Y, 0, 0);
            var tex = Paths.GetSparrowAtlas("alphabet", this, null);
            Frames = tex;
            this.textSize = textSize;
            tex.Framerate = 24;
            ClipToBounds = true;
            Height = 0; // למנוע באג
            Width = 0;
        }

        public void CreateBoldLetter(string letter)
        {
            Frames.AddByPrefix(letter, letter.ToUpper() + " bold");
            Frames.PlayAnimation2(letter);
        }

        public void CreateBoldNumber(string letter)
        {
            Frames.AddByPrefix(letter, "bold" + letter);
            Frames.PlayAnimation2(letter);
        }

        public void CreateBoldSymbol(string letter)
        {
            switch (letter)
            {
                case ".":
                    Frames.AddByPrefix(letter, "PERIOD bold");
                    break;
                case "'":
                    Frames.AddByPrefix(letter, "APOSTRAPHIE bold");
                    break;
                case "?":
                    Frames.AddByPrefix(letter, "QUESTION MARK bold");
                    break;
                case "!":
                    Frames.AddByPrefix(letter, "EXCLAMATION POINT bold");
                    break;
                case "(":
                    Frames.AddByPrefix(letter, "bold (");
                    break;
                case ")":
                    Frames.AddByPrefix(letter, "bold )");
                    break;
                default:
                    Frames.AddByPrefix(letter, "bold " + letter);
                    break;
            }
            Frames.PlayAnimation2(letter);
            switch (letter)
            {
                case "'":
                    Y -= 20 * textSize;
                    break;
                case "-":
                    //x -= 35 - (90 * (1.0 - textSize));
                    Y += 20 * textSize;
                    break;
                case "(":
                    X -= 65 * textSize;
                    Y -= 5 * textSize;
                    OffsetX = -58 * textSize;
                    break;
                case ")":
                    X -= 20 / textSize;
                    Y -= 5 * textSize;
                    OffsetX = 12 * textSize;
                    break;
                case ".":
                    Y += 45 * textSize;
                    X += 5 * textSize;
                    OffsetX += 3 * textSize;
                    break;
            }
        }

        public void CreateLetter(string letter)
        {
            var letterCase = "lowercase";
            if (letter.ToLower() != letter)
            {
                letterCase = "capital";
            }

            Frames.AddByPrefix(letter, letter + " " + letterCase);
            Frames.PlayAnimation2(letter);

            Y = 110 - Height;
            Y += row * 60;
        }

        public void CreateNumber(string letter)
        {
            Frames.AddByPrefix(letter, letter);
            Frames.PlayAnimation2(letter);

            Y = 110 - Height;
            Y += row * 60;
        }

        public void CreateSymbol(string letter)
        {
            switch (letter)
            {
                case "#":
                    Frames.AddByPrefix(letter, "hashtag");
                    break;
                case ".":
                    Frames.AddByPrefix(letter, "period");
                    break;
                case "'":
                    Frames.AddByPrefix(letter, "apostraphie");
                    Y -= 50;
                    break;
                case "?":
                    Frames.AddByPrefix(letter, "question mark");
                    break;
                case "!":
                    Frames.AddByPrefix(letter, "exclamation point");
                    break;
                case ",":
                    Frames.AddByPrefix(letter, "comma");
                    break;
                default:
                    Frames.AddByPrefix(letter, letter);
                    break;
            }
            Frames.PlayAnimation2(letter);

            Y = 110 - Height;
            Y += row * 60;
            switch (letter)
            {
                case "'":
                    Y -= 20;
                    break;
                case "-":
                    //x -= 35 - (90 * (1.0 - textSize));
                    Y -= 16;
                    break;
            }
        }
    }
}
