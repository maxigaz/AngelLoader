﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Properties;
using JetBrains.Annotations;

namespace AngelLoader.Forms
{
    public sealed partial class MessageBoxCustomForm : Form, Misc.ILocalizable
    {
        #region P/Invoke crap

        [SuppressMessage("ReSharper", "IdentifierTypo")]
        private enum SHSTOCKICONID : uint
        {
            SIID_HELP = 23,
            SIID_WARNING = 78,
            SIID_INFO = 79,
            SIID_ERROR = 80
        }

        [Flags]
        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        public enum SHGSI : uint
        {
            SHGSI_ICONLOCATION = 0,
            SHGSI_ICON = 0x000000100,
            SHGSI_SYSICONINDEX = 0x000004000,
            SHGSI_LINKOVERLAY = 0x000008000,
            SHGSI_SELECTED = 0x000010000,
            SHGSI_LARGEICON = 0x000000000,
            SHGSI_SMALLICON = 0x000000001,
            SHGSI_SHELLICONSIZE = 0x000000004
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        private struct SHSTOCKICONINFO
        {
            internal uint cbSize;
            internal IntPtr hIcon;
            internal int iSysIconIndex;
            internal int iIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260/*MAX_PATH*/)]
            internal string szPath;
        }

        [DllImport("Shell32.dll", SetLastError = false)]
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        private static extern int SHGetStockIconInfo(SHSTOCKICONID siid, SHGSI uFlags, ref SHSTOCKICONINFO psii);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        #endregion

        private readonly bool _multiChoice;
        private const int _bottomAreaHeight = 42;
        private const int _leftAreaWidth = 60;
        private const int _edgePadding = 21;

        public readonly List<string> SelectedItems = new List<string>();

        public MessageBoxCustomForm(string messageTop, string messageBottom, string title, MessageBoxIcon icon,
            string okText, string cancelText, bool okIsDangerous, string[]? choiceStrings = null)
        {
            InitializeComponent();

            _multiChoice = choiceStrings?.Length > 0;

            #region Set fonts

            // Set these after InitializeComponent() in case that sets other fonts, but before anything else
            MessageTopLabel.Font = SystemFonts.MessageBoxFont;
            MessageBottomLabel.Font = SystemFonts.MessageBoxFont;
            SelectAllButton.Font = SystemFonts.MessageBoxFont;
            OKButton.Font = SystemFonts.MessageBoxFont;
            Cancel_Button.Font = SystemFonts.MessageBoxFont;
            ChoiceListBox.Font = SystemFonts.DefaultFont;

            #endregion

            #region Set passed-in values

            if (icon != MessageBoxIcon.None) SetMessageBoxIcon(icon);

            Text = title;
            MessageTopLabel.Text = messageTop;
            MessageBottomLabel.Text = messageBottom;

            if (_multiChoice)
            {
                // Set this first: the list is now populated
                for (int i = 0; i < choiceStrings!.Length; i++)
                {
                    ChoiceListBox.Items.Add(choiceStrings[i]);
                }
            }
            else
            {
                ChoiceListBox.Hide();
                SelectButtonsFLP.Hide();
                MessageBottomLabel.Hide();
            }

            #endregion

            #region Autosize controls

            int innerControlWidth = MainFLP.Width - 10;
            MessageTopLabel.MaximumSize = new Size(innerControlWidth, MessageTopLabel.MaximumSize.Height);
            MessageBottomLabel.MaximumSize = new Size(innerControlWidth, MessageBottomLabel.MaximumSize.Height);

            // Set this second: the list is now sized based on its content
            if (_multiChoice)
            {
                ChoiceListBox.Height =
                    (ChoiceListBox.ItemHeight * ChoiceListBox.Items.Count.Clamp(5, 20)) +
                    ((SystemInformation.BorderSize.Height * 4) + 3);
            }

            // Set these before window autosizing
            if (_multiChoice)
            {
                ChoiceListBox.Width = innerControlWidth;
                SelectButtonsFLP.Width = innerControlWidth + 1;
            }

            // Set these before setting button text
            if (okIsDangerous)
            {
                OKButton.TextImageRelation = TextImageRelation.ImageBeforeText;
                OKButton.ImageAlign = ContentAlignment.MiddleCenter;
                OKButton.Image = Resources.ExclMarkCircleRed_14;
            }

            OKButton.SetTextAutoSize(okText, OKButton.Width);
            Cancel_Button.SetTextAutoSize(cancelText, Cancel_Button.Width);

            #endregion

            Localize();

            SelectButtonsFLP.Height = SelectAllButton.Height;

            #region Autosize window

            // Run this after localization, so we have the right button widths

            #region Local functions

            static int GetFlowLayoutPanelControlsWidthAll(FlowLayoutPanel flp)
            {
                int ret = 0;
                for (int i = 0; i < flp.Controls.Count; i++)
                {
                    Control c = flp.Controls[i];
                    ret += c.Margin.Left + c.Margin.Right + c.Width;
                }
                ret += flp.Padding.Left + flp.Padding.Right;

                return ret;
            }

            int GetControlFullHeight(Control control, bool onlyIfVisible = false) =>
                !onlyIfVisible || _multiChoice
                    ? control.Margin.Top +
                      control.Margin.Bottom +
                      control.Height
                    : 0;

            static int MathMax4(int num1, int num2, int num3, int num4) => Math.Max(Math.Max(Math.Max(num1, num2), num3), num4);

            #endregion

            // Set this last: all controls sizes are now set, so we can size the window
            ClientSize = new Size(
                 width: _leftAreaWidth +
                        MathMax4(GetFlowLayoutPanelControlsWidthAll(BottomFLP),
                                 MessageTopLabel.Width,
                                 _multiChoice ? MessageBottomLabel.Width : 0,
                                 _multiChoice ? SelectButtonsFLP.Width : 0) +
                        _edgePadding,
                height: _bottomAreaHeight +
                        GetControlFullHeight(MessageTopLabel) +
                        GetControlFullHeight(ChoiceListBox, onlyIfVisible: true) +
                        GetControlFullHeight(SelectButtonsFLP, onlyIfVisible: true) +
                       (_multiChoice && messageBottom.IsEmpty() ? _edgePadding : GetControlFullHeight(MessageBottomLabel, onlyIfVisible: true)));

            #endregion

            if (ChoiceListBox.Items.Count > 0) ChoiceListBox.SetSelected(0, true);
        }

        private void SetMessageBoxIcon(MessageBoxIcon icon)
        {
            SHSTOCKICONINFO sii = new SHSTOCKICONINFO();
            try
            {
                SHSTOCKICONID sysIcon =
                      icon == MessageBoxIcon.Error ||
                      icon == MessageBoxIcon.Hand ||
                      icon == MessageBoxIcon.Stop
                    ? SHSTOCKICONID.SIID_ERROR
                    : icon == MessageBoxIcon.Question
                    ? SHSTOCKICONID.SIID_HELP
                    : icon == MessageBoxIcon.Exclamation ||
                      icon == MessageBoxIcon.Warning
                    ? SHSTOCKICONID.SIID_WARNING
                    : icon == MessageBoxIcon.Asterisk ||
                      icon == MessageBoxIcon.Information
                    ? SHSTOCKICONID.SIID_INFO
                    : throw new ArgumentOutOfRangeException();

                sii.cbSize = (uint)Marshal.SizeOf(typeof(SHSTOCKICONINFO));

                int result = SHGetStockIconInfo(sysIcon, SHGSI.SHGSI_ICON, ref sii);
                Marshal.ThrowExceptionForHR(result, new IntPtr(-1));

                IconPictureBox.Image = Icon.FromHandle(sii.hIcon).ToBitmap();
            }
            catch
            {
                // "Wrong style" image (different style from the MessageBox one) but better than nothing if the
                // above fails
                IconPictureBox.Image = SystemIcons.Warning.ToBitmap();
            }
            finally
            {
                DestroyIcon(sii.hIcon);
            }
        }

        public void Localize()
        {
            if (_multiChoice)
            {
                SelectAllButton.SetTextAutoSize(LText.Global.SelectAll, SelectAllButton.Width);
            }
        }

        private void SelectAllButton_Click(object sender, EventArgs e)
        {
            if (ChoiceListBox.Items.Count > 0)
            {
                for (int i = 0; i < ChoiceListBox.Items.Count; i++)
                {
                    ChoiceListBox.SetSelected(i, true);
                }
            }
        }

        // Shouldn't happen, but just in case
        private void ChoiceListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_multiChoice) OKButton.Enabled = ChoiceListBox.SelectedIndex > -1;
        }

        private void MessageBoxCustomForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK && _multiChoice && ChoiceListBox.SelectedIndex > -1)
            {
                foreach (object item in ChoiceListBox.SelectedItems)
                {
                    SelectedItems.Add(item.ToString());
                }
            }
        }
    }
}
