using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RimWorldLauncher
{
    public static class FileSystemInfoExtensions
    {
        /// <summary>
        ///     Command to set the reparse point data block.
        /// </summary>
        private const int FSCTL_SET_REPARSE_POINT = 0x000900A4;

        /// <summary>
        ///     Reparse point tag used to identify mount points and junction points.
        /// </summary>
        private const uint IO_REPARSE_TAG_MOUNT_POINT = 0xA0000003;

        /// <summary>
        ///     This prefix indicates to NTFS that the path is to be treated as a non-interpreted
        ///     path in the virtual file system.
        /// </summary>
        private const string NonInterpretedPathPrefix = @"\??\";

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode,
            IntPtr InBuffer, int nInBufferSize,
            IntPtr OutBuffer, int nOutBufferSize,
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

        private static Dictionary<string, FileSystemWatcher> _fileWatchers = new Dictionary<string, FileSystemWatcher>();

        public static FileSystemWatcher Watch(this FileInfo info, FileSystemEventHandler onChanged)
        {
            FileSystemWatcher watcher;
            if (_fileWatchers.ContainsKey(info.FullName))
            {
                watcher = info.GetWatcher();
            }
            else
            {
                _fileWatchers[info.FullName] = watcher = new FileSystemWatcher()
                {
                    Path = info.DirectoryName,
                    Filter = info.Name,
                    NotifyFilter = NotifyFilters.LastWrite,
                    EnableRaisingEvents = true
                };
            }
            watcher.Changed += onChanged;
            return watcher;
        }

        public static FileSystemWatcher GetWatcher(this FileInfo info)
        {
            return _fileWatchers.ContainsKey(info.FullName) ? _fileWatchers[info.FullName] : null;
        }

        public static bool IsSymlink(this FileSystemInfo info)
        {
            return info.Attributes.HasFlag(FileAttributes.ReparsePoint);
        }

        public static DirectoryInfo CreateJunction(this DirectoryInfo parent, string junctionPointName, DirectoryInfo targetDir, bool overwrite)
        {
            if (!targetDir.Exists)
                throw new IOException("Target path does not exist or is not a directory.");

            DirectoryInfo junctionPoint = parent.EnumerateDirectories().FirstOrDefault((directory) => directory.Name.ToLower() == junctionPointName.ToLower());
            if (junctionPoint != null)
            {
                if (!overwrite)
                    throw new IOException("Directory already exists and overwrite parameter is false.");
            }
            else
            {
                junctionPoint = parent.CreateSubdirectory(junctionPointName);
            }

            using (var handle = OpenReparsePoint(junctionPoint.FullName, EFileAccess.GenericWrite))
            {
                var targetDirBytes = Encoding.Unicode.GetBytes(NonInterpretedPathPrefix + targetDir.FullName);

                var reparseDataBuffer =
                    new REPARSE_DATA_BUFFER
                    {
                        ReparseTag = IO_REPARSE_TAG_MOUNT_POINT,
                        ReparseDataLength = (ushort)(targetDirBytes.Length + 12),
                        SubstituteNameOffset = 0,
                        SubstituteNameLength = (ushort)targetDirBytes.Length,
                        PrintNameOffset = (ushort)(targetDirBytes.Length + 2),
                        PrintNameLength = 0,
                        PathBuffer = new byte[0x3ff0]
                    };

                Array.Copy(targetDirBytes, reparseDataBuffer.PathBuffer, targetDirBytes.Length);

                var inBufferSize = Marshal.SizeOf(reparseDataBuffer);
                var inBuffer = Marshal.AllocHGlobal(inBufferSize);

                try
                {
                    Marshal.StructureToPtr(reparseDataBuffer, inBuffer, false);

                    int bytesReturned;
                    var result = DeviceIoControl(handle.DangerousGetHandle(), FSCTL_SET_REPARSE_POINT,
                        inBuffer, targetDirBytes.Length + 20, IntPtr.Zero, 0, out bytesReturned, IntPtr.Zero);

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
            GenericRead = 0x80000000,
            GenericWrite = 0x40000000,
            GenericExecute = 0x20000000,
            GenericAll = 0x10000000
        }

        [Flags]
        private enum EFileShare : uint
        {
            None = 0x00000000,
            Read = 0x00000001,
            Write = 0x00000002,
            Delete = 0x00000004
        }

        private enum ECreationDisposition : uint
        {
            New = 1,
            CreateAlways = 2,
            OpenExisting = 3,
            OpenAlways = 4,
            TruncateExisting = 5
        }

        [Flags]
        private enum EFileAttributes : uint
        {
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
            Write_Through = 0x80000000,
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
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct REPARSE_DATA_BUFFER
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
            public ushort Reserved;

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
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x3FF0)] public byte[] PathBuffer;
        }
    }
}
