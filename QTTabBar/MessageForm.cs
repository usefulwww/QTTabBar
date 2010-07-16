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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Media;
using System.Windows.Forms;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    internal class MessageForm : Form {
        private Button btnCancel;
        private Button btnOk;
        private Button btnOk2;
        private IContainer components;
        private int duration;
        private int elapsed;
        private Label labelCounter;
        private Label labelMessage;
        private Panel panel1;
        private PictureBox pictureBoxIcon;
        private SystemSound sound;
        private string strExecute;
        private Timer timerClose;

        private MessageForm(string strMessage, string strTitle, string strExecute, MessageBoxIcon icon, int msecDuration) {
            Icon question;
            this.InitializeComponent();
            this.Text = strTitle;
            this.labelMessage.Text = strMessage;
            this.strExecute = strExecute;
            this.duration = msecDuration;
            this.labelCounter.Text = (this.duration / 0x3e8).ToString();
            using(Graphics graphics = this.labelMessage.CreateGraphics()) {
                SizeF ef = graphics.MeasureString(strMessage, this.labelMessage.Font, this.labelMessage.Width);
                int num = ((int)ef.Height) - this.labelMessage.Height;
                this.labelMessage.Height = (int)ef.Height;
                this.panel1.Height += num;
                base.ClientSize = new Size(base.ClientSize.Width, base.ClientSize.Height + num);
                this.pictureBoxIcon.Size = new Size(0x20, 0x20);
            }
            if(!string.IsNullOrEmpty(this.strExecute)) {
                this.btnOk.Visible = false;
                this.btnOk2.Visible = this.btnCancel.Visible = true;
            }
            switch(icon) {
                case MessageBoxIcon.Question:
                    question = SystemIcons.Question;
                    this.sound = SystemSounds.Question;
                    break;

                case MessageBoxIcon.Exclamation:
                    question = SystemIcons.Exclamation;
                    this.sound = SystemSounds.Exclamation;
                    break;

                case MessageBoxIcon.None:
                    question = SystemIcons.Information;
                    this.sound = null;
                    break;

                case MessageBoxIcon.Hand:
                    question = SystemIcons.Error;
                    this.sound = SystemSounds.Hand;
                    break;

                default:
                    question = SystemIcons.Asterisk;
                    this.sound = SystemSounds.Asterisk;
                    break;
            }
            this.pictureBoxIcon.Image = question.ToBitmap();
        }

        private void btnOk_Click(object sender, EventArgs e) {
            this.CloseMessageForm();
        }

        private void btnOk2_Click(object sender, EventArgs e) {
            if(!string.IsNullOrEmpty(this.strExecute)) {
                try {
                    Process.Start(this.strExecute);
                }
                catch {
                }
            }
        }

        private void CloseMessageForm() {
            this.timerClose.Enabled = false;
            if(!base.IsDisposed) {
                base.Dispose();
            }
        }

        protected override void Dispose(bool disposing) {
            if(disposing && (this.components != null)) {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent() {
            this.components = new Container();
            this.btnOk = new Button();
            this.btnOk2 = new Button();
            this.btnCancel = new Button();
            this.panel1 = new Panel();
            this.labelMessage = new Label();
            this.pictureBoxIcon = new PictureBox();
            this.timerClose = new Timer(this.components);
            this.labelCounter = new Label();
            this.panel1.SuspendLayout();
            ((ISupportInitialize)this.pictureBoxIcon).BeginInit();
            base.SuspendLayout();
            this.btnOk.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this.btnOk.Font = new Font(this.Font.FontFamily, 9f);
            this.btnOk.Location = new Point(0x13e, 0x7a);
            this.btnOk.Size = new Size(0x4b, 0x1c);
            this.btnOk.TabIndex = 0;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += this.btnOk_Click;
            this.btnOk2.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this.btnOk2.Font = new Font(this.Font.FontFamily, 9f);
            this.btnOk2.Location = new Point(0xed, 0x7a);
            this.btnOk2.Size = new Size(0x4b, 0x1c);
            this.btnOk2.TabIndex = 0;
            this.btnOk2.Text = "OK";
            this.btnOk2.UseVisualStyleBackColor = true;
            this.btnOk2.Visible = false;
            this.btnOk2.Click += this.btnOk2_Click;
            this.btnCancel.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this.btnCancel.Font = new Font(this.Font.FontFamily, 9f);
            this.btnCancel.Location = new Point(0x13e, 0x7a);
            this.btnCancel.Size = new Size(0x4b, 0x1c);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Visible = false;
            this.btnCancel.Click += this.btnOk_Click;
            this.panel1.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.panel1.BackColor = SystemColors.Window;
            this.panel1.Controls.Add(this.labelMessage);
            this.panel1.Controls.Add(this.pictureBoxIcon);
            this.panel1.Location = new Point(0, 0);
            this.panel1.Size = new Size(0x196, 0x70);
            this.labelMessage.Font = new Font(this.Font.FontFamily, 9f);
            this.labelMessage.Location = new Point(0x4d, 0x17);
            this.labelMessage.Size = new Size(0x13c, 0x34);
            this.pictureBoxIcon.Location = new Point(0x1c, 20);
            this.pictureBoxIcon.Size = new Size(0x20, 0x20);
            this.timerClose.Interval = 0x3e8;
            this.timerClose.Tick += this.timerClose_Tick;
            this.labelCounter.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this.labelCounter.AutoSize = true;
            this.labelCounter.Location = new Point(0x19, 130);
            this.labelCounter.Size = new Size(0x23, 13);
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0x195, 0xa2);
            base.Controls.Add(this.labelCounter);
            base.Controls.Add(this.panel1);
            base.Controls.Add(this.btnOk);
            base.Controls.Add(this.btnOk2);
            base.Controls.Add(this.btnCancel);
            base.FormBorderStyle = FormBorderStyle.FixedSingle;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.KeyPreview = true;
            base.ShowIcon = false;
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.Manual;
            base.Shown += this.MessageForm_Shown;
            this.panel1.ResumeLayout(false);
            ((ISupportInitialize)this.pictureBoxIcon).EndInit();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void MessageForm_Shown(object sender, EventArgs e) {
            if(this.sound != null) {
                this.sound.Play();
            }
            if(this.duration == 0) {
                base.ControlBox = this.labelCounter.Visible = this.btnOk.Visible = false;
            }
            this.timerClose.Start();
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);
            if(e.KeyData == (Keys.Control | Keys.C)) {
                QTTabBarClass.SetStringClipboard(this.Text + Environment.NewLine + Environment.NewLine + this.labelMessage.Text);
            }
        }

        public static void Show(string strMessage, string strTitle, string strExecute, MessageBoxIcon icon, int msecDuration) {
            MessageForm form = new MessageForm(strMessage, strTitle, strExecute, icon, msecDuration);
            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            form.Location = new Point((workingArea.X + workingArea.Width) - form.Width, (workingArea.Y + workingArea.Height) - form.Height);
            form.ShowMessageForm();
        }

        public static void Show(IntPtr hwndParent, string strMessage, string strTitle, MessageBoxIcon icon, int msecDuration, bool fModal = false) {
            MessageForm form = new MessageForm(strMessage, strTitle, null, icon, msecDuration);
            if(hwndParent != IntPtr.Zero) {
                RECT rect;
                PInvoke.GetWindowRect(hwndParent, out rect);
                form.Location = new Point(rect.left + ((rect.Width - form.Width) / 2), rect.top + ((rect.Height - form.Height) / 2));
            }
            else {
                Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
                form.Location = new Point((workingArea.X + workingArea.Width) - form.Width, (workingArea.Y + workingArea.Height) - form.Height);
            }
            if(fModal) {
                form.TopMost = true;
                form.ShowDialog();
            }
            else {
                form.ShowMessageForm();
            }
        }

        private void ShowMessageForm() {
            PInvoke.ShowWindow(base.Handle, 4);
            this.MessageForm_Shown(this, EventArgs.Empty);
        }

        private void timerClose_Tick(object sender, EventArgs e) {
            this.elapsed += 0x3e8;
            this.labelCounter.Text = ((this.duration - this.elapsed) / 0x3e8).ToString();
            this.labelCounter.Refresh();
            if(this.elapsed >= this.duration) {
                this.timerClose.Stop();
                if(!base.IsDisposed) {
                    base.Dispose();
                }
            }
        }
    }
}
