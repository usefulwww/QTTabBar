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
