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
        private const int NoZipPeriodInDays = 180;

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
            DateTime now = DateTime.Now;
            var afterZipFolder = Directory.CreateDirectory($"{SourceLocation}zippedFiles{Path.DirectorySeparatorChar}").FullName;
            var tempFolder = Directory.CreateDirectory($"{SourceLocation}temp{Path.DirectorySeparatorChar}").FullName;

            string[] zipFiles = Directory.GetFiles(SourceLocation, "*.log", SearchOption.TopDirectoryOnly);
            foreach (string zipFile in zipFiles)
            {
                
                if (ShouldZip(zipFile, now))
                {
                    string tempFileLocation = $"{tempFolder}{Path.GetFileName(zipFile)}";
                    File.Move(zipFile, tempFileLocation);
                    if (TryZip(tempFolder, zipFile))
                    {
                        File.Move(tempFileLocation, $"{afterZipFolder}{Path.GetFileName(zipFile)}");
                    }
                    else
                    {
                        File.Move(tempFileLocation, $"{SourceLocation}{Path.GetFileName(zipFile)}");
                        Console.WriteLine($"{stopwatch.Elapsed} | WARN !!! Failed to zip file: {zipFile}");
                    }

                    if (IsDirectoryEmpty(new DirectoryInfo(tempFileLocation)))
                    {
                        throw new IOException($"failed to revert change for file: {zipFile}");
                    }
                }
            }
            RemoveTempFolder(tempFolder, stopwatch);
            return stopwatch;
        }

        private static bool TryZip(string tempFolder, string zipFile)
        {
            bool isSuccess = true;
            try
            {
                ZipFile.CreateFromDirectory(tempFolder, $"{zipFile}.zip");
            }
            catch (Exception e)
            {
                isSuccess = false;
            }

            return isSuccess;
        }

        private static void RemoveTempFolder(string tempFolder, Stopwatch stopwatch)
        {
            if (IsDirectoryEmpty(new DirectoryInfo(tempFolder)))
            {
                Directory.Delete(tempFolder);
                Console.WriteLine($"{stopwatch.Elapsed} | Temp folder removed.");
            }
            else
            {
                Console.WriteLine($"{stopwatch.Elapsed} | WARN !!! check temp folder {tempFolder}");
            }
        }

        private static bool ShouldZip(string zipFile, DateTime now)
        {
            FileInfo file = new FileInfo(zipFile);
            return (now - file.LastWriteTime).TotalDays > NoZipPeriodInDays;
        }
        
        private static bool IsDirectoryEmpty(DirectoryInfo directory)
        {
            FileInfo[] files = directory.GetFiles();
            DirectoryInfo[] subdirs = directory.GetDirectories();

            return (files.Length == 0 && subdirs.Length == 0);
        }
    }
}