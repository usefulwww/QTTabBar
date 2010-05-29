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

namespace QTTabBarLib {
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;

    internal sealed class ToolStripSearchBox : ToolStripControlHost {
        private bool fLocked;
        private bool fNowDragging;
        private bool fSuppressTextChangeEvent;
        private const int GRIPWIDTH = 12;
        private const int MAXWIDTH = 0x400;
        private const int MINWIDTH = 0x20;
        private string strDefText;
        private System.Windows.Forms.TextBox tb;
        private const int TBSIZE_LARGE = 0x18;
        private const int TBSIZE_SMALL = 0x12;

        public event CancelEventHandler ErasingText;

        public event EventHandler ResizeComplete;

        public ToolStripSearchBox(bool fLarge, bool fLocked, string strDefText, int tbWidth)
            : base(CreateControlInstance(fLarge, strDefText, tbWidth)) {
            base.AutoSize = false;
            this.Padding = fLarge ? new Padding(4, 0, 4, 0) : new Padding(2, 0, 2, 0);
            this.tb = (System.Windows.Forms.TextBox)base.Control;
            this.strDefText = strDefText;
            this.fLocked = fLocked;
        }

        private static Control CreateControlInstance(bool fLarge, string strDefText, int tbWidth) {
            System.Windows.Forms.TextBox box = new System.Windows.Forms.TextBox();
            box.AutoSize = !QTUtility.IsVista;
            box.ForeColor = SystemColors.GrayText;
            box.Text = strDefText;
            box.ImeMode = ImeMode.NoControl;
            box.Size = new Size(tbWidth, fLarge ? 0x18 : 0x12);
            box.Font = new Font(SystemFonts.IconTitleFont.FontFamily, fLarge ? 9f : (QTUtility.IsVista ? 8.25f : 9f));
            return box;
        }

        private bool IsMouseOnTheEdge(Point pnt) {
            if(pnt.X <= (this.tb.Width - 12)) {
                return false;
            }
            return ((pnt.Y < this.tb.Bottom) && (pnt.Y > this.tb.TabIndex));
        }

        protected override void OnBoundsChanged() {
            base.OnBoundsChanged();
            if((base.Parent != null) && !base.Parent.Disposing) {
                base.Parent.Refresh();
            }
        }

        protected override void OnGotFocus(EventArgs e) {
            if(this.tb.Text == this.strDefText) {
                this.tb.ForeColor = SystemColors.ControlText;
                this.fSuppressTextChangeEvent = true;
                this.tb.Text = string.Empty;
                this.fSuppressTextChangeEvent = false;
            }
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(EventArgs e) {
            this.fNowDragging = false;
            if((this.tb.Text.Length == 0) && (this.ErasingText != null)) {
                CancelEventArgs args = new CancelEventArgs();
                this.ErasingText(this, args);
                if(!args.Cancel) {
                    this.fSuppressTextChangeEvent = true;
                    this.tb.ForeColor = SystemColors.GrayText;
                    this.tb.Text = this.strDefText;
                    this.fSuppressTextChangeEvent = false;
                }
            }
            base.OnLostFocus(e);
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            if((!this.fLocked && this.IsMouseOnTheEdge(e.Location)) && (e.Button == MouseButtons.Left)) {
                this.StartDrag(true);
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseLeave(EventArgs e) {
            this.fNowDragging = false;
            base.OnMouseLeave(e);
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            if(!this.fLocked) {
                if(this.IsMouseOnTheEdge(e.Location)) {
                    this.tb.Cursor = Cursors.VSplit;
                }
                else {
                    this.tb.Cursor = Cursors.IBeam;
                }
                if((this.fNowDragging && (0x20 <= e.X)) && (e.X <= 0x400)) {
                    int num = 0x400;
                    ToolStrip owner = base.Owner;
                    if(((owner != null) && !owner.Disposing) && !(owner is ToolStripOverflow)) {
                        num = (owner.DisplayRectangle.Width - this.Bounds.X) - 0x18;
                    }
                    base.Width = Math.Min(e.X + 12, num);
                }
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs mevent) {
            if(!this.fLocked) {
                this.StartDrag(false);
            }
            base.OnMouseUp(mevent);
        }

        protected override void OnTextChanged(EventArgs e) {
            if(!this.fSuppressTextChangeEvent) {
                base.OnTextChanged(e);
            }
        }

        public void RefreshText() {
            this.fSuppressTextChangeEvent = true;
            if(this.tb.Focused) {
                this.tb.ForeColor = SystemColors.ControlText;
                this.tb.Text = string.Empty;
            }
            else {
                this.tb.ForeColor = SystemColors.GrayText;
                this.tb.Text = this.strDefText;
            }
            this.fSuppressTextChangeEvent = false;
        }

        private void StartDrag(bool fStart) {
            this.fNowDragging = fStart;
            if(!fStart && (this.ResizeComplete != null)) {
                this.ResizeComplete(this, EventArgs.Empty);
            }
        }

        public System.Windows.Forms.TextBox TextBox {
            get {
                return this.tb;
            }
        }
    }
}
