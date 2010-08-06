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

using System.Runtime.InteropServices;

namespace QTTabBarLib.Interop {
    [StructLayout(LayoutKind.Sequential)]
    public struct FOLDERSETTINGS {
        public int ViewMode;
        public int fFlags;
    }

    public static class FVM {
        public const int AUTO       = -1;
        public const int FIRST      = 1;
        public const int ICON       = 1;
        public const int SMALLICON  = 2;
        public const int LIST       = 3;
        public const int DETAILS    = 4;
        public const int THUMBNAIL  = 5;
        public const int TILE       = 6;
        public const int THUMBSTRIP = 7;
        public const int CONTENT    = 8;
        public const int LAST       = 8;
    }

    public class FWF {
        public const int NONE                  = 0x00000000;
        public const int AUTOARRANGE           = 0x00000001;
        public const int ABBREVIATEDNAMES      = 0x00000002;
        public const int SNAPTOGRID            = 0x00000004;
        public const int OWNERDATA             = 0x00000008;
        public const int BESTFITWINDOW         = 0x00000010;
        public const int DESKTOP               = 0x00000020;
        public const int SINGLESEL             = 0x00000040;
        public const int NOSUBFOLDERS          = 0x00000080;
        public const int TRANSPARENT           = 0x00000100;
        public const int NOCLIENTEDGE          = 0x00000200;
        public const int NOSCROLL              = 0x00000400;
        public const int ALIGNLEFT             = 0x00000800;
        public const int NOICONS               = 0x00001000;
        public const int SHOWSELALWAYS         = 0x00002000;
        public const int NOVISIBLE             = 0x00004000;
        public const int SINGLECLICKACTIVATE   = 0x00008000;
        public const int NOWEBVIEW             = 0x00010000;
        public const int HIDEFILENAMES         = 0x00020000;
        public const int CHECKSELECT           = 0x00040000;
        public const int NOENUMREFRESH         = 0x00080000;
        public const int NOGROUPING            = 0x00100000;
        public const int FULLROWSELECT         = 0x00200000;
        public const int NOFILTERS             = 0x00400000;
        public const int NOCOLUMNHEADER        = 0x00800000;
        public const int NOHEADERINALLVIEWS    = 0x01000000;
        public const int EXTENDEDTILES         = 0x02000000;
        public const int TRICHECKSELECT        = 0x04000000;
        public const int AUTOCHECKSELECT       = 0x08000000;
        public const int NOBROWSERVIEWSTATE    = 0x10000000;
        public const int SUBSETGROUPS          = 0x20000000;
        public const int USESEARCHFOLDER       = 0x40000000;
        public const uint ALLOWRTLREADING      = 0x80000000;
    }
}
