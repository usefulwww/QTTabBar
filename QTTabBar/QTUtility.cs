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
    using QTPlugin;
    using QTTabBarLib.Interop;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Xml;

    public enum Settings : uint {
        NeverCloseWndLocked = 8 * 00 + 0, // 0x01,
        NeverCloseWindow    = 8 * 00 + 1, // 0x02,
        FixedWidthTabs      = 8 * 00 + 2, // 0x04,
        ShowTooltips        = 8 * 00 + 3, // 0x08,
        RestoreClosed       = 8 * 00 + 4, // 0x10,
        CloseWhenGroup      = 8 * 00 + 5, // 0x20,
        DontOpenSame        = 8 * 00 + 6, // 0x40,
        ActivateNewTab      = 8 * 00 + 7, // 0x80,
        CaptureMiddleClick  = 8 * 05 + 0, // 0x01,
        NoHistory           = 8 * 05 + 1, // 0x02,
        LimitedWidthTabs    = 8 * 05 + 2, // 0x04,
        UseTabSkin          = 8 * 05 + 3, // 0x08,
        ActiveTabInBold     = 8 * 05 + 4, // 0x10,
        CaptureX1X2         = 8 * 05 + 5, // 0x20,
        NavButtonsOnRight   = 8 * 05 + 6, // 0x40,
        ShowNavButtons      = 8 * 05 + 7, // 0x80,
        ShowHashResult      = 8 * 06 + 0, // 0x01,
        NoWindowResizing    = 8 * 06 + 1, // 0x02,
        NoNewWndFolderTree  = 8 * 06 + 2, // 0x04,
        RestoreLockedTabs   = 8 * 06 + 3, // 0x08,
        DontCaptureNewWnds  = 8 * 06 + 4, // 0x10,
        NoRecentFiles       = 8 * 06 + 5, // 0x20,
        NoDblClickUpLevel   = 8 * 06 + 6, // 0x40,
        MidClickNewWindow   = 8 * 06 + 7, // 0x80,
        BackspaceUpLevel    = 8 * 07 + 0, // 0x01,
        FolderIcon          = 8 * 07 + 1, // 0x02,
        ToolbarBGColor      = 8 * 07 + 2, // 0x04,
        HideMenuBar         = 8 * 07 + 3, // 0x08,
        MultipleRow2        = 8 * 07 + 4, // 0x10,
        MultipleRow1        = 8 * 07 + 5, // 0x20,
        /* [unused]         = 8 * 07 + 6, // 0x40 */
        HashTopMost         = 8 * 07 + 7, // 0x80 */
        PreviewsWithShift   = 8 * 08 + 0, // 0x01,
        ShowTooltipPreviews = 8 * 08 + 1, // 0x02,
        SaveTransparency    = 8 * 08 + 2, // 0x04,
        AlternateRowColors  = 8 * 08 + 3, // 0x08,
        HashFullPath        = 8 * 08 + 4, // 0x10, */
        ExtWhileRenaming    = 8 * 08 + 5, // 0x20,
        NoFullRowSelect     = 8 * 08 + 6, // 0x40,
        DetailsGridLines    = 8 * 08 + 7, // 0x80,
        /* [unused]         = 8 * 09 + 0, // 0x01 */
        HashClearOnClose    = 8 * 09 + 1, // 0x02 */
        SubDirTipsPreview   = 8 * 09 + 2, // 0x04,
        SubDirTipsFiles     = 8 * 09 + 3, // 0x08,
        SubDirTipsHidden    = 8 * 09 + 4, // 0x10,
        SubDirTipsWithShift = 8 * 09 + 5, // 0x20,
        NoShowSubDirTips    = 8 * 09 + 6, // 0x40,
        /* [unused]         = 8 * 09 + 7, // 0x80 */
        F2Selection         = 8 * 10 + 0, // 0x01,
        TrayOnClose         = 8 * 10 + 1, // 0x02,
        SubDirTipsSystem    = 8 * 10 + 2, // 0x04,
        CtrlWheelChangeView = 8 * 10 + 3, // 0x08,
        HorizontalScroll    = 8 * 10 + 4, // 0x10,
        NoRenameAmbTabs     = 8 * 10 + 5, // 0x20,
        NoTabsFromOutside   = 8 * 10 + 6, // 0x40,
        DragDropOntoTabs    = 8 * 10 + 7, // 0x80,
        TabCloseBtnsOnHover = 8 * 11 + 0, // 0x01,
        CursorLoop          = 8 * 11 + 1, // 0x02,
        TabCloseBtnsWithAlt = 8 * 11 + 2, // 0x04,
        ShowSubDirTipOnTab  = 8 * 11 + 3, // 0x08,
        ShowTabCloseButtons = 8 * 11 + 4, // 0x10,
        RebarImageActual    = 8 * 11 + 5, // 0x20,
        RebarImageTile      = 8 * 11 + 6, // 0x40,
        RebarImage          = 8 * 11 + 7, // 0x80,
        RebarImageStretch2  = 8 * 13 + 0, // 0x01,
        TabTitleShadows     = 8 * 13 + 1, // 0x02,
        TabSwitcher         = 8 * 13 + 2, // 0x04,
        MidClickFolderTree  = 8 * 13 + 3, // 0x08, 
        XPStyleMenus        = 8 * 13 + 4, // 0x10,
        NonDefaultMenu      = 8 * 13 + 5, // 0x20,
        AlignTabTextCenter  = 8 * 13 + 6, // 0x40,
        TrayOnMinimize      = 8 * 13 + 7, // 0x80,
        /* [unused]         = 8 * 14 + 0, // 0x01 */
        /* [unused]         = 8 * 14 + 1, // 0x02 */
        AllRecentFiles      = 8 * 14 + 2, // 0x04 */
        PreviewInfo         = 8 * 14 + 3, // 0x08,
        DisableSound        = 8 * 14 + 4, // 0x10,
        ShowDriveLetters    = 8 * 14 + 5, // 0x20,
        KeepOnSeparate      = 8 * 14 + 6, // 0x40,
        AutoUpdate          = 8 * 14 + 7, // 0x80,
    }

    internal static class QTUtility {
        internal static string Action_BarDblClick;
        internal static Version BetaRevision = new Version(0, 1);
        internal static PathList ClosedTabHistoryList = new PathList(0x10);
        internal static byte[] ConfigValues;
        internal static string CreateWindowTMPGroup = string.Empty;
        internal static string CreateWindowTMPPath = string.Empty;
        internal static Version CurrentVersion = new Version(1, 5, 0, 0);
        internal static int DefaultRebarCOLORREF = -1;
        internal static Dictionary<string, int> dicGroupNamesAndKeys = new Dictionary<string, int>();
        internal static Dictionary<int, string> dicGroupShortcutKeys = new Dictionary<int, string>();
        internal static Dictionary<string, int[]> dicPluginShortcutKeys = new Dictionary<string, int[]>();
        internal static Dictionary<int, MenuItemArguments> dicUserAppShortcutKeys = new Dictionary<int, MenuItemArguments>();
        internal static Dictionary<string, string> DisplayNameCacheDic = new Dictionary<string, string>();
        internal static PathList ExecutedPathsList = new PathList(0x10);
        internal static bool fExplorerPrevented;
        internal const int FLAG_KEYENABLED = 0x100000;
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
        internal static InstanceManager instanceManager = new InstanceManager();
        internal static int InstancesCount;
        internal static bool IsRTL;
        internal static bool IsVista;
        internal static Dictionary<string, byte[]> ITEMIDLIST_Dic_Session = new Dictionary<string, byte[]>();
        internal static readonly int[] KEYS_DEFAULT = new int[] { 
      0, 0, 0x160025, 0x160027, 0x120009, 0x130009, 0, 0, 0x120057, 0x130057, 0, 0, 0, 0x13005a, 0x12004e, 0x13004e, 
      0x12004c, 0x13004c, 0x12004f, 0, 0x14004f, 0x1400bc, 0x1400be, 0x140047, 0x140048, 0x140055, 0x14004d, 0, 0, 0, 0, 0, 
      0, 0, 0, 0, 0, 0, 0, 0, 0
     };
        internal static List<string> LockedTabsToRestoreList = new List<string>();
        public static int MaxCount_Executed = 0x10;
        internal static int MaxCount_History = 0x10;
        internal static int MaxTabWidth;
        internal static int MinTabWidth;
        internal static List<string> NoCapturePathsList = new List<string>();
        internal static bool NowDebugging = 
#if DEBUG
            true;
#else
            false;
#endif
        internal static int OptionsDialogTabIndex;
        internal static string Path_LanguageFile;
        internal static string PATH_MYNETWORK;
        internal static string Path_PluginLangFile;
        internal static string Path_RebarImage;
        internal static string PATH_SEARCHFOLDER;
        internal static string Path_TabImage;
        internal static string PathToSelectInCommandLineArg;
        internal static int[] PluginShortcutKeysCache = new int[1];
        internal static List<string> PreviewExtsList_Img = new List<string>();
        internal static List<string> PreviewExtsList_Txt = new List<string>();
        internal static string PreviewFontName;
        internal static float PreviewFontSize;
        internal static int PreviewMaxHeight = 0x100;
        internal static int PreviewMaxWidth = 0x200;
        internal static System.Drawing.Color RebarBGColor;
        internal const string REGUSER = @"Software\Quizo\QTTabBar";
        internal static string[] ResMain;
        internal static string[] ResMisc;
        internal static bool RestoreFolderTree_Hide;
        internal static SolidBrush sbAlternate;
        internal static readonly char[] SEPARATOR_CHAR = new char[] { ';' };
        internal const string SEPARATOR_PATH_HASH_SESSION = "*?*?*";
        internal static int ShellViewRowCOLORREF_Background;
        internal static int ShellViewRowCOLORREF_Text;
        internal static int[] ShortcutKeys;
        internal static List<string> StartUpGroupList = new List<string>();
        internal static string StartUpGroupNameNowOpening = string.Empty;
        internal static Font StartUpTabFont;
        internal static Font TabFont;
        internal static int TabHeight;
        internal static System.Drawing.Color TabHiliteColor;
        internal static Padding TabImageSizingMargin;
        internal static System.Drawing.Color TabTextColor_Active;
        internal static System.Drawing.Color TabTextColor_ActivShdw;
        internal static System.Drawing.Color TabTextColor_Inactv;
        internal static System.Drawing.Color TabTextColor_InAtvShdw;
        internal static int TabWidth;
        internal static Dictionary<string, string[]> TextResourcesDic;
        internal static List<byte[]> TMPIDLList = new List<byte[]>();
        internal static List<string> TMPPathList = new List<string>();
        internal static byte[] TMPTargetIDL;
        internal static Dictionary<string, string[]> UserAppsDic = new Dictionary<string, string[]>();
        internal static byte WindowAlpha = 0xff;

        static QTUtility() {
            if(Environment.CommandLine.IndexOf("RegAsm.exe\"", StringComparison.OrdinalIgnoreCase) == -1) {
                ImageListGlobal = new ImageList();
                ImageListGlobal.ColorDepth = ColorDepth.Depth32Bit;
                ImageListGlobal.Images.Add("folder", GetIcon(string.Empty, false));
                try {
                    IsVista = Environment.OSVersion.Version.Major > 5;
                    // TODO: make this more comprehensible 
                    ConfigValues = new byte[] { 200, 0, 4, 0, 4, 0x60, 0x10, 0x22, 2, 8, 0xe0, 8, 0, 0x20, 0, 0 };
                    if(!IsVista) {
                        ConfigValues[13] = 0x30;
                    }
                    SetConfigAt(Settings.AutoUpdate, true);
                    using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar")) {
                        if(key != null) {
                            float num;
                            byte[] inputValues = (byte[])key.GetValue("Config");
                            ConfigValues = GetSettingValue<byte>(inputValues, ConfigValues, false);
                            string path = (string)key.GetValue("LanguageFile", string.Empty);
                            if((path.Length > 0) && File.Exists(path)) {
                                Path_LanguageFile = path;
                                TextResourcesDic = ReadLanguageFile(path);
                            }
                            else {
                                Path_LanguageFile = string.Empty;
                            }
                            ValidateTextResources();
                            TabWidth = QTUtility2.GetRegistryValueSafe<int>(key, "TabWidth", 80);
                            TabHeight = QTUtility2.GetRegistryValueSafe<int>(key, "TabHeight", 0x18);
                            MaxTabWidth = QTUtility2.GetRegistryValueSafe<int>(key, "TabWidthMax", 150);
                            MinTabWidth = QTUtility2.GetRegistryValueSafe<int>(key, "TabWidthMin", 70);
                            if(TabHeight > 50) {
                                TabHeight = 50;
                            }
                            if(TabHeight < 10) {
                                TabHeight = 10;
                            }
                            TabTextColor_Active = System.Drawing.Color.FromArgb(QTUtility2.GetRegistryValueSafe<int>(key, "TitleColorActive", SystemColors.ControlText.ToArgb()));
                            TabTextColor_Inactv = System.Drawing.Color.FromArgb(QTUtility2.GetRegistryValueSafe<int>(key, "TitleColorInactive", SystemColors.ControlText.ToArgb()));
                            TabHiliteColor = System.Drawing.Color.FromArgb(QTUtility2.GetRegistryValueSafe<int>(key, "HighlightColorClassic", SystemColors.Highlight.ToArgb()));
                            TabTextColor_ActivShdw = System.Drawing.Color.FromArgb(QTUtility2.GetRegistryValueSafe<int>(key, "TitleColorShadowActive", System.Drawing.Color.Silver.ToArgb()));
                            TabTextColor_InAtvShdw = System.Drawing.Color.FromArgb(QTUtility2.GetRegistryValueSafe<int>(key, "TitleColorShadowInActv", System.Drawing.Color.White.ToArgb()));
                            RebarBGColor = System.Drawing.Color.FromArgb(QTUtility2.GetRegistryValueSafe<int>(key, "ToolbarBGColor", SystemColors.Control.ToArgb()));
                            ShellViewRowCOLORREF_Background = QTUtility2.GetRegistryValueSafe<int>(key, "AlternateColor_Bk", 0xfaf5f1);
                            ShellViewRowCOLORREF_Text = QTUtility2.GetRegistryValueSafe<int>(key, "AlternateColor_Text", QTUtility2.MakeCOLORREF(SystemColors.WindowText));
                            string familyName = (string)key.GetValue("TabFont", string.Empty);
                            string s = (string)key.GetValue("TabFontSize", "0");
                            if(float.TryParse(s, out num) && (num != 0f)) {
                                TabFont = new Font(familyName, num);
                            }
                            Action_BarDblClick = (string)key.GetValue("Action_BarDblClick", string.Empty);
                            MaxCount_History = QTUtility2.GetRegistryValueSafe<int>(key, "Max_Undo", 0x10);
                            using(RegistryKey key2 = key.CreateSubKey("RecentlyClosed")) {
                                if(key2 != null) {
                                    List<string> collection = new List<string>();
                                    foreach(string str4 in key2.GetValueNames()) {
                                        collection.Add((string)key2.GetValue(str4));
                                    }
                                    ClosedTabHistoryList = new PathList(collection, MaxCount_History);
                                }
                            }
                            if(CheckConfig(Settings.AllRecentFiles)) {
                                MaxCount_Executed = QTUtility2.GetRegistryValueSafe<int>(key, "Max_RecentFile", 0x10);
                                using(RegistryKey key3 = key.CreateSubKey("RecentFiles")) {
                                    if(key3 != null) {
                                        List<string> list2 = new List<string>();
                                        foreach(string str5 in key3.GetValueNames()) {
                                            list2.Add((string)key3.GetValue(str5));
                                        }
                                        ExecutedPathsList = new PathList(list2, MaxCount_Executed);
                                    }
                                }
                            }
                            RefreshShortcutKeys(key);
                            RefreshGroupShortcutKeyDic(key);
                            Path_TabImage = (string)key.GetValue("TabImage", string.Empty);
                            byte[] buffer2 = (byte[])key.GetValue("TabImageSizingMargin", new byte[4]);
                            if(buffer2.Length != 4) {
                                TabImageSizingMargin = Padding.Empty;
                            }
                            else {
                                TabImageSizingMargin = new Padding(buffer2[0], buffer2[1], buffer2[2], buffer2[3]);
                            }
                            if((Path_TabImage.Length > 0) && !File.Exists(Path_TabImage)) {
                                Path_TabImage = string.Empty;
                            }
                            Path_RebarImage = (string)key.GetValue("ToolbarBGImage", string.Empty);
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
                            RefreshPreviewExtensions();
                            PreviewMaxWidth = ValidateMaxMin((int)key.GetValue("PreviewMaxWidth", 0x200), 0x780, 0x80);
                            PreviewMaxHeight = ValidateMaxMin((int)key.GetValue("PreviewMaxHeight", 0x100), 0x4b0, 0x60);
                            PreviewFontName = (string)key.GetValue("PreviewFont", null);
                            string str8 = (string)key.GetValue("PreviewFontSize", null);
                            if(!float.TryParse(str8, out PreviewFontSize)) {
                                PreviewFontSize = 10.5f;
                            }
                        }
                    }
                    RefreshGroupsDic();
                    RefreshUserAppDic(true);
                    if(IsVista) {
                        PATH_SEARCHFOLDER = "::{9343812E-1C37-4A49-A12E-4B2D810D956B}";
                        PATH_MYNETWORK = "::{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}";
                    }
                    else {
                        PATH_SEARCHFOLDER = "::{E17D4FC0-5564-11D1-83F2-00A0C90DC849}";
                        PATH_MYNETWORK = "::{208D2C60-3AEA-1069-A2D7-08002B30309D}";
                        GetShellClickMode();
                    }
                    PluginManager.Initialize();
                    IsRTL = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft;
                }
                catch(Exception exception) {
                    QTUtility2.MakeErrorLog(exception, null);
                }
            }
        }

        public static bool CheckConfig(Settings setting) {
            int index = (int)setting >> 3;
            byte value = (byte)(1 << ((int)setting & 7));
            return ((ConfigValues[index] & value) == value);
        }

        public static bool ExtHasIcon(string ext) {
            if((!(ext == ".exe") && !(ext == ".lnk")) && (!(ext == ".ico") && !(ext == ".url"))) {
                return (ext == ".sln");
            }
            return true;
        }

        public static bool ExtIsCompressed(string ext) {
            if(!(ext == ".zip") && !(ext == ".lzh")) {
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
            if(IsVista && path.StartsWith("::")) {
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
                if(path.StartsWith(@"\\")) {
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
                        IntPtr pIDL = ShellMethods.CreateIDL(buffer);
                        if(pIDL != IntPtr.Zero) {
                            ImageListGlobal.Images.Add(path, GetIcon(pIDL));
                            PInvoke.CoTaskMemFree(pIDL);
                            return path;
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
            string lpSubKey = @"Software\Microsoft\Windows\CurrentVersion\Explorer";
            iIconUnderLineVal = 0;
            int lpcbData = 4;
            try {
                IntPtr ptr;
                if(PInvoke.RegOpenKeyEx((IntPtr)(-2147483647), lpSubKey, 0, 0x20019, out ptr) == 0) {
                    int num2;
                    IntPtr lpData = Marshal.AllocHGlobal(4);
                    if(PInvoke.RegQueryValueEx(ptr, "IconUnderline", IntPtr.Zero, out num2, lpData, ref lpcbData) == 0) {
                        byte[] destination = new byte[4];
                        Marshal.Copy(lpData, destination, 0, 4);
                        iIconUnderLineVal = destination[0];
                    }
                    Marshal.FreeHGlobal(lpData);
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
                QTUtility2.MakeErrorLog(exception, null);
            }
        }

        public static TabBarOption GetTabBarOption() {
            Dictionary<int, object> dictionary = new Dictionary<int, object>();
            dictionary[0] = CheckConfig(Settings.ActivateNewTab);
            dictionary[1] = CheckConfig(Settings.DontOpenSame);
            dictionary[2] = CheckConfig(Settings.CloseWhenGroup);
            dictionary[3] = CheckConfig(Settings.ShowTooltips);
            dictionary[4] = CheckConfig(Settings.CaptureX1X2);
            dictionary[5] = CheckConfig(Settings.ShowNavButtons);
            dictionary[6] = !CheckConfig(Settings.NoHistory);
            dictionary[7] = !CheckConfig(Settings.NoRecentFiles);
            dictionary[8] = CheckConfig(Settings.DragDropOntoTabs);
            dictionary[9] = !CheckConfig(Settings.NoRenameAmbTabs);
            dictionary[10] = CheckConfig(Settings.MidClickNewWindow);
            dictionary[11] = !CheckConfig(Settings.NoDblClickUpLevel);
            dictionary[12] = CheckConfig(Settings.NoWindowResizing);
            dictionary[13] = CheckConfig(Settings.SaveTransparency);
            dictionary[14] = !CheckConfig(Settings.DontCaptureNewWnds);
            dictionary[15] = !CheckConfig(Settings.NoNewWndFolderTree);
            dictionary[0x10] = CheckConfig(Settings.NoTabsFromOutside);
            dictionary[0x11] = !CheckConfig(Settings.HorizontalScroll);
            dictionary[0x12] = !CheckConfig(Settings.CtrlWheelChangeView);
            dictionary[0x13] = CheckConfig(Settings.NeverCloseWindow);
            dictionary[20] = CheckConfig(Settings.NeverCloseWndLocked);
            dictionary[0x15] = CheckConfig(Settings.RestoreClosed);
            dictionary[0x16] = CheckConfig(Settings.RestoreLockedTabs);
            dictionary[0x17] = CheckConfig(Settings.UseTabSkin);
            dictionary[0x18] = CheckConfig(Settings.FolderIcon);
            dictionary[0x19] = CheckConfig(Settings.ActiveTabInBold);
            dictionary[0x1a] = CheckConfig(Settings.ToolbarBGColor);
            dictionary[0x1b] = CheckConfig(Settings.HideMenuBar);
            dictionary[0x1c] = CheckConfig(Settings.BackspaceUpLevel);
            dictionary[0x1d] = CheckConfig(Settings.NoFullRowSelect) ^ IsVista;
            dictionary[30] = CheckConfig(Settings.DetailsGridLines);
            dictionary[0x1f] = CheckConfig(Settings.AlternateRowColors);
            dictionary[0x20] = CheckConfig(Settings.ShowTooltipPreviews);
            dictionary[0x21] = CheckConfig(Settings.PreviewsWithShift);
            dictionary[0x22] = !CheckConfig(Settings.NoShowSubDirTips);
            dictionary[0x23] = CheckConfig(Settings.SubDirTipsWithShift);
            dictionary[0x24] = !CheckConfig(Settings.SubDirTipsPreview);
            dictionary[0x25] = CheckConfig(Settings.SubDirTipsHidden);
            dictionary[0x26] = CheckConfig(Settings.SubDirTipsSystem);
            dictionary[0x27] = CheckConfig(Settings.SubDirTipsFiles);
            dictionary[40] = !CheckConfig(Settings.ExtWhileRenaming);
            dictionary[0x29] = CheckConfig(Settings.TrayOnClose);
            dictionary[0x10000] = CheckConfig(Settings.NavButtonsOnRight) ? 1 : 0;
            dictionary[0x10001] = MaxCount_History;
            dictionary[0x10002] = ConfigValues[1];
            dictionary[0x10003] = ConfigValues[2];
            dictionary[0x10004] = ConfigValues[3];
            dictionary[0x10005] = ConfigValues[4];
            dictionary[0x10006] = CheckConfig(Settings.MultipleRow1) ? 1 : (CheckConfig(Settings.MultipleRow2) ? 2 : 0);
            dictionary[0x10007] = CheckConfig(Settings.CaptureMiddleClick) ? 1 : 0;
            dictionary[0x10008] = TabHeight;
            dictionary[0x10009] = CheckConfig(Settings.FixedWidthTabs) ? 1 : (CheckConfig(Settings.LimitedWidthTabs) ? 2 : 0);
            dictionary[0x1000a] = TabWidth;
            dictionary[0x1000b] = MaxTabWidth;
            dictionary[0x1000c] = MinTabWidth;
            dictionary[0x1000d] = PreviewMaxWidth;
            dictionary[0x1000e] = PreviewMaxHeight;
            dictionary[0x20000] = Path_LanguageFile;
            dictionary[0x20001] = Action_BarDblClick;
            dictionary[0x20002] = Path_TabImage;
            dictionary[0x20003] = (TabFont == null) ? Control.DefaultFont.Name : TabFont.Name;
            dictionary[0x20004] = Path_PluginLangFile;
            dictionary[0x20005] = PreviewFontName;
            dictionary[0x40000] = TabTextColor_Active;
            dictionary[0x40001] = TabTextColor_Inactv;
            dictionary[0x40002] = RebarBGColor;
            dictionary[0x40003] = QTUtility2.MakeColor(ShellViewRowCOLORREF_Text);
            dictionary[0x40004] = QTUtility2.MakeColor(ShellViewRowCOLORREF_Background);
            dictionary[0x40005] = TabHiliteColor;
            dictionary[0x80000] = TabImageSizingMargin;
            dictionary[0x80001] = (TabFont == null) ? Control.DefaultFont.Size : TabFont.Size;
            dictionary[0x80002] = PreviewExtsList_Txt.ToArray();
            dictionary[0x80003] = PreviewExtsList_Img.ToArray();
            dictionary[0x80004] = PreviewFontSize;
            return new TabBarOption(dictionary);
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
                        if(!(irk.ImageKey == "noimage")) {
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
                            IntPtr pIDL = ShellMethods.CreateIDL(buffer);
                            if(!(pIDL != IntPtr.Zero)) {
                                return;
                            }
                            ImageListGlobal.Images.Add(irk.ImageKey, GetIcon(pIDL));
                            PInvoke.CoTaskMemFree(pIDL);
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

        public static Dictionary<string, string[]> ReadLanguageFile(string path) {
            Dictionary<string, string[]> dictionary = new Dictionary<string, string[]>();
            string newValue = "\r\n";
            string oldValue = @"\r\n";
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
                QTUtility2.MakeErrorLog(exception2, null);
                return null;
            }
            //return dictionary;
        }

        public static void RefreshGroupMenuesOnReorderFinished(ToolStripItemCollection itemsList) {
            Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar").DeleteSubKey("Groups", false);
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar\Groups")) {
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
            using(RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Quizo\QTTabBar\Groups", false)) {
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
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar")) {
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

        public static void RefreshPreviewExtensions() {
            string defaultValue = ".txt;.ini;.inf;.cs;.log;.js;.vbs";
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar")) {
                string str2 = (string)key.GetValue("TextExtensions", defaultValue);
                if(str2.Length > 0) {
                    foreach(string str3 in str2.Split(SEPARATOR_CHAR)) {
                        if(str3.Length > 0) {
                            string item = str3;
                            if(!str3.StartsWith(".")) {
                                item = "." + str3;
                            }
                            PreviewExtsList_Txt.Add(item);
                        }
                    }
                }
                string str5 = (string)key.GetValue("ImageExtensions");
                if(str5 == null) {
                    PreviewExtsList_Img = ThumbnailTooltipForm.MakeDefaultImgExts();
                }
                else if(str5.Length > 0) {
                    foreach(string str6 in str5.Split(SEPARATOR_CHAR)) {
                        if(str6.Length > 0) {
                            string str7 = str6;
                            if(!str6.StartsWith(".")) {
                                str7 = "." + str6;
                            }
                            PreviewExtsList_Img.Add(str7);
                        }
                    }
                }
            }
        }

        public static void RefreshShortcutKeys(RegistryKey rkUser) {
            ShortcutKeys = GetSettingValue<int>(QTUtility2.ReadRegBinary<int>("ShortcutKeys", rkUser), KEYS_DEFAULT, true);
        }

        public static void RefreshUserAppDic(bool fCheckShortcuts) {
            UserAppsDic.Clear();
            using(RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Quizo\QTTabBar\UserApps", false)) {
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
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar\UserApps")) {
                foreach(string str in key.GetValueNames()) {
                    key.DeleteValue(str, false);
                }
                int num = 1;
                string[] array = new string[] { "separator", string.Empty, string.Empty };
                foreach(ToolStripItem item in itemsList) {
                    string[] strArray2 = null;
                    if(item.Text.Length == 0) {
                        QTUtility2.WriteRegBinary<string>(array, "Separator" + num++, key);
                        continue;
                    }
                    if(UserAppsDic.TryGetValue(item.Name, out strArray2)) {
                        if((strArray2 == null) || (strArray2.Length == 0)) {
                            key.SetValue(item.Name, new byte[0]);
                            continue;
                        }
                        QTUtility2.WriteRegBinary<string>(strArray2, item.Name, key);
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
            instanceManager.PushInstance(hwndExplr, tabBar);
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar")) {
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
                if(ExtHasIcon(ext) && !path.StartsWith(@"\\")) {
                    return new ImageReservationKey(path, 2);
                }
                return new ImageReservationKey(ext, 1);
            }
            if(path.StartsWith(@"\\")) {
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
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar")) {
                if(key != null) {
                    string str = string.Empty;
                    foreach(string str2 in closingPaths) {
                        str = str + str2 + ";";
                    }
                    str = str.TrimEnd(SEPARATOR_CHAR);
                    key.SetValue("TabsOnLastClosedWindow", str);
                }
            }
        }

        public static void SaveGroupsReg() {
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar\Groups")) {
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

        public static void SetConfigAt(Settings setting, bool fOn) {
            int index = (int)setting >> 3;
            byte value = (byte)(1 << ((int)setting & 7));
            if (fOn) {
                ConfigValues[index] = (byte)(ConfigValues[index] | value);
            }
            else {
                ConfigValues[index] = (byte)(ConfigValues[index] & ((byte)(0xff - value)));
            }
        }
        
        private static void SetImageKey(string key, string itemPath) {
            if(!ImageListGlobal.Images.ContainsKey(key)) {
                ImageListGlobal.Images.Add(key, GetIcon(itemPath, false));
            }
        }

        public static void SetTabBarOption(TabBarOption tabBarOption, QTTabBarClass tabBar) {
            bool fAutoSubText = !CheckConfig(Settings.NoRenameAmbTabs);
            bool fPaintBG = CheckConfig(Settings.ToolbarBGColor);
            string str = Path_LanguageFile;
            Dictionary<int, object> dictionary = tabBarOption.Dictionary;
            Dictionary<int, object> dictionary2 = GetTabBarOption().Dictionary;
            Dictionary<int, object> dictionary3 = new Dictionary<int, object>(dictionary2);
            foreach(int num in dictionary3.Keys) {
                if(!dictionary.ContainsKey(num)) {
                    continue;
                }
                object obj2 = dictionary[num];
                if(((((num < 0x10000) && (obj2 is bool)) || (((0x10000 <= num) && (num < 0x20000)) && (obj2 is int))) || ((((0x20000 <= num) && (num < 0x40000)) && (obj2 is string)) || (((0x40000 <= num) && (num < 0x80000)) && (obj2 is System.Drawing.Color)))) || ((((num == 0x80000) && (obj2 is Padding)) || ((num == 0x80001) && (obj2 is float))) || ((((num == 0x80002) && (obj2 is string[])) || ((num == 0x80003) && (obj2 is string[]))) || ((num == 0x80004) && (obj2 is float))))) {
                    dictionary2[num] = dictionary[num];
                }
            }
            SetConfigAt(Settings.ActivateNewTab, (bool)dictionary2[0]);
            SetConfigAt(Settings.DontOpenSame, (bool)dictionary2[1]);
            SetConfigAt(Settings.CloseWhenGroup, (bool)dictionary2[2]);
            SetConfigAt(Settings.ShowTooltips, (bool)dictionary2[3]);
            SetConfigAt(Settings.CaptureX1X2, (bool)dictionary2[4]);
            SetConfigAt(Settings.ShowNavButtons, (bool)dictionary2[5]);
            SetConfigAt(Settings.NoHistory, !((bool)dictionary2[6]));
            SetConfigAt(Settings.NoRecentFiles, !((bool)dictionary2[7]));
            SetConfigAt(Settings.DragDropOntoTabs, (bool)dictionary2[8]);
            SetConfigAt(Settings.NoRenameAmbTabs, !((bool)dictionary2[9]));
            SetConfigAt(Settings.MidClickNewWindow, (bool)dictionary2[10]);
            SetConfigAt(Settings.NoDblClickUpLevel, !((bool)dictionary2[11]));
            SetConfigAt(Settings.NoWindowResizing, (bool)dictionary2[12]);
            SetConfigAt(Settings.SaveTransparency, (bool)dictionary2[13]);
            SetConfigAt(Settings.DontCaptureNewWnds, !((bool)dictionary2[14]));
            SetConfigAt(Settings.NoNewWndFolderTree, !((bool)dictionary2[15]));
            SetConfigAt(Settings.NoTabsFromOutside, (bool)dictionary2[0x10]);
            SetConfigAt(Settings.HorizontalScroll, !((bool)dictionary2[0x11]));
            SetConfigAt(Settings.CtrlWheelChangeView, !((bool)dictionary2[0x12]));
            SetConfigAt(Settings.NeverCloseWindow, (bool)dictionary2[0x13]);
            SetConfigAt(Settings.NeverCloseWndLocked, (bool)dictionary2[20]);
            SetConfigAt(Settings.RestoreClosed, (bool)dictionary2[0x15]);
            SetConfigAt(Settings.RestoreLockedTabs, (bool)dictionary2[0x16]);
            SetConfigAt(Settings.UseTabSkin, (bool)dictionary2[0x17]);
            SetConfigAt(Settings.FolderIcon, (bool)dictionary2[0x18]);
            SetConfigAt(Settings.ActiveTabInBold, (bool)dictionary2[0x19]);
            SetConfigAt(Settings.ToolbarBGColor, (bool)dictionary2[0x1a]);
            SetConfigAt(Settings.HideMenuBar, (bool)dictionary2[0x1b]);
            SetConfigAt(Settings.BackspaceUpLevel, (bool)dictionary2[0x1c]);
            SetConfigAt(Settings.NoFullRowSelect, ((bool)dictionary2[0x1d]) ^ IsVista);
            SetConfigAt(Settings.DetailsGridLines, (bool)dictionary2[30]);
            SetConfigAt(Settings.AlternateRowColors, (bool)dictionary2[0x1f]);
            SetConfigAt(Settings.ShowTooltipPreviews, (bool)dictionary2[0x20]);
            SetConfigAt(Settings.PreviewsWithShift, (bool)dictionary2[0x21]);
            SetConfigAt(Settings.NoShowSubDirTips, !((bool)dictionary2[0x22]));
            SetConfigAt(Settings.SubDirTipsWithShift, (bool)dictionary2[0x23]);
            SetConfigAt(Settings.SubDirTipsPreview, !((bool)dictionary2[0x24]));
            SetConfigAt(Settings.SubDirTipsHidden, (bool)dictionary2[0x25]);
            SetConfigAt(Settings.SubDirTipsSystem, (bool)dictionary2[0x26]);
            SetConfigAt(Settings.SubDirTipsFiles, (bool)dictionary2[0x27]);
            SetConfigAt(Settings.ExtWhileRenaming, !((bool)dictionary2[40]));
            SetConfigAt(Settings.TrayOnClose, (bool)dictionary2[0x29]);
            SetConfigAt(Settings.NavButtonsOnRight, ((int)dictionary2[0x10000]) == 1);
            MaxCount_History = ValidateMaxMin((int)dictionary2[0x10001], 0x40, 1);
            ConfigValues[1] = (byte)ValidateMaxMin((int)dictionary2[0x10002], 3, 0);
            ConfigValues[2] = (byte)ValidateMaxMin((int)dictionary2[0x10003], 4, 0);
            ConfigValues[3] = (byte)ValidateMaxMin((int)dictionary2[0x10004], 8, 0);
            ConfigValues[4] = (byte)ValidateMaxMin((int)dictionary2[0x10005], 11, 0);
            if(((int)dictionary2[0x10006]) == 0) {
                ConfigValues[7] = (byte)(ConfigValues[7] & 0xcf);
            }
            else if(((int)dictionary2[0x10006]) == 1) {
                ConfigValues[7] = (byte)(ConfigValues[7] | 0x20);
                ConfigValues[7] = (byte)(ConfigValues[7] & 0xef);
            }
            else {
                ConfigValues[7] = (byte)(ConfigValues[7] | 0x10);
                ConfigValues[7] = (byte)(ConfigValues[7] & 0xdf);
            }
            if(((int)dictionary2[0x10007]) == 1) {
                ConfigValues[5] = (byte)(ConfigValues[5] | 1);
            }
            else {
                ConfigValues[5] = (byte)(ConfigValues[5] & 0xfe);
            }
            TabHeight = ValidateMaxMin((int)dictionary2[0x10008], 50, 10);
            if(((int)dictionary2[0x10009]) == 0) {
                ConfigValues[0] = (byte)(ConfigValues[0] & 0xfb);
                ConfigValues[5] = (byte)(ConfigValues[5] & 0xfb);
            }
            else if(((int)dictionary2[0x10009]) == 1) {
                ConfigValues[0] = (byte)(ConfigValues[0] | 4);
                ConfigValues[5] = (byte)(ConfigValues[5] & 0xfb);
            }
            else {
                ConfigValues[0] = (byte)(ConfigValues[0] & 0xfb);
                ConfigValues[5] = (byte)(ConfigValues[5] | 4);
            }
            TabWidth = ValidateMaxMin((int)dictionary2[0x1000a], 0x200, 10);
            MaxTabWidth = ValidateMaxMin((int)dictionary2[0x1000b], 0x200, 10);
            MinTabWidth = ValidateMaxMin((int)dictionary2[0x1000c], 0x200, 10);
            if(MaxTabWidth < MinTabWidth) {
                MinTabWidth = MaxTabWidth;
            }
            PreviewMaxWidth = ValidateMaxMin((int)dictionary2[0x1000d], 0x780, 0x80);
            PreviewMaxHeight = ValidateMaxMin((int)dictionary2[0x1000e], 0x4b0, 0x60);
            Path_LanguageFile = (string)dictionary2[0x20000];
            Action_BarDblClick = (string)dictionary2[0x20001];
            Path_TabImage = (string)dictionary2[0x20002];
            if(TabFont == null) {
                Font defaultFont = Control.DefaultFont;
                if(((defaultFont.FontFamily.Name != ((string)dictionary2[0x20003])) || (defaultFont.Size != ((float)dictionary2[0x80001]))) && (((float)dictionary2[0x80001]) > 0f)) {
                    TabFont = new Font((string)dictionary2[0x20003], (float)dictionary2[0x80001]);
                }
            }
            else if((((float)dictionary2[0x80001]) > 0f) && ((TabFont.Name != ((string)dictionary2[0x20003])) || (TabFont.Size != ((float)dictionary2[0x80001])))) {
                TabFont = new Font((string)dictionary2[0x20003], (float)dictionary2[0x80001]);
            }
            Path_PluginLangFile = (string)dictionary2[0x20004];
            PreviewFontName = (string)dictionary2[0x20005];
            TabTextColor_Active = (System.Drawing.Color)dictionary2[0x40000];
            TabTextColor_Inactv = (System.Drawing.Color)dictionary2[0x40001];
            RebarBGColor = (System.Drawing.Color)dictionary2[0x40002];
            ShellViewRowCOLORREF_Text = QTUtility2.MakeCOLORREF((System.Drawing.Color)dictionary2[0x40003]);
            ShellViewRowCOLORREF_Background = QTUtility2.MakeCOLORREF((System.Drawing.Color)dictionary2[0x40004]);
            TabHiliteColor = (System.Drawing.Color)dictionary2[0x40005];
            TabImageSizingMargin = (Padding)dictionary2[0x80000];
            PreviewExtsList_Txt = new List<string>((string[])dictionary2[0x80002]);
            PreviewExtsList_Img = new List<string>((string[])dictionary2[0x80003]);
            PreviewFontSize = (float)dictionary2[0x80004];
            OptionsDialog.WriteRegistry(TabFont, PreviewExtsList_Txt.ToArray(), PreviewExtsList_Img.ToArray());
            tabBar.RefreshOptions(fAutoSubText, fPaintBG, str);
        }

        private static int ValidateMaxMin(int value, int max, int min) {
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
                string[] strArray3 = null;
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
