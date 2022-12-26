using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfGame.Controls;

namespace WpfGame
{
    public class State : Page, IUpdatable
    {
        static State()
        {
            ContentProperty.OverrideMetadata(typeof(State), new FrameworkPropertyMetadata(null, OnPropertyChanged));
        }
        private SubState _state;
        public bool IsOpenSubState { get; private set; }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null && !(e.NewValue is Camera2D))
                throw new InvalidOperationException("Only camera2D support");
        }

        public State()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
                this.RegisterTimer(((IUpdatable)this).Update);
        }


        protected virtual void Update()
        {
            if (IsLoaded)
            {
                if (Content is Camera2D camera2D)
                {
                    ((IUpdatable)camera2D).Update();
                }
            }
        }

        void IUpdatable.Update()
        {
            if (!IsOpenSubState)
                Update();
            else (_state as IUpdatable)?.Update();
        }

        public virtual void OpenSubState(SubState subState)
        {
            if (IsOpenSubState)
                throw new InvalidCastException("SubState Is aleardy open");
            subState._current = this;
            (Content as Camera2D).Children.Add(subState);
            IsOpenSubState = true;
            _state = subState;
        }

        public virtual void CloseSubState()
        {
            _state._current = null;
            (Content as Camera2D).Children.Remove(_state);
            _state = null;
            IsOpenSubState = false;
        }

        public void FireKeyDown(KeyEventArgs e)
        {
            if (_state != null)
                _state?.FireKeyDown(e);
            else
                OnKeyDown(e);
        }

        public void FireKeyUp(KeyEventArgs e)
        {
            if (_state != null)
                _state?.FireKeyUp(e);
            else
                OnKeyUp(e);
        }
    }
}
