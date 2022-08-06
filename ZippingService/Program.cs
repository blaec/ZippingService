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
        private const string SourceFoldersLocation = @"C:\Users\blaec\Downloads\old\";
        private static readonly int CurrentYear = DateTime.Today.Year;

        public static void Main(string[] args)
        {
            Stopwatch sw = Stopwatch.StartNew();
            HashSet<int> years = new HashSet<int>();
            string[] zipFolders = Directory.GetDirectories(SourceFoldersLocation);
            foreach (var zipFolder in zipFolders)
            {
                var year = Directory.GetLastWriteTime(zipFolder).Year;
                if (CurrentYear == year) continue;
                
                years.Add(year);
                var destination = $"{SourceFoldersLocation}{year}{Path.DirectorySeparatorChar}";
                Directory.CreateDirectory(destination);
                Directory.Move(zipFolder, $"{destination}{Path.GetFileName(zipFolder)}");
            }

            foreach (var archive in years.Select(year => $"{SourceFoldersLocation}{year}"))
            {
                Console.WriteLine($"{sw.Elapsed} | Compressing folder: {archive}...");
                ZipFile.CreateFromDirectory(archive, $"{archive}.zip");
                Console.WriteLine($"{sw.Elapsed} | New zip file: {archive}.zip created.");
            }
            Console.WriteLine($"{sw.Elapsed} | Done!");
            Console.ReadLine();
        }
    }
}