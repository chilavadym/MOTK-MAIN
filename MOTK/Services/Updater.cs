using System;
using System.Diagnostics;
using System.IO;

namespace MOTK.Services
{
    public class Updater
    {
        public static Process CheckForUpdates(string filePath)
        {
            if(File.Exists(filePath))
            {
                return Process.Start(filePath, "/quickcheck");
            }
            else
            {
                throw new Exception("Updater not found");
            }
        }
    }
}
