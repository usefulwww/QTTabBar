namespace QuizoPlugins {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Windows.Forms;

    internal sealed class MemoForm : Form {
        private Button button1;
        private ToolStripMenuItem colorToolStripMenuItem;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private IContainer components;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem copyToolStripMenuItem;
        private string currentPath = string.Empty;
        private ToolStripMenuItem cutToolStripMenuItem;
        private ToolStripMenuItem defaultFontToolStripMenuItem;
        private ToolStripMenuItem defaultToolStripMenuItem;
        private ToolStripMenuItem deleteToolStripMenuItem;
        private bool fFirstLoadComplete;
        private bool fNowShown;
        private List<Font> fontList = new List<Font>();
        private ListView listView1;
        private static int NUM_LVTEXTWIDTH = 100;
        private Memo owner;
        private ToolStripMenuItem pasteToolStripMenuItem;
        private static string PATH_DAT = (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Quizo\QTTabBar\1.0.9\memo.dat");
        private RichTextBox richTextBox1;
        private Dictionary<string, string> rtfDic;
        private ToolStripMenuItem searchToolStripMenuItem;
        private TextBox textBox1;
        private ToolStripSeparator toolStripSeparator1;
        private QuizoPlugins.ToolStripTrackBar toolStripTrackBar;
        private Dictionary<string, string> txtDic;
        
        [Serializable]
        public sealed class MemoStore {
            private Rectangle _bounds;
            private Dictionary<string, string> _dic;
            private double _opacity;
            private Dictionary<string, string> _txtDic;

            public Rectangle Bounds {
                get {
                    return this._bounds;
                }
                set {
                    this._bounds = value;
                }
            }

            public Dictionary<string, string> Dictionary {
                get {
                    return this._dic;
                }
                set {
                    this._dic = value;
                }
            }

            public double Opacity {
                get {
                    return this._opacity;
                }
                set {
                    this._opacity = value;
                }
            }

            public Dictionary<string, string> TxtDictionary {
                get {
                    return this._txtDic;
                }
                set {
                    this._txtDic = value;
                }
            }
        }
    
        public MemoForm(Memo memo) {
            this.owner = memo;
            this.InitializeComponent();
            this.toolStripTrackBar = new QuizoPlugins.ToolStripTrackBar();
            this.toolStripTrackBar.BlockColorChange = true;
            this.toolStripTrackBar.ValueChanged += new EventHandler(this.toolStripTrackBar_ValueChanged);
            this.contextMenuStrip1.Items.Add(this.toolStripTrackBar);
        }

        private void button1_Click(object sender, EventArgs e) {
            if(this.textBox1.TextLength == 0) {
                this.CreateMemoList();
            }
            else {
                string text = this.textBox1.Text;
                List<string> list = new List<string>();
                foreach(string str2 in this.txtDic.Keys) {
                    if(this.txtDic[str2].IndexOf(text, StringComparison.OrdinalIgnoreCase) != -1) {
                        list.Add(str2);
                    }
                }
                this.listView1.BeginUpdate();
                this.listView1.Items.Clear();
                foreach(string str3 in list) {
                    string str4 = this.txtDic[str3];
                    if(str4.Length > NUM_LVTEXTWIDTH) {
                        str4 = str4.Substring(0, NUM_LVTEXTWIDTH) + "...";
                    }
                    ListViewItem item = new ListViewItem(new string[] { str3, str4 });
                    this.listView1.Items.Add(item);
                }
                this.listView1.EndUpdate();
            }
        }

        public bool ContainsPath(string path) {
            if(this.rtfDic == null) {
                this.LoadDB();
            }
            return this.rtfDic.ContainsKey(path);
        }

        private void contextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            if(e.ClickedItem == this.colorToolStripMenuItem) {
                ColorDialog dialog = new ColorDialog();
                if(this.richTextBox1.SelectionColor != Color.Empty) {
                    dialog.Color = this.richTextBox1.SelectionColor;
                }
                if(dialog.ShowDialog() == DialogResult.OK) {
                    this.richTextBox1.SelectionColor = dialog.Color;
                }
                dialog.Dispose();
            }
            else if(e.ClickedItem == this.defaultToolStripMenuItem) {
                this.richTextBox1.SelectionColor = SystemColors.WindowText;
            }
            else if(e.ClickedItem == this.defaultFontToolStripMenuItem) {
                this.richTextBox1.SelectionFont = this.richTextBox1.Font;
            }
            else if(e.ClickedItem == this.cutToolStripMenuItem) {
                this.richTextBox1.Cut();
            }
            else if(e.ClickedItem == this.copyToolStripMenuItem) {
                this.richTextBox1.Copy();
            }
            else if(e.ClickedItem == this.pasteToolStripMenuItem) {
                this.richTextBox1.Paste();
            }
            else if(e.ClickedItem == this.deleteToolStripMenuItem) {
                this.richTextBox1.SelectedText = string.Empty;
            }
            else if(e.ClickedItem == this.searchToolStripMenuItem) {
                if(this.richTextBox1.CanUndo) {
                    this.FixCurrent();
                }
                this.CreateMemoList();
                this.richTextBox1.Visible = false;
                this.Refresh();
                this.textBox1.Focus();
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e) {
            if(this.richTextBox1.SelectionLength < 1) {
                this.colorToolStripMenuItem.Visible = this.defaultToolStripMenuItem.Visible = this.defaultFontToolStripMenuItem.Visible = false;
                this.toolStripTrackBar.Visible = true;
                this.cutToolStripMenuItem.Enabled = this.copyToolStripMenuItem.Enabled = this.deleteToolStripMenuItem.Enabled = false;
            }
            else {
                this.colorToolStripMenuItem.Visible = this.defaultToolStripMenuItem.Visible = this.defaultFontToolStripMenuItem.Visible = true;
                this.toolStripTrackBar.Visible = false;
                this.cutToolStripMenuItem.Enabled = this.copyToolStripMenuItem.Enabled = this.deleteToolStripMenuItem.Enabled = true;
            }
        }

        private void CreateMemoList() {
            this.listView1.BeginUpdate();
            this.listView1.Items.Clear();
            foreach(string str in this.txtDic.Keys) {
                string str2 = this.txtDic[str];
                if(str2.Length > NUM_LVTEXTWIDTH) {
                    str2 = str2.Substring(0, NUM_LVTEXTWIDTH) + "...";
                }
                ListViewItem item = new ListViewItem(new string[] { str, str2 });
                item.ToolTipText = str;
                this.listView1.Items.Add(item);
            }
            this.listView1.EndUpdate();
        }

        protected override void Dispose(bool disposing) {
            this.owner = null;
            if(disposing && (this.components != null)) {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void FixCurrent() {
            if(this.richTextBox1.TextLength > 0) {
                this.rtfDic[this.currentPath] = this.richTextBox1.Rtf;
                this.txtDic[this.currentPath] = this.richTextBox1.Text;
            }
            else {
                this.rtfDic.Remove(this.currentPath);
                this.txtDic.Remove(this.currentPath);
            }
            this.SaveDB();
        }

        public void GiveFocus() {
            if(!this.richTextBox1.Visible) {
                this.richTextBox1.Visible = true;
            }
            this.richTextBox1.Focus();
        }

        public void HideMemoForm() {
            try {
                if(this.fNowShown) {
                    this.fNowShown = false;
                    base.Hide();
                    if(this.richTextBox1.CanUndo) {
                        this.FixCurrent();
                    }
                }
                foreach(Font font in this.fontList) {
                    font.Dispose();
                }
                this.fontList.Clear();
            }
            catch(Exception) {
            }
        }

        private void InitializeComponent() {
            this.components = new Container();
            this.richTextBox1 = new RichTextBox();
            this.contextMenuStrip1 = new ContextMenuStrip(this.components);
            this.cutToolStripMenuItem = new ToolStripMenuItem();
            this.copyToolStripMenuItem = new ToolStripMenuItem();
            this.pasteToolStripMenuItem = new ToolStripMenuItem();
            this.deleteToolStripMenuItem = new ToolStripMenuItem();
            this.searchToolStripMenuItem = new ToolStripMenuItem();
            this.toolStripSeparator1 = new ToolStripSeparator();
            this.colorToolStripMenuItem = new ToolStripMenuItem();
            this.defaultToolStripMenuItem = new ToolStripMenuItem();
            this.defaultFontToolStripMenuItem = new ToolStripMenuItem();
            this.listView1 = new ListView();
            this.columnHeader1 = new ColumnHeader();
            this.columnHeader2 = new ColumnHeader();
            this.textBox1 = new TextBox();
            this.button1 = new Button();
            this.contextMenuStrip1.SuspendLayout();
            base.SuspendLayout();
            this.richTextBox1.AcceptsTab = true;
            this.richTextBox1.AutoWordSelection = true;
            this.richTextBox1.BorderStyle = BorderStyle.FixedSingle;
            this.richTextBox1.ContextMenuStrip = this.contextMenuStrip1;
            this.richTextBox1.Dock = DockStyle.Fill;
            this.richTextBox1.HideSelection = false;
            this.richTextBox1.ImeMode = ImeMode.On;
            this.richTextBox1.Location = new Point(0, 0);
            this.richTextBox1.ScrollBars = RichTextBoxScrollBars.Vertical;
            this.richTextBox1.Size = new Size(0xe1, 0xe1);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.TabStop = false;
            this.richTextBox1.Text = "";
            this.richTextBox1.LinkClicked += new LinkClickedEventHandler(this.richTextBox1_LinkClicked);
            this.richTextBox1.KeyDown += new KeyEventHandler(this.richTextBox1_KeyDown);
            this.contextMenuStrip1.Items.AddRange(new ToolStripItem[] { this.cutToolStripMenuItem, this.copyToolStripMenuItem, this.pasteToolStripMenuItem, this.deleteToolStripMenuItem, this.searchToolStripMenuItem, this.toolStripSeparator1, this.colorToolStripMenuItem, this.defaultToolStripMenuItem, this.defaultFontToolStripMenuItem });
            this.contextMenuStrip1.ShowImageMargin = false;
            this.contextMenuStrip1.Size = new Size(0x80, 0xd0);
            this.contextMenuStrip1.Opening += new CancelEventHandler(this.contextMenuStrip1_Opening);
            this.contextMenuStrip1.ItemClicked += new ToolStripItemClickedEventHandler(this.contextMenuStrip1_ItemClicked);
            this.cutToolStripMenuItem.Size = new Size(0x7f, 0x16);
            this.cutToolStripMenuItem.Text = "Cu&t";
            this.copyToolStripMenuItem.Size = new Size(0x7f, 0x16);
            this.copyToolStripMenuItem.Text = "&Copy";
            this.pasteToolStripMenuItem.Size = new Size(0x7f, 0x16);
            this.pasteToolStripMenuItem.Text = "&Paste";
            this.deleteToolStripMenuItem.Size = new Size(0x7f, 0x16);
            this.deleteToolStripMenuItem.Text = "&Delete";
            this.searchToolStripMenuItem.Size = new Size(0x7f, 0x16);
            this.searchToolStripMenuItem.Text = "&Search";
            this.toolStripSeparator1.Size = new Size(0x7c, 6);
            this.colorToolStripMenuItem.Size = new Size(0x7f, 0x16);
            this.colorToolStripMenuItem.Text = "Co&lor";
            this.defaultToolStripMenuItem.Size = new Size(0x7f, 0x16);
            this.defaultToolStripMenuItem.Text = "D&efault color";
            this.defaultFontToolStripMenuItem.Size = new Size(0x7f, 0x16);
            this.defaultFontToolStripMenuItem.Text = "Default &font";
            this.listView1.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this.listView1.Columns.AddRange(new ColumnHeader[] { this.columnHeader1, this.columnHeader2 });
            this.listView1.FullRowSelect = true;
            this.listView1.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            this.listView1.Location = new Point(12, 0x29);
            this.listView1.MultiSelect = false;
            this.listView1.ShowGroups = false;
            this.listView1.ShowItemToolTips = true;
            this.listView1.Size = new Size(0xc9, 0xac);
            this.listView1.TabIndex = 1;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = View.Details;
            this.listView1.ItemActivate += new EventHandler(this.listView1_ItemActivate);
            this.columnHeader1.Text = "Path";
            this.columnHeader1.Width = 0x5d;
            this.columnHeader2.Text = "Text";
            this.columnHeader2.Width = 500;
            this.textBox1.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.textBox1.Location = new Point(12, 14);
            this.textBox1.Size = new Size(0x83, 20);
            this.textBox1.TabIndex = 2;
            this.textBox1.KeyPress += new KeyPressEventHandler(this.textBox1_KeyPress);
            this.button1.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this.button1.Location = new Point(0x95, 12);
            this.button1.Size = new Size(0x40, 0x17);
            this.button1.TabIndex = 3;
            this.button1.Text = "Search";
            this.button1.Click += new EventHandler(this.button1_Click);
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0xe1, 0xe1);
            base.Controls.Add(this.richTextBox1);
            base.Controls.Add(this.listView1);
            base.Controls.Add(this.button1);
            base.Controls.Add(this.textBox1);
            base.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "MemoForm";
            base.Opacity = 0.85;
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.Manual;
            this.Text = "Memo";
            base.FormClosing += new FormClosingEventHandler(this.MemoForm_FormClosing);
            this.contextMenuStrip1.ResumeLayout(false);
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void listView1_ItemActivate(object sender, EventArgs e) {
            string text = this.listView1.SelectedItems[0].Text;
            if(Directory.Exists(text)) {
                this.owner.OpenDirectory(text);
            }
        }

        private void LoadDB() {
            if(File.Exists(PATH_DAT)) {
                try {
                    using(FileStream stream = new FileStream(PATH_DAT, FileMode.Open)) {
                        BinaryFormatter formatter = new BinaryFormatter();
                        MemoStore store = (MemoStore)formatter.Deserialize(stream);
                        this.rtfDic = store.Dictionary;
                        base.Bounds = store.Bounds;
                        if(store.Opacity > 0.2) {
                            base.Opacity = store.Opacity;
                        }
                        this.txtDic = store.TxtDictionary;
                    }
                }
                catch(Exception) {
                }
            }
            else {
                base.Bounds = new Rectangle(Point.Empty, base.Size);
            }
            if(this.rtfDic == null) {
                StringComparer currentCultureIgnoreCase = StringComparer.CurrentCultureIgnoreCase;
                this.rtfDic = new Dictionary<string, string>(currentCultureIgnoreCase);
                this.txtDic = new Dictionary<string, string>(currentCultureIgnoreCase);
            }
        }

        private void MemoForm_FormClosing(object sender, FormClosingEventArgs e) {
            if(e.CloseReason != CloseReason.WindowsShutDown) {
                e.Cancel = true;
                if(this.richTextBox1.Visible) {
                    this.HideMemoForm();
                }
                else {
                    this.richTextBox1.Visible = true;
                    this.richTextBox1.Focus();
                }
            }
        }

        private void richTextBox1_KeyDown(object sender, KeyEventArgs e) {
            if(e.Modifiers == Keys.Control) {
                if(e.KeyCode == Keys.Add) {
                    Font selectionFont = this.richTextBox1.SelectionFont;
                    if(selectionFont == null) {
                        selectionFont = this.Font;
                    }
                    Font item = new Font(this.Font.FontFamily, selectionFont.Size + 0.75f);
                    this.richTextBox1.SelectionFont = item;
                    this.fontList.Add(item);
                }
                else if(e.KeyCode == Keys.Subtract) {
                    Font font = this.richTextBox1.SelectionFont;
                    if(font == null) {
                        font = this.Font;
                    }
                    if(font.Size > 6f) {
                        Font font4 = new Font(this.Font.FontFamily, font.Size - 0.75f);
                        this.richTextBox1.SelectionFont = font4;
                        this.fontList.Add(font4);
                    }
                }
                else if(e.KeyCode == Keys.F) {
                    if(this.richTextBox1.CanUndo) {
                        this.FixCurrent();
                    }
                    IntPtr hWnd = PInvoke.SendMessage(this.listView1.Handle, 0x104e, IntPtr.Zero, IntPtr.Zero);
                    if(hWnd != IntPtr.Zero) {
                        PInvoke.SetWindowPos(hWnd, new IntPtr(-1), 0, 0, 0, 0, 0x13);
                    }
                    this.CreateMemoList();
                    this.richTextBox1.Visible = false;
                    this.Refresh();
                    this.textBox1.Focus();
                }
            }
        }

        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e) {
            Process.Start(e.LinkText);
        }

        private void SaveDB() {
            MemoStore graph = new MemoStore();
            graph.Bounds = base.Bounds;
            graph.Dictionary = this.rtfDic;
            graph.TxtDictionary = this.txtDic;
            graph.Opacity = base.Opacity;
            if(!Directory.Exists(Path.GetDirectoryName(PATH_DAT))) {
                Directory.CreateDirectory(Path.GetDirectoryName(PATH_DAT));
            }
            BinaryFormatter formatter = new BinaryFormatter();
            using(FileStream stream = new FileStream(PATH_DAT, FileMode.Create)) {
                formatter.Serialize(stream, graph);
            }
        }

        public void ShowMemoForm(string path) {
            try {
                if(!this.fFirstLoadComplete) {
                    this.LoadDB();
                    this.toolStripTrackBar.Value = (int)(base.Opacity * 255.0);
                    this.toolStripTrackBar.BackColor = ProfessionalColors.MenuItemPressedGradientBegin;
                    this.fFirstLoadComplete = true;
                }
                if(this.fNowShown && this.richTextBox1.CanUndo) {
                    this.FixCurrent();
                }
                this.currentPath = path;
                if(path.Length > 3) {
                    this.Text = Path.GetFileName(path);
                }
                else {
                    this.Text = path;
                }
                this.richTextBox1.SuspendLayout();
                this.richTextBox1.Clear();
                if(this.rtfDic.ContainsKey(this.currentPath)) {
                    this.richTextBox1.Rtf = this.rtfDic[this.currentPath];
                }
                this.richTextBox1.Visible = true;
                this.richTextBox1.ResumeLayout();
                if(!this.fNowShown) {
                    base.Show();
                    PInvoke.SetWindowPos(base.Handle, (IntPtr)(-1), 0, 0, 0, 0, 0x53);
                    this.fNowShown = true;
                }
            }
            catch(Exception) {
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e) {
            if(e.KeyChar == '\r') {
                e.Handled = true;
                this.button1.PerformClick();
            }
        }

        private void toolStripTrackBar_ValueChanged(object sender, EventArgs e) {
            base.Opacity = ((double)this.toolStripTrackBar.Value) / 255.0;
        }

        protected override bool ShowWithoutActivation {
            get {
                return true;
            }
        }
    }
}
