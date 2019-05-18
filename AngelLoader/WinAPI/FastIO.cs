﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace AngelLoader.WinAPI
{
    internal static class FastIO
    {
        private enum FINDEX_INFO_LEVELS
        {
            FindExInfoStandard = 0,
            FindExInfoBasic = 1
        }

        private enum FINDEX_SEARCH_OPS
        {
            FindExSearchNameMatch = 0,
            FindExSearchLimitToDirectories = 1,
            FindExSearchLimitToDevices = 2
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr FindFirstFileEx(
            string lpFileName,
            FINDEX_INFO_LEVELS fInfoLevelId,
            out WIN32_FIND_DATA lpFindFileData,
            FINDEX_SEARCH_OPS fSearchOp,
            IntPtr lpSearchFilter,
            int dwAdditionalFlags);

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct WIN32_FIND_DATA
        {
            internal uint dwFileAttributes;
            internal System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
            internal System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
            internal System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
            internal uint nFileSizeHigh;
            internal uint nFileSizeLow;
            internal uint dwReserved0;
            internal uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            internal string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            internal string cAlternateFileName;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool FindNextFileW(IntPtr hFindFile, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FindClose(IntPtr hFindFile);

        private static void ThrowException(string searchPattern, int err, string path)
        {
            if (searchPattern == null) searchPattern = "<null>";
            if (path == null) path = "<null>";

            var ex = new Win32Exception(err);
            throw new Win32Exception(err,
                "System error code: " + err + "\r\n" +
                ex.Message + "\r\n" +
                "path: '" + path + "'\r\n" +
                "search pattern: " + searchPattern + "\r\n");
        }

        private const int fileAttributeDirectory = 16;
        private const int FIND_FIRST_EX_LARGE_FETCH = 2;
        private const int ERROR_FILE_NOT_FOUND = 2;
        private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        // ~2.4x faster than GetFiles() - huge boost to cold startup time
        internal static List<string> GetFilesTopOnly(string path, string searchPattern)
        {
            if (string.IsNullOrEmpty(searchPattern))
            {
                throw new ArgumentException(nameof(searchPattern) + @" was null or empty", nameof(searchPattern));
            }

            path = path.TrimEnd('\\');

            if (string.IsNullOrWhiteSpace(path) || Path.GetInvalidPathChars().Any(path.Contains))
            {
                throw new ArgumentException("The path '" + path + "' is empty, consists only of whitespace, or contains invalid characters.");
            }

            var ret = new List<string>();

            // Other relevant errors (though we don't use them specifically at the moment)
            //const int ERROR_PATH_NOT_FOUND = 0x3;
            //const int ERROR_REM_NOT_LIST = 0x33;
            //const int ERROR_BAD_NETPATH = 0x35;

            IntPtr findHandle = FindFirstFileEx(@"\\?\" + path.TrimEnd('\\') + '\\' + searchPattern,
                FINDEX_INFO_LEVELS.FindExInfoBasic, out WIN32_FIND_DATA findData,
                FINDEX_SEARCH_OPS.FindExSearchNameMatch, IntPtr.Zero, FIND_FIRST_EX_LARGE_FETCH);
            if (findHandle == INVALID_HANDLE_VALUE)
            {
                var err = Marshal.GetLastWin32Error();
                if (err == ERROR_FILE_NOT_FOUND) return ret;

                FindClose(findHandle);

                // Since the framework isn't here to save us, we should blanket-catch and throw on every
                // possible error other than file-not-found (as that's an intended scenario, obviously).
                // This isn't as nice as you'd get from a framework method call, but it gets the job done.
                ThrowException(searchPattern, err, path);
            }
            // Be extra careful so we don't leak handles
            // Try surrounds loop so as not to enter it every loop (speculative perf)
            try
            {
                do
                {
                    if ((findData.dwFileAttributes & fileAttributeDirectory) != fileAttributeDirectory &&
                        findData.cFileName != "." && findData.cFileName != "..")
                    {
                        // Exception could occur here
                        var fullName = Path.Combine(path, findData.cFileName);

                        ret.Add(fullName);
                    }
                } while (FindNextFileW(findHandle, out findData));
            }
            catch (Exception)
            {
                FindClose(findHandle);
                throw;
            }

            FindClose(findHandle);

            return ret;
        }
    }
}