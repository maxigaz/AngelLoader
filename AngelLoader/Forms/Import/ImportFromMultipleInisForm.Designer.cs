﻿namespace AngelLoader.Forms.Import
{
    partial class ImportFromMultipleInisForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.OKButton = new System.Windows.Forms.Button();
            this.Cancel_Button = new System.Windows.Forms.Button();
            this.OKCancelFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.ImportControls = new AngelLoader.Forms.Import.User_FMSel_NDL_ImportControls();
            this.ImportSizeCheckBox = new System.Windows.Forms.CheckBox();
            this.ImportFinishedOnCheckBox = new System.Windows.Forms.CheckBox();
            this.ImportSelectedReadmeCheckBox = new System.Windows.Forms.CheckBox();
            this.ImportTagsCheckBox = new System.Windows.Forms.CheckBox();
            this.ImportDisabledModsCheckBox = new System.Windows.Forms.CheckBox();
            this.ImportRatingCheckBox = new System.Windows.Forms.CheckBox();
            this.ImportCommentCheckBox = new System.Windows.Forms.CheckBox();
            this.ImportLastPlayedCheckBox = new System.Windows.Forms.CheckBox();
            this.ImportReleaseDateCheckBox = new System.Windows.Forms.CheckBox();
            this.ImportTitleCheckBox = new System.Windows.Forms.CheckBox();
            this.OKCancelFlowLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // OKButton
            // 
            this.OKButton.AutoSize = true;
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = new System.Drawing.Point(3, 3);
            this.OKButton.Name = "OKButton";
            this.OKButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 1;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.AutoSize = true;
            this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel_Button.Location = new System.Drawing.Point(84, 3);
            this.Cancel_Button.Name = "Cancel_Button";
            this.Cancel_Button.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.Cancel_Button.Size = new System.Drawing.Size(75, 23);
            this.Cancel_Button.TabIndex = 0;
            this.Cancel_Button.Text = "Cancel";
            this.Cancel_Button.UseVisualStyleBackColor = true;
            // 
            // OKCancelFlowLayoutPanel
            // 
            this.OKCancelFlowLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.OKCancelFlowLayoutPanel.AutoSize = true;
            this.OKCancelFlowLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.OKCancelFlowLayoutPanel.Controls.Add(this.Cancel_Button);
            this.OKCancelFlowLayoutPanel.Controls.Add(this.OKButton);
            this.OKCancelFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.OKCancelFlowLayoutPanel.Location = new System.Drawing.Point(386, 477);
            this.OKCancelFlowLayoutPanel.Name = "OKCancelFlowLayoutPanel";
            this.OKCancelFlowLayoutPanel.Size = new System.Drawing.Size(162, 29);
            this.OKCancelFlowLayoutPanel.TabIndex = 0;
            // 
            // ImportControls
            // 
            this.ImportControls.Location = new System.Drawing.Point(0, 0);
            this.ImportControls.Name = "ImportControls";
            this.ImportControls.Size = new System.Drawing.Size(551, 312);
            this.ImportControls.TabIndex = 1;
            // 
            // ImportSizeCheckBox
            // 
            this.ImportSizeCheckBox.AutoSize = true;
            this.ImportSizeCheckBox.Checked = true;
            this.ImportSizeCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ImportSizeCheckBox.Location = new System.Drawing.Point(16, 456);
            this.ImportSizeCheckBox.Name = "ImportSizeCheckBox";
            this.ImportSizeCheckBox.Size = new System.Drawing.Size(46, 17);
            this.ImportSizeCheckBox.TabIndex = 19;
            this.ImportSizeCheckBox.Text = "Size";
            this.ImportSizeCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportFinishedOnCheckBox
            // 
            this.ImportFinishedOnCheckBox.AutoSize = true;
            this.ImportFinishedOnCheckBox.Checked = true;
            this.ImportFinishedOnCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ImportFinishedOnCheckBox.Location = new System.Drawing.Point(16, 440);
            this.ImportFinishedOnCheckBox.Name = "ImportFinishedOnCheckBox";
            this.ImportFinishedOnCheckBox.Size = new System.Drawing.Size(80, 17);
            this.ImportFinishedOnCheckBox.TabIndex = 20;
            this.ImportFinishedOnCheckBox.Text = "Finished on";
            this.ImportFinishedOnCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportSelectedReadmeCheckBox
            // 
            this.ImportSelectedReadmeCheckBox.AutoSize = true;
            this.ImportSelectedReadmeCheckBox.Checked = true;
            this.ImportSelectedReadmeCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ImportSelectedReadmeCheckBox.Location = new System.Drawing.Point(16, 424);
            this.ImportSelectedReadmeCheckBox.Name = "ImportSelectedReadmeCheckBox";
            this.ImportSelectedReadmeCheckBox.Size = new System.Drawing.Size(106, 17);
            this.ImportSelectedReadmeCheckBox.TabIndex = 21;
            this.ImportSelectedReadmeCheckBox.Text = "Selected readme";
            this.ImportSelectedReadmeCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportTagsCheckBox
            // 
            this.ImportTagsCheckBox.AutoSize = true;
            this.ImportTagsCheckBox.Checked = true;
            this.ImportTagsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ImportTagsCheckBox.Location = new System.Drawing.Point(16, 408);
            this.ImportTagsCheckBox.Name = "ImportTagsCheckBox";
            this.ImportTagsCheckBox.Size = new System.Drawing.Size(50, 17);
            this.ImportTagsCheckBox.TabIndex = 22;
            this.ImportTagsCheckBox.Text = "Tags";
            this.ImportTagsCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportDisabledModsCheckBox
            // 
            this.ImportDisabledModsCheckBox.AutoSize = true;
            this.ImportDisabledModsCheckBox.Checked = true;
            this.ImportDisabledModsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ImportDisabledModsCheckBox.Location = new System.Drawing.Point(16, 392);
            this.ImportDisabledModsCheckBox.Name = "ImportDisabledModsCheckBox";
            this.ImportDisabledModsCheckBox.Size = new System.Drawing.Size(95, 17);
            this.ImportDisabledModsCheckBox.TabIndex = 23;
            this.ImportDisabledModsCheckBox.Text = "Disabled mods";
            this.ImportDisabledModsCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportRatingCheckBox
            // 
            this.ImportRatingCheckBox.AutoSize = true;
            this.ImportRatingCheckBox.Checked = true;
            this.ImportRatingCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ImportRatingCheckBox.Location = new System.Drawing.Point(16, 376);
            this.ImportRatingCheckBox.Name = "ImportRatingCheckBox";
            this.ImportRatingCheckBox.Size = new System.Drawing.Size(57, 17);
            this.ImportRatingCheckBox.TabIndex = 24;
            this.ImportRatingCheckBox.Text = "Rating";
            this.ImportRatingCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportCommentCheckBox
            // 
            this.ImportCommentCheckBox.AutoSize = true;
            this.ImportCommentCheckBox.Checked = true;
            this.ImportCommentCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ImportCommentCheckBox.Location = new System.Drawing.Point(16, 360);
            this.ImportCommentCheckBox.Name = "ImportCommentCheckBox";
            this.ImportCommentCheckBox.Size = new System.Drawing.Size(70, 17);
            this.ImportCommentCheckBox.TabIndex = 25;
            this.ImportCommentCheckBox.Text = "Comment";
            this.ImportCommentCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportLastPlayedCheckBox
            // 
            this.ImportLastPlayedCheckBox.AutoSize = true;
            this.ImportLastPlayedCheckBox.Checked = true;
            this.ImportLastPlayedCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ImportLastPlayedCheckBox.Location = new System.Drawing.Point(16, 344);
            this.ImportLastPlayedCheckBox.Name = "ImportLastPlayedCheckBox";
            this.ImportLastPlayedCheckBox.Size = new System.Drawing.Size(80, 17);
            this.ImportLastPlayedCheckBox.TabIndex = 26;
            this.ImportLastPlayedCheckBox.Text = "Last played";
            this.ImportLastPlayedCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportReleaseDateCheckBox
            // 
            this.ImportReleaseDateCheckBox.AutoSize = true;
            this.ImportReleaseDateCheckBox.Checked = true;
            this.ImportReleaseDateCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ImportReleaseDateCheckBox.Location = new System.Drawing.Point(16, 328);
            this.ImportReleaseDateCheckBox.Name = "ImportReleaseDateCheckBox";
            this.ImportReleaseDateCheckBox.Size = new System.Drawing.Size(89, 17);
            this.ImportReleaseDateCheckBox.TabIndex = 27;
            this.ImportReleaseDateCheckBox.Text = "Release date";
            this.ImportReleaseDateCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportTitleCheckBox
            // 
            this.ImportTitleCheckBox.AutoSize = true;
            this.ImportTitleCheckBox.Checked = true;
            this.ImportTitleCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ImportTitleCheckBox.Location = new System.Drawing.Point(16, 312);
            this.ImportTitleCheckBox.Name = "ImportTitleCheckBox";
            this.ImportTitleCheckBox.Size = new System.Drawing.Size(46, 17);
            this.ImportTitleCheckBox.TabIndex = 28;
            this.ImportTitleCheckBox.Text = "Title";
            this.ImportTitleCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportFromMultipleInisForm
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel_Button;
            this.ClientSize = new System.Drawing.Size(553, 512);
            this.Controls.Add(this.ImportSizeCheckBox);
            this.Controls.Add(this.ImportFinishedOnCheckBox);
            this.Controls.Add(this.ImportSelectedReadmeCheckBox);
            this.Controls.Add(this.ImportTagsCheckBox);
            this.Controls.Add(this.ImportDisabledModsCheckBox);
            this.Controls.Add(this.ImportRatingCheckBox);
            this.Controls.Add(this.ImportCommentCheckBox);
            this.Controls.Add(this.ImportLastPlayedCheckBox);
            this.Controls.Add(this.ImportReleaseDateCheckBox);
            this.Controls.Add(this.ImportTitleCheckBox);
            this.Controls.Add(this.ImportControls);
            this.Controls.Add(this.OKCancelFlowLayoutPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = global::AngelLoader.Properties.Resources.AngelLoader;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ImportFromMultipleInisForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "[Import From Multiple]";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ImportFromMultipleInisForm_FormClosing);
            this.OKCancelFlowLayoutPanel.ResumeLayout(false);
            this.OKCancelFlowLayoutPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Button Cancel_Button;
        private System.Windows.Forms.FlowLayoutPanel OKCancelFlowLayoutPanel;
        private User_FMSel_NDL_ImportControls ImportControls;
        private System.Windows.Forms.CheckBox ImportSizeCheckBox;
        private System.Windows.Forms.CheckBox ImportFinishedOnCheckBox;
        private System.Windows.Forms.CheckBox ImportSelectedReadmeCheckBox;
        private System.Windows.Forms.CheckBox ImportTagsCheckBox;
        private System.Windows.Forms.CheckBox ImportDisabledModsCheckBox;
        private System.Windows.Forms.CheckBox ImportRatingCheckBox;
        private System.Windows.Forms.CheckBox ImportCommentCheckBox;
        private System.Windows.Forms.CheckBox ImportLastPlayedCheckBox;
        private System.Windows.Forms.CheckBox ImportReleaseDateCheckBox;
        private System.Windows.Forms.CheckBox ImportTitleCheckBox;
    }
}