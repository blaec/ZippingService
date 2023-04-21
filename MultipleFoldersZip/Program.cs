using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace MultipleFoldersZip
{
    /// <summary>
    /// Iterate though all 1st level folders in source-folder-location
    /// Get year from folder's last write time, create folder with this year and move all similar folders into this folder
    /// Compress all newly created folders
    /// Only skip folders created in current year
    /// </summary>
    internal class Program
    {
        private const string SourceLocation = @"C:\terminalCheck\logs\";
        private static readonly int CurrentYear = DateTime.Today.Year;

        public static void Main(string[] args)
        {
            var stopwatch = ZipFolders();
            Console.WriteLine($"{stopwatch.Elapsed} | Done!");
            Console.ReadLine();
        }

        private static Stopwatch ZipFolders()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            HashSet<int> years = new HashSet<int>();
            
            string[] zipFolders = Directory.GetDirectories(SourceLocation);
            foreach (var zipFolder in zipFolders)
            {
                var year = Directory.GetLastWriteTime(zipFolder).Year;
                if (CurrentYear == year) continue;

                years.Add(year);
                var destination = $"{SourceLocation}{year}{Path.DirectorySeparatorChar}";
                Directory.CreateDirectory(destination);
                Directory.Move(zipFolder, $"{destination}{Path.GetFileName(zipFolder)}");
            }

            foreach (var zippedFolder in years.Select(year => $"{SourceLocation}{year}"))
            {
                Console.WriteLine($"{stopwatch.Elapsed} | Compressing folder: {zippedFolder}...");
                ZipFile.CreateFromDirectory(zippedFolder, $"{zippedFolder}.zip");
                Console.WriteLine($"{stopwatch.Elapsed} | New zip file: {zippedFolder}.zip created.");
            }

            return stopwatch;
        }
    }
}
