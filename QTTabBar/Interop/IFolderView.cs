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
    using BandObjectLib;
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, SuppressUnmanagedCodeSecurity, Guid("cde725b0-ccc9-4519-917e-325d72fab4ce"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IFolderView {
        [PreserveSig]
        int GetCurrentViewMode(ref int pViewMode);
        [PreserveSig]
        int SetCurrentViewMode(int ViewMode);
        [PreserveSig]
        int GetFolder(ref Guid riid, out IPersistFolder2 ppv);
        [PreserveSig]
        int Item(int iItemIndex, out IntPtr ppidl);
        [PreserveSig]
        int ItemCount(uint uFlags, out int pcItems);
        [PreserveSig]
        int Items(uint uFlags, ref Guid riid, out IEnumIDList ppv);
        [PreserveSig]
        int GetSelectionMarkedItem(out int piItem);
        [PreserveSig]
        int GetFocusedItem(out int piItem);
        [PreserveSig]
        int GetItemPosition(IntPtr pidl, out POINT ppt);
        [PreserveSig]
        int GetSpacing(ref POINT ppt);
        [PreserveSig]
        int GetDefaultSpacing(ref POINT ppt);
        [PreserveSig]
        int GetAutoArrange();
        [PreserveSig]
        int SelectItem(int iItem, uint dwFlags);
        [PreserveSig]
        int SelectAndPositionItems(uint cidl, IntPtr apidl, IntPtr apt, int dwFlags);
    }
}
