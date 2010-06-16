using QTPlugin;
using QTPlugin.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace QuizoPlugins {
    [Plugin(PluginType.Background, Author = "Quizo", Name = "CreateNewItemButton", Version = "0.9.0.0", Description = "Create New Item by shortcut key.")]
    public class CreateNewItem : IPluginClient {
        private IPluginServer pluginServer;
        private IShellBrowser shellBrowser;

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr ILCreateFromPath([MarshalAs(UnmanagedType.LPTStr)] string pszPath);

        [DllImport("shell32.dll")]
        private static extern IntPtr ILFindLastID(IntPtr pidl);

        #region IPluginClient Members

        public void Open(IPluginServer pluginServer, IShellBrowser shellBrowser) {
            this.pluginServer = pluginServer;
            this.shellBrowser = shellBrowser;
        }

        public bool QueryShortcutKeys(out string[] actions) {
            actions = new string[] { "Create a new folder", "Create a new text file" };
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
            const uint SVSI_SELECT = 0x00000001;
            const uint SVSI_EDIT = 0x00000003;
            const uint SVSI_DESELECTOTHERS = 0x00000004;
            const uint SVSI_ENSUREVISIBLE = 0x00000008;

            // Create folder or file
            IShellView shellView = null;
            IntPtr pIDL = IntPtr.Zero;

            try {
                string path = this.pluginServer.SelectedTab.Address.Path;

                if(String.IsNullOrEmpty(path) || !Directory.Exists(path)) {
                    System.Media.SystemSounds.Hand.Play();
                    return;
                }

                // make new name
                bool fDir = index == 0;
                int i = 2;
                string name = fDir ? "New folder" : "New Text Document";
                string ext = fDir ? String.Empty : ".txt";
                string pathNew = path + "\\" + name + ext;

                while((fDir && Directory.Exists(pathNew)) || (!fDir && File.Exists(pathNew))) {
                    pathNew = path + "\\" + name + " (" + i + ")" + ext;
                    i++;
                }

                if(fDir) {
                    Directory.CreateDirectory(pathNew);
                }
                else {
                    using(File.Create(pathNew)) {
                    }
                }


                // Select and put into rename mode.
                if(0 == this.shellBrowser.QueryActiveShellView(out shellView)) {
                    shellView.Refresh();

                    pIDL = ILCreateFromPath(pathNew);
                    if(pIDL != IntPtr.Zero) {
                        IntPtr pIDLRltv = ILFindLastID(pIDL);
                        if(pIDLRltv != IntPtr.Zero) {
                            shellView.SelectItem(pIDLRltv, SVSI_SELECT | SVSI_DESELECTOTHERS | SVSI_ENSUREVISIBLE | SVSI_EDIT);
                            return;
                        }
                    }
                }
            }
            catch {
            }
            finally {
                if(shellView != null)
                    Marshal.ReleaseComObject(shellView);

                if(pIDL != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(pIDL);
            }

            System.Media.SystemSounds.Hand.Play();
        }

        #endregion

    }
}
