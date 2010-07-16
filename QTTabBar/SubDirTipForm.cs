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
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    internal sealed class SubDirTipForm : Form {
        private IContainer components;
        private DropDownMenuDropTarget contextMenuSubDir;
        private string currentDir;
        private byte[] currentIDL;
        private ToolStripMenuItem draggingItem;
        private string draggingPath;
        private ExtComparer extComparer = new ExtComparer();
        private bool fClickClose;
        private bool fDesktop;
        private bool fDragStarted;
        private bool fDropHilitedOpened;
        private bool fShownByKey;
        private bool fSuppressThumbnail;
        private IntPtr hwndDialogParent;
        private IntPtr hwndFocusOnMenu;
        private IntPtr hwndMessageReflect;
        private bool isShowing;
        private int iThumbnailIndex = -1;
        private int iToolTipIndex = -1;
        private LabelEx lblSubDirBtn;
        private ListViewWrapper listViewWrapper;
        private List<Rectangle> lstRcts = new List<Rectangle>();
        private List<string> lstTempDirectoryPaths = new List<string>();
        private bool menuIsShowing;
        private const string NAME_DUMMY = "dummy";
        private const string NAME_RECYCLEBIN = "$RECYCLE.BIN";
        private const string NAME_RECYLER = "RECYCLER";
        private const string NAME_VOLUMEINFO = "System Volume Information";
        private Point pntDragStart;
        private static Size SystemDragSize = SystemInformation.DragSize;
        private ThumbnailTooltipForm thumbnailTip;
        private Timer timerToolTipByKey;
        private ToolStripMeuItemComparer tsmiComparer;

        public event EventHandler MenuClosed;

        public event ToolStripItemClickedEventHandler MenuItemClicked;

        public event ItemRightClickedEventHandler MenuItemRightClicked;

        public event EventHandler MultipleMenuItemsClicked;

        public event ItemRightClickedEventHandler MultipleMenuItemsRightClicked;

        public SubDirTipForm(IntPtr hwndMessageReflect, IntPtr hwndFocusOnMenu, bool fEnableShiftKeyOnDDMR, ListViewWrapper lvw) {
            this.listViewWrapper = lvw;
            this.hwndMessageReflect = hwndMessageReflect;
            this.hwndFocusOnMenu = hwndFocusOnMenu;
            this.hwndDialogParent = hwndFocusOnMenu;
            this.fDesktop = !fEnableShiftKeyOnDDMR;
            this.InitializeComponent();
            this.contextMenuSubDir.ImageList = QTUtility.ImageListGlobal;
            this.contextMenuSubDir.MessageParent = hwndMessageReflect;
            IntPtr handle = this.lblSubDirBtn.Handle;
            PInvoke.SetWindowLongPtr(handle, -20, PInvoke.Ptr_OP_OR(PInvoke.GetWindowLongPtr(handle, -20), 0x8000000));
        }

        private void CheckedItemsClick() {
            List<string> lstCheckedPaths = new List<string>();
            List<QMenuItem> lstCheckedItems = new List<QMenuItem>();
            if(this.GetCheckedItems(this.contextMenuSubDir, lstCheckedPaths, lstCheckedItems, false)) {
                this.lstTempDirectoryPaths.Clear();
                foreach(QMenuItem item in lstCheckedItems) {
                    if((item is ToolStripMenuItemEx) || (item.IDLData != null)) {
                        this.MenuItemClicked(this, new ToolStripItemClickedEventArgs(item));
                        continue;
                    }
                    this.lstTempDirectoryPaths.Add(item.Path);
                }
                if((this.lstTempDirectoryPaths.Count > 0) && (this.MultipleMenuItemsClicked != null)) {
                    this.MultipleMenuItemsClicked(this, EventArgs.Empty);
                    this.lstTempDirectoryPaths.Clear();
                }
            }
        }

        private void CheckedItemsRightClick(ItemRightClickedEventArgs e) {
            List<string> lstCheckedPaths = new List<string>();
            List<QMenuItem> lstCheckedItems = new List<QMenuItem>();
            if(this.GetCheckedItems(this.contextMenuSubDir, lstCheckedPaths, lstCheckedItems, false)) {
                if(lstCheckedPaths.Count <= 1) {
                    if(lstCheckedPaths.Count == 1) {
                        this.MenuItemRightClicked(this, e);
                    }
                }
                else {
                    string path = string.Empty;
                    foreach(string str2 in lstCheckedPaths) {
                        if((!string.IsNullOrEmpty(str2) && (str2.Length > 3)) && !str2.StartsWith("::")) {
                            path = str2;
                            break;
                        }
                    }
                    if(path.Length > 0) {
                        try {
                            bool flag = true;
                            string directoryName = Path.GetDirectoryName(path);
                            if(!string.IsNullOrEmpty(directoryName)) {
                                foreach(string str4 in lstCheckedPaths) {
                                    if(!string.Equals(directoryName, Path.GetDirectoryName(str4), StringComparison.OrdinalIgnoreCase)) {
                                        flag = false;
                                        break;
                                    }
                                }
                                if(flag) {
                                    this.lstTempDirectoryPaths = new List<string>(lstCheckedPaths);
                                    this.MultipleMenuItemsRightClicked(this, e);
                                    this.lstTempDirectoryPaths.Clear();
                                    return;
                                }
                            }
                        }
                        catch {
                        }
                    }
                    SystemSounds.Beep.Play();
                }
            }
        }

        public void ClearThumbnailCache() {
            if(this.thumbnailTip != null) {
                this.thumbnailTip.ClearCache();
            }
        }

        private void contextMenuSubDir_Closed(object sender, ToolStripDropDownClosedEventArgs e) {
            this.menuIsShowing = false;
            this.fSuppressThumbnail = false;
            this.contextMenuSubDir.SuppressMouseMove = false;
            this.fDropHilitedOpened = false;
            this.contextMenuSubDir.Path = null;
            this.draggingPath = null;
            this.draggingItem = null;
            this.lstRcts.Clear();
            this.HideThumbnailTooltip(true);
            this.iThumbnailIndex = -1;
            this.iToolTipIndex = -1;
            this.lblSubDirBtn.SetPressed(false);
            if(!this.fClickClose) {
                if(this.fShownByKey) {
                    this.fShownByKey = false;
                    this.HideSubDirTip();
                }
                else if(!listViewWrapper.MouseIsOverListView()) {
                    this.HideSubDirTip();
                }
            }
            this.fClickClose = false;
            this.contextMenuSubDir.SuspendLayout();
            if(!this.fDragStarted) {
                while(this.contextMenuSubDir.Items.Count > 0) {
                    this.contextMenuSubDir.Items[0].Dispose();
                }
            }
            this.fDragStarted = false;
            this.contextMenuSubDir.ItemsClearVirtual();
            this.contextMenuSubDir.ResumeLayout();
            DropDownMenuBase.ExitMenuMode();
            if(this.MenuClosed != null) {
                this.MenuClosed(this, EventArgs.Empty);
            }
        }

        private QMenuItem CreateDirectoryItem(DirectoryInfo diSub, string title, bool fIcon, bool fLink) {
            bool flag;
            FileSystemInfo targetIfFolderLink = QTTabBarClass.GetTargetIfFolderLink(diSub, out flag);
            if(!flag) {
                return null;
            }
            QMenuItem item = new QMenuItem(title, MenuTarget.Folder, MenuGenre.SubDirTip);
            item.Exists = true;
            item.HasIcon = fIcon;
            string fullName = diSub.FullName;
            if(fIcon) {
                item.SetImageReservationKey(fullName, null);
            }
            else if(!fLink) {
                item.ImageKey = "folder";
            }
            item.Path = fullName;
            item.TargetPath = targetIfFolderLink.FullName;
            item.QueryVirtualMenu += this.directoryItem_QueryVirtualMenu;
            return item;
        }

        private List<QMenuItem> CreateMenu(DirectoryInfo di, string pathChild) {
            List<QMenuItem> list = new List<QMenuItem>();
            List<QMenuItem> collection = new List<QMenuItem>();
            bool flag = true;
            try {
                flag = new DriveInfo(di.FullName).DriveFormat == "NTFS";
            }
            catch {
            }
            try {
                bool flag2 = QTUtility.CheckConfig(Settings.SubDirTipsSystem);
                bool flag3 = QTUtility.CheckConfig(Settings.SubDirTipsHidden);
                FileAttributes attributes = FileAttributes.ReparsePoint | FileAttributes.System | FileAttributes.Hidden;
                int num = 0;
                foreach(DirectoryInfo info in di.GetDirectories()) {
                    try {
                        string fullName = info.FullName;
                        string name = info.Name;
                        if((((fullName.Length != 0x1c) || !string.Equals(name, "System Volume Information", StringComparison.OrdinalIgnoreCase)) && ((fullName.Length != 15) || !string.Equals(name, "$RECYCLE.BIN", StringComparison.OrdinalIgnoreCase))) && ((fullName.Length != 11) || !string.Equals(name, "RECYCLER", StringComparison.OrdinalIgnoreCase))) {
                            FileAttributes attributes2 = info.Attributes;
                            if(!QTUtility.IsVista || ((attributes2 & attributes) != attributes)) {
                                bool flag5 = (attributes2 & FileAttributes.System) != 0;
                                bool flag6 = (attributes2 & FileAttributes.ReadOnly) != 0;
                                bool flag7 = (attributes2 & FileAttributes.Hidden) != 0;
                                if((!flag5 || flag2) && (!flag7 || flag3)) {
                                    bool flag4;
                                    string title = QTUtility2.MakeNameEllipsis(name, out flag4);
                                    QMenuItem item = this.CreateDirectoryItem(info, title, flag5 || flag6, false);
                                    if(item != null) {
                                        if(flag4) {
                                            item.OriginalTitle = name;
                                        }
                                        if((pathChild != null) && (item.Path == pathChild)) {
                                            item.BackColor = QTUtility2.MakeModColor(SystemColors.Highlight);
                                            pathChild = null;
                                        }
                                        list.Add(item);
                                    }
                                    else {
                                        string path = fullName;
                                        ToolStripMenuItemEx ex = new ToolStripMenuItemEx(title);
                                        ex.Exists = true;
                                        ex.SetImageReservationKey(path, null);
                                        ex.ThumbnailIndex = 0xffff + num++;
                                        ex.ThumbnailPath = path;
                                        ex.Path = path;
                                        ex.Name = name;
                                        ex.Extension = Path.GetExtension(path).ToLower();
                                        if(flag4) {
                                            ex.OriginalTitle = name;
                                        }
                                        ex.MouseMove += this.tsmi_Files_MouseMove;
                                        ex.MouseDown += this.tsmi_MouseDown;
                                        ex.MouseUp += this.tsmi_MouseUp;
                                        collection.Add(ex);
                                    }
                                }
                            }
                        }
                    }
                    catch(Exception exception) {
                        QTUtility2.MakeErrorLog(exception, "creating subdir menu");
                    }
                }
                if(!flag) {
                    if(this.tsmiComparer == null) {
                        this.tsmiComparer = new ToolStripMeuItemComparer();
                    }
                    list.Sort(this.tsmiComparer);
                }
                if(!QTUtility.CheckConfig(Settings.SubDirTipsFiles)) {
                    return list;
                }
                int num2 = 0;
                string str5 = ".lnk";
                string str6 = ".url";
                foreach(FileInfo info2 in di.GetFiles()) {
                    try {
                        FileAttributes attributes3 = info2.Attributes;
                        bool flag8 = (attributes3 & FileAttributes.System) != 0;
                        bool flag9 = (attributes3 & FileAttributes.Hidden) != 0;
                        if((!flag8 || flag2) && (!flag9 || flag3)) {
                            string fileNameWithoutExtension;
                            bool flag10;
                            string lnkPath = info2.FullName;
                            string str8 = lnkPath;
                            string ext = info2.Extension.ToLower();
                            if(ext == str5) {
                                string linkTargetPath = ShellMethods.GetLinkTargetPath(lnkPath);
                                if(!string.IsNullOrEmpty(linkTargetPath)) {
                                    DirectoryInfo diSub = new DirectoryInfo(linkTargetPath);
                                    if(diSub.Exists) {
                                        string str12 = QTUtility2.MakeNameEllipsis(Path.GetFileNameWithoutExtension(info2.Name), out flag10);
                                        QMenuItem item2 = this.CreateDirectoryItem(diSub, str12, false, true);
                                        if(item2 != null) {
                                            item2.Path = lnkPath;
                                            item2.TargetPath = linkTargetPath;
                                            item2.Name = info2.Name;
                                            item2.Extension = ext;
                                            if(flag10) {
                                                item2.OriginalTitle = info2.Name;
                                            }
                                            item2.HasIcon = true;
                                            item2.SetImageReservationKey(lnkPath, ext);
                                            collection.Add(item2);
                                        }
                                        goto Label_04A3;
                                    }
                                    str8 = linkTargetPath;
                                }
                            }
                            if((ext == str5) || (ext == str6)) {
                                fileNameWithoutExtension = Path.GetFileNameWithoutExtension(info2.Name);
                            }
                            else {
                                fileNameWithoutExtension = info2.Name;
                            }
                            ToolStripMenuItemEx ex2 = new ToolStripMenuItemEx(QTUtility2.MakeNameEllipsis(fileNameWithoutExtension, out flag10));
                            ex2.ThumbnailIndex = num2++;
                            ex2.ThumbnailPath = str8;
                            ex2.Path = lnkPath;
                            ex2.Name = info2.Name;
                            ex2.Extension = ext;
                            if(flag10) {
                                ex2.OriginalTitle = fileNameWithoutExtension;
                            }
                            ex2.Exists = true;
                            ex2.SetImageReservationKey(lnkPath, ext);
                            ex2.MouseMove += this.tsmi_Files_MouseMove;
                            ex2.MouseDown += this.tsmi_MouseDown;
                            ex2.MouseUp += this.tsmi_MouseUp;
                            collection.Add(ex2);
                        }
                    }
                    catch(Exception exception2) {
                        QTUtility2.MakeErrorLog(exception2, "creating subfile menu");
                    }
                Label_04A3: ;
                }
                collection.Sort(this.extComparer);
                list.AddRange(collection);
            }
            catch {
            }
            return list;
        }

        private List<QMenuItem> CreateMenuFromIDL(IDLWrapper idlw, byte[] idlChild) {
            List<QMenuItem> list = new List<QMenuItem>();
            List<QMenuItem> collection = new List<QMenuItem>();
            if(idlw.Available) {
                IShellFolder shellFolder = null;
                IEnumIDList ppenumIDList = null;
                IntPtr zero = IntPtr.Zero;
                IntPtr ptr2 = IntPtr.Zero;
                if(idlChild != null) {
                    zero = ShellMethods.CreateIDL(idlChild);
                    ptr2 = PInvoke.ILFindLastID(zero);
                }
                bool flag = QTUtility.CheckConfig(Settings.SubDirTipsHidden);
                int grfFlags = 0x60;
                if(flag) {
                    grfFlags |= 0x80;
                }
                try {
                    IntPtr ptr3;
                    if(!ShellMethods.GetShellFolder(idlw.PIDL, out shellFolder) || (shellFolder.EnumObjects(IntPtr.Zero, grfFlags, out ppenumIDList) != 0)) {
                        return list;
                    }
                    int num2 = 0;
                    while(ppenumIDList.Next(1, out ptr3, null) == 0) {
                        IntPtr pIDL = PInvoke.ILCombine(idlw.PIDL, ptr3);
                        string str = ShellMethods.GetDisplayName(shellFolder, ptr3, false);
                        if(!string.IsNullOrEmpty(str)) {
                            uint rgfInOut = 0x60000000;
                            IntPtr[] apidl = new IntPtr[] { ptr3 };
                            if(shellFolder.GetAttributesOf(1, apidl, ref rgfInOut) == 0) {
                                bool flag4;
                                bool flag2 = (rgfInOut & 0x20000000) == 0x20000000;
                                bool flag3 = (rgfInOut & 0x40000000) == 0x40000000;
                                string name = ShellMethods.GetDisplayName(shellFolder, ptr3, true);
                                string title = QTUtility2.MakeNameEllipsis(name, out flag4);
                                if(flag3 && !flag2) {
                                    ToolStripMenuItemEx ex = new ToolStripMenuItemEx(title);
                                    ex.ThumbnailIndex = num2++;
                                    ex.ThumbnailPath = str;
                                    ex.Path = str;
                                    ex.Name = name;
                                    ex.Extension = Path.GetExtension(str).ToLower();
                                    if(flag4) {
                                        ex.OriginalTitle = name;
                                    }
                                    ex.Exists = true;
                                    ex.SetImageReservationKey(str, Path.GetExtension(str).ToLower());
                                    ex.MouseMove += this.tsmi_Files_MouseMove;
                                    ex.MouseDown += this.tsmi_MouseDown;
                                    ex.MouseUp += this.tsmi_MouseUp;
                                    collection.Add(ex);
                                }
                                else {
                                    QMenuItem item = new QMenuItem(title, flag2 ? MenuTarget.Folder : MenuTarget.File, MenuGenre.SubDirTip);
                                    if(str.Length == 3) {
                                        if(!QTUtility.ImageListGlobal.Images.ContainsKey(str)) {
                                            QTUtility.ImageListGlobal.Images.Add(str, QTUtility.GetIcon(pIDL));
                                        }
                                        item.ImageKey = str;
                                    }
                                    else {
                                        item.SetImageReservationKey(str, flag2 ? null : Path.GetExtension(str).ToLower());
                                    }
                                    item.Exists = flag3;
                                    item.Path = str;
                                    item.TargetPath = str;
                                    item.ForceToolTip = true;
                                    if((idlChild != null) && (shellFolder.CompareIDs((IntPtr)0x10000000, ptr3, ptr2) == 0)) {
                                        item.BackColor = QTUtility2.MakeModColor(SystemColors.Highlight);
                                    }
                                    if(!flag2) {
                                        item.IDLData = ShellMethods.GetIDLData(pIDL);
                                    }
                                    item.QueryVirtualMenu += this.directory_FromIDL_QueryVirtualMenu;
                                    list.Add(item);
                                }
                            }
                            if(ptr3 != IntPtr.Zero) {
                                PInvoke.CoTaskMemFree(ptr3);
                            }
                            if(pIDL != IntPtr.Zero) {
                                PInvoke.CoTaskMemFree(pIDL);
                            }
                        }
                    }
                    collection.Sort(this.extComparer);
                    list.AddRange(collection);
                }
                catch {
                }
                finally {
                    if(shellFolder != null) {
                        Marshal.ReleaseComObject(shellFolder);
                    }
                    if(ppenumIDList != null) {
                        Marshal.ReleaseComObject(ppenumIDList);
                    }
                    if(zero != IntPtr.Zero) {
                        PInvoke.CoTaskMemFree(zero);
                    }
                }
            }
            return list;
        }

        private List<QMenuItem> CreateParentMenu(IDLWrapper idlw, List<QMenuItem> lst) {
            if(lst == null) {
                lst = new List<QMenuItem>();
            }
            if(idlw.Available) {
                IntPtr ptr;
                if(!TryGetParentIDL(idlw.PIDL, out ptr)) {
                    return lst;
                }
                using(IDLWrapper wrapper = new IDLWrapper(ptr)) {
                    if(!wrapper.HasPath) {
                        return lst;
                    }
                    bool flag = PInvoke.ILGetSize(wrapper.PIDL) == 2;
                    QMenuItem item = new QMenuItem(ShellMethods.GetDisplayName(wrapper.PIDL, true), MenuTarget.Folder, MenuGenre.SubDirTip);
                    if(!QTUtility.ImageListGlobal.Images.ContainsKey(wrapper.Path)) {
                        QTUtility.ImageListGlobal.Images.Add(wrapper.Path, QTUtility.GetIcon(wrapper.PIDL));
                    }
                    item.ImageKey = item.Path = item.TargetPath = wrapper.Path;
                    item.IDLDataChild = idlw.IDL;
                    item.PathChild = idlw.Path;
                    item.MouseMove += this.tsmi_Folder_MouseMove;
                    DropDownMenuDropTarget target = new DropDownMenuDropTarget(null, true, !this.fDesktop, false, this.hwndDialogParent);
                    target.SuspendLayout();
                    target.CheckOnEdgeClick = true;
                    target.MessageParent = this.hwndMessageReflect;
                    target.Items.Add(new ToolStripMenuItem("dummy"));
                    target.ImageList = QTUtility.ImageListGlobal;
                    target.SpaceKeyExecute = true;
                    target.MouseLeave += this.ddmr_MouseLeave;
                    target.ItemRightClicked += this.ddmr_ItemRightClicked;
                    target.Opened += this.ddmr_Opened;
                    target.MenuDragEnter += this.ddmr_MenuDragEnter;
                    item.DropDown = target;
                    item.DropDownOpening += this.tsmi_DropDownOpening;
                    item.DropDownItemClicked += this.ddmr_ItemClicked;
                    if(wrapper.IsFileSystem) {
                        item.MouseDown += this.tsmi_MouseDown;
                        item.MouseUp += this.tsmi_MouseUp;
                        target.MouseDragMove += this.ddmr_MouseDragMove;
                        target.MouseUpBeforeDrop += this.ddmr_MouseUpBeforeDrop;
                        target.KeyUp += this.ddmr_KeyUp;
                        target.PreviewKeyDown += this.ddmr_PreviewKeyDown;
                        target.MouseScroll += this.ddmr_MouseScroll;
                        target.Path = wrapper.Path;
                    }
                    target.ResumeLayout();
                    lst.Add(item);
                    if(!flag) {
                        this.CreateParentMenu(wrapper, lst);
                    }
                }
            }
            return lst;
        }

        private void ddmr_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            if(this.MenuItemClicked != null) {
                if((e.ClickedItem is ToolStripMenuItem) && ((ToolStripMenuItem)e.ClickedItem).Checked) {
                    this.CheckedItemsClick();
                    this.contextMenuSubDir.Close(ToolStripDropDownCloseReason.ItemClicked);
                }
                else {
                    this.contextMenuSubDir.Close(ToolStripDropDownCloseReason.ItemClicked);
                    this.MenuItemClicked(this, e);
                }
            }
        }

        private void ddmr_ItemRightClicked(object sender, ItemRightClickedEventArgs e) {
            if(this.MenuItemRightClicked != null) {
                this.fSuppressThumbnail = true;
                if(((e.ClickedItem is ToolStripMenuItem) && ((ToolStripMenuItem)e.ClickedItem).Checked) && (this.MultipleMenuItemsRightClicked != null)) {
                    this.CheckedItemsRightClick(e);
                }
                else {
                    this.MenuItemRightClicked(this, e);
                }
                ((DropDownMenuReorderable)sender).SuppressMouseMove = false;
                if(e.HRESULT != 0) {
                    this.fSuppressThumbnail = false;
                }
            }
        }

        private void ddmr_KeyUp(object sender, KeyEventArgs e) {
            if(((Keys.Left > e.KeyCode) || (e.KeyCode > Keys.Down)) && (((e.KeyCode != Keys.End) && (e.KeyCode != Keys.Home)) && ((e.KeyCode != Keys.Prior) && (e.KeyCode != Keys.Next)))) {
                if(e.KeyCode == Keys.Escape) {
                    this.HideThumbnailTooltip(true);
                }
            }
            else {
                DropDownMenuReorderable reorderable = (DropDownMenuReorderable)sender;
                reorderable.UpdateToolTipByKey(null);
                foreach(ToolStripItem item in reorderable.Items) {
                    if(item.Selected) {
                        ToolStripMenuItemEx tsmi = item as ToolStripMenuItemEx;
                        if(tsmi == null) {
                            break;
                        }
                        reorderable.SuppressMouseMoveOnce = true;
                        if(!this.ShowThumbnailTooltip(tsmi, true) && (tsmi.ThumbnailIndex != this.iToolTipIndex)) {
                            if(this.timerToolTipByKey == null) {
                                this.timerToolTipByKey = new Timer(this.components);
                                this.timerToolTipByKey.Interval = SystemInformation.MouseHoverTime;
                                this.timerToolTipByKey.Tick += this.timerToolTipByKey_Tick;
                            }
                            this.timerToolTipByKey.Tag = tsmi;
                            this.timerToolTipByKey.Enabled = false;
                            this.timerToolTipByKey.Enabled = true;
                        }
                        return;
                    }
                }
                this.HideThumbnailTooltip(true);
            }
        }

        private void ddmr_MenuDragEnter(object sender, EventArgs e) {
            if(this.hwndFocusOnMenu != PInvoke.GetFocus()) {
                PInvoke.SetFocus(this.hwndFocusOnMenu);
            }
        }

        private void ddmr_MouseDragMove(object sender, MouseEventArgs e) {
            if(((e.Button == MouseButtons.Left) || (e.Button == MouseButtons.Right)) && (this.draggingItem != null)) {
                Size size = new Size(Math.Abs((e.X - this.pntDragStart.X)), Math.Abs((e.Y - this.pntDragStart.Y)));
                if((size.Width > SystemDragSize.Width) || (size.Height > SystemDragSize.Height)) {
                    DropDownMenuDropTarget ddmrt = (DropDownMenuDropTarget)sender;
                    ddmrt.SuppressMouseMove = false;
                    if(this.draggingItem.Checked) {
                        this.DoDragDropCheckedItems(ddmrt);
                    }
                    else if(!string.IsNullOrEmpty(this.draggingPath) && ((this.draggingPath.Length > 3) || Directory.Exists(this.draggingPath))) {
                        this.fDragStarted = true;
                        List<ToolStripItem> list = new List<ToolStripItem>();
                        foreach(ToolStripItem item in this.contextMenuSubDir.Items) {
                            list.Add(item);
                        }
                        ShellMethods.DoDragDrop(this.draggingPath, this);
                        if(!this.fDragStarted) {
                            foreach(ToolStripItem item2 in list) {
                                item2.Dispose();
                            }
                        }
                        this.fDragStarted = false;
                        this.contextMenuSubDir.Close(ToolStripDropDownCloseReason.ItemClicked);
                    }
                }
            }
        }

        private void ddmr_MouseLeave(object sender, EventArgs e) {
            DropDownMenuReorderable reorderable = (DropDownMenuReorderable)sender;
            reorderable.SuppressMouseMove = false;
            if(!reorderable.Bounds.Contains(MousePosition)) {
                this.HideThumbnailTooltip();
            }
        }

        private void ddmr_MouseScroll(object sender, EventArgs e) {
            this.HideThumbnailTooltip(true);
        }

        private void ddmr_MouseUpBeforeDrop(object sender, EventArgs e) {
            this.draggingPath = null;
            this.draggingItem = null;
            this.HideThumbnailTooltip();
        }

        private void ddmr_Opened(object sender, EventArgs e) {
            this.lstRcts.Add(((DropDownMenuReorderable)sender).Bounds);
        }

        private void ddmr_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) {
            if((Keys.Left <= e.KeyCode) && (e.KeyCode <= Keys.Down)) {
                if(this.timerToolTipByKey != null) {
                    this.timerToolTipByKey.Enabled = false;
                }
                if((this.iToolTipIndex != -1) && ((e.KeyCode == Keys.Up) || (e.KeyCode == Keys.Down))) {
                    ((DropDownMenuReorderable)sender).UpdateToolTipByKey(null);
                }
                this.iToolTipIndex = -1;
            }
        }

        private void directory_FromIDL_QueryVirtualMenu(object sender, EventArgs e) {
            QMenuItem item = (QMenuItem)sender;
            string path = item.Path;
            item.MouseMove += this.tsmi_Folder_MouseMove;
            bool flag = (path.ToLower() == @"a:\") || (path.ToLower() == @"b:\");
            bool flag2 = (path.Length == 3) ? (!flag && new DriveInfo(path).IsReady) : true;
            if((item.Target == MenuTarget.Folder) && flag2) {
                DropDownMenuDropTarget target = new DropDownMenuDropTarget(null, true, !this.fDesktop, false, this.hwndDialogParent);
                target.SuspendLayout();
                target.CheckOnEdgeClick = true;
                target.MessageParent = this.hwndMessageReflect;
                target.Items.Add(new ToolStripMenuItem("dummy"));
                target.ImageList = QTUtility.ImageListGlobal;
                target.SpaceKeyExecute = true;
                target.Path = path;
                target.MouseLeave += this.ddmr_MouseLeave;
                target.ItemRightClicked += this.ddmr_ItemRightClicked;
                target.Opened += this.ddmr_Opened;
                target.MouseScroll += this.ddmr_MouseScroll;
                if(item.Exists) {
                    target.MouseDragMove += this.ddmr_MouseDragMove;
                    target.MouseUpBeforeDrop += this.ddmr_MouseUpBeforeDrop;
                    target.KeyUp += this.ddmr_KeyUp;
                    target.PreviewKeyDown += this.ddmr_PreviewKeyDown;
                    target.MenuDragEnter += this.ddmr_MenuDragEnter;
                }
                item.DropDown = target;
                item.DropDownOpening += this.tsmi_DropDownOpening;
                item.DropDownItemClicked += this.ddmr_ItemClicked;
                target.ResumeLayout();
            }
        }

        private void directoryItem_QueryVirtualMenu(object sender, EventArgs e) {
            QMenuItem item = (QMenuItem)sender;
            item.MouseMove += this.tsmi_Folder_MouseMove;
            item.MouseDown += this.tsmi_MouseDown;
            item.MouseUp += this.tsmi_MouseUp;
            bool fSearchHidden = QTUtility.CheckConfig(Settings.SubDirTipsHidden);
            bool fSearchSystem = QTUtility.CheckConfig(Settings.SubDirTipsSystem);
            bool flag3 = QTUtility.CheckConfig(Settings.SubDirTipsFiles);
            bool flag4;
            using(FindFile file = new FindFile(item.TargetPath, fSearchHidden, fSearchSystem)) {
                flag4 = file.SubDirectoryExists() || (flag3 && file.SubFileExists());
            }
            if(flag4) {
                DropDownMenuDropTarget target = new DropDownMenuDropTarget(null, true, !this.fDesktop, false, this.hwndDialogParent);
                target.SuspendLayout();
                target.CheckOnEdgeClick = true;
                target.MessageParent = this.hwndMessageReflect;
                target.Items.Add(new ToolStripMenuItem("dummy"));
                target.ImageList = QTUtility.ImageListGlobal;
                target.SpaceKeyExecute = true;
                target.Path = item.TargetPath;
                target.MouseLeave += this.ddmr_MouseLeave;
                target.MouseDragMove += this.ddmr_MouseDragMove;
                target.MouseUpBeforeDrop += this.ddmr_MouseUpBeforeDrop;
                target.ItemRightClicked += this.ddmr_ItemRightClicked;
                target.Opened += this.ddmr_Opened;
                target.KeyUp += this.ddmr_KeyUp;
                target.PreviewKeyDown += this.ddmr_PreviewKeyDown;
                target.MenuDragEnter += this.ddmr_MenuDragEnter;
                target.MouseScroll += this.ddmr_MouseScroll;
                item.DropDown = target;
                item.DropDownOpening += this.tsmi_DropDownOpening;
                item.DropDownItemClicked += this.ddmr_ItemClicked;
                target.ResumeLayout();
            }
        }

        protected override void Dispose(bool disposing) {
            if(disposing && (this.components != null)) {
                this.components.Dispose();
            }
            if(this.thumbnailTip != null) {
                this.thumbnailTip.Dispose();
                this.thumbnailTip = null;
            }
            base.Dispose(disposing);
        }

        private void DoDragDropCheckedItems(DropDownMenuDropTarget ddmrt) {
            List<string> lstCheckedPaths = new List<string>();
            List<QMenuItem> lstCheckedItems = new List<QMenuItem>();
            if(this.GetCheckedItems(this.contextMenuSubDir, lstCheckedPaths, lstCheckedItems, true)) {
                if(lstCheckedPaths.Count > 0) {
                    try {
                        string directoryName = Path.GetDirectoryName(lstCheckedPaths[0]);
                        foreach(string str2 in lstCheckedPaths) {
                            if(!string.Equals(directoryName, Path.GetDirectoryName(str2), StringComparison.OrdinalIgnoreCase)) {
                                SystemSounds.Beep.Play();
                                ddmrt.SetSuppressMouseUp();
                                return;
                            }
                        }
                        this.fDragStarted = true;
                        List<ToolStripItem> list3 = new List<ToolStripItem>();
                        foreach(ToolStripItem item in this.contextMenuSubDir.Items) {
                            list3.Add(item);
                        }
                        ShellMethods.DoDragDrop(lstCheckedPaths, this, true);
                        if(!this.fDragStarted) {
                            foreach(ToolStripItem item2 in list3) {
                                item2.Dispose();
                            }
                        }
                        this.fDragStarted = false;
                        this.contextMenuSubDir.Close(ToolStripDropDownCloseReason.ItemClicked);
                    }
                    catch {
                    }
                }
                else {
                    SystemSounds.Beep.Play();
                    ddmrt.SetSuppressMouseUp();
                }
            }
        }

        private bool GetCheckedItems(DropDownMenuReorderable ddmr, List<string> lstCheckedPaths, List<QMenuItem> lstCheckedItems, bool fDragDrop) {
            bool flag = false;
            foreach(ToolStripItem item in ddmr.Items) {
                QMenuItem item2 = item as QMenuItem;
                if(item2 != null) {
                    if(item2.Checked) {
                        flag = true;
                        lstCheckedItems.Add(item2);
                        lstCheckedPaths.Add(item2.Path);
                        if(fDragDrop) {
                            continue;
                        }
                    }
                    if(item2.HasDropDownItems && this.GetCheckedItems((DropDownMenuReorderable)item2.DropDown, lstCheckedPaths, lstCheckedItems, fDragDrop)) {
                        flag = true;
                    }
                }
            }
            return flag;
        }

        public void HideMenu() {
            if(this.menuIsShowing) {
                this.contextMenuSubDir.Close(ToolStripDropDownCloseReason.AppFocusChange);
            }
        }

        public void HideSubDirTip(bool fForce = false) {
            if(fForce) {
                this.fShownByKey = false;
            }
            this.isShowing = false;
            this.currentDir = this.contextMenuSubDir.Path = string.Empty;
            this.currentIDL = null;
            if(this.menuIsShowing) {
                this.contextMenuSubDir.Close(ToolStripDropDownCloseReason.AppFocusChange);
            }
            PInvoke.ShowWindow(base.Handle, 0);
        }

        private void HideThumbnailTooltip() {
            if(((this.thumbnailTip != null) && this.thumbnailTip.IsShowing) && this.thumbnailTip.HideToolTip()) {
                this.iThumbnailIndex = -1;
            }
        }

        private void HideThumbnailTooltip(bool fKey) {
            if(this.thumbnailTip != null) {
                if(fKey) {
                    this.thumbnailTip.IsShownByKey = false;
                }
                this.HideThumbnailTooltip();
            }
        }

        private void InitializeComponent() {
            this.components = new Container();
            this.contextMenuSubDir = new DropDownMenuDropTarget(this.components, true, !this.fDesktop, true, this.hwndDialogParent);
            this.lblSubDirBtn = new LabelEx();
            base.SuspendLayout();
            this.contextMenuSubDir.SpaceKeyExecute = true;
            this.contextMenuSubDir.CheckOnEdgeClick = true;
            this.contextMenuSubDir.Closed += this.contextMenuSubDir_Closed;
            this.contextMenuSubDir.ItemClicked += this.ddmr_ItemClicked;
            this.contextMenuSubDir.ItemRightClicked += this.ddmr_ItemRightClicked;
            this.contextMenuSubDir.MouseDragMove += this.ddmr_MouseDragMove;
            this.contextMenuSubDir.MouseLeave += this.ddmr_MouseLeave;
            this.contextMenuSubDir.MouseUpBeforeDrop += this.ddmr_MouseUpBeforeDrop;
            this.contextMenuSubDir.MouseScroll += this.ddmr_MouseScroll;
            this.contextMenuSubDir.KeyUp += this.ddmr_KeyUp;
            this.contextMenuSubDir.PreviewKeyDown += this.ddmr_PreviewKeyDown;
            this.contextMenuSubDir.MenuDragEnter += this.ddmr_MenuDragEnter;
            this.lblSubDirBtn.BackColor = SystemColors.Window;
            this.lblSubDirBtn.Location = new Point(0, 0);
            this.lblSubDirBtn.Size = new Size(0x10, 0x10);
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(15, 15);
            base.Controls.Add(this.lblSubDirBtn);
            base.FormBorderStyle = FormBorderStyle.None;
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.Manual;
            base.ResumeLayout(false);
        }

        public bool MouseIsOnThis() {
            return base.RectangleToScreen(this.lblSubDirBtn.Bounds).Contains(MousePosition);
        }

        protected override void OnClick(EventArgs e) {
            if(this.menuIsShowing) {
                RECT rect2;
                this.lblSubDirBtn.SetPressed(false);
                PInvoke.GetWindowRect(base.Handle, out rect2);
                PInvoke.SetWindowPos(base.Handle, (IntPtr)(-1), rect2.left - 1, rect2.top - 1, 15, 15, 0x10);
                this.fClickClose = true;
                this.contextMenuSubDir.Close(ToolStripDropDownCloseReason.ItemClicked);
                return;
            }
            if(string.IsNullOrEmpty(this.currentDir)) {
                return;
            }
            this.contextMenuSubDir.Close(ToolStripDropDownCloseReason.ItemClicked);
            List<QMenuItem> lstItems = null;
            try {
                if(this.currentIDL != null) {
                    using(IDLWrapper wrapper = new IDLWrapper(this.currentIDL)) {
                        lstItems = this.CreateMenuFromIDL(wrapper, null);
                        goto Label_009D;
                    }
                }
                if(this.currentDir.StartsWith("::")) {
                    using(IDLWrapper wrapper2 = new IDLWrapper(this.currentDir)) {
                        lstItems = this.CreateMenuFromIDL(wrapper2, null);
                        goto Label_009D;
                    }
                }
                lstItems = this.CreateMenu(new DirectoryInfo(this.currentDir), null);
            }
            catch {
            }
        Label_009D:
            if((lstItems != null) && (lstItems.Count > 0)) {
                RECT rect;
                PInvoke.GetWindowRect(base.Handle, out rect);
                PInvoke.SetWindowPos(base.Handle, (IntPtr)(-1), rect.left + 1, rect.top + 1, 15, 15, 0x10);
                this.lblSubDirBtn.SetPressed(true);
                this.contextMenuSubDir.SuspendLayout();
                this.contextMenuSubDir.AddItemsRangeVirtual(lstItems);
                this.contextMenuSubDir.ResumeLayout();
                Rectangle workingArea = Screen.FromHandle(base.Handle).WorkingArea;
                Rectangle bounds = this.contextMenuSubDir.Bounds;
                if((((rect.right + 1) + bounds.Width) > workingArea.Right) && (((rect.bottom + 1) + bounds.Height) > workingArea.Bottom)) {
                    rect.right -= bounds.Width + 0x10;
                    if(this.fDropHilitedOpened) {
                        Point mousePosition = MousePosition;
                        if((rect.right < mousePosition.X) && (mousePosition.X < (rect.right + bounds.Width))) {
                            rect.right -= ((rect.right + bounds.Width) - mousePosition.X) + 0x11;
                        }
                    }
                }
                this.contextMenuSubDir.SetShowingByKey(this.fShownByKey);
                this.contextMenuSubDir.Show(new Point(rect.right + 1, rect.bottom + 1));
                this.contextMenuSubDir.Items[0].Select();
                this.menuIsShowing = true;
            }
        }

        public void OnExplorerInactivated() {
            if(!this.menuIsShowing) {
                this.HideSubDirTip();
            }
            else {
                Point mousePosition = MousePosition;
                if(!this.contextMenuSubDir.Bounds.Contains(mousePosition)) {
                    foreach(Rectangle rectangle in this.lstRcts) {
                        if(rectangle.Contains(mousePosition)) {
                            return;
                        }
                    }
                    this.HideSubDirTip();
                }
            }
        }

        public void PerformClickByKey() {
            if(this.isShowing && !this.menuIsShowing) {
                this.fShownByKey = true;
                this.OnClick(EventArgs.Empty);
            }
        }

        public void ShowMenu() {
            if(this.isShowing) {
                this.fDropHilitedOpened = true;
                this.OnClick(EventArgs.Empty);
            }
        }

        public bool ShowMenuWithoutShowForm(string path, Point pnt, bool fParent) {
            if(string.IsNullOrEmpty(path)) {
                goto Label_0258;
            }
            this.contextMenuSubDir.Close(ToolStripDropDownCloseReason.ItemClicked);
            this.currentDir = path;
            this.currentIDL = null;
            if(fParent) {
                this.contextMenuSubDir.Path = null;
            }
            else {
                this.contextMenuSubDir.Path = path;
            }
            List<QMenuItem> lstItems = null;
            try {
                if(fParent) {
                    using(IDLWrapper wrapper = new IDLWrapper(path)) {
                        lstItems = this.CreateParentMenu(wrapper, null);
                        lstItems.Reverse();
                        goto Label_00AC;
                    }
                }
                if(path.StartsWith("::")) {
                    using(IDLWrapper wrapper2 = new IDLWrapper(path)) {
                        lstItems = this.CreateMenuFromIDL(wrapper2, null);
                        goto Label_00AC;
                    }
                }
                DirectoryInfo di = new DirectoryInfo(this.currentDir);
                lstItems = this.CreateMenu(di, null);
            }
            catch {
            }
        Label_00AC:
            if((lstItems != null) && (lstItems.Count > 0)) {
                this.contextMenuSubDir.SuspendLayout();
                this.contextMenuSubDir.MaximumSize = Size.Empty;
                this.contextMenuSubDir.AddItemsRangeVirtual(lstItems);
                this.contextMenuSubDir.ResumeLayout();
                Screen screen = Screen.FromPoint(pnt);
                if(fParent) {
                    int y = pnt.Y;
                    pnt.Y -= this.contextMenuSubDir.Height;
                    if(screen.Bounds.Top > pnt.Y) {
                        if(((y - screen.Bounds.Top) - 8) < (10 + (lstItems[0].Height * 4))) {
                            pnt.X += 0x1a;
                        }
                        else {
                            this.contextMenuSubDir.MaximumSize = new Size(0, (y - screen.Bounds.Top) - 8);
                            pnt.Y = screen.Bounds.Top + 8;
                        }
                    }
                }
                else if((screen.Bounds.Height - pnt.Y) < this.contextMenuSubDir.Height) {
                    this.contextMenuSubDir.MaximumSize = new Size(0, (screen.Bounds.Height - pnt.Y) - 0x20);
                }
                this.contextMenuSubDir.Show(pnt);
                if(fParent) {
                    this.contextMenuSubDir.Items[lstItems.Count - 1].Select();
                }
                else {
                    this.contextMenuSubDir.Items[0].Select();
                }
                this.menuIsShowing = true;
                return true;
            }
        Label_0258:
            return false;
        }

        public void ShowSubDirTip(string path, byte[] idl, Point pnt) {
            this.lblSubDirBtn.SetPressed(false);
            IntPtr hwnd = PInvoke.WindowFromPoint(new Point(pnt.X, pnt.Y + 2));
            if(hwnd == lblSubDirBtn.Handle || hwnd == listViewWrapper.GetListViewHandle()) {
                this.isShowing = true;
                this.currentDir = this.contextMenuSubDir.Path = path;
                this.currentIDL = idl;
                PInvoke.SetWindowPos(base.Handle, (IntPtr)(-1), pnt.X, pnt.Y, 15, 15, 0x10);
                PInvoke.ShowWindow(base.Handle, 4);
            }
        }

        private bool ShowThumbnailTooltip(ToolStripMenuItemEx tsmi, bool fKey) {
            if((this.menuIsShowing && (this.draggingPath == null)) && !this.fSuppressThumbnail) {
                if((!QTUtility.CheckConfig(Settings.SubDirTipsPreview) ^ (ModifierKeys == Keys.Shift)) && ThumbnailTooltipForm.ExtIsSupported(Path.GetExtension(tsmi.ThumbnailPath).ToLower())) {
                    if(this.iThumbnailIndex == tsmi.ThumbnailIndex) {
                        return false;
                    }
                    if(this.thumbnailTip == null) {
                        this.thumbnailTip = new ThumbnailTooltipForm();
                    }
                    if(this.thumbnailTip.IsShownByKey && !fKey) {
                        this.thumbnailTip.IsShownByKey = false;
                        return false;
                    }
                    this.thumbnailTip.IsShownByKey = fKey;
                    this.iThumbnailIndex = tsmi.ThumbnailIndex;
                    if(this.thumbnailTip.ShowToolTip(tsmi.ThumbnailPath, tsmi.Owner.RectangleToScreen(tsmi.Bounds))) {
                        tsmi.ToolTipText = null;
                        return true;
                    }
                }
                if(tsmi.ToolTipText == null) {
                    string originalTitle = tsmi.OriginalTitle;
                    string shellInfoTipText = ShellMethods.GetShellInfoTipText(tsmi.Path, false);
                    if(shellInfoTipText != null) {
                        if(originalTitle == null) {
                            originalTitle = shellInfoTipText;
                        }
                        else {
                            originalTitle = originalTitle + "\r\n" + shellInfoTipText;
                        }
                    }
                    tsmi.ToolTipText = originalTitle;
                }
                this.HideThumbnailTooltip(fKey);
            }
            return false;
        }

        private void timerToolTipByKey_Tick(object sender, EventArgs e) {
            try {
                this.timerToolTipByKey.Enabled = false;
                ToolStripMenuItemEx tag = this.timerToolTipByKey.Tag as ToolStripMenuItemEx;
                if(((tag != null) && !tag.IsDisposed) && this.menuIsShowing) {
                    DropDownMenuReorderable owner = tag.Owner as DropDownMenuReorderable;
                    if(((owner != null) && owner.Visible) && (!owner.IsDisposed && !owner.Disposing)) {
                        owner.UpdateToolTipByKey(tag);
                        this.iToolTipIndex = tag.ThumbnailIndex;
                    }
                }
                this.timerToolTipByKey.Tag = null;
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception, null);
            }
        }

        private static bool TryGetParentIDL(IntPtr pIDL, out IntPtr pIDLParent) {
            pIDLParent = IntPtr.Zero;
            if(!(pIDL != IntPtr.Zero)) {
                return false;
            }
            IntPtr pidl = PInvoke.ILFindLastID(pIDL);
            uint num = PInvoke.ILGetSize(pIDL);
            uint num2 = PInvoke.ILGetSize(pidl);
            if(num != num2) {
                uint num3 = (num - num2) + 2;
                byte[] iDLData = ShellMethods.GetIDLData(pIDL);
                byte[] buffer2 = new byte[num3];
                pIDLParent = Marshal.AllocCoTaskMem((int)num3);
                Marshal.Copy(buffer2, 0, pIDLParent, (int)num3);
                Marshal.Copy(iDLData, 0, pIDLParent, ((int)num3) - 2);
                return true;
            }
            pIDLParent = Marshal.AllocCoTaskMem(2);
            byte[] source = new byte[2];
            Marshal.Copy(source, 0, pIDLParent, 2);
            return true;
        }

        private void tsmi_DropDownOpening(object sender, EventArgs e) {
            QMenuItem item = (QMenuItem)sender;
            item.DropDown.SuspendLayout();
            item.DropDownItems[0].Dispose();
            item.DropDownOpening -= this.tsmi_DropDownOpening;
            List<QMenuItem> lstItems = null;
            try {
                if(item.TargetPath.StartsWith("::")) {
                    using(IDLWrapper wrapper = new IDLWrapper(item.TargetPath)) {
                        lstItems = this.CreateMenuFromIDL(wrapper, item.IDLDataChild);
                        goto Label_008E;
                    }
                }
                DirectoryInfo di = new DirectoryInfo(item.TargetPath);
                lstItems = this.CreateMenu(di, item.PathChild);
            }
            catch {
            }
        Label_008E:
            if((lstItems != null) && (lstItems.Count > 0)) {
                ((DropDownMenuReorderable)item.DropDown).AddItemsRangeVirtual(lstItems);
            }
            item.DropDown.ResumeLayout();
        }

        private void tsmi_Files_MouseMove(object sender, MouseEventArgs e) {
            this.ShowThumbnailTooltip((ToolStripMenuItemEx)sender, false);
        }

        private void tsmi_Folder_MouseMove(object sender, MouseEventArgs e) {
            this.HideThumbnailTooltip();
            QMenuItem item = (QMenuItem)sender;
            if(item.ForceToolTip || (ModifierKeys == Keys.Shift)) {
                if((item.ToolTipText == null) || (item.ToolTipText == item.OriginalTitle)) {
                    string originalTitle = item.OriginalTitle;
                    string shellInfoTipText = ShellMethods.GetShellInfoTipText(item.Path, true);
                    if(shellInfoTipText != null) {
                        if(originalTitle == null) {
                            originalTitle = shellInfoTipText;
                        }
                        else {
                            originalTitle = originalTitle + "\r\n" + shellInfoTipText;
                        }
                    }
                    item.ToolTipText = originalTitle;
                }
            }
            else if(item.OriginalTitle != null) {
                item.ToolTipText = item.OriginalTitle;
            }
        }

        private void tsmi_MouseDown(object sender, MouseEventArgs e) {
            if((e.Button == MouseButtons.Left) || (e.Button == MouseButtons.Right)) {
                QMenuItem item = (QMenuItem)sender;
                DropDownMenuReorderable owner = (DropDownMenuReorderable)item.Owner;
                owner.SuppressStartIndex = owner.Items.IndexOf(item);
                owner.SuppressMouseMove = true;
                this.draggingItem = item;
                this.draggingPath = item.Path;
                this.pntDragStart = owner.PointToClient(MousePosition);
            }
        }

        private void tsmi_MouseUp(object sender, MouseEventArgs e) {
            this.draggingPath = null;
            this.draggingItem = null;
        }

        protected override void WndProc(ref Message m) {
            if(m.Msg == WM.MOUSEACTIVATE) {
                if(((((int)((long)m.LParam)) >> 0x10) & 0xffff) == 0x201) {
                    this.OnClick(EventArgs.Empty);
                }
                m.Result = (IntPtr)4;
            }
            else if(((m.Msg == WM.INITMENUPOPUP) || (m.Msg == WM.DRAWITEM)) || (m.Msg == WM.MEASUREITEM)) {
                if(this.hwndMessageReflect != IntPtr.Zero) {
                    PInvoke.SendMessage(this.hwndMessageReflect, (uint)m.Msg, m.WParam, m.LParam);
                }
            }
            else {
                base.WndProc(ref m);
            }
        }

        public List<string> ExecutedDirectories {
            get {
                return new List<string>(this.lstTempDirectoryPaths);
            }
        }

        public bool IsMouseOnMenus {
            get {
                if(this.contextMenuSubDir.Visible) {
                    Point mousePosition = MousePosition;
                    if(this.contextMenuSubDir.Bounds.Contains(mousePosition)) {
                        return true;
                    }
                    foreach(Rectangle rectangle in this.lstRcts) {
                        if(rectangle.Contains(mousePosition)) {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public bool IsShowing {
            get {
                return this.isShowing;
            }
        }

        public bool IsShownByKey {
            get {
                return this.fShownByKey;
            }
        }

        public bool MenuIsShowing {
            get {
                return this.menuIsShowing;
            }
        }

        private sealed class ExtComparer : IComparer<QMenuItem> {
            public int Compare(QMenuItem x, QMenuItem y) {
                if((x.Extension.Length == 0) && (y.Extension.Length == 0)) {
                    return string.Compare(x.Name, y.Name);
                }
                if(x.Extension.Length == 0) {
                    return 1;
                }
                if(y.Extension.Length == 0) {
                    return -1;
                }
                int num = string.Compare(x.Extension, y.Extension);
                if(num == 0) {
                    return string.Compare(x.Name, y.Name);
                }
                return num;
            }
        }

        private sealed class LabelEx : Label {
            private static Bitmap bmpCold = Resources_Image.imgSubDirBtnCold;
            private static Bitmap bmpPrssed = Resources_Image.imgSubDirBtnPress;
            private bool fPressed;

            protected override void OnPaint(PaintEventArgs e) {
                e.Graphics.DrawImage(this.fPressed ? bmpPrssed : bmpCold, new Rectangle(0, 0, 15, 15), new Rectangle(0, 0, 15, 15), GraphicsUnit.Pixel);
            }

            public void SetPressed(bool fPressed) {
                this.fPressed = fPressed;
                base.Invalidate();
            }
        }

        internal sealed class ToolStripMenuItemEx : QMenuItem {
            private int thumbnailIndex;
            private string thumbnailPath;

            public ToolStripMenuItemEx(string title)
                : base(title, MenuTarget.File, MenuGenre.SubDirTip) {
            }

            public int ThumbnailIndex {
                get {
                    return this.thumbnailIndex;
                }
                set {
                    this.thumbnailIndex = value;
                }
            }

            public string ThumbnailPath {
                get {
                    return this.thumbnailPath;
                }
                set {
                    this.thumbnailPath = value;
                }
            }
        }

        private sealed class ToolStripMeuItemComparer : IComparer<QMenuItem> {
            public int Compare(QMenuItem x, QMenuItem y) {
                return string.Compare(x.Text, y.Text);
            }
        }
    }
}
