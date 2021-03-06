﻿#define FenGen_FMDataSource

using System;
using System.Collections.Generic;
using static AngelLoader.GameSupport;

namespace AngelLoader.DataClasses
{
    // FenGen reads this and outputs fast ini read and write methods.
    // TODO: Version-header ini files right from the start, in case I have to change the format after release
    /*
     Notes to self:
        -Keep properties to one line, cause FenGen doesn't handle multiline ones
        -Keep names shortish for more performance when reading
    */

    // [FenGen:WriteEmptyValues=false]
    internal sealed class FanMission
    {
        // Cached value to avoid doing the expensive check every startup. If a matching archive is found in the
        // normal archive list combine, this will be set to false again. Results in a nice perf gain if there are
        // archive-less FMs in the list.
        internal bool NoArchive;

        // Since our scanned values are very complex due to having the option to choose what to scan for as well
        // as being able to import from three other loaders, we need a simple way to say "scan on select or not".
        internal bool MarkedScanned;

        // For drawing rows in "Recently-Added Yellow" color
        // [FenGen:DoNotSerialize]
        internal bool MarkedRecent;

        // Disgusting hack to let an FM disappear from the list after being deleted. It will only be filtered out,
        // but on next run of the FM finder, it will be properly removed if the archive is no longer there.
        // [FenGen:DoNotSerialize]
        internal bool MarkedDeleted;

        internal string Archive = "";
        internal string InstalledDir = "";

        internal string Title = "";
        // [FenGen:ListType=MultipleLines]
        internal readonly List<string> AltTitles = new List<string>();

        internal string Author = "";

        internal Game Game = Game.Null;

        internal bool Installed;

        internal bool NoReadmes;

        private string _selectedReadme = "";
        // @DIRSEP: Always backslashes for backward compatibility and prevention of find misses in readme chooser box
        internal string SelectedReadme { get => _selectedReadme; set => _selectedReadme = value.ToBackSlashes(); }

        // [FenGen:DoNotSerialize]
        private ulong _sizeBytes = 0;
        // [FenGen:NumericEmpty=0]
        internal ulong SizeBytes { get => _sizeBytes; set => _sizeBytes = value.Clamp(ulong.MinValue, ulong.MaxValue); }

        // [FenGen:DoNotSerialize]
        private int _rating = -1;
        // [FenGen:NumericEmpty=-1]
        internal int Rating { get => _rating; set => _rating = value.Clamp(-1, 10); }

        internal readonly ExpandableDate ReleaseDate = new ExpandableDate();
        internal readonly ExpandableDate LastPlayed = new ExpandableDate();

        // We get this value for free when we get the FM archives and dirs on startup, but this value is fragile:
        // it updates whenever the user so much as moves the file or folder. We store it here to keep it permanent
        // even across moves, new PCs or Windows installs with file restores, etc.
        // NOTE: This is not an ExpandableDate, because the way we get the date value is not in unix hex string
        // format, and it's expensive to convert it to such. With a regular nullable DateTime we're only paying
        // like 3-5ms extra on startup (for 1574 FMs), so it's good enough for now.
        internal DateTime? DateAdded = null;

        // [FenGen:DoNotSerialize]
        private uint _finishedOn = 0;
        // [FenGen:NumericEmpty=0]
        internal uint FinishedOn { get => _finishedOn; set => _finishedOn = value.Clamp(0u, 15u); }
        internal bool FinishedOnUnknown;

        // [FenGen:DoNotSerialize]
        internal string CommentSingleLine = "";
        internal string Comment = "";

        internal string DisabledMods = "";
        internal bool DisableAllMods;

        // [FenGen:DoNotSerialize]
        internal bool ResourcesScanned;
        internal CustomResources Resources = CustomResources.None;

        internal bool LangsScanned;
        internal string Langs = "";
        internal string SelectedLang = "";

        // [FenGen:DoNotSerialize]
        internal readonly CatAndTagsList Tags = new CatAndTagsList();
        internal string TagsString = "";
    }
}
