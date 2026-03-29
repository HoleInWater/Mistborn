using System.IO;
using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for file operations
    /// </summary>
    public static class FileUtils
    {
        /// <summary>
        /// Checks if a file exists at the given path
        /// </summary>
        public static bool FileExists(string path)
        {
            return File.Exists(path);
        }

        /// <summary>
        /// Checks if a directory exists at the given path
        /// </summary>
        public static bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        /// <summary>
        /// Creates a directory if it doesn't exist
        /// </summary>
        public static void CreateDirectory(string path)
        {
            if (!DirectoryExists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// Deletes a file if it exists
        /// </summary>
        public static void DeleteFile(string path)
        {
            if (FileExists(path))
            {
                File.Delete(path);
            }
        }

        /// <summary>
        /// Deletes a directory and all its contents
        /// </summary>
        public static void DeleteDirectory(string path)
        {
            if (DirectoryExists(path))
            {
                Directory.Delete(path, true);
            }
        }

        /// <summary>
        /// Reads all text from a file
        /// </summary>
        public static string ReadAllText(string path)
        {
            if (!FileExists(path))
            {
                Debug.LogError($"FileUtils: File not found at {path}");
                return null;
            }
            return File.ReadAllText(path);
        }

        /// <summary>
        /// Writes all text to a file (creates or overwrites)
        /// </summary>
        public static void WriteAllText(string path, string content)
        {
            try
            {
                File.WriteAllText(path, content);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"FileUtils: Failed to write to file {path}: {e.Message}");
            }
        }

        /// <summary>
        /// Copies a file to a new location
        /// </summary>
        public static void CopyFile(string source, string destination, bool overwrite = false)
        {
            if (!FileExists(source))
            {
                Debug.LogError($"FileUtils: Source file not found at {source}");
                return;
            }
            
            if (FileExists(destination) && !overwrite)
            {
                Debug.LogError($"FileUtils: Destination file already exists at {destination}");
                return;
            }
            
            File.Copy(source, destination, overwrite);
        }

        /// <summary>
        /// Moves a file to a new location
        /// </summary>
        public static void MoveFile(string source, string destination)
        {
            if (!FileExists(source))
            {
                Debug.LogError($"FileUtils: Source file not found at {source}");
                return;
            }
            
            File.Move(source, destination);
        }

        /// <summary>
        /// Gets the file extension of a path
        /// </summary>
        public static string GetExtension(string path)
        {
            return Path.GetExtension(path);
        }

        /// <summary>
        /// Gets the file name without extension
        /// </summary>
        public static string GetFileNameWithoutExtension(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }

        /// <summary>
        /// Gets the file name with extension
        /// </summary>
        public static string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        /// <summary>
        /// Gets the directory path of a file
        /// </summary>
        public static string GetDirectoryName(string path)
        {
            return Path.GetDirectoryName(path);
        }

        /// <summary>
        /// Combines two paths
        /// </summary>
        public static string CombinePath(string path1, string path2)
        {
            return Path.Combine(path1, path2);
        }

        /// <summary>
        /// Gets all files in a directory
        /// </summary>
        public static string[] GetFilesInDirectory(string path, string searchPattern = "*")
        {
            if (!DirectoryExists(path))
            {
                Debug.LogError($"FileUtils: Directory not found at {path}");
                return new string[0];
            }
            return Directory.GetFiles(path, searchPattern);
        }

        /// <summary>
        /// Gets all directories in a directory
        /// </summary>
        public static string[] GetDirectoriesInDirectory(string path)
        {
            if (!DirectoryExists(path))
            {
                Debug.LogError($"FileUtils: Directory not found at {path}");
                return new string[0];
            }
            return Directory.GetDirectories(path);
        }

        /// <summary>
        /// Gets the file size in bytes
        /// </summary>
        public static long GetFileSize(string path)
        {
            if (!FileExists(path))
            {
                Debug.LogError($"FileUtils: File not found at {path}");
                return -1;
            }
            return new FileInfo(path).Length;
        }

        /// <summary>
        /// Gets the last modified time of a file
        /// </summary>
        public static System.DateTime GetLastModifiedTime(string path)
        {
            if (!FileExists(path))
            {
                Debug.LogError($"FileUtils: File not found at {path}");
                return System.DateTime.MinValue;
            }
            return File.GetLastWriteTime(path);
        }

        /// <summary>
        /// Reads all bytes from a file
        /// </summary>
        public static byte[] ReadAllBytes(string path)
        {
            if (!FileExists(path))
            {
                Debug.LogError($"FileUtils: File not found at {path}");
                return null;
            }
            return File.ReadAllBytes(path);
        }

        /// <summary>
        /// Writes all bytes to a file
        /// </summary>
        public static void WriteAllBytes(string path, byte[] bytes)
        {
            try
            {
                File.WriteAllBytes(path, bytes);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"FileUtils: Failed to write bytes to file {path}: {e.Message}");
            }
        }

        /// <summary>
        /// Gets the size of a directory in bytes (recursive)
        /// </summary>
        public static long GetDirectorySize(string path)
        {
            if (!DirectoryExists(path))
            {
                Debug.LogError($"FileUtils: Directory not found at {path}");
                return -1;
            }
            
            long size = 0;
            foreach (string file in GetFilesInDirectory(path, "*"))
            {
                size += GetFileSize(file);
            }
            
            foreach (string dir in GetDirectoriesInDirectory(path))
            {
                size += GetDirectorySize(dir);
            }
            
            return size;
        }
    }
}
