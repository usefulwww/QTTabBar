//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2007-2010  Quizo, Paul Accisano
//
//    QTTabBar is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    QTTabBar is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with QTTabBar.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace QTTabBarLib {
    internal sealed class ToolStripSearchBox : ToolStripControlHost {
        private bool fLocked;
        private bool fNowDragging;
        private bool fSuppressTextChangeEvent;
        private const int GRIPWIDTH = 12;
        private const int MAXWIDTH = 0x400;
        private const int MINWIDTH = 0x20;
        private string strDefText;
        private TextBox tb;
        private const int TBSIZE_LARGE = 0x18;
        private const int TBSIZE_SMALL = 0x12;

        public event CancelEventHandler ErasingText;

        public event EventHandler ResizeComplete;

        public ToolStripSearchBox(bool fLarge, bool fLocked, string strDefText, int tbWidth)
            : base(CreateControlInstance(fLarge, strDefText, tbWidth)) {
            AutoSize = false;
            Padding = fLarge ? new Padding(4, 0, 4, 0) : new Padding(2, 0, 2, 0);
            tb = (TextBox)Control;
            this.strDefText = strDefText;
            this.fLocked = fLocked;
        }

        private static Control CreateControlInstance(bool fLarge, string strDefText, int tbWidth) {
            TextBox box = new TextBox();
            box.AutoSize = QTUtility.IsXP;
            box.ForeColor = SystemColors.GrayText;
            box.Text = strDefText;
            box.ImeMode = ImeMode.NoControl;
            box.Size = new Size(tbWidth, fLarge ? 0x18 : 0x12);
            box.Font = new Font(SystemFonts.IconTitleFont.FontFamily, fLarge ? 9f : (!QTUtility.IsXP ? 8.25f : 9f));
            return box;
        }

        private bool IsMouseOnTheEdge(Point pnt) {
            if(pnt.X <= (tb.Width - 12)) {
                return false;
            }
            return ((pnt.Y < tb.Bottom) && (pnt.Y > tb.TabIndex));
        }

        protected override void OnBoundsChanged() {
            base.OnBoundsChanged();
            if((Parent != null) && !Parent.Disposing) {
                Parent.Refresh();
            }
        }

        protected override void OnGotFocus(EventArgs e) {
            if(tb.Text == strDefText) {
                tb.ForeColor = SystemColors.ControlText;
                fSuppressTextChangeEvent = true;
                tb.Text = string.Empty;
                fSuppressTextChangeEvent = false;
            }
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(EventArgs e) {
            fNowDragging = false;
            if((tb.Text.Length == 0) && (ErasingText != null)) {
                CancelEventArgs args = new CancelEventArgs();
                ErasingText(this, args);
                if(!args.Cancel) {
                    fSuppressTextChangeEvent = true;
                    tb.ForeColor = SystemColors.GrayText;
                    tb.Text = strDefText;
                    fSuppressTextChangeEvent = false;
                }
            }
            base.OnLostFocus(e);
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            if((!fLocked && IsMouseOnTheEdge(e.Location)) && (e.Button == MouseButtons.Left)) {
                StartDrag(true);
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseLeave(EventArgs e) {
            fNowDragging = false;
            base.OnMouseLeave(e);
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            if(!fLocked) {
                if(IsMouseOnTheEdge(e.Location)) {
                    tb.Cursor = Cursors.VSplit;
                }
                else {
                    tb.Cursor = Cursors.IBeam;
                }
                if(fNowDragging) {
                    int min = 32;
                    int max = 1024;
                    ToolStrip owner = Owner;
                    if(((owner != null) && !owner.Disposing) && !(owner is ToolStripOverflow)) {
                        max = (owner.DisplayRectangle.Width - Bounds.X) - 24;
                    }
                    Width = Math.Max(Math.Min(e.X + 12, max), min);
                }
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs mevent) {
            if(!fLocked) {
                StartDrag(false);
            }
            base.OnMouseUp(mevent);
        }

        protected override void OnTextChanged(EventArgs e) {
            if(!fSuppressTextChangeEvent) {
                base.OnTextChanged(e);
            }
        }

        public void RefreshText() {
            fSuppressTextChangeEvent = true;
            if(tb.Focused) {
                tb.ForeColor = SystemColors.ControlText;
                tb.Text = string.Empty;
            }
            else {
                tb.ForeColor = SystemColors.GrayText;
                tb.Text = strDefText;
            }
            fSuppressTextChangeEvent = false;
        }

        private void StartDrag(bool fStart) {
            fNowDragging = fStart;
            if(!fStart && (ResizeComplete != null)) {
                ResizeComplete(this, EventArgs.Empty);
            }
        }

        public TextBox TextBox {
            get {
                return tb;
            }
        }
    }
}
