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
using System.IO;
using System.Linq;
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
            bool needsRefresh = false;
            char[] separator = new char[] { Path.DirectorySeparatorChar };
            Dictionary<string, List<QTabItem>> commonTextTabs = new Dictionary<string, List<QTabItem>>();
            foreach(QTabItem item in tabControl.TabPages) {
                if(!item.CurrentPath.StartsWith("::")) {
                    string text = item.Text.ToLower();
                    if(commonTextTabs.ContainsKey(text)) {
                        commonTextTabs[text].Add(item);
                    }
                    else {
                        commonTextTabs[text] = new List<QTabItem> { item };
                    }
                }
            }
            foreach(List<QTabItem> tabs in commonTextTabs.Values) {
                if(tabs.Count > 1) {
                    if(tabs.All(tab => tab.CurrentPath == tabs[0].CurrentPath)) {
                        foreach(QTabItem tab in tabs.Where(item => item.Comment.Length > 0)) {
                            tab.Comment = string.Empty;
                            tab.RefreshRectangle();
                            needsRefresh = true;
                        }
                    }
                    else {
                        List<string[]> pathArrays = tabs.Select(item => item.CurrentPath
                                .Split(separator).Reverse().Skip(1).ToArray()).ToList();
                        for(int i = 0; i < tabs.Count; i++) {
                            string comment = pathArrays[i].FirstOrDefault(str => !pathArrays.Where(
                                    (path, j) => i != j && path.Contains(str)).Any()) ?? tabs[i].currentPath;
                            if(comment.Length == 2 && comment[1] == ':') {
                                comment += @"\";
                            }
                            if(tabs[i].Comment != comment) {
                                tabs[i].Comment = comment;
                                tabs[i].RefreshRectangle();
                                needsRefresh = true;
                            }
                        }
                    }
                }
                else if(tabs[0].Comment.Length > 0) {
                    needsRefresh = true;
                    tabs[0].Comment = string.Empty;
                    tabs[0].RefreshRectangle();
                }
            }
            if(needsRefresh) {
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
            Dictionary<string, Address[]> dictionary = dicSelectdItems.Keys
                    .ToDictionary(str => str, str => dicSelectdItems[str]);
            item.dicSelectdItems = dictionary;
            return item;
        }

        public string[] GetHistoryBack() {
            return stckHistoryBackward.Select(data => data.Path).ToArray();
        }

        public string[] GetHistoryForward() {
            return stckHistoryForward.Select(data => data.Path).ToArray();
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

        public void NavigatedTo(string path, byte[] idl, int hash, bool autoNav) {
            if((idl == null) || (idl.Length == 0)) {
                idl = ShellMethods.GetIDLData(path);
            }
            if(autoNav && stckHistoryBackward.Count > 0 && stckHistoryBackward.Peek().AutoNav) {
                stckHistoryBackward.Pop();
            }
            stckHistoryBackward.Push(new LogData(path, idl, hash, autoNav));
            lstHistoryBranches.AddRange(stckHistoryForward.Except(lstHistoryBranches));
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
