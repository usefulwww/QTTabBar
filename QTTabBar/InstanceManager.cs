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
using System.Runtime.InteropServices;
using System.Threading;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    internal static class InstanceManager {
        private static Dictionary<IntPtr, IntPtr> dicBtnBarHandle = new Dictionary<IntPtr, IntPtr>();
        private static ReaderWriterLock rwLockBtnBar = new ReaderWriterLock();
        private static ReaderWriterLock rwLockTabBar = new ReaderWriterLock();
        private static StackDictionary<IntPtr, InstancePair> sdInstancePair = new StackDictionary<IntPtr, InstancePair>();

        public static void AddButtonBarHandle(IntPtr hwndExplr, IntPtr hwndBtnBar) {
            try {
                rwLockBtnBar.AcquireWriterLock(-1);
                dicBtnBarHandle[hwndExplr] = hwndBtnBar;
            }
            finally {
                rwLockBtnBar.ReleaseWriterLock();
            }
        }

        public static IEnumerable<IntPtr> ButtonBarHandles() {
            try {
                rwLockBtnBar.AcquireReaderLock(-1);
                foreach(IntPtr hwndBB in dicBtnBarHandle.Values) {
                    yield return hwndBB;
                }
            }
            finally {
                rwLockBtnBar.ReleaseReaderLock();
            }
        }

        public static IEnumerable<IntPtr> ExplorerHandles() {
            try {
                rwLockTabBar.AcquireReaderLock(-1);
                foreach(IntPtr hwnd in sdInstancePair.Keys) {
                    yield return hwnd;
                }
            }
            finally {
                rwLockTabBar.ReleaseReaderLock();
            }
        }

        public static QTTabBarClass GetTabBar(IntPtr hwndExplr) {
            QTTabBarClass class2;
            try {
                InstancePair pair;
                rwLockTabBar.AcquireReaderLock(-1);
                if((sdInstancePair.TryGetValue(hwndExplr, out pair) && (pair.tabBar != null)) && pair.tabBar.IsHandleCreated) {
                    return pair.tabBar;
                }
                class2 = null;
            }
            finally {
                rwLockTabBar.ReleaseReaderLock();
            }
            return class2;
        }

        public static IntPtr GetTabBarHandle(IntPtr hwndExplr) {
            IntPtr zero;
            try {
                InstancePair pair;
                rwLockTabBar.AcquireReaderLock(-1);
                if((sdInstancePair.TryGetValue(hwndExplr, out pair) && (pair.tabBar != null)) && pair.tabBar.IsHandleCreated) {
                    return pair.hwnd;
                }
                zero = IntPtr.Zero;
            }
            finally {
                rwLockTabBar.ReleaseReaderLock();
            }
            return zero;
        }

        public static bool NextInstanceExists() {
            try {
                rwLockTabBar.AcquireWriterLock(-1);
                while(sdInstancePair.Count > 0) {
                    IntPtr ptr;
                    InstancePair pair = sdInstancePair.Peek(out ptr);
                    if(((pair.tabBar != null) && pair.tabBar.IsHandleCreated) && (PInvoke.IsWindow(pair.hwnd) && PInvoke.IsWindow(ptr))) {
                        return true;
                    }
                    sdInstancePair.Pop();
                }
            }
            finally {
                rwLockTabBar.ReleaseWriterLock();
            }
            return false;
        }

        public static void PushInstance(IntPtr hwndExplr, QTTabBarClass tabBar) {
            try {
                rwLockTabBar.AcquireWriterLock(-1);
                sdInstancePair.Push(hwndExplr, new InstancePair(tabBar, tabBar.Handle));
            }
            finally {
                rwLockTabBar.ReleaseWriterLock();
            }
        }

        public static void RemoveButtonBarHandle(IntPtr hwndExplr) {
            try {
                rwLockBtnBar.AcquireWriterLock(-1);
                dicBtnBarHandle.Remove(hwndExplr);
            }
            finally {
                rwLockBtnBar.ReleaseWriterLock();
            }
        }

        public static bool RemoveInstance(IntPtr hwndExplr, QTTabBarClass tabBar) {
            bool flag2;
            try {
                rwLockTabBar.AcquireWriterLock(-1);
                bool flag = tabBar == CurrentTabBar;
                sdInstancePair.Remove(hwndExplr);
                flag2 = flag;
            }
            finally {
                rwLockTabBar.ReleaseWriterLock();
            }
            return flag2;
        }

        public static IEnumerable<IntPtr> TabBarHandles() {
            try {
                rwLockTabBar.AcquireReaderLock(-1);
                foreach(InstancePair ip in sdInstancePair.Values) {
                    yield return ip.hwnd;
                }
            }
            finally {
                rwLockTabBar.ReleaseReaderLock();
            }
        }

        public static bool TryGetButtonBarHandle(IntPtr hwndExplr, out IntPtr hwndButtonBar) {
            try {
                IntPtr ptr;
                rwLockBtnBar.AcquireReaderLock(-1);
                if(dicBtnBarHandle.TryGetValue(hwndExplr, out ptr) && PInvoke.IsWindow(ptr)) {
                    hwndButtonBar = ptr;
                    return true;
                }
                hwndButtonBar = IntPtr.Zero;
            }
            finally {
                rwLockBtnBar.ReleaseReaderLock();
            }
            return false;
        }

        public static IntPtr CurrentHandle {
            get {
                IntPtr zero;
                try {
                    rwLockTabBar.AcquireReaderLock(-1);
                    if(sdInstancePair.Count > 0) {
                        InstancePair pair = sdInstancePair.Peek();
                        if((pair.tabBar != null) && pair.tabBar.IsHandleCreated) {
                            return pair.hwnd;
                        }
                    }
                    zero = IntPtr.Zero;
                }
                finally {
                    rwLockTabBar.ReleaseReaderLock();
                }
                return zero;
            }
        }

        public static QTTabBarClass CurrentTabBar {
            get {
                QTTabBarClass class2;
                try {
                    rwLockTabBar.AcquireReaderLock(-1);
                    if(sdInstancePair.Count > 0) {
                        return sdInstancePair.Peek().tabBar;
                    }
                    class2 = null;
                }
                finally {
                    rwLockTabBar.ReleaseReaderLock();
                }
                return class2;
            }
        }

        public delegate void TabBarCommand(QTTabBarClass tabBar);

        [StructLayout(LayoutKind.Sequential)]
        private struct InstancePair {
            public QTTabBarClass tabBar;
            public IntPtr hwnd;
            public InstancePair(QTTabBarClass tabBar, IntPtr hwnd) {
                this.tabBar = tabBar;
                this.hwnd = hwnd;
            }
        }
    }
}
