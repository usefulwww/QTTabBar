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

namespace QTTabBarLib {
    using BandObjectLib;
    using Microsoft.Win32;
    using QTPlugin;
    using QTTabBarLib.Interop;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Media;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;

    [Guid("D2BF470E-ED1C-487F-A555-2BD8835EB6CE"), ComVisible(true)]
    public sealed class QTCoTaskBarClass : BandObject, BandObjectLib.IPersistStream, IDeskBand2 {
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
        private IFolderView folderView;
        private bool fRequiredRefresh_MenuItems;
        private bool fThumbnailPending;
        private List<ToolStripItem> GroupItemsList = new List<ToolStripItem>();
        private TitleMenuItem groupsMenuItem;
        private FileHashComputerForm hashForm;
        private IntPtr hHook_KeyDesktop;
        private IntPtr hHook_MsgDesktop;
        private IntPtr hHook_MsgShell_TrayWnd;
        private TitleMenuItem historyMenuItem;
        private QTTabBarLib.Interop.HookProc hookProc_Keys_Desktop;
        private QTTabBarLib.Interop.HookProc hookProc_Msg_Desktop;
        private QTTabBarLib.Interop.HookProc hookProc_Msg_ShellTrayWnd;
        private IntPtr hwndListView;
        private IntPtr hwndShellTray;
        private static IContextMenu2 iContextMenu2;
        private int iHookTimeout;
        private int iMainMenuShownCount;
        private int itemIndexDROPHILITED = -1;
        private TitleMenuItem labelGroupTitle;
        private TitleMenuItem labelHistoryTitle;
        private TitleMenuItem labelRecentFileTitle;
        private TitleMenuItem labelUserAppTitle;
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
        private uint ProcessID;
        private List<ToolStripItem> RecentFileItemsList = new List<ToolStripItem>();
        private TitleMenuItem recentFileMenuItem;
        private IShellBrowser shellBrowser;
        private NativeWindowControler shellViewControler;
        private StringFormat stringFormat;
        private SubDirTipForm subDirTip;
        private const string TEXT_TOOLBAR = "QTTab Desktop Tool";
        private IntPtr ThisHandle;
        private int thumbnailIndex = -1;
        private ThumbnailTooltipForm thumbnailTooltip;
        private System.Windows.Forms.Timer timer_HoverSubDirTipMenu;
        private System.Windows.Forms.Timer timer_HoverThumbnail;
        private System.Windows.Forms.Timer timer_Thumbnail;
        private System.Windows.Forms.Timer timerHooks;
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

        public QTCoTaskBarClass() {
            byte[] buffer = new byte[3];
            buffer[1] = 1;
            buffer[2] = 2;
            this.Order_Root = buffer;
            byte[] buffer2 = new byte[4];
            buffer2[0] = 1;
            this.ConfigValues = buffer2;
            this.WidthOfBar = 80;
        }

        private void AddMenuItems_Group() {
            if((this.ConfigValues[2] & 0x80) == 0) {
                if(this.ExpandState[0]) {
                    this.contextMenu.AddItem(this.labelGroupTitle, "labelG");
                    this.contextMenu.AddItemsRange(this.GroupItemsList.ToArray(), "groupItem");
                }
                else {
                    this.ddmrGroups.AddItemsRange(this.GroupItemsList.ToArray(), "groupItem");
                    this.contextMenu.AddItem(this.groupsMenuItem, "submenu");
                }
            }
        }

        private void AddMenuItems_History() {
            if(((this.ConfigValues[2] & 0x40) == 0) && !QTUtility.CheckConfig(5, 2)) {
                if(this.ExpandState[1]) {
                    this.contextMenu.AddItem(this.labelHistoryTitle, "labelH");
                    this.contextMenu.AddItemsRange(this.UndoClosedItemsList.ToArray(), "historyItem");
                }
                else {
                    this.ddmrHistory.AddItemsRange(this.UndoClosedItemsList.ToArray(), "historyItem");
                    this.contextMenu.AddItem(this.historyMenuItem, "submenu");
                }
            }
        }

        private void AddMenuItems_Recent() {
            if(((this.ConfigValues[2] & 1) == 0) && !QTUtility.CheckConfig(6, 0x20)) {
                if(this.ExpandState[3]) {
                    this.contextMenu.AddItem(this.labelRecentFileTitle, "labelR");
                    this.contextMenu.AddItemsRange(this.RecentFileItemsList.ToArray(), "recentItem");
                }
                else {
                    this.ddmrRecentFile.AddItemsRange(this.RecentFileItemsList.ToArray(), "recentItem");
                    this.contextMenu.AddItem(this.recentFileMenuItem, "submenu");
                }
            }
        }

        private void AddMenuItems_UserApp() {
            if((this.ConfigValues[2] & 0x20) == 0) {
                if(this.ExpandState[2]) {
                    this.contextMenu.AddItem(this.labelUserAppTitle, "labelU");
                    this.contextMenu.AddItemsRange(this.UserappItemsList.ToArray(), "userappItem");
                }
                else {
                    this.ddmrUserapps.AddItemsRange(this.UserappItemsList.ToArray(), "userappItem");
                    this.contextMenu.AddItem(this.userAppsMenuItem, "submenu");
                }
            }
        }

        void BandObjectLib.IPersistStream.GetClassID(out Guid pClassID) {
            pClassID = new Guid("D2BF470E-ED1C-487F-A555-2BD8835EB6CE");
        }

        int BandObjectLib.IPersistStream.GetSizeMax(out ulong pcbSize) {
            pcbSize = 0L;
            return 0;
        }

        void BandObjectLib.IPersistStream.IPersistStreamLoad(object pStm) {
        }

        int BandObjectLib.IPersistStream.IsDirty() {
            return 0;
        }

        void BandObjectLib.IPersistStream.Save(IntPtr pStm, bool fClearDirty) {
        }

        private void BlockedExplorer_Exited(object sender, EventArgs e) {
            Process.Start(this.BlockedExplorerURL);
            this.BlockedExplorerURL = string.Empty;
            this.BlockedExplorerProcess.Close();
            this.BlockedExplorerProcess = null;
        }

        private IntPtr CallbackGetMsgProc_Desktop(int nCode, IntPtr wParam, IntPtr lParam) {
            if(nCode >= 0) {
                BandObjectLib.MSG msg = (BandObjectLib.MSG)Marshal.PtrToStructure(lParam, typeof(BandObjectLib.MSG));
                if(msg.hwnd != this.hwndListView) {
                    return PInvoke.CallNextHookEx(this.hHook_MsgDesktop, nCode, wParam, lParam);
                }
                switch(msg.message) {
                    case 520:
                        if(!QTUtility.CheckConfig(5, 1)) {
                            int index = PInvoke.ListView_HitTest(this.hwndListView, msg.lParam);
                            if((index != -1) && this.HandleTabFolderActions(index, Control.ModifierKeys, false)) {
                                Marshal.StructureToPtr(new BandObjectLib.MSG(), lParam, false);
                            }
                        }
                        break;

                    case 0x20a: {
                            IntPtr handle = PInvoke.WindowFromPoint(new Point(QTUtility2.GET_X_LPARAM(msg.lParam), QTUtility2.GET_Y_LPARAM(msg.lParam)));
                            if((handle != IntPtr.Zero) && (handle != msg.hwnd)) {
                                Control control = Control.FromHandle(handle);
                                if(control != null) {
                                    DropDownMenuReorderable reorderable = control as DropDownMenuReorderable;
                                    if((reorderable != null) && reorderable.CanScroll) {
                                        PInvoke.SendMessage(handle, 0x20a, msg.wParam, msg.lParam);
                                    }
                                }
                            }
                            break;
                        }
                    case 0x8064:
                        this.GetFolderView();
                        break;

                    case 0x203:
                        if(((this.ConfigValues[1] & 4) == 0) && (PInvoke.ListView_HitTest(this.hwndListView, msg.lParam) == -1)) {
                            QTUtility2.SendCOPYDATASTRUCT(this.ThisHandle, (IntPtr)0xff, "fromdesktop", msg.lParam);
                        }
                        break;
                }
            }
            return PInvoke.CallNextHookEx(this.hHook_MsgDesktop, nCode, wParam, lParam);
        }

        private IntPtr CallbackGetMsgProc_ShellTrayWnd(int nCode, IntPtr wParam, IntPtr lParam) {
            if(nCode >= 0) {
                BandObjectLib.MSG msg = (BandObjectLib.MSG)Marshal.PtrToStructure(lParam, typeof(BandObjectLib.MSG));
                switch(msg.message) {
                    case 0xa3:
                        if(((this.ConfigValues[1] & 8) == 0) && (msg.hwnd == this.hwndShellTray)) {
                            this.OnDesktopDblClicked(Control.MousePosition);
                            Marshal.StructureToPtr(new BandObjectLib.MSG(), lParam, false);
                        }
                        break;

                    case 0x20a: {
                            IntPtr handle = PInvoke.WindowFromPoint(new Point(QTUtility2.GET_X_LPARAM(msg.lParam), QTUtility2.GET_Y_LPARAM(msg.lParam)));
                            if((handle != IntPtr.Zero) && (handle != msg.hwnd)) {
                                Control control = Control.FromHandle(handle);
                                if(control != null) {
                                    DropDownMenuReorderable reorderable = control as DropDownMenuReorderable;
                                    if((reorderable != null) && reorderable.CanScroll) {
                                        PInvoke.SendMessage(handle, 0x20a, msg.wParam, msg.lParam);
                                        Marshal.StructureToPtr(new BandObjectLib.MSG(), lParam, false);
                                    }
                                }
                            }
                            break;
                        }
                }
            }
            return PInvoke.CallNextHookEx(this.hHook_MsgShell_TrayWnd, nCode, wParam, lParam);
        }

        private IntPtr CallbackKeyProc_Desktop(int nCode, IntPtr wParam, IntPtr lParam) {
            if(nCode >= 0) {
                if((((int)lParam) & -2147483648) == 0) {
                    if(this.HandleKEYDOWN(wParam, (((int)lParam) & 0x40000000) == 0x40000000)) {
                        return new IntPtr(1);
                    }
                }
                else {
                    this.HideThumbnailTooltip();
                    if((!QTUtility.CheckConfig(9, 0x40) && QTUtility.CheckConfig(9, 0x20)) && ((this.subDirTip != null) && !this.subDirTip.MenuIsShowing)) {
                        this.HideSubDirTip();
                    }
                }
            }
            return PInvoke.CallNextHookEx(this.hHook_KeyDesktop, nCode, wParam, lParam);
        }

        public void CanRenderComposited(out bool pfCanRenderComposited) {
            pfCanRenderComposited = true;
        }

        private void ClearMenuItems() {
            this.fRequiredRefresh_MenuItems = false;
            if(this.contextMenu.Items.Count > 0) {
                this.contextMenu.ItemsClear();
                this.ddmrGroups.ItemsClear();
                this.ddmrHistory.ItemsClear();
                this.ddmrUserapps.ItemsClear();
                this.ddmrRecentFile.ItemsClear();
                foreach(ToolStripItem item in this.GroupItemsList) {
                    item.Dispose();
                }
                foreach(ToolStripItem item2 in this.UndoClosedItemsList) {
                    item2.Dispose();
                }
                foreach(ToolStripItem item3 in this.UserappItemsList) {
                    item3.Dispose();
                }
                foreach(ToolStripItem item4 in this.RecentFileItemsList) {
                    item4.Dispose();
                }
                this.GroupItemsList.Clear();
                this.UndoClosedItemsList.Clear();
                this.UserappItemsList.Clear();
                this.RecentFileItemsList.Clear();
            }
        }

        private void ClearThumbnailCache() {
            this.iMainMenuShownCount = 0;
            if(this.subDirTip != null) {
                this.subDirTip.ClearThumbnailCache();
            }
            if(this.thumbnailTooltip != null) {
                this.HideThumbnailTooltip();
                this.thumbnailTooltip.ClearCache();
            }
        }

        public override void CloseDW(uint dwReserved) {
            if(iContextMenu2 != null) {
                Marshal.ReleaseComObject(iContextMenu2);
                iContextMenu2 = null;
            }
            if(this.folderView != null) {
                Marshal.ReleaseComObject(this.folderView);
                this.folderView = null;
            }
            if(this.shellBrowser != null) {
                Marshal.ReleaseComObject(this.shellBrowser);
                this.shellBrowser = null;
            }
            DisposeInvoker method = new DisposeInvoker(this.InvokeDispose);
            if(this.thumbnailTooltip != null) {
                this.thumbnailTooltip.Invoke(method, new object[] { this.thumbnailTooltip });
                this.thumbnailTooltip = null;
            }
            if(this.subDirTip != null) {
                this.subDirTip.Invoke(method, new object[] { this.subDirTip });
                this.subDirTip = null;
            }
            if(this.hashForm != null) {
                this.hashForm.Invoke(method, new object[] { this.hashForm });
                this.hashForm = null;
            }
            if(this.hHook_MsgDesktop != IntPtr.Zero) {
                PInvoke.UnhookWindowsHookEx(this.hHook_MsgDesktop);
                this.hHook_MsgDesktop = IntPtr.Zero;
            }
            if(this.hHook_MsgShell_TrayWnd != IntPtr.Zero) {
                PInvoke.UnhookWindowsHookEx(this.hHook_MsgShell_TrayWnd);
                this.hHook_MsgShell_TrayWnd = IntPtr.Zero;
            }
            if(this.hHook_KeyDesktop != IntPtr.Zero) {
                PInvoke.UnhookWindowsHookEx(this.hHook_KeyDesktop);
                this.hHook_KeyDesktop = IntPtr.Zero;
            }
            if(this.shellViewControler != null) {
                this.shellViewControler.ReleaseHandle();
                this.shellViewControler = null;
            }
            base.CloseDW(dwReserved);
        }

        private void contextMenu_Closing(object sender, ToolStripDropDownClosingEventArgs e) {
            if(this.CancelClosing) {
                e.Cancel = true;
                this.CancelClosing = false;
            }
            else {
                List<int> list = new List<int>();
                for(int i = 0; i < this.contextMenu.Items.Count; i++) {
                    if(list.Count == this.Order_Root.Length) {
                        break;
                    }
                    ToolStripItem item = this.contextMenu.Items[i];
                    if(item is TitleMenuItem) {
                        if((item == this.groupsMenuItem) || (item == this.labelGroupTitle)) {
                            list.Add(0);
                        }
                        else if((item == this.historyMenuItem) || (item == this.labelHistoryTitle)) {
                            list.Add(1);
                        }
                        else if((item == this.userAppsMenuItem) || (item == this.labelUserAppTitle)) {
                            list.Add(2);
                        }
                        else if((item == this.recentFileMenuItem) || (item == this.labelRecentFileTitle)) {
                            list.Add(3);
                        }
                    }
                }
                for(int j = 0; j < 3; j++) {
                    if(j < list.Count) {
                        this.Order_Root[j] = (byte)list[j];
                    }
                    else {
                        this.Order_Root[j] = 15;
                    }
                }
                this.SaveSetting();
            }
        }

        private void contextMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            TitleMenuItem clickedItem = e.ClickedItem as TitleMenuItem;
            if(clickedItem != null) {
                if(clickedItem.IsOpened) {
                    this.OnLabelTitleClickedToClose(clickedItem.Genre);
                }
                else {
                    this.OnSubMenuTitleClickedToOpen(clickedItem.Genre);
                }
            }
            else {
                QMenuItem item2 = e.ClickedItem as QMenuItem;
                if(item2 != null) {
                    if(item2.Genre == MenuGenre.Group) {
                        Keys modifierKeys = Control.ModifierKeys;
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
                                    string str2 = string.Empty;
                                    foreach(string str3 in QTUtility.StartUpGroupList) {
                                        str2 = str2 + str3 + ";";
                                    }
                                    str2 = str2.TrimEnd(QTUtility.SEPARATOR_CHAR);
                                    key.SetValue("StartUpGroups", str2);
                                }
                            }
                        }
                        else {
                            Thread thread = new Thread(new ParameterizedThreadStart(this.OpenGroup));
                            thread.SetApartmentState(ApartmentState.STA);
                            thread.IsBackground = true;
                            thread.Start(new object[] { text, Control.ModifierKeys });
                        }
                    }
                    else if(item2.Genre == MenuGenre.History) {
                        Thread thread2 = new Thread(new ParameterizedThreadStart(this.OpenTab));
                        thread2.SetApartmentState(ApartmentState.STA);
                        thread2.IsBackground = true;
                        thread2.Start(new object[] { item2.Path, Control.ModifierKeys });
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
                            startInfo.ErrorDialogParentHandle = this.ThisHandle;
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
                        foreach(ToolStripItem item2 in this.contextMenu.Items) {
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
                        foreach(ToolStripItem item4 in this.contextMenu.Items) {
                            QMenuItem item5 = item4 as QMenuItem;
                            if(item5 != null) {
                                if(item5.Genre == MenuGenre.Application) {
                                    if(item5.Target == MenuTarget.VirtualFolder) {
                                        key2.SetValue(item4.Text, new byte[0]);
                                    }
                                    else {
                                        QTUtility2.WriteRegBinary<string>(QTUtility.UserAppsDic[item4.Text], item4.Text, key2);
                                    }
                                }
                                continue;
                            }
                            if((item4 is ToolStripSeparator) && (item4.Name == "appSep")) {
                                QTUtility2.WriteRegBinary<string>(array, "Separator" + num2++, key2);
                            }
                        }
                    }
                    QTUtility.fRequiredRefresh_App = true;
                }
            }
        }

        private void contextMenuForSetting_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            if(!(e.ClickedItem is ToolStripSeparator)) {
                if(e.ClickedItem == this.tsmiTaskBar) {
                    this.tsmiTaskBar.Checked = !this.tsmiTaskBar.Checked;
                }
                else if(e.ClickedItem == this.tsmiDesktop) {
                    this.tsmiDesktop.Checked = !this.tsmiDesktop.Checked;
                }
                else if(e.ClickedItem == this.tsmiLockItems) {
                    this.tsmiLockItems.Checked = !this.tsmiLockItems.Checked;
                    this.fRequiredRefresh_MenuItems = true;
                    this.contextMenu.ReorderEnabled = this.ddmrGroups.ReorderEnabled = this.ddmrUserapps.ReorderEnabled = !this.tsmiLockItems.Checked;
                }
                else if(e.ClickedItem == this.tsmiOnGroup) {
                    this.fRequiredRefresh_MenuItems = true;
                    this.tsmiOnGroup.Checked = !this.tsmiOnGroup.Checked;
                }
                else if(e.ClickedItem == this.tsmiOnHistory) {
                    this.fRequiredRefresh_MenuItems = true;
                    this.tsmiOnHistory.Checked = !this.tsmiOnHistory.Checked;
                }
                else if(e.ClickedItem == this.tsmiOnUserApps) {
                    this.fRequiredRefresh_MenuItems = true;
                    this.tsmiOnUserApps.Checked = !this.tsmiOnUserApps.Checked;
                }
                else if(e.ClickedItem == this.tsmiOnRecentFile) {
                    this.fRequiredRefresh_MenuItems = true;
                    this.tsmiOnRecentFile.Checked = !this.tsmiOnRecentFile.Checked;
                }
                else if(e.ClickedItem == this.tsmiVSTitle) {
                    this.tsmiVSTitle.Checked = !this.tsmiVSTitle.Checked;
                    TitleMenuItem.DrawBackground = this.tsmiVSTitle.Checked;
                }
                else if(e.ClickedItem == this.tsmiOneClick) {
                    this.tsmiOneClick.Checked = !this.tsmiOneClick.Checked;
                }
                else if(e.ClickedItem == this.tsmiAppKeys) {
                    this.tsmiAppKeys.Checked = !this.tsmiAppKeys.Checked;
                    if(this.tsmiAppKeys.Checked) {
                        this.ConfigValues[2] = (byte)(this.ConfigValues[2] & 0xf7);
                    }
                    else {
                        this.ConfigValues[2] = (byte)(this.ConfigValues[2] | 8);
                    }
                }
                this.SaveSetting();
            }
        }

        private void contextMenuForSetting_Opening(object sender, CancelEventArgs e) {
            string[] strArray = QTUtility.TextResourcesDic["TaskBar_Menu"];
            this.tsmiTaskBar.Text = strArray[0];
            this.tsmiDesktop.Text = strArray[1];
            this.tsmiLockItems.Text = strArray[2];
            this.tsmiVSTitle.Text = strArray[3];
            this.tsmiOneClick.Text = strArray[4];
            this.tsmiAppKeys.Text = strArray[5];
            string[] strArray2 = QTUtility.TextResourcesDic["TaskBar_Titles"];
            this.tsmiOnGroup.Text = strArray2[0];
            this.tsmiOnHistory.Text = strArray2[1];
            this.tsmiOnUserApps.Text = strArray2[2];
            this.tsmiOnRecentFile.Text = strArray2[3];
        }

        protected override void Dispose(bool disposing) {
            if(disposing && (this.components != null)) {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void DoFileTools(int index) {
            try {
                if((index == 2) || (index == 3)) {
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
                }
                else if(this.folderView != null) {
                    int num;
                    int iItem = -1;
                    if((this.folderView.ItemCount(2, out num) == 0) && (num > 0)) {
                        List<IntPtr> list = new List<IntPtr>();
                        for(int i = 0; i < num; i++) {
                            IntPtr ptr;
                            if(((2 == ((int)PInvoke.SendMessage(this.hwndListView, 0x102c, (IntPtr)i, (IntPtr)2))) && (this.folderView.Item(i, out ptr) == 0)) && (ptr != IntPtr.Zero)) {
                                iItem = i;
                                list.Add(ptr);
                            }
                        }
                        if(index == 4) {
                            List<string> list2 = new List<string>();
                            foreach(IntPtr ptr2 in list) {
                                using(IDLWrapper wrapper2 = new IDLWrapper(ptr2)) {
                                    if(wrapper2.IsLink) {
                                        string linkTargetPath = ShellMethods.GetLinkTargetPath(wrapper2.Path);
                                        if(File.Exists(linkTargetPath)) {
                                            list2.Add(linkTargetPath);
                                        }
                                    }
                                    else if(wrapper2.IsFileSystemFile) {
                                        list2.Add(wrapper2.Path);
                                    }
                                    continue;
                                }
                            }
                            if(this.hashForm == null) {
                                this.hashForm = new FileHashComputerForm();
                            }
                            this.hashForm.ShowFileHashForm(list2.ToArray());
                        }
                        else if(index == 5) {
                            if(list.Count == 1) {
                                if(this.ShowSubDirTip(list[0], iItem, this.hwndListView, false)) {
                                    this.subDirTip.PerformClickByKey();
                                }
                                else {
                                    this.HideSubDirTip();
                                }
                            }
                            foreach(IntPtr ptr3 in list) {
                                PInvoke.CoTaskMemFree(ptr3);
                            }
                        }
                        else if((index == 0) || (index == 1)) {
                            bool flag = index == 0;
                            string str = string.Empty;
                            foreach(IntPtr ptr4 in list) {
                                string displayName = ShellMethods.GetDisplayName(ptr4, !flag);
                                if(displayName.Length > 0) {
                                    str = str + ((str.Length == 0) ? string.Empty : "\r\n") + displayName;
                                }
                                PInvoke.CoTaskMemFree(ptr4);
                            }
                            if(str.Length > 0) {
                                QTTabBarClass.SetStringClipboard(str);
                            }
                        }
                    }
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
            Point pnt = e.IsKey ? e.Point : Control.MousePosition;
            if(clickedItem.Target == MenuTarget.VirtualFolder) {
                return;
            }
            if(clickedItem.Genre == MenuGenre.Group) {
                this.fContextmenuedOnMain_Grp = sender == this.contextMenu;
                string str = MenuUtility.TrackGroupContextMenu(e.ClickedItem.Text, pnt, ((DropDownMenuReorderable)sender).Handle);
                this.fContextmenuedOnMain_Grp = false;
                if(!string.IsNullOrEmpty(str)) {
                    this.OpenTab(new object[] { str, Control.ModifierKeys });
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
                this.UndoClosedItemsList.Remove(clickedItem);
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
                this.RecentFileItemsList.Remove(clickedItem);
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

        private static IntPtr GetDesktopHwnd() {
            return PInvoke.FindWindowEx(PInvoke.FindWindowEx(PInvoke.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Progman", null), IntPtr.Zero, "SHELLDLL_DefView", null), IntPtr.Zero, "SysListView32", null);
        }

        private bool GetFolderView() {
            IntPtr pUnk = PInvoke.SendMessage(GetProgmanHWnd(), 0x407, IntPtr.Zero, IntPtr.Zero);
            if(pUnk != IntPtr.Zero) {
                try {
                    IShellView view;
                    this.shellBrowser = (IShellBrowser)Marshal.GetObjectForIUnknown(pUnk);
                    if(this.shellBrowser.QueryActiveShellView(out view) == 0) {
                        this.folderView = view as IFolderView;
                        if(this.folderView != null) {
                            return true;
                        }
                    }
                }
                catch {
                }
                finally {
                    Marshal.Release(pUnk);
                }
            }
            return false;
        }

        private static IntPtr GetProgmanHWnd() {
            return PInvoke.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Progman", null);
        }

        private bool GetTargetWindow(IDLWrapper idlw, bool fNeedWait, out IntPtr hwndTabBar, out bool fOpened) {
            hwndTabBar = IntPtr.Zero;
            fOpened = false;
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar")) {
                if(key != null) {
                    hwndTabBar = QTUtility2.ReadRegHandle("Handle", key);
                }
                if((hwndTabBar == IntPtr.Zero) || !PInvoke.IsWindow(hwndTabBar)) {
                    hwndTabBar = QTUtility.instanceManager.CurrentHandle;
                    if((idlw.Available && ((hwndTabBar == IntPtr.Zero) || !PInvoke.IsWindow(hwndTabBar))) && (this.shellBrowser.BrowseObject(idlw.PIDL, 1) == 0)) {
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
            QTUtility.RefreshGroupMenuesOnReorderFinished(this.groupsMenuItem.DropDownItems);
        }

        private void HandleDROPHILITED(int iItem, IntPtr hwndListView) {
            if(iItem == -1) {
                if(this.timer_HoverSubDirTipMenu != null) {
                    this.timer_HoverSubDirTipMenu.Enabled = false;
                }
                if(this.subDirTip != null) {
                    this.subDirTip.HideMenu();
                    this.HideSubDirTip();
                }
                this.itemIndexDROPHILITED = -1;
            }
            else {
                if(this.timer_HoverSubDirTipMenu == null) {
                    this.timer_HoverSubDirTipMenu = new System.Windows.Forms.Timer(this.components);
                    this.timer_HoverSubDirTipMenu.Interval = 0x4b0;
                    this.timer_HoverSubDirTipMenu.Tick += new EventHandler(this.timer_HoverSubDirTipMenu_Tick);
                }
                this.itemIndexDROPHILITED = iItem;
                this.timer_HoverSubDirTipMenu.Tag = hwndListView;
                this.timer_HoverSubDirTipMenu.Enabled = false;
                this.timer_HoverSubDirTipMenu.Enabled = true;
            }
        }

        private bool HandleKEYDOWN(IntPtr wParam, bool fRepeat) {
            int key = ((int)wParam) | ((int)Control.ModifierKeys);
            if(((int)wParam) == 0x10) {
                if(!fRepeat) {
                    if(!QTUtility.CheckConfig(8, 1)) {
                        this.HideThumbnailTooltip();
                    }
                    if(((!QTUtility.CheckConfig(9, 0x40) && !QTUtility.CheckConfig(9, 0x20)) && ((this.subDirTip != null) && this.subDirTip.IsShowing)) && !this.subDirTip.MenuIsShowing) {
                        this.HideSubDirTip();
                    }
                }
                return false;
            }
            if(key == 0x71) {
                if(!QTUtility.CheckConfig(10, 1)) {
                    QTTabBarClass.HandleF2(this.hwndListView);
                }
                return false;
            }
            key |= 0x100000;
            if(((key == QTUtility.ShortcutKeys[0x1b]) || (key == QTUtility.ShortcutKeys[0x1c])) || (((key == QTUtility.ShortcutKeys[0x1d]) || (key == QTUtility.ShortcutKeys[30])) || (key == QTUtility.ShortcutKeys[0x1f]))) {
                if(!fRepeat) {
                    if((this.subDirTip != null) && this.subDirTip.MenuIsShowing) {
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
                    this.DoFileTools(index);
                }
                return true;
            }
            if(key == QTUtility.ShortcutKeys[0x26]) {
                if(!QTUtility.CheckConfig(9, 0x40)) {
                    if(!fRepeat) {
                        this.DoFileTools(5);
                    }
                    return true;
                }
            }
            else {
                if(((this.ConfigValues[2] & 8) == 0) && QTUtility.dicUserAppShortcutKeys.ContainsKey(key)) {
                    if(fRepeat) {
                        goto Label_02D8;
                    }
                    MenuItemArguments mia = QTUtility.dicUserAppShortcutKeys[key];
                    if(this.folderView == null) {
                        goto Label_02D8;
                    }
                    Guid riid = ExplorerGUIDs.IID_IEnumIDList;
                    IEnumIDList ppv = null;
                    try {
                        try {
                            int num3;
                            List<Address> list2 = new List<Address>();
                            if(this.folderView.ItemCount(1, out num3) != 0) {
                                return false;
                            }
                            if(((num3 != 0) && (this.folderView.Items(1, ref riid, out ppv) == 0)) && (ppv != null)) {
                                IntPtr ptr;
                                while(ppv.Next(1, out ptr, null) == 0) {
                                    using(IDLWrapper wrapper = new IDLWrapper(ptr)) {
                                        if((wrapper.Available && wrapper.HasPath) && wrapper.IsFileSystem) {
                                            list2.Add(new Address(ptr, wrapper.Path));
                                        }
                                        continue;
                                    }
                                }
                            }
                            AppLauncher launcher = new AppLauncher(list2.ToArray(), Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                            launcher.ReplaceTokens_WorkingDir(mia);
                            launcher.ReplaceTokens_Arguments(mia);
                            AppLauncher.Execute(mia, IntPtr.Zero);
                            return true;
                        }
                        catch(Exception exception) {
                            QTUtility2.MakeErrorLog(exception, null);
                        }
                        goto Label_02D8;
                    }
                    finally {
                        if(ppv != null) {
                            Marshal.ReleaseComObject(ppv);
                            ppv = null;
                        }
                        mia.RestoreOriginalArgs();
                    }
                }
                if(!fRepeat && QTUtility.dicGroupShortcutKeys.ContainsKey(key)) {
                    Thread thread = new Thread(new ParameterizedThreadStart(this.OpenGroup));
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
            if(QTUtility.CheckConfig(6, 0x80)) {
                if(modKey == Keys.Control) {
                    modKey = Keys.None;
                }
                else if(modKey == Keys.None) {
                    modKey = Keys.Control;
                }
            }
            if(!QTUtility.CheckConfig(0, 0x80)) {
                if((modKey & Keys.Shift) == Keys.Shift) {
                    modKey &= ~Keys.Shift;
                }
                else {
                    modKey |= Keys.Shift;
                }
            }
            IEnumIDList ppv = null;
            Guid riid = ExplorerGUIDs.IID_IEnumIDList;
            if(this.folderView != null) {
                try {
                    IntPtr ptr2;
                    byte[] idl = null;
                    List<byte[]> list2 = new List<byte[]>();
                    List<string> list3 = new List<string>();
                    if(index != -1) {
                        IntPtr ptr;
                        if((this.folderView.Item(index, out ptr) == 0) && (ptr != IntPtr.Zero)) {
                            using(IDLWrapper wrapper = new IDLWrapper(ptr)) {
                                IDLWrapper wrapper2;
                                if(IDLIsFolder(wrapper, out wrapper2)) {
                                    if(wrapper2 != null) {
                                        idl = wrapper2.IDL;
                                        wrapper2.Dispose();
                                    }
                                    else {
                                        idl = wrapper.IDL;
                                    }
                                    goto Label_01D6;
                                }
                                return false;
                            }
                        }
                        return false;
                    }
                    if((this.folderView.Items(1, ref riid, out ppv) == 0) && (ppv != null)) {
                        goto Label_016C;
                    }
                    return false;
                Label_0107:
                    using(IDLWrapper wrapper3 = new IDLWrapper(ptr2)) {
                        IDLWrapper wrapper4;
                        if(IDLIsFolder(wrapper3, out wrapper4)) {
                            if(wrapper4 != null) {
                                list2.Add(wrapper4.IDL);
                                wrapper4.Dispose();
                            }
                            else {
                                list2.Add(wrapper3.IDL);
                            }
                        }
                        else if(fEnqExec && wrapper3.IsFileSystemFile) {
                            list3.Add(wrapper3.Path);
                        }
                    }
                Label_016C:
                    if(ppv.Next(1, out ptr2, null) == 0) {
                        goto Label_0107;
                    }
                    if(list2.Count > 0) {
                        idl = list2[0];
                        list2.RemoveAt(0);
                    }
                    else {
                        if(fEnqExec) {
                            foreach(string str in list3) {
                                QTUtility.ExecutedPathsList.Add(str);
                            }
                        }
                        return false;
                    }
                Label_01D6:
                    if(idl != null) {
                        if(list2.Count == 0) {
                            if(index == -1) {
                                using(IDLWrapper wrapper5 = new IDLWrapper(idl)) {
                                    if(wrapper5.IsFileSystemFile) {
                                        return false;
                                    }
                                }
                            }
                            object[] objArray = new object[3];
                            objArray[1] = modKey;
                            objArray[2] = idl;
                            this.OpenTab(objArray);
                            return true;
                        }
                        Thread thread = new Thread(new ParameterizedThreadStart(this.OpenFolders2));
                        thread.SetApartmentState(ApartmentState.STA);
                        thread.IsBackground = true;
                        thread.Start(new object[] { idl, list2, modKey });
                        return true;
                    }
                }
                finally {
                    if(ppv != null) {
                        Marshal.ReleaseComObject(ppv);
                    }
                }
            }
            return false;
        }

        private void HideSubDirTip() {
            if((this.subDirTip != null) && this.subDirTip.IsShowing) {
                this.subDirTip.HideSubDirTip();
            }
            this.itemIndexDROPHILITED = -1;
        }

        private void HideSubDirTip_DesktopInactivated() {
            if((this.subDirTip != null) && this.subDirTip.IsShowing) {
                this.subDirTip.OnExplorerInactivated();
            }
        }

        private void HideThumbnailTooltip() {
            if(((this.thumbnailTooltip != null) && this.thumbnailTooltip.IsShowing) && this.thumbnailTooltip.HideToolTip()) {
                this.thumbnailIndex = -1;
            }
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
            bool flag = (this.ConfigValues[1] & 2) == 0;
            this.components = new Container();
            this.contextMenu = new DropDownMenuReorderable(this.components, true, false);
            this.contextMenuForSetting = new ContextMenuStripEx(this.components, true);
            this.labelGroupTitle = new TitleMenuItem(MenuGenre.Group, true);
            this.labelHistoryTitle = new TitleMenuItem(MenuGenre.History, true);
            this.labelUserAppTitle = new TitleMenuItem(MenuGenre.Application, true);
            this.labelRecentFileTitle = new TitleMenuItem(MenuGenre.RecentFile, true);
            this.contextMenu.SuspendLayout();
            this.contextMenuForSetting.SuspendLayout();
            base.SuspendLayout();
            this.contextMenu.ProhibitedKey.Add("historyItem");
            this.contextMenu.ProhibitedKey.Add("recentItem");
            this.contextMenu.ReorderEnabled = flag;
            this.contextMenu.MessageParent = base.Handle;
            this.contextMenu.ImageList = QTUtility.ImageListGlobal;
            this.contextMenu.ItemClicked += new ToolStripItemClickedEventHandler(this.contextMenu_ItemClicked);
            this.contextMenu.Closing += new ToolStripDropDownClosingEventHandler(this.contextMenu_Closing);
            this.contextMenu.ReorderFinished += new MenuReorderedEventHandler(this.contextMenu_ReorderFinished);
            this.contextMenu.ItemRightClicked += new ItemRightClickedEventHandler(this.dropDownMenues_ItemRightClicked);
            if(QTUtility.IsVista) {
                IntPtr handle = this.contextMenu.Handle;
            }
            this.ddmrGroups = new DropDownMenuReorderable(this.components, true, false);
            this.ddmrGroups.ReorderEnabled = flag;
            this.ddmrGroups.ReorderFinished += new MenuReorderedEventHandler(this.groupsMenuItem_ReorderFinished);
            this.ddmrGroups.ItemRightClicked += new ItemRightClickedEventHandler(this.dropDownMenues_ItemRightClicked);
            this.groupsMenuItem = new TitleMenuItem(MenuGenre.Group, false);
            this.groupsMenuItem.DropDown = this.ddmrGroups;
            this.groupsMenuItem.DropDown.ImageList = QTUtility.ImageListGlobal;
            this.groupsMenuItem.DropDownItemClicked += new ToolStripItemClickedEventHandler(this.contextMenu_ItemClicked);
            this.ddmrHistory = new DropDownMenuReorderable(this.components, true, false);
            this.ddmrHistory.ReorderEnabled = false;
            this.ddmrHistory.MessageParent = base.Handle;
            this.ddmrHistory.ItemRightClicked += new ItemRightClickedEventHandler(this.dropDownMenues_ItemRightClicked);
            this.historyMenuItem = new TitleMenuItem(MenuGenre.History, false);
            this.historyMenuItem.DropDown = this.ddmrHistory;
            this.historyMenuItem.DropDown.ImageList = QTUtility.ImageListGlobal;
            this.historyMenuItem.DropDownItemClicked += new ToolStripItemClickedEventHandler(this.contextMenu_ItemClicked);
            this.ddmrUserapps = new DropDownMenuReorderable(this.components);
            this.ddmrUserapps.ReorderEnabled = flag;
            this.ddmrUserapps.MessageParent = base.Handle;
            this.ddmrUserapps.ReorderFinished += new MenuReorderedEventHandler(this.userAppsMenuItem_ReorderFinished);
            this.ddmrUserapps.ItemRightClicked += new ItemRightClickedEventHandler(this.dropDownMenues_ItemRightClicked);
            this.userAppsMenuItem = new TitleMenuItem(MenuGenre.Application, false);
            this.userAppsMenuItem.DropDown = this.ddmrUserapps;
            this.userAppsMenuItem.DropDown.ImageList = QTUtility.ImageListGlobal;
            this.userAppsMenuItem.DropDownItemClicked += new ToolStripItemClickedEventHandler(this.contextMenu_ItemClicked);
            this.ddmrRecentFile = new DropDownMenuReorderable(this.components, false, false, false);
            this.ddmrRecentFile.MessageParent = base.Handle;
            this.ddmrRecentFile.ItemRightClicked += new ItemRightClickedEventHandler(this.dropDownMenues_ItemRightClicked);
            this.recentFileMenuItem = new TitleMenuItem(MenuGenre.RecentFile, false);
            this.recentFileMenuItem.DropDown = this.ddmrRecentFile;
            this.recentFileMenuItem.DropDown.ImageList = QTUtility.ImageListGlobal;
            this.recentFileMenuItem.DropDownItemClicked += new ToolStripItemClickedEventHandler(this.contextMenu_ItemClicked);
            this.contextMenuForSetting.ShowImageMargin = false;
            this.tsmiTaskBar = new ToolStripMenuItem();
            this.tsmiDesktop = new ToolStripMenuItem();
            this.tsmiLockItems = new ToolStripMenuItem();
            this.tsmiVSTitle = new ToolStripMenuItem();
            this.tsmiTaskBar.Checked = (this.ConfigValues[1] & 8) == 0;
            this.tsmiDesktop.Checked = (this.ConfigValues[1] & 4) == 0;
            this.tsmiLockItems.Checked = (this.ConfigValues[1] & 2) == 2;
            this.tsmiVSTitle.Checked = (this.ConfigValues[1] & 1) == 0;
            this.tsmiOnGroup = new ToolStripMenuItem();
            this.tsmiOnHistory = new ToolStripMenuItem();
            this.tsmiOnUserApps = new ToolStripMenuItem();
            this.tsmiOnRecentFile = new ToolStripMenuItem();
            this.tsmiOneClick = new ToolStripMenuItem();
            this.tsmiAppKeys = new ToolStripMenuItem();
            this.tsmiOnGroup.Checked = (this.ConfigValues[2] & 0x80) == 0;
            this.tsmiOnHistory.Checked = (this.ConfigValues[2] & 0x40) == 0;
            this.tsmiOnUserApps.Checked = (this.ConfigValues[2] & 0x20) == 0;
            this.tsmiOnRecentFile.Checked = (this.ConfigValues[2] & 1) == 0;
            this.tsmiOneClick.Checked = (this.ConfigValues[2] & 0x10) != 0;
            this.tsmiAppKeys.Checked = (this.ConfigValues[2] & 8) == 0;
            this.contextMenuForSetting.Items.AddRange(new ToolStripItem[] { this.tsmiTaskBar, this.tsmiDesktop, new ToolStripSeparator(), this.tsmiOnGroup, this.tsmiOnHistory, this.tsmiOnUserApps, this.tsmiOnRecentFile, new ToolStripSeparator(), this.tsmiLockItems, this.tsmiVSTitle, this.tsmiOneClick, this.tsmiAppKeys });
            this.contextMenuForSetting.ItemClicked += new ToolStripItemClickedEventHandler(this.contextMenuForSetting_ItemClicked);
            this.contextMenuForSetting.Opening += new CancelEventHandler(this.contextMenuForSetting_Opening);
            this.ContextMenuStrip = this.contextMenuForSetting;
            base.Width = this.WidthOfBar;
            base.MinSize = new Size(30, 0x16);
            this.Dock = DockStyle.Fill;
            base.MouseClick += new MouseEventHandler(this.QTCoTaskBarClass_MouseClick);
            base.MouseDoubleClick += new MouseEventHandler(this.QTCoTaskBarClass_MouseDoubleClick);
            this.contextMenu.ResumeLayout(false);
            this.contextMenuForSetting.ResumeLayout(false);
            base.ResumeLayout(false);
        }

        private void InsertMenuItems_History() {
            if(((this.ConfigValues[2] & 0x40) == 0) && !QTUtility.CheckConfig(5, 2)) {
                if(this.ExpandState[1]) {
                    int index = this.contextMenu.Items.IndexOf(this.labelHistoryTitle);
                    if(index != -1) {
                        foreach(ToolStripItem item in this.UndoClosedItemsList) {
                            this.contextMenu.Items.Insert(++index, item);
                        }
                    }
                }
                else {
                    this.ddmrHistory.AddItemsRange(this.UndoClosedItemsList.ToArray(), "historyItem");
                }
            }
        }

        private void InsertMenuItems_Recent() {
            if(((this.ConfigValues[2] & 1) == 0) && !QTUtility.CheckConfig(6, 0x20)) {
                if(this.ExpandState[3]) {
                    int index = this.contextMenu.Items.IndexOf(this.labelRecentFileTitle);
                    if(index != -1) {
                        foreach(ToolStripItem item in this.RecentFileItemsList) {
                            this.contextMenu.Items.Insert(++index, item);
                        }
                    }
                }
                else {
                    this.ddmrRecentFile.AddItemsRange(this.RecentFileItemsList.ToArray(), "recentItem");
                }
            }
        }

        private void InstallDesktopHook() {
            uint num;
            IntPtr desktopHwnd = GetDesktopHwnd();
            if(this.timerHooks == null) {
                if(desktopHwnd == IntPtr.Zero) {
                    this.timerHooks = new System.Windows.Forms.Timer();
                    this.timerHooks.Tick += new EventHandler(this.timerHooks_Tick);
                    this.timerHooks.Interval = 0xbb8;
                    this.timerHooks.Start();
                    return;
                }
            }
            else {
                if(desktopHwnd == IntPtr.Zero) {
                    return;
                }
                this.timerHooks.Stop();
                this.timerHooks.Dispose();
                this.timerHooks = null;
            }
            this.hwndListView = desktopHwnd;
            this.hwndShellTray = WindowUtils.GetShellTrayWnd();
            this.hookProc_Msg_Desktop = new QTTabBarLib.Interop.HookProc(this.CallbackGetMsgProc_Desktop);
            this.hookProc_Msg_ShellTrayWnd = new QTTabBarLib.Interop.HookProc(this.CallbackGetMsgProc_ShellTrayWnd);
            this.hookProc_Keys_Desktop = new QTTabBarLib.Interop.HookProc(this.CallbackKeyProc_Desktop);
            int windowThreadProcessId = PInvoke.GetWindowThreadProcessId(this.hwndListView, out num);
            int dwThreadId = PInvoke.GetWindowThreadProcessId(this.hwndShellTray, out this.ProcessID);
            this.hHook_MsgDesktop = PInvoke.SetWindowsHookEx(3, this.hookProc_Msg_Desktop, IntPtr.Zero, windowThreadProcessId);
            this.hHook_MsgShell_TrayWnd = PInvoke.SetWindowsHookEx(3, this.hookProc_Msg_ShellTrayWnd, IntPtr.Zero, dwThreadId);
            this.hHook_KeyDesktop = PInvoke.SetWindowsHookEx(2, this.hookProc_Keys_Desktop, IntPtr.Zero, windowThreadProcessId);
            PInvoke.PostMessage(this.hwndListView, 0x8064, IntPtr.Zero, IntPtr.Zero);
            IntPtr windowLongPtr = PInvoke.GetWindowLongPtr(this.hwndListView, -8);
            if(windowLongPtr != IntPtr.Zero) {
                this.shellViewControler = new NativeWindowControler(windowLongPtr);
                this.shellViewControler.OptionalHandle = this.hwndListView;
                this.shellViewControler.MessageCaptured += new NativeWindowControler.MessageEventHandler(this.shellViewControler_MessageCaptured);
            }
        }

        private void InvokeDispose(IDisposable disposable) {
            disposable.Dispose();
        }

        protected override void OnCreateControl() {
            base.OnCreateControl();
            this.ThisHandle = base.Handle;
            this.InstallDesktopHook();
        }

        private void OnDesktopDblClicked(Point popUpPoint) {
            if(++this.iMainMenuShownCount > 0x40) {
                this.ClearThumbnailCache();
            }
            this.contextMenu.SuspendLayout();
            this.ddmrGroups.SuspendLayout();
            this.ddmrHistory.SuspendLayout();
            this.ddmrUserapps.SuspendLayout();
            this.ddmrRecentFile.SuspendLayout();
            if(this.fRequiredRefresh_MenuItems) {
                this.ClearMenuItems();
            }
            this.RecreateUndoClosedItems();
            if(this.contextMenu.Items.Count == 0) {
                string[] strArray = QTUtility.TextResourcesDic["TaskBar_Titles"];
                this.labelGroupTitle.Text = this.groupsMenuItem.Text = strArray[0];
                this.historyMenuItem.Text = this.labelHistoryTitle.Text = strArray[1];
                this.userAppsMenuItem.Text = this.labelUserAppTitle.Text = strArray[2];
                this.recentFileMenuItem.Text = this.labelRecentFileTitle.Text = strArray[3];
                if((this.ConfigValues[2] & 0x80) == 0) {
                    QTUtility.RefreshGroupsDic();
                    foreach(string str in QTUtility.GroupPathsDic.Keys) {
                        string str2 = QTUtility.GroupPathsDic[str];
                        if(str2.Length == 0) {
                            ToolStripSeparator separator = new ToolStripSeparator();
                            separator.Name = "groupSep";
                            this.GroupItemsList.Add(separator);
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
                        this.GroupItemsList.Add(item);
                    }
                }
                if((this.ConfigValues[2] & 0x20) == 0) {
                    this.UserappItemsList = MenuUtility.CreateAppLauncherItems(base.Handle, (this.ConfigValues[1] & 2) == 0, new ItemRightClickedEventHandler(this.dropDownMenues_ItemRightClicked), new EventHandler(QTCoTaskBarClass.subMenuItems_DoubleClick), true);
                }
                bool flag = false;
                bool flag2 = false;
                bool flag3 = false;
                bool flag4 = false;
                for(int i = 0; i < 3; i++) {
                    switch(this.Order_Root[i]) {
                        case 0:
                            if(!flag) {
                                this.AddMenuItems_Group();
                                flag = true;
                            }
                            break;

                        case 1:
                            if(!flag2) {
                                this.AddMenuItems_History();
                                flag2 = true;
                            }
                            break;

                        case 2:
                            if(!flag3) {
                                this.AddMenuItems_UserApp();
                                flag3 = true;
                            }
                            break;

                        case 3:
                            if(!flag4) {
                                this.AddMenuItems_Recent();
                                flag4 = true;
                            }
                            break;
                    }
                }
                if(!flag) {
                    this.AddMenuItems_Group();
                }
                if(!flag2) {
                    this.AddMenuItems_History();
                }
                if(!flag3) {
                    this.AddMenuItems_UserApp();
                }
                if(!flag4) {
                    this.AddMenuItems_Recent();
                }
            }
            else {
                this.InsertMenuItems_History();
                this.InsertMenuItems_Recent();
            }
            this.ddmrUserapps.ResumeLayout();
            this.ddmrHistory.ResumeLayout();
            this.ddmrGroups.ResumeLayout();
            this.ddmrRecentFile.ResumeLayout();
            this.contextMenu.ResumeLayout();
            if(this.contextMenu.Items.Count > 0) {
                if(QTUtility.IsVista) {
                    this.contextMenu.SendToBack();
                }
                this.contextMenu.Show(popUpPoint);
            }
        }

        protected override void OnHandleCreated(EventArgs e) {
            base.OnHandleCreated(e);
            if(base.IsHandleCreated) {
                using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar")) {
                    QTUtility2.WriteRegHandle("TaskBarHandle", key, base.Handle);
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
            this.ExpandState[index] = !this.ExpandState[index];
            this.CancelClosing = true;
            switch(index) {
                case 0:
                    labelGroupTitle = this.labelGroupTitle;
                    groupItemsList = this.GroupItemsList;
                    groupsMenuItem = this.groupsMenuItem;
                    str = "groupItem";
                    break;

                case 1:
                    labelGroupTitle = this.labelHistoryTitle;
                    groupItemsList = this.UndoClosedItemsList;
                    groupsMenuItem = this.historyMenuItem;
                    str = "historyItem";
                    break;

                case 2:
                    labelGroupTitle = this.labelUserAppTitle;
                    groupItemsList = this.UserappItemsList;
                    groupsMenuItem = this.userAppsMenuItem;
                    str = "userappItem";
                    break;

                default:
                    labelGroupTitle = this.labelRecentFileTitle;
                    groupItemsList = this.RecentFileItemsList;
                    groupsMenuItem = this.recentFileMenuItem;
                    str = "recentItem";
                    break;
            }
            int num2 = this.contextMenu.Items.IndexOf(labelGroupTitle);
            this.contextMenu.SuspendLayout();
            this.contextMenu.Items.Remove(labelGroupTitle);
            foreach(ToolStripItem item3 in groupItemsList) {
                this.contextMenu.Items.Remove(item3);
            }
            this.contextMenu.InsertItem(num2, groupsMenuItem, "submenu");
            ((DropDownMenuReorderable)groupsMenuItem.DropDown).AddItemsRange(groupItemsList.ToArray(), str);
            this.contextMenu.ResumeLayout();
        }

        protected override void OnMouseEnter(EventArgs e) {
            this.NowMouseHovering = true;
            base.OnMouseEnter(e);
            base.Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e) {
            this.NowMouseHovering = false;
            base.OnMouseLeave(e);
            base.Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e) {
            if(VisualStyleRenderer.IsSupported) {
                if(this.bgRenderer == null) {
                    this.bgRenderer = new VisualStyleRenderer(VisualStyleElement.Taskbar.BackgroundTop.Normal);
                }
                this.bgRenderer.DrawParentBackground(e.Graphics, e.ClipRectangle, this);
            }
            else {
                base.OnPaintBackground(e);
            }
            if(this.NowMouseHovering) {
                System.Drawing.Color baseColor = VisualStyleRenderer.IsSupported ? SystemColors.Window : SystemColors.WindowText;
                if(this.stringFormat == null) {
                    this.stringFormat = new StringFormat();
                    this.stringFormat.Alignment = StringAlignment.Center;
                    this.stringFormat.LineAlignment = StringAlignment.Center;
                    this.stringFormat.FormatFlags = StringFormatFlags.NoWrap;
                }
                using(SolidBrush brush = new SolidBrush(System.Drawing.Color.FromArgb(0x80, baseColor))) {
                    e.Graphics.DrawString("QTTab Desktop Tool", this.Font, brush, new Rectangle(0, 5, e.ClipRectangle.Width - 1, e.ClipRectangle.Height - 6), this.stringFormat);
                }
                using(Pen pen = new Pen(System.Drawing.Color.FromArgb(0x80, baseColor))) {
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
            this.ExpandState[index] = !this.ExpandState[index];
            this.CancelClosing = true;
            switch(index) {
                case 0:
                    groupsMenuItem = this.groupsMenuItem;
                    labelGroupTitle = this.labelGroupTitle;
                    groupItemsList = this.GroupItemsList;
                    str = "groupItem";
                    break;

                case 1:
                    groupsMenuItem = this.historyMenuItem;
                    labelGroupTitle = this.labelHistoryTitle;
                    groupItemsList = this.UndoClosedItemsList;
                    str = "historyItem";
                    break;

                case 2:
                    groupsMenuItem = this.userAppsMenuItem;
                    labelGroupTitle = this.labelUserAppTitle;
                    groupItemsList = this.UserappItemsList;
                    str = "userappItem";
                    break;

                default:
                    groupsMenuItem = this.recentFileMenuItem;
                    labelGroupTitle = this.labelRecentFileTitle;
                    groupItemsList = this.RecentFileItemsList;
                    str = "recentItem";
                    break;
            }
            groupsMenuItem.DropDown.Hide();
            this.contextMenu.SuspendLayout();
            int num2 = this.contextMenu.Items.IndexOf(groupsMenuItem);
            this.contextMenu.Items.Remove(groupsMenuItem);
            this.contextMenu.InsertItem(num2, labelGroupTitle, "label" + index);
            foreach(ToolStripItem item3 in groupItemsList) {
                this.contextMenu.InsertItem(++num2, item3, str);
            }
            this.contextMenu.ResumeLayout();
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
                    if(this.GetTargetWindow(wrapper, true, out ptr, out flag) && !flag) {
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
                    if(this.GetTargetWindow(wrapper, true, out ptr, out flag2)) {
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
                if(this.GetTargetWindow(wrapper, false, out ptr, out flag) && !flag) {
                    SendCOPYDATASTRUCT_IDL(ptr, (IntPtr)4, wrapper.IDL, (IntPtr)((long)keys));
                }
            }
        }

        private void QTCoTaskBarClass_MouseClick(object sender, MouseEventArgs e) {
            if((e.Button == MouseButtons.Left) && ((this.ConfigValues[2] & 0x10) != 0)) {
                this.OnDesktopDblClicked(Control.MousePosition);
            }
        }

        private void QTCoTaskBarClass_MouseDoubleClick(object sender, MouseEventArgs e) {
            if((e.Button == MouseButtons.Left) && ((this.ConfigValues[2] & 0x10) == 0)) {
                this.OnDesktopDblClicked(Control.MousePosition);
            }
        }

        private void ReadSetting() {
            using(RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Quizo\QTTabBar")) {
                if(key != null) {
                    byte[] defaultValue = new byte[4];
                    defaultValue[0] = 1;
                    byte[] buffer = (byte[])key.GetValue("Config_Desktop", defaultValue);
                    this.ConfigValues = buffer;
                    this.Order_Root[0] = (byte)(buffer[0] >> 4);
                    this.Order_Root[1] = (byte)(buffer[0] & 15);
                    this.Order_Root[2] = (byte)((buffer[3] >> 5) & 7);
                    this.ExpandState[0] = (buffer[1] & 0x80) == 0x80;
                    this.ExpandState[1] = (buffer[1] & 0x40) == 0x40;
                    this.ExpandState[2] = (buffer[1] & 0x20) == 0x20;
                    this.ExpandState[3] = (buffer[2] & 2) == 2;
                    this.WidthOfBar = (int)key.GetValue("TaskbarWidth", 80);
                }
            }
        }

        private void RecreateUndoClosedItems() {
            if(this.UndoClosedItemsList.Count > 0) {
                this.ddmrHistory.Items.Clear();
                foreach(ToolStripItem item in this.UndoClosedItemsList) {
                    item.Dispose();
                }
                this.UndoClosedItemsList.Clear();
            }
            if(this.RecentFileItemsList.Count > 0) {
                this.ddmrRecentFile.Items.Clear();
                foreach(ToolStripItem item2 in this.RecentFileItemsList) {
                    item2.Dispose();
                }
            }
            if(((this.ConfigValues[2] & 0x40) == 0) && !QTUtility.CheckConfig(5, 2)) {
                this.UndoClosedItemsList = MenuUtility.CreateUndoClosedItems(null);
            }
            if(((this.ConfigValues[2] & 1) == 0) && !QTUtility.CheckConfig(6, 0x20)) {
                this.RecentFileItemsList = MenuUtility.CreateRecentFilesItems();
            }
        }

        [ComRegisterFunction]
        private static void Register(System.Type t) {
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
            this.ConfigValues = new byte[4];
            this.ConfigValues[0] = (byte)((this.Order_Root[0] << 4) | this.Order_Root[1]);
            this.ConfigValues[1] = (byte)(((this.ExpandState[0] ? 0x80 : 0) | (this.ExpandState[1] ? 0x40 : 0)) | (this.ExpandState[2] ? 0x20 : 0));
            this.ConfigValues[1] = (byte)(this.ConfigValues[1] | (this.tsmiTaskBar.Checked ? ((byte)0) : ((byte)8)));
            this.ConfigValues[1] = (byte)(this.ConfigValues[1] | (this.tsmiDesktop.Checked ? ((byte)0) : ((byte)4)));
            this.ConfigValues[1] = (byte)(this.ConfigValues[1] | (this.tsmiLockItems.Checked ? ((byte)2) : ((byte)0)));
            this.ConfigValues[1] = (byte)(this.ConfigValues[1] | (this.tsmiVSTitle.Checked ? ((byte)0) : ((byte)1)));
            this.ConfigValues[2] = (byte)(this.ConfigValues[2] | (this.tsmiOnGroup.Checked ? ((byte)0) : ((byte)0x80)));
            this.ConfigValues[2] = (byte)(this.ConfigValues[2] | (this.tsmiOnHistory.Checked ? ((byte)0) : ((byte)0x40)));
            this.ConfigValues[2] = (byte)(this.ConfigValues[2] | (this.tsmiOnUserApps.Checked ? ((byte)0) : ((byte)0x20)));
            this.ConfigValues[2] = (byte)(this.ConfigValues[2] | (this.tsmiOnRecentFile.Checked ? ((byte)0) : ((byte)1)));
            this.ConfigValues[2] = (byte)(this.ConfigValues[2] | (this.tsmiOneClick.Checked ? ((byte)0x10) : ((byte)0)));
            this.ConfigValues[2] = (byte)(this.ConfigValues[2] | (this.tsmiAppKeys.Checked ? ((byte)0) : ((byte)8)));
            this.ConfigValues[2] = (byte)(this.ConfigValues[2] | (this.ExpandState[3] ? ((byte)2) : ((byte)0)));
            this.ConfigValues[3] = (byte)(this.ConfigValues[3] | ((byte)(this.Order_Root[2] << 5)));
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar")) {
                if(key != null) {
                    key.SetValue("Config_Desktop", this.ConfigValues);
                    key.SetValue("TaskBarWidth", base.Width);
                }
            }
        }

        private static IntPtr SendCOPYDATASTRUCT_IDL(IntPtr hWnd, IntPtr wParam, byte[] idl, IntPtr dwData) {
            if((idl == null) || (idl.Length == 0)) {
                return (IntPtr)(-1);
            }
            QTTabBarLib.Interop.COPYDATASTRUCT structure = new QTTabBarLib.Interop.COPYDATASTRUCT();
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
            if(base.BandObjectSite != null) {
                Marshal.ReleaseComObject(base.BandObjectSite);
            }
            base.BandObjectSite = (IInputObjectSite)pUnkSite;
            Application.EnableVisualStyles();
            if(QTUtility.NowDebugging) {
                Control.CheckForIllegalCrossThreadCalls = true;
            }
            this.ReadSetting();
            this.InitializeComponent();
            TitleMenuItem.DrawBackground = this.tsmiVSTitle.Checked;
            Myself = this;
        }

        private bool shellViewControler_MessageCaptured(ref Message msg) {
            QTTabBarLib.Interop.NMHDR nmhdr;
            if(msg.Msg == 0x4e) {
                nmhdr = (QTTabBarLib.Interop.NMHDR)Marshal.PtrToStructure(msg.LParam, typeof(QTTabBarLib.Interop.NMHDR));
                if(nmhdr.hwndFrom != this.shellViewControler.OptionalHandle) {
                    return false;
                }
                if(this.folderView == null) {
                    return false;
                }
                switch(nmhdr.code) {
                    case -103:
                        if(!QTUtility.CheckConfig(9, 0x40)) {
                            this.HideSubDirTip();
                        }
                        goto Label_057E;

                    case -102:
                        goto Label_057E;

                    case -101:
                        if(!QTUtility.CheckConfig(9, 0x40)) {
                            QTTabBarLib.Interop.NMLISTVIEW nmlistview = (QTTabBarLib.Interop.NMLISTVIEW)Marshal.PtrToStructure(msg.LParam, typeof(QTTabBarLib.Interop.NMLISTVIEW));
                            if(nmlistview.uChanged == 8) {
                                uint num = nmlistview.uNewState & 8;
                                uint num2 = nmlistview.uOldState & 8;
                                if((nmlistview.iItem != this.itemIndexDROPHILITED) && (num2 != num)) {
                                    if(num != 0) {
                                        this.HandleDROPHILITED(nmlistview.iItem, nmhdr.hwndFrom);
                                    }
                                    else {
                                        this.HandleDROPHILITED(-1, IntPtr.Zero);
                                    }
                                }
                            }
                        }
                        goto Label_057E;

                    case -8:
                        this.HideThumbnailTooltip();
                        this.HideSubDirTip_DesktopInactivated();
                        goto Label_057E;

                    case -114: {
                            NMITEMACTIVATE nmitemactivate = (NMITEMACTIVATE)Marshal.PtrToStructure(msg.LParam, typeof(NMITEMACTIVATE));
                            bool fEnqExec = !QTUtility.CheckConfig(6, 0x20);
                            Keys modKey = ((((nmitemactivate.uKeyFlags & 1) == 1) ? Keys.Alt : Keys.None) | (((nmitemactivate.uKeyFlags & 2) == 2) ? Keys.Control : Keys.None)) | (((nmitemactivate.uKeyFlags & 4) == 4) ? Keys.Shift : Keys.None);
                            return this.HandleTabFolderActions(-1, modKey, fEnqExec);
                        }
                    case -175:
                        if(!QTUtility.IsVista && !QTUtility.CheckConfig(8, 0x20)) {
                            this.shellViewControler.DefWndProc(ref msg);
                            if(msg.Result == IntPtr.Zero) {
                                QTTabBarLib.Interop.NMLVDISPINFO nmlvdispinfo = (QTTabBarLib.Interop.NMLVDISPINFO)Marshal.PtrToStructure(msg.LParam, typeof(QTTabBarLib.Interop.NMLVDISPINFO));
                                QTTabBarClass.HandleRenaming(this.hwndListView, nmlvdispinfo.item.lParam, this);
                            }
                        }
                        goto Label_057E;

                    case -158: {
                            if(!QTUtility.CheckConfig(8, 2) || !(!QTUtility.CheckConfig(8, 1) ^ (Control.ModifierKeys == Keys.Shift))) {
                                goto Label_057E;
                            }
                            QTTabBarLib.Interop.NMLVGETINFOTIP nmlvgetinfotip = (QTTabBarLib.Interop.NMLVGETINFOTIP)Marshal.PtrToStructure(msg.LParam, typeof(QTTabBarLib.Interop.NMLVGETINFOTIP));
                            IntPtr zero = IntPtr.Zero;
                            try {
                                if((this.folderView.Item(nmlvgetinfotip.iItem, out zero) != 0) || !(zero != IntPtr.Zero)) {
                                    goto Label_057E;
                                }
                                if(((this.thumbnailTooltip != null) && this.thumbnailTooltip.IsShowing) && (nmlvgetinfotip.iItem == this.thumbnailIndex)) {
                                    return true;
                                }
                                if((this.timer_HoverThumbnail != null) && this.timer_HoverThumbnail.Enabled) {
                                    return true;
                                }
                                QTTabBarLib.Interop.RECT lprc = QTTabBarClass.GetLVITEMRECT(nmhdr.hwndFrom, nmlvgetinfotip.iItem, false, 0);
                                Point mousePosition = Control.MousePosition;
                                return this.ShowThumbnailTooltip(zero, nmlvgetinfotip.iItem, nmhdr.hwndFrom, !PInvoke.PtInRect(ref lprc, new BandObjectLib.POINT(mousePosition)));
                            }
                            finally {
                                if(zero != IntPtr.Zero) {
                                    PInvoke.CoTaskMemFree(zero);
                                }
                            }
                            goto Label_0264;
                        }
                    case -121:
                        goto Label_0264;
                }
            }
            goto Label_057E;
        Label_0264:
            if(QTUtility.CheckConfig(8, 2) || !QTUtility.CheckConfig(9, 0x40)) {
                QTTabBarLib.Interop.NMLISTVIEW nmlistview2 = (QTTabBarLib.Interop.NMLISTVIEW)Marshal.PtrToStructure(msg.LParam, typeof(QTTabBarLib.Interop.NMLISTVIEW));
                Keys modifierKeys = Control.ModifierKeys;
                if(QTUtility.CheckConfig(8, 2)) {
                    if(this.timer_HoverThumbnail == null) {
                        this.timer_HoverThumbnail = new System.Windows.Forms.Timer(this.components);
                        this.timer_HoverThumbnail.Interval = (int)(SystemInformation.MouseHoverTime * 0.2);
                        this.timer_HoverThumbnail.Tick += new EventHandler(this.timer_HoverThumbnail_Tick);
                    }
                    this.timer_HoverThumbnail.Enabled = false;
                    this.timer_HoverThumbnail.Enabled = true;
                    if(((this.thumbnailTooltip != null) && (this.thumbnailTooltip.IsShowing || this.fThumbnailPending)) && (nmlistview2.iItem != this.thumbnailIndex)) {
                        if(!QTUtility.CheckConfig(8, 1) ^ (modifierKeys == Keys.Shift)) {
                            if(nmlistview2.iItem > -1) {
                                IntPtr ppidl = IntPtr.Zero;
                                try {
                                    if(((this.folderView.Item(nmlistview2.iItem, out ppidl) == 0) && (ppidl != IntPtr.Zero)) && this.ShowThumbnailTooltip(ppidl, nmlistview2.iItem, nmhdr.hwndFrom, false)) {
                                        return false;
                                    }
                                }
                                finally {
                                    if(ppidl != IntPtr.Zero) {
                                        PInvoke.CoTaskMemFree(ppidl);
                                    }
                                }
                            }
                            this.HideThumbnailTooltip();
                        }
                        else {
                            this.HideThumbnailTooltip();
                        }
                    }
                }
                if(!QTUtility.CheckConfig(9, 0x40)) {
                    if((!QTUtility.CheckConfig(9, 0x20) ^ (modifierKeys == Keys.Shift)) && (nmlistview2.iItem > -1)) {
                        IntPtr ptr3 = IntPtr.Zero;
                        try {
                            if(((this.folderView.Item(nmlistview2.iItem, out ptr3) == 0) && (ptr3 != IntPtr.Zero)) && this.ShowSubDirTip(ptr3, nmlistview2.iItem, nmhdr.hwndFrom, false)) {
                                return false;
                            }
                        }
                        finally {
                            if(ptr3 != IntPtr.Zero) {
                                PInvoke.CoTaskMemFree(ptr3);
                            }
                        }
                    }
                    this.HideSubDirTip();
                }
            }
        Label_057E:
            return false;
        }

        private bool ShowSubDirTip(IntPtr pIDL, int iItem, IntPtr hwndListView, bool fSkipFocusCheck) {
            if(fSkipFocusCheck || (this.hwndListView == PInvoke.GetFocus())) {
                try {
                    string displayName = ShellMethods.GetDisplayName(pIDL, false);
                    if(!QTTabBarClass.TryMakeSubDirTipPath(ref displayName)) {
                        return false;
                    }
                    int pViewMode = 1;
                    this.folderView.GetCurrentViewMode(ref pViewMode);
                    QTTabBarLib.Interop.RECT rect = QTTabBarClass.GetLVITEMRECT(hwndListView, iItem, true, pViewMode);
                    Point pnt = new Point(rect.right - 0x10, rect.bottom - 0x10);
                    if(this.subDirTip == null) {
                        this.subDirTip = new SubDirTipForm(this.ThisHandle, this.hwndListView, false);
                        this.subDirTip.MenuItemClicked += new ToolStripItemClickedEventHandler(this.subDirTip_MenuItemClicked);
                        this.subDirTip.MultipleMenuItemsClicked += new EventHandler(this.subDirTip_MultipleMenuItemsClicked);
                        this.subDirTip.MenuItemRightClicked += new ItemRightClickedEventHandler(this.subDirTip_MenuItemRightClicked);
                        this.subDirTip.MultipleMenuItemsRightClicked += new ItemRightClickedEventHandler(this.subDirTip_MultipleMenuItemsRightClicked);
                    }
                    byte[] idl = null;
                    if(QTUtility.IsVista && string.Equals(displayName, IDLWrapper.PATH_USERSFILES, StringComparison.OrdinalIgnoreCase)) {
                        idl = ShellMethods.GetIDLData(pIDL);
                    }
                    this.subDirTip.ShowSubDirTip(displayName, idl, pnt, hwndListView);
                    return true;
                }
                catch(Exception exception) {
                    QTUtility2.MakeErrorLog(exception, null);
                }
            }
            return false;
        }

        private bool ShowThumbnailTooltip(IntPtr pIDL, int iItem, IntPtr hwndListView, bool fKey) {
            StringBuilder pszPath = new StringBuilder(260);
            if(PInvoke.SHGetPathFromIDList(pIDL, pszPath)) {
                string path = pszPath.ToString();
                if(File.Exists(path)) {
                    if((path.StartsWith("::") || path.StartsWith(@"\\")) || path.ToLower().StartsWith(@"a:\")) {
                        return false;
                    }
                    string ext = Path.GetExtension(path).ToLower();
                    if(ext == ".lnk") {
                        path = ShellMethods.GetLinkTargetPath(path);
                        if(path.Length == 0) {
                            return false;
                        }
                        ext = Path.GetExtension(path).ToLower();
                    }
                    if(ThumbnailTooltipForm.ExtIsSupported(ext)) {
                        if(this.thumbnailTooltip == null) {
                            this.thumbnailTooltip = new ThumbnailTooltipForm();
                            this.thumbnailTooltip.ThumbnailVisibleChanged += new QEventHandler(this.thumbnailTooltip_ThumbnailVisibleChanged);
                            this.timer_Thumbnail = new System.Windows.Forms.Timer(this.components);
                            this.timer_Thumbnail.Interval = 400;
                            this.timer_Thumbnail.Tick += new EventHandler(this.timer_Thumbnail_Tick);
                        }
                        if(this.thumbnailTooltip.IsShownByKey && !fKey) {
                            this.thumbnailTooltip.IsShownByKey = false;
                            return true;
                        }
                        this.thumbnailIndex = iItem;
                        this.thumbnailTooltip.IsShownByKey = fKey;
                        QTTabBarLib.Interop.RECT rect = QTTabBarClass.GetLVITEMRECT(hwndListView, iItem, false, 0);
                        return this.thumbnailTooltip.ShowToolTip(path, new Point(rect.right - 0x10, rect.bottom - 8));
                    }
                }
            }
            this.HideThumbnailTooltip();
            return false;
        }

        private void subDirTip_MenuItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            QMenuItem clickedItem = (QMenuItem)e.ClickedItem;
            if(clickedItem.Target == MenuTarget.Folder) {
                if(clickedItem.IDLData == null) {
                    this.OpenTab(new object[] { clickedItem.TargetPath, Control.ModifierKeys });
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
                if(!QTUtility.CheckConfig(6, 0x20)) {
                    QTUtility.ExecutedPathsList.Add(path);
                }
            }
            catch {
            }
        }

        private void subDirTip_MenuItemRightClicked(object sender, ItemRightClickedEventArgs e) {
            using(IDLWrapper wrapper = new IDLWrapper(((QMenuItem)e.ClickedItem).Path)) {
                e.HRESULT = ShellMethods.PopUpSystemContextMenu(wrapper, e.IsKey ? e.Point : Control.MousePosition, ref iContextMenu2, this.subDirTip.Handle, false);
            }
        }

        private void subDirTip_MultipleMenuItemsClicked(object sender, EventArgs e) {
            List<string> executedDirectories = this.subDirTip.ExecutedDirectories;
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
                Keys modifierKeys = Control.ModifierKeys;
                if(!QTUtility.CheckConfig(0, 0x80)) {
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
                    this.OpenTab(objArray);
                }
                else if(list2.Count > 1) {
                    byte[] buffer = list2[0];
                    list2.RemoveAt(0);
                    Thread thread = new Thread(new ParameterizedThreadStart(this.OpenFolders2));
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.IsBackground = true;
                    thread.Start(new object[] { buffer, list2, modifierKeys });
                }
            }
        }

        private void subDirTip_MultipleMenuItemsRightClicked(object sender, ItemRightClickedEventArgs e) {
            List<string> executedDirectories = this.subDirTip.ExecutedDirectories;
            e.HRESULT = ShellMethods.PopUpSystemContextMenu(executedDirectories, e.IsKey ? e.Point : Control.MousePosition, ref iContextMenu2, this.subDirTip.Handle);
        }

        private static void subMenuItems_DoubleClick(object sender, EventArgs e) {
            string path = ((DirectoryMenuItem)sender).Path;
            if(Directory.Exists(path)) {
                try {
                    if(Myself != null) {
                        object obj2 = new object[] { path, Control.ModifierKeys };
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

        private void thumbnailTooltip_ThumbnailVisibleChanged(object sender, QEventArgs e) {
            this.timer_Thumbnail.Enabled = false;
            if(e.Direction == ArrowDirection.Up) {
                this.fThumbnailPending = false;
            }
            else {
                this.fThumbnailPending = true;
                this.timer_Thumbnail.Enabled = true;
            }
        }

        private void timer_HoverSubDirTipMenu_Tick(object sender, EventArgs e) {
            this.timer_HoverSubDirTipMenu.Enabled = false;
            int itemIndexDROPHILITED = this.itemIndexDROPHILITED;
            if(Control.MouseButtons != MouseButtons.None) {
                IntPtr ptr2;
                IntPtr tag = (IntPtr)this.timer_HoverSubDirTipMenu.Tag;
                Point mousePosition = Control.MousePosition;
                PInvoke.MapWindowPoints(IntPtr.Zero, tag, ref mousePosition, 1);
                if(((itemIndexDROPHILITED == PInvoke.ListView_HitTest(tag, QTUtility2.Make_LPARAM(mousePosition.X, mousePosition.Y))) && (this.folderView.Item(itemIndexDROPHILITED, out ptr2) == 0)) && (ptr2 != IntPtr.Zero)) {
                    using(IDLWrapper wrapper = new IDLWrapper(ptr2)) {
                        if(this.subDirTip != null) {
                            this.subDirTip.HideMenu();
                        }
                        if(!string.Equals(wrapper.Path, "::{645FF040-5081-101B-9F08-00AA002F954E}", StringComparison.OrdinalIgnoreCase) && this.ShowSubDirTip(wrapper.PIDL, itemIndexDROPHILITED, tag, true)) {
                            this.itemIndexDROPHILITED = itemIndexDROPHILITED;
                            PInvoke.SetFocus(tag);
                            PInvoke.SetForegroundWindow(tag);
                            this.HideThumbnailTooltip();
                            this.subDirTip.ShowMenu();
                            return;
                        }
                    }
                }
                if((this.subDirTip != null) && this.subDirTip.IsMouseOnMenus) {
                    this.itemIndexDROPHILITED = -1;
                    return;
                }
            }
            this.HideSubDirTip();
        }

        private void timer_HoverThumbnail_Tick(object sender, EventArgs e) {
            this.timer_HoverThumbnail.Enabled = false;
        }

        private void timer_Thumbnail_Tick(object sender, EventArgs e) {
            this.timer_Thumbnail.Enabled = false;
            this.fThumbnailPending = false;
        }

        private void timerHooks_Tick(object sender, EventArgs e) {
            if(++this.iHookTimeout > 5) {
                this.timerHooks.Stop();
                MessageBox.Show("Failed to hook Desktop. Please re-enable QT Tab Desktop tool.");
            }
            else {
                this.InstallDesktopHook();
            }
        }

        [ComUnregisterFunction]
        private static void Unregister(System.Type t) {
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
            QTUtility.RefreshUserappMenuesOnReorderFinished(this.userAppsMenuItem.DropDownItems);
            QTUtility.fRequiredRefresh_App = true;
        }

        protected override void WndProc(ref Message m) {
            if(m.Msg != 0x4a) {
                if(((m.Msg == 0x117) || (m.Msg == 0x2b)) || (m.Msg == 0x2c)) {
                    uint num3;
                    uint num4;
                    if(this.fContextmenuedOnMain_Grp) {
                        base.WndProc(ref m);
                        return;
                    }
                    if((iContextMenu2 != null) && (m.HWnd == base.Handle)) {
                        try {
                            iContextMenu2.HandleMenuMsg(m.Msg, m.WParam, m.LParam);
                        }
                        catch {
                        }
                        return;
                    }
                    int windowThreadProcessId = PInvoke.GetWindowThreadProcessId(m.WParam, out num3);
                    int num6 = PInvoke.GetWindowThreadProcessId(base.Handle, out num4);
                    if(windowThreadProcessId != num6) {
                        return;
                    }
                }
                else if(((m.Msg == 0x21) && ((this.ConfigValues[2] & 0x10) != 0)) && ((((((int)((long)m.LParam)) >> 0x10) & 0xffff) == 0x201) && this.contextMenu.Visible)) {
                    this.contextMenu.Close(ToolStripDropDownCloseReason.AppClicked);
                    m.Result = (IntPtr)4;
                    return;
                }
            }
            else {
                QTTabBarLib.Interop.COPYDATASTRUCT copydatastruct = (QTTabBarLib.Interop.COPYDATASTRUCT)Marshal.PtrToStructure(m.LParam, typeof(QTTabBarLib.Interop.COPYDATASTRUCT));
                switch(((int)m.WParam)) {
                    case 0: {
                            string[] strArray = Marshal.PtrToStringAuto(copydatastruct.lpData).Split(QTUtility.SEPARATOR_CHAR);
                            if((strArray.Length > 1) && (strArray[1].Length > 0)) {
                                QTUtility.PathToSelectInCommandLineArg = strArray[1];
                            }
                            QTUtility.fExplorerPrevented = true;
                            try {
                                this.BlockedExplorerURL = strArray[0];
                                this.BlockedExplorerProcess = Process.GetProcessById((int)copydatastruct.dwData);
                                this.BlockedExplorerProcess.EnableRaisingEvents = true;
                                this.BlockedExplorerProcess.Exited += new EventHandler(this.BlockedExplorer_Exited);
                            }
                            catch(Exception exception) {
                                QTUtility2.MakeErrorLog(exception, null);
                            }
                            break;
                        }
                    case 3:
                        this.fRequiredRefresh_MenuItems = true;
                        break;

                    case 0xff: {
                            int x = QTUtility2.GET_X_LPARAM(copydatastruct.dwData);
                            int y = QTUtility2.GET_Y_LPARAM(copydatastruct.dwData);
                            PInvoke.SetForegroundWindow(this.hwndShellTray);
                            this.OnDesktopDblClicked(new Point(x, y));
                            break;
                        }
                }
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
                this.bmpArrow_Opn = Resources_Image.menuOpen;
                this.bmpArrow_Cls = Resources_Image.menuClose;
                if(sf == null) {
                    Init();
                }
            }

            protected override void Dispose(bool disposing) {
                if(this.bmpArrow_Opn != null) {
                    this.bmpArrow_Opn.Dispose();
                    this.bmpArrow_Opn = null;
                }
                if(this.bmpArrow_Cls != null) {
                    this.bmpArrow_Cls.Dispose();
                    this.bmpArrow_Cls = null;
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
                    Rectangle rect = new Rectangle(1, 0, this.Bounds.Width, this.Bounds.Height);
                    e.Graphics.DrawImage(bmpTitle, new Rectangle(new Point(1, 0), new Size(1, this.Bounds.Height)), new Rectangle(Point.Empty, new Size(1, 0x18)), GraphicsUnit.Pixel);
                    e.Graphics.DrawImage(bmpTitle, new Rectangle(new Point(2, 0), new Size(this.Bounds.Width - 3, 1)), new Rectangle(new Point(1, 0), new Size(0x62, 1)), GraphicsUnit.Pixel);
                    e.Graphics.DrawImage(bmpTitle, new Rectangle(new Point(this.Bounds.Width - 1, 0), new Size(1, this.Bounds.Height)), new Rectangle(new Point(0x63, 0), new Size(1, 0x18)), GraphicsUnit.Pixel);
                    e.Graphics.DrawImage(bmpTitle, new Rectangle(new Point(2, this.Bounds.Height - 1), new Size(this.Bounds.Width - 3, 1)), new Rectangle(new Point(1, 0x17), new Size(0x62, 1)), GraphicsUnit.Pixel);
                    e.Graphics.DrawImage(bmpTitle, new Rectangle(new Point(2, 1), new Size(this.Bounds.Width - 3, this.Bounds.Height - 2)), new Rectangle(new Point(1, 1), new Size(0x62, 0x16)), GraphicsUnit.Pixel);
                    if(this.Selected) {
                        SolidBrush brush = new SolidBrush(System.Drawing.Color.FromArgb(0x60, SystemColors.Highlight));
                        e.Graphics.FillRectangle(brush, rect);
                        brush.Dispose();
                    }
                    if(this.HasDropDownItems) {
                        int y = (rect.Height - 0x10) / 2;
                        if(y < 0) {
                            y = 5;
                        }
                        else {
                            y += 5;
                        }
                        using(SolidBrush brush2 = new SolidBrush(System.Drawing.Color.FromArgb(this.Selected ? 0xff : 0x80, System.Drawing.Color.White))) {
                            Point point = new Point(rect.Width - 15, y);
                            Point[] points = new Point[] { point, new Point(point.X, point.Y + 8), new Point(point.X + 4, point.Y + 4) };
                            e.Graphics.FillPolygon(brush2, points);
                        }
                    }
                    e.Graphics.DrawString(this.Text, this.Font, Brushes.White, new RectangleF(34f, 2f, (float)(rect.Width - 0x22), (float)(rect.Height - 2)), sf);
                }
                else {
                    base.OnPaint(e);
                }
                e.Graphics.DrawImage(this.fOpened ? this.bmpArrow_Cls : this.bmpArrow_Opn, new Rectangle(5, 4, 0x10, 0x10));
            }

            public static bool DrawBackground {
                set {
                    drawBackground = value;
                }
            }

            public MenuGenre Genre {
                get {
                    return this.genre;
                }
            }

            public bool IsOpened {
                get {
                    return this.fOpened;
                }
            }
        }
    }
}
