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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    class TreeViewWrapper : IDisposable {
        public delegate void TreeViewMiddleClickedHandler(IShellItem item);
        public event TreeViewMiddleClickedHandler TreeViewMiddleClicked;

        private bool fDisposed;
        private INameSpaceTreeControl treeControl;
        private NativeWindowController treeController;
        public TreeViewWrapper(IntPtr hwnd, INameSpaceTreeControl treeControl) {
            this.treeControl = treeControl;
            treeController = new NativeWindowController(hwnd);
            treeController.MessageCaptured += TreeControl_MessageCaptured;
        }

        private void HandleClick(Point pt) {
            IShellItem item = null;
            IntPtr ptr = IntPtr.Zero;
            try {
                TVHITTESTINFO structure = new TVHITTESTINFO { pt = pt };
                ptr = Marshal.AllocHGlobal(Marshal.SizeOf(structure));
                Marshal.StructureToPtr(structure, ptr, false);
                IntPtr wParam = PInvoke.SendMessage(treeController.Handle, 0x1111, IntPtr.Zero, ptr);
                if(wParam != IntPtr.Zero) {
                    structure = (TVHITTESTINFO)Marshal.PtrToStructure(ptr, typeof(TVHITTESTINFO));
                    if((structure.flags & 0x10) == 0 && (structure.flags & 0x80) == 0) {
                        treeControl.HitTest(pt, out item);
                        if(item != null) {
                            TreeViewMiddleClicked(item);
                        }
                    }
                }
            }
            finally {
                if(item != null) {
                    Marshal.ReleaseComObject(item);
                }
                if(ptr != IntPtr.Zero) {
                    Marshal.FreeHGlobal(ptr);
                }
            }
        }

        private bool TreeControl_MessageCaptured(ref Message msg) {
            switch(msg.Msg) {
                case WM.LBUTTONUP:
                    // TODO: Doesn't seem to ever fire...
                    break;

                case WM.MBUTTONUP:
                    // TODO
                    if(/*!QTUtility.CheckConfig(Settings.NoMidClickTree) &&*/ treeControl != null && TreeViewMiddleClicked != null) {
                        HandleClick(QTUtility2.PointFromLPARAM(msg.LParam));
                    }
                    break;

                case WM.DESTROY:
                    if(treeControl != null) {
                        Marshal.ReleaseComObject(treeControl);
                        treeControl = null;
                    }
                    break;
            }
            return false;
        }

        #region IDisposable Members

        public void Dispose() {
            if(fDisposed) return;
            if(treeControl != null) {
                Marshal.ReleaseComObject(treeControl);
                treeControl = null;
            }
            fDisposed = true;
        }

        #endregion
    }
}
