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
using System.Linq;
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
            InitializeComponent();
            this.fLargeIcon = fLargeIcon;
            this.pluginManager = pluginManager;
            ButtonItemsDisplayName = QTUtility.TextResourcesDic["ButtonBar_BtnName"];
            string[] strArray = QTUtility.TextResourcesDic["ButtonBar_Option"];
            string[] strArray2 = QTUtility.TextResourcesDic["DialogButtons"];
            buttonOK.Text = strArray2[0];
            buttonCancel.Text = strArray2[1];
            buttonRestoreDefault.Text = strArray[3];
            buttonUp.Text = strArray[4];
            buttonDown.Text = strArray[5];
            buttonBrowseImage.Text = strArray[6];
            buttonResetImage.Text = strArray[11];
            comboBoxImageSize.Items.AddRange(new string[] { strArray[7], strArray[8] });
            comboBoxImageText.Items.AddRange(new string[] { strArray[12], strArray[13], strArray[14] });
            chbLockSearchBox.Text = strArray[15];
            comboBoxImageSize.SelectedIndex = this.fLargeIcon ? 1 : 0;
            /*
            if((QTButtonBar.ConfigValues[0] & 0x20) == 0x20) {
                comboBoxImageText.SelectedIndex = ((QTButtonBar.ConfigValues[0] & 0x10) == 0x10) ? 1 : 0;
            }
            else {
                comboBoxImageText.SelectedIndex = 2;
            }*/
            comboBoxImageSize.SelectedIndexChanged += comboBoxes_ImageSizeAndText_SelectedIndexChanged;
            comboBoxImageText.SelectedIndexChanged += comboBoxes_ImageSizeAndText_SelectedIndexChanged;
            chbLockSearchBox.Checked = Config.BBar.LockSearchBarWidth;
            InitializeImages(currentImagePath, true);
            List<PluginInformation> list = new List<PluginInformation>();
            foreach(int item in currentItemIndexes) {
                if(item >= 0x10000) {
                    int count = list.Count;
                    if(PluginManager.ActivatedButtonsOrder.Count > count) {
                        string key = ""; //PluginManager.ActivatedButtonsOrder[count];
                        foreach(PluginInformation information in PluginManager.PluginInformations
                                .Where(information => information.PluginID == key)) {
                            list.Add(information);
                            if(information.PluginType == PluginType.BackgroundMultiple) {
                                int num3;
                                if(dicActivePluginMulti.TryGetValue(key, out num3)) {
                                    dicActivePluginMulti[key] = num3 + 1;
                                }
                                else {
                                    dicActivePluginMulti[key] = 1;
                                }
                                listBoxCurrent.Items.Add(information.Clone(num3));
                            }
                            else {
                                listBoxCurrent.Items.Add(information);
                            }
                            break;
                        }
                    }
                }
                else if(item != 0x15) {
                    listBoxCurrent.Items.Add(item);
                }
            }
            listBoxPool.Items.Add(0);
            for(int j = 1; j < ButtonItemsDisplayName.Length; j++) {
                if(Array.IndexOf(currentItemIndexes, j) == -1) {
                    listBoxPool.Items.Add(j);
                }
            }
            listBoxPool.Items.AddRange(PluginManager.PluginInformations
                    .Where(info => info.Enabled && info.PluginType == PluginType.Interactive)
                    .Except(list).ToArray());
            if(this.pluginManager == null) return;
            foreach(Plugin plugin in this.pluginManager.Plugins) {
                if(plugin.PluginInformation.PluginType == PluginType.BackgroundMultiple) {
                    IBarMultipleCustomItems instance = plugin.Instance as IBarMultipleCustomItems;
                    if(instance != null) {
                        try {
                            int num5 = instance.Count;
                            if(num5 > 0) {
                                int num6;
                                dicActivePluginMulti.TryGetValue(plugin.PluginInformation.PluginID, out num6);
                                for(int k = 0; k < (num5 - num6); k++) {
                                    listBoxPool.Items.Add(plugin.PluginInformation.Clone(num6 + k));
                                }
                            }
                            else if(num5 == -1) {
                                listBoxPool.Items.Add(plugin.PluginInformation.Clone(-1));
                            }
                        }
                        catch(Exception exception) {
                            PluginManager.HandlePluginException(exception, IntPtr.Zero, plugin.PluginInformation.Name, "Adding multi items to pool.");
                        }
                    }
                }
                else if(plugin.BackgroundButtonSupported && !plugin.BackgroundButtonEnabled) {
                    listBoxPool.Items.Add(plugin.PluginInformation);
                }
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e) {
            int selectedIndex = listBoxPool.SelectedIndex;
            if(selectedIndex != -1) {
                fChangedExists = true;
                object selectedItem = listBoxPool.SelectedItem;
                bool flag;
                PluginInformation information = selectedItem as PluginInformation;
                if(information != null) {
                    flag = information.Index != -1;
                }
                else {
                    flag = 0 != ((int)selectedItem);
                }
                if(flag) {
                    listBoxPool.Items.RemoveAt(selectedIndex);
                    if(listBoxPool.Items.Count > selectedIndex) {
                        listBoxPool.SelectedIndex = selectedIndex;
                    }
                    else {
                        listBoxPool.SelectedIndex = selectedIndex - 1;
                    }
                }
                if(listBoxCurrent.SelectedIndex != -1) {
                    listBoxCurrent.Items.Insert(listBoxCurrent.SelectedIndex, selectedItem);
                }
                else {
                    listBoxCurrent.Items.Add(selectedItem);
                }
            }
        }

        private void ButtonBarOptionForm_FormClosing(object sender, FormClosingEventArgs e) {
            FormHeight = ClientSize.Height;
        }

        private void ButtonBarOptionForm_Load(object sender, EventArgs e) {
            if((FormHeight > 0xe9) & (FormHeight < 0x401)) {
                ClientSize = new Size(ClientSize.Width, FormHeight);
            }
        }

        private void buttonBrowseImage_Click(object sender, EventArgs e) {
            using(OpenFileDialog dialog = new OpenFileDialog()) {
                dialog.Filter = "Image Files(*.PNG;*.BMP;*.JPG;*.GIF)|*.PNG;*.BMP;*.JPG;*.GIF";
                dialog.FileName = strImageStripPath;
                dialog.RestoreDirectory = true;
                dialog.Title = "432 x 40";
                if(DialogResult.OK == dialog.ShowDialog()) {
                    if(InitializeImages(dialog.FileName, false)) {
                        fChangedExists = true;
                        Refresh();
                    }
                    else {
                        MessageBox.Show(QTUtility.ResMisc[1]);
                    }
                }
            }
        }

        private void buttonDown_Click(object sender, EventArgs e) {
            fChangedExists = true;
            int selectedIndex = listBoxCurrent.SelectedIndex;
            if((selectedIndex != -1) && (selectedIndex < (listBoxCurrent.Items.Count - 1))) {
                object selectedItem = listBoxCurrent.SelectedItem;
                listBoxCurrent.Items.RemoveAt(selectedIndex);
                listBoxCurrent.Items.Insert(selectedIndex + 1, selectedItem);
                listBoxCurrent.SelectedIndex = selectedIndex + 1;
            }
        }

        private void buttonOKCancel_Click(object sender, EventArgs e) {
            if(sender == buttonOK) {
                DialogResult = DialogResult.OK;
            }
            else {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonRemove_Click(object sender, EventArgs e) {
            int selectedIndex = listBoxCurrent.SelectedIndex;
            if(selectedIndex != -1) {
                fChangedExists = true;
                object selectedItem = listBoxCurrent.SelectedItem;
                listBoxCurrent.Items.RemoveAt(selectedIndex);
                if(listBoxCurrent.Items.Count > selectedIndex) {
                    listBoxCurrent.SelectedIndex = selectedIndex;
                }
                else {
                    listBoxCurrent.SelectedIndex = selectedIndex - 1;
                }
                if(selectedItem is int) {
                    int num2 = (int)selectedItem;
                    if(num2 != 0) {
                        for(int i = 0; i < listBoxPool.Items.Count; i++) {
                            object obj3 = listBoxPool.Items[i];
                            if((obj3 is PluginInformation) || ((obj3 is int) && (num2 < ((int)obj3)))) {
                                listBoxPool.Items.Insert(i, selectedItem);
                                return;
                            }
                        }
                        listBoxPool.Items.Add(selectedItem);
                    }
                }
                else {
                    PluginInformation information = selectedItem as PluginInformation;
                    if((information == null) || (information.Index != -1)) {
                        listBoxPool.Items.Add(selectedItem);
                    }
                }
            }
        }

        private void buttonResetImage_Click(object sender, EventArgs e) {
            if(strImageStripPath.Length > 0) {
                fChangedExists = true;
            }
            InitializeImages(string.Empty, true);
            Refresh();
        }

        private void buttonRestoreDefault_Click(object sender, EventArgs e) {
            fChangedExists = true;
            listBoxCurrent.SuspendLayout();
            listBoxPool.SuspendLayout();
            listBoxCurrent.Items.Clear();
            listBoxPool.Items.Clear();
            dicActivePluginMulti.Clear();
            /*
            foreach(int index in QTButtonBar.DefaultButtonIndices) {
                listBoxCurrent.Items.Add(index);
            }
            listBoxPool.Items.Add(0);
            for(int j = 1; j < ButtonItemsDisplayName.Length; j++) {
                if(Array.IndexOf(QTButtonBar.DefaultButtonIndices, j) == -1) {
                    listBoxPool.Items.Add(j);
                }
            }*/
            listBoxPool.Items.AddRange(PluginManager.PluginInformations.Where(info => 
                    info.Enabled && info.PluginType == PluginType.Interactive).ToArray());
            if(pluginManager != null) {
                foreach(Plugin plugin in pluginManager.Plugins) {
                    if(plugin.PluginInformation.PluginType == PluginType.BackgroundMultiple) {
                        IBarMultipleCustomItems instance = plugin.Instance as IBarMultipleCustomItems;
                        if(instance != null) {
                            try {
                                int count = instance.Count;
                                if(count > 0) {
                                    for(int k = 0; k < count; k++) {
                                        listBoxPool.Items.Add(plugin.PluginInformation.Clone(k));
                                    }
                                }
                                else if(count == -1) {
                                    listBoxPool.Items.Add(plugin.PluginInformation.Clone(-1));
                                }
                            }
                            catch(Exception exception) {
                                PluginManager.HandlePluginException(exception, Handle, plugin.PluginInformation.Name, "Adding multi items to pool.");
                            }
                        }
                        continue;
                    }
                    if(plugin.BackgroundButtonSupported) {
                        listBoxPool.Items.Add(plugin.PluginInformation);
                    }
                }
            }
            InitializeImages(string.Empty, true);
            listBoxPool.ResumeLayout();
            listBoxCurrent.ResumeLayout();
        }

        private void buttonUp_Click(object sender, EventArgs e) {
            fChangedExists = true;
            int selectedIndex = listBoxCurrent.SelectedIndex;
            if(selectedIndex > 0) {
                object selectedItem = listBoxCurrent.SelectedItem;
                listBoxCurrent.Items.RemoveAt(selectedIndex);
                listBoxCurrent.Items.Insert(selectedIndex - 1, selectedItem);
                listBoxCurrent.SelectedIndex = selectedIndex - 1;
            }
        }

        private void chbLockSearchBox_CheckedChanged(object sender, EventArgs e) {
            fChangedExists = true;
        }

        private void comboBoxes_ImageSizeAndText_SelectedIndexChanged(object sender, EventArgs e) {
            fChangedExists = true;
            if(sender == comboBoxImageSize) {
                fLargeIcon = comboBoxImageSize.SelectedIndex == 1;
                listBoxPool.ItemHeight = listBoxCurrent.ItemHeight = fLargeIcon ? 0x18 : 0x12;
            }
        }

        protected override void Dispose(bool disposing) {
            if(imageStripLarge != null) {
                imageStripLarge.Dispose();
                imageStripLarge = null;
            }
            if(imageStripSmall != null) {
                imageStripSmall.Dispose();
                imageStripSmall = null;
            }
            pluginManager = null;
            base.Dispose(disposing);
        }
        /*
        public int[] GetButtonIndices() {
            List<int> list = new List<int>();
            List<string> list2 = new List<string>();
            foreach(object obj2 in listBoxCurrent.Items) {
                PluginInformation information = obj2 as PluginInformation;
                if(information != null) {
                    list2.Add(information.PluginID);
                    list.Add(0x10000);
                    if((information.PluginType == PluginType.BackgroundMultiple) && (information.Index > -1)) {
                        pluginManager.PushBackgroundMultiple(information.PluginID, information.Index);
                    }
                }
                else {
                    list.Add((int)obj2);
                }
            }
            PluginManager.ActivatedButtonsOrder = list2;
            return list.ToArray();
        }*/

        private void InitializeComponent() {
            buttonAdd = new Button();
            buttonRemove = new Button();
            buttonOK = new Button();
            buttonRestoreDefault = new Button();
            buttonCancel = new Button();
            buttonUp = new Button();
            buttonDown = new Button();
            listBoxPool = new ListBox();
            listBoxCurrent = new ListBox();
            comboBoxImageSize = new ComboBox();
            comboBoxImageText = new ComboBox();
            buttonBrowseImage = new Button();
            buttonResetImage = new Button();
            textBoxImgPath = new TextBox();
            chbLockSearchBox = new CheckBox();
            SuspendLayout();
            chbLockSearchBox.AutoSize = true;
            chbLockSearchBox.Anchor = AnchorStyles.Bottom;
            chbLockSearchBox.Location = new Point(0xb8, 0xcb);
            chbLockSearchBox.UseVisualStyleBackColor = true;
            chbLockSearchBox.TabIndex = 14;
            chbLockSearchBox.CheckedChanged += chbLockSearchBox_CheckedChanged;
            buttonResetImage.Anchor = AnchorStyles.Bottom;
            buttonResetImage.Location = new Point(0x20d, 0xca);
            buttonResetImage.Size = new Size(0x7a, 0x17);
            buttonResetImage.TabIndex = 13;
            buttonResetImage.AutoSize = true;
            buttonResetImage.Click += buttonResetImage_Click;
            buttonBrowseImage.Anchor = AnchorStyles.Bottom;
            buttonBrowseImage.Location = new Point(0x20d, 0xad);
            buttonBrowseImage.Size = new Size(0x7a, 0x17);
            buttonBrowseImage.TabIndex = 12;
            buttonBrowseImage.AutoSize = true;
            buttonBrowseImage.Click += buttonBrowseImage_Click;
            textBoxImgPath.Anchor = AnchorStyles.Bottom;
            textBoxImgPath.Location = new Point(0xb8, 0xaf);
            textBoxImgPath.ReadOnly = true;
            textBoxImgPath.Size = new Size(0x150, 20);
            textBoxImgPath.TabIndex = 11;
            comboBoxImageSize.Anchor = AnchorStyles.Bottom;
            comboBoxImageSize.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxImageSize.FormattingEnabled = true;
            comboBoxImageSize.Location = new Point(12, 0xcd);
            comboBoxImageSize.Size = new Size(0xa6, 20);
            comboBoxImageSize.TabIndex = 10;
            comboBoxImageText.Anchor = AnchorStyles.Bottom;
            comboBoxImageText.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxImageText.Location = new Point(12, 0xaf);
            comboBoxImageText.Size = new Size(0xa6, 20);
            comboBoxImageText.TabIndex = 9;
            buttonDown.Anchor = AnchorStyles.Bottom;
            buttonDown.Location = new Point(0x23c, 0x89);
            buttonDown.Size = new Size(0x4b, 0x17);
            buttonDown.TabIndex = 8;
            buttonDown.Click += buttonDown_Click;
            buttonUp.Anchor = AnchorStyles.Bottom;
            buttonUp.Location = new Point(0x23c, 0x6f);
            buttonUp.Size = new Size(0x4b, 0x17);
            buttonUp.TabIndex = 7;
            buttonUp.Click += buttonUp_Click;
            buttonRestoreDefault.Anchor = AnchorStyles.Top;
            buttonRestoreDefault.Location = new Point(0x23c, 0x4a);
            buttonRestoreDefault.Size = new Size(0x4b, 0x17);
            buttonRestoreDefault.TabIndex = 6;
            buttonRestoreDefault.Click += buttonRestoreDefault_Click;
            buttonCancel.Anchor = AnchorStyles.Top;
            buttonCancel.Location = new Point(0x23c, 0x26);
            buttonCancel.Size = new Size(0x4b, 0x17);
            buttonCancel.TabIndex = 5;
            buttonCancel.Click += buttonOKCancel_Click;
            buttonOK.Anchor = AnchorStyles.Top;
            buttonOK.Location = new Point(0x23c, 12);
            buttonOK.Size = new Size(0x4b, 0x17);
            buttonOK.TabIndex = 4;
            buttonOK.Click += buttonOKCancel_Click;
            listBoxCurrent.Anchor = AnchorStyles.Bottom | AnchorStyles.Top;
            listBoxCurrent.DrawMode = DrawMode.OwnerDrawFixed;
            listBoxCurrent.FormattingEnabled = true;
            listBoxCurrent.ItemHeight = 0x18;
            listBoxCurrent.Location = new Point(0x14e, 12);
            listBoxCurrent.Size = new Size(0xce, 0x94);
            listBoxCurrent.TabIndex = 3;
            listBoxCurrent.DrawItem += listBoxes_DrawItem;
            buttonRemove.Anchor = AnchorStyles.Top;
            buttonRemove.Location = new Point(240, 0x4f);
            buttonRemove.Size = new Size(0x4b, 0x17);
            buttonRemove.TabIndex = 2;
            buttonRemove.Text = "<<";
            buttonRemove.Click += buttonRemove_Click;
            buttonAdd.Anchor = AnchorStyles.Top;
            buttonAdd.Location = new Point(240, 0x29);
            buttonAdd.Size = new Size(0x4b, 0x17);
            buttonAdd.TabIndex = 1;
            buttonAdd.Text = ">>";
            buttonAdd.Click += buttonAdd_Click;
            listBoxPool.Anchor = AnchorStyles.Bottom | AnchorStyles.Top;
            listBoxPool.DrawMode = DrawMode.OwnerDrawFixed;
            listBoxPool.FormattingEnabled = true;
            listBoxPool.ItemHeight = 0x18;
            listBoxPool.Location = new Point(12, 12);
            listBoxPool.Size = new Size(0xce, 0x94);
            listBoxPool.TabIndex = 0;
            listBoxPool.DrawItem += listBoxes_DrawItem;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = buttonOK;
            ClientSize = new Size(0x29a, 0xea);
            Controls.Add(textBoxImgPath);
            Controls.Add(buttonBrowseImage);
            Controls.Add(buttonResetImage);
            Controls.Add(comboBoxImageText);
            Controls.Add(comboBoxImageSize);
            Controls.Add(listBoxCurrent);
            Controls.Add(listBoxPool);
            Controls.Add(buttonDown);
            Controls.Add(buttonUp);
            Controls.Add(buttonRestoreDefault);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            Controls.Add(buttonRemove);
            Controls.Add(buttonAdd);
            Controls.Add(chbLockSearchBox);
            MaximizeBox = false;
            MinimizeBox = false;
            MaximumSize = new Size(0x2aa, 0x400);
            MinimumSize = new Size(0x2aa, 0xea);
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Button options";
            Load += ButtonBarOptionForm_Load;
            FormClosing += ButtonBarOptionForm_FormClosing;
            ResumeLayout(false);
            PerformLayout();
        }

        private bool InitializeImages(string path, bool fLoadDefaultOnFail) {
            Bitmap bitmap;
            Bitmap bitmap2;
            if(imageStripLarge == null) {
                imageStripLarge = new ImageStrip(new Size(0x18, 0x18));
                imageStripSmall = new ImageStrip(new Size(0x10, 0x10));
            }
            if(!string.IsNullOrEmpty(path) && QTButtonBar.LoadExternalImage(path, out bitmap, out bitmap2)) {
                imageStripLarge.AddStrip(bitmap);
                imageStripSmall.AddStrip(bitmap2);
                bitmap.Dispose();
                bitmap2.Dispose();
                textBoxImgPath.Text = strImageStripPath = path;
                return true;
            }
            if(fLoadDefaultOnFail) {
                bitmap = Resources_Image.ButtonStrip24;
                bitmap2 = Resources_Image.ButtonStrip16;
                imageStripLarge.AddStrip(bitmap);
                imageStripSmall.AddStrip(bitmap2);
                bitmap.Dispose();
                bitmap2.Dispose();
                textBoxImgPath.Text = strImageStripPath = string.Empty;
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
                    s = ButtonItemsDisplayName[index];
                    if((index != 0) && (index < 0x13)) {
                        Rectangle rect = new Rectangle(e.Bounds.Location, fLargeIcon ? new Size(0x18, 0x18) : new Size(0x10, 0x10));
                        try {
                            Image image2 = fLargeIcon ? imageStripLarge[index - 1] : imageStripSmall[index - 1];
                            if(image2 != null) {
                                e.Graphics.DrawImage(image2, rect);
                            }
                        }
                        catch {
                        }
                    }
                }
                else {
                    Image image = fLargeIcon ? information.ImageLarge : information.ImageSmall;
                    s = information.Name;
                    if((information.PluginType == PluginType.BackgroundMultiple) && (information.Index != -1)) {
                        foreach(Plugin plugin in pluginManager.Plugins) {
                            if(plugin.PluginInformation.PluginID == information.PluginID) {
                                IBarMultipleCustomItems instance = plugin.Instance as IBarMultipleCustomItems;
                                if(instance == null) {
                                    break;
                                }
                                try {
                                    image = instance.GetImage(fLargeIcon, information.Index);
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
                        e.Graphics.DrawImage(image, new Rectangle(e.Bounds.Location, fLargeIcon ? new Size(0x18, 0x18) : new Size(0x10, 0x10)));
                    }
                }
                bool flag = (e.State & DrawItemState.Selected) != DrawItemState.None;
                e.Graphics.DrawString(s, Font, flag ? SystemBrushes.HighlightText : SystemBrushes.ControlText, new PointF((e.Bounds.X + 0x1c), (e.Bounds.Y + (fLargeIcon ? 5 : 2))));
            }
        }

        public string ImageStripPath {
            get {
                return strImageStripPath;
            }
        }

        public int ItemTextMode {
            get {
                return comboBoxImageText.SelectedIndex;
            }
        }

        public bool LockSearchBox {
            get {
                return chbLockSearchBox.Checked;
            }
        }
    }
}
