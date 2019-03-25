﻿using System.Collections.Generic;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;

namespace AngelLoader.Importing
{
    // Has to be public so it can be passed to a public constructor on a form
    public enum ImportType
    {
        DarkLoader,
        NewDarkLoader,
        FMSel
    }

    internal static class ImportCommon
    {
        internal static List<FanMission> MergeImportedFMData(ImportType importType, List<FanMission> importedFMs,
            List<FanMission> mainList)
        {
            // Perf
            var checkedList = new List<FanMission>();
            int initCount = mainList.Count;

            // We can't just send back the list we got in, because we will have deep-copied them to the main list
            var importedFMsInMainList = new List<FanMission>();

            for (int impFMi = 0; impFMi < importedFMs.Count; impFMi++)
            {
                var importedFM = importedFMs[impFMi];

                bool existingFound = false;
                for (int mainFMi = 0; mainFMi < initCount; mainFMi++)
                {
                    var mainFM = mainList[mainFMi];

                    if (!mainFM.Checked &&
                        (importType == ImportType.DarkLoader &&
                         mainFM.Archive.EqualsI(importedFM.Archive)) ||
                        (importType == ImportType.FMSel &&
                         (!importedFM.Archive.IsEmpty() && mainFM.Archive.EqualsI(importedFM.Archive)) ||
                          importedFM.InstalledDir.EqualsI(mainFM.InstalledDir)) ||
                        (importType == ImportType.NewDarkLoader &&
                         mainFM.InstalledDir.EqualsI(importedFM.InstalledDir)))
                    {
                        if (!importedFM.Title.IsEmpty()) mainFM.Title = importedFM.Title;
                        if (importedFM.ReleaseDate != null) mainFM.ReleaseDate = importedFM.ReleaseDate;
                        mainFM.LastPlayed = importedFM.LastPlayed;
                        mainFM.FinishedOn = importedFM.FinishedOn;
                        if (importType != ImportType.FMSel) mainFM.FinishedOnUnknown = false;
                        mainFM.Comment = importedFM.Comment;

                        if (importType == ImportType.NewDarkLoader ||
                            importType == ImportType.FMSel)
                        {
                            mainFM.Rating = importedFM.Rating;
                            mainFM.DisabledMods = importedFM.DisabledMods;
                            mainFM.DisableAllMods = importedFM.DisableAllMods;
                            mainFM.TagsString = importedFM.TagsString;
                            mainFM.SelectedReadme = importedFM.SelectedReadme;
                        }
                        if (importType == ImportType.NewDarkLoader || importType == ImportType.DarkLoader)
                        {
                            if (mainFM.SizeBytes == 0) mainFM.SizeBytes = importedFM.SizeBytes;
                        }
                        else if (importType == ImportType.FMSel && mainFM.FinishedOn == 0 && !mainFM.FinishedOnUnknown)
                        {
                            mainFM.FinishedOnUnknown = importedFM.FinishedOnUnknown;
                        }

                        mainFM.MarkedScanned = true;

                        mainFM.Checked = true;

                        // So we only loop through checked FMs when we reset them
                        checkedList.Add(mainFM);

                        importedFMsInMainList.Add(mainFM);

                        existingFound = true;
                        break;
                    }
                }
                if (!existingFound)
                {
                    var newFM = new FanMission
                    {
                        Archive = importedFM.Archive,
                        InstalledDir = importedFM.InstalledDir,
                        Title =
                            !importedFM.Title.IsEmpty() ? importedFM.Title :
                            !importedFM.Archive.IsEmpty() ? importedFM.Archive :
                            importedFM.InstalledDir,
                        ReleaseDate = importedFM.ReleaseDate,
                        LastPlayed = importedFM.LastPlayed,
                        Comment = importedFM.Comment
                    };

                    if (importType == ImportType.NewDarkLoader ||
                        importType == ImportType.FMSel)
                    {
                        newFM.Rating = importedFM.Rating;
                        newFM.DisabledMods = importedFM.DisabledMods;
                        newFM.DisableAllMods = importedFM.DisableAllMods;
                        newFM.TagsString = importedFM.TagsString;
                        newFM.SelectedReadme = importedFM.SelectedReadme;
                    }
                    if (importType == ImportType.NewDarkLoader || importType == ImportType.DarkLoader)
                    {
                        newFM.SizeBytes = importedFM.SizeBytes;
                        newFM.FinishedOn = importedFM.FinishedOn;
                    }
                    else if (importType == ImportType.FMSel)
                    {
                        newFM.FinishedOnUnknown = importedFM.FinishedOnUnknown;
                    }

                    newFM.MarkedScanned = true;

                    mainList.Add(newFM);
                    importedFMsInMainList.Add(newFM);
                }
            }

            // Reset temp bool
            for (int i = 0; i < checkedList.Count; i++) checkedList[i].Checked = false;

            return importedFMsInMainList;
        }
    }
}
