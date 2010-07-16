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
using QTPlugin;

namespace QTTabBarLib {
    internal sealed class ButtonBarOptionForm : Form {
        private Button buttonAdd;
        private Button buttonBrowseImage;
        private Button buttonCancel;
        private Button buttonDown;
        private string[] ButtonItemsDisplayName;
        private Button buttonOK;
        private Button buttonRemove;
        private Button buttonResetImage;
        private Button buttonRestoreDefault;
        private Button buttonUp;
        private CheckBox chbLockSearchBox;
        private ComboBox comboBoxImageSize;
        private ComboBox comboBoxImageText;
        private Dictionary<string, int> dicActivePluginMulti = new Dictionary<string, int>();
        public bool fChangedExists;
        public bool fLargeIcon;
        private static int FormHeight = 0xea;
        private ImageStrip imageStripLarge;
        private ImageStrip imageStripSmall;
        private ListBox listBoxCurrent;
        private ListBox listBoxPool;
        private PluginManager pluginManager;
        private string strImageStripPath;
        private TextBox textBoxImgPath;

        public ButtonBarOptionForm(int[] currentItemIndexes, bool fLargeIcon, string currentImagePath, PluginManager pluginManager) {
            this.InitializeComponent();
            this.fLargeIcon = fLargeIcon;
            this.pluginManager = pluginManager;
            this.ButtonItemsDisplayName = QTUtility.TextResourcesDic["ButtonBar_BtnName"];
            string[] strArray = QTUtility.TextResourcesDic["ButtonBar_Option"];
            string[] strArray2 = QTUtility.TextResourcesDic["DialogButtons"];
            this.buttonOK.Text = strArray2[0];
            this.buttonCancel.Text = strArray2[1];
            this.buttonRestoreDefault.Text = strArray[3];
            this.buttonUp.Text = strArray[4];
            this.buttonDown.Text = strArray[5];
            this.buttonBrowseImage.Text = strArray[6];
            this.buttonResetImage.Text = strArray[11];
            this.comboBoxImageSize.Items.AddRange(new string[] { strArray[7], strArray[8] });
            this.comboBoxImageText.Items.AddRange(new string[] { strArray[12], strArray[13], strArray[14] });
            this.chbLockSearchBox.Text = strArray[15];
            this.comboBoxImageSize.SelectedIndex = this.fLargeIcon ? 1 : 0;
            if((QTButtonBar.ConfigValues[0] & 0x20) == 0x20) {
                this.comboBoxImageText.SelectedIndex = ((QTButtonBar.ConfigValues[0] & 0x10) == 0x10) ? 1 : 0;
            }
            else {
                this.comboBoxImageText.SelectedIndex = 2;
            }
            this.comboBoxImageSize.SelectedIndexChanged += this.comboBoxes_ImageSizeAndText_SelectedIndexChanged;
            this.comboBoxImageText.SelectedIndexChanged += this.comboBoxes_ImageSizeAndText_SelectedIndexChanged;
            this.chbLockSearchBox.Checked = (QTButtonBar.ConfigValues[0] & 8) != 0;
            this.InitializeImages(currentImagePath, true);
            List<PluginInformation> list = new List<PluginInformation>();
            foreach(int item in currentItemIndexes) {
                if(item >= 0x10000) {
                    int count = list.Count;
                    if(PluginManager.ActivatedButtonsOrder.Count > count) {
                        string key = PluginManager.ActivatedButtonsOrder[count];
                        foreach(PluginInformation information in PluginManager.PluginInformations) {
                            if(key != information.PluginID) {
                                continue;
                            }
                            list.Add(information);
                            if(information.PluginType == PluginType.BackgroundMultiple) {
                                int num3;
                                if(this.dicActivePluginMulti.TryGetValue(key, out num3)) {
                                    this.dicActivePluginMulti[key] = num3 + 1;
                                }
                                else {
                                    this.dicActivePluginMulti[key] = 1;
                                }
                                this.listBoxCurrent.Items.Add(information.Clone(num3));
                            }
                            else {
                                this.listBoxCurrent.Items.Add(information);
                            }
                            break;
                        }
                    }
                }
                else if(item != 0x15) {
                    this.listBoxCurrent.Items.Add(item);
                }
            }
            this.listBoxPool.Items.Add(0);
            for(int j = 1; j < this.ButtonItemsDisplayName.Length; j++) {
                if(Array.IndexOf(currentItemIndexes, j) == -1) {
                    this.listBoxPool.Items.Add(j);
                }
            }
            foreach(PluginInformation information2 in PluginManager.PluginInformations) {
                if((information2.Enabled && (information2.PluginType == PluginType.Interactive)) && !list.Contains(information2)) {
                    this.listBoxPool.Items.Add(information2);
                }
            }
            if(this.pluginManager != null) {
                foreach(Plugin plugin in this.pluginManager.Plugins) {
                    if(plugin.PluginInformation.PluginType == PluginType.BackgroundMultiple) {
                        IBarMultipleCustomItems instance = plugin.Instance as IBarMultipleCustomItems;
                        if(instance != null) {
                            try {
                                int num5 = instance.Count;
                                if(num5 > 0) {
                                    int num6;
                                    this.dicActivePluginMulti.TryGetValue(plugin.PluginInformation.PluginID, out num6);
                                    for(int k = 0; k < (num5 - num6); k++) {
                                        this.listBoxPool.Items.Add(plugin.PluginInformation.Clone(num6 + k));
                                    }
                                }
                                else if(num5 == -1) {
                                    this.listBoxPool.Items.Add(plugin.PluginInformation.Clone(-1));
                                }
                            }
                            catch(Exception exception) {
                                PluginManager.HandlePluginException(exception, IntPtr.Zero, plugin.PluginInformation.Name, "Adding multi items to pool.");
                            }
                        }
                        continue;
                    }
                    if(plugin.BackgroundButtonSupported && !plugin.BackgroundButtonEnabled) {
                        this.listBoxPool.Items.Add(plugin.PluginInformation);
                    }
                }
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e) {
            int selectedIndex = this.listBoxPool.SelectedIndex;
            if(selectedIndex != -1) {
                this.fChangedExists = true;
                object selectedItem = this.listBoxPool.SelectedItem;
                bool flag;
                PluginInformation information = selectedItem as PluginInformation;
                if(information != null) {
                    flag = information.Index != -1;
                }
                else {
                    flag = 0 != ((int)selectedItem);
                }
                if(flag) {
                    this.listBoxPool.Items.RemoveAt(selectedIndex);
                    if(this.listBoxPool.Items.Count > selectedIndex) {
                        this.listBoxPool.SelectedIndex = selectedIndex;
                    }
                    else {
                        this.listBoxPool.SelectedIndex = selectedIndex - 1;
                    }
                }
                if(this.listBoxCurrent.SelectedIndex != -1) {
                    this.listBoxCurrent.Items.Insert(this.listBoxCurrent.SelectedIndex, selectedItem);
                }
                else {
                    this.listBoxCurrent.Items.Add(selectedItem);
                }
            }
        }

        private void ButtonBarOptionForm_FormClosing(object sender, FormClosingEventArgs e) {
            FormHeight = base.ClientSize.Height;
        }

        private void ButtonBarOptionForm_Load(object sender, EventArgs e) {
            if((FormHeight > 0xe9) & (FormHeight < 0x401)) {
                base.ClientSize = new Size(base.ClientSize.Width, FormHeight);
            }
        }

        private void buttonBrowseImage_Click(object sender, EventArgs e) {
            using(OpenFileDialog dialog = new OpenFileDialog()) {
                dialog.Filter = "Image Files(*.PNG;*.BMP;*.JPG;*.GIF)|*.PNG;*.BMP;*.JPG;*.GIF";
                dialog.FileName = this.strImageStripPath;
                dialog.RestoreDirectory = true;
                dialog.Title = "432 x 40";
                if(DialogResult.OK == dialog.ShowDialog()) {
                    if(this.InitializeImages(dialog.FileName, false)) {
                        this.fChangedExists = true;
                        this.Refresh();
                    }
                    else {
                        MessageBox.Show(QTUtility.ResMisc[1]);
                    }
                }
            }
        }

        private void buttonDown_Click(object sender, EventArgs e) {
            this.fChangedExists = true;
            int selectedIndex = this.listBoxCurrent.SelectedIndex;
            if((selectedIndex != -1) && (selectedIndex < (this.listBoxCurrent.Items.Count - 1))) {
                object selectedItem = this.listBoxCurrent.SelectedItem;
                this.listBoxCurrent.Items.RemoveAt(selectedIndex);
                this.listBoxCurrent.Items.Insert(selectedIndex + 1, selectedItem);
                this.listBoxCurrent.SelectedIndex = selectedIndex + 1;
            }
        }

        private void buttonOKCancel_Click(object sender, EventArgs e) {
            if(sender == this.buttonOK) {
                base.DialogResult = DialogResult.OK;
            }
            else {
                base.DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonRemove_Click(object sender, EventArgs e) {
            int selectedIndex = this.listBoxCurrent.SelectedIndex;
            if(selectedIndex != -1) {
                this.fChangedExists = true;
                object selectedItem = this.listBoxCurrent.SelectedItem;
                this.listBoxCurrent.Items.RemoveAt(selectedIndex);
                if(this.listBoxCurrent.Items.Count > selectedIndex) {
                    this.listBoxCurrent.SelectedIndex = selectedIndex;
                }
                else {
                    this.listBoxCurrent.SelectedIndex = selectedIndex - 1;
                }
                if(selectedItem is int) {
                    int num2 = (int)selectedItem;
                    if(num2 != 0) {
                        for(int i = 0; i < this.listBoxPool.Items.Count; i++) {
                            object obj3 = this.listBoxPool.Items[i];
                            if((obj3 is PluginInformation) || ((obj3 is int) && (num2 < ((int)obj3)))) {
                                this.listBoxPool.Items.Insert(i, selectedItem);
                                return;
                            }
                        }
                        this.listBoxPool.Items.Add(selectedItem);
                    }
                }
                else {
                    PluginInformation information = selectedItem as PluginInformation;
                    if((information == null) || (information.Index != -1)) {
                        this.listBoxPool.Items.Add(selectedItem);
                    }
                }
            }
        }

        private void buttonResetImage_Click(object sender, EventArgs e) {
            if(this.strImageStripPath.Length > 0) {
                this.fChangedExists = true;
            }
            this.InitializeImages(string.Empty, true);
            this.Refresh();
        }

        private void buttonRestoreDefault_Click(object sender, EventArgs e) {
            this.fChangedExists = true;
            this.listBoxCurrent.SuspendLayout();
            this.listBoxPool.SuspendLayout();
            this.listBoxCurrent.Items.Clear();
            this.listBoxPool.Items.Clear();
            this.dicActivePluginMulti.Clear();
            foreach(int index in QTButtonBar.DefaultButtonIndices) {
                this.listBoxCurrent.Items.Add(index);
            }
            this.listBoxPool.Items.Add(0);
            for(int j = 1; j < this.ButtonItemsDisplayName.Length; j++) {
                if(Array.IndexOf(QTButtonBar.DefaultButtonIndices, j) == -1) {
                    this.listBoxPool.Items.Add(j);
                }
            }
            foreach(PluginInformation information in PluginManager.PluginInformations) {
                if(information.Enabled && (information.PluginType == PluginType.Interactive)) {
                    this.listBoxPool.Items.Add(information);
                }
            }
            if(this.pluginManager != null) {
                foreach(Plugin plugin in this.pluginManager.Plugins) {
                    if(plugin.PluginInformation.PluginType == PluginType.BackgroundMultiple) {
                        IBarMultipleCustomItems instance = plugin.Instance as IBarMultipleCustomItems;
                        if(instance != null) {
                            try {
                                int count = instance.Count;
                                if(count > 0) {
                                    for(int k = 0; k < count; k++) {
                                        this.listBoxPool.Items.Add(plugin.PluginInformation.Clone(k));
                                    }
                                }
                                else if(count == -1) {
                                    this.listBoxPool.Items.Add(plugin.PluginInformation.Clone(-1));
                                }
                            }
                            catch(Exception exception) {
                                PluginManager.HandlePluginException(exception, base.Handle, plugin.PluginInformation.Name, "Adding multi items to pool.");
                            }
                        }
                        continue;
                    }
                    if(plugin.BackgroundButtonSupported) {
                        this.listBoxPool.Items.Add(plugin.PluginInformation);
                    }
                }
            }
            this.InitializeImages(string.Empty, true);
            this.listBoxPool.ResumeLayout();
            this.listBoxCurrent.ResumeLayout();
        }

        private void buttonUp_Click(object sender, EventArgs e) {
            this.fChangedExists = true;
            int selectedIndex = this.listBoxCurrent.SelectedIndex;
            if(selectedIndex > 0) {
                object selectedItem = this.listBoxCurrent.SelectedItem;
                this.listBoxCurrent.Items.RemoveAt(selectedIndex);
                this.listBoxCurrent.Items.Insert(selectedIndex - 1, selectedItem);
                this.listBoxCurrent.SelectedIndex = selectedIndex - 1;
            }
        }

        private void chbLockSearchBox_CheckedChanged(object sender, EventArgs e) {
            this.fChangedExists = true;
        }

        private void comboBoxes_ImageSizeAndText_SelectedIndexChanged(object sender, EventArgs e) {
            this.fChangedExists = true;
            if(sender == this.comboBoxImageSize) {
                this.fLargeIcon = this.comboBoxImageSize.SelectedIndex == 1;
                this.listBoxPool.ItemHeight = this.listBoxCurrent.ItemHeight = this.fLargeIcon ? 0x18 : 0x12;
            }
        }

        protected override void Dispose(bool disposing) {
            if(this.imageStripLarge != null) {
                this.imageStripLarge.Dispose();
                this.imageStripLarge = null;
            }
            if(this.imageStripSmall != null) {
                this.imageStripSmall.Dispose();
                this.imageStripSmall = null;
            }
            this.pluginManager = null;
            base.Dispose(disposing);
        }

        public int[] GetButtonIndices() {
            List<int> list = new List<int>();
            List<string> list2 = new List<string>();
            foreach(object obj2 in this.listBoxCurrent.Items) {
                PluginInformation information = obj2 as PluginInformation;
                if(information != null) {
                    list2.Add(information.PluginID);
                    list.Add(0x10000);
                    if((information.PluginType == PluginType.BackgroundMultiple) && (information.Index > -1)) {
                        this.pluginManager.PushBackgroundMultiple(information.PluginID, information.Index);
                    }
                }
                else {
                    list.Add((int)obj2);
                }
            }
            PluginManager.ActivatedButtonsOrder = list2;
            return list.ToArray();
        }

        private void InitializeComponent() {
            this.buttonAdd = new Button();
            this.buttonRemove = new Button();
            this.buttonOK = new Button();
            this.buttonRestoreDefault = new Button();
            this.buttonCancel = new Button();
            this.buttonUp = new Button();
            this.buttonDown = new Button();
            this.listBoxPool = new ListBox();
            this.listBoxCurrent = new ListBox();
            this.comboBoxImageSize = new ComboBox();
            this.comboBoxImageText = new ComboBox();
            this.buttonBrowseImage = new Button();
            this.buttonResetImage = new Button();
            this.textBoxImgPath = new TextBox();
            this.chbLockSearchBox = new CheckBox();
            base.SuspendLayout();
            this.chbLockSearchBox.AutoSize = true;
            this.chbLockSearchBox.Anchor = AnchorStyles.Bottom;
            this.chbLockSearchBox.Location = new Point(0xb8, 0xcb);
            this.chbLockSearchBox.UseVisualStyleBackColor = true;
            this.chbLockSearchBox.TabIndex = 14;
            this.chbLockSearchBox.CheckedChanged += this.chbLockSearchBox_CheckedChanged;
            this.buttonResetImage.Anchor = AnchorStyles.Bottom;
            this.buttonResetImage.Location = new Point(0x20d, 0xca);
            this.buttonResetImage.Size = new Size(0x7a, 0x17);
            this.buttonResetImage.TabIndex = 13;
            this.buttonResetImage.AutoSize = true;
            this.buttonResetImage.Click += this.buttonResetImage_Click;
            this.buttonBrowseImage.Anchor = AnchorStyles.Bottom;
            this.buttonBrowseImage.Location = new Point(0x20d, 0xad);
            this.buttonBrowseImage.Size = new Size(0x7a, 0x17);
            this.buttonBrowseImage.TabIndex = 12;
            this.buttonBrowseImage.AutoSize = true;
            this.buttonBrowseImage.Click += this.buttonBrowseImage_Click;
            this.textBoxImgPath.Anchor = AnchorStyles.Bottom;
            this.textBoxImgPath.Location = new Point(0xb8, 0xaf);
            this.textBoxImgPath.ReadOnly = true;
            this.textBoxImgPath.Size = new Size(0x150, 20);
            this.textBoxImgPath.TabIndex = 11;
            this.comboBoxImageSize.Anchor = AnchorStyles.Bottom;
            this.comboBoxImageSize.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboBoxImageSize.FormattingEnabled = true;
            this.comboBoxImageSize.Location = new Point(12, 0xcd);
            this.comboBoxImageSize.Size = new Size(0xa6, 20);
            this.comboBoxImageSize.TabIndex = 10;
            this.comboBoxImageText.Anchor = AnchorStyles.Bottom;
            this.comboBoxImageText.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboBoxImageText.Location = new Point(12, 0xaf);
            this.comboBoxImageText.Size = new Size(0xa6, 20);
            this.comboBoxImageText.TabIndex = 9;
            this.buttonDown.Anchor = AnchorStyles.Bottom;
            this.buttonDown.Location = new Point(0x23c, 0x89);
            this.buttonDown.Size = new Size(0x4b, 0x17);
            this.buttonDown.TabIndex = 8;
            this.buttonDown.Click += this.buttonDown_Click;
            this.buttonUp.Anchor = AnchorStyles.Bottom;
            this.buttonUp.Location = new Point(0x23c, 0x6f);
            this.buttonUp.Size = new Size(0x4b, 0x17);
            this.buttonUp.TabIndex = 7;
            this.buttonUp.Click += this.buttonUp_Click;
            this.buttonRestoreDefault.Anchor = AnchorStyles.Top;
            this.buttonRestoreDefault.Location = new Point(0x23c, 0x4a);
            this.buttonRestoreDefault.Size = new Size(0x4b, 0x17);
            this.buttonRestoreDefault.TabIndex = 6;
            this.buttonRestoreDefault.Click += this.buttonRestoreDefault_Click;
            this.buttonCancel.Anchor = AnchorStyles.Top;
            this.buttonCancel.Location = new Point(0x23c, 0x26);
            this.buttonCancel.Size = new Size(0x4b, 0x17);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Click += this.buttonOKCancel_Click;
            this.buttonOK.Anchor = AnchorStyles.Top;
            this.buttonOK.Location = new Point(0x23c, 12);
            this.buttonOK.Size = new Size(0x4b, 0x17);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Click += this.buttonOKCancel_Click;
            this.listBoxCurrent.Anchor = AnchorStyles.Bottom | AnchorStyles.Top;
            this.listBoxCurrent.DrawMode = DrawMode.OwnerDrawFixed;
            this.listBoxCurrent.FormattingEnabled = true;
            this.listBoxCurrent.ItemHeight = 0x18;
            this.listBoxCurrent.Location = new Point(0x14e, 12);
            this.listBoxCurrent.Size = new Size(0xce, 0x94);
            this.listBoxCurrent.TabIndex = 3;
            this.listBoxCurrent.DrawItem += this.listBoxes_DrawItem;
            this.buttonRemove.Anchor = AnchorStyles.Top;
            this.buttonRemove.Location = new Point(240, 0x4f);
            this.buttonRemove.Size = new Size(0x4b, 0x17);
            this.buttonRemove.TabIndex = 2;
            this.buttonRemove.Text = "<<";
            this.buttonRemove.Click += this.buttonRemove_Click;
            this.buttonAdd.Anchor = AnchorStyles.Top;
            this.buttonAdd.Location = new Point(240, 0x29);
            this.buttonAdd.Size = new Size(0x4b, 0x17);
            this.buttonAdd.TabIndex = 1;
            this.buttonAdd.Text = ">>";
            this.buttonAdd.Click += this.buttonAdd_Click;
            this.listBoxPool.Anchor = AnchorStyles.Bottom | AnchorStyles.Top;
            this.listBoxPool.DrawMode = DrawMode.OwnerDrawFixed;
            this.listBoxPool.FormattingEnabled = true;
            this.listBoxPool.ItemHeight = 0x18;
            this.listBoxPool.Location = new Point(12, 12);
            this.listBoxPool.Size = new Size(0xce, 0x94);
            this.listBoxPool.TabIndex = 0;
            this.listBoxPool.DrawItem += this.listBoxes_DrawItem;
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.CancelButton = this.buttonOK;
            base.ClientSize = new Size(0x29a, 0xea);
            base.Controls.Add(this.textBoxImgPath);
            base.Controls.Add(this.buttonBrowseImage);
            base.Controls.Add(this.buttonResetImage);
            base.Controls.Add(this.comboBoxImageText);
            base.Controls.Add(this.comboBoxImageSize);
            base.Controls.Add(this.listBoxCurrent);
            base.Controls.Add(this.listBoxPool);
            base.Controls.Add(this.buttonDown);
            base.Controls.Add(this.buttonUp);
            base.Controls.Add(this.buttonRestoreDefault);
            base.Controls.Add(this.buttonCancel);
            base.Controls.Add(this.buttonOK);
            base.Controls.Add(this.buttonRemove);
            base.Controls.Add(this.buttonAdd);
            base.Controls.Add(this.chbLockSearchBox);
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            this.MaximumSize = new Size(0x2aa, 0x400);
            this.MinimumSize = new Size(0x2aa, 0xea);
            base.ShowIcon = false;
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Button options";
            base.Load += this.ButtonBarOptionForm_Load;
            base.FormClosing += this.ButtonBarOptionForm_FormClosing;
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private bool InitializeImages(string path, bool fLoadDefaultOnFail) {
            Bitmap bitmap;
            Bitmap bitmap2;
            if(this.imageStripLarge == null) {
                this.imageStripLarge = new ImageStrip(new Size(0x18, 0x18));
                this.imageStripSmall = new ImageStrip(new Size(0x10, 0x10));
            }
            if(!string.IsNullOrEmpty(path) && QTButtonBar.LoadExternalImage(path, out bitmap, out bitmap2)) {
                this.imageStripLarge.AddStrip(bitmap);
                this.imageStripSmall.AddStrip(bitmap2);
                bitmap.Dispose();
                bitmap2.Dispose();
                this.textBoxImgPath.Text = this.strImageStripPath = path;
                return true;
            }
            if(fLoadDefaultOnFail) {
                bitmap = Resources_Image.ButtonStrip24;
                bitmap2 = Resources_Image.ButtonStrip16;
                this.imageStripLarge.AddStrip(bitmap);
                this.imageStripSmall.AddStrip(bitmap2);
                bitmap.Dispose();
                bitmap2.Dispose();
                this.textBoxImgPath.Text = this.strImageStripPath = string.Empty;
            }
            return false;
        }

        private void listBoxes_DrawItem(object sender, DrawItemEventArgs e) {
            ListBox box = (ListBox)sender;
            if(box.Items.Count >= 1) {
                e.DrawBackground();
                object obj2 = box.Items[e.Index];
                string s;
                PluginInformation information = obj2 as PluginInformation;
                if(information == null) {
                    int index = (int)obj2;
                    s = this.ButtonItemsDisplayName[index];
                    if((index != 0) && (index < 0x13)) {
                        Rectangle rect = new Rectangle(e.Bounds.Location, this.fLargeIcon ? new Size(0x18, 0x18) : new Size(0x10, 0x10));
                        try {
                            Image image2 = this.fLargeIcon ? this.imageStripLarge[index - 1] : this.imageStripSmall[index - 1];
                            if(image2 != null) {
                                e.Graphics.DrawImage(image2, rect);
                            }
                        }
                        catch {
                        }
                    }
                }
                else {
                    Image image = this.fLargeIcon ? information.ImageLarge : information.ImageSmall;
                    s = information.Name;
                    if((information.PluginType == PluginType.BackgroundMultiple) && (information.Index != -1)) {
                        foreach(Plugin plugin in this.pluginManager.Plugins) {
                            if(plugin.PluginInformation.PluginID == information.PluginID) {
                                IBarMultipleCustomItems instance = plugin.Instance as IBarMultipleCustomItems;
                                if(instance == null) {
                                    break;
                                }
                                try {
                                    image = instance.GetImage(this.fLargeIcon, information.Index);
                                    s = instance.GetName(information.Index);
                                    break;
                                }
                                catch {
                                    s = "Failed to get name.";
                                    break;
                                }
                            }
                        }
                    }
                    if(image != null) {
                        e.Graphics.DrawImage(image, new Rectangle(e.Bounds.Location, this.fLargeIcon ? new Size(0x18, 0x18) : new Size(0x10, 0x10)));
                    }
                }
                bool flag = (e.State & DrawItemState.Selected) != DrawItemState.None;
                e.Graphics.DrawString(s, this.Font, flag ? SystemBrushes.HighlightText : SystemBrushes.ControlText, new PointF((e.Bounds.X + 0x1c), (e.Bounds.Y + (this.fLargeIcon ? 5 : 2))));
            }
        }

        public string ImageStripPath {
            get {
                return this.strImageStripPath;
            }
        }

        public int ItemTextMode {
            get {
                return this.comboBoxImageText.SelectedIndex;
            }
        }

        public bool LockSearchBox {
            get {
                return this.chbLockSearchBox.Checked;
            }
        }
    }
}
