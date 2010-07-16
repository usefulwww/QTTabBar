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
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    internal class DropDownMenuBase : ToolStripDropDownMenu {
        private bool fChangeImageSelected;
        protected bool fEnableShiftKey;
        private static bool fFirstDropDownOpened;
        protected static bool fImageMarginModified;
        private bool fOnceKeyDown;
        private static bool fRendererInitialized;
        protected bool fRespondModKeys;
        protected bool fSuspendPainting;
        public const string IMAGEKEY_BACK = "back";
        private const string IMAGEKEY_CONTROL = "control";
        public const string IMAGEKEY_CURRENT = "current";
        public const string IMAGEKEY_FORWARD = "forward";
        private const string IMAGEKEY_SHIFT = "shift";
        private QMenuItem lastKeyImageChangedItem;
        private QMenuItem lastMouseActiveItem;
        private IntPtr lparamPreviousMouseMove;
        private List<QMenuItem> lstQMIResponds;
        private static ToolStripRenderer menuRenderer;
        private static int nCurrentRenderer = -1;

        private static event EventHandler menuRendererChanged;

        public DropDownMenuBase(IContainer container) {
            this.lstQMIResponds = new List<QMenuItem>();
            if(!fRendererInitialized) {
                fRendererInitialized = true;
                InitializeMenuRenderer();
            }
            if(container != null) {
                container.Add(this);
            }
            base.Renderer = menuRenderer;
            menuRendererChanged = (EventHandler)Delegate.Combine(menuRendererChanged, new EventHandler(this.DropDownMenuBase_menuRendererChanged));
        }

        public DropDownMenuBase(IContainer container, bool fRespondModKeys, bool fEnableShiftKey)
            : this(container) {
            this.fRespondModKeys = fRespondModKeys;
            this.fEnableShiftKey = fEnableShiftKey;
        }

        public DropDownMenuBase(IContainer container, bool fRespondModKeys, bool fEnableShiftKey, bool fChangeImageSelected)
            : this(container) {
            this.fRespondModKeys = fRespondModKeys;
            this.fEnableShiftKey = fEnableShiftKey;
            this.fChangeImageSelected = fChangeImageSelected;
        }

        protected override void Dispose(bool disposing) {
            menuRendererChanged = (EventHandler)Delegate.Remove(menuRendererChanged, new EventHandler(this.DropDownMenuBase_menuRendererChanged));
            this.lstQMIResponds.Clear();
            base.Dispose(disposing);
        }

        private void DropDownMenuBase_menuRendererChanged(object sender, EventArgs e) {
            if(base.InvokeRequired) {
                base.Invoke(new MethodInvoker(this.RefreshRenderer));
            }
            else {
                this.RefreshRenderer();
            }
        }

        public static void ExitMenuMode() {
            Type type = Type.GetType("System.Windows.Forms.ToolStripManager+ModalMenuFilter, System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            if(type != null) {
                type.GetMethod("ExitMenuMode", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);
            }
        }

        public static bool InitializeMenuRenderer() {
            bool flag = false;
            bool fVista = false;
            if(QTUtility.CheckConfig(Settings.NonDefaultMenu)) {
                if(QTUtility.CheckConfig(Settings.XPStyleMenus)) {
                    if(nCurrentRenderer != 1) {
                        menuRenderer = new XPMenuRenderer(true);
                        nCurrentRenderer = 1;
                        flag = true;
                    }
                }
                else if(nCurrentRenderer != 2) {
                    menuRenderer = new VistaMenuRenderer(true);
                    nCurrentRenderer = 2;
                    flag = fVista = true;
                }
            }
            else if(nCurrentRenderer != 0) {
                menuRenderer = new DefaultMenuRenderer();
                nCurrentRenderer = 0;
                flag = true;
            }
            if(flag) {
                SetImageMargin(fVista);
                if(menuRendererChanged != null) {
                    menuRendererChanged(null, EventArgs.Empty);
                }
            }
            return flag;
        }

        private static bool IsCursorOnTheEdgeOfScreen(out AnchorStyles dir, out int marginFar) {
            Point mousePosition = MousePosition;
            Rectangle workingArea = Screen.FromPoint(mousePosition).WorkingArea;
            dir = AnchorStyles.None;
            marginFar = workingArea.Right - mousePosition.X;
            if(marginFar < 0x80) {
                dir |= AnchorStyles.Right;
            }
            if((workingArea.Bottom - mousePosition.Y) < 0x80) {
                dir |= AnchorStyles.Bottom;
            }
            return (dir != AnchorStyles.None);
        }

        private static bool IsQmiResponds(QMenuItem qmi) {
            return ((((qmi != null) && (qmi.Genre != MenuGenre.Application)) && (qmi.Genre != MenuGenre.RecentFile)) && (qmi.Target != MenuTarget.File));
        }

        protected override void OnClosed(ToolStripDropDownClosedEventArgs e) {
            this.fOnceKeyDown = false;
            this.ResetImageKeys();
            base.OnClosed(e);
        }

        protected override void OnItemAdded(ToolStripItemEventArgs e) {
            QMenuItem qmi = e.Item as QMenuItem;
            if(IsQmiResponds(qmi)) {
                this.lstQMIResponds.Add(qmi);
            }
            base.OnItemAdded(e);
        }

        protected override void OnItemClicked(ToolStripItemClickedEventArgs e) {
            if(e.ClickedItem.ImageKey != "current") {
                base.OnItemClicked(e);
            }
        }

        protected override void OnItemRemoved(ToolStripItemEventArgs e) {
            QMenuItem qmi = e.Item as QMenuItem;
            if(IsQmiResponds(qmi)) {
                this.lstQMIResponds.Remove(qmi);
            }
            base.OnItemRemoved(e);
        }

        protected override void OnOpening(CancelEventArgs e) {
            if(!fFirstDropDownOpened) {
                QTUtility.ImageListGlobal.Images.Add("control", Resources_Image.imgNewWindow);
                QTUtility.ImageListGlobal.Images.Add("shift", Resources_Image.imgNewTab);
                QTUtility.ImageListGlobal.Images.Add("back", Resources_Image.imgBack);
                QTUtility.ImageListGlobal.Images.Add("forward", Resources_Image.imgForward);
                QTUtility.ImageListGlobal.Images.Add("current", Resources_Image.imgCurrent);
                fFirstDropDownOpened = true;
            }
            base.OnOpening(e);
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e) {
            if(!this.fRespondModKeys) {
                base.OnPreviewKeyDown(e);
                return;
            }
            if(((this.fEnableShiftKey && e.Shift) || (e.Control || this.fChangeImageSelected)) && ((e.KeyCode == Keys.Down) || (e.KeyCode == Keys.Up))) {
                base.SuspendLayout();
                try {
                    int index;
                    ToolStripItem item = null;
                    foreach(ToolStripItem item2 in this.Items) {
                        if(item2.Selected) {
                            item = item2;
                            break;
                        }
                    }
                    if(item != null) {
                        QMenuItem item3 = item as QMenuItem;
                        if(item3 != null) {
                            if((item3.Genre == MenuGenre.Application) || (item3.Genre == MenuGenre.RecentFile)) {
                                base.OnPreviewKeyDown(e);
                                return;
                            }
                            item3.RestoreOriginalImage();
                        }
                        index = this.Items.IndexOf(item);
                    }
                    else if(e.KeyCode == Keys.Down) {
                        index = this.Items.Count - 1;
                    }
                    else {
                        index = 0;
                    }
                    if(index != -1) {
                        int num2;
                        if(e.KeyCode == Keys.Down) {
                            if(index == (this.Items.Count - 1)) {
                                num2 = 0;
                            }
                            else {
                                num2 = index + 1;
                            }
                            for(int i = 0; (this.Items[num2] is ToolStripSeparator) && (i < this.Items.Count); i++) {
                                if(num2 == (this.Items.Count - 1)) {
                                    num2 = 0;
                                }
                                else {
                                    num2++;
                                }
                            }
                        }
                        else {
                            if(index == 0) {
                                num2 = this.Items.Count - 1;
                            }
                            else {
                                num2 = index - 1;
                            }
                            for(int j = 0; (this.Items[num2] is ToolStripSeparator) && (j < this.Items.Count); j++) {
                                if(num2 == 0) {
                                    num2 = this.Items.Count - 1;
                                }
                                else {
                                    num2--;
                                }
                            }
                        }
                        if(this.Items[num2].Enabled) {
                            QMenuItem item4 = this.Items[num2] as QMenuItem;
                            if(((item4 != null) && (item4.Genre != MenuGenre.Application)) && (item4.Target == MenuTarget.Folder)) {
                                switch(ModifierKeys) {
                                    case Keys.Control:
                                        item4.ImageKey = "control";
                                        goto Label_0254;

                                    case Keys.Shift:
                                        item4.ImageKey = "shift";
                                        goto Label_0254;
                                }
                                item4.RestoreOriginalImage(false, item4.Genre == MenuGenre.Navigation);
                            }
                        }
                    }
                }
                finally {
                    base.ResumeLayout(false);
                }
            }
        Label_0254:
            base.OnPreviewKeyDown(e);
        }

        public override bool PreProcessMessage(ref Message msg) {
            if(msg.Msg == WM.KEYDOWN) {
                Keys wParam = (Keys)((int)((long)msg.WParam));
                if((wParam == Keys.Escape) && ((base.OwnerItem == null) || !(base.OwnerItem is ToolStripMenuItem))) {
                    base.Close(ToolStripDropDownCloseReason.Keyboard);
                    base.PreProcessMessage(ref msg);
                    return true;
                }
            }
            return base.PreProcessMessage(ref msg);
        }

        private void RefreshRenderer() {
            base.Renderer = menuRenderer;
        }

        private void ResetImageKeys() {
            if(this.fRespondModKeys) {
                base.SuspendLayout();
                foreach(QMenuItem item in this.lstQMIResponds) {
                    item.RestoreOriginalImage();
                }
                base.ResumeLayout(false);
            }
        }

        private static void SetImageMargin(bool fVista) {
            if(fImageMarginModified != fVista) {
                fImageMarginModified = fVista;
                try {
                    typeof(ToolStripDropDownMenu).GetField("DefaultImageMarginWidth", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, fVista ? 0x1f : 0x19);
                }
                catch {
                }
            }
        }

        public bool UpdateToolTip_OnTheEdge(ToolStripItem item) {
            AnchorStyles styles;
            int num;
            if(base.ShowItemToolTips && IsCursorOnTheEdgeOfScreen(out styles, out num)) {
                try {
                    FieldInfo field = typeof(ToolStrip).GetField("currentlyActiveTooltipItem", BindingFlags.NonPublic | BindingFlags.Instance);
                    ToolStripItem item2 = (ToolStripItem)field.GetValue(this);
                    ToolTip tip = (ToolTip)typeof(ToolStrip).GetProperty("ToolTip", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this, null);
                    if(tip != null) {
                        if(item != item2) {
                            tip.Hide(this);
                            tip.Active = false;
                            field.SetValue(this, item);
                            IntPtr handle = PInvoke.GetCursor();
                            if(handle != IntPtr.Zero) {
                                Cursor cursor = new Cursor(handle);
                                tip.Active = true;
                                Point position = Cursor.Position;
                                position.Y += this.Cursor.Size.Height - cursor.HotSpot.Y;
                                using(Font font = SystemFonts.StatusFont) {
                                    using(Graphics graphics = base.CreateGraphics()) {
                                        SizeF ef = graphics.MeasureString(item.ToolTipText, font);
                                        if((num < ef.Width) || (styles == (AnchorStyles.Right | AnchorStyles.Bottom))) {
                                            position.X = base.Bounds.X - ((int)ef.Width);
                                        }
                                        else {
                                            position.X += 0x20;
                                        }
                                    }
                                }
                                tip.Show(item.ToolTipText, this, base.PointToClient(position), tip.AutoPopDelay);
                            }
                        }
                        return true;
                    }
                }
                catch {
                }
            }
            return false;
        }

        protected override void WndProc(ref Message m) {
            try {
                QMenuItem ownerItem;
                if(!this.fRespondModKeys) {
                    base.WndProc(ref m);
                    return;
                }
                int wParam = (int)((long)m.WParam);
                switch(m.Msg) {
                    case WM.KEYDOWN:
                        break;

                    case WM.KEYUP:
                        if(this.fOnceKeyDown && ((wParam == 0x10) || (wParam == 0x11))) {
                            bool flag2 = false;
                            foreach(QMenuItem item4 in this.lstQMIResponds) {
                                if(!item4.Selected) {
                                    continue;
                                }
                                if(item4.Enabled) {
                                    Keys modifierKeys = ModifierKeys;
                                    if(modifierKeys == Keys.Control) {
                                        item4.ImageKey = "control";
                                    }
                                    else if(this.fEnableShiftKey && (modifierKeys == Keys.Shift)) {
                                        item4.ImageKey = "shift";
                                    }
                                    else {
                                        item4.RestoreOriginalImage(this.fChangeImageSelected, false);
                                    }
                                    this.lastKeyImageChangedItem = item4;
                                }
                                flag2 = true;
                                break;
                            }
                            ownerItem = base.OwnerItem as QMenuItem;
                            if(ownerItem != null) {
                                DropDownMenuBase owner = ownerItem.Owner as DropDownMenuBase;
                                if((owner != null) && owner.Visible) {
                                    if(flag2) {
                                        PInvoke.SendMessage(owner.Handle, 0x2a3, IntPtr.Zero, IntPtr.Zero);
                                    }
                                    else {
                                        QTUtility2.SendCOPYDATASTRUCT(owner.Handle, (IntPtr)wParam, string.Empty, (IntPtr)1);
                                    }
                                }
                            }
                        }
                        goto Label_07C2;

                    case WM.MOUSEMOVE:
                        goto Label_0562;

                    case WM.MOUSELEAVE:
                        goto Label_072E;

                    case WM.PAINT:
                        if(this.fSuspendPainting) {
                            PInvoke.ValidateRect(m.HWnd, IntPtr.Zero);
                        }
                        else {
                            base.WndProc(ref m);
                        }
                        return;

                    case WM.COPYDATA: {
                            COPYDATASTRUCT copydatastruct = (COPYDATASTRUCT)Marshal.PtrToStructure(m.LParam, typeof(COPYDATASTRUCT));
                            ownerItem = base.GetItemAt(base.PointToClient(MousePosition)) as QMenuItem;
                            if(!(copydatastruct.dwData == IntPtr.Zero)) {
                                goto Label_04B6;
                            }
                            if(ownerItem == null) {
                                goto Label_0462;
                            }
                            Keys keys3 = ModifierKeys;
                            if((wParam == 0x11) && ((keys3 & Keys.Shift) != Keys.Shift)) {
                                ownerItem.ImageKey = "control";
                            }
                            else if((this.fEnableShiftKey && (wParam == 0x10)) && ((keys3 & Keys.Control) != Keys.Control)) {
                                ownerItem.ImageKey = "shift";
                            }
                            else {
                                ownerItem.RestoreOriginalImage(this.fChangeImageSelected, false);
                            }
                            this.lastKeyImageChangedItem = ownerItem;
                            goto Label_07C2;
                        }
                    default:
                        goto Label_07C2;
                }
                this.fOnceKeyDown = true;
                if((((int)((long)m.LParam)) & 0x40000000) == 0) {
                    if((wParam == 0x10) || (wParam == 0x11)) {
                        bool flag = false;
                        foreach(QMenuItem item2 in this.lstQMIResponds) {
                            if(!item2.Selected) {
                                continue;
                            }
                            if(item2.Enabled) {
                                Keys keys = ModifierKeys;
                                if((wParam == 0x11) && ((keys & Keys.Shift) != Keys.Shift)) {
                                    item2.ImageKey = "control";
                                }
                                else if((this.fEnableShiftKey && (wParam == 0x10)) && ((keys & Keys.Control) != Keys.Control)) {
                                    item2.ImageKey = "shift";
                                }
                                else {
                                    item2.RestoreOriginalImage(this.fChangeImageSelected, false);
                                }
                                this.lastKeyImageChangedItem = item2;
                            }
                            flag = true;
                            break;
                        }
                        ownerItem = base.OwnerItem as QMenuItem;
                        if(ownerItem != null) {
                            DropDownMenuBase base2 = ownerItem.Owner as DropDownMenuBase;
                            if((base2 != null) && base2.Visible) {
                                if(flag) {
                                    PInvoke.SendMessage(base2.Handle, 0x2a3, IntPtr.Zero, IntPtr.Zero);
                                }
                                else {
                                    QTUtility2.SendCOPYDATASTRUCT(base2.Handle, (IntPtr)wParam, string.Empty, IntPtr.Zero);
                                }
                            }
                        }
                    }
                    else if((wParam == 13) && ((this.fEnableShiftKey && (ModifierKeys == Keys.Shift)) || (ModifierKeys == Keys.Control))) {
                        foreach(ToolStripItem item3 in this.Items) {
                            if(item3.Selected) {
                                if(item3.Enabled) {
                                    this.OnItemClicked(new ToolStripItemClickedEventArgs(item3));
                                }
                                break;
                            }
                        }
                    }
                }
                goto Label_07C2;
            Label_0462:
                ownerItem = base.OwnerItem as QMenuItem;
                if(ownerItem != null) {
                    DropDownMenuBase base4 = ownerItem.Owner as DropDownMenuBase;
                    if((base4 != null) && base4.Visible) {
                        QTUtility2.SendCOPYDATASTRUCT(base4.Handle, (IntPtr)wParam, string.Empty, IntPtr.Zero);
                    }
                }
                goto Label_07C2;
            Label_04B6:
                if(ownerItem != null) {
                    Keys keys4 = ModifierKeys;
                    if(keys4 == Keys.Control) {
                        ownerItem.ImageKey = "control";
                    }
                    else if(this.fEnableShiftKey && (keys4 == Keys.Shift)) {
                        ownerItem.ImageKey = "shift";
                    }
                    else {
                        ownerItem.RestoreOriginalImage(this.fChangeImageSelected, false);
                    }
                    this.lastKeyImageChangedItem = ownerItem;
                }
                else {
                    ownerItem = base.OwnerItem as QMenuItem;
                    if(ownerItem != null) {
                        DropDownMenuBase base5 = ownerItem.Owner as DropDownMenuBase;
                        if((base5 != null) && base5.Visible) {
                            QTUtility2.SendCOPYDATASTRUCT(base5.Handle, (IntPtr)wParam, string.Empty, (IntPtr)1);
                        }
                    }
                }
                goto Label_07C2;
            Label_0562:
                if((m.WParam == IntPtr.Zero) && (m.LParam == this.lparamPreviousMouseMove)) {
                    m.Result = IntPtr.Zero;
                    return;
                }
                this.lparamPreviousMouseMove = m.LParam;
                if((!this.fEnableShiftKey || ((wParam & 4) != 4)) && (((wParam & 8) != 8) && !this.fChangeImageSelected)) {
                    goto Label_07C2;
                }
                ToolStripItem itemAt = base.GetItemAt(new Point(QTUtility2.GET_X_LPARAM(m.LParam), QTUtility2.GET_Y_LPARAM(m.LParam)));
                if(itemAt == null) {
                    base.WndProc(ref m);
                    return;
                }
                ownerItem = itemAt as QMenuItem;
                if(!IsQmiResponds(ownerItem)) {
                    goto Label_06F8;
                }
                if(ownerItem == this.lastMouseActiveItem) {
                    goto Label_07C2;
                }
                if(this.lstQMIResponds.Count > 0x1c) {
                    this.fSuspendPainting = true;
                }
                base.SuspendLayout();
                if(ownerItem.Enabled) {
                    switch(wParam) {
                        case 8:
                            ownerItem.ImageKey = "control";
                            goto Label_06AB;

                        case 4:
                            ownerItem.ImageKey = "shift";
                            goto Label_06AB;
                    }
                    if(((ownerItem.Genre == MenuGenre.Navigation) && (ownerItem.MenuItemArguments != null)) && (!ownerItem.MenuItemArguments.IsBack || (ownerItem.MenuItemArguments.Index != 0))) {
                        ownerItem.ImageKey = ownerItem.MenuItemArguments.IsBack ? "back" : "forward";
                    }
                    else {
                        ownerItem.RestoreOriginalImage();
                    }
                }
            Label_06AB:
                if(this.lastMouseActiveItem != null) {
                    this.lastMouseActiveItem.RestoreOriginalImage();
                }
                if((ownerItem != this.lastKeyImageChangedItem) && (this.lastKeyImageChangedItem != null)) {
                    this.lastKeyImageChangedItem.RestoreOriginalImage();
                    this.lastKeyImageChangedItem = null;
                }
                this.lastMouseActiveItem = ownerItem;
                this.fSuspendPainting = false;
                base.ResumeLayout(false);
                goto Label_07C2;
            Label_06F8:
                if(this.lastMouseActiveItem != null) {
                    this.lastMouseActiveItem.RestoreOriginalImage();
                    this.lastMouseActiveItem = null;
                }
                if(this.lastKeyImageChangedItem != null) {
                    this.lastKeyImageChangedItem.RestoreOriginalImage();
                    this.lastKeyImageChangedItem = null;
                }
                goto Label_07C2;
            Label_072E:
                this.ResetImageKeys();
                this.lastMouseActiveItem = null;
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception, "MSG:" + m.Msg.ToString("X") + ", WPARAM:" + m.WParam.ToString("X") + ", LPARAM:" + m.LParam.ToString("X"));
            }
        Label_07C2:
            base.WndProc(ref m);
            this.fSuspendPainting = false;
        }

        public static ToolStripRenderer CurrentRenderer {
            get {
                return menuRenderer;
            }
        }
    }
}
