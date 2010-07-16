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
using System.Drawing.Imaging;

namespace QTTabBarLib {
    internal sealed class ImageStrip : IDisposable {
        private List<Bitmap> lstImages = new List<Bitmap>();
        private Size size;
        private Color transparentColor;

        public ImageStrip(Size size) {
            this.size = size;
        }

        public void AddStrip(Bitmap bmp) {
            int width = bmp.Width;
            int num2 = 0;
            bool flag = this.transparentColor != Color.Empty;
            if(((width % this.size.Width) != 0) || (bmp.Height != this.size.Height)) {
                throw new ArgumentException("size invalid.");
            }
            Rectangle rect = new Rectangle(Point.Empty, this.size);
            while((width - this.size.Width) > -1) {
                Bitmap image = bmp.Clone(rect, PixelFormat.Format32bppArgb);
                if(flag) {
                    image.MakeTransparent(this.transparentColor);
                }
                if((this.lstImages.Count > num2) && (this.lstImages[num2] != null)) {
                    using(Graphics graphics = Graphics.FromImage(this.lstImages[num2])) {
                        graphics.Clear(Color.Transparent);
                        graphics.DrawImage(image, 0, 0);
                        image.Dispose();
                        goto Label_00E4;
                    }
                }
                this.lstImages.Add(image);
            Label_00E4:
                num2++;
                width -= this.size.Width;
                rect.X += this.size.Width;
            }
        }

        public void Dispose() {
            foreach(Bitmap bitmap in this.lstImages) {
                if(bitmap != null) {
                    bitmap.Dispose();
                }
            }
            this.lstImages.Clear();
        }

        public Bitmap this[int index] {
            get {
                return this.lstImages[index];
            }
        }

        public Color TransparentColor {
            set {
                this.transparentColor = value;
            }
        }
    }
}
