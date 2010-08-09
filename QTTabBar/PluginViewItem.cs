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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace QTTabBarLib {
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
            PluginInfo = pi;
            PluingAssembly = pa;
            Name = pi.Name;
            version = pi.Version;
            author = pi.Author;
            description = pi.Description;
            fEnabled = pi.Enabled;
            Size = new Size(0x100, 0x37);
            Margin = Padding.Empty;
            Dock = DockStyle.Fill;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
        }

        public void Applied() {
            fHasOptionQueried = false;
        }

        private void btns_Click(object sender, EventArgs e) {
            if(sender == btnOption) {
                if(OptionButtonClick != null) {
                    OptionButtonClick(this, EventArgs.Empty);
                }
            }
            else if(sender == btnDisable) {
                if(DisableButtonClick != null) {
                    DisableButtonClick(this, EventArgs.Empty);
                }
            }
            else if((sender == btnRemove) && (RemoveButtonClick != null)) {
                RemoveButtonClick(this, EventArgs.Empty);
            }
        }

        private static Color ConvertColor(Color clr) {
            return Color.FromArgb((int)(clr.A * 0.6), clr);
        }

        private void CreateButtons() {
            SuspendLayout();
            btnOption = new Button();
            btnOption.Text = PluginView.BTN_OPTION;
            btnOption.Size = new Size(0x4b, 0x17);
            btnOption.Location = new Point(0x30, Height - 0x1d);
            btnOption.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            btnOption.UseVisualStyleBackColor = true;
            btnOption.Enabled = fHasOption;
            btnOption.Click += btns_Click;
            btnDisable = new Button();
            btnDisable.Text = fEnabled ? PluginView.BTN_DISABLE : PluginView.BTN_ENABLE;
            btnDisable.Size = new Size(0x4b, 0x17);
            btnDisable.Location = new Point(Width - 0xa8, Height - 0x1d);
            btnDisable.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            btnDisable.UseVisualStyleBackColor = true;
            btnDisable.Click += btns_Click;
            btnRemove = new Button();
            btnRemove.Text = PluginView.BTN_REMOVE;
            btnRemove.Size = new Size(0x4b, 0x17);
            btnRemove.Location = new Point(Width - 0x57, Height - 0x1d);
            btnRemove.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            btnRemove.UseVisualStyleBackColor = true;
            btnRemove.Click += btns_Click;
            Controls.Add(btnOption);
            Controls.Add(btnDisable);
            Controls.Add(btnRemove);
            ResumeLayout();
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            if((e.KeyData == Keys.Delete) && (RemoveButtonClick != null)) {
                RemoveButtonClick(this, EventArgs.Empty);
            }
            else {
                base.OnKeyDown(e);
            }
        }

        protected override void OnPaint(PaintEventArgs e) {
            if(fSelected) {
                Color clr = fEnabled ? SystemColors.Highlight : ConvertColor(SystemColors.Highlight);
                Color color2 = ConvertColor(clr);
                using(LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, Width, Height), clr, color2, LinearGradientMode.Vertical)) {
                    e.Graphics.FillRectangle(brush, e.ClipRectangle);
                    goto Label_0079;
                }
            }
            e.Graphics.FillRectangle(SystemBrushes.Window, e.ClipRectangle);
        Label_0079:
            if(PluginInfo.ImageLarge != null) {
                Rectangle destRect = new Rectangle(12, 12, 0x18, 0x18);
                if(fEnabled) {
                    e.Graphics.DrawImage(PluginInfo.ImageLarge, destRect, new Rectangle(0, 0, 0x18, 0x18), GraphicsUnit.Pixel);
                }
                else {
                    ColorMatrix newColorMatrix = new ColorMatrix();
                    using(ImageAttributes attributes = new ImageAttributes()) {
                        newColorMatrix.Matrix33 = 0.5f;
                        attributes.SetColorMatrix(newColorMatrix);
                        e.Graphics.DrawImage(PluginInfo.ImageLarge, destRect, 0, 0, 0x18, 0x18, GraphicsUnit.Pixel, attributes);
                    }
                }
            }
            Brush brush2 = fSelected ? SystemBrushes.HighlightText : (fEnabled ? SystemBrushes.WindowText : SystemBrushes.GrayText);
            using(StringFormat format = new StringFormat()) {
                SizeF ef;
                format.Trimming = StringTrimming.EllipsisCharacter;
                format.FormatFlags |= StringFormatFlags.LineLimit | StringFormatFlags.NoWrap;
                using(Font font = new Font(Font, FontStyle.Bold)) {
                    ef = e.Graphics.MeasureString(Name, font, (int)(Width * 0.66));
                    e.Graphics.DrawString(Name, font, brush2, new RectangleF(new PointF(48f, 8f), ef), format);
                }
                e.Graphics.DrawString(version + "    by " + author, Font, brush2, 60f + ef.Width, 8f, format);
                format.FormatFlags &= ~StringFormatFlags.NoWrap;
                SizeF size = e.Graphics.MeasureString(description, Font, new SizeF(Math.Max(Width - 60, 0x40), fSelected ? (0x45) : (0x13)), format);
                e.Graphics.DrawString(description, Font, brush2, new RectangleF(new PointF(48f, 30f), size), format);
            }
            using(Pen pen = new Pen(ConvertColor(SystemColors.GrayText))) {
                pen.DashStyle = DashStyle.Dot;
                e.Graphics.DrawLine(pen, 0, Height - 1, Width - 1, Height - 1);
            }
        }

        public float SelectPlugin(bool fHasOption) {
            SizeF ef;
            Select();
            if(fSelected) {
                return Height;
            }
            fSelected = true;
            this.fHasOption = fHasOption;
            fHasOptionQueried = true;
            using(Graphics graphics = CreateGraphics()) {
                ef = graphics.MeasureString(description, Font, Math.Max(Width - 60, 0x40));
            }
            float num = 65f + ef.Height;
            if(btnOption == null) {
                CreateButtons();
                return num;
            }
            SetButtonsVisble(true);
            return num;
        }

        private void SetButtonsVisble(bool fVisible) {
            btnOption.Visible = btnDisable.Visible = btnRemove.Visible = fVisible;
        }

        public void UnselectPlugin() {
            if(fSelected) {
                fSelected = false;
                if(btnOption != null) {
                    SetButtonsVisble(false);
                }
                Invalidate();
            }
        }

        public bool HasOption {
            get {
                return fHasOption;
            }
        }

        public bool HasOptionQueried {
            get {
                return fHasOptionQueried;
            }
        }

        public bool PluginEnabled {
            get {
                return fEnabled;
            }
            set {
                fEnabled = value;
                if(fEnabled) {
                    if(fHasOption && (btnOption != null)) {
                        btnOption.Enabled = true;
                    }
                    if(btnDisable != null) {
                        btnDisable.Text = PluginView.BTN_DISABLE;
                    }
                }
                else {
                    if(fHasOption && (btnOption != null)) {
                        btnOption.Enabled = false;
                    }
                    if(btnDisable != null) {
                        btnDisable.Text = PluginView.BTN_ENABLE;
                    }
                }
                Invalidate();
            }
        }
    }
}
