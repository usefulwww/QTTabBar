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
    using SHDocVw;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Media;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Messaging;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;

    [ComVisible(true), Guid("d2bf470e-ed1c-487f-a333-2bd8835eb6ce")]
    public sealed class QTTabBarClass : BandObject {
        private int BandHeight;
        private VisualStyleRenderer bgRenderer;
        private Bitmap bmpRebar;
        private ToolStripButton buttonBack;
        private ToolStripButton buttonForward;
        private ToolStripDropDownButton buttonNavHistoryMenu;
        private IContainer components;
        private ContextMenuStripEx contextMenuDropped;
        private QTabItem ContextMenuedTab;
        private static ContextMenuStripEx contextMenuNotifyIcon;
        private ContextMenuStripEx contextMenuSys;
        private ContextMenuStripEx contextMenuTab;
        private string CurrentAddress;
        private QTabItem CurrentTab;
        private int CurrentTravelLogIndex;
        private Cursor curTabCloning;
        private Cursor curTabDrag;
        private static Dictionary<IntPtr, int> dicNotifyIcon;
        private Rectangle DraggingDestRect;
        private QTabItem DraggingTab;
        private DropTargetWrapper dropTargetWrapper;
        private NativeWindowController explorerController;
        private IntPtr ExplorerHandle;
        private bool fDrivesContainedDD;
        private bool fIFolderViewNotImplemented;
        private static bool fInitialized;
        private volatile bool FirstNavigationCompleted;
        private bool fNavigatedByTabSelection;
        private bool fNowInTray;
        private bool fNowQuitting;
        private bool fNowRestoring;
        private bool fNowTravelByTree;
        private bool fOptionDialogCreated;
        private bool fThumbnailPending;
        private bool fToggleTabMenu;
        private IntPtr hHook_Key;
        private IntPtr hHook_Mouse;
        private IntPtr hHook_Msg;
        private HookProc hookProc_GetMsg;
        private HookProc hookProc_Key;
        private HookProc hookProc_Mouse;
        private IntPtr hwndDirectUI;
        private static IntPtr hwndNotifyIconParent;
        private IntPtr hwndSeachBand_Edit;
        private IntPtr hwndSearchBand;
        private IntPtr hwndSysListView32;
        private IntPtr hwndTravelBand;
        private static Icon icoNotify;
        private IContextMenu2 iContextMenu2;
        private int iModKeyStateDD;
        private const int INTERVAL_SELCTTAB = 700;
        internal const int INTERVAL_SHOWMENU = 0x4b0;
        private int iSequential_WM_CLOSE;
        private bool IsShown;
        private int itemIndexDROPHILITED = -1;
        private Dictionary<int, ITravelLogEntry> LogEntryDic = new Dictionary<int, ITravelLogEntry>();
        private ListViewWrapper listViewWrapper;
        private List<QTabItem> lstActivatedTabs = new List<QTabItem>(0x10);
        private List<ToolStripItem> lstPluginMenuItems_Sys;
        private List<ToolStripItem> lstPluginMenuItems_Tab;
        private static List<PluginAssembly> lstTempAssemblies_Refresh;
        private static FileHashComputerForm md5Form;
        private ToolStripTextBox menuTextBoxTabAlias;
        private int navBtnsFlag;
        private bool NavigatedByCode;
        private static NotifyIcon notifyIcon;
        private bool NowNavigating;
        private bool NowInTravelLog;
        private bool NowModalDialogShown;
        private bool NowOpenedByGroupOpener;
        private bool NowTabCloned;
        private bool NowTabCreated;
        private bool NowTabDragging;
        private bool NowTabsAddingRemoving;
        private bool NowTopMost;
        private static OptionsDialog optionsDialog;
        private PluginManager pluginManager;
        private NativeWindowController rebarController;
        private IShellBrowser ShellBrowser;
        private NativeWindowController shellViewController;
        private string strDraggingDrive;
        private string strDraggingStartPath;
        private int subDirIndex = -1;
        private SubDirTipForm subDirTip;
        private SubDirTipForm subDirTip_Tab;
        private static object syncObj_NotifyIcon = new object();
        private QTabControl tabControl1;
        private QTabItem tabForDD;
        private TabSwitchForm tabSwitcher;
        private TextureBrush textureBrushRebar;
        private int thumbnailIndex;
        private ThumbnailTooltipForm thumbnailTooltip;
        private System.Windows.Forms.Timer timer_HoverSubDirTipMenu;
        private System.Windows.Forms.Timer timer_HoverThumbnail;
        private System.Windows.Forms.Timer timer_Thumbnail;
        private System.Windows.Forms.Timer timerOnTab;
        private System.Windows.Forms.Timer timerSelectionChanged;
        private static bool TMP_fPaintBG;
        private static string TMP_strOldLangPath;
        private static bool TMPfNowOptionDialogOpening;
        private ToolStripEx toolStrip;
        private System.Windows.Forms.ToolTip toolTipForDD;
        private NativeWindowController travelBtnController;
        private ITravelLogStg TravelLog;
        private IntPtr TravelToolBarHandle;
        private ToolStripMenuItem tsmiAddToGroup;
        private ToolStripMenuItem tsmiBrowseFolder;
        private ToolStripMenuItem tsmiCloneThis;
        private ToolStripMenuItem tsmiClose;
        private ToolStripMenuItem tsmiCloseAllButCurrent;
        private ToolStripMenuItem tsmiCloseAllButThis;
        private ToolStripMenuItem tsmiCloseLeft;
        private ToolStripMenuItem tsmiCloseRight;
        private ToolStripMenuItem tsmiCloseWindow;
        private ToolStripMenuItem tsmiCopy;
        private ToolStripMenuItem tsmiCreateGroup;
        private ToolStripMenuItem tsmiCreateWindow;
        private ToolStripMenuItem tsmiExecuted;
        private ToolStripMenuItem tsmiGroups;
        private ToolStripMenuItem tsmiHistory;
        private ToolStripMenuItem tsmiLastActiv;
        private ToolStripMenuItem tsmiLockThis;
        private ToolStripMenuItem tsmiLockToolbar;
        private ToolStripMenuItem tsmiMergeWindows;
        private ToolStripMenuItem tsmiOption;
        private ToolStripMenuItem tsmiProp;
        private ToolStripMenuItem tsmiTabOrder;
        private ToolStripMenuItem tsmiUndoClose;
        private ToolStripSeparator tssep_Sys1;
        private ToolStripSeparator tssep_Sys2;
        private ToolStripSeparator tssep_Tab1;
        private ToolStripSeparator tssep_Tab2;
        private ToolStripSeparator tssep_Tab3;

        public QTTabBarClass() {
            if(!fInitialized) {
                InitializeStaticFields();
            }
            QTUtility.InstancesCount++;
            this.BandHeight = QTUtility.TabHeight + 2;
            this.InitializeComponent();
            this.lstActivatedTabs.Add(this.CurrentTab);
        }

        private void AddInsertTab(QTabItem tab) {
            switch(QTUtility.ConfigValues[1]) {
                case 1:
                    this.tabControl1.TabPages.Insert(0, tab);
                    return;

                case 2:
                case 3: {
                        int index = this.tabControl1.TabPages.IndexOf(this.CurrentTab);
                        if(index == -1) {
                            this.tabControl1.TabPages.Add(tab);
                            return;
                        }
                        this.tabControl1.TabPages.Insert((QTUtility.ConfigValues[1] == 2) ? (index + 1) : index, tab);
                        return;
                    }
            }
            this.tabControl1.TabPages.Add(tab);
        }

        private void AddStartUpTabs(string openingGRP, string openingPath) {
            if(Control.ModifierKeys != Keys.Shift) {
                if(QTUtility.StartUpGroupList.Count > 0) {
                    bool flag = QTUtility.CheckConfig(Settings.DontOpenSame);
                    foreach(string str in QTUtility.StartUpGroupList) {
                        string str2;
                        if((openingGRP != str) && QTUtility.GroupPathsDic.TryGetValue(str, out str2)) {
                            if(QTUtility.StartUpGroupNameNowOpening == str) {
                                QTUtility.StartUpGroupNameNowOpening = string.Empty;
                            }
                            else {
                                foreach(string str3 in str2.Split(QTUtility.SEPARATOR_CHAR)) {
                                    if(flag) {
                                        if(string.Equals(str3, openingPath, StringComparison.OrdinalIgnoreCase)) {
                                            this.tabControl1.TabPages.Swap(0, this.tabControl1.TabCount - 1);
                                            goto Label_0188;
                                        }
                                        bool flag2 = false;
                                        foreach(QTabItem item in this.tabControl1.TabPages) {
                                            if(string.Equals(str3, item.CurrentPath, StringComparison.OrdinalIgnoreCase)) {
                                                flag2 = true;
                                                break;
                                            }
                                        }
                                        if(flag2) {
                                            goto Label_0188;
                                        }
                                    }
                                    using(IDLWrapper wrapper = new IDLWrapper(str3)) {
                                        if(wrapper.Available) {
                                            QTabItem tabPage = new QTabItem(QTUtility2.MakePathDisplayText(str3, false), str3, this.tabControl1);
                                            tabPage.NavigatedTo(str3, wrapper.IDL, -1);
                                            tabPage.ToolTipText = QTUtility2.MakePathDisplayText(str3, true);
                                            tabPage.UnderLine = true;
                                            this.tabControl1.TabPages.Add(tabPage);
                                        }
                                    }
                                Label_0188: ;
                                }
                            }
                        }
                    }
                }
                if(QTUtility.CheckConfig(Settings.RestoreLockedTabs)) {
                    this.RestoreTabsOnInitialize(1, openingPath);
                }
                else if(QTUtility.CheckConfig(Settings.RestoreClosed)) {
                    this.RestoreTabsOnInitialize(0, openingPath);
                }
            }
        }

        private static void AddToHistory(QTabItem closingTab) {
            string currentPath = closingTab.CurrentPath;
            if((!QTUtility.CheckConfig(Settings.NoHistory) && !string.IsNullOrEmpty(currentPath)) && !IsSearchResultFolder(currentPath)) {
                if(QTUtility2.IsShellPathButNotFileSystem(currentPath) && (currentPath.IndexOf("???") == -1)) {
                    currentPath = currentPath + "???" + closingTab.GetLogHash(true, 0);
                }
                QTUtility.ClosedTabHistoryList.Add(currentPath);
                SyncButtonBarBroadCast(2);
            }
        }

        private void AppendUserApps(List<string> listDroppedPaths) {
            WindowUtils.BringExplorerToFront(this.ExplorerHandle);
            if(this.contextMenuDropped == null) {
                ToolStripMenuItem item = new ToolStripMenuItem();
                item.Tag = 1;
                this.contextMenuDropped = new ContextMenuStripEx(this.components, false);
                this.contextMenuDropped.SuspendLayout();
                this.contextMenuDropped.Items.Add(item);
                this.contextMenuDropped.Items.Add(new ToolStripMenuItem());
                this.contextMenuDropped.ShowImageMargin = false;
                this.contextMenuDropped.ItemClicked += new ToolStripItemClickedEventHandler(this.contextMenuDropped_ItemClicked);
                this.contextMenuDropped.ResumeLayout(false);
            }
            string str = QTUtility.ResMain[0x15];
            str = str + ((listDroppedPaths.Count > 1) ? (listDroppedPaths.Count + QTUtility.ResMain[0x16]) : ("\"" + Path.GetFileName(listDroppedPaths[0]) + "\""));
            this.contextMenuDropped.SuspendLayout();
            this.contextMenuDropped.Items[0].Text = str;
            this.contextMenuDropped.Items[1].Text = QTUtility.ResMain[0x17];
            this.contextMenuDropped.Tag = listDroppedPaths;
            this.contextMenuDropped.ResumeLayout();
            this.contextMenuDropped.Show(Control.MousePosition);
        }

        private void AsyncComplete(IAsyncResult ar) {
            AsyncResult result = (AsyncResult)ar;
            ((MethodInvoker)result.AsyncDelegate).EndInvoke(ar);
            if(base.IsHandleCreated) {
                base.Invoke(new NavigationCompleteCallback(this.CallBackDoOpenGroups), new object[] { result.AsyncState, IntPtr.Zero });
            }
        }

        private void AsyncComplete_ButtonBarPlugin(IAsyncResult ar) {
            AsyncResult result = (AsyncResult)ar;
            ((WaitTimeoutCallback)result.AsyncDelegate).EndInvoke(ar);
            if(base.IsHandleCreated) {
                base.Invoke(new MethodInvoker(this.CallbackPlugin));
            }
        }

        private void AsyncComplete_FolderTree(IAsyncResult ar) {
            AsyncResult result = (AsyncResult)ar;
            ((WaitTimeoutCallback)result.AsyncDelegate).EndInvoke(ar);
            if(base.IsHandleCreated) {
                base.Invoke(new FormMethodInvoker(this.CallbackFolderTree), new object[] { result.AsyncState });
            }
        }

        private static void AsyncComplete_ItemEdit(IAsyncResult ar) {
            AsyncResult result = (AsyncResult)ar;
            ((WaitTimeoutCallback)result.AsyncDelegate).EndInvoke(ar);
            Control control = (Control)((object[])result.AsyncState)[2];
            if(control.IsHandleCreated) {
                control.Invoke(new FormMethodInvoker(QTTabBarClass.CallbackItemEdit), new object[] { result.AsyncState });
            }
        }

        private void AsyncComplete_MultiPath(IAsyncResult ar) {
            AsyncResult result = (AsyncResult)ar;
            ((MethodInvoker)result.AsyncDelegate).EndInvoke(ar);
            if(base.IsHandleCreated) {
                base.Invoke(new FormMethodInvoker(this.CallbackMultiPath), new object[] { result.AsyncState });
            }
        }

        // This function is a standin for BeforeNavigate2.  On 7, the path is
        // empty and cancel has no effect.
        private void BeforeNavigate(string path, ref bool cancel) {
            if(this.NowNavigating || !this.IsShown) {
                return;
            }
            this.NowNavigating = true;
            this.HideThumbnailTooltip(7);
            this.HideSubDirTip(7);
            this.HideSubDirTip_Tab_Menu();
            if(this.timer_HoverThumbnail != null) {
                this.timer_HoverThumbnail.Enabled = false;
            }
            this.NowTabDragging = false;
            if(!this.NavigatedByCode) {
                // TODO this won't work...
                this.SaveSelectedItems(this.CurrentTab);
                if((!string.IsNullOrEmpty(path) &&
                        this.CurrentTab.TabLocked &&
                        !QTUtility2.IsShellPathButNotFileSystem(this.CurrentTab.CurrentPath)) &&
                        !QTUtility2.IsShellPathButNotFileSystem(path)) {
                    this.CloneTabButton(this.CurrentTab, path, true, this.tabControl1.SelectedIndex + 1);
                    cancel = true;
                    return;
                }
            }
            if(this.NowInTravelLog) {
                if(this.CurrentTravelLogIndex > 0) {
                    this.CurrentTravelLogIndex--;

                    // TODO ...
                    if(!IsSpecialFolderNeedsToTravel(path)) {
                        this.NavigateBackToTheFuture();
                    }
                }
                else {
                    this.NowInTravelLog = false;
                }
            }
        }

        private void CallBackDoOpenGroups(object obj, IntPtr ptr) {
            string[] strArray = (string[])obj;
            this.tabControl1.SetRedraw(false);
            foreach(string str in strArray) {
                this.OpenGroup(str, false);
            }
            this.tabControl1.SetRedraw(true);
            this.RestoreFromTray();
        }

        private bool CallbackEnumChildProc_SearchBand(IntPtr hwnd, IntPtr lParam) {
            StringBuilder lpClassName = new StringBuilder(260);
            PInvoke.GetClassName(hwnd, lpClassName, lpClassName.Capacity);
            if(lpClassName.ToString() == "UniversalSearchBand") {
                this.hwndSearchBand = hwnd;
                return false;
            }
            this.hwndSearchBand = IntPtr.Zero;
            return true;
        }

        private bool CallbackEnumChildProc_SearchBand_Edit(IntPtr hwnd, IntPtr lParam) {
            StringBuilder lpClassName = new StringBuilder(260);
            PInvoke.GetClassName(hwnd, lpClassName, lpClassName.Capacity);
            if((lpClassName.ToString() == "Edit") && ((((int)PInvoke.GetWindowLongPtr(hwnd, -16)) & 0x10000000) == 0x10000000)) {
                this.hwndSeachBand_Edit = hwnd;
                return false;
            }
            this.hwndSeachBand_Edit = IntPtr.Zero;
            return true;
        }

        private bool CallbackEnumChildProc_SysListView32(IntPtr hwnd, IntPtr lParam) {
            StringBuilder lpClassName = new StringBuilder(260);
            PInvoke.GetClassName(hwnd, lpClassName, lpClassName.Capacity);
            if(lpClassName.ToString() == "SysListView32") {
                this.hwndSysListView32 = hwnd;
                return false;
            }
            else if(lpClassName.ToString() == "DirectUIHWND") {
                this.hwndDirectUI = hwnd;
            }
            this.hwndSysListView32 = IntPtr.Zero;
            return true;
        }

        private bool CallbackEnumChildProc_TravelBand(IntPtr hwnd, IntPtr lParam) {
            StringBuilder lpClassName = new StringBuilder(260);
            PInvoke.GetClassName(hwnd, lpClassName, lpClassName.Capacity);
            if(lpClassName.ToString() == "TravelBand") {
                this.hwndTravelBand = hwnd;
                return false;
            }
            this.hwndTravelBand = IntPtr.Zero;
            return true;
        }

        private void CallbackFirstNavComp() {
            int num = 0;
            while(!this.FirstNavigationCompleted) {
                Thread.Sleep(100);
                if(++num > 100) {
                    return;
                }
            }
        }

        private void CallbackFolderTree(object obj) {
            bool fShow = (bool)obj;
            this.ShowFolderTree(fShow);
            if(fShow) {
                PInvoke.SetRedraw(this.ExplorerHandle, true);
                PInvoke.RedrawWindow(this.ExplorerHandle, IntPtr.Zero, IntPtr.Zero, 0x289);
            }
        }

        private IntPtr CallbackGetMsgProc(int nCode, IntPtr wParam, IntPtr lParam) {
            if(nCode >= 0) {
                BandObjectLib.MSG msg = (BandObjectLib.MSG)Marshal.PtrToStructure(lParam, typeof(BandObjectLib.MSG));
                if(!QTUtility.IsVista) {
                    if(msg.message == WM.CLOSE) {
                        if(this.iSequential_WM_CLOSE > 0) {
                            Marshal.StructureToPtr(new BandObjectLib.MSG(), lParam, false);
                            return PInvoke.CallNextHookEx(this.hHook_Msg, nCode, wParam, lParam);
                        }
                        this.iSequential_WM_CLOSE++;
                    }
                    else {
                        this.iSequential_WM_CLOSE = 0;
                    }
                }
                switch(msg.message) {

                    case WM.LBUTTONDOWN:
                    case WM.LBUTTONUP:
                        if((!QTUtility.IsVista && !QTUtility.CheckConfig(Settings.NoMidClickTree)) && ((((int)((long)msg.wParam)) & 4) != 0)) {
                            this.HandleLBUTTON_Tree(msg, msg.message == 0x201);
                        }
                        break;

                    case WM.MBUTTONUP:
                        if(!QTUtility.IsVista && !base.Explorer.Busy && !QTUtility.CheckConfig(Settings.NoMidClickTree)) {
                            this.Handle_MButtonUp_Tree(msg);
                        }
                        break;

                    case WM.CLOSE:
                        if(!QTUtility.IsVista) {
                            if((msg.hwnd == this.ExplorerHandle) && this.HandleCLOSE(msg.lParam)) {
                                Marshal.StructureToPtr(new BandObjectLib.MSG(), lParam, false);
                            }
                            break;
                        }
                        if(msg.hwnd == WindowUtils.GetShellTabWindowClass(this.ExplorerHandle)) {
                            try {
                                bool flag = this.tabControl1.TabCount == 1;
                                string currentPath = ((QTabItem)this.tabControl1.SelectedTab).CurrentPath;
                                if((!Directory.Exists(currentPath) && (currentPath.Length > 3)) && (currentPath.Substring(1, 2) == @":\")) {
                                    if(flag) {
                                        WindowUtils.CloseExplorer(this.ExplorerHandle, 2);
                                    }
                                    else {
                                        this.CloseTab((QTabItem)this.tabControl1.SelectedTab, true);
                                    }
                                }
                            }
                            catch {
                            }
                            Marshal.StructureToPtr(new BandObjectLib.MSG(), lParam, false);
                        }
                        break;

                    case WM.COMMAND:
                        if(!QTUtility.IsVista) {
                            int num = ((int)((long)msg.wParam)) & 0xffff;
                            if(num == 0xa021) {
                                WindowUtils.CloseExplorer(this.ExplorerHandle, 3);
                                Marshal.StructureToPtr(new BandObjectLib.MSG(), lParam, false);
                            }
                        }
                        break;
                }
            }
            return PInvoke.CallNextHookEx(this.hHook_Msg, nCode, wParam, lParam);
        }

        private static void CallbackItemEdit(object obj) {
            object[] objArray = (object[])obj;
            IntPtr hWnd = (IntPtr)objArray[0];
            int num = (int)objArray[1];
            PInvoke.SendMessage(hWnd, 0xb1, IntPtr.Zero, (IntPtr)num);
        }

        private IntPtr CallbackKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam) {
            if((nCode >= 0) && !this.NowModalDialogShown) {

                // This seems incredibly janky.  But it's the only way I could 
                // get it not to overflow...  Casting it right to a uint throws
                // an exception if the last bit is set.
                uint flags = (uint)((long)lParam);

                if((flags & 0x80000000) == 0) {
                    if(this.HandleKEYDOWN(wParam, (flags & 0x40000000) == 0x40000000)) {
                        return new IntPtr(1);
                    }
                }
                else {
                    this.HideThumbnailTooltip(3);
                    if((!QTUtility.CheckConfig(Settings.NoShowSubDirTips) && QTUtility.CheckConfig(Settings.SubDirTipsWithShift)) && ((this.subDirTip != null) && !this.subDirTip.MenuIsShowing)) {
                        this.HideSubDirTip(4);
                    }
                    if(this.NowTabDragging && (this.DraggingTab != null)) {
                        this.Cursor = Cursors.Default;
                    }
                    switch(((int)wParam)) {
                        case 0x11:
                            if(!QTUtility.CheckConfig(Settings.NoTabSwitcher)) {
                                this.HideTabSwitcher(true);
                            }
                            goto Label_0124;

                        case 0x12:
                            if(QTUtility.CheckConfig(Settings.ShowTabCloseButtons) && QTUtility.CheckConfig(Settings.TabCloseBtnsWithAlt)) {
                                this.tabControl1.ShowCloseButton(false);
                            }
                            goto Label_0124;

                        case 9:
                            if((!QTUtility.CheckConfig(Settings.NoTabSwitcher) && (this.tabSwitcher != null)) && this.tabSwitcher.IsShown) {
                                this.tabControl1.SetPseudoHotIndex(this.tabSwitcher.SelectedIndex);
                            }
                            goto Label_0124;
                    }
                }
            }
        Label_0124:
            return PInvoke.CallNextHookEx(this.hHook_Key, nCode, wParam, lParam);
        }

        private IntPtr CallbackMouseProc(int nCode, IntPtr wParam, IntPtr lParam) {
            if((nCode >= 0) && !this.NowModalDialogShown) {
                IntPtr ptr = (IntPtr)1;
                switch(((int)wParam)) {
                    case WM.MOUSEWHEEL:
                        if(!this.HandleMOUSEWHEEL(lParam)) {
                            break;
                        }
                        return ptr;

                    case WM.XBUTTONDOWN:
                        if(!QTUtility.CheckConfig(Settings.CaptureX1X2)) {
                            break;
                        }
                        return ptr;

                    case WM.XBUTTONUP:
                        if(!QTUtility.CheckConfig(Settings.CaptureX1X2)) {
                            break;
                        }
                        this.HandleXBUTTON();
                        return ptr;
                }
            }
            return PInvoke.CallNextHookEx(this.hHook_Mouse, nCode, wParam, lParam);
        }

        private void CallbackMultiPath(object obj) {
            object[] objArray = (object[])obj;
            string[] collection = (string[])objArray[0];
            int num = (int)objArray[1];
            switch(num) {
                case 0:
                    foreach(string str in collection) {
                        this.OpenNewTab(str, true);
                    }
                    break;

                case 1: {
                        bool flag = true;
                        foreach(string str2 in collection) {
                            this.OpenNewTab(str2, !flag);
                            flag = false;
                        }
                        break;
                    }
                default:
                    QTUtility.TMPPathList = new List<string>(collection);
                    using(IDLWrapper wrapper = new IDLWrapper(collection[0])) {
                        this.OpenNewWindow(wrapper);
                    }
                    break;
            }
            if(num == 1) {
                this.RestoreFromTray();
                WindowUtils.BringExplorerToFront(this.ExplorerHandle);
            }
        }

        private void CallbackPlugin() {
            this.SyncButtonBarCurrent(0x100);
        }

        private void CancelFailedNavigation(string failedPath, bool fRollBackForward, int countRollback) {
            this.ShowMessageNavCanceled(failedPath, false);
            if(fRollBackForward) {
                for(int i = 0; i < countRollback; i++) {
                    this.CurrentTab.GoForward();
                }
            }
            else {
                for(int j = 0; j < countRollback; j++) {
                    this.CurrentTab.GoBackward();
                }
            }
            this.NavigatedByCode = false;
        }

        private void CancelFailedTabChanging(string newPath) {
            if(!this.CloseTab((QTabItem)this.tabControl1.SelectedTab, true)) {
                if(this.tabControl1.TabCount == 1) {
                    WindowUtils.CloseExplorer(this.ExplorerHandle, 2);
                }
                else {
                    this.ShowMessageNavCanceled(newPath, false);
                    if(this.CurrentTab == null) {
                        this.tabControl1.SelectedIndex = 0;
                    }
                }
            }
            else {
                QTUtility.ClosedTabHistoryList.Remove(newPath);
                if(this.tabControl1.TabCount == 0) {
                    this.ShowMessageNavCanceled(newPath, true);
                    WindowUtils.CloseExplorer(this.ExplorerHandle, 2);
                }
                else {
                    if(this.CurrentTab == null) {
                        this.tabControl1.SelectedIndex = 0;
                    }
                    else {
                        this.tabControl1.SelectTab(this.CurrentTab);
                    }
                    this.ShowMessageNavCanceled(newPath, false);
                }
            }
        }

        private void ChangeViewMode(bool fUp) {
            IShellView ppshv = null;
            try {
                if(this.ShellBrowser.QueryActiveShellView(out ppshv) == 0) {
                    IFolderView view2 = (IFolderView)ppshv;
                    int pViewMode = 0;
                    if(view2.GetCurrentViewMode(ref pViewMode) == 0) {
                        FolderViewMode mode = (FolderViewMode)pViewMode;
                        switch(mode) {
                            case FolderViewMode.FVM_ICON:
                                mode = fUp ? FolderViewMode.FVM_TILE : FolderViewMode.FVM_LIST;
                                break;

                            case FolderViewMode.FVM_LIST:
                                mode = fUp ? FolderViewMode.FVM_ICON : FolderViewMode.FVM_DETAILS;
                                break;

                            case FolderViewMode.FVM_DETAILS:
                                if(fUp) {
                                    mode = FolderViewMode.FVM_LIST;
                                }
                                break;

                            case FolderViewMode.FVM_THUMBNAIL:
                                mode = fUp ? FolderViewMode.FVM_THUMBSTRIP : FolderViewMode.FVM_TILE;
                                break;

                            case FolderViewMode.FVM_TILE:
                                mode = fUp ? FolderViewMode.FVM_THUMBNAIL : FolderViewMode.FVM_ICON;
                                break;

                            case FolderViewMode.FVM_THUMBSTRIP:
                                if(!fUp) {
                                    mode = FolderViewMode.FVM_THUMBNAIL;
                                }
                                break;
                        }
                        if(pViewMode != (int)mode) {
                            view2.SetCurrentViewMode((int)mode);
                        }
                    }
                }
            }
            catch {
            }
            finally {
                if(ppshv != null) {
                    Marshal.ReleaseComObject(ppshv);
                }
            }
        }

        private static bool CheckProcessID(IntPtr hwnd1, IntPtr hwnd2) {
            uint num;
            uint num2;
            PInvoke.GetWindowThreadProcessId(hwnd1, out num);
            PInvoke.GetWindowThreadProcessId(hwnd2, out num2);
            return ((num == num2) && (num != 0));
        }

        private void ChooseNewDirectory() {
            this.NowModalDialogShown = true;
            bool nowTopMost = this.NowTopMost;
            if(nowTopMost) {
                this.ToggleTopMost();
            }
            using(FolderBrowserDialog dialog = new FolderBrowserDialog()) {
                dialog.SelectedPath = this.CurrentAddress;
                if(DialogResult.OK == dialog.ShowDialog()) {
                    this.OpenNewTab(dialog.SelectedPath, false);
                }
            }
            this.NowModalDialogShown = false;
            if(nowTopMost) {
                this.ToggleTopMost();
            }
        }

        private void ClearTravelLogs() {
            IEnumTravelLogEntry ppenum = null;
            try {
                ITravelLogEntry entry2;
                if((this.TravelLog.EnumEntries(0x30, out ppenum) != 0) || (ppenum == null)) {
                    return;
                }
                int num = 0;
            Label_0018:
                entry2 = null;
                try {
                    if(ppenum.Next(1, out entry2, 0) == 0) {
                        IntPtr ptr;
                        if((num++ != 0) && (entry2.GetURL(out ptr) == 0)) {
                            string path = Marshal.PtrToStringUni(ptr);
                            PInvoke.CoTaskMemFree(ptr);
                            if(!IsSpecialFolderNeedsToTravel(path)) {
                                this.TravelLog.RemoveEntry(entry2);
                            }
                        }
                        goto Label_0018;
                    }
                }
                finally {
                    if(entry2 != null) {
                        Marshal.ReleaseComObject(entry2);
                    }
                }
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception, null);
            }
            finally {
                if(ppenum != null) {
                    Marshal.ReleaseComObject(ppenum);
                }
            }
        }

        private void CloneTabButton(QTabItem tab, LogData log) {
            this.NowTabCloned = true;
            QTabItem item = tab.Clone();
            this.AddInsertTab(item);
            using(IDLWrapper wrapper = new IDLWrapper(log.IDL)) {
                if(wrapper.Available) {
                    item.NavigatedTo(wrapper.Path, wrapper.IDL, log.Hash);
                }
            }
            this.tabControl1.SelectTab(item);
        }

        private void CloneTabButton(QTabItem tab, string optionURL, bool fSelect, int index) {
            this.NowTabCloned = fSelect;
            QTabItem item = tab.Clone();
            if(index < 0) {
                this.AddInsertTab(item);
            }
            else if((-1 < index) && (index < (this.tabControl1.TabCount + 1))) {
                this.tabControl1.TabPages.Insert(index, item);
            }
            else {
                this.AddInsertTab(item);
            }
            if(optionURL != null) {
                using(IDLWrapper wrapper = new IDLWrapper(optionURL)) {
                    item.NavigatedTo(optionURL, wrapper.IDL, -1);
                }
            }
            if(fSelect) {
                this.tabControl1.SelectTab(item);
            }
            else {
                item.RefreshRectangle();
                this.tabControl1.Refresh();
            }
        }

        private List<string> CloseAllUnlocked() {
            List<string> list = new List<string>();
            List<QTabItem> list2 = new List<QTabItem>();
            this.tabControl1.SetRedraw(false);
            if(this.CurrentTab.TabLocked) {
                foreach(QTabItem item in this.tabControl1.TabPages) {
                    if(!item.TabLocked) {
                        list.Add(item.CurrentPath);
                        list2.Add(item);
                        AddToHistory(item);
                        this.tabControl1.TabPages.Remove(item);
                    }
                }
                this.SyncButtonBarCurrent(0x1003f);
            }
            else {
                foreach(QTabItem item2 in this.tabControl1.TabPages) {
                    if(!item2.TabLocked && (item2 != this.CurrentTab)) {
                        list.Add(item2.CurrentPath);
                        list2.Add(item2);
                        AddToHistory(item2);
                        this.tabControl1.TabPages.Remove(item2);
                    }
                }
                list.Add(this.CurrentTab.CurrentPath);
                this.CloseTab(this.CurrentTab, false);
            }
            for(int i = 0; i < list2.Count; i++) {
                list2[i].OnClose();
                list2[i] = null;
            }
            if(this.tabControl1.TabCount > 0) {
                this.tabControl1.SetRedraw(true);
            }
            return list;
        }

        public override void CloseDW(uint dwReserved) {
            try {
                if(this.thumbnailTooltip != null) {
                    this.thumbnailTooltip.Dispose();
                    this.thumbnailTooltip = null;
                }
                if(this.subDirTip != null) {
                    this.subDirTip.Dispose();
                    this.subDirTip = null;
                }
                if(this.subDirTip_Tab != null) {
                    this.subDirTip_Tab.Dispose();
                    this.subDirTip_Tab = null;
                }
                if(this.IsShown) {
                    if(this.pluginManager != null) {
                        this.pluginManager.Close(false);
                        this.pluginManager = null;
                    }
                    if(this.hHook_Key != IntPtr.Zero) {
                        PInvoke.UnhookWindowsHookEx(this.hHook_Key);
                        this.hHook_Key = IntPtr.Zero;
                    }
                    if(this.hHook_Mouse != IntPtr.Zero) {
                        PInvoke.UnhookWindowsHookEx(this.hHook_Mouse);
                        this.hHook_Mouse = IntPtr.Zero;
                    }
                    if(this.hHook_Msg != IntPtr.Zero) {
                        PInvoke.UnhookWindowsHookEx(this.hHook_Msg);
                        this.hHook_Msg = IntPtr.Zero;
                    }
                    if(this.explorerController != null) {
                        this.explorerController.ReleaseHandle();
                        this.explorerController = null;
                    }
                    if(this.shellViewController != null) {
                        this.shellViewController.ReleaseHandle();
                        this.shellViewController = null;
                    }
                    if(this.rebarController != null) {
                        this.rebarController.ReleaseHandle();
                        this.rebarController = null;
                    }
                    if(QTUtility.IsVista && (this.travelBtnController != null)) {
                        this.travelBtnController.ReleaseHandle();
                        this.travelBtnController = null;
                    }
                    if(dicNotifyIcon != null) {
                        dicNotifyIcon.Remove(this.ExplorerHandle);
                    }
                    if((hwndNotifyIconParent == this.ExplorerHandle) && (notifyIcon != null)) {
                        notifyIcon.Dispose();
                        notifyIcon = null;
                        contextMenuNotifyIcon.Dispose();
                        contextMenuNotifyIcon = null;
                        hwndNotifyIconParent = IntPtr.Zero;
                        if(dicNotifyIcon.Count > 0) {
                            foreach(IntPtr ptr in dicNotifyIcon.Keys) {
                                IntPtr tabBarHandle = QTUtility.instanceManager.GetTabBarHandle(ptr);
                                if(1 == ((int)QTUtility2.SendCOPYDATASTRUCT(tabBarHandle, (IntPtr)0x30, "createNI", IntPtr.Zero))) {
                                    break;
                                }
                            }
                        }
                    }
                    using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar")) {
                        if(!QTUtility.CheckConfig(Settings.NoHistory)) {
                            foreach(QTabItem item in this.tabControl1.TabPages) {
                                AddToHistory(item);
                            }
                            QTUtility.SaveRecentlyClosed(key);
                        }
                        if(!QTUtility.CheckConfig(Settings.NoRecentFiles) && QTUtility.CheckConfig(Settings.AllRecentFiles)) {
                            QTUtility.SaveRecentFiles(key);
                        }
                        List<string> list = new List<string>();
                        foreach(QTabItem item2 in this.tabControl1.TabPages) {
                            if(item2.TabLocked) {
                                list.Add(item2.CurrentPath);
                            }
                        }
                        QTUtility2.WriteRegBinary<string>(list.ToArray(), "TabsLocked", key);
                        string str = string.Empty;
                        foreach(string str2 in QTUtility.StartUpGroupList) {
                            str = str + str2 + ";";
                        }
                        str = str.TrimEnd(QTUtility.SEPARATOR_CHAR);
                        key.SetValue("StartUpGroups", str);
                        if(QTUtility.instanceManager.RemoveInstance(this.ExplorerHandle, this)) {
                            QTUtility.instanceManager.NextInstanceExists();
                            QTUtility2.WriteRegHandle("Handle", key, QTUtility.instanceManager.CurrentHandle);
                        }
                        if(QTUtility.CheckConfig(Settings.SaveTransparency)) {
                            if(0x80000 != ((int)PInvoke.Ptr_OP_AND(PInvoke.GetWindowLongPtr(this.ExplorerHandle, -20), 0x80000))) {
                                QTUtility.WindowAlpha = 0xff;
                            }
                            else {
                                byte num;
                                int num2;
                                int num3;
                                if(PInvoke.GetLayeredWindowAttributes(this.ExplorerHandle, out num2, out num, out num3)) {
                                    QTUtility.WindowAlpha = num;
                                }
                                else {
                                    QTUtility.WindowAlpha = 0xff;
                                }
                            }
                            key.SetValue("WindowAlpha", QTUtility.WindowAlpha);
                        }
                        IDLWrapper.SaveCache(key);
                    }
                    if((md5Form != null) && !md5Form.InvokeRequired) {
                        md5Form.SaveMD5FormStat();
                        md5Form.Dispose();
                        md5Form = null;
                    }
                    this.Cursor = Cursors.Default;
                    if((this.curTabDrag != null) && (this.curTabDrag != Cursors.Default)) {
                        PInvoke.DestroyIcon(this.curTabDrag.Handle);
                        GC.SuppressFinalize(this.curTabDrag);
                        this.curTabDrag = null;
                    }
                    if((this.curTabCloning != null) && (this.curTabCloning != Cursors.Default)) {
                        PInvoke.DestroyIcon(this.curTabCloning.Handle);
                        GC.SuppressFinalize(this.curTabCloning);
                        this.curTabCloning = null;
                    }
                    if(this.dropTargetWrapper != null) {
                        this.dropTargetWrapper.Dispose();
                        this.dropTargetWrapper = null;
                    }
                    if((optionsDialog != null) && this.fOptionDialogCreated) {
                        this.fOptionDialogCreated = false;
                        try {
                            optionsDialog.Invoke(new MethodInvoker(this.odCallback_Close));
                        }
                        catch(Exception exception) {
                            QTUtility2.MakeErrorLog(exception, "optionDialogDisposing");
                        }
                    }
                    if(this.tabSwitcher != null) {
                        this.tabSwitcher.Dispose();
                        this.tabSwitcher = null;
                    }
                }
                QTUtility.InstancesCount--;
                if(this.TravelLog != null) {
                    Marshal.FinalReleaseComObject(this.TravelLog);
                    this.TravelLog = null;
                }
                if(this.iContextMenu2 != null) {
                    Marshal.FinalReleaseComObject(this.iContextMenu2);
                    this.iContextMenu2 = null;
                }
                if(this.ShellBrowser != null) {
                    Marshal.FinalReleaseComObject(this.ShellBrowser);
                    this.ShellBrowser = null;
                }
                foreach(ITravelLogEntry entry in this.LogEntryDic.Values) {
                    if(entry != null) {
                        Marshal.FinalReleaseComObject(entry);
                    }
                }
                this.LogEntryDic.Clear();
                if(this.bmpRebar != null) {
                    this.bmpRebar.Dispose();
                    this.bmpRebar = null;
                }
                if(this.textureBrushRebar != null) {
                    this.textureBrushRebar.Dispose();
                    this.textureBrushRebar = null;
                }
                base.fFinalRelease = true;
                base.CloseDW(dwReserved);
            }
            catch(Exception exception2) {
                QTUtility2.MakeErrorLog(exception2, "tabbar closing");
            }
        }

        private void CloseLeftRight(bool fLeft, int index) {
            if(index == -1) {
                index = this.tabControl1.SelectedIndex;
            }
            if(fLeft ? (index > 0) : (index < (this.tabControl1.TabCount - 1))) {
                List<QTabItem> list = new List<QTabItem>();
                for(int i = 0; i < this.tabControl1.TabPages.Count; i++) {
                    if(fLeft ? (i < index) : (i > index)) {
                        list.Add((QTabItem)this.tabControl1.TabPages[i]);
                    }
                }
                this.tabControl1.SetRedraw(false);
                foreach(QTabItem item in list) {
                    this.CloseTab(item);
                }
                this.tabControl1.SetRedraw(true);
            }
        }

        private bool CloseTab(QTabItem closingTab) {
            return ((this.tabControl1.TabCount > 1) && this.CloseTab(closingTab, false));
        }

        private bool CloseTab(QTabItem closingTab, bool fCritical) {
            QTabItemBase base3;
            bool flag2;
            if(closingTab == null) {
                return false;
            }
            if((!fCritical && closingTab.TabLocked) && QTUtility2.PathExists(closingTab.CurrentPath)) {
                return false;
            }
            int index = this.tabControl1.TabPages.IndexOf(closingTab);
            if(index == -1) {
                return false;
            }
            this.lstActivatedTabs.Remove(closingTab);
            AddToHistory(closingTab);
            bool flag = closingTab == this.CurrentTab;
            this.tabControl1.TabPages.Remove(closingTab);
            closingTab.OnClose();
            closingTab = null;
            if(flag) {
                this.CurrentTab = null;
            }
            if(!flag) {
                this.SyncButtonBarCurrent(0x1003c);
                goto Label_0249;
            }
            int tabCount = this.tabControl1.TabCount;
            if(tabCount <= 0) {
                goto Label_0249;
            }
            QTabItemBase tabPage = null;
            switch(QTUtility.ConfigValues[2]) {
                case 0:
                    if(index != tabCount) {
                        tabPage = this.tabControl1.TabPages[index];
                    }
                    else {
                        tabPage = this.tabControl1.TabPages[index - 1];
                    }
                    goto Label_020D;

                case 1:
                    if(index != 0) {
                        tabPage = this.tabControl1.TabPages[index - 1];
                    }
                    else {
                        tabPage = this.tabControl1.TabPages[0];
                    }
                    goto Label_020D;

                case 2:
                    tabPage = this.tabControl1.TabPages[tabCount - 1];
                    goto Label_020D;

                case 3:
                    tabPage = this.tabControl1.TabPages[0];
                    goto Label_020D;

                case 4:
                    if(this.lstActivatedTabs.Count <= 0) {
                        tabPage = this.tabControl1.TabPages[0];
                        goto Label_020D;
                    }
                    base3 = this.lstActivatedTabs[this.lstActivatedTabs.Count - 1];
                    this.lstActivatedTabs.RemoveAt(this.lstActivatedTabs.Count - 1);
                    flag2 = false;
                    foreach(QTabItem item in this.tabControl1.TabPages) {
                        if(item == base3) {
                            flag2 = true;
                            break;
                        }
                    }
                    break;

                default:
                    goto Label_020D;
            }
            if(flag2) {
                tabPage = base3;
            }
            else {
                tabPage = this.tabControl1.TabPages[0];
            }
        Label_020D:
            if(tabPage != null) {
                this.tabControl1.SelectTab(tabPage);
            }
            else {
                this.tabControl1.SelectTab(0);
            }
            this.SyncButtonBarCurrent((tabPage == null) ? 60 : 0x3f);
        Label_0249:
            return true;
        }

        private void contextMenuDropped_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            if(e.ClickedItem.Tag != null) {
                List<string> tag = (List<string>)this.contextMenuDropped.Tag;
                using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar\UserApps")) {
                    if(key != null) {
                        List<string> list2 = new List<string>();
                        foreach(string str in key.GetValueNames()) {
                            list2.Add(str.ToLower());
                        }
                        foreach(string str2 in tag) {
                            try {
                                string directoryName = Path.GetDirectoryName(str2);
                                if(directoryName == null) {
                                    directoryName = string.Empty;
                                }
                                string fileName = Path.GetFileName(str2);
                                string regValueName = fileName;
                                int num = 2;
                                while(list2.Contains(regValueName.ToLower())) {
                                    regValueName = fileName + "_" + num++;
                                }
                                string[] array = new string[] { str2, string.Empty, directoryName, "0" };
                                QTUtility2.WriteRegBinary<string>(array, regValueName, key);
                                QTUtility.UserAppsDic[regValueName] = array;
                                list2.Add(regValueName.ToLower());
                                continue;
                            }
                            catch {
                                continue;
                            }
                        }
                        SyncTaskBarMenu();
                        SyncButtonBarBroadCast(0x200);
                    }
                }
            }
        }

        private static void contextMenuNotifyIcon_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            if(!(e.ClickedItem is ToolStripSeparator)) {
                if(e.ClickedItem.Tag == null) {
                    if(e.ClickedItem.Text == "Restore all") {
                        RestoreAllWindowFromTray();
                    }
                    else {
                        notifyIcon.Visible = false;
                        contextMenuNotifyIcon.Hide();
                        bool flag = false;
                        Dictionary<IntPtr, int> dictionary = new Dictionary<IntPtr, int>(dicNotifyIcon);
                        foreach(IntPtr ptr in dictionary.Keys) {
                            if(ptr != hwndNotifyIconParent) {
                                if(ptr != IntPtr.Zero) {
                                    WindowUtils.CloseExplorer(ptr, 2);
                                    Thread.Sleep(100);
                                }
                            }
                            else {
                                flag = true;
                            }
                        }
                        if(flag && (hwndNotifyIconParent != IntPtr.Zero)) {
                            WindowUtils.CloseExplorer(hwndNotifyIconParent, 2);
                        }
                    }
                }
                else {
                    IntPtr tag = (IntPtr)e.ClickedItem.Tag;
                    ShowTaksbarItem(tag, true);
                }
            }
        }

        private static void contextMenuNotifyIcon_SubItems_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            IntPtr tag = (IntPtr)e.ClickedItem.Tag;
            int index = ((ToolStripMenuItem)sender).DropDownItems.IndexOf(e.ClickedItem);
            ShowTaksbarItem(tag, true);
            QTTabBarClass tabBar = QTUtility.instanceManager.GetTabBar(tag);
            if((tabBar != null) && (index > -1)) {
                tabBar.Invoke(new NavigationCompleteCallback(QTTabBarClass.contextMenuNotifyIcon_SubItems_SelectTab), new object[] { tabBar, (IntPtr)index });
            }
        }

        private static void contextMenuNotifyIcon_SubItems_SelectTab(object tabBar, IntPtr index) {
            ((QTTabBarClass)tabBar).tabControl1.SelectedIndex = (int)index;
        }

        private void contextMenuSys_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            if(e.ClickedItem == this.tsmiOption) {
                this.OpenOptionsDialog();
            }
            else if(e.ClickedItem == this.tsmiCloseAllButCurrent) {
                if(this.tabControl1.TabCount != 1) {
                    foreach(QTabItem item in this.tabControl1.TabPages) {
                        if(item != this.CurrentTab) {
                            this.CloseTab(item);
                        }
                    }
                }
            }
            else if(e.ClickedItem == this.tsmiBrowseFolder) {
                this.ChooseNewDirectory();
            }
            else if(e.ClickedItem == this.tsmiCloseWindow) {
                WindowUtils.CloseExplorer(this.ExplorerHandle, 1);
            }
            else {
                if(e.ClickedItem == this.tsmiLastActiv) {
                    try {
                        this.tabControl1.SelectTab(this.lstActivatedTabs[this.lstActivatedTabs.Count - 2]);
                        return;
                    }
                    catch {
                        return;
                    }
                }
                if(e.ClickedItem == this.tsmiLockToolbar) {
                    WindowUtils.LockToolbar(!this.tsmiLockToolbar.Checked, this.ExplorerHandle, base.ReBarHandle);
                }
                else if(e.ClickedItem == this.tsmiMergeWindows) {
                    this.MergeAllWindows();
                }
            }
        }

        private void contextMenuSys_Opening(object sender, CancelEventArgs e) {
            this.InitializeSysMenu(false);
            this.contextMenuSys.SuspendLayout();
            this.tsmiGroups.DropDown.SuspendLayout();
            this.tsmiUndoClose.DropDown.SuspendLayout();
            MenuUtility.CreateGroupItems(this.tsmiGroups);
            MenuUtility.CreateUndoClosedItems(this.tsmiUndoClose);
            if((this.lstActivatedTabs.Count > 1) && this.tabControl1.TabPages.Contains(this.lstActivatedTabs[this.lstActivatedTabs.Count - 2])) {
                this.tsmiLastActiv.ToolTipText = this.lstActivatedTabs[this.lstActivatedTabs.Count - 2].CurrentPath;
                this.tsmiLastActiv.Enabled = true;
            }
            else {
                this.tsmiLastActiv.ToolTipText = string.Empty;
                this.tsmiLastActiv.Enabled = false;
            }
            while(this.tsmiExecuted.DropDownItems.Count > 0) {
                this.tsmiExecuted.DropDownItems[0].Dispose();
            }
            List<ToolStripItem> list = MenuUtility.CreateRecentFilesItems();
            if(list.Count > 0) {
                this.tsmiExecuted.DropDown.SuspendLayout();
                this.tsmiExecuted.DropDownItems.AddRange(list.ToArray());
                this.tsmiExecuted.DropDown.ResumeLayout();
            }
            this.tsmiExecuted.Enabled = this.tsmiExecuted.DropDownItems.Count > 0;
            this.tsmiMergeWindows.Enabled = QTUtility.InstancesCount > 1;
            this.tsmiLockToolbar.Checked = WindowUtils.IsToolbarLocked(base.ReBarHandle);
            if((this.lstPluginMenuItems_Sys != null) && (this.lstPluginMenuItems_Sys.Count > 0)) {
                foreach(ToolStripItem item in this.lstPluginMenuItems_Sys) {
                    item.Dispose();
                }
                this.lstPluginMenuItems_Sys = null;
            }
            if((this.pluginManager != null) && (this.pluginManager.dicFullNamesMenuRegistered_Sys.Count > 0)) {
                this.lstPluginMenuItems_Sys = new List<ToolStripItem>();
                int index = this.contextMenuSys.Items.IndexOf(this.tsmiOption);
                ToolStripSeparator separator = new ToolStripSeparator();
                this.contextMenuSys.Items.Insert(index, separator);
                foreach(string str in this.pluginManager.dicFullNamesMenuRegistered_Sys.Keys) {
                    ToolStripMenuItem item2 = new ToolStripMenuItem(this.pluginManager.dicFullNamesMenuRegistered_Sys[str]);
                    item2.Name = str;
                    item2.Tag = MenuType.Bar;
                    item2.Click += new EventHandler(this.pluginitems_Click);
                    this.contextMenuSys.Items.Insert(index, item2);
                    this.lstPluginMenuItems_Sys.Add(item2);
                }
                this.lstPluginMenuItems_Sys.Add(separator);
            }
            this.tsmiUndoClose.DropDown.ResumeLayout();
            this.tsmiGroups.DropDown.ResumeLayout();
            this.contextMenuSys.ResumeLayout();
        }

        private void contextMenuTab_Closed(object sender, ToolStripDropDownClosedEventArgs e) {
            this.tabControl1.SetContextMenuState(false);
            if(this.ContextMenuedTab != this.CurrentTab) {
                this.tabControl1.Refresh();
            }
        }

        private void contextMenuTab_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            if(this.ContextMenuedTab != null) {
                if(e.ClickedItem == this.tsmiClose) {
                    if(this.tabControl1.TabCount == 1) {
                        WindowUtils.CloseExplorer(this.ExplorerHandle, 1);
                    }
                    else {
                        this.CloseTab(this.ContextMenuedTab);
                    }
                }
                else if(e.ClickedItem == this.tsmiCloseAllButThis) {
                    this.tabControl1.SetRedraw(false);
                    foreach(QTabItem item in this.tabControl1.TabPages) {
                        if(item != this.ContextMenuedTab) {
                            this.CloseTab(item);
                        }
                    }
                    this.tabControl1.SetRedraw(true);
                }
                else if(e.ClickedItem == this.tsmiCloseLeft) {
                    int index = this.tabControl1.TabPages.IndexOf(this.ContextMenuedTab);
                    if(index > 0) {
                        this.CloseLeftRight(true, index);
                    }
                }
                else if(e.ClickedItem == this.tsmiCloseRight) {
                    int num2 = this.tabControl1.TabPages.IndexOf(this.ContextMenuedTab);
                    if(num2 >= 0) {
                        this.CloseLeftRight(false, num2);
                    }
                }
                else if(e.ClickedItem == this.tsmiCreateGroup) {
                    this.CreateGroup(this.ContextMenuedTab);
                }
                else if(e.ClickedItem == this.tsmiLockThis) {
                    this.ContextMenuedTab.TabLocked = !this.ContextMenuedTab.TabLocked;
                }
                else if(e.ClickedItem == this.tsmiCloneThis) {
                    this.CloneTabButton(this.ContextMenuedTab, null, true, -1);
                }
                else if(e.ClickedItem == this.tsmiCreateWindow) {
                    using(IDLWrapper wrapper = new IDLWrapper(this.ContextMenuedTab.CurrentIDL)) {
                        this.OpenNewWindow(wrapper);
                    }
                    if(!QTUtility.CheckConfig(Settings.KeepOnSeparate) != ((Control.ModifierKeys & Keys.Shift) != Keys.None)) {
                        this.CloseTab(this.ContextMenuedTab);
                    }
                }
                else if(e.ClickedItem == this.tsmiCopy) {
                    string currentPath = this.ContextMenuedTab.CurrentPath;
                    if(currentPath.IndexOf("???") != -1) {
                        currentPath = currentPath.Substring(0, currentPath.IndexOf("???"));
                    }
                    else if(currentPath.IndexOf("*?*?*") != -1) {
                        currentPath = currentPath.Substring(0, currentPath.IndexOf("*?*?*"));
                    }
                    SetStringClipboard(currentPath);
                }
                else if(e.ClickedItem == this.tsmiProp) {
                    ShellMethods.ShowProperties(this.ContextMenuedTab.CurrentIDL);
                }
            }
        }

        private void contextMenuTab_Opening(object sender, CancelEventArgs e) {
            this.InitializeTabMenu(false);
            int index = this.tabControl1.TabPages.IndexOf(this.ContextMenuedTab);
            if((index == -1) || (this.ContextMenuedTab == null)) {
                e.Cancel = true;
            }
            else {
                this.tabControl1.SetContextMenuState(true);
                this.contextMenuTab.SuspendLayout();
                if(this.tabControl1.TabCount == 1) {
                    this.tsmiTabOrder.Enabled = this.tsmiCloseAllButThis.Enabled = this.tsmiCloseLeft.Enabled = this.tsmiCloseRight.Enabled = false;
                }
                else {
                    if(index == 0) {
                        this.tsmiCloseLeft.Enabled = false;
                        this.tsmiCloseRight.Enabled = true;
                    }
                    else if(index == (this.tabControl1.TabCount - 1)) {
                        this.tsmiCloseLeft.Enabled = true;
                        this.tsmiCloseRight.Enabled = false;
                    }
                    else {
                        this.tsmiCloseLeft.Enabled = this.tsmiCloseRight.Enabled = true;
                    }
                    this.tsmiTabOrder.Enabled = this.tsmiCloseAllButThis.Enabled = true;
                }
                this.tsmiClose.Enabled = !this.ContextMenuedTab.TabLocked;
                this.tsmiLockThis.Text = this.ContextMenuedTab.TabLocked ? QTUtility.ResMain[20] : QTUtility.ResMain[6];
                QTUtility.RefreshGroupsDic();
                if(QTUtility.GroupPathsDic.Count > 0) {
                    this.tsmiAddToGroup.DropDown.SuspendLayout();
                    this.tsmiAddToGroup.Enabled = true;
                    while(this.tsmiAddToGroup.DropDownItems.Count > 0) {
                        this.tsmiAddToGroup.DropDownItems[0].Dispose();
                    }
                    foreach(string str in QTUtility.GroupPathsDic.Keys) {
                        string str2 = QTUtility.GroupPathsDic[str];
                        if(!string.IsNullOrEmpty(str2)) {
                            ToolStripMenuItem item = new ToolStripMenuItem(str);
                            item.ImageKey = QTUtility.GetImageKey(str2.Split(QTUtility.SEPARATOR_CHAR)[0], null);
                            this.tsmiAddToGroup.DropDownItems.Add(item);
                        }
                    }
                    this.tsmiAddToGroup.DropDown.ResumeLayout();
                }
                else {
                    this.tsmiAddToGroup.Enabled = false;
                }
                this.tsmiHistory.DropDown.SuspendLayout();
                while(this.tsmiHistory.DropDownItems.Count > 0) {
                    this.tsmiHistory.DropDownItems[0].Dispose();
                }
                if((this.ContextMenuedTab.HistoryCount_Back + this.ContextMenuedTab.HistoryCount_Forward) > 1) {
                    this.tsmiHistory.DropDownItems.AddRange(this.CreateNavBtnMenuItems(false).ToArray());
                    this.tsmiHistory.DropDownItems.AddRange(this.CreateBranchMenu(false, this.components, new ToolStripItemClickedEventHandler(this.tsmiBranchRoot_DropDownItemClicked)).ToArray());
                    this.tsmiHistory.Enabled = true;
                }
                else {
                    this.tsmiHistory.Enabled = false;
                }
                this.tsmiHistory.DropDown.ResumeLayout();
                this.contextMenuTab.Items.Remove(this.menuTextBoxTabAlias);
                if(QTUtility.CheckConfig(Settings.NoRenameAmbTabs)) {
                    this.contextMenuTab.Items.Insert(12, this.menuTextBoxTabAlias);
                    if(this.ContextMenuedTab.Comment.Length > 0) {
                        this.menuTextBoxTabAlias.Text = this.ContextMenuedTab.Comment;
                        this.menuTextBoxTabAlias.ForeColor = SystemColors.WindowText;
                    }
                    else {
                        this.menuTextBoxTabAlias.Text = QTUtility.ResMain[0x1b];
                        this.menuTextBoxTabAlias.ForeColor = SystemColors.GrayText;
                    }
                    this.menuTextBoxTabAlias.Enabled = !this.tabControl1.AutoSubText;
                }
                if(this.tsmiTabOrder.DropDownItems.Count == 0) {
                    ((ToolStripDropDownMenu)this.tsmiTabOrder.DropDown).ShowImageMargin = false;
                    ToolStripMenuItem item2 = new ToolStripMenuItem(QTUtility.ResMain[0x1d]);
                    ToolStripMenuItem item3 = new ToolStripMenuItem(QTUtility.ResMain[30]);
                    ToolStripMenuItem item4 = new ToolStripMenuItem(QTUtility.ResMain[0x1f]);
                    ToolStripSeparator separator = new ToolStripSeparator();
                    ToolStripMenuItem item5 = new ToolStripMenuItem(QTUtility.ResMain[0x22]);
                    item2.Name = "Name";
                    item3.Name = "Drive";
                    item4.Name = "Active";
                    separator.Enabled = false;
                    item5.Name = "Rev";
                    this.tsmiTabOrder.DropDownItems.Add(item2);
                    this.tsmiTabOrder.DropDownItems.Add(item3);
                    this.tsmiTabOrder.DropDownItems.Add(item4);
                    this.tsmiTabOrder.DropDownItems.Add(separator);
                    this.tsmiTabOrder.DropDownItems.Add(item5);
                    this.tsmiTabOrder.DropDownItemClicked += new ToolStripItemClickedEventHandler(this.menuitemTabOrder_DropDownItemClicked);
                }
                if((this.lstPluginMenuItems_Tab != null) && (this.lstPluginMenuItems_Tab.Count > 0)) {
                    foreach(ToolStripItem item6 in this.lstPluginMenuItems_Tab) {
                        item6.Dispose();
                    }
                    this.lstPluginMenuItems_Tab = null;
                }
                if((this.pluginManager != null) && (this.pluginManager.dicFullNamesMenuRegistered_Tab.Count > 0)) {
                    this.lstPluginMenuItems_Tab = new List<ToolStripItem>();
                    int num2 = this.contextMenuTab.Items.IndexOf(this.tsmiProp);
                    ToolStripSeparator separator2 = new ToolStripSeparator();
                    this.contextMenuTab.Items.Insert(num2, separator2);
                    foreach(string str3 in this.pluginManager.dicFullNamesMenuRegistered_Tab.Keys) {
                        ToolStripMenuItem item7 = new ToolStripMenuItem(this.pluginManager.dicFullNamesMenuRegistered_Tab[str3]);
                        item7.Name = str3;
                        item7.Tag = MenuType.Tab;
                        item7.Click += new EventHandler(this.pluginitems_Click);
                        this.contextMenuTab.Items.Insert(num2, item7);
                        this.lstPluginMenuItems_Tab.Add(item7);
                    }
                    this.lstPluginMenuItems_Tab.Add(separator2);
                }
                this.contextMenuTab.ResumeLayout();
            }
        }

        private void Controls_GotFocus(object sender, EventArgs e) {
            this.OnGotFocus(e);
        }

        internal List<ToolStripItem> CreateBranchMenu(bool fCurrent, IContainer container, ToolStripItemClickedEventHandler itemClickedEvent) {
            QTabItem item = fCurrent ? this.CurrentTab : this.ContextMenuedTab;
            List<ToolStripItem> list = new List<ToolStripItem>();
            List<LogData> branches = item.Branches;
            if(branches.Count > 0) {
                ToolStripMenuItem item2 = new ToolStripMenuItem(QTUtility.ResMain[0x18]);
                item2.Tag = item;
                item2.DropDown = new DropDownMenuBase(container, true, true);
                item2.DropDown.ImageList = QTUtility.ImageListGlobal;
                item2.DropDownItemClicked += itemClickedEvent;
                int index = -1;
                foreach(LogData data in branches) {
                    index++;
                    if(IsSpecialFolderNeedsToTravel(data.Path)) {
                        if(this.LogEntryDic.ContainsKey(data.Hash)) {
                            goto Label_00B3;
                        }
                        continue;
                    }
                    if(!QTUtility2.PathExists(data.Path)) {
                        continue;
                    }
                Label_00B3:
                    item2.DropDownItems.Add(MenuUtility.CreateMenuItem(new MenuItemArguments(data.Path, false, index, MenuGenre.Branch)));
                }
                if(item2.DropDownItems.Count > 0) {
                    list.Add(new ToolStripSeparator());
                    list.Add(item2);
                }
            }
            return list;
        }

        private static void CreateContextMenuItems_NotifyIcon(IntPtr hwnd, int sw) {
            contextMenuNotifyIcon.Invoke(new NavigationCompleteCallback(QTTabBarClass.CreateContextMenuItems_NotifyIcon_Core), new object[] { sw, hwnd });
        }

        private static void CreateContextMenuItems_NotifyIcon_Core(object sw, IntPtr hwndExplr) {
            if(hwndExplr != IntPtr.Zero) {
                dicNotifyIcon[hwndExplr] = (int)sw;
            }
            contextMenuNotifyIcon.Hide();
            contextMenuNotifyIcon.SuspendLayout();
            contextMenuNotifyIcon.Items.Clear();
            foreach(IntPtr ptr in dicNotifyIcon.Keys) {
                StringBuilder lpString = new StringBuilder(260);
                PInvoke.GetWindowText(ptr, lpString, lpString.Capacity);
                ToolStripMenuItem item = new ToolStripMenuItem(lpString.ToString());
                item.Tag = ptr;
                QTTabBarClass tabBar = QTUtility.instanceManager.GetTabBar(ptr);
                if(tabBar != null) {
                    string currentAddress = tabBar.CurrentAddress;
                    if(currentAddress.Length > 0) {
                        item.ToolTipText = QTUtility2.MakePathDisplayText(currentAddress, true);
                        item.ImageKey = QTUtility.GetImageKey(currentAddress, null);
                    }
                    if(tabBar.tabControl1.TabCount > 1) {
                        for(int i = 0; i < tabBar.tabControl1.TabCount; i++) {
                            QTabItem item2 = (QTabItem)tabBar.tabControl1.TabPages[i];
                            ToolStripMenuItem item3 = new ToolStripMenuItem(item2.Text);
                            item3.Tag = ptr;
                            item3.ToolTipText = QTUtility2.MakePathDisplayText(item2.CurrentPath, true);
                            item3.ImageKey = QTUtility.GetImageKey(item2.CurrentPath, null);
                            item.DropDownItems.Add(item3);
                        }
                        if(item.DropDownItems.Count > 0) {
                            item.DropDownItemClicked += new ToolStripItemClickedEventHandler(QTTabBarClass.contextMenuNotifyIcon_SubItems_DropDownItemClicked);
                            item.DropDown.ImageList = QTUtility.ImageListGlobal;
                        }
                    }
                }
                contextMenuNotifyIcon.Items.Add(item);
            }
            contextMenuNotifyIcon.Items.Add(new ToolStripSeparator());
            contextMenuNotifyIcon.Items.Add("Restore all");
            contextMenuNotifyIcon.Items.Add("Close all");
            contextMenuNotifyIcon.ResumeLayout();
        }

        private static Cursor CreateCursor(Bitmap bmpColor) {
            Cursor cursor;
            using(bmpColor) {
                using(Bitmap bitmap = new Bitmap(0x20, 0x20)) {
                    ICONINFO piconinfo = new ICONINFO();
                    piconinfo.fIcon = false;
                    piconinfo.hbmColor = bmpColor.GetHbitmap();
                    piconinfo.hbmMask = bitmap.GetHbitmap();
                    try {
                        cursor = new Cursor(PInvoke.CreateIconIndirect(ref piconinfo));
                    }
                    catch {
                        cursor = Cursors.Default;
                    }
                }
            }
            return cursor;
        }

        private void CreateGroup(QTabItem contextMenuedTab) {
            this.NowModalDialogShown = true;
            QTUtility.RefreshGroupsDic();
            using(CreateNewGroupForm form = new CreateNewGroupForm(contextMenuedTab.CurrentPath, this.tabControl1.TabPages)) {
                if(this.NowTopMost) {
                    form.TopMost = true;
                }
                if(DialogResult.OK == form.ShowDialog()) {
                    QTUtility.SaveGroupsReg();
                    SyncButtonBarBroadCast(1);
                    SyncTaskBarMenu();
                }
            }
            this.NowModalDialogShown = false;
        }

        internal List<QMenuItem> CreateNavBtnMenuItems(bool fCurrent) {
            QTabItem item = fCurrent ? this.CurrentTab : this.ContextMenuedTab;
            List<QMenuItem> list = new List<QMenuItem>();
            string[] historyBack = item.GetHistoryBack();
            string[] historyForward = item.GetHistoryForward();
            if((historyBack.Length + historyForward.Length) > 1) {
                for(int i = historyBack.Length - 1; i >= 0; i--) {
                    QMenuItem item2 = MenuUtility.CreateMenuItem(new MenuItemArguments(historyBack[i], true, i, MenuGenre.Navigation));
                    if(IsSpecialFolderNeedsToTravel(historyBack[i])) {
                        item2.Enabled = this.LogEntryDic.ContainsKey(item.GetLogHash(true, i));
                    }
                    else if(!QTUtility2.PathExists(historyBack[i])) {
                        item2.Enabled = false;
                    }
                    if(item2.Enabled && (i == 0)) {
                        item2.BackColor = QTUtility2.MakeModColor(SystemColors.Highlight);
                    }
                    list.Add(item2);
                }
                for(int j = 0; j < historyForward.Length; j++) {
                    QMenuItem item3 = MenuUtility.CreateMenuItem(new MenuItemArguments(historyForward[j], false, j, MenuGenre.Navigation));
                    if(IsSpecialFolderNeedsToTravel(historyForward[j])) {
                        item3.Enabled = this.LogEntryDic.ContainsKey(item.GetLogHash(false, j));
                    }
                    else if(!QTUtility2.PathExists(historyForward[j])) {
                        item3.Enabled = false;
                    }
                    list.Add(item3);
                }
            }
            return list;
        }

        private QTabItem CreateNewTab(IDLWrapper idlw) {
            string path = idlw.Path;
            QTabItem tab = new QTabItem(QTUtility2.MakePathDisplayText(path, false), path, this.tabControl1);
            tab.NavigatedTo(path, idlw.IDL, -1);
            tab.ToolTipText = QTUtility2.MakePathDisplayText(path, true);
            this.AddInsertTab(tab);
            return tab;
        }

        private void CreateRebarImage() {
            if(this.bmpRebar != null) {
                this.bmpRebar.Dispose();
                this.bmpRebar = null;
            }
            if(this.textureBrushRebar != null) {
                this.textureBrushRebar.Dispose();
                this.textureBrushRebar = null;
            }
            if(File.Exists(QTUtility.Path_RebarImage)) {
                try {
                    using(Bitmap bitmap = new Bitmap(QTUtility.Path_RebarImage)) {
                        this.bmpRebar = new Bitmap(bitmap, bitmap.Size);
                        this.textureBrushRebar = new TextureBrush(this.bmpRebar);
                        if(string.Equals(Path.GetExtension(QTUtility.Path_RebarImage), ".bmp", StringComparison.OrdinalIgnoreCase)) {
                            this.bmpRebar.MakeTransparent(System.Drawing.Color.Magenta);
                        }
                    }
                }
                catch {
                }
            }
        }

        internal static Bitmap[] CreateTabImage() {
            if(File.Exists(QTUtility.Path_TabImage)) {
                try {
                    Bitmap[] bitmapArray = new Bitmap[3];
                    using(Bitmap bitmap = new Bitmap(QTUtility.Path_TabImage)) {
                        int height = bitmap.Height / 3;
                        bitmapArray[0] = bitmap.Clone(new Rectangle(0, 0, bitmap.Width, height), PixelFormat.Format32bppArgb);
                        bitmapArray[1] = bitmap.Clone(new Rectangle(0, height, bitmap.Width, height), PixelFormat.Format32bppArgb);
                        bitmapArray[2] = bitmap.Clone(new Rectangle(0, height * 2, bitmap.Width, height), PixelFormat.Format32bppArgb);
                    }
                    if(string.Equals(Path.GetExtension(QTUtility.Path_TabImage), ".bmp", StringComparison.OrdinalIgnoreCase)) {
                        bitmapArray[0].MakeTransparent(System.Drawing.Color.Magenta);
                        bitmapArray[1].MakeTransparent(System.Drawing.Color.Magenta);
                        bitmapArray[2].MakeTransparent(System.Drawing.Color.Magenta);
                    }
                    return bitmapArray;
                }
                catch {
                }
            }
            return null;
        }

        private static List<string> CreateTMPPathsToOpenNew(Address[] addresses, string pathExclude) {
            List<string> list = new List<string>();
            QTUtility2.InitializeTemporaryPaths();
            for(int i = 0; i < addresses.Length; i++) {
                try {
                    using(IDLWrapper wrapper = new IDLWrapper(addresses[i].ITEMIDLIST)) {
                        if(wrapper.Available && wrapper.HasPath) {
                            string path = wrapper.Path;
                            if((((path.Length > 0) && !string.Equals(path, pathExclude, StringComparison.OrdinalIgnoreCase)) && (!QTUtility2.IsShellPathButNotFileSystem(path) && wrapper.IsFolder)) && !wrapper.IsLinkToDeadFolder) {
                                list.Add(path);
                            }
                        }
                    }
                }
                catch {
                }
            }
            return list;
        }

        private void ddmrUndoClose_ItemRightClicked(object sender, ItemRightClickedEventArgs e) {
            QMenuItem clickedItem = e.ClickedItem as QMenuItem;
            if(clickedItem != null) {
                using(IDLWrapper wrapper = new IDLWrapper(clickedItem.Path)) {
                    e.HRESULT = ShellMethods.PopUpSystemContextMenu(wrapper, e.IsKey ? e.Point : Control.MousePosition, ref this.iContextMenu2, ((DropDownMenuReorderable)sender).Handle, true);
                }
                if(e.HRESULT == 0xffff) {
                    QTUtility.ClosedTabHistoryList.Remove(clickedItem.Path);
                    e.ClickedItem.Dispose();
                }
            }
        }

        private void ddrmrGroups_ItemMiddleClicked(object sender, ItemRightClickedEventArgs e) {
            this.ReplaceByGroup(e.ClickedItem.Text);
        }

        protected override void Dispose(bool disposing) {
            if(disposing && (this.components != null)) {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        internal bool DoFileTools(int index) {
            try {
                Address[] addressArray;
                List<string> list;
                int num;
                string displayName = string.Empty;
                switch(index) {
                    case 0:
                    case 1:
                    case 4:
                        string str2;
                        if(!this.TryGetSelection(out addressArray, out str2, index == 1)) {
                            goto Label_019C;
                        }
                        list = new List<string>();
                        num = 0;
                        goto Label_00A1;

                    case 2:
                    case 3: {
                            using(IDLWrapper wrapper = new IDLWrapper(ShellMethods.ShellGetPath(this.ShellBrowser))) {
                                if(wrapper.Available) {
                                    displayName = ShellMethods.GetDisplayName(wrapper.PIDL, index == 3);
                                }
                                goto Label_019C;
                            }
                        }
                    case 5:
                        foreach(QTabItem item in this.tabControl1.TabPages) {
                            string currentPath = item.CurrentPath;
                            int length = currentPath.IndexOf("???");
                            if(length != -1) {
                                currentPath = currentPath.Substring(0, length);
                            }
                            int num3 = currentPath.IndexOf("*?*?*");
                            if(num3 != -1) {
                                currentPath = currentPath.Substring(0, num3);
                            }
                            displayName = displayName + ((displayName.Length == 0) ? string.Empty : "\r\n") + currentPath;
                        }
                        goto Label_019C;

                    default:
                        goto Label_019C;
                }
            Label_004B:
                if(addressArray[num].Path != null) {
                    if(index != 4) {
                        displayName = displayName + ((displayName.Length == 0) ? string.Empty : "\r\n") + addressArray[num].Path;
                    }
                    else {
                        list.Add(addressArray[num].Path);
                    }
                }
                num++;
            Label_00A1:
                if(num < addressArray.Length) {
                    goto Label_004B;
                }
                if(index == 4) {
                    ShowMD5(list.ToArray());
                    return true;
                }
            Label_019C:
                if(displayName.Length > 0) {
                    SetStringClipboard(displayName);
                    return true;
                }
            }
            catch {
            }
            if(index == 4) {
                ShowMD5(null);
                return true;
            }
            return false;
        }

        // This function is either called by BeforeNavigate2 (on XP and Vista)
        // or NavigateComplete2 (on 7)
        private void DoFirstNavigation(bool before, string path) {

            if((QTUtility.TMPPathList.Count > 0) || (QTUtility.TMPIDLList.Count > 0)) {
                // Loads tmp paths?
                foreach(string str2 in QTUtility.TMPPathList) {
                    if(!string.Equals(str2, path, StringComparison.OrdinalIgnoreCase)) {
                        using(IDLWrapper wrapper = new IDLWrapper(str2)) {
                            if(wrapper.Available) {
                                this.CreateNewTab(wrapper);
                            }
                            continue;
                        }
                    }
                }
                foreach(byte[] buffer in QTUtility.TMPIDLList) {
                    if(QTUtility.TMPTargetIDL != buffer) {
                        using(IDLWrapper wrapper2 = new IDLWrapper(buffer)) {
                            this.OpenNewTab(wrapper2, true, false);
                            continue;
                        }
                    }
                    QTUtility.TMPTargetIDL = null;
                }
                QTUtility2.InitializeTemporaryPaths();
                this.AddStartUpTabs(string.Empty, path);
                this.InitializeOpenedWindow();
            }
            else if(QTUtility.CreateWindowTMPGroup.Length != 0) {
                // Loads tmp group?
                string createWindowTMPGroup = QTUtility.CreateWindowTMPGroup;
                QTUtility.CreateWindowTMPGroup = string.Empty;
                this.CurrentTab.CurrentPath = path;
                this.NowOpenedByGroupOpener = true;
                this.OpenGroup(createWindowTMPGroup, false);
                this.AddStartUpTabs(createWindowTMPGroup, path);
                this.InitializeOpenedWindow();
            }
            else if(QTUtility.CheckConfig(Settings.NoTabsFromOutside) || (QTUtility.CreateWindowTMPPath == path)) {
                // What config is that?
                QTUtility.CreateWindowTMPPath = string.Empty;
                this.AddStartUpTabs(string.Empty, path);
                this.InitializeOpenedWindow();
            }
            else if((path.StartsWith(QTUtility.ResMisc[0]) || (path.EndsWith(QTUtility.ResMisc[0]) && QTUtility2.IsShellPathButNotFileSystem(path))) || string.Equals(path, QTUtility.PATH_SEARCHFOLDER, StringComparison.OrdinalIgnoreCase)) {
                this.InitializeOpenedWindow();
            }
            else {
                for(int i = 0; i < QTUtility.NoCapturePathsList.Count; i++) {
                    if(string.Equals(QTUtility.NoCapturePathsList[i], path, StringComparison.OrdinalIgnoreCase)) {
                        this.InitializeOpenedWindow();
                        return;
                    }
                }
                if(QTUtility.InstancesCount > 1) {
                    if((Control.ModifierKeys == Keys.Control) || !QTUtility.instanceManager.NextInstanceExists()) {
                        this.InitializeOpenedWindow();
                        this.AddStartUpTabs(string.Empty, path);
                    }
                    else {
                        if(QTUtility.IsVista) {
                            PInvoke.SetWindowPos(this.ExplorerHandle, IntPtr.Zero, 0, 0, 0, 0, 0x259f);
                        }
                        NavigateOnOldWindow(path);
                        this.fNowQuitting = true;
                        if(QTUtility.IsVista) {
                            base.Explorer.Quit();
                        }
                        else {
                            WindowUtils.CloseExplorer(this.ExplorerHandle, 0);
                        }
                    }
                }
                else {
                    if(!QTUtility.CheckConfig(Settings.DontCaptureNewWnds)) {
                        uint num2;
                        uint num3;
                        PInvoke.GetWindowThreadProcessId(base.Handle, out num2);
                        PInvoke.GetWindowThreadProcessId(WindowUtils.GetShellTrayWnd(), out num3);
                        if(((num2 != num3) && (num2 != 0)) && (num3 != 0)) {
                            string nameToSelectFromCommandLineArg = GetNameToSelectFromCommandLineArg();
                            if(!WindowUtils.IsExplorerProcessSeparated()) {
                                bool flag = false;
                                using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar")) {
                                    IntPtr hWnd = QTUtility2.ReadRegHandle("Handle", key);
                                    if(PInvoke.IsWindow(hWnd)) {
                                        string strMsg = path + ((nameToSelectFromCommandLineArg.Length > 0) ? (";" + nameToSelectFromCommandLineArg) : string.Empty);
                                        QTUtility2.SendCOPYDATASTRUCT(hWnd, new IntPtr(15), strMsg, IntPtr.Zero);
                                        flag = true;
                                    }
                                    else {
                                        IntPtr ptr2 = QTUtility2.ReadRegHandle("TaskBarHandle", key);
                                        if(PInvoke.IsWindow(ptr2)) {
                                            string str6 = path + ((nameToSelectFromCommandLineArg.Length > 0) ? (";" + nameToSelectFromCommandLineArg) : string.Empty);
                                            QTUtility2.SendCOPYDATASTRUCT(ptr2, IntPtr.Zero, str6, (IntPtr)num2);
                                            flag = true;
                                        }
                                    }
                                }
                                if(flag) {
                                    this.fNowQuitting = true;
                                    base.Explorer.Quit();
                                    return;
                                }
                            }
                            QTUtility.PathToSelectInCommandLineArg = nameToSelectFromCommandLineArg;
                        }
                    }
                    this.AddStartUpTabs(string.Empty, path);
                    this.InitializeOpenedWindow();
                }
            }
        }

        private int dropTargetWrapper_DragFileDrop(out IntPtr hwnd, out byte[] idlReal) {
            this.HideToolTipForDD();
            hwnd = this.tabControl1.Handle;
            idlReal = null;
            QTabItem tabMouseOn = (QTabItem)this.tabControl1.GetTabMouseOn();
            if((tabMouseOn == null) || !QTUtility.CheckConfig(Settings.DragDropOntoTabs)) {
                return 1;
            }
            if((tabMouseOn.CurrentIDL != null) && (tabMouseOn.CurrentIDL.Length > 0)) {
                idlReal = tabMouseOn.CurrentIDL;
                return 0;
            }
            return -1;
        }

        private DragDropEffects dropTargetWrapper_DragFileEnter(IntPtr hDrop, BandObjectLib.POINT pnt, int grfKeyState) {
            if(QTUtility.CheckConfig(Settings.DragDropOntoTabs)) {
                int num = HandleDragEnter(hDrop, out this.strDraggingDrive, out this.strDraggingStartPath);
                this.fDrivesContainedDD = num == 2;
                if(num == -1) {
                    return DragDropEffects.None;
                }
                if(this.tabControl1.GetTabMouseOn() == null) {
                    return DragDropEffects.Copy;
                }
                switch(num) {
                    case 0:
                        return DropTargetWrapper.MakeEffect(grfKeyState, 0);

                    case 1:
                        return DropTargetWrapper.MakeEffect(grfKeyState, 1);

                    case 2:
                        return DragDropEffects.None;
                }
            }
            return DragDropEffects.Copy;
        }

        private void dropTargetWrapper_DragFileLeave(object sender, EventArgs e) {
            this.HideToolTipForDD();
            this.strDraggingDrive = null;
            this.strDraggingStartPath = null;
            this.tabControl1.Refresh();
        }

        private void dropTargetWrapper_DragFileOver(object sender, DragEventArgs e) {
            e.Effect = DragDropEffects.None;
            QTabItem tabMouseOn = (QTabItem)this.tabControl1.GetTabMouseOn();
            bool flag = true;
            if(tabMouseOn != this.tabForDD) {
                this.tabControl1.Refresh();
                this.HideSubDirTip_Tab_Menu();
                this.fToggleTabMenu = false;
                flag = false;
            }
            if(tabMouseOn == null) {
                e.Effect = DragDropEffects.Copy;
            }
            else if(tabMouseOn.CurrentPath.Length > 2) {
                if(!this.fDrivesContainedDD && !string.Equals(this.strDraggingStartPath, tabMouseOn.CurrentPath, StringComparison.OrdinalIgnoreCase)) {
                    using(IDLWrapper wrapper = new IDLWrapper(tabMouseOn.CurrentIDL, !flag)) {
                        int num;
                        if(!wrapper.Available || !wrapper.IsDropTarget) {
                            goto Label_013B;
                        }
                        string b = tabMouseOn.CurrentPath.Substring(0, 3);
                        if((this.strDraggingDrive != null) && string.Equals(this.strDraggingDrive, b, StringComparison.OrdinalIgnoreCase)) {
                            num = 0;
                        }
                        else {
                            num = 1;
                        }
                        this.ShowToolTipForDD(tabMouseOn, num, e.KeyState);
                        if(!QTUtility.CheckConfig(Settings.DragDropOntoTabs)) {
                            e.Effect = DragDropEffects.Copy;
                        }
                        else {
                            e.Effect = DropTargetWrapper.MakeEffect(e.KeyState, num);
                        }
                        return;
                    }
                }
                if(this.toolTipForDD != null) {
                    this.toolTipForDD.Hide(this.tabControl1);
                }
                this.ShowToolTipForDD(tabMouseOn, -1, e.KeyState);
                return;
            }
        Label_013B:
            this.HideToolTipForDD();
        }

        private void Explorer_DownloadBegin() {
            bool dummy = false;
            BeforeNavigate("", ref dummy);
        }

        private void Explorer_BeforeNavigate2(object pDisp, ref object URL, ref object Flags, ref object TargetFrameName, ref object PostData, ref object Headers, ref bool Cancel) {
            if(this.IsShown) {
                BeforeNavigate((string)URL, ref Cancel);
            } 
            else {
                DoFirstNavigation(true, (string)URL);
            }
        }

        private void Explorer_NavigateComplete2(object pDisp, ref object URL) {
            this.NowNavigating = false;
            string path = (string)URL;
            if(!this.IsShown) {
                DoFirstNavigation(false, path);
            }

            if(this.fNowQuitting) {
                base.Explorer.Quit();
            }
            else {
                int hash = -1;
                bool flag = IsSpecialFolderNeedsToTravel(path);
                bool flag2 = QTUtility2.IsShellPathButNotFileSystem(path);
                if(!this.NavigatedByCode && flag) {
                    hash = DateTime.Now.GetHashCode();
                    this.LogEntryDic[hash] = this.GetCurrentLogEntry();
                }
                this.ClearTravelLogs();
                try {
                    this.tabControl1.SetRedraw(false);
                    if(this.fNowTravelByTree) {
                        using(IDLWrapper wrapper = new IDLWrapper(this.GetCurrentPIDL())) {
                            QTabItem tabPage = this.CreateNewTab(wrapper);
                            this.tabControl1.SelectTabDirectly(tabPage);
                            this.CurrentTab = tabPage;
                        }
                    }
                    if(this.tabControl1.AutoSubText && !this.fNavigatedByTabSelection) {
                        this.CurrentTab.Comment = string.Empty;
                    }
                    this.CurrentAddress = path;
                    this.CurrentTab.Text = base.Explorer.LocationName;
                    this.CurrentTab.CurrentIDL = null;
                    this.CurrentTab.TooltipText2 = null;
                    byte[] idl = null;
                    using(IDLWrapper wrapper2 = new IDLWrapper(this.GetCurrentPIDL())) {
                        this.CurrentTab.CurrentIDL = idl = wrapper2.IDL;
                        if(flag) {
                            if((!this.NavigatedByCode && (idl != null)) && (idl.Length > 0)) {
                                path = path + "*?*?*" + hash;
                                QTUtility.ITEMIDLIST_Dic_Session[path] = idl;
                                this.CurrentTab.CurrentPath = this.CurrentAddress = path;
                            }
                        }
                        else if((flag2 && wrapper2.Available) && !this.CurrentTab.CurrentPath.Contains("???")) {
                            string str2;
                            int num2;
                            if(IDLWrapper.GetIDLHash(wrapper2.PIDL, out num2, out str2)) {
                                hash = num2;
                                this.CurrentTab.CurrentPath = this.CurrentAddress = path = str2;
                            }
                            else if((idl != null) && (idl.Length > 0)) {
                                hash = num2;
                                path = path + "???" + hash;
                                IDLWrapper.AddCache(path, idl);
                                this.CurrentTab.CurrentPath = this.CurrentAddress = path;
                            }
                        }
                        if(!this.NavigatedByCode) {
                            this.CurrentTab.NavigatedTo(this.CurrentAddress, idl, hash);
                        }
                    }
                    this.SyncTravelState();
                    if(!QTUtility.IsVista) {
                        if(this.CurrentAddress.StartsWith(QTUtility.PATH_SEARCHFOLDER)) {
                            this.ShowSearchBar(true);
                        }
                        else if(QTUtility.fExplorerPrevented || QTUtility.fRestoreFolderTree) {
                            if(!QTUtility.CheckConfig(Settings.NoNewWndFolderTree) || QTUtility.fRestoreFolderTree) {
                                this.ShowFolderTree(true);
                            }
                            QTUtility.fExplorerPrevented = QTUtility.fRestoreFolderTree = false;
                        }
                    }
                    if(this.CurrentAddress.StartsWith("::")) {
                        this.CurrentTab.ToolTipText = this.CurrentTab.Text;
                        QTUtility.DisplayNameCacheDic[this.CurrentAddress] = this.CurrentTab.Text;
                    }
                    else if(flag2) {
                        this.CurrentTab.ToolTipText = (string)URL;
                    }
                    else if(((this.CurrentAddress.Length == 3) || this.CurrentAddress.StartsWith(@"\\")) || (this.CurrentAddress.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || this.CurrentAddress.StartsWith("ftp://", StringComparison.OrdinalIgnoreCase))) {
                        this.CurrentTab.ToolTipText = this.CurrentTab.CurrentPath;
                        QTUtility.DisplayNameCacheDic[this.CurrentAddress] = this.CurrentTab.Text;
                    }
                    else {
                        this.CurrentTab.ToolTipText = this.CurrentTab.CurrentPath;
                    }
                    if(this.NavigatedByCode && !this.NowTabCreated) {
                        string str3;
                        Address[] selectedItemsAt = this.CurrentTab.GetSelectedItemsAt(this.CurrentAddress, out str3);
                        if(selectedItemsAt != null) {
                            this.TrySetSelection(selectedItemsAt, str3, true);
                        }
                    }
                    else if(!string.IsNullOrEmpty(QTUtility.PathToSelectInCommandLineArg)) {
                        Address[] addresses = new Address[] { new Address(QTUtility.PathToSelectInCommandLineArg) };
                        this.TrySetSelection(addresses, null, true);
                        QTUtility.PathToSelectInCommandLineArg = string.Empty;
                    }
                    if(QTUtility.RestoreFolderTree_Hide) {
                        new WaitTimeoutCallback(QTTabBarClass.WaitTimeout).BeginInvoke(150, new AsyncCallback(this.AsyncComplete_FolderTree), false);
                    }
                    if(this.fNowRestoring) {
                        this.fNowRestoring = false;
                        if(QTUtility.LockedTabsToRestoreList.Contains(path)) {
                            this.CurrentTab.TabLocked = true;
                        }
                    }
                    if((QTUtility.IsVista || this.FirstNavigationCompleted) && (!PInvoke.IsWindowVisible(this.ExplorerHandle) || PInvoke.IsIconic(this.ExplorerHandle))) {
                        WindowUtils.BringExplorerToFront(this.ExplorerHandle);
                    }
                    if(this.pluginManager != null) {
                        this.pluginManager.OnNavigationComplete(this.tabControl1.SelectedIndex, idl, (string)URL);
                    }
                    if(this.buttonNavHistoryMenu.DropDown.Visible) {
                        this.buttonNavHistoryMenu.DropDown.Close(ToolStripDropDownCloseReason.AppFocusChange);
                    }
                    if(QTUtility.CheckConfig(Settings.AutoUpdate)) {
                        UpdateChecker.Check(false);
                    }
                }
                catch(Exception exception) {
                    QTUtility2.MakeErrorLog(exception, null);
                }
                finally {
                    QTUtility.RestoreFolderTree_Hide = this.fIFolderViewNotImplemented = this.NavigatedByCode = this.fNavigatedByTabSelection = this.NowTabCreated = this.fNowTravelByTree = false;
                    this.subDirIndex = this.thumbnailIndex = this.itemIndexDROPHILITED = -1;
                    this.tabControl1.SetRedraw(true);
                    this.FirstNavigationCompleted = true;
                }
            }
        }

        private bool explorerController_MessageCaptured(ref System.Windows.Forms.Message msg) {
            if(msg.Msg == WM.CLOSE) {
                if(this.iSequential_WM_CLOSE > 0) {
                    return true;
                }
                this.iSequential_WM_CLOSE++;
            }
            else {
                this.iSequential_WM_CLOSE = 0;
            }
            int num6 = msg.Msg;
            if(num6 <= WM.NCRBUTTONDOWN) {
                switch(num6) {
                    case WM.SETTINGCHANGE:
                        if(!QTUtility.IsVista) {
                            QTUtility.GetShellClickMode();
                        }
                        if("Environment" == Marshal.PtrToStringUni(msg.LParam)) {
                            QTUtility.fRequiredRefresh_App = true;
                            SyncTaskBarMenu();
                        }
                        goto Label_05CE;

                    case WM.NCLBUTTONDOWN:
                    case WM.NCRBUTTONDOWN:
                        goto Label_05B1;

                    case WM.MOVE:
                    case WM.SIZE:
                        this.HideThumbnailTooltip(0);
                        this.HideSubDirTip(0);
                        goto Label_05CE;

                    case 4: // Uh....  There is no 0x04!
                        goto Label_05CE;

                    case WM.ACTIVATE: {
                            int num3 = ((int)msg.WParam) & 0xffff;
                            if(num3 > 0) {
                                QTUtility.RegisterPrimaryInstance(this.ExplorerHandle, this);
                                if((this.fNowInTray && (notifyIcon != null)) && dicNotifyIcon.ContainsKey(this.ExplorerHandle)) {
                                    ShowTaksbarItem(this.ExplorerHandle, true);
                                }
                                this.fNowInTray = false;
                            }
                            else {
                                this.HideThumbnailTooltip(1);
                                this.HideSubDirTip_ExplorerInactivated();
                                this.HideTabSwitcher(false);
                                if(this.tabControl1.Focused) {
                                    listViewWrapper.SetFocus();
                                }
                                if((QTUtility.CheckConfig(Settings.ShowTabCloseButtons) && QTUtility.CheckConfig(Settings.TabCloseBtnsWithAlt)) && this.tabControl1.EnableCloseButton) {
                                    this.tabControl1.EnableCloseButton = false;
                                    this.tabControl1.Refresh();
                                }
                            }
                            goto Label_05CE;
                        }
                    case WM.CLOSE:
                        return this.HandleCLOSE(msg.LParam);
                }
            }
            else if(num6 <= WM.SYSCOMMAND) {
                switch(num6) {
                    case WM.NCMBUTTONDOWN:
                    case WM.NCXBUTTONDOWN:
                        goto Label_05B1;

                    case WM.SYSCOMMAND:
                        if((((int)msg.WParam) & 0xfff0) == 0xf020) {
                            if(this.pluginManager != null) {
                                this.pluginManager.OnExplorerStateChanged(ExplorerWindowActions.Minimized);
                            }
                            if(QTUtility.CheckConfig(Settings.TrayOnMinimize)) {
                                this.fNowInTray = true;
                                ShowTaksbarItem(this.ExplorerHandle, false);
                                return true;
                            }
                            goto Label_05CE;
                        }
                        if((((int)msg.WParam) & 0xfff0) == 0xf030) {
                            if(this.pluginManager != null) {
                                this.pluginManager.OnExplorerStateChanged(ExplorerWindowActions.Maximized);
                            }
                            goto Label_05CE;
                        }
                        if((((int)msg.WParam) & 0xfff0) == 0xf120) {
                            if(this.pluginManager != null) {
                                this.pluginManager.OnExplorerStateChanged(ExplorerWindowActions.Restored);
                            }
                            goto Label_05CE;
                        }
                        if((QTUtility.CheckConfig(Settings.TrayOnClose) && ((((int)msg.WParam) == 0xf060) || (((int)msg.WParam) == 0xf063))) && (Control.ModifierKeys != Keys.Shift)) {
                            this.fNowInTray = true;
                            ShowTaksbarItem(this.ExplorerHandle, false);
                            return true;
                        }
                        if(QTUtility.IsVista || ((((int)msg.WParam) != 0xf060) && (((int)msg.WParam) != 0xf063))) {
                            goto Label_05CE;
                        }
                        WindowUtils.CloseExplorer(this.ExplorerHandle, 3);
                        return true;
                }
            }
            else {
                switch(num6) {
                    case WM.POWERBROADCAST:
                        if(((int)msg.WParam) == 7) {
                            this.OnAwake();
                        }
                        goto Label_05CE;

                    case WM.DEVICECHANGE:
                        if(((int)msg.WParam) == 0x8004) {
                            DEV_BROADCAST_HDR dev_broadcast_hdr = (DEV_BROADCAST_HDR)Marshal.PtrToStructure(msg.LParam, typeof(DEV_BROADCAST_HDR));
                            if(dev_broadcast_hdr.dbch_devicetype == 2) {
                                DEV_BROADCAST_VOLUME dev_broadcast_volume = (DEV_BROADCAST_VOLUME)Marshal.PtrToStructure(msg.LParam, typeof(DEV_BROADCAST_VOLUME));
                                uint num4 = dev_broadcast_volume.dbcv_unitmask;
                                ushort num5 = 0;
                                while(num5 < 0x1a) {
                                    if((num4 & 1) != 0) {
                                        break;
                                    }
                                    num4 = num4 >> 1;
                                    num5 = (ushort)(num5 + 1);
                                }
                                num5 = (ushort)(num5 + 0x41);
                                string str = ((char)num5) + @":\";
                                List<QTabItem> list = new List<QTabItem>();
                                foreach(QTabItem item in this.tabControl1.TabPages) {
                                    if((item != this.CurrentTab) && item.CurrentPath.StartsWith(str, StringComparison.OrdinalIgnoreCase)) {
                                        list.Add(item);
                                    }
                                }
                                foreach(QTabItem item2 in list) {
                                    this.CloseTab(item2, true);
                                }
                                if((this.CurrentTab != null) && this.CurrentTab.CurrentPath.StartsWith(str, StringComparison.OrdinalIgnoreCase)) {
                                    this.CloseTab(this.CurrentTab, true);
                                }
                                if(this.tabControl1.TabCount == 0) {
                                    WindowUtils.CloseExplorer(this.ExplorerHandle, 2);
                                }
                            }
                        }
                        goto Label_05CE;

                    case WM.PARENTNOTIFY:
                        switch((((int)msg.WParam) & 0xffff)) {
                            case 0x207:
                            case 0x20b:
                            case 0x201:
                            case 0x204:
                                this.HideTabSwitcher(false);
                                goto Label_05CE;
                        }
                        goto Label_05CE;
                }
                if(num6 == WM.APPCOMMAND) {
                    int num = ((((int)((long)msg.LParam)) >> 0x10) & 0xffff) & -61441;
                    int num2 = ((((int)((long)msg.LParam)) >> 0x10) & 0xffff) & 0xf000;
                    bool flag = (num2 != 0x8000) || QTUtility.CheckConfig(Settings.CaptureX1X2);
                    switch(num) {
                        case 1:
                            if(flag) {
                                this.NavigateCurrentTab(true);
                            }
                            return true;

                        case 2:
                            if(flag) {
                                this.NavigateCurrentTab(false);
                            }
                            return true;

                        case 0x1f:
                            WindowUtils.CloseExplorer(this.ExplorerHandle, 0);
                            return true;
                    }
                }
            }
            goto Label_05CE;
        Label_05B1:
            this.HideTabSwitcher(false);
        Label_05CE:
            return false;
        }

        public override void GetBandInfo(uint dwBandID, uint dwViewMode, ref DESKBANDINFO dbi) {
            if((dbi.dwMask & DBIM.ACTUAL) != ((DBIM)0)) {
                dbi.ptActual.X = base.Size.Width;
                dbi.ptActual.Y = this.BandHeight;
            }
            if((dbi.dwMask & DBIM.INTEGRAL) != ((DBIM)0)) {
                dbi.ptIntegral.X = -1;
                dbi.ptIntegral.Y = 10;
            }
            if((dbi.dwMask & DBIM.MAXSIZE) != ((DBIM)0)) {
                dbi.ptMaxSize.X = -1;
                dbi.ptMaxSize.Y = this.BandHeight;
            }
            if((dbi.dwMask & DBIM.MINSIZE) != ((DBIM)0)) {
                dbi.ptMinSize.X = base.MinSize.Width;
                dbi.ptMinSize.Y = this.BandHeight;
            }
            if((dbi.dwMask & DBIM.MODEFLAGS) != ((DBIM)0)) {
                dbi.dwModeFlags = DBIMF.NORMAL;
            }
            if((dbi.dwMask & DBIM.BKCOLOR) != ((DBIM)0)) {
                dbi.dwMask &= ~DBIM.BKCOLOR;
            }
            if((dbi.dwMask & DBIM.TITLE) != ((DBIM)0)) {
                dbi.wszTitle = null;
            }
        }

        private ITravelLogEntry GetCurrentLogEntry() {
            IEnumTravelLogEntry ppenum = null;
            ITravelLogEntry rgElt = null;
            ITravelLogEntry entry3;
            try {
                if(this.TravelLog.EnumEntries(1, out ppenum) == 0) {
                    ppenum.Next(1, out rgElt, 0);
                }
                entry3 = rgElt;
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception, null);
                entry3 = null;
            }
            finally {
                if(ppenum != null) {
                    Marshal.ReleaseComObject(ppenum);
                }
            }
            return entry3;
        }

        internal IntPtr GetCurrentPIDL() {
            IntPtr ptr = ShellMethods.ShellGetPath(this.ShellBrowser);
            if(ptr == IntPtr.Zero) {
                ptr = ShellMethods.ShellGetPath2(this.ExplorerHandle);
            }
            return ptr;
        }

        private Cursor GetCursor(bool fDragging) {
            if(fDragging) {
                if(this.curTabDrag == null) {
                    this.curTabDrag = CreateCursor(Resources_Image.imgCurTabDrag);
                }
                return this.curTabDrag;
            }
            if(this.curTabCloning == null) {
                this.curTabCloning = CreateCursor(Resources_Image.imgCurTabCloning);
            }
            return this.curTabCloning;
        }

        private static string GetNameToSelectFromCommandLineArg() {
            string str = Marshal.PtrToStringUni(PInvoke.GetCommandLine());
            if(!string.IsNullOrEmpty(str)) {
                int index = str.IndexOf("/select,", StringComparison.CurrentCultureIgnoreCase);
                if(index == -1) {
                    index = str.IndexOf(",select,", StringComparison.CurrentCultureIgnoreCase);
                }
                if(index != -1) {
                    index += 8;
                    if(str.Length < index) {
                        return string.Empty;
                    }
                    string path = str.Substring(index).Split(new char[] { ',' })[0].Trim().Trim(new char[] { ' ', '"' });
                    try {
                        if(File.Exists(path) || Directory.Exists(path)) {
                            return Path.GetFileName(path);
                        }
                    }
                    catch {
                    }
                }
            }
            return string.Empty;
        }

        private IntPtr GetSearchBand_Edit() {
            PInvoke.EnumChildWindows(this.ExplorerHandle, new EnumWndProc(this.CallbackEnumChildProc_SearchBand), IntPtr.Zero);
            if(this.hwndSearchBand != IntPtr.Zero) {
                PInvoke.EnumChildWindows(this.hwndSearchBand, new EnumWndProc(this.CallbackEnumChildProc_SearchBand_Edit), IntPtr.Zero);
                return this.hwndSeachBand_Edit;
            }
            return IntPtr.Zero;
        }

        internal IShellBrowser GetShellBrower() {
            return this.ShellBrowser;
        }

        internal static FileSystemInfo GetTargetIfFolderLink(DirectoryInfo di, out bool fTargetIsDirectory) {
            fTargetIsDirectory = true;
            try {
                if((di.Attributes & FileAttributes.ReadOnly) == 0) {
                    return di;
                }
                FileInfo[] files = di.GetFiles();
                if((files.Length != 2) || ((!(files[0].Name == "desktop.ini") || !(files[1].Name == "target.lnk")) && (!(files[0].Name == "target.lnk") || !(files[1].Name == "desktop.ini")))) {
                    return di;
                }
                string lnkPath = (files[1].Name == "target.lnk") ? files[1].FullName : files[0].FullName;
                string linkTargetPath = ShellMethods.GetLinkTargetPath(lnkPath);
                if(string.IsNullOrEmpty(linkTargetPath)) {
                    return di;
                }
                DirectoryInfo info = new DirectoryInfo(linkTargetPath);
                if(info.Exists) {
                    return info;
                }
                if(File.Exists(linkTargetPath)) {
                    fTargetIsDirectory = false;
                    return new FileInfo(linkTargetPath);
                }
            }
            catch {
            }
            return di;
        }

        private IntPtr GetTravelToolBarWindow32() {
            PInvoke.EnumChildWindows(this.ExplorerHandle, new EnumWndProc(this.CallbackEnumChildProc_TravelBand), IntPtr.Zero);
            if(this.hwndTravelBand != IntPtr.Zero) {
                return PInvoke.FindWindowEx(this.hwndTravelBand, IntPtr.Zero, "ToolbarWindow32", null);
            }
            return IntPtr.Zero;
        }

        private void Handle_MButtonUp_Tree(BandObjectLib.MSG msg) {
            IntPtr ptr;
            if((this.ShellBrowser.GetControlWindow(3, out ptr) == 0) && (msg.hwnd == ptr)) {
                BandObjectLib.POINT point;
                TVHITTESTINFO structure = new TVHITTESTINFO();
                point.x = QTUtility2.GET_X_LPARAM(msg.lParam);
                point.y = QTUtility2.GET_Y_LPARAM(msg.lParam);
                structure.pt = point;
                IntPtr ptr2 = Marshal.AllocHGlobal(Marshal.SizeOf(structure));
                Marshal.StructureToPtr(structure, ptr2, false);
                IntPtr wParam = PInvoke.SendMessage(ptr, 0x1111, IntPtr.Zero, ptr2);
                if(wParam != IntPtr.Zero) {
                    int num = (int)PInvoke.SendMessage(ptr, 0x1127, wParam, (IntPtr)2);
                    if((num & 2) == 0) {
                        this.NavigatedByCode = this.fNowTravelByTree = true;
                        PInvoke.SendMessage(ptr, 0x110b, (IntPtr)9, wParam);
                    }
                }
            }
        }

        private bool HandleCLOSE(IntPtr lParam) {
            bool flag = QTUtility.CheckConfig(Settings.NeverCloseWindow);
            bool flag2 = QTUtility.CheckConfig(Settings.NeverCloseWndLocked);
            List<string> closingPaths = new List<string>();
            int num = (int)lParam;
            switch(num) {
                case 1:
                    if(!flag2) {
                        foreach(QTabItem item in this.tabControl1.TabPages) {
                            closingPaths.Add(item.CurrentPath);
                            AddToHistory(item);
                        }
                        break;
                    }
                    closingPaths = this.CloseAllUnlocked();
                    if(this.tabControl1.TabCount <= 0) {
                        break;
                    }
                    return true;

                case 2:
                    return false;

                default: {
                        bool flag3 = QTUtility2.PathExists(this.CurrentTab.CurrentPath);
                        if((!QTUtility.IsVista && flag3) && (num == 0)) {
                            return true;
                        }
                        if(!flag3) {
                            this.CloseTab(this.CurrentTab, true);
                            return (this.tabControl1.TabCount > 0);
                        }
                        if(flag2 && !flag) {
                            closingPaths = this.CloseAllUnlocked();
                            if(this.tabControl1.TabCount > 0) {
                                return true;
                            }
                            QTUtility.SaveClosing(closingPaths);
                            return false;
                        }
                        Keys modifierKeys = Control.ModifierKeys;
                        if((modifierKeys == (Keys.Control | Keys.Shift)) || !flag) {
                            foreach(QTabItem item2 in this.tabControl1.TabPages) {
                                closingPaths.Add(item2.CurrentPath);
                                AddToHistory(item2);
                            }
                            QTUtility.SaveClosing(closingPaths);
                            return false;
                        }
                        if(modifierKeys == Keys.Control) {
                            closingPaths = this.CloseAllUnlocked();
                        }
                        else {
                            closingPaths.Add(this.CurrentTab.CurrentPath);
                            this.CloseTab(this.CurrentTab, false);
                        }
                        if(this.tabControl1.TabCount > 0) {
                            return true;
                        }
                        QTUtility.SaveClosing(closingPaths);
                        return false;
                    }
            }
            QTUtility.SaveClosing(closingPaths);
            return false;
        }

        internal static int HandleDragEnter(IntPtr hDrop, out string strDraggingDrive, out string strDraggingStartPath) {
            strDraggingDrive = (string)(strDraggingStartPath = null);
            int capacity = (int)PInvoke.DragQueryFile(hDrop, uint.MaxValue, null, 0);
            if(capacity < 1) {
                return -1;
            }
            List<string> list = new List<string>(capacity);
            for(int i = 0; i < capacity; i++) {
                StringBuilder lpszFile = new StringBuilder(260);
                PInvoke.DragQueryFile(hDrop, (uint)i, lpszFile, lpszFile.Capacity);
                if(lpszFile.Length > 0) {
                    list.Add(lpszFile.ToString());
                }
            }
            if(list.Count <= 0) {
                return -1;
            }
            if(list[0].Length < 4) {
                return 2;
            }
            bool flag = true;
            string b = QTUtility2.MakeRootName(list[0]);
            foreach(string str2 in list) {
                if(File.Exists(str2) || Directory.Exists(str2)) {
                    if(str2.Length <= 3) {
                        return 2;
                    }
                    if(!string.Equals(QTUtility2.MakeRootName(str2), b, StringComparison.OrdinalIgnoreCase)) {
                        flag = false;
                    }
                    continue;
                }
                return -1;
            }
            if(flag) {
                strDraggingDrive = b;
                strDraggingStartPath = Path.GetDirectoryName(list[0]);
                return 0;
            }
            return 1;
        }

        internal static void HandleF2(ListViewWrapper listViewWrapper) {
            IntPtr hWnd = listViewWrapper.GetEditControl();
            if(hWnd != IntPtr.Zero) {
                IntPtr lParam = Marshal.AllocHGlobal(520);
                if(0 < ((int)PInvoke.SendMessage(hWnd, 13, (IntPtr)260, lParam))) {
                    string str = Marshal.PtrToStringUni(lParam);
                    if(str.Length > 2) {
                        int num = str.LastIndexOf(".");
                        if(num != -1) {
                            IntPtr ptr3 = PInvoke.SendMessage(hWnd, 0xb0, IntPtr.Zero, IntPtr.Zero);
                            int num2 = QTUtility2.GET_X_LPARAM(ptr3);
                            int length = QTUtility2.GET_Y_LPARAM(ptr3);
                            if((length - num2) >= 0) {
                                if((num2 == 0) && (length == num)) {
                                    num2 = length = num;
                                }
                                else if((num2 == length) && (length == num)) {
                                    num2 = num + 1;
                                    length = str.Length;
                                }
                                else if((num2 == (num + 1)) && (length == str.Length)) {
                                    num2 = 0;
                                    length = -1;
                                }
                                else if((num2 == 0) && (length == str.Length)) {
                                    num2 = 0;
                                    length = 0;
                                }
                                else {
                                    num2 = 0;
                                    length = num;
                                }
                                PInvoke.SendMessage(hWnd, 0xb1, (IntPtr)num2, (IntPtr)length);
                            }
                        }
                    }
                }
                Marshal.FreeHGlobal(lParam);
            }
        }

        private void HandleF5() {
            IntPtr ptr;
            if(QTUtility.instanceManager.TryGetButtonBarHandle(this.ExplorerHandle, out ptr)) {
                QTUtility2.SendCOPYDATASTRUCT(ptr, (IntPtr)9, "browser_refresh", IntPtr.Zero);
            }
        }

        private void HandleFileDrop(IntPtr hDrop) {
            this.HideToolTipForDD();
            int capacity = (int)PInvoke.DragQueryFile(hDrop, uint.MaxValue, null, 0);
            if(capacity >= 1) {
                List<string> listDroppedPaths = new List<string>(capacity);
                for(int i = 0; i < capacity; i++) {
                    StringBuilder lpszFile = new StringBuilder(260);
                    PInvoke.DragQueryFile(hDrop, (uint)i, lpszFile, lpszFile.Capacity);
                    listDroppedPaths.Add(lpszFile.ToString());
                }
                this.OpenDroppedFolder(listDroppedPaths);
            }
        }

        private bool HandleKEYDOWN(IntPtr wParam, bool fRepeat) {
            bool flag;
            Keys key = (Keys)((int)wParam);
            Keys mkey = (Keys)(((int)wParam) | ((int)Control.ModifierKeys));

            switch(key) {
                case Keys.ShiftKey:
                    if(fRepeat) {
                        return false;
                    }

                    if(!QTUtility.CheckConfig(Settings.PreviewsWithShift)) {
                        this.HideThumbnailTooltip(5);
                    }
                    
                    if(!QTUtility.CheckConfig(Settings.NoShowSubDirTips)) {
                        if(QTUtility.CheckConfig(Settings.SubDirTipsWithShift)) {
                            if(listViewWrapper.MouseIsOverListView()) {
                                listViewWrapper.FireHotTrack();
                            }
                        }
                        else if(this.subDirTip != null && this.subDirTip.IsShowing && !this.subDirTip.MenuIsShowing) {
                            this.HideSubDirTip(6);
                        }
                    }
                    return false;

                case Keys.Enter:
                    return false;

                case Keys.Menu:
                    if((!fRepeat && QTUtility.CheckConfig(Settings.ShowTabCloseButtons)) && QTUtility.CheckConfig(Settings.TabCloseBtnsWithAlt)) {
                        this.tabControl1.ShowCloseButton(true);
                        if(QTUtility.IsVista) {
                            return QTUtility.CheckConfig(Settings.HideMenuBar);
                        }
                    }
                    return false;

                case Keys.ControlKey:
                    if(!fRepeat && this.NowTabDragging && (this.DraggingTab != null) && this.tabControl1.GetTabMouseOn() == null) {
                        this.Cursor = this.GetCursor(false);
                    }
                    break;

                case Keys.Tab:
                    if(!QTUtility.CheckConfig(Settings.NoTabSwitcher) && (mkey & Keys.Control) != Keys.None) {
                        return this.ShowTabSwitcher((mkey & Keys.Shift) != Keys.None, fRepeat);
                    }
                    break;
            }

            switch(mkey) {
                case Keys.Back:
                    if(QTUtility.IsVista) {
                        if(listViewWrapper.HasFocus()) {
                            if(!fRepeat) {
                                if(QTUtility.CheckConfig(Settings.BackspaceUpLevel)) {
                                    this.UpOneLevel();
                                }
                                else {
                                    this.NavigateCurrentTab(true);
                                }
                            }
                            return true;
                        }
                    }
                    return false;

                case Keys.Alt | Keys.Left:
                    this.NavigateCurrentTab(true);
                    return true;

                case Keys.Alt | Keys.Right:
                    this.NavigateCurrentTab(false);
                    return true;

                case Keys.Alt | Keys.F4:
                    if(!fRepeat) {
                        WindowUtils.CloseExplorer(this.ExplorerHandle, 1);
                    }
                    return true;

                case Keys.F2:
                    if(!QTUtility.CheckConfig(Settings.F2Selection)) {
                        HandleF2(listViewWrapper);
                    }
                    return false;

            }
            if(((Keys.Control | Keys.NumPad0) <= mkey && mkey <= (Keys.Control | Keys.NumPad9)) ||
                    ((Keys.Control | Keys.D0) <= mkey && mkey <= (Keys.Control | Keys.D9))) {
                int num3;
                if(mkey >= (Keys.Control | Keys.NumPad0)) {
                    num3 = (int)(mkey - (Keys.Control | Keys.NumPad0));
                }
                else {
                    num3 = (int)(mkey - (Keys.Control | Keys.D0));
                }
                if(num3 == 0) {
                    num3 = 10;
                }
                if(this.tabControl1.TabCount >= num3) {
                    this.tabControl1.SelectTab((int)(num3 - 1));
                }
                return true;
            }

            int imkey = (int)mkey | 0x100000;
            if(imkey == QTUtility.ShortcutKeys[0]) {
                this.NavigateCurrentTab(true);
                return true;
            }
            if(imkey == QTUtility.ShortcutKeys[1]) {
                this.NavigateCurrentTab(false);
                return true;
            }
            if(imkey == QTUtility.ShortcutKeys[2]) {
                if(!fRepeat) {
                    this.NavigateToFirstOrLast(true);
                }
                return true;
            }
            if(imkey == QTUtility.ShortcutKeys[3]) {
                if(!fRepeat) {
                    this.NavigateToFirstOrLast(false);
                }
                return true;
            }
            if(imkey == QTUtility.ShortcutKeys[4]) {
                if(!fRepeat) {
                    int selectedIndex = this.tabControl1.SelectedIndex;
                    if(selectedIndex == (this.tabControl1.TabCount - 1)) {
                        this.tabControl1.SelectedIndex = 0;
                    }
                    else {
                        this.tabControl1.SelectedIndex = selectedIndex + 1;
                    }
                }
                return true;
            }
            if(imkey == QTUtility.ShortcutKeys[5]) {
                if(!fRepeat) {
                    int num5 = this.tabControl1.SelectedIndex;
                    if(num5 == 0) {
                        this.tabControl1.SelectedIndex = this.tabControl1.TabCount - 1;
                    }
                    else {
                        this.tabControl1.SelectedIndex = num5 - 1;
                    }
                }
                return true;
            }
            if(imkey == QTUtility.ShortcutKeys[6]) {
                if(!fRepeat && (this.tabControl1.TabCount > 0)) {
                    this.tabControl1.SelectedIndex = 0;
                }
                return true;
            }
            if(imkey == QTUtility.ShortcutKeys[7]) {
                if(!fRepeat && (this.tabControl1.TabCount > 1)) {
                    this.tabControl1.SelectedIndex = this.tabControl1.TabCount - 1;
                }
                return true;
            }
            if(imkey == QTUtility.ShortcutKeys[8]) {
                if(!fRepeat) {
                    if(this.tabControl1.TabCount > 1) {
                        this.CloseTab(this.CurrentTab);
                    }
                    else {
                        WindowUtils.CloseExplorer(this.ExplorerHandle, 1);
                    }
                }
                return true;
            }
            if(imkey == QTUtility.ShortcutKeys[9]) {
                if(!fRepeat && (this.tabControl1.TabCount > 1)) {
                    foreach(QTabItem item in this.tabControl1.TabPages) {
                        if(item != this.CurrentTab) {
                            this.CloseTab(item);
                        }
                    }
                }
                return true;
            }
            if(imkey == QTUtility.ShortcutKeys[10]) {
                if(!fRepeat) {
                    this.CloseLeftRight(true, -1);
                }
                return true;
            }
            if(imkey == QTUtility.ShortcutKeys[11]) {
                if(!fRepeat) {
                    this.CloseLeftRight(false, -1);
                }
                return true;
            }
            if(imkey == QTUtility.ShortcutKeys[12]) {
                if(!fRepeat) {
                    WindowUtils.CloseExplorer(this.ExplorerHandle, 1);
                }
                return true;
            }
            if(imkey == QTUtility.ShortcutKeys[13]) {
                if(!fRepeat) {
                    this.RestoreNearest();
                }
                return true;
            }
            if(imkey == QTUtility.ShortcutKeys[14]) {
                if(!fRepeat) {
                    this.CloneTabButton(this.CurrentTab, null, true, -1);
                }
                return true;
            }
            if(imkey == QTUtility.ShortcutKeys[15]) {
                if(!fRepeat && (this.CurrentTab != null)) {
                    using(IDLWrapper wrapper = new IDLWrapper(this.CurrentTab.CurrentIDL)) {
                        this.OpenNewWindow(wrapper);
                    }
                }
                return true;
            }
            if(imkey == QTUtility.ShortcutKeys[0x10]) {
                if(this.CurrentTab != null) {
                    this.CurrentTab.TabLocked = !this.CurrentTab.TabLocked;
                }
                return true;
            }
            if(imkey != QTUtility.ShortcutKeys[0x11]) {
                if(imkey == QTUtility.ShortcutKeys[0x12]) {
                    if(!fRepeat) {
                        this.ChooseNewDirectory();
                    }
                    return true;
                }
                if(imkey == QTUtility.ShortcutKeys[0x13]) {
                    if(!fRepeat && (this.CurrentTab != null)) {
                        this.CreateGroup(this.CurrentTab);
                    }
                    return true;
                }
                if(imkey == QTUtility.ShortcutKeys[20]) {
                    if(!fRepeat) {
                        this.OpenOptionsDialog();
                    }
                    return true;
                }
                if(imkey == QTUtility.ShortcutKeys[0x15]) {
                    if(!fRepeat) {
                        Rectangle tabRect = this.tabControl1.GetTabRect(this.tabControl1.TabCount - 1, true);
                        this.contextMenuSys.Show(base.PointToScreen(new Point(tabRect.Right + 10, tabRect.Bottom - 10)));
                    }
                    return true;
                }
                if(imkey == QTUtility.ShortcutKeys[0x16]) {
                    if(!fRepeat) {
                        int index = this.tabControl1.TabPages.IndexOf(this.CurrentTab);
                        if(index != -1) {
                            this.ContextMenuedTab = this.CurrentTab;
                            Rectangle rectangle2 = this.tabControl1.GetTabRect(index, true);
                            this.contextMenuTab.Show(base.PointToScreen(new Point(rectangle2.Right + 10, rectangle2.Bottom - 10)));
                        }
                    }
                    return true;
                }
                if(((imkey == QTUtility.ShortcutKeys[0x17]) || (imkey == QTUtility.ShortcutKeys[0x18])) || (imkey == QTUtility.ShortcutKeys[0x19])) {
                    if(!fRepeat) {
                        IntPtr ptr2;
                        int num7 = 3;
                        if(imkey == QTUtility.ShortcutKeys[0x18]) {
                            num7 = 4;
                        }
                        else if(imkey == QTUtility.ShortcutKeys[0x19]) {
                            num7 = 5;
                        }
                        if(QTUtility.instanceManager.TryGetButtonBarHandle(this.ExplorerHandle, out ptr2)) {
                            return (1 == ((int)QTUtility2.SendCOPYDATASTRUCT(ptr2, (IntPtr)4, "fromTab", (IntPtr)num7)));
                        }
                    }
                }
                else {
                    if(imkey == QTUtility.ShortcutKeys[0x1a]) {
                        WindowUtils.ShowMenuBar(QTUtility.CheckConfig(Settings.HideMenuBar), base.ReBarHandle);
                        if(QTUtility.CheckConfig(Settings.HideMenuBar)) {
                            QTUtility.ConfigValues[7] = (byte)(QTUtility.ConfigValues[7] & 0xf7);
                        }
                        else {
                            QTUtility.ConfigValues[7] = (byte)(QTUtility.ConfigValues[7] | 8);
                        }
                        using(RegistryKey rkey = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar")) {
                            rkey.SetValue("Config", QTUtility.ConfigValues);
                        }
                        return true;
                    }
                    if(((imkey == QTUtility.ShortcutKeys[0x1b]) || (imkey == QTUtility.ShortcutKeys[0x1c])) || (((imkey == QTUtility.ShortcutKeys[0x1d]) || (imkey == QTUtility.ShortcutKeys[30])) || (imkey == QTUtility.ShortcutKeys[0x1f]))) {
                        if(!fRepeat) {
                            int num8 = 0;
                            if(imkey == QTUtility.ShortcutKeys[0x1c]) {
                                num8 = 1;
                            }
                            else if(imkey == QTUtility.ShortcutKeys[0x1d]) {
                                num8 = 2;
                            }
                            else if(imkey == QTUtility.ShortcutKeys[30]) {
                                num8 = 3;
                            }
                            else if(imkey == QTUtility.ShortcutKeys[0x1f]) {
                                num8 = 4;
                            }
                            if((num8 < 2) && (((this.subDirTip != null) && this.subDirTip.MenuIsShowing) || ((this.subDirTip_Tab != null) && this.subDirTip_Tab.MenuIsShowing))) {
                                return false;
                            }
                            this.DoFileTools(num8);
                        }
                        return true;
                    }
                    if(imkey == QTUtility.ShortcutKeys[0x20]) {
                        this.ToggleTopMost();
                        this.SyncButtonBarCurrent(0x40);
                        return true;
                    }
                    if((imkey == QTUtility.ShortcutKeys[0x21]) || (imkey == QTUtility.ShortcutKeys[0x22])) {
                        int num9;
                        int num10;
                        byte num11;
                        if(0x80000 != ((int)PInvoke.Ptr_OP_AND(PInvoke.GetWindowLongPtr(this.ExplorerHandle, -20), 0x80000))) {
                            if(imkey == QTUtility.ShortcutKeys[0x21]) {
                                return true;
                            }
                            PInvoke.SetWindowLongPtr(this.ExplorerHandle, -20, PInvoke.Ptr_OP_OR(PInvoke.GetWindowLongPtr(this.ExplorerHandle, -20), 0x80000));
                            PInvoke.SetLayeredWindowAttributes(this.ExplorerHandle, 0, 0xff, 2);
                        }
                        if(PInvoke.GetLayeredWindowAttributes(this.ExplorerHandle, out num9, out num11, out num10)) {
                            IntPtr ptr3;
                            if(imkey == QTUtility.ShortcutKeys[0x21]) {
                                if(num11 > 0xf3) {
                                    num11 = 0xff;
                                }
                                else {
                                    num11 = (byte)(num11 + 12);
                                }
                            }
                            else if(num11 < 0x20) {
                                num11 = 20;
                            }
                            else {
                                num11 = (byte)(num11 - 12);
                            }
                            PInvoke.SetLayeredWindowAttributes(this.ExplorerHandle, 0, num11, 2);
                            if(QTUtility.instanceManager.TryGetButtonBarHandle(this.ExplorerHandle, out ptr3)) {
                                QTUtility2.SendCOPYDATASTRUCT(ptr3, (IntPtr)7, "track", (IntPtr)num11);
                            }
                            if(num11 == 0xff) {
                                PInvoke.SetWindowLongPtr(this.ExplorerHandle, -20, PInvoke.Ptr_OP_AND(PInvoke.GetWindowLongPtr(this.ExplorerHandle, -20), 0xfff7ffff));
                            }
                        }
                        return true;
                    }
                    if(imkey == QTUtility.ShortcutKeys[0x23]) {
                        listViewWrapper.SetFocus();
                        return true;
                    }
                    if(imkey == QTUtility.ShortcutKeys[0x24]) {
                        if(QTUtility.IsVista) {
                            PInvoke.SetFocus(this.GetSearchBand_Edit());
                            return true;
                        }
                    }
                    else if(imkey == QTUtility.ShortcutKeys[0x25]) {
                        IntPtr ptr4;
                        if(QTUtility.instanceManager.TryGetButtonBarHandle(this.ExplorerHandle, out ptr4) && PInvoke.IsWindow(ptr4)) {
                            QTUtility2.SendCOPYDATASTRUCT(ptr4, (IntPtr)8, null, IntPtr.Zero);
                            return true;
                        }
                    }
                    else if(imkey == QTUtility.ShortcutKeys[0x26]) {
                        if(!QTUtility.CheckConfig(Settings.NoShowSubDirTips)) {
                            if(!fRepeat) {
                                this.ShowAndClickSubDirTip();
                            }
                            return true;
                        }
                    }
                    else {
                        if(imkey == QTUtility.ShortcutKeys[0x27]) {
                            if(!fRepeat) {
                                ShowTaksbarItem(this.ExplorerHandle, false);
                            }
                            return true;
                        }
                        if(imkey == QTUtility.ShortcutKeys[40]) {
                            this.tabControl1.Focus();
                            this.tabControl1.FocusNextTab(false, true, false);
                            return true;
                        }
                        if(Array.IndexOf<int>(QTUtility.PluginShortcutKeysCache, imkey) != -1) {
                            foreach(string str in QTUtility.dicPluginShortcutKeys.Keys) {
                                int[] numArray = QTUtility.dicPluginShortcutKeys[str];
                                if(numArray != null) {
                                    for(int i = 0; i < numArray.Length; i++) {
                                        if(imkey == numArray[i]) {
                                            Plugin plugin;
                                            if(this.pluginManager.TryGetPlugin(str, out plugin)) {
                                                try {
                                                    plugin.Instance.OnShortcutKeyPressed(i);
                                                }
                                                catch(Exception exception) {
                                                    PluginManager.HandlePluginException(exception, this.ExplorerHandle, plugin.PluginInformation.Name, "On shortcut key pressed. Index is " + i);
                                                }
                                                return true;
                                            }
                                            return false;
                                        }
                                    }
                                }
                            }
                        }
                        else {
                            if(!fRepeat && QTUtility.dicUserAppShortcutKeys.ContainsKey(imkey)) {
                                MenuItemArguments mia = QTUtility.dicUserAppShortcutKeys[imkey];
                                try {
                                    using(IDLWrapper wrapper2 = new IDLWrapper(this.GetCurrentPIDL())) {
                                        Address[] addressArray;
                                        string str2;
                                        if((wrapper2.Available && wrapper2.HasPath) && this.TryGetSelection(out addressArray, out str2, false)) {
                                            AppLauncher launcher = new AppLauncher(addressArray, wrapper2.Path);
                                            launcher.ReplaceTokens_WorkingDir(mia);
                                            launcher.ReplaceTokens_Arguments(mia);
                                        }
                                    }
                                    AppLauncher.Execute(mia, this.ExplorerHandle);
                                }
                                catch(Exception exception2) {
                                    QTUtility2.MakeErrorLog(exception2, null);
                                }
                                finally {
                                    mia.RestoreOriginalArgs();
                                }
                                return true;
                            }
                            if(!fRepeat && QTUtility.dicGroupShortcutKeys.ContainsKey(imkey)) {
                                this.OpenGroup(QTUtility.dicGroupShortcutKeys[imkey], false);
                                return true;
                            }
                        }
                    }
                }
                imkey -= 0x100000;
                return (imkey == 0x20057);
            }

            flag = true;
            foreach(QTabItem item2 in this.tabControl1.TabPages) {
                if(!item2.TabLocked) {
                    flag = false;
                    break;
                }
            }

            foreach(QTabItem item3 in this.tabControl1.TabPages) {
                item3.TabLocked = !flag;
            }
            return true;
        }

        private void HandleLBUTTON_Tree(BandObjectLib.MSG msg, bool fMouseDown) {
            IntPtr ptr;
            if((this.ShellBrowser.GetControlWindow(3, out ptr) == 0) && (msg.hwnd == ptr)) {
                BandObjectLib.POINT point;
                TVHITTESTINFO structure = new TVHITTESTINFO();
                point.x = QTUtility2.GET_X_LPARAM(msg.lParam);
                point.y = QTUtility2.GET_Y_LPARAM(msg.lParam);
                structure.pt = point;
                IntPtr ptr2 = Marshal.AllocHGlobal(Marshal.SizeOf(structure));
                Marshal.StructureToPtr(structure, ptr2, false);
                IntPtr wParam = PInvoke.SendMessage(ptr, 0x1111, IntPtr.Zero, ptr2);
                if(wParam != IntPtr.Zero) {
                    structure = (TVHITTESTINFO)Marshal.PtrToStructure(ptr2, typeof(TVHITTESTINFO));
                    bool flag = false;
                    if(fMouseDown) {
                        flag = (((structure.flags != 1) && (structure.flags != 0x10)) && ((structure.flags & 2) == 0)) && ((structure.flags & 4) == 0);
                    }
                    else {
                        flag = ((structure.flags & 2) != 0) || ((structure.flags & 4) != 0);
                    }
                    if(flag) {
                        int num = (int)PInvoke.SendMessage(ptr, 0x1127, wParam, (IntPtr)2);
                        if((num & 2) == 0) {
                            this.NavigatedByCode = this.fNowTravelByTree = true;
                        }
                    }
                }
                Marshal.FreeHGlobal(ptr2);
            }
        }        

        private bool HandleLVKEYDOWN_CursorLoop(Keys key) {
            // TODO
            /*
            IntPtr hWnd = IntPtr.Zero; // this.GetExplorerListView();
            if(hWnd == PInvoke.GetFocus()) {
                if(IntPtr.Zero != PInvoke.SendMessage(hWnd, 0x10af, IntPtr.Zero, IntPtr.Zero)) {
                    return false;
                }
                int num = (int)PInvoke.SendMessage(hWnd, 0x1004, IntPtr.Zero, IntPtr.Zero);
                if(num > 1) {
                    if((key == 0x26) || (key == 40)) {
                        bool flag = key == 0x26;
                        int num2 = flag ? 0 : (num - 1);
                        int iItem = flag ? (num - 1) : 0;
                        if(1 != ((int)PInvoke.SendMessage(hWnd, 0x102c, (IntPtr)num2, (IntPtr)1))) {
                            goto Label_025A;
                        }
                        IShellView ppshv = null;
                        try {
                            try {
                                if(this.ShellBrowser.QueryActiveShellView(out ppshv) == 0) {
                                    PInvoke.SetRedraw(hWnd, false);
                                    ((IFolderView)ppshv).SelectItem(iItem, 0x1d);
                                    PInvoke.SendMessage(hWnd, 0x1015, (IntPtr)num2, (IntPtr)num2);
                                    PInvoke.SetRedraw(hWnd, true);
                                    return true;
                                }
                            }
                            catch {
                            }
                            goto Label_025A;
                        }
                        finally {
                            if(ppshv != null) {
                                Marshal.ReleaseComObject(ppshv);
                            }
                        }
                    }
                    int num4 = (int)PInvoke.SendMessage(hWnd, 0x100c, (IntPtr)(-1), (IntPtr)1);
                    if(num4 != -1) {
                        switch(((int)PInvoke.SendMessage(hWnd, 0x108f, IntPtr.Zero, IntPtr.Zero))) {
                            case 0:
                            case 4: {
                                    bool flag2 = key == 0x25;
                                    int num6 = (int)PInvoke.SendMessage(hWnd, 0x100c, (IntPtr)num4, flag2 ? ((IntPtr)0x400) : ((IntPtr)0x800));
                                    if((QTUtility.IsVista && (num6 == num4)) || (!QTUtility.IsVista && (((num6 == -1) || ((num4 == 0) && flag2)) || ((num4 == (num - 1)) && !flag2)))) {
                                        IShellView view3 = null;
                                        try {
                                            int num7 = flag2 ? (num4 - 1) : (num4 + 1);
                                            if((num7 < 0) || (num7 > (num - 1))) {
                                                num7 = flag2 ? (num - 1) : 0;
                                            }
                                            if(this.ShellBrowser.QueryActiveShellView(out view3) == 0) {
                                                PInvoke.SetRedraw(hWnd, false);
                                                ((IFolderView)view3).SelectItem(num7, 0x1d);
                                                PInvoke.SendMessage(hWnd, 0x1015, (IntPtr)num4, (IntPtr)num4);
                                                PInvoke.SetRedraw(hWnd, true);
                                                return true;
                                            }
                                        }
                                        catch {
                                        }
                                        finally {
                                            if(view3 != null) {
                                                Marshal.ReleaseComObject(view3);
                                            }
                                        }
                                    }
                                    break;
                                }
                        }
                    }
                }
            }
        Label_025A:
             */
            return false;
        }

        private bool HandleMOUSEWHEEL(IntPtr lParam) {
            if(!base.IsHandleCreated) {
                return false;
            }
            MOUSEHOOKSTRUCTEX mousehookstructex = (MOUSEHOOKSTRUCTEX)Marshal.PtrToStructure(lParam, typeof(MOUSEHOOKSTRUCTEX));
            int y = mousehookstructex.mouseData >> 0x10;
            IntPtr handle = PInvoke.WindowFromPoint(new Point(mousehookstructex.mhs.pt.x, mousehookstructex.mhs.pt.y));
            Control control = Control.FromHandle(handle);
            bool flag = false;
            if(control != null) {
                IntPtr ptr2;
                DropDownMenuReorderable reorderable = control as DropDownMenuReorderable;
                if(reorderable != null) {
                    if(reorderable.CanScroll) {
                        PInvoke.SendMessage(handle, WM.MOUSEWHEEL, QTUtility2.Make_LPARAM(0, y), QTUtility2.Make_LPARAM(mousehookstructex.mhs.pt.x, mousehookstructex.mhs.pt.y));
                    }
                    return true;
                }
                flag = (control == this.tabControl1) || (handle == base.Handle);
                if(!flag && QTUtility.instanceManager.TryGetButtonBarHandle(this.ExplorerHandle, out ptr2)) {
                    flag = (handle == ptr2) || (handle == listViewWrapper.GetListViewHandle()); // TODO make sure this didn't break
                }
            }
            if(!flag) {
                Keys modifierKeys = Control.ModifierKeys;
                if((!QTUtility.CheckConfig(Settings.HorizontalScroll) && (modifierKeys == Keys.Shift)) || ((!QTUtility.IsVista && !QTUtility.CheckConfig(Settings.CtrlWheelChangeView)) && (modifierKeys == Keys.Control))) {
                    if(listViewWrapper.MouseIsOverListView()) {
                        switch(modifierKeys) {
                            case Keys.Shift:
                                listViewWrapper.ScrollHorizontal(y);
                                return true;

                            case Keys.Control:
                                this.ChangeViewMode(y > 0);
                                return true;
                        }
                    }
                }
                return false;
            }
            if(((this.tabControl1.TabCount < 2) || (this.ExplorerHandle != PInvoke.GetForegroundWindow())) || base.Explorer.Busy) {
                return false;
            }
            int selectedIndex = this.tabControl1.SelectedIndex;
            if(y < 0) {
                if(selectedIndex == (this.tabControl1.TabCount - 1)) {
                    this.tabControl1.SelectedIndex = 0;
                }
                else {
                    this.tabControl1.SelectedIndex = selectedIndex + 1;
                }
            }
            else if(selectedIndex < 1) {
                this.tabControl1.SelectedIndex = this.tabControl1.TabCount - 1;
            }
            else {
                this.tabControl1.SelectedIndex = selectedIndex - 1;
            }
            return true;
        }

        internal static void HandleRenaming(ListViewWrapper lvw, IntPtr pIDL, Control ctrl) {
            if(pIDL != IntPtr.Zero) {
                StringBuilder pszPath = new StringBuilder(260);
                if(PInvoke.SHGetPathFromIDList(pIDL, pszPath) && (pszPath.Length > 0)) {
                    string path = pszPath.ToString();
                    if(!Directory.Exists(path)) {
                        if(File.Exists(path)) {
                            string extension = Path.GetExtension(path);
                            if(!string.IsNullOrEmpty(extension) && (extension.Equals(".lnk", StringComparison.OrdinalIgnoreCase) || extension.Equals(".url", StringComparison.OrdinalIgnoreCase))) {
                                return;
                            }
                        }
                        IntPtr hWnd = lvw.GetEditControl();
                        if(hWnd != IntPtr.Zero) {
                            IntPtr lParam = Marshal.AllocHGlobal(520);
                            if(0 < ((int)PInvoke.SendMessage(hWnd, WM.GETTEXT, (IntPtr)260, lParam))) {
                                string str3 = Marshal.PtrToStringUni(lParam);
                                if(str3.Length > 2) {
                                    int num = str3.LastIndexOf(".");
                                    if(num > 0) {
                                        new WaitTimeoutCallback(QTTabBarClass.WaitTimeout).BeginInvoke(0x4b, new AsyncCallback(QTTabBarClass.AsyncComplete_ItemEdit), new object[] { hWnd, num, ctrl });
                                    }
                                }
                            }
                            Marshal.FreeHGlobal(lParam);
                        }
                    }
                }
            }
        }

        private void HandleTabClickAction(QTabItem clickedTab, bool fWheel) {
            byte num = fWheel ? QTUtility.ConfigValues[12] : QTUtility.ConfigValues[3];
            switch(num) {
                case 0:
                case 1:
                    if(!(fWheel ^ (num == 1))) {
                        this.UpOneLevel();
                        return;
                    }
                    this.NowTabDragging = false;
                    if(this.tabControl1.TabCount <= 1) {
                        if(clickedTab.TabLocked) {
                            break;
                        }
                        WindowUtils.CloseExplorer(this.ExplorerHandle, 1);
                        return;
                    }
                    this.CloseTab(clickedTab);
                    return;

                case 2:
                    clickedTab.TabLocked = !clickedTab.TabLocked;
                    return;

                case 3:
                    this.ContextMenuedTab = clickedTab;
                    this.contextMenuTab.Show(Control.MousePosition);
                    return;

                case 4:
                    using(IDLWrapper wrapper = new IDLWrapper(clickedTab.CurrentIDL)) {
                        this.OpenNewWindow(wrapper);
                    }
                    if((Control.ModifierKeys & Keys.Shift) != Keys.Shift) {
                        break;
                    }
                    this.CloseTab(clickedTab);
                    return;

                case 5:
                    this.CloneTabButton(clickedTab, null, true, -1);
                    return;

                case 6: {
                        string currentPath = clickedTab.CurrentPath;
                        if(currentPath.IndexOf("???") != -1) {
                            currentPath = currentPath.Substring(0, currentPath.IndexOf("???"));
                        }
                        SetStringClipboard(currentPath);
                        return;
                    }
                case 7:
                    ShellMethods.ShowProperties(clickedTab.CurrentIDL);
                    break;

                default:
                    return;
            }
        }

        private bool HandleTabFolderActions(int index, Keys modKeys, bool fEnqExec) {
            IShellView ppshv = null;
            IntPtr zero = IntPtr.Zero;
            IntPtr ppidl = IntPtr.Zero;
            try {
                Address[] addressArray;
                IntPtr pIDL = IntPtr.Zero;
                bool flag = true;
                if(index != -1) {
                    addressArray = new Address[0];
                    if((this.ShellBrowser.QueryActiveShellView(out ppshv) != 0) || (ppshv == null)) {
                        return false;
                    }
                    IFolderView view2 = (IFolderView)ppshv;
                    if(view2.Item(index, out ppidl) != 0) {
                        return false;
                    }
                    zero = ShellMethods.ShellGetPath(this.ShellBrowser);
                    if((zero == IntPtr.Zero) || (ppidl == IntPtr.Zero)) {
                        return false;
                    }
                    pIDL = PInvoke.ILCombine(zero, ppidl);
                }
                else {
                    string str;
                    if(this.TryGetSelection(out addressArray, out str, false) && (addressArray.Length > 0)) {
                        List<Address> list = new List<Address>(addressArray);
                        pIDL = ShellMethods.CreateIDL(list[0].ITEMIDLIST);
                        list.RemoveAt(0);
                        addressArray = list.ToArray();
                        flag = (addressArray.Length > 0) || (modKeys == Keys.Shift);
                    }
                    else {
                        return false;
                    }
                }
                using(IDLWrapper wrapper = new IDLWrapper(pIDL)) {
                    if((wrapper.Available && wrapper.HasPath) && wrapper.IsReadyIfDrive) {
                        if(wrapper.IsFolder) {
                            if(modKeys == Keys.Control) {
                                if(!wrapper.IsLinkToDeadFolder) {
                                    QTUtility.TMPPathList.AddRange(CreateTMPPathsToOpenNew(addressArray, wrapper.Path));
                                    this.OpenNewWindow(wrapper);
                                }
                                else {
                                    SystemSounds.Hand.Play();
                                }
                            }
                            else if(modKeys == (Keys.Alt | Keys.Control | Keys.Shift)) {
                                DirectoryInfo info = new DirectoryInfo(wrapper.Path);
                                if(info.Exists) {
                                    DirectoryInfo[] directories = info.GetDirectories();
                                    if((directories.Length + this.tabControl1.TabCount) < 0x41) {
                                        this.tabControl1.SetRedraw(false);
                                        foreach(DirectoryInfo info2 in directories) {
                                            if(info2.Name != "System Volume Information") {
                                                using(IDLWrapper wrapper2 = new IDLWrapper(info2.FullName)) {
                                                    if(wrapper2.Available && (!wrapper2.IsLink || Directory.Exists(ShellMethods.GetLinkTargetPath(info2.FullName)))) {
                                                        this.OpenNewTab(wrapper2, true, false);
                                                    }
                                                }
                                            }
                                        }
                                        this.tabControl1.SetRedraw(true);
                                    }
                                    else {
                                        SystemSounds.Hand.Play();
                                    }
                                }
                            }
                            else {
                                if(addressArray.Length > 1) {
                                    this.tabControl1.SetRedraw(false);
                                }
                                try {
                                    if(flag) {
                                        this.OpenNewTab(wrapper, (modKeys & Keys.Shift) == Keys.Shift, false);
                                    }
                                    else if(!wrapper.IsFileSystemFile) {
                                        this.Navigate(wrapper);
                                    }
                                    else {
                                        return false;
                                    }
                                    for(int i = 0; i < addressArray.Length; i++) {
                                        using(IDLWrapper wrapper3 = new IDLWrapper(addressArray[i].ITEMIDLIST)) {
                                            if(((wrapper3.Available && wrapper3.HasPath) && (wrapper3.IsReadyIfDrive && wrapper3.IsFolder)) && !wrapper3.IsLinkToDeadFolder) {
                                                string path = wrapper3.Path;
                                                if(((path != wrapper.Path) && (path.Length > 0)) && !QTUtility2.IsShellPathButNotFileSystem(path)) {
                                                    this.OpenNewTab(wrapper3, true, false);
                                                }
                                            }
                                        }
                                    }
                                }
                                finally {
                                    if(addressArray.Length > 1) {
                                        this.tabControl1.SetRedraw(true);
                                    }
                                }
                            }
                            return true;
                        }
                        if(wrapper.IsLink) {
                            using(IDLWrapper wrapper4 = new IDLWrapper(ShellMethods.GetLinkTargetIDL(wrapper.Path))) {
                                if(((wrapper4.Available && wrapper4.HasPath) && (wrapper4.IsReadyIfDrive && wrapper4.IsFolder)) && !wrapper.IsLinkToDeadFolder) {
                                    if(modKeys == Keys.Control) {
                                        QTUtility.TMPPathList.AddRange(CreateTMPPathsToOpenNew(addressArray, wrapper.Path));
                                        this.OpenNewWindow(wrapper4);
                                    }
                                    else {
                                        if(flag) {
                                            this.OpenNewTab(wrapper4, (modKeys & Keys.Shift) == Keys.Shift, false);
                                        }
                                        else {
                                            this.Navigate(wrapper4);
                                        }
                                        for(int j = 0; j < addressArray.Length; j++) {
                                            using(IDLWrapper wrapper5 = new IDLWrapper(addressArray[j].ITEMIDLIST)) {
                                                if(((wrapper5.Available && wrapper5.HasPath) && (wrapper5.IsReadyIfDrive && wrapper5.IsFolder)) && !wrapper5.IsLinkToDeadFolder) {
                                                    string str3 = wrapper5.Path;
                                                    if(((str3 != wrapper4.Path) && (str3.Length > 0)) && !QTUtility2.IsShellPathButNotFileSystem(str3)) {
                                                        this.OpenNewTab(wrapper5, true, false);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    return true;
                                }
                            }
                        }
                        if(fEnqExec) {
                            List<string> list2 = new List<string>();
                            list2.Add(wrapper.Path);
                            foreach(Address address in addressArray) {
                                using(IDLWrapper wrapper6 = new IDLWrapper(address.ITEMIDLIST)) {
                                    if(wrapper6.IsFolder) {
                                        return true;
                                    }
                                    if(wrapper6.HasPath && !wrapper6.IsLinkToDeadFolder) {
                                        list2.Add(wrapper6.Path);
                                    }
                                }
                            }
                            foreach(string str4 in list2) {
                                QTUtility.ExecutedPathsList.Add(str4);
                            }
                        }
                    }
                }
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception, null, false);
            }
            finally {
                if(ppshv != null) {
                    Marshal.ReleaseComObject(ppshv);
                }
                if(zero != IntPtr.Zero) {
                    PInvoke.CoTaskMemFree(zero);
                }
                if(ppidl != IntPtr.Zero) {
                    PInvoke.CoTaskMemFree(ppidl);
                }
            }
            return false;
        }

        private void HandleXBUTTON() {
            if(!base.Explorer.Busy) {
                MouseButtons mouseButtons = Control.MouseButtons;
                Keys modifierKeys = Control.ModifierKeys;
                switch(mouseButtons) {
                    case MouseButtons.XButton1:
                        if(modifierKeys == Keys.Control) {
                            this.NavigateToFirstOrLast(true);
                            return;
                        }
                        this.NavigateCurrentTab(true);
                        return;

                    case MouseButtons.XButton2:
                        if(modifierKeys == Keys.Control) {
                            this.NavigateToFirstOrLast(false);
                            return;
                        }
                        this.NavigateCurrentTab(false);
                        break;
                }
            }
        }

        private void HideSubDirTip(int iReason) {
            if((this.subDirTip != null) && this.subDirTip.IsShowing) {
                bool fForce = iReason < 0;
                if(fForce || !this.subDirTip.IsShownByKey) {
                    this.subDirTip.HideSubDirTip(fForce);
                    this.subDirIndex = -1;
                }
            }
            this.itemIndexDROPHILITED = -1;
        }

        private void HideSubDirTip_ExplorerInactivated() {
            if((this.subDirTip != null) && this.subDirTip.IsShowing) {
                this.subDirTip.OnExplorerInactivated();
            }
        }

        private void HideSubDirTip_Tab_Menu() {
            if(this.subDirTip_Tab != null) {
                this.subDirTip_Tab.HideMenu();
            }
        }

        private void HideTabSwitcher(bool fSwitch) {
            if((this.tabSwitcher != null) && this.tabSwitcher.IsShown) {
                this.tabSwitcher.HideSwitcher(fSwitch);
                this.tabControl1.SetPseudoHotIndex(-1);
            }
        }

        private void HideThumbnailTooltip(int iReason) {
            if((this.thumbnailTooltip != null) && this.thumbnailTooltip.IsShowing) {
                if(((iReason == 0) || (iReason == 7)) || (iReason == 9)) {
                    this.thumbnailTooltip.IsShownByKey = false;
                }
                if(this.thumbnailTooltip.HideToolTip()) {
                    this.thumbnailIndex = -1;
                }
            }
        }

        private void HideToolTipForDD() {
            this.tabForDD = null;
            this.iModKeyStateDD = 0;
            if(this.toolTipForDD != null) {
                this.toolTipForDD.Hide(this.tabControl1);
            }
            if(this.timerOnTab != null) {
                this.timerOnTab.Enabled = false;
            }
        }

        private void InitializeComponent() {
            this.components = new Container();
            this.buttonNavHistoryMenu = new ToolStripDropDownButton();
            this.tabControl1 = new QTabControl();
            this.CurrentTab = new QTabItem(string.Empty, string.Empty, this.tabControl1);
            this.contextMenuTab = new ContextMenuStripEx(this.components, false);
            this.contextMenuSys = new ContextMenuStripEx(this.components, false);
            this.tabControl1.SuspendLayout();
            this.contextMenuSys.SuspendLayout();
            this.contextMenuTab.SuspendLayout();
            base.SuspendLayout();
            bool flag = QTUtility.CheckConfig(Settings.ShowNavButtons);
            if(flag) {
                this.InitializeNavBtns(false);
            }
            this.buttonNavHistoryMenu.AutoSize = false;
            this.buttonNavHistoryMenu.DisplayStyle = ToolStripItemDisplayStyle.None;
            this.buttonNavHistoryMenu.Enabled = false;
            this.buttonNavHistoryMenu.Size = new Size(13, 0x15);
            this.buttonNavHistoryMenu.DropDown = new DropDownMenuBase(this.components, true, true, true);
            this.buttonNavHistoryMenu.DropDown.ItemClicked += new ToolStripItemClickedEventHandler(this.NavigationButton_DropDownMenu_ItemClicked);
            this.buttonNavHistoryMenu.DropDownOpening += new EventHandler(this.NavigationButtons_DropDownOpening);
            this.buttonNavHistoryMenu.DropDown.ImageList = QTUtility.ImageListGlobal;
            this.tabControl1.SetRedraw(false);
            this.tabControl1.TabPages.Add(this.CurrentTab);
            this.tabControl1.Dock = DockStyle.Fill;
            this.tabControl1.ContextMenuStrip = this.contextMenuTab;
            this.tabControl1.RefreshOptions(true);
            this.tabControl1.RowCountChanged += new QEventHandler(this.tabControl1_RowCountChanged);
            this.tabControl1.Deselecting += new QTabCancelEventHandler(this.tabControl1_Deselecting);
            this.tabControl1.Selecting += new QTabCancelEventHandler(this.tabControl1_Selecting);
            this.tabControl1.SelectedIndexChanged += new EventHandler(this.tabControl1_SelectedIndexChanged);
            this.tabControl1.GotFocus += new EventHandler(this.Controls_GotFocus);
            this.tabControl1.MouseEnter += new EventHandler(this.tabControl1_MouseEnter);
            this.tabControl1.MouseLeave += new EventHandler(this.tabControl1_MouseLeave);
            this.tabControl1.MouseDown += new MouseEventHandler(this.tabControl1_MouseDown);
            this.tabControl1.MouseUp += new MouseEventHandler(this.tabControl1_MouseUp);
            this.tabControl1.MouseMove += new MouseEventHandler(this.tabControl1_MouseMove);
            this.tabControl1.MouseDoubleClick += new MouseEventHandler(this.tabControl1_MouseDoubleClick);
            this.tabControl1.ItemDrag += new ItemDragEventHandler(this.tabControl1_ItemDrag);
            this.tabControl1.PointedTabChanged += new QTabCancelEventHandler(this.tabControl1_PointedTabChanged);
            this.tabControl1.TabCountChanged += new QTabCancelEventHandler(this.tabControl1_TabCountChanged);
            this.tabControl1.CloseButtonClicked += new QTabCancelEventHandler(this.tabControl1_CloseButtonClicked);
            this.tabControl1.TabIconMouseDown += new QTabCancelEventHandler(this.tabControl1_TabIconMouseDown);
            this.contextMenuTab.Items.Add(new ToolStripMenuItem());
            this.contextMenuTab.ShowImageMargin = false;
            this.contextMenuTab.ItemClicked += new ToolStripItemClickedEventHandler(this.contextMenuTab_ItemClicked);
            this.contextMenuTab.Opening += new CancelEventHandler(this.contextMenuTab_Opening);
            this.contextMenuTab.Closed += new ToolStripDropDownClosedEventHandler(this.contextMenuTab_Closed);
            this.contextMenuSys.Items.Add(new ToolStripMenuItem());
            this.contextMenuSys.ShowImageMargin = false;
            this.contextMenuSys.ItemClicked += new ToolStripItemClickedEventHandler(this.contextMenuSys_ItemClicked);
            this.contextMenuSys.Opening += new CancelEventHandler(this.contextMenuSys_Opening);
            base.Controls.Add(this.tabControl1);
            if(flag) {
                base.Controls.Add(this.toolStrip);
            }
            base.MinSize = new Size(150, QTUtility.TabHeight + 2);
            base.Height = QTUtility.TabHeight + 2;
            this.ContextMenuStrip = this.contextMenuSys;
            base.MouseDoubleClick += new MouseEventHandler(this.QTTabBarClass_MouseDoubleClick);
            this.tabControl1.ResumeLayout(false);
            this.contextMenuSys.ResumeLayout(false);
            this.contextMenuTab.ResumeLayout(false);
            if(flag) {
                this.toolStrip.ResumeLayout(false);
                this.toolStrip.PerformLayout();
            }
            base.ResumeLayout(false);
        }

        private void InitializeInstallation() {
            this.InitializeOpenedWindow();
            object locationURL = base.Explorer.LocationURL;
            if(this.ShellBrowser != null) {
                IntPtr pIDL = ShellMethods.ShellGetPath(this.ShellBrowser);
                if(pIDL != IntPtr.Zero) {
                    locationURL = ShellMethods.GetPath(pIDL);
                }
            }
            this.Explorer_NavigateComplete2(null, ref locationURL);
        }

        private void InitializeNavBtns(bool fSync) {
            this.toolStrip = new ToolStripEx();
            this.buttonBack = new ToolStripButton();
            this.buttonForward = new ToolStripButton();
            this.toolStrip.SuspendLayout();
            if(!QTUtility.ImageListGlobal.Images.ContainsKey("navBack")) {
                QTUtility.ImageListGlobal.Images.Add("navBack", Resources_Image.imgNavBack);
            }
            if(!QTUtility.ImageListGlobal.Images.ContainsKey("navFrwd")) {
                QTUtility.ImageListGlobal.Images.Add("navFrwd", Resources_Image.imgNavFwd);
            }
            this.toolStrip.Dock = QTUtility.CheckConfig(Settings.NavButtonsOnRight) ? DockStyle.Right : DockStyle.Left;
            this.toolStrip.AutoSize = false;
            this.toolStrip.CanOverflow = false;
            this.toolStrip.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            this.toolStrip.Items.AddRange(new ToolStripItem[] { this.buttonBack, this.buttonForward, this.buttonNavHistoryMenu });
            this.toolStrip.Renderer = new ToolbarRenderer();
            this.toolStrip.Width = 0x3f;
            this.toolStrip.TabStop = false;
            this.toolStrip.BackColor = System.Drawing.Color.Transparent;
            this.buttonBack.AutoSize = false;
            this.buttonBack.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.buttonBack.Enabled = fSync ? ((this.navBtnsFlag & 1) != 0) : false;
            this.buttonBack.Image = QTUtility.ImageListGlobal.Images["navBack"];
            this.buttonBack.Size = new Size(0x15, 0x15);
            this.buttonBack.Click += new EventHandler(this.NavigationButtons_Click);
            this.buttonForward.AutoSize = false;
            this.buttonForward.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.buttonForward.Enabled = fSync ? ((this.navBtnsFlag & 2) != 0) : false;
            this.buttonForward.Image = QTUtility.ImageListGlobal.Images["navFrwd"];
            this.buttonForward.Size = new Size(0x15, 0x15);
            this.buttonForward.Click += new EventHandler(this.NavigationButtons_Click);
        }

        private void InitializeOpenedWindow() {
            this.IsShown = true;
            QTUtility.RegisterPrimaryInstance(this.ExplorerHandle, this);
            this.InstallHooks();
            this.pluginManager = new PluginManager(this);
            if(!this.SyncButtonBarCurrent(0x100)) {
                new WaitTimeoutCallback(QTTabBarClass.WaitTimeout).BeginInvoke(0x7d0, new AsyncCallback(this.AsyncComplete_ButtonBarPlugin), null);
            }
            if(QTUtility.CheckConfig(Settings.HideMenuBar)) {
                WindowUtils.ShowMenuBar(false, base.ReBarHandle);
            }
            if(QTUtility.CheckConfig(Settings.SaveTransparency) && (QTUtility.WindowAlpha < 0xff)) {
                PInvoke.SetWindowLongPtr(this.ExplorerHandle, -20, PInvoke.Ptr_OP_OR(PInvoke.GetWindowLongPtr(this.ExplorerHandle, -20), 0x80000));
                PInvoke.SetLayeredWindowAttributes(this.ExplorerHandle, 0, QTUtility.WindowAlpha, 2);
            }

            listViewWrapper = new ListViewWrapper(ShellBrowser, ExplorerHandle);
            listViewWrapper.SVDestroy       += ListView_SVDestroy;
            listViewWrapper.SVMouseActivate += ListView_SVMouseActivate;
            listViewWrapper.ItemInserted    += ListView_ItemInserted;
            listViewWrapper.ItemDeleted     += ListView_ItemDeleted;
            listViewWrapper.ItemActivated   += ListView_ItemActivated;
            listViewWrapper.AllItemsDeleted += ListView_AllItemsDeleted;
            listViewWrapper.SelectionChanged+= ListView_SelectionChanged;
            listViewWrapper.BeginDrag       += ListView_BeginDrag;
            listViewWrapper.DropHilighted   += ListView_DropHilighted;
            listViewWrapper.HotTrack        += ListView_HotTrack;
            listViewWrapper.MiddleClick     += ListView_MiddleClick;
            listViewWrapper.DoubleClick     += ListView_DoubleClick;
            listViewWrapper.KeyDown         += ListView_KeyDown;
            listViewWrapper.GetInfoTip      += ListView_GetInfoTip;
            listViewWrapper.BeginLabelEdit  += ListView_BeginLabelEdit;
            listViewWrapper.EndLabelEdit    += ListView_EndLabelEdit;
            listViewWrapper.BeginScroll     += ListView_BeginScroll;
            listViewWrapper.MouseLeave      += ListView_MouseLeave;
            
        }

        private static void InitializeStaticFields() {
            fInitialized = true;
            Application.EnableVisualStyles();
        }

        private void InitializeSysMenu(bool fText) {
            bool flag = false;
            if(this.tsmiGroups == null) {
                flag = true;
                this.tsmiGroups = new ToolStripMenuItem(QTUtility.ResMain[12]);
                this.tsmiUndoClose = new ToolStripMenuItem(QTUtility.ResMain[13]);
                this.tsmiLastActiv = new ToolStripMenuItem(QTUtility.ResMain[14]);
                this.tsmiExecuted = new ToolStripMenuItem(QTUtility.ResMain[15]);
                this.tsmiBrowseFolder = new ToolStripMenuItem(QTUtility.ResMain[0x10] + "...");
                this.tsmiCloseAllButCurrent = new ToolStripMenuItem(QTUtility.ResMain[0x11]);
                this.tsmiCloseWindow = new ToolStripMenuItem(QTUtility.ResMain[0x12]);
                this.tsmiOption = new ToolStripMenuItem(QTUtility.ResMain[0x13]);
                this.tsmiLockToolbar = new ToolStripMenuItem(QTUtility.ResMain[0x20]);
                this.tsmiMergeWindows = new ToolStripMenuItem(QTUtility.ResMain[0x21]);
                this.tssep_Sys1 = new ToolStripSeparator();
                this.tssep_Sys2 = new ToolStripSeparator();
                this.contextMenuSys.SuspendLayout();
                this.contextMenuSys.Items[0].Dispose();
                this.contextMenuSys.Items.AddRange(new ToolStripItem[] { this.tsmiGroups, this.tsmiUndoClose, this.tsmiLastActiv, this.tsmiExecuted, this.tssep_Sys1, this.tsmiBrowseFolder, this.tsmiCloseAllButCurrent, this.tsmiCloseWindow, this.tsmiMergeWindows, this.tsmiLockToolbar, this.tssep_Sys2, this.tsmiOption });
                DropDownMenuReorderable reorderable = new DropDownMenuReorderable(this.components, true, false);
                reorderable.ReorderFinished += new MenuReorderedEventHandler(this.menuitemGroups_ReorderFinished);
                reorderable.ItemRightClicked += new ItemRightClickedEventHandler(MenuUtility.GroupMenu_ItemRightClicked);
                reorderable.ItemMiddleClicked += new ItemRightClickedEventHandler(this.ddrmrGroups_ItemMiddleClicked);
                reorderable.ImageList = QTUtility.ImageListGlobal;
                this.tsmiGroups.DropDown = reorderable;
                this.tsmiGroups.DropDownItemClicked += new ToolStripItemClickedEventHandler(this.menuitemGroups_DropDownItemClicked);
                DropDownMenuReorderable reorderable2 = new DropDownMenuReorderable(this.components);
                reorderable2.ReorderEnabled = false;
                reorderable2.MessageParent = base.Handle;
                reorderable2.ImageList = QTUtility.ImageListGlobal;
                reorderable2.ItemRightClicked += new ItemRightClickedEventHandler(this.ddmrUndoClose_ItemRightClicked);
                this.tsmiUndoClose.DropDown = reorderable2;
                this.tsmiUndoClose.DropDownItemClicked += new ToolStripItemClickedEventHandler(this.menuitemUndoClose_DropDownItemClicked);
                DropDownMenuReorderable reorderable3 = new DropDownMenuReorderable(this.components);
                reorderable3.MessageParent = base.Handle;
                reorderable3.ItemRightClicked += new ItemRightClickedEventHandler(this.menuitemExecuted_ItemRightClicked);
                reorderable3.ItemClicked += new ToolStripItemClickedEventHandler(this.menuitemExecuted_DropDownItemClicked);
                reorderable3.ImageList = QTUtility.ImageListGlobal;
                this.tsmiExecuted.DropDown = reorderable3;
                this.tssep_Sys1.Enabled = false;
                this.tssep_Sys2.Enabled = false;
                this.contextMenuSys.ResumeLayout(false);
            }
            if(!flag && fText) {
                this.tsmiGroups.Text = QTUtility.ResMain[12];
                this.tsmiUndoClose.Text = QTUtility.ResMain[13];
                this.tsmiLastActiv.Text = QTUtility.ResMain[14];
                this.tsmiExecuted.Text = QTUtility.ResMain[15];
                this.tsmiBrowseFolder.Text = QTUtility.ResMain[0x10] + "...";
                this.tsmiCloseAllButCurrent.Text = QTUtility.ResMain[0x11];
                this.tsmiCloseWindow.Text = QTUtility.ResMain[0x12];
                this.tsmiOption.Text = QTUtility.ResMain[0x13];
                this.tsmiLockToolbar.Text = QTUtility.ResMain[0x20];
                this.tsmiMergeWindows.Text = QTUtility.ResMain[0x21];
            }
        }

        private void InitializeTabMenu(bool fText) {
            bool flag = false;
            if(this.tsmiClose == null) {
                flag = true;
                this.tsmiClose = new ToolStripMenuItem(QTUtility.ResMain[0]);
                this.tsmiCloseRight = new ToolStripMenuItem(QTUtility.ResMain[1]);
                this.tsmiCloseLeft = new ToolStripMenuItem(QTUtility.ResMain[2]);
                this.tsmiCloseAllButThis = new ToolStripMenuItem(QTUtility.ResMain[3]);
                this.tsmiAddToGroup = new ToolStripMenuItem(QTUtility.ResMain[4]);
                this.tsmiCreateGroup = new ToolStripMenuItem(QTUtility.ResMain[5] + "...");
                this.tsmiLockThis = new ToolStripMenuItem(QTUtility.ResMain[6]);
                this.tsmiCloneThis = new ToolStripMenuItem(QTUtility.ResMain[7]);
                this.tsmiCreateWindow = new ToolStripMenuItem(QTUtility.ResMain[8]);
                this.tsmiCopy = new ToolStripMenuItem(QTUtility.ResMain[9]);
                this.tsmiProp = new ToolStripMenuItem(QTUtility.ResMain[10]);
                this.tsmiHistory = new ToolStripMenuItem(QTUtility.ResMain[11]);
                this.tsmiTabOrder = new ToolStripMenuItem(QTUtility.ResMain[0x1c]);
                this.menuTextBoxTabAlias = new ToolStripTextBox();
                this.tssep_Tab1 = new ToolStripSeparator();
                this.tssep_Tab2 = new ToolStripSeparator();
                this.tssep_Tab3 = new ToolStripSeparator();
                this.contextMenuTab.SuspendLayout();
                this.contextMenuTab.Items[0].Dispose();
                this.contextMenuTab.Items.AddRange(new ToolStripItem[] { this.tsmiClose, this.tsmiCloseRight, this.tsmiCloseLeft, this.tsmiCloseAllButThis, this.tssep_Tab1, this.tsmiAddToGroup, this.tsmiCreateGroup, this.tssep_Tab2, this.tsmiLockThis, this.tsmiCloneThis, this.tsmiCreateWindow, this.tsmiCopy, this.tsmiTabOrder, this.tssep_Tab3, this.tsmiProp, this.tsmiHistory });
                this.tsmiAddToGroup.DropDownItemClicked += new ToolStripItemClickedEventHandler(this.menuitemAddToGroup_DropDownItemClicked);
                ((ToolStripDropDownMenu)this.tsmiAddToGroup.DropDown).ImageList = QTUtility.ImageListGlobal;
                this.tsmiHistory.DropDown = new DropDownMenuBase(this.components, true, true, true);
                this.tsmiHistory.DropDownItemClicked += new ToolStripItemClickedEventHandler(this.menuitemHistory_DropDownItemClicked);
                ((ToolStripDropDownMenu)this.tsmiHistory.DropDown).ImageList = QTUtility.ImageListGlobal;
                this.menuTextBoxTabAlias.Text = this.menuTextBoxTabAlias.ToolTipText = QTUtility.ResMain[0x1b];
                this.menuTextBoxTabAlias.GotFocus += new EventHandler(this.menuTextBoxTabAlias_GotFocus);
                this.menuTextBoxTabAlias.LostFocus += new EventHandler(this.menuTextBoxTabAlias_LostFocus);
                this.menuTextBoxTabAlias.KeyPress += new KeyPressEventHandler(this.menuTextBoxTabAlias_KeyPress);
                this.tsmiTabOrder.DropDown = new ContextMenuStripEx(this.components, false);
                this.tssep_Tab1.Enabled = false;
                this.tssep_Tab2.Enabled = false;
                this.tssep_Tab3.Enabled = false;
                this.contextMenuTab.ResumeLayout(false);
            }
            if(!flag && fText) {
                this.tsmiClose.Text = QTUtility.ResMain[0];
                this.tsmiCloseRight.Text = QTUtility.ResMain[1];
                this.tsmiCloseLeft.Text = QTUtility.ResMain[2];
                this.tsmiCloseAllButThis.Text = QTUtility.ResMain[3];
                this.tsmiAddToGroup.Text = QTUtility.ResMain[4];
                this.tsmiCreateGroup.Text = QTUtility.ResMain[5] + "...";
                this.tsmiLockThis.Text = QTUtility.ResMain[6];
                this.tsmiCloneThis.Text = QTUtility.ResMain[7];
                this.tsmiCreateWindow.Text = QTUtility.ResMain[8];
                this.tsmiCopy.Text = QTUtility.ResMain[9];
                this.tsmiProp.Text = QTUtility.ResMain[10];
                this.tsmiHistory.Text = QTUtility.ResMain[11];
                this.tsmiTabOrder.Text = QTUtility.ResMain[0x1c];
                this.menuTextBoxTabAlias.Text = this.menuTextBoxTabAlias.ToolTipText = QTUtility.ResMain[0x1b];
            }
        }

        private void InstallHooks() {
            this.hookProc_Key = new HookProc(this.CallbackKeyboardProc);
            this.hookProc_Mouse = new HookProc(this.CallbackMouseProc);
            this.hookProc_GetMsg = new HookProc(this.CallbackGetMsgProc);
            int currentThreadId = PInvoke.GetCurrentThreadId();
            this.hHook_Key = PInvoke.SetWindowsHookEx(2, this.hookProc_Key, IntPtr.Zero, currentThreadId);
            this.hHook_Mouse = PInvoke.SetWindowsHookEx(7, this.hookProc_Mouse, IntPtr.Zero, currentThreadId);
            this.hHook_Msg = PInvoke.SetWindowsHookEx(3, this.hookProc_GetMsg, IntPtr.Zero, currentThreadId);
            this.explorerController = new NativeWindowController(this.ExplorerHandle);
            this.explorerController.MessageCaptured += new NativeWindowController.MessageEventHandler(this.explorerController_MessageCaptured);
            if(base.ReBarHandle != IntPtr.Zero) {
                this.rebarController = new NativeWindowController(base.ReBarHandle);
                this.rebarController.MessageCaptured += new NativeWindowController.MessageEventHandler(this.rebarController_MessageCaptured);
                if(QTUtility.CheckConfig(Settings.ToolbarBGColor)) {
                    if(QTUtility.DefaultRebarCOLORREF == -1) {
                        QTUtility.DefaultRebarCOLORREF = (int)PInvoke.SendMessage(base.ReBarHandle, 0x414, IntPtr.Zero, IntPtr.Zero);
                    }
                    int num2 = QTUtility2.MakeCOLORREF(QTUtility.RebarBGColor);
                    PInvoke.SendMessage(base.ReBarHandle, 0x413, IntPtr.Zero, (IntPtr)num2);
                }
            }
            if(QTUtility.IsVista) {
                this.TravelToolBarHandle = this.GetTravelToolBarWindow32();
                if(this.TravelToolBarHandle != IntPtr.Zero) {
                    this.travelBtnController = new NativeWindowController(this.TravelToolBarHandle);
                    this.travelBtnController.MessageCaptured += new NativeWindowController.MessageEventHandler(this.travelBtnController_MessageCaptured);
                }
            }
            this.dropTargetWrapper = new DropTargetWrapper(this);
            this.dropTargetWrapper.DragFileEnter += new DropTargetWrapper.DragFileEnterEventHandler(this.dropTargetWrapper_DragFileEnter);
            this.dropTargetWrapper.DragFileOver += new DragEventHandler(this.dropTargetWrapper_DragFileOver);
            this.dropTargetWrapper.DragFileLeave += new EventHandler(this.dropTargetWrapper_DragFileLeave);
            this.dropTargetWrapper.DragFileDrop += new DropTargetWrapper.DragFileDropEventHandler(this.dropTargetWrapper_DragFileDrop);
        }

        private bool IsFolderTreeVisible() {
            IntPtr ptr;
            return (!QTUtility.IsVista && (0 == this.ShellBrowser.GetControlWindow(3, out ptr)));
        }

        private static bool IsSearchResultFolder(string path) {
            if(QTUtility.IsVista) {
                return path.StartsWith(QTUtility.ResMisc[2], StringComparison.OrdinalIgnoreCase);
            }
            return path.StartsWith(QTUtility.PATH_SEARCHFOLDER, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSpecialFolderNeedsToTravel(string path) {
            int index = path.IndexOf("*?*?*");
            if(index != -1) {
                path = path.Substring(0, index);
            }
            if(!IsSearchResultFolder(path)) {
                if(string.Equals("::{13E7F612-F261-4391-BEA2-39DF4F3FA311}", path, StringComparison.OrdinalIgnoreCase)) {
                    return true;
                }
                if(!path.StartsWith(QTUtility.ResMisc[0], StringComparison.OrdinalIgnoreCase) && (!path.EndsWith(QTUtility.ResMisc[0], StringComparison.OrdinalIgnoreCase) || Path.IsPathRooted(path))) {
                    return false;
                }
            }
            return true;
        }

        private bool ListView_SVDestroy() {
            // I'm pretty sure this is unnecessary.  Refreshing doesn't seem to
            // produce this message on any platform.
            HandleF5();
            return false;
        }

        private bool ListView_SVMouseActivate(ref int result) {
            // The purpose of this is probably to prevent accidentally
            // renaming an item when clicking out of a SubDirTip menu.
            if((subDirTip != null && subDirTip.MenuIsShowing) || (subDirTip_Tab != null && subDirTip_Tab.MenuIsShowing)) {
                if(listViewWrapper.GetSelectedCount() == 1 && listViewWrapper.HotItemIsSelected()) {
                    result = 2;
                    if(subDirTip != null) {
                        subDirTip.HideMenu();
                    }
                    HideSubDirTip_Tab_Menu();
                    listViewWrapper.SetFocus();
                    return true;
                }
            }
            return false;
        }

        private bool ListView_ItemInserted() { 
            return ListView_ItemDeleted();
        }

        private bool ListView_ItemDeleted() {
            IntPtr ptr;
            if(QTUtility.instanceManager.TryGetButtonBarHandle(this.ExplorerHandle, out ptr)) {
                QTUtility2.SendCOPYDATASTRUCT(ptr, (IntPtr)14, null, IntPtr.Zero);
            }
            if(!QTUtility.CheckConfig(Settings.NoShowSubDirTips)) {
                HideSubDirTip(1);
            }
            return false;
        }

        private bool ListView_ItemActivated(Keys modKeys) {
            if(this.timerSelectionChanged != null) {
                this.timerSelectionChanged.Enabled = false;
            }
            int num = listViewWrapper.GetSelectedCount();
            bool fEnqExec = !QTUtility.CheckConfig(Settings.NoRecentFiles);
            if(num != 1 || modKeys != Keys.None || fEnqExec) {
                if(modKeys == Keys.Alt) {
                    return false;
                }
                if(fEnqExec || num > 1 || (modKeys & Keys.Shift) != Keys.None || modKeys == Keys.Control) {
                    return this.HandleTabFolderActions(-1, modKeys, fEnqExec);
                }
            }
            return false;
        }

        private bool ListView_AllItemsDeleted() {
            // No idea for this one.
            // TODO: Figure out how important this is.
            this.HandleF5();
            return false;
        }

        private bool ListView_SelectionChanged() {
            if(this.pluginManager != null && this.pluginManager.SelectionChangeAttached) {
                if(this.timerSelectionChanged == null) {
                    this.timerSelectionChanged = new System.Windows.Forms.Timer(this.components);
                    this.timerSelectionChanged.Interval = 250;
                    this.timerSelectionChanged.Tick += new EventHandler(this.timerSelectionChanged_Tick);
                }
                else {
                    this.timerSelectionChanged.Enabled = false;
                }
                this.timerSelectionChanged.Enabled = true;
            }
            return false;
        }

        private bool ListView_BeginDrag() {
            // This won't be necessary it seems.  On Windows 7, when you
            // start to drag, a MOUSELEAVE message is sent, which hides
            // the SubDirTip anyway.
            this.HideSubDirTip(0xff);
            return false;
        }

        private bool ListView_DropHilighted(int iItem) {
            if(!QTUtility.CheckConfig(Settings.NoShowSubDirTips)) {
                if(iItem != itemIndexDROPHILITED) {
                    if(iItem == -1) {
                        if(timer_HoverSubDirTipMenu != null) {
                            timer_HoverSubDirTipMenu.Enabled = false;
                        }
                        //if(subDirTip != null) {
                        //    subDirTip.HideMenu();
                        //    HideSubDirTip(10);
                        //}
                        itemIndexDROPHILITED = -1;
                    }
                    else {
                        if(timer_HoverSubDirTipMenu == null) {
                            timer_HoverSubDirTipMenu = new System.Windows.Forms.Timer(components);
                            timer_HoverSubDirTipMenu.Interval = 1200;
                            timer_HoverSubDirTipMenu.Tick += new EventHandler(timer_HoverSubDirTipMenu_Tick);
                        }
                        itemIndexDROPHILITED = iItem;
                        timer_HoverSubDirTipMenu.Enabled = false;
                        timer_HoverSubDirTipMenu.Enabled = true;
                    }
                }
            }
            return false;
        }

        private bool ListView_HotTrack(int iItem) { 
            if(QTUtility.CheckConfig(Settings.ShowTooltipPreviews) || !QTUtility.CheckConfig(Settings.NoShowSubDirTips)) {

                Keys modifierKeys = Control.ModifierKeys;
                if(QTUtility.CheckConfig(Settings.ShowTooltipPreviews)) {
                    if((this.thumbnailTooltip != null) && (this.thumbnailTooltip.IsShowing || this.fThumbnailPending)) {
                        if(!QTUtility.CheckConfig(Settings.PreviewsWithShift) ^ (modifierKeys == Keys.Shift)) {
                            if(iItem != this.thumbnailIndex) {
                                if(iItem > -1 && listViewWrapper.IsTrackingItemName()) {
                                    if(this.ShowThumbnailTooltip(iItem, Control.MousePosition, false)) {
                                        return false;
                                    }
                                }
                                if(this.thumbnailTooltip.HideToolTip()) {
                                    this.thumbnailIndex = -1;
                                }
                            }
                        }
                        else if(this.thumbnailTooltip.HideToolTip()) {
                            this.thumbnailIndex = -1;
                        }
                    }
                    if(this.timer_HoverThumbnail == null) {
                        this.timer_HoverThumbnail = new System.Windows.Forms.Timer(this.components);
                        this.timer_HoverThumbnail.Interval = (int)(SystemInformation.MouseHoverTime * 0.2);
                        this.timer_HoverThumbnail.Tick += new EventHandler(this.timer_HoverThumbnail_Tick);
                    }
                    this.timer_HoverThumbnail.Enabled = false;
                    this.timer_HoverThumbnail.Enabled = true;
                }
                if(!QTUtility.CheckConfig(Settings.NoShowSubDirTips)) {
                    if(!QTUtility.CheckConfig(Settings.SubDirTipsWithShift) ^ (modifierKeys == Keys.Shift)) {
                        if((this.subDirIndex == iItem) && (QTUtility.IsVista || (iItem != -1))) {
                            return false;
                        }
                        if(QTUtility.IsVista) {
                            this.subDirIndex = iItem;
                        }
                        if(iItem > -1 && listViewWrapper.IsTrackingItemName()) {
                            if(this.ShowSubDirTip(iItem, false)) {
                                if(!QTUtility.IsVista) {
                                    this.subDirIndex = iItem;
                                }
                                return false;
                            }
                        }
                    }
                    this.HideSubDirTip(2);
                    this.subDirIndex = -1;
                }
            }
            return false;
        }

        private bool ListView_MiddleClick(Point pt) {
            if(QTUtility.CheckConfig(Settings.NoCaptureMidClick)) {
                return false;
            }
            int index = listViewWrapper.HitTest(pt, false);
            if(index <= -1) {
                return false;
            }
            Keys modifierKeys = Control.ModifierKeys;
            if(modifierKeys != (Keys.Alt | Keys.Control | Keys.Shift)) {
                modifierKeys &= ~Keys.Alt;
                if(QTUtility.CheckConfig(Settings.MidClickNewWindow)) {
                    switch(modifierKeys) {
                        case Keys.Control:
                            modifierKeys = Keys.None;
                            break;

                        case (Keys.Control | Keys.Shift):
                            modifierKeys = Keys.Shift;
                            break;

                        default:
                            modifierKeys = Keys.Control;
                            break;
                    }
                }
                else if(modifierKeys == (Keys.Control | Keys.Shift)) {
                    modifierKeys = Keys.Control;
                }
            }
            return this.HandleTabFolderActions(index, modifierKeys, false);
        }

        private bool ListView_DoubleClick(Point pt) {
            if(!QTUtility.CheckConfig(Settings.NoDblClickUpLevel) && listViewWrapper.IsTrackingBackground()) {
                this.UpOneLevel();
                return true;
            }
            return false;
        }

        private bool ListView_KeyDown(Keys key) {
            if(QTUtility.CheckConfig(Settings.ShowTooltipPreviews)) {
                if(QTUtility.CheckConfig(Settings.PreviewsWithShift)) {
                    if(key != Keys.ShiftKey) {
                        this.HideThumbnailTooltip(2);
                    }
                }
                else {
                    this.HideThumbnailTooltip(2);
                }
            }
            if(!QTUtility.CheckConfig(Settings.NoShowSubDirTips)) {
                if(QTUtility.CheckConfig(Settings.SubDirTipsWithShift)) {
                    if(key != Keys.ShiftKey) {
                        this.HideSubDirTip(3);
                    }
                }
                else if(key != Keys.ControlKey) {
                    this.HideSubDirTip(3);
                }
            }

            if(QTUtility.CheckConfig(Settings.CursorLoop) && Control.ModifierKeys == Keys.None) {
                if(key == Keys.Left || key == Keys.Right || key == Keys.Up || key == Keys.Down) {
                    return this.HandleLVKEYDOWN_CursorLoop(key);
                }
            }
            
            return false;
        }

        private bool ListView_GetInfoTip(int iItem, bool byKey) {
            if(QTUtility.CheckConfig(Settings.ShowTooltipPreviews) && (!QTUtility.CheckConfig(Settings.PreviewsWithShift) ^ (Control.ModifierKeys == Keys.Shift))) {
                if(((this.thumbnailTooltip != null) && this.thumbnailTooltip.IsShowing) && (iItem == this.thumbnailIndex)) {
                    return true;
                }
                else if((this.timer_HoverThumbnail != null) && this.timer_HoverThumbnail.Enabled) {
                    return true;
                }
                else if(byKey) {
                    Rectangle rect = listViewWrapper.GetFocusedItemRect();
                    return this.ShowThumbnailTooltip(iItem, new Point(rect.Right - 32, rect.Bottom - 16), true);
                }
                else {
                    return this.ShowThumbnailTooltip(iItem, Control.MousePosition, false);
                }
            }
            return false;
        }

        private bool ListView_BeginLabelEdit(LVITEM item) {
            if(QTUtility.IsVista || QTUtility.CheckConfig(Settings.ExtWhileRenaming)) {
                return false;
            }
            if(item.lParam != IntPtr.Zero) {
                IntPtr ptr2 = ShellMethods.ShellGetPath(this.ShellBrowser);
                if((ptr2 != IntPtr.Zero)) {
                    IntPtr ptr3 = PInvoke.ILCombine(ptr2, item.lParam);
                    HandleRenaming(listViewWrapper, ptr3, this);
                    PInvoke.CoTaskMemFree(ptr2);
                    PInvoke.CoTaskMemFree(ptr3);
                }
            }
            return false;
        }

        private bool ListView_EndLabelEdit(LVITEM item) {
            if(item.pszText == IntPtr.Zero) {
                return false;
            }
            IShellView ppshv = null;
            IntPtr zero = IntPtr.Zero;
            IntPtr ptr5 = IntPtr.Zero;
            IntPtr pIDL = IntPtr.Zero;
            try {
                if(this.ShellBrowser.QueryActiveShellView(out ppshv) == 0) {
                    IFolderView view2 = (IFolderView)ppshv;
                    if(view2.Item(item.iItem, out zero) == 0) {
                        ptr5 = ShellMethods.ShellGetPath(this.ShellBrowser);
                        pIDL = PInvoke.ILCombine(ptr5, zero);
                        string displayName = ShellMethods.GetDisplayName(pIDL, true);
                        string str2 = Marshal.PtrToStringUni(item.pszText);
                        if(displayName != str2) {
                            this.HandleF5();
                        }
                    }
                }
            }
            catch {
            }
            finally {
                if(ppshv != null) {
                    Marshal.ReleaseComObject(ppshv);
                }
                if(zero != IntPtr.Zero) {
                    PInvoke.CoTaskMemFree(zero);
                }
                if(ptr5 != IntPtr.Zero) {
                    PInvoke.CoTaskMemFree(ptr5);
                }
                if(pIDL != IntPtr.Zero) {
                    PInvoke.CoTaskMemFree(pIDL);
                }
            }
            return false;
        }
   
        private bool ListView_BeginScroll() {
            if(QTUtility.CheckConfig(Settings.ShowTooltipPreviews)) {
                this.HideThumbnailTooltip(8);
            }
            if(!QTUtility.CheckConfig(Settings.NoShowSubDirTips)) {
                this.HideSubDirTip(8);
            }
            return false;
        }
        
        private bool ListView_MouseLeave() {
            this.HideThumbnailTooltip(4);
            if(this.timer_HoverThumbnail != null) {
                this.timer_HoverThumbnail.Enabled = false;
            }
            if(((this.subDirTip != null) && !this.subDirTip.MouseIsOnThis()) && !this.subDirTip.MenuIsShowing) {
                this.HideSubDirTip(5);
            }
            return false;
        }

        private string MakeTravelBtnTooltipText(bool fBack) {
            string path = string.Empty;
            if(fBack) {
                string[] historyBack = this.CurrentTab.GetHistoryBack();
                if(historyBack.Length > 1) {
                    path = historyBack[1];
                }
            }
            else {
                string[] historyForward = this.CurrentTab.GetHistoryForward();
                if(historyForward.Length > 0) {
                    path = historyForward[0];
                }
            }
            if(path.Length > 0) {
                string str2 = QTUtility2.MakePathDisplayText(path, false);
                if(!string.IsNullOrEmpty(str2)) {
                    return str2;
                }
            }
            return path;
        }

        private void menuitemAddToGroup_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            string str3;
            string text = e.ClickedItem.Text;
            string currentPath = this.ContextMenuedTab.CurrentPath;
            bool flag = Control.ModifierKeys == Keys.Control;
            if(QTUtility.GroupPathsDic.TryGetValue(text, out str3)) {
                if(str3 == null) {
                    str3 = string.Empty;
                }
                if(!flag) {
                    foreach(string str4 in str3.Split(QTUtility.SEPARATOR_CHAR)) {
                        if(string.Equals(str4, currentPath, StringComparison.CurrentCultureIgnoreCase)) {
                            return;
                        }
                    }
                }
                QTUtility.GroupPathsDic[text] = ((str3.Length == 0) ? string.Empty : (str3 + ";")) + currentPath;
                QTUtility.SaveGroupsReg();
            }
        }

        private void menuitemExecuted_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            try {
                string toolTipText = e.ClickedItem.ToolTipText;
                ProcessStartInfo startInfo = new ProcessStartInfo(toolTipText);
                startInfo.WorkingDirectory = Path.GetDirectoryName(toolTipText);
                startInfo.ErrorDialog = true;
                startInfo.ErrorDialogParentHandle = this.ExplorerHandle;
                Process.Start(startInfo);
                QTUtility.ExecutedPathsList.Add(toolTipText);
            }
            catch {
                SystemSounds.Hand.Play();
            }
        }

        private void menuitemExecuted_ItemRightClicked(object sender, ItemRightClickedEventArgs e) {
            using(IDLWrapper wrapper = new IDLWrapper(e.ClickedItem.ToolTipText)) {
                e.HRESULT = ShellMethods.PopUpSystemContextMenu(wrapper, e.IsKey ? e.Point : Control.MousePosition, ref this.iContextMenu2, ((DropDownMenuReorderable)sender).Handle, true);
            }
            if(e.HRESULT == 0xffff) {
                QTUtility.ExecutedPathsList.Remove(e.ClickedItem.ToolTipText);
                e.ClickedItem.Dispose();
            }
        }

        private void menuitemGroups_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            Keys modifierKeys = Control.ModifierKeys;
            string text = e.ClickedItem.Text;
            if(modifierKeys == (Keys.Control | Keys.Shift)) {
                if(QTUtility.StartUpGroupList.Contains(text)) {
                    QTUtility.StartUpGroupList.Remove(text);
                }
                else {
                    QTUtility.StartUpGroupList.Add(text);
                }
            }
            else {
                this.OpenGroup(text, modifierKeys == Keys.Control);
            }
        }

        private void menuitemGroups_ReorderFinished(object sender, ToolStripItemClickedEventArgs e) {
            QTUtility.RefreshGroupMenuesOnReorderFinished(this.tsmiGroups.DropDownItems);
            SyncTaskBarMenu();
        }

        private void menuitemHistory_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            QMenuItem clickedItem = e.ClickedItem as QMenuItem;
            if((this.ContextMenuedTab != null) && (clickedItem != null)) {
                MenuItemArguments menuItemArguments = clickedItem.MenuItemArguments;
                switch(Control.ModifierKeys) {
                    case Keys.Shift:
                        this.CloneTabButton(this.ContextMenuedTab, null, true, -1);
                        this.NavigateToHistory(new object[] { menuItemArguments.Path, menuItemArguments.IsBack, menuItemArguments.Index });
                        return;

                    case Keys.Control: {
                            using(IDLWrapper wrapper = new IDLWrapper(menuItemArguments.Path)) {
                                this.OpenNewWindow(wrapper);
                                return;
                            }
                        }
                    default:
                        this.tabControl1.SelectTab(this.ContextMenuedTab);
                        this.NavigateToHistory(new object[] { menuItemArguments.Path, menuItemArguments.IsBack, menuItemArguments.Index });
                        return;
                }
            }
        }

        private void menuitemTabOrder_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            if(e.ClickedItem.Name == "Name") {
                this.ReorderTab(0, false);
            }
            else if(e.ClickedItem.Name == "Drive") {
                this.ReorderTab(1, false);
            }
            else if(e.ClickedItem.Name == "Active") {
                this.ReorderTab(2, false);
            }
            else if(e.ClickedItem.Name == "Rev") {
                this.ReorderTab(3, false);
            }
        }

        private void menuitemUndoClose_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            QMenuItem clickedItem = (QMenuItem)e.ClickedItem;
            if(Control.ModifierKeys != Keys.Control) {
                this.OpenNewTab(clickedItem.Path, false);
            }
            else {
                using(IDLWrapper wrapper = new IDLWrapper(clickedItem.Path)) {
                    this.OpenNewWindow(wrapper);
                }
            }
        }

        private void menuTextBoxTabAlias_GotFocus(object sender, EventArgs e) {
            this.menuTextBoxTabAlias.ForeColor = SystemColors.WindowText;
            if(this.menuTextBoxTabAlias.TextBox.ImeMode != ImeMode.On) {
                this.menuTextBoxTabAlias.TextBox.ImeMode = ImeMode.On;
            }
            if(this.menuTextBoxTabAlias.Text == QTUtility.ResMain[0x1b]) {
                this.menuTextBoxTabAlias.Text = string.Empty;
            }
        }

        private void menuTextBoxTabAlias_KeyPress(object sender, KeyPressEventArgs e) {
            if(e.KeyChar == '\r') {
                e.Handled = true;
                this.contextMenuTab.Close(ToolStripDropDownCloseReason.ItemClicked);
            }
        }

        private void menuTextBoxTabAlias_LostFocus(object sender, EventArgs e) {
            string text = this.menuTextBoxTabAlias.Text;
            if(text.Length == 0) {
                this.menuTextBoxTabAlias.Text = QTUtility.ResMain[0x1b];
            }
            if((text != QTUtility.ResMain[0x1b]) && (this.ContextMenuedTab != null)) {
                this.ContextMenuedTab.Comment = text;
                this.ContextMenuedTab.RefreshRectangle();
                this.tabControl1.Refresh();
            }
            this.menuTextBoxTabAlias.TextBox.SelectionStart = 0;
        }

        private void MergeAllWindows() {
            List<IntPtr> list = new List<IntPtr>();
            foreach(IntPtr ptr in QTUtility.instanceManager.ExplorerHandles()) {
                if(ptr != this.ExplorerHandle) {
                    list.Add(ptr);
                }
            }
            try {
                this.tabControl1.SetRedraw(false);
                foreach(IntPtr ptr2 in list) {
                    QTTabBarClass tabBar = QTUtility.instanceManager.GetTabBar(ptr2);
                    if(tabBar != null) {
                        foreach(QTabItem item in tabBar.tabControl1.TabPages) {
                            item.Clone(true).ResetOwner(this.tabControl1);
                        }
                        WindowUtils.CloseExplorer(ptr2, 2);
                    }
                }
                QTabItem.CheckSubTexts(this.tabControl1);
                this.SyncButtonBarCurrent(60);
            }
            finally {
                this.tabControl1.SetRedraw(true);
            }
        }

        private int Navigate(IDLWrapper idlw) {
            if(idlw.Available) {
                try {
                    return this.ShellBrowser.BrowseObject(idlw.PIDL, 1);
                }
                catch(COMException) {
                }
            }
            return 1;
        }

        private void NavigateBackToTheFuture() {
            IEnumTravelLogEntry ppenum = null;
            ITravelLogEntry rgElt = null;
            try {
                int num;
                if(((this.TravelLog.EnumEntries(0x20, out ppenum) == 0) && (this.TravelLog.GetCount(0x20, out num) == 0)) && (num > 0)) {
                    while(ppenum.Next(1, out rgElt, 0) == 0) {
                        if(--num == 0) {
                            break;
                        }
                        if(rgElt != null) {
                            Marshal.ReleaseComObject(rgElt);
                            rgElt = null;
                        }
                    }
                    if(rgElt != null) {
                        this.TravelLog.TravelTo(rgElt);
                    }
                }
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception, null);
            }
            finally {
                if(ppenum != null) {
                    Marshal.ReleaseComObject(ppenum);
                }
                if(rgElt != null) {
                    Marshal.ReleaseComObject(rgElt);
                }
            }
        }

        private void NavigateBranches(QTabItem tab, int index) {
            LogData log = tab.Branches[index];
            Keys modifierKeys = Control.ModifierKeys;
            if(modifierKeys == Keys.Control) {
                using(IDLWrapper wrapper = new IDLWrapper(log.IDL)) {
                    if(!wrapper.Available) {
                        this.ShowMessageNavCanceled(log.Path, false);
                    }
                    else {
                        this.OpenNewWindow(wrapper);
                    }
                }
            }
            else if(modifierKeys == Keys.Shift) {
                this.CloneTabButton(tab, log);
            }
            else {
                this.tabControl1.SelectTab(tab);
                if(IsSpecialFolderNeedsToTravel(log.Path)) {
                    this.SaveSelectedItems(this.CurrentTab);
                    this.NavigatedByCode = true;
                    this.NavigateToPastSpecialDir(log.Hash);
                }
                else {
                    this.NavigatedByCode = false;
                    using(IDLWrapper wrapper2 = new IDLWrapper(log.IDL)) {
                        if(!wrapper2.Available) {
                            this.ShowMessageNavCanceled(log.Path, false);
                        }
                        else {
                            this.SaveSelectedItems(this.CurrentTab);
                            this.Navigate(wrapper2);
                        }
                    }
                }
            }
        }

        private bool NavigateCurrentTab(bool fBack) {
            LogData data;
            string currentPath = this.CurrentTab.CurrentPath;
            if(fBack) {
                data = this.CurrentTab.GoBackward();
            }
            else {
                data = this.CurrentTab.GoForward();
            }
            if((data.Path == null) || (data.Path.Length == 0)) {
                return false;
            }
            if((this.CurrentTab.TabLocked && !data.Path.Contains("*?*?*")) && !currentPath.Contains("*?*?*")) {
                try {
                    this.NowTabCloned = true;
                    QTabItem tab = this.CurrentTab.Clone();
                    this.AddInsertTab(tab);
                    if(fBack) {
                        this.CurrentTab.GoForward();
                    }
                    else {
                        this.CurrentTab.GoBackward();
                    }
                    this.tabControl1.SelectTab(tab);
                }
                catch(Exception exception) {
                    QTUtility2.MakeErrorLog(exception, null);
                }
                return true;
            }
            string path = data.Path;
            if(IsSpecialFolderNeedsToTravel(path) && this.LogEntryDic.ContainsKey(data.Hash)) {
                this.SaveSelectedItems(this.CurrentTab);
                this.NavigatedByCode = true;
                return this.NavigateToPastSpecialDir(data.Hash);
            }
            using(IDLWrapper wrapper = new IDLWrapper(data.IDL)) {
                if(!wrapper.Available) {
                    this.CancelFailedNavigation(path, fBack, 1);
                    return false;
                }
                this.SaveSelectedItems(this.CurrentTab);
                this.NavigatedByCode = true;
                return (0 == this.Navigate(wrapper));
            }
        }

        private static void NavigateOnOldWindow(string path) {
            QTUtility2.SendCOPYDATASTRUCT(QTUtility.instanceManager.CurrentHandle, new IntPtr(0x10), path, IntPtr.Zero);
        }

        private void NavigateToFirstOrLast(bool fBack) {
            string[] historyBack;
            if(fBack) {
                historyBack = this.CurrentTab.GetHistoryBack();
            }
            else {
                historyBack = this.CurrentTab.GetHistoryForward();
            }
            if(historyBack.Length > (fBack ? 1 : 0)) {
                this.NavigateToHistory(new object[] { historyBack[historyBack.Length - 1], fBack, historyBack.Length - 1 });
            }
        }

        private void NavigateToHistory(object[] tag) {
            string path = tag[0].ToString();
            bool fRollBackForward = (bool)tag[1];
            int num = (int)tag[2];
            LogData data = new LogData();
            int countRollback = fRollBackForward ? num : (num + 1);
            if(fRollBackForward) {
                for(int i = 0; i < num; i++) {
                    data = this.CurrentTab.GoBackward();
                }
            }
            else {
                for(int j = 0; j < (num + 1); j++) {
                    data = this.CurrentTab.GoForward();
                }
            }
            if((data.Path == null) || (data.Path.Length == 0)) {
                this.CancelFailedNavigation("( Unknown Path )", fRollBackForward, countRollback);
            }
            else if(this.CurrentTab.TabLocked) {
                this.NowTabCloned = true;
                QTabItem tab = this.CurrentTab.Clone();
                this.AddInsertTab(tab);
                if(fRollBackForward) {
                    for(int k = 0; k < num; k++) {
                        this.CurrentTab.GoForward();
                    }
                }
                else {
                    for(int m = 0; m < (num + 1); m++) {
                        this.CurrentTab.GoBackward();
                    }
                }
                this.tabControl1.SelectTab(tab);
            }
            else if(IsSpecialFolderNeedsToTravel(path)) {
                this.SaveSelectedItems(this.CurrentTab);
                this.NavigatedByCode = true;
                this.NavigateToPastSpecialDir(data.Hash);
            }
            else {
                using(IDLWrapper wrapper = new IDLWrapper(data.IDL)) {
                    if(!wrapper.Available) {
                        this.CancelFailedNavigation(path, fRollBackForward, countRollback);
                    }
                    else {
                        this.SaveSelectedItems(this.CurrentTab);
                        this.NavigatedByCode = true;
                        this.Navigate(wrapper);
                    }
                }
            }
        }

        private bool NavigateToIndex(bool fBack, int index) {
            string[] historyBack;
            if(index == 0) {
                return false;
            }
            if(fBack) {
                historyBack = this.CurrentTab.GetHistoryBack();
                if((historyBack.Length - 1) < index) {
                    return false;
                }
            }
            else {
                historyBack = this.CurrentTab.GetHistoryForward();
                if(historyBack.Length < index) {
                    return false;
                }
            }
            string str = fBack ? historyBack[index] : historyBack[index - 1];
            if(!fBack) {
                index--;
            }
            this.NavigateToHistory(new object[] { str, fBack, index });
            return true;
        }

        private bool NavigateToPastSpecialDir(int hash) {
            IEnumTravelLogEntry ppenum = null;
            try {
                ITravelLogEntry entry2;
                if(this.TravelLog.EnumEntries(0x31, out ppenum) != 0) {
                    goto Label_007C;
                }
            Label_0013:
                do {
                    entry2 = null;
                    if(ppenum.Next(1, out entry2, 0) != 0) {
                        goto Label_007C;
                    }
                    if(entry2 != this.LogEntryDic[hash]) {
                        goto Label_0057;
                    }
                }
                while(this.TravelLog.TravelTo(entry2) != 0);
                this.NowInTravelLog = true;
                this.CurrentTravelLogIndex++;
                return true;
            Label_0057:
                if(entry2 != null) {
                    Marshal.ReleaseComObject(entry2);
                }
                goto Label_0013;
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception, null);
            }
            finally {
                if(ppenum != null) {
                    Marshal.ReleaseComObject(ppenum);
                }
            }
        Label_007C:
            return false;
        }

        private void NavigationButton_DropDownMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            QMenuItem clickedItem = e.ClickedItem as QMenuItem;
            if(clickedItem != null) {
                MenuItemArguments menuItemArguments = clickedItem.MenuItemArguments;
                switch(Control.ModifierKeys) {
                    case Keys.Shift:
                        this.CloneTabButton(this.CurrentTab, null, true, -1);
                        this.NavigateToHistory(new object[] { menuItemArguments.Path, menuItemArguments.IsBack, menuItemArguments.Index });
                        return;

                    case Keys.Control: {
                            using(IDLWrapper wrapper = new IDLWrapper(menuItemArguments.Path)) {
                                this.OpenNewWindow(wrapper);
                                return;
                            }
                        }
                    default:
                        this.NavigateToHistory(new object[] { menuItemArguments.Path, menuItemArguments.IsBack, menuItemArguments.Index });
                        return;
                }
            }
        }

        private void NavigationButtons_Click(object sender, EventArgs e) {
            this.NavigateCurrentTab(sender == this.buttonBack);
        }

        private void NavigationButtons_DropDownOpening(object sender, EventArgs e) {
            this.buttonNavHistoryMenu.DropDown.SuspendLayout();
            while(this.buttonNavHistoryMenu.DropDownItems.Count > 0) {
                this.buttonNavHistoryMenu.DropDownItems[0].Dispose();
            }
            if((this.CurrentTab.HistoryCount_Back + this.CurrentTab.HistoryCount_Forward) > 1) {
                this.buttonNavHistoryMenu.DropDownItems.AddRange(this.CreateNavBtnMenuItems(true).ToArray());
                this.buttonNavHistoryMenu.DropDownItems.AddRange(this.CreateBranchMenu(true, this.components, new ToolStripItemClickedEventHandler(this.tsmiBranchRoot_DropDownItemClicked)).ToArray());
            }
            else {
                ToolStripMenuItem item = new ToolStripMenuItem("none");
                item.Enabled = false;
                this.buttonNavHistoryMenu.DropDownItems.Add(item);
            }
            this.buttonNavHistoryMenu.DropDown.ResumeLayout();
        }

        private static void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e) {
            if(e.Button == MouseButtons.Left) {
                RestoreAllWindowFromTray();
            }
        }

        private void odCallback_Activate() {
            if(optionsDialog.WindowState == FormWindowState.Minimized) {
                optionsDialog.WindowState = FormWindowState.Normal;
            }
            else {
                optionsDialog.Activate();
            }
        }

        private void odCallback_Close() {
            optionsDialog.OwnerWindowClosing();
        }

        private void odCallback_DialogResult(object o) {
            if(o is string) {
                base.Invoke(new FormMethodInvoker(this.odCallback_PluginOption), new object[] { o });
            }
            else if(o is List<PluginAssembly>) {
                base.Invoke(new FormMethodInvoker(this.odCallback_ManagePlugin), new object[] { o });
            }
            else {
                DialogResult result = (DialogResult)o;
                if(result != DialogResult.Yes) {
                    optionsDialog = null;
                }
                if((result == DialogResult.OK) || (result == DialogResult.Yes)) {
                    base.Invoke(new MethodInvoker(this.odCallback_RefreshOptions));
                }
            }
        }

        private void odCallback_ManagePlugin(object o) {
            lstTempAssemblies_Refresh = (List<PluginAssembly>)o;
            SyncTabBarBroadcastPlugin(base.Handle);
            this.RefreshPlugins(true);
            lstTempAssemblies_Refresh = null;
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar")) {
                if(key != null) {
                    QTUtility2.WriteRegBinary<int>(QTButtonBar.ButtonIndexes, "Buttons_Order", key);
                }
            }
            List<string> list = new List<string>();
            using(RegistryKey key2 = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar\Plugins")) {
                if(key2 != null) {
                    using(RegistryKey key3 = key2.CreateSubKey("Paths")) {
                        foreach(string str in key3.GetValueNames()) {
                            key3.DeleteValue(str);
                        }
                        foreach(PluginAssembly assembly in PluginManager.PluginAssemblies) {
                            if(assembly.PluginInfosExist) {
                                foreach(PluginInformation information in assembly.PluginInformations) {
                                    if(information.Enabled) {
                                        list.Add(information.PluginID);
                                    }
                                }
                                key3.SetValue(assembly.Title + assembly.Version, assembly.Path);
                            }
                        }
                    }
                    QTUtility2.WriteRegBinary<string>(list.ToArray(), "Enabled", key2);
                    key2.SetValue("LanguageFile", QTUtility.Path_PluginLangFile);
                }
            }
            PluginManager.SaveButtonOrder();
        }

        private void odCallback_PluginOption(object o) {
            Plugin plugin;
            if(this.pluginManager.TryGetPlugin((string)o, out plugin) && (plugin.Instance != null)) {
                plugin.Instance.OnOption();
            }
        }

        private void odCallback_RefreshOptions() {
            this.RefreshOptions(this.tabControl1.AutoSubText, TMP_fPaintBG, TMP_strOldLangPath);
        }

        private void OnAwake() {
        }

        protected override void OnExplorerAttached() {
            this.ExplorerHandle = (IntPtr)base.Explorer.HWND;
            if(QTUtility.CheckConfig(Settings.NoWindowResizing)) {
                PInvoke.SetWindowLongPtr(this.ExplorerHandle, -16, PInvoke.Ptr_OP_AND(PInvoke.GetWindowLongPtr(this.ExplorerHandle, -16), 0xfffbffff));
            }
            try {
                object obj2;
                object obj3;
                _IServiceProvider bandObjectSite = (_IServiceProvider)base.BandObjectSite;
                Guid guid = ExplorerGUIDs.IID_IShellBrowser;
                Guid riid = ExplorerGUIDs.IID_IUnknown;
                bandObjectSite.QueryService(ref guid, ref riid, out obj2);
                this.ShellBrowser = (IShellBrowser)obj2;
                Guid guid3 = ExplorerGUIDs.IID_ITravelLogStg;
                Guid guid4 = ExplorerGUIDs.IID_ITravelLogStg;
                bandObjectSite.QueryService(ref guid3, ref guid4, out obj3);
                this.TravelLog = (ITravelLogStg)obj3;
            }
            catch(COMException exception) {
                QTUtility2.MakeErrorLog(exception, null);
            }
            base.Explorer.BeforeNavigate2 += new DWebBrowserEvents2_BeforeNavigate2EventHandler(this.Explorer_BeforeNavigate2);
            base.Explorer.NavigateComplete2 += new DWebBrowserEvents2_NavigateComplete2EventHandler(this.Explorer_NavigateComplete2);
            base.Explorer.DownloadBegin += new SHDocVw.DWebBrowserEvents2_DownloadBeginEventHandler(this.Explorer_DownloadBegin);
        }

        protected override void OnPaintBackground(PaintEventArgs e) {
            if(VisualStyleRenderer.IsSupported) {
                if(this.bgRenderer == null) {
                    this.bgRenderer = new VisualStyleRenderer(VisualStyleElement.Rebar.Band.Normal);
                }
                this.bgRenderer.DrawParentBackground(e.Graphics, e.ClipRectangle, this);
            }
            else {
                if(base.ReBarHandle != IntPtr.Zero) {
                    int colorref = (int)PInvoke.SendMessage(base.ReBarHandle, 0x414, IntPtr.Zero, IntPtr.Zero);
                    using(SolidBrush brush = new SolidBrush(QTUtility2.MakeColor(colorref))) {
                        e.Graphics.FillRectangle(brush, e.ClipRectangle);
                        return;
                    }
                }
                base.OnPaintBackground(e);
            }
        }

        private void OpenDroppedFolder(List<string> listDroppedPaths) {
            Keys modifierKeys = Control.ModifierKeys;
            QTUtility2.InitializeTemporaryPaths();
            bool blockSelecting = modifierKeys == Keys.Shift;
            bool flag2 = modifierKeys == Keys.Control;
            bool flag3 = false;
            this.tabControl1.SetRedraw(false);
            try {
                foreach(string str in listDroppedPaths) {
                    if(!string.IsNullOrEmpty(str)) {
                        try {
                            string directoryName = Path.GetDirectoryName(str);
                            if(((directoryName == null) || (directoryName.Length != 3)) || (Path.GetFileName(str) != "System Volume Information")) {
                                using(IDLWrapper wrapper = new IDLWrapper(str)) {
                                    if(wrapper.Available) {
                                        if(wrapper.IsLink) {
                                            if(wrapper.IsLinkToDeadFolder) {
                                                continue;
                                            }
                                            using(IDLWrapper wrapper2 = new IDLWrapper(ShellMethods.GetLinkTargetIDL(str))) {
                                                if(wrapper2.Available && wrapper2.IsFolder) {
                                                    if(flag2) {
                                                        QTUtility.TMPIDLList.Add(wrapper2.IDL);
                                                    }
                                                    else {
                                                        this.OpenNewTab(wrapper2, blockSelecting, false);
                                                        blockSelecting = true;
                                                    }
                                                    flag3 = true;
                                                }
                                                continue;
                                            }
                                        }
                                        if(wrapper.IsFolder && wrapper.IsReadyIfDrive) {
                                            if(flag2) {
                                                QTUtility.TMPIDLList.Add(wrapper.IDL);
                                            }
                                            else {
                                                this.OpenNewTab(wrapper, blockSelecting, false);
                                                blockSelecting = true;
                                            }
                                            flag3 = true;
                                        }
                                    }
                                }
                            }
                            continue;
                        }
                        catch {
                            continue;
                        }
                    }
                }
            }
            finally {
                this.tabControl1.SetRedraw(true);
            }
            if(flag2) {
                if(QTUtility.TMPIDLList.Count <= 0) {
                    return;
                }
                QTUtility.TMPTargetIDL = QTUtility.TMPIDLList[0];
                using(IDLWrapper wrapper3 = new IDLWrapper(QTUtility.TMPTargetIDL)) {
                    this.ShellBrowser.BrowseObject(wrapper3.PIDL, 2);
                    return;
                }
            }
            if(!flag3 && (listDroppedPaths.Count > 0)) {
                List<string> list = new List<string>();
                foreach(string str3 in listDroppedPaths) {
                    if(File.Exists(str3)) {
                        list.Add(str3);
                    }
                }
                if(list.Count > 0) {
                    this.AppendUserApps(list);
                }
            }
        }

        private void OpenGroup(string groupName, bool fForceNewWindow) {
            if(!fForceNewWindow) {
                string str3;
                this.NowTabsAddingRemoving = true;
                bool flag = false;
                string str4 = null;
                int num = 0;
                QTabItem tabPage = null;
                Keys modifierKeys = Control.ModifierKeys;
                bool flag2 = QTUtility.CheckConfig(Settings.CloseWhenGroup);
                bool flag3 = QTUtility.CheckConfig(Settings.DontOpenSame) == (modifierKeys != Keys.Shift);
                bool flag4 = QTUtility.CheckConfig(Settings.ActivateNewTab) == (modifierKeys != Keys.Control);
                bool flag5 = false;
                if(this.NowOpenedByGroupOpener) {
                    flag3 = true;
                    this.NowOpenedByGroupOpener = false;
                }
                QTUtility.RefreshGroupsDic();
                if(QTUtility.GroupPathsDic.TryGetValue(groupName, out str3)) {
                    string[] strArray = str3.Split(QTUtility.SEPARATOR_CHAR);
                    List<string> list = new List<string>();
                    List<QTabItem> list2 = new List<QTabItem>();
                    foreach(QTabItem item2 in this.tabControl1.TabPages) {
                        list.Add(item2.CurrentPath.ToLower());
                        list2.Add(item2);
                    }
                    if(strArray.Length != 0) {
                        try {
                            this.tabControl1.SetRedraw(false);
                            for(int i = 0; i < strArray.Length; i++) {
                                if(QTUtility2.PathExists(strArray[i]) || strArray[i].Contains("???")) {
                                    if(str4 == null) {
                                        str4 = strArray[i];
                                    }
                                    if((flag2 || !flag3) || !list.Contains(strArray[i].ToLower())) {
                                        num++;
                                        using(IDLWrapper wrapper2 = new IDLWrapper(strArray[i])) {
                                            if(wrapper2.Available) {
                                                if(tabPage == null) {
                                                    tabPage = this.CreateNewTab(wrapper2);
                                                }
                                                else {
                                                    this.CreateNewTab(wrapper2);
                                                }
                                            }
                                        }
                                        flag = true;
                                    }
                                    else if(tabPage == null) {
                                        foreach(QTabItem item3 in this.tabControl1.TabPages) {
                                            if(string.Equals(item3.CurrentPath, strArray[i], StringComparison.OrdinalIgnoreCase)) {
                                                tabPage = item3;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            if(flag2 && (num > 0)) {
                                for(int j = 0; j < list2.Count; j++) {
                                    AddToHistory(list2[j]);
                                    this.tabControl1.TabPages.Remove(list2[j]);
                                    list2[j].OnClose();
                                    list2[j] = null;
                                }
                            }
                            this.NowTabsAddingRemoving = false;
                            if(((str4 != null) && (flag4 || (this.tabControl1.SelectedIndex == -1))) && (tabPage != null)) {
                                if(flag) {
                                    this.NowTabCreated = true;
                                }
                                flag5 = tabPage != this.CurrentTab;
                                this.tabControl1.SelectTab(tabPage);
                            }
                        }
                        finally {
                            this.tabControl1.SetRedraw(true);
                        }
                    }
                }
                this.SyncButtonBarCurrent(flag5 ? 0x3f : 0x1003f);
                this.NowTabsAddingRemoving = false;
            }
            else {
                string str;
                if(QTUtility.GroupPathsDic.TryGetValue(groupName, out str) && (str.Length > 0)) {
                    string path = str.Split(QTUtility.SEPARATOR_CHAR)[0];
                    QTUtility.CreateWindowTMPGroup = groupName;
                    using(IDLWrapper wrapper = new IDLWrapper(path)) {
                        if(wrapper.Available) {
                            this.OpenNewWindow(wrapper);
                            return;
                        }
                    }
                }
                QTUtility.CreateWindowTMPGroup = string.Empty;
            }
        }

        private bool OpenNewTab(string path, bool blockSelecting) {
            using(IDLWrapper wrapper = new IDLWrapper(path)) {
                if(wrapper.Available) {
                    return this.OpenNewTab(wrapper, blockSelecting, false);
                }
            }
            return false;
        }

        private bool OpenNewTab(IDLWrapper idlw, bool blockSelecting, bool fForceNew) {
            if((idlw == null) || !idlw.Available) {
                return false;
            }
            QTabItem tabPage = null;
            if(blockSelecting) {
                this.NowTabsAddingRemoving = true;
            }
            try {
                if(!fForceNew && QTUtility.CheckConfig(Settings.DontOpenSame)) {
                    foreach(QTabItem item2 in this.tabControl1.TabPages) {
                        if(string.Equals(item2.CurrentPath, idlw.Path, StringComparison.CurrentCultureIgnoreCase)) {
                            tabPage = item2;
                            break;
                        }
                    }
                    if(tabPage != null) {
                        if(QTUtility.CheckConfig(Settings.ActivateNewTab)) {
                            this.tabControl1.SelectTab(tabPage);
                        }
                        this.SyncButtonBarCurrent(0x3f);
                        return false;
                    }
                }
                string path = idlw.Path;
                if(!idlw.Special && !path.StartsWith("::")) {
                    string directoryName = Path.GetDirectoryName(path);
                    if(!string.IsNullOrEmpty(directoryName)) {
                        using(IDLWrapper wrapper = new IDLWrapper(directoryName)) {
                            if(wrapper.Special && idlw.Available) {
                                IShellFolder ppv = null;
                                try {
                                    IntPtr ptr;
                                    if(PInvoke.SHBindToParent(idlw.PIDL, ExplorerGUIDs.IID_IShellFolder, out ppv, out ptr) == 0) {
                                        using(IDLWrapper wrapper2 = new IDLWrapper(PInvoke.ILCombine(wrapper.PIDL, ptr))) {
                                            if(wrapper2.Available && wrapper2.HasPath) {
                                                if(!blockSelecting && QTUtility.CheckConfig(Settings.ActivateNewTab)) {
                                                    this.NowTabCreated = true;
                                                    this.tabControl1.SelectTab(this.CreateNewTab(wrapper2));
                                                }
                                                else {
                                                    this.CreateNewTab(wrapper2);
                                                    this.SyncButtonBarCurrent(0x1003f);
                                                }
                                                return true;
                                            }
                                        }
                                    }
                                }
                                catch {
                                }
                                finally {
                                    if(ppv != null) {
                                        Marshal.ReleaseComObject(ppv);
                                    }
                                }
                            }
                        }
                    }
                }
                if(!blockSelecting && QTUtility.CheckConfig(Settings.ActivateNewTab)) {
                    this.NowTabCreated = true;
                    this.tabControl1.SelectTab(this.CreateNewTab(idlw));
                }
                else {
                    this.CreateNewTab(idlw);
                    this.SyncButtonBarCurrent(0x1003f);
                }
            }
            finally {
                if(blockSelecting) {
                    this.NowTabsAddingRemoving = false;
                }
            }
            return true;
        }

        private void OpenNewWindow(IDLWrapper idlw) {
            bool flag = this.IsFolderTreeVisible();
            bool flag2 = false;
            if((idlw != null) && idlw.Available) {
                using(IDLWrapper wrapper = new IDLWrapper(ShellMethods.ShellGetPath(this.ShellBrowser))) {
                    if(wrapper.Available) {
                        IShellFolder ppshf = null;
                        try {
                            PInvoke.SHGetDesktopFolder(out ppshf);
                            flag2 = 0 == ppshf.CompareIDs((IntPtr)0x10000000, wrapper.PIDL, idlw.PIDL);
                        }
                        catch {
                        }
                        finally {
                            if(ppshf != null) {
                                Marshal.ReleaseComObject(ppshf);
                            }
                        }
                    }
                }
                uint wFlags = 2;
                if(flag2) {
                    if(flag) {
                        if(CheckProcessID(this.ExplorerHandle, WindowUtils.GetShellTrayWnd()) || WindowUtils.IsExplorerProcessSeparated()) {
                            PInvoke.SetRedraw(this.ExplorerHandle, false);
                            this.ShowFolderTree(false);
                            wFlags |= 0x20;
                            new WaitTimeoutCallback(QTTabBarClass.WaitTimeout).BeginInvoke(200, new AsyncCallback(this.AsyncComplete_FolderTree), true);
                        }
                        else {
                            QTUtility.fRestoreFolderTree = true;
                        }
                    }
                    else {
                        if(!QTUtility.IsVista) {
                            QTUtility.RestoreFolderTree_Hide = true;
                        }
                        wFlags |= 0x20;
                    }
                }
                else if(flag) {
                    QTUtility.fRestoreFolderTree = true;
                }
                QTUtility.CreateWindowTMPPath = idlw.Path;
                if(this.ShellBrowser.BrowseObject(idlw.PIDL, wFlags) != 0) {
                    MessageBox.Show(string.Format(QTUtility.TextResourcesDic["TabBar_Message"][0], idlw.Path));
                    QTUtility.CreateWindowTMPGroup = QTUtility.CreateWindowTMPPath = string.Empty;
                }
                QTUtility.fRestoreFolderTree = false;
            }
            else if((idlw != null) && idlw.HasPath) {
                MessageForm.Show(this.ExplorerHandle, string.Format(QTUtility.TextResourcesDic["TabBar_Message"][0], idlw.Path), idlw.Path, MessageBoxIcon.Asterisk, 0x1770);
            }
        }

        private void OpenOptionsDialog() {
            if(!TMPfNowOptionDialogOpening) {
                if(optionsDialog == null) {
                    TMPfNowOptionDialogOpening = true;
                    TMP_fPaintBG = QTUtility.CheckConfig(Settings.ToolbarBGColor);
                    TMP_strOldLangPath = QTUtility.Path_LanguageFile;
                    this.fOptionDialogCreated = true;
                    Thread thread = new Thread(new ThreadStart(this.OpenOptionsDialogCore));
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                }
                else {
                    optionsDialog.Invoke(new MethodInvoker(this.odCallback_Activate));
                }
            }
        }

        private void OpenOptionsDialogCore() {
            bool checkForIllegalCrossThreadCalls = Control.CheckForIllegalCrossThreadCalls;
            Control.CheckForIllegalCrossThreadCalls = false;
            using(optionsDialog = new OptionsDialog(this.pluginManager, new FormMethodInvoker(this.odCallback_DialogResult))) {
                TMPfNowOptionDialogOpening = false;
                optionsDialog.ShowDialog();
            }
            optionsDialog = null;
            this.fOptionDialogCreated = false;
            Control.CheckForIllegalCrossThreadCalls = checkForIllegalCrossThreadCalls;
        }

        private void pluginitems_Click(object sender, EventArgs e) {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            string name = item.Name;
            MenuType tag = (MenuType)item.Tag;
            foreach(Plugin plugin in this.pluginManager.Plugins) {
                if(plugin.PluginInformation.PluginID == name) {
                    try {
                        if(tag == MenuType.Tab) {
                            if(this.ContextMenuedTab != null) {
                                plugin.Instance.OnMenuItemClick(tag, item.Text, new PluginServer.TabWrapper(this.ContextMenuedTab, this));
                            }
                        }
                        else {
                            plugin.Instance.OnMenuItemClick(tag, item.Text, null);
                        }
                    }
                    catch(Exception exception) {
                        PluginManager.HandlePluginException(exception, this.ExplorerHandle, plugin.PluginInformation.Name, "On menu item \"" + item.Text + "\"clicked.");
                    }
                    break;
                }
            }
        }

        private void QTTabBarClass_MouseDoubleClick(object sender, MouseEventArgs e) {
            bool flag;
            switch(QTUtility.ConfigValues[4]) {
                case 0:
                    this.ChooseNewDirectory();
                    return;

                case 1:
                    this.UpOneLevel();
                    return;

                case 2:
                    if(this.tabControl1.TabCount > 1) {
                        foreach(QTabItem item in this.tabControl1.TabPages) {
                            if(item != this.CurrentTab) {
                                this.CloseTab(item);
                            }
                        }
                    }
                    return;

                case 3:
                    flag = false;
                    foreach(QTabItem item2 in this.tabControl1.TabPages) {
                        if(!item2.TabLocked) {
                            flag = true;
                            break;
                        }
                    }
                    break;

                case 4:
                    this.contextMenuSys.Show(Control.MousePosition);
                    return;

                case 5:
                    this.RestoreNearest();
                    return;

                case 6:
                    this.OpenOptionsDialog();
                    return;

                case 7:
                    WindowUtils.CloseExplorer(this.ExplorerHandle, 1);
                    return;

                case 8:
                    if(this.CurrentTab != null) {
                        this.CloneTabButton(this.CurrentTab, null, false, -1);
                    }
                    return;

                case 9:
                    if(!QTUtility.GroupPathsDic.ContainsKey(QTUtility.Action_BarDblClick)) {
                        this.OpenNewTab(QTUtility.Action_BarDblClick, false);
                        return;
                    }
                    this.OpenGroup(QTUtility.Action_BarDblClick, false);
                    return;

                case 10:
                    if(QTUtility.Action_BarDblClick.Length <= 0) {
                        using(IDLWrapper wrapper2 = new IDLWrapper(this.CurrentTab.CurrentIDL)) {
                            this.OpenNewWindow(wrapper2);
                            return;
                        }
                    }
                    if(!QTUtility.GroupPathsDic.ContainsKey(QTUtility.Action_BarDblClick)) {
                        using(IDLWrapper wrapper = new IDLWrapper(QTUtility.Action_BarDblClick)) {
                            this.OpenNewWindow(wrapper);
                            return;
                        }
                    }
                    this.OpenGroup(QTUtility.Action_BarDblClick, true);
                    return;

                case 11:
                    if((this.lstActivatedTabs.Count > 1) && this.tabControl1.TabPages.Contains(this.lstActivatedTabs[this.lstActivatedTabs.Count - 2])) {
                        try {
                            this.tabControl1.SelectTab(this.lstActivatedTabs[this.lstActivatedTabs.Count - 2]);
                        }
                        catch(ArgumentException) {
                        }
                    }
                    return;

                default:
                    return;
            }
            foreach(QTabItem item3 in this.tabControl1.TabPages) {
                if(flag) {
                    if(!item3.TabLocked) {
                        item3.TabLocked = true;
                    }
                }
                else if(item3.TabLocked) {
                    item3.TabLocked = false;
                }
            }
        }

        private bool rebarController_MessageCaptured(ref System.Windows.Forms.Message m) {
            if((m.Msg == WM.ERASEBKGND) && (QTUtility.CheckConfig(Settings.ToolbarBGColor) || QTUtility.CheckConfig(Settings.RebarImage))) {
                bool flag = false;
                using(Graphics graphics = Graphics.FromHdc(m.WParam)) {
                    RECT rect;
                    PInvoke.GetWindowRect(this.rebarController.Handle, out rect);
                    Rectangle rectangle = new Rectangle(0, 0, rect.Width, rect.Height);
                    if(QTUtility.CheckConfig(Settings.ToolbarBGColor)) {
                        using(SolidBrush brush = new SolidBrush(QTUtility.RebarBGColor)) {
                            graphics.FillRectangle(brush, rectangle);
                            flag = true;
                        }
                    }
                    if((VisualStyleRenderer.IsSupported && QTUtility.CheckConfig(Settings.RebarImage)) && (QTUtility.Path_RebarImage.Length > 0)) {
                        if(this.bmpRebar == null) {
                            this.CreateRebarImage();
                        }
                        if(this.bmpRebar != null) {
                            switch(((QTUtility.ConfigValues[11] & 0x60) | (QTUtility.ConfigValues[13] & 1))) {
                                case 1: {
                                        if(!flag) {
                                            this.rebarController.DefWndProc(ref m);
                                        }
                                        int num2 = (int)PInvoke.SendMessage(this.rebarController.Handle, 0x40c, IntPtr.Zero, IntPtr.Zero);
                                        RECT rect2 = new RECT();
                                        IntPtr lParam = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(RECT)));
                                        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                                        for(int i = 0; i < num2; i++) {
                                            RECT rect3 = new RECT();
                                            IntPtr ptr2 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(RECT)));
                                            PInvoke.SendMessage(this.rebarController.Handle, 0x422, (IntPtr)i, ptr2);
                                            rect3 = (RECT)Marshal.PtrToStructure(ptr2, typeof(RECT));
                                            Marshal.FreeHGlobal(ptr2);
                                            if(IntPtr.Zero != PInvoke.SendMessage(this.rebarController.Handle, 0x409, (IntPtr)i, lParam)) {
                                                rect2 = (RECT)Marshal.PtrToStructure(lParam, typeof(RECT));
                                                rect2.left -= QTUtility.IsVista ? 4 : rect3.left;
                                                rect2.top -= rect3.top;
                                                rect2.right += rect3.right;
                                                rect2.bottom += rect3.bottom;
                                                graphics.DrawImage(this.bmpRebar, rect2.ToRectangle());
                                            }
                                        }
                                        Marshal.FreeHGlobal(lParam);
                                        break;
                                    }
                                case 0x20: {
                                        if(!flag) {
                                            this.rebarController.DefWndProc(ref m);
                                        }
                                        Rectangle destRect = new Rectangle(Point.Empty, this.bmpRebar.Size);
                                        graphics.DrawImage(this.bmpRebar, destRect, destRect, GraphicsUnit.Pixel);
                                        break;
                                    }
                                case 0x40:
                                    if(this.textureBrushRebar == null) {
                                        this.textureBrushRebar = new TextureBrush(this.bmpRebar);
                                    }
                                    graphics.FillRectangle(this.textureBrushRebar, rectangle);
                                    break;

                                default:
                                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                                    graphics.DrawImage(this.bmpRebar, rectangle);
                                    break;
                            }
                            flag = true;
                        }
                    }
                }
                if(flag) {
                    m.Result = (IntPtr)1;
                    return true;
                }
            }
            return false;
        }

        internal void RefreshOptions(bool fAutoSubText, bool fPaintBG, string oldPath_LangFile) {
            if(QTUtility.Path_LanguageFile != oldPath_LangFile) {
                if(File.Exists(QTUtility.Path_LanguageFile)) {
                    QTUtility.TextResourcesDic = QTUtility.ReadLanguageFile(QTUtility.Path_LanguageFile);
                }
                else {
                    QTUtility.Path_LanguageFile = string.Empty;
                    QTUtility.TextResourcesDic = null;
                }
                QTUtility.ValidateTextResources();
                this.RefreshTexts();
            }
            bool fRebarBGCanceled = fPaintBG && !QTUtility.CheckConfig(Settings.ToolbarBGColor);
            this.RefreshTabBar(fRebarBGCanceled);
            SyncTabBarBroadcast(base.Handle, fRebarBGCanceled);
            if(fRebarBGCanceled) {
                QTUtility.DefaultRebarCOLORREF = -1;
            }
            QTUtility.ClosedTabHistoryList.MaxCapacity = QTUtility.MaxCount_History;
            QTUtility.ExecutedPathsList.MaxCapacity = QTUtility.MaxCount_Executed;
            SyncButtonBarBroadCast(0x100);
            this.SyncButtonBarCurrent(0x3f);
            SyncTaskBarMenu();
            if(fAutoSubText && !this.tabControl1.AutoSubText) {
                foreach(QTabItem item in this.tabControl1.TabPages) {
                    item.Comment = string.Empty;
                    item.RefreshRectangle();
                }
                this.tabControl1.Refresh();
            }
            else if(!fAutoSubText && this.tabControl1.AutoSubText) {
                QTabItem.CheckSubTexts(this.tabControl1);
            }
            if(this.pluginManager != null) {
                this.pluginManager.OnSettingsChanged(0);
            }
            if(DropDownMenuBase.InitializeMenuRenderer() && (this.pluginManager != null)) {
                this.pluginManager.OnMenuRendererChanged();
            }
            ContextMenuStripEx.InitializeMenuRenderer();
        }

        private void RefreshPlugins(bool fStatic) {
            if(this.pluginManager != null) {
                this.pluginManager.ClearFilterEngines();
                if(fStatic) {
                    PluginManager.ClearIEncodingDetector();
                }
                foreach(PluginAssembly assembly in PluginManager.PluginAssemblies) {
                    if(!lstTempAssemblies_Refresh.Contains(assembly)) {
                        this.pluginManager.UninstallPluginAssembly(assembly, fStatic);
                    }
                }
                foreach(PluginAssembly assembly2 in lstTempAssemblies_Refresh) {
                    if(fStatic) {
                        PluginManager.AddAssembly(assembly2);
                    }
                    this.pluginManager.RefreshPluginAssembly(assembly2, fStatic);
                }
            }
        }

        internal void RefreshRebar() {
            try {
                base.SuspendLayout();
                this.tabControl1.SuspendLayout();
                IOleCommandTarget bandObjectSite = base.BandObjectSite as IOleCommandTarget;
                if(bandObjectSite != null) {
                    Guid pguidCmdGroup = ExplorerGUIDs.CGID_DeskBand;
                    bandObjectSite.Exec(ref pguidCmdGroup, 0, 0, IntPtr.Zero, IntPtr.Zero);
                    if(QTUtility.IsVista) {
                        IntPtr windowLongPtr = PInvoke.GetWindowLongPtr(base.ReBarHandle, -8);
                        NMHDR structure = new NMHDR();
                        structure.hwndFrom = base.ReBarHandle;
                        structure.idFrom = (IntPtr)0xa005;
                        structure.code = -831;
                        IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(structure));
                        Marshal.StructureToPtr(structure, ptr, false);
                        PInvoke.SendMessage(windowLongPtr, 0x4e, (IntPtr)0xa005, ptr);
                        Marshal.FreeHGlobal(ptr);
                    }
                    else {
                        RECT rect;
                        PInvoke.GetWindowRect(this.ExplorerHandle, out rect);
                        int num = (rect.Height << 0x10) | rect.Width;
                        PInvoke.SendMessage(this.ExplorerHandle, 5, IntPtr.Zero, (IntPtr)num);
                    }
                }
            }
            catch(COMException exception) {
                QTUtility2.MakeErrorLog(exception, null);
            }
            finally {
                this.tabControl1.ResumeLayout();
                base.ResumeLayout();
            }
        }

        private void RefreshTabBar(bool fRebarBGCanceled) {
            base.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabControl1.RefreshOptions(false);
            if(QTUtility.CheckConfig(Settings.ShowNavButtons)) {
                if(this.toolStrip == null) {
                    this.InitializeNavBtns(true);
                    this.buttonNavHistoryMenu.Enabled = this.navBtnsFlag != 0;
                    base.Controls.Add(this.toolStrip);
                }
                else {
                    this.toolStrip.SuspendLayout();
                }
                this.toolStrip.Dock = QTUtility.CheckConfig(Settings.NavButtonsOnRight) ? DockStyle.Right : DockStyle.Left;
                this.toolStrip.ResumeLayout(false);
                this.toolStrip.PerformLayout();
            }
            else if(this.toolStrip != null) {
                this.toolStrip.Dock = DockStyle.None;
            }
            IntPtr windowLongPtr = PInvoke.GetWindowLongPtr(this.ExplorerHandle, -16);
            if(QTUtility.CheckConfig(Settings.NoWindowResizing)) {
                PInvoke.SetWindowLongPtr(this.ExplorerHandle, -16, PInvoke.Ptr_OP_AND(windowLongPtr, 0xfffbffff));
            }
            else {
                PInvoke.SetWindowLongPtr(this.ExplorerHandle, -16, PInvoke.Ptr_OP_OR(windowLongPtr, 0x40000));
            }
            int iType = 0;
            if(QTUtility.CheckConfig(Settings.MultipleRow1)) {
                iType = 1;
            }
            else if(QTUtility.CheckConfig(Settings.MultipleRow2)) {
                iType = 2;
            }
            this.SetBarRows(this.tabControl1.SetTabRowType(iType));
            if(base.ReBarHandle != IntPtr.Zero) {
                if(fRebarBGCanceled && (QTUtility.DefaultRebarCOLORREF != -1)) {
                    PInvoke.SendMessage(base.ReBarHandle, 0x413, IntPtr.Zero, (IntPtr)QTUtility.DefaultRebarCOLORREF);
                }
                else if(QTUtility.CheckConfig(Settings.ToolbarBGColor)) {
                    if(QTUtility.DefaultRebarCOLORREF == -1) {
                        QTUtility.DefaultRebarCOLORREF = (int)PInvoke.SendMessage(base.ReBarHandle, 0x414, IntPtr.Zero, IntPtr.Zero);
                    }
                    int num2 = QTUtility2.MakeCOLORREF(QTUtility.RebarBGColor);
                    PInvoke.SendMessage(base.ReBarHandle, 0x413, IntPtr.Zero, (IntPtr)num2);
                }
                IntPtr hWnd = PInvoke.GetWindowLongPtr(base.ReBarHandle, -8);
                if(hWnd != IntPtr.Zero) {
                    PInvoke.RedrawWindow(hWnd, IntPtr.Zero, IntPtr.Zero, 0x289);
                }
            }
            WindowUtils.ShowMenuBar(!QTUtility.CheckConfig(Settings.HideMenuBar), base.ReBarHandle);
            if(QTUtility.CheckConfig(Settings.AlternateRowColors)) {
                System.Drawing.Color color = QTUtility2.MakeColor(QTUtility.ShellViewRowCOLORREF_Background);
                if(QTUtility.sbAlternate == null) {
                    QTUtility.sbAlternate = new SolidBrush(color);
                }
                else {
                    QTUtility.sbAlternate.Color = color;
                }
            }
            foreach(QTabItem item in this.tabControl1.TabPages) {
                item.RefreshRectangle();
            }
            if(QTUtility.CheckConfig(Settings.RebarImage)) {
                this.CreateRebarImage();
            }
            
            // For some very strange reason, executing this line disables
            // right-click in the list view.  It's not necessary I suppose, 
            // but it still bothers me that I don't know why it happens.
            // TODO: Investigate

            //listViewWrapper.Initialize();

            this.tabControl1.ResumeLayout();
            base.ResumeLayout(true);
        }

        private void RefreshTexts() {
            IntPtr ptr;
            if(QTUtility.instanceManager.TryGetButtonBarHandle(this.ExplorerHandle, out ptr)) {
                QTUtility2.SendCOPYDATASTRUCT(ptr, (IntPtr)10, "refreshText", IntPtr.Zero);
            }
            OptionsDialog.RefreshTexts();
        }

        [ComRegisterFunction]
        private static void Register(System.Type t) {
            string name = t.GUID.ToString("B");
            try {
                using(RegistryKey key = Registry.ClassesRoot.OpenSubKey("CLSID", true)) {
                    key.DeleteSubKeyTree("{D2BF470E-ED1C-487F-A444-2BD8835EB6CE}");
                    Registry.ClassesRoot.DeleteSubKeyTree("QTTabBarLib.QTCoBar");
                }
            }
            catch {
            }
            using(RegistryKey key2 = Registry.ClassesRoot.CreateSubKey(@"CLSID\" + name)) {
                key2.SetValue(null, "QT TabBar");
                key2.SetValue("MenuText", "QT TabBar");
                key2.SetValue("HelpText", "QT TabBar");
            }
            using(RegistryKey key3 = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Internet Explorer\Toolbar")) {
                key3.SetValue(name, "QTTabBar");
            }
        }

        private void ReorderTab(int index, bool fDescending) {
            this.tabControl1.SetRedraw(false);
            try {
                if(index == 3) {
                    if(this.tabControl1.TabCount > 1) {
                        int indexSource = 0;
                        for(int i = this.tabControl1.TabCount - 1; indexSource < i; i--) {
                            this.tabControl1.TabPages.Swap(indexSource, i);
                            this.tabControl1.TabPages.Swap(i - 1, indexSource);
                            indexSource++;
                        }
                    }
                }
                else {
                    int num3 = fDescending ? -1 : 1;
                    for(int j = 0; j < (this.tabControl1.TabCount - 1); j++) {
                        for(int k = this.tabControl1.TabCount - 1; k > j; k--) {
                            string strA = string.Empty;
                            string strB = string.Empty;
                            if(index == 0) {
                                strA = this.tabControl1.TabPages[j].Text;
                                strB = this.tabControl1.TabPages[k].Text;
                            }
                            else if(index == 1) {
                                strA = ((QTabItem)this.tabControl1.TabPages[j]).CurrentPath;
                                strB = ((QTabItem)this.tabControl1.TabPages[k]).CurrentPath;
                            }
                            else {
                                int num6 = this.lstActivatedTabs.IndexOf((QTabItem)this.tabControl1.TabPages[j]);
                                int num7 = this.lstActivatedTabs.IndexOf((QTabItem)this.tabControl1.TabPages[k]);
                                if(((num6 - num7) * num3) < 0) {
                                    this.tabControl1.TabPages.Swap(j, k);
                                }
                                continue;
                            }
                            if((string.Compare(strA, strB) * num3) > 0) {
                                this.tabControl1.TabPages.Swap(j, k);
                            }
                        }
                    }
                }
            }
            finally {
                this.tabControl1.SetRedraw(true);
            }
            this.SyncButtonBarCurrent(12);
        }

        private void ReplaceByGroup(string groupName) {
            byte num = QTUtility.ConfigValues[0];
            if(QTUtility.CheckConfig(Settings.CloseWhenGroup)) {
                QTUtility.ConfigValues[0] = (byte)(QTUtility.ConfigValues[0] & 0xdf);
            }
            else {
                QTUtility.ConfigValues[0] = (byte)(QTUtility.ConfigValues[0] | 0x20);
            }
            this.OpenGroup(groupName, false);
            QTUtility.ConfigValues[0] = num;
        }

        private static void RestoreAllWindowFromTray() {
            Dictionary<IntPtr, int> dictionary = new Dictionary<IntPtr, int>(dicNotifyIcon);
            foreach(IntPtr ptr in dictionary.Keys) {
                ShowTaksbarItem(ptr, true);
            }
        }

        private void RestoreFromTray() {
            if((dicNotifyIcon != null) && dicNotifyIcon.ContainsKey(this.ExplorerHandle)) {
                ShowTaksbarItem(this.ExplorerHandle, true);
            }
        }

        private void RestoreNearest() {
            if(QTUtility.ClosedTabHistoryList.Count <= 0) {
                return;
            }
            Stack<string> stack = new Stack<string>(QTUtility.ClosedTabHistoryList);
        Label_001B:
            while(stack.Count <= 0) {
            }
            string b = stack.Pop();
            bool flag = false;
            foreach(QTabItem item in this.tabControl1.TabPages) {
                if(string.Equals(item.CurrentPath, b, StringComparison.OrdinalIgnoreCase)) {
                    flag = true;
                    break;
                }
            }
            if(flag) {
                if(stack.Count == 0) {
                    if(!string.Equals(b, this.CurrentAddress, StringComparison.OrdinalIgnoreCase)) {
                        this.OpenNewTab(b, false);
                    }
                    return;
                }
                goto Label_001B;
            }
            this.OpenNewTab(b, false);
        }

        private void RestoreTabsOnInitialize(int iIndex, string openingPath) {
            QTUtility.RefreshLockedTabsList();
            byte num = QTUtility.ConfigValues[1];
            QTUtility.ConfigValues[1] = 0;
            try {
                if(iIndex == 1) {
                    foreach(string str in QTUtility.LockedTabsToRestoreList) {
                        bool flag = false;
                        foreach(QTabItem item2 in this.tabControl1.TabPages) {
                            if(item2.CurrentPath == str) {
                                if(item2 == this.CurrentTab) {
                                    this.fNowRestoring = true;
                                }
                                else {
                                    item2.TabLocked = true;
                                    flag = true;
                                }
                                break;
                            }
                        }
                        if(!flag) {
                            if(str != openingPath) {
                                using(IDLWrapper wrapper = new IDLWrapper(str)) {
                                    if(wrapper.Available) {
                                        this.CreateNewTab(wrapper).TabLocked = true;
                                    }
                                    continue;
                                }
                            }
                            this.tabControl1.TabPages.Swap(0, this.tabControl1.TabCount - 1);
                            this.fNowRestoring = true;
                        }
                    }
                }
                else if(iIndex == 0) {
                    using(RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Quizo\QTTabBar", false)) {
                        if(key != null) {
                            string[] strArray = ((string)key.GetValue("TabsOnLastClosedWindow", string.Empty)).Split(QTUtility.SEPARATOR_CHAR);
                            if((strArray.Length > 0) && (strArray[0].Length > 0)) {
                                foreach(string str2 in strArray) {
                                    bool flag2 = false;
                                    foreach(QTabItem item3 in this.tabControl1.TabPages) {
                                        if(item3.CurrentPath == str2) {
                                            flag2 = true;
                                            break;
                                        }
                                    }
                                    if(!flag2 && (str2.Length > 0)) {
                                        if(str2 == openingPath) {
                                            this.tabControl1.TabPages.Swap(0, this.tabControl1.TabCount - 1);
                                        }
                                        else {
                                            using(IDLWrapper wrapper2 = new IDLWrapper(str2)) {
                                                if(wrapper2.Available) {
                                                    QTabItem item4 = this.CreateNewTab(wrapper2);
                                                    if(QTUtility.LockedTabsToRestoreList.Contains(str2)) {
                                                        item4.TabLocked = true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                this.fNowRestoring = true;
                            }
                        }
                    }
                }
            }
            finally {
                QTUtility.ConfigValues[1] = num;
            }
        }

        private void SaveSelectedItems(QTabItem tab) {
            Address[] addressArray;
            string str;
            if(((tab != null) && !string.IsNullOrEmpty(this.CurrentAddress)) && this.TryGetSelection(out addressArray, out str, false)) {
                tab.SetSelectedItemsAt(this.CurrentAddress, addressArray, str);
            }
        }

        private void SetBarRows(int count) {
            this.BandHeight = (count * (QTUtility.TabHeight - 3)) + 5;
            this.RefreshRebar();
        }

        internal static void SetStringClipboard(string str) {
            try {
                Clipboard.SetDataObject(str, true);
                if(!QTUtility.CheckConfig(Settings.DisableSound)) {
                    SystemSounds.Asterisk.Play();
                }
            }
            catch {
                SystemSounds.Hand.Play();
            }
        }

        protected override bool ShouldHaveBreak() {
            bool breakBar = true;
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar")) {
                if(key != null) {
                    breakBar = ((int)key.GetValue("BreakTabBar", 1) == 1);
                }
            }
            return breakBar;
        }

        private void ShowAndClickSubDirTip() {
            try {
                Address[] addressArray;
                string str;
                if(this.TryGetSelection(out addressArray, out str, false) && ((addressArray.Length == 1) && !string.IsNullOrEmpty(addressArray[0].Path))) {
                    string path = addressArray[0].Path;
                    if(!path.StartsWith("::") && !Directory.Exists(path)) {
                        if(!string.Equals(Path.GetExtension(path), ".lnk", StringComparison.OrdinalIgnoreCase)) {
                            return;
                        }
                        path = ShellMethods.GetLinkTargetPath(path);
                        if(string.IsNullOrEmpty(path) || !Directory.Exists(path)) {
                            return;
                        }
                    }
                    if(this.subDirTip == null) {
                        this.subDirTip = new SubDirTipForm(base.Handle, this.ExplorerHandle, true, listViewWrapper);
                        this.subDirTip.MenuItemClicked += new ToolStripItemClickedEventHandler(this.subDirTip_MenuItemClicked);
                        this.subDirTip.MultipleMenuItemsClicked += new EventHandler(this.subDirTip_MultipleMenuItemsClicked);
                        this.subDirTip.MenuItemRightClicked += new ItemRightClickedEventHandler(this.subDirTip_MenuItemRightClicked);
                        this.subDirTip.MultipleMenuItemsRightClicked += new ItemRightClickedEventHandler(this.subDirTip_MultipleMenuItemsRightClicked);
                    }

                    int iItem = listViewWrapper.GetFocusedItem();
                    if(iItem != -1) {
                        this.ShowSubDirTip(iItem, false);
                        this.subDirTip.PerformClickByKey();
                    }
                }
            }
            catch {
            }
        }

        public override void ShowDW(bool fShow) {
            base.ShowDW(fShow);
            if((fShow && !this.FirstNavigationCompleted) && ((base.Explorer != null) && (base.Explorer.ReadyState == tagREADYSTATE.READYSTATE_COMPLETE))) {
                this.InitializeInstallation();
            }
            if(!fShow) {
                using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar")) {
                    key.SetValue("BreakTabBar", base.BandHasBreak() ? 1 : 0);
                }
            }
        }

        private void ShowFolderTree(bool fShow) {
            if(!QTUtility.IsVista && (fShow != this.IsFolderTreeVisible())) {
                object pvaClsid = "{EFA24E64-B078-11d0-89E4-00C04FC9E26E}";
                object pvarShow = fShow;
                object pvarSize = null;
                base.Explorer.ShowBrowserBar(ref pvaClsid, ref pvarShow, ref pvarSize);
            }
        }

        internal static void ShowMD5(string[] paths) {
            if(md5Form == null) {
                md5Form = new FileHashComputerForm();
            }
            List<string> list = new List<string>();
            if(paths != null) {
                for(int i = 0; i < paths.Length; i++) {
                    if(File.Exists(paths[i])) {
                        list.Add(paths[i]);
                    }
                }
            }
            string[] strArray = null;
            if(list.Count > 0) {
                strArray = list.ToArray();
            }
            if(md5Form.InvokeRequired) {
                md5Form.Invoke(new FormMethodInvoker(QTTabBarClass.ShowMD5FormCore), new object[] { strArray });
            }
            else {
                ShowMD5FormCore(strArray);
            }
        }

        private static void ShowMD5FormCore(object paths) {
            md5Form.ShowFileHashForm((string[])paths);
        }

        private void ShowMessageNavCanceled(string failedPath, bool fModal) {
            MessageForm.Show(this.ExplorerHandle, string.Format(QTUtility.TextResourcesDic["TabBar_Message"][0], failedPath), string.Empty, MessageBoxIcon.Asterisk, 0x2710, fModal);
        }

        private void ShowSearchBar(bool fShow) {
            if(QTUtility.IsVista) {
                if(!fShow) {
                    return;
                }
                using(IDLWrapper wrapper = new IDLWrapper(QTUtility.PATH_SEARCHFOLDER)) {
                    if(wrapper.Available) {
                        this.ShellBrowser.BrowseObject(wrapper.PIDL, 2);
                    }
                    return;
                }
            }
            object pvaClsid = "{C4EE31F3-4768-11D2-BE5C-00A0C9A83DA1}";
            object pvarShow = fShow;
            object pvarSize = null;
            base.Explorer.ShowBrowserBar(ref pvaClsid, ref pvarShow, ref pvarSize);
        }

        public ListViewWrapper GetListViewWrapper() {
            return listViewWrapper;
        }

        public IntPtr GetShellViewHWND() {
            IntPtr hwndShellView = IntPtr.Zero;
            IShellView ppshv = null;
            try {
                if(this.ShellBrowser.QueryActiveShellView(out ppshv) == 0) {
                    ppshv.GetWindow(out hwndShellView);
                }
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception, null);
            }
            finally {
                if(ppshv != null) {
                    Marshal.ReleaseComObject(ppshv);
                }
            }
            return hwndShellView;
        }

        private bool ShowSubDirTip(int iItem, bool fSkipForegroundCheck) {
            string str;
            if((fSkipForegroundCheck || (this.ExplorerHandle == PInvoke.GetForegroundWindow())) && this.TryGetHotTrackPath(iItem, out str)) {
                bool flag = false;
                try {
                    if(!TryMakeSubDirTipPath(ref str)) {
                        return false;
                    }
                    Point pnt = listViewWrapper.GetSubDirTipPoint(iItem);

                    if(this.subDirTip == null) {
                        this.subDirTip = new SubDirTipForm(base.Handle, this.ExplorerHandle, true, listViewWrapper);
                        this.subDirTip.MenuItemClicked += new ToolStripItemClickedEventHandler(this.subDirTip_MenuItemClicked);
                        this.subDirTip.MultipleMenuItemsClicked += new EventHandler(this.subDirTip_MultipleMenuItemsClicked);
                        this.subDirTip.MenuItemRightClicked += new ItemRightClickedEventHandler(this.subDirTip_MenuItemRightClicked);
                        this.subDirTip.MultipleMenuItemsRightClicked += new ItemRightClickedEventHandler(this.subDirTip_MultipleMenuItemsRightClicked);
                    }
                    this.subDirTip.ShowSubDirTip(str, null, pnt);
                    flag = true;
                }
                catch {
                }
                return flag;
            }
            return false;
        }

        private void ShowSubdirTip_Tab(QTabItem tab, bool fShow, int offsetX, bool fKey, bool fParent) {
            try {
                if(fShow) {
                    if(base.Explorer.Busy || string.IsNullOrEmpty(tab.CurrentPath)) {
                        this.tabControl1.SetSubDirTipShown(false);
                    }
                    else {
                        string currentPath = tab.CurrentPath;
                        if(fParent || TryMakeSubDirTipPath(ref currentPath)) {
                            if(this.subDirTip_Tab == null) {
                                this.subDirTip_Tab = new SubDirTipForm(base.Handle, this.ExplorerHandle, true, listViewWrapper);
                                this.subDirTip_Tab.MenuItemClicked += new ToolStripItemClickedEventHandler(this.subDirTip_MenuItemClicked);
                                this.subDirTip_Tab.MultipleMenuItemsClicked += new EventHandler(this.subDirTip_MultipleMenuItemsClicked);
                                this.subDirTip_Tab.MenuItemRightClicked += new ItemRightClickedEventHandler(this.subDirTip_MenuItemRightClicked);
                                this.subDirTip_Tab.MenuClosed += new EventHandler(this.subDirTip_Tab_MenuClosed);
                                this.subDirTip_Tab.MultipleMenuItemsRightClicked += new ItemRightClickedEventHandler(this.subDirTip_MultipleMenuItemsRightClicked);
                            }
                            this.ContextMenuedTab = tab;
                            Point pnt = this.tabControl1.PointToScreen(new Point(tab.TabBounds.X + offsetX, fParent ? tab.TabBounds.Top : (tab.TabBounds.Bottom - 3)));
                            if(tab != this.CurrentTab) {
                                pnt.X += 2;
                            }
                            this.tabControl1.SetSubDirTipShown(this.subDirTip_Tab.ShowMenuWithoutShowForm(currentPath, pnt, fParent));
                        }
                        else {
                            this.tabControl1.SetSubDirTipShown(false);
                            this.HideSubDirTip_Tab_Menu();
                        }
                    }
                }
                else {
                    this.HideSubDirTip_Tab_Menu();
                }
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception, "tabsubdir");
            }
        }

        private bool ShowTabSwitcher(bool fShift, bool fRepeat) {
            this.HideSubDirTip(-1);
            if(this.tabControl1.TabCount < 2) {
                return false;
            }
            if(this.tabSwitcher == null) {
                this.tabSwitcher = new TabSwitchForm();
                this.tabSwitcher.Switched += new ItemCheckEventHandler(this.tabSwitcher_Switched);
            }
            if(!this.tabSwitcher.IsShown) {
                List<QTTabBarLib.PathData> lstPaths = new List<QTTabBarLib.PathData>();
                string str = !QTUtility.CheckConfig(Settings.NoRenameAmbTabs) ? " @ " : " : ";
                foreach(QTabItem item in this.tabControl1.TabPages) {
                    string strDisplay = string.IsNullOrEmpty(item.Comment) ? item.Text : (item.Text + str + item.Comment);
                    lstPaths.Add(new QTTabBarLib.PathData(strDisplay, item.CurrentPath, item.ImageKey));
                }
                this.tabSwitcher.ShowSwitcher(this.ExplorerHandle, this.tabControl1.SelectedIndex, lstPaths);
            }
            int index = this.tabSwitcher.Switch(fShift);
            if(!fRepeat || (this.tabControl1.TabCount < 13)) {
                this.tabControl1.SetPseudoHotIndex(index);
            }
            return true;
        }

        private static void ShowTaksbarItem(IntPtr hwndExplr, bool fShow) {
            lock(syncObj_NotifyIcon) {
                if(dicNotifyIcon == null) {
                    dicNotifyIcon = new Dictionary<IntPtr, int>();
                }
                if(notifyIcon == null) {
                    if(icoNotify == null) {
                        icoNotify = QTUtility.GetIcon(string.Empty, false);
                    }
                    contextMenuNotifyIcon = new ContextMenuStripEx(null, false);
                    contextMenuNotifyIcon.ImageList = QTUtility.ImageListGlobal;
                    contextMenuNotifyIcon.ItemClicked += new ToolStripItemClickedEventHandler(QTTabBarClass.contextMenuNotifyIcon_ItemClicked);
                    IntPtr handle = contextMenuNotifyIcon.Handle;
                    notifyIcon = new NotifyIcon();
                    notifyIcon.Icon = icoNotify;
                    notifyIcon.MouseDoubleClick += new MouseEventHandler(QTTabBarClass.notifyIcon_MouseDoubleClick);
                    notifyIcon.ContextMenuStrip = contextMenuNotifyIcon;
                    if(!fShow) {
                        notifyIcon.Visible = true;
                    }
                    hwndNotifyIconParent = hwndExplr;
                }
            }
            ITaskbarList o = null;
            try {
                object obj2;
                Guid rclsid = ExplorerGUIDs.CLSID_TaskbarList;
                Guid riid = ExplorerGUIDs.IID_ITaskbarList;
                PInvoke.CoCreateInstance(ref rclsid, IntPtr.Zero, 1, ref riid, out obj2);
                o = (ITaskbarList)obj2;
                o.HrInit();
                if(fShow) {
                    int num;
                    if(!dicNotifyIcon.TryGetValue(hwndExplr, out num)) {
                        num = 1;
                    }
                    o.AddTab(hwndExplr);
                    PInvoke.ShowWindow(hwndExplr, num);
                    PInvoke.SetForegroundWindow(hwndExplr);
                    dicNotifyIcon.Remove(hwndExplr);
                    if(dicNotifyIcon.Count == 0) {
                        notifyIcon.Visible = false;
                    }
                    CreateContextMenuItems_NotifyIcon(IntPtr.Zero, 0);
                }
                else {
                    bool flag = PInvoke.IsZoomed(hwndExplr);
                    PInvoke.ShowWindow(hwndExplr, 0);
                    o.DeleteTab(hwndExplr);
                    notifyIcon.Visible = true;
                    CreateContextMenuItems_NotifyIcon(hwndExplr, flag ? 3 : 1);
                }
                if(notifyIcon.Visible) {
                    int count = dicNotifyIcon.Count;
                    string str = count + " window" + ((count > 1) ? "s" : string.Empty);
                    if(str.Length > 0x40) {
                        str = str.Substring(0, 60) + "...";
                    }
                    notifyIcon.Text = str;
                }
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception, null);
            }
            finally {
                if(o != null) {
                    Marshal.ReleaseComObject(o);
                }
            }
        }

        private bool ShowThumbnailTooltip(int iItem, Point pnt, bool fKey) {
            string linkTargetPath;
            if(this.TryGetHotTrackPath(iItem, out linkTargetPath)) {
                if((linkTargetPath.StartsWith("::") || linkTargetPath.StartsWith(@"\\")) || linkTargetPath.ToLower().StartsWith(@"a:\")) {
                    return false;
                }
                string ext = Path.GetExtension(linkTargetPath).ToLower();
                if(ext == ".lnk") {
                    linkTargetPath = ShellMethods.GetLinkTargetPath(linkTargetPath);
                    if(linkTargetPath.Length == 0) {
                        return false;
                    }
                    ext = Path.GetExtension(linkTargetPath).ToLower();
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
                    return this.thumbnailTooltip.ShowToolTip(linkTargetPath, pnt);
                }
                this.HideThumbnailTooltip(6);
            }
            return false;
        }

        private void ShowToolTipForDD(QTabItem tab, int iState, int grfKeyState) {
            if(((this.tabForDD == null) || (this.tabForDD != tab)) || (this.iModKeyStateDD != grfKeyState)) {
                this.tabForDD = tab;
                this.iModKeyStateDD = grfKeyState;
                if(this.timerOnTab == null) {
                    this.timerOnTab = new System.Windows.Forms.Timer(this.components);
                    this.timerOnTab.Tick += new EventHandler(this.timerOnTab_Tick);
                }
                this.timerOnTab.Enabled = false;
                this.timerOnTab.Interval = QTUtility.CheckConfig(Settings.DragDropOntoTabs) ? 0x4b0 : 700;
                this.timerOnTab.Enabled = true;
                if(QTUtility.CheckConfig(Settings.DragDropOntoTabs) && (iState != -1)) {
                    Rectangle tabRect = this.tabControl1.GetTabRect(tab);
                    Point lpPoints = new Point(tabRect.X + ((tabRect.Width * 3) / 4), tabRect.Bottom + 0x10);
                    string[] strArray = QTUtility.TextResourcesDic["DragDropToolTip"];
                    string str = string.Empty;
                    switch((grfKeyState & 12)) {
                        case 4:
                            str = strArray[1];
                            break;

                        case 8:
                            str = strArray[0];
                            break;

                        case 12:
                            str = strArray[2];
                            break;

                        default:
                            if(iState == 1) {
                                str = strArray[0];
                            }
                            else {
                                str = strArray[1];
                            }
                            break;
                    }
                    if(this.toolTipForDD == null) {
                        this.toolTipForDD = new System.Windows.Forms.ToolTip(this.components);
                        this.toolTipForDD.UseAnimation = this.toolTipForDD.UseFading = false;
                    }
                    this.toolTipForDD.ToolTipTitle = str;
                    if(PInvoke.GetForegroundWindow() != this.ExplorerHandle) {
                        System.Type type = typeof(System.Windows.Forms.ToolTip);
                        BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Instance;
                        MethodInfo method = type.GetMethod("SetTrackPosition", bindingAttr);
                        MethodInfo info2 = type.GetMethod("SetTool", bindingAttr);
                        PInvoke.MapWindowPoints(this.tabControl1.Handle, IntPtr.Zero, ref lpPoints, 1);
                        method.Invoke(this.toolTipForDD, new object[] { lpPoints.X, lpPoints.Y });
                        info2.Invoke(this.toolTipForDD, new object[] { this.tabControl1, tab.CurrentPath, 2, lpPoints });
                    }
                    else {
                        this.toolTipForDD.Active = true;
                        this.toolTipForDD.Show(tab.CurrentPath, this.tabControl1, lpPoints);
                    }
                }
            }
        }

        private void subDirTip_MenuItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            QMenuItem clickedItem = (QMenuItem)e.ClickedItem;
            if(clickedItem.Target == MenuTarget.Folder) {
                if(clickedItem.IDLData != null) {
                    using(IDLWrapper wrapper = new IDLWrapper(clickedItem.IDLData)) {
                        this.Navigate(wrapper);
                    }
                    return;
                }
                string targetPath = clickedItem.TargetPath;
                Keys modifierKeys = Control.ModifierKeys;
                bool flag = (this.subDirTip_Tab != null) && (sender == this.subDirTip_Tab);
                if((modifierKeys & Keys.Control) == Keys.Control) {
                    using(IDLWrapper wrapper2 = new IDLWrapper(targetPath)) {
                        this.OpenNewWindow(wrapper2);
                        return;
                    }
                }
                if((modifierKeys & Keys.Shift) == Keys.Shift) {
                    using(IDLWrapper wrapper3 = new IDLWrapper(targetPath)) {
                        this.OpenNewTab(wrapper3, false, true);
                        return;
                    }
                }
                if((!flag || (this.ContextMenuedTab == this.CurrentTab)) && this.CurrentTab.TabLocked) {
                    this.CloneTabButton(this.CurrentTab, targetPath, true, this.tabControl1.SelectedIndex + 1);
                    return;
                }
                if(flag && (this.ContextMenuedTab != this.CurrentTab)) {
                    if(this.ContextMenuedTab != null) {
                        if(this.ContextMenuedTab.TabLocked) {
                            this.CloneTabButton(this.ContextMenuedTab, targetPath, true, this.tabControl1.TabPages.IndexOf(this.ContextMenuedTab) + 1);
                            return;
                        }
                        this.NowTabCloned = targetPath == this.CurrentAddress;
                        this.ContextMenuedTab.NavigatedTo(targetPath, null, -1);
                        this.tabControl1.SelectTab(this.ContextMenuedTab);
                    }
                    return;
                }
                using(IDLWrapper wrapper4 = new IDLWrapper(targetPath)) {
                    this.Navigate(wrapper4);
                    return;
                }
            }
            try {
                string path = clickedItem.Path;
                ProcessStartInfo startInfo = new ProcessStartInfo(path);
                startInfo.WorkingDirectory = Path.GetDirectoryName(path);
                startInfo.ErrorDialog = true;
                startInfo.ErrorDialogParentHandle = this.ExplorerHandle;
                Process.Start(startInfo);
                if(!QTUtility.CheckConfig(Settings.NoRecentFiles)) {
                    QTUtility.ExecutedPathsList.Add(path);
                }
            }
            catch {
            }
        }

        private void subDirTip_MenuItemRightClicked(object sender, ItemRightClickedEventArgs e) {
            QMenuItem clickedItem = e.ClickedItem as QMenuItem;
            if(clickedItem != null) {
                using(IDLWrapper wrapper = new IDLWrapper(clickedItem.Path)) {
                    e.HRESULT = ShellMethods.PopUpSystemContextMenu(wrapper, e.IsKey ? e.Point : Control.MousePosition, ref this.iContextMenu2, ((SubDirTipForm)sender).Handle, false);
                }
            }
        }

        private void subDirTip_MultipleMenuItemsClicked(object sender, EventArgs e) {
            List<string> executedDirectories = ((SubDirTipForm)sender).ExecutedDirectories;
            if((Control.ModifierKeys & Keys.Control) == Keys.Control) {
                QTUtility2.InitializeTemporaryPaths();
                QTUtility.TMPPathList.AddRange(executedDirectories);
                using(IDLWrapper wrapper = new IDLWrapper(executedDirectories[0])) {
                    this.OpenNewWindow(wrapper);
                    return;
                }
            }
            bool flag = true;
            foreach(string str in executedDirectories) {
                this.OpenNewTab(str, !flag);
                flag = false;
            }
        }

        private void subDirTip_MultipleMenuItemsRightClicked(object sender, ItemRightClickedEventArgs e) {
            List<string> executedDirectories = ((SubDirTipForm)sender).ExecutedDirectories;
            e.HRESULT = ShellMethods.PopUpSystemContextMenu(executedDirectories, e.IsKey ? e.Point : Control.MousePosition, ref this.iContextMenu2, ((SubDirTipForm)sender).Handle);
        }

        private void subDirTip_Tab_MenuClosed(object sender, EventArgs e) {
            this.tabControl1.SetSubDirTipShown(false);
            this.tabControl1.RefreshFolderImage();
        }

        private static void SyncButtonBarBroadCast(int mask) {
            int num = mask << 0x10;
            if(((mask & 1) == 1) && (QTUtility.GroupPathsDic.Count > 0)) {
                num++;
            }
            if(((mask & 2) == 2) && (QTUtility.ClosedTabHistoryList.Count > 0)) {
                num += 2;
            }
            if(((mask & 4) == 4) && (QTUtility.UserAppsDic.Count > 0)) {
                num += 4;
            }
            if(((mask & 8) == 8) && QTUtility.CheckConfig(Settings.ShowTooltips)) {
                num += 8;
            }
            try {
                foreach(IntPtr ptr in QTUtility.instanceManager.ButtonBarHandles()) {
                    if(PInvoke.IsWindow(ptr)) {
                        QTUtility2.SendCOPYDATASTRUCT(ptr, (IntPtr)1, "fromTabBC", (IntPtr)num);
                    }
                }
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception, null);
            }
        }

        private bool SyncButtonBarCurrent(int mask) {
            IntPtr ptr;
            bool flag = false;
            if(QTUtility.instanceManager.TryGetButtonBarHandle(this.ExplorerHandle, out ptr)) {
                int num = mask << 0x10;
                if(mask != 0x100) {
                    int index = this.tabControl1.TabPages.IndexOf(this.CurrentTab);
                    int tabCount = this.tabControl1.TabCount;
                    if((this.navBtnsFlag & 1) != 0) {
                        num++;
                    }
                    if((this.navBtnsFlag & 2) != 0) {
                        num += 2;
                    }
                    if(index > 0) {
                        num += 4;
                    }
                    if((tabCount - index) > 1) {
                        num += 8;
                    }
                    if(tabCount > 1) {
                        num += 0x10;
                    }
                    if(!QTUtility.CheckConfig(Settings.NeverCloseWindow) || (tabCount > 1)) {
                        num += 0x20;
                    }
                    if(this.NowTopMost) {
                        num += 0x40;
                    }
                    if((((mask & 0x80) != 0) && (this.CurrentTab != null)) && ((this.CurrentTab.CurrentIDL != null) && (this.CurrentTab.CurrentIDL.Length == 2))) {
                        num += 0x80;
                    }
                }
                QTUtility2.SendCOPYDATASTRUCT(ptr, (IntPtr)2, "fromTab", (IntPtr)num);
                flag = true;
            }
            if(((mask & 0x10000) == 0x10000) && this.tabControl1.AutoSubText) {
                QTabItem.CheckSubTexts(this.tabControl1);
            }
            return flag;
        }

        private static void SyncTabBarBroadcast(IntPtr hwndThis, bool fRebarBGCanceled) {
            try {
                foreach(IntPtr ptr in QTUtility.instanceManager.TabBarHandles()) {
                    if((ptr != hwndThis) && PInvoke.IsWindow(ptr)) {
                        QTUtility2.SendCOPYDATASTRUCT(ptr, (IntPtr)0x11, "refresh", fRebarBGCanceled ? ((IntPtr)1) : IntPtr.Zero);
                    }
                }
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception, null);
            }
        }

        private static void SyncTabBarBroadcastPlugin(IntPtr hwndThis) {
            try {
                foreach(IntPtr ptr in QTUtility.instanceManager.TabBarHandles()) {
                    if(ptr != hwndThis) {
                        QTUtility2.SendCOPYDATASTRUCT(ptr, (IntPtr)0x12, "refreshPlugin", IntPtr.Zero);
                    }
                }
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception, null);
            }
        }

        internal static void SyncTaskBarMenu() {
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar")) {
                IntPtr hWnd = QTUtility2.ReadRegHandle("TaskBarHandle", key);
                if((hWnd != IntPtr.Zero) && PInvoke.IsWindow(hWnd)) {
                    QTUtility2.SendCOPYDATASTRUCT(hWnd, (IntPtr)3, string.Empty, IntPtr.Zero);
                }
            }
        }

        private void SyncToolbarTravelButton() {
            if(QTUtility.IsVista) {
                IntPtr ptr = (IntPtr)0x10001;
                IntPtr ptr2 = (IntPtr)0x10000;
                bool flag = (this.navBtnsFlag & 1) != 0;
                bool flag2 = (this.navBtnsFlag & 2) != 0;
                PInvoke.SendMessage(this.TravelToolBarHandle, 0x401, (IntPtr)0x100, flag ? ptr : ptr2);
                PInvoke.SendMessage(this.TravelToolBarHandle, 0x401, (IntPtr)0x101, flag2 ? ptr : ptr2);
                PInvoke.SendMessage(this.TravelToolBarHandle, 0x401, (IntPtr)0x102, (flag || flag2) ? ptr : ptr2);
            }
        }

        private void SyncTravelState() {
            if(this.CurrentTab != null) {
                this.navBtnsFlag = ((this.CurrentTab.HistoryCount_Back > 1) ? 1 : 0) | ((this.CurrentTab.HistoryCount_Forward > 0) ? 2 : 0);
                if(QTUtility.CheckConfig(Settings.ShowNavButtons) && (this.toolStrip != null)) {
                    this.buttonBack.Enabled = (this.navBtnsFlag & 1) != 0;
                    this.buttonForward.Enabled = (this.navBtnsFlag & 2) != 0;
                    this.buttonNavHistoryMenu.Enabled = this.navBtnsFlag != 0;
                }
                this.SyncButtonBarCurrent(0x100bf);
                this.SyncToolbarTravelButton();
            }
        }

        private void tabControl1_CloseButtonClicked(object sender, QTabCancelEventArgs e) {
            if(this.NowTabDragging) {
                this.Cursor = Cursors.Default;
                this.NowTabDragging = false;
                this.DraggingTab = null;
                this.DraggingDestRect = Rectangle.Empty;
                this.SyncButtonBarCurrent(12);
                e.Cancel = true;
            }
            else if(!base.Explorer.Busy) {
                QTabItem tabPage = (QTabItem)e.TabPage;
                if(this.tabControl1.TabCount > 1) {
                    e.Cancel = !this.CloseTab(tabPage);
                }
                else {
                    WindowUtils.CloseExplorer(this.ExplorerHandle, 1);
                }
            }
        }

        private void tabControl1_Deselecting(object sender, QTabCancelEventArgs e) {
            if(e.TabPageIndex != -1) {
                this.SaveSelectedItems((QTabItem)e.TabPage);
            }
        }

        private void tabControl1_ItemDrag(object sender, ItemDragEventArgs e) {
            QTabItem item = (QTabItem)e.Item;
            string currentPath = item.CurrentPath;
            if(Directory.Exists(currentPath)) {
                ShellMethods.DoDragDrop(currentPath, this);
            }
        }

        private void tabControl1_MouseDoubleClick(object sender, MouseEventArgs e) {
            if((Control.ModifierKeys != Keys.Control) && (e.Button == MouseButtons.Left)) {
                QTabItem tabMouseOn = (QTabItem)this.tabControl1.GetTabMouseOn();
                if(tabMouseOn != null) {
                    this.HandleTabClickAction(tabMouseOn, false);
                }
                else {
                    this.OnMouseDoubleClick(e);
                }
            }
        }

        private void tabControl1_MouseDown(object sender, MouseEventArgs e) {
            QTabItem tabMouseOn = (QTabItem)this.tabControl1.GetTabMouseOn();
            this.DraggingTab = null;
            if(tabMouseOn != null) {
                if(e.Button == MouseButtons.Left) {
                    this.NowTabDragging = true;
                    this.DraggingTab = tabMouseOn;
                }
                else if(e.Button == MouseButtons.Right) {
                    this.ContextMenuedTab = tabMouseOn;
                }
            }
        }

        private void tabControl1_MouseEnter(object sender, EventArgs e) {
            if(this.pluginManager != null) {
                this.pluginManager.OnMouseEnter();
            }
        }

        private void tabControl1_MouseLeave(object sender, EventArgs e) {
            if(this.pluginManager != null) {
                this.pluginManager.OnMouseLeave();
            }
        }

        private void tabControl1_MouseMove(object sender, MouseEventArgs e) {
            RECT rect;
            if((this.tabControl1.Capture && (((e.X < 0) || (e.Y < 0)) || ((e.X > this.tabControl1.Width) || (e.Y > this.tabControl1.Height)))) && (PInvoke.GetWindowRect(base.ReBarHandle, out rect) && !PInvoke.PtInRect(ref rect, new BandObjectLib.POINT(this.tabControl1.PointToScreen(e.Location))))) {
                this.Cursor = Cursors.Default;
                this.tabControl1.Capture = false;
            }
            else if((this.NowTabDragging && (this.DraggingTab != null)) && ((Control.ModifierKeys & Keys.Shift) != Keys.Shift)) {
                if(base.Explorer.Busy || (Control.MouseButtons != MouseButtons.Left)) {
                    this.NowTabDragging = false;
                    this.DraggingTab = null;
                }
                else {
                    int num;
                    QTabItem tabMouseOn = (QTabItem)this.tabControl1.GetTabMouseOn(out num);
                    int index = this.tabControl1.TabPages.IndexOf(this.DraggingTab);
                    if((num > (this.tabControl1.TabCount - 1)) || (num < 0)) {
                        if((num == -1) && (Control.ModifierKeys == Keys.Control)) {
                            this.Cursor = this.GetCursor(false);
                            this.DraggingDestRect = new Rectangle(1, 0, 0, 0);
                        }
                        else {
                            this.Cursor = Cursors.Default;
                        }
                    }
                    else if((index <= (this.tabControl1.TabCount - 1)) && (index >= 0)) {
                        Rectangle tabRect = this.tabControl1.GetTabRect(num, false);
                        Rectangle rectangle2 = this.tabControl1.GetTabRect(index, false);
                        if(tabMouseOn != null) {
                            if(tabMouseOn != this.DraggingTab) {
                                if(!this.DraggingDestRect.Contains(this.tabControl1.PointToClient(Control.MousePosition))) {
                                    this.Cursor = this.GetCursor(true);
                                    bool flag = tabMouseOn.Row != this.DraggingTab.Row;
                                    bool flag2 = this.tabControl1.SelectedTab != this.DraggingTab;
                                    this.tabControl1.TabPages.Swap(index, num);
                                    if(num < index) {
                                        this.DraggingDestRect = new Rectangle(tabRect.X + rectangle2.Width, tabRect.Y, tabRect.Width - rectangle2.Width, tabRect.Height);
                                    }
                                    else {
                                        this.DraggingDestRect = new Rectangle(tabRect.X, tabRect.Y, tabRect.Width - rectangle2.Width, tabRect.Height);
                                    }
                                    if((flag && !flag2) && !QTUtility.CheckConfig(Settings.MultipleRow2)) {
                                        Rectangle rectangle3 = this.tabControl1.GetTabRect(num, false);
                                        Point p = new Point(rectangle3.X + (rectangle3.Width / 2), rectangle3.Y + (QTUtility.TabHeight / 2));
                                        Cursor.Position = this.tabControl1.PointToScreen(p);
                                    }
                                    this.SyncButtonBarCurrent(12);
                                }
                            }
                            else if((this.curTabCloning != null) && (this.Cursor == this.curTabCloning)) {
                                this.Cursor = this.GetCursor(true);
                            }
                        }
                    }
                }
            }
        }

        private void tabControl1_MouseUp(object sender, MouseEventArgs e) {
            if(this.NowTabDragging && (e.Button == MouseButtons.Left)) {
                QTabItem tabMouseOn = (QTabItem)this.tabControl1.GetTabMouseOn();
                Keys modifierKeys = Control.ModifierKeys;
                if(((tabMouseOn == null) && (this.DraggingTab != null)) && ((modifierKeys == Keys.Control) || (modifierKeys == (Keys.Control | Keys.Shift)))) {
                    bool flag = false;
                    BandObjectLib.POINT pt = new BandObjectLib.POINT(this.tabControl1.PointToScreen(e.Location));
                    if(QTUtility.IsVista) {
                        RECT rect;
                        PInvoke.GetWindowRect(base.ReBarHandle, out rect);
                        flag = PInvoke.PtInRect(ref rect, pt);
                    }
                    else {
                        RECT rect2;
                        IntPtr ptr;
                        if(QTUtility.instanceManager.TryGetButtonBarHandle(this.ExplorerHandle, out ptr) && PInvoke.IsWindowVisible(ptr)) {
                            PInvoke.GetWindowRect(ptr, out rect2);
                            if(PInvoke.PtInRect(ref rect2, pt)) {
                                flag = true;
                            }
                        }
                        PInvoke.GetWindowRect(base.Handle, out rect2);
                        if(PInvoke.PtInRect(ref rect2, pt)) {
                            flag = true;
                        }
                    }
                    if(flag) {
                        this.CloneTabButton(this.DraggingTab, null, false, this.tabControl1.TabCount);
                    }
                }
                else if(((tabMouseOn != null) && (tabMouseOn == this.DraggingTab)) && ((modifierKeys == Keys.Control) && (this.DraggingDestRect == Rectangle.Empty))) {
                    this.DraggingTab.TabLocked = !this.DraggingTab.TabLocked;
                }
                this.NowTabDragging = false;
                this.DraggingTab = null;
                this.DraggingDestRect = Rectangle.Empty;
                this.SyncButtonBarCurrent(12);
            }
            else if((e.Button == MouseButtons.Middle) && !base.Explorer.Busy) {
                this.DraggingTab = null;
                this.NowTabDragging = false;
                QTabItem clickedTab = (QTabItem)this.tabControl1.GetTabMouseOn();
                if((clickedTab != null) && ((Control.ModifierKeys & Keys.Control) != Keys.Control)) {
                    this.HandleTabClickAction(clickedTab, true);
                }
            }
            this.Cursor = Cursors.Default;
        }

        private void tabControl1_PointedTabChanged(object sender, QTabCancelEventArgs e) {
            if(this.pluginManager != null) {
                if(e.Action == TabControlAction.Selecting) {
                    QTabItem tabPage = (QTabItem)e.TabPage;
                    this.pluginManager.OnPointedTabChanged(e.TabPageIndex, tabPage.CurrentIDL, tabPage.CurrentPath);
                }
                else if(e.Action == TabControlAction.Deselecting) {
                    this.pluginManager.OnPointedTabChanged(-1, null, string.Empty);
                }
            }
        }

        private void tabControl1_RowCountChanged(object sender, QEventArgs e) {
            this.SetBarRows(e.RowCount);
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e) {
            QTabItem selectedTab = (QTabItem)this.tabControl1.SelectedTab;
            string currentPath = selectedTab.CurrentPath;
            if(IsSpecialFolderNeedsToTravel(currentPath) && this.LogEntryDic.ContainsKey(selectedTab.GetLogHash(true, 0))) {
                this.NavigatedByCode = true;
                this.CurrentTab = selectedTab;
                while(this.lstActivatedTabs.Remove(this.CurrentTab)) {
                }
                this.lstActivatedTabs.Add(this.CurrentTab);
                if(this.lstActivatedTabs.Count > 15) {
                    this.lstActivatedTabs.RemoveAt(0);
                }
                this.fNavigatedByTabSelection = this.NavigateToPastSpecialDir(this.CurrentTab.GetLogHash(true, 0));
                if(this.pluginManager != null) {
                    this.pluginManager.OnTabChanged(this.tabControl1.SelectedIndex, selectedTab.CurrentIDL, selectedTab.CurrentPath);
                }
                if(this.tabControl1.Focused) {
                    listViewWrapper.SetFocus();
                }
            }
            else {
                IDLWrapper idlw = null;
                if((selectedTab.CurrentIDL != null) && (selectedTab.CurrentIDL.Length > 0)) {
                    idlw = new IDLWrapper(selectedTab.CurrentIDL, true);
                }
                if((idlw == null) || !idlw.Available) {
                    idlw = new IDLWrapper(selectedTab.CurrentPath);
                }
                using(idlw) {
                    if(!idlw.Available) {
                        this.CancelFailedTabChanging(currentPath);
                        return;
                    }
                    this.CurrentTab = selectedTab;
                    while(this.lstActivatedTabs.Remove(this.CurrentTab)) {
                    }
                    this.lstActivatedTabs.Add(this.CurrentTab);
                    if(this.lstActivatedTabs.Count > 15) {
                        this.lstActivatedTabs.RemoveAt(0);
                    }
                    if(((currentPath != this.CurrentAddress) || (!QTUtility.IsVista && (currentPath == QTUtility.PATH_SEARCHFOLDER))) || this.NowTabCloned) {
                        this.NavigatedByCode = true;
                        this.fNavigatedByTabSelection = true;
                        this.NowTabCloned = false;
                        if(this.Navigate(idlw) != 0) {
                            this.CancelFailedTabChanging(currentPath);
                            return;
                        }
                    }
                    else {
                        this.SyncTravelState();
                    }
                }
                if(this.tabControl1.Focused) {
                    listViewWrapper.SetFocus();
                }
                if(this.pluginManager != null) {
                    this.pluginManager.OnTabChanged(this.tabControl1.SelectedIndex, this.CurrentTab.CurrentIDL, this.CurrentTab.CurrentPath);
                }
            }
        }

        private void tabControl1_Selecting(object sender, QTabCancelEventArgs e) {
            if(this.NowTabsAddingRemoving) {
                e.Cancel = true;
            }
        }

        private void tabControl1_TabCountChanged(object sender, QTabCancelEventArgs e) {
            if(this.pluginManager != null) {
                QTabItem tabPage = (QTabItem)e.TabPage;
                if(e.Action == TabControlAction.Selected) {
                    this.pluginManager.OnTabAdded(e.TabPageIndex, tabPage.CurrentIDL, tabPage.CurrentPath);
                }
                else if(e.Action == TabControlAction.Deselected) {
                    this.pluginManager.OnTabRemoved(e.TabPageIndex, tabPage.CurrentIDL, tabPage.CurrentPath);
                }
            }
        }

        private void tabControl1_TabIconMouseDown(object sender, QTabCancelEventArgs e) {
            this.ShowSubdirTip_Tab((QTabItem)e.TabPage, e.Action == TabControlAction.Selecting, e.TabPageIndex, false, e.Cancel);
        }

        private void tabSwitcher_Switched(object sender, ItemCheckEventArgs e) {
            this.tabControl1.SelectedIndex = e.Index;
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
            if(Control.MouseButtons != MouseButtons.None) {
                //IntPtr tag = (IntPtr)this.timer_HoverSubDirTipMenu.Tag;
                
                int idx = listViewWrapper.GetHotItem();
                if(this.itemIndexDROPHILITED == idx) {
                    if(this.subDirTip != null) {
                        this.subDirTip.HideMenu();
                    }
                    if(this.ShowSubDirTip(this.itemIndexDROPHILITED, true)) {
                        WindowUtils.BringExplorerToFront(this.ExplorerHandle);
                        this.subDirTip.ShowMenu();
                        return;
                    }
                }
                if((this.subDirTip != null) && this.subDirTip.IsMouseOnMenus) {
                    this.itemIndexDROPHILITED = -1;
                    return;
                }
            }
            this.HideSubDirTip(10);
        }

        private void timer_HoverThumbnail_Tick(object sender, EventArgs e) {
            this.timer_HoverThumbnail.Enabled = false;
        }

        private void timer_Thumbnail_Tick(object sender, EventArgs e) {
            this.timer_Thumbnail.Enabled = false;
            this.fThumbnailPending = false;
        }

        private void timerOnTab_Tick(object sender, EventArgs e) {
            this.timerOnTab.Enabled = false;
            QTabItem tabMouseOn = (QTabItem)this.tabControl1.GetTabMouseOn();
            if(((tabMouseOn != null) && (tabMouseOn == this.tabForDD)) && this.tabControl1.TabPages.Contains(tabMouseOn)) {
                if(QTUtility.CheckConfig(Settings.DragDropOntoTabs)) {
                    WindowUtils.BringExplorerToFront(this.ExplorerHandle);
                    this.ShowSubdirTip_Tab(tabMouseOn, true, this.tabControl1.TabOffset, false, this.fToggleTabMenu);
                    this.fToggleTabMenu = !this.fToggleTabMenu;
                    this.timerOnTab.Enabled = true;
                    if(this.toolTipForDD != null) {
                        this.toolTipForDD.Active = false;
                    }
                }
                else {
                    this.tabControl1.SelectTab(tabMouseOn);
                }
            }
        }

        private void timerSelectionChanged_Tick(object sender, EventArgs e) {
            try {
                this.timerSelectionChanged.Enabled = false;
                if((this.pluginManager != null) && (this.CurrentTab != null)) {
                    this.pluginManager.OnSelectionChanged(this.tabControl1.SelectedIndex, this.CurrentTab.CurrentIDL, this.CurrentTab.CurrentPath);
                }
            }
            catch {
            }
        }

        private void ToggleTopMost() {
            if(PInvoke.Ptr_OP_AND(PInvoke.GetWindowLongPtr(this.ExplorerHandle, -20), 8) != IntPtr.Zero) {
                PInvoke.SetWindowPos(this.ExplorerHandle, (IntPtr)(-2), 0, 0, 0, 0, 3);
                this.NowTopMost = false;
            }
            else {
                PInvoke.SetWindowPos(this.ExplorerHandle, (IntPtr)(-1), 0, 0, 0, 0, 3);
                this.NowTopMost = true;
            }
        }

        public override int TranslateAcceleratorIO(ref BandObjectLib.MSG msg) {
            if(msg.message == WM.KEYDOWN) {
                Keys wParam = (Keys)((int)((long)msg.wParam));
                bool flag = (((int)((long)msg.lParam)) & 0x40000000) != 0;
                switch(wParam) {
                    case Keys.Delete: {
                            if(!this.tabControl1.Focused || ((this.subDirTip_Tab != null) && this.subDirTip_Tab.MenuIsShowing)) {
                                break;
                            }
                            int focusedTabIndex = this.tabControl1.GetFocusedTabIndex();
                            if((-1 < focusedTabIndex) && (focusedTabIndex < this.tabControl1.TabCount)) {
                                bool flag3 = focusedTabIndex == (this.tabControl1.TabCount - 1);
                                if(this.CloseTab((QTabItem)this.tabControl1.TabPages[focusedTabIndex]) && flag3) {
                                    this.tabControl1.FocusNextTab(true, false, false);
                                }
                            }
                            return 0;
                        }
                    case Keys.Apps:
                        if(!flag) {
                            int index = this.tabControl1.GetFocusedTabIndex();
                            if((-1 >= index) || (index >= this.tabControl1.TabCount)) {
                                break;
                            }
                            this.ContextMenuedTab = (QTabItem)this.tabControl1.TabPages[index];
                            Rectangle tabRect = this.tabControl1.GetTabRect(index, true);
                            this.contextMenuTab.Show(base.PointToScreen(new Point(tabRect.Right + 10, tabRect.Bottom - 10)));
                        }
                        return 0;

                    case Keys.F6:
                    case Keys.Tab:
                    case Keys.Left:
                    case Keys.Right: {
                            if(!this.tabControl1.Focused || ((this.subDirTip_Tab != null) && this.subDirTip_Tab.MenuIsShowing)) {
                                break;
                            }
                            bool fBack = (Control.ModifierKeys == Keys.Shift) || (wParam == Keys.Left);
                            if(!this.tabControl1.FocusNextTab(fBack, false, false)) {
                                break;
                            }
                            return 0;
                        }
                    case Keys.Back:
                        return 0;

                    case Keys.Return:
                    case Keys.Space:
                        if(!flag && !this.tabControl1.SelectFocusedTab()) {
                            break;
                        }
                        listViewWrapper.SetFocus();
                        return 0;

                    case Keys.Escape:
                        if(this.tabControl1.Focused && ((this.subDirTip_Tab == null) || !this.subDirTip_Tab.MenuIsShowing)) {
                            listViewWrapper.SetFocus();
                        }
                        break;

                    case Keys.End:
                    case Keys.Home:
                        if((!this.tabControl1.Focused || ((this.subDirTip_Tab != null) && this.subDirTip_Tab.MenuIsShowing)) || !this.tabControl1.FocusNextTab(wParam == Keys.Home, false, true)) {
                            break;
                        }
                        return 0;

                    case Keys.Up:
                    case Keys.Down:
                        if(((!QTUtility.CheckConfig(Settings.ShowSubDirTipOnTab) || !this.tabControl1.Focused) || ((this.subDirTip_Tab != null) && this.subDirTip_Tab.MenuIsShowing)) || (!flag && !this.tabControl1.PerformFocusedFolderIconClick(wParam == Keys.Up))) {
                            break;
                        }
                        return 0;
                }
            }
            return base.TranslateAcceleratorIO(ref msg);
        }

        private bool travelBtnController_MessageCaptured(ref System.Windows.Forms.Message m) {
            if(this.CurrentTab == null) {
                return false;
            }
            switch(m.Msg) {
                case WM.LBUTTONDOWN:
                case WM.LBUTTONUP: {
                        BandObjectLib.POINT structure = new BandObjectLib.POINT();
                        structure.x = QTUtility2.GET_X_LPARAM(m.LParam);
                        structure.y = QTUtility2.GET_Y_LPARAM(m.LParam);
                        IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(structure));
                        Marshal.StructureToPtr(structure, ptr, false);
                        int num = (int)PInvoke.SendMessage(this.travelBtnController.Handle, 0x445, IntPtr.Zero, ptr);
                        Marshal.FreeHGlobal(ptr);
                        bool flag = this.CurrentTab.HistoryCount_Back > 1;
                        bool flag2 = this.CurrentTab.HistoryCount_Forward > 0;
                        if(m.Msg != 0x202) {
                            PInvoke.SetCapture(this.travelBtnController.Handle);
                            if(((flag && (num == 0)) || (flag2 && (num == 1))) || ((flag || flag2) && (num == 2))) {
                                int num5 = (int)PInvoke.SendMessage(this.travelBtnController.Handle, 0x412, (IntPtr)(0x100 + num), IntPtr.Zero);
                                int num6 = num5 | 2;
                                PInvoke.SendMessage(this.travelBtnController.Handle, 0x411, (IntPtr)(0x100 + num), (IntPtr)num6);
                            }
                            if((num == 2) && (flag || flag2)) {
                                RECT rect;
                                IntPtr hWnd = PInvoke.SendMessage(this.travelBtnController.Handle, 0x423, IntPtr.Zero, IntPtr.Zero);
                                if(hWnd != IntPtr.Zero) {
                                    PInvoke.SendMessage(hWnd, 0x41c, IntPtr.Zero, IntPtr.Zero);
                                }
                                PInvoke.GetWindowRect(this.travelBtnController.Handle, out rect);
                                this.NavigationButtons_DropDownOpening(this.buttonNavHistoryMenu, new EventArgs());
                                this.buttonNavHistoryMenu.DropDown.Show(new Point(rect.left - 2, rect.bottom + 1));
                            }
                            break;
                        }
                        PInvoke.ReleaseCapture();
                        for(int i = 0; i < 3; i++) {
                            int num3 = (int)PInvoke.SendMessage(this.travelBtnController.Handle, 0x412, (IntPtr)(0x100 + i), IntPtr.Zero);
                            int num4 = num3 & -3;
                            PInvoke.SendMessage(this.travelBtnController.Handle, 0x411, (IntPtr)(0x100 + i), (IntPtr)num4);
                        }
                        if((num == 0) && flag) {
                            this.NavigateCurrentTab(true);
                        }
                        else if((num == 1) && flag2) {
                            this.NavigateCurrentTab(false);
                        }
                        break;
                    }
                case WM.LBUTTONDBLCLK:
                    m.Result = IntPtr.Zero;
                    return true;

                case WM.USER+1:
                    if(((((int)((long)m.LParam)) >> 0x10) & 0xffff) == 1) {
                        goto Label_0427;
                    }
                    m.Result = (IntPtr)1;
                    return true;

                case WM.MOUSEACTIVATE:
                    if(this.buttonNavHistoryMenu.DropDown.Visible) {
                        m.Result = (IntPtr)4;
                        this.buttonNavHistoryMenu.DropDown.Close(ToolStripDropDownCloseReason.AppClicked);
                        return true;
                    }
                    goto Label_0427;

                case WM.NOTIFY: {
                        NMHDR nmhdr = (NMHDR)Marshal.PtrToStructure(m.LParam, typeof(NMHDR));
                        if(nmhdr.code != -530) {
                            goto Label_0427;
                        }
                        NMTTDISPINFO nmttdispinfo = (NMTTDISPINFO)Marshal.PtrToStructure(m.LParam, typeof(NMTTDISPINFO));
                        string str = string.Empty;
                        if(nmttdispinfo.hdr.idFrom == ((IntPtr)0x100)) {
                            str = this.MakeTravelBtnTooltipText(true);
                            if(str.Length > 0x4f) {
                                str = "Back";
                            }
                        }
                        else if(nmttdispinfo.hdr.idFrom == ((IntPtr)0x101)) {
                            str = this.MakeTravelBtnTooltipText(false);
                            if(str.Length > 0x4f) {
                                str = "Forward";
                            }
                        }
                        else {
                            return false;
                        }
                        nmttdispinfo.szText = str;
                        Marshal.StructureToPtr(nmttdispinfo, m.LParam, false);
                        m.Result = IntPtr.Zero;
                        return true;
                    }
                default:
                    goto Label_0427;
            }
            m.Result = IntPtr.Zero;
            return true;
        Label_0427:
            return false;
        }

        private bool TryGetHotTrackPath(int iItem, out string path) {
            return TryGetHotTrackPath(iItem, out path, null);
        }

        private bool TryGetHotTrackPath(int iItem, out string path, string matchName) {
            path = null;
            if(!this.fIFolderViewNotImplemented) {
                IShellView ppshv = null;
                IFolderView view2 = null;
                IPersistFolder2 ppv = null;
                IntPtr zero = IntPtr.Zero;
                IntPtr ppidl = IntPtr.Zero;
                IntPtr ptr3 = IntPtr.Zero;
                try {
                    if(this.ShellBrowser.QueryActiveShellView(out ppshv) == 0) {
                        try {
                            view2 = (IFolderView)ppshv;
                        }
                        catch(InvalidCastException) {
                            this.fIFolderViewNotImplemented = true;
                            return false;
                        }
                        Guid riid = ExplorerGUIDs.IID_IPersistFolder2;
                        if((((view2 != null) && (view2.GetFolder(ref riid, out ppv) == 0)) && ((ppv != null) && (ppv.GetCurFolder(out ptr3) == 0))) && (((ptr3 != IntPtr.Zero) && (view2.Item(iItem, out ppidl) == 0)) && (ppidl != IntPtr.Zero))) {
                            if(!string.IsNullOrEmpty(matchName)) {
                                string realName = ShellMethods.GetDisplayName(ppidl, false);
                                if(realName != matchName) {
                                    path = null;
                                    return false;
                                }
                            }
                            zero = PInvoke.ILCombine(ptr3, ppidl);
                            path = ShellMethods.GetDisplayName(zero, false);
                            if(!string.IsNullOrEmpty(path)) {
                                return true;
                            }
                            path = null;
                        }
                    }
                }
                catch(Exception exception) {
                    QTUtility2.MakeErrorLog(exception, null);
                }
                finally {
                    if(ppshv != null) {
                        Marshal.ReleaseComObject(ppshv);
                    }
                    if(ppv != null) {
                        Marshal.ReleaseComObject(ppv);
                    }
                    if(ppidl != IntPtr.Zero) {
                        PInvoke.CoTaskMemFree(ppidl);
                    }
                    if(zero != IntPtr.Zero) {
                        PInvoke.CoTaskMemFree(zero);
                    }
                    if(ptr3 != IntPtr.Zero) {
                        PInvoke.CoTaskMemFree(ptr3);
                    }
                }
            }
            return false;
        }

        internal bool TryGetSelection(out Address[] adSelectedItems, out string pathFocused, bool fDisplayName) {
            adSelectedItems = new Address[0];
            pathFocused = string.Empty;
            List<Address> list = new List<Address>();
            if(!this.fIFolderViewNotImplemented) {
                IShellFolder shellFolder = null;
                IShellView ppshv = null;
                IFolderView view2 = null;
                IPersistFolder2 ppv = null;
                IEnumIDList list2 = null;
                IntPtr zero = IntPtr.Zero;
                try {
                    if(this.ShellBrowser.QueryActiveShellView(out ppshv) == 0) {
                        int num;
                        IntPtr ptr2;
                        int num2;
                        try {
                            view2 = (IFolderView)ppshv;
                        }
                        catch(InvalidCastException) {
                            this.fIFolderViewNotImplemented = true;
                            return false;
                        }
                        Guid riid = ExplorerGUIDs.IID_IPersistFolder2;
                        Guid guid2 = ExplorerGUIDs.IID_IEnumIDList;
                        if(view2.GetFolder(ref riid, out ppv) != 0) {
                            return false;
                        }
                        if((ppv.GetCurFolder(out zero) != 0) || (zero == IntPtr.Zero)) {
                            return false;
                        }
                        if(!ShellMethods.GetShellFolder(zero, out shellFolder)) {
                            return false;
                        }
                        if((view2.GetFocusedItem(out num) == 0) && (view2.Item(num, out ptr2) == 0)) {
                            STRRET strret;
                            IntPtr pv = PInvoke.ILCombine(zero, ptr2);
                            StringBuilder pszBuf = new StringBuilder(260);
                            if(shellFolder.GetDisplayNameOf(ptr2, 0x8000, out strret) == 0) {
                                PInvoke.StrRetToBuf(ref strret, ptr2, pszBuf, pszBuf.Capacity);
                            }
                            pathFocused = pszBuf.ToString();
                            PInvoke.CoTaskMemFree(ptr2);
                            PInvoke.CoTaskMemFree(pv);
                        }
                        if(view2.ItemCount(1, out num2) != 0) {
                            return false;
                        }
                        if(num2 != 0) {
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
                                IntPtr pidl = PInvoke.ILCombine(zero, ptr4);
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
                        shellFolder = null;
                    }
                    if(ppshv != null) {
                        Marshal.ReleaseComObject(ppshv);
                        ppshv = null;
                    }
                    if(ppv != null) {
                        Marshal.ReleaseComObject(ppv);
                        ppv = null;
                    }
                    if(list2 != null) {
                        Marshal.ReleaseComObject(list2);
                        list2 = null;
                    }
                    if(zero != IntPtr.Zero) {
                        PInvoke.CoTaskMemFree(zero);
                    }
                }
            }
            return false;
        }

        internal static bool TryMakeSubDirTipPath(ref string path) {
            if(!string.IsNullOrEmpty(path)) {
                bool flag;
                if(path.StartsWith("::")) {
                    using(IDLWrapper wrapper = new IDLWrapper(path)) {
                        return wrapper.IsFolder;
                    }
                }
                if(path.StartsWith(@"a:\", StringComparison.OrdinalIgnoreCase) || path.StartsWith(@"b:\", StringComparison.OrdinalIgnoreCase)) {
                    return false;
                }
                if(!Directory.Exists(path)) {
                    if(!File.Exists(path) || !string.Equals(Path.GetExtension(path), ".lnk", StringComparison.OrdinalIgnoreCase)) {
                        return false;
                    }
                    path = ShellMethods.GetLinkTargetPath(path);
                    if(!Directory.Exists(path)) {
                        return false;
                    }
                }
                FileSystemInfo targetIfFolderLink = GetTargetIfFolderLink(new DirectoryInfo(path), out flag);
                if(flag) {
                    bool fSearchHidden = QTUtility.CheckConfig(Settings.SubDirTipsHidden);
                    bool fSearchSystem = QTUtility.CheckConfig(Settings.SubDirTipsSystem);
                    bool flag4 = QTUtility.CheckConfig(Settings.SubDirTipsFiles);
                    path = targetIfFolderLink.FullName;
                    using(FindFile file = new FindFile(path, fSearchHidden, fSearchSystem)) {
                        return (file.SubDirectoryExists() || (flag4 && file.SubFileExists()));
                    }
                }
            }
            return false;
        }

        internal bool TrySetSelection(Address[] addresses, string pathToFocus, bool fDeselectOthers) {
            if(addresses != null) {
                IShellFolder ppshf = null;
                IShellView ppshv = null;
                try {
                    if(this.ShellBrowser.QueryActiveShellView(out ppshv) == 0) {
                        IntPtr ptr3;
                        if(PInvoke.SHGetDesktopFolder(out ppshf) != 0) {
                            return false;
                        }
                        bool flag = true;
                        bool flag2 = false;
                        bool flag3 = (pathToFocus != null) && (pathToFocus.Length > 0);
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
                        ppshv = null;
                    }
                    if(ppshf != null) {
                        Marshal.ReleaseComObject(ppshf);
                        ppshf = null;
                    }
                }
            }
            return false;
        }

        private void tsmiBranchRoot_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            QTabItem tag = (QTabItem)((ToolStripMenuItem)sender).Tag;
            if(tag != null) {
                this.NavigateBranches(tag, ((QMenuItem)e.ClickedItem).MenuItemArguments.Index);
            }
        }

        public override void UIActivateIO(int fActivate, ref BandObjectLib.MSG Msg) {
            if(fActivate != 0) {
                this.tabControl1.Focus();
                this.tabControl1.FocusNextTab(Control.ModifierKeys == Keys.Shift, true, false);
            }
        }

        [ComUnregisterFunction]
        private static void Unregister(System.Type t) {
            string name = t.GUID.ToString("B");
            try {
                using(RegistryKey key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Internet Explorer\Toolbar")) {
                    key.DeleteValue(name, false);
                }
            }
            catch {
            }
            try {
                using(RegistryKey key2 = Registry.ClassesRoot.OpenSubKey("CLSID", true)) {
                    try {
                        key2.DeleteSubKeyTree(name);
                    }
                    catch {
                    }
                    try {
                        key2.DeleteSubKeyTree("{D2BF470E-ED1C-487F-A444-2BD8835EB6CE}");
                    }
                    catch {
                    }
                }
            }
            catch {
            }
            
            return;
            
            // TODO: Make the following code optional in the Uninstaller.
            try {
                using(RegistryKey key3 = Registry.Users) {
                    try {
                        foreach(string str2 in key3.GetSubKeyNames()) {
                            bool flag = true;
                            try {
                                using(RegistryKey key4 = key3.OpenSubKey(str2 + @"\Software\Quizo", true)) {
                                    if(key4 != null) {
                                        try {
                                            key4.DeleteSubKeyTree("QTTabBar");
                                            string[] subKeyNames = key4.GetSubKeyNames();
                                            flag = (subKeyNames != null) && (subKeyNames.Length > 0);
                                        }
                                        catch {
                                        }
                                    }
                                }
                            }
                            catch {
                            }
                            try {
                                if(!flag) {
                                    using(RegistryKey key5 = key3.OpenSubKey(str2 + @"\Software", true)) {
                                        if(key5 != null) {
                                            key5.DeleteSubKeyTree("Quizo");
                                        }
                                    }
                                }
                            }
                            catch {
                            }
                        }
                    }
                    catch {
                    }
                }
            }
            catch {
            }
        }

        private void UpOneLevel() {
            if(this.CurrentTab.TabLocked) {
                QTabItem tab = this.CurrentTab.Clone();
                this.AddInsertTab(tab);
                this.tabControl1.SelectTab(tab);
            }
            if(QTUtility.IsVista) {
                PInvoke.SendMessage(WindowUtils.GetShellTabWindowClass(this.ExplorerHandle), 0x111, (IntPtr)0xa022, IntPtr.Zero);
            }
            else {
                PInvoke.SendMessage(this.ExplorerHandle, 0x111, (IntPtr)0xa022, IntPtr.Zero);
            }
        }

        internal static void WaitTimeout(int msec) {
            Thread.Sleep(msec);
        }

        protected override void WndProc(ref System.Windows.Forms.Message m) {
            bool flag;
            switch(m.Msg) {

                case WM.APP + 1:
                    this.NowModalDialogShown = m.WParam != IntPtr.Zero;
                    return;

                case WM.DROPFILES:
                    this.HandleFileDrop(m.WParam);
                    break;

                case WM.DRAWITEM:
                case WM.MEASUREITEM:
                case WM.INITMENUPOPUP:
                    if((this.iContextMenu2 != null) && (m.HWnd == base.Handle)) {
                        try {
                            this.iContextMenu2.HandleMenuMsg(m.Msg, m.WParam, m.LParam);
                        }
                        catch {
                        }
                        return;
                    }
                    break;
            }
            if(m.Msg != WM.COPYDATA) {
                base.WndProc(ref m);
                return;
            }
            if(!this.NowModalDialogShown) {
                COPYDATASTRUCT copydatastruct = (COPYDATASTRUCT)Marshal.PtrToStructure(m.LParam, typeof(COPYDATASTRUCT));
                int wParam = (int)m.WParam;
                string str = "null";
                byte[] destination = null;
                switch(wParam) {
                    case 4:
                    case 0x40:
                        destination = new byte[copydatastruct.cbData];
                        Marshal.Copy(copydatastruct.lpData, destination, 0, copydatastruct.cbData);
                        break;

                    default:
                        str = Marshal.PtrToStringAuto(copydatastruct.lpData);
                        if(str == "null") {
                            str = string.Empty;
                        }
                        break;
                }
                if(wParam <= 0xf00) {
                    flag = false;
                    switch(wParam) {
                        case 0:
                        case 2: {
                                if(string.IsNullOrEmpty(str)) {
                                    goto Label_0B07;
                                }
                                string[] strArray = str.Trim().Split(QTUtility.SEPARATOR_CHAR);
                                if(wParam != 2) {
                                    foreach(string str2 in strArray) {
                                        this.OpenGroup(str2, false);
                                    }
                                    flag = true;
                                    goto Label_0B07;
                                }
                                this.NowOpenedByGroupOpener = true;
                                new MethodInvoker(this.CallbackFirstNavComp).BeginInvoke(new AsyncCallback(this.AsyncComplete), strArray);
                                return;
                            }
                        case 1:
                            goto Label_0B07;

                        case 3:
                            this.OpenGroup(str, true);
                            flag = false;
                            goto Label_0B07;

                        case 4:
                        case 0x40: {
                                bool flag2 = true;
                                bool blockSelecting = false;
                                if(wParam == 4) {
                                    switch(((Keys)((int)copydatastruct.dwData))) {
                                        case Keys.Control:
                                            flag2 = false;
                                            break;

                                        case Keys.Shift:
                                            blockSelecting = true;
                                            break;
                                    }
                                }
                                else {
                                    flag2 = copydatastruct.dwData == IntPtr.Zero;
                                }
                                using(IDLWrapper wrapper5 = new IDLWrapper(destination)) {
                                    if(wrapper5.Available) {
                                        if(flag2 && (!wrapper5.HasPath || !QTUtility2.TargetIsInNoCapture(IntPtr.Zero, wrapper5.Path))) {
                                            bool flag4 = this.OpenNewTab(wrapper5, blockSelecting, wParam == 0x40);
                                            if((wParam == 0x40) && !flag4) {
                                                m.Result = (IntPtr)1;
                                            }
                                            flag = true;
                                        }
                                        else {
                                            this.OpenNewWindow(wrapper5);
                                            flag = false;
                                        }
                                    }
                                    goto Label_0B07;
                                }
                            }
                        case 9:
                            if(QTUtility.TMPTargetIDL != null) {
                                Keys dwData = (Keys)((int)copydatastruct.dwData);
                                if(dwData == Keys.Control) {
                                    using(IDLWrapper wrapper6 = new IDLWrapper(QTUtility.TMPTargetIDL)) {
                                        this.OpenNewWindow(wrapper6);
                                        return;
                                    }
                                }
                                bool flag5 = (dwData & Keys.Shift) != Keys.None;
                                using(IDLWrapper wrapper7 = new IDLWrapper(QTUtility.TMPTargetIDL)) {
                                    if(wrapper7.HasPath && QTUtility2.TargetIsInNoCapture(IntPtr.Zero, wrapper7.Path)) {
                                        this.OpenNewWindow(wrapper7);
                                        return;
                                    }
                                    this.OpenNewTab(wrapper7, flag5, false);
                                }
                                foreach(byte[] buffer2 in QTUtility.TMPIDLList) {
                                    using(IDLWrapper wrapper8 = new IDLWrapper(buffer2)) {
                                        this.OpenNewTab(wrapper8, true, false);
                                        continue;
                                    }
                                }
                                if(flag5) {
                                    WindowUtils.BringExplorerToFront(this.ExplorerHandle);
                                }
                            }
                            return;

                        case 15: {
                                string[] strArray2 = str.Split(QTUtility.SEPARATOR_CHAR);
                                if((strArray2.Length == 2) && (strArray2[1].Length > 0)) {
                                    QTUtility.PathToSelectInCommandLineArg = strArray2[1];
                                }
                                if(Control.ModifierKeys != Keys.Control) {
                                    this.OpenNewTab(strArray2[0], false);
                                    flag = true;
                                }
                                else {
                                    using(IDLWrapper wrapper9 = new IDLWrapper(strArray2[0])) {
                                        this.OpenNewWindow(wrapper9);
                                    }
                                    flag = false;
                                }
                                if(!QTUtility.CheckConfig(Settings.NoNewWndFolderTree)) {
                                    this.ShowFolderTree(true);
                                }
                                goto Label_0B07;
                            }
                        case 0x10:
                            if((QTUtility.CheckConfig(Settings.TrayOnClose) && (notifyIcon != null)) && (dicNotifyIcon != null)) {
                                Dictionary<IntPtr, int> dictionary = new Dictionary<IntPtr, int>(dicNotifyIcon);
                                foreach(IntPtr ptr in dictionary.Keys) {
                                    QTTabBarClass tabBar = QTUtility.instanceManager.GetTabBar(ptr);
                                    if((tabBar != null) && (tabBar.CurrentAddress == str)) {
                                        ShowTaksbarItem(ptr, true);
                                        return;
                                    }
                                }
                            }
                            this.OpenNewTab(str, false);
                            QTUtility.RegisterPrimaryInstance(this.ExplorerHandle, this);
                            flag = true;
                            goto Label_0B07;

                        case 0x11:
                            this.RefreshTabBar(copydatastruct.dwData != IntPtr.Zero);
                            return;

                        case 0x12:
                            this.RefreshPlugins(false);
                            return;

                        case 80:
                            this.ReplaceByGroup(str);
                            return;

                        case 0x20:
                            using(RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Quizo\QTTabBar", false)) {
                                if(key != null) {
                                    string[] collection = ((string)key.GetValue("TabsOnLastClosedWindow", string.Empty)).Split(QTUtility.SEPARATOR_CHAR);
                                    if((collection.Length > 0) && (collection[0].Length > 0)) {
                                        if(copydatastruct.dwData == IntPtr.Zero) {
                                            QTUtility.TMPPathList = new List<string>(collection);
                                            using(IDLWrapper wrapper10 = new IDLWrapper(collection[0])) {
                                                this.OpenNewWindow(wrapper10);
                                                return;
                                            }
                                        }
                                        new MethodInvoker(this.CallbackFirstNavComp).BeginInvoke(new AsyncCallback(this.AsyncComplete_MultiPath), new object[] { collection, 0 });
                                    }
                                }
                            }
                            return;

                        case 0x30: {
                                contextMenuNotifyIcon = new ContextMenuStripEx(null, false);
                                contextMenuNotifyIcon.ImageList = QTUtility.ImageListGlobal;
                                contextMenuNotifyIcon.ItemClicked += new ToolStripItemClickedEventHandler(QTTabBarClass.contextMenuNotifyIcon_ItemClicked);
                                IntPtr handle = contextMenuNotifyIcon.Handle;
                                notifyIcon = new NotifyIcon();
                                notifyIcon.Icon = icoNotify;
                                notifyIcon.ContextMenuStrip = contextMenuNotifyIcon;
                                notifyIcon.MouseDoubleClick += new MouseEventHandler(QTTabBarClass.notifyIcon_MouseDoubleClick);
                                hwndNotifyIconParent = this.ExplorerHandle;
                                int count = dicNotifyIcon.Count;
                                string str4 = count + " window" + ((count > 1) ? "s" : string.Empty);
                                if(str4.Length > 0x40) {
                                    str4 = str4.Substring(0, 60) + "...";
                                }
                                notifyIcon.Text = str4;
                                notifyIcon.Visible = true;
                                CreateContextMenuItems_NotifyIcon(IntPtr.Zero, 0);
                                m.Result = (IntPtr)1;
                                return;
                            }
                    }
                    goto Label_0B07;
                }
                int num2 = wParam - 0xf00;
                Keys modifierKeys = Control.ModifierKeys;
                switch(num2) {
                    case 1:
                        this.NavigateCurrentTab(true);
                        return;

                    case 2:
                        this.NavigateCurrentTab(false);
                        return;

                    case 3:
                        this.OpenGroup(str, modifierKeys == Keys.Control);
                        return;

                    case 4: {
                            if(copydatastruct.dwData == IntPtr.Zero) {
                                this.OpenNewTab(str, false);
                                return;
                            }
                            using(IDLWrapper wrapper3 = new IDLWrapper(str)) {
                                this.OpenNewWindow(wrapper3);
                                return;
                            }
                        }
                    case 5:
                    case 9:
                    case 0xf4:
                    case 0xf5:
                    case 0xf6:
                    case 0xf7:
                    case 0xf8:
                    case 0xf9:
                    case 0xfe:
                        return;

                    case 6: {
                            using(IDLWrapper wrapper4 = new IDLWrapper(this.CurrentTab.CurrentIDL)) {
                                this.OpenNewWindow(wrapper4);
                                return;
                            }
                        }
                    case 7:
                        this.CloneTabButton(this.CurrentTab, null, true, -1);
                        return;

                    case 8:
                        this.CurrentTab.TabLocked = !this.CurrentTab.TabLocked;
                        return;

                    case 10:
                        this.ToggleTopMost();
                        return;

                    case 11:
                        if(QTUtility.CheckConfig(Settings.NeverCloseWindow)) {
                            this.CloseTab(this.CurrentTab);
                            return;
                        }
                        this.CloseTab(this.CurrentTab, false);
                        if(this.tabControl1.TabCount == 0) {
                            WindowUtils.CloseExplorer(this.ExplorerHandle, 2);
                        }
                        return;

                    case 12:
                        if(this.tabControl1.TabCount > 1) {
                            foreach(QTabItem item in this.tabControl1.TabPages) {
                                if(item != this.CurrentTab) {
                                    this.CloseTab(item);
                                }
                            }
                        }
                        return;

                    case 13:
                        WindowUtils.CloseExplorer(this.ExplorerHandle, 1);
                        return;

                    case 14:
                        this.CloseLeftRight(true, -1);
                        return;

                    case 15:
                        this.CloseLeftRight(false, -1);
                        return;

                    case 0x10:
                        this.UpOneLevel();
                        return;

                    case 0x11:
                        base.Explorer.Refresh();
                        return;

                    case 0x12:
                        this.ShowSearchBar(true);
                        return;

                    case 0x30: {
                            if(modifierKeys != Keys.Control) {
                                this.OpenNewTab(str, false);
                                return;
                            }
                            using(IDLWrapper wrapper2 = new IDLWrapper(str)) {
                                this.OpenNewWindow(wrapper2);
                                return;
                            }
                        }
                    case 0xf1:
                    case 0xf2: {
                            object[] tag = new object[] { str, num2 == 0xf1, (int)copydatastruct.dwData };
                            if(modifierKeys != Keys.Shift) {
                                if(modifierKeys == Keys.Control) {
                                    using(IDLWrapper wrapper = new IDLWrapper(str)) {
                                        this.OpenNewWindow(wrapper);
                                        return;
                                    }
                                }
                                this.NavigateToHistory(tag);
                                return;
                            }
                            this.CloneTabButton(this.CurrentTab, null, true, -1);
                            this.NavigateToHistory(tag);
                            return;
                        }
                    case 0xf3:
                        this.NavigateBranches(this.CurrentTab, (int)copydatastruct.dwData);
                        return;

                    case 250:
                        this.HideThumbnailTooltip(9);
                        this.HideSubDirTip(9);
                        return;

                    case 0xfb:
                        if(Directory.Exists(str)) {
                            this.OpenNewTab(str, false);
                        }
                        return;

                    case 0xfc:
                        if(!(copydatastruct.dwData == ((IntPtr)1))) {
                            this.contextMenuSys.Show(Control.MousePosition);
                            return;
                        }
                        this.contextMenuSys.Show(base.PointToScreen(Point.Empty));
                        return;

                    case 0xfd:
                        this.OnMouseDoubleClick(new MouseEventArgs(MouseButtons.Left, 0, 0, 0, 0));
                        return;

                    case 0xff:
                        this.SyncButtonBarCurrent(0x3f);
                        return;

                    default:
                        return;
                }
            }
            return;
        Label_0B07:
            if(flag) {
                bool flag6 = QTUtility.IsVista && PInvoke.IsIconic(this.ExplorerHandle);
                this.RestoreFromTray();
                WindowUtils.BringExplorerToFront(this.ExplorerHandle);
                if(flag6) {
                    foreach(QTabItem item2 in this.tabControl1.TabPages) {
                        item2.RefreshRectangle();
                    }
                    this.tabControl1.Refresh();
                }
            }
        }

        internal PluginManager PluginServerInstance {
            get {
                return this.pluginManager;
            }
        }

        internal sealed class PluginServer : IPluginServer, IDisposable {
            private Dictionary<string, string[]> dicLocalizingStrings;
            private PluginManager pluginManager;
            private QTPlugin.Interop.IShellBrowser shellBrowser;
            private QTTabBarClass tabBar;

            public event PluginEventHandler ExplorerStateChanged;

            public event EventHandler MenuRendererChanged;

            public event EventHandler MouseEnter;

            public event EventHandler MouseLeave;

            public event PluginEventHandler NavigationComplete;

            public event PluginEventHandler PointedTabChanged;

            public event PluginEventHandler SelectionChanged;

            public event PluginEventHandler SettingsChanged;

            public event PluginEventHandler TabAdded;

            public event PluginEventHandler TabChanged;

            public event PluginEventHandler TabRemoved;

            public PluginServer(QTTabBarClass tabBar, PluginManager manager) {
                this.tabBar = tabBar;
                this.shellBrowser = (QTPlugin.Interop.IShellBrowser)this.tabBar.ShellBrowser;
                this.pluginManager = manager;
                if((QTUtility.Path_PluginLangFile.Length > 0) && File.Exists(QTUtility.Path_PluginLangFile)) {
                    this.dicLocalizingStrings = QTUtility.ReadLanguageFile(QTUtility.Path_PluginLangFile);
                }
                if(this.dicLocalizingStrings == null) {
                    this.dicLocalizingStrings = new Dictionary<string, string[]>();
                }
            }

            public bool AddApplication(string name, ProcessStartInfo startInfo) {
                return false;
            }

            public bool AddGroup(string groupName, string[] paths) {
                if((paths != null) && (paths.Length > 0)) {
                    string str = string.Empty;
                    for(int i = 0; i < paths.Length; i++) {
                        str = str + paths[i] + ";";
                    }
                    str = str.TrimEnd(QTUtility.SEPARATOR_CHAR);
                    if(str.Length > 0) {
                        string str2;
                        if(QTUtility.GroupPathsDic.TryGetValue(groupName, out str2)) {
                            str = ((str2.Length == 0) ? string.Empty : (str2 + ";")) + str;
                        }
                        QTUtility.GroupPathsDic[groupName] = str;
                        QTUtility.SaveGroupsReg();
                        return true;
                    }
                }
                return false;
            }

            private static byte[] AddressToIDL(Address address) {
                if((address.ITEMIDLIST != null) && (address.ITEMIDLIST.Length != 0)) {
                    return address.ITEMIDLIST;
                }
                if((address.Path != null) && (address.Path.Length != 0)) {
                    IntPtr pIDL = PInvoke.ILCreateFromPath(address.Path);
                    if(pIDL != IntPtr.Zero) {
                        byte[] iDLData = ShellMethods.GetIDLData(pIDL);
                        PInvoke.CoTaskMemFree(pIDL);
                        return iDLData;
                    }
                }
                return null;
            }

            private static string AddressToPath(Address address) {
                string str = string.Empty;
                if((address.Path == null) || (address.Path.Length == 0)) {
                    IntPtr pidl = AddressToPIDL(address);
                    if(pidl != IntPtr.Zero) {
                        StringBuilder pszPath = new StringBuilder(260);
                        if(PInvoke.SHGetPathFromIDList(pidl, pszPath)) {
                            str = pszPath.ToString();
                        }
                        PInvoke.CoTaskMemFree(pidl);
                    }
                    return str;
                }
                return address.Path;
            }

            private static IntPtr AddressToPIDL(Address address) {
                IntPtr zero = IntPtr.Zero;
                if((address.ITEMIDLIST != null) && (address.ITEMIDLIST.Length != 0)) {
                    return ShellMethods.CreateIDL(address.ITEMIDLIST);
                }
                if((address.Path != null) && (address.Path.Length != 0)) {
                    zero = PInvoke.ILCreateFromPath(address.Path);
                }
                return zero;
            }

            internal void ClearEvents() {
                this.TabChanged = null;
                this.TabAdded = null;
                this.TabRemoved = null;
                this.NavigationComplete = null;
                this.SelectionChanged = null;
                this.ExplorerStateChanged = null;
                this.SettingsChanged = null;
                this.MouseEnter = null;
                this.PointedTabChanged = null;
                this.MouseLeave = null;
                this.MenuRendererChanged = null;
            }

            public bool CreateTab(Address address, int index, bool fLocked, bool fSelect) {
                address.ITEMIDLIST = AddressToIDL(address);
                address.Path = AddressToPath(address);
                if((address.ITEMIDLIST == null) || (address.ITEMIDLIST.Length <= 0)) {
                    return false;
                }
                QTabItem tab = new QTabItem(QTUtility2.MakePathDisplayText(address.Path, false), address.Path, this.tabBar.tabControl1);
                tab.NavigatedTo(address.Path, address.ITEMIDLIST, -1);
                tab.ToolTipText = QTUtility2.MakePathDisplayText(address.Path, true);
                tab.TabLocked = fLocked;
                if(index < 0) {
                    this.tabBar.AddInsertTab(tab);
                }
                else {
                    if(index > this.tabBar.tabControl1.TabCount) {
                        index = this.tabBar.tabControl1.TabCount;
                    }
                    this.tabBar.tabControl1.TabPages.Insert(index, tab);
                }
                if(fSelect) {
                    this.tabBar.tabControl1.SelectTab(tab);
                }
                return true;
            }

            public bool CreateWindow(Address address) {
                using(IDLWrapper wrapper = new IDLWrapper(AddressToPIDL(address))) {
                    if(wrapper.Available) {
                        this.tabBar.OpenNewWindow(wrapper);
                        return true;
                    }
                }
                return false;
            }

            public void Dispose() {
                this.tabBar = null;
                this.pluginManager = null;
                this.shellBrowser = null;
            }

            public bool ExecuteCommand(Commands command, object arg) {
                if(this.tabBar != null) {
                    IntPtr ptr;
                    switch(command) {
                        case Commands.GoBack:
                        case Commands.GoForward:
                            if(arg is int) {
                                return this.tabBar.NavigateToIndex(command == Commands.GoBack, (int)arg);
                            }
                            break;

                        case Commands.GoUpOneLevel:
                            this.tabBar.UpOneLevel();
                            return true;

                        case Commands.RefreshBrowser:
                            this.tabBar.Explorer.Refresh();
                            return true;

                        case Commands.CloseCurrentTab:
                            return this.tabBar.CloseTab(this.tabBar.CurrentTab);

                        case Commands.CloseLeft:
                        case Commands.CloseRight:
                            this.tabBar.CloseLeftRight(command == Commands.CloseLeft, -1);
                            return true;

                        case Commands.CloseAllButCurrent:
                            if(this.tabBar.tabControl1.TabCount > 1) {
                                foreach(QTabItem item in this.tabBar.tabControl1.TabPages) {
                                    if(item != this.tabBar.CurrentTab) {
                                        this.tabBar.CloseTab(item);
                                    }
                                }
                            }
                            return true;

                        case Commands.CloseAllButOne: {
                                TabWrapper wrapper = arg as TabWrapper;
                                if(wrapper == null) {
                                    break;
                                }
                                foreach(QTabItem item2 in this.tabBar.tabControl1.TabPages) {
                                    if(item2 != wrapper.Tab) {
                                        this.tabBar.CloseTab(item2);
                                    }
                                }
                                return true;
                            }
                        case Commands.CloseWindow:
                            WindowUtils.CloseExplorer(this.tabBar.ExplorerHandle, 2);
                            return true;

                        case Commands.UndoClose:
                            this.tabBar.RestoreNearest();
                            return true;

                        case Commands.BrowseFolder:
                            this.tabBar.ChooseNewDirectory();
                            return true;

                        case Commands.ToggleTopMost:
                            this.tabBar.ToggleTopMost();
                            this.tabBar.SyncButtonBarCurrent(0x40);
                            return true;

                        case Commands.FocusFileList:
                            this.tabBar.listViewWrapper.SetFocus();
                            return true;

                        case Commands.OpenTabBarOptionDialog:
                            this.tabBar.OpenOptionsDialog();
                            return true;

                        case Commands.OpenButtonBarOptionDialog:
                            if(!QTUtility.instanceManager.TryGetButtonBarHandle(this.tabBar.ExplorerHandle, out ptr)) {
                                break;
                            }
                            QTUtility2.SendCOPYDATASTRUCT(ptr, (IntPtr)12, "showop", IntPtr.Zero);
                            return true;

                        case Commands.IsFolderTreeVisible:
                            return this.tabBar.IsFolderTreeVisible();

                        case Commands.IsButtonBarVisible:
                            return QTUtility.instanceManager.TryGetButtonBarHandle(this.tabBar.ExplorerHandle, out ptr);

                        case Commands.ShowFolderTree:
                            if(QTUtility.IsVista || !(arg is bool)) {
                                break;
                            }
                            this.tabBar.ShowFolderTree((bool)arg);
                            return true;

                        case Commands.ShowButtonBar:
                            if(!QTUtility.instanceManager.TryGetButtonBarHandle(this.tabBar.ExplorerHandle, out ptr)) {
                            }
                            break;

                        case Commands.MD5:
                            if(!(arg is string[])) {
                                break;
                            }
                            if(QTTabBarClass.md5Form == null) {
                                QTTabBarClass.md5Form = new FileHashComputerForm();
                            }
                            if(QTTabBarClass.md5Form.InvokeRequired) {
                                QTTabBarClass.md5Form.Invoke(new FormMethodInvoker(QTTabBarClass.ShowMD5FormCore), new object[] { arg });
                            }
                            else {
                                QTTabBarClass.ShowMD5FormCore(arg);
                            }
                            return true;

                        case Commands.ShowProperties: {
                                if((arg == null) || !(arg is Address)) {
                                    break;
                                }
                                Address address = (Address)arg;
                                IntPtr pv = AddressToPIDL(address);
                                if(!(pv != IntPtr.Zero)) {
                                    break;
                                }
                                SHELLEXECUTEINFO structure = new SHELLEXECUTEINFO();
                                structure.cbSize = Marshal.SizeOf(structure);
                                structure.hwnd = this.tabBar.ExplorerHandle;
                                structure.fMask = 0x40c;
                                structure.lpVerb = Marshal.StringToHGlobalUni("properties");
                                structure.lpIDList = pv;
                                PInvoke.ShellExecuteEx(ref structure);
                                PInvoke.CoTaskMemFree(pv);
                                if(structure.lpVerb != IntPtr.Zero) {
                                    Marshal.FreeHGlobal(structure.lpVerb);
                                }
                                return true;
                            }
                        case Commands.SetModalState:
                            if(((arg == null) || !(arg is bool)) || !((bool)arg)) {
                                this.tabBar.NowModalDialogShown = false;
                                break;
                            }
                            this.tabBar.NowModalDialogShown = true;
                            break;

                        case Commands.SetSearchBoxStr:
                            if(((arg == null) || !(arg is string)) || !QTUtility.instanceManager.TryGetButtonBarHandle(this.tabBar.ExplorerHandle, out ptr)) {
                                break;
                            }
                            return (IntPtr.Zero == QTUtility2.SendCOPYDATASTRUCT(ptr, (IntPtr)0x10, (string)arg, IntPtr.Zero));

                        case Commands.ReorderTabsByName:
                        case Commands.ReorderTabsByPath:
                        case Commands.ReorderTabsByActv:
                        case Commands.ReorderTabsRevers:
                            if(this.tabBar.tabControl1.TabCount > 1) {
                                bool fDescending = ((arg != null) && (arg is bool)) && ((bool)arg);
                                this.tabBar.ReorderTab(((int)command) - 0x18, fDescending);
                            }
                            break;
                    }
                }
                return false;
            }

            public ProcessStartInfo[] GetApplications(string name) {
                return null;
            }

            public string[] GetGroupPaths(string groupName) {
                string str;
                if(QTUtility.GroupPathsDic.TryGetValue(groupName, out str)) {
                    return str.Split(QTUtility.SEPARATOR_CHAR);
                }
                return null;
            }

            public ToolStripRenderer GetMenuRenderer() {
                return DropDownMenuBase.CurrentRenderer;
            }

            public ITab[] GetTabs() {
                List<TabWrapper> list = new List<TabWrapper>();
                foreach(QTabItem item in this.tabBar.tabControl1.TabPages) {
                    list.Add(new TabWrapper(item, this.tabBar));
                }
                return list.ToArray();
            }

            public ITab HitTest(Point pnt) {
                QTabItem tabMouseOn = (QTabItem)this.tabBar.tabControl1.GetTabMouseOn();
                if(tabMouseOn != null) {
                    return new TabWrapper(tabMouseOn, this.tabBar);
                }
                return null;
            }

            public void OnExplorerStateChanged(ExplorerWindowActions windowAction) {
                if(this.ExplorerStateChanged != null) {
                    this.ExplorerStateChanged(this, new PluginEventArgs(windowAction));
                }
            }

            public void OnMenuRendererChanged() {
                if(this.MenuRendererChanged != null) {
                    this.MenuRendererChanged(this, EventArgs.Empty);
                }
            }

            public void OnMouseEnter() {
                if(this.MouseEnter != null) {
                    this.MouseEnter(this, EventArgs.Empty);
                }
            }

            public void OnMouseLeave() {
                if(this.MouseLeave != null) {
                    this.MouseLeave(this, EventArgs.Empty);
                }
            }

            public void OnNavigationComplete(int index, byte[] idl, string path) {
                if(this.NavigationComplete != null) {
                    this.NavigationComplete(this, new PluginEventArgs(index, new Address(idl, path)));
                }
            }

            public void OnPointedTabChanged(int index, byte[] idl, string path) {
                if(this.PointedTabChanged != null) {
                    this.PointedTabChanged(this, new PluginEventArgs(index, new Address(idl, path)));
                }
            }

            public void OnSelectionChanged(int index, byte[] idl, string path) {
                if(this.SelectionChanged != null) {
                    this.SelectionChanged(this, new PluginEventArgs(index, new Address(idl, path)));
                }
            }

            public void OnSettingsChanged(int iType) {
                if(this.SettingsChanged != null) {
                    this.SettingsChanged(this, new PluginEventArgs(iType, new Address()));
                }
            }

            public void OnTabAdded(int index, byte[] idl, string path) {
                if(this.TabAdded != null) {
                    this.TabAdded(this, new PluginEventArgs(index, new Address(idl, path)));
                }
            }

            public void OnTabChanged(int index, byte[] idl, string path) {
                if(this.TabChanged != null) {
                    this.TabChanged(this, new PluginEventArgs(index, new Address(idl, path)));
                }
            }

            public void OnTabRemoved(int index, byte[] idl, string path) {
                if(this.TabRemoved != null) {
                    this.TabRemoved(this, new PluginEventArgs(index, new Address(idl, path)));
                }
            }

            public void OpenGroup(string[] groupNames) {
                foreach(string str in groupNames) {
                    this.tabBar.OpenGroup(str, false);
                }
            }

            public bool OpenPlugin(IPluginClient pluginClient, out string[] shortcutActions) {
                pluginClient.Open(this, this.shellBrowser);
                return pluginClient.QueryShortcutKeys(out shortcutActions);
            }

            public void RegisterMenu(IPluginClient pluginClient, MenuType menuType, string menuText, bool fRegister) {
                this.pluginManager.RegisterMenu(pluginClient, menuType, menuText, fRegister);
            }

            public bool RemoveApplication(string name) {
                return false;
            }

            internal void RemoveEvents(IPluginClient pluginClient) {
                if(this.TabChanged != null) {
                    foreach(PluginEventHandler handler in this.TabChanged.GetInvocationList()) {
                        if(handler.Target == pluginClient) {
                            this.TabChanged = (PluginEventHandler)Delegate.Remove(this.TabChanged, handler);
                        }
                    }
                }
                if(this.TabAdded != null) {
                    foreach(PluginEventHandler handler2 in this.TabAdded.GetInvocationList()) {
                        if(handler2.Target == pluginClient) {
                            this.TabAdded = (PluginEventHandler)Delegate.Remove(this.TabAdded, handler2);
                        }
                    }
                }
                if(this.TabRemoved != null) {
                    foreach(PluginEventHandler handler3 in this.TabRemoved.GetInvocationList()) {
                        if(handler3.Target == pluginClient) {
                            this.TabRemoved = (PluginEventHandler)Delegate.Remove(this.TabRemoved, handler3);
                        }
                    }
                }
                if(this.NavigationComplete != null) {
                    foreach(PluginEventHandler handler4 in this.NavigationComplete.GetInvocationList()) {
                        if(handler4.Target == pluginClient) {
                            this.NavigationComplete = (PluginEventHandler)Delegate.Remove(this.NavigationComplete, handler4);
                        }
                    }
                }
                if(this.SelectionChanged != null) {
                    foreach(PluginEventHandler handler5 in this.SelectionChanged.GetInvocationList()) {
                        if(handler5.Target == pluginClient) {
                            this.SelectionChanged = (PluginEventHandler)Delegate.Remove(this.SelectionChanged, handler5);
                        }
                    }
                }
                if(this.ExplorerStateChanged != null) {
                    foreach(PluginEventHandler handler6 in this.ExplorerStateChanged.GetInvocationList()) {
                        if(handler6.Target == pluginClient) {
                            this.ExplorerStateChanged = (PluginEventHandler)Delegate.Remove(this.ExplorerStateChanged, handler6);
                        }
                    }
                }
                if(this.SettingsChanged != null) {
                    foreach(PluginEventHandler handler7 in this.SettingsChanged.GetInvocationList()) {
                        if(handler7.Target == pluginClient) {
                            this.SettingsChanged = (PluginEventHandler)Delegate.Remove(this.SettingsChanged, handler7);
                        }
                    }
                }
                if(this.MouseEnter != null) {
                    foreach(EventHandler handler8 in this.MouseEnter.GetInvocationList()) {
                        if(handler8.Target == pluginClient) {
                            this.MouseEnter = (EventHandler)Delegate.Remove(this.MouseEnter, handler8);
                        }
                    }
                }
                if(this.PointedTabChanged != null) {
                    foreach(PluginEventHandler handler9 in this.PointedTabChanged.GetInvocationList()) {
                        if(handler9.Target == pluginClient) {
                            this.PointedTabChanged = (PluginEventHandler)Delegate.Remove(this.PointedTabChanged, handler9);
                        }
                    }
                }
                if(this.MouseLeave != null) {
                    foreach(EventHandler handler10 in this.MouseLeave.GetInvocationList()) {
                        if(handler10.Target == pluginClient) {
                            this.MouseLeave = (EventHandler)Delegate.Remove(this.MouseLeave, handler10);
                        }
                    }
                }
                if(this.MenuRendererChanged != null) {
                    foreach(EventHandler handler11 in this.MenuRendererChanged.GetInvocationList()) {
                        if(handler11.Target == pluginClient) {
                            this.MenuRendererChanged = (EventHandler)Delegate.Remove(this.MenuRendererChanged, handler11);
                        }
                    }
                }
            }

            public bool RemoveGroup(string groupName) {
                bool flag = QTUtility.GroupPathsDic.Remove(groupName);
                if(flag) {
                    QTUtility.SaveGroupsReg();
                }
                return flag;
            }

            public bool TryGetLocalizedStrings(IPluginClient pluginClient, int count, out string[] arrStrings) {
                string key = this.pluginManager.InstanceToFullName(pluginClient, true);
                if(((key.Length > 0) && this.dicLocalizingStrings.TryGetValue(key, out arrStrings)) && ((arrStrings != null) && (arrStrings.Length == count))) {
                    return true;
                }
                arrStrings = null;
                return false;
            }

            public bool TryGetSelection(out Address[] adSelectedItems) {
                string str;
                return this.tabBar.TryGetSelection(out adSelectedItems, out str, false);
            }

            public bool TrySetSelection(Address[] itemsToSelect, bool fDeselectOthers) {
                return this.tabBar.TrySetSelection(itemsToSelect, null, fDeselectOthers);
            }

            public void UpdateItem(IBarButton barItem, bool fEnabled, bool fRefreshImage) {
                IntPtr ptr;
                string strMsg = this.pluginManager.InstanceToFullName(barItem, false);
                if((strMsg.Length > 0) && QTUtility.instanceManager.TryGetButtonBarHandle(this.tabBar.ExplorerHandle, out ptr)) {
                    int num = 0;
                    if(fEnabled) {
                        num |= 1;
                    }
                    if(fRefreshImage) {
                        num |= 2;
                    }
                    QTUtility2.SendCOPYDATASTRUCT(ptr, (IntPtr)11, strMsg, (IntPtr)num);
                }
            }

            public IntPtr ExplorerHandle {
                get {
                    return this.tabBar.ExplorerHandle;
                }
            }

            public string[] Groups {
                get {
                    string[] array = new string[QTUtility.GroupPathsDic.Keys.Count];
                    QTUtility.GroupPathsDic.Keys.CopyTo(array, 0);
                    return array;
                }
            }

            public IntPtr Handle {
                get {
                    if(this.tabBar.IsHandleCreated) {
                        return this.tabBar.Handle;
                    }
                    return IntPtr.Zero;
                }
            }

            public ITab SelectedTab {
                get {
                    if(this.tabBar.CurrentTab != null) {
                        return new TabWrapper(this.tabBar.CurrentTab, this.tabBar);
                    }
                    return null;
                }
                set {
                    TabWrapper wrapper = value as TabWrapper;
                    if((wrapper.Tab != null) && this.tabBar.tabControl1.TabPages.Contains(wrapper.Tab)) {
                        this.tabBar.tabControl1.SelectTab(wrapper.Tab);
                    }
                }
            }

            public bool SelectionChangedAttached {
                get {
                    return (this.SelectionChanged != null);
                }
            }

            public QTPlugin.TabBarOption TabBarOption {
                get {
                    return QTUtility.GetTabBarOption();
                }
                set {
                    QTUtility.SetTabBarOption(value, this.tabBar);
                }
            }

            internal sealed class TabWrapper : ITab {
                private QTabItem tab;
                private QTTabBarClass tabBar;

                public TabWrapper(QTabItem tab, QTTabBarClass tabBar) {
                    this.tab = tab;
                    this.tabBar = tabBar;
                    this.tab.Closed += new EventHandler(this.tab_Closed);
                }

                public bool Browse(QTPlugin.Address address) {
                    if(this.tab != null) {
                        this.tabBar.tabControl1.SelectTab(this.tab);
                        IntPtr pidl = QTTabBarClass.PluginServer.AddressToPIDL(address);
                        if(pidl != IntPtr.Zero) {
                            int num = this.tabBar.ShellBrowser.BrowseObject(pidl, 1);
                            PInvoke.CoTaskMemFree(pidl);
                            return (num == 0);
                        }
                    }
                    return false;
                }

                public bool Browse(bool fBack) {
                    if(this.tab != null) {
                        this.tabBar.tabControl1.SelectTab(this.tab);
                        return this.tabBar.NavigateCurrentTab(fBack);
                    }
                    return false;
                }

                public void Clone(int index, bool fSelect) {
                    if(this.tab != null) {
                        this.tabBar.CloneTabButton(this.tab, null, fSelect, index);
                    }
                }

                public bool Close() {
                    return (((this.tab != null) && (this.tabBar.tabControl1.TabCount > 1)) && this.tabBar.CloseTab(this.tab, true));
                }

                public QTPlugin.Address[] GetBraches() {
                    if(this.tab == null) {
                        return null;
                    }
                    List<LogData> branches = this.tab.Branches;
                    List<QTPlugin.Address> list2 = new List<QTPlugin.Address>();
                    foreach(LogData data in branches) {
                        if((data.IDL != null) || !string.IsNullOrEmpty(data.Path)) {
                            list2.Add(new QTPlugin.Address(data.IDL, data.Path));
                        }
                    }
                    return list2.ToArray();
                }

                public QTPlugin.Address[] GetHistory(bool fBack) {
                    if(this.tab == null) {
                        return null;
                    }
                    LogData[] logs = this.tab.GetLogs(fBack);
                    List<QTPlugin.Address> list = new List<QTPlugin.Address>();
                    foreach(LogData data in logs) {
                        list.Add(new QTPlugin.Address(data.IDL, data.Path));
                    }
                    return list.ToArray();
                }

                public bool Insert(int index) {
                    if(((this.tab != null) && (-1 < index)) && (index < (this.tabBar.tabControl1.TabCount + 1))) {
                        int indexSource = this.tabBar.tabControl1.TabPages.IndexOf(this.tab);
                        if(indexSource > -1) {
                            this.tabBar.tabControl1.TabPages.Swap(indexSource, index);
                            return true;
                        }
                    }
                    return false;
                }

                private void tab_Closed(object sender, EventArgs e) {
                    this.tab.Closed -= new EventHandler(this.tab_Closed);
                    this.tab = null;
                    this.tabBar = null;
                }

                public QTPlugin.Address Address {
                    get {
                        if(this.tab == null) {
                            return new QTPlugin.Address();
                        }
                        QTPlugin.Address address = new QTPlugin.Address(this.tab.CurrentIDL, this.tab.CurrentPath);
                        if((address.ITEMIDLIST == null) && !string.IsNullOrEmpty(address.Path)) {
                            IDLWrapper wrapper;
                            IntPtr pidl = PInvoke.ILCreateFromPath(address.Path);
                            if(pidl != IntPtr.Zero) {
                                address = new QTPlugin.Address(pidl, this.tab.CurrentPath);
                                PInvoke.CoTaskMemFree(pidl);
                                return address;
                            }
                            if(!IDLWrapper.TryGetCache(address.Path, out wrapper)) {
                                return address;
                            }
                            using(wrapper) {
                                address.ITEMIDLIST = wrapper.IDL;
                            }
                        }
                        return address;
                    }
                }

                public int Index {
                    get {
                        if(this.tab != null) {
                            return this.tabBar.tabControl1.TabPages.IndexOf(this.tab);
                        }
                        return -1;
                    }
                }

                public bool Locked {
                    get {
                        return ((this.tab != null) && this.tab.TabLocked);
                    }
                    set {
                        if(this.tab != null) {
                            this.tab.TabLocked = value;
                            this.tabBar.tabControl1.Refresh();
                        }
                    }
                }

                public bool Selected {
                    get {
                        return ((this.tab != null) && (this.tabBar.CurrentTab == this.tab));
                    }
                    set {
                        if((this.tab != null) && value) {
                            this.tabBar.tabControl1.SelectTab(this.tab);
                        }
                    }
                }

                public string SubText {
                    get {
                        if(this.tab != null) {
                            return this.tab.Comment;
                        }
                        return string.Empty;
                    }
                    set {
                        if((this.tab != null) && (value != null)) {
                            this.tab.Comment = value;
                            this.tab.RefreshRectangle();
                            this.tabBar.tabControl1.Refresh();
                        }
                    }
                }

                public QTabItem Tab {
                    get {
                        return this.tab;
                    }
                }

                public string Text {
                    get {
                        if(this.tab != null) {
                            return this.tab.Text;
                        }
                        return string.Empty;
                    }
                    set {
                        if(((this.tab != null) && (value != null)) && (value.Length > 0)) {
                            this.tab.Text = value;
                        }
                    }
                }
            }
        }
    }
}
