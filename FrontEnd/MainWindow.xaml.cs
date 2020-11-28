using System.Windows;
using System.IO;
using Microsoft.Win32;
using System.Collections.Generic;
using FrontEnd.Classes;
using System.Collections.ObjectModel;
using Ookii.Dialogs.Wpf;
using GBSharp;
using Newtonsoft.Json;
using FrontEnd.MainMenu;
using System.Windows.Controls;

namespace FrontEnd
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<Game> games;
        FrontEndData fed;

        public MainWindow()
        {
            InitializeComponent();

            games = new ObservableCollection<Game>();

            if(FileManager.FileExists("FED.dat"))
            {
                LoadFed();
            }
            else
            {
                Button btn = new Button();
                container.Child = btn;
                btn.Click += SetGameLibrary;
                btn.Content = "Set Game Library";
            }

            /*string[] files = Directory.GetFiles("C:/Users/JEM/Source/Repos/GBSharp/MonoGB/bin/Windows/x86/Debug/Roms/Games");

            foreach(string f in files)
            {
                var ext = Path.GetExtension(f);
                if(ext == ".gb" || ext == ".gbc")
                {
                    var game = new Game();
                    game.Name = Path.GetFileName(f);
                    game.FilePath = "\"" + f + "\"";
                    games.Add(game);
                }
            }*/

            DataContext = games;
        }

        private void SetGameLibrary(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            var browser = new VistaFolderBrowserDialog();

            if(browser.ShowDialog() == true)
            {
                if(container.Child is Button btn)
                {
                    btn.Click -= SetGameLibrary;
                    container.Child = new GamesDisplay();
                }

                CreateFed(browser.SelectedPath);

                LoadFed();
            }
        }

        private void LoadFed()
        {
            StreamReader sr = new StreamReader(FileManager.GetReadStream("FED.dat"));
            fed = JsonConvert.DeserializeObject<FrontEndData>(sr.ReadToEnd());
            sr.Close();

            games.Clear();
            foreach(Game g in fed.Games)
            {
                games.Add(g);
            }
            container.Child = new GamesDisplay() { Margin = new Thickness(2) };
        }

        private void CreateFed(string directory)
        {
            fed = new FrontEndData();
            fed.LibraryPath = directory;
            string[] files = Directory.GetFiles(directory);

            foreach (string f in files)
            {
                var ext = Path.GetExtension(f);
                if (ext == ".gb" || ext == ".gbc")
                {
                    var game = new Game();
                    game.Name = Path.GetFileName(f);
                    game.FilePath = "\"" + f + "\"";
                    fed.Games.Add(game);
                }
            }

            FileManager.DeleteFile("FED.dat");
            FileManager.CreateFile("FED.dat");

            using (StreamWriter sw = new StreamWriter(FileManager.GetWriteStream("FED.dat")))
            {
                sw.Write(JsonConvert.SerializeObject(fed));
            }
        }
    }
}
