using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace MultipleFilesZip
{
    /// <summary>
    /// Iterate though all files that match ZipFilePattern in SourceLocation folder
    /// move files created in previous years into folder named by year when this file was created
    /// and create zip files from all these new folders
    /// Later non-zipped folders should be removed manually
    /// </summary>
    internal class Program
    {
        private const string SourceLocation = @"C:\terminalCheck\logs\";
        private static readonly int CurrentYear = DateTime.Today.Year;
        private const string ZipFilePattern = "*.log";

        public static void Main(string[] args)
        {
            var stopwatch = ZipFilesInFolder();
            Console.WriteLine($"{stopwatch.Elapsed} | Done!");
            Console.ReadLine();
        }

        private static Stopwatch ZipFilesInFolder()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            HashSet<int> years = new HashSet<int>();
            
            string[] zipFiles = Directory.GetFiles(SourceLocation, ZipFilePattern, SearchOption.TopDirectoryOnly);
            foreach (var zipFile in zipFiles)
            {
                FileInfo file = new FileInfo(zipFile);
                var year = file.LastWriteTime.Year;
                if (CurrentYear == year) continue;

                years.Add(year);
                var destination = $"{SourceLocation}{year}{Path.DirectorySeparatorChar}";
                Directory.CreateDirectory(destination);
                File.Move(zipFile, $"{destination}{Path.GetFileName(zipFile)}");
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
