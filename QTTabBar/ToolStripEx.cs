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
using System.Reflection;
using System.Windows.Forms;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    internal sealed class ToolStripEx : ToolStrip {
        private bool fMA;

        public event EventHandler MouseActivated;

        public void HideToolTip() {
            if(base.ShowItemToolTips) {
                BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Instance;
                try {
                    typeof(ToolStrip).GetMethod("UpdateToolTip", bindingAttr).Invoke(this, new object[1]);
                    Type type = Type.GetType("System.Windows.Forms.MouseHoverTimer, System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
                    PropertyInfo property = typeof(ToolStrip).GetProperty("MouseHoverTimer", bindingAttr);
                    type.GetMethod("Cancel", Type.EmptyTypes).Invoke(property.GetValue(this, null), null);
                }
                catch(Exception exception) {
                    QTUtility2.MakeErrorLog(exception, null);
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs mea) {
            if(!base.OverflowButton.DropDown.Visible) {
                foreach(ToolStripItem item in this.Items) {
                    if((item.Visible && (item is ToolStripDropDownItem)) && (((ToolStripDropDownItem)item).HasDropDownItems && ((ToolStripDropDownItem)item).DropDown.Visible)) {
                        return;
                    }
                }
                base.OnMouseMove(mea);
            }
        }

        internal void RaiseOnResize() {
            this.OnResize(EventArgs.Empty);
        }

        protected override void WndProc(ref Message m) {
            if(m.Msg == WM.MOUSEACTIVATE) {
                this.fMA = false;
                if(0x201 == QTUtility2.GET_Y_LPARAM(m.LParam)) {
                    base.WndProc(ref m);
                    if(2 == ((int)m.Result)) {
                        this.fMA = true;
                    }
                    return;
                }
            }
            else if(m.Msg == WM.LBUTTONUP) {
                if(this.fMA && (this.MouseActivated != null)) {
                    base.WndProc(ref m);
                    this.MouseActivated(this, EventArgs.Empty);
                    this.fMA = false;
                    return;
                }
                this.fMA = false;
            }
            base.WndProc(ref m);
        }

        protected override bool DefaultShowItemToolTips {
            get {
                return false;
            }
        }
    }
}
