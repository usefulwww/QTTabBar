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

namespace QTTabBarLib.Interop {
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, Guid("000214F4-0000-0000-c000-000000000046"), SuppressUnmanagedCodeSecurity, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IContextMenu2 {
        [PreserveSig]
        int QueryContextMenu(IntPtr hMenu, uint indexMenu, uint idCmdFirst, uint idCmdLast, uint uFlags);
        [PreserveSig]
        int InvokeCommand(ref CMINVOKECOMMANDINFO pici);
        void GetCommandString(uint idCmd, uint uFlags, ref int pwReserved, IntPtr commandstring, uint cch);
        [PreserveSig]
        int HandleMenuMsg(int uMsg, IntPtr wParam, IntPtr lParam);
    }
}
