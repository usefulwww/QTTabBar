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
using System.Runtime.InteropServices;
using System.Threading;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    internal sealed class InstanceManager {
        private Dictionary<IntPtr, IntPtr> dicBtnBarHandle = new Dictionary<IntPtr, IntPtr>();
        private ReaderWriterLock rwLockBtnBar = new ReaderWriterLock();
        private ReaderWriterLock rwLockTabBar = new ReaderWriterLock();
        private StackDictionary<IntPtr, InstancePair> sdInstancePair = new StackDictionary<IntPtr, InstancePair>();

        public void AddButtonBarHandle(IntPtr hwndExplr, IntPtr hwndBtnBar) {
            try {
                this.rwLockBtnBar.AcquireWriterLock(-1);
                this.dicBtnBarHandle[hwndExplr] = hwndBtnBar;
            }
            finally {
                this.rwLockBtnBar.ReleaseWriterLock();
            }
        }

        public IEnumerable<IntPtr> ButtonBarHandles() {
            //<ButtonBarHandles>d__a _a = new <ButtonBarHandles>d__a(-2);
            //_a.<>4__this = this;
            //return _a;

            try {
                this.rwLockBtnBar.AcquireReaderLock(-1);
                foreach(IntPtr hwndBB in this.dicBtnBarHandle.Values) {
                    yield return hwndBB;
                }
            }
            finally {
                this.rwLockBtnBar.ReleaseReaderLock();
            }
        }

        public IEnumerable<IntPtr> ExplorerHandles() {
            //<ExplorerHandles>d__5 d__ = new <ExplorerHandles>d__5(-2);
            //d__.<>4__this = this;
            //return d__;

            try {
                this.rwLockTabBar.AcquireReaderLock(-1);
                foreach(IntPtr hwnd in this.sdInstancePair.Keys) {
                    yield return hwnd;
                }
            }
            finally {
                this.rwLockTabBar.ReleaseReaderLock();
            }
        }

        public QTTabBarClass GetTabBar(IntPtr hwndExplr) {
            QTTabBarClass class2;
            try {
                InstancePair pair;
                this.rwLockTabBar.AcquireReaderLock(-1);
                if((this.sdInstancePair.TryGetValue(hwndExplr, out pair) && (pair.tabBar != null)) && pair.tabBar.IsHandleCreated) {
                    return pair.tabBar;
                }
                class2 = null;
            }
            finally {
                this.rwLockTabBar.ReleaseReaderLock();
            }
            return class2;
        }

        public IntPtr GetTabBarHandle(IntPtr hwndExplr) {
            IntPtr zero;
            try {
                InstancePair pair;
                this.rwLockTabBar.AcquireReaderLock(-1);
                if((this.sdInstancePair.TryGetValue(hwndExplr, out pair) && (pair.tabBar != null)) && pair.tabBar.IsHandleCreated) {
                    return pair.hwnd;
                }
                zero = IntPtr.Zero;
            }
            finally {
                this.rwLockTabBar.ReleaseReaderLock();
            }
            return zero;
        }

        public bool NextInstanceExists() {
            try {
                this.rwLockTabBar.AcquireWriterLock(-1);
                while(this.sdInstancePair.Count > 0) {
                    IntPtr ptr;
                    InstancePair pair = this.sdInstancePair.Peek(out ptr);
                    if(((pair.tabBar != null) && pair.tabBar.IsHandleCreated) && (PInvoke.IsWindow(pair.hwnd) && PInvoke.IsWindow(ptr))) {
                        return true;
                    }
                    this.sdInstancePair.Pop();
                }
            }
            finally {
                this.rwLockTabBar.ReleaseWriterLock();
            }
            return false;
        }

        public void PushInstance(IntPtr hwndExplr, QTTabBarClass tabBar) {
            try {
                this.rwLockTabBar.AcquireWriterLock(-1);
                this.sdInstancePair.Push(hwndExplr, new InstancePair(tabBar, tabBar.Handle));
            }
            finally {
                this.rwLockTabBar.ReleaseWriterLock();
            }
        }

        public void RemoveButtonBarHandle(IntPtr hwndExplr) {
            try {
                this.rwLockBtnBar.AcquireWriterLock(-1);
                this.dicBtnBarHandle.Remove(hwndExplr);
            }
            finally {
                this.rwLockBtnBar.ReleaseWriterLock();
            }
        }

        public bool RemoveInstance(IntPtr hwndExplr, QTTabBarClass tabBar) {
            bool flag2;
            try {
                this.rwLockTabBar.AcquireWriterLock(-1);
                bool flag = tabBar == this.CurrentTabBar;
                this.sdInstancePair.Remove(hwndExplr);
                flag2 = flag;
            }
            finally {
                this.rwLockTabBar.ReleaseWriterLock();
            }
            return flag2;
        }

        public IEnumerable<IntPtr> TabBarHandles() {
            //<TabBarHandles>d__0 d__ = new <TabBarHandles>d__0(-2);
            //d__.<>4__this = this;
            //return d__;

            try {
                this.rwLockTabBar.AcquireReaderLock(-1);
                foreach(InstancePair ip in this.sdInstancePair.Values) {
                    yield return ip.hwnd;
                }
            }
            finally {
                this.rwLockTabBar.ReleaseReaderLock();
            }
        }

        public bool TryGetButtonBarHandle(IntPtr hwndExplr, out IntPtr hwndButtonBar) {
            try {
                IntPtr ptr;
                this.rwLockBtnBar.AcquireReaderLock(-1);
                if(this.dicBtnBarHandle.TryGetValue(hwndExplr, out ptr) && PInvoke.IsWindow(ptr)) {
                    hwndButtonBar = ptr;
                    return true;
                }
                hwndButtonBar = IntPtr.Zero;
            }
            finally {
                this.rwLockBtnBar.ReleaseReaderLock();
            }
            return false;
        }

        public IntPtr CurrentHandle {
            get {
                IntPtr zero;
                try {
                    this.rwLockTabBar.AcquireReaderLock(-1);
                    if(this.sdInstancePair.Count > 0) {
                        InstancePair pair = this.sdInstancePair.Peek();
                        if((pair.tabBar != null) && pair.tabBar.IsHandleCreated) {
                            return pair.hwnd;
                        }
                    }
                    zero = IntPtr.Zero;
                }
                finally {
                    this.rwLockTabBar.ReleaseReaderLock();
                }
                return zero;
            }
        }

        public QTTabBarClass CurrentTabBar {
            get {
                QTTabBarClass class2;
                try {
                    this.rwLockTabBar.AcquireReaderLock(-1);
                    if(this.sdInstancePair.Count > 0) {
                        return this.sdInstancePair.Peek().tabBar;
                    }
                    class2 = null;
                }
                finally {
                    this.rwLockTabBar.ReleaseReaderLock();
                }
                return class2;
            }
        }

#if false
    [CompilerGenerated]
    private sealed class <ButtonBarHandles>d__a : IEnumerator<IntPtr>, IEnumerable<IntPtr>, IEnumerable, IEnumerator, IDisposable
    {
      private int <>1__state;
      private IntPtr <>2__current;
      public InstanceManager <>4__this;
      public Dictionary<IntPtr, IntPtr>.ValueCollection.Enumerator <>7__wrapc;
      public IntPtr <hwndBB>5__b;
      
      [DebuggerHidden]
      public <ButtonBarHandles>d__a(int <>1__state)
      {
        this.<>1__state = <>1__state;
      }
      
      private bool MoveNext()
      {
        try
        {
          switch (this.<>1__state)
          {
            case 0:
              this.<>1__state = -1;
              this.<>1__state = 1;
              this.<>4__this.rwLockBtnBar.AcquireReaderLock(-1);
              this.<>7__wrapc = this.<>4__this.dicBtnBarHandle.Values.GetEnumerator();
              this.<>1__state = 2;
              while (this.<>7__wrapc.MoveNext())
              {
                this.<hwndBB>5__b = this.<>7__wrapc.Current;
                this.<>2__current = this.<hwndBB>5__b;
                this.<>1__state = 3;
                return true;
              Label_007F:
                this.<>1__state = 2;
              }
              this.<>1__state = 1;
              this.<>7__wrapc.Dispose();
              this.<>1__state = -1;
              this.<>4__this.rwLockBtnBar.ReleaseReaderLock();
              break;
            
            case 3:
              goto Label_007F;
          }
          return false;
        }
        fault
        {
          ((IDisposable) this).Dispose();
        }
      }
      
      [DebuggerHidden]
      IEnumerator<IntPtr> IEnumerable<IntPtr>.GetEnumerator()
      {
        if (Interlocked.CompareExchange(ref this.<>1__state, 0, -2) == -2)
        {
          return this;
        }
        InstanceManager.<ButtonBarHandles>d__a _a = new InstanceManager.<ButtonBarHandles>d__a(0);
        _a.<>4__this = this.<>4__this;
        return _a;
      }
      
      [DebuggerHidden]
      IEnumerator IEnumerable.GetEnumerator()
      {
        return this.System.Collections.Generic.IEnumerable<System.IntPtr>.GetEnumerator();
      }
      
      [DebuggerHidden]
      void IEnumerator.Reset()
      {
        throw new NotSupportedException();
      }
      
      void IDisposable.Dispose()
      {
        switch (this.<>1__state)
        {
          case 1:
          case 2:
          case 3:
            try
            {
              switch (this.<>1__state)
              {
                case 2:
                case 3:
                  try
                  {
                  }
                  finally
                  {
                    this.<>1__state = 1;
                    this.<>7__wrapc.Dispose();
                  }
                  break;
              }
            }
            finally
            {
              this.<>1__state = -1;
              this.<>4__this.rwLockBtnBar.ReleaseReaderLock();
            }
            break;
          
          default:
            return;
        }
      }
      
      IntPtr IEnumerator<IntPtr>.Current
      {
        [DebuggerHidden]
        get
        {
          return this.<>2__current;
        }
      }
      
      object IEnumerator.Current
      {
        [DebuggerHidden]
        get
        {
          return this.<>2__current;
        }
      }
    }
    
    [CompilerGenerated]
    private sealed class <ExplorerHandles>d__5 : IEnumerator<IntPtr>, IEnumerable<IntPtr>, IEnumerable, IEnumerator, IDisposable
    {
      private int <>1__state;
      private IntPtr <>2__current;
      public InstanceManager <>4__this;
      public IEnumerator<IntPtr> <>7__wrap7;
      public IntPtr <hwnd>5__6;
      
      [DebuggerHidden]
      public <ExplorerHandles>d__5(int <>1__state)
      {
        this.<>1__state = <>1__state;
      }
      
      private bool MoveNext()
      {
        try
        {
          switch (this.<>1__state)
          {
            case 0:
              this.<>1__state = -1;
              this.<>1__state = 1;
              this.<>4__this.rwLockTabBar.AcquireReaderLock(-1);
              this.<>7__wrap7 = this.<>4__this.sdInstancePair.Keys.GetEnumerator();
              this.<>1__state = 2;
              while (this.<>7__wrap7.MoveNext())
              {
                this.<hwnd>5__6 = this.<>7__wrap7.Current;
                this.<>2__current = this.<hwnd>5__6;
                this.<>1__state = 3;
                return true;
              Label_007F:
                this.<>1__state = 2;
              }
              this.<>1__state = 1;
              if (this.<>7__wrap7 != null)
              {
                this.<>7__wrap7.Dispose();
              }
              this.<>1__state = -1;
              this.<>4__this.rwLockTabBar.ReleaseReaderLock();
              break;
            
            case 3:
              goto Label_007F;
          }
          return false;
        }
        fault
        {
          ((IDisposable) this).Dispose();
        }
      }
      
      [DebuggerHidden]
      IEnumerator<IntPtr> IEnumerable<IntPtr>.GetEnumerator()
      {
        if (Interlocked.CompareExchange(ref this.<>1__state, 0, -2) == -2)
        {
          return this;
        }
        InstanceManager.<ExplorerHandles>d__5 d__ = new InstanceManager.<ExplorerHandles>d__5(0);
        d__.<>4__this = this.<>4__this;
        return d__;
      }
      
      [DebuggerHidden]
      IEnumerator IEnumerable.GetEnumerator()
      {
        return this.System.Collections.Generic.IEnumerable<System.IntPtr>.GetEnumerator();
      }
      
      [DebuggerHidden]
      void IEnumerator.Reset()
      {
        throw new NotSupportedException();
      }
      
      void IDisposable.Dispose()
      {
        switch (this.<>1__state)
        {
          case 1:
          case 2:
          case 3:
            try
            {
              switch (this.<>1__state)
              {
                case 2:
                case 3:
                  try
                  {
                  }
                  finally
                  {
                    this.<>1__state = 1;
                    if (this.<>7__wrap7 != null)
                    {
                      this.<>7__wrap7.Dispose();
                    }
                  }
                  break;
              }
            }
            finally
            {
              this.<>1__state = -1;
              this.<>4__this.rwLockTabBar.ReleaseReaderLock();
            }
            break;
          
          default:
            return;
        }
      }
      
      IntPtr IEnumerator<IntPtr>.Current
      {
        [DebuggerHidden]
        get
        {
          return this.<>2__current;
        }
      }
      
      object IEnumerator.Current
      {
        [DebuggerHidden]
        get
        {
          return this.<>2__current;
        }
      }
    }
    
    [CompilerGenerated]
    private sealed class <TabBarHandles>d__0 : IEnumerator<IntPtr>, IEnumerable<IntPtr>, IEnumerable, IEnumerator, IDisposable
    {
      private int <>1__state;
      private IntPtr <>2__current;
      public InstanceManager <>4__this;
      public IEnumerator<InstanceManager.InstancePair> <>7__wrap2;
      public InstanceManager.InstancePair <ip>5__1;
      
      [DebuggerHidden]
      public <TabBarHandles>d__0(int <>1__state)
      {
        this.<>1__state = <>1__state;
      }
      
      private bool MoveNext()
      {
        try
        {
          switch (this.<>1__state)
          {
            case 0:
              this.<>1__state = -1;
              this.<>1__state = 1;
              this.<>4__this.rwLockTabBar.AcquireReaderLock(-1);
              this.<>7__wrap2 = this.<>4__this.sdInstancePair.Values.GetEnumerator();
              this.<>1__state = 2;
              while (this.<>7__wrap2.MoveNext())
              {
                this.<ip>5__1 = this.<>7__wrap2.Current;
                this.<>2__current = this.<ip>5__1.hwnd;
                this.<>1__state = 3;
                return true;
              Label_0084:
                this.<>1__state = 2;
              }
              this.<>1__state = 1;
              if (this.<>7__wrap2 != null)
              {
                this.<>7__wrap2.Dispose();
              }
              this.<>1__state = -1;
              this.<>4__this.rwLockTabBar.ReleaseReaderLock();
              break;
            
            case 3:
              goto Label_0084;
          }
          return false;
        }
        fault
        {
          ((IDisposable) this).Dispose();
        }
      }
      
      [DebuggerHidden]
      IEnumerator<IntPtr> IEnumerable<IntPtr>.GetEnumerator()
      {
        if (Interlocked.CompareExchange(ref this.<>1__state, 0, -2) == -2)
        {
          return this;
        }
        InstanceManager.<TabBarHandles>d__0 d__ = new InstanceManager.<TabBarHandles>d__0(0);
        d__.<>4__this = this.<>4__this;
        return d__;
      }
      
      [DebuggerHidden]
      IEnumerator IEnumerable.GetEnumerator()
      {
        return this.System.Collections.Generic.IEnumerable<System.IntPtr>.GetEnumerator();
      }
      
      [DebuggerHidden]
      void IEnumerator.Reset()
      {
        throw new NotSupportedException();
      }
      
      void IDisposable.Dispose()
      {
        switch (this.<>1__state)
        {
          case 1:
          case 2:
          case 3:
            try
            {
              switch (this.<>1__state)
              {
                case 2:
                case 3:
                  try
                  {
                  }
                  finally
                  {
                    this.<>1__state = 1;
                    if (this.<>7__wrap2 != null)
                    {
                      this.<>7__wrap2.Dispose();
                    }
                  }
                  break;
              }
            }
            finally
            {
              this.<>1__state = -1;
              this.<>4__this.rwLockTabBar.ReleaseReaderLock();
            }
            break;
          
          default:
            return;
        }
      }
      
      IntPtr IEnumerator<IntPtr>.Current
      {
        [DebuggerHidden]
        get
        {
          return this.<>2__current;
        }
      }
      
      object IEnumerator.Current
      {
        [DebuggerHidden]
        get
        {
          return this.<>2__current;
        }
      }
    }
#endif

        [StructLayout(LayoutKind.Sequential)]
        private struct InstancePair {
            public QTTabBarClass tabBar;
            public IntPtr hwnd;
            public InstancePair(QTTabBarClass tabBar, IntPtr hwnd) {
                this.tabBar = tabBar;
                this.hwnd = hwnd;
            }
        }
    }
}
