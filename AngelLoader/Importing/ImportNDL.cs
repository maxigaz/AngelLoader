﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AngelLoader.Common;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;
using static AngelLoader.Common.Logger;
using static AngelLoader.Ini.Ini;

namespace AngelLoader.Importing
{
    internal static class ImportNDL
    {
        internal static async Task<(ImportError Error, List<FanMission> FMs)>
        Import(string iniFile, List<FanMission> mainList)
        {
            var lines = await Task.Run(() => File.ReadAllLines(iniFile));
            var fms = new List<FanMission>();

            var error = await Task.Run(() =>
            {
                bool archiveDirRead = false;
                string archiveDir = "";

                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];

                    #region Read archive directory

                    if (!archiveDirRead && line == "[Config]")
                    {
                        while (i < lines.Length - 1)
                        {
                            var lc = lines[i + 1];
                            if (lc.StartsWithFast_NoNullChecks("ArchiveRoot="))
                            {
                                archiveDir = lc.Substring(12).Trim();
                                break;
                            }
                            else if (!lc.IsEmpty() && lc[0] == '[' && lc[lc.Length - 1] == ']')
                            {
                                break;
                            }
                            i++;
                        }

                        if (archiveDir.IsEmpty()) return ImportError.NoArchiveDirsFound;

                        i = -1;
                        archiveDirRead = true;
                        continue;
                    }

                    #endregion

                    #region Read FM entries

                    // MUST CHECK archiveDirRead OR IT ADDS EVERY FM TWICE!
                    if (archiveDirRead &&
                        line.Length >= 5 && line[0] == '[' && line[1] == 'F' && line[2] == 'M' && line[3] == '=')
                    {
                        // NOTE: There can be a problem like:
                        // installed name is CoolMission[1]
                        // it gets written like [FM=CoolMission[1]]
                        // it gets read and all [ and ] chars are removed
                        // it gets written back out like [FM=CoolMission1]
                        // Rare I guess, so just ignore?
                        var instName = line.Substring(4, line.Length - 5);

                        var fm = new FanMission { InstalledDir = instName };

                        // Unfortunately NDL doesn't store its archive names, so we have to do a file search
                        // similar to DarkLoader
                        try
                        {
                            // NDL always searches subdirectories as well
                            foreach (var f in Directory.EnumerateFiles(archiveDir, "*",
                                SearchOption.AllDirectories))
                            {
                                if (!f.ContainsI(Path.DirectorySeparatorChar + ".fix" +
                                                 Path.DirectorySeparatorChar))
                                {
                                    var fn = Path.GetFileNameWithoutExtension(f);
                                    if (fn.ToInstDirNameNDL().EqualsI(instName) || fn.EqualsI(instName))
                                    {
                                        fm.Archive = Path.GetFileName(f);
                                        break;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log("Exception in NewDarkLoader archive dir file enumeration", ex);
                        }

                        while (i < lines.Length - 1)
                        {
                            var lineFM = lines[i + 1];
                            if (lineFM.StartsWithFast_NoNullChecks("NiceName="))
                            {
                                fm.Title = lineFM.Substring(9);
                            }
                            else if (lineFM.StartsWithFast_NoNullChecks("ReleaseDate="))
                            {
                                fm.ReleaseDate = ReadNullableHexDate(lineFM.Substring(12));
                            }
                            else if (lineFM.StartsWithFast_NoNullChecks("LastCompleted="))
                            {
                                fm.LastPlayed = ReadNullableHexDate(lineFM.Substring(14));
                            }
                            else if (lineFM.StartsWithFast_NoNullChecks("Finished="))
                            {
                                int.TryParse(lineFM.Substring(9), out int result);
                                // result will be 0 on fail, which is the empty value so it's fine
                                fm.FinishedOn = result;
                            }
                            else if (lineFM.StartsWithFast_NoNullChecks("Rating="))
                            {
                                fm.Rating = int.TryParse(lineFM.Substring(7), out int result) ? result : -1;
                            }
                            else if (lineFM.StartsWithFast_NoNullChecks("Comment="))
                            {
                                fm.Comment = lineFM.Substring(8);
                            }
                            else if (lineFM.StartsWithFast_NoNullChecks("ModExclude="))
                            {
                                var val = lineFM.Substring(11);
                                if (val == "*")
                                {
                                    fm.DisableAllMods = true;
                                }
                                else
                                {
                                    fm.DisabledMods = val;
                                }
                            }
                            else if (lineFM.StartsWithFast_NoNullChecks("Tags="))
                            {
                                var val = lineFM.Substring(5);
                                if (!val.IsEmpty() && val != "[none]") fm.TagsString = val;
                            }
                            else if (lineFM.StartsWithFast_NoNullChecks("InfoFile="))
                            {
                                fm.SelectedReadme = lineFM.Substring(9);
                            }
                            else if (lineFM.StartsWithFast_NoNullChecks("FMSize="))
                            {
                                ulong.TryParse(lineFM.Substring(7), out ulong result);
                                fm.SizeBytes = result;
                            }
                            else if (!lineFM.IsEmpty() && lineFM[0] == '[' && lineFM[lineFM.Length - 1] == ']')
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
            });

            if (error != ImportError.None) return (error, fms);

            var importedFMsInMainList = ImportCommon.MergeImportedFMData(ImportType.NewDarkLoader, fms, mainList);

            return (ImportError.None, importedFMsInMainList);
        }
    }
}
