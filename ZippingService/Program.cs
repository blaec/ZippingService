using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace ZippingService
{
    /// <summary>
    /// Iterate though all 1st level folders in source-folder-location
    /// Get year from folder's last write time, create folder with this year and move all similar folders into this folder
    /// Compress all newly created folders
    /// Only skip folders created in current year
    /// </summary>
    internal class Program
    {
        private const bool IsZipFolders = false;
        private const string SourceLocation = @"C:\Users\blaec\Downloads\old\";
        private static readonly int CurrentYear = DateTime.Today.Year;
        private static readonly int NoZipPeriodInDays = 180;

        public static void Main(string[] args)
        {
            var stopwatch = IsZipFolders 
                ? ZipFolders() 
                : ZipFiles();
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

            foreach (var archive in years.Select(year => $"{SourceLocation}{year}"))
            {
                Console.WriteLine($"{stopwatch.Elapsed} | Compressing folder: {archive}...");
                ZipFile.CreateFromDirectory(archive, $"{archive}.zip");
                Console.WriteLine($"{stopwatch.Elapsed} | New zip file: {archive}.zip created.");
            }

            return stopwatch;
        }

        private static Stopwatch ZipFiles()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            // string[] zipFiles = Directory.GetDirectories(SourceLocation);

            return stopwatch;
        }
        
    }
}