﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngelLoader.Common;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;
using AngelLoader.Ini;
using SevenZip;
using static AngelLoader.Common.Common;
using static AngelLoader.Common.Utility.Methods;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace AngelLoader
{
    // TODO: Important! FMSel probably puts any other diffed stuff in its backup files too, so account for that.
    // If we want just the screens and saves, just extract those. If we implement a similar thing to FMSel for
    // backing up and restoring diffs, then we can just extract everything again.

    // TODO: Allow import of NDL's .dml fixes
    /* Process:
    -Find dml backups (easy)
    -If FM is installed, put it into the installed folder (always do this)
    -then:
     -if our bak file exists, put it into there, making sure to also remove it from the fmsel.inf remove list if
      it's there
     -else if our bak file doesn't exist:
      -If NDL's bak file exists, create a new bak file in our folder and put everything in NDL's bak file, plus
       our found .dml, into there, making sure to also remove it from the fmsel.inf remove list if it's there
      -else if no bak files exist:
       -Just create a new bak file in our folder, and put the dml in
    */

    // NOTE: Zip quirk: LastWriteTime (and presumably any other metadata) must be set BEFORE opening the entry
    //       for writing. Even if you put it after the using block, it throws. So always set this before writing!

    internal static class FMBackupAndRestore
    {
        internal static async Task BackupFM(FanMission fm, string fmInstalledPath, string fmArchivePath)
        {
            bool backupSavesAndScreensOnly = Config.BackupFMData == BackupFMData.SavesAndScreensOnly &&
                                         (fm.Game != Game.Thief3 || !Config.T3UseCentralSaves);
            bool backupAll = Config.BackupFMData == BackupFMData.AllChangedFiles;

            if (!GameIsKnownAndSupported(fm))
            {
                // log it
                return;
            }

            await Task.Run(() =>
            {
                if (backupSavesAndScreensOnly && fm.InstalledDir.IsEmpty()) return;

                var thisFMInstallsBasePath = GetFMInstallsBasePath(fm);
                var savesDir = fm.Game == Game.Thief3 ? "SaveGames" : "saves";
                var savesPath = Path.Combine(thisFMInstallsBasePath, fm.InstalledDir, savesDir);
                // Screenshots directory name is the same for T1/T2/T3
                var screensPath = Path.Combine(thisFMInstallsBasePath, fm.InstalledDir, "screenshots");

                var bakFile = Path.Combine(Config.FMsBackupPath,
                    (!fm.Archive.IsEmpty() ? fm.Archive.RemoveExtension() : fm.InstalledDir) +
                    Paths.FMBackupSuffix);

                if (backupSavesAndScreensOnly)
                {
                    var savesAndScreensFiles = new List<string>();

                    if (Directory.Exists(savesPath))
                    {
                        savesAndScreensFiles.AddRange(Directory.GetFiles(savesPath, "*", SearchOption.AllDirectories));
                    }
                    if (Directory.Exists(screensPath))
                    {
                        savesAndScreensFiles.AddRange(Directory.GetFiles(screensPath, "*", SearchOption.AllDirectories));
                    }

                    if (savesAndScreensFiles.Count == 0) return;

                    using (var archive =
                        new ZipArchive(new FileStream(bakFile, FileMode.Create, FileAccess.Write),
                            ZipArchiveMode.Create))
                    {
                        foreach (var f in savesAndScreensFiles)
                        {
                            var fn = f.Substring(fmInstalledPath.Length).Trim(Path.DirectorySeparatorChar);
                            AddEntry(archive, f, fn);
                        }
                    }

                    return;
                }

                var installedFMFiles = Directory.GetFiles(fmInstalledPath, "*", SearchOption.AllDirectories);

                bool fmIsT3 = fm.Game == Game.Thief3;
                var (changedList, addedList, fullList) =
                    GetFMDiff(installedFMFiles, fmInstalledPath, fmArchivePath, fmIsT3);

                try
                {
                    using (var archive =
                        new ZipArchive(new FileStream(bakFile, FileMode.Create, FileAccess.Write),
                            ZipArchiveMode.Create))
                    {
                        foreach (var f in installedFMFiles)
                        {
                            var fn = f.Substring(fmInstalledPath.Length).Replace('\\', '/').Trim('/');
                            if (IsSaveOrScreenshot(fn, fmIsT3) ||
                                (!fn.EqualsI("fmsel.inf") && (changedList.ContainsI(fn) || addedList.ContainsI(fn))))
                            {
                                AddEntry(archive, f, fn);
                            }
                        }

                        var fmSelInfString = "";
                        for (var i = 0; i < fullList.Count; i++)
                        {
                            var f = fullList[i];
                            if (!installedFMFiles.ContainsI(
                                Path.Combine(fmInstalledPath, f).Replace('/', Path.DirectorySeparatorChar)))
                            {
                                fmSelInfString += "RemoveFile=" + f.Replace('\\', '/') + "\r\n";
                            }
                        }

                        if (!fmSelInfString.IsEmpty())
                        {
                            var entry = archive.CreateEntry("fmsel.inf", CompressionLevel.Fastest);
                            using (var eo = entry.Open())
                            using (var sw = new StreamWriter(eo, Encoding.UTF8))
                            {
                                sw.Write(fmSelInfString);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // log it
                }
            });
        }

        private static void AddEntry(ZipArchive archive, string fileNameOnDisk, string entryFileName,
            CompressionLevel compressionLevel = CompressionLevel.Fastest)
        {
            var entry = archive.CreateEntry(entryFileName, compressionLevel);
            entry.LastWriteTime = new FileInfo(fileNameOnDisk).LastWriteTime;
            using (var fs = new FileStream(fileNameOnDisk, FileMode.Open, FileAccess.Read))
            using (var eo = entry.Open())
            {
                fs.CopyTo(eo);
            }
        }

        private static bool IsSaveOrScreenshot(string path, bool fmIsT3)
        {
            return path.StartsWithI("screenshots/") ||
                   (fmIsT3 && path.StartsWithI("SaveGames/")) ||
                   (!fmIsT3 && path.StartsWithI("saves/"));
        }

        private static (List<string> ChangedList, List<string> AddedList, List<string> FullList)
        GetFMDiff(string[] installedFMFiles, string fmInstalledPath, string fmArchivePath, bool fmIsT3)
        {
            var changedList = new List<string>();
            var addedList = new List<string>();
            var fullList = new List<string>();

            bool fmIsZip = fmArchivePath.ExtEqualsI(".zip");
            if (fmIsZip)
            {
                using (var archive = new ZipArchive(new FileStream(fmArchivePath, FileMode.Open, FileAccess.Read),
                    ZipArchiveMode.Read, leaveOpen: false))
                {
                    for (var i = 0; i < archive.Entries.Count; i++)
                    {
                        var entry = archive.Entries[i];
                        var efn = entry.FullName.Replace('\\', '/');

                        if (efn.EqualsI("fmsel.inf") ||
                            (efn.Length > 0 && efn[efn.Length - 1] == '/') ||
                            IsSaveOrScreenshot(efn, fmIsT3))
                        {
                            continue;
                        }

                        fullList.Add(entry.FullName);

                        var fileInInstalledDir = Path.Combine(fmInstalledPath, entry.FullName);
                        if (installedFMFiles.ContainsI(fileInInstalledDir.Replace('/', Path.DirectorySeparatorChar)))
                        {
                            try
                            {
                                var fi = new FileInfo(fileInInstalledDir);
                                if (fi.LastWriteTime.ToUniversalTime() != entry.LastWriteTime.ToUniversalTime())
                                {
                                    changedList.Add(entry.FullName);
                                }
                            }
                            catch (Exception ex)
                            {
                                // log it
                            }
                        }
                    }
                    foreach (var f in installedFMFiles)
                    {
                        var fn = f.Substring(fmInstalledPath.Length).Replace('\\', '/').Trim('/');

                        if (fn.EqualsI("fmsel.inf") || IsSaveOrScreenshot(fn, fmIsT3))
                        {
                            continue;
                        }

                        bool found = false;
                        for (var i = 0; i < archive.Entries.Count; i++)
                        {
                            if (archive.Entries[i].FullName.EqualsI(fn))
                            {
                                found = true;
                                break;
                            }
                        }
                        if (!found) addedList.Add(fn);
                    }
                }
            }
            else
            {
                using (var archive = new SevenZipExtractor(fmArchivePath))
                {
                    for (var i = 0; i < archive.ArchiveFileData.Count; i++)
                    {
                        var entry = archive.ArchiveFileData[i];
                        var efn = entry.FileName.Replace('\\', '/');

                        if (
                            efn.EqualsI("fmsel.inf") ||
                            // IsDirectory has been unreliable in the past, so check manually here too
                            entry.IsDirectory || (efn.Length > 0 && efn[efn.Length - 1] == '/') ||
                            IsSaveOrScreenshot(efn, fmIsT3))
                        {
                            continue;
                        }

                        fullList.Add(efn);

                        var fileInInstalledDir = Path.Combine(fmInstalledPath, efn);
                        if (File.Exists(fileInInstalledDir))
                        {
                            try
                            {
                                var fi = new FileInfo(fileInInstalledDir);
                                if (fi.LastWriteTime.ToUniversalTime() != entry.LastWriteTime.ToUniversalTime())
                                {
                                    changedList.Add(efn);
                                }
                            }
                            catch (Exception ex)
                            {
                                // log it
                            }
                        }
                    }
                    foreach (var f in installedFMFiles)
                    {
                        if (Path.GetFileName(f).EqualsI("fmsel.inf")) continue;

                        var fn = f.Substring(fmInstalledPath.Length).Replace('\\', '/').Trim('/');

                        bool found = false;
                        for (var i = 0; i < archive.ArchiveFileData.Count; i++)
                        {
                            var entry = archive.ArchiveFileData[i];
                            var efn = entry.FileName.Replace('\\', '/');
                            if (!entry.IsDirectory && efn.EqualsI(fn))
                            {
                                found = true;
                                break;
                            }
                        }
                        if (!found) addedList.Add(fn);
                    }
                }
            }

            return (changedList, addedList, fullList);
        }

        internal static async Task RestoreSavesAndScreenshots(FanMission fm)
        {
            bool restoreSavesAndScreensOnly = Config.BackupFMData == BackupFMData.SavesAndScreensOnly &&
                                             (fm.Game != Game.Thief3 || !Config.T3UseCentralSaves);
            bool fmIsT3 = fm.Game == Game.Thief3;

            //if (fm.Game == Game.Thief3 && Config.T3UseCentralSaves)
            //{
            //    // log it
            //    return;
            //}

            await Task.Run(() =>
            {
                (string Name, bool DarkLoader) fileToUse = ("", false);

                #region DarkLoader

                string dlBakDir = Path.Combine(Config.FMsBackupPath, Paths.DarkLoaderSaveBakDir);

                if (Directory.Exists(dlBakDir))
                {
                    foreach (var f in Directory.EnumerateFiles(dlBakDir, "*.zip", SearchOption.TopDirectoryOnly))
                    {
                        var fn = f.GetFileNameFast();
                        int index = fn.LastIndexOf("_saves.zip", StringComparison.OrdinalIgnoreCase);
                        if (index == -1) continue;

                        var an = fn.Substring(0, index).Trim();
                        // Account for the fact that DarkLoader trims archive names for save backup zips
                        // Note: I guess it doesn't?! The code heavily implies it does. Still, it works either
                        // way, so whatever.
                        if (!an.IsEmpty() && an.EqualsI(fm.Archive.RemoveExtension().Trim()))
                        {
                            fileToUse = (f, true);
                            break;
                        }
                    }
                }

                #endregion

                #region AngelLoader / FMSel / NewDarkLoader

                if (fileToUse.Name.IsEmpty())
                {
                    var bakFiles = new List<FileInfo>();

                    void AddBakFilesFrom(string path)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            var fNoExt = i == 0 ? fm.Archive.RemoveExtension() : fm.InstalledDir;
                            var bakFile = Path.Combine(path, fNoExt + Paths.FMBackupSuffix);
                            if (File.Exists(bakFile)) bakFiles.Add(new FileInfo(bakFile));
                        }
                    }

                    // Our backup path, separate to avoid creating any more ambiguity
                    AddBakFilesFrom(Config.FMsBackupPath);

                    // If ArchiveName.bak and InstalledName.bak files both exist, use the newest of the two
                    fileToUse.Name = bakFiles.Count == 1
                        ? bakFiles[0].FullName
                        : bakFiles.Count > 1
                        ? bakFiles.OrderByDescending(x => x.LastWriteTime).ToList()[0].FullName
                        : "";

                    bakFiles.Clear();

                    // Use file from our bak dir if it exists, otherwise use the newest file from all archive dirs
                    // (for automatic use of FMSel/NDL saves)
                    if (fileToUse.Name.IsEmpty())
                    {
                        foreach (var path in GetFMArchivePaths()) AddBakFilesFrom(path);

                        if (bakFiles.Count == 0) return;

                        // Use the newest of all files found in all archive dirs
                        fileToUse.Name = bakFiles.OrderByDescending(x => x.LastWriteTime).ToList()[0].FullName;
                    }
                }

                #endregion

                var excludes = new List<string>();

                var thisFMInstallsBasePath = GetFMInstallsBasePath(fm);
                var fmInstalledPath = Path.Combine(thisFMInstallsBasePath, fm.InstalledDir);

                using (var archive = new ZipArchive(new FileStream(fileToUse.Name, FileMode.Open, FileAccess.Read),
                    ZipArchiveMode.Read))
                {
                    var filesCount = archive.Entries.Count;
                    if (fileToUse.DarkLoader)
                    {
                        var fmSavesPath = Path.Combine(fmInstalledPath, "saves");
                        for (var i = 0; i < filesCount; i++)
                        {
                            var f = archive.Entries[i];
                            var fn = f.FullName.Replace('\\', '/');
                            if ((fn.Length > 0 && fn[fn.Length - 1] == '/') ||
                                !fn.StartsWithI("saves/"))
                            {
                                continue;
                            }

                            Directory.CreateDirectory(Path.Combine(fmInstalledPath,
                                fn.Substring(0, fn.LastIndexOf('/'))));

                            f.ExtractToFile(Path.Combine(fmInstalledPath, fn), overwrite: true);
                        }
                    }
                    else
                    {
                        var savesDir = fmIsT3 ? "SaveGames" : "saves";
                        const string screensDir = "screenshots";
                        if (restoreSavesAndScreensOnly)
                        {
                            for (var i = 0; i < filesCount; i++)
                            {
                                var f = archive.Entries[i];
                                var fn = f.FullName.Replace('\\', '/');

                                if ((fn.Length > 0 && fn[fn.Length - 1] == '/') ||
                                    (!fn.StartsWithI(savesDir + '/') && !fn.StartsWithI(screensDir + '/')))
                                {
                                    continue;
                                }

                                Directory.CreateDirectory(Path.Combine(fmInstalledPath,
                                    fn.Substring(0, fn.LastIndexOf('/'))));

                                f.ExtractToFile(Path.Combine(fmInstalledPath, fn), overwrite: true);
                            }
                        }
                        else
                        {
                            var fmSelInf = archive.GetEntry("fmsel.inf");
                            // Cap the length, cause... well, nobody's going to put a 500MB binary file named
                            // fmsel.inf, but hey...
                            if (fmSelInf != null && fmSelInf.Length < ByteSize.MB * 5)
                            {
                                using (var eo = fmSelInf.Open())
                                using (var sr = new StreamReader(eo))
                                {
                                    string line;
                                    while ((line = sr.ReadLine()) != null)
                                    {
                                        if (line.StartsWithFast_NoNullChecks("RemoveFile="))
                                        {
                                            var val = line.Substring(11).Replace('\\', '/').Trim();
                                            if (!val.StartsWithI(savesDir + '/') &&
                                                !val.StartsWithI(screensDir + '/') &&
                                                !val.EqualsI("fmsel.inf") &&
                                                // Maliciously formed path checks
                                                !val.StartsWith("/") &&
                                                !val.Contains(':') &&
                                                !val.Contains("./"))
                                            {
                                                excludes.Add(val);
                                            }
                                        }
                                    }
                                }
                            }

                            for (var i = 0; i < filesCount; i++)
                            {
                                var f = archive.Entries[i];
                                var fn = f.FullName.Replace('\\', '/');

                                if (fn.EqualsI("fmsel.inf") ||
                                    (fn.Length > 0 && fn[fn.Length - 1] == '/') ||
                                    excludes.Contains(fn))
                                {
                                    continue;
                                }

                                if (fn.Contains('/'))
                                {
                                    Directory.CreateDirectory(Path.Combine(fmInstalledPath,
                                        fn.Substring(0, fn.LastIndexOf('/'))));
                                }

                                f.ExtractToFile(Path.Combine(fmInstalledPath, fn), overwrite: true);
                            }
                        }
                    }
                }

                if (!restoreSavesAndScreensOnly)
                {
                    foreach (var f in Directory.EnumerateFiles(fmInstalledPath, "*", SearchOption.AllDirectories))
                    {
                        if (excludes.ContainsI(f.Substring(fmInstalledPath.Length)
                            .Replace(Path.DirectorySeparatorChar, '/').Trim('/')))
                        {
                            File.Delete(f);
                        }
                    }
                }

                if (fileToUse.DarkLoader)
                {
                    var dlOrigBakDir = Path.Combine(Config.FMsBackupPath, Paths.DarkLoaderSaveOrigBakDir);
                    Directory.CreateDirectory(dlOrigBakDir);
                    File.Move(fileToUse.Name, Path.Combine(dlOrigBakDir, fileToUse.Name.GetFileNameFast()));
                }
            });
        }
    }
}
