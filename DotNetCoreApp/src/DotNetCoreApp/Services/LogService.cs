using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCoreApp.Services
{
    public static class LogService
    {
        public static void SaveLogs(string text)
        {
            text = $"{DateTime.Now:t}: {text} {Environment.NewLine}";

            string date = DateTime.Now.ToString("dd_MM");
            string filePath = Path.Combine(System.AppContext.BaseDirectory, $"Logs");
            string fileName = $"logs_{date}.txt";
            try
            {
                // If the directory doesn't exist, create it.
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }

                string fullpath = filePath + Path.DirectorySeparatorChar + fileName;
                File.AppendAllText(fullpath, text);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
