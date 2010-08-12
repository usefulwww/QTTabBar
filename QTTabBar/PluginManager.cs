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
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;
using QTPlugin;

namespace QTTabBarLib {
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
        private static IEncodingDetector plgEncodingDetector;
        private IFilterCore plgFilterCore;
        private IFilter plgIFilter;
        private QTTabBarClass.PluginServer pluginServer;

        public PluginManager(QTTabBarClass tabBar) {
            pluginServer = new QTTabBarClass.PluginServer(tabBar, this);
            LoadStartupPlugins();
            iRefCount++;
        }

        public static void AddAssembly(PluginAssembly pa) {
            PluginAssembly assembly;
            if(dicPluginAssemblies.TryGetValue(pa.Path, out assembly) && (assembly != pa)) {
                assembly.Dispose();
            }
            dicPluginAssemblies[pa.Path] = pa;
        }

        public void AddRef() {
            iRefCount++;
        }

        public void ClearBackgroundMultipleOrders() {
            dicMultiplePluginOrders.Clear();
        }

        public void ClearBackgroundMultiples() {
            dicMultiplePluginCounter.Clear();
        }

        public void ClearFilterEngines() {
            plgIFilter = null;
            plgFilterCore = null;
        }

        public static void ClearIEncodingDetector() {
            plgEncodingDetector = null;
        }

        public void Close(bool fInteractive) {
            iRefCount--;
            if(iClosingCount == 0) {
                pluginServer.ClearEvents();
            }
            foreach(Plugin plugin in dicPluginInstances.Values) {
                if((plugin.PluginInformation != null) && (fInteractive ^ (plugin.PluginInformation.PluginType != PluginType.Interactive))) {
                    plugin.Close(EndCode.WindowClosed);
                }
            }
            if(!fInteractive) {
                plgIFilter = null;
                plgFilterCore = null;
            }
            if(iRefCount == 0) {
                dicPluginInstances.Clear();
                pluginServer.Dispose();
                pluginServer = null;
            }
            iClosingCount++;
        }

        public static void HandlePluginException(Exception ex, IntPtr hwnd, string pluginID, string strCase) {
            MessageForm.Show(hwnd, "Error : " + strCase + "\r\nPlugin : \"" + pluginID + "\"\r\nErrorType : " + ex, "Plugin Error", MessageBoxIcon.Hand, 0x7530);
        }

        public int IncrementBackgroundMultiple(PluginInformation pi) {
            int num;
            if(dicMultiplePluginCounter.TryGetValue(pi.PluginID, out num)) {
                dicMultiplePluginCounter[pi.PluginID] = num + 1;
                return (num + 1);
            }
            dicMultiplePluginCounter[pi.PluginID] = 0;
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
                                        foreach(PluginInformation information in pa.PluginInformations
                                                .Where(information => array.Contains(information.PluginID))) {
                                            information.Enabled = true;
                                            pa.Enabled = true;
                                            if(information.PluginType == PluginType.Static) {
                                                LoadStatics(information, pa, false);
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
                        foreach(PluginKey key3 in keyArray.Where(key3 => key3.Keys != null)) {
                            QTUtility.dicPluginShortcutKeys[key3.PluginID] = key3.Keys;
                            list.AddRange(key3.Keys);
                        }
                        QTUtility.PluginShortcutKeysCache = list.ToArray();
                    }
                }
            }
        }

        public string InstanceToFullName(IPluginClient pluginClient, bool fTypeFullName) {
            Plugin plugin = dicPluginInstances.Values.FirstOrDefault(plugin1 => plugin1.Instance == pluginClient);
            return plugin == null
                    ? null
                    : fTypeFullName
                            ? plugin.PluginInformation.TypeFullName
                            : plugin.PluginInformation.PluginID;
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
                    dicPluginInstances[pi.PluginID] = plugin;
                    if((!pluginServer.OpenPlugin(plugin.Instance, out strArray) || (strArray == null)) || (strArray.Length <= 0)) {
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
            foreach(PluginInformation information in PluginInformations.Where(information => information.Enabled)) {
                if(information.PluginType == PluginType.Background) {
                    Plugin plugin = Load(information, null);
                    if(plugin != null) {
                        if(plgIFilter == null) {
                            plgIFilter = plugin.Instance as IFilter;
                        }
                        if(plgFilterCore == null) {
                            plgFilterCore = plugin.Instance as IFilterCore;
                        }
                    }
                    else {
                        information.Enabled = false;
                    }
                    continue;
                }
                if((information.PluginType == PluginType.BackgroundMultiple) && (Load(information, null) == null)) {
                    information.Enabled = false;
                }
            }
        }

        private static bool LoadStatics(PluginInformation pi, PluginAssembly pa, bool fForce) {
            Plugin plugin = pa.Load(pi.PluginID);
            if((plugin != null) && (plugin.Instance != null)) {
                dicStaticPluginInstances[pi.PluginID] = plugin;
                if((plgEncodingDetector == null) || fForce) {
                    IEncodingDetector instance = plugin.Instance as IEncodingDetector;
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
            pluginServer.OnExplorerStateChanged(windowAction);
        }

        public void OnMenuRendererChanged() {
            pluginServer.OnMenuRendererChanged();
        }

        public void OnMouseEnter() {
            pluginServer.OnMouseEnter();
        }

        public void OnMouseLeave() {
            pluginServer.OnMouseLeave();
        }

        public void OnNavigationComplete(int index, byte[] idl, string path) {
            pluginServer.OnNavigationComplete(index, idl, path);
        }

        public void OnPointedTabChanged(int index, byte[] idl, string path) {
            pluginServer.OnPointedTabChanged(index, idl, path);
        }

        public void OnSelectionChanged(int index, byte[] idl, string path) {
            pluginServer.OnSelectionChanged(index, idl, path);
        }

        public void OnSettingsChanged(int iType) {
            pluginServer.OnSettingsChanged(iType);
        }

        public void OnTabAdded(int index, byte[] idl, string path) {
            pluginServer.OnTabAdded(index, idl, path);
        }

        public void OnTabChanged(int index, byte[] idl, string path) {
            pluginServer.OnTabChanged(index, idl, path);
        }

        public void OnTabRemoved(int index, byte[] idl, string path) {
            pluginServer.OnTabRemoved(index, idl, path);
        }

        public bool PluginInstantialized(string pluginID) {
            return dicPluginInstances.ContainsKey(pluginID);
        }

        public void PushBackgroundMultiple(string pluginID, int i) {
            List<int> list;
            if(dicMultiplePluginOrders.TryGetValue(pluginID, out list)) {
                list.Add(i);
            }
            else {
                list = new List<int>();
                list.Add(i);
                dicMultiplePluginOrders[pluginID] = list;
            }
        }

        public void RefreshPluginAssembly(PluginAssembly pa, bool fStatic) {
            foreach(PluginInformation information in pa.PluginInformations) {
                if(!information.Enabled) {
                    UnloadPluginInstance(information.PluginID, EndCode.Unloaded, fStatic);
                    continue;
                }
                if(information.PluginType == PluginType.Background) {
                    if(!PluginInstantialized(information.PluginID)) {
                        Plugin plugin = Load(information, pa);
                        if(plugin != null) {
                            if(plgIFilter == null) {
                                plgIFilter = plugin.Instance as IFilter;
                            }
                            if(plgFilterCore == null) {
                                plgFilterCore = plugin.Instance as IFilterCore;
                            }
                        }
                        else {
                            information.Enabled = false;
                        }
                    }
                    continue;
                }
                if(information.PluginType == PluginType.BackgroundMultiple) {
                    if(!PluginInstantialized(information.PluginID) && (Load(information, pa) == null)) {
                        information.Enabled = false;
                    }
                }
                else if((fStatic && (information.PluginType == PluginType.Static)) && !dicStaticPluginInstances.ContainsKey(information.PluginID)) {
                    LoadStatics(information, pa, false);
                }
            }
        }

        public void RegisterMenu(IPluginClient pluginClient, MenuType menuType, string menuText, bool fRegister) {
            foreach(Plugin plugin in dicPluginInstances.Values.Where(plugin => plugin.Instance == pluginClient)) {
                if(fRegister) {
                    if((menuType & MenuType.Bar) == MenuType.Bar) {
                        dicFullNamesMenuRegistered_Sys[plugin.PluginInformation.PluginID] = menuText;
                    }
                    if((menuType & MenuType.Tab) == MenuType.Tab) {
                        dicFullNamesMenuRegistered_Tab[plugin.PluginInformation.PluginID] = menuText;
                    }
                }
                else {
                    if((menuType & MenuType.Bar) == MenuType.Bar) {
                        dicFullNamesMenuRegistered_Sys.Remove(plugin.PluginInformation.PluginID);
                    }
                    if((menuType & MenuType.Tab) == MenuType.Tab) {
                        dicFullNamesMenuRegistered_Tab.Remove(plugin.PluginInformation.PluginID);
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
                    QTUtility2.WriteRegBinary(lstPluginButtonsOrder.ToArray(), "Buttons_Order", key);
                }
            }
        }

        public static void SavePluginShortcutKeys() {
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar\Plugins")) {
                QTUtility2.WriteRegBinary(QTUtility.dicPluginShortcutKeys.Keys
                    .Select(str => new PluginKey(str, QTUtility.dicPluginShortcutKeys[str])).ToArray(),
                    "ShortcutKeys", key);
            }
        }

        public bool TryGetMultipleOrder(string pluginID, out List<int> order) {
            return dicMultiplePluginOrders.TryGetValue(pluginID, out order);
        }

        public bool TryGetPlugin(string pluginID, out Plugin plugin) {
            return dicPluginInstances.TryGetValue(pluginID, out plugin);
        }

        public void UninstallPluginAssembly(PluginAssembly pa, bool fStatic) {
            foreach(PluginInformation information in pa.PluginInformations) {
                UnloadPluginInstance(information.PluginID, EndCode.Removed, fStatic);
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
                QTUtility.PluginShortcutKeysCache = (QTUtility.dicPluginShortcutKeys.Values
                        .Where(numArray => numArray != null)
                        .SelectMany(numArray => numArray)).ToArray();
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
            dicFullNamesMenuRegistered_Sys.Remove(pluginID);
            dicFullNamesMenuRegistered_Tab.Remove(pluginID);
            if(dicPluginInstances.TryGetValue(pluginID, out plugin)) {
                pluginServer.RemoveEvents(plugin.Instance);
                dicPluginInstances.Remove(pluginID);
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

        public static IEncodingDetector IEncodingDetector {
            get {
                return plgEncodingDetector;
            }
        }

        public IFilter IFilter {
            get {
                return plgIFilter;
            }
        }

        public IFilterCore IFilterCore {
            get {
                return plgFilterCore;
            }
        }

        public static List<PluginAssembly> PluginAssemblies {
            get {
                return new List<PluginAssembly>(dicPluginAssemblies.Values);
            }
        }

        public static IEnumerable<PluginInformation> PluginInformations {
            get {
                return dicPluginAssemblies.Values.SelectMany(pa => pa.PluginInformations);
            }
        }

        public IEnumerable<Plugin> Plugins {
            get {
                return new List<Plugin>(dicPluginInstances.Values);
            }
        }

        public bool SelectionChangeAttached {
            get {
                return pluginServer.SelectionChangedAttached;
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
