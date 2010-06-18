namespace QuizoPlugins {
    using System;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;

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
            bar.ValueChanged += new EventHandler(this.OnValueChange);
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
