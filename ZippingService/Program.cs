using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace ZippingService
{
    /// <summary>
    /// 
    /// </summary>
    internal class Program
    {
        private const string SourceFoldersLocation = @"C:\Users\blaec\Downloads\old\";
        private static readonly int CurrentYear = DateTime.Today.Year;

        public static void Main(string[] args)
        {
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
                Console.WriteLine($"Start creating new zip file: {archive}...");
                ZipFile.CreateFromDirectory(archive, $"{archive}.zip");
                Console.WriteLine($"New zip file: {archive}.zip created.");
            }
            
            Console.ReadLine();
        }
    }
}