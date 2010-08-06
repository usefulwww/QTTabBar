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
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using QTPlugin;
using QTTabBarLib.Interop;
using Timer = System.Windows.Forms.Timer;

namespace QTTabBarLib {
    public abstract class ExtendedListViewCommon : AbstractListView {

        #region Delegates
        internal delegate bool DoubleClickHandler(Point pt);
        internal delegate void EndLabelEditHandler(LVITEM item);
        internal delegate bool ItemActivatedHandler(Keys modKeys);
        internal delegate void ItemCountChangedHandler(int count);
        internal delegate bool MiddleClickHandler(Point pt);
        internal delegate bool MouseActivateHandler(ref int result);
        internal delegate void SelectionChangedHandler();
        #endregion

        #region Events
        internal event DoubleClickHandler DoubleClick;            // OK
        internal event EndLabelEditHandler EndLabelEdit;          // SysListView Only
        internal event ItemActivatedHandler ItemActivated;        // OK
        internal event ItemCountChangedHandler ItemCountChanged;  // OK
        internal event MiddleClickHandler MiddleClick;            // OK
        internal event MouseActivateHandler MouseActivate;        // OK
        internal event SelectionChangedHandler SelectionChanged;  // OK
        internal event EventHandler SubDirTip_MenuClosed;
        internal event ToolStripItemClickedEventHandler SubDirTip_MenuItemClicked;
        internal event ItemRightClickedEventHandler SubDirTip_MenuItemRightClicked;
        internal event EventHandler SubDirTip_MultipleMenuItemsClicked;
        internal event ItemRightClickedEventHandler SubDirTip_MultipleMenuItemsRightClicked;
        #endregion

        protected static readonly Int32 WM_AFTERPAINT = (Int32)PInvoke.RegisterWindowMessage("QTTabBar_AfterPaint");

        protected NativeWindowController ListViewController;
        protected NativeWindowController ShellViewController;
        private bool fThumbnailPending;
        private bool fTrackMouseEvent;
        private IntPtr hwndExplorer;
        private IntPtr hwndSubDirTipMessageReflect;
        private int itemIndexDROPHILITED = -1;
        protected ShellBrowserEx ShellBrowser;
        private int subDirIndex = -1;
        private SubDirTipForm subDirTip;
        private int thumbnailIndex = -1;
        private ThumbnailTooltipForm thumbnailTooltip;
        private Timer timer_HoverSubDirTipMenu;
        private Timer timer_HoverThumbnail;
        private Timer timer_Thumbnail;

        internal ExtendedListViewCommon(ShellBrowserEx shellBrowser, IntPtr hwndShellView, IntPtr hwndListView, IntPtr hwndSubDirTipMessageReflect) {
            this.ShellBrowser = shellBrowser;
            this.hwndSubDirTipMessageReflect = hwndSubDirTipMessageReflect;

            ListViewController = new NativeWindowController(hwndListView);
            ListViewController.MessageCaptured += ListViewController_MessageCaptured;
            ShellViewController = new NativeWindowController(hwndShellView);
            ShellViewController.MessageCaptured += ShellViewController_MessageCaptured;

            TRACKMOUSEEVENT structure = new TRACKMOUSEEVENT();
            structure.cbSize = Marshal.SizeOf(structure);
            structure.dwFlags = 2;
            structure.hwndTrack = ListViewController.Handle;
            PInvoke.TrackMouseEvent(ref structure);

            hwndExplorer = PInvoke.GetAncestor(hwndShellView, 3 /* GA_ROOTOWNER */);
        }

        public override IntPtr Handle {
            get { return ListViewController.Handle; }
        }

        // TODO: make this an enum
        public override int ViewMode {
            get { return ShellBrowser.ViewMode; }
        }

        #region IDisposable Members

        public override void Dispose(bool fDisposing) {
            if(fDisposed) return;
            if(ListViewController != null) {
                ListViewController.ReleaseHandle();
                ListViewController = null;
            }
            if(ShellViewController != null) {
                ShellViewController.ReleaseHandle();
                ShellViewController = null;
            }
            if(timer_HoverSubDirTipMenu != null) {
                timer_HoverSubDirTipMenu.Dispose();
                timer_HoverSubDirTipMenu = null;
            }
            if(timer_HoverThumbnail != null) {
                timer_HoverThumbnail.Dispose();
                timer_HoverThumbnail = null;
            }
            if(timer_Thumbnail != null) {
                timer_Thumbnail.Dispose();
                timer_Thumbnail = null;
            }
            if(this.thumbnailTooltip != null) {
                this.thumbnailTooltip.Dispose();
                this.thumbnailTooltip = null;
            }
            if(this.subDirTip != null) {
                this.subDirTip.Dispose();
                this.subDirTip = null;
            }
  
            base.Dispose(fDisposing);
        }


        #endregion

        public abstract override IntPtr GetEditControl();

        public abstract override int GetFocusedItem(); 

        public abstract override Rectangle GetFocusedItemRect(); 

        public override int GetHotItem() {
            return HitTest(Control.MousePosition, true);
        }

        public abstract override int GetItemCount();

        public abstract override int GetSelectedCount();

        public abstract override Point GetSubDirTipPoint(bool fByKey);

        public override void HandleF2() {
            IntPtr hWnd = GetEditControl();
            if(hWnd == IntPtr.Zero) return;
            IntPtr lParam = Marshal.AllocHGlobal(520);
            if(0 < ((int)PInvoke.SendMessage(hWnd, 13, (IntPtr)260, lParam))) {
                string str = Marshal.PtrToStringUni(lParam);
                if(str.Length > 2) {
                    int num = str.LastIndexOf(".");
                    if(num != -1) {
                        IntPtr ptr3 = PInvoke.SendMessage(hWnd, 0xb0, IntPtr.Zero, IntPtr.Zero);
                        int start = QTUtility2.GET_X_LPARAM(ptr3);
                        int length = QTUtility2.GET_Y_LPARAM(ptr3);
                        if((length - start) >= 0) {
                            if((start == 0) && (length == num)) {
                                start = length = num;
                            }
                            else if((start == length) && (length == num)) {
                                start = num + 1;
                                length = str.Length;
                            }
                            else if((start == (num + 1)) && (length == str.Length)) {
                                start = 0;
                                length = -1;
                            }
                            else if((start == 0) && (length == str.Length)) {
                                start = 0;
                                length = 0;
                            }
                            else {
                                start = 0;
                                length = num;
                            }
                            PInvoke.SendMessage(hWnd, 0xb1, (IntPtr)start, (IntPtr)length);
                        }
                    }
                }
            }
            Marshal.FreeHGlobal(lParam);
        }

        protected bool HandleLVKEYDOWN_CursorLoop(Keys key) {
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

        public override bool HasFocus() {
            return (ListViewController != null &&
                PInvoke.GetFocus() == ListViewController.Handle);
        }

        public override void HideSubDirTip(int iReason) {
            if((this.subDirTip != null) && this.subDirTip.IsShowing) {
                bool fForce = iReason < 0;
                if(fForce || !this.subDirTip.IsShownByKey) {
                    this.subDirTip.HideSubDirTip(fForce);
                    this.subDirIndex = -1;
                }
            }
            this.itemIndexDROPHILITED = -1;
        }

        public override void HideSubDirTipMenu() {
            if(subDirTip != null) {
                subDirTip.HideMenu();
            }
        }

        public override void HideSubDirTip_ExplorerInactivated() {
            if((this.subDirTip != null) && this.subDirTip.IsShowing) {
                this.subDirTip.OnExplorerInactivated();
            }
        }

        public override void HideThumbnailTooltip(int iReason) {
            if((this.thumbnailTooltip != null) && this.thumbnailTooltip.IsShowing) {
                if(((iReason == 0) || (iReason == 7)) || (iReason == 9)) {
                    this.thumbnailTooltip.IsShownByKey = false;
                }
                if(this.thumbnailTooltip.HideToolTip()) {
                    this.thumbnailIndex = -1;
                }
            }
        }

        public override int HitTest(IntPtr LParam) {
            return HitTest(new Point(QTUtility2.GET_X_LPARAM(LParam), QTUtility2.GET_Y_LPARAM(LParam)), false);
        }

        public abstract override int HitTest(Point pt, bool ScreenCoords);

        public abstract override bool HotItemIsSelected(); 

        // If the ListView is in Details mode, returns true only if the mouse
        // is over the ItemName column.  Returns true always for any other mode.
        // This function only returns valid results if the mouse is known to be
        // over an item.  Otherwise, its return value is undefined.
        public abstract override bool IsTrackingItemName();

        protected virtual bool ListViewController_MessageCaptured(ref Message msg) {

            if(msg.Msg == WM_AFTERPAINT) {
                RefreshSubDirTip(true);
                return true;
            }

            switch(msg.Msg) {
                case WM.DESTROY:
                    this.HideThumbnailTooltip(7);
                    this.HideSubDirTip(7);
                    if(this.timer_HoverThumbnail != null) {
                        this.timer_HoverThumbnail.Enabled = false;
                    }
                    ListViewController.DefWndProc(ref msg);
                    OnListViewDestroyed();
                    return true;

                case WM.PAINT:
                    // It's very dangerous to do automation-related things
                    // during WM_PAINT.  So, use PostMessage to do it later.
                    PInvoke.PostMessage(ListViewController.Handle, (uint)WM_AFTERPAINT, IntPtr.Zero, IntPtr.Zero);
                    break;

                case WM.MOUSEMOVE:
                    ResetTrackMouseEvent();
                    break;

                case WM.LBUTTONDBLCLK:
                    if(DoubleClick != null) {
                        return DoubleClick(new Point(
                            QTUtility2.GET_X_LPARAM(msg.LParam),
                            QTUtility2.GET_Y_LPARAM(msg.LParam)));
                    }
                    break;
                
                case WM.MBUTTONUP:
                    if(MiddleClick != null) {
                        MiddleClick(new Point(
                            QTUtility2.GET_X_LPARAM(msg.LParam),
                            QTUtility2.GET_Y_LPARAM(msg.LParam)));
                    }
                    break;

                case WM.MOUSELEAVE:
                    fTrackMouseEvent = true;
                    this.HideThumbnailTooltip(4);
                    if(this.timer_HoverThumbnail != null) {
                        this.timer_HoverThumbnail.Enabled = false;
                    }
                    if(((this.subDirTip != null) && !this.subDirTip.MouseIsOnThis()) && !this.subDirTip.MenuIsShowing) {
                        this.HideSubDirTip(5);
                    }
                    break;
            }
            return false;
        }

        public override bool MouseIsOverListView() {
            return (ListViewController != null &&
                PInvoke.WindowFromPoint(Control.MousePosition) == ListViewController.Handle);
        }

        protected bool OnDoubleClick(Point pt) {
            return DoubleClick != null && DoubleClick(pt);
        }

        protected void OnDropHilighted(int iItem) {
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
                            timer_HoverSubDirTipMenu = new Timer();
                            timer_HoverSubDirTipMenu.Interval = 1200;
                            timer_HoverSubDirTipMenu.Tick += timer_HoverSubDirTipMenu_Tick;
                        }
                        itemIndexDROPHILITED = iItem;
                        timer_HoverSubDirTipMenu.Enabled = false;
                        timer_HoverSubDirTipMenu.Enabled = true;
                    }
                }
            }
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
                            timer_HoverSubDirTipMenu = new Timer();
                            timer_HoverSubDirTipMenu.Interval = 1200;
                            timer_HoverSubDirTipMenu.Tick += timer_HoverSubDirTipMenu_Tick;
                        }
                        itemIndexDROPHILITED = iItem;
                        timer_HoverSubDirTipMenu.Enabled = false;
                        timer_HoverSubDirTipMenu.Enabled = true;
                    }
                }
            }
        }

        protected void OnEndLabelEdit(LVITEM item) {
            if(EndLabelEdit != null) {
                EndLabelEdit(item);
            }
        }

        protected bool OnGetInfoTip(int iItem, bool byKey) {
            if(QTUtility.CheckConfig(Settings.ShowTooltipPreviews) && (!QTUtility.CheckConfig(Settings.PreviewsWithShift) ^ (Control.ModifierKeys == Keys.Shift))) {
                if(((this.thumbnailTooltip != null) && this.thumbnailTooltip.IsShowing) && (iItem == this.thumbnailIndex)) {
                    return true;
                }
                else if((this.timer_HoverThumbnail != null) && this.timer_HoverThumbnail.Enabled) {
                    return true;
                }
                else if(byKey) {
                    Rectangle rect = GetFocusedItemRect();
                    return this.ShowThumbnailTooltip(iItem, new Point(rect.Right - 32, rect.Bottom - 16), true);
                }
                else {
                    return this.ShowThumbnailTooltip(iItem, Control.MousePosition, false);
                }
            }
            return false;
        }

        protected void OnHotTrack(int iItem) {
            Keys modifierKeys = Control.ModifierKeys;
            if(QTUtility.CheckConfig(Settings.ShowTooltipPreviews)) {
                if((this.thumbnailTooltip != null) && (this.thumbnailTooltip.IsShowing || this.fThumbnailPending)) {
                    if(!QTUtility.CheckConfig(Settings.PreviewsWithShift) ^ (modifierKeys == Keys.Shift)) {
                        if(iItem != this.thumbnailIndex) {
                            if(iItem > -1 && IsTrackingItemName()) {
                                if(this.ShowThumbnailTooltip(iItem, Control.MousePosition, false)) {
                                    return;
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
                    this.timer_HoverThumbnail = new Timer();
                    this.timer_HoverThumbnail.Interval = (int)(SystemInformation.MouseHoverTime * 0.2);
                    this.timer_HoverThumbnail.Tick += this.timer_HoverThumbnail_Tick;
                }
                this.timer_HoverThumbnail.Enabled = false;
                this.timer_HoverThumbnail.Enabled = true;
            }
            RefreshSubDirTip();
            
            return;
        }

        protected bool OnItemActivated(Keys modKeys) {
            return ItemActivated != null && ItemActivated(modKeys);
        }

        protected void OnItemCountChanged() {
            if(ItemCountChanged != null) {
                ItemCountChanged(GetItemCount());
            }
        }

        protected bool OnKeyDown(Keys key) {
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

        protected bool OnMiddleClick(Point pt) {
            return MiddleClick != null && MiddleClick(pt);
        }

        protected bool OnMouseActivate(ref int result) {
            return MouseActivate != null && MouseActivate(ref result);
        }

        protected void OnSelectionChanged() {
            if(SelectionChanged != null) {
                SelectionChanged();
            }
        }

        protected virtual bool OnShellViewNotify(NMHDR nmhdr, ref Message msg) {
            if(nmhdr.hwndFrom != ListViewController.Handle) {
                if(nmhdr.code == -12 /*NM_CUSTOMDRAW*/ && nmhdr.idFrom == IntPtr.Zero) {
                    ResetTrackMouseEvent();
                }
            }
            return false;
        }

        public abstract override bool PointIsBackground(Point pt, bool screenCoords); 

        public override void RefreshSubDirTip(bool force = false) {
            RefreshSubDirTip(force, GetHotItem());    
        }

        private void RefreshSubDirTip(bool force, int iItem) {
            if(!QTUtility.CheckConfig(Settings.NoShowSubDirTips)) {
                if(!QTUtility.CheckConfig(Settings.SubDirTipsWithShift) ^ (Control.ModifierKeys == Keys.Shift)) {
                    if(subDirTip != null && subDirTip.MouseIsOnThis()) {
                        return;
                    }
                    if(!force && this.subDirIndex == iItem && (QTUtility.IsVista || (iItem != -1))) {
                        return;
                    }
                    if(QTUtility.IsVista) {
                        this.subDirIndex = iItem;
                    }
                    if(iItem > -1 && this.ShowSubDirTip(iItem, false, false)) {
                        if(!QTUtility.IsVista) {
                            this.subDirIndex = iItem;
                        }
                        return;
                    }
                }
                this.HideSubDirTip(2);
                this.subDirIndex = -1;
            }
        }

        private void ResetTrackMouseEvent() {
            if(this.fTrackMouseEvent) {
                this.fTrackMouseEvent = false;
                TRACKMOUSEEVENT structure = new TRACKMOUSEEVENT();
                structure.cbSize = Marshal.SizeOf(structure);
                structure.dwFlags = 2;
                structure.hwndTrack = Handle;
                PInvoke.TrackMouseEvent(ref structure);
            }
        }

        public override void ScrollHorizontal(int amount) {
            if(ListViewController != null) {
                // We'll intercept this message later for the ItemsView.  It's
                // important to use PostMessage here to prevent reentry issues
                // with the Automation Thread.
                PInvoke.PostMessage(ListViewController.Handle, LVM.SCROLL, (IntPtr)(-amount), IntPtr.Zero);
            }
        }

        public override void SetFocus() {
            if(ListViewController != null) {
                PInvoke.SetFocus(ListViewController.Handle);
            }
        }

        public override void SetRedraw(bool redraw) {
            if(ListViewController != null) {
                PInvoke.SetRedraw(ListViewController.Handle, redraw);
            }
        }

        protected virtual bool ShellViewController_MessageCaptured(ref Message msg) {
            if(msg.Msg == WM.MOUSEACTIVATE) {
                int res = (int)msg.Result;
                bool ret = OnMouseActivate(ref res);
                msg.Result = (IntPtr)res;
                return ret;
            }
            else if(msg.Msg == WM.NOTIFY) {
                NMHDR nmhdr = (NMHDR)Marshal.PtrToStructure(msg.LParam, typeof(NMHDR));
                return OnShellViewNotify(nmhdr, ref msg);
            }
            return false;
        }

        public override void ShowAndClickSubDirTip() {
            try {
                Address[] addressArray;
                string str;
                if(ShellBrowser.TryGetSelection(out addressArray, out str, false) && ((addressArray.Length == 1) && !string.IsNullOrEmpty(addressArray[0].Path))) {
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
                        this.subDirTip = new SubDirTipForm(hwndSubDirTipMessageReflect, true, this);
                        this.subDirTip.MenuClosed += this.subDirTip_MenuClosed;
                        this.subDirTip.MenuItemClicked += this.subDirTip_MenuItemClicked;
                        this.subDirTip.MultipleMenuItemsClicked += this.subDirTip_MultipleMenuItemsClicked;
                        this.subDirTip.MenuItemRightClicked += this.subDirTip_MenuItemRightClicked;
                        this.subDirTip.MultipleMenuItemsRightClicked += this.subDirTip_MultipleMenuItemsRightClicked;
                    }

                    int iItem = GetFocusedItem();
                    if(iItem != -1) {
                        this.ShowSubDirTip(iItem, true, false);
                        this.subDirTip.PerformClickByKey();
                    }
                }
            }
            catch {
            }
        }

        private bool ShowSubDirTip(int iItem, bool fByKey, bool fSkipForegroundCheck) {
            string str;
            if((fSkipForegroundCheck || (this.hwndExplorer == PInvoke.GetForegroundWindow())) && ShellBrowser.TryGetHotTrackPath(iItem, out str)) {
                bool flag = false;
                try {
                    if(!TryMakeSubDirTipPath(ref str)) {
                        return false;
                    }
                    Point pnt = GetSubDirTipPoint(fByKey);
                    if(this.subDirTip == null) {
                        this.subDirTip = new SubDirTipForm(hwndSubDirTipMessageReflect, true, this);
                        this.subDirTip.MenuClosed += this.subDirTip_MenuClosed;
                        this.subDirTip.MenuItemClicked += this.subDirTip_MenuItemClicked;
                        this.subDirTip.MultipleMenuItemsClicked += this.subDirTip_MultipleMenuItemsClicked;
                        this.subDirTip.MenuItemRightClicked += this.subDirTip_MenuItemRightClicked;
                        this.subDirTip.MultipleMenuItemsRightClicked += this.subDirTip_MultipleMenuItemsRightClicked;
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

        private bool ShowThumbnailTooltip(int iItem, Point pnt, bool fKey) {
            string linkTargetPath;
            if(ShellBrowser.TryGetHotTrackPath(iItem, out linkTargetPath)) {
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
                        this.thumbnailTooltip.ThumbnailVisibleChanged += this.thumbnailTooltip_ThumbnailVisibleChanged;
                        this.timer_Thumbnail = new Timer();
                        this.timer_Thumbnail.Interval = 400;
                        this.timer_Thumbnail.Tick += this.timer_Thumbnail_Tick;
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

        public override bool SubDirTipMenuIsShowing() {
            return subDirTip != null && subDirTip.MenuIsShowing;
        }

        public static bool TryMakeSubDirTipPath(ref string path) {
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
                FileSystemInfo targetIfFolderLink = ShellMethods.GetTargetIfFolderLink(new DirectoryInfo(path), out flag);
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

        private void subDirTip_MenuClosed(object sender, EventArgs e) {
            if(SubDirTip_MenuClosed != null) {
                SubDirTip_MenuClosed(sender, e);
            }
        }

        private void subDirTip_MenuItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            if(SubDirTip_MenuItemClicked != null) {
                SubDirTip_MenuItemClicked(sender, e);
            }
        }

        private void subDirTip_MenuItemRightClicked(object sender, ItemRightClickedEventArgs e) {
            if(SubDirTip_MenuItemRightClicked != null) {
                SubDirTip_MenuItemRightClicked(sender, e);
            }
        }

        private void subDirTip_MultipleMenuItemsClicked(object sender, EventArgs e) {
            if(SubDirTip_MultipleMenuItemsClicked != null) {
                SubDirTip_MultipleMenuItemsClicked(sender, e);
            }
        }

        private void subDirTip_MultipleMenuItemsRightClicked(object sender, ItemRightClickedEventArgs e) {
            if(SubDirTip_MultipleMenuItemsRightClicked != null) {
                SubDirTip_MultipleMenuItemsRightClicked(sender, e);
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
            if(Control.MouseButtons != MouseButtons.None) {
                //IntPtr tag = (IntPtr)this.timer_HoverSubDirTipMenu.Tag;

                int idx = GetHotItem();
                if(this.itemIndexDROPHILITED == idx) {
                    if(this.subDirTip != null) {
                        this.subDirTip.HideMenu();
                    }
                    // TODO: Check if the item is the Recycle Bin and deny if it is.
                    // string.Equals(wrapper.Path, "::{645FF040-5081-101B-9F08-00AA002F954E}"
                    if(this.ShowSubDirTip(this.itemIndexDROPHILITED, false, true)) {
                        if(this.hwndExplorer != IntPtr.Zero) {
                            WindowUtils.BringExplorerToFront(this.hwndExplorer);
                        }
                        PInvoke.SetFocus(ListViewController.Handle);
                        PInvoke.SetForegroundWindow(ListViewController.Handle);
                        HideThumbnailTooltip(-1);
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
    }
}
