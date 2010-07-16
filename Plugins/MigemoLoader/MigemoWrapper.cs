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
using System.Runtime.InteropServices;

namespace QuizoPlugins {
    sealed class MigemoWrapper : IDisposable {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll")]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        private delegate IntPtr migemo_open(string dict);
        private delegate IntPtr migemo_query(IntPtr pMigemo, string query);
        private delegate void migemo_release(IntPtr pMigemo, IntPtr stringToRelease);
        private delegate void migemo_close(IntPtr pMigemo);
        private delegate int migemo_is_enable(IntPtr pMigemo);

        private migemo_query mQuery;
        private migemo_release mRlease;
        private migemo_close mClose;
        private migemo_is_enable mIsEnable;

        private IntPtr pMigemo;
        private IntPtr hMoudleMigemo;

        public MigemoWrapper(string pathMigemoDll, string pathDict) {
            if(!String.IsNullOrEmpty(pathMigemoDll) && !String.IsNullOrEmpty(pathDict)) {
                this.hMoudleMigemo = LoadLibrary(pathMigemoDll);

                if(this.hMoudleMigemo != IntPtr.Zero) {
                    IntPtr pOpen = GetProcAddress(this.hMoudleMigemo, "migemo_open");
                    IntPtr pQuery = GetProcAddress(this.hMoudleMigemo, "migemo_query");
                    IntPtr pRelease = GetProcAddress(this.hMoudleMigemo, "migemo_release");
                    IntPtr pClose = GetProcAddress(this.hMoudleMigemo, "migemo_close");
                    IntPtr pIsEnable = GetProcAddress(this.hMoudleMigemo, "migemo_is_enable");

                    bool fSuccess = pOpen != IntPtr.Zero &&
                                    pQuery != IntPtr.Zero &&
                                    pRelease != IntPtr.Zero &&
                                    pClose != IntPtr.Zero &&
                                    pIsEnable != IntPtr.Zero;

                    if(fSuccess) {
                        migemo_open mOpen = Marshal.GetDelegateForFunctionPointer(pOpen, typeof(migemo_open)) as migemo_open;
                        this.mQuery = Marshal.GetDelegateForFunctionPointer(pQuery, typeof(migemo_query)) as migemo_query;
                        this.mRlease = Marshal.GetDelegateForFunctionPointer(pRelease, typeof(migemo_release)) as migemo_release;
                        this.mClose = Marshal.GetDelegateForFunctionPointer(pClose, typeof(migemo_close)) as migemo_close;
                        this.mIsEnable = Marshal.GetDelegateForFunctionPointer(pIsEnable, typeof(migemo_is_enable)) as migemo_is_enable;

                        if(mOpen != null &&
                            this.mQuery != null &&
                            this.mRlease != null &&
                            this.mClose != null &&
                            this.mIsEnable != null) {
                            this.pMigemo = mOpen(pathDict);

                            if(this.IsEnable) {
                                return;
                            }
                            else if(this.pMigemo != IntPtr.Zero) {
                                this.mClose(this.pMigemo);
                            }
                        }
                    }

                    FreeLibrary(this.hMoudleMigemo);
                    this.hMoudleMigemo = IntPtr.Zero;
                }
            }

            throw new ArgumentException();
        }

        public string QueryRegexStr(string strQuery) {
            if(this.IsEnable && strQuery != null) {
                IntPtr pRegexStr = IntPtr.Zero;
                try {
                    pRegexStr = this.mQuery(this.pMigemo, strQuery);
                    if(pRegexStr != IntPtr.Zero) {
                        return Marshal.PtrToStringAnsi(pRegexStr);
                    }
                }
                finally {
                    if(pRegexStr != IntPtr.Zero)
                        this.mRlease(this.pMigemo, pRegexStr);
                }

            }
            return strQuery;
        }

        public bool IsEnable {
            get {
                return this.pMigemo != IntPtr.Zero && 0 != this.mIsEnable(this.pMigemo);
            }
        }

        #region IDisposable member

        public void Dispose() {
            if(this.pMigemo != IntPtr.Zero) {
                this.mClose(this.pMigemo);
                this.pMigemo = IntPtr.Zero;
            }

            if(this.hMoudleMigemo != IntPtr.Zero) {
                FreeLibrary(this.hMoudleMigemo);
                this.hMoudleMigemo = IntPtr.Zero;
            }
        }

        #endregion

    }
}
