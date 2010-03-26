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
    using Microsoft.Win32;
    using QTPlugin;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class PluginManager {
        public Dictionary<string, string> dicFullNamesMenuRegistered_Sys = new Dictionary<string, string>();
        public Dictionary<string, string> dicFullNamesMenuRegistered_Tab = new Dictionary<string, string>();
        private Dictionary<string, int> dicMultiplePluginCounter = new Dictionary<string, int>();
        private Dictionary<string, List<int>> dicMultiplePluginOrders = new Dictionary<string, List<int>>();
        private static Dictionary<string, PluginAssembly> dicPluginAssemblies = new Dictionary<string, PluginAssembly>();
        private Dictionary<string, Plugin> dicPluginInstances = new Dictionary<string, Plugin>();
        private static Dictionary<string, Plugin> dicStaticPluginInstances = new Dictionary<string, Plugin>();
        private int iClosingCount;
        private int iRefCount;
        private static List<string> lstPluginButtonsOrder = new List<string>();
        private static QTPlugin.IEncodingDetector plgEncodingDetector;
        private QTPlugin.IFilterCore plgFilterCore;
        private QTPlugin.IFilter plgIFilter;
        private QTTabBarClass.PluginServer pluginServer;

        public PluginManager(QTTabBarClass tabBar) {
            this.pluginServer = new QTTabBarClass.PluginServer(tabBar, this);
            this.LoadStartupPlugins();
            this.iRefCount++;
        }

        public static void AddAssembly(PluginAssembly pa) {
            PluginAssembly assembly;
            if(dicPluginAssemblies.TryGetValue(pa.Path, out assembly) && (assembly != pa)) {
                assembly.Dispose();
            }
            dicPluginAssemblies[pa.Path] = pa;
        }

        public void AddRef() {
            this.iRefCount++;
        }

        public void ClearBackgroundMultipleOrders() {
            this.dicMultiplePluginOrders.Clear();
        }

        public void ClearBackgroundMultiples() {
            this.dicMultiplePluginCounter.Clear();
        }

        public void ClearFilterEngines() {
            this.plgIFilter = null;
            this.plgFilterCore = null;
        }

        public static void ClearIEncodingDetector() {
            plgEncodingDetector = null;
        }

        public void Close(bool fInteractive) {
            this.iRefCount--;
            if(this.iClosingCount == 0) {
                this.pluginServer.ClearEvents();
            }
            foreach(Plugin plugin in this.dicPluginInstances.Values) {
                if((plugin.PluginInformation != null) && (fInteractive ^ (plugin.PluginInformation.PluginType != PluginType.Interactive))) {
                    plugin.Close(EndCode.WindowClosed);
                }
            }
            if(!fInteractive) {
                this.plgIFilter = null;
                this.plgFilterCore = null;
            }
            if(this.iRefCount == 0) {
                this.dicPluginInstances.Clear();
                this.pluginServer.Dispose();
                this.pluginServer = null;
            }
            this.iClosingCount++;
        }

        public static void HandlePluginException(Exception ex, IntPtr hwnd, string pluginID, string strCase) {
            MessageForm.Show(hwnd, "Error : " + strCase + "\r\nPlugin : \"" + pluginID + "\"\r\nErrorType : " + ex.ToString(), "Plugin Error", MessageBoxIcon.Hand, 0x7530);
        }

        public int IncrementBackgroundMultiple(PluginInformation pi) {
            int num;
            if(this.dicMultiplePluginCounter.TryGetValue(pi.PluginID, out num)) {
                this.dicMultiplePluginCounter[pi.PluginID] = num + 1;
                return (num + 1);
            }
            this.dicMultiplePluginCounter[pi.PluginID] = 0;
            return 0;
        }

        public static void Initialize() {
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar\Plugins")) {
                if(key != null) {
                    string[] strArray = QTUtility2.ReadRegBinary<string>("Buttons_Order", key);
                    string[] array = QTUtility2.ReadRegBinary<string>("Enabled", key);
                    PluginKey[] keyArray = QTUtility2.ReadRegBinary<PluginKey>("ShortcutKeys", key);
                    QTUtility.Path_PluginLangFile = (string)key.GetValue("LanguageFile", string.Empty);
                    using(RegistryKey key2 = key.CreateSubKey("Paths")) {
                        bool flag = (array != null) && (array.Length > 0);
                        foreach(string str in key2.GetValueNames()) {
                            string path = (string)key2.GetValue(str, string.Empty);
                            if(path.Length > 0) {
                                PluginAssembly pa = new PluginAssembly(path);
                                if(pa.PluginInfosExist) {
                                    if(flag) {
                                        foreach(PluginInformation information in pa.PluginInformations) {
                                            if(Array.IndexOf<string>(array, information.PluginID) != -1) {
                                                information.Enabled = true;
                                                pa.Enabled = true;
                                                if(information.PluginType == PluginType.Static) {
                                                    LoadStatics(information, pa, false);
                                                }
                                            }
                                        }
                                    }
                                    dicPluginAssemblies[path] = pa;
                                }
                            }
                        }
                    }
                    if((strArray != null) && (strArray.Length > 0)) {
                        foreach(string str3 in strArray) {
                            foreach(PluginAssembly assembly2 in dicPluginAssemblies.Values) {
                                PluginInformation information2;
                                if(assembly2.TryGetPluginInformation(str3, out information2)) {
                                    if(information2.Enabled) {
                                        lstPluginButtonsOrder.Add(str3);
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    if(keyArray != null) {
                        List<int> list = new List<int>();
                        foreach(PluginKey key3 in keyArray) {
                            if(key3.Keys != null) {
                                QTUtility.dicPluginShortcutKeys[key3.PluginID] = key3.Keys;
                                for(int i = 0; i < key3.Keys.Length; i++) {
                                    list.Add(key3.Keys[i]);
                                }
                            }
                        }
                        QTUtility.PluginShortcutKeysCache = list.ToArray();
                    }
                }
            }
        }

        public string InstanceToFullName(IPluginClient pluginClient, bool fTypeFullName) {
            foreach(Plugin plugin in this.dicPluginInstances.Values) {
                if(plugin.Instance == pluginClient) {
                    return (fTypeFullName ? plugin.PluginInformation.TypeFullName : plugin.PluginInformation.PluginID);
                }
            }
            return string.Empty;
        }

        public Plugin Load(PluginInformation pi, PluginAssembly pa) {
            try {
                if((pa == null) && !dicPluginAssemblies.TryGetValue(pi.Path, out pa)) {
                    return null;
                }
                Plugin plugin = pa.Load(pi.PluginID);
                if(plugin != null) {
                    string[] strArray;
                    int[] numArray;
                    this.dicPluginInstances[pi.PluginID] = plugin;
                    if((!this.pluginServer.OpenPlugin(plugin.Instance, out strArray) || (strArray == null)) || (strArray.Length <= 0)) {
                        return plugin;
                    }
                    pi.ShortcutKeyActions = strArray;
                    if(QTUtility.dicPluginShortcutKeys.TryGetValue(pi.PluginID, out numArray)) {
                        if(numArray == null) {
                            QTUtility.dicPluginShortcutKeys[pi.PluginID] = new int[strArray.Length];
                            return plugin;
                        }
                        if(numArray.Length != strArray.Length) {
                            int[] numArray2 = new int[strArray.Length];
                            int num = Math.Min(numArray.Length, strArray.Length);
                            for(int i = 0; i < num; i++) {
                                numArray2[i] = numArray[i];
                            }
                            QTUtility.dicPluginShortcutKeys[pi.PluginID] = numArray2;
                        }
                        return plugin;
                    }
                    QTUtility.dicPluginShortcutKeys[pi.PluginID] = new int[strArray.Length];
                }
                return plugin;
            }
            catch(Exception exception) {
                HandlePluginException(exception, IntPtr.Zero, pi.Name, "Loading plugin.");
                QTUtility2.MakeErrorLog(exception, null);
            }
            return null;
        }

        private void LoadStartupPlugins() {
            foreach(PluginInformation information in PluginInformations) {
                if(!information.Enabled) {
                    continue;
                }
                if(information.PluginType == PluginType.Background) {
                    Plugin plugin = this.Load(information, null);
                    if(plugin != null) {
                        if(this.plgIFilter == null) {
                            this.plgIFilter = plugin.Instance as QTPlugin.IFilter;
                        }
                        if(this.plgFilterCore == null) {
                            this.plgFilterCore = plugin.Instance as QTPlugin.IFilterCore;
                        }
                    }
                    else {
                        information.Enabled = false;
                    }
                    continue;
                }
                if((information.PluginType == PluginType.BackgroundMultiple) && (this.Load(information, null) == null)) {
                    information.Enabled = false;
                }
            }
        }

        private static bool LoadStatics(PluginInformation pi, PluginAssembly pa, bool fForce) {
            Plugin plugin = pa.Load(pi.PluginID);
            if((plugin != null) && (plugin.Instance != null)) {
                dicStaticPluginInstances[pi.PluginID] = plugin;
                if((plgEncodingDetector == null) || fForce) {
                    QTPlugin.IEncodingDetector instance = plugin.Instance as QTPlugin.IEncodingDetector;
                    if(instance != null) {
                        try {
                            instance.Open(null, null);
                            plgEncodingDetector = instance;
                            return true;
                        }
                        catch(Exception exception) {
                            HandlePluginException(exception, IntPtr.Zero, pi.Name, "Loading static plugin.");
                        }
                    }
                }
            }
            return false;
        }

        public void OnExplorerStateChanged(ExplorerWindowActions windowAction) {
            this.pluginServer.OnExplorerStateChanged(windowAction);
        }

        public void OnMenuRendererChanged() {
            this.pluginServer.OnMenuRendererChanged();
        }

        public void OnMouseEnter() {
            this.pluginServer.OnMouseEnter();
        }

        public void OnMouseLeave() {
            this.pluginServer.OnMouseLeave();
        }

        public void OnNavigationComplete(int index, byte[] idl, string path) {
            this.pluginServer.OnNavigationComplete(index, idl, path);
        }

        public void OnPointedTabChanged(int index, byte[] idl, string path) {
            this.pluginServer.OnPointedTabChanged(index, idl, path);
        }

        public void OnSelectionChanged(int index, byte[] idl, string path) {
            this.pluginServer.OnSelectionChanged(index, idl, path);
        }

        public void OnSettingsChanged(int iType) {
            this.pluginServer.OnSettingsChanged(iType);
        }

        public void OnTabAdded(int index, byte[] idl, string path) {
            this.pluginServer.OnTabAdded(index, idl, path);
        }

        public void OnTabChanged(int index, byte[] idl, string path) {
            this.pluginServer.OnTabChanged(index, idl, path);
        }

        public void OnTabRemoved(int index, byte[] idl, string path) {
            this.pluginServer.OnTabRemoved(index, idl, path);
        }

        public bool PluginInstantialized(string pluginID) {
            return this.dicPluginInstances.ContainsKey(pluginID);
        }

        public void PushBackgroundMultiple(string pluginID, int i) {
            List<int> list;
            if(this.dicMultiplePluginOrders.TryGetValue(pluginID, out list)) {
                list.Add(i);
            }
            else {
                list = new List<int>();
                list.Add(i);
                this.dicMultiplePluginOrders[pluginID] = list;
            }
        }

        public void RefreshPluginAssembly(PluginAssembly pa, bool fStatic) {
            foreach(PluginInformation information in pa.PluginInformations) {
                if(!information.Enabled) {
                    this.UnloadPluginInstance(information.PluginID, EndCode.Unloaded, fStatic);
                    continue;
                }
                if(information.PluginType == PluginType.Background) {
                    if(!this.PluginInstantialized(information.PluginID)) {
                        Plugin plugin = this.Load(information, pa);
                        if(plugin != null) {
                            if(this.plgIFilter == null) {
                                this.plgIFilter = plugin.Instance as QTPlugin.IFilter;
                            }
                            if(this.plgFilterCore == null) {
                                this.plgFilterCore = plugin.Instance as QTPlugin.IFilterCore;
                            }
                        }
                        else {
                            information.Enabled = false;
                        }
                    }
                    continue;
                }
                if(information.PluginType == PluginType.BackgroundMultiple) {
                    if(!this.PluginInstantialized(information.PluginID) && (this.Load(information, pa) == null)) {
                        information.Enabled = false;
                    }
                }
                else if((fStatic && (information.PluginType == PluginType.Static)) && !dicStaticPluginInstances.ContainsKey(information.PluginID)) {
                    LoadStatics(information, pa, false);
                }
            }
        }

        public void RegisterMenu(IPluginClient pluginClient, MenuType menuType, string menuText, bool fRegister) {
            foreach(Plugin plugin in this.dicPluginInstances.Values) {
                if(plugin.Instance != pluginClient) {
                    continue;
                }
                if(fRegister) {
                    if((menuType & MenuType.Bar) == MenuType.Bar) {
                        this.dicFullNamesMenuRegistered_Sys[plugin.PluginInformation.PluginID] = menuText;
                    }
                    if((menuType & MenuType.Tab) == MenuType.Tab) {
                        this.dicFullNamesMenuRegistered_Tab[plugin.PluginInformation.PluginID] = menuText;
                    }
                }
                else {
                    if((menuType & MenuType.Bar) == MenuType.Bar) {
                        this.dicFullNamesMenuRegistered_Sys.Remove(plugin.PluginInformation.PluginID);
                    }
                    if((menuType & MenuType.Tab) == MenuType.Tab) {
                        this.dicFullNamesMenuRegistered_Tab.Remove(plugin.PluginInformation.PluginID);
                    }
                }
                break;
            }
        }

        public static bool RemoveFromButtonBarOrder(string pluginID) {
            int index = lstPluginButtonsOrder.IndexOf(pluginID);
            if(index != -1) {
                lstPluginButtonsOrder.Remove(pluginID);
                int num2 = 0;
                int length = -1;
                for(int i = 0; i < QTButtonBar.ButtonIndexes.Length; i++) {
                    if((QTButtonBar.ButtonIndexes[i] == 0x10000) && (num2++ == index)) {
                        length = i;
                        break;
                    }
                }
                if(length != -1) {
                    if(QTButtonBar.ButtonIndexes.Length > 1) {
                        int[] destinationArray = new int[QTButtonBar.ButtonIndexes.Length - 1];
                        if(length != 0) {
                            Array.Copy(QTButtonBar.ButtonIndexes, destinationArray, length);
                        }
                        if(length != (QTButtonBar.ButtonIndexes.Length - 1)) {
                            Array.Copy(QTButtonBar.ButtonIndexes, length + 1, destinationArray, length, (QTButtonBar.ButtonIndexes.Length - length) - 1);
                        }
                        QTButtonBar.ButtonIndexes = destinationArray;
                    }
                    else {
                        QTButtonBar.ButtonIndexes = new int[0];
                    }
                    return true;
                }
            }
            return false;
        }

        public static void SaveButtonOrder() {
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar\Plugins")) {
                if(key != null) {
                    QTUtility2.WriteRegBinary<string>(lstPluginButtonsOrder.ToArray(), "Buttons_Order", key);
                }
            }
        }

        public static void SavePluginShortcutKeys() {
            List<PluginKey> list = new List<PluginKey>();
            foreach(string str in QTUtility.dicPluginShortcutKeys.Keys) {
                list.Add(new PluginKey(str, QTUtility.dicPluginShortcutKeys[str]));
            }
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar\Plugins")) {
                QTUtility2.WriteRegBinary<PluginKey>(list.ToArray(), "ShortcutKeys", key);
            }
        }

        public bool TryGetMultipleOrder(string pluginID, out List<int> order) {
            return this.dicMultiplePluginOrders.TryGetValue(pluginID, out order);
        }

        public bool TryGetPlugin(string pluginID, out Plugin plugin) {
            return this.dicPluginInstances.TryGetValue(pluginID, out plugin);
        }

        public void UninstallPluginAssembly(PluginAssembly pa, bool fStatic) {
            foreach(PluginInformation information in pa.PluginInformations) {
                this.UnloadPluginInstance(information.PluginID, EndCode.Removed, fStatic);
                if(fStatic) {
                    Plugin plugin;
                    QTUtility.dicPluginShortcutKeys.Remove(information.PluginID);
                    if((information.PluginType == PluginType.Static) && dicStaticPluginInstances.TryGetValue(information.PluginID, out plugin)) {
                        plugin.Close(EndCode.Removed);
                        dicStaticPluginInstances.Remove(information.PluginID);
                    }
                }
            }
            if(fStatic) {
                dicPluginAssemblies.Remove(pa.Path);
                List<int> list = new List<int>();
                foreach(int[] numArray in QTUtility.dicPluginShortcutKeys.Values) {
                    if(numArray != null) {
                        for(int i = 0; i < numArray.Length; i++) {
                            list.Add(numArray[i]);
                        }
                    }
                }
                QTUtility.PluginShortcutKeysCache = list.ToArray();
                SavePluginShortcutKeys();
                pa.Uninstall();
                pa.Dispose();
            }
        }

        public void UnloadPluginInstance(string pluginID, EndCode code, bool fStatic) {
            Plugin plugin;
            if(fStatic) {
                RemoveFromButtonBarOrder(pluginID);
            }
            this.dicFullNamesMenuRegistered_Sys.Remove(pluginID);
            this.dicFullNamesMenuRegistered_Tab.Remove(pluginID);
            if(this.dicPluginInstances.TryGetValue(pluginID, out plugin)) {
                this.pluginServer.RemoveEvents(plugin.Instance);
                this.dicPluginInstances.Remove(pluginID);
                plugin.Close(code);
            }
        }

        public static List<string> ActivatedButtonsOrder {
            get {
                return lstPluginButtonsOrder;
            }
            set {
                lstPluginButtonsOrder = value;
            }
        }

        public static QTPlugin.IEncodingDetector IEncodingDetector {
            get {
                return plgEncodingDetector;
            }
        }

        public QTPlugin.IFilter IFilter {
            get {
                return this.plgIFilter;
            }
        }

        public QTPlugin.IFilterCore IFilterCore {
            get {
                return this.plgFilterCore;
            }
        }

        public static List<PluginAssembly> PluginAssemblies {
            get {
                return new List<PluginAssembly>(dicPluginAssemblies.Values);
            }
        }

        public static IEnumerable<PluginInformation> PluginInformations {
            get {
                //return new <get_PluginInformations>d__0(-2);

                foreach(PluginAssembly pa in PluginManager.dicPluginAssemblies.Values) {
                    foreach(PluginInformation info in pa.PluginInformations) {
                        yield return info;
                    }
                }
            }
        }

        public List<Plugin> Plugins {
            get {
                return new List<Plugin>(this.dicPluginInstances.Values);
            }
        }

        public bool SelectionChangeAttached {
            get {
                return this.pluginServer.SelectionChangedAttached;
            }
        }

#if false
    [CompilerGenerated]
    private sealed class <get_PluginInformations>d__0 : IEnumerable<PluginInformation>, IEnumerator<PluginInformation>, IEnumerable, IEnumerator, IDisposable
    {
      private int <>1__state;
      private PluginInformation <>2__current;
      public Dictionary<string, PluginAssembly>.ValueCollection.Enumerator <>7__wrap3;
      public List<PluginInformation>.Enumerator <>7__wrap4;
      public PluginInformation <info>5__2;
      public PluginAssembly <pa>5__1;
      
      [DebuggerHidden]
      public <get_PluginInformations>d__0(int <>1__state)
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
              this.<>7__wrap3 = PluginManager.dicPluginAssemblies.Values.GetEnumerator();
              this.<>1__state = 1;
              while (this.<>7__wrap3.MoveNext())
              {
                this.<pa>5__1 = this.<>7__wrap3.Current;
                this.<>7__wrap4 = this.<pa>5__1.PluginInformations.GetEnumerator();
                this.<>1__state = 2;
                while (this.<>7__wrap4.MoveNext())
                {
                  this.<info>5__2 = this.<>7__wrap4.Current;
                  this.<>2__current = this.<info>5__2;
                  this.<>1__state = 3;
                  return true;
                Label_0097:
                  this.<>1__state = 2;
                }
                this.<>1__state = 1;
                this.<>7__wrap4.Dispose();
              }
              this.<>1__state = -1;
              this.<>7__wrap3.Dispose();
              break;
            
            case 3:
              goto Label_0097;
          }
          return false;
        }
        fault
        {
          ((IDisposable) this).Dispose();
        }
      }
      
      [DebuggerHidden]
      IEnumerator<PluginInformation> IEnumerable<PluginInformation>.GetEnumerator()
      {
        if (Interlocked.CompareExchange(ref this.<>1__state, 0, -2) == -2)
        {
          return this;
        }
        return new PluginManager.<get_PluginInformations>d__0(0);
      }
      
      [DebuggerHidden]
      IEnumerator IEnumerable.GetEnumerator()
      {
        return this.System.Collections.Generic.IEnumerable<QTTabBarLib.PluginInformation>.GetEnumerator();
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
                    this.<>7__wrap4.Dispose();
                  }
                  break;
              }
            }
            finally
            {
              this.<>1__state = -1;
              this.<>7__wrap3.Dispose();
            }
            break;
          
          default:
            return;
        }
      }
      
      PluginInformation IEnumerator<PluginInformation>.Current
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

    }
}
