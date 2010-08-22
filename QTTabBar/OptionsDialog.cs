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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Microsoft.Win32;
using QTPlugin;
using QTTabBarLib.Interop;
using ContentAlignment = System.Drawing.ContentAlignment;

namespace QTTabBarLib {
    internal sealed class OptionsDialog : Form {
        private static int[] arrSpecialFolderCSIDLs;
        private static string[] arrSpecialFolderDipNms;
        private Button btnActTxtClr;
        private Button btnAdd_NoCapture;
        private Button btnAddImgExt;
        private Button btnAddSep_app;
        private Button btnAddSep_Grp;
        private Button btnAddSpcFol_Grp;
        private Button btnAddSpcFol_NoCapture;
        private Button btnAddTextExt;
        private Button btnAddToken_Arg;
        private Button btnAddToken_Wrk;
        private Button btnAddVFolder_app;
        private Button btnAlternate_Default;
        private Button btnAlternateColor;
        private Button btnAlternateColor_Text;
        private Button btnApply;
        private Button btnBFD_app;
        private Button btnBrowseAction_BarDblClck;
        private Button btnBrowsePlugin;
        private Button btnBrowsePluginLang;
        private Button btnCancel;
        private Button btnCheckUpdates;
        private Button btnClearRecentFile;
        private Button btnCopyKeys;
        private Button btnDefaultImgExt;
        private Button btnDefaultTextExt;
        private Button btnDefTxtClr;
        private Button btnDelImgExt;
        private Button btnDelTextExt;
        private Button btnDown_app;
        private Button btnDown_Grp;
        private Button btnExportSettings;
        private Button btnHiliteClsc;
        private Button btnHistoryClear;
        private Button btnInactTxtClr;
        private Button btnLangBrowse;
        private Button btnMinus_app;
        private Button btnMinus_Grp;
        private Button btnOFD_app;
        private Button btnOFD_NoCapture;
        private Button btnOK;
        private Button btnPayPal;
        private Button btnPlus_app;
        private Button btnPlus_Grp;
        private Button btnPreviewFont;
        private Button btnPreviewFontDefault;
        private Button btnRebarImage;
        private Button btnRemove_NoCapture;
        private Button btnShadowAct;
        private Button btnShadowIna;
        private Button btnStartUpGrp;
        private Button btnTabFont;
        private Button btnTabImage;
        private Button btnToolBarBGClr;
        private Button btnUp_app;
        private Button btnUp_Grp;
        private FormMethodInvoker callBack;
        private CheckBox chbActivateNew;
        private CheckBox chbAlternateColor;
        private CheckBox chbAlwaysShowHeader;
        private CheckBox chbAutoSubText;
        private CheckBox chbAutoUpdate;
        private CheckBox chbBlockProcess;
        private CheckBox chbBoldActv;
        private CheckBox chbBSUpOneLvl;
        private CheckBox chbCloseWhenGroup;
        private CheckBox chbCursorLoop;
        private CheckBox chbDD;
        private CheckBox chbDontOpenSame;
        private CheckBox chbDriveLetter;
        private CheckBox chbF2Selection;
        private CheckBox chbFolderIcon;
        private CheckBox chbFoldrTree;
        private CheckBox chbForceSysListView;
        private CheckBox chbGridLine;
        private CheckBox chbGroupKey;
        private CheckBox chbHideMenu;
        private CheckBox chbHolizontalScroll;
        private CheckBox chbNavBtn;
        private CheckBox chbNCADblClck;
        private CheckBox chbNeverCloseWindow;
        private CheckBox chbNeverCloseWndLocked;
        private CheckBox chbNoFulRowSelect;
        private CheckBox chbNoHistory;
        private CheckBox chbNoTabFromOuteside;
        private CheckBox chbPlaySound;
        private CheckBox chbPreviewInfo;
        private CheckBox chbPreviewMode;
        private CheckBox chbRebarBGImage;
        private CheckBox chbRemoveOnSeparate;
        private CheckBox chbRestoreClosed;
        private CheckBox chbRestoreLocked;
        private CheckBox chbSaveExecuted;
        private CheckBox chbSelectWithoutExt;
        private CheckBox chbSendToTray;
        private CheckBox chbSendToTrayOnMinimize;
        private CheckBox chbShowPreview;
        private CheckBox chbShowTooltip;
        private CheckBox chbSubDirTip;
        private CheckBox chbSubDirTipMode;
        private CheckBox chbSubDirTipModeFile;
        private CheckBox chbSubDirTipModeHidden;
        private CheckBox chbSubDirTipModeSystem;
        private CheckBox chbSubDirTipOnTab;
        private CheckBox chbSubDirTipPreview;
        private CheckBox chbTabCloseBtnAlt;
        private CheckBox chbTabCloseBtnHover;
        private CheckBox chbTabCloseButton;
        private CheckBox chbTabSwitcher;
        private CheckBox chbTabTitleShadow;
        private CheckBox chbToolbarBGClr;
        private CheckBox chbTreeShftWhlTab;
        private CheckBox chbUserAppKey;
        private CheckBox chbUseTabSkin;
        private CheckBox chbWhlChangeView;
        private CheckBox chbWhlClick;
        private CheckBox chbWndRestrAlpha;
        private CheckBox chbWndUnresizable;
        private CheckBox chbX1X2;
        private ColumnHeader clmKeys_Action;
        private ColumnHeader clmKeys_Key;
        private ColumnHeader clmnHeader_NoCapture;
        private ComboBox cmbActvClose;
        private ComboBox cmbBGDblClick;
        private ComboBox cmbImgExts;
        private ComboBox cmbMenuRenderer;
        private ComboBox cmbMultiRow;
        private ComboBox cmbNavBtn;
        private ComboBox cmbNewTabLoc;
        private ComboBox cmbRebarBGImageMode;
        private ComboBox cmbSpclFol_Grp;
        private ComboBox cmbSpclFol_NoCapture;
        private ComboBox cmbTabDblClck;
        private ComboBox cmbTabSizeMode;
        private ComboBox cmbTabTextAlignment;
        private ComboBox cmbTabWhlClck;
        private ComboBox cmbTextExts;
        private ComboBox cmbWhlClick;
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
        private ListView listView_NoCapture;
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
        private TextBox tbArgs;
        private TextBox tbGroupKey;
        private TextBox tbPath;
        private TextBox tbRebarImagePath;
        private TextBox tbTabImagePath;
        private TextBox tbUserAppKey;
        private TextBox tbWorking;
        private const string TEXT_EXT_DEFVAL_IMG = "(Image & movie file)";
        private const string TEXT_EXT_DEFVAL_TXT = "(Text file)";
        private const string TEXT_ONW_DEFVAL = "Enter path";
        private const string TEXT_SEPDISPLAY = "----------- Separator -----------";
        private TextBox textBoxAction_BarDblClck;
        private TextBox textBoxLang;
        private TextBox textBoxPluginLang;
        private TreeNode tnGroupsRoot;
        private TreeNode tnRoot_UserApps;
        private TreeView treeViewGroup;
        private TreeView treeViewUserApps;

        public OptionsDialog(PluginManager pluginManager, FormMethodInvoker callBack) {
            InitializeStaticFields();
            this.pluginManager = pluginManager;
            this.callBack = callBack;
            InitializeComponent();
            tabImageSetting = new TabImageSetting();
            propertyGrid1.SelectedObject = tabImageSetting;
            propertyGrid1.ExpandAllGridItems();
            fntStartUpGroup = new Font(treeViewGroup.Font, FontStyle.Underline);
            sfPlugins = new StringFormat(StringFormatFlags.NoWrap);
            sfPlugins.Alignment = StringAlignment.Near;
            sfPlugins.LineAlignment = StringAlignment.Center;
            SuspendLayout();
            lblVer.Text = "QTTabBar ver " + QTUtility2.MakeVersionString();
            tabPage1_Gnrl.Text = ResOpt_Genre[0];
            tabPage2_Tabs.Text = ResOpt_Genre[1];
            tabPage3_Wndw.Text = ResOpt_Genre[2];
            tabPage4_View.Text = ResOpt_Genre[3];
            tabPage5_Grps.Text = ResOpt_Genre[4];
            tabPage6_Apps.Text = ResOpt_Genre[5];
            tabPage7_Plug.Text = ResOpt_Genre[6];
            tabPage8_Keys.Text = ResOpt_Genre[7];
            tabPage9_Misc.Text = ResOpt_Genre[8];
            tabPageA_Path.Text = ResOpt_Genre[9];
            string[] strArray = QTUtility.TextResourcesDic["DialogButtons"];
            btnOK.Text = strArray[0];
            btnCancel.Text = strArray[1];
            btnApply.Text = strArray[2];
            chbActivateNew.Text = ResOpt[0];
            chbDontOpenSame.Text = ResOpt[1];
            chbCloseWhenGroup.Text = ResOpt[2];
            chbShowTooltip.Text = ResOpt[3];
            chbX1X2.Text = ResOpt[4];
            chbNavBtn.Text = ResOpt[5];
            chbNoHistory.Text = ResOpt[6];
            chbSaveExecuted.Text = ResOpt[7];
            chbDD.Text = ResOpt[8];
            lblLang.Text = ResOpt[9];
            lblNewTabLoc.Text = ResOpt[10];
            lblActvClose.Text = ResOpt[11];
            lblTabDblClk.Text = ResOpt[12];
            lblBGDblClik.Text = ResOpt[13];
            lblAction_BarDblClick.Text = ResOpt[14];
            lblMultiRows.Text = ResOpt[15];
            chbAutoSubText.Text = ResOpt[0x10];
            chbWhlClick.Text = ResOpt[0x11];
            chbNCADblClck.Text = ResOpt[0x12];
            chbWndUnresizable.Text = ResOpt[0x13];
            chbWndRestrAlpha.Text = ResOpt[20];
            chbBlockProcess.Text = ResOpt[0x15];
            chbFoldrTree.Text = ResOpt[0x16];
            chbNoTabFromOuteside.Text = ResOpt[0x17];
            chbHolizontalScroll.Text = ResOpt[0x18];
            chbWhlChangeView.Text = ResOpt[0x19];
            chbNeverCloseWindow.Text = ResOpt[0x1a];
            chbNeverCloseWndLocked.Text = ResOpt[0x1b];
            chbRestoreClosed.Text = ResOpt[0x1c];
            chbRestoreLocked.Text = ResOpt[0x1d];
            chbUseTabSkin.Text = ResOpt[30];
            btnHiliteClsc.Text = ResOpt[0x1f];
            lblTabSizeTitle.Text = ResOpt[0x20];
            lblTabWidth.Text = ResOpt[0x21];
            lblTabHeight.Text = ResOpt[0x22];
            lblTabWFix.Text = ResOpt_DropDown[0x18];
            lblTabWMax.Text = ResOpt[0x23];
            lblTabWMin.Text = ResOpt[0x24];
            lblTabFont.Text = ResOpt[0x25];
            btnTabFont.Text = ResOpt[0x25];
            chbBoldActv.Text = ResOpt[0x26];
            lblTabTxtClr.Text = ResOpt[0x27];
            btnActTxtClr.Text = ResOpt[40];
            btnShadowAct.Text = ResOpt[40];
            btnInactTxtClr.Text = ResOpt[0x29];
            btnShadowIna.Text = ResOpt[0x29];
            btnDefTxtClr.Text = ResOpt[0x2a];
            chbToolbarBGClr.Text = btnToolBarBGClr.Text = ResOpt[0x2b];
            chbFolderIcon.Text = ResOpt[0x2c];
            lblUserApps_Path.Text = ResOpt[0x2d] + ":";
            lblUserApps_Args.Text = ResOpt[0x2e] + ":";
            lblUserApps_Working.Text = ResOpt[0x2f] + ":";
            lblUserApps_Key.Text = ResOpt_Genre[7] + ":";
            lblGroupKey.Text = ResOpt_Genre[7] + ":";
            lblPluginLang.Text = ResOpt[0x30];
            chbHideMenu.Text = ResOpt[0x31];
            chbBSUpOneLvl.Text = ResOpt[50];
            chbNoFulRowSelect.Text = ResOpt[0x33];
            chbGridLine.Text = ResOpt[0x34];
            chbAlternateColor.Text = ResOpt[0x35];
            chbShowPreview.Text = ResOpt[0x36];
            chbPreviewMode.Text = ResOpt[0x37];
            chbSubDirTip.Text = ResOpt[0x38];
            chbSubDirTipMode.Text = ResOpt[0x37];
            chbSubDirTipModeHidden.Text = ResOpt[0x39];
            chbSubDirTipPreview.Text = ResOpt[0x36];
            chbSubDirTipModeFile.Text = ResOpt[0x3a];
            chbSelectWithoutExt.Text = ResOpt[0x3b];
            chbSubDirTipModeSystem.Text = ResOpt[60];
            chbSendToTray.Text = ResOpt[0x3d];
            lblPreviewWidth.Text = ResOpt[0x3e];
            lblPreviewHeight.Text = ResOpt[0x3f];
            chbRebarBGImage.Text = ResOpt[0x40];
            chbF2Selection.Text = ResOpt[0x41];
            chbTabCloseButton.Text = ResOpt[0x42];
            lblTabWhlClk.Text = ResOpt[0x43];
            chbSubDirTipOnTab.Text = ResOpt[0x44];
            clmnHeader_NoCapture.Text = ResOpt[0x45];
            chbTabCloseBtnAlt.Text = ResOpt[70];
            chbTabCloseBtnHover.Text = ResOpt[0x47];
            btnExportSettings.Text = ResOpt[0x48];
            chbCursorLoop.Text = ResOpt[0x49];
            lblNetworkTimeOut.Text = ResOpt[0x4a];
            chbSendToTrayOnMinimize.Text = ResOpt[0x4b];
            btnPreviewFont.Text = ResOpt[0x25];
            lblTabTextAlignment.Text = ResOpt[0x4c];
            lblMenuRenderer.Text = ResOpt[0x4d];
            string[] strArray2 = QTUtility.TextResourcesDic["TabBar_Option2"];
            chbTreeShftWhlTab.Text = strArray2[0];
            chbTabSwitcher.Text = strArray2[1];
            chbTabTitleShadow.Text = strArray2[2];
            chbAutoUpdate.Text = strArray2[3];
            chbRemoveOnSeparate.Text = strArray2[4];
            chbDriveLetter.Text = strArray2[5];
            chbPlaySound.Text = strArray2[6];
            btnBrowsePlugin.Text = strArray2[7];
            PluginView.BTN_OPTION = strArray2[8];
            PluginView.BTN_DISABLE = strArray2[9];
            PluginView.BTN_ENABLE = strArray2[10];
            PluginView.BTN_REMOVE = strArray2[11];
            PluginView.MNU_PLUGINABOUT = strArray2[12];
            RES_REMOVEPLGIN = strArray2[13];
            chbPreviewInfo.Text = strArray2[14];
            chbForceSysListView.Text = strArray2[15];
            chbAlwaysShowHeader.Text = strArray2[16];
            string[] strArray3 = QTUtility.TextResourcesDic["TabBar_Option_Buttons"];
            btnHistoryClear.Text = btnClearRecentFile.Text = strArray3[0];
            btnUp_Grp.Text = strArray3[1];
            btnUp_app.Text = strArray3[1];
            btnDown_Grp.Text = strArray3[2];
            btnDown_app.Text = strArray3[2];
            btnAddSep_Grp.Text = strArray3[3];
            btnAddSep_app.Text = strArray3[3];
            btnStartUpGrp.Text = strArray3[4];
            btnAlternateColor.Text = strArray3[6];
            btnAlternateColor_Text.Text = strArray3[7];
            btnAlternate_Default.Text = strArray3[8];
            btnDefaultTextExt.Text = strArray3[8];
            btnDefaultImgExt.Text = strArray3[8];
            btnPreviewFontDefault.Text = strArray3[8];
            btnAddTextExt.Text = strArray3[9];
            btnAddImgExt.Text = strArray3[9];
            btnDelTextExt.Text = strArray3[10];
            btnDelImgExt.Text = strArray3[10];
            btnCheckUpdates.Text = strArray3[11];
            btnCopyKeys.Text = strArray3[12];
            cmbNavBtn.Items.AddRange(new string[] { ResOpt_DropDown[0], ResOpt_DropDown[1] });
            cmbNewTabLoc.Items.AddRange(new string[] { ResOpt_DropDown[2], ResOpt_DropDown[3], ResOpt_DropDown[1], ResOpt_DropDown[0] });
            cmbActvClose.Items.AddRange(new string[] { ResOpt_DropDown[1], ResOpt_DropDown[0], ResOpt_DropDown[2], ResOpt_DropDown[3], ResOpt_DropDown[4] });
            cmbTabDblClck.Items.AddRange(new string[] { ResOpt_DropDown[5], ResOpt_DropDown[6], ResOpt_DropDown[7], ResOpt_DropDown[8], ResOpt_DropDown[9], ResOpt_DropDown[10], ResOpt_DropDown[11], ResOpt_DropDown[12], ResOpt_DropDown[13] });
            cmbBGDblClick.Items.AddRange(new string[] { ResOpt_DropDown[14], ResOpt_DropDown[5], ResOpt_DropDown[15], ResOpt_DropDown[0x10], ResOpt_DropDown[8], ResOpt_DropDown[0x11], ResOpt_DropDown[0x12], ResOpt_DropDown[0x13], ResOpt_DropDown[20], ResOpt_DropDown[0x16], ResOpt_DropDown[9], ResOpt_DropDown[4], ResOpt_DropDown[13] });
            cmbTabWhlClck.Items.AddRange(new string[] { ResOpt_DropDown[6], ResOpt_DropDown[5], ResOpt_DropDown[7], ResOpt_DropDown[8], ResOpt_DropDown[9], ResOpt_DropDown[10], ResOpt_DropDown[11], ResOpt_DropDown[12], ResOpt_DropDown[13] });
            cmbMultiRow.Items.AddRange(new string[] { ResOpt_DropDown[13], ResOpt_DropDown[0x15] + "1", ResOpt_DropDown[0x15] + "2" });
            cmbWhlClick.Items.AddRange(new string[] { ResOpt_DropDown[0x16], ResOpt_DropDown[9] });
            cmbTabSizeMode.Items.AddRange(new string[] { ResOpt_DropDown[0x17], ResOpt_DropDown[0x18], ResOpt_DropDown[0x19] });
            cmbTabTextAlignment.Items.AddRange(new string[] { ResOpt_DropDown[0x1d], ResOpt_DropDown[30] });
            cmbTextExts.Items.Add("(Text file)");
            cmbTextExts.Items.AddRange(QTUtility.PreviewExtsList_Txt.ToArray());
            cmbTextExts.SelectedIndex = 0;
            cmbImgExts.Items.Add("(Image & movie file)");
            cmbImgExts.Items.AddRange(QTUtility.PreviewExtsList_Img.ToArray());
            cmbImgExts.SelectedIndex = 0;
            cmbRebarBGImageMode.Items.AddRange(new string[] { ResOpt_DropDown[0x1a], ResOpt_DropDown[0x1b], ResOpt_DropDown[0x1c], ResOpt_DropDown[0x21] });
            cmbMenuRenderer.Items.AddRange(new string[] { ResOpt[0x2a], ResOpt_DropDown[0x1f], ResOpt_DropDown[0x20] });
            if(arrSpecialFolderCSIDLs == null) {
                if(!QTUtility.IsXP) {
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
            cmbSpclFol_NoCapture.Items.AddRange(arrSpecialFolderDipNms);
            cmbSpclFol_NoCapture.SelectedIndex = 0;
            cmbSpclFol_Grp.Items.AddRange(arrSpecialFolderDipNms);
            cmbSpclFol_Grp.SelectedIndex = 0;
            using(RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Quizo\QTTabBar")) {
                if(key != null) {
                    int[] numArray = QTUtility2.ReadRegBinary<int>("OptionWindowBounds", key);
                    if((numArray != null) && (numArray.Length == 4)) {
                        StartPosition = FormStartPosition.Manual;
                        SetDesktopBounds(numArray[0], numArray[1], numArray[2], numArray[3]);
                    }
                }
            }
            SetValues();
            tabControl1.SelectedIndex = QTUtility.OptionsDialogTabIndex;
            ResumeLayout();
        }

        private void btnAdd_NoCapture_Click(object sender, EventArgs e) {
            listView_NoCapture.BeginUpdate();
            listView_NoCapture.SelectedItems.Clear();
            foreach(ListViewItem item in listView_NoCapture.Items) {
                if(item.Text.Length == 0 || item.Text == "Enter path") {
                    item.Selected = true;
                    item.BeginEdit();
                    listView_NoCapture.EndUpdate();
                    return;
                }
            }
            ListViewItem item2 = new ListViewItem("Enter path");
            listView_NoCapture.Items.Add(item2);
            listView_NoCapture.EndUpdate();
            item2.BeginEdit();
        }

        private void btnAddPreviewExt_Click(object sender, EventArgs e) {
            EnterExtension(sender == btnAddTextExt);
        }

        private void btnAddSep_app_Click(object sender, EventArgs e) {
            TreeNode selectedNode = treeViewUserApps.SelectedNode;
            TreeNode parent = tnRoot_UserApps;
            int count = tnRoot_UserApps.Nodes.Count;
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
            treeViewUserApps.SelectedNode = node;
        }

        private void btnAddSep_Click(object sender, EventArgs e) {
            TreeNode selectedNode = treeViewGroup.SelectedNode;
            if((selectedNode != null) && (selectedNode.Level != 2)) {
                TreeNode node = new TreeNode("----------- Separator -----------");
                node.Tag = MIA_GROUPSEP;
                node.ForeColor = SystemColors.GrayText;
                node.ImageKey = node.SelectedImageKey = "noimage";
                treeViewGroup.BeginUpdate();
                if(selectedNode.Level == 1) {
                    tnGroupsRoot.Nodes.Insert(selectedNode.Index + 1, node);
                }
                else if(selectedNode.Level == 0) {
                    tnGroupsRoot.Nodes.Add(node);
                }
                treeViewGroup.EndUpdate();
                node.EnsureVisible();
            }
        }

        private void btnAddSpcFol_Grp_Click(object sender, EventArgs e) {
            TreeNode selectedNode = treeViewGroup.SelectedNode;
            if(selectedNode != null) {
                string selectedItem = (string)cmbSpclFol_Grp.SelectedItem;
                int selectedIndex = cmbSpclFol_Grp.SelectedIndex;
                string specialFolderCLSID = ShellMethods.GetSpecialFolderCLSID(arrSpecialFolderCSIDLs[selectedIndex], false);
                if((selectedIndex == 3) && !QTUtility.IsXP) {
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
                    tnGroupsRoot.Nodes.Add(node3);
                    treeViewGroup.SelectedNode = node3;
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
            listView_NoCapture.BeginUpdate();
            listView_NoCapture.SelectedItems.Clear();
            string selectedItem = (string)cmbSpclFol_NoCapture.SelectedItem;
            int selectedIndex = cmbSpclFol_NoCapture.SelectedIndex;
            string specialFolderCLSID = ShellMethods.GetSpecialFolderCLSID(arrSpecialFolderCSIDLs[selectedIndex], false);
            if((selectedIndex == 3) && !QTUtility.IsXP) {
                specialFolderCLSID = "::{26EE0668-A00A-44D7-9371-BEB064C98683}";
            }
            foreach(ListViewItem item in listView_NoCapture.Items) {
                if(((item.Text.Length == 0) || (item.Text == "Enter path")) || (item.Text == selectedItem)) {
                    if(item.Text != selectedItem) {
                        item.Text = selectedItem;
                        item.Name = specialFolderCLSID;
                    }
                    item.Selected = true;
                    listView_NoCapture.EndUpdate();
                    return;
                }
            }
            ListViewItem item2 = new ListViewItem(selectedItem);
            item2.Name = specialFolderCLSID;
            listView_NoCapture.Items.Add(item2);
            item2.Selected = true;
            listView_NoCapture.EndUpdate();
        }

        private void btnAddToken_Arg_Click(object sender, EventArgs e) {
            CreateTokenMenu();
            cmsAddToken.Items[0].Enabled = true;
            Rectangle rectangle = tabPage6_Apps.RectangleToScreen(btnAddToken_Arg.Bounds);
            cmsAddToken.Show(rectangle.Right, rectangle.Top);
        }

        private void btnAddToken_Wrk_Click(object sender, EventArgs e) {
            CreateTokenMenu();
            cmsAddToken.Items[0].Enabled = false;
            Rectangle rectangle = tabPage6_Apps.RectangleToScreen(btnAddToken_Wrk.Bounds);
            cmsAddToken.Show(rectangle.Right, rectangle.Top);
        }

        private void btnAddVirtualFolder_app_Click(object sender, EventArgs e) {
            TreeNode selectedNode = treeViewUserApps.SelectedNode;
            TreeNode tnParent = tnRoot_UserApps;
            int count = tnRoot_UserApps.Nodes.Count;
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
            treeViewUserApps.SelectedNode = node;
            node.BeginEdit();
        }

        private void btnAlternateColor_Click(object sender, EventArgs e) {
            if(sender == btnAlternate_Default) {
                QTUtility.ShellViewRowCOLORREF_Background = 0xfaf5f1;
                Color windowText = SystemColors.WindowText;
                QTUtility.ShellViewRowCOLORREF_Text = QTUtility2.MakeCOLORREF(windowText);
                btnAlternateColor.BackColor = QTUtility2.MakeColor(QTUtility.ShellViewRowCOLORREF_Background);
                btnAlternateColor_Text.ForeColor = windowText;
            }
            else {
                bool flag = sender == btnAlternateColor;
                using(ColorDialogEx ex = new ColorDialogEx()) {
                    ex.Color = flag ? btnAlternateColor.BackColor : btnAlternateColor_Text.ForeColor;
                    if(DialogResult.OK == ex.ShowDialog()) {
                        if(flag) {
                            btnAlternateColor.BackColor = ex.Color;
                        }
                        else {
                            btnAlternateColor_Text.ForeColor = ex.Color;
                        }
                    }
                }
            }
        }

        private void btnBFD_app_Click(object sender, EventArgs e) {
            TreeNode selectedNode = treeViewUserApps.SelectedNode;
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
                        tbPath.Text = dialog.SelectedPath;
                        tbArgs.Text = tbWorking.Text = string.Empty;
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
                dialog.SelectedPath = textBoxAction_BarDblClck.Text;
                if(DialogResult.OK == dialog.ShowDialog()) {
                    textBoxAction_BarDblClck.Text = dialog.SelectedPath;
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
                    foreach(PluginAssembly assembly in dialog.FileNames
                            .Select(str => new PluginAssembly(str))
                            .Where(assembly => assembly.PluginInfosExist)) {
                        CreatePluginViewItem(new PluginAssembly[] { assembly }, true);
                        if(flag) {
                            flag = false;
                            SelectPluginBottom();
                        }
                    }
                }
            }
        }

        private void btnBrowsePluginLang_Click(object sender, EventArgs e) {
            using(OpenFileDialog dialog = new OpenFileDialog()) {
                dialog.Filter = "Language XML files (*.xml)|*.xml";
                dialog.RestoreDirectory = true;
                if(textBoxPluginLang.Text.Length > 0) {
                    dialog.FileName = textBoxPluginLang.Text;
                }
                if(dialog.ShowDialog() == DialogResult.OK) {
                    textBoxPluginLang.Text = dialog.FileName;
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            if(callBack != null) {
                callBack.BeginInvoke(DialogResult.Cancel, null, null);
                callBack = null;
            }
            Close();
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
            for(int i = 0; i < listViewKeyboard.Items.Count; i++) {
                ListViewItem item = listViewKeyboard.Items[i];
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
            cmbImgExts.Items.Clear();
            cmbImgExts.Items.Add("(Image & movie file)");
            cmbImgExts.Items.AddRange(ThumbnailTooltipForm.MakeDefaultImgExts().ToArray());
            cmbImgExts.SelectedIndex = 0;
        }

        private void btnDefaultTextExt_Click(object sender, EventArgs e) {
            cmbTextExts.Items.Clear();
            cmbTextExts.Items.AddRange(new string[] { "(Text file)", ".txt", ".ini", ".inf", ".cs", ".log", ".js", ".vbs" });
            cmbTextExts.SelectedIndex = 0;
        }

        private void btnDelPreiviewExt_Click(object sender, EventArgs e) {
            ComboBox box = (sender == btnDelTextExt) ? cmbTextExts : cmbImgExts;
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
                        startInfo.ErrorDialogParentHandle = Handle;
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
                    dialog.Font = btnTabFont.Font;
                    dialog.ShowEffects = false;
                    dialog.AllowVerticalFonts = false;
                    if(DialogResult.OK == dialog.ShowDialog()) {
                        btnTabFont.Font = dialog.Font;
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
                if(textBoxLang.Text.Length > 0) {
                    dialog.FileName = textBoxLang.Text;
                }
                if(dialog.ShowDialog() == DialogResult.OK) {
                    textBoxLang.Text = dialog.FileName;
                }
            }
        }

        private void btnMinus_app_Click(object sender, EventArgs e) {
            TreeNode selectedNode = treeViewUserApps.SelectedNode;
            if((selectedNode != null) && (selectedNode.Level > 0)) {
                selectedNode.Remove();
            }
        }

        private void btnMinus_Click(object sender, EventArgs e) {
            TreeNode selectedNode = treeViewGroup.SelectedNode;
            if((selectedNode != null) && (selectedNode.Level != 0)) {
                selectedNode.Remove();
            }
        }

        private void btnOFD_app_Click(object sender, EventArgs e) {
            TreeNode selectedNode = treeViewUserApps.SelectedNode;
            if((selectedNode != null) && (selectedNode.Tag != null)) {
                using(OpenFileDialog dialog = new OpenFileDialog()) {
                    dialog.RestoreDirectory = true;
                    dialog.DereferenceLinks = false;
                    string path = ((MenuItemArguments)selectedNode.Tag).Path;
                    if(((path.Length > 3) && !path.StartsWith(@"\\")) && (File.Exists(path) || Directory.Exists(path))) {
                        dialog.FileName = path;
                    }
                    if(dialog.ShowDialog() == DialogResult.OK) {
                        tbPath.Text = dialog.FileName;
                        if(tbWorking.Text.Length == 0) {
                            tbWorking.Text = Path.GetDirectoryName(dialog.FileName);
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
                    listView_NoCapture.BeginUpdate();
                    listView_NoCapture.SelectedItems.Clear();
                    bool flag = false;
                    foreach(ListViewItem item in listView_NoCapture.Items) {
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
                        listView_NoCapture.Items.Add(item2);
                    }
                    listView_NoCapture.EndUpdate();
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
            TreeNode selectedNode = treeViewUserApps.SelectedNode;
            TreeNode tnParent = tnRoot_UserApps;
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
                selectedNode = tnRoot_UserApps;
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
            treeViewUserApps.SelectedNode = node;
            node.BeginEdit();
        }

        private void btnPlus_Click(object sender, EventArgs e) {
            TreeNode selectedNode = treeViewGroup.SelectedNode;
            if(selectedNode != null) {
                if((selectedNode.Level == 0) || (selectedNode.Tag == MIA_GROUPSEP)) {
                    int index = (selectedNode.Level == 0) ? tnGroupsRoot.Nodes.Count : (selectedNode.Index + 1);
                    string text = CreateUniqueName("NewGroup", null, tnGroupsRoot);
                    TreeNode node = new TreeNode(text);
                    node.Tag = new MenuItemArguments(text, null, null, 0, MenuGenre.Group);
                    tnGroupsRoot.Nodes.Insert(index, node);
                    treeViewGroup.SelectedNode = node;
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
            if(sender == btnPreviewFont) {
                try {
                    using(FontDialog dialog = new FontDialog()) {
                        dialog.Font = btnPreviewFont.Font;
                        dialog.ShowEffects = false;
                        dialog.AllowVerticalFonts = false;
                        if(DialogResult.OK == dialog.ShowDialog()) {
                            btnPreviewFont.Font = dialog.Font;
                        }
                    }
                }
                catch {
                    SystemSounds.Hand.Play();
                }
            }
            else {
                btnPreviewFont.Font = null;
            }
        }

        private void btnRebarImage_Click(object sender, EventArgs e) {
            using(OpenFileDialog dialog = new OpenFileDialog()) {
                dialog.Filter = "Image Files(*.PNG;*.BMP;*.JPG;*.GIF)|*.PNG;*.BMP;*.JPG;*.GIF";
                dialog.FileName = tbRebarImagePath.Text;
                dialog.RestoreDirectory = true;
                dialog.Title = "Toolbar background image";
                if(DialogResult.OK == dialog.ShowDialog()) {
                    tbRebarImagePath.Text = dialog.FileName;
                }
            }
        }

        private void btnRemove_NoCapture_Click(object sender, EventArgs e) {
            int count = listView_NoCapture.SelectedItems.Count;
            if(count > 0) {
                listView_NoCapture.BeginUpdate();
                int index = listView_NoCapture.Items.IndexOf(listView_NoCapture.SelectedItems[0]);
                foreach(ListViewItem item in listView_NoCapture.SelectedItems) {
                    listView_NoCapture.Items.Remove(item);
                }
                if(count == 1) {
                    count = listView_NoCapture.Items.Count;
                    if(count > index) {
                        listView_NoCapture.Items[index].Selected = true;
                    }
                    else if(count > 0) {
                        listView_NoCapture.Items[count - 1].Selected = true;
                    }
                }
                listView_NoCapture.EndUpdate();
            }
        }

        private void btnShadowClrs_Click(object sender, EventArgs e) {
            using(ColorDialogEx ex = new ColorDialogEx()) {
                ex.Color = ((Button)sender).ForeColor;
                if(DialogResult.OK == ex.ShowDialog()) {
                    ((Button)sender).ForeColor = ex.Color;
                }
            }
        }

        private void btnStartUpGrp_Click(object sender, EventArgs e) {
            TreeNode selectedNode = treeViewGroup.SelectedNode;
            if((selectedNode != null) && (selectedNode.Level > 0)) {
                TreeNode parent;
                if(selectedNode.Level == 1) {
                    parent = selectedNode;
                }
                else {
                    parent = selectedNode.Parent;
                }
                if(parent.NodeFont == fntStartUpGroup) {
                    parent.NodeFont = treeViewGroup.Font;
                }
                else {
                    parent.NodeFont = fntStartUpGroup;
                }
            }
        }

        private void btnTabImage_Click(object sender, EventArgs e) {
            using(OpenFileDialog dialog = new OpenFileDialog()) {
                dialog.Filter = "Image Files(*.PNG;*.BMP;*.JPG;*.GIF)|*.PNG;*.BMP;*.JPG;*.GIF";
                dialog.FileName = tbTabImagePath.Text;
                dialog.RestoreDirectory = true;
                dialog.Title = "Tab image";
                if(DialogResult.OK == dialog.ShowDialog()) {
                    tbTabImagePath.Text = dialog.FileName;
                }
            }
        }

        private void btnUpDown_app_Click(object sender, EventArgs e) {
            bool flag = sender == btnUp_app;
            TreeNode selectedNode = treeViewUserApps.SelectedNode;
            if((selectedNode != null) && (selectedNode != tnRoot_UserApps)) {
                TreeNode parent = selectedNode.Parent;
                TreeNode node3 = flag ? selectedNode.PrevNode : selectedNode.NextNode;
                treeViewUserApps.BeginUpdate();
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
                treeViewUserApps.SelectedNode = selectedNode;
                treeViewUserApps.EndUpdate();
                selectedNode.EnsureVisible();
            }
        }

        private void buttonActClr_Click(object sender, EventArgs e) {
            using(ColorDialogEx ex = new ColorDialogEx()) {
                ex.Color = btnActTxtClr.ForeColor;
                if(DialogResult.OK == ex.ShowDialog()) {
                    btnActTxtClr.ForeColor = ex.Color;
                }
            }
        }

        private void buttonApply_Click(object sender, EventArgs e) {
            Save(true);
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
                ex.Color = btnHiliteClsc.ForeColor;
                if(DialogResult.OK == ex.ShowDialog()) {
                    btnHiliteClsc.ForeColor = ex.Color;
                }
            }
        }

        private void buttonInactClr_Click(object sender, EventArgs e) {
            using(ColorDialogEx ex = new ColorDialogEx()) {
                ex.Color = btnInactTxtClr.ForeColor;
                if(DialogResult.OK == ex.ShowDialog()) {
                    btnInactTxtClr.ForeColor = ex.Color;
                }
            }
        }

        private void buttonOK_Click(object sender, EventArgs e) {
            Save(false);
        }

        private void buttonRstClr_Click(object sender, EventArgs e) {
            btnActTxtClr.ForeColor = btnInactTxtClr.ForeColor = SystemColors.ControlText;
            btnHiliteClsc.ForeColor = SystemColors.Highlight;
            btnTabFont.Font = null;
        }

        private void buttonToolBarBGClr_Click(object sender, EventArgs e) {
            using(ColorDialogEx ex = new ColorDialogEx()) {
                ex.Color = btnToolBarBGClr.BackColor;
                if(DialogResult.OK == ex.ShowDialog()) {
                    btnToolBarBGClr.BackColor = ex.Color;
                }
            }
        }

        private void chbAlternateColor_CheckedChanged(object sender, EventArgs e) {
            btnAlternate_Default.Enabled = btnAlternateColor.Enabled = btnAlternateColor_Text.Enabled = chbAlternateColor.Checked;
        }

        private void chbDrawMode_CheckedChanged(object sender, EventArgs e) {
            propertyGrid1.Enabled = btnHiliteClsc.Enabled = tbTabImagePath.Enabled = btnTabImage.Enabled = chbUseTabSkin.Checked;
        }

        private void chbFolderIcon_CheckedChanged(object sender, EventArgs e) {
            chbDriveLetter.Enabled = chbSubDirTipOnTab.Enabled = chbFolderIcon.Checked;
        }

        private void chbGroupKey_CheckedChanged(object sender, EventArgs e) {
            if(!fSuppressTextChangeEvent_Group) {
                TreeNode selectedNode = treeViewGroup.SelectedNode;
                if(((selectedNode != null) && (selectedNode.Tag != null)) && (selectedNode.Tag is MenuItemArguments)) {
                    MenuItemArguments tag = (MenuItemArguments)selectedNode.Tag;
                    if(chbGroupKey.Checked) {
                        tag.KeyShortcut |= 0x100000;
                    }
                    else {
                        tag.KeyShortcut &= -1048577;
                    }
                    tbGroupKey.Enabled = chbGroupKey.Checked;
                }
            }
        }

        private void chbMMButton_CheckedChanged(object sender, EventArgs e) {
            cmbWhlClick.Enabled = chbWhlClick.Checked;
        }

        private void chbNavBtn_CheckedChanged(object sender, EventArgs e) {
            cmbNavBtn.Enabled = chbNavBtn.Checked;
        }

        private void chbRebarBGImage_CheckedChanged(object sender, EventArgs e) {
            cmbRebarBGImageMode.Enabled = btnRebarImage.Enabled = tbRebarImagePath.Enabled = chbRebarBGImage.Checked;
        }

        private void chbsCloseWindow_CheckedChanged(object sender, EventArgs e) {
            if(sender == chbNeverCloseWndLocked) {
                if(chbNeverCloseWndLocked.Checked) {
                    chbRestoreLocked.Enabled = chbRestoreLocked.Checked = false;
                }
                else {
                    chbRestoreLocked.Enabled = true;
                }
            }
            else if(sender == chbRestoreLocked) {
                if(chbRestoreLocked.Checked) {
                    chbRestoreClosed.Checked = true;
                }
            }
            else if((sender == chbRestoreClosed) && !chbRestoreClosed.Checked) {
                chbRestoreLocked.Checked = false;
            }
        }

        private void chbShowPreviewTooltip_CheckedChanged(object sender, EventArgs e) {
            tabPage9_Misc.SuspendLayout();
            nudPreviewMaxWidth.Enabled = nudPreviewMaxHeight.Enabled = cmbTextExts.Enabled = btnAddTextExt.Enabled = btnDelTextExt.Enabled = btnDefaultTextExt.Enabled = cmbImgExts.Enabled = btnAddImgExt.Enabled = btnDelImgExt.Enabled = btnDefaultImgExt.Enabled = btnPreviewFont.Enabled = btnPreviewFontDefault.Enabled = chbPreviewInfo.Enabled = chbPreviewMode.Enabled = chbShowPreview.Checked;
            tabPage9_Misc.ResumeLayout();
        }

        private void chbSubDirTip_CheckedChanged(object sender, EventArgs e) {
            chbSubDirTipMode.Enabled = chbSubDirTipModeFile.Enabled = chbSubDirTipModeHidden.Enabled = chbSubDirTipPreview.Enabled = chbSubDirTipModeSystem.Enabled = chbSubDirTip.Checked;
        }

        private void chbTabCloseBtns_CheckedChanged(object sender, EventArgs e) {
            if((sender == chbTabCloseBtnHover) && chbTabCloseBtnHover.Checked) {
                chbTabCloseBtnAlt.Checked = false;
            }
            else if((sender == chbTabCloseBtnAlt) && chbTabCloseBtnAlt.Checked) {
                chbTabCloseBtnHover.Checked = false;
            }
        }

        private void chbTabCloseButton_CheckedChanged(object sender, EventArgs e) {
            chbTabCloseBtnAlt.Enabled = chbTabCloseBtnHover.Enabled = chbTabCloseButton.Checked;
        }

        private void chbTabTitleShadow_CheckedChanged(object sender, EventArgs e) {
            btnShadowAct.Enabled = btnShadowIna.Enabled = chbTabTitleShadow.Checked;
        }

        private void chbToolbarBGClr_CheckedChanged(object sender, EventArgs e) {
            btnToolBarBGClr.Enabled = chbToolbarBGClr.Checked;
        }

        private void chbUserAppKey_CheckedChanged(object sender, EventArgs e) {
            if(!fSuppressTextChangeEvent_UserApps) {
                TreeNode selectedNode = treeViewUserApps.SelectedNode;
                if(((selectedNode != null) && (selectedNode.Tag != null)) && (selectedNode.Tag is MenuItemArguments)) {
                    MenuItemArguments tag = (MenuItemArguments)selectedNode.Tag;
                    if(chbUserAppKey.Checked) {
                        tag.KeyShortcut |= 0x100000;
                    }
                    else {
                        tag.KeyShortcut &= -1048577;
                    }
                    tbUserAppKey.Enabled = chbUserAppKey.Checked;
                }
            }
        }

        private bool CheckExistance_GroupKey(Keys keys, TreeNode tnCurrent) {
            foreach(TreeNode node in tnGroupsRoot.Nodes) {
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
                        if(node == treeViewGroup.SelectedNode) {
                            chbGroupKey.Checked = false;
                            tbGroupKey.Text = " - ";
                        }
                        return true;
                    }
                    return false;
                }
            }
            return true;
        }

        private bool CheckExistance_Shortcuts(Keys keys, ListViewItem lviCurrent) {
            foreach(ListViewItem item in listViewKeyboard.Items) {
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
                            if(node == treeViewUserApps.SelectedNode) {
                                chbUserAppKey.Checked = false;
                                tbUserAppKey.Text = " - ";
                            }
                            return true;
                        }
                        return false;
                    }
                }
                if(!CheckExistance_UserAppKey(keys, node, tnCurrent)) {
                    return false;
                }
            }
            return true;
        }

        private void cmsAddToken_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            if(cmsAddToken.Items[0].Enabled) {
                tbArgs.Paste(e.ClickedItem.Name);
            }
            else {
                tbWorking.Paste(e.ClickedItem.Name);
            }
        }

        private void comboBoxes_SelectedIndexChanged(object sender, EventArgs e) {
            if(sender == cmbBGDblClick) {
                lblAction_BarDblClick.Enabled = textBoxAction_BarDblClck.Enabled = btnBrowseAction_BarDblClck.Enabled = (cmbBGDblClick.SelectedIndex == 9) || (cmbBGDblClick.SelectedIndex == 10);
            }
            else if(sender == cmbTabSizeMode) {
                nudTabWidth.Enabled = cmbTabSizeMode.SelectedIndex == 1;
                nudTabWidthMax.Enabled = nudTabWidthMin.Enabled = cmbTabSizeMode.SelectedIndex == 2;
            }
            else if(sender == cmbTextExts) {
                iComboBoxTextPreview = cmbTextExts.SelectedIndex;
            }
            else if(sender == cmbImgExts) {
                iConboBoxImagPreview = cmbImgExts.SelectedIndex;
            }
        }

        private void comboBoxPreviewExts_KeyPress(object sender, KeyPressEventArgs e) {
            if(e.KeyChar == '\r') {
                e.Handled = true;
                EnterExtension(sender == cmbTextExts);
            }
            else {
                ComboBox box = (ComboBox)sender;
                if((box.SelectedIndex == 0) && (box.Text == ((sender == cmbTextExts) ? "(Text file)" : "(Image & movie file)"))) {
                    box.Text = string.Empty;
                }
            }
        }

        private void CreateNoCapturePaths() {
            listView_NoCapture.BeginUpdate();
            foreach(string str in QTUtility.NoCapturePathsList) {
                ListViewItem item = new ListViewItem(str);
                if(str.StartsWith("::")) {
                    string displayName = ShellMethods.GetDisplayName(str);
                    if(!string.IsNullOrEmpty(displayName)) {
                        item.Text = displayName;
                    }
                }
                item.Name = str;
                listView_NoCapture.Items.Add(item);
            }
            listView_NoCapture.EndUpdate();
        }

        private void CreatePluginViewItem(PluginAssembly[] pluginAssemblies, bool fAddedByUser) {
            foreach(PluginAssembly assembly in pluginAssemblies.Where(assembly => assembly.PluginInfosExist
                    && pluginView.PluginViewItems.All(item => item.PluginInfo.Path != assembly.Path))) {
                int num = 0;
                foreach(PluginInformation information in assembly.PluginInformations) {
                    if(fAddedByUser || information.Enabled) {
                        information.Enabled = assembly.Enabled = true;
                    }
                    pluginView.AddPluginViewItem(information, assembly);
                    num++;
                }
                if((num > 0) && fAddedByUser) {
                    lstPluginAssembliesUserAdded.Add(assembly);
                }
            }
        }

        private void CreateShortcutItems() {
            string[] strArray = QTUtility.TextResourcesDic["ShortcutKeys_ActionNames"];
            string[] strArray2 = QTUtility.TextResourcesDic["ShortcutKeys_Groups"];
            ListViewGroup group = listViewKeyboard.Groups.Add("general", strArray2[0]);
            listViewKeyboard.BeginUpdate();
            for(int i = 0; i < QTUtility.ShortcutKeys.Length; i++) {
                bool flag = (QTUtility.ShortcutKeys[i] & 0x100000) == 0x100000;
                Keys key = ((Keys)QTUtility.ShortcutKeys[i]) & ((Keys)(-1048577));
                ListViewItem item = new ListViewItem(new string[] { strArray[i], QTUtility2.MakeKeyString(key) });
                item.Checked = flag;
                item.Group = group;
                item.Tag = key;
                listViewKeyboard.Items.Add(item);
            }
            foreach(string str in QTUtility.dicPluginShortcutKeys.Keys) {
                Plugin plugin;
                int[] numArray = QTUtility.dicPluginShortcutKeys[str];
                if(pluginManager.TryGetPlugin(str, out plugin) && (plugin.PluginInformation.ShortcutKeyActions != null)) {
                    ListViewGroup group2 = listViewKeyboard.Groups.Add(plugin.PluginInformation.PluginID, plugin.PluginInformation.Name + " (" + strArray2[1] + ")");
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
                            listViewKeyboard.Items.Add(item2);
                        }
                        continue;
                    }
                    foreach(string action in plugin.PluginInformation.ShortcutKeyActions) {
                        listViewKeyboard.Items.Add(new ListViewItem(new string[] {action, " - "}) {
                                Checked = false,
                                Group = group2,
                                Tag = 0,
                                Name = str
                        });
                    }
                }
            }
            listViewKeyboard.EndUpdate();
        }

        private void CreateTokenMenu() {
            if(cmsAddToken.Items.Count == 0) {
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
                cmsAddToken.Items.AddRange(new ToolStripItem[] { item, item2, item3, item4, item5 });
            }
        }

        private static string CreateUniqueName(string strInitialName, TreeNode tnSelf, TreeNode tnParent) {
            int num = 1;
            string b = strInitialName;
            while(tnParent.Nodes.Cast<TreeNode>().Any(node =>
                    tnSelf != node && string.Equals(node.Text, b, StringComparison.OrdinalIgnoreCase))) {
                b = strInitialName + "_" + ++num;
            }
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
                RemovePluginShortcutKeys(information.PluginID);
            }
            if(lstPluginAssembliesUserAdded.Contains(pa)) {
                pa.Dispose();
                lstPluginAssembliesUserAdded.Remove(pa);
            }
        }

        protected override void Dispose(bool disposing) {
            fntStartUpGroup.Dispose();
            sfPlugins.Dispose();
            cmsAddToken.Dispose();
            pluginManager = null;
            foreach(PluginAssembly assembly in lstPluginAssembliesUserAdded) {
                assembly.Dispose();
            }
            lstPluginAssembliesUserAdded.Clear();
            base.Dispose(disposing);
        }

        private void EnterExtension(bool fText) {
            ComboBox box = fText ? cmbTextExts : cmbImgExts;
            string str = fText ? "(Text file)" : "(Image & movie file)";
            int index = fText ? iComboBoxTextPreview : iConboBoxImagPreview;
            if(box.Text.Length > 0) {
                if((box.SelectedIndex != 0) || (box.Text != str)) {
                    string b = box.Text.ToLower();
                    if(!b.StartsWith(".")) {
                        b = "." + b;
                    }
                    if(box.Items.Cast<string>().Any(str3 => string.Equals(str3, b, StringComparison.OrdinalIgnoreCase))) {
                        return;
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
            btnOK = new Button();
            btnApply = new Button();
            btnCancel = new Button();
            lblVer = new LinkLabel();
            tabControl1 = new TabControl();
            tabPage1_Gnrl = new TabPage();
            tabPage2_Tabs = new TabPage();
            tabPage3_Wndw = new TabPage();
            tabPage4_View = new TabPage();
            tabPage5_Grps = new TabPage();
            tabPage6_Apps = new TabPage();
            tabPage7_Plug = new TabPage();
            tabPage8_Keys = new TabPage();
            tabPage9_Misc = new TabPage();
            tabPageA_Path = new TabPage();
            chbActivateNew = new CheckBox();
            chbDontOpenSame = new CheckBox();
            chbCloseWhenGroup = new CheckBox();
            chbShowTooltip = new CheckBox();
            chbX1X2 = new CheckBox();
            chbNavBtn = new CheckBox();
            chbNoHistory = new CheckBox();
            chbSaveExecuted = new CheckBox();
            chbDD = new CheckBox();
            chbAutoUpdate = new CheckBox();
            chbPlaySound = new CheckBox();
            cmbNavBtn = new ComboBox();
            btnHistoryClear = new Button();
            btnClearRecentFile = new Button();
            nudMaxUndo = new NumericUpDown();
            nudMaxRecentFile = new NumericUpDown();
            lblLang = new Label();
            lblNetworkTimeOut = new Label();
            textBoxLang = new TextBox();
            btnLangBrowse = new Button();
            btnCheckUpdates = new Button();
            btnExportSettings = new Button();
            nudNetworkTimeOut = new NumericUpDown();
            lblNewTabLoc = new Label();
            lblActvClose = new Label();
            lblTabDblClk = new Label();
            lblBGDblClik = new Label();
            lblTabWhlClk = new Label();
            lblAction_BarDblClick = new Label();
            lblMultiRows = new Label();
            cmbNewTabLoc = new ComboBox();
            cmbActvClose = new ComboBox();
            cmbTabDblClck = new ComboBox();
            cmbBGDblClick = new ComboBox();
            cmbTabWhlClck = new ComboBox();
            cmbMultiRow = new ComboBox();
            textBoxAction_BarDblClck = new TextBox();
            btnBrowseAction_BarDblClck = new Button();
            chbAutoSubText = new CheckBox();
            chbTabCloseButton = new CheckBox();
            chbTabCloseBtnAlt = new CheckBox();
            chbTabCloseBtnHover = new CheckBox();
            chbSubDirTipOnTab = new CheckBox();
            chbTreeShftWhlTab = new CheckBox();
            chbTabSwitcher = new CheckBox();
            chbRemoveOnSeparate = new CheckBox();
            chbDriveLetter = new CheckBox();
            chbWhlClick = new CheckBox();
            chbNCADblClck = new CheckBox();
            chbBlockProcess = new CheckBox();
            chbFoldrTree = new CheckBox();
            chbWndUnresizable = new CheckBox();
            chbWndRestrAlpha = new CheckBox();
            chbNoTabFromOuteside = new CheckBox();
            chbHolizontalScroll = new CheckBox();
            chbWhlChangeView = new CheckBox();
            chbNeverCloseWindow = new CheckBox();
            chbNeverCloseWndLocked = new CheckBox();
            chbRestoreClosed = new CheckBox();
            chbRestoreLocked = new CheckBox();
            chbSendToTray = new CheckBox();
            chbSendToTrayOnMinimize = new CheckBox();
            cmbWhlClick = new ComboBox();
            lblSep = new Label();
            chbUseTabSkin = new CheckBox();
            chbToolbarBGClr = new CheckBox();
            chbFolderIcon = new CheckBox();
            chbBoldActv = new CheckBox();
            chbRebarBGImage = new CheckBox();
            chbTabTitleShadow = new CheckBox();
            propertyGrid1 = new PropertyGrid();
            nudTabWidth = new NumericUpDown();
            nudTabHeight = new NumericUpDown();
            nudTabWidthMax = new NumericUpDown();
            nudTabWidthMin = new NumericUpDown();
            lblTabSizeTitle = new Label();
            lblTabWidth = new Label();
            lblTabHeight = new Label();
            lblTabWMin = new Label();
            lblTabWMax = new Label();
            lblTabWFix = new Label();
            lblTabFont = new Label();
            lblMenuRenderer = new Label();
            lblTabTextAlignment = new Label();
            lblTabTxtClr = new Label();
            cmbTabSizeMode = new ComboBox();
            cmbTabTextAlignment = new ComboBox();
            cmbRebarBGImageMode = new ComboBox();
            cmbMenuRenderer = new ComboBox();
            btnHiliteClsc = new Button();
            btnTabFont = new Button();
            btnActTxtClr = new Button();
            btnInactTxtClr = new Button();
            btnDefTxtClr = new Button();
            btnToolBarBGClr = new Button();
            btnRebarImage = new Button();
            btnShadowAct = new Button();
            btnShadowIna = new Button();
            btnTabImage = new Button();
            tbRebarImagePath = new TextBox();
            tbTabImagePath = new TextBox();
            treeViewGroup = new TreeView();
            btnUp_Grp = new Button();
            btnDown_Grp = new Button();
            btnMinus_Grp = new Button();
            btnPlus_Grp = new Button();
            btnStartUpGrp = new Button();
            btnAddSep_Grp = new Button();
            cmbSpclFol_Grp = new ComboBox();
            btnAddSpcFol_Grp = new Button();
            lblGroupKey = new Label();
            tbGroupKey = new TextBox();
            chbGroupKey = new CheckBox();
            treeViewUserApps = new TreeView();
            btnUp_app = new Button();
            btnDown_app = new Button();
            btnAddSep_app = new Button();
            btnAddVFolder_app = new Button();
            btnPlus_app = new Button();
            btnMinus_app = new Button();
            lblUserApps_Path = new Label();
            lblUserApps_Args = new Label();
            lblUserApps_Working = new Label();
            tbPath = new TextBox();
            tbArgs = new TextBox();
            tbWorking = new TextBox();
            tbUserAppKey = new TextBox();
            chbUserAppKey = new CheckBox();
            lblUserApps_Key = new Label();
            btnOFD_app = new Button();
            btnBFD_app = new Button();
            btnAddToken_Arg = new Button();
            btnAddToken_Wrk = new Button();
            cmsAddToken = new ContextMenuStrip();
            chbHideMenu = new CheckBox();
            chbAlwaysShowHeader = new CheckBox();
            chbForceSysListView = new CheckBox();
            chbBSUpOneLvl = new CheckBox();
            chbNoFulRowSelect = new CheckBox();
            chbGridLine = new CheckBox();
            chbAlternateColor = new CheckBox();
            chbShowPreview = new CheckBox();
            chbPreviewMode = new CheckBox();
            chbPreviewInfo = new CheckBox();
            chbSubDirTip = new CheckBox();
            chbSubDirTipMode = new CheckBox();
            chbSubDirTipModeHidden = new CheckBox();
            chbSubDirTipModeSystem = new CheckBox();
            chbSubDirTipModeFile = new CheckBox();
            chbSubDirTipPreview = new CheckBox();
            chbSelectWithoutExt = new CheckBox();
            chbF2Selection = new CheckBox();
            chbCursorLoop = new CheckBox();
            btnAlternateColor = new Button();
            btnAlternateColor_Text = new Button();
            btnAlternate_Default = new Button();
            btnAddTextExt = new Button();
            btnDelTextExt = new Button();
            btnDefaultTextExt = new Button();
            btnAddImgExt = new Button();
            btnDelImgExt = new Button();
            btnDefaultImgExt = new Button();
            btnPreviewFont = new Button();
            btnPreviewFontDefault = new Button();
            btnPayPal = new Button();
            nudPreviewMaxHeight = new NumericUpDown();
            nudPreviewMaxWidth = new NumericUpDown();
            lblPreviewHeight = new Label();
            lblPreviewWidth = new Label();
            cmbTextExts = new ComboBox();
            cmbImgExts = new ComboBox();
            pluginView = new PluginView();
            btnBrowsePlugin = new Button();
            lblPluginLang = new Label();
            textBoxPluginLang = new TextBox();
            btnBrowsePluginLang = new Button();
            listViewKeyboard = new ListViewEx();
            clmKeys_Action = new ColumnHeader();
            clmKeys_Key = new ColumnHeader();
            btnCopyKeys = new Button();
            listView_NoCapture = new ListView();
            btnOFD_NoCapture = new Button();
            btnAdd_NoCapture = new Button();
            btnRemove_NoCapture = new Button();
            clmnHeader_NoCapture = new ColumnHeader();
            cmbSpclFol_NoCapture = new ComboBox();
            btnAddSpcFol_NoCapture = new Button();
            tabControl1.SuspendLayout();
            tabPage1_Gnrl.SuspendLayout();
            tabPage2_Tabs.SuspendLayout();
            tabPage3_Wndw.SuspendLayout();
            tabPage4_View.SuspendLayout();
            tabPage5_Grps.SuspendLayout();
            tabPage6_Apps.SuspendLayout();
            tabPage7_Plug.SuspendLayout();
            tabPage8_Keys.SuspendLayout();
            tabPage9_Misc.SuspendLayout();
            tabPageA_Path.SuspendLayout();
            nudMaxUndo.BeginInit();
            nudMaxRecentFile.BeginInit();
            nudNetworkTimeOut.BeginInit();
            nudTabWidthMin.BeginInit();
            nudTabWidthMax.BeginInit();
            nudTabHeight.BeginInit();
            nudTabWidth.BeginInit();
            nudPreviewMaxWidth.BeginInit();
            nudPreviewMaxHeight.BeginInit();
            SuspendLayout();
            btnOK.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            btnOK.Location = new Point(0xf1, 0x246);
            btnOK.Size = new Size(0x58, 0x17);
            btnOK.TabIndex = 4;
            btnOK.Click += buttonOK_Click;
            btnApply.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            btnApply.Location = new Point(0x1ad, 0x246);
            btnApply.Size = new Size(0x58, 0x17);
            btnApply.TabIndex = 6;
            btnApply.Click += buttonApply_Click;
            btnCancel.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            btnCancel.Location = new Point(0x14f, 0x246);
            btnCancel.Size = new Size(0x58, 0x17);
            btnCancel.TabIndex = 5;
            btnCancel.Click += btnCancel_Click;
            lblVer.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            lblVer.AutoSize = true;
            lblVer.LinkColor = Color.Blue;
            lblVer.ActiveLinkColor = Color.Red;
            lblVer.VisitedLinkColor = Color.Purple;
            lblVer.Location = new Point(12, 0x24b);
            lblVer.Click += lblVer_Click;
            tabControl1.Controls.Add(tabPage1_Gnrl);
            tabControl1.Controls.Add(tabPage2_Tabs);
            tabControl1.Controls.Add(tabPage3_Wndw);
            tabControl1.Controls.Add(tabPage4_View);
            tabControl1.Controls.Add(tabPage5_Grps);
            tabControl1.Controls.Add(tabPage6_Apps);
            tabControl1.Controls.Add(tabPage7_Plug);
            tabControl1.Controls.Add(tabPage8_Keys);
            tabControl1.Controls.Add(tabPage9_Misc);
            tabControl1.Controls.Add(tabPageA_Path);
            tabControl1.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            tabControl1.Location = new Point(9, 9);
            tabControl1.Margin = new Padding(0);
            tabControl1.Multiline = true;
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(0x207, 0x238);
            tabControl1.TabIndex = 0;
            tabControl1.SelectedIndexChanged += tabControl1_SelectedIndexChanged;
            tabPage1_Gnrl.Controls.Add(chbActivateNew);
            tabPage1_Gnrl.Controls.Add(chbDontOpenSame);
            tabPage1_Gnrl.Controls.Add(chbCloseWhenGroup);
            tabPage1_Gnrl.Controls.Add(chbShowTooltip);
            tabPage1_Gnrl.Controls.Add(chbX1X2);
            tabPage1_Gnrl.Controls.Add(chbNavBtn);
            tabPage1_Gnrl.Controls.Add(chbNoHistory);
            tabPage1_Gnrl.Controls.Add(chbSaveExecuted);
            tabPage1_Gnrl.Controls.Add(chbDD);
            tabPage1_Gnrl.Controls.Add(chbPlaySound);
            tabPage1_Gnrl.Controls.Add(cmbNavBtn);
            tabPage1_Gnrl.Controls.Add(btnHistoryClear);
            tabPage1_Gnrl.Controls.Add(nudMaxUndo);
            tabPage1_Gnrl.Controls.Add(nudMaxRecentFile);
            tabPage1_Gnrl.Controls.Add(btnClearRecentFile);
            tabPage1_Gnrl.Controls.Add(lblLang);
            tabPage1_Gnrl.Controls.Add(textBoxLang);
            tabPage1_Gnrl.Controls.Add(btnLangBrowse);
            tabPage1_Gnrl.Controls.Add(btnExportSettings);
            tabPage1_Gnrl.Controls.Add(btnCheckUpdates);
            tabPage1_Gnrl.Controls.Add(lblNetworkTimeOut);
            tabPage1_Gnrl.Controls.Add(nudNetworkTimeOut);
            tabPage1_Gnrl.Controls.Add(chbAutoUpdate);
            tabPage1_Gnrl.Location = new Point(4, 0x16);
            tabPage1_Gnrl.Padding = new Padding(3);
            tabPage1_Gnrl.Size = new Size(0x1ff, 0x1d7);
            tabPage1_Gnrl.TabIndex = 0;
            tabPage1_Gnrl.UseVisualStyleBackColor = true;
            btnCheckUpdates.AutoSize = true;
            btnCheckUpdates.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            btnCheckUpdates.Location = new Point(0x12a, 0x1f1);
            btnCheckUpdates.TabIndex = 20;
            btnCheckUpdates.Click += btnCheckUpdates_Click;
            chbAutoUpdate.AutoSize = true;
            chbAutoUpdate.Location = new Point(0x1b, 0x1f3);
            chbAutoUpdate.TabIndex = 0x13;
            btnExportSettings.AutoSize = true;
            btnExportSettings.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            btnExportSettings.Location = new Point(0x1b, 0x1c7);
            btnExportSettings.TabIndex = 0x12;
            btnExportSettings.Click += btnExportSettings_Click;
            lblNetworkTimeOut.AutoSize = true;
            lblNetworkTimeOut.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            lblNetworkTimeOut.Location = new Point(0x1b, 0x1a5);
            nudNetworkTimeOut.Location = new Point(0x194, 420);
            int[] bits = new int[4];
            bits[0] = 60;
            nudNetworkTimeOut.Maximum = new decimal(bits);
            int[] numArray2 = new int[4];
            nudNetworkTimeOut.Minimum = new decimal(numArray2);
            nudNetworkTimeOut.Size = new Size(0x33, 0x15);
            nudNetworkTimeOut.TabIndex = 0x11;
            nudNetworkTimeOut.TextAlign = HorizontalAlignment.Right;
            int[] numArray3 = new int[4];
            numArray3[0] = 6;
            nudNetworkTimeOut.Value = new decimal(numArray3);
            btnLangBrowse.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnLangBrowse.Location = new Point(0x1a7, 0x175);
            btnLangBrowse.Size = new Size(0x22, 0x19);
            btnLangBrowse.TabIndex = 0x10;
            btnLangBrowse.Text = "...";
            btnLangBrowse.Click += btnLangBrowse_Click;
            textBoxLang.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            textBoxLang.Location = new Point(0x2d, 0x178);
            textBoxLang.Size = new Size(0x174, 0x15);
            textBoxLang.MaxLength = 260;
            textBoxLang.TabIndex = 15;
            textBoxLang.KeyPress += textBoxesPath_KeyPress;
            lblLang.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            lblLang.AutoSize = true;
            lblLang.Location = new Point(0x1b, 0x160);
            chbPlaySound.AutoSize = true;
            chbPlaySound.Location = new Point(0x1b, 0x111);
            chbPlaySound.TabIndex = 14;
            chbDD.AutoSize = true;
            chbDD.Location = new Point(0x1b, 0xf5);
            chbDD.TabIndex = 13;
            btnClearRecentFile.Location = new Point(0x163, 0xd7);
            btnClearRecentFile.Size = new Size(100, 0x17);
            btnClearRecentFile.TabIndex = 12;
            btnClearRecentFile.Text = "Clear";
            btnClearRecentFile.Click += btnClearRecentFile_Click;
            nudMaxRecentFile.Location = new Point(0x12a, 0xd8);
            int[] numArray4 = new int[4];
            numArray4[0] = 0x40;
            nudMaxRecentFile.Maximum = new decimal(numArray4);
            int[] numArray5 = new int[4];
            numArray5[0] = 1;
            nudMaxRecentFile.Minimum = new decimal(numArray5);
            nudMaxRecentFile.Size = new Size(0x33, 0x15);
            nudMaxRecentFile.TabIndex = 11;
            nudMaxRecentFile.TextAlign = HorizontalAlignment.Right;
            int[] numArray6 = new int[4];
            numArray6[0] = 1;
            nudMaxRecentFile.Value = new decimal(numArray6);
            chbSaveExecuted.AutoSize = true;
            chbSaveExecuted.Location = new Point(0x1b, 0xd9);
            chbSaveExecuted.ThreeState = true;
            chbSaveExecuted.TabIndex = 10;
            btnHistoryClear.Location = new Point(0x163, 0xbb);
            btnHistoryClear.Size = new Size(100, 0x17);
            btnHistoryClear.TabIndex = 9;
            btnHistoryClear.Text = "Clear";
            btnHistoryClear.Click += buttonHistoryClear_Click;
            nudMaxUndo.Location = new Point(0x12a, 0xbc);
            int[] numArray7 = new int[4];
            numArray7[0] = 0x40;
            nudMaxUndo.Maximum = new decimal(numArray7);
            int[] numArray8 = new int[4];
            numArray8[0] = 1;
            nudMaxUndo.Minimum = new decimal(numArray8);
            nudMaxUndo.Size = new Size(0x33, 0x15);
            nudMaxUndo.TabIndex = 8;
            nudMaxUndo.TextAlign = HorizontalAlignment.Right;
            int[] numArray9 = new int[4];
            numArray9[0] = 1;
            nudMaxUndo.Value = new decimal(numArray9);
            chbNoHistory.AutoSize = true;
            chbNoHistory.Location = new Point(0x1b, 0xbd);
            chbNoHistory.TabIndex = 7;
            cmbNavBtn.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbNavBtn.Location = new Point(0x176, 0x9f);
            cmbNavBtn.Size = new Size(0x51, 0x15);
            cmbNavBtn.TabIndex = 6;
            chbNavBtn.AutoSize = true;
            chbNavBtn.Location = new Point(0x1b, 0xa1);
            chbNavBtn.TabIndex = 5;
            chbNavBtn.CheckedChanged += chbNavBtn_CheckedChanged;
            chbX1X2.AutoSize = true;
            chbX1X2.Location = new Point(0x1b, 0x85);
            chbX1X2.TabIndex = 4;
            chbShowTooltip.AutoSize = true;
            chbShowTooltip.Location = new Point(0x1b, 0x69);
            chbShowTooltip.TabIndex = 3;
            chbCloseWhenGroup.AutoSize = true;
            chbCloseWhenGroup.Location = new Point(0x1b, 0x4d);
            chbCloseWhenGroup.TabIndex = 2;
            chbDontOpenSame.AutoSize = true;
            chbDontOpenSame.Location = new Point(0x1b, 0x31);
            chbDontOpenSame.TabIndex = 1;
            chbActivateNew.AutoSize = true;
            chbActivateNew.Location = new Point(0x1b, 0x15);
            chbActivateNew.TabIndex = 0;
            tabPage2_Tabs.Controls.Add(lblNewTabLoc);
            tabPage2_Tabs.Controls.Add(lblActvClose);
            tabPage2_Tabs.Controls.Add(lblTabDblClk);
            tabPage2_Tabs.Controls.Add(lblBGDblClik);
            tabPage2_Tabs.Controls.Add(lblTabWhlClk);
            tabPage2_Tabs.Controls.Add(lblAction_BarDblClick);
            tabPage2_Tabs.Controls.Add(lblMultiRows);
            tabPage2_Tabs.Controls.Add(cmbNewTabLoc);
            tabPage2_Tabs.Controls.Add(cmbActvClose);
            tabPage2_Tabs.Controls.Add(cmbTabDblClck);
            tabPage2_Tabs.Controls.Add(cmbBGDblClick);
            tabPage2_Tabs.Controls.Add(cmbTabWhlClck);
            tabPage2_Tabs.Controls.Add(cmbMultiRow);
            tabPage2_Tabs.Controls.Add(textBoxAction_BarDblClck);
            tabPage2_Tabs.Controls.Add(btnBrowseAction_BarDblClck);
            tabPage2_Tabs.Controls.Add(chbAutoSubText);
            tabPage2_Tabs.Controls.Add(chbTabCloseButton);
            tabPage2_Tabs.Controls.Add(chbTabCloseBtnAlt);
            tabPage2_Tabs.Controls.Add(chbTabCloseBtnHover);
            tabPage2_Tabs.Controls.Add(chbFolderIcon);
            tabPage2_Tabs.Controls.Add(chbSubDirTipOnTab);
            tabPage2_Tabs.Controls.Add(chbDriveLetter);
            tabPage2_Tabs.Controls.Add(chbTabSwitcher);
            tabPage2_Tabs.Controls.Add(chbTreeShftWhlTab);
            tabPage2_Tabs.Controls.Add(chbRemoveOnSeparate);
            tabPage2_Tabs.Location = new Point(4, 0x16);
            tabPage2_Tabs.Padding = new Padding(3);
            tabPage2_Tabs.Size = new Size(0x1ff, 0x1d7);
            tabPage2_Tabs.TabIndex = 1;
            tabPage2_Tabs.UseVisualStyleBackColor = true;
            chbRemoveOnSeparate.AutoSize = true;
            chbRemoveOnSeparate.Location = new Point(0x1b, 0x1f2);
            chbRemoveOnSeparate.TabIndex = 0x11;
            chbTreeShftWhlTab.AutoSize = true;
            chbTreeShftWhlTab.Location = new Point(0x1b, 0x1da);
            chbTreeShftWhlTab.TabIndex = 0x10;
            chbTabSwitcher.AutoSize = true;
            chbTabSwitcher.Location = new Point(0x1b, 450);
            chbTabSwitcher.TabIndex = 15;
            chbDriveLetter.AutoSize = true;
            chbDriveLetter.Location = new Point(0x36, 0x1aa);
            chbDriveLetter.TabIndex = 14;
            chbSubDirTipOnTab.AutoSize = true;
            chbSubDirTipOnTab.Location = new Point(0x36, 0x196);
            chbSubDirTipOnTab.TabIndex = 13;
            chbFolderIcon.AutoSize = true;
            chbFolderIcon.Location = new Point(0x1b, 0x182);
            chbFolderIcon.TabIndex = 12;
            chbFolderIcon.CheckedChanged += chbFolderIcon_CheckedChanged;
            chbTabCloseBtnHover.AutoSize = true;
            chbTabCloseBtnHover.Location = new Point(0x36, 0x16a);
            chbTabCloseBtnHover.TabIndex = 11;
            chbTabCloseBtnHover.CheckedChanged += chbTabCloseBtns_CheckedChanged;
            chbTabCloseBtnAlt.AutoSize = true;
            chbTabCloseBtnAlt.Location = new Point(0x36, 0x156);
            chbTabCloseBtnAlt.TabIndex = 10;
            chbTabCloseBtnAlt.CheckedChanged += chbTabCloseBtns_CheckedChanged;
            chbTabCloseButton.AutoSize = true;
            chbTabCloseButton.Location = new Point(0x1b, 0x142);
            chbTabCloseButton.TabIndex = 9;
            chbTabCloseButton.CheckedChanged += chbTabCloseButton_CheckedChanged;
            chbAutoSubText.AutoSize = true;
            chbAutoSubText.Location = new Point(0x1b, 0x12a);
            chbAutoSubText.TabIndex = 8;
            cmbMultiRow.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMultiRow.Location = new Point(0x11d, 0x102);
            cmbMultiRow.Size = new Size(0xa8, 0x15);
            cmbMultiRow.TabIndex = 7;
            lblMultiRows.AutoSize = true;
            lblMultiRows.Location = new Point(0x19, 0x105);
            btnBrowseAction_BarDblClck.Location = new Point(0x1a3, 0xd5);
            btnBrowseAction_BarDblClck.Size = new Size(0x22, 0x19);
            btnBrowseAction_BarDblClck.TabIndex = 6;
            btnBrowseAction_BarDblClck.Text = "...";
            btnBrowseAction_BarDblClck.Click += btnBrowseAction_Click;
            textBoxAction_BarDblClck.Location = new Point(0x97, 0xd7);
            textBoxAction_BarDblClck.Size = new Size(0x107, 0x15);
            textBoxAction_BarDblClck.MaxLength = 260;
            textBoxAction_BarDblClck.TabIndex = 5;
            lblAction_BarDblClick.AutoSize = true;
            lblAction_BarDblClick.Location = new Point(0x2e, 0xda);
            cmbBGDblClick.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBGDblClick.Location = new Point(0x11d, 0xb1);
            cmbBGDblClick.Size = new Size(0xa8, 0x15);
            cmbBGDblClick.TabIndex = 4;
            cmbBGDblClick.SelectedIndexChanged += comboBoxes_SelectedIndexChanged;
            lblBGDblClik.AutoSize = true;
            lblBGDblClik.Location = new Point(0x19, 0xb5);
            cmbTabWhlClck.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTabWhlClck.Location = new Point(0x11d, 0x89);
            cmbTabWhlClck.Size = new Size(0xa8, 0x15);
            cmbTabWhlClck.TabIndex = 3;
            lblTabWhlClk.AutoSize = true;
            lblTabWhlClk.Location = new Point(0x19, 0x8d);
            cmbTabDblClck.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTabDblClck.Location = new Point(0x11d, 0x61);
            cmbTabDblClck.Size = new Size(0xa8, 0x15);
            cmbTabDblClck.TabIndex = 2;
            lblTabDblClk.AutoSize = true;
            lblTabDblClk.Location = new Point(0x19, 0x65);
            cmbActvClose.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbActvClose.Location = new Point(0x11d, 0x39);
            cmbActvClose.Size = new Size(0xa8, 0x15);
            cmbActvClose.TabIndex = 1;
            lblActvClose.AutoSize = true;
            lblActvClose.Location = new Point(0x19, 0x3d);
            cmbNewTabLoc.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbNewTabLoc.Location = new Point(0x11d, 0x11);
            cmbNewTabLoc.Size = new Size(0xa8, 0x15);
            cmbNewTabLoc.TabIndex = 0;
            lblNewTabLoc.AutoSize = true;
            lblNewTabLoc.Location = new Point(0x19, 0x15);
            tabPage3_Wndw.Controls.Add(chbWhlClick);
            tabPage3_Wndw.Controls.Add(chbNCADblClck);
            tabPage3_Wndw.Controls.Add(chbBlockProcess);
            tabPage3_Wndw.Controls.Add(chbFoldrTree);
            tabPage3_Wndw.Controls.Add(chbWndUnresizable);
            tabPage3_Wndw.Controls.Add(chbWndRestrAlpha);
            tabPage3_Wndw.Controls.Add(chbNoTabFromOuteside);
            tabPage3_Wndw.Controls.Add(chbHolizontalScroll);
            tabPage3_Wndw.Controls.Add(chbWhlChangeView);
            tabPage3_Wndw.Controls.Add(chbNeverCloseWindow);
            tabPage3_Wndw.Controls.Add(chbNeverCloseWndLocked);
            tabPage3_Wndw.Controls.Add(chbRestoreClosed);
            tabPage3_Wndw.Controls.Add(chbRestoreLocked);
            tabPage3_Wndw.Controls.Add(chbSendToTray);
            tabPage3_Wndw.Controls.Add(chbSendToTrayOnMinimize);
            tabPage3_Wndw.Controls.Add(cmbWhlClick);
            tabPage3_Wndw.Controls.Add(lblSep);
            tabPage3_Wndw.Location = new Point(4, 0x16);
            tabPage3_Wndw.Padding = new Padding(3);
            tabPage3_Wndw.Size = new Size(0x1ff, 0x1d7);
            tabPage3_Wndw.TabIndex = 4;
            tabPage3_Wndw.UseVisualStyleBackColor = true;
            lblSep.BorderStyle = BorderStyle.Fixed3D;
            lblSep.Location = new Point(0x1a, 0x115);
            lblSep.Margin = new Padding(0);
            lblSep.Size = new Size(0x149, 2);
            cmbWhlClick.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbWhlClick.Location = new Point(0x11d, 0x11);
            cmbWhlClick.Size = new Size(0x6f, 0x15);
            cmbWhlClick.TabIndex = 1;
            chbSendToTrayOnMinimize.AutoSize = true;
            chbSendToTrayOnMinimize.Location = new Point(0x1b, 0x1b3);
            chbSendToTrayOnMinimize.TabIndex = 13;
            chbSendToTray.AutoSize = true;
            chbSendToTray.Location = new Point(0x1b, 0x197);
            chbSendToTray.TabIndex = 12;
            chbRestoreLocked.AutoSize = true;
            chbRestoreLocked.Location = new Point(0x1b, 0x17b);
            chbRestoreLocked.TabIndex = 11;
            chbRestoreLocked.CheckedChanged += chbsCloseWindow_CheckedChanged;
            chbRestoreClosed.AutoSize = true;
            chbRestoreClosed.Location = new Point(0x1b, 0x15f);
            chbRestoreClosed.TabIndex = 10;
            chbRestoreClosed.CheckedChanged += chbsCloseWindow_CheckedChanged;
            chbNeverCloseWndLocked.AutoSize = true;
            chbNeverCloseWndLocked.Location = new Point(0x1b, 0x143);
            chbNeverCloseWndLocked.TabIndex = 9;
            chbNeverCloseWndLocked.CheckedChanged += chbsCloseWindow_CheckedChanged;
            chbNeverCloseWindow.AutoSize = true;
            chbNeverCloseWindow.Location = new Point(0x1b, 0x127);
            chbNeverCloseWindow.TabIndex = 8;
            chbWhlChangeView.AutoSize = true;
            chbWhlChangeView.Location = new Point(0x1b, 0xf5);
            chbWhlChangeView.TabIndex = 9;
            chbHolizontalScroll.AutoSize = true;
            chbHolizontalScroll.Location = new Point(0x1b, 0xd9);
            chbHolizontalScroll.TabIndex = 8;
            chbNoTabFromOuteside.AutoSize = true;
            chbNoTabFromOuteside.Location = new Point(0x1b, 0xbd);
            chbNoTabFromOuteside.TabIndex = 7;
            chbFoldrTree.AutoSize = true;
            chbFoldrTree.Location = new Point(0x1b, 0xa1);
            chbFoldrTree.TabIndex = 6;
            chbBlockProcess.AutoSize = true;
            chbBlockProcess.Location = new Point(0x1b, 0x85);
            chbBlockProcess.TabIndex = 5;
            chbWndRestrAlpha.AutoSize = true;
            chbWndRestrAlpha.Location = new Point(0x1b, 0x69);
            chbWndRestrAlpha.TabIndex = 4;
            chbWndUnresizable.AutoSize = true;
            chbWndUnresizable.Location = new Point(0x1b, 0x4d);
            chbWndUnresizable.TabIndex = 3;
            chbNCADblClck.AutoSize = true;
            chbNCADblClck.Location = new Point(0x1b, 0x31);
            chbNCADblClck.TabIndex = 2;
            chbWhlClick.AutoSize = true;
            chbWhlClick.Location = new Point(0x1b, 0x15);
            chbWhlClick.TabIndex = 0;
            chbWhlClick.CheckedChanged += chbMMButton_CheckedChanged;
            tabPage4_View.Controls.Add(chbUseTabSkin);
            tabPage4_View.Controls.Add(chbBoldActv);
            tabPage4_View.Controls.Add(chbToolbarBGClr);
            tabPage4_View.Controls.Add(chbRebarBGImage);
            tabPage4_View.Controls.Add(chbTabTitleShadow);
            tabPage4_View.Controls.Add(propertyGrid1);
            tabPage4_View.Controls.Add(nudTabWidth);
            tabPage4_View.Controls.Add(nudTabHeight);
            tabPage4_View.Controls.Add(nudTabWidthMax);
            tabPage4_View.Controls.Add(nudTabWidthMin);
            tabPage4_View.Controls.Add(lblTabSizeTitle);
            tabPage4_View.Controls.Add(lblTabWidth);
            tabPage4_View.Controls.Add(lblTabHeight);
            tabPage4_View.Controls.Add(lblTabWFix);
            tabPage4_View.Controls.Add(lblTabWMax);
            tabPage4_View.Controls.Add(lblTabWMin);
            tabPage4_View.Controls.Add(lblTabFont);
            tabPage4_View.Controls.Add(lblTabTxtClr);
            tabPage4_View.Controls.Add(lblTabTextAlignment);
            tabPage4_View.Controls.Add(lblMenuRenderer);
            tabPage4_View.Controls.Add(cmbTabSizeMode);
            tabPage4_View.Controls.Add(cmbTabTextAlignment);
            tabPage4_View.Controls.Add(cmbRebarBGImageMode);
            tabPage4_View.Controls.Add(cmbMenuRenderer);
            tabPage4_View.Controls.Add(btnHiliteClsc);
            tabPage4_View.Controls.Add(btnTabFont);
            tabPage4_View.Controls.Add(btnActTxtClr);
            tabPage4_View.Controls.Add(btnInactTxtClr);
            tabPage4_View.Controls.Add(btnDefTxtClr);
            tabPage4_View.Controls.Add(btnToolBarBGClr);
            tabPage4_View.Controls.Add(btnRebarImage);
            tabPage4_View.Controls.Add(btnShadowAct);
            tabPage4_View.Controls.Add(btnShadowIna);
            tabPage4_View.Controls.Add(btnTabImage);
            tabPage4_View.Controls.Add(tbRebarImagePath);
            tabPage4_View.Controls.Add(tbTabImagePath);
            tabPage4_View.Location = new Point(4, 0x16);
            tabPage4_View.Size = new Size(0x1ff, 0x1d7);
            tabPage4_View.TabIndex = 3;
            tabPage4_View.UseVisualStyleBackColor = true;
            cmbMenuRenderer.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMenuRenderer.Location = new Point(0x99, 0x1df);
            cmbMenuRenderer.Size = new Size(100, 0x15);
            cmbMenuRenderer.TabIndex = 0x19;
            lblMenuRenderer.AutoSize = true;
            lblMenuRenderer.Location = new Point(13, 0x1e1);
            cmbRebarBGImageMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRebarBGImageMode.Location = new Point(0xdf, 0x1c0);
            cmbRebarBGImageMode.Size = new Size(180, 0x15);
            cmbRebarBGImageMode.TabIndex = 0x18;
            btnRebarImage.Location = new Point(440, 0x1a6);
            btnRebarImage.Size = new Size(0x22, 0x19);
            btnRebarImage.TabIndex = 0x17;
            btnRebarImage.Text = "...";
            btnRebarImage.Click += btnRebarImage_Click;
            tbRebarImagePath.Location = new Point(0xdf, 0x1a7);
            tbRebarImagePath.Size = new Size(0xd5, 0x15);
            tbRebarImagePath.MaxLength = 260;
            tbRebarImagePath.TabIndex = 0x16;
            tbRebarImagePath.KeyPress += textBoxesPath_KeyPress;
            chbRebarBGImage.AutoSize = true;
            chbRebarBGImage.Location = new Point(0x10, 0x1a9);
            chbRebarBGImage.TabIndex = 0x15;
            chbRebarBGImage.CheckedChanged += chbRebarBGImage_CheckedChanged;
            btnToolBarBGClr.Location = new Point(0xdf, 0x18b);
            btnToolBarBGClr.Size = new Size(180, 0x17);
            btnToolBarBGClr.TabIndex = 20;
            btnToolBarBGClr.Click += buttonToolBarBGClr_Click;
            chbToolbarBGClr.AutoSize = true;
            chbToolbarBGClr.Location = new Point(0x10, 400);
            chbToolbarBGClr.TabIndex = 0x13;
            chbToolbarBGClr.CheckedChanged += chbToolbarBGClr_CheckedChanged;
            btnShadowIna.AutoSize = true;
            btnShadowIna.Location = new Point(0x107, 0x160);
            btnShadowIna.Size = new Size(100, 0x17);
            btnShadowIna.TabIndex = 0x12;
            btnShadowIna.Click += btnShadowClrs_Click;
            btnShadowAct.AutoSize = true;
            btnShadowAct.Location = new Point(0x99, 0x160);
            btnShadowAct.Size = new Size(100, 0x17);
            btnShadowAct.TabIndex = 0x11;
            btnShadowAct.Click += btnShadowClrs_Click;
            chbTabTitleShadow.AutoSize = true;
            chbTabTitleShadow.Location = new Point(0x10, 0x165);
            chbTabTitleShadow.TabIndex = 0x10;
            chbTabTitleShadow.CheckedChanged += chbTabTitleShadow_CheckedChanged;
            btnDefTxtClr.AutoSize = true;
            btnDefTxtClr.Location = new Point(0x175, 0x13f);
            btnDefTxtClr.Size = new Size(100, 0x17);
            btnDefTxtClr.TabIndex = 15;
            btnDefTxtClr.Click += buttonRstClr_Click;
            btnInactTxtClr.AutoSize = true;
            btnInactTxtClr.Location = new Point(0x107, 0x13f);
            btnInactTxtClr.Size = new Size(100, 0x17);
            btnInactTxtClr.TabIndex = 14;
            btnInactTxtClr.Click += buttonInactClr_Click;
            btnActTxtClr.AutoSize = true;
            btnActTxtClr.Location = new Point(0x99, 0x13f);
            btnActTxtClr.Size = new Size(100, 0x17);
            btnActTxtClr.TabIndex = 13;
            btnActTxtClr.Click += buttonActClr_Click;
            lblTabTxtClr.AutoSize = true;
            lblTabTxtClr.Location = new Point(13, 0x144);
            chbBoldActv.AutoSize = true;
            chbBoldActv.Location = new Point(0x109, 0x121);
            chbBoldActv.TabIndex = 12;
            btnTabFont.Location = new Point(0x99, 0x11c);
            btnTabFont.Size = new Size(100, 0x19);
            btnTabFont.TabIndex = 11;
            btnTabFont.Click += btnFont_Click;
            lblTabFont.AutoSize = true;
            lblTabFont.Location = new Point(13, 290);
            cmbTabTextAlignment.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTabTextAlignment.Location = new Point(0x99, 0xfe);
            cmbTabTextAlignment.Size = new Size(100, 0x15);
            cmbTabTextAlignment.TabIndex = 10;
            lblTabTextAlignment.Location = new Point(13, 0x100);
            lblTabTextAlignment.AutoSize = true;
            nudTabWidthMin.Location = new Point(0x152, 0xdf);
            int[] numArray10 = new int[4];
            numArray10[0] = 0x200;
            nudTabWidthMin.Maximum = new decimal(numArray10);
            int[] numArray11 = new int[4];
            numArray11[0] = 10;
            nudTabWidthMin.Minimum = new decimal(numArray11);
            nudTabWidthMin.Size = new Size(0x33, 0x15);
            nudTabWidthMin.TabIndex = 9;
            nudTabWidthMin.TextAlign = HorizontalAlignment.Center;
            int[] numArray12 = new int[4];
            numArray12[0] = 0x19;
            nudTabWidthMin.Value = new decimal(numArray12);
            nudTabWidthMin.ValueChanged += numericUpDownMax_ValueChanged;
            lblTabWMin.Location = new Point(0xfc, 0xdf);
            lblTabWMin.Size = new Size(0x4c, 0x15);
            lblTabWMin.TextAlign = ContentAlignment.MiddleRight;
            nudTabWidthMax.Location = new Point(0x152, 0xc5);
            int[] numArray13 = new int[4];
            numArray13[0] = 0x200;
            nudTabWidthMax.Maximum = new decimal(numArray13);
            int[] numArray14 = new int[4];
            numArray14[0] = 10;
            nudTabWidthMax.Minimum = new decimal(numArray14);
            nudTabWidthMax.Size = new Size(0x33, 0x15);
            nudTabWidthMax.TabIndex = 8;
            nudTabWidthMax.TextAlign = HorizontalAlignment.Center;
            int[] numArray15 = new int[4];
            numArray15[0] = 0x19;
            nudTabWidthMax.Value = new decimal(numArray15);
            nudTabWidthMax.ValueChanged += numericUpDownMax_ValueChanged;
            lblTabWMax.Location = new Point(0xfc, 0xc5);
            lblTabWMax.Size = new Size(0x4c, 0x15);
            lblTabWMax.TextAlign = ContentAlignment.MiddleRight;
            nudTabWidth.Location = new Point(0x152, 0xab);
            int[] numArray16 = new int[4];
            numArray16[0] = 0x200;
            nudTabWidth.Maximum = new decimal(numArray16);
            int[] numArray17 = new int[4];
            numArray17[0] = 10;
            nudTabWidth.Minimum = new decimal(numArray17);
            nudTabWidth.Size = new Size(0x33, 0x15);
            nudTabWidth.TabIndex = 7;
            nudTabWidth.TextAlign = HorizontalAlignment.Center;
            int[] numArray18 = new int[4];
            numArray18[0] = 0x18;
            nudTabWidth.Value = new decimal(numArray18);
            lblTabWFix.Location = new Point(0x102, 0xab);
            lblTabWFix.Size = new Size(70, 0x15);
            lblTabWFix.TextAlign = ContentAlignment.MiddleRight;
            cmbTabSizeMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTabSizeMode.Location = new Point(0x99, 170);
            cmbTabSizeMode.Size = new Size(100, 0x15);
            cmbTabSizeMode.TabIndex = 6;
            cmbTabSizeMode.SelectedIndexChanged += comboBoxes_SelectedIndexChanged;
            nudTabHeight.Location = new Point(0x99, 140);
            int[] numArray19 = new int[4];
            numArray19[0] = 50;
            nudTabHeight.Maximum = new decimal(numArray19);
            int[] numArray20 = new int[4];
            numArray20[0] = 10;
            nudTabHeight.Minimum = new decimal(numArray20);
            nudTabHeight.Size = new Size(0x33, 0x15);
            nudTabHeight.TabIndex = 5;
            nudTabHeight.TextAlign = HorizontalAlignment.Center;
            int[] numArray21 = new int[4];
            numArray21[0] = 0x18;
            nudTabHeight.Value = new decimal(numArray21);
            lblTabWidth.AutoSize = true;
            lblTabWidth.Location = new Point(0x4a, 0xac);
            lblTabHeight.AutoSize = true;
            lblTabHeight.Location = new Point(0x4a, 0x8f);
            lblTabSizeTitle.AutoSize = true;
            lblTabSizeTitle.Location = new Point(13, 0x79);
            propertyGrid1.HelpVisible = false;
            propertyGrid1.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            propertyGrid1.Location = new Point(15, 0x26);
            propertyGrid1.PropertySort = PropertySort.NoSort;
            propertyGrid1.Size = new Size(0x176, 0x48);
            propertyGrid1.TabIndex = 3;
            propertyGrid1.ToolbarVisible = false;
            btnHiliteClsc.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnHiliteClsc.AutoSize = true;
            btnHiliteClsc.Location = new Point(0x1a3, 0x26);
            btnHiliteClsc.Size = new Size(0x4b, 0x17);
            btnHiliteClsc.TabIndex = 4;
            btnHiliteClsc.Click += buttonHL_Click;
            btnTabImage.Location = new Point(440, 11);
            btnTabImage.Size = new Size(0x22, 0x19);
            btnTabImage.TabIndex = 2;
            btnTabImage.Text = "...";
            btnTabImage.Click += btnTabImage_Click;
            tbTabImagePath.Location = new Point(0xdf, 12);
            tbTabImagePath.Size = new Size(0xd5, 0x15);
            tbTabImagePath.MaxLength = 260;
            tbTabImagePath.TabIndex = 1;
            tbTabImagePath.KeyPress += textBoxesPath_KeyPress;
            chbUseTabSkin.AutoSize = true;
            chbUseTabSkin.Location = new Point(15, 14);
            chbUseTabSkin.TabIndex = 0;
            chbUseTabSkin.CheckedChanged += chbDrawMode_CheckedChanged;
            tabPage5_Grps.Controls.Add(btnUp_Grp);
            tabPage5_Grps.Controls.Add(btnDown_Grp);
            tabPage5_Grps.Controls.Add(btnAddSep_Grp);
            tabPage5_Grps.Controls.Add(btnStartUpGrp);
            tabPage5_Grps.Controls.Add(btnPlus_Grp);
            tabPage5_Grps.Controls.Add(btnMinus_Grp);
            tabPage5_Grps.Controls.Add(treeViewGroup);
            tabPage5_Grps.Controls.Add(cmbSpclFol_Grp);
            tabPage5_Grps.Controls.Add(btnAddSpcFol_Grp);
            tabPage5_Grps.Controls.Add(lblGroupKey);
            tabPage5_Grps.Controls.Add(tbGroupKey);
            tabPage5_Grps.Controls.Add(chbGroupKey);
            tabPage5_Grps.Location = new Point(4, 0x16);
            tabPage5_Grps.Padding = new Padding(3);
            tabPage5_Grps.Size = new Size(0x1ff, 0x1d7);
            tabPage5_Grps.TabIndex = 2;
            tabPage5_Grps.UseVisualStyleBackColor = true;
            treeViewGroup.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            treeViewGroup.HideSelection = false;
            treeViewGroup.ImageKey = "noimage";
            treeViewGroup.SelectedImageKey = "noimage";
            treeViewGroup.ImageList = QTUtility.ImageListGlobal;
            treeViewGroup.LabelEdit = true;
            treeViewGroup.Location = new Point(5, 0x2d);
            treeViewGroup.ShowNodeToolTips = true;
            treeViewGroup.Size = new Size(0x1ed, 0x156);
            treeViewGroup.TabIndex = 6;
            treeViewGroup.AfterSelect += treeViewGroup_AfterSelect;
            treeViewGroup.BeforeLabelEdit += treeViewGroup_BeforeLabelEdit;
            treeViewGroup.AfterLabelEdit += treeViewGroup_AfterLabelEdit;
            treeViewGroup.KeyDown += treeViewGroup_KeyDown;
            btnUp_Grp.Enabled = false;
            btnUp_Grp.Location = new Point(5, 0x10);
            btnUp_Grp.Size = new Size(50, 0x17);
            btnUp_Grp.TabIndex = 0;
            btnUp_Grp.Click += UpDownButtons_Click;
            btnDown_Grp.Enabled = false;
            btnDown_Grp.Location = new Point(0x3d, 0x10);
            btnDown_Grp.Size = new Size(50, 0x17);
            btnDown_Grp.TabIndex = 1;
            btnDown_Grp.Click += UpDownButtons_Click;
            btnAddSep_Grp.Location = new Point(0x75, 0x10);
            btnAddSep_Grp.Size = new Size(120, 0x17);
            btnAddSep_Grp.TabIndex = 2;
            btnAddSep_Grp.Click += btnAddSep_Click;
            btnStartUpGrp.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnStartUpGrp.Location = new Point(0x150, 0x10);
            btnStartUpGrp.Size = new Size(100, 0x17);
            btnStartUpGrp.TabIndex = 3;
            btnStartUpGrp.Click += btnStartUpGrp_Click;
            btnPlus_Grp.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnPlus_Grp.Location = new Point(0x1ba, 0x10);
            btnPlus_Grp.Size = new Size(0x19, 0x17);
            btnPlus_Grp.TabIndex = 4;
            btnPlus_Grp.Text = "+";
            btnPlus_Grp.Click += btnPlus_Click;
            btnMinus_Grp.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnMinus_Grp.Location = new Point(0x1d9, 0x10);
            btnMinus_Grp.Size = new Size(0x19, 0x17);
            btnMinus_Grp.TabIndex = 5;
            btnMinus_Grp.Text = "-";
            btnMinus_Grp.Click += btnMinus_Click;
            cmbSpclFol_Grp.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            cmbSpclFol_Grp.Enabled = false;
            cmbSpclFol_Grp.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSpclFol_Grp.Location = new Point(5, 0x187);
            cmbSpclFol_Grp.Size = new Size(150, 0x15);
            cmbSpclFol_Grp.TabIndex = 7;
            btnAddSpcFol_Grp.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            btnAddSpcFol_Grp.Enabled = false;
            btnAddSpcFol_Grp.Location = new Point(0x9e, 390);
            btnAddSpcFol_Grp.Size = new Size(0x19, 0x17);
            btnAddSpcFol_Grp.TabIndex = 8;
            btnAddSpcFol_Grp.Text = "+";
            btnAddSpcFol_Grp.Click += btnAddSpcFol_Grp_Click;
            lblGroupKey.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            lblGroupKey.AutoSize = true;
            lblGroupKey.Location = new Point(6, 0x1ac);
            lblGroupKey.Size = new Size(0x61, 13);
            chbGroupKey.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            chbGroupKey.AutoSize = true;
            chbGroupKey.Enabled = false;
            chbGroupKey.Location = new Point(0x6b, 420);
            chbGroupKey.TabIndex = 9;
            chbGroupKey.CheckedChanged += chbGroupKey_CheckedChanged;
            tbGroupKey.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            tbGroupKey.Enabled = false;
            tbGroupKey.Location = new Point(0x7f, 0x1a9);
            tbGroupKey.Size = new Size(340, 0x15);
            tbGroupKey.TextAlign = HorizontalAlignment.Center;
            tbGroupKey.TabIndex = 10;
            tbGroupKey.PreviewKeyDown += tbGroupKey_PreviewKeyDown;
            tbGroupKey.KeyPress += tbGroupKey_KeyPress;
            tabPage6_Apps.Controls.Add(treeViewUserApps);
            tabPage6_Apps.Controls.Add(btnUp_app);
            tabPage6_Apps.Controls.Add(btnDown_app);
            tabPage6_Apps.Controls.Add(btnAddSep_app);
            tabPage6_Apps.Controls.Add(btnAddVFolder_app);
            tabPage6_Apps.Controls.Add(btnPlus_app);
            tabPage6_Apps.Controls.Add(btnMinus_app);
            tabPage6_Apps.Controls.Add(lblUserApps_Path);
            tabPage6_Apps.Controls.Add(lblUserApps_Args);
            tabPage6_Apps.Controls.Add(lblUserApps_Working);
            tabPage6_Apps.Controls.Add(lblUserApps_Key);
            tabPage6_Apps.Controls.Add(tbPath);
            tabPage6_Apps.Controls.Add(tbArgs);
            tabPage6_Apps.Controls.Add(tbWorking);
            tabPage6_Apps.Controls.Add(chbUserAppKey);
            tabPage6_Apps.Controls.Add(tbUserAppKey);
            tabPage6_Apps.Controls.Add(btnOFD_app);
            tabPage6_Apps.Controls.Add(btnBFD_app);
            tabPage6_Apps.Controls.Add(btnAddToken_Arg);
            tabPage6_Apps.Controls.Add(btnAddToken_Wrk);
            tabPage6_Apps.Location = new Point(4, 0x16);
            tabPage6_Apps.Padding = new Padding(3);
            tabPage6_Apps.Size = new Size(0x1ff, 0x1d7);
            tabPage6_Apps.TabIndex = 5;
            tabPage6_Apps.UseVisualStyleBackColor = true;
            treeViewUserApps.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            treeViewUserApps.HideSelection = false;
            treeViewUserApps.ImageKey = "noimage";
            treeViewUserApps.SelectedImageKey = "noimage";
            treeViewUserApps.ImageList = QTUtility.ImageListGlobal;
            treeViewUserApps.LabelEdit = true;
            treeViewUserApps.Location = new Point(5, 0x2d);
            treeViewUserApps.Size = new Size(0x1ed, 0x12e);
            treeViewUserApps.TabIndex = 6;
            treeViewUserApps.AfterLabelEdit += treeViewUserApps_AfterLabelEdit;
            treeViewUserApps.AfterSelect += treeViewUserApps_AfterSelect;
            treeViewUserApps.BeforeLabelEdit += treeViewUserApps_BeforeLabelEdit;
            treeViewUserApps.KeyDown += treeViewUserApps_KeyDown;
            btnUp_app.Enabled = false;
            btnUp_app.Location = new Point(5, 0x10);
            btnUp_app.Size = new Size(50, 0x17);
            btnUp_app.TabIndex = 0;
            btnUp_app.Click += btnUpDown_app_Click;
            btnDown_app.Enabled = false;
            btnDown_app.Location = new Point(0x3d, 0x10);
            btnDown_app.Size = new Size(50, 0x17);
            btnDown_app.TabIndex = 1;
            btnDown_app.Click += btnUpDown_app_Click;
            btnAddSep_app.Location = new Point(0x75, 0x10);
            btnAddSep_app.Size = new Size(120, 0x17);
            btnAddSep_app.TabIndex = 2;
            btnAddSep_app.Click += btnAddSep_app_Click;
            btnAddVFolder_app.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnAddVFolder_app.Location = new Point(0x182, 0x10);
            btnAddVFolder_app.Size = new Size(50, 0x18);
            btnAddVFolder_app.TabIndex = 3;
            btnAddVFolder_app.Text = "+";
            btnAddVFolder_app.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnAddVFolder_app.ImageAlign = ContentAlignment.TopLeft;
            btnAddVFolder_app.Click += btnAddVirtualFolder_app_Click;
            btnPlus_app.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnPlus_app.Location = new Point(0x1ba, 0x10);
            btnPlus_app.Size = new Size(0x19, 0x17);
            btnPlus_app.TabIndex = 4;
            btnPlus_app.Text = "+";
            btnPlus_app.Click += btnPlus_app_Click;
            btnMinus_app.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnMinus_app.Location = new Point(0x1d9, 0x10);
            btnMinus_app.Size = new Size(0x19, 0x17);
            btnMinus_app.TabIndex = 5;
            btnMinus_app.Text = "-";
            btnMinus_app.Click += btnMinus_app_Click;
            lblUserApps_Path.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            lblUserApps_Path.AutoSize = true;
            lblUserApps_Path.Location = new Point(6, 0x164);
            lblUserApps_Path.Size = new Size(0x21, 13);
            lblUserApps_Args.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            lblUserApps_Args.AutoSize = true;
            lblUserApps_Args.Location = new Point(6, 380);
            lblUserApps_Args.Size = new Size(0x3f, 13);
            lblUserApps_Working.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            lblUserApps_Working.AutoSize = true;
            lblUserApps_Working.Location = new Point(6, 0x194);
            lblUserApps_Working.Size = new Size(0x61, 13);
            lblUserApps_Key.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            lblUserApps_Key.AutoSize = true;
            lblUserApps_Key.Location = new Point(6, 0x1ac);
            lblUserApps_Key.Size = new Size(0x61, 13);
            tbPath.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            tbPath.Enabled = false;
            tbPath.Location = new Point(0x6b, 0x161);
            tbPath.Size = new Size(0x149, 0x15);
            tbPath.MaxLength = 260;
            tbPath.TabIndex = 7;
            tbPath.TextChanged += tbsUserApps_TextChanged;
            tbPath.KeyPress += textBoxesPath_KeyPress;
            tbArgs.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            tbArgs.Enabled = false;
            tbArgs.Location = new Point(0x6b, 0x179);
            tbArgs.Size = new Size(360, 0x15);
            tbArgs.MaxLength = 260;
            tbArgs.TabIndex = 10;
            tbArgs.TextChanged += tbsUserApps_TextChanged;
            tbWorking.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            tbWorking.Enabled = false;
            tbWorking.Location = new Point(0x6b, 0x191);
            tbWorking.Size = new Size(360, 0x15);
            tbWorking.MaxLength = 260;
            tbWorking.TabIndex = 12;
            tbWorking.TextChanged += tbsUserApps_TextChanged;
            tbWorking.KeyPress += textBoxesPath_KeyPress;
            chbUserAppKey.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            chbUserAppKey.AutoSize = true;
            chbUserAppKey.Enabled = false;
            chbUserAppKey.Location = new Point(0x6b, 420);
            chbUserAppKey.TabIndex = 14;
            chbUserAppKey.CheckedChanged += chbUserAppKey_CheckedChanged;
            tbUserAppKey.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            tbUserAppKey.Enabled = false;
            tbUserAppKey.Location = new Point(0x7f, 0x1a9);
            tbUserAppKey.Size = new Size(340, 0x15);
            tbUserAppKey.TextAlign = HorizontalAlignment.Center;
            tbUserAppKey.TabIndex = 15;
            tbUserAppKey.PreviewKeyDown += tbUserAppKey_PreviewKeyDown;
            tbUserAppKey.KeyPress += tbUserAppKey_KeyPress;
            btnOFD_app.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            btnOFD_app.Enabled = false;
            btnOFD_app.Location = new Point(0x1ba, 0x161);
            btnOFD_app.Size = new Size(0x19, 0x15);
            btnOFD_app.TabIndex = 8;
            btnOFD_app.Text = "...";
            btnOFD_app.UseVisualStyleBackColor = true;
            btnOFD_app.Click += btnOFD_app_Click;
            btnBFD_app.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            btnBFD_app.Enabled = false;
            btnBFD_app.Location = new Point(0x1d9, 0x161);
            btnBFD_app.Size = new Size(0x19, 0x15);
            btnBFD_app.TabIndex = 9;
            btnBFD_app.Text = ".";
            btnBFD_app.UseVisualStyleBackColor = true;
            btnBFD_app.Click += btnBFD_app_Click;
            btnAddToken_Arg.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            btnAddToken_Arg.Location = new Point(0x1d9, 0x179);
            btnAddToken_Arg.Enabled = false;
            btnAddToken_Arg.Size = new Size(0x19, 0x15);
            btnAddToken_Arg.TabIndex = 11;
            btnAddToken_Arg.Text = "%";
            btnAddToken_Arg.UseVisualStyleBackColor = true;
            btnAddToken_Arg.Click += btnAddToken_Arg_Click;
            btnAddToken_Wrk.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            btnAddToken_Wrk.Enabled = false;
            btnAddToken_Wrk.Location = new Point(0x1d9, 0x191);
            btnAddToken_Wrk.Size = new Size(0x19, 0x15);
            btnAddToken_Wrk.TabIndex = 13;
            btnAddToken_Wrk.Text = "%";
            btnAddToken_Wrk.UseVisualStyleBackColor = true;
            btnAddToken_Wrk.Click += btnAddToken_Wrk_Click;
            cmsAddToken.ShowImageMargin = false;
            cmsAddToken.ItemClicked += cmsAddToken_ItemClicked;
            tabPage9_Misc.Controls.Add(chbForceSysListView);
            tabPage9_Misc.Controls.Add(chbAlwaysShowHeader);
            tabPage9_Misc.Controls.Add(chbHideMenu);
            tabPage9_Misc.Controls.Add(chbBSUpOneLvl);
            tabPage9_Misc.Controls.Add(chbNoFulRowSelect);
            tabPage9_Misc.Controls.Add(chbGridLine);
            tabPage9_Misc.Controls.Add(chbAlternateColor);
            tabPage9_Misc.Controls.Add(chbShowPreview);
            tabPage9_Misc.Controls.Add(chbPreviewMode);
            tabPage9_Misc.Controls.Add(chbPreviewInfo);
            tabPage9_Misc.Controls.Add(chbSubDirTip);
            tabPage9_Misc.Controls.Add(chbSubDirTipMode);
            tabPage9_Misc.Controls.Add(chbSubDirTipModeHidden);
            tabPage9_Misc.Controls.Add(chbSubDirTipModeSystem);
            tabPage9_Misc.Controls.Add(chbSubDirTipModeFile);
            tabPage9_Misc.Controls.Add(chbSubDirTipPreview);
            tabPage9_Misc.Controls.Add(chbSelectWithoutExt);
            tabPage9_Misc.Controls.Add(chbF2Selection);
            tabPage9_Misc.Controls.Add(chbCursorLoop);
            tabPage9_Misc.Controls.Add(btnAlternateColor);
            tabPage9_Misc.Controls.Add(btnAlternateColor_Text);
            tabPage9_Misc.Controls.Add(btnAlternate_Default);
            tabPage9_Misc.Controls.Add(btnAddImgExt);
            tabPage9_Misc.Controls.Add(btnDelImgExt);
            tabPage9_Misc.Controls.Add(btnDefaultImgExt);
            tabPage9_Misc.Controls.Add(btnPreviewFont);
            tabPage9_Misc.Controls.Add(btnPreviewFontDefault);
            tabPage9_Misc.Controls.Add(cmbImgExts);
            tabPage9_Misc.Controls.Add(btnAddTextExt);
            tabPage9_Misc.Controls.Add(btnDelTextExt);
            tabPage9_Misc.Controls.Add(btnDefaultTextExt);
            tabPage9_Misc.Controls.Add(cmbTextExts);
            tabPage9_Misc.Controls.Add(btnPayPal);
            tabPage9_Misc.Controls.Add(nudPreviewMaxHeight);
            tabPage9_Misc.Controls.Add(nudPreviewMaxWidth);
            tabPage9_Misc.Controls.Add(lblPreviewHeight);
            tabPage9_Misc.Controls.Add(lblPreviewWidth);
            tabPage9_Misc.Location = new Point(4, 0x16);
            tabPage9_Misc.Padding = new Padding(3);
            tabPage9_Misc.Size = new Size(0x1ff, 0x1d7);
            tabPage9_Misc.TabIndex = 6;
            tabPage9_Misc.UseVisualStyleBackColor = true;
            btnPayPal.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            btnPayPal.BackgroundImage = Resources_String.paypalBtn;
            btnPayPal.BackgroundImageLayout = ImageLayout.Center;
            btnPayPal.Location = new Point(0x1a8, 0x17b);
            btnPayPal.Size = new Size(0x47, 0x4c);
            btnPayPal.Cursor = Cursors.Hand;
            btnPayPal.TabIndex = 0x20;
            btnPayPal.UseVisualStyleBackColor = false;
            btnPayPal.Click += btnPayPal_Click;
            chbCursorLoop.AutoSize = true;
            chbCursorLoop.Location = new Point(0x1b, 0x1e1);
            chbCursorLoop.TabIndex = 0x1f;
            chbF2Selection.AutoSize = true;
            chbF2Selection.Location = new Point(0x1b, 0x1c9);
            chbF2Selection.TabIndex = 30;
            chbSelectWithoutExt.AutoSize = true;
            chbSelectWithoutExt.Location = new Point(0x1b, 0x1b1);
            chbSelectWithoutExt.TabIndex = 0x1d;
            chbSubDirTipModeSystem.AutoSize = true;
            chbSubDirTipModeSystem.Location = new Point(0x2d, 0x193);
            chbSubDirTipModeSystem.TabIndex = 0x1c;
            chbSubDirTipModeFile.AutoSize = true;
            chbSubDirTipModeFile.Location = new Point(0xd7, 0x179);
            chbSubDirTipModeFile.TabIndex = 0x1b;
            chbSubDirTipModeHidden.AutoSize = true;
            chbSubDirTipModeHidden.Location = new Point(0x2d, 0x179);
            chbSubDirTipModeHidden.TabIndex = 0x1a;
            chbSubDirTipPreview.AutoSize = true;
            chbSubDirTipPreview.Location = new Point(0xd7, 0x15f);
            chbSubDirTipPreview.TabIndex = 0x19;
            chbSubDirTipMode.AutoSize = true;
            chbSubDirTipMode.Location = new Point(0x2d, 0x15f);
            chbSubDirTipMode.TabIndex = 0x18;
            chbSubDirTip.AutoSize = true;
            chbSubDirTip.Location = new Point(0x1b, 330);
            chbSubDirTip.TabIndex = 0x17;
            chbSubDirTip.CheckedChanged += chbSubDirTip_CheckedChanged;
            btnPreviewFontDefault.Location = new Point(0x191, 0x127);
            btnPreviewFontDefault.Size = new Size(100, 0x17);
            btnPreviewFontDefault.TabIndex = 0x16;
            btnPreviewFontDefault.Click += btnPreviewFont_Click;
            btnPreviewFont.Location = new Point(0x123, 0x127);
            btnPreviewFont.Size = new Size(100, 0x17);
            btnPreviewFont.TabIndex = 0x15;
            btnPreviewFont.Click += btnPreviewFont_Click;
            btnDefaultTextExt.Location = new Point(0x191, 0x10d);
            btnDefaultTextExt.Size = new Size(100, 0x17);
            btnDefaultTextExt.TabIndex = 20;
            btnDefaultTextExt.Click += btnDefaultTextExt_Click;
            btnDelTextExt.Location = new Point(0x123, 0x10d);
            btnDelTextExt.Size = new Size(100, 0x17);
            btnDelTextExt.TabIndex = 0x13;
            btnDelTextExt.Click += btnDelPreiviewExt_Click;
            btnAddTextExt.Location = new Point(0xb5, 0x10d);
            btnAddTextExt.Size = new Size(100, 0x17);
            btnAddTextExt.TabIndex = 0x12;
            btnAddTextExt.Click += btnAddPreviewExt_Click;
            cmbTextExts.Location = new Point(0x2d, 0x10d);
            cmbTextExts.Size = new Size(130, 0x17);
            cmbTextExts.TabIndex = 0x11;
            cmbTextExts.SelectedIndexChanged += comboBoxes_SelectedIndexChanged;
            cmbTextExts.KeyPress += comboBoxPreviewExts_KeyPress;
            btnDefaultImgExt.Location = new Point(0x191, 0xf3);
            btnDefaultImgExt.Size = new Size(100, 0x17);
            btnDefaultImgExt.TabIndex = 0x10;
            btnDefaultImgExt.Click += btnDefaultImgExt_Click;
            btnDelImgExt.Location = new Point(0x123, 0xf3);
            btnDelImgExt.Size = new Size(100, 0x17);
            btnDelImgExt.TabIndex = 15;
            btnDelImgExt.Click += btnDelPreiviewExt_Click;
            btnAddImgExt.Location = new Point(0xb5, 0xf3);
            btnAddImgExt.Size = new Size(100, 0x17);
            btnAddImgExt.TabIndex = 14;
            btnAddImgExt.Click += btnAddPreviewExt_Click;
            cmbImgExts.Location = new Point(0x2d, 0xf3);
            cmbImgExts.Size = new Size(130, 0x17);
            cmbImgExts.TabIndex = 13;
            cmbImgExts.SelectedIndexChanged += comboBoxes_SelectedIndexChanged;
            cmbImgExts.KeyPress += comboBoxPreviewExts_KeyPress;
            lblPreviewWidth.Location = new Point(0x129, 0xbf);
            lblPreviewWidth.Size = new Size(0x62, 0x15);
            lblPreviewWidth.TextAlign = ContentAlignment.MiddleRight;
            lblPreviewHeight.Location = new Point(0x129, 0xd9);
            lblPreviewHeight.Size = new Size(0x62, 0x15);
            lblPreviewHeight.TextAlign = ContentAlignment.MiddleRight;
            nudPreviewMaxWidth.Location = new Point(0x191, 0xbf);
            int[] numArray22 = new int[4];
            numArray22[0] = 0x780;
            nudPreviewMaxWidth.Maximum = new decimal(numArray22);
            int[] numArray23 = new int[4];
            numArray23[0] = 0x80;
            nudPreviewMaxWidth.Minimum = new decimal(numArray23);
            nudPreviewMaxWidth.Size = new Size(0x3e, 0x15);
            nudPreviewMaxWidth.TabIndex = 11;
            nudPreviewMaxWidth.TextAlign = HorizontalAlignment.Center;
            int[] numArray24 = new int[4];
            numArray24[0] = 0x200;
            nudPreviewMaxWidth.Value = new decimal(numArray24);
            nudPreviewMaxHeight.Location = new Point(0x191, 0xd9);
            int[] numArray25 = new int[4];
            numArray25[0] = 0x4b0;
            nudPreviewMaxHeight.Maximum = new decimal(numArray25);
            int[] numArray26 = new int[4];
            numArray26[0] = 0x60;
            nudPreviewMaxHeight.Minimum = new decimal(numArray26);
            nudPreviewMaxHeight.Size = new Size(0x3e, 0x15);
            nudPreviewMaxHeight.TabIndex = 12;
            nudPreviewMaxHeight.TextAlign = HorizontalAlignment.Center;
            int[] numArray27 = new int[4];
            numArray27[0] = 0x100;
            nudPreviewMaxHeight.Value = new decimal(numArray27);
            chbPreviewInfo.AutoSize = true;
            chbPreviewInfo.Location = new Point(0x2d, 0xd7);
            chbPreviewInfo.TabIndex = 10;
            chbPreviewMode.AutoSize = true;
            chbPreviewMode.Location = new Point(0x2d, 0xbf);
            chbPreviewMode.TabIndex = 9;
            chbShowPreview.AutoSize = true;
            chbShowPreview.Location = new Point(0x1b, 170);
            chbShowPreview.TabIndex = 8;
            chbShowPreview.CheckedChanged += chbShowPreviewTooltip_CheckedChanged;
            btnAlternate_Default.Enabled = false;
            btnAlternate_Default.Location = new Point(0x191, 0x87);
            btnAlternate_Default.Size = new Size(100, 0x17);
            btnAlternate_Default.TabIndex = 7;
            btnAlternate_Default.Click += btnAlternateColor_Click;
            btnAlternateColor_Text.Enabled = false;
            btnAlternateColor_Text.Location = new Point(0x123, 0x87);
            btnAlternateColor_Text.Size = new Size(100, 0x17);
            btnAlternateColor_Text.TabIndex = 6;
            btnAlternateColor_Text.Click += btnAlternateColor_Click;
            btnAlternateColor.Enabled = false;
            btnAlternateColor.Location = new Point(0xb5, 0x87);
            btnAlternateColor.Size = new Size(100, 0x17);
            btnAlternateColor.TabIndex = 5;
            btnAlternateColor.Click += btnAlternateColor_Click;
            chbAlternateColor.AutoSize = true;
            chbAlternateColor.Location = new Point(0x1b, 0x70);
            chbAlternateColor.TabIndex = 4;
            chbAlternateColor.CheckedChanged += chbAlternateColor_CheckedChanged;
            chbGridLine.AutoSize = true;
            chbGridLine.Location = new Point(0x1b, 0x58);
            chbGridLine.TabIndex = 3;
            chbNoFulRowSelect.AutoSize = true;
            chbNoFulRowSelect.Location = new Point(0x1b, 0x40);
            chbNoFulRowSelect.TabIndex = 2;
            chbBSUpOneLvl.AutoSize = true;
            chbBSUpOneLvl.Location = new Point(0x1b, 40);
            chbBSUpOneLvl.TabIndex = 1;
            chbHideMenu.AutoSize = true;
            chbHideMenu.Location = new Point(0x1b, 0x10);
            chbHideMenu.TabIndex = 0;
            chbForceSysListView.AutoSize = true;
            chbForceSysListView.Location = new Point(240, 16);
            chbForceSysListView.TabIndex = 0;
            chbAlwaysShowHeader.AutoSize = true;
            chbAlwaysShowHeader.Location = new Point(240, 40);
            chbAlwaysShowHeader.TabIndex = 0;
            tabPage7_Plug.Controls.Add(btnBrowsePlugin);
            tabPage7_Plug.Controls.Add(pluginView);
            tabPage7_Plug.Controls.Add(lblPluginLang);
            tabPage7_Plug.Controls.Add(textBoxPluginLang);
            tabPage7_Plug.Controls.Add(btnBrowsePluginLang);
            tabPage7_Plug.Location = new Point(4, 0x16);
            tabPage7_Plug.Padding = new Padding(3);
            tabPage7_Plug.Size = new Size(0x1ff, 0x1d7);
            tabPage7_Plug.TabIndex = 2;
            tabPage7_Plug.UseVisualStyleBackColor = true;
            btnBrowsePlugin.AutoSize = true;
            btnBrowsePlugin.Location = new Point(5, 0x10);
            btnBrowsePlugin.TabIndex = 0;
            btnBrowsePlugin.UseVisualStyleBackColor = true;
            btnBrowsePlugin.Click += btnBrowsePlugin_Click;
            pluginView.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            pluginView.ColumnCount = 1;
            pluginView.ColumnStyles.Add(new ColumnStyle());
            pluginView.Location = new Point(5, 0x2d);
            pluginView.Size = new Size(0x1ed, 0x156);
            pluginView.TabIndex = 1;
            lblPluginLang.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            lblPluginLang.AutoSize = true;
            lblPluginLang.Location = new Point(0x1b, 0x194);
            textBoxPluginLang.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            textBoxPluginLang.Location = new Point(0x2d, 0x1ac);
            textBoxPluginLang.Size = new Size(0x174, 0x15);
            textBoxPluginLang.MaxLength = 260;
            textBoxPluginLang.TabIndex = 2;
            textBoxPluginLang.KeyPress += textBoxesPath_KeyPress;
            btnBrowsePluginLang.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            btnBrowsePluginLang.Location = new Point(0x1a7, 0x1a9);
            btnBrowsePluginLang.Size = new Size(0x22, 0x19);
            btnBrowsePluginLang.TabIndex = 3;
            btnBrowsePluginLang.Text = "...";
            btnBrowsePluginLang.Click += btnBrowsePluginLang_Click;
            tabPage8_Keys.Controls.Add(btnCopyKeys);
            tabPage8_Keys.Controls.Add(listViewKeyboard);
            tabPage8_Keys.Location = new Point(4, 0x16);
            tabPage8_Keys.Padding = new Padding(3);
            tabPage8_Keys.Size = new Size(0x1ff, 0x1d7);
            tabPage8_Keys.TabIndex = 2;
            tabPage8_Keys.UseVisualStyleBackColor = true;
            btnCopyKeys.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            btnCopyKeys.Location = new Point(5, 0x10);
            btnCopyKeys.AutoSize = true;
            btnCopyKeys.TabIndex = 0;
            btnCopyKeys.UseVisualStyleBackColor = true;
            btnCopyKeys.Click += btnCopyKeys_Click;
            listViewKeyboard.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            listViewKeyboard.CheckBoxes = true;
            listViewKeyboard.Columns.AddRange(new ColumnHeader[] { clmKeys_Action, clmKeys_Key });
            listViewKeyboard.FullRowSelect = true;
            listViewKeyboard.GridLines = true;
            listViewKeyboard.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            listViewKeyboard.Location = new Point(5, 0x2d);
            listViewKeyboard.MultiSelect = false;
            listViewKeyboard.Size = new Size(0x1ed, 0x191);
            listViewKeyboard.ShowItemToolTips = true;
            listViewKeyboard.TabIndex = 1;
            listViewKeyboard.UseCompatibleStateImageBehavior = false;
            listViewKeyboard.View = View.Details;
            listViewKeyboard.PreviewKeyDown += listViewKeyboard_PreviewKeyDown;
            listViewKeyboard.KeyPress += listViewKeyboard_KeyPress;
            clmKeys_Action.Text = "Action";
            clmKeys_Action.Width = 0x15c;
            clmKeys_Key.Text = "Key";
            clmKeys_Key.TextAlign = HorizontalAlignment.Center;
            clmKeys_Key.Width = 120;
            tabPageA_Path.Controls.Add(listView_NoCapture);
            tabPageA_Path.Controls.Add(btnOFD_NoCapture);
            tabPageA_Path.Controls.Add(btnAdd_NoCapture);
            tabPageA_Path.Controls.Add(btnRemove_NoCapture);
            tabPageA_Path.Controls.Add(cmbSpclFol_NoCapture);
            tabPageA_Path.Controls.Add(btnAddSpcFol_NoCapture);
            tabPageA_Path.Location = new Point(4, 0x16);
            tabPageA_Path.Padding = new Padding(3);
            tabPageA_Path.Size = new Size(0x1ff, 0x1d7);
            tabPageA_Path.TabIndex = 2;
            tabPageA_Path.UseVisualStyleBackColor = true;
            listView_NoCapture.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            listView_NoCapture.Columns.AddRange(new ColumnHeader[] { clmnHeader_NoCapture });
            listView_NoCapture.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            listView_NoCapture.HideSelection = false;
            listView_NoCapture.LabelEdit = true;
            listView_NoCapture.Location = new Point(5, 0x2d);
            listView_NoCapture.Size = new Size(0x1ed, 0xa3);
            listView_NoCapture.FullRowSelect = true;
            listView_NoCapture.TabIndex = 8;
            listView_NoCapture.UseCompatibleStateImageBehavior = false;
            listView_NoCapture.View = View.Details;
            listView_NoCapture.ItemActivate += listView_NoCapture_ItemActivate;
            listView_NoCapture.SelectedIndexChanged += listView_NoCapture_SelectedIndexChanged;
            listView_NoCapture.KeyDown += listView_NoCapture_KeyDown;
            listView_NoCapture.BeforeLabelEdit += listView_NoCapture_BeforeLabelEdit;
            listView_NoCapture.AfterLabelEdit += listView_NoCapture_AfterLabelEdit;
            clmnHeader_NoCapture.Width = 0x1d8;
            btnOFD_NoCapture.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnOFD_NoCapture.Location = new Point(0x19b, 0x10);
            btnOFD_NoCapture.Size = new Size(0x19, 0x17);
            btnOFD_NoCapture.TabIndex = 0;
            btnOFD_NoCapture.Text = "...";
            btnOFD_NoCapture.UseVisualStyleBackColor = true;
            btnOFD_NoCapture.Click += btnOFD_NoCapture_Click;
            btnAdd_NoCapture.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnAdd_NoCapture.Location = new Point(0x1ba, 0x10);
            btnAdd_NoCapture.Size = new Size(0x19, 0x17);
            btnAdd_NoCapture.TabIndex = 1;
            btnAdd_NoCapture.Text = "+";
            btnAdd_NoCapture.UseVisualStyleBackColor = true;
            btnAdd_NoCapture.Click += btnAdd_NoCapture_Click;
            btnRemove_NoCapture.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnRemove_NoCapture.Enabled = false;
            btnRemove_NoCapture.Location = new Point(0x1d9, 0x10);
            btnRemove_NoCapture.Size = new Size(0x19, 0x17);
            btnRemove_NoCapture.TabIndex = 2;
            btnRemove_NoCapture.Text = "-";
            btnRemove_NoCapture.UseVisualStyleBackColor = true;
            btnRemove_NoCapture.Click += btnRemove_NoCapture_Click;
            cmbSpclFol_NoCapture.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            cmbSpclFol_NoCapture.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSpclFol_NoCapture.Location = new Point(5, 0xd4);
            cmbSpclFol_NoCapture.Size = new Size(150, 0x15);
            cmbSpclFol_NoCapture.TabIndex = 3;
            btnAddSpcFol_NoCapture.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            btnAddSpcFol_NoCapture.Location = new Point(0x9e, 0xd3);
            btnAddSpcFol_NoCapture.Size = new Size(0x19, 0x17);
            btnAddSpcFol_NoCapture.Text = "+";
            btnAddSpcFol_NoCapture.TabIndex = 4;
            btnAddSpcFol_NoCapture.Click += btnAddSpcFol_NoCapture_Click;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(0x211, 0x269);
            MinimumSize = new Size(0x221, 0x28e);
            Controls.Add(tabControl1);
            Controls.Add(lblVer);
            Controls.Add(btnOK);
            Controls.Add(btnCancel);
            Controls.Add(btnApply);
            MaximizeBox = false;
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "QTTabBar Options";
            FormClosing += OptionsDialog_FormClosing;
            tabControl1.ResumeLayout(false);
            tabPage1_Gnrl.ResumeLayout(false);
            tabPage1_Gnrl.PerformLayout();
            tabPage2_Tabs.ResumeLayout(false);
            tabPage2_Tabs.PerformLayout();
            tabPage3_Wndw.ResumeLayout(false);
            tabPage3_Wndw.PerformLayout();
            tabPage4_View.ResumeLayout(false);
            tabPage4_View.PerformLayout();
            tabPage5_Grps.ResumeLayout(false);
            tabPage5_Grps.PerformLayout();
            tabPage6_Apps.ResumeLayout(false);
            tabPage6_Apps.PerformLayout();
            tabPage7_Plug.ResumeLayout(false);
            tabPage7_Plug.PerformLayout();
            tabPage8_Keys.ResumeLayout(false);
            tabPage8_Keys.PerformLayout();
            tabPage9_Misc.ResumeLayout(false);
            tabPage9_Misc.PerformLayout();
            tabPageA_Path.ResumeLayout(false);
            tabPageA_Path.PerformLayout();
            nudMaxRecentFile.EndInit();
            nudMaxUndo.EndInit();
            nudNetworkTimeOut.EndInit();
            nudTabWidthMin.EndInit();
            nudTabWidthMax.EndInit();
            nudTabHeight.EndInit();
            nudTabWidth.EndInit();
            nudPreviewMaxHeight.EndInit();
            nudPreviewMaxWidth.EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private void InitializePluginView() {
            pluginView.SuspendLayout();
            CreatePluginViewItem(PluginManager.PluginAssemblies.ToArray(), false);
            pluginView.ResumeLayout();
            pluginView.PluginRemoved += pluginView_PluginRemoved;
            pluginView.PluginOptionRequired += pluginView_PluginOptionRequired;
            pluginView.PluginAboutRequired += pluginView_PluginAboutRequired;
            pluginView.QueryPluginInfoHasOption += pluginView_QueryPluginInfoHasOption;
            pluginView.DragDropEx += pluginView_DragDropEx;
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
            tnGroupsRoot = new TreeNode(ResOpt_Genre[4]);
            tnGroupsRoot.ImageKey = tnGroupsRoot.SelectedImageKey = "groupRoot";
            treeViewGroup.BeginUpdate();
            foreach(string str in QTUtility.GroupPathsDic.Keys) {
                string str2 = QTUtility.GroupPathsDic[str];
                if(str2.Length > 0) {
                    TreeNode node = new TreeNode(str);
                    if(QTUtility.StartUpGroupList.Contains(str)) {
                        node.NodeFont = fntStartUpGroup;
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
                        tnGroupsRoot.Nodes.Add(node);
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
                tnGroupsRoot.Nodes.Add(node3);
            }
            treeViewGroup.Nodes.Add(tnGroupsRoot);
            treeViewGroup.SelectedNode = tnGroupsRoot;
            tnGroupsRoot.Expand();
            treeViewGroup.EndUpdate();
        }

        private void InitializeTreeView_UserApps() {
            btnAddVFolder_app.Image = QTUtility.ImageListGlobal.Images["folder"];
            tnRoot_UserApps = new TreeNode(ResOpt_Genre[5]);
            tnRoot_UserApps.ImageKey = tnRoot_UserApps.SelectedImageKey = "userAppsRoot";
            treeViewUserApps.BeginUpdate();
            foreach(string str in QTUtility.UserAppsDic.Keys) {
                string[] appVals = QTUtility.UserAppsDic[str];
                if(appVals != null) {
                    if((appVals.Length == 3) || (appVals.Length == 4)) {
                        tnRoot_UserApps.Nodes.Add(CreateUserAppNode(str, appVals));
                    }
                    continue;
                }
                using(RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Quizo\QTTabBar\UserApps\" + str)) {
                    TreeNode node;
                    if((key != null) && CreateUserAppNode_Sub(str, key, out node)) {
                        tnRoot_UserApps.Nodes.Add(node);
                    }
                    continue;
                }
            }
            treeViewUserApps.Nodes.Add(tnRoot_UserApps);
            tnRoot_UserApps.Expand();
            treeViewUserApps.EndUpdate();
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
            fNowListViewItemEditing = false;
            ListViewItem item = listView_NoCapture.Items[e.Item];
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
                    index = Array.IndexOf(arrSpecialFolderDipNms, e.Label);
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
            fNowListViewItemEditing = true;
        }

        private void listView_NoCapture_ItemActivate(object sender, EventArgs e) {
            if(listView_NoCapture.SelectedItems.Count > 0) {
                ListViewItem item = listView_NoCapture.SelectedItems[0];
                listView_NoCapture.SelectedItems.Clear();
                item.BeginEdit();
            }
        }

        private void listView_NoCapture_KeyDown(object sender, KeyEventArgs e) {
            if(listView_NoCapture.SelectedItems.Count > 0) {
                Keys keyCode = e.KeyCode;
                if(keyCode != Keys.Delete) {
                    if(keyCode != Keys.F2) {
                        return;
                    }
                }
                else {
                    foreach(ListViewItem item in listView_NoCapture.SelectedItems) {
                        listView_NoCapture.Items.Remove(item);
                    }
                    return;
                }
                ListViewItem item2 = listView_NoCapture.SelectedItems[0];
                listView_NoCapture.SelectedItems.Clear();
                item2.BeginEdit();
            }
        }

        private void listView_NoCapture_SelectedIndexChanged(object sender, EventArgs e) {
            btnRemove_NoCapture.Enabled = listView_NoCapture.SelectedItems.Count > 0;
        }

        private void listViewKeyboard_KeyPress(object sender, KeyPressEventArgs e) {
            e.Handled = true;
        }

        private void listViewKeyboard_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) {
            if(!IsInvalidShortcutKey(e.KeyCode, e.Modifiers) && (listViewKeyboard.SelectedItems.Count == 1)) {
                ListViewItem lviCurrent = listViewKeyboard.SelectedItems[0];
                if((lviCurrent.Tag != null) && (((Keys)lviCurrent.Tag) != e.KeyData)) {
                    if(e.KeyCode == Keys.Escape) {
                        lviCurrent.SubItems[1].Text = " - ";
                        lviCurrent.Tag = Keys.None;
                    }
                    else if(((e.Modifiers != Keys.None) || ((Keys.F1 <= e.KeyCode) && (e.KeyCode <= Keys.F24))) && ((CheckExistance_Shortcuts(e.KeyData, lviCurrent) && CheckExistance_UserAppKey(e.KeyData, tnRoot_UserApps, null)) && CheckExistance_GroupKey(e.KeyData, null))) {
                        lviCurrent.SubItems[1].Text = QTUtility2.MakeKeyString(e.KeyData);
                        lviCurrent.Tag = e.KeyData;
                    }
                    e.IsInputKey = false;
                }
            }
        }

        private void numericUpDownMax_ValueChanged(object sender, EventArgs e) {
            if(sender == nudTabWidthMax) {
                if(nudTabWidthMax.Value < nudTabWidthMin.Value) {
                    nudTabWidthMax.Value = nudTabWidthMin.Value;
                }
            }
            else if(nudTabWidthMin.Value > nudTabWidthMax.Value) {
                nudTabWidthMin.Value = nudTabWidthMax.Value;
            }
        }

        protected override void OnResize(EventArgs e) {
            base.OnResize(e);
            TabPage selectedTab = tabControl1.SelectedTab;
            if(selectedTab != null) {
                selectedTab.Invalidate();
            }
        }

        private void OptionsDialog_FormClosing(object sender, FormClosingEventArgs e) {
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar")) {
                if(key != null) {
                    Rectangle bounds = Bounds;
                    if(WindowState == FormWindowState.Minimized) {
                        bounds = RestoreBounds;
                    }
                    int[] array = new int[] { bounds.Left, bounds.Top, bounds.Width, bounds.Height };
                    QTUtility2.WriteRegBinary(array, "OptionWindowBounds", key);
                }
            }
            if(callBack != null) {
                callBack.BeginInvoke(DialogResult.Cancel, null, null);
                callBack = null;
            }
        }

        public void OwnerWindowClosing() {
            callBack = null;
            Close();
        }

        private void pluginView_DragDropEx(object sender, EventArgs e) {
            bool flag = true;
            foreach(PluginAssembly assembly in pluginView.DroppedFiles
                    .Where(str => !string.IsNullOrEmpty(str))
                    .Select(str => new PluginAssembly(str))
                    .Where(assembly => assembly.PluginInfosExist)) {
                CreatePluginViewItem(new PluginAssembly[] { assembly }, true);
                if(flag) {
                    flag = false;
                    SelectPluginBottom();
                }
            }
        }

        private void pluginView_PluginAboutRequired(object sender, PluginOptionEventArgs e) {
            PluginInformation pluginInfo = e.PluginViewItem.PluginInfo;
            string strMessage = pluginInfo.Name + Environment.NewLine + pluginInfo.Version + Environment.NewLine + pluginInfo.Author + Environment.NewLine + Environment.NewLine + "\"" + pluginInfo.Path + "\"";
            MessageForm.Show(Handle, strMessage, string.Format(PluginView.MNU_PLUGINABOUT, pluginInfo.Name), MessageBoxIcon.Asterisk, 0xea60);
        }

        private void pluginView_PluginOptionRequired(object sender, PluginOptionEventArgs e) {
            Plugin plugin;
            if((pluginManager.TryGetPlugin(e.PluginViewItem.PluginInfo.PluginID, out plugin) && (plugin != null)) && (plugin.Instance != null)) {
                try {
                    plugin.Instance.OnOption();
                }
                catch(Exception exception) {
                    PluginManager.HandlePluginException(exception, Handle, plugin.PluginInformation.Name, "Open plugin option.");
                    QTUtility2.MakeErrorLog(exception, "Error at Plugin: " + e.PluginViewItem.PluginInfo.Name, true);
                }
            }
        }

        private void pluginView_PluginRemoved(object sender, PluginOptionEventArgs e) {
            PluginAssembly pluingAssembly = e.PluginViewItem.PluingAssembly;
            if(pluingAssembly.PluginInformations.Count > 1) {
                string str = pluingAssembly.PluginInformations.Select(info => info.Name).StringJoin(", ");
                if(DialogResult.OK != MessageBox.Show(string.Format(RES_REMOVEPLGIN, str), string.Empty, MessageBoxButtons.OKCancel, MessageBoxIcon.Question)) {
                    e.Cancel = true;
                    return;
                }
                pluginView.RemovePluginsRange(pluingAssembly.PluginInformations.ToArray());
            }
            DeletePluginAssembly(pluingAssembly);
        }

        private bool pluginView_QueryPluginInfoHasOption(PluginInformation pi) {
            Plugin plugin;
            if(pluginManager.TryGetPlugin(pi.PluginID, out plugin)) {
                try {
                    return plugin.Instance.HasOption;
                }
                catch(Exception exception) {
                    PluginManager.HandlePluginException(exception, Handle, pi.Name, "Checking if the plugin has option.");
                    QTUtility2.MakeErrorLog(exception, "Error at Plugin: " + pi.Name, true);
                }
            }
            return false;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            if((tabControl1.SelectedIndex == 7) && listViewKeyboard.Focused) {
                bool flag = (keyData & Keys.Control) == Keys.Control;
                bool flag2 = (keyData & Keys.Shift) == Keys.Shift;
                bool flag3 = (keyData & Keys.Alt) == Keys.Alt;
                if(flag3) {
                    return true;
                }
                if((keyData == (Keys.Control | Keys.Tab)) || (keyData == (Keys.Control | Keys.Shift | Keys.Tab))) {
                    return true;
                }
                if(flag || flag2) {
                    if((((keyData & Keys.Up) == Keys.Up) || ((keyData & Keys.Down) == Keys.Down)) || (((keyData & Keys.Left) == Keys.Left) || ((keyData & Keys.Right) == Keys.Right))) {
                        return true;
                    }
                    if((((keyData & Keys.Home) == Keys.Home) || ((keyData & Keys.End) == Keys.End)) || (((keyData & Keys.Next) == Keys.Next) || ((keyData & Keys.Prior) == Keys.Prior))) {
                        return true;
                    }
                }
            }
            if((ActiveControl == tbUserAppKey) || (ActiveControl == tbGroupKey)) {
                return ((keyData != Keys.Tab) && (keyData != (Keys.Shift | Keys.Tab)));
            }
            if((keyData == Keys.Return) && (ActiveControl is Button)) {
                ((Button)ActiveControl).PerformClick();
                return true;
            }
            if((keyData != Keys.Escape) && (keyData != Keys.Return)) {
                return base.ProcessCmdKey(ref msg, keyData);
            }
            switch(tabControl1.SelectedIndex) {
                case 4:
                    if(!fNowTreeNodeEditing) {
                        break;
                    }
                    return base.ProcessCmdKey(ref msg, keyData);

                case 5:
                    if(!fNowTreeNodeEditing) {
                        if(keyData != Keys.Return) {
                            break;
                        }
                        if(ActiveControl == tbPath) {
                            tbArgs.Focus();
                            return true;
                        }
                        if(ActiveControl == tbArgs) {
                            tbWorking.Focus();
                            return true;
                        }
                        if(ActiveControl == tbWorking) {
                            tbPath.Focus();
                            return true;
                        }
                        if(ActiveControl != treeViewUserApps) {
                            break;
                        }
                        if(tbPath.Enabled) {
                            tbPath.Focus();
                        }
                        return true;
                    }
                    return base.ProcessCmdKey(ref msg, keyData);

                case 7:
                    if(!listViewKeyboard.Focused) {
                        break;
                    }
                    return true;

                case 8:
                    if((ActiveControl != cmbTextExts) && (ActiveControl != cmbImgExts)) {
                        break;
                    }
                    return false;

                case 9:
                    if(!fNowListViewItemEditing) {
                        if((keyData == Keys.Return) && listView_NoCapture.Focused) {
                            return base.ProcessCmdKey(ref msg, keyData);
                        }
                        break;
                    }
                    return base.ProcessCmdKey(ref msg, keyData);
            }
            if(keyData == Keys.Escape) {
                Close();
            }
            else {
                Save(false);
            }
            return true;
        }

        public static void RefreshTexts() {
            ResOpt = QTUtility.TextResourcesDic["TabBar_Option"];
            ResOpt_DropDown = QTUtility.TextResourcesDic["TabBar_Option_DropDown"];
            ResOpt_Genre = QTUtility.TextResourcesDic["TabBar_Option_Genre"];
        }

        private void RemovePluginShortcutKeys(string pluginID) {
            listViewKeyboard.BeginUpdate();
            ListViewGroup group = listViewKeyboard.Groups[pluginID];
            if(group != null) {
                foreach(ListViewItem item2 in group.Items) {
                    listViewKeyboard.Items.Remove(item2);
                }
                listViewKeyboard.Groups.Remove(group);
            }
            listViewKeyboard.EndUpdate();
        }

        private void Save(bool fApply) {
            SaveSettings(fApply);
            if(callBack != null) {
                callBack.BeginInvoke(fApply ? DialogResult.Yes : DialogResult.OK, null, null);
            }
            if(!fApply) {
                callBack = null;
                Close();
            }
        }

        private void SaveGroupTreeView(RegistryKey rkUser) {
            QTUtility.GroupPathsDic.Clear();
            QTUtility.StartUpGroupList.Clear();
            QTUtility.dicGroupShortcutKeys.Clear();
            QTUtility.dicGroupNamesAndKeys.Clear();
            List<PluginKey> list = new List<PluginKey>();
            int num = 1;
            foreach(TreeNode node in tnGroupsRoot.Nodes) {
                MenuItemArguments tag = (MenuItemArguments)node.Tag;
                if(tag == MIA_GROUPSEP) {
                    QTUtility.GroupPathsDic["Separator" + num++] = string.Empty;
                }
                else if(node.Nodes.Count > 0) {
                    string text = node.Text;
                    if(text.Length > 0) {
                        string str2 = node.Nodes.Cast<TreeNode>()
                                .Select(node2 => node2.Name)
                                .Where(name => name.Length > 0)
                                .StringJoin(";");
                        if(str2.Length > 0) {
                            string item = text.Replace(";", "_");
                            QTUtility.GroupPathsDic[item] = str2;
                            if(node.NodeFont == fntStartUpGroup) {
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
                QTUtility2.WriteRegBinary(list.ToArray(), "ShortcutKeys_Group", rkUser);
            }
        }

        private void SaveNoCapturePaths(RegistryKey rkUser) {
            string str = string.Empty;
            QTUtility.NoCapturePathsList.Clear();
            foreach(ListViewItem item in listView_NoCapture.Items) {
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
            lstPluginAssembliesUserAdded.Clear();
            if(File.Exists(textBoxPluginLang.Text)) {
                QTUtility.Path_PluginLangFile = textBoxPluginLang.Text;
            }
            else {
                QTUtility.Path_PluginLangFile = string.Empty;
            }
            List<PluginAssembly> list = new List<PluginAssembly>();
            foreach(PluginViewItem item in pluginView.PluginViewItems) {
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
                    RemovePluginShortcutKeys(pluginInfo.PluginID);
                }
            }
            if(callBack != null) {
                callBack(list);
            }
            if(fApply) {
                pluginView.NotifyApplied();
                string[] strArray = QTUtility.TextResourcesDic["ShortcutKeys_Groups"];
                foreach(Plugin plugin in pluginManager.Plugins) {
                    PluginInformation pluginInformation = plugin.PluginInformation;
                    if((((pluginInformation != null) && pluginInformation.Enabled) && ((pluginInformation.PluginType == PluginType.Background) || (pluginInformation.PluginType == PluginType.BackgroundMultiple))) && (listViewKeyboard.Groups[pluginInformation.PluginID] == null)) {
                        string[] shortcutKeyActions = pluginInformation.ShortcutKeyActions;
                        if((shortcutKeyActions != null) && (shortcutKeyActions.Length > 0)) {
                            ListViewGroup group2 = listViewKeyboard.Groups.Add(pluginInformation.PluginID, pluginInformation.Name + " (" + strArray[1] + ")");
                            foreach(string action in shortcutKeyActions) {
                                listViewKeyboard.Items.Add(new ListViewItem(new string[] { action, " - " }) {
                                    Checked = false,
                                    Group = group2,
                                    Tag = 0,
                                    Name = pluginInformation.PluginID
                                });
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
            if(chbActivateNew.Checked) {
                QTUtility.SetConfigAt(Settings.ActivateNewTab, true);
            }
            if(chbDontOpenSame.Checked) {
                QTUtility.SetConfigAt(Settings.DontOpenSame, true);
            }
            if(chbCloseWhenGroup.Checked) {
                QTUtility.SetConfigAt(Settings.CloseWhenGroup, true);
            }
            if(chbRestoreClosed.Checked) {
                QTUtility.SetConfigAt(Settings.RestoreTabs, true);
            }
            if(chbShowTooltip.Checked) {
                QTUtility.SetConfigAt(Settings.ShowTooltips, true);
            }
            if(chbNeverCloseWindow.Checked) {
                QTUtility.SetConfigAt(Settings.NeverCloseWindow, true);
            }
            if(chbNeverCloseWndLocked.Checked) {
                QTUtility.SetConfigAt(Settings.NeverCloseWndLocked, true);
            }
            QTUtility.ConfigValues[1] = (byte)cmbNewTabLoc.SelectedIndex;
            QTUtility.ConfigValues[2] = (byte)cmbActvClose.SelectedIndex;
            QTUtility.ConfigValues[3] = (byte)cmbTabDblClck.SelectedIndex;
            QTUtility.ConfigValues[4] = (byte)cmbBGDblClick.SelectedIndex;
            QTUtility.ConfigValues[12] = (byte)cmbTabWhlClck.SelectedIndex;
            QTUtility.Action_BarDblClick = textBoxAction_BarDblClck.Text;
            if(cmbTabSizeMode.SelectedIndex == 1) {
                QTUtility.SetConfigAt(Settings.FixedWidthTabs, true);
            }
            else if(cmbTabSizeMode.SelectedIndex == 2) {
                QTUtility.SetConfigAt(Settings.LimitedWidthTabs, true);
            }
            QTUtility.TabWidth = (int)nudTabWidth.Value;
            QTUtility.TabHeight = (int)nudTabHeight.Value;
            QTUtility.MaxTabWidth = (int)nudTabWidthMax.Value;
            QTUtility.MinTabWidth = (int)nudTabWidthMin.Value;
            if(chbNavBtn.Checked) {
                QTUtility.SetConfigAt(Settings.ShowNavButtons, true);
            }
            if(cmbNavBtn.SelectedIndex == 1) {
                QTUtility.SetConfigAt(Settings.NavButtonsOnRight, true);
            }
            if(chbX1X2.Checked) {
                QTUtility.SetConfigAt(Settings.CaptureX1X2, true);
            }
            if(chbBoldActv.Checked) {
                QTUtility.SetConfigAt(Settings.ActiveTabInBold, true);
            }
            if(chbUseTabSkin.Checked) {
                QTUtility.SetConfigAt(Settings.UseTabSkin, true);
            }
            if(!chbNoHistory.Checked) {
                QTUtility.SetConfigAt(Settings.NoHistory, true);
            }
            if(!chbWhlClick.Checked) {
                QTUtility.SetConfigAt(Settings.NoCaptureMidClick, true);
            }
            if(cmbWhlClick.SelectedIndex == 1) {
                QTUtility.SetConfigAt(Settings.MidClickNewWindow, true);
            }
            if(!chbNCADblClck.Checked) {
                QTUtility.SetConfigAt(Settings.NoDblClickUpLevel, true);
            }
            if(chbSaveExecuted.CheckState != CheckState.Unchecked) {
                if(chbSaveExecuted.CheckState == CheckState.Checked) {
                    QTUtility.SetConfigAt(Settings.AllRecentFiles, true);
                }
            }
            else {
                QTUtility.SetConfigAt(Settings.NoRecentFiles, true);
            }
            if(!chbBlockProcess.Checked) {
                QTUtility.SetConfigAt(Settings.DontCaptureNewWnds, true);
            }
            if(chbRestoreLocked.Checked) {
                QTUtility.SetConfigAt(Settings.RestoreLockedTabs, true);
            }
            if(!chbFoldrTree.Checked) {
                QTUtility.SetConfigAt(Settings.NoNewWndFolderTree, true);
            }
            if(chbWndUnresizable.Checked) {
                QTUtility.SetConfigAt(Settings.NoWindowResizing, true);
            }
            if(flag3) {
                QTUtility.SetConfigAt(Settings.ShowHashResult, true);
            }
            QTUtility.TabTextColor_Active = btnActTxtClr.ForeColor;
            QTUtility.TabTextColor_Inactv = btnInactTxtClr.ForeColor;
            QTUtility.TabHiliteColor = btnHiliteClsc.ForeColor;
            QTUtility.RebarBGColor = btnToolBarBGClr.BackColor;
            QTUtility.TabTextColor_ActivShdw = btnShadowAct.ForeColor;
            QTUtility.TabTextColor_InAtvShdw = btnShadowIna.ForeColor;
            if(flag4) {
                QTUtility.SetConfigAt(Settings.HashTopMost, true);
            }
            if(cmbMultiRow.SelectedIndex == 1) {
                QTUtility.SetConfigAt(Settings.MultipleRow1, true);
            }
            else if(cmbMultiRow.SelectedIndex == 2) {
                QTUtility.SetConfigAt(Settings.MultipleRow2, true);
            }
            if(chbHideMenu.Checked) {
                QTUtility.SetConfigAt(Settings.HideMenuBar, true);
            }
            if(chbToolbarBGClr.Checked) {
                QTUtility.SetConfigAt(Settings.ToolbarBGColor, true);
            }
            if(chbFolderIcon.Checked) {
                QTUtility.SetConfigAt(Settings.FolderIcon, true);
            }
            if(chbBSUpOneLvl.Checked) {
                QTUtility.SetConfigAt(Settings.BackspaceUpLevel, true);
            }
            if(chbForceSysListView.Checked) {
                QTUtility.SetConfigAt(Settings.ForceSysListView, true);
            }
            if(chbAlwaysShowHeader.Checked) {
                QTUtility.SetConfigAt(Settings.AlwaysShowHeaders, true);
            }
            QTUtility.Path_TabImage = tbTabImagePath.Text;
            QTUtility.TabImageSizingMargin = tabImageSetting.SizingMargin;
            if(chbGridLine.Checked) {
                QTUtility.SetConfigAt(Settings.DetailsGridLines, true);
            }
            if(chbNoFulRowSelect.Checked ^ !QTUtility.IsXP) {
                QTUtility.SetConfigAt(Settings.ToggleFullRowSelect, true);
            }
            if(QTUtility.IsXP && !chbSelectWithoutExt.Checked) {
                QTUtility.SetConfigAt(Settings.ExtWhileRenaming, true);
            }
            if(flag) {
                QTUtility.SetConfigAt(Settings.HashFullPath, true);
            }
            if(chbAlternateColor.Checked) {
                QTUtility.SetConfigAt(Settings.AlternateRowColors, true);
            }
            QTUtility.ShellViewRowCOLORREF_Background = QTUtility2.MakeCOLORREF(btnAlternateColor.BackColor);
            QTUtility.ShellViewRowCOLORREF_Text = QTUtility2.MakeCOLORREF(btnAlternateColor_Text.ForeColor);
            if(chbWndRestrAlpha.Checked) {
                QTUtility.SetConfigAt(Settings.SaveTransparency, true);
            }
            if(chbShowPreview.Checked) {
                QTUtility.SetConfigAt(Settings.ShowTooltipPreviews, true);
            }
            if(chbPreviewMode.Checked) {
                QTUtility.SetConfigAt(Settings.PreviewsWithShift, true);
            }
            if(!chbSubDirTip.Checked) {
                QTUtility.SetConfigAt(Settings.NoShowSubDirTips, true);
            }
            if(chbSubDirTipMode.Checked) {
                QTUtility.SetConfigAt(Settings.SubDirTipsWithShift, true);
            }
            if(chbSubDirTipModeHidden.Checked) {
                QTUtility.SetConfigAt(Settings.SubDirTipsHidden, true);
            }
            if(chbSubDirTipModeFile.Checked) {
                QTUtility.SetConfigAt(Settings.SubDirTipsFiles, true);
            }
            if(!chbSubDirTipPreview.Checked) {
                QTUtility.SetConfigAt(Settings.SubDirTipsPreview, true);
            }
            if(flag2) {
                QTUtility.SetConfigAt(Settings.HashClearOnClose, true);
            }
            if(chbDD.Checked) {
                QTUtility.SetConfigAt(Settings.DragDropOntoTabs, true);
            }
            if(chbNoTabFromOuteside.Checked) {
                QTUtility.SetConfigAt(Settings.NoTabsFromOutside, true);
            }
            if(!chbAutoSubText.Checked) {
                QTUtility.SetConfigAt(Settings.NoRenameAmbTabs, true);
            }
            if(!chbHolizontalScroll.Checked) {
                QTUtility.SetConfigAt(Settings.HorizontalScroll, true);
            }
            if(!chbWhlChangeView.Checked) {
                QTUtility.SetConfigAt(Settings.CtrlWheelChangeView, true);
            }
            if(chbSubDirTipModeSystem.Checked) {
                QTUtility.SetConfigAt(Settings.SubDirTipsSystem, true);
            }
            if(chbSendToTray.Checked) {
                QTUtility.SetConfigAt(Settings.TrayOnClose, true);
            }
            if(!chbF2Selection.Checked) {
                QTUtility.SetConfigAt(Settings.F2Selection, true);
            }
            if(chbRebarBGImage.Checked) {
                QTUtility.SetConfigAt(Settings.RebarImage, true);
            }
            switch(cmbRebarBGImageMode.SelectedIndex) {
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
            QTUtility.Path_RebarImage = tbRebarImagePath.Text;
            if(chbTabCloseButton.Checked) {
                QTUtility.SetConfigAt(Settings.ShowTabCloseButtons, true);
            }
            if(chbSubDirTipOnTab.Checked) {
                QTUtility.SetConfigAt(Settings.ShowSubDirTipOnTab, true);
            }
            if(chbTabCloseBtnAlt.Checked) {
                QTUtility.SetConfigAt(Settings.TabCloseBtnsWithAlt, true);
            }
            if(chbCursorLoop.Checked) {
                QTUtility.SetConfigAt(Settings.CursorLoop, true);
            }
            if(chbTabCloseBtnHover.Checked) {
                QTUtility.SetConfigAt(Settings.TabCloseBtnsOnHover, true);
            }
            if(chbSendToTrayOnMinimize.Checked) {
                QTUtility.SetConfigAt(Settings.TrayOnMinimize, true);
            }
            if(cmbTabTextAlignment.SelectedIndex == 1) {
                QTUtility.SetConfigAt(Settings.AlignTabTextCenter, true);
            }
            if(cmbMenuRenderer.SelectedIndex != 0) {
                QTUtility.SetConfigAt(Settings.NonDefaultMenu, true);
                if(cmbMenuRenderer.SelectedIndex == 2) {
                    QTUtility.SetConfigAt(Settings.XPStyleMenus, true);
                }
            }
            if(!chbTreeShftWhlTab.Checked) {
                QTUtility.SetConfigAt(Settings.NoMidClickTree, true);
            }
            if(!chbTabSwitcher.Checked) {
                QTUtility.SetConfigAt(Settings.NoTabSwitcher, true);
            }
            if(chbTabTitleShadow.Checked) {
                QTUtility.SetConfigAt(Settings.TabTitleShadows, true);
            }
            if(chbAutoUpdate.Checked) {
                QTUtility.SetConfigAt(Settings.AutoUpdate, true);
            }
            if(!chbRemoveOnSeparate.Checked) {
                QTUtility.SetConfigAt(Settings.KeepOnSeparate, true);
            }
            if(chbDriveLetter.Checked) {
                QTUtility.SetConfigAt(Settings.ShowDriveLetters, true);
            }
            if(!chbPlaySound.Checked) {
                QTUtility.SetConfigAt(Settings.DisableSound, true);
            }
            if(!chbPreviewInfo.Checked) {
                QTUtility.SetConfigAt(Settings.PreviewInfo, true);
            }
            QTUtility.MaxCount_History = (int)nudMaxUndo.Value;
            QTUtility.MaxCount_Executed = (int)nudMaxRecentFile.Value;
            if(File.Exists(textBoxLang.Text)) {
                QTUtility.Path_LanguageFile = textBoxLang.Text;
            }
            else {
                QTUtility.Path_LanguageFile = string.Empty;
            }
            QTUtility.PreviewMaxWidth = (int)nudPreviewMaxWidth.Value;
            QTUtility.PreviewMaxHeight = (int)nudPreviewMaxHeight.Value;
            if(btnPreviewFont.Font != Font) {
                QTUtility.PreviewFontName = btnPreviewFont.Font.Name;
                QTUtility.PreviewFontSize = btnPreviewFont.Font.SizeInPoints;
            }
            else {
                QTUtility.PreviewFontName = null;
                QTUtility.PreviewFontSize = 0f;
            }
            IDLWrapper.iPingTimeOutMS = ((int)nudNetworkTimeOut.Value) * 0x3e8;
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar")) {
                SaveGroupTreeView(key);
                SaveUserAppsTreeView(key);
                SavePlugins(fApply);
                SaveShortcuts();
                SaveNoCapturePaths(key);
            }
            List<string> list = new List<string>();
            List<string> list2 = new List<string>();
            for(int i = 1; i < cmbTextExts.Items.Count; i++) {
                list.Add(cmbTextExts.Items[i].ToString());
            }
            for(int j = 1; j < cmbImgExts.Items.Count; j++) {
                list2.Add(cmbImgExts.Items[j].ToString());
            }
            WriteRegistry(btnTabFont.Font, list.ToArray(), list2.ToArray());
        }

        private void SaveShortcuts() {
            List<int> list = new List<int>();
            Dictionary<string, List<int>> dictionary = new Dictionary<string, List<int>>();
            for(int i = 0; i < listViewKeyboard.Items.Count; i++) {
                ListViewItem item = listViewKeyboard.Items[i];
                bool flag = item.Checked;
                int num2 = ((int)((Keys)item.Tag)) | (flag ? 0x100000 : 0);
                if(item.Group.Name == "general") {
                    list.Add(num2);
                }
                else {
                    string name = item.Name;
                    if(pluginManager.PluginInstantialized(name)) {
                        if(!dictionary.ContainsKey(name)) {
                            dictionary[name] = new List<int>();
                        }
                        dictionary[name].Add(num2);
                    }
                }
            }
            QTUtility.ShortcutKeys = list.ToArray();
            List<PluginKey> list2 = dictionary.Keys
                    .Select(str2 => new PluginKey(str2, dictionary[str2].ToArray())).ToList();
            QTUtility.dicPluginShortcutKeys.Clear();
            List<int> list3 = new List<int>();
            foreach(PluginKey key in list2) {
                QTUtility.dicPluginShortcutKeys[key.PluginID] = key.Keys;
                list3.AddRange(key.Keys);
            }
            QTUtility.PluginShortcutKeysCache = list3.ToArray();
            using(RegistryKey key2 = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar\Plugins")) {
                QTUtility2.WriteRegBinary(list2.ToArray(), "ShortcutKeys", key2);
            }
        }

        private static void SaveUserAppsRegistry(TreeNode tn, RegistryKey rk, ref int separatorIndex, bool fRoot) {
            if(tn.Text == "----------- Separator -----------") {
                string[] array = new string[] { string.Empty, string.Empty, string.Empty, string.Empty };
                string regValueName = "Separator" + (separatorIndex++);
                if(fRoot) {
                    QTUtility.UserAppsDic[regValueName] = array;
                }
                QTUtility2.WriteRegBinary(array, regValueName, rk);
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
                    QTUtility2.WriteRegBinary(strArray2, tn.Text, rk);
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
            bool flag;
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
                foreach(TreeNode node in tnRoot_UserApps.Nodes) {
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
            int index = pluginView.ItemsCount - 1;
            if(index > 0) {
                pluginView.SelectPlugin(index);
            }
        }

        private void SetValues() {
            bool isSupported = VisualStyleRenderer.IsSupported;
            chbActivateNew.Checked = (QTUtility.ConfigValues[0] & 0x80) == 0x80;
            chbDontOpenSame.Checked = (QTUtility.ConfigValues[0] & 0x40) == 0x40;
            chbCloseWhenGroup.Checked = (QTUtility.ConfigValues[0] & 0x20) == 0x20;
            chbRestoreClosed.Checked = (QTUtility.ConfigValues[0] & 0x10) == 0x10;
            chbShowTooltip.Checked = (QTUtility.ConfigValues[0] & 8) == 8;
            chbNeverCloseWindow.Checked = (QTUtility.ConfigValues[0] & 2) == 2;
            chbNeverCloseWndLocked.Checked = (QTUtility.ConfigValues[0] & 1) == 1;
            cmbNewTabLoc.SelectedIndex = QTUtility.ConfigValues[1];
            cmbActvClose.SelectedIndex = QTUtility.ConfigValues[2];
            if(QTUtility.ConfigValues[3] > (cmbTabDblClck.Items.Count - 1)) {
                QTUtility.ConfigValues[3] = (byte)(cmbTabDblClck.Items.Count - 1);
            }
            if(QTUtility.ConfigValues[4] > (cmbBGDblClick.Items.Count - 1)) {
                QTUtility.ConfigValues[4] = (byte)(cmbBGDblClick.Items.Count - 1);
            }
            if(QTUtility.ConfigValues[12] > (cmbTabWhlClck.Items.Count - 1)) {
                QTUtility.ConfigValues[12] = 0;
            }
            cmbTabDblClck.SelectedIndex = QTUtility.ConfigValues[3];
            cmbBGDblClick.SelectedIndex = QTUtility.ConfigValues[4];
            cmbTabWhlClck.SelectedIndex = QTUtility.ConfigValues[12];
            cmbTabTextAlignment.SelectedIndex = QTUtility.CheckConfig(Settings.AlignTabTextCenter) ? 1 : 0;
            chbNavBtn.Checked = (QTUtility.ConfigValues[5] & 0x80) == 0x80;
            if((QTUtility.ConfigValues[5] & 0x40) == 0x40) {
                cmbNavBtn.SelectedIndex = 1;
            }
            else {
                cmbNavBtn.SelectedIndex = 0;
            }
            if((QTUtility.ConfigValues[5] & 0x80) == 0) {
                cmbNavBtn.Enabled = false;
            }
            chbX1X2.Checked = (QTUtility.ConfigValues[5] & 0x20) == 0x20;
            chbBoldActv.Checked = (QTUtility.ConfigValues[5] & 0x10) == 0x10;
            chbUseTabSkin.Checked = (QTUtility.ConfigValues[5] & 8) == 8;
            chbNoHistory.Checked = (QTUtility.ConfigValues[5] & 2) == 0;
            chbWhlClick.Checked = (QTUtility.ConfigValues[5] & 1) == 0;
            propertyGrid1.Enabled = btnHiliteClsc.Enabled = tbTabImagePath.Enabled = btnTabImage.Enabled = chbUseTabSkin.Checked;
            if(!isSupported) {
                chbUseTabSkin.Checked = btnHiliteClsc.Enabled = tbTabImagePath.Enabled = btnTabImage.Enabled = true;
                chbUseTabSkin.Enabled = false;
            }
            tbTabImagePath.Text = QTUtility.Path_TabImage;
            tabImageSetting.SizingMargin = QTUtility.TabImageSizingMargin;
            cmbWhlClick.SelectedIndex = ((QTUtility.ConfigValues[6] & 0x80) == 0x80) ? 1 : 0;
            cmbWhlClick.Enabled = chbWhlClick.Checked;
            chbNCADblClck.Checked = (QTUtility.ConfigValues[6] & 0x40) == 0;
            chbBlockProcess.Checked = (QTUtility.ConfigValues[6] & 0x10) == 0;
            chbRestoreLocked.Checked = (QTUtility.ConfigValues[6] & 8) == 8;
            chbFoldrTree.Checked = (QTUtility.ConfigValues[6] & 4) == 0;
            chbWndUnresizable.Checked = (QTUtility.ConfigValues[6] & 2) == 2;
            if(!QTUtility.CheckConfig(Settings.NoRecentFiles)) {
                if(QTUtility.CheckConfig(Settings.AllRecentFiles)) {
                    chbSaveExecuted.CheckState = CheckState.Checked;
                }
                else {
                    chbSaveExecuted.CheckState = CheckState.Indeterminate;
                }
            }
            else {
                chbSaveExecuted.CheckState = CheckState.Unchecked;
            }
            if(chbNeverCloseWndLocked.Checked) {
                chbRestoreLocked.Checked = false;
            }
            if((QTUtility.ConfigValues[7] & 0x20) == 0x20) {
                cmbMultiRow.SelectedIndex = 1;
            }
            else if((QTUtility.ConfigValues[7] & 0x10) == 0x10) {
                cmbMultiRow.SelectedIndex = 2;
            }
            else {
                cmbMultiRow.SelectedIndex = 0;
            }
            if((QTUtility.ConfigValues[0] & 4) == 4) {
                cmbTabSizeMode.SelectedIndex = 1;
                nudTabWidthMax.Enabled = nudTabWidthMin.Enabled = false;
            }
            else if((QTUtility.ConfigValues[5] & 4) == 4) {
                cmbTabSizeMode.SelectedIndex = 2;
            }
            else {
                cmbTabSizeMode.SelectedIndex = 0;
                nudTabWidth.Enabled = nudTabWidthMax.Enabled = nudTabWidthMin.Enabled = false;
            }
            nudTabWidthMax.Value = QTUtility.MaxTabWidth;
            nudTabWidthMin.Value = QTUtility.MinTabWidth;
            nudTabHeight.Value = QTUtility.TabHeight;
            if(QTUtility.TabWidth > nudTabWidth.Maximum) {
                nudTabWidth.Value = nudTabWidth.Maximum;
            }
            else if(QTUtility.TabWidth < nudTabWidth.Minimum) {
                nudTabWidth.Value = nudTabWidth.Minimum;
            }
            else {
                nudTabWidth.Value = QTUtility.TabWidth;
            }
            btnActTxtClr.ForeColor = QTUtility.TabTextColor_Active;
            btnInactTxtClr.ForeColor = QTUtility.TabTextColor_Inactv;
            btnHiliteClsc.ForeColor = QTUtility.TabHiliteColor;
            btnShadowAct.ForeColor = QTUtility.TabTextColor_ActivShdw;
            btnShadowIna.ForeColor = QTUtility.TabTextColor_InAtvShdw;
            chbTabTitleShadow.Checked = QTUtility.CheckConfig(Settings.TabTitleShadows);
            btnShadowAct.Enabled = btnShadowIna.Enabled = chbTabTitleShadow.Checked;
            textBoxAction_BarDblClck.Text = QTUtility.Action_BarDblClick;
            QTUtility.RefreshGroupsDic();
            InitializeTreeView_Group();
            QTUtility.RefreshUserAppDic(false);
            InitializeTreeView_UserApps();
            btnTabFont.Font = QTUtility.TabFont;
            if(QTUtility.CheckConfig(Settings.HideMenuBar)) {
                chbHideMenu.Checked = true;
            }
            if(!QTUtility.IsXP) {
                chbBSUpOneLvl.Checked = (QTUtility.ConfigValues[7] & 1) == 1;
                chbWhlChangeView.Enabled = chbSelectWithoutExt.Enabled = chbTreeShftWhlTab.Enabled = false;
            }
            else {
                chbBSUpOneLvl.Enabled = false;
                chbSelectWithoutExt.Checked = (QTUtility.ConfigValues[8] & 0x20) == 0;
                chbTreeShftWhlTab.Checked = !QTUtility.CheckConfig(Settings.NoMidClickTree);
            }
            chbForceSysListView.Enabled = chbAlwaysShowHeader.Enabled =
                     Environment.OSVersion.Version.ToString().StartsWith("6.1.");
            chbForceSysListView.Checked = QTUtility.CheckConfig(Settings.ForceSysListView);
            chbAlwaysShowHeader.Checked = QTUtility.CheckConfig(Settings.AlwaysShowHeaders);
            btnToolBarBGClr.Enabled = chbToolbarBGClr.Checked = (QTUtility.ConfigValues[7] & 4) == 4;
            btnToolBarBGClr.BackColor = QTUtility.RebarBGColor;
            chbFolderIcon.Checked = (QTUtility.ConfigValues[7] & 2) == 2;
            chbGridLine.Checked = (QTUtility.ConfigValues[8] & 0x80) == 0x80;
            chbNoFulRowSelect.Checked = ((QTUtility.ConfigValues[8] & 0x40) == 0x40) ^ !QTUtility.IsXP;
            chbAlternateColor.Checked = (QTUtility.ConfigValues[8] & 8) == 8;
            btnAlternateColor.BackColor = QTUtility2.MakeColor(QTUtility.ShellViewRowCOLORREF_Background);
            btnAlternateColor_Text.ForeColor = QTUtility2.MakeColor(QTUtility.ShellViewRowCOLORREF_Text);
            btnAlternate_Default.Enabled = btnAlternateColor.Enabled = btnAlternateColor_Text.Enabled = chbAlternateColor.Checked;
            if(isSupported) {
                chbRebarBGImage.Checked = (QTUtility.ConfigValues[11] & 0x80) == 0x80;
                tbRebarImagePath.Text = QTUtility.Path_RebarImage;
                if((QTUtility.ConfigValues[11] & 0x40) == 0x40) {
                    cmbRebarBGImageMode.SelectedIndex = 1;
                }
                else if((QTUtility.ConfigValues[11] & 0x20) == 0x20) {
                    cmbRebarBGImageMode.SelectedIndex = 2;
                }
                else if(QTUtility.CheckConfig(Settings.RebarImageStretch2)) {
                    cmbRebarBGImageMode.SelectedIndex = 3;
                }
                else {
                    cmbRebarBGImageMode.SelectedIndex = 0;
                }
                cmbRebarBGImageMode.Enabled = btnRebarImage.Enabled = tbRebarImagePath.Enabled = chbRebarBGImage.Checked;
            }
            else {
                chbRebarBGImage.Enabled = tbRebarImagePath.Enabled = btnRebarImage.Enabled = cmbRebarBGImageMode.Enabled = false;
            }
            chbWndRestrAlpha.Checked = (QTUtility.ConfigValues[8] & 4) == 4;
            chbShowPreview.Checked = QTUtility.CheckConfig(Settings.ShowTooltipPreviews);
            chbPreviewMode.Checked = QTUtility.CheckConfig(Settings.PreviewsWithShift);
            chbPreviewInfo.Checked = !QTUtility.CheckConfig(Settings.PreviewInfo);
            nudPreviewMaxWidth.Enabled = nudPreviewMaxHeight.Enabled = cmbTextExts.Enabled = btnAddTextExt.Enabled = btnDelTextExt.Enabled = btnDefaultTextExt.Enabled = cmbImgExts.Enabled = btnAddImgExt.Enabled = btnDelImgExt.Enabled = btnDefaultImgExt.Enabled = btnPreviewFont.Enabled = btnPreviewFontDefault.Enabled = chbPreviewInfo.Enabled = chbPreviewMode.Enabled = chbShowPreview.Checked;
            nudPreviewMaxWidth.Value = QTUtility.PreviewMaxWidth;
            nudPreviewMaxHeight.Value = QTUtility.PreviewMaxHeight;
            if(QTUtility.PreviewFontName != null) {
                try {
                    btnPreviewFont.Font = new Font(QTUtility.PreviewFontName, QTUtility.PreviewFontSize);
                }
                catch {
                }
            }
            chbSubDirTip.Checked = !QTUtility.CheckConfig(Settings.NoShowSubDirTips);
            chbSubDirTipMode.Checked = QTUtility.CheckConfig(Settings.SubDirTipsWithShift);
            chbSubDirTipModeHidden.Checked = QTUtility.CheckConfig(Settings.SubDirTipsHidden);
            chbSubDirTipModeFile.Checked = QTUtility.CheckConfig(Settings.SubDirTipsFiles);
            chbSubDirTipPreview.Checked = !QTUtility.CheckConfig(Settings.SubDirTipsPreview);
            chbSubDirTipMode.Enabled = chbSubDirTipModeHidden.Enabled = chbSubDirTipModeFile.Enabled = chbSubDirTipPreview.Enabled = chbSubDirTip.Checked;
            chbSubDirTipModeSystem.Checked = QTUtility.CheckConfig(Settings.SubDirTipsSystem);
            nudMaxUndo.Value = QTUtility.MaxCount_History;
            nudMaxRecentFile.Value = QTUtility.MaxCount_Executed;
            chbDD.Checked = QTUtility.CheckConfig(Settings.DragDropOntoTabs);
            chbNoTabFromOuteside.Checked = QTUtility.CheckConfig(Settings.NoTabsFromOutside);
            chbHolizontalScroll.Checked = !QTUtility.CheckConfig(Settings.HorizontalScroll);
            chbWhlChangeView.Checked = !QTUtility.CheckConfig(Settings.CtrlWheelChangeView);
            chbAutoSubText.Checked = !QTUtility.CheckConfig(Settings.NoRenameAmbTabs);
            chbSendToTray.Checked = QTUtility.CheckConfig(Settings.TrayOnClose);
            chbSendToTrayOnMinimize.Checked = QTUtility.CheckConfig(Settings.TrayOnMinimize);
            chbF2Selection.Checked = !QTUtility.CheckConfig(Settings.F2Selection);
            chbTabCloseBtnAlt.Enabled = chbTabCloseBtnHover.Enabled = chbTabCloseButton.Checked = QTUtility.CheckConfig(Settings.ShowTabCloseButtons);
            chbTabCloseBtnAlt.Checked = QTUtility.CheckConfig(Settings.TabCloseBtnsWithAlt);
            chbTabCloseBtnHover.Checked = QTUtility.CheckConfig(Settings.TabCloseBtnsOnHover);
            chbCursorLoop.Checked = QTUtility.CheckConfig(Settings.CursorLoop);
            chbDriveLetter.Enabled = chbSubDirTipOnTab.Enabled = chbFolderIcon.Checked;
            chbSubDirTipOnTab.Checked = QTUtility.CheckConfig(Settings.ShowSubDirTipOnTab);
            nudNetworkTimeOut.Value = IDLWrapper.iPingTimeOutMS / 0x3e8;
            textBoxLang.Text = QTUtility.Path_LanguageFile;
            if(QTUtility.CheckConfig(Settings.NonDefaultMenu)) {
                if(QTUtility.CheckConfig(Settings.XPStyleMenus)) {
                    cmbMenuRenderer.SelectedIndex = 2;
                }
                else {
                    cmbMenuRenderer.SelectedIndex = 1;
                }
            }
            else {
                cmbMenuRenderer.SelectedIndex = 0;
            }
            chbTabSwitcher.Checked = !QTUtility.CheckConfig(Settings.NoTabSwitcher);
            chbAutoUpdate.Checked = QTUtility.CheckConfig(Settings.AutoUpdate);
            chbRemoveOnSeparate.Checked = !QTUtility.CheckConfig(Settings.KeepOnSeparate);
            chbDriveLetter.Checked = QTUtility.CheckConfig(Settings.ShowDriveLetters);
            chbPlaySound.Checked = !QTUtility.CheckConfig(Settings.DisableSound);
            InitializePluginView();
            textBoxPluginLang.Text = QTUtility.Path_PluginLangFile;
            CreateShortcutItems();
            CreateNoCapturePaths();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e) {
            QTUtility.OptionsDialogTabIndex = tabControl1.SelectedIndex;
        }

        private void tbGroupKey_KeyPress(object sender, KeyPressEventArgs e) {
            e.Handled = true;
        }

        private void tbGroupKey_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) {
            TreeNode selectedNode = treeViewGroup.SelectedNode;
            if((((selectedNode != null) && (selectedNode.Tag != null)) && (selectedNode.Tag is MenuItemArguments)) && !IsInvalidShortcutKey(e.KeyCode, e.Modifiers)) {
                MenuItemArguments tag = (MenuItemArguments)selectedNode.Tag;
                int num = chbGroupKey.Checked ? 0x100000 : 0;
                if(e.KeyCode == Keys.Escape) {
                    tbGroupKey.Text = " - ";
                    tag.KeyShortcut = num;
                }
                else if(((e.Modifiers != Keys.None) && CheckExistance_Shortcuts(e.KeyData, null)) && (CheckExistance_UserAppKey(e.KeyData, tnRoot_UserApps, null) && CheckExistance_GroupKey(e.KeyData, selectedNode))) {
                    tbGroupKey.Text = QTUtility2.MakeKeyString(e.KeyData);
                    tag.KeyShortcut = num | (int)e.KeyData;
                }
            }
        }

        private void tbsUserApps_TextChanged(object sender, EventArgs e) {
            if(!fSuppressTextChangeEvent_UserApps) {
                TreeNode selectedNode = treeViewUserApps.SelectedNode;
                if(((selectedNode != null) && (selectedNode.Tag != null)) && (selectedNode.Tag is MenuItemArguments)) {
                    MenuItemArguments tag = (MenuItemArguments)selectedNode.Tag;
                    if(sender == tbPath) {
                        string str = QTUtility2.SanitizePathString(tbPath.Text);
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
                    else if(sender == tbArgs) {
                        tag.Argument = tag.OriginalArgument = tbArgs.Text;
                    }
                    else if(sender == tbWorking) {
                        tag.WorkingDirectory = tag.OriginalWorkingDirectory = QTUtility2.SanitizePathString(tbWorking.Text);
                    }
                }
            }
        }

        private void tbUserAppKey_KeyPress(object sender, KeyPressEventArgs e) {
            e.Handled = true;
        }

        private void tbUserAppKey_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) {
            TreeNode selectedNode = treeViewUserApps.SelectedNode;
            if((((selectedNode != null) && (selectedNode.Tag != null)) && (selectedNode.Tag is MenuItemArguments)) && !IsInvalidShortcutKey(e.KeyCode, e.Modifiers)) {
                MenuItemArguments tag = (MenuItemArguments)selectedNode.Tag;
                int num = chbUserAppKey.Checked ? 0x100000 : 0;
                if(e.KeyCode == Keys.Escape) {
                    tbUserAppKey.Text = " - ";
                    tag.KeyShortcut = num;
                }
                else if(((e.Modifiers != Keys.None) && CheckExistance_Shortcuts(e.KeyData, null)) && (CheckExistance_UserAppKey(e.KeyData, tnRoot_UserApps, selectedNode) && CheckExistance_GroupKey(e.KeyData, null))) {
                    tbUserAppKey.Text = QTUtility2.MakeKeyString(e.KeyData);
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
            fNowTreeNodeEditing = false;
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
            fSuppressTextChangeEvent_Group = true;
            int level = e.Node.Level;
            if(level != 1) {
                chbGroupKey.Checked = false;
                tbGroupKey.Clear();
                chbGroupKey.Enabled = tbGroupKey.Enabled = false;
            }
            if(level == 0) {
                btnMinus_Grp.Enabled = btnStartUpGrp.Enabled = btnUp_Grp.Enabled = btnDown_Grp.Enabled = false;
                cmbSpclFol_Grp.Enabled = btnAddSpcFol_Grp.Enabled = btnPlus_Grp.Enabled = true;
                fSuppressTextChangeEvent_Group = false;
            }
            else {
                MenuItemArguments tag = (MenuItemArguments)e.Node.Tag;
                if(tag == MIA_GROUPSEP) {
                    cmbSpclFol_Grp.Enabled = btnAddSpcFol_Grp.Enabled = btnStartUpGrp.Enabled = false;
                    tbGroupKey.Clear();
                    chbGroupKey.Checked = chbGroupKey.Enabled = tbGroupKey.Enabled = false;
                    btnPlus_Grp.Enabled = btnUp_Grp.Enabled = btnDown_Grp.Enabled = btnMinus_Grp.Enabled = true;
                }
                else {
                    if(level == 1) {
                        chbGroupKey.Enabled = true;
                        if(tag != null) {
                            chbGroupKey.Checked = (tag.KeyShortcut & 0x100000) != 0;
                            tbGroupKey.Text = QTUtility2.MakeKeyString(((Keys)tag.KeyShortcut) & ((Keys)(-1048577)));
                        }
                        else {
                            chbGroupKey.Checked = false;
                            tbGroupKey.Text = string.Empty;
                        }
                        tbGroupKey.Enabled = chbGroupKey.Checked;
                    }
                    cmbSpclFol_Grp.Enabled = btnAddSpcFol_Grp.Enabled = btnStartUpGrp.Enabled = btnPlus_Grp.Enabled = btnMinus_Grp.Enabled = true;
                }
                btnAddSep_Grp.Enabled = level != 2;
                int count = e.Node.Parent.Nodes.Count;
                if(count > 1) {
                    if(e.Node.Index == 0) {
                        btnUp_Grp.Enabled = false;
                        btnDown_Grp.Enabled = true;
                    }
                    else if(e.Node.Index == (count - 1)) {
                        btnUp_Grp.Enabled = true;
                        btnDown_Grp.Enabled = false;
                    }
                    else {
                        btnUp_Grp.Enabled = btnDown_Grp.Enabled = true;
                    }
                }
                else {
                    btnUp_Grp.Enabled = btnDown_Grp.Enabled = false;
                }
                fSuppressTextChangeEvent_Group = false;
            }
        }

        private void treeViewGroup_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e) {
            if((e.Node.Level == 0) || (e.Node.Tag == MIA_GROUPSEP)) {
                e.CancelEdit = true;
            }
            else {
                fNowTreeNodeEditing = true;
            }
        }

        private void treeViewGroup_KeyDown(object sender, KeyEventArgs e) {
            TreeNode selectedNode = treeViewGroup.SelectedNode;
            if((selectedNode != null) && (selectedNode.Level > 0)) {
                Keys keyCode = e.KeyCode;
                if(keyCode <= Keys.Down) {
                    switch(keyCode) {
                        case Keys.Up:
                            if(((e.Modifiers == Keys.Alt) || (e.Modifiers == Keys.Control)) && btnUp_Grp.Enabled) {
                                btnUp_Grp.PerformClick();
                                e.Handled = true;
                            }
                            return;

                        case Keys.Right:
                            return;

                        case Keys.Down:
                            if(((e.Modifiers == Keys.Alt) || (e.Modifiers == Keys.Control)) && btnDown_Grp.Enabled) {
                                btnDown_Grp.PerformClick();
                                e.Handled = true;
                            }
                            return;

                        case Keys.Return:
                            if(btnPlus_Grp.Enabled) {
                                btnPlus_Grp.PerformClick();
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
            fNowTreeNodeEditing = false;
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
            fSuppressTextChangeEvent_UserApps = true;
            btnAddVFolder_app.Enabled = e.Node.Level < 10;
            if(e.Node.Level == 0) {
                btnMinus_app.Enabled = btnUp_app.Enabled = btnDown_app.Enabled = false;
            }
            else {
                if(e.Node.Level == 1) {
                    if(e.Node.Index == 0) {
                        btnUp_app.Enabled = false;
                        btnDown_app.Enabled = tnRoot_UserApps.Nodes.Count > 1;
                    }
                    else if(e.Node.Index == (tnRoot_UserApps.Nodes.Count - 1)) {
                        btnUp_app.Enabled = true;
                        btnDown_app.Enabled = false;
                    }
                    else {
                        btnUp_app.Enabled = btnDown_app.Enabled = true;
                    }
                }
                else {
                    btnUp_app.Enabled = btnDown_app.Enabled = true;
                }
                btnMinus_app.Enabled = true;
            }
            if((e.Node.Tag == null) || (e.Node.Text == "----------- Separator -----------")) {
                tbPath.Text = tbArgs.Text = tbWorking.Text = tbUserAppKey.Text = string.Empty;
                btnOFD_app.Enabled = btnBFD_app.Enabled = btnAddToken_Arg.Enabled = btnAddToken_Wrk.Enabled = tbPath.Enabled = tbArgs.Enabled = tbWorking.Enabled = chbUserAppKey.Enabled = chbUserAppKey.Checked = tbUserAppKey.Enabled = false;
            }
            else {
                MenuItemArguments tag = (MenuItemArguments)e.Node.Tag;
                tbPath.Text = tag.Path;
                tbArgs.Text = tag.Argument;
                tbWorking.Text = tag.WorkingDirectory;
                tbUserAppKey.Text = QTUtility2.MakeKeyString(((Keys)tag.KeyShortcut) & ((Keys)(-1048577)));
                chbUserAppKey.Checked = (tag.KeyShortcut & 0x100000) != 0;
                btnOFD_app.Enabled = btnBFD_app.Enabled = btnAddToken_Arg.Enabled = btnAddToken_Wrk.Enabled = tbPath.Enabled = tbArgs.Enabled = tbWorking.Enabled = chbUserAppKey.Enabled = true;
                tbUserAppKey.Enabled = chbUserAppKey.Checked;
            }
            fSuppressTextChangeEvent_UserApps = false;
        }

        private void treeViewUserApps_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e) {
            if((e.Node.Level == 0) || ((e.Node.Tag != null) && (e.Node.Text == "----------- Separator -----------"))) {
                e.CancelEdit = true;
            }
            else {
                fNowTreeNodeEditing = true;
            }
        }

        private void treeViewUserApps_KeyDown(object sender, KeyEventArgs e) {
            TreeNode selectedNode = treeViewUserApps.SelectedNode;
            if((selectedNode != null) && (selectedNode.Level > 0)) {
                switch(e.KeyCode) {
                    case Keys.Up:
                        if(((e.Modifiers == Keys.Alt) || (e.Modifiers == Keys.Control)) && btnUp_app.Enabled) {
                            btnUp_app.PerformClick();
                            e.Handled = true;
                            Application.DoEvents();
                        }
                        return;

                    case Keys.Right:
                        return;

                    case Keys.Down:
                        if(((e.Modifiers == Keys.Alt) || (e.Modifiers == Keys.Control)) && btnDown_app.Enabled) {
                            btnDown_app.PerformClick();
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
            bool flag = sender == btnUp_Grp;
            TreeNode selectedNode = treeViewGroup.SelectedNode;
            if(selectedNode != null) {
                int index = selectedNode.Index;
                int level = selectedNode.Level;
                if(level != 0) {
                    TreeNode parent = selectedNode.Parent;
                    treeViewGroup.BeginUpdate();
                    if(flag && (index != 0)) {
                        selectedNode.Remove();
                        parent.Nodes.Insert(index - 1, selectedNode);
                        treeViewGroup.SelectedNode = selectedNode;
                        if((level == 2) && ((index - 1) == 0)) {
                            parent.ImageKey = parent.SelectedImageKey = selectedNode.ImageKey;
                        }
                    }
                    else if(!flag && (index != (parent.Nodes.Count - 1))) {
                        selectedNode.Remove();
                        parent.Nodes.Insert(index + 1, selectedNode);
                        treeViewGroup.SelectedNode = selectedNode;
                        if((level == 2) && (index == 0)) {
                            parent.ImageKey = parent.SelectedImageKey = parent.Nodes[0].ImageKey;
                        }
                    }
                    treeViewGroup.EndUpdate();
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
                    if((tabFont != null) && !tabFont.Equals(DefaultFont)) {
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
                        foreach(string ext in strsTextExts) {
                            QTUtility.PreviewExtsList_Txt.Add(ext);
                            str = str + ext + ";";
                        }
                        if(str.Length > 0) {
                            str = str.TrimEnd(QTUtility.SEPARATOR_CHAR);
                        }
                    }
                    if(strsImgExts != null) {
                        foreach(string ext in strsImgExts) {
                            QTUtility.PreviewExtsList_Img.Add(ext);
                            str2 = str2 + ext + ";";
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
                    QTUtility2.WriteRegBinary(QTUtility.ShortcutKeys, "ShortcutKeys", key);
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

        private sealed class ListViewEx : ListView {
            private StringFormat sfCenter = new StringFormat(StringFormatFlags.NoWrap);
            private StringFormat sfLeft = new StringFormat(StringFormatFlags.NoWrap);
            private VisualStyleRenderer vsrChecked;
            private VisualStyleRenderer vsrUnChecked;

            public ListViewEx() {
                OwnerDraw = true;
                DoubleBuffered = true;
                sfLeft.LineAlignment = StringAlignment.Center;
                sfLeft.Trimming = StringTrimming.EllipsisCharacter;
                sfCenter.Alignment = StringAlignment.Center;
                sfCenter.LineAlignment = StringAlignment.Center;
                sfCenter.Trimming = StringTrimming.EllipsisCharacter;
            }

            protected override void Dispose(bool disposing) {
                sfCenter.Dispose();
                sfLeft.Dispose();
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
                    RectangleF layoutRectangle = new RectangleF((e.Bounds.X + 20), e.Bounds.Y, (e.Bounds.Width - 20), e.Bounds.Height);
                    if(VisualStyleRenderer.IsSupported) {
                        if(vsrChecked == null) {
                            vsrChecked = new VisualStyleRenderer(VisualStyleElement.Button.CheckBox.CheckedNormal);
                            vsrUnChecked = new VisualStyleRenderer(VisualStyleElement.Button.CheckBox.UncheckedNormal);
                        }
                        if(e.Item.Checked) {
                            vsrChecked.DrawBackground(e.Graphics, bounds);
                        }
                        else {
                            vsrUnChecked.DrawBackground(e.Graphics, bounds);
                        }
                    }
                    else {
                        ControlPaint.DrawCheckBox(e.Graphics, bounds, e.Item.Checked ? ButtonState.Checked : ButtonState.Normal);
                    }
                    e.Graphics.DrawString(e.Item.Text, Font, brush, layoutRectangle, sfLeft);
                }
                else if(e.ColumnIndex == 1) {
                    e.Graphics.DrawString(e.SubItem.Text, Font, brush, e.Bounds, sfCenter);
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
                    return padding;
                }
                set {
                    padding = value;
                    if((padding.Left > 50) || (padding.Left < 0)) {
                        padding.Left = 0;
                    }
                    if((padding.Top > 50) || (padding.Top < 0)) {
                        padding.Top = 0;
                    }
                    if((padding.Right > 50) || (padding.Right < 0)) {
                        padding.Right = 0;
                    }
                    if((padding.Bottom > 50) || (padding.Bottom < 0)) {
                        padding.Bottom = 0;
                    }
                }
            }
        }
    }
}
