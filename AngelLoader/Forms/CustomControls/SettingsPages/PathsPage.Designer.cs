﻿namespace AngelLoader.Forms.CustomControls.SettingsPages
{
    partial class PathsPage
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.PagePanel = new System.Windows.Forms.Panel();
            this.ActualPathsPanel = new System.Windows.Forms.Panel();
            this.SteamOptionsGroupBox = new System.Windows.Forms.GroupBox();
            this.LaunchTheseGamesThroughSteamPanel = new System.Windows.Forms.Panel();
            this.LaunchTheseGamesThroughSteamCheckBox = new System.Windows.Forms.CheckBox();
            this.T1UseSteamCheckBox = new System.Windows.Forms.CheckBox();
            this.SS2UseSteamCheckBox = new System.Windows.Forms.CheckBox();
            this.T3UseSteamCheckBox = new System.Windows.Forms.CheckBox();
            this.T2UseSteamCheckBox = new System.Windows.Forms.CheckBox();
            this.SteamExeLabel = new System.Windows.Forms.Label();
            this.SteamExeTextBox = new System.Windows.Forms.TextBox();
            this.SteamExeBrowseButton = new System.Windows.Forms.Button();
            this.PathsToGameExesGroupBox = new System.Windows.Forms.GroupBox();
            this.GameRequirementsPanel = new System.Windows.Forms.Panel();
            this.GameRequirementsLabel = new System.Windows.Forms.Label();
            this.SS2ExePathLabel = new System.Windows.Forms.Label();
            this.Thief3ExePathLabel = new System.Windows.Forms.Label();
            this.Thief2ExePathLabel = new System.Windows.Forms.Label();
            this.Thief1ExePathLabel = new System.Windows.Forms.Label();
            this.SS2ExePathBrowseButton = new System.Windows.Forms.Button();
            this.Thief3ExePathBrowseButton = new System.Windows.Forms.Button();
            this.Thief2ExePathBrowseButton = new System.Windows.Forms.Button();
            this.Thief1ExePathBrowseButton = new System.Windows.Forms.Button();
            this.SS2ExePathTextBox = new System.Windows.Forms.TextBox();
            this.Thief3ExePathTextBox = new System.Windows.Forms.TextBox();
            this.Thief2ExePathTextBox = new System.Windows.Forms.TextBox();
            this.Thief1ExePathTextBox = new System.Windows.Forms.TextBox();
            this.FMArchivePathsGroupBox = new System.Windows.Forms.GroupBox();
            this.IncludeSubfoldersCheckBox = new System.Windows.Forms.CheckBox();
            this.AddFMArchivePathButton = new System.Windows.Forms.Button();
            this.RemoveFMArchivePathButton = new System.Windows.Forms.Button();
            this.FMArchivePathsListBox = new System.Windows.Forms.ListBox();
            this.OtherGroupBox = new System.Windows.Forms.GroupBox();
            this.BackupPathLabel = new System.Windows.Forms.Label();
            this.BackupPathBrowseButton = new System.Windows.Forms.Button();
            this.BackupPathTextBox = new System.Windows.Forms.TextBox();
            this.DummyAutoScrollPanel = new System.Windows.Forms.Control();
            this.PagePanel.SuspendLayout();
            this.ActualPathsPanel.SuspendLayout();
            this.SteamOptionsGroupBox.SuspendLayout();
            this.LaunchTheseGamesThroughSteamPanel.SuspendLayout();
            this.PathsToGameExesGroupBox.SuspendLayout();
            this.GameRequirementsPanel.SuspendLayout();
            this.FMArchivePathsGroupBox.SuspendLayout();
            this.OtherGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // PagePanel
            // 
            this.PagePanel.AutoScroll = true;
            this.PagePanel.Controls.Add(this.ActualPathsPanel);
            this.PagePanel.Controls.Add(this.DummyAutoScrollPanel);
            this.PagePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PagePanel.Location = new System.Drawing.Point(0, 0);
            this.PagePanel.Name = "PagePanel";
            this.PagePanel.Size = new System.Drawing.Size(440, 803);
            this.PagePanel.TabIndex = 3;
            // 
            // ActualPathsPanel
            // 
            this.ActualPathsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ActualPathsPanel.Controls.Add(this.SteamOptionsGroupBox);
            this.ActualPathsPanel.Controls.Add(this.PathsToGameExesGroupBox);
            this.ActualPathsPanel.Controls.Add(this.FMArchivePathsGroupBox);
            this.ActualPathsPanel.Controls.Add(this.OtherGroupBox);
            this.ActualPathsPanel.Location = new System.Drawing.Point(0, 0);
            this.ActualPathsPanel.MinimumSize = new System.Drawing.Size(440, 0);
            this.ActualPathsPanel.Name = "ActualPathsPanel";
            this.ActualPathsPanel.Size = new System.Drawing.Size(440, 784);
            this.ActualPathsPanel.TabIndex = 4;
            // 
            // SteamOptionsGroupBox
            // 
            this.SteamOptionsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SteamOptionsGroupBox.Controls.Add(this.LaunchTheseGamesThroughSteamPanel);
            this.SteamOptionsGroupBox.Controls.Add(this.SteamExeLabel);
            this.SteamOptionsGroupBox.Controls.Add(this.SteamExeTextBox);
            this.SteamOptionsGroupBox.Controls.Add(this.SteamExeBrowseButton);
            this.SteamOptionsGroupBox.Location = new System.Drawing.Point(8, 248);
            this.SteamOptionsGroupBox.Name = "SteamOptionsGroupBox";
            this.SteamOptionsGroupBox.Size = new System.Drawing.Size(424, 176);
            this.SteamOptionsGroupBox.TabIndex = 1;
            this.SteamOptionsGroupBox.TabStop = false;
            this.SteamOptionsGroupBox.Text = "Steam options";
            // 
            // LaunchTheseGamesThroughSteamPanel
            // 
            this.LaunchTheseGamesThroughSteamPanel.Controls.Add(this.LaunchTheseGamesThroughSteamCheckBox);
            this.LaunchTheseGamesThroughSteamPanel.Controls.Add(this.T1UseSteamCheckBox);
            this.LaunchTheseGamesThroughSteamPanel.Controls.Add(this.SS2UseSteamCheckBox);
            this.LaunchTheseGamesThroughSteamPanel.Controls.Add(this.T3UseSteamCheckBox);
            this.LaunchTheseGamesThroughSteamPanel.Controls.Add(this.T2UseSteamCheckBox);
            this.LaunchTheseGamesThroughSteamPanel.Enabled = false;
            this.LaunchTheseGamesThroughSteamPanel.Location = new System.Drawing.Point(16, 72);
            this.LaunchTheseGamesThroughSteamPanel.Name = "LaunchTheseGamesThroughSteamPanel";
            this.LaunchTheseGamesThroughSteamPanel.Size = new System.Drawing.Size(392, 96);
            this.LaunchTheseGamesThroughSteamPanel.TabIndex = 7;
            // 
            // LaunchTheseGamesThroughSteamCheckBox
            // 
            this.LaunchTheseGamesThroughSteamCheckBox.AutoSize = true;
            this.LaunchTheseGamesThroughSteamCheckBox.Checked = true;
            this.LaunchTheseGamesThroughSteamCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.LaunchTheseGamesThroughSteamCheckBox.Location = new System.Drawing.Point(0, 0);
            this.LaunchTheseGamesThroughSteamCheckBox.Name = "LaunchTheseGamesThroughSteamCheckBox";
            this.LaunchTheseGamesThroughSteamCheckBox.Size = new System.Drawing.Size(238, 17);
            this.LaunchTheseGamesThroughSteamCheckBox.TabIndex = 0;
            this.LaunchTheseGamesThroughSteamCheckBox.Text = "If Steam exists, use it to launch these games:";
            this.LaunchTheseGamesThroughSteamCheckBox.UseVisualStyleBackColor = true;
            // 
            // T1UseSteamCheckBox
            // 
            this.T1UseSteamCheckBox.AutoSize = true;
            this.T1UseSteamCheckBox.Checked = true;
            this.T1UseSteamCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.T1UseSteamCheckBox.Location = new System.Drawing.Point(8, 24);
            this.T1UseSteamCheckBox.Name = "T1UseSteamCheckBox";
            this.T1UseSteamCheckBox.Size = new System.Drawing.Size(59, 17);
            this.T1UseSteamCheckBox.TabIndex = 1;
            this.T1UseSteamCheckBox.Text = "Thief 1";
            this.T1UseSteamCheckBox.UseVisualStyleBackColor = true;
            // 
            // SS2UseSteamCheckBox
            // 
            this.SS2UseSteamCheckBox.AutoSize = true;
            this.SS2UseSteamCheckBox.Checked = true;
            this.SS2UseSteamCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.SS2UseSteamCheckBox.Location = new System.Drawing.Point(8, 72);
            this.SS2UseSteamCheckBox.Name = "SS2UseSteamCheckBox";
            this.SS2UseSteamCheckBox.Size = new System.Drawing.Size(103, 17);
            this.SS2UseSteamCheckBox.TabIndex = 4;
            this.SS2UseSteamCheckBox.Text = "System Shock 2";
            this.SS2UseSteamCheckBox.UseVisualStyleBackColor = true;
            // 
            // T3UseSteamCheckBox
            // 
            this.T3UseSteamCheckBox.AutoSize = true;
            this.T3UseSteamCheckBox.Checked = true;
            this.T3UseSteamCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.T3UseSteamCheckBox.Location = new System.Drawing.Point(8, 56);
            this.T3UseSteamCheckBox.Name = "T3UseSteamCheckBox";
            this.T3UseSteamCheckBox.Size = new System.Drawing.Size(59, 17);
            this.T3UseSteamCheckBox.TabIndex = 3;
            this.T3UseSteamCheckBox.Text = "Thief 3";
            this.T3UseSteamCheckBox.UseVisualStyleBackColor = true;
            // 
            // T2UseSteamCheckBox
            // 
            this.T2UseSteamCheckBox.AutoSize = true;
            this.T2UseSteamCheckBox.Checked = true;
            this.T2UseSteamCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.T2UseSteamCheckBox.Location = new System.Drawing.Point(8, 40);
            this.T2UseSteamCheckBox.Name = "T2UseSteamCheckBox";
            this.T2UseSteamCheckBox.Size = new System.Drawing.Size(59, 17);
            this.T2UseSteamCheckBox.TabIndex = 2;
            this.T2UseSteamCheckBox.Text = "Thief 2";
            this.T2UseSteamCheckBox.UseVisualStyleBackColor = true;
            // 
            // SteamExeLabel
            // 
            this.SteamExeLabel.AutoSize = true;
            this.SteamExeLabel.Location = new System.Drawing.Point(16, 24);
            this.SteamExeLabel.Name = "SteamExeLabel";
            this.SteamExeLabel.Size = new System.Drawing.Size(178, 13);
            this.SteamExeLabel.TabIndex = 0;
            this.SteamExeLabel.Text = "Path to Steam executable (optional):";
            // 
            // SteamExeTextBox
            // 
            this.SteamExeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SteamExeTextBox.Location = new System.Drawing.Point(16, 40);
            this.SteamExeTextBox.Name = "SteamExeTextBox";
            this.SteamExeTextBox.Size = new System.Drawing.Size(320, 20);
            this.SteamExeTextBox.TabIndex = 1;
            // 
            // SteamExeBrowseButton
            // 
            this.SteamExeBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SteamExeBrowseButton.AutoSize = true;
            this.SteamExeBrowseButton.Location = new System.Drawing.Point(336, 39);
            this.SteamExeBrowseButton.Name = "SteamExeBrowseButton";
            this.SteamExeBrowseButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.SteamExeBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.SteamExeBrowseButton.TabIndex = 2;
            this.SteamExeBrowseButton.Text = "Browse...";
            this.SteamExeBrowseButton.UseVisualStyleBackColor = true;
            // 
            // PathsToGameExesGroupBox
            // 
            this.PathsToGameExesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PathsToGameExesGroupBox.Controls.Add(this.GameRequirementsPanel);
            this.PathsToGameExesGroupBox.Controls.Add(this.SS2ExePathLabel);
            this.PathsToGameExesGroupBox.Controls.Add(this.Thief3ExePathLabel);
            this.PathsToGameExesGroupBox.Controls.Add(this.Thief2ExePathLabel);
            this.PathsToGameExesGroupBox.Controls.Add(this.Thief1ExePathLabel);
            this.PathsToGameExesGroupBox.Controls.Add(this.SS2ExePathBrowseButton);
            this.PathsToGameExesGroupBox.Controls.Add(this.Thief3ExePathBrowseButton);
            this.PathsToGameExesGroupBox.Controls.Add(this.Thief2ExePathBrowseButton);
            this.PathsToGameExesGroupBox.Controls.Add(this.Thief1ExePathBrowseButton);
            this.PathsToGameExesGroupBox.Controls.Add(this.SS2ExePathTextBox);
            this.PathsToGameExesGroupBox.Controls.Add(this.Thief3ExePathTextBox);
            this.PathsToGameExesGroupBox.Controls.Add(this.Thief2ExePathTextBox);
            this.PathsToGameExesGroupBox.Controls.Add(this.Thief1ExePathTextBox);
            this.PathsToGameExesGroupBox.Location = new System.Drawing.Point(8, 8);
            this.PathsToGameExesGroupBox.MinimumSize = new System.Drawing.Size(424, 0);
            this.PathsToGameExesGroupBox.Name = "PathsToGameExesGroupBox";
            this.PathsToGameExesGroupBox.Size = new System.Drawing.Size(424, 232);
            this.PathsToGameExesGroupBox.TabIndex = 0;
            this.PathsToGameExesGroupBox.TabStop = false;
            this.PathsToGameExesGroupBox.Text = "Paths to game executables";
            // 
            // GameRequirementsPanel
            // 
            this.GameRequirementsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GameRequirementsPanel.AutoScroll = true;
            this.GameRequirementsPanel.Controls.Add(this.GameRequirementsLabel);
            this.GameRequirementsPanel.Location = new System.Drawing.Point(16, 192);
            this.GameRequirementsPanel.Name = "GameRequirementsPanel";
            this.GameRequirementsPanel.Size = new System.Drawing.Size(392, 32);
            this.GameRequirementsPanel.TabIndex = 12;
            // 
            // GameRequirementsLabel
            // 
            this.GameRequirementsLabel.AutoSize = true;
            this.GameRequirementsLabel.Location = new System.Drawing.Point(0, 0);
            this.GameRequirementsLabel.Name = "GameRequirementsLabel";
            this.GameRequirementsLabel.Size = new System.Drawing.Size(273, 26);
            this.GameRequirementsLabel.TabIndex = 0;
            this.GameRequirementsLabel.Text = "* Thief 1, Thief 2 and System Shock 2 require NewDark.\r\n* Thief 3 requires the Sn" +
    "eaky Upgrade 1.1.9.1 or above.";
            this.GameRequirementsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // SS2ExePathLabel
            // 
            this.SS2ExePathLabel.AutoSize = true;
            this.SS2ExePathLabel.Location = new System.Drawing.Point(16, 144);
            this.SS2ExePathLabel.Name = "SS2ExePathLabel";
            this.SS2ExePathLabel.Size = new System.Drawing.Size(87, 13);
            this.SS2ExePathLabel.TabIndex = 9;
            this.SS2ExePathLabel.Text = "System Shock 2:";
            // 
            // Thief3ExePathLabel
            // 
            this.Thief3ExePathLabel.AutoSize = true;
            this.Thief3ExePathLabel.Location = new System.Drawing.Point(16, 104);
            this.Thief3ExePathLabel.Name = "Thief3ExePathLabel";
            this.Thief3ExePathLabel.Size = new System.Drawing.Size(43, 13);
            this.Thief3ExePathLabel.TabIndex = 6;
            this.Thief3ExePathLabel.Text = "Thief 3:";
            // 
            // Thief2ExePathLabel
            // 
            this.Thief2ExePathLabel.AutoSize = true;
            this.Thief2ExePathLabel.Location = new System.Drawing.Point(16, 64);
            this.Thief2ExePathLabel.Name = "Thief2ExePathLabel";
            this.Thief2ExePathLabel.Size = new System.Drawing.Size(43, 13);
            this.Thief2ExePathLabel.TabIndex = 3;
            this.Thief2ExePathLabel.Text = "Thief 2:";
            // 
            // Thief1ExePathLabel
            // 
            this.Thief1ExePathLabel.AutoSize = true;
            this.Thief1ExePathLabel.Location = new System.Drawing.Point(16, 24);
            this.Thief1ExePathLabel.Name = "Thief1ExePathLabel";
            this.Thief1ExePathLabel.Size = new System.Drawing.Size(43, 13);
            this.Thief1ExePathLabel.TabIndex = 0;
            this.Thief1ExePathLabel.Text = "Thief 1:";
            // 
            // SS2ExePathBrowseButton
            // 
            this.SS2ExePathBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SS2ExePathBrowseButton.AutoSize = true;
            this.SS2ExePathBrowseButton.Location = new System.Drawing.Point(336, 159);
            this.SS2ExePathBrowseButton.Name = "SS2ExePathBrowseButton";
            this.SS2ExePathBrowseButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.SS2ExePathBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.SS2ExePathBrowseButton.TabIndex = 11;
            this.SS2ExePathBrowseButton.Text = "Browse...";
            this.SS2ExePathBrowseButton.UseVisualStyleBackColor = true;
            // 
            // Thief3ExePathBrowseButton
            // 
            this.Thief3ExePathBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Thief3ExePathBrowseButton.AutoSize = true;
            this.Thief3ExePathBrowseButton.Location = new System.Drawing.Point(336, 119);
            this.Thief3ExePathBrowseButton.Name = "Thief3ExePathBrowseButton";
            this.Thief3ExePathBrowseButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.Thief3ExePathBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.Thief3ExePathBrowseButton.TabIndex = 8;
            this.Thief3ExePathBrowseButton.Text = "Browse...";
            this.Thief3ExePathBrowseButton.UseVisualStyleBackColor = true;
            // 
            // Thief2ExePathBrowseButton
            // 
            this.Thief2ExePathBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Thief2ExePathBrowseButton.AutoSize = true;
            this.Thief2ExePathBrowseButton.Location = new System.Drawing.Point(336, 79);
            this.Thief2ExePathBrowseButton.Name = "Thief2ExePathBrowseButton";
            this.Thief2ExePathBrowseButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.Thief2ExePathBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.Thief2ExePathBrowseButton.TabIndex = 5;
            this.Thief2ExePathBrowseButton.Text = "Browse...";
            this.Thief2ExePathBrowseButton.UseVisualStyleBackColor = true;
            // 
            // Thief1ExePathBrowseButton
            // 
            this.Thief1ExePathBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Thief1ExePathBrowseButton.AutoSize = true;
            this.Thief1ExePathBrowseButton.Location = new System.Drawing.Point(336, 39);
            this.Thief1ExePathBrowseButton.Name = "Thief1ExePathBrowseButton";
            this.Thief1ExePathBrowseButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.Thief1ExePathBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.Thief1ExePathBrowseButton.TabIndex = 2;
            this.Thief1ExePathBrowseButton.Text = "Browse...";
            this.Thief1ExePathBrowseButton.UseVisualStyleBackColor = true;
            // 
            // SS2ExePathTextBox
            // 
            this.SS2ExePathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SS2ExePathTextBox.Location = new System.Drawing.Point(16, 160);
            this.SS2ExePathTextBox.Name = "SS2ExePathTextBox";
            this.SS2ExePathTextBox.Size = new System.Drawing.Size(320, 20);
            this.SS2ExePathTextBox.TabIndex = 10;
            // 
            // Thief3ExePathTextBox
            // 
            this.Thief3ExePathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Thief3ExePathTextBox.Location = new System.Drawing.Point(16, 120);
            this.Thief3ExePathTextBox.Name = "Thief3ExePathTextBox";
            this.Thief3ExePathTextBox.Size = new System.Drawing.Size(320, 20);
            this.Thief3ExePathTextBox.TabIndex = 7;
            // 
            // Thief2ExePathTextBox
            // 
            this.Thief2ExePathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Thief2ExePathTextBox.Location = new System.Drawing.Point(16, 80);
            this.Thief2ExePathTextBox.Name = "Thief2ExePathTextBox";
            this.Thief2ExePathTextBox.Size = new System.Drawing.Size(320, 20);
            this.Thief2ExePathTextBox.TabIndex = 4;
            // 
            // Thief1ExePathTextBox
            // 
            this.Thief1ExePathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Thief1ExePathTextBox.Location = new System.Drawing.Point(16, 40);
            this.Thief1ExePathTextBox.Name = "Thief1ExePathTextBox";
            this.Thief1ExePathTextBox.Size = new System.Drawing.Size(320, 20);
            this.Thief1ExePathTextBox.TabIndex = 1;
            // 
            // FMArchivePathsGroupBox
            // 
            this.FMArchivePathsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FMArchivePathsGroupBox.Controls.Add(this.IncludeSubfoldersCheckBox);
            this.FMArchivePathsGroupBox.Controls.Add(this.AddFMArchivePathButton);
            this.FMArchivePathsGroupBox.Controls.Add(this.RemoveFMArchivePathButton);
            this.FMArchivePathsGroupBox.Controls.Add(this.FMArchivePathsListBox);
            this.FMArchivePathsGroupBox.Location = new System.Drawing.Point(8, 520);
            this.FMArchivePathsGroupBox.MinimumSize = new System.Drawing.Size(424, 0);
            this.FMArchivePathsGroupBox.Name = "FMArchivePathsGroupBox";
            this.FMArchivePathsGroupBox.Size = new System.Drawing.Size(424, 256);
            this.FMArchivePathsGroupBox.TabIndex = 3;
            this.FMArchivePathsGroupBox.TabStop = false;
            this.FMArchivePathsGroupBox.Text = "FM archive paths";
            // 
            // IncludeSubfoldersCheckBox
            // 
            this.IncludeSubfoldersCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.IncludeSubfoldersCheckBox.AutoSize = true;
            this.IncludeSubfoldersCheckBox.Location = new System.Drawing.Point(16, 228);
            this.IncludeSubfoldersCheckBox.Name = "IncludeSubfoldersCheckBox";
            this.IncludeSubfoldersCheckBox.Size = new System.Drawing.Size(112, 17);
            this.IncludeSubfoldersCheckBox.TabIndex = 1;
            this.IncludeSubfoldersCheckBox.Text = "Include subfolders";
            this.IncludeSubfoldersCheckBox.UseVisualStyleBackColor = true;
            // 
            // AddFMArchivePathButton
            // 
            this.AddFMArchivePathButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.AddFMArchivePathButton.Location = new System.Drawing.Point(386, 224);
            this.AddFMArchivePathButton.Name = "AddFMArchivePathButton";
            this.AddFMArchivePathButton.Size = new System.Drawing.Size(23, 23);
            this.AddFMArchivePathButton.TabIndex = 3;
            this.AddFMArchivePathButton.UseVisualStyleBackColor = true;
            this.AddFMArchivePathButton.Paint += new System.Windows.Forms.PaintEventHandler(this.AddFMArchivePathButton_Paint);
            // 
            // RemoveFMArchivePathButton
            // 
            this.RemoveFMArchivePathButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.RemoveFMArchivePathButton.Location = new System.Drawing.Point(362, 224);
            this.RemoveFMArchivePathButton.Name = "RemoveFMArchivePathButton";
            this.RemoveFMArchivePathButton.Size = new System.Drawing.Size(23, 23);
            this.RemoveFMArchivePathButton.TabIndex = 2;
            this.RemoveFMArchivePathButton.UseVisualStyleBackColor = true;
            this.RemoveFMArchivePathButton.Paint += new System.Windows.Forms.PaintEventHandler(this.RemoveFMArchivePathButton_Paint);
            // 
            // FMArchivePathsListBox
            // 
            this.FMArchivePathsListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FMArchivePathsListBox.FormattingEnabled = true;
            this.FMArchivePathsListBox.Location = new System.Drawing.Point(16, 24);
            this.FMArchivePathsListBox.Name = "FMArchivePathsListBox";
            this.FMArchivePathsListBox.Size = new System.Drawing.Size(392, 199);
            this.FMArchivePathsListBox.TabIndex = 0;
            // 
            // OtherGroupBox
            // 
            this.OtherGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OtherGroupBox.Controls.Add(this.BackupPathLabel);
            this.OtherGroupBox.Controls.Add(this.BackupPathBrowseButton);
            this.OtherGroupBox.Controls.Add(this.BackupPathTextBox);
            this.OtherGroupBox.Location = new System.Drawing.Point(8, 432);
            this.OtherGroupBox.MinimumSize = new System.Drawing.Size(424, 0);
            this.OtherGroupBox.Name = "OtherGroupBox";
            this.OtherGroupBox.Size = new System.Drawing.Size(424, 72);
            this.OtherGroupBox.TabIndex = 2;
            this.OtherGroupBox.TabStop = false;
            this.OtherGroupBox.Text = "Other";
            // 
            // BackupPathLabel
            // 
            this.BackupPathLabel.AutoSize = true;
            this.BackupPathLabel.Location = new System.Drawing.Point(16, 24);
            this.BackupPathLabel.Name = "BackupPathLabel";
            this.BackupPathLabel.Size = new System.Drawing.Size(88, 13);
            this.BackupPathLabel.TabIndex = 0;
            this.BackupPathLabel.Text = "FM backup path:";
            // 
            // BackupPathBrowseButton
            // 
            this.BackupPathBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BackupPathBrowseButton.AutoSize = true;
            this.BackupPathBrowseButton.Location = new System.Drawing.Point(336, 39);
            this.BackupPathBrowseButton.Name = "BackupPathBrowseButton";
            this.BackupPathBrowseButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.BackupPathBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.BackupPathBrowseButton.TabIndex = 2;
            this.BackupPathBrowseButton.Text = "Browse...";
            this.BackupPathBrowseButton.UseVisualStyleBackColor = true;
            // 
            // BackupPathTextBox
            // 
            this.BackupPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BackupPathTextBox.Location = new System.Drawing.Point(16, 40);
            this.BackupPathTextBox.Name = "BackupPathTextBox";
            this.BackupPathTextBox.Size = new System.Drawing.Size(320, 20);
            this.BackupPathTextBox.TabIndex = 1;
            // 
            // DummyAutoScrollPanel
            // 
            this.DummyAutoScrollPanel.Location = new System.Drawing.Point(8, 200);
            this.DummyAutoScrollPanel.Name = "DummyAutoScrollPanel";
            this.DummyAutoScrollPanel.Size = new System.Drawing.Size(424, 8);
            this.DummyAutoScrollPanel.TabIndex = 13;
            // 
            // PathsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.PagePanel);
            this.Name = "PathsPage";
            this.Size = new System.Drawing.Size(440, 803);
            this.PagePanel.ResumeLayout(false);
            this.ActualPathsPanel.ResumeLayout(false);
            this.SteamOptionsGroupBox.ResumeLayout(false);
            this.SteamOptionsGroupBox.PerformLayout();
            this.LaunchTheseGamesThroughSteamPanel.ResumeLayout(false);
            this.LaunchTheseGamesThroughSteamPanel.PerformLayout();
            this.PathsToGameExesGroupBox.ResumeLayout(false);
            this.PathsToGameExesGroupBox.PerformLayout();
            this.GameRequirementsPanel.ResumeLayout(false);
            this.GameRequirementsPanel.PerformLayout();
            this.FMArchivePathsGroupBox.ResumeLayout(false);
            this.FMArchivePathsGroupBox.PerformLayout();
            this.OtherGroupBox.ResumeLayout(false);
            this.OtherGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.Panel PagePanel;
        internal System.Windows.Forms.GroupBox OtherGroupBox;
        internal System.Windows.Forms.Label BackupPathLabel;
        internal System.Windows.Forms.Button BackupPathBrowseButton;
        internal System.Windows.Forms.TextBox BackupPathTextBox;
        internal System.Windows.Forms.GroupBox PathsToGameExesGroupBox;
        internal System.Windows.Forms.Panel GameRequirementsPanel;
        internal System.Windows.Forms.Label GameRequirementsLabel;
        internal System.Windows.Forms.Label Thief3ExePathLabel;
        internal System.Windows.Forms.Label Thief2ExePathLabel;
        internal System.Windows.Forms.Label Thief1ExePathLabel;
        internal System.Windows.Forms.Button Thief3ExePathBrowseButton;
        internal System.Windows.Forms.Button Thief2ExePathBrowseButton;
        internal System.Windows.Forms.Button Thief1ExePathBrowseButton;
        internal System.Windows.Forms.TextBox Thief3ExePathTextBox;
        internal System.Windows.Forms.TextBox Thief2ExePathTextBox;
        internal System.Windows.Forms.TextBox Thief1ExePathTextBox;
        internal System.Windows.Forms.GroupBox FMArchivePathsGroupBox;
        internal System.Windows.Forms.CheckBox IncludeSubfoldersCheckBox;
        internal System.Windows.Forms.Button AddFMArchivePathButton;
        internal System.Windows.Forms.Button RemoveFMArchivePathButton;
        internal System.Windows.Forms.ListBox FMArchivePathsListBox;
        internal System.Windows.Forms.Panel ActualPathsPanel;
        internal System.Windows.Forms.Control DummyAutoScrollPanel;
        internal System.Windows.Forms.Label SteamExeLabel;
        internal System.Windows.Forms.TextBox SteamExeTextBox;
        internal System.Windows.Forms.Button SteamExeBrowseButton;
        internal System.Windows.Forms.GroupBox SteamOptionsGroupBox;
        internal System.Windows.Forms.CheckBox T3UseSteamCheckBox;
        internal System.Windows.Forms.CheckBox T2UseSteamCheckBox;
        internal System.Windows.Forms.CheckBox T1UseSteamCheckBox;
        internal System.Windows.Forms.Panel LaunchTheseGamesThroughSteamPanel;
        internal System.Windows.Forms.CheckBox LaunchTheseGamesThroughSteamCheckBox;
        internal System.Windows.Forms.Label SS2ExePathLabel;
        internal System.Windows.Forms.Button SS2ExePathBrowseButton;
        internal System.Windows.Forms.TextBox SS2ExePathTextBox;
        internal System.Windows.Forms.CheckBox SS2UseSteamCheckBox;
    }
}
