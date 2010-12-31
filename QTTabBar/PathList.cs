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
using System.Collections.ObjectModel;

namespace QTTabBarLib {
    internal sealed class PathList : Collection<string> {
        private int maxCapacity;

        public PathList(int capacity) {
            if(capacity < 1) {
                capacity = 1;
            }
            maxCapacity = capacity;
        }

        public PathList(IList<string> collection, int capacity)
            : base(collection) {
            if(capacity < 1) {
                capacity = 1;
            }
            maxCapacity = capacity;
            ensureCount();
        }

        private void ensureCount() {
            while(Count > maxCapacity) {
                RemoveItem(0);
            }
        }

        private int ensureUnique(string path) {
            for(int i = 0; i < Count; i++) {
                if(path.PathEquals(base[i])) {
                    RemoveItem(i);
                    return i;
                }
            }
            return -1;
        }

        protected override void InsertItem(int index, string item) {
            int num = ensureUnique(item);
            if((num != -1) && (num < index)) {
                index--;
            }
            base.InsertItem(index, item);
            ensureCount();
        }

        public string[] ToArray() {
            string[] strArray = new string[Count];
            for(int i = 0; i < Count; i++) {
                strArray[i] = base[i];
            }
            return strArray;
        }

        public int MaxCapacity {
            set {
                if(value < 1) {
                    value = 1;
                }
                maxCapacity = value;
                ensureCount();
            }
        }
    }
}
