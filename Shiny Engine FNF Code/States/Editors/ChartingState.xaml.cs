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

namespace Shiny_Engine_FNF.States.Editors
{
    /// <summary>
    /// Interaction logic for ChartingState.xaml
    /// </summary>
    public partial class ChartingState : Page
    {

        public static string[] noteTypeList = new string[] //Used for backwards compatibility with 0.1 - 0.3.2 charts, though, you should add your hardcoded custom note types here too.
        {
            "",
            "Alt Animation",
            "Hey!",
            "Hurt Note",
            "GF Sing",
            "No Animation"
        };

        public const int GRID_SIZE = 40;

        public ChartingState()
        {
            InitializeComponent();
        }
    }
}
