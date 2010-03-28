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
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.Resources;
    using System.Runtime.CompilerServices;

    [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0"), DebuggerNonUserCode, CompilerGenerated]
    internal class Resources_String {
        private static CultureInfo resourceCulture;
        private static System.Resources.ResourceManager resourceMan;

        internal Resources_String() {
        }

        internal static string ButtonBar_BtnName {
            get {
                return ResourceManager.GetString("ButtonBar_BtnName", resourceCulture);
            }
        }

        internal static string ButtonBar_Misc {
            get {
                return ResourceManager.GetString("ButtonBar_Misc", resourceCulture);
            }
        }

        internal static string ButtonBar_Option {
            get {
                return ResourceManager.GetString("ButtonBar_Option", resourceCulture);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }

        internal static string DialogButtons {
            get {
                return ResourceManager.GetString("DialogButtons", resourceCulture);
            }
        }

        internal static string DragDropToolTip {
            get {
                return ResourceManager.GetString("DragDropToolTip", resourceCulture);
            }
        }

        internal static string Misc_Strings {
            get {
                return ResourceManager.GetString("Misc_Strings", resourceCulture);
            }
        }

        internal static Bitmap paypalBtn {
            get {
                return (Bitmap)ResourceManager.GetObject("paypalBtn", resourceCulture);
            }
        }

        internal static string PayPalURL {
            get {
                return ResourceManager.GetString("PayPalURL", resourceCulture);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static System.Resources.ResourceManager ResourceManager {
            get {
                if(object.ReferenceEquals(resourceMan, null)) {
                    System.Resources.ResourceManager manager = new System.Resources.ResourceManager("QTTabBarLib.Resources_String", typeof(Resources_String).Assembly);
                    resourceMan = manager;
                }
                return resourceMan;
            }
        }

        internal static string ShortcutKeys_ActionNames {
            get {
                return ResourceManager.GetString("ShortcutKeys_ActionNames", resourceCulture);
            }
        }

        internal static string ShortcutKeys_Groups {
            get {
                return ResourceManager.GetString("ShortcutKeys_Groups", resourceCulture);
            }
        }

        internal static string ShortcutKeys_MsgReassign {
            get {
                return ResourceManager.GetString("ShortcutKeys_MsgReassign", resourceCulture);
            }
        }

        internal static string SiteURL {
            get {
                return ResourceManager.GetString("SiteURL", resourceCulture);
            }
        }

        internal static string TabBar_Menu {
            get {
                return ResourceManager.GetString("TabBar_Menu", resourceCulture);
            }
        }

        internal static string TabBar_Message {
            get {
                return ResourceManager.GetString("TabBar_Message", resourceCulture);
            }
        }

        internal static string TabBar_NewGroup {
            get {
                return ResourceManager.GetString("TabBar_NewGroup", resourceCulture);
            }
        }

        internal static string TabBar_Option {
            get {
                return ResourceManager.GetString("TabBar_Option", resourceCulture);
            }
        }

        internal static string TabBar_Option_Buttons {
            get {
                return ResourceManager.GetString("TabBar_Option_Buttons", resourceCulture);
            }
        }

        internal static string TabBar_Option_DropDown {
            get {
                return ResourceManager.GetString("TabBar_Option_DropDown", resourceCulture);
            }
        }

        internal static string TabBar_Option_Genre {
            get {
                return ResourceManager.GetString("TabBar_Option_Genre", resourceCulture);
            }
        }

        internal static string TabBar_Option2 {
            get {
                return ResourceManager.GetString("TabBar_Option2", resourceCulture);
            }
        }

        internal static string TaskBar_Menu {
            get {
                return ResourceManager.GetString("TaskBar_Menu", resourceCulture);
            }
        }

        internal static string TaskBar_Titles {
            get {
                return ResourceManager.GetString("TaskBar_Titles", resourceCulture);
            }
        }

        internal static string UpdateCheck {
            get {
                return ResourceManager.GetString("UpdateCheck", resourceCulture);
            }
        }
    }
}
