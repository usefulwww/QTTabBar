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
        private Point lastDragPoint;
        private Point lastLButtonPoint;
        private Int64 lastLButtonTime;
        private Point lastMouseMovePoint;


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

        public override int GetFocusedItem() {
            if(HasFocus()) {
                return AutoMan.DoQuery(factory => {
                    AutomationElement elem = factory.FromKeyboardFocus();
                    return elem == null ? -1 : elem.GetItemIndex();
                });
            }
            return -1;
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

        public override int GetItemCount() {
            return AutoMan.DoQuery(factory => {
                AutomationElement elem = factory.FromHandle(ListViewController.Handle);
                return elem == null ? 0 : elem.GetItemCount();
            });
        }

        public override int GetSelectedCount() {
            return AutoMan.DoQuery(factory => {
                AutomationElement elem = factory.FromHandle(ListViewController.Handle);
                return elem == null ? 0 : elem.GetSelectedCount();
            });
        }

        public override Point GetSubDirTipPoint(bool fByKey) {
            int viewMode = ViewMode;
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

        public override int HitTest(Point pt, bool ScreenCoords) {
            if(!ScreenCoords) {
                PInvoke.ClientToScreen(Handle, ref pt);
            }
            else if(PInvoke.WindowFromPoint(pt) != Handle) {
                return -1;
            }
            return AutoMan.DoQuery(factory => {
                AutomationElement elem = ListItemElementFromPoint(factory, pt);
                return elem == null ? -1 : elem.GetItemIndex();
            });
        }

        public override bool HotItemIsSelected() {
            return AutoMan.DoQuery(factory => {
                AutomationElement elem = ListItemElementFromPoint(factory, Control.MousePosition);
                return elem == null ? false : elem.IsSelected();
            });
        }

        public override bool IsTrackingItemName() {
            if(ViewMode != FVM.DETAILS) return true;
            return AutoMan.DoQuery(factory => {
                AutomationElement elem = factory.FromPoint(Control.MousePosition);
                return elem == null ? false : elem.GetAutomationId() == "System.ItemNameDisplay";
            });
        }

        protected override bool ListViewController_MessageCaptured(ref Message msg) {
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
                                        : ScrollAmount.SmallIncrement, amount * lines);
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
                        Point pt = new Point(
                            QTUtility2.GET_X_LPARAM(msg.LParam),
                            QTUtility2.GET_Y_LPARAM(msg.LParam));
                        if((now - lastLButtonTime) / 10000 <= SystemInformation.DoubleClickTime) {
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
                    }
                    return false;

                case WM.MOUSEMOVE: {
                        Point pt = new Point(QTUtility2.GET_X_LPARAM(msg.LParam), QTUtility2.GET_Y_LPARAM(msg.LParam));
                        if(pt != lastMouseMovePoint) {
                            lastMouseMovePoint = pt;
                            if(QTUtility.CheckConfig(Settings.ShowTooltipPreviews) || !QTUtility.CheckConfig(Settings.NoShowSubDirTips)) {
                                OnHotTrack(HitTest(pt, false));
                            }
                        }
                    }
                    break;

                case WM.KEYDOWN:
                    if(OnKeyDown((Keys)msg.WParam)) return true;
                    if((Keys)msg.WParam == Keys.Enter) {
                        return OnItemActivated(Control.ModifierKeys);
                    }
                    break;

                case WM.USER + 209: { // This message appears to control dragging.
                        Point pt = new Point((int)msg.WParam, (int)msg.LParam);
                        if(pt == lastDragPoint) {
                            return false;
                        }
                        lastDragPoint = pt;
                        OnDropHilighted(HitTest(pt, false));
                    }
                    break;

                case WM.NOTIFY: {
                        NMHDR nmhdr = (NMHDR)Marshal.PtrToStructure(msg.LParam, typeof(NMHDR));
                        if(nmhdr.code == -530 /* TTN_NEEDTEXT */) {
                            NMTTDISPINFO dispinfo = (NMTTDISPINFO)Marshal.PtrToStructure(msg.LParam, typeof(NMTTDISPINFO));
                            if((dispinfo.uFlags & 0x20 /* TTF_TRACK */) != 0) {
                                return OnGetInfoTip(GetFocusedItem(), true);
                            }
                            else {
                                int i = GetHotItem();
                                if(i != -1 && IsTrackingItemName()) {
                                    return OnGetInfoTip(i, false);
                                }
                            }
                        }
                    }
                    break;
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
    }
}
