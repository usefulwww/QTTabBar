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
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;

    internal sealed class PluginViewItem : Control {
        private string author;
        private Button btnDisable;
        private Button btnOption;
        private Button btnRemove;
        public const float DEFAULT_HEIGHT = 55f;
        private string description;
        private bool fEnabled;
        private bool fHasOption;
        private bool fHasOptionQueried;
        private bool fSelected;
        private const int IMAGESIZE = 0x18;
        public const int MAX_HEIGHT = 0x86;
        private const int PADDING = 12;
        public PluginInformation PluginInfo;
        public PluginAssembly PluingAssembly;
        private string version;
        private const int XPOS_TEXTS = 0x30;
        private const int YPOS_DESCRIPTION = 30;

        public event EventHandler DisableButtonClick;

        public event EventHandler OptionButtonClick;

        public event EventHandler RemoveButtonClick;

        public PluginViewItem(PluginInformation pi, PluginAssembly pa) {
            this.PluginInfo = pi;
            this.PluingAssembly = pa;
            base.Name = pi.Name;
            this.version = pi.Version;
            this.author = pi.Author;
            this.description = pi.Description;
            this.fEnabled = pi.Enabled;
            base.Size = new Size(0x100, 0x37);
            base.Margin = Padding.Empty;
            base.Dock = DockStyle.Fill;
            base.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
        }

        public void Applied() {
            this.fHasOptionQueried = false;
        }

        private void btns_Click(object sender, EventArgs e) {
            if(sender == this.btnOption) {
                if(this.OptionButtonClick != null) {
                    this.OptionButtonClick(this, EventArgs.Empty);
                }
            }
            else if(sender == this.btnDisable) {
                if(this.DisableButtonClick != null) {
                    this.DisableButtonClick(this, EventArgs.Empty);
                }
            }
            else if((sender == this.btnRemove) && (this.RemoveButtonClick != null)) {
                this.RemoveButtonClick(this, EventArgs.Empty);
            }
        }

        private static Color ConvertColor(Color clr) {
            return Color.FromArgb((int)(clr.A * 0.6), clr);
        }

        private void CreateButtons() {
            base.SuspendLayout();
            this.btnOption = new Button();
            this.btnOption.Text = PluginView.BTN_OPTION;
            this.btnOption.Size = new Size(0x4b, 0x17);
            this.btnOption.Location = new Point(0x30, base.Height - 0x1d);
            this.btnOption.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this.btnOption.UseVisualStyleBackColor = true;
            this.btnOption.Enabled = this.fHasOption;
            this.btnOption.Click += new EventHandler(this.btns_Click);
            this.btnDisable = new Button();
            this.btnDisable.Text = this.fEnabled ? PluginView.BTN_DISABLE : PluginView.BTN_ENABLE;
            this.btnDisable.Size = new Size(0x4b, 0x17);
            this.btnDisable.Location = new Point(base.Width - 0xa8, base.Height - 0x1d);
            this.btnDisable.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this.btnDisable.UseVisualStyleBackColor = true;
            this.btnDisable.Click += new EventHandler(this.btns_Click);
            this.btnRemove = new Button();
            this.btnRemove.Text = PluginView.BTN_REMOVE;
            this.btnRemove.Size = new Size(0x4b, 0x17);
            this.btnRemove.Location = new Point(base.Width - 0x57, base.Height - 0x1d);
            this.btnRemove.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new EventHandler(this.btns_Click);
            base.Controls.Add(this.btnOption);
            base.Controls.Add(this.btnDisable);
            base.Controls.Add(this.btnRemove);
            base.ResumeLayout();
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            if((e.KeyData == Keys.Delete) && (this.RemoveButtonClick != null)) {
                this.RemoveButtonClick(this, EventArgs.Empty);
            }
            else {
                base.OnKeyDown(e);
            }
        }

        protected override void OnPaint(PaintEventArgs e) {
            if(this.fSelected) {
                Color clr = this.fEnabled ? SystemColors.Highlight : ConvertColor(SystemColors.Highlight);
                Color color2 = ConvertColor(clr);
                using(LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, base.Width, base.Height), clr, color2, LinearGradientMode.Vertical)) {
                    e.Graphics.FillRectangle(brush, e.ClipRectangle);
                    goto Label_0079;
                }
            }
            e.Graphics.FillRectangle(SystemBrushes.Window, e.ClipRectangle);
        Label_0079:
            if(this.PluginInfo.ImageLarge != null) {
                Rectangle destRect = new Rectangle(12, 12, 0x18, 0x18);
                if(this.fEnabled) {
                    e.Graphics.DrawImage(this.PluginInfo.ImageLarge, destRect, new Rectangle(0, 0, 0x18, 0x18), GraphicsUnit.Pixel);
                }
                else {
                    ColorMatrix newColorMatrix = new ColorMatrix();
                    using(ImageAttributes attributes = new ImageAttributes()) {
                        newColorMatrix.Matrix33 = 0.5f;
                        attributes.SetColorMatrix(newColorMatrix);
                        e.Graphics.DrawImage(this.PluginInfo.ImageLarge, destRect, 0, 0, 0x18, 0x18, GraphicsUnit.Pixel, attributes);
                    }
                }
            }
            Brush brush2 = this.fSelected ? SystemBrushes.HighlightText : (this.fEnabled ? SystemBrushes.WindowText : SystemBrushes.GrayText);
            using(StringFormat format = new StringFormat()) {
                SizeF ef;
                format.Trimming = StringTrimming.EllipsisCharacter;
                format.FormatFlags |= StringFormatFlags.LineLimit | StringFormatFlags.NoWrap;
                using(Font font = new Font(this.Font, FontStyle.Bold)) {
                    ef = e.Graphics.MeasureString(base.Name, font, (int)(base.Width * 0.66));
                    e.Graphics.DrawString(base.Name, font, brush2, new RectangleF(new PointF(48f, 8f), ef), format);
                }
                e.Graphics.DrawString(this.version + "    by " + this.author, this.Font, brush2, 60f + ef.Width, 8f, format);
                format.FormatFlags &= ~StringFormatFlags.NoWrap;
                SizeF size = e.Graphics.MeasureString(this.description, this.Font, new SizeF((float)Math.Max(base.Width - 60, 0x40), this.fSelected ? ((float)0x45) : ((float)0x13)), format);
                e.Graphics.DrawString(this.description, this.Font, brush2, new RectangleF(new PointF(48f, 30f), size), format);
            }
            using(Pen pen = new Pen(ConvertColor(SystemColors.GrayText))) {
                pen.DashStyle = DashStyle.Dot;
                e.Graphics.DrawLine(pen, 0, base.Height - 1, base.Width - 1, base.Height - 1);
            }
        }

        public float SelectPlugin(bool fHasOption) {
            SizeF ef;
            base.Select();
            if(this.fSelected) {
                return (float)base.Height;
            }
            this.fSelected = true;
            this.fHasOption = fHasOption;
            this.fHasOptionQueried = true;
            using(Graphics graphics = base.CreateGraphics()) {
                ef = graphics.MeasureString(this.description, this.Font, Math.Max(base.Width - 60, 0x40));
            }
            float num = 65f + ef.Height;
            if(this.btnOption == null) {
                this.CreateButtons();
                return num;
            }
            this.SetButtonsVisble(true);
            return num;
        }

        private void SetButtonsVisble(bool fVisible) {
            this.btnOption.Visible = this.btnDisable.Visible = this.btnRemove.Visible = fVisible;
        }

        public void UnselectPlugin() {
            if(this.fSelected) {
                this.fSelected = false;
                if(this.btnOption != null) {
                    this.SetButtonsVisble(false);
                }
                base.Invalidate();
            }
        }

        public bool HasOption {
            get {
                return this.fHasOption;
            }
        }

        public bool HasOptionQueried {
            get {
                return this.fHasOptionQueried;
            }
        }

        public bool PluginEnabled {
            get {
                return this.fEnabled;
            }
            set {
                this.fEnabled = value;
                if(this.fEnabled) {
                    if(this.fHasOption && (this.btnOption != null)) {
                        this.btnOption.Enabled = true;
                    }
                    if(this.btnDisable != null) {
                        this.btnDisable.Text = PluginView.BTN_DISABLE;
                    }
                }
                else {
                    if(this.fHasOption && (this.btnOption != null)) {
                        this.btnOption.Enabled = false;
                    }
                    if(this.btnDisable != null) {
                        this.btnDisable.Text = PluginView.BTN_ENABLE;
                    }
                }
                base.Invalidate();
            }
        }
    }
}
