//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2010  Paul Accisano
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
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;

    internal sealed class UpDown : Control {
        private NativeUpDown nativeUpDown;

        public event QEventHandler ValueChanged;

        public UpDown() {
            base.Size = new Size(0x24, 0x18);
            this.nativeUpDown = new NativeUpDown(this);
        }

        protected override void Dispose(bool disposing) {
            if(this.nativeUpDown != null) {
                this.nativeUpDown.DestroyHandle();
                this.nativeUpDown = null;
            }
            base.Dispose(disposing);
        }

        protected override void WndProc(ref Message m) {
            if(((this.nativeUpDown != null) && (m.Msg == 0x4e)) && (this.ValueChanged != null)) {
                QTTabBarLib.Interop.NMHDR nmhdr = (QTTabBarLib.Interop.NMHDR)Marshal.PtrToStructure(m.LParam, typeof(QTTabBarLib.Interop.NMHDR));
                if((nmhdr.code == -722) && (nmhdr.hwndFrom == this.nativeUpDown.Handle)) {
                    NMUPDOWN nmupdown = (NMUPDOWN)Marshal.PtrToStructure(m.LParam, typeof(NMUPDOWN));
                    this.ValueChanged(this, new QEventArgs((nmupdown.iDelta < 0) ? ArrowDirection.Right : ArrowDirection.Left));
                }
            }
            base.WndProc(ref m);
        }

        private sealed class NativeUpDown : NativeWindow {
            private bool fTrackMouseEvent;
            private static Rectangle rctDw = new Rectangle(0, 0, 0x12, 0x18);
            private static Rectangle rctUp = new Rectangle(0x12, 0, 0x12, 0x18);
            private VisualStyleRenderer rendererDown_Hot;
            private VisualStyleRenderer rendererDown_Normal;
            private VisualStyleRenderer rendererDown_Pressed;
            private VisualStyleRenderer rendererUp_Hot;
            private VisualStyleRenderer rendererUp_Normal;
            private VisualStyleRenderer rendererUp_Pressed;
            private int stateDown;
            private int stateUP;
            private QTTabBarLib.Interop.TRACKMOUSEEVENT TME;

            public NativeUpDown(UpDown OwnerControl) {
                CreateParams cp = new CreateParams();
                cp.ClassName = "msctls_updown32";
                cp.X = cp.Y = 0;
                cp.Width = OwnerControl.Width;
                cp.Height = OwnerControl.Height;
                cp.Parent = OwnerControl.Handle;
                cp.Style = 0x50000040;
                this.CreateHandle(cp);
                this.fTrackMouseEvent = true;
                this.TME = new QTTabBarLib.Interop.TRACKMOUSEEVENT();
                this.TME.cbSize = Marshal.SizeOf(this.TME);
                this.TME.dwFlags = 2;
                this.TME.hwndTrack = base.Handle;
            }

            private void InitializeRenderer() {
                this.rendererDown_Normal = new VisualStyleRenderer(VisualStyleElement.Spin.DownHorizontal.Normal);
                this.rendererUp_Normal = new VisualStyleRenderer(VisualStyleElement.Spin.UpHorizontal.Normal);
                this.rendererDown_Hot = new VisualStyleRenderer(VisualStyleElement.Spin.DownHorizontal.Hot);
                this.rendererUp_Hot = new VisualStyleRenderer(VisualStyleElement.Spin.UpHorizontal.Hot);
                this.rendererDown_Pressed = new VisualStyleRenderer(VisualStyleElement.Spin.DownHorizontal.Pressed);
                this.rendererUp_Pressed = new VisualStyleRenderer(VisualStyleElement.Spin.UpHorizontal.Pressed);
            }

            protected override void WndProc(ref Message m) {
                if(VisualStyleRenderer.IsSupported) {
                    switch(m.Msg) {
                        case 0x200: {
                                if(this.fTrackMouseEvent) {
                                    this.fTrackMouseEvent = false;
                                    PInvoke.TrackMouseEvent(ref this.TME);
                                }
                                bool flag = (((int)m.WParam) & 1) == 1;
                                if((((int)((long)m.LParam)) & 0xffff) < 0x13) {
                                    this.stateDown = flag ? 2 : 1;
                                    this.stateUP = 0;
                                }
                                else {
                                    this.stateDown = 0;
                                    this.stateUP = flag ? 2 : 1;
                                }
                                PInvoke.InvalidateRect(m.HWnd, IntPtr.Zero, false);
                                break;
                            }
                        case 0x201:
                            if((((int)((long)m.LParam)) & 0xffff) >= 0x13) {
                                this.stateUP = 2;
                                break;
                            }
                            this.stateDown = 2;
                            break;

                        case 0x202:
                            if((((int)((long)m.LParam)) & 0xffff) >= 0x13) {
                                this.stateUP = 1;
                                break;
                            }
                            this.stateDown = 1;
                            break;

                        case 0x2a3:
                            this.stateDown = this.stateUP = 0;
                            this.fTrackMouseEvent = true;
                            PInvoke.InvalidateRect(m.HWnd, IntPtr.Zero, false);
                            break;

                        case 15: {
                                if(this.rendererDown_Normal == null) {
                                    this.InitializeRenderer();
                                }
                                IntPtr dC = PInvoke.GetDC(m.HWnd);
                                if(!(dC != IntPtr.Zero)) {
                                    break;
                                }
                                using(Graphics graphics = Graphics.FromHdc(dC)) {
                                    VisualStyleRenderer renderer;
                                    VisualStyleRenderer renderer2;
                                    if(this.stateDown == 0) {
                                        renderer = this.rendererDown_Normal;
                                    }
                                    else if(this.stateDown == 1) {
                                        renderer = this.rendererDown_Hot;
                                    }
                                    else {
                                        renderer = this.rendererDown_Pressed;
                                    }
                                    if(this.stateUP == 0) {
                                        renderer2 = this.rendererUp_Normal;
                                    }
                                    else if(this.stateUP == 1) {
                                        renderer2 = this.rendererUp_Hot;
                                    }
                                    else {
                                        renderer2 = this.rendererUp_Pressed;
                                    }
                                    renderer.DrawBackground(graphics, rctDw);
                                    renderer2.DrawBackground(graphics, rctUp);
                                }
                                PInvoke.ReleaseDC(m.HWnd, dC);
                                PInvoke.ValidateRect(m.HWnd, IntPtr.Zero);
                                m.Result = IntPtr.Zero;
                                return;
                            }
                    }
                }
                base.WndProc(ref m);
            }
        }
    }
}
