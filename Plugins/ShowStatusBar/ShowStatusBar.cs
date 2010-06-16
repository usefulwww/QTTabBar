using System;
using System.Collections.Generic;
using System.Text;
using QTPlugin;
using QTPlugin.Interop;
using System.Runtime.InteropServices;

namespace QuizoPlugins {
    [Plugin(PluginType.Background, Author = "Quizo", Name = "Show StatusBar", Version = "0.9.0.0", Description = "ShowStatusBar")]
    public class ShowStatusBar : IPluginClient {
        private IPluginServer pluginServer;
        private IShellBrowser shellBrowser;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, StringBuilder lpszClass, string lpszWindow);

        private static bool fVista = IsVista();
        private const int WM_COMMAND = 0x0111;

        private static bool IsVista() {
            return Environment.OSVersion.Version.Major > 5;
        }


        #region IPluginClient members

        public void Open(IPluginServer pluginServer, IShellBrowser shellBrowser) {
            this.pluginServer = pluginServer;
            this.shellBrowser = shellBrowser;
        }

        public bool QueryShortcutKeys(out string[] actions) {
            actions = new string[] { "Show statusbar" };
            return true;
        }

        public void Close(EndCode endCode) {

        }

        public bool HasOption {
            get {
                return false;
            }
        }

        public void OnMenuItemClick(MenuType menuType, string menuText, ITab tab) {
        }

        public void OnOption() {
        }

        public void OnShortcutKeyPressed(int index) {
            IntPtr hwndExplr = this.pluginServer.ExplorerHandle;
            IntPtr hwnd = fVista ? FindWindowEx(hwndExplr, IntPtr.Zero, new StringBuilder("ShellTabWindowClass"), null) : hwndExplr;

            int command = 0xA202;

            if(fVista)
                SendMessage(hwnd, WM_COMMAND, (IntPtr)command, IntPtr.Zero);
            else
                PostMessage(hwnd, WM_COMMAND, (IntPtr)command, IntPtr.Zero);
        }

        #endregion

    }
}
