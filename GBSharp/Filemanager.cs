﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp
{
    public static class FileManager
    {
        public static string SavePath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/GBSharp/";
        internal static byte[] LoadSaveFile(string name, int checksum)
        {
            string path = SavePath + name + "_" + checksum.ToString() + ".gbsav";
            if (!File.Exists(path)) return new byte[1] { 0 };

            byte[] bytes;
            using (BinaryReader br = new BinaryReader(File.OpenRead(path)))
            {
                bytes = br.ReadBytes((int)br.BaseStream.Length);
                br.Close();
            }

            return bytes;
        }

        internal static void SaveFile(string name, int checksum, int[] data)
        {
            string path = SavePath + name + "_" + checksum.ToString() + ".gbsav";
            byte[] saveData = new byte[data.Length];

            if(!Directory.Exists(SavePath))
            {
                Directory.CreateDirectory(SavePath);
            }

            for(int i = 0; i < data.Length; i++)
            {
                saveData[i] = (byte)data[i];
            }
            using (BinaryWriter bw = new BinaryWriter(File.OpenWrite(path)))
            {
                bw.Write(saveData);
                bw.Close();
            }
        }
    }
}
