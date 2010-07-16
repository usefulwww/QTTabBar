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
using System.Runtime.InteropServices;
using System.Text;
using BandObjectLib;
using Microsoft.Win32;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    internal static class WindowUtils {
        public static void BringExplorerToFront(IntPtr hwndExplr) {
            PInvoke.ShowWindow(hwndExplr, PInvoke.IsIconic(hwndExplr) ? 9 : 5);
            PInvoke.SetForegroundWindow(hwndExplr);
        }

        public static void CloseExplorer(IntPtr hwndExplr, int nCode) {
            if(QTUtility.IsVista) {
                PInvoke.SendMessage(hwndExplr, 0x10, IntPtr.Zero, (IntPtr)nCode);
            }
            else {
                if(nCode == 0) {
                    nCode = 3;
                }
                PInvoke.PostMessage(hwndExplr, 0x10, IntPtr.Zero, (IntPtr)nCode);
            }
        }

        public static IntPtr GetShellTabWindowClass(IntPtr hwndExplr) {
            return PInvoke.FindWindowEx(hwndExplr, IntPtr.Zero, "ShellTabWindowClass", null);
        }

        public static IntPtr GetShellTrayWnd() {
            return PInvoke.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Shell_TrayWnd", null);
        }

        public static bool IsExplorerProcessSeparated() {
            using(RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", false)) {
                return ((key != null) && (((int)key.GetValue("SeparateProcess", 0)) != 0));
            }
        }

        public static bool IsToolbarLocked(IntPtr hwndReBar) {
            if((hwndReBar == IntPtr.Zero) || !PInvoke.IsWindow(hwndReBar)) {
                return false;
            }
            REBARBANDINFO structure = new REBARBANDINFO();
            structure.cbSize = Marshal.SizeOf(structure);
            structure.fMask = 1;
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(structure));
            Marshal.StructureToPtr(structure, ptr, false);
            IntPtr ptr2 = PInvoke.SendMessage(hwndReBar, 0x405, IntPtr.Zero, ptr);
            structure = (REBARBANDINFO)Marshal.PtrToStructure(ptr, typeof(REBARBANDINFO));
            Marshal.FreeHGlobal(ptr);
            return ((ptr2 != IntPtr.Zero) && ((structure.fStyle & 0x100) != 0));
        }

        public static void LockToolbar(bool fLock, IntPtr hwndExplr, IntPtr hwndReBar) {
            if(IsToolbarLocked(hwndReBar) ^ fLock) {
                if(QTUtility.IsVista) {
                    PInvoke.SendMessage(GetShellTabWindowClass(hwndExplr), 0x111, (IntPtr)0xa20c, IntPtr.Zero);
                }
                else {
                    PInvoke.SendMessage(hwndExplr, 0x111, (IntPtr)0xa20c, IntPtr.Zero);
                }
            }
        }

        public static void ShowMenuBar(bool fShow, IntPtr hwndRebar) {
            if((hwndRebar != IntPtr.Zero) && PInvoke.IsWindow(hwndRebar)) {
                int num = (int)PInvoke.SendMessage(hwndRebar, 0x40c, IntPtr.Zero, IntPtr.Zero);
                REBARBANDINFO structure = new REBARBANDINFO();
                structure.cbSize = Marshal.SizeOf(structure);
                structure.fMask = 0x110;
                for(int i = 0; i < num; i++) {
                    IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(structure));
                    Marshal.StructureToPtr(structure, ptr, false);
                    PInvoke.SendMessage(hwndRebar, 0x405, (IntPtr)i, ptr);
                    structure = (REBARBANDINFO)Marshal.PtrToStructure(ptr, typeof(REBARBANDINFO));
                    Marshal.FreeHGlobal(ptr);
                    StringBuilder lpClassName = new StringBuilder(260);
                    PInvoke.GetClassName(structure.hwndChild, lpClassName, lpClassName.Capacity);
                    if((lpClassName.ToString() == "ToolbarWindow32") && (structure.wID == 1)) {
                        PInvoke.SendMessage(hwndRebar, 0x423, (IntPtr)i, fShow ? ((IntPtr)1) : IntPtr.Zero);
                        return;
                    }
                }
            }
        }
    }
}
