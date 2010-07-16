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
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    internal sealed class IDLWrapper : IDisposable {
        private uint attributes;
        private static Dictionary<string, byte[]> dicCacheIDLs;
        private static Dictionary<string, int> dicCSIDLTable;
        private static Dictionary<string, Guid> dicFOLDERIDTable;
        private static bool fCacheDirty;
        private bool fSpecial;
        private byte[] idl;
        internal const string INDICATOR_FTP = "ftp://";
        internal const string INDICATOR_HTTP = "http://";
        internal const string INDICATOR_NAMESPACE = "::";
        internal const string INDICATOR_NETWORK = @"\\";
        internal static int iPingTimeOutMS;
        private static List<string> lstPingSuccessedPaths;
        private string path;
        internal static string PATH_USERSFILES;
        private IntPtr pIDL;
        internal const string SEPARATOR_PATH_HASH = "???";

        static IDLWrapper() {
            try {
                string str;
                dicCacheIDLs = new Dictionary<string, byte[]>();
                dicCSIDLTable = new Dictionary<string, int>();
                dicFOLDERIDTable = new Dictionary<string, Guid>();
                lstPingSuccessedPaths = new List<string>();
                if(ShellMethods.TryGetFolderPath(5, out str)) {
                    dicCSIDLTable[str.ToLower()] = 5;
                }
                if(ShellMethods.TryGetFolderPath(0x27, out str)) {
                    dicCSIDLTable[str.ToLower()] = 0x27;
                }
                if(ShellMethods.TryGetFolderPath(0, out str)) {
                    dicCSIDLTable[str.ToLower()] = 0;
                }
                if(ShellMethods.TryGetFolderPath(13, out str)) {
                    dicCSIDLTable[str.ToLower()] = 13;
                }
                if(ShellMethods.TryGetFolderPath(14, out str)) {
                    dicCSIDLTable[str.ToLower()] = 14;
                }
                if(QTUtility.IsVista) {
                    string str2;
                    string str3;
                    string str4;
                    string str5;
                    string str6;
                    PATH_USERSFILES = ShellMethods.GetFolderPath(40).ToLower();
                    Guid rfid = new Guid("{374DE290-123F-4565-9164-39C4925E467B}");
                    Guid guid2 = new Guid("{4C5C32FF-BB9D-43b0-B5B4-2D72E54EAAA4}");
                    Guid guid3 = new Guid("{bfb9d5e0-c6a9-404c-b2b2-ae6db6af4968}");
                    Guid guid4 = new Guid("{56784854-C6CB-462b-8169-88E350ACB882}");
                    Guid guid5 = new Guid("{1777F761-68AD-4D8A-87BD-30B759FA33DD}");
                    PInvoke.SHGetKnownFolderPath(ref rfid, 0, IntPtr.Zero, out str2);
                    PInvoke.SHGetKnownFolderPath(ref guid2, 0, IntPtr.Zero, out str3);
                    PInvoke.SHGetKnownFolderPath(ref guid3, 0, IntPtr.Zero, out str4);
                    PInvoke.SHGetKnownFolderPath(ref guid4, 0, IntPtr.Zero, out str5);
                    PInvoke.SHGetKnownFolderPath(ref guid5, 0, IntPtr.Zero, out str6);
                    if(!string.IsNullOrEmpty(PATH_USERSFILES)) {
                        dicFOLDERIDTable[PATH_USERSFILES] = new Guid("{f3ce0f7c-4901-4acc-8648-d5d44b04ef8f}");
                        dicFOLDERIDTable[PATH_USERSFILES + @"\searches"] = new Guid("{7d1d3a04-debb-4115-95cf-2f29da2920da}");
                    }
                    if(!string.IsNullOrEmpty(str2)) {
                        dicFOLDERIDTable[str2.ToLower()] = rfid;
                    }
                    if(!string.IsNullOrEmpty(str3)) {
                        dicFOLDERIDTable[str3.ToLower()] = guid2;
                    }
                    if(!string.IsNullOrEmpty(str4)) {
                        dicFOLDERIDTable[str4.ToLower()] = guid3;
                    }
                    if(!string.IsNullOrEmpty(str5)) {
                        dicFOLDERIDTable[str5.ToLower()] = guid4;
                    }
                    if(!string.IsNullOrEmpty(str6)) {
                        dicFOLDERIDTable[str6.ToLower()] = guid5;
                    }
                }
                ReadCache();
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception, null, false);
            }
        }

        public IDLWrapper(IntPtr pIDL) {
            this.attributes = 0xfffffff0;
            this.pIDL = pIDL;
        }

        public IDLWrapper(byte[] idl, bool fValidate = true) {
            this.attributes = 0xfffffff0;
            this.idl = idl;
            if(this.idl != null) {
                this.pIDL = ShellMethods.CreateIDL(idl);
                if(fValidate && (ShellMethods.GetAttributes(this.pIDL, true) != 0)) {
                    this.idl = null;
                    this.pIDL = IntPtr.Zero;
                }
            }
        }

        public IDLWrapper(string path, bool fMsgModal = false) {
            this.attributes = 0xfffffff0;
            if(!string.IsNullOrEmpty(path)) {
                this.path = path;
                if(path.Contains("???") && dicCacheIDLs.TryGetValue(path, out this.idl)) {
                    this.pIDL = ShellMethods.CreateIDL(this.idl);
                }
                else {
                    this.pIDL = TranslateLocation(path);
                    if(this.pIDL == IntPtr.Zero) {
                        if(PathIsNetwork(this.path) && TryPing(this.path)) {
                            Application.DoEvents();
                            MessageForm.Show(IntPtr.Zero, "Timed out:    \"" + this.path + "\"", "Timed Out", MessageBoxIcon.Hand, 0x2710, fMsgModal);
                        }
                        else {
                            this.pIDL = PInvoke.ILCreateFromPath(path);
                        }
                    }
                    else {
                        this.fSpecial = true;
                    }
                }
            }
        }

        private IDLWrapper(string path, byte[] idl, IntPtr pIDL) {
            this.attributes = 0xfffffff0;
            this.path = path;
            this.idl = idl;
            this.pIDL = pIDL;
        }

        public static void AddCache(string path, byte[] idl) {
            dicCacheIDLs[path] = idl;
            fCacheDirty = true;
        }

        public static bool DirectoryExists(string path) {
            return ((!string.IsNullOrEmpty(path) && !path.StartsWith("::")) && Directory.Exists(path));
        }

        public void Dispose() {
            if(this.pIDL != IntPtr.Zero) {
                PInvoke.CoTaskMemFree(this.pIDL);
                this.pIDL = IntPtr.Zero;
            }
            this.path = null;
            this.idl = null;
        }

        public static bool FileExists(string path) {
            return ((!string.IsNullOrEmpty(path) && !path.StartsWith("::")) && File.Exists(path));
        }

        public static bool GetIDLHash(IntPtr pIDL, out int hash, out string key) {
            IShellFolder ppshf = null;
            IntPtr lParam = (IntPtr)0x10000000;
            try {
                if(PInvoke.SHGetDesktopFolder(out ppshf) == 0) {
                    foreach(string str in dicCacheIDLs.Keys) {
                        IntPtr ptr2 = ShellMethods.CreateIDL(dicCacheIDLs[str]);
                        if(ptr2 != IntPtr.Zero) {
                            if(ppshf.CompareIDs(lParam, pIDL, ptr2) == 0) {
                                key = str;
                                PInvoke.CoTaskMemFree(ptr2);
                                if(int.TryParse(str.Substring(str.IndexOf("???") + 3), out hash)) {
                                    return true;
                                }
                                goto Label_00B7;
                            }
                            PInvoke.CoTaskMemFree(ptr2);
                        }
                    }
                }
            }
            catch {
            }
            finally {
                if(ppshf != null) {
                    Marshal.ReleaseComObject(ppshf);
                }
            }
        Label_00B7:
            key = string.Empty;
            hash = DateTime.Now.GetHashCode();
            return false;
        }

        public static bool PathIsNetwork(string path) {
            if(!path.StartsWith(@"\\") && !path.StartsWith("http://")) {
                return path.StartsWith("ftp://");
            }
            return true;
        }

        private static void ReadCache() {
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar")) {
                if(key != null) {
                    iPingTimeOutMS = (int)key.GetValue("NetworkTimeout", 0);
                    using(RegistryKey key2 = key.CreateSubKey("Cache")) {
                        if(key2 != null) {
                            foreach(string str in key2.GetValueNames()) {
                                if(str.Contains("???")) {
                                    byte[] buffer = key2.GetValue(str, null) as byte[];
                                    if((buffer != null) && (buffer.Length > 0)) {
                                        dicCacheIDLs[str] = buffer;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void SaveCache(RegistryKey rkUser) {
            try {
                if(fCacheDirty && (rkUser != null)) {
                    rkUser.DeleteSubKey("Cache", false);
                    using(RegistryKey key = rkUser.CreateSubKey("Cache")) {
                        if(key != null) {
                            int num = dicCacheIDLs.Count - 0x30;
                            List<string> list = new List<string>();
                            if(num > 0) {
                                foreach(string str in dicCacheIDLs.Keys) {
                                    if(num <= 0) {
                                        break;
                                    }
                                    byte[] buffer = dicCacheIDLs[str];
                                    if(((buffer == null) || (buffer.Length == 0)) || (buffer[0] != 20)) {
                                        list.Add(str);
                                        num--;
                                    }
                                }
                                foreach(string str2 in list) {
                                    dicCacheIDLs.Remove(str2);
                                }
                                if(num > 0) {
                                    list.Clear();
                                    foreach(string str3 in dicCacheIDLs.Keys) {
                                        if(num <= 0) {
                                            break;
                                        }
                                        list.Add(str3);
                                        num--;
                                    }
                                    foreach(string str4 in list) {
                                        dicCacheIDLs.Remove(str4);
                                    }
                                }
                            }
                            foreach(string str5 in dicCacheIDLs.Keys) {
                                key.SetValue(str5, dicCacheIDLs[str5]);
                            }
                        }
                    }
                }
                fCacheDirty = false;
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception, null, false);
            }
        }

        private static IntPtr TranslateLocation(string path) {
            int num;
            Guid guid;
            IntPtr zero = IntPtr.Zero;
            if(dicCSIDLTable.TryGetValue(path.ToLower(), out num)) {
                PInvoke.SHGetFolderLocation(IntPtr.Zero, num, IntPtr.Zero, 0, out zero);
                return zero;
            }
            if(QTUtility.IsVista && dicFOLDERIDTable.TryGetValue(path.ToLower(), out guid)) {
                PInvoke.SHGetKnownFolderIDList(ref guid, 0, IntPtr.Zero, out zero);
            }
            return zero;
        }

        public static bool TryGetCache(string path, out IDLWrapper idlw) {
            byte[] buffer;
            idlw = null;
            if(dicCacheIDLs.TryGetValue(path, out buffer)) {
                IntPtr pIDL = ShellMethods.CreateIDL(buffer);
                idlw = new IDLWrapper(path, buffer, pIDL);
                return true;
            }
            return false;
        }

        private static bool TryPing(string path) {
            if(iPingTimeOutMS < 1) {
                return false;
            }
            if(path.Length > 2) {
                string hostNameOrAddress;
                if(path.StartsWith("http://") || path.StartsWith("ftp://")) {
                    int startIndex = path.StartsWith("http://") ? 7 : 6;
                    int index = path.IndexOf("/", startIndex);
                    if(index != -1) {
                        if(index <= startIndex) {
                            return true;
                        }
                        hostNameOrAddress = path.Substring(startIndex, index - startIndex);
                    }
                    else {
                        hostNameOrAddress = path.Substring(startIndex);
                    }
                }
                else {
                    path = path.Substring(2);
                    int length = path.IndexOf(@"\");
                    if(length == -1) {
                        hostNameOrAddress = path;
                    }
                    else {
                        hostNameOrAddress = path.Substring(0, length);
                    }
                }
                if(hostNameOrAddress.Length > 0) {
                    try {
                        if(!lstPingSuccessedPaths.Contains(path)) {
                            MessageForm.Show(IntPtr.Zero, path + "\r\n\r\nWait a moment...", "Accessing network...", MessageBoxIcon.None, 0);
                            Application.DoEvents();
                        }
                        using(Ping ping = new Ping()) {
                            if(ping.Send(hostNameOrAddress, iPingTimeOutMS).Status == IPStatus.Success) {
                                lstPingSuccessedPaths.Add(path);
                                return false;
                            }
                        }
                    }
                    catch(Exception exception) {
                        QTUtility2.MakeErrorLog(exception, null, true);
                    }
                }
            }
            lstPingSuccessedPaths.Remove(path);
            return true;
        }

        public bool Available {
            get {
                return (this.pIDL != IntPtr.Zero);
            }
        }

        public bool HasPath {
            get {
                return ((this.Path != null) && (this.path.Length > 0));
            }
        }

        public byte[] IDL {
            get {
                if((this.idl == null) && (this.pIDL != IntPtr.Zero)) {
                    this.idl = ShellMethods.GetIDLData(this.pIDL);
                }
                return this.idl;
            }
        }

        public bool IsDropTarget {
            get {
                if(this.attributes == 0xfffffff0) {
                    this.attributes = ShellMethods.GetAttributes(this.pIDL, false);
                }
                if(this.attributes == uint.MaxValue) {
                    return false;
                }
                return ((this.attributes & 0x100) == 0x100);
            }
        }

        public bool IsFileSystem {
            get {
                if(this.attributes == 0xfffffff0) {
                    this.attributes = ShellMethods.GetAttributes(this.pIDL, false);
                }
                if(this.attributes == uint.MaxValue) {
                    return false;
                }
                return ((this.attributes & 0x40000000) == 0x40000000);
            }
        }

        public bool IsFileSystemFile {
            get {
                try {
                    return FileExists(this.Path);
                }
                catch {
                }
                return false;
            }
        }

        public bool IsFileSystemFolder {
            get {
                try {
                    return DirectoryExists(this.Path);
                }
                catch {
                }
                return false;
            }
        }

        public bool IsFolder {
            get {
                if(this.attributes == 0xfffffff0) {
                    this.attributes = ShellMethods.GetAttributes(this.pIDL, false);
                }
                if(this.attributes == uint.MaxValue) {
                    return false;
                }
                return ((this.attributes & 0x20000000) == 0x20000000);
            }
        }

        public bool IsLink {
            get {
                if(this.attributes == 0xfffffff0) {
                    this.attributes = ShellMethods.GetAttributes(this.pIDL, false);
                }
                if(this.attributes == uint.MaxValue) {
                    return false;
                }
                return ((this.attributes & 0x10000) == 0x10000);
            }
        }

        public bool IsLinkToDeadFolder {
            get {
                bool flag = false;
                try {
                    if(!this.IsLink) {
                        return false;
                    }
                    string linkTargetPath = ShellMethods.GetLinkTargetPath(this.Path);
                    if(FileExists(linkTargetPath)) {
                        return false;
                    }
                    flag = !QTUtility2.IsShellPathButNotFileSystem(linkTargetPath) && !DirectoryExists(linkTargetPath);
                }
                catch {
                }
                return flag;
            }
        }

        public bool IsReadyIfDrive {
            get {
                if((this.path != null) && (this.path.Length == 3)) {
                    try {
                        return new DriveInfo(this.path).IsReady;
                    }
                    catch {
                    }
                }
                return true;
            }
        }

        public string Path {
            get {
                if((this.path == null) && (this.pIDL != IntPtr.Zero)) {
                    this.path = ShellMethods.GetPath(this.pIDL);
                }
                return this.path;
            }
        }

        public IntPtr PIDL {
            get {
                return this.pIDL;
            }
        }

        public bool Special {
            get {
                return this.fSpecial;
            }
        }
    }
}
