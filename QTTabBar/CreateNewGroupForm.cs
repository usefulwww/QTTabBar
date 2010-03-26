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
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    internal sealed class CreateNewGroupForm : Form {
        private Button buttonCancel;
        private Button buttonOK;
        private CheckBox checkBox1;
        private IContainer components;
        private Label label1;
        private string newPath;
        private QTabControl.QTabCollection Tabs;
        private TextBox textBox1;

        public CreateNewGroupForm(string currentPath, QTabControl.QTabCollection tabs) {
            this.newPath = currentPath;
            this.Tabs = tabs;
            this.InitializeComponent();
            this.textBox1.Text = QTUtility2.MakePathDisplayText(this.newPath, false);
            string[] strArray = QTUtility.TextResourcesDic["TabBar_NewGroup"];
            this.Text = strArray[0];
            this.label1.Text = strArray[1];
            this.checkBox1.Text = strArray[2];
            base.ActiveControl = this.textBox1;
        }

        private void buttonOK_Click(object sender, EventArgs e) {
            string key = this.textBox1.Text.Replace(";", "_");
            int num = 0;
            string str2 = key;
            while(QTUtility.GroupPathsDic.ContainsKey(str2)) {
                str2 = key + "_" + ++num;
            }
            key = str2;
            if(!this.checkBox1.Checked) {
                QTUtility.GroupPathsDic.Add(key, this.newPath);
            }
            else {
                str2 = string.Empty;
                foreach(QTabItem item in this.Tabs) {
                    str2 = str2 + item.CurrentPath + ";";
                }
                QTUtility.GroupPathsDic.Add(key, str2.Trim(QTUtility.SEPARATOR_CHAR));
            }
        }

        protected override void Dispose(bool disposing) {
            if(disposing && (this.components != null)) {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent() {
            this.buttonOK = new Button();
            this.buttonCancel = new Button();
            this.label1 = new Label();
            this.textBox1 = new TextBox();
            this.checkBox1 = new CheckBox();
            base.SuspendLayout();
            this.buttonOK.DialogResult = DialogResult.OK;
            this.buttonOK.Enabled = false;
            this.buttonOK.Location = new Point(0x8b, 0x42);
            this.buttonOK.Size = new Size(0x4b, 0x17);
            this.buttonOK.TabIndex = 0;
            this.buttonOK.Text = "OK";
            this.buttonOK.Click += new EventHandler(this.buttonOK_Click);
            this.buttonCancel.DialogResult = DialogResult.Cancel;
            this.buttonCancel.Location = new Point(220, 0x42);
            this.buttonCancel.Size = new Size(0x4b, 0x17);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Cancel";
            this.label1.AutoSize = true;
            this.label1.Location = new Point(12, 0x12);
            this.label1.Size = new Size(0x41, 12);
            this.textBox1.Location = new Point(0x7d, 15);
            this.textBox1.Size = new Size(170, 20);
            this.textBox1.TabIndex = 2;
            this.textBox1.TextChanged += new EventHandler(this.textBox1_TextChanged);
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new Point(12, 70);
            this.checkBox1.Size = new Size(90, 0x1c);
            this.checkBox1.TabIndex = 3;
            base.AcceptButton = this.buttonOK;
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.CancelButton = this.buttonCancel;
            base.ClientSize = new Size(0x133, 0x73);
            base.Controls.Add(this.checkBox1);
            base.Controls.Add(this.textBox1);
            base.Controls.Add(this.label1);
            base.Controls.Add(this.buttonCancel);
            base.Controls.Add(this.buttonOK);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.ShowIcon = false;
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.CenterParent;
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void textBox1_TextChanged(object sender, EventArgs e) {
            this.buttonOK.Enabled = this.textBox1.Text.Length != 0;
        }
    }
}
