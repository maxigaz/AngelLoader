﻿using System;
using System.Diagnostics;
using AngelLoader.Common.Utility;
using static AngelLoader.Common.DataClasses.TopRightTabEnumStatic;

namespace AngelLoader.Common.DataClasses
{
    internal sealed class ConfigVar
    {
        internal string Name = "";
        internal string Command = "";
    }

    #region Columns

    internal sealed class ColumnData
    {
        internal Column Id;
        internal int DisplayIndex = -1;
        internal int Width = Defaults.ColumnWidth;
        internal bool Visible = true;
    }

    // Public for interface use
    public enum Column
    {
        Game,
        Installed,
        Title,
        Archive,
        Author,
        Size,
        Rating,
        Finished,
        ReleaseDate,
        LastPlayed,
        DisabledMods,
        Comment
    }

    #endregion

    [Flags] internal enum Game : uint { Null = 0, Thief1 = 1, Thief2 = 2, Thief3 = 4, Unsupported = 8 }

    internal enum GameOrganization { ByTab, OneList }

    // Public for interface use
    public enum RatingDisplayStyle { NewDarkLoader, FMSel }

    internal enum DateFormat { CurrentCultureShort, CurrentCultureLong, Custom }

    [Flags] internal enum FinishedState : uint { Null = 0, Finished = 1, Unfinished = 2 }

    [Flags] internal enum FinishedOn : uint { None = 0, Normal = 1, Hard = 2, Expert = 4, Extreme = 8 }

    internal enum BackupFMData { SavesAndScreensOnly, AllChangedFiles }

    // Struct for immutability?!
    internal struct FMInstallPaths
    {
        internal FMInstallPaths(string t1, string t2, string t3)
        {
            T1 = t1;
            T2 = t2;
            T3 = t3;
        }
        internal readonly string T1;
        internal readonly string T2;
        internal readonly string T3;
    }

    #region Top-right tabs

    // Dopey, but for perf so we only have to get it once
    internal static class TopRightTabEnumStatic
    {
        internal static readonly int TopRightTabsCount = Enum.GetValues(typeof(TopRightTab)).Length;
    }

    internal enum TopRightTab { Statistics, EditFM, Comment, Tags, Patch }

    internal sealed class TopRightTabData
    {
        private int _position;
        internal int Position { get => _position; set => _position = value.Clamp(0, TopRightTabsCount - 1); }

        internal bool Visible = true;
    }

    internal sealed class TopRightTabsData
    {
        internal readonly TopRightTabData[] Tabs = new TopRightTabData[TopRightTabsCount];

        internal TopRightTab SelectedTab = TopRightTab.Statistics;

        internal TopRightTabsData()
        {
            for (int i = 0; i < TopRightTabsCount; i++) Tabs[i] = new TopRightTabData();
            ResetAllPositions();
        }

        internal TopRightTabData StatsTab => Tabs[(int)TopRightTab.Statistics];
        internal TopRightTabData EditFMTab => Tabs[(int)TopRightTab.EditFM];
        internal TopRightTabData CommentTab => Tabs[(int)TopRightTab.Comment];
        internal TopRightTabData TagsTab => Tabs[(int)TopRightTab.Tags];
        internal TopRightTabData PatchTab => Tabs[(int)TopRightTab.Patch];

        internal void EnsureValidity()
        {
            #region Fallback if multiple tabs have the same position

            int[] set = { -1, -1, -1, -1, -1 };

            Debug.Assert(set.Length == TopRightTabsCount, nameof(set) + ".Length != " + nameof(TopRightTabsCount));

            // PERF: Unmeasurable. LINQ Distinct().Count() was 6ms. Sheesh.
            for (int i = 0; i < Tabs.Length; i++)
            {
                for (int j = 0; j < set.Length; j++)
                {
                    if (set[j] == Tabs[i].Position)
                    {
                        ResetAllPositions();
                        goto breakout;
                    }
                }

                set[i] = Tabs[i].Position;
            }

            breakout:

            #endregion

            // Fallback if no tabs are visible
            if (NoneVisible()) SetAllVisible(true);

            // Fallback if selected tab is not marked as visible
            if (!Tabs[(int)SelectedTab].Visible)
            {
                for (int i = 0; i < TopRightTabsCount; i++)
                {
                    if (Tabs[i].Visible)
                    {
                        SelectedTab = (TopRightTab)i;
                        break;
                    }
                }
            }
        }

        private bool NoneVisible()
        {
            for (int i = 0; i < TopRightTabsCount; i++) if (Tabs[i].Visible) return false;
            return true;
        }

        private void SetAllVisible(bool visible)
        {
            for (int i = 0; i < TopRightTabsCount; i++) Tabs[i].Visible = visible;
        }

        private void ResetAllPositions()
        {
            for (int i = 0; i < TopRightTabsCount; i++) Tabs[i].Position = i;
        }
    }

    #endregion

    #region Filter

    internal sealed class Filter
    {
        internal void Clear(bool clearGames = true)
        {
            Title = "";
            Author = "";
            if (clearGames) Games = Game.Null;
            Tags.Clear();
            _ratingFrom = -1;
            _ratingTo = 10;
            _releaseDateFrom = null;
            _releaseDateTo = null;
            _lastPlayedFrom = null;
            _lastPlayedTo = null;
            Finished = FinishedState.Null;
            ShowUnsupported = false;
        }

        internal string Title = "";
        internal string Author = "";
        internal Game Games = Game.Null;
        internal readonly TagsFilter Tags = new TagsFilter();

        #region Rating

        internal void SetRatingFromAndTo(int from, int to)
        {
            RatingFrom = Math.Min(from, to);
            RatingTo = Math.Max(from, to);
        }
        private int _ratingFrom = -1;
        internal int RatingFrom { get => _ratingFrom; set => _ratingFrom = value.Clamp(-1, 10); }
        private int _ratingTo = 10;
        internal int RatingTo { get => _ratingTo; set => _ratingTo = value.Clamp(-1, 10); }

        #endregion

        #region Release date

        internal void SetReleaseDateFromAndTo(DateTime? from, DateTime? to)
        {
            if (from != null && to != null && from.Value.CompareTo(to) > 0)
            {
                ReleaseDateFrom = to;
                ReleaseDateTo = from;
            }
            else
            {
                ReleaseDateFrom = from;
                ReleaseDateTo = to;
            }
        }

        private DateTime? _releaseDateFrom;
        internal DateTime? ReleaseDateFrom
        {
            get => _releaseDateFrom;
            set => _releaseDateFrom = value?.Clamp(DateTime.MinValue, DateTime.MaxValue);
        }
        private DateTime? _releaseDateTo;
        internal DateTime? ReleaseDateTo
        {
            get => _releaseDateTo;
            set => _releaseDateTo = value?.Clamp(DateTime.MinValue, DateTime.MaxValue);
        }

        #endregion

        #region Last played

        internal void SetLastPlayedFromAndTo(DateTime? from, DateTime? to)
        {
            if (from != null && to != null && from.Value.CompareTo(to) > 0)
            {
                LastPlayedFrom = to;
                LastPlayedTo = from;
            }
            else
            {
                LastPlayedFrom = from;
                LastPlayedTo = to;
            }
        }

        private DateTime? _lastPlayedFrom;
        internal DateTime? LastPlayedFrom
        {
            get => _lastPlayedFrom;
            set => _lastPlayedFrom = value?.Clamp(DateTime.MinValue, DateTime.MaxValue);
        }
        private DateTime? _lastPlayedTo;
        internal DateTime? LastPlayedTo
        {
            get => _lastPlayedTo;
            set => _lastPlayedTo = value?.Clamp(DateTime.MinValue, DateTime.MaxValue);
        }

        #endregion

        internal FinishedState Finished = FinishedState.Null;

        internal bool ShowUnsupported;

        internal void DeepCopyTo(Filter dest)
        {
            dest.Clear();
            dest.Title = Title;
            dest.Author = Author;
            dest.SetRatingFromAndTo(RatingFrom, RatingTo);
            dest.ShowUnsupported = ShowUnsupported;

            var relFrom = ReleaseDateFrom == null
                ? (DateTime?)null
                : new DateTime(ReleaseDateFrom.Value.Year, ReleaseDateFrom.Value.Month,
                    ReleaseDateFrom.Value.Day, 0, 0, 0, ReleaseDateFrom.Value.Kind);
            var relTo = ReleaseDateTo == null
                ? (DateTime?)null
                : new DateTime(ReleaseDateTo.Value.Year, ReleaseDateTo.Value.Month,
                    ReleaseDateTo.Value.Day, 0, 0, 0, ReleaseDateTo.Value.Kind);

            dest.SetReleaseDateFromAndTo(relFrom, relTo);

            var lpFrom = LastPlayedFrom == null
                ? (DateTime?)null
                : new DateTime(LastPlayedFrom.Value.Year, LastPlayedFrom.Value.Month,
                    LastPlayedFrom.Value.Day, 0, 0, 0, LastPlayedFrom.Value.Kind);
            var lpTo = LastPlayedTo == null
                ? (DateTime?)null
                : new DateTime(LastPlayedTo.Value.Year, LastPlayedTo.Value.Month,
                    LastPlayedTo.Value.Day, 0, 0, 0, LastPlayedTo.Value.Kind);

            dest.SetLastPlayedFromAndTo(lpFrom, lpTo);

            dest.Finished = Finished;
            dest.Games = Games;
            Tags.DeepCopyTo(dest.Tags);
        }
    }

    internal sealed class TagsFilter
    {
        internal readonly CatAndTagsList AndTags = new CatAndTagsList();
        internal readonly CatAndTagsList OrTags = new CatAndTagsList();
        internal readonly CatAndTagsList NotTags = new CatAndTagsList();

        internal void Clear()
        {
            AndTags.Clear();
            OrTags.Clear();
            NotTags.Clear();
        }

        internal bool IsEmpty() => AndTags.Count == 0 &&
                                   OrTags.Count == 0 &&
                                   NotTags.Count == 0;

        internal void DeepCopyTo(TagsFilter dest)
        {
            AndTags.DeepCopyTo(dest.AndTags);
            OrTags.DeepCopyTo(dest.OrTags);
            NotTags.DeepCopyTo(dest.NotTags);
        }
    }

    #endregion

    internal enum SettingsTab { Paths, FMDisplay, Other }

    public sealed class SelectedFM
    {
        internal void DeepCopyTo(SelectedFM dest)
        {
            dest.IndexFromTop = IndexFromTop;
            dest.InstalledName = InstalledName;
        }

        internal void Clear()
        {
            IndexFromTop = 0;
            InstalledName = null;
        }

        private string _installedName;
        internal string InstalledName { get => _installedName; set => _installedName = value.IsEmpty() ? null : value; }

        private int _indexFromTop;
        /// <summary>
        /// The index relative to the first displayed item (not the first item period) in the list.
        /// </summary>
        internal int IndexFromTop { get => _indexFromTop; set => _indexFromTop = value.ClampToZero(); }
    }

    internal sealed class GameTabsState
    {
        internal readonly SelectedFM T1SelFM = new SelectedFM();
        internal readonly SelectedFM T2SelFM = new SelectedFM();
        internal readonly SelectedFM T3SelFM = new SelectedFM();

        internal readonly Filter T1Filter = new Filter();
        internal readonly Filter T2Filter = new Filter();
        internal readonly Filter T3Filter = new Filter();

        // TODO: Add sorted column / sort order as a per-tab thing

        internal void DeepCopyTo(GameTabsState dest)
        {
            T1Filter.DeepCopyTo(dest.T1Filter);
            T2Filter.DeepCopyTo(dest.T2Filter);
            T3Filter.DeepCopyTo(dest.T3Filter);
            T1SelFM.DeepCopyTo(dest.T1SelFM);
            T2SelFM.DeepCopyTo(dest.T2SelFM);
            T3SelFM.DeepCopyTo(dest.T3SelFM);
        }

        internal void ClearSelectedFMs()
        {
            T1SelFM.Clear();
            T2SelFM.Clear();
            T3SelFM.Clear();
        }
    }
}
