﻿using System.Windows.Forms;
using AngelLoader.DataClasses;

namespace AngelLoader.Forms.CustomControls.Static_LazyLoaded
{
    internal static class ViewHTMLReadmeLLButton
    {
        private static bool _constructed;
        private static Button? Button;

        internal static void Localize()
        {
            if (_constructed) Button!.SetTextAutoSize(LText.ReadmeArea.ViewHTMLReadme);
        }

        internal static void Center(Control parent)
        {
            if (_constructed) Button!.CenterHV(parent);
        }

        internal static bool Visible => _constructed && Button!.Visible;

        internal static void Hide()
        {
            if (_constructed) Button!.Hide();
        }

        internal static void Show(MainForm owner)
        {
            if (!_constructed)
            {
                var container = owner.MainSplitContainer.Panel2;

                Button = new Button();
                container.Controls.Add(Button);
                Button.Anchor = AnchorStyles.None;
                Button.AutoSize = true;
                // This thing gets centered later so no location is specified here
                Button.Padding = new Padding(6, 0, 6, 0);
                Button.Height = 23;
                Button.TabIndex = 49;
                Button.UseVisualStyleBackColor = true;
                Button.Visible = false;
                Button.Click += owner.ViewHTMLReadmeButton_Click;
                Button.MouseLeave += owner.ReadmeArea_MouseLeave;

                _constructed = true;

                Localize();
                Button.CenterHV(container);
            }

            Button!.Show();
        }
    }
}
