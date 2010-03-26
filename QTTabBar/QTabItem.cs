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
    using QTPlugin;
    using QTTabBarLib.Interop;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    internal sealed class QTabItem : QTabItemBase {
        private byte[] currentIDL;
        private string currentPath;
        private Dictionary<string, string> dicFocusedItemName;
        private Dictionary<string, Address[]> dicSelectdItems;
        private bool fNowSlowTip;
        private List<LogData> lstHistoryBranches;
        private Stack<LogData> stckHistoryBackward;
        private Stack<LogData> stckHistoryForward;
        private string tooltipText2;

        public event EventHandler Closed;

        public QTabItem(string title, string path, QTabControl parent)
            : base(title, parent) {
            this.stckHistoryForward = new Stack<LogData>();
            this.stckHistoryBackward = new Stack<LogData>();
            this.dicSelectdItems = new Dictionary<string, Address[]>();
            this.dicFocusedItemName = new Dictionary<string, string>();
            this.lstHistoryBranches = new List<LogData>();
            this.CurrentPath = path;
        }

        public static void CheckSubTexts(QTabControl tabControl) {
            bool flag = false;
            char[] separator = new char[] { '\\' };
            Dictionary<string, List<QTabItem>> dictionary = new Dictionary<string, List<QTabItem>>();
            foreach(QTabItem item in tabControl.TabPages) {
                if(!item.CurrentPath.StartsWith("::")) {
                    if(dictionary.ContainsKey(item.Text)) {
                        dictionary[item.Text].Add(item);
                    }
                    else {
                        List<QTabItem> list = new List<QTabItem>();
                        list.Add(item);
                        dictionary[item.Text] = list;
                    }
                }
            }
            foreach(string str in dictionary.Keys) {
                List<QTabItem> list2 = dictionary[str];
                if(list2.Count > 1) {
                    bool flag2 = true;
                    for(int i = 1; i < list2.Count; i++) {
                        if(list2[i].CurrentPath != list2[0].CurrentPath) {
                            flag2 = false;
                            break;
                        }
                    }
                    if(flag2) {
                        foreach(QTabItem item2 in list2) {
                            if(item2.Comment.Length > 0) {
                                item2.Comment = string.Empty;
                                item2.RefreshRectangle();
                                flag = true;
                            }
                        }
                    }
                    else {
                        foreach(QTabItem item3 in list2) {
                            string str2 = string.Empty;
                            string[] strArray = item3.CurrentPath.Split(separator);
                            if(strArray.Length > 1) {
                                for(int j = strArray.Length - 2; j > -1; j--) {
                                    str2 = strArray[j];
                                    bool flag3 = false;
                                    foreach(QTabItem item4 in list2) {
                                        if((item4 == item3) || !(item4.CurrentPath != item3.CurrentPath)) {
                                            continue;
                                        }
                                        string[] strArray2 = item4.CurrentPath.Split(separator);
                                        if(strArray2.Length > 1) {
                                            for(int k = strArray2.Length - 2; k > -1; k--) {
                                                if(str2 == strArray2[k]) {
                                                    flag3 = true;
                                                    str2 = string.Empty;
                                                    break;
                                                }
                                            }
                                        }
                                        if(flag3) {
                                            break;
                                        }
                                    }
                                    if(!flag3) {
                                        break;
                                    }
                                }
                            }
                            if(str2.Length > 0) {
                                if((str2.Length == 2) && (str2[1] == ':')) {
                                    str2 = str2 + @"\";
                                }
                                item3.Comment = str2;
                            }
                            else {
                                item3.Comment = item3.CurrentPath;
                            }
                            item3.RefreshRectangle();
                            flag = true;
                        }
                    }
                    continue;
                }
                if(list2[0].Comment.Length > 0) {
                    flag = true;
                    list2[0].Comment = string.Empty;
                    list2[0].RefreshRectangle();
                }
            }
            if(flag) {
                tabControl.Refresh();
            }
        }

        public QTabItem Clone() {
            return this.Clone(false);
        }

        public QTabItem Clone(bool fAll) {
            QTabItem item = new QTabItem(base.Text, this.currentPath, base.Owner);
            item.TabBounds = base.TabBounds;
            item.Comment = base.Comment;
            item.currentIDL = this.currentIDL;
            item.ToolTipText = base.ToolTipText;
            item.tooltipText2 = this.tooltipText2;
            if(fAll) {
                item.tabLocked = base.tabLocked;
            }
            LogData[] array = this.stckHistoryForward.ToArray();
            LogData[] dataArray2 = this.stckHistoryBackward.ToArray();
            Array.Reverse(array);
            Array.Reverse(dataArray2);
            item.stckHistoryForward = new Stack<LogData>(array);
            item.stckHistoryBackward = new Stack<LogData>(dataArray2);
            item.dicFocusedItemName = new Dictionary<string, string>(this.dicFocusedItemName);
            item.lstHistoryBranches = new List<LogData>(this.lstHistoryBranches.ToArray());
            Dictionary<string, Address[]> dictionary = new Dictionary<string, Address[]>();
            foreach(string str in this.dicSelectdItems.Keys) {
                dictionary.Add(str, this.dicSelectdItems[str]);
            }
            item.dicSelectdItems = dictionary;
            return item;
        }

        public string[] GetHistoryBack() {
            List<string> list = new List<string>();
            foreach(LogData data in this.stckHistoryBackward) {
                list.Add(data.Path);
            }
            return list.ToArray();
        }

        public string[] GetHistoryForward() {
            List<string> list = new List<string>();
            foreach(LogData data in this.stckHistoryForward) {
                list.Add(data.Path);
            }
            return list.ToArray();
        }

        public int GetLogHash(bool back, int index) {
            LogData[] dataArray;
            if(back) {
                dataArray = this.stckHistoryBackward.ToArray();
            }
            else {
                dataArray = this.stckHistoryForward.ToArray();
            }
            if((index > -1) && (index < dataArray.Length)) {
                return dataArray[index].Hash;
            }
            return -1;
        }

        public LogData[] GetLogs(bool fBack) {
            List<LogData> list = new List<LogData>(fBack ? this.stckHistoryBackward : this.stckHistoryForward);
            if(fBack && (list.Count > 0)) {
                list.RemoveAt(0);
            }
            return list.ToArray();
        }

        public Address[] GetSelectedItemsAt(string path, out string focused) {
            Address[] addressArray;
            this.dicSelectdItems.TryGetValue(path, out addressArray);
            this.dicFocusedItemName.TryGetValue(path, out focused);
            return addressArray;
        }

        public LogData GoBackward() {
            LogData data = new LogData();
            if(this.stckHistoryBackward.Count > 1) {
                this.stckHistoryForward.Push(this.stckHistoryBackward.Pop());
                data = this.stckHistoryBackward.Peek();
                this.CurrentPath = data.Path;
            }
            return data;
        }

        public LogData GoForward() {
            LogData data = new LogData();
            if(this.stckHistoryForward.Count != 0) {
                this.stckHistoryBackward.Push(this.stckHistoryForward.Pop());
                data = this.stckHistoryBackward.Peek();
                this.CurrentPath = data.Path;
            }
            return data;
        }

        public void NavigatedTo(string path, byte[] idl, int hash) {
            if((idl == null) || (idl.Length == 0)) {
                idl = ShellMethods.GetIDLData(path);
            }
            this.stckHistoryBackward.Push(new LogData(path, idl, hash));
            foreach(LogData data in this.stckHistoryForward) {
                if(!this.lstHistoryBranches.Contains(data)) {
                    this.lstHistoryBranches.Add(data);
                }
            }
            foreach(LogData data2 in this.stckHistoryBackward) {
                this.lstHistoryBranches.Remove(data2);
            }
            this.stckHistoryForward.Clear();
            this.CurrentPath = path;
            this.currentIDL = idl;
        }

        public override void OnClose() {
            if(this.Closed != null) {
                this.Closed(null, EventArgs.Empty);
                this.Closed = null;
            }
            base.OnClose();
        }

        public void SetSelectedItemsAt(string path, Address[] names, string focused) {
            this.dicSelectdItems[path] = names;
            this.dicFocusedItemName[path] = focused;
        }

        public List<LogData> Branches {
            get {
                return this.lstHistoryBranches;
            }
        }

        public byte[] CurrentIDL {
            get {
                return this.currentIDL;
            }
            set {
                this.currentIDL = value;
            }
        }

        public string CurrentPath {
            get {
                return this.currentPath;
            }
            set {
                if(value == null) {
                    this.currentPath = string.Empty;
                    base.ImageKey = string.Empty;
                }
                else {
                    this.currentPath = value;
                    base.ImageKey = value;
                }
            }
        }

        public int HistoryCount_Back {
            get {
                return this.stckHistoryBackward.Count;
            }
        }

        public int HistoryCount_Forward {
            get {
                return this.stckHistoryForward.Count;
            }
        }

        public string PathInitial {
            get {
                if((this.currentPath != null) && (this.currentPath.Length > 3)) {
                    char ch = this.currentPath[0];
                    if((('A' <= ch) && (ch <= 'Z')) || (('a' <= ch) && (ch <= 'z'))) {
                        return ch.ToString();
                    }
                }
                return string.Empty;
            }
        }

        public string TooltipText2 {
            get {
                bool fAllowSlow = Control.ModifierKeys == Keys.Shift;
                if(this.fNowSlowTip ^ fAllowSlow) {
                    this.tooltipText2 = null;
                }
                if(this.tooltipText2 == null) {
                    this.fNowSlowTip = fAllowSlow;
                    using(IDLWrapper wrapper = new IDLWrapper(this.currentIDL)) {
                        if(wrapper.Available) {
                            this.tooltipText2 = ShellMethods.GetShellInfoTipText(wrapper.PIDL, fAllowSlow);
                        }
                    }
                    if(((this.tooltipText2 == null) && !string.IsNullOrEmpty(this.currentPath)) && !this.currentPath.StartsWith(@"\\")) {
                        this.tooltipText2 = ShellMethods.GetShellInfoTipText(this.currentPath, fAllowSlow);
                    }
                }
                return this.tooltipText2;
            }
            set {
                this.fNowSlowTip = false;
                this.tooltipText2 = value;
            }
        }
    }
}
