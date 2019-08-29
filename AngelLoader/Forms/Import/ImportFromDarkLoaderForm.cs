﻿using System;
using System.IO;
using System.Windows.Forms;
using AngelLoader.Common;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;

namespace AngelLoader.Forms.Import
{
    public partial class ImportFromDarkLoaderForm : Form
    {
        internal string DarkLoaderIniFile = "";
        internal bool ImportFMData;
        internal bool ImportTitle;
        internal bool ImportSize;
        internal bool ImportComment;
        internal bool ImportReleaseDate;
        internal bool ImportLastPlayed;
        internal bool ImportFinishedOn;

        internal bool ImportSaves;

        internal ImportFromDarkLoaderForm() => InitializeComponent();

        private void ImportFromDarkLoaderForm_Load(object sender, EventArgs e) => Localize();

        private void Localize()
        {
            Text = LText.Importing.ImportFromDarkLoader_TitleText;
            ImportControls.Localize();
            OKButton.SetTextAutoSize(LText.Global.OK, OKButton.Width);
            Cancel_Button.SetTextAutoSize(LText.Global.Cancel, Cancel_Button.Width);
        }

        private void ImportFromDarkLoaderForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult != DialogResult.OK) return;

            var file = ImportControls.DarkLoaderIniText;

            bool fileNameIsDLIni;
            try
            {
                fileNameIsDLIni = Path.GetFileName(file).EqualsI(Paths.DarkLoaderIni);
            }
            catch (ArgumentException)
            {
                MessageBox.Show(LText.Importing.SelectedFileIsNotAValidPath);
                e.Cancel = true;
                return;
            }

            if (!fileNameIsDLIni)
            {
                // TODO: do something nicer here
                MessageBox.Show(LText.Importing.DarkLoader_SelectedFileIsNotDarkLoaderIni);
                e.Cancel = true;
                return;
            }

            var iniFileExists = File.Exists(file);
            if (!iniFileExists)
            {
                MessageBox.Show(LText.Importing.DarkLoader_SelectedDarkLoaderIniWasNotFound);
                e.Cancel = true;
                return;
            }

            DarkLoaderIniFile = file;
            ImportTitle = ImportControls.ImportTitle;
            ImportSize = ImportControls.ImportSize;
            ImportComment = ImportControls.ImportComment;
            ImportReleaseDate = ImportControls.ImportReleaseDate;
            ImportLastPlayed = ImportControls.ImportLastPlayed;
            ImportFinishedOn = ImportControls.ImportFinishedOn;

            ImportFMData = ImportControls.ImportFMData &&
                           (ImportTitle || ImportSize || ImportComment || ImportReleaseDate ||
                            ImportLastPlayed || ImportFinishedOn);

            ImportSaves = ImportControls.ImportSaves;
        }

        #region Research notes

        /* DarkLoader:

        Saves:
        -When you uninstall a mission, it puts the saves in [GameExePath]\allsaves
            ex. C:\Thief2\allsaves
        -Saves are put into a zip, in its base directory (no "saves" folder within)
        -The zip is named [archive]_saves.zip
            ex. 2002-02-19_Justforshow_saves.zip
        -This name is NOT run through the badchar remover, but it does appear to have whitespace trimmed off both
         ends.

        ---

        Non-FM headers:
        [options]
        [window]
        [mission directories]
        [Thief 1]
        [Thief 2]
        [Thief2x]
        [SShock 2]
        (or anything that doesn't have a .number at the end)
         
        FM headers look like this:
        
        [1999-06-11_DeceptiveSceptre,The.538256]
        
        First an opening bracket ('[') then the archive name minus the extension (which is always '.zip'), in
        full (not truncated), with the following characters removed:
        ], Chr(9) (TAB), Chr(10) (LF), Chr(13) (CR)
         
        or, put another way:
        badchars=[']',#9,#10,#13];

        Then comes a dot (.) followed by the size, in bytes, of the archive, then a closing bracket (']').

        FM key-value pairs:
        type=(int)
        
        Will be one of these values (numeric, not named):
        darkGameUnknown = 0; <- if it hasn't been scanned, it will be this
        darkGameThief   = 1;
        darkGameThief2  = 2;
        darkGameT2x     = 3;
        darkGameSS2     = 4;

        but we should ignore this and scan for the type ourselves, because:
        a) DarkLoader gets the type wrong with NewDark (marks Thief1 as Thief2), and
        b) we don't want to pollute our own list with archive types we don't support (T2x, SS2)

        comment=(string)
        Looks like this:
        comment="This is a comment"
        The string is always surrounded with double-quotes (").
        Escapes are handled like this:
        #9  -> \t
        #10 -> \n
        #13 -> \r
        "   -> \"
        \   -> \\

        title=(string)
        Handled the exact same way as comment= above.

        misdate=(int)
        Mission release date in number of days since December 30, 1899.

        date=(int)
        Last played date in number of days since December 30, 1899.

        finished=(int)
        A 4-bit flags field, exactly the same as AngelLoader uses, so no conversion needed at all.
        */

        #endregion
    }
}
