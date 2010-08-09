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
            bool flag = transparentColor != Color.Empty;
            if(((width % size.Width) != 0) || (bmp.Height != size.Height)) {
                throw new ArgumentException("size invalid.");
            }
            Rectangle rect = new Rectangle(Point.Empty, size);
            while((width - size.Width) > -1) {
                Bitmap image = bmp.Clone(rect, PixelFormat.Format32bppArgb);
                if(flag) {
                    image.MakeTransparent(transparentColor);
                }
                if((lstImages.Count > num2) && (lstImages[num2] != null)) {
                    using(Graphics graphics = Graphics.FromImage(lstImages[num2])) {
                        graphics.Clear(Color.Transparent);
                        graphics.DrawImage(image, 0, 0);
                        image.Dispose();
                        goto Label_00E4;
                    }
                }
                lstImages.Add(image);
            Label_00E4:
                num2++;
                width -= size.Width;
                rect.X += size.Width;
            }
        }

        public void Dispose() {
            foreach(Bitmap bitmap in lstImages) {
                if(bitmap != null) {
                    bitmap.Dispose();
                }
            }
            lstImages.Clear();
        }

        public Bitmap this[int index] {
            get {
                return lstImages[index];
            }
        }

        public Color TransparentColor {
            set {
                transparentColor = value;
            }
        }
    }
}
