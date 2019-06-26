﻿using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using AngelLoader.Common.Utility;

namespace AngelLoader.Common
{
    internal static class Logger
    {
        private static readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();

        #region Interop

        internal struct SYSTEMTIME
        {
            internal ushort wYear;
            internal ushort wMonth;
            internal ushort wDayOfWeek;
            internal ushort wDay;
            internal ushort wHour;
            internal ushort wMinute;
            internal ushort wSecond;
            internal ushort wMilliseconds;
        }

        [DllImport("kernel32.dll")]
        internal static extern void GetLocalTime(ref SYSTEMTIME systemTime);

        // For logging purposes: It takes an entire 5ms to get one DateTime.Now, but I don't really need hardcore
        // accuracy in logging dates, they're really just there for vague temporality. Because we log on startup,
        // this claws back some startup time.
        internal static string GetDateTimeStringFast()
        {
            var dt = new SYSTEMTIME();
            GetLocalTime(ref dt);
            return dt.wYear.ToString() + '/' + dt.wMonth + '/' + dt.wDay + ' ' +
                   dt.wHour + ':' + dt.wMinute + ':' + dt.wSecond;
        }

        #endregion

        internal static void ClearLogFile(string logFile = "")
        {
            if (logFile.IsEmpty()) logFile = Paths.LogFile;

            try
            {
                Lock.EnterWriteLock();
                File.Delete(logFile);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                try
                {
                    Lock.ExitWriteLock();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }

        internal static void Log(string message, Exception ex = null, bool stackTrace = false, bool methodName = true,
            [CallerMemberName] string callerMemberName = "")
        {
            try
            {
                Lock.EnterReadLock();
                if (File.Exists(Paths.LogFile) && new FileInfo(Paths.LogFile).Length > ByteSize.MB * 50) ClearLogFile();
            }
            catch (Exception ex1)
            {
                Debug.WriteLine(ex1);
            }
            finally
            {
                try
                {
                    Lock.ExitReadLock();
                }
                catch (Exception logEx)
                {
                    Debug.WriteLine(logEx);
                }
            }

            try
            {
                Lock.EnterWriteLock();
                using (var sw = new StreamWriter(Paths.LogFile, append: true))
                {
                    var st = new StackTrace(1);
                    var methodNameStr = methodName ? callerMemberName + "\r\n" : "";
                    sw.WriteLine(GetDateTimeStringFast() + " " + methodNameStr + message);
                    if (stackTrace) sw.WriteLine("STACK TRACE:\r\n" + st);
                    if (ex != null) sw.WriteLine("EXCEPTION:\r\n" + ex);
                    sw.WriteLine();
                }
            }
            catch (Exception logEx)
            {
                Debug.WriteLine(logEx);
            }
            finally
            {
                try
                {
                    Lock.ExitWriteLock();
                }
                catch (Exception logEx)
                {
                    Debug.WriteLine(logEx);
                }
            }
        }
    }
}
