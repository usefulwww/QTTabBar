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
using System.ComponentModel;
using System.Windows.Forms;

namespace QTTabBarLib {
    internal sealed class ContextMenuStripEx : ContextMenuStrip {
        private bool fDefaultShowCheckMargin;
        private static ToolStripRenderer menuRenderer;
        private static int nCurrentRenderer;

        private static event EventHandler menuRendererChanged;

        static ContextMenuStripEx() {
            InitializeMenuRenderer();
        }

        public ContextMenuStripEx(IContainer container, bool fShowCheckMargin) {
            if(container != null) {
                container.Add(this);
            }
            fDefaultShowCheckMargin = fShowCheckMargin;
            ShowCheckMargin = fShowCheckMargin || (nCurrentRenderer == 2);
            Renderer = menuRenderer;
            menuRendererChanged = (EventHandler)Delegate.Combine(menuRendererChanged, new EventHandler(ContextMenuStripEx_menuRendererChanged));
        }

        private void ContextMenuStripEx_menuRendererChanged(object sender, EventArgs e) {
            if(InvokeRequired) {
                Invoke(new MethodInvoker(SetRenderer));
            }
            else {
                SetRenderer();
            }
        }

        protected override void Dispose(bool disposing) {
            menuRendererChanged = (EventHandler)Delegate.Remove(menuRendererChanged, new EventHandler(ContextMenuStripEx_menuRendererChanged));
            base.Dispose(disposing);
        }

        public void EnsureHandleCreated() {
            if(!IsHandleCreated) {
                CreateHandle();
            }
        }

        public static void InitializeMenuRenderer() {
            bool flag = false;
            if(Config.NonDefaultMenu) {
                if(Config.XPStyleMenus) {
                    if(nCurrentRenderer != 1) {
                        menuRenderer = new XPMenuRenderer(false);
                        nCurrentRenderer = 1;
                        flag = true;
                    }
                }
                else if(nCurrentRenderer != 2) {
                    menuRenderer = new VistaMenuRenderer(false);
                    nCurrentRenderer = 2;
                    flag = true;
                }
            }
            else if(nCurrentRenderer != 0) {
                menuRenderer = null;
                nCurrentRenderer = 0;
                flag = true;
            }
            if(flag && (menuRendererChanged != null)) {
                menuRendererChanged(null, EventArgs.Empty);
            }
        }

        private void SetRenderer() {
            Renderer = menuRenderer;
            ShowCheckMargin = (nCurrentRenderer == 2) || fDefaultShowCheckMargin;
        }
    }
}
