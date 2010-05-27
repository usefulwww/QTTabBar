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

using QTTabBarLib.Interop;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace QTTabBarLib.Automation {
    class AutomationQueryManager : IDisposable {
        private static readonly Guid IID_IUIAutomation = new Guid("{30CBE57D-D9D0-452A-AB13-7AC5AC4825EE}");
        private static readonly Guid CLSID_CUIAutomation = new Guid("{FF48DBA4-60EF-4201-AA87-54103EEF594E}");

        private static IUIAutomation pAutomation;
        public delegate T Query<T>(AutomationElementManager manager);

        private class Worker<T> {
            private T ret;
            private Query<T> query;

            public Worker(Query<T> query) {
                this.query = query;
            }

            public void DoWork(object state) {
                using(AutomationElementManager man = new AutomationElementManager(pAutomation)) {
                    ret = query(man);
                }
                ((AutoResetEvent)state).Set();
            }

            public T GetReturn() {
                return ret;
            }
        };

        public AutomationQueryManager() {
            Guid rclsid = CLSID_CUIAutomation;
            Guid riid = IID_IUIAutomation;
            object obj = null;
            PInvoke.CoCreateInstance(ref rclsid, IntPtr.Zero, 1, ref riid, out obj);
            if(obj == null) return;
            pAutomation = obj as IUIAutomation;
        }

        ~AutomationQueryManager() {
            Dispose();
        }

        public void Dispose() {
            if(pAutomation != null) {
                Marshal.ReleaseComObject(pAutomation);
                pAutomation = null;
            }
            GC.SuppressFinalize(this);
        }

        public T DoQuery<T>(Query<T> query) {
            WaitHandle handle = new AutoResetEvent(false);
            Worker<T> worker = new Worker<T>(query);
            ThreadPool.QueueUserWorkItem(new WaitCallback(worker.DoWork), handle);
            handle.WaitOne();
            return worker.GetReturn();
        }
    }
}
