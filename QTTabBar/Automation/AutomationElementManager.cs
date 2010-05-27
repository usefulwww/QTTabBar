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

using QTTabBarLib.Interop;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Drawing;

namespace QTTabBarLib.Automation {

    class AutomationElementManager : IDisposable {
        private List<AutomationElement> elems;
        private IUIAutomation pAutomation;

        internal AutomationElementManager(IUIAutomation pAutomation) {
            elems = new List<AutomationElement>();
            this.pAutomation = pAutomation;
        }

        public AutomationElement FromHandle(IntPtr hwnd) {
            IUIAutomationElement pElement;
            pAutomation.ElementFromHandle(hwnd, out pElement);
            return pElement == null ? null : new AutomationElement(pElement, this);
        }

        public AutomationElement FromPoint(Point pt) {
            IUIAutomationElement pElement;
            pAutomation.ElementFromPoint(pt, out pElement);
            return pElement == null ? null : new AutomationElement(pElement, this);
        }

        public AutomationElement FromKeyboardFocus() {
            IUIAutomationElement pElement;
            pAutomation.GetFocusedElement(out pElement);
            return pElement == null ? null : new AutomationElement(pElement, this);
        }

        internal void AddToDisposeList(AutomationElement elem) {
            elems.Add(elem);
        }

        public IUIAutomation GetIUIAutomation() {
            return pAutomation;
        }

        public void Dispose() {
            foreach(AutomationElement elem in elems) {
                elem.Dispose();
            }
            elems.Clear();
        }
    }
}
