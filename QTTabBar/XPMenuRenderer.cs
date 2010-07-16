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

using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace QTTabBarLib {
    internal sealed class XPMenuRenderer : ToolStripSystemRenderer {
        private bool fNoPrefix;

        public XPMenuRenderer(bool fNoPrefix) {
            this.fNoPrefix = fNoPrefix;
        }

        protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e) {
            ToolStripMenuItem item = e.Item as ToolStripMenuItem;
            if((item != null) && item.Checked) {
                Rectangle imageRectangle = e.ImageRectangle;
                imageRectangle.Inflate(1, 1);
                using(SolidBrush brush = new SolidBrush(Color.FromArgb(0x80, SystemColors.Highlight))) {
                    e.Graphics.FillRectangle(brush, imageRectangle);
                    e.Graphics.DrawRectangle(SystemPens.Highlight, imageRectangle);
                }
            }
            QMenuItem item2 = e.Item as QMenuItem;
            if(((item2 != null) && item2.IsCut) && ((e.ImageRectangle != Rectangle.Empty) && (e.Image != null))) {
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
            if(this.fNoPrefix) {
                e.TextFormat |= TextFormatFlags.NoPrefix;
            }
            e.TextColor = e.Item.Selected ? SystemColors.HighlightText : SystemColors.MenuText;
            base.OnRenderItemText(e);
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e) {
            if((e.Item.BackColor != e.ToolStrip.BackColor) && !e.Item.Selected) {
                using(SolidBrush brush = new SolidBrush(e.Item.BackColor)) {
                    e.Graphics.FillRectangle(brush, new Rectangle(Point.Empty, e.Item.Size));
                    return;
                }
            }
            e.Graphics.FillRectangle(e.Item.Selected ? SystemBrushes.MenuHighlight : SystemBrushes.Menu, new Rectangle(Point.Empty, e.Item.Size));
        }
    }
}
