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
    class RBBS {
        public const int BREAK =           0x0001;  // break to new line
        public const int FIXEDSIZE =       0x0002;  // band can't be sized
        public const int CHILDEDGE =       0x0004;  // edge around top & bottom of child window
        public const int HIDDEN =          0x0008;  // don't show
        public const int NOVERT =          0x0010;  // don't show when vertical
        public const int FIXEDBMP =        0x0020;  // bitmap doesn't move during band resize
        public const int VARIABLEHEIGHT =  0x0040;  // allow autosizing of this child vertically
        public const int GRIPPERALWAYS =   0x0080;  // always show the gripper
        public const int NOGRIPPER =       0x0100;  // never show the gripper
        public const int USECHEVRON =      0x0200;  // display drop-down button for this band if it's sized smaller than ideal width
        public const int HIDETITLE =       0x0400;  // keep band title hidden
        public const int TOPALIGN =        0x0800;  // keep band in top row
    }
}
