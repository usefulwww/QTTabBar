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
using System.Drawing;
using System.Windows.Forms;

namespace QTTabBarLib {
    internal sealed class DataGridViewProgressBarColumn : DataGridViewColumn {
        private List<DataGridViewProgressBarCell> lstCells;
        private static string VALUE_WAITING = "           Waiting...           ";

        public DataGridViewProgressBarColumn()
            : base(new DataGridViewProgressBarCell()) {
            this.lstCells = new List<DataGridViewProgressBarCell>();
            base.SortMode = DataGridViewColumnSortMode.Automatic;
        }

        public void FinishProgress(DataGridViewProgressBarCell cell) {
            if(cell.CalculatingStatus == HashCalcStatus.Calculating) {
                cell.CalculatingStatus = HashCalcStatus.Finished;
            }
            cell.EndProgress();
            this.lstCells.Remove(cell);
        }

        public void InitializeProgress(DataGridViewProgressBarCell cell) {
            if(cell.ColumnIndex != base.Index) {
                throw new ArgumentException("cell is not contained in this column.");
            }
            if(!this.lstCells.Contains(cell)) {
                cell.OldStyle = cell.Style.Clone();
                cell.Value = VALUE_WAITING;
                cell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                cell.Style.ForeColor = cell.Style.SelectionForeColor = SystemColors.GrayText;
                cell.CalculatingStatus = HashCalcStatus.Calculating;
                this.lstCells.Add(cell);
            }
        }

        public void StopAll() {
            foreach(DataGridViewProgressBarCell cell in this.lstCells) {
                if(cell.CalculatingStatus != HashCalcStatus.Finished) {
                    cell.CalculatingStatus = HashCalcStatus.Aborted;
                }
                cell.EndProgress();
            }
            this.lstCells.Clear();
        }
    }
}
