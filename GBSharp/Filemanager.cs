using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp
{
    static class Filemanager
    {
        private static string SavePath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/";
        public static byte[] LoadSaveFile()
        {
            return new byte[256];
        }

        public static void SaveFile(string name, int checksum, byte[] data)
        {
            string path = SavePath + name + "_" + checksum + ".gbsav";
            using (BinaryWriter br = new BinaryWriter(File.OpenWrite(path)))
            {
                br.Write(data);
                br.Close();
            }
        }
    }
}
