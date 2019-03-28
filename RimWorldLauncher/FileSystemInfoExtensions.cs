using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace RimWorldLauncher
{
    enum SymbolicLink
    {
        File = 0,
        Directory = 1
    }

    public static class FileSystemInfoExtensions
    {
        [DllImport("kernel32.dll")]
        static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);

        private static readonly Dictionary<string, FileSystemWatcher> FileWatchers =
            new Dictionary<string, FileSystemWatcher>();
        
        /// <summary>
        ///     Creates a directory junction inside <paramref name="parent" /> with the name <paramref name="junctionPointName" />,
        ///     which leads to <paramref name="targetDir" />.
        /// </summary>
        /// <param name="parent">The parent folder of the new junction.</param>
        /// <param name="junctionPointName">The name of the junction.</param>
        /// <param name="targetDir">The directory that the junction should lead to.</param>
        /// <param name="overwrite">Whether or not to overwrite the junction if it already exists.</param>
        /// <returns></returns>
        public static DirectoryInfo CreateJunction(this DirectoryInfo parent, string junctionPointName,
            DirectoryInfo targetDir, bool overwrite)
        {
            if (!targetDir.Exists)
                throw new IOException("Target path does not exist or is not a directory.");

            var junctionPoint = parent.EnumerateDirectories()
                .FirstOrDefault(directory => directory.Name.ToLower() == junctionPointName.ToLower());
            if (junctionPoint != null)
            {
                if (!overwrite)
                    throw new IOException("Directory already exists.");
                if (!junctionPoint.IsSymlink())
                    throw new IOException(
                        $"Cannot overwrite the {junctionPoint.FullName} as it is not a directory junction. Only a directory junction can be overwritten.");
                junctionPoint.Delete();
            }

            //CreateSymbolicLink(Path.Combine(parent.FullName, junctionPointName), targetDir.FullName, SymbolicLink.Directory);
            var cmd = $"/c MKLINK /J \"{Path.Combine(parent.FullName, junctionPointName)}\" \"{targetDir.FullName}\"";
            Process.Start("CMD.exe",
                cmd);

            return junctionPoint;
        }

        public static DirectoryInfo FromPath(string path)
        {
            path = Environment.ExpandEnvironmentVariables(path);
            if (string.IsNullOrEmpty(path)) return null;
            DirectoryInfo directoryInfo;
            try
            {
                directoryInfo = new DirectoryInfo(path);
            }
            catch (Exception)
            {
                return null;
            }

            return directoryInfo;
        }

        /// <summary>
        ///     Obtains the FileSystemWatcher of <paramref name="file" /> or null.
        /// </summary>
        /// <param name="file">The file to obtain the watcher for.</param>
        /// <returns>The watcher of <paramref name="file" /> or null.</returns>
        public static FileSystemWatcher GetWatcher(this FileInfo file)
        {
            return FileWatchers.ContainsKey(file.FullName) ? FileWatchers[file.FullName] : null;
        }

        /// <summary>
        ///     Checks if <paramref name="info" /> is a symbolic link or a directory junction.
        /// </summary>
        /// <param name="info">The file or directory to check.</param>
        /// <returns>true if<paramref name="info" /> is a symbolic link or a directory junction.</returns>
        public static bool IsSymlink(this FileSystemInfo info)
        {
            return info.Attributes.HasFlag(FileAttributes.ReparsePoint);
        }

        /// <summary>
        ///     Starts watching <paramref name="file" /> for change in its content.
        /// </summary>
        /// <param name="file">The file to watch.</param>
        /// <param name="onChanged">The event to run when <paramref name="file" /> is changed.</param>
        /// <returns>A FileSystemWatcher that can be used to control the execution of <paramref name="onChanged" />.</returns>
        public static FileSystemWatcher Watch(this FileInfo file, FileSystemEventHandler onChanged)
        {
            FileSystemWatcher watcher;
            if (FileWatchers.ContainsKey(file.FullName))
                watcher = file.GetWatcher();
            else
                FileWatchers[file.FullName] = watcher = new FileSystemWatcher
                {
                    Path = file.DirectoryName,
                    Filter = file.Name,
                    NotifyFilter = NotifyFilters.LastWrite,
                    EnableRaisingEvents = true
                };
            watcher.Changed += onChanged;
            return watcher;
        }
    }
}