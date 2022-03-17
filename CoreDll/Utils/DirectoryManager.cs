
using System;
using System.IO;

namespace CoreDll.Utils
{
    public static class DirectoryManager
    {
        public static void Delete(string path, bool recursive = true)
        {
            Directory.Delete(path, recursive);
        }

        public static void DeleteFilesIn(string sourceDirName, bool recursive = false, Predicate<FileInfo> fileFilter = null)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // Delete the files in the directory.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                if (fileFilter == null)
                {
                    file.Delete();
                }
                else
                {
                    if (fileFilter(file))
                    {
                        file.Delete();
                    }
                }
            }

            // If recursive deleting.
            if (recursive)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    DirectoryManager.DeleteFilesIn(subdir.FullName, recursive, fileFilter);
                }
            }
        }

        public static void Copy(string sourceDirName, string destDirName, bool copySubDirs = true, bool overwriteFiles = false)
        {
            DirectoryManager.Copy(sourceDirName, destDirName, copySubDirs, overwriteFiles);
        }

        public static void Copy(string sourceDirName, string destDirName, bool copySubDirs = true, bool overwriteFiles = false, Predicate<FileInfo> fileFilter = null)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                if (fileFilter == null)
                {
                    string temppath = Path.Combine(destDirName, file.Name);
                    file.CopyTo(temppath, overwriteFiles);
                }
                else
                {
                    if (fileFilter(file))
                    {
                        string temppath = Path.Combine(destDirName, file.Name);
                        file.CopyTo(temppath, overwriteFiles);
                    }
                }
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryManager.Copy(subdir.FullName, temppath, copySubDirs, overwriteFiles, fileFilter);
                }
            }
        }

        /// <summary>
        /// Copy and delete the specified directories/files.
        /// </summary>
        /// <param name="sourceDirName"></param>
        /// <param name="destDirName"></param>
        /// <param name="overwriteFiles"></param>
        /// <param name="fileFilter"></param>
        public static void Move(string sourceDirName, string destDirName, bool overwriteFiles = false, Predicate<FileInfo> fileFilter = null)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                if (fileFilter == null)
                {
                    string temppath = Path.Combine(destDirName, file.Name);
                    file.CopyTo(temppath, overwriteFiles);
                    file.Delete();
                }
                else
                {
                    if (fileFilter(file))
                    {
                        string temppath = Path.Combine(destDirName, file.Name);
                        file.CopyTo(temppath, overwriteFiles);
                        file.Delete();
                    }
                }
            }

            // If copying subdirectories, copy them and their contents to new location.            
            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destDirName, subdir.Name);
                DirectoryManager.Move(subdir.FullName, temppath, overwriteFiles, fileFilter);
            }

            //Directory.Delete(dir.FullName, true);
            dir.Delete();
        }
    }
}