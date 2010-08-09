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

using System.Drawing;
using System.Windows.Forms.VisualStyles;

namespace QTTabBarLib {
    internal class QTabItemBase {
        private string commentText = string.Empty;
        private Edges edge;
        private static Font font;
        private static Font fontSubText;
        private bool fUnderLine;
        private string imageKey = string.Empty;
        private int iRow;
        protected QTabControl Owner;
        private static RectangleF rctMeasure = new RectangleF(0f, 0f, 512f, 50f);
        internal const string SEPARATOR_COMMENT = ": ";
        internal const string SEPARATOR_SUBTEXT = "@ ";
        private static StringFormat sfMeasure;
        private SizeF sizeSubTitle;
        private SizeF sizeTitle;
        private Rectangle tabBounds;
        protected bool tabLocked;
        private string titleText;
        private string tooltipText = string.Empty;

        public QTabItemBase(string title, QTabControl parent) {
            titleText = title;
            Owner = parent;
            if(font == null) {
                font = new Font(parent.Font, FontStyle.Bold);
            }
            if(fontSubText == null) {
                float sizeInPoints = font.SizeInPoints;
                fontSubText = new Font(parent.Font.FontFamily, (sizeInPoints > 8.25f) ? (sizeInPoints - 0.75f) : sizeInPoints, FontStyle.Bold);
            }
            if(sfMeasure == null) {
                sfMeasure = new StringFormat();
                sfMeasure.FormatFlags |= StringFormatFlags.NoWrap;
            }
            if(title.Length > 0) {
                RefreshRectangle();
            }
            else {
                tabBounds = new Rectangle(tabBounds.X, tabBounds.Y, 100, 0x18);
            }
        }

        private static SizeF GetTextSize(string str, Graphics g, bool fTitle) {
            SizeF empty;
            CharacterRange[] ranges = new CharacterRange[] { new CharacterRange(0, str.Length) };
            sfMeasure.SetMeasurableCharacterRanges(ranges);
            Region[] regionArray = g.MeasureCharacterRanges(str, fTitle ? font : fontSubText, rctMeasure, sfMeasure);
            using(regionArray[0]) {
                empty = regionArray[0].GetBounds(g).Size;
                empty.Width += 6f;
            }
            return empty;
        }

        public virtual void OnClose() {
            Owner = null;
        }

        public void RefreshRectangle() {
            using(Graphics graphics = Owner.CreateGraphics()) {
                float num = 6f;
                if(Owner.DrawFolderImage) {
                    num = 26f;
                }
                else if(tabLocked) {
                    num = 13f;
                }
                if((Owner.EnableCloseButton && !Owner.TabCloseButtonOnHover) && !Owner.TabCloseButtonOnAlt) {
                    num += 17f;
                }
                sizeTitle = GetTextSize(titleText, graphics, true);
                sizeSubTitle = SizeF.Empty;
                if(commentText.Length > 0) {
                    sizeSubTitle = GetTextSize("@ " + commentText, graphics, false);
                    num += sizeSubTitle.Width + 3f;
                }
                else {
                    num++;
                }
                int width = (int)(sizeTitle.Width + num);
                if(Owner.OncePainted && (width > Owner.Width)) {
                    if(Owner.Width > 40) {
                        width = Owner.Width - 40;
                    }
                    else {
                        width = 0x20;
                    }
                }
                tabBounds = new Rectangle(tabBounds.X, tabBounds.Y, width, (int)sizeTitle.Height);
            }
        }

        public void ResetOwner(QTabControl owner) {
            Owner = owner;
            owner.TabPages.Add(this);
        }

        public string Comment {
            get {
                return commentText;
            }
            set {
                commentText = value ?? string.Empty;
            }
        }

        public Edges Edge {
            get {
                return edge;
            }
            set {
                edge = value;
            }
        }

        public string ImageKey {
            get {
                return imageKey;
            }
            set {
                if(((Owner != null) && Owner.DrawFolderImage) && !string.IsNullOrEmpty(value)) {
                    imageKey = QTUtility.GetImageKey(value, null);
                }
                else {
                    imageKey = value;
                }
            }
        }

        public int Row {
            get {
                return iRow;
            }
            set {
                iRow = value;
            }
        }

        public SizeF SubTitleTextSize {
            get {
                return sizeSubTitle;
            }
        }

        public Rectangle TabBounds {
            get {
                return tabBounds;
            }
            set {
                tabBounds = value;
            }
        }

        public static Font TabFont {
            set {
                if(font != null) {
                    font.Dispose();
                }
                font = new Font(value, FontStyle.Regular);
                if(fontSubText != null) {
                    fontSubText.Dispose();
                }
                float sizeInPoints = font.SizeInPoints;
                fontSubText = new Font(value.FontFamily, (sizeInPoints > 8.25f) ? (sizeInPoints - 0.75f) : sizeInPoints, FontStyle.Regular);
            }
        }

        public bool TabLocked {
            get {
                return tabLocked;
            }
            set {
                tabLocked = value;
                RefreshRectangle();
                if(Owner != null) {
                    Owner.Refresh();
                }
            }
        }

        public string Text {
            get {
                return titleText;
            }
            set {
                titleText = value;
                RefreshRectangle();
                if(Owner != null) {
                    Owner.Refresh();
                }
            }
        }

        public SizeF TitleTextSize {
            get {
                return sizeTitle;
            }
        }

        public string ToolTipText {
            get {
                return tooltipText;
            }
            set {
                tooltipText = value;
            }
        }

        public bool UnderLine {
            get {
                return fUnderLine;
            }
            set {
                fUnderLine = value;
            }
        }
    }
}
