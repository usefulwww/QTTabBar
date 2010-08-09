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
using System.Linq;
using System.Runtime.InteropServices;
using BandObjectLib;
using QTPlugin;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    public class ShellBrowserEx : IDisposable {
        private IShellBrowser shellBrowser;
        private bool shared;
        public ShellBrowserEx(IShellBrowser shellBrowser, bool shared = false) {
            this.shellBrowser = shellBrowser;
            this.shared = shared;
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
                if(shared) {
                    Marshal.ReleaseComObject(shellBrowser);
                }
                else {
                    Marshal.FinalReleaseComObject(shellBrowser);
                }
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

        public bool SelectionAvailable() {
            using(FVWrapper w = GetFolderView()) {
                int items;
                return w.FolderView.ItemCount(1, out items) == 0;
            }
        }

        public IEnumerable<IDLWrapper> GetItems(bool selectedOnly = false, bool noAppend = false) {
            Guid guid = ExplorerGUIDs.IID_IEnumIDList;
            IEnumIDList list = null;
            try {
                using(FVWrapper w = GetFolderView())
                using(IDLWrapper path = noAppend ? null : GetShellPath(w.FolderView)) {
                    w.FolderView.Items(0x80000000 | (selectedOnly ? 1u : 2u), ref guid, out list);
                    if(list == null) {
                        yield break;
                    }
                    IntPtr ptr;
                    while(list.Next(1, out ptr, null) == 0) {
                        using(IDLWrapper wrapper1 = new IDLWrapper(ptr)) {
                            if(!wrapper1.Available) continue;
                            if(noAppend) {
                                yield return wrapper1;
                            }
                            else {
                                using(IDLWrapper wrapper2 = new IDLWrapper(PInvoke.ILCombine(path.PIDL, wrapper1.PIDL))) {                                
                                    yield return wrapper2;
                                }
                            }
                        }
                    }
                }
            }
            finally {
                if(list != null) {
                    Marshal.ReleaseComObject(list);
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
        
        public IDLWrapper GetFocusedItem() {
            int focusedIndex;
            using(FVWrapper w = GetFolderView()) {
                if(w.FolderView.GetFocusedItem(out focusedIndex) != 0) {
                    return new IDLWrapper(IntPtr.Zero);
                }
            }
            return GetItem(focusedIndex);
        }

        public bool TryGetSelection(out Address[] adSelectedItems, bool fDisplayName) {
            if(!SelectionAvailable()) {
                adSelectedItems = new Address[0];
                return false;
            }

            adSelectedItems = GetItems(true).Select(wrapper => fDisplayName 
                     ? new Address(wrapper.PIDL, wrapper.DisplayName)
                     : new Address(wrapper.PIDL, wrapper.ParseName)).ToArray();
            return true;
        }

        public bool TryGetSelection(out Address[] adSelectedItems, out string pathFocused, bool fDisplayName) {
            using(IDLWrapper wrapper = GetFocusedItem()) {
                pathFocused = wrapper.ParseName;
            }
            return TryGetSelection(out adSelectedItems, fDisplayName);
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
