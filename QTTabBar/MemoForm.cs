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
    using System;
    using System.Collections.Generic;
    using System.Drawing;

    public sealed class MemoForm {
        [Serializable]
        public sealed class MemoStore {
            private Rectangle _bounds;
            private Dictionary<string, string> _dic;
            private double _opacity;
            private Dictionary<string, string> _txtDic;

            public Rectangle Bounds {
                get {
                    return this._bounds;
                }
                set {
                    this._bounds = value;
                }
            }

            public Dictionary<string, string> Dictionary {
                get {
                    return this._dic;
                }
                set {
                    this._dic = value;
                }
            }

            public double Opacity {
                get {
                    return this._opacity;
                }
                set {
                    this._opacity = value;
                }
            }

            public Dictionary<string, string> TxtDictionary {
                get {
                    return this._txtDic;
                }
                set {
                    this._txtDic = value;
                }
            }
        }
    }
}
