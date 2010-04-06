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
using System.Collections.Generic;
using System.Text;

namespace BandObjectLib {
    class RBBIM {
        public const int STYLE =          0x0001;
        public const int COLORS =         0x0002;
        public const int TEXT =           0x0004;
        public const int IMAGE =          0x0008;
        public const int CHILD =          0x0010;
        public const int CHILDSIZE =      0x0020;
        public const int SIZE =           0x0040;
        public const int BACKGROUND =     0x0080;
        public const int ID =             0x0100;
        public const int IDEALSIZE =      0x0200;
        public const int LPARAM =         0x0400;
        public const int HEADERSIZE =     0x0800; // control the size of the header
        public const int CHEVRONLOCATION =0x1000;
        public const int CHEVRONSTATE =   0x2000;
    }
}
