using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfGame;

namespace Shiny_Engine_FNF
{
    /// <summary>
    /// Interaction logic for AssetsNotExsist.xaml
    /// </summary>
    public partial class MessageState : State
    {
        public MessageState(string message)
        {
            InitializeComponent();

            messagetxt.Text = message;
        }
    }
}
