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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace QTTabBarLib.Automation {
    // All interaction with AutomationElements MUST be done in a thread other
    // than the UI thread.  Use this class to execute code in the Automation 
    // thread.
    // http://msdn.microsoft.com/en-us/library/ee671692%28VS.85%29.aspx

    class AutomationManager : IDisposable {
        private static readonly Guid IID_IUIAutomation = new Guid("{30CBE57D-D9D0-452A-AB13-7AC5AC4825EE}");
        private static readonly Guid CLSID_CUIAutomation = new Guid("{FF48DBA4-60EF-4201-AA87-54103EEF594E}");
        private const int TreeScope_Element = 1;
        private const int TreeScope_Descendants = 4;
        private const int UIA_Selection_InvalidatedEventId = 20013;
        private const int UIA_SelectionItem_ElementAddedToSelectionEventId = 20010;
        private const int UIA_SelectionItem_ElementRemovedFromSelectionEventId = 20011;
        private const int UIA_SelectionItem_ElementSelectedEventId = 20012;

        private EventHandler handler = null;
        private IntPtr hwndListView;
        private static IUIAutomation pAutomation;
        public delegate T Query<T>(AutomationElementFactory factory);
        public delegate void SelChangeCallback();
        
        private class Worker<T> {
            private T ret;
            private Query<T> query;

            public Worker(Query<T> query) {
                this.query = query;
            }

            public void DoWork(object state) {
                try {
                    using(AutomationElementFactory factory = new AutomationElementFactory(pAutomation)) {
                        ret = query(factory);
                    }
                }
                finally {
                    ((AutoResetEvent)state).Set();
                }
            }

            public T GetReturn() {
                return ret;
            }
        };

        private class EventHandler : IUIAutomationEventHandler {
            private AutomationManager parent;
            private SelChangeCallback callback;
            private IntPtr hwnd;
            private int threadId;

            public EventHandler(AutomationManager parent, int threadId, IntPtr hwnd, SelChangeCallback callback) {
                this.hwnd = hwnd;
                this.parent = parent;
                this.callback = callback;
                this.threadId = threadId;
            }

            public int HandleAutomationEvent(IUIAutomationElement sender, int eventId) {
                if(PInvoke.GetCurrentThreadId() == threadId && parent.handler == this) {
                    callback();
                }
                Marshal.ReleaseComObject(sender);
                return 0;
            }

            public IntPtr GetHwnd() {
                return hwnd;
            }
        }
        
        public AutomationManager() {
            Guid rclsid = CLSID_CUIAutomation;
            Guid riid = IID_IUIAutomation;
            object obj = null;
            PInvoke.CoCreateInstance(ref rclsid, IntPtr.Zero, 1, ref riid, out obj);
            if(obj == null) return;
            pAutomation = obj as IUIAutomation;
        }

        ~AutomationManager() {
            Dispose();
        }

        private void RegisterSelChangedEventAT(object state) {
            IUIAutomationElement elem = null;
            try {
                pAutomation.ElementFromHandle(handler.GetHwnd(), out elem);
                if(elem != null) {
                    pAutomation.AddAutomationEventHandler(UIA_Selection_InvalidatedEventId,
                            elem, TreeScope_Element, IntPtr.Zero, handler);
                    pAutomation.AddAutomationEventHandler(UIA_SelectionItem_ElementAddedToSelectionEventId,
                            elem, TreeScope_Descendants, IntPtr.Zero, handler);
                    pAutomation.AddAutomationEventHandler(UIA_SelectionItem_ElementRemovedFromSelectionEventId,
                            elem, TreeScope_Descendants, IntPtr.Zero, handler);
                    pAutomation.AddAutomationEventHandler(UIA_SelectionItem_ElementSelectedEventId,
                            elem, TreeScope_Descendants, IntPtr.Zero, handler);
                }
            }
            catch(Exception ex) {
            }
            finally {
                if(elem != null) {
                    Marshal.ReleaseComObject(elem);
                }
                ((AutoResetEvent)state).Set();
            }
        }

        public void RegisterSelChangedEvent(IntPtr hwnd, SelChangeCallback callback) {
            WaitHandle handle = new AutoResetEvent(false);
            handler = new EventHandler(this, PInvoke.GetCurrentThreadId(), hwnd, callback);
            //ThreadPool.QueueUserWorkItem(new WaitCallback(RegisterSelChangedEventAT), handle);
            //handle.WaitOne();
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
