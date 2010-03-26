//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2010  Paul Accisano
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
    using QTTabBarLib.Interop;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    internal sealed class DropDownMenuDropTarget : DropDownMenuReorderable {
        private Bitmap bmpInsertL;
        private Bitmap bmpInsertR;
        private DropTargetWrapper dropTargetWrapper;
        private static bool fContainsFileDropList;
        private bool fDrawDropTarget;
        private bool fDrivesContained;
        private bool fEnableShiftKeyTemp;
        private bool fEnterVirtualBottom;
        private bool fIsRootMenu;
        private bool fKeyTargetIsThis;
        private bool fRespondModKeysTemp;
        private bool fShownByKey;
        private bool fSuppressMouseUp;
        private bool fThisPathExists;
        private bool fTop;
        private IntPtr hwndDialogParent;
        private int iDDRetval;
        private int iItemDragOverRegion;
        private int iScrollLine;
        private ToolStripItem itemHover;
        private ToolStripItem itemKeyInsertionMarkPrev;
        private MethodInfo miUnselect;
        private string strDraggingDrive;
        private string strDraggingStartPath;
        private static string strExtExecutable;
        private string strTargetPath;
        private Timer timerScroll;

        public event EventHandler MenuDragEnter;

        public DropDownMenuDropTarget(IContainer container, bool respondModKeys, bool enableShiftKey, bool isRoot, IntPtr hwndDialogParent)
            : base(container, respondModKeys, enableShiftKey, false) {
            this.iDDRetval = -1;
            this.iItemDragOverRegion = -1;
            this.fIsRootMenu = isRoot;
            this.hwndDialogParent = hwndDialogParent;
            base.HandleCreated += new EventHandler(this.DropDownMenuDropTarget_HandleCreated);
        }

        private void BeginScrollTimer(ToolStripItem item, Point pntClient) {
            int y = pntClient.Y;
            int height = item.Bounds.Height;
            if(base.CanScroll && ((y < ((height * 0.5) + 11.0)) || ((base.Height - (height + 11)) < y))) {
                if(this.timerScroll == null) {
                    this.timerScroll = new Timer();
                    this.timerScroll.Tick += new EventHandler(this.timerScroll_Tick);
                }
                else if(this.timerScroll.Enabled) {
                    return;
                }
                this.timerScroll.Tag = y < ((height * 0.5) + 11.0);
                this.iScrollLine = 1;
                if((y < 0x10) || ((base.Height - 0x10) < y)) {
                    this.timerScroll.Interval = 100;
                    if((y < 9) || ((base.Height - 9) < y)) {
                        this.iScrollLine = 2;
                    }
                }
                else {
                    this.timerScroll.Interval = 250;
                }
                base.fSuppressMouseMove_Scroll = true;
                this.timerScroll.Enabled = false;
                this.timerScroll.Enabled = true;
            }
            else if(this.timerScroll != null) {
                this.timerScroll.Enabled = false;
            }
        }

        private void CloseAllDropDown() {
            foreach(ToolStripItem item in this.DisplayedItems) {
                QMenuItem item2 = item as QMenuItem;
                if((item2 != null) && item2.Selected) {
                    if(item2.HasDropDownItems && item2.DropDown.Visible) {
                        item2.HideDropDown();
                    }
                    this.miUnselect.Invoke(item2, null);
                    break;
                }
            }
        }

        private void CopyCutFiles(bool fCut) {
            List<string> lstPaths = new List<string>();
            DropDownMenuDropTarget root = GetRoot(this);
            if(root != null) {
                GetCheckedItem(root, lstPaths, fCut, true);
                if(lstPaths.Count == 0) {
                    foreach(ToolStripItem item in this.DisplayedItems) {
                        if(item.Selected) {
                            QMenuItem item2 = item as QMenuItem;
                            if((item2 != null) && !string.IsNullOrEmpty(item2.Path)) {
                                item2.IsCut = fCut;
                                lstPaths.Add(item2.Path);
                            }
                            break;
                        }
                    }
                }
                if(ShellMethods.SetClipboardFileDropPaths(lstPaths, fCut, this.hwndDialogParent)) {
                    fContainsFileDropList = true;
                }
            }
        }

        private void CopyFileNames(bool fPath) {
            List<string> lstPaths = new List<string>();
            DropDownMenuDropTarget root = GetRoot(this);
            if(root != null) {
                GetCheckedItem(root, lstPaths, false, true);
            }
            if(lstPaths.Count == 0) {
                foreach(ToolStripItem item in this.DisplayedItems) {
                    if(!item.Selected) {
                        continue;
                    }
                    QMenuItem item2 = item as QMenuItem;
                    if((item2 != null) && !string.IsNullOrEmpty(item2.Path)) {
                        string path = item2.Path;
                        if(!fPath) {
                            try {
                                path = Path.GetFileName(path);
                            }
                            catch {
                            }
                        }
                        if(!string.IsNullOrEmpty(path)) {
                            QTTabBarClass.SetStringClipboard(path);
                            fContainsFileDropList = false;
                            this.itemKeyInsertionMarkPrev = null;
                            base.Invalidate();
                        }
                    }
                    break;
                }
            }
            else {
                string str = string.Empty;
                foreach(string str3 in lstPaths) {
                    if(fPath) {
                        str = str + str3 + Environment.NewLine;
                    }
                    else {
                        try {
                            str = str + Path.GetFileName(str3) + Environment.NewLine;
                            continue;
                        }
                        catch {
                            continue;
                        }
                    }
                }
                if(str.Length > 0) {
                    QTTabBarClass.SetStringClipboard(str);
                    fContainsFileDropList = false;
                    this.itemKeyInsertionMarkPrev = null;
                    base.Invalidate();
                }
            }
        }

        private void DeleteFiles(bool fShiftKey) {
            List<string> lstPaths = new List<string>();
            DropDownMenuDropTarget root = GetRoot(this);
            if(root != null) {
                GetCheckedItem(root, lstPaths, false, false);
                if(lstPaths.Count == 0) {
                    foreach(ToolStripItem item in this.DisplayedItems) {
                        if(item.Selected) {
                            QMenuItem item2 = item as QMenuItem;
                            if((item2 != null) && !string.IsNullOrEmpty(item2.Path)) {
                                lstPaths.Add(item2.Path);
                            }
                            break;
                        }
                    }
                }
                ShellMethods.DeleteFile(lstPaths, fShiftKey, this.hwndDialogParent);
                if(!QTUtility.IsVista) {
                    root.Close(ToolStripDropDownCloseReason.ItemClicked);
                }
            }
        }

        protected override void Dispose(bool disposing) {
            if(this.dropTargetWrapper != null) {
                this.dropTargetWrapper.Dispose();
                this.dropTargetWrapper = null;
            }
            if(this.bmpInsertL != null) {
                this.bmpInsertL.Dispose();
                this.bmpInsertL = null;
            }
            if(this.bmpInsertR != null) {
                this.bmpInsertR.Dispose();
                this.bmpInsertR = null;
            }
            if(this.timerScroll != null) {
                this.timerScroll.Dispose();
                this.timerScroll = null;
            }
            base.Dispose(disposing);
        }

        private void DropDownMenuDropTarget_HandleCreated(object sender, EventArgs e) {
            this.dropTargetWrapper = new DropTargetWrapper(this);
            this.dropTargetWrapper.DragFileEnter += new DropTargetWrapper.DragFileEnterEventHandler(this.dropTargetWrapper_DragFileEnter);
            this.dropTargetWrapper.DragFileOver += new DragEventHandler(this.dropTargetWrapper_DragFileOver);
            this.dropTargetWrapper.DragFileLeave += new EventHandler(this.dropTargetWrapper_DragFileLeave);
            this.dropTargetWrapper.DragFileDrop += new DropTargetWrapper.DragFileDropEventHandler(this.dropTargetWrapper_DragFileDrop);
            this.dropTargetWrapper.DragDropEnd += new EventHandler(this.dropTargetWrapper_DragDropEnd);
            try {
                this.miUnselect = typeof(ToolStripItem).GetMethod("Unselect", BindingFlags.NonPublic | BindingFlags.Instance);
            }
            catch {
            }
        }

        private void dropTargetWrapper_DragDropEnd(object sender, EventArgs e) {
            base.CancelClosingAncestors(false, false);
            base.ShowItemToolTips = true;
            base.Close(ToolStripDropDownCloseReason.AppFocusChange);
        }

        private int dropTargetWrapper_DragFileDrop(out IntPtr hwnd, out byte[] idlReal) {
            base.fRespondModKeys = this.fRespondModKeysTemp;
            base.fEnableShiftKey = this.fEnableShiftKeyTemp;
            hwnd = IntPtr.Zero;
            idlReal = null;
            try {
                if((this.itemHover != null) && !string.IsNullOrEmpty(this.strTargetPath)) {
                    byte[] iDLData = ShellMethods.GetIDLData(this.strTargetPath);
                    if((iDLData != null) && (iDLData.Length > 0)) {
                        idlReal = iDLData;
                        base.CancelClosingAncestors(true, false);
                        base.ShowItemToolTips = false;
                        return 0;
                    }
                }
            }
            finally {
                this.strDraggingDrive = null;
                this.strDraggingStartPath = null;
                this.strTargetPath = null;
                this.itemHover = null;
            }
            return -1;
        }

        private DragDropEffects dropTargetWrapper_DragFileEnter(IntPtr hDrop, BandObjectLib.POINT pnt, int grfKeyState) {
            this.fRespondModKeysTemp = base.fRespondModKeys;
            this.fEnableShiftKeyTemp = base.fEnableShiftKey;
            base.fRespondModKeys = false;
            base.fEnableShiftKey = false;
            if(this.MenuDragEnter != null) {
                this.MenuDragEnter(this, EventArgs.Empty);
            }
            this.fDrivesContained = false;
            switch(QTTabBarClass.HandleDragEnter(hDrop, out this.strDraggingDrive, out this.strDraggingStartPath)) {
                case -1:
                    return DragDropEffects.None;

                case 0:
                    return DropTargetWrapper.MakeEffect(grfKeyState, 0);

                case 1:
                    return DropTargetWrapper.MakeEffect(grfKeyState, 1);

                case 2:
                    this.fDrivesContained = true;
                    return DragDropEffects.None;
            }
            return DragDropEffects.None;
        }

        private void dropTargetWrapper_DragFileLeave(object sender, EventArgs e) {
            base.fRespondModKeys = this.fRespondModKeysTemp;
            base.fEnableShiftKey = this.fEnableShiftKeyTemp;
            this.strDraggingDrive = null;
            this.strDraggingStartPath = null;
            this.strTargetPath = null;
            this.itemHover = null;
            this.fSuppressMouseUp = true;
            this.iItemDragOverRegion = -1;
            base.Invalidate();
        }

        private void dropTargetWrapper_DragFileOver(object sender, DragEventArgs e) {
            int iSourceState = -1;
            Point point = base.PointToClient(new Point(e.X, e.Y));
            ToolStripItem itemAt = base.GetItemAt(point);
            bool flag = false;
            if(itemAt != null) {
                Rectangle bounds = itemAt.Bounds;
                bool flag2 = (bounds.Bottom - point.Y) >= (point.Y - bounds.Top);
                flag = this.fTop != flag2;
                this.fTop = flag2;
            }
            bool flag3 = ((this.fTop && (this.iItemDragOverRegion != 0)) || (!this.fTop && (this.iItemDragOverRegion != 1))) && (itemAt == this.Items[this.Items.Count - 1]);
            if((itemAt != this.itemHover) || flag3) {
                if(itemAt != null) {
                    this.iItemDragOverRegion = this.fTop ? 0 : 1;
                    QMenuItem item2 = itemAt as QMenuItem;
                    if(item2 != null) {
                        bool flag4 = item2 is SubDirTipForm.ToolStripMenuItemEx;
                        if((flag3 && !this.fTop) && Directory.Exists(base.Path)) {
                            this.fDrawDropTarget = false;
                            this.strTargetPath = base.Path;
                            iSourceState = this.MakeDragOverRetval();
                            if((!flag4 && item2.HasDropDownItems) && !item2.DropDown.Visible) {
                                this.OnMouseLeave(EventArgs.Empty);
                                this.OnMouseMove(new MouseEventArgs(Control.MouseButtons, 0, point.X, point.Y, 0));
                            }
                        }
                        else if(flag4) {
                            bool flag5;
                            if(PathIsExecutable(item2.Path, out flag5)) {
                                this.fDrawDropTarget = true;
                                if(flag5) {
                                    iSourceState = -1;
                                    this.CloseAllDropDown();
                                }
                                else {
                                    this.strTargetPath = item2.Path;
                                    item2.Select();
                                    iSourceState = 2;
                                }
                            }
                            else {
                                this.fDrawDropTarget = false;
                                if(Directory.Exists(base.Path)) {
                                    this.strTargetPath = base.Path;
                                    iSourceState = this.MakeDragOverRetval();
                                }
                                this.CloseAllDropDown();
                            }
                        }
                        else if(Directory.Exists(item2.TargetPath)) {
                            this.fDrawDropTarget = true;
                            this.strTargetPath = item2.TargetPath;
                            iSourceState = this.MakeDragOverRetval();
                            this.OnMouseLeave(EventArgs.Empty);
                            this.OnMouseMove(new MouseEventArgs(Control.MouseButtons, 0, point.X, point.Y, 0));
                        }
                    }
                }
                flag = true;
            }
            else {
                iSourceState = this.iDDRetval;
            }
            if(itemAt != null) {
                this.BeginScrollTimer(itemAt, point);
            }
            this.itemHover = itemAt;
            this.iDDRetval = iSourceState;
            if(flag) {
                base.Invalidate();
            }
            if(iSourceState == -1) {
                this.strTargetPath = null;
                e.Effect = DragDropEffects.None;
            }
            else if(this.fDrivesContained) {
                e.Effect = DragDropEffects.Link;
            }
            else if(iSourceState == 2) {
                e.Effect = DragDropEffects.Copy;
            }
            else {
                e.Effect = DropTargetWrapper.MakeEffect(e.KeyState, iSourceState);
            }
        }

        private static void GetCheckedItem(DropDownMenuDropTarget ddmdtRoot, List<string> lstPaths, bool fCut, bool fSetCut) {
            foreach(ToolStripItem item in ddmdtRoot.Items) {
                QMenuItem item2 = item as QMenuItem;
                if(item2 != null) {
                    if(item2.Checked) {
                        if(!string.IsNullOrEmpty(item2.Path)) {
                            lstPaths.Add(item2.Path);
                            if(fSetCut) {
                                item2.IsCut = fCut;
                            }
                        }
                        else if(fSetCut) {
                            item2.IsCut = false;
                        }
                        continue;
                    }
                    if(fSetCut) {
                        item2.IsCut = false;
                    }
                    if(item2.HasDropDownItems) {
                        GetCheckedItem((DropDownMenuDropTarget)item2.DropDown, lstPaths, fCut, fSetCut);
                    }
                }
            }
        }

        private static DropDownMenuDropTarget GetRoot(DropDownMenuDropTarget ddmdt) {
            if(ddmdt.fIsRootMenu) {
                return ddmdt;
            }
            ToolStripItem ownerItem = ddmdt.OwnerItem;
            if(ownerItem != null) {
                ToolStrip owner = ownerItem.Owner;
                if(owner != null) {
                    DropDownMenuDropTarget target = owner as DropDownMenuDropTarget;
                    if(target != null) {
                        return GetRoot(target);
                    }
                }
            }
            return null;
        }

        private bool IsKeyTargetItem(ToolStripItem item) {
            bool flag;
            SubDirTipForm.ToolStripMenuItemEx ex = item as SubDirTipForm.ToolStripMenuItemEx;
            return ((ex == null) || (PathIsExecutable(ex.Path, out flag) && !flag));
        }

        private int MakeDragOverRetval() {
            if(string.Equals(this.strTargetPath, this.strDraggingStartPath, StringComparison.OrdinalIgnoreCase)) {
                return 3;
            }
            if((this.strDraggingDrive != null) && string.Equals(this.strDraggingDrive, this.strTargetPath.Substring(0, 3), StringComparison.OrdinalIgnoreCase)) {
                return 0;
            }
            return 1;
        }

        protected override void OnClosed(ToolStripDropDownClosedEventArgs e) {
            this.fSuppressMouseUp = false;
            this.fDrawDropTarget = false;
            this.iItemDragOverRegion = -1;
            this.fKeyTargetIsThis = false;
            this.itemKeyInsertionMarkPrev = null;
            base.OnClosed(e);
        }

        protected override void OnItemClicked(ToolStripItemClickedEventArgs e) {
            if(this.fSuppressMouseUp) {
                this.fSuppressMouseUp = false;
            }
            else {
                base.OnItemClicked(e);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            this.fSuppressMouseUp = false;
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            bool fKeyTargetIsThis = this.fKeyTargetIsThis;
            this.fKeyTargetIsThis = false;
            this.itemKeyInsertionMarkPrev = null;
            if(fKeyTargetIsThis) {
                base.Invalidate();
            }
            base.OnMouseMove(e);
        }

        protected override void OnOpened(EventArgs e) {
            try {
                this.fThisPathExists = Directory.Exists(base.Path);
                fContainsFileDropList = ShellMethods.ClipboardContainsFileDropList(this.hwndDialogParent);
            }
            catch {
            }
            if(((((base.OwnerItem == null) || (base.OwnerItem.Owner == null)) || !base.OwnerItem.Owner.RectangleToScreen(base.OwnerItem.Bounds).Contains(Control.MousePosition)) && (!this.fIsRootMenu || this.fShownByKey)) && (((this.DisplayedItems.Count > 0) && !this.IsKeyTargetItem(this.DisplayedItems[0])) && fContainsFileDropList)) {
                this.fKeyTargetIsThis = true;
                this.itemKeyInsertionMarkPrev = this.DisplayedItems[0];
            }
            base.OnOpened(e);
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            if(this.itemHover != null) {
                Rectangle bounds = this.itemHover.Bounds;
                if(this.fDrawDropTarget) {
                    return;
                }
                using(Pen pen = new Pen(Color.Black, 2f)) {
                    using(Pen pen2 = new Pen(Color.Black, 1f)) {
                        if(this.fTop) {
                            e.Graphics.DrawLine(pen, 3, bounds.Top, bounds.Right - 2, bounds.Top);
                            e.Graphics.DrawLine(pen2, 3, bounds.Top - 3, 3, bounds.Top + 2);
                            e.Graphics.DrawLine(pen2, 4, bounds.Top - 2, 4, bounds.Top + 1);
                            e.Graphics.DrawLine(pen2, (int)(bounds.Right - 2), (int)(bounds.Top - 3), (int)(bounds.Right - 2), (int)(bounds.Top + 2));
                            e.Graphics.DrawLine(pen2, (int)(bounds.Right - 3), (int)(bounds.Top - 2), (int)(bounds.Right - 3), (int)(bounds.Top + 1));
                        }
                        else {
                            e.Graphics.DrawLine(pen, 3, bounds.Bottom, bounds.Right - 2, bounds.Bottom);
                            e.Graphics.DrawLine(pen2, 3, bounds.Bottom - 3, 3, bounds.Bottom + 2);
                            e.Graphics.DrawLine(pen2, 4, bounds.Bottom - 2, 4, bounds.Bottom + 1);
                            e.Graphics.DrawLine(pen2, (int)(bounds.Right - 2), (int)(bounds.Bottom - 3), (int)(bounds.Right - 2), (int)(bounds.Bottom + 2));
                            e.Graphics.DrawLine(pen2, (int)(bounds.Right - 3), (int)(bounds.Bottom - 2), (int)(bounds.Right - 3), (int)(bounds.Bottom + 1));
                        }
                    }
                    return;
                }
            }
            if(this.itemKeyInsertionMarkPrev != null) {
                Bitmap bmpInsertR;
                Rectangle rectangle2 = this.itemKeyInsertionMarkPrev.Bounds;
                if(QTUtility.IsRTL) {
                    if(this.bmpInsertR == null) {
                        this.bmpInsertR = Resources_Image.imgInsertR;
                    }
                    bmpInsertR = this.bmpInsertR;
                }
                else {
                    if(this.bmpInsertL == null) {
                        this.bmpInsertL = Resources_Image.imgInsertL;
                    }
                    bmpInsertR = this.bmpInsertL;
                }
                e.Graphics.DrawImage(bmpInsertR, new Rectangle(2, rectangle2.Bottom - 6, 12, 12), new Rectangle(0, 0, 12, 12), GraphicsUnit.Pixel);
            }
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e) {
            if(((e.KeyCode == Keys.Up) || (e.KeyCode == Keys.Down)) && (this.fThisPathExists && fContainsFileDropList)) {
                for(int i = 0; i < this.DisplayedItems.Count; i++) {
                    ToolStripItem item = this.DisplayedItems[i];
                    if(item.Selected) {
                        int index = this.Items.IndexOf(item);
                        if(index != -1) {
                            int num3;
                            if(e.KeyCode == Keys.Up) {
                                if(index == 0) {
                                    num3 = this.Items.Count - 1;
                                }
                                else {
                                    num3 = index - 1;
                                }
                            }
                            else if(index == (this.Items.Count - 1)) {
                                num3 = 0;
                            }
                            else {
                                num3 = index + 1;
                            }
                            item = this.Items[num3];
                            if(((num3 == 0) && (e.KeyCode == Keys.Down)) && (!this.fEnterVirtualBottom && this.IsKeyTargetItem(this.Items[this.Items.Count - 1]))) {
                                this.itemKeyInsertionMarkPrev = this.Items[this.Items.Count - 1];
                                this.fEnterVirtualBottom = true;
                                this.fKeyTargetIsThis = true;
                                base.Invalidate();
                                return;
                            }
                            this.fEnterVirtualBottom = false;
                            if(!this.IsKeyTargetItem(item)) {
                                this.itemKeyInsertionMarkPrev = item;
                                this.fKeyTargetIsThis = true;
                                base.Invalidate();
                                return;
                            }
                            this.itemKeyInsertionMarkPrev = null;
                            if(this.fKeyTargetIsThis) {
                                base.Invalidate();
                            }
                            this.fKeyTargetIsThis = false;
                        }
                        break;
                    }
                }
            }
            base.OnPreviewKeyDown(e);
        }

        private void PasteFiles() {
            string path = null;
            if(this.fKeyTargetIsThis) {
                path = base.Path;
            }
            else {
                foreach(ToolStripItem item in this.DisplayedItems) {
                    if(!item.Selected) {
                        continue;
                    }
                    QMenuItem item2 = item as QMenuItem;
                    if(item2 != null) {
                        if(item2 is SubDirTipForm.ToolStripMenuItemEx) {
                            bool flag2;
                            if(PathIsExecutable(item2.Path, out flag2) && !flag2) {
                                StringCollection fileDropList = Clipboard.GetFileDropList();
                                if((fileDropList != null) && (fileDropList.Count > 0)) {
                                    string str2 = string.Empty;
                                    foreach(string str3 in fileDropList) {
                                        str2 = str2 + "\"" + str3 + "\" ";
                                    }
                                    str2 = str2.Trim();
                                    if(str2.Length > 0) {
                                        MenuItemArguments mia = new MenuItemArguments(item2.Path, MenuTarget.File, MenuGenre.Application);
                                        mia.Argument = str2;
                                        AppLauncher.Execute(mia, this.fIsRootMenu ? base.Handle : IntPtr.Zero);
                                    }
                                }
                                return;
                            }
                        }
                        else if(Directory.Exists(item2.TargetPath)) {
                            path = item2.TargetPath;
                        }
                    }
                    break;
                }
            }
            if(!string.IsNullOrEmpty(path)) {
                ShellMethods.PasteFile(path, this.hwndDialogParent);
                if(!QTUtility.IsVista) {
                    DropDownMenuDropTarget root = GetRoot(this);
                    if(root != null) {
                        root.Close(ToolStripDropDownCloseReason.ItemClicked);
                    }
                }
            }
        }

        private static bool PathIsExecutable(string path, out bool fLinkTargetIsNotDropTarget) {
            fLinkTargetIsNotDropTarget = false;
            if(string.IsNullOrEmpty(path)) {
                return false;
            }
            string extension = Path.GetExtension(path);
            if(string.IsNullOrEmpty(extension)) {
                return false;
            }
            if(strExtExecutable == null) {
                strExtExecutable = Environment.GetEnvironmentVariable("PATHEXT");
                if(strExtExecutable == null) {
                    strExtExecutable = ".COM;.EXE;.BAT;.CMD;.VBS;.VBE;.JS;.JSE;.WSF;.WSH;.MSC";
                }
            }
            if(!string.Equals(".lnk", extension, StringComparison.OrdinalIgnoreCase)) {
                return (strExtExecutable.IndexOf(extension, StringComparison.OrdinalIgnoreCase) != -1);
            }
            string linkTargetPath = ShellMethods.GetLinkTargetPath(path);
            if(File.Exists(linkTargetPath)) {
                string str3 = Path.GetExtension(linkTargetPath);
                if(strExtExecutable.IndexOf(str3, StringComparison.OrdinalIgnoreCase) != -1) {
                    return true;
                }
            }
            fLinkTargetIsNotDropTarget = true;
            return true;
        }

        protected override bool ProcessCmdKey(ref Message m, Keys keyData) {
            bool flag = (((int)((long)m.LParam)) & 0x40000000) != 0;
            Keys keys = keyData & Keys.KeyCode;
            Keys keys2 = keyData & ~Keys.KeyCode;
            if(keys2 == Keys.Control) {
                if(flag) {
                    return true;
                }
                switch(keys) {
                    case Keys.V:
                        this.PasteFiles();
                        return true;

                    case Keys.C:
                        this.CopyCutFiles(false);
                        return true;

                    case Keys.X:
                        this.CopyCutFiles(true);
                        return true;
                }
            }
            switch(keys) {
                case Keys.Down:
                    if(this.fEnterVirtualBottom) {
                        return true;
                    }
                    break;

                case Keys.Up:
                    this.fEnterVirtualBottom = false;
                    break;

                case Keys.Delete:
                    if(!flag && ((keyData == Keys.Delete) || (keyData == (Keys.Shift | Keys.Delete)))) {
                        this.DeleteFiles(keyData != Keys.Delete);
                    }
                    return true;
            }
            int num = ((int)keyData) | 0x100000;
            if((num != QTUtility.ShortcutKeys[0x1b]) && (num != QTUtility.ShortcutKeys[0x1c])) {
                return base.ProcessCmdKey(ref m, keyData);
            }
            if(!flag) {
                this.CopyFileNames(num == QTUtility.ShortcutKeys[0x1b]);
            }
            return true;
        }

        public void SetShowingByKey(bool value) {
            this.fShownByKey = value;
        }

        public void SetSuppressMouseUp() {
            this.fSuppressMouseUp = true;
        }

        private void timerScroll_Tick(object sender, EventArgs e) {
            this.timerScroll.Enabled = false;
            base.fSuppressMouseMove_Scroll = false;
            if(!base.IsDisposed && base.Visible) {
                base.ScrollMenu((bool)this.timerScroll.Tag, this.iScrollLine);
            }
        }
    }
}
