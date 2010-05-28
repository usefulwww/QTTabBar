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
    using System.Drawing;
    using System.Windows.Forms.VisualStyles;

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
            this.titleText = title;
            this.Owner = parent;
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
                this.RefreshRectangle();
            }
            else {
                this.tabBounds = new Rectangle(this.tabBounds.X, this.tabBounds.Y, 100, 0x18);
            }
        }

        private static SizeF GetTextSize(string str, Graphics g, bool fTitle) {
            SizeF empty = SizeF.Empty;
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
            this.Owner = null;
        }

        public void RefreshRectangle() {
            using(Graphics graphics = this.Owner.CreateGraphics()) {
                float num = 6f;
                if(this.Owner.DrawFolderImage) {
                    num = 26f;
                }
                else if(this.tabLocked) {
                    num = 13f;
                }
                if((this.Owner.EnableCloseButton && !this.Owner.TabCloseButtonOnHover) && !this.Owner.TabCloseButtonOnAlt) {
                    num += 17f;
                }
                this.sizeTitle = GetTextSize(this.titleText, graphics, true);
                this.sizeSubTitle = SizeF.Empty;
                if(this.commentText.Length > 0) {
                    this.sizeSubTitle = GetTextSize("@ " + this.commentText, graphics, false);
                    num += this.sizeSubTitle.Width + 3f;
                }
                else {
                    num++;
                }
                int width = (int)(this.sizeTitle.Width + num);
                if(this.Owner.OncePainted && (width > this.Owner.Width)) {
                    if(this.Owner.Width > 40) {
                        width = this.Owner.Width - 40;
                    }
                    else {
                        width = 0x20;
                    }
                }
                this.tabBounds = new Rectangle(this.tabBounds.X, this.tabBounds.Y, width, (int)this.sizeTitle.Height);
            }
        }

        public void ResetOwner(QTabControl owner) {
            this.Owner = owner;
            owner.TabPages.Add(this);
        }

        public string Comment {
            get {
                return this.commentText;
            }
            set {
                if(value == null) {
                    this.commentText = string.Empty;
                }
                else {
                    this.commentText = value;
                }
            }
        }

        public Edges Edge {
            get {
                return this.edge;
            }
            set {
                this.edge = value;
            }
        }

        public string ImageKey {
            get {
                return this.imageKey;
            }
            set {
                if(((this.Owner != null) && this.Owner.DrawFolderImage) && !string.IsNullOrEmpty(value)) {
                    this.imageKey = QTUtility.GetImageKey(value, null);
                }
                else {
                    this.imageKey = value;
                }
            }
        }

        public int Row {
            get {
                return this.iRow;
            }
            set {
                this.iRow = value;
            }
        }

        public SizeF SubTitleTextSize {
            get {
                return this.sizeSubTitle;
            }
        }

        public Rectangle TabBounds {
            get {
                return this.tabBounds;
            }
            set {
                this.tabBounds = value;
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
                return this.tabLocked;
            }
            set {
                this.tabLocked = value;
                this.RefreshRectangle();
                if(this.Owner != null) {
                    this.Owner.Refresh();
                }
            }
        }

        public string Text {
            get {
                return this.titleText;
            }
            set {
                this.titleText = value;
                this.RefreshRectangle();
                if(this.Owner != null) {
                    this.Owner.Refresh();
                }
            }
        }

        public SizeF TitleTextSize {
            get {
                return this.sizeTitle;
            }
        }

        public string ToolTipText {
            get {
                return this.tooltipText;
            }
            set {
                this.tooltipText = value;
            }
        }

        public bool UnderLine {
            get {
                return this.fUnderLine;
            }
            set {
                this.fUnderLine = value;
            }
        }
    }
}
