﻿using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Properties;
using static AngelLoader.GameSupport;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls
{
    public sealed partial class DataGridViewCustom
    {
        #region Backing fields

        private bool _fmMenuConstructed;
        private bool _installUninstallMenuEnabled;
        private bool _sayInstall;
        private bool _sayShockEd;
        private bool _playFMMenuItemEnabled;
        private bool _scanFMMenuItemEnabled;
        private bool _openInDromEdSepVisible;
        private bool _openInDromEdMenuItemVisible;
        private bool _playFMInMPMenuItemVisible;
        private bool _convertAudioSubMenuEnabled;
        private bool _deleteFMMenuItemEnabled;
        private int _rating = -1;
        private bool _finishedOnNormalChecked;
        private bool _finishedOnHardChecked;
        private bool _finishedOnExpertChecked;
        private bool _finishedOnExtremeChecked;
        private bool _finishedOnUnknownChecked;

        #endregion

        private IDisposable[]? FMContextMenuDisposables;

        #region FM context menu fields

#pragma warning disable IDE0069 // Disposable fields should be disposed

        // These are disposed by adding them to an array and iterating through it in Dispose()
        // TODO: This probably doesn't even need to happen, as they prolly get dumped with everything else on app exit

        private ContextMenuStrip? FMContextMenu;

        private ToolStripMenuItem? PlayFMMenuItem;
        private ToolStripMenuItem? PlayFMInMPMenuItem;
        private ToolStripMenuItem? PlayFMAdvancedMenuItem;
        private ToolStripMenuItem? InstallUninstallMenuItem;

        private ToolStripSeparator? DeleteFMSep;

        private ToolStripMenuItem? DeleteFMMenuItem;

        private ToolStripSeparator? OpenInDromEdSep;

        private ToolStripMenuItem? OpenInDromEdMenuItem;

        private ToolStripSeparator? FMContextMenuSep1;

        private ToolStripMenuItem? ScanFMMenuItem;
        private ToolStripMenuItem? ConvertAudioMenuItem;
        private ToolStripMenuItem? ConvertWAVsTo16BitMenuItem;
        private ToolStripMenuItem? ConvertOGGsToWAVsMenuItem;

        private ToolStripSeparator? FMContextMenuSep2;

        private ToolStripMenuItem? RatingMenuItem;
        private ToolStripMenuItem? RatingMenuUnrated;
        private ToolStripMenuItem? Rating0MenuItem;
        private ToolStripMenuItem? Rating1MenuItem;
        private ToolStripMenuItem? Rating2MenuItem;
        private ToolStripMenuItem? Rating3MenuItem;
        private ToolStripMenuItem? Rating4MenuItem;
        private ToolStripMenuItem? Rating5MenuItem;
        private ToolStripMenuItem? Rating6MenuItem;
        private ToolStripMenuItem? Rating7MenuItem;
        private ToolStripMenuItem? Rating8MenuItem;
        private ToolStripMenuItem? Rating9MenuItem;
        private ToolStripMenuItem? Rating10MenuItem;

        private ToolStripMenuItem? FinishedOnMenuItem;
        private ContextMenuStripCustom? FinishedOnMenu;
        private ToolStripMenuItem? FinishedOnNormalMenuItem;
        private ToolStripMenuItem? FinishedOnHardMenuItem;
        private ToolStripMenuItem? FinishedOnExpertMenuItem;
        private ToolStripMenuItem? FinishedOnExtremeMenuItem;
        private ToolStripMenuItem? FinishedOnUnknownMenuItem;

        private ToolStripSeparator? FMContextMenuSep3;

        private ToolStripMenuItem? WebSearchMenuItem;

#pragma warning restore IDE0069 // Disposable fields should be disposed

        #endregion

        #region Private methods

        private void ConstructFMContextMenu()
        {
            if (_fmMenuConstructed) return;

            #region Instantiation

            FMContextMenuDisposables = new IDisposable[]
            {
                FMContextMenu = new ContextMenuStrip { Name = nameof(FMContextMenu) },
                PlayFMMenuItem = new ToolStripMenuItem { Name = nameof(PlayFMMenuItem) },
                PlayFMInMPMenuItem = new ToolStripMenuItem { Name = nameof(PlayFMInMPMenuItem) },
                PlayFMAdvancedMenuItem = new ToolStripMenuItem { Name = nameof(PlayFMAdvancedMenuItem) },

                InstallUninstallMenuItem = new ToolStripMenuItem { Name = nameof(InstallUninstallMenuItem) },

                DeleteFMSep = new ToolStripSeparator { Name = nameof(DeleteFMSep) },
                DeleteFMMenuItem = new ToolStripMenuItem { Name = nameof(DeleteFMMenuItem), Image = Resources.Trash_16 },

                OpenInDromEdSep = new ToolStripSeparator { Name = nameof(OpenInDromEdSep) },
                OpenInDromEdMenuItem = new ToolStripMenuItem { Name = nameof(OpenInDromEdMenuItem) },

                FMContextMenuSep1 = new ToolStripSeparator { Name = nameof(FMContextMenuSep1) },

                ScanFMMenuItem = new ToolStripMenuItem { Name = nameof(ScanFMMenuItem) },

                ConvertAudioMenuItem = new ToolStripMenuItem { Name = nameof(ConvertAudioMenuItem) },
                ConvertWAVsTo16BitMenuItem = new ToolStripMenuItem { Name = nameof(ConvertWAVsTo16BitMenuItem) },
                ConvertOGGsToWAVsMenuItem = new ToolStripMenuItem { Name = nameof(ConvertOGGsToWAVsMenuItem) },

                FMContextMenuSep2 = new ToolStripSeparator { Name = nameof(FMContextMenuSep2) },

                RatingMenuItem = new ToolStripMenuItem { Name = nameof(RatingMenuItem) },
                RatingMenuUnrated = new ToolStripMenuItem { Name = nameof(RatingMenuUnrated), CheckOnClick = true },
                Rating0MenuItem = new ToolStripMenuItem { Name = nameof(Rating0MenuItem), CheckOnClick = true },
                Rating1MenuItem = new ToolStripMenuItem { Name = nameof(Rating1MenuItem), CheckOnClick = true },
                Rating2MenuItem = new ToolStripMenuItem { Name = nameof(Rating2MenuItem), CheckOnClick = true },
                Rating3MenuItem = new ToolStripMenuItem { Name = nameof(Rating3MenuItem), CheckOnClick = true },
                Rating4MenuItem = new ToolStripMenuItem { Name = nameof(Rating4MenuItem), CheckOnClick = true },
                Rating5MenuItem = new ToolStripMenuItem { Name = nameof(Rating5MenuItem), CheckOnClick = true },
                Rating6MenuItem = new ToolStripMenuItem { Name = nameof(Rating6MenuItem), CheckOnClick = true },
                Rating7MenuItem = new ToolStripMenuItem { Name = nameof(Rating7MenuItem), CheckOnClick = true },
                Rating8MenuItem = new ToolStripMenuItem { Name = nameof(Rating8MenuItem), CheckOnClick = true },
                Rating9MenuItem = new ToolStripMenuItem { Name = nameof(Rating9MenuItem), CheckOnClick = true },
                Rating10MenuItem = new ToolStripMenuItem { Name = nameof(Rating10MenuItem), CheckOnClick = true },

                FinishedOnMenuItem = new ToolStripMenuItem { Name = nameof(FinishedOnMenuItem) },
                FinishedOnMenu = new ContextMenuStripCustom { Name = nameof(FinishedOnMenu) },
                FinishedOnNormalMenuItem = new ToolStripMenuItem { Name = nameof(FinishedOnNormalMenuItem), CheckOnClick = true },
                FinishedOnHardMenuItem = new ToolStripMenuItem { Name = nameof(FinishedOnHardMenuItem), CheckOnClick = true },
                FinishedOnExpertMenuItem = new ToolStripMenuItem { Name = nameof(FinishedOnExpertMenuItem), CheckOnClick = true },
                FinishedOnExtremeMenuItem = new ToolStripMenuItem { Name = nameof(FinishedOnExtremeMenuItem), CheckOnClick = true },
                FinishedOnUnknownMenuItem = new ToolStripMenuItem { Name = nameof(FinishedOnUnknownMenuItem), CheckOnClick = true },

                FMContextMenuSep3 = new ToolStripSeparator { Name = nameof(FMContextMenuSep3) },

                WebSearchMenuItem = new ToolStripMenuItem { Name = nameof(WebSearchMenuItem) }
            };
            #endregion

            #region Add items to menu

            FMContextMenu.Items.AddRange(new ToolStripItem[]
            {
                PlayFMMenuItem,
                PlayFMInMPMenuItem,
                //PlayFMAdvancedMenuItem,
                InstallUninstallMenuItem,
                DeleteFMSep,
                DeleteFMMenuItem,
                OpenInDromEdSep,
                OpenInDromEdMenuItem,
                FMContextMenuSep1,
                ScanFMMenuItem,
                ConvertAudioMenuItem,
                FMContextMenuSep2,
                RatingMenuItem,
                FinishedOnMenuItem,
                FMContextMenuSep3,
                WebSearchMenuItem
            });

            ConvertAudioMenuItem.DropDownItems.AddRange(new ToolStripItem[]
            {
                ConvertWAVsTo16BitMenuItem,
                ConvertOGGsToWAVsMenuItem
            });

            RatingMenuItem.DropDownItems.AddRange(new ToolStripItem[]
            {
                RatingMenuUnrated,
                Rating0MenuItem,
                Rating1MenuItem,
                Rating2MenuItem,
                Rating3MenuItem,
                Rating4MenuItem,
                Rating5MenuItem,
                Rating6MenuItem,
                Rating7MenuItem,
                Rating8MenuItem,
                Rating9MenuItem,
                Rating10MenuItem
            });

            FinishedOnMenu.Items.AddRange(new ToolStripItem[]
            {
                FinishedOnNormalMenuItem,
                FinishedOnHardMenuItem,
                FinishedOnExpertMenuItem,
                FinishedOnExtremeMenuItem,
                FinishedOnUnknownMenuItem
            });

            FinishedOnMenu.SetPreventCloseOnClickItems(FinishedOnMenu.Items.Cast<ToolStripMenuItem>().ToArray());
            FinishedOnMenuItem.DropDown = FinishedOnMenu;

            #endregion

            #region Event hookups

            FMContextMenu.Opening += FMContextMenu_Opening;
            PlayFMMenuItem.Click += PlayFMMenuItem_Click;
            PlayFMInMPMenuItem.Click += PlayFMInMPMenuItem_Click;
            InstallUninstallMenuItem.Click += InstallUninstallMenuItem_Click;
            DeleteFMMenuItem.Click += DeleteFMMenuItem_Click;
            OpenInDromEdMenuItem.Click += OpenInDromEdMenuItem_Click;
            ScanFMMenuItem.Click += ScanFMMenuItem_Click;
            ConvertWAVsTo16BitMenuItem.Click += ConvertWAVsTo16BitMenuItem_Click;
            ConvertOGGsToWAVsMenuItem.Click += ConvertOGGsToWAVsMenuItem_Click;

            foreach (ToolStripMenuItem item in RatingMenuItem.DropDownItems)
            {
                item.Click += RatingMenuItems_Click;
                item.CheckedChanged += RatingRCMenuItems_CheckedChanged;
            }

            RatingMenuUnrated.Click += RatingMenuItems_Click;
            Rating0MenuItem.Click += RatingMenuItems_Click;
            Rating1MenuItem.Click += RatingMenuItems_Click;
            Rating2MenuItem.Click += RatingMenuItems_Click;
            Rating3MenuItem.Click += RatingMenuItems_Click;
            Rating4MenuItem.Click += RatingMenuItems_Click;
            Rating5MenuItem.Click += RatingMenuItems_Click;
            Rating6MenuItem.Click += RatingMenuItems_Click;
            Rating7MenuItem.Click += RatingMenuItems_Click;
            Rating8MenuItem.Click += RatingMenuItems_Click;
            Rating9MenuItem.Click += RatingMenuItems_Click;
            Rating10MenuItem.Click += RatingMenuItems_Click;

            FinishedOnNormalMenuItem.Click += FinishedOnMenuItems_Click;
            FinishedOnHardMenuItem.Click += FinishedOnMenuItems_Click;
            FinishedOnExpertMenuItem.Click += FinishedOnMenuItems_Click;
            FinishedOnExtremeMenuItem.Click += FinishedOnMenuItems_Click;
            FinishedOnUnknownMenuItem.Click += FinishedOnMenuItems_Click;
            FinishedOnUnknownMenuItem.CheckedChanged += FinishedOnUnknownMenuItem_CheckedChanged;

            WebSearchMenuItem.Click += WebSearchMenuItem_Click;

            #endregion

            #region Set main menu item values

            InstallUninstallMenuItem.Enabled = _installUninstallMenuEnabled;
            DeleteFMMenuItem.Enabled = _deleteFMMenuItemEnabled;
            SetConcreteInstallUninstallMenuItemText(_sayInstall);
            SetConcreteDromEdMenuItemText(_sayShockEd);
            PlayFMMenuItem.Enabled = _playFMMenuItemEnabled;
            PlayFMInMPMenuItem.Visible = _playFMInMPMenuItemVisible;
            ScanFMMenuItem.Enabled = _scanFMMenuItemEnabled;
            OpenInDromEdSep.Visible = _openInDromEdSepVisible;
            OpenInDromEdMenuItem.Visible = _openInDromEdMenuItemVisible;
            ConvertAudioMenuItem.Enabled = _convertAudioSubMenuEnabled;

            #region Set Finished On checked values

            FinishedOnNormalMenuItem.Checked = _finishedOnNormalChecked;
            FinishedOnHardMenuItem.Checked = _finishedOnHardChecked;
            FinishedOnExpertMenuItem.Checked = _finishedOnExpertChecked;
            FinishedOnExtremeMenuItem.Checked = _finishedOnExtremeChecked;
            FinishedOnUnknownMenuItem.Checked = _finishedOnUnknownChecked;

            #endregion

            #endregion

            _fmMenuConstructed = true;

            UpdateRatingList(Config.RatingDisplayStyle == RatingDisplayStyle.FMSel);
            SetRatingMenuItemChecked(_rating);
            SetFMMenuTextToLocalized();
        }

        private void UncheckFinishedOnMenuItemsExceptUnknown()
        {
            if (_fmMenuConstructed)
            {
                FinishedOnNormalMenuItem!.Checked = false;
                FinishedOnHardMenuItem!.Checked = false;
                FinishedOnExpertMenuItem!.Checked = false;
                FinishedOnExtremeMenuItem!.Checked = false;
            }
            else
            {
                _finishedOnNormalChecked = false;
                _finishedOnHardChecked = false;
                _finishedOnExpertChecked = false;
                _finishedOnExtremeChecked = false;
            }
        }

        #endregion

        #region API methods

        private void SetFMMenuTextToLocalized()
        {
            if (!_fmMenuConstructed) return;

            #region Get current FM info

            // Some menu items' text depends on FM state. Because this could be run after startup, we need to
            // make sure those items' text is set correctly.
            FanMission? selFM = SelectedRows.Count > 0 ? GetSelectedFM() : null;
            bool sayInstall = selFM == null || !selFM.Installed;
            // @GENGAMES
            bool sayShockEd = selFM != null && selFM.Game == Game.SS2;

            #endregion

            #region Play

            PlayFMMenuItem!.Text = LText.FMsList.FMMenu_PlayFM.EscapeAmpersands();
            PlayFMInMPMenuItem!.Text = LText.FMsList.FMMenu_PlayFM_Multiplayer.EscapeAmpersands();

            //PlayFMAdvancedMenuItem.Text = LText.FMsList.FMMenu_PlayFMAdvanced.EscapeAmpersands();
            //Core.SetDefaultConfigVarNamesToLocalized();

            #endregion

            SetConcreteInstallUninstallMenuItemText(sayInstall);

            DeleteFMMenuItem!.Text = LText.FMsList.FMMenu_DeleteFM.EscapeAmpersands();

            SetConcreteDromEdMenuItemText(sayShockEd);

            ScanFMMenuItem!.Text = LText.FMsList.FMMenu_ScanFM.EscapeAmpersands();

            #region Convert audio submenu

            ConvertAudioMenuItem!.Text = LText.FMsList.FMMenu_ConvertAudio.EscapeAmpersands();
            ConvertWAVsTo16BitMenuItem!.Text = LText.FMsList.ConvertAudioMenu_ConvertWAVsTo16Bit.EscapeAmpersands();
            ConvertOGGsToWAVsMenuItem!.Text = LText.FMsList.ConvertAudioMenu_ConvertOGGsToWAVs.EscapeAmpersands();

            #endregion

            #region Rating submenu

            RatingMenuItem!.Text = LText.FMsList.FMMenu_Rating.EscapeAmpersands();
            RatingMenuUnrated!.Text = LText.Global.Unrated.EscapeAmpersands();

            #endregion

            #region Finished On submenu

            FinishedOnMenuItem!.Text = LText.FMsList.FMMenu_FinishedOn.EscapeAmpersands();

            FinishedOnNormalMenuItem!.Text = GetLocalizedDifficultyName(selFM, Difficulty.Normal).EscapeAmpersands();
            FinishedOnHardMenuItem!.Text = GetLocalizedDifficultyName(selFM, Difficulty.Hard).EscapeAmpersands();
            FinishedOnExpertMenuItem!.Text = GetLocalizedDifficultyName(selFM, Difficulty.Expert).EscapeAmpersands();
            FinishedOnExtremeMenuItem!.Text = GetLocalizedDifficultyName(selFM, Difficulty.Extreme).EscapeAmpersands();
            FinishedOnUnknownMenuItem!.Text = LText.Difficulties.Unknown.EscapeAmpersands();

            #endregion

            WebSearchMenuItem!.Text = LText.FMsList.FMMenu_WebSearch.EscapeAmpersands();
        }

        internal void UpdateRatingList(bool fmSelStyle)
        {
            if (!_fmMenuConstructed) return;

            for (int i = 0; i <= 10; i++)
            {
                string num = (fmSelStyle ? i / 2.0 : i).ToString(CultureInfo.CurrentCulture);
                RatingMenuItem!.DropDownItems[i + 1].Text = num;
            }
        }

        internal ContextMenuStrip GetFinishedOnMenu()
        {
            ConstructFMContextMenu();
            return FinishedOnMenu!;
        }

        internal void SetPlayFMMenuItemEnabled(bool value)
        {
            if (_fmMenuConstructed)
            {
                PlayFMMenuItem!.Enabled = value;
            }
            else
            {
                _playFMMenuItemEnabled = value;
            }
        }

        internal void SetPlayFMInMPMenuItemVisible(bool value)
        {
            if (_fmMenuConstructed)
            {
                PlayFMInMPMenuItem!.Visible = value;
            }
            else
            {
                _playFMInMPMenuItemVisible = value;
            }
        }

        internal void SetInstallUninstallMenuItemEnabled(bool value)
        {
            if (_fmMenuConstructed)
            {
                InstallUninstallMenuItem!.Enabled = value;
            }
            else
            {
                _installUninstallMenuEnabled = value;
            }
        }

        internal void SetInstallUninstallMenuItemText(bool sayInstall)
        {
            if (_fmMenuConstructed)
            {
                SetConcreteInstallUninstallMenuItemText(sayInstall);
            }
            else
            {
                _sayInstall = sayInstall;
            }
        }

        internal void SetDeleteFMMenuItemEnabled(bool value)
        {
            if (_fmMenuConstructed)
            {
                DeleteFMMenuItem!.Enabled = value;
            }
            else
            {
                _deleteFMMenuItemEnabled = value;
            }
        }

        internal void SetOpenInDromEdVisible(bool value)
        {
            if (_fmMenuConstructed)
            {
                OpenInDromEdSep!.Visible = value;
                OpenInDromEdMenuItem!.Visible = value;
            }
            else
            {
                _openInDromEdSepVisible = value;
                _openInDromEdMenuItemVisible = value;
            }
        }

        internal void SetOpenInDromEdMenuItemText(bool sayShockEd)
        {
            if (_fmMenuConstructed)
            {
                SetConcreteDromEdMenuItemText(sayShockEd);
            }
            else
            {
                _sayShockEd = sayShockEd;
            }
        }

        internal void SetScanFMMenuItemEnabled(bool value)
        {
            if (_fmMenuConstructed)
            {
                ScanFMMenuItem!.Enabled = value;
            }
            else
            {
                _scanFMMenuItemEnabled = value;
            }
        }

        internal void SetConvertAudioRCSubMenuEnabled(bool value)
        {
            if (_fmMenuConstructed)
            {
                ConvertAudioMenuItem!.Enabled = value;
            }
            else
            {
                _convertAudioSubMenuEnabled = value;
            }
        }

        internal void SetRatingMenuItemChecked(int value)
        {
            value = value.Clamp(-1, 10);

            if (_fmMenuConstructed)
            {
                ((ToolStripMenuItem)RatingMenuItem!.DropDownItems[value + 1]).Checked = true;
            }
            else
            {
                _rating = value;
            }
        }

        internal void SetFinishedOnMenuItemChecked(Difficulty difficulty, bool value)
        {
            if (value && !_fmMenuConstructed) _finishedOnUnknownChecked = false;

            switch (difficulty)
            {
                case Difficulty.Normal:
                    if (_fmMenuConstructed)
                    {
                        FinishedOnNormalMenuItem!.Checked = value;
                    }
                    else
                    {
                        _finishedOnNormalChecked = value;
                    }
                    break;
                case Difficulty.Hard:
                    if (_fmMenuConstructed)
                    {
                        FinishedOnHardMenuItem!.Checked = value;
                    }
                    else
                    {
                        _finishedOnHardChecked = value;
                    }
                    break;
                case Difficulty.Expert:
                    if (_fmMenuConstructed)
                    {
                        FinishedOnExpertMenuItem!.Checked = value;
                    }
                    else
                    {
                        _finishedOnExpertChecked = value;
                    }
                    break;
                case Difficulty.Extreme:
                    if (_fmMenuConstructed)
                    {
                        FinishedOnExtremeMenuItem!.Checked = value;
                    }
                    else
                    {
                        _finishedOnExtremeChecked = value;
                    }
                    break;
            }
        }

        internal void SetFinishedOnMenuItemText(Difficulty difficulty, string text)
        {
            switch (difficulty)
            {
                case Difficulty.Normal:
                    if (_fmMenuConstructed) FinishedOnNormalMenuItem!.Text = text;
                    break;
                case Difficulty.Hard:
                    if (_fmMenuConstructed) FinishedOnHardMenuItem!.Text = text;
                    break;
                case Difficulty.Expert:
                    if (_fmMenuConstructed) FinishedOnExpertMenuItem!.Text = text;
                    break;
                case Difficulty.Extreme:
                    if (_fmMenuConstructed) FinishedOnExtremeMenuItem!.Text = text;
                    break;
            }
        }

        internal void SetFinishedOnUnknownMenuItemChecked(bool value)
        {
            if (_fmMenuConstructed)
            {
                FinishedOnUnknownMenuItem!.Checked = value;
            }
            else
            {
                _finishedOnUnknownChecked = value;
            }

            if (value) UncheckFinishedOnMenuItemsExceptUnknown();
        }

        internal void ClearFinishedOnMenuItemChecks()
        {
            SetFinishedOnUnknownMenuItemChecked(false);
            UncheckFinishedOnMenuItemsExceptUnknown();
        }

        #endregion

        #region Event handlers

        private void FMContextMenu_Opening(object sender, CancelEventArgs e)
        {
            // Fix for a corner case where the user could press the right mouse button, hold it, keyboard-switch
            // to an empty tab, then let up the mouse and a menu would come up even though no FM was selected.
            if (RowCount == 0 || SelectedRows.Count == 0) e.Cancel = true;
        }

        private async void PlayFMMenuItem_Click(object sender, EventArgs e) => await FMInstallAndPlay.InstallIfNeededAndPlay(GetSelectedFM());

        private async void PlayFMInMPMenuItem_Click(object sender, EventArgs e) => await FMInstallAndPlay.InstallIfNeededAndPlay(GetSelectedFM(), playMP: true);

        private async void InstallUninstallMenuItem_Click(object sender, EventArgs e) => await FMInstallAndPlay.InstallOrUninstall(GetSelectedFM());

        private async void DeleteFMMenuItem_Click(object sender, EventArgs e) => await Core.DeleteFMArchive(GetSelectedFM());

        private async void OpenInDromEdMenuItem_Click(object sender, EventArgs e)
        {
            var fm = GetSelectedFM();
            if (fm.Installed || await FMInstallAndPlay.InstallFM(fm)) FMInstallAndPlay.OpenFMInEditor(fm);
        }

        private async void ScanFMMenuItem_Click(object sender, EventArgs e) => await FMScan.ScanFMAndRefresh(GetSelectedFM());

        private async void ConvertWAVsTo16BitMenuItem_Click(object sender, EventArgs e) => await FMAudio.ConvertWAVsTo16Bit(GetSelectedFM(), true);

        private async void ConvertOGGsToWAVsMenuItem_Click(object sender, EventArgs e) => await FMAudio.ConvertOGGsToWAVs(GetSelectedFM(), true);

        private void RatingMenuItems_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < RatingMenuItem!.DropDownItems.Count; i++)
            {
                if (RatingMenuItem.DropDownItems[i] == sender)
                {
                    GetSelectedFM().Rating = i - 1;
                    Owner.RefreshSelectedFM(refreshReadme: false);
                    Ini.WriteFullFMDataIni();
                    break;
                }
            }
        }

        private void RatingRCMenuItems_CheckedChanged(object sender, EventArgs e)
        {
            var s = (ToolStripMenuItem)sender;
            if (!s.Checked) return;

            foreach (ToolStripMenuItem item in RatingMenuItem!.DropDownItems) if (item != s) item.Checked = false;
        }

        private void FinishedOnMenuItems_Click(object sender, EventArgs e)
        {
            var senderItem = (ToolStripMenuItem)sender;

            var fm = GetSelectedFM();

            fm.FinishedOn = 0;
            fm.FinishedOnUnknown = false;

            if (senderItem == FinishedOnUnknownMenuItem)
            {
                fm.FinishedOnUnknown = senderItem.Checked;
            }
            else
            {
                uint at = 1;
                foreach (ToolStripMenuItem item in FinishedOnMenu!.Items)
                {
                    if (item == FinishedOnUnknownMenuItem) continue;

                    if (item.Checked) fm.FinishedOn |= at;
                    at <<= 1;
                }
                if (fm.FinishedOn > 0)
                {
                    FinishedOnUnknownMenuItem!.Checked = false;
                    fm.FinishedOnUnknown = false;
                }
            }

            Owner.RefreshSelectedFMRowOnly();
            Ini.WriteFullFMDataIni();
        }

        private void FinishedOnUnknownMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (FinishedOnUnknownMenuItem!.Checked) UncheckFinishedOnMenuItemsExceptUnknown();
        }

        private void WebSearchMenuItem_Click(object sender, EventArgs e) => Core.OpenWebSearchUrl(GetSelectedFM().Title);

        #endregion

        private void DisposeFMContextMenu()
        {
            for (int i = 0; i < FMContextMenuDisposables?.Length; i++)
            {
                FMContextMenuDisposables?[i]?.Dispose();
            }
        }
    }
}
