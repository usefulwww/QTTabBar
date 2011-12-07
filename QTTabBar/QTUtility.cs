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
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;
using Microsoft.Win32;
using QTPlugin;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    internal static class QTUtility {
        internal static Version BetaRevision = new Version(0, 3);
        internal static PathList ClosedTabHistoryList = new PathList(0x10);
        internal static byte[] ConfigValues;
        internal static string CreateWindowTMPGroup = string.Empty;
        internal static string CreateWindowTMPPath = string.Empty;
        internal static Version CurrentVersion = new Version(1, 5, 0, 0);
        internal static Dictionary<string, int> dicGroupNamesAndKeys = new Dictionary<string, int>();
        internal static Dictionary<int, string> dicGroupShortcutKeys = new Dictionary<int, string>();
        internal static Dictionary<string, int[]> dicPluginShortcutKeys = new Dictionary<string, int[]>();
        internal static Dictionary<int, MenuItemArguments> dicUserAppShortcutKeys = new Dictionary<int, MenuItemArguments>();
        internal static Dictionary<string, string> DisplayNameCacheDic = new Dictionary<string, string>();
        internal static PathList ExecutedPathsList = new PathList(0x10);
        internal static bool fExplorerPrevented;
        internal const int FLAG_KEYENABLED = 0x100000;
        internal static bool fIsDevelopmentVersion = true;  // <----------------- Change me before releasing!
        internal static bool fRequiredRefresh_App;
        internal static bool fRestoreFolderTree;
        internal static bool fSingleClick;
        internal static Dictionary<string, string> GroupPathsDic = new Dictionary<string, string>();
        internal static int iIconUnderLineVal;
        internal const string IMAGEKEY_FOLDER = "folder";
        internal const string IMAGEKEY_MYNETWORK = "mynetwork";
        internal const string IMAGEKEY_NOEXT = "noext";
        internal const string IMAGEKEY_NOIMAGE = "noimage";
        internal static ImageList ImageListGlobal;
        internal static int InstancesCount;
        internal static readonly bool IsRTL = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft;
        internal static readonly bool IsXP = Environment.OSVersion.Version.Major <= 5;
        internal static Dictionary<string, byte[]> ITEMIDLIST_Dic_Session = new Dictionary<string, byte[]>();
        internal static List<string> LockedTabsToRestoreList = new List<string>();
        internal const BindAction LAST_KEYBOARD_ACTION = BindAction.FocusTabBar;
        internal static List<string> NoCapturePathsList = new List<string>();
        internal static bool NowDebugging = 
#if DEBUG
            true;
#else
            false;
#endif
        internal static string Path_LanguageFile;
        internal static string PATH_MYNETWORK;
        internal static string Path_PluginLangFile;
        internal static string PATH_SEARCHFOLDER;
        internal static string PathToSelectInCommandLineArg;
        internal const string REGUSER = RegConst.Root;
        internal static string[] ResMain;
        internal static string[] ResMisc;
        internal static bool RestoreFolderTree_Hide;
        internal static SolidBrush sbAlternate;
        internal static readonly char[] SEPARATOR_CHAR = new char[] { ';' };
        internal const string SEPARATOR_PATH_HASH_SESSION = "*?*?*";
        internal static List<string> StartUpGroupList = new List<string>();
        internal static string StartUpGroupNameNowOpening = string.Empty;
        internal static Font StartUpTabFont;
        internal static Dictionary<string, string[]> TextResourcesDic;
        internal static List<byte[]> TMPIDLList = new List<byte[]>();
        internal static List<string> TMPPathList = new List<string>();
        internal static byte[] TMPTargetIDL;
        internal static Dictionary<string, string[]> UserAppsDic = new Dictionary<string, string[]>();
        internal static byte WindowAlpha = 0xff;

        static QTUtility() {
            // I'm tempted to just return for everything except "explorer"
            // Maybe I should...
            String processName = Process.GetCurrentProcess().ProcessName.ToLower();
            if(processName == "iexplore" || processName == "regasm" || processName == "gacutil") {
                return;
            }

            try {
                // Load the config
                ConfigManager.Initialize();

                // Create and enable the API hooks
                HookLibManager.Initialize();

                // Create the global imagelist
                ImageListGlobal = new ImageList { ColorDepth = ColorDepth.Depth32Bit };
                ImageListGlobal.Images.Add("folder", GetIcon(string.Empty, false));

                using(RegistryKey key = Registry.CurrentUser.CreateSubKey(RegConst.Root)) {
                    if(key != null) {
                        string path = (string)key.GetValue("LanguageFile", string.Empty);
                        if((path.Length > 0) && File.Exists(path)) {
                            Path_LanguageFile = path;
                            TextResourcesDic = ReadLanguageFile(path);
                        }
                        else {
                            Path_LanguageFile = string.Empty;
                        }
                        ValidateTextResources();

                        using(RegistryKey key2 = key.CreateSubKey("RecentlyClosed")) {
                            if(key2 != null) {
                                List<string> collection = key2.GetValueNames()
                                        .Select(str4 => (string)key2.GetValue(str4)).ToList();
                                ClosedTabHistoryList = new PathList(collection, Config.Misc.TabHistoryCount);
                            }
                        }
                        if(Config.AllRecentFiles) {
                            using(RegistryKey key3 = key.CreateSubKey("RecentFiles")) {
                                if(key3 != null) {
                                    List<string> list2 = key3.GetValueNames().Select(str5 =>
                                            (string)key3.GetValue(str5)).ToList();
                                    ExecutedPathsList = new PathList(list2, Config.Misc.FileHistoryCount);
                                }
                            }
                        }
                        RefreshGroupShortcutKeyDic(key);
                        RefreshLockedTabsList();
                        string str6 = (string)key.GetValue("StartUpGroups", string.Empty);
                        if(str6.Length > 0) {
                            StartUpGroupList = new List<string>(str6.Split(SEPARATOR_CHAR));
                        }
                        string str7 = (string)key.GetValue("NoCaptureAt", string.Empty);
                        if(str7.Length > 0) {
                            NoCapturePathsList = new List<string>(str7.Split(SEPARATOR_CHAR));
                        }
                        if(!byte.TryParse((string)key.GetValue("WindowAlpha", "255"), out WindowAlpha)) {
                            WindowAlpha = 0xff;
                        }
                    }
                }
                RefreshGroupsDic();
                RefreshUserAppDic(true);
                if(!IsXP) {
                    PATH_SEARCHFOLDER = "::{9343812E-1C37-4A49-A12E-4B2D810D956B}";
                    PATH_MYNETWORK = "::{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}";
                }
                else {
                    PATH_SEARCHFOLDER = "::{E17D4FC0-5564-11D1-83F2-00A0C90DC849}";
                    PATH_MYNETWORK = "::{208D2C60-3AEA-1069-A2D7-08002B30309D}";
                    GetShellClickMode();
                }

                // Initialize plugins
                PluginManager.Initialize();
            }
            catch(Exception exception) {
                // TODO: Any errors here would be very serious.  Alert the user as such.
                QTUtility2.MakeErrorLog(exception);
            }
        }

        public static bool ExtHasIcon(string ext) {
            if(ext != ".exe" && ext != ".lnk" && ext != ".ico" && ext != ".url") {
                return (ext == ".sln");
            }
            return true;
        }

        public static bool ExtIsCompressed(string ext) {
            if(ext != ".zip" && ext != ".lzh") {
                return (ext == ".cab");
            }
            return true;
        }

        public static Icon GetIcon(IntPtr pIDL) {
            SHFILEINFO psfi = new SHFILEINFO();
            if((IntPtr.Zero != PInvoke.SHGetFileInfo(pIDL, 0, ref psfi, Marshal.SizeOf(psfi), 0x109)) && (psfi.hIcon != IntPtr.Zero)) {
                Icon icon = new Icon(Icon.FromHandle(psfi.hIcon), 0x10, 0x10);
                PInvoke.DestroyIcon(psfi.hIcon);
                return icon;
            }
            return Resources_Image.icoEmpty;
        }

        public static Icon GetIcon(string path, bool fExtension) {
            Icon icon;
            SHFILEINFO psfi = new SHFILEINFO();
            if(fExtension) {
                if(path.Length == 0) {
                    path = ".*";
                }
                if((IntPtr.Zero != PInvoke.SHGetFileInfo("*" + path, 0x80, ref psfi, Marshal.SizeOf(psfi), 0x111)) && (psfi.hIcon != IntPtr.Zero)) {
                    icon = new Icon(Icon.FromHandle(psfi.hIcon), 0x10, 0x10);
                    PInvoke.DestroyIcon(psfi.hIcon);
                    return icon;
                }
                return Resources_Image.icoEmpty;
            }
            if(path.Length == 0) {
                if((IntPtr.Zero != PInvoke.SHGetFileInfo("dummy", 0x10, ref psfi, Marshal.SizeOf(psfi), 0x111)) && (psfi.hIcon != IntPtr.Zero)) {
                    icon = new Icon(Icon.FromHandle(psfi.hIcon), 0x10, 0x10);
                    PInvoke.DestroyIcon(psfi.hIcon);
                    return icon;
                }
                return Resources_Image.icoEmpty;
            }
            if(!IsXP && path.StartsWith("::")) {
                IntPtr pszPath = PInvoke.ILCreateFromPath(path);
                if(pszPath != IntPtr.Zero) {
                    if((IntPtr.Zero != PInvoke.SHGetFileInfo(pszPath, 0, ref psfi, Marshal.SizeOf(psfi), 0x109)) && (psfi.hIcon != IntPtr.Zero)) {
                        icon = new Icon(Icon.FromHandle(psfi.hIcon), 0x10, 0x10);
                        PInvoke.DestroyIcon(psfi.hIcon);
                        PInvoke.CoTaskMemFree(pszPath);
                        return icon;
                    }
                    PInvoke.CoTaskMemFree(pszPath);
                }
            }
            else if((IntPtr.Zero != PInvoke.SHGetFileInfo(path, 0, ref psfi, Marshal.SizeOf(psfi), 0x101)) && (psfi.hIcon != IntPtr.Zero)) {
                icon = new Icon(Icon.FromHandle(psfi.hIcon), 0x10, 0x10);
                PInvoke.DestroyIcon(psfi.hIcon);
                return icon;
            }
            return Resources_Image.icoEmpty;
        }

        public static string GetImageKey(string path, string ext) {
            if(!string.IsNullOrEmpty(path)) {
                if(QTUtility2.IsNetworkPath(path)) {
                    if(ext != null) {
                        ext = ext.ToLower();
                        if(ext.Length == 0) {
                            SetImageKey("noext", path);
                            return "noext";
                        }
                        if(!ImageListGlobal.Images.ContainsKey(ext)) {
                            ImageListGlobal.Images.Add(ext, GetIcon(ext, true));
                        }
                        return ext;
                    }
                    if(IsNetworkRootFolder(path)) {
                        SetImageKey(path, path);
                        return path;
                    }
                    SetImageKey("mynetwork", PATH_MYNETWORK);
                    return "mynetwork";
                }
                if(path.StartsWith("::")) {
                    SetImageKey(path, path);
                    return path;
                }
                if(ext != null) {
                    ext = ext.ToLower();
                    if(ext.Length == 0) {
                        SetImageKey("noext", path);
                        return "noext";
                    }
                    if(ExtHasIcon(ext)) {
                        SetImageKey(path, path);
                        return path;
                    }
                    SetImageKey(ext, path);
                    return ext;
                }
                if(path.Contains("*?*?*")) {
                    byte[] buffer;
                    if(ImageListGlobal.Images.ContainsKey(path)) {
                        return path;
                    }
                    if(ITEMIDLIST_Dic_Session.TryGetValue(path, out buffer)) {
                        using(IDLWrapper w = new IDLWrapper(buffer)) {
                            if(w.Available) {
                                ImageListGlobal.Images.Add(path, GetIcon(w.PIDL));
                                return path;
                            }
                        }
                    }
                    return "noimage";
                }
                if(QTUtility2.IsShellPathButNotFileSystem(path)) {
                    IDLWrapper wrapper;
                    if(ImageListGlobal.Images.ContainsKey(path)) {
                        return path;
                    }
                    if(IDLWrapper.TryGetCache(path, out wrapper)) {
                        using(wrapper) {
                            if(wrapper.Available) {
                                ImageListGlobal.Images.Add(path, GetIcon(wrapper.PIDL));
                                return path;
                            }
                        }
                    }
                    return "noimage";
                }
                if(path.StartsWith("ftp://") || path.StartsWith("http://")) {
                    return "folder";
                }
                try {
                    DirectoryInfo info = new DirectoryInfo(path);
                    if(info.Exists) {
                        FileAttributes attributes = info.Attributes;
                        if(((attributes & FileAttributes.System) != 0) || ((attributes & FileAttributes.ReadOnly) != 0)) {
                            SetImageKey(path, path);
                            return path;
                        }
                        return "folder";
                    }
                    if(File.Exists(path)) {
                        ext = Path.GetExtension(path).ToLower();
                        if(ext.Length == 0) {
                            SetImageKey("noext", path);
                            return "noext";
                        }
                        if(ExtHasIcon(ext)) {
                            SetImageKey(path, path);
                            return path;
                        }
                        SetImageKey(ext, path);
                        return ext;
                    }
                    if(path.ToLower().Contains(@".zip\")) {
                        return "folder";
                    }
                }
                catch {
                }
            }
            return "noimage";
        }

        public static DateTime GetLinkerTimestamp() {
            string filePath = System.Reflection.Assembly.GetCallingAssembly().Location;
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;
            byte[] buf = new byte[2048];
            Stream stream = null;

            try {
                stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                stream.Read(buf, 0, 2048);
            }
            finally {
                if(stream != null) {
                    stream.Close();
                }
            }

            int offset = BitConverter.ToInt32(buf, c_PeHeaderOffset);
            int secondsSince1970 = BitConverter.ToInt32(buf, offset + c_LinkerTimestampOffset);
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0);
            dt = dt.AddSeconds(secondsSince1970);
            dt = dt.AddHours(TimeZone.CurrentTimeZone.GetUtcOffset(dt).Hours);
            return dt;
        }

        public static T[] GetSettingValue<T>(T[] inputValues, T[] defaultValues, bool fClone) {
            if((inputValues == null) || (inputValues.Length == 0)) {
                if(!fClone) {
                    return defaultValues;
                }
                return (T[])defaultValues.Clone();
            }
            int length = defaultValues.Length;
            int num2 = inputValues.Length;
            T[] localArray = new T[length];
            for(int i = 0; i < length; i++) {
                if(i < num2) {
                    localArray[i] = inputValues[i];
                }
                else {
                    localArray[i] = defaultValues[i];
                }
            }
            return localArray;
        }

        public static void GetShellClickMode() {
            const string lpSubKey = @"Software\Microsoft\Windows\CurrentVersion\Explorer";
            iIconUnderLineVal = 0;
            int lpcbData = 4;
            try {
                IntPtr ptr;
                if(PInvoke.RegOpenKeyEx((IntPtr)(-2147483647), lpSubKey, 0, 0x20019, out ptr) == 0) {
                    using(SafePtr lpData = new SafePtr(4)) {
                        int num2;
                        if(PInvoke.RegQueryValueEx(ptr, "IconUnderline", IntPtr.Zero, out num2, lpData, ref lpcbData) == 0) {
                            byte[] destination = new byte[4];
                            Marshal.Copy(lpData, destination, 0, 4);
                            iIconUnderLineVal = destination[0];
                        }                        
                    }
                    PInvoke.RegCloseKey(ptr);
                }
                using(RegistryKey key = Registry.CurrentUser.OpenSubKey(lpSubKey, false)) {
                    byte[] buffer2 = (byte[])key.GetValue("ShellState");
                    fSingleClick = false;
                    if((buffer2 != null) && (buffer2.Length > 3)) {
                        byte num3 = buffer2[4];
                        fSingleClick = (num3 & 0x20) == 0;
                    }
                }
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception);
            }
        }

        public static TabBarOption GetTabBarOption() {
            return null; // TODO
        }

        private static bool IsNetworkRootFolder(string path) {
            string str = path.Substring(2);
            int index = str.IndexOf(Path.DirectorySeparatorChar);
            if(index != -1) {
                string str2 = str.Substring(index + 1);
                if(str2.Length > 0) {
                    return (str2.IndexOf(Path.DirectorySeparatorChar) == -1);
                }
            }
            return false;
        }

        public static void LoadReservedImage(ImageReservationKey irk) {
            if(!ImageListGlobal.Images.ContainsKey(irk.ImageKey)) {
                switch(irk.ImageType) {
                    case 0:
                        if(irk.ImageKey != "noimage") {
                            if(irk.ImageKey == "noext") {
                                ImageListGlobal.Images.Add("noext", GetIcon(string.Empty, true));
                                return;
                            }
                            return;
                        }
                        return;

                    case 1:
                        ImageListGlobal.Images.Add(irk.ImageKey, GetIcon(irk.ImageKey, true));
                        return;

                    case 2:
                    case 4:
                        ImageListGlobal.Images.Add(irk.ImageKey, GetIcon(irk.ImageKey, false));
                        return;

                    case 3:
                        return;

                    case 5:
                        byte[] buffer;
                        if(ITEMIDLIST_Dic_Session.TryGetValue(irk.ImageKey, out buffer)) {
                            using(IDLWrapper w = new IDLWrapper(buffer)) {
                                if(!w.Available) return;
                                ImageListGlobal.Images.Add(irk.ImageKey, GetIcon(w.PIDL));
                            }
                        }
                        return;

                    case 6:
                        IDLWrapper wrapper;
                        if(IDLWrapper.TryGetCache(irk.ImageKey, out wrapper)) {
                            using(wrapper) {
                                if(wrapper.Available) {
                                    ImageListGlobal.Images.Add(irk.ImageKey, GetIcon(wrapper.PIDL));
                                }
                            }
                        }
                        return;
                }
            }
        }

        public static MouseChord MakeMouseChord(MouseChord button, Keys modifiers) {
            if((modifiers & Keys.Shift) != 0) button |= MouseChord.Shift;
            if((modifiers & Keys.Control) != 0) button |= MouseChord.Ctrl;
            if((modifiers & Keys.Alt) != 0) button |= MouseChord.Alt;
            return button;
        }

        public static Dictionary<string, string[]> ReadLanguageFile(string path) {
            Dictionary<string, string[]> dictionary = new Dictionary<string, string[]>();
            const string newValue = "\r\n";
            const string oldValue = @"\r\n";
            try {
                using(XmlTextReader reader = new XmlTextReader(path)) {
                    while(reader.Read()) {
                        if((reader.NodeType == XmlNodeType.Element) && (reader.Name != "root")) {
                            string[] strArray = reader.ReadString().Split(new string[] { newValue }, StringSplitOptions.RemoveEmptyEntries);
                            for(int i = 0; i < strArray.Length; i++) {
                                strArray[i] = strArray[i].Replace(oldValue, newValue);
                            }
                            dictionary[reader.Name] = strArray;
                        }
                    }
                }
                return dictionary;
            }
            catch(XmlException exception) {
                MessageBox.Show(string.Concat(new object[] { "Invalid language file.\r\n\r\n\"", exception.SourceUri, "\"\r\nLine: ", exception.LineNumber, "\r\nPosition: ", exception.LinePosition }));
                return null;
            }
            catch(Exception exception2) {
                QTUtility2.MakeErrorLog(exception2);
                return null;
            }
            //return dictionary;
        }

        public static void RefreshGroupMenuesOnReorderFinished(ToolStripItemCollection itemsList) {
            Registry.CurrentUser.CreateSubKey(RegConst.Root).DeleteSubKey("Groups", false);
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(RegConst.Root + @"Groups")) {
                int num = 1;
                foreach(ToolStripItem item in itemsList) {
                    if(item.Text.Length == 0) {
                        key.SetValue("Separator" + num++, string.Empty);
                    }
                    else {
                        key.SetValue(item.Text, GroupPathsDic[item.Text]);
                    }
                }
            }
        }

        public static void RefreshGroupsDic() {
            GroupPathsDic.Clear();
            using(RegistryKey key = Registry.CurrentUser.OpenSubKey(RegConst.Root + @"Groups", false)) {
                if(key != null) {
                    foreach(string str in key.GetValueNames()) {
                        string str2 = (string)key.GetValue(str);
                        if(str2 != null) {
                            GroupPathsDic.Add(str, str2);
                        }
                    }
                }
            }
        }

        private static void RefreshGroupShortcutKeyDic(RegistryKey rkUser) {
            PluginKey[] keyArray = QTUtility2.ReadRegBinary<PluginKey>("ShortcutKeys_Group", rkUser);
            if(keyArray != null) {
                foreach(PluginKey key in keyArray) {
                    int[] keys = key.Keys;
                    if((keys != null) && (keys.Length == 1)) {
                        if((keys[0] & -1048577) != 0) {
                            dicGroupShortcutKeys[keys[0]] = key.PluginID;
                        }
                        dicGroupNamesAndKeys[key.PluginID] = keys[0];
                    }
                }
            }
        }

        public static void RefreshLockedTabsList() {
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(RegConst.Root)) {
                if(key != null) {
                    string[] collection = QTUtility2.ReadRegBinary<string>("TabsLocked", key);
                    if((collection != null) && (collection.Length != 0)) {
                        LockedTabsToRestoreList = new List<string>(collection);
                    }
                    else {
                        LockedTabsToRestoreList.Clear();
                    }
                }
            }
        }

        public static void RefreshUserAppDic(bool fCheckShortcuts) {
            UserAppsDic.Clear();
            using(RegistryKey key = Registry.CurrentUser.OpenSubKey(RegConst.Root + @"UserApps", false)) {
                if(key != null) {
                    foreach(string str in key.GetValueNames()) {
                        if(str.Length > 0) {
                            string[] strArray = QTUtility2.ReadRegBinary<string>(str, key);
                            if((strArray != null) && ((strArray.Length == 3) || (strArray.Length == 4))) {
                                int num;
                                UserAppsDic.Add(str, strArray);
                                if((strArray.Length == 4) && int.TryParse(strArray[3], out num)) {
                                    dicUserAppShortcutKeys[num] = new MenuItemArguments(strArray[0], strArray[1], strArray[2], num, MenuGenre.Application);
                                }
                            }
                            else {
                                using(RegistryKey key2 = key.OpenSubKey(str, false)) {
                                    if(key2 != null) {
                                        UserAppsDic.Add(str, null);
                                        if(fCheckShortcuts) {
                                            RefreshUserAppShortcutKeyDic(key2);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void RefreshUserappMenuesOnReorderFinished(ToolStripItemCollection itemsList) {
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(RegConst.Root + @"UserApps")) {
                foreach(string str in key.GetValueNames()) {
                    key.DeleteValue(str, false);
                }
                int num = 1;
                string[] array = new string[] { "separator", string.Empty, string.Empty };
                foreach(ToolStripItem item in itemsList) {
                    string[] strArray2;
                    if(item.Text.Length == 0) {
                        QTUtility2.WriteRegBinary(array, "Separator" + num++, key);
                        continue;
                    }
                    if(UserAppsDic.TryGetValue(item.Name, out strArray2)) {
                        if((strArray2 == null) || (strArray2.Length == 0)) {
                            key.SetValue(item.Name, new byte[0]);
                            continue;
                        }
                        QTUtility2.WriteRegBinary(strArray2, item.Name, key);
                    }
                }
            }
        }

        private static void RefreshUserAppShortcutKeyDic(RegistryKey rk) {
            if(rk != null) {
                foreach(string str in rk.GetValueNames()) {
                    if(str.Length > 0) {
                        string[] strArray = QTUtility2.ReadRegBinary<string>(str, rk);
                        if((strArray != null) && (strArray.Length == 4)) {
                            int num;
                            if(int.TryParse(strArray[3], out num)) {
                                dicUserAppShortcutKeys[num] = new MenuItemArguments(strArray[0], strArray[1], strArray[2], num, MenuGenre.Application);
                            }
                        }
                        else {
                            using(RegistryKey key = rk.OpenSubKey(str, false)) {
                                if(key != null) {
                                    RefreshUserAppShortcutKeyDic(key);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void RegisterPrimaryInstance(IntPtr hwndExplr, QTTabBarClass tabBar) {
            InstanceManager.PushInstance(hwndExplr, tabBar);
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(RegConst.Root)) {
                QTUtility2.WriteRegHandle("Handle", key, tabBar.Handle);
            }
        }

        public static ImageReservationKey ReserveImageKey(QMenuItem qmi, string path, string ext) {
            ImageReservationKey key = null;
            if(string.IsNullOrEmpty(path)) {
                return new ImageReservationKey("noimage", 0);
            }
            if(!string.IsNullOrEmpty(ext)) {
                ext = ext.ToLower();
                if(ExtHasIcon(ext) && !QTUtility2.IsNetworkPath(path)) {
                    return new ImageReservationKey(path, 2);
                }
                return new ImageReservationKey(ext, 1);
            }
            if(QTUtility2.IsNetworkPath(path)) {
                if(IsNetworkRootFolder(path)) {
                    return new ImageReservationKey(path, 4);
                }
                return new ImageReservationKey("folder", 3);
            }
            if(path.StartsWith("::")) {
                return new ImageReservationKey(path, 4);
            }
            if(path.Contains("*?*?*")) {
                return new ImageReservationKey(path, 5);
            }
            if(QTUtility2.IsShellPathButNotFileSystem(path)) {
                return new ImageReservationKey(path, 6);
            }
            if(path.StartsWith("ftp://") || path.StartsWith("http://")) {
                return new ImageReservationKey("folder", 3);
            }
            try {
                if(qmi.Exists) {
                    if(qmi.Target == MenuTarget.Folder) {
                        if(qmi.HasIcon) {
                            return new ImageReservationKey(path, 4);
                        }
                        return new ImageReservationKey("folder", 3);
                    }
                    if(qmi.Target == MenuTarget.File) {
                        ext = Path.GetExtension(path).ToLower();
                        if(ext.Length == 0) {
                            return new ImageReservationKey("noext", 0);
                        }
                        if(ExtHasIcon(ext)) {
                            return new ImageReservationKey(path, 2);
                        }
                        return new ImageReservationKey(ext, 1);
                    }
                }
                DirectoryInfo info = new DirectoryInfo(path);
                if(info.Exists) {
                    FileAttributes attributes = info.Attributes;
                    if(((attributes & FileAttributes.System) != 0) || ((attributes & FileAttributes.ReadOnly) != 0)) {
                        return new ImageReservationKey(path, 4);
                    }
                    return new ImageReservationKey("folder", 3);
                }
                if(!File.Exists(path)) {
                    return new ImageReservationKey("noimage", 0);
                }
                ext = Path.GetExtension(path).ToLower();
                if(ext.Length == 0) {
                    return new ImageReservationKey("noext", 0);
                }
                if(ExtHasIcon(ext)) {
                    return new ImageReservationKey(path, 2);
                }
                key = new ImageReservationKey(ext, 1);
            }
            catch {
            }
            return key;
        }

        public static void SaveClosing(List<string> closingPaths) {
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(RegConst.Root)) {
                if(key != null) {
                    key.SetValue("TabsOnLastClosedWindow", closingPaths.StringJoin(";"));
                }
            }
        }

        public static void SaveGroupsReg() {
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(RegConst.Root + @"Groups")) {
                if(key != null) {
                    foreach(string str in key.GetValueNames()) {
                        key.DeleteValue(str, false);
                    }
                    foreach(string str2 in GroupPathsDic.Keys) {
                        key.SetValue(str2, GroupPathsDic[str2]);
                    }
                }
            }
        }

        public static void SaveRecentFiles(RegistryKey rkUser) {
            if(rkUser != null) {
                using(RegistryKey key = rkUser.CreateSubKey("RecentFiles")) {
                    if(key != null) {
                        foreach(string str in key.GetValueNames()) {
                            key.DeleteValue(str, false);
                        }
                        for(int i = 0; i < ExecutedPathsList.Count; i++) {
                            key.SetValue(i.ToString(), ExecutedPathsList[i]);
                        }
                    }
                }
            }
        }

        public static void SaveRecentlyClosed(RegistryKey rkUser) {
            if(rkUser != null) {
                using(RegistryKey key = rkUser.CreateSubKey("RecentlyClosed")) {
                    if(key != null) {
                        foreach(string str in key.GetValueNames()) {
                            key.DeleteValue(str, false);
                        }
                        for(int i = 0; i < ClosedTabHistoryList.Count; i++) {
                            key.SetValue(i.ToString(), ClosedTabHistoryList[i]);
                        }
                    }
                }
            }
        }
        
        private static void SetImageKey(string key, string itemPath) {
            if(!ImageListGlobal.Images.ContainsKey(key)) {
                ImageListGlobal.Images.Add(key, GetIcon(itemPath, false));
            }
        }

        public static void SetTabBarOption(TabBarOption tabBarOption, QTTabBarClass tabBar) {
            // TODO
        }

        public static void ValidateMaxMin(ref int value, int max, int min) {
            value = ValidateMaxMin(value, min, max);
        }

        public static int ValidateMaxMin(int value, int max, int min) {
            int num = Math.Max(max, min);
            int num2 = Math.Min(max, min);
            if(value < num2) {
                value = num2;
                return value;
            }
            if(value > num) {
                value = num;
            }
            return value;
        }

        public static void ValidateTextResources() {
            string[] strArray = new string[] { 
        "ButtonBar_BtnName", "ButtonBar_Misc", "ButtonBar_Option", "DialogButtons", "DragDropToolTip", "Misc_Strings", "TabBar_Menu", "TabBar_Message", "TabBar_NewGroup", "TabBar_Option", "TabBar_Option2", "TabBar_Option_DropDown", "TabBar_Option_Genre", "TabBar_Option_Buttons", "TaskBar_Menu", "TaskBar_Titles", 
        "ShortcutKeys_ActionNames", "ShortcutKeys_MsgReassign", "ShortcutKeys_Groups", "UpdateCheck"
       };
            if(TextResourcesDic == null) {
                TextResourcesDic = new Dictionary<string, string[]>();
            }
            foreach(string str in strArray) {
                string[] strArray2 = Resources_String.ResourceManager.GetString(str).Split(SEPARATOR_CHAR);
                string[] strArray3;
                TextResourcesDic.TryGetValue(str, out strArray3);
                if(strArray3 == null) {
                    TextResourcesDic[str] = strArray2;
                }
                else if(strArray3.Length < strArray2.Length) {
                    List<string> list = new List<string>(strArray3);
                    for(int i = strArray3.Length; i < strArray2.Length; i++) {
                        list.Add(strArray2[i]);
                    }
                    TextResourcesDic[str] = list.ToArray();
                }
            }
            ResMain = TextResourcesDic["TabBar_Menu"];
            ResMisc = TextResourcesDic["Misc_Strings"];
        }
    }
}
