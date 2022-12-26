using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfGame.Controls;
using WpfGame;

namespace WpfGame.AnimationsSheet
{
    public abstract class AnimationSheet : ISupportInitialize
    {

        Sprite _control;
        Dictionary<string, string> _animchase;
        Dictionary<string, int[]> _animsortchase;

        public double Framerate { get; set; }

        internal Dictionary<string, AnimationSubTextureDataCollection> _rect;

        bool _playingAnim;
        BitmapSource _image;


        public bool IsPlayingAmimation => _playingAnim;
        private int _times;
        private string _name;
        public bool Looping { get; set; } = false;

        private Action _act;
        Action _lastact;


        internal AnimationSubTextureDataCollection GetAnimationSubTexturesByName(string name)
        {
            _rect.TryGetValue(name, out AnimationSubTextureDataCollection textureDatas);
            return textureDatas;
        }

        public int GetAnimationFramesCountByName(string name)
        {
            _rect.TryGetValue(name, out AnimationSubTextureDataCollection textureDatas);
            return textureDatas != null ? textureDatas.Count : 0;
        }


        public void RawSetControl(Sprite padding)
        {
            _control = padding;
        }

        public Sprite Control
        {
            get => _control;
            set
            {
                if (_control != null)
                    _control.Unloaded -= Control_Unloaded;
                _control = value;
                _control.Unloaded += Control_Unloaded;
                _control.Frames = this;
            }
        }

        /// <summary>
        /// for not set offset by frame
        /// </summary>
        public bool EnableTransform { get; set; } = true;

        public string[] AnimationsNames
        {
            get => _rect.Keys.ToArray();
        }

        public int CurrentFrame
        {
            get => _times;
            set
            {
                _times = value;
                _playingAnim = true;
                RefreshFrame();
                _playingAnim = false;
            }
        }

        public string CurrentAnimationName => _currentanimation;

        string _currentanimation;

        public Dictionary<string, string> AnimationChase => _animchase;

        public Dictionary<string, int[]> AnimationSortChase => _animsortchase;

        private void Control_Unloaded(object sender, RoutedEventArgs e)
        {
            //_image.Freeze();
            //CacheKiller.DestroyImage(_image);
        }

        protected AnimationSheet(string texture, double framerate = 30)
        {
            Framerate = framerate;
            _rect = new Dictionary<string, AnimationSubTextureDataCollection>();
            _animsortchase = new Dictionary<string, int[]>();
            _animchase = new Dictionary<string, string>();
            _image = Sprite.CreateGoodImage(texture);
        }

        internal protected abstract void ReadAmimation();

        private void RefreshFrame()
        {
            if (_times < _rect[_name].Count && _playingAnim)
            {
                var rc = _rect[_name][_times];
                _control.FrameSize = new Size(rc.Frame.Width, rc.Frame.Height);
                _control.InvokeFrameChangingPosition();
                if (EnableTransform)
                    _control.SacondOffsetPoint = new Point(-rc.Frame.X, -rc.Frame.Y);
                _control.image.Margin = new Thickness(-rc.Bounds.X, -rc.Bounds.Y, 0, 0);
                _control.Width = rc.Bounds.Width;
                _control.Height = rc.Bounds.Height;
                _times++;
            }
            else
            {
                _playingAnim = Looping;
                _currentanimation = Looping ? _name : null;
                _times = 0;
                _control.FrameSize = new Size();
                if (_act == _lastact)
                    _act = null;
                _lastact = _act;
                _act?.Invoke();
            }
        }

        internal void Update()
        {
            if (StaticTimer.Stopwatch.ElapsedMilliseconds % Framerate < Framerate / 1.5) // time delay
            {
                if (_name != null && _playingAnim)
                {
                    RefreshFrame();
                }
            }
        }


        public void ResetAnimationSort(string name)
        {
            _rect[name].ResetSort();
        }

        public void AppendByPrefix(string addname, params string[] names)
        {
            _rect.Add(addname, new AnimationSubTextureDataCollection());
            foreach (var name in names)
            {
                foreach (var anim in GetAnimationSubTexturesByName(name))
                    _rect[addname].Add(anim);
            }
        }

        public void SortAnimation(string name, int[] indexes)
        {
            if (IsPlayingAmimation) return;
            _rect[name].Sort(indexes);
        }

        public void PlayAnimation(string name, bool force = false, bool lopping = true, Action action = null)
        {
            if (_playingAnim && !(force && _currentanimation != name) && _control.Visibility == Visibility.Visible) return;
            Looping = lopping;
            _currentanimation = name;
            _playingAnim = true;
            _control.Source = _image;
            _name = name;
            _act = action;
            _times = 0;
        }

        public void PlayAnimation2(string name, bool force = false, bool lopping = true, Action action = null)
        {
            if (_animchase.ContainsKey(name))
            {
                if (_animsortchase.ContainsKey(name))
                    SortAnimation(_animchase[name], _animsortchase[name]);
                PlayAnimation(_animchase[name], force, lopping, action);
            }
            else if (_rect.ContainsKey(name))
                PlayAnimation(name, force, lopping, action);
            CurrentFriendlyAnimationName = name;
        }

        public string CurrentFriendlyAnimationName { get; private set; } = "";

        public void AddByPrefix(string name, string original)
        {
            original = _rect.Keys.FindCloseString(original);
            if (!_animchase.ContainsKey(name))
                _animchase.Add(name, original);
        }

        public void AddByIndices(string name, string original, int[] sort)
        {
            AddByPrefix(name, original);
            _animsortchase.Add(name, sort);
        }


        public void ReverseAnimation(string name)
        {
            if (_playingAnim) return;
            if (_rect.ContainsKey(name)) _rect[name].Reverse();
        }

        public void ReverseAnimation2(string name)
        {
            if (_animchase.ContainsKey(name))
            {
                ReverseAnimation(_animchase[name]);
            }
        }

        public void UpdateWhenComplite(Action action)
        {
            _act = action;
        }

        public void Add(string name, params int[] frames)
        {
            if (this is TextureAnimationSheet texture)
            {
                if (!_rect.ContainsKey(name))
                {
                    var collection = new AnimationSubTextureDataCollection();
                    foreach (int i in frames) collection.Add(texture._globalcollection[i]);
                    _rect.Add(name, collection);
                }
            }
        }

        internal AnimationSubTextureData[] GetAnimationSubTextures()
        {
            List<AnimationSubTextureData> datas = new List<AnimationSubTextureData>();
            foreach (var r in _rect)
            {
                datas.AddRange(r.Value);
            }
            return datas.ToArray();
        }

        bool _preInitalized;
        bool _initalized;
        public void BeginInit()
        {
            if (!_preInitalized)
            {
                _preInitalized = true;
                ReadAmimation();
            }
        }

        public void EndInit()
        {
            if (!_initalized)
            {
                _initalized = true;
            }
        }

        public BitmapSource ImageSource => _image;
    }

    internal class AnimationSubTextureData : IComparable
    {
        Rect _rect;
        Rect _frame;
        int _num;

        public AnimationSubTextureData(Rect rect, Rect frame, int num)
        {
            _rect = rect;
            _frame = frame;
            _num = num;
        }

        public int Number => _num;

        public Rect Bounds => _rect;

        public Rect Frame => _frame;

        public Transform Transform { get; set; }

        public int CompareTo(object obj)
        {
            if (obj is AnimationSubTextureData data)
                return _num.CompareTo(data._num);
            return 0;
        }
    }

    internal class AnimationSubTextureDataCollection : ICollection<AnimationSubTextureData>
    {
        readonly List<AnimationSubTextureData> _data;
        List<AnimationSubTextureData> _sortdata;


        public AnimationSubTextureDataCollection()
        {
            _data = new List<AnimationSubTextureData>();
            _sortdata = new List<AnimationSubTextureData>();
        }

        public AnimationSubTextureData this[int index]
        {
            get => _sortdata[index];
            set => _sortdata[index] = value;
        }

        public int Count => _sortdata.Count;

        public bool IsReadOnly => false;

        public void Add(AnimationSubTextureData item)
        {
            _data.Add(item);
            ResetSort();
        }

        public void Clear()
        {
            _data.Clear();
            ResetSort();
        }

        public void Reverse()
        {
            _sortdata.Reverse();
        }

        public bool Contains(AnimationSubTextureData item)
        {
            return _sortdata.Contains(item);
        }

        public void CopyTo(AnimationSubTextureData[] array, int arrayIndex)
        {
            _sortdata.CopyTo(array, arrayIndex);
        }

        public IEnumerator<AnimationSubTextureData> GetEnumerator()
        {
            return _sortdata.GetEnumerator();
        }

        public bool Remove(AnimationSubTextureData item)
        {
            ResetSort();
            return _data.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _sortdata.GetEnumerator();
        }

        public void Sort(int[] sortsindex)
        {
            ResetSort();
            _sortdata.Clear();
            for (int i = 0; i < sortsindex.Length; i++)
            {
                _sortdata.Add(_data[(int)Mathf.Clamp(sortsindex[i],0, _data.Count-1)]); // data sort
            }
        }
        public void ResetSort()
        {
            _sortdata = _data.ToArray().ToList();
        }
    }
}
