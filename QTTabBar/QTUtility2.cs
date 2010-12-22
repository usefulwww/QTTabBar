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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    internal static class QTUtility2 {
        private const int THRESHOLD_ELLIPSIS = 40;
        private static bool fConsoleAllocated;

        public static void AllocDebugConsole() {
            if(fConsoleAllocated) {
                return;
            }
            const int STD_OUTPUT_HANDLE = -11;
            const int MY_CODE_PAGE = 437;
            PInvoke.AllocConsole();
            IntPtr stdHandle = PInvoke.GetStdHandle(STD_OUTPUT_HANDLE);
            SafeFileHandle safeFileHandle = new SafeFileHandle(stdHandle, true);
            FileStream fileStream = new FileStream(safeFileHandle, FileAccess.Write);
            Encoding encoding = System.Text.Encoding.GetEncoding(MY_CODE_PAGE);
            StreamWriter standardOutput = new StreamWriter(fileStream, encoding);
            standardOutput.AutoFlush = true;
            Console.SetOut(standardOutput);
            fConsoleAllocated = true;
        }

        public static int GET_X_LPARAM(IntPtr lParam) {
            return (short)(((int)lParam) & 0xffff);
        }

        public static int GET_Y_LPARAM(IntPtr lParam) {
            return (short)((((int)lParam) >> 0x10) & 0xffff);
        }

        public static string GetDriveDisplayText(string path) {
            if((path.Length != 3) || !path.EndsWith(@":\")) {
                return string.Empty;
            }
            switch(PInvoke.GetDriveType(path)) {
                case 0:
                case 1:
                case 4:
                    return path;
            }
            return ShellMethods.GetDisplayName(path);
        }

        public static T GetRegistryValueSafe<T>(RegistryKey rk, string valName, T defaultVal) {
            object obj2 = rk.GetValue(valName, defaultVal);
            if((obj2 != null) && (obj2 is T)) {
                return (T)obj2;
            }
            return defaultVal;
        }

        public static void InitializeTemporaryPaths() {
            QTUtility.TMPPathList.Clear();
            QTUtility.TMPIDLList.Clear();
            QTUtility.TMPTargetIDL = null;
        }

        public static bool IsNetworkPath(string path) {
            if(path.StartsWith("::")) {
                return false;
            }
            else if(path.StartsWith(@"\\")) {
                return true;
            }
            try {
                if(Path.IsPathRooted(path)) {
                    DriveInfo drive = new DriveInfo(Path.GetPathRoot(path));
                    if(drive.DriveType == DriveType.Network) {
                        return true;
                    }
                }
            }
            catch {
            }
            return false;
        }

        public static bool IsShellPathButNotFileSystem(string path) {
            path = path.ToLower();
            return ((!path.StartsWith("http://") && !path.StartsWith("ftp://")) && !Path.IsPathRooted(path));
        }

        public static bool IsValidPathChar(char ch) {
            return (((((ch != '"') && (ch != '<')) && ((ch != '>') && (ch != '|'))) && (ch != '*')) && (ch != '?'));
        }

        public static int Make_INT(int x, int y) {
            return ((x & 0xffff) | ((y & 0xffff) << 0x10));
        }

        public static IntPtr Make_LPARAM(int x, int y) {
            return (IntPtr)((x & 0xffff) | ((y & 0xffff) << 0x10));
        }

        public static IntPtr Make_LPARAM(Point pt) {
            return (IntPtr)((pt.X & 0xffff) | ((pt.Y & 0xffff) << 0x10));
        }
        
        public static Color MakeColor(int colorref) {
            return Color.FromArgb(colorref & 0xff, (colorref >> 8) & 0xff, (colorref >> 0x10) & 0xff);
        }

        public static int MakeCOLORREF(Color clr) {
            return ((clr.R | (clr.G << 8)) | (clr.B << 0x10));
        }

        public static void MakeErrorLog(Exception ex, string optional, bool fCritical = false) {
            // TODO Make a release-acceptable version of this code

            //if(QTUtility.NowDebugging || fCritical) {
                try {
                    string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\QTTabBarException.log";
                    bool flag = File.Exists(path);
                    using(StreamWriter writer = new StreamWriter(path, true)) {
                        if(!flag) {
                            writer.WriteLine("This file was created by QTTabBar for debugging. Sorry for inconvenience. You can delete this without any harm.");
                            writer.WriteLine("--------------");
                            writer.WriteLine();
                        }
                        writer.WriteLine(DateTime.Now.ToString());
                        writer.WriteLine("OS ver: " + Environment.OSVersion.Version);
                        writer.WriteLine("QT ver: " + MakeVersionString());
                        writer.WriteLine((ex == null) ? "Exception : N/A" : ex.ToString());
                        if(!string.IsNullOrEmpty(optional)) {
                            writer.WriteLine("Optional information : " + optional);
                        }
                        writer.WriteLine("--------------");
                        writer.WriteLine();
                    }
                    SystemSounds.Exclamation.Play();
                }
                catch {
                }
            //}
        }

        public static string MakeKeyString(Keys key) {
            if(key == Keys.None) {
                return " - ";
            }
            string str = string.Empty;
            if((key & Keys.Control) == Keys.Control) {
                str = "Ctrl + ";
            }
            if((key & Keys.Shift) == Keys.Shift) {
                str = str + "Shift + ";
            }
            if((key & Keys.Alt) == Keys.Alt) {
                str = str + "Alt + ";
            }
            return (str + ((key & Keys.KeyCode)));
        }

        public static Color MakeModColor(Color clr) {
            float num = 0.875f;
            return Color.FromArgb(((int)((0xff - clr.R) * num)) + clr.R, ((int)((0xff - clr.G) * num)) + clr.G, ((int)((0xff - clr.B) * num)) + clr.B);
        }

        public static string MakeNameEllipsis(string name, out bool fTruncated) {
            fTruncated = false;
            if(name.Length > 40) {
                name = name.Substring(0, 0x25) + "...";
                fTruncated = true;
            }
            return name;
        }

        public static string MakePathDisplayText(string path, bool fToolTip) {
            int index = path.IndexOf("???");
            int length = path.IndexOf("*?*?*");
            if((index != -1) && IsShellPathButNotFileSystem(path)) {
                return path.Substring(0, index);
            }
            if(fToolTip && !path.StartsWith("::")) {
                if(length != -1) {
                    return path.Substring(0, length);
                }
                return path;
            }
            if(((path.Length == 3) && path.EndsWith(@":\")) || (path.StartsWith("::") || (length != -1))) {
                string driveDisplayText;
                if(QTUtility.DisplayNameCacheDic.TryGetValue(path, out driveDisplayText)) {
                    return driveDisplayText;
                }
                if(path.Length == 3) {
                    driveDisplayText = GetDriveDisplayText(path);
                }
                else if(length != -1) {
                    string str2 = path.Substring(0, length);
                    if(path.StartsWith("::")) {
                        driveDisplayText = ShellMethods.GetDisplayName(str2);
                    }
                    else {
                        driveDisplayText = str2;
                    }
                }
                else {
                    driveDisplayText = ShellMethods.GetDisplayName(path);
                }
                if(!string.IsNullOrEmpty(driveDisplayText)) {
                    QTUtility.DisplayNameCacheDic[path] = driveDisplayText;
                    return driveDisplayText;
                }
                return path;
            }
            if(!path.StartsWith("http://") && !path.StartsWith("ftp://")) {
                try {
                    string fileName = Path.GetFileName(path);
                    if(fileName.Length > 0x40) {
                        fileName = fileName.Substring(0, 0x3d) + "...";
                    }
                    return fileName;
                }
                catch {
                }
            }
            return path;
        }

        public static string MakeRootName(string path) {
            if((path == null) || (path.Length <= 2)) {
                return path;
            }
            if(path.StartsWith(@"\\")) {
                int index = path.IndexOf(@"\", 2);
                if(index != -1) {
                    return path.Substring(0, index);
                }
                return path;
            }
            return path.Substring(0, 3);
        }

        public static string MakeVersionString() {
            if(QTUtility.fIsDevelopmentVersion) {
                return "DevBuild: " + QTUtility.GetLinkerTimestamp();
            }
            else {
                string str = QTUtility.CurrentVersion.ToString();
                if(QTUtility.BetaRevision.Major > 0) {
                    str = str + " Beta " + QTUtility.BetaRevision.Major;
                }
                else if(QTUtility.BetaRevision.Minor > 0) {
                    str = str + " Alpha " + QTUtility.BetaRevision.Minor;
                }
                return str;
            }
        }

        public static bool PathEquals(this string str1, string str2) {
            return string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);
        }

        public static bool PathExists(string path) {
            if(string.IsNullOrEmpty(path)) {
                return false;
            }
            path = path.ToLower();
            if(path.StartsWith("::") || path.StartsWith(@"\\") || path.StartsWith("http://") || path.StartsWith("ftp://") || path.Contains("???")) {
                return true;
            }
            if(Path.IsPathRooted(path)) {
                DriveInfo drive = new DriveInfo(Path.GetPathRoot(path));
                switch(drive.DriveType) {
                    case DriveType.Unknown:
                    case DriveType.NoRootDirectory:
                        return false;
                    case DriveType.Network:
                        return true;
                }
            }
            if(Directory.Exists(path)) {
                return true;
            }
            if(File.Exists(path)) {
                string ext = Path.GetExtension(path).ToLower();
                return (QTUtility.ExtIsCompressed(ext) || (!QTUtility.IsXP && (ext == ".search-ms")));
            }
            if(QTUtility.IsXP || ((!path.Contains(@".zip\") && !path.Contains(@".cab\")) && !path.Contains(@".lzh\"))) {
                return !Path.IsPathRooted(path);
            }
            string str2 = string.Empty;
            if(path.Contains(@".zip\")) {
                str2 = @".zip\";
            }
            else if(path.Contains(@".cab\")) {
                str2 = @".cab\";
            }
            else if(path.Contains(@".lzh\")) {
                str2 = @".lzh\";
            }
            return File.Exists(path.Substring(0, path.IndexOf(str2) + 4));
        }

        public static Point PointFromLPARAM(IntPtr lParam) {
            return new Point(((int)lParam) & 0xffff, (((int)lParam) >> 0x10) & 0xffff);
        }

        public static T[] ReadRegBinary<T>(string regValueName, RegistryKey rkUserApps) {
            byte[] buffer;
            try {
                buffer = (byte[])rkUserApps.GetValue(regValueName, null);
            }
            catch {
                return null;
            }
            if((buffer != null) && (buffer.Length > 0)) {
                using(MemoryStream stream = new MemoryStream(buffer)) {
                    BinaryFormatter formatter = new BinaryFormatter();
                    return (T[])formatter.Deserialize(stream);
                }
            }
            return null;
        }

        public static IntPtr ReadRegHandle(string valName, RegistryKey rk) {
            if(IntPtr.Size == 4) {
                object obj2 = rk.GetValue(valName, 0);
                if(obj2 is int) {
                    return (IntPtr)((int)obj2);
                }
                return (IntPtr)((uint)obj2);
            }
            else {
                object obj2 = rk.GetValue(valName, 0L);
                if(obj2 is long) {
                    return (IntPtr)((long)obj2);
                }
                return (IntPtr)((ulong)obj2);
            }
        }

        public static string SanitizePathString(string path) {
            if(path == null) {
                return null;
            }
            path = path.Trim();
            StringBuilder builder = new StringBuilder(path.Length);
            foreach(char ch in path) {
                if(IsValidPathChar(ch) && (ch > '\x001f')) {
                    builder.Append(ch);
                }
            }
            return builder.ToString();
        }

        public static IntPtr SendCOPYDATASTRUCT(IntPtr hWnd, IntPtr wParam, string strMsg, IntPtr dwData) {
            if(string.IsNullOrEmpty(strMsg)) {
                strMsg = "null";
            }
            COPYDATASTRUCT structure = new COPYDATASTRUCT();
            IntPtr hglobal = Marshal.StringToHGlobalUni(strMsg);
            structure.lpData = hglobal;
            structure.cbData = (strMsg.Length + 1) * 2;
            structure.dwData = dwData;
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(structure));
            Marshal.StructureToPtr(structure, ptr, false);
            IntPtr ptr3 = PInvoke.SendMessage(hWnd, WM.COPYDATA, wParam, ptr);
            Marshal.FreeHGlobal(hglobal);
            Marshal.FreeHGlobal(ptr);
            return ptr3;
        }

        public static string StringJoin<T>(this IEnumerable<T> list, string separator) {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach(T t in list) {
                if(first) first = false;
                else sb.Append(separator);
                sb.Append(t.ToString());
            }
            return sb.ToString();
        }

        public static string StringJoin(this IEnumerable list, string separator) {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach(object t in list) {
                if(first) first = false;
                else sb.Append(separator);
                sb.Append(t.ToString());
            }
            return sb.ToString();
        }

        public static bool TargetIsInNoCapture(IntPtr pIDL, string path) {
            if(pIDL != IntPtr.Zero) {
                path = ShellMethods.GetPath(pIDL);
            }
            return !string.IsNullOrEmpty(path) && QTUtility.NoCapturePathsList.Any(path2 => path.PathEquals(path2));
        }

        public static void WriteRegBinary<T>(T[] array, string regValueName, RegistryKey rkUserApps) {
            if(array != null) {
                byte[] buffer;
                BinaryFormatter formatter = new BinaryFormatter();
                using(MemoryStream stream = new MemoryStream()) {
                    formatter.Serialize(stream, array);
                    buffer = stream.GetBuffer();
                }
                int num = 0;
                for(int i = 0; i < buffer.Length; i++) {
                    if(buffer[i] == 0) {
                        if(num == 0) {
                            num = i;
                        }
                    }
                    else {
                        num = 0;
                    }
                }
                byte[] buffer2 = new byte[num];
                if(num != 0) {
                    for(int j = 0; j < num; j++) {
                        buffer2[j] = buffer[j];
                    }
                }
                else {
                    buffer2 = buffer;
                }
                if(rkUserApps != null) {
                    rkUserApps.SetValue(regValueName, buffer2);
                }
            }
        }

        public static void WriteRegHandle(string valName, RegistryKey rk, IntPtr hwnd) {
            if(IntPtr.Size == 4) {
                rk.SetValue(valName, (int)hwnd);
            }
            else {
                rk.SetValue(valName, (long)hwnd, RegistryValueKind.QWord);
            }
        }
    }
}
