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
using System.Runtime.InteropServices;
using System.Drawing;

namespace QTTabBarLib {
    class AutomationElement {
        private static readonly Guid IID_IUIAutomation = new Guid("{30CBE57D-D9D0-452A-AB13-7AC5AC4825EE}");
        private static readonly Guid CLSID_CUIAutomation = new Guid("{FF48DBA4-60EF-4201-AA87-54103EEF594E}");
        private static readonly Guid IID_IUIAutomationRegistrar = new Guid("{8609C4EC-4A1A-4D88-A357-5A66E060E1CF}");
        private static readonly Guid CLSID_CUIAutomationRegistrar = new Guid("{6E29FABF-9977-42D1-8D0E-CA7E61AD87E6}");
        private static readonly Guid ItemCount_Property_GUID = new Guid("{ABBF5C45-5CCC-47b7-BB4E-87CB87BBD162}");
        private static readonly Guid SelectedItemCount_Property_GUID = new Guid("{8FE316D2-0E52-460a-9C1E-48F273D470A3}");
        private static readonly Guid ItemIndex_Property_GUID = new Guid("{92A053DA-2969-4021-BF27-514CFC2E4A69}");
        
        private const int UIAutomationType_Int = 1;

        private const int UIA_SelectionItemPatternId = 10010;

        private static int UIA_ItemCountPropertyId;
        private static int UIA_SelectedCountPropertyId;
        private static int UIA_ItemIndexPropertyId;
        private const int UIA_BoundingRectanglePropertyId   = 30001;
        private const int UIA_NamePropertyId                = 30005;
        private const int UIA_AutomationIdPropertyId        = 30011;
        private const int UIA_ClassNamePropertyId           = 30012;

        private static IUIAutomation pAutomation;
        private static int instances = 0;
        private IUIAutomationElement pElement;

        static AutomationElement() {
            Guid rclsid = CLSID_CUIAutomation;
            Guid riid = IID_IUIAutomation;
            object obj = null;
            PInvoke.CoCreateInstance(ref rclsid, IntPtr.Zero, 1, ref riid, out obj);
            if(obj == null) return;
            pAutomation = obj as IUIAutomation;
            
            rclsid = CLSID_CUIAutomationRegistrar;
            riid = IID_IUIAutomationRegistrar;
            PInvoke.CoCreateInstance(ref rclsid, IntPtr.Zero, 1, ref riid, out obj);
            IUIAutomationRegistrar pRegistrar = obj as IUIAutomationRegistrar;
            try {
                UIAutomationPropertyInfo propinfo;
                propinfo = new UIAutomationPropertyInfo {
                    guid = ItemCount_Property_GUID,
                    pProgrammaticName = "ItemCount",
                    type = UIAutomationType_Int
                };
                pRegistrar.RegisterProperty(propinfo, out UIA_ItemCountPropertyId);
                propinfo = new UIAutomationPropertyInfo {
                    guid = SelectedItemCount_Property_GUID,
                    pProgrammaticName = "SelectedItemCount",
                    type = UIAutomationType_Int
                };
                pRegistrar.RegisterProperty(propinfo, out UIA_SelectedCountPropertyId);
                propinfo = new UIAutomationPropertyInfo {
                    guid = ItemIndex_Property_GUID,
                    pProgrammaticName = "ItemIndex",
                    type = UIAutomationType_Int
                };
                pRegistrar.RegisterProperty(propinfo, out UIA_ItemIndexPropertyId);   
            }
            finally {
                if(pRegistrar != null) Marshal.ReleaseComObject(pRegistrar);
            }
        }

        private AutomationElement(IUIAutomationElement elem) {
            pElement = elem;
            lock(typeof(AutomationElement)) {
                ++instances;
            }
        }

        ~AutomationElement() {
            if(pElement != null) {
                lock(typeof(AutomationElement)) {
                    --instances;
                }
                Marshal.ReleaseComObject(pElement);
                pElement = null;
            }
        }

        public AutomationElement FindMatchingChild(Predicate<AutomationElement> pred) {
            IUIAutomationTreeWalker walker;
            pAutomation.get_ControlViewWalker(out walker);
            try {
                IUIAutomationElement elem;
                walker.GetFirstChildElement(pElement, out elem);
                while(elem != null) {
                    AutomationElement ae = new AutomationElement(elem);
                    if(pred(ae)) {
                        return ae;
                    }
                    IUIAutomationElement next;
                    walker.GetNextSiblingElement(elem, out next);
                    elem = next;
                }
                return null;
            }
            finally {
                if(walker != null) Marshal.ReleaseComObject(walker);
            }
        }

        public static AutomationElement FromHandle(IntPtr hwnd) {
            IUIAutomationElement elem;
            pAutomation.ElementFromHandle(hwnd, out elem);
            return (elem == null) ? null : new AutomationElement(elem);
        }

        public static AutomationElement FromKeyboardFocus() {
            IUIAutomationElement elem;
            pAutomation.GetFocusedElement(out elem);
            return (elem == null) ? null : new AutomationElement(elem);
        }

        public static AutomationElement FromPoint(Point pt) {
            IUIAutomationElement elem;
            pAutomation.ElementFromPoint(pt, out elem);
            return (elem == null) ? null : new AutomationElement(elem);
        }

        public bool IsSelected() {
            object obj;
            pElement.GetCurrentPattern(UIA_SelectionItemPatternId, out obj);
            try {
                if(obj == null) {
                    return false;
                }
                IUIAutomationSelectionItemPattern selprov = obj as IUIAutomationSelectionItemPattern;
                bool ret;
                selprov.get_CurrentIsSelected(out ret);
                return ret;
            }
            finally {
                if(obj != null) Marshal.ReleaseComObject(obj);
            }
        }

        public string GetAutomationId() {
            object obj;
            pElement.GetCurrentPropertyValue(UIA_AutomationIdPropertyId, out obj);
            return obj.ToString();
        }

        public Rectangle GetBoundingRect() {
            object obj;
            pElement.GetCurrentPropertyValue(UIA_BoundingRectanglePropertyId, out obj);
            double[] rect = obj as double[];
            return new Rectangle((int)rect[0], (int)rect[1], (int)rect[2], (int)rect[3]);
        }

        public string GetClassName() {
            object obj;
            pElement.GetCurrentPropertyValue(UIA_ClassNamePropertyId, out obj);
            return obj.ToString();
        }

        public AutomationElement GetFirstChild() {
            IUIAutomationTreeWalker walker;
            pAutomation.get_ControlViewWalker(out walker);
            try {
                IUIAutomationElement elem;
                walker.GetFirstChildElement(pElement, out elem);
                return (elem == null) ? null : new AutomationElement(elem);
            }
            finally {
                if(walker != null) Marshal.ReleaseComObject(walker);
            }

        }

        public static int GetInstanceCount() {
            return instances;
        }

        // Only valid for ItemsView element.
        public int GetItemCount() {
            object obj;
            pElement.GetCurrentPropertyValue(UIA_ItemCountPropertyId, out obj);
            return (int)obj;
        }
        
        // Only valid for ListItem element.
        public int GetItemIndex() {
            object obj;
            pElement.GetCurrentPropertyValue(UIA_ItemIndexPropertyId, out obj);
            return (int)obj - 1;
        }

        public string GetItemName() {
            object obj;
            pElement.GetCurrentPropertyValue(UIA_NamePropertyId, out obj);
            return obj.ToString();
        }

        public AutomationElement GetNextSibling() {
            IUIAutomationTreeWalker walker;
            pAutomation.get_ControlViewWalker(out walker);
            try {
                IUIAutomationElement elem;
                walker.GetNextSiblingElement(pElement, out elem);
                return (elem == null) ? null : new AutomationElement(elem);
            }
            finally {
                if(walker != null) Marshal.ReleaseComObject(walker);
            }

        }

        public AutomationElement GetParent() {
            IUIAutomationTreeWalker walker;
            pAutomation.get_ControlViewWalker(out walker);
            try {
                IUIAutomationElement elem;
                walker.GetParentElement(pElement, out elem);
                return (elem == null) ? null : new AutomationElement(elem);
            }
            finally {
                if(walker != null) Marshal.ReleaseComObject(walker);
            }
        }

        // Only valid for ItemsView element.
        public int GetSelectedCount() {
            object obj;
            pElement.GetCurrentPropertyValue(UIA_SelectedCountPropertyId, out obj);
            return (int)obj;
        }

        static void Uninitialize() {
            if(pAutomation != null) {
                Marshal.ReleaseComObject(pAutomation);
                pAutomation = null;
            }
        }
    }
}
