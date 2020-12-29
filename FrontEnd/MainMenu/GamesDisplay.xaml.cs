using FrontEnd.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FrontEnd.MainMenu
{
    /// <summary>
    /// Interaction logic for GamesDisplay.xaml
    /// </summary>
    public partial class GamesDisplay : UserControl
    {
        public GamesDisplay()
        {
            InitializeComponent();

            
        }

        private void GameDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBox box && box.SelectedItem is Game game)
            {
                Process.Start("C:/Users/JEM/Source/Repos/GBSharp/MonoGB/bin/Windows/x86/Release/MonoGB.exe", game.FilePath);
            }
        }
    }
}
