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
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;

    internal sealed class DataGridViewProgressBarCell : DataGridViewTextBoxCell {
        private HashCalcStatus calcStatus;
        private bool fNowCalculating;
        private bool fStopped = true;
        private long lCallbackInterval = 0x64000L;
        private long lFileSize;
        private DataGridViewCellStyle oldStyle;
        private long progressValue;
        private static string VALUE_ABORTED = "            Canceled.           ";
        private static string VALUE_EMPTY = "                                ";

        public void EndProgress() {
            this.fStopped = true;
            this.progressValue = 0L;
            if(this.calcStatus == HashCalcStatus.Aborted) {
                base.Value = VALUE_ABORTED;
                base.Style.ForeColor = Color.Red;
                base.Style.SelectionForeColor = this.oldStyle.SelectionForeColor;
            }
            else {
                base.Style = this.oldStyle;
            }
            this.Invalidate();
        }

        private void Invalidate() {
            if(base.DataGridView != null) {
                base.DataGridView.InvalidateCell(this);
            }
        }

        protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts) {
            if(this.fNowCalculating) {
                this.PaintBackGround(graphics, advancedBorderStyle, cellStyle, cellBounds);
                paintParts &= ~(DataGridViewPaintParts.ContentBackground | DataGridViewPaintParts.Background);
            }
            base.Paint(graphics, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
        }

        private void PaintBackGround(Graphics graphics, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewCellStyle cellStyle, Rectangle cellBounds) {
            Rectangle rectangle = base.BorderWidths(advancedBorderStyle);
            Rectangle rect = cellBounds;
            rect.Offset(rectangle.X, rectangle.Y);
            rect.Width -= rectangle.Right;
            rect.Height -= rectangle.Bottom;
            using(SolidBrush brush = new SolidBrush(cellStyle.BackColor)) {
                graphics.FillRectangle(brush, rect);
            }
            if(this.lFileSize <= 0L) {
                return;
            }
            Rectangle bounds = rect;
            double num = ((double)this.progressValue) / ((double)this.lFileSize);
            bounds.Width = (int)(bounds.Width * num);
            if(VisualStyleRenderer.IsSupported) {
                try {
                    new VisualStyleRenderer(VisualStyleElement.ProgressBar.Chunk.Normal).DrawBackground(graphics, bounds);
                    goto Label_00E1;
                }
                catch {
                    goto Label_00E1;
                }
            }
            using(SolidBrush brush2 = new SolidBrush(SystemColors.Highlight)) {
                graphics.FillRectangle(brush2, bounds);
            }
        Label_00E1:
            using(StringFormat format = new StringFormat()) {
                format.Alignment = format.LineAlignment = StringAlignment.Center;
                using(SolidBrush brush3 = new SolidBrush(cellStyle.ForeColor)) {
                    graphics.DrawString(((int)(num * 100.0)) + "%", cellStyle.Font, brush3, rect, format);
                }
            }
        }

        public void Progress(int value) {
            if(this.fNowCalculating) {
                if(this.fStopped) {
                    base.Value = VALUE_EMPTY;
                    this.fStopped = false;
                }
                long num = value * this.lCallbackInterval;
                if((this.progressValue + num) <= this.lFileSize) {
                    this.progressValue += num;
                    this.Invalidate();
                }
            }
        }

        protected override bool SetValue(int rowIndex, object value) {
            this.fStopped = true;
            this.progressValue = 0L;
            return base.SetValue(rowIndex, value);
        }

        public HashCalcStatus CalculatingStatus {
            get {
                return this.calcStatus;
            }
            set {
                this.fNowCalculating = value == HashCalcStatus.Calculating;
                this.calcStatus = value;
            }
        }

        public long FileSize {
            get {
                return this.lFileSize;
            }
            set {
                this.lFileSize = value;
            }
        }

        public DataGridViewCellStyle OldStyle {
            get {
                return this.oldStyle;
            }
            set {
                this.oldStyle = value;
            }
        }
    }
}
