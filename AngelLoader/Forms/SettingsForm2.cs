﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngelLoader.Common;
using AngelLoader.Common.DataClasses;
using AngelLoader.CustomControls.SettingsForm;
using System.IO;
using System.Text.RegularExpressions;
using AngelLoader.Common.Utility;
using AngelLoader.Properties;
using AngelLoader.WinAPI.Dialogs;
using static AngelLoader.Common.Logger;

namespace AngelLoader.Forms
{
    internal sealed partial class SettingsForm2 : Form, IEventDisabler, ILocalizable, ISettingsWindow
    {
        private readonly ILocalizable OwnerForm;

        private readonly bool Startup;

        private readonly ConfigData InConfig;
        public ConfigData OutConfig { get; } = new ConfigData();

        private readonly TextBox[] GameExePathTextBoxes;

        public new DialogResult ShowDialog() => ((Form)this).ShowDialog();

        private enum PathError { True, False }

        private enum PageIndex
        {
            Paths,
            FMDisplay,
            Other
        }

        private readonly UserControl[] Pages;

        // August 4 is chosen more-or-less randomly, but both its name and its number are different short vs. long
        // (Aug vs. August; 8 vs. 08), and the same thing with 4 (4 vs. 04).
        private readonly DateTime exampleDate = new DateTime(DateTime.Now.Year, 8, 4);

        public bool EventsDisabled { get; set; }

        private readonly PathsPage PathsPage = new PathsPage { Visible = false };
        private readonly FMDisplayPage FMDisplayPage = new FMDisplayPage { Visible = false };
        private readonly OtherPage OtherPage = new OtherPage { Visible = false };

        internal SettingsForm2(ILocalizable ownerForm, ConfigData config)
        {
            InitializeComponent();

            OwnerForm = ownerForm;
            InConfig = config;

            #region Add pages

            Pages = new UserControl[] { PathsPage, FMDisplayPage, OtherPage };

            PagePanel.Controls.Add(PathsPage);
            PagePanel.Controls.Add(FMDisplayPage);
            PagePanel.Controls.Add(OtherPage);

            PathsPage.Dock = DockStyle.Fill;
            FMDisplayPage.Dock = DockStyle.Fill;
            OtherPage.Dock = DockStyle.Fill;

            #endregion

            #region Hook up page events

            PathsPage.Thief1ExePathTextBox.Leave += GameExePathTextBoxes_Leave;
            PathsPage.Thief1ExePathBrowseButton.Click += GameExePathBrowseButtons_Click;
            PathsPage.Thief3ExePathBrowseButton.Click += GameExePathBrowseButtons_Click;
            PathsPage.Thief2ExePathBrowseButton.Click += GameExePathBrowseButtons_Click;
            PathsPage.Thief3ExePathTextBox.Leave += GameExePathTextBoxes_Leave;
            PathsPage.Thief2ExePathTextBox.Leave += GameExePathTextBoxes_Leave;
            PathsPage.AddFMArchivePathButton.Click += AddFMArchivePathButton_Click;
            PathsPage.RemoveFMArchivePathButton.Click += RemoveFMArchivePathButton_Click;
            PathsPage.BackupPathTextBox.Leave += BackupPathTextBox_Leave;
            PathsPage.BackupPathBrowseButton.Click += BackupPathBrowseButton_Click;
            FMDisplayPage.RatingUseStarsCheckBox.CheckedChanged += RatingUseStarsCheckBox_CheckedChanged;
            FMDisplayPage.RatingFMSelDisplayStyleRadioButton.CheckedChanged += RatingOutOfFiveRadioButton_CheckedChanged;
            FMDisplayPage.RatingNDLDisplayStyleRadioButton.CheckedChanged += RatingOutOfTenRadioButton_CheckedChanged;
            FMDisplayPage.DateSeparator2TextBox.TextChanged += DateSeparatorTextBoxes_TextChanged;
            FMDisplayPage.Date1ComboBox.SelectedIndexChanged += DateComboBoxes_SelectedIndexChanged;
            FMDisplayPage.DateSeparator1TextBox.TextChanged += DateSeparatorTextBoxes_TextChanged;
            FMDisplayPage.Date4ComboBox.SelectedIndexChanged += DateComboBoxes_SelectedIndexChanged;
            FMDisplayPage.Date2ComboBox.SelectedIndexChanged += DateComboBoxes_SelectedIndexChanged;
            FMDisplayPage.Date3ComboBox.SelectedIndexChanged += DateComboBoxes_SelectedIndexChanged;
            FMDisplayPage.DateCustomRadioButton.CheckedChanged += DateCustomRadioButton_CheckedChanged;
            FMDisplayPage.DateCurrentCultureLongRadioButton.CheckedChanged += DateCurrentCultureLongRadioButton_CheckedChanged;
            FMDisplayPage.DateCurrentCultureShortRadioButton.CheckedChanged += DateCurrentCultureShortRadioButton_CheckedChanged;
            FMDisplayPage.EnableIgnoreArticlesCheckBox.CheckedChanged += ArticlesCheckBox_CheckedChanged;
            FMDisplayPage.ArticlesTextBox.Leave += ArticlesTextBox_Leave;
            OtherPage.LanguageComboBox.SelectedIndexChanged += LanguageComboBox_SelectedIndexChanged;
            OtherPage.WebSearchUrlResetButton.Click += WebSearchURLResetButton_Click;

            #endregion

            GameExePathTextBoxes = new[]
            {
                PathsPage.Thief1ExePathTextBox,
                PathsPage.Thief2ExePathTextBox,
                PathsPage.Thief3ExePathTextBox
            };

            Text = LText.SettingsWindow.TitleText;

            PagesListBox.SelectedIndex = (int)(
                InConfig.SettingsTab == SettingsTab.FMDisplay ? PageIndex.FMDisplay :
                InConfig.SettingsTab == SettingsTab.Other ? PageIndex.Other :
                PageIndex.Paths);

            // Language can change while the form is open, so store original sizes for later use as minimums
            OKButton.Tag = OKButton.Size;
            Cancel_Button.Tag = Cancel_Button.Size;

            Width = Math.Min(InConfig.SettingsWindowSize.Width, Screen.PrimaryScreen.WorkingArea.Width);
            Height = Math.Min(InConfig.SettingsWindowSize.Height, Screen.PrimaryScreen.WorkingArea.Height);
            MainSplitContainer.SplitterDistance = InConfig.SettingsWindowSplitterDistance;
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            #region Paths page

            PathsPage.Thief1ExePathTextBox.Text = InConfig.T1Exe;
            PathsPage.Thief2ExePathTextBox.Text = InConfig.T2Exe;
            PathsPage.Thief3ExePathTextBox.Text = InConfig.T3Exe;

            PathsPage.BackupPathTextBox.Text = InConfig.FMsBackupPath;

            PathsPage.FMArchivePathsListBox.Items.Clear();
            foreach (var path in InConfig.FMArchivePaths) PathsPage.FMArchivePathsListBox.Items.Add(path);

            PathsPage.IncludeSubfoldersCheckBox.Checked = InConfig.FMArchivePathsIncludeSubfolders;

            #endregion

            #region FM Display page

            #region Game organization

            switch (InConfig.GameOrganization)
            {
                case GameOrganization.ByTab:
                    FMDisplayPage.OrganizeGamesByTabRadioButton.Checked = true;
                    break;
                case GameOrganization.OneList:
                    FMDisplayPage.SortGamesInOneListRadioButton.Checked = true;
                    break;
            }

            #region Articles

            FMDisplayPage.EnableIgnoreArticlesCheckBox.Checked = InConfig.EnableArticles;

            for (var i = 0; i < InConfig.Articles.Count; i++)
            {
                var article = InConfig.Articles[i];
                if (i > 0) FMDisplayPage.ArticlesTextBox.Text += @", ";
                FMDisplayPage.ArticlesTextBox.Text += article;
            }

            FMDisplayPage.MoveArticlesToEndCheckBox.Checked = InConfig.MoveArticlesToEnd;

            #region Date format

            // NOTE: This section actually depends on the events in order to work. Also it appears to depend on
            // none of the date-related checkboxes being checked by default. Absolutely don't make any of them
            // checked by default!

            switch (InConfig.DateFormat)
            {
                case DateFormat.CurrentCultureShort:
                    FMDisplayPage.DateCurrentCultureShortRadioButton.Checked = true;
                    break;
                case DateFormat.CurrentCultureLong:
                    FMDisplayPage.DateCurrentCultureLongRadioButton.Checked = true;
                    break;
                case DateFormat.Custom:
                    FMDisplayPage.DateCustomRadioButton.Checked = true;
                    break;
            }

            object[] dateFormatList = { "", "d", "dd", "ddd", "dddd", "M", "MM", "MMM", "MMMM", "yy", "yyyy" };
            FMDisplayPage.Date1ComboBox.Items.AddRange(dateFormatList);
            FMDisplayPage.Date2ComboBox.Items.AddRange(dateFormatList);
            FMDisplayPage.Date3ComboBox.Items.AddRange(dateFormatList);
            FMDisplayPage.Date4ComboBox.Items.AddRange(dateFormatList);

            var d1 = InConfig.DateCustomFormat1;
            var s1 = InConfig.DateCustomSeparator1;
            var d2 = InConfig.DateCustomFormat2;
            var s2 = InConfig.DateCustomSeparator2;
            var d3 = InConfig.DateCustomFormat3;
            var s3 = InConfig.DateCustomSeparator3;
            var d4 = InConfig.DateCustomFormat4;

            FMDisplayPage.Date1ComboBox.SelectedItem = !d1.IsEmpty() && FMDisplayPage.Date1ComboBox.Items.Contains(d1) ? d1 : "dd";
            FMDisplayPage.DateSeparator1TextBox.Text = !s1.IsEmpty() ? s1 : "/";
            FMDisplayPage.Date2ComboBox.SelectedItem = !d2.IsEmpty() && FMDisplayPage.Date2ComboBox.Items.Contains(d2) ? d2 : "MM";
            FMDisplayPage.DateSeparator2TextBox.Text = !s2.IsEmpty() ? s2 : "/";
            FMDisplayPage.Date3ComboBox.SelectedItem = !d3.IsEmpty() && FMDisplayPage.Date3ComboBox.Items.Contains(d3) ? d3 : "yyyy";
            FMDisplayPage.DateSeparator3TextBox.Text = !s3.IsEmpty() ? s3 : "";
            FMDisplayPage.Date4ComboBox.SelectedItem = !d4.IsEmpty() && FMDisplayPage.Date4ComboBox.Items.Contains(d4) ? d4 : "";

            #endregion

            #region Rating display style

            using (new DisableEvents(this))
            {
                switch (InConfig.RatingDisplayStyle)
                {
                    case RatingDisplayStyle.NewDarkLoader:
                        FMDisplayPage.RatingNDLDisplayStyleRadioButton.Checked = true;
                        break;
                    case RatingDisplayStyle.FMSel:
                        FMDisplayPage.RatingFMSelDisplayStyleRadioButton.Checked = true;
                        break;
                }

                FMDisplayPage.RatingUseStarsCheckBox.Checked = InConfig.RatingUseStars;

                FMDisplayPage.RatingExamplePictureBox.Image = FMDisplayPage.RatingNDLDisplayStyleRadioButton.Checked
                    ? Resources.RatingExample_NDL
                    : FMDisplayPage.RatingFMSelDisplayStyleRadioButton.Checked && FMDisplayPage.RatingUseStarsCheckBox.Checked
                    ? Resources.RatingExample_FMSel_Stars
                    : Resources.RatingExample_FMSel_Number;

                FMDisplayPage.RatingUseStarsCheckBox.Enabled = FMDisplayPage.RatingFMSelDisplayStyleRadioButton.Checked;
            }

            #endregion

            #endregion

            #endregion

            #region File conversion

            OtherPage.ConvertWAVsTo16BitOnInstallCheckBox.Checked = InConfig.ConvertWAVsTo16BitOnInstall;
            OtherPage.ConvertOGGsToWAVsOnInstallCheckBox.Checked = InConfig.ConvertOGGsToWAVsOnInstall;

            #endregion

            #endregion

            #region Other page

            #region Uninstalling FMs

            OtherPage.ConfirmUninstallCheckBox.Checked = InConfig.ConfirmUninstall;

            switch (InConfig.BackupFMData)
            {
                case BackupFMData.SavesAndScreensOnly:
                    OtherPage.BackupSavesAndScreensOnlyRadioButton.Checked = true;
                    break;
                case BackupFMData.AllChangedFiles:
                    OtherPage.BackupAllChangedDataRadioButton.Checked = true;
                    break;
            }

            OtherPage.BackupAlwaysAskCheckBox.Checked = InConfig.BackupAlwaysAsk;

            #endregion

            #region Languages

            using (new DisableEvents(this))
            {
                foreach (var item in InConfig.LanguageNames)
                {
                    OtherPage.LanguageComboBox.BackingItems.Add(item.Key);
                    OtherPage.LanguageComboBox.Items.Add(item.Value);
                }

                const string engLang = "English";

                if (OtherPage.LanguageComboBox.BackingItems.ContainsI(engLang))
                {
                    OtherPage.LanguageComboBox.BackingItems.Remove(engLang);
                    OtherPage.LanguageComboBox.BackingItems.Insert(0, engLang);
                    OtherPage.LanguageComboBox.Items.Remove(engLang);
                    OtherPage.LanguageComboBox.Items.Insert(0, engLang);
                }
                else
                {
                    OtherPage.LanguageComboBox.BackingItems.Insert(0, engLang);
                    OtherPage.LanguageComboBox.Items.Insert(0, engLang);
                }

                OtherPage.LanguageComboBox.SelectBackingIndexOf(OtherPage.LanguageComboBox.BackingItems.Contains(InConfig.Language)
                    ? InConfig.Language
                    : engLang);
            }

            #endregion

            OtherPage.WebSearchUrlTextBox.Text = InConfig.WebSearchUrl;

            OtherPage.ConfirmPlayOnDCOrEnterCheckBox.Checked = InConfig.ConfirmPlayOnDCOrEnter;

            #region Show/hide UI elements

            OtherPage.HideUninstallButtonCheckBox.Checked = InConfig.HideUninstallButton;
            OtherPage.HideFMListZoomButtonsCheckBox.Checked = InConfig.HideFMListZoomButtons;

            #endregion

            #endregion

            SetUITextToLocalized();
        }

        public void SetUITextToLocalized(bool suspendResume = true)
        {
            if (suspendResume) this.SuspendDrawing();
            try
            {
                OKButton.SetTextAutoSize(LText.Global.OK, ((Size)OKButton.Tag).Width);
                Cancel_Button.SetTextAutoSize(LText.Global.Cancel, ((Size)Cancel_Button.Tag).Width);

                #region Paths tab

                PagesListBox.Items[(int)PageIndex.Paths] = Startup
                    ? LText.SettingsWindow.InitialSettings_TabText
                    : LText.SettingsWindow.Paths_TabText;

                PathsPage.PathsToGameExesGroupBox.Text = LText.SettingsWindow.Paths_PathsToGameExes;
                PathsPage.Thief1ExePathLabel.Text = LText.SettingsWindow.Paths_Thief1;
                PathsPage.Thief2ExePathLabel.Text = LText.SettingsWindow.Paths_Thief2;
                PathsPage.Thief3ExePathLabel.Text = LText.SettingsWindow.Paths_Thief3;

                PathsPage.OtherGroupBox.Text = LText.SettingsWindow.Paths_Other;
                PathsPage.BackupPathLabel.Text = LText.SettingsWindow.Paths_BackupPath;

                // Manual "flow layout" for textbox/browse button combos
                for (int i = 0; i < 4; i++)
                {
                    var button =
                        i == 0 ? PathsPage.Thief1ExePathBrowseButton :
                        i == 1 ? PathsPage.Thief2ExePathBrowseButton :
                        i == 2 ? PathsPage.Thief3ExePathBrowseButton :
                        PathsPage.BackupPathBrowseButton;

                    var textBox =
                        i == 0 ? PathsPage.Thief1ExePathTextBox :
                        i == 1 ? PathsPage.Thief2ExePathTextBox :
                        i == 2 ? PathsPage.Thief3ExePathTextBox :
                        PathsPage.BackupPathTextBox;

                    button.SetTextAutoSize(textBox, LText.Global.BrowseEllipses);
                }

                PathsPage.GameRequirementsLabel.Text =
                    LText.SettingsWindow.Paths_Thief1AndThief2RequireNewDark + "\r\n" +
                    LText.SettingsWindow.Paths_Thief3RequiresSneakyUpgrade;

                PathsPage.FMArchivePathsGroupBox.Text = LText.SettingsWindow.Paths_FMArchivePaths;
                PathsPage.IncludeSubfoldersCheckBox.Text = LText.SettingsWindow.Paths_IncludeSubfolders;
                MainToolTip.SetToolTip(PathsPage.AddFMArchivePathButton, LText.SettingsWindow.Paths_AddArchivePathToolTip);
                MainToolTip.SetToolTip(PathsPage.RemoveFMArchivePathButton, LText.SettingsWindow.Paths_RemoveArchivePathToolTip);

                #endregion

                #region FM Display tab

                PagesListBox.Items[(int)PageIndex.FMDisplay] = LText.SettingsWindow.FMDisplay_TabText;

                FMDisplayPage.GameOrganizationGroupBox.Text = LText.SettingsWindow.FMDisplay_GameOrganization;
                FMDisplayPage.OrganizeGamesByTabRadioButton.Text = LText.SettingsWindow.FMDisplay_GameOrganizationByTab;
                FMDisplayPage.SortGamesInOneListRadioButton.Text = LText.SettingsWindow.FMDisplay_GameOrganizationOneList;

                FMDisplayPage.SortingGroupBox.Text = LText.SettingsWindow.FMDisplay_Sorting;
                FMDisplayPage.EnableIgnoreArticlesCheckBox.Text = LText.SettingsWindow.FMDisplay_IgnoreArticles;
                FMDisplayPage.MoveArticlesToEndCheckBox.Text = LText.SettingsWindow.FMDisplay_MoveArticlesToEnd;

                FMDisplayPage.RatingDisplayStyleGroupBox.Text = LText.SettingsWindow.FMDisplay_RatingDisplayStyle;
                FMDisplayPage.RatingNDLDisplayStyleRadioButton.Text = LText.SettingsWindow.FMDisplay_RatingDisplayStyleNDL;
                FMDisplayPage.RatingFMSelDisplayStyleRadioButton.Text = LText.SettingsWindow.FMDisplay_RatingDisplayStyleFMSel;
                FMDisplayPage.RatingUseStarsCheckBox.Text = LText.SettingsWindow.FMDisplay_RatingDisplayStyleUseStars;

                FMDisplayPage.DateFormatGroupBox.Text = LText.SettingsWindow.FMDisplay_DateFormat;
                FMDisplayPage.DateCurrentCultureShortRadioButton.Text = LText.SettingsWindow.FMDisplay_CurrentCultureShort;
                FMDisplayPage.DateCurrentCultureLongRadioButton.Text = LText.SettingsWindow.FMDisplay_CurrentCultureLong;
                FMDisplayPage.DateCustomRadioButton.Text = LText.SettingsWindow.FMDisplay_Custom;

                #endregion

                #region Other tab

                PagesListBox.Items[(int)PageIndex.Other] = LText.SettingsWindow.Other_TabText;

                OtherPage.FMFileConversionGroupBox.Text = LText.SettingsWindow.Other_FMFileConversion;
                OtherPage.ConvertWAVsTo16BitOnInstallCheckBox.Text = LText.SettingsWindow.Other_ConvertWAVsTo16BitOnInstall;
                OtherPage.ConvertOGGsToWAVsOnInstallCheckBox.Text = LText.SettingsWindow.Other_ConvertOGGsToWAVsOnInstall;

                OtherPage.UninstallingFMsGroupBox.Text = LText.SettingsWindow.Other_UninstallingFMs;
                OtherPage.ConfirmUninstallCheckBox.Text = LText.SettingsWindow.Other_ConfirmBeforeUninstalling;
                OtherPage.WhatToBackUpLabel.Text = LText.SettingsWindow.Other_WhenUninstallingBackUp;
                OtherPage.BackupSavesAndScreensOnlyRadioButton.Text = LText.SettingsWindow.Other_BackUpSavesAndScreenshotsOnly;
                OtherPage.BackupAllChangedDataRadioButton.Text = LText.SettingsWindow.Other_BackUpAllChangedFiles;
                OtherPage.BackupAlwaysAskCheckBox.Text = LText.SettingsWindow.Other_BackUpAlwaysAsk;

                OtherPage.LanguageGroupBox.Text = LText.SettingsWindow.Other_Language;

                OtherPage.WebSearchGroupBox.Text = LText.SettingsWindow.Other_WebSearch;
                OtherPage.WebSearchUrlLabel.Text = LText.SettingsWindow.Other_WebSearchURL;
                OtherPage.WebSearchTitleExplanationLabel.Text = LText.SettingsWindow.Other_WebSearchTitleVar;
                MainToolTip.SetToolTip(OtherPage.WebSearchUrlResetButton, LText.SettingsWindow.Other_WebSearchResetToolTip);

                OtherPage.PlayFMOnDCOrEnterGroupBox.Text = LText.SettingsWindow.Other_ConfirmPlayOnDCOrEnter;
                OtherPage.ConfirmPlayOnDCOrEnterCheckBox.Text = LText.SettingsWindow.Other_ConfirmPlayOnDCOrEnter_Ask;

                OtherPage.ShowOrHideUIElementsGroupBox.Text = LText.SettingsWindow.Other_ShowOrHideInterfaceElements;
                OtherPage.HideUninstallButtonCheckBox.Text = LText.SettingsWindow.Other_HideUninstallButton;
                OtherPage.HideFMListZoomButtonsCheckBox.Text = LText.SettingsWindow.Other_HideFMListZoomButtons;

                #endregion
            }
            finally
            {
                if (suspendResume) this.ResumeDrawing();
            }
        }

        private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Special case: these are meta, so they should always be set even if the user clicked Cancel
            OutConfig.SettingsTab =
                PagesListBox.SelectedIndex == (int)PageIndex.FMDisplay ? SettingsTab.FMDisplay :
                PagesListBox.SelectedIndex == (int)PageIndex.Other ? SettingsTab.Other :
                SettingsTab.Paths;
            OutConfig.SettingsWindowSize = Size;
            OutConfig.SettingsWindowSplitterDistance = MainSplitContainer.SplitterDistance;

            if (DialogResult != DialogResult.OK)
            {
                if (!Startup)
                {
                    try
                    {
                        if (!OtherPage.LanguageComboBox.SelectedBackingItem().EqualsI(InConfig.Language))
                        {
                            Ini.Ini.ReadLocalizationIni(Path.Combine(Paths.Languages, InConfig.Language + ".ini"));
                        }
                    }
                    catch (Exception ex)
                    {
                        Log("Exception in language reading", ex);
                        return;
                    }

                    try
                    {
                        OwnerForm.SetUITextToLocalized();
                    }
                    catch (Exception ex)
                    {
                        Log("OwnerForm might be uninitialized or somethin' again - not supposed to happen", ex);
                    }
                }
                return;
            }

            FormatArticles();

            #region Checks

            bool error = false;

            // TODO: Run a similar thing to Model.CheckPaths() to check for cam_mod.ini etc. to be thorough

            foreach (var tb in GameExePathTextBoxes)
            {
                if (!tb.Text.IsWhiteSpace() && !File.Exists(tb.Text))
                {
                    error = true;
                    ShowPathError(tb, true);
                }
            }

            if (!Directory.Exists(PathsPage.BackupPathTextBox.Text))
            {
                error = true;
                ShowPathError(PathsPage.BackupPathTextBox, true);
            }

            if (error)
            {
                e.Cancel = true;
                PagesListBox.SelectedIndex = (int)PageIndex.Paths;
                return;
            }
            else
            {
                foreach (var tb in GameExePathTextBoxes)
                {
                    tb.BackColor = SystemColors.Window;
                    tb.Tag = PathError.False;
                }
                PathsPage.BackupPathTextBox.BackColor = SystemColors.Window;
                PathsPage.BackupPathTextBox.Tag = PathError.False;
                ErrorLabel.Hide();

                // Extremely petty visual nicety - makes the error stuff go away before the form closes
                Refresh();
            }

            #endregion

            #region Paths page

            OutConfig.T1Exe = PathsPage.Thief1ExePathTextBox.Text.Trim();
            OutConfig.T2Exe = PathsPage.Thief2ExePathTextBox.Text.Trim();
            OutConfig.T3Exe = PathsPage.Thief3ExePathTextBox.Text.Trim();

            OutConfig.FMsBackupPath = PathsPage.BackupPathTextBox.Text.Trim();

            // Manual so we can use Trim() on each
            OutConfig.FMArchivePaths.Clear();
            foreach (string path in PathsPage.FMArchivePathsListBox.Items) OutConfig.FMArchivePaths.Add(path.Trim());

            OutConfig.FMArchivePathsIncludeSubfolders = PathsPage.IncludeSubfoldersCheckBox.Checked;

            #endregion

            #region FM Display page

            #region Game organization

            OutConfig.GameOrganization = FMDisplayPage.OrganizeGamesByTabRadioButton.Checked
                    ? GameOrganization.ByTab
                    : GameOrganization.OneList;

            #endregion

            #region Articles

            OutConfig.EnableArticles = FMDisplayPage.EnableIgnoreArticlesCheckBox.Checked;

            var retArticles = FMDisplayPage.ArticlesTextBox.Text
                .Replace(", ", ",")
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Distinct(StringComparer.InvariantCultureIgnoreCase)
                .ToList();

            // Just in case
            for (var i = 0; i < retArticles.Count; i++)
            {
                if (retArticles[i].IsWhiteSpace())
                {
                    retArticles.RemoveAt(i);
                    i--;
                }
            }

            OutConfig.Articles.ClearAndAdd(retArticles);

            OutConfig.MoveArticlesToEnd = FMDisplayPage.MoveArticlesToEndCheckBox.Checked;

            #endregion

            #region Date format

            OutConfig.DateFormat =
                FMDisplayPage.DateCurrentCultureShortRadioButton.Checked ? DateFormat.CurrentCultureShort :
                FMDisplayPage.DateCurrentCultureLongRadioButton.Checked ? DateFormat.CurrentCultureLong :
                DateFormat.Custom;

            OutConfig.DateCustomFormat1 = FMDisplayPage.Date1ComboBox.SelectedItem.ToString();
            OutConfig.DateCustomSeparator1 = FMDisplayPage.DateSeparator1TextBox.Text;
            OutConfig.DateCustomFormat2 = FMDisplayPage.Date2ComboBox.SelectedItem.ToString();
            OutConfig.DateCustomSeparator2 = FMDisplayPage.DateSeparator2TextBox.Text;
            OutConfig.DateCustomFormat3 = FMDisplayPage.Date3ComboBox.SelectedItem.ToString();
            OutConfig.DateCustomSeparator3 = FMDisplayPage.DateSeparator3TextBox.Text;
            OutConfig.DateCustomFormat4 = FMDisplayPage.Date4ComboBox.SelectedItem.ToString();

            var formatString = FMDisplayPage.Date1ComboBox.SelectedItem +
                               FMDisplayPage.DateSeparator1TextBox.Text.EscapeAllChars() +
                               FMDisplayPage.Date2ComboBox.SelectedItem +
                               FMDisplayPage.DateSeparator2TextBox.Text.EscapeAllChars() +
                               FMDisplayPage.Date3ComboBox.SelectedItem +
                               FMDisplayPage.DateSeparator3TextBox.Text.EscapeAllChars() +
                               FMDisplayPage.Date4ComboBox.SelectedItem;

            try
            {
                _ = exampleDate.ToString(formatString);
                OutConfig.DateCustomFormatString = formatString;
            }
            catch (FormatException)
            {
                MessageBox.Show(LText.SettingsWindow.FMDisplay_ErrorInvalidDateFormat, LText.AlertMessages.Error,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = true;
                return;
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show(LText.SettingsWindow.FMDisplay_ErrorDateOutOfRange, LText.AlertMessages.Error,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = true;
                return;
            }

            #endregion

            #region Rating display style

            OutConfig.RatingDisplayStyle = FMDisplayPage.RatingNDLDisplayStyleRadioButton.Checked
                ? RatingDisplayStyle.NewDarkLoader
                : RatingDisplayStyle.FMSel;
            OutConfig.RatingUseStars = FMDisplayPage.RatingUseStarsCheckBox.Checked;

            #endregion

            #endregion

            #region Other page

            #region File conversion

            OutConfig.ConvertWAVsTo16BitOnInstall = OtherPage.ConvertWAVsTo16BitOnInstallCheckBox.Checked;
            OutConfig.ConvertOGGsToWAVsOnInstall = OtherPage.ConvertOGGsToWAVsOnInstallCheckBox.Checked;

            #endregion

            #region Uninstalling FMs

            OutConfig.ConfirmUninstall = OtherPage.ConfirmUninstallCheckBox.Checked;

            OutConfig.BackupFMData = OtherPage.BackupSavesAndScreensOnlyRadioButton.Checked
                ? BackupFMData.SavesAndScreensOnly
                : BackupFMData.AllChangedFiles;

            OutConfig.BackupAlwaysAsk = OtherPage.BackupAlwaysAskCheckBox.Checked;

            #endregion

            OutConfig.Language = OtherPage.LanguageComboBox.SelectedBackingItem();

            OutConfig.WebSearchUrl = OtherPage.WebSearchUrlTextBox.Text;

            OutConfig.ConfirmPlayOnDCOrEnter = OtherPage.ConfirmPlayOnDCOrEnterCheckBox.Checked;

            #region Show/hide UI elements

            OutConfig.HideUninstallButton = OtherPage.HideUninstallButtonCheckBox.Checked;
            OutConfig.HideFMListZoomButtons = OtherPage.HideFMListZoomButtonsCheckBox.Checked;

            #endregion

            #endregion
        }

        #region Page selection handler

        private void PagesListBox_SelectedIndexChanged(object sender, EventArgs e) => ShowPage(PagesListBox.SelectedIndex);

        // This is to allow for selection to change immediately on mousedown. In that case the event will fire
        // again when you let up the mouse, but that's okay because a re-select is a visual no-op and the work
        // is basically nothing.
        private void PagesListBox_MouseDown(object sender, MouseEventArgs e) => ShowPage(PagesListBox.IndexFromPoint(e.Location));

        private void ShowPage(int index)
        {
            int pagesLength = Pages.Length;
            if (index < 0 || index > pagesLength - 1) return;

            Pages[index].Show();
            for (int i = 0; i < pagesLength; i++) if (i != index) Pages[i].Hide();
        }

        #endregion

        #region Paths page

        #region Game exe paths

        private void GameExePathTextBoxes_Leave(object sender, EventArgs e)
        {
            var s = (TextBox)sender;
            ShowPathError(s, !s.Text.IsEmpty() && !File.Exists(s.Text));
        }

        private void GameExePathBrowseButtons_Click(object sender, EventArgs e)
        {
            var tb =
                sender == PathsPage.Thief1ExePathBrowseButton ? PathsPage.Thief1ExePathTextBox :
                sender == PathsPage.Thief2ExePathBrowseButton ? PathsPage.Thief2ExePathTextBox :
                PathsPage.Thief3ExePathTextBox;

            string initialPath = "";
            try
            {
                initialPath = Path.GetDirectoryName(tb.Text);
            }
            catch
            {
                // ignore
            }

            var (result, fileName) = BrowseForExeFile(initialPath);
            if (result == DialogResult.OK) tb.Text = fileName ?? "";
        }

        private void BackupPathTextBox_Leave(object sender, EventArgs e)
        {
            var s = (TextBox)sender;
            ShowPathError(s, !Directory.Exists(s.Text));
        }

        private void BackupPathBrowseButton_Click(object sender, EventArgs e)
        {
            using (var d = new AutoFolderBrowserDialog())
            {
                d.InitialDirectory = PathsPage.BackupPathTextBox.Text;
                d.MultiSelect = false;
                if (d.ShowDialog() == DialogResult.OK) PathsPage.BackupPathTextBox.Text = d.DirectoryName;
            }
        }

        private static (DialogResult Result, string FileName)
        BrowseForExeFile(string initialPath)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.InitialDirectory = initialPath;
                dialog.Filter = LText.BrowseDialogs.ExeFiles + @"|*.exe";
                return (dialog.ShowDialog(), dialog.FileName);
            }
        }

        #endregion

        #region Archive paths

        private bool FMArchivePathExistsInBox(string path)
        {
            foreach (var item in PathsPage.FMArchivePathsListBox.Items)
            {
                if (item.ToString().EqualsI(path)) return true;
            }

            return false;
        }

        private void AddFMArchivePathButton_Click(object sender, EventArgs e)
        {
            using (var d = new AutoFolderBrowserDialog())
            {
                var lb = PathsPage.FMArchivePathsListBox;
                var initDir =
                    lb.SelectedIndex > -1 ? lb.SelectedItem.ToString() :
                    lb.Items.Count > 0 ? lb.Items[lb.Items.Count - 1].ToString() :
                    "";
                if (!initDir.IsWhiteSpace())
                {
                    try
                    {
                        d.InitialDirectory = Path.GetDirectoryName(initDir);
                    }
                    catch
                    {
                        // ignore
                    }
                }
                d.MultiSelect = true;
                if (d.ShowDialog() == DialogResult.OK)
                {
                    foreach (var dir in d.DirectoryNames)
                    {
                        if (!FMArchivePathExistsInBox(dir)) PathsPage.FMArchivePathsListBox.Items.Add(dir);
                    }
                }
            }
        }

        private void RemoveFMArchivePathButton_Click(object sender, EventArgs e) => PathsPage.FMArchivePathsListBox.RemoveAndSelectNearest();

        #endregion

        #endregion

        #region FM Display page

        #region Articles

        private void ArticlesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            FMDisplayPage.ArticlesTextBox.Enabled = FMDisplayPage.EnableIgnoreArticlesCheckBox.Checked;
            FMDisplayPage.MoveArticlesToEndCheckBox.Enabled = FMDisplayPage.EnableIgnoreArticlesCheckBox.Checked;
        }

        private void ArticlesTextBox_Leave(object sender, EventArgs e) => FormatArticles();

        private void FormatArticles()
        {
            var articles = FMDisplayPage.ArticlesTextBox.Text;

            // Copied wholesale from Autovid, ridiculous looking, but works

            if (articles.IsWhiteSpace())
            {
                FMDisplayPage.ArticlesTextBox.Text = "";
                return;
            }

            // Remove duplicate consecutive spaces
            articles = Regex.Replace(articles, @"\s{2,}", " ");

            // Remove spaces surrounding commas
            articles = Regex.Replace(articles, @"\s?\,\s?", ",");

            // Remove duplicate consecutive commas
            articles = Regex.Replace(articles, @"\,{2,}", ",");

            // Remove commas from start and end
            articles = articles.Trim(',');

            var articlesArray = articles.Split(',', ' ').Distinct(StringComparer.InvariantCultureIgnoreCase).ToArray();

            articles = "";
            for (var i = 0; i < articlesArray.Length; i++)
            {
                if (i > 0) articles += ", ";
                articles += articlesArray[i];
            }

            FMDisplayPage.ArticlesTextBox.Text = articles;
        }

        #endregion

        #region Date display

        private void UpdateExampleDate()
        {

            var formatString = FMDisplayPage.Date1ComboBox.SelectedItem +
                               FMDisplayPage.DateSeparator1TextBox.Text.EscapeAllChars() +
                               FMDisplayPage.Date2ComboBox.SelectedItem +
                               FMDisplayPage.DateSeparator2TextBox.Text.EscapeAllChars() +
                               FMDisplayPage.Date3ComboBox.SelectedItem +
                               FMDisplayPage.DateSeparator3TextBox.Text.EscapeAllChars() +
                               FMDisplayPage.Date4ComboBox.SelectedItem;

            try
            {
                var date = exampleDate.ToString(formatString);
                FMDisplayPage.PreviewDateLabel.Text = date;
            }
            catch (FormatException)
            {
                MessageBox.Show(LText.SettingsWindow.FMDisplay_ErrorInvalidDateFormat, LText.AlertMessages.Error,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show(LText.SettingsWindow.FMDisplay_ErrorDateOutOfRange, LText.AlertMessages.Error,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DateCurrentCultureShortRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            FMDisplayPage.DateCustomFormatPanel.Enabled = false;
            FMDisplayPage.PreviewDateLabel.Text = exampleDate.ToShortDateString();
        }

        private void DateCurrentCultureLongRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            FMDisplayPage.DateCustomFormatPanel.Enabled = false;
            FMDisplayPage.PreviewDateLabel.Text = exampleDate.ToLongDateString();
        }

        private void DateCustomRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            var s = (RadioButton)sender;
            FMDisplayPage.DateCustomFormatPanel.Enabled = s.Checked;

            if (s.Checked) UpdateExampleDate();
        }

        private void DateComboBoxes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (FMDisplayPage.DateCustomFormatPanel.Enabled) UpdateExampleDate();
        }

        private void DateSeparatorTextBoxes_TextChanged(object sender, EventArgs e)
        {
            if (FMDisplayPage.DateCustomFormatPanel.Enabled) UpdateExampleDate();
        }

        #endregion

        #region Rating display

        private void RatingOutOfTenRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            if (FMDisplayPage.RatingNDLDisplayStyleRadioButton.Checked)
            {
                FMDisplayPage.RatingUseStarsCheckBox.Enabled = false;
                FMDisplayPage.RatingExamplePictureBox.Image = Resources.RatingExample_NDL;
            }
        }

        private void RatingOutOfFiveRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            if (FMDisplayPage.RatingFMSelDisplayStyleRadioButton.Checked)
            {
                FMDisplayPage.RatingUseStarsCheckBox.Enabled = true;
                FMDisplayPage.RatingExamplePictureBox.Image = FMDisplayPage.RatingUseStarsCheckBox.Checked
                    ? Resources.RatingExample_FMSel_Stars
                    : Resources.RatingExample_FMSel_Number;
            }
        }

        private void RatingUseStarsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            FMDisplayPage.RatingExamplePictureBox.Image = FMDisplayPage.RatingUseStarsCheckBox.Checked
                ? Resources.RatingExample_FMSel_Stars
                : Resources.RatingExample_FMSel_Number;
        }

        #endregion

        #endregion

        #region Other page

        private void WebSearchURLResetButton_Click(object sender, EventArgs e) => OtherPage.WebSearchUrlTextBox.Text = Defaults.WebSearchUrl;

        private void LanguageComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            var s = OtherPage.LanguageComboBox;
            try
            {
                Ini.Ini.ReadLocalizationIni(Path.Combine(Paths.Languages, s.SelectedBackingItem() + ".ini"));
                SetUITextToLocalized();
                if (!Startup)
                {
                    try
                    {
                        OwnerForm.SetUITextToLocalized();
                    }
                    catch (Exception ex)
                    {
                        Log("OwnerForm might be uninitialized or somethin' again - not supposed to happen", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Exception in language reading", ex);
            }
        }

        #endregion

        private void ShowPathError(TextBox textBox, bool shown)
        {
            if (shown)
            {
                if (textBox != null)
                {
                    textBox.BackColor = Color.MistyRose;
                    textBox.Tag = PathError.True;
                }
                ErrorLabel.Text = LText.SettingsWindow.Paths_ErrorSomePathsAreInvalid;
                ErrorLabel.Show();
            }
            else
            {
                if (textBox != null)
                {
                    textBox.BackColor = SystemColors.Window;
                    textBox.Tag = PathError.False;
                }

                bool errorsRemaining = PathsPage.BackupPathTextBox.Tag is PathError bError && bError == PathError.True;
                foreach (var tb in GameExePathTextBoxes)
                {
                    if (tb.Tag is PathError gError && gError == PathError.True) errorsRemaining = true;
                }
                if (errorsRemaining) return;

                ErrorLabel.Text = "";
                ErrorLabel.Hide();
            }
        }
    }
}
