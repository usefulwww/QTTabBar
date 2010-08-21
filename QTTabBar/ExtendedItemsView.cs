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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using QTTabBarLib.Interop;
using QTTabBarLib.Automation;

namespace QTTabBarLib {
    class ExtendedItemsView : ExtendedListViewCommon {
        private AutomationManager AutoMan;
        private Point lastLButtonPoint;
        private Int64 lastLButtonTime;
        private Point lastMouseMovePoint;
        private CachedListItemElement cachedElement;

        internal ExtendedItemsView(ShellBrowserEx ShellBrowser, IntPtr hwndShellView, IntPtr hwndListView, IntPtr hwndSubDirTipMessageReflect, AutomationManager AutoMan)
                : base(ShellBrowser, hwndShellView, hwndListView, hwndSubDirTipMessageReflect) {
            this.AutoMan = AutoMan;
        }

        private IntPtr hwndEnumResult;

        private bool CallbackEnumChildProc_Edit(IntPtr hwnd, IntPtr lParam) {
            if(PInvoke.GetClassName(hwnd) == "Edit") {
                hwndEnumResult = hwnd;
                return false;
            }
            return true;
        }

        private AutomationElement ListItemElementFromPoint(AutomationElementFactory factory, Point pt) {
            if(PInvoke.WindowFromPoint(pt) != Handle) return null;
            AutomationElement elem = factory.FromPoint(pt);
            if(elem == null) return null;
            if(elem.GetClassName() == "UIItem") return elem;
            elem = elem.GetParent();
            if(elem == null) return null;
            if(elem.GetClassName() == "UIItem") return elem;
            return null;
        }

        public override IntPtr GetEditControl() {
            hwndEnumResult = IntPtr.Zero;
            PInvoke.EnumChildWindows(Handle, CallbackEnumChildProc_Edit, IntPtr.Zero);
            return hwndEnumResult;
        }

        public override Rectangle GetFocusedItemRect() {
            if(HasFocus()) {
                return AutoMan.DoQuery(factory => {
                    AutomationElement elem = factory.FromKeyboardFocus();
                    return elem == null ? new Rectangle(0, 0, 0, 0) : elem.GetBoundingRect();
                });
            }
            return new Rectangle(0, 0, 0, 0);
        }

        public override Point GetSubDirTipPoint(bool fByKey) {
            if(fByKey) {
                cachedElement = null;
                AutoMan.DoQuery(factory => {
                    AutomationElement elem = factory.FromKeyboardFocus();
                    if(elem != null) cachedElement = new CachedListItemElement(elem, this);
                    return 0;
                });
            }
            else {
                Point pt = Control.MousePosition;
                PInvoke.ScreenToClient(Handle, ref pt);
                if(cachedElement == null || !cachedElement.FullRect.Contains(pt)) {
                    cachedElement = null;
                    AutoMan.DoQuery(factory => {
                        AutomationElement elem = ListItemElementFromPoint(factory, pt);
                        if(elem != null) cachedElement = new CachedListItemElement(elem, this);
                        return 0;
                    });
                }
            }

            if(cachedElement == null) return new Point(0, 0);
            int x, y;
            Point ret = new Point(0, 0);
            switch(ShellBrowser.ViewMode) {
                case FVM.CONTENT:
                    y = cachedElement.FullRect.Bottom;
                    x = cachedElement.LabelRect.Left;
                    ret = new Point(x, y - 16);
                    break;

                case FVM.DETAILS:
                    x = cachedElement.LabelRect.Right;
                    y = cachedElement.LabelRect.Top;
                    y += (cachedElement.LabelRect.Bottom - y) / 2;
                    ret = new Point(x - 16, y - 8);
                    break;

                case FVM.SMALLICON:
                    x = cachedElement.FullRect.Right;
                    y = cachedElement.FullRect.Top;
                    x -= (cachedElement.FullRect.Bottom - y) / 2;
                    y += (cachedElement.FullRect.Bottom - y) / 2;
                    ret = new Point(x - 8, y - 8);
                    break;

                case FVM.TILE:
                    y = cachedElement.FullRect.Bottom;
                    x = cachedElement.IconRect.Right;
                    ret = new Point(x - 16, y - 16);
                    break;

                case FVM.THUMBSTRIP:
                case FVM.THUMBNAIL:
                case FVM.ICON:
                    x = cachedElement.FullRect.Right;
                    y = cachedElement.IconRect.Bottom;
                    ret = new Point(x - 16, y - 16);
                    break;

                case FVM.LIST:
                    x = cachedElement.FullRect.Right;
                    y = cachedElement.FullRect.Bottom;
                    ret = new Point(x, y - 15);
                    break;
            }
            PInvoke.ClientToScreen(Handle, ref ret);
            return ret;
        }

        private Point GetWindowPos() {
            RECT rect;
            PInvoke.GetWindowRect(Handle, out rect);
            return new Point(rect.left, rect.top);
        }

        protected override bool HandleCursorLoop(Keys key) {
            int focusedIdx = ShellBrowser.GetFocusedIndex();
            int itemCount = ShellBrowser.GetItemCount();
            int selectMe = -1;
            int viewMode = ShellBrowser.ViewMode;
            switch(viewMode) {
                case FVM.CONTENT:
                case FVM.DETAILS:
                    if(key == Keys.Up && focusedIdx == 0) {
                        selectMe = itemCount - 1;
                    }
                    else if(key == Keys.Down && focusedIdx == itemCount - 1) {
                        selectMe = 0;
                    }
                    break;

                case FVM.ICON:
                case FVM.SMALLICON:
                case FVM.THUMBNAIL:
                case FVM.TILE:
                case FVM.LIST:
                    Keys KeyNextItem, KeyPrevItem, KeyNextPage, KeyPrevPage;
                    if(viewMode == FVM.LIST) {
                        KeyNextItem = Keys.Down;
                        KeyPrevItem = Keys.Up;
                        KeyNextPage = Keys.Right;
                        KeyPrevPage = Keys.Left;
                    }
                    else {
                        KeyNextItem = Keys.Right;
                        KeyPrevItem = Keys.Left;
                        KeyNextPage = Keys.Down;
                        KeyPrevPage = Keys.Up;
                    }
                    int pageCount = AutoMan.DoQuery(factory => {
                        AutomationElement elem = factory.FromHandle(Handle);
                        if(elem == null) return -1;
                        return viewMode == FVM.LIST ? elem.GetRowCount() : elem.GetColumnCount();
                    });
                    if(pageCount == -1) return false;
                    int page = focusedIdx % pageCount;
                    if(key == KeyNextItem && (page == pageCount - 1 || focusedIdx == itemCount - 1)) {
                        selectMe = focusedIdx - page;
                    }
                    else if(key == KeyPrevItem && page == 0) {
                        selectMe = Math.Min(focusedIdx + pageCount - 1, itemCount - 1);
                    }
                    else if(key == KeyNextPage && focusedIdx + pageCount >= itemCount) {
                        selectMe = page;
                    }
                    else if(key == KeyPrevPage && focusedIdx < pageCount) {
                        int x = itemCount - focusedIdx - 1;
                        selectMe = x - x % pageCount + focusedIdx;
                    }
                    break;
            }
            if(selectMe >= 0) {
                ShellBrowser.SelectItem(selectMe);
                return true;
            }
            else {
                return false;
            }
        }

        public override int HitTest(Point pt, bool ScreenCoords) {
            if(cachedElement != null) {
                Point pt2 = pt;
                if(ScreenCoords) {
                    PInvoke.ScreenToClient(Handle, ref pt2);
                }
                if(cachedElement.FullRect.Contains(pt2)) {
                    return cachedElement.Index;
                }
            }
            if(!ScreenCoords) {
                PInvoke.ClientToScreen(Handle, ref pt);
            }
            if(subDirTip != null && subDirTip.IsShowing && subDirTip.Bounds.Contains(pt)) {
                return subDirIndex;
            }
            if(PInvoke.WindowFromPoint(pt) != Handle) {
                return -1;
            }
            return AutoMan.DoQuery(factory => {
                AutomationElement elem = ListItemElementFromPoint(factory, pt);
                if(elem == null) return -1;
                cachedElement = new CachedListItemElement(elem, this);
                return cachedElement.Index;
            });
        }

        public override bool HotItemIsSelected() {
            return AutoMan.DoQuery(factory => {
                AutomationElement elem = ListItemElementFromPoint(factory, Control.MousePosition);
                return elem == null ? false : elem.IsSelected();
            });
        }

        public override bool IsTrackingItemName() {
            if(ShellBrowser.ViewMode != FVM.DETAILS) return true;
            Point pt = Control.MousePosition;
            if(cachedElement != null) {
                PInvoke.ScreenToClient(Handle, ref pt);
                return cachedElement.LabelRect.Contains(pt);
            }
            else {
                return AutoMan.DoQuery(factory => {
                    AutomationElement elem = factory.FromPoint(pt);
                    return elem == null ? false : elem.GetAutomationId() == "System.ItemNameDisplay";
                });
            }
        }

        protected override bool ListViewController_MessageCaptured(ref Message msg) {
            if(msg.Msg == WM_AFTERPAINT) {
                cachedElement = null;
            }

            if(base.ListViewController_MessageCaptured(ref msg)) {
                return true;
            }

            switch(msg.Msg) {
                case LVM.SCROLL: {
                    int amount = msg.WParam.ToInt32();
                    SetRedraw(false);
                    AutoMan.DoQuery(factory => {
                        AutomationElement elem = factory.FromHandle(Handle);
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
                                    : ScrollAmount.SmallIncrement, amount*lines);
                        }
                        return 0;
                    });
                    SetRedraw(true);
                    return true;
                }

                case WM.MOUSEACTIVATE: {
                    int res = (int)msg.Result;
                    bool ret = OnMouseActivate(ref res);
                    msg.Result = (IntPtr)res;
                    return ret;
                }

                case WM.LBUTTONDOWN: {
                    // The ItemsView's window class doesn't have the CS_DBLCLKS
                    // class style, which means we won't be receiving the
                    // WM.LBUTTONDBLCLK message.  We'll just have to make do
                    // without...                    
                    Int64 now = DateTime.Now.Ticks;
                    Point pt = QTUtility2.PointFromLPARAM(msg.LParam);
                    if((now - lastLButtonTime)/10000 <= SystemInformation.DoubleClickTime) {
                        Size size = SystemInformation.DoubleClickSize;
                        if(Math.Abs(pt.X - lastLButtonPoint.X) <= size.Width) {
                            if(Math.Abs(pt.Y - lastLButtonPoint.Y) <= size.Height) {
                                lastLButtonTime = 0;
                                if(OnDoubleClick(pt)) {
                                    return true;
                                }
                                if(HitTest(pt, false) > -1) {
                                    // Explorer includes an option to make
                                    // single-clicking activate items.
                                    // TODO: Support that.
                                    if(OnItemActivated(Control.ModifierKeys)) {
                                        return true;
                                    }
                                }
                                return false;
                            }
                        }
                    }
                    lastLButtonPoint = pt;
                    lastLButtonTime = now;
                    return false;
                }

                case WM.MOUSEMOVE: {
                    Point pt = QTUtility2.PointFromLPARAM(msg.LParam);
                    if(pt != lastMouseMovePoint) {
                        lastMouseMovePoint = pt;
                        if(QTUtility.CheckConfig(Settings.ShowTooltipPreviews) ||
                                !QTUtility.CheckConfig(Settings.NoShowSubDirTips)) {
                            OnHotTrack(HitTest(pt, false));
                        }
                    }
                    break;
                }


                case WM.KEYDOWN:
                    if(OnKeyDown((Keys)msg.WParam)) return true;
                    if((Keys)msg.WParam == Keys.Enter) {
                        return OnItemActivated(Control.ModifierKeys);
                    }
                    break;

                case WM.NOTIFY: {
                    NMHDR nmhdr = (NMHDR)Marshal.PtrToStructure(msg.LParam, typeof(NMHDR));
                    if(nmhdr.code == -530 /* TTN_NEEDTEXT */) {
                        NMTTDISPINFO dispinfo = (NMTTDISPINFO)Marshal.PtrToStructure(msg.LParam, typeof(NMTTDISPINFO));
                        if((dispinfo.uFlags & 0x20 /* TTF_TRACK */) != 0) {
                            return OnGetInfoTip(ShellBrowser.GetFocusedIndex(), true);
                        }
                        else {
                            int i = GetHotItem();
                            if(i != -1 && IsTrackingItemName()) {
                                return OnGetInfoTip(i, false);
                            }
                        }
                    }
                    break;
                }
            }
            return false;
        }

        public override bool PointIsBackground(Point pt, bool screenCoords) {
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

        protected override bool ShellViewController_MessageCaptured(ref Message msg) {
            if(base.ShellViewController_MessageCaptured(ref msg)) {
                return true;
            }

            switch(msg.Msg) {
                // Undocumented message that seems to be fired every time the 
                // selection changes.
                case WM.USER + 163:
                    OnSelectionChanged();
                    break;
                
                // Undocumented message that seems to be fired every time the
                // item count changes.
                case WM.USER + 174:
                    OnItemCountChanged();
                    break;
            }
            return false;
        }

        private class CachedListItemElement {
            public CachedListItemElement(AutomationElement elem, ExtendedItemsView parent) {
                Point offset = parent.GetWindowPos();
                offset = new Point(-offset.X, -offset.Y);
                Index = elem.GetItemIndex();
                Rectangle rect = elem.GetBoundingRect();
                rect.Offset(offset);
                FullRect = rect;
                bool foundLabel = false;
                bool foundIcon = false;
                foreach(AutomationElement child in elem.GetChildren()) {
                    if(!foundLabel && child.GetAutomationId() == "System.ItemNameDisplay") {
                        rect = child.GetBoundingRect();
                        rect.Offset(offset);
                        LabelRect = rect;
                        if(foundIcon) break;
                        foundLabel = true;
                    }
                    else if(!foundIcon && child.GetClassName() == "UIImage") {
                        rect = child.GetBoundingRect();
                        rect.Offset(offset);
                        IconRect = rect;
                        if(foundLabel) break;
                        foundIcon = true;
                    }
                }
            }
            
            public int Index { get; private set; }
            public Rectangle FullRect { get; private set; }
            public Rectangle LabelRect { get; private set; }
            public Rectangle IconRect { get; private set; }
        }
    }
}
