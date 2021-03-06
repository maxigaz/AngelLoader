﻿using System;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using FMScanner;

namespace AngelLoader.Forms
{
    public partial class ScanAllFMsForm : Form
    {
        private readonly CheckBox[] CheckBoxes;

        internal readonly ScanOptions ScanOptions = ScanOptions.FalseDefault();
        internal bool NoneSelected;

        public ScanAllFMsForm()
        {
            InitializeComponent();

            CheckBoxes = new[]
            {
                TitleCheckBox,
                AuthorCheckBox,
                GameCheckBox,
                CustomResourcesCheckBox,
                SizeCheckBox,
                ReleaseDateCheckBox,
                TagsCheckBox
            };

            Localize();
        }

        private void Localize()
        {
            Text = LText.ScanAllFMsBox.TitleText;

            ScanAllFMsForLabel.Text = LText.ScanAllFMsBox.ScanAllFMsFor;
            TitleCheckBox.Text = LText.ScanAllFMsBox.Title;
            AuthorCheckBox.Text = LText.ScanAllFMsBox.Author;
            GameCheckBox.Text = LText.ScanAllFMsBox.Game;
            CustomResourcesCheckBox.Text = LText.ScanAllFMsBox.CustomResources;
            SizeCheckBox.Text = LText.ScanAllFMsBox.Size;
            ReleaseDateCheckBox.Text = LText.ScanAllFMsBox.ReleaseDate;
            TagsCheckBox.Text = LText.ScanAllFMsBox.Tags;

            SelectAllButton.SetTextAutoSize(LText.Global.SelectAll);
            SelectNoneButton.SetTextAutoSize(LText.Global.SelectNone);

            ScanButton.SetTextAutoSize(LText.ScanAllFMsBox.Scan, ScanButton.Width);
            Cancel_Button.SetTextAutoSize(LText.Global.Cancel, Cancel_Button.Width);
        }

        private void SelectAllButton_Click(object sender, EventArgs e) => SetCheckBoxValues(true);

        private void SelectNoneButton_Click(object sender, EventArgs e) => SetCheckBoxValues(false);

        private void SetCheckBoxValues(bool enabled)
        {
            foreach (CheckBox cb in CheckBoxes) cb.Checked = enabled;
        }

        private void ScanAllFMs_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult != DialogResult.OK) return;

            bool noneChecked = true;
            for (int i = 0; i < CheckBoxes.Length; i++)
            {
                if (CheckBoxes[i].Checked)
                {
                    noneChecked = false;
                    break;
                }
            }

            if (noneChecked)
            {
                NoneSelected = true;
            }
            else
            {
                ScanOptions.ScanTitle = TitleCheckBox.Checked;
                ScanOptions.ScanAuthor = AuthorCheckBox.Checked;
                ScanOptions.ScanGameType = GameCheckBox.Checked;
                ScanOptions.ScanCustomResources = CustomResourcesCheckBox.Checked;
                ScanOptions.ScanSize = SizeCheckBox.Checked;
                ScanOptions.ScanReleaseDate = ReleaseDateCheckBox.Checked;
                ScanOptions.ScanTags = TagsCheckBox.Checked;
            }
        }
    }
}
