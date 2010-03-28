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
    using QTTabBarLib.Interop;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;

    internal class DropDownMenuReorderable : DropDownMenuBase {
        private const int COUNT_SCROLL = 3;
        private ToolStripControlHost downButton;
        private ToolStripItem draggingItem;
        private bool fBlockItemAddRemove;
        private bool fCancelClosingAncestors;
        private bool fCheckOnEdgeClick;
        private bool fEnableScroll;
        private bool fNowScrollButtonsRequired;
        private bool fReordered;
        private bool fReorderEnabled;
        private bool fSpaceKeyExecute;
        private bool fSuppressMouseMove;
        protected bool fSuppressMouseMove_Scroll;
        private bool fSuppressMouseMoveOnce;
        private bool fVirtualMode;
        private int iSuppressStartIndex;
        private List<string> lstProhibitedKeys;
        private Dictionary<string, List<ToolStripItem>> MenuItemGroups;
        public IntPtr MessageParent;
        private static MethodInfo miScroll;
        private MouseButtons mouseButtons;
        private string pathParent;
        private static PropertyInfo piScroll;
        private static PropertyInfo piScrollButtonDn;
        private static PropertyInfo piScrollButtonUp;
        private Stack<ToolStripItem> stcVirtualItems_Bottom;
        private Stack<ToolStripItem> stcVirtualItems_Top;
        private ToolStripMenuItem tsmiPreviousEdgeClicked;
        private ToolStripControlHost upButton;
        private const int VIRTUAL_MAX_CONTAINEDITEMCOUNT = 0x40;
        private const int VIRTUAL_THRESHOLD_MINITEMCOUNT = 0x80;

        public event ItemRightClickedEventHandler ItemMiddleClicked;

        public event ItemRightClickedEventHandler ItemRightClicked;

        public event MouseEventHandler MouseDragMove;

        public event EventHandler MouseScroll;

        public event EventHandler MouseUpBeforeDrop;

        public event MenuReorderedEventHandler ReorderFinished;

        public DropDownMenuReorderable(IContainer container)
            : base(container) {
            this.iSuppressStartIndex = -1;
            this.MenuItemGroups = new Dictionary<string, List<ToolStripItem>>();
            this.lstProhibitedKeys = new List<string>();
            this.fReorderEnabled = true;
            this.fEnableScroll = true;
        }

        public DropDownMenuReorderable(IContainer container, bool respondModKeys, bool enableShiftKey)
            : base(container, respondModKeys, enableShiftKey) {
            this.iSuppressStartIndex = -1;
            this.MenuItemGroups = new Dictionary<string, List<ToolStripItem>>();
            this.lstProhibitedKeys = new List<string>();
            this.fReorderEnabled = true;
            this.fEnableScroll = true;
        }

        public DropDownMenuReorderable(IContainer container, bool respondModKeys, bool enableShiftKey, bool enableReorder)
            : base(container, respondModKeys, enableShiftKey) {
            this.iSuppressStartIndex = -1;
            this.MenuItemGroups = new Dictionary<string, List<ToolStripItem>>();
            this.lstProhibitedKeys = new List<string>();
            this.fReorderEnabled = true;
            this.fEnableScroll = true;
            this.fReorderEnabled = enableReorder;
        }

        public void AddItem(ToolStripItem item, string key) {
            this.Items.Add(item);
            if(!this.MenuItemGroups.ContainsKey(key)) {
                this.MenuItemGroups[key] = new List<ToolStripItem>();
            }
            this.MenuItemGroups[key].Add(item);
        }

        public void AddItemsRange(ToolStripItem[] items, string key) {
            this.Items.AddRange(items);
            if(!this.MenuItemGroups.ContainsKey(key)) {
                this.MenuItemGroups[key] = new List<ToolStripItem>();
            }
            this.MenuItemGroups[key].AddRange(items);
        }

        public void AddItemsRangeVirtual(List<QMenuItem> lstItems) {
            if(lstItems.Count < 0x80) {
                this.fVirtualMode = false;
                this.Items.AddRange(lstItems.ToArray());
            }
            else {
                this.fVirtualMode = true;
                if(this.stcVirtualItems_Top == null) {
                    this.stcVirtualItems_Top = new Stack<ToolStripItem>();
                    this.stcVirtualItems_Bottom = new Stack<ToolStripItem>();
                }
                ToolStripMenuItem[] toolStripItems = new ToolStripMenuItem[0x40];
                for(int i = lstItems.Count - 1; i > -1; i--) {
                    if(i < 0x40) {
                        toolStripItems[i] = lstItems[i];
                    }
                    else {
                        this.stcVirtualItems_Bottom.Push(lstItems[i]);
                    }
                }
                this.Items.AddRange(toolStripItems);
            }
        }

        protected void CancelClosingAncestors(bool fCancel, bool fClose) {
            this.fCancelClosingAncestors = fCancel;
            DropDownMenuReorderable owner = null;
            if((base.OwnerItem != null) && (base.OwnerItem is ToolStripDropDownItem)) {
                owner = base.OwnerItem.Owner as DropDownMenuReorderable;
            }
            if(!fCancel && fClose) {
                base.Close(ToolStripDropDownCloseReason.ItemClicked);
            }
            if(owner != null) {
                owner.CancelClosingAncestors(fCancel, fClose);
            }
        }

        private void ChangeSelection(ToolStripItem tsi) {
            typeof(ToolStripDropDownMenu).GetMethod("ChangeSelection", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(this, new object[] { tsi });
            typeof(ToolStrip).GetMethod("ClearAllSelectionsExcept", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(this, new object[] { tsi });
        }

        private void CloseChildDropDown() {
            foreach(ToolStripItem item in this.Items) {
                ToolStripMenuItem item2 = item as ToolStripMenuItem;
                if(((item2 != null) && item2.HasDropDownItems) && item2.DropDown.Visible) {
                    item2.DropDown.Close(ToolStripDropDownCloseReason.AppFocusChange);
                    break;
                }
            }
        }

        private bool ContainsInIdenticalMenuGroup(ToolStripItem dragging, ToolStripItem covered) {
            if(!this.fReorderEnabled) {
                return false;
            }
            if(dragging is ToolStripSeparator) {
                return false;
            }
            string item = string.Empty;
            string str2 = string.Empty;
            foreach(string str3 in this.MenuItemGroups.Keys) {
                if(this.MenuItemGroups[str3].Contains(dragging)) {
                    item = str3;
                }
                if(this.MenuItemGroups[str3].Contains(covered)) {
                    str2 = str3;
                }
            }
            return (((item.Length != 0) && (str2.Length != 0)) && ((item == str2) && !this.lstProhibitedKeys.Contains(item)));
        }

        protected override void Dispose(bool disposing) {
            this.DisposeVirtual(true);
            base.Dispose(disposing);
        }

        private void DisposeVirtual(bool disposing) {
            if(this.stcVirtualItems_Top != null) {
                while(this.stcVirtualItems_Top.Count > 0) {
                    this.stcVirtualItems_Top.Pop().Dispose();
                }
            }
            if(this.stcVirtualItems_Bottom != null) {
                while(this.stcVirtualItems_Bottom.Count > 0) {
                    this.stcVirtualItems_Bottom.Pop().Dispose();
                }
            }
        }

        private void GetScrollButtons() {
            try {
                if(piScrollButtonUp == null) {
                    piScrollButtonUp = typeof(ToolStripDropDownMenu).GetProperty("UpScrollButton", BindingFlags.ExactBinding | BindingFlags.NonPublic | BindingFlags.Instance);
                }
                if(piScrollButtonDn == null) {
                    piScrollButtonDn = typeof(ToolStripDropDownMenu).GetProperty("DownScrollButton", BindingFlags.ExactBinding | BindingFlags.NonPublic | BindingFlags.Instance);
                }
                this.upButton = (ToolStripControlHost)piScrollButtonUp.GetValue(this, null);
                this.downButton = (ToolStripControlHost)piScrollButtonDn.GetValue(this, null);
            }
            catch {
            }
        }

        private bool HandleArrowKeyVirtual(bool fUp) {
            int num = -1;
            for(int i = 0; i < this.Items.Count; i++) {
                if(this.Items[i].Selected) {
                    num = i;
                }
            }
            if(num != -1) {
                bool flag = (fUp && (this.stcVirtualItems_Top.Count == 0)) || (!fUp && (this.stcVirtualItems_Bottom.Count == 0));
                bool flag2 = (fUp && (num == 0)) || (!fUp && (num == (this.Items.Count - 1)));
                bool flag3 = ((fUp && (-1 < num)) && (num < 2)) || ((!fUp && (-1 < num)) && (num > (this.Items.Count - 3)));
                if(!fUp) {
                    ToolStripItem nextItem = null;
                    if(flag2) {
                        if(flag) {
                            this.ScrollEndVirtual(!fUp);
                            this.Select(true, !fUp);
                            return true;
                        }
                        this.fBlockItemAddRemove = true;
                        base.SuspendLayout();
                        ToolStripItem item2 = this.Items[0];
                        this.Items.RemoveAt(0);
                        this.stcVirtualItems_Top.Push(item2);
                        nextItem = this.stcVirtualItems_Bottom.Pop();
                        this.Items.Add(nextItem);
                        base.ResumeLayout();
                        this.fBlockItemAddRemove = false;
                    }
                    if(nextItem == null) {
                        nextItem = this.GetNextItem(this.Items[num], ArrowDirection.Down);
                    }
                    this.ChangeSelection(nextItem);
                    return true;
                }
                if(flag3) {
                    if(flag) {
                        if(flag2) {
                            this.ScrollEndVirtual(!fUp);
                            this.Select(true, !fUp);
                            return true;
                        }
                    }
                    else {
                        this.fBlockItemAddRemove = true;
                        base.SuspendLayout();
                        ToolStripItem item = this.Items[this.Items.Count - 1];
                        this.Items.RemoveAt(this.Items.Count - 1);
                        this.stcVirtualItems_Bottom.Push(item);
                        ToolStripItem item4 = this.stcVirtualItems_Top.Pop();
                        this.Items.Insert(0, item4);
                        base.ResumeLayout();
                        this.fBlockItemAddRemove = false;
                    }
                }
            }
            return false;
        }

        private void HandlePageKeys(Keys keys) {
            bool fUp = keys == Keys.Prior;
            int num = -1;
            int num2 = -1;
            int height = base.Height;
            Rectangle displayRectangle = this.DisplayRectangle;
            for(int i = 0; i < this.Items.Count; i++) {
                if(num == -1) {
                    if(this.Items[i].Bounds.Y >= displayRectangle.Y) {
                        num = i;
                    }
                }
                else if(displayRectangle.Bottom < this.Items[i].Bounds.Bottom) {
                    num2 = i - 1;
                    break;
                }
            }
            if(num2 == -1) {
                num2 = this.Items.Count - 1;
            }
            if((fUp && (num > -1)) && !this.Items[num].Selected) {
                this.Items[num].Select();
            }
            else if((!fUp && (num2 > -1)) && !this.Items[num2].Selected) {
                this.Items[num2].Select();
            }
            else {
                if(num != -1) {
                    int count = (num2 - num) + 1;
                    if(count > 0) {
                        int num5 = fUp ? (num - count) : (num2 + count);
                        if((-1 < num5) && (num5 < this.Items.Count)) {
                            this.ChangeSelection(this.Items[num5]);
                            return;
                        }
                        this.ScrollMenu(fUp, count);
                    }
                }
                for(int j = 0; j < this.Items.Count; j++) {
                    if(fUp && (this.Items[j].Bounds.Y >= displayRectangle.Y)) {
                        this.Items[j].Select();
                        return;
                    }
                    if(!fUp && (displayRectangle.Bottom < this.Items[j].Bounds.Bottom)) {
                        if(j > 0) {
                            this.Items[j - 1].Select();
                        }
                        return;
                    }
                }
                if(!fUp) {
                    this.Items[this.Items.Count - 1].Select();
                }
            }
        }

        private void HideToolTip() {
            if(base.ShowItemToolTips) {
                BindingFlags bindingAttr = BindingFlags.ExactBinding | BindingFlags.NonPublic | BindingFlags.Instance;
                try {
                    typeof(ToolStrip).GetMethod("UpdateToolTip", bindingAttr).Invoke(this, new object[1]);
                    System.Type type = System.Type.GetType("System.Windows.Forms.MouseHoverTimer, System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
                    PropertyInfo property = typeof(ToolStrip).GetProperty("MouseHoverTimer", bindingAttr);
                    type.GetMethod("Cancel", System.Type.EmptyTypes).Invoke(property.GetValue(this, null), null);
                }
                catch(Exception exception) {
                    QTUtility2.MakeErrorLog(exception, null);
                }
            }
        }

        public void InsertItem(int index, ToolStripItem item, string key) {
            this.Items.Insert(index, item);
            if(!this.MenuItemGroups.ContainsKey(key)) {
                this.MenuItemGroups[key] = new List<ToolStripItem>();
            }
            this.MenuItemGroups[key].Add(item);
        }

        private static bool IsPseudoMnemonic(char charCode, string text) {
            if(!string.IsNullOrEmpty(text)) {
                char ch = char.ToUpper(charCode, CultureInfo.CurrentCulture);
                if((char.ToUpper(text[0], CultureInfo.CurrentCulture) == ch) || (char.ToLower(charCode, CultureInfo.CurrentCulture) == char.ToLower(text[0], CultureInfo.CurrentCulture))) {
                    return true;
                }
            }
            return false;
        }

        public void ItemsClear() {
            this.MenuItemGroups.Clear();
            this.Items.Clear();
        }

        public void ItemsClearVirtual() {
            this.Items.Clear();
            this.DisposeVirtual(false);
        }

        protected override void OnClosing(ToolStripDropDownClosingEventArgs e) {
            this.UpdateToolTipByKey(null);
            this.draggingItem = null;
            this.fSuppressMouseMove_Scroll = false;
            this.fBlockItemAddRemove = false;
            this.fReordered = false;
            this.tsmiPreviousEdgeClicked = null;
            if(this.fCancelClosingAncestors) {
                e.Cancel = true;
            }
            base.OnClosing(e);
        }

        protected override void OnItemAdded(ToolStripItemEventArgs e) {
            if(!this.fBlockItemAddRemove) {
                base.OnItemAdded(e);
            }
        }

        protected override void OnItemClicked(ToolStripItemClickedEventArgs e) {
            if(this.fSuppressMouseMove) {
                this.fSuppressMouseMove = false;
                if(this.Items.IndexOf(e.ClickedItem) != this.iSuppressStartIndex) {
                    if(this.MouseUpBeforeDrop != null) {
                        this.MouseUpBeforeDrop(this, EventArgs.Empty);
                    }
                    return;
                }
                this.iSuppressStartIndex = -1;
            }
            if(this.fCheckOnEdgeClick) {
                ToolStripMenuItem clickedItem = e.ClickedItem as ToolStripMenuItem;
                if(clickedItem != null) {
                    Rectangle rectangle = base.RectangleToScreen(e.ClickedItem.Bounds);
                    if(this.RightToLeft == RightToLeft.Yes) {
                        int num = DropDownMenuBase.fImageMarginModified ? 0x1f : 0x19;
                        rectangle.X += rectangle.Width - num;
                        rectangle.Width = num;
                    }
                    else {
                        rectangle.Width = DropDownMenuBase.fImageMarginModified ? 0x1f : 0x19;
                    }
                    if(rectangle.Contains(Control.MousePosition)) {
                        clickedItem.Checked = !clickedItem.Checked;
                        if((Control.ModifierKeys == Keys.Shift) && (this.tsmiPreviousEdgeClicked != null)) {
                            int index = this.Items.IndexOf(clickedItem);
                            int num3 = this.Items.IndexOf(this.tsmiPreviousEdgeClicked);
                            if(((index != num3) && (index != -1)) && (num3 != -1)) {
                                int num4 = Math.Min(index, num3);
                                int num5 = Math.Max(index, num3);
                                if(num5 < this.Items.Count) {
                                    bool flag = clickedItem.Checked;
                                    for(int i = num4; i <= num5; i++) {
                                        ToolStripMenuItem item2 = this.Items[i] as ToolStripMenuItem;
                                        if(item2 != null) {
                                            item2.Checked = flag;
                                        }
                                    }
                                }
                            }
                        }
                        this.tsmiPreviousEdgeClicked = clickedItem;
                        return;
                    }
                }
            }
            if(!(e.ClickedItem is DirectoryMenuItem) || (((DirectoryMenuItem)e.ClickedItem).DropDownItems.Count != 0)) {
                base.OnItemClicked(e);
            }
        }

        protected virtual void OnItemMiddleClicked(ItemRightClickedEventArgs e) {
            if(this.ItemMiddleClicked != null) {
                this.ItemMiddleClicked(this, e);
                if(e.HRESULT != 0xffff) {
                    base.Close(ToolStripDropDownCloseReason.ItemClicked);
                }
            }
        }

        protected override void OnItemRemoved(ToolStripItemEventArgs e) {
            if(!this.fBlockItemAddRemove) {
                base.OnItemRemoved(e);
            }
        }

        protected override void OnKeyUp(KeyEventArgs e) {
            if((e.KeyCode == Keys.Apps) && (this.ItemRightClicked != null)) {
                foreach(ToolStripItem item in this.Items) {
                    if(item.Selected) {
                        if((item is ToolStripMenuItem) && ((ToolStripMenuItem)item).HasDropDownItems) {
                            ((ToolStripMenuItem)item).DropDown.Close(ToolStripDropDownCloseReason.AppFocusChange);
                        }
                        this.CancelClosingAncestors(true, false);
                        ItemRightClickedEventArgs args = new ItemRightClickedEventArgs(item, true, base.PointToScreen(new Point(item.Bounds.X, item.Bounds.Y + item.Height)));
                        this.ItemRightClicked(this, args);
                        this.CancelClosingAncestors(false, (args.HRESULT != 0xffff) && (args.HRESULT != 0xfffd));
                        break;
                    }
                }
            }
            else if((e.KeyCode == Keys.Space) && this.fSpaceKeyExecute) {
                foreach(ToolStripItem item2 in base.Items) {
                    if(item2.Selected) {
                        this.OnItemClicked(new ToolStripItemClickedEventArgs(item2));
                        return;
                    }
                }
            }
            base.OnKeyUp(e);
        }

        protected override void OnLayout(LayoutEventArgs e) {
            base.OnLayout(e);
            if(this.fEnableScroll) {
                try {
                    if(piScroll == null) {
                        piScroll = typeof(ToolStripDropDownMenu).GetProperty("RequiresScrollButtons", BindingFlags.ExactBinding | BindingFlags.NonPublic | BindingFlags.Instance);
                    }
                    this.fNowScrollButtonsRequired = (bool)piScroll.GetValue(this, null);
                    if(this.fNowScrollButtonsRequired && (this.upButton == null)) {
                        this.GetScrollButtons();
                    }
                }
                catch {
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            if(this.fReorderEnabled) {
                this.mouseButtons = e.Button;
                this.draggingItem = base.GetItemAt(e.Location);
                if((this.draggingItem != null) && string.Equals(this.draggingItem.GetType().ToString(), "System.Windows.Forms.ToolStripScrollButton")) {
                    this.draggingItem = null;
                }
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            if(((e.Button == this.mouseButtons) && (this.mouseButtons == MouseButtons.Left)) && (this.draggingItem != null)) {
                ToolStripItem itemAt = base.GetItemAt(e.Location);
                if((itemAt != null) && (itemAt != this.draggingItem)) {
                    if(this.ContainsInIdenticalMenuGroup(this.draggingItem, itemAt)) {
                        int index = this.Items.IndexOf(itemAt);
                        if(index != -1) {
                            if((this.draggingItem is ToolStripDropDownItem) && (((ToolStripDropDownItem)this.draggingItem).DropDownItems.Count > 0)) {
                                ((ToolStripDropDownItem)this.draggingItem).DropDown.Hide();
                            }
                            this.fBlockItemAddRemove = true;
                            base.SuspendLayout();
                            this.Items.Remove(this.draggingItem);
                            this.Items.Insert(index, this.draggingItem);
                            base.ResumeLayout();
                            this.fBlockItemAddRemove = false;
                            this.fReordered = true;
                        }
                    }
                    else {
                        this.fReordered = true;
                    }
                }
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e) {
            if((this.fReordered && (this.ReorderFinished != null)) && (this.draggingItem != null)) {
                if(this.fReorderEnabled) {
                    this.ReorderFinished(this, new ToolStripItemClickedEventArgs(this.draggingItem));
                }
                this.draggingItem = null;
            }
            ToolStripItem itemAt = base.GetItemAt(e.Location);
            if((!this.fReordered && (itemAt != null)) && !(itemAt is ToolStripSeparator)) {
                if(e.Button == MouseButtons.Right) {
                    if(this.ItemRightClicked != null) {
                        itemAt.Select();
                        if((itemAt is ToolStripMenuItem) && ((ToolStripMenuItem)itemAt).HasDropDownItems) {
                            ((ToolStripMenuItem)itemAt).DropDown.Close(ToolStripDropDownCloseReason.AppFocusChange);
                        }
                        this.CancelClosingAncestors(true, false);
                        ItemRightClickedEventArgs args = new ItemRightClickedEventArgs(itemAt, false, e.Location);
                        this.ItemRightClicked(this, args);
                        this.CancelClosingAncestors(false, (args.HRESULT != 0xffff) && (args.HRESULT != 0xfffd));
                    }
                }
                else {
                    base.OnMouseUp(e);
                    if(e.Button == MouseButtons.Middle) {
                        this.OnItemMiddleClicked(new ItemRightClickedEventArgs(itemAt, false, e.Location));
                    }
                }
            }
            else {
                this.fReordered = false;
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e) {
            if(this.fEnableScroll && this.fNowScrollButtonsRequired) {
                Point mousePosition = Control.MousePosition;
                if(base.Bounds.Contains(mousePosition)) {
                    this.ScrollMenu(e.Delta > 0, 3);
                }
                else {
                    Control control = Control.FromHandle(PInvoke.WindowFromPoint(mousePosition));
                    if(control != null) {
                        DropDownMenuReorderable reorderable = control as DropDownMenuReorderable;
                        if(reorderable != null) {
                            reorderable.OnMouseWheel(e);
                        }
                    }
                }
            }
            base.OnMouseWheel(e);
        }

        protected override void OnOpening(CancelEventArgs e) {
            if((base.OwnerItem != null) && ((base.OwnerItem.IsDisposed || (base.OwnerItem.Owner == null)) || base.OwnerItem.Owner.IsDisposed)) {
                e.Cancel = true;
            }
            else {
                base.OnOpening(e);
            }
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e) {
            switch(e.KeyCode) {
                case Keys.Prior:
                case Keys.Next:
                    this.HandlePageKeys(e.KeyCode);
                    e.IsInputKey = true;
                    return;

                case Keys.End:
                case Keys.Home:
                    if(this.fVirtualMode) {
                        this.ScrollEndVirtual(e.KeyCode == Keys.Home);
                    }
                    goto Label_014F;

                case Keys.Right:
                    if(!(base.OwnerItem is ToolStripDropDownButton)) {
                        foreach(ToolStripItem item in this.Items) {
                            if(item.Selected) {
                                if((item is ToolStripMenuItem) && !((ToolStripMenuItem)item).HasDropDownItems) {
                                    e.IsInputKey = true;
                                }
                                break;
                            }
                        }
                    }
                    goto Label_014F;

                case Keys.Return:
                    if(!this.fCancelClosingAncestors) {
                        foreach(ToolStripItem item2 in this.Items) {
                            if(item2.Selected) {
                                QMenuItem item3 = item2 as QMenuItem;
                                if((item3 != null) && item3.HasDropDownItems) {
                                    e.IsInputKey = true;
                                    item2.PerformClick();
                                }
                                break;
                            }
                        }
                        goto Label_014F;
                    }
                    e.IsInputKey = true;
                    break;

                default:
                    goto Label_014F;
            }
            return;
        Label_014F:
            base.OnPreviewKeyDown(e);
        }

        public override bool PreProcessMessage(ref Message msg) {
            if(this.fCancelClosingAncestors) {
                return false;
            }
            if(msg.Msg == 0x100) {
                Keys wParam = (Keys)((int)((long)msg.WParam));
                switch(wParam) {
                    case Keys.Up:
                    case Keys.Down:
                        if(this.MouseScroll != null) {
                            this.MouseScroll(this, EventArgs.Empty);
                        }
                        if(this.fVirtualMode && this.HandleArrowKeyVirtual(wParam == Keys.Up)) {
                            return true;
                        }
                        break;
                }
            }
            return base.PreProcessMessage(ref msg);
        }

        protected override bool ProcessMnemonic(char charCode) {
            if((base.Visible && base.Enabled) && !this.fCancelClosingAncestors) {
                ToolStripItem item = null;
                int num = 0;
                for(int i = 0; i < this.DisplayedItems.Count; i++) {
                    if(this.DisplayedItems[i].Selected) {
                        item = this.DisplayedItems[i];
                        num = i;
                        break;
                    }
                }
                ToolStripItem item2 = null;
                int num3 = num;
                for(int j = 0; j < this.DisplayedItems.Count; j++) {
                    ToolStripItem item3 = this.DisplayedItems[num3];
                    num3 = (num3 + 1) % this.DisplayedItems.Count;
                    if((((item3 is ToolStripMenuItem) && !string.IsNullOrEmpty(item3.Text)) && (item3.Enabled && ((item3.DisplayStyle & ToolStripItemDisplayStyle.Text) == ToolStripItemDisplayStyle.Text))) && IsPseudoMnemonic(charCode, item3.Text)) {
                        if(item2 != null) {
                            if(item2 == item) {
                                item3.Select();
                            }
                            else {
                                item2.Select();
                            }
                            return true;
                        }
                        item2 = item3;
                    }
                }
                if(item2 != null) {
                    item2.PerformClick();
                    return true;
                }
            }
            return false;
        }

        private void ScrollEndVirtual(bool fUp) {
            this.fBlockItemAddRemove = true;
            base.SuspendLayout();
            if(fUp) {
                while(this.stcVirtualItems_Top.Count > 0) {
                    this.Items.Insert(0, this.stcVirtualItems_Top.Pop());
                }
                while(this.Items.Count > 0x40) {
                    ToolStripItem item = this.Items[this.Items.Count - 1];
                    this.Items.RemoveAt(this.Items.Count - 1);
                    this.stcVirtualItems_Bottom.Push(item);
                }
            }
            else {
                List<ToolStripItem> list = new List<ToolStripItem>();
                while(this.stcVirtualItems_Bottom.Count > 0) {
                    list.Add(this.stcVirtualItems_Bottom.Pop());
                }
                this.Items.AddRange(list.ToArray());
                while(this.Items.Count > 0x40) {
                    ToolStripItem item2 = this.Items[0];
                    this.Items.RemoveAt(0);
                    this.stcVirtualItems_Top.Push(item2);
                }
            }
            base.ResumeLayout();
            this.Refresh();
            this.fBlockItemAddRemove = false;
        }

        protected void ScrollMenu(bool fUp, int count) {
            this.fSuppressMouseMove_Scroll = true;
            this.fSuppressMouseMoveOnce = true;
            this.HideToolTip();
            base.SuspendLayout();
            int num = this.ScrollMenuCore(fUp, count);
            if((num < count) && this.fVirtualMode) {
                this.ScrollMenuVirtual(fUp, count - num);
            }
            base.ResumeLayout();
            this.Refresh();
            this.fSuppressMouseMove_Scroll = false;
            if(this.MouseScroll != null) {
                this.MouseScroll(this, EventArgs.Empty);
            }
        }

        private int ScrollMenuCore(bool fUp, int count) {
            if(count >= 1) {
                ToolStripControlHost host = fUp ? this.upButton : this.downButton;
                if((host != null) && host.Visible) {
                    Control control = host.Control;
                    if((control != null) && control.Enabled) {
                        this.CloseChildDropDown();
                        if(miScroll == null) {
                            miScroll = typeof(ToolStripDropDownMenu).GetMethod("ScrollInternal", BindingFlags.ExactBinding | BindingFlags.NonPublic | BindingFlags.Instance, null, new System.Type[] { typeof(bool) }, null);
                        }
                        base.fSuspendPainting = true;
                        try {
                            miScroll.Invoke(this, new object[] { fUp });
                            for(int i = 1; i < count; i++) {
                                if(control.Enabled) {
                                    miScroll.Invoke(this, new object[] { fUp });
                                }
                                else {
                                    return i;
                                }
                            }
                            return count;
                        }
                        finally {
                            base.fSuspendPainting = false;
                        }
                    }
                }
            }
            return 0;
        }

        private bool ScrollMenuVirtual(bool fUp, int count) {
            if((fUp && (this.stcVirtualItems_Top.Count == 0)) || (!fUp && (this.stcVirtualItems_Bottom.Count == 0))) {
                return false;
            }
            this.fBlockItemAddRemove = true;
            this.CloseChildDropDown();
            for(int i = 0; i < count; i++) {
                if(fUp) {
                    ToolStripItem item = this.Items[this.Items.Count - 1];
                    this.Items.RemoveAt(this.Items.Count - 1);
                    this.stcVirtualItems_Bottom.Push(item);
                    ToolStripItem item2 = this.stcVirtualItems_Top.Pop();
                    this.Items.Insert(0, item2);
                    if(this.stcVirtualItems_Top.Count != 0) {
                        continue;
                    }
                    break;
                }
                ToolStripItem item3 = this.Items[0];
                this.Items.RemoveAt(0);
                this.stcVirtualItems_Top.Push(item3);
                ToolStripItem item4 = this.stcVirtualItems_Bottom.Pop();
                this.Items.Add(item4);
                if(this.stcVirtualItems_Bottom.Count == 0) {
                    break;
                }
            }
            this.fBlockItemAddRemove = false;
            return true;
        }

        public void UpdateToolTipByKey(ToolStripMenuItem item) {
            try {
                if(base.ShowItemToolTips) {
                    ToolTip tip = typeof(ToolStrip).GetProperty("ToolTip", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this, null) as ToolTip;
                    if(tip != null) {
                        tip.Hide(this);
                        tip.Active = false;
                        if((item != null) && !string.IsNullOrEmpty(item.ToolTipText)) {
                            tip.Active = true;
                            Point point = new Point(item.Bounds.Width / 2, item.Bounds.Top + 0x20);
                            tip.Show(item.ToolTipText, this, point, tip.AutoPopDelay);
                        }
                    }
                }
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception, null);
            }
        }

        protected override void WndProc(ref Message m) {
            if((this.MessageParent != IntPtr.Zero) && (((m.Msg == 0x117) || (m.Msg == 0x2b)) || (m.Msg == 0x2c))) {
                PInvoke.SendMessage(this.MessageParent, (uint)m.Msg, m.WParam, m.LParam);
            }
            else {
                if(m.Msg == 0x2a3) {
                    if(this.fCancelClosingAncestors) {
                        return;
                    }
                }
                else if(m.Msg == 0x200) {
                    if(this.fSuppressMouseMove_Scroll) {
                        m.Result = IntPtr.Zero;
                        return;
                    }
                    if(this.fSuppressMouseMove) {
                        if(this.MouseDragMove != null) {
                            this.MouseDragMove(this, new MouseEventArgs(Control.MouseButtons, 0, QTUtility2.GET_X_LPARAM(m.LParam), QTUtility2.GET_Y_LPARAM(m.LParam), 0));
                        }
                        return;
                    }
                    if(this.fSuppressMouseMoveOnce) {
                        this.fSuppressMouseMoveOnce = false;
                        return;
                    }
                }
                base.WndProc(ref m);
            }
        }

        public bool CanScroll {
            get {
                return (this.fEnableScroll && this.fNowScrollButtonsRequired);
            }
        }

        public bool CheckOnEdgeClick {
            set {
                this.fCheckOnEdgeClick = value;
            }
        }

        public string Path {
            get {
                return this.pathParent;
            }
            set {
                this.pathParent = value;
            }
        }

        public IList<string> ProhibitedKey {
            get {
                return this.lstProhibitedKeys;
            }
        }

        public bool ReorderEnabled {
            set {
                this.fReorderEnabled = value;
            }
        }

        public bool SpaceKeyExecute {
            set {
                this.fSpaceKeyExecute = value;
            }
        }

        public bool SuppressMouseMove {
            set {
                this.fSuppressMouseMove = value;
            }
        }

        public bool SuppressMouseMoveOnce {
            set {
                this.fSuppressMouseMoveOnce = value;
            }
        }

        public int SuppressStartIndex {
            set {
                this.iSuppressStartIndex = value;
            }
        }
    }
}
