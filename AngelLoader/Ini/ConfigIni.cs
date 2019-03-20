﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AngelLoader.Common;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;

namespace AngelLoader.Ini
{
    // TODO: Maybe make this file have sections, cause it's getting pretty giant-blob-like

    internal static partial class Ini
    {
        // Not autogenerating these, because there's too many special cases, and adding stuff by hand is not that
        // big of a deal really.

        private static DateTime? ReadNullableDate(string hexDate)
        {
            var success = long.TryParse(
                hexDate,
                NumberStyles.HexNumber,
                DateTimeFormatInfo.InvariantInfo,
                out long result);

            if (!success) return null;

            try
            {
                var dateTime = DateTimeOffset
                    .FromUnixTimeSeconds(result)
                    .DateTime
                    .ToLocalTime();

                return dateTime;
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }

        private static void ReadTags(string line, Filter filter, string prefix)
        {
            var tagsList =
                line.StartsWithI(prefix + "FilterTagsAnd=") ? filter.Tags.AndTags :
                line.StartsWithI(prefix + "FilterTagsOr=") ? filter.Tags.OrTags :
                line.StartsWithI(prefix + "FilterTagsNot=") ? filter.Tags.NotTags :
                null;

            var val = line.Substring(line.IndexOf('=') + 1);

            if (tagsList == null || val.IsWhiteSpace()) return;

            var tagsArray = val.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var item in tagsArray)
            {
                string cat, tag;
                var colonCount = item.CountChars(':');
                if (colonCount > 1) continue;
                if (colonCount == 1)
                {
                    var index = item.IndexOf(':');
                    cat = item.Substring(0, index).Trim().ToLowerInvariant();
                    tag = item.Substring(index + 1).Trim();
                    if (cat.IsEmpty()) continue;
                }
                else
                {
                    cat = "misc";
                    tag = item.Trim();
                }

                CatAndTags match = null;
                for (int i = 0; i < tagsList.Count; i++)
                {
                    if (tagsList[i].Category == cat) match = tagsList[i];
                }
                if (match == null)
                {
                    tagsList.Add(new CatAndTags { Category = cat });
                    if (!tag.IsEmpty()) tagsList[tagsList.Count - 1].Tags.Add(tag);
                }
                else
                {
                    if (!tag.IsEmpty() && !match.Tags.ContainsI(tag)) match.Tags.Add(tag);
                }
            }
        }

        internal static void ReadFinishedStates(string val, Filter filter)
        {
            var list = val.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            foreach (var finishedState in list)
            {
                switch (finishedState.Trim())
                {
                    case nameof(FinishedState.Finished):
                        filter.Finished.Add(FinishedState.Finished);
                        break;
                    case nameof(FinishedState.Unfinished):
                        filter.Finished.Add(FinishedState.Unfinished);
                        break;
                }
            }
        }

        // I tried removing the reflection in this one and it measured no faster, so leaving it as is.
        internal static void ReadConfigIni(string path, ConfigData config)
        {
            var iniLines = File.ReadAllLines(path);

            foreach (var line in iniLines)
            {
                if (!line.Contains('=')) continue;

                var lineT = line.TrimStart();

                if (lineT.Length > 0 && (lineT[0] == ';' || lineT[0] == '[')) continue;

                var val = lineT.Substring(lineT.IndexOf('=') + 1);

                if (lineT.StartsWithI("Column") && line[6] != '=')
                {
                    var colName = lineT.Substring(6, lineT.IndexOf('=') - 6);

                    var field = typeof(Column).GetField(colName, BFlagsEnum);
                    if (field == null) continue;

                    var col = ConvertStringToColumnData(val);
                    if (col == null) continue;

                    col.Id = (Column)field.GetValue(null);
                    if (config.Columns.Any(x => x.Id == col.Id)) continue;

                    config.Columns.Add(col);
                }
                #region Filter

                else if (lineT.StartsWithI("FilterTitle="))
                {
                    config.Filter.Title = val;
                }
                else if (lineT.StartsWithI("T1FilterTitle="))
                {
                    config.GameTabsState.T1Filter.Title = val;
                }
                else if (lineT.StartsWithI("T2FilterTitle="))
                {
                    config.GameTabsState.T2Filter.Title = val;
                }
                else if (lineT.StartsWithI("T3FilterTitle="))
                {
                    config.GameTabsState.T3Filter.Title = val;
                }
                else if (lineT.StartsWithI("FilterAuthor="))
                {
                    config.Filter.Author = val;
                }
                else if (lineT.StartsWithI("T1FilterAuthor="))
                {
                    config.GameTabsState.T1Filter.Author = val;
                }
                else if (lineT.StartsWithI("T2FilterAuthor="))
                {
                    config.GameTabsState.T2Filter.Author = val;
                }
                else if (lineT.StartsWithI("T3FilterAuthor="))
                {
                    config.GameTabsState.T3Filter.Author = val;
                }
                else if (lineT.StartsWithI("FilterReleaseDateFrom="))
                {
                    config.Filter.ReleaseDateFrom = ReadNullableDate(val);
                }
                else if (lineT.StartsWithI("T1FilterReleaseDateFrom="))
                {
                    config.GameTabsState.T1Filter.ReleaseDateFrom = ReadNullableDate(val);
                }
                else if (lineT.StartsWithI("T2FilterReleaseDateFrom="))
                {
                    config.GameTabsState.T2Filter.ReleaseDateFrom = ReadNullableDate(val);
                }
                else if (lineT.StartsWithI("T3FilterReleaseDateFrom="))
                {
                    config.GameTabsState.T3Filter.ReleaseDateFrom = ReadNullableDate(val);
                }
                else if (lineT.StartsWithI("FilterReleaseDateTo="))
                {
                    config.Filter.ReleaseDateTo = ReadNullableDate(val);
                }
                else if (lineT.StartsWithI("T1FilterReleaseDateTo="))
                {
                    config.GameTabsState.T1Filter.ReleaseDateTo = ReadNullableDate(val);
                }
                else if (lineT.StartsWithI("T2FilterReleaseDateTo="))
                {
                    config.GameTabsState.T2Filter.ReleaseDateTo = ReadNullableDate(val);
                }
                else if (lineT.StartsWithI("T3FilterReleaseDateTo="))
                {
                    config.GameTabsState.T3Filter.ReleaseDateTo = ReadNullableDate(val);
                }
                else if (lineT.StartsWithI("FilterLastPlayedFrom="))
                {
                    config.Filter.LastPlayedFrom = ReadNullableDate(val);
                }
                else if (lineT.StartsWithI("T1FilterLastPlayedFrom="))
                {
                    config.GameTabsState.T1Filter.LastPlayedFrom = ReadNullableDate(val);
                }
                else if (lineT.StartsWithI("T2FilterLastPlayedFrom="))
                {
                    config.GameTabsState.T2Filter.LastPlayedFrom = ReadNullableDate(val);
                }
                else if (lineT.StartsWithI("T3FilterLastPlayedFrom="))
                {
                    config.GameTabsState.T3Filter.LastPlayedFrom = ReadNullableDate(val);
                }
                else if (lineT.StartsWithI("FilterLastPlayedTo="))
                {
                    config.Filter.LastPlayedTo = ReadNullableDate(val);
                }
                else if (lineT.StartsWithI("T1FilterLastPlayedTo="))
                {
                    config.GameTabsState.T1Filter.LastPlayedTo = ReadNullableDate(val);
                }
                else if (lineT.StartsWithI("T2FilterLastPlayedTo="))
                {
                    config.GameTabsState.T2Filter.LastPlayedTo = ReadNullableDate(val);
                }
                else if (lineT.StartsWithI("T3FilterLastPlayedTo="))
                {
                    config.GameTabsState.T3Filter.LastPlayedTo = ReadNullableDate(val);
                }
                else if (lineT.StartsWithI("FilterTags") && line[10] != '=')
                {
                    ReadTags(lineT, config.Filter, "");
                }
                else if (lineT.StartsWithI("T1FilterTags") && line[12] != '=')
                {
                    ReadTags(lineT, config.GameTabsState.T1Filter, "T1");
                }
                else if (lineT.StartsWithI("T2FilterTags") && line[12] != '=')
                {
                    ReadTags(lineT, config.GameTabsState.T2Filter, "T2");
                }
                else if (lineT.StartsWithI("T3FilterTags") && line[12] != '=')
                {
                    ReadTags(lineT, config.GameTabsState.T3Filter, "T3");
                }
                else if (lineT.StartsWithI("FilterGames="))
                {
                    var list = val.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Distinct(StringComparer.OrdinalIgnoreCase).ToList();

                    foreach (var game in list)
                    {
                        switch (game.Trim())
                        {
                            case nameof(Game.Thief1):
                                config.Filter.Games.Add(Game.Thief1);
                                break;
                            case nameof(Game.Thief2):
                                config.Filter.Games.Add(Game.Thief2);
                                break;
                            case nameof(Game.Thief3):
                                config.Filter.Games.Add(Game.Thief3);
                                break;
                        }
                    }
                }
                else if (lineT.StartsWithI("FilterRatingFrom="))
                {
                    if (int.TryParse(val, out int result)) config.Filter.RatingFrom = result;
                }
                else if (lineT.StartsWithI("T1FilterRatingFrom="))
                {
                    if (int.TryParse(val, out int result)) config.GameTabsState.T1Filter.RatingFrom = result;
                }
                else if (lineT.StartsWithI("T2FilterRatingFrom="))
                {
                    if (int.TryParse(val, out int result)) config.GameTabsState.T2Filter.RatingFrom = result;
                }
                else if (lineT.StartsWithI("T3FilterRatingFrom="))
                {
                    if (int.TryParse(val, out int result)) config.GameTabsState.T3Filter.RatingFrom = result;
                }
                else if (lineT.StartsWithI("FilterRatingTo="))
                {
                    if (int.TryParse(val, out int result)) config.Filter.RatingTo = result;
                }
                else if (lineT.StartsWithI("T1FilterRatingTo="))
                {
                    if (int.TryParse(val, out int result)) config.GameTabsState.T1Filter.RatingTo = result;
                }
                else if (lineT.StartsWithI("T2FilterRatingTo="))
                {
                    if (int.TryParse(val, out int result)) config.GameTabsState.T2Filter.RatingTo = result;
                }
                else if (lineT.StartsWithI("T3FilterRatingTo="))
                {
                    if (int.TryParse(val, out int result)) config.GameTabsState.T3Filter.RatingTo = result;
                }
                else if (lineT.StartsWithI("FilterFinishedStates="))
                {
                    ReadFinishedStates(val, config.Filter);
                }
                else if (lineT.StartsWithI("T1FilterFinishedStates="))
                {
                    ReadFinishedStates(val, config.GameTabsState.T1Filter);
                }
                else if (lineT.StartsWithI("T2FilterFinishedStates="))
                {
                    ReadFinishedStates(val, config.GameTabsState.T2Filter);
                }
                else if (lineT.StartsWithI("T3FilterFinishedStates="))
                {
                    ReadFinishedStates(val, config.GameTabsState.T3Filter);
                }
                else if (lineT.StartsWithI("FilterShowJunk="))
                {
                    config.Filter.ShowJunk = val.EqualsTrue();
                }
                else if (lineT.StartsWithI("T1FilterShowJunk="))
                {
                    config.GameTabsState.T1Filter.ShowJunk = val.EqualsTrue();
                }
                else if (lineT.StartsWithI("T2FilterShowJunk="))
                {
                    config.GameTabsState.T2Filter.ShowJunk = val.EqualsTrue();
                }
                else if (lineT.StartsWithI("T3FilterShowJunk="))
                {
                    config.GameTabsState.T3Filter.ShowJunk = val.EqualsTrue();
                }

                #endregion
                else if (lineT.StartsWithI(nameof(config.EnableArticles) + "="))
                {
                    config.EnableArticles = val.EqualsTrue();
                }
                else if (lineT.StartsWithI(nameof(config.Articles) + "="))
                {
                    var articles = val.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    for (var a = 0; a < articles.Length; a++) articles[a] = articles[a].Trim();
                    config.Articles.Clear();
                    config.Articles.AddRange(articles.Distinct(StringComparer.OrdinalIgnoreCase));
                }
                else if (lineT.StartsWithI(nameof(config.MoveArticlesToEnd) + "="))
                {
                    config.MoveArticlesToEnd = val.EqualsTrue();
                }
                else if (lineT.StartsWithI(nameof(config.SortDirection) + "="))
                {
                    if (val.EqualsI("Ascending"))
                    {
                        config.SortDirection = SortOrder.Ascending;
                    }
                    else if (val.EqualsI("Descending"))
                    {
                        config.SortDirection = SortOrder.Descending;
                    }
                }
                else if (lineT.StartsWithI(nameof(config.SortedColumn) + "="))
                {
                    var field = typeof(Column).GetField(val, BFlagsEnum);
                    if (field != null)
                    {
                        config.SortedColumn = (Column)field.GetValue(null);
                    }
                }
                else if (lineT.StartsWithI(nameof(config.RatingDisplayStyle) + "="))
                {
                    var field = typeof(RatingDisplayStyle).GetField(val, BFlagsEnum);
                    if (field != null)
                    {
                        config.RatingDisplayStyle = (RatingDisplayStyle)field.GetValue(null);
                    }
                }
                else if (lineT.StartsWithI(nameof(config.RatingDisplayStyle) + "="))
                {
                    config.RatingUseStars = val.EqualsTrue();
                }
                else if (lineT.StartsWithI(nameof(config.TopRightTab) + "="))
                {
                    var field = typeof(TopRightTab).GetField(val, BFlagsEnum);
                    if (field != null)
                    {
                        config.TopRightTab = (TopRightTab)field.GetValue(null);
                    }
                }
                else if (lineT.StartsWithI(nameof(config.SettingsTab) + "="))
                {
                    var field = typeof(SettingsTab).GetField(val, BFlagsEnum);
                    if (field != null)
                    {
                        config.SettingsTab = (SettingsTab)field.GetValue(null);
                    }
                }
                else if (lineT.StartsWithI("FMArchivePath="))
                {
                    config.FMArchivePaths.Add(val.Trim());
                }
                else if (lineT.StartsWithI(nameof(config.FMArchivePathsIncludeSubfolders) + "="))
                {
                    config.FMArchivePathsIncludeSubfolders = val.EqualsTrue();
                }
                else if (lineT.StartsWithI(nameof(config.FMsBackupPath) + "="))
                {
                    config.FMsBackupPath = val.Trim();
                }
                else if (lineT.StartsWithI(nameof(config.T1Exe) + "="))
                {
                    config.T1Exe = val.Trim();
                }
                else if (lineT.StartsWithI(nameof(config.T2Exe) + "="))
                {
                    config.T2Exe = val.Trim();
                }
                else if (lineT.StartsWithI(nameof(config.T3Exe) + "="))
                {
                    config.T3Exe = val.Trim();
                }
                else if (lineT.StartsWithI(nameof(config.GameOrganization) + "="))
                {
                    var field = typeof(GameOrganization).GetField(val, BFlagsEnum);
                    if (field != null)
                    {
                        config.GameOrganization = (GameOrganization)field.GetValue(null);
                    }
                }
                else if (lineT.StartsWithI(nameof(config.GameTab) + "="))
                {
                    switch (val)
                    {
                        case nameof(Game.Thief2):
                            config.GameTab = Game.Thief2;
                            break;
                        case nameof(Game.Thief3):
                            config.GameTab = Game.Thief3;
                            break;
                        default:
                            config.GameTab = Game.Thief1;
                            break;
                    }
                }
                else if (lineT.StartsWithI("T1SelFMInstDir="))
                {
                    config.GameTabsState.T1SelFM.InstalledName = val;
                }
                else if (lineT.StartsWithI("T2SelFMInstDir="))
                {
                    config.GameTabsState.T2SelFM.InstalledName = val;
                }
                else if (lineT.StartsWithI("T3SelFMInstDir="))
                {
                    config.GameTabsState.T3SelFM.InstalledName = val;
                }
                else if (lineT.StartsWithI("T1SelFMIndexFromTop="))
                {
                    if (int.TryParse(val, out int result))
                    {
                        config.GameTabsState.T1SelFM.IndexFromTop = result;
                    }
                }
                else if (lineT.StartsWithI("T2SelFMIndexFromTop="))
                {
                    if (int.TryParse(val, out int result))
                    {
                        config.GameTabsState.T2SelFM.IndexFromTop = result;
                    }
                }
                else if (lineT.StartsWithI("T3SelFMIndexFromTop="))
                {
                    if (int.TryParse(val, out int result))
                    {
                        config.GameTabsState.T3SelFM.IndexFromTop = result;
                    }
                }
                else if (lineT.StartsWithI("SelFMInstDir="))
                {
                    config.SelFM.InstalledName = val;
                }
                else if (lineT.StartsWithI("SelFMIndexFromTop="))
                {
                    if (int.TryParse(val, out int result))
                    {
                        config.SelFM.IndexFromTop = result;
                    }
                }
                else if (lineT.StartsWithI(nameof(config.DateFormat) + "="))
                {
                    var field = typeof(DateFormat).GetField(val, BFlagsEnum);
                    if (field != null)
                    {
                        config.DateFormat = (DateFormat)field.GetValue(null);
                    }
                }
                else if (lineT.StartsWithI(nameof(config.DateCustomFormat1) + "="))
                {
                    config.DateCustomFormat1 = val;
                }
                else if (lineT.StartsWithI(nameof(config.DateCustomSeparator1) + "="))
                {
                    config.DateCustomSeparator1 = val;
                }
                else if (lineT.StartsWithI(nameof(config.DateCustomFormat2) + "="))
                {
                    config.DateCustomFormat2 = val;
                }
                else if (lineT.StartsWithI(nameof(config.DateCustomSeparator2) + "="))
                {
                    config.DateCustomSeparator2 = val;
                }
                else if (lineT.StartsWithI(nameof(config.DateCustomFormat3) + "="))
                {
                    config.DateCustomFormat3 = val;
                }
                else if (lineT.StartsWithI(nameof(config.DateCustomSeparator3) + "="))
                {
                    config.DateCustomSeparator3 = val;
                }
                else if (lineT.StartsWithI(nameof(config.DateCustomFormat4) + "="))
                {
                    config.DateCustomFormat4 = val;
                }
                else if (lineT.StartsWithI(nameof(config.ReadmeZoomFactor) + "="))
                {
                    if (float.TryParse(val, out float result))
                    {
                        config.ReadmeZoomFactor = result;
                    }
                }
                else if (lineT.StartsWithI(nameof(config.MainWindowState) + "="))
                {
                    var field = typeof(FormWindowState).GetField(val, BFlagsEnum);
                    if (field != null)
                    {
                        var windowState = (FormWindowState)field.GetValue(null);
                        if (windowState != FormWindowState.Minimized)
                        {
                            config.MainWindowState = windowState;
                        }
                    }
                }
                else if (lineT.StartsWithI(nameof(config.MainWindowSize) + "="))
                {
                    if (!val.Contains(',')) continue;

                    var values = val.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    var widthExists = int.TryParse(values[0].Trim(), out var width);
                    var heightExists = int.TryParse(values[1].Trim(), out var height);

                    if (widthExists && heightExists)
                    {
                        config.MainWindowSize = new Size(width, height);
                    }
                }
                else if (lineT.StartsWithI(nameof(config.MainHorizontalSplitterDistance) + "="))
                {
                    if (int.TryParse(val, out int result))
                    {
                        config.MainHorizontalSplitterDistance = result;
                    }
                }
                else if (lineT.StartsWithI(nameof(config.TopVerticalSplitterDistance) + "="))
                {
                    if (int.TryParse(val, out int result))
                    {
                        config.TopVerticalSplitterDistance = result;
                    }
                }
                else if (lineT.StartsWithI(nameof(config.ConvertWAVsTo16BitOnInstall) + "="))
                {
                    config.ConvertWAVsTo16BitOnInstall = val.EqualsTrue();
                }
                else if (lineT.StartsWithI(nameof(config.ConvertOGGsToWAVsOnInstall) + "="))
                {
                    config.ConvertOGGsToWAVsOnInstall = val.EqualsTrue();
                }
                else if (lineT.StartsWithI(nameof(config.BackupSaves) + "="))
                {
                    var field = typeof(BackupSaves).GetField(val, BFlagsEnum);
                    if (field != null)
                    {
                        config.BackupSaves = (BackupSaves)field.GetValue(null);
                    }
                }
                else if (lineT.StartsWithI(nameof(config.Language) + "="))
                {
                    config.Language = val;
                }
                else if (lineT.StartsWithI(nameof(config.WebSearchUrl) + "="))
                {
                    config.WebSearchUrl = val;
                }
            }

            var sep1 = config.DateCustomSeparator1.EscapeAllChars();
            var sep2 = config.DateCustomSeparator2.EscapeAllChars();
            var sep3 = config.DateCustomSeparator3.EscapeAllChars();

            var formatString = config.DateCustomFormat1 +
                               sep1 +
                               config.DateCustomFormat2 +
                               sep2 +
                               config.DateCustomFormat3 +
                               sep3 +
                               config.DateCustomFormat4;

            try
            {
                var temp = new DateTime(2000, 1, 1).ToString(formatString);
                config.DateCustomFormatString = formatString;
            }
            catch (FormatException)
            {
                config.DateFormat = DateFormat.CurrentCultureShort;
            }
            catch (ArgumentOutOfRangeException)
            {
                config.DateFormat = DateFormat.CurrentCultureShort;
            }
        }

        // This is faster with reflection removed.
        internal static void WriteConfigIni(ConfigData config, string fileName)
        {
            string commaCombine<T>(List<T> list)
            {
                var ret = "";
                for (var i = 0; i < list.Count; i++)
                {
                    if (i > 0) ret += ",";
                    ret += list[i].ToString();
                }

                return ret;
            }

            using (var sw = new StreamWriter(fileName, false, Encoding.UTF8))
            {
                #region Settings window

                sw.WriteLine(nameof(config.SettingsTab) + "=" + config.SettingsTab);

                #region Paths

                sw.WriteLine(nameof(config.T1Exe) + "=" + config.T1Exe.Trim());
                sw.WriteLine(nameof(config.T2Exe) + "=" + config.T2Exe.Trim());
                sw.WriteLine(nameof(config.T3Exe) + "=" + config.T3Exe.Trim());
                sw.WriteLine(nameof(config.FMsBackupPath) + "=" + config.FMsBackupPath.Trim());
                foreach (string path in config.FMArchivePaths) sw.WriteLine("FMArchivePath=" + path.Trim());
                sw.WriteLine(nameof(config.FMArchivePathsIncludeSubfolders) + "=" + config.FMArchivePathsIncludeSubfolders);

                #endregion

                sw.WriteLine(nameof(config.GameOrganization) + "=" + config.GameOrganization);

                sw.WriteLine(nameof(config.EnableArticles) + "=" + config.EnableArticles);
                sw.WriteLine(nameof(config.Articles) + "=" + commaCombine(config.Articles));
                sw.WriteLine(nameof(config.MoveArticlesToEnd) + "=" + config.MoveArticlesToEnd);

                sw.WriteLine(nameof(config.RatingDisplayStyle) + "=" + config.RatingDisplayStyle);
                sw.WriteLine(nameof(config.RatingUseStars) + "=" + config.RatingUseStars);

                sw.WriteLine(nameof(config.DateFormat) + "=" + config.DateFormat);
                sw.WriteLine(nameof(config.DateCustomFormat1) + "=" + config.DateCustomFormat1);
                sw.WriteLine(nameof(config.DateCustomSeparator1) + "=" + config.DateCustomSeparator1);
                sw.WriteLine(nameof(config.DateCustomFormat2) + "=" + config.DateCustomFormat2);
                sw.WriteLine(nameof(config.DateCustomSeparator2) + "=" + config.DateCustomSeparator2);
                sw.WriteLine(nameof(config.DateCustomFormat3) + "=" + config.DateCustomFormat3);
                sw.WriteLine(nameof(config.DateCustomSeparator3) + "=" + config.DateCustomSeparator3);
                sw.WriteLine(nameof(config.DateCustomFormat4) + "=" + config.DateCustomFormat4);

                sw.WriteLine(nameof(config.ConvertWAVsTo16BitOnInstall) + "=" + config.ConvertWAVsTo16BitOnInstall);
                sw.WriteLine(nameof(config.ConvertOGGsToWAVsOnInstall) + "=" + config.ConvertOGGsToWAVsOnInstall);
                sw.WriteLine(nameof(config.BackupSaves) + "=" + config.BackupSaves);
                sw.WriteLine(nameof(config.Language) + "=" + config.Language);
                sw.WriteLine(nameof(config.WebSearchUrl) + "=" + config.WebSearchUrl);

                #endregion

                #region Filters

                string FilterDate(DateTime? dt) => dt == null
                    ? ""
                    : new DateTimeOffset((DateTime)dt).ToUnixTimeSeconds().ToString("X");

                for (int fi = 0; fi < 4; fi++)
                {
                    var filter =
                        fi == 0 ? config.Filter :
                        fi == 1 ? config.GameTabsState.T1Filter :
                        fi == 2 ? config.GameTabsState.T2Filter :
                        config.GameTabsState.T3Filter;
                    var p = fi == 0 ? "" : fi == 1 ? "T1" : fi == 2 ? "T2" : "T3";

                    if (fi == 0) sw.WriteLine("FilterGames=" + commaCombine(config.Filter.Games));

                    sw.WriteLine(p + "FilterTitle=" + filter.Title);
                    sw.WriteLine(p + "FilterAuthor=" + filter.Author);

                    sw.WriteLine(p + "FilterReleaseDateFrom=" + FilterDate(filter.ReleaseDateFrom));
                    sw.WriteLine(p + "FilterReleaseDateTo=" + FilterDate(filter.ReleaseDateTo));

                    sw.WriteLine(p + "FilterLastPlayedFrom=" + FilterDate(filter.LastPlayedFrom));
                    sw.WriteLine(p + "FilterLastPlayedTo=" + FilterDate(filter.LastPlayedTo));

                    sw.WriteLine(p + "FilterFinishedStates=" + commaCombine(filter.Finished));

                    sw.WriteLine(p + "FilterRatingFrom=" + filter.RatingFrom);
                    sw.WriteLine(p + "FilterRatingTo=" + filter.RatingTo);

                    sw.WriteLine(p + "FilterShowJunk=" + filter.ShowJunk);

                    #region Tags

                    string TagsToString(List<CatAndTags> tagsList)
                    {
                        var intermediateTagsList = new List<string>();
                        foreach (var catAndTag in tagsList)
                        {
                            if (catAndTag.Tags.Count == 0)
                            {
                                intermediateTagsList.Add(catAndTag.Category + ":");
                            }
                            else
                            {
                                foreach (var tag in catAndTag.Tags)
                                {
                                    intermediateTagsList.Add(catAndTag.Category + ":" + tag);
                                }
                            }
                        }

                        string filterTagsString = "";
                        for (int ti = 0; ti < intermediateTagsList.Count; ti++)
                        {
                            if (ti > 0) filterTagsString += ",";
                            filterTagsString += intermediateTagsList[ti];
                        }

                        return filterTagsString;
                    }

                    sw.WriteLine(p + "FilterTagsAnd=" + TagsToString(filter.Tags.AndTags));
                    sw.WriteLine(p + "FilterTagsOr=" + TagsToString(filter.Tags.OrTags));
                    sw.WriteLine(p + "FilterTagsNot=" + TagsToString(filter.Tags.NotTags));
                }

                #endregion

                #endregion

                #region Columns

                sw.WriteLine(nameof(config.SortedColumn) + "=" + config.SortedColumn);
                sw.WriteLine(nameof(config.SortDirection) + "=" + config.SortDirection);

                foreach (var col in config.Columns)
                {
                    sw.WriteLine("Column" + col.Id + "=" + col.DisplayIndex + "," + col.Width + "," + col.Visible);
                }

                #endregion

                #region Selected FM

                sw.WriteLine("SelFMInstDir=" + config.SelFM.InstalledName);
                sw.WriteLine("SelFMIndexFromTop=" + config.SelFM.IndexFromTop);
                sw.WriteLine("T1SelFMInstDir=" + config.GameTabsState.T1SelFM.InstalledName);
                sw.WriteLine("T1SelFMIndexFromTop=" + config.GameTabsState.T1SelFM.IndexFromTop);
                sw.WriteLine("T2SelFMInstDir=" + config.GameTabsState.T2SelFM.InstalledName);
                sw.WriteLine("T2SelFMIndexFromTop=" + config.GameTabsState.T2SelFM.IndexFromTop);
                sw.WriteLine("T3SelFMInstDir=" + config.GameTabsState.T3SelFM.InstalledName);
                sw.WriteLine("T3SelFMIndexFromTop=" + config.GameTabsState.T3SelFM.IndexFromTop);

                #endregion

                #region Window state

                sw.WriteLine(nameof(config.MainWindowState) + "=" +
                             (config.MainWindowState == FormWindowState.Minimized
                                 ? FormWindowState.Maximized
                                 : config.MainWindowState));

                sw.WriteLine(nameof(config.MainWindowSize) + "=" + config.MainWindowSize.Width + "," + config.MainWindowSize.Height);

                sw.WriteLine(nameof(config.MainHorizontalSplitterDistance) + "=" + config.MainHorizontalSplitterDistance);
                sw.WriteLine(nameof(config.TopVerticalSplitterDistance) + "=" + config.TopVerticalSplitterDistance);

                sw.WriteLine(nameof(config.GameTab) + "=" + config.GameTab);
                sw.WriteLine(nameof(config.TopRightTab) + "=" + config.TopRightTab);

                sw.WriteLine(nameof(config.ReadmeZoomFactor) + "=" + config.ReadmeZoomFactor.ToString(CultureInfo.InvariantCulture));

                #endregion
            }
        }
    }
}
