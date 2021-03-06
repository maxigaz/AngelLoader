﻿using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace AngelLoader.Forms.CustomControls
{
    public class ContextMenuStripCustom : ContextMenuStrip
    {
        private bool _preventClose;
        private ToolStripMenuItem[]? _preventCloseItems;

        public ContextMenuStripCustom() { }

        public ContextMenuStripCustom(IContainer container) : base(container) { }

        internal void SetPreventCloseOnClickItems(params ToolStripMenuItem[] items) => _preventCloseItems = items;

        protected override void OnItemClicked(ToolStripItemClickedEventArgs e)
        {
            _preventClose = _preventCloseItems.Contains(e.ClickedItem) && ((ToolStripMenuItem)e.ClickedItem).CheckOnClick;

            base.OnItemClicked(e);
        }

        protected override void OnClosing(ToolStripDropDownClosingEventArgs e)
        {
            if (_preventClose)
            {
                _preventClose = false;
                e.Cancel = true;
                return;
            }

            base.OnClosing(e);
        }
    }
}
