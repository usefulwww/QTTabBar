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
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using BandObjectLib;
using QTPlugin;
using QTTabBarLib.Automation;
using QTTabBarLib.Interop;
using Timer = System.Windows.Forms.Timer;

namespace QTTabBarLib {
    public class ListViewWrapper : IDisposable {
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
        internal event ItemCountChangedHandler ItemCountChanged;  // TODO
        internal event MiddleClickHandler MiddleClick;            // OK
        internal event MouseActivateHandler MouseActivate;        // OK
        internal event SelectionChangedHandler SelectionChanged;  // OK
        internal event EventHandler SubDirTip_MenuClosed;
        internal event ToolStripItemClickedEventHandler SubDirTip_MenuItemClicked;
        internal event ItemRightClickedEventHandler SubDirTip_MenuItemRightClicked;
        internal event EventHandler SubDirTip_MultipleMenuItemsClicked;
        internal event ItemRightClickedEventHandler SubDirTip_MultipleMenuItemsRightClicked;
        #endregion

        private static readonly Int32 WM_AFTERPAINT = (Int32)PInvoke.RegisterWindowMessage("QTTabBar_AfterPaint");

        private AutomationManager AutoMan;
        private NativeWindowController ContainerController;
        private NativeWindowController EditController;
        private NativeWindowController ListViewController;
        private IShellBrowser ShellBrowser;
        private NativeWindowController ShellViewController;
        private bool fIFolderViewNotImplemented;
        private bool fIsSysListView;
        private bool fListViewHasFocus;
        private bool fThumbnailPending;
        private bool fTrackMouseEvent;
        private IntPtr hwndEnumResult;
        private IntPtr hwndExplorer;
        private IntPtr hwndShellContainer;
        private IntPtr hwndSubDirTipFocusOnMenu;
        private IntPtr hwndSubDirTipMessageReflect;
        private int iListViewItemState;
        private int itemIndexDROPHILITED = -1;
        private Point lastDragPoint;
        private Point lastLButtonPoint;
        private Int64 lastLButtonTime;
        private Point lastMouseMovePoint;
        private List<int> lstColumnFMT;
        private int subDirIndex = -1;
        private SubDirTipForm subDirTip;
        private int thumbnailIndex;
        private ThumbnailTooltipForm thumbnailTooltip;
        private Timer timer_HoverSubDirTipMenu;
        private Timer timer_HoverThumbnail;
        private Timer timer_Thumbnail;

        internal ListViewWrapper(IShellBrowser ShellBrowser, IntPtr hwndExplorer, IntPtr hwndSubDirTipMessageReflect, IntPtr hwndSubDirTipFocusOnMenu) {
            this.ShellBrowser = ShellBrowser;
            this.hwndExplorer = hwndExplorer;
            this.hwndSubDirTipMessageReflect = hwndSubDirTipMessageReflect;
            this.hwndSubDirTipFocusOnMenu = hwndSubDirTipFocusOnMenu;
            AutoMan = new AutomationManager();
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
            Initialize();
        }

        internal ListViewWrapper(IntPtr hwndShellView, IntPtr hwndSubDirTipMessageReflect, IntPtr hwndSubDirTipFocusOnMenu) {
            ShellBrowser = null;
            hwndExplorer = IntPtr.Zero;
            this.hwndSubDirTipMessageReflect = hwndSubDirTipMessageReflect;
            this.hwndSubDirTipFocusOnMenu = hwndSubDirTipFocusOnMenu;
            RecaptureHandles(hwndShellView);
        }

        #region IDisposable Members

        public void Dispose() {
            if(this.thumbnailTooltip != null) {
                this.thumbnailTooltip.Dispose();
                this.thumbnailTooltip = null;
            }
            if(this.subDirTip != null) {
                this.subDirTip.Dispose();
                this.subDirTip = null;
            }
            // TODO
        }

        #endregion

        private bool CallbackEnumChildProc_Container(IntPtr hwnd, IntPtr lParam) {
            if(GetWindowClassName(hwnd) == "ShellTabWindowClass") {
                hwndEnumResult = hwnd;
                return false;
            }
            return true;
        }

        private bool CallbackEnumChildProc_Edit(IntPtr hwnd, IntPtr lParam) {
            if(GetWindowClassName(hwnd) == "Edit") {
                hwndEnumResult = hwnd;
                return false;
            }
            return true;
        }

        private bool CallbackEnumChildProc_ListView(IntPtr hwnd, IntPtr lParam) {
            string name = GetWindowClassName(hwnd);
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
            if(GetWindowClassName(hwnd) == "SHELLDLL_DefView") {
                hwndEnumResult = hwnd;
                return false;
            }
            return true;
        }

        private bool ContainerController_MessageCaptured(ref Message msg) {
            if(msg.Msg == WM.PARENTNOTIFY && PInvoke.LoWord((int)msg.WParam) == WM.CREATE) {
                string name = GetWindowClassName(msg.LParam);
                if(name == "SHELLDLL_DefView") {
                    RecaptureHandles(msg.LParam);
                }
            }
            return false;
        }

        private bool EditController_MessageCaptured(ref Message msg) {
            if(msg.Msg == 0xb1 /* EM_SETSEL */ && msg.WParam.ToInt32() != -1) {
                msg.LParam = EditController.OptionalHandle;
                EditController.MessageCaptured -= EditController_MessageCaptured;
            }
            return false;
        }

        public int GetCurrentViewMode() {
            if(ShellBrowser == null) {
                return FVM.ICON;
            }
            IShellView ppshv = null;
            FOLDERSETTINGS lpfs = new FOLDERSETTINGS();
            try {
                if(this.ShellBrowser.QueryActiveShellView(out ppshv) == 0) {
                    ppshv.GetCurrentInfo(ref lpfs);
                }
            }
            finally {
                if(ppshv != null) {
                    Marshal.ReleaseComObject(ppshv);
                }
            }
            return lpfs.ViewMode;
        }

        public IntPtr GetEditControl() {
            if(ListViewController == null) {
                return IntPtr.Zero;
            }
            else if(fIsSysListView) {
                return PInvoke.SendMessage(ListViewController.Handle, LVM.GETEDITCONTROL, IntPtr.Zero, IntPtr.Zero);
            }
            else {
                hwndEnumResult = IntPtr.Zero;
                PInvoke.EnumChildWindows(ListViewController.Handle, CallbackEnumChildProc_Edit, IntPtr.Zero);
                return hwndEnumResult;
            }
        }

        public int GetFocusedItem() {
            if(!HasFocus()) {
                return -1;
            }

            if(fIsSysListView) {
                int count = GetItemCount();
                for(int i = 0; i < count; ++i) {
                    int state = (int)PInvoke.SendMessage(ListViewController.Handle, LVM.GETITEMSTATE, (IntPtr)i, (IntPtr)LVIS.FOCUSED);
                    if(state != 0) {
                        return i;
                    }
                }
                return -1;
            }
            else {
                return AutoMan.DoQuery(factory => {
                    AutomationElement elem = factory.FromKeyboardFocus();
                    return elem == null ? -1 : elem.GetItemIndex();
                });
            }
        }

        public Rectangle GetFocusedItemRect() {
            if(HasFocus()) {
                if(fIsSysListView) {
                    int i = GetFocusedItem();
                    int fvm = GetCurrentViewMode();
                    return GetLVITEMRECT(ListViewController.Handle, i, false, fvm).ToRectangle();
                }
                else {
                    return AutoMan.DoQuery(factory => {
                        AutomationElement elem = factory.FromKeyboardFocus();
                        return elem == null ? new Rectangle(0, 0, 0, 0) : elem.GetBoundingRect();
                    });
                }
            }
            return new Rectangle(0, 0, 0, 0);
        }

        public int GetHotItem() {
            return HitTest(Control.MousePosition, true);
        }

        public int GetItemCount() {
            if(ListViewController == null) {
                return 0;
            }
            else if(fIsSysListView) {
                return (int)PInvoke.SendMessage(ListViewController.Handle, LVM.GETITEMCOUNT, IntPtr.Zero, IntPtr.Zero);
            }
            else {
                return AutoMan.DoQuery(factory => {
                    AutomationElement elem = factory.FromHandle(ListViewController.Handle);
                    return elem == null ? 0 : elem.GetItemCount();
                });
            }
        }

        // Try not to use this, ever!

        private RECT GetLVITEMRECT(IntPtr hwnd, int iItem, bool fSubDirTip, int fvm) {
            int code;
            bool flag = false;
            bool flag2 = false;
            if(fSubDirTip) {
                switch(fvm) {
                    case FVM.ICON:
                        flag = !QTUtility.IsVista;
                        code = LVIR.ICON;
                        break;

                    case FVM.DETAILS:
                        code = LVIR.LABEL;
                        break;

                    case FVM.LIST:
                        if(QTUtility.IsVista) {
                            code = LVIR.LABEL;
                        }
                        else {
                            flag2 = true;
                            code = LVIR.ICON;
                        }
                        break;

                    case FVM.TILE:
                        code = LVIR.ICON;
                        break;

                    default:
                        code = LVIR.BOUNDS;
                        break;
                }
            }
            else {
                code = (fvm == FVM.DETAILS) ? LVIR.LABEL : LVIR.BOUNDS;
            }

            RECT rect = new RECT();
            rect.left = code;
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(rect));
            Marshal.StructureToPtr(rect, ptr, false);
            PInvoke.SendMessage(ListViewController.Handle, LVM.GETITEMRECT, (IntPtr)iItem, ptr);
            rect = (RECT)Marshal.PtrToStructure(ptr, typeof(RECT));
            Marshal.FreeHGlobal(ptr);
            PInvoke.MapWindowPoints(ListViewController.Handle, IntPtr.Zero, ref rect, 2);

            if(flag) {
                if((fvm == FVM.THUMBNAIL) || (fvm == FVM.THUMBSTRIP)) {
                    rect.right -= 13;
                    return rect;
                }
                int num3 = (int)PInvoke.SendMessage(hwnd, LVM.GETITEMSPACING, IntPtr.Zero, IntPtr.Zero);
                Size iconSize = SystemInformation.IconSize;
                rect.right = ((rect.left + (((num3 & 0xffff) - iconSize.Width) / 2)) + iconSize.Width) + 8;
                rect.bottom = (rect.top + iconSize.Height) + 6;
                return rect;
            }
            if(flag2) {
                LVITEM structure = new LVITEM();
                structure.pszText = Marshal.AllocHGlobal(520);
                structure.cchTextMax = 260;
                structure.iItem = iItem;
                structure.mask = 1;
                IntPtr zero = Marshal.AllocHGlobal(Marshal.SizeOf(structure));
                Marshal.StructureToPtr(structure, zero, false);
                PInvoke.SendMessage(hwnd, LVM.GETITEM, IntPtr.Zero, zero);
                int num4 = (int)PInvoke.SendMessage(hwnd, LVM.GETSTRINGWIDTH, IntPtr.Zero, structure.pszText);
                num4 += 20;
                Marshal.FreeHGlobal(structure.pszText);
                Marshal.FreeHGlobal(zero);
                rect.right += num4;
                rect.top += 2;
                rect.bottom += 2;
            }
            return rect;
        }

        public IntPtr GetListViewHandle() {
            return (ListViewController == null) ? IntPtr.Zero : ListViewController.Handle;
        }

        public int GetSelectedCount() {
            if(ListViewController == null) {
                return 0;
            }
            else if(fIsSysListView) {
                return (int)PInvoke.SendMessage(ListViewController.Handle, LVM.GETSELECTEDCOUNT, IntPtr.Zero, IntPtr.Zero);
            }
            else {
                return AutoMan.DoQuery(factory => {
                    AutomationElement elem = factory.FromHandle(ListViewController.Handle);
                    return elem == null ? 0 : elem.GetSelectedCount();
                });
            }
        }

        public Point GetSubDirTipPoint(bool fByKey) {
            int viewMode = GetCurrentViewMode();
            if(fIsSysListView) {
                int iItem = fByKey ? GetFocusedItem() : GetHotItem();
                RECT rect = GetLVITEMRECT(ListViewController.Handle, iItem, true, viewMode);
                return new Point(rect.right - 16, rect.bottom - 16);
            }
            else {
                return AutoMan.DoQuery(factory => {
                    AutomationElement elem = fByKey ?
                        factory.FromKeyboardFocus() :
                        ListItemElementFromPoint(factory, Control.MousePosition);

                    if(elem == null) return new Point(0, 0);

                    Rectangle rect = elem.GetBoundingRect();
                    int x, y;
                    switch(viewMode) {
                        case FVM.CONTENT:
                            y = rect.Bottom;
                            elem = elem.FindMatchingChild(child => 
                                    child.GetAutomationId() == "System.ItemNameDisplay");
                            if(elem == null) return new Point(0, 0);
                            x = elem.GetBoundingRect().Left;
                            return new Point(x, y - 16);

                        case FVM.DETAILS:
                            elem = elem.FindMatchingChild(child => 
                                    child.GetAutomationId() == "System.ItemNameDisplay");
                            if(elem == null) return new Point(0, 0);
                            rect = elem.GetBoundingRect();
                            x = rect.Right;
                            y = rect.Top;
                            y += (rect.Bottom - y) / 2;
                            return new Point(x - 16, y - 8);

                        case FVM.SMALLICON:
                            x = rect.Right;
                            y = rect.Top;
                            x -= (rect.Bottom - y) / 2;
                            y += (rect.Bottom - y) / 2;
                            return new Point(x - 8, y - 8);

                        case FVM.TILE:
                            y = rect.Bottom;
                            elem = elem.FindMatchingChild(child => 
                                    child.GetClassName() == "UIImage");
                            if(elem == null) return new Point(0, 0);
                            x = elem.GetBoundingRect().Right;
                            return new Point(x - 16, y - 16);

                        case FVM.THUMBSTRIP:
                        case FVM.THUMBNAIL:
                        case FVM.ICON:
                            x = rect.Right;
                            elem = elem.FindMatchingChild(child => 
                                    child.GetClassName() == "UIImage");
                            if(elem == null) return new Point(0, 0);
                            y = elem.GetBoundingRect().Bottom;
                            return new Point(x - 16, y - 16);

                        case FVM.LIST:
                        default:
                            x = rect.Right;
                            y = rect.Bottom;
                            return new Point(x, y - 15);
                    }
                });
            }
        }

        private static string GetWindowClassName(IntPtr hwnd) {
            StringBuilder lpClassName = new StringBuilder(260);
            PInvoke.GetClassName(hwnd, lpClassName, lpClassName.Capacity);
            return lpClassName.ToString();
        }

        private void HandleDropHilighted(int iItem) {
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

        public void HandleF2() {
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

        private bool HandleGetInfoTip(int iItem, bool byKey) {
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

        private void HandleHotTrack(int iItem) {
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

        private bool HandleKeyDown(Keys key) {
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

        private bool HandleLVCUSTOMDRAW(ref Message msg) {
            // TODO this needs to be cleaned
            if(QTUtility.CheckConfig(Settings.AlternateRowColors) && (GetCurrentViewMode() == FVM.DETAILS)) {
                NMLVCUSTOMDRAW structure = (NMLVCUSTOMDRAW)Marshal.PtrToStructure(msg.LParam, typeof(NMLVCUSTOMDRAW));
                int dwItemSpec = (int)structure.nmcd.dwItemSpec;
                switch(structure.nmcd.dwDrawStage) {
                    case CDDS.SUBITEM | CDDS.ITEMPREPAINT:
                        iListViewItemState = (int)PInvoke.SendMessage(
                                ListViewController.Handle, LVM.GETITEMSTATE, (IntPtr)dwItemSpec,
                                (IntPtr)(LVIS.FOCUSED | LVIS.SELECTED | LVIS.DROPHILITED));

                        if(QTUtility.IsVista) {
                            int num4 = lstColumnFMT[structure.iSubItem];
                            structure.clrTextBk = QTUtility.ShellViewRowCOLORREF_Background;
                            structure.clrText = QTUtility.ShellViewRowCOLORREF_Text;
                            Marshal.StructureToPtr(structure, msg.LParam, false);
                            bool drawingHotItem = (dwItemSpec == GetHotItem());
                            bool fullRowSel = !QTUtility.CheckConfig(Settings.ToggleFullRowSelect);

                            msg.Result = (IntPtr)(CDRF.NEWFONT);
                            if(structure.iSubItem == 0 && !drawingHotItem) {
                                if(iListViewItemState == 0 && (num4 & 0x600) != 0) {
                                    msg.Result = (IntPtr)(CDRF.NEWFONT | CDRF.NOTIFYPOSTPAINT);
                                }
                                else if(iListViewItemState == LVIS.FOCUSED && !fullRowSel) {
                                    msg.Result = (IntPtr)(CDRF.NEWFONT | CDRF.NOTIFYPOSTPAINT);
                                }
                            }

                            if(structure.iSubItem > 0 && (!fullRowSel || !drawingHotItem)) {
                                if(!fullRowSel || (iListViewItemState & (LVIS.SELECTED | LVIS.DROPHILITED)) == 0) {
                                    using(Graphics graphics = Graphics.FromHdc(structure.nmcd.hdc)) {
                                        if(QTUtility.sbAlternate == null) {
                                            QTUtility.sbAlternate = new SolidBrush(QTUtility2.MakeColor(QTUtility.ShellViewRowCOLORREF_Background));
                                        }
                                        graphics.FillRectangle(QTUtility.sbAlternate, structure.nmcd.rc.ToRectangle());
                                    }
                                }
                            }
                        }
                        else {
                            msg.Result = (IntPtr)(CDRF.NOTIFYPOSTPAINT);
                        }
                        return true;

                    case CDDS.SUBITEM | CDDS.ITEMPOSTPAINT: {
                        RECT rc = structure.nmcd.rc;
                        if(!QTUtility.IsVista) {
                            rc = PInvoke.ListView_GetItemRect(ListViewController.Handle, dwItemSpec, structure.iSubItem, 2);
                        }
                        else {
                            rc.left += 0x10;
                        }
                        bool flag3 = false;
                        bool flag4 = false;
                        bool flag5 = QTUtility.CheckConfig(Settings.DetailsGridLines);
                        bool flag6 = QTUtility.CheckConfig(Settings.ToggleFullRowSelect) ^ QTUtility.IsVista;
                        bool flag7 = false;
                        if(!QTUtility.IsVista && QTUtility.fSingleClick) {
                            flag7 = (dwItemSpec == GetHotItem());
                        }
                        LVITEM lvitem = new LVITEM();
                        lvitem.pszText = Marshal.AllocHGlobal(520);
                        lvitem.cchTextMax = 260;
                        lvitem.iSubItem = structure.iSubItem;
                        lvitem.iItem = dwItemSpec;
                        lvitem.mask = 1;
                        IntPtr ptr3 = Marshal.AllocHGlobal(Marshal.SizeOf(lvitem));
                        Marshal.StructureToPtr(lvitem, ptr3, false);
                        PInvoke.SendMessage(ListViewController.Handle, LVM.GETITEM, IntPtr.Zero, ptr3);
                        if(QTUtility.sbAlternate == null) {
                            QTUtility.sbAlternate = new SolidBrush(QTUtility2.MakeColor(QTUtility.ShellViewRowCOLORREF_Background));
                        }
                        using(Graphics graphics2 = Graphics.FromHdc(structure.nmcd.hdc)) {
                            Rectangle rect = rc.ToRectangle();
                            if(flag5) {
                                rect = new Rectangle(rc.left + 1, rc.top, rc.Width - 1, rc.Height - 1);
                            }
                            graphics2.FillRectangle(QTUtility.sbAlternate, rect);
                            if(!QTUtility.IsVista && ((structure.iSubItem == 0) || flag6)) {
                                flag4 = (iListViewItemState & 8) == 8;
                                if((iListViewItemState != 0) && (((iListViewItemState == 1) && fListViewHasFocus) || (iListViewItemState != 1))) {
                                    int width;
                                    if(flag6) {
                                        width = rc.Width;
                                    }
                                    else {
                                        width = 8 + ((int)PInvoke.SendMessage(ListViewController.Handle, LVM.GETSTRINGWIDTH, IntPtr.Zero, lvitem.pszText));
                                        if(width > rc.Width) {
                                            width = rc.Width;
                                        }
                                    }
                                    Rectangle rectangle2 = new Rectangle(rc.left, rc.top, width, flag5 ? (rc.Height - 1) : rc.Height);
                                    if(((iListViewItemState & 2) == 2) || flag4) {
                                        if(flag4) {
                                            graphics2.FillRectangle(SystemBrushes.Highlight, rectangle2);
                                        }
                                        else if(QTUtility.fSingleClick && flag7) {
                                            graphics2.FillRectangle(fListViewHasFocus ? SystemBrushes.HotTrack : SystemBrushes.Control, rectangle2);
                                        }
                                        else {
                                            graphics2.FillRectangle(fListViewHasFocus ? SystemBrushes.Highlight : SystemBrushes.Control, rectangle2);
                                        }
                                        flag3 = true;
                                    }
                                    if((fListViewHasFocus && ((iListViewItemState & 1) == 1)) && !flag6) {
                                        ControlPaint.DrawFocusRectangle(graphics2, rectangle2);
                                    }
                                }
                            }
                            if(QTUtility.IsVista && ((iListViewItemState & 1) == 1)) {
                                int num6 = rc.Width;
                                if(!flag6) {
                                    num6 = 4 + ((int)PInvoke.SendMessage(ListViewController.Handle, LVM.GETSTRINGWIDTH, IntPtr.Zero, lvitem.pszText));
                                    if(num6 > rc.Width) {
                                        num6 = rc.Width;
                                    }
                                }
                                Rectangle rectangle = new Rectangle(rc.left + 1, rc.top + 1, num6, flag5 ? (rc.Height - 2) : (rc.Height - 1));
                                ControlPaint.DrawFocusRectangle(graphics2, rectangle);
                            }
                        }
                        IntPtr zero = IntPtr.Zero;
                        IntPtr hgdiobj = IntPtr.Zero;
                        if(!QTUtility.IsVista && QTUtility.fSingleClick) {
                            LOGFONT logfont;
                            zero = PInvoke.GetCurrentObject(structure.nmcd.hdc, 6);
                            PInvoke.GetObject(zero, Marshal.SizeOf(typeof(LOGFONT)), out logfont);
                            if((structure.iSubItem == 0) || flag6) {
                                logfont.lfUnderline = ((QTUtility.iIconUnderLineVal == 3) || flag7) ? ((byte)1) : ((byte)0);
                            }
                            else {
                                logfont.lfUnderline = 0;
                            }
                            hgdiobj = PInvoke.CreateFontIndirect(ref logfont);
                            PInvoke.SelectObject(structure.nmcd.hdc, hgdiobj);
                        }
                        PInvoke.SetBkMode(structure.nmcd.hdc, 1);
                        int dwDTFormat = 0x8824;
                        if(QTUtility.IsRTL ? ((lstColumnFMT[structure.iSubItem] & 1) == 0) : ((lstColumnFMT[structure.iSubItem] & 1) == 1)) {
                            if(QTUtility.IsRTL) {
                                dwDTFormat &= -3;
                            }
                            else {
                                dwDTFormat |= 2;
                            }
                            rc.right -= 6;
                        }
                        else if(structure.iSubItem == 0) {
                            rc.left += 2;
                            rc.right -= 2;
                        }
                        else {
                            rc.left += 6;
                        }
                        if(flag3) {
                            PInvoke.SetTextColor(structure.nmcd.hdc, QTUtility2.MakeCOLORREF((fListViewHasFocus || flag4) ? SystemColors.HighlightText : SystemColors.WindowText));
                        }
                        else {
                            PInvoke.SetTextColor(structure.nmcd.hdc, QTUtility.ShellViewRowCOLORREF_Text);
                        }
                        PInvoke.DrawTextExW(structure.nmcd.hdc, lvitem.pszText, -1, ref rc, dwDTFormat, IntPtr.Zero);
                        Marshal.FreeHGlobal(lvitem.pszText);
                        Marshal.FreeHGlobal(ptr3);
                        msg.Result = IntPtr.Zero;
                        if(zero != IntPtr.Zero) {
                            PInvoke.SelectObject(structure.nmcd.hdc, zero);
                        }
                        if(hgdiobj != IntPtr.Zero) {
                            PInvoke.DeleteObject(hgdiobj);
                        }
                        return true;
                    }
                    case CDDS.ITEMPREPAINT:
                        if((dwItemSpec % 2) == 0) {
                            msg.Result = (IntPtr)0x20;
                            return true;
                        }
                        msg.Result = IntPtr.Zero;
                        return false;

                    case CDDS.PREPAINT: {
                        HDITEM hditem = new HDITEM();
                        hditem.mask = 4;
                        IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(hditem));
                        Marshal.StructureToPtr(hditem, ptr, false);
                        IntPtr hWnd = PInvoke.SendMessage(ListViewController.Handle, LVM.GETHEADER, IntPtr.Zero, IntPtr.Zero);
                        int num2 = (int)PInvoke.SendMessage(hWnd, 0x1200, IntPtr.Zero, IntPtr.Zero);
                        if(lstColumnFMT == null) {
                            lstColumnFMT = new List<int>();
                        }
                        else {
                            lstColumnFMT.Clear();
                        }
                        for(int i = 0; i < num2; i++) {
                            PInvoke.SendMessage(hWnd, 0x120b, (IntPtr)i, ptr);
                            hditem = (HDITEM)Marshal.PtrToStructure(ptr, typeof(HDITEM));
                            lstColumnFMT.Add(hditem.fmt);
                        }
                        Marshal.FreeHGlobal(ptr);
                        fListViewHasFocus = ListViewController.Handle == PInvoke.GetFocus();
                        msg.Result = (IntPtr)0x20;
                        return true;
                    }
                }
            }
            return false;
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

        private void HandleRenaming(IntPtr pIDL) {
            StringBuilder pszPath = new StringBuilder(260);
            if(!PInvoke.SHGetPathFromIDList(pIDL, pszPath) || (pszPath.Length <= 0)) return;
            string path = pszPath.ToString();
            if(Directory.Exists(path)) return;
            if(File.Exists(path)) {
                string extension = Path.GetExtension(path);
                if(!string.IsNullOrEmpty(extension) && (extension.Equals(".lnk", StringComparison.OrdinalIgnoreCase) || extension.Equals(".url", StringComparison.OrdinalIgnoreCase))) {
                    return;
                }
            }
            IntPtr hWnd = GetEditControl();
            if(hWnd == IntPtr.Zero) return;

            IntPtr lParam = Marshal.AllocHGlobal(520);
            if((int)PInvoke.SendMessage(hWnd, WM.GETTEXT, (IntPtr)260, lParam) > 0) {
                string str3 = Marshal.PtrToStringUni(lParam);
                if(str3.Length > 2) {
                    int num = str3.LastIndexOf(".");
                    if(num > 0) {
                        // Explorer will send the EM_SETSEL message to select the
                        // entire filename.  We will intercept this message and
                        // change the params to select only the part before the
                        // extension.
                        EditController = new NativeWindowController(hWnd);
                        EditController.OptionalHandle = (IntPtr)num;
                        EditController.MessageCaptured += EditController_MessageCaptured;
                    }
                }
            }
            Marshal.FreeHGlobal(lParam);
        }

        public bool HasFocus() {
            return (ListViewController != null &&
                PInvoke.GetFocus() == ListViewController.Handle);
        }

        public void HideSubDirTip(int iReason) {
            if((this.subDirTip != null) && this.subDirTip.IsShowing) {
                bool fForce = iReason < 0;
                if(fForce || !this.subDirTip.IsShownByKey) {
                    this.subDirTip.HideSubDirTip(fForce);
                    this.subDirIndex = -1;
                }
            }
            this.itemIndexDROPHILITED = -1;
        }

        public void HideSubDirTipMenu() {
            if(subDirTip != null) {
                subDirTip.HideMenu();
            }
        }

        public void HideSubDirTip_ExplorerInactivated() {
            if((this.subDirTip != null) && this.subDirTip.IsShowing) {
                this.subDirTip.OnExplorerInactivated();
            }
        }

        public void HideThumbnailTooltip(int iReason) {
            if((this.thumbnailTooltip != null) && this.thumbnailTooltip.IsShowing) {
                if(((iReason == 0) || (iReason == 7)) || (iReason == 9)) {
                    this.thumbnailTooltip.IsShownByKey = false;
                }
                if(this.thumbnailTooltip.HideToolTip()) {
                    this.thumbnailIndex = -1;
                }
            }
        }

        public int HitTest(IntPtr LParam) {
            return HitTest(new Point(QTUtility2.GET_X_LPARAM(LParam), QTUtility2.GET_Y_LPARAM(LParam)), false);
        }

        public int HitTest(Point pt, bool ScreenCoords) {
            if(ListViewController == null) {
                return -1;
            }
            else if(fIsSysListView) {
                if(ScreenCoords) {
                    PInvoke.ScreenToClient(ListViewController.Handle, ref pt);
                }
                LVHITTESTINFO structure = new LVHITTESTINFO();
                structure.pt.x = pt.X;
                structure.pt.y = pt.Y;
                IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(structure));
                Marshal.StructureToPtr(structure, ptr, false);
                int num = (int)PInvoke.SendMessage(ListViewController.Handle, LVM.HITTEST, IntPtr.Zero, ptr);
                Marshal.FreeHGlobal(ptr);
                return num;
            }
            else {
                if(!ScreenCoords) {
                    PInvoke.ClientToScreen(ListViewController.Handle, ref pt);
                }
                else if(PInvoke.WindowFromPoint(pt) != ListViewController.Handle) {
                    return -1;
                }
                return AutoMan.DoQuery(factory => {
                    AutomationElement elem = ListItemElementFromPoint(factory, pt);
                    return elem == null ? -1 : elem.GetItemIndex();
                });
            }
        }

        public bool HotItemIsSelected() {
            if(ListViewController == null) {
                return false;
            }
            else if(fIsSysListView) {
                int hot = (int)PInvoke.SendMessage(ListViewController.Handle, LVM.GETHOTITEM, IntPtr.Zero, IntPtr.Zero);
                if(hot == -1) return false;
                int state = (int)PInvoke.SendMessage(ListViewController.Handle, LVM.GETITEMSTATE, (IntPtr)hot, (IntPtr)LVIS.SELECTED);
                return ((state & LVIS.SELECTED) != 0);
            }
            else {
                return AutoMan.DoQuery(factory => {
                    AutomationElement elem = ListItemElementFromPoint(factory, Control.MousePosition);
                    return elem == null ? false : elem.IsSelected();
                });
            }
        }

        public void Initialize() {
            if(ShellViewController != null) {
                ShellViewController.ReleaseHandle();
                ShellViewController = null;
            }
            if(ListViewController != null) {
                ListViewController.ReleaseHandle();
                ListViewController = null;
            }

            hwndEnumResult = IntPtr.Zero;
            PInvoke.EnumChildWindows(hwndExplorer, CallbackEnumChildProc_ShellView, IntPtr.Zero);
            if(hwndEnumResult != IntPtr.Zero) {
                RecaptureHandles(hwndEnumResult);
            }
        }

        // If the ListView is in Details mode, returns true only if the mouse
        // is over the ItemName column.  Returns true always for any other mode.
        // This function only returns valid results if the mouse is known to be
        // over an item.  Otherwise, its return value is undefined.
        public bool IsTrackingItemName() {
            if(ListViewController == null) {
                return false;
            }
            
            if(GetCurrentViewMode() != FVM.DETAILS) return true;
            if(fIsSysListView) {
                if(GetItemCount() == 0) return false;
                RECT rect = PInvoke.ListView_GetItemRect(ListViewController.Handle, 0, 0, 2);
                Point mousePosition = Control.MousePosition;
                PInvoke.MapWindowPoints(IntPtr.Zero, ListViewController.Handle, ref mousePosition, 1);
                return (Math.Min(rect.left, rect.right) <= mousePosition.X) && (mousePosition.X <= Math.Max(rect.left, rect.right));
            }
            else {
                return AutoMan.DoQuery(factory => {
                    AutomationElement elem = factory.FromPoint(Control.MousePosition);
                    return elem == null ? false : elem.GetAutomationId() == "System.ItemNameDisplay";
                });
            }
        }

        private AutomationElement ListItemElementFromPoint(AutomationElementFactory factory, Point pt) {
            if(PInvoke.WindowFromPoint(pt) != ListViewController.Handle) return null;
            AutomationElement elem = factory.FromPoint(pt);
            if(elem == null) return null;
            if(elem.GetClassName() == "UIItem") return elem;
            elem = elem.GetParent();
            if(elem == null) return null;
            if(elem.GetClassName() == "UIItem") return elem;
            return null;
        }

        private bool ListViewController_MessageCaptured(ref Message msg) {

            // First block is for both SysListView and ItemsView
            if(msg.Msg == WM_AFTERPAINT) {
                RefreshSubDirTip(true);

                // I'm putting this here just to make filetools work.
                // TODO: work out a better solution later.
                if(!fIsSysListView && SelectionChanged != null) {
                    SelectionChanged();
                }
                return true;
            }

            switch(msg.Msg) {
                case WM.DESTROY:
                    this.HideThumbnailTooltip(7);
                    this.HideSubDirTip(7);
                    if(this.timer_HoverThumbnail != null) {
                        this.timer_HoverThumbnail.Enabled = false;
                    }
                    ListViewController.ReleaseHandle();
                    ListViewController = null;
                    break;

                case WM.PAINT:
                    // It's very dangerous to do automation-related things
                    // during WM_PAINT.  So, use PostMessage to do it later.
                    PInvoke.PostMessage(ListViewController.Handle, (uint)WM_AFTERPAINT, IntPtr.Zero, IntPtr.Zero);
                    break;

                case WM.SETREDRAW:
                    if(msg.WParam != IntPtr.Zero) {
                        SetSLV32StyleFlags();
                    }
                    break;

                case WM.MOUSEMOVE:
                    if(this.fTrackMouseEvent) {
                        this.fTrackMouseEvent = false;
                        TRACKMOUSEEVENT structure = new TRACKMOUSEEVENT();
                        structure.cbSize = Marshal.SizeOf(structure);
                        structure.dwFlags = 2;
                        structure.hwndTrack = msg.HWnd;
                        PInvoke.TrackMouseEvent(ref structure);
                    }
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

            // From here on down is only for the ItemsView.
            if(fIsSysListView) {
                return false;
            }

            switch(msg.Msg) {
                case LVM.SCROLL: {
                    int amount = msg.WParam.ToInt32();
                    SetRedraw(false);
                    AutoMan.DoQuery(factory => {
                        AutomationElement elem = factory.FromHandle(ListViewController.Handle);
                        amount /= SystemInformation.MouseWheelScrollDelta;
                        bool dec = amount < 0;
                        if(dec) {
                            amount = -amount;
                        }
                        int lines = SystemInformation.MouseWheelScrollLines;
                        if(lines < 0) {
                            elem.ScrollHorizontal(dec
                                    ? ScrollAmount.LargeDecrement
                                    : ScrollAmount.LargeIncrement, amount);
                        }
                        else {
                            elem.ScrollHorizontal(dec
                                    ? ScrollAmount.SmallDecrement
                                    : ScrollAmount.SmallIncrement, amount * lines);
                        }
                        return 0;
                    });
                    SetRedraw(true);
                    return true;
                }

                case WM.MOUSEACTIVATE:
                    if(MouseActivate != null) {
                        int res = (int)msg.Result;
                        bool ret = MouseActivate(ref res);
                        msg.Result = (IntPtr)res;
                        return ret;
                    }
                    break;

                case WM.LBUTTONDOWN:
                    // The ItemsView's window class doesn't have the CS_DBLCLKS
                    // class style, which means we won't be receiving the
                    // WM.LBUTTONDBLCLK message.  We'll just have to make do
                    // without...
                    if(DoubleClick != null || ItemActivated != null) {
                        Int64 now = DateTime.Now.Ticks;
                        Point pt = new Point(
                            QTUtility2.GET_X_LPARAM(msg.LParam),
                            QTUtility2.GET_Y_LPARAM(msg.LParam));
                        if((now - lastLButtonTime) / 10000 <= SystemInformation.DoubleClickTime) {
                            Size size = SystemInformation.DoubleClickSize;
                            if(Math.Abs(pt.X - lastLButtonPoint.X) <= size.Width) {
                                if(Math.Abs(pt.Y - lastLButtonPoint.Y) <= size.Height) {
                                    lastLButtonTime = 0;
                                    if(DoubleClick != null && DoubleClick(pt)) {
                                        return true;
                                    }
                                    if(ItemActivated != null) {
                                        if(HitTest(pt, false) > -1) {
                                            // Explorer includes an option to make
                                            // single-clicking activate items.
                                            // TODO: Support that.
                                            if(ItemActivated(Control.ModifierKeys)) {
                                                return true;
                                            }
                                        }
                                    }
                                    return false;
                                }
                            }
                        }
                        lastLButtonPoint = pt;
                        lastLButtonTime = now;
                    }
                    return false;

                case WM.MOUSEMOVE: {
                        Point pt = new Point(QTUtility2.GET_X_LPARAM(msg.LParam), QTUtility2.GET_Y_LPARAM(msg.LParam));
                        if(pt != lastMouseMovePoint) {
                            lastMouseMovePoint = pt;
                            if(QTUtility.CheckConfig(Settings.ShowTooltipPreviews) || !QTUtility.CheckConfig(Settings.NoShowSubDirTips)) {
                                HandleHotTrack(HitTest(pt, false));
                            }
                        }
                    }
                    break;

                case WM.KEYDOWN:
                    if(HandleKeyDown((Keys)msg.WParam)) return true;
                    if((Keys)msg.WParam == Keys.Enter && ItemActivated != null) {
                        return ItemActivated(Control.ModifierKeys);
                    }
                    break;

                case WM.USER + 209: { // This message appears to control dragging.
                        Point pt = new Point((int)msg.WParam, (int)msg.LParam);
                        if(pt == lastDragPoint) {
                            return false;
                        }
                        lastDragPoint = pt;
                        HandleDropHilighted(HitTest(pt, false));
                    }
                    break;
                
                case WM.NOTIFY: {
                        NMHDR nmhdr = (NMHDR)Marshal.PtrToStructure(msg.LParam, typeof(NMHDR));
                        if(nmhdr.code == -530 /* TTN_NEEDTEXT */) {
                            NMTTDISPINFO dispinfo = (NMTTDISPINFO)Marshal.PtrToStructure(msg.LParam, typeof(NMTTDISPINFO));
                            if((dispinfo.uFlags & 0x20 /* TTF_TRACK */) != 0) {
                                return HandleGetInfoTip(GetFocusedItem(), true);
                            }
                            else {
                                int i = GetHotItem();
                                if(i != -1 && IsTrackingItemName()) {
                                    return HandleGetInfoTip(i, false);
                                }
                            }
                        }
                    }
                    break;
            }
            return false;
        }

        public bool MouseIsOverListView() {
            return (ListViewController != null &&
                PInvoke.WindowFromPoint(Control.MousePosition) == ListViewController.Handle);
        }

        public bool PointIsBackground(Point pt, bool screenCoords) {
            if(fIsSysListView) {
                return HitTest(pt, screenCoords) == -1;
            }
            else {
                if(ListViewController == null) {
                    return false;
                }
                if(!screenCoords) {
                    PInvoke.ClientToScreen(ListViewController.Handle, ref pt);
                }
                return AutoMan.DoQuery(factory => {
                    AutomationElement elem = factory.FromPoint(pt);
                    if(elem == null) return false;
                    string className = elem.GetClassName();
                    return className == "UIItemsView" || className == "UIGroupItem";
                });
            }
        }

        void RecaptureHandles(IntPtr hwndShellView) {
            if(ShellViewController != null) {
                ShellViewController.ReleaseHandle();
                ShellViewController = null;
            }
            if(ListViewController != null) {
                ListViewController.ReleaseHandle();
                ListViewController = null;
            }

            ShellViewController = new NativeWindowController(hwndShellView);
            ShellViewController.MessageCaptured += ShellViewController_MessageCaptured;

            hwndEnumResult = IntPtr.Zero;
            PInvoke.EnumChildWindows(hwndShellView, CallbackEnumChildProc_ListView, IntPtr.Zero);
            if(hwndEnumResult == IntPtr.Zero) {
                return;
            }

            ListViewController = new NativeWindowController(hwndEnumResult);
            ListViewController.MessageCaptured += ListViewController_MessageCaptured;

            SetSLV32StyleFlags();
            this.fTrackMouseEvent = false;
            TRACKMOUSEEVENT structure = new TRACKMOUSEEVENT();
            structure.cbSize = Marshal.SizeOf(structure);
            structure.dwFlags = 2;
            structure.hwndTrack = ListViewController.Handle;
            PInvoke.TrackMouseEvent(ref structure);

            // TODO: Initialize stuff here
            this.subDirIndex = this.thumbnailIndex = this.itemIndexDROPHILITED = -1;
        }

        public void RefreshSubDirTip(bool force = false) {
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

        public void ScrollHorizontal(int amount) {
            if(ListViewController != null) {
                // We'll intercept this message later for the ItemsView.  It's
                // important to use PostMessage here to prevent reentry issues
                // with the Automation Thread.
                PInvoke.PostMessage(ListViewController.Handle, LVM.SCROLL, (IntPtr)(-amount), IntPtr.Zero);
            }
        }

        public void SetFocus() {
            if(ListViewController != null) {
                PInvoke.SetFocus(ListViewController.Handle);
            }
        }

        public void SetRedraw(bool redraw) {
            if(ListViewController != null) {
                PInvoke.SetRedraw(ListViewController.Handle, redraw);
            }
        }

        private void SetSLV32StyleFlags() {
            if(!fIsSysListView || GetCurrentViewMode() != FVM.DETAILS) return;
            uint flags = 0;
            if(QTUtility.CheckConfig(Settings.DetailsGridLines)) {
                flags |= LVS_EX.GRIDLINES;
            }
            else {
                flags &= ~LVS_EX.GRIDLINES;
            }
            if(QTUtility.CheckConfig(Settings.ToggleFullRowSelect) ^ QTUtility.IsVista) {
                flags |= LVS_EX.FULLROWSELECT;
            }
            else {
                flags &= ~LVS_EX.FULLROWSELECT;
            }
            const uint mask = LVS_EX.GRIDLINES | LVS_EX.FULLROWSELECT;
            PInvoke.SendMessage(ListViewController.Handle, LVM.SETEXTENDEDLISTVIEWSTYLE, (IntPtr)mask, (IntPtr)flags);
        }

        private bool ShellViewController_MessageCaptured(ref Message msg) {
            if(msg.Msg == WM.MOUSEACTIVATE) {
                if(MouseActivate != null) {
                    int res = (int)msg.Result;
                    bool ret = MouseActivate(ref res);
                    msg.Result = (IntPtr)res;
                    return ret;
                }
            }
            else if(msg.Msg != WM.NOTIFY) {
                return false;
            }

            NMHDR nmhdr = (NMHDR)Marshal.PtrToStructure(msg.LParam, typeof(NMHDR));
            if(nmhdr.hwndFrom != ListViewController.Handle) {
                if(((nmhdr.code == -12 /*NM_CUSTOMDRAW*/) && (nmhdr.idFrom == IntPtr.Zero)) && fTrackMouseEvent) {
                    fTrackMouseEvent = false;
                    TRACKMOUSEEVENT structure = new TRACKMOUSEEVENT();
                    structure.cbSize = Marshal.SizeOf(structure);
                    structure.dwFlags = 2; /*TME_LEAVE*/
                    structure.hwndTrack = ListViewController.Handle;
                    PInvoke.TrackMouseEvent(ref structure);
                }
                return false;
            }

            // Process WM.NOTIFY.  These are all notifications from the 
            // SysListView32 control.  We will not get ANY of these on 
            // Windows 7, which means every single one of them has to 
            // have an alternative somewhere for the non-SysListView32,
            // or it's not going to happen.
            switch(nmhdr.code) {
                case -12: // NM_CUSTOMDRAW
                    // This is for drawing alternating row colors.  I doubt
                    // very much we'll find an alternative for this...
                    return HandleLVCUSTOMDRAW(ref msg);

                case LVN.ITEMCHANGED: {
                    // There are three things happening here.
                    // 1. Notify plugins of selection changing: TODO
                    // 2. Redraw for Full Row Select: Not happening
                    // 3. Set new item DropHilighted: Handled in the ListView
                    //    Controller.
                    IntPtr ptr;
                    if(QTUtility.instanceManager.TryGetButtonBarHandle(this.hwndExplorer, out ptr)) {
                        QTUtility2.SendCOPYDATASTRUCT(ptr, (IntPtr)13, null, (IntPtr)GetItemCount());
                    }
                    bool flag = QTUtility.IsVista && QTUtility.CheckConfig(Settings.ToggleFullRowSelect);
                    NMLISTVIEW nmlistview2 = (NMLISTVIEW)Marshal.PtrToStructure(msg.LParam, typeof(NMLISTVIEW));
                    if(nmlistview2.uChanged == 8 /*LVIF_STATE*/) {
                        uint num5 = nmlistview2.uNewState & LVIS.SELECTED;
                        uint num6 = nmlistview2.uOldState & LVIS.SELECTED;
                        uint num7 = nmlistview2.uNewState & LVIS.DROPHILITED;
                        uint num8 = nmlistview2.uOldState & LVIS.DROPHILITED;
                        uint num9 = nmlistview2.uNewState & LVIS.CUT;
                        uint num10 = nmlistview2.uOldState & LVIS.CUT;
                        if((num8 != num7)) {
                            if(num7 == 0) {
                                HandleDropHilighted(-1);
                            }
                            else {
                                HandleDropHilighted(nmlistview2.iItem);
                            }
                        }
                        if(flag) {
                            if(nmlistview2.iItem != -1 && ((num5 != num6) || (num7 != num8) || (num9 != num10)) && GetCurrentViewMode() == FVM.DETAILS) {
                                PInvoke.SendMessage(nmlistview2.hdr.hwndFrom, LVM.REDRAWITEMS, (IntPtr)nmlistview2.iItem, (IntPtr)nmlistview2.iItem);
                            }
                        }
                        if(SelectionChanged != null && num5 != num6) {
                            SelectionChanged();
                        }
                    }
                    break;
                }

                case LVN.INSERTITEM:
                case LVN.DELETEITEM:
                case LVN.DELETEALLITEMS:
                    if(!QTUtility.CheckConfig(Settings.NoShowSubDirTips)) {
                        HideSubDirTip(1);
                    }
                    if(QTUtility.CheckConfig(Settings.AlternateRowColors) && (GetCurrentViewMode() == FVM.DETAILS)) {
                        PInvoke.InvalidateRect(nmhdr.hwndFrom, IntPtr.Zero, true);
                    }
                    ShellViewController.DefWndProc(ref msg);
                    if(ItemCountChanged != null) {
                        ItemCountChanged(GetItemCount());
                    }
                    return true;

                case LVN.BEGINDRAG:
                    // This won't be necessary it seems.  On Windows 7, when you
                    // start to drag, a MOUSELEAVE message is sent, which hides
                    // the SubDirTip anyway.
                    ShellViewController.DefWndProc(ref msg);
                    this.HideSubDirTip(0xff);
                    break;

                case LVN.ITEMACTIVATE:
                    if(ItemActivated != null) {
                        NMITEMACTIVATE nmitemactivate = (NMITEMACTIVATE)Marshal.PtrToStructure(msg.LParam, typeof(NMITEMACTIVATE));
                        Keys modKeys =
                            (((nmitemactivate.uKeyFlags & 1) == 1) ? Keys.Alt : Keys.None) |
                            (((nmitemactivate.uKeyFlags & 2) == 2) ? Keys.Control : Keys.None) |
                            (((nmitemactivate.uKeyFlags & 4) == 4) ? Keys.Shift : Keys.None);
                        if(ItemActivated(modKeys)) return true;
                    }
                    break;

                case LVN.ODSTATECHANGED:
                    // FullRowSelect doesn't look possible anyway, so whatever.
                    if(QTUtility.IsVista && QTUtility.CheckConfig(Settings.ToggleFullRowSelect)) {
                        NMLVODSTATECHANGE nmlvodstatechange = (NMLVODSTATECHANGE)Marshal.PtrToStructure(msg.LParam, typeof(NMLVODSTATECHANGE));
                        if(((nmlvodstatechange.uNewState & 2) == 2) && (GetCurrentViewMode() == FVM.DETAILS)) {
                            PInvoke.SendMessage(nmlvodstatechange.hdr.hwndFrom, LVM.REDRAWITEMS, (IntPtr)nmlvodstatechange.iFrom, (IntPtr)nmlvodstatechange.iTo);
                        }
                    }
                    break;

                case LVN.HOTTRACK:
                    // This will be handled through WM_MOUSEMOVE.
                    if(QTUtility.CheckConfig(Settings.ShowTooltipPreviews) || !QTUtility.CheckConfig(Settings.NoShowSubDirTips)) {
                        NMLISTVIEW nmlistview = (NMLISTVIEW)Marshal.PtrToStructure(msg.LParam, typeof(NMLISTVIEW));
                        HandleHotTrack(nmlistview.iItem);
                    }
                    break;

                case LVN.KEYDOWN: {
                    // This will be handled through WM_KEYDOWN.
                        NMLVKEYDOWN nmlvkeydown = (NMLVKEYDOWN)Marshal.PtrToStructure(msg.LParam, typeof(NMLVKEYDOWN));
                        if(HandleKeyDown((Keys)nmlvkeydown.wVKey)) {
                            msg.Result = (IntPtr)1;
                            return true;
                        }
                        else {
                            return false;
                        }
                    }
                    break;

                case LVN.GETINFOTIP: {
                        NMLVGETINFOTIP nmlvgetinfotip = (NMLVGETINFOTIP)Marshal.PtrToStructure(msg.LParam, typeof(NMLVGETINFOTIP));
                        return HandleGetInfoTip(nmlvgetinfotip.iItem, GetHotItem() != nmlvgetinfotip.iItem); // TODO there's got to be a better way.
                    }

                case LVN.BEGINLABELEDIT:
                    // This is just for file renaming, which there's no need to
                    // mess with in Windows 7.
                    ShellViewController.DefWndProc(ref msg);
                    if(!QTUtility.IsVista && QTUtility.CheckConfig(Settings.ExtWhileRenaming)) {
                        NMLVDISPINFO nmlvdispinfo = (NMLVDISPINFO)Marshal.PtrToStructure(msg.LParam, typeof(NMLVDISPINFO));
                        if(nmlvdispinfo.item.lParam != IntPtr.Zero) {
                            if(ShellBrowser == null) {
                                HandleRenaming(nmlvdispinfo.item.lParam);
                            }
                            else {
                                IntPtr ptr2 = ShellMethods.ShellGetPath(ShellBrowser);
                                if((ptr2 != IntPtr.Zero)) {
                                    IntPtr ptr3 = PInvoke.ILCombine(ptr2, nmlvdispinfo.item.lParam);
                                    HandleRenaming(ptr3);
                                    PInvoke.CoTaskMemFree(ptr2);
                                    PInvoke.CoTaskMemFree(ptr3);
                                }
                            }
                        }
                    }
                    break;

                case LVN.ENDLABELEDIT:
                    if(EndLabelEdit != null) {
                        NMLVDISPINFO nmlvdispinfo2 = (NMLVDISPINFO)Marshal.PtrToStructure(msg.LParam, typeof(NMLVDISPINFO));
                        EndLabelEdit(nmlvdispinfo2.item);
                    }
                    break;
            }
            return false;
        }

        public void ShowAndClickSubDirTip() {
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
                        this.subDirTip = new SubDirTipForm(hwndSubDirTipMessageReflect, hwndSubDirTipFocusOnMenu, true, this);
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
            if((fSkipForegroundCheck || (this.hwndExplorer == PInvoke.GetForegroundWindow())) && this.TryGetHotTrackPath(iItem, out str)) {
                bool flag = false;
                try {
                    if(!TryMakeSubDirTipPath(ref str)) {
                        return false;
                    }
                    Point pnt = GetSubDirTipPoint(fByKey);
                    if(this.subDirTip == null) {
                        this.subDirTip = new SubDirTipForm(hwndSubDirTipMessageReflect, hwndSubDirTipFocusOnMenu, true, this);
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

        public bool SubDirTipMenuIsShowing() {
            return subDirTip != null && subDirTip.MenuIsShowing;
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
                    }
                    if(ppshv != null) {
                        Marshal.ReleaseComObject(ppshv);
                    }
                    if(ppv != null) {
                        Marshal.ReleaseComObject(ppv);
                    }
                    if(list2 != null) {
                        Marshal.ReleaseComObject(list2);
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
                        bool flag3 = !string.IsNullOrEmpty(pathToFocus);
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
                    }
                    if(ppshf != null) {
                        Marshal.ReleaseComObject(ppshf);
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
