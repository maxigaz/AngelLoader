﻿using System.Windows.Forms;
using AngelLoader.Forms.CustomControls;
using static AngelLoader.Logger;
using static AngelLoader.Misc;

namespace AngelLoader.Forms
{
    public partial class MainForm
    {
        // You know the drill
#pragma warning disable IDE0069 // Disposable fields should be disposed

        // Not great code really, but works.

        private ProgressPanel? ProgressBox;

        private void ConstructProgressBox()
        {
            if (ProgressBox != null) return;

            ProgressBox = new ProgressPanel();
            Controls.Add(ProgressBox);
            ProgressBox.Inject(this);
            ProgressBox.Localize();
            ProgressBox.Anchor = AnchorStyles.None;
        }

        private void LocalizeProgressBox() => ProgressBox?.Localize();

        internal void EnableEverything(bool enabled)
        {
            bool doFocus = !EverythingPanel.Enabled && enabled;

            EverythingPanel.Enabled = enabled;

            if (!doFocus) return;

            // The "mouse wheel scroll without needing to focus" thing stops working when no control is focused
            // (this happens when we disable and enable EverythingPanel). Therefore, we need to give focus to a
            // control here. One is as good as the next, but FMsDGV seems like a sensible choice.
            FMsDGV.Focus();
            FMsDGV.SelectProperly();
        }

        public void ShowProgressBox(ProgressTasks progressTask, bool suppressShow = false)
        {
            ConstructProgressBox();

            Log(nameof(ShowProgressBox) + ": " + progressTask, methodName: false);
            ProgressBox!.ShowProgressWindow(progressTask, suppressShow);
        }

        public void ReportScanProgress(int fmNumber, int fmsTotal, int percent, string fmName)
        {
            ConstructProgressBox();
            ProgressBox!.ReportScanProgress(fmNumber, fmsTotal, percent, fmName);
        }

        public void ReportFMExtractProgress(int percent)
        {
            ConstructProgressBox();
            ProgressBox!.ReportFMExtractProgress(percent);
        }

        public void ReportCachingProgress(int percent)
        {
            ConstructProgressBox();
            ProgressBox!.ReportCachingProgress(percent);
        }

        public void SetCancelingFMInstall()
        {
            ConstructProgressBox();
            ProgressBox!.SetCancelingFMInstall();
        }

        public void HideProgressBox()
        {
            ConstructProgressBox();
            ProgressBox!.HideThis();
        }
    }
}
