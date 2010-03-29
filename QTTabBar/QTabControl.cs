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
    using QTTabBarLib.Interop;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;

    internal sealed class QTabControl : Control {
        private Bitmap bmpCloseBtn_Cold;
        private Bitmap bmpCloseBtn_ColdAlt;
        private Bitmap bmpCloseBtn_Hot;
        private Bitmap bmpCloseBtn_Pressed;
        private Bitmap bmpFolIconBG;
        private Bitmap bmpLocked;
        private SolidBrush brshActive;
        private SolidBrush brshInactv;
        private Color[] colorSet;
        private IContainer components;
        private QTabItemBase draggingTab;
        private bool fActiveTxtBold;
        private bool fAutoSubText;
        private bool fCloseBtnOnHover;
        private bool fDrawCloseButton;
        private bool fDrawFolderImg;
        private bool fDrawShadow;
        private bool fForceClassic;
        private bool fLimitSize;
        private bool fNeedToDrawUpDown;
        private bool fNowMouseIsOnCloseBtn;
        private bool fNowMouseIsOnIcon;
        private bool fNowShowCloseBtnAlt;
        private bool fNowTabContextMenuStripShowing;
        private Font fnt_Underline;
        private Font fntBold;
        private Font fntBold_Underline;
        private Font fntDriveLetter;
        private Font fntSubText;
        private bool fOncePainted;
        internal const float FONTSIZE_DIFF = 0.75f;
        private bool fRedrawSuspended;
        private bool fShowSubDirTip;
        private bool fShowToolTips;
        private bool fSubDirShown;
        private bool fSuppressDoubleClick;
        private bool fSuppressMouseUp;
        private QTabItemBase hotTab;
        private int iCurrentRow;
        private int iFocusedTabIndex = -1;
        private int iMultipleType;
        private int iPointedChanged_LastRaisedIndex = -2;
        private int iPseudoHotIndex = -1;
        private int iScrollClickedCount;
        private int iScrollWidth;
        private int iSelectedIndex;
        private int iTabIndexOfSubDirShown = -1;
        private int iTabMouseOnButtonsIndex = -1;
        private Size itemSize = new Size(100, 0x18);
        private int iToolTipIndex = -1;
        private int maxTabWidth = 10;
        private int minTabWidth = 10;
        private QTabItemBase selectedTabPage;
        private StringFormat sfTypoGraphic;
        private TabSizeMode sizeMode;
        private Padding sizingMargin;
        private Bitmap[] tabImages;
        private QTabCollection tabPages;
        private StringAlignment tabTextAlignment;
        private Timer timerSuppressDoubleClick;
        private System.Windows.Forms.ToolTip toolTip;
        private UpDown upDown;
        private const int UPDOWN_WIDTH = 0x24;
        private VisualStyleRenderer vsr_LHot;
        private VisualStyleRenderer vsr_LNormal;
        private VisualStyleRenderer vsr_LPressed;
        private VisualStyleRenderer vsr_MHot;
        private VisualStyleRenderer vsr_MNormal;
        private VisualStyleRenderer vsr_MPressed;
        private VisualStyleRenderer vsr_RHot;
        private VisualStyleRenderer vsr_RNormal;
        private VisualStyleRenderer vsr_RPressed;

        public event QTabCancelEventHandler CloseButtonClicked;

        public event QTabCancelEventHandler Deselecting;

        public event ItemDragEventHandler ItemDrag;

        public event QTabCancelEventHandler PointedTabChanged;

        public event QEventHandler RowCountChanged;

        public event EventHandler SelectedIndexChanged;

        public event QTabCancelEventHandler Selecting;

        public event QTabCancelEventHandler TabCountChanged;

        public event QTabCancelEventHandler TabIconMouseDown;

        public QTabControl() {
            base.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.SupportsTransparentBackColor | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            this.components = new Container();
            this.tabPages = new QTabCollection(this);
            this.BackColor = Color.Transparent;
            this.sfTypoGraphic = StringFormat.GenericTypographic;
            this.sfTypoGraphic.FormatFlags |= StringFormatFlags.NoWrap;
            this.sfTypoGraphic.Trimming = StringTrimming.EllipsisCharacter;
            this.colorSet = new Color[] { QTUtility.TabTextColor_Active, QTUtility.TabTextColor_Inactv, QTUtility.TabHiliteColor, QTUtility.TabTextColor_ActivShdw, QTUtility.TabTextColor_InAtvShdw };
            this.brshActive = new SolidBrush(this.colorSet[0]);
            this.brshInactv = new SolidBrush(this.colorSet[1]);
            this.timerSuppressDoubleClick = new Timer(this.components);
            this.timerSuppressDoubleClick.Interval = SystemInformation.DoubleClickTime + 100;
            this.timerSuppressDoubleClick.Tick += new EventHandler(this.timerSuppressDoubleClick_Tick);
            if(VisualStyleRenderer.IsSupported) {
                this.InitializeRenderer();
            }
        }

        private bool CalculateItemRectangle() {
            int x = 0;
            int count = this.tabPages.Count;
            if(this.sizeMode == TabSizeMode.Fixed) {
                for(int i = 0; i < count; i++) {
                    this.tabPages[i].TabBounds = new Rectangle(x, 0, this.itemSize.Width, this.itemSize.Height);
                    this.tabPages[i].Edge = 0;
                    x += this.itemSize.Width;
                }
            }
            else {
                int width;
                if(this.fLimitSize) {
                    for(int j = 0; j < count; j++) {
                        width = this.tabPages[j].TabBounds.Width;
                        if(width > this.maxTabWidth) {
                            width = this.maxTabWidth;
                        }
                        if(width < this.minTabWidth) {
                            width = this.minTabWidth;
                        }
                        this.tabPages[j].TabBounds = new Rectangle(x, 0, width, this.itemSize.Height);
                        this.tabPages[j].Edge = 0;
                        x += width;
                    }
                }
                else {
                    for(int k = 0; k < count; k++) {
                        width = this.tabPages[k].TabBounds.Width;
                        this.tabPages[k].TabBounds = new Rectangle(x, 0, width, this.itemSize.Height);
                        this.tabPages[k].Edge = 0;
                        x += width;
                    }
                }
            }
            if(this.tabPages.Count > 1) {
                this.tabPages[0].Edge = Edges.Left;
                this.tabPages[this.tabPages.Count - 1].Edge = Edges.Right;
            }
            return (x > (base.Width - 0x24));
        }

        private void CalculateItemRectangle_MultiRows() {
            int x = 0;
            int count = this.tabPages.Count;
            int width = base.Width;
            int num4 = this.itemSize.Width;
            int height = this.itemSize.Height;
            int num6 = height - 3;
            int num7 = 0;
            int num8 = 0;
            if(this.sizeMode == TabSizeMode.Fixed) {
                for(int i = 0; i < count; i++) {
                    if((x + num4) > width) {
                        num7++;
                        x = 0;
                    }
                    this.tabPages[i].TabBounds = new Rectangle(x, num6 * num7, num4, height);
                    this.tabPages[i].Row = num7;
                    if(x == 0) {
                        this.tabPages[i].Edge = Edges.Left;
                    }
                    else if((i == (count - 1)) || (((x + num4) + num4) > width)) {
                        this.tabPages[i].Edge = Edges.Right;
                    }
                    else {
                        this.tabPages[i].Edge = 0;
                    }
                    x += num4;
                    if(i == this.iSelectedIndex) {
                        num8 = num7;
                    }
                }
            }
            else {
                int maxTabWidth;
                if(this.fLimitSize) {
                    for(int j = 0; j < count; j++) {
                        maxTabWidth = this.tabPages[j].TabBounds.Width;
                        if(maxTabWidth > this.maxTabWidth) {
                            maxTabWidth = this.maxTabWidth;
                        }
                        if(maxTabWidth < this.minTabWidth) {
                            maxTabWidth = this.minTabWidth;
                        }
                        if((x + maxTabWidth) > width) {
                            num7++;
                            x = 0;
                        }
                        this.tabPages[j].TabBounds = new Rectangle(x, num6 * num7, maxTabWidth, height);
                        this.tabPages[j].Row = num7;
                        if(x == 0) {
                            this.tabPages[j].Edge = Edges.Left;
                        }
                        else if(j == (count - 1)) {
                            this.tabPages[j].Edge = Edges.Right;
                        }
                        else {
                            int minTabWidth = this.tabPages[j + 1].TabBounds.Width;
                            if(minTabWidth > this.maxTabWidth) {
                                minTabWidth = this.maxTabWidth;
                            }
                            if(minTabWidth < this.minTabWidth) {
                                minTabWidth = this.minTabWidth;
                            }
                            if(((x + maxTabWidth) + minTabWidth) > width) {
                                this.tabPages[j].Edge = Edges.Right;
                            }
                            else {
                                this.tabPages[j].Edge = 0;
                            }
                        }
                        x += maxTabWidth;
                        if(j == this.iSelectedIndex) {
                            num8 = num7;
                        }
                    }
                }
                else {
                    for(int k = 0; k < count; k++) {
                        maxTabWidth = this.tabPages[k].TabBounds.Width;
                        if((x + maxTabWidth) > width) {
                            num7++;
                            x = 0;
                        }
                        this.tabPages[k].TabBounds = new Rectangle(x, num6 * num7, maxTabWidth, height);
                        this.tabPages[k].Row = num7;
                        if(x == 0) {
                            this.tabPages[k].Edge = Edges.Left;
                        }
                        else if(k == (count - 1)) {
                            this.tabPages[k].Edge = Edges.Right;
                        }
                        else {
                            int num14 = this.tabPages[k + 1].TabBounds.Width;
                            if(((x + maxTabWidth) + num14) > width) {
                                this.tabPages[k].Edge = Edges.Right;
                            }
                            else {
                                this.tabPages[k].Edge = 0;
                            }
                        }
                        x += maxTabWidth;
                        if(k == this.iSelectedIndex) {
                            num8 = num7;
                        }
                    }
                }
            }
            if((num7 != 0) && (this.iMultipleType == 1)) {
                int num15 = num7 - num8;
                if(num15 > 0) {
                    for(int m = 0; m < count; m++) {
                        QTabItemBase base2 = this.tabPages[m];
                        Rectangle tabBounds = base2.TabBounds;
                        if(base2.Row > num8) {
                            base2.Row -= num8 + 1;
                            tabBounds.Y = base2.Row * num6;
                            base2.TabBounds = tabBounds;
                        }
                        else {
                            tabBounds.Y += num15 * num6;
                            base2.TabBounds = tabBounds;
                            base2.Row += num15;
                        }
                    }
                }
            }
            if(num7 != this.iCurrentRow) {
                this.iCurrentRow = num7;
                if(this.RowCountChanged != null) {
                    this.RowCountChanged(this, new QEventArgs(this.iCurrentRow + 1));
                }
            }
        }

        private bool ChangeSelection(QTabItemBase tabToSelect, int index) {
            if(((this.Deselecting != null) && (this.iSelectedIndex > -1)) && (this.iSelectedIndex < this.tabPages.Count)) {
                QTabCancelEventArgs e = new QTabCancelEventArgs(this.tabPages[this.iSelectedIndex], this.iSelectedIndex, false, TabControlAction.Deselecting);
                this.Deselecting(this, e);
            }
            int iSelectedIndex = this.iSelectedIndex;
            QTabItemBase selectedTabPage = this.selectedTabPage;
            this.iSelectedIndex = index;
            this.selectedTabPage = tabToSelect;
            if(this.Selecting != null) {
                QTabCancelEventArgs args2 = new QTabCancelEventArgs(tabToSelect, index, false, TabControlAction.Selecting);
                this.Selecting(this, args2);
                if(args2.Cancel) {
                    this.iSelectedIndex = iSelectedIndex;
                    this.selectedTabPage = selectedTabPage;
                    return false;
                }
            }
            if(this.fNeedToDrawUpDown) {
                if((tabToSelect.TabBounds.X + this.iScrollWidth) < 0) {
                    this.iScrollWidth = -tabToSelect.TabBounds.X;
                    this.iScrollClickedCount = index;
                }
                else if((tabToSelect.TabBounds.X + this.iScrollWidth) > (base.Width - 0x24)) {
                    while((tabToSelect.TabBounds.Right + this.iScrollWidth) > base.Width) {
                        this.OnUpDownClicked(true, true);
                    }
                }
            }
            this.Refresh();
            if(this.SelectedIndexChanged != null) {
                this.SelectedIndexChanged(this, new EventArgs());
            }
            this.iFocusedTabIndex = -1;
            return true;
        }

        protected override void Dispose(bool disposing) {
            if(disposing && (this.components != null)) {
                this.components.Dispose();
            }
            if(this.brshActive != null) {
                this.brshActive.Dispose();
                this.brshActive = null;
            }
            if(this.brshInactv != null) {
                this.brshInactv.Dispose();
                this.brshInactv = null;
            }
            if(this.sfTypoGraphic != null) {
                this.sfTypoGraphic.Dispose();
                this.sfTypoGraphic = null;
            }
            if(this.bmpLocked != null) {
                this.bmpLocked.Dispose();
                this.bmpLocked = null;
            }
            if(this.bmpCloseBtn_Cold != null) {
                this.bmpCloseBtn_Cold.Dispose();
                this.bmpCloseBtn_Cold = null;
            }
            if(this.bmpCloseBtn_Hot != null) {
                this.bmpCloseBtn_Hot.Dispose();
                this.bmpCloseBtn_Hot = null;
            }
            if(this.bmpCloseBtn_Pressed != null) {
                this.bmpCloseBtn_Pressed.Dispose();
                this.bmpCloseBtn_Pressed = null;
            }
            if(this.bmpCloseBtn_ColdAlt != null) {
                this.bmpCloseBtn_ColdAlt.Dispose();
            }
            if(this.bmpFolIconBG != null) {
                this.bmpFolIconBG.Dispose();
                this.bmpFolIconBG = null;
            }
            if(this.fnt_Underline != null) {
                this.fnt_Underline.Dispose();
                this.fnt_Underline = null;
            }
            if(this.fntBold != null) {
                this.fntBold.Dispose();
                this.fntBold = null;
            }
            if(this.fntBold_Underline != null) {
                this.fntBold_Underline.Dispose();
                this.fntBold_Underline = null;
            }
            if(this.fntSubText != null) {
                this.fntSubText.Dispose();
                this.fntSubText = null;
            }
            if(this.fntDriveLetter != null) {
                this.fntDriveLetter.Dispose();
                this.fntDriveLetter = null;
            }
            foreach(QTabItemBase base2 in this.tabPages) {
                if(base2 != null) {
                    base2.OnClose();
                }
            }
            base.Dispose(disposing);
        }

        private void DrawBackground(Graphics g, bool bSelected, bool fHot, Rectangle rctItem, Edges edges, bool fVisualStyle, int index) {
            if(!fVisualStyle) {
                int num = bSelected ? 0 : 1;
                if(this.tabImages == null) {
                    g.FillRectangle(SystemBrushes.Control, rctItem);
                    g.DrawLine(SystemPens.ControlLightLight, new Point(rctItem.X + 2, rctItem.Y), new Point(((rctItem.X + rctItem.Width) - 2) - num, rctItem.Y));
                    g.DrawLine(SystemPens.ControlLightLight, new Point(rctItem.X + 2, rctItem.Y), new Point(rctItem.X, rctItem.Y + 2));
                    g.DrawLine(SystemPens.ControlLightLight, new Point(rctItem.X, rctItem.Y + 2), new Point(rctItem.X, (rctItem.Y + rctItem.Height) - 1));
                    g.DrawLine(SystemPens.ControlDarkDark, new Point((rctItem.X + rctItem.Width) - num, rctItem.Y + 2), new Point((rctItem.X + rctItem.Width) - num, (rctItem.Y + rctItem.Height) - 1));
                    g.DrawLine(SystemPens.ControlDark, new Point(((rctItem.X + rctItem.Width) - num) - 1, rctItem.Y + 1), new Point(((rctItem.X + rctItem.Width) - num) - 1, (rctItem.Y + rctItem.Height) - 1));
                    g.DrawLine(SystemPens.ControlDarkDark, new Point(((rctItem.X + rctItem.Width) - num) - 1, rctItem.Y + 1), new Point((rctItem.X + rctItem.Width) - num, rctItem.Y + 2));
                    if(bSelected) {
                        Pen pen = new Pen(this.colorSet[2], 2f);
                        g.DrawLine(pen, new Point(rctItem.X, (rctItem.Y + rctItem.Height) - 1), new Point((rctItem.X + rctItem.Width) + 1, (rctItem.Y + rctItem.Height) - 1));
                        pen.Dispose();
                    }
                }
                else {
                    Bitmap bitmap;
                    if(bSelected) {
                        bitmap = this.tabImages[0];
                    }
                    else if(fHot || (this.iPseudoHotIndex == index)) {
                        bitmap = this.tabImages[2];
                    }
                    else {
                        bitmap = this.tabImages[1];
                    }
                    if(bitmap != null) {
                        int left = this.sizingMargin.Left;
                        int top = this.sizingMargin.Top;
                        int right = this.sizingMargin.Right;
                        int bottom = this.sizingMargin.Bottom;
                        int vertical = this.sizingMargin.Vertical;
                        int horizontal = this.sizingMargin.Horizontal;
                        Rectangle[] rectangleArray = new Rectangle[] { new Rectangle(rctItem.X, rctItem.Y, left, top), new Rectangle(rctItem.X + left, rctItem.Y, rctItem.Width - horizontal, top), new Rectangle(rctItem.Right - right, rctItem.Y, right, top), new Rectangle(rctItem.X, rctItem.Y + top, left, rctItem.Height - vertical), new Rectangle(rctItem.X + left, rctItem.Y + top, rctItem.Width - horizontal, rctItem.Height - vertical), new Rectangle(rctItem.Right - right, rctItem.Y + top, right, rctItem.Height - vertical), new Rectangle(rctItem.X, rctItem.Bottom - bottom, left, bottom), new Rectangle(rctItem.X + left, rctItem.Bottom - bottom, rctItem.Width - horizontal, bottom), new Rectangle(rctItem.Right - right, rctItem.Bottom - bottom, right, bottom) };
                        Rectangle[] rectangleArray2 = new Rectangle[9];
                        int width = bitmap.Width;
                        int height = bitmap.Height;
                        rectangleArray2[0] = new Rectangle(0, 0, left, top);
                        rectangleArray2[1] = new Rectangle(left, 0, width - horizontal, top);
                        rectangleArray2[2] = new Rectangle(width - right, 0, right, top);
                        rectangleArray2[3] = new Rectangle(0, top, left, height - vertical);
                        rectangleArray2[4] = new Rectangle(left, top, width - horizontal, height - vertical);
                        rectangleArray2[5] = new Rectangle(width - right, top, right, height - vertical);
                        rectangleArray2[6] = new Rectangle(0, height - bottom, left, bottom);
                        rectangleArray2[7] = new Rectangle(left, height - bottom, width - horizontal, bottom);
                        rectangleArray2[8] = new Rectangle(width - right, height - bottom, right, bottom);
                        for(int i = 0; i < 9; i++) {
                            g.DrawImage(bitmap, rectangleArray[i], rectangleArray2[i], GraphicsUnit.Pixel);
                        }
                    }
                }
            }
            else {
                VisualStyleRenderer renderer;
                if(!bSelected) {
                    if(!fHot && (this.iPseudoHotIndex != index)) {
                        Edges edges4 = edges;
                        if(edges4 == Edges.Left) {
                            renderer = this.vsr_LNormal;
                        }
                        else if(edges4 == Edges.Right) {
                            renderer = this.vsr_RNormal;
                        }
                        else {
                            renderer = this.vsr_MNormal;
                        }
                    }
                    else {
                        Edges edges3 = edges;
                        if(edges3 == Edges.Left) {
                            renderer = this.vsr_LHot;
                        }
                        else if(edges3 == Edges.Right) {
                            renderer = this.vsr_RHot;
                        }
                        else {
                            renderer = this.vsr_MHot;
                        }
                    }
                }
                else {
                    Edges edges2 = edges;
                    if(edges2 == Edges.Left) {
                        renderer = this.vsr_LPressed;
                    }
                    else if(edges2 == Edges.Right) {
                        renderer = this.vsr_RPressed;
                    }
                    else {
                        renderer = this.vsr_MPressed;
                    }
                    renderer.DrawBackground(g, rctItem);
                    return;
                }
                renderer.DrawBackground(g, rctItem);
            }
        }

        private static void DrawDriveLetter(Graphics g, string str, Font fnt, Rectangle rctFldImg, bool fSelected) {
            Rectangle layoutRectangle = new Rectangle(rctFldImg.X + 7, rctFldImg.Y + 6, 0x10, 0x10);
            using(SolidBrush brush = new SolidBrush(QTUtility2.MakeModColor(fSelected ? QTUtility.TabTextColor_ActivShdw : QTUtility.TabTextColor_InAtvShdw))) {
                Rectangle rectangle2 = layoutRectangle;
                rectangle2.Offset(1, 0);
                g.DrawString(str, fnt, brush, rectangle2);
                rectangle2.Offset(-2, 0);
                g.DrawString(str, fnt, brush, rectangle2);
                rectangle2.Offset(1, -1);
                g.DrawString(str, fnt, brush, rectangle2);
                rectangle2.Offset(0, 2);
                g.DrawString(str, fnt, brush, rectangle2);
                rectangle2.Offset(1, 0);
                g.DrawString(str, fnt, brush, rectangle2);
                rectangle2.Offset(0, -2);
                g.DrawString(str, fnt, brush, rectangle2);
                rectangle2.Offset(-2, 0);
                g.DrawString(str, fnt, brush, rectangle2);
                rectangle2.Offset(0, 2);
                g.DrawString(str, fnt, brush, rectangle2);
                brush.Color = fSelected ? QTUtility.TabTextColor_Active : QTUtility.TabTextColor_Inactv;
                g.DrawString(str, fnt, brush, layoutRectangle);
            }
        }

        private void DrawTab(Graphics g, Rectangle itemRct, int index, QTabItemBase tabHot, bool fVisualStyle) {
            Rectangle rectangle2;
            Rectangle rctItem = rectangle2 = itemRct;
            QTabItemBase base2 = this.tabPages[index];
            bool bSelected = this.iSelectedIndex == index;
            bool fHot = base2 == tabHot;
            rectangle2.X += 2;
            if(bSelected) {
                rctItem.Width += 4;
            }
            else {
                rctItem.X += 2;
                rctItem.Y += 2;
                rctItem.Height -= 2;
                rectangle2.Y += 2;
            }
            this.DrawBackground(g, bSelected, fHot, rctItem, base2.Edge, fVisualStyle, index);
            int num = (rctItem.Height - 0x10) / 2;
            if(this.fDrawFolderImg && QTUtility.ImageListGlobal.Images.ContainsKey(base2.ImageKey)) {
                Rectangle rect = new Rectangle(rctItem.X + (bSelected ? 7 : 5), rctItem.Y + num, 0x10, 0x10);
                rectangle2.X += 0x18;
                rectangle2.Width -= 0x18;
                if((this.fNowMouseIsOnIcon && (this.iTabMouseOnButtonsIndex == index)) || (this.iTabIndexOfSubDirShown == index)) {
                    if(this.fSubDirShown && (this.iTabIndexOfSubDirShown == index)) {
                        rect.X++;
                        rect.Y++;
                    }
                    if(this.bmpFolIconBG == null) {
                        this.bmpFolIconBG = Resources_Image.imgFolIconBG;
                    }
                    g.DrawImage(this.bmpFolIconBG, new Rectangle(rect.X - 2, rect.Y - 2, rect.Width + 4, rect.Height + 4));
                }
                g.DrawImage(QTUtility.ImageListGlobal.Images[base2.ImageKey], rect);
                if(QTUtility.CheckConfig(14, 0x20)) {
                    string pathInitial = ((QTabItem)base2).PathInitial;
                    if(pathInitial.Length > 0) {
                        DrawDriveLetter(g, pathInitial, this.fntDriveLetter, rect, bSelected);
                    }
                }
            }
            else {
                rectangle2.X += 4;
                rectangle2.Width -= 4;
            }
            if(base2.TabLocked) {
                Rectangle rectangle4 = new Rectangle(rctItem.X + (bSelected ? 6 : 4), rctItem.Y + num, 9, 11);
                if(this.fDrawFolderImg) {
                    rectangle4.X += 9;
                    rectangle4.Y += 5;
                }
                else {
                    rectangle4.Y += 2;
                    rectangle2.X += 10;
                    rectangle2.Width -= 10;
                }
                if(this.bmpLocked == null) {
                    this.bmpLocked = Resources_Image.imgLocked;
                }
                g.DrawImage(this.bmpLocked, rectangle4);
            }
            bool flag3 = base2.Comment.Length > 0;
            if((this.fDrawCloseButton && !this.fCloseBtnOnHover) && !this.fNowShowCloseBtnAlt) {
                rectangle2.Width -= 15;
            }
            float num2 = flag3 ? ((base2.TitleTextSize.Width + base2.SubTitleTextSize.Width) + 4f) : (base2.TitleTextSize.Width + 2f);
            float num3 = Math.Max((float)((rectangle2.Height - base2.TitleTextSize.Height) / 2f), (float)0f);
            float num4 = (this.tabTextAlignment == StringAlignment.Center) ? Math.Max((float)((rectangle2.Width - num2) / 2f), (float)0f) : 0f;
            RectangleF rct = new RectangleF(rectangle2.X + num4, rectangle2.Y + num3, Math.Min((float)(base2.TitleTextSize.Width + 2f), (float)(rectangle2.Width - num4)), (float)rectangle2.Height);
            if(this.fDrawShadow) {
                DrawTextWithShadow(g, base2.Text, bSelected ? this.colorSet[0] : this.colorSet[1], bSelected ? this.colorSet[3] : this.colorSet[4], (bSelected && this.fActiveTxtBold) ? (base2.UnderLine ? this.fntBold_Underline : this.fntBold) : (base2.UnderLine ? this.fnt_Underline : this.Font), rct, this.sfTypoGraphic);
            }
            else {
                g.DrawString(base2.Text, (bSelected && this.fActiveTxtBold) ? (base2.UnderLine ? this.fntBold_Underline : this.fntBold) : (base2.UnderLine ? this.fnt_Underline : this.Font), bSelected ? this.brshActive : this.brshInactv, rct, this.sfTypoGraphic);
            }
            if(this.iFocusedTabIndex == index) {
                Rectangle rectangle = rctItem;
                rectangle.Inflate(-2, -1);
                rectangle.Y++;
                rectangle.Width--;
                ControlPaint.DrawFocusRectangle(g, rectangle);
            }
            if(flag3 && (rectangle2.Width > base2.TitleTextSize.Width)) {
                float num5 = Math.Max((float)((rectangle2.Height - base2.SubTitleTextSize.Height) / 2f), (float)0f);
                RectangleF ef2 = new RectangleF(rct.Right, rectangle2.Y + num5, Math.Min((float)(base2.SubTitleTextSize.Width + 2f), (float)(rectangle2.Width - ((base2.TitleTextSize.Width + num4) + 4f))), (float)rectangle2.Height);
                if(this.fDrawShadow) {
                    DrawTextWithShadow(g, (this.fAutoSubText ? "@ " : ": ") + base2.Comment, this.colorSet[1], this.colorSet[4], this.fntSubText, ef2, this.sfTypoGraphic);
                }
                else {
                    g.DrawString((this.fAutoSubText ? "@ " : ": ") + base2.Comment, this.fntSubText, this.brshInactv, ef2, this.sfTypoGraphic);
                }
            }
            if(this.fDrawCloseButton && (!this.fCloseBtnOnHover || fHot)) {
                Rectangle closeButtonRectangle = this.GetCloseButtonRectangle(base2.TabBounds, bSelected);
                if(this.fNowMouseIsOnCloseBtn && (this.iTabMouseOnButtonsIndex == index)) {
                    if(Control.MouseButtons == MouseButtons.Left) {
                        if(this.bmpCloseBtn_Pressed == null) {
                            this.bmpCloseBtn_Pressed = Resources_Image.imgCloseButton_Press;
                        }
                        g.DrawImage(this.bmpCloseBtn_Pressed, closeButtonRectangle);
                    }
                    else {
                        if(this.bmpCloseBtn_Hot == null) {
                            this.bmpCloseBtn_Hot = Resources_Image.imgCloseButton_Hot;
                        }
                        g.DrawImage(this.bmpCloseBtn_Hot, closeButtonRectangle);
                    }
                }
                else if(this.fNowShowCloseBtnAlt || this.fCloseBtnOnHover) {
                    if(this.bmpCloseBtn_ColdAlt == null) {
                        this.bmpCloseBtn_ColdAlt = Resources_Image.imgCloseButton_ColdAlt;
                    }
                    g.DrawImage(this.bmpCloseBtn_ColdAlt, closeButtonRectangle);
                }
                else {
                    if(this.bmpCloseBtn_Cold == null) {
                        this.bmpCloseBtn_Cold = Resources_Image.imgCloseButton_Cold;
                    }
                    g.DrawImage(this.bmpCloseBtn_Cold, closeButtonRectangle);
                }
            }
        }

        private static void DrawTextWithShadow(Graphics g, string txt, Color clrTxt, Color clrShdw, Font fnt, RectangleF rct, StringFormat sf) {
            RectangleF layoutRectangle = rct;
            RectangleF ef2 = rct;
            RectangleF ef3 = rct;
            layoutRectangle.Offset(1f, 1f);
            ef2.Offset(2f, 0f);
            ef3.Offset(1f, 2f);
            Color color = Color.FromArgb(0xc0, clrShdw);
            Color color2 = Color.FromArgb(0x80, clrShdw);
            using(SolidBrush brush = new SolidBrush(Color.FromArgb(0x40, clrShdw))) {
                g.DrawString(txt, fnt, brush, ef3, sf);
                brush.Color = color2;
                g.DrawString(txt, fnt, brush, ef2, sf);
                brush.Color = color;
                g.DrawString(txt, fnt, brush, layoutRectangle, sf);
                brush.Color = clrTxt;
                g.DrawString(txt, fnt, brush, rct, sf);
            }
        }

        public bool FocusNextTab(bool fBack, bool fEntered, bool fEnd) {
            if(this.tabPages.Count <= 0) {
                return false;
            }
            if(fEntered) {
                this.iFocusedTabIndex = fBack ? (this.tabPages.Count - 1) : 0;
                this.SetPseudoHotIndex(this.iFocusedTabIndex);
                return true;
            }
            if((fBack && (this.iFocusedTabIndex == 0)) || (!fBack && (this.iFocusedTabIndex == (this.tabPages.Count - 1)))) {
                this.iFocusedTabIndex = -1;
                return false;
            }
            if(fEnd) {
                this.iFocusedTabIndex = fBack ? 0 : (this.tabPages.Count - 1);
            }
            else {
                this.iFocusedTabIndex += fBack ? -1 : 1;
                if(this.iFocusedTabIndex < 0) {
                    this.iFocusedTabIndex = this.tabPages.Count - 1;
                }
            }
            this.SetPseudoHotIndex(this.iFocusedTabIndex);
            return true;
        }

        private Rectangle GetCloseButtonRectangle(Rectangle rctTab, bool fSelected) {
            int num = ((this.itemSize.Height - 15) / 2) + 1;
            if(!fSelected) {
                num += 2;
            }
            if((this.iMultipleType == 0) && this.fNeedToDrawUpDown) {
                rctTab.X += this.iScrollWidth;
            }
            return new Rectangle(rctTab.Right - 0x11, rctTab.Top + num, 15, 15);
        }

        public int GetFocusedTabIndex() {
            return this.iFocusedTabIndex;
        }

        private Rectangle GetFolderIconRectangle(Rectangle rctTab, bool fSelected) {
            int num = (rctTab.Height - 0x10) / 2;
            if(!fSelected) {
                num += 2;
            }
            if((this.iMultipleType == 0) && this.fNeedToDrawUpDown) {
                rctTab.X += this.iScrollWidth;
            }
            return new Rectangle(rctTab.X + (fSelected ? 5 : 3), (rctTab.Y + num) - 2, 20, 20);
        }

        private Rectangle GetItemRectangle(int index) {
            Rectangle tabBounds = this.tabPages[index].TabBounds;
            if(this.fNeedToDrawUpDown) {
                tabBounds.X += this.iScrollWidth;
            }
            return tabBounds;
        }

        private Rectangle GetItemRectWithInflation(int index) {
            Rectangle tabBounds = this.tabPages[index].TabBounds;
            if(index == this.iSelectedIndex) {
                tabBounds.Inflate(4, 0);
            }
            if(this.fNeedToDrawUpDown) {
                tabBounds.X += this.iScrollWidth;
            }
            return tabBounds;
        }

        public QTabItemBase GetTabMouseOn() {
            Point pt = base.PointToClient(Control.MousePosition);
            if(((this.upDown != null) && this.upDown.Visible) && this.upDown.Bounds.Contains(pt)) {
                return null;
            }
            QTabItemBase base2 = null;
            QTabItemBase base3 = null;
            for(int i = 0; i < this.tabPages.Count; i++) {
                if(this.GetItemRectWithInflation(i).Contains(pt)) {
                    if(base2 == null) {
                        base2 = this.tabPages[i];
                        if(this.iMultipleType == 0) {
                            return base2;
                        }
                    }
                    else {
                        base3 = this.tabPages[i];
                        break;
                    }
                }
            }
            if((base3 != null) && (base2.Row <= base3.Row)) {
                return base3;
            }
            return base2;
        }

        public QTabItemBase GetTabMouseOn(out int index) {
            Point pt = base.PointToClient(Control.MousePosition);
            QTabItemBase base2 = null;
            QTabItemBase base3 = null;
            int num = -1;
            int num2 = -1;
            for(int i = 0; i < this.tabPages.Count; i++) {
                if(this.GetItemRectWithInflation(i).Contains(pt)) {
                    if(base2 == null) {
                        base2 = this.tabPages[i];
                        num = i;
                        if(this.iMultipleType == 0) {
                            index = i;
                            return base2;
                        }
                    }
                    else {
                        base3 = this.tabPages[i];
                        num2 = i;
                        break;
                    }
                }
            }
            if(base3 != null) {
                if(base2.Row > base3.Row) {
                    index = num;
                    return base2;
                }
                index = num2;
                return base3;
            }
            index = num;
            return base2;
        }

        public Rectangle GetTabRect(QTabItemBase tab) {
            Rectangle tabBounds = tab.TabBounds;
            if(this.fNeedToDrawUpDown) {
                tabBounds.X += this.iScrollWidth;
            }
            return tabBounds;
        }

        public Rectangle GetTabRect(int index, bool fInflation) {
            if((index <= -1) || (index >= this.tabPages.Count)) {
                throw new ArgumentOutOfRangeException("index," + index, "index is out of range.");
            }
            if(fInflation) {
                return this.GetItemRectWithInflation(index);
            }
            return this.GetItemRectangle(index);
        }

        private bool HitTestOnButtons(Rectangle rctTab, Point pntClient, bool fCloseButton, bool fSelected) {
            if(fCloseButton) {
                return this.GetCloseButtonRectangle(rctTab, fSelected).Contains(pntClient);
            }
            return this.GetFolderIconRectangle(rctTab, fSelected).Contains(pntClient);
        }

        private void InitializeRenderer() {
            this.vsr_LPressed = new VisualStyleRenderer(VisualStyleElement.Tab.TopTabItemLeftEdge.Pressed);
            this.vsr_RPressed = new VisualStyleRenderer(VisualStyleElement.Tab.TopTabItemRightEdge.Pressed);
            this.vsr_MPressed = new VisualStyleRenderer(VisualStyleElement.Tab.TopTabItem.Pressed);
            this.vsr_LNormal = new VisualStyleRenderer(VisualStyleElement.Tab.TopTabItemLeftEdge.Normal);
            this.vsr_RNormal = new VisualStyleRenderer(VisualStyleElement.Tab.TopTabItemRightEdge.Normal);
            this.vsr_MNormal = new VisualStyleRenderer(VisualStyleElement.Tab.TopTabItem.Normal);
            this.vsr_LHot = new VisualStyleRenderer(VisualStyleElement.Tab.TopTabItem.Hot);
            this.vsr_RHot = new VisualStyleRenderer(VisualStyleElement.Tab.TopTabItemRightEdge.Hot);
            this.vsr_MHot = new VisualStyleRenderer(VisualStyleElement.Tab.TopTabItem.Hot);
        }

        private void InvalidateTabsOnMouseMove(QTabItemBase tabPage, int index, Point pnt) {
            this.iTabMouseOnButtonsIndex = index;
            if(tabPage != this.hotTab) {
                this.hotTab = tabPage;
                if((tabPage != null) && !tabPage.TabLocked) {
                    bool fSelected = index == this.iSelectedIndex;
                    if(this.fDrawCloseButton) {
                        this.fNowMouseIsOnCloseBtn = this.HitTestOnButtons(tabPage.TabBounds, pnt, true, fSelected);
                    }
                    if(this.fDrawFolderImg && this.fShowSubDirTip) {
                        this.fNowMouseIsOnIcon = this.HitTestOnButtons(tabPage.TabBounds, pnt, false, fSelected);
                    }
                }
                else {
                    this.fNowMouseIsOnCloseBtn = false;
                    this.fNowMouseIsOnIcon = false;
                }
                PInvoke.InvalidateRect(base.Handle, IntPtr.Zero, true);
            }
            else if(tabPage != null) {
                bool flag2 = index == this.iSelectedIndex;
                bool flag3 = false;
                if(this.fDrawCloseButton) {
                    bool flag4 = this.HitTestOnButtons(tabPage.TabBounds, pnt, true, flag2);
                    if(this.fNowMouseIsOnCloseBtn ^ flag4) {
                        this.fNowMouseIsOnCloseBtn = flag4 && !tabPage.TabLocked;
                        flag3 = true;
                    }
                }
                if(this.fDrawFolderImg && this.fShowSubDirTip) {
                    bool flag5 = this.HitTestOnButtons(tabPage.TabBounds, pnt, false, flag2);
                    if(this.fNowMouseIsOnIcon ^ flag5) {
                        this.fNowMouseIsOnIcon = flag5;
                        flag3 = true;
                    }
                }
                if(flag3) {
                    PInvoke.InvalidateRect(base.Handle, IntPtr.Zero, true);
                }
            }
        }

        protected override void OnLostFocus(EventArgs e) {
            this.iFocusedTabIndex = -1;
            if(this.iPseudoHotIndex != -1) {
                this.SetPseudoHotIndex(-1);
            }
            base.OnLostFocus(e);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e) {
            if(!this.fSuppressDoubleClick) {
                int num;
                QTabItemBase tabMouseOn = this.GetTabMouseOn(out num);
                if(((!this.fDrawCloseButton || (tabMouseOn == null)) || !this.HitTestOnButtons(tabMouseOn.TabBounds, e.Location, true, num == this.iSelectedIndex)) && ((!this.fDrawFolderImg || !this.fShowSubDirTip) || ((tabMouseOn == null) || !this.HitTestOnButtons(tabMouseOn.TabBounds, e.Location, false, num == this.iSelectedIndex)))) {
                    base.OnMouseDoubleClick(e);
                    this.fSuppressMouseUp = true;
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            int num;
            QTabItemBase tabMouseOn = this.GetTabMouseOn(out num);
            if(tabMouseOn != null) {
                bool cancel = e.Button == MouseButtons.Right;
                if((!cancel && this.fDrawCloseButton) && this.HitTestOnButtons(tabMouseOn.TabBounds, e.Location, true, num == this.iSelectedIndex)) {
                    PInvoke.InvalidateRect(base.Handle, IntPtr.Zero, true);
                    return;
                }
                if((this.fNowMouseIsOnIcon && this.HitTestOnButtons(tabMouseOn.TabBounds, e.Location, false, num == this.iSelectedIndex)) && (this.TabIconMouseDown != null)) {
                    if((e.Button == MouseButtons.Left) || cancel) {
                        this.iTabIndexOfSubDirShown = num;
                        int tabPageIndex = 0;
                        if((this.iMultipleType == 0) && this.fNeedToDrawUpDown) {
                            tabPageIndex = this.iScrollWidth;
                        }
                        this.TabIconMouseDown(this, new QTabCancelEventArgs(tabMouseOn, tabPageIndex, cancel, TabControlAction.Selecting));
                        PInvoke.InvalidateRect(base.Handle, IntPtr.Zero, true);
                    }
                    return;
                }
                if(((e.Button == MouseButtons.Left) && ((Control.ModifierKeys & Keys.Control) != Keys.Control)) && this.SelectTab(tabMouseOn)) {
                    this.fSuppressDoubleClick = true;
                    this.timerSuppressDoubleClick.Enabled = true;
                }
            }
            this.draggingTab = tabMouseOn;
            base.OnMouseDown(e);
        }

        protected override void OnMouseLeave(EventArgs e) {
            this.iToolTipIndex = -1;
            if(this.fShowToolTips && (this.toolTip != null)) {
                this.toolTip.Active = false;
            }
            this.iPointedChanged_LastRaisedIndex = -2;
            if((this.PointedTabChanged != null) && (this.hotTab != null)) {
                this.PointedTabChanged(null, new QTabCancelEventArgs(null, -1, false, TabControlAction.Deselecting));
            }
            this.hotTab = null;
            this.fNowMouseIsOnCloseBtn = this.fNowMouseIsOnIcon = false;
            PInvoke.InvalidateRect(base.Handle, IntPtr.Zero, true);
            base.OnMouseLeave(e);
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            int num;
            if(((e.Button == MouseButtons.Right) && !base.Parent.RectangleToScreen(base.Bounds).Contains(Control.MousePosition)) && ((this.ItemDrag != null) && (this.draggingTab != null))) {
                this.ItemDrag(this, new ItemDragEventArgs(e.Button, this.draggingTab));
            }
            QTabItemBase tabMouseOn = this.GetTabMouseOn(out num);
            this.InvalidateTabsOnMouseMove(tabMouseOn, num, e.Location);
            if((this.PointedTabChanged != null) && (num != this.iPointedChanged_LastRaisedIndex)) {
                if(tabMouseOn != null) {
                    this.iPointedChanged_LastRaisedIndex = num;
                    this.PointedTabChanged(this, new QTabCancelEventArgs(tabMouseOn, num, false, TabControlAction.Selecting));
                }
                else if(this.iPointedChanged_LastRaisedIndex != -2) {
                    this.iPointedChanged_LastRaisedIndex = -1;
                    this.PointedTabChanged(this, new QTabCancelEventArgs(null, -1, false, TabControlAction.Deselecting));
                }
            }
            if(this.fShowToolTips) {
                if(tabMouseOn != null) {
                    if(((this.iToolTipIndex != num) && base.IsHandleCreated) && !string.IsNullOrEmpty(tabMouseOn.ToolTipText)) {
                        if(this.toolTip == null) {
                            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
                            this.toolTip.ShowAlways = true;
                        }
                        else {
                            this.toolTip.Active = false;
                        }
                        string toolTipText = tabMouseOn.ToolTipText;
                        string str2 = ((QTabItem)tabMouseOn).TooltipText2;
                        if((str2 != null) && (str2.Length > 0)) {
                            toolTipText = toolTipText + "\r\n" + str2;
                        }
                        this.iToolTipIndex = num;
                        this.toolTip.SetToolTip(this, toolTipText);
                        this.toolTip.Active = true;
                    }
                }
                else {
                    this.iToolTipIndex = -1;
                    if(this.toolTip != null) {
                        this.toolTip.Active = false;
                    }
                }
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e) {
            this.draggingTab = null;
            if(this.fSuppressMouseUp) {
                this.fSuppressMouseUp = false;
                base.OnMouseUp(e);
            }
            else {
                int num;
                QTabItemBase tabMouseOn = this.GetTabMouseOn(out num);
                if(((this.fDrawCloseButton && (e.Button != MouseButtons.Right)) && ((this.CloseButtonClicked != null) && (tabMouseOn != null))) && (!tabMouseOn.TabLocked && this.HitTestOnButtons(tabMouseOn.TabBounds, e.Location, true, num == this.iSelectedIndex))) {
                    if(e.Button == MouseButtons.Left) {
                        this.iTabMouseOnButtonsIndex = -1;
                        QTabCancelEventArgs args = new QTabCancelEventArgs(tabMouseOn, num, false, TabControlAction.Deselected);
                        this.CloseButtonClicked(this, args);
                        if(args.Cancel) {
                            PInvoke.InvalidateRect(base.Handle, IntPtr.Zero, true);
                        }
                    }
                }
                else {
                    base.OnMouseUp(e);
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e) {
            this.fOncePainted = true;
            if(this.iMultipleType != 0) {
                this.OnPaint_MultipleRow(e);
            }
            else {
                this.fNeedToDrawUpDown = this.CalculateItemRectangle();
                try {
                    QTabItemBase tabMouseOn = this.GetTabMouseOn();
                    bool fVisualStyle = !this.fForceClassic && VisualStyleRenderer.IsSupported;
                    if(fVisualStyle && (this.vsr_LPressed == null)) {
                        this.InitializeRenderer();
                    }
                    for(int i = 0; i < this.tabPages.Count; i++) {
                        if(i != this.iSelectedIndex) {
                            this.DrawTab(e.Graphics, this.GetItemRectangle(i), i, tabMouseOn, fVisualStyle);
                        }
                    }
                    if((this.tabPages.Count > 0) && (this.iSelectedIndex > -1)) {
                        this.DrawTab(e.Graphics, this.GetItemRectangle(this.iSelectedIndex), this.iSelectedIndex, tabMouseOn, fVisualStyle);
                    }
                    if((this.fNeedToDrawUpDown && (this.iSelectedIndex < this.tabPages.Count)) && ((this.iSelectedIndex > -1) && (this.GetItemRectangle(this.iSelectedIndex).X != 0))) {
                        e.Graphics.FillRectangle(SystemBrushes.Control, new Rectangle(0, 0, 2, e.ClipRectangle.Height));
                    }
                    this.ShowUpDown(this.fNeedToDrawUpDown);
                }
                catch(Exception exception) {
                    QTUtility2.MakeErrorLog(exception, null);
                }
            }
        }

        private void OnPaint_MultipleRow(PaintEventArgs e) {
            this.CalculateItemRectangle_MultiRows();
            try {
                QTabItemBase tabMouseOn = this.GetTabMouseOn();
                bool fVisualStyle = !this.fForceClassic && VisualStyleRenderer.IsSupported;
                if(fVisualStyle && (this.vsr_LPressed == null)) {
                    this.InitializeRenderer();
                }
                bool flag2 = false;
                for(int i = 0; i < (this.iCurrentRow + 1); i++) {
                    for(int j = 0; j < this.tabPages.Count; j++) {
                        QTabItemBase base3 = this.tabPages[j];
                        if(base3.Row == i) {
                            if(j != this.iSelectedIndex) {
                                this.DrawTab(e.Graphics, base3.TabBounds, j, tabMouseOn, fVisualStyle);
                            }
                            else {
                                flag2 = true;
                            }
                        }
                    }
                    if(flag2) {
                        this.DrawTab(e.Graphics, this.tabPages[this.iSelectedIndex].TabBounds, this.iSelectedIndex, tabMouseOn, fVisualStyle);
                        flag2 = false;
                    }
                }
                this.ShowUpDown(false);
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception, null);
            }
        }

        private void OnTabPageAdded(QTabItemBase tabPage, int index) {
            if(index == 0) {
                this.selectedTabPage = tabPage;
            }
            if(this.TabCountChanged != null) {
                this.TabCountChanged(this, new QTabCancelEventArgs(tabPage, index, false, TabControlAction.Selected));
            }
        }

        private void OnTabPageInserted(QTabItemBase tabPage, int index) {
            if(index <= this.iSelectedIndex) {
                this.iSelectedIndex++;
            }
            if(this.TabCountChanged != null) {
                this.TabCountChanged(this, new QTabCancelEventArgs(tabPage, index, false, TabControlAction.Selected));
            }
        }

        private void OnTabPageRemoved(QTabItemBase tabPage, int index) {
            if(!base.Disposing && (index != -1)) {
                if(index == this.iSelectedIndex) {
                    this.iSelectedIndex = -1;
                }
                else if(index < this.iSelectedIndex) {
                    this.iSelectedIndex--;
                }
                if(this.TabCountChanged != null) {
                    this.TabCountChanged(this, new QTabCancelEventArgs(tabPage, index, false, TabControlAction.Deselected));
                }
            }
        }

        private void OnUpDownClicked(bool dir, bool lockPaint) {
            int num = base.Width - 0x24;
            if((!dir || ((this.tabPages[this.tabPages.Count - 1].TabBounds.Right + this.iScrollWidth) >= num)) && (dir || ((this.tabPages[0].TabBounds.Left + this.iScrollWidth) != 0))) {
                this.iScrollClickedCount += dir ? 1 : -1;
                if(this.iScrollClickedCount > (this.tabPages.Count - 1)) {
                    this.iScrollClickedCount = this.tabPages.Count - 1;
                }
                else if(this.iScrollClickedCount < 0) {
                    this.iScrollClickedCount = 0;
                }
                else {
                    this.iScrollWidth = -this.tabPages[this.iScrollClickedCount].TabBounds.X;
                    if(!lockPaint) {
                        base.Invalidate();
                    }
                }
            }
        }

        public bool PerformFocusedFolderIconClick(bool fParent) {
            if(((this.TabIconMouseDown == null) || !this.Focused) || ((-1 >= this.iFocusedTabIndex) || (this.iFocusedTabIndex >= this.tabPages.Count))) {
                return false;
            }
            this.iTabIndexOfSubDirShown = this.iFocusedTabIndex;
            QTabItemBase tabPage = this.tabPages[this.iFocusedTabIndex];
            int tabPageIndex = 0;
            if((this.iMultipleType == 0) && this.fNeedToDrawUpDown) {
                tabPageIndex = this.iScrollWidth;
            }
            this.TabIconMouseDown(this, new QTabCancelEventArgs(tabPage, tabPageIndex, fParent, TabControlAction.Selecting));
            PInvoke.InvalidateRect(base.Handle, IntPtr.Zero, true);
            return true;
        }

        public override void Refresh() {
            if(!this.fRedrawSuspended) {
                base.Refresh();
            }
        }

        public void RefreshFolderImage() {
            this.iTabMouseOnButtonsIndex = -1;
            this.fNowMouseIsOnIcon = false;
            PInvoke.InvalidateRect(base.Handle, IntPtr.Zero, true);
        }

        public void RefreshOptions(bool fInit) {
            if(fInit) {
                if(QTUtility.CheckConfig(7, 0x20)) {
                    this.iMultipleType = 1;
                }
                else if(QTUtility.CheckConfig(7, 0x10)) {
                    this.iMultipleType = 2;
                }
                this.fDrawFolderImg = QTUtility.CheckConfig(7, 2);
            }
            else {
                this.colorSet = new Color[] { QTUtility.TabTextColor_Active, QTUtility.TabTextColor_Inactv, QTUtility.TabHiliteColor, QTUtility.TabTextColor_ActivShdw, QTUtility.TabTextColor_InAtvShdw };
                this.brshActive.Color = this.colorSet[0];
                this.brshInactv.Color = this.colorSet[1];
            }
            this.fShowToolTips = QTUtility.CheckConfig(0, 8);
            if(QTUtility.CheckConfig(0, 4)) {
                this.sizeMode = TabSizeMode.Fixed;
                this.fLimitSize = false;
            }
            else {
                this.sizeMode = TabSizeMode.Normal;
                this.fLimitSize = QTUtility.CheckConfig(5, 4);
            }
            this.itemSize = new Size(QTUtility.TabWidth, QTUtility.TabHeight);
            if((QTUtility.MaxTabWidth >= QTUtility.MinTabWidth) && (QTUtility.MinTabWidth > 9)) {
                this.maxTabWidth = QTUtility.MaxTabWidth;
                this.minTabWidth = QTUtility.MinTabWidth;
            }
            this.fActiveTxtBold = QTUtility.CheckConfig(5, 0x10);
            this.fForceClassic = QTUtility.CheckConfig(5, 8);
            this.SetFont(QTUtility.TabFont);
            this.sizingMargin = QTUtility.TabImageSizingMargin + new Padding(0, 0, 1, 1);
            if(QTUtility.CheckConfig(5, 8) && (QTUtility.Path_TabImage.Length > 0)) {
                this.SetTabImages(QTTabBarClass.CreateTabImage());
            }
            else {
                this.SetTabImages(null);
            }
            this.tabTextAlignment = QTUtility.CheckConfig(13, 0x40) ? StringAlignment.Center : StringAlignment.Near;
            this.fAutoSubText = !QTUtility.CheckConfig(10, 0x20);
            this.fDrawShadow = QTUtility.CheckConfig(13, 2);
            this.fDrawCloseButton = QTUtility.CheckConfig(11, 0x10) && !QTUtility.CheckConfig(11, 4);
            this.fCloseBtnOnHover = QTUtility.CheckConfig(11, 1);
            this.fShowSubDirTip = QTUtility.CheckConfig(11, 8);
            if(!fInit && (this.fDrawFolderImg != QTUtility.CheckConfig(7, 2))) {
                this.fDrawFolderImg = QTUtility.CheckConfig(7, 2);
                if(this.fDrawFolderImg) {
                    foreach(QTabItemBase base2 in this.TabPages) {
                        base2.ImageKey = base2.ImageKey;
                    }
                }
                else {
                    this.fNowMouseIsOnIcon = false;
                }
            }
        }

        public bool SelectFocusedTab() {
            if((this.Focused && (-1 < this.iFocusedTabIndex)) && (this.iFocusedTabIndex < this.tabPages.Count)) {
                this.SelectedIndex = this.iFocusedTabIndex;
                return true;
            }
            return false;
        }

        public bool SelectTab(QTabItemBase tabPage) {
            int index = this.tabPages.IndexOf(tabPage);
            if(index == -1) {
                throw new ArgumentException("arg was not found.");
            }
            return (((index != -1) && (this.selectedTabPage != tabPage)) && this.ChangeSelection(tabPage, index));
        }

        public void SelectTab(int index) {
            if((index <= -1) || (index >= this.tabPages.Count)) {
                throw new ArgumentOutOfRangeException("index," + index, "index is out of range.");
            }
            QTabItemBase tabToSelect = this.tabPages[index];
            if(this.selectedTabPage != tabToSelect) {
                this.ChangeSelection(tabToSelect, index);
            }
            else if(this.iSelectedIndex != index) {
                this.iSelectedIndex = index;
            }
        }

        public void SelectTabDirectly(QTabItemBase tabPage) {
            int index = this.tabPages.IndexOf(tabPage);
            this.selectedTabPage = tabPage;
            this.SelectedIndex = index;
        }

        public void SetContextMenuState(bool fShow) {
            this.fNowTabContextMenuStripShowing = fShow;
        }

        private void SetFont(Font fnt) {
            this.Font = fnt;
            if(this.fntBold != null) {
                this.fntBold.Dispose();
            }
            this.fntBold = new Font(this.Font, FontStyle.Bold);
            if(this.fnt_Underline != null) {
                this.fnt_Underline.Dispose();
            }
            this.fnt_Underline = new Font(this.Font, FontStyle.Underline);
            if(this.fntBold_Underline != null) {
                this.fntBold_Underline.Dispose();
            }
            this.fntBold_Underline = new Font(this.Font, FontStyle.Underline | FontStyle.Bold);
            if(this.fntSubText != null) {
                this.fntSubText.Dispose();
            }
            float sizeInPoints = this.Font.SizeInPoints;
            this.fntSubText = new Font(this.Font.FontFamily, (sizeInPoints > 8.25f) ? (sizeInPoints - 0.75f) : sizeInPoints);
            if(this.fntDriveLetter != null) {
                this.fntDriveLetter.Dispose();
            }
            this.fntDriveLetter = new Font(this.Font.FontFamily, 8.25f);
            QTabItemBase.TabFont = this.Font;
        }

        public void SetPseudoHotIndex(int index) {
            int iPseudoHotIndex = this.iPseudoHotIndex;
            this.iPseudoHotIndex = index;
            if((iPseudoHotIndex > -1) && (iPseudoHotIndex < this.TabCount)) {
                base.Invalidate(this.GetTabRect(iPseudoHotIndex, true));
            }
            if((this.iPseudoHotIndex > -1) && (this.iPseudoHotIndex < this.TabCount)) {
                base.Invalidate(this.GetTabRect(this.iPseudoHotIndex, true));
            }
            base.Update();
        }

        public void SetRedraw(bool bRedraw) {
            if(bRedraw && this.fRedrawSuspended) {
                base.Refresh();
            }
            this.fRedrawSuspended = !bRedraw;
        }

        public void SetSubDirTipShown(bool fShown) {
            if(!fShown) {
                this.iTabIndexOfSubDirShown = -1;
            }
            this.fSubDirShown = fShown;
        }

        private void SetTabImages(Bitmap[] bmps) {
            if((bmps != null) && (bmps.Length == 3)) {
                if(this.tabImages == null) {
                    this.tabImages = bmps;
                }
                else if((this.tabImages[0] != null) && (this.tabImages[1] != null)) {
                    Bitmap bitmap = this.tabImages[0];
                    Bitmap bitmap2 = this.tabImages[1];
                    Bitmap bitmap3 = this.tabImages[2];
                    this.tabImages[0] = bmps[0];
                    this.tabImages[1] = bmps[1];
                    this.tabImages[2] = bmps[2];
                    bitmap.Dispose();
                    bitmap2.Dispose();
                    bitmap3.Dispose();
                }
                else {
                    this.tabImages = bmps;
                }
            }
            else if(((this.tabImages != null) && (this.tabImages[0] != null)) && ((this.tabImages[1] != null) && (this.tabImages[2] != null))) {
                Bitmap bitmap4 = this.tabImages[0];
                Bitmap bitmap5 = this.tabImages[1];
                Bitmap bitmap6 = this.tabImages[2];
                this.tabImages = null;
                bitmap4.Dispose();
                bitmap5.Dispose();
                bitmap6.Dispose();
            }
        }

        public int SetTabRowType(int iType) {
            this.iMultipleType = iType;
            if(iType != 0) {
                this.fNeedToDrawUpDown = false;
                return (this.iCurrentRow + 1);
            }
            return 1;
        }

        public void ShowCloseButton(bool fShow) {
            this.fDrawCloseButton = this.fNowShowCloseBtnAlt = fShow;
            base.Invalidate();
        }

        private void ShowUpDown(bool fShow) {
            if(fShow) {
                if(this.upDown == null) {
                    this.upDown = new UpDown();
                    this.upDown.Anchor = AnchorStyles.Right;
                    this.upDown.ValueChanged += new QEventHandler(this.upDown_ValueChanged);
                    base.Controls.Add(this.upDown);
                }
                this.upDown.Location = new Point(base.Width - 0x24, 0);
                this.upDown.Visible = true;
                this.upDown.BringToFront();
            }
            else if((this.upDown != null) && this.upDown.Visible) {
                this.upDown.Visible = false;
            }
        }

        private void timerSuppressDoubleClick_Tick(object sender, EventArgs e) {
            this.timerSuppressDoubleClick.Enabled = false;
            this.fSuppressDoubleClick = false;
        }

        private void upDown_ValueChanged(object sender, QEventArgs e) {
            this.OnUpDownClicked(e.Direction == ArrowDirection.Right, false);
        }

        protected override void WndProc(ref Message m) {
            QTabItemBase tabMouseOn;
            int num;
            int msg = m.Msg;
            switch(msg) {
                case WM.SETCURSOR:
                    if(this.fSubDirShown || this.fNowTabContextMenuStripShowing) {
                        uint num4 = ((uint)((long)m.LParam)) & 0xffff;
                        uint num5 = (((uint)((long)m.LParam)) >> 0x10) & 0xffff;
                        if((num4 == 1) && (num5 == 0x200)) {
                            tabMouseOn = this.GetTabMouseOn(out num);
                            this.InvalidateTabsOnMouseMove(tabMouseOn, num, base.PointToClient(Control.MousePosition));
                            m.Result = (IntPtr)1;
                            return;
                        }
                    }
                    break;

                case WM.MOUSEACTIVATE: {
                        if(!this.fSubDirShown || (this.TabIconMouseDown == null)) {
                            break;
                        }
                        int num2 = (((int)((long)m.LParam)) >> 0x10) & 0xffff;
                        if(num2 == 0x207) {
                            break;
                        }
                        bool cancel = num2 == 0x204;
                        m.Result = (IntPtr)4;
                        tabMouseOn = this.GetTabMouseOn(out num);
                        if(((tabMouseOn == null) || (num == this.iTabIndexOfSubDirShown)) || !this.HitTestOnButtons(tabMouseOn.TabBounds, base.PointToClient(Control.MousePosition), false, num == this.iSelectedIndex)) {
                            this.TabIconMouseDown(this, new QTabCancelEventArgs(null, -1, false, TabControlAction.Deselected));
                            return;
                        }
                        int tabPageIndex = 0;
                        if((this.iMultipleType == 0) && this.fNeedToDrawUpDown) {
                            tabPageIndex = this.iScrollWidth;
                        }
                        this.TabIconMouseDown(this, new QTabCancelEventArgs(tabMouseOn, tabPageIndex, cancel, TabControlAction.Selecting));
                        if(this.fSubDirShown) {
                            this.iTabIndexOfSubDirShown = num;
                        }
                        else {
                            this.iTabIndexOfSubDirShown = -1;
                        }
                        this.fNowMouseIsOnIcon = true;
                        this.iTabMouseOnButtonsIndex = num;
                        PInvoke.InvalidateRect(base.Handle, IntPtr.Zero, true);
                        return;
                    }

                case WM.ERASEBKGND:
                    if(!this.fRedrawSuspended) {
                        break;
                    }
                    m.Result = (IntPtr)1;
                    return;

                default:
                    if(msg != WM.CONTEXTMENU) {
                        break;
                    }
                    if((QTUtility2.GET_X_LPARAM(m.LParam) != -1) || (QTUtility2.GET_Y_LPARAM(m.LParam) != -1)) {
                        tabMouseOn = this.GetTabMouseOn(out num);
                        if(tabMouseOn == null) {
                            PInvoke.SendMessage(base.Parent.Handle, 0x7b, m.WParam, m.LParam);
                            return;
                        }
                        if(!this.fShowSubDirTip || !this.HitTestOnButtons(tabMouseOn.TabBounds, base.PointToClient(Control.MousePosition), false, num == this.iSelectedIndex)) {
                            break;
                        }
                    }
                    return;
            }
            base.WndProc(ref m);
        }

        public bool AutoSubText {
            get {
                return this.fAutoSubText;
            }
        }

        protected override bool CanEnableIme {
            get {
                return false;
            }
        }

        public bool DrawFolderImage {
            get {
                return this.fDrawFolderImg;
            }
        }

        public bool EnableCloseButton {
            get {
                return this.fDrawCloseButton;
            }
            set {
                this.fDrawCloseButton = value;
            }
        }

        public bool OncePainted {
            get {
                return this.fOncePainted;
            }
        }

        public int SelectedIndex {
            get {
                return this.iSelectedIndex;
            }
            set {
                this.SelectTab(value);
            }
        }

        public QTabItemBase SelectedTab {
            get {
                return this.tabPages[this.iSelectedIndex];
            }
        }

        public bool TabCloseButtonOnAlt {
            get {
                return this.fNowShowCloseBtnAlt;
            }
        }

        public bool TabCloseButtonOnHover {
            get {
                return this.fCloseBtnOnHover;
            }
        }

        public int TabCount {
            get {
                return this.tabPages.Count;
            }
        }

        public int TabOffset {
            get {
                if((this.iMultipleType == 0) && this.fNeedToDrawUpDown) {
                    return this.iScrollWidth;
                }
                return 0;
            }
        }

        public QTabCollection TabPages {
            get {
                return this.tabPages;
            }
        }

        public sealed class QTabCollection : List<QTabItemBase> {
            private QTabControl Owner;

            public QTabCollection(QTabControl owner) {
                this.Owner = owner;
            }

            public void Add(QTabItemBase tabPage) {
                base.Add(tabPage);
                this.Owner.OnTabPageAdded(tabPage, base.Count - 1);
                this.Owner.Refresh();
            }

            public IEnumerator<QTabItemBase> GetEnumerator() {
                //<GetEnumerator>d__0 d__ = new <GetEnumerator>d__0(0);
                //d__.<>4__this = this;
                //return d__;

                QTabItemBase[] tmpArr = this.ToArray();
                for(int i = 0; i < tmpArr.Length; ++i) {
                    if(this.Contains(tmpArr[i])) {
                        yield return tmpArr[i];
                    }
                }
            }

            public void Insert(int index, QTabItemBase tabPage) {
                base.Insert(index, tabPage);
                this.Owner.OnTabPageInserted(tabPage, index);
                this.Owner.Refresh();
            }

            public bool Remove(QTabItemBase tabPage) {
                int index = base.IndexOf(tabPage);
                this.Owner.OnTabPageRemoved(tabPage, index);
                bool flag = base.Remove(tabPage);
                this.Owner.Refresh();
                return flag;
            }

            public void Swap(int indexSource, int indexDestination) {
                int selectedIndex = this.Owner.SelectedIndex;
                int num2 = (indexSource > indexDestination) ? indexSource : indexDestination;
                int num3 = (indexSource > indexDestination) ? indexDestination : indexSource;
                QTabItemBase item = base[indexSource];
                base.Remove(item);
                base.Insert(indexDestination, item);
                if((num2 >= selectedIndex) && (selectedIndex >= num3)) {
                    if(num2 == selectedIndex) {
                        if(num2 == indexSource) {
                            this.Owner.SelectedIndex = indexDestination;
                        }
                        else {
                            this.Owner.SelectedIndex--;
                        }
                    }
                    else if((num3 < selectedIndex) && (selectedIndex < num2)) {
                        if(num2 == indexSource) {
                            this.Owner.SelectedIndex++;
                        }
                        else {
                            this.Owner.SelectedIndex--;
                        }
                    }
                    else if(num3 == selectedIndex) {
                        if(num2 == indexSource) {
                            this.Owner.SelectedIndex++;
                        }
                        else {
                            this.Owner.SelectedIndex = indexDestination;
                        }
                    }
                }
                this.Owner.Refresh();
            }

#if false
      [CompilerGenerated]
      private sealed class <GetEnumerator>d__0 : IEnumerator<QTabItemBase>, IEnumerator, IDisposable
      {
        private int <>1__state;
        private QTabItemBase <>2__current;
        public QTabControl.QTabCollection <>4__this;
        public int <i>5__2;
        public QTabItemBase[] <tmpArr>5__1;
        
        [DebuggerHidden]
        public <GetEnumerator>d__0(int <>1__state)
        {
          this.<>1__state = <>1__state;
        }
        
        private bool MoveNext()
        {
          switch (this.<>1__state)
          {
            case 0:
              this.<>1__state = -1;
              this.<tmpArr>5__1 = this.<>4__this.ToArray();
              this.<i>5__2 = 0;
              goto Label_0083;
            
            case 1:
              this.<>1__state = -1;
              goto Label_0075;
            
            default:
              goto Label_0093;
          }
        Label_0075:
          this.<i>5__2++;
        Label_0083:
          if (this.<i>5__2 < this.<tmpArr>5__1.Length)
          {
            if (this.<>4__this.Contains(this.<tmpArr>5__1[this.<i>5__2]))
            {
              this.<>2__current = this.<tmpArr>5__1[this.<i>5__2];
              this.<>1__state = 1;
              return true;
            }
            goto Label_0075;
          }
        Label_0093:
          return false;
        }
        
        [DebuggerHidden]
        void IEnumerator.Reset()
        {
          throw new NotSupportedException();
        }
        
        void IDisposable.Dispose()
        {
        }
        
        QTabItemBase IEnumerator<QTabItemBase>.Current
        {
          [DebuggerHidden]
          get
          {
            return this.<>2__current;
          }
        }
        
        object IEnumerator.Current
        {
          [DebuggerHidden]
          get
          {
            return this.<>2__current;
          }
        }
      }
#endif

        }
    }
}
