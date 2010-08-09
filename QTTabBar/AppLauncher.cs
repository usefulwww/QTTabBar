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
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using QTPlugin;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    internal sealed class AppLauncher {
        public int iSelDirsCount;
        public int iSelFileCount;
        public int iSelItemsCount;
        private static Regex reAll = new Regex("%(f|d|s|c|cd)%", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex reCrnt = new Regex("%c%", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex reDirs = new Regex("%d%", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex reDrCr = new Regex("%cd%", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex reFile = new Regex("%f%", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex reSlct = new Regex("%s%", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private string strCurrentPath;
        public string strSelDirs;
        public string strSelFiles;
        public string strSelObjs;

        public AppLauncher(Address[] addresses, string pathCurrent) {
            List<string> list = new List<string>();
            List<string> list2 = new List<string>();
            strCurrentPath = strSelFiles = strSelDirs = strSelObjs = string.Empty;
            iSelItemsCount = iSelFileCount = iSelDirsCount = 0;
            if(addresses != null) {
                iSelItemsCount = addresses.Length;
                for(int i = 0; i < addresses.Length; i++) {
                    using(IDLWrapper wrapper = new IDLWrapper(addresses[i].ITEMIDLIST)) {
                        if(wrapper.Available && wrapper.HasPath) {
                            if(wrapper.IsFileSystemFile) {
                                list.Add(addresses[i].Path);
                            }
                            else {
                                list2.Add(addresses[i].Path);
                            }
                        }
                    }
                }
            }
            if(Directory.Exists(pathCurrent)) {
                strCurrentPath = "\"" + pathCurrent + "\"";
            }
            foreach(string str in list) {
                strSelFiles = strSelFiles + "\"" + str + "\" ";
            }
            foreach(string str2 in list2) {
                strSelDirs = strSelDirs + "\"" + str2 + "\" ";
            }
            strSelObjs = (strSelFiles + strSelDirs).Trim();
            strSelFiles = strSelFiles.Trim();
            strSelDirs = strSelDirs.Trim();
            iSelFileCount = list.Count;
            iSelDirsCount = list2.Count;
        }

        public static void Execute(MenuItemArguments mia, IntPtr hwnd) {
            if(((mia == null) || (mia.Genre != MenuGenre.Application)) || string.IsNullOrEmpty(mia.Path)) {
                QTUtility2.MakeErrorLog(null, "Applauncher.Execute bug found");
            }
            else {
                string path = mia.Path;
                string argument = mia.Argument;
                string workingDirectory = mia.WorkingDirectory;
                if(!string.Equals(path, "separator", StringComparison.OrdinalIgnoreCase)) {
                    bool flag = false;
                    if(path.StartsWith("::")) {
                        using(IDLWrapper wrapper = new IDLWrapper(path)) {
                            if(wrapper.Available) {
                                SHELLEXECUTEINFO structure = new SHELLEXECUTEINFO();
                                structure.cbSize = Marshal.SizeOf(structure);
                                structure.nShow = 1;
                                structure.fMask = 4;
                                structure.lpIDList = wrapper.PIDL;
                                structure.hwnd = hwnd;
                                PInvoke.ShellExecuteEx(ref structure);
                                return;
                            }
                        }
                        flag = true;
                    }
                    if(path.StartsWith(@"\\")) {
                        using(IDLWrapper wrapper2 = new IDLWrapper(path)) {
                            if(!wrapper2.Available) {
                                flag = true;
                            }
                        }
                    }
                    if(!flag && Directory.Exists(path)) {
                        ProcessStartInfo startInfo = new ProcessStartInfo(path);
                        startInfo.ErrorDialog = true;
                        try {
                            Process.Start(startInfo);
                        }
                        catch {
                        }
                        return;
                        //flag = true;
                    }
                    if(!flag) {
                        SHELLEXECUTEINFO shellexecuteinfo2 = new SHELLEXECUTEINFO();
                        shellexecuteinfo2.cbSize = Marshal.SizeOf(shellexecuteinfo2);
                        shellexecuteinfo2.nShow = 1;
                        shellexecuteinfo2.fMask = 0x4000200;
                        shellexecuteinfo2.hwnd = hwnd;
                        try {
                            shellexecuteinfo2.lpFile = Marshal.StringToHGlobalUni(path);
                            if(!string.IsNullOrEmpty(argument)) {
                                shellexecuteinfo2.lpParameters = Marshal.StringToHGlobalUni(argument);
                            }
                            if(!string.IsNullOrEmpty(workingDirectory)) {
                                workingDirectory = workingDirectory.Trim(new char[] { '"', '\'' });
                                shellexecuteinfo2.lpDirectory = Marshal.StringToHGlobalUni(workingDirectory);
                            }
                            else {
                                try {
                                    shellexecuteinfo2.lpDirectory = Marshal.StringToHGlobalUni(Path.GetDirectoryName(path));
                                }
                                catch {
                                }
                            }
                            if(PInvoke.ShellExecuteEx(ref shellexecuteinfo2)) {
                                string extension = Path.GetExtension(path);
                                if(!string.Equals(".exe", extension, StringComparison.OrdinalIgnoreCase) && !string.Equals(".com", extension, StringComparison.OrdinalIgnoreCase)) {
                                    QTUtility.ExecutedPathsList.Add(path);
                                }
                            }
                        }
                        finally {
                            if(shellexecuteinfo2.lpFile != IntPtr.Zero) {
                                Marshal.FreeHGlobal(shellexecuteinfo2.lpFile);
                            }
                            if(shellexecuteinfo2.lpParameters != IntPtr.Zero) {
                                Marshal.FreeHGlobal(shellexecuteinfo2.lpParameters);
                            }
                            if(shellexecuteinfo2.lpDirectory != IntPtr.Zero) {
                                Marshal.FreeHGlobal(shellexecuteinfo2.lpDirectory);
                            }
                        }
                    }
                    else {
                        if(!string.IsNullOrEmpty(argument)) {
                            path = path + ", " + argument;
                        }
                        if(!string.IsNullOrEmpty(workingDirectory)) {
                            path = path + ", " + workingDirectory;
                        }
                        MessageBox.Show("Operation failed.\r\nPlease make sure the file or the target of link exists:\r\n\r\n" + path, "Error - QTTabBar", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    }
                }
            }
        }

        public static bool IsTokened(MenuItemArguments mia) {
            bool flag = false;
            if(!string.IsNullOrEmpty(mia.OriginalWorkingDirectory)) {
                flag = reAll.IsMatch(mia.OriginalWorkingDirectory);
            }
            if(!flag && !string.IsNullOrEmpty(mia.OriginalArgument)) {
                flag = reAll.IsMatch(mia.OriginalArgument);
            }
            return flag;
        }

        public static void ReplaceAllTokens(MenuItemArguments mia, string strCurrentPath) {
            AppLauncher launcher = new AppLauncher(null, strCurrentPath);
            launcher.ReplaceTokens_WorkingDir(mia);
            launcher.ReplaceTokens_Arguments(mia);
        }

        public int ReplaceTokens_Arguments(MenuItemArguments mia) {
            int num = 0;
            mia.TokenReplaced = true;
            if(!string.IsNullOrEmpty(mia.Argument)) {
                if(iSelItemsCount > 0) {
                    if(reSlct.IsMatch(mia.Argument)) {
                        mia.Argument = reSlct.Replace(mia.Argument, strSelObjs);
                        num |= 1;
                    }
                    if(reFile.IsMatch(mia.Argument)) {
                        mia.Argument = reFile.Replace(mia.Argument, strSelFiles);
                        num |= 2;
                    }
                    if(reDirs.IsMatch(mia.Argument)) {
                        mia.Argument = reDirs.Replace(mia.Argument, strSelDirs);
                        num |= 4;
                    }
                    if(reDrCr.IsMatch(mia.Argument)) {
                        if(strSelDirs.Length > 0) {
                            mia.Argument = reDrCr.Replace(mia.Argument, strSelDirs);
                            num |= 4;
                        }
                        else if(strCurrentPath != null) {
                            mia.Argument = reDrCr.Replace(mia.Argument, strCurrentPath);
                            num |= 8;
                        }
                    }
                }
                else if((strCurrentPath != null) && reDrCr.IsMatch(mia.Argument)) {
                    mia.Argument = reDrCr.Replace(mia.Argument, strCurrentPath);
                    num |= 8;
                }
                else if(reAll.IsMatch(mia.Argument)) {
                    num = 10;
                }
                if((strCurrentPath != null) && reCrnt.IsMatch(mia.Argument)) {
                    mia.Argument = reCrnt.Replace(mia.Argument, strCurrentPath);
                    num |= 8;
                }
                mia.Argument = reAll.Replace(mia.Argument, string.Empty);
            }
            return num;
        }

        public bool ReplaceTokens_WorkingDir(MenuItemArguments mia) {
            bool flag = false;
            mia.TokenReplaced = true;
            if(!string.IsNullOrEmpty(mia.WorkingDirectory)) {
                if(iSelItemsCount == 1) {
                    if(reSlct.IsMatch(mia.WorkingDirectory) && Directory.Exists(strSelDirs.Trim(new char[] { '"' }))) {
                        mia.WorkingDirectory = reSlct.Replace(mia.WorkingDirectory, strSelDirs);
                        flag = true;
                    }
                    else if(reDirs.IsMatch(mia.WorkingDirectory) && (strSelDirs.Length > 0)) {
                        mia.WorkingDirectory = reDirs.Replace(mia.WorkingDirectory, strSelDirs);
                        flag = true;
                    }
                    else if(reDrCr.IsMatch(mia.WorkingDirectory) && Directory.Exists(strSelDirs.Trim(new char[] { '"' }))) {
                        mia.WorkingDirectory = reDrCr.Replace(mia.WorkingDirectory, strSelDirs);
                        flag = true;
                    }
                }
                if(!flag && (strCurrentPath != null)) {
                    if(reDrCr.IsMatch(mia.WorkingDirectory)) {
                        mia.WorkingDirectory = reDrCr.Replace(mia.WorkingDirectory, strCurrentPath);
                        flag = true;
                    }
                    else if(reCrnt.IsMatch(mia.WorkingDirectory)) {
                        mia.WorkingDirectory = reCrnt.Replace(mia.WorkingDirectory, strCurrentPath);
                        flag = true;
                    }
                }
                mia.WorkingDirectory = reAll.Replace(mia.WorkingDirectory, string.Empty);
            }
            return flag;
        }
    }
}
