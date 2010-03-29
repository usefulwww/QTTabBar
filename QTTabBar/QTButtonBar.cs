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
    using QTPlugin.Interop;
    using QTTabBarLib.Interop;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.IO;
    using System.Media;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Messaging;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;

    [ComVisible(true), Guid("d2bf470e-ed1c-487f-a666-2bd8835eb6ce")]
    public sealed class QTButtonBar : BandObject {
        private VisualStyleRenderer BackgroundRenderer;
        private static int BarHeight;
        private const int BARHEIGHT_LARGE = 0x22;
        private const int BARHEIGHT_SMALL = 0x1a;
        internal const int BUTTONINDEX_PLUGIN = 0x10000;
        internal static int[] ButtonIndexes;
        private static string[] ButtonItemsDisplayName;
        private IContainer components;
        internal static byte[] ConfigValues;
        private ContextMenuStripEx contextMenu;
        private DropDownMenuReorderable ddmrGroupButton;
        private DropDownMenuReorderable ddmrRecentlyClosed;
        private DropDownMenuReorderable ddmrUserAppButton;
        internal static int[] DefaultButtonIndices;
        private DropTargetWrapper dropTargetWrapper;
        private IntPtr ExplorerHandle;
        private static bool fInitialized;
        private static bool fNoSettings;
        private bool fSearchBoxInputStart;
        private IContextMenu2 iContextMenu2;
        private static ImageStrip imageStrip_Large;
        private static ImageStrip imageStrip_Small;
        private static string ImageStripPath;
        private static string ImageStripPath_CachePath;
        private const int INTERVAL_REARRANGE = 300;
        private const int INTERVAL_SEARCHSTART = 250;
        private int iPluginCreatingIndex;
        private int iSearchResultCount = -1;
        private static bool LargeButton;
        private static bool LockDropDownItems;
        private List<ToolStripItem> lstPluginCustomItem = new List<ToolStripItem>();
        private List<IntPtr> lstPUITEMIDCHILD = new List<IntPtr>();
        private List<QMenuItem> lstTokenedItems = new List<QMenuItem>();
        private ToolStripMenuItem menuCustomize;
        private ToolStripMenuItem menuLockItem;
        private ToolStripMenuItem menuLockToolbar;
        private DropDownMenuBase NavDropDown;
        private PluginManager pluginManager;
        private static Regex reAsterisc = new Regex(@"\\\*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex reQuestion = new Regex(@"\\\?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static string[] ResBBOption;
        private ToolStripSearchBox searchBox;
        private static int SearchBoxWidth;
        private static int[] selectiveLablesIndices;
        private QTTabBarLib.Interop.IShellBrowser shellBrowser;
        private static readonly Size sizeLargeButton = new Size(0x18, 0x18);
        private static readonly Size sizeSmallButton = new Size(0x10, 0x10);
        private string strSearch = string.Empty;
        private Timer timerSearchBox_Rearrange;
        private Timer timerSerachBox_Search;
        private ToolStripEx toolStrip;

        public QTButtonBar() {
            if(!fInitialized) {
                InitializeStaticFields();
            }
            this.InitializeComponent();
        }

        private void ActivatedByClickOnThis() {
            Point point = this.toolStrip.PointToClient(Control.MousePosition);
            ToolStripItem itemAt = this.toolStrip.GetItemAt(point);
            if((itemAt != null) && itemAt.Enabled) {
                if(itemAt is ToolStripSplitButton) {
                    if(((itemAt.Bounds.X + ((ToolStripSplitButton)itemAt).ButtonBounds.Width) + ((ToolStripSplitButton)itemAt).SplitterBounds.Width) < point.X) {
                        ((ToolStripSplitButton)itemAt).ShowDropDown();
                    }
                    else {
                        ((ToolStripSplitButton)itemAt).PerformButtonClick();
                    }
                }
                else if(itemAt is ToolStripDropDownItem) {
                    ((ToolStripDropDownItem)itemAt).ShowDropDown();
                }
                else {
                    itemAt.PerformClick();
                }
            }
        }

        private void AddHistoryItems(ToolStripDropDownItem button) {
            QTTabBarClass tabBar = QTUtility.instanceManager.GetTabBar(this.ExplorerHandle);
            if(tabBar != null) {
                button.DropDownItems.Clear();
                List<QMenuItem> list = tabBar.CreateNavBtnMenuItems(true);
                if(list.Count != 0) {
                    button.DropDownItems.AddRange(list.ToArray());
                    button.DropDownItems.AddRange(tabBar.CreateBranchMenu(true, this.components, new ToolStripItemClickedEventHandler(this.navBranchRoot_DropDownItemClicked)).ToArray());
                }
                else {
                    ToolStripMenuItem item = new ToolStripMenuItem("none");
                    item.Enabled = false;
                    button.DropDownItems.Add(item);
                }
            }
        }

        private void AddUserAppItems() {
            if((QTUtility.UserAppsDic.Count > 0) && (this.ddmrUserAppButton != null)) {
                if(QTUtility.fRequiredRefresh_App) {
                    this.SyncButtonBarBroadCast_ClearApps();
                }
                if(this.ddmrUserAppButton.Items.Count == 0) {
                    this.lstTokenedItems.Clear();
                    List<ToolStripItem> lstItems = MenuUtility.CreateAppLauncherItems(base.Handle, !LockDropDownItems, new ItemRightClickedEventHandler(this.ddmr45_ItemRightClicked), new EventHandler(this.userAppsSubDir_DoubleCliced), false);
                    QTTabBarClass tabBar = QTUtility.instanceManager.GetTabBar(this.ExplorerHandle);
                    if(tabBar != null) {
                        Address[] addressArray;
                        string str2;
                        IntPtr currentPIDL = tabBar.GetCurrentPIDL();
                        string path = ShellMethods.GetPath(currentPIDL);
                        if(currentPIDL != IntPtr.Zero) {
                            QTTabBarLib.Interop.PInvoke.CoTaskMemFree(currentPIDL);
                        }
                        if(tabBar.TryGetSelection(out addressArray, out str2, false)) {
                            this.ReplaceTokens(lstItems, new AppLauncher(addressArray, path), true);
                        }
                    }
                    this.ddmrUserAppButton.AddItemsRange(lstItems.ToArray(), "u");
                }
                else if(this.lstTokenedItems.Count > 0) {
                    foreach(QMenuItem item in this.lstTokenedItems) {
                        item.MenuItemArguments.RestoreOriginalArgs();
                    }
                    QTTabBarClass class3 = QTUtility.instanceManager.GetTabBar(this.ExplorerHandle);
                    if(class3 != null) {
                        Address[] addressArray2;
                        string str4;
                        IntPtr pIDL = class3.GetCurrentPIDL();
                        string pathCurrent = ShellMethods.GetPath(pIDL);
                        if(pIDL != IntPtr.Zero) {
                            QTTabBarLib.Interop.PInvoke.CoTaskMemFree(pIDL);
                        }
                        if(class3.TryGetSelection(out addressArray2, out str4, false)) {
                            ToolStripItem[] itemArray = this.lstTokenedItems.ToArray();
                            this.ReplaceTokens(itemArray, new AppLauncher(addressArray2, pathCurrent), false);
                        }
                    }
                }
            }
        }

        private void AsyncComplete(IAsyncResult ar) {
            AsyncResult result = (AsyncResult)ar;
            ((WaitTimeoutCallback)result.AsyncDelegate).EndInvoke(ar);
            if(base.IsHandleCreated) {
                base.Invoke(new MethodInvoker(this.CallBackSearchBox));
            }
        }

        private void BroadcastConfigChanged(bool fRefreshRequired) {
            foreach(IntPtr ptr in QTUtility.instanceManager.ButtonBarHandles()) {
                if((ptr != base.Handle) && QTTabBarLib.Interop.PInvoke.IsWindow(ptr)) {
                    QTUtility2.SendCOPYDATASTRUCT(ptr, (IntPtr)5, "fromBBBC", fRefreshRequired ? ((IntPtr)1) : IntPtr.Zero);
                }
            }
        }

        private void CallBackSearchBox() {
            base.Explorer.Refresh();
        }

        private static bool CheckDisplayName(QTTabBarLib.Interop.IShellFolder shellFolder, IntPtr pIDLLast, Regex re) {
            if(pIDLLast != IntPtr.Zero) {
                QTTabBarLib.Interop.STRRET strret;
                uint uFlags = 0;
                StringBuilder pszBuf = new StringBuilder(260);
                if(shellFolder.GetDisplayNameOf(pIDLLast, uFlags, out strret) == 0) {
                    QTTabBarLib.Interop.PInvoke.StrRetToBuf(ref strret, pIDLLast, pszBuf, pszBuf.Capacity);
                }
                if(pszBuf.Length > 0) {
                    return re.IsMatch(pszBuf.ToString());
                }
            }
            return false;
        }

        private void ClearToolStripItems() {
            List<ToolStripItem> list = new List<ToolStripItem>();
            foreach(ToolStripItem item in this.toolStrip.Items) {
                if(!this.lstPluginCustomItem.Contains(item)) {
                    list.Add(item);
                }
            }
            this.toolStrip.Items.Clear();
            this.lstPluginCustomItem.Clear();
            foreach(ToolStripItem item2 in list) {
                item2.Dispose();
            }
        }

        private void ClearUserAppsMenu() {
            if(this.ddmrUserAppButton != null) {
                while(this.ddmrUserAppButton.Items.Count > 0) {
                    this.ddmrUserAppButton.Items[0].Dispose();
                }
            }
        }

        public override void CloseDW(uint dwReserved) {
            try {
                if(this.pluginManager != null) {
                    this.pluginManager.Close(true);
                    this.pluginManager = null;
                }
                if(this.iContextMenu2 != null) {
                    Marshal.FinalReleaseComObject(this.iContextMenu2);
                    this.iContextMenu2 = null;
                }
                foreach(IntPtr ptr in this.lstPUITEMIDCHILD) {
                    if(ptr != IntPtr.Zero) {
                        QTTabBarLib.Interop.PInvoke.CoTaskMemFree(ptr);
                    }
                }
                if(this.dropTargetWrapper != null) {
                    this.dropTargetWrapper.Dispose();
                    this.dropTargetWrapper = null;
                }
                QTUtility.instanceManager.RemoveButtonBarHandle(this.ExplorerHandle);
                base.fFinalRelease = false;
                base.CloseDW(dwReserved);
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception, "buttonbar closing");
            }
        }

        private void contextMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            bool fRefreshRequired = false;
            if(e.ClickedItem != this.menuCustomize) {
                if(e.ClickedItem != this.menuLockItem) {
                    WindowUtils.LockToolbar(!this.menuLockToolbar.Checked, this.ExplorerHandle, base.ReBarHandle);
                    return;
                }
                this.menuLockItem.Checked = !this.menuLockItem.Checked;
                LockDropDownItems = this.menuLockItem.Checked;
                bool flag3 = false;
                for(int i = 0; i < this.toolStrip.Items.Count; i++) {
                    ToolStripItem item2 = this.toolStrip.Items[i];
                    if((item2.Tag != null) && ((((int)item2.Tag) == 3) || (((int)item2.Tag) == 5))) {
                        flag3 = true;
                        ((DropDownMenuReorderable)((ToolStripDropDownItem)item2).DropDown).ReorderEnabled = !LockDropDownItems;
                    }
                }
                if(flag3) {
                    SaveSetting();
                }
            }
            else {
                bool flag2 = QTTabBarLib.Interop.PInvoke.Ptr_OP_AND(QTTabBarLib.Interop.PInvoke.GetWindowLongPtr(this.ExplorerHandle, -20), 8) == new IntPtr(8);
                using(ButtonBarOptionForm form = new ButtonBarOptionForm(ButtonIndexes, LargeButton, ImageStripPath, this.pluginManager)) {
                    if(flag2) {
                        form.TopMost = true;
                    }
                    IntPtr tabBarHandle = QTUtility.instanceManager.GetTabBarHandle(this.ExplorerHandle);
                    QTTabBarLib.Interop.PInvoke.SendMessage(tabBarHandle, 0x8001, (IntPtr)1, IntPtr.Zero);
                    DialogResult result = form.ShowDialog();
                    QTTabBarLib.Interop.PInvoke.SendMessage(tabBarHandle, 0x8001, IntPtr.Zero, IntPtr.Zero);
                    if((result != DialogResult.OK) || !form.fChangedExists) {
                        return;
                    }
                    fRefreshRequired = LargeButton != form.fLargeIcon;
                    ButtonIndexes = form.GetButtonIndices();
                    LargeButton = form.fLargeIcon;
                    BarHeight = LargeButton ? 0x22 : 0x1a;
                    ImageStripPath = form.ImageStripPath;
                    switch(form.ItemTextMode) {
                        case 0:
                            ConfigValues[0] = (byte)(ConfigValues[0] | 0x20);
                            ConfigValues[0] = (byte)(ConfigValues[0] & 0xef);
                            break;

                        case 1:
                            ConfigValues[0] = (byte)(ConfigValues[0] | 0x30);
                            break;

                        case 2:
                            ConfigValues[0] = (byte)(ConfigValues[0] & 0xcf);
                            break;
                    }
                    if(form.LockSearchBox) {
                        ConfigValues[0] = (byte)(ConfigValues[0] | 8);
                    }
                    else {
                        ConfigValues[0] = (byte)(ConfigValues[0] & 0xf7);
                    }
                }
                PluginManager.SaveButtonOrder();
                this.CreateItems(true);
                if(flag2) {
                    foreach(ToolStripItem item in this.toolStrip.Items) {
                        if((item.Tag != null) && (((int)item.Tag) == 10)) {
                            ((ToolStripButton)item).Checked = true;
                            break;
                        }
                    }
                }
                SaveSetting();
                if(this.pluginManager != null) {
                    this.pluginManager.OnSettingsChanged(1);
                }
            }
            this.BroadcastConfigChanged(fRefreshRequired);
            this.RefreshEnabledState(fRefreshRequired);
        }

        private void contextMenu_Opening(object sender, CancelEventArgs e) {
            this.menuLockToolbar.Checked = WindowUtils.IsToolbarLocked(base.ReBarHandle);
        }

        private void copyButton_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            QTTabBarClass tabBar = QTUtility.instanceManager.GetTabBar(this.ExplorerHandle);
            if(tabBar != null) {
                tabBar.DoFileTools(((DropDownMenuBase)sender).Items.IndexOf(e.ClickedItem));
            }
        }

        private void copyButton_Opening(object sender, CancelEventArgs e) {
            Address[] addressArray;
            string str;
            this.toolStrip.HideToolTip();
            DropDownMenuBase base2 = (DropDownMenuBase)sender;
            for(int i = 0; i < 5; i++) {
                int num2 = QTUtility.ShortcutKeys[0x1b + i];
                if(num2 > 0x100000) {
                    num2 -= 0x100000;
                    ((ToolStripMenuItem)base2.Items[i]).ShortcutKeyDisplayString = QTUtility2.MakeKeyString((Keys)num2).Replace(" ", string.Empty);
                }
                else {
                    ((ToolStripMenuItem)base2.Items[i]).ShortcutKeyDisplayString = string.Empty;
                }
            }
            QTTabBarClass tabBar = QTUtility.instanceManager.GetTabBar(this.ExplorerHandle);
            if((tabBar != null) && tabBar.TryGetSelection(out addressArray, out str, false)) {
                base2.Items[0].Enabled = base2.Items[1].Enabled = addressArray.Length > 0;
                base2.Items[2].Enabled = base2.Items[3].Enabled = true;
            }
            else {
                base2.Items[0].Enabled = base2.Items[1].Enabled = base2.Items[2].Enabled = base2.Items[3].Enabled = false;
            }
        }

        private ToolStripDropDownButton CreateDropDownButton(int index) {
            ToolStripDropDownButton button = new ToolStripDropDownButton();
            switch(index) {
                case -1:
                    if(this.NavDropDown == null) {
                        this.NavDropDown = new DropDownMenuBase(this.components, true, true, true);
                        this.NavDropDown.ImageList = QTUtility.ImageListGlobal;
                        this.NavDropDown.ItemClicked += new ToolStripItemClickedEventHandler(this.dropDownButtons_DropDown_ItemClicked);
                        this.NavDropDown.Closed += new ToolStripDropDownClosedEventHandler(this.dropDownButtons_DropDown_Closed);
                    }
                    button.DropDown = this.NavDropDown;
                    button.Tag = -1;
                    button.AutoToolTip = false;
                    break;

                case 3:
                    if(this.ddmrGroupButton == null) {
                        this.ddmrGroupButton = new DropDownMenuReorderable(this.components, true, false);
                        this.ddmrGroupButton.ImageList = QTUtility.ImageListGlobal;
                        this.ddmrGroupButton.ReorderEnabled = !LockDropDownItems;
                        this.ddmrGroupButton.ItemRightClicked += new ItemRightClickedEventHandler(MenuUtility.GroupMenu_ItemRightClicked);
                        this.ddmrGroupButton.ItemMiddleClicked += new ItemRightClickedEventHandler(this.ddmrGroupButton_ItemMiddleClicked);
                        this.ddmrGroupButton.ReorderFinished += new MenuReorderedEventHandler(this.dropDownButtons_DropDown_ReorderFinished);
                        this.ddmrGroupButton.ItemClicked += new ToolStripItemClickedEventHandler(this.dropDownButtons_DropDown_ItemClicked);
                        this.ddmrGroupButton.Closed += new ToolStripDropDownClosedEventHandler(this.dropDownButtons_DropDown_Closed);
                    }
                    button.DropDown = this.ddmrGroupButton;
                    button.Enabled = QTUtility.GroupPathsDic.Count > 0;
                    break;

                case 4:
                    if(this.ddmrRecentlyClosed == null) {
                        this.ddmrRecentlyClosed = new DropDownMenuReorderable(this.components, true, false);
                        this.ddmrRecentlyClosed.ImageList = QTUtility.ImageListGlobal;
                        this.ddmrRecentlyClosed.ReorderEnabled = false;
                        this.ddmrRecentlyClosed.MessageParent = base.Handle;
                        this.ddmrRecentlyClosed.ItemRightClicked += new ItemRightClickedEventHandler(this.ddmr45_ItemRightClicked);
                        this.ddmrRecentlyClosed.ItemClicked += new ToolStripItemClickedEventHandler(this.dropDownButtons_DropDown_ItemClicked);
                        this.ddmrRecentlyClosed.Closed += new ToolStripDropDownClosedEventHandler(this.dropDownButtons_DropDown_Closed);
                    }
                    button.DropDown = this.ddmrRecentlyClosed;
                    button.Enabled = QTUtility.ClosedTabHistoryList.Count > 0;
                    break;

                case 5:
                    if(this.ddmrUserAppButton == null) {
                        this.ddmrUserAppButton = new DropDownMenuReorderable(this.components);
                        this.ddmrUserAppButton.ImageList = QTUtility.ImageListGlobal;
                        this.ddmrUserAppButton.ReorderEnabled = !LockDropDownItems;
                        this.ddmrUserAppButton.MessageParent = base.Handle;
                        this.ddmrUserAppButton.ItemRightClicked += new ItemRightClickedEventHandler(this.ddmr45_ItemRightClicked);
                        this.ddmrUserAppButton.ReorderFinished += new MenuReorderedEventHandler(this.dropDownButtons_DropDown_ReorderFinished);
                        this.ddmrUserAppButton.ItemClicked += new ToolStripItemClickedEventHandler(this.dropDownButtons_DropDown_ItemClicked);
                        this.ddmrUserAppButton.Closed += new ToolStripDropDownClosedEventHandler(this.dropDownButtons_DropDown_Closed);
                    }
                    button.DropDown = this.ddmrUserAppButton;
                    button.Enabled = QTUtility.UserAppsDic.Count > 0;
                    break;
            }
            button.DropDownOpening += new EventHandler(this.dropDownButtons_DropDownOpening);
            return button;
        }

        private void CreateItems(bool fRefresh) {
            ManageImageList(fRefresh);
            this.toolStrip.SuspendLayout();
            if(this.iSearchResultCount != -1) {
                base.Explorer.Refresh();
            }
            this.RefreshSearchBox(false);
            if(this.searchBox != null) {
                this.searchBox.Dispose();
                this.timerSerachBox_Search.Dispose();
                this.timerSearchBox_Rearrange.Dispose();
                this.searchBox = null;
                this.timerSerachBox_Search = null;
                this.timerSearchBox_Rearrange = null;
            }
            this.ClearToolStripItems();
            this.toolStrip.ShowItemToolTips = QTUtility.CheckConfig(0, 8);
            base.Height = LargeButton ? 0x22 : 0x1a;
            bool flag = (ConfigValues[0] & 0x20) == 0x20;
            bool flag2 = (ConfigValues[0] & 0x10) == 0x10;
            this.UnloadPluginsOnCreation();
            foreach(int num in ButtonIndexes) {
                ToolStripItem item = null;
                switch(num) {
                    case 0: {
                            ToolStripSeparator separator = new ToolStripSeparator();
                            separator.Tag = 0;
                            this.toolStrip.Items.Add(separator);
                            goto Label_050D;
                        }
                    case 3:
                    case 4:
                    case 5:
                        item = this.CreateDropDownButton(num);
                        break;

                    case 9: {
                            item = new ToolStripDropDownButton();
                            string[] strArray = QTUtility.TextResourcesDic["ButtonBar_Misc"];
                            DropDownMenuBase base2 = new DropDownMenuBase(this.components);
                            base2.ShowCheckMargin = QTUtility.CheckConfig(13, 0x20) && !QTUtility.CheckConfig(13, 0x10);
                            base2.ShowImageMargin = false;
                            base2.Items.AddRange(new ToolStripItem[] { new ToolStripMenuItem(strArray[0]), new ToolStripMenuItem(strArray[1]), new ToolStripMenuItem(strArray[2]), new ToolStripMenuItem(strArray[3]), new ToolStripMenuItem(strArray[4]), new ToolStripMenuItem(strArray[6]) });
                            base2.ItemClicked += new ToolStripItemClickedEventHandler(this.copyButton_DropDownItemClicked);
                            base2.Opening += new CancelEventHandler(this.copyButton_Opening);
                            ((ToolStripDropDownButton)item).DropDown = base2;
                            break;
                        }
                    case 10:
                        item = new ToolStripButton();
                        ((ToolStripButton)item).CheckOnClick = true;
                        break;

                    case 0x13: {
                            ToolStripTrackBar bar = new ToolStripTrackBar();
                            bar.Tag = num;
                            bar.ToolTipText = ButtonItemsDisplayName[0x13];
                            bar.ValueChanged += new EventHandler(this.trackBar_ValueChanged);
                            this.toolStrip.Items.Add(bar);
                            goto Label_050D;
                        }
                    case 20:
                        this.searchBox = new ToolStripSearchBox(LargeButton, (ConfigValues[0] & 8) != 0, ButtonItemsDisplayName[0x12], SearchBoxWidth);
                        this.searchBox.ToolTipText = ButtonItemsDisplayName[20];
                        this.searchBox.Tag = num;
                        this.searchBox.ErasingText += new CancelEventHandler(this.searchBox_ErasingText);
                        this.searchBox.ResizeComplete += new EventHandler(this.searchBox_ResizeComplete);
                        this.searchBox.TextChanged += new EventHandler(this.searchBox_TextChanged);
                        this.searchBox.KeyPress += new KeyPressEventHandler(this.searchBox_KeyPress);
                        this.searchBox.GotFocus += new EventHandler(this.searchBox_GotFocus);
                        this.toolStrip.Items.Add(this.searchBox);
                        this.timerSerachBox_Search = new Timer(this.components);
                        this.timerSerachBox_Search.Interval = 250;
                        this.timerSerachBox_Search.Tick += new EventHandler(this.timerSerachBox_Search_Tick);
                        this.timerSearchBox_Rearrange = new Timer(this.components);
                        this.timerSearchBox_Rearrange.Interval = 300;
                        this.timerSearchBox_Rearrange.Tick += new EventHandler(this.timerSearchBox_Rearrange_Tick);
                        goto Label_050D;

                    case 0x10000:
                        this.CreatePluginItem();
                        goto Label_050D;

                    default:
                        if(num >= 0x15) {
                            goto Label_050D;
                        }
                        item = new ToolStripButton();
                        break;
                }
                if(flag) {
                    if(flag2) {
                        if(Array.IndexOf<int>(selectiveLablesIndices, num) != -1) {
                            item.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
                        }
                        else {
                            item.DisplayStyle = ToolStripItemDisplayStyle.Image;
                        }
                    }
                    else {
                        item.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
                    }
                }
                else {
                    item.DisplayStyle = ToolStripItemDisplayStyle.Image;
                }
                item.ImageScaling = ToolStripItemImageScaling.None;
                item.Text = item.ToolTipText = ButtonItemsDisplayName[num];
                item.Image = (LargeButton ? imageStrip_Large[num - 1] : imageStrip_Small[num - 1]).Clone(new Rectangle(Point.Empty, LargeButton ? sizeLargeButton : sizeSmallButton), PixelFormat.Format32bppArgb);
                item.Tag = num;
                this.toolStrip.Items.Add(item);
                if(((num == 1) && (Array.IndexOf<int>(ButtonIndexes, 2) == -1)) || (num == 2)) {
                    this.toolStrip.Items.Add(this.CreateDropDownButton(-1));
                }
            Label_050D: ;
            }
            if(ButtonIndexes.Length == 0) {
                ToolStripSeparator separator2 = new ToolStripSeparator();
                separator2.Tag = 0;
                this.toolStrip.Items.Add(separator2);
            }
            this.toolStrip.ResumeLayout();
            this.toolStrip.RaiseOnResize();
            this.iPluginCreatingIndex = 0;
            if(this.pluginManager != null) {
                this.pluginManager.ClearBackgroundMultipleOrders();
            }
        }

        private void CreatePluginItem() {
            if((this.pluginManager != null) && (PluginManager.ActivatedButtonsOrder.Count > this.iPluginCreatingIndex)) {
                string pluginID = string.Empty;
                try {
                    int num = 0x10000 + this.iPluginCreatingIndex;
                    pluginID = PluginManager.ActivatedButtonsOrder[this.iPluginCreatingIndex];
                    bool flag = (ConfigValues[0] & 0x20) == 0x20;
                    bool flag2 = (ConfigValues[0] & 0x10) == 0x10;
                    PluginInformation pi = null;
                    foreach(PluginInformation information2 in PluginManager.PluginInformations) {
                        if(information2.PluginID == pluginID) {
                            pi = information2;
                            break;
                        }
                    }
                    if(pi != null) {
                        Plugin plugin = null;
                        this.pluginManager.TryGetPlugin(pluginID, out plugin);
                        if(plugin == null) {
                            plugin = this.pluginManager.Load(pi, null);
                        }
                        if(plugin != null) {
                            bool flag3 = false;
                            IBarDropButton instance = plugin.Instance as IBarDropButton;
                            if(instance != null) {
                                instance.InitializeItem();
                                if(instance.IsSplitButton) {
                                    ToolStripSplitButton button2 = new ToolStripSplitButton(instance.Text);
                                    button2.ImageScaling = ToolStripItemImageScaling.None;
                                    button2.DropDownButtonWidth = LargeButton ? 14 : 11;
                                    if(flag2) {
                                        button2.DisplayStyle = instance.ShowTextLabel ? ToolStripItemDisplayStyle.ImageAndText : ToolStripItemDisplayStyle.Image;
                                    }
                                    else {
                                        button2.DisplayStyle = flag ? ToolStripItemDisplayStyle.ImageAndText : ToolStripItemDisplayStyle.Image;
                                    }
                                    button2.ToolTipText = instance.Text;
                                    button2.Image = instance.GetImage(LargeButton);
                                    button2.Tag = num;
                                    DropDownMenuReorderable reorderable = new DropDownMenuReorderable(this.components);
                                    button2.DropDown = reorderable;
                                    button2.DropDownOpening += new EventHandler(this.pluginDropDown_DropDownOpening);
                                    button2.ButtonClick += new EventHandler(this.pluginButton_ButtonClick);
                                    reorderable.ItemClicked += new ToolStripItemClickedEventHandler(this.pluginDropDown_ItemClicked);
                                    reorderable.ItemRightClicked += new ItemRightClickedEventHandler(this.pluginDropDown_ItemRightClicked);
                                    this.toolStrip.Items.Add(button2);
                                }
                                else {
                                    ToolStripDropDownButton button3 = new ToolStripDropDownButton(instance.Text);
                                    button3.ImageScaling = ToolStripItemImageScaling.None;
                                    if(flag2) {
                                        button3.DisplayStyle = instance.ShowTextLabel ? ToolStripItemDisplayStyle.ImageAndText : ToolStripItemDisplayStyle.Image;
                                    }
                                    else {
                                        button3.DisplayStyle = flag ? ToolStripItemDisplayStyle.ImageAndText : ToolStripItemDisplayStyle.Image;
                                    }
                                    button3.ToolTipText = instance.Text;
                                    button3.Image = instance.GetImage(LargeButton);
                                    button3.Tag = num;
                                    DropDownMenuReorderable reorderable2 = new DropDownMenuReorderable(this.components);
                                    button3.DropDown = reorderable2;
                                    button3.DropDownOpening += new EventHandler(this.pluginDropDown_DropDownOpening);
                                    reorderable2.ItemClicked += new ToolStripItemClickedEventHandler(this.pluginDropDown_ItemClicked);
                                    reorderable2.ItemRightClicked += new ItemRightClickedEventHandler(this.pluginDropDown_ItemRightClicked);
                                    this.toolStrip.Items.Add(button3);
                                }
                                flag3 = true;
                            }
                            else {
                                IBarButton button4 = plugin.Instance as IBarButton;
                                if(button4 != null) {
                                    button4.InitializeItem();
                                    ToolStripButton button5 = new ToolStripButton(button4.Text);
                                    button5.ImageScaling = ToolStripItemImageScaling.None;
                                    if(flag2) {
                                        button5.DisplayStyle = button4.ShowTextLabel ? ToolStripItemDisplayStyle.ImageAndText : ToolStripItemDisplayStyle.Image;
                                    }
                                    else {
                                        button5.DisplayStyle = flag ? ToolStripItemDisplayStyle.ImageAndText : ToolStripItemDisplayStyle.Image;
                                    }
                                    button5.ToolTipText = button4.Text;
                                    button5.Image = button4.GetImage(LargeButton);
                                    button5.Tag = num;
                                    button5.Click += new EventHandler(this.pluginButton_ButtonClick);
                                    this.toolStrip.Items.Add(button5);
                                    flag3 = true;
                                }
                                else {
                                    IBarCustomItem item = plugin.Instance as IBarCustomItem;
                                    if(item != null) {
                                        DisplayStyle displayStyle = flag2 ? DisplayStyle.SelectiveText : (flag ? DisplayStyle.ShowTextLabel : DisplayStyle.NoLabel);
                                        ToolStripItem item2 = item.CreateItem(LargeButton, displayStyle);
                                        if(item2 != null) {
                                            item2.ImageScaling = ToolStripItemImageScaling.None;
                                            item2.Tag = num;
                                            this.toolStrip.Items.Add(item2);
                                            flag3 = true;
                                            this.lstPluginCustomItem.Add(item2);
                                        }
                                    }
                                    else {
                                        IBarMultipleCustomItems items = plugin.Instance as IBarMultipleCustomItems;
                                        if(items != null) {
                                            DisplayStyle style2 = flag2 ? DisplayStyle.SelectiveText : (flag ? DisplayStyle.ShowTextLabel : DisplayStyle.NoLabel);
                                            int index = this.pluginManager.IncrementBackgroundMultiple(pi);
                                            if(index == 0) {
                                                List<int> list;
                                                int[] order = null;
                                                if(this.pluginManager.TryGetMultipleOrder(pi.PluginID, out list)) {
                                                    order = list.ToArray();
                                                }
                                                items.Initialize(order);
                                            }
                                            ToolStripItem item3 = items.CreateItem(LargeButton, style2, index);
                                            if(item3 != null) {
                                                item3.Tag = num;
                                                this.toolStrip.Items.Add(item3);
                                                flag3 = true;
                                                this.lstPluginCustomItem.Add(item3);
                                            }
                                        }
                                    }
                                }
                            }
                            if(flag3 && ((pi.PluginType == PluginType.Background) || (pi.PluginType == PluginType.BackgroundMultiple))) {
                                plugin.BackgroundButtonEnabled = true;
                            }
                        }
                    }
                }
                catch(Exception exception) {
                    PluginManager.HandlePluginException(exception, this.ExplorerHandle, pluginID, "Loading plugin button.");
                }
                finally {
                    this.iPluginCreatingIndex++;
                }
            }
        }

        private void ddmr45_ItemRightClicked(object sender, ItemRightClickedEventArgs e) {
            QMenuItem clickedItem = e.ClickedItem as QMenuItem;
            if(clickedItem != null) {
                bool fCanRemove = sender == this.ddmrRecentlyClosed;
                using(IDLWrapper wrapper = new IDLWrapper(clickedItem.Path)) {
                    e.HRESULT = ShellMethods.PopUpSystemContextMenu(wrapper, e.IsKey ? e.Point : Control.MousePosition, ref this.iContextMenu2, ((DropDownMenuReorderable)sender).Handle, fCanRemove);
                }
                if(fCanRemove && (e.HRESULT == 0xffff)) {
                    QTUtility.ClosedTabHistoryList.Remove(clickedItem.Path);
                    clickedItem.Dispose();
                }
            }
        }

        private void ddmrGroupButton_ItemMiddleClicked(object sender, ItemRightClickedEventArgs e) {
            IntPtr tabBarHandle = QTUtility.instanceManager.GetTabBarHandle(this.ExplorerHandle);
            if(tabBarHandle != IntPtr.Zero) {
                QTUtility2.SendCOPYDATASTRUCT(tabBarHandle, (IntPtr)80, e.ClickedItem.Text, IntPtr.Zero);
            }
        }

        protected override void Dispose(bool disposing) {
            if(disposing && (this.components != null)) {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool DoItems(int index) {
            foreach(ToolStripItem item in this.toolStrip.Items) {
                if((item.Tag == null) || (((int)item.Tag) != index)) {
                    continue;
                }
                if(item is ToolStripDropDownItem) {
                    ((ToolStripDropDownItem)item).ShowDropDown();
                }
                else {
                    item.PerformClick();
                }
                return true;
            }
            return false;
        }

        private void dropDownButtons_DropDown_Closed(object sender, ToolStripDropDownClosedEventArgs e) {
            DropDownMenuBase.ExitMenuMode();
        }

        private void dropDownButtons_DropDown_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            ToolStripItem ownerItem = ((ToolStripDropDown)sender).OwnerItem;
            if((ownerItem != null) && (ownerItem.Tag != null)) {
                QMenuItem clickedItem = e.ClickedItem as QMenuItem;
                int tag = (int)ownerItem.Tag;
                IntPtr tabBarHandle = QTUtility.instanceManager.GetTabBarHandle(this.ExplorerHandle);
                IntPtr wParam = (IntPtr)(0xf00 | tag);
                switch(tag) {
                    case -1:
                        if(clickedItem != null) {
                            MenuItemArguments menuItemArguments = clickedItem.MenuItemArguments;
                            wParam = menuItemArguments.IsBack ? ((IntPtr)0xff1) : ((IntPtr)0xff2);
                            QTUtility2.SendCOPYDATASTRUCT(tabBarHandle, wParam, menuItemArguments.Path, (IntPtr)menuItemArguments.Index);
                        }
                        return;

                    case 0:
                    case 1:
                    case 2:
                        return;

                    case 3:
                        if(clickedItem != null) {
                            this.ddmrGroupButton.Close();
                            if(Control.ModifierKeys != (Keys.Control | Keys.Shift)) {
                                QTUtility2.SendCOPYDATASTRUCT(tabBarHandle, wParam, e.ClickedItem.Text, IntPtr.Zero);
                                return;
                            }
                            if(!QTUtility.StartUpGroupList.Contains(e.ClickedItem.Text)) {
                                QTUtility.StartUpGroupList.Add(e.ClickedItem.Text);
                                return;
                            }
                            QTUtility.StartUpGroupList.Remove(e.ClickedItem.Text);
                        }
                        return;

                    case 4:
                        if(clickedItem != null) {
                            IntPtr dwData = (Control.ModifierKeys != Keys.Control) ? IntPtr.Zero : ((IntPtr)1);
                            QTUtility2.SendCOPYDATASTRUCT(tabBarHandle, wParam, clickedItem.Path, dwData);
                        }
                        return;

                    case 5:
                        if((clickedItem != null) && (clickedItem.Target == MenuTarget.File)) {
                            AppLauncher.Execute(clickedItem.MenuItemArguments, this.ExplorerHandle);
                        }
                        return;
                }
            }
        }

        private void dropDownButtons_DropDown_ReorderFinished(object sender, ToolStripItemClickedEventArgs e) {
            DropDownMenuReorderable reorderable = (DropDownMenuReorderable)sender;
            switch(((int)reorderable.OwnerItem.Tag)) {
                case 3:
                    QTUtility.RefreshGroupMenuesOnReorderFinished(reorderable.Items);
                    break;

                case 5:
                    QTUtility.RefreshUserappMenuesOnReorderFinished(reorderable.Items);
                    QTUtility.fRequiredRefresh_App = true;
                    break;
            }
            QTTabBarClass.SyncTaskBarMenu();
        }

        private void dropDownButtons_DropDownOpening(object sender, EventArgs e) {
            this.toolStrip.HideToolTip();
            ToolStripDropDownItem button = (ToolStripDropDownItem)sender;
            button.DropDown.SuspendLayout();
            switch(((int)button.Tag)) {
                case -1:
                    this.AddHistoryItems(button);
                    break;

                case 3:
                    MenuUtility.CreateGroupItems(button);
                    break;

                case 4:
                    MenuUtility.CreateUndoClosedItems(button);
                    break;

                case 5:
                    this.AddUserAppItems();
                    break;
            }
            button.DropDown.ResumeLayout();
        }

        private void EnableItemAt(int buttonIndex, bool fEnable) {
            foreach(ToolStripItem item in this.toolStrip.Items) {
                if((item.Tag != null) && (((int)item.Tag) == buttonIndex)) {
                    item.Enabled = fEnable;
                    break;
                }
            }
        }

        public override void GetBandInfo(uint dwBandID, uint dwViewMode, ref DESKBANDINFO dbi) {
            if((dbi.dwMask & DBIM.ACTUAL) != ((DBIM)0)) {
                dbi.ptActual.X = base.Size.Width;
                dbi.ptActual.Y = BarHeight;
            }
            if((dbi.dwMask & DBIM.INTEGRAL) != ((DBIM)0)) {
                dbi.ptIntegral.X = -1;
                dbi.ptIntegral.Y = 10;
            }
            if((dbi.dwMask & DBIM.MAXSIZE) != ((DBIM)0)) {
                dbi.ptMaxSize.X = -1;
                dbi.ptMaxSize.Y = BarHeight;
            }
            if((dbi.dwMask & DBIM.MINSIZE) != ((DBIM)0)) {
                dbi.ptMinSize.X = base.MinSize.Width;
                dbi.ptMinSize.Y = BarHeight;
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

        private void InitializeComponent() {
            this.components = new Container();
            this.toolStrip = new ToolStripEx();
            this.contextMenu = new ContextMenuStripEx(this.components, true);
            this.menuCustomize = new ToolStripMenuItem(ResBBOption[9]);
            this.menuLockItem = new ToolStripMenuItem(ResBBOption[10]);
            this.menuLockToolbar = new ToolStripMenuItem(QTUtility.ResMain[0x20]);
            this.toolStrip.SuspendLayout();
            this.contextMenu.SuspendLayout();
            base.SuspendLayout();
            this.toolStrip.Dock = DockStyle.Fill;
            this.toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            this.toolStrip.ImeMode = ImeMode.Disable;
            this.toolStrip.Renderer = new ToolbarRenderer();
            this.toolStrip.BackColor = System.Drawing.Color.Transparent;
            this.toolStrip.ItemClicked += new ToolStripItemClickedEventHandler(this.toolStrip_ItemClicked);
            this.toolStrip.GotFocus += new EventHandler(this.toolStrip_GotFocus);
            this.toolStrip.MouseDoubleClick += new MouseEventHandler(this.toolStrip_MouseDoubleClick);
            this.toolStrip.MouseActivated += new EventHandler(this.toolStrip_MouseActivated);
            this.toolStrip.PreviewKeyDown += new PreviewKeyDownEventHandler(this.toolStrip_PreviewKeyDown);
            this.menuLockItem.Checked = LockDropDownItems;
            this.contextMenu.Items.Add(this.menuCustomize);
            this.contextMenu.Items.Add(this.menuLockItem);
            this.contextMenu.Items.Add(this.menuLockToolbar);
            this.contextMenu.ShowImageMargin = false;
            this.contextMenu.Opening += new CancelEventHandler(this.contextMenu_Opening);
            this.contextMenu.ItemClicked += new ToolStripItemClickedEventHandler(this.contextMenu_ItemClicked);
            base.Controls.Add(this.toolStrip);
            base.Height = BarHeight;
            base.MinSize = new Size(20, BarHeight);
            this.ContextMenuStrip = this.contextMenu;
            this.toolStrip.ResumeLayout(false);
            this.contextMenu.ResumeLayout(false);
            base.ResumeLayout();
        }

        private static void InitializeStaticFields() {
            fInitialized = true;
            if(QTUtility.IsVista) {
                DefaultButtonIndices = new int[] { 3, 4, 5, 0, 6, 7, 0, 11, 13, 12, 14, 15, 0, 9, 20 };
            }
            else {
                DefaultButtonIndices = new int[] { 
          1, 2, 0, 3, 4, 5, 0, 6, 7, 0, 11, 13, 12, 14, 15, 0, 
          9, 20
         };
            }
            selectiveLablesIndices = new int[] { 1, 3, 6, 7, 9, 0x12 };
            RefreshTexts();
            imageStrip_Large = new ImageStrip(new Size(0x18, 0x18));
            imageStrip_Small = new ImageStrip(new Size(0x10, 0x10));
            ReadSetting();
        }

        private static void LoadDefaultImages(bool fWriteReg) {
            ImageStripPath_CachePath = ImageStripPath = string.Empty;
            imageStrip_Large.TransparentColor = imageStrip_Small.TransparentColor = System.Drawing.Color.Empty;
            Bitmap bmp = Resources_Image.ButtonStrip24;
            Bitmap bitmap2 = Resources_Image.ButtonStrip16;
            imageStrip_Large.AddStrip(bmp);
            imageStrip_Small.AddStrip(bitmap2);
            bmp.Dispose();
            bitmap2.Dispose();
            if(fWriteReg) {
                using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar")) {
                    key.SetValue("Buttons_ImagePath", string.Empty);
                }
            }
        }

        private static bool LoadExternalImage(string path) {
            Bitmap bitmap;
            Bitmap bitmap2;
            if(LoadExternalImage(path, out bitmap, out bitmap2)) {
                imageStrip_Large.AddStrip(bitmap);
                imageStrip_Small.AddStrip(bitmap2);
                bitmap.Dispose();
                bitmap2.Dispose();
                ImageStripPath_CachePath = path;
                if(string.Equals(Path.GetExtension(path), ".bmp", StringComparison.OrdinalIgnoreCase)) {
                    imageStrip_Large.TransparentColor = imageStrip_Small.TransparentColor = System.Drawing.Color.Magenta;
                }
                else {
                    imageStrip_Large.TransparentColor = imageStrip_Small.TransparentColor = System.Drawing.Color.Empty;
                }
                return true;
            }
            ImageStripPath_CachePath = string.Empty;
            return false;
        }

        internal static bool LoadExternalImage(string path, out Bitmap bmpLarge, out Bitmap bmpSmall) {
            bmpLarge = (Bitmap)(bmpSmall = null);
            if(File.Exists(path)) {
                try {
                    using(Bitmap bitmap = new Bitmap(path)) {
                        if((bitmap.Width >= 0x1b0) && (bitmap.Height >= 40)) {
                            bmpLarge = bitmap.Clone(new Rectangle(0, 0, 0x1b0, 0x18), PixelFormat.Format32bppArgb);
                            bmpSmall = bitmap.Clone(new Rectangle(0, 0x18, 0x120, 0x10), PixelFormat.Format32bppArgb);
                            return true;
                        }
                    }
                }
                catch {
                }
            }
            return false;
        }

        private static void ManageImageList(bool fRefresh) {
            if(ImageStripPath == null) {
                LoadDefaultImages(false);
            }
            else if((fRefresh || !string.Equals(ImageStripPath, ImageStripPath_CachePath, StringComparison.OrdinalIgnoreCase)) && ((ImageStripPath.Length == 0) || !LoadExternalImage(ImageStripPath))) {
                LoadDefaultImages(true);
            }
        }

        private void navBranchRoot_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            QTUtility2.SendCOPYDATASTRUCT(QTUtility.instanceManager.GetTabBarHandle(this.ExplorerHandle), (IntPtr)0xff3, "Btn_Branch", (IntPtr)((QMenuItem)e.ClickedItem).MenuItemArguments.Index);
        }

        protected override void OnExplorerAttached() {
            this.ExplorerHandle = (IntPtr)base.Explorer.HWND;
            QTUtility.instanceManager.AddButtonBarHandle(this.ExplorerHandle, base.Handle);
            this.dropTargetWrapper = new DropTargetWrapper(this);
            if(fNoSettings) {
                if(this.pluginManager == null) {
                    QTTabBarClass tabBar = QTUtility.instanceManager.GetTabBar(this.ExplorerHandle);
                    if(tabBar != null) {
                        this.pluginManager = tabBar.PluginServerInstance;
                        if(this.pluginManager != null) {
                            this.pluginManager.AddRef();
                        }
                    }
                    this.CreateItems(false);
                }
                fNoSettings = false;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e) {
            if(VisualStyleRenderer.IsSupported) {
                if(this.BackgroundRenderer == null) {
                    this.BackgroundRenderer = new VisualStyleRenderer(VisualStyleElement.Rebar.Band.Normal);
                }
                this.BackgroundRenderer.DrawParentBackground(e.Graphics, e.ClipRectangle, this);
            }
            else {
                if(base.ReBarHandle != IntPtr.Zero) {
                    int colorref = (int)QTTabBarLib.Interop.PInvoke.SendMessage(base.ReBarHandle, 0x414, IntPtr.Zero, IntPtr.Zero);
                    using(SolidBrush brush = new SolidBrush(QTUtility2.MakeColor(colorref))) {
                        e.Graphics.FillRectangle(brush, e.ClipRectangle);
                        return;
                    }
                }
                base.OnPaintBackground(e);
            }
        }

        private void pluginButton_ButtonClick(object sender, EventArgs e) {
            ToolStripItem item = (ToolStripItem)sender;
            int num = ((int)item.Tag) - 0x10000;
            if(PluginManager.ActivatedButtonsOrder.Count > num) {
                Plugin plugin;
                string pluginID = PluginManager.ActivatedButtonsOrder[num];
                if(this.pluginManager.TryGetPlugin(pluginID, out plugin)) {
                    try {
                        ((IBarButton)plugin.Instance).OnButtonClick();
                    }
                    catch(Exception exception) {
                        PluginManager.HandlePluginException(exception, this.ExplorerHandle, plugin.PluginInformation.Name, "On button clicked.");
                    }
                }
            }
        }

        private void pluginDropDown_DropDownOpening(object sender, EventArgs e) {
            this.toolStrip.HideToolTip();
            ToolStripDropDownItem item = (ToolStripDropDownItem)sender;
            item.DropDown.SuspendLayout();
            int num = ((int)item.Tag) - 0x10000;
            if(PluginManager.ActivatedButtonsOrder.Count > num) {
                Plugin plugin;
                string pluginID = PluginManager.ActivatedButtonsOrder[num];
                if(this.pluginManager.TryGetPlugin(pluginID, out plugin)) {
                    try {
                        ((IBarDropButton)plugin.Instance).OnDropDownOpening((ToolStripDropDownMenu)item.DropDown);
                    }
                    catch(Exception exception) {
                        PluginManager.HandlePluginException(exception, this.ExplorerHandle, plugin.PluginInformation.Name, "On dropdwon menu is showing.");
                    }
                }
            }
            item.DropDown.ResumeLayout();
        }

        private void pluginDropDown_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            ToolStripDropDownItem ownerItem = (ToolStripDropDownItem)((DropDownMenuReorderable)sender).OwnerItem;
            int num = ((int)ownerItem.Tag) - 0x10000;
            if((PluginManager.ActivatedButtonsOrder.Count > num) && (num > -1)) {
                Plugin plugin;
                string pluginID = PluginManager.ActivatedButtonsOrder[num];
                if(this.pluginManager.TryGetPlugin(pluginID, out plugin)) {
                    try {
                        ((IBarDropButton)plugin.Instance).OnDropDownItemClick(e.ClickedItem, MouseButtons.Left);
                    }
                    catch(Exception exception) {
                        PluginManager.HandlePluginException(exception, this.ExplorerHandle, plugin.PluginInformation.Name, "On dropdown menu is clicked.");
                    }
                }
            }
        }

        private void pluginDropDown_ItemRightClicked(object sender, ItemRightClickedEventArgs e) {
            ToolStripDropDownItem ownerItem = (ToolStripDropDownItem)((DropDownMenuReorderable)sender).OwnerItem;
            int num = ((int)ownerItem.Tag) - 0x10000;
            if((PluginManager.ActivatedButtonsOrder.Count > num) && (num > -1)) {
                Plugin plugin;
                string pluginID = PluginManager.ActivatedButtonsOrder[num];
                if(this.pluginManager.TryGetPlugin(pluginID, out plugin)) {
                    try {
                        ((IBarDropButton)plugin.Instance).OnDropDownItemClick(e.ClickedItem, MouseButtons.Right);
                    }
                    catch(Exception exception) {
                        PluginManager.HandlePluginException(exception, this.ExplorerHandle, plugin.PluginInformation.Name, "On dropdown menu is right clicked.");
                    }
                }
            }
        }

        private static void ReadSetting() {
            using(RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Quizo\QTTabBar")) {
                if(key != null) {
                    int[] numArray = QTUtility2.ReadRegBinary<int>("Buttons_Order", key);
                    if((numArray == null) || (numArray.Length == 0)) {
                        fNoSettings = true;
                        ButtonIndexes = DefaultButtonIndices;
                    }
                    else {
                        ButtonIndexes = numArray;
                    }
                    byte[] buffer = (byte[])key.GetValue("Config_Buttons", new byte[4]);
                    if(buffer.Length != 4) {
                        ConfigValues = new byte[4];
                    }
                    else {
                        ConfigValues = buffer;
                    }
                    LargeButton = (ConfigValues[0] & 0x80) == 0;
                    BarHeight = LargeButton ? 0x22 : 0x1a;
                    LockDropDownItems = (ConfigValues[0] & 0x40) == 0x40;
                    ImageStripPath = (string)key.GetValue("Buttons_ImagePath", string.Empty);
                    if(ImageStripPath.Length == 0) {
                        ImageStripPath = null;
                    }
                    SearchBoxWidth = (int)key.GetValue("SearchBoxWidth", 100);
                }
                else {
                    fNoSettings = true;
                    ButtonIndexes = DefaultButtonIndices;
                    ConfigValues = new byte[4];
                    LargeButton = true;
                    BarHeight = 0x22;
                }
            }
        }

        private void RearrangeFolderView() {
            QTTabBarLib.Interop.IShellView ppshv = null;
            try {
                if(this.ShellBrowser.QueryActiveShellView(out ppshv) == 0) {
                    IntPtr ptr;
                    IShellFolderView view2 = (IShellFolderView)ppshv;
                    if((view2.GetArrangeParam(out ptr) == 0) && ((((int)ptr) & 0xffff) != 0)) {
                        view2.Rearrange(ptr);
                        view2.Rearrange(ptr);
                    }
                }
            }
            catch {
            }
            finally {
                if(ppshv != null) {
                    Marshal.ReleaseComObject(ppshv);
                    ppshv = null;
                }
            }
        }

        private void RefreshEnabledState(bool fRefreshRequired) {
            QTUtility2.SendCOPYDATASTRUCT(QTUtility.instanceManager.GetTabBarHandle(this.ExplorerHandle), (IntPtr)0xfff, "fromBBRefresh", IntPtr.Zero);
            if(fRefreshRequired) {
                QTTabBarClass tabBar = QTUtility.instanceManager.GetTabBar(this.ExplorerHandle);
                if(tabBar != null) {
                    tabBar.RefreshRebar();
                }
            }
        }

        private void RefreshSearchBox(bool fBrowserRefreshRequired) {
            if(this.searchBox != null) {
                this.searchBox.RefreshText();
            }
            if(this.timerSerachBox_Search != null) {
                this.timerSerachBox_Search.Stop();
            }
            if(this.timerSearchBox_Rearrange != null) {
                this.timerSearchBox_Rearrange.Stop();
            }
            this.strSearch = string.Empty;
            this.fSearchBoxInputStart = false;
            this.iSearchResultCount = -1;
            try {
                foreach(IntPtr ptr in this.lstPUITEMIDCHILD) {
                    if(ptr != IntPtr.Zero) {
                        QTTabBarLib.Interop.PInvoke.CoTaskMemFree(ptr);
                    }
                }
            }
            catch {
            }
            this.lstPUITEMIDCHILD.Clear();
            if(fBrowserRefreshRequired) {
                new WaitTimeoutCallback(QTTabBarClass.WaitTimeout).BeginInvoke(100, new AsyncCallback(this.AsyncComplete), null);
            }
        }

        private static void RefreshTexts() {
            ButtonItemsDisplayName = QTUtility.TextResourcesDic["ButtonBar_BtnName"];
            ResBBOption = QTUtility.TextResourcesDic["ButtonBar_Option"];
        }

        [ComRegisterFunction]
        private static void Register(System.Type t) {
            string name = t.GUID.ToString("B");
            string str2 = (CultureInfo.CurrentCulture.Parent.Name == "ja") ? "QT Tab 標準のボタン" : "QT Tab Standard Buttons";
            using(RegistryKey key = Registry.ClassesRoot.CreateSubKey(@"CLSID\" + name)) {
                key.SetValue(null, str2);
                key.SetValue("MenuText", str2);
                key.SetValue("HelpText", str2);
            }
            using(RegistryKey key2 = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Internet Explorer\Toolbar")) {
                key2.SetValue(name, "QTButtonBar");
            }
        }

        private void ReplaceTokens(IEnumerable lstItems, AppLauncher al, bool fInitialize) {
            foreach(ToolStripItem item in lstItems) {
                QMenuItem item2 = item as QMenuItem;
                if(item2 != null) {
                    MenuItemArguments menuItemArguments = item2.MenuItemArguments;
                    if(menuItemArguments != null) {
                        if(menuItemArguments.Target == MenuTarget.File) {
                            string name = item.Name;
                            if(al.ReplaceTokens_WorkingDir(menuItemArguments)) {
                                name = name + "*";
                            }
                            int num = al.ReplaceTokens_Arguments(menuItemArguments);
                            if(num > 0) {
                                if((num == 1) && (al.iSelItemsCount == 1)) {
                                    name = name + " - " + Path.GetFileName(al.strSelObjs.Trim(new char[] { '"' }));
                                }
                                else if((num == 2) && (al.iSelFileCount == 1)) {
                                    name = name + " - " + Path.GetFileName(al.strSelFiles.Trim(new char[] { '"' }));
                                }
                                else if((num == 4) && (al.iSelDirsCount == 1)) {
                                    name = name + " - " + Path.GetFileName(al.strSelDirs.Trim(new char[] { '"' }));
                                }
                                else if(num == 8) {
                                    name = name + " - Current folder";
                                }
                                else if(num != 10) {
                                    name = name + " - *";
                                }
                            }
                            if(fInitialize && AppLauncher.IsTokened(menuItemArguments)) {
                                this.lstTokenedItems.Add(item2);
                            }
                            item.Text = name;
                            continue;
                        }
                        if((menuItemArguments.Target == MenuTarget.VirtualFolder) && item2.HasDropDownItems) {
                            this.ReplaceTokens(item2.DropDownItems, al, fInitialize);
                        }
                    }
                }
            }
        }

        private static void SaveSetting() {
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar")) {
                if(!LargeButton) {
                    ConfigValues[0] = (byte)(ConfigValues[0] | 0x80);
                }
                else {
                    ConfigValues[0] = (byte)(ConfigValues[0] & 0x7f);
                }
                if(LockDropDownItems) {
                    ConfigValues[0] = (byte)(ConfigValues[0] | 0x40);
                }
                else {
                    ConfigValues[0] = (byte)(ConfigValues[0] & 0xbf);
                }
                QTUtility2.WriteRegBinary<int>(ButtonIndexes, "Buttons_Order", key);
                key.SetValue("Config_Buttons", ConfigValues);
                key.SetValue("Buttons_ImagePath", ImageStripPath);
            }
        }

        private void searchBox_ErasingText(object sender, CancelEventArgs e) {
            e.Cancel = this.lstPUITEMIDCHILD.Count != 0;
        }

        private void searchBox_GotFocus(object sender, EventArgs e) {
            this.OnGotFocus(e);
        }

        private void searchBox_KeyPress(object sender, KeyPressEventArgs e) {
            if(e.KeyChar == '\r') {
                string text = this.searchBox.Text;
                if(text.Length > 0) {
                    this.ShellViewIncrementalSearch(text);
                    e.Handled = true;
                }
            }
            else if(e.KeyChar == '\x001b') {
                QTTabBarClass tabBar = QTUtility.instanceManager.GetTabBar(this.ExplorerHandle);
                if(tabBar != null) {
                    QTTabBarLib.Interop.PInvoke.SetFocus(tabBar.GetSysListView32());
                    e.Handled = true;
                }
            }
        }

        private void searchBox_ResizeComplete(object sender, EventArgs e) {
            SearchBoxWidth = this.searchBox.TextBox.Width;
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar")) {
                key.SetValue("SearchBoxWidth", SearchBoxWidth);
            }
            foreach(IntPtr ptr in QTUtility.instanceManager.ButtonBarHandles()) {
                if((ptr != base.Handle) && QTTabBarLib.Interop.PInvoke.IsWindow(ptr)) {
                    QTUtility2.SendCOPYDATASTRUCT(ptr, (IntPtr)15, "fromBBBC_sb", IntPtr.Zero);
                }
            }
            this.toolStrip.RaiseOnResize();
        }

        private void searchBox_TextChanged(object sender, EventArgs e) {
            this.timerSerachBox_Search.Stop();
            this.timerSearchBox_Rearrange.Stop();
            string text = this.searchBox.Text;
            if(!text.StartsWith("/") || ((text.Length >= 3) && text.EndsWith("/"))) {
                this.fSearchBoxInputStart = true;
                this.strSearch = text;
                this.iSearchResultCount = -1;
                this.timerSerachBox_Search.Start();
            }
        }

        private bool ShellViewIncrementalSearch(string str) {
            QTUtility2.SendCOPYDATASTRUCT(QTUtility.instanceManager.GetTabBarHandle(this.ExplorerHandle), (IntPtr)0xffa, "svis", IntPtr.Zero);
            QTTabBarLib.Interop.IShellView ppshv = null;
            QTTabBarLib.Interop.IShellFolder shellFolder = null;
            IntPtr zero = IntPtr.Zero;
            bool flag = false;
            try {
                if(this.ShellBrowser.QueryActiveShellView(out ppshv) == 0) {
                    int num;
                    QTTabBarLib.Interop.IFolderView view2 = (QTTabBarLib.Interop.IFolderView)ppshv;
                    IShellFolderView view3 = (IShellFolderView)ppshv;
                    QTTabBarLib.Interop.IPersistFolder2 ppv = null;
                    try {
                        Guid riid = ExplorerGUIDs.IID_IPersistFolder2;
                        if(view2.GetFolder(ref riid, out ppv) == 0) {
                            ppv.GetCurFolder(out zero);
                        }
                    }
                    finally {
                        if(ppv != null) {
                            Marshal.ReleaseComObject(ppv);
                            ppv = null;
                        }
                    }
                    if(zero == IntPtr.Zero) {
                        QTUtility2.MakeErrorLog(null, "failed current pidl");
                        return false;
                    }
                    view2.ItemCount(2, out num);
                    IntPtr hwnd = QTUtility.instanceManager.GetTabBar(this.ExplorerHandle).GetSysListView32();
                    QTTabBarLib.Interop.PInvoke.SetRedraw(hwnd, false);
                    try {
                        Regex regex;
                        int num2;
                        if(str.StartsWith("/") && str.EndsWith("/")) {
                            try {
                                regex = new Regex(str.Substring(1, str.Length - 2), RegexOptions.IgnoreCase);
                                goto Label_0184;
                            }
                            catch {
                                SystemSounds.Asterisk.Play();
                                return false;
                            }
                        }
                        if(((this.pluginManager == null) || (this.pluginManager.IFilter == null)) || (!this.pluginManager.IFilter.QueryRegex(str, out regex) || (regex == null))) {
                            string input = Regex.Escape(str);
                            input = reAsterisc.Replace(input, ".*");
                            regex = new Regex(reQuestion.Replace(input, "."), RegexOptions.IgnoreCase);
                        }
                    Label_0184:
                        num2 = num;
                        if(!ShellMethods.GetShellFolder(zero, out shellFolder)) {
                            return false;
                        }
                        bool flag2 = (this.pluginManager != null) && (this.pluginManager.IFilterCore != null);
                        IFilterCore iFilterCore = null;
                        QTPlugin.Interop.IShellFolder folder3 = null;
                        if(flag2) {
                            iFilterCore = this.pluginManager.IFilterCore;
                            folder3 = (QTPlugin.Interop.IShellFolder)shellFolder;
                        }
                        List<IntPtr> collection = new List<IntPtr>();
                        for(int i = 0; i < num2; i++) {
                            IntPtr ptr4;
                            if(view2.Item(i, out ptr4) == 0) {
                                if((flag2 && iFilterCore.IsMatch(folder3, ptr4, regex)) || (!flag2 && CheckDisplayName(shellFolder, ptr4, regex))) {
                                    QTTabBarLib.Interop.PInvoke.CoTaskMemFree(ptr4);
                                }
                                else {
                                    int num4;
                                    collection.Add(ptr4);
                                    if(view3.RemoveObject(ptr4, out num4) == 0) {
                                        num2--;
                                        i--;
                                    }
                                }
                            }
                        }
                        int count = this.lstPUITEMIDCHILD.Count;
                        for(int j = 0; j < count; j++) {
                            IntPtr pIDLChild = this.lstPUITEMIDCHILD[j];
                            if((flag2 && iFilterCore.IsMatch(folder3, pIDLChild, regex)) || (!flag2 && CheckDisplayName(shellFolder, pIDLChild, regex))) {
                                int num7;
                                this.lstPUITEMIDCHILD.RemoveAt(j);
                                count--;
                                j--;
                                view3.AddObject(pIDLChild, out num7);
                                QTTabBarLib.Interop.PInvoke.CoTaskMemFree(pIDLChild);
                                flag = true;
                            }
                        }
                        this.lstPUITEMIDCHILD.AddRange(collection);
                        view2.ItemCount(2, out this.iSearchResultCount);
                    }
                    finally {
                        QTTabBarLib.Interop.PInvoke.SetRedraw(hwnd, true);
                    }
                    this.ShellBrowser.SetStatusTextSB(string.Concat(new object[] { this.iSearchResultCount, " / ", this.iSearchResultCount + this.lstPUITEMIDCHILD.Count, QTUtility.TextResourcesDic["ButtonBar_Misc"][5] }));
                }
                return flag;
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception, null);
                flag = false;
            }
            finally {
                if(ppshv != null) {
                    Marshal.ReleaseComObject(ppshv);
                }
                if((shellFolder != null) && (Marshal.ReleaseComObject(shellFolder) != 0)) {
                    QTUtility2.MakeErrorLog(null, "shellfolder is not released.");
                }
                if(zero != IntPtr.Zero) {
                    QTTabBarLib.Interop.PInvoke.CoTaskMemFree(zero);
                }
            }
            return flag;
        }

        private void SyncButtonBarBroadCast_ClearApps() {
            QTUtility.fRequiredRefresh_App = false;
            this.ClearUserAppsMenu();
            IntPtr dwData = (IntPtr)0x2000000;
            foreach(IntPtr ptr2 in QTUtility.instanceManager.ButtonBarHandles()) {
                if((ptr2 != base.Handle) && QTTabBarLib.Interop.PInvoke.IsWindow(ptr2)) {
                    QTUtility2.SendCOPYDATASTRUCT(ptr2, (IntPtr)1, "fromBBBC", dwData);
                }
            }
        }

        private void timerSearchBox_Rearrange_Tick(object sender, EventArgs e) {
            if(!this.fSearchBoxInputStart) {
                this.timerSearchBox_Rearrange.Stop();
                this.RearrangeFolderView();
            }
        }

        private void timerSerachBox_Search_Tick(object sender, EventArgs e) {
            this.timerSerachBox_Search.Stop();
            bool flag = this.ShellViewIncrementalSearch(this.strSearch);
            this.fSearchBoxInputStart = false;
            if(flag) {
                this.timerSearchBox_Rearrange.Start();
            }
        }

        private void toolStrip_GotFocus(object sender, EventArgs e) {
            if((this != null) && base.IsHandleCreated) {
                this.OnGotFocus(e);
            }
        }

        private void toolStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            if((e.ClickedItem != null) && (e.ClickedItem.Tag != null)) {
                IntPtr tabBarHandle = QTUtility.instanceManager.GetTabBarHandle(this.ExplorerHandle);
                if(tabBarHandle != IntPtr.Zero) {
                    int tag = (int)e.ClickedItem.Tag;
                    if((tag < 0x10000) && (tag != 9)) {
                        IntPtr ptr2;
                        if((tag == 1) || (tag == 2)) {
                            ptr2 = (IntPtr)(0xf00 | tag);
                            QTUtility2.SendCOPYDATASTRUCT(tabBarHandle, ptr2, string.Empty, IntPtr.Zero);
                        }
                        else if((-1 >= tag) || (tag >= 6)) {
                            ptr2 = (IntPtr)(0xf00 | tag);
                            QTUtility2.SendCOPYDATASTRUCT(tabBarHandle, ptr2, string.Empty, IntPtr.Zero);
                        }
                    }
                }
            }
        }

        private void toolStrip_MouseActivated(object sender, EventArgs e) {
            this.ActivatedByClickOnThis();
        }

        private void toolStrip_MouseDoubleClick(object sender, MouseEventArgs e) {
            if(this.toolStrip.GetItemAt(e.Location) == null) {
                QTUtility2.SendCOPYDATASTRUCT(QTUtility.instanceManager.GetTabBarHandle(this.ExplorerHandle), (IntPtr)0xffd, string.Empty, IntPtr.Zero);
            }
        }

        private void toolStrip_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) {
            switch(e.KeyCode) {
                case Keys.Left:
                case Keys.Right:
                case Keys.F6:
                case Keys.Tab:
                    e.IsInputKey = true;
                    DropDownMenuBase.ExitMenuMode();
                    break;

                case Keys.Up:
                    break;

                default:
                    return;
            }
        }

        private void trackBar_ValueChanged(object sender, EventArgs e) {
            int num = ((ToolStripTrackBar)sender).Value;
            QTTabBarLib.Interop.PInvoke.SetWindowLongPtr(this.ExplorerHandle, -20, QTTabBarLib.Interop.PInvoke.Ptr_OP_OR(QTTabBarLib.Interop.PInvoke.GetWindowLongPtr(this.ExplorerHandle, -20), 0x80000));
            QTTabBarLib.Interop.PInvoke.SetLayeredWindowAttributes(this.ExplorerHandle, 0, (byte)num, 2);
            base.Explorer.StatusText = (((num * 100) / 0xff)).ToString() + "%";
        }

        public override int TranslateAcceleratorIO(ref BandObjectLib.MSG msg) {
            if(msg.message == WM.KEYDOWN) {
                Keys wParam = (Keys)((int)((long)msg.wParam));
                switch(wParam) {
                    case Keys.Left:
                    case Keys.Right:
                    case Keys.Tab:
                    case Keys.F6: {
                            switch(wParam) {
                                case Keys.Right:
                                case Keys.Left:
                                    foreach(ToolStripItem item in this.toolStrip.Items) {
                                        if((item.Visible && item.Enabled) && (item.Selected && (item is ToolStripControlHost))) {
                                            return 1;
                                        }
                                    }
                                    break;
                            }
                            bool flag = (Control.ModifierKeys == Keys.Shift) || (wParam == Keys.Left);
                            if(flag && this.toolStrip.OverflowButton.Selected) {
                                for(int j = this.toolStrip.Items.Count - 1; j > -1; j--) {
                                    ToolStripItem item2 = this.toolStrip.Items[j];
                                    if(item2.Visible && item2.Enabled) {
                                        item2.Select();
                                        return 0;
                                    }
                                }
                            }
                            int count = this.toolStrip.Items.Count;
                            for(int i = 0; i < this.toolStrip.Items.Count; i++) {
                                if(this.toolStrip.Items[i].Selected) {
                                    ToolStripItem start = this.toolStrip.Items[i];
                                    if(start is ToolStripControlHost) {
                                        this.toolStrip.Select();
                                    }
                                    while((start = this.toolStrip.GetNextItem(start, flag ? ArrowDirection.Left : ArrowDirection.Right)) != null) {
                                        int index = this.toolStrip.Items.IndexOf(start);
                                        if(flag) {
                                            if((index > i) || (start is ToolStripOverflowButton)) {
                                                return 1;
                                            }
                                        }
                                        else if(index < i) {
                                            if(this.toolStrip.OverflowButton.Visible) {
                                                this.toolStrip.OverflowButton.Select();
                                                return 0;
                                            }
                                            return 1;
                                        }
                                        ToolStripControlHost host = start as ToolStripControlHost;
                                        if(host != null) {
                                            host.Control.Select();
                                            return 0;
                                        }
                                        if(start.Enabled) {
                                            start.Select();
                                            return 0;
                                        }
                                    }
                                    return 1;
                                }
                            }
                            break;
                        }
                    case Keys.Down:
                    case Keys.Space:
                    case Keys.Return:
                        if(this.toolStrip.OverflowButton.Selected) {
                            this.toolStrip.OverflowButton.ShowDropDown();
                            return 0;
                        }
                        for(int k = 0; k < this.toolStrip.Items.Count; k++) {
                            if(this.toolStrip.Items[k].Selected) {
                                ToolStripItem item4 = this.toolStrip.Items[k];
                                if(item4 is ToolStripDropDownItem) {
                                    ((ToolStripDropDownItem)item4).ShowDropDown();
                                }
                                else {
                                    if((item4 is ToolStripSearchBox) && ((wParam == Keys.Return) || (wParam == Keys.Space))) {
                                        return 1;
                                    }
                                    if(wParam != Keys.Down) {
                                        item4.PerformClick();
                                    }
                                }
                                return 0;
                            }
                        }
                        break;

                    case Keys.Back:
                        foreach(ToolStripItem item5 in this.toolStrip.Items) {
                            if(item5.Selected && (item5 is ToolStripControlHost)) {
                                QTTabBarLib.Interop.PInvoke.SendMessage(msg.hwnd, 0x102, msg.wParam, msg.lParam);
                                return 0;
                            }
                        }
                        break;

                    case Keys.A:
                    case Keys.C:
                    case Keys.V:
                    case Keys.X:
                    case Keys.Z:
                        if(((Control.ModifierKeys == Keys.Control) && (this.searchBox != null)) && this.searchBox.Selected) {
                            QTTabBarLib.Interop.PInvoke.TranslateMessage(ref msg);
                            if(wParam == Keys.A) {
                                this.searchBox.TextBox.SelectAll();
                            }
                            return 0;
                        }
                        break;

                    case Keys.Delete:
                        foreach(ToolStripItem item6 in this.toolStrip.Items) {
                            if(item6.Selected && (item6 is ToolStripControlHost)) {
                                QTTabBarLib.Interop.PInvoke.SendMessage(msg.hwnd, 0x100, msg.wParam, msg.lParam);
                                return 0;
                            }
                        }
                        break;
                }
            }
            return 1;
        }

        public override void UIActivateIO(int fActivate, ref BandObjectLib.MSG Msg) {
            if(fActivate != 0) {
                this.toolStrip.Focus();
                if(this.toolStrip.Items.Count != 0) {
                    ToolStripItem item;
                    if(Control.ModifierKeys != Keys.Shift) {
                        for(int i = 0; i < this.toolStrip.Items.Count; i++) {
                            item = this.toolStrip.Items[i];
                            if((item.Enabled && item.Visible) && !(item is ToolStripSeparator)) {
                                if(item is ToolStripControlHost) {
                                    ((ToolStripControlHost)item).Control.Select();
                                    return;
                                }
                                item.Select();
                                return;
                            }
                        }
                    }
                    else if(this.toolStrip.OverflowButton.Visible) {
                        this.toolStrip.OverflowButton.Select();
                    }
                    else {
                        for(int j = this.toolStrip.Items.Count - 1; j > -1; j--) {
                            item = this.toolStrip.Items[j];
                            if((item.Enabled && item.Visible) && !(item is ToolStripSeparator)) {
                                if(item is ToolStripControlHost) {
                                    ((ToolStripControlHost)item).Control.Select();
                                    return;
                                }
                                item.Select();
                                return;
                            }
                        }
                    }
                }
            }
        }

        private void UnloadPluginsOnCreation() {
            if(this.pluginManager != null) {
                foreach(Plugin plugin in this.pluginManager.Plugins) {
                    PluginType pluginType = plugin.PluginInformation.PluginType;
                    string pluginID = plugin.PluginInformation.PluginID;
                    if(pluginType == PluginType.Interactive) {
                        if(!PluginManager.ActivatedButtonsOrder.Contains(pluginID)) {
                            this.pluginManager.UnloadPluginInstance(pluginID, EndCode.Unloaded, true);
                        }
                        continue;
                    }
                    if(((pluginType == PluginType.Background) || (pluginType == PluginType.BackgroundMultiple)) && (plugin.BackgroundButtonEnabled && !PluginManager.ActivatedButtonsOrder.Contains(pluginID))) {
                        try {
                            if(plugin.Instance != null) {
                                plugin.Instance.Close(EndCode.Hidden);
                            }
                        }
                        catch(Exception exception) {
                            PluginManager.HandlePluginException(exception, this.ExplorerHandle, plugin.PluginInformation.Name, "Closing plugin button. (EndCode.Hidden)");
                        }
                        if(pluginType == PluginType.Background) {
                            PluginManager.RemoveFromButtonBarOrder(pluginID);
                        }
                        else {
                            while(PluginManager.RemoveFromButtonBarOrder(pluginID)) {
                            }
                        }
                        plugin.BackgroundButtonEnabled = false;
                    }
                }
                this.pluginManager.ClearBackgroundMultiples();
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
                using(RegistryKey key2 = Registry.ClassesRoot.CreateSubKey("CLSID")) {
                    key2.DeleteSubKeyTree(name);
                }
            }
            catch {
            }
        }

        private void userAppsSubDir_DoubleCliced(object sender, EventArgs e) {
            this.ddmrUserAppButton.Close();
            string path = ((QMenuItem)sender).Path;
            IntPtr dwData = (Control.ModifierKeys != Keys.Control) ? IntPtr.Zero : ((IntPtr)1);
            QTUtility2.SendCOPYDATASTRUCT(QTUtility.instanceManager.GetTabBarHandle(this.ExplorerHandle), (IntPtr)0xf04, path, dwData);
        }

        protected override void WndProc(ref System.Windows.Forms.Message m) {
            int num3;
            int num4;
            switch(m.Msg) {
                case WM.INITMENUPOPUP:
                case WM.DRAWITEM:
                case WM.MEASUREITEM:
                    if((this.iContextMenu2 == null) || !(m.HWnd == base.Handle)) {
                        goto Label_08F8;
                    }
                    try {
                        this.iContextMenu2.HandleMenuMsg(m.Msg, m.WParam, m.LParam);
                    }
                    catch {
                    }
                    return;

                case WM.DROPFILES:
                    QTTabBarLib.Interop.PInvoke.SendMessage(QTUtility.instanceManager.GetTabBarHandle(this.ExplorerHandle), 0x233, m.WParam, IntPtr.Zero);
                    return;

                case WM.APP:
                    m.Result = this.toolStrip.IsHandleCreated ? this.toolStrip.Handle : IntPtr.Zero;
                    return;

                case WM.COPYDATA: {
                        QTTabBarLib.Interop.COPYDATASTRUCT copydatastruct = (QTTabBarLib.Interop.COPYDATASTRUCT)Marshal.PtrToStructure(m.LParam, typeof(QTTabBarLib.Interop.COPYDATASTRUCT));
                        switch(((int)m.WParam)) {
                            case 1: {
                                    int num = ((int)copydatastruct.dwData) & 0xffff;
                                    int num2 = (((int)copydatastruct.dwData) >> 0x10) & 0xffff;
                                    if((num2 & 1) == 1) {
                                        this.EnableItemAt(3, (num & 1) == 1);
                                    }
                                    if((num2 & 2) == 2) {
                                        this.EnableItemAt(4, (num & 2) == 2);
                                    }
                                    if((num2 & 4) == 4) {
                                        this.EnableItemAt(5, (num & 4) == 4);
                                    }
                                    if((num2 & 8) == 8) {
                                        this.toolStrip.ShowItemToolTips = (num & 8) == 8;
                                    }
                                    if((num2 & 0x100) == 0x100) {
                                        this.ClearUserAppsMenu();
                                        this.CreateItems(false);
                                    }
                                    if((num2 & 0x200) == 0x200) {
                                        this.ClearUserAppsMenu();
                                    }
                                    return;
                                }
                            case 2:
                                num3 = ((int)copydatastruct.dwData) & 0xffff;
                                num4 = (((int)copydatastruct.dwData) >> 0x10) & 0xffff;
                                if((num4 & 1) == 1) {
                                    this.EnableItemAt(1, (num3 & 1) == 1);
                                }
                                if((num4 & 2) == 2) {
                                    this.EnableItemAt(2, (num3 & 2) == 2);
                                }
                                if(((num4 & 1) == 1) || ((num4 & 2) == 2)) {
                                    this.EnableItemAt(-1, ((num3 & 1) == 1) || ((num3 & 2) == 2));
                                }
                                if((num4 & 4) == 4) {
                                    this.EnableItemAt(14, (num3 & 4) == 4);
                                }
                                if((num4 & 8) == 8) {
                                    this.EnableItemAt(15, (num3 & 8) == 8);
                                }
                                if((num4 & 0x10) == 0x10) {
                                    this.EnableItemAt(12, (num3 & 0x10) == 0x10);
                                }
                                if((num4 & 0x20) == 0x20) {
                                    this.EnableItemAt(11, (num3 & 0x20) == 0x20);
                                }
                                if((num4 & 0x40) == 0x40) {
                                    foreach(ToolStripItem item in this.toolStrip.Items) {
                                        if((item.Tag != null) && (((int)item.Tag) == 10)) {
                                            ((ToolStripButton)item).Checked = (num3 & 0x40) == 0x40;
                                            break;
                                        }
                                    }
                                }
                                break;

                            case 3:
                            case 6:
                                return;

                            case 4:
                                if(this.DoItems((int)copydatastruct.dwData)) {
                                    m.Result = (IntPtr)1;
                                }
                                return;

                            case 5:
                                this.CreateItems(true);
                                this.menuLockItem.Checked = LockDropDownItems;
                                this.RefreshEnabledState(copydatastruct.dwData != IntPtr.Zero);
                                foreach(ToolStripItem item2 in this.toolStrip.Items) {
                                    if((item2.Tag != null) && (((int)item2.Tag) == 10)) {
                                        ((ToolStripButton)item2).Checked = QTTabBarLib.Interop.PInvoke.Ptr_OP_AND(QTTabBarLib.Interop.PInvoke.GetWindowLongPtr(this.ExplorerHandle, -20), 8) == new IntPtr(8);
                                        break;
                                    }
                                }
                                return;

                            case 7:
                                foreach(ToolStripItem item3 in this.toolStrip.Items) {
                                    if((item3.Tag != null) && (((int)item3.Tag) == 0x13)) {
                                        ((ToolStripTrackBar)item3).SetValueWithoutEvent((int)copydatastruct.dwData);
                                        break;
                                    }
                                }
                                return;

                            case 8:
                                if(this.searchBox != null) {
                                    this.searchBox.TextBox.Focus();
                                }
                                return;

                            case 9:
                                this.RefreshSearchBox(false);
                                return;

                            case 10:
                                RefreshTexts();
                                return;

                            case 11:
                                if(this.pluginManager != null) {
                                    string pluginID = Marshal.PtrToStringAuto(copydatastruct.lpData);
                                    for(int i = 0; i < PluginManager.ActivatedButtonsOrder.Count; i++) {
                                        if(pluginID == PluginManager.ActivatedButtonsOrder[i]) {
                                            foreach(ToolStripItem item4 in this.toolStrip.Items) {
                                                if(((int)item4.Tag) == (0x10000 + i)) {
                                                    Plugin plugin;
                                                    int dwData = (int)copydatastruct.dwData;
                                                    bool flag = (dwData & 1) == 1;
                                                    bool flag2 = (dwData & 2) == 2;
                                                    item4.Enabled = flag;
                                                    if(!this.pluginManager.TryGetPlugin(pluginID, out plugin)) {
                                                        break;
                                                    }
                                                    IBarButton instance = plugin.Instance as IBarButton;
                                                    if(instance == null) {
                                                        break;
                                                    }
                                                    try {
                                                        item4.ToolTipText = instance.Text;
                                                        if(flag2) {
                                                            item4.Image = instance.GetImage(LargeButton);
                                                        }
                                                        break;
                                                    }
                                                    catch(Exception exception) {
                                                        PluginManager.HandlePluginException(exception, this.ExplorerHandle, plugin.PluginInformation.Name, "Refreshing plugin image and text.");
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                return;

                            case 12:
                                this.contextMenu_ItemClicked(null, new ToolStripItemClickedEventArgs(this.menuCustomize));
                                return;

                            case 13:
                                if((this.iSearchResultCount > -1) && (this.iSearchResultCount != ((int)copydatastruct.dwData))) {
                                    this.RefreshSearchBox(true);
                                }
                                return;

                            case 14:
                                if(this.iSearchResultCount > 0) {
                                    this.iSearchResultCount--;
                                }
                                return;

                            case 15:
                                if(this.searchBox != null) {
                                    this.searchBox.TextBox.Width = SearchBoxWidth;
                                    this.toolStrip.RaiseOnResize();
                                }
                                return;

                            case 16: {
                                    if(this.searchBox == null) {
                                        m.Result = (IntPtr)1;
                                        return;
                                    }
                                    string str2 = Marshal.PtrToStringUni(copydatastruct.lpData);
                                    if(str2 == "null") {
                                        str2 = string.Empty;
                                    }
                                    this.searchBox.Focus();
                                    this.searchBox.Text = str2;
                                    m.Result = IntPtr.Zero;
                                    return;
                                }
                        }
                        return;
                    }
                case WM.CONTEXTMENU:
                    if((((this.ddmrGroupButton == null) || !this.ddmrGroupButton.Visible) && ((this.ddmrUserAppButton == null) || !this.ddmrUserAppButton.Visible)) && ((this.ddmrRecentlyClosed == null) || !this.ddmrRecentlyClosed.Visible)) {
                        Point p = new Point(QTUtility2.GET_X_LPARAM(m.LParam), QTUtility2.GET_Y_LPARAM(m.LParam));
                        if((p.X == -1) && (p.Y == -1)) {
                            QTUtility2.SendCOPYDATASTRUCT(QTUtility.instanceManager.GetTabBarHandle(this.ExplorerHandle), (IntPtr)0xffc, "btnCM", (IntPtr)1);
                            return;
                        }
                        if((this.toolStrip.GetItemAt(this.toolStrip.PointToClient(p)) != null) || (this.toolStrip.Items.Count == 0)) {
                            goto Label_08F8;
                        }
                        QTUtility2.SendCOPYDATASTRUCT(QTUtility.instanceManager.GetTabBarHandle(this.ExplorerHandle), (IntPtr)0xffc, "btnCM", IntPtr.Zero);
                    }
                    return;

                default:
                    goto Label_08F8;
            }
            if((num4 & 0x80) == 0x80) {
                this.EnableItemAt(0x10, (num3 & 0x80) == 0);
            }
            if(num4 == 0x100) {
                if(this.pluginManager == null) {
                    QTTabBarClass tabBar = QTUtility.instanceManager.GetTabBar(this.ExplorerHandle);
                    if(tabBar != null) {
                        this.pluginManager = tabBar.PluginServerInstance;
                        if(this.pluginManager != null) {
                            this.pluginManager.AddRef();
                        }
                    }
                }
                this.CreateItems(false);
            }
            if((this.NavDropDown != null) && this.NavDropDown.Visible) {
                this.NavDropDown.Close(ToolStripDropDownCloseReason.AppClicked);
            }
            if((this.ddmrGroupButton != null) && this.ddmrGroupButton.Visible) {
                this.ddmrGroupButton.Close(ToolStripDropDownCloseReason.AppClicked);
            }
            if((this.ddmrRecentlyClosed != null) && this.ddmrRecentlyClosed.Visible) {
                this.ddmrRecentlyClosed.Close(ToolStripDropDownCloseReason.AppClicked);
            }
            if((this.ddmrUserAppButton != null) && this.ddmrUserAppButton.Visible) {
                this.ddmrUserAppButton.Close(ToolStripDropDownCloseReason.AppClicked);
            }
            return;
        Label_08F8:
            base.WndProc(ref m);
        }

        private QTTabBarLib.Interop.IShellBrowser ShellBrowser {
            get {
                if(this.shellBrowser == null) {
                    QTTabBarClass tabBar = QTUtility.instanceManager.GetTabBar(this.ExplorerHandle);
                    if(tabBar != null) {
                        this.shellBrowser = tabBar.GetShellBrower();
                    }
                }
                return this.shellBrowser;
            }
        }
    }
}
