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
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Media;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;

    internal sealed class OptionsDialog : Form {
        private static int[] arrSpecialFolderCSIDLs;
        private static string[] arrSpecialFolderDipNms;
        private System.Windows.Forms.Button btnActTxtClr;
        private System.Windows.Forms.Button btnAdd_NoCapture;
        private System.Windows.Forms.Button btnAddImgExt;
        private System.Windows.Forms.Button btnAddSep_app;
        private System.Windows.Forms.Button btnAddSep_Grp;
        private System.Windows.Forms.Button btnAddSpcFol_Grp;
        private System.Windows.Forms.Button btnAddSpcFol_NoCapture;
        private System.Windows.Forms.Button btnAddTextExt;
        private System.Windows.Forms.Button btnAddToken_Arg;
        private System.Windows.Forms.Button btnAddToken_Wrk;
        private System.Windows.Forms.Button btnAddVFolder_app;
        private System.Windows.Forms.Button btnAlternate_Default;
        private System.Windows.Forms.Button btnAlternateColor;
        private System.Windows.Forms.Button btnAlternateColor_Text;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnBFD_app;
        private System.Windows.Forms.Button btnBrowseAction_BarDblClck;
        private System.Windows.Forms.Button btnBrowsePlugin;
        private System.Windows.Forms.Button btnBrowsePluginLang;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnCheckUpdates;
        private System.Windows.Forms.Button btnClearRecentFile;
        private System.Windows.Forms.Button btnCopyKeys;
        private System.Windows.Forms.Button btnDefaultImgExt;
        private System.Windows.Forms.Button btnDefaultTextExt;
        private System.Windows.Forms.Button btnDefTxtClr;
        private System.Windows.Forms.Button btnDelImgExt;
        private System.Windows.Forms.Button btnDelTextExt;
        private System.Windows.Forms.Button btnDown_app;
        private System.Windows.Forms.Button btnDown_Grp;
        private System.Windows.Forms.Button btnExportSettings;
        private System.Windows.Forms.Button btnHiliteClsc;
        private System.Windows.Forms.Button btnHistoryClear;
        private System.Windows.Forms.Button btnInactTxtClr;
        private System.Windows.Forms.Button btnLangBrowse;
        private System.Windows.Forms.Button btnMinus_app;
        private System.Windows.Forms.Button btnMinus_Grp;
        private System.Windows.Forms.Button btnOFD_app;
        private System.Windows.Forms.Button btnOFD_NoCapture;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnPayPal;
        private System.Windows.Forms.Button btnPlus_app;
        private System.Windows.Forms.Button btnPlus_Grp;
        private System.Windows.Forms.Button btnPreviewFont;
        private System.Windows.Forms.Button btnPreviewFontDefault;
        private System.Windows.Forms.Button btnRebarImage;
        private System.Windows.Forms.Button btnRemove_NoCapture;
        private System.Windows.Forms.Button btnShadowAct;
        private System.Windows.Forms.Button btnShadowIna;
        private System.Windows.Forms.Button btnStartUpGrp;
        private System.Windows.Forms.Button btnTabFont;
        private System.Windows.Forms.Button btnTabImage;
        private System.Windows.Forms.Button btnToolBarBGClr;
        private System.Windows.Forms.Button btnUp_app;
        private System.Windows.Forms.Button btnUp_Grp;
        private FormMethodInvoker callBack;
        private System.Windows.Forms.CheckBox chbActivateNew;
        private System.Windows.Forms.CheckBox chbAlternateColor;
        private System.Windows.Forms.CheckBox chbAutoSubText;
        private System.Windows.Forms.CheckBox chbAutoUpdate;
        private System.Windows.Forms.CheckBox chbBlockProcess;
        private System.Windows.Forms.CheckBox chbBoldActv;
        private System.Windows.Forms.CheckBox chbBSUpOneLvl;
        private System.Windows.Forms.CheckBox chbCloseWhenGroup;
        private System.Windows.Forms.CheckBox chbCursorLoop;
        private System.Windows.Forms.CheckBox chbDD;
        private System.Windows.Forms.CheckBox chbDontOpenSame;
        private System.Windows.Forms.CheckBox chbDriveLetter;
        private System.Windows.Forms.CheckBox chbF2Selection;
        private System.Windows.Forms.CheckBox chbFolderIcon;
        private System.Windows.Forms.CheckBox chbFoldrTree;
        private System.Windows.Forms.CheckBox chbGridLine;
        private System.Windows.Forms.CheckBox chbGroupKey;
        private System.Windows.Forms.CheckBox chbHideMenu;
        private System.Windows.Forms.CheckBox chbHolizontalScroll;
        private System.Windows.Forms.CheckBox chbNavBtn;
        private System.Windows.Forms.CheckBox chbNCADblClck;
        private System.Windows.Forms.CheckBox chbNeverCloseWindow;
        private System.Windows.Forms.CheckBox chbNeverCloseWndLocked;
        private System.Windows.Forms.CheckBox chbNoFulRowSelect;
        private System.Windows.Forms.CheckBox chbNoHistory;
        private System.Windows.Forms.CheckBox chbNoTabFromOuteside;
        private System.Windows.Forms.CheckBox chbPlaySound;
        private System.Windows.Forms.CheckBox chbPreviewInfo;
        private System.Windows.Forms.CheckBox chbPreviewMode;
        private System.Windows.Forms.CheckBox chbRebarBGImage;
        private System.Windows.Forms.CheckBox chbRemoveOnSeparate;
        private System.Windows.Forms.CheckBox chbRestoreClosed;
        private System.Windows.Forms.CheckBox chbRestoreLocked;
        private System.Windows.Forms.CheckBox chbSaveExecuted;
        private System.Windows.Forms.CheckBox chbSelectWithoutExt;
        private System.Windows.Forms.CheckBox chbSendToTray;
        private System.Windows.Forms.CheckBox chbSendToTrayOnMinimize;
        private System.Windows.Forms.CheckBox chbShowPreview;
        private System.Windows.Forms.CheckBox chbShowTooltip;
        private System.Windows.Forms.CheckBox chbSubDirTip;
        private System.Windows.Forms.CheckBox chbSubDirTipMode;
        private System.Windows.Forms.CheckBox chbSubDirTipModeFile;
        private System.Windows.Forms.CheckBox chbSubDirTipModeHidden;
        private System.Windows.Forms.CheckBox chbSubDirTipModeSystem;
        private System.Windows.Forms.CheckBox chbSubDirTipOnTab;
        private System.Windows.Forms.CheckBox chbSubDirTipPreview;
        private System.Windows.Forms.CheckBox chbTabCloseBtnAlt;
        private System.Windows.Forms.CheckBox chbTabCloseBtnHover;
        private System.Windows.Forms.CheckBox chbTabCloseButton;
        private System.Windows.Forms.CheckBox chbTabSwitcher;
        private System.Windows.Forms.CheckBox chbTabTitleShadow;
        private System.Windows.Forms.CheckBox chbToolbarBGClr;
        private System.Windows.Forms.CheckBox chbTreeShftWhlTab;
        private System.Windows.Forms.CheckBox chbUserAppKey;
        private System.Windows.Forms.CheckBox chbUseTabSkin;
        private System.Windows.Forms.CheckBox chbWhlChangeView;
        private System.Windows.Forms.CheckBox chbWhlClick;
        private System.Windows.Forms.CheckBox chbWndRestrAlpha;
        private System.Windows.Forms.CheckBox chbWndUnresizable;
        private System.Windows.Forms.CheckBox chbX1X2;
        private ColumnHeader clmKeys_Action;
        private ColumnHeader clmKeys_Key;
        private ColumnHeader clmnHeader_NoCapture;
        private System.Windows.Forms.ComboBox cmbActvClose;
        private System.Windows.Forms.ComboBox cmbBGDblClick;
        private System.Windows.Forms.ComboBox cmbImgExts;
        private System.Windows.Forms.ComboBox cmbMenuRenderer;
        private System.Windows.Forms.ComboBox cmbMultiRow;
        private System.Windows.Forms.ComboBox cmbNavBtn;
        private System.Windows.Forms.ComboBox cmbNewTabLoc;
        private System.Windows.Forms.ComboBox cmbRebarBGImageMode;
        private System.Windows.Forms.ComboBox cmbSpclFol_Grp;
        private System.Windows.Forms.ComboBox cmbSpclFol_NoCapture;
        private System.Windows.Forms.ComboBox cmbTabDblClck;
        private System.Windows.Forms.ComboBox cmbTabSizeMode;
        private System.Windows.Forms.ComboBox cmbTabTextAlignment;
        private System.Windows.Forms.ComboBox cmbTabWhlClck;
        private System.Windows.Forms.ComboBox cmbTextExts;
        private System.Windows.Forms.ComboBox cmbWhlClick;
        private ContextMenuStrip cmsAddToken;
        private static bool fInitialized;
        private bool fNowListViewItemEditing;
        private bool fNowTreeNodeEditing;
        private Font fntStartUpGroup;
        private bool fSuppressTextChangeEvent_Group;
        private bool fSuppressTextChangeEvent_UserApps;
        private int iComboBoxTextPreview;
        private int iConboBoxImagPreview;
        private Label lblAction_BarDblClick;
        private Label lblActvClose;
        private Label lblBGDblClik;
        private Label lblGroupKey;
        private Label lblLang;
        private Label lblMenuRenderer;
        private Label lblMultiRows;
        private Label lblNetworkTimeOut;
        private Label lblNewTabLoc;
        private Label lblPluginLang;
        private Label lblPreviewHeight;
        private Label lblPreviewWidth;
        private Label lblSep;
        private Label lblTabDblClk;
        private Label lblTabFont;
        private Label lblTabHeight;
        private Label lblTabSizeTitle;
        private Label lblTabTextAlignment;
        private Label lblTabTxtClr;
        private Label lblTabWFix;
        private Label lblTabWhlClk;
        private Label lblTabWidth;
        private Label lblTabWMax;
        private Label lblTabWMin;
        private Label lblUserApps_Args;
        private Label lblUserApps_Key;
        private Label lblUserApps_Path;
        private Label lblUserApps_Working;
        private LinkLabel lblVer;
        private System.Windows.Forms.ListView listView_NoCapture;
        private ListViewEx listViewKeyboard;
        private List<PluginAssembly> lstPluginAssembliesUserAdded = new List<PluginAssembly>();
        private const int MAX_PATH = 260;
        private static MenuItemArguments MIA_GROUPSEP;
        private NumericUpDown nudMaxRecentFile;
        private NumericUpDown nudMaxUndo;
        private NumericUpDown nudNetworkTimeOut;
        private NumericUpDown nudPreviewMaxHeight;
        private NumericUpDown nudPreviewMaxWidth;
        private NumericUpDown nudTabHeight;
        private NumericUpDown nudTabWidth;
        private NumericUpDown nudTabWidthMax;
        private NumericUpDown nudTabWidthMin;
        private PluginManager pluginManager;
        private PluginView pluginView;
        private PropertyGrid propertyGrid1;
        private static string RES_REMOVEPLGIN;
        private static string[] ResOpt;
        private static string[] ResOpt_DropDown;
        private static string[] ResOpt_Genre;
        private StringFormat sfPlugins;
        private TabControl tabControl1;
        private TabImageSetting tabImageSetting;
        private TabPage tabPage1_Gnrl;
        private TabPage tabPage2_Tabs;
        private TabPage tabPage3_Wndw;
        private TabPage tabPage4_View;
        private TabPage tabPage5_Grps;
        private TabPage tabPage6_Apps;
        private TabPage tabPage7_Plug;
        private TabPage tabPage8_Keys;
        private TabPage tabPage9_Misc;
        private TabPage tabPageA_Path;
        private System.Windows.Forms.TextBox tbArgs;
        private System.Windows.Forms.TextBox tbGroupKey;
        private System.Windows.Forms.TextBox tbPath;
        private System.Windows.Forms.TextBox tbRebarImagePath;
        private System.Windows.Forms.TextBox tbTabImagePath;
        private System.Windows.Forms.TextBox tbUserAppKey;
        private System.Windows.Forms.TextBox tbWorking;
        private const string TEXT_EXT_DEFVAL_IMG = "(Image & movie file)";
        private const string TEXT_EXT_DEFVAL_TXT = "(Text file)";
        private const string TEXT_ONW_DEFVAL = "Enter path";
        private const string TEXT_SEPDISPLAY = "----------- Separator -----------";
        private System.Windows.Forms.TextBox textBoxAction_BarDblClck;
        private System.Windows.Forms.TextBox textBoxLang;
        private System.Windows.Forms.TextBox textBoxPluginLang;
        private TreeNode tnGroupsRoot;
        private TreeNode tnRoot_UserApps;
        private System.Windows.Forms.TreeView treeViewGroup;
        private System.Windows.Forms.TreeView treeViewUserApps;

        public OptionsDialog(PluginManager pluginManager, FormMethodInvoker callBack) {
            InitializeStaticFields();
            this.pluginManager = pluginManager;
            this.callBack = callBack;
            this.InitializeComponent();
            this.tabImageSetting = new TabImageSetting();
            this.propertyGrid1.SelectedObject = this.tabImageSetting;
            this.propertyGrid1.ExpandAllGridItems();
            this.fntStartUpGroup = new Font(this.treeViewGroup.Font, FontStyle.Underline);
            this.sfPlugins = new StringFormat(StringFormatFlags.NoWrap);
            this.sfPlugins.Alignment = StringAlignment.Near;
            this.sfPlugins.LineAlignment = StringAlignment.Center;
            base.SuspendLayout();
            this.lblVer.Text = "QTTabBar ver " + QTUtility2.MakeVersionString();
            this.tabPage1_Gnrl.Text = ResOpt_Genre[0];
            this.tabPage2_Tabs.Text = ResOpt_Genre[1];
            this.tabPage3_Wndw.Text = ResOpt_Genre[2];
            this.tabPage4_View.Text = ResOpt_Genre[3];
            this.tabPage5_Grps.Text = ResOpt_Genre[4];
            this.tabPage6_Apps.Text = ResOpt_Genre[5];
            this.tabPage7_Plug.Text = ResOpt_Genre[6];
            this.tabPage8_Keys.Text = ResOpt_Genre[7];
            this.tabPage9_Misc.Text = ResOpt_Genre[8];
            this.tabPageA_Path.Text = ResOpt_Genre[9];
            string[] strArray = QTUtility.TextResourcesDic["DialogButtons"];
            this.btnOK.Text = strArray[0];
            this.btnCancel.Text = strArray[1];
            this.btnApply.Text = strArray[2];
            this.chbActivateNew.Text = ResOpt[0];
            this.chbDontOpenSame.Text = ResOpt[1];
            this.chbCloseWhenGroup.Text = ResOpt[2];
            this.chbShowTooltip.Text = ResOpt[3];
            this.chbX1X2.Text = ResOpt[4];
            this.chbNavBtn.Text = ResOpt[5];
            this.chbNoHistory.Text = ResOpt[6];
            this.chbSaveExecuted.Text = ResOpt[7];
            this.chbDD.Text = ResOpt[8];
            this.lblLang.Text = ResOpt[9];
            this.lblNewTabLoc.Text = ResOpt[10];
            this.lblActvClose.Text = ResOpt[11];
            this.lblTabDblClk.Text = ResOpt[12];
            this.lblBGDblClik.Text = ResOpt[13];
            this.lblAction_BarDblClick.Text = ResOpt[14];
            this.lblMultiRows.Text = ResOpt[15];
            this.chbAutoSubText.Text = ResOpt[0x10];
            this.chbWhlClick.Text = ResOpt[0x11];
            this.chbNCADblClck.Text = ResOpt[0x12];
            this.chbWndUnresizable.Text = ResOpt[0x13];
            this.chbWndRestrAlpha.Text = ResOpt[20];
            this.chbBlockProcess.Text = ResOpt[0x15];
            this.chbFoldrTree.Text = ResOpt[0x16];
            this.chbNoTabFromOuteside.Text = ResOpt[0x17];
            this.chbHolizontalScroll.Text = ResOpt[0x18];
            this.chbWhlChangeView.Text = ResOpt[0x19];
            this.chbNeverCloseWindow.Text = ResOpt[0x1a];
            this.chbNeverCloseWndLocked.Text = ResOpt[0x1b];
            this.chbRestoreClosed.Text = ResOpt[0x1c];
            this.chbRestoreLocked.Text = ResOpt[0x1d];
            this.chbUseTabSkin.Text = ResOpt[30];
            this.btnHiliteClsc.Text = ResOpt[0x1f];
            this.lblTabSizeTitle.Text = ResOpt[0x20];
            this.lblTabWidth.Text = ResOpt[0x21];
            this.lblTabHeight.Text = ResOpt[0x22];
            this.lblTabWFix.Text = ResOpt_DropDown[0x18];
            this.lblTabWMax.Text = ResOpt[0x23];
            this.lblTabWMin.Text = ResOpt[0x24];
            this.lblTabFont.Text = ResOpt[0x25];
            this.btnTabFont.Text = ResOpt[0x25];
            this.chbBoldActv.Text = ResOpt[0x26];
            this.lblTabTxtClr.Text = ResOpt[0x27];
            this.btnActTxtClr.Text = ResOpt[40];
            this.btnShadowAct.Text = ResOpt[40];
            this.btnInactTxtClr.Text = ResOpt[0x29];
            this.btnShadowIna.Text = ResOpt[0x29];
            this.btnDefTxtClr.Text = ResOpt[0x2a];
            this.chbToolbarBGClr.Text = this.btnToolBarBGClr.Text = ResOpt[0x2b];
            this.chbFolderIcon.Text = ResOpt[0x2c];
            this.lblUserApps_Path.Text = ResOpt[0x2d] + ":";
            this.lblUserApps_Args.Text = ResOpt[0x2e] + ":";
            this.lblUserApps_Working.Text = ResOpt[0x2f] + ":";
            this.lblUserApps_Key.Text = ResOpt_Genre[7] + ":";
            this.lblGroupKey.Text = ResOpt_Genre[7] + ":";
            this.lblPluginLang.Text = ResOpt[0x30];
            this.chbHideMenu.Text = ResOpt[0x31];
            this.chbBSUpOneLvl.Text = ResOpt[50];
            this.chbNoFulRowSelect.Text = ResOpt[0x33];
            this.chbGridLine.Text = ResOpt[0x34];
            this.chbAlternateColor.Text = ResOpt[0x35];
            this.chbShowPreview.Text = ResOpt[0x36];
            this.chbPreviewMode.Text = ResOpt[0x37];
            this.chbSubDirTip.Text = ResOpt[0x38];
            this.chbSubDirTipMode.Text = ResOpt[0x37];
            this.chbSubDirTipModeHidden.Text = ResOpt[0x39];
            this.chbSubDirTipPreview.Text = ResOpt[0x36];
            this.chbSubDirTipModeFile.Text = ResOpt[0x3a];
            this.chbSelectWithoutExt.Text = ResOpt[0x3b];
            this.chbSubDirTipModeSystem.Text = ResOpt[60];
            this.chbSendToTray.Text = ResOpt[0x3d];
            this.lblPreviewWidth.Text = ResOpt[0x3e];
            this.lblPreviewHeight.Text = ResOpt[0x3f];
            this.chbRebarBGImage.Text = ResOpt[0x40];
            this.chbF2Selection.Text = ResOpt[0x41];
            this.chbTabCloseButton.Text = ResOpt[0x42];
            this.lblTabWhlClk.Text = ResOpt[0x43];
            this.chbSubDirTipOnTab.Text = ResOpt[0x44];
            this.clmnHeader_NoCapture.Text = ResOpt[0x45];
            this.chbTabCloseBtnAlt.Text = ResOpt[70];
            this.chbTabCloseBtnHover.Text = ResOpt[0x47];
            this.btnExportSettings.Text = ResOpt[0x48];
            this.chbCursorLoop.Text = ResOpt[0x49];
            this.lblNetworkTimeOut.Text = ResOpt[0x4a];
            this.chbSendToTrayOnMinimize.Text = ResOpt[0x4b];
            this.btnPreviewFont.Text = ResOpt[0x25];
            this.lblTabTextAlignment.Text = ResOpt[0x4c];
            this.lblMenuRenderer.Text = ResOpt[0x4d];
            string[] strArray2 = QTUtility.TextResourcesDic["TabBar_Option2"];
            this.chbTreeShftWhlTab.Text = strArray2[0];
            this.chbTabSwitcher.Text = strArray2[1];
            this.chbTabTitleShadow.Text = strArray2[2];
            this.chbAutoUpdate.Text = strArray2[3];
            this.chbRemoveOnSeparate.Text = strArray2[4];
            this.chbDriveLetter.Text = strArray2[5];
            this.chbPlaySound.Text = strArray2[6];
            this.btnBrowsePlugin.Text = strArray2[7];
            PluginView.BTN_OPTION = strArray2[8];
            PluginView.BTN_DISABLE = strArray2[9];
            PluginView.BTN_ENABLE = strArray2[10];
            PluginView.BTN_REMOVE = strArray2[11];
            PluginView.MNU_PLUGINABOUT = strArray2[12];
            RES_REMOVEPLGIN = strArray2[13];
            this.chbPreviewInfo.Text = strArray2[14];
            string[] strArray3 = QTUtility.TextResourcesDic["TabBar_Option_Buttons"];
            this.btnHistoryClear.Text = this.btnClearRecentFile.Text = strArray3[0];
            this.btnUp_Grp.Text = strArray3[1];
            this.btnUp_app.Text = strArray3[1];
            this.btnDown_Grp.Text = strArray3[2];
            this.btnDown_app.Text = strArray3[2];
            this.btnAddSep_Grp.Text = strArray3[3];
            this.btnAddSep_app.Text = strArray3[3];
            this.btnStartUpGrp.Text = strArray3[4];
            this.btnAlternateColor.Text = strArray3[6];
            this.btnAlternateColor_Text.Text = strArray3[7];
            this.btnAlternate_Default.Text = strArray3[8];
            this.btnDefaultTextExt.Text = strArray3[8];
            this.btnDefaultImgExt.Text = strArray3[8];
            this.btnPreviewFontDefault.Text = strArray3[8];
            this.btnAddTextExt.Text = strArray3[9];
            this.btnAddImgExt.Text = strArray3[9];
            this.btnDelTextExt.Text = strArray3[10];
            this.btnDelImgExt.Text = strArray3[10];
            this.btnCheckUpdates.Text = strArray3[11];
            this.btnCopyKeys.Text = strArray3[12];
            this.cmbNavBtn.Items.AddRange(new string[] { ResOpt_DropDown[0], ResOpt_DropDown[1] });
            this.cmbNewTabLoc.Items.AddRange(new string[] { ResOpt_DropDown[2], ResOpt_DropDown[3], ResOpt_DropDown[1], ResOpt_DropDown[0] });
            this.cmbActvClose.Items.AddRange(new string[] { ResOpt_DropDown[1], ResOpt_DropDown[0], ResOpt_DropDown[2], ResOpt_DropDown[3], ResOpt_DropDown[4] });
            this.cmbTabDblClck.Items.AddRange(new string[] { ResOpt_DropDown[5], ResOpt_DropDown[6], ResOpt_DropDown[7], ResOpt_DropDown[8], ResOpt_DropDown[9], ResOpt_DropDown[10], ResOpt_DropDown[11], ResOpt_DropDown[12], ResOpt_DropDown[13] });
            this.cmbBGDblClick.Items.AddRange(new string[] { ResOpt_DropDown[14], ResOpt_DropDown[5], ResOpt_DropDown[15], ResOpt_DropDown[0x10], ResOpt_DropDown[8], ResOpt_DropDown[0x11], ResOpt_DropDown[0x12], ResOpt_DropDown[0x13], ResOpt_DropDown[20], ResOpt_DropDown[0x16], ResOpt_DropDown[9], ResOpt_DropDown[4], ResOpt_DropDown[13] });
            this.cmbTabWhlClck.Items.AddRange(new string[] { ResOpt_DropDown[6], ResOpt_DropDown[5], ResOpt_DropDown[7], ResOpt_DropDown[8], ResOpt_DropDown[9], ResOpt_DropDown[10], ResOpt_DropDown[11], ResOpt_DropDown[12], ResOpt_DropDown[13] });
            this.cmbMultiRow.Items.AddRange(new string[] { ResOpt_DropDown[13], ResOpt_DropDown[0x15] + "1", ResOpt_DropDown[0x15] + "2" });
            this.cmbWhlClick.Items.AddRange(new string[] { ResOpt_DropDown[0x16], ResOpt_DropDown[9] });
            this.cmbTabSizeMode.Items.AddRange(new string[] { ResOpt_DropDown[0x17], ResOpt_DropDown[0x18], ResOpt_DropDown[0x19] });
            this.cmbTabTextAlignment.Items.AddRange(new string[] { ResOpt_DropDown[0x1d], ResOpt_DropDown[30] });
            this.cmbTextExts.Items.Add("(Text file)");
            this.cmbTextExts.Items.AddRange(QTUtility.PreviewExtsList_Txt.ToArray());
            this.cmbTextExts.SelectedIndex = 0;
            this.cmbImgExts.Items.Add("(Image & movie file)");
            this.cmbImgExts.Items.AddRange(QTUtility.PreviewExtsList_Img.ToArray());
            this.cmbImgExts.SelectedIndex = 0;
            this.cmbRebarBGImageMode.Items.AddRange(new string[] { ResOpt_DropDown[0x1a], ResOpt_DropDown[0x1b], ResOpt_DropDown[0x1c], ResOpt_DropDown[0x21] });
            this.cmbMenuRenderer.Items.AddRange(new string[] { ResOpt[0x2a], ResOpt_DropDown[0x1f], ResOpt_DropDown[0x20] });
            if(arrSpecialFolderCSIDLs == null) {
                if(QTUtility.IsVista) {
                    arrSpecialFolderCSIDLs = new int[] { 
            0x11, 0, 10, 3, 0x12, 0x31, 4, 5, 0x27, 13, 14, 6, 0x1a, 40, 0x20, 0x21, 
            8, 0x22
           };
                }
                else {
                    arrSpecialFolderCSIDLs = new int[] { 0x11, 0, 10, 3, 0x12, 0x31, 4, 5, 0x27, 6, 0x1a, 40, 0x20, 0x21, 8, 0x22 };
                }
                arrSpecialFolderDipNms = new string[arrSpecialFolderCSIDLs.Length];
                for(int i = 0; i < arrSpecialFolderCSIDLs.Length; i++) {
                    arrSpecialFolderDipNms[i] = ShellMethods.GetSpecialFolderCLSID(arrSpecialFolderCSIDLs[i], true);
                }
            }
            this.cmbSpclFol_NoCapture.Items.AddRange(arrSpecialFolderDipNms);
            this.cmbSpclFol_NoCapture.SelectedIndex = 0;
            this.cmbSpclFol_Grp.Items.AddRange(arrSpecialFolderDipNms);
            this.cmbSpclFol_Grp.SelectedIndex = 0;
            using(RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Quizo\QTTabBar")) {
                if(key != null) {
                    int[] numArray = QTUtility2.ReadRegBinary<int>("OptionWindowBounds", key);
                    if((numArray != null) && (numArray.Length == 4)) {
                        base.StartPosition = FormStartPosition.Manual;
                        base.SetDesktopBounds(numArray[0], numArray[1], numArray[2], numArray[3]);
                    }
                }
            }
            this.SetValues();
            this.tabControl1.SelectedIndex = QTUtility.OptionsDialogTabIndex;
            base.ResumeLayout();
        }

        private void btnAdd_NoCapture_Click(object sender, EventArgs e) {
            this.listView_NoCapture.BeginUpdate();
            this.listView_NoCapture.SelectedItems.Clear();
            foreach(ListViewItem item in this.listView_NoCapture.Items) {
                if((item.Text.Length == 0) || (item.Text == "Enter path")) {
                    item.Selected = true;
                    item.BeginEdit();
                    this.listView_NoCapture.EndUpdate();
                    return;
                }
            }
            ListViewItem item2 = new ListViewItem("Enter path");
            this.listView_NoCapture.Items.Add(item2);
            this.listView_NoCapture.EndUpdate();
            item2.BeginEdit();
        }

        private void btnAddPreviewExt_Click(object sender, EventArgs e) {
            this.EnterExtension(sender == this.btnAddTextExt);
        }

        private void btnAddSep_app_Click(object sender, EventArgs e) {
            TreeNode selectedNode = this.treeViewUserApps.SelectedNode;
            TreeNode parent = this.tnRoot_UserApps;
            int count = this.tnRoot_UserApps.Nodes.Count;
            if((selectedNode != null) && (selectedNode.Level > 0)) {
                if(selectedNode.Tag != null) {
                    count = selectedNode.Index + 1;
                    parent = selectedNode.Parent;
                }
                else {
                    count = selectedNode.Nodes.Count;
                    parent = selectedNode;
                }
            }
            TreeNode node = new TreeNode("----------- Separator -----------");
            node.ImageKey = node.SelectedImageKey = "noimage";
            node.Tag = new MenuItemArguments(string.Empty, string.Empty, string.Empty, 0, MenuGenre.Application);
            node.ForeColor = SystemColors.GrayText;
            parent.Nodes.Insert(count, node);
            this.treeViewUserApps.SelectedNode = node;
        }

        private void btnAddSep_Click(object sender, EventArgs e) {
            TreeNode selectedNode = this.treeViewGroup.SelectedNode;
            if((selectedNode != null) && (selectedNode.Level != 2)) {
                TreeNode node = new TreeNode("----------- Separator -----------");
                node.Tag = MIA_GROUPSEP;
                node.ForeColor = SystemColors.GrayText;
                node.ImageKey = node.SelectedImageKey = "noimage";
                this.treeViewGroup.BeginUpdate();
                if(selectedNode.Level == 1) {
                    this.tnGroupsRoot.Nodes.Insert(selectedNode.Index + 1, node);
                }
                else if(selectedNode.Level == 0) {
                    this.tnGroupsRoot.Nodes.Add(node);
                }
                this.treeViewGroup.EndUpdate();
                node.EnsureVisible();
            }
        }

        private void btnAddSpcFol_Grp_Click(object sender, EventArgs e) {
            TreeNode selectedNode = this.treeViewGroup.SelectedNode;
            if(selectedNode != null) {
                string selectedItem = (string)this.cmbSpclFol_Grp.SelectedItem;
                int selectedIndex = this.cmbSpclFol_Grp.SelectedIndex;
                string specialFolderCLSID = ShellMethods.GetSpecialFolderCLSID(arrSpecialFolderCSIDLs[selectedIndex], false);
                if((selectedIndex == 3) && QTUtility.IsVista) {
                    specialFolderCLSID = "::{26EE0668-A00A-44D7-9371-BEB064C98683}";
                }
                TreeNode node = new TreeNode(selectedItem);
                node.Name = specialFolderCLSID;
                node.ToolTipText = selectedItem;
                node.ImageKey = node.SelectedImageKey = QTUtility.GetImageKey(specialFolderCLSID, null);
                if(selectedNode.Level == 0) {
                    selectedItem = CreateUniqueName(selectedItem, null, selectedNode);
                    TreeNode node3 = new TreeNode(selectedItem);
                    node3.ImageKey = node3.SelectedImageKey = node.ImageKey;
                    node3.Nodes.Add(node);
                    node3.Tag = new MenuItemArguments(selectedItem, null, null, 0, MenuGenre.Group);
                    this.tnGroupsRoot.Nodes.Add(node3);
                    this.treeViewGroup.SelectedNode = node3;
                    node.Expand();
                    node.BeginEdit();
                }
                else {
                    TreeNode node4 = (selectedNode.Level == 1) ? selectedNode : selectedNode.Parent;
                    node4.Nodes.Add(node);
                    node4.Expand();
                    if(node.Index == 0) {
                        node4.ImageKey = node4.SelectedImageKey = node.ImageKey;
                    }
                }
            }
        }

        private void btnAddSpcFol_NoCapture_Click(object sender, EventArgs e) {
            this.listView_NoCapture.BeginUpdate();
            this.listView_NoCapture.SelectedItems.Clear();
            string selectedItem = (string)this.cmbSpclFol_NoCapture.SelectedItem;
            int selectedIndex = this.cmbSpclFol_NoCapture.SelectedIndex;
            string specialFolderCLSID = ShellMethods.GetSpecialFolderCLSID(arrSpecialFolderCSIDLs[selectedIndex], false);
            if((selectedIndex == 3) && QTUtility.IsVista) {
                specialFolderCLSID = "::{26EE0668-A00A-44D7-9371-BEB064C98683}";
            }
            foreach(ListViewItem item in this.listView_NoCapture.Items) {
                if(((item.Text.Length == 0) || (item.Text == "Enter path")) || (item.Text == selectedItem)) {
                    if(item.Text != selectedItem) {
                        item.Text = selectedItem;
                        item.Name = specialFolderCLSID;
                    }
                    item.Selected = true;
                    this.listView_NoCapture.EndUpdate();
                    return;
                }
            }
            ListViewItem item2 = new ListViewItem(selectedItem);
            item2.Name = specialFolderCLSID;
            this.listView_NoCapture.Items.Add(item2);
            item2.Selected = true;
            this.listView_NoCapture.EndUpdate();
        }

        private void btnAddToken_Arg_Click(object sender, EventArgs e) {
            this.CreateTokenMenu();
            this.cmsAddToken.Items[0].Enabled = true;
            Rectangle rectangle = this.tabPage6_Apps.RectangleToScreen(this.btnAddToken_Arg.Bounds);
            this.cmsAddToken.Show(rectangle.Right, rectangle.Top);
        }

        private void btnAddToken_Wrk_Click(object sender, EventArgs e) {
            this.CreateTokenMenu();
            this.cmsAddToken.Items[0].Enabled = false;
            Rectangle rectangle = this.tabPage6_Apps.RectangleToScreen(this.btnAddToken_Wrk.Bounds);
            this.cmsAddToken.Show(rectangle.Right, rectangle.Top);
        }

        private void btnAddVirtualFolder_app_Click(object sender, EventArgs e) {
            TreeNode selectedNode = this.treeViewUserApps.SelectedNode;
            TreeNode tnParent = this.tnRoot_UserApps;
            int count = this.tnRoot_UserApps.Nodes.Count;
            if((selectedNode != null) && (selectedNode.Level > 0)) {
                if(selectedNode.Tag != null) {
                    count = selectedNode.Index + 1;
                    tnParent = selectedNode.Parent;
                }
                else {
                    count = selectedNode.Nodes.Count;
                    tnParent = selectedNode;
                }
            }
            TreeNode node = new TreeNode(CreateUniqueName("New Folder", null, tnParent));
            node.ImageKey = node.SelectedImageKey = "folder";
            node.Tag = null;
            tnParent.Nodes.Insert(count, node);
            this.treeViewUserApps.SelectedNode = node;
            node.BeginEdit();
        }

        private void btnAlternateColor_Click(object sender, EventArgs e) {
            if(sender == this.btnAlternate_Default) {
                QTUtility.ShellViewRowCOLORREF_Background = 0xfaf5f1;
                System.Drawing.Color windowText = SystemColors.WindowText;
                QTUtility.ShellViewRowCOLORREF_Text = QTUtility2.MakeCOLORREF(windowText);
                this.btnAlternateColor.BackColor = QTUtility2.MakeColor(QTUtility.ShellViewRowCOLORREF_Background);
                this.btnAlternateColor_Text.ForeColor = windowText;
            }
            else {
                bool flag = sender == this.btnAlternateColor;
                using(ColorDialogEx ex = new ColorDialogEx()) {
                    ex.Color = flag ? this.btnAlternateColor.BackColor : this.btnAlternateColor_Text.ForeColor;
                    if(DialogResult.OK == ex.ShowDialog()) {
                        if(flag) {
                            this.btnAlternateColor.BackColor = ex.Color;
                        }
                        else {
                            this.btnAlternateColor_Text.ForeColor = ex.Color;
                        }
                    }
                }
            }
        }

        private void btnBFD_app_Click(object sender, EventArgs e) {
            TreeNode selectedNode = this.treeViewUserApps.SelectedNode;
            if((selectedNode != null) && (selectedNode.Tag != null)) {
                using(FolderBrowserDialog dialog = new FolderBrowserDialog()) {
                    string path = ((MenuItemArguments)selectedNode.Tag).Path;
                    if((path.Length > 2) && !path.StartsWith(@"\\")) {
                        if(Directory.Exists(path)) {
                            if(path.Length > 3) {
                                path = path.TrimEnd(new char[] { '\\' });
                            }
                        }
                        else if(File.Exists(path)) {
                            path = Path.GetDirectoryName(path);
                        }
                        dialog.SelectedPath = path;
                    }
                    if(dialog.ShowDialog() == DialogResult.OK) {
                        this.tbPath.Text = dialog.SelectedPath;
                        this.tbArgs.Text = this.tbWorking.Text = string.Empty;
                        if(selectedNode.Text.StartsWith("New Item")) {
                            if(dialog.SelectedPath.Length == 3) {
                                selectedNode.Text = dialog.SelectedPath;
                            }
                            else {
                                selectedNode.Text = Path.GetFileName(dialog.SelectedPath);
                            }
                        }
                    }
                }
            }
        }

        private void btnBrowseAction_Click(object sender, EventArgs e) {
            using(FolderBrowserDialog dialog = new FolderBrowserDialog()) {
                dialog.SelectedPath = this.textBoxAction_BarDblClck.Text;
                if(DialogResult.OK == dialog.ShowDialog()) {
                    this.textBoxAction_BarDblClck.Text = dialog.SelectedPath;
                }
            }
        }

        private void btnBrowsePlugin_Click(object sender, EventArgs e) {
            using(OpenFileDialog dialog = new OpenFileDialog()) {
                dialog.Filter = "Plugin files (*.dll)|*.dll";
                dialog.RestoreDirectory = true;
                dialog.Multiselect = true;
                if(DialogResult.OK == dialog.ShowDialog()) {
                    bool flag = true;
                    foreach(string str in dialog.FileNames) {
                        PluginAssembly assembly = new PluginAssembly(str);
                        if(assembly.PluginInfosExist) {
                            this.CreatePluginViewItem(new PluginAssembly[] { assembly }, true);
                            if(flag) {
                                flag = false;
                                this.SelectPluginBottom();
                            }
                        }
                    }
                }
            }
        }

        private void btnBrowsePluginLang_Click(object sender, EventArgs e) {
            using(OpenFileDialog dialog = new OpenFileDialog()) {
                dialog.Filter = "Language XML files (*.xml)|*.xml";
                dialog.RestoreDirectory = true;
                if(this.textBoxPluginLang.Text.Length > 0) {
                    dialog.FileName = this.textBoxPluginLang.Text;
                }
                if(dialog.ShowDialog() == DialogResult.OK) {
                    this.textBoxPluginLang.Text = dialog.FileName;
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            if(this.callBack != null) {
                this.callBack.BeginInvoke(DialogResult.Cancel, null, null);
                this.callBack = null;
            }
            base.Close();
        }

        private void btnCheckUpdates_Click(object sender, EventArgs e) {
            UpdateChecker.Check(true);
        }

        private void btnClearRecentFile_Click(object sender, EventArgs e) {
            QTUtility.ExecutedPathsList.Clear();
            try {
                using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar")) {
                    key.DeleteSubKeyTree("RecentFiles");
                }
            }
            catch {
            }
        }

        private void btnCopyKeys_Click(object sender, EventArgs e) {
            string str = string.Empty;
            string str2 = "\t\t";
            for(int i = 0; i < this.listViewKeyboard.Items.Count; i++) {
                ListViewItem item = this.listViewKeyboard.Items[i];
                Keys tag = (Keys)item.Tag;
                if(item.Checked && (tag != Keys.None)) {
                    if(item.Group.Name == "general") {
                        string str3 = str;
                        str = str3 + QTUtility2.MakeKeyString(tag) + str2 + item.Text + "\n";
                    }
                    else {
                        string str4 = str;
                        str = str4 + QTUtility2.MakeKeyString(tag) + str2 + item.Text + "  -  " + item.Group.Header + "\n";
                    }
                }
            }
            foreach(int num2 in QTUtility.dicUserAppShortcutKeys.Keys) {
                if((num2 & 0x100000) != 0) {
                    MenuItemArguments arguments = QTUtility.dicUserAppShortcutKeys[num2];
                    string str5 = str;
                    str = str5 + QTUtility2.MakeKeyString(((Keys)num2) & ((Keys)(-1048577))) + str2 + "\"" + arguments.Path + "\"";
                    if(!string.IsNullOrEmpty(arguments.Argument)) {
                        str = str + ", " + arguments.Argument;
                    }
                    str = str + "\n";
                }
            }
            QTTabBarClass.SetStringClipboard(str);
        }

        private void btnDefaultImgExt_Click(object sender, EventArgs e) {
            this.cmbImgExts.Items.Clear();
            this.cmbImgExts.Items.Add("(Image & movie file)");
            this.cmbImgExts.Items.AddRange(ThumbnailTooltipForm.MakeDefaultImgExts().ToArray());
            this.cmbImgExts.SelectedIndex = 0;
        }

        private void btnDefaultTextExt_Click(object sender, EventArgs e) {
            this.cmbTextExts.Items.Clear();
            this.cmbTextExts.Items.AddRange(new string[] { "(Text file)", ".txt", ".ini", ".inf", ".cs", ".log", ".js", ".vbs" });
            this.cmbTextExts.SelectedIndex = 0;
        }

        private void btnDelPreiviewExt_Click(object sender, EventArgs e) {
            System.Windows.Forms.ComboBox box = (sender == this.btnDelTextExt) ? this.cmbTextExts : this.cmbImgExts;
            int selectedIndex = box.SelectedIndex;
            if(selectedIndex > 0) {
                box.Items.RemoveAt(selectedIndex);
                if(selectedIndex < box.Items.Count) {
                    box.SelectedIndex = selectedIndex;
                }
                else {
                    box.SelectedIndex = 0;
                }
            }
        }

        private void btnExportSettings_Click(object sender, EventArgs e) {
            using(SaveFileDialog dialog = new SaveFileDialog()) {
                dialog.Filter = "Registry file (*.reg)|*.reg";
                dialog.RestoreDirectory = true;
                if(DialogResult.OK == dialog.ShowDialog()) {
                    string fileName = dialog.FileName;
                    try {
                        using(RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Quizo\QTTabBar", true)) {
                            if(key != null) {
                                key.DeleteSubKey("Cache", false);
                            }
                        }
                        ProcessStartInfo startInfo = new ProcessStartInfo(ShellMethods.GetFolderPath(0x24) + @"\regedit.exe");
                        startInfo.Arguments = "/e \"" + fileName + "\" HKEY_CURRENT_USER\\Software\\Quizo\\QTTabBar";
                        startInfo.ErrorDialog = true;
                        startInfo.ErrorDialogParentHandle = base.Handle;
                        Process.Start(startInfo);
                    }
                    catch {
                    }
                }
            }
        }

        private void btnFont_Click(object sender, EventArgs e) {
            try {
                using(FontDialog dialog = new FontDialog()) {
                    dialog.Font = this.btnTabFont.Font;
                    dialog.ShowEffects = false;
                    dialog.AllowVerticalFonts = false;
                    if(DialogResult.OK == dialog.ShowDialog()) {
                        this.btnTabFont.Font = dialog.Font;
                    }
                }
            }
            catch {
                SystemSounds.Hand.Play();
            }
        }

        private void btnLangBrowse_Click(object sender, EventArgs e) {
            using(OpenFileDialog dialog = new OpenFileDialog()) {
                dialog.Filter = "Language XML files (*.xml)|*.xml";
                dialog.RestoreDirectory = true;
                if(this.textBoxLang.Text.Length > 0) {
                    dialog.FileName = this.textBoxLang.Text;
                }
                if(dialog.ShowDialog() == DialogResult.OK) {
                    this.textBoxLang.Text = dialog.FileName;
                }
            }
        }

        private void btnMinus_app_Click(object sender, EventArgs e) {
            TreeNode selectedNode = this.treeViewUserApps.SelectedNode;
            if((selectedNode != null) && (selectedNode.Level > 0)) {
                selectedNode.Remove();
            }
        }

        private void btnMinus_Click(object sender, EventArgs e) {
            TreeNode selectedNode = this.treeViewGroup.SelectedNode;
            if((selectedNode != null) && (selectedNode.Level != 0)) {
                selectedNode.Remove();
            }
        }

        private void btnOFD_app_Click(object sender, EventArgs e) {
            TreeNode selectedNode = this.treeViewUserApps.SelectedNode;
            if((selectedNode != null) && (selectedNode.Tag != null)) {
                using(OpenFileDialog dialog = new OpenFileDialog()) {
                    dialog.RestoreDirectory = true;
                    dialog.DereferenceLinks = false;
                    string path = ((MenuItemArguments)selectedNode.Tag).Path;
                    if(((path.Length > 3) && !path.StartsWith(@"\\")) && (File.Exists(path) || Directory.Exists(path))) {
                        dialog.FileName = path;
                    }
                    if(dialog.ShowDialog() == DialogResult.OK) {
                        this.tbPath.Text = dialog.FileName;
                        if(this.tbWorking.Text.Length == 0) {
                            this.tbWorking.Text = Path.GetDirectoryName(dialog.FileName);
                        }
                        if(selectedNode.Text.StartsWith("New Item")) {
                            selectedNode.Text = Path.GetFileName(dialog.FileName);
                        }
                    }
                }
            }
        }

        private void btnOFD_NoCapture_Click(object sender, EventArgs e) {
            using(FolderBrowserDialog dialog = new FolderBrowserDialog()) {
                if(DialogResult.OK == dialog.ShowDialog()) {
                    this.listView_NoCapture.BeginUpdate();
                    this.listView_NoCapture.SelectedItems.Clear();
                    bool flag = false;
                    foreach(ListViewItem item in this.listView_NoCapture.Items) {
                        if(string.Equals(dialog.SelectedPath, item.Name, StringComparison.OrdinalIgnoreCase)) {
                            item.Selected = true;
                            flag = true;
                            break;
                        }
                        if(item.Text == "Enter path") {
                            item.Text = item.Name = dialog.SelectedPath;
                            item.Selected = true;
                            flag = true;
                            break;
                        }
                    }
                    if(!flag) {
                        ListViewItem item2 = new ListViewItem(dialog.SelectedPath);
                        item2.Name = dialog.SelectedPath;
                        this.listView_NoCapture.Items.Add(item2);
                    }
                    this.listView_NoCapture.EndUpdate();
                }
            }
        }

        private void btnPayPal_Click(object sender, EventArgs e) {
            try {
                Process.Start(Resources_String.PayPalURL);
            }
            catch {
            }
        }

        private void btnPlus_app_Click(object sender, EventArgs e) {
            TreeNode selectedNode = this.treeViewUserApps.SelectedNode;
            TreeNode tnParent = this.tnRoot_UserApps;
            int index = 0;
            if((selectedNode != null) && (selectedNode.Level > 0)) {
                if(selectedNode.Tag != null) {
                    index = selectedNode.Index + 1;
                    tnParent = selectedNode.Parent;
                }
                else {
                    tnParent = selectedNode;
                }
            }
            else {
                selectedNode = this.tnRoot_UserApps;
            }
            TreeNode node = new TreeNode(CreateUniqueName("New Item", null, tnParent));
            node.ImageKey = node.SelectedImageKey = "noimage";
            node.Tag = new MenuItemArguments(string.Empty, string.Empty, string.Empty, 0, MenuGenre.Application);
            if(selectedNode.Tag == null) {
                selectedNode.Nodes.Add(node);
            }
            else {
                tnParent.Nodes.Insert(index, node);
            }
            this.treeViewUserApps.SelectedNode = node;
            node.BeginEdit();
        }

        private void btnPlus_Click(object sender, EventArgs e) {
            TreeNode selectedNode = this.treeViewGroup.SelectedNode;
            if(selectedNode != null) {
                if((selectedNode.Level == 0) || (selectedNode.Tag == MIA_GROUPSEP)) {
                    int index = (selectedNode.Level == 0) ? this.tnGroupsRoot.Nodes.Count : (selectedNode.Index + 1);
                    string text = CreateUniqueName("NewGroup", null, this.tnGroupsRoot);
                    TreeNode node = new TreeNode(text);
                    node.Tag = new MenuItemArguments(text, null, null, 0, MenuGenre.Group);
                    this.tnGroupsRoot.Nodes.Insert(index, node);
                    this.treeViewGroup.SelectedNode = node;
                    node.BeginEdit();
                }
                else {
                    using(FolderBrowserDialog dialog = new FolderBrowserDialog()) {
                        if(dialog.ShowDialog() == DialogResult.OK) {
                            TreeNode parent;
                            string selectedPath = dialog.SelectedPath;
                            string str3 = QTUtility2.MakePathDisplayText(selectedPath, true);
                            TreeNode node3 = new TreeNode(str3);
                            node3.Name = selectedPath;
                            node3.ToolTipText = str3;
                            node3.ImageKey = node3.SelectedImageKey = QTUtility.GetImageKey(selectedPath, null);
                            if(selectedNode.Level == 1) {
                                parent = selectedNode;
                            }
                            else {
                                parent = selectedNode.Parent;
                            }
                            parent.Nodes.Add(node3);
                            parent.Expand();
                            if(node3.Index == 0) {
                                parent.ImageKey = parent.SelectedImageKey = node3.ImageKey;
                            }
                        }
                    }
                }
            }
        }

        private void btnPreviewFont_Click(object sender, EventArgs e) {
            if(sender == this.btnPreviewFont) {
                try {
                    using(FontDialog dialog = new FontDialog()) {
                        dialog.Font = this.btnPreviewFont.Font;
                        dialog.ShowEffects = false;
                        dialog.AllowVerticalFonts = false;
                        if(DialogResult.OK == dialog.ShowDialog()) {
                            this.btnPreviewFont.Font = dialog.Font;
                        }
                    }
                }
                catch {
                    SystemSounds.Hand.Play();
                }
            }
            else {
                this.btnPreviewFont.Font = null;
            }
        }

        private void btnRebarImage_Click(object sender, EventArgs e) {
            using(OpenFileDialog dialog = new OpenFileDialog()) {
                dialog.Filter = "Image Files(*.PNG;*.BMP;*.JPG;*.GIF)|*.PNG;*.BMP;*.JPG;*.GIF";
                dialog.FileName = this.tbRebarImagePath.Text;
                dialog.RestoreDirectory = true;
                dialog.Title = "Toolbar background image";
                if(DialogResult.OK == dialog.ShowDialog()) {
                    this.tbRebarImagePath.Text = dialog.FileName;
                }
            }
        }

        private void btnRemove_NoCapture_Click(object sender, EventArgs e) {
            int count = this.listView_NoCapture.SelectedItems.Count;
            if(count > 0) {
                this.listView_NoCapture.BeginUpdate();
                int index = this.listView_NoCapture.Items.IndexOf(this.listView_NoCapture.SelectedItems[0]);
                foreach(ListViewItem item in this.listView_NoCapture.SelectedItems) {
                    this.listView_NoCapture.Items.Remove(item);
                }
                if(count == 1) {
                    count = this.listView_NoCapture.Items.Count;
                    if(count > index) {
                        this.listView_NoCapture.Items[index].Selected = true;
                    }
                    else if(count > 0) {
                        this.listView_NoCapture.Items[count - 1].Selected = true;
                    }
                }
                this.listView_NoCapture.EndUpdate();
            }
        }

        private void btnShadowClrs_Click(object sender, EventArgs e) {
            using(ColorDialogEx ex = new ColorDialogEx()) {
                ex.Color = ((System.Windows.Forms.Button)sender).ForeColor;
                if(DialogResult.OK == ex.ShowDialog()) {
                    ((System.Windows.Forms.Button)sender).ForeColor = ex.Color;
                }
            }
        }

        private void btnStartUpGrp_Click(object sender, EventArgs e) {
            TreeNode selectedNode = this.treeViewGroup.SelectedNode;
            if((selectedNode != null) && (selectedNode.Level > 0)) {
                TreeNode parent;
                if(selectedNode.Level == 1) {
                    parent = selectedNode;
                }
                else {
                    parent = selectedNode.Parent;
                }
                if(parent.NodeFont == this.fntStartUpGroup) {
                    parent.NodeFont = this.treeViewGroup.Font;
                }
                else {
                    parent.NodeFont = this.fntStartUpGroup;
                }
            }
        }

        private void btnTabImage_Click(object sender, EventArgs e) {
            using(OpenFileDialog dialog = new OpenFileDialog()) {
                dialog.Filter = "Image Files(*.PNG;*.BMP;*.JPG;*.GIF)|*.PNG;*.BMP;*.JPG;*.GIF";
                dialog.FileName = this.tbTabImagePath.Text;
                dialog.RestoreDirectory = true;
                dialog.Title = "Tab image";
                if(DialogResult.OK == dialog.ShowDialog()) {
                    this.tbTabImagePath.Text = dialog.FileName;
                }
            }
        }

        private void btnUpDown_app_Click(object sender, EventArgs e) {
            bool flag = sender == this.btnUp_app;
            TreeNode selectedNode = this.treeViewUserApps.SelectedNode;
            if((selectedNode != null) && (selectedNode != this.tnRoot_UserApps)) {
                TreeNode parent = selectedNode.Parent;
                TreeNode node3 = flag ? selectedNode.PrevNode : selectedNode.NextNode;
                this.treeViewUserApps.BeginUpdate();
                if(node3 != null) {
                    selectedNode.Remove();
                    if(node3.Tag != null) {
                        parent.Nodes.Insert(flag ? node3.Index : (node3.Index + 1), selectedNode);
                    }
                    else {
                        node3.Nodes.Insert(flag ? node3.Nodes.Count : 0, selectedNode);
                    }
                }
                else if(selectedNode.Level > 1) {
                    TreeNode node4 = parent.Parent;
                    TreeNode node5 = flag ? parent.PrevNode : parent.NextNode;
                    selectedNode.Remove();
                    if(node5 != null) {
                        node4.Nodes.Insert(flag ? (node5.Index + 1) : node5.Index, selectedNode);
                    }
                    else {
                        node4.Nodes.Insert(flag ? parent.Index : (parent.Index + 1), selectedNode);
                    }
                }
                this.treeViewUserApps.SelectedNode = selectedNode;
                this.treeViewUserApps.EndUpdate();
                selectedNode.EnsureVisible();
            }
        }

        private void buttonActClr_Click(object sender, EventArgs e) {
            using(ColorDialogEx ex = new ColorDialogEx()) {
                ex.Color = this.btnActTxtClr.ForeColor;
                if(DialogResult.OK == ex.ShowDialog()) {
                    this.btnActTxtClr.ForeColor = ex.Color;
                }
            }
        }

        private void buttonApply_Click(object sender, EventArgs e) {
            this.Save(true);
        }

        private void buttonHistoryClear_Click(object sender, EventArgs e) {
            QTUtility.ClosedTabHistoryList.Clear();
            try {
                using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar")) {
                    key.DeleteSubKeyTree("RecentlyClosed");
                }
            }
            catch {
            }
        }

        private void buttonHL_Click(object sender, EventArgs e) {
            using(ColorDialogEx ex = new ColorDialogEx()) {
                ex.Color = this.btnHiliteClsc.ForeColor;
                if(DialogResult.OK == ex.ShowDialog()) {
                    this.btnHiliteClsc.ForeColor = ex.Color;
                }
            }
        }

        private void buttonInactClr_Click(object sender, EventArgs e) {
            using(ColorDialogEx ex = new ColorDialogEx()) {
                ex.Color = this.btnInactTxtClr.ForeColor;
                if(DialogResult.OK == ex.ShowDialog()) {
                    this.btnInactTxtClr.ForeColor = ex.Color;
                }
            }
        }

        private void buttonOK_Click(object sender, EventArgs e) {
            this.Save(false);
        }

        private void buttonRstClr_Click(object sender, EventArgs e) {
            this.btnActTxtClr.ForeColor = this.btnInactTxtClr.ForeColor = SystemColors.ControlText;
            this.btnHiliteClsc.ForeColor = SystemColors.Highlight;
            this.btnTabFont.Font = null;
        }

        private void buttonToolBarBGClr_Click(object sender, EventArgs e) {
            using(ColorDialogEx ex = new ColorDialogEx()) {
                ex.Color = this.btnToolBarBGClr.BackColor;
                if(DialogResult.OK == ex.ShowDialog()) {
                    this.btnToolBarBGClr.BackColor = ex.Color;
                }
            }
        }

        private void chbAlternateColor_CheckedChanged(object sender, EventArgs e) {
            this.btnAlternate_Default.Enabled = this.btnAlternateColor.Enabled = this.btnAlternateColor_Text.Enabled = this.chbAlternateColor.Checked;
        }

        private void chbDrawMode_CheckedChanged(object sender, EventArgs e) {
            this.propertyGrid1.Enabled = this.btnHiliteClsc.Enabled = this.tbTabImagePath.Enabled = this.btnTabImage.Enabled = this.chbUseTabSkin.Checked;
        }

        private void chbFolderIcon_CheckedChanged(object sender, EventArgs e) {
            this.chbDriveLetter.Enabled = this.chbSubDirTipOnTab.Enabled = this.chbFolderIcon.Checked;
        }

        private void chbGroupKey_CheckedChanged(object sender, EventArgs e) {
            if(!this.fSuppressTextChangeEvent_Group) {
                TreeNode selectedNode = this.treeViewGroup.SelectedNode;
                if(((selectedNode != null) && (selectedNode.Tag != null)) && (selectedNode.Tag is MenuItemArguments)) {
                    MenuItemArguments tag = (MenuItemArguments)selectedNode.Tag;
                    if(this.chbGroupKey.Checked) {
                        tag.KeyShortcut |= 0x100000;
                    }
                    else {
                        tag.KeyShortcut &= -1048577;
                    }
                    this.tbGroupKey.Enabled = this.chbGroupKey.Checked;
                }
            }
        }

        private void chbMMButton_CheckedChanged(object sender, EventArgs e) {
            this.cmbWhlClick.Enabled = this.chbWhlClick.Checked;
        }

        private void chbNavBtn_CheckedChanged(object sender, EventArgs e) {
            this.cmbNavBtn.Enabled = this.chbNavBtn.Checked;
        }

        private void chbRebarBGImage_CheckedChanged(object sender, EventArgs e) {
            this.cmbRebarBGImageMode.Enabled = this.btnRebarImage.Enabled = this.tbRebarImagePath.Enabled = this.chbRebarBGImage.Checked;
        }

        private void chbsCloseWindow_CheckedChanged(object sender, EventArgs e) {
            if(sender == this.chbNeverCloseWndLocked) {
                if(this.chbNeverCloseWndLocked.Checked) {
                    this.chbRestoreLocked.Enabled = this.chbRestoreLocked.Checked = false;
                }
                else {
                    this.chbRestoreLocked.Enabled = true;
                }
            }
            else if(sender == this.chbRestoreLocked) {
                if(this.chbRestoreLocked.Checked) {
                    this.chbRestoreClosed.Checked = true;
                }
            }
            else if((sender == this.chbRestoreClosed) && !this.chbRestoreClosed.Checked) {
                this.chbRestoreLocked.Checked = false;
            }
        }

        private void chbShowPreviewTooltip_CheckedChanged(object sender, EventArgs e) {
            this.tabPage9_Misc.SuspendLayout();
            this.nudPreviewMaxWidth.Enabled = this.nudPreviewMaxHeight.Enabled = this.cmbTextExts.Enabled = this.btnAddTextExt.Enabled = this.btnDelTextExt.Enabled = this.btnDefaultTextExt.Enabled = this.cmbImgExts.Enabled = this.btnAddImgExt.Enabled = this.btnDelImgExt.Enabled = this.btnDefaultImgExt.Enabled = this.btnPreviewFont.Enabled = this.btnPreviewFontDefault.Enabled = this.chbPreviewInfo.Enabled = this.chbPreviewMode.Enabled = this.chbShowPreview.Checked;
            this.tabPage9_Misc.ResumeLayout();
        }

        private void chbSubDirTip_CheckedChanged(object sender, EventArgs e) {
            this.chbSubDirTipMode.Enabled = this.chbSubDirTipModeFile.Enabled = this.chbSubDirTipModeHidden.Enabled = this.chbSubDirTipPreview.Enabled = this.chbSubDirTipModeSystem.Enabled = this.chbSubDirTip.Checked;
        }

        private void chbTabCloseBtns_CheckedChanged(object sender, EventArgs e) {
            if((sender == this.chbTabCloseBtnHover) && this.chbTabCloseBtnHover.Checked) {
                this.chbTabCloseBtnAlt.Checked = false;
            }
            else if((sender == this.chbTabCloseBtnAlt) && this.chbTabCloseBtnAlt.Checked) {
                this.chbTabCloseBtnHover.Checked = false;
            }
        }

        private void chbTabCloseButton_CheckedChanged(object sender, EventArgs e) {
            this.chbTabCloseBtnAlt.Enabled = this.chbTabCloseBtnHover.Enabled = this.chbTabCloseButton.Checked;
        }

        private void chbTabTitleShadow_CheckedChanged(object sender, EventArgs e) {
            this.btnShadowAct.Enabled = this.btnShadowIna.Enabled = this.chbTabTitleShadow.Checked;
        }

        private void chbToolbarBGClr_CheckedChanged(object sender, EventArgs e) {
            this.btnToolBarBGClr.Enabled = this.chbToolbarBGClr.Checked;
        }

        private void chbUserAppKey_CheckedChanged(object sender, EventArgs e) {
            if(!this.fSuppressTextChangeEvent_UserApps) {
                TreeNode selectedNode = this.treeViewUserApps.SelectedNode;
                if(((selectedNode != null) && (selectedNode.Tag != null)) && (selectedNode.Tag is MenuItemArguments)) {
                    MenuItemArguments tag = (MenuItemArguments)selectedNode.Tag;
                    if(this.chbUserAppKey.Checked) {
                        tag.KeyShortcut |= 0x100000;
                    }
                    else {
                        tag.KeyShortcut &= -1048577;
                    }
                    this.tbUserAppKey.Enabled = this.chbUserAppKey.Checked;
                }
            }
        }

        private bool CheckExistance_GroupKey(Keys keys, TreeNode tnCurrent) {
            foreach(TreeNode node in this.tnGroupsRoot.Nodes) {
                if(((node == tnCurrent) || (node.Tag == null)) || !(node.Tag is MenuItemArguments)) {
                    continue;
                }
                MenuItemArguments tag = (MenuItemArguments)node.Tag;
                int num = tag.KeyShortcut & -1048577;
                if(num == (int)keys) {
                    string[] strArray = QTUtility.TextResourcesDic["ShortcutKeys_MsgReassign"];
                    string text = string.Format(strArray[0], QTUtility2.MakeKeyString(keys), node.Text);
                    if(DialogResult.OK == MessageBox.Show(text, strArray[1], MessageBoxButtons.OKCancel, MessageBoxIcon.Question)) {
                        tag.KeyShortcut = 0;
                        if(node == this.treeViewGroup.SelectedNode) {
                            this.chbGroupKey.Checked = false;
                            this.tbGroupKey.Text = " - ";
                        }
                        return true;
                    }
                    return false;
                }
            }
            return true;
        }

        private bool CheckExistance_Shortcuts(Keys keys, ListViewItem lviCurrent) {
            foreach(ListViewItem item in this.listViewKeyboard.Items) {
                if((keys == ((Keys)item.Tag)) && (item != lviCurrent)) {
                    string[] strArray = QTUtility.TextResourcesDic["ShortcutKeys_MsgReassign"];
                    string text = string.Format(strArray[0], QTUtility2.MakeKeyString(keys), item.SubItems[0].Text);
                    if(DialogResult.OK == MessageBox.Show(text, strArray[1], MessageBoxButtons.OKCancel, MessageBoxIcon.Question)) {
                        item.Checked = false;
                        item.SubItems[1].Text = " - ";
                        item.Tag = Keys.None;
                        return true;
                    }
                    return false;
                }
            }
            return true;
        }

        private bool CheckExistance_UserAppKey(Keys keys, TreeNode tnRoot, TreeNode tnCurrent) {
            foreach(TreeNode node in tnRoot.Nodes) {
                if(((node != tnCurrent) && (node.Tag != null)) && (node.Tag is MenuItemArguments)) {
                    MenuItemArguments tag = (MenuItemArguments)node.Tag;
                    int num = tag.KeyShortcut & -1048577;
                    if(num == (int)keys) {
                        string[] strArray = QTUtility.TextResourcesDic["ShortcutKeys_MsgReassign"];
                        string text = string.Format(strArray[0], QTUtility2.MakeKeyString(keys), node.Text);
                        if(DialogResult.OK == MessageBox.Show(text, strArray[1], MessageBoxButtons.OKCancel, MessageBoxIcon.Question)) {
                            tag.KeyShortcut = 0;
                            if(node == this.treeViewUserApps.SelectedNode) {
                                this.chbUserAppKey.Checked = false;
                                this.tbUserAppKey.Text = " - ";
                            }
                            return true;
                        }
                        return false;
                    }
                }
                if(!this.CheckExistance_UserAppKey(keys, node, tnCurrent)) {
                    return false;
                }
            }
            return true;
        }

        private void cmsAddToken_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            if(this.cmsAddToken.Items[0].Enabled) {
                this.tbArgs.Paste(e.ClickedItem.Name);
            }
            else {
                this.tbWorking.Paste(e.ClickedItem.Name);
            }
        }

        private void comboBoxes_SelectedIndexChanged(object sender, EventArgs e) {
            if(sender == this.cmbBGDblClick) {
                this.lblAction_BarDblClick.Enabled = this.textBoxAction_BarDblClck.Enabled = this.btnBrowseAction_BarDblClck.Enabled = (this.cmbBGDblClick.SelectedIndex == 9) || (this.cmbBGDblClick.SelectedIndex == 10);
            }
            else if(sender == this.cmbTabSizeMode) {
                this.nudTabWidth.Enabled = this.cmbTabSizeMode.SelectedIndex == 1;
                this.nudTabWidthMax.Enabled = this.nudTabWidthMin.Enabled = this.cmbTabSizeMode.SelectedIndex == 2;
            }
            else if(sender == this.cmbTextExts) {
                this.iComboBoxTextPreview = this.cmbTextExts.SelectedIndex;
            }
            else if(sender == this.cmbImgExts) {
                this.iConboBoxImagPreview = this.cmbImgExts.SelectedIndex;
            }
        }

        private void comboBoxPreviewExts_KeyPress(object sender, KeyPressEventArgs e) {
            if(e.KeyChar == '\r') {
                e.Handled = true;
                this.EnterExtension(sender == this.cmbTextExts);
            }
            else {
                System.Windows.Forms.ComboBox box = (System.Windows.Forms.ComboBox)sender;
                if((box.SelectedIndex == 0) && (box.Text == ((sender == this.cmbTextExts) ? "(Text file)" : "(Image & movie file)"))) {
                    box.Text = string.Empty;
                }
            }
        }

        private void CreateNoCapturePaths() {
            this.listView_NoCapture.BeginUpdate();
            foreach(string str in QTUtility.NoCapturePathsList) {
                ListViewItem item = new ListViewItem(str);
                if(str.StartsWith("::")) {
                    string displayName = ShellMethods.GetDisplayName(str);
                    if(!string.IsNullOrEmpty(displayName)) {
                        item.Text = displayName;
                    }
                }
                item.Name = str;
                this.listView_NoCapture.Items.Add(item);
            }
            this.listView_NoCapture.EndUpdate();
        }

        private void CreatePluginViewItem(PluginAssembly[] pluginAssemblies, bool fAddedByUser) {
            foreach(PluginAssembly assembly in pluginAssemblies) {
                if(assembly.PluginInfosExist) {
                    bool flag = false;
                    foreach(PluginViewItem item in this.pluginView.PluginViewItems) {
                        if(assembly.Path == item.PluginInfo.Path) {
                            flag = true;
                            break;
                        }
                    }
                    if(!flag) {
                        int num = 0;
                        foreach(PluginInformation information in assembly.PluginInformations) {
                            if(fAddedByUser || information.Enabled) {
                                information.Enabled = assembly.Enabled = true;
                            }
                            this.pluginView.AddPluginViewItem(information, assembly);
                            num++;
                        }
                        if((num > 0) && fAddedByUser) {
                            this.lstPluginAssembliesUserAdded.Add(assembly);
                        }
                    }
                }
            }
        }

        private void CreateShortcutItems() {
            string[] strArray = QTUtility.TextResourcesDic["ShortcutKeys_ActionNames"];
            string[] strArray2 = QTUtility.TextResourcesDic["ShortcutKeys_Groups"];
            ListViewGroup group = this.listViewKeyboard.Groups.Add("general", strArray2[0]);
            this.listViewKeyboard.BeginUpdate();
            for(int i = 0; i < QTUtility.ShortcutKeys.Length; i++) {
                bool flag = (QTUtility.ShortcutKeys[i] & 0x100000) == 0x100000;
                Keys key = ((Keys)QTUtility.ShortcutKeys[i]) & ((Keys)(-1048577));
                ListViewItem item = new ListViewItem(new string[] { strArray[i], QTUtility2.MakeKeyString(key) });
                item.Checked = flag;
                item.Group = group;
                item.Tag = key;
                this.listViewKeyboard.Items.Add(item);
            }
            foreach(string str in QTUtility.dicPluginShortcutKeys.Keys) {
                Plugin plugin;
                int[] numArray = QTUtility.dicPluginShortcutKeys[str];
                if(this.pluginManager.TryGetPlugin(str, out plugin) && (plugin.PluginInformation.ShortcutKeyActions != null)) {
                    ListViewGroup group2 = this.listViewKeyboard.Groups.Add(plugin.PluginInformation.PluginID, plugin.PluginInformation.Name + " (" + strArray2[1] + ")");
                    if((numArray != null) && (plugin.PluginInformation.ShortcutKeyActions.Length == numArray.Length)) {
                        for(int k = 0; k < numArray.Length; k++) {
                            bool flag2 = (numArray[k] & 0x100000) == 0x100000;
                            Keys keys2 = ((Keys)numArray[k]) & ((Keys)(-1048577));
                            ListViewItem item2 = new ListViewItem(new string[] { plugin.PluginInformation.ShortcutKeyActions[k], QTUtility2.MakeKeyString(keys2) });
                            item2.Checked = flag2;
                            item2.Group = group2;
                            item2.Tag = keys2;
                            item2.Name = str;
                            item2.ToolTipText = plugin.PluginInformation.Name + " ver:" + plugin.PluginInformation.Version;
                            this.listViewKeyboard.Items.Add(item2);
                        }
                        continue;
                    }
                    for(int j = 0; j < plugin.PluginInformation.ShortcutKeyActions.Length; j++) {
                        ListViewItem item3 = new ListViewItem(new string[] { plugin.PluginInformation.ShortcutKeyActions[j], " - " });
                        item3.Checked = false;
                        item3.Group = group2;
                        item3.Tag = 0;
                        item3.Name = str;
                        this.listViewKeyboard.Items.Add(item3);
                    }
                }
            }
            this.listViewKeyboard.EndUpdate();
        }

        private void CreateTokenMenu() {
            if(this.cmsAddToken.Items.Count == 0) {
                ToolStripMenuItem item = new ToolStripMenuItem("%f%   selected file");
                ToolStripMenuItem item2 = new ToolStripMenuItem("%d%   selected folder");
                ToolStripMenuItem item3 = new ToolStripMenuItem("%s%   selected item");
                ToolStripMenuItem item4 = new ToolStripMenuItem("%c%   current folder");
                ToolStripMenuItem item5 = new ToolStripMenuItem("%cd%  current or selected folder");
                item.Name = "%f%";
                item2.Name = "%d%";
                item3.Name = "%s%";
                item4.Name = "%c%";
                item5.Name = "%cd%";
                this.cmsAddToken.Items.AddRange(new ToolStripItem[] { item, item2, item3, item4, item5 });
            }
        }

        private static string CreateUniqueName(string strInitialName, TreeNode tnSelf, TreeNode tnParent) {
            int num = 1;
            string b = strInitialName;
            bool flag = false;
            do {
                flag = false;
                foreach(TreeNode node in tnParent.Nodes) {
                    if(((tnSelf == null) || (tnSelf != node)) && string.Equals(node.Text, b, StringComparison.OrdinalIgnoreCase)) {
                        b = strInitialName + "_" + ++num;
                        flag = true;
                        break;
                    }
                }
            }
            while(flag);
            return b;
        }

        private static TreeNode CreateUserAppNode(string key, string[] appVals) {
            int result = 0;
            if(appVals.Length == 4) {
                int.TryParse(appVals[3], out result);
            }
            MenuItemArguments arguments = new MenuItemArguments(appVals[0], appVals[1], appVals[2], result, MenuGenre.Application);
            TreeNode node = new TreeNode();
            node.Tag = arguments;
            if(((appVals[0] == null) || (appVals[0].Length == 0)) || (appVals[0] == "separator")) {
                node.Text = "----------- Separator -----------";
                node.ForeColor = SystemColors.GrayText;
                node.ImageKey = node.SelectedImageKey = "noimage";
                return node;
            }
            string name = appVals[0];
            try {
                name = Environment.ExpandEnvironmentVariables(name);
            }
            catch {
            }
            string ext = null;
            if(!name.StartsWith("::")) {
                ext = Directory.Exists(name) ? null : Path.GetExtension(name);
            }
            node.Text = key;
            node.ImageKey = node.SelectedImageKey = QTUtility.GetImageKey(name, ext);
            return node;
        }

        private static bool CreateUserAppNode_Sub(string displayName, RegistryKey rk, out TreeNode tn) {
            List<TreeNode> list = new List<TreeNode>();
            tn = null;
            if(rk != null) {
                foreach(string str in rk.GetValueNames()) {
                    string[] appVals = QTUtility2.ReadRegBinary<string>(str, rk);
                    if(appVals != null) {
                        if((appVals.Length == 3) || (appVals.Length == 4)) {
                            list.Add(CreateUserAppNode(str, appVals));
                        }
                    }
                    else {
                        using(RegistryKey key = rk.OpenSubKey(str)) {
                            TreeNode node;
                            if((key != null) && CreateUserAppNode_Sub(str, key, out node)) {
                                list.Add(node);
                            }
                        }
                    }
                }
            }
            if(list.Count > 0) {
                tn = new TreeNode(displayName);
                tn.ImageKey = tn.SelectedImageKey = "folder";
                tn.Nodes.AddRange(list.ToArray());
                return true;
            }
            return false;
        }

        private void DeletePluginAssembly(PluginAssembly pa) {
            foreach(PluginInformation information in pa.PluginInformations) {
                this.RemovePluginShortcutKeys(information.PluginID);
            }
            if(this.lstPluginAssembliesUserAdded.Contains(pa)) {
                pa.Dispose();
                this.lstPluginAssembliesUserAdded.Remove(pa);
            }
        }

        protected override void Dispose(bool disposing) {
            this.fntStartUpGroup.Dispose();
            this.sfPlugins.Dispose();
            this.cmsAddToken.Dispose();
            this.pluginManager = null;
            foreach(PluginAssembly assembly in this.lstPluginAssembliesUserAdded) {
                assembly.Dispose();
            }
            this.lstPluginAssembliesUserAdded.Clear();
            base.Dispose(disposing);
        }

        private void EnterExtension(bool fText) {
            System.Windows.Forms.ComboBox box = fText ? this.cmbTextExts : this.cmbImgExts;
            string str = fText ? "(Text file)" : "(Image & movie file)";
            int index = fText ? this.iComboBoxTextPreview : this.iConboBoxImagPreview;
            if(box.Text.Length > 0) {
                if((box.SelectedIndex != 0) || (box.Text != str)) {
                    string b = box.Text.ToLower();
                    if(!b.StartsWith(".")) {
                        b = "." + b;
                    }
                    foreach(string str3 in box.Items) {
                        if(string.Equals(str3, b, StringComparison.OrdinalIgnoreCase)) {
                            return;
                        }
                    }
                    box.Text = b;
                    if(index == 0) {
                        box.Items.Add(b);
                    }
                    else {
                        box.Items[index] = b;
                    }
                }
            }
            else if(index > 0) {
                box.Items.RemoveAt(index);
                box.SelectedIndex = 0;
            }
            else if(index == 0) {
                box.Text = str;
            }
        }

        private void InitializeComponent() {
            this.btnOK = new System.Windows.Forms.Button();
            this.btnApply = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblVer = new LinkLabel();
            this.tabControl1 = new TabControl();
            this.tabPage1_Gnrl = new TabPage();
            this.tabPage2_Tabs = new TabPage();
            this.tabPage3_Wndw = new TabPage();
            this.tabPage4_View = new TabPage();
            this.tabPage5_Grps = new TabPage();
            this.tabPage6_Apps = new TabPage();
            this.tabPage7_Plug = new TabPage();
            this.tabPage8_Keys = new TabPage();
            this.tabPage9_Misc = new TabPage();
            this.tabPageA_Path = new TabPage();
            this.chbActivateNew = new System.Windows.Forms.CheckBox();
            this.chbDontOpenSame = new System.Windows.Forms.CheckBox();
            this.chbCloseWhenGroup = new System.Windows.Forms.CheckBox();
            this.chbShowTooltip = new System.Windows.Forms.CheckBox();
            this.chbX1X2 = new System.Windows.Forms.CheckBox();
            this.chbNavBtn = new System.Windows.Forms.CheckBox();
            this.chbNoHistory = new System.Windows.Forms.CheckBox();
            this.chbSaveExecuted = new System.Windows.Forms.CheckBox();
            this.chbDD = new System.Windows.Forms.CheckBox();
            this.chbAutoUpdate = new System.Windows.Forms.CheckBox();
            this.chbPlaySound = new System.Windows.Forms.CheckBox();
            this.cmbNavBtn = new System.Windows.Forms.ComboBox();
            this.btnHistoryClear = new System.Windows.Forms.Button();
            this.btnClearRecentFile = new System.Windows.Forms.Button();
            this.nudMaxUndo = new NumericUpDown();
            this.nudMaxRecentFile = new NumericUpDown();
            this.lblLang = new Label();
            this.lblNetworkTimeOut = new Label();
            this.textBoxLang = new System.Windows.Forms.TextBox();
            this.btnLangBrowse = new System.Windows.Forms.Button();
            this.btnCheckUpdates = new System.Windows.Forms.Button();
            this.btnExportSettings = new System.Windows.Forms.Button();
            this.nudNetworkTimeOut = new NumericUpDown();
            this.lblNewTabLoc = new Label();
            this.lblActvClose = new Label();
            this.lblTabDblClk = new Label();
            this.lblBGDblClik = new Label();
            this.lblTabWhlClk = new Label();
            this.lblAction_BarDblClick = new Label();
            this.lblMultiRows = new Label();
            this.cmbNewTabLoc = new System.Windows.Forms.ComboBox();
            this.cmbActvClose = new System.Windows.Forms.ComboBox();
            this.cmbTabDblClck = new System.Windows.Forms.ComboBox();
            this.cmbBGDblClick = new System.Windows.Forms.ComboBox();
            this.cmbTabWhlClck = new System.Windows.Forms.ComboBox();
            this.cmbMultiRow = new System.Windows.Forms.ComboBox();
            this.textBoxAction_BarDblClck = new System.Windows.Forms.TextBox();
            this.btnBrowseAction_BarDblClck = new System.Windows.Forms.Button();
            this.chbAutoSubText = new System.Windows.Forms.CheckBox();
            this.chbTabCloseButton = new System.Windows.Forms.CheckBox();
            this.chbTabCloseBtnAlt = new System.Windows.Forms.CheckBox();
            this.chbTabCloseBtnHover = new System.Windows.Forms.CheckBox();
            this.chbSubDirTipOnTab = new System.Windows.Forms.CheckBox();
            this.chbTreeShftWhlTab = new System.Windows.Forms.CheckBox();
            this.chbTabSwitcher = new System.Windows.Forms.CheckBox();
            this.chbRemoveOnSeparate = new System.Windows.Forms.CheckBox();
            this.chbDriveLetter = new System.Windows.Forms.CheckBox();
            this.chbWhlClick = new System.Windows.Forms.CheckBox();
            this.chbNCADblClck = new System.Windows.Forms.CheckBox();
            this.chbBlockProcess = new System.Windows.Forms.CheckBox();
            this.chbFoldrTree = new System.Windows.Forms.CheckBox();
            this.chbWndUnresizable = new System.Windows.Forms.CheckBox();
            this.chbWndRestrAlpha = new System.Windows.Forms.CheckBox();
            this.chbNoTabFromOuteside = new System.Windows.Forms.CheckBox();
            this.chbHolizontalScroll = new System.Windows.Forms.CheckBox();
            this.chbWhlChangeView = new System.Windows.Forms.CheckBox();
            this.chbNeverCloseWindow = new System.Windows.Forms.CheckBox();
            this.chbNeverCloseWndLocked = new System.Windows.Forms.CheckBox();
            this.chbRestoreClosed = new System.Windows.Forms.CheckBox();
            this.chbRestoreLocked = new System.Windows.Forms.CheckBox();
            this.chbSendToTray = new System.Windows.Forms.CheckBox();
            this.chbSendToTrayOnMinimize = new System.Windows.Forms.CheckBox();
            this.cmbWhlClick = new System.Windows.Forms.ComboBox();
            this.lblSep = new Label();
            this.chbUseTabSkin = new System.Windows.Forms.CheckBox();
            this.chbToolbarBGClr = new System.Windows.Forms.CheckBox();
            this.chbFolderIcon = new System.Windows.Forms.CheckBox();
            this.chbBoldActv = new System.Windows.Forms.CheckBox();
            this.chbRebarBGImage = new System.Windows.Forms.CheckBox();
            this.chbTabTitleShadow = new System.Windows.Forms.CheckBox();
            this.propertyGrid1 = new PropertyGrid();
            this.nudTabWidth = new NumericUpDown();
            this.nudTabHeight = new NumericUpDown();
            this.nudTabWidthMax = new NumericUpDown();
            this.nudTabWidthMin = new NumericUpDown();
            this.lblTabSizeTitle = new Label();
            this.lblTabWidth = new Label();
            this.lblTabHeight = new Label();
            this.lblTabWMin = new Label();
            this.lblTabWMax = new Label();
            this.lblTabWFix = new Label();
            this.lblTabFont = new Label();
            this.lblMenuRenderer = new Label();
            this.lblTabTextAlignment = new Label();
            this.lblTabTxtClr = new Label();
            this.cmbTabSizeMode = new System.Windows.Forms.ComboBox();
            this.cmbTabTextAlignment = new System.Windows.Forms.ComboBox();
            this.cmbRebarBGImageMode = new System.Windows.Forms.ComboBox();
            this.cmbMenuRenderer = new System.Windows.Forms.ComboBox();
            this.btnHiliteClsc = new System.Windows.Forms.Button();
            this.btnTabFont = new System.Windows.Forms.Button();
            this.btnActTxtClr = new System.Windows.Forms.Button();
            this.btnInactTxtClr = new System.Windows.Forms.Button();
            this.btnDefTxtClr = new System.Windows.Forms.Button();
            this.btnToolBarBGClr = new System.Windows.Forms.Button();
            this.btnRebarImage = new System.Windows.Forms.Button();
            this.btnShadowAct = new System.Windows.Forms.Button();
            this.btnShadowIna = new System.Windows.Forms.Button();
            this.btnTabImage = new System.Windows.Forms.Button();
            this.tbRebarImagePath = new System.Windows.Forms.TextBox();
            this.tbTabImagePath = new System.Windows.Forms.TextBox();
            this.treeViewGroup = new System.Windows.Forms.TreeView();
            this.btnUp_Grp = new System.Windows.Forms.Button();
            this.btnDown_Grp = new System.Windows.Forms.Button();
            this.btnMinus_Grp = new System.Windows.Forms.Button();
            this.btnPlus_Grp = new System.Windows.Forms.Button();
            this.btnStartUpGrp = new System.Windows.Forms.Button();
            this.btnAddSep_Grp = new System.Windows.Forms.Button();
            this.cmbSpclFol_Grp = new System.Windows.Forms.ComboBox();
            this.btnAddSpcFol_Grp = new System.Windows.Forms.Button();
            this.lblGroupKey = new Label();
            this.tbGroupKey = new System.Windows.Forms.TextBox();
            this.chbGroupKey = new System.Windows.Forms.CheckBox();
            this.treeViewUserApps = new System.Windows.Forms.TreeView();
            this.btnUp_app = new System.Windows.Forms.Button();
            this.btnDown_app = new System.Windows.Forms.Button();
            this.btnAddSep_app = new System.Windows.Forms.Button();
            this.btnAddVFolder_app = new System.Windows.Forms.Button();
            this.btnPlus_app = new System.Windows.Forms.Button();
            this.btnMinus_app = new System.Windows.Forms.Button();
            this.lblUserApps_Path = new Label();
            this.lblUserApps_Args = new Label();
            this.lblUserApps_Working = new Label();
            this.tbPath = new System.Windows.Forms.TextBox();
            this.tbArgs = new System.Windows.Forms.TextBox();
            this.tbWorking = new System.Windows.Forms.TextBox();
            this.tbUserAppKey = new System.Windows.Forms.TextBox();
            this.chbUserAppKey = new System.Windows.Forms.CheckBox();
            this.lblUserApps_Key = new Label();
            this.btnOFD_app = new System.Windows.Forms.Button();
            this.btnBFD_app = new System.Windows.Forms.Button();
            this.btnAddToken_Arg = new System.Windows.Forms.Button();
            this.btnAddToken_Wrk = new System.Windows.Forms.Button();
            this.cmsAddToken = new ContextMenuStrip();
            this.chbHideMenu = new System.Windows.Forms.CheckBox();
            this.chbBSUpOneLvl = new System.Windows.Forms.CheckBox();
            this.chbNoFulRowSelect = new System.Windows.Forms.CheckBox();
            this.chbGridLine = new System.Windows.Forms.CheckBox();
            this.chbAlternateColor = new System.Windows.Forms.CheckBox();
            this.chbShowPreview = new System.Windows.Forms.CheckBox();
            this.chbPreviewMode = new System.Windows.Forms.CheckBox();
            this.chbPreviewInfo = new System.Windows.Forms.CheckBox();
            this.chbSubDirTip = new System.Windows.Forms.CheckBox();
            this.chbSubDirTipMode = new System.Windows.Forms.CheckBox();
            this.chbSubDirTipModeHidden = new System.Windows.Forms.CheckBox();
            this.chbSubDirTipModeSystem = new System.Windows.Forms.CheckBox();
            this.chbSubDirTipModeFile = new System.Windows.Forms.CheckBox();
            this.chbSubDirTipPreview = new System.Windows.Forms.CheckBox();
            this.chbSelectWithoutExt = new System.Windows.Forms.CheckBox();
            this.chbF2Selection = new System.Windows.Forms.CheckBox();
            this.chbCursorLoop = new System.Windows.Forms.CheckBox();
            this.btnAlternateColor = new System.Windows.Forms.Button();
            this.btnAlternateColor_Text = new System.Windows.Forms.Button();
            this.btnAlternate_Default = new System.Windows.Forms.Button();
            this.btnAddTextExt = new System.Windows.Forms.Button();
            this.btnDelTextExt = new System.Windows.Forms.Button();
            this.btnDefaultTextExt = new System.Windows.Forms.Button();
            this.btnAddImgExt = new System.Windows.Forms.Button();
            this.btnDelImgExt = new System.Windows.Forms.Button();
            this.btnDefaultImgExt = new System.Windows.Forms.Button();
            this.btnPreviewFont = new System.Windows.Forms.Button();
            this.btnPreviewFontDefault = new System.Windows.Forms.Button();
            this.btnPayPal = new System.Windows.Forms.Button();
            this.nudPreviewMaxHeight = new NumericUpDown();
            this.nudPreviewMaxWidth = new NumericUpDown();
            this.lblPreviewHeight = new Label();
            this.lblPreviewWidth = new Label();
            this.cmbTextExts = new System.Windows.Forms.ComboBox();
            this.cmbImgExts = new System.Windows.Forms.ComboBox();
            this.pluginView = new PluginView();
            this.btnBrowsePlugin = new System.Windows.Forms.Button();
            this.lblPluginLang = new Label();
            this.textBoxPluginLang = new System.Windows.Forms.TextBox();
            this.btnBrowsePluginLang = new System.Windows.Forms.Button();
            this.listViewKeyboard = new ListViewEx();
            this.clmKeys_Action = new ColumnHeader();
            this.clmKeys_Key = new ColumnHeader();
            this.btnCopyKeys = new System.Windows.Forms.Button();
            this.listView_NoCapture = new System.Windows.Forms.ListView();
            this.btnOFD_NoCapture = new System.Windows.Forms.Button();
            this.btnAdd_NoCapture = new System.Windows.Forms.Button();
            this.btnRemove_NoCapture = new System.Windows.Forms.Button();
            this.clmnHeader_NoCapture = new ColumnHeader();
            this.cmbSpclFol_NoCapture = new System.Windows.Forms.ComboBox();
            this.btnAddSpcFol_NoCapture = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPage1_Gnrl.SuspendLayout();
            this.tabPage2_Tabs.SuspendLayout();
            this.tabPage3_Wndw.SuspendLayout();
            this.tabPage4_View.SuspendLayout();
            this.tabPage5_Grps.SuspendLayout();
            this.tabPage6_Apps.SuspendLayout();
            this.tabPage7_Plug.SuspendLayout();
            this.tabPage8_Keys.SuspendLayout();
            this.tabPage9_Misc.SuspendLayout();
            this.tabPageA_Path.SuspendLayout();
            this.nudMaxUndo.BeginInit();
            this.nudMaxRecentFile.BeginInit();
            this.nudNetworkTimeOut.BeginInit();
            this.nudTabWidthMin.BeginInit();
            this.nudTabWidthMax.BeginInit();
            this.nudTabHeight.BeginInit();
            this.nudTabWidth.BeginInit();
            this.nudPreviewMaxWidth.BeginInit();
            this.nudPreviewMaxHeight.BeginInit();
            base.SuspendLayout();
            this.btnOK.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this.btnOK.Location = new Point(0xf1, 0x246);
            this.btnOK.Size = new Size(0x58, 0x17);
            this.btnOK.TabIndex = 4;
            this.btnOK.Click += new EventHandler(this.buttonOK_Click);
            this.btnApply.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this.btnApply.Location = new Point(0x1ad, 0x246);
            this.btnApply.Size = new Size(0x58, 0x17);
            this.btnApply.TabIndex = 6;
            this.btnApply.Click += new EventHandler(this.buttonApply_Click);
            this.btnCancel.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this.btnCancel.Location = new Point(0x14f, 0x246);
            this.btnCancel.Size = new Size(0x58, 0x17);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
            this.lblVer.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this.lblVer.AutoSize = true;
            this.lblVer.LinkColor = System.Drawing.Color.Blue;
            this.lblVer.ActiveLinkColor = System.Drawing.Color.Red;
            this.lblVer.VisitedLinkColor = System.Drawing.Color.Purple;
            this.lblVer.Location = new Point(12, 0x24b);
            this.lblVer.Click += new EventHandler(this.lblVer_Click);
            this.tabControl1.Controls.Add(this.tabPage1_Gnrl);
            this.tabControl1.Controls.Add(this.tabPage2_Tabs);
            this.tabControl1.Controls.Add(this.tabPage3_Wndw);
            this.tabControl1.Controls.Add(this.tabPage4_View);
            this.tabControl1.Controls.Add(this.tabPage5_Grps);
            this.tabControl1.Controls.Add(this.tabPage6_Apps);
            this.tabControl1.Controls.Add(this.tabPage7_Plug);
            this.tabControl1.Controls.Add(this.tabPage8_Keys);
            this.tabControl1.Controls.Add(this.tabPage9_Misc);
            this.tabControl1.Controls.Add(this.tabPageA_Path);
            this.tabControl1.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this.tabControl1.Location = new Point(9, 9);
            this.tabControl1.Margin = new Padding(0);
            this.tabControl1.Multiline = true;
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new Size(0x207, 0x238);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.SelectedIndexChanged += new EventHandler(this.tabControl1_SelectedIndexChanged);
            this.tabPage1_Gnrl.Controls.Add(this.chbActivateNew);
            this.tabPage1_Gnrl.Controls.Add(this.chbDontOpenSame);
            this.tabPage1_Gnrl.Controls.Add(this.chbCloseWhenGroup);
            this.tabPage1_Gnrl.Controls.Add(this.chbShowTooltip);
            this.tabPage1_Gnrl.Controls.Add(this.chbX1X2);
            this.tabPage1_Gnrl.Controls.Add(this.chbNavBtn);
            this.tabPage1_Gnrl.Controls.Add(this.chbNoHistory);
            this.tabPage1_Gnrl.Controls.Add(this.chbSaveExecuted);
            this.tabPage1_Gnrl.Controls.Add(this.chbDD);
            this.tabPage1_Gnrl.Controls.Add(this.chbPlaySound);
            this.tabPage1_Gnrl.Controls.Add(this.cmbNavBtn);
            this.tabPage1_Gnrl.Controls.Add(this.btnHistoryClear);
            this.tabPage1_Gnrl.Controls.Add(this.nudMaxUndo);
            this.tabPage1_Gnrl.Controls.Add(this.nudMaxRecentFile);
            this.tabPage1_Gnrl.Controls.Add(this.btnClearRecentFile);
            this.tabPage1_Gnrl.Controls.Add(this.lblLang);
            this.tabPage1_Gnrl.Controls.Add(this.textBoxLang);
            this.tabPage1_Gnrl.Controls.Add(this.btnLangBrowse);
            this.tabPage1_Gnrl.Controls.Add(this.btnExportSettings);
            this.tabPage1_Gnrl.Controls.Add(this.btnCheckUpdates);
            this.tabPage1_Gnrl.Controls.Add(this.lblNetworkTimeOut);
            this.tabPage1_Gnrl.Controls.Add(this.nudNetworkTimeOut);
            this.tabPage1_Gnrl.Controls.Add(this.chbAutoUpdate);
            this.tabPage1_Gnrl.Location = new Point(4, 0x16);
            this.tabPage1_Gnrl.Padding = new Padding(3);
            this.tabPage1_Gnrl.Size = new Size(0x1ff, 0x1d7);
            this.tabPage1_Gnrl.TabIndex = 0;
            this.tabPage1_Gnrl.UseVisualStyleBackColor = true;
            this.btnCheckUpdates.AutoSize = true;
            this.btnCheckUpdates.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            this.btnCheckUpdates.Location = new Point(0x12a, 0x1f1);
            this.btnCheckUpdates.TabIndex = 20;
            this.btnCheckUpdates.Click += new EventHandler(this.btnCheckUpdates_Click);
            this.chbAutoUpdate.AutoSize = true;
            this.chbAutoUpdate.Location = new Point(0x1b, 0x1f3);
            this.chbAutoUpdate.TabIndex = 0x13;
            this.btnExportSettings.AutoSize = true;
            this.btnExportSettings.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            this.btnExportSettings.Location = new Point(0x1b, 0x1c7);
            this.btnExportSettings.TabIndex = 0x12;
            this.btnExportSettings.Click += new EventHandler(this.btnExportSettings_Click);
            this.lblNetworkTimeOut.AutoSize = true;
            this.lblNetworkTimeOut.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            this.lblNetworkTimeOut.Location = new Point(0x1b, 0x1a5);
            this.nudNetworkTimeOut.Location = new Point(0x194, 420);
            int[] bits = new int[4];
            bits[0] = 60;
            this.nudNetworkTimeOut.Maximum = new decimal(bits);
            int[] numArray2 = new int[4];
            this.nudNetworkTimeOut.Minimum = new decimal(numArray2);
            this.nudNetworkTimeOut.Size = new Size(0x33, 0x15);
            this.nudNetworkTimeOut.TabIndex = 0x11;
            this.nudNetworkTimeOut.TextAlign = HorizontalAlignment.Right;
            int[] numArray3 = new int[4];
            numArray3[0] = 6;
            this.nudNetworkTimeOut.Value = new decimal(numArray3);
            this.btnLangBrowse.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this.btnLangBrowse.Location = new Point(0x1a7, 0x175);
            this.btnLangBrowse.Size = new Size(0x22, 0x19);
            this.btnLangBrowse.TabIndex = 0x10;
            this.btnLangBrowse.Text = "...";
            this.btnLangBrowse.Click += new EventHandler(this.btnLangBrowse_Click);
            this.textBoxLang.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.textBoxLang.Location = new Point(0x2d, 0x178);
            this.textBoxLang.Size = new Size(0x174, 0x15);
            this.textBoxLang.MaxLength = 260;
            this.textBoxLang.TabIndex = 15;
            this.textBoxLang.KeyPress += new KeyPressEventHandler(this.textBoxesPath_KeyPress);
            this.lblLang.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            this.lblLang.AutoSize = true;
            this.lblLang.Location = new Point(0x1b, 0x160);
            this.chbPlaySound.AutoSize = true;
            this.chbPlaySound.Location = new Point(0x1b, 0x111);
            this.chbPlaySound.TabIndex = 14;
            this.chbDD.AutoSize = true;
            this.chbDD.Location = new Point(0x1b, 0xf5);
            this.chbDD.TabIndex = 13;
            this.btnClearRecentFile.Location = new Point(0x163, 0xd7);
            this.btnClearRecentFile.Size = new Size(100, 0x17);
            this.btnClearRecentFile.TabIndex = 12;
            this.btnClearRecentFile.Text = "Clear";
            this.btnClearRecentFile.Click += new EventHandler(this.btnClearRecentFile_Click);
            this.nudMaxRecentFile.Location = new Point(0x12a, 0xd8);
            int[] numArray4 = new int[4];
            numArray4[0] = 0x40;
            this.nudMaxRecentFile.Maximum = new decimal(numArray4);
            int[] numArray5 = new int[4];
            numArray5[0] = 1;
            this.nudMaxRecentFile.Minimum = new decimal(numArray5);
            this.nudMaxRecentFile.Size = new Size(0x33, 0x15);
            this.nudMaxRecentFile.TabIndex = 11;
            this.nudMaxRecentFile.TextAlign = HorizontalAlignment.Right;
            int[] numArray6 = new int[4];
            numArray6[0] = 1;
            this.nudMaxRecentFile.Value = new decimal(numArray6);
            this.chbSaveExecuted.AutoSize = true;
            this.chbSaveExecuted.Location = new Point(0x1b, 0xd9);
            this.chbSaveExecuted.ThreeState = true;
            this.chbSaveExecuted.TabIndex = 10;
            this.btnHistoryClear.Location = new Point(0x163, 0xbb);
            this.btnHistoryClear.Size = new Size(100, 0x17);
            this.btnHistoryClear.TabIndex = 9;
            this.btnHistoryClear.Text = "Clear";
            this.btnHistoryClear.Click += new EventHandler(this.buttonHistoryClear_Click);
            this.nudMaxUndo.Location = new Point(0x12a, 0xbc);
            int[] numArray7 = new int[4];
            numArray7[0] = 0x40;
            this.nudMaxUndo.Maximum = new decimal(numArray7);
            int[] numArray8 = new int[4];
            numArray8[0] = 1;
            this.nudMaxUndo.Minimum = new decimal(numArray8);
            this.nudMaxUndo.Size = new Size(0x33, 0x15);
            this.nudMaxUndo.TabIndex = 8;
            this.nudMaxUndo.TextAlign = HorizontalAlignment.Right;
            int[] numArray9 = new int[4];
            numArray9[0] = 1;
            this.nudMaxUndo.Value = new decimal(numArray9);
            this.chbNoHistory.AutoSize = true;
            this.chbNoHistory.Location = new Point(0x1b, 0xbd);
            this.chbNoHistory.TabIndex = 7;
            this.cmbNavBtn.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbNavBtn.Location = new Point(0x176, 0x9f);
            this.cmbNavBtn.Size = new Size(0x51, 0x15);
            this.cmbNavBtn.TabIndex = 6;
            this.chbNavBtn.AutoSize = true;
            this.chbNavBtn.Location = new Point(0x1b, 0xa1);
            this.chbNavBtn.TabIndex = 5;
            this.chbNavBtn.CheckedChanged += new EventHandler(this.chbNavBtn_CheckedChanged);
            this.chbX1X2.AutoSize = true;
            this.chbX1X2.Location = new Point(0x1b, 0x85);
            this.chbX1X2.TabIndex = 4;
            this.chbShowTooltip.AutoSize = true;
            this.chbShowTooltip.Location = new Point(0x1b, 0x69);
            this.chbShowTooltip.TabIndex = 3;
            this.chbCloseWhenGroup.AutoSize = true;
            this.chbCloseWhenGroup.Location = new Point(0x1b, 0x4d);
            this.chbCloseWhenGroup.TabIndex = 2;
            this.chbDontOpenSame.AutoSize = true;
            this.chbDontOpenSame.Location = new Point(0x1b, 0x31);
            this.chbDontOpenSame.TabIndex = 1;
            this.chbActivateNew.AutoSize = true;
            this.chbActivateNew.Location = new Point(0x1b, 0x15);
            this.chbActivateNew.TabIndex = 0;
            this.tabPage2_Tabs.Controls.Add(this.lblNewTabLoc);
            this.tabPage2_Tabs.Controls.Add(this.lblActvClose);
            this.tabPage2_Tabs.Controls.Add(this.lblTabDblClk);
            this.tabPage2_Tabs.Controls.Add(this.lblBGDblClik);
            this.tabPage2_Tabs.Controls.Add(this.lblTabWhlClk);
            this.tabPage2_Tabs.Controls.Add(this.lblAction_BarDblClick);
            this.tabPage2_Tabs.Controls.Add(this.lblMultiRows);
            this.tabPage2_Tabs.Controls.Add(this.cmbNewTabLoc);
            this.tabPage2_Tabs.Controls.Add(this.cmbActvClose);
            this.tabPage2_Tabs.Controls.Add(this.cmbTabDblClck);
            this.tabPage2_Tabs.Controls.Add(this.cmbBGDblClick);
            this.tabPage2_Tabs.Controls.Add(this.cmbTabWhlClck);
            this.tabPage2_Tabs.Controls.Add(this.cmbMultiRow);
            this.tabPage2_Tabs.Controls.Add(this.textBoxAction_BarDblClck);
            this.tabPage2_Tabs.Controls.Add(this.btnBrowseAction_BarDblClck);
            this.tabPage2_Tabs.Controls.Add(this.chbAutoSubText);
            this.tabPage2_Tabs.Controls.Add(this.chbTabCloseButton);
            this.tabPage2_Tabs.Controls.Add(this.chbTabCloseBtnAlt);
            this.tabPage2_Tabs.Controls.Add(this.chbTabCloseBtnHover);
            this.tabPage2_Tabs.Controls.Add(this.chbFolderIcon);
            this.tabPage2_Tabs.Controls.Add(this.chbSubDirTipOnTab);
            this.tabPage2_Tabs.Controls.Add(this.chbDriveLetter);
            this.tabPage2_Tabs.Controls.Add(this.chbTabSwitcher);
            this.tabPage2_Tabs.Controls.Add(this.chbTreeShftWhlTab);
            this.tabPage2_Tabs.Controls.Add(this.chbRemoveOnSeparate);
            this.tabPage2_Tabs.Location = new Point(4, 0x16);
            this.tabPage2_Tabs.Padding = new Padding(3);
            this.tabPage2_Tabs.Size = new Size(0x1ff, 0x1d7);
            this.tabPage2_Tabs.TabIndex = 1;
            this.tabPage2_Tabs.UseVisualStyleBackColor = true;
            this.chbRemoveOnSeparate.AutoSize = true;
            this.chbRemoveOnSeparate.Location = new Point(0x1b, 0x1f2);
            this.chbRemoveOnSeparate.TabIndex = 0x11;
            this.chbTreeShftWhlTab.AutoSize = true;
            this.chbTreeShftWhlTab.Location = new Point(0x1b, 0x1da);
            this.chbTreeShftWhlTab.TabIndex = 0x10;
            this.chbTabSwitcher.AutoSize = true;
            this.chbTabSwitcher.Location = new Point(0x1b, 450);
            this.chbTabSwitcher.TabIndex = 15;
            this.chbDriveLetter.AutoSize = true;
            this.chbDriveLetter.Location = new Point(0x36, 0x1aa);
            this.chbDriveLetter.TabIndex = 14;
            this.chbSubDirTipOnTab.AutoSize = true;
            this.chbSubDirTipOnTab.Location = new Point(0x36, 0x196);
            this.chbSubDirTipOnTab.TabIndex = 13;
            this.chbFolderIcon.AutoSize = true;
            this.chbFolderIcon.Location = new Point(0x1b, 0x182);
            this.chbFolderIcon.TabIndex = 12;
            this.chbFolderIcon.CheckedChanged += new EventHandler(this.chbFolderIcon_CheckedChanged);
            this.chbTabCloseBtnHover.AutoSize = true;
            this.chbTabCloseBtnHover.Location = new Point(0x36, 0x16a);
            this.chbTabCloseBtnHover.TabIndex = 11;
            this.chbTabCloseBtnHover.CheckedChanged += new EventHandler(this.chbTabCloseBtns_CheckedChanged);
            this.chbTabCloseBtnAlt.AutoSize = true;
            this.chbTabCloseBtnAlt.Location = new Point(0x36, 0x156);
            this.chbTabCloseBtnAlt.TabIndex = 10;
            this.chbTabCloseBtnAlt.CheckedChanged += new EventHandler(this.chbTabCloseBtns_CheckedChanged);
            this.chbTabCloseButton.AutoSize = true;
            this.chbTabCloseButton.Location = new Point(0x1b, 0x142);
            this.chbTabCloseButton.TabIndex = 9;
            this.chbTabCloseButton.CheckedChanged += new EventHandler(this.chbTabCloseButton_CheckedChanged);
            this.chbAutoSubText.AutoSize = true;
            this.chbAutoSubText.Location = new Point(0x1b, 0x12a);
            this.chbAutoSubText.TabIndex = 8;
            this.cmbMultiRow.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbMultiRow.Location = new Point(0x11d, 0x102);
            this.cmbMultiRow.Size = new Size(0xa8, 0x15);
            this.cmbMultiRow.TabIndex = 7;
            this.lblMultiRows.AutoSize = true;
            this.lblMultiRows.Location = new Point(0x19, 0x105);
            this.btnBrowseAction_BarDblClck.Location = new Point(0x1a3, 0xd5);
            this.btnBrowseAction_BarDblClck.Size = new Size(0x22, 0x19);
            this.btnBrowseAction_BarDblClck.TabIndex = 6;
            this.btnBrowseAction_BarDblClck.Text = "...";
            this.btnBrowseAction_BarDblClck.Click += new EventHandler(this.btnBrowseAction_Click);
            this.textBoxAction_BarDblClck.Location = new Point(0x97, 0xd7);
            this.textBoxAction_BarDblClck.Size = new Size(0x107, 0x15);
            this.textBoxAction_BarDblClck.MaxLength = 260;
            this.textBoxAction_BarDblClck.TabIndex = 5;
            this.lblAction_BarDblClick.AutoSize = true;
            this.lblAction_BarDblClick.Location = new Point(0x2e, 0xda);
            this.cmbBGDblClick.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbBGDblClick.Location = new Point(0x11d, 0xb1);
            this.cmbBGDblClick.Size = new Size(0xa8, 0x15);
            this.cmbBGDblClick.TabIndex = 4;
            this.cmbBGDblClick.SelectedIndexChanged += new EventHandler(this.comboBoxes_SelectedIndexChanged);
            this.lblBGDblClik.AutoSize = true;
            this.lblBGDblClik.Location = new Point(0x19, 0xb5);
            this.cmbTabWhlClck.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbTabWhlClck.Location = new Point(0x11d, 0x89);
            this.cmbTabWhlClck.Size = new Size(0xa8, 0x15);
            this.cmbTabWhlClck.TabIndex = 3;
            this.lblTabWhlClk.AutoSize = true;
            this.lblTabWhlClk.Location = new Point(0x19, 0x8d);
            this.cmbTabDblClck.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbTabDblClck.Location = new Point(0x11d, 0x61);
            this.cmbTabDblClck.Size = new Size(0xa8, 0x15);
            this.cmbTabDblClck.TabIndex = 2;
            this.lblTabDblClk.AutoSize = true;
            this.lblTabDblClk.Location = new Point(0x19, 0x65);
            this.cmbActvClose.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbActvClose.Location = new Point(0x11d, 0x39);
            this.cmbActvClose.Size = new Size(0xa8, 0x15);
            this.cmbActvClose.TabIndex = 1;
            this.lblActvClose.AutoSize = true;
            this.lblActvClose.Location = new Point(0x19, 0x3d);
            this.cmbNewTabLoc.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbNewTabLoc.Location = new Point(0x11d, 0x11);
            this.cmbNewTabLoc.Size = new Size(0xa8, 0x15);
            this.cmbNewTabLoc.TabIndex = 0;
            this.lblNewTabLoc.AutoSize = true;
            this.lblNewTabLoc.Location = new Point(0x19, 0x15);
            this.tabPage3_Wndw.Controls.Add(this.chbWhlClick);
            this.tabPage3_Wndw.Controls.Add(this.chbNCADblClck);
            this.tabPage3_Wndw.Controls.Add(this.chbBlockProcess);
            this.tabPage3_Wndw.Controls.Add(this.chbFoldrTree);
            this.tabPage3_Wndw.Controls.Add(this.chbWndUnresizable);
            this.tabPage3_Wndw.Controls.Add(this.chbWndRestrAlpha);
            this.tabPage3_Wndw.Controls.Add(this.chbNoTabFromOuteside);
            this.tabPage3_Wndw.Controls.Add(this.chbHolizontalScroll);
            this.tabPage3_Wndw.Controls.Add(this.chbWhlChangeView);
            this.tabPage3_Wndw.Controls.Add(this.chbNeverCloseWindow);
            this.tabPage3_Wndw.Controls.Add(this.chbNeverCloseWndLocked);
            this.tabPage3_Wndw.Controls.Add(this.chbRestoreClosed);
            this.tabPage3_Wndw.Controls.Add(this.chbRestoreLocked);
            this.tabPage3_Wndw.Controls.Add(this.chbSendToTray);
            this.tabPage3_Wndw.Controls.Add(this.chbSendToTrayOnMinimize);
            this.tabPage3_Wndw.Controls.Add(this.cmbWhlClick);
            this.tabPage3_Wndw.Controls.Add(this.lblSep);
            this.tabPage3_Wndw.Location = new Point(4, 0x16);
            this.tabPage3_Wndw.Padding = new Padding(3);
            this.tabPage3_Wndw.Size = new Size(0x1ff, 0x1d7);
            this.tabPage3_Wndw.TabIndex = 4;
            this.tabPage3_Wndw.UseVisualStyleBackColor = true;
            this.lblSep.BorderStyle = BorderStyle.Fixed3D;
            this.lblSep.Location = new Point(0x1a, 0x115);
            this.lblSep.Margin = new Padding(0);
            this.lblSep.Size = new Size(0x149, 2);
            this.cmbWhlClick.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbWhlClick.Location = new Point(0x11d, 0x11);
            this.cmbWhlClick.Size = new Size(0x6f, 0x15);
            this.cmbWhlClick.TabIndex = 1;
            this.chbSendToTrayOnMinimize.AutoSize = true;
            this.chbSendToTrayOnMinimize.Location = new Point(0x1b, 0x1b3);
            this.chbSendToTrayOnMinimize.TabIndex = 13;
            this.chbSendToTray.AutoSize = true;
            this.chbSendToTray.Location = new Point(0x1b, 0x197);
            this.chbSendToTray.TabIndex = 12;
            this.chbRestoreLocked.AutoSize = true;
            this.chbRestoreLocked.Location = new Point(0x1b, 0x17b);
            this.chbRestoreLocked.TabIndex = 11;
            this.chbRestoreLocked.CheckedChanged += new EventHandler(this.chbsCloseWindow_CheckedChanged);
            this.chbRestoreClosed.AutoSize = true;
            this.chbRestoreClosed.Location = new Point(0x1b, 0x15f);
            this.chbRestoreClosed.TabIndex = 10;
            this.chbRestoreClosed.CheckedChanged += new EventHandler(this.chbsCloseWindow_CheckedChanged);
            this.chbNeverCloseWndLocked.AutoSize = true;
            this.chbNeverCloseWndLocked.Location = new Point(0x1b, 0x143);
            this.chbNeverCloseWndLocked.TabIndex = 9;
            this.chbNeverCloseWndLocked.CheckedChanged += new EventHandler(this.chbsCloseWindow_CheckedChanged);
            this.chbNeverCloseWindow.AutoSize = true;
            this.chbNeverCloseWindow.Location = new Point(0x1b, 0x127);
            this.chbNeverCloseWindow.TabIndex = 8;
            this.chbWhlChangeView.AutoSize = true;
            this.chbWhlChangeView.Location = new Point(0x1b, 0xf5);
            this.chbWhlChangeView.TabIndex = 9;
            this.chbHolizontalScroll.AutoSize = true;
            this.chbHolizontalScroll.Location = new Point(0x1b, 0xd9);
            this.chbHolizontalScroll.TabIndex = 8;
            this.chbNoTabFromOuteside.AutoSize = true;
            this.chbNoTabFromOuteside.Location = new Point(0x1b, 0xbd);
            this.chbNoTabFromOuteside.TabIndex = 7;
            this.chbFoldrTree.AutoSize = true;
            this.chbFoldrTree.Location = new Point(0x1b, 0xa1);
            this.chbFoldrTree.TabIndex = 6;
            this.chbBlockProcess.AutoSize = true;
            this.chbBlockProcess.Location = new Point(0x1b, 0x85);
            this.chbBlockProcess.TabIndex = 5;
            this.chbWndRestrAlpha.AutoSize = true;
            this.chbWndRestrAlpha.Location = new Point(0x1b, 0x69);
            this.chbWndRestrAlpha.TabIndex = 4;
            this.chbWndUnresizable.AutoSize = true;
            this.chbWndUnresizable.Location = new Point(0x1b, 0x4d);
            this.chbWndUnresizable.TabIndex = 3;
            this.chbNCADblClck.AutoSize = true;
            this.chbNCADblClck.Location = new Point(0x1b, 0x31);
            this.chbNCADblClck.TabIndex = 2;
            this.chbWhlClick.AutoSize = true;
            this.chbWhlClick.Location = new Point(0x1b, 0x15);
            this.chbWhlClick.TabIndex = 0;
            this.chbWhlClick.CheckedChanged += new EventHandler(this.chbMMButton_CheckedChanged);
            this.tabPage4_View.Controls.Add(this.chbUseTabSkin);
            this.tabPage4_View.Controls.Add(this.chbBoldActv);
            this.tabPage4_View.Controls.Add(this.chbToolbarBGClr);
            this.tabPage4_View.Controls.Add(this.chbRebarBGImage);
            this.tabPage4_View.Controls.Add(this.chbTabTitleShadow);
            this.tabPage4_View.Controls.Add(this.propertyGrid1);
            this.tabPage4_View.Controls.Add(this.nudTabWidth);
            this.tabPage4_View.Controls.Add(this.nudTabHeight);
            this.tabPage4_View.Controls.Add(this.nudTabWidthMax);
            this.tabPage4_View.Controls.Add(this.nudTabWidthMin);
            this.tabPage4_View.Controls.Add(this.lblTabSizeTitle);
            this.tabPage4_View.Controls.Add(this.lblTabWidth);
            this.tabPage4_View.Controls.Add(this.lblTabHeight);
            this.tabPage4_View.Controls.Add(this.lblTabWFix);
            this.tabPage4_View.Controls.Add(this.lblTabWMax);
            this.tabPage4_View.Controls.Add(this.lblTabWMin);
            this.tabPage4_View.Controls.Add(this.lblTabFont);
            this.tabPage4_View.Controls.Add(this.lblTabTxtClr);
            this.tabPage4_View.Controls.Add(this.lblTabTextAlignment);
            this.tabPage4_View.Controls.Add(this.lblMenuRenderer);
            this.tabPage4_View.Controls.Add(this.cmbTabSizeMode);
            this.tabPage4_View.Controls.Add(this.cmbTabTextAlignment);
            this.tabPage4_View.Controls.Add(this.cmbRebarBGImageMode);
            this.tabPage4_View.Controls.Add(this.cmbMenuRenderer);
            this.tabPage4_View.Controls.Add(this.btnHiliteClsc);
            this.tabPage4_View.Controls.Add(this.btnTabFont);
            this.tabPage4_View.Controls.Add(this.btnActTxtClr);
            this.tabPage4_View.Controls.Add(this.btnInactTxtClr);
            this.tabPage4_View.Controls.Add(this.btnDefTxtClr);
            this.tabPage4_View.Controls.Add(this.btnToolBarBGClr);
            this.tabPage4_View.Controls.Add(this.btnRebarImage);
            this.tabPage4_View.Controls.Add(this.btnShadowAct);
            this.tabPage4_View.Controls.Add(this.btnShadowIna);
            this.tabPage4_View.Controls.Add(this.btnTabImage);
            this.tabPage4_View.Controls.Add(this.tbRebarImagePath);
            this.tabPage4_View.Controls.Add(this.tbTabImagePath);
            this.tabPage4_View.Location = new Point(4, 0x16);
            this.tabPage4_View.Size = new Size(0x1ff, 0x1d7);
            this.tabPage4_View.TabIndex = 3;
            this.tabPage4_View.UseVisualStyleBackColor = true;
            this.cmbMenuRenderer.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbMenuRenderer.Location = new Point(0x99, 0x1df);
            this.cmbMenuRenderer.Size = new Size(100, 0x15);
            this.cmbMenuRenderer.TabIndex = 0x19;
            this.lblMenuRenderer.AutoSize = true;
            this.lblMenuRenderer.Location = new Point(13, 0x1e1);
            this.cmbRebarBGImageMode.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbRebarBGImageMode.Location = new Point(0xdf, 0x1c0);
            this.cmbRebarBGImageMode.Size = new Size(180, 0x15);
            this.cmbRebarBGImageMode.TabIndex = 0x18;
            this.btnRebarImage.Location = new Point(440, 0x1a6);
            this.btnRebarImage.Size = new Size(0x22, 0x19);
            this.btnRebarImage.TabIndex = 0x17;
            this.btnRebarImage.Text = "...";
            this.btnRebarImage.Click += new EventHandler(this.btnRebarImage_Click);
            this.tbRebarImagePath.Location = new Point(0xdf, 0x1a7);
            this.tbRebarImagePath.Size = new Size(0xd5, 0x15);
            this.tbRebarImagePath.MaxLength = 260;
            this.tbRebarImagePath.TabIndex = 0x16;
            this.tbRebarImagePath.KeyPress += new KeyPressEventHandler(this.textBoxesPath_KeyPress);
            this.chbRebarBGImage.AutoSize = true;
            this.chbRebarBGImage.Location = new Point(0x10, 0x1a9);
            this.chbRebarBGImage.TabIndex = 0x15;
            this.chbRebarBGImage.CheckedChanged += new EventHandler(this.chbRebarBGImage_CheckedChanged);
            this.btnToolBarBGClr.Location = new Point(0xdf, 0x18b);
            this.btnToolBarBGClr.Size = new Size(180, 0x17);
            this.btnToolBarBGClr.TabIndex = 20;
            this.btnToolBarBGClr.Click += new EventHandler(this.buttonToolBarBGClr_Click);
            this.chbToolbarBGClr.AutoSize = true;
            this.chbToolbarBGClr.Location = new Point(0x10, 400);
            this.chbToolbarBGClr.TabIndex = 0x13;
            this.chbToolbarBGClr.CheckedChanged += new EventHandler(this.chbToolbarBGClr_CheckedChanged);
            this.btnShadowIna.AutoSize = true;
            this.btnShadowIna.Location = new Point(0x107, 0x160);
            this.btnShadowIna.Size = new Size(100, 0x17);
            this.btnShadowIna.TabIndex = 0x12;
            this.btnShadowIna.Click += new EventHandler(this.btnShadowClrs_Click);
            this.btnShadowAct.AutoSize = true;
            this.btnShadowAct.Location = new Point(0x99, 0x160);
            this.btnShadowAct.Size = new Size(100, 0x17);
            this.btnShadowAct.TabIndex = 0x11;
            this.btnShadowAct.Click += new EventHandler(this.btnShadowClrs_Click);
            this.chbTabTitleShadow.AutoSize = true;
            this.chbTabTitleShadow.Location = new Point(0x10, 0x165);
            this.chbTabTitleShadow.TabIndex = 0x10;
            this.chbTabTitleShadow.CheckedChanged += new EventHandler(this.chbTabTitleShadow_CheckedChanged);
            this.btnDefTxtClr.AutoSize = true;
            this.btnDefTxtClr.Location = new Point(0x175, 0x13f);
            this.btnDefTxtClr.Size = new Size(100, 0x17);
            this.btnDefTxtClr.TabIndex = 15;
            this.btnDefTxtClr.Click += new EventHandler(this.buttonRstClr_Click);
            this.btnInactTxtClr.AutoSize = true;
            this.btnInactTxtClr.Location = new Point(0x107, 0x13f);
            this.btnInactTxtClr.Size = new Size(100, 0x17);
            this.btnInactTxtClr.TabIndex = 14;
            this.btnInactTxtClr.Click += new EventHandler(this.buttonInactClr_Click);
            this.btnActTxtClr.AutoSize = true;
            this.btnActTxtClr.Location = new Point(0x99, 0x13f);
            this.btnActTxtClr.Size = new Size(100, 0x17);
            this.btnActTxtClr.TabIndex = 13;
            this.btnActTxtClr.Click += new EventHandler(this.buttonActClr_Click);
            this.lblTabTxtClr.AutoSize = true;
            this.lblTabTxtClr.Location = new Point(13, 0x144);
            this.chbBoldActv.AutoSize = true;
            this.chbBoldActv.Location = new Point(0x109, 0x121);
            this.chbBoldActv.TabIndex = 12;
            this.btnTabFont.Location = new Point(0x99, 0x11c);
            this.btnTabFont.Size = new Size(100, 0x19);
            this.btnTabFont.TabIndex = 11;
            this.btnTabFont.Click += new EventHandler(this.btnFont_Click);
            this.lblTabFont.AutoSize = true;
            this.lblTabFont.Location = new Point(13, 290);
            this.cmbTabTextAlignment.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbTabTextAlignment.Location = new Point(0x99, 0xfe);
            this.cmbTabTextAlignment.Size = new Size(100, 0x15);
            this.cmbTabTextAlignment.TabIndex = 10;
            this.lblTabTextAlignment.Location = new Point(13, 0x100);
            this.lblTabTextAlignment.AutoSize = true;
            this.nudTabWidthMin.Location = new Point(0x152, 0xdf);
            int[] numArray10 = new int[4];
            numArray10[0] = 0x200;
            this.nudTabWidthMin.Maximum = new decimal(numArray10);
            int[] numArray11 = new int[4];
            numArray11[0] = 10;
            this.nudTabWidthMin.Minimum = new decimal(numArray11);
            this.nudTabWidthMin.Size = new Size(0x33, 0x15);
            this.nudTabWidthMin.TabIndex = 9;
            this.nudTabWidthMin.TextAlign = HorizontalAlignment.Center;
            int[] numArray12 = new int[4];
            numArray12[0] = 0x19;
            this.nudTabWidthMin.Value = new decimal(numArray12);
            this.nudTabWidthMin.ValueChanged += new EventHandler(this.numericUpDownMax_ValueChanged);
            this.lblTabWMin.Location = new Point(0xfc, 0xdf);
            this.lblTabWMin.Size = new Size(0x4c, 0x15);
            this.lblTabWMin.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.nudTabWidthMax.Location = new Point(0x152, 0xc5);
            int[] numArray13 = new int[4];
            numArray13[0] = 0x200;
            this.nudTabWidthMax.Maximum = new decimal(numArray13);
            int[] numArray14 = new int[4];
            numArray14[0] = 10;
            this.nudTabWidthMax.Minimum = new decimal(numArray14);
            this.nudTabWidthMax.Size = new Size(0x33, 0x15);
            this.nudTabWidthMax.TabIndex = 8;
            this.nudTabWidthMax.TextAlign = HorizontalAlignment.Center;
            int[] numArray15 = new int[4];
            numArray15[0] = 0x19;
            this.nudTabWidthMax.Value = new decimal(numArray15);
            this.nudTabWidthMax.ValueChanged += new EventHandler(this.numericUpDownMax_ValueChanged);
            this.lblTabWMax.Location = new Point(0xfc, 0xc5);
            this.lblTabWMax.Size = new Size(0x4c, 0x15);
            this.lblTabWMax.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.nudTabWidth.Location = new Point(0x152, 0xab);
            int[] numArray16 = new int[4];
            numArray16[0] = 0x200;
            this.nudTabWidth.Maximum = new decimal(numArray16);
            int[] numArray17 = new int[4];
            numArray17[0] = 10;
            this.nudTabWidth.Minimum = new decimal(numArray17);
            this.nudTabWidth.Size = new Size(0x33, 0x15);
            this.nudTabWidth.TabIndex = 7;
            this.nudTabWidth.TextAlign = HorizontalAlignment.Center;
            int[] numArray18 = new int[4];
            numArray18[0] = 0x18;
            this.nudTabWidth.Value = new decimal(numArray18);
            this.lblTabWFix.Location = new Point(0x102, 0xab);
            this.lblTabWFix.Size = new Size(70, 0x15);
            this.lblTabWFix.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cmbTabSizeMode.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbTabSizeMode.Location = new Point(0x99, 170);
            this.cmbTabSizeMode.Size = new Size(100, 0x15);
            this.cmbTabSizeMode.TabIndex = 6;
            this.cmbTabSizeMode.SelectedIndexChanged += new EventHandler(this.comboBoxes_SelectedIndexChanged);
            this.nudTabHeight.Location = new Point(0x99, 140);
            int[] numArray19 = new int[4];
            numArray19[0] = 50;
            this.nudTabHeight.Maximum = new decimal(numArray19);
            int[] numArray20 = new int[4];
            numArray20[0] = 10;
            this.nudTabHeight.Minimum = new decimal(numArray20);
            this.nudTabHeight.Size = new Size(0x33, 0x15);
            this.nudTabHeight.TabIndex = 5;
            this.nudTabHeight.TextAlign = HorizontalAlignment.Center;
            int[] numArray21 = new int[4];
            numArray21[0] = 0x18;
            this.nudTabHeight.Value = new decimal(numArray21);
            this.lblTabWidth.AutoSize = true;
            this.lblTabWidth.Location = new Point(0x4a, 0xac);
            this.lblTabHeight.AutoSize = true;
            this.lblTabHeight.Location = new Point(0x4a, 0x8f);
            this.lblTabSizeTitle.AutoSize = true;
            this.lblTabSizeTitle.Location = new Point(13, 0x79);
            this.propertyGrid1.HelpVisible = false;
            this.propertyGrid1.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.propertyGrid1.Location = new Point(15, 0x26);
            this.propertyGrid1.PropertySort = PropertySort.NoSort;
            this.propertyGrid1.Size = new Size(0x176, 0x48);
            this.propertyGrid1.TabIndex = 3;
            this.propertyGrid1.ToolbarVisible = false;
            this.btnHiliteClsc.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this.btnHiliteClsc.AutoSize = true;
            this.btnHiliteClsc.Location = new Point(0x1a3, 0x26);
            this.btnHiliteClsc.Size = new Size(0x4b, 0x17);
            this.btnHiliteClsc.TabIndex = 4;
            this.btnHiliteClsc.Click += new EventHandler(this.buttonHL_Click);
            this.btnTabImage.Location = new Point(440, 11);
            this.btnTabImage.Size = new Size(0x22, 0x19);
            this.btnTabImage.TabIndex = 2;
            this.btnTabImage.Text = "...";
            this.btnTabImage.Click += new EventHandler(this.btnTabImage_Click);
            this.tbTabImagePath.Location = new Point(0xdf, 12);
            this.tbTabImagePath.Size = new Size(0xd5, 0x15);
            this.tbTabImagePath.MaxLength = 260;
            this.tbTabImagePath.TabIndex = 1;
            this.tbTabImagePath.KeyPress += new KeyPressEventHandler(this.textBoxesPath_KeyPress);
            this.chbUseTabSkin.AutoSize = true;
            this.chbUseTabSkin.Location = new Point(15, 14);
            this.chbUseTabSkin.TabIndex = 0;
            this.chbUseTabSkin.CheckedChanged += new EventHandler(this.chbDrawMode_CheckedChanged);
            this.tabPage5_Grps.Controls.Add(this.btnUp_Grp);
            this.tabPage5_Grps.Controls.Add(this.btnDown_Grp);
            this.tabPage5_Grps.Controls.Add(this.btnAddSep_Grp);
            this.tabPage5_Grps.Controls.Add(this.btnStartUpGrp);
            this.tabPage5_Grps.Controls.Add(this.btnPlus_Grp);
            this.tabPage5_Grps.Controls.Add(this.btnMinus_Grp);
            this.tabPage5_Grps.Controls.Add(this.treeViewGroup);
            this.tabPage5_Grps.Controls.Add(this.cmbSpclFol_Grp);
            this.tabPage5_Grps.Controls.Add(this.btnAddSpcFol_Grp);
            this.tabPage5_Grps.Controls.Add(this.lblGroupKey);
            this.tabPage5_Grps.Controls.Add(this.tbGroupKey);
            this.tabPage5_Grps.Controls.Add(this.chbGroupKey);
            this.tabPage5_Grps.Location = new Point(4, 0x16);
            this.tabPage5_Grps.Padding = new Padding(3);
            this.tabPage5_Grps.Size = new Size(0x1ff, 0x1d7);
            this.tabPage5_Grps.TabIndex = 2;
            this.tabPage5_Grps.UseVisualStyleBackColor = true;
            this.treeViewGroup.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this.treeViewGroup.HideSelection = false;
            this.treeViewGroup.ImageKey = "noimage";
            this.treeViewGroup.SelectedImageKey = "noimage";
            this.treeViewGroup.ImageList = QTUtility.ImageListGlobal;
            this.treeViewGroup.LabelEdit = true;
            this.treeViewGroup.Location = new Point(5, 0x2d);
            this.treeViewGroup.ShowNodeToolTips = true;
            this.treeViewGroup.Size = new Size(0x1ed, 0x156);
            this.treeViewGroup.TabIndex = 6;
            this.treeViewGroup.AfterSelect += new TreeViewEventHandler(this.treeViewGroup_AfterSelect);
            this.treeViewGroup.BeforeLabelEdit += new NodeLabelEditEventHandler(this.treeViewGroup_BeforeLabelEdit);
            this.treeViewGroup.AfterLabelEdit += new NodeLabelEditEventHandler(this.treeViewGroup_AfterLabelEdit);
            this.treeViewGroup.KeyDown += new KeyEventHandler(this.treeViewGroup_KeyDown);
            this.btnUp_Grp.Enabled = false;
            this.btnUp_Grp.Location = new Point(5, 0x10);
            this.btnUp_Grp.Size = new Size(50, 0x17);
            this.btnUp_Grp.TabIndex = 0;
            this.btnUp_Grp.Click += new EventHandler(this.UpDownButtons_Click);
            this.btnDown_Grp.Enabled = false;
            this.btnDown_Grp.Location = new Point(0x3d, 0x10);
            this.btnDown_Grp.Size = new Size(50, 0x17);
            this.btnDown_Grp.TabIndex = 1;
            this.btnDown_Grp.Click += new EventHandler(this.UpDownButtons_Click);
            this.btnAddSep_Grp.Location = new Point(0x75, 0x10);
            this.btnAddSep_Grp.Size = new Size(120, 0x17);
            this.btnAddSep_Grp.TabIndex = 2;
            this.btnAddSep_Grp.Click += new EventHandler(this.btnAddSep_Click);
            this.btnStartUpGrp.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this.btnStartUpGrp.Location = new Point(0x150, 0x10);
            this.btnStartUpGrp.Size = new Size(100, 0x17);
            this.btnStartUpGrp.TabIndex = 3;
            this.btnStartUpGrp.Click += new EventHandler(this.btnStartUpGrp_Click);
            this.btnPlus_Grp.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this.btnPlus_Grp.Location = new Point(0x1ba, 0x10);
            this.btnPlus_Grp.Size = new Size(0x19, 0x17);
            this.btnPlus_Grp.TabIndex = 4;
            this.btnPlus_Grp.Text = "+";
            this.btnPlus_Grp.Click += new EventHandler(this.btnPlus_Click);
            this.btnMinus_Grp.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this.btnMinus_Grp.Location = new Point(0x1d9, 0x10);
            this.btnMinus_Grp.Size = new Size(0x19, 0x17);
            this.btnMinus_Grp.TabIndex = 5;
            this.btnMinus_Grp.Text = "-";
            this.btnMinus_Grp.Click += new EventHandler(this.btnMinus_Click);
            this.cmbSpclFol_Grp.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this.cmbSpclFol_Grp.Enabled = false;
            this.cmbSpclFol_Grp.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbSpclFol_Grp.Location = new Point(5, 0x187);
            this.cmbSpclFol_Grp.Size = new Size(150, 0x15);
            this.cmbSpclFol_Grp.TabIndex = 7;
            this.btnAddSpcFol_Grp.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this.btnAddSpcFol_Grp.Enabled = false;
            this.btnAddSpcFol_Grp.Location = new Point(0x9e, 390);
            this.btnAddSpcFol_Grp.Size = new Size(0x19, 0x17);
            this.btnAddSpcFol_Grp.TabIndex = 8;
            this.btnAddSpcFol_Grp.Text = "+";
            this.btnAddSpcFol_Grp.Click += new EventHandler(this.btnAddSpcFol_Grp_Click);
            this.lblGroupKey.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this.lblGroupKey.AutoSize = true;
            this.lblGroupKey.Location = new Point(6, 0x1ac);
            this.lblGroupKey.Size = new Size(0x61, 13);
            this.chbGroupKey.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this.chbGroupKey.AutoSize = true;
            this.chbGroupKey.Enabled = false;
            this.chbGroupKey.Location = new Point(0x6b, 420);
            this.chbGroupKey.TabIndex = 9;
            this.chbGroupKey.CheckedChanged += new EventHandler(this.chbGroupKey_CheckedChanged);
            this.tbGroupKey.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this.tbGroupKey.Enabled = false;
            this.tbGroupKey.Location = new Point(0x7f, 0x1a9);
            this.tbGroupKey.Size = new Size(340, 0x15);
            this.tbGroupKey.TextAlign = HorizontalAlignment.Center;
            this.tbGroupKey.TabIndex = 10;
            this.tbGroupKey.PreviewKeyDown += new PreviewKeyDownEventHandler(this.tbGroupKey_PreviewKeyDown);
            this.tbGroupKey.KeyPress += new KeyPressEventHandler(this.tbGroupKey_KeyPress);
            this.tabPage6_Apps.Controls.Add(this.treeViewUserApps);
            this.tabPage6_Apps.Controls.Add(this.btnUp_app);
            this.tabPage6_Apps.Controls.Add(this.btnDown_app);
            this.tabPage6_Apps.Controls.Add(this.btnAddSep_app);
            this.tabPage6_Apps.Controls.Add(this.btnAddVFolder_app);
            this.tabPage6_Apps.Controls.Add(this.btnPlus_app);
            this.tabPage6_Apps.Controls.Add(this.btnMinus_app);
            this.tabPage6_Apps.Controls.Add(this.lblUserApps_Path);
            this.tabPage6_Apps.Controls.Add(this.lblUserApps_Args);
            this.tabPage6_Apps.Controls.Add(this.lblUserApps_Working);
            this.tabPage6_Apps.Controls.Add(this.lblUserApps_Key);
            this.tabPage6_Apps.Controls.Add(this.tbPath);
            this.tabPage6_Apps.Controls.Add(this.tbArgs);
            this.tabPage6_Apps.Controls.Add(this.tbWorking);
            this.tabPage6_Apps.Controls.Add(this.chbUserAppKey);
            this.tabPage6_Apps.Controls.Add(this.tbUserAppKey);
            this.tabPage6_Apps.Controls.Add(this.btnOFD_app);
            this.tabPage6_Apps.Controls.Add(this.btnBFD_app);
            this.tabPage6_Apps.Controls.Add(this.btnAddToken_Arg);
            this.tabPage6_Apps.Controls.Add(this.btnAddToken_Wrk);
            this.tabPage6_Apps.Location = new Point(4, 0x16);
            this.tabPage6_Apps.Padding = new Padding(3);
            this.tabPage6_Apps.Size = new Size(0x1ff, 0x1d7);
            this.tabPage6_Apps.TabIndex = 5;
            this.tabPage6_Apps.UseVisualStyleBackColor = true;
            this.treeViewUserApps.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this.treeViewUserApps.HideSelection = false;
            this.treeViewUserApps.ImageKey = "noimage";
            this.treeViewUserApps.SelectedImageKey = "noimage";
            this.treeViewUserApps.ImageList = QTUtility.ImageListGlobal;
            this.treeViewUserApps.LabelEdit = true;
            this.treeViewUserApps.Location = new Point(5, 0x2d);
            this.treeViewUserApps.Size = new Size(0x1ed, 0x12e);
            this.treeViewUserApps.TabIndex = 6;
            this.treeViewUserApps.AfterLabelEdit += new NodeLabelEditEventHandler(this.treeViewUserApps_AfterLabelEdit);
            this.treeViewUserApps.AfterSelect += new TreeViewEventHandler(this.treeViewUserApps_AfterSelect);
            this.treeViewUserApps.BeforeLabelEdit += new NodeLabelEditEventHandler(this.treeViewUserApps_BeforeLabelEdit);
            this.treeViewUserApps.KeyDown += new KeyEventHandler(this.treeViewUserApps_KeyDown);
            this.btnUp_app.Enabled = false;
            this.btnUp_app.Location = new Point(5, 0x10);
            this.btnUp_app.Size = new Size(50, 0x17);
            this.btnUp_app.TabIndex = 0;
            this.btnUp_app.Click += new EventHandler(this.btnUpDown_app_Click);
            this.btnDown_app.Enabled = false;
            this.btnDown_app.Location = new Point(0x3d, 0x10);
            this.btnDown_app.Size = new Size(50, 0x17);
            this.btnDown_app.TabIndex = 1;
            this.btnDown_app.Click += new EventHandler(this.btnUpDown_app_Click);
            this.btnAddSep_app.Location = new Point(0x75, 0x10);
            this.btnAddSep_app.Size = new Size(120, 0x17);
            this.btnAddSep_app.TabIndex = 2;
            this.btnAddSep_app.Click += new EventHandler(this.btnAddSep_app_Click);
            this.btnAddVFolder_app.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this.btnAddVFolder_app.Location = new Point(0x182, 0x10);
            this.btnAddVFolder_app.Size = new Size(50, 0x18);
            this.btnAddVFolder_app.TabIndex = 3;
            this.btnAddVFolder_app.Text = "+";
            this.btnAddVFolder_app.TextImageRelation = TextImageRelation.ImageBeforeText;
            this.btnAddVFolder_app.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.btnAddVFolder_app.Click += new EventHandler(this.btnAddVirtualFolder_app_Click);
            this.btnPlus_app.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this.btnPlus_app.Location = new Point(0x1ba, 0x10);
            this.btnPlus_app.Size = new Size(0x19, 0x17);
            this.btnPlus_app.TabIndex = 4;
            this.btnPlus_app.Text = "+";
            this.btnPlus_app.Click += new EventHandler(this.btnPlus_app_Click);
            this.btnMinus_app.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this.btnMinus_app.Location = new Point(0x1d9, 0x10);
            this.btnMinus_app.Size = new Size(0x19, 0x17);
            this.btnMinus_app.TabIndex = 5;
            this.btnMinus_app.Text = "-";
            this.btnMinus_app.Click += new EventHandler(this.btnMinus_app_Click);
            this.lblUserApps_Path.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this.lblUserApps_Path.AutoSize = true;
            this.lblUserApps_Path.Location = new Point(6, 0x164);
            this.lblUserApps_Path.Size = new Size(0x21, 13);
            this.lblUserApps_Args.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this.lblUserApps_Args.AutoSize = true;
            this.lblUserApps_Args.Location = new Point(6, 380);
            this.lblUserApps_Args.Size = new Size(0x3f, 13);
            this.lblUserApps_Working.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this.lblUserApps_Working.AutoSize = true;
            this.lblUserApps_Working.Location = new Point(6, 0x194);
            this.lblUserApps_Working.Size = new Size(0x61, 13);
            this.lblUserApps_Key.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this.lblUserApps_Key.AutoSize = true;
            this.lblUserApps_Key.Location = new Point(6, 0x1ac);
            this.lblUserApps_Key.Size = new Size(0x61, 13);
            this.tbPath.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this.tbPath.Enabled = false;
            this.tbPath.Location = new Point(0x6b, 0x161);
            this.tbPath.Size = new Size(0x149, 0x15);
            this.tbPath.MaxLength = 260;
            this.tbPath.TabIndex = 7;
            this.tbPath.TextChanged += new EventHandler(this.tbsUserApps_TextChanged);
            this.tbPath.KeyPress += new KeyPressEventHandler(this.textBoxesPath_KeyPress);
            this.tbArgs.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this.tbArgs.Enabled = false;
            this.tbArgs.Location = new Point(0x6b, 0x179);
            this.tbArgs.Size = new Size(360, 0x15);
            this.tbArgs.MaxLength = 260;
            this.tbArgs.TabIndex = 10;
            this.tbArgs.TextChanged += new EventHandler(this.tbsUserApps_TextChanged);
            this.tbWorking.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this.tbWorking.Enabled = false;
            this.tbWorking.Location = new Point(0x6b, 0x191);
            this.tbWorking.Size = new Size(360, 0x15);
            this.tbWorking.MaxLength = 260;
            this.tbWorking.TabIndex = 12;
            this.tbWorking.TextChanged += new EventHandler(this.tbsUserApps_TextChanged);
            this.tbWorking.KeyPress += new KeyPressEventHandler(this.textBoxesPath_KeyPress);
            this.chbUserAppKey.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this.chbUserAppKey.AutoSize = true;
            this.chbUserAppKey.Enabled = false;
            this.chbUserAppKey.Location = new Point(0x6b, 420);
            this.chbUserAppKey.TabIndex = 14;
            this.chbUserAppKey.CheckedChanged += new EventHandler(this.chbUserAppKey_CheckedChanged);
            this.tbUserAppKey.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this.tbUserAppKey.Enabled = false;
            this.tbUserAppKey.Location = new Point(0x7f, 0x1a9);
            this.tbUserAppKey.Size = new Size(340, 0x15);
            this.tbUserAppKey.TextAlign = HorizontalAlignment.Center;
            this.tbUserAppKey.TabIndex = 15;
            this.tbUserAppKey.PreviewKeyDown += new PreviewKeyDownEventHandler(this.tbUserAppKey_PreviewKeyDown);
            this.tbUserAppKey.KeyPress += new KeyPressEventHandler(this.tbUserAppKey_KeyPress);
            this.btnOFD_app.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this.btnOFD_app.Enabled = false;
            this.btnOFD_app.Location = new Point(0x1ba, 0x161);
            this.btnOFD_app.Size = new Size(0x19, 0x15);
            this.btnOFD_app.TabIndex = 8;
            this.btnOFD_app.Text = "...";
            this.btnOFD_app.UseVisualStyleBackColor = true;
            this.btnOFD_app.Click += new EventHandler(this.btnOFD_app_Click);
            this.btnBFD_app.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this.btnBFD_app.Enabled = false;
            this.btnBFD_app.Location = new Point(0x1d9, 0x161);
            this.btnBFD_app.Size = new Size(0x19, 0x15);
            this.btnBFD_app.TabIndex = 9;
            this.btnBFD_app.Text = ".";
            this.btnBFD_app.UseVisualStyleBackColor = true;
            this.btnBFD_app.Click += new EventHandler(this.btnBFD_app_Click);
            this.btnAddToken_Arg.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this.btnAddToken_Arg.Location = new Point(0x1d9, 0x179);
            this.btnAddToken_Arg.Enabled = false;
            this.btnAddToken_Arg.Size = new Size(0x19, 0x15);
            this.btnAddToken_Arg.TabIndex = 11;
            this.btnAddToken_Arg.Text = "%";
            this.btnAddToken_Arg.UseVisualStyleBackColor = true;
            this.btnAddToken_Arg.Click += new EventHandler(this.btnAddToken_Arg_Click);
            this.btnAddToken_Wrk.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this.btnAddToken_Wrk.Enabled = false;
            this.btnAddToken_Wrk.Location = new Point(0x1d9, 0x191);
            this.btnAddToken_Wrk.Size = new Size(0x19, 0x15);
            this.btnAddToken_Wrk.TabIndex = 13;
            this.btnAddToken_Wrk.Text = "%";
            this.btnAddToken_Wrk.UseVisualStyleBackColor = true;
            this.btnAddToken_Wrk.Click += new EventHandler(this.btnAddToken_Wrk_Click);
            this.cmsAddToken.ShowImageMargin = false;
            this.cmsAddToken.ItemClicked += new ToolStripItemClickedEventHandler(this.cmsAddToken_ItemClicked);
            this.tabPage9_Misc.Controls.Add(this.chbHideMenu);
            this.tabPage9_Misc.Controls.Add(this.chbBSUpOneLvl);
            this.tabPage9_Misc.Controls.Add(this.chbNoFulRowSelect);
            this.tabPage9_Misc.Controls.Add(this.chbGridLine);
            this.tabPage9_Misc.Controls.Add(this.chbAlternateColor);
            this.tabPage9_Misc.Controls.Add(this.chbShowPreview);
            this.tabPage9_Misc.Controls.Add(this.chbPreviewMode);
            this.tabPage9_Misc.Controls.Add(this.chbPreviewInfo);
            this.tabPage9_Misc.Controls.Add(this.chbSubDirTip);
            this.tabPage9_Misc.Controls.Add(this.chbSubDirTipMode);
            this.tabPage9_Misc.Controls.Add(this.chbSubDirTipModeHidden);
            this.tabPage9_Misc.Controls.Add(this.chbSubDirTipModeSystem);
            this.tabPage9_Misc.Controls.Add(this.chbSubDirTipModeFile);
            this.tabPage9_Misc.Controls.Add(this.chbSubDirTipPreview);
            this.tabPage9_Misc.Controls.Add(this.chbSelectWithoutExt);
            this.tabPage9_Misc.Controls.Add(this.chbF2Selection);
            this.tabPage9_Misc.Controls.Add(this.chbCursorLoop);
            this.tabPage9_Misc.Controls.Add(this.btnAlternateColor);
            this.tabPage9_Misc.Controls.Add(this.btnAlternateColor_Text);
            this.tabPage9_Misc.Controls.Add(this.btnAlternate_Default);
            this.tabPage9_Misc.Controls.Add(this.btnAddImgExt);
            this.tabPage9_Misc.Controls.Add(this.btnDelImgExt);
            this.tabPage9_Misc.Controls.Add(this.btnDefaultImgExt);
            this.tabPage9_Misc.Controls.Add(this.btnPreviewFont);
            this.tabPage9_Misc.Controls.Add(this.btnPreviewFontDefault);
            this.tabPage9_Misc.Controls.Add(this.cmbImgExts);
            this.tabPage9_Misc.Controls.Add(this.btnAddTextExt);
            this.tabPage9_Misc.Controls.Add(this.btnDelTextExt);
            this.tabPage9_Misc.Controls.Add(this.btnDefaultTextExt);
            this.tabPage9_Misc.Controls.Add(this.cmbTextExts);
            this.tabPage9_Misc.Controls.Add(this.btnPayPal);
            this.tabPage9_Misc.Controls.Add(this.nudPreviewMaxHeight);
            this.tabPage9_Misc.Controls.Add(this.nudPreviewMaxWidth);
            this.tabPage9_Misc.Controls.Add(this.lblPreviewHeight);
            this.tabPage9_Misc.Controls.Add(this.lblPreviewWidth);
            this.tabPage9_Misc.Location = new Point(4, 0x16);
            this.tabPage9_Misc.Padding = new Padding(3);
            this.tabPage9_Misc.Size = new Size(0x1ff, 0x1d7);
            this.tabPage9_Misc.TabIndex = 6;
            this.tabPage9_Misc.UseVisualStyleBackColor = true;
            this.btnPayPal.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this.btnPayPal.BackgroundImage = Resources_String.paypalBtn;
            this.btnPayPal.BackgroundImageLayout = ImageLayout.Center;
            this.btnPayPal.Location = new Point(0x1a8, 0x17b);
            this.btnPayPal.Size = new Size(0x47, 0x4c);
            this.btnPayPal.Cursor = Cursors.Hand;
            this.btnPayPal.TabIndex = 0x20;
            this.btnPayPal.UseVisualStyleBackColor = false;
            this.btnPayPal.Click += new EventHandler(this.btnPayPal_Click);
            this.chbCursorLoop.AutoSize = true;
            this.chbCursorLoop.Location = new Point(0x1b, 0x1e1);
            this.chbCursorLoop.TabIndex = 0x1f;
            this.chbF2Selection.AutoSize = true;
            this.chbF2Selection.Location = new Point(0x1b, 0x1c9);
            this.chbF2Selection.TabIndex = 30;
            this.chbSelectWithoutExt.AutoSize = true;
            this.chbSelectWithoutExt.Location = new Point(0x1b, 0x1b1);
            this.chbSelectWithoutExt.TabIndex = 0x1d;
            this.chbSubDirTipModeSystem.AutoSize = true;
            this.chbSubDirTipModeSystem.Location = new Point(0x2d, 0x193);
            this.chbSubDirTipModeSystem.TabIndex = 0x1c;
            this.chbSubDirTipModeFile.AutoSize = true;
            this.chbSubDirTipModeFile.Location = new Point(0xd7, 0x179);
            this.chbSubDirTipModeFile.TabIndex = 0x1b;
            this.chbSubDirTipModeHidden.AutoSize = true;
            this.chbSubDirTipModeHidden.Location = new Point(0x2d, 0x179);
            this.chbSubDirTipModeHidden.TabIndex = 0x1a;
            this.chbSubDirTipPreview.AutoSize = true;
            this.chbSubDirTipPreview.Location = new Point(0xd7, 0x15f);
            this.chbSubDirTipPreview.TabIndex = 0x19;
            this.chbSubDirTipMode.AutoSize = true;
            this.chbSubDirTipMode.Location = new Point(0x2d, 0x15f);
            this.chbSubDirTipMode.TabIndex = 0x18;
            this.chbSubDirTip.AutoSize = true;
            this.chbSubDirTip.Location = new Point(0x1b, 330);
            this.chbSubDirTip.TabIndex = 0x17;
            this.chbSubDirTip.CheckedChanged += new EventHandler(this.chbSubDirTip_CheckedChanged);
            this.btnPreviewFontDefault.Location = new Point(0x191, 0x127);
            this.btnPreviewFontDefault.Size = new Size(100, 0x17);
            this.btnPreviewFontDefault.TabIndex = 0x16;
            this.btnPreviewFontDefault.Click += new EventHandler(this.btnPreviewFont_Click);
            this.btnPreviewFont.Location = new Point(0x123, 0x127);
            this.btnPreviewFont.Size = new Size(100, 0x17);
            this.btnPreviewFont.TabIndex = 0x15;
            this.btnPreviewFont.Click += new EventHandler(this.btnPreviewFont_Click);
            this.btnDefaultTextExt.Location = new Point(0x191, 0x10d);
            this.btnDefaultTextExt.Size = new Size(100, 0x17);
            this.btnDefaultTextExt.TabIndex = 20;
            this.btnDefaultTextExt.Click += new EventHandler(this.btnDefaultTextExt_Click);
            this.btnDelTextExt.Location = new Point(0x123, 0x10d);
            this.btnDelTextExt.Size = new Size(100, 0x17);
            this.btnDelTextExt.TabIndex = 0x13;
            this.btnDelTextExt.Click += new EventHandler(this.btnDelPreiviewExt_Click);
            this.btnAddTextExt.Location = new Point(0xb5, 0x10d);
            this.btnAddTextExt.Size = new Size(100, 0x17);
            this.btnAddTextExt.TabIndex = 0x12;
            this.btnAddTextExt.Click += new EventHandler(this.btnAddPreviewExt_Click);
            this.cmbTextExts.Location = new Point(0x2d, 0x10d);
            this.cmbTextExts.Size = new Size(130, 0x17);
            this.cmbTextExts.TabIndex = 0x11;
            this.cmbTextExts.SelectedIndexChanged += new EventHandler(this.comboBoxes_SelectedIndexChanged);
            this.cmbTextExts.KeyPress += new KeyPressEventHandler(this.comboBoxPreviewExts_KeyPress);
            this.btnDefaultImgExt.Location = new Point(0x191, 0xf3);
            this.btnDefaultImgExt.Size = new Size(100, 0x17);
            this.btnDefaultImgExt.TabIndex = 0x10;
            this.btnDefaultImgExt.Click += new EventHandler(this.btnDefaultImgExt_Click);
            this.btnDelImgExt.Location = new Point(0x123, 0xf3);
            this.btnDelImgExt.Size = new Size(100, 0x17);
            this.btnDelImgExt.TabIndex = 15;
            this.btnDelImgExt.Click += new EventHandler(this.btnDelPreiviewExt_Click);
            this.btnAddImgExt.Location = new Point(0xb5, 0xf3);
            this.btnAddImgExt.Size = new Size(100, 0x17);
            this.btnAddImgExt.TabIndex = 14;
            this.btnAddImgExt.Click += new EventHandler(this.btnAddPreviewExt_Click);
            this.cmbImgExts.Location = new Point(0x2d, 0xf3);
            this.cmbImgExts.Size = new Size(130, 0x17);
            this.cmbImgExts.TabIndex = 13;
            this.cmbImgExts.SelectedIndexChanged += new EventHandler(this.comboBoxes_SelectedIndexChanged);
            this.cmbImgExts.KeyPress += new KeyPressEventHandler(this.comboBoxPreviewExts_KeyPress);
            this.lblPreviewWidth.Location = new Point(0x129, 0xbf);
            this.lblPreviewWidth.Size = new Size(0x62, 0x15);
            this.lblPreviewWidth.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblPreviewHeight.Location = new Point(0x129, 0xd9);
            this.lblPreviewHeight.Size = new Size(0x62, 0x15);
            this.lblPreviewHeight.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.nudPreviewMaxWidth.Location = new Point(0x191, 0xbf);
            int[] numArray22 = new int[4];
            numArray22[0] = 0x780;
            this.nudPreviewMaxWidth.Maximum = new decimal(numArray22);
            int[] numArray23 = new int[4];
            numArray23[0] = 0x80;
            this.nudPreviewMaxWidth.Minimum = new decimal(numArray23);
            this.nudPreviewMaxWidth.Size = new Size(0x3e, 0x15);
            this.nudPreviewMaxWidth.TabIndex = 11;
            this.nudPreviewMaxWidth.TextAlign = HorizontalAlignment.Center;
            int[] numArray24 = new int[4];
            numArray24[0] = 0x200;
            this.nudPreviewMaxWidth.Value = new decimal(numArray24);
            this.nudPreviewMaxHeight.Location = new Point(0x191, 0xd9);
            int[] numArray25 = new int[4];
            numArray25[0] = 0x4b0;
            this.nudPreviewMaxHeight.Maximum = new decimal(numArray25);
            int[] numArray26 = new int[4];
            numArray26[0] = 0x60;
            this.nudPreviewMaxHeight.Minimum = new decimal(numArray26);
            this.nudPreviewMaxHeight.Size = new Size(0x3e, 0x15);
            this.nudPreviewMaxHeight.TabIndex = 12;
            this.nudPreviewMaxHeight.TextAlign = HorizontalAlignment.Center;
            int[] numArray27 = new int[4];
            numArray27[0] = 0x100;
            this.nudPreviewMaxHeight.Value = new decimal(numArray27);
            this.chbPreviewInfo.AutoSize = true;
            this.chbPreviewInfo.Location = new Point(0x2d, 0xd7);
            this.chbPreviewInfo.TabIndex = 10;
            this.chbPreviewMode.AutoSize = true;
            this.chbPreviewMode.Location = new Point(0x2d, 0xbf);
            this.chbPreviewMode.TabIndex = 9;
            this.chbShowPreview.AutoSize = true;
            this.chbShowPreview.Location = new Point(0x1b, 170);
            this.chbShowPreview.TabIndex = 8;
            this.chbShowPreview.CheckedChanged += new EventHandler(this.chbShowPreviewTooltip_CheckedChanged);
            this.btnAlternate_Default.Enabled = false;
            this.btnAlternate_Default.Location = new Point(0x191, 0x87);
            this.btnAlternate_Default.Size = new Size(100, 0x17);
            this.btnAlternate_Default.TabIndex = 7;
            this.btnAlternate_Default.Click += new EventHandler(this.btnAlternateColor_Click);
            this.btnAlternateColor_Text.Enabled = false;
            this.btnAlternateColor_Text.Location = new Point(0x123, 0x87);
            this.btnAlternateColor_Text.Size = new Size(100, 0x17);
            this.btnAlternateColor_Text.TabIndex = 6;
            this.btnAlternateColor_Text.Click += new EventHandler(this.btnAlternateColor_Click);
            this.btnAlternateColor.Enabled = false;
            this.btnAlternateColor.Location = new Point(0xb5, 0x87);
            this.btnAlternateColor.Size = new Size(100, 0x17);
            this.btnAlternateColor.TabIndex = 5;
            this.btnAlternateColor.Click += new EventHandler(this.btnAlternateColor_Click);
            this.chbAlternateColor.AutoSize = true;
            this.chbAlternateColor.Location = new Point(0x1b, 0x70);
            this.chbAlternateColor.TabIndex = 4;
            this.chbAlternateColor.CheckedChanged += new EventHandler(this.chbAlternateColor_CheckedChanged);
            this.chbGridLine.AutoSize = true;
            this.chbGridLine.Location = new Point(0x1b, 0x58);
            this.chbGridLine.TabIndex = 3;
            this.chbNoFulRowSelect.AutoSize = true;
            this.chbNoFulRowSelect.Location = new Point(0x1b, 0x40);
            this.chbNoFulRowSelect.TabIndex = 2;
            this.chbBSUpOneLvl.AutoSize = true;
            this.chbBSUpOneLvl.Location = new Point(0x1b, 40);
            this.chbBSUpOneLvl.TabIndex = 1;
            this.chbHideMenu.AutoSize = true;
            this.chbHideMenu.Location = new Point(0x1b, 0x10);
            this.chbHideMenu.TabIndex = 0;
            this.tabPage7_Plug.Controls.Add(this.btnBrowsePlugin);
            this.tabPage7_Plug.Controls.Add(this.pluginView);
            this.tabPage7_Plug.Controls.Add(this.lblPluginLang);
            this.tabPage7_Plug.Controls.Add(this.textBoxPluginLang);
            this.tabPage7_Plug.Controls.Add(this.btnBrowsePluginLang);
            this.tabPage7_Plug.Location = new Point(4, 0x16);
            this.tabPage7_Plug.Padding = new Padding(3);
            this.tabPage7_Plug.Size = new Size(0x1ff, 0x1d7);
            this.tabPage7_Plug.TabIndex = 2;
            this.tabPage7_Plug.UseVisualStyleBackColor = true;
            this.btnBrowsePlugin.AutoSize = true;
            this.btnBrowsePlugin.Location = new Point(5, 0x10);
            this.btnBrowsePlugin.TabIndex = 0;
            this.btnBrowsePlugin.UseVisualStyleBackColor = true;
            this.btnBrowsePlugin.Click += new EventHandler(this.btnBrowsePlugin_Click);
            this.pluginView.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this.pluginView.ColumnCount = 1;
            this.pluginView.ColumnStyles.Add(new ColumnStyle());
            this.pluginView.Location = new Point(5, 0x2d);
            this.pluginView.Size = new Size(0x1ed, 0x156);
            this.pluginView.TabIndex = 1;
            this.lblPluginLang.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this.lblPluginLang.AutoSize = true;
            this.lblPluginLang.Location = new Point(0x1b, 0x194);
            this.textBoxPluginLang.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this.textBoxPluginLang.Location = new Point(0x2d, 0x1ac);
            this.textBoxPluginLang.Size = new Size(0x174, 0x15);
            this.textBoxPluginLang.MaxLength = 260;
            this.textBoxPluginLang.TabIndex = 2;
            this.textBoxPluginLang.KeyPress += new KeyPressEventHandler(this.textBoxesPath_KeyPress);
            this.btnBrowsePluginLang.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this.btnBrowsePluginLang.Location = new Point(0x1a7, 0x1a9);
            this.btnBrowsePluginLang.Size = new Size(0x22, 0x19);
            this.btnBrowsePluginLang.TabIndex = 3;
            this.btnBrowsePluginLang.Text = "...";
            this.btnBrowsePluginLang.Click += new EventHandler(this.btnBrowsePluginLang_Click);
            this.tabPage8_Keys.Controls.Add(this.btnCopyKeys);
            this.tabPage8_Keys.Controls.Add(this.listViewKeyboard);
            this.tabPage8_Keys.Location = new Point(4, 0x16);
            this.tabPage8_Keys.Padding = new Padding(3);
            this.tabPage8_Keys.Size = new Size(0x1ff, 0x1d7);
            this.tabPage8_Keys.TabIndex = 2;
            this.tabPage8_Keys.UseVisualStyleBackColor = true;
            this.btnCopyKeys.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            this.btnCopyKeys.Location = new Point(5, 0x10);
            this.btnCopyKeys.AutoSize = true;
            this.btnCopyKeys.TabIndex = 0;
            this.btnCopyKeys.UseVisualStyleBackColor = true;
            this.btnCopyKeys.Click += new EventHandler(this.btnCopyKeys_Click);
            this.listViewKeyboard.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this.listViewKeyboard.CheckBoxes = true;
            this.listViewKeyboard.Columns.AddRange(new ColumnHeader[] { this.clmKeys_Action, this.clmKeys_Key });
            this.listViewKeyboard.FullRowSelect = true;
            this.listViewKeyboard.GridLines = true;
            this.listViewKeyboard.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            this.listViewKeyboard.Location = new Point(5, 0x2d);
            this.listViewKeyboard.MultiSelect = false;
            this.listViewKeyboard.Size = new Size(0x1ed, 0x191);
            this.listViewKeyboard.ShowItemToolTips = true;
            this.listViewKeyboard.TabIndex = 1;
            this.listViewKeyboard.UseCompatibleStateImageBehavior = false;
            this.listViewKeyboard.View = View.Details;
            this.listViewKeyboard.PreviewKeyDown += new PreviewKeyDownEventHandler(this.listViewKeyboard_PreviewKeyDown);
            this.listViewKeyboard.KeyPress += new KeyPressEventHandler(this.listViewKeyboard_KeyPress);
            this.clmKeys_Action.Text = "Action";
            this.clmKeys_Action.Width = 0x15c;
            this.clmKeys_Key.Text = "Key";
            this.clmKeys_Key.TextAlign = HorizontalAlignment.Center;
            this.clmKeys_Key.Width = 120;
            this.tabPageA_Path.Controls.Add(this.listView_NoCapture);
            this.tabPageA_Path.Controls.Add(this.btnOFD_NoCapture);
            this.tabPageA_Path.Controls.Add(this.btnAdd_NoCapture);
            this.tabPageA_Path.Controls.Add(this.btnRemove_NoCapture);
            this.tabPageA_Path.Controls.Add(this.cmbSpclFol_NoCapture);
            this.tabPageA_Path.Controls.Add(this.btnAddSpcFol_NoCapture);
            this.tabPageA_Path.Location = new Point(4, 0x16);
            this.tabPageA_Path.Padding = new Padding(3);
            this.tabPageA_Path.Size = new Size(0x1ff, 0x1d7);
            this.tabPageA_Path.TabIndex = 2;
            this.tabPageA_Path.UseVisualStyleBackColor = true;
            this.listView_NoCapture.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.listView_NoCapture.Columns.AddRange(new ColumnHeader[] { this.clmnHeader_NoCapture });
            this.listView_NoCapture.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            this.listView_NoCapture.HideSelection = false;
            this.listView_NoCapture.LabelEdit = true;
            this.listView_NoCapture.Location = new Point(5, 0x2d);
            this.listView_NoCapture.Size = new Size(0x1ed, 0xa3);
            this.listView_NoCapture.FullRowSelect = true;
            this.listView_NoCapture.TabIndex = 8;
            this.listView_NoCapture.UseCompatibleStateImageBehavior = false;
            this.listView_NoCapture.View = View.Details;
            this.listView_NoCapture.ItemActivate += new EventHandler(this.listView_NoCapture_ItemActivate);
            this.listView_NoCapture.SelectedIndexChanged += new EventHandler(this.listView_NoCapture_SelectedIndexChanged);
            this.listView_NoCapture.KeyDown += new KeyEventHandler(this.listView_NoCapture_KeyDown);
            this.listView_NoCapture.BeforeLabelEdit += new LabelEditEventHandler(this.listView_NoCapture_BeforeLabelEdit);
            this.listView_NoCapture.AfterLabelEdit += new LabelEditEventHandler(this.listView_NoCapture_AfterLabelEdit);
            this.clmnHeader_NoCapture.Width = 0x1d8;
            this.btnOFD_NoCapture.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this.btnOFD_NoCapture.Location = new Point(0x19b, 0x10);
            this.btnOFD_NoCapture.Size = new Size(0x19, 0x17);
            this.btnOFD_NoCapture.TabIndex = 0;
            this.btnOFD_NoCapture.Text = "...";
            this.btnOFD_NoCapture.UseVisualStyleBackColor = true;
            this.btnOFD_NoCapture.Click += new EventHandler(this.btnOFD_NoCapture_Click);
            this.btnAdd_NoCapture.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this.btnAdd_NoCapture.Location = new Point(0x1ba, 0x10);
            this.btnAdd_NoCapture.Size = new Size(0x19, 0x17);
            this.btnAdd_NoCapture.TabIndex = 1;
            this.btnAdd_NoCapture.Text = "+";
            this.btnAdd_NoCapture.UseVisualStyleBackColor = true;
            this.btnAdd_NoCapture.Click += new EventHandler(this.btnAdd_NoCapture_Click);
            this.btnRemove_NoCapture.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this.btnRemove_NoCapture.Enabled = false;
            this.btnRemove_NoCapture.Location = new Point(0x1d9, 0x10);
            this.btnRemove_NoCapture.Size = new Size(0x19, 0x17);
            this.btnRemove_NoCapture.TabIndex = 2;
            this.btnRemove_NoCapture.Text = "-";
            this.btnRemove_NoCapture.UseVisualStyleBackColor = true;
            this.btnRemove_NoCapture.Click += new EventHandler(this.btnRemove_NoCapture_Click);
            this.cmbSpclFol_NoCapture.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            this.cmbSpclFol_NoCapture.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbSpclFol_NoCapture.Location = new Point(5, 0xd4);
            this.cmbSpclFol_NoCapture.Size = new Size(150, 0x15);
            this.cmbSpclFol_NoCapture.TabIndex = 3;
            this.btnAddSpcFol_NoCapture.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            this.btnAddSpcFol_NoCapture.Location = new Point(0x9e, 0xd3);
            this.btnAddSpcFol_NoCapture.Size = new Size(0x19, 0x17);
            this.btnAddSpcFol_NoCapture.Text = "+";
            this.btnAddSpcFol_NoCapture.TabIndex = 4;
            this.btnAddSpcFol_NoCapture.Click += new EventHandler(this.btnAddSpcFol_NoCapture_Click);
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0x211, 0x269);
            this.MinimumSize = new Size(0x221, 0x28e);
            base.Controls.Add(this.tabControl1);
            base.Controls.Add(this.lblVer);
            base.Controls.Add(this.btnOK);
            base.Controls.Add(this.btnCancel);
            base.Controls.Add(this.btnApply);
            base.MaximizeBox = false;
            base.ShowIcon = false;
            base.StartPosition = FormStartPosition.CenterParent;
            this.Text = "QTTabBar Options";
            base.FormClosing += new FormClosingEventHandler(this.OptionsDialog_FormClosing);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1_Gnrl.ResumeLayout(false);
            this.tabPage1_Gnrl.PerformLayout();
            this.tabPage2_Tabs.ResumeLayout(false);
            this.tabPage2_Tabs.PerformLayout();
            this.tabPage3_Wndw.ResumeLayout(false);
            this.tabPage3_Wndw.PerformLayout();
            this.tabPage4_View.ResumeLayout(false);
            this.tabPage4_View.PerformLayout();
            this.tabPage5_Grps.ResumeLayout(false);
            this.tabPage5_Grps.PerformLayout();
            this.tabPage6_Apps.ResumeLayout(false);
            this.tabPage6_Apps.PerformLayout();
            this.tabPage7_Plug.ResumeLayout(false);
            this.tabPage7_Plug.PerformLayout();
            this.tabPage8_Keys.ResumeLayout(false);
            this.tabPage8_Keys.PerformLayout();
            this.tabPage9_Misc.ResumeLayout(false);
            this.tabPage9_Misc.PerformLayout();
            this.tabPageA_Path.ResumeLayout(false);
            this.tabPageA_Path.PerformLayout();
            this.nudMaxRecentFile.EndInit();
            this.nudMaxUndo.EndInit();
            this.nudNetworkTimeOut.EndInit();
            this.nudTabWidthMin.EndInit();
            this.nudTabWidthMax.EndInit();
            this.nudTabHeight.EndInit();
            this.nudTabWidth.EndInit();
            this.nudPreviewMaxHeight.EndInit();
            this.nudPreviewMaxWidth.EndInit();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void InitializePluginView() {
            this.pluginView.SuspendLayout();
            this.CreatePluginViewItem(PluginManager.PluginAssemblies.ToArray(), false);
            this.pluginView.ResumeLayout();
            this.pluginView.PluginRemoved += new PluginOptionEventHandler(this.pluginView_PluginRemoved);
            this.pluginView.PluginOptionRequired += new PluginOptionEventHandler(this.pluginView_PluginOptionRequired);
            this.pluginView.PluginAboutRequired += new PluginOptionEventHandler(this.pluginView_PluginAboutRequired);
            this.pluginView.QueryPluginInfoHasOption += new PluginInfoHasOption(this.pluginView_QueryPluginInfoHasOption);
            this.pluginView.DragDropEx += new EventHandler(this.pluginView_DragDropEx);
        }

        private static void InitializeStaticFields() {
            if(!fInitialized) {
                fInitialized = true;
                MIA_GROUPSEP = new MenuItemArguments("sep", null, null, 0, MenuGenre.Group);
                RefreshTexts();
                QTUtility.ImageListGlobal.Images.Add("groupRoot", Resources_Image.imgGroupRoot);
                QTUtility.ImageListGlobal.Images.Add("userAppsRoot", Resources_Image.imgUserAppsRoot);
                QTUtility.ImageListGlobal.Images.Add("noimage", Resources_Image.icoEmpty);
            }
        }

        private void InitializeTreeView_Group() {
            this.tnGroupsRoot = new TreeNode(ResOpt_Genre[4]);
            this.tnGroupsRoot.ImageKey = this.tnGroupsRoot.SelectedImageKey = "groupRoot";
            this.treeViewGroup.BeginUpdate();
            foreach(string str in QTUtility.GroupPathsDic.Keys) {
                string str2 = QTUtility.GroupPathsDic[str];
                if(str2.Length > 0) {
                    TreeNode node = new TreeNode(str);
                    if(QTUtility.StartUpGroupList.Contains(str)) {
                        node.NodeFont = this.fntStartUpGroup;
                    }
                    bool flag = true;
                    foreach(string str3 in str2.Split(QTUtility.SEPARATOR_CHAR)) {
                        string text = QTUtility2.MakePathDisplayText(str3, true);
                        TreeNode node2 = new TreeNode(text);
                        node2.Name = str3;
                        node2.ToolTipText = text;
                        node2.ImageKey = node2.SelectedImageKey = QTUtility.GetImageKey(str3, null);
                        node.Nodes.Add(node2);
                        if(flag) {
                            node.ImageKey = node.SelectedImageKey = node2.ImageKey;
                            flag = false;
                        }
                    }
                    if(node.Nodes.Count > 0) {
                        this.tnGroupsRoot.Nodes.Add(node);
                        int keyShortcut = 0;
                        if(QTUtility.dicGroupNamesAndKeys.ContainsKey(str)) {
                            keyShortcut = QTUtility.dicGroupNamesAndKeys[str];
                        }
                        node.Tag = new MenuItemArguments(str, null, null, keyShortcut, MenuGenre.Group);
                    }
                    continue;
                }
                TreeNode node3 = new TreeNode("----------- Separator -----------");
                node3.Tag = MIA_GROUPSEP;
                node3.ForeColor = SystemColors.GrayText;
                node3.ImageKey = node3.SelectedImageKey = "noimage";
                this.tnGroupsRoot.Nodes.Add(node3);
            }
            this.treeViewGroup.Nodes.Add(this.tnGroupsRoot);
            this.treeViewGroup.SelectedNode = this.tnGroupsRoot;
            this.tnGroupsRoot.Expand();
            this.treeViewGroup.EndUpdate();
        }

        private void InitializeTreeView_UserApps() {
            this.btnAddVFolder_app.Image = QTUtility.ImageListGlobal.Images["folder"];
            this.tnRoot_UserApps = new TreeNode(ResOpt_Genre[5]);
            this.tnRoot_UserApps.ImageKey = this.tnRoot_UserApps.SelectedImageKey = "userAppsRoot";
            this.treeViewUserApps.BeginUpdate();
            foreach(string str in QTUtility.UserAppsDic.Keys) {
                string[] appVals = QTUtility.UserAppsDic[str];
                if(appVals != null) {
                    if((appVals.Length == 3) || (appVals.Length == 4)) {
                        this.tnRoot_UserApps.Nodes.Add(CreateUserAppNode(str, appVals));
                    }
                    continue;
                }
                using(RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Quizo\QTTabBar\UserApps\" + str)) {
                    TreeNode node;
                    if((key != null) && CreateUserAppNode_Sub(str, key, out node)) {
                        this.tnRoot_UserApps.Nodes.Add(node);
                    }
                    continue;
                }
            }
            this.treeViewUserApps.Nodes.Add(this.tnRoot_UserApps);
            this.tnRoot_UserApps.Expand();
            this.treeViewUserApps.EndUpdate();
        }

        private static bool IsInvalidShortcutKey(Keys key, Keys modKeys) {
            switch(key) {
                case Keys.HanjaMode:
                case Keys.IMEConvert:
                case Keys.IMENonconvert:
                case Keys.ProcessKey:
                case Keys.Return:
                case Keys.ShiftKey:
                case Keys.ControlKey:
                case Keys.Menu:
                case Keys.KanaMode:
                case Keys.None:
                    return true;

                default: {
                        bool flag = (modKeys & Keys.Control) == Keys.Control;
                        bool flag2 = (modKeys & Keys.Shift) == Keys.Shift;
                        bool flag3 = (modKeys & Keys.Alt) == Keys.Alt;
                        if((flag && !flag2) && !flag3) {
                            switch(key) {
                                case Keys.A:
                                case Keys.C:
                                case Keys.V:
                                case Keys.X:
                                case Keys.Z:
                                    SystemSounds.Hand.Play();
                                    return true;
                            }
                        }
                        else if((!flag && !flag2) && flag3) {
                            switch(key) {
                                case Keys.Left:
                                case Keys.Right:
                                case Keys.F4:
                                    SystemSounds.Hand.Play();
                                    return true;
                            }
                        }
                        break;
                    }
            }
            return false;
        }

        private void lblVer_Click(object sender, EventArgs e) {
            try {
                Process.Start(Resources_String.SiteURL);
            }
            catch {
            }
        }

        private void listView_NoCapture_AfterLabelEdit(object sender, LabelEditEventArgs e) {
            this.fNowListViewItemEditing = false;
            ListViewItem item = this.listView_NoCapture.Items[e.Item];
            item.Selected = true;
            if(e.Label != null) {
                bool flag = false;
                try {
                    flag = ((Path.IsPathRooted(e.Label) && !e.Label.StartsWith("/")) && !e.Label.StartsWith(@"\\\")) && (e.Label.StartsWith(@"\\") || !e.Label.StartsWith(@"\"));
                }
                catch {
                }
                bool flag2 = false;
                int index = -1;
                if(!flag) {
                    index = Array.IndexOf<string>(arrSpecialFolderDipNms, e.Label);
                    flag2 = index != -1;
                }
                if((e.Label.Length > 0) && (flag || flag2)) {
                    if(flag2) {
                        item.Name = ShellMethods.GetSpecialFolderCLSID(arrSpecialFolderCSIDLs[index], false);
                    }
                    else {
                        item.Name = e.Label;
                    }
                }
                else {
                    SystemSounds.Asterisk.Play();
                    e.CancelEdit = true;
                    item.BeginEdit();
                }
            }
        }

        private void listView_NoCapture_BeforeLabelEdit(object sender, LabelEditEventArgs e) {
            this.fNowListViewItemEditing = true;
        }

        private void listView_NoCapture_ItemActivate(object sender, EventArgs e) {
            if(this.listView_NoCapture.SelectedItems.Count > 0) {
                ListViewItem item = this.listView_NoCapture.SelectedItems[0];
                this.listView_NoCapture.SelectedItems.Clear();
                item.BeginEdit();
            }
        }

        private void listView_NoCapture_KeyDown(object sender, KeyEventArgs e) {
            if(this.listView_NoCapture.SelectedItems.Count > 0) {
                Keys keyCode = e.KeyCode;
                if(keyCode != Keys.Delete) {
                    if(keyCode != Keys.F2) {
                        return;
                    }
                }
                else {
                    foreach(ListViewItem item in this.listView_NoCapture.SelectedItems) {
                        this.listView_NoCapture.Items.Remove(item);
                    }
                    return;
                }
                ListViewItem item2 = this.listView_NoCapture.SelectedItems[0];
                this.listView_NoCapture.SelectedItems.Clear();
                item2.BeginEdit();
            }
        }

        private void listView_NoCapture_SelectedIndexChanged(object sender, EventArgs e) {
            this.btnRemove_NoCapture.Enabled = this.listView_NoCapture.SelectedItems.Count > 0;
        }

        private void listViewKeyboard_KeyPress(object sender, KeyPressEventArgs e) {
            e.Handled = true;
        }

        private void listViewKeyboard_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) {
            if(!IsInvalidShortcutKey(e.KeyCode, e.Modifiers) && (this.listViewKeyboard.SelectedItems.Count == 1)) {
                ListViewItem lviCurrent = this.listViewKeyboard.SelectedItems[0];
                if((lviCurrent.Tag != null) && (((Keys)lviCurrent.Tag) != e.KeyData)) {
                    if(e.KeyCode == Keys.Escape) {
                        lviCurrent.SubItems[1].Text = " - ";
                        lviCurrent.Tag = Keys.None;
                    }
                    else if(((e.Modifiers != Keys.None) || ((Keys.F1 <= e.KeyCode) && (e.KeyCode <= Keys.F24))) && ((this.CheckExistance_Shortcuts(e.KeyData, lviCurrent) && this.CheckExistance_UserAppKey(e.KeyData, this.tnRoot_UserApps, null)) && this.CheckExistance_GroupKey(e.KeyData, null))) {
                        lviCurrent.SubItems[1].Text = QTUtility2.MakeKeyString(e.KeyData);
                        lviCurrent.Tag = e.KeyData;
                    }
                    e.IsInputKey = false;
                }
            }
        }

        private void numericUpDownMax_ValueChanged(object sender, EventArgs e) {
            if(sender == this.nudTabWidthMax) {
                if(this.nudTabWidthMax.Value < this.nudTabWidthMin.Value) {
                    this.nudTabWidthMax.Value = this.nudTabWidthMin.Value;
                }
            }
            else if(this.nudTabWidthMin.Value > this.nudTabWidthMax.Value) {
                this.nudTabWidthMin.Value = this.nudTabWidthMax.Value;
            }
        }

        protected override void OnResize(EventArgs e) {
            base.OnResize(e);
            TabPage selectedTab = this.tabControl1.SelectedTab;
            if(selectedTab != null) {
                selectedTab.Invalidate();
            }
        }

        private void OptionsDialog_FormClosing(object sender, FormClosingEventArgs e) {
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar")) {
                if(key != null) {
                    Rectangle bounds = base.Bounds;
                    if(base.WindowState == FormWindowState.Minimized) {
                        bounds = base.RestoreBounds;
                    }
                    int[] array = new int[] { bounds.Left, bounds.Top, bounds.Width, bounds.Height };
                    QTUtility2.WriteRegBinary<int>(array, "OptionWindowBounds", key);
                }
            }
            if(this.callBack != null) {
                this.callBack.BeginInvoke(DialogResult.Cancel, null, null);
                this.callBack = null;
            }
        }

        public void OwnerWindowClosing() {
            this.callBack = null;
            base.Close();
        }

        private void pluginView_DragDropEx(object sender, EventArgs e) {
            bool flag = true;
            foreach(string str in this.pluginView.DroppedFiles) {
                if(!string.IsNullOrEmpty(str)) {
                    PluginAssembly assembly = new PluginAssembly(str);
                    if(assembly.PluginInfosExist) {
                        this.CreatePluginViewItem(new PluginAssembly[] { assembly }, true);
                        if(flag) {
                            flag = false;
                            this.SelectPluginBottom();
                        }
                    }
                }
            }
        }

        private void pluginView_PluginAboutRequired(object sender, PluginOptionEventArgs e) {
            PluginInformation pluginInfo = e.PluginViewItem.PluginInfo;
            string strMessage = pluginInfo.Name + Environment.NewLine + pluginInfo.Version + Environment.NewLine + pluginInfo.Author + Environment.NewLine + Environment.NewLine + "\"" + pluginInfo.Path + "\"";
            MessageForm.Show(base.Handle, strMessage, string.Format(PluginView.MNU_PLUGINABOUT, pluginInfo.Name), MessageBoxIcon.Asterisk, 0xea60);
        }

        private void pluginView_PluginOptionRequired(object sender, PluginOptionEventArgs e) {
            Plugin plugin;
            if((this.pluginManager.TryGetPlugin(e.PluginViewItem.PluginInfo.PluginID, out plugin) && (plugin != null)) && (plugin.Instance != null)) {
                try {
                    plugin.Instance.OnOption();
                }
                catch(Exception exception) {
                    PluginManager.HandlePluginException(exception, base.Handle, plugin.PluginInformation.Name, "Open plugin option.");
                    QTUtility2.MakeErrorLog(exception, "Error at Plugin: " + e.PluginViewItem.PluginInfo.Name, true);
                }
            }
        }

        private void pluginView_PluginRemoved(object sender, PluginOptionEventArgs e) {
            PluginAssembly pluingAssembly = e.PluginViewItem.PluingAssembly;
            if(pluingAssembly.PluginInformations.Count > 1) {
                string str = string.Empty;
                foreach(PluginInformation information in pluingAssembly.PluginInformations) {
                    str = str + " " + information.Name + ",";
                }
                str = str.TrimEnd(new char[] { ',' });
                if(DialogResult.OK != MessageBox.Show(string.Format(RES_REMOVEPLGIN, str), string.Empty, MessageBoxButtons.OKCancel, MessageBoxIcon.Question)) {
                    e.Cancel = true;
                    return;
                }
                this.pluginView.RemovePluginsRange(pluingAssembly.PluginInformations.ToArray());
            }
            this.DeletePluginAssembly(pluingAssembly);
        }

        private bool pluginView_QueryPluginInfoHasOption(PluginInformation pi) {
            Plugin plugin;
            if(this.pluginManager.TryGetPlugin(pi.PluginID, out plugin)) {
                try {
                    return plugin.Instance.HasOption;
                }
                catch(Exception exception) {
                    PluginManager.HandlePluginException(exception, base.Handle, pi.Name, "Checking if the plugin has option.");
                    QTUtility2.MakeErrorLog(exception, "Error at Plugin: " + pi.Name, true);
                }
            }
            return false;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            if((this.tabControl1.SelectedIndex == 7) && this.listViewKeyboard.Focused) {
                bool flag = (keyData & Keys.Control) == Keys.Control;
                bool flag2 = (keyData & Keys.Shift) == Keys.Shift;
                bool flag3 = (keyData & Keys.Alt) == Keys.Alt;
                if(flag3) {
                    return true;
                }
                if((keyData == (Keys.Control | Keys.Tab)) || (keyData == (Keys.Control | Keys.Shift | Keys.Tab))) {
                    return true;
                }
                if((flag || flag2) || flag3) {
                    if((((keyData & Keys.Up) == Keys.Up) || ((keyData & Keys.Down) == Keys.Down)) || (((keyData & Keys.Left) == Keys.Left) || ((keyData & Keys.Right) == Keys.Right))) {
                        return true;
                    }
                    if((((keyData & Keys.Home) == Keys.Home) || ((keyData & Keys.End) == Keys.End)) || (((keyData & Keys.Next) == Keys.Next) || ((keyData & Keys.Prior) == Keys.Prior))) {
                        return true;
                    }
                }
            }
            if((base.ActiveControl == this.tbUserAppKey) || (base.ActiveControl == this.tbGroupKey)) {
                return ((keyData != Keys.Tab) && (keyData != (Keys.Shift | Keys.Tab)));
            }
            if((keyData == Keys.Return) && (base.ActiveControl is System.Windows.Forms.Button)) {
                ((System.Windows.Forms.Button)base.ActiveControl).PerformClick();
                return true;
            }
            if((keyData != Keys.Escape) && (keyData != Keys.Return)) {
                return base.ProcessCmdKey(ref msg, keyData);
            }
            switch(this.tabControl1.SelectedIndex) {
                case 4:
                    if(!this.fNowTreeNodeEditing) {
                        break;
                    }
                    return base.ProcessCmdKey(ref msg, keyData);

                case 5:
                    if(!this.fNowTreeNodeEditing) {
                        if(keyData != Keys.Return) {
                            break;
                        }
                        if(base.ActiveControl == this.tbPath) {
                            this.tbArgs.Focus();
                            return true;
                        }
                        if(base.ActiveControl == this.tbArgs) {
                            this.tbWorking.Focus();
                            return true;
                        }
                        if(base.ActiveControl == this.tbWorking) {
                            this.tbPath.Focus();
                            return true;
                        }
                        if(base.ActiveControl != this.treeViewUserApps) {
                            break;
                        }
                        if(this.tbPath.Enabled) {
                            this.tbPath.Focus();
                        }
                        return true;
                    }
                    return base.ProcessCmdKey(ref msg, keyData);

                case 7:
                    if(!this.listViewKeyboard.Focused) {
                        break;
                    }
                    return true;

                case 8:
                    if((base.ActiveControl != this.cmbTextExts) && (base.ActiveControl != this.cmbImgExts)) {
                        break;
                    }
                    return false;

                case 9:
                    if(!this.fNowListViewItemEditing) {
                        if((keyData == Keys.Return) && this.listView_NoCapture.Focused) {
                            return base.ProcessCmdKey(ref msg, keyData);
                        }
                        break;
                    }
                    return base.ProcessCmdKey(ref msg, keyData);
            }
            if(keyData == Keys.Escape) {
                base.Close();
            }
            else {
                this.Save(false);
            }
            return true;
        }

        public static void RefreshTexts() {
            ResOpt = QTUtility.TextResourcesDic["TabBar_Option"];
            ResOpt_DropDown = QTUtility.TextResourcesDic["TabBar_Option_DropDown"];
            ResOpt_Genre = QTUtility.TextResourcesDic["TabBar_Option_Genre"];
        }

        private void RemovePluginShortcutKeys(string pluginID) {
            this.listViewKeyboard.BeginUpdate();
            ListViewGroup group = this.listViewKeyboard.Groups[pluginID];
            if(group != null) {
                List<ListViewItem> list = new List<ListViewItem>();
                foreach(ListViewItem item in group.Items) {
                    list.Add(item);
                }
                foreach(ListViewItem item2 in list) {
                    this.listViewKeyboard.Items.Remove(item2);
                }
                this.listViewKeyboard.Groups.Remove(group);
            }
            this.listViewKeyboard.EndUpdate();
        }

        private void Save(bool fApply) {
            this.SaveSettings(fApply);
            if(this.callBack != null) {
                this.callBack.BeginInvoke(fApply ? DialogResult.Yes : DialogResult.OK, null, null);
            }
            if(!fApply) {
                this.callBack = null;
                base.Close();
            }
        }

        private void SaveGroupTreeView(RegistryKey rkUser) {
            QTUtility.GroupPathsDic.Clear();
            QTUtility.StartUpGroupList.Clear();
            QTUtility.dicGroupShortcutKeys.Clear();
            QTUtility.dicGroupNamesAndKeys.Clear();
            List<PluginKey> list = new List<PluginKey>();
            int num = 1;
            foreach(TreeNode node in this.tnGroupsRoot.Nodes) {
                MenuItemArguments tag = (MenuItemArguments)node.Tag;
                if(tag == MIA_GROUPSEP) {
                    QTUtility.GroupPathsDic["Separator" + num++] = string.Empty;
                }
                else if(node.Nodes.Count > 0) {
                    string text = node.Text;
                    if(text.Length > 0) {
                        string str2 = string.Empty;
                        foreach(TreeNode node2 in node.Nodes) {
                            if(node2.Name.Length > 0) {
                                str2 = str2 + node2.Name + ";";
                            }
                        }
                        if(str2.Length > 0) {
                            str2 = str2.TrimEnd(QTUtility.SEPARATOR_CHAR);
                            string item = text.Replace(";", "_");
                            QTUtility.GroupPathsDic[item] = str2;
                            if(node.NodeFont == this.fntStartUpGroup) {
                                QTUtility.StartUpGroupList.Add(item);
                            }
                            if(tag.KeyShortcut != 0) {
                                if(tag.KeyShortcut > 0x100000) {
                                    QTUtility.dicGroupShortcutKeys[tag.KeyShortcut] = item;
                                }
                                QTUtility.dicGroupNamesAndKeys[item] = tag.KeyShortcut;
                                list.Add(new PluginKey(item, new int[] { tag.KeyShortcut }));
                            }
                        }
                    }
                }
            }
            rkUser.DeleteSubKey("Groups", false);
            using(RegistryKey key = rkUser.CreateSubKey("Groups")) {
                if(key != null) {
                    foreach(string str4 in QTUtility.GroupPathsDic.Keys) {
                        key.SetValue(str4, QTUtility.GroupPathsDic[str4]);
                    }
                }
            }
            rkUser.DeleteValue("ShortcutKeys_Group", false);
            if(list.Count > 0) {
                QTUtility2.WriteRegBinary<PluginKey>(list.ToArray(), "ShortcutKeys_Group", rkUser);
            }
        }

        private void SaveNoCapturePaths(RegistryKey rkUser) {
            string str = string.Empty;
            QTUtility.NoCapturePathsList.Clear();
            foreach(ListViewItem item in this.listView_NoCapture.Items) {
                if(item.Name.Length > 0) {
                    string str2 = item.Name.Trim();
                    if((str2.Length > 0) && (str2 != "Enter path")) {
                        str = str + str2 + ";";
                        QTUtility.NoCapturePathsList.Add(str2);
                    }
                }
            }
            str = str.TrimEnd(QTUtility.SEPARATOR_CHAR);
            rkUser.SetValue("NoCaptureAt", str);
        }

        private void SavePlugins(bool fApply) {
            this.lstPluginAssembliesUserAdded.Clear();
            if(File.Exists(this.textBoxPluginLang.Text)) {
                QTUtility.Path_PluginLangFile = this.textBoxPluginLang.Text;
            }
            else {
                QTUtility.Path_PluginLangFile = string.Empty;
            }
            List<PluginAssembly> list = new List<PluginAssembly>();
            foreach(PluginViewItem item in this.pluginView.PluginViewItems) {
                PluginAssembly pluingAssembly = item.PluingAssembly;
                if(!list.Contains(pluingAssembly)) {
                    pluingAssembly.Enabled = false;
                    list.Add(pluingAssembly);
                }
                PluginInformation pluginInfo = item.PluginInfo;
                pluginInfo.Enabled = item.PluginEnabled;
                if(pluginInfo.Enabled) {
                    pluingAssembly.Enabled = true;
                }
                else {
                    this.RemovePluginShortcutKeys(pluginInfo.PluginID);
                }
            }
            if(this.callBack != null) {
                this.callBack(list);
            }
            if(fApply) {
                this.pluginView.NotifyApplied();
                string[] strArray = QTUtility.TextResourcesDic["ShortcutKeys_Groups"];
                foreach(Plugin plugin in this.pluginManager.Plugins) {
                    PluginInformation pluginInformation = plugin.PluginInformation;
                    if((((pluginInformation != null) && pluginInformation.Enabled) && ((pluginInformation.PluginType == PluginType.Background) || (pluginInformation.PluginType == PluginType.BackgroundMultiple))) && (this.listViewKeyboard.Groups[pluginInformation.PluginID] == null)) {
                        string[] shortcutKeyActions = pluginInformation.ShortcutKeyActions;
                        if((shortcutKeyActions != null) && (shortcutKeyActions.Length > 0)) {
                            ListViewGroup group2 = this.listViewKeyboard.Groups.Add(pluginInformation.PluginID, pluginInformation.Name + " (" + strArray[1] + ")");
                            for(int i = 0; i < shortcutKeyActions.Length; i++) {
                                ListViewItem item2 = new ListViewItem(new string[] { shortcutKeyActions[i], " - " });
                                item2.Checked = false;
                                item2.Group = group2;
                                item2.Tag = 0;
                                item2.Name = pluginInformation.PluginID;
                                this.listViewKeyboard.Items.Add(item2);
                            }
                        }
                    }
                }
            }
        }

        private void SaveSettings(bool fApply) {
            bool flag = QTUtility.CheckConfig(Settings.HashFullPath);
            bool flag2 = QTUtility.CheckConfig(Settings.HashClearOnClose);
            bool flag3 = QTUtility.CheckConfig(Settings.ShowHashResult);
            bool flag4 = QTUtility.CheckConfig(Settings.HashTopMost);
            QTUtility.ConfigValues[0] = 0;
            QTUtility.ConfigValues[5] = 0;
            QTUtility.ConfigValues[6] = 0;
            QTUtility.ConfigValues[7] = 0;
            QTUtility.ConfigValues[8] = 0;
            QTUtility.ConfigValues[9] = 0;
            QTUtility.ConfigValues[10] = 0;
            QTUtility.ConfigValues[11] = 0;
            QTUtility.ConfigValues[13] = 0;
            QTUtility.ConfigValues[14] = 0;
            if(this.chbActivateNew.Checked) {
                QTUtility.SetConfigAt(Settings.ActivateNewTab, true);
            }
            if(this.chbDontOpenSame.Checked) {
                QTUtility.SetConfigAt(Settings.DontOpenSame, true);
            }
            if(this.chbCloseWhenGroup.Checked) {
                QTUtility.SetConfigAt(Settings.CloseWhenGroup, true);
            }
            if(this.chbRestoreClosed.Checked) {
                QTUtility.SetConfigAt(Settings.RestoreClosed, true);
            }
            if(this.chbShowTooltip.Checked) {
                QTUtility.SetConfigAt(Settings.ShowTooltips, true);
            }
            if(this.chbNeverCloseWindow.Checked) {
                QTUtility.SetConfigAt(Settings.NeverCloseWindow, true);
            }
            if(this.chbNeverCloseWndLocked.Checked) {
                QTUtility.SetConfigAt(Settings.NeverCloseWndLocked, true);
            }
            QTUtility.ConfigValues[1] = (byte)this.cmbNewTabLoc.SelectedIndex;
            QTUtility.ConfigValues[2] = (byte)this.cmbActvClose.SelectedIndex;
            QTUtility.ConfigValues[3] = (byte)this.cmbTabDblClck.SelectedIndex;
            QTUtility.ConfigValues[4] = (byte)this.cmbBGDblClick.SelectedIndex;
            QTUtility.ConfigValues[12] = (byte)this.cmbTabWhlClck.SelectedIndex;
            QTUtility.Action_BarDblClick = this.textBoxAction_BarDblClck.Text;
            if(this.cmbTabSizeMode.SelectedIndex == 1) {
                QTUtility.SetConfigAt(Settings.FixedWidthTabs, true);
            }
            else if(this.cmbTabSizeMode.SelectedIndex == 2) {
                QTUtility.SetConfigAt(Settings.LimitedWidthTabs, true);
            }
            QTUtility.TabWidth = (int)this.nudTabWidth.Value;
            QTUtility.TabHeight = (int)this.nudTabHeight.Value;
            QTUtility.MaxTabWidth = (int)this.nudTabWidthMax.Value;
            QTUtility.MinTabWidth = (int)this.nudTabWidthMin.Value;
            if(this.chbNavBtn.Checked) {
                QTUtility.SetConfigAt(Settings.ShowNavButtons, true);
            }
            if(this.cmbNavBtn.SelectedIndex == 1) {
                QTUtility.SetConfigAt(Settings.NavButtonsOnRight, true);
            }
            if(this.chbX1X2.Checked) {
                QTUtility.SetConfigAt(Settings.CaptureX1X2, true);
            }
            if(this.chbBoldActv.Checked) {
                QTUtility.SetConfigAt(Settings.ActiveTabInBold, true);
            }
            if(this.chbUseTabSkin.Checked) {
                QTUtility.SetConfigAt(Settings.UseTabSkin, true);
            }
            if(!this.chbNoHistory.Checked) {
                QTUtility.SetConfigAt(Settings.NoHistory, true);
            }
            if(!this.chbWhlClick.Checked) {
                QTUtility.SetConfigAt(Settings.NoCaptureMidClick, true);
            }
            if(this.cmbWhlClick.SelectedIndex == 1) {
                QTUtility.SetConfigAt(Settings.MidClickNewWindow, true);
            }
            if(!this.chbNCADblClck.Checked) {
                QTUtility.SetConfigAt(Settings.NoDblClickUpLevel, true);
            }
            if(this.chbSaveExecuted.CheckState != CheckState.Unchecked) {
                if(this.chbSaveExecuted.CheckState == CheckState.Checked) {
                    QTUtility.SetConfigAt(Settings.AllRecentFiles, true);
                }
            }
            else {
                QTUtility.SetConfigAt(Settings.NoRecentFiles, true);
            }
            if(!this.chbBlockProcess.Checked) {
                QTUtility.SetConfigAt(Settings.DontCaptureNewWnds, true);
            }
            if(this.chbRestoreLocked.Checked) {
                QTUtility.SetConfigAt(Settings.RestoreLockedTabs, true);
            }
            if(!this.chbFoldrTree.Checked) {
                QTUtility.SetConfigAt(Settings.NoNewWndFolderTree, true);
            }
            if(this.chbWndUnresizable.Checked) {
                QTUtility.SetConfigAt(Settings.NoWindowResizing, true);
            }
            if(flag3) {
                QTUtility.SetConfigAt(Settings.ShowHashResult, true);
            }
            QTUtility.TabTextColor_Active = this.btnActTxtClr.ForeColor;
            QTUtility.TabTextColor_Inactv = this.btnInactTxtClr.ForeColor;
            QTUtility.TabHiliteColor = this.btnHiliteClsc.ForeColor;
            QTUtility.RebarBGColor = this.btnToolBarBGClr.BackColor;
            QTUtility.TabTextColor_ActivShdw = this.btnShadowAct.ForeColor;
            QTUtility.TabTextColor_InAtvShdw = this.btnShadowIna.ForeColor;
            if(flag4) {
                QTUtility.SetConfigAt(Settings.HashTopMost, true);
            }
            if(this.cmbMultiRow.SelectedIndex == 1) {
                QTUtility.SetConfigAt(Settings.MultipleRow1, true);
            }
            else if(this.cmbMultiRow.SelectedIndex == 2) {
                QTUtility.SetConfigAt(Settings.MultipleRow2, true);
            }
            if(this.chbHideMenu.Checked) {
                QTUtility.SetConfigAt(Settings.HideMenuBar, true);
            }
            if(this.chbToolbarBGClr.Checked) {
                QTUtility.SetConfigAt(Settings.ToolbarBGColor, true);
            }
            if(this.chbFolderIcon.Checked) {
                QTUtility.SetConfigAt(Settings.FolderIcon, true);
            }
            if(this.chbBSUpOneLvl.Checked) {
                QTUtility.SetConfigAt(Settings.BackspaceUpLevel, true);
            }
            QTUtility.Path_TabImage = this.tbTabImagePath.Text;
            QTUtility.TabImageSizingMargin = this.tabImageSetting.SizingMargin;
            if(this.chbGridLine.Checked) {
                QTUtility.SetConfigAt(Settings.DetailsGridLines, true);
            }
            if(this.chbNoFulRowSelect.Checked ^ QTUtility.IsVista) {
                QTUtility.SetConfigAt(Settings.NoFullRowSelect, true);
            }
            if(!QTUtility.IsVista && !this.chbSelectWithoutExt.Checked) {
                QTUtility.SetConfigAt(Settings.ExtWhileRenaming, true);
            }
            if(flag) {
                QTUtility.SetConfigAt(Settings.HashFullPath, true);
            }
            if(this.chbAlternateColor.Checked) {
                QTUtility.SetConfigAt(Settings.AlternateRowColors, true);
            }
            QTUtility.ShellViewRowCOLORREF_Background = QTUtility2.MakeCOLORREF(this.btnAlternateColor.BackColor);
            QTUtility.ShellViewRowCOLORREF_Text = QTUtility2.MakeCOLORREF(this.btnAlternateColor_Text.ForeColor);
            if(this.chbWndRestrAlpha.Checked) {
                QTUtility.SetConfigAt(Settings.SaveTransparency, true);
            }
            if(this.chbShowPreview.Checked) {
                QTUtility.SetConfigAt(Settings.ShowTooltipPreviews, true);
            }
            if(this.chbPreviewMode.Checked) {
                QTUtility.SetConfigAt(Settings.PreviewsWithShift, true);
            }
            if(!this.chbSubDirTip.Checked) {
                QTUtility.SetConfigAt(Settings.NoShowSubDirTips, true);
            }
            if(this.chbSubDirTipMode.Checked) {
                QTUtility.SetConfigAt(Settings.SubDirTipsWithShift, true);
            }
            if(this.chbSubDirTipModeHidden.Checked) {
                QTUtility.SetConfigAt(Settings.SubDirTipsHidden, true);
            }
            if(this.chbSubDirTipModeFile.Checked) {
                QTUtility.SetConfigAt(Settings.SubDirTipsFiles, true);
            }
            if(!this.chbSubDirTipPreview.Checked) {
                QTUtility.SetConfigAt(Settings.SubDirTipsPreview, true);
            }
            if(flag2) {
                QTUtility.SetConfigAt(Settings.HashClearOnClose, true);
            }
            if(this.chbDD.Checked) {
                QTUtility.SetConfigAt(Settings.DragDropOntoTabs, true);
            }
            if(this.chbNoTabFromOuteside.Checked) {
                QTUtility.SetConfigAt(Settings.NoTabsFromOutside, true);
            }
            if(!this.chbAutoSubText.Checked) {
                QTUtility.SetConfigAt(Settings.NoRenameAmbTabs, true);
            }
            if(!this.chbHolizontalScroll.Checked) {
                QTUtility.SetConfigAt(Settings.HorizontalScroll, true);
            }
            if(!this.chbWhlChangeView.Checked) {
                QTUtility.SetConfigAt(Settings.CtrlWheelChangeView, true);
            }
            if(this.chbSubDirTipModeSystem.Checked) {
                QTUtility.SetConfigAt(Settings.SubDirTipsSystem, true);
            }
            if(this.chbSendToTray.Checked) {
                QTUtility.SetConfigAt(Settings.TrayOnClose, true);
            }
            if(!this.chbF2Selection.Checked) {
                QTUtility.SetConfigAt(Settings.F2Selection, true);
            }
            if(this.chbRebarBGImage.Checked) {
                QTUtility.SetConfigAt(Settings.RebarImage, true);
            }
            switch(this.cmbRebarBGImageMode.SelectedIndex) {
                case 1:
                    QTUtility.SetConfigAt(Settings.RebarImageTile, true);
                    break;

                case 2:
                    QTUtility.SetConfigAt(Settings.RebarImageActual, true);
                    break;

                case 3:
                    QTUtility.SetConfigAt(Settings.RebarImageStretch2, true);
                    break;
            }
            QTUtility.Path_RebarImage = this.tbRebarImagePath.Text;
            if(this.chbTabCloseButton.Checked) {
                QTUtility.SetConfigAt(Settings.ShowTabCloseButtons, true);
            }
            if(this.chbSubDirTipOnTab.Checked) {
                QTUtility.SetConfigAt(Settings.ShowSubDirTipOnTab, true);
            }
            if(this.chbTabCloseBtnAlt.Checked) {
                QTUtility.SetConfigAt(Settings.TabCloseBtnsWithAlt, true);
            }
            if(this.chbCursorLoop.Checked) {
                QTUtility.SetConfigAt(Settings.CursorLoop, true);
            }
            if(this.chbTabCloseBtnHover.Checked) {
                QTUtility.SetConfigAt(Settings.TabCloseBtnsOnHover, true);
            }
            if(this.chbSendToTrayOnMinimize.Checked) {
                QTUtility.SetConfigAt(Settings.TrayOnMinimize, true);
            }
            if(this.cmbTabTextAlignment.SelectedIndex == 1) {
                QTUtility.SetConfigAt(Settings.AlignTabTextCenter, true);
            }
            if(this.cmbMenuRenderer.SelectedIndex != 0) {
                QTUtility.SetConfigAt(Settings.NonDefaultMenu, true);
                if(this.cmbMenuRenderer.SelectedIndex == 2) {
                    QTUtility.SetConfigAt(Settings.XPStyleMenus, true);
                }
            }
            if(!this.chbTreeShftWhlTab.Checked) {
                QTUtility.SetConfigAt(Settings.NoMidClickTree, true);
            }
            if(!this.chbTabSwitcher.Checked) {
                QTUtility.SetConfigAt(Settings.NoTabSwitcher, true);
            }
            if(this.chbTabTitleShadow.Checked) {
                QTUtility.SetConfigAt(Settings.TabTitleShadows, true);
            }
            if(this.chbAutoUpdate.Checked) {
                QTUtility.SetConfigAt(Settings.AutoUpdate, true);
            }
            if(!this.chbRemoveOnSeparate.Checked) {
                QTUtility.SetConfigAt(Settings.KeepOnSeparate, true);
            }
            if(this.chbDriveLetter.Checked) {
                QTUtility.SetConfigAt(Settings.ShowDriveLetters, true);
            }
            if(!this.chbPlaySound.Checked) {
                QTUtility.SetConfigAt(Settings.DisableSound, true);
            }
            if(!this.chbPreviewInfo.Checked) {
                QTUtility.SetConfigAt(Settings.PreviewInfo, true);
            }
            QTUtility.MaxCount_History = (int)this.nudMaxUndo.Value;
            QTUtility.MaxCount_Executed = (int)this.nudMaxRecentFile.Value;
            if(File.Exists(this.textBoxLang.Text)) {
                QTUtility.Path_LanguageFile = this.textBoxLang.Text;
            }
            else {
                QTUtility.Path_LanguageFile = string.Empty;
            }
            QTUtility.PreviewMaxWidth = (int)this.nudPreviewMaxWidth.Value;
            QTUtility.PreviewMaxHeight = (int)this.nudPreviewMaxHeight.Value;
            if(this.btnPreviewFont.Font != this.Font) {
                QTUtility.PreviewFontName = this.btnPreviewFont.Font.Name;
                QTUtility.PreviewFontSize = this.btnPreviewFont.Font.SizeInPoints;
            }
            else {
                QTUtility.PreviewFontName = null;
                QTUtility.PreviewFontSize = 0f;
            }
            IDLWrapper.iPingTimeOutMS = ((int)this.nudNetworkTimeOut.Value) * 0x3e8;
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar")) {
                this.SaveGroupTreeView(key);
                this.SaveUserAppsTreeView(key);
                this.SavePlugins(fApply);
                this.SaveShortcuts();
                this.SaveNoCapturePaths(key);
            }
            List<string> list = new List<string>();
            List<string> list2 = new List<string>();
            for(int i = 1; i < this.cmbTextExts.Items.Count; i++) {
                list.Add(this.cmbTextExts.Items[i].ToString());
            }
            for(int j = 1; j < this.cmbImgExts.Items.Count; j++) {
                list2.Add(this.cmbImgExts.Items[j].ToString());
            }
            WriteRegistry(this.btnTabFont.Font, list.ToArray(), list2.ToArray());
        }

        private void SaveShortcuts() {
            List<int> list = new List<int>();
            Dictionary<string, List<int>> dictionary = new Dictionary<string, List<int>>();
            for(int i = 0; i < this.listViewKeyboard.Items.Count; i++) {
                ListViewItem item = this.listViewKeyboard.Items[i];
                bool flag = item.Checked;
                int num2 = ((int)((Keys)item.Tag)) | (flag ? 0x100000 : 0);
                if(item.Group.Name == "general") {
                    list.Add(num2);
                }
                else {
                    string name = item.Name;
                    if(this.pluginManager.PluginInstantialized(name)) {
                        if(!dictionary.ContainsKey(name)) {
                            dictionary[name] = new List<int>();
                        }
                        dictionary[name].Add(num2);
                    }
                }
            }
            QTUtility.ShortcutKeys = list.ToArray();
            List<PluginKey> list2 = new List<PluginKey>();
            foreach(string str2 in dictionary.Keys) {
                list2.Add(new PluginKey(str2, dictionary[str2].ToArray()));
            }
            QTUtility.dicPluginShortcutKeys.Clear();
            List<int> list3 = new List<int>();
            foreach(PluginKey key in list2) {
                QTUtility.dicPluginShortcutKeys[key.PluginID] = key.Keys;
                for(int j = 0; j < key.Keys.Length; j++) {
                    list3.Add(key.Keys[j]);
                }
            }
            QTUtility.PluginShortcutKeysCache = list3.ToArray();
            using(RegistryKey key2 = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar\Plugins")) {
                QTUtility2.WriteRegBinary<PluginKey>(list2.ToArray(), "ShortcutKeys", key2);
            }
        }

        private static void SaveUserAppsRegistry(TreeNode tn, RegistryKey rk, ref int separatorIndex, bool fRoot) {
            if(tn.Text == "----------- Separator -----------") {
                string[] array = new string[] { string.Empty, string.Empty, string.Empty, string.Empty };
                string regValueName = "Separator" + ((int)separatorIndex++);
                if(fRoot) {
                    QTUtility.UserAppsDic[regValueName] = array;
                }
                QTUtility2.WriteRegBinary<string>(array, regValueName, rk);
            }
            else if(tn.Text.Length > 0) {
                MenuItemArguments tag = (MenuItemArguments)tn.Tag;
                if(!string.IsNullOrEmpty(tag.Path)) {
                    string[] strArray2 = new string[] { tag.Path, tag.Argument, tag.WorkingDirectory, tag.KeyShortcut.ToString() };
                    if(tag.KeyShortcut > 0x100000) {
                        QTUtility.dicUserAppShortcutKeys[tag.KeyShortcut] = tag;
                    }
                    if(fRoot) {
                        QTUtility.UserAppsDic[tn.Text] = strArray2;
                    }
                    QTUtility2.WriteRegBinary<string>(strArray2, tn.Text, rk);
                }
            }
        }

        private static void SaveUserAppsRegistry_Sub(TreeNode tn, RegistryKey rk) {
            int separatorIndex = 1;
            using(RegistryKey key = rk.CreateSubKey(tn.Text)) {
                foreach(TreeNode node in tn.Nodes) {
                    if(node.Tag != null) {
                        SaveUserAppsRegistry(node, key, ref separatorIndex, false);
                    }
                    else if(node.Nodes.Count > 0) {
                        SaveUserAppsRegistry_Sub(node, key);
                    }
                }
            }
            rk.SetValue(tn.Text, new byte[0]);
        }

        private void SaveUserAppsTreeView(RegistryKey rkUser) {
            QTUtility.UserAppsDic.Clear();
            QTUtility.dicUserAppShortcutKeys.Clear();
            int separatorIndex = 1;
            bool flag = false;
            using(RegistryKey key = rkUser.OpenSubKey("UserApps", false)) {
                flag = key != null;
            }
            try {
                if(flag) {
                    rkUser.DeleteSubKeyTree("UserApps");
                }
            }
            catch {
            }
            using(RegistryKey key2 = rkUser.CreateSubKey("UserApps")) {
                foreach(TreeNode node in this.tnRoot_UserApps.Nodes) {
                    if(node.Tag != null) {
                        SaveUserAppsRegistry(node, key2, ref separatorIndex, true);
                    }
                    else if(node.Nodes.Count > 0) {
                        QTUtility.UserAppsDic[node.Text] = null;
                        SaveUserAppsRegistry_Sub(node, key2);
                    }
                }
            }
        }

        private void SelectPluginBottom() {
            int index = this.pluginView.ItemsCount - 1;
            if(index > 0) {
                this.pluginView.SelectPlugin(index);
            }
        }

        private void SetValues() {
            bool isSupported = VisualStyleRenderer.IsSupported;
            this.chbActivateNew.Checked = (QTUtility.ConfigValues[0] & 0x80) == 0x80;
            this.chbDontOpenSame.Checked = (QTUtility.ConfigValues[0] & 0x40) == 0x40;
            this.chbCloseWhenGroup.Checked = (QTUtility.ConfigValues[0] & 0x20) == 0x20;
            this.chbRestoreClosed.Checked = (QTUtility.ConfigValues[0] & 0x10) == 0x10;
            this.chbShowTooltip.Checked = (QTUtility.ConfigValues[0] & 8) == 8;
            this.chbNeverCloseWindow.Checked = (QTUtility.ConfigValues[0] & 2) == 2;
            this.chbNeverCloseWndLocked.Checked = (QTUtility.ConfigValues[0] & 1) == 1;
            this.cmbNewTabLoc.SelectedIndex = QTUtility.ConfigValues[1];
            this.cmbActvClose.SelectedIndex = QTUtility.ConfigValues[2];
            if(QTUtility.ConfigValues[3] > (this.cmbTabDblClck.Items.Count - 1)) {
                QTUtility.ConfigValues[3] = (byte)(this.cmbTabDblClck.Items.Count - 1);
            }
            if(QTUtility.ConfigValues[4] > (this.cmbBGDblClick.Items.Count - 1)) {
                QTUtility.ConfigValues[4] = (byte)(this.cmbBGDblClick.Items.Count - 1);
            }
            if(QTUtility.ConfigValues[12] > (this.cmbTabWhlClck.Items.Count - 1)) {
                QTUtility.ConfigValues[12] = 0;
            }
            this.cmbTabDblClck.SelectedIndex = QTUtility.ConfigValues[3];
            this.cmbBGDblClick.SelectedIndex = QTUtility.ConfigValues[4];
            this.cmbTabWhlClck.SelectedIndex = QTUtility.ConfigValues[12];
            this.cmbTabTextAlignment.SelectedIndex = QTUtility.CheckConfig(Settings.AlignTabTextCenter) ? 1 : 0;
            this.chbNavBtn.Checked = (QTUtility.ConfigValues[5] & 0x80) == 0x80;
            if((QTUtility.ConfigValues[5] & 0x40) == 0x40) {
                this.cmbNavBtn.SelectedIndex = 1;
            }
            else {
                this.cmbNavBtn.SelectedIndex = 0;
            }
            if((QTUtility.ConfigValues[5] & 0x80) == 0) {
                this.cmbNavBtn.Enabled = false;
            }
            this.chbX1X2.Checked = (QTUtility.ConfigValues[5] & 0x20) == 0x20;
            this.chbBoldActv.Checked = (QTUtility.ConfigValues[5] & 0x10) == 0x10;
            this.chbUseTabSkin.Checked = (QTUtility.ConfigValues[5] & 8) == 8;
            this.chbNoHistory.Checked = (QTUtility.ConfigValues[5] & 2) == 0;
            this.chbWhlClick.Checked = (QTUtility.ConfigValues[5] & 1) == 0;
            this.propertyGrid1.Enabled = this.btnHiliteClsc.Enabled = this.tbTabImagePath.Enabled = this.btnTabImage.Enabled = this.chbUseTabSkin.Checked;
            if(!isSupported) {
                this.chbUseTabSkin.Checked = this.btnHiliteClsc.Enabled = this.tbTabImagePath.Enabled = this.btnTabImage.Enabled = true;
                this.chbUseTabSkin.Enabled = false;
            }
            this.tbTabImagePath.Text = QTUtility.Path_TabImage;
            this.tabImageSetting.SizingMargin = QTUtility.TabImageSizingMargin;
            this.cmbWhlClick.SelectedIndex = ((QTUtility.ConfigValues[6] & 0x80) == 0x80) ? 1 : 0;
            this.cmbWhlClick.Enabled = this.chbWhlClick.Checked;
            this.chbNCADblClck.Checked = (QTUtility.ConfigValues[6] & 0x40) == 0;
            this.chbBlockProcess.Checked = (QTUtility.ConfigValues[6] & 0x10) == 0;
            this.chbRestoreLocked.Checked = (QTUtility.ConfigValues[6] & 8) == 8;
            this.chbFoldrTree.Checked = (QTUtility.ConfigValues[6] & 4) == 0;
            this.chbWndUnresizable.Checked = (QTUtility.ConfigValues[6] & 2) == 2;
            if(!QTUtility.CheckConfig(Settings.NoRecentFiles)) {
                if(QTUtility.CheckConfig(Settings.AllRecentFiles)) {
                    this.chbSaveExecuted.CheckState = CheckState.Checked;
                }
                else {
                    this.chbSaveExecuted.CheckState = CheckState.Indeterminate;
                }
            }
            else {
                this.chbSaveExecuted.CheckState = CheckState.Unchecked;
            }
            if(this.chbNeverCloseWndLocked.Checked) {
                this.chbRestoreLocked.Checked = false;
            }
            if((QTUtility.ConfigValues[7] & 0x20) == 0x20) {
                this.cmbMultiRow.SelectedIndex = 1;
            }
            else if((QTUtility.ConfigValues[7] & 0x10) == 0x10) {
                this.cmbMultiRow.SelectedIndex = 2;
            }
            else {
                this.cmbMultiRow.SelectedIndex = 0;
            }
            if((QTUtility.ConfigValues[0] & 4) == 4) {
                this.cmbTabSizeMode.SelectedIndex = 1;
                this.nudTabWidthMax.Enabled = this.nudTabWidthMin.Enabled = false;
            }
            else if((QTUtility.ConfigValues[5] & 4) == 4) {
                this.cmbTabSizeMode.SelectedIndex = 2;
            }
            else {
                this.cmbTabSizeMode.SelectedIndex = 0;
                this.nudTabWidth.Enabled = this.nudTabWidthMax.Enabled = this.nudTabWidthMin.Enabled = false;
            }
            this.nudTabWidthMax.Value = QTUtility.MaxTabWidth;
            this.nudTabWidthMin.Value = QTUtility.MinTabWidth;
            this.nudTabHeight.Value = QTUtility.TabHeight;
            if(QTUtility.TabWidth > this.nudTabWidth.Maximum) {
                this.nudTabWidth.Value = this.nudTabWidth.Maximum;
            }
            else if(QTUtility.TabWidth < this.nudTabWidth.Minimum) {
                this.nudTabWidth.Value = this.nudTabWidth.Minimum;
            }
            else {
                this.nudTabWidth.Value = QTUtility.TabWidth;
            }
            this.btnActTxtClr.ForeColor = QTUtility.TabTextColor_Active;
            this.btnInactTxtClr.ForeColor = QTUtility.TabTextColor_Inactv;
            this.btnHiliteClsc.ForeColor = QTUtility.TabHiliteColor;
            this.btnShadowAct.ForeColor = QTUtility.TabTextColor_ActivShdw;
            this.btnShadowIna.ForeColor = QTUtility.TabTextColor_InAtvShdw;
            this.chbTabTitleShadow.Checked = QTUtility.CheckConfig(Settings.TabTitleShadows);
            this.btnShadowAct.Enabled = this.btnShadowIna.Enabled = this.chbTabTitleShadow.Checked;
            this.textBoxAction_BarDblClck.Text = QTUtility.Action_BarDblClick;
            QTUtility.RefreshGroupsDic();
            this.InitializeTreeView_Group();
            QTUtility.RefreshUserAppDic(false);
            this.InitializeTreeView_UserApps();
            this.btnTabFont.Font = QTUtility.TabFont;
            if(QTUtility.CheckConfig(Settings.HideMenuBar)) {
                this.chbHideMenu.Checked = true;
            }
            if(QTUtility.IsVista) {
                this.chbBSUpOneLvl.Checked = (QTUtility.ConfigValues[7] & 1) == 1;
                this.chbWhlChangeView.Enabled = this.chbSelectWithoutExt.Enabled = this.chbTreeShftWhlTab.Enabled = false;
            }
            else {
                this.chbBSUpOneLvl.Enabled = false;
                this.chbSelectWithoutExt.Checked = (QTUtility.ConfigValues[8] & 0x20) == 0;
                this.chbTreeShftWhlTab.Checked = !QTUtility.CheckConfig(Settings.NoMidClickTree);
            }
            this.btnToolBarBGClr.Enabled = this.chbToolbarBGClr.Checked = (QTUtility.ConfigValues[7] & 4) == 4;
            this.btnToolBarBGClr.BackColor = QTUtility.RebarBGColor;
            this.chbFolderIcon.Checked = (QTUtility.ConfigValues[7] & 2) == 2;
            this.chbGridLine.Checked = (QTUtility.ConfigValues[8] & 0x80) == 0x80;
            this.chbNoFulRowSelect.Checked = ((QTUtility.ConfigValues[8] & 0x40) == 0x40) ^ QTUtility.IsVista;
            this.chbAlternateColor.Checked = (QTUtility.ConfigValues[8] & 8) == 8;
            this.btnAlternateColor.BackColor = QTUtility2.MakeColor(QTUtility.ShellViewRowCOLORREF_Background);
            this.btnAlternateColor_Text.ForeColor = QTUtility2.MakeColor(QTUtility.ShellViewRowCOLORREF_Text);
            this.btnAlternate_Default.Enabled = this.btnAlternateColor.Enabled = this.btnAlternateColor_Text.Enabled = this.chbAlternateColor.Checked;
            if(isSupported) {
                this.chbRebarBGImage.Checked = (QTUtility.ConfigValues[11] & 0x80) == 0x80;
                this.tbRebarImagePath.Text = QTUtility.Path_RebarImage;
                if((QTUtility.ConfigValues[11] & 0x40) == 0x40) {
                    this.cmbRebarBGImageMode.SelectedIndex = 1;
                }
                else if((QTUtility.ConfigValues[11] & 0x20) == 0x20) {
                    this.cmbRebarBGImageMode.SelectedIndex = 2;
                }
                else if(QTUtility.CheckConfig(Settings.RebarImageStretch2)) {
                    this.cmbRebarBGImageMode.SelectedIndex = 3;
                }
                else {
                    this.cmbRebarBGImageMode.SelectedIndex = 0;
                }
                this.cmbRebarBGImageMode.Enabled = this.btnRebarImage.Enabled = this.tbRebarImagePath.Enabled = this.chbRebarBGImage.Checked;
            }
            else {
                this.chbRebarBGImage.Enabled = this.tbRebarImagePath.Enabled = this.btnRebarImage.Enabled = this.cmbRebarBGImageMode.Enabled = false;
            }
            this.chbWndRestrAlpha.Checked = (QTUtility.ConfigValues[8] & 4) == 4;
            this.chbShowPreview.Checked = QTUtility.CheckConfig(Settings.ShowTooltipPreviews);
            this.chbPreviewMode.Checked = QTUtility.CheckConfig(Settings.PreviewsWithShift);
            this.chbPreviewInfo.Checked = !QTUtility.CheckConfig(Settings.PreviewInfo);
            this.nudPreviewMaxWidth.Enabled = this.nudPreviewMaxHeight.Enabled = this.cmbTextExts.Enabled = this.btnAddTextExt.Enabled = this.btnDelTextExt.Enabled = this.btnDefaultTextExt.Enabled = this.cmbImgExts.Enabled = this.btnAddImgExt.Enabled = this.btnDelImgExt.Enabled = this.btnDefaultImgExt.Enabled = this.btnPreviewFont.Enabled = this.btnPreviewFontDefault.Enabled = this.chbPreviewInfo.Enabled = this.chbPreviewMode.Enabled = this.chbShowPreview.Checked;
            this.nudPreviewMaxWidth.Value = QTUtility.PreviewMaxWidth;
            this.nudPreviewMaxHeight.Value = QTUtility.PreviewMaxHeight;
            if(QTUtility.PreviewFontName != null) {
                try {
                    this.btnPreviewFont.Font = new Font(QTUtility.PreviewFontName, QTUtility.PreviewFontSize);
                }
                catch {
                }
            }
            this.chbSubDirTip.Checked = !QTUtility.CheckConfig(Settings.NoShowSubDirTips);
            this.chbSubDirTipMode.Checked = QTUtility.CheckConfig(Settings.SubDirTipsWithShift);
            this.chbSubDirTipModeHidden.Checked = QTUtility.CheckConfig(Settings.SubDirTipsHidden);
            this.chbSubDirTipModeFile.Checked = QTUtility.CheckConfig(Settings.SubDirTipsFiles);
            this.chbSubDirTipPreview.Checked = !QTUtility.CheckConfig(Settings.SubDirTipsPreview);
            this.chbSubDirTipMode.Enabled = this.chbSubDirTipModeHidden.Enabled = this.chbSubDirTipModeFile.Enabled = this.chbSubDirTipPreview.Enabled = this.chbSubDirTip.Checked;
            this.chbSubDirTipModeSystem.Checked = QTUtility.CheckConfig(Settings.SubDirTipsSystem);
            this.nudMaxUndo.Value = QTUtility.MaxCount_History;
            this.nudMaxRecentFile.Value = QTUtility.MaxCount_Executed;
            this.chbDD.Checked = QTUtility.CheckConfig(Settings.DragDropOntoTabs);
            this.chbNoTabFromOuteside.Checked = QTUtility.CheckConfig(Settings.NoTabsFromOutside);
            this.chbHolizontalScroll.Checked = !QTUtility.CheckConfig(Settings.HorizontalScroll);
            this.chbWhlChangeView.Checked = !QTUtility.CheckConfig(Settings.CtrlWheelChangeView);
            this.chbAutoSubText.Checked = !QTUtility.CheckConfig(Settings.NoRenameAmbTabs);
            this.chbSendToTray.Checked = QTUtility.CheckConfig(Settings.TrayOnClose);
            this.chbSendToTrayOnMinimize.Checked = QTUtility.CheckConfig(Settings.TrayOnMinimize);
            this.chbF2Selection.Checked = !QTUtility.CheckConfig(Settings.F2Selection);
            this.chbTabCloseBtnAlt.Enabled = this.chbTabCloseBtnHover.Enabled = this.chbTabCloseButton.Checked = QTUtility.CheckConfig(Settings.ShowTabCloseButtons);
            this.chbTabCloseBtnAlt.Checked = QTUtility.CheckConfig(Settings.TabCloseBtnsWithAlt);
            this.chbTabCloseBtnHover.Checked = QTUtility.CheckConfig(Settings.TabCloseBtnsOnHover);
            this.chbCursorLoop.Checked = QTUtility.CheckConfig(Settings.CursorLoop);
            this.chbDriveLetter.Enabled = this.chbSubDirTipOnTab.Enabled = this.chbFolderIcon.Checked;
            this.chbSubDirTipOnTab.Checked = QTUtility.CheckConfig(Settings.ShowSubDirTipOnTab);
            this.nudNetworkTimeOut.Value = IDLWrapper.iPingTimeOutMS / 0x3e8;
            this.textBoxLang.Text = QTUtility.Path_LanguageFile;
            if(QTUtility.CheckConfig(Settings.NonDefaultMenu)) {
                if(QTUtility.CheckConfig(Settings.XPStyleMenus)) {
                    this.cmbMenuRenderer.SelectedIndex = 2;
                }
                else {
                    this.cmbMenuRenderer.SelectedIndex = 1;
                }
            }
            else {
                this.cmbMenuRenderer.SelectedIndex = 0;
            }
            this.chbTabSwitcher.Checked = !QTUtility.CheckConfig(Settings.NoTabSwitcher);
            this.chbAutoUpdate.Checked = QTUtility.CheckConfig(Settings.AutoUpdate);
            this.chbRemoveOnSeparate.Checked = !QTUtility.CheckConfig(Settings.KeepOnSeparate);
            this.chbDriveLetter.Checked = QTUtility.CheckConfig(Settings.ShowDriveLetters);
            this.chbPlaySound.Checked = !QTUtility.CheckConfig(Settings.DisableSound);
            this.InitializePluginView();
            this.textBoxPluginLang.Text = QTUtility.Path_PluginLangFile;
            this.CreateShortcutItems();
            this.CreateNoCapturePaths();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e) {
            QTUtility.OptionsDialogTabIndex = this.tabControl1.SelectedIndex;
        }

        private void tbGroupKey_KeyPress(object sender, KeyPressEventArgs e) {
            e.Handled = true;
        }

        private void tbGroupKey_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) {
            TreeNode selectedNode = this.treeViewGroup.SelectedNode;
            if((((selectedNode != null) && (selectedNode.Tag != null)) && (selectedNode.Tag is MenuItemArguments)) && !IsInvalidShortcutKey(e.KeyCode, e.Modifiers)) {
                MenuItemArguments tag = (MenuItemArguments)selectedNode.Tag;
                int num = this.chbGroupKey.Checked ? 0x100000 : 0;
                if(e.KeyCode == Keys.Escape) {
                    this.tbGroupKey.Text = " - ";
                    tag.KeyShortcut = num;
                }
                else if(((e.Modifiers != Keys.None) && this.CheckExistance_Shortcuts(e.KeyData, null)) && (this.CheckExistance_UserAppKey(e.KeyData, this.tnRoot_UserApps, null) && this.CheckExistance_GroupKey(e.KeyData, selectedNode))) {
                    this.tbGroupKey.Text = QTUtility2.MakeKeyString(e.KeyData);
                    tag.KeyShortcut = num | (int)e.KeyData;
                }
            }
        }

        private void tbsUserApps_TextChanged(object sender, EventArgs e) {
            if(!this.fSuppressTextChangeEvent_UserApps) {
                TreeNode selectedNode = this.treeViewUserApps.SelectedNode;
                if(((selectedNode != null) && (selectedNode.Tag != null)) && (selectedNode.Tag is MenuItemArguments)) {
                    MenuItemArguments tag = (MenuItemArguments)selectedNode.Tag;
                    if(sender == this.tbPath) {
                        string str = QTUtility2.SanitizePathString(this.tbPath.Text);
                        tag.Path = str;
                        if(str.Length > 2) {
                            string name = str;
                            try {
                                name = Environment.ExpandEnvironmentVariables(name);
                            }
                            catch {
                            }
                            if((name.StartsWith(@"\\") || name.StartsWith("::")) || (File.Exists(name) || Directory.Exists(name))) {
                                selectedNode.ImageKey = selectedNode.SelectedImageKey = QTUtility.GetImageKey(name, Directory.Exists(name) ? null : Path.GetExtension(name));
                                return;
                            }
                        }
                        selectedNode.ImageKey = selectedNode.SelectedImageKey = "noimage";
                    }
                    else if(sender == this.tbArgs) {
                        tag.Argument = tag.OriginalArgument = this.tbArgs.Text;
                    }
                    else if(sender == this.tbWorking) {
                        tag.WorkingDirectory = tag.OriginalWorkingDirectory = QTUtility2.SanitizePathString(this.tbWorking.Text);
                    }
                }
            }
        }

        private void tbUserAppKey_KeyPress(object sender, KeyPressEventArgs e) {
            e.Handled = true;
        }

        private void tbUserAppKey_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) {
            TreeNode selectedNode = this.treeViewUserApps.SelectedNode;
            if((((selectedNode != null) && (selectedNode.Tag != null)) && (selectedNode.Tag is MenuItemArguments)) && !IsInvalidShortcutKey(e.KeyCode, e.Modifiers)) {
                MenuItemArguments tag = (MenuItemArguments)selectedNode.Tag;
                int num = this.chbUserAppKey.Checked ? 0x100000 : 0;
                if(e.KeyCode == Keys.Escape) {
                    this.tbUserAppKey.Text = " - ";
                    tag.KeyShortcut = num;
                }
                else if(((e.Modifiers != Keys.None) && this.CheckExistance_Shortcuts(e.KeyData, null)) && (this.CheckExistance_UserAppKey(e.KeyData, this.tnRoot_UserApps, selectedNode) && this.CheckExistance_GroupKey(e.KeyData, null))) {
                    this.tbUserAppKey.Text = QTUtility2.MakeKeyString(e.KeyData);
                    tag.KeyShortcut = num | (int)e.KeyData;
                }
            }
        }

        private void textBoxesPath_KeyPress(object sender, KeyPressEventArgs e) {
            if(!QTUtility2.IsValidPathChar(e.KeyChar)) {
                e.Handled = true;
                SystemSounds.Hand.Play();
            }
        }

        private void treeViewGroup_AfterLabelEdit(object sender, NodeLabelEditEventArgs e) {
            this.fNowTreeNodeEditing = false;
            if(e.Label != null) {
                if(((e.Label.Length == 0) || ((e.Node.Level == 1) && (e.Label.Length > 0x40))) || ((e.Node.Level == 2) && (e.Label.Length > 260))) {
                    SystemSounds.Beep.Play();
                    e.CancelEdit = true;
                    e.Node.BeginEdit();
                }
                else if(e.Node.Level == 1) {
                    TreeNode tnSelf = e.Node;
                    string b = CreateUniqueName(e.Label, tnSelf, tnSelf.Parent);
                    e.CancelEdit = !string.Equals(e.Label, b, StringComparison.OrdinalIgnoreCase);
                    tnSelf.Text = b;
                }
                else if((e.Node.Level == 2) && !string.Equals(e.Node.Text, e.Label)) {
                    string path = QTUtility2.SanitizePathString(e.Label).Replace(";", string.Empty);
                    if(path.Length == 0) {
                        e.CancelEdit = true;
                        e.Node.BeginEdit();
                    }
                    else {
                        if(path.Length > 3) {
                            path = path.Trim().TrimEnd(new char[] { '\\' });
                        }
                        if((path.Length == 2) && (path[1] == ':')) {
                            path = path + @"\";
                        }
                        string str3 = QTUtility2.MakePathDisplayText(path, true);
                        e.Node.EndEdit(true);
                        e.CancelEdit = true;
                        e.Node.Text = str3;
                        e.Node.Name = path;
                        e.Node.ToolTipText = str3;
                        e.Node.ImageKey = e.Node.SelectedImageKey = QTUtility.GetImageKey(path, null);
                        if(e.Node.Index == 0) {
                            e.Node.Parent.ImageKey = e.Node.Parent.SelectedImageKey = e.Node.ImageKey;
                        }
                    }
                }
            }
        }

        private void treeViewGroup_AfterSelect(object sender, TreeViewEventArgs e) {
            this.fSuppressTextChangeEvent_Group = true;
            int level = e.Node.Level;
            if(level != 1) {
                this.chbGroupKey.Checked = false;
                this.tbGroupKey.Clear();
                this.chbGroupKey.Enabled = this.tbGroupKey.Enabled = false;
            }
            if(level == 0) {
                this.btnMinus_Grp.Enabled = this.btnStartUpGrp.Enabled = this.btnUp_Grp.Enabled = this.btnDown_Grp.Enabled = false;
                this.cmbSpclFol_Grp.Enabled = this.btnAddSpcFol_Grp.Enabled = this.btnPlus_Grp.Enabled = true;
                this.fSuppressTextChangeEvent_Group = false;
            }
            else {
                MenuItemArguments tag = (MenuItemArguments)e.Node.Tag;
                if(tag == MIA_GROUPSEP) {
                    this.cmbSpclFol_Grp.Enabled = this.btnAddSpcFol_Grp.Enabled = this.btnStartUpGrp.Enabled = false;
                    this.tbGroupKey.Clear();
                    this.chbGroupKey.Checked = this.chbGroupKey.Enabled = this.tbGroupKey.Enabled = false;
                    this.btnPlus_Grp.Enabled = this.btnUp_Grp.Enabled = this.btnDown_Grp.Enabled = this.btnMinus_Grp.Enabled = true;
                }
                else {
                    if(level == 1) {
                        this.chbGroupKey.Enabled = true;
                        if(tag != null) {
                            this.chbGroupKey.Checked = (tag.KeyShortcut & 0x100000) != 0;
                            this.tbGroupKey.Text = QTUtility2.MakeKeyString(((Keys)tag.KeyShortcut) & ((Keys)(-1048577)));
                        }
                        else {
                            this.chbGroupKey.Checked = false;
                            this.tbGroupKey.Text = string.Empty;
                        }
                        this.tbGroupKey.Enabled = this.chbGroupKey.Checked;
                    }
                    this.cmbSpclFol_Grp.Enabled = this.btnAddSpcFol_Grp.Enabled = this.btnStartUpGrp.Enabled = this.btnPlus_Grp.Enabled = this.btnMinus_Grp.Enabled = true;
                }
                this.btnAddSep_Grp.Enabled = level != 2;
                int count = e.Node.Parent.Nodes.Count;
                if(count > 1) {
                    if(e.Node.Index == 0) {
                        this.btnUp_Grp.Enabled = false;
                        this.btnDown_Grp.Enabled = true;
                    }
                    else if(e.Node.Index == (count - 1)) {
                        this.btnUp_Grp.Enabled = true;
                        this.btnDown_Grp.Enabled = false;
                    }
                    else {
                        this.btnUp_Grp.Enabled = this.btnDown_Grp.Enabled = true;
                    }
                }
                else {
                    this.btnUp_Grp.Enabled = this.btnDown_Grp.Enabled = false;
                }
                this.fSuppressTextChangeEvent_Group = false;
            }
        }

        private void treeViewGroup_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e) {
            if((e.Node.Level == 0) || (e.Node.Tag == MIA_GROUPSEP)) {
                e.CancelEdit = true;
            }
            else {
                this.fNowTreeNodeEditing = true;
            }
        }

        private void treeViewGroup_KeyDown(object sender, KeyEventArgs e) {
            TreeNode selectedNode = this.treeViewGroup.SelectedNode;
            if((selectedNode != null) && (selectedNode.Level > 0)) {
                Keys keyCode = e.KeyCode;
                if(keyCode <= Keys.Down) {
                    switch(keyCode) {
                        case Keys.Up:
                            if(((e.Modifiers == Keys.Alt) || (e.Modifiers == Keys.Control)) && this.btnUp_Grp.Enabled) {
                                this.btnUp_Grp.PerformClick();
                                e.Handled = true;
                            }
                            return;

                        case Keys.Right:
                            return;

                        case Keys.Down:
                            if(((e.Modifiers == Keys.Alt) || (e.Modifiers == Keys.Control)) && this.btnDown_Grp.Enabled) {
                                this.btnDown_Grp.PerformClick();
                                e.Handled = true;
                            }
                            return;

                        case Keys.Return:
                            if(this.btnPlus_Grp.Enabled) {
                                this.btnPlus_Grp.PerformClick();
                                e.Handled = true;
                            }
                            return;
                    }
                }
                else {
                    if(keyCode != Keys.Delete) {
                        if(keyCode != Keys.F2) {
                            return;
                        }
                    }
                    else {
                        selectedNode.Remove();
                        return;
                    }
                    selectedNode.BeginEdit();
                }
            }
        }

        private void treeViewUserApps_AfterLabelEdit(object sender, NodeLabelEditEventArgs e) {
            this.fNowTreeNodeEditing = false;
            if(e.Label != null) {
                if((e.Label.Length == 0) || (e.Label.Length > 0x20)) {
                    SystemSounds.Beep.Play();
                    e.CancelEdit = true;
                    e.Node.BeginEdit();
                }
                else {
                    TreeNode tnSelf = e.Node;
                    string b = CreateUniqueName(e.Label, tnSelf, tnSelf.Parent);
                    e.CancelEdit = !string.Equals(e.Label, b, StringComparison.OrdinalIgnoreCase);
                    tnSelf.Text = b;
                }
            }
        }

        private void treeViewUserApps_AfterSelect(object sender, TreeViewEventArgs e) {
            this.fSuppressTextChangeEvent_UserApps = true;
            this.btnAddVFolder_app.Enabled = e.Node.Level < 10;
            if(e.Node.Level == 0) {
                this.btnMinus_app.Enabled = this.btnUp_app.Enabled = this.btnDown_app.Enabled = false;
            }
            else {
                if(e.Node.Level == 1) {
                    if(e.Node.Index == 0) {
                        this.btnUp_app.Enabled = false;
                        this.btnDown_app.Enabled = this.tnRoot_UserApps.Nodes.Count > 1;
                    }
                    else if(e.Node.Index == (this.tnRoot_UserApps.Nodes.Count - 1)) {
                        this.btnUp_app.Enabled = true;
                        this.btnDown_app.Enabled = false;
                    }
                    else {
                        this.btnUp_app.Enabled = this.btnDown_app.Enabled = true;
                    }
                }
                else {
                    this.btnUp_app.Enabled = this.btnDown_app.Enabled = true;
                }
                this.btnMinus_app.Enabled = true;
            }
            if((e.Node.Tag == null) || (e.Node.Text == "----------- Separator -----------")) {
                this.tbPath.Text = this.tbArgs.Text = this.tbWorking.Text = this.tbUserAppKey.Text = string.Empty;
                this.btnOFD_app.Enabled = this.btnBFD_app.Enabled = this.btnAddToken_Arg.Enabled = this.btnAddToken_Wrk.Enabled = this.tbPath.Enabled = this.tbArgs.Enabled = this.tbWorking.Enabled = this.chbUserAppKey.Enabled = this.chbUserAppKey.Checked = this.tbUserAppKey.Enabled = false;
            }
            else {
                MenuItemArguments tag = (MenuItemArguments)e.Node.Tag;
                this.tbPath.Text = tag.Path;
                this.tbArgs.Text = tag.Argument;
                this.tbWorking.Text = tag.WorkingDirectory;
                this.tbUserAppKey.Text = QTUtility2.MakeKeyString(((Keys)tag.KeyShortcut) & ((Keys)(-1048577)));
                this.chbUserAppKey.Checked = (tag.KeyShortcut & 0x100000) != 0;
                this.btnOFD_app.Enabled = this.btnBFD_app.Enabled = this.btnAddToken_Arg.Enabled = this.btnAddToken_Wrk.Enabled = this.tbPath.Enabled = this.tbArgs.Enabled = this.tbWorking.Enabled = this.chbUserAppKey.Enabled = true;
                this.tbUserAppKey.Enabled = this.chbUserAppKey.Checked;
            }
            this.fSuppressTextChangeEvent_UserApps = false;
        }

        private void treeViewUserApps_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e) {
            if((e.Node.Level == 0) || ((e.Node.Tag != null) && (e.Node.Text == "----------- Separator -----------"))) {
                e.CancelEdit = true;
            }
            else {
                this.fNowTreeNodeEditing = true;
            }
        }

        private void treeViewUserApps_KeyDown(object sender, KeyEventArgs e) {
            TreeNode selectedNode = this.treeViewUserApps.SelectedNode;
            if((selectedNode != null) && (selectedNode.Level > 0)) {
                switch(e.KeyCode) {
                    case Keys.Up:
                        if(((e.Modifiers == Keys.Alt) || (e.Modifiers == Keys.Control)) && this.btnUp_app.Enabled) {
                            this.btnUp_app.PerformClick();
                            e.Handled = true;
                            Application.DoEvents();
                        }
                        return;

                    case Keys.Right:
                        return;

                    case Keys.Down:
                        if(((e.Modifiers == Keys.Alt) || (e.Modifiers == Keys.Control)) && this.btnDown_app.Enabled) {
                            this.btnDown_app.PerformClick();
                            e.Handled = true;
                            Application.DoEvents();
                        }
                        return;

                    case Keys.Delete:
                        selectedNode.Remove();
                        return;

                    case Keys.F2:
                        selectedNode.BeginEdit();
                        return;
                }
            }
        }

        private void UpDownButtons_Click(object sender, EventArgs e) {
            bool flag = sender == this.btnUp_Grp;
            TreeNode selectedNode = this.treeViewGroup.SelectedNode;
            if(selectedNode != null) {
                int index = selectedNode.Index;
                int level = selectedNode.Level;
                if(level != 0) {
                    TreeNode parent = selectedNode.Parent;
                    this.treeViewGroup.BeginUpdate();
                    if(flag && (index != 0)) {
                        selectedNode.Remove();
                        parent.Nodes.Insert(index - 1, selectedNode);
                        this.treeViewGroup.SelectedNode = selectedNode;
                        if((level == 2) && ((index - 1) == 0)) {
                            parent.ImageKey = parent.SelectedImageKey = selectedNode.ImageKey;
                        }
                    }
                    else if(!flag && (index != (parent.Nodes.Count - 1))) {
                        selectedNode.Remove();
                        parent.Nodes.Insert(index + 1, selectedNode);
                        this.treeViewGroup.SelectedNode = selectedNode;
                        if((level == 2) && (index == 0)) {
                            parent.ImageKey = parent.SelectedImageKey = parent.Nodes[0].ImageKey;
                        }
                    }
                    this.treeViewGroup.EndUpdate();
                    selectedNode.EnsureVisible();
                }
            }
        }

        public static void WriteRegistry(Font tabFont, string[] strsTextExts, string[] strsImgExts) {
            InitializeStaticFields();
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar")) {
                if(key != null) {
                    key.SetValue("Config", QTUtility.ConfigValues);
                    key.SetValue("TabWidth", QTUtility.TabWidth);
                    key.SetValue("TabHeight", QTUtility.TabHeight);
                    key.SetValue("TabWidthMax", QTUtility.MaxTabWidth);
                    key.SetValue("TabWidthMin", QTUtility.MinTabWidth);
                    key.SetValue("TitleColorActive", QTUtility.TabTextColor_Active.ToArgb());
                    key.SetValue("TitleColorInactive", QTUtility.TabTextColor_Inactv.ToArgb());
                    key.SetValue("HighlightColorClassic", QTUtility.TabHiliteColor.ToArgb());
                    key.SetValue("TitleColorShadowActive", QTUtility.TabTextColor_ActivShdw.ToArgb());
                    key.SetValue("TitleColorShadowInActv", QTUtility.TabTextColor_InAtvShdw.ToArgb());
                    key.SetValue("ToolbarBGColor", QTUtility.RebarBGColor.ToArgb());
                    key.SetValue("AlternateColor_Bk", QTUtility.ShellViewRowCOLORREF_Background);
                    key.SetValue("AlternateColor_Text", QTUtility.ShellViewRowCOLORREF_Text);
                    key.SetValue("Max_Undo", QTUtility.MaxCount_History);
                    key.SetValue("Max_RecentFile", QTUtility.MaxCount_Executed);
                    key.SetValue("PreviewMaxWidth", QTUtility.PreviewMaxWidth);
                    key.SetValue("PreviewMaxHeight", QTUtility.PreviewMaxHeight);
                    key.SetValue("Action_BarDblClick", QTUtility.Action_BarDblClick);
                    if((tabFont != null) && !tabFont.Equals(Control.DefaultFont)) {
                        key.SetValue("TabFont", tabFont.Name);
                        key.SetValue("TabFontSize", tabFont.SizeInPoints.ToString());
                        QTUtility.TabFont = tabFont;
                    }
                    else {
                        key.DeleteValue("TabFont", false);
                        key.DeleteValue("TabFontSize", false);
                        QTUtility.TabFont = null;
                    }
                    key.SetValue("TabImage", QTUtility.Path_TabImage);
                    byte[] buffer = new byte[] { (byte)QTUtility.TabImageSizingMargin.Left, (byte)QTUtility.TabImageSizingMargin.Top, (byte)QTUtility.TabImageSizingMargin.Right, (byte)QTUtility.TabImageSizingMargin.Bottom };
                    key.SetValue("TabImageSizingMargin", buffer);
                    QTUtility.PreviewExtsList_Txt.Clear();
                    QTUtility.PreviewExtsList_Img.Clear();
                    string str = string.Empty;
                    string str2 = string.Empty;
                    if(strsTextExts != null) {
                        for(int i = 0; i < strsTextExts.Length; i++) {
                            QTUtility.PreviewExtsList_Txt.Add(strsTextExts[i]);
                            str = str + strsTextExts[i] + ";";
                        }
                        if(str.Length > 0) {
                            str = str.TrimEnd(QTUtility.SEPARATOR_CHAR);
                        }
                    }
                    if(strsImgExts != null) {
                        for(int j = 0; j < strsImgExts.Length; j++) {
                            QTUtility.PreviewExtsList_Img.Add(strsImgExts[j]);
                            str2 = str2 + strsImgExts[j] + ";";
                        }
                        if(str2.Length > 0) {
                            str2 = str2.TrimEnd(QTUtility.SEPARATOR_CHAR);
                        }
                    }
                    key.SetValue("TextExtensions", str);
                    key.SetValue("ImageExtensions", str2);
                    if(QTUtility.PreviewFontName != null) {
                        key.SetValue("PreviewFont", QTUtility.PreviewFontName);
                        key.SetValue("PreviewFontSize", QTUtility.PreviewFontSize.ToString());
                    }
                    else {
                        key.DeleteValue("PreviewFont", false);
                        key.DeleteValue("PreviewFontSize", false);
                    }
                    key.SetValue("NetworkTimeout", IDLWrapper.iPingTimeOutMS);
                    key.SetValue("LanguageFile", QTUtility.Path_LanguageFile);
                    using(RegistryKey key2 = key.CreateSubKey("Plugins")) {
                        if(key2 != null) {
                            key2.SetValue("LanguageFile", QTUtility.Path_PluginLangFile);
                        }
                    }
                    key.SetValue("ToolbarBGImage", QTUtility.Path_RebarImage);
                    QTUtility2.WriteRegBinary<int>(QTUtility.ShortcutKeys, "ShortcutKeys", key);
                }
            }
        }

        private sealed class ColorDialogEx : ColorDialog {
            protected override int Options {
                get {
                    return (base.Options | 2);
                }
            }
        }

        private sealed class ListViewEx : System.Windows.Forms.ListView {
            private StringFormat sfCenter = new StringFormat(StringFormatFlags.NoWrap);
            private StringFormat sfLeft = new StringFormat(StringFormatFlags.NoWrap);
            private VisualStyleRenderer vsrChecked;
            private VisualStyleRenderer vsrUnChecked;

            public ListViewEx() {
                base.OwnerDraw = true;
                this.DoubleBuffered = true;
                this.sfLeft.LineAlignment = StringAlignment.Center;
                this.sfLeft.Trimming = StringTrimming.EllipsisCharacter;
                this.sfCenter.Alignment = StringAlignment.Center;
                this.sfCenter.LineAlignment = StringAlignment.Center;
                this.sfCenter.Trimming = StringTrimming.EllipsisCharacter;
            }

            protected override void Dispose(bool disposing) {
                this.sfCenter.Dispose();
                this.sfLeft.Dispose();
                base.Dispose(disposing);
            }

            protected override void OnDrawColumnHeader(DrawListViewColumnHeaderEventArgs e) {
                e.DrawDefault = true;
            }

            protected override void OnDrawSubItem(DrawListViewSubItemEventArgs e) {
                Brush brush = e.Item.Selected ? SystemBrushes.HighlightText : (e.Item.Checked ? SystemBrushes.WindowText : SystemBrushes.GrayText);
                e.Graphics.FillRectangle(e.Item.Selected ? SystemBrushes.Highlight : SystemBrushes.Window, e.Bounds);
                if(e.ColumnIndex == 0) {
                    int y = ((e.Bounds.Height - 0x10) / 2) + e.Bounds.Y;
                    Rectangle bounds = new Rectangle(e.Bounds.X + 2, y, 0x10, 0x10);
                    RectangleF layoutRectangle = new RectangleF((float)(e.Bounds.X + 20), (float)e.Bounds.Y, (float)(e.Bounds.Width - 20), (float)e.Bounds.Height);
                    if(VisualStyleRenderer.IsSupported) {
                        if(this.vsrChecked == null) {
                            this.vsrChecked = new VisualStyleRenderer(VisualStyleElement.Button.CheckBox.CheckedNormal);
                            this.vsrUnChecked = new VisualStyleRenderer(VisualStyleElement.Button.CheckBox.UncheckedNormal);
                        }
                        if(e.Item.Checked) {
                            this.vsrChecked.DrawBackground(e.Graphics, bounds);
                        }
                        else {
                            this.vsrUnChecked.DrawBackground(e.Graphics, bounds);
                        }
                    }
                    else {
                        ControlPaint.DrawCheckBox(e.Graphics, bounds, e.Item.Checked ? ButtonState.Checked : ButtonState.Normal);
                    }
                    e.Graphics.DrawString(e.Item.Text, this.Font, brush, layoutRectangle, this.sfLeft);
                }
                else if(e.ColumnIndex == 1) {
                    e.Graphics.DrawString(e.SubItem.Text, this.Font, brush, e.Bounds, this.sfCenter);
                }
            }

            protected override bool CanEnableIme {
                get {
                    return false;
                }
            }
        }

        private sealed class TabImageSetting {
            private Padding padding;

            [DisplayName("Sizing margins")]
            public Padding SizingMargin {
                get {
                    return this.padding;
                }
                set {
                    this.padding = value;
                    if((this.padding.Left > 50) || (this.padding.Left < 0)) {
                        this.padding.Left = 0;
                    }
                    if((this.padding.Top > 50) || (this.padding.Top < 0)) {
                        this.padding.Top = 0;
                    }
                    if((this.padding.Right > 50) || (this.padding.Right < 0)) {
                        this.padding.Right = 0;
                    }
                    if((this.padding.Bottom > 50) || (this.padding.Bottom < 0)) {
                        this.padding.Bottom = 0;
                    }
                }
            }
        }
    }
}
