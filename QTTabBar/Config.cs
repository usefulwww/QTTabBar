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
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Linq;
using Microsoft.Win32;

namespace QTTabBarLib {

    public enum TabPos {
        Rightmost,
        Right,
        Left,
        Leftmost,
        LastActive,
    }

    public enum StretchMode {
        Full,
        Real,
        Tile,
    }

    public enum MouseTarget {
        Anywhere,
        Tab,
        TabBarBackground,
        FolderLink,
        ExplorerItem,
        ExplorerBackground
    }

    [Flags]
    public enum MouseChord {
        None    =   0,
        Shift   =   1,
        Ctrl    =   2,
        Alt     =   4,
        Left    =   8,
        Right   =  16,
        Middle  =  32,
        Double  =  64,
        X1      = 128,
        X2      = 256,
    }

    // WARNING
    // reordering these will break existing settings.
    public enum BindAction {
        GoBack,
        GoForward,
        GoFirst,
        GoLast,
        NextTab,
        PreviousTab,
        FirstTab,
        LastTab,
        CloseCurrent,
        CloseAllButCurrent,
        CloseLeft,
        CloseRight,
        CloseWindow,
        RestoreLastClosed,
        CloneCurrent,
        TearOffCurrent,
        LockCurrent,
        LockAll,
        BrowseFolder,
        CreateNewGroup,
        ShowOptions,
        ShowToolbarMenu,
        ShowTabMenuCurrent,
        ShowGroupMenu,
        ShowRecentFolderMenu,
        ShowUserAppsMenu,
        ToggleMenuBar,
        CopySelectedPaths,
        CopySelectedNames,
        CopyCurrentFolderPath,
        CopyCurrentFolderName,
        ChecksumSelected,
        ToggleTopMost,
        TransparencyPlus,
        TransparencyMinus,
        FocusFileList,
        FocusSearchBarReal,
        FocusSearchBarBBar,
        ShowSDTSelected,
        SendToTray,
        FocusTabBar,

        // New
        NewTab,
        NewWindow,
        NewFolder,
        NewFile,
        SwitchToLastActivated,
        MergeWindows,
        ShowRecentFilesMenu,
        SortTabsByName,
        SortTabsByPath,
        SortTabsByActive,
        
        // Mouse-only from here on down
        UpOneLevel,
        Refresh,
        Paste,
        Maximize,
        Minimize,

        // Item Actions
        ItemOpenInNewTab,
        ItemOpenInNewTabNoSel,
        ItemOpenInNewWindow,
        ItemCut,
        ItemCopy,        
        ItemDelete,
        ItemProperties,
        CopyItemPath,
        CopyItemName,
        ChecksumItem,

        // Tab Actions
        CloseTab,
        CloseLeftTab,
        CloseRightTab,
        UpOneLevelTab, //hmm
        LockTab,
        ShowTabMenu,
        TearOffTab,
        CloneTab,
        CopyTabPath,
        TabProperties,
        ShowTabSubfolderMenu,
        CloseAllButThis,
    }

    [Serializable]
    public class Config {

        // Shortcuts to the loaded config, for convenience.
        public static _Window Window    { get { return ConfigManager.LoadedConfig.window; } }
        public static _Tabs Tabs        { get { return ConfigManager.LoadedConfig.tabs; } }
        public static _Tweaks Tweaks    { get { return ConfigManager.LoadedConfig.tweaks; } }
        public static _Tips Tips        { get { return ConfigManager.LoadedConfig.tips; } }
        public static _Misc Misc        { get { return ConfigManager.LoadedConfig.misc; } }
        public static _Skin Skin        { get { return ConfigManager.LoadedConfig.skin; } }
        public static _BBar BBar        { get { return ConfigManager.LoadedConfig.bbar; } }
        public static _Mouse Mouse      { get { return ConfigManager.LoadedConfig.mouse; } }
        public static _Keys Keys        { get { return ConfigManager.LoadedConfig.keys; } }
        public static _Plugin Plugin    { get { return ConfigManager.LoadedConfig.plugin; } }
        public static _Lang Lang        { get { return ConfigManager.LoadedConfig.lang; } }

        public _Window window   { get; set; }
        public _Tabs tabs       { get; set; }
        public _Tweaks tweaks   { get; set; }
        public _Tips tips       { get; set; }
        public _Misc misc       { get; set; }
        public _Skin skin       { get; set; }
        public _BBar bbar       { get; set; }
        public _Mouse mouse     { get; set; }
        public _Keys keys       { get; set; }
        public _Plugin plugin   { get; set; }
        public _Lang lang       { get; set; }

        public Config() {
            window = new _Window();
            tabs = new _Tabs();
            tweaks = new _Tweaks();
            tips = new _Tips();
            misc = new _Misc();
            skin = new _Skin();
            bbar = new _BBar();
            mouse = new _Mouse();
            keys = new _Keys();
            plugin = new _Plugin();
            lang = new _Lang();
        }

        [Serializable]
        public class _Window {
            public bool CaptureNewWindows        { get; set; }
            public bool RestoreSession           { get; set; }
            public bool RestoreOnlyLocked        { get; set; }
            public bool CloseBtnClosesUnlocked   { get; set; }
            public bool CloseBtnClosesSingleTab  { get; set; }
            public bool TrayOnClose              { get; set; }
            public bool TrayOnMinimize           { get; set; }
            public byte[] DefaultLocation        { get; set; }

            public _Window() {
                CaptureNewWindows = false;
                RestoreSession = false;
                RestoreOnlyLocked = false;
                CloseBtnClosesSingleTab = true;
                CloseBtnClosesUnlocked = false;
                TrayOnClose = false;
                TrayOnMinimize = false;
                
                // TODO: should be Libraries on win7
                // It's just Computer for now.
                using(IDLWrapper w = new IDLWrapper("::{20D04FE0-3AEA-1069-A2D8-08002B30309D}")) {
                    DefaultLocation = w.IDL;
                }
            }
        }

        [Serializable]
        public class _Tabs {
            public TabPos NewTabPosition         { get; set; }
            public TabPos NextAfterClosed        { get; set; }
            public bool ActivateNewTab           { get; set; }
            public bool NeverOpenSame            { get; set; }
            public bool RenameAmbTabs            { get; set; }
            public bool DragOverTabOpensSDT      { get; set; }
            public bool ShowFolderIcon           { get; set; }
            public bool ShowSubDirTipOnTab       { get; set; }
            public bool ShowDriveLetters         { get; set; }
            public bool ShowCloseButtons         { get; set; }
            public bool CloseBtnsWithAlt         { get; set; }
            public bool CloseBtnsOnHover         { get; set; }
            public bool ShowNavButtons           { get; set; }
            public bool NavButtonsOnRight        { get; set; }
            public bool MultipleTabRows          { get; set; }
            public bool ActiveTabOnBottomRow     { get; set; }

            public _Tabs() {
                NewTabPosition = TabPos.Rightmost;
                NextAfterClosed = TabPos.LastActive;
                ActivateNewTab = true;
                NeverOpenSame = true;
                RenameAmbTabs = false;
                DragOverTabOpensSDT = false;
                ShowFolderIcon = true;
                ShowSubDirTipOnTab = true;
                ShowDriveLetters = false;
                ShowCloseButtons = false;
                CloseBtnsWithAlt = false;
                CloseBtnsOnHover = false;
                ShowNavButtons = false;
                MultipleTabRows = true;
                ActiveTabOnBottomRow = true;
            }
        }

        [Serializable]
        public class _Tweaks {
            public bool AlwaysShowHeaders        { get; set; }
            public bool KillExtWhileRenaming     { get; set; }
            public bool RedirectLibraryFolders   { get; set; }
            public bool F2Selection              { get; set; }
            public bool WrapArrowKeySelection    { get; set; }
            public bool BackspaceUpLevel         { get; set; }
            public bool HorizontalScroll         { get; set; }
            public bool ForceSysListView         { get; set; }
            public bool ToggleFullRowSelect      { get; set; }
            public bool DetailsGridLines         { get; set; }
            public bool AlternateRowColors       { get; set; }
            public Color BackgroundColor         { get; set; }
            public Color TextColor               { get; set; }


            public _Tweaks() {
                AlwaysShowHeaders = false;
                KillExtWhileRenaming = true;
                RedirectLibraryFolders = false;
                F2Selection = true;
                WrapArrowKeySelection = false;
                BackspaceUpLevel = false;
                HorizontalScroll = true;
                ForceSysListView = false;
                ToggleFullRowSelect = false;
                DetailsGridLines = false;
                AlternateRowColors = false;
                TextColor = new Color();
                BackgroundColor = new Color();
            }
        }

        [Serializable]
        public class _Tips {
            public bool ShowSubDirTips           { get; set; }
            public bool SubDirTipsPreview        { get; set; }
            public bool SubDirTipsFiles          { get; set; }
            public bool SubDirTipsWithShift      { get; set; }
            public bool ShowTooltipPreviews      { get; set; }
            public bool ShowPreviewsWithShift    { get; set; }
            public bool ShowPreviewInfo          { get; set; }
            public int PreviewMaxWidth           { get; set; }
            public int PreviewMaxHeight          { get; set; }
            public Font PreviewFont              { get; set; }

            public _Tips() {
                ShowSubDirTips = true;
                SubDirTipsPreview = true;
                SubDirTipsFiles = true;
                SubDirTipsWithShift = false;
                ShowTooltipPreviews = true;
                ShowPreviewsWithShift = false;
                ShowPreviewInfo = true;
                PreviewMaxWidth = 512;
                PreviewMaxHeight = 256;
            }
        }

        [Serializable]
        public class _Misc {
            public bool TaskbarThumbnails        { get; set; }
            public bool KeepHistory              { get; set; }
            public int TabHistoryCount           { get; set; }
            public bool KeepRecentFiles          { get; set; }
            public int FileHistoryCount          { get; set; }
            public int NetworkTimeout            { get; set; }
            public bool AutoUpdate               { get; set; }
            public bool UseIniFile               { get; set; }

            public _Misc() {
                TaskbarThumbnails = false;
                KeepHistory = true;
                TabHistoryCount = 15;
                KeepRecentFiles = true;
                FileHistoryCount = 15;
                NetworkTimeout = 0; // TODO
                AutoUpdate = true;
                UseIniFile = false;
            }
        }

        [Serializable]
        public class _Skin {
            public bool UseTabSkin               { get; set; }
            public string TabImageFile           { get; set; }
            public Rectangle TabSizeMargin       { get; set; }
            public Rectangle TabContentMargin    { get; set; }
            public int OverlapPixels             { get; set; }
            public bool HitTestTransparent       { get; set; }
            public int TabHeight                 { get; set; }
            public int TabMinWidth               { get; set; }
            public int TabMaxWidth               { get; set; }
            public bool FixedWidthTabs           { get; set; }
            public Font TabTextFont              { get; set; }
            public Color TabTextActiveColor      { get; set; }
            public Color TabTextInactiveColor    { get; set; }
            public Color TabTextHotColor         { get; set; }
            public Color TabShadActiveColor      { get; set; }
            public Color TabShadInactiveColor    { get; set; }
            public Color TabShadHotColor         { get; set; }
            public bool TabTitleShadows          { get; set; }
            public bool TabTextCentered          { get; set; }
            public bool UseRebarBGColor          { get; set; }
            public Color RebarColor              { get; set; }
            public bool UseRebarImage            { get; set; }
            public StretchMode RebarStretchMode  { get; set; }
            public string RebarImageFile         { get; set; }
            public bool RebarImageSeperateBars   { get; set; }
            public Rectangle RebarSizeMargin     { get; set; }
            public bool ActiveTabInBold          { get; set; }

            public _Skin() {
                UseTabSkin = false;
                TabImageFile = "";
                TabSizeMargin = new Rectangle(0, 0, 0, 0);
                TabContentMargin = new Rectangle(0, 0, 0, 0);
                OverlapPixels = 0;
                HitTestTransparent = false;
                TabHeight = 24;
                TabMinWidth = 70; // TODO
                TabMaxWidth = 150; // TODO
                FixedWidthTabs = false;
                TabTextActiveColor = Color.Black;
                TabTextInactiveColor = Color.Black;
                TabTextHotColor = Color.Black;
                TabShadActiveColor = Color.Gray;
                TabShadInactiveColor = Color.White;
                TabShadHotColor = Color.White;
                TabTitleShadows = false;
                TabTextCentered = false;
                UseRebarBGColor = false;
                RebarColor = Color.Gray;
                UseRebarImage = false;
                RebarStretchMode = StretchMode.Full;
                RebarImageFile = "";
                RebarImageSeperateBars = false;
                RebarSizeMargin = new Rectangle(0, 0, 0, 0);
                ActiveTabInBold = false;
            }
        }

        [Serializable]
        public class _BBar {
            public int[] ButtonIndexes           { get; set; }
            public bool LargeButtons             { get; set; }
            public bool LockSearchBarWidth       { get; set; }
            public bool LockDropDownButtons      { get; set; }
            public bool ShowButtonLabels         { get; set; }
            public string ImageStripPath         { get; set; }
            
            public _BBar() {
                // todo
                // we can't check QTUtility.IsXP here, due to a circular reference between
                // QTUtility's static constructor and ConfigManager's.
                ButtonIndexes = //QTUtility.IsXP 
                        //? new int[] {1, 2, 0, 3, 4, 5, 0, 6, 7, 0, 11, 13, 12, 14, 15, 0, 9, 20} 
                        /*:*/ new int[] {3, 4, 5, 0, 6, 7, 0, 11, 13, 12, 14, 15, 0, 9, 20};
                LockDropDownButtons = false;
                LargeButtons = true;
                LockSearchBarWidth = false;
                ShowButtonLabels = false;
                ImageStripPath = "";
            }
        }

        [Serializable]
        public class _Mouse {
            public bool MouseScrollsHotWnd       { get; set; }
            public Dictionary<MouseChord, BindAction> GlobalMouseActions { get; set; }
            public Dictionary<MouseChord, BindAction> TabActions { get; set; }
            public Dictionary<MouseChord, BindAction> BarActions { get; set; }
            public Dictionary<MouseChord, BindAction> LinkActions { get; set; }
            public Dictionary<MouseChord, BindAction> ItemActions { get; set; }
            public Dictionary<MouseChord, BindAction> MarginActions { get; set; }

            public _Mouse() {
                MouseScrollsHotWnd = false;
                GlobalMouseActions = new Dictionary<MouseChord, BindAction> {
                    {MouseChord.X1, BindAction.GoBack},
                    {MouseChord.X2, BindAction.GoForward}
                };
                TabActions = new Dictionary<MouseChord, BindAction> { 
                    {MouseChord.Middle, BindAction.CloseTab},
                    {MouseChord.Ctrl | MouseChord.Left, BindAction.LockTab},
                    {MouseChord.Double, BindAction.UpOneLevelTab},
                };
                BarActions = new Dictionary<MouseChord, BindAction> {
                    {MouseChord.Double, BindAction.NewTab},
                    {MouseChord.Middle, BindAction.RestoreLastClosed}
                };
                LinkActions = new Dictionary<MouseChord, BindAction> {
                    {MouseChord.Middle, BindAction.ItemOpenInNewTab},
                    {MouseChord.Ctrl | MouseChord.Middle, BindAction.ItemOpenInNewWindow}
                };
                ItemActions = new Dictionary<MouseChord, BindAction> {
                    {MouseChord.Middle, BindAction.ItemOpenInNewTab},
                    {MouseChord.Ctrl | MouseChord.Middle, BindAction.ItemOpenInNewWindow}                        
                };
                MarginActions = new Dictionary<MouseChord, BindAction> {
                    {MouseChord.Double, BindAction.UpOneLevel}
                };
            }
        }

        [Serializable]
        public class _Keys {
            public int[] Shortcuts               { get; set; }

            public _Keys() {
                // todo
                Shortcuts = new int[] { 
                  0, 0, 0x160025, 0x160027, 0x120009, 0x130009, 0, 0, 0x120057, 0x130057, 0, 0, 0, 0x13005a, 0x12004e, 0x13004e, 
                  0x12004c, 0x13004c, 0x12004f, 0, 0x14004f, 0x1400bc, 0x1400be, 0x140047, 0x140048, 0x140055, 0x14004d, 0, 0, 0, 0, 0, 
                  0, 0, 0, 0, 0, 0, 0, 0, 0
                 };
            }
        }

        [Serializable]
        public class _Plugin {
            public string[] Enabled              { get; set; }

            public _Plugin() {
                Enabled = new string[0];
            }
        }

        [Serializable]
        public class _Lang {
            public bool UseBuiltIn               { get; set; }
            public string LangFile               { get; set; }

            public _Lang() {
                UseBuiltIn = true;
                LangFile = "English";
            }
        }

        //-----------

        // Maybe
        public static bool CtrlWheelChangeView { get; set; }
        public static bool ShowHashResult { get; set; }
        public static bool HashTopMost { get; set; }
        public static bool HashFullPath { get; set; }
        public static bool HashClearOnClose { get; set; }
        public static bool KeepOnSeparate { get; set; }        

        // DEATH ROW
        public static bool DontCaptureNewWnds { get; set; }
        public static bool AllRecentFiles { get; set; } 
        public static bool AlignTabTextCenter { get; set; }
        public static bool ShowTooltips        { get; set; }
        public static bool CloseWhenGroup      { get; set; }
        public static bool NoCaptureMidClick   { get; set; }        
        public static bool LimitedWidthTabs    { get; set; }        
        public static bool CaptureX1X2         { get; set; }
        public static bool NoWindowResizing    { get; set; }
        public static bool NoNewWndFolderTree  { get; set; }
        public static bool NoDblClickUpLevel   { get; set; }
        public static bool MidClickNewWindow   { get; set; }
        public static bool HideMenuBar         { get; set; }
        public static bool SaveTransparency    { get; set; }
        public static bool SubDirTipsHidden    { get; set; }
        public static bool SubDirTipsSystem    { get; set; }
        public static bool RebarImageTile      { get; set; }
        public static bool RebarImageStretch2  { get; set; }
        public static bool NoTabSwitcher       { get; set; }
        public static bool NoMidClickTree      { get; set; }
        public static bool XPStyleMenus        { get; set; }
        public static bool NonDefaultMenu      { get; set; }
        public static bool DisableSound        { get; set; }
    }

    public static class ConfigManager {
        public static Config LoadedConfig;

        static ConfigManager() {
            LoadedConfig = new Config();
            ReadConfig();
        }

        public static void ReadConfig() {
            const string RegPath = @"Software\Quizo\QTTabBar\Config\"; // TODO

            // Properties from all categories
            foreach(PropertyInfo category in typeof(Config).GetProperties().Where(c => c.CanWrite)) {
                Type cls = category.PropertyType;
                object val = category.GetValue(LoadedConfig, null);
                using(RegistryKey key = Registry.CurrentUser.CreateSubKey(RegPath + cls.Name.Substring(1))) {
                    foreach(PropertyInfo prop in cls.GetProperties()) {
                        object obj = key.GetValue(prop.Name);
                        if(obj == null) continue;
                        Type t = prop.PropertyType;
                        try {
                            if(t == typeof(bool)) {
                                prop.SetValue(val, (int)obj != 0, null);
                            }
                            else if(t == typeof(int) || t == typeof(string)) {
                                prop.SetValue(val, obj, null);
                            }
                            else if(t.IsEnum) {
                                prop.SetValue(val, Enum.Parse(t, obj.ToString()), null);
                            }
                            else {
                                DataContractJsonSerializer ser = new DataContractJsonSerializer(t);
                                using(MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(obj.ToString()))) {
                                    prop.SetValue(val, ser.ReadObject(stream), null);
                                }
                            }
                        }
                        catch {
                        }
                    }
                }
            }

            // TODO non-props   
        }
        public static void WriteConfig() {
            const string RegPath = @"Software\Quizo\QTTabBar\Config\"; // TODO

            // Properties from all categories
            foreach(PropertyInfo category in typeof(Config).GetProperties().Where(c => c.CanWrite)) {
                Type cls = category.PropertyType;
                object val = category.GetValue(LoadedConfig, null);
                using(RegistryKey key = Registry.CurrentUser.CreateSubKey(RegPath + cls.Name.Substring(1))) {
                    foreach(var prop in cls.GetProperties()) {
                        Type t = prop.PropertyType;
                        if(t == typeof(bool)) {
                            key.SetValue(prop.Name, (bool)prop.GetValue(val, null) ? 1 : 0);
                        }
                        else if(t == typeof(int) || t == typeof(string) || t.IsEnum) {
                            key.SetValue(prop.Name, prop.GetValue(val, null));
                        }
                        else {
                            DataContractJsonSerializer ser = new DataContractJsonSerializer(t);
                            using(MemoryStream stream = new MemoryStream()) {
                                ser.WriteObject(stream, prop.GetValue(val, null));
                                stream.Position = 0;
                                StreamReader reader = new StreamReader(stream);
                                key.SetValue(prop.Name, reader.ReadToEnd());                                
                            }
                        }
                    }
                }
            }

            // TODO non-props
        }
    }
}
