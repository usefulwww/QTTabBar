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
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
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

        public event PluginInfoHasOption QueryPluginInfoHasOption;

        public PluginView() {
            AutoScroll = true;
            BackColor = SystemColors.Window;
            ColumnCount = 1;
            ColumnStyles.Add(new ColumnStyle());
            Padding = new Padding(0, 0, 6, 0);
            RowCount = 1;
            RowStyles.Add(new RowStyle());
            BorderStyle = BorderStyle.FixedSingle;
            VisibleChanged += PluginView_VisibleChanged;
        }

        public void AddPluginViewItem(PluginInformation pi, PluginAssembly pa) {
            SuspendLayout();
            PluginViewItem control = new PluginViewItem(pi, pa);
            control.MouseDown += pvi_MouseDown;
            control.MouseUp += pvi_MouseUp;
            control.DoubleClick += pvi_DoubleClick;
            control.OptionButtonClick += pvi_OptionButtonClick;
            control.DisableButtonClick += pvi_DisableButtonClick;
            control.RemoveButtonClick += pvi_RemoveButtonClick;
            int count = Controls.Count;
            RowStyles.Insert(count, new RowStyle(SizeType.Absolute, 55f));
            Controls.Add(control, 0, count);
            RowCount = Controls.Count + 1;
            ResumeLayout();
            for(int i = 0; i < Controls.Count; i++) {
                SetRow(Controls[i], i);
            }
        }

        private void contextMenuStripPlugin_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            PluginViewItem sourceControl = contextMenuStripPlugin.SourceControl as PluginViewItem;
            if(sourceControl != null) {
                if(e.ClickedItem == tsmiOption) {
                    ShowPluginOption(sourceControl);
                }
                else if(e.ClickedItem == tsmiPluginAbout) {
                    if(PluginAboutRequired != null) {
                        PluginAboutRequired(this, new PluginOptionEventArgs(sourceControl));
                    }
                }
                else if(e.ClickedItem == tsmiDisable) {
                    sourceControl.PluginEnabled = !sourceControl.PluginEnabled;
                }
                else if(e.ClickedItem == tsmiUninstall) {
                    contextMenuStripPlugin.Close(ToolStripDropDownCloseReason.ItemClicked);
                    RemovePluginViewItem(sourceControl, false);
                }
            }
        }

        private void CreateContextMenu() {
            contextMenuStripPlugin = new ContextMenuStrip();
            tsmiOption = new ToolStripMenuItem();
            tsmiPluginAbout = new ToolStripMenuItem();
            tss = new ToolStripSeparator();
            tsmiUninstall = new ToolStripMenuItem();
            tsmiDisable = new ToolStripMenuItem();
            contextMenuStripPlugin.SuspendLayout();
            tsmiOption.Text = BTN_OPTION;
            tsmiUninstall.Text = BTN_REMOVE;
            tsmiDisable.Text = BTN_DISABLE;
            tss.Enabled = false;
            contextMenuStripPlugin.Items.AddRange(new ToolStripItem[] { tsmiOption, tsmiPluginAbout, tss, tsmiUninstall, tsmiDisable });
            contextMenuStripPlugin.ShowImageMargin = false;
            contextMenuStripPlugin.Size = new Size(120, 0x62);
            contextMenuStripPlugin.ItemClicked += contextMenuStripPlugin_ItemClicked;
            contextMenuStripPlugin.ResumeLayout(false);
        }

        protected override void Dispose(bool disposing) {
            if(contextMenuStripPlugin != null) {
                contextMenuStripPlugin.Dispose();
                contextMenuStripPlugin = null;
            }
            if(IsHandleCreated) {
                PInvoke.DragAcceptFiles(Handle, false);
            }
            base.Dispose(disposing);
        }

        public void NotifyApplied() {
            for(int i = 0; i < Controls.Count; i++) {
                ((PluginViewItem)Controls[i]).Applied();
            }
        }

        private bool PluginInfoHasOption(PluginInformation pi) {
            return ((QueryPluginInfoHasOption != null) && QueryPluginInfoHasOption(pi));
        }

        private void PluginView_VisibleChanged(object sender, EventArgs e) {
            if(Visible) {
                VisibleChanged -= PluginView_VisibleChanged;
                PInvoke.DragAcceptFiles(Handle, true);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            if((keyData == Keys.Up) || (keyData == Keys.Left)) {
                if(iSelectedIndex > 0) {
                    SelectPlugin(iSelectedIndex - 1);
                }
                return true;
            }
            if((keyData == Keys.Down) || (keyData == Keys.Right)) {
                if(iSelectedIndex < (Controls.Count - 1)) {
                    SelectPlugin(iSelectedIndex + 1);
                }
                return true;
            }
            if(keyData == Keys.Home) {
                if(Controls.Count > 0) {
                    SelectPlugin(0);
                }
                return true;
            }
            if(keyData == Keys.End) {
                if(Controls.Count > 0) {
                    SelectPlugin(Controls.Count - 1);
                }
                return true;
            }
            if(((keyData == Keys.Apps) && (Controls.Count > 0)) && ((-1 < iSelectedIndex) && (iSelectedIndex < Controls.Count))) {
                PluginViewItem control = (PluginViewItem)Controls[iSelectedIndex];
                if(contextMenuStripPlugin == null) {
                    CreateContextMenu();
                }
                tsmiPluginAbout.Text = string.Format(MNU_PLUGINABOUT, control.Name);
                tsmiOption.Enabled = control.HasOption && control.PluginEnabled;
                tsmiDisable.Text = control.PluginEnabled ? BTN_DISABLE : BTN_ENABLE;
                contextMenuStripPlugin.Show(control, 2, control.Height + 2);
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
                ShowPluginOption(pvi);
            }
        }

        private void pvi_MouseDown(object sender, MouseEventArgs e) {
            int index = Controls.IndexOf((PluginViewItem)sender);
            if(index != -1) {
                SelectPlugin(index);
            }
        }

        private void pvi_MouseUp(object sender, MouseEventArgs e) {
            if(e.Button == MouseButtons.Right) {
                PluginViewItem control = (PluginViewItem)sender;
                if(contextMenuStripPlugin == null) {
                    CreateContextMenu();
                }
                tsmiPluginAbout.Text = string.Format(MNU_PLUGINABOUT, control.Name);
                tsmiOption.Enabled = control.HasOption && control.PluginEnabled;
                tsmiDisable.Text = control.PluginEnabled ? BTN_DISABLE : BTN_ENABLE;
                contextMenuStripPlugin.Show(control, e.Location);
            }
        }

        private void pvi_OptionButtonClick(object sender, EventArgs e) {
            ShowPluginOption((PluginViewItem)sender);
        }

        private void pvi_RemoveButtonClick(object sender, EventArgs e) {
            RemovePluginViewItem((PluginViewItem)sender, false);
        }

        public void RemovePluginsRange(PluginInformation[] pis) {
            SuspendLayout();
            try {
                List<PluginViewItem> list = Controls.OfType<PluginViewItem>()
                    .Where(item => pis.Contains(item.PluginInfo)).ToList();
                foreach(PluginViewItem item2 in list) {
                    RemovePluginViewItem(item2, true);
                }
            }
            finally {
                ResumeLayout();
            }
        }

        private bool RemovePluginViewItem(PluginViewItem pvi, bool noRaiseEvent) {
            bool flag;
            SuspendLayout();
            try {
                if(!noRaiseEvent) {
                    PluginOptionEventArgs e = new PluginOptionEventArgs(pvi);
                    if(PluginRemoved != null) {
                        PluginRemoved(this, e);
                        if(e.Cancel) {
                            return false;
                        }
                    }
                }
                int index = Controls.IndexOf(pvi);
                if(iSelectedIndex == index) {
                    iSelectedIndex = -1;
                }
                if(index > -1) {
                    Controls.RemoveAt(index);
                    RowStyles.RemoveAt(index);
                    RowCount--;
                    for(int i = 0; i < Controls.Count; i++) {
                        SetRow(Controls[i], i);
                    }
                    pvi.Dispose();
                    return true;
                }
                flag = false;
            }
            finally {
                ResumeLayout();
            }
            return flag;
        }

        public void SelectPlugin(int index) {
            iSelectedIndex = index;
            PluginViewItem activeControl = null;
            SuspendLayout();
            for(int i = 0; i < Controls.Count; i++) {
                PluginViewItem item2 = (PluginViewItem)Controls[i];
                if(i == index) {
                    activeControl = item2;
                    bool hasOption = item2.HasOption;
                    if(!item2.HasOptionQueried) {
                        hasOption = PluginInfoHasOption(item2.PluginInfo);
                    }
                    RowStyles[i].Height = Math.Min(item2.SelectPlugin(hasOption), 134f);
                    item2.Invalidate();
                }
                else {
                    item2.UnselectPlugin();
                    RowStyles[i].Height = 55f;
                }
            }
            ResumeLayout();
            if(activeControl != null) {
                ScrollControlIntoView(activeControl);
            }
        }

        private void ShowPluginOption(PluginViewItem pvi) {
            if(PluginOptionRequired != null) {
                PluginOptionRequired(this, new PluginOptionEventArgs(pvi));
            }
        }

        protected override void WndProc(ref Message m) {
            if((m.Msg == WM.DROPFILES) && (DragDropEx != null)) {
                int num = (int)PInvoke.DragQueryFile(m.WParam, uint.MaxValue, null, 0);
                try {
                    if(num > 0) {
                        lstFilePaths.Clear();
                        for(int i = 0; i < num; i++) {
                            StringBuilder lpszFile = new StringBuilder(260);
                            PInvoke.DragQueryFile(m.WParam, (uint)i, lpszFile, lpszFile.Capacity);
                            lstFilePaths.Add(lpszFile.ToString());
                        }
                        if(lstFilePaths.Count > 0) {
                            DragDropEx(this, EventArgs.Empty);
                        }
                    }
                }
                finally {
                    PInvoke.DragFinish(m.WParam);
                    lstFilePaths.Clear();
                }
            }
            else {
                base.WndProc(ref m);
            }
        }

        public IEnumerable<string> DroppedFiles {
            get {
                return lstFilePaths;
            }
        }

        public int ItemsCount {
            get {
                return Controls.Count;
            }
        }

        public IEnumerable<PluginViewItem> PluginViewItems {
            get {
                //<get_PluginViewItems>d__0 d__ = new <get_PluginViewItems>d__0(-2);
                //d__.<>4__this = this;
                //return d__;

                for(int i = 0; i < Controls.Count; i++) {
                    yield return (PluginViewItem)Controls[i];
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
