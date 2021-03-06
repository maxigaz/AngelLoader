﻿using System.Drawing;
using System.Windows.Forms;
using AngelLoader.DataClasses;

namespace AngelLoader.Forms.CustomControls.Static_LazyLoaded
{
    internal static class Lazy_FMsListZoomButtons
    {
        private static bool _constructed;
        private static ToolStripButtonCustom? ZoomInButton;
        private static ToolStripButtonCustom? ZoomOutButton;
        private static ToolStripButtonCustom? ResetZoomButton;

        private static void Construct(MainForm owner)
        {
            if (_constructed) return;

            // Insert them in reverse order so we always insert at 0
            ResetZoomButton = new ToolStripButtonCustom();
            owner.RefreshAreaToolStrip.Items.Insert(0, ResetZoomButton);
            ResetZoomButton.AutoSize = false;
            ResetZoomButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            ResetZoomButton.Image = Images.ZoomReset;
            ResetZoomButton.Margin = new Padding(0);
            ResetZoomButton.Size = new Size(25, 25);
            ResetZoomButton.Click += owner.FMsListResetZoomButton_Click;

            ZoomOutButton = new ToolStripButtonCustom();
            owner.RefreshAreaToolStrip.Items.Insert(0, ZoomOutButton);
            ZoomOutButton.AutoSize = false;
            ZoomOutButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            ZoomOutButton.Image = Images.ZoomOut;
            ZoomOutButton.Margin = new Padding(0);
            ZoomOutButton.Size = new Size(25, 25);
            ZoomOutButton.Click += owner.FMsListZoomOutButton_Click;

            ZoomInButton = new ToolStripButtonCustom();
            owner.RefreshAreaToolStrip.Items.Insert(0, ZoomInButton);
            ZoomInButton.AutoSize = false;
            ZoomInButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            ZoomInButton.Image = Images.ZoomIn;
            ZoomInButton.Margin = new Padding(0);
            ZoomInButton.Size = new Size(25, 25);
            ZoomInButton.Click += owner.FMsListZoomInButton_Click;

            _constructed = true;

            Localize();
        }

        internal static void Localize()
        {
            if (!_constructed) return;

            ZoomInButton!.ToolTipText = LText.FMsList.ZoomInToolTip;
            ZoomOutButton!.ToolTipText = LText.FMsList.ZoomOutToolTip;
            ResetZoomButton!.ToolTipText = LText.FMsList.ResetZoomToolTip;
        }

        internal static void SetVisible(MainForm owner, bool value)
        {
            if (value)
            {
                Construct(owner);

                ZoomInButton!.Visible = true;
                ZoomOutButton!.Visible = true;
                ResetZoomButton!.Visible = true;
            }
            else
            {
                if (!_constructed) return;

                ZoomInButton!.Visible = false;
                ZoomOutButton!.Visible = false;
                ResetZoomButton!.Visible = false;
            }
        }
    }
}
