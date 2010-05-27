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
    using QTTabBarLib.Automation;
    using QTTabBarLib.Interop;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms;
    
    public class ListViewWrapper {
        private AutomationQueryManager QueryMan;
        private IntPtr ExplorerHandle;
        private IntPtr ShellContainer;
        private IntPtr hwndEnumResult;
        private bool fIsSysListView;
        private bool fTrackMouseEvent;
        private NativeWindowController ContainerController;
        private NativeWindowController ShellViewController;
        private NativeWindowController ListViewController;
        private Point lastDragPoint;
        private Point lastLButtonPoint;
        private Int64 lastLButtonTime;
        private Point lastMouseMovePoint;
        private int iListViewItemState;
        private List<int> lstColumnFMT;
        private bool fListViewHasFocus;
        private IShellBrowser ShellBrowser;
        private int StoredListItemIdx;
        //private AutomationElement StoredListItemElem;

        internal delegate bool SVDestroyHandler();
        internal delegate bool SVMouseActivateHandler(ref int result);
        internal delegate bool ItemInsertedHandler();
        internal delegate bool ItemDeletedHandler();
        internal delegate bool ItemActivatedHandler(Keys modKeys);
        internal delegate bool AllItemsDeletedHandler();
        internal delegate bool SelectionChangedHandler();
        internal delegate bool BeginDragHandler();
        internal delegate bool DropHilightedHandler(int iItem);
        internal delegate bool HotTrackHandler(int iItem);
        internal delegate bool MiddleClickHandler(Point pt);
        internal delegate bool DoubleClickHandler(Point pt);
        internal delegate bool KeyDownHandler(Keys key);
        internal delegate bool GetInfoTipHandler(int iItem, bool byKey);
        internal delegate bool BeginLabelEditHandler(LVITEM item);
        internal delegate bool EndLabelEditHandler(LVITEM item);
        internal delegate bool BeginScrollHandler();
        internal delegate bool MouseLeaveHandler();

        internal event SVDestroyHandler SVDestroy;                // OK
        internal event SVMouseActivateHandler SVMouseActivate;    // OK
        internal event ItemInsertedHandler ItemInserted;          // TODO
        internal event ItemDeletedHandler ItemDeleted;            // TODO
        internal event ItemActivatedHandler ItemActivated;        // OK
        internal event AllItemsDeletedHandler AllItemsDeleted;    // SysListView Only
        internal event SelectionChangedHandler SelectionChanged;  // TODO
        internal event BeginDragHandler BeginDrag;                // OK
        internal event DropHilightedHandler DropHilighted;        // OK
        internal event HotTrackHandler HotTrack;                  // OK
        internal event MiddleClickHandler MiddleClick;            // OK
        internal event DoubleClickHandler DoubleClick;            // OK
        internal event KeyDownHandler KeyDown;                    // OK
        internal event GetInfoTipHandler GetInfoTip;              // TODO
        internal event BeginLabelEditHandler BeginLabelEdit;      // SysListView Only
        internal event EndLabelEditHandler EndLabelEdit;          // SysListView Only
        internal event BeginScrollHandler BeginScroll;            // TODO
        internal event MouseLeaveHandler MouseLeave;              // SysListView Only

        internal ListViewWrapper(IShellBrowser ShellBrowser, IntPtr ExplorerHandle) {
            this.ShellBrowser = ShellBrowser;
            this.ExplorerHandle = ExplorerHandle;
            QueryMan = new AutomationQueryManager();
            if(QTUtility.IsVista) {
                hwndEnumResult = IntPtr.Zero;
                PInvoke.EnumChildWindows(ExplorerHandle, new EnumWndProc(this.CallbackEnumChildProc_Container), IntPtr.Zero);
                ShellContainer = hwndEnumResult;
            }
            else {
                ShellContainer = ExplorerHandle;
            }
            if(ShellContainer != IntPtr.Zero) {
                ContainerController = new NativeWindowController(ShellContainer);
                ContainerController.MessageCaptured += new NativeWindowController.MessageEventHandler(ContainerController_MessageCaptured);
            }
            Initialize();
        }

        private bool CallbackEnumChildProc_Container(IntPtr hwnd, IntPtr lParam) {
            if(GetWindowClassName(hwnd) == "ShellTabWindowClass") {
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

        private bool ContainerController_MessageCaptured(ref System.Windows.Forms.Message msg) {
            if(msg.Msg == WM.PARENTNOTIFY && PInvoke.LoWord((int)msg.WParam) == WM.CREATE) {
                string name = GetWindowClassName(msg.LParam);
                if(name == "SHELLDLL_DefView") {
                    RecaptureHandles(msg.LParam);
                }
            }
            return false;
        }

        public void FireHotTrack() {
            // TODO
        }

        public void ScrollHorizontal(int amount) {
            if(ListViewController.Handle != null) {
                if(fIsSysListView) {
                    PInvoke.SendMessage(ListViewController.Handle, LVM.SCROLL, (IntPtr)(-amount), IntPtr.Zero);
                }
                else {
                    // TODO
                }
            }
        }

        public int GetCurrentViewMode() {
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
                // Not supported.
                return IntPtr.Zero;
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
                return QueryMan.DoQuery<int>(ElemMan => {
                    AutomationElement elem = ElemMan.FromKeyboardFocus();
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
                    return QueryMan.DoQuery<Rectangle>(ElemMan => {
                        AutomationElement elem = ElemMan.FromKeyboardFocus();
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
                return QueryMan.DoQuery<int>(ElemMan => {
                    AutomationElement elem = ElemMan.FromHandle(ListViewController.Handle);
                    return elem == null ? 0 : elem.GetItemCount();
                });
            }
        }

        // Try not to use this, ever!
        public IntPtr GetListViewHandle() {
            return (ListViewController == null) ? IntPtr.Zero : ListViewController.Handle;
        }

        private RECT GetLVITEMRECT(IntPtr hwnd, int iItem, bool fSubDirTip, int fvm) {
            RECT rect;
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

            rect = new RECT();
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
                IntPtr zero = IntPtr.Zero;
                structure.pszText = Marshal.AllocHGlobal(520);
                structure.cchTextMax = 260;
                structure.iItem = iItem;
                structure.mask = 1;
                zero = Marshal.AllocHGlobal(Marshal.SizeOf(structure));
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

        public int GetSelectedCount() {
            if(ListViewController == null) {
                return 0;
            }
            else if(fIsSysListView) {
                return (int)PInvoke.SendMessage(ListViewController.Handle, LVM.GETSELECTEDCOUNT, IntPtr.Zero, IntPtr.Zero);
            }
            else {
                return QueryMan.DoQuery<int>(ElemMan => {
                    AutomationElement elem = ElemMan.FromHandle(ListViewController.Handle);
                    return elem == null ? 0 : elem.GetSelectedCount();
                });
            }
        }

        public Point GetSubDirTipPoint(int iItem) {
            int viewMode = GetCurrentViewMode();
            if(fIsSysListView) {
                RECT rect = GetLVITEMRECT(ListViewController.Handle, iItem, true, viewMode);
                return new Point(rect.right - 16, rect.bottom - 16);
            }
            else {

                return new Point(0, 0);

                /*
                if(iItem != StoredListItemIdx || StoredListItemElem == null) {
                    // TODO ... maybe
                }
                
                AutomationElement elem = StoredListItemElem;
                Rectangle rect = elem.GetBoundingRect();
                int x, y;
                switch(viewMode) {
                    case FVM.CONTENT:
                        y = rect.Bottom;
                        elem = elem.FindMatchingChild(child => child.GetAutomationId() == "System.ItemNameDisplay");
                        if(elem == null) return new Point(0, 0);
                        x = elem.GetBoundingRect().Left;
                        return new Point(x, y - 16);

                    case FVM.DETAILS:
                        elem = elem.FindMatchingChild(child => child.GetAutomationId() == "System.ItemNameDisplay");
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
                        elem = elem.FindMatchingChild(child => child.GetClassName() == "UIImage");
                        if(elem == null) return new Point(0, 0);
                        x = elem.GetBoundingRect().Right;
                        return new Point(x - 16, y - 16);

                    case FVM.THUMBSTRIP:
                    case FVM.THUMBNAIL:
                    case FVM.ICON:
                        x = rect.Right;
                        elem = elem.FindMatchingChild(child => child.GetClassName() == "UIImage");
                        if(elem == null) return new Point(0, 0);
                        y = elem.GetBoundingRect().Bottom;
                        return new Point(x - 16, y - 16);

                    case FVM.LIST:
                    default:
                        x = rect.Right;
                        y = rect.Bottom;
                        return new Point(x, y - 15);
                }
            */
            }
        }

        private static string GetWindowClassName(IntPtr hwnd) {
            StringBuilder lpClassName = new StringBuilder(260);
            PInvoke.GetClassName(hwnd, lpClassName, lpClassName.Capacity);
            return lpClassName.ToString();
        }

        private bool HandleLVCUSTOMDRAW(ref System.Windows.Forms.Message msg) {
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
                            bool fullRowSel = !QTUtility.CheckConfig(Settings.NoFullRowSelect);

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
                            bool flag6 = QTUtility.CheckConfig(Settings.NoFullRowSelect) ^ QTUtility.IsVista;
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
                                        int width = 0;
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

        public bool HasFocus() {
            return (ListViewController != null &&
                PInvoke.GetFocus() == ListViewController.Handle);
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
                return QueryMan.DoQuery<bool>(ElemMan => {
                    AutomationElement elem = ListItemElementAt(Control.MousePosition, ElemMan);
                    return elem == null ? false : elem.IsSelected();
                });
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
                return QueryMan.DoQuery<int>(ElemMan => {
                    AutomationElement elem = ListItemElementAt(pt, ElemMan);
                    return elem == null ? -1 : elem.GetItemIndex();
                });
            }
        }

        private AutomationElement ListItemElementAt(Point pt, AutomationElementManager ElemMan) {
            if(PInvoke.WindowFromPoint(pt) != ListViewController.Handle) return null;
            AutomationElement elem = ElemMan.FromPoint(pt);
            if(elem == null) return null;
            if(elem.GetClassName() == "UIItem") return elem;
            elem = elem.GetParent();
            if(elem.GetClassName() == "UIItem") return elem;
            return null;
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
            PInvoke.EnumChildWindows(ExplorerHandle, new EnumWndProc(this.CallbackEnumChildProc_ShellView), IntPtr.Zero);
            if(hwndEnumResult != IntPtr.Zero) {
                RecaptureHandles(hwndEnumResult);
            }
        }

        // If the ListView is in Details mode, returns true only if the mouse
        // is over the ItemName column.  Returns true always for any other mode.
        // This function only returns value results if the mouse is known to be
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
                return QueryMan.DoQuery<bool>(ElemMan => {
                    AutomationElement elem = ElemMan.FromPoint(Control.MousePosition);
                    return elem == null ? false : elem.GetAutomationId() == "System.ItemNameDisplay";
                });
            }
        }

        public bool IsTrackingBackground() {
            if(ListViewController == null || PInvoke.WindowFromPoint(Control.MousePosition) != ListViewController.Handle) {
                return false;
            }
            if(fIsSysListView) {
                return GetHotItem() == -1;
            }
            else {
                return QueryMan.DoQuery<bool>(ElemMan => {
                    AutomationElement elem = ElemMan.FromPoint(Control.MousePosition);
                    if(elem == null) return false;
                    string className = elem.GetClassName();
                    return className == "UIItemsView" || className == "UIGroupItem";
                });
            }            
        }

        /*
        private AutomationElement ListItemElementAt(Point pt) {
            if(PInvoke.WindowFromPoint(pt) != ListViewController.Handle) return null;
            AutomationElement elem = AutomationElement.FromPoint(pt);
            if(elem == null) return null;
            if(elem.GetClassName() == "UIItem") return elem;
            elem = elem.GetParent();
            if(elem.GetClassName() == "UIItem") return elem;
            return null;
        }
        */

        private bool ListViewController_MessageCaptured(ref System.Windows.Forms.Message msg) {

            // First block is for both SysListView and ItemsView
            switch(msg.Msg) {
                case WM.DESTROY:
                    ListViewController.ReleaseHandle();
                    ListViewController = null;
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
                    if(MouseLeave != null) return MouseLeave();
                    break;
            }

            // From here on down is only for the ItemsView.
            if(fIsSysListView) {
                return false;
            }

            switch(msg.Msg) {
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

                case WM.MOUSEMOVE:
                    if(HotTrack != null) {
                        Point pt = new Point(QTUtility2.GET_X_LPARAM(msg.LParam), QTUtility2.GET_Y_LPARAM(msg.LParam));
                        if(pt != lastMouseMovePoint) {
                            lastMouseMovePoint = pt;
                            return HotTrack(HitTest(pt, false));
                        }
                    }
                    break;

                case WM.KEYDOWN:
                    if(KeyDown != null && KeyDown((Keys)msg.WParam)) return true;
                    if((Keys)msg.WParam == Keys.Enter && ItemActivated != null) {
                        return ItemActivated(Control.ModifierKeys);
                    }
                    break;


                case WM.USER + 209: // This message appears to control dragging.
                    if(DropHilighted != null) {
                        Point pt = new Point((int)msg.WParam, (int)msg.LParam);
                        if(pt == lastDragPoint) {
                            return false;
                        }
                        lastDragPoint = pt;
                        return DropHilighted(HitTest(pt, false));
                    }
                    break;
                
                case WM.NOTIFY:
                    if(GetInfoTip != null) {
                        NMHDR nmhdr = (NMHDR)Marshal.PtrToStructure(msg.LParam, typeof(NMHDR));
                        if(nmhdr.code == -530 /* TTN_NEEDTEXT */) {
                            NMTTDISPINFO dispinfo = (NMTTDISPINFO)Marshal.PtrToStructure(msg.LParam, typeof(NMTTDISPINFO));
                            if((dispinfo.uFlags & 0x20 /* TTF_TRACK */) != 0) {
                                return GetInfoTip(GetFocusedItem(), true);
                            }
                            else {
                                int i = GetHotItem();
                                if(i != -1 && IsTrackingItemName()) {
                                    return GetInfoTip(i, false);    
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
            ShellViewController.MessageCaptured += new NativeWindowController.MessageEventHandler(ShellViewController_MessageCaptured);

            hwndEnumResult = IntPtr.Zero;
            PInvoke.EnumChildWindows(hwndShellView, new EnumWndProc(this.CallbackEnumChildProc_ListView), IntPtr.Zero);
            if(hwndEnumResult == IntPtr.Zero) {
                return;
            }

            ListViewController = new NativeWindowController(hwndEnumResult);
            ListViewController.MessageCaptured += new NativeWindowController.MessageEventHandler(ListViewController_MessageCaptured);

            if(fIsSysListView) {
                uint mask = LVS_EX.GRIDLINES | LVS_EX.FULLROWSELECT;
                uint flags = 0;
                if(QTUtility.CheckConfig(Settings.DetailsGridLines)) {
                    flags |= LVS_EX.GRIDLINES;
                }
                if(QTUtility.IsVista) {
                    if(!QTUtility.CheckConfig(Settings.NoFullRowSelect)) {
                        flags |= LVS_EX.FULLROWSELECT;
                    }
                }
                else if(QTUtility.CheckConfig(Settings.NoFullRowSelect)) {
                    if(((int)PInvoke.SendMessage(ListViewController.Handle, LVM.GETVIEW, IntPtr.Zero, IntPtr.Zero)) == 1) {
                        flags |= LVS_EX.FULLROWSELECT;
                    }
                }
                PInvoke.SendMessage(ListViewController.Handle, LVM.SETEXTENDEDLISTVIEWSTYLE, (IntPtr)mask, (IntPtr)flags);
            }

            this.fTrackMouseEvent = false;
            TRACKMOUSEEVENT structure = new TRACKMOUSEEVENT();
            structure.cbSize = Marshal.SizeOf(structure);
            structure.dwFlags = 2;
            structure.hwndTrack = ListViewController.Handle;
            PInvoke.TrackMouseEvent(ref structure);
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

        private bool ShellViewController_MessageCaptured(ref System.Windows.Forms.Message msg) {
            IntPtr ptr;
            switch(msg.Msg) {
                // The ShellView is destroyed and recreated when Explorer is refreshed.
                case WM.DESTROY:
                    if(SVDestroy != null) {
                        ShellViewController.ReleaseHandle();
                        ShellViewController = null;
                        return SVDestroy();
                    }
                    break;

                case WM.MOUSEACTIVATE:
                    if(SVMouseActivate != null) {
                        int res = (int)msg.Result;
                        bool ret = SVMouseActivate(ref res);
                        msg.Result = (IntPtr)res;
                        return ret;
                    }
                    break;

                /*
            fEnteredMenuLoop is used nowhere... Unfinished feature?
            case WM.ENTERMENULOOP:
                this.fEnteredMenuLoop = true;
                break;

            case WM.EXITMENULOOP:
                this.fEnteredMenuLoop = false;
                break;
                */
            }

            if(msg.Msg != WM.NOTIFY) {
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
                        if(QTUtility.instanceManager.TryGetButtonBarHandle(this.ExplorerHandle, out ptr)) {
                            QTUtility2.SendCOPYDATASTRUCT(ptr, (IntPtr)13, null, (IntPtr)GetItemCount());
                        }
                        bool flag = QTUtility.IsVista && QTUtility.CheckConfig(Settings.NoFullRowSelect);
                        if(DropHilighted != null || SelectionChanged != null || flag) {
                            NMLISTVIEW nmlistview2 = (NMLISTVIEW)Marshal.PtrToStructure(msg.LParam, typeof(NMLISTVIEW));
                            if(nmlistview2.uChanged == 8 /*LVIF_STATE*/) {
                                uint num5 = nmlistview2.uNewState & LVIS.SELECTED;
                                uint num6 = nmlistview2.uOldState & LVIS.SELECTED;
                                uint num7 = nmlistview2.uNewState & LVIS.DROPHILITED;
                                uint num8 = nmlistview2.uOldState & LVIS.DROPHILITED;
                                uint num9 = nmlistview2.uNewState & LVIS.CUT;
                                uint num10 = nmlistview2.uOldState & LVIS.CUT;
                                if(DropHilighted != null && (num8 != num7)) {
                                    if(num7 == 0) {
                                        DropHilighted(-1);
                                    }
                                    else {
                                        DropHilighted(nmlistview2.iItem);
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
                        }
                        break;
                    }

                case LVN.INSERTITEM:
                    ItemInserted();
                    if(QTUtility.CheckConfig(Settings.AlternateRowColors) && (GetCurrentViewMode() == FVM.DETAILS)) {
                        PInvoke.InvalidateRect(nmhdr.hwndFrom, IntPtr.Zero, true);
                    }
                    break;

                case LVN.DELETEITEM:
                    ItemDeleted();
                    if(QTUtility.CheckConfig(Settings.AlternateRowColors) && (GetCurrentViewMode() == FVM.DETAILS)) {
                        PInvoke.InvalidateRect(nmhdr.hwndFrom, IntPtr.Zero, true);
                    }
                    break;

                case LVN.DELETEALLITEMS:
                    if(AllItemsDeleted != null) return AllItemsDeleted();
                    break;

                case LVN.BEGINDRAG:
                    if(BeginDrag != null) {
                        ShellViewController.DefWndProc(ref msg);
                        BeginDrag();
                        return true;
                    }
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
                    if(QTUtility.IsVista && QTUtility.CheckConfig(Settings.NoFullRowSelect)) {
                        NMLVODSTATECHANGE nmlvodstatechange = (NMLVODSTATECHANGE)Marshal.PtrToStructure(msg.LParam, typeof(NMLVODSTATECHANGE));
                        if(((nmlvodstatechange.uNewState & 2) == 2) && (GetCurrentViewMode() == FVM.DETAILS)) {
                            PInvoke.SendMessage(nmlvodstatechange.hdr.hwndFrom, LVM.REDRAWITEMS, (IntPtr)nmlvodstatechange.iFrom, (IntPtr)nmlvodstatechange.iTo);
                        }
                    }
                    break;

                case LVN.HOTTRACK:
                    // This will be handled through WM_MOUSEMOVE.
                    if(HotTrack != null) {
                        NMLISTVIEW nmlistview = (NMLISTVIEW)Marshal.PtrToStructure(msg.LParam, typeof(NMLISTVIEW));
                        HotTrack(nmlistview.iItem);
                        break;
                    }
                    break;

                case LVN.KEYDOWN:
                    // This will be handled through WM_KEYDOWN.
                    if(KeyDown != null) {
                        NMLVKEYDOWN nmlvkeydown = (NMLVKEYDOWN)Marshal.PtrToStructure(msg.LParam, typeof(NMLVKEYDOWN));
                        if(KeyDown((Keys)nmlvkeydown.wVKey)) {
                            msg.Result = (IntPtr)1;
                            return true;
                        }
                        else {
                            return false;
                        }
                    }
                    break;

                case LVN.GETINFOTIP:
                    if(GetInfoTip != null) {
                        NMLVGETINFOTIP nmlvgetinfotip = (NMLVGETINFOTIP)Marshal.PtrToStructure(msg.LParam, typeof(NMLVGETINFOTIP));
                        return GetInfoTip(nmlvgetinfotip.iItem, GetHotItem() != nmlvgetinfotip.iItem); // TODO there's got to be a better way.
                    }
                    break;

                case LVN.BEGINLABELEDIT:
                    // This is just for file renaming, which there's no need to
                    // mess with in Windows 7.
                    ShellViewController.DefWndProc(ref msg);
                    if(msg.Result == IntPtr.Zero && BeginLabelEdit != null) {
                        NMLVDISPINFO nmlvdispinfo = (NMLVDISPINFO)Marshal.PtrToStructure(msg.LParam, typeof(NMLVDISPINFO));
                        BeginLabelEdit(nmlvdispinfo.item);
                    }
                    break;

                case LVN.ENDLABELEDIT:
                    if(EndLabelEdit != null) {
                        NMLVDISPINFO nmlvdispinfo2 = (NMLVDISPINFO)Marshal.PtrToStructure(msg.LParam, typeof(NMLVDISPINFO));
                        EndLabelEdit(nmlvdispinfo2.item);
                    }
                    break;

                case LVN.BEGINSCROLL:
                    // This we can handle by intercepting SBM_SETSCROLLINFO
                    // when it's sent to the scrollbars.
                    if(BeginScroll != null) {
                        return BeginScroll();
                    }
                    break;
            }
            return false;
        }

    }
}

/// //////////////////////////////////////////////////////////////////////////////
/*
internal IntPtr GetExplorerListView() {
    bool isSLV32;
    IntPtr ptr;
    return this.GetExplorerListView(out isSLV32, out ptr);
}

internal IntPtr GetExplorerListView(out bool isSysListView32) {
    IntPtr ptr;
    return this.GetExplorerListView(out isSysListView32, out ptr);
}

internal IntPtr GetExplorerListView(out bool isSysListView32, out IntPtr hwndShellView) {
    hwndShellView = IntPtr.Zero;
    IShellView ppshv = null;
    try {
        if(this.ShellBrowser.QueryActiveShellView(out ppshv) == 0) {
            ppshv.GetWindow(out hwndShellView);
            if(hwndShellView != IntPtr.Zero) {
                this.hwndDirectUI = IntPtr.Zero;
                this.hwndSysListView32 = IntPtr.Zero;
                PInvoke.EnumChildWindows(
                        hwndShellView,
                        new EnumWndProc(this.CallbackEnumChildProc_SysListView32),
                        IntPtr.Zero);

                if(this.hwndSysListView32 != IntPtr.Zero) {
                    isSysListView32 = true;
                    return this.hwndSysListView32;
                }
                else if(this.hwndDirectUI != IntPtr.Zero) {
                    isSysListView32 = false;
                    return this.hwndDirectUI;
                }
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
    }
    isSysListView32 = false;
    return IntPtr.Zero;
}

*/
