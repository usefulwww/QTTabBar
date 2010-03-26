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
    using System.Drawing;
    using System.Windows.Forms;

    internal sealed class ItemRightClickedEventArgs {
        private ToolStripItem clickedItem;
        private int hresult;
        private bool isKey;
        private System.Drawing.Point pnt;

        public ItemRightClickedEventArgs(ToolStripItem clickedItem, bool fKey, System.Drawing.Point pnt) {
            this.clickedItem = clickedItem;
            this.isKey = fKey;
            this.pnt = pnt;
        }

        public ToolStripItem ClickedItem {
            get {
                return this.clickedItem;
            }
        }

        public int HRESULT {
            get {
                return this.hresult;
            }
            set {
                this.hresult = value;
            }
        }

        public bool IsKey {
            get {
                return this.isKey;
            }
        }

        public System.Drawing.Point Point {
            get {
                return this.pnt;
            }
        }
    }
}
