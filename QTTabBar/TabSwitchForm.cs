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
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;

    internal sealed class TabSwitchForm : Form {
        private IContainer components;
        private Dictionary<int, Rectangle> dicItemRcts = new Dictionary<int, Rectangle>();
        private bool fCompositionEnabled;
        private bool fDoubleBufferRequired;
        private bool fIsShown;
        private Font fntMenu;
        private Font fntMenuBold;
        private int iHoveredIndex = -1;
        private int initialSelectedIndex;
        private List<PathData> lstPaths = new List<PathData>();
        private const int MAXITEM = 11;
        private static int menuHeight;
        private int selectedIndex;
        private System.Windows.Forms.ToolTip toolTipSwitcher;

        public event ItemCheckEventHandler Switched;

        public TabSwitchForm() {
            this.InitializeComponent();
            this.fntMenu = SystemFonts.MenuFont;
            this.fntMenuBold = new Font(this.fntMenu, FontStyle.Bold);
            this.SetCompositionState();
        }

        protected override void Dispose(bool disposing) {
            if(this.fntMenu != null) {
                this.fntMenu.Dispose();
                this.fntMenu = null;
            }
            if(this.fntMenuBold != null) {
                this.fntMenuBold.Dispose();
                this.fntMenuBold = null;
            }
            if(disposing && (this.components != null)) {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void DrawItems(Graphics g) {
            this.dicItemRcts.Clear();
            int x = this.fCompositionEnabled ? 2 : 6;
            int num2 = this.fCompositionEnabled ? 0 : (QTUtility.IsVista ? 2 : 8);
            int y = QTUtility.IsVista ? 4 : 8;
            int num4 = TabSwitchForm.menuHeight + (QTUtility.IsVista ? 11 : 0x12);
            int width = base.ClientSize.Width;
            int menuHeight = TabSwitchForm.menuHeight;
            int num7 = Math.Max((menuHeight - 0x10) / 2, 0);
            int count = this.lstPaths.Count;
            int num9 = 5;
            bool flag = count > 11;
            int num10 = flag ? num9 : ((count - 1) / 2);
            int num11 = flag ? num9 : ((count - 1) - num10);
            int num12 = this.selectedIndex - num10;
            if(num12 < 0) {
                num12 += count;
            }
            this.DrawText(g, GetTitleText(this.lstPaths[this.selectedIndex]), new Rectangle(num2, y, (width - 4) - num2, menuHeight), false, false, StringAlignment.Center);
            for(int i = 0; i < num10; i++) {
                int num14 = i + num12;
                if(num14 < 0) {
                    num14 += count;
                }
                else if(num14 > (count - 1)) {
                    num14 -= count;
                }
                Rectangle rectangle = new Rectangle(x, num4, 0x18, menuHeight);
                Rectangle rectangle2 = new Rectangle(0x24, num4 + num7, 0x10, 0x10);
                Rectangle rectangle3 = new Rectangle(0x36, num4, 300, menuHeight);
                Rectangle rectangle4 = new Rectangle(4, num4, width - 8, menuHeight);
                bool flag2 = num14 == this.initialSelectedIndex;
                this.DrawText(g, (num14 + 1).ToString(), rectangle, false, flag2, StringAlignment.Far);
                g.DrawImage(GetImage(this.lstPaths[num14]), rectangle2);
                this.DrawText(g, this.lstPaths[num14].strDisplay, rectangle3, false, flag2, StringAlignment.Near);
                this.dicItemRcts[num14] = rectangle4;
                if(num14 == this.iHoveredIndex) {
                    DrawSelection(g, rectangle4, Control.MouseButtons == MouseButtons.Left);
                }
                num4 += menuHeight;
            }
            Rectangle rct = new Rectangle(x, num4, 0x18, menuHeight);
            Rectangle rect = new Rectangle(0x24, num4 + num7, 0x10, 0x10);
            Rectangle rectangle7 = new Rectangle(0x36, num4, 300, menuHeight);
            Rectangle rectangle8 = new Rectangle(4, num4, width - 8, menuHeight);
            bool fBold = this.selectedIndex == this.initialSelectedIndex;
            if(!this.fCompositionEnabled) {
                g.FillRectangle(SystemBrushes.MenuHighlight, rectangle8);
            }
            this.DrawText(g, (this.selectedIndex + 1).ToString(), rct, true, fBold, StringAlignment.Far);
            g.DrawImage(GetImage(this.lstPaths[this.selectedIndex]), rect);
            this.DrawText(g, this.lstPaths[this.selectedIndex].strDisplay, rectangle7, true, fBold, StringAlignment.Near);
            if(this.fCompositionEnabled) {
                DrawSelection(g, rectangle8, true);
            }
            this.dicItemRcts[this.selectedIndex] = rectangle8;
            num4 += menuHeight;
            for(int j = 0; j < num11; j++) {
                int num16 = (j + this.selectedIndex) + 1;
                if(num16 < 0) {
                    num16 += count;
                }
                else if(num16 > (count - 1)) {
                    num16 -= count;
                }
                Rectangle rectangle9 = new Rectangle(x, num4, 0x18, menuHeight);
                Rectangle rectangle10 = new Rectangle(0x24, num4 + num7, 0x10, 0x10);
                Rectangle rectangle11 = new Rectangle(0x36, num4, 300, menuHeight);
                Rectangle rectangle12 = new Rectangle(4, num4, width - 8, menuHeight);
                bool flag4 = num16 == this.initialSelectedIndex;
                this.DrawText(g, (num16 + 1).ToString(), rectangle9, false, flag4, StringAlignment.Far);
                g.DrawImage(GetImage(this.lstPaths[num16]), rectangle10);
                this.DrawText(g, this.lstPaths[num16].strDisplay, rectangle11, false, flag4, StringAlignment.Near);
                this.dicItemRcts[num16] = rectangle12;
                if(num16 == this.iHoveredIndex) {
                    DrawSelection(g, rectangle12, Control.MouseButtons == MouseButtons.Left);
                }
                num4 += menuHeight;
            }
        }

        private static void DrawSelection(Graphics g, Rectangle rct, bool fHot) {
            if(QTUtility.IsVista) {
                int x = rct.X;
                int y = rct.Y;
                int num3 = rct.Width - 2;
                int height = rct.Height;
                if(!fHot) {
                    g.DrawImage(Resources_Image.imgVistaMenu_LT, new Rectangle(x + 3, y, 4, 4));
                    g.DrawImage(Resources_Image.imgVistaMenu_LM, new Rectangle(x + 3, y + 4, 4, height - 8));
                    g.DrawImage(Resources_Image.imgVistaMenu_LB, new Rectangle(x + 3, (y + height) - 4, 4, 4));
                    g.DrawImage(Resources_Image.imgVistaMenu_MT, new Rectangle(x + 7, y, num3 - 11, 4));
                    g.DrawImage(Resources_Image.imgVistaMenu_MM, new Rectangle(x + 7, y + 4, num3 - 11, height - 8));
                    g.DrawImage(Resources_Image.imgVistaMenu_MB, new Rectangle(x + 7, (y + height) - 4, num3 - 11, 4));
                    g.DrawImage(Resources_Image.imgVistaMenu_RT, new Rectangle((x + num3) - 4, y, 4, 4));
                    g.DrawImage(Resources_Image.imgVistaMenu_RM, new Rectangle((x + num3) - 4, y + 4, 4, height - 8));
                    g.DrawImage(Resources_Image.imgVistaMenu_RB, new Rectangle((x + num3) - 4, (y + height) - 4, 4, 4));
                }
                else {
                    g.DrawImage(Resources_Image.imgVistaMenu_LT_hot, new Rectangle(x + 3, y, 4, 4));
                    g.DrawImage(Resources_Image.imgVistaMenu_LM_hot, new Rectangle(x + 3, y + 4, 4, height - 8));
                    g.DrawImage(Resources_Image.imgVistaMenu_LB_hot, new Rectangle(x + 3, (y + height) - 4, 4, 4));
                    g.DrawImage(Resources_Image.imgVistaMenu_MT_hot, new Rectangle(x + 7, y, num3 - 11, 4));
                    g.DrawImage(Resources_Image.imgVistaMenu_MM_hot, new Rectangle(x + 7, y + 4, num3 - 11, height - 8));
                    g.DrawImage(Resources_Image.imgVistaMenu_MB_hot, new Rectangle(x + 7, (y + height) - 4, num3 - 11, 4));
                    g.DrawImage(Resources_Image.imgVistaMenu_RT_hot, new Rectangle((x + num3) - 4, y, 4, 4));
                    g.DrawImage(Resources_Image.imgVistaMenu_RM_hot, new Rectangle((x + num3) - 4, y + 4, 4, height - 8));
                    g.DrawImage(Resources_Image.imgVistaMenu_RB_hot, new Rectangle((x + num3) - 4, (y + height) - 4, 4, 4));
                }
            }
            else {
                int alpha = fHot ? 0x60 : 0x20;
                using(SolidBrush brush = new SolidBrush(Color.FromArgb(alpha, SystemColors.MenuHighlight))) {
                    g.FillRectangle(brush, rct);
                }
            }
        }

        private void DrawText(Graphics g, string text, Rectangle rct, bool fSelect, bool fBold, StringAlignment horizontalAlign) {
            if(this.fCompositionEnabled) {
                IntPtr hdc = g.GetHdc();
                DrawTextOnGlass(hdc, text, fBold ? this.fntMenuBold : this.fntMenu, rct, 4, horizontalAlign);
                g.ReleaseHdc(hdc);
            }
            else {
                using(StringFormat format = StringFormat.GenericDefault) {
                    format.Alignment = horizontalAlign;
                    format.LineAlignment = StringAlignment.Center;
                    format.Trimming = StringTrimming.EllipsisCharacter;
                    format.FormatFlags |= StringFormatFlags.NoWrap;
                    if(horizontalAlign == StringAlignment.Center) {
                        format.Trimming = StringTrimming.EllipsisPath;
                    }
                    g.DrawString(text, fBold ? this.fntMenuBold : this.fntMenu, fSelect ? SystemBrushes.HighlightText : SystemBrushes.MenuText, rct, format);
                }
            }
        }

        private static void DrawTextOnGlass(IntPtr hDC, string text, Font font, Rectangle rct, int iGlowSize, StringAlignment horizontalAlign) {
            QTTabBarLib.Interop.RECT rect = new QTTabBarLib.Interop.RECT();
            QTTabBarLib.Interop.RECT pRect = new QTTabBarLib.Interop.RECT();
            rect.left = rct.Left;
            rect.right = rct.Right + (4 * iGlowSize);
            rect.top = rct.Top;
            rect.bottom = rct.Bottom + (2 * iGlowSize);
            pRect.left = 2 * iGlowSize;
            pRect.top = 2;
            pRect.right = rect.Width - (3 * iGlowSize);
            pRect.bottom = rect.Height - 2;
            TextFormatFlags dwFlags = TextFormatFlags.ModifyString | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine;
            switch(horizontalAlign) {
                case StringAlignment.Near:
                    dwFlags |= TextFormatFlags.EndEllipsis;
                    break;

                case StringAlignment.Center:
                    dwFlags |= TextFormatFlags.PathEllipsis | TextFormatFlags.HorizontalCenter;
                    break;

                case StringAlignment.Far:
                    dwFlags |= TextFormatFlags.Right;
                    break;
            }
            IntPtr ptr = PInvoke.CreateCompatibleDC(hDC);
            if(ptr != IntPtr.Zero) {
                IntPtr ptr5;
                QTTabBarLib.Interop.BITMAPINFO pbmi = new QTTabBarLib.Interop.BITMAPINFO();
                pbmi.bmiHeader.biSize = Marshal.SizeOf(typeof(QTTabBarLib.Interop.BITMAPINFOHEADER));
                pbmi.bmiHeader.biWidth = rect.Width;
                pbmi.bmiHeader.biHeight = -rect.Height;
                pbmi.bmiHeader.biPlanes = 1;
                pbmi.bmiHeader.biBitCount = 0x20;
                pbmi.bmiHeader.biCompression = 0;
                IntPtr hgdiobj = PInvoke.CreateDIBSection(ptr, ref pbmi, 0, out ptr5, IntPtr.Zero, 0);
                if(hgdiobj != IntPtr.Zero) {
                    IntPtr ptr3 = PInvoke.SelectObject(ptr, hgdiobj);
                    IntPtr ptr6 = font.ToHfont();
                    IntPtr ptr4 = PInvoke.SelectObject(ptr, ptr6);
                    VisualStyleRenderer renderer = new VisualStyleRenderer(VisualStyleElement.Window.Caption.Active);
                    DTTOPTS pOptions = new DTTOPTS();
                    pOptions.dwSize = Marshal.SizeOf(typeof(DTTOPTS));
                    pOptions.dwFlags = 0x2800;
                    pOptions.iGlowSize = iGlowSize;
                    PInvoke.DrawThemeTextEx(renderer.Handle, ptr, 0, 0, text, -1, dwFlags, ref pRect, ref pOptions);
                    PInvoke.BitBlt(hDC, rect.left, rect.top, rect.Width, rect.Height, ptr, 0, 0, 0xcc0020);
                    PInvoke.SelectObject(ptr, ptr3);
                    PInvoke.SelectObject(ptr, ptr4);
                    PInvoke.DeleteObject(hgdiobj);
                    PInvoke.DeleteObject(ptr6);
                }
                PInvoke.DeleteDC(ptr);
            }
        }

        private static Image GetImage(PathData pathData) {
            string strImageKey = pathData.strImageKey;
            if(!QTUtility.ImageListGlobal.Images.ContainsKey(strImageKey)) {
                strImageKey = QTUtility.GetImageKey(pathData.strPath, null);
            }
            return QTUtility.ImageListGlobal.Images[strImageKey];
        }

        private static string GetTitleText(PathData pathData) {
            string strPath = pathData.strPath;
            if(strPath.StartsWith("::")) {
                return pathData.strDisplay;
            }
            int index = strPath.IndexOf("???");
            int length = strPath.IndexOf("*?*?*");
            if(index != -1) {
                return strPath.Substring(0, index);
            }
            if(length != -1) {
                strPath = strPath.Substring(0, length);
            }
            return strPath;
        }

        public void HideSwitcher(bool fSwitch) {
            this.HideSwitcherInner(false, fSwitch);
        }

        private void HideSwitcherInner(bool fClickClose, bool fSwitch) {
            if(this.fIsShown) {
                if(this.fCompositionEnabled) {
                    base.SetStyle(ControlStyles.OptimizedDoubleBuffer, false);
                    this.fDoubleBufferRequired = true;
                }
                this.fIsShown = false;
                PInvoke.ShowWindow(base.Handle, 0);
                if(fSwitch && (this.Switched != null)) {
                    ItemCheckEventArgs e = new ItemCheckEventArgs(fClickClose ? this.iHoveredIndex : this.selectedIndex, CheckState.Checked, CheckState.Checked);
                    this.Switched(this, e);
                }
            }
        }

        private void InitializeComponent() {
            this.components = new Container();
            this.toolTipSwitcher = new System.Windows.Forms.ToolTip(this.components);
            base.SuspendLayout();
            this.toolTipSwitcher.ShowAlways = true;
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0x110, 0x130);
            base.ControlBox = false;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "TabSwitchForm";
            base.ShowIcon = false;
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.Manual;
            base.ResumeLayout(false);
        }

        protected override void OnMouseLeave(EventArgs e) {
            base.OnMouseLeave(e);
            if(this.iHoveredIndex != -1) {
                this.iHoveredIndex = -1;
                base.Invalidate();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
            Dictionary<int, Rectangle> dictionary = new Dictionary<int, Rectangle>(this.dicItemRcts);
            foreach(int num in dictionary.Keys) {
                Rectangle rectangle = this.dicItemRcts[num];
                if(rectangle.Contains(e.Location)) {
                    if(this.iHoveredIndex != num) {
                        if(this.dicItemRcts.ContainsKey(this.iHoveredIndex)) {
                            base.Invalidate(this.dicItemRcts[this.iHoveredIndex]);
                        }
                        this.iHoveredIndex = num;
                        base.Invalidate(this.dicItemRcts[num]);
                        this.toolTipSwitcher.Active = false;
                        this.toolTipSwitcher.SetToolTip(this, GetTitleText(this.lstPaths[this.iHoveredIndex]));
                        this.toolTipSwitcher.Active = true;
                    }
                    return;
                }
            }
            if(this.iHoveredIndex != -1) {
                base.Invalidate();
                this.iHoveredIndex = -1;
            }
            this.toolTipSwitcher.Active = false;
        }

        protected override void OnPaint(PaintEventArgs e) {
            if(this.lstPaths.Count > 0) {
                this.DrawItems(e.Graphics);
            }
            if(this.fDoubleBufferRequired) {
                base.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
                this.fDoubleBufferRequired = false;
            }
        }

        private void SetCompositionState() {
            if(QTUtility.IsVista) {
                if((0 <= PInvoke.DwmIsCompositionEnabled(out this.fCompositionEnabled)) && this.fCompositionEnabled) {
                    QTTabBarLib.Interop.MARGINS pMarInset = new QTTabBarLib.Interop.MARGINS();
                    pMarInset.cxLeftWidth = -1;
                    if(0 <= PInvoke.DwmExtendFrameIntoClientArea(base.Handle, ref pMarInset)) {
                        base.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
                        this.BackColor = Color.Black;
                        return;
                    }
                }
                this.BackColor = SystemColors.Menu;
            }
            else {
                base.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            }
            base.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        }

        public void ShowSwitcher(IntPtr hwndExplr, int indexCurrent, List<PathData> lstPaths) {
            QTTabBarLib.Interop.RECT rect;
            this.initialSelectedIndex = this.selectedIndex = indexCurrent;
            this.lstPaths = lstPaths;
            menuHeight = SystemInformation.MenuHeight;
            if(QTUtility.IsVista && (PInvoke.DwmIsCompositionEnabled(out this.fCompositionEnabled) != 0)) {
                this.fCompositionEnabled = false;
            }
            int num = Math.Min(11, lstPaths.Count);
            int cy = 0x2a + (menuHeight * (num + 1));
            int cx = 0x184;
            PInvoke.GetWindowRect(hwndExplr, out rect);
            int x = rect.left + ((rect.Width - cx) / 2);
            int y = rect.top + ((rect.Height - cy) / 2);
            PInvoke.SetWindowPos(base.Handle, (IntPtr)(-1), x, y, cx, cy, 0x18);
            PInvoke.ShowWindow(base.Handle, 4);
            this.fIsShown = true;
            this.fDoubleBufferRequired = this.fCompositionEnabled;
        }

        public int Switch(bool fShift) {
            this.selectedIndex += fShift ? -1 : 1;
            this.iHoveredIndex = -1;
            if(this.selectedIndex < 0) {
                this.selectedIndex = this.lstPaths.Count - 1;
            }
            else if(this.selectedIndex > (this.lstPaths.Count - 1)) {
                this.selectedIndex = 0;
            }
            if(this.toolTipSwitcher.Active) {
                this.toolTipSwitcher.Hide(this);
            }
            base.Invalidate();
            return this.selectedIndex;
        }

        protected override void WndProc(ref Message m) {
            switch(m.Msg) {
                case 0x202:
                    if(this.iHoveredIndex != -1) {
                        Point pt = new Point(QTUtility2.GET_X_LPARAM(m.LParam), QTUtility2.GET_Y_LPARAM(m.LParam));
                        Dictionary<int, Rectangle> dictionary2 = new Dictionary<int, Rectangle>(this.dicItemRcts);
                        foreach(int num2 in dictionary2.Keys) {
                            if(num2 == this.iHoveredIndex) {
                                Rectangle rectangle2 = this.dicItemRcts[num2];
                                if(rectangle2.Contains(pt)) {
                                    this.HideSwitcherInner(true, true);
                                }
                                break;
                            }
                        }
                    }
                    goto Label_01D5;

                case 0x31e:
                    this.SetCompositionState();
                    goto Label_01D5;

                case 0x21:
                    if((QTUtility2.GET_Y_LPARAM(m.LParam) == 0x201) && (this.iHoveredIndex != -1)) {
                        Point point = base.PointToClient(Control.MousePosition);
                        Dictionary<int, Rectangle> dictionary = new Dictionary<int, Rectangle>(this.dicItemRcts);
                        foreach(int num in dictionary.Keys) {
                            if(num == this.iHoveredIndex) {
                                Rectangle rectangle = this.dicItemRcts[num];
                                if(rectangle.Contains(point)) {
                                    base.Invalidate(this.dicItemRcts[num]);
                                }
                                break;
                            }
                        }
                    }
                    break;

                case 0x84:
                    base.WndProc(ref m);
                    switch(((int)m.Result)) {
                        case 10:
                        case 11:
                        case 12:
                        case 13:
                        case 14:
                        case 15:
                        case 0x10:
                        case 0x11:
                            m.Result = (IntPtr)0x12;
                            return;
                    }
                    return;

                default:
                    goto Label_01D5;
            }
            m.Result = (IntPtr)4;
            return;
        Label_01D5:
            base.WndProc(ref m);
        }

        protected override System.Windows.Forms.CreateParams CreateParams {
            get {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                if(!QTUtility.IsVista) {
                    createParams.ClassStyle |= 0x20000;
                }
                return createParams;
            }
        }

        public bool IsShown {
            get {
                return this.fIsShown;
            }
        }

        public int SelectedIndex {
            get {
                return this.selectedIndex;
            }
        }
    }
}
