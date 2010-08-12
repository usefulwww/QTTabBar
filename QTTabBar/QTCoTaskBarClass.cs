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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using BandObjectLib;
using Microsoft.Win32;
using QTPlugin;
using QTTabBarLib.Interop;
using Timer = System.Windows.Forms.Timer;

namespace QTTabBarLib {
    [Guid("D2BF470E-ED1C-487F-A555-2BD8835EB6CE"), ComVisible(true)]
    public sealed class QTCoTaskBarClass : BandObject, IPersistStream, IDeskBand2 {
        private VisualStyleRenderer bgRenderer;
        private Process BlockedExplorerProcess;
        private string BlockedExplorerURL;
        private bool CancelClosing;
        private IContainer components;
        private byte[] ConfigValues;
        private DropDownMenuReorderable contextMenu;
        private ContextMenuStripEx contextMenuForSetting;
        private DropDownMenuReorderable ddmrGroups;
        private DropDownMenuReorderable ddmrHistory;
        private DropDownMenuReorderable ddmrRecentFile;
        private DropDownMenuReorderable ddmrUserapps;
        private bool[] ExpandState = new bool[4];
        private bool fContextmenuedOnMain_Grp;
        private bool fRequiredRefresh_MenuItems;
        private List<ToolStripItem> GroupItemsList = new List<ToolStripItem>();
        private TitleMenuItem groupsMenuItem;
        private FileHashComputerForm hashForm;
        private IntPtr hHook_KeyDesktop;
        private IntPtr hHook_MsgShell_TrayWnd;
        private IntPtr hHook_MsgDesktop;
        private TitleMenuItem historyMenuItem;
        private HookProc hookProc_Keys_Desktop;
        private HookProc hookProc_Msg_Desktop;
        private HookProc hookProc_Msg_ShellTrayWnd;
        private IntPtr hwndListView;
        private IntPtr hwndShellTray;
        private static IContextMenu2 iContextMenu2;
        private int iHookTimeout;
        private int iMainMenuShownCount;
        private TitleMenuItem labelGroupTitle;
        private TitleMenuItem labelHistoryTitle;
        private TitleMenuItem labelRecentFileTitle;
        private TitleMenuItem labelUserAppTitle;
        private ExtendedSysListView32 listView;
        private const string MENUKEY_ITEM_GROUP = "groupItem";
        private const string MENUKEY_ITEM_HISTORY = "historyItem";
        private const string MENUKEY_ITEM_RECENT = "recentItem";
        private const string MENUKEY_ITEM_USERAPP = "userappItem";
        private const string MENUKEY_LABEL_GROUP = "labelG";
        private const string MENUKEY_LABEL_HISTORY = "labelH";
        private const string MENUKEY_LABEL_RECENT = "labelR";
        private const string MENUKEY_LABEL_USERAPP = "labelU";
        private const string MENUKEY_LABELS = "label";
        private const string MENUKEY_SUBMENUS = "submenu";
        private static QTCoTaskBarClass Myself;
        private bool NowMouseHovering;
        private byte[] Order_Root;
        private List<ToolStripItem> RecentFileItemsList = new List<ToolStripItem>();
        private TitleMenuItem recentFileMenuItem;
        private ShellBrowserEx ShellBrowser;
        private StringFormat stringFormat;
        private const string TEXT_TOOLBAR = "QTTab Desktop Tool";
        private IntPtr ThisHandle;
        private Timer timerHooks;
        private Timer timerISV;
        private ToolStripMenuItem tsmiAppKeys;
        private ToolStripMenuItem tsmiDesktop;
        private ToolStripMenuItem tsmiLockItems;
        private ToolStripMenuItem tsmiOneClick;
        private ToolStripMenuItem tsmiOnGroup;
        private ToolStripMenuItem tsmiOnHistory;
        private ToolStripMenuItem tsmiOnRecentFile;
        private ToolStripMenuItem tsmiOnUserApps;
        private ToolStripMenuItem tsmiTaskBar;
        private ToolStripMenuItem tsmiVSTitle;
        private const string TSS_NAME_APP = "appSep";
        private const string TSS_NAME_GRP = "groupSep";
        private List<ToolStripItem> UndoClosedItemsList = new List<ToolStripItem>();
        private List<ToolStripItem> UserappItemsList = new List<ToolStripItem>();
        private TitleMenuItem userAppsMenuItem;
        private int WidthOfBar;
        
        private delegate void DisposeInvoker(IDisposable diposable);

        public QTCoTaskBarClass() {
            byte[] buffer = new byte[3];
            buffer[1] = 1;
            buffer[2] = 2;
            Order_Root = buffer;
            byte[] buffer2 = new byte[4];
            buffer2[0] = 1;
            ConfigValues = buffer2;
            WidthOfBar = 80;
        }

        private void AddMenuItems_Group() {
            if((ConfigValues[2] & 0x80) == 0) {
                if(ExpandState[0]) {
                    contextMenu.AddItem(labelGroupTitle, "labelG");
                    contextMenu.AddItemsRange(GroupItemsList.ToArray(), "groupItem");
                }
                else {
                    ddmrGroups.AddItemsRange(GroupItemsList.ToArray(), "groupItem");
                    contextMenu.AddItem(groupsMenuItem, "submenu");
                }
            }
        }

        private void AddMenuItems_History() {
            if(((ConfigValues[2] & 0x40) == 0) && !QTUtility.CheckConfig(Settings.NoHistory)) {
                if(ExpandState[1]) {
                    contextMenu.AddItem(labelHistoryTitle, "labelH");
                    contextMenu.AddItemsRange(UndoClosedItemsList.ToArray(), "historyItem");
                }
                else {
                    ddmrHistory.AddItemsRange(UndoClosedItemsList.ToArray(), "historyItem");
                    contextMenu.AddItem(historyMenuItem, "submenu");
                }
            }
        }

        private void AddMenuItems_Recent() {
            if(((ConfigValues[2] & 1) == 0) && !QTUtility.CheckConfig(Settings.NoRecentFiles)) {
                if(ExpandState[3]) {
                    contextMenu.AddItem(labelRecentFileTitle, "labelR");
                    contextMenu.AddItemsRange(RecentFileItemsList.ToArray(), "recentItem");
                }
                else {
                    ddmrRecentFile.AddItemsRange(RecentFileItemsList.ToArray(), "recentItem");
                    contextMenu.AddItem(recentFileMenuItem, "submenu");
                }
            }
        }

        private void AddMenuItems_UserApp() {
            if((ConfigValues[2] & 0x20) == 0) {
                if(ExpandState[2]) {
                    contextMenu.AddItem(labelUserAppTitle, "labelU");
                    contextMenu.AddItemsRange(UserappItemsList.ToArray(), "userappItem");
                }
                else {
                    ddmrUserapps.AddItemsRange(UserappItemsList.ToArray(), "userappItem");
                    contextMenu.AddItem(userAppsMenuItem, "submenu");
                }
            }
        }

        void IPersistStream.GetClassID(out Guid pClassID) {
            pClassID = new Guid("D2BF470E-ED1C-487F-A555-2BD8835EB6CE");
        }

        int IPersistStream.GetSizeMax(out ulong pcbSize) {
            pcbSize = 0L;
            return 0;
        }

        void IPersistStream.IPersistStreamLoad(object pStm) {
        }

        int IPersistStream.IsDirty() {
            return 0;
        }

        void IPersistStream.Save(IntPtr pStm, bool fClearDirty) {
        }

        private void BlockedExplorer_Exited(object sender, EventArgs e) {
            Process.Start(BlockedExplorerURL);
            BlockedExplorerURL = string.Empty;
            BlockedExplorerProcess.Close();
            BlockedExplorerProcess = null;
        }

        private IntPtr CallbackGetMsgProc_Desktop(int nCode, IntPtr wParam, IntPtr lParam) {
            if(nCode >= 0) {
                MSG msg = (MSG)Marshal.PtrToStructure(lParam, typeof(MSG));
                if(msg.hwnd == hwndListView && msg.message == WM.APP + 100) {
                    IntPtr hwnd = PInvoke.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Progman", null);
                    IntPtr pUnk = PInvoke.SendMessage(hwnd, WM.USER + 7, IntPtr.Zero, IntPtr.Zero);
                    if(pUnk == IntPtr.Zero) {
                        timerISV = new Timer();
                        timerISV.Tag = msg.hwnd;
                        timerISV.Tick += (sender, args) => {
                            PInvoke.PostMessage((IntPtr)timerISV.Tag, WM.APP + 100, IntPtr.Zero, IntPtr.Zero);
                            timerISV.Enabled = false;
                        };
                        timerISV.Interval = 2000;
                        timerISV.Start();
                    }
                    else {
                        ShellBrowser = new ShellBrowserEx((IShellBrowser)Marshal.GetObjectForIUnknown(pUnk), true);

                        listView = new ExtendedSysListView32(ShellBrowser, PInvoke.GetParent(hwndListView), hwndListView, ThisHandle);
                        listView.MouseActivate += ListView_MouseActivate;
                        listView.ItemActivated += ListView_ItemActivated;
                        listView.MiddleClick += ListView_MiddleClick;
                        listView.DoubleClick += ListView_DoubleClick;
                        listView.ListViewDestroyed += ListView_Destroyed;
                        listView.SubDirTip_MenuItemClicked += subDirTip_MenuItemClicked;
                        listView.SubDirTip_MenuItemRightClicked += subDirTip_MenuItemRightClicked;
                        listView.SubDirTip_MultipleMenuItemsClicked += subDirTip_MultipleMenuItemsClicked;
                        listView.SubDirTip_MultipleMenuItemsRightClicked += subDirTip_MultipleMenuItemsRightClicked;
                    }
                }
            }
            return PInvoke.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        private IntPtr CallbackGetMsgProc_ShellTrayWnd(int nCode, IntPtr wParam, IntPtr lParam) {
            if(nCode >= 0) {
                MSG msg = (MSG)Marshal.PtrToStructure(lParam, typeof(MSG));
                switch(msg.message) {
                    case WM.NCLBUTTONDBLCLK:
                        if(((ConfigValues[1] & 8) == 0) && (msg.hwnd == hwndShellTray)) {
                            OnDesktopDblClicked(MousePosition);
                            Marshal.StructureToPtr(new MSG(), lParam, false);
                        }
                        break;

                    case WM.MOUSEWHEEL: {
                            IntPtr handle = PInvoke.WindowFromPoint(new Point(QTUtility2.GET_X_LPARAM(msg.lParam), QTUtility2.GET_Y_LPARAM(msg.lParam)));
                            if((handle != IntPtr.Zero) && (handle != msg.hwnd)) {
                                Control control = FromHandle(handle);
                                if(control != null) {
                                    DropDownMenuReorderable reorderable = control as DropDownMenuReorderable;
                                    if((reorderable != null) && reorderable.CanScroll) {
                                        PInvoke.SendMessage(handle, 0x20a, msg.wParam, msg.lParam);
                                        Marshal.StructureToPtr(new MSG(), lParam, false);
                                    }
                                }
                            }
                            break;
                        }
                }
            }
            return PInvoke.CallNextHookEx(hHook_MsgShell_TrayWnd, nCode, wParam, lParam);
        }

        private IntPtr CallbackKeyProc_Desktop(int nCode, IntPtr wParam, IntPtr lParam) {
            if(nCode >= 0) {
                if((((int)lParam) & -2147483648) == 0) {
                    if(HandleKEYDOWN(wParam, (((int)lParam) & 0x40000000) == 0x40000000)) {
                        return new IntPtr(1);
                    }
                }
                else {
                    listView.HideThumbnailTooltip();
                    if(!listView.SubDirTipMenuIsShowing()) {
                        listView.HideSubDirTip();
                    }
                }
            }
            return PInvoke.CallNextHookEx(hHook_KeyDesktop, nCode, wParam, lParam);
        }

        public void CanRenderComposited(out bool pfCanRenderComposited) {
            pfCanRenderComposited = true;
        }

        private void ClearMenuItems() {
            fRequiredRefresh_MenuItems = false;
            if(contextMenu.Items.Count > 0) {
                contextMenu.ItemsClear();
                ddmrGroups.ItemsClear();
                ddmrHistory.ItemsClear();
                ddmrUserapps.ItemsClear();
                ddmrRecentFile.ItemsClear();
                foreach(ToolStripItem item in GroupItemsList) {
                    item.Dispose();
                }
                foreach(ToolStripItem item2 in UndoClosedItemsList) {
                    item2.Dispose();
                }
                foreach(ToolStripItem item3 in UserappItemsList) {
                    item3.Dispose();
                }
                foreach(ToolStripItem item4 in RecentFileItemsList) {
                    item4.Dispose();
                }
                GroupItemsList.Clear();
                UndoClosedItemsList.Clear();
                UserappItemsList.Clear();
                RecentFileItemsList.Clear();
            }
        }

        // TODO
        private void ClearThumbnailCache() {
            /*
            this.iMainMenuShownCount = 0;
            if(this.subDirTip != null) {
                this.subDirTip.ClearThumbnailCache();
            }
            if(this.thumbnailTooltip != null) {
                listView.HideThumbnailTooltip();
                this.thumbnailTooltip.ClearCache();
            }
            */
        }

        public override void CloseDW(uint dwReserved) {
            if(iContextMenu2 != null) {
                Marshal.ReleaseComObject(iContextMenu2);
                iContextMenu2 = null;
            }
            if(ShellBrowser != null) {
                ShellBrowser.Dispose();
                ShellBrowser = null;
            }
            lock(this) {
                if(listView != null) {
                    listView.RemoteDispose();
                    listView = null;
                }
            }
            DisposeInvoker method = disposable => disposable.Dispose();
            if(hashForm != null) {
                hashForm.Invoke(method, new object[] { hashForm });
                hashForm = null;
            }
            if(hHook_MsgDesktop != IntPtr.Zero) {
                PInvoke.UnhookWindowsHookEx(hHook_MsgDesktop);
                hHook_MsgDesktop = IntPtr.Zero;
            }
            if(hHook_MsgShell_TrayWnd != IntPtr.Zero) {
                PInvoke.UnhookWindowsHookEx(hHook_MsgShell_TrayWnd);
                hHook_MsgShell_TrayWnd = IntPtr.Zero;
            }
            if(hHook_KeyDesktop != IntPtr.Zero) {
                PInvoke.UnhookWindowsHookEx(hHook_KeyDesktop);
                hHook_KeyDesktop = IntPtr.Zero;
            }
            base.CloseDW(dwReserved);
        }

        private void contextMenu_Closing(object sender, ToolStripDropDownClosingEventArgs e) {
            if(CancelClosing) {
                e.Cancel = true;
                CancelClosing = false;
            }
            else {
                List<int> list = new List<int>();
                for(int i = 0; i < contextMenu.Items.Count; i++) {
                    if(list.Count == Order_Root.Length) {
                        break;
                    }
                    ToolStripItem item = contextMenu.Items[i];
                    if(item is TitleMenuItem) {
                        if((item == groupsMenuItem) || (item == labelGroupTitle)) {
                            list.Add(0);
                        }
                        else if((item == historyMenuItem) || (item == labelHistoryTitle)) {
                            list.Add(1);
                        }
                        else if((item == userAppsMenuItem) || (item == labelUserAppTitle)) {
                            list.Add(2);
                        }
                        else if((item == recentFileMenuItem) || (item == labelRecentFileTitle)) {
                            list.Add(3);
                        }
                    }
                }
                for(int j = 0; j < 3; j++) {
                    if(j < list.Count) {
                        Order_Root[j] = (byte)list[j];
                    }
                    else {
                        Order_Root[j] = 15;
                    }
                }
                SaveSetting();
            }
        }

        private void contextMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            TitleMenuItem clickedItem = e.ClickedItem as TitleMenuItem;
            if(clickedItem != null) {
                if(clickedItem.IsOpened) {
                    OnLabelTitleClickedToClose(clickedItem.Genre);
                }
                else {
                    OnSubMenuTitleClickedToOpen(clickedItem.Genre);
                }
            }
            else {
                QMenuItem item2 = e.ClickedItem as QMenuItem;
                if(item2 != null) {
                    if(item2.Genre == MenuGenre.Group) {
                        Keys modifierKeys = ModifierKeys;
                        string text = item2.Text;
                        if(modifierKeys == (Keys.Control | Keys.Shift)) {
                            if(QTUtility.StartUpGroupList.Contains(text)) {
                                QTUtility.StartUpGroupList.Remove(text);
                            }
                            else {
                                QTUtility.StartUpGroupList.Add(text);
                            }
                            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar")) {
                                if(key != null) {
                                    key.SetValue("StartUpGroups", QTUtility.StartUpGroupList.StringJoin(";"));
                                }
                            }
                        }
                        else {
                            Thread thread = new Thread(OpenGroup);
                            thread.SetApartmentState(ApartmentState.STA);
                            thread.IsBackground = true;
                            thread.Start(new object[] { text, ModifierKeys });
                        }
                    }
                    else if(item2.Genre == MenuGenre.History) {
                        Thread thread2 = new Thread(OpenTab);
                        thread2.SetApartmentState(ApartmentState.STA);
                        thread2.IsBackground = true;
                        thread2.Start(new object[] { item2.Path, ModifierKeys });
                    }
                    else if((item2.Genre == MenuGenre.Application) && (item2.Target == MenuTarget.File)) {
                        if(!item2.MenuItemArguments.TokenReplaced) {
                            AppLauncher.ReplaceAllTokens(item2.MenuItemArguments, Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                        }
                        AppLauncher.Execute(item2.MenuItemArguments, IntPtr.Zero);
                    }
                    else if(item2.Genre == MenuGenre.RecentFile) {
                        try {
                            ProcessStartInfo startInfo = new ProcessStartInfo(item2.Path);
                            startInfo.WorkingDirectory = Path.GetDirectoryName(item2.Path);
                            startInfo.ErrorDialog = true;
                            startInfo.ErrorDialogParentHandle = ThisHandle;
                            Process.Start(startInfo);
                            QTUtility.ExecutedPathsList.Add(item2.Path);
                        }
                        catch {
                            SystemSounds.Hand.Play();
                        }
                    }
                }
            }
        }

        private void contextMenu_ReorderFinished(object sender, ToolStripItemClickedEventArgs e) {
            QMenuItem clickedItem = e.ClickedItem as QMenuItem;
            if(clickedItem != null) {
                if(clickedItem.Genre == MenuGenre.Group) {
                    Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar").DeleteSubKey("Groups", false);
                    using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar\Groups")) {
                        int num = 1;
                        foreach(ToolStripItem item2 in contextMenu.Items) {
                            QMenuItem item3 = item2 as QMenuItem;
                            if((item3 != null) && (item3.Genre == MenuGenre.Group)) {
                                key.SetValue(item2.Text, QTUtility.GroupPathsDic[item2.Text]);
                            }
                            else if((item2 is ToolStripSeparator) && (item2.Name == "groupSep")) {
                                key.SetValue("Separator" + num++, string.Empty);
                            }
                        }
                        return;
                    }
                }
                if(clickedItem.Genre == MenuGenre.Application) {
                    using(RegistryKey key2 = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar\UserApps")) {
                        foreach(string str in key2.GetValueNames()) {
                            key2.DeleteValue(str);
                        }
                        int num2 = 1;
                        string[] array = new string[] { "separator", string.Empty, string.Empty };
                        foreach(ToolStripItem item4 in contextMenu.Items) {
                            QMenuItem item5 = item4 as QMenuItem;
                            if(item5 != null) {
                                if(item5.Genre == MenuGenre.Application) {
                                    if(item5.Target == MenuTarget.VirtualFolder) {
                                        key2.SetValue(item4.Text, new byte[0]);
                                    }
                                    else {
                                        QTUtility2.WriteRegBinary(QTUtility.UserAppsDic[item4.Text], item4.Text, key2);
                                    }
                                }
                                continue;
                            }
                            if((item4 is ToolStripSeparator) && (item4.Name == "appSep")) {
                                QTUtility2.WriteRegBinary(array, "Separator" + num2++, key2);
                            }
                        }
                    }
                    QTUtility.fRequiredRefresh_App = true;
                }
            }
        }

        private void contextMenuForSetting_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            if(!(e.ClickedItem is ToolStripSeparator)) {
                if(e.ClickedItem == tsmiTaskBar) {
                    tsmiTaskBar.Checked = !tsmiTaskBar.Checked;
                }
                else if(e.ClickedItem == tsmiDesktop) {
                    tsmiDesktop.Checked = !tsmiDesktop.Checked;
                }
                else if(e.ClickedItem == tsmiLockItems) {
                    tsmiLockItems.Checked = !tsmiLockItems.Checked;
                    fRequiredRefresh_MenuItems = true;
                    contextMenu.ReorderEnabled = ddmrGroups.ReorderEnabled = ddmrUserapps.ReorderEnabled = !tsmiLockItems.Checked;
                }
                else if(e.ClickedItem == tsmiOnGroup) {
                    fRequiredRefresh_MenuItems = true;
                    tsmiOnGroup.Checked = !tsmiOnGroup.Checked;
                }
                else if(e.ClickedItem == tsmiOnHistory) {
                    fRequiredRefresh_MenuItems = true;
                    tsmiOnHistory.Checked = !tsmiOnHistory.Checked;
                }
                else if(e.ClickedItem == tsmiOnUserApps) {
                    fRequiredRefresh_MenuItems = true;
                    tsmiOnUserApps.Checked = !tsmiOnUserApps.Checked;
                }
                else if(e.ClickedItem == tsmiOnRecentFile) {
                    fRequiredRefresh_MenuItems = true;
                    tsmiOnRecentFile.Checked = !tsmiOnRecentFile.Checked;
                }
                else if(e.ClickedItem == tsmiVSTitle) {
                    tsmiVSTitle.Checked = !tsmiVSTitle.Checked;
                    TitleMenuItem.DrawBackground = tsmiVSTitle.Checked;
                }
                else if(e.ClickedItem == tsmiOneClick) {
                    tsmiOneClick.Checked = !tsmiOneClick.Checked;
                }
                else if(e.ClickedItem == tsmiAppKeys) {
                    tsmiAppKeys.Checked = !tsmiAppKeys.Checked;
                    if(tsmiAppKeys.Checked) {
                        ConfigValues[2] = (byte)(ConfigValues[2] & 0xf7);
                    }
                    else {
                        ConfigValues[2] = (byte)(ConfigValues[2] | 8);
                    }
                }
                SaveSetting();
            }
        }

        private void contextMenuForSetting_Opening(object sender, CancelEventArgs e) {
            string[] strArray = QTUtility.TextResourcesDic["TaskBar_Menu"];
            tsmiTaskBar.Text = strArray[0];
            tsmiDesktop.Text = strArray[1];
            tsmiLockItems.Text = strArray[2];
            tsmiVSTitle.Text = strArray[3];
            tsmiOneClick.Text = strArray[4];
            tsmiAppKeys.Text = strArray[5];
            string[] strArray2 = QTUtility.TextResourcesDic["TaskBar_Titles"];
            tsmiOnGroup.Text = strArray2[0];
            tsmiOnHistory.Text = strArray2[1];
            tsmiOnUserApps.Text = strArray2[2];
            tsmiOnRecentFile.Text = strArray2[3];
        }

        protected override void Dispose(bool disposing) {
            if(disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void DoFileTools(int index) {
            try {
                switch(index) {
                    case 0:
                    case 1: {
                        bool flag = index == 0;
                        string str = ShellBrowser.GetItems()
                                .Select(wrapper2 => flag ? wrapper2.ParseName : wrapper2.DisplayName)
                                .Where(name => name.Length > 0).StringJoin("\r\n");
                        if(str.Length > 0) {
                            QTTabBarClass.SetStringClipboard(str);
                        }
                        break;
                    }

                    case 2:
                    case 3: {
                        string folderPath = string.Empty;
                        if(index == 2) {
                            folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                        }
                        else {
                            byte[] idl = new byte[2];
                            using(IDLWrapper wrapper = new IDLWrapper(idl)) {
                                if(wrapper.Available) {
                                    folderPath = ShellMethods.GetDisplayName(wrapper.PIDL, true);
                                }
                            }
                        }
                        if(folderPath.Length > 0) {
                            QTTabBarClass.SetStringClipboard(folderPath);
                        }
                        break;
                    }

                    case 4: {
                        List<string> list2 = new List<string>();
                        foreach(IDLWrapper wrapper2 in ShellBrowser.GetItems(true)) {
                            if(wrapper2.IsLink) {
                                string linkTargetPath = ShellMethods.GetLinkTargetPath(wrapper2.Path);
                                if(File.Exists(linkTargetPath)) {
                                    list2.Add(linkTargetPath);
                                }
                            }
                            else if(wrapper2.IsFileSystemFile) {
                                list2.Add(wrapper2.Path);
                            }
                        }
                        if(hashForm == null) {
                            hashForm = new FileHashComputerForm();
                        }
                        hashForm.ShowFileHashForm(list2.ToArray());
                        break;
                    }

                    case 5:
                        listView.ShowAndClickSubDirTip();
                        break;
                }
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception, null);
            }
        }

        private void dropDownMenues_ItemRightClicked(object sender, ItemRightClickedEventArgs e) {
            QMenuItem clickedItem = e.ClickedItem as QMenuItem;
            if(clickedItem == null) {
                return;
            }
            Point pnt = e.IsKey ? e.Point : MousePosition;
            if(clickedItem.Target == MenuTarget.VirtualFolder) {
                return;
            }
            if(clickedItem.Genre == MenuGenre.Group) {
                fContextmenuedOnMain_Grp = sender == contextMenu;
                string str = MenuUtility.TrackGroupContextMenu(e.ClickedItem.Text, pnt, ((DropDownMenuReorderable)sender).Handle);
                fContextmenuedOnMain_Grp = false;
                if(!string.IsNullOrEmpty(str)) {
                    OpenTab(new object[] { str, ModifierKeys });
                    return;
                }
                e.HRESULT = 0xfffd;
                return;
            }
            bool fCanRemove = clickedItem.Genre != MenuGenre.Application;
            using(IDLWrapper wrapper = new IDLWrapper(clickedItem.Path)) {
                e.HRESULT = ShellMethods.PopUpSystemContextMenu(wrapper, pnt, ref iContextMenu2, ((DropDownMenuReorderable)sender).Handle, fCanRemove);
            }
            if(e.HRESULT != 0xffff) {
                return;
            }
            if(clickedItem.Genre == MenuGenre.History) {
                QTUtility.ClosedTabHistoryList.Remove(clickedItem.Path);
                UndoClosedItemsList.Remove(clickedItem);
                try {
                    using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar")) {
                        QTUtility.SaveRecentlyClosed(key);
                    }
                    goto Label_019F;
                }
                catch {
                    goto Label_019F;
                }
            }
            if(clickedItem.Genre == MenuGenre.RecentFile) {
                QTUtility.ExecutedPathsList.Remove(clickedItem.Path);
                RecentFileItemsList.Remove(clickedItem);
                try {
                    using(RegistryKey key2 = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar")) {
                        QTUtility.SaveRecentFiles(key2);
                    }
                }
                catch {
                }
            }
        Label_019F:
            clickedItem.Dispose();
        }

        public void GetCompositionState(out bool pfCompositionEnabled) {
            pfCompositionEnabled = true;
        }

        // TODO: figure out what this is
        private bool GetTargetWindow(IDLWrapper idlw, bool fNeedWait, out IntPtr hwndTabBar, out bool fOpened) {
            hwndTabBar = IntPtr.Zero;
            fOpened = false;
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar")) {
                if(key != null) {
                    hwndTabBar = QTUtility2.ReadRegHandle("Handle", key);
                }
                if((hwndTabBar == IntPtr.Zero) || !PInvoke.IsWindow(hwndTabBar)) {                              // what?! --.
                    hwndTabBar = QTUtility.instanceManager.CurrentHandle;                                       //          v
                    if((idlw.Available && ((hwndTabBar == IntPtr.Zero) || !PInvoke.IsWindow(hwndTabBar))) && ShellBrowser.Navigate(idlw)  == 0) {
                        fOpened = true;
                        if(fNeedWait) {
                            int num = 0;
                            while(!PInvoke.IsWindow(hwndTabBar)) {
                                Thread.Sleep(100);
                                hwndTabBar = QTUtility2.ReadRegHandle("Handle", key);
                                if(++num > 50) {
                                    goto Label_00EB;
                                }
                            }
                        }
                        else {
                            return true;
                        }
                    }
                }
            }
        Label_00EB:
            if(hwndTabBar != IntPtr.Zero) {
                return PInvoke.IsWindow(hwndTabBar);
            }
            return false;
        }

        private void groupsMenuItem_ReorderFinished(object sender, ToolStripItemClickedEventArgs e) {
            QTUtility.RefreshGroupMenuesOnReorderFinished(groupsMenuItem.DropDownItems);
        }

        private bool HandleKEYDOWN(IntPtr wParam, bool fRepeat) {
            int key = ((int)wParam) | ((int)ModifierKeys);
            if(((int)wParam) == 0x10) {
                if(!fRepeat) {
                    if(!QTUtility.CheckConfig(Settings.PreviewsWithShift)) {
                        listView.HideThumbnailTooltip();
                    }
                    if(!QTUtility.CheckConfig(Settings.NoShowSubDirTips) && !QTUtility.CheckConfig(Settings.SubDirTipsWithShift) && !listView.SubDirTipMenuIsShowing()) {
                        listView.HideSubDirTip();
                    }
                }
                return false;
            }
            if(key == 0x71) {
                if(!QTUtility.CheckConfig(Settings.F2Selection)) {
                    // TODO
                    // QTTabBarClass.HandleF2(this.hwndListView);
                }
                return false;
            }
            key |= 0x100000;
            if(((key == QTUtility.ShortcutKeys[0x1b]) || (key == QTUtility.ShortcutKeys[0x1c])) || (((key == QTUtility.ShortcutKeys[0x1d]) || (key == QTUtility.ShortcutKeys[30])) || (key == QTUtility.ShortcutKeys[0x1f]))) {
                if(!fRepeat) {
                    if(listView.SubDirTipMenuIsShowing()) {
                        return false;
                    }
                    int index = 0;
                    if(key == QTUtility.ShortcutKeys[0x1c]) {
                        index = 1;
                    }
                    else if(key == QTUtility.ShortcutKeys[0x1d]) {
                        index = 2;
                    }
                    else if(key == QTUtility.ShortcutKeys[30]) {
                        index = 3;
                    }
                    else if(key == QTUtility.ShortcutKeys[0x1f]) {
                        index = 4;
                    }
                    DoFileTools(index);
                }
                return true;
            }
            if(key == QTUtility.ShortcutKeys[0x26]) {
                if(!QTUtility.CheckConfig(Settings.NoShowSubDirTips)) {
                    if(!fRepeat) {
                        DoFileTools(5);
                    }
                    return true;
                }
            }
            else {
                if(((ConfigValues[2] & 8) == 0) && QTUtility.dicUserAppShortcutKeys.ContainsKey(key)) {
                    if(fRepeat) {
                        goto Label_02D8;
                    }
                    MenuItemArguments mia = QTUtility.dicUserAppShortcutKeys[key];
                    try {
                        Address[] list2 = (from wrapper in ShellBrowser.GetItems(true)
                                where wrapper.Available && wrapper.HasPath && wrapper.IsFileSystem
                                select wrapper.ToAddress()).ToArray();
                        if(list2.Length == 0) return false;
                        AppLauncher launcher = new AppLauncher(list2, Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                        launcher.ReplaceTokens_WorkingDir(mia);
                        launcher.ReplaceTokens_Arguments(mia);
                        AppLauncher.Execute(mia, IntPtr.Zero);
                        return true;
                    }
                    catch(Exception exception) {
                        QTUtility2.MakeErrorLog(exception, null);
                    }
                    finally {
                        mia.RestoreOriginalArgs();
                    }
                }
                if(!fRepeat && QTUtility.dicGroupShortcutKeys.ContainsKey(key)) {
                    Thread thread = new Thread(OpenGroup);
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.IsBackground = true;
                    thread.Start(new object[] { QTUtility.dicGroupShortcutKeys[key], Keys.None });
                    return true;
                }
            }
        Label_02D8:
            return false;
        }

        private bool HandleTabFolderActions(int index, Keys modKey, bool fEnqExec) {
            if(QTUtility.CheckConfig(Settings.MidClickNewWindow)) {
                if(modKey == Keys.Control) {
                    modKey = Keys.None;
                }
                else if(modKey == Keys.None) {
                    modKey = Keys.Control;
                }
            }
            if(!QTUtility.CheckConfig(Settings.ActivateNewTab)) {
                if((modKey & Keys.Shift) == Keys.Shift) {
                    modKey &= ~Keys.Shift;
                }
                else {
                    modKey |= Keys.Shift;
                }
            }
            
            byte[] idl = null;
            List<byte[]> folderIDLs = new List<byte[]>();
            List<string> execPaths = new List<string>();
            if(index != -1) {
                using(IDLWrapper wrapper = ShellBrowser.GetItem(index)) {
                    IDLWrapper wrapper2;
                    if(wrapper.Available && IDLIsFolder(wrapper, out wrapper2)) {
                        if(wrapper2 != null) {
                            idl = wrapper2.IDL;
                            wrapper2.Dispose();
                        }
                        else {
                            idl = wrapper.IDL;
                        }
                    }
                }
            }
            else {
                foreach(IDLWrapper wrapper in ShellBrowser.GetItems(true)) {
                    IDLWrapper wrapper2;
                    if(IDLIsFolder(wrapper, out wrapper2)) {
                        if(wrapper2 != null) {
                            folderIDLs.Add(wrapper2.IDL);
                            wrapper2.Dispose();
                        }
                        else {
                            folderIDLs.Add(wrapper.IDL);
                        }
                    }
                    else if(fEnqExec && wrapper.IsFileSystemFile) {
                        execPaths.Add(wrapper.Path);
                    }
                }
                if(folderIDLs.Count > 0) {
                    idl = folderIDLs[0];
                    folderIDLs.RemoveAt(0);
                }
                else {
                    if(fEnqExec) {
                        foreach(string str in execPaths) {
                            QTUtility.ExecutedPathsList.Add(str);
                        }
                    }
                }
            }
            
            if(idl == null) return false;

            if(folderIDLs.Count == 0) {
                if(index == -1) {
                    using(IDLWrapper wrapper5 = new IDLWrapper(idl)) {
                        if(wrapper5.IsFileSystemFile) {
                            return false;
                        }
                    }
                }
                OpenTab(new object[] {null, modKey, idl});
            }
            else {
                Thread thread = new Thread(OpenFolders2);
                thread.SetApartmentState(ApartmentState.STA);
                thread.IsBackground = true;
                thread.Start(new object[] {idl, folderIDLs, modKey});
            }
            return true;
        }

        private static bool IDLIsFolder(IDLWrapper idlw, out IDLWrapper idlwLinkTarget) {
            idlwLinkTarget = null;
            if(!idlw.IsLink) {
                return idlw.IsFolder;
            }
            if(idlw.HasPath) {
                idlwLinkTarget = new IDLWrapper(ShellMethods.GetLinkTargetPath(idlw.Path));
                if(idlwLinkTarget.Available && idlwLinkTarget.IsFolder) {
                    return true;
                }
                idlwLinkTarget.Dispose();
                idlwLinkTarget = null;
            }
            return false;
        }

        private void InitializeComponent() {
            bool flag = (ConfigValues[1] & 2) == 0;
            components = new Container();
            contextMenu = new DropDownMenuReorderable(components, true, false);
            contextMenuForSetting = new ContextMenuStripEx(components, true);
            labelGroupTitle = new TitleMenuItem(MenuGenre.Group, true);
            labelHistoryTitle = new TitleMenuItem(MenuGenre.History, true);
            labelUserAppTitle = new TitleMenuItem(MenuGenre.Application, true);
            labelRecentFileTitle = new TitleMenuItem(MenuGenre.RecentFile, true);
            contextMenu.SuspendLayout();
            contextMenuForSetting.SuspendLayout();
            SuspendLayout();
            contextMenu.ProhibitedKey.Add("historyItem");
            contextMenu.ProhibitedKey.Add("recentItem");
            contextMenu.ReorderEnabled = flag;
            contextMenu.MessageParent = Handle;
            contextMenu.ImageList = QTUtility.ImageListGlobal;
            contextMenu.ItemClicked += contextMenu_ItemClicked;
            contextMenu.Closing += contextMenu_Closing;
            contextMenu.ReorderFinished += contextMenu_ReorderFinished;
            contextMenu.ItemRightClicked += dropDownMenues_ItemRightClicked;
            ddmrGroups = new DropDownMenuReorderable(components, true, false);
            ddmrGroups.ReorderEnabled = flag;
            ddmrGroups.ReorderFinished += groupsMenuItem_ReorderFinished;
            ddmrGroups.ItemRightClicked += dropDownMenues_ItemRightClicked;
            groupsMenuItem = new TitleMenuItem(MenuGenre.Group, false);
            groupsMenuItem.DropDown = ddmrGroups;
            groupsMenuItem.DropDown.ImageList = QTUtility.ImageListGlobal;
            groupsMenuItem.DropDownItemClicked += contextMenu_ItemClicked;
            ddmrHistory = new DropDownMenuReorderable(components, true, false);
            ddmrHistory.ReorderEnabled = false;
            ddmrHistory.MessageParent = Handle;
            ddmrHistory.ItemRightClicked += dropDownMenues_ItemRightClicked;
            historyMenuItem = new TitleMenuItem(MenuGenre.History, false);
            historyMenuItem.DropDown = ddmrHistory;
            historyMenuItem.DropDown.ImageList = QTUtility.ImageListGlobal;
            historyMenuItem.DropDownItemClicked += contextMenu_ItemClicked;
            ddmrUserapps = new DropDownMenuReorderable(components);
            ddmrUserapps.ReorderEnabled = flag;
            ddmrUserapps.MessageParent = Handle;
            ddmrUserapps.ReorderFinished += userAppsMenuItem_ReorderFinished;
            ddmrUserapps.ItemRightClicked += dropDownMenues_ItemRightClicked;
            userAppsMenuItem = new TitleMenuItem(MenuGenre.Application, false);
            userAppsMenuItem.DropDown = ddmrUserapps;
            userAppsMenuItem.DropDown.ImageList = QTUtility.ImageListGlobal;
            userAppsMenuItem.DropDownItemClicked += contextMenu_ItemClicked;
            ddmrRecentFile = new DropDownMenuReorderable(components, false, false, false);
            ddmrRecentFile.MessageParent = Handle;
            ddmrRecentFile.ItemRightClicked += dropDownMenues_ItemRightClicked;
            recentFileMenuItem = new TitleMenuItem(MenuGenre.RecentFile, false);
            recentFileMenuItem.DropDown = ddmrRecentFile;
            recentFileMenuItem.DropDown.ImageList = QTUtility.ImageListGlobal;
            recentFileMenuItem.DropDownItemClicked += contextMenu_ItemClicked;
            contextMenuForSetting.ShowImageMargin = false;
            tsmiTaskBar = new ToolStripMenuItem();
            tsmiDesktop = new ToolStripMenuItem();
            tsmiLockItems = new ToolStripMenuItem();
            tsmiVSTitle = new ToolStripMenuItem();
            tsmiTaskBar.Checked = (ConfigValues[1] & 8) == 0;
            tsmiDesktop.Checked = (ConfigValues[1] & 4) == 0;
            tsmiLockItems.Checked = (ConfigValues[1] & 2) == 2;
            tsmiVSTitle.Checked = (ConfigValues[1] & 1) == 0;
            tsmiOnGroup = new ToolStripMenuItem();
            tsmiOnHistory = new ToolStripMenuItem();
            tsmiOnUserApps = new ToolStripMenuItem();
            tsmiOnRecentFile = new ToolStripMenuItem();
            tsmiOneClick = new ToolStripMenuItem();
            tsmiAppKeys = new ToolStripMenuItem();
            tsmiOnGroup.Checked = (ConfigValues[2] & 0x80) == 0;
            tsmiOnHistory.Checked = (ConfigValues[2] & 0x40) == 0;
            tsmiOnUserApps.Checked = (ConfigValues[2] & 0x20) == 0;
            tsmiOnRecentFile.Checked = (ConfigValues[2] & 1) == 0;
            tsmiOneClick.Checked = (ConfigValues[2] & 0x10) != 0;
            tsmiAppKeys.Checked = (ConfigValues[2] & 8) == 0;
            contextMenuForSetting.Items.AddRange(new ToolStripItem[] { tsmiTaskBar, tsmiDesktop, new ToolStripSeparator(), tsmiOnGroup, tsmiOnHistory, tsmiOnUserApps, tsmiOnRecentFile, new ToolStripSeparator(), tsmiLockItems, tsmiVSTitle, tsmiOneClick, tsmiAppKeys });
            contextMenuForSetting.ItemClicked += contextMenuForSetting_ItemClicked;
            contextMenuForSetting.Opening += contextMenuForSetting_Opening;
            ContextMenuStrip = contextMenuForSetting;
            Width = WidthOfBar;
            MinSize = new Size(30, 0x16);
            Dock = DockStyle.Fill;
            MouseClick += QTCoTaskBarClass_MouseClick;
            MouseDoubleClick += QTCoTaskBarClass_MouseDoubleClick;
            contextMenu.ResumeLayout(false);
            contextMenuForSetting.ResumeLayout(false);
            ResumeLayout(false);
        }

        private void InsertMenuItems_History() {
            if(((ConfigValues[2] & 0x40) == 0) && !QTUtility.CheckConfig(Settings.NoHistory)) {
                if(ExpandState[1]) {
                    int index = contextMenu.Items.IndexOf(labelHistoryTitle);
                    if(index != -1) {
                        foreach(ToolStripItem item in UndoClosedItemsList) {
                            contextMenu.Items.Insert(++index, item);
                        }
                    }
                }
                else {
                    ddmrHistory.AddItemsRange(UndoClosedItemsList.ToArray(), "historyItem");
                }
            }
        }

        private void InsertMenuItems_Recent() {
            if(((ConfigValues[2] & 1) == 0) && !QTUtility.CheckConfig(Settings.NoRecentFiles)) {
                if(ExpandState[3]) {
                    int index = contextMenu.Items.IndexOf(labelRecentFileTitle);
                    if(index != -1) {
                        foreach(ToolStripItem item in RecentFileItemsList) {
                            contextMenu.Items.Insert(++index, item);
                        }
                    }
                }
                else {
                    ddmrRecentFile.AddItemsRange(RecentFileItemsList.ToArray(), "recentItem");
                }
            }
        }

        // TODO
        private void InstallDesktopHook() {            
            uint num;
            IntPtr progmanHwnd = PInvoke.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Progman", null);
            IntPtr shellViewHwnd = PInvoke.FindWindowEx(progmanHwnd, IntPtr.Zero, "SHELLDLL_DefView", null);
            IntPtr desktopHwnd = PInvoke.FindWindowEx(shellViewHwnd, IntPtr.Zero, "SysListView32", null);
            if(timerHooks == null) {
                if(desktopHwnd == IntPtr.Zero) {
                    timerHooks = new Timer();
                    timerHooks.Tick += timerHooks_Tick;
                    timerHooks.Interval = 3000;
                    timerHooks.Start();
                    return;
                }
            }
            else {
                if(desktopHwnd == IntPtr.Zero) {
                    return;
                }
                timerHooks.Stop();
                timerHooks.Dispose();
                timerHooks = null;
            }
            hwndListView = desktopHwnd;
            hwndShellTray = WindowUtils.GetShellTrayWnd();
            hookProc_Msg_Desktop = new HookProc(CallbackGetMsgProc_Desktop);
            hookProc_Msg_ShellTrayWnd = new HookProc(CallbackGetMsgProc_ShellTrayWnd);
            hookProc_Keys_Desktop = new HookProc(CallbackKeyProc_Desktop);
            int windowThreadProcessId = PInvoke.GetWindowThreadProcessId(hwndListView, out num);
            int dwThreadId = PInvoke.GetWindowThreadProcessId(hwndShellTray, out num);
            hHook_MsgDesktop = PInvoke.SetWindowsHookEx(3, hookProc_Msg_Desktop, IntPtr.Zero, windowThreadProcessId);
            hHook_MsgShell_TrayWnd = PInvoke.SetWindowsHookEx(3, hookProc_Msg_ShellTrayWnd, IntPtr.Zero, dwThreadId);
            hHook_KeyDesktop = PInvoke.SetWindowsHookEx(2, hookProc_Keys_Desktop, IntPtr.Zero, windowThreadProcessId); 
            PInvoke.PostMessage(hwndListView, WM.APP + 100, IntPtr.Zero, IntPtr.Zero);
        }

        private bool ListView_LostFocus() {
            listView.HideThumbnailTooltip();
            listView.HideSubDirTip_ExplorerInactivated();
            return false;
        }

        // TODO
        private bool ListView_MouseActivate(ref int result) {
            return false;
        }

        private bool ListView_ItemActivated(Keys modKeys) {
            bool fEnqExec = !QTUtility.CheckConfig(Settings.NoRecentFiles);
            return HandleTabFolderActions(-1, modKeys, fEnqExec);
        }

        private bool ListView_MiddleClick(Point pt) {
            if(!QTUtility.CheckConfig(Settings.NoCaptureMidClick)) {
                int index = listView.HitTest(pt, false);
                if(index != -1) {
                    HandleTabFolderActions(index, ModifierKeys, false);
                } 
            }
            return false;
        }

        private bool ListView_DoubleClick(Point pt) {
            if((ConfigValues[1] & 4) == 0 && listView.PointIsBackground(pt, false)) {
                QTUtility2.SendCOPYDATASTRUCT(ThisHandle, (IntPtr)0xff, "fromdesktop", QTUtility2.Make_LPARAM(pt.X, pt.Y));
            }
            return false;
        }

        protected override void OnCreateControl() {
            base.OnCreateControl();
            ThisHandle = Handle;
            InstallDesktopHook();
        }

        private void OnDesktopDblClicked(Point popUpPoint) {
            if(++iMainMenuShownCount > 0x40) {
                ClearThumbnailCache();
            }
            contextMenu.SuspendLayout();
            ddmrGroups.SuspendLayout();
            ddmrHistory.SuspendLayout();
            ddmrUserapps.SuspendLayout();
            ddmrRecentFile.SuspendLayout();
            if(fRequiredRefresh_MenuItems) {
                ClearMenuItems();
            }
            RecreateUndoClosedItems();
            if(contextMenu.Items.Count == 0) {
                string[] strArray = QTUtility.TextResourcesDic["TaskBar_Titles"];
                labelGroupTitle.Text = groupsMenuItem.Text = strArray[0];
                historyMenuItem.Text = labelHistoryTitle.Text = strArray[1];
                userAppsMenuItem.Text = labelUserAppTitle.Text = strArray[2];
                recentFileMenuItem.Text = labelRecentFileTitle.Text = strArray[3];
                if((ConfigValues[2] & 0x80) == 0) {
                    QTUtility.RefreshGroupsDic();
                    foreach(string str in QTUtility.GroupPathsDic.Keys) {
                        string str2 = QTUtility.GroupPathsDic[str];
                        if(str2.Length == 0) {
                            ToolStripSeparator separator = new ToolStripSeparator();
                            separator.Name = "groupSep";
                            GroupItemsList.Add(separator);
                            continue;
                        }
                        string path = str2.Split(QTUtility.SEPARATOR_CHAR)[0];
                        QMenuItem item = new QMenuItem(str, MenuGenre.Group);
                        item.SetImageReservationKey(path, null);
                        if(QTUtility.StartUpGroupList.Contains(str)) {
                            if(QTUtility.StartUpTabFont == null) {
                                QTUtility.StartUpTabFont = new Font(item.Font, FontStyle.Underline);
                            }
                            item.Font = QTUtility.StartUpTabFont;
                        }
                        GroupItemsList.Add(item);
                    }
                }
                if((ConfigValues[2] & 0x20) == 0) {
                    UserappItemsList = MenuUtility.CreateAppLauncherItems(Handle, (ConfigValues[1] & 2) == 0, dropDownMenues_ItemRightClicked, subMenuItems_DoubleClick, true);
                }
                bool flag = false;
                bool flag2 = false;
                bool flag3 = false;
                bool flag4 = false;
                for(int i = 0; i < 3; i++) {
                    switch(Order_Root[i]) {
                        case 0:
                            if(!flag) {
                                AddMenuItems_Group();
                                flag = true;
                            }
                            break;

                        case 1:
                            if(!flag2) {
                                AddMenuItems_History();
                                flag2 = true;
                            }
                            break;

                        case 2:
                            if(!flag3) {
                                AddMenuItems_UserApp();
                                flag3 = true;
                            }
                            break;

                        case 3:
                            if(!flag4) {
                                AddMenuItems_Recent();
                                flag4 = true;
                            }
                            break;
                    }
                }
                if(!flag) {
                    AddMenuItems_Group();
                }
                if(!flag2) {
                    AddMenuItems_History();
                }
                if(!flag3) {
                    AddMenuItems_UserApp();
                }
                if(!flag4) {
                    AddMenuItems_Recent();
                }
            }
            else {
                InsertMenuItems_History();
                InsertMenuItems_Recent();
            }
            ddmrUserapps.ResumeLayout();
            ddmrHistory.ResumeLayout();
            ddmrGroups.ResumeLayout();
            ddmrRecentFile.ResumeLayout();
            contextMenu.ResumeLayout();
            if(contextMenu.Items.Count > 0) {
                if(QTUtility.IsVista) {
                    contextMenu.SendToBack();
                }
                contextMenu.Show(popUpPoint);
            }
        }

        protected override void OnHandleCreated(EventArgs e) {
            base.OnHandleCreated(e);
            if(IsHandleCreated) {
                using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar")) {
                    QTUtility2.WriteRegHandle("TaskBarHandle", key, Handle);
                }
            }
        }

        private void OnLabelTitleClickedToClose(MenuGenre genre) {
            ToolStripMenuItem labelGroupTitle;
            List<ToolStripItem> groupItemsList;
            ToolStripMenuItem groupsMenuItem;
            string str;
            int index = 0;
            switch(genre) {
                case MenuGenre.History:
                    index = 1;
                    break;

                case MenuGenre.Group:
                    index = 0;
                    break;

                case MenuGenre.Application:
                    index = 2;
                    break;

                case MenuGenre.RecentFile:
                    index = 3;
                    break;
            }
            ExpandState[index] = !ExpandState[index];
            CancelClosing = true;
            switch(index) {
                case 0:
                    labelGroupTitle = this.labelGroupTitle;
                    groupItemsList = GroupItemsList;
                    groupsMenuItem = this.groupsMenuItem;
                    str = "groupItem";
                    break;

                case 1:
                    labelGroupTitle = labelHistoryTitle;
                    groupItemsList = UndoClosedItemsList;
                    groupsMenuItem = historyMenuItem;
                    str = "historyItem";
                    break;

                case 2:
                    labelGroupTitle = labelUserAppTitle;
                    groupItemsList = UserappItemsList;
                    groupsMenuItem = userAppsMenuItem;
                    str = "userappItem";
                    break;

                default:
                    labelGroupTitle = labelRecentFileTitle;
                    groupItemsList = RecentFileItemsList;
                    groupsMenuItem = recentFileMenuItem;
                    str = "recentItem";
                    break;
            }
            int num2 = contextMenu.Items.IndexOf(labelGroupTitle);
            contextMenu.SuspendLayout();
            contextMenu.Items.Remove(labelGroupTitle);
            foreach(ToolStripItem item3 in groupItemsList) {
                contextMenu.Items.Remove(item3);
            }
            contextMenu.InsertItem(num2, groupsMenuItem, "submenu");
            ((DropDownMenuReorderable)groupsMenuItem.DropDown).AddItemsRange(groupItemsList.ToArray(), str);
            contextMenu.ResumeLayout();
        }

        protected override void OnMouseEnter(EventArgs e) {
            NowMouseHovering = true;
            base.OnMouseEnter(e);
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e) {
            NowMouseHovering = false;
            base.OnMouseLeave(e);
            Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e) {
            if(VisualStyleRenderer.IsSupported) {
                if(bgRenderer == null) {
                    bgRenderer = new VisualStyleRenderer(VisualStyleElement.Taskbar.BackgroundTop.Normal);
                }
                bgRenderer.DrawParentBackground(e.Graphics, e.ClipRectangle, this);
            }
            else {
                base.OnPaintBackground(e);
            }
            if(NowMouseHovering) {
                Color baseColor = VisualStyleRenderer.IsSupported ? SystemColors.Window : SystemColors.WindowText;
                if(stringFormat == null) {
                    stringFormat = new StringFormat();
                    stringFormat.Alignment = StringAlignment.Center;
                    stringFormat.LineAlignment = StringAlignment.Center;
                    stringFormat.FormatFlags = StringFormatFlags.NoWrap;
                }
                using(SolidBrush brush = new SolidBrush(Color.FromArgb(0x80, baseColor))) {
                    e.Graphics.DrawString("QTTab Desktop Tool", Font, brush, new Rectangle(0, 5, e.ClipRectangle.Width - 1, e.ClipRectangle.Height - 6), stringFormat);
                }
                using(Pen pen = new Pen(Color.FromArgb(0x80, baseColor))) {
                    e.Graphics.DrawRectangle(pen, new Rectangle(0, 2, e.ClipRectangle.Width - 1, e.ClipRectangle.Height - 3));
                }
            }
        }

        private void OnSubMenuTitleClickedToOpen(MenuGenre genre) {
            ToolStripMenuItem groupsMenuItem;
            ToolStripMenuItem labelGroupTitle;
            List<ToolStripItem> groupItemsList;
            string str;
            int index = 0;
            switch(genre) {
                case MenuGenre.History:
                    index = 1;
                    break;

                case MenuGenre.Group:
                    index = 0;
                    break;

                case MenuGenre.Application:
                    index = 2;
                    break;

                case MenuGenre.RecentFile:
                    index = 3;
                    break;
            }
            ExpandState[index] = !ExpandState[index];
            CancelClosing = true;
            switch(index) {
                case 0:
                    groupsMenuItem = this.groupsMenuItem;
                    labelGroupTitle = this.labelGroupTitle;
                    groupItemsList = GroupItemsList;
                    str = "groupItem";
                    break;

                case 1:
                    groupsMenuItem = historyMenuItem;
                    labelGroupTitle = labelHistoryTitle;
                    groupItemsList = UndoClosedItemsList;
                    str = "historyItem";
                    break;

                case 2:
                    groupsMenuItem = userAppsMenuItem;
                    labelGroupTitle = labelUserAppTitle;
                    groupItemsList = UserappItemsList;
                    str = "userappItem";
                    break;

                default:
                    groupsMenuItem = recentFileMenuItem;
                    labelGroupTitle = labelRecentFileTitle;
                    groupItemsList = RecentFileItemsList;
                    str = "recentItem";
                    break;
            }
            groupsMenuItem.DropDown.Hide();
            contextMenu.SuspendLayout();
            int num2 = contextMenu.Items.IndexOf(groupsMenuItem);
            contextMenu.Items.Remove(groupsMenuItem);
            contextMenu.InsertItem(num2, labelGroupTitle, "label" + index);
            foreach(ToolStripItem item3 in groupItemsList) {
                contextMenu.InsertItem(++num2, item3, str);
            }
            contextMenu.ResumeLayout();
        }

        private void OpenFolders2(object obj) {
            object[] objArray = (object[])obj;
            byte[] idl = (byte[])objArray[0];
            List<byte[]> list = (List<byte[]>)objArray[1];
            int num = (int)objArray[2];
            if(list.Count != 0) {
                QTUtility2.InitializeTemporaryPaths();
                QTUtility.TMPTargetIDL = idl;
                QTUtility.TMPIDLList = list;
                using(IDLWrapper wrapper = new IDLWrapper(idl)) {
                    IntPtr ptr;
                    bool flag;
                    if(GetTargetWindow(wrapper, true, out ptr, out flag) && !flag) {
                        QTUtility2.SendCOPYDATASTRUCT(ptr, (IntPtr)9, null, (IntPtr)num);
                    }
                }
            }
        }

        private void OpenGroup(object obj) {
            string str2;
            object[] objArray = (object[])obj;
            string key = (string)objArray[0];
            Keys keys = (Keys)objArray[1];
            bool flag = keys == Keys.Control;
            if(QTUtility.GroupPathsDic.TryGetValue(key, out str2) && (str2.Length > 0)) {
                if(QTUtility.StartUpGroupList.Contains(key)) {
                    QTUtility.StartUpGroupNameNowOpening = key;
                }
                string path = str2.Split(QTUtility.SEPARATOR_CHAR)[0];
                using(IDLWrapper wrapper = new IDLWrapper(path, true)) {
                    IntPtr ptr;
                    bool flag2;
                    if(GetTargetWindow(wrapper, true, out ptr, out flag2)) {
                        if(flag2) {
                            QTUtility2.SendCOPYDATASTRUCT(ptr, (IntPtr)2, key, IntPtr.Zero);
                        }
                        else {
                            IntPtr wParam = flag ? ((IntPtr)3) : IntPtr.Zero;
                            if((wParam == IntPtr.Zero) && QTUtility2.TargetIsInNoCapture(IntPtr.Zero, path)) {
                                wParam = (IntPtr)3;
                            }
                            QTUtility2.SendCOPYDATASTRUCT(ptr, wParam, key, IntPtr.Zero);
                        }
                    }
                }
            }
        }

        private void OpenTab(object obj) {
            IDLWrapper wrapper;
            object[] objArray = (object[])obj;
            string path = (string)objArray[0];
            Keys keys = (Keys)objArray[1];
            byte[] idl = null;
            if(objArray.Length == 3) {
                idl = (byte[])objArray[2];
            }
            if(idl != null) {
                wrapper = new IDLWrapper(idl);
            }
            else {
                wrapper = new IDLWrapper(path, true);
            }
            using(wrapper) {
                IntPtr ptr;
                bool flag;
                if(GetTargetWindow(wrapper, false, out ptr, out flag) && !flag) {
                    SendCOPYDATASTRUCT_IDL(ptr, (IntPtr)4, wrapper.IDL, (IntPtr)((long)keys));
                }
            }
        }

        private void QTCoTaskBarClass_MouseClick(object sender, MouseEventArgs e) {
            if((e.Button == MouseButtons.Left) && ((ConfigValues[2] & 0x10) != 0)) {
                OnDesktopDblClicked(MousePosition);
            }
        }

        private void QTCoTaskBarClass_MouseDoubleClick(object sender, MouseEventArgs e) {
            if((e.Button == MouseButtons.Left) && ((ConfigValues[2] & 0x10) == 0)) {
                OnDesktopDblClicked(MousePosition);
            }
        }

        private void ReadSetting() {
            using(RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Quizo\QTTabBar")) {
                if(key != null) {
                    byte[] defaultValue = new byte[4];
                    defaultValue[0] = 1;
                    byte[] buffer = (byte[])key.GetValue("Config_Desktop", defaultValue);
                    ConfigValues = buffer;
                    Order_Root[0] = (byte)(buffer[0] >> 4);
                    Order_Root[1] = (byte)(buffer[0] & 15);
                    Order_Root[2] = (byte)((buffer[3] >> 5) & 7);
                    ExpandState[0] = (buffer[1] & 0x80) == 0x80;
                    ExpandState[1] = (buffer[1] & 0x40) == 0x40;
                    ExpandState[2] = (buffer[1] & 0x20) == 0x20;
                    ExpandState[3] = (buffer[2] & 2) == 2;
                    WidthOfBar = (int)key.GetValue("TaskbarWidth", 80);
                }
            }
        }

        private void RecreateUndoClosedItems() {
            if(UndoClosedItemsList.Count > 0) {
                ddmrHistory.Items.Clear();
                foreach(ToolStripItem item in UndoClosedItemsList) {
                    item.Dispose();
                }
                UndoClosedItemsList.Clear();
            }
            if(RecentFileItemsList.Count > 0) {
                ddmrRecentFile.Items.Clear();
                foreach(ToolStripItem item2 in RecentFileItemsList) {
                    item2.Dispose();
                }
            }
            if(((ConfigValues[2] & 0x40) == 0) && !QTUtility.CheckConfig(Settings.NoHistory)) {
                UndoClosedItemsList = MenuUtility.CreateUndoClosedItems(null);
            }
            if(((ConfigValues[2] & 1) == 0) && !QTUtility.CheckConfig(Settings.NoRecentFiles)) {
                RecentFileItemsList = MenuUtility.CreateRecentFilesItems();
            }
        }

        [ComRegisterFunction]
        private static void Register(Type t) {
            string str = t.GUID.ToString("B");
            string str2 = (CultureInfo.CurrentCulture.Parent.Name == "ja") ? "QT Tab デスクトップ ツール" : "QT Tab Desktop Tool";
            using(RegistryKey key = Registry.ClassesRoot.CreateSubKey(@"CLSID\" + str)) {
                key.SetValue(null, str2);
                key.SetValue("MenuText", str2);
                key.SetValue("HelpText", str2);
                key.CreateSubKey(@"Implemented Categories\{00021492-0000-0000-C000-000000000046}");
            }
        }

        private void SaveSetting() {
            ConfigValues = new byte[4];
            ConfigValues[0] = (byte)((Order_Root[0] << 4) | Order_Root[1]);
            ConfigValues[1] = (byte)(((ExpandState[0] ? 0x80 : 0) | (ExpandState[1] ? 0x40 : 0)) | (ExpandState[2] ? 0x20 : 0));
            ConfigValues[1] = (byte)(ConfigValues[1] | (tsmiTaskBar.Checked ? (0) : (8)));
            ConfigValues[1] = (byte)(ConfigValues[1] | (tsmiDesktop.Checked ? (0) : (4)));
            ConfigValues[1] = (byte)(ConfigValues[1] | (tsmiLockItems.Checked ? (2) : (0)));
            ConfigValues[1] = (byte)(ConfigValues[1] | (tsmiVSTitle.Checked ? (0) : (1)));
            ConfigValues[2] = (byte)(ConfigValues[2] | (tsmiOnGroup.Checked ? (0) : (0x80)));
            ConfigValues[2] = (byte)(ConfigValues[2] | (tsmiOnHistory.Checked ? (0) : (0x40)));
            ConfigValues[2] = (byte)(ConfigValues[2] | (tsmiOnUserApps.Checked ? (0) : (0x20)));
            ConfigValues[2] = (byte)(ConfigValues[2] | (tsmiOnRecentFile.Checked ? (0) : (1)));
            ConfigValues[2] = (byte)(ConfigValues[2] | (tsmiOneClick.Checked ? (0x10) : (0)));
            ConfigValues[2] = (byte)(ConfigValues[2] | (tsmiAppKeys.Checked ? (0) : (8)));
            ConfigValues[2] = (byte)(ConfigValues[2] | (ExpandState[3] ? (2) : (0)));
            ConfigValues[3] = (byte)(ConfigValues[3] | ((byte)(Order_Root[2] << 5)));
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar")) {
                if(key != null) {
                    key.SetValue("Config_Desktop", ConfigValues);
                    key.SetValue("TaskBarWidth", Width);
                }
            }
        }

        private static IntPtr SendCOPYDATASTRUCT_IDL(IntPtr hWnd, IntPtr wParam, byte[] idl, IntPtr dwData) {
            if((idl == null) || (idl.Length == 0)) {
                return (IntPtr)(-1);
            }
            COPYDATASTRUCT structure = new COPYDATASTRUCT();
            int length = idl.Length;
            IntPtr destination = Marshal.AllocHGlobal(length);
            Marshal.Copy(idl, 0, destination, length);
            structure.lpData = destination;
            structure.cbData = length;
            structure.dwData = dwData;
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(structure));
            Marshal.StructureToPtr(structure, ptr, false);
            IntPtr ptr3 = PInvoke.SendMessage(hWnd, 0x4a, wParam, ptr);
            Marshal.FreeHGlobal(destination);
            Marshal.FreeHGlobal(ptr);
            return ptr3;
        }

        public void SetCompositionState(bool fCompositionEnabled) {
        }

        public override void SetSite(object pUnkSite) {
            if(BandObjectSite != null) {
                Marshal.ReleaseComObject(BandObjectSite);
            }
            BandObjectSite = (IInputObjectSite)pUnkSite;
            /*
            if(pUnkSite != null) {
                Thread.Sleep(10000);
                object obj2;
                _IServiceProvider bandObjectSite = (_IServiceProvider)base.BandObjectSite;
                Guid guid = ExplorerGUIDs.IID_IShellView;
                Guid riid = ExplorerGUIDs.IID_IUnknown;
                try {
                    bandObjectSite.QueryService(ref guid, ref riid, out obj2);
                    IShellBrowser _ShellBrowser = (IShellBrowser)obj2;
                    if(obj2 != null) {
                        Marshal.ReleaseComObject(_ShellBrowser);
                    }
                }
                catch(Exception e) {
                    Console.WriteLine(e);
                }
            }*/

            Application.EnableVisualStyles();
            if(QTUtility.NowDebugging) {
                CheckForIllegalCrossThreadCalls = true;
            }
            ReadSetting();
            InitializeComponent();
            TitleMenuItem.DrawBackground = tsmiVSTitle.Checked;
            Myself = this;
        }

        private void ListView_Destroyed(object sender, EventArgs e) {
            lock(this) {
                listView.Dispose();
                listView = null;
            }
        }

        private void subDirTip_MenuItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            QMenuItem clickedItem = (QMenuItem)e.ClickedItem;
            if(clickedItem.Target == MenuTarget.Folder) {
                if(clickedItem.IDLData == null) {
                    OpenTab(new object[] { clickedItem.TargetPath, ModifierKeys });
                    return;
                }
                using(IDLWrapper wrapper = new IDLWrapper(clickedItem.IDLData)) {
                    SHELLEXECUTEINFO structure = new SHELLEXECUTEINFO();
                    structure.cbSize = Marshal.SizeOf(structure);
                    structure.nShow = 1;
                    structure.fMask = 4;
                    structure.lpIDList = wrapper.PIDL;
                    try {
                        PInvoke.ShellExecuteEx(ref structure);
                    }
                    catch {
                    }
                    return;
                }
            }
            try {
                string path = clickedItem.Path;
                ProcessStartInfo startInfo = new ProcessStartInfo(path);
                startInfo.WorkingDirectory = Path.GetDirectoryName(path);
                startInfo.ErrorDialog = true;
                Process.Start(startInfo);
                if(!QTUtility.CheckConfig(Settings.NoRecentFiles)) {
                    QTUtility.ExecutedPathsList.Add(path);
                }
            }
            catch {
            }
        }

        private void subDirTip_MenuItemRightClicked(object sender, ItemRightClickedEventArgs e) {
            using(IDLWrapper wrapper = new IDLWrapper(((QMenuItem)e.ClickedItem).Path)) {
                e.HRESULT = ShellMethods.PopUpSystemContextMenu(wrapper, e.IsKey ? e.Point : MousePosition, ref iContextMenu2, ((SubDirTipForm)sender).Handle, false);
            }
        }

        private void subDirTip_MultipleMenuItemsClicked(object sender, EventArgs e) {
            List<string> executedDirectories = ((SubDirTipForm)sender).ExecutedDirectories;
            if(executedDirectories.Count > 0) {
                List<byte[]> list2 = new List<byte[]>();
                foreach(string str in executedDirectories) {
                    using(IDLWrapper wrapper = new IDLWrapper(str)) {
                        IDLWrapper wrapper2;
                        if(IDLIsFolder(wrapper, out wrapper2)) {
                            if(wrapper2 != null) {
                                list2.Add(wrapper2.IDL);
                                wrapper2.Dispose();
                            }
                            else {
                                list2.Add(wrapper.IDL);
                            }
                        }
                        continue;
                    }
                }
                Keys modifierKeys = ModifierKeys;
                if(!QTUtility.CheckConfig(Settings.ActivateNewTab)) {
                    switch(modifierKeys) {
                        case Keys.Shift:
                            modifierKeys = Keys.None;
                            break;

                        case Keys.None:
                            modifierKeys = Keys.Shift;
                            break;
                    }
                }
                if(list2.Count == 1) {
                    object[] objArray = new object[3];
                    objArray[1] = modifierKeys;
                    objArray[2] = list2[0];
                    OpenTab(objArray);
                }
                else if(list2.Count > 1) {
                    byte[] buffer = list2[0];
                    list2.RemoveAt(0);
                    Thread thread = new Thread(OpenFolders2);
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.IsBackground = true;
                    thread.Start(new object[] { buffer, list2, modifierKeys });
                }
            }
        }

        private void subDirTip_MultipleMenuItemsRightClicked(object sender, ItemRightClickedEventArgs e) {
            List<string> executedDirectories = ((SubDirTipForm)sender).ExecutedDirectories;
            e.HRESULT = ShellMethods.PopUpSystemContextMenu(executedDirectories, e.IsKey ? e.Point : MousePosition, ref iContextMenu2, ((SubDirTipForm)sender).Handle);
        }

        private static void subMenuItems_DoubleClick(object sender, EventArgs e) {
            string path = ((DirectoryMenuItem)sender).Path;
            if(Directory.Exists(path)) {
                try {
                    if(Myself != null) {
                        object obj2 = new object[] { path, ModifierKeys };
                        Myself.OpenTab(obj2);
                    }
                    else {
                        Process.Start(path);
                    }
                }
                catch {
                    MessageBox.Show("Operation failed.\r\nPlease make sure the folder exists or you have permission to access to:\r\n\r\n\t" + path, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
            }
        }

        private void timerHooks_Tick(object sender, EventArgs e) {
            if(++iHookTimeout > 5) {
                timerHooks.Stop();
                MessageBox.Show("Failed to hook Desktop. Please re-enable QT Tab Desktop tool.");
            }
            else {
                InstallDesktopHook();
            }
        }

        [ComUnregisterFunction]
        private static void Unregister(Type t) {
            string subkey = t.GUID.ToString("B");
            try {
                using(RegistryKey key = Registry.ClassesRoot.CreateSubKey("CLSID")) {
                    key.DeleteSubKeyTree(subkey);
                }
            }
            catch {
            }
        }

        private void userAppsMenuItem_ReorderFinished(object sender, ToolStripItemClickedEventArgs e) {
            QTUtility.RefreshUserappMenuesOnReorderFinished(userAppsMenuItem.DropDownItems);
            QTUtility.fRequiredRefresh_App = true;
        }

        protected override void WndProc(ref Message m) {
            switch(m.Msg) {
                case WM.COPYDATA: {
                    COPYDATASTRUCT copydatastruct = (COPYDATASTRUCT)Marshal.PtrToStructure(m.LParam, typeof(COPYDATASTRUCT));
                    switch(((int)m.WParam)) {
                        case 0: {
                            string[] strArray =
                                    Marshal.PtrToStringAuto(copydatastruct.lpData).Split(QTUtility.SEPARATOR_CHAR);
                            if((strArray.Length > 1) && (strArray[1].Length > 0)) {
                                QTUtility.PathToSelectInCommandLineArg = strArray[1];
                            }
                            QTUtility.fExplorerPrevented = true;
                            try {
                                BlockedExplorerURL = strArray[0];
                                BlockedExplorerProcess = Process.GetProcessById((int)copydatastruct.dwData);
                                BlockedExplorerProcess.EnableRaisingEvents = true;
                                BlockedExplorerProcess.Exited += BlockedExplorer_Exited;
                            }
                            catch(Exception exception) {
                                QTUtility2.MakeErrorLog(exception, null);
                            }
                            break;
                        }
                        case 3:
                            fRequiredRefresh_MenuItems = true;
                            break;

                        case 0xff: {
                            int x = QTUtility2.GET_X_LPARAM(copydatastruct.dwData);
                            int y = QTUtility2.GET_Y_LPARAM(copydatastruct.dwData);
                            PInvoke.SetForegroundWindow(hwndShellTray);
                            OnDesktopDblClicked(new Point(x, y));
                            break;
                        }
                    }
                    break;
                }

                case WM.APP + 100:
                    if(hHook_MsgDesktop != IntPtr.Zero) {
                        PInvoke.UnhookWindowsHookEx(hHook_MsgDesktop);
                        hHook_MsgDesktop = IntPtr.Zero;
                    }
                    break;

                case WM.MEASUREITEM:
                case WM.DRAWITEM:
                case WM.INITMENUPOPUP: {
                    uint num3;
                    uint num4;
                    if(fContextmenuedOnMain_Grp) {
                        base.WndProc(ref m);
                        return;
                    }
                    if((iContextMenu2 != null) && (m.HWnd == Handle)) {
                        try {
                            iContextMenu2.HandleMenuMsg(m.Msg, m.WParam, m.LParam);
                        }
                        catch {
                        }
                        return;
                    }
                    int windowThreadProcessId = PInvoke.GetWindowThreadProcessId(m.WParam, out num3);
                    int num6 = PInvoke.GetWindowThreadProcessId(Handle, out num4);
                    if(windowThreadProcessId != num6) {
                        return;
                    }
                    break;
                }
                    
                case WM.MOUSEACTIVATE:
                    if((ConfigValues[2] & 0x10) != 0 &&
                            ((((int)((long)m.LParam)) >> 0x10) & 0xffff) == 0x201 && contextMenu.Visible) {
                        contextMenu.Close(ToolStripDropDownCloseReason.AppClicked);
                        m.Result = (IntPtr)4;
                        return;
                    }
                    break;
            }
            base.WndProc(ref m);
        }

        private sealed class TitleMenuItem : ToolStripMenuItem {
            private Bitmap bmpArrow_Cls;
            private Bitmap bmpArrow_Opn;
            private static Bitmap bmpTitle;
            private static bool drawBackground;
            private bool fOpened;
            private MenuGenre genre;
            private static StringFormat sf;

            public TitleMenuItem(MenuGenre genre, bool fOpened) {
                this.genre = genre;
                this.fOpened = fOpened;
                bmpArrow_Opn = Resources_Image.menuOpen;
                bmpArrow_Cls = Resources_Image.menuClose;
                if(sf == null) {
                    Init();
                }
            }

            protected override void Dispose(bool disposing) {
                if(bmpArrow_Opn != null) {
                    bmpArrow_Opn.Dispose();
                    bmpArrow_Opn = null;
                }
                if(bmpArrow_Cls != null) {
                    bmpArrow_Cls.Dispose();
                    bmpArrow_Cls = null;
                }
                base.Dispose(disposing);
            }

            private static void Init() {
                sf = new StringFormat();
                sf.Alignment = StringAlignment.Near;
                sf.LineAlignment = StringAlignment.Center;
                bmpTitle = Resources_Image.TitleBar;
            }

            protected override void OnPaint(PaintEventArgs e) {
                if(drawBackground) {
                    Rectangle rect = new Rectangle(1, 0, Bounds.Width, Bounds.Height);
                    e.Graphics.DrawImage(bmpTitle, new Rectangle(new Point(1, 0), new Size(1, Bounds.Height)), new Rectangle(Point.Empty, new Size(1, 0x18)), GraphicsUnit.Pixel);
                    e.Graphics.DrawImage(bmpTitle, new Rectangle(new Point(2, 0), new Size(Bounds.Width - 3, 1)), new Rectangle(new Point(1, 0), new Size(0x62, 1)), GraphicsUnit.Pixel);
                    e.Graphics.DrawImage(bmpTitle, new Rectangle(new Point(Bounds.Width - 1, 0), new Size(1, Bounds.Height)), new Rectangle(new Point(0x63, 0), new Size(1, 0x18)), GraphicsUnit.Pixel);
                    e.Graphics.DrawImage(bmpTitle, new Rectangle(new Point(2, Bounds.Height - 1), new Size(Bounds.Width - 3, 1)), new Rectangle(new Point(1, 0x17), new Size(0x62, 1)), GraphicsUnit.Pixel);
                    e.Graphics.DrawImage(bmpTitle, new Rectangle(new Point(2, 1), new Size(Bounds.Width - 3, Bounds.Height - 2)), new Rectangle(new Point(1, 1), new Size(0x62, 0x16)), GraphicsUnit.Pixel);
                    if(Selected) {
                        SolidBrush brush = new SolidBrush(Color.FromArgb(0x60, SystemColors.Highlight));
                        e.Graphics.FillRectangle(brush, rect);
                        brush.Dispose();
                    }
                    if(HasDropDownItems) {
                        int y = (rect.Height - 0x10) / 2;
                        if(y < 0) {
                            y = 5;
                        }
                        else {
                            y += 5;
                        }
                        using(SolidBrush brush2 = new SolidBrush(Color.FromArgb(Selected ? 0xff : 0x80, Color.White))) {
                            Point point = new Point(rect.Width - 15, y);
                            Point[] points = new Point[] { point, new Point(point.X, point.Y + 8), new Point(point.X + 4, point.Y + 4) };
                            e.Graphics.FillPolygon(brush2, points);
                        }
                    }
                    e.Graphics.DrawString(Text, Font, Brushes.White, new RectangleF(34f, 2f, (rect.Width - 0x22), (rect.Height - 2)), sf);
                }
                else {
                    base.OnPaint(e);
                }
                e.Graphics.DrawImage(fOpened ? bmpArrow_Cls : bmpArrow_Opn, new Rectangle(5, 4, 0x10, 0x10));
            }

            public static bool DrawBackground {
                set {
                    drawBackground = value;
                }
            }

            public MenuGenre Genre {
                get {
                    return genre;
                }
            }

            public bool IsOpened {
                get {
                    return fOpened;
                }
            }
        }
    }
}
