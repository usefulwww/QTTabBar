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
    using System.Windows.Forms;

    internal sealed class QTabCancelEventArgs : TabControlCancelEventArgs {
        private QTabItemBase tabPage;

        public QTabCancelEventArgs(QTabItemBase tabPage, int tabPageIndex, bool cancel, TabControlAction action)
            : base(null, tabPageIndex, cancel, action) {
            this.tabPage = tabPage;
        }

        public QTabItemBase TabPage {
            get {
                return this.tabPage;
            }
        }
    }
}
