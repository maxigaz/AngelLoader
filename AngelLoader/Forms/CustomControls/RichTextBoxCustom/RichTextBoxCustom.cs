﻿using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls
{
    internal sealed partial class RichTextBoxCustom : RichTextBox
    {
        #region Private fields / properties

        private Font? _monospaceFont;
        private Font MonospaceFont => _monospaceFont ??= new Font(FontFamily.GenericMonospace, 10.0f);

        private bool _contentIsPlainText;
        private bool ContentIsPlainText
        {
            get => _contentIsPlainText;
            set
            {
                _contentIsPlainText = value;
                if (_contentIsPlainText)
                {
                    SetFontTypeInternal(Config.ReadmeUseFixedWidthFont, false);
                }
                else
                {
                    ResetFont();
                }
            }
        }

        #endregion

        public RichTextBoxCustom() => InitWorkarounds();

        #region Private methods

        private void SetFontTypeInternal(bool useFixed, bool outsideCall)
        {
            if (!ContentIsPlainText) return;

            try
            {
                if (outsideCall)
                {
                    SaveZoom();
                    this.SuspendDrawing();
                }

                Font = useFixed ? MonospaceFont : DefaultFont;

                string savedText = Text;

                if (outsideCall)
                {
                    Clear();
                    ResetScrollInfo();
                }

                // We have to reload because links don't get recognized until we do
                Text = savedText;
            }
            finally
            {
                if (outsideCall)
                {
                    RestoreZoom();
                    this.ResumeDrawing();
                }
            }
        }

        #endregion

        #region API methods

        #region Zoom stuff

        internal void SetAndStoreZoomFactor(float zoomFactor)
        {
            SetStoredZoomFactorClamped(zoomFactor);
            SetZoomFactorClamped(zoomFactor);
        }

        internal void ZoomIn()
        {
            try
            {
                SetZoomFactorClamped(ZoomFactor + 0.1f);
            }
            catch (ArgumentException)
            {
                // leave it as is
            }
        }

        internal void ZoomOut()
        {
            try
            {
                SetZoomFactorClamped(ZoomFactor - 0.1f);
            }
            catch (ArgumentException)
            {
                // leave it as is
            }
        }

        internal void ResetZoomFactor()
        {
            this.SuspendDrawing();

            // We have to set another value first, or it won't take.
            ZoomFactor = 1.1f;
            ZoomFactor = 1.0f;

            this.ResumeDrawing();
        }

        #endregion

        internal void SetFontType(bool useFixed) => SetFontTypeInternal(useFixed, outsideCall: true);

        #region Load content

        /// <summary>
        /// Sets the text without resetting the zoom factor.
        /// </summary>
        /// <param name="text"></param>
        internal void SetText(string text)
        {
            SaveZoom();

            try
            {
                this.SuspendDrawing();

                // Blank the text to reset the scroll position to the top
                Clear();
                ResetScrollInfo();

                ContentIsPlainText = true;
                if (!text.IsEmpty()) Text = text;

                RestoreZoom();
            }
            finally
            {
                this.ResumeDrawing();
            }
        }

        /// <summary>
        /// Loads a file into the box without resetting the zoom factor.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileType"></param>
        internal void LoadContent(string path, ReadmeType fileType)
        {
            AssertR(fileType != ReadmeType.HTML, nameof(fileType) + " is ReadmeType.HTML");

            SaveZoom();

            try
            {
                this.SuspendDrawing();

                // On Windows 10 at least, images don't display if we're ReadOnly. Why not. We need to be ReadOnly
                // though - it doesn't make sense to let the user edit a readme - so un-set us just long enough
                // to load in the content correctly, then set us back again.
                ReadOnly = false;

                // Blank the text to reset the scroll position to the top
                Clear();
                ResetScrollInfo();

                switch (fileType)
                {
                    case ReadmeType.GLML:
                        string text = File.ReadAllText(path);
                        // This resets the font if false, so don't do it after the load or it messes up the RTF.
                        ContentIsPlainText = false;
                        Rtf = GLMLToRTF(text);
                        break;
                    case ReadmeType.RichText:
                        // Use ReadAllBytes and byte[] search, because ReadAllText and string.Replace is ~30x slower
                        byte[] bytes = File.ReadAllBytes(path);

                        ReplaceByteSequence(bytes, _shppict, _shppictBlanked);
                        ReplaceByteSequence(bytes, _nonshppict, _nonshppictBlanked);

                        // Ditto the above
                        ContentIsPlainText = false;
                        using (var ms = new MemoryStream(bytes)) LoadFile(ms, RichTextBoxStreamType.RichText);
                        break;
                    case ReadmeType.PlainText:
                        ContentIsPlainText = true;
                        // Load the file ourselves so we can do encoding detection. Otherwise it just loads with
                        // frigging whatever (default system encoding maybe?)
                        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                        {
                            var fe = new FMScanner.SimpleHelpers.FileEncoding();
                            Encoding enc = fe.DetectFileEncoding(fs, Encoding.Default);

                            fs.Position = 0;

                            using var sr = new StreamReader(fs, enc);
                            Text = sr.ReadToEnd();
                        }
                        break;
                }
            }
            finally
            {
                ReadOnly = true;
                RestoreZoom();
                this.ResumeDrawing();
            }
        }

        #endregion

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeWorkarounds();
                _monospaceFont?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
