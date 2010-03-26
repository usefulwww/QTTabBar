//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2010  Paul Accisano
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
    using QTTabBarLib.Interop;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class PluginView : TableLayoutPanel {
        public static string BTN_DISABLE = "Disable";
        public static string BTN_ENABLE = "Enable";
        public static string BTN_OPTION = "Option";
        public static string BTN_REMOVE = "Uninstall";
        private ContextMenuStrip contextMenuStripPlugin;
        private int iSelectedIndex = -1;
        private List<string> lstFilePaths = new List<string>();
        public static string MNU_PLUGINABOUT = "About {0}";
        private ToolStripMenuItem tsmiDisable;
        private ToolStripMenuItem tsmiOption;
        private ToolStripMenuItem tsmiPluginAbout;
        private ToolStripMenuItem tsmiUninstall;
        private ToolStripSeparator tss;

        public event EventHandler DragDropEx;

        public event PluginOptionEventHandler PluginAboutRequired;

        public event PluginOptionEventHandler PluginOptionRequired;

        public event PluginOptionEventHandler PluginRemoved;

        public event QTTabBarLib.PluginInfoHasOption QueryPluginInfoHasOption;

        public PluginView() {
            base.AutoScroll = true;
            base.BackColor = SystemColors.Window;
            base.ColumnCount = 1;
            base.ColumnStyles.Add(new ColumnStyle());
            base.Padding = new Padding(0, 0, 6, 0);
            base.RowCount = 1;
            base.RowStyles.Add(new RowStyle());
            base.BorderStyle = BorderStyle.FixedSingle;
            base.VisibleChanged += new EventHandler(this.PluginView_VisibleChanged);
        }

        public void AddPluginViewItem(PluginInformation pi, PluginAssembly pa) {
            base.SuspendLayout();
            PluginViewItem control = new PluginViewItem(pi, pa);
            control.MouseDown += new MouseEventHandler(this.pvi_MouseDown);
            control.MouseUp += new MouseEventHandler(this.pvi_MouseUp);
            control.DoubleClick += new EventHandler(this.pvi_DoubleClick);
            control.OptionButtonClick += new EventHandler(this.pvi_OptionButtonClick);
            control.DisableButtonClick += new EventHandler(this.pvi_DisableButtonClick);
            control.RemoveButtonClick += new EventHandler(this.pvi_RemoveButtonClick);
            int count = base.Controls.Count;
            base.RowStyles.Insert(count, new RowStyle(SizeType.Absolute, 55f));
            base.Controls.Add(control, 0, count);
            base.RowCount = base.Controls.Count + 1;
            base.ResumeLayout();
            for(int i = 0; i < base.Controls.Count; i++) {
                base.SetRow(base.Controls[i], i);
            }
        }

        private void contextMenuStripPlugin_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            PluginViewItem sourceControl = this.contextMenuStripPlugin.SourceControl as PluginViewItem;
            if(sourceControl != null) {
                if(e.ClickedItem == this.tsmiOption) {
                    this.ShowPluginOption(sourceControl);
                }
                else if(e.ClickedItem == this.tsmiPluginAbout) {
                    if(this.PluginAboutRequired != null) {
                        this.PluginAboutRequired(this, new PluginOptionEventArgs(sourceControl));
                    }
                }
                else if(e.ClickedItem == this.tsmiDisable) {
                    sourceControl.PluginEnabled = !sourceControl.PluginEnabled;
                }
                else if(e.ClickedItem == this.tsmiUninstall) {
                    this.contextMenuStripPlugin.Close(ToolStripDropDownCloseReason.ItemClicked);
                    this.RemovePluginViewItem(sourceControl, false);
                }
            }
        }

        private void CreateContextMenu() {
            this.contextMenuStripPlugin = new ContextMenuStrip();
            this.tsmiOption = new ToolStripMenuItem();
            this.tsmiPluginAbout = new ToolStripMenuItem();
            this.tss = new ToolStripSeparator();
            this.tsmiUninstall = new ToolStripMenuItem();
            this.tsmiDisable = new ToolStripMenuItem();
            this.contextMenuStripPlugin.SuspendLayout();
            this.tsmiOption.Text = BTN_OPTION;
            this.tsmiUninstall.Text = BTN_REMOVE;
            this.tsmiDisable.Text = BTN_DISABLE;
            this.tss.Enabled = false;
            this.contextMenuStripPlugin.Items.AddRange(new ToolStripItem[] { this.tsmiOption, this.tsmiPluginAbout, this.tss, this.tsmiUninstall, this.tsmiDisable });
            this.contextMenuStripPlugin.ShowImageMargin = false;
            this.contextMenuStripPlugin.Size = new Size(120, 0x62);
            this.contextMenuStripPlugin.ItemClicked += new ToolStripItemClickedEventHandler(this.contextMenuStripPlugin_ItemClicked);
            this.contextMenuStripPlugin.ResumeLayout(false);
        }

        protected override void Dispose(bool disposing) {
            if(this.contextMenuStripPlugin != null) {
                this.contextMenuStripPlugin.Dispose();
                this.contextMenuStripPlugin = null;
            }
            if(base.IsHandleCreated) {
                PInvoke.DragAcceptFiles(base.Handle, false);
            }
            base.Dispose(disposing);
        }

        public void NotifyApplied() {
            for(int i = 0; i < base.Controls.Count; i++) {
                ((PluginViewItem)base.Controls[i]).Applied();
            }
        }

        private bool PluginInfoHasOption(PluginInformation pi) {
            return ((this.QueryPluginInfoHasOption != null) && this.QueryPluginInfoHasOption(pi));
        }

        private void PluginView_VisibleChanged(object sender, EventArgs e) {
            if(base.Visible) {
                base.VisibleChanged -= new EventHandler(this.PluginView_VisibleChanged);
                PInvoke.DragAcceptFiles(base.Handle, true);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            if((keyData == Keys.Up) || (keyData == Keys.Left)) {
                if(this.iSelectedIndex > 0) {
                    this.SelectPlugin(this.iSelectedIndex - 1);
                }
                return true;
            }
            if((keyData == Keys.Down) || (keyData == Keys.Right)) {
                if(this.iSelectedIndex < (base.Controls.Count - 1)) {
                    this.SelectPlugin(this.iSelectedIndex + 1);
                }
                return true;
            }
            if(keyData == Keys.Home) {
                if(base.Controls.Count > 0) {
                    this.SelectPlugin(0);
                }
                return true;
            }
            if(keyData == Keys.End) {
                if(base.Controls.Count > 0) {
                    this.SelectPlugin(base.Controls.Count - 1);
                }
                return true;
            }
            if(((keyData == Keys.Apps) && (base.Controls.Count > 0)) && ((-1 < this.iSelectedIndex) && (this.iSelectedIndex < base.Controls.Count))) {
                PluginViewItem control = (PluginViewItem)base.Controls[this.iSelectedIndex];
                if(this.contextMenuStripPlugin == null) {
                    this.CreateContextMenu();
                }
                this.tsmiPluginAbout.Text = string.Format(MNU_PLUGINABOUT, control.Name);
                this.tsmiOption.Enabled = control.HasOption && control.PluginEnabled;
                this.tsmiDisable.Text = control.PluginEnabled ? BTN_DISABLE : BTN_ENABLE;
                this.contextMenuStripPlugin.Show(control, 2, control.Height + 2);
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void pvi_DisableButtonClick(object sender, EventArgs e) {
            PluginViewItem item = (PluginViewItem)sender;
            item.PluginEnabled = !item.PluginEnabled;
        }

        private void pvi_DoubleClick(object sender, EventArgs e) {
            PluginViewItem pvi = (PluginViewItem)sender;
            if(pvi.HasOption) {
                this.ShowPluginOption(pvi);
            }
        }

        private void pvi_MouseDown(object sender, MouseEventArgs e) {
            int index = base.Controls.IndexOf((PluginViewItem)sender);
            if(index != -1) {
                this.SelectPlugin(index);
            }
        }

        private void pvi_MouseUp(object sender, MouseEventArgs e) {
            if(e.Button == MouseButtons.Right) {
                PluginViewItem control = (PluginViewItem)sender;
                if(this.contextMenuStripPlugin == null) {
                    this.CreateContextMenu();
                }
                this.tsmiPluginAbout.Text = string.Format(MNU_PLUGINABOUT, control.Name);
                this.tsmiOption.Enabled = control.HasOption && control.PluginEnabled;
                this.tsmiDisable.Text = control.PluginEnabled ? BTN_DISABLE : BTN_ENABLE;
                this.contextMenuStripPlugin.Show(control, e.Location);
            }
        }

        private void pvi_OptionButtonClick(object sender, EventArgs e) {
            this.ShowPluginOption((PluginViewItem)sender);
        }

        private void pvi_RemoveButtonClick(object sender, EventArgs e) {
            this.RemovePluginViewItem((PluginViewItem)sender, false);
        }

        public void RemovePluginsRange(PluginInformation[] pis) {
            base.SuspendLayout();
            try {
                List<PluginViewItem> list = new List<PluginViewItem>();
                foreach(Control control in base.Controls) {
                    PluginViewItem item = control as PluginViewItem;
                    if((item != null) && (Array.IndexOf<PluginInformation>(pis, item.PluginInfo) != -1)) {
                        list.Add(item);
                    }
                }
                foreach(PluginViewItem item2 in list) {
                    this.RemovePluginViewItem(item2, true);
                }
            }
            finally {
                base.ResumeLayout();
            }
        }

        private bool RemovePluginViewItem(PluginViewItem pvi, bool noRaiseEvent) {
            bool flag;
            base.SuspendLayout();
            try {
                if(!noRaiseEvent) {
                    PluginOptionEventArgs e = new PluginOptionEventArgs(pvi);
                    if(this.PluginRemoved != null) {
                        this.PluginRemoved(this, e);
                        if(e.Cancel) {
                            return false;
                        }
                    }
                }
                int index = base.Controls.IndexOf(pvi);
                if(this.iSelectedIndex == index) {
                    this.iSelectedIndex = -1;
                }
                if(index > -1) {
                    base.Controls.RemoveAt(index);
                    base.RowStyles.RemoveAt(index);
                    base.RowCount--;
                    for(int i = 0; i < base.Controls.Count; i++) {
                        base.SetRow(base.Controls[i], i);
                    }
                    pvi.Dispose();
                    return true;
                }
                flag = false;
            }
            finally {
                base.ResumeLayout();
            }
            return flag;
        }

        public void SelectPlugin(int index) {
            this.iSelectedIndex = index;
            PluginViewItem activeControl = null;
            base.SuspendLayout();
            for(int i = 0; i < base.Controls.Count; i++) {
                PluginViewItem item2 = (PluginViewItem)base.Controls[i];
                if(i == index) {
                    activeControl = item2;
                    bool hasOption = item2.HasOption;
                    if(!item2.HasOptionQueried) {
                        hasOption = this.PluginInfoHasOption(item2.PluginInfo);
                    }
                    base.RowStyles[i].Height = Math.Min(item2.SelectPlugin(hasOption), 134f);
                    item2.Invalidate();
                }
                else {
                    item2.UnselectPlugin();
                    base.RowStyles[i].Height = 55f;
                }
            }
            base.ResumeLayout();
            if(activeControl != null) {
                base.ScrollControlIntoView(activeControl);
            }
        }

        private void ShowPluginOption(PluginViewItem pvi) {
            if(this.PluginOptionRequired != null) {
                this.PluginOptionRequired(this, new PluginOptionEventArgs(pvi));
            }
        }

        protected override void WndProc(ref Message m) {
            if((m.Msg == 0x233) && (this.DragDropEx != null)) {
                int num = (int)PInvoke.DragQueryFile(m.WParam, uint.MaxValue, null, 0);
                try {
                    if(num > 0) {
                        this.lstFilePaths.Clear();
                        for(int i = 0; i < num; i++) {
                            StringBuilder lpszFile = new StringBuilder(260);
                            PInvoke.DragQueryFile(m.WParam, (uint)i, lpszFile, lpszFile.Capacity);
                            this.lstFilePaths.Add(lpszFile.ToString());
                        }
                        if(this.lstFilePaths.Count > 0) {
                            this.DragDropEx(this, EventArgs.Empty);
                        }
                    }
                }
                finally {
                    PInvoke.DragFinish(m.WParam);
                    this.lstFilePaths.Clear();
                }
            }
            else {
                base.WndProc(ref m);
            }
        }

        public IEnumerable<string> DroppedFiles {
            get {
                return this.lstFilePaths;
            }
        }

        public int ItemsCount {
            get {
                return base.Controls.Count;
            }
        }

        public IEnumerable<PluginViewItem> PluginViewItems {
            get {
                //<get_PluginViewItems>d__0 d__ = new <get_PluginViewItems>d__0(-2);
                //d__.<>4__this = this;
                //return d__;

                for(int i = 0; i < this.Controls.Count; i++) {
                    yield return (PluginViewItem)this.Controls[i];
                }
            }
        }

#if false
    [CompilerGenerated]
    private sealed class <get_PluginViewItems>d__0 : IEnumerator<PluginViewItem>, IEnumerable<PluginViewItem>, IEnumerable, IEnumerator, IDisposable
    {
      private int <>1__state;
      private PluginViewItem <>2__current;
      public PluginView <>4__this;
      public int <i>5__1;
      
      [DebuggerHidden]
      public <get_PluginViewItems>d__0(int <>1__state)
      {
        this.<>1__state = <>1__state;
      }
      
      private bool MoveNext()
      {
        switch (this.<>1__state)
        {
          case 0:
            this.<>1__state = -1;
            this.<i>5__1 = 0;
            break;
          
          case 1:
            this.<>1__state = -1;
            this.<i>5__1++;
            break;
          
          default:
            goto Label_007E;
        }
        if (this.<i>5__1 < this.<>4__this.Controls.Count)
        {
          this.<>2__current = (PluginViewItem) this.<>4__this.Controls[this.<i>5__1];
          this.<>1__state = 1;
          return true;
        }
      Label_007E:
        return false;
      }
      
      [DebuggerHidden]
      IEnumerator<PluginViewItem> IEnumerable<PluginViewItem>.GetEnumerator()
      {
        if (Interlocked.CompareExchange(ref this.<>1__state, 0, -2) == -2)
        {
          return this;
        }
        PluginView.<get_PluginViewItems>d__0 d__ = new PluginView.<get_PluginViewItems>d__0(0);
        d__.<>4__this = this.<>4__this;
        return d__;
      }
      
      [DebuggerHidden]
      IEnumerator IEnumerable.GetEnumerator()
      {
        return this.System.Collections.Generic.IEnumerable<QTTabBarLib.PluginViewItem>.GetEnumerator();
      }
      
      [DebuggerHidden]
      void IEnumerator.Reset()
      {
        throw new NotSupportedException();
      }
      
      void IDisposable.Dispose()
      {
      }
      
      PluginViewItem IEnumerator<PluginViewItem>.Current
      {
        [DebuggerHidden]
        get
        {
          return this.<>2__current;
        }
      }
      
      object IEnumerator.Current
      {
        [DebuggerHidden]
        get
        {
          return this.<>2__current;
        }
      }
    }
#endif

    }
}
