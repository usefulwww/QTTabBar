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
    using Microsoft.Win32;
    using QTTabBarLib.Interop;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Windows.Forms;

    internal static class MenuUtility {
        private static void AddChildrenOnOpening(DirectoryMenuItem parentItem) {
            bool flag;
            DirectoryInfo info = new DirectoryInfo(parentItem.Path);
            EventPack eventPack = parentItem.EventPack;
            foreach(DirectoryInfo info2 in info.GetDirectories()) {
                if((info2.Attributes & FileAttributes.Hidden) == 0) {
                    string text = QTUtility2.MakeNameEllipsis(info2.Name, out flag);
                    DropDownMenuReorderable reorderable = new DropDownMenuReorderable(null);
                    reorderable.MessageParent = eventPack.MessageParentHandle;
                    reorderable.ItemRightClicked += eventPack.ItemRightClickEventHandler;
                    reorderable.ImageList = QTUtility.ImageListGlobal;
                    DirectoryMenuItem item = new DirectoryMenuItem(text);
                    item.SetImageReservationKey(info2.FullName, null);
                    item.Path = info2.FullName;
                    item.EventPack = eventPack;
                    item.ModifiiedDate = info2.LastWriteTime;
                    if(flag) {
                        item.ToolTipText = info2.Name;
                    }
                    item.DropDown = reorderable;
                    item.DoubleClickEnabled = true;
                    item.DropDownItems.Add(new ToolStripMenuItem());
                    item.DropDownItemClicked += new ToolStripItemClickedEventHandler(MenuUtility.realDirectory_DropDownItemClicked);
                    item.DropDownOpening += new EventHandler(MenuUtility.realDirectory_DropDownOpening);
                    item.DoubleClick += eventPack.DirDoubleClickEventHandler;
                    parentItem.DropDownItems.Add(item);
                }
            }
            foreach(FileInfo info3 in info.GetFiles()) {
                if((info3.Attributes & FileAttributes.Hidden) == 0) {
                    string fileNameWithoutExtension;
                    string ext = info3.Extension.ToLower();
                    switch(ext) {
                        case ".lnk":
                        case ".url":
                            fileNameWithoutExtension = Path.GetFileNameWithoutExtension(info3.Name);
                            break;

                        default:
                            fileNameWithoutExtension = info3.Name;
                            break;
                    }
                    string str4 = fileNameWithoutExtension;
                    QMenuItem item2 = new QMenuItem(QTUtility2.MakeNameEllipsis(fileNameWithoutExtension, out flag), MenuTarget.File, MenuGenre.Application);
                    item2.Path = info3.FullName;
                    item2.SetImageReservationKey(info3.FullName, ext);
                    item2.MouseMove += new MouseEventHandler(MenuUtility.qmi_File_MouseMove);
                    if(flag) {
                        item2.ToolTipText = str4;
                    }
                    parentItem.DropDownItems.Add(item2);
                }
            }
        }

        public static List<ToolStripItem> CreateAppLauncherItems(IntPtr hwndParent, bool fReorderEnabled, ItemRightClickedEventHandler rightClickHandler, EventHandler dirDoubleClickEvent, bool fFromTaskBar) {
            QTUtility.RefreshUserAppDic(false);
            List<ToolStripItem> list = new List<ToolStripItem>();
            EventPack ep = new EventPack(hwndParent, rightClickHandler, dirDoubleClickEvent, fFromTaskBar);
            foreach(string str in QTUtility.UserAppsDic.Keys) {
                string[] appVals = QTUtility.UserAppsDic[str];
                if(appVals != null) {
                    list.Add(CreateMenuItem_AppLauncher(str, appVals, ep));
                }
                else {
                    using(RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Quizo\QTTabBar\UserApps\" + str, false)) {
                        if(key != null) {
                            ToolStripItem item = CreateMenuItem_AppLauncher_Virtual(str, fReorderEnabled, key, ep);
                            if(item != null) {
                                list.Add(item);
                            }
                        }
                        continue;
                    }
                }
            }
            return list;
        }

        public static void CreateGroupItems(ToolStripDropDownItem dropDownItem) {
            QTUtility.RefreshGroupsDic();
            if(QTUtility.GroupPathsDic.Count > 0) {
                dropDownItem.Enabled = true;
                DropDownMenuReorderable dropDown = (DropDownMenuReorderable)dropDownItem.DropDown;
                while(dropDown.Items.Count > 0) {
                    dropDown.Items[0].Dispose();
                }
                dropDown.ItemsClear();
                string key = "groups";
                foreach(string str2 in QTUtility.GroupPathsDic.Keys) {
                    string str3 = QTUtility.GroupPathsDic[str2];
                    string path = str3.Split(QTUtility.SEPARATOR_CHAR)[0];
                    if(str3.Length == 0) {
                        dropDown.AddItem(new ToolStripSeparator(), key);
                    }
                    else if(QTUtility2.PathExists(path)) {
                        QMenuItem item = new QMenuItem(str2, MenuGenre.Group);
                        item.SetImageReservationKey(path, null);
                        dropDown.AddItem(item, key);
                        if(QTUtility.StartUpGroupList.Contains(str2)) {
                            if(QTUtility.StartUpTabFont == null) {
                                QTUtility.StartUpTabFont = new Font(item.Font, FontStyle.Underline);
                            }
                            item.Font = QTUtility.StartUpTabFont;
                        }
                    }
                }
            }
            else {
                dropDownItem.Enabled = false;
            }
        }

        public static QMenuItem CreateMenuItem(MenuItemArguments mia) {
            QMenuItem item = new QMenuItem(QTUtility2.MakePathDisplayText(mia.Path, false), mia);
            if(((mia.Genre == MenuGenre.Navigation) && mia.IsBack) && (mia.Index == 0)) {
                item.ImageKey = "current";
            }
            else {
                item.SetImageReservationKey(mia.Path, null);
            }
            item.ToolTipText = QTUtility2.MakePathDisplayText(mia.Path, true);
            return item;
        }

        private static ToolStripItem CreateMenuItem_AppLauncher(string key, string[] appVals, EventPack ep) {
            string name = appVals[0];
            try {
                name = Environment.ExpandEnvironmentVariables(name);
            }
            catch {
            }
            MenuItemArguments mia = new MenuItemArguments(name, appVals[1], appVals[2], 0, MenuGenre.Application);
            if((appVals[0].Length == 0) || string.Equals(appVals[0], "separator", StringComparison.OrdinalIgnoreCase)) {
                ToolStripSeparator separator = new ToolStripSeparator();
                separator.Name = "appSep";
                return separator;
            }
            if(appVals.Length == 4) {
                int.TryParse(appVals[3], out mia.KeyShortcut);
            }
            if((name.StartsWith(@"\\") || name.StartsWith("::")) || !Directory.Exists(name)) {
                mia.Target = MenuTarget.File;
                QMenuItem item = new QMenuItem(key, mia);
                item.Name = key;
                item.SetImageReservationKey(name, Path.GetExtension(name));
                item.MouseMove += new MouseEventHandler(MenuUtility.qmi_File_MouseMove);
                if(!ep.FromTaskBar && (mia.KeyShortcut > 0x100000)) {
                    int num = mia.KeyShortcut & -1048577;
                    item.ShortcutKeyDisplayString = QTUtility2.MakeKeyString((Keys)num).Replace(" ", string.Empty);
                }
                return item;
            }
            mia.Target = MenuTarget.Folder;
            DropDownMenuReorderable reorderable = new DropDownMenuReorderable(null);
            reorderable.MessageParent = ep.MessageParentHandle;
            reorderable.ItemRightClicked += ep.ItemRightClickEventHandler;
            reorderable.ImageList = QTUtility.ImageListGlobal;
            DirectoryMenuItem item2 = new DirectoryMenuItem(key);
            item2.SetImageReservationKey(name, null);
            item2.Name = key;
            item2.Path = name;
            item2.MenuItemArguments = mia;
            item2.EventPack = ep;
            item2.ModifiiedDate = Directory.GetLastWriteTime(name);
            item2.DropDown = reorderable;
            item2.DoubleClickEnabled = true;
            item2.DropDownItems.Add(new ToolStripMenuItem());
            item2.DropDownItemClicked += new ToolStripItemClickedEventHandler(MenuUtility.realDirectory_DropDownItemClicked);
            item2.DropDownOpening += new EventHandler(MenuUtility.realDirectory_DropDownOpening);
            item2.DoubleClick += ep.DirDoubleClickEventHandler;
            return item2;
        }

        private static ToolStripItem CreateMenuItem_AppLauncher_Virtual(string key, bool fReorderEnabled, RegistryKey rkSub, EventPack ep) {
            string[] valueNames = rkSub.GetValueNames();
            if((valueNames != null) && (valueNames.Length > 0)) {
                List<ToolStripItem> list = new List<ToolStripItem>();
                foreach(string str in valueNames) {
                    string[] appVals = QTUtility2.ReadRegBinary<string>(str, rkSub);
                    if(appVals != null) {
                        if((appVals.Length == 3) || (appVals.Length == 4)) {
                            list.Add(CreateMenuItem_AppLauncher(str, appVals, ep));
                        }
                    }
                    else {
                        using(RegistryKey key2 = rkSub.OpenSubKey(str, false)) {
                            if(key2 != null) {
                                ToolStripItem item = CreateMenuItem_AppLauncher_Virtual(str, fReorderEnabled, key2, ep);
                                if(item != null) {
                                    list.Add(item);
                                }
                            }
                        }
                    }
                }
                if(list.Count > 0) {
                    QMenuItem item2 = new QMenuItem(key, new MenuItemArguments(key, MenuTarget.VirtualFolder, MenuGenre.Application));
                    item2.ImageKey = "folder";
                    item2.Name = key;
                    DropDownMenuReorderable reorderable = new DropDownMenuReorderable(null);
                    reorderable.ReorderEnabled = fReorderEnabled;
                    reorderable.MessageParent = ep.MessageParentHandle;
                    reorderable.ItemRightClicked += ep.ItemRightClickEventHandler;
                    reorderable.ImageList = QTUtility.ImageListGlobal;
                    reorderable.AddItemsRange(list.ToArray(), "userappItem");
                    reorderable.ItemClicked += new ToolStripItemClickedEventHandler(MenuUtility.virtualDirectory_DropDownItemClicked);
                    reorderable.ReorderFinished += new MenuReorderedEventHandler(MenuUtility.virtualDirectory_ReorderFinished);
                    string name = rkSub.Name;
                    reorderable.Name = name.Substring(name.IndexOf(@"Software\Quizo\QTTabBar\UserApps\"));
                    item2.DropDown = reorderable;
                    return item2;
                }
            }
            return null;
        }

        public static List<ToolStripItem> CreateRecentFilesItems() {
            List<ToolStripItem> list = new List<ToolStripItem>();
            List<string> list2 = new List<string>();
            if(QTUtility.ExecutedPathsList.Count > 0) {
                for(int i = QTUtility.ExecutedPathsList.Count - 1; i >= 0; i--) {
                    string path = QTUtility.ExecutedPathsList[i];
                    if(path.StartsWith(@"\\") || File.Exists(path)) {
                        bool flag;
                        QMenuItem item = new QMenuItem(QTUtility2.MakeNameEllipsis(Path.GetFileName(path), out flag), MenuGenre.RecentFile);
                        item.Path = item.ToolTipText = path;
                        item.SetImageReservationKey(path, Path.GetExtension(path));
                        list.Add(item);
                    }
                    else {
                        list2.Add(path);
                    }
                }
            }
            foreach(string str2 in list2) {
                QTUtility.ExecutedPathsList.Remove(str2);
            }
            return list;
        }

        public static List<ToolStripItem> CreateUndoClosedItems(ToolStripDropDownItem dropDownItem) {
            List<ToolStripItem> list = new List<ToolStripItem>();
            string[] strArray = QTUtility.ClosedTabHistoryList.ToArray();
            bool flag = dropDownItem != null;
            if(flag) {
                while(dropDownItem.DropDownItems.Count > 0) {
                    dropDownItem.DropDownItems[0].Dispose();
                }
            }
            if(strArray.Length > 0) {
                if(flag) {
                    dropDownItem.Enabled = true;
                }
                for(int i = strArray.Length - 1; i >= 0; i--) {
                    if(strArray[i].Length > 0) {
                        if(!QTUtility2.PathExists(strArray[i])) {
                            QTUtility.ClosedTabHistoryList.Remove(strArray[i]);
                        }
                        else {
                            QMenuItem item = CreateMenuItem(new MenuItemArguments(strArray[i], MenuTarget.Folder, MenuGenre.History));
                            if(flag) {
                                dropDownItem.DropDownItems.Add(item);
                            }
                            else {
                                list.Add(item);
                            }
                        }
                    }
                }
                return list;
            }
            if(flag) {
                dropDownItem.Enabled = false;
            }
            return list;
        }

        public static void GroupMenu_ItemRightClicked(object sender, ItemRightClickedEventArgs e) {
            DropDownMenuReorderable reorderable = (DropDownMenuReorderable)sender;
            string str = TrackGroupContextMenu(e.ClickedItem.Text, e.IsKey ? e.Point : Control.MousePosition, reorderable.Handle);
            if(!string.IsNullOrEmpty(str)) {
                QTUtility2.SendCOPYDATASTRUCT(QTUtility.instanceManager.CurrentHandle, (IntPtr)0xf30, str, IntPtr.Zero);
            }
            else {
                e.HRESULT = 0xfffd;
            }
        }

        private static void qmi_File_MouseMove(object sender, MouseEventArgs e) {
            if(QTUtility.CheckConfig(0, 8)) {
                QMenuItem item = (QMenuItem)sender;
                if((item.ToolTipText == null) && !string.IsNullOrEmpty(item.Path)) {
                    string str = item.Path.StartsWith("::") ? item.Text : Path.GetFileName(item.Path);
                    string shellInfoTipText = ShellMethods.GetShellInfoTipText(item.Path, false);
                    if(shellInfoTipText != null) {
                        if(str == null) {
                            str = shellInfoTipText;
                        }
                        else {
                            str = str + "\r\n" + shellInfoTipText;
                        }
                    }
                    item.ToolTipText = str;
                }
            }
        }

        private static void realDirectory_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            if(!(e.ClickedItem is DirectoryMenuItem)) {
                try {
                    Process.Start(((QMenuItem)e.ClickedItem).Path);
                }
                catch {
                    MessageBox.Show("Operation failed.\r\nPlease make sure the file or the target of link exists:\r\n\r\n\t" + e.ClickedItem.Name, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
            }
        }

        private static void realDirectory_DropDownOpening(object sender, EventArgs e) {
            DirectoryMenuItem parentItem = (DirectoryMenuItem)sender;
            if(!parentItem.OnceOpened) {
                parentItem.OnceOpened = true;
                parentItem.DropDown.SuspendLayout();
                parentItem.DropDownItems[0].Dispose();
                AddChildrenOnOpening(parentItem);
                parentItem.DropDown.ResumeLayout();
                if(QTUtility.IsVista) {
                    parentItem.DropDown.BringToFront();
                }
            }
            else {
                DateTime lastWriteTime = Directory.GetLastWriteTime(parentItem.Path);
                if(parentItem.ModifiiedDate != lastWriteTime) {
                    parentItem.DropDown.SuspendLayout();
                    parentItem.ModifiiedDate = lastWriteTime;
                    while(parentItem.DropDownItems.Count > 0) {
                        parentItem.DropDownItems[0].Dispose();
                    }
                    AddChildrenOnOpening(parentItem);
                    parentItem.DropDown.ResumeLayout();
                }
            }
        }

        public static string TrackGroupContextMenu(string groupName, Point pnt, IntPtr pDropDownHandle) {
            string str;
            string name = string.Empty;
            if(!QTUtility.GroupPathsDic.TryGetValue(groupName, out str)) {
                return name;
            }
            string[] strArray = str.Split(QTUtility.SEPARATOR_CHAR);
            ContextMenu menu = new ContextMenu();
            if(QTUtility.IsVista) {
                for(int i = 0; i < strArray.Length; i++) {
                    string text = string.Empty;
                    if(strArray[i].StartsWith(@"\\")) {
                        text = strArray[i];
                    }
                    else {
                        text = ShellMethods.GetDisplayName(strArray[i]);
                    }
                    MenuItem item = new MenuItem(text);
                    item.Name = strArray[i];
                    menu.MenuItems.Add(item);
                }
            }
            else {
                for(int j = 0; j < strArray.Length; j++) {
                    string displayName = string.Empty;
                    if(strArray[j].StartsWith(@"\\")) {
                        displayName = strArray[j];
                    }
                    else {
                        displayName = ShellMethods.GetDisplayName(strArray[j]);
                    }
                    MenuItemEx ex = new MenuItemEx(displayName);
                    ex.Name = strArray[j];
                    ex.Image = QTUtility.ImageListGlobal.Images[QTUtility.GetImageKey(strArray[j], null)];
                    menu.MenuItems.Add(ex);
                }
            }
            List<IntPtr> list = new List<IntPtr>();
            if(QTUtility.IsVista) {
                for(int k = 0; k < strArray.Length; k++) {
                    string imageKey = QTUtility.GetImageKey(strArray[k], null);
                    IntPtr hbitmap = ((Bitmap)QTUtility.ImageListGlobal.Images[imageKey]).GetHbitmap(System.Drawing.Color.Black);
                    if(hbitmap != IntPtr.Zero) {
                        list.Add(hbitmap);
                        PInvoke.SetMenuItemBitmaps(menu.Handle, k, 0x400, hbitmap, IntPtr.Zero);
                    }
                }
            }
            uint maxValue = uint.MaxValue;
            if(menu.MenuItems.Count > 0) {
                maxValue = PInvoke.TrackPopupMenu(menu.Handle, 0x180, pnt.X, pnt.Y, 0, pDropDownHandle, IntPtr.Zero);
                if(maxValue != 0) {
                    for(int m = 0; m < menu.MenuItems.Count; m++) {
                        if(maxValue == PInvoke.GetMenuItemID(menu.Handle, m)) {
                            name = menu.MenuItems[m].Name;
                            break;
                        }
                    }
                }
            }
            menu.Dispose();
            if(QTUtility.IsVista) {
                foreach(IntPtr ptr2 in list) {
                    PInvoke.DeleteObject(ptr2);
                }
            }
            if(maxValue != 0) {
                return name;
            }
            return null;
        }

        private static void virtualDirectory_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            QMenuItem clickedItem = e.ClickedItem as QMenuItem;
            if((clickedItem != null) && (clickedItem.Target == MenuTarget.File)) {
                if(!clickedItem.MenuItemArguments.TokenReplaced) {
                    AppLauncher.ReplaceAllTokens(clickedItem.MenuItemArguments, string.Empty);
                }
                AppLauncher.Execute(clickedItem.MenuItemArguments, IntPtr.Zero);
            }
        }

        private static void virtualDirectory_ReorderFinished(object sender, ToolStripItemClickedEventArgs e) {
            DropDownMenuReorderable reorderable = (DropDownMenuReorderable)sender;
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(reorderable.Name)) {
                if(key != null) {
                    foreach(string str in key.GetValueNames()) {
                        key.DeleteValue(str, false);
                    }
                    int num = 1;
                    string[] array = new string[] { "separator", string.Empty, string.Empty };
                    foreach(ToolStripItem item in reorderable.Items) {
                        if(item is ToolStripSeparator) {
                            QTUtility2.WriteRegBinary<string>(array, "Separator" + num++, key);
                        }
                        else {
                            QMenuItem item2 = item as QMenuItem;
                            if(item2 != null) {
                                MenuItemArguments menuItemArguments = item2.MenuItemArguments;
                                if(menuItemArguments.Target == MenuTarget.VirtualFolder) {
                                    key.SetValue(item.Name, new byte[0]);
                                    continue;
                                }
                                string[] strArray2 = new string[] { menuItemArguments.Path, menuItemArguments.OriginalArgument, menuItemArguments.OriginalWorkingDirectory, menuItemArguments.KeyShortcut.ToString() };
                                QTUtility2.WriteRegBinary<string>(strArray2, item.Name, key);
                            }
                        }
                    }
                    QTUtility.fRequiredRefresh_App = true;
                    QTTabBarClass.SyncTaskBarMenu();
                }
            }
        }
    }
}
