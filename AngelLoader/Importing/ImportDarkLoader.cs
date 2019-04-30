﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngelLoader.Common;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;
using static AngelLoader.Common.Logger;

namespace AngelLoader.Importing
{
    internal static class ImportDarkLoader
    {
        private static readonly string[] NonFMHeaders =
        {
            "[options]",
            "[window]",
            "[mission directories]",
            "[Thief 1]",
            "[Thief 2]",
            "[Thief2x]",
            "[SShock 2]"
        };

        private enum DLGame
        {
            darkGameUnknown = 0, // <- if it hasn't been scanned, it will be this
            darkGameThief = 1,
            darkGameThief2 = 2,
            darkGameT2x = 3,
            darkGameSS2 = 4
        }

        private static readonly Regex DarkLoaderFMRegex = new Regex(@"\.[0123456789]+]$", RegexOptions.Compiled);

        private static string RemoveDLArchiveBadChars(string archive)
        {
            foreach (string s in new[] { "]", "\u0009", "\u000A", "\u000D" }) archive = archive.Replace(s, "");
            return archive;
        }

        // Don't replace \r\n or \\ escapes because we use those in the exact same way so no conversion needed
        private static string DLUnescapeChars(string str) => str.Replace(@"\t", "\u0009").Replace(@"\""", "\"");

        internal static async Task<(ImportError Error, List<FanMission> FMs)>
        Import(string iniFile, bool importFMData, bool importSaves, List<FanMission> mainList)
        {
            var lines = await Task.Run(() => File.ReadAllLines(iniFile));
            var fms = new List<FanMission>();

            var error = ImportError.None;

            if (importFMData)
            {
                bool missionDirsRead = false;
                var archiveDirs = new List<string>();

                error = await Task.Run(() =>
                {
                    try
                    {
                        for (int i = 0; i < lines.Length; i++)
                        {
                            var line = lines[i];
                            var lineTS = line.TrimStart();
                            var lineTB = lineTS.TrimEnd();

                            #region Read archive directories

                            // We need to know the archive dirs before doing anything, because we may need to recreate
                            // some lossy names (if any bad chars have been removed by DarkLoader).
                            if (!missionDirsRead && lineTB == "[mission directories]")
                            {
                                while (i < lines.Length - 1)
                                {
                                    var lt = lines[i + 1].Trim();
                                    if (!lt.IsEmpty() && lt[0] != '[' && lt.EndsWith("=1"))
                                    {
                                        archiveDirs.Add(lt.Substring(0, lt.Length - 2));
                                    }
                                    else if (!lt.IsEmpty() && lt[0] == '[' && lt[lt.Length - 1] == ']')
                                    {
                                        break;
                                    }
                                    i++;
                                }

                                if (archiveDirs.Count == 0 || archiveDirs.All(x => x.IsWhiteSpace()))
                                {
                                    return ImportError.NoArchiveDirsFound;
                                }

                                // Restart from the beginning of the file, this time skipping anything that isn't an
                                // FM entry
                                i = -1;
                                missionDirsRead = true;
                                continue;
                            }

                            #endregion

                            #region Read FM entries

                            // MUST CHECK missionDirsRead OR IT ADDS EVERY FM TWICE!
                            if (missionDirsRead &&
                                !NonFMHeaders.Contains(lineTB) && lineTB.Length > 0 && lineTB[0] == '[' &&
                                lineTB[lineTB.Length - 1] == ']' && lineTB.Contains('.') &&
                                DarkLoaderFMRegex.Match(lineTB).Success)
                            {
                                var lastIndexDot = lineTB.LastIndexOf('.');
                                var archive = lineTB.Substring(1, lastIndexDot - 1);
                                var size = lineTB.Substring(lastIndexDot + 1, lineTB.Length - lastIndexDot - 2);

                                foreach (var dir in archiveDirs)
                                {
                                    if (!Directory.Exists(dir)) continue;
                                    try
                                    {
                                        // DarkLoader only does zip format
                                        foreach (var f in Directory.EnumerateFiles(dir, "*.zip",
                                            SearchOption.TopDirectoryOnly))
                                        {
                                            var fn = Path.GetFileNameWithoutExtension(f);
                                            if (RemoveDLArchiveBadChars(fn).EqualsI(archive))
                                            {
                                                archive = fn;
                                                goto breakout;
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Log("Exception in DarkLoader archive dir file enumeration", ex);
                                    }
                                }

                                breakout:

                                // Add .zip back on; required because everything expects it, and furthermore if there's
                                // a dot anywhere in the name then everything after it will be treated as the extension
                                // and is liable to be lopped off at any time
                                archive += ".zip";

                                ulong.TryParse(size, out ulong sizeBytes);
                                var fm = new FanMission
                                {
                                    Archive = archive,
                                    InstalledDir = archive.ToInstDirNameFMSel(),
                                    SizeBytes = sizeBytes
                                };

                                // We don't import game type, because DarkLoader by default gets it wrong for NewDark
                                // FMs (the user could have changed it manually in the ini file, and in fact it's
                                // somewhat likely they would have done so, but still, better to just scan for it
                                // ourselves later)

                                while (i < lines.Length - 1)
                                {
                                    var lts = lines[i + 1].TrimStart();
                                    var ltb = lts.TrimEnd();

                                    if (lts.StartsWith("comment=\""))
                                    {
                                        var comment = ltb.Substring(9);
                                        if (comment.Length >= 2 && comment[comment.Length - 1] == '\"')
                                        {
                                            comment = comment.Substring(0, comment.Length - 1);
                                            fm.Comment = DLUnescapeChars(comment);
                                        }
                                    }
                                    else if (lts.StartsWith("title=\""))
                                    {
                                        var title = ltb.Substring(7);
                                        if (title.Length >= 2 && title[title.Length - 1] == '\"')
                                        {
                                            title = title.Substring(0, title.Length - 1);
                                            fm.Title = DLUnescapeChars(title);
                                        }
                                    }
                                    else if (lts.StartsWith("misdate="))
                                    {
                                        ulong.TryParse(ltb.Substring(8), out ulong result);
                                        try
                                        {
                                            var date = new DateTime(1899, 12, 30).AddDays(result);
                                            fm.ReleaseDate = date.Year > 1998 ? date : (DateTime?)null;
                                        }
                                        catch (ArgumentOutOfRangeException)
                                        {
                                            fm.ReleaseDate = null;
                                        }
                                    }
                                    else if (lts.StartsWith("date="))
                                    {
                                        ulong.TryParse(ltb.Substring(5), out ulong result);
                                        try
                                        {
                                            var date = new DateTime(1899, 12, 30).AddDays(result);
                                            fm.LastPlayed = date.Year > 1998 ? date : (DateTime?)null;
                                        }
                                        catch (ArgumentOutOfRangeException)
                                        {
                                            fm.LastPlayed = null;
                                        }
                                    }
                                    else if (lts.StartsWith("finished="))
                                    {
                                        int.TryParse(ltb.Substring(9), out int result);
                                        // result will be 0 on fail, which is the empty value so it's fine
                                        fm.FinishedOn = result;
                                    }
                                    else if (!ltb.IsEmpty() && ltb[0] == '[' && ltb[ltb.Length - 1] == ']')
                                    {
                                        break;
                                    }
                                    i++;
                                }

                                fms.Add(fm);
                            }

                            #endregion
                        }
                        return ImportError.None;
                    }
                    catch (Exception ex)
                    {
                        Log("Exception in " + nameof(ImportDarkLoader) + "." + nameof(Import), ex);
                        return ImportError.Unknown;
                    }
                    finally
                    {
                        Core.View.InvokeSync(new Action(Core.ProgressBox.HideThis));
                    }
                });
            }

            if (error != ImportError.None) return (error, fms);

            if (importSaves)
            {
                bool success = await ImportSaves(lines);
            }

            var importedFMsInMainList = ImportCommon.MergeImportedFMData(ImportType.DarkLoader, fms, mainList);

            return (ImportError.None, importedFMsInMainList);
        }

        private static async Task<bool> ImportSaves(string[] lines)
        {
            var t1Dir = "";
            var t2Dir = "";
            var t1DirRead = false;
            var t2DirRead = false;

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var lineTS = line.TrimStart();
                var lineTB = lineTS.TrimEnd();

                if (lineTB == "[options]")
                {
                    while (i < lines.Length - 1)
                    {
                        var lt = lines[i + 1].Trim();
                        if (lt.StartsWithI("thief1dir="))
                        {
                            t1Dir = lt.Substring(10).Trim();
                            t1DirRead = true;
                        }
                        else if (lt.StartsWithI("thief2dir="))
                        {
                            t2Dir = lt.Substring(10).Trim();
                            t2DirRead = true;
                        }
                        else if (!lt.IsEmpty() && lt[0] == '[' && lt[lt.Length - 1] == ']')
                        {
                            break;
                        }
                        if (t1DirRead && t2DirRead) goto breakout;
                        i++;
                    }
                }
            }

            breakout:

            if (t1Dir.IsWhiteSpace() && t2Dir.IsWhiteSpace()) return true;

            await Task.Run(() =>
            {
                for (int i = 0; i < 2; i++)
                {
                    if (i == 0 && t1Dir.IsEmpty()) continue;
                    if (i == 1 && t2Dir.IsEmpty()) continue;

                    string savesPath = Path.Combine(i == 0 ? t1Dir : t2Dir, "allsaves");
                    if (!Directory.Exists(savesPath)) continue;

                    var convertedPath = Path.Combine(Common.Common.Config.FMsBackupPath, Paths.DarkLoaderSaveBakDir);
                    Directory.CreateDirectory(convertedPath);

                    // Converting takes too long, so just copy them to our backup folder and they'll be handled
                    // appropriately next time the user installs an FM
                    foreach (var f in Directory.EnumerateFiles(savesPath, "*.zip", SearchOption.TopDirectoryOnly))
                    {
                        var dest = Path.Combine(convertedPath, f.GetFileNameFast());
                        File.Copy(f, dest, overwrite: true);
                    }
                }
            });

            return true;
        }
    }
}
