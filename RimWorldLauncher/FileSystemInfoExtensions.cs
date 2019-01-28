using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace RimWorldLauncher
{
    public static class FileSystemInfoExtensions
    {
        /// <summary>
        ///     Command to set the reparse point data block.
        /// </summary>
        private const int FsctlSetReparsePoint = 0x000900A4;

        /// <summary>
        ///     Reparse point tag used to identify mount points and junction points.
        /// </summary>
        private const uint IoReparseTagMountPoint = 0xA0000003;

        /// <summary>
        ///     This prefix indicates to NTFS that the path is to be treated as a non-interpreted
        ///     path in the virtual file system.
        /// </summary>
        private const string NonInterpretedPathPrefix = @"\??\";

        private static readonly Dictionary<string, FileSystemWatcher> FileWatchers =
            new Dictionary<string, FileSystemWatcher>();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode,
            IntPtr inBuffer, int nInBufferSize,
            IntPtr outBuffer, int nOutBufferSize,
            out int pBytesReturned, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateFile(
            string lpFileName,
            EFileAccess dwDesiredAccess,
            EFileShare dwShareMode,
            IntPtr lpSecurityAttributes,
            ECreationDisposition dwCreationDisposition,
            EFileAttributes dwFlagsAndAttributes,
            IntPtr hTemplateFile);

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
            }
            else
            {
                junctionPoint = parent.CreateSubdirectory(junctionPointName);
            }

            using (var handle = OpenReparsePoint(junctionPoint.FullName, EFileAccess.GenericWrite))
            {
                var targetDirBytes = Encoding.Unicode.GetBytes(NonInterpretedPathPrefix + targetDir.FullName);

                var reparseDataBuffer =
                    new ReparseDataBuffer
                    {
                        ReparseTag = IoReparseTagMountPoint,
                        ReparseDataLength = (ushort) (targetDirBytes.Length + 12),
                        SubstituteNameOffset = 0,
                        SubstituteNameLength = (ushort) targetDirBytes.Length,
                        PrintNameOffset = (ushort) (targetDirBytes.Length + 2),
                        PrintNameLength = 0,
                        PathBuffer = new byte[0x3ff0]
                    };

                Array.Copy(targetDirBytes, reparseDataBuffer.PathBuffer, targetDirBytes.Length);

                var inBufferSize = Marshal.SizeOf(reparseDataBuffer);
                var inBuffer = Marshal.AllocHGlobal(inBufferSize);

                try
                {
                    Marshal.StructureToPtr(reparseDataBuffer, inBuffer, false);

                    var result = DeviceIoControl(handle.DangerousGetHandle(), FsctlSetReparsePoint,
                        inBuffer, targetDirBytes.Length + 20, IntPtr.Zero, 0, out _, IntPtr.Zero);

                    if (!result)
                        throw new Exception("Unable to create junction point.");
                }
                finally
                {
                    Marshal.FreeHGlobal(inBuffer);
                }
            }

            return junctionPoint;
        }

        private static SafeFileHandle OpenReparsePoint(string reparsePoint, EFileAccess accessMode)
        {
            var reparsePointHandle = new SafeFileHandle(CreateFile(reparsePoint, accessMode,
                EFileShare.Read | EFileShare.Write | EFileShare.Delete,
                IntPtr.Zero, ECreationDisposition.OpenExisting,
                EFileAttributes.BackupSemantics | EFileAttributes.OpenReparsePoint, IntPtr.Zero), true);

            if (Marshal.GetLastWin32Error() != 0)
                throw new Exception("Unable to open reparse point.");

            return reparsePointHandle;
        }

        [Flags]
        private enum EFileAccess : uint
        {
            // ReSharper disable UnusedMember.Local
            GenericRead = 0x80000000,
            GenericWrite = 0x40000000,
            GenericExecute = 0x20000000,
            GenericAll = 0x10000000
            // ReSharper restore UnusedMember.Local
        }

        [Flags]
        private enum EFileShare : uint
        {
            // ReSharper disable UnusedMember.Local
            None = 0x00000000,
            Read = 0x00000001,
            Write = 0x00000002,
            Delete = 0x00000004
            // ReSharper restore UnusedMember.Local
        }

        private enum ECreationDisposition : uint
        {
            // ReSharper disable UnusedMember.Local
            New = 1,
            CreateAlways = 2,
            OpenExisting = 3,
            OpenAlways = 4,
            TruncateExisting = 5
            // ReSharper restore UnusedMember.Local
        }

        [Flags]
        private enum EFileAttributes : uint
        {
            // ReSharper disable UnusedMember.Local
            Readonly = 0x00000001,
            Hidden = 0x00000002,
            System = 0x00000004,
            Directory = 0x00000010,
            Archive = 0x00000020,
            Device = 0x00000040,
            Normal = 0x00000080,
            Temporary = 0x00000100,
            SparseFile = 0x00000200,
            ReparsePoint = 0x00000400,
            Compressed = 0x00000800,
            Offline = 0x00001000,
            NotContentIndexed = 0x00002000,
            Encrypted = 0x00004000,
            WriteThrough = 0x80000000,
            Overlapped = 0x40000000,
            NoBuffering = 0x20000000,
            RandomAccess = 0x10000000,
            SequentialScan = 0x08000000,
            DeleteOnClose = 0x04000000,
            BackupSemantics = 0x02000000,
            PosixSemantics = 0x01000000,
            OpenReparsePoint = 0x00200000,
            OpenNoRecall = 0x00100000,
            FirstPipeInstance = 0x00080000
            // ReSharper restore UnusedMember.Local
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ReparseDataBuffer
        {
            /// <summary>
            ///     Reparse point tag. Must be a Microsoft reparse point tag.
            /// </summary>
            public uint ReparseTag;

            /// <summary>
            ///     Size, in bytes, of the data after the Reserved member. This can be calculated by:
            ///     (4 * sizeof(ushort)) + SubstituteNameLength + PrintNameLength +
            ///     (namesAreNullTerminated ? 2 * sizeof(char) : 0);
            /// </summary>
            public ushort ReparseDataLength;

            /// <summary>
            ///     Reserved; do not use.
            /// </summary>
            // ReSharper disable once MemberCanBePrivate.Local
            public readonly ushort Reserved;

            /// <summary>
            ///     Offset, in bytes, of the substitute name string in the PathBuffer array.
            /// </summary>
            public ushort SubstituteNameOffset;

            /// <summary>
            ///     Length, in bytes, of the substitute name string. If this string is null-terminated,
            ///     SubstituteNameLength does not include space for the null character.
            /// </summary>
            public ushort SubstituteNameLength;

            /// <summary>
            ///     Offset, in bytes, of the print name string in the PathBuffer array.
            /// </summary>
            public ushort PrintNameOffset;

            /// <summary>
            ///     Length, in bytes, of the print name string. If this string is null-terminated,
            ///     PrintNameLength does not include space for the null character.
            /// </summary>
            public ushort PrintNameLength;

            /// <summary>
            ///     A buffer containing the unicode-encoded path string. The path string contains
            ///     the substitute name string and print name string.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x3FF0)]
            public byte[] PathBuffer;
        }
    }
}