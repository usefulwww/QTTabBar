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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using BandObjectLib;
using Microsoft.Win32;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    internal class RebarController : IDisposable {
        private QTTabBarClass tabbar;
        private IOleCommandTarget bandObjectSite;
        private IntPtr ExplorerHandle;
        private TextureBrush textureBrushRebar; 
        private Bitmap bmpRebar;
        private NativeWindowController rebarController;

        public IntPtr Handle { get; private set; }

        public bool MenuBarShown {
            get {
                using(RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", false)) {
                    return  key != null && (int)key.GetValue("AlwaysShowMenus", 0) != 0;
                }
            }
            set {
                using(RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", false)) {
                    if(key != null) key.SetValue("AlwaysShowMenus", value ? 1 : 0);
                }
                EnsureMenuBarIsCorrect();
            }
        }

        public bool Locked {
            get {
                REBARBANDINFO structure = new REBARBANDINFO();
                structure.cbSize = Marshal.SizeOf(structure);
                structure.fMask = 1;
                IntPtr ptr2 = PInvoke.SendMessage(Handle, 0x405, IntPtr.Zero, ref structure);
                return ptr2 != IntPtr.Zero && (structure.fStyle & 0x100) != 0;
            }
            set {
                const int LockToolbarsCmdID = 0xA20C;
                if(Locked == value) return;
                PInvoke.SendMessage(QTUtility.IsXP ? ExplorerHandle : WindowUtils.GetShellTabWindowClass(ExplorerHandle), 
                        WM.COMMAND, (IntPtr)LockToolbarsCmdID, IntPtr.Zero);
            }
        }

        public RebarController(QTTabBarClass tabbar, IntPtr hwndReBar, IOleCommandTarget bandObjectSite) {
            this.tabbar = tabbar;
            this.bandObjectSite = bandObjectSite;
            ExplorerHandle = PInvoke.GetAncestor(hwndReBar, 2);
            Handle = hwndReBar;
            rebarController = new NativeWindowController(hwndReBar);
            rebarController.MessageCaptured += MessageCaptured;

            if(Config.UseRebarBGColor) {
                if(QTUtility.DefaultRebarCOLORREF == -1) {
                    QTUtility.DefaultRebarCOLORREF = (int)PInvoke.SendMessage(Handle, RB.GETBKCOLOR, IntPtr.Zero, IntPtr.Zero);
                }
                int num2 = QTUtility2.MakeCOLORREF(QTUtility.RebarBGColor);
                PInvoke.SendMessage(Handle, RB.SETBKCOLOR, IntPtr.Zero, (IntPtr)num2);
            }

            EnsureMenuBarIsCorrect();
        }

        private void CreateRebarImage() {
            if(bmpRebar != null) {
                bmpRebar.Dispose();
                bmpRebar = null;
            }
            if(textureBrushRebar != null) {
                textureBrushRebar.Dispose();
                textureBrushRebar = null;
            }
            if(!File.Exists(QTUtility.Path_RebarImage)) return;
            try {
                using(Bitmap bitmap = new Bitmap(QTUtility.Path_RebarImage)) {
                    bmpRebar = new Bitmap(bitmap, bitmap.Size);
                    textureBrushRebar = new TextureBrush(bmpRebar);
                    if(Path.GetExtension(QTUtility.Path_RebarImage).PathEquals(".bmp")) {
                        bmpRebar.MakeTransparent(Color.Magenta);
                    }
                }
            }
            catch {
            }
        }

        public void EnsureMenuBarIsCorrect() {
            REBARBANDINFO structure = new REBARBANDINFO();
            structure.cbSize = Marshal.SizeOf(structure);
            structure.fMask = RBBIM.CHILD | RBBIM.ID;
            int num = (int)PInvoke.SendMessage(Handle, RB.GETBANDCOUNT, IntPtr.Zero, IntPtr.Zero);
            for(int i = 0; i < num; i++) {
                PInvoke.SendMessage(Handle, RB.GETBANDINFO, (IntPtr)i, ref structure);
                if(PInvoke.GetClassName(structure.hwndChild) == "ToolbarWindow32" && structure.wID == 1) {
                    PInvoke.SendMessage(Handle, RB.SHOWBAND, (IntPtr)i, MenuBarShown ? ((IntPtr)1) : IntPtr.Zero);
                    return;
                }
            }
        }

        internal void RefreshHeight() {
            const int DBID_BANDINFOCHANGED = 0;
            const int OLECMDEXECOPT_DODEFAULT = 0;
            const int RBN_HEIGHTCHANGE = -831;
            const int GWL_HWNDPARENT = -8;
            try {
                tabbar.SuspendLayout();
                if(bandObjectSite != null) {
                    Guid pguidCmdGroup = ExplorerGUIDs.CGID_DeskBand;
                    bandObjectSite.Exec(ref pguidCmdGroup, DBID_BANDINFOCHANGED, OLECMDEXECOPT_DODEFAULT, IntPtr.Zero, IntPtr.Zero);
                    if(QTUtility.IsXP) {
                        RECT rect;
                        PInvoke.GetWindowRect(ExplorerHandle, out rect);
                        int num = (rect.Height << 0x10) | rect.Width;
                        PInvoke.SendMessage(ExplorerHandle, 5, IntPtr.Zero, (IntPtr)num);
                    }
                    else {
                        IntPtr windowLongPtr = PInvoke.GetWindowLongPtr(Handle, GWL_HWNDPARENT);
                        NMHDR structure = new NMHDR {
                            hwndFrom = Handle,
                            idFrom = (IntPtr)40965, // magic id
                            code = RBN_HEIGHTCHANGE
                        };
                        PInvoke.SendMessage(windowLongPtr, WM.NOTIFY, structure.idFrom, ref structure);
                    }
                }
            }
            catch(COMException exception) {
                QTUtility2.MakeErrorLog(exception);
            }
            finally {
                tabbar.ResumeLayout();
            }
        }

        public void RefreshBG(bool fRebarBGCanceled) {
            if(fRebarBGCanceled && QTUtility.DefaultRebarCOLORREF != -1) {
                // Restore the default BG color
                PInvoke.SendMessage(Handle, RB.SETBKCOLOR, IntPtr.Zero, (IntPtr)QTUtility.DefaultRebarCOLORREF);
            }
            else if(Config.UseRebarBGColor) {
                // Save the default color and set the new one
                if(QTUtility.DefaultRebarCOLORREF == -1) {
                    QTUtility.DefaultRebarCOLORREF = (int)PInvoke.SendMessage(Handle, RB.GETBKCOLOR, IntPtr.Zero, IntPtr.Zero);
                }
                int c = QTUtility2.MakeCOLORREF(QTUtility.RebarBGColor);
                PInvoke.SendMessage(Handle, RB.SETBKCOLOR, IntPtr.Zero, (IntPtr)c);
            }
            // Refresh the rebar
            IntPtr hWnd = PInvoke.GetWindowLongPtr(Handle, -8);
            if(hWnd != IntPtr.Zero) {
                PInvoke.RedrawWindow(hWnd, IntPtr.Zero, IntPtr.Zero, 0x289);
            }
            if(Config.UseRebarImage) {
                CreateRebarImage();
            }
        }

        private bool MessageCaptured(ref Message m) {
            // Make sure the menu bar obeys the Explorer setting.
            // Was this really so hard, Microsoft?
            if(m.Msg == RB.SETBANDINFO) {
                REBARBANDINFO pInfo = (REBARBANDINFO)Marshal.PtrToStructure(m.LParam, typeof(REBARBANDINFO));
                if((PInvoke.GetClassName(pInfo.hwndChild) == "ToolbarWindow32") && (pInfo.wID == 1)) {
                    if(MenuBarShown) {
                        pInfo.fStyle &= ~RBBS.HIDDEN;
                    }
                    else {
                        pInfo.fStyle |= RBBS.HIDDEN;
                    }
                    Marshal.StructureToPtr(pInfo, m.LParam, false);
                }
                return false;
            }

            if(m.Msg == WM.ERASEBKGND && (Config.UseRebarBGColor || Config.UseRebarImage)) {
                bool fFilled = false;
                using(Graphics graphics = Graphics.FromHdc(m.WParam)) {
                    RECT rect;
                    PInvoke.GetWindowRect(Handle, out rect);
                    Rectangle rectangle = new Rectangle(0, 0, rect.Width, rect.Height);

                    // Fill the Rebar background color
                    if(Config.UseRebarBGColor) {
                        using(SolidBrush brush = new SolidBrush(QTUtility.RebarBGColor)) {
                            graphics.FillRectangle(brush, rectangle);
                            fFilled = true;
                        }
                    }

                    // Draw the Rebar image
                    if(VisualStyleRenderer.IsSupported && Config.UseRebarImage && QTUtility.Path_RebarImage.Length > 0) {
                        if(bmpRebar == null) {
                            CreateRebarImage();
                        }
                        if(bmpRebar != null) {
                            switch(Config.RebarStretchMode) {
                                /* *case 1: { // Stretch on each band
                                    // TODO
                                    if(!fFilled) rebarController.DefWndProc(ref m);
                                    int bandCount = (int)PInvoke.SendMessage(rebarController.Handle, RB.GETBANDCOUNT, IntPtr.Zero, IntPtr.Zero);
                                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                                    RECT rectBand = new RECT();
                                    RECT rectMargin = new RECT();
                                    
                                    // Draw the bitmap on each band
                                    for(int i = 0; i < bandCount; i++) {
                                        if(PInvoke.SendMessage(rebarController.Handle, RB.GETRECT, (IntPtr)i, ref rectBand) == IntPtr.Zero) {
                                            continue;
                                        }
                                        PInvoke.SendMessage(rebarController.Handle, RB.GETBANDBORDERS, (IntPtr)i, ref rectMargin);
                                        rectBand.left   -= !QTUtility.IsXP ? 4 : rectMargin.left;
                                        rectBand.top    -= rectMargin.top;
                                        rectBand.right  += rectMargin.right;
                                        rectBand.bottom += rectMargin.bottom;
                                        graphics.DrawImage(bmpRebar, rectBand.ToRectangle());
                                    }
                                    break;
                                } */
                                case StretchMode.Real: {
                                    if(!fFilled) rebarController.DefWndProc(ref m);
                                    Rectangle destRect = new Rectangle(Point.Empty, bmpRebar.Size);
                                    graphics.DrawImage(bmpRebar, destRect, destRect, GraphicsUnit.Pixel);
                                    break;
                                }
                                case StretchMode.Tile:
                                    textureBrushRebar = textureBrushRebar ?? new TextureBrush(bmpRebar);
                                    graphics.FillRectangle(textureBrushRebar, rectangle);
                                    break;

                                default: // Full
                                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                                    graphics.DrawImage(bmpRebar, rectangle);
                                    break;
                            }
                            fFilled = true;
                        }
                    }
                }
                if(fFilled) {
                    m.Result = (IntPtr)1;
                    return true;
                }
            }
            return false;
        }

        public void Dispose() {
            if(bmpRebar != null) {
                bmpRebar.Dispose();
                bmpRebar = null;
            }
            if(textureBrushRebar != null) {
                textureBrushRebar.Dispose();
                textureBrushRebar = null;
            }
        }
    }
}
