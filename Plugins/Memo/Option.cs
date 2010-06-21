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

namespace QuizoPlugins {
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    public class Option : Form {
        private Button buttonCancel;
        private Button buttonOK;
        private ComboBox comboBoxMode;
        private IContainer components;

        public Option() {
            this.InitializeComponent();
            this.comboBoxMode.Items.AddRange(new string[] { Memo.ResStrs[1], Memo.ResStrs[2], Memo.ResStrs[3] });
            if((Memo.ConfigValues[0] & 0x80) == 0x80) {
                this.comboBoxMode.SelectedIndex = ((Memo.ConfigValues[0] & 0x40) == 0x40) ? 2 : 1;
            }
            else {
                this.comboBoxMode.SelectedIndex = 0;
            }
        }

        protected override void Dispose(bool disposing) {
            if(disposing && (this.components != null)) {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent() {
            this.comboBoxMode = new ComboBox();
            this.buttonOK = new Button();
            this.buttonCancel = new Button();
            base.SuspendLayout();
            this.comboBoxMode.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboBoxMode.FormattingEnabled = true;
            this.comboBoxMode.Location = new Point(0x25, 0x1d);
            this.comboBoxMode.Name = "comboBoxMode";
            this.comboBoxMode.Size = new Size(0xb2, 0x15);
            this.comboBoxMode.TabIndex = 1;
            this.buttonOK.DialogResult = DialogResult.OK;
            this.buttonOK.Location = new Point(0x55, 0x48);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new Size(0x4b, 0x17);
            this.buttonOK.TabIndex = 2;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonCancel.DialogResult = DialogResult.Cancel;
            this.buttonCancel.Location = new Point(0xa6, 0x48);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new Size(0x4b, 0x17);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            base.AcceptButton = this.buttonOK;
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.CancelButton = this.buttonCancel;
            base.ClientSize = new Size(0xfd, 0x6b);
            base.Controls.Add(this.buttonCancel);
            base.Controls.Add(this.buttonOK);
            base.Controls.Add(this.comboBoxMode);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "Option";
            base.ShowIcon = false;
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Memo Option";
            base.ResumeLayout(false);
        }

        public int ShowMode {
            get {
                return this.comboBoxMode.SelectedIndex;
            }
        }
    }
}
