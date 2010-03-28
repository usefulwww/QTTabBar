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
    using Microsoft.Win32;
    using QTTabBarLib.Interop;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Media;
    using System.Runtime.CompilerServices;
    using System.Runtime.Remoting.Messaging;
    using System.Security.Cryptography;
    using System.Text;
    using System.Windows.Forms;

    internal sealed class FileHashComputerForm : Form {
        private Button btnClear;
        private Button btnClose;
        private Button btnRefresh;
        private int cErr_Prv;
        private CheckBox chbClearOnClose;
        private CheckBox chbFullPath;
        private CheckBox chbShowResult;
        private CheckBox chbTopMost;
        private DataGridViewImageColumn clmn1_Icon;
        private DataGridViewTextBoxColumn clmn2_Path;
        private DataGridViewProgressBarColumn clmn3_Hash;
        private static System.Drawing.Color clrNew = System.Drawing.Color.FromArgb(0xff, 0xea, 0xff);
        private int cMatched_Prv;
        private ComboBox cmbHashType;
        private static int colorIndex;
        private static int colorIndexModTimeDiffers = 1;
        private static System.Drawing.Color[] colors = new System.Drawing.Color[] { System.Drawing.Color.FromArgb(0xd1, 0xff, 0xff), System.Drawing.Color.FromArgb(0xd1, 0xff, 0xd1), System.Drawing.Color.FromArgb(0xff, 0xff, 0xd1), System.Drawing.Color.FromArgb(0xff, 0xd1, 0xd1), System.Drawing.Color.FromArgb(0xff, 0xd1, 0xff), System.Drawing.Color.FromArgb(0xd1, 0xd1, 0xff), System.Drawing.Color.FromArgb(0xd1, 0xff, 0xe8), System.Drawing.Color.FromArgb(0xe8, 0xff, 0xd1) };
        private IContainer components;
        private DataGridView dgvHash;
        private Dictionary<string, List<DataGridViewRow>> dicResult = new Dictionary<string, List<DataGridViewRow>>();
        private volatile bool fCancellationPending;
        private volatile int iThreadsCounter;
        private Panel panel1;
        private Queue<PathRowPairs> qPendings = new Queue<PathRowPairs>();
        private static string VALUE_EMPTY = "                                ";
        private static string VALUE_ERROR = "Error. Failed to open?";

        public FileHashComputerForm() {
            this.InitializeComponent();
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar")) {
                if(key != null) {
                    int num = (int)key.GetValue("MD5FormLocation", 0x640064);
                    int num2 = (int)key.GetValue("MD5FormSize", 0xc801c2);
                    int num3 = (int)key.GetValue("HashType", 0);
                    base.Location = new Point((short)(num & 0xffff), (short)((num >> 0x10) & 0xffff));
                    base.Size = new Size(num2 & 0xffff, (num2 >> 0x10) & 0xffff);
                    if((num3 < 0) || (num3 > (this.cmbHashType.Items.Count - 1))) {
                        num3 = 0;
                    }
                    this.cmbHashType.SelectedIndex = num3;
                }
            }
            this.chbFullPath.Checked = QTUtility.CheckConfig(8, 0x10);
            this.chbClearOnClose.Checked = QTUtility.CheckConfig(9, 2);
            this.chbShowResult.Checked = QTUtility.CheckConfig(6, 1);
            this.chbTopMost.Checked = !QTUtility.CheckConfig(7, 0x80);
            PInvoke.DragAcceptFiles(base.Handle, true);
        }

        private void AddDropped(IntPtr hDrop) {
            int capacity = (int)PInvoke.DragQueryFile(hDrop, uint.MaxValue, null, 0);
            if(capacity > 0) {
                List<string> list = new List<string>(capacity);
                for(int i = 0; i < capacity; i++) {
                    StringBuilder lpszFile = new StringBuilder(260);
                    PInvoke.DragQueryFile(hDrop, (uint)i, lpszFile, lpszFile.Capacity);
                    if(File.Exists(lpszFile.ToString())) {
                        list.Add(lpszFile.ToString());
                    }
                }
                if(list.Count > 0) {
                    this.ShowFileHashForm(list.ToArray());
                }
            }
            PInvoke.DragFinish(hDrop);
        }

        private void AsyncComplete(IAsyncResult ar) {
            AsyncResult result = (AsyncResult)ar;
            ((HashInvoker)result.AsyncDelegate).EndInvoke(ar);
            if(base.IsHandleCreated && !base.Disposing) {
                base.BeginInvoke(new MethodInvoker(this.ComputeFinished));
            }
            this.iThreadsCounter--;
        }

        private void buttonClear_Click(object sender, EventArgs e) {
            this.ClearRows();
        }

        private void buttonClose_Click(object sender, EventArgs e) {
            this.qPendings.Clear();
            if(this.iThreadsCounter > 0) {
                this.clmn3_Hash.StopAll();
                this.fCancellationPending = true;
            }
            else {
                if(this.chbClearOnClose.Checked) {
                    this.ClearRows();
                }
                this.SaveMD5FormStat();
                base.Hide();
            }
        }

        private void buttonRefresh_Click(object sender, EventArgs e) {
            List<string> list = new List<string>();
            foreach(DataGridViewRow row in (IEnumerable)this.dgvHash.Rows) {
                if(!list.Contains(row.Cells[1].ToolTipText)) {
                    list.Add(row.Cells[1].ToolTipText);
                }
            }
            this.ClearRows();
            this.ShowFileHashForm(list.ToArray());
        }

        private void checkBoxFullPath_CheckedChanged(object sender, EventArgs e) {
            bool flag = this.chbFullPath.Checked;
            this.dgvHash.SuspendLayout();
            foreach(DataGridViewRow row in (IEnumerable)this.dgvHash.Rows) {
                row.Cells[1].Value = flag ? row.Cells[1].ToolTipText : Path.GetFileName(row.Cells[1].ToolTipText);
            }
            this.dgvHash.ResumeLayout();
        }

        private void checkBoxTopMost_CheckedChanged(object sender, EventArgs e) {
            IntPtr hWndInsertAfter = this.chbTopMost.Checked ? ((IntPtr)(-1)) : ((IntPtr)(-2));
            PInvoke.SetWindowPos(base.Handle, hWndInsertAfter, 0, 0, 0, 0, 0x53);
        }

        private void ClearNewColor() {
            foreach(DataGridViewRow row in (IEnumerable)this.dgvHash.Rows) {
                row.Cells[0].Style.BackColor = System.Drawing.Color.Empty;
            }
        }

        private void ClearRows() {
            this.dgvHash.SuspendLayout();
            this.dgvHash.Rows.Clear();
            this.dgvHash.ResumeLayout();
            colorIndex = 0;
            this.dicResult.Clear();
            this.cMatched_Prv = this.cErr_Prv = 0;
        }

        private void ComputeFinished() {
            if(this.fCancellationPending) {
                this.SetButtonsEnabled(true);
                this.Text = this.MakeHashTypeText();
                this.fCancellationPending = false;
                this.qPendings.Clear();
            }
            else if(this.qPendings.Count > 0) {
                PathRowPairs pairs = this.qPendings.Dequeue();
                this.iThreadsCounter++;
                new HashInvoker(this.ComputeHashCore).BeginInvoke(pairs.Paths.ToArray(), pairs.Rows.ToArray(), this.cmbHashType.SelectedIndex, new AsyncCallback(this.AsyncComplete), null);
            }
            else {
                this.SetButtonsEnabled(true);
                this.Text = this.MakeHashTypeText();
                int num = 0;
                int num2 = 0;
                int count = 0;
                foreach(List<DataGridViewRow> list in this.dicResult.Values) {
                    if(list.Count > 0) {
                        if(list[0].Cells[2].ToolTipText == VALUE_ERROR) {
                            count = list.Count;
                        }
                        if(list.Count > 1) {
                            num2++;
                            num += list.Count;
                        }
                    }
                }
                if(this.chbShowResult.Checked) {
                    string str = (count > 1) ? "s." : ".";
                    if(this.cMatched_Prv != num2) {
                        string str2 = (num > 1) ? "s" : string.Empty;
                        string str3 = (num2 > 1) ? "s" : string.Empty;
                        MessageForm.Show(base.Handle, ((num2 > 0) ? string.Concat(new object[] { num, " file", str2, " matched in ", num2, " ", this.MakeHashTypeText(), str3 }) : string.Empty) + ((count > 0) ? string.Concat(new object[] { (num2 > 0) ? ", " : string.Empty, count, " error", str }) : "."), "Result", MessageBoxIcon.Asterisk, 0x1388);
                    }
                    else if(this.cErr_Prv != count) {
                        MessageForm.Show(base.Handle, count + " error" + str, "Result", MessageBoxIcon.Hand, 0x1388);
                    }
                }
                else if(this.cMatched_Prv != num2) {
                    SystemSounds.Asterisk.Play();
                }
                else if(this.cErr_Prv != count) {
                    SystemSounds.Hand.Play();
                }
                this.cMatched_Prv = num2;
                this.cErr_Prv = count;
            }
        }

        private void ComputeHashCore(string[] paths, DataGridViewRow[] rows, int iHashType) {
            for(int i = 0; i < paths.Length; i++) {
                try {
                    byte[] buffer;
                    DataGridViewProgressBarCell cell = (DataGridViewProgressBarCell)rows[i].Cells[2];
                    using(FileHashStream stream = new FileHashStream(paths[i], new ReportProgressCallback(this.ReportCallbackAsync), cell)) {
                        using(HashAlgorithm algorithm = this.CreateHashAlgorithm(iHashType)) {
                            buffer = algorithm.ComputeHash(stream);
                        }
                        if(stream.IsAborted) {
                            cell.CalculatingStatus = HashCalcStatus.Aborted;
                            return;
                        }
                    }
                    StringBuilder builder = new StringBuilder(buffer.Length);
                    for(int j = 0; j < buffer.Length; j++) {
                        builder.Append(buffer[j].ToString("X2"));
                    }
                    base.Invoke(new HashInvoker2(this.SetRowSync), new object[] { paths[i], builder.ToString(), rows[i] });
                }
                catch {
                    base.Invoke(new HashInvoker2(this.SetRowSync), new object[] { paths[i], VALUE_ERROR, rows[i] });
                }
                if(this.fCancellationPending) {
                    return;
                }
            }
        }

        private HashAlgorithm CreateHashAlgorithm(int index) {
            switch(index) {
                case 1:
                    return new SHA1CryptoServiceProvider();

                case 2:
                    return new SHA256Managed();

                case 3:
                    return new SHA384Managed();

                case 4:
                    return new SHA512Managed();
            }
            return new MD5CryptoServiceProvider();
        }

        private void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e) {
            if((e.RowIndex >= 0) && (e.ColumnIndex > 0)) {
                string toolTipText = this.dgvHash.Rows[e.RowIndex].Cells[1].ToolTipText;
                IntPtr currentHandle = QTUtility.instanceManager.CurrentHandle;
                if(PInvoke.IsWindow(currentHandle)) {
                    QTUtility2.SendCOPYDATASTRUCT(currentHandle, (IntPtr)0xffb, Path.GetDirectoryName(toolTipText), IntPtr.Zero);
                }
            }
        }

        private void dataGridView1_CellStateChanged(object sender, DataGridViewCellStateChangedEventArgs e) {
            if((e.Cell.ColumnIndex == 0) && (e.StateChanged == DataGridViewElementStates.Selected)) {
                this.dgvHash.Rows[e.Cell.RowIndex].Cells[1].Selected = this.dgvHash.Rows[e.Cell.RowIndex].Cells[2].Selected = e.Cell.Selected;
            }
        }

        private void dataGridView1_KeyDown(object sender, KeyEventArgs e) {
            if((e.KeyCode == Keys.Delete) && (this.dgvHash.SelectedCells.Count > 0)) {
                if(this.iThreadsCounter > 0) {
                    e.Handled = true;
                }
                else {
                    List<DataGridViewRow> list = new List<DataGridViewRow>();
                    foreach(DataGridViewCell cell in this.dgvHash.SelectedCells) {
                        DataGridViewRow owningRow = cell.OwningRow;
                        if(!list.Contains(owningRow)) {
                            list.Add(owningRow);
                        }
                    }
                    this.dgvHash.SuspendLayout();
                    foreach(DataGridViewRow row2 in list) {
                        this.dgvHash.Rows.Remove(row2);
                    }
                    this.dgvHash.ResumeLayout();
                    this.dicResult.Clear();
                    this.cMatched_Prv = this.cErr_Prv = 0;
                    foreach(DataGridViewRow row3 in (IEnumerable)this.dgvHash.Rows) {
                        string toolTipText = row3.Cells[2].ToolTipText;
                        if(toolTipText == VALUE_ERROR) {
                            this.cErr_Prv++;
                        }
                        if(!this.dicResult.ContainsKey(toolTipText)) {
                            List<DataGridViewRow> list2 = new List<DataGridViewRow>();
                            list2.Add(row3);
                            this.dicResult[toolTipText] = list2;
                        }
                        else {
                            this.dicResult[toolTipText].Add(row3);
                        }
                    }
                    foreach(List<DataGridViewRow> list3 in this.dicResult.Values) {
                        if(list3.Count > 1) {
                            this.cMatched_Prv++;
                        }
                    }
                }
            }
            else if(e.KeyCode == Keys.Escape) {
                base.Hide();
            }
        }

        private void dataGridView1_MouseDown(object sender, MouseEventArgs e) {
            if((this.dgvHash.HitTest(e.X, e.Y).RowIndex == -1) && (this.dgvHash.Columns[0].HeaderCell.Size.Height < e.Y)) {
                foreach(DataGridViewRow row in (IEnumerable)this.dgvHash.Rows) {
                    row.Selected = false;
                    row.Cells[0].Style.BackColor = System.Drawing.Color.Empty;
                }
            }
        }

        protected override void Dispose(bool disposing) {
            if(base.IsHandleCreated) {
                PInvoke.DragAcceptFiles(base.Handle, false);
            }
            this.fCancellationPending = true;
            while(this.iThreadsCounter > 0) {
                Application.DoEvents();
            }
            if(disposing && (this.components != null)) {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        public void HideHashForm() {
            this.qPendings.Clear();
            if(this.iThreadsCounter > 0) {
                if(!this.chbClearOnClose.Checked) {
                    this.clmn3_Hash.StopAll();
                }
                this.fCancellationPending = true;
            }
            if(this.chbClearOnClose.Checked) {
                this.ClearRows();
            }
            this.SaveMD5FormStat();
            base.Hide();
        }

        private void InitializeComponent() {
            this.dgvHash = new DataGridViewEx();
            this.clmn1_Icon = new DataGridViewImageColumn();
            this.clmn2_Path = new DataGridViewTextBoxColumn();
            this.clmn3_Hash = new DataGridViewProgressBarColumn();
            this.panel1 = new Panel();
            this.btnClear = new Button();
            this.btnClose = new Button();
            this.btnRefresh = new Button();
            this.chbFullPath = new CheckBox();
            this.chbClearOnClose = new CheckBox();
            this.chbShowResult = new CheckBox();
            this.chbTopMost = new CheckBox();
            this.cmbHashType = new ComboBox();
            ((ISupportInitialize)this.dgvHash).BeginInit();
            this.panel1.SuspendLayout();
            base.SuspendLayout();
            this.dgvHash.AllowUserToAddRows = false;
            this.dgvHash.AllowUserToResizeRows = false;
            this.dgvHash.BackgroundColor = SystemColors.Window;
            this.dgvHash.BorderStyle = BorderStyle.None;
            this.dgvHash.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            this.dgvHash.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvHash.Columns.AddRange(new DataGridViewColumn[] { this.clmn1_Icon, this.clmn2_Path, this.clmn3_Hash });
            this.dgvHash.Dock = DockStyle.Fill;
            this.dgvHash.GridColor = SystemColors.ControlLight;
            this.dgvHash.Location = new Point(0, 0);
            this.dgvHash.ReadOnly = true;
            this.dgvHash.RowHeadersVisible = false;
            this.dgvHash.RowTemplate.Height = 0x15;
            this.dgvHash.Size = new Size(0x1bf, 0x6c);
            this.dgvHash.MouseDown += new MouseEventHandler(this.dataGridView1_MouseDown);
            this.dgvHash.KeyDown += new KeyEventHandler(this.dataGridView1_KeyDown);
            this.dgvHash.CellStateChanged += new DataGridViewCellStateChangedEventHandler(this.dataGridView1_CellStateChanged);
            this.dgvHash.CellMouseDoubleClick += new DataGridViewCellMouseEventHandler(this.dataGridView1_CellMouseDoubleClick);
            this.clmn1_Icon.ReadOnly = true;
            this.clmn1_Icon.Resizable = DataGridViewTriState.False;
            this.clmn1_Icon.Width = 0x12;
            this.clmn2_Path.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            this.clmn2_Path.HeaderText = "Path";
            this.clmn2_Path.MinimumWidth = 80;
            this.clmn2_Path.ReadOnly = true;
            this.clmn3_Hash.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.clmn3_Hash.HeaderText = "Hash";
            this.clmn3_Hash.MinimumWidth = 200;
            this.clmn3_Hash.ReadOnly = true;
            this.panel1.Controls.Add(this.cmbHashType);
            this.panel1.Controls.Add(this.chbTopMost);
            this.panel1.Controls.Add(this.chbShowResult);
            this.panel1.Controls.Add(this.chbClearOnClose);
            this.panel1.Controls.Add(this.chbFullPath);
            this.panel1.Controls.Add(this.btnClear);
            this.panel1.Controls.Add(this.btnClose);
            this.panel1.Controls.Add(this.btnRefresh);
            this.panel1.Dock = DockStyle.Bottom;
            this.panel1.Location = new Point(0, 0x6c);
            this.panel1.Size = new Size(0x1bf, 0x5d);
            this.panel1.Paint += new PaintEventHandler(this.panel1_Paint);
            this.btnClose.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this.btnClose.Location = new Point(0x171, 0x43);
            this.btnClose.Size = new Size(0x4b, 0x17);
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new EventHandler(this.buttonClose_Click);
            this.btnClear.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this.btnClear.Location = new Point(0x120, 0x43);
            this.btnClear.Size = new Size(0x4b, 0x17);
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new EventHandler(this.buttonClear_Click);
            this.btnRefresh.Location = new Point(0x89, 8);
            this.btnRefresh.Size = new Size(0x4b, 0x15);
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new EventHandler(this.buttonRefresh_Click);
            this.cmbHashType.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbHashType.Items.AddRange(new object[] { "MD5", "SHA-1", "SHA-256", "SHA-384", "SHA-512" });
            this.cmbHashType.Location = new Point(12, 8);
            this.cmbHashType.Size = new Size(0x79, 0x15);
            this.chbTopMost.AutoSize = true;
            this.chbTopMost.Location = new Point(12, 0x43);
            this.chbTopMost.Size = new Size(0x4b, 0x17);
            this.chbTopMost.Text = "Always on top";
            this.chbTopMost.UseVisualStyleBackColor = true;
            this.chbTopMost.CheckedChanged += new EventHandler(this.checkBoxTopMost_CheckedChanged);
            this.chbShowResult.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this.chbShowResult.AutoSize = true;
            this.chbShowResult.Location = new Point(0xd7, 0x29);
            this.chbShowResult.Size = new Size(0x5e, 0x11);
            this.chbShowResult.Text = "Show result";
            this.chbShowResult.UseVisualStyleBackColor = true;
            this.chbClearOnClose.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this.chbClearOnClose.AutoSize = true;
            this.chbClearOnClose.Location = new Point(0x60, 0x29);
            this.chbClearOnClose.Size = new Size(0x5e, 0x11);
            this.chbClearOnClose.Text = "Clear on close";
            this.chbClearOnClose.UseVisualStyleBackColor = true;
            this.chbFullPath.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this.chbFullPath.AutoSize = true;
            this.chbFullPath.Location = new Point(12, 0x29);
            this.chbFullPath.Size = new Size(0x5e, 0x11);
            this.chbFullPath.Text = "Full path";
            this.chbFullPath.UseVisualStyleBackColor = true;
            this.chbFullPath.CheckedChanged += new EventHandler(this.checkBoxFullPath_CheckedChanged);
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0x1bf, 0xab);
            base.Controls.Add(this.dgvHash);
            base.Controls.Add(this.panel1);
            this.MinimumSize = new Size(320, 0xd5);
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.ShowIcon = false;
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.Manual;
            this.Text = "Hash";
            base.FormClosing += new FormClosingEventHandler(this.MD5Form_FormClosing);
            ((ISupportInitialize)this.dgvHash).EndInit();
            this.panel1.ResumeLayout(false);
            base.ResumeLayout(false);
        }

        private string MakeHashTypeText() {
            switch(this.cmbHashType.SelectedIndex) {
                case 1:
                    return "SHA-1";

                case 2:
                    return "SHA-256";

                case 3:
                    return "SHA-384";

                case 4:
                    return "SHA-512";
            }
            return "MD5";
        }

        private void MD5Form_FormClosing(object sender, FormClosingEventArgs e) {
            if(e.CloseReason == CloseReason.WindowsShutDown) {
                this.SaveMD5FormStat();
            }
            else {
                e.Cancel = true;
                this.HideHashForm();
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e) {
            e.Graphics.DrawLine(SystemPens.ControlDark, 0, 0, this.panel1.Width - 1, 0);
        }

        private bool ReportCallbackAsync(DataGridViewProgressBarCell cell) {
            base.Invoke(new ReportProgressCallback(this.ReportCellProgress), new object[] { cell });
            return this.fCancellationPending;
        }

        private bool ReportCellProgress(DataGridViewProgressBarCell cell) {
            cell.Progress(1);
            return false;
        }

        public void SaveMD5FormStat() {
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar")) {
                key.SetValue("MD5FormLocation", QTUtility2.Make_INT(base.Left, base.Top));
                key.SetValue("MD5FormSize", base.Width | (base.Height << 0x10));
                key.SetValue("HashType", this.cmbHashType.SelectedIndex);
                QTUtility.ConfigValues[8] = this.chbFullPath.Checked ? ((byte)(QTUtility.ConfigValues[8] | 0x10)) : ((byte)(QTUtility.ConfigValues[8] & -17));
                QTUtility.ConfigValues[9] = this.chbClearOnClose.Checked ? ((byte)(QTUtility.ConfigValues[9] | 2)) : ((byte)(QTUtility.ConfigValues[9] & -3));
                QTUtility.ConfigValues[6] = this.chbShowResult.Checked ? ((byte)(QTUtility.ConfigValues[6] | 1)) : ((byte)(QTUtility.ConfigValues[6] & -2));
                QTUtility.ConfigValues[7] = this.chbTopMost.Checked ? ((byte)(QTUtility.ConfigValues[7] & -129)) : ((byte)(QTUtility.ConfigValues[7] | 0x80));
                key.SetValue("Config", QTUtility.ConfigValues);
            }
        }

        private void SetButtonsEnabled(bool fEnabled) {
            this.btnClose.Text = fEnabled ? "Close" : "Stop";
            this.btnClear.Enabled = this.btnRefresh.Enabled = this.cmbHashType.Enabled = fEnabled;
        }

        private void SetCellColorByHash(DataGridViewRow addedRow, string strHash) {
            List<DataGridViewRow> list;
            if(!this.dicResult.ContainsKey(strHash)) {
                list = new List<DataGridViewRow>();
                list.Add(addedRow);
                this.dicResult[strHash] = list;
            }
            else {
                list = this.dicResult[strHash];
                list.Add(addedRow);
            }
            if(list.Count > 1) {
                RowProperties tag = (RowProperties)list[0].Tag;
                if(tag.colorIndex < 0) {
                    foreach(DataGridViewRow row in list) {
                        ((RowProperties)row.Tag).colorIndex = colorIndex;
                        row.Cells[2].Style.BackColor = colors[colorIndex];
                    }
                    colorIndex++;
                    if(colorIndex > (colors.Length - 1)) {
                        colorIndex = 0;
                    }
                }
                else {
                    ((RowProperties)addedRow.Tag).colorIndex = tag.colorIndex;
                    addedRow.Cells[2].Style.BackColor = colors[tag.colorIndex];
                }
            }
        }

        private void SetRowSync(string path, string strHash, DataGridViewRow row) {
            if(this.dgvHash.IsHandleCreated) {
                DataGridViewProgressBarCell cell = (DataGridViewProgressBarCell)row.Cells[2];
                this.clmn3_Hash.FinishProgress(cell);
                if(cell.CalculatingStatus == HashCalcStatus.Finished) {
                    row.Cells[2].Value = strHash;
                    row.Cells[2].ToolTipText = strHash;
                    row.Cells[0].Style.BackColor = clrNew;
                    this.SetCellColorByHash(row, strHash);
                }
            }
        }

        public void ShowFileHashForm(string[] paths) {
            if(!this.fCancellationPending) {
                this.ClearNewColor();
                this.Text = this.MakeHashTypeText() + " (Now calculating...)";
                this.SetButtonsEnabled(false);
                List<string> list = new List<string>();
                List<DataGridViewRow> rows = new List<DataGridViewRow>();
                bool flag = this.chbFullPath.Checked;
                if(paths != null) {
                    this.dgvHash.SuspendLayout();
                    foreach(string str in paths) {
                        FileInfo info = new FileInfo(str);
                        if(info.Exists) {
                            DateTime lastWriteTime = info.LastWriteTime;
                            List<DataGridViewRow> list3 = new List<DataGridViewRow>();
                            List<DataGridViewRow> list4 = new List<DataGridViewRow>();
                            int rowIndex = -1;
                            bool flag2 = false;
                            foreach(DataGridViewRow row in (IEnumerable)this.dgvHash.Rows) {
                                if(!flag2 && string.Equals(row.Cells[1].ToolTipText, str, StringComparison.OrdinalIgnoreCase)) {
                                    if((row.Tag != null) && (lastWriteTime != ((RowProperties)row.Tag).modTime)) {
                                        list3.Add(row);
                                        row.Selected = false;
                                    }
                                    else if(((DataGridViewProgressBarCell)row.Cells[2]).CalculatingStatus == HashCalcStatus.Aborted) {
                                        list4.Add(row);
                                    }
                                    else {
                                        row.Selected = true;
                                        flag2 = true;
                                    }
                                }
                                else {
                                    row.Selected = false;
                                }
                            }
                            foreach(DataGridViewRow row2 in list4) {
                                rowIndex = row2.Index;
                                this.dgvHash.Rows.Remove(row2);
                            }
                            if(!flag2) {
                                DataGridViewRow item = new DataGridViewRow();
                                item.CreateCells(this.dgvHash, new object[] { QTUtility.ImageListGlobal.Images[QTUtility.GetImageKey(str, Path.GetExtension(str))], flag ? str : Path.GetFileName(str), VALUE_EMPTY });
                                item.Cells[0].Style.BackColor = clrNew;
                                item.Cells[1].ToolTipText = str;
                                item.Tag = new RowProperties(lastWriteTime);
                                if(list3.Count > 0) {
                                    RowProperties tag = (RowProperties)list3[0].Tag;
                                    if(tag.colorIndexModTimeDiffers < 0) {
                                        list3.Add(item);
                                        foreach(DataGridViewRow row4 in list3) {
                                            ((RowProperties)row4.Tag).colorIndexModTimeDiffers = colorIndexModTimeDiffers;
                                            row4.Cells[1].Style.BackColor = colors[colorIndexModTimeDiffers];
                                        }
                                        colorIndexModTimeDiffers++;
                                        if(colorIndexModTimeDiffers > (colors.Length - 1)) {
                                            colorIndexModTimeDiffers = 0;
                                        }
                                    }
                                    else {
                                        ((RowProperties)item.Tag).colorIndexModTimeDiffers = tag.colorIndexModTimeDiffers;
                                        item.Cells[1].Style.BackColor = colors[tag.colorIndexModTimeDiffers];
                                    }
                                }
                                if(rowIndex != -1) {
                                    this.dgvHash.Rows.Insert(rowIndex, item);
                                }
                                else {
                                    this.dgvHash.Rows.Add(item);
                                }
                                this.dgvHash.FirstDisplayedScrollingRowIndex = this.dgvHash.Rows.IndexOf(item);
                                list.Add(str);
                                rows.Add(item);
                                this.clmn3_Hash.InitializeProgress((DataGridViewProgressBarCell)item.Cells[2]);
                            }
                        }
                    }
                    if(list.Count > 0) {
                        if(this.iThreadsCounter > 0) {
                            this.qPendings.Enqueue(new PathRowPairs(list, rows));
                        }
                        else {
                            this.iThreadsCounter++;
                            new HashInvoker(this.ComputeHashCore).BeginInvoke(list.ToArray(), rows.ToArray(), this.cmbHashType.SelectedIndex, new AsyncCallback(this.AsyncComplete), null);
                        }
                    }
                    this.dgvHash.ResumeLayout();
                }
                base.Show();
                PInvoke.SetWindowPos(base.Handle, this.chbTopMost.Checked ? ((IntPtr)(-1)) : IntPtr.Zero, 0, 0, 0, 0, 0x53);
                if(list.Count == 0) {
                    this.ComputeFinished();
                }
            }
        }

        protected override void WndProc(ref System.Windows.Forms.Message m) {
            if(m.Msg == 0x233) {
                this.AddDropped(m.WParam);
            }
            base.WndProc(ref m);
        }

        protected override bool ShowWithoutActivation {
            get {
                return true;
            }
        }

        private sealed class FileHashStream : Stream {
            private FileHashComputerForm.ReportProgressCallback callbackCancelAsync;
            private DataGridViewProgressBarCell cell;
            private bool fAborted;
            private bool fEnoughSize;
            private FileStream fs;
            private long lCallbackInterval = 0x64000L;
            private long lCounter;
            private const long MIN_REPORTSIZE = 0x200000L;

            public FileHashStream(string path, FileHashComputerForm.ReportProgressCallback callbackCancelAsync, DataGridViewProgressBarCell cell) {
                this.fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                long length = this.fs.Length;
                this.fEnoughSize = length > 0x200000L;
                this.callbackCancelAsync = callbackCancelAsync;
                this.cell = cell;
                this.cell.FileSize = length;
            }

            protected override void Dispose(bool disposing) {
                if(this.fs != null) {
                    this.fs.Dispose();
                }
                this.callbackCancelAsync = null;
                base.Dispose(disposing);
            }

            public override void Flush() {
            }

            public override int Read(byte[] buffer, int offset, int count) {
                if(this.fEnoughSize) {
                    this.lCounter += count;
                    if(this.lCounter >= this.lCallbackInterval) {
                        this.lCounter = 0L;
                        if((this.callbackCancelAsync != null) && this.callbackCancelAsync(this.cell)) {
                            this.fAborted = true;
                            return 0;
                        }
                    }
                }
                return this.fs.Read(buffer, offset, count);
            }

            public override long Seek(long offset, SeekOrigin origin) {
                return this.fs.Seek(offset, origin);
            }

            public override void SetLength(long value) {
            }

            public override void Write(byte[] buffer, int offset, int count) {
            }

            public override bool CanRead {
                get {
                    return this.fs.CanRead;
                }
            }

            public override bool CanSeek {
                get {
                    return this.fs.CanSeek;
                }
            }

            public override bool CanWrite {
                get {
                    return false;
                }
            }

            public bool IsAborted {
                get {
                    return this.fAborted;
                }
            }

            public override long Length {
                get {
                    return this.fs.Length;
                }
            }

            public override long Position {
                get {
                    return this.fs.Position;
                }
                set {
                    this.fs.Position = value;
                }
            }
        }

        private delegate void HashInvoker(string[] paths, DataGridViewRow[] rows, int iHashType);

        private delegate void HashInvoker2(string path, string md5, DataGridViewRow row);

        private sealed class PathRowPairs {
            public List<string> Paths;
            public List<DataGridViewRow> Rows;

            public PathRowPairs(List<string> paths, List<DataGridViewRow> rows) {
                this.Paths = paths;
                this.Rows = rows;
            }
        }

        private delegate bool ReportProgressCallback(DataGridViewProgressBarCell cell);

        private sealed class RowProperties {
            public int colorIndex;
            public int colorIndexModTimeDiffers;
            public DateTime modTime;

            public RowProperties(DateTime modTime) {
                this.colorIndex = this.colorIndexModTimeDiffers = -1;
                this.modTime = modTime;
            }
        }
    }
}
