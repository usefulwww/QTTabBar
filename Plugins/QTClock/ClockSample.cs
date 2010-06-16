using System;
using System.Windows.Forms;
using System.Drawing;

using QTPlugin;
using QTPlugin.Interop;

namespace QuizoPlugins {
    [Plugin(PluginType.Interactive, Author = "Quizo", Name = "SampleClock", Version = "0.9.0.0", Description = "Sample clock plugin")]
    public class ClockSample : IBarCustomItem {
        private ToolStripLabel labelTime;
        private Timer timer;
        private bool fOn;

        #region IPluginClient Members

        public void Open(IPluginServer pluginServer, IShellBrowser shellBrowser) {
        }

        public void Close(EndCode code) {
            if(this.timer != null) {
                this.timer.Dispose();
            }

            if(this.labelTime != null) {
                this.labelTime.Dispose();
            }
        }

        public bool QueryShortcutKeys(out string[] actions) {
            actions = null;
            return false;
        }


        public void OnMenuItemClick(MenuType menuType, string menuText, ITab tab) {
        }

        public bool HasOption {
            get {
                return false;
            }
        }

        public void OnOption() {
        }

        public void OnShortcutKeyPressed(int iIndex) {
        }

        #endregion


        #region IBarCustomItem Members

        public ToolStripItem CreateItem(bool fLarge, DisplayStyle displayStyle) {
            if(this.labelTime == null) {
                this.labelTime = new ToolStripLabel();
                this.labelTime.DisplayStyle = ToolStripItemDisplayStyle.Text;
                this.labelTime.AutoSize = true;
                this.labelTime.Font = new Font("Courier New", this.labelTime.Font.SizeInPoints);
                this.labelTime.Alignment = ToolStripItemAlignment.Right;
                this.labelTime.Padding = new Padding(0, 0, 24, 0);
            }

            if(this.timer == null) {
                this.timer = new Timer();
                this.timer.Interval = 1000;
                this.timer.Tick += new EventHandler(timer_Tick);
            }

            this.timer.Start();

            return this.labelTime;
        }

        #endregion


        private void timer_Tick(object sender, EventArgs e) {
            DateTime dt = DateTime.Now;

            int h = dt.Hour;
            int m = dt.Minute;
            string sep = this.fOn ? " " : ":";

            this.labelTime.Text = h + sep + (m < 10 ? "0" : String.Empty) + m;
            this.labelTime.ToolTipText = dt.ToLongDateString();

            this.fOn = !this.fOn;
        }
    }
}
