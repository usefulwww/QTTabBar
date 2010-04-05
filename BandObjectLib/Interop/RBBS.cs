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
