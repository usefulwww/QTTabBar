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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    static class HookLibManager {
        private static bool fShellBrowserIsHooked;
        private static readonly HookLibCallback fpHookResult = HookResult;
        private static IntPtr hHookLib;
        private static int[] hookStatus = Enumerable.Repeat(-1, Enum.GetNames(typeof(Hooks)).Length).ToArray();
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void HookLibCallback(int hookId, int retcode);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int InitShellBrowserHookDelegate(IntPtr shellBrowser);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int InitHookLibDelegate(HookLibCallback fpHookResult);

        public enum HookCheckPoint{
            Initial,
            ShellBrowser,
            NewWindow,
            Automation,
        }

        // Unmarked hooks exist only to set other hooks.
        private enum Hooks {
            CoCreateInstance = 0,           // Treeview Middle-click
            RegisterDragDrop,               // DragDrop into SubDirTips
            SHCreateShellFolderView,
            BrowseObject,                   // Control Panel dialog OK/Cancel buttons
            CreateViewWindow3,              // Header in all Views
            MessageSFVCB,                   // Refresh = clear text
            UiaReturnRawElementProvider,
            QueryInterface,                 // Scrolling Lag workaround
            TravelToEntry,                  // Clear Search bar = back
            OnActivateSelection,            // Recently activated files
            SetNavigationState,             // Breadcrumb Bar Middle-click
            CreateInstance,
            ShowWindow,                     // New Explorer window capturing
            UpdateWindowList,               // Compatibility with SHOpenFolderAndSelectItems
        }

        static HookLibManager() {
            string installPath = "";
            using(RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Quizo\QTTabBar")) {
                if(key != null) {
                    installPath = (string)key.GetValue("InstallPath", "");
                }
            }
            string filename = IntPtr.Size == 8 ? "QTHookLib64.dll" : "QTHookLib32.dll";
            hHookLib = PInvoke.LoadLibrary(Path.Combine(installPath, filename));
            int retcode = -1;
            if(hHookLib == IntPtr.Zero) {
                int error = Marshal.GetLastWin32Error();
                QTUtility2.MakeErrorLog(null, "LoadLibrary error: " + error);
            }
            else {
                IntPtr pFunc = PInvoke.GetProcAddress(hHookLib, "Initialize");
                if(pFunc != IntPtr.Zero) {
                    InitHookLibDelegate initialize = (InitHookLibDelegate)
                            Marshal.GetDelegateForFunctionPointer(pFunc, typeof(InitHookLibDelegate));
                    try {
                        retcode = initialize(fpHookResult);
                    }
                    catch(Exception e) {
                        QTUtility2.MakeErrorLog(e, "");
                    }

                }
            }

            if(retcode == 0) return;
            QTUtility2.MakeErrorLog(null, "HookLib Initialize failed: " + retcode);

            // TODO: Localize this
            MessageForm.Show(IntPtr.Zero,
                    "Error:  Unable to load QTTabBar hook library.  " +
                            "Some features might not be functional.  " +
                                    "You may need to reinstall the applicaiton.",
                    "Error", MessageBoxIcon.Hand, 30000, false, true);
        }

        private static void HookResult(int hookId, int retcode) {
            lock(fpHookResult) {
                hookStatus[hookId] = retcode;
            }
        }

        public static void InitShellBrowserHook(IShellBrowser shellBrowser) {
            lock(typeof(HookLibManager)) {
                if(fShellBrowserIsHooked || hHookLib == IntPtr.Zero) return;
                IntPtr pFunc = PInvoke.GetProcAddress(hHookLib, "InitShellBrowserHook");
                if(pFunc == IntPtr.Zero) return;
                InitShellBrowserHookDelegate initShellBrowserHook = (InitShellBrowserHookDelegate)
                        Marshal.GetDelegateForFunctionPointer(pFunc, typeof(InitShellBrowserHookDelegate));
                IntPtr pShellBrowser = Marshal.GetComInterfaceForObject(shellBrowser, typeof(IShellBrowser));
                if(pShellBrowser == IntPtr.Zero) return;
                int retcode = -1;
                try {
                    retcode = initShellBrowserHook(pShellBrowser);
                }
                catch(Exception e) {
                    QTUtility2.MakeErrorLog(e, "");
                }
                finally {
                    Marshal.Release(pShellBrowser);
                }
                if(retcode != 0) {
                    QTUtility2.MakeErrorLog(null, "InitShellBrowserHook failed: " + retcode);

                    // TODO: Localize this
                    MessageForm.Show(IntPtr.Zero,
                            "Error:  Unable to initialize QTTabBar hook library.  " +
                            "Some features might not be functional.  " +
                            "You may need to reinstall the applicaiton.",
                            "Error", MessageBoxIcon.Hand, 30000, false, true);
                }
                else {
                    fShellBrowserIsHooked = true;
                }
            }
        }

        public static void CheckHooks() {
            // TODO
        }
    }
}
