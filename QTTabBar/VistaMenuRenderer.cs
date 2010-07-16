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
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace QTTabBarLib {
    internal sealed class VistaMenuRenderer : ToolStripSystemRenderer {
        private static Bitmap bmpCheck = Resources_Image.imgVistaMenu_Check;
        private static Bitmap bmpLB = Resources_Image.imgVistaMenu_LB;
        private static Bitmap bmpLM = Resources_Image.imgVistaMenu_LM;
        private static Bitmap bmpLT = Resources_Image.imgVistaMenu_LT;
        private static Bitmap bmpMB = Resources_Image.imgVistaMenu_MB;
        private static Bitmap bmpMM = Resources_Image.imgVistaMenu_MM;
        private static Bitmap bmpMT = Resources_Image.imgVistaMenu_MT;
        private static Bitmap bmpRB = Resources_Image.imgVistaMenu_RB;
        private static Bitmap bmpRM = Resources_Image.imgVistaMenu_RM;
        private static Bitmap bmpRT = Resources_Image.imgVistaMenu_RT;
        private static Color clrBG = Color.FromArgb(240, 240, 240);
        private static Color clrGray = Color.FromArgb(0x80, 0x80, 0x80);
        private static Color clrLight = Color.FromArgb(0xe3, 0xe3, 0xe3);
        private static Color clrLightLight = Color.White;
        private static Color clrTxt = Color.Black;
        private bool fNoPrefix;

        public VistaMenuRenderer(bool fNoPrefix) {
            this.fNoPrefix = fNoPrefix;
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e) {
            e.ArrowColor = e.Item.Enabled ? clrTxt : clrGray;
            base.OnRenderArrow(e);
        }

        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e) {
            if(e.ToolStrip.RightToLeft == RightToLeft.No) {
                using(Pen pen = new Pen(clrLight)) {
                    e.Graphics.DrawLine(pen, new Point(e.AffectedBounds.Width - 2, 0), new Point(e.AffectedBounds.Width - 2, e.AffectedBounds.Height));
                }
                using(Pen pen2 = new Pen(clrLightLight)) {
                    e.Graphics.DrawLine(pen2, new Point(e.AffectedBounds.Width - 1, 0), new Point(e.AffectedBounds.Width - 1, e.AffectedBounds.Height));
                    return;
                }
            }
            using(Pen pen3 = new Pen(clrLight)) {
                e.Graphics.DrawLine(pen3, new Point(e.ToolStrip.Width - e.ToolStrip.Padding.Right, 0), new Point(e.ToolStrip.Width - e.ToolStrip.Padding.Right, e.AffectedBounds.Height));
            }
            using(Pen pen4 = new Pen(clrLightLight)) {
                e.Graphics.DrawLine(pen4, new Point((e.ToolStrip.Width - e.ToolStrip.Padding.Right) + 1, 0), new Point((e.ToolStrip.Width - e.ToolStrip.Padding.Right) + 1, e.AffectedBounds.Height));
            }
        }

        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e) {
            ToolStripDropDownMenu toolStrip = e.ToolStrip as ToolStripDropDownMenu;
            if(toolStrip != null) {
                if(!toolStrip.ShowImageMargin) {
                    e.Graphics.DrawImage(bmpCheck, new Rectangle(3, 0, 0x16, Math.Min(0x16, e.Item.Size.Height)));
                }
                else {
                    Rectangle rect = new Rectangle(7, 2, 0x12, 0x12);
                    using(SolidBrush brush = new SolidBrush(Color.FromArgb(0x80, SystemColors.Highlight))) {
                        e.Graphics.FillRectangle(brush, rect);
                        e.Graphics.DrawRectangle(SystemPens.Highlight, rect);
                    }
                }
            }
        }

        protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e) {
            QMenuItem item = e.Item as QMenuItem;
            if(((item != null) && item.IsCut) && ((e.ImageRectangle != Rectangle.Empty) && (e.Image != null))) {
                ColorMatrix newColorMatrix = new ColorMatrix();
                using(ImageAttributes attributes = new ImageAttributes()) {
                    newColorMatrix.Matrix33 = 0.5f;
                    attributes.SetColorMatrix(newColorMatrix);
                    Size size = e.Image.Size;
                    e.Graphics.DrawImage(e.Image, e.ImageRectangle, 0, 0, size.Width, size.Height, GraphicsUnit.Pixel, attributes);
                    return;
                }
            }
            base.OnRenderItemImage(e);
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e) {
            if(e.Item.Enabled) {
                e.TextColor = clrTxt;
            }
            else {
                e.TextColor = clrGray;
            }
            if(e.Item.RightToLeft == RightToLeft.No) {
                Rectangle textRectangle = e.TextRectangle;
                textRectangle.X -= 6;
                e.TextRectangle = textRectangle;
            }
            if(this.fNoPrefix) {
                e.TextFormat |= TextFormatFlags.NoPrefix;
            }
            base.OnRenderItemText(e);
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e) {
            if(e.Item.BackColor != e.ToolStrip.BackColor) {
                e.Graphics.FillRectangle(Brushes.White, new Rectangle(Point.Empty, e.Item.Size));
            }
            if(e.Item.Selected) {
                int num = e.Item.Size.Width - 2;
                int height = e.Item.Size.Height;
                e.Graphics.DrawImage(bmpLT, new Rectangle(3, 0, 4, 4));
                e.Graphics.DrawImage(bmpLM, new Rectangle(3, 4, 4, height - 8));
                e.Graphics.DrawImage(bmpLB, new Rectangle(3, height - 4, 4, 4));
                e.Graphics.DrawImage(bmpMT, new Rectangle(7, 0, num - 11, 4));
                e.Graphics.DrawImage(bmpMM, new Rectangle(7, 4, num - 11, height - 8));
                e.Graphics.DrawImage(bmpMB, new Rectangle(7, height - 4, num - 11, 4));
                e.Graphics.DrawImage(bmpRT, new Rectangle(num - 4, 0, 4, 4));
                e.Graphics.DrawImage(bmpRM, new Rectangle(num - 4, 4, 4, height - 8));
                e.Graphics.DrawImage(bmpRB, new Rectangle(num - 4, height - 4, 4, 4));
            }
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e) {
            Rectangle rectangle = new Rectangle(Point.Empty, e.Item.Size);
            if(e.ToolStrip.RightToLeft == RightToLeft.No) {
                rectangle.X += e.ToolStrip.Padding.Left - 10;
                rectangle.Width = e.ToolStrip.Width;
            }
            else {
                rectangle.X += 2;
                rectangle.Width = (e.ToolStrip.Width - rectangle.X) - e.ToolStrip.Padding.Right;
            }
            int y = rectangle.Height / 2;
            using(Pen pen = new Pen(clrLight)) {
                e.Graphics.DrawLine(pen, new Point(rectangle.X, y), new Point(rectangle.Width, y));
            }
            using(Pen pen2 = new Pen(clrLightLight)) {
                e.Graphics.DrawLine(pen2, new Point(rectangle.X, y + 1), new Point(rectangle.Width, y + 1));
            }
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e) {
            using(SolidBrush brush = new SolidBrush(clrBG)) {
                e.Graphics.FillRectangle(brush, e.AffectedBounds);
            }
        }
    }
}
