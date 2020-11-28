using System;
using System.Collections.Generic;
using System.Text;

namespace FrontEnd.Classes
{
    public class FrontEndData
    {
        public string LibraryPath { get; set; }

        public List<Game> Games { get; set; }

        public FrontEndData()
        {
            Games = new List<Game>();
        }
    }
}
