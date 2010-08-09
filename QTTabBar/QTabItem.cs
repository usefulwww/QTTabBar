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
using System.Windows.Forms;
using QTPlugin;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
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
            stckHistoryForward = new Stack<LogData>();
            stckHistoryBackward = new Stack<LogData>();
            dicSelectdItems = new Dictionary<string, Address[]>();
            dicFocusedItemName = new Dictionary<string, string>();
            lstHistoryBranches = new List<LogData>();
            CurrentPath = path;
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
                                        if((item4 == item3) || item4.CurrentPath == item3.CurrentPath) {
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

        public QTabItem Clone(bool fAll = false) {
            QTabItem item = new QTabItem(Text, currentPath, Owner);
            item.TabBounds = TabBounds;
            item.Comment = Comment;
            item.currentIDL = currentIDL;
            item.ToolTipText = ToolTipText;
            item.tooltipText2 = tooltipText2;
            if(fAll) {
                item.tabLocked = tabLocked;
            }
            LogData[] array = stckHistoryForward.ToArray();
            LogData[] dataArray2 = stckHistoryBackward.ToArray();
            Array.Reverse(array);
            Array.Reverse(dataArray2);
            item.stckHistoryForward = new Stack<LogData>(array);
            item.stckHistoryBackward = new Stack<LogData>(dataArray2);
            item.dicFocusedItemName = new Dictionary<string, string>(dicFocusedItemName);
            item.lstHistoryBranches = new List<LogData>(lstHistoryBranches.ToArray());
            Dictionary<string, Address[]> dictionary = new Dictionary<string, Address[]>();
            foreach(string str in dicSelectdItems.Keys) {
                dictionary.Add(str, dicSelectdItems[str]);
            }
            item.dicSelectdItems = dictionary;
            return item;
        }

        public string[] GetHistoryBack() {
            List<string> list = new List<string>();
            foreach(LogData data in stckHistoryBackward) {
                list.Add(data.Path);
            }
            return list.ToArray();
        }

        public string[] GetHistoryForward() {
            List<string> list = new List<string>();
            foreach(LogData data in stckHistoryForward) {
                list.Add(data.Path);
            }
            return list.ToArray();
        }

        public int GetLogHash(bool back, int index) {
            LogData[] dataArray;
            if(back) {
                dataArray = stckHistoryBackward.ToArray();
            }
            else {
                dataArray = stckHistoryForward.ToArray();
            }
            if((index > -1) && (index < dataArray.Length)) {
                return dataArray[index].Hash;
            }
            return -1;
        }

        public IEnumerable<LogData> GetLogs(bool fBack) {
            List<LogData> list = new List<LogData>(fBack ? stckHistoryBackward : stckHistoryForward);
            if(fBack && (list.Count > 0)) {
                list.RemoveAt(0);
            }
            return list;
        }

        public Address[] GetSelectedItemsAt(string path, out string focused) {
            Address[] addressArray;
            dicSelectdItems.TryGetValue(path, out addressArray);
            dicFocusedItemName.TryGetValue(path, out focused);
            return addressArray;
        }

        public LogData GoBackward() {
            LogData data = new LogData();
            if(stckHistoryBackward.Count > 1) {
                stckHistoryForward.Push(stckHistoryBackward.Pop());
                data = stckHistoryBackward.Peek();
                CurrentPath = data.Path;
            }
            return data;
        }

        public LogData GoForward() {
            LogData data = new LogData();
            if(stckHistoryForward.Count != 0) {
                stckHistoryBackward.Push(stckHistoryForward.Pop());
                data = stckHistoryBackward.Peek();
                CurrentPath = data.Path;
            }
            return data;
        }

        public void NavigatedTo(string path, byte[] idl, int hash) {
            if((idl == null) || (idl.Length == 0)) {
                idl = ShellMethods.GetIDLData(path);
            }
            stckHistoryBackward.Push(new LogData(path, idl, hash));
            foreach(LogData data in stckHistoryForward) {
                if(!lstHistoryBranches.Contains(data)) {
                    lstHistoryBranches.Add(data);
                }
            }
            foreach(LogData data2 in stckHistoryBackward) {
                lstHistoryBranches.Remove(data2);
            }
            stckHistoryForward.Clear();
            CurrentPath = path;
            currentIDL = idl;
        }

        public override void OnClose() {
            if(Closed != null) {
                Closed(null, EventArgs.Empty);
                Closed = null;
            }
            base.OnClose();
        }

        public void SetSelectedItemsAt(string path, Address[] names, string focused) {
            dicSelectdItems[path] = names;
            dicFocusedItemName[path] = focused;
        }

        public List<LogData> Branches {
            get {
                return lstHistoryBranches;
            }
        }

        public byte[] CurrentIDL {
            get {
                return currentIDL;
            }
            set {
                currentIDL = value;
            }
        }

        public string CurrentPath {
            get {
                return currentPath;
            }
            set {
                if(value == null) {
                    currentPath = string.Empty;
                    ImageKey = string.Empty;
                }
                else {
                    currentPath = value;
                    ImageKey = value;
                }
            }
        }

        public int HistoryCount_Back {
            get {
                return stckHistoryBackward.Count;
            }
        }

        public int HistoryCount_Forward {
            get {
                return stckHistoryForward.Count;
            }
        }

        public string PathInitial {
            get {
                if((currentPath != null) && (currentPath.Length > 3)) {
                    char ch = currentPath[0];
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
                if(fNowSlowTip ^ fAllowSlow) {
                    tooltipText2 = null;
                }
                if(tooltipText2 == null) {
                    fNowSlowTip = fAllowSlow;
                    using(IDLWrapper wrapper = new IDLWrapper(currentIDL)) {
                        if(wrapper.Available) {
                            tooltipText2 = ShellMethods.GetShellInfoTipText(wrapper.PIDL, fAllowSlow);
                        }
                    }
                    if(((tooltipText2 == null) && !string.IsNullOrEmpty(currentPath)) && !currentPath.StartsWith(@"\\")) {
                        tooltipText2 = ShellMethods.GetShellInfoTipText(currentPath, fAllowSlow);
                    }
                }
                return tooltipText2;
            }
            set {
                fNowSlowTip = false;
                tooltipText2 = value;
            }
        }
    }
}
