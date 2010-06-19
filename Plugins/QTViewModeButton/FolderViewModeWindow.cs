//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2010  Quizo, Paul Accisano
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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Runtime.InteropServices;

using QTPlugin;
using QTPlugin.Interop;

namespace QuizoPlugins {
    internal sealed partial class FolderViewModeWindow : Form {
        private bool fFirstFocusing;
        public event EventHandler ViewModeChanged;
        private Color clrBG, clrBorder;


        public FolderViewModeWindow(string[] resLabels) {
            ProfessionalColorTable pct = new ProfessionalColorTable();
            this.clrBG = pct.ToolStripDropDownBackground;
            this.clrBorder = pct.MenuBorder;

            this.BackColor = this.clrBG;

            InitializeComponent();

            this.labelTHUMBSTRIP.Text = resLabels[0];
            this.labelTHUMBNAIL.Text = resLabels[1];
            this.labelTILE.Text = resLabels[2];
            this.labelICON.Text = resLabels[3];
            this.labelLIST.Text = resLabels[4];
            this.labelDETAIL.Text = resLabels[5];

            Rectangle rct = new Rectangle(4, 4, 16, 16);
            System.Drawing.Imaging.PixelFormat pf = System.Drawing.Imaging.PixelFormat.Format32bppArgb;
            this.labelTHUMBSTRIP.Image2 = Resource.imgFilm.Clone(rct, pf);
            this.labelTHUMBNAIL.Image2 = Resource.imgThumb.Clone(rct, pf);
            this.labelTILE.Image2 = Resource.imgTiles.Clone(rct, pf);
            this.labelICON.Image2 = Resource.imgIcon.Clone(rct, pf);
            this.labelLIST.Image2 = Resource.imgList.Clone(rct, pf);
            this.labelDETAIL.Image2 = Resource.imgDetails.Clone(rct, pf);

            this.labelTHUMBSTRIP.Tag = FOLDERVIEWMODE.FVM_THUMBSTRIP;
            this.labelTHUMBNAIL.Tag = FOLDERVIEWMODE.FVM_THUMBNAIL;
            this.labelTILE.Tag = FOLDERVIEWMODE.FVM_TILE;
            this.labelICON.Tag = FOLDERVIEWMODE.FVM_ICON;
            this.labelLIST.Tag = FOLDERVIEWMODE.FVM_LIST;
            this.labelDETAIL.Tag = FOLDERVIEWMODE.FVM_DETAILS;

            this.trackBar1.LostFocus += new EventHandler(trackBar1_LostFocus);
            this.trackBar1.KeyDown += new KeyEventHandler(trackBar1_KeyDown);

            IntPtr p = this.trackBar1.Handle;
        }


        public void ShowWindow(Point pnt, FOLDERVIEWMODE fvmCurrentMode) {
            const uint SWP_NOACTIVATE = 0x0010;
            const int SW_SHOWNOACTIVATE = 4;

            this.trackBar1.Value = FolderViewModeWindow.ModeToInt(fvmCurrentMode);

            // set the slider of trackbar under mouse position
            RECT rct = this.GetThumbRect();
            Point pntCenter = new Point(rct.left + rct.Width / 2, rct.top + rct.Height / 2);
            Rectangle rctScreen = Screen.FromPoint(pnt).Bounds;

            pnt.X = pnt.X - pntCenter.X;
            pnt.Y = pnt.Y - pntCenter.Y;

            // ensure visible in the screen
            if(pnt.X < rctScreen.Left)
                pnt.X = rctScreen.Left;
            else if(pnt.X + this.Width > rctScreen.Right)
                pnt.X = rctScreen.Right - this.Width;

            if(pnt.Y < rctScreen.Top)
                pnt.Y = rctScreen.Top;
            else if(pnt.Y + this.Height > rctScreen.Bottom)
                pnt.Y = rctScreen.Bottom - this.Height;

            PInvoke.SetWindowPos(this.Handle, (IntPtr)(-1), pnt.X, pnt.Y, this.Width, this.Height, SWP_NOACTIVATE);
            PInvoke.ShowWindow(this.Handle, SW_SHOWNOACTIVATE);

            this.trackBar1.Focus();
        }

        public void HideWindow() {
            PInvoke.ShowWindow(this.Handle, 0);
        }

        public FOLDERVIEWMODE ViewMode {
            get {
                switch(this.trackBar1.Value) {
                    case 5:
                        return FOLDERVIEWMODE.FVM_THUMBSTRIP;
                    case 4:
                        return FOLDERVIEWMODE.FVM_THUMBNAIL;
                    case 3:
                        return FOLDERVIEWMODE.FVM_TILE;
                    case 1:
                        return FOLDERVIEWMODE.FVM_LIST;
                    case 0:
                        return FOLDERVIEWMODE.FVM_DETAILS;
                    default:
                        return FOLDERVIEWMODE.FVM_ICON;
                }
            }
        }

        protected override CreateParams CreateParams {
            get {
                const int CS_DROPSHADOW = 0x00020000;

                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;

                return cp;
            }
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            using(Pen p = new Pen(this.clrBorder)) {
                e.Graphics.DrawRectangle(p, new Rectangle(0, 0, this.Width - 1, this.Height - 1));
            }
        }



        private void ChangeLabelsState() {
            this.labelDETAIL.ForeColor =
            this.labelLIST.ForeColor =
            this.labelICON.ForeColor =
            this.labelTILE.ForeColor =
            this.labelTHUMBNAIL.ForeColor =
            this.labelTHUMBSTRIP.ForeColor = SystemColors.GrayText;

            switch(this.trackBar1.Value) {
                case 0:
                    this.labelDETAIL.ForeColor = SystemColors.ControlText;
                    break;
                case 1:
                    this.labelLIST.ForeColor = SystemColors.ControlText;
                    break;
                case 2:
                    this.labelICON.ForeColor = SystemColors.ControlText;
                    break;
                case 3:
                    this.labelTILE.ForeColor = SystemColors.ControlText;
                    break;
                case 4:
                    this.labelTHUMBNAIL.ForeColor = SystemColors.ControlText;
                    break;
                case 5:
                    this.labelTHUMBSTRIP.ForeColor = SystemColors.ControlText;
                    break;
            }
        }

        private static int ModeToInt(FOLDERVIEWMODE fvm) {
            switch(fvm) {
                case FOLDERVIEWMODE.FVM_THUMBSTRIP:
                    return 5;
                case FOLDERVIEWMODE.FVM_THUMBNAIL:
                    return 4;
                case FOLDERVIEWMODE.FVM_TILE:
                    return 3;
                case FOLDERVIEWMODE.FVM_LIST:
                    return 1;
                case FOLDERVIEWMODE.FVM_DETAILS:
                    return 0;
                default:
                    return 2;
            }
        }

        private RECT GetThumbRect() {
            RECT rct = new RECT();

            if(this.trackBar1.IsHandleCreated) {
                const int WM_USER = 0x0400;
                const int TBM_GETTHUMBRECT = (WM_USER + 25);

                PInvoke.SendMessage(this.trackBar1.Handle, TBM_GETTHUMBRECT, IntPtr.Zero, ref rct);
            }
            return rct;
        }


        private void trackBar1_LostFocus(object sender, EventArgs e) {
            if(fFirstFocusing) {
                this.HideWindow();
                this.fFirstFocusing = false;
            }
            else
                this.fFirstFocusing = true;
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e) {
            if(this.ViewModeChanged != null)
                this.ViewModeChanged(this, e);

            this.ChangeLabelsState();
        }

        private void trackBar1_KeyDown(object sender, KeyEventArgs e) {
            if(e.KeyCode == Keys.Escape) {
                this.HideWindow();
            }
        }

        private void labelButtons_Click(object sender, EventArgs e) {
            FOLDERVIEWMODE mode = (FOLDERVIEWMODE)((LabelEx)sender).Tag;

            this.trackBar1.Value = FolderViewModeWindow.ModeToInt(mode);
        }
    }

    sealed class LabelEx : Label {
        private bool fMouseOn;
        private VisualStyleRenderer vsrNormal, vsrPressed;
        private Image image;

        public LabelEx() {
            this.Size = new Size(107, 28);
            this.Padding = new Padding(24, 0, 4, 0);
            this.Margin = Padding.Empty;
            this.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        }

        protected override void Dispose(bool disposing) {
            this.image = null;
            base.Dispose(disposing);
        }

        protected override void OnMouseEnter(EventArgs e) {
            this.fMouseOn = true;
            base.OnMouseEnter(e);
            this.Invalidate();
        }
        protected override void OnMouseLeave(EventArgs e) {
            this.fMouseOn = false;
            base.OnMouseLeave(e);
            this.Invalidate();
        }
        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);
            this.Invalidate();
        }
        protected override void OnMouseUp(MouseEventArgs e) {
            base.OnMouseUp(e);
            this.Invalidate();
        }


        protected override void OnPaintBackground(PaintEventArgs e) {
            if(this.fMouseOn) {
                MouseButtons mb = Control.MouseButtons;

                if(VisualStyleRenderer.IsSupported) {
                    if(vsrNormal == null)
                        vsrNormal = new VisualStyleRenderer(VisualStyleElement.Button.PushButton.Normal);
                    if(vsrPressed == null)
                        vsrPressed = new VisualStyleRenderer(VisualStyleElement.Button.PushButton.Pressed);

                    if(mb == MouseButtons.Left)
                        vsrPressed.DrawBackground(e.Graphics, e.ClipRectangle);
                    else
                        vsrNormal.DrawBackground(e.Graphics, e.ClipRectangle);
                }
                else {
                    ControlPaint.DrawButton(e.Graphics, e.ClipRectangle, mb == MouseButtons.Left ? ButtonState.Pushed : ButtonState.Normal);
                }
            }
            else
                base.OnPaintBackground(e);

            if(this.image != null) {
                e.Graphics.DrawImage(this.image, new Rectangle(6, 6, 16, 16));
            }
        }

        public Image Image2 {
            get {
                return this.image;
            }
            set {
                this.image = value;
            }
        }
    }

    sealed class TrackBarEx : TrackBar {
        private int cumulativeWheelData;

        protected override void OnMouseWheel(MouseEventArgs e) {
            HandledMouseEventArgs args = e as HandledMouseEventArgs;
            if(args != null) {
                if(args.Handled) {
                    return;
                }
                args.Handled = true;
            }
            if(((Control.ModifierKeys & (Keys.Alt | Keys.Shift)) == Keys.None) && (Control.MouseButtons == MouseButtons.None)) {
                this.cumulativeWheelData += e.Delta;
                int num3 = (int)(((float)this.cumulativeWheelData) / 120f);
                if(num3 != 0) {
                    if(num3 > 0) {
                        this.Value = Math.Min(num3 + this.Value, this.Maximum);
                        this.cumulativeWheelData -= (int)(num3 * 120f);
                    }
                    else {
                        this.Value = Math.Max(num3 + this.Value, this.Minimum);
                        this.cumulativeWheelData -= (int)(num3 * 120f);
                    }
                }
                if(e.Delta != this.Value) {
                    this.OnScroll(EventArgs.Empty);
                    this.OnValueChanged(EventArgs.Empty);
                }
            }
        }
    }

}