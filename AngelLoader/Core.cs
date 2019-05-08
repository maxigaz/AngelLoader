﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngelLoader.Common;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;
using AngelLoader.CustomControls;
using AngelLoader.Forms;
using AngelLoader.Importing;
using FMScanner;
using Ookii.Dialogs.WinForms;
using static AngelLoader.Common.Common;
using static AngelLoader.Common.Logger;
using static AngelLoader.Common.Utility.Methods;
using static AngelLoader.Ini.Ini;

namespace AngelLoader
{
    internal interface IView : ILocalizable
    {
        int CurrentSortedColumnIndex { get; }
        SortOrder CurrentSortDirection { get; }
        void ShowFMsListZoomButtons(bool visible);
        void ShowInstallUninstallButton(bool enabled);
        Task ClearAllUIAndInternalFilters();
        void ChangeGameOrganization();
        void UpdateRatingDisplayStyle(RatingDisplayStyle style, bool startup);
        void RefreshFMsListKeepSelection();
        Task SortAndSetFilter(bool suppressRefresh = false, bool forceRefreshReadme = false,
            bool forceSuppressSelectionChangedEvent = false, bool suppressSuspendResume = false);
        void Init();
        void SortFMsDGV(Column column, SortOrder sortDirection);
        int GetRowCount();
        void SetRowCount(int count);
        void Show();
        void ShowAlert(string message, string title);
        object InvokeSync(Delegate method);
        object InvokeSync(Delegate method, params object[] args);
        object InvokeAsync(Delegate method);
        object InvokeAsync(Delegate method, params object[] args);
        void Block(bool block);
        Task RefreshFMsList(bool refreshReadme, bool suppressSelectionChangedEvent = false,
            bool suppressSuspendResume = false);
        Task RefreshSelectedFM(bool refreshReadme, bool refreshGridRowOnly = false);
        bool AskToContinue(string message, string title, bool noIcon = false);

        (bool Cancel, bool DontAskAgain)
        AskToContinueYesNoCustomStrings(string message, string title, TaskDialogIcon? icon,
            bool showDontAskAgain, string yes, string no);

        (bool Cancel, bool Continue, bool DontAskAgain)
        AskToContinueWithCancelCustomStrings(string message, string title, TaskDialogIcon? icon,
            bool showDontAskAgain, string yes, string no, string cancel);
    }

    internal static class Core
    {
        internal static IView View { get; set; }
        internal static ProgressPanel ProgressBox;

        internal static List<FanMission> FMsViewList = new List<FanMission>();
        private static readonly List<FanMission> FMDataIniList = new List<FanMission>();

        private static CancellationTokenSource ScanCts;

        internal static async Task Init()
        {
            View = new MainForm();

            try
            {
                Directory.CreateDirectory(Paths.Data);
                Directory.CreateDirectory(Paths.Languages);
            }
            catch (Exception ex)
            {
                const string message = "Failed to create required application directories on startup.";
                Log(message, ex);
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }

            bool openSettings;
            if (File.Exists(Paths.ConfigIni))
            {
                try
                {
                    ReadConfigIni(Paths.ConfigIni, Config);
                    var checkPaths = CheckPaths();
                    openSettings = checkPaths == Error.BackupPathNotSpecified;
                }
                catch (Exception ex)
                {
                    var message = Paths.ConfigIni + " exists but there was an error while reading it.";
                    Log(message, ex);
                    openSettings = true;
                }
            }
            else
            {
                openSettings = true;
            }

            // Have to read langs here because which language to use will be stored in the config file.
            // Gather all lang files in preparation to read their LanguageName= value so we can get the lang's
            // name in its own language
            var langFiles = Directory.GetFiles(Paths.Languages, "*.ini", SearchOption.TopDirectoryOnly);
            bool selFound = false;

            // Do it ONCE here, not every loop!
            Config.LanguageNames.Clear();

            for (int i = 0; i < langFiles.Length; i++)
            {
                var f = langFiles[i];
                var fn = f.GetFileNameFast().RemoveExtension();
                if (!selFound && fn.EqualsI(Config.Language))
                {
                    try
                    {
                        ReadLocalizationIni(f);
                        selFound = true;
                    }
                    catch (Exception ex)
                    {
                        Log("There was an error while reading " + f + ".", ex);
                    }
                }
                ReadTranslatedLanguageName(f);

                // These need to be set after language read. Slightly awkward but oh well.
                SetDefaultConfigVarNamesToLocalized();
            }

            if (openSettings)
            {
                if (await OpenSettings(startup: true))
                {
                    var checkPaths = CheckPaths();

                    Debug.Assert(checkPaths == Error.None, "checkPaths returned an error the second time");

                    WriteConfigIni(Config, Paths.ConfigIni);
                }
                else
                {
                    // Since nothing of consequence has yet happened, it's okay to do the brutal quit
                    Environment.Exit(0);
                }
            }

            FindFMs.Find(FMDataIniList, startup: true);
            View.Init();
            View.Show();
        }

        public static async Task<bool> OpenSettings(bool startup = false)
        {
            using (var sf = new SettingsForm(View, Config, startup))
            {
                // This needs to be separate so the below line can work
                var result = sf.ShowDialog();

                // Special case: this is meta, so it should always be set even if the user clicked Cancel
                Config.SettingsTab = sf.OutConfig.SettingsTab;

                if (result != DialogResult.OK) return false;

                #region Set changed bools

                bool archivePathsChanged =
                    !startup &&
                    (!Config.FMArchivePaths.SequenceEqual(sf.OutConfig.FMArchivePaths, StringComparer.OrdinalIgnoreCase) ||
                    Config.FMArchivePathsIncludeSubfolders != sf.OutConfig.FMArchivePathsIncludeSubfolders);

                bool gamePathsChanged =
                    !startup &&
                    (!Config.T1Exe.EqualsI(sf.OutConfig.T1Exe) ||
                    !Config.T2Exe.EqualsI(sf.OutConfig.T2Exe) ||
                    !Config.T3Exe.EqualsI(sf.OutConfig.T3Exe));

                bool gameOrganizationChanged =
                    !startup && (Config.GameOrganization != sf.OutConfig.GameOrganization);

                bool articlesChanged =
                    !startup &&
                    (Config.EnableArticles != sf.OutConfig.EnableArticles ||
                    !Config.Articles.SequenceEqual(sf.OutConfig.Articles, StringComparer.InvariantCultureIgnoreCase) ||
                    Config.MoveArticlesToEnd != sf.OutConfig.MoveArticlesToEnd);

                bool dateFormatChanged =
                    !startup &&
                    (Config.DateFormat != sf.OutConfig.DateFormat ||
                    Config.DateCustomFormatString != sf.OutConfig.DateCustomFormatString);

                bool ratingDisplayStyleChanged =
                    !startup &&
                    (Config.RatingDisplayStyle != sf.OutConfig.RatingDisplayStyle ||
                    Config.RatingUseStars != sf.OutConfig.RatingUseStars);

                bool languageChanged =
                    !startup && !Config.Language.EqualsI(sf.OutConfig.Language);

                #endregion

                #region Set config data

                // Set values individually (rather than deep-copying) so that non-Settings values don't get
                // overwritten.

                #region Paths tab

                #region Game exes

                Config.T1Exe = sf.OutConfig.T1Exe;
                Config.T2Exe = sf.OutConfig.T2Exe;
                Config.T3Exe = sf.OutConfig.T3Exe;

                // TODO: These should probably go in the Settings form along with the cam_mod.ini check
                // Note: SettingsForm is supposed to check these for validity, so we shouldn't have any exceptions
                //       being thrown here.
                Config.T1FMInstallPath = !Config.T1Exe.IsWhiteSpace()
                    ? GetInstFMsPathFromCamModIni(Path.GetDirectoryName(Config.T1Exe), out Error _)
                    : "";
                Config.T1DromEdDetected = !GetDromEdExe(Game.Thief1).IsEmpty();

                Config.T2FMInstallPath = !Config.T2Exe.IsWhiteSpace()
                    ? GetInstFMsPathFromCamModIni(Path.GetDirectoryName(Config.T2Exe), out Error _)
                    : "";
                Config.T2DromEdDetected = !GetDromEdExe(Game.Thief2).IsEmpty();

                if (!Config.T3Exe.IsWhiteSpace())
                {
                    var (error, useCentralSaves, t3FMInstPath) = GetInstFMsPathFromT3();
                    if (error == Error.None)
                    {
                        Config.T3FMInstallPath = t3FMInstPath;
                        Config.T3UseCentralSaves = useCentralSaves;
                    }
                }
                else
                {
                    Config.T3FMInstallPath = "";
                }

                #endregion

                Config.FMsBackupPath = sf.OutConfig.FMsBackupPath;

                Config.FMArchivePaths.ClearAndAdd(sf.OutConfig.FMArchivePaths);

                Config.FMArchivePathsIncludeSubfolders = sf.OutConfig.FMArchivePathsIncludeSubfolders;

                #endregion

                if (startup)
                {
                    Config.Language = sf.OutConfig.Language;
                    return true;
                }

                // From this point on, we're not in startup mode.

                // For clarity, don't copy the other tabs' data on startup, because their tabs won't be shown and
                // so they won't have been changed

                #region FM Display tab

                Config.GameOrganization = sf.OutConfig.GameOrganization;

                Config.EnableArticles = sf.OutConfig.EnableArticles;
                Config.Articles.ClearAndAdd(sf.OutConfig.Articles);

                Config.MoveArticlesToEnd = sf.OutConfig.MoveArticlesToEnd;

                Config.RatingDisplayStyle = sf.OutConfig.RatingDisplayStyle;
                Config.RatingUseStars = sf.OutConfig.RatingUseStars;

                Config.DateFormat = sf.OutConfig.DateFormat;
                Config.DateCustomFormat1 = sf.OutConfig.DateCustomFormat1;
                Config.DateCustomSeparator1 = sf.OutConfig.DateCustomSeparator1;
                Config.DateCustomFormat2 = sf.OutConfig.DateCustomFormat2;
                Config.DateCustomSeparator2 = sf.OutConfig.DateCustomSeparator2;
                Config.DateCustomFormat3 = sf.OutConfig.DateCustomFormat3;
                Config.DateCustomSeparator3 = sf.OutConfig.DateCustomSeparator3;
                Config.DateCustomFormat4 = sf.OutConfig.DateCustomFormat4;
                Config.DateCustomFormatString = sf.OutConfig.DateCustomFormatString;

                #endregion

                #region Other tab

                Config.ConvertWAVsTo16BitOnInstall = sf.OutConfig.ConvertWAVsTo16BitOnInstall;
                Config.ConvertOGGsToWAVsOnInstall = sf.OutConfig.ConvertOGGsToWAVsOnInstall;

                Config.ConfirmUninstall = sf.OutConfig.ConfirmUninstall;

                Config.BackupFMData = sf.OutConfig.BackupFMData;
                Config.BackupAlwaysAsk = sf.OutConfig.BackupAlwaysAsk;

                Config.Language = sf.OutConfig.Language;

                Config.WebSearchUrl = sf.OutConfig.WebSearchUrl;

                Config.ConfirmPlayOnDCOrEnter = sf.OutConfig.ConfirmPlayOnDCOrEnter;

                Config.HideUninstallButton = sf.OutConfig.HideUninstallButton;
                Config.HideFMListZoomButtons = sf.OutConfig.HideFMListZoomButtons;

                #endregion

                // These ones MUST NOT be set on startup, because the source values won't be valid
                Config.SortedColumn = (Column)View.CurrentSortedColumnIndex;
                Config.SortDirection = View.CurrentSortDirection;

                #endregion

                #region Change-specific actions (pre-refresh)

                View.ShowInstallUninstallButton(!Config.HideUninstallButton);
                View.ShowFMsListZoomButtons(!Config.HideFMListZoomButtons);

                if (archivePathsChanged || gamePathsChanged)
                {
                    FindFMs.Find(FMDataIniList);
                }
                if (gameOrganizationChanged)
                {
                    // Clear everything to defaults so we don't have any leftover state screwing things all up
                    Config.ClearAllSelectedFMs();
                    Config.ClearAllFilters();
                    Config.GameTab = Game.Thief1;
                    await View.ClearAllUIAndInternalFilters();
                    if (Config.GameOrganization == GameOrganization.ByTab) Config.Filter.Games.Add(Game.Thief1);
                    View.ChangeGameOrganization();
                }
                if (ratingDisplayStyleChanged)
                {
                    View.UpdateRatingDisplayStyle(Config.RatingDisplayStyle, startup: false);
                }
                if ((archivePathsChanged || gamePathsChanged) && languageChanged)
                {
                    // Do this again if the FMs list might have changed
                    SetFMSizesToLocalized();
                }

                #endregion

                #region Call appropriate refresh method (if applicable)

                // Game paths should have been checked and verified before OK was clicked, so assume they're good
                // here
                if (archivePathsChanged || gamePathsChanged || gameOrganizationChanged || articlesChanged)
                {
                    if (archivePathsChanged || gamePathsChanged)
                    {
                        if (ViewListGamesNull.Count > 0) await ScanNewFMsForGameType(useViewListGamesNull: true);
                    }

                    await View.SortAndSetFilter(forceRefreshReadme: true, forceSuppressSelectionChangedEvent: true);
                }
                else if (dateFormatChanged || languageChanged)
                {
                    View.RefreshFMsListKeepSelection();
                }

                #endregion
            }

            return true;
        }

        internal static void SetDefaultConfigVarNamesToLocalized()
        {
            //Defaults.CV_ForceFullScreen.Name = LText.ConfigVars.ForceFullScreen;
            //Defaults.CV_ForceWindowed.Name = LText.ConfigVars.ForceWindowed;
            //Defaults.CV_ForceNewMantle.Name = LText.ConfigVars.ForceNewMantle;
            //Defaults.CV_ForceOldMantle.Name = LText.ConfigVars.ForceOldMantle;
        }

        internal static void SortFMsViewList(Column column, SortOrder sortDirection)
        {
            var articles = Config.EnableArticles ? Config.Articles : new List<string>();

            void SortByTitle(bool reverse = false)
            {
                var ascending = reverse ? SortOrder.Descending : SortOrder.Ascending;

                FMsViewList = sortDirection == ascending
                    ? FMsViewList.OrderBy(x => x.Title, new FMTitleComparer(articles)).ToList()
                    : FMsViewList.OrderByDescending(x => x.Title, new FMTitleComparer(articles)).ToList();
            }

            // For any column which could have empty entries, sort by title first in order to maintain a
            // consistent order

            switch (column)
            {
                case Column.Game:
                    SortByTitle();
                    FMsViewList = sortDirection == SortOrder.Ascending
                        ? FMsViewList.OrderBy(x => x.Game).ToList()
                        : FMsViewList.OrderByDescending(x => x.Game).ToList();
                    break;

                case Column.Installed:
                    SortByTitle();
                    // Reverse this because "Installed" should go on top and blanks should go on bottom
                    FMsViewList = sortDirection == SortOrder.Descending
                        ? FMsViewList.OrderBy(x => x.Installed).ToList()
                        : FMsViewList.OrderByDescending(x => x.Installed).ToList();
                    break;

                case Column.Title:
                    SortByTitle();
                    break;

                case Column.Archive:
                    FMsViewList = sortDirection == SortOrder.Ascending
                        ? FMsViewList.OrderBy(x => x.Archive).ToList()
                        : FMsViewList.OrderByDescending(x => x.Archive).ToList();
                    break;

                case Column.Author:
                    SortByTitle();
                    FMsViewList = sortDirection == SortOrder.Ascending
                        ? FMsViewList.OrderBy(x => x.Author).ToList()
                        : FMsViewList.OrderByDescending(x => x.Author).ToList();
                    break;

                case Column.Size:
                    SortByTitle();
                    FMsViewList = sortDirection == SortOrder.Ascending
                        ? FMsViewList.OrderBy(x => x.SizeBytes).ToList()
                        : FMsViewList.OrderByDescending(x => x.SizeBytes).ToList();
                    break;

                case Column.Rating:
                    SortByTitle();
                    FMsViewList = sortDirection == SortOrder.Ascending
                        ? FMsViewList.OrderBy(x => x.Rating).ToList()
                        : FMsViewList.OrderByDescending(x => x.Rating).ToList();
                    break;

                case Column.Finished:
                    SortByTitle();
                    // FinishedOnUnknown is a separate value, so...
                    if (sortDirection == SortOrder.Ascending)
                    {
                        FMsViewList = FMsViewList.OrderBy(x => x.FinishedOn).ToList();
                        FMsViewList = FMsViewList.OrderBy(x => x.FinishedOnUnknown).ToList();
                    }
                    else
                    {
                        FMsViewList = FMsViewList.OrderByDescending(x => x.FinishedOn).ToList();
                        FMsViewList = FMsViewList.OrderByDescending(x => x.FinishedOnUnknown).ToList();
                    }
                    break;

                case Column.ReleaseDate:
                    SortByTitle();
                    // Sort this one down to the day only, because the exact time may very well not be known, and
                    // even if it is, it's not visible or editable anywhere and it'd be weird to have missions
                    // sorted out of name order because of an invisible time difference.
                    FMsViewList = sortDirection == SortOrder.Ascending
                        ? FMsViewList.OrderBy(x => x.ReleaseDate?.Date ?? x.ReleaseDate).ToList()
                        : FMsViewList.OrderByDescending(x => x.ReleaseDate?.Date ?? x.ReleaseDate).ToList();
                    break;

                case Column.LastPlayed:
                    SortByTitle();
                    // Sort this one by exact DateTime because the time is (indirectly) changeable down to the
                    // second (you change it by playing it), and the user will expect precise sorting.
                    FMsViewList = sortDirection == SortOrder.Ascending
                        ? FMsViewList.OrderBy(x => x.LastPlayed).ToList()
                        : FMsViewList.OrderByDescending(x => x.LastPlayed).ToList();
                    break;

                case Column.DisabledMods:
                    SortByTitle();
                    FMsViewList = sortDirection == SortOrder.Ascending
                        ? FMsViewList.OrderBy(x => x.DisabledMods).ToList()
                        : FMsViewList.OrderByDescending(x => x.DisabledMods).ToList();
                    break;

                case Column.Comment:
                    SortByTitle();
                    FMsViewList = sortDirection == SortOrder.Ascending
                        ? FMsViewList.OrderBy(x => x.CommentSingleLine).ToList()
                        : FMsViewList.OrderByDescending(x => x.CommentSingleLine).ToList();
                    break;
            }
        }

        private static Error CheckPaths()
        {
            var t1Exists = !Config.T1Exe.IsEmpty() && File.Exists(Config.T1Exe);
            var t2Exists = !Config.T2Exe.IsEmpty() && File.Exists(Config.T2Exe);
            var t3Exists = !Config.T3Exe.IsEmpty() && File.Exists(Config.T3Exe);

            if (t1Exists)
            {
                var gamePath = Path.GetDirectoryName(Config.T1Exe);
                var gameFMsPath = GetInstFMsPathFromCamModIni(gamePath, out Error error);
                Config.T1DromEdDetected = !GetDromEdExe(Game.Thief1).IsEmpty();
                if (error == Error.CamModIniNotFound) return Error.T1CamModIniNotFound;
                Config.T1FMInstallPath = gameFMsPath;
            }
            if (t2Exists)
            {
                var gamePath = Path.GetDirectoryName(Config.T2Exe);
                var gameFMsPath = GetInstFMsPathFromCamModIni(gamePath, out Error error);
                Config.T2DromEdDetected = !GetDromEdExe(Game.Thief2).IsEmpty();
                if (error == Error.CamModIniNotFound) return Error.T2CamModIniNotFound;
                Config.T2FMInstallPath = gameFMsPath;
            }
            if (t3Exists)
            {
                var (error, useCentralSaves, path) = GetInstFMsPathFromT3();
                if (error != Error.None) return error;
                Config.T3FMInstallPath = path;
                Config.T3UseCentralSaves = useCentralSaves;
            }

            if (!t1Exists && !t2Exists && !t3Exists) return Error.NoGamesSpecified;

            if (!Directory.Exists(Config.FMsBackupPath))
            {
                return Error.BackupPathNotSpecified;
            }

            return Error.None;
        }

        internal static string GetInstFMsPathFromCamModIni(string gamePath, out Error error)
        {
            string CreateAndReturn(string fmsPath)
            {
                try
                {
                    Directory.CreateDirectory(fmsPath);
                }
                catch (Exception ex)
                {
                    Log("Exception creating FM installed base dir", ex);
                }

                return fmsPath;
            }

            var camModIni = Path.Combine(gamePath, "cam_mod.ini");

            if (!File.Exists(camModIni))
            {
                //error = Error.CamModIniNotFound;
                //return null;
                error = Error.None;
                return CreateAndReturn(Path.Combine(gamePath, "FMs"));
            }

            string path = null;

            using (var sr = new StreamReader(camModIni))
            {
                /*
                 Conforms to the way NewDark reads it:
                 - Zero or more whitespace characters allowed at the start of the line (before the key)
                 - The key-value separator is one or more whitespace characters
                 - Keys are case-insensitive
                 - If duplicate keys exist, later ones replace earlier ones
                 - Comment lines start with ;
                 - No section headers
                */
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.IsEmpty()) continue;

                    line = line.TrimStart();

                    if (line.IsEmpty() || line[0] == ';') continue;

                    if (line.StartsWithI(@"fm_path") && line.Length > 7 && char.IsWhiteSpace(line[7]))
                    {
                        path = line.Substring(7).Trim();
                    }
                }
            }

            if (!path.IsEmpty() &&
                (path.StartsWithFast_NoNullChecks(".\\") || path.StartsWithFast_NoNullChecks("..\\") ||
                path.StartsWithFast_NoNullChecks("./") || path.StartsWithFast_NoNullChecks("../")))
            {
                try
                {
                    path = Paths.RelativeToAbsolute(gamePath, path);
                }
                catch (Exception)
                {
                    error = Error.None;
                    return CreateAndReturn(Path.Combine(gamePath, "FMs"));
                }
            }

            error = Error.None;
            return Directory.Exists(path) ? path : CreateAndReturn(Path.Combine(gamePath, "FMs"));
        }

        internal static (Error Error, bool UseCentralSaves, string Path)
        GetInstFMsPathFromT3()
        {
            var soIni = Paths.GetSneakyOptionsIni();
            var errorMessage = LText.AlertMessages.Misc_SneakyOptionsIniNotFound;
            if (soIni.IsEmpty())
            {
                MessageBox.Show(errorMessage, LText.AlertMessages.Alert, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return (Error.SneakyOptionsNoRegKey, false, null);
            }

            if (!File.Exists(soIni))
            {
                MessageBox.Show(errorMessage, LText.AlertMessages.Alert, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return (Error.SneakyOptionsNotFound, false, null);
            }

            bool ignoreSavesKeyFound = false;
            bool ignoreSavesKey = true;

            bool fmInstPathFound = false;
            string fmInstPath = "";

            var lines = File.ReadAllLines(soIni);
            for (var i = 0; i < lines.Length; i++)
            {
                var lineT = lines[i].Trim();
                if (lineT.EqualsI("[Loader]"))
                {
                    /*
                     Conforms to the way Sneaky Upgrade reads it:
                     - Whitespace allowed on both sides of section headers (but not within brackets)
                     - Section headers and keys are case-insensitive
                     - Key-value separator is '='
                     - Whitespace allowed on left side of key (but not right side before '=')
                     - Case-insensitive "true" is true, anything else is false
                     - If duplicate keys exist, the earliest one is used
                    */
                    while (i < lines.Length - 1)
                    {
                        var lt = lines[i + 1].Trim();
                        if (!ignoreSavesKeyFound &&
                            !lt.IsEmpty() && lt[0] != '[' && lt.StartsWithI("IgnoreSavesKey="))
                        {
                            ignoreSavesKey = lt.Substring(lt.IndexOf('=') + 1).EqualsTrue();
                            ignoreSavesKeyFound = true;
                        }
                        else if (!fmInstPathFound &&
                                 !lt.IsEmpty() && lt[0] != '[' && lt.StartsWithI("InstallPath="))
                        {
                            fmInstPath = lt.Substring(lt.IndexOf('=') + 1).Trim();
                            fmInstPathFound = true;
                        }
                        else if (!lt.IsEmpty() && lt[0] == '[' && lt[lt.Length - 1] == ']')
                        {
                            break;
                        }

                        if (ignoreSavesKeyFound && fmInstPathFound) break;

                        i++;
                    }
                    break;
                }
            }

            return fmInstPathFound
                ? (Error.None, !ignoreSavesKey, fmInstPath)
                : (Error.T3FMInstPathNotFound, false, null);
        }

        #region Scan

        // Super quick-n-cheap hack for perf
        internal static List<int> ViewListGamesNull = new List<int>();

        internal static async Task<bool> ScanFM(FanMission fm, ScanOptions scanOptions)
        {
            return await ScanFMs(new List<FanMission> { fm }, scanOptions);
        }

        internal static async Task<bool> ScanFMs(List<FanMission> fmsToScan, ScanOptions scanOptions, bool markAsScanned = true)
        {
            if (fmsToScan == null || fmsToScan.Count == 0 || (fmsToScan.Count == 1 && fmsToScan[0] == null))
            {
                return false;
            }

            // Removed from general use, but just in case I want to add the option back...
            const bool overwriteUnscannedFields = false;

            void ReportProgress(ProgressReport pr)
            {
                var fmIsZip = pr.FMName.ExtIsArchive();
                var name = fmIsZip ? pr.FMName.GetFileNameFast() : pr.FMName.GetDirNameFast();
                ProgressBox.ReportScanProgress(pr.FMNumber, pr.FMsTotal, pr.Percent, name);
            }

            var scanningOne = fmsToScan.Count == 1;

            if (scanningOne)
            {
                Log(nameof(ScanFMs) + ": Scanning one", methodName: false);
                // Just use a cheap check and throw up the progress box for .7z files, otherwise not. Not as nice
                // as the timer method, but that can cause race conditions I don't know how to fix, so whatever.
                if (fmsToScan[0].Archive.ExtIs7z())
                {
                    ProgressBox.ShowScanningAllFMs();
                }
                else
                {
                    // Block user input to the form to mimic the UI thread being blocked, because we're async here
                    //View.BeginInvoke(new Action(View.Block));
                    View.Block(true);
                    ProgressBox.ProgressTask = ProgressPanel.ProgressTasks.ScanAllFMs;
                    ProgressBox.ShowProgressWindow(ProgressBox.ProgressTask, suppressShow: true);
                }
            }
            else
            {
                ProgressBox.ShowScanningAllFMs();
            }

            // TODO: This is pretty hairy, try and organize this better
            try
            {
                ScanCts = new CancellationTokenSource();

                var fms = new List<string>();

                Log(nameof(ScanFMs) + ": about to call " + nameof(GetFMArchivePaths) + " with subfolders=" +
                    Config.FMArchivePathsIncludeSubfolders);

                // Get archive paths list only once and cache it - in case of "include subfolders" being true,
                // cause then it will hit the actual disk rather than just going through a list of paths in
                // memory
                var archivePaths = await Task.Run(GetFMArchivePaths);

                // Safety net to guarantee that the in and out lists will have the same count and order
                var fmsToScanFiltered = new List<FanMission>();

                for (var i = 0; i < fmsToScan.Count; i++)
                {
                    var fm = fmsToScan[i];
                    var fmArchivePath = await Task.Run(() => FindFMArchive(fm, archivePaths));
                    if (!fm.Archive.IsEmpty() && !fmArchivePath.IsEmpty())
                    {
                        fmsToScanFiltered.Add(fm);
                        fms.Add(fmArchivePath);
                    }
                    else if (GameIsKnownAndSupported(fm))
                    {
                        var fmInstalledPath = GetFMInstallsBasePath(fm.Game);
                        if (!fmInstalledPath.IsEmpty())
                        {
                            fmsToScanFiltered.Add(fm);
                            fms.Add(Path.Combine(fmInstalledPath, fm.InstalledDir));
                        }
                    }

                    if (ScanCts.IsCancellationRequested)
                    {
                        ScanCts?.Dispose();
                        return false;
                    }
                }

                List<ScannedFMData> fmDataList;
                try
                {
                    var progress = new Progress<ProgressReport>(ReportProgress);

                    Paths.PrepareTempPath(Paths.FMScannerTemp);

                    using (var scanner = new Scanner())
                    {
                        scanner.LogFile = Paths.ScannerLogFile;
                        scanner.ZipEntryNameEncoding = Encoding.UTF8;

                        fmDataList = await scanner.ScanAsync(fms, Paths.FMScannerTemp, scanOptions, progress, ScanCts.Token);
                    }
                }
                catch (OperationCanceledException)
                {
                    return false;
                }
                finally
                {
                    ScanCts?.Dispose();
                }

                for (var i = 0; i < fmsToScanFiltered.Count; i++)
                {
                    var scannedFM = fmDataList[i];

                    if (scannedFM == null)
                    {
                        // We need to return fail for scanning one, else we get into an infinite loop because
                        // of a refresh that gets called in that case
                        if (scanningOne)
                        {
                            Log(nameof(ScanFMs) + " (one) scanned FM was null. FM was:\r\n" +
                                "Archive: " + fmsToScanFiltered[0].Archive + "\r\n" +
                                "InstalledDir: " + fmsToScanFiltered[0].InstalledDir,
                                methodName: false);
                            return false;
                        }
                        continue;
                    }

                    var sel = fmsToScanFiltered[i];
                    if (sel == null)
                    {
                        // Same as above (this should never happen now, but hey)
                        if (scanningOne) return false;
                        continue;
                    }

                    var gameSup = scannedFM.Game != Games.Unsupported;

                    if (overwriteUnscannedFields || scanOptions.ScanTitle)
                    {
                        sel.Title =
                            !scannedFM.Title.IsEmpty() ? scannedFM.Title
                            : scannedFM.ArchiveName.ExtIsArchive() ? scannedFM.ArchiveName.RemoveExtension()
                            : scannedFM.ArchiveName;

                        if (gameSup)
                        {
                            sel.AltTitles.ClearAndAdd(sel.Title);
                            sel.AltTitles.AddRange(scannedFM.AlternateTitles);
                        }
                        else
                        {
                            sel.AltTitles.Clear();
                        }
                    }

                    if (overwriteUnscannedFields || scanOptions.ScanSize)
                    {
                        sel.SizeString = gameSup ? scannedFM.Size.ConvertSize() : "";
                        sel.SizeBytes = (ulong)(gameSup ? scannedFM.Size ?? 0 : 0);
                    }
                    if (overwriteUnscannedFields || scanOptions.ScanReleaseDate)
                    {
                        sel.ReleaseDate = gameSup ? scannedFM.LastUpdateDate : null;
                    }
                    if (overwriteUnscannedFields || scanOptions.ScanCustomResources)
                    {
                        sel.HasMap = gameSup ? scannedFM.HasMap : null;
                        sel.HasAutomap = gameSup ? scannedFM.HasAutomap : null;
                        sel.HasScripts = gameSup ? scannedFM.HasCustomScripts : null;
                        sel.HasTextures = gameSup ? scannedFM.HasCustomTextures : null;
                        sel.HasSounds = gameSup ? scannedFM.HasCustomSounds : null;
                        sel.HasObjects = gameSup ? scannedFM.HasCustomObjects : null;
                        sel.HasCreatures = gameSup ? scannedFM.HasCustomCreatures : null;
                        sel.HasMotions = gameSup ? scannedFM.HasCustomMotions : null;
                        sel.HasMovies = gameSup ? scannedFM.HasMovies : null;
                        sel.HasSubtitles = gameSup ? scannedFM.HasCustomSubtitles : null;
                    }

                    if (overwriteUnscannedFields || scanOptions.ScanAuthor)
                    {
                        sel.Author = gameSup ? scannedFM.Author : "";
                    }

                    if (overwriteUnscannedFields || scanOptions.ScanGameType)
                    {
                        sel.Game =
                            scannedFM.Game == Games.Unsupported ? Game.Unsupported :
                            scannedFM.Game == Games.TDP ? Game.Thief1 :
                            scannedFM.Game == Games.TMA ? Game.Thief2 :
                            scannedFM.Game == Games.TDS ? Game.Thief3 :
                            (Game?)null;
                    }

                    if (overwriteUnscannedFields || scanOptions.ScanLanguages)
                    {
                        sel.Languages = gameSup ? scannedFM.Languages : new string[0];
                        sel.LanguagesString = gameSup
                            ? scannedFM.Languages != null ? string.Join(", ", scannedFM.Languages) : ""
                            : "";
                    }

                    if (overwriteUnscannedFields || scanOptions.ScanTags)
                    {
                        sel.TagsString = gameSup ? scannedFM.TagsString : "";

                        // Don't clear the tags, because the user could have added a bunch and we should only
                        // add to those, not overwrite them
                        if (gameSup) AddTagsToFMAndGlobalList(sel.TagsString, sel.Tags);
                    }

                    sel.MarkedScanned = markAsScanned;
                }

                WriteFullFMDataIni();
            }
            catch (Exception ex)
            {
                Log("Exception in ScanFMs", ex);
                var message = scanningOne
                    ? LText.AlertMessages.Scan_ExceptionInScanOne
                    : LText.AlertMessages.Scan_ExceptionInScanMultiple;
                View.ShowAlert(message, LText.AlertMessages.Error);
                return false;
            }
            finally
            {
                View.Block(false);
                ProgressBox.HideThis();
            }

            return true;
        }

        internal static void CancelScan()
        {
            try
            {
                ScanCts?.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        internal static async Task ScanNewFMsForGameType(bool useViewListGamesNull = false)
        {
            var fmsToScan = new List<FanMission>();

            restart:

            if (useViewListGamesNull)
            {
                try
                {
                    // NOTE: We use FMDataIniList index because that's the list that the indexes are pulled from!
                    foreach (var index in ViewListGamesNull) fmsToScan.Add(FMDataIniList[index]);
                }
                catch
                {
                    // Cheap fallback in case something goes wrong, because what we're doing is a little iffy
                    useViewListGamesNull = false;
                    goto restart;
                }
                finally
                {
                    // Critical that this gets cleared immediately after use!
                    ViewListGamesNull.Clear();
                }
            }
            else
            {
                foreach (var fm in FMsViewList)
                {
                    if (fm.Game == null) fmsToScan.Add(fm);
                }
            }

            if (fmsToScan.Count > 0)
            {
                var scanOptions = ScanOptions.FalseDefault(scanGameType: true);

                try
                {
                    await ScanFMs(fmsToScan, scanOptions, markAsScanned: false);
                }
                catch (Exception ex)
                {
                    Log("Exception in ScanFMs", ex);
                }
            }
        }

        #endregion

        #region Importing

        internal static async Task<bool>
        ImportFromDarkLoader(string iniFile, bool importFMData, bool importSaves)
        {
            ProgressBox.ShowImportDarkLoader();
            try
            {
                var (error, fmsToScan) = await ImportDarkLoader.Import(iniFile, importFMData, importSaves, FMDataIniList);
                if (error != ImportError.None)
                {
                    Log("Import.Error: " + error, stackTrace: true);

                    if (error == ImportError.NoArchiveDirsFound)
                    {
                        View.ShowAlert(LText.Importing.DarkLoader_NoArchiveDirsFound, LText.AlertMessages.Alert);
                        return false;
                    }

                    return false;
                }

                await ScanAndFind(fmsToScan,
                    ScanOptions.FalseDefault(scanGameType: true, scanCustomResources: true));
            }
            catch (Exception ex)
            {
                Log("Exception in DarkLoader import", ex);
                return false;
            }
            finally
            {
                ProgressBox.HideThis();
            }

            return true;
        }

        internal static async Task<bool> ImportFromNDL(string iniFile)
        {
            ProgressBox.ShowImportNDL();
            try
            {
                var (error, fmsToScan) = await ImportNDL.Import(iniFile, FMDataIniList);
                if (error != ImportError.None)
                {
                    Log("Import error: " + error, stackTrace: true);
                    return false;
                }

                await ScanAndFind(fmsToScan,
                    ScanOptions.FalseDefault(scanGameType: true, scanCustomResources: true));
            }
            catch (Exception ex)
            {
                Log("Exception in NewDarkLoader import", ex);
                return false;
            }
            finally
            {
                ProgressBox.HideThis();
            }

            return true;
        }

        internal static async Task<bool> ImportFromFMSel(string iniFile)
        {
            ProgressBox.ShowImportFMSel();
            try
            {
                var (error, fmsToScan) = await ImportFMSel.Import(iniFile, FMDataIniList);
                if (error != ImportError.None)
                {
                    Log("Import error: " + error, stackTrace: true);
                    return false;
                }

                await ScanAndFind(fmsToScan,
                    ScanOptions.FalseDefault(scanGameType: true, scanCustomResources: true, scanSize: true));
            }
            catch (Exception ex)
            {
                Log("Exception in FMSel import", ex);
                return false;
            }
            finally
            {
                ProgressBox.HideThis();
            }

            return true;
        }

        private static async Task ScanAndFind(List<FanMission> fms, ScanOptions scanOptions)
        {
            if (fms.Count == 0) return;

            await ScanFMs(fms, scanOptions);
            FindFMs.Find(FMDataIniList);
        }

        #endregion

        internal static async Task RefreshFromDisk()
        {
            FindFMs.Find(FMDataIniList);
            // This await call takes 15ms just to make the call alone(?!) so don't do it unless we have to
            if (ViewListGamesNull.Count > 0) await ScanNewFMsForGameType(useViewListGamesNull: true);
            await View.SortAndSetFilter();
        }

        #region Audio conversion (mainly for pre-checks)

        internal static async Task ConvertOGGsToWAVs(FanMission fm)
        {
            if (!fm.Installed || !GameIsDark(fm)) return;

            Debug.Assert(fm.Game != null, "fm.Game != null");

            var gameExe = GetGameExeFromGameType((Game)fm.Game);
            var gameName = GetGameNameFromGameType((Game)fm.Game);
            if (GameIsRunning(gameExe))
            {
                View.ShowAlert(
                    gameName + ":\r\n" + LText.AlertMessages.FileConversion_GameIsRunning,
                    LText.AlertMessages.Alert);
                return;
            }

            if (!FMIsReallyInstalled(fm))
            {
                var yes = View.AskToContinue(LText.AlertMessages.Misc_FMMarkedInstalledButNotInstalled,
                    LText.AlertMessages.Alert);
                if (yes)
                {
                    fm.Installed = false;
                    await View.RefreshSelectedFM(refreshReadme: false);
                }
                return;
            }

            Debug.Assert(fm.Installed, "fm is not installed");

            Debug.Assert(!fm.InstalledDir.IsEmpty(), "fm.InstalledFolderName is null or empty");

            var ac = new AudioConverter(fm, GetFMInstallsBasePath(fm.Game));
            try
            {
                ProgressBox.ShowConvertingFiles();
                await ac.ConvertOGGsToWAVs();
            }
            finally
            {
                ProgressBox.HideThis();
            }
        }

        internal static async Task ConvertWAVsTo16Bit(FanMission fm)
        {
            if (!fm.Installed || !GameIsDark(fm)) return;

            Debug.Assert(fm.Game != null, "fm.Game != null");

            var gameExe = GetGameExeFromGameType((Game)fm.Game);
            var gameName = GetGameNameFromGameType((Game)fm.Game);
            if (GameIsRunning(gameExe))
            {
                View.ShowAlert(gameName + ":\r\n" + LText.AlertMessages.FileConversion_GameIsRunning,
                    LText.AlertMessages.Alert);
                return;
            }

            if (!FMIsReallyInstalled(fm))
            {
                var yes = View.AskToContinue(LText.AlertMessages.Misc_FMMarkedInstalledButNotInstalled,
                    LText.AlertMessages.Alert);
                if (yes)
                {
                    fm.Installed = false;
                    await View.RefreshSelectedFM(refreshReadme: false);
                }
                return;
            }

            Debug.Assert(fm.Installed, "fm is not installed");

            Debug.Assert(!fm.InstalledDir.IsEmpty(), "fm.InstalledFolderName is null or empty");

            var ac = new AudioConverter(fm, GetFMInstallsBasePath(fm.Game));
            try
            {
                ProgressBox.ShowConvertingFiles();
                await ac.ConvertWAVsTo16Bit();
            }
            finally
            {
                ProgressBox.HideThis();
            }
        }

        #endregion

        #region DML

        internal static bool AddDML(FanMission fm, string sourceDMLPath)
        {
            if (!FMIsReallyInstalled(fm))
            {
                View.ShowAlert(LText.AlertMessages.Patch_AddDML_InstallDirNotFound, LText.AlertMessages.Alert);
                return false;
            }

            var installedFMPath = Path.Combine(GetFMInstallsBasePath(fm.Game), fm.InstalledDir);
            try
            {
                var dmlFile = Path.GetFileName(sourceDMLPath);
                if (dmlFile == null) return false;
                File.Copy(sourceDMLPath, Path.Combine(installedFMPath, dmlFile), overwrite: true);
            }
            catch (Exception ex)
            {
                Log("Unable to add .dml to installed folder " + fm.InstalledDir, ex);
                View.ShowAlert(LText.AlertMessages.Patch_AddDML_UnableToAdd, LText.AlertMessages.Alert);
                return false;
            }

            return true;
        }

        internal static bool RemoveDML(FanMission fm, string dmlFile)
        {
            if (!FMIsReallyInstalled(fm))
            {
                View.ShowAlert(LText.AlertMessages.Patch_RemoveDML_InstallDirNotFound, LText.AlertMessages.Alert);
                return false;
            }

            var installedFMPath = Path.Combine(GetFMInstallsBasePath(fm.Game), fm.InstalledDir);
            try
            {
                File.Delete(Path.Combine(installedFMPath, dmlFile));
            }
            catch (Exception ex)
            {
                Log("Unable to remove .dml from installed folder " + fm.InstalledDir, ex);
                View.ShowAlert(LText.AlertMessages.Patch_RemoveDML_UnableToRemove, LText.AlertMessages.Alert);
                return false;
            }

            return true;
        }

        internal static (bool Success, string[] DMLFiles)
        GetDMLFiles(FanMission fm)
        {
            try
            {
                var dmlFiles = Directory.GetFiles(Path.Combine(GetFMInstallsBasePath(fm.Game), fm.InstalledDir),
                    "*.dml", SearchOption.TopDirectoryOnly);
                for (int i = 0; i < dmlFiles.Length; i++)
                {
                    dmlFiles[i] = Path.GetFileName(dmlFiles[i]);
                }
                return (true, dmlFiles);
            }
            catch (Exception ex)
            {
                Log("Exception getting DML files for " + fm.InstalledDir + ", game: " + fm.Game, ex);
                return (false, new string[] { });
            }
        }

        #endregion

        #region Readme

        internal static (string ReadmePath, ReadmeType ReadmeType)
        GetReadmeFileAndType(FanMission fm)
        {
            Debug.Assert(!fm.InstalledDir.IsEmpty(), "fm.InstalledFolderName is null or empty");

            var instBasePath = GetFMInstallsBasePath(fm.Game);
            if (fm.Installed)
            {
                if (instBasePath.IsWhiteSpace())
                {
                    var ex = new ArgumentException(@"FM installs base path is empty", nameof(instBasePath));
                    Log(ex.Message, ex);
                    throw ex;
                }
                else if (!Directory.Exists(instBasePath))
                {
                    var ex = new DirectoryNotFoundException("FM installs base path doesn't exist");
                    Log(ex.Message, ex);
                    throw ex;
                }
            }

            var readmeOnDisk = FMIsReallyInstalled(fm)
                ? Path.Combine(GetFMInstallsBasePath(fm.Game), fm.InstalledDir, fm.SelectedReadme)
                : Path.Combine(Paths.FMsCache, fm.InstalledDir, fm.SelectedReadme);

            if (fm.SelectedReadme.ExtIsHtml()) return (readmeOnDisk, ReadmeType.HTML);

            var rtfHeader = new char[6];

            // This might throw, but all calls to this method are supposed to be wrapped in a try-catch block
            using (var sr = new StreamReader(readmeOnDisk, Encoding.ASCII)) sr.ReadBlock(rtfHeader, 0, 6);

            var rType = string.Concat(rtfHeader).EqualsI(@"{\rtf1") ? ReadmeType.RichText : ReadmeType.PlainText;

            return (readmeOnDisk, rType);
        }

        // Autodetect safe (non-spoiler) readme
        internal static string DetectSafeReadme(List<string> readmeFiles, string fmTitle)
        {
            // Since an FM's readmes are very few in number, we can afford to be all kinds of lazy and slow here

            string StripPunctuation(string str)
            {
                return str.Replace(" ", "").Replace("-", "").Replace("_", "").Replace(".", "").Replace(",", "")
                    .Replace(";", "").Replace("'", "");
            }

            bool allEqual = true;
            for (var i = 0; i < readmeFiles.Count; i++)
            {
                var rf = readmeFiles[i];
                if (rf == null) continue;

                if (i > 0 && !StripPunctuation(Path.GetFileNameWithoutExtension(readmeFiles[i]))
                        .EqualsI(StripPunctuation(Path.GetFileNameWithoutExtension(readmeFiles[i - 1]))))
                {
                    allEqual = false;
                    break;
                }
            }

            string FirstByPreferredFormat(List<string> files)
            {
                // Don't use IsValidReadme(), because we want a specific search order
                return
                    files.FirstOrDefault(x => x.ExtIsGlml()) ??
                    files.FirstOrDefault(x => x.ExtIsRtf()) ??
                    files.FirstOrDefault(x => x.ExtIsTxt()) ??
                    files.FirstOrDefault(x => x.ExtIsWri()) ??
                    files.FirstOrDefault(x => x.ExtIsHtml());
            }

            bool ContainsUnsafePhrase(string str)
            {
                return str.ContainsI("loot") ||
                       str.ContainsI("walkthrough") ||
                       str.ContainsI("walkthru") ||
                       str.ContainsI("secret") ||
                       str.ContainsI("spoiler") ||
                       str.ContainsI("tips") ||
                       str.ContainsI("convo") ||
                       str.ContainsI("conversation") ||
                       str.ContainsI("cheat") ||
                       str.ContainsI("notes");
            }

            bool ContainsUnsafeOrJunkPhrase(string str)
            {
                return ContainsUnsafePhrase(str) ||
                       str.EqualsI("scripts") ||
                       str.ContainsI("copyright") ||
                       str.ContainsI("install") ||
                       str.ContainsI("update") ||
                       str.ContainsI("patch") ||
                       str.ContainsI("nvscript") ||
                       str.ContainsI("tnhscript") ||
                       str.ContainsI("GayleSaver") ||
                       str.ContainsI("changelog") ||
                       str.ContainsI("changes") ||
                       str.ContainsI("credits") ||
                       str.ContainsI("objectives") ||
                       str.ContainsI("hint");
            }

            var safeReadme = "";
            if (allEqual)
            {
                safeReadme = FirstByPreferredFormat(readmeFiles);
            }
            else
            {

                var safeReadmes = new List<string>();
                foreach (var rf in readmeFiles)
                {
                    if (rf == null) continue;

                    var fn = StripPunctuation(Path.GetFileNameWithoutExtension(rf));

                    if (fn.EqualsI("Readme") || fn.EqualsI("ReadmeEn") || fn.EqualsI("ReadmeEng") ||
                        fn.EqualsI("FMInfo") || fn.EqualsI("FMInfoEn") || fn.EqualsI("FMInfoEng") ||
                        fn.EqualsI("fm") || fn.EqualsI("fmEn") || fn.EqualsI("fmEng") ||
                        fn.EqualsI("GameInfo") || fn.EqualsI("GameInfoEn") || fn.EqualsI("GameInfoEng") ||
                        fn.EqualsI("Mission") || fn.EqualsI("MissionEn") || fn.EqualsI("MissionEng") ||
                        fn.EqualsI("MissionInfo") || fn.EqualsI("MissionInfoEn") || fn.EqualsI("MissionInfoEng") ||
                        fn.EqualsI("Info") || fn.EqualsI("InfoEn") || fn.EqualsI("InfoEng") ||
                        fn.EqualsI("Entry") || fn.EqualsI("EntryEn") || fn.EqualsI("EntryEng") ||
                        fn.EqualsI("English") ||
                        (fn.StartsWithI(StripPunctuation(fmTitle)) && !ContainsUnsafeOrJunkPhrase(fn)) ||
                        (fn.EndsWithI("Readme") && !ContainsUnsafePhrase(fn)))
                    {
                        safeReadmes.Add(rf);
                    }
                }

                if (safeReadmes.Count > 0)
                {
                    safeReadmes.Sort(new SafeReadmeComparer());

                    var eng = safeReadmes.FirstOrDefault(
                        x => Path.GetFileNameWithoutExtension(x).EndsWithI("en") ||
                             Path.GetFileNameWithoutExtension(x).EndsWithI("eng"));
                    foreach (var item in new[] { "readme", "fminfo", "fm", "gameinfo", "mission", "missioninfo", "info", "entry" })
                    {
                        var str = safeReadmes.FirstOrDefault(x => Path.GetFileNameWithoutExtension(x).EqualsI(item));
                        if (str != null)
                        {
                            safeReadmes.Remove(str);
                            safeReadmes.Insert(0, str);
                        }
                    }
                    if (eng != null)
                    {
                        safeReadmes.Remove(eng);
                        safeReadmes.Insert(0, eng);
                    }
                    safeReadme = FirstByPreferredFormat(safeReadmes);
                }
            }

            if (safeReadme.IsEmpty())
            {
                int numSafe = 0;
                int safeIndex = -1;
                for (var i = 0; i < readmeFiles.Count; i++)
                {
                    var rf = readmeFiles[i];
                    if (rf == null) continue;

                    var fn = StripPunctuation(Path.GetFileNameWithoutExtension(rf));
                    if (!ContainsUnsafeOrJunkPhrase(fn))
                    {
                        numSafe++;
                        safeIndex = i;
                    }
                }

                if (numSafe == 1 && safeIndex > -1) safeReadme = readmeFiles[safeIndex];
            }

            return safeReadme;
        }

        #endregion

        internal static void OpenFMFolder(FanMission fm)
        {
            var installsBasePath = GetFMInstallsBasePath(fm.Game);
            if (installsBasePath.IsEmpty())
            {
                View.ShowAlert(LText.AlertMessages.Patch_FMFolderNotFound, LText.AlertMessages.Alert);
                return;
            }
            var fmDir = Path.Combine(installsBasePath, fm.InstalledDir);
            if (!Directory.Exists(fmDir))
            {
                View.ShowAlert(LText.AlertMessages.Patch_FMFolderNotFound, LText.AlertMessages.Alert);
                return;
            }

            try
            {
                Process.Start(fmDir);
            }
            catch (Exception ex)
            {
                Log("Exception trying to open FM folder " + fmDir, ex);
            }
        }

        internal static void OpenWebSearchUrl(FanMission fm)
        {
            var url = Config.WebSearchUrl;
            if (url.IsWhiteSpace() || url.Length > 32766) return;

            var index = url.IndexOf("$TITLE$", StringComparison.OrdinalIgnoreCase);

            var finalUrl = Uri.EscapeUriString(index == -1
                ? url
                : url.Substring(0, index) + fm.Title + url.Substring(index + "$TITLE$".Length));

            try
            {
                Process.Start(finalUrl);
            }
            catch (FileNotFoundException ex)
            {
                Log("\"The PATH environment variable has a string containing quotes.\" (that's what MS docs says?!)", ex);
            }
            catch (Win32Exception ex)
            {
                Log("Problem opening web search URL", ex);
                View.ShowAlert(LText.AlertMessages.WebSearchURL_ProblemOpening, LText.AlertMessages.Alert);
            }
        }

        internal static void ViewHTMLReadme(FanMission fm)
        {
            string path;
            try
            {
                (path, _) = GetReadmeFileAndType(fm);
            }
            catch (Exception ex)
            {
                Log("Exception in " + nameof(GetReadmeFileAndType), ex);
                return;
            }

            if (File.Exists(path))
            {
                try
                {
                    Process.Start(path);
                }
                catch (Exception ex)
                {
                    Log("Exception opening HTML readme " + path, ex);
                }
            }
            else
            {
                Log("File not found: " + path, stackTrace: true);
            }
        }

        internal static void OpenLink(string link)
        {
            try
            {
                Process.Start(link);
            }
            catch (Exception ex)
            {
                Log("Problem opening clickable link from rtfbox", ex);
            }
        }

        #region Add/remove tag

        internal static List<string> ListMatchingTags(string searchText)
        {
            // Smartasses who try to break it get nothing
            if (searchText.CountChars(':') > 1 || searchText.IsWhiteSpace())
            {
                return null;
            }

            (string First, string Second) text;

            var index = searchText.IndexOf(':');
            if (index > -1)
            {
                text.First = searchText.Substring(0, index).Trim();
                text.Second = searchText.Substring(index + 1).Trim();
            }
            else
            {
                text.First = searchText.Trim();
                text.Second = "";
            }

            // Shut up, it works
            var list = new List<string>();
            foreach (var gCat in GlobalTags)
            {
                if (gCat.Category.Name.ContainsI(text.First))
                {
                    if (gCat.Tags.Count == 0)
                    {
                        if (gCat.Category.Name != "misc") list.Add(gCat.Category.Name + ":");
                    }
                    else
                    {
                        foreach (var gTag in gCat.Tags)
                        {
                            if (!text.Second.IsWhiteSpace() && !gTag.Name.ContainsI(text.Second)) continue;
                            if (gCat.Category.Name == "misc")
                            {
                                if (text.Second.IsWhiteSpace() && !gCat.Category.Name.ContainsI(text.First))
                                {
                                    list.Add(gTag.Name);
                                }
                            }
                            else
                            {
                                list.Add(gCat.Category.Name + ": " + gTag.Name);
                            }
                        }
                    }
                }
                // if, not else if - we want to display found tags both categorized and uncategorized
                if (gCat.Category.Name == "misc")
                {
                    foreach (var gTag in gCat.Tags)
                    {
                        if (gTag.Name.ContainsI(searchText)) list.Add(gTag.Name);
                    }
                }
            }

            list.Sort(StringComparer.OrdinalIgnoreCase);

            return list;
        }

        internal static bool RemoveTagFromFM(FanMission fm, string catText, string tagText)
        {
            if (tagText.IsEmpty()) return false;

            // Parent node (category)
            if (catText.IsEmpty())
            {
                // TODO: These messageboxes are annoying, but they prevent accidental deletion.
                // Figure out something better.
                var cont = View.AskToContinue(LText.TagsTab.AskRemoveCategory, LText.TagsTab.TabText, true);
                if (!cont) return false;

                var cat = fm.Tags.FirstOrDefault(x => x.Category == tagText);
                if (cat != null)
                {
                    fm.Tags.Remove(cat);
                    UpdateFMTagsString(fm);

                    // TODO: Profile the FirstOrDefaults and see if I should make them for loops
                    var globalCat = GlobalTags.FirstOrDefault(x => x.Category.Name == cat.Category);
                    if (globalCat != null && !globalCat.Category.IsPreset)
                    {
                        if (globalCat.Category.UsedCount > 0) globalCat.Category.UsedCount--;
                        if (globalCat.Category.UsedCount == 0) GlobalTags.Remove(globalCat);
                    }
                }
            }
            // Child node (tag)
            else
            {
                var cont = View.AskToContinue(LText.TagsTab.AskRemoveTag, LText.TagsTab.TabText, true);
                if (!cont) return false;

                var cat = fm.Tags.FirstOrDefault(x => x.Category == catText);
                var tag = cat?.Tags.FirstOrDefault(x => x == tagText);
                if (tag != null)
                {
                    cat.Tags.Remove(tag);
                    if (cat.Tags.Count == 0) fm.Tags.Remove(cat);
                    UpdateFMTagsString(fm);

                    var globalCat = GlobalTags.FirstOrDefault(x => x.Category.Name == cat.Category);
                    var globalTag = globalCat?.Tags.FirstOrDefault(x => x.Name == tagText);
                    if (globalTag != null && !globalTag.IsPreset)
                    {
                        if (globalTag.UsedCount > 0) globalTag.UsedCount--;
                        if (globalTag.UsedCount == 0) globalCat.Tags.Remove(globalTag);
                    }
                }
            }

            return true;
        }

        #endregion

        internal static void UpdateConfig(
            FormWindowState mainWindowState,
            Size mainWindowSize,
            Point mainWindowLocation,
            float mainSplitterPercent,
            float topSplitterPercent,
            List<ColumnData> columns,
            int sortedColumn,
            SortOrder sortDirection,
            float fmsListFontSizeInPoints,
            Filter filter,
            SelectedFM selectedFM,
            GameTabsState gameTabsState,
            Game gameTab,
            TopRightTab topRightTab,
            TopRightTabOrder topRightTabOrder,
            bool topRightPanelCollapsed,
            float readmeZoomFactor)
        {
            Config.MainWindowState = mainWindowState;
            Config.MainWindowSize = new Size { Width = mainWindowSize.Width, Height = mainWindowSize.Height };
            Config.MainWindowLocation = new Point(mainWindowLocation.X, mainWindowLocation.Y);
            Config.MainSplitterPercent = mainSplitterPercent;
            Config.TopSplitterPercent = topSplitterPercent;

            Config.Columns.ClearAndAdd(columns);

            Config.SortedColumn = (Column)sortedColumn;
            Config.SortDirection = sortDirection;

            Config.FMsListFontSizeInPoints = fmsListFontSizeInPoints;

            filter.DeepCopyTo(Config.Filter);

            Config.TopRightTab = topRightTab;

            Config.TopRightTabOrder.StatsTabPosition = topRightTabOrder.StatsTabPosition;
            Config.TopRightTabOrder.EditFMTabPosition = topRightTabOrder.EditFMTabPosition;
            Config.TopRightTabOrder.CommentTabPosition = topRightTabOrder.CommentTabPosition;
            Config.TopRightTabOrder.TagsTabPosition = topRightTabOrder.TagsTabPosition;
            Config.TopRightTabOrder.PatchTabPosition = topRightTabOrder.PatchTabPosition;

            Config.TopRightPanelCollapsed = topRightPanelCollapsed;

            switch (Config.GameOrganization)
            {
                case GameOrganization.OneList:
                    Config.ClearAllSelectedFMs();
                    selectedFM.DeepCopyTo(Config.SelFM);
                    Config.GameTab = Game.Thief1;
                    break;

                case GameOrganization.ByTab:
                    Config.SelFM.Clear();
                    gameTabsState.DeepCopyTo(Config.GameTabsState);
                    Config.GameTab = gameTab;
                    break;
            }

            Config.ReadmeZoomFactor = readmeZoomFactor;
        }

        private static readonly ReaderWriterLockSlim ReadWriteLock = new ReaderWriterLockSlim();

        internal static void WriteFullFMDataIni()
        {
            try
            {
                ReadWriteLock.EnterWriteLock();
                WriteFMDataIni(FMDataIniList, Paths.FMDataIni);
                ReadWriteLock.ExitWriteLock();
            }
            catch (Exception ex)
            {
                Log("Exception writing FM data ini", ex);
            }
        }

        internal static void Shutdown()
        {
            try
            {
                WriteConfigIni(Config, Paths.ConfigIni);
            }
            catch (Exception ex)
            {
                Log("Exception writing config ini", ex);
            }

            WriteFullFMDataIni();

            Application.Exit();
        }
    }
}
