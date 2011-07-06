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
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows.Forms;
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

    public enum MouseTargets {
        // TODO
    }

    public enum MouseAction {
        // TODO
    }

    public struct MouseChord {
        public Keys modifiers;
        public MouseButtons button;
        public bool dbl;
        public MouseTargets target;
    }

    public static class Config {

       public static class Window {
            public static bool CaptureNewWindows        { get; set; }
            public static bool RestoreSession           { get; set; }
            public static bool RestoreOnlyLocked        { get; set; }
            public static bool CloseBtnClosesUnlocked   { get; set; }
            public static bool CloseBtnClosesSingleTab  { get; set; }
            public static bool TrayOnClose              { get; set; }
            public static bool TrayOnMinimize           { get; set; }
        }

        public static class Tabs {
            public static TabPos NewTabPosition         { get; set; }
            public static TabPos NextAfterClosed        { get; set; }
            public static bool ActivateNewTab           { get; set; }
            public static bool NeverOpenSame            { get; set; }
            public static bool RenameAmbTabs            { get; set; }
            public static bool DragOverTabOpensSDT      { get; set; }
            public static bool ShowFolderIcon           { get; set; }
            public static bool ShowSubDirTipOnTab       { get; set; }
            public static bool ShowDriveLetters         { get; set; }
            public static bool ShowCloseButtons         { get; set; }
            public static bool CloseBtnsWithAlt         { get; set; }
            public static bool CloseBtnsOnHover         { get; set; }
            public static bool ShowNavButtons           { get; set; }
            public static bool NavButtonsOnRight        { get; set; }
            public static bool MultipleTabRows          { get; set; }
            public static bool ActiveTabOnBottomRow     { get; set; }
        }

        public static class Tweaks {
            public static bool AlwaysShowHeaders        { get; set; }
            public static bool KillExtWhileRenaming     { get; set; }
            public static bool F2Selection              { get; set; }
            public static bool WrapArrowKeySelection    { get; set; }
            public static bool BackspaceUpLevel         { get; set; }
            public static bool HorizontalScroll         { get; set; }
            public static bool ForceSysListView         { get; set; }
            public static bool ToggleFullRowSelect      { get; set; }
            public static bool DetailsGridLines         { get; set; }
            public static bool AlternateRowColors       { get; set; }
        }

        public static class Tips {
            public static bool ShowSubDirTips           { get; set; }
            public static bool SubDirTipsPreview        { get; set; }
            public static bool SubDirTipsFiles          { get; set; }
            public static bool SubDirTipsWithShift      { get; set; }
            public static bool ShowTooltipPreviews      { get; set; }
            public static bool ShowPreviewsWithShift    { get; set; }
            public static bool ShowPreviewInfo          { get; set; }
            public static int PreviewMaxWidth           { get; set; }
            public static int PreviewMaxHeight          { get; set; }
            public static Font PreviewFont              { get; set; }
        }

        public static class Misc {
            public static bool TaskbarThumbnails        { get; set; }
            public static bool KeepHistory              { get; set; }
            public static int TabHistoryCount           { get; set; }
            public static bool KeepRecentFiles          { get; set; }
            public static int FileHistoryCount          { get; set; }
            public static int NetworkTimeout            { get; set; }
            public static bool AutoUpdate               { get; set; }
            public static bool UseIniFile               { get; set; }
        }

        public static class Skin {
            public static bool UseTabSkin               { get; set; }
            public static string TabImageFile           { get; set; }
            public static Rectangle TabSizeMargin       { get; set; }
            public static int TabHeight                 { get; set; }
            public static int TabMinWidth               { get; set; }
            public static int TabMaxWidth               { get; set; }
            public static bool FixedWidthTabs           { get; set; }
            public static Font TabTextFont              { get; set; }
            public static Color TabTextActiveColor      { get; set; }
            public static Color TabTextInactiveColor    { get; set; }
            public static Color TabTextHotColor         { get; set; }
            public static Color TabShadActiveColor      { get; set; }
            public static Color TabShadInactiveColor    { get; set; }
            public static Color TabShadHotColor         { get; set; }
            public static bool TabTitleShadows          { get; set; }
            public static bool TabTextCentered          { get; set; }
            public static bool UseRebarBGColor          { get; set; }
            public static Color RebarColor              { get; set; }
            public static bool UseRebarImage            { get; set; }
            public static StretchMode RebarStretchMode  { get; set; }
            public static string RebarImageFile         { get; set; }
            public static bool RebarImageSeperateBars   { get; set; }
            public static Rectangle RebarSizeMargin     { get; set; }
            public static bool ActiveTabInBold          { get; set; }
        }

        public static class BBar {
            public static bool LargeButtons             { get; set; }
            public static bool LockSearchBarWidth       { get; set; }
            public static bool ButtonText               { get; set; }
            public static bool SelectiveText            { get; set; }
        }

        public static class Mouse {
            public static bool MouseScrollsHotWnd       { get; set; }
            public static Dictionary<MouseChord, MouseAction> MouseActions;
        }

        public static class Lang {
            public static bool UseBuiltIn               { get; set; }
            public static string LangFile               { get; set; }
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
        public enum ConfigCats {
            Window,
            Tabs,
            Tweaks,
            Tips,
            Misc,
            Skin,
            BBar,
            Mouse,
            Lang
        }

        public static void ReadConfig() {
            const string RegPath = @"Software\Quizo\QTTabBar\Config\"; // TODO
            foreach(var cls in typeof(Config).GetNestedTypes()) {
                using(RegistryKey key = Registry.CurrentUser.CreateSubKey(RegPath + cls.Name)) {
                    foreach(var prop in cls.GetProperties()) {
                        object obj = key.GetValue(prop.Name);
                        if(obj == null) continue;
                        Type t = prop.PropertyType;
                        try {
                            if(t == typeof(bool)) {
                                prop.SetValue(null, (int)obj != 0, null);
                            }
                            if(t == typeof(int) || t == typeof(string)) {
                                prop.SetValue(null, obj, null);
                            }
                            else if(t.IsEnum) {
                                prop.SetValue(null, Enum.Parse(t, obj.ToString()), null);
                            }
                            else {
                                DataContractJsonSerializer ser = new DataContractJsonSerializer(t);
                                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(obj.ToString()));
                                prop.SetValue(null, ser.ReadObject(stream), null);
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
            foreach(var cls in typeof(Config).GetNestedTypes()) {
                using(RegistryKey key = Registry.CurrentUser.CreateSubKey(RegPath + cls.Name)) {
                    foreach(var prop in cls.GetProperties()) {
                        Type t = prop.PropertyType;
                        if(t == typeof(bool)) {
                            key.SetValue(prop.Name, (bool)prop.GetValue(null, null) ? 1 : 0);
                        }
                        else if(t == typeof(int) || t == typeof(string) || t.IsEnum) {
                            key.SetValue(prop.Name, prop.GetValue(null, null));
                        }
                        else {
                            DataContractJsonSerializer ser = new DataContractJsonSerializer(t);
                            MemoryStream stream = new MemoryStream();
                            ser.WriteObject(stream, prop.GetValue(null, null));
                            stream.Seek(0, SeekOrigin.Begin);
                            StreamReader reader = new StreamReader(stream);
                            key.SetValue(prop.Name, reader.ReadToEnd());
                        }
                    }
                }
            }

            // TODO non-props
        }

        public static void SetDefaults(ConfigCats cat) {
            switch(cat) {
                case ConfigCats.Window:
                    Config.Window.CaptureNewWindows = false;
                    Config.Window.RestoreSession = false;
                    Config.Window.RestoreOnlyLocked = false;
                    Config.Window.CloseBtnClosesSingleTab = true;
                    Config.Window.CloseBtnClosesUnlocked = false;
                    Config.Window.TrayOnClose = false;
                    Config.Window.TrayOnMinimize = false;
                    break;

                case ConfigCats.Tabs:
                    Config.Tabs.NewTabPosition = TabPos.Rightmost;
                    Config.Tabs.NextAfterClosed = TabPos.LastActive;
                    Config.Tabs.ActivateNewTab = true;
                    Config.Tabs.NeverOpenSame = true;
                    Config.Tabs.RenameAmbTabs = false;
                    Config.Tabs.DragOverTabOpensSDT = false;
                    Config.Tabs.ShowFolderIcon = true;
                    Config.Tabs.ShowSubDirTipOnTab = true;
                    Config.Tabs.ShowDriveLetters = false;
                    Config.Tabs.ShowCloseButtons = false;
                    Config.Tabs.CloseBtnsWithAlt = false;
                    Config.Tabs.CloseBtnsOnHover = false;
                    Config.Tabs.ShowNavButtons = false;
                    Config.Tabs.MultipleTabRows = true;
                    Config.Tabs.ActiveTabOnBottomRow = true;
                    break;

                case ConfigCats.Tweaks:
                    Config.Tweaks.AlwaysShowHeaders = false;
                    Config.Tweaks.KillExtWhileRenaming = true;
                    Config.Tweaks.F2Selection = true;
                    Config.Tweaks.WrapArrowKeySelection = false;
                    Config.Tweaks.BackspaceUpLevel = false;
                    Config.Tweaks.HorizontalScroll = true;
                    Config.Tweaks.ForceSysListView = false;
                    Config.Tweaks.ToggleFullRowSelect = false;
                    Config.Tweaks.DetailsGridLines = false;
                    Config.Tweaks.AlternateRowColors = false;
                    break;

                case ConfigCats.Tips:
                    Config.Tips.ShowSubDirTips = true;
                    Config.Tips.SubDirTipsPreview = true;
                    Config.Tips.SubDirTipsFiles = true;
                    Config.Tips.SubDirTipsWithShift = false;
                    Config.Tips.ShowTooltipPreviews = true;
                    Config.Tips.ShowPreviewsWithShift = false;
                    Config.Tips.ShowPreviewInfo = true;
                    Config.Tips.PreviewMaxWidth = 512;
                    Config.Tips.PreviewMaxHeight = 256;
                    break;

                case ConfigCats.Skin:
                    Config.Skin.UseTabSkin = false;
                    Config.Skin.TabImageFile = "";
                    Config.Skin.TabSizeMargin = new Rectangle(0, 0, 0, 0);
                    Config.Skin.TabHeight = 24;
                    Config.Skin.TabMinWidth = 70; // TODO
                    Config.Skin.TabMaxWidth = 150; // TODO
                    Config.Skin.FixedWidthTabs = false;
                    Config.Skin.TabTextActiveColor = Color.Black;
                    Config.Skin.TabTextInactiveColor = Color.Black;
                    Config.Skin.TabTextHotColor = Color.Black;
                    Config.Skin.TabShadActiveColor = Color.Gray;
                    Config.Skin.TabShadInactiveColor = Color.White;
                    Config.Skin.TabShadHotColor = Color.White;
                    Config.Skin.TabTitleShadows = false;
                    Config.Skin.TabTextCentered = false;
                    Config.Skin.UseRebarBGColor = false;
                    Config.Skin.RebarColor = Color.Gray;
                    Config.Skin.UseRebarImage = false;
                    Config.Skin.RebarStretchMode = StretchMode.Full;
                    Config.Skin.RebarImageFile = "";
                    Config.Skin.RebarImageSeperateBars = false;
                    Config.Skin.RebarSizeMargin = new Rectangle(0, 0, 0, 0);
                    Config.Skin.ActiveTabInBold = false;
                    break;

                case ConfigCats.BBar:
                    Config.BBar.LargeButtons = true;
                    Config.BBar.LockSearchBarWidth = false;
                    Config.BBar.ButtonText = false;
                    Config.BBar.SelectiveText = false;
                    break;

                case ConfigCats.Mouse:
                    Config.Mouse.MouseScrollsHotWnd = false;
                    // TODO actions
                    break;

                case ConfigCats.Lang:
                    Config.Lang.UseBuiltIn = true;
                    Config.Lang.LangFile = "English";
                    break;
            }
        }
    }
}
