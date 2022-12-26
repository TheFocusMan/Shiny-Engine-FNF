using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfGame.Controls;

namespace WpfGame
{
    public class SubState : UserControl, IUpdatable
    {
        internal State _current = null;

        static SubState()
        {
            ContentProperty.OverrideMetadata(typeof(SubState), new FrameworkPropertyMetadata(null, OnPropertyChanged));
        }


        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null && !(e.NewValue is Camera2D))
                throw new InvalidOperationException("Only camera2D support");
        }


        public State CurrentState => _current;

        public SubState() : base()
        {

        }

        protected virtual void Update()
        {
            if (Content is Camera2D)
                ((IUpdatable)Content).Update();
        }

        public void Close()
        {
            _current.CloseSubState();
        }

        void IUpdatable.Update()
        {
            Update();
        }


        public void FireKeyDown(KeyEventArgs e)
        {
            OnKeyDown(e);
        }

        public void FireKeyUp(KeyEventArgs e)
        {
            OnKeyUp(e);
        }
    }
}
