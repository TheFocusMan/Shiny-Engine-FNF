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

namespace Shiny_Engine_FNF
{
    /// <summary>
    /// Interaction logic for GameOverSubstate.xaml
    /// </summary>
    public partial class GameOverSubstate : UserControl
    {
        public static string characterName= "bf";
	public static string deathSoundName= "fnf_loss_sfx";
	public static string loopSoundName= "gameOver";
	public static string endSoundName = "gameOverEnd";
        public GameOverSubstate()
        {
            InitializeComponent();
        }
    }
}
