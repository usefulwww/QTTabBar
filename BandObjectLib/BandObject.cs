//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2002-2010  Pavel Zolnikov, Quizo, Paul Accisano
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

namespace BandObjectLib {
    using SHDocVw;
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    public class BandObject : UserControl, IDeskBand, IDockingWindow, IInputObject, IObjectWithSite, BandObjectLib.IOleWindow {
        private Size _minSize = new Size(-1, -1);
        protected IInputObjectSite BandObjectSite;
        protected WebBrowserClass Explorer;
        protected bool fClosedDW;
        protected bool fFinalRelease;
        protected IntPtr ReBarHandle;
        private RebarSubclass RebarSubclassInst = null;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);        

        internal sealed class RebarSubclass : NativeWindow {
            BandObject parent;
            public RebarSubclass(IntPtr hwnd, BandObject parent) {
                this.parent = parent;
                base.AssignHandle(hwnd);
            }

            protected override void WndProc(ref Message m) {
                if(m.Msg == RB.SETBANDINFO) {
                    REBARBANDINFO pInfo = (REBARBANDINFO)Marshal.PtrToStructure(m.LParam, typeof(REBARBANDINFO));
                    if(pInfo.hwndChild == parent.Handle &&
                            (pInfo.fMask & RBBIM.STYLE) != 0) {
                        if(parent.ShouldHaveBreak()) {
                            pInfo.fStyle |= RBBS.BREAK;
                        }
                        else {
                            pInfo.fStyle &= ~RBBS.BREAK;
                        }
                        Marshal.StructureToPtr(pInfo, m.LParam, false);
                        pInfo = (REBARBANDINFO)Marshal.PtrToStructure(m.LParam, typeof(REBARBANDINFO));
                        base.WndProc(ref m);
                        base.ReleaseHandle();
                        return;
                    }
                }
                base.WndProc(ref m);
            }
        }

        // Determines if the DeskBand is preceded by a break.
        protected bool BandHasBreak() {
            if(ReBarHandle != null) {
                int n = (int)SendMessage(ReBarHandle, RB.GETBANDCOUNT, IntPtr.Zero, IntPtr.Zero);
                for(int i = 0; i < n; ++i) {
                    REBARBANDINFO info = new REBARBANDINFO();
                    info.cbSize = Marshal.SizeOf(info);
                    info.fMask = RBBIM.STYLE | RBBIM.CHILD;
                    IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(info));
                    Marshal.StructureToPtr(info, ptr, false);
                    SendMessage(ReBarHandle, RB.GETBANDINFO, (IntPtr)i, ptr);
                    info = (REBARBANDINFO)Marshal.PtrToStructure(ptr, typeof(REBARBANDINFO));
                    Marshal.FreeHGlobal(ptr);
                    if(info.hwndChild == base.Handle) {
                        return (info.fStyle & RBBS.BREAK) != 0;
                    }
                }
            }
            return true;
        }

        public virtual void CloseDW(uint dwReserved) {
            this.fClosedDW = true;
            this.ShowDW(false);
            this.Dispose(true);
            if(this.Explorer != null) {
                Marshal.ReleaseComObject(this.Explorer);
                this.Explorer = null;
            }
            if(this.BandObjectSite != null) {
                Marshal.ReleaseComObject(this.BandObjectSite);
                this.BandObjectSite = null;
            }
            if(this.RebarSubclassInst != null) {
                this.RebarSubclassInst.ReleaseHandle();
                RebarSubclassInst = null;
            }
        }

        public virtual void ContextSensitiveHelp(bool fEnterMode) {
        }

        public virtual void GetBandInfo(uint dwBandID, uint dwViewMode, ref DESKBANDINFO dbi) {
            if((dbi.dwMask & DBIM.ACTUAL) != ((DBIM)0)) {
                dbi.ptActual.X = base.Size.Width;
                dbi.ptActual.Y = base.Size.Height;
            }
            if((dbi.dwMask & DBIM.INTEGRAL) != ((DBIM)0)) {
                dbi.ptIntegral.X = -1;
                dbi.ptIntegral.Y = -1;
            }
            if((dbi.dwMask & DBIM.MAXSIZE) != ((DBIM)0)) {
                dbi.ptMaxSize.X = dbi.ptMaxSize.Y = -1;
            }
            if((dbi.dwMask & DBIM.MINSIZE) != ((DBIM)0)) {
                dbi.ptMinSize.X = this.MinSize.Width;
                dbi.ptMinSize.Y = this.MinSize.Height;
            }
            if((dbi.dwMask & DBIM.MODEFLAGS) != ((DBIM)0)) {
                dbi.dwModeFlags = DBIMF.NORMAL;
            }
            if((dbi.dwMask & DBIM.BKCOLOR) != ((DBIM)0)) {
                dbi.dwMask &= ~DBIM.BKCOLOR;
            }
            if((dbi.dwMask & DBIM.TITLE) != ((DBIM)0)) {
                dbi.wszTitle = null;
            }
        }

        public virtual void GetSite(ref Guid riid, out object ppvSite) {
            ppvSite = this.BandObjectSite;
        }

        public virtual void GetWindow(out IntPtr phwnd) {
            phwnd = base.Handle;
        }

        public virtual int HasFocusIO() {
            if(!base.ContainsFocus) {
                return 1;
            }
            return 0;
        }

        protected virtual void OnExplorerAttached() {
        }

        protected override void OnGotFocus(EventArgs e) {
            base.OnGotFocus(e);
            if((!this.fClosedDW && (this.BandObjectSite != null)) && base.IsHandleCreated) {
                this.BandObjectSite.OnFocusChangeIS(this, 1);
            }
        }

        protected override void OnLostFocus(EventArgs e) {
            base.OnLostFocus(e);
            if((!this.fClosedDW && (this.BandObjectSite != null)) && (base.ActiveControl == null)) {
                this.BandObjectSite.OnFocusChangeIS(this, 0);
            }
        }

        public virtual void ResizeBorderDW(IntPtr prcBorder, object punkToolbarSite, bool fReserved) {
        }

        // Override this to set whether the DeskBand has a break when it is 
        // first displayed
        protected virtual bool ShouldHaveBreak() {
            return true;
        }

        public virtual void SetSite(object pUnkSite) {
            if(this.BandObjectSite != null) {
                Marshal.ReleaseComObject(this.BandObjectSite);
            }
            if(this.Explorer != null) {
                Marshal.ReleaseComObject(this.Explorer);
                this.Explorer = null;
            }
            this.BandObjectSite = pUnkSite as IInputObjectSite;
            if(this.BandObjectSite != null) {
                Guid guid = ExplorerGUIDs.IID_IWebBrowserApp;
                Guid riid = ExplorerGUIDs.IID_IUnknown;
                try {
                    object obj2;
                    ((_IServiceProvider)this.BandObjectSite).QueryService(ref guid, ref riid, out obj2);
                    this.Explorer = (WebBrowserClass)Marshal.CreateWrapperOfType(obj2 as IWebBrowser, typeof(WebBrowserClass));
                    this.OnExplorerAttached();
                }
                catch(COMException) {
                }
            }
            try {
                BandObjectLib.IOleWindow window = pUnkSite as BandObjectLib.IOleWindow;
                if(window != null) {
                    window.GetWindow(out this.ReBarHandle);
                }
            }
            catch {
            }
        }

        public virtual void ShowDW(bool fShow) {
            if(this.ReBarHandle != IntPtr.Zero) {
                if(RebarSubclassInst != null) {
                    RebarSubclassInst.ReleaseHandle();
                    RebarSubclassInst = null;
                }
                RebarSubclassInst = new RebarSubclass(ReBarHandle, this);
            }
            base.Visible = fShow;
        }

        public virtual int TranslateAcceleratorIO(ref BandObjectLib.MSG msg) {
            if(((msg.message == 0x100) && ((msg.wParam == ((IntPtr)9L)) || (msg.wParam == ((IntPtr)0x75L)))) && base.SelectNextControl(base.ActiveControl, (Control.ModifierKeys & Keys.Shift) != Keys.Shift, true, false, false)) {
                return 0;
            }
            return 1;
        }

        public virtual void UIActivateIO(int fActivate, ref BandObjectLib.MSG Msg) {
            if(fActivate != 0) {
                Control nextControl = base.GetNextControl(this, true);
                if(nextControl != null) {
                    nextControl.Select();
                }
                base.Focus();
            }
        }

        public Size MinSize {
            get {
                return this._minSize;
            }
            set {
                this._minSize = value;
            }
        }
    }
}
