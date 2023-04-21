using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace SingleFileZip
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
        private const int NoZipPeriodInDays = 180;
        private const string ZipFilePattern = "*.log";
        
        private static Stopwatch _stopwatch;
        private static string _zippedFilesFolder;
        private static string _tempFolder;
        private static DirectoryInfo _tempDirInfo;

        
        public static void Main(string[] args)
        {
            _zippedFilesFolder = Directory
                .CreateDirectory($"{SourceLocation}zippedFiles{Path.DirectorySeparatorChar}")
                .FullName;
            _tempFolder = Directory
                .CreateDirectory($"{SourceLocation}temp{Path.DirectorySeparatorChar}")
                .FullName;
            _tempDirInfo = new DirectoryInfo(_tempFolder);

            var stopwatch = ZipFiles();
            Console.WriteLine($"{stopwatch.Elapsed} | Done!");
            Console.ReadLine();
        }


        private static Stopwatch ZipFiles()
        {
            _stopwatch = Stopwatch.StartNew();
            DateTime now = DateTime.Now;

            string[] zipFiles = Directory.GetFiles(SourceLocation, ZipFilePattern, SearchOption.TopDirectoryOnly);
            foreach (string zipFile in zipFiles)
            {
                if (IsSkipZip(zipFile, now)) continue;

                try
                {
                    ArchiveFile(zipFile);
                }
                catch (Exception e)
                {
                    Console.WriteLine( $"{_stopwatch.Elapsed} | WARN !!! Failed zipping file returned to source: {zipFile}");
                }
            }

            RemoveTempFolder();
            return _stopwatch;
        } 

        private static void ArchiveFile(string zipFile)
        {
            if (!IsTempDirectoryEmpty())
            {
                Console.WriteLine($"{_stopwatch.Elapsed} | WARN !!! Can't zip file: {zipFile} because temp folder is not empty");
                return;
            }
            
            string zipFileName = Path.GetFileName(zipFile);
            string tempFileLocation = $"{_tempFolder}{zipFileName}";
            // Console.WriteLine($"{stopwatch.Elapsed} | DEBUG !!! Moving file: {zipFile} to temp dir: {tempFileLocation}");
            File.Move(zipFile, tempFileLocation);
            if (IsTempDirectoryEmpty())
            {
                Console.WriteLine($"{_stopwatch.Elapsed} | WARN !!! Failed moving file: {zipFile} to temp folder");
            }
            else
            {
                // Console.WriteLine($"{stopwatch.Elapsed} | DEBUG !!! Start compressing file: {zipFile}");
                if (TryZip(zipFile))
                {
                    File.Move(tempFileLocation, $"{_zippedFilesFolder}{zipFileName}");
                }
                else
                {
                    Console.WriteLine($"{_stopwatch.Elapsed} | WARN !!! Failed to zip file: {zipFile}");
                    File.Move(tempFileLocation, $"{SourceLocation}{zipFileName}");
                    Console.WriteLine( $"{_stopwatch.Elapsed} | INFO !!! Failed zip file returned to source: {zipFile}");
                }

                if (!IsTempDirectoryEmpty())
                {
                    throw new IOException($"failed to revert change for file: {zipFile}");
                }
            }
        }

        private static bool TryZip(string zipFile)
        {
            bool isSuccess = true;
            try
            {
                ZipFile.CreateFromDirectory(_tempFolder, $"{zipFile}.zip");
            }
            catch (Exception ignore)
            {
                isSuccess = false;
            }

            return isSuccess;
        }

        private static void RemoveTempFolder()
        {
            if (IsTempDirectoryEmpty())
            {
                Directory.Delete(_tempFolder);
                Console.WriteLine($"{_stopwatch.Elapsed} | Temp folder removed.");
            }
            else
            {
                Console.WriteLine($"{_stopwatch.Elapsed} | WARN !!! check temp folder {_tempFolder}");
            }
        }

        private static bool IsSkipZip(string zipFile, DateTime now)
        {
            FileInfo file = new FileInfo(zipFile);
            return (now - file.LastWriteTime).TotalDays <= NoZipPeriodInDays;
        }

        private static bool IsTempDirectoryEmpty()
        {
            FileInfo[] files = _tempDirInfo.GetFiles();
            DirectoryInfo[] subFolders = _tempDirInfo.GetDirectories();

            return files.Length == 0 
                   && subFolders.Length == 0;
        }
    }
}