using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace FrontEnd.Classes
{
    public class Game : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _imagePath;
        private string _name;
        private int _timePlayed;

        public string ImagePath
        {
            get => _imagePath;
            set
            {
                if(_imagePath != value)
                {
                    _imagePath = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ImagePath"));
                }
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if(_name != value)
                {
                    _name = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
                }
            }
        }

        public int TimePlayed
        {
            get => _timePlayed;
            set
            {
                if(_timePlayed != value)
                {
                    _timePlayed = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TimePlayed"));
                }
            }
        }

        public string FilePath { get; set; }

        public Game()
        {
            _imagePath = "/Res/Missing.png";
            Name = "Unknown";
            FilePath = "";
            _timePlayed = 0;
        }
    }
}
