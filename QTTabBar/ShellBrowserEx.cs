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
using System.Text;
using BandObjectLib;
using QTPlugin;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    public class ShellBrowserEx : IDisposable {
        private IShellBrowser shellBrowser;

        public ShellBrowserEx(IShellBrowser shellBrowser) {
            this.shellBrowser = shellBrowser;
        }

        public int ViewMode {
            get {
                int pViewMode = 0;
                using(FVWrapper w = GetFolderView()) {
                    return w.FolderView.GetCurrentViewMode(ref pViewMode) == 0 
                            ? pViewMode : FVM.ICON;
                }
            }
            set {
                using(FVWrapper w = GetFolderView()) {
                    w.FolderView.SetCurrentViewMode(value);
                }
            }
        }

        public void Dispose() {
            if(shellBrowser != null) {
                Marshal.FinalReleaseComObject(shellBrowser);
                shellBrowser = null;
            }
        }

        private FVWrapper GetFolderView() {
            IShellView ppshv;
            if(shellBrowser.QueryActiveShellView(out ppshv) == 0) {
                IFolderView view = ppshv as IFolderView;
                if(view != null) {
                    return new FVWrapper(view);
                }
            }
            return null;
        }

        public IShellBrowser GetIShellBrowser() {
            return shellBrowser;
        }

        public IDLWrapper GetItem(int idx, bool noAppend = false) {
            using(FVWrapper w = GetFolderView()) {
                IntPtr ppidl = IntPtr.Zero;
                try {
                    w.FolderView.Item(idx, out ppidl);
                    if(noAppend || ppidl == IntPtr.Zero) {
                        return new IDLWrapper(ppidl);
                    }
                    using(IDLWrapper path = GetShellPath(w.FolderView)) {
                        return new IDLWrapper(PInvoke.ILCombine(path.PIDL, ppidl));
                    }
                }
                finally {
                    if(ppidl != IntPtr.Zero && !noAppend) {
                        PInvoke.CoTaskMemFree(ppidl);
                    }
                }
            }
        }

        private static IDLWrapper GetShellPath(IFolderView folderView) {
            IPersistFolder2 ppv = null;
            try {
                Guid riid = ExplorerGUIDs.IID_IPersistFolder2;
                if(folderView.GetFolder(ref riid, out ppv) == 0) {
                    IntPtr ptr;
                    ppv.GetCurFolder(out ptr);
                    return new IDLWrapper(ptr);
                }
            }
            catch {
            }
            finally {
                if(ppv != null) {
                    Marshal.ReleaseComObject(ppv);
                }
            }
            return new IDLWrapper(IntPtr.Zero);
        }

        public IDLWrapper GetShellPath() {
            using(FVWrapper w = GetFolderView()) {
                return GetShellPath(w.FolderView);
            }
        }

        public IDLWrapper ILAppend(IntPtr ptr) {
            if(ptr == IntPtr.Zero) {
                return new IDLWrapper(ptr);
            }
            using(IDLWrapper path = GetShellPath()) {
                return new IDLWrapper(path.Available
                        ? PInvoke.ILCombine(path.PIDL, ptr)
                        : IntPtr.Zero);
            }
        }

        public bool IsFolderTreeVisible() {
            IntPtr ptr;
            return IsFolderTreeVisible(out ptr);
        }

        public bool IsFolderTreeVisible(out IntPtr hwnd) {
            hwnd = IntPtr.Zero;
            return (!QTUtility.IsVista && (0 == shellBrowser.GetControlWindow(3, out hwnd)));
        }

        // TODO: make flags an enum
        public int Navigate(IDLWrapper idlw, uint flags = 1u) {
            if(idlw.Available) {
                try {
                    return shellBrowser.BrowseObject(idlw.PIDL, flags);
                }
                catch(COMException) {
                }
            }
            return 1;
        }

        internal void SetStatusText(string status) {
            shellBrowser.SetStatusTextSB(status);
        }


        public bool TryGetHotTrackPath(int iItem, out string path) {
            return TryGetHotTrackPath(iItem, out path, null);
        }

        public bool TryGetHotTrackPath(int iItem, out string path, string matchName) {
            path = null;
            try {
                using(IDLWrapper wrapper = GetItem(iItem, true)) {
                    if(wrapper.Available) {
                        if(!string.IsNullOrEmpty(matchName) && matchName != wrapper.ParseName) {
                            return false;
                        }
                        using(IDLWrapper wrapper2 = ILAppend(wrapper.PIDL)) {
                            path = wrapper2.ParseName;
                            if(!string.IsNullOrEmpty(path)) {
                                return true;
                            }
                            path = null;
                        }
                    }
                }
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception, null);
            }
            return false;
        }

        public bool TryGetSelection(out Address[] adSelectedItems, out string pathFocused, bool fDisplayName) {
            adSelectedItems = new Address[0];
            pathFocused = string.Empty;
            List<Address> list = new List<Address>();
            IShellFolder shellFolder = null;
            IEnumIDList list2 = null;
            try {
                using(FVWrapper w = GetFolderView())
                using(IDLWrapper zero = GetShellPath(w.FolderView)) {
                    IFolderView view2 = w.FolderView;
                    int focusedIndex;
                    IntPtr focusedIDL;
                    int itemCount;
                    Guid guid2 = ExplorerGUIDs.IID_IEnumIDList;
                    if(!ShellMethods.GetShellFolder(zero.PIDL, out shellFolder)) {
                        return false;
                    }
                    if((view2.GetFocusedItem(out focusedIndex) == 0) && (view2.Item(focusedIndex, out focusedIDL) == 0)) {
                        STRRET strret;
                        IntPtr pv = PInvoke.ILCombine(zero.PIDL, focusedIDL);
                        StringBuilder pszBuf = new StringBuilder(260);
                        if(shellFolder.GetDisplayNameOf(focusedIDL, 0x8000, out strret) == 0) {
                            PInvoke.StrRetToBuf(ref strret, focusedIDL, pszBuf, pszBuf.Capacity);
                        }
                        pathFocused = pszBuf.ToString();
                        PInvoke.CoTaskMemFree(focusedIDL);
                        PInvoke.CoTaskMemFree(pv);
                    }
                    if(view2.ItemCount(1, out itemCount) != 0) {
                        return false;
                    }
                    if(itemCount != 0) {
                        IntPtr ptr4;
                        if((view2.Items(1, ref guid2, out list2) != 0) || (list2 == null)) {
                            return false;
                        }
                        uint uFlags = fDisplayName ? 0 : 0x8000u;
                        while(list2.Next(1, out ptr4, null) == 0) {
                            STRRET strret2;
                            StringBuilder builder2 = new StringBuilder(260);
                            if(shellFolder.GetDisplayNameOf(ptr4, uFlags, out strret2) == 0) {
                                PInvoke.StrRetToBuf(ref strret2, ptr4, builder2, builder2.Capacity);
                            }
                            IntPtr pidl = PInvoke.ILCombine(zero.PIDL, ptr4);
                            list.Add(new Address(pidl, builder2.ToString()));
                            PInvoke.CoTaskMemFree(ptr4);
                            PInvoke.CoTaskMemFree(pidl);
                        }
                        adSelectedItems = list.ToArray();
                    }
                    return true;
                }
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception, null);
            }
            finally {
                if(shellFolder != null) {
                    Marshal.ReleaseComObject(shellFolder);
                }
                if(list2 != null) {
                    Marshal.ReleaseComObject(list2);
                }
            }
            return false;
        }

        // TODO: Clean
        public bool TrySetSelection(Address[] addresses, string pathToFocus, bool fDeselectOthers) {
            if(addresses != null) {
                IShellFolder ppshf = null;
                IShellView ppshv = null;
                try {
                    if(shellBrowser.QueryActiveShellView(out ppshv) == 0) {
                        IntPtr ptr3;
                        if(PInvoke.SHGetDesktopFolder(out ppshf) != 0) {
                            return false;
                        }
                        bool flag = true;
                        bool flag2 = false;
                        bool flag3 = !string.IsNullOrEmpty(pathToFocus);
                        uint pchEaten = 0;
                        uint pdwAttributes = 0;
                        if(fDeselectOthers) {
                            ((IFolderView)ppshv).SelectItem(0, 4);
                        }
                        foreach(Address address in addresses) {
                            IntPtr zero = IntPtr.Zero;
                            if((address.ITEMIDLIST != null) && (address.ITEMIDLIST.Length > 0)) {
                                zero = ShellMethods.CreateIDL(address.ITEMIDLIST);
                            }
                            if((((zero != IntPtr.Zero) || (address.Path == null)) || ((address.Path.Length <= 0) || (ppshf.ParseDisplayName(IntPtr.Zero, IntPtr.Zero, address.Path, ref pchEaten, out zero, ref pdwAttributes) == 0))) && (zero != IntPtr.Zero)) {
                                IntPtr pidlItem = PInvoke.ILFindLastID(zero);
                                uint uFlags = 1;
                                if(flag) {
                                    uFlags |= 8;
                                    if(!flag3) {
                                        flag2 = true;
                                        uFlags |= 0x10;
                                    }
                                    if(fDeselectOthers) {
                                        uFlags |= 4;
                                    }
                                    flag = false;
                                }
                                if((!flag2 && flag3) && (address.Path == pathToFocus)) {
                                    flag2 = true;
                                    uFlags |= 0x10;
                                }
                                ppshv.SelectItem(pidlItem, uFlags);
                                PInvoke.CoTaskMemFree(zero);
                            }
                        }
                        if((!flag2 && flag3) && (ppshf.ParseDisplayName(IntPtr.Zero, IntPtr.Zero, pathToFocus, ref pchEaten, out ptr3, ref pdwAttributes) == 0)) {
                            IntPtr ptr4 = PInvoke.ILFindLastID(ptr3);
                            ppshv.SelectItem(ptr4, 0x18);
                            PInvoke.CoTaskMemFree(ptr3);
                        }
                        return true;
                    }
                }
                catch(Exception exception) {
                    QTUtility2.MakeErrorLog(exception, null);
                }
                finally {
                    if(ppshv != null) {
                        Marshal.ReleaseComObject(ppshv);
                    }
                    if(ppshf != null) {
                        Marshal.ReleaseComObject(ppshf);
                    }
                }
            }
            return false;
        }

        private class FVWrapper : IDisposable {
            public FVWrapper(IFolderView folderView) {
                FolderView = folderView;
            }

            public IFolderView FolderView { get; private set; }

            public void Dispose() {
                if(FolderView != null) {
                    Marshal.ReleaseComObject(FolderView);
                    FolderView = null;
                }
            }
        }
    }
}
