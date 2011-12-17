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
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using BandObjectLib;
using Microsoft.Win32;
using QTPlugin;
using QTTabBarLib.Interop;
using SHDocVw;
using Timer = System.Windows.Forms.Timer;

namespace QTTabBarLib {
    [ComVisible(true), Guid("d2bf470e-ed1c-487f-a333-2bd8835eb6ce")]
    public sealed class QTTabBarClass : BandObject {
        private int BandHeight;
        private VisualStyleRenderer bgRenderer;
        private BreadcrumbBar breadcrumbBar;
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
        private static bool fInitialized;
        private readonly bool fIsFirstLoad;
        private volatile bool FirstNavigationCompleted;
        private bool fAutoNavigating;
        private bool fNavigatedByTabSelection;
        private bool fNeedsNewWindowPulse;
        private bool fNowInTray;
        private bool fNowQuitting;
        private bool fNowRestoring;
        private bool fNowTravelByTree;
        private bool fToggleTabMenu;
        private IntPtr hHook_Key;
        private IntPtr hHook_Mouse;
        private IntPtr hHook_Msg;
        private HookProc hookProc_GetMsg;
        private HookProc hookProc_Key;
        private HookProc hookProc_Mouse;
        private static IntPtr hwndNotifyIconParent;
        private static Icon icoNotify;
        private IContextMenu2 iContextMenu2;
        private int iModKeyStateDD;
        private const int INTERVAL_SELCTTAB = 700;
        private const int INTERVAL_SHOWMENU = 0x4b0;
        private int iSequential_WM_CLOSE;
        private bool IsShown;
        private byte[] lastAttemptedBrowseObjectIDL;
        private byte[] lastCompletedBrowseObjectIDL;
        private Dictionary<int, ITravelLogEntry> LogEntryDic = new Dictionary<int, ITravelLogEntry>();
        private AbstractListView listView = new AbstractListView();
        private ListViewMonitor listViewManager;
        private List<QTabItem> lstActivatedTabs = new List<QTabItem>(0x10);
        private List<ToolStripItem> lstPluginMenuItems_Sys;
        private List<ToolStripItem> lstPluginMenuItems_Tab;
        private static List<PluginAssembly> lstTempAssemblies_Refresh;
        private static FileHashComputerForm md5Form;
        private ToolStripTextBox menuTextBoxTabAlias;
        private int navBtnsFlag;
        private bool NavigatedByCode;
        private static NotifyIcon notifyIcon;
        private bool NowInTravelLog;
        private bool NowModalDialogShown;
        private bool NowOpenedByGroupOpener;
        private bool NowTabCloned;
        private bool NowTabCreated;
        private bool NowTabDragging;
        private bool NowTabsAddingRemoving;
        private bool NowTopMost;
        private PluginManager pluginManager;
        internal RebarController rebarController;
        private ShellBrowserEx ShellBrowser;
        private string strDraggingDrive;
        private string strDraggingStartPath;
        private SubDirTipForm subDirTip_Tab;
        private static object syncObj_NotifyIcon = new object();
        private QTabControl tabControl1;
        private QTabItem tabForDD;
        private TabSwitchForm tabSwitcher;
        private Timer timerOnTab;
        private Timer timerSelectionChanged;
        private ToolStripEx toolStrip;
        private ToolTip toolTipForDD;
        private NativeWindowController travelBtnController;
        private ITravelLogStg TravelLog;
        private IntPtr TravelToolBarHandle;
        private TreeViewWrapper treeViewWrapper;
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
        private readonly uint WM_NEWTREECONTROL = PInvoke.RegisterWindowMessage("QTTabBar_NewTreeControl");
        private readonly uint WM_BROWSEOBJECT = PInvoke.RegisterWindowMessage("QTTabBar_BrowseObject");
        private readonly uint WM_HEADERINALLVIEWS = PInvoke.RegisterWindowMessage("QTTabBar_HeaderInAllViews");
        private readonly uint WM_LISTREFRESHED = PInvoke.RegisterWindowMessage("QTTabBar_ListRefreshed");
        private readonly uint WM_SHOWHIDEBARS = PInvoke.RegisterWindowMessage("QTTabBar_ShowHideBars");
        private readonly uint WM_NEWWINDOW = PInvoke.RegisterWindowMessage("QTTabBar_NewWindow");

        
        // TODO: group delegates
        public delegate bool FolderClickedHandler(IDLWrapper item, Keys modkeys, bool middle);

        public QTTabBarClass() {
            try {
                string installDateString;
                DateTime installDate;
                string minDate = DateTime.MinValue.ToString();
                using(RegistryKey key = Registry.LocalMachine.OpenSubKey(RegConst.Root)) {
                    installDateString = key == null ? minDate : (string)key.GetValue("InstallDate", minDate);
                    installDate = DateTime.Parse(installDateString);
                }
                using(RegistryKey key = Registry.CurrentUser.CreateSubKey(RegConst.Root)) {
                    DateTime lastActivation = DateTime.Parse((string)key.GetValue("ActivationDate", minDate));
                    fIsFirstLoad = installDate.CompareTo(lastActivation) > 0;
                    if(fIsFirstLoad) key.SetValue("ActivationDate", installDateString);
                }
            }
            catch {
            }
            if(!fInitialized) {
                InitializeStaticFields();
            }
            QTUtility.InstancesCount++;
            BandHeight = Config.Skin.TabHeight + 2;
            InitializeComponent();
            lstActivatedTabs.Add(CurrentTab);
        }

        private void AddInsertTab(QTabItem tab) {
            switch(Config.Tabs.NewTabPosition) {
                case TabPos.Leftmost:
                    tabControl1.TabPages.Insert(0, tab);
                    break;

                case TabPos.Right:
                case TabPos.Left: {
                    int index = tabControl1.TabPages.IndexOf(CurrentTab);
                    if(index == -1) {
                        tabControl1.TabPages.Add(tab);
                    }
                    else {
                        tabControl1.TabPages.Insert(Config.Tabs.NewTabPosition == TabPos.Right ? (index + 1) : index, tab);    
                    }
                    break;
                }

                default: // TabPos.Rightmost
                    tabControl1.TabPages.Add(tab);
                    break;
            }
        }

        private void AddStartUpTabs(string openingGRP, string openingPath) {
            if(ModifierKeys == Keys.Shift || QTUtility.InstancesCount != 1) return;
            foreach(string path in GroupsManager.Groups.Where(g => g.Startup && openingGRP != g.Name).SelectMany(g => g.Paths)) {
                if(Config.Tabs.NeverOpenSame) {
                    if(path.PathEquals(openingPath)) {
                        tabControl1.TabPages.Relocate(0, tabControl1.TabCount - 1);
                        continue;
                    }
                    if(tabControl1.TabPages.Cast<QTabItem>().Any(item => path.PathEquals(item.CurrentPath))) {
                        continue;
                    }
                }
                using(IDLWrapper wrapper = new IDLWrapper(path)) {
                    if(!wrapper.Available) continue;
                    QTabItem tabPage = new QTabItem(QTUtility2.MakePathDisplayText(path, false), path, tabControl1);
                    tabPage.NavigatedTo(path, wrapper.IDL, -1, false);
                    tabPage.ToolTipText = QTUtility2.MakePathDisplayText(path, true);
                    tabPage.UnderLine = true;
                    tabControl1.TabPages.Add(tabPage);
                }
            }
            if(Config.Window.RestoreOnlyLocked) {
                RestoreTabsOnInitialize(1, openingPath);
            }
            else if(Config.Window.RestoreSession || fIsFirstLoad) {
                RestoreTabsOnInitialize(0, openingPath);
            }
        }

        private static void AddToHistory(QTabItem closingTab) {
            string currentPath = closingTab.CurrentPath;
            if((Config.Misc.KeepHistory && !string.IsNullOrEmpty(currentPath)) && !IsSearchResultFolder(currentPath)) {
                if(QTUtility2.IsShellPathButNotFileSystem(currentPath) && (currentPath.IndexOf("???") == -1)) {
                    currentPath = currentPath + "???" + closingTab.GetLogHash(true, 0);
                }
                QTUtility.ClosedTabHistoryList.Add(currentPath);
                SyncButtonBarBroadCast(2);
            }
        }

        private void AppendUserApps(List<string> listDroppedPaths) {
            WindowUtils.BringExplorerToFront(ExplorerHandle);
            if(contextMenuDropped == null) {
                ToolStripMenuItem item = new ToolStripMenuItem();
                item.Tag = 1;
                contextMenuDropped = new ContextMenuStripEx(components, false);
                contextMenuDropped.SuspendLayout();
                contextMenuDropped.Items.Add(item);
                contextMenuDropped.Items.Add(new ToolStripMenuItem());
                contextMenuDropped.ShowImageMargin = false;
                contextMenuDropped.ItemClicked += contextMenuDropped_ItemClicked;
                contextMenuDropped.ResumeLayout(false);
            }
            string str = QTUtility.ResMain[0x15];
            str = str + ((listDroppedPaths.Count > 1) ? (listDroppedPaths.Count + QTUtility.ResMain[0x16]) : ("\"" + Path.GetFileName(listDroppedPaths[0]) + "\""));
            contextMenuDropped.SuspendLayout();
            contextMenuDropped.Items[0].Text = str;
            contextMenuDropped.Items[1].Text = QTUtility.ResMain[0x17];
            contextMenuDropped.Tag = listDroppedPaths;
            contextMenuDropped.ResumeLayout();
            contextMenuDropped.Show(MousePosition);
        }

        // TODO: Kill all of these.
        private void AsyncComplete(IAsyncResult ar) {
            AsyncResult result = (AsyncResult)ar;
            ((MethodInvoker)result.AsyncDelegate).EndInvoke(ar);
            if(IsHandleCreated) {
                Invoke(new NavigationCompleteCallback(CallBackDoOpenGroups), new object[] { result.AsyncState, IntPtr.Zero });
            }
        }

        private void AsyncComplete_ButtonBarPlugin(IAsyncResult ar) {
            AsyncResult result = (AsyncResult)ar;
            ((WaitTimeoutCallback)result.AsyncDelegate).EndInvoke(ar);
            if(IsHandleCreated) {
                Invoke(new MethodInvoker(CallbackPlugin));
            }
        }

        private void AsyncComplete_FolderTree(IAsyncResult ar) {
            AsyncResult result = (AsyncResult)ar;
            ((WaitTimeoutCallback)result.AsyncDelegate).EndInvoke(ar);
            if(IsHandleCreated) {
                Invoke(new FormMethodInvoker(CallbackFolderTree), new object[] { result.AsyncState });
            }
        }

        private void AsyncComplete_MultiPath(IAsyncResult ar) {
            AsyncResult result = (AsyncResult)ar;
            ((MethodInvoker)result.AsyncDelegate).EndInvoke(ar);
            if(IsHandleCreated) {
                Invoke(new FormMethodInvoker(CallbackMultiPath), new object[] { result.AsyncState });
            }
        }

        // This function is used as a more available version of BeforeNavigate2.
        // Return true to suppress the navigation.  Target IDL should not be relied
        // upon; it's not guaranteed to be accurate.
        private bool BeforeNavigate(IDLWrapper target, bool autonav) {
            if(!IsShown) return false;
            HideSubDirTip_Tab_Menu();
            NowTabDragging = false;
            fAutoNavigating = autonav;
            if(!NavigatedByCode) {
                SaveSelectedItems(CurrentTab);
            }
            if(NowInTravelLog) {
                if(CurrentTravelLogIndex > 0) {
                    CurrentTravelLogIndex--;
                    if(!IsSpecialFolderNeedsToTravel(target.Path)) {
                        NavigateBackToTheFuture();
                    }
                }
                else {
                    NowInTravelLog = false;
                }
            }
            lastAttemptedBrowseObjectIDL = target.IDL;
            return false;
        }

        private void CallBackDoOpenGroups(object obj, IntPtr ptr) {
            string[] strArray = (string[])obj;
            tabControl1.SetRedraw(false);
            foreach(string str in strArray) {
                OpenGroup(str, false);
            }
            tabControl1.SetRedraw(true);
            RestoreFromTray();
        }

        private void CallbackFirstNavComp() {
            int num = 0;
            while(!FirstNavigationCompleted) {
                Thread.Sleep(100);
                if(++num > 100) {
                    return;
                }
            }
        }

        private void CallbackFolderTree(object obj) {
            bool fShow = (bool)obj;
            ShowFolderTree(fShow);
            if(fShow) {
                PInvoke.SetRedraw(ExplorerHandle, true);
                PInvoke.RedrawWindow(ExplorerHandle, IntPtr.Zero, IntPtr.Zero, 0x289);
            }
        }

        private IntPtr CallbackGetMsgProc(int nCode, IntPtr wParam, IntPtr lParam) {
            if(nCode >= 0) {
                MSG msg = (MSG)Marshal.PtrToStructure(lParam, typeof(MSG));
                try {
                    if(QTUtility.IsXP) {
                        if(msg.message == WM.CLOSE) {
                            if(iSequential_WM_CLOSE > 0) {
                                Marshal.StructureToPtr(new MSG(), lParam, false);
                                return PInvoke.CallNextHookEx(hHook_Msg, nCode, wParam, lParam);
                            }
                            iSequential_WM_CLOSE++;
                        }
                        else {
                            iSequential_WM_CLOSE = 0;
                        }
                    }

                    if(msg.message == WM_NEWTREECONTROL) {
                        object obj = Marshal.GetObjectForIUnknown(msg.wParam);
                        try {
                            if(obj != null) {
                                IOleWindow window = obj as IOleWindow;
                                if(window != null) {
                                    IntPtr hwnd;
                                    window.GetWindow(out hwnd);
                                    if(hwnd != IntPtr.Zero && PInvoke.IsChild(ExplorerHandle, hwnd)) {
                                        hwnd = WindowUtils.FindChildWindow(hwnd,
                                                child => PInvoke.GetClassName(child) == "SysTreeView32");
                                        if(hwnd != IntPtr.Zero) {
                                            INameSpaceTreeControl control = obj as INameSpaceTreeControl;
                                            if(control != null) {
                                                if(treeViewWrapper != null) {
                                                    treeViewWrapper.Dispose();
                                                }
                                                treeViewWrapper = new TreeViewWrapper(hwnd, control);
                                                treeViewWrapper.TreeViewClicked += FolderLinkClicked;
                                                obj = null; // Release the object only if we didn't get this far.
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        finally {
                            if(obj != null) {
                                Marshal.ReleaseComObject(obj);
                            }
                        }
                        return PInvoke.CallNextHookEx(hHook_Msg, nCode, wParam, lParam);   
                    }
                    else if(msg.message == WM_LISTREFRESHED) {
                        HandleF5();
                        return PInvoke.CallNextHookEx(hHook_Msg, nCode, wParam, lParam);   
                    }

                    switch(msg.message) {
                        case WM.LBUTTONDOWN:
                        case WM.LBUTTONUP:
                            if((QTUtility.IsXP && !Config.NoMidClickTree) && ((((int)((long)msg.wParam)) & 4) != 0)) {
                                HandleLBUTTON_Tree(msg, msg.message == 0x201);
                            }
                            break;

                        case WM.MBUTTONUP:
                            if(QTUtility.IsXP && !Explorer.Busy && !Config.NoMidClickTree) {
                                Handle_MButtonUp_Tree(msg);
                            }
                            break;

                        case WM.CLOSE:
                            if(QTUtility.IsXP) {
                                if((msg.hwnd == ExplorerHandle) && HandleCLOSE(msg.lParam)) {
                                    Marshal.StructureToPtr(new MSG(), lParam, false);
                                }
                                break;
                            }
                            if(msg.hwnd == WindowUtils.GetShellTabWindowClass(ExplorerHandle)) {
                                try {
                                    bool flag = tabControl1.TabCount == 1;
                                    string currentPath = ((QTabItem)tabControl1.SelectedTab).CurrentPath;
                                    if(!Directory.Exists(currentPath) && currentPath.Length > 3 /* && currentPath.Substring(1, 2) == @":\" */ ) {
                                        if(flag) {
                                            WindowUtils.CloseExplorer(ExplorerHandle, 2);
                                        }
                                        else {
                                            CloseTab((QTabItem)tabControl1.SelectedTab, true);
                                        }
                                    }
                                }
                                catch {
                                }
                                Marshal.StructureToPtr(new MSG(), lParam, false);
                            }
                            break;

                        case WM.COMMAND:
                            if(QTUtility.IsXP) {
                                int num = ((int)((long)msg.wParam)) & 0xffff;
                                if(num == 0xa021) {
                                    WindowUtils.CloseExplorer(ExplorerHandle, 3);
                                    Marshal.StructureToPtr(new MSG(), lParam, false);
                                }
                            }
                            break;
                    }
                }
                catch(Exception ex) {
                    QTUtility2.MakeErrorLog(ex, String.Format("Message: {0:x4}", msg.message));
                }
            }
            return PInvoke.CallNextHookEx(hHook_Msg, nCode, wParam, lParam);
        }

        private IntPtr CallbackKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam) {
            const uint KB_TRANSITION_FLAG = 0x80000000;
            const uint KB_PREVIOUS_STATE_FLAG = 0x40000000;
            if(nCode < 0 || NowModalDialogShown) {
                return PInvoke.CallNextHookEx(hHook_Key, nCode, wParam, lParam);
            }

            try {
                uint flags = (uint)((long)lParam);
                bool isKeyPress = (flags & KB_TRANSITION_FLAG) == 0;
                bool isRepeat = (flags & KB_PREVIOUS_STATE_FLAG) != 0;
                Keys key = (Keys)((int)wParam);

                if(key == Keys.ShiftKey) {
                    if(isKeyPress || !isRepeat) {
                        listView.HandleShiftKey();
                    }
                }

                if(isKeyPress) {
                    if(HandleKEYDOWN(key, isRepeat)) {
                        return new IntPtr(1);
                    }
                }
                else {
                    listView.HideThumbnailTooltip(3);
                    if(NowTabDragging && DraggingTab != null) {
                        Cursor = Cursors.Default;
                    }

                    switch(key) {
                        case Keys.ShiftKey:
                            if(!Config.NoTabSwitcher) {
                                HideTabSwitcher(true);
                            }
                            break;

                        case Keys.Menu: // Alt key
                            if(Config.Tabs.ShowCloseButtons && Config.Tabs.CloseBtnsWithAlt) {
                                tabControl1.ShowCloseButton(false);
                            }
                            break;

                        case Keys.Tab:
                            if(!Config.NoTabSwitcher && tabSwitcher != null && tabSwitcher.IsShown) {
                                tabControl1.SetPseudoHotIndex(tabSwitcher.SelectedIndex);
                            }
                            break;
                    }
                }
            }
            catch(Exception ex) {
                QTUtility2.MakeErrorLog(ex,
                        String.Format("LParam: {0:x4}, WParam: {1:x4}", (long)lParam, (long)wParam));
            }
            return PInvoke.CallNextHookEx(hHook_Key, nCode, wParam, lParam);
        }

        private IntPtr CallbackMouseProc(int nCode, IntPtr wParam, IntPtr lParam) {
            try {
                if(nCode >= 0 && !NowModalDialogShown) {
                    IntPtr ptr = (IntPtr)1;
                    switch(((int)wParam)) {
                        case WM.MOUSEWHEEL:
                            if(!HandleMOUSEWHEEL(lParam)) {
                                break;
                            }
                            return ptr;

                        case WM.XBUTTONDOWN:
                        case WM.XBUTTONUP:
                            MouseButtons mouseButtons = MouseButtons;
                            Keys modifierKeys = ModifierKeys;
                            MouseChord chord = mouseButtons == MouseButtons.XButton1
                                    ? MouseChord.X1
                                    : mouseButtons == MouseButtons.XButton2 ? MouseChord.X2 : MouseChord.None;
                            if(chord == MouseChord.None) break;
                            chord = QTUtility.MakeMouseChord(chord, modifierKeys);
                            BindAction action;
                            if(!Config.Mouse.GlobalMouseActions.TryGetValue(chord, out action)) {
                                break;
                            }
                            if(((int)wParam) == WM.XBUTTONUP && !Explorer.Busy) {
                                DoBindAction(action);
                            }
                            return ptr;
                    }
                }
            }
            catch(Exception ex) {
                QTUtility2.MakeErrorLog(ex, String.Format("LParam: {0:x4}, WParam: {1:x4}", (long)lParam, (long)wParam));
            }
            return PInvoke.CallNextHookEx(hHook_Mouse, nCode, wParam, lParam);
        }

        private void CallbackMultiPath(object obj) {
            object[] objArray = (object[])obj;
            string[] collection = (string[])objArray[0];
            int num = (int)objArray[1];
            switch(num) {
                case 0:
                    foreach(string str in collection) {
                        OpenNewTab(str, true);
                    }
                    break;

                case 1: {
                        bool flag = true;
                        foreach(string str2 in collection) {
                            OpenNewTab(str2, !flag);
                            flag = false;
                        }
                        break;
                    }
                default:
                    QTUtility.TMPPathList = new List<string>(collection);
                    using(IDLWrapper wrapper = new IDLWrapper(collection[0])) {
                        OpenNewWindow(wrapper);
                    }
                    break;
            }
            if(num == 1) {
                RestoreFromTray();
                WindowUtils.BringExplorerToFront(ExplorerHandle);
            }
        }

        private void CallbackPlugin() {
            SyncButtonBarCurrent(0x100);
        }

        private void CancelFailedNavigation(string failedPath, bool fRollBackForward, int countRollback) {
            ShowMessageNavCanceled(failedPath, false);
            if(fRollBackForward) {
                for(int i = 0; i < countRollback; i++) {
                    CurrentTab.GoForward();
                }
            }
            else {
                for(int j = 0; j < countRollback; j++) {
                    CurrentTab.GoBackward();
                }
            }
            NavigatedByCode = false;
        }

        private void CancelFailedTabChanging(string newPath) {
            if(!CloseTab((QTabItem)tabControl1.SelectedTab, true)) {
                if(tabControl1.TabCount == 1) {
                    WindowUtils.CloseExplorer(ExplorerHandle, 2);
                }
                else {
                    ShowMessageNavCanceled(newPath, false);
                    if(CurrentTab == null) {
                        tabControl1.SelectedIndex = 0;
                    }
                }
            }
            else {
                QTUtility.ClosedTabHistoryList.Remove(newPath);
                if(tabControl1.TabCount == 0) {
                    ShowMessageNavCanceled(newPath, true);
                    WindowUtils.CloseExplorer(ExplorerHandle, 2);
                }
                else {
                    if(CurrentTab == null) {
                        tabControl1.SelectedIndex = 0;
                    }
                    else {
                        tabControl1.SelectTab(CurrentTab);
                    }
                    ShowMessageNavCanceled(newPath, false);
                }
            }
        }

        private void ChangeViewMode(bool fUp) {
            FVM orig = ShellBrowser.ViewMode;
            FVM mode = orig;
            switch(mode) {
                case FVM.ICON:
                    mode = fUp ? FVM.TILE : FVM.LIST;
                    break;

                case FVM.LIST:
                    mode = fUp ? FVM.ICON : FVM.DETAILS;
                    break;

                case FVM.DETAILS:
                    if(fUp) {
                        mode = FVM.LIST;
                    }
                    break;

                case FVM.THUMBNAIL:
                    mode = fUp ? FVM.THUMBSTRIP : FVM.TILE;
                    break;

                case FVM.TILE:
                    mode = fUp ? FVM.THUMBNAIL : FVM.ICON;
                    break;

                case FVM.THUMBSTRIP:
                    if(!fUp) {
                        mode = FVM.THUMBNAIL;
                    }
                    break;
            }
            if(mode != orig) {
                ShellBrowser.ViewMode = mode;
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
            NowModalDialogShown = true;
            bool nowTopMost = NowTopMost;
            if(nowTopMost) {
                ToggleTopMost();
            }
            using(FolderBrowserDialog dialog = new FolderBrowserDialog()) {
                dialog.SelectedPath = CurrentAddress;
                if(DialogResult.OK == dialog.ShowDialog()) {
                    OpenNewTab(dialog.SelectedPath);
                }
            }
            NowModalDialogShown = false;
            if(nowTopMost) {
                ToggleTopMost();
            }
        }

        private void ClearTravelLogs() {
            IEnumTravelLogEntry ppenum = null;
            try {
                if((TravelLog.EnumEntries(0x30, out ppenum) != 0) || (ppenum == null)) {
                    return;
                }
                int num = 0;
            Label_0018:
                ITravelLogEntry entry2 = null;
                try {
                    if(ppenum.Next(1, out entry2, 0) == 0) {
                        IntPtr ptr;
                        if((num++ != 0) && (entry2.GetURL(out ptr) == 0)) {
                            string path = Marshal.PtrToStringUni(ptr);
                            PInvoke.CoTaskMemFree(ptr);
                            if(!IsSpecialFolderNeedsToTravel(path)) {
                                TravelLog.RemoveEntry(entry2);
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
                QTUtility2.MakeErrorLog(exception);
            }
            finally {
                if(ppenum != null) {
                    Marshal.ReleaseComObject(ppenum);
                }
            }
        }

        private void CloneTabButton(QTabItem tab, LogData log) {
            NowTabCloned = true;
            QTabItem item = tab.Clone();
            AddInsertTab(item);
            using(IDLWrapper wrapper = new IDLWrapper(log.IDL)) {
                if(wrapper.Available) {
                    item.NavigatedTo(wrapper.Path, wrapper.IDL, log.Hash, false);
                }
            }
            tabControl1.SelectTab(item);
        }

        private QTabItem CloneTabButton(QTabItem tab, string optionURL, bool fSelect, int index) {
            NowTabCloned = fSelect;
            QTabItem item = tab.Clone();
            if(index < 0) {
                AddInsertTab(item);
            }
            else if((-1 < index) && (index < (tabControl1.TabCount + 1))) {
                tabControl1.TabPages.Insert(index, item);
            }
            else {
                AddInsertTab(item);
            }
            if(optionURL != null) {
                using(IDLWrapper wrapper = new IDLWrapper(optionURL)) {
                    item.NavigatedTo(optionURL, wrapper.IDL, -1, false);
                }
            }
            if(fSelect) {
                tabControl1.SelectTab(item);
            }
            else {
                item.RefreshRectangle();
                tabControl1.Refresh();
            }
            return item;
        }

        private List<string> CloseAllTabsExcept(QTabItemBase leaveThisOne, bool leaveLocked = true) {
            List<QTabItemBase> tabs = tabControl1.TabPages.Where(item => 
                !(leaveLocked && item.TabLocked) && item != leaveThisOne).ToList();
            List<string> paths = tabs.Select(tab => ((QTabItem)tab).CurrentPath).ToList();
            CloseTabs(tabs, !leaveLocked);
            return paths;
        }

        public override void CloseDW(uint dwReserved) {
            try {
                if(treeViewWrapper != null) {
                    treeViewWrapper.Dispose();
                    treeViewWrapper = null;
                }
                if(listViewManager != null) {
                    listViewManager.Dispose();
                    listViewManager = null;
                }
                if(subDirTip_Tab != null) {
                    subDirTip_Tab.Dispose();
                    subDirTip_Tab = null;
                }
                if(IsShown) {
                    if(pluginManager != null) {
                        pluginManager.Close(false);
                        pluginManager = null;
                    }
                    if(hHook_Key != IntPtr.Zero) {
                        PInvoke.UnhookWindowsHookEx(hHook_Key);
                        hHook_Key = IntPtr.Zero;
                    }
                    if(hHook_Mouse != IntPtr.Zero) {
                        PInvoke.UnhookWindowsHookEx(hHook_Mouse);
                        hHook_Mouse = IntPtr.Zero;
                    }
                    if(hHook_Msg != IntPtr.Zero) {
                        PInvoke.UnhookWindowsHookEx(hHook_Msg);
                        hHook_Msg = IntPtr.Zero;
                    }
                    if(explorerController != null) {
                        explorerController.ReleaseHandle();
                        explorerController = null;
                    }
                    if(rebarController != null) {
                        rebarController.Dispose();
                        rebarController = null;
                    }
                    if(!QTUtility.IsXP && (travelBtnController != null)) {
                        travelBtnController.ReleaseHandle();
                        travelBtnController = null;
                    }
                    if(dicNotifyIcon != null) {
                        dicNotifyIcon.Remove(ExplorerHandle);
                    }
                    if((hwndNotifyIconParent == ExplorerHandle) && (notifyIcon != null)) {
                        notifyIcon.Dispose();
                        notifyIcon = null;
                        contextMenuNotifyIcon.Dispose();
                        contextMenuNotifyIcon = null;
                        hwndNotifyIconParent = IntPtr.Zero;
                        if(dicNotifyIcon.Count > 0) {
                            foreach(IntPtr ptr in dicNotifyIcon.Keys) {
                                IntPtr tabBarHandle = InstanceManager.GetTabBarHandle(ptr);
                                if(1 == ((int)QTUtility2.SendCOPYDATASTRUCT(tabBarHandle, (IntPtr)0x30, "createNI", IntPtr.Zero))) {
                                    break;
                                }
                            }
                        }
                    }
                    
                    // TODO: check this
                    using(RegistryKey key = Registry.CurrentUser.CreateSubKey(RegConst.Root)) {
                        if(Config.Misc.KeepHistory) {
                            foreach(QTabItem item in tabControl1.TabPages) {
                                AddToHistory(item);
                            }
                            QTUtility.SaveRecentlyClosed(key);
                        }
                        if(Config.Misc.KeepRecentFiles && Config.AllRecentFiles) {
                            QTUtility.SaveRecentFiles(key);
                        }
                        string[] list = (from QTabItem item2 in tabControl1.TabPages
                                where item2.TabLocked
                                select item2.CurrentPath).ToArray();
                        QTUtility2.WriteRegBinary(list, "TabsLocked", key);
                        if(InstanceManager.RemoveInstance(ExplorerHandle, this)) {
                            InstanceManager.NextInstanceExists();
                            QTUtility2.WriteRegHandle("Handle", key, InstanceManager.CurrentHandle);
                        }
                        if(Config.SaveTransparency) {
                            if(0x80000 != ((int)PInvoke.Ptr_OP_AND(PInvoke.GetWindowLongPtr(ExplorerHandle, -20), 0x80000))) {
                                QTUtility.WindowAlpha = 0xff;
                            }
                            else {
                                byte num;
                                int num2;
                                int num3;
                                if(PInvoke.GetLayeredWindowAttributes(ExplorerHandle, out num2, out num, out num3)) {
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
                    Cursor = Cursors.Default;
                    if((curTabDrag != null) && (curTabDrag != Cursors.Default)) {
                        PInvoke.DestroyIcon(curTabDrag.Handle);
                        GC.SuppressFinalize(curTabDrag);
                        curTabDrag = null;
                    }
                    if((curTabCloning != null) && (curTabCloning != Cursors.Default)) {
                        PInvoke.DestroyIcon(curTabCloning.Handle);
                        GC.SuppressFinalize(curTabCloning);
                        curTabCloning = null;
                    }
                    if(dropTargetWrapper != null) {
                        dropTargetWrapper.Dispose();
                        dropTargetWrapper = null;
                    }
                    OptionsDialog.ForceClose();
                    if(tabSwitcher != null) {
                        tabSwitcher.Dispose();
                        tabSwitcher = null;
                    }
                }
                QTUtility.InstancesCount--;
                if(TravelLog != null) {
                    Marshal.FinalReleaseComObject(TravelLog);
                    TravelLog = null;
                }
                if(iContextMenu2 != null) {
                    Marshal.FinalReleaseComObject(iContextMenu2);
                    iContextMenu2 = null;
                }
                if(ShellBrowser != null) {
                    ShellBrowser.Dispose();
                    ShellBrowser = null;
                }
                foreach(ITravelLogEntry entry in LogEntryDic.Values) {
                    if(entry != null) {
                        Marshal.FinalReleaseComObject(entry);
                    }
                }
                LogEntryDic.Clear();
                fFinalRelease = true;
                base.CloseDW(dwReserved);
            }
            catch(Exception exception2) {
                QTUtility2.MakeErrorLog(exception2, "tabbar closing");
            }
        }

        private void CloseLeftRight(bool fLeft, int index) {
            if(index == -1) {
                index = tabControl1.SelectedIndex;
            }
            if(fLeft ? (index <= 0) : (index >= (tabControl1.TabCount - 1))) return;
            CloseTabs(fLeft
                    ? tabControl1.TabPages.Take(index).ToList()
                    : tabControl1.TabPages.Skip(index + 1).ToList());
        }
        
        // TODO: Optional params
        private bool CloseTab(QTabItem closingTab) {
            return ((tabControl1.TabCount > 1) && CloseTab(closingTab, false));
        }

        private bool CloseTab(QTabItem closingTab, bool fCritical, bool fSkipSync = false) {
            if(closingTab == null) {
                return false;
            }
            if((!fCritical && closingTab.TabLocked) && QTUtility2.PathExists(closingTab.CurrentPath)) {
                return false;
            }
            int index = tabControl1.TabPages.IndexOf(closingTab);
            if(index == -1) {
                return false;
            }
            lstActivatedTabs.Remove(closingTab);
            AddToHistory(closingTab);
            tabControl1.TabPages.Remove(closingTab);
            closingTab.OnClose();
            if(closingTab != CurrentTab) {
                if(!fSkipSync) SyncButtonBarCurrent(0x1003c);
                return true;
            }
            CurrentTab = null;
            int tabCount = tabControl1.TabCount;
            if(tabCount == 0) return true;
            QTabItemBase tabPage = null;
            switch(Config.Tabs.NextAfterClosed) {
                case TabPos.Right:
                    tabPage = tabControl1.TabPages[index == tabCount ? index - 1: index];
                    break;

                case TabPos.Left:
                    tabPage = tabControl1.TabPages[index == 0 ? 0 : index - 1];
                    break;

                case TabPos.Rightmost:
                    tabPage = tabControl1.TabPages[tabCount - 1];
                    break;

                case TabPos.Leftmost:
                    tabPage = tabControl1.TabPages[0];
                    break;

                case TabPos.LastActive:
                    if(lstActivatedTabs.Count > 0) {
                        QTabItemBase lastTab = lstActivatedTabs[lstActivatedTabs.Count - 1];
                        lstActivatedTabs.RemoveAt(lstActivatedTabs.Count - 1);
                        tabPage = tabControl1.TabPages.Contains(lastTab)
                                ? lastTab
                                : tabControl1.TabPages[0];
                    }
                    else {
                        tabPage = tabControl1.TabPages[0];
                    }
                    break;
            }
            if(tabPage != null) {
                tabControl1.SelectTab(tabPage);
            }
            else {
                tabControl1.SelectTab(0);
            }
            if(!fSkipSync) SyncButtonBarCurrent((tabPage == null) ? 60 : 0x3f);
            return true;
        }

        private void CloseTabs(IEnumerable<QTabItemBase> tabs, bool fCritical = false) {
            tabControl1.SetRedraw(false);
            bool closeCurrent = false;
            foreach(QTabItem tab in tabs) {
                if(tab == CurrentTab)
                    closeCurrent = true;
                else
                    CloseTab(tab, fCritical, true);
            }
            if(closeCurrent) {
                CloseTab(CurrentTab, fCritical);
            }
            else {
                SyncButtonBarCurrent(0x1003f);
            }
            if(tabControl1.TabCount > 0) {
                tabControl1.SetRedraw(true);
            }
        }

        private void contextMenuDropped_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            if(e.ClickedItem.Tag != null) {
                List<string> tag = (List<string>)contextMenuDropped.Tag;
                using(RegistryKey key = Registry.CurrentUser.CreateSubKey(RegConst.Root + @"UserApps")) {
                    if(key != null) {
                        List<string> list2 = key.GetValueNames().Select(str => str.ToLower()).ToList();
                        foreach(string str2 in tag) {
                            try {
                                string directoryName = Path.GetDirectoryName(str2) ?? string.Empty;
                                string fileName = Path.GetFileName(str2);
                                string regValueName = fileName;
                                int num = 2;
                                while(list2.Contains(regValueName.ToLower())) {
                                    regValueName = fileName + "_" + num++;
                                }
                                string[] array = new string[] { str2, string.Empty, directoryName, "0" };
                                QTUtility2.WriteRegBinary(array, regValueName, key);
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
                    ShowTaskbarItem(tag, true);
                }
            }
        }

        private static void contextMenuNotifyIcon_SubItems_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            IntPtr tag = (IntPtr)e.ClickedItem.Tag;
            int index = ((ToolStripMenuItem)sender).DropDownItems.IndexOf(e.ClickedItem);
            ShowTaskbarItem(tag, true);
            QTTabBarClass tabBar = InstanceManager.GetTabBar(tag);
            if((tabBar != null) && (index > -1)) {
                tabBar.Invoke(new NavigationCompleteCallback(contextMenuNotifyIcon_SubItems_SelectTab), new object[] { tabBar, (IntPtr)index });
            }
        }

        private static void contextMenuNotifyIcon_SubItems_SelectTab(object tabBar, IntPtr index) {
            ((QTTabBarClass)tabBar).tabControl1.SelectedIndex = (int)index;
        }

        private void contextMenuSys_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            if(e.ClickedItem == tsmiOption) {
                OptionsDialog.Open();
            }
            else if(e.ClickedItem == tsmiCloseAllButCurrent) {
                if(tabControl1.TabCount != 1) {
                    CloseAllTabsExcept(CurrentTab);
                }
            }
            else if(e.ClickedItem == tsmiBrowseFolder) {
                ChooseNewDirectory();
            }
            else if(e.ClickedItem == tsmiCloseWindow) {
                WindowUtils.CloseExplorer(ExplorerHandle, 1);
            }
            else {
                if(e.ClickedItem == tsmiLastActiv) {
                    try {
                        tabControl1.SelectTab(lstActivatedTabs[lstActivatedTabs.Count - 2]);
                        return;
                    }
                    catch {
                        return;
                    }
                }
                if(e.ClickedItem == tsmiLockToolbar) {
                    rebarController.Locked = !tsmiLockToolbar.Checked;
                }
                else if(e.ClickedItem == tsmiMergeWindows) {
                    MergeAllWindows();
                }
            }
        }

        private void contextMenuSys_Opening(object sender, CancelEventArgs e) {
            InitializeSysMenu(false);
            contextMenuSys.SuspendLayout();
            tsmiGroups.DropDown.SuspendLayout();
            tsmiUndoClose.DropDown.SuspendLayout();
            MenuUtility.CreateGroupItems(tsmiGroups);
            MenuUtility.CreateUndoClosedItems(tsmiUndoClose);
            if((lstActivatedTabs.Count > 1) && tabControl1.TabPages.Contains(lstActivatedTabs[lstActivatedTabs.Count - 2])) {
                tsmiLastActiv.ToolTipText = lstActivatedTabs[lstActivatedTabs.Count - 2].CurrentPath;
                tsmiLastActiv.Enabled = true;
            }
            else {
                tsmiLastActiv.ToolTipText = string.Empty;
                tsmiLastActiv.Enabled = false;
            }
            while(tsmiExecuted.DropDownItems.Count > 0) {
                tsmiExecuted.DropDownItems[0].Dispose();
            }
            List<ToolStripItem> list = MenuUtility.CreateRecentFilesItems();
            if(list.Count > 0) {
                tsmiExecuted.DropDown.SuspendLayout();
                tsmiExecuted.DropDownItems.AddRange(list.ToArray());
                tsmiExecuted.DropDown.ResumeLayout();
            }
            tsmiExecuted.Enabled = tsmiExecuted.DropDownItems.Count > 0;
            tsmiMergeWindows.Enabled = QTUtility.InstancesCount > 1;
            tsmiLockToolbar.Checked = rebarController.Locked;
            if((lstPluginMenuItems_Sys != null) && (lstPluginMenuItems_Sys.Count > 0)) {
                foreach(ToolStripItem item in lstPluginMenuItems_Sys) {
                    item.Dispose();
                }
                lstPluginMenuItems_Sys = null;
            }
            if((pluginManager != null) && (pluginManager.dicFullNamesMenuRegistered_Sys.Count > 0)) {
                lstPluginMenuItems_Sys = new List<ToolStripItem>();
                int index = contextMenuSys.Items.IndexOf(tsmiOption);
                ToolStripSeparator separator = new ToolStripSeparator();
                contextMenuSys.Items.Insert(index, separator);
                foreach(string str in pluginManager.dicFullNamesMenuRegistered_Sys.Keys) {
                    ToolStripMenuItem item2 = new ToolStripMenuItem(pluginManager.dicFullNamesMenuRegistered_Sys[str]);
                    item2.Name = str;
                    item2.Tag = MenuType.Bar;
                    item2.Click += pluginitems_Click;
                    contextMenuSys.Items.Insert(index, item2);
                    lstPluginMenuItems_Sys.Add(item2);
                }
                lstPluginMenuItems_Sys.Add(separator);
            }
            tsmiUndoClose.DropDown.ResumeLayout();
            tsmiGroups.DropDown.ResumeLayout();
            contextMenuSys.ResumeLayout();
        }

        private void contextMenuTab_Closed(object sender, ToolStripDropDownClosedEventArgs e) {
            tabControl1.SetContextMenuState(false);
            if(ContextMenuedTab != CurrentTab) {
                tabControl1.Refresh();
            }
        }

        private void contextMenuTab_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            if(ContextMenuedTab != null) {
                if(e.ClickedItem == tsmiClose) {
                    if(tabControl1.TabCount == 1) {
                        WindowUtils.CloseExplorer(ExplorerHandle, 1);
                    }
                    else {
                        CloseTab(ContextMenuedTab);
                    }
                }
                else if(e.ClickedItem == tsmiCloseAllButThis) {
                    CloseAllTabsExcept(ContextMenuedTab);
                }
                else if(e.ClickedItem == tsmiCloseLeft) {
                    int index = tabControl1.TabPages.IndexOf(ContextMenuedTab);
                    if(index > 0) {
                        CloseLeftRight(true, index);
                    }
                }
                else if(e.ClickedItem == tsmiCloseRight) {
                    int num2 = tabControl1.TabPages.IndexOf(ContextMenuedTab);
                    if(num2 >= 0) {
                        CloseLeftRight(false, num2);
                    }
                }
                else if(e.ClickedItem == tsmiCreateGroup) {
                    CreateGroup(ContextMenuedTab);
                }
                else if(e.ClickedItem == tsmiLockThis) {
                    ContextMenuedTab.TabLocked = !ContextMenuedTab.TabLocked;
                }
                else if(e.ClickedItem == tsmiCloneThis) {
                    CloneTabButton(ContextMenuedTab, null, true, -1);
                }
                else if(e.ClickedItem == tsmiCreateWindow) {
                    using(IDLWrapper wrapper = new IDLWrapper(ContextMenuedTab.CurrentIDL)) {
                        OpenNewWindow(wrapper);
                    }
                    if(!Config.KeepOnSeparate != ((ModifierKeys & Keys.Shift) != Keys.None)) {
                        CloseTab(ContextMenuedTab);
                    }
                }
                else if(e.ClickedItem == tsmiCopy) {
                    string currentPath = ContextMenuedTab.CurrentPath;
                    if(currentPath.IndexOf("???") != -1) {
                        currentPath = currentPath.Substring(0, currentPath.IndexOf("???"));
                    }
                    else if(currentPath.IndexOf("*?*?*") != -1) {
                        currentPath = currentPath.Substring(0, currentPath.IndexOf("*?*?*"));
                    }
                    SetStringClipboard(currentPath);
                }
                else if(e.ClickedItem == tsmiProp) {
                    ShellMethods.ShowProperties(ContextMenuedTab.CurrentIDL);
                }
            }
        }

        private void contextMenuTab_Opening(object sender, CancelEventArgs e) {
            InitializeTabMenu(false);
            int index = tabControl1.TabPages.IndexOf(ContextMenuedTab);
            if((index == -1) || (ContextMenuedTab == null)) {
                e.Cancel = true;
            }
            else {
                tabControl1.SetContextMenuState(true);
                contextMenuTab.SuspendLayout();
                if(tabControl1.TabCount == 1) {
                    tsmiTabOrder.Enabled = tsmiCloseAllButThis.Enabled = tsmiCloseLeft.Enabled = tsmiCloseRight.Enabled = false;
                }
                else {
                    if(index == 0) {
                        tsmiCloseLeft.Enabled = false;
                        tsmiCloseRight.Enabled = true;
                    }
                    else if(index == (tabControl1.TabCount - 1)) {
                        tsmiCloseLeft.Enabled = true;
                        tsmiCloseRight.Enabled = false;
                    }
                    else {
                        tsmiCloseLeft.Enabled = tsmiCloseRight.Enabled = true;
                    }
                    tsmiTabOrder.Enabled = tsmiCloseAllButThis.Enabled = true;
                }
                tsmiClose.Enabled = !ContextMenuedTab.TabLocked;
                tsmiLockThis.Text = ContextMenuedTab.TabLocked ? QTUtility.ResMain[20] : QTUtility.ResMain[6];
                if(GroupsManager.GroupCount > 0) {
                    tsmiAddToGroup.DropDown.SuspendLayout();
                    tsmiAddToGroup.Enabled = true;
                    while(tsmiAddToGroup.DropDownItems.Count > 0) {
                        tsmiAddToGroup.DropDownItems[0].Dispose();
                    }
                    foreach(Group g in GroupsManager.Groups.Where(g => g.Paths.Count > 0)) {
                        tsmiAddToGroup.DropDownItems.Add(new ToolStripMenuItem(g.Name) {
                            ImageKey = QTUtility.GetImageKey(g.Paths[0], null)
                        });
                    }
                    tsmiAddToGroup.DropDown.ResumeLayout();
                }
                else {
                    tsmiAddToGroup.Enabled = false;
                }
                tsmiHistory.DropDown.SuspendLayout();
                while(tsmiHistory.DropDownItems.Count > 0) {
                    tsmiHistory.DropDownItems[0].Dispose();
                }
                if((ContextMenuedTab.HistoryCount_Back + ContextMenuedTab.HistoryCount_Forward) > 1) {
                    tsmiHistory.DropDownItems.AddRange(CreateNavBtnMenuItems(false).ToArray());
                    tsmiHistory.DropDownItems.AddRange(CreateBranchMenu(false, components, tsmiBranchRoot_DropDownItemClicked).ToArray());
                    tsmiHistory.Enabled = true;
                }
                else {
                    tsmiHistory.Enabled = false;
                }
                tsmiHistory.DropDown.ResumeLayout();
                contextMenuTab.Items.Remove(menuTextBoxTabAlias);
                if(!Config.Tabs.RenameAmbTabs) {
                    contextMenuTab.Items.Insert(12, menuTextBoxTabAlias);
                    if(ContextMenuedTab.Comment.Length > 0) {
                        menuTextBoxTabAlias.Text = ContextMenuedTab.Comment;
                        menuTextBoxTabAlias.ForeColor = SystemColors.WindowText;
                    }
                    else {
                        menuTextBoxTabAlias.Text = QTUtility.ResMain[0x1b];
                        menuTextBoxTabAlias.ForeColor = SystemColors.GrayText;
                    }
                    menuTextBoxTabAlias.Enabled = !tabControl1.AutoSubText;
                }
                if(tsmiTabOrder.DropDownItems.Count == 0) {
                    ((ToolStripDropDownMenu)tsmiTabOrder.DropDown).ShowImageMargin = false;
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
                    tsmiTabOrder.DropDownItems.Add(item2);
                    tsmiTabOrder.DropDownItems.Add(item3);
                    tsmiTabOrder.DropDownItems.Add(item4);
                    tsmiTabOrder.DropDownItems.Add(separator);
                    tsmiTabOrder.DropDownItems.Add(item5);
                    tsmiTabOrder.DropDownItemClicked += menuitemTabOrder_DropDownItemClicked;
                }
                if((lstPluginMenuItems_Tab != null) && (lstPluginMenuItems_Tab.Count > 0)) {
                    foreach(ToolStripItem item6 in lstPluginMenuItems_Tab) {
                        item6.Dispose();
                    }
                    lstPluginMenuItems_Tab = null;
                }
                if((pluginManager != null) && (pluginManager.dicFullNamesMenuRegistered_Tab.Count > 0)) {
                    lstPluginMenuItems_Tab = new List<ToolStripItem>();
                    int num2 = contextMenuTab.Items.IndexOf(tsmiProp);
                    ToolStripSeparator separator2 = new ToolStripSeparator();
                    contextMenuTab.Items.Insert(num2, separator2);
                    foreach(string str3 in pluginManager.dicFullNamesMenuRegistered_Tab.Keys) {
                        ToolStripMenuItem item7 = new ToolStripMenuItem(pluginManager.dicFullNamesMenuRegistered_Tab[str3]);
                        item7.Name = str3;
                        item7.Tag = MenuType.Tab;
                        item7.Click += pluginitems_Click;
                        contextMenuTab.Items.Insert(num2, item7);
                        lstPluginMenuItems_Tab.Add(item7);
                    }
                    lstPluginMenuItems_Tab.Add(separator2);
                }
                contextMenuTab.ResumeLayout();
            }
        }

        private void Controls_GotFocus(object sender, EventArgs e) {
            OnGotFocus(e);
        }

        internal List<ToolStripItem> CreateBranchMenu(bool fCurrent, IContainer container, ToolStripItemClickedEventHandler itemClickedEvent) {
            QTabItem item = fCurrent ? CurrentTab : ContextMenuedTab;
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
                        if(LogEntryDic.ContainsKey(data.Hash)) {
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
            contextMenuNotifyIcon.EnsureHandleCreated();
            contextMenuNotifyIcon.Invoke(new NavigationCompleteCallback(CreateContextMenuItems_NotifyIcon_Core), new object[] { sw, hwnd });
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
                QTTabBarClass tabBar = InstanceManager.GetTabBar(ptr);
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
                            item.DropDownItemClicked += contextMenuNotifyIcon_SubItems_DropDownItemClicked;
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
            NowModalDialogShown = true;
            using(CreateNewGroupForm form = new CreateNewGroupForm(contextMenuedTab.CurrentPath, tabControl1.TabPages)) {
                if(NowTopMost) {
                    form.TopMost = true;
                }
                if(DialogResult.OK == form.ShowDialog()) {
                    // TODO: should not be necessary
                    SyncButtonBarBroadCast(1);
                    SyncTaskBarMenu();
                }
            }
            NowModalDialogShown = false;
        }

        internal List<QMenuItem> CreateNavBtnMenuItems(bool fCurrent) {
            QTabItem item = fCurrent ? CurrentTab : ContextMenuedTab;
            List<QMenuItem> list = new List<QMenuItem>();
            string[] historyBack = item.GetHistoryBack();
            string[] historyForward = item.GetHistoryForward();
            if((historyBack.Length + historyForward.Length) > 1) {
                for(int i = historyBack.Length - 1; i >= 0; i--) {
                    QMenuItem item2 = MenuUtility.CreateMenuItem(new MenuItemArguments(historyBack[i], true, i, MenuGenre.Navigation));
                    if(IsSpecialFolderNeedsToTravel(historyBack[i])) {
                        item2.Enabled = LogEntryDic.ContainsKey(item.GetLogHash(true, i));
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
                        item3.Enabled = LogEntryDic.ContainsKey(item.GetLogHash(false, j));
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
            QTabItem tab = new QTabItem(QTUtility2.MakePathDisplayText(path, false), path, tabControl1);
            tab.NavigatedTo(path, idlw.IDL, -1, false);
            tab.ToolTipText = QTUtility2.MakePathDisplayText(path, true);
            AddInsertTab(tab);
            return tab;
        }

        internal static Bitmap[] CreateTabImage() {
            if(File.Exists(Config.Skin.TabImageFile)) {
                try {
                    Bitmap[] bitmapArray = new Bitmap[3];
                    using(Bitmap bitmap = new Bitmap(Config.Skin.TabImageFile)) {
                        int height = bitmap.Height / 3;
                        bitmapArray[0] = bitmap.Clone(new Rectangle(0, 0, bitmap.Width, height), PixelFormat.Format32bppArgb);
                        bitmapArray[1] = bitmap.Clone(new Rectangle(0, height, bitmap.Width, height), PixelFormat.Format32bppArgb);
                        bitmapArray[2] = bitmap.Clone(new Rectangle(0, height * 2, bitmap.Width, height), PixelFormat.Format32bppArgb);
                    }
                    if(Path.GetExtension(Config.Skin.TabImageFile).PathEquals(".bmp")) {
                        bitmapArray[0].MakeTransparent(Color.Magenta);
                        bitmapArray[1].MakeTransparent(Color.Magenta);
                        bitmapArray[2].MakeTransparent(Color.Magenta);
                    }
                    return bitmapArray;
                }
                catch {
                }
            }
            return null;
        }

        // todo: handle links
        private static IEnumerable<string> CreateTMPPathsToOpenNew(Address[] addresses, string pathExclude) {
            List<string> list = new List<string>();
            QTUtility2.InitializeTemporaryPaths();
            for(int i = 0; i < addresses.Length; i++) {
                try {
                    using(IDLWrapper wrapper = new IDLWrapper(addresses[i].ITEMIDLIST)) {
                        if(wrapper.Available && wrapper.HasPath) {
                            string path = wrapper.Path;
                            if(path.Length > 0 && !path.PathEquals(pathExclude) && 
                                    !QTUtility2.IsShellPathButNotFileSystem(path) && 
                                    wrapper.IsFolder && !wrapper.IsLinkToDeadFolder) {
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
                    e.HRESULT = ShellMethods.PopUpSystemContextMenu(wrapper, e.IsKey ? e.Point : MousePosition, ref iContextMenu2, ((DropDownMenuReorderable)sender).Handle, true);
                }
                if(e.HRESULT == 0xffff) {
                    QTUtility.ClosedTabHistoryList.Remove(clickedItem.Path);
                    e.ClickedItem.Dispose();
                }
            }
        }

        private void ddrmrGroups_ItemMiddleClicked(object sender, ItemRightClickedEventArgs e) {
            ReplaceByGroup(e.ClickedItem.Text);
        }

        protected override void Dispose(bool disposing) {
            if(disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool DoBindAction(BindAction action, bool fRepeat = false, QTabItem tab = null, IDLWrapper item = null) {
            if(fRepeat && !(
                    action == BindAction.GoBack ||
                    action == BindAction.GoForward ||
                    action == BindAction.TransparencyPlus ||
                    action == BindAction.TransparencyMinus)) {
                return false;
            }

            if(tab == null) tab = CurrentTab;

            IntPtr ptr;
            switch(action) {
                case BindAction.GoBack:
                    NavigateCurrentTab(true);
                    break;

                case BindAction.GoForward:
                    NavigateCurrentTab(false);
                    break;

                case BindAction.GoFirst:
                    NavigateToFirstOrLast(true);
                    break;

                case BindAction.GoLast:
                    NavigateToFirstOrLast(false);
                    break;

                case BindAction.NextTab:
                    if(tabControl1.SelectedIndex == tabControl1.TabCount - 1) {
                        tabControl1.SelectedIndex = 0;
                    }
                    else {
                        tabControl1.SelectedIndex++;
                    }
                    break;

                case BindAction.PreviousTab:
                    if(tabControl1.SelectedIndex == 0) {
                        tabControl1.SelectedIndex = tabControl1.TabCount - 1;
                    }
                    else {
                        tabControl1.SelectedIndex--;
                    }
                    break;

                case BindAction.FirstTab:
                    tabControl1.SelectedIndex = 0;
                    break;

                case BindAction.LastTab:
                    tabControl1.SelectedIndex = tabControl1.TabCount - 1;
                    break;

                case BindAction.CloseCurrent:
                case BindAction.CloseTab:
                    NowTabDragging = false;
                    if(!tab.TabLocked) {
                        if(tabControl1.TabCount > 1) {
                            CloseTab(tab);
                        }
                        else {
                            WindowUtils.CloseExplorer(ExplorerHandle, 1);
                        }
                    }
                    break;

                case BindAction.CloseAllButCurrent:
                case BindAction.CloseAllButThis:
                    CloseAllTabsExcept(tab);
                    break;

                case BindAction.CloseLeft:
                case BindAction.CloseLeftTab:
                    CloseLeftRight(true, tab.Index);
                    break;

                case BindAction.CloseRight:
                case BindAction.CloseRightTab:
                    CloseLeftRight(false, tab.Index);
                    break;

                case BindAction.CloseWindow:
                    WindowUtils.CloseExplorer(ExplorerHandle, 1);
                    break;

                case BindAction.RestoreLastClosed:
                    RestoreLastClosed();
                    break;

                case BindAction.CloneCurrent:
                case BindAction.CloneTab:
                    CloneTabButton(tab, null, true, -1);
                    break;

                case BindAction.TearOffCurrent:
                case BindAction.TearOffTab:
                    if(tabControl1.TabCount > 1) {
                        using(IDLWrapper wrapper = new IDLWrapper(tab.CurrentIDL)) {
                            OpenNewWindow(wrapper);
                        }
                        CloseTab(tab);
                    }
                    break;

                case BindAction.LockCurrent:
                case BindAction.LockTab:
                    tab.TabLocked = !tab.TabLocked;
                    break;

                case BindAction.LockAll:
                    bool lockState = tabControl1.TabPages.Any(t => t.TabLocked);
                    tabControl1.TabPages.ForEach(t => t.TabLocked = !lockState);
                    break;

                case BindAction.BrowseFolder:
                    ChooseNewDirectory();
                    break;

                case BindAction.CreateNewGroup:
                    CreateGroup(tab);
                    break;

                case BindAction.ShowOptions:
                    OptionsDialog.Open();
                    break;

                case BindAction.ShowToolbarMenu: // hmm.
                    Rectangle tabRect = tabControl1.GetTabRect(tabControl1.TabCount - 1, true);
                    contextMenuSys.Show(PointToScreen(new Point(tabRect.Right + 10, tabRect.Bottom - 10)));
                    break;

                case BindAction.ShowTabMenuCurrent:
                    if(tab.Index != -1) {
                        ContextMenuedTab = tab;
                        Rectangle rect = tabControl1.GetTabRect(tab.Index, true);
                        contextMenuTab.Show(PointToScreen(new Point(rect.Right + 10, rect.Bottom - 10)));
                    }
                    break;

                case BindAction.ShowTabMenu:
                    ContextMenuedTab = tab;
                    contextMenuTab.Show(MousePosition);
                    break;

                case BindAction.ShowGroupMenu:
                    return InstanceManager.TryGetButtonBarHandle(ExplorerHandle, out ptr) &&
                            1 == (int)QTUtility2.SendCOPYDATASTRUCT(ptr, (IntPtr)4, "fromTab", (IntPtr)3);

                case BindAction.ShowRecentFolderMenu:
                    return InstanceManager.TryGetButtonBarHandle(ExplorerHandle, out ptr) &&
                            1 == (int)QTUtility2.SendCOPYDATASTRUCT(ptr, (IntPtr)4, "fromTab", (IntPtr)4);

                case BindAction.ShowUserAppsMenu:
                    return InstanceManager.TryGetButtonBarHandle(ExplorerHandle, out ptr) &&
                            1 == (int)QTUtility2.SendCOPYDATASTRUCT(ptr, (IntPtr)4, "fromTab", (IntPtr)5);

                case BindAction.ToggleMenuBar:
                    rebarController.MenuBarShown = !rebarController.MenuBarShown;
                    break;

                case BindAction.CopySelectedPaths:
                    if(listView.SubDirTipMenuIsShowing() || (subDirTip_Tab != null && subDirTip_Tab.MenuIsShowing)) {
                        return false;
                    }
                    DoFileTools(0);
                    break;

                case BindAction.CopySelectedNames:
                    if(listView.SubDirTipMenuIsShowing() || (subDirTip_Tab != null && subDirTip_Tab.MenuIsShowing)) {
                        return false;
                    }
                    DoFileTools(1);
                    break;

                case BindAction.CopyCurrentFolderPath:
                    DoFileTools(2);
                    break;

                case BindAction.CopyCurrentFolderName:
                    DoFileTools(3);
                    break;

                case BindAction.ChecksumSelected:
                    DoFileTools(4);
                    break;

                case BindAction.ToggleTopMost:
                    ToggleTopMost();
                    SyncButtonBarCurrent(0x40);
                    break;

                case BindAction.TransparencyPlus:
                case BindAction.TransparencyMinus: {
                        // TODO!!!
                        int num9;
                        int num10;
                        byte num11;
                        if(0x80000 != ((int)PInvoke.Ptr_OP_AND(PInvoke.GetWindowLongPtr(ExplorerHandle, -20), 0x80000))) {
                            if(action == BindAction.TransparencyPlus) {
                                return true;
                            }
                            PInvoke.SetWindowLongPtr(ExplorerHandle, -20, PInvoke.Ptr_OP_OR(PInvoke.GetWindowLongPtr(ExplorerHandle, -20), 0x80000));
                            PInvoke.SetLayeredWindowAttributes(ExplorerHandle, 0, 0xff, 2);
                        }
                        if(PInvoke.GetLayeredWindowAttributes(ExplorerHandle, out num9, out num11, out num10)) {
                            IntPtr ptr3;
                            if(action == BindAction.TransparencyPlus) {
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
                            PInvoke.SetLayeredWindowAttributes(ExplorerHandle, 0, num11, 2);
                            if(InstanceManager.TryGetButtonBarHandle(ExplorerHandle, out ptr3)) {
                                QTUtility2.SendCOPYDATASTRUCT(ptr3, (IntPtr)7, "track", (IntPtr)num11);
                            }
                            if(num11 == 0xff) {
                                PInvoke.SetWindowLongPtr(ExplorerHandle, -20, PInvoke.Ptr_OP_AND(PInvoke.GetWindowLongPtr(ExplorerHandle, -20), 0xfff7ffff));
                            }
                        }
                    }
                    break;

                case BindAction.FocusFileList:
                    listView.SetFocus();
                    break;

                case BindAction.FocusSearchBarReal:
                    if(QTUtility.IsXP) return false;
                    // todo, I don't think this works
                    PInvoke.SetFocus(GetSearchBand_Edit());
                    break;

                case BindAction.FocusSearchBarBBar:
                    if(!InstanceManager.TryGetButtonBarHandle(ExplorerHandle, out ptr) || !PInvoke.IsWindow(ptr)) return false;
                    QTUtility2.SendCOPYDATASTRUCT(ptr, (IntPtr)8, null, IntPtr.Zero);
                    break;

                case BindAction.ShowSDTSelected:
                    if(!Config.Tips.ShowSubDirTips) return false;
                    listView.ShowAndClickSubDirTip();
                    break;

                case BindAction.SendToTray:
                    ShowTaskbarItem(ExplorerHandle, false);
                    break;

                case BindAction.FocusTabBar:
                    tabControl1.Focus();
                    tabControl1.FocusNextTab(false, true, false);
                    break;

                case BindAction.NewTab:
                    using(IDLWrapper wrapper = new IDLWrapper(Config.Window.DefaultLocation)) {
                        OpenNewTab(wrapper, false, true);    
                    }
                    break;

                case BindAction.NewWindow:
                    using(IDLWrapper wrapper = new IDLWrapper(Config.Window.DefaultLocation)) {
                        OpenNewWindow(wrapper);
                    }
                    break;

                // TODO all the blank ones
                case BindAction.NewFolder:
                    break;
                case BindAction.NewFile:
                    break;

                case BindAction.SwitchToLastActivated:
                    if(lstActivatedTabs.Count > 1 && tabControl1.TabPages.Contains(lstActivatedTabs[lstActivatedTabs.Count - 2])) {
                        try {
                            tabControl1.SelectTab(lstActivatedTabs[lstActivatedTabs.Count - 2]);
                        }
                        catch(ArgumentException) {
                        }
                    }
                    break;

                case BindAction.MergeWindows:
                    break;
                case BindAction.ShowRecentFilesMenu:
                    break;
                case BindAction.SortTabsByName:
                    break;
                case BindAction.SortTabsByPath:
                    break;
                case BindAction.SortTabsByActive:
                    break;

                case BindAction.UpOneLevelTab:
                case BindAction.UpOneLevel:
                    UpOneLevel(); // Hmm...
                    break;

                case BindAction.Refresh:
                    break;
                case BindAction.Paste:
                    break;
                case BindAction.Maximize:
                    break;
                case BindAction.Minimize:
                    break;

                case BindAction.CopyTabPath:
                    string currentPath = tab.CurrentPath;
                    if(currentPath.IndexOf("???") != -1) {
                        currentPath = currentPath.Substring(0, currentPath.IndexOf("???"));
                    }
                    SetStringClipboard(currentPath);
                    break;

                case BindAction.TabProperties:
                    ShellMethods.ShowProperties(tab.CurrentIDL);
                    break;

                case BindAction.ShowTabSubfolderMenu:
                    break;

                case BindAction.ItemOpenInNewTab:
                case BindAction.ItemOpenInNewTabNoSel:
                case BindAction.ItemOpenInNewWindow:
                    if(item.Available && item.HasPath && item.IsReadyIfDrive && !item.IsLinkToDeadFolder) {
                        using(IDLWrapper linkWrapper = item.ResolveTargetIfLink()) {
                            IDLWrapper actualItem = linkWrapper ?? item;
                            if(actualItem.IsFolder) {
                                if(action == BindAction.ItemOpenInNewWindow) {
                                    OpenNewWindow(actualItem);
                                }
                                else {
                                    OpenNewTab(actualItem, action == BindAction.ItemOpenInNewTabNoSel);
                                }
                            }
                        }
                    }
                    break;

                case BindAction.ItemCut:
                case BindAction.ItemCopy:      
                case BindAction.ItemDelete:
                    break;

                case BindAction.ItemProperties:
                    ShellMethods.ShowProperties(item.IDL);
                    break;

                case BindAction.CopyItemPath:
                case BindAction.CopyItemName:
                case BindAction.ChecksumItem:
                    break;
            }
            return true;
        }

        // todo: clean, enum.
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
                        if(!ShellBrowser.TryGetSelection(out addressArray, out str2, index == 1)) {
                            goto Label_019C;
                        }
                        list = new List<string>();
                        num = 0;
                        goto Label_00A1;

                    case 2:
                    case 3: {
                            using(IDLWrapper wrapper = ShellBrowser.GetShellPath()) {
                                if(wrapper.Available) {
                                    displayName = ShellMethods.GetDisplayName(wrapper.PIDL, index == 3);
                                }
                                goto Label_019C;
                            }
                        }
                    case 5:
                        foreach(QTabItem item in tabControl1.TabPages) {
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
                foreach(string str2 in QTUtility.TMPPathList.Where(str2 => !str2.PathEquals(path))) {
                    using(IDLWrapper wrapper = new IDLWrapper(str2)) {
                        if(wrapper.Available) {
                            CreateNewTab(wrapper);
                        }
                    }
                }
                foreach(byte[] buffer in QTUtility.TMPIDLList) {
                    if(QTUtility.TMPTargetIDL != buffer) {
                        using(IDLWrapper wrapper2 = new IDLWrapper(buffer)) {
                            OpenNewTab(wrapper2, true, false);
                            continue;
                        }
                    }
                    QTUtility.TMPTargetIDL = null;
                }
                QTUtility2.InitializeTemporaryPaths();
                AddStartUpTabs(string.Empty, path);
                InitializeOpenedWindow();
            }
            else if(QTUtility.CreateWindowTMPGroup.Length != 0) {
                string createWindowTMPGroup = QTUtility.CreateWindowTMPGroup;
                QTUtility.CreateWindowTMPGroup = string.Empty;
                CurrentTab.CurrentPath = path;
                NowOpenedByGroupOpener = true;
                OpenGroup(createWindowTMPGroup, false);
                AddStartUpTabs(createWindowTMPGroup, path);
                InitializeOpenedWindow();
            }
            else if(!Config.Window.CaptureNewWindows || QTUtility.CreateWindowTMPPath == path) {
                QTUtility.CreateWindowTMPPath = string.Empty;
                AddStartUpTabs(string.Empty, path);
                InitializeOpenedWindow();
            }
            else if(path.StartsWith(QTUtility.ResMisc[0]) ||
                    (path.EndsWith(QTUtility.ResMisc[0]) && QTUtility2.IsShellPathButNotFileSystem(path)) ||
                    path.PathEquals(QTUtility.PATH_SEARCHFOLDER)) {
                InitializeOpenedWindow();
            }
            else {
                if(QTUtility.NoCapturePathsList.Any(ncPath => ncPath.PathEquals(path))) {
                    InitializeOpenedWindow();
                    return;
                }
                if(QTUtility.InstancesCount > 1) {
                    if((ModifierKeys == Keys.Control) || !InstanceManager.NextInstanceExists()) {
                        InitializeOpenedWindow();
                        AddStartUpTabs(string.Empty, path);
                    }
                    else {
                        if(!QTUtility.IsXP) {
                            PInvoke.SetWindowPos(ExplorerHandle, IntPtr.Zero, 0, 0, 0, 0, 0x259f);
                        }
                        NavigateOnOldWindow(path);
                        fNowQuitting = true;
                        if(!QTUtility.IsXP) {
                            Explorer.Quit();
                        }
                        else {
                            WindowUtils.CloseExplorer(ExplorerHandle, 0);
                        }
                    }
                }
                else {
                    if(!Config.DontCaptureNewWnds) {
                        uint num2;
                        uint num3;
                        PInvoke.GetWindowThreadProcessId(Handle, out num2);
                        PInvoke.GetWindowThreadProcessId(WindowUtils.GetShellTrayWnd(), out num3);
                        if(((num2 != num3) && (num2 != 0)) && (num3 != 0)) {
                            string nameToSelectFromCommandLineArg = GetNameToSelectFromCommandLineArg();
                            if(!WindowUtils.IsExplorerProcessSeparated()) {
                                bool flag = false;
                                using(RegistryKey key = Registry.CurrentUser.CreateSubKey(RegConst.Root)) {
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
                                    fNowQuitting = true;
                                    Explorer.Quit();
                                    return;
                                }
                            }
                            QTUtility.PathToSelectInCommandLineArg = nameToSelectFromCommandLineArg;
                        }
                    }
                    AddStartUpTabs(string.Empty, path);
                    InitializeOpenedWindow();
                }
            }
        }

        private int dropTargetWrapper_DragFileDrop(out IntPtr hwnd, out byte[] idlReal) {
            HideToolTipForDD();
            hwnd = tabControl1.Handle;
            idlReal = null;
            QTabItem tabMouseOn = (QTabItem)tabControl1.GetTabMouseOn();
            if((tabMouseOn == null) || !Config.Tabs.DragOverTabOpensSDT) {
                return 1;
            }
            if((tabMouseOn.CurrentIDL != null) && (tabMouseOn.CurrentIDL.Length > 0)) {
                idlReal = tabMouseOn.CurrentIDL;
                return 0;
            }
            return -1;
        }

        private DragDropEffects dropTargetWrapper_DragFileEnter(IntPtr hDrop, Point pnt, int grfKeyState) {
            if(Config.Tabs.DragOverTabOpensSDT) {
                int num = HandleDragEnter(hDrop, out strDraggingDrive, out strDraggingStartPath);
                fDrivesContainedDD = num == 2;
                if(num == -1) {
                    return DragDropEffects.None;
                }
                if(tabControl1.GetTabMouseOn() == null) {
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
            HideToolTipForDD();
            strDraggingDrive = null;
            strDraggingStartPath = null;
            tabControl1.Refresh();
        }

        private void dropTargetWrapper_DragFileOver(object sender, DragEventArgs e) {
            e.Effect = DragDropEffects.None;
            QTabItem tabMouseOn = (QTabItem)tabControl1.GetTabMouseOn();
            bool flag = true;
            if(tabMouseOn != tabForDD) {
                tabControl1.Refresh();
                HideSubDirTip_Tab_Menu();
                fToggleTabMenu = false;
                flag = false;
            }
            if(tabMouseOn == null) {
                e.Effect = DragDropEffects.Copy;
            }
            else if(tabMouseOn.CurrentPath.Length > 2) {
                if(fDrivesContainedDD || strDraggingStartPath.PathEquals(tabMouseOn.CurrentPath)) {
                    if(toolTipForDD != null) {
                        toolTipForDD.Hide(tabControl1);
                    }
                    ShowToolTipForDD(tabMouseOn, -1, e.KeyState);
                }
                else {
                    using(IDLWrapper wrapper = new IDLWrapper(tabMouseOn.CurrentIDL, !flag)) {
                        if(wrapper.Available && wrapper.IsDropTarget) {
                            string b = tabMouseOn.CurrentPath.Substring(0, 3);
                            int num = strDraggingDrive != null && strDraggingDrive.Equals(b, StringComparison.OrdinalIgnoreCase)
                                    ? 0 : 1;
                            ShowToolTipForDD(tabMouseOn, num, e.KeyState);
                            e.Effect = Config.Tabs.DragOverTabOpensSDT
                                    ? DropTargetWrapper.MakeEffect(e.KeyState, num)
                                    : DragDropEffects.Copy;
                        }
                        else {
                            HideToolTipForDD();
                        }
                    }
                }
            }
            else {
                HideToolTipForDD();
            }
        }

        private void Explorer_BeforeNavigate2(object pDisp, ref object URL, ref object Flags, ref object TargetFrameName, ref object PostData, ref object Headers, ref bool Cancel) {
            if(!IsShown) {
                DoFirstNavigation(true, (string)URL);
            }
        }

        private void Explorer_NavigateComplete2(object pDisp, ref object URL) {
            string path = (string)URL;
            lastCompletedBrowseObjectIDL = lastAttemptedBrowseObjectIDL;
            ShellBrowser.OnNavigateComplete();
            if(!IsShown) {
                DoFirstNavigation(false, path);
            }            

            if(fNowQuitting) {
                Explorer.Quit();
            }
            else {
                int hash = -1;
                bool flag = IsSpecialFolderNeedsToTravel(path);
                bool flag2 = QTUtility2.IsShellPathButNotFileSystem(path);
                bool flag3 = QTUtility2.IsShellPathButNotFileSystem(CurrentTab.CurrentPath);

                // If we're navigating on a locked tab, we simulate opening the target folder
                // in a new tab.  First we clone the tab at the old address and lock it.  Then
                // we move the current tab to the "new tab" position and unlock it.
                if(!flag2 && !flag3 && !NavigatedByCode && CurrentTab.TabLocked) {
                    int pos = tabControl1.SelectedIndex;
                    tabControl1.SetRedraw(false);
                    QTabItem item = CloneTabButton(CurrentTab, null, false, pos);
                    item.TabLocked = true;
                    CurrentTab.TabLocked = false;
                    pos++;
                    int max = tabControl1.TabPages.Count - 1;

                    switch(Config.Tabs.NewTabPosition) {
                        case TabPos.Rightmost:
                            if(pos != max) {
                                tabControl1.TabPages.Relocate(pos, max);
                            }
                            break;
                        case TabPos.Leftmost:
                            tabControl1.TabPages.Relocate(pos, 0);
                            break;
                        case TabPos.Left:
                            tabControl1.TabPages.Relocate(pos, pos - 1);
                            break;
                    }
                    tabControl1.SetRedraw(true);

                    lstActivatedTabs.Remove(CurrentTab);
                    lstActivatedTabs.Add(item);
                    lstActivatedTabs.Add(CurrentTab);
                    if(lstActivatedTabs.Count > 15) {
                        lstActivatedTabs.RemoveAt(0);
                    }
                }

                if(!NavigatedByCode && flag) {
                    hash = DateTime.Now.GetHashCode();
                    LogEntryDic[hash] = GetCurrentLogEntry();
                }
                ClearTravelLogs();
                try {
                    tabControl1.SetRedraw(false);
                    if(fNowTravelByTree) {
                        using(IDLWrapper wrapper = GetCurrentPIDL()) {
                            QTabItem tabPage = CreateNewTab(wrapper);
                            tabControl1.SelectTabDirectly(tabPage);
                            CurrentTab = tabPage;
                        }
                    }
                    if(tabControl1.AutoSubText && !fNavigatedByTabSelection) {
                        CurrentTab.Comment = string.Empty;
                    }
                    CurrentAddress = path;
                    CurrentTab.Text = Explorer.LocationName;
                    CurrentTab.CurrentIDL = null;
                    CurrentTab.TooltipText2 = null;
                    byte[] idl;
                    using(IDLWrapper wrapper2 = GetCurrentPIDL()) {
                        CurrentTab.CurrentIDL = idl = wrapper2.IDL;
                        if(flag) {
                            if((!NavigatedByCode && (idl != null)) && (idl.Length > 0)) {
                                path = path + "*?*?*" + hash;
                                QTUtility.ITEMIDLIST_Dic_Session[path] = idl;
                                CurrentTab.CurrentPath = CurrentAddress = path;
                            }
                        }
                        else if((flag2 && wrapper2.Available) && !CurrentTab.CurrentPath.Contains("???")) {
                            string str2;
                            int num2;
                            if(IDLWrapper.GetIDLHash(wrapper2.PIDL, out num2, out str2)) {
                                hash = num2;
                                CurrentTab.CurrentPath = CurrentAddress = path = str2;
                            }
                            else if((idl != null) && (idl.Length > 0)) {
                                hash = num2;
                                path = path + "???" + hash;
                                IDLWrapper.AddCache(path, idl);
                                CurrentTab.CurrentPath = CurrentAddress = path;
                            }
                        }
                        if(!NavigatedByCode) {
                            CurrentTab.NavigatedTo(CurrentAddress, idl, hash, fAutoNavigating);
                        }
                    }
                    SyncTravelState();
                    if(QTUtility.IsXP) {
                        if(CurrentAddress.StartsWith(QTUtility.PATH_SEARCHFOLDER)) {
                            ShowSearchBar(true);
                        }
                        else if(QTUtility.fExplorerPrevented || QTUtility.fRestoreFolderTree) {
                            if(!Config.NoNewWndFolderTree || QTUtility.fRestoreFolderTree) {
                                ShowFolderTree(true);
                            }
                            QTUtility.fExplorerPrevented = QTUtility.fRestoreFolderTree = false;
                        }
                    }
                    if(CurrentAddress.StartsWith("::")) {
                        CurrentTab.ToolTipText = CurrentTab.Text;
                        QTUtility.DisplayNameCacheDic[CurrentAddress] = CurrentTab.Text;
                    }
                    else if(flag2) {
                        CurrentTab.ToolTipText = (string)URL;
                    }
                    else if(((CurrentAddress.Length == 3) || CurrentAddress.StartsWith(@"\\")) || (CurrentAddress.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || CurrentAddress.StartsWith("ftp://", StringComparison.OrdinalIgnoreCase))) {
                        CurrentTab.ToolTipText = CurrentTab.CurrentPath;
                        QTUtility.DisplayNameCacheDic[CurrentAddress] = CurrentTab.Text;
                    }
                    else {
                        CurrentTab.ToolTipText = CurrentTab.CurrentPath;
                    }
                    if(NavigatedByCode && !NowTabCreated) {
                        string str3;
                        Address[] selectedItemsAt = CurrentTab.GetSelectedItemsAt(CurrentAddress, out str3);
                        if(selectedItemsAt != null) {
                            ShellBrowser.TrySetSelection(selectedItemsAt, str3, true);
                        }
                    }
                    else if(!string.IsNullOrEmpty(QTUtility.PathToSelectInCommandLineArg)) {
                        Address[] addresses = new Address[] { new Address(QTUtility.PathToSelectInCommandLineArg) };
                        ShellBrowser.TrySetSelection(addresses, null, true);
                        QTUtility.PathToSelectInCommandLineArg = string.Empty;
                    }
                    if(QTUtility.RestoreFolderTree_Hide) {
                        new WaitTimeoutCallback(WaitTimeout).BeginInvoke(150, AsyncComplete_FolderTree, false);
                    }
                    if(fNowRestoring) {
                        fNowRestoring = false;
                        if(QTUtility.LockedTabsToRestoreList.Contains(path)) {
                            CurrentTab.TabLocked = true;
                        }
                    }
                    if((!QTUtility.IsXP || FirstNavigationCompleted) && (!PInvoke.IsWindowVisible(ExplorerHandle) || PInvoke.IsIconic(ExplorerHandle))) {
                        WindowUtils.BringExplorerToFront(ExplorerHandle);
                    }
                    if(pluginManager != null) {
                        pluginManager.OnNavigationComplete(tabControl1.SelectedIndex, idl, (string)URL);
                    }
                    if(buttonNavHistoryMenu.DropDown.Visible) {
                        buttonNavHistoryMenu.DropDown.Close(ToolStripDropDownCloseReason.AppFocusChange);
                    }
                    if(Config.Misc.AutoUpdate) {
                        UpdateChecker.Check(false);
                    }
                }
                catch(Exception exception) {
                    QTUtility2.MakeErrorLog(exception);
                }
                finally {
                    QTUtility.RestoreFolderTree_Hide = NavigatedByCode = fNavigatedByTabSelection = NowTabCreated = fNowTravelByTree = false;
                    tabControl1.SetRedraw(true);
                    FirstNavigationCompleted = true;
                }
            }
        }

        private bool explorerController_MessageCaptured(ref Message msg) {
            if(msg.Msg != WM.CLOSE) {
                iSequential_WM_CLOSE = 0;
            }
            
            if(msg.Msg == WM_BROWSEOBJECT) {
                SBSP flags = (SBSP)Marshal.ReadInt32(msg.WParam);
                if((flags & SBSP.NAVIGATEBACK) != 0) {
                    msg.Result = (IntPtr)1;
                    if(!NavigateCurrentTab(true) && CloseTab(CurrentTab, true) && tabControl1.TabCount == 0) {
                        WindowUtils.CloseExplorer(ExplorerHandle, 2);
                    }
                }
                else if((flags & SBSP.NAVIGATEFORWARD) != 0) {
                    msg.Result = (IntPtr)1;
                    NavigateCurrentTab(false);
                }
                else {
                    IntPtr pidl = IntPtr.Zero;
                    if(msg.LParam != IntPtr.Zero) {
                        pidl = PInvoke.ILClone(msg.LParam);
                    }
                    bool autonav = (flags & SBSP.AUTONAVIGATE) != 0;
                    using(IDLWrapper wrapper = new IDLWrapper(pidl)) {
                        msg.Result = (IntPtr)(BeforeNavigate(wrapper, autonav) ? 1 : 0);
                    }
                }
                return true;
            }
            else if(msg.Msg == WM_HEADERINALLVIEWS) {
                msg.Result = (IntPtr)(Config.Tweaks.AlwaysShowHeaders ? 1 : 0);
                return true;
            }
            else if(msg.Msg == WM_SHOWHIDEBARS) {
                // Todo: hardcoding = bad
                object pvaTabBar = new Guid("{d2bf470e-ed1c-487f-a333-2bd8835eb6ce}").ToString("B");
                object pvaButtonBar = new Guid("{d2bf470e-ed1c-487f-a666-2bd8835eb6ce}").ToString("B");
                object pvarShow = (msg.WParam != IntPtr.Zero);
                object pvarSize = null;
                try {
                    Explorer.ShowBrowserBar(pvaTabBar, pvarShow, pvarSize);
                    Explorer.ShowBrowserBar(pvaButtonBar, pvarShow, pvarSize);
                    msg.Result = (IntPtr)1;
                }
                catch(COMException) {
                }
                return true;
            }
            else if(msg.Msg == WM_NEWWINDOW) {
                if(fNeedsNewWindowPulse && msg.LParam != IntPtr.Zero) {
                    Marshal.WriteIntPtr(msg.LParam, Marshal.GetIDispatchForObject(Explorer));
                    msg.Result = (IntPtr)1;
                    fNeedsNewWindowPulse = false;
                }
                return true;
            }

            switch(msg.Msg) {
                case WM.SETTINGCHANGE:
                    if(QTUtility.IsXP) {
                        QTUtility.GetShellClickMode();
                    }
                    if(Marshal.PtrToStringUni(msg.LParam) == "Environment") {
                        QTUtility.fRequiredRefresh_App = true;
                        SyncTaskBarMenu();
                    }
                    return false;

                case WM.NCLBUTTONDOWN:
                case WM.NCRBUTTONDOWN:
                    HideTabSwitcher(false);
                    return false;

                case WM.MOVE:
                case WM.SIZE:
                    listView.HideThumbnailTooltip(0);
                    listView.HideSubDirTip(0);
                    return false;

                case WM.ACTIVATE: {
                    int num3 = ((int) msg.WParam) & 0xffff;
                    if(num3 > 0) {
                        QTUtility.RegisterPrimaryInstance(ExplorerHandle, this);
                        if(fNowInTray && notifyIcon != null && dicNotifyIcon != null &&
                                dicNotifyIcon.ContainsKey(ExplorerHandle)) {
                            ShowTaskbarItem(ExplorerHandle, true);
                        }
                        fNowInTray = false;
                    }
                    else {
                        listView.HideThumbnailTooltip(1);
                        listView.HideSubDirTip_ExplorerInactivated();
                        HideTabSwitcher(false);
                        if(tabControl1.Focused) {
                            listView.SetFocus();
                        }
                        if((Config.Tabs.ShowCloseButtons &&
                                Config.Tabs.CloseBtnsWithAlt) &&
                                        tabControl1.EnableCloseButton) {
                            tabControl1.EnableCloseButton = false;
                            tabControl1.Refresh();
                        }
                    }
                    return false;
                }
                case WM.CLOSE:
                    if(iSequential_WM_CLOSE > 0) {
                        return true;
                    }
                    iSequential_WM_CLOSE++;
                    return HandleCLOSE(msg.LParam);

                case WM.NCMBUTTONDOWN:
                case WM.NCXBUTTONDOWN:
                    HideTabSwitcher(false);
                    return false;

                case WM.SYSCOMMAND:
                    if((((int) msg.WParam) & 0xfff0) == 0xf020) {
                        if(pluginManager != null) {
                            pluginManager.OnExplorerStateChanged(ExplorerWindowActions.Minimized);
                        }
                        if(Config.Window.TrayOnMinimize) {
                            fNowInTray = true;
                            ShowTaskbarItem(ExplorerHandle, false);
                            return true;
                        }
                        return false;
                    }
                    if((((int) msg.WParam) & 0xfff0) == 0xf030) {
                        if(pluginManager != null) {
                            pluginManager.OnExplorerStateChanged(ExplorerWindowActions.Maximized);
                        }
                        return false;
                    }
                    if((((int) msg.WParam) & 0xfff0) == 0xf120) {
                        if(pluginManager != null) {
                            pluginManager.OnExplorerStateChanged(ExplorerWindowActions.Restored);
                        }
                        return false;
                    }
                    if((Config.Window.TrayOnClose &&
                            ((((int) msg.WParam) == 0xf060) || (((int) msg.WParam) == 0xf063))) &&
                                    (ModifierKeys != Keys.Shift)) {
                        fNowInTray = true;
                        ShowTaskbarItem(ExplorerHandle, false);
                        return true;
                    }
                    if(!QTUtility.IsXP || ((((int) msg.WParam) != 0xf060) && (((int) msg.WParam) != 0xf063))) {
                        return false;
                    }
                    WindowUtils.CloseExplorer(ExplorerHandle, 3);
                    return true;

                case WM.POWERBROADCAST:
                    if(((int) msg.WParam) == 7) {
                        OnAwake();
                    }
                    return false;

                case WM.DEVICECHANGE:
                    if(((int) msg.WParam) == 0x8004) {
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
                                num5 = (ushort) (num5 + 1);
                            }
                            num5 = (ushort) (num5 + 0x41);
                            string str = ((char) num5) + @":\";
                            CloseTabs(tabControl1.TabPages.Where(item =>
                                    ((QTabItem)item).CurrentPath.PathStartsWith(str)).ToList(), true);
                            if(tabControl1.TabCount == 0) {
                                WindowUtils.CloseExplorer(ExplorerHandle, 2);
                            }
                        }
                    }
                    return false;

                case WM.PARENTNOTIFY:
                    switch((((int)msg.WParam) & 0xffff)) {
                        case WM.LBUTTONDOWN:
                        case WM.RBUTTONDOWN:
                        case WM.MBUTTONDOWN:
                        case WM.XBUTTONDOWN:
                            HideTabSwitcher(false);
                            break;
                    }
                    return false;

                case WM.APPCOMMAND: {
                    int num = ((((int)((long)msg.LParam)) >> 0x10) & 0xffff) & -61441;
                    int num2 = ((((int)((long)msg.LParam)) >> 0x10) & 0xffff) & 0xf000;
                    bool flag = (num2 != 0x8000) || Config.CaptureX1X2;
                    switch(num) {
                        case 1:
                            if(flag) {
                                NavigateCurrentTab(true);
                            }
                            return true;

                        case 2:
                            if(flag) {
                                NavigateCurrentTab(false);
                            }
                            return true;

                        case 0x1f:
                            WindowUtils.CloseExplorer(ExplorerHandle, 0);
                            return true;
                    }
                    return false;
                }
            }
            return false;
        }

        public override void GetBandInfo(uint dwBandID, uint dwViewMode, ref DESKBANDINFO dbi) {
            if((dbi.dwMask & DBIM.ACTUAL) != (0)) {
                dbi.ptActual.X = Size.Width;
                dbi.ptActual.Y = BandHeight;
            }
            if((dbi.dwMask & DBIM.INTEGRAL) != (0)) {
                dbi.ptIntegral.X = -1;
                dbi.ptIntegral.Y = 10;
            }
            if((dbi.dwMask & DBIM.MAXSIZE) != (0)) {
                dbi.ptMaxSize.X = -1;
                dbi.ptMaxSize.Y = BandHeight;
            }
            if((dbi.dwMask & DBIM.MINSIZE) != (0)) {
                dbi.ptMinSize.X = MinSize.Width;
                dbi.ptMinSize.Y = BandHeight;
            }
            if((dbi.dwMask & DBIM.MODEFLAGS) != (0)) {
                dbi.dwModeFlags = DBIMF.NORMAL;
            }
            if((dbi.dwMask & DBIM.BKCOLOR) != (0)) {
                dbi.dwMask &= ~DBIM.BKCOLOR;
            }
            if((dbi.dwMask & DBIM.TITLE) != (0)) {
                dbi.wszTitle = null;
            }
        }

        private ITravelLogEntry GetCurrentLogEntry() {
            IEnumTravelLogEntry ppenum = null;
            ITravelLogEntry rgElt = null;
            ITravelLogEntry entry3;
            try {
                if(TravelLog.EnumEntries(1, out ppenum) == 0) {
                    ppenum.Next(1, out rgElt, 0);
                }
                entry3 = rgElt;
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception);
                entry3 = null;
            }
            finally {
                if(ppenum != null) {
                    Marshal.ReleaseComObject(ppenum);
                }
            }
            return entry3;
        }

        internal IDLWrapper GetCurrentPIDL() {
            IDLWrapper wrapper = ShellBrowser.GetShellPath();
            if(!wrapper.Available) {
                wrapper.Dispose();
                wrapper = new IDLWrapper(ShellMethods.ShellGetPath2(ExplorerHandle));
                if(!wrapper.Available) {
                    wrapper.Dispose();
                    wrapper = new IDLWrapper(lastCompletedBrowseObjectIDL);
                }
            }
            return wrapper;
        }

        private Cursor GetCursor(bool fDragging) {
            return fDragging ?
                    curTabDrag ?? (curTabDrag = CreateCursor(Resources_Image.imgCurTabDrag)) :
                    curTabCloning ?? (curTabCloning = CreateCursor(Resources_Image.imgCurTabCloning));
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

        internal PluginManager GetPluginManager() {
            return pluginManager;
        }

        private IntPtr GetSearchBand_Edit() {
            IntPtr hwndSearchBand = WindowUtils.FindChildWindow(ExplorerHandle, hwnd => PInvoke.GetClassName(hwnd) == "UniversalSearchBand");
            if(hwndSearchBand != IntPtr.Zero) {
                hwndSearchBand = WindowUtils.FindChildWindow(hwndSearchBand, hwnd =>
                        PInvoke.GetClassName(hwnd) == "Edit" && ((int)PInvoke.GetWindowLongPtr(hwnd, -16) & 0x10000000) != 0);
            }
            return hwndSearchBand;
        }

        public ShellBrowserEx GetShellBrowser() {
            return ShellBrowser;
        }

        private IntPtr GetTravelToolBarWindow32() {
            IntPtr hwndTravelBand = WindowUtils.FindChildWindow(ExplorerHandle, hwnd => PInvoke.GetClassName(hwnd) == "TravelBand");
            return hwndTravelBand != IntPtr.Zero 
                    ? PInvoke.FindWindowEx(hwndTravelBand, IntPtr.Zero, "ToolbarWindow32", null) 
                    : IntPtr.Zero;
        }
        
        // TODO
        private void Handle_MButtonUp_Tree(MSG msg) {
            IntPtr ptr;
            if(ShellBrowser.IsFolderTreeVisible(out ptr) && msg.hwnd == ptr) {
                TVHITTESTINFO structure = new TVHITTESTINFO {pt = QTUtility2.PointFromLPARAM(msg.lParam)};
                IntPtr wParam = PInvoke.SendMessage(ptr, 0x1111, IntPtr.Zero, ref structure);
                if(wParam != IntPtr.Zero) {
                    int num = (int)PInvoke.SendMessage(ptr, 0x1127, wParam, (IntPtr)2);
                    if((num & 2) == 0) {
                        NavigatedByCode = fNowTravelByTree = true;
                        PInvoke.SendMessage(ptr, 0x110b, (IntPtr)9, wParam);
                    }
                }
            }
        }

        private bool HandleCLOSE(IntPtr lParam) {
            bool flag = Config.Window.CloseBtnClosesSingleTab;
            bool flag2 = Config.Window.CloseBtnClosesUnlocked;
            List<string> closingPaths = new List<string>();
            int num = (int)lParam;
            switch(num) {
                case 1:
                    closingPaths = CloseAllTabsExcept(null, flag2);
                    if(tabControl1.TabCount > 0) {
                        return true;
                    }
                    break;
                    
                case 2:
                    return false;

                default: {
                        bool flag3 = QTUtility2.PathExists(CurrentTab.CurrentPath);
                        if((QTUtility.IsXP && flag3) && (num == 0)) {
                            return true;
                        }
                        if(!flag3) {
                            CloseTab(CurrentTab, true);
                            return (tabControl1.TabCount > 0);
                        }
                        if(flag2 && !flag) {
                            closingPaths = CloseAllTabsExcept(null);
                            if(tabControl1.TabCount > 0) {
                                return true;
                            }
                            QTUtility.SaveClosing(closingPaths);
                            return false;
                        }
                        Keys modifierKeys = ModifierKeys;
                        if((modifierKeys == (Keys.Control | Keys.Shift)) || !flag) {
                            foreach(QTabItem item2 in tabControl1.TabPages) {
                                closingPaths.Add(item2.CurrentPath);
                                AddToHistory(item2);
                            }
                            QTUtility.SaveClosing(closingPaths);
                            return false;
                        }
                        if(modifierKeys == Keys.Control) {
                            closingPaths = CloseAllTabsExcept(null);
                        }
                        else {
                            closingPaths.Add(CurrentTab.CurrentPath);
                            CloseTab(CurrentTab, false);
                        }
                        if(tabControl1.TabCount > 0) {
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
            strDraggingDrive = (strDraggingStartPath = null);
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
                    if(!QTUtility2.MakeRootName(str2).PathEquals(b)) {
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

        private void HandleF5() {
            IntPtr ptr;
            if(InstanceManager.TryGetButtonBarHandle(ExplorerHandle, out ptr)) {
                QTUtility2.SendCOPYDATASTRUCT(ptr, (IntPtr)9, "browser_refresh", IntPtr.Zero);
            }
        }

        private void HandleFileDrop(IntPtr hDrop) {
            HideToolTipForDD();
            int capacity = (int)PInvoke.DragQueryFile(hDrop, uint.MaxValue, null, 0);
            if(capacity >= 1) {
                List<string> listDroppedPaths = new List<string>(capacity);
                for(int i = 0; i < capacity; i++) {
                    StringBuilder lpszFile = new StringBuilder(260);
                    PInvoke.DragQueryFile(hDrop, (uint)i, lpszFile, lpszFile.Capacity);
                    listDroppedPaths.Add(lpszFile.ToString());
                }
                OpenDroppedFolder(listDroppedPaths);
            }
        }

        private bool HandleKEYDOWN(Keys key, bool fRepeat) {
            Keys mkey = key | ModifierKeys;

            switch(key) {
                case Keys.Enter:
                    return false;

                case Keys.Menu:
                    if(!fRepeat && Config.Tabs.ShowCloseButtons && Config.Tabs.CloseBtnsWithAlt) {
                        tabControl1.ShowCloseButton(true);
                        if(!QTUtility.IsXP) return Config.HideMenuBar;
                    }
                    return false;

                case Keys.ControlKey:
                    if(!fRepeat && NowTabDragging && DraggingTab != null && tabControl1.GetTabMouseOn() == null) {
                        Cursor = GetCursor(false);
                    }
                    break;

                case Keys.Tab:
                    if(!Config.NoTabSwitcher && (mkey & Keys.Control) != Keys.None) {
                        return ShowTabSwitcher((mkey & Keys.Shift) != Keys.None, fRepeat);
                    }
                    break;
            }

            switch(mkey) {
                case Keys.Back:
                    if(!QTUtility.IsXP) {
                        if(listView.HasFocus()) {
                            if(!fRepeat) {
                                if(Config.Tweaks.BackspaceUpLevel) {
                                    UpOneLevel();
                                }
                                else {
                                    NavigateCurrentTab(true);
                                }
                            }
                            return true;
                        }
                    }
                    return false;

                case Keys.Alt | Keys.Left:
                    NavigateCurrentTab(true);
                    return true;

                case Keys.Alt | Keys.Right:
                    NavigateCurrentTab(false);
                    return true;

                case Keys.Alt | Keys.F4:
                    if(!fRepeat) {
                        WindowUtils.CloseExplorer(ExplorerHandle, 1);
                    }
                    return true;

                case Keys.F2:
                    if(!Config.Tweaks.F2Selection) {
                        listView.HandleF2();
                    }
                    return false;

            }

            // Ctrl+number = switch to tab #n
            if(((Keys.Control | Keys.NumPad0) <= mkey && mkey <= (Keys.Control | Keys.NumPad9)) ||
                    ((Keys.Control | Keys.D0) <= mkey && mkey <= (Keys.Control | Keys.D9))) {
                int digit;
                if(mkey >= (Keys.Control | Keys.NumPad0)) {
                    digit = (mkey - (Keys.Control | Keys.NumPad0));
                }
                else {
                    digit = (mkey - (Keys.Control | Keys.D0));
                }
                if(digit == 0) {
                    digit = 10;
                }
                if(tabControl1.TabCount >= digit) {
                    tabControl1.SelectTab(digit - 1);
                }
                return true;
            }

            // Check for hotkeys
            int imkey = (int)mkey | QTUtility.FLAG_KEYENABLED;
            for(int i = 0; i < Config.Keys.Shortcuts.Length; ++i) {
                if(Config.Keys.Shortcuts[i] == imkey) {
                    return DoBindAction((BindAction)i);
                }
            }

            // Check for plugin hotkeys.
            int idx = -1;
            KeyValuePair<string, int[]> pair = QTUtility.dicPluginShortcutKeys
                        .FirstOrDefault(p => (idx = Array.IndexOf(p.Value, imkey)) != -1);
            if(idx != -1) {
                Plugin plugin;
                if(pluginManager.TryGetPlugin(pair.Key, out plugin)) {
                    try {
                        plugin.Instance.OnShortcutKeyPressed(idx);
                    }
                    catch(Exception exception) {
                        PluginManager.HandlePluginException(exception,
                                ExplorerHandle, plugin.PluginInformation.Name,
                                "On shortcut key pressed. Index is " + idx);
                    }
                    return true;
                }
                return false;
            }

            // Check for app hotkeys
            if(!fRepeat && QTUtility.dicUserAppShortcutKeys.ContainsKey(imkey)) {
                MenuItemArguments mia = QTUtility.dicUserAppShortcutKeys[imkey];
                try {
                    using(IDLWrapper wrapper2 = GetCurrentPIDL()) {
                        Address[] addressArray;
                        string str2;
                        if(wrapper2.Available && wrapper2.HasPath && ShellBrowser.TryGetSelection(out addressArray, out str2, false)) {
                            AppLauncher launcher = new AppLauncher(addressArray, wrapper2.Path);
                            launcher.ReplaceTokens_WorkingDir(mia);
                            launcher.ReplaceTokens_Arguments(mia);
                        }
                    }
                    AppLauncher.Execute(mia, ExplorerHandle);
                }
                catch(Exception exception2) {
                    QTUtility2.MakeErrorLog(exception2);
                }
                finally {
                    mia.RestoreOriginalArgs();
                }
                return true;
            }

            // Check for group hotkey
            if(!fRepeat) {
                foreach(Group g in GroupsManager.Groups.Where(g => g.ShortcutKey == mkey)) {
                    OpenGroup(g.Name, false);
                    return true;
                }    
            }

            // This is important I guess?  Not sure
            if(mkey == (Keys.Control | Keys.W)) return true;

            return false;
        }

        // TODO
        private void HandleLBUTTON_Tree(MSG msg, bool fMouseDown) {
            IntPtr ptr;
            if(ShellBrowser.IsFolderTreeVisible(out ptr) && msg.hwnd == ptr) {
                TVHITTESTINFO structure = new TVHITTESTINFO {pt = QTUtility2.PointFromLPARAM(msg.lParam)};
                IntPtr wParam = PInvoke.SendMessage(ptr, 0x1111, IntPtr.Zero, ref structure);
                if(wParam != IntPtr.Zero) {
                    bool flag;
                    if(fMouseDown) {
                        flag = (((structure.flags != 1) && (structure.flags != 0x10)) && ((structure.flags & 2) == 0)) && ((structure.flags & 4) == 0);
                    }
                    else {
                        flag = ((structure.flags & 2) != 0) || ((structure.flags & 4) != 0);
                    }
                    if(flag) {
                        int num = (int)PInvoke.SendMessage(ptr, 0x1127, wParam, (IntPtr)2);
                        if((num & 2) == 0) {
                            NavigatedByCode = fNowTravelByTree = true;
                        }
                    }
                }
            }
        }        

        private bool HandleMOUSEWHEEL(IntPtr lParam) {
            if(!IsHandleCreated) {
                return false;
            }
            MOUSEHOOKSTRUCTEX mousehookstructex = (MOUSEHOOKSTRUCTEX)Marshal.PtrToStructure(lParam, typeof(MOUSEHOOKSTRUCTEX));
            int y = mousehookstructex.mouseData >> 0x10;
            IntPtr handle = PInvoke.WindowFromPoint(mousehookstructex.mhs.pt);
            Control control = FromHandle(handle);
            bool flag = false;
            if(control != null) {
                IntPtr ptr2;
                DropDownMenuReorderable reorderable = control as DropDownMenuReorderable;
                if(reorderable != null) {
                    if(reorderable.CanScroll) {
                        PInvoke.SendMessage(handle, WM.MOUSEWHEEL, QTUtility2.Make_LPARAM(0, y), QTUtility2.Make_LPARAM(mousehookstructex.mhs.pt));
                    }
                    return true;
                }
                flag = (control == tabControl1) || (handle == Handle);
                if(!flag && InstanceManager.TryGetButtonBarHandle(ExplorerHandle, out ptr2)) {
                    flag = (handle == ptr2) || (handle == listView.Handle); // TODO make sure this didn't break
                }
            }
            if(!flag) {
                Keys modifierKeys = ModifierKeys;
                if((!Config.Tweaks.HorizontalScroll && (modifierKeys == Keys.Shift)) || ((QTUtility.IsXP && !Config.CtrlWheelChangeView) && (modifierKeys == Keys.Control))) {
                    if(listView.MouseIsOverListView()) {
                        switch(modifierKeys) {
                            case Keys.Shift:
                                listView.ScrollHorizontal(y);
                                return true;

                            case Keys.Control:
                                ChangeViewMode(y > 0);
                                return true;
                        }
                    }
                }
                return false;
            }
            if(((tabControl1.TabCount < 2) || (ExplorerHandle != PInvoke.GetForegroundWindow())) || Explorer.Busy) {
                return false;
            }
            int selectedIndex = tabControl1.SelectedIndex;
            if(y < 0) {
                if(selectedIndex == (tabControl1.TabCount - 1)) {
                    tabControl1.SelectedIndex = 0;
                }
                else {
                    tabControl1.SelectedIndex = selectedIndex + 1;
                }
            }
            else if(selectedIndex < 1) {
                tabControl1.SelectedIndex = tabControl1.TabCount - 1;
            }
            else {
                tabControl1.SelectedIndex = selectedIndex - 1;
            }
            return true;
        }

        // todo: clean this crap up...
        private bool HandleItemActivate(Keys modKeys, bool fEnqExec) {
            IntPtr zero = IntPtr.Zero;
            IntPtr ppidl = IntPtr.Zero;
            try {
                Address[] addressArray;
                IDLWrapper wrapper1;
                bool fOpenFirstInTab;
                string str;
                if(ShellBrowser.TryGetSelection(out addressArray, out str, false) && (addressArray.Length > 0)) {
                    List<Address> list = new List<Address>(addressArray);
                    wrapper1 = new IDLWrapper(list[0]);
                    list.RemoveAt(0);
                    addressArray = list.ToArray();
                    fOpenFirstInTab = (addressArray.Length > 0) || (modKeys == Keys.Shift);
                }
                else {
                    return false;
                }
                using(IDLWrapper wrapper = wrapper1) {
                    if((wrapper.Available && wrapper.HasPath) && wrapper.IsReadyIfDrive) {
                        if(wrapper.IsFolder) {
                            if(modKeys == Keys.Control) {
                                if(!wrapper.IsLinkToDeadFolder) {
                                    QTUtility.TMPPathList.AddRange(CreateTMPPathsToOpenNew(addressArray, wrapper.Path));
                                    OpenNewWindow(wrapper);
                                }
                                else {
                                    SystemSounds.Hand.Play();
                                }
                            }
                            else if(modKeys == (Keys.Alt | Keys.Control | Keys.Shift)) {
                                DirectoryInfo info = new DirectoryInfo(wrapper.Path);
                                if(info.Exists) {
                                    DirectoryInfo[] directories = info.GetDirectories();
                                    if((directories.Length + tabControl1.TabCount) < 0x41) {
                                        tabControl1.SetRedraw(false);
                                        foreach(DirectoryInfo info2 in directories) {
                                            if(info2.Name != "System Volume Information") {
                                                using(IDLWrapper wrapper2 = new IDLWrapper(info2.FullName)) {
                                                    if(wrapper2.Available && (!wrapper2.IsLink || Directory.Exists(ShellMethods.GetLinkTargetPath(info2.FullName)))) {
                                                        OpenNewTab(wrapper2, true, false);
                                                    }
                                                }
                                            }
                                        }
                                        tabControl1.SetRedraw(true);
                                    }
                                    else {
                                        SystemSounds.Hand.Play();
                                    }
                                }
                            }
                            else {
                                if(addressArray.Length > 1) {
                                    tabControl1.SetRedraw(false);
                                }
                                try {
                                    if(fOpenFirstInTab) {
                                        OpenNewTab(wrapper, (modKeys & Keys.Shift) == Keys.Shift, false);
                                    }
                                    else if(!wrapper.IsFileSystemFile) {
                                        ShellBrowser.Navigate(wrapper);
                                    }
                                    else {
                                        return false;
                                    }
                                    for(int i = 0; i < addressArray.Length; i++) {
                                        using(IDLWrapper wrapper3 = new IDLWrapper(addressArray[i].ITEMIDLIST)) {
                                            if(((wrapper3.Available && wrapper3.HasPath) && (wrapper3.IsReadyIfDrive && wrapper3.IsFolder)) && !wrapper3.IsLinkToDeadFolder) {
                                                string path = wrapper3.Path;
                                                if(((path != wrapper.Path) && (path.Length > 0)) && !QTUtility2.IsShellPathButNotFileSystem(path)) {
                                                    OpenNewTab(wrapper3, true, false);
                                                }
                                            }
                                        }
                                    }
                                }
                                finally {
                                    if(addressArray.Length > 1) {
                                        tabControl1.SetRedraw(true);
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
                                        OpenNewWindow(wrapper4);
                                    }
                                    else {
                                        if(fOpenFirstInTab) {
                                            OpenNewTab(wrapper4, (modKeys & Keys.Shift) == Keys.Shift, false);
                                        }
                                        else {
                                            ShellBrowser.Navigate(wrapper4);
                                        }
                                        for(int j = 0; j < addressArray.Length; j++) {
                                            using(IDLWrapper wrapper5 = new IDLWrapper(addressArray[j].ITEMIDLIST)) {
                                                if(((wrapper5.Available && wrapper5.HasPath) && (wrapper5.IsReadyIfDrive && wrapper5.IsFolder)) && !wrapper5.IsLinkToDeadFolder) {
                                                    string str3 = wrapper5.Path;
                                                    if(((str3 != wrapper4.Path) && (str3.Length > 0)) && !QTUtility2.IsShellPathButNotFileSystem(str3)) {
                                                        OpenNewTab(wrapper5, true, false);
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
                QTUtility2.MakeErrorLog(exception);
            }
            finally {
                if(zero != IntPtr.Zero) {
                    PInvoke.CoTaskMemFree(zero);
                }
                if(ppidl != IntPtr.Zero) {
                    PInvoke.CoTaskMemFree(ppidl);
                }
            }
            return false;
        }

        private void HideSubDirTip_Tab_Menu() {
            if(subDirTip_Tab != null) {
                subDirTip_Tab.HideMenu();
            }
        }

        private void HideTabSwitcher(bool fSwitch) {
            if((tabSwitcher != null) && tabSwitcher.IsShown) {
                tabSwitcher.HideSwitcher(fSwitch);
                tabControl1.SetPseudoHotIndex(-1);
            }
        }

        private void HideToolTipForDD() {
            tabForDD = null;
            iModKeyStateDD = 0;
            if(toolTipForDD != null) {
                toolTipForDD.Hide(tabControl1);
            }
            if(timerOnTab != null) {
                timerOnTab.Enabled = false;
            }
        }

        private void InitializeComponent() {
            components = new Container();
            buttonNavHistoryMenu = new ToolStripDropDownButton();
            tabControl1 = new QTabControl();
            CurrentTab = new QTabItem(string.Empty, string.Empty, tabControl1);
            contextMenuTab = new ContextMenuStripEx(components, false);
            contextMenuSys = new ContextMenuStripEx(components, false);
            tabControl1.SuspendLayout();
            contextMenuSys.SuspendLayout();
            contextMenuTab.SuspendLayout();
            SuspendLayout();
            bool flag = Config.Tabs.ShowNavButtons;
            if(flag) {
                InitializeNavBtns(false);
            }
            buttonNavHistoryMenu.AutoSize = false;
            buttonNavHistoryMenu.DisplayStyle = ToolStripItemDisplayStyle.None;
            buttonNavHistoryMenu.Enabled = false;
            buttonNavHistoryMenu.Size = new Size(13, 0x15);
            buttonNavHistoryMenu.DropDown = new DropDownMenuBase(components, true, true, true);
            buttonNavHistoryMenu.DropDown.ItemClicked += NavigationButton_DropDownMenu_ItemClicked;
            buttonNavHistoryMenu.DropDownOpening += NavigationButtons_DropDownOpening;
            buttonNavHistoryMenu.DropDown.ImageList = QTUtility.ImageListGlobal;
            tabControl1.SetRedraw(false);
            tabControl1.TabPages.Add(CurrentTab);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.ContextMenuStrip = contextMenuTab;
            tabControl1.RefreshOptions(true);
            tabControl1.RowCountChanged += tabControl1_RowCountChanged;
            tabControl1.Deselecting += tabControl1_Deselecting;
            tabControl1.Selecting += tabControl1_Selecting;
            tabControl1.SelectedIndexChanged += tabControl1_SelectedIndexChanged;
            tabControl1.GotFocus += Controls_GotFocus;
            tabControl1.MouseEnter += tabControl1_MouseEnter;
            tabControl1.MouseLeave += tabControl1_MouseLeave;
            tabControl1.MouseDown += tabControl1_MouseDown;
            tabControl1.MouseUp += tabControl1_MouseUp;
            tabControl1.MouseMove += tabControl1_MouseMove;
            tabControl1.MouseDoubleClick += tabControl1_MouseDoubleClick;
            tabControl1.ItemDrag += tabControl1_ItemDrag;
            tabControl1.PointedTabChanged += tabControl1_PointedTabChanged;
            tabControl1.TabCountChanged += tabControl1_TabCountChanged;
            tabControl1.CloseButtonClicked += tabControl1_CloseButtonClicked;
            tabControl1.TabIconMouseDown += tabControl1_TabIconMouseDown;
            contextMenuTab.Items.Add(new ToolStripMenuItem());
            contextMenuTab.ShowImageMargin = false;
            contextMenuTab.ItemClicked += contextMenuTab_ItemClicked;
            contextMenuTab.Opening += contextMenuTab_Opening;
            contextMenuTab.Closed += contextMenuTab_Closed;
            contextMenuSys.Items.Add(new ToolStripMenuItem());
            contextMenuSys.ShowImageMargin = false;
            contextMenuSys.ItemClicked += contextMenuSys_ItemClicked;
            contextMenuSys.Opening += contextMenuSys_Opening;
            Controls.Add(tabControl1);
            if(flag) {
                Controls.Add(toolStrip);
            }
            MinSize = new Size(150, Config.Skin.TabHeight + 2);
            Height = Config.Skin.TabHeight + 2;
            ContextMenuStrip = contextMenuSys;
            MouseDoubleClick += QTTabBarClass_MouseDoubleClick;
            MouseUp += QTTabBarClass_MouseUp;
            tabControl1.ResumeLayout(false);
            contextMenuSys.ResumeLayout(false);
            contextMenuTab.ResumeLayout(false);
            if(flag) {
                toolStrip.ResumeLayout(false);
                toolStrip.PerformLayout();
            }
            ResumeLayout(false);
        }

        private void InitializeInstallation() {
            InitializeOpenedWindow();
            object locationURL = Explorer.LocationURL;
            if(ShellBrowser != null) {
                using(IDLWrapper wrapper = ShellBrowser.GetShellPath()) {
                    if(wrapper.Available) {
                        locationURL = wrapper.Path;
                    }
                }
            }
            Explorer_NavigateComplete2(null, ref locationURL);
        }

        private void InitializeNavBtns(bool fSync) {
            toolStrip = new ToolStripEx();
            buttonBack = new ToolStripButton();
            buttonForward = new ToolStripButton();
            toolStrip.SuspendLayout();
            if(!QTUtility.ImageListGlobal.Images.ContainsKey("navBack")) {
                QTUtility.ImageListGlobal.Images.Add("navBack", Resources_Image.imgNavBack);
            }
            if(!QTUtility.ImageListGlobal.Images.ContainsKey("navFrwd")) {
                QTUtility.ImageListGlobal.Images.Add("navFrwd", Resources_Image.imgNavFwd);
            }
            toolStrip.Dock = Config.Tabs.NavButtonsOnRight ? DockStyle.Right : DockStyle.Left;
            toolStrip.AutoSize = false;
            toolStrip.CanOverflow = false;
            toolStrip.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip.Items.AddRange(new ToolStripItem[] { buttonBack, buttonForward, buttonNavHistoryMenu });
            toolStrip.Renderer = new ToolbarRenderer();
            toolStrip.Width = 0x3f;
            toolStrip.TabStop = false;
            toolStrip.BackColor = Color.Transparent;
            buttonBack.AutoSize = false;
            buttonBack.DisplayStyle = ToolStripItemDisplayStyle.Image;
            buttonBack.Enabled = fSync ? ((navBtnsFlag & 1) != 0) : false;
            buttonBack.Image = QTUtility.ImageListGlobal.Images["navBack"];
            buttonBack.Size = new Size(0x15, 0x15);
            buttonBack.Click += NavigationButtons_Click;
            buttonForward.AutoSize = false;
            buttonForward.DisplayStyle = ToolStripItemDisplayStyle.Image;
            buttonForward.Enabled = fSync ? ((navBtnsFlag & 2) != 0) : false;
            buttonForward.Image = QTUtility.ImageListGlobal.Images["navFrwd"];
            buttonForward.Size = new Size(0x15, 0x15);
            buttonForward.Click += NavigationButtons_Click;
        }

        private void InitializeOpenedWindow() {
            IsShown = true;
            QTUtility.RegisterPrimaryInstance(ExplorerHandle, this);
            InstallHooks();
            pluginManager = new PluginManager(this);
            if(!SyncButtonBarCurrent(0x100)) {
                new WaitTimeoutCallback(WaitTimeout).BeginInvoke(0x7d0, AsyncComplete_ButtonBarPlugin, null);
            }
            if(Config.SaveTransparency && (QTUtility.WindowAlpha < 0xff)) {
                PInvoke.SetWindowLongPtr(ExplorerHandle, -20, PInvoke.Ptr_OP_OR(PInvoke.GetWindowLongPtr(ExplorerHandle, -20), 0x80000));
                PInvoke.SetLayeredWindowAttributes(ExplorerHandle, 0, QTUtility.WindowAlpha, 2);
            }

            listViewManager = new ListViewMonitor(ShellBrowser, ExplorerHandle, Handle);
            listViewManager.ListViewChanged += ListViewMonitor_ListViewChanged;
            listViewManager.Initialize();

            IntPtr hwndBreadcrumbBar = WindowUtils.FindChildWindow(ExplorerHandle, hwnd => PInvoke.GetClassName(hwnd) == "Breadcrumb Parent");
            if(hwndBreadcrumbBar != IntPtr.Zero) {
                hwndBreadcrumbBar = PInvoke.FindWindowEx(hwndBreadcrumbBar, IntPtr.Zero, "ToolbarWindow32", null);
                if(hwndBreadcrumbBar != IntPtr.Zero) {
                    breadcrumbBar = new BreadcrumbBar(hwndBreadcrumbBar);
                    breadcrumbBar.ItemClicked += FolderLinkClicked;
                }
            }
        }

        private static void InitializeStaticFields() {
            fInitialized = true;
            Application.EnableVisualStyles();
        }

        private void InitializeSysMenu(bool fText) {
            bool flag = false;
            if(tsmiGroups == null) {
                flag = true;
                tsmiGroups = new ToolStripMenuItem(QTUtility.ResMain[12]);
                tsmiUndoClose = new ToolStripMenuItem(QTUtility.ResMain[13]);
                tsmiLastActiv = new ToolStripMenuItem(QTUtility.ResMain[14]);
                tsmiExecuted = new ToolStripMenuItem(QTUtility.ResMain[15]);
                tsmiBrowseFolder = new ToolStripMenuItem(QTUtility.ResMain[0x10] + "...");
                tsmiCloseAllButCurrent = new ToolStripMenuItem(QTUtility.ResMain[0x11]);
                tsmiCloseWindow = new ToolStripMenuItem(QTUtility.ResMain[0x12]);
                tsmiOption = new ToolStripMenuItem(QTUtility.ResMain[0x13]);
                tsmiLockToolbar = new ToolStripMenuItem(QTUtility.ResMain[0x20]);
                tsmiMergeWindows = new ToolStripMenuItem(QTUtility.ResMain[0x21]);
                tssep_Sys1 = new ToolStripSeparator();
                tssep_Sys2 = new ToolStripSeparator();
                contextMenuSys.SuspendLayout();
                contextMenuSys.Items[0].Dispose();
                contextMenuSys.Items.AddRange(new ToolStripItem[] { tsmiGroups, tsmiUndoClose, tsmiLastActiv, tsmiExecuted, tssep_Sys1, tsmiBrowseFolder, tsmiCloseAllButCurrent, tsmiCloseWindow, tsmiMergeWindows, tsmiLockToolbar, tssep_Sys2, tsmiOption });
                DropDownMenuReorderable reorderable = new DropDownMenuReorderable(components, true, false);
                reorderable.ReorderFinished += menuitemGroups_ReorderFinished;
                reorderable.ItemRightClicked += MenuUtility.GroupMenu_ItemRightClicked;
                reorderable.ItemMiddleClicked += ddrmrGroups_ItemMiddleClicked;
                reorderable.ImageList = QTUtility.ImageListGlobal;
                tsmiGroups.DropDown = reorderable;
                tsmiGroups.DropDownItemClicked += menuitemGroups_DropDownItemClicked;
                DropDownMenuReorderable reorderable2 = new DropDownMenuReorderable(components);
                reorderable2.ReorderEnabled = false;
                reorderable2.MessageParent = Handle;
                reorderable2.ImageList = QTUtility.ImageListGlobal;
                reorderable2.ItemRightClicked += ddmrUndoClose_ItemRightClicked;
                tsmiUndoClose.DropDown = reorderable2;
                tsmiUndoClose.DropDownItemClicked += menuitemUndoClose_DropDownItemClicked;
                DropDownMenuReorderable reorderable3 = new DropDownMenuReorderable(components);
                reorderable3.MessageParent = Handle;
                reorderable3.ItemRightClicked += menuitemExecuted_ItemRightClicked;
                reorderable3.ItemClicked += menuitemExecuted_DropDownItemClicked;
                reorderable3.ImageList = QTUtility.ImageListGlobal;
                tsmiExecuted.DropDown = reorderable3;
                tssep_Sys1.Enabled = false;
                tssep_Sys2.Enabled = false;
                contextMenuSys.ResumeLayout(false);
            }
            if(!flag && fText) {
                tsmiGroups.Text = QTUtility.ResMain[12];
                tsmiUndoClose.Text = QTUtility.ResMain[13];
                tsmiLastActiv.Text = QTUtility.ResMain[14];
                tsmiExecuted.Text = QTUtility.ResMain[15];
                tsmiBrowseFolder.Text = QTUtility.ResMain[0x10] + "...";
                tsmiCloseAllButCurrent.Text = QTUtility.ResMain[0x11];
                tsmiCloseWindow.Text = QTUtility.ResMain[0x12];
                tsmiOption.Text = QTUtility.ResMain[0x13];
                tsmiLockToolbar.Text = QTUtility.ResMain[0x20];
                tsmiMergeWindows.Text = QTUtility.ResMain[0x21];
            }
        }

        private void InitializeTabMenu(bool fText) {
            bool flag = false;
            if(tsmiClose == null) {
                flag = true;
                tsmiClose = new ToolStripMenuItem(QTUtility.ResMain[0]);
                tsmiCloseRight = new ToolStripMenuItem(QTUtility.ResMain[1]);
                tsmiCloseLeft = new ToolStripMenuItem(QTUtility.ResMain[2]);
                tsmiCloseAllButThis = new ToolStripMenuItem(QTUtility.ResMain[3]);
                tsmiAddToGroup = new ToolStripMenuItem(QTUtility.ResMain[4]);
                tsmiCreateGroup = new ToolStripMenuItem(QTUtility.ResMain[5] + "...");
                tsmiLockThis = new ToolStripMenuItem(QTUtility.ResMain[6]);
                tsmiCloneThis = new ToolStripMenuItem(QTUtility.ResMain[7]);
                tsmiCreateWindow = new ToolStripMenuItem(QTUtility.ResMain[8]);
                tsmiCopy = new ToolStripMenuItem(QTUtility.ResMain[9]);
                tsmiProp = new ToolStripMenuItem(QTUtility.ResMain[10]);
                tsmiHistory = new ToolStripMenuItem(QTUtility.ResMain[11]);
                tsmiTabOrder = new ToolStripMenuItem(QTUtility.ResMain[0x1c]);
                menuTextBoxTabAlias = new ToolStripTextBox();
                tssep_Tab1 = new ToolStripSeparator();
                tssep_Tab2 = new ToolStripSeparator();
                tssep_Tab3 = new ToolStripSeparator();
                contextMenuTab.SuspendLayout();
                contextMenuTab.Items[0].Dispose();
                contextMenuTab.Items.AddRange(new ToolStripItem[] { tsmiClose, tsmiCloseRight, tsmiCloseLeft, tsmiCloseAllButThis, tssep_Tab1, tsmiAddToGroup, tsmiCreateGroup, tssep_Tab2, tsmiLockThis, tsmiCloneThis, tsmiCreateWindow, tsmiCopy, tsmiTabOrder, tssep_Tab3, tsmiProp, tsmiHistory });
                tsmiAddToGroup.DropDownItemClicked += menuitemAddToGroup_DropDownItemClicked;
                (tsmiAddToGroup.DropDown).ImageList = QTUtility.ImageListGlobal;
                tsmiHistory.DropDown = new DropDownMenuBase(components, true, true, true);
                tsmiHistory.DropDownItemClicked += menuitemHistory_DropDownItemClicked;
                (tsmiHistory.DropDown).ImageList = QTUtility.ImageListGlobal;
                menuTextBoxTabAlias.Text = menuTextBoxTabAlias.ToolTipText = QTUtility.ResMain[0x1b];
                menuTextBoxTabAlias.GotFocus += menuTextBoxTabAlias_GotFocus;
                menuTextBoxTabAlias.LostFocus += menuTextBoxTabAlias_LostFocus;
                menuTextBoxTabAlias.KeyPress += menuTextBoxTabAlias_KeyPress;
                tsmiTabOrder.DropDown = new ContextMenuStripEx(components, false);
                tssep_Tab1.Enabled = false;
                tssep_Tab2.Enabled = false;
                tssep_Tab3.Enabled = false;
                contextMenuTab.ResumeLayout(false);
            }
            if(!flag && fText) {
                tsmiClose.Text = QTUtility.ResMain[0];
                tsmiCloseRight.Text = QTUtility.ResMain[1];
                tsmiCloseLeft.Text = QTUtility.ResMain[2];
                tsmiCloseAllButThis.Text = QTUtility.ResMain[3];
                tsmiAddToGroup.Text = QTUtility.ResMain[4];
                tsmiCreateGroup.Text = QTUtility.ResMain[5] + "...";
                tsmiLockThis.Text = QTUtility.ResMain[6];
                tsmiCloneThis.Text = QTUtility.ResMain[7];
                tsmiCreateWindow.Text = QTUtility.ResMain[8];
                tsmiCopy.Text = QTUtility.ResMain[9];
                tsmiProp.Text = QTUtility.ResMain[10];
                tsmiHistory.Text = QTUtility.ResMain[11];
                tsmiTabOrder.Text = QTUtility.ResMain[0x1c];
                menuTextBoxTabAlias.Text = menuTextBoxTabAlias.ToolTipText = QTUtility.ResMain[0x1b];
            }
        }

        private void InstallHooks() {
            hookProc_Key = new HookProc(CallbackKeyboardProc);
            hookProc_Mouse = new HookProc(CallbackMouseProc);
            hookProc_GetMsg = new HookProc(CallbackGetMsgProc);
            int currentThreadId = PInvoke.GetCurrentThreadId();
            hHook_Key = PInvoke.SetWindowsHookEx(2, hookProc_Key, IntPtr.Zero, currentThreadId);
            hHook_Mouse = PInvoke.SetWindowsHookEx(7, hookProc_Mouse, IntPtr.Zero, currentThreadId);
            hHook_Msg = PInvoke.SetWindowsHookEx(3, hookProc_GetMsg, IntPtr.Zero, currentThreadId);
            explorerController = new NativeWindowController(ExplorerHandle);
            explorerController.MessageCaptured += explorerController_MessageCaptured;
            if(ReBarHandle != IntPtr.Zero) {
                rebarController = new RebarController(this, ReBarHandle, BandObjectSite as IOleCommandTarget);
            }
            if(!QTUtility.IsXP) {
                TravelToolBarHandle = GetTravelToolBarWindow32();
                if(TravelToolBarHandle != IntPtr.Zero) {
                    travelBtnController = new NativeWindowController(TravelToolBarHandle);
                    travelBtnController.MessageCaptured += travelBtnController_MessageCaptured;
                }
            }
            dropTargetWrapper = new DropTargetWrapper(this);
            dropTargetWrapper.DragFileEnter += dropTargetWrapper_DragFileEnter;
            dropTargetWrapper.DragFileOver += dropTargetWrapper_DragFileOver;
            dropTargetWrapper.DragFileLeave += dropTargetWrapper_DragFileLeave;
            dropTargetWrapper.DragFileDrop += dropTargetWrapper_DragFileDrop;
        }

        private static bool IsSearchResultFolder(string path) {
            return path.PathStartsWith(QTUtility.IsXP ? QTUtility.ResMisc[2] : QTUtility.PATH_SEARCHFOLDER);
        }

        private static bool IsSpecialFolderNeedsToTravel(string path) {
            int index = path.IndexOf("*?*?*");
            if(index != -1) {
                path = path.Substring(0, index);
            }
            if(!IsSearchResultFolder(path)) {
                if(path.PathEquals("::{13E7F612-F261-4391-BEA2-39DF4F3FA311}")) {
                    return true;
                }
                if(!path.PathStartsWith(QTUtility.ResMisc[0]) && (!path.EndsWith(QTUtility.ResMisc[0], StringComparison.OrdinalIgnoreCase) || Path.IsPathRooted(path))) {
                    return false;
                }
            }
            return true;
        }

        private void ListView_ItemCountChanged(int count) {
            IntPtr ptr;
            if(InstanceManager.TryGetButtonBarHandle(ExplorerHandle, out ptr)) {
                QTUtility2.SendCOPYDATASTRUCT(ptr, (IntPtr)14, null, IntPtr.Zero);
            }
            return;
        }

        private bool ListView_SelectionActivated(Keys modKeys) {
            if(timerSelectionChanged != null) {
                timerSelectionChanged.Enabled = false;
            }
            int num = ShellBrowser.GetSelectedCount();
            bool fEnqExec = Config.Misc.KeepRecentFiles;
            return (fEnqExec || num != 1 || (modKeys != Keys.None && modKeys != Keys.Alt)) &&
                    HandleItemActivate(modKeys, fEnqExec);
        }

        private void ListView_SelectionChanged() {
            if(pluginManager != null && pluginManager.SelectionChangeAttached) {
                if(timerSelectionChanged == null) {
                    timerSelectionChanged = new Timer(components);
                    timerSelectionChanged.Interval = 250;
                    timerSelectionChanged.Tick += timerSelectionChanged_Tick;
                }
                else {
                    timerSelectionChanged.Enabled = false;
                }
                timerSelectionChanged.Enabled = true;
            }
            return;
        }

        private bool ListView_MiddleClick(Point pt) {
            MouseChord chord = QTUtility.MakeMouseChord(MouseChord.Middle, ModifierKeys);
            BindAction action;
            if(Config.Mouse.MarginActions.TryGetValue(chord, out action)) {
                if(listView.PointIsBackground(pt, false)) {
                    return DoBindAction(action);
                }
            }
            if(Config.Mouse.ItemActions.TryGetValue(chord, out action)) {
                int index = listView.HitTest(pt, false);
                if(index <= -1) {
                    return false;
                }
                using(IDLWrapper wrapper = ShellBrowser.GetItem(index)) {
                    return DoBindAction(action, false, null, wrapper);
                }
            }
            return false;
        }

        private bool ListView_MouseActivate(ref int result) {
            // The purpose of this is to prevent accidentally
            // renaming an item when clicking out of a SubDirTip menu.
            bool ret = false;
            if(listView.SubDirTipMenuIsShowing() || (subDirTip_Tab != null && subDirTip_Tab.MenuIsShowing)) {
                if(ShellBrowser.GetSelectedCount() == 1 && listView.HotItemIsSelected()) {
                    result = 2;
                    listView.HideSubDirTipMenu();
                    HideSubDirTip_Tab_Menu();
                    listView.SetFocus();
                    ret = true;
                }
            }
            listView.RefreshSubDirTip(true);
            return ret;
        }

        private bool ListView_DoubleClick(Point pt) {
            MouseChord chord = QTUtility.MakeMouseChord(MouseChord.Double, ModifierKeys);
            BindAction action;
            if(Config.Mouse.MarginActions.TryGetValue(chord, out action) && listView.PointIsBackground(pt, false)) {
                DoBindAction(action);
                return true;
            }
            return false;
        }

        private void ListView_EndLabelEdit(LVITEM item) {
            if(item.pszText != IntPtr.Zero) {
                using(IDLWrapper wrapper = ShellBrowser.GetItem(item.iItem)) {
                    if(wrapper.DisplayName != Marshal.PtrToStringUni(item.pszText)) {
                        HandleF5();
                    }
                }
            }
            return;
        }

        private void ListViewMonitor_ListViewChanged(object sender, EventArgs args) {
            listView = listViewManager.CurrentListView;
            ExtendedListViewCommon elvc = listView as ExtendedListViewCommon;
            if(elvc != null) {
                elvc.ItemCountChanged += ListView_ItemCountChanged;
                elvc.SelectionActivated += ListView_SelectionActivated;
                elvc.SelectionChanged += ListView_SelectionChanged;
                elvc.MiddleClick += ListView_MiddleClick;
                elvc.DoubleClick += ListView_DoubleClick;
                elvc.EndLabelEdit += ListView_EndLabelEdit;
                elvc.MouseActivate += ListView_MouseActivate;
                elvc.SubDirTip_MenuItemClicked += subDirTip_MenuItemClicked;
                elvc.SubDirTip_MenuItemRightClicked += subDirTip_MenuItemRightClicked;
                elvc.SubDirTip_MultipleMenuItemsClicked += subDirTip_MultipleMenuItemsClicked;
                elvc.SubDirTip_MultipleMenuItemsRightClicked += subDirTip_MultipleMenuItemsRightClicked;               
            }
            HandleF5();
        }
          
        private string MakeTravelBtnTooltipText(bool fBack) {
            string path = string.Empty;
            if(fBack) {
                string[] historyBack = CurrentTab.GetHistoryBack();
                if(historyBack.Length > 1) {
                    path = historyBack[1];
                }
            }
            else {
                string[] historyForward = CurrentTab.GetHistoryForward();
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
            // TODO we should be using tags I think
            string groupName = e.ClickedItem.Text;
            string currentPath = ContextMenuedTab.CurrentPath;
            bool addSame = ModifierKeys == Keys.Control;
            Group g = GroupsManager.GetGroup(groupName);
            if(g == null) return;
            if(addSame || !g.Paths.Any(p => p.PathEquals(currentPath))) {
                g.Paths.Add(currentPath);
                GroupsManager.SaveGroups();
            }
        }

        private void menuitemExecuted_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            try {
                string toolTipText = e.ClickedItem.ToolTipText;
                ProcessStartInfo startInfo = new ProcessStartInfo(toolTipText);
                startInfo.WorkingDirectory = Path.GetDirectoryName(toolTipText);
                startInfo.ErrorDialog = true;
                startInfo.ErrorDialogParentHandle = ExplorerHandle;
                Process.Start(startInfo);
                QTUtility.ExecutedPathsList.Add(toolTipText);
            }
            catch {
                SystemSounds.Hand.Play();
            }
        }

        private void menuitemExecuted_ItemRightClicked(object sender, ItemRightClickedEventArgs e) {
            using(IDLWrapper wrapper = new IDLWrapper(e.ClickedItem.ToolTipText)) {
                e.HRESULT = ShellMethods.PopUpSystemContextMenu(wrapper, e.IsKey ? e.Point : MousePosition, ref iContextMenu2, ((DropDownMenuReorderable)sender).Handle, true);
            }
            if(e.HRESULT == 0xffff) {
                QTUtility.ExecutedPathsList.Remove(e.ClickedItem.ToolTipText);
                e.ClickedItem.Dispose();
            }
        }

        private void menuitemGroups_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            Keys modifierKeys = ModifierKeys;
            string groupName = e.ClickedItem.Text;
            if(modifierKeys == (Keys.Control | Keys.Shift)) {
                Group g = GroupsManager.GetGroup(groupName);
                g.Startup = !g.Startup;
                GroupsManager.SaveGroups();
            }
            else {
                OpenGroup(groupName, modifierKeys == Keys.Control);
            }
        }

        private void menuitemGroups_ReorderFinished(object sender, ToolStripItemClickedEventArgs e) {
            GroupsManager.RefreshGroupMenuesOnReorderFinished(tsmiGroups.DropDownItems);
            SyncTaskBarMenu();
        }

        private void menuitemHistory_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            QMenuItem clickedItem = e.ClickedItem as QMenuItem;
            if((ContextMenuedTab != null) && (clickedItem != null)) {
                MenuItemArguments menuItemArguments = clickedItem.MenuItemArguments;
                switch(ModifierKeys) {
                    case Keys.Shift:
                        CloneTabButton(ContextMenuedTab, null, true, -1);
                        NavigateToHistory(new object[] { menuItemArguments.Path, menuItemArguments.IsBack, menuItemArguments.Index });
                        return;

                    case Keys.Control: {
                            using(IDLWrapper wrapper = new IDLWrapper(menuItemArguments.Path)) {
                                OpenNewWindow(wrapper);
                                return;
                            }
                        }
                    default:
                        tabControl1.SelectTab(ContextMenuedTab);
                        NavigateToHistory(new object[] { menuItemArguments.Path, menuItemArguments.IsBack, menuItemArguments.Index });
                        return;
                }
            }
        }

        private void menuitemTabOrder_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            if(e.ClickedItem.Name == "Name") {
                ReorderTab(0, false);
            }
            else if(e.ClickedItem.Name == "Drive") {
                ReorderTab(1, false);
            }
            else if(e.ClickedItem.Name == "Active") {
                ReorderTab(2, false);
            }
            else if(e.ClickedItem.Name == "Rev") {
                ReorderTab(3, false);
            }
        }

        private void menuitemUndoClose_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            QMenuItem clickedItem = (QMenuItem)e.ClickedItem;
            if(ModifierKeys != Keys.Control) {
                OpenNewTab(clickedItem.Path, false);
            }
            else {
                using(IDLWrapper wrapper = new IDLWrapper(clickedItem.Path)) {
                    OpenNewWindow(wrapper);
                }
            }
        }

        private void menuTextBoxTabAlias_GotFocus(object sender, EventArgs e) {
            menuTextBoxTabAlias.ForeColor = SystemColors.WindowText;
            if(menuTextBoxTabAlias.TextBox.ImeMode != ImeMode.On) {
                menuTextBoxTabAlias.TextBox.ImeMode = ImeMode.On;
            }
            if(menuTextBoxTabAlias.Text == QTUtility.ResMain[0x1b]) {
                menuTextBoxTabAlias.Text = string.Empty;
            }
        }

        private void menuTextBoxTabAlias_KeyPress(object sender, KeyPressEventArgs e) {
            if(e.KeyChar == '\r') {
                e.Handled = true;
                contextMenuTab.Close(ToolStripDropDownCloseReason.ItemClicked);
            }
        }

        private void menuTextBoxTabAlias_LostFocus(object sender, EventArgs e) {
            string text = menuTextBoxTabAlias.Text;
            if(text.Length == 0) {
                menuTextBoxTabAlias.Text = QTUtility.ResMain[0x1b];
            }
            if((text != QTUtility.ResMain[0x1b]) && (ContextMenuedTab != null)) {
                ContextMenuedTab.Comment = text;
                ContextMenuedTab.RefreshRectangle();
                tabControl1.Refresh();
            }
            menuTextBoxTabAlias.TextBox.SelectionStart = 0;
        }

        private void MergeAllWindows() {
            List<IntPtr> list = new List<IntPtr>();
            foreach(IntPtr ptr in InstanceManager.ExplorerHandles()) {
                if(ptr != ExplorerHandle) {
                    list.Add(ptr);
                }
            }
            try {
                tabControl1.SetRedraw(false);
                foreach(IntPtr ptr2 in list) {
                    QTTabBarClass tabBar = InstanceManager.GetTabBar(ptr2);
                    if(tabBar != null) {
                        foreach(QTabItem item in tabBar.tabControl1.TabPages) {
                            item.Clone(true).ResetOwner(tabControl1);
                        }
                        WindowUtils.CloseExplorer(ptr2, 2);
                    }
                }
                QTabItem.CheckSubTexts(tabControl1);
                SyncButtonBarCurrent(60);
            }
            finally {
                tabControl1.SetRedraw(true);
            }
        }

        private void NavigateBackToTheFuture() {
            IEnumTravelLogEntry ppenum = null;
            ITravelLogEntry rgElt = null;
            try {
                int num;
                if(((TravelLog.EnumEntries(0x20, out ppenum) == 0) && (TravelLog.GetCount(0x20, out num) == 0)) && (num > 0)) {
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
                        TravelLog.TravelTo(rgElt);
                    }
                }
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception);
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
            Keys modifierKeys = ModifierKeys;
            if(modifierKeys == Keys.Control) {
                using(IDLWrapper wrapper = new IDLWrapper(log.IDL)) {
                    if(!wrapper.Available) {
                        ShowMessageNavCanceled(log.Path, false);
                    }
                    else {
                        OpenNewWindow(wrapper);
                    }
                }
            }
            else if(modifierKeys == Keys.Shift) {
                CloneTabButton(tab, log);
            }
            else {
                tabControl1.SelectTab(tab);
                if(IsSpecialFolderNeedsToTravel(log.Path)) {
                    SaveSelectedItems(CurrentTab);
                    NavigatedByCode = true;
                    NavigateToPastSpecialDir(log.Hash);
                }
                else {
                    NavigatedByCode = false;
                    using(IDLWrapper wrapper2 = new IDLWrapper(log.IDL)) {
                        if(!wrapper2.Available) {
                            ShowMessageNavCanceled(log.Path, false);
                        }
                        else {
                            SaveSelectedItems(CurrentTab);
                            ShellBrowser.Navigate(wrapper2);
                        }
                    }
                }
            }
        }

        private bool NavigateCurrentTab(bool fBack) {
            string currentPath = CurrentTab.CurrentPath;
            LogData data = fBack ? CurrentTab.GoBackward() : CurrentTab.GoForward();
            if(string.IsNullOrEmpty(data.Path)) {
                return false;
            }
            if((CurrentTab.TabLocked && !data.Path.Contains("*?*?*")) && !currentPath.Contains("*?*?*")) {
                try {
                    NowTabCloned = true;
                    QTabItem tab = CurrentTab.Clone();
                    AddInsertTab(tab);
                    if(fBack) {
                        CurrentTab.GoForward();
                    }
                    else {
                        CurrentTab.GoBackward();
                    }
                    tabControl1.SelectTab(tab);
                }
                catch(Exception exception) {
                    QTUtility2.MakeErrorLog(exception);
                }
                return true;
            }
            string path = data.Path;
            if(IsSpecialFolderNeedsToTravel(path) && LogEntryDic.ContainsKey(data.Hash)) {
                SaveSelectedItems(CurrentTab);
                NavigatedByCode = true;
                return NavigateToPastSpecialDir(data.Hash);
            }
            using(IDLWrapper wrapper = new IDLWrapper(data.IDL)) {
                if(!wrapper.Available) {
                    CancelFailedNavigation(path, fBack, 1);
                    return false;
                }
                SaveSelectedItems(CurrentTab);
                NavigatedByCode = true;
                return (0 == ShellBrowser.Navigate(wrapper));
            }
        }

        private static void NavigateOnOldWindow(string path) {
            QTUtility2.SendCOPYDATASTRUCT(InstanceManager.CurrentHandle, new IntPtr(0x10), path, IntPtr.Zero);
        }

        private void NavigateToFirstOrLast(bool fBack) {
            string[] historyBack;
            if(fBack) {
                historyBack = CurrentTab.GetHistoryBack();
            }
            else {
                historyBack = CurrentTab.GetHistoryForward();
            }
            if(historyBack.Length > (fBack ? 1 : 0)) {
                NavigateToHistory(new object[] { historyBack[historyBack.Length - 1], fBack, historyBack.Length - 1 });
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
                    data = CurrentTab.GoBackward();
                }
            }
            else {
                for(int j = 0; j < (num + 1); j++) {
                    data = CurrentTab.GoForward();
                }
            }
            if(string.IsNullOrEmpty(data.Path)) {
                CancelFailedNavigation("( Unknown Path )", fRollBackForward, countRollback);
            }
            else if(CurrentTab.TabLocked) {
                NowTabCloned = true;
                QTabItem tab = CurrentTab.Clone();
                AddInsertTab(tab);
                if(fRollBackForward) {
                    for(int k = 0; k < num; k++) {
                        CurrentTab.GoForward();
                    }
                }
                else {
                    for(int m = 0; m < (num + 1); m++) {
                        CurrentTab.GoBackward();
                    }
                }
                tabControl1.SelectTab(tab);
            }
            else if(IsSpecialFolderNeedsToTravel(path)) {
                SaveSelectedItems(CurrentTab);
                NavigatedByCode = true;
                NavigateToPastSpecialDir(data.Hash);
            }
            else {
                using(IDLWrapper wrapper = new IDLWrapper(data.IDL)) {
                    if(!wrapper.Available) {
                        CancelFailedNavigation(path, fRollBackForward, countRollback);
                    }
                    else {
                        SaveSelectedItems(CurrentTab);
                        NavigatedByCode = true;
                        ShellBrowser.Navigate(wrapper);
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
                historyBack = CurrentTab.GetHistoryBack();
                if((historyBack.Length - 1) < index) {
                    return false;
                }
            }
            else {
                historyBack = CurrentTab.GetHistoryForward();
                if(historyBack.Length < index) {
                    return false;
                }
            }
            string str = fBack ? historyBack[index] : historyBack[index - 1];
            if(!fBack) {
                index--;
            }
            NavigateToHistory(new object[] { str, fBack, index });
            return true;
        }

        private bool NavigateToPastSpecialDir(int hash) {
            IEnumTravelLogEntry ppenum = null;
            try {
                ITravelLogEntry entry2;
                if(TravelLog.EnumEntries(0x31, out ppenum) != 0) {
                    goto Label_007C;
                }
            Label_0013:
                do {
                    if(ppenum.Next(1, out entry2, 0) != 0) {
                        goto Label_007C;
                    }
                    if(entry2 != LogEntryDic[hash]) {
                        goto Label_0057;
                    }
                }
                while(TravelLog.TravelTo(entry2) != 0);
                NowInTravelLog = true;
                CurrentTravelLogIndex++;
                return true;
            Label_0057:
                if(entry2 != null) {
                    Marshal.ReleaseComObject(entry2);
                }
                goto Label_0013;
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception);
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
                switch(ModifierKeys) {
                    case Keys.Shift:
                        CloneTabButton(CurrentTab, null, true, -1);
                        NavigateToHistory(new object[] { menuItemArguments.Path, menuItemArguments.IsBack, menuItemArguments.Index });
                        return;

                    case Keys.Control: {
                            using(IDLWrapper wrapper = new IDLWrapper(menuItemArguments.Path)) {
                                OpenNewWindow(wrapper);
                                return;
                            }
                        }
                    default:
                        NavigateToHistory(new object[] { menuItemArguments.Path, menuItemArguments.IsBack, menuItemArguments.Index });
                        return;
                }
            }
        }

        private void NavigationButtons_Click(object sender, EventArgs e) {
            NavigateCurrentTab(sender == buttonBack);
        }

        private void NavigationButtons_DropDownOpening(object sender, EventArgs e) {
            buttonNavHistoryMenu.DropDown.SuspendLayout();
            while(buttonNavHistoryMenu.DropDownItems.Count > 0) {
                buttonNavHistoryMenu.DropDownItems[0].Dispose();
            }
            if((CurrentTab.HistoryCount_Back + CurrentTab.HistoryCount_Forward) > 1) {
                buttonNavHistoryMenu.DropDownItems.AddRange(CreateNavBtnMenuItems(true).ToArray());
                buttonNavHistoryMenu.DropDownItems.AddRange(CreateBranchMenu(true, components, tsmiBranchRoot_DropDownItemClicked).ToArray());
            }
            else {
                ToolStripMenuItem item = new ToolStripMenuItem("none");
                item.Enabled = false;
                buttonNavHistoryMenu.DropDownItems.Add(item);
            }
            buttonNavHistoryMenu.DropDown.ResumeLayout();
        }

        private static void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e) {
            if(e.Button == MouseButtons.Left) {
                RestoreAllWindowFromTray();
            }
        }

        // TODO
        internal void odCallback_ManagePlugin(object o) {
            lstTempAssemblies_Refresh = (List<PluginAssembly>)o;
            SyncTabBarBroadcastPlugin(Handle);
            RefreshPlugins(true);
            lstTempAssemblies_Refresh = null;
            /*
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(RegConst.Root)) {
                if(key != null) {
                    //QTUtility2.WriteRegBinary(QTButtonBar.ButtonIndexes, "Buttons_Order", key);
                }
            }
            List<string> list = new List<string>();
            using(RegistryKey key2 = Registry.CurrentUser.CreateSubKey(RegConst.Root + @"Plugins")) {
                if(key2 != null) {
                    using(RegistryKey key3 = key2.CreateSubKey("Paths")) {
                        foreach(string str in key3.GetValueNames()) {
                            key3.DeleteValue(str);
                        }
                        int idx = 0;
                        foreach(PluginAssembly assembly in PluginManager.PluginAssemblies
                                .Where(assembly => assembly.PluginInfosExist)) {
                            list.AddRange(from information in assembly.PluginInformations
                                    where information.Enabled
                                    select information.PluginID);
                            key3.SetValue((idx++).ToString(), assembly.Path);
                        }
                    }
                    QTUtility2.WriteRegBinary(list.ToArray(), "Enabled", key2);
                    key2.SetValue("LanguageFile", QTUtility.Path_PluginLangFile);
                }
            }
            PluginManager.SaveButtonOrder();*/
        }

        private void OnAwake() {
        }

        protected override void OnExplorerAttached() {
            ExplorerHandle = (IntPtr)Explorer.HWND;
            if(Config.NoWindowResizing) {
                PInvoke.SetWindowLongPtr(ExplorerHandle, -16, PInvoke.Ptr_OP_AND(PInvoke.GetWindowLongPtr(ExplorerHandle, -16), 0xfffbffff));
            }
            try {
                object obj2;
                object obj3;
                _IServiceProvider bandObjectSite = (_IServiceProvider)BandObjectSite;
                Guid guid = ExplorerGUIDs.IID_IShellBrowser;
                Guid riid = ExplorerGUIDs.IID_IUnknown;
                bandObjectSite.QueryService(ref guid, ref riid, out obj2);
                ShellBrowser = new ShellBrowserEx((IShellBrowser)obj2);
                HookLibManager.InitShellBrowserHook(ShellBrowser.GetIShellBrowser());
                if(Config.Tweaks.ForceSysListView) {
                    ShellBrowser.SetUsingListView(true);
                }
                Guid guid3 = ExplorerGUIDs.IID_ITravelLogStg;
                Guid guid4 = ExplorerGUIDs.IID_ITravelLogStg;
                bandObjectSite.QueryService(ref guid3, ref guid4, out obj3);
                TravelLog = (ITravelLogStg)obj3;
            }
            catch(COMException exception) {
                QTUtility2.MakeErrorLog(exception);
            }
            Explorer.BeforeNavigate2 += Explorer_BeforeNavigate2;
            Explorer.NavigateComplete2 += Explorer_NavigateComplete2;
        }

        protected override void OnPaintBackground(PaintEventArgs e) {
            if(VisualStyleRenderer.IsSupported) {
                if(bgRenderer == null) {
                    bgRenderer = new VisualStyleRenderer(VisualStyleElement.Rebar.Band.Normal);
                }
                bgRenderer.DrawParentBackground(e.Graphics, e.ClipRectangle, this);
            }
            else {
                if(ReBarHandle != IntPtr.Zero) {
                    int colorref = (int)PInvoke.SendMessage(ReBarHandle, 0x414, IntPtr.Zero, IntPtr.Zero);
                    using(SolidBrush brush = new SolidBrush(QTUtility2.MakeColor(colorref))) {
                        e.Graphics.FillRectangle(brush, e.ClipRectangle);
                        return;
                    }
                }
                base.OnPaintBackground(e);
            }
        }

        private void OpenDroppedFolder(List<string> listDroppedPaths) {
            Keys modifierKeys = ModifierKeys;
            QTUtility2.InitializeTemporaryPaths();
            bool blockSelecting = modifierKeys == Keys.Shift;
            bool flag2 = modifierKeys == Keys.Control;
            bool flag3 = false;
            tabControl1.SetRedraw(false);
            try {
                foreach(string str in listDroppedPaths.Where(str => !string.IsNullOrEmpty(str))) {
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
                                                    OpenNewTab(wrapper2, blockSelecting);
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
                                            OpenNewTab(wrapper, blockSelecting);
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
            finally {
                tabControl1.SetRedraw(true);
            }
            if(flag2) {
                if(QTUtility.TMPIDLList.Count <= 0) {
                    return;
                }
                QTUtility.TMPTargetIDL = QTUtility.TMPIDLList[0];
                using(IDLWrapper wrapper3 = new IDLWrapper(QTUtility.TMPTargetIDL)) {
                    ShellBrowser.Navigate(wrapper3, SBSP.NEWBROWSER);
                    return;
                }
            }
            if(!flag3 && (listDroppedPaths.Count > 0)) {
                List<string> list = listDroppedPaths.Where(File.Exists).ToList();
                if(list.Count > 0) {
                    AppendUserApps(list);
                }
            }
        }

        private void OpenGroup(string groupName, bool fForceNewWindow) {
            // todo clean
            if(!fForceNewWindow) {
                string str3;
                NowTabsAddingRemoving = true;
                bool flag = false;
                string str4 = null;
                int num = 0;
                QTabItem tabPage = null;
                Keys modifierKeys = ModifierKeys;
                bool flag2 = Config.CloseWhenGroup;
                bool flag3 = Config.Tabs.NeverOpenSame == (modifierKeys != Keys.Shift);
                bool flag4 = Config.Tabs.ActivateNewTab == (modifierKeys != Keys.Control);
                bool flag5 = false;
                if(NowOpenedByGroupOpener) {
                    flag3 = true;
                    NowOpenedByGroupOpener = false;
                }
                Group g = GroupsManager.GetGroup(groupName);
                if(g != null) {
                    List<string> list = new List<string>();
                    List<QTabItem> list2 = new List<QTabItem>();
                    foreach(QTabItem item2 in tabControl1.TabPages) {
                        list.Add(item2.CurrentPath.ToLower());
                        list2.Add(item2);
                    }
                    if(g.Paths.Count != 0) {
                        try {
                            tabControl1.SetRedraw(false);
                            foreach(string gpath in g.Paths.Where(gpath => 
                                    QTUtility2.PathExists(gpath) || gpath.Contains("???"))) {
                                if(str4 == null) {
                                    str4 = gpath;
                                }
                                if((flag2 || !flag3) || !list.Contains(gpath.ToLower())) {
                                    num++;
                                    using(IDLWrapper wrapper2 = new IDLWrapper(gpath)) {
                                        if(wrapper2.Available) {
                                            if(tabPage == null) {
                                                tabPage = CreateNewTab(wrapper2);
                                            }
                                            else {
                                                CreateNewTab(wrapper2);
                                            }
                                        }
                                    }
                                    flag = true;
                                }
                                else if(tabPage == null) {
                                    foreach(QTabItem item3 in tabControl1.TabPages) {
                                        if(item3.CurrentPath.PathEquals(gpath)) {
                                            tabPage = item3;
                                            break;
                                        }
                                    }
                                }
                            }
                            if(flag2 && (num > 0)) {
                                for(int j = 0; j < list2.Count; j++) {
                                    AddToHistory(list2[j]);
                                    tabControl1.TabPages.Remove(list2[j]);
                                    list2[j].OnClose();
                                    list2[j] = null;
                                }
                            }
                            NowTabsAddingRemoving = false;
                            if(((str4 != null) && (flag4 || (tabControl1.SelectedIndex == -1))) && (tabPage != null)) {
                                if(flag) {
                                    NowTabCreated = true;
                                }
                                flag5 = tabPage != CurrentTab;
                                tabControl1.SelectTab(tabPage);
                            }
                        }
                        finally {
                            tabControl1.SetRedraw(true);
                        }
                    }
                }
                SyncButtonBarCurrent(flag5 ? 0x3f : 0x1003f);
                NowTabsAddingRemoving = false;
            }
            else {
                string str;
                Group g = GroupsManager.GetGroup(groupName);
                if(g != null && g.Paths.Count > 0) {
                    string path = g.Paths[0];
                    QTUtility.CreateWindowTMPGroup = groupName;
                    using(IDLWrapper wrapper = new IDLWrapper(path)) {
                        if(wrapper.Available) {
                            OpenNewWindow(wrapper);
                            return;
                        }
                    }
                }
                QTUtility.CreateWindowTMPGroup = string.Empty;
            }
        }

        private bool OpenNewTab(string path, bool blockSelecting = false, bool fForceNew = false) {
            using(IDLWrapper wrapper = new IDLWrapper(path)) {
                if(wrapper.Available) {
                    return OpenNewTab(wrapper, blockSelecting, fForceNew);
                }
            }
            return false;
        }

        private bool OpenNewTab(IDLWrapper idlwGiven, bool blockSelecting = false, bool fForceNew = false) {
            // Check that the folder exists and is navigable.
            if(idlwGiven == null || !idlwGiven.Available || !idlwGiven.HasPath || !idlwGiven.IsReadyIfDrive || idlwGiven.IsLinkToDeadFolder) {
                SystemSounds.Hand.Play();
                return false;
            }

            // If the IDL is a link, resolve it.  Otherwise keep using the one we're given.
            using(IDLWrapper idlwLink = idlwGiven.ResolveTargetIfLink()) {
                IDLWrapper idlw = idlwLink ?? idlwGiven;

                // Recheck a few things
                if(!idlw.Available || !idlw.HasPath || !idlw.IsReadyIfDrive || !idlw.IsFolder) {
                    SystemSounds.Hand.Play();
                    return false;
                }

                if(blockSelecting) {
                    NowTabsAddingRemoving = true;
                }
                try {
                    // Check if it's already open
                    if(!fForceNew && Config.Tabs.NeverOpenSame) {
                        QTabItem tabPage = tabControl1.TabPages.Cast<QTabItem>().FirstOrDefault(
                                item2 => item2.CurrentPath.PathEquals(idlw.Path));
                        if(tabPage != null) {
                            if(Config.Tabs.ActivateNewTab) {
                                tabControl1.SelectTab(tabPage);
                            }
                            SyncButtonBarCurrent(0x3f);
                            return false;
                        }
                    }

                    // TODO
                    // This entire block is a mystery to me, and I think it should be
                    // removed. It's gone in Quizo's version.
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
                                                    if(!blockSelecting && Config.Tabs.ActivateNewTab) {
                                                        NowTabCreated = true;
                                                        tabControl1.SelectTab(CreateNewTab(wrapper2));
                                                    }
                                                    else {
                                                        CreateNewTab(wrapper2);
                                                        SyncButtonBarCurrent(0x1003f);
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

                    // This should work for everything...
                    if(!blockSelecting && Config.Tabs.ActivateNewTab) {
                        NowTabCreated = true;
                        tabControl1.SelectTab(CreateNewTab(idlw));
                    }
                    else {
                        CreateNewTab(idlw);
                        SyncButtonBarCurrent(0x1003f);
                    }
                }
                finally {
                    if(blockSelecting) {
                        NowTabsAddingRemoving = false;
                    }
                }
            }
            return true;
        }

        private void OpenNewWindow(IDLWrapper idlwGiven) {
            // Check that the folder exists and is navigable.
            if(idlwGiven == null || !idlwGiven.Available || !idlwGiven.HasPath || !idlwGiven.IsReadyIfDrive || idlwGiven.IsLinkToDeadFolder) {
                SystemSounds.Hand.Play();
                return;
            }
            
            // If the IDL is a link, resolve it.  Otherwise keep using the one we're given.
            using(IDLWrapper idlwLink = idlwGiven.ResolveTargetIfLink()) {
                IDLWrapper idlw = idlwLink ?? idlwGiven;

                // Recheck a few things
                if(!idlw.Available || !idlw.HasPath || !idlw.IsReadyIfDrive || !idlw.IsFolder) {
                    SystemSounds.Hand.Play();
                    return;
                }

                bool isFolderTreeVisible = ShellBrowser.IsFolderTreeVisible();    
                bool fSameAsCurrent;
                using(IDLWrapper wrapper = ShellBrowser.GetShellPath()) {
                    fSameAsCurrent = (wrapper == idlw);
                }

                // There's some weird magic going on here, but it's apparently necessary.
                // TODO: understand it
                SBSP wFlags = SBSP.NEWBROWSER;
                if(fSameAsCurrent) {
                    if(isFolderTreeVisible) {
                        if(CheckProcessID(ExplorerHandle, WindowUtils.GetShellTrayWnd()) || WindowUtils.IsExplorerProcessSeparated()) {
                            PInvoke.SetRedraw(ExplorerHandle, false);
                            ShowFolderTree(false);
                            wFlags |= SBSP.EXPLOREMODE;
                            new WaitTimeoutCallback(WaitTimeout).BeginInvoke(200, AsyncComplete_FolderTree, true);
                        }
                        else {
                            QTUtility.fRestoreFolderTree = true;
                        }
                    }
                    else {
                        if(QTUtility.IsXP) {
                            QTUtility.RestoreFolderTree_Hide = true;
                        }
                        wFlags |= SBSP.EXPLOREMODE;
                    }
                }
                else if(isFolderTreeVisible) {
                    QTUtility.fRestoreFolderTree = true;
                }

                QTUtility.CreateWindowTMPPath = idlw.Path;
                if(ShellBrowser.Navigate(idlw, wFlags) != 0) {
                    QTUtility2.MakeErrorLog(null, string.Format("Failed navigation: {0}", idlw.Path));
                    MessageBox.Show(string.Format(QTUtility.TextResourcesDic["TabBar_Message"][0], idlw.Path));
                    QTUtility.CreateWindowTMPGroup = QTUtility.CreateWindowTMPPath = string.Empty;
                }
                QTUtility.fRestoreFolderTree = false;
            }
        }

        private void pluginitems_Click(object sender, EventArgs e) {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            string name = item.Name;
            MenuType tag = (MenuType)item.Tag;
            foreach(Plugin plugin in pluginManager.Plugins.Where(plugin => plugin.PluginInformation.PluginID == name)) {
                try {
                    if(tag == MenuType.Tab) {
                        if(ContextMenuedTab != null) {
                            plugin.Instance.OnMenuItemClick(tag, item.Text, new PluginServer.TabWrapper(ContextMenuedTab, this));
                        }
                    }
                    else {
                        plugin.Instance.OnMenuItemClick(tag, item.Text, null);
                    }
                }
                catch(Exception exception) {
                    PluginManager.HandlePluginException(exception, ExplorerHandle, plugin.PluginInformation.Name, "On menu item \"" + item.Text + "\"clicked.");
                }
                break;
            }
        }

        private void QTTabBarClass_MouseDoubleClick(object sender, MouseEventArgs e) {
            MouseChord chord = QTUtility.MakeMouseChord(MouseChord.Double, ModifierKeys);
            BindAction action;
            if(Config.Mouse.BarActions.TryGetValue(chord, out action)) {
                DoBindAction(action);
            }
        }

        private void QTTabBarClass_MouseUp(object sender, MouseEventArgs e) {
            MouseChord chord;
            if(e.Button == MouseButtons.Left) {
                chord = QTUtility.MakeMouseChord(MouseChord.Left, ModifierKeys);
            }
            else if(e.Button == MouseButtons.Middle) {
                chord = QTUtility.MakeMouseChord(MouseChord.Middle, ModifierKeys);
            }
            else {
                return;
            }
            BindAction action;
            if(Config.Mouse.BarActions.TryGetValue(chord, out action)) {
                DoBindAction(action);
            }
        }

        internal void RefreshOptions() {
            // TODO
            bool fAutoSubText = false;
            string oldPath_LangFile = "";

            if(QTUtility.Path_LanguageFile != oldPath_LangFile) {
                if(File.Exists(QTUtility.Path_LanguageFile)) {
                    QTUtility.TextResourcesDic = QTUtility.ReadLanguageFile(QTUtility.Path_LanguageFile);
                }
                else {
                    QTUtility.Path_LanguageFile = string.Empty;
                    QTUtility.TextResourcesDic = null;
                }
                QTUtility.ValidateTextResources();
                RefreshTexts();
            }
            RefreshTabBar();
            SyncTabBarBroadcast(Handle);
            QTUtility.ClosedTabHistoryList.MaxCapacity = Config.Misc.TabHistoryCount;
            QTUtility.ExecutedPathsList.MaxCapacity = Config.Misc.FileHistoryCount;
            SyncButtonBarBroadCast(0x100);
            SyncButtonBarCurrent(0x3f);
            SyncTaskBarMenu();
            if(fAutoSubText && !tabControl1.AutoSubText) {
                foreach(QTabItem item in tabControl1.TabPages) {
                    item.Comment = string.Empty;
                    item.RefreshRectangle();
                }
                tabControl1.Refresh();
            }
            else if(!fAutoSubText && tabControl1.AutoSubText) {
                QTabItem.CheckSubTexts(tabControl1);
            }
            if(pluginManager != null) {
                pluginManager.OnSettingsChanged(0);
            }
            if(DropDownMenuBase.InitializeMenuRenderer() && (pluginManager != null)) {
                pluginManager.OnMenuRendererChanged();
            }
            ContextMenuStripEx.InitializeMenuRenderer();
        }

        private void RefreshPlugins(bool fStatic) {
            if(pluginManager != null) {
                pluginManager.ClearFilterEngines();
                if(fStatic) {
                    PluginManager.ClearIEncodingDetector();
                }
                foreach(PluginAssembly assembly in PluginManager.PluginAssemblies.Except(lstTempAssemblies_Refresh)) {
                    pluginManager.UninstallPluginAssembly(assembly, fStatic);
                }
                foreach(PluginAssembly assembly2 in lstTempAssemblies_Refresh) {
                    if(fStatic) {
                        PluginManager.AddAssembly(assembly2);
                    }
                    pluginManager.RefreshPluginAssembly(assembly2, fStatic);
                }
            }
        }

        private void RefreshTabBar() {
            SuspendLayout();
            tabControl1.SuspendLayout();
            tabControl1.RefreshOptions(false);
            if(Config.Tabs.ShowNavButtons) {
                if(toolStrip == null) {
                    InitializeNavBtns(true);
                    buttonNavHistoryMenu.Enabled = navBtnsFlag != 0;
                    Controls.Add(toolStrip);
                }
                else {
                    toolStrip.SuspendLayout();
                }
                toolStrip.Dock = Config.Tabs.NavButtonsOnRight ? DockStyle.Right : DockStyle.Left;
                toolStrip.ResumeLayout(false);
                toolStrip.PerformLayout();
            }
            else if(toolStrip != null) {
                toolStrip.Dock = DockStyle.None;
            }
            IntPtr windowLongPtr = PInvoke.GetWindowLongPtr(ExplorerHandle, -16);
            if(Config.NoWindowResizing) {
                PInvoke.SetWindowLongPtr(ExplorerHandle, -16, PInvoke.Ptr_OP_AND(windowLongPtr, 0xfffbffff));
            }
            else {
                PInvoke.SetWindowLongPtr(ExplorerHandle, -16, PInvoke.Ptr_OP_OR(windowLongPtr, 0x40000));
            }
            int iType = 0;
            if(Config.Tabs.MultipleTabRows) {
                iType = Config.Tabs.ActiveTabOnBottomRow ? 1 : 2;
            }
            SetBarRows(tabControl1.SetTabRowType(iType));
            rebarController.RefreshBG();
            if(Config.Tweaks.AlternateRowColors) {
                Color color = Config.Tweaks.AltRowBackgroundColor;
                if(QTUtility.sbAlternate == null) {
                    QTUtility.sbAlternate = new SolidBrush(color);
                }
                else {
                    QTUtility.sbAlternate.Color = color;
                }
            }
            foreach(QTabItem item in tabControl1.TabPages) {
                item.RefreshRectangle();
            }
            ShellBrowser.SetUsingListView(Config.Tweaks.ForceSysListView);
            tabControl1.ResumeLayout();
            ResumeLayout(true);
        }

        private void RefreshTexts() {
            IntPtr ptr;
            if(InstanceManager.TryGetButtonBarHandle(ExplorerHandle, out ptr)) {
                QTUtility2.SendCOPYDATASTRUCT(ptr, (IntPtr)10, "refreshText", IntPtr.Zero);
            }
            OptionsDialog_OLD.RefreshTexts();
        }

        [ComRegisterFunction]
        private static void Register(Type t) {
            string name = t.GUID.ToString("B");
            using(RegistryKey key2 = Registry.ClassesRoot.CreateSubKey(@"CLSID\" + name)) {
                key2.SetValue(null, "QTTabBar");
                key2.SetValue("MenuText", "QTTabBar");
                key2.SetValue("HelpText", "QTTabBar");
            }
            using(RegistryKey key3 = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Internet Explorer\Toolbar")) {
                key3.SetValue(name, "QTTabBar");
            }
        }

        private void ReorderTab(int index, bool fDescending) {
            tabControl1.SetRedraw(false);
            try {
                if(index == 3) {
                    if(tabControl1.TabCount > 1) {
                        int indexSource = 0;
                        for(int i = tabControl1.TabCount - 1; indexSource < i; i--) {
                            tabControl1.TabPages.Relocate(indexSource, i);
                            tabControl1.TabPages.Relocate(i - 1, indexSource);
                            indexSource++;
                        }
                    }
                }
                else {
                    int num3 = fDescending ? -1 : 1;
                    for(int j = 0; j < (tabControl1.TabCount - 1); j++) {
                        for(int k = tabControl1.TabCount - 1; k > j; k--) {
                            string strA;
                            string strB;
                            if(index == 0) {
                                strA = tabControl1.TabPages[j].Text;
                                strB = tabControl1.TabPages[k].Text;
                            }
                            else if(index == 1) {
                                strA = ((QTabItem)tabControl1.TabPages[j]).CurrentPath;
                                strB = ((QTabItem)tabControl1.TabPages[k]).CurrentPath;
                            }
                            else {
                                int num6 = lstActivatedTabs.IndexOf((QTabItem)tabControl1.TabPages[j]);
                                int num7 = lstActivatedTabs.IndexOf((QTabItem)tabControl1.TabPages[k]);
                                if(((num6 - num7) * num3) < 0) {
                                    tabControl1.TabPages.Relocate(j, k);
                                }
                                continue;
                            }
                            if((string.Compare(strA, strB) * num3) > 0) {
                                tabControl1.TabPages.Relocate(j, k);
                            }
                        }
                    }
                }
            }
            finally {
                tabControl1.SetRedraw(true);
            }
            SyncButtonBarCurrent(12);
        }

        private void ReplaceByGroup(string groupName) {
            // TODO: figure this out
            /*
            byte num = QTUtility.ConfigValues[0];
            if(Config.CloseWhenGroup) {
                QTUtility.ConfigValues[0] = (byte)(QTUtility.ConfigValues[0] & 0xdf);
            }
            else {
                QTUtility.ConfigValues[0] = (byte)(QTUtility.ConfigValues[0] | 0x20);
            }
            */
            OpenGroup(groupName, false);
            //QTUtility.ConfigValues[0] = num;
        }

        private static void RestoreAllWindowFromTray() {
            Dictionary<IntPtr, int> dictionary = new Dictionary<IntPtr, int>(dicNotifyIcon);
            foreach(IntPtr ptr in dictionary.Keys) {
                ShowTaskbarItem(ptr, true);
            }
        }

        private void RestoreFromTray() {
            if((dicNotifyIcon != null) && dicNotifyIcon.ContainsKey(ExplorerHandle)) {
                ShowTaskbarItem(ExplorerHandle, true);
            }
        }

        private void RestoreLastClosed() {
            if(QTUtility.ClosedTabHistoryList.Count <= 0) {
                return;
            }
            Stack<string> stack = new Stack<string>(QTUtility.ClosedTabHistoryList);
            string path = null;
            while(stack.Count > 0) {
                path = stack.Pop();
                if(!tabControl1.TabPages.Cast<QTabItem>().Any(item => item.CurrentPath.PathEquals(path))) {
                    OpenNewTab(path, false);
                    return;
                }
            }
            if(!path.PathEquals(CurrentAddress)) {
                OpenNewTab(path, false);
            }
        }

        private void RestoreTabsOnInitialize(int iIndex, string openingPath) {
            QTUtility.RefreshLockedTabsList();
            // TODO: unjank
            TabPos num = Config.Tabs.NewTabPosition;
            Config.Tabs.NewTabPosition = TabPos.Rightmost;
            try {
                if(iIndex == 1) {
                    foreach(string str in QTUtility.LockedTabsToRestoreList) {
                        bool flag = false;
                        foreach(QTabItem item2 in tabControl1.TabPages) {
                            if(item2.CurrentPath == str) {
                                if(item2 == CurrentTab) {
                                    fNowRestoring = true;
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
                                        CreateNewTab(wrapper).TabLocked = true;
                                    }
                                    continue;
                                }
                            }
                            tabControl1.TabPages.Relocate(0, tabControl1.TabCount - 1);
                            fNowRestoring = true;
                        }
                    }
                }
                else if(iIndex == 0) {
                    using(RegistryKey key = Registry.CurrentUser.OpenSubKey(RegConst.Root, false)) {
                        if(key != null) {
                            string[] strArray = ((string)key.GetValue("TabsOnLastClosedWindow", string.Empty)).Split(QTUtility.SEPARATOR_CHAR);
                            if((strArray.Length > 0) && (strArray[0].Length > 0)) {
                                foreach(string str2 in strArray.Where(str2 => str2.Length > 0
                                        && !tabControl1.TabPages.Cast<QTabItem>().Any(item3 => item3.CurrentPath == str2))) {
                                    if(str2 == openingPath) {
                                        tabControl1.TabPages.Relocate(0, tabControl1.TabCount - 1);
                                    }
                                    else {
                                        using(IDLWrapper wrapper2 = new IDLWrapper(str2)) {
                                            if(wrapper2.Available) {
                                                QTabItem item4 = CreateNewTab(wrapper2);
                                                if(QTUtility.LockedTabsToRestoreList.Contains(str2)) {
                                                    item4.TabLocked = true;
                                                }
                                            }
                                        }
                                    }
                                }
                                fNowRestoring = true;
                            }
                        }
                    }
                }
            }
            finally {
                Config.Tabs.NewTabPosition = num;
            }
        }

        private void SaveSelectedItems(QTabItem tab) {
            Address[] addressArray;
            string str;
            if(((tab != null) && !string.IsNullOrEmpty(CurrentAddress)) && ShellBrowser.TryGetSelection(out addressArray, out str, false)) {
                tab.SetSelectedItemsAt(CurrentAddress, addressArray, str);
            }
        }

        private void SetBarRows(int count) {
            BandHeight = (count * (Config.Skin.TabHeight - 3)) + 5;
            rebarController.RefreshHeight();
        }

        internal static void SetStringClipboard(string str) {
            try {
                Clipboard.SetDataObject(str, true);
                if(!Config.DisableSound) {
                    SystemSounds.Asterisk.Play();
                }
            }
            catch {
                SystemSounds.Hand.Play();
            }
        }

        protected override bool ShouldHaveBreak() {
            bool breakBar = true;
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(RegConst.Root)) {
                if(key != null) {
                    breakBar = ((int)key.GetValue("BreakTabBar", 1) == 1);
                }
            }
            return breakBar;
        }

        public override void ShowDW(bool fShow) {
            base.ShowDW(fShow);
            if((fShow && !FirstNavigationCompleted) && ((Explorer != null) && (Explorer.ReadyState == tagREADYSTATE.READYSTATE_COMPLETE))) {
                InitializeInstallation();
            }
            if(!fShow) {
                using(RegistryKey key = Registry.CurrentUser.CreateSubKey(RegConst.Root)) {
                    key.SetValue("BreakTabBar", BandHasBreak() ? 1 : 0);
                }
            }
        }

        private void ShowFolderTree(bool fShow) {
            if(QTUtility.IsXP && (fShow != ShellBrowser.IsFolderTreeVisible())) {
                object pvaClsid = "{EFA24E64-B078-11d0-89E4-00C04FC9E26E}";
                object pvarShow = fShow;
                object pvarSize = null;
                Explorer.ShowBrowserBar(ref pvaClsid, ref pvarShow, ref pvarSize);
            }
        }

        internal static void ShowMD5(string[] paths) {
            if(md5Form == null) {
                md5Form = new FileHashComputerForm();
            }
            List<string> list = new List<string>();
            if(paths != null) {
                list.AddRange(paths.Where(File.Exists));
            }
            string[] strArray = null;
            if(list.Count > 0) {
                strArray = list.ToArray();
            }
            if(md5Form.InvokeRequired) {
                md5Form.Invoke(new FormMethodInvoker(ShowMD5FormCore), new object[] { strArray });
            }
            else {
                ShowMD5FormCore(strArray);
            }
        }

        private static void ShowMD5FormCore(object paths) {
            md5Form.ShowFileHashForm((string[])paths);
        }

        private void ShowMessageNavCanceled(string failedPath, bool fModal) {
            QTUtility2.MakeErrorLog(null, string.Format("Failed navigation: {0}", failedPath));
            MessageForm.Show(ExplorerHandle, string.Format(QTUtility.TextResourcesDic["TabBar_Message"][0], failedPath), string.Empty, MessageBoxIcon.Asterisk, 0x2710, fModal);
        }

        private void ShowSearchBar(bool fShow) {
            if(!QTUtility.IsXP) {
                if(!fShow) {
                    return;
                }
                using(IDLWrapper wrapper = new IDLWrapper(QTUtility.PATH_SEARCHFOLDER)) {
                    if(wrapper.Available) {
                        ShellBrowser.Navigate(wrapper, SBSP.NEWBROWSER);
                    }
                    return;
                }
            }
            object pvaClsid = "{C4EE31F3-4768-11D2-BE5C-00A0C9A83DA1}";
            object pvarShow = fShow;
            object pvarSize = null;
            Explorer.ShowBrowserBar(ref pvaClsid, ref pvarShow, ref pvarSize);
        }

        public AbstractListView GetListView() {
            return listView;
        }

        private void ShowSubdirTip_Tab(QTabItem tab, bool fShow, int offsetX, bool fKey, bool fParent) {
            try {
                if(fShow) {
                    if(Explorer.Busy || string.IsNullOrEmpty(tab.CurrentPath)) {
                        tabControl1.SetSubDirTipShown(false);
                    }
                    else {
                        string currentPath = tab.CurrentPath;
                        if(fParent || ShellMethods.TryMakeSubDirTipPath(ref currentPath)) {
                            if(subDirTip_Tab == null) {
                                subDirTip_Tab = new SubDirTipForm(Handle, true, listView);
                                subDirTip_Tab.MenuItemClicked += subDirTip_MenuItemClicked;
                                subDirTip_Tab.MultipleMenuItemsClicked += subDirTip_MultipleMenuItemsClicked;
                                subDirTip_Tab.MenuItemRightClicked += subDirTip_MenuItemRightClicked;
                                subDirTip_Tab.MenuClosed += subDirTip_Tab_MenuClosed;
                                subDirTip_Tab.MultipleMenuItemsRightClicked += subDirTip_MultipleMenuItemsRightClicked;
                            }
                            ContextMenuedTab = tab;
                            Point pnt = tabControl1.PointToScreen(new Point(tab.TabBounds.X + offsetX, fParent ? tab.TabBounds.Top : (tab.TabBounds.Bottom - 3)));
                            if(tab != CurrentTab) {
                                pnt.X += 2;
                            }
                            tabControl1.SetSubDirTipShown(subDirTip_Tab.ShowMenuWithoutShowForm(currentPath, pnt, fParent));
                        }
                        else {
                            tabControl1.SetSubDirTipShown(false);
                            HideSubDirTip_Tab_Menu();
                        }
                    }
                }
                else {
                    HideSubDirTip_Tab_Menu();
                }
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception, "tabsubdir");
            }
        }

        private bool ShowTabSwitcher(bool fShift, bool fRepeat) {
            listView.HideSubDirTip();
            listView.HideThumbnailTooltip();
            if(tabControl1.TabCount < 2) {
                return false;
            }
            if(tabSwitcher == null) {
                tabSwitcher = new TabSwitchForm();
                tabSwitcher.Switched += tabSwitcher_Switched;
            }
            if(!tabSwitcher.IsShown) {
                List<PathData> lstPaths = new List<PathData>();
                string str = Config.Tabs.RenameAmbTabs ? " @ " : " : ";
                foreach(QTabItem item in tabControl1.TabPages) {
                    string strDisplay = item.Text;
                    if(!string.IsNullOrEmpty(item.Comment)) {
                        strDisplay += str + item.Comment;
                    }
                    lstPaths.Add(new PathData(strDisplay, item.CurrentPath, item.ImageKey));
                }
                tabSwitcher.ShowSwitcher(ExplorerHandle, tabControl1.SelectedIndex, lstPaths);
            }
            int index = tabSwitcher.Switch(fShift);
            if(!fRepeat || tabControl1.TabCount < 13) {
                tabControl1.SetPseudoHotIndex(index);
            }
            return true;
        }

        private static void ShowTaskbarItem(IntPtr hwndExplr, bool fShow) {
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
                    contextMenuNotifyIcon.ItemClicked += contextMenuNotifyIcon_ItemClicked;
                    notifyIcon = new NotifyIcon();
                    notifyIcon.Icon = icoNotify;
                    notifyIcon.MouseDoubleClick += notifyIcon_MouseDoubleClick;
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
                    if(str.Length > 64) {
                        str = str.Substring(0, 60) + "...";
                    }
                    notifyIcon.Text = str;
                }
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception);
            }
            finally {
                if(o != null) {
                    Marshal.ReleaseComObject(o);
                }
            }
        }

        private void ShowToolTipForDD(QTabItem tab, int iState, int grfKeyState) {
            if(((tabForDD == null) || (tabForDD != tab)) || (iModKeyStateDD != grfKeyState)) {
                tabForDD = tab;
                iModKeyStateDD = grfKeyState;
                if(timerOnTab == null) {
                    timerOnTab = new Timer(components);
                    timerOnTab.Tick += timerOnTab_Tick;
                }
                timerOnTab.Enabled = false;
                timerOnTab.Interval = Config.Tabs.DragOverTabOpensSDT ? INTERVAL_SHOWMENU : INTERVAL_SELCTTAB;
                timerOnTab.Enabled = true;
                if(Config.Tabs.DragOverTabOpensSDT && (iState != -1)) {
                    Rectangle tabRect = tabControl1.GetTabRect(tab);
                    Point lpPoints = new Point(tabRect.X + ((tabRect.Width * 3) / 4), tabRect.Bottom + 0x10);
                    string[] strArray = QTUtility.TextResourcesDic["DragDropToolTip"];
                    string str;
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
                    if(toolTipForDD == null) {
                        toolTipForDD = new ToolTip(components);
                        toolTipForDD.UseAnimation = toolTipForDD.UseFading = false;
                    }
                    toolTipForDD.ToolTipTitle = str;
                    if(PInvoke.GetForegroundWindow() != ExplorerHandle) {
                        Type type = typeof(ToolTip);
                        BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Instance;
                        MethodInfo method = type.GetMethod("SetTrackPosition", bindingAttr);
                        MethodInfo info2 = type.GetMethod("SetTool", bindingAttr);
                        PInvoke.MapWindowPoints(tabControl1.Handle, IntPtr.Zero, ref lpPoints, 1);
                        method.Invoke(toolTipForDD, new object[] { lpPoints.X, lpPoints.Y });
                        info2.Invoke(toolTipForDD, new object[] { tabControl1, tab.CurrentPath, 2, lpPoints });
                    }
                    else {
                        toolTipForDD.Active = true;
                        toolTipForDD.Show(tab.CurrentPath, tabControl1, lpPoints);
                    }
                }
            }
        }

        private void subDirTip_MenuItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            QMenuItem clickedItem = (QMenuItem)e.ClickedItem;
            if(clickedItem.Target == MenuTarget.Folder) {
                if(clickedItem.IDLData != null) {
                    using(IDLWrapper wrapper = new IDLWrapper(clickedItem.IDLData)) {
                        ShellBrowser.Navigate(wrapper);
                    }
                    return;
                }
                string targetPath = clickedItem.TargetPath;
                Keys modifierKeys = ModifierKeys;
                bool flag = (subDirTip_Tab != null) && (sender == subDirTip_Tab);
                if((modifierKeys & Keys.Control) == Keys.Control) {
                    using(IDLWrapper wrapper2 = new IDLWrapper(targetPath)) {
                        OpenNewWindow(wrapper2);
                        return;
                    }
                }
                if((modifierKeys & Keys.Shift) == Keys.Shift) {
                    using(IDLWrapper wrapper3 = new IDLWrapper(targetPath)) {
                        OpenNewTab(wrapper3, false, true);
                        return;
                    }
                }
                if((!flag || (ContextMenuedTab == CurrentTab)) && CurrentTab.TabLocked) {
                    CloneTabButton(CurrentTab, targetPath, true, tabControl1.SelectedIndex + 1);
                    return;
                }
                if(flag && (ContextMenuedTab != CurrentTab)) {
                    if(ContextMenuedTab != null) {
                        if(ContextMenuedTab.TabLocked) {
                            CloneTabButton(ContextMenuedTab, targetPath, true, tabControl1.TabPages.IndexOf(ContextMenuedTab) + 1);
                            return;
                        }
                        NowTabCloned = targetPath == CurrentAddress;
                        ContextMenuedTab.NavigatedTo(targetPath, null, -1, false);
                        tabControl1.SelectTab(ContextMenuedTab);
                    }
                    return;
                }
                using(IDLWrapper wrapper4 = new IDLWrapper(targetPath)) {
                    ShellBrowser.Navigate(wrapper4);
                    return;
                }
            }
            try {
                string path = clickedItem.Path;
                ProcessStartInfo startInfo = new ProcessStartInfo(path);
                startInfo.WorkingDirectory = Path.GetDirectoryName(path);
                startInfo.ErrorDialog = true;
                startInfo.ErrorDialogParentHandle = ExplorerHandle;
                Process.Start(startInfo);
                if(Config.Misc.KeepRecentFiles) {
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
                    e.HRESULT = ShellMethods.PopUpSystemContextMenu(wrapper, e.IsKey ? e.Point : MousePosition, ref iContextMenu2, ((SubDirTipForm)sender).Handle, false);
                }
            }
        }

        private void subDirTip_MultipleMenuItemsClicked(object sender, EventArgs e) {
            List<string> executedDirectories = ((SubDirTipForm)sender).ExecutedDirectories;
            if((ModifierKeys & Keys.Control) == Keys.Control) {
                QTUtility2.InitializeTemporaryPaths();
                QTUtility.TMPPathList.AddRange(executedDirectories);
                using(IDLWrapper wrapper = new IDLWrapper(executedDirectories[0])) {
                    OpenNewWindow(wrapper);
                    return;
                }
            }
            bool flag = true;
            foreach(string str in executedDirectories) {
                OpenNewTab(str, !flag);
                flag = false;
            }
        }

        private void subDirTip_MultipleMenuItemsRightClicked(object sender, ItemRightClickedEventArgs e) {
            List<string> executedDirectories = ((SubDirTipForm)sender).ExecutedDirectories;
            e.HRESULT = ShellMethods.PopUpSystemContextMenu(executedDirectories, e.IsKey ? e.Point : MousePosition, ref iContextMenu2, ((SubDirTipForm)sender).Handle);
        }

        private void subDirTip_Tab_MenuClosed(object sender, EventArgs e) {
            tabControl1.SetSubDirTipShown(false);
            tabControl1.RefreshFolderImage();
        }

        private static void SyncButtonBarBroadCast(int mask) {
            int num = mask << 0x10;
            if(((mask & 1) == 1) && (GroupsManager.GroupCount > 0)) {
                num++;
            }
            if(((mask & 2) == 2) && (QTUtility.ClosedTabHistoryList.Count > 0)) {
                num += 2;
            }
            if(((mask & 4) == 4) && (QTUtility.UserAppsDic.Count > 0)) {
                num += 4;
            }
            if(((mask & 8) == 8) && Config.ShowTooltips) {
                num += 8;
            }
            try {
                foreach(IntPtr ptr in InstanceManager.ButtonBarHandles().Where(PInvoke.IsWindow)) {
                    QTUtility2.SendCOPYDATASTRUCT(ptr, (IntPtr)1, "fromTabBC", (IntPtr)num);
                }
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception);
            }
        }

        private bool SyncButtonBarCurrent(int mask) {
            IntPtr ptr;
            bool flag = false;
            if(InstanceManager.TryGetButtonBarHandle(ExplorerHandle, out ptr)) {
                int num = mask << 0x10;
                if(mask != 0x100) {
                    int index = tabControl1.TabPages.IndexOf(CurrentTab);
                    int tabCount = tabControl1.TabCount;
                    if((navBtnsFlag & 1) != 0) {
                        num++;
                    }
                    if((navBtnsFlag & 2) != 0) {
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
                    if(!Config.Window.CloseBtnClosesSingleTab || (tabCount > 1)) {
                        num += 0x20;
                    }
                    if(NowTopMost) {
                        num += 0x40;
                    }
                    if((((mask & 0x80) != 0) && (CurrentTab != null)) && ((CurrentTab.CurrentIDL != null) && (CurrentTab.CurrentIDL.Length == 2))) {
                        num += 0x80;
                    }
                }
                QTUtility2.SendCOPYDATASTRUCT(ptr, (IntPtr)2, "fromTab", (IntPtr)num);
                flag = true;
            }
            if(((mask & 0x10000) == 0x10000) && tabControl1.AutoSubText) {
                QTabItem.CheckSubTexts(tabControl1);
            }
            return flag;
        }

        private static void SyncTabBarBroadcast(IntPtr hwndThis) {
            try {
                foreach(IntPtr ptr in InstanceManager.TabBarHandles().Where(PInvoke.IsWindow)) {
                    if(ptr != hwndThis) {
                        QTUtility2.SendCOPYDATASTRUCT(ptr, (IntPtr)0x11, "refresh", IntPtr.Zero);
                    }
                }
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception);
            }
        }

        private static void SyncTabBarBroadcastPlugin(IntPtr hwndThis) {
            try {
                foreach(IntPtr ptr in InstanceManager.TabBarHandles()) {
                    if(ptr != hwndThis) {
                        QTUtility2.SendCOPYDATASTRUCT(ptr, (IntPtr)0x12, "refreshPlugin", IntPtr.Zero);
                    }
                }
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception);
            }
        }

        internal static void SyncTaskBarMenu() {
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(RegConst.Root)) {
                IntPtr hWnd = QTUtility2.ReadRegHandle("TaskBarHandle", key);
                if((hWnd != IntPtr.Zero) && PInvoke.IsWindow(hWnd)) {
                    QTUtility2.SendCOPYDATASTRUCT(hWnd, (IntPtr)3, string.Empty, IntPtr.Zero);
                }
            }
        }

        private void SyncToolbarTravelButton() {
            if(!QTUtility.IsXP) {
                IntPtr ptr = (IntPtr)0x10001;
                IntPtr ptr2 = (IntPtr)0x10000;
                bool flag = (navBtnsFlag & 1) != 0;
                bool flag2 = (navBtnsFlag & 2) != 0;
                PInvoke.SendMessage(TravelToolBarHandle, 0x401, (IntPtr)0x100, flag ? ptr : ptr2);
                PInvoke.SendMessage(TravelToolBarHandle, 0x401, (IntPtr)0x101, flag2 ? ptr : ptr2);
                PInvoke.SendMessage(TravelToolBarHandle, 0x401, (IntPtr)0x102, (flag || flag2) ? ptr : ptr2);
            }
        }

        private void SyncTravelState() {
            if(CurrentTab != null) {
                navBtnsFlag = ((CurrentTab.HistoryCount_Back > 1) ? 1 : 0) | ((CurrentTab.HistoryCount_Forward > 0) ? 2 : 0);
                if(Config.Tabs.ShowNavButtons && (toolStrip != null)) {
                    buttonBack.Enabled = (navBtnsFlag & 1) != 0;
                    buttonForward.Enabled = (navBtnsFlag & 2) != 0;
                    buttonNavHistoryMenu.Enabled = navBtnsFlag != 0;
                }
                SyncButtonBarCurrent(0x100bf);
                SyncToolbarTravelButton();
            }
        }

        private void tabControl1_CloseButtonClicked(object sender, QTabCancelEventArgs e) {
            if(NowTabDragging) {
                Cursor = Cursors.Default;
                NowTabDragging = false;
                DraggingTab = null;
                DraggingDestRect = Rectangle.Empty;
                SyncButtonBarCurrent(12);
                e.Cancel = true;
            }
            else if(!Explorer.Busy) {
                QTabItem tabPage = (QTabItem)e.TabPage;
                if(tabControl1.TabCount > 1) {
                    e.Cancel = !CloseTab(tabPage);
                }
                else {
                    WindowUtils.CloseExplorer(ExplorerHandle, 1);
                }
            }
        }

        private void tabControl1_Deselecting(object sender, QTabCancelEventArgs e) {
            if(e.TabPageIndex != -1) {
                SaveSelectedItems((QTabItem)e.TabPage);
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
            if((ModifierKeys != Keys.Control) && (e.Button == MouseButtons.Left)) {
                QTabItem tabMouseOn = (QTabItem)tabControl1.GetTabMouseOn();
                if(tabMouseOn != null) {
                    MouseChord chord = QTUtility.MakeMouseChord(MouseChord.Double, ModifierKeys);
                    BindAction action;
                    if(Config.Mouse.TabActions.TryGetValue(chord, out action)) {
                        DoBindAction(action, false, DraggingTab);
                    }
                }
                else {
                    OnMouseDoubleClick(e);
                }
            }
        }

        private void tabControl1_MouseDown(object sender, MouseEventArgs e) {
            QTabItem tabMouseOn = (QTabItem)tabControl1.GetTabMouseOn();
            DraggingTab = null;
            if(tabMouseOn != null) {
                if(e.Button == MouseButtons.Left) {
                    NowTabDragging = true;
                    DraggingTab = tabMouseOn;
                }
                else if(e.Button == MouseButtons.Right) {
                    ContextMenuedTab = tabMouseOn;
                }
            }
        }

        private void tabControl1_MouseEnter(object sender, EventArgs e) {
            if(pluginManager != null) {
                pluginManager.OnMouseEnter();
            }
        }

        private void tabControl1_MouseLeave(object sender, EventArgs e) {
            if(pluginManager != null) {
                pluginManager.OnMouseLeave();
            }
        }

        private void tabControl1_MouseMove(object sender, MouseEventArgs e) {
            RECT rect;
            if((tabControl1.Capture && (((e.X < 0) || (e.Y < 0)) || ((e.X > tabControl1.Width) || (e.Y > tabControl1.Height)))) && (PInvoke.GetWindowRect(ReBarHandle, out rect) && !PInvoke.PtInRect(ref rect, tabControl1.PointToScreen(e.Location)))) {
                Cursor = Cursors.Default;
                tabControl1.Capture = false;
            }
            else if((NowTabDragging && (DraggingTab != null)) && ((ModifierKeys & Keys.Shift) != Keys.Shift)) {
                if(Explorer.Busy || (MouseButtons != MouseButtons.Left)) {
                    NowTabDragging = false;
                    // Leave DraggingTab set so MouseUp doesn't get confused.
                    // It will be unset in MouseUp.
                }
                else {
                    int num;
                    QTabItem tabMouseOn = (QTabItem)tabControl1.GetTabMouseOn(out num);
                    int index = tabControl1.TabPages.IndexOf(DraggingTab);
                    if((num > (tabControl1.TabCount - 1)) || (num < 0)) {
                        if((num == -1) && (ModifierKeys == Keys.Control)) {
                            Cursor = GetCursor(false);
                            DraggingDestRect = new Rectangle(1, 0, 0, 0);
                        }
                        else {
                            Cursor = Cursors.Default;
                        }
                    }
                    else if((index <= (tabControl1.TabCount - 1)) && (index >= 0)) {
                        Rectangle tabRect = tabControl1.GetTabRect(num, false);
                        Rectangle rectangle2 = tabControl1.GetTabRect(index, false);
                        if(tabMouseOn != null) {
                            if(tabMouseOn != DraggingTab) {
                                if(!DraggingDestRect.Contains(tabControl1.PointToClient(MousePosition))) {
                                    Cursor = GetCursor(true);
                                    bool flag = tabMouseOn.Row != DraggingTab.Row;
                                    bool flag2 = tabControl1.SelectedTab != DraggingTab;
                                    tabControl1.TabPages.Relocate(index, num);
                                    if(num < index) {
                                        DraggingDestRect = new Rectangle(tabRect.X + rectangle2.Width, tabRect.Y, tabRect.Width - rectangle2.Width, tabRect.Height);
                                    }
                                    else {
                                        DraggingDestRect = new Rectangle(tabRect.X, tabRect.Y, tabRect.Width - rectangle2.Width, tabRect.Height);
                                    }
                                    if((flag && !flag2) && !Config.Tabs.MultipleTabRows) {
                                        Rectangle rectangle3 = tabControl1.GetTabRect(num, false);
                                        Point p = new Point(rectangle3.X + (rectangle3.Width / 2), rectangle3.Y + (Config.Skin.TabHeight / 2));
                                        Cursor.Position = tabControl1.PointToScreen(p);
                                    }
                                    SyncButtonBarCurrent(12);
                                }
                            }
                            else if((curTabCloning != null) && (Cursor == curTabCloning)) {
                                Cursor = GetCursor(true);
                            }
                        }
                    }
                }
            }
        }

        private void tabControl1_MouseUp(object sender, MouseEventArgs e) {
            QTabItem tabMouseOn = (QTabItem)tabControl1.GetTabMouseOn();
            if(NowTabDragging && e.Button == MouseButtons.Left) {
                Keys modifierKeys = ModifierKeys;
                if(tabMouseOn == null) {
                    if(DraggingTab != null && (modifierKeys == Keys.Control || modifierKeys == (Keys.Control | Keys.Shift))) {
                        bool cloning = false;
                        Point pt = tabControl1.PointToScreen(e.Location);
                        if(!QTUtility.IsXP) {
                            RECT rect;
                            PInvoke.GetWindowRect(ReBarHandle, out rect);
                            cloning = PInvoke.PtInRect(ref rect, pt);
                        }
                        else {
                            RECT rect2;
                            IntPtr ptr;
                            if(InstanceManager.TryGetButtonBarHandle(ExplorerHandle, out ptr) && PInvoke.IsWindowVisible(ptr)) {
                                PInvoke.GetWindowRect(ptr, out rect2);
                                if(PInvoke.PtInRect(ref rect2, pt)) {
                                    cloning = true;
                                }
                            }
                            PInvoke.GetWindowRect(Handle, out rect2);
                            if(PInvoke.PtInRect(ref rect2, pt)) {
                                cloning = true;
                            }
                        }
                        if(cloning) {
                            CloneTabButton(DraggingTab, null, false, tabControl1.TabCount);
                        }
                    }
                } 
                else if(tabMouseOn == DraggingTab && DraggingDestRect == Rectangle.Empty) {
                    MouseChord chord = QTUtility.MakeMouseChord(MouseChord.Left, ModifierKeys);
                    BindAction action;
                    if(Config.Mouse.TabActions.TryGetValue(chord, out action)) {
                        DoBindAction(action, false, DraggingTab);
                    }
                }
                NowTabDragging = false;
                DraggingTab = null;
                DraggingDestRect = Rectangle.Empty;
                SyncButtonBarCurrent(12);
            }
            else if(e.Button == MouseButtons.Middle && !Explorer.Busy && tabMouseOn != null) {
                DraggingTab = null;
                NowTabDragging = false;
                MouseChord chord = QTUtility.MakeMouseChord(MouseChord.Middle, ModifierKeys);
                BindAction action;
                if(Config.Mouse.TabActions.TryGetValue(chord, out action)) {
                    DoBindAction(action, false, tabMouseOn);
                }
            }
            else if(tabMouseOn == null) {
                NowTabDragging = false;
                if(DraggingTab == null) OnMouseUp(e); // This will prevent the bar's MouseUp from 
                DraggingTab = null;                   // firing if the MouseDown was on a tab.
            }
            Cursor = Cursors.Default;
        }

        private void tabControl1_PointedTabChanged(object sender, QTabCancelEventArgs e) {
            if(pluginManager != null) {
                if(e.Action == TabControlAction.Selecting) {
                    QTabItem tabPage = (QTabItem)e.TabPage;
                    pluginManager.OnPointedTabChanged(e.TabPageIndex, tabPage.CurrentIDL, tabPage.CurrentPath);
                }
                else if(e.Action == TabControlAction.Deselecting) {
                    pluginManager.OnPointedTabChanged(-1, null, string.Empty);
                }
            }
        }

        private void tabControl1_RowCountChanged(object sender, QEventArgs e) {
            SetBarRows(e.RowCount);
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e) {
            QTabItem selectedTab = (QTabItem)tabControl1.SelectedTab;
            string currentPath = selectedTab.CurrentPath;
            if(IsSpecialFolderNeedsToTravel(currentPath) && LogEntryDic.ContainsKey(selectedTab.GetLogHash(true, 0))) {
                NavigatedByCode = true;
                CurrentTab = selectedTab;
                while(lstActivatedTabs.Remove(CurrentTab)) {
                }
                lstActivatedTabs.Add(CurrentTab);
                if(lstActivatedTabs.Count > 15) {
                    lstActivatedTabs.RemoveAt(0);
                }
                fNavigatedByTabSelection = NavigateToPastSpecialDir(CurrentTab.GetLogHash(true, 0));
                if(pluginManager != null) {
                    pluginManager.OnTabChanged(tabControl1.SelectedIndex, selectedTab.CurrentIDL, selectedTab.CurrentPath);
                }
                if(tabControl1.Focused) {
                    listView.SetFocus();
                }
            }
            else {
                IDLWrapper idlw = null;
                if((selectedTab.CurrentIDL != null) && (selectedTab.CurrentIDL.Length > 0)) {
                    idlw = new IDLWrapper(selectedTab.CurrentIDL);
                }
                if((idlw == null) || !idlw.Available) {
                    idlw = new IDLWrapper(selectedTab.CurrentPath);
                }
                using(idlw) {
                    if(!idlw.Available) {
                        CancelFailedTabChanging(currentPath);
                        return;
                    }
                    CurrentTab = selectedTab;
                    while(lstActivatedTabs.Remove(CurrentTab)) {
                    }
                    lstActivatedTabs.Add(CurrentTab);
                    if(lstActivatedTabs.Count > 15) {
                        lstActivatedTabs.RemoveAt(0);
                    }
                    if(((currentPath != CurrentAddress) || (QTUtility.IsXP && (currentPath == QTUtility.PATH_SEARCHFOLDER))) || NowTabCloned) {
                        NavigatedByCode = true;
                        fNavigatedByTabSelection = true;
                        NowTabCloned = false;
                        if(ShellBrowser.Navigate(idlw) != 0) {
                            CancelFailedTabChanging(currentPath);
                            return;
                        }
                    }
                    else {
                        SyncTravelState();
                    }
                }
                if(tabControl1.Focused) {
                    listView.SetFocus();
                }
                if(pluginManager != null) {
                    pluginManager.OnTabChanged(tabControl1.SelectedIndex, CurrentTab.CurrentIDL, CurrentTab.CurrentPath);
                }
            }
        }

        private void tabControl1_Selecting(object sender, QTabCancelEventArgs e) {
            if(NowTabsAddingRemoving) {
                e.Cancel = true;
            }
        }

        private void tabControl1_TabCountChanged(object sender, QTabCancelEventArgs e) {
            if(pluginManager != null) {
                QTabItem tabPage = (QTabItem)e.TabPage;
                if(e.Action == TabControlAction.Selected) {
                    pluginManager.OnTabAdded(e.TabPageIndex, tabPage.CurrentIDL, tabPage.CurrentPath);
                }
                else if(e.Action == TabControlAction.Deselected) {
                    pluginManager.OnTabRemoved(e.TabPageIndex, tabPage.CurrentIDL, tabPage.CurrentPath);
                }
            }
        }

        private void tabControl1_TabIconMouseDown(object sender, QTabCancelEventArgs e) {
            ShowSubdirTip_Tab((QTabItem)e.TabPage, e.Action == TabControlAction.Selecting, e.TabPageIndex, false, e.Cancel);
        }

        private void tabSwitcher_Switched(object sender, ItemCheckEventArgs e) {
            tabControl1.SelectedIndex = e.Index;
        }

        private void timerOnTab_Tick(object sender, EventArgs e) {
            timerOnTab.Enabled = false;
            QTabItem tabMouseOn = (QTabItem)tabControl1.GetTabMouseOn();
            if(((tabMouseOn != null) && (tabMouseOn == tabForDD)) && tabControl1.TabPages.Contains(tabMouseOn)) {
                if(Config.Tabs.DragOverTabOpensSDT) {
                    WindowUtils.BringExplorerToFront(ExplorerHandle);
                    ShowSubdirTip_Tab(tabMouseOn, true, tabControl1.TabOffset, false, fToggleTabMenu);
                    fToggleTabMenu = !fToggleTabMenu;
                    timerOnTab.Enabled = true;
                    if(toolTipForDD != null) {
                        toolTipForDD.Active = false;
                    }
                }
                else {
                    tabControl1.SelectTab(tabMouseOn);
                }
            }
        }

        private void timerSelectionChanged_Tick(object sender, EventArgs e) {
            try {
                timerSelectionChanged.Enabled = false;
                if((pluginManager != null) && (CurrentTab != null)) {
                    pluginManager.OnSelectionChanged(tabControl1.SelectedIndex, CurrentTab.CurrentIDL, CurrentTab.CurrentPath);
                }
            }
            catch {
            }
        }

        private void ToggleTopMost() {
            if(PInvoke.Ptr_OP_AND(PInvoke.GetWindowLongPtr(ExplorerHandle, -20), 8) != IntPtr.Zero) {
                PInvoke.SetWindowPos(ExplorerHandle, (IntPtr)(-2), 0, 0, 0, 0, 3);
                NowTopMost = false;
            }
            else {
                PInvoke.SetWindowPos(ExplorerHandle, (IntPtr)(-1), 0, 0, 0, 0, 3);
                NowTopMost = true;
            }
        }

        public override int TranslateAcceleratorIO(ref MSG msg) {
            if(msg.message == WM.KEYDOWN) {
                Keys wParam = (Keys)((int)((long)msg.wParam));
                bool flag = (((int)((long)msg.lParam)) & 0x40000000) != 0;
                switch(wParam) {
                    case Keys.Delete: {
                            if(!tabControl1.Focused || ((subDirTip_Tab != null) && subDirTip_Tab.MenuIsShowing)) {
                                break;
                            }
                            int focusedTabIndex = tabControl1.GetFocusedTabIndex();
                            if((-1 < focusedTabIndex) && (focusedTabIndex < tabControl1.TabCount)) {
                                bool flag3 = focusedTabIndex == (tabControl1.TabCount - 1);
                                if(CloseTab((QTabItem)tabControl1.TabPages[focusedTabIndex]) && flag3) {
                                    tabControl1.FocusNextTab(true, false, false);
                                }
                            }
                            return 0;
                        }
                    case Keys.Apps:
                        if(!flag) {
                            int index = tabControl1.GetFocusedTabIndex();
                            if((-1 >= index) || (index >= tabControl1.TabCount)) {
                                break;
                            }
                            ContextMenuedTab = (QTabItem)tabControl1.TabPages[index];
                            Rectangle tabRect = tabControl1.GetTabRect(index, true);
                            contextMenuTab.Show(PointToScreen(new Point(tabRect.Right + 10, tabRect.Bottom - 10)));
                        }
                        return 0;

                    case Keys.F6:
                    case Keys.Tab:
                    case Keys.Left:
                    case Keys.Right: {
                            if(!tabControl1.Focused || ((subDirTip_Tab != null) && subDirTip_Tab.MenuIsShowing)) {
                                break;
                            }
                            bool fBack = (ModifierKeys == Keys.Shift) || (wParam == Keys.Left);
                            if(!tabControl1.FocusNextTab(fBack, false, false)) {
                                break;
                            }
                            return 0;
                        }
                    case Keys.Back:
                        return 0;

                    case Keys.Return:
                    case Keys.Space:
                        if(!flag && !tabControl1.SelectFocusedTab()) {
                            break;
                        }
                        listView.SetFocus();
                        return 0;

                    case Keys.Escape:
                        if(tabControl1.Focused && ((subDirTip_Tab == null) || !subDirTip_Tab.MenuIsShowing)) {
                            listView.SetFocus();
                        }
                        break;

                    case Keys.End:
                    case Keys.Home:
                        if((!tabControl1.Focused || ((subDirTip_Tab != null) && subDirTip_Tab.MenuIsShowing)) || !tabControl1.FocusNextTab(wParam == Keys.Home, false, true)) {
                            break;
                        }
                        return 0;

                    case Keys.Up:
                    case Keys.Down:
                        if(((!Config.Tabs.ShowSubDirTipOnTab || !tabControl1.Focused) || ((subDirTip_Tab != null) && subDirTip_Tab.MenuIsShowing)) || (!flag && !tabControl1.PerformFocusedFolderIconClick(wParam == Keys.Up))) {
                            break;
                        }
                        return 0;
                }
            }
            return base.TranslateAcceleratorIO(ref msg);
        }

        private bool travelBtnController_MessageCaptured(ref Message m) {
            if(CurrentTab == null) {
                return false;
            }
            switch(m.Msg) {
                case WM.LBUTTONDOWN:
                case WM.LBUTTONUP: {
                        Point pt = QTUtility2.PointFromLPARAM(m.LParam);
                        int num = (int)PInvoke.SendMessage(travelBtnController.Handle, 0x445, IntPtr.Zero, ref pt);
                        bool flag = CurrentTab.HistoryCount_Back > 1;
                        bool flag2 = CurrentTab.HistoryCount_Forward > 0;
                        if(m.Msg != 0x202) {
                            PInvoke.SetCapture(travelBtnController.Handle);
                            if(((flag && (num == 0)) || (flag2 && (num == 1))) || ((flag || flag2) && (num == 2))) {
                                int num5 = (int)PInvoke.SendMessage(travelBtnController.Handle, 0x412, (IntPtr)(0x100 + num), IntPtr.Zero);
                                int num6 = num5 | 2;
                                PInvoke.SendMessage(travelBtnController.Handle, 0x411, (IntPtr)(0x100 + num), (IntPtr)num6);
                            }
                            if((num == 2) && (flag || flag2)) {
                                RECT rect;
                                IntPtr hWnd = PInvoke.SendMessage(travelBtnController.Handle, 0x423, IntPtr.Zero, IntPtr.Zero);
                                if(hWnd != IntPtr.Zero) {
                                    PInvoke.SendMessage(hWnd, 0x41c, IntPtr.Zero, IntPtr.Zero);
                                }
                                PInvoke.GetWindowRect(travelBtnController.Handle, out rect);
                                NavigationButtons_DropDownOpening(buttonNavHistoryMenu, new EventArgs());
                                buttonNavHistoryMenu.DropDown.Show(new Point(rect.left - 2, rect.bottom + 1));
                            }
                            break;
                        }
                        PInvoke.ReleaseCapture();
                        for(int i = 0; i < 3; i++) {
                            int num3 = (int)PInvoke.SendMessage(travelBtnController.Handle, 0x412, (IntPtr)(0x100 + i), IntPtr.Zero);
                            int num4 = num3 & -3;
                            PInvoke.SendMessage(travelBtnController.Handle, 0x411, (IntPtr)(0x100 + i), (IntPtr)num4);
                        }
                        if((num == 0) && flag) {
                            NavigateCurrentTab(true);
                        }
                        else if((num == 1) && flag2) {
                            NavigateCurrentTab(false);
                        }
                        break;
                    }
                case WM.LBUTTONDBLCLK:
                    m.Result = IntPtr.Zero;
                    return true;

                case WM.USER+1:
                    if(((((int)((long)m.LParam)) >> 0x10) & 0xffff) == 1) {
                        return false;
                    }
                    m.Result = (IntPtr)1;
                    return true;

                case WM.MOUSEACTIVATE:
                    if(buttonNavHistoryMenu.DropDown.Visible) {
                        m.Result = (IntPtr)4;
                        buttonNavHistoryMenu.DropDown.Close(ToolStripDropDownCloseReason.AppClicked);
                        return true;
                    }
                    return false;

                case WM.NOTIFY: {
                        NMHDR nmhdr = (NMHDR)Marshal.PtrToStructure(m.LParam, typeof(NMHDR));
                        if(nmhdr.code != -530) {
                            return false;
                        }
                        NMTTDISPINFO nmttdispinfo = (NMTTDISPINFO)Marshal.PtrToStructure(m.LParam, typeof(NMTTDISPINFO));
                        string str;
                        if(nmttdispinfo.hdr.idFrom == ((IntPtr)0x100)) {
                            str = MakeTravelBtnTooltipText(true);
                            if(str.Length > 0x4f) {
                                str = "Back";
                            }
                        }
                        else if(nmttdispinfo.hdr.idFrom == ((IntPtr)0x101)) {
                            str = MakeTravelBtnTooltipText(false);
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
                    return false;
            }
            m.Result = IntPtr.Zero;
            return true;
       }

        private bool FolderLinkClicked(IDLWrapper wrapper, Keys modifierKeys, bool middle) {
            MouseChord chord = QTUtility.MakeMouseChord(middle ? MouseChord.Middle : MouseChord.Left, modifierKeys);
            BindAction action;
            if(Config.Mouse.LinkActions.TryGetValue(chord, out action)) {
                DoBindAction(action, false, null, wrapper);
                return true;
            }
            else {
                return false;
            }
        }

        internal bool TryGetSelection(out Address[] adSelectedItems, out string pathFocused, bool fDisplayName) {
            return ShellBrowser.TryGetSelection(out adSelectedItems, out pathFocused, fDisplayName);
        }

        private void tsmiBranchRoot_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            QTabItem tag = (QTabItem)((ToolStripMenuItem)sender).Tag;
            if(tag != null) {
                NavigateBranches(tag, ((QMenuItem)e.ClickedItem).MenuItemArguments.Index);
            }
        }

        public override void UIActivateIO(int fActivate, ref MSG Msg) {
            if(fActivate != 0) {
                tabControl1.Focus();
                tabControl1.FocusNextTab(ModifierKeys == Keys.Shift, true, false);
            }
        }

        [ComUnregisterFunction]
        private static void Unregister(Type t) {
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
#if false
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
#endif
        }

        private void UpOneLevel() {
            if(CurrentTab.TabLocked) {
                QTabItem tab = CurrentTab.Clone();
                AddInsertTab(tab);
                tabControl1.SelectTab(tab);
            }
            if(!QTUtility.IsXP) {
                PInvoke.SendMessage(WindowUtils.GetShellTabWindowClass(ExplorerHandle), 0x111, (IntPtr)0xa022, IntPtr.Zero);
            }
            else {
                PInvoke.SendMessage(ExplorerHandle, 0x111, (IntPtr)0xa022, IntPtr.Zero);
            }
        }

        internal static void WaitTimeout(int msec) {
            Thread.Sleep(msec);
        }

        protected override void WndProc(ref Message m) {
            try {
                bool flag;
                switch(m.Msg) {

                    case WM.APP + 1:
                        NowModalDialogShown = m.WParam != IntPtr.Zero;
                        return;

                    case WM.DROPFILES:
                        HandleFileDrop(m.WParam);
                        break;

                    case WM.DRAWITEM:
                    case WM.MEASUREITEM:
                    case WM.INITMENUPOPUP:
                        if((iContextMenu2 != null) && (m.HWnd == Handle)) {
                            try {
                                iContextMenu2.HandleMenuMsg(m.Msg, m.WParam, m.LParam);
                            }
                            catch {
                            }
                            return;
                        }
                        break;
                }
                
                if(m.Msg == WM_NEWWINDOW) {
                    using(IDLWrapper wrapper = new IDLWrapper(PInvoke.ILClone(m.LParam))) {
                        if(!Config.Window.CaptureNewWindows
                                || QTUtility2.IsShellPathButNotFileSystem(wrapper.Path)
                                || wrapper.Path.PathEquals(QTUtility.PATH_SEARCHFOLDER)
                                || QTUtility.NoCapturePathsList.Any(path => wrapper.Path.PathEquals(path))
                                || ModifierKeys == Keys.Control) {
                            m.Result = IntPtr.Zero;
                        }
                        else {
                            fNeedsNewWindowPulse = true;
                            OpenNewTab(wrapper);
                            WindowUtils.BringExplorerToFront(ExplorerHandle);
                            m.Result = (IntPtr)1;
                        }
                    }
                    return;
                }
                
                if(m.Msg != WM.COPYDATA) {
                    base.WndProc(ref m);
                    return;
                }
                if(!NowModalDialogShown) {
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
                                            OpenGroup(str2, false);
                                        }
                                        flag = true;
                                        goto Label_0B07;
                                    }
                                    NowOpenedByGroupOpener = true;
                                    new MethodInvoker(CallbackFirstNavComp).BeginInvoke(AsyncComplete, strArray);
                                    return;
                                }
                            case 1:
                                goto Label_0B07;

                            case 3:
                                OpenGroup(str, true);
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
                                                bool flag4 = OpenNewTab(wrapper5, blockSelecting, wParam == 0x40);
                                                if((wParam == 0x40) && !flag4) {
                                                    m.Result = (IntPtr)1;
                                                }
                                                flag = true;
                                            }
                                            else {
                                                OpenNewWindow(wrapper5);
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
                                            OpenNewWindow(wrapper6);
                                            return;
                                        }
                                    }
                                    bool flag5 = (dwData & Keys.Shift) != Keys.None;
                                    using(IDLWrapper wrapper7 = new IDLWrapper(QTUtility.TMPTargetIDL)) {
                                        if(wrapper7.HasPath && QTUtility2.TargetIsInNoCapture(IntPtr.Zero, wrapper7.Path)) {
                                            OpenNewWindow(wrapper7);
                                            return;
                                        }
                                        OpenNewTab(wrapper7, flag5);
                                    }
                                    foreach(byte[] buffer2 in QTUtility.TMPIDLList) {
                                        using(IDLWrapper wrapper8 = new IDLWrapper(buffer2)) {
                                            OpenNewTab(wrapper8, true);
                                            continue;
                                        }
                                    }
                                    if(flag5) {
                                        WindowUtils.BringExplorerToFront(ExplorerHandle);
                                    }
                                }
                                return;

                            case 15: {
                                    string[] strArray2 = str.Split(QTUtility.SEPARATOR_CHAR);
                                    if((strArray2.Length == 2) && (strArray2[1].Length > 0)) {
                                        QTUtility.PathToSelectInCommandLineArg = strArray2[1];
                                    }
                                    if(ModifierKeys != Keys.Control) {
                                        OpenNewTab(strArray2[0], false);
                                        flag = true;
                                    }
                                    else {
                                        using(IDLWrapper wrapper9 = new IDLWrapper(strArray2[0])) {
                                            OpenNewWindow(wrapper9);
                                        }
                                    }
                                    if(!Config.NoNewWndFolderTree) {
                                        ShowFolderTree(true);
                                    }
                                    goto Label_0B07;
                                }
                            case 0x10:
                                if((Config.Window.TrayOnClose && (notifyIcon != null)) && (dicNotifyIcon != null)) {
                                    Dictionary<IntPtr, int> dictionary = new Dictionary<IntPtr, int>(dicNotifyIcon);
                                    foreach(IntPtr ptr in dictionary.Keys) {
                                        QTTabBarClass tabBar = InstanceManager.GetTabBar(ptr);
                                        if((tabBar != null) && (tabBar.CurrentAddress == str)) {
                                            ShowTaskbarItem(ptr, true);
                                            return;
                                        }
                                    }
                                }
                                OpenNewTab(str);
                                QTUtility.RegisterPrimaryInstance(ExplorerHandle, this);
                                flag = true;
                                goto Label_0B07;

                            case 0x11:
                                RefreshTabBar();
                                return;

                            case 0x12:
                                RefreshPlugins(false);
                                return;

                            case 80:
                                ReplaceByGroup(str);
                                return;

                            case 0x20:
                                using(RegistryKey key = Registry.CurrentUser.OpenSubKey(RegConst.Root, false)) {
                                    if(key != null) {
                                        string[] collection = ((string)key.GetValue("TabsOnLastClosedWindow", string.Empty)).Split(QTUtility.SEPARATOR_CHAR);
                                        if((collection.Length > 0) && (collection[0].Length > 0)) {
                                            if(copydatastruct.dwData == IntPtr.Zero) {
                                                QTUtility.TMPPathList = new List<string>(collection);
                                                using(IDLWrapper wrapper10 = new IDLWrapper(collection[0])) {
                                                    OpenNewWindow(wrapper10);
                                                    return;
                                                }
                                            }
                                            new MethodInvoker(CallbackFirstNavComp).BeginInvoke(AsyncComplete_MultiPath, new object[] { collection, 0 });
                                        }
                                    }
                                }
                                return;

                            case 0x30: {
                                    contextMenuNotifyIcon = new ContextMenuStripEx(null, false);
                                    contextMenuNotifyIcon.ImageList = QTUtility.ImageListGlobal;
                                    contextMenuNotifyIcon.ItemClicked += contextMenuNotifyIcon_ItemClicked;
                                    notifyIcon = new NotifyIcon();
                                    notifyIcon.Icon = icoNotify;
                                    notifyIcon.ContextMenuStrip = contextMenuNotifyIcon;
                                    notifyIcon.MouseDoubleClick += notifyIcon_MouseDoubleClick;
                                    hwndNotifyIconParent = ExplorerHandle;
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
                    Keys modifierKeys = ModifierKeys;
                    switch(num2) {
                        case 1:
                            NavigateCurrentTab(true);
                            return;

                        case 2:
                            NavigateCurrentTab(false);
                            return;

                        case 3:
                            OpenGroup(str, modifierKeys == Keys.Control);
                            return;

                        case 4: {
                                if(copydatastruct.dwData == IntPtr.Zero) {
                                    OpenNewTab(str);
                                    return;
                                }
                                using(IDLWrapper wrapper3 = new IDLWrapper(str)) {
                                    OpenNewWindow(wrapper3);
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
                                using(IDLWrapper wrapper4 = new IDLWrapper(CurrentTab.CurrentIDL)) {
                                    OpenNewWindow(wrapper4);
                                    return;
                                }
                            }
                        case 7:
                            CloneTabButton(CurrentTab, null, true, -1);
                            return;

                        case 8:
                            CurrentTab.TabLocked = !CurrentTab.TabLocked;
                            return;

                        case 10:
                            ToggleTopMost();
                            return;

                        case 11:
                            if(Config.Window.CloseBtnClosesSingleTab) {
                                CloseTab(CurrentTab);
                                return;
                            }
                            CloseTab(CurrentTab, false);
                            if(tabControl1.TabCount == 0) {
                                WindowUtils.CloseExplorer(ExplorerHandle, 2);
                            }
                            return;

                        case 12:
                            if(tabControl1.TabCount > 1) {
                                CloseAllTabsExcept(CurrentTab);
                            }
                            return;

                        case 13:
                            WindowUtils.CloseExplorer(ExplorerHandle, 1);
                            return;

                        case 14:
                            CloseLeftRight(true, -1);
                            return;

                        case 15:
                            CloseLeftRight(false, -1);
                            return;

                        case 0x10:
                            UpOneLevel();
                            return;

                        case 0x11:
                            Explorer.Refresh();
                            return;

                        case 0x12:
                            ShowSearchBar(true);
                            return;

                        case 0x30: {
                                if(modifierKeys != Keys.Control) {
                                    OpenNewTab(str);
                                    return;
                                }
                                using(IDLWrapper wrapper2 = new IDLWrapper(str)) {
                                    OpenNewWindow(wrapper2);
                                    return;
                                }
                            }
                        case 0xf1:
                        case 0xf2: {
                                object[] tag = new object[] { str, num2 == 0xf1, (int)copydatastruct.dwData };
                                if(modifierKeys != Keys.Shift) {
                                    if(modifierKeys == Keys.Control) {
                                        using(IDLWrapper wrapper = new IDLWrapper(str)) {
                                            OpenNewWindow(wrapper);
                                            return;
                                        }
                                    }
                                    NavigateToHistory(tag);
                                    return;
                                }
                                CloneTabButton(CurrentTab, null, true, -1);
                                NavigateToHistory(tag);
                                return;
                            }
                        case 0xf3:
                            NavigateBranches(CurrentTab, (int)copydatastruct.dwData);
                            return;

                        case 250:
                            listView.HideSubDirTip(9);
                            listView.HideThumbnailTooltip(9);
                            return;

                        case 0xfb:
                            if(Directory.Exists(str)) {
                                OpenNewTab(str);
                            }
                            return;

                        case 0xfc:
                            if(!(copydatastruct.dwData == ((IntPtr)1))) {
                                contextMenuSys.Show(MousePosition);
                                return;
                            }
                            contextMenuSys.Show(PointToScreen(Point.Empty));
                            return;

                        case 0xfd:
                            OnMouseDoubleClick(new MouseEventArgs(MouseButtons.Left, 0, 0, 0, 0));
                            return;

                        case 0xff:
                            SyncButtonBarCurrent(0x3f);
                            return;

                        default:
                            return;
                    }
                }
                return;
            Label_0B07:
                if(flag) {
                    bool flag6 = !QTUtility.IsXP && PInvoke.IsIconic(ExplorerHandle);
                    RestoreFromTray();
                    WindowUtils.BringExplorerToFront(ExplorerHandle);
                    if(flag6) {
                        foreach(QTabItem item2 in tabControl1.TabPages) {
                            item2.RefreshRectangle();
                        }
                        tabControl1.Refresh();
                    }
                }
            }
            catch(Exception ex) {
                QTUtility2.MakeErrorLog(ex, String.Format("Message: {0:x4}", m.Msg));
            }
        }

        internal PluginManager PluginServerInstance {
            get {
                return pluginManager;
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
                shellBrowser = (QTPlugin.Interop.IShellBrowser)this.tabBar.ShellBrowser.GetIShellBrowser();
                pluginManager = manager;
                if((QTUtility.Path_PluginLangFile.Length > 0) && File.Exists(QTUtility.Path_PluginLangFile)) {
                    dicLocalizingStrings = QTUtility.ReadLanguageFile(QTUtility.Path_PluginLangFile);
                }
                if(dicLocalizingStrings == null) {
                    dicLocalizingStrings = new Dictionary<string, string[]>();
                }
            }

            public bool AddApplication(string name, ProcessStartInfo startInfo) {
                return false;
            }

            public bool AddGroup(string groupName, string[] paths) {
                if(paths == null || paths.Length == 0) return false;
                GroupsManager.AddGroup(groupName, paths);
                return true;
            }

            internal void ClearEvents() {
                TabChanged = null;
                TabAdded = null;
                TabRemoved = null;
                NavigationComplete = null;
                SelectionChanged = null;
                ExplorerStateChanged = null;
                SettingsChanged = null;
                MouseEnter = null;
                PointedTabChanged = null;
                MouseLeave = null;
                MenuRendererChanged = null;
            }

            public bool CreateTab(Address address, int index, bool fLocked, bool fSelect) {
                using(IDLWrapper wrapper = new IDLWrapper(address)) {
                    address.ITEMIDLIST = wrapper.IDL;
                    address.Path = wrapper.Path;
                }
                if((address.ITEMIDLIST == null) || (address.ITEMIDLIST.Length <= 0)) {
                    return false;
                }
                QTabItem tab = new QTabItem(QTUtility2.MakePathDisplayText(address.Path, false), address.Path, tabBar.tabControl1);
                tab.NavigatedTo(address.Path, address.ITEMIDLIST, -1, false);
                tab.ToolTipText = QTUtility2.MakePathDisplayText(address.Path, true);
                tab.TabLocked = fLocked;
                if(index < 0) {
                    tabBar.AddInsertTab(tab);
                }
                else {
                    if(index > tabBar.tabControl1.TabCount) {
                        index = tabBar.tabControl1.TabCount;
                    }
                    tabBar.tabControl1.TabPages.Insert(index, tab);
                }
                if(fSelect) {
                    tabBar.tabControl1.SelectTab(tab);
                }
                return true;
            }

            public bool CreateWindow(Address address) {
                using(IDLWrapper wrapper = new IDLWrapper(address)) {
                    if(wrapper.Available) {
                        tabBar.OpenNewWindow(wrapper);
                        return true;
                    }
                }
                return false;
            }

            public void Dispose() {
                tabBar = null;
                pluginManager = null;
                shellBrowser = null;
            }

            public bool ExecuteCommand(Commands command, object arg) {
                if(tabBar != null) {
                    IntPtr ptr;
                    switch(command) {
                        case Commands.GoBack:
                        case Commands.GoForward:
                            if(arg is int) {
                                return tabBar.NavigateToIndex(command == Commands.GoBack, (int)arg);
                            }
                            break;

                        case Commands.GoUpOneLevel:
                            tabBar.UpOneLevel();
                            return true;

                        case Commands.RefreshBrowser:
                            tabBar.Explorer.Refresh();
                            return true;

                        case Commands.CloseCurrentTab:
                            return tabBar.CloseTab(tabBar.CurrentTab);

                        case Commands.CloseLeft:
                        case Commands.CloseRight:
                            tabBar.CloseLeftRight(command == Commands.CloseLeft, -1);
                            return true;

                        case Commands.CloseAllButCurrent:
                            tabBar.CloseAllTabsExcept(tabBar.CurrentTab);
                            return true;

                        case Commands.CloseAllButOne: {
                            TabWrapper wrapper = arg as TabWrapper;
                            if(wrapper == null) break;
                            tabBar.CloseAllTabsExcept(wrapper.Tab);
                            return true;
                        }
                        case Commands.CloseWindow:
                            WindowUtils.CloseExplorer(tabBar.ExplorerHandle, 2);
                            return true;

                        case Commands.UndoClose:
                            tabBar.RestoreLastClosed();
                            return true;

                        case Commands.BrowseFolder:
                            tabBar.ChooseNewDirectory();
                            return true;

                        case Commands.ToggleTopMost:
                            tabBar.ToggleTopMost();
                            tabBar.SyncButtonBarCurrent(0x40);
                            return true;

                        case Commands.FocusFileList:
                            tabBar.listView.SetFocus();
                            return true;

                        case Commands.OpenTabBarOptionDialog:
                            OptionsDialog.Open();
                            return true;

                        case Commands.OpenButtonBarOptionDialog:
                            if(!InstanceManager.TryGetButtonBarHandle(tabBar.ExplorerHandle, out ptr)) {
                                break;
                            }
                            QTUtility2.SendCOPYDATASTRUCT(ptr, (IntPtr)12, "showop", IntPtr.Zero);
                            return true;

                        case Commands.IsFolderTreeVisible:
                            return tabBar.ShellBrowser.IsFolderTreeVisible();

                        case Commands.IsButtonBarVisible:
                            return InstanceManager.TryGetButtonBarHandle(tabBar.ExplorerHandle, out ptr);

                        case Commands.ShowFolderTree:
                            if(!QTUtility.IsXP || !(arg is bool)) {
                                break;
                            }
                            tabBar.ShowFolderTree((bool)arg);
                            return true;

                        case Commands.ShowButtonBar:
                            if(!InstanceManager.TryGetButtonBarHandle(tabBar.ExplorerHandle, out ptr)) {
                            }
                            break;

                        case Commands.MD5:
                            if(!(arg is string[])) {
                                break;
                            }
                            if(md5Form == null) {
                                md5Form = new FileHashComputerForm();
                            }
                            if(md5Form.InvokeRequired) {
                                md5Form.Invoke(new FormMethodInvoker(ShowMD5FormCore), new object[] { arg });
                            }
                            else {
                                ShowMD5FormCore(arg);
                            }
                            return true;

                        case Commands.ShowProperties: {
                            if((arg == null) || !(arg is Address)) {
                                break;
                            }
                            Address address = (Address)arg;
                            using(IDLWrapper wrapper = new IDLWrapper(address)) {
                                if(!wrapper.Available) break;
                                ShellMethods.ShowProperties(wrapper.IDL, tabBar.ExplorerHandle);
                                return true;
                            }                               
                        }
                        case Commands.SetModalState:
                            if(((arg == null) || !(arg is bool)) || !((bool)arg)) {
                                tabBar.NowModalDialogShown = false;
                                break;
                            }
                            tabBar.NowModalDialogShown = true;
                            break;

                        case Commands.SetSearchBoxStr:
                            if(((arg == null) || !(arg is string)) || !InstanceManager.TryGetButtonBarHandle(tabBar.ExplorerHandle, out ptr)) {
                                break;
                            }
                            return (IntPtr.Zero == QTUtility2.SendCOPYDATASTRUCT(ptr, (IntPtr)0x10, (string)arg, IntPtr.Zero));

                        case Commands.ReorderTabsByName:
                        case Commands.ReorderTabsByPath:
                        case Commands.ReorderTabsByActv:
                        case Commands.ReorderTabsRevers:
                            if(tabBar.tabControl1.TabCount > 1) {
                                bool fDescending = ((arg != null) && (arg is bool)) && ((bool)arg);
                                tabBar.ReorderTab(((int)command) - 0x18, fDescending);
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
                Group g = GroupsManager.GetGroup(groupName);
                return g == null ? null : g.Paths.ToArray();
            }

            public ToolStripRenderer GetMenuRenderer() {
                return DropDownMenuBase.CurrentRenderer;
            }

            public ITab[] GetTabs() {
                return (from QTabItem item in tabBar.tabControl1.TabPages
                        select new TabWrapper(item, tabBar)).ToArray();
            }

            public ITab HitTest(Point pnt) {
                QTabItem tabMouseOn = (QTabItem)tabBar.tabControl1.GetTabMouseOn();
                return tabMouseOn != null ? new TabWrapper(tabMouseOn, tabBar) : null;
            }

            public void OnExplorerStateChanged(ExplorerWindowActions windowAction) {
                if(ExplorerStateChanged != null) {
                    ExplorerStateChanged(this, new PluginEventArgs(windowAction));
                }
            }

            public void OnMenuRendererChanged() {
                if(MenuRendererChanged != null) {
                    MenuRendererChanged(this, EventArgs.Empty);
                }
            }

            public void OnMouseEnter() {
                if(MouseEnter != null) {
                    MouseEnter(this, EventArgs.Empty);
                }
            }

            public void OnMouseLeave() {
                if(MouseLeave != null) {
                    MouseLeave(this, EventArgs.Empty);
                }
            }

            public void OnNavigationComplete(int index, byte[] idl, string path) {
                if(NavigationComplete != null) {
                    NavigationComplete(this, new PluginEventArgs(index, new Address(idl, path)));
                }
            }

            public void OnPointedTabChanged(int index, byte[] idl, string path) {
                if(PointedTabChanged != null) {
                    PointedTabChanged(this, new PluginEventArgs(index, new Address(idl, path)));
                }
            }

            public void OnSelectionChanged(int index, byte[] idl, string path) {
                if(SelectionChanged != null) {
                    SelectionChanged(this, new PluginEventArgs(index, new Address(idl, path)));
                }
            }

            public void OnSettingsChanged(int iType) {
                if(SettingsChanged != null) {
                    SettingsChanged(this, new PluginEventArgs(iType, new Address()));
                }
            }

            public void OnTabAdded(int index, byte[] idl, string path) {
                if(TabAdded != null) {
                    TabAdded(this, new PluginEventArgs(index, new Address(idl, path)));
                }
            }

            public void OnTabChanged(int index, byte[] idl, string path) {
                if(TabChanged != null) {
                    TabChanged(this, new PluginEventArgs(index, new Address(idl, path)));
                }
            }

            public void OnTabRemoved(int index, byte[] idl, string path) {
                if(TabRemoved != null) {
                    TabRemoved(this, new PluginEventArgs(index, new Address(idl, path)));
                }
            }

            public void OpenGroup(string[] groupNames) {
                foreach(string str in groupNames) {
                    tabBar.OpenGroup(str, false);
                }
            }

            public bool OpenPlugin(IPluginClient pluginClient, out string[] shortcutActions) {
                pluginClient.Open(this, shellBrowser);
                return pluginClient.QueryShortcutKeys(out shortcutActions);
            }

            public void RegisterMenu(IPluginClient pluginClient, MenuType menuType, string menuText, bool fRegister) {
                pluginManager.RegisterMenu(pluginClient, menuType, menuText, fRegister);
            }

            public bool RemoveApplication(string name) {
                return false;
            }

            internal void RemoveEvents(IPluginClient pluginClient) {
                if(TabChanged != null) {
                    foreach(PluginEventHandler handler in TabChanged.GetInvocationList()) {
                        if(handler.Target == pluginClient) {
                            TabChanged = (PluginEventHandler)Delegate.Remove(TabChanged, handler);
                        }
                    }
                }
                if(TabAdded != null) {
                    foreach(PluginEventHandler handler2 in TabAdded.GetInvocationList()) {
                        if(handler2.Target == pluginClient) {
                            TabAdded = (PluginEventHandler)Delegate.Remove(TabAdded, handler2);
                        }
                    }
                }
                if(TabRemoved != null) {
                    foreach(PluginEventHandler handler3 in TabRemoved.GetInvocationList()) {
                        if(handler3.Target == pluginClient) {
                            TabRemoved = (PluginEventHandler)Delegate.Remove(TabRemoved, handler3);
                        }
                    }
                }
                if(NavigationComplete != null) {
                    foreach(PluginEventHandler handler4 in NavigationComplete.GetInvocationList()) {
                        if(handler4.Target == pluginClient) {
                            NavigationComplete = (PluginEventHandler)Delegate.Remove(NavigationComplete, handler4);
                        }
                    }
                }
                if(SelectionChanged != null) {
                    foreach(PluginEventHandler handler5 in SelectionChanged.GetInvocationList()) {
                        if(handler5.Target == pluginClient) {
                            SelectionChanged = (PluginEventHandler)Delegate.Remove(SelectionChanged, handler5);
                        }
                    }
                }
                if(ExplorerStateChanged != null) {
                    foreach(PluginEventHandler handler6 in ExplorerStateChanged.GetInvocationList()) {
                        if(handler6.Target == pluginClient) {
                            ExplorerStateChanged = (PluginEventHandler)Delegate.Remove(ExplorerStateChanged, handler6);
                        }
                    }
                }
                if(SettingsChanged != null) {
                    foreach(PluginEventHandler handler7 in SettingsChanged.GetInvocationList()) {
                        if(handler7.Target == pluginClient) {
                            SettingsChanged = (PluginEventHandler)Delegate.Remove(SettingsChanged, handler7);
                        }
                    }
                }
                if(MouseEnter != null) {
                    foreach(EventHandler handler8 in MouseEnter.GetInvocationList()) {
                        if(handler8.Target == pluginClient) {
                            MouseEnter = (EventHandler)Delegate.Remove(MouseEnter, handler8);
                        }
                    }
                }
                if(PointedTabChanged != null) {
                    foreach(PluginEventHandler handler9 in PointedTabChanged.GetInvocationList()) {
                        if(handler9.Target == pluginClient) {
                            PointedTabChanged = (PluginEventHandler)Delegate.Remove(PointedTabChanged, handler9);
                        }
                    }
                }
                if(MouseLeave != null) {
                    foreach(EventHandler handler10 in MouseLeave.GetInvocationList()) {
                        if(handler10.Target == pluginClient) {
                            MouseLeave = (EventHandler)Delegate.Remove(MouseLeave, handler10);
                        }
                    }
                }
                if(MenuRendererChanged != null) {
                    foreach(EventHandler handler11 in MenuRendererChanged.GetInvocationList()) {
                        if(handler11.Target == pluginClient) {
                            MenuRendererChanged = (EventHandler)Delegate.Remove(MenuRendererChanged, handler11);
                        }
                    }
                }
            }

            public bool RemoveGroup(string groupName) {
                return GroupsManager.RemoveGroup(groupName);
            }

            public bool TryGetLocalizedStrings(IPluginClient pluginClient, int count, out string[] arrStrings) {
                string key = pluginManager.InstanceToFullName(pluginClient, true);
                if(((key.Length > 0) && dicLocalizingStrings.TryGetValue(key, out arrStrings)) && ((arrStrings != null) && (arrStrings.Length == count))) {
                    return true;
                }
                arrStrings = null;
                return false;
            }

            public bool TryGetSelection(out Address[] adSelectedItems) {
                string str;
                return tabBar.ShellBrowser.TryGetSelection(out adSelectedItems, out str, false);
            }

            public bool TrySetSelection(Address[] itemsToSelect, bool fDeselectOthers) {
                return tabBar.ShellBrowser.TrySetSelection(itemsToSelect, null, fDeselectOthers);
            }

            public void UpdateItem(IBarButton barItem, bool fEnabled, bool fRefreshImage) {
                IntPtr ptr;
                string strMsg = pluginManager.InstanceToFullName(barItem, false);
                if((strMsg.Length > 0) && InstanceManager.TryGetButtonBarHandle(tabBar.ExplorerHandle, out ptr)) {
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
                    return tabBar.ExplorerHandle;
                }
            }

            public string[] Groups {
                get {
                    return GroupsManager.Groups.Select(g => g.Name).ToArray();
                }
            }

            public IntPtr Handle {
                get {
                    if(tabBar.IsHandleCreated) {
                        return tabBar.Handle;
                    }
                    return IntPtr.Zero;
                }
            }

            public ITab SelectedTab {
                get {
                    if(tabBar.CurrentTab != null) {
                        return new TabWrapper(tabBar.CurrentTab, tabBar);
                    }
                    return null;
                }
                set {
                    TabWrapper wrapper = value as TabWrapper;
                    if((wrapper.Tab != null) && tabBar.tabControl1.TabPages.Contains(wrapper.Tab)) {
                        tabBar.tabControl1.SelectTab(wrapper.Tab);
                    }
                }
            }

            public bool SelectionChangedAttached {
                get {
                    return (SelectionChanged != null);
                }
            }

            public TabBarOption TabBarOption {
                get {
                    return QTUtility.GetTabBarOption();
                }
                set {
                    QTUtility.SetTabBarOption(value, tabBar);
                }
            }

            internal sealed class TabWrapper : ITab {
                private QTabItem tab;
                private QTTabBarClass tabBar;

                public TabWrapper(QTabItem tab, QTTabBarClass tabBar) {
                    this.tab = tab;
                    this.tabBar = tabBar;
                    this.tab.Closed += tab_Closed;
                }

                public bool Browse(Address address) {
                    if(tab != null) {
                        tabBar.tabControl1.SelectTab(tab);
                        using(IDLWrapper wrapper = new IDLWrapper(address)) {
                            return tabBar.ShellBrowser.Navigate(wrapper) == 0;
                        }
                    }
                    return false;
                }

                public bool Browse(bool fBack) {
                    if(tab != null) {
                        tabBar.tabControl1.SelectTab(tab);
                        return tabBar.NavigateCurrentTab(fBack);
                    }
                    return false;
                }

                public void Clone(int index, bool fSelect) {
                    if(tab != null) {
                        tabBar.CloneTabButton(tab, null, fSelect, index);
                    }
                }

                public bool Close() {
                    return (((tab != null) && (tabBar.tabControl1.TabCount > 1)) && tabBar.CloseTab(tab, true));
                }

                public Address[] GetBraches() {
                    if(tab == null) {
                        return null;
                    }
                    return (from data in tab.Branches
                        where data.IDL != null || !string.IsNullOrEmpty(data.Path)
                        select new Address(data.IDL, data.Path)).ToArray();
                }

                public Address[] GetHistory(bool fBack) {
                    if(tab == null) {
                        return null;
                    }
                    IEnumerable<LogData> logs = tab.GetLogs(fBack);
                    return logs.Select(data => new Address(data.IDL, data.Path)).ToArray();
                }

                public bool Insert(int index) {
                    if(((tab != null) && (-1 < index)) && (index < (tabBar.tabControl1.TabCount + 1))) {
                        int indexSource = tabBar.tabControl1.TabPages.IndexOf(tab);
                        if(indexSource > -1) {
                            tabBar.tabControl1.TabPages.Relocate(indexSource, index);
                            return true;
                        }
                    }
                    return false;
                }

                private void tab_Closed(object sender, EventArgs e) {
                    tab.Closed -= tab_Closed;
                    tab = null;
                    tabBar = null;
                }

                public Address Address {
                    get {
                        if(tab == null) {
                            return new Address();
                        }
                        Address address = new Address(tab.CurrentIDL, tab.CurrentPath);
                        if((address.ITEMIDLIST == null) && !string.IsNullOrEmpty(address.Path)) {
                            IDLWrapper wrapper;
                            IntPtr pidl = PInvoke.ILCreateFromPath(address.Path);
                            if(pidl != IntPtr.Zero) {
                                address = new Address(pidl, tab.CurrentPath);
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
                        if(tab != null) {
                            return tabBar.tabControl1.TabPages.IndexOf(tab);
                        }
                        return -1;
                    }
                }

                public bool Locked {
                    get {
                        return ((tab != null) && tab.TabLocked);
                    }
                    set {
                        if(tab != null) {
                            tab.TabLocked = value;
                            tabBar.tabControl1.Refresh();
                        }
                    }
                }

                public bool Selected {
                    get {
                        return ((tab != null) && (tabBar.CurrentTab == tab));
                    }
                    set {
                        if((tab != null) && value) {
                            tabBar.tabControl1.SelectTab(tab);
                        }
                    }
                }

                public string SubText {
                    get {
                        if(tab != null) {
                            return tab.Comment;
                        }
                        return string.Empty;
                    }
                    set {
                        if((tab != null) && (value != null)) {
                            tab.Comment = value;
                            tab.RefreshRectangle();
                            tabBar.tabControl1.Refresh();
                        }
                    }
                }

                public QTabItem Tab {
                    get {
                        return tab;
                    }
                }

                public string Text {
                    get {
                        return tab != null ? tab.Text : string.Empty;
                    }
                    set {
                        if(tab != null && !string.IsNullOrEmpty(value)) {
                            tab.Text = value;
                        }
                    }
                }
            }
        }
    }
}
