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

namespace QTPlugin {
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Runtime.InteropServices;

    public class TabBarOption {
        private Dictionary<int, object> dic;

        public TabBarOption() {
            this.dic = new Dictionary<int, object>();
        }

        public TabBarOption(Dictionary<int, object> option_value_pairs) {
            this.dic = new Dictionary<int, object>();
            this.dic = option_value_pairs;
        }

        public void SetValue(TabBar_Options_Boolean option, bool value) {
            this.dic[(int)option] = value;
        }

        public void SetValue(TabBar_Options_Color option, Color value) {
            this.dic[(int)option] = value;
        }

        public void SetValue(TabBar_Options_Int option, int value) {
            this.dic[(int)option] = value;
        }

        public void SetValue(TabBar_Options_Misc option, object value) {
            this.dic[(int)option] = value;
        }

        public void SetValue(TabBar_Options_String option, string value) {
            if(value == null) {
                value = string.Empty;
            }
            this.dic[(int)option] = value;
        }

        public void SetValues(Dictionary<int, object> option_value_pairs) {
            foreach(int num in option_value_pairs.Keys) {
                this.dic[num] = option_value_pairs[num];
            }
        }

        public bool TryGetValue(TabBar_Options_Boolean option, out bool value) {
            object obj2;
            if(this.dic.TryGetValue((int)option, out obj2) && (obj2 is bool)) {
                value = (bool)obj2;
                return true;
            }
            value = false;
            return false;
        }

        public bool TryGetValue(TabBar_Options_Color option, out Color value) {
            object obj2;
            if(this.dic.TryGetValue((int)option, out obj2) && (obj2 is Color)) {
                value = (Color)obj2;
                return true;
            }
            value = Color.Empty;
            return false;
        }

        public bool TryGetValue(TabBar_Options_Int option, out int value) {
            object obj2;
            if(this.dic.TryGetValue((int)option, out obj2) && (obj2 is int)) {
                value = (int)obj2;
                return true;
            }
            value = 0;
            return false;
        }

        public bool TryGetValue(TabBar_Options_Misc option, out object value) {
            return this.dic.TryGetValue((int)option, out value);
        }

        public bool TryGetValue(TabBar_Options_String option, out string value) {
            object obj2;
            if(this.dic.TryGetValue((int)option, out obj2) && (obj2 is string)) {
                value = (string)obj2;
                return true;
            }
            value = null;
            return false;
        }

        public Dictionary<int, object> Dictionary {
            get {
                return this.dic;
            }
        }
    }
}
