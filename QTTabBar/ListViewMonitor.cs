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
using System.Windows.Forms;
using QTTabBarLib.Automation;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    class ListViewMonitor : IDisposable {
        public event EventHandler ListViewChanged;
        
        private AutomationManager AutoMan;
        private IntPtr hwndShellContainer;
        private NativeWindowController ContainerController;
        private IntPtr hwndEnumResult;
        private ShellBrowserEx ShellBrowser;
        private IntPtr hwndExplorer;
        private IntPtr hwndSubDirTipMessageReflect;
        private bool fIsSysListView;
        private bool fDisposed;

        internal ListViewMonitor(ShellBrowserEx shellBrowser, IntPtr hwndExplorer, IntPtr hwndSubDirTipMessageReflect) {
            ShellBrowser = shellBrowser;
            this.hwndExplorer = hwndExplorer;
            this.hwndSubDirTipMessageReflect = hwndSubDirTipMessageReflect;
            if(QTUtility.IsVista) {
                hwndEnumResult = IntPtr.Zero;
                PInvoke.EnumChildWindows(hwndExplorer, CallbackEnumChildProc_Container, IntPtr.Zero);
                hwndShellContainer = hwndEnumResult;
            }
            else {
                hwndShellContainer = hwndExplorer;
            }
            if(hwndShellContainer != IntPtr.Zero) {
                ContainerController = new NativeWindowController(hwndShellContainer);
                ContainerController.MessageCaptured += ContainerController_MessageCaptured;
            }
        }

        public AbstractListView CurrentListView { get; private set; }

        private bool CallbackEnumChildProc_Container(IntPtr hwnd, IntPtr lParam) {
            if(PInvoke.GetClassName(hwnd) == "ShellTabWindowClass") {
                hwndEnumResult = hwnd;
                return false;
            }
            return true;
        }

        private bool ContainerController_MessageCaptured(ref Message msg) {
            if(msg.Msg == WM.PARENTNOTIFY && PInvoke.LoWord((int)msg.WParam) == WM.CREATE) {
                string name = PInvoke.GetClassName(msg.LParam);
                if(name == "SHELLDLL_DefView") {
                    RecaptureHandles(msg.LParam);
                }
            }
            return false;
        }

        private bool CallbackEnumChildProc_ListView(IntPtr hwnd, IntPtr lParam) {
            string name = PInvoke.GetClassName(hwnd);
            if(name == "SysListView32") {
                fIsSysListView = true;
                hwndEnumResult = hwnd;
                return false;
            }
            else if(QTUtility.IsVista && name == "DirectUIHWND") {
                fIsSysListView = false;
                hwndEnumResult = hwnd;
                return false;
            }
            return true;
        }

        private bool CallbackEnumChildProc_ShellView(IntPtr hwnd, IntPtr lParam) {
            if(PInvoke.GetClassName(hwnd) == "SHELLDLL_DefView") {
                hwndEnumResult = hwnd;
                return false;
            }
            return true;
        }

        public void Initialize() {
            hwndEnumResult = IntPtr.Zero;
            PInvoke.EnumChildWindows(hwndExplorer, CallbackEnumChildProc_ShellView, IntPtr.Zero);
            if(hwndEnumResult == IntPtr.Zero) {
                if(CurrentListView != null) {
                    CurrentListView.Dispose();
                }
                CurrentListView = new AbstractListView();
                ListViewChanged(this, null);
            }
            else {
                RecaptureHandles(hwndEnumResult);
            }
        }

        private void RecaptureHandles(IntPtr hwndShellView) {
            if(CurrentListView != null) {
                CurrentListView.Dispose();
            }

            hwndEnumResult = IntPtr.Zero;
            PInvoke.EnumChildWindows(hwndShellView, CallbackEnumChildProc_ListView, IntPtr.Zero);
            if(hwndEnumResult == IntPtr.Zero) {
                CurrentListView = new AbstractListView();
            }
            else if(fIsSysListView) {
                CurrentListView = new ExtendedSysListView32(ShellBrowser, hwndShellView, hwndEnumResult, hwndSubDirTipMessageReflect);
            }
            else {
                if(AutoMan == null) {
                    AutoMan = new AutomationManager();
                }
                CurrentListView = new ExtendedItemsView(ShellBrowser, hwndShellView, hwndEnumResult, hwndSubDirTipMessageReflect, AutoMan);
            }
            CurrentListView.ListViewDestroyed += ListView_Destroyed;
            ListViewChanged(this, null);
        }

        private void ListView_Destroyed(object sender, EventArgs args) {
            if(CurrentListView == sender) {
                CurrentListView.Dispose();
                CurrentListView = new AbstractListView();
                ListViewChanged(this, null);
            }
        }

        #region IDisposable Members

        public void Dispose() {
            if(fDisposed) return;
            if(CurrentListView != null) {
                CurrentListView.Dispose();
                CurrentListView = null;
            }
            if(AutoMan != null) {
                AutoMan.Dispose();
                AutoMan = null;
            }
            fDisposed = true;
        }

        #endregion
    }
}
