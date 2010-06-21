//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2010  Quizo, Paul Accisano
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
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using QTPlugin.Interop;

namespace QuizoPlugins {
    public partial class SettingWindow : Form {
        private IntPtr hwndExplorer;
        private Dictionary<string, Rectangle> dicPresets;
        private string startingPreset;

        public SettingWindow(Rectangle rctInitial, byte[] config, int delta_RESIZE, IntPtr hwnd, Dictionary<string, Rectangle> dicPresets, string startingPreset) {
            InitializeComponent();


            if(System.Globalization.CultureInfo.CurrentCulture.Parent.Name == "ja") {
                string[] strs = Resource.ResStrs_Options_ja.Split(new char[] { ';' });

                this.chbInitialSize.Text = strs[0];
                this.chbInitialLoc.Text = strs[1];
                this.buttonRestoreSize.Text = strs[2];
                this.buttonRestoreLoc.Text = strs[3];
                this.checkBoxResizeMode.Text = strs[4];
                this.labelDELTARESIZE.Text = strs[5];
                this.groupBoxPresets.Text = strs[6];
                this.buttonSet.Text = strs[7];
                this.buttonDel.Text = strs[8];
                this.buttonOK.Text = strs[9];
                this.buttonCancel.Text = strs[10];
                this.buttonGetCurLoc.Text = strs[11];
                this.buttonGetCurSize.Text = strs[12];
                this.chbStartingPreset.Text = strs[13];
                this.buttonGetCurrentToPreset.Text = strs[14];
            }


            this.hwndExplorer = hwnd;
            this.dicPresets = new Dictionary<string, Rectangle>(dicPresets);
            this.startingPreset = startingPreset;

            Rectangle rctScreen = Screen.FromHandle(hwnd).Bounds;
            this.nudInitialW.Maximum = rctScreen.Width;
            this.nudInitialH.Maximum = rctScreen.Height;

            RECT rct;
            PInvoke_QTWM.GetWindowRect(hwnd, out rct);
            this.Text += " ( " + rct.left + ", " + rct.top + " )  " + rct.Width + " x " + rct.Height;

            try {
                if((config[0] & 0x80) != 0) {
                    this.chbInitialSize.Checked = true;
                }

                if((config[0] & 0x40) != 0) {
                    this.checkBoxResizeMode.Checked = false;
                }

                if((config[0] & 0x20) != 0) {
                    this.chbInitialLoc.Checked = true;
                }

                if((config[0] & 0x10) != 0) {
                    this.chbStartingPreset.Checked = true;
                }

                if((config[0] & 0x08) != 0) {
                    this.chbRestoreClosedRct.Checked = true;
                }


                this.nudInitialX.Value = rctInitial.X;
                this.nudInitialY.Value = rctInitial.Y;
                this.nudInitialW.Value = rctInitial.Width;
                this.nudInitialH.Value = rctInitial.Height;

                if(delta_RESIZE < 33 && delta_RESIZE > 0)
                    this.nudDelta.Value = delta_RESIZE;

                this.chbInitialLoc_CheckedChanged(null, EventArgs.Empty);
                this.chbInitialSize_CheckedChanged(null, EventArgs.Empty);

                if(this.chbStartingPreset.Checked) {
                    this.nudInitialX.Enabled = this.nudInitialY.Enabled = this.chbInitialLoc.Enabled =
                    this.buttonRestoreLoc.Enabled = this.buttonGetCurLoc.Enabled =
                    this.buttonRestoreSize.Enabled = this.buttonGetCurSize.Enabled =
                    this.nudInitialW.Enabled = this.nudInitialH.Enabled = this.chbInitialSize.Enabled = false;
                }
                else {
                    this.comboBox2.Enabled = false;
                }
            }
            catch {
            }

            foreach(string name in this.dicPresets.Keys) {
                this.comboBox1.Items.Add(name);
                this.comboBox2.Items.Add(name);
            }

            if(this.startingPreset != null && this.startingPreset.Length > 0) {
                int indexStartingPreset = this.comboBox2.Items.IndexOf(this.startingPreset);
                if(indexStartingPreset != -1) {
                    this.comboBox2.SelectedIndex = indexStartingPreset;
                }
            }

            if(this.comboBox1.Items.Count > 0)
                this.comboBox1.SelectedIndex = 0;
        }


        public Point InitialLocation {
            get {
                return new Point((int)this.nudInitialX.Value, (int)this.nudInitialY.Value);
            }
        }

        public Size InitialSize {
            get {
                return new Size((int)this.nudInitialW.Value, (int)this.nudInitialH.Value);
            }
        }

        public int ResizeDelta {
            get {
                return (int)this.nudDelta.Value;
            }
        }

        public byte[] ConfigValues {
            get {
                byte[] config = new byte[] { 0, 0, 0, 0 };

                if(this.chbInitialSize.Checked)
                    config[0] |= 0x80;

                if(!this.checkBoxResizeMode.Checked)
                    config[0] |= 0x40;

                if(this.chbInitialLoc.Checked)
                    config[0] |= 0x20;

                if(this.chbStartingPreset.Checked)
                    config[0] |= 0x10;

                return config;
            }
        }

        public Dictionary<string, Rectangle> Presets {
            get {
                return this.dicPresets;
            }
        }

        public string StartingPreset {
            get {
                if(this.comboBox2.SelectedItem != null)
                    return this.comboBox2.SelectedItem.ToString();
                else
                    return String.Empty;
            }
        }

        private void chbInitialLoc_CheckedChanged(object sender, EventArgs e) {
            this.nudInitialX.Enabled = this.nudInitialY.Enabled =
            this.buttonRestoreLoc.Enabled = this.buttonGetCurLoc.Enabled =
            this.chbInitialLoc.Checked;
        }

        private void chbInitialSize_CheckedChanged(object sender, EventArgs e) {
            this.nudInitialW.Enabled = this.nudInitialH.Enabled =
            this.buttonRestoreSize.Enabled = this.buttonGetCurSize.Enabled =
            this.chbInitialSize.Checked;
        }

        private void chbStartingPreset_CheckedChanged(object sender, EventArgs e) {
            this.nudInitialX.Enabled = this.nudInitialY.Enabled = this.chbInitialLoc.Enabled =
            this.buttonRestoreLoc.Enabled = this.buttonGetCurLoc.Enabled =
            this.buttonRestoreSize.Enabled = this.buttonGetCurSize.Enabled =
            this.nudInitialW.Enabled = this.nudInitialH.Enabled = this.chbInitialSize.Enabled = !this.chbStartingPreset.Checked;

            this.comboBox2.Enabled = this.chbStartingPreset.Checked;
        }

        private void chbRestoreClosedRct_CheckedChanged(object sender, EventArgs e) {

        }

        private void buttonRestoreLoc_Click(object sender, EventArgs e) {
            const uint SWP_NOSIZE = 0x0001;
            const uint SWP_NOZORDER = 0x0004;

            if(this.hwndExplorer != IntPtr.Zero) {
                Point pnt = this.InitialLocation;

                PInvoke_QTWM.SetWindowPos(this.hwndExplorer, IntPtr.Zero, pnt.X, pnt.Y, 0, 0, SWP_NOSIZE | SWP_NOZORDER);

                QTWindowManager.RemoveMAXIMIZE(this.hwndExplorer);
            }
        }

        private void buttonRestoreSize_Click(object sender, EventArgs e) {
            const uint SWP_NOMOVE = 0x0002;
            const uint SWP_NOZORDER = 0x0004;

            if(this.hwndExplorer != IntPtr.Zero) {
                Size size = this.InitialSize;

                PInvoke_QTWM.SetWindowPos(this.hwndExplorer, IntPtr.Zero, 0, 0, size.Width, size.Height, SWP_NOMOVE | SWP_NOZORDER);

                QTWindowManager.RemoveMAXIMIZE(this.hwndExplorer);
            }
        }

        private void buttonGetCurLoc_Click(object sender, EventArgs e) {
            RECT rct;
            PInvoke_QTWM.GetWindowRect(this.hwndExplorer, out rct);

            this.nudInitialX.Value = rct.left;
            this.nudInitialY.Value = rct.top;
        }

        private void buttonGetCurSize_Click(object sender, EventArgs e) {
            RECT rct;
            PInvoke_QTWM.GetWindowRect(this.hwndExplorer, out rct);

            this.nudInitialW.Value = rct.Width;
            this.nudInitialH.Value = rct.Height;
        }

        private void buttonGetCurrentToPreset_Click(object sender, EventArgs e) {
            RECT rct;
            PInvoke_QTWM.GetWindowRect(this.hwndExplorer, out rct);

            this.nudPresets_X.Value = rct.left;
            this.nudPresets_Y.Value = rct.top;
            this.nudPresets_W.Value = rct.Width;
            this.nudPresets_H.Value = rct.Height;
        }

        private void buttonSet_Click(object sender, EventArgs e) {
            if(this.comboBox1.SelectedIndex != -1) {
                if(this.comboBox1.SelectedItem != null) {
                    string name = this.comboBox1.SelectedItem.ToString();

                    this.dicPresets[name] = new Rectangle((int)this.nudPresets_X.Value, (int)this.nudPresets_Y.Value, (int)this.nudPresets_W.Value, (int)this.nudPresets_H.Value);

                }
            }
            else if(this.comboBox1.Text.Length > 0) {
                string name = this.comboBox1.Text;

                this.dicPresets[name] = new Rectangle((int)this.nudPresets_X.Value, (int)this.nudPresets_Y.Value, (int)this.nudPresets_W.Value, (int)this.nudPresets_H.Value);

                this.comboBox1.Items.Add(name);

            }
        }

        private void buttonDel_Click(object sender, EventArgs e) {
            if(this.comboBox1.SelectedItem != null) {
                this.dicPresets.Remove(this.comboBox1.SelectedItem.ToString());
                this.comboBox1.Items.Remove(this.comboBox1.SelectedItem);

                this.comboBox1.Text = String.Empty;

            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) {
            if(this.comboBox1.SelectedItem != null) {
                string name = this.comboBox1.SelectedItem.ToString();

                Rectangle rct;
                if(this.dicPresets.TryGetValue(name, out rct)) {
                    this.nudPresets_X.Value = rct.X;
                    this.nudPresets_Y.Value = rct.Y;
                    this.nudPresets_W.Value = rct.Width;
                    this.nudPresets_H.Value = rct.Height;
                }
            }
        }

    }
}