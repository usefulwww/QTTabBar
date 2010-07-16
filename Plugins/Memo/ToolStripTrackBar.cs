//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2010  Quizo, Paul Accisano
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
using System.Drawing;
using System.Windows.Forms;

namespace QuizoPlugins {
    internal sealed class ToolStripTrackBar : ToolStripControlHost {
        private bool blockColorChange;
        private bool fSuppressEvent;

        public event EventHandler ValueChanged;

        public ToolStripTrackBar()
            : base(new TrackBar()) {
            TrackBar control = (TrackBar)base.Control;
            control.MaximumSize = new Size(80, 0x16);
            control.Maximum = 0xff;
            control.Minimum = 20;
            control.Value = 0xff;
            control.SmallChange = 15;
            control.LargeChange = 0x1a;
            control.TickFrequency = 0x1a;
        }

        protected override void OnGotFocus(EventArgs e) {
            if(!this.blockColorChange) {
                base.Control.BackColor = ProfessionalColors.ButtonSelectedHighlight;
            }
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(EventArgs e) {
            if(!this.blockColorChange) {
                base.Control.BackColor = SystemColors.Control;
            }
            base.OnLostFocus(e);
        }

        protected override void OnSubscribeControlEvents(Control control) {
            base.OnSubscribeControlEvents(control);
            TrackBar bar = (TrackBar)control;
            bar.ValueChanged += this.OnValueChange;
        }

        private void OnValueChange(object sender, EventArgs e) {
            if(!this.fSuppressEvent && (this.ValueChanged != null)) {
                this.ValueChanged(this, e);
            }
        }

        public void SetValueWithoutEvent(int value) {
            TrackBar control = (TrackBar)base.Control;
            if((control.Minimum <= value) && (value <= control.Maximum)) {
                this.fSuppressEvent = true;
                control.Value = value;
                this.fSuppressEvent = false;
            }
        }

        public bool BlockColorChange {
            get {
                return this.blockColorChange;
            }
            set {
                this.blockColorChange = value;
            }
        }

        public int Value {
            get {
                return ((TrackBar)base.Control).Value;
            }
            set {
                if((value < 20) || (value > 0xff)) {
                    ((TrackBar)base.Control).Value = 0xff;
                }
                else {
                    ((TrackBar)base.Control).Value = value;
                }
            }
        }
    }
}
