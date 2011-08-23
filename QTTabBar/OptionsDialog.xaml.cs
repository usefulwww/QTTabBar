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
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using QTTabBarLib.Interop;
using Binding = System.Windows.Data.Binding;
using Image = System.Drawing.Image;
using RadioButton = System.Windows.Controls.RadioButton;
using Size = System.Drawing.Size;

namespace QTTabBarLib {
    /// <summary>
    /// Interaction logic for OptionsDialog.xaml
    /// </summary>
    public partial class OptionsDialog : IDisposable {

        private static OptionsDialog instance;
        private static Thread instanceThread;
        public Config workingConfig { get; set; }

        // Button bar stuff
        private ImageStrip imageStripLarge;
        private ImageStrip imageStripSmall;
        private string[] ButtonItemsDisplayName;
        private ObservableCollection<ButtonEntry> ButtonPool;
        private ObservableCollection<ButtonEntry> CurrentButtons;

        // Plugin stuff
        private PluginManager pluginManager;
        private ObservableCollection<PluginEntry> CurrentPlugins;

        // todo: localize
        private static Dictionary<TabPos, string> NewTabPosItems = new Dictionary<TabPos, string> {
            {TabPos.Left,       "Left of the current tab"   },
            {TabPos.Right,      "Right of the current tab"  },
            {TabPos.Leftmost,   "In the leftmost position"  },
            {TabPos.Rightmost,  "In the rightmost position" },
        };
        private static Dictionary<TabPos, string> NextAfterCloseItems = new Dictionary<TabPos, string> {
            {TabPos.Left,       "Tab to the left"   },
            {TabPos.Right,      "Tab to the right"  },
            {TabPos.Leftmost,   "Leftmost tab"      },
            {TabPos.Rightmost,  "Rightmost tab"     },
            {TabPos.LastActive, "Last activated tab"},
        };

        #region ---------- Static Methods ----------

        public static void Open() {
            // TODO: Primary process only
            lock(typeof(OptionsDialog)) {
                if(instance == null) {
                    instanceThread = new Thread(ThreadEntry);
                    instanceThread.SetApartmentState(ApartmentState.STA);
                    lock(instanceThread) {
                        instanceThread.Start();
                        // Don't return until we know that instance is set.
                        Monitor.Wait(instanceThread);
                    }
                }
                else {
                    instance.Dispatcher.Invoke(new Action(() => {
                        if(instance.WindowState == WindowState.Minimized) {
                            instance.WindowState = WindowState.Normal;
                        }
                        else {
                            instance.Activate();
                        }
                    }));
                }
            }
        }

        public static void ForceClose() {
            lock(typeof(OptionsDialog)) {
                if(instance != null) {
                    instance.Dispatcher.Invoke(new Action(() => instance.Close()));
                }
            }
        }

        private static void ThreadEntry() {
            using(instance = new OptionsDialog()) {
                lock(instanceThread) {
                    Monitor.Pulse(instanceThread);
                }
                instance.ShowDialog();
            }
            lock(typeof(OptionsDialog)) {
                instance = null;
            }
        }

        #endregion

        private OptionsDialog() {
            QTTabBarClass tabBar = InstanceManager.CurrentTabBar;
            if(tabBar != null) {
                // we should probably assert this.
                pluginManager = tabBar.GetPluginManager();    
            }
            workingConfig = QTUtility2.DeepClone(ConfigManager.LoadedConfig);
            InitializeComponent();
            cmbNewTabPos.ItemsSource = NewTabPosItems;
            cmbNextAfterClosed.ItemsSource = NextAfterCloseItems;

            // Initialize the button bar tab.
            // todo: options, localize, etc...
            ButtonItemsDisplayName = QTUtility.TextResourcesDic["ButtonBar_BtnName"];
            imageStripLarge = new ImageStrip(new Size(24, 24));
            using(Bitmap b = Resources_Image.ButtonStrip24) {
                imageStripLarge.AddStrip(b);
            }
            imageStripSmall = new ImageStrip(new Size(16, 16));
            using(Bitmap b = Resources_Image.ButtonStrip16) {
                imageStripSmall.AddStrip(b);
            }
            ButtonPool = new ObservableCollection<ButtonEntry>();
            CurrentButtons = new ObservableCollection<ButtonEntry>();
            ButtonPool.Add(new ButtonEntry(this, QTButtonBar.BII_SEPARATOR));
            for(int i = 1; i < QTButtonBar.INTERNAL_BUTTON_COUNT; i++) {
                if(!workingConfig.bbar.ButtonIndexes.Contains(i)) {
                    ButtonPool.Add(new ButtonEntry(this, i));
                }
            }
            foreach(int i in workingConfig.bbar.ButtonIndexes) {
                CurrentButtons.Add(new ButtonEntry(this, i));
            }
            lstButtonBarPool.ItemsSource = ButtonPool;
            lstButtonBarCurrent.ItemsSource = CurrentButtons;

            // Initialize the plugin tab
            CurrentPlugins = new ObservableCollection<PluginEntry>();
            foreach(PluginAssembly assembly in PluginManager.PluginAssemblies) {
                CreatePluginEntry(assembly, false);
            }
            lstPluginView.ItemsSource = CurrentPlugins;
        }

        public void Dispose() {
            // TODO
        }

        private void UpdateOptions() {
            List<PluginAssembly> assemblies = CommitPlugins();
            workingConfig.bbar.ButtonIndexes = CurrentButtons.Select(e => e.Index).ToArray();
            bool fButtonBarNeedsRefresh = Config.BBar.LargeButtons != workingConfig.bbar.LargeButtons;
            ConfigManager.LoadedConfig = QTUtility2.DeepClone(workingConfig);

            PluginManager.SavePluginAssemblies();
            ConfigManager.WriteConfig();
            QTTabBarClass tabBar = InstanceManager.CurrentTabBar;
            if(tabBar != null) {
                tabBar.Invoke(new Action(tabBar.RefreshOptions));
                tabBar.Invoke(new Action(() => tabBar.odCallback_ManagePlugin(assemblies)));
            }
            QTButtonBar.BroadcastConfigChanged(fButtonBarNeedsRefresh);
        }

        private void ListBoxItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            lstCategories.Focus();
            e.Handled = true;
        }

        private void ListBoxItem_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            ((ListBoxItem)sender).Focus();
            ((ListBoxItem)sender).IsSelected = true;
            e.Handled = true;
        }

        private void btnResetPage_Click(object sender, RoutedEventArgs e) {
            // todo: confirm
            TabItem tab = ((TabItem)tabbedPanel.SelectedItem);
            BindingExpression expr = tab.GetBindingExpression(DataContextProperty);
            if(expr != null) {
                PropertyInfo prop = typeof(Config).GetProperty(expr.ParentBinding.Path.Path);
                object c = Activator.CreateInstance(prop.PropertyType);
                prop.SetValue(workingConfig, c, null);
                expr.UpdateTarget();
            }
            else {
                // todo
            }
        }

        private void btnResetAll_Click(object sender, RoutedEventArgs e) {
            // todo: confirm
            workingConfig = new Config();
            tabbedPanel.GetBindingExpression(DataContextProperty).UpdateTarget();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e) {
            UpdateOptions();
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void btnApply_Click(object sender, RoutedEventArgs e) {
            UpdateOptions();
        }

        #region ---------- Button Bar ----------

        private void btnBBarAdd_Click(object sender, RoutedEventArgs e) {
            int sel = lstButtonBarPool.SelectedIndex;
            if(sel == -1) return;
            int idx = ButtonPool[sel].Index;
            if(idx != QTButtonBar.BII_SEPARATOR) {
                ButtonPool.RemoveAt(sel);
                if(sel == ButtonPool.Count) --sel;
                if(sel >= 0) {
                    lstButtonBarPool.SelectedIndex = sel;
                    lstButtonBarPool.ScrollIntoView(lstButtonBarPool.SelectedItem);
                }
            }
            if(lstButtonBarCurrent.SelectedIndex == -1) {
                CurrentButtons.Add(new ButtonEntry(this, idx));
                lstButtonBarCurrent.SelectedIndex = CurrentButtons.Count - 1;
            }
            else {
                CurrentButtons.Insert(lstButtonBarCurrent.SelectedIndex + 1, new ButtonEntry(this, idx));
                lstButtonBarCurrent.SelectedIndex++;
            }
            lstButtonBarCurrent.ScrollIntoView(lstButtonBarCurrent.SelectedItem);
        }

        private void btnBBarRemove_Click(object sender, RoutedEventArgs e) {
            int sel = lstButtonBarCurrent.SelectedIndex;
            if(sel == -1) return;
            int idx = CurrentButtons[sel].Index;
            CurrentButtons.RemoveAt(sel);
            if(sel == CurrentButtons.Count) --sel;
            if(sel >= 0) {
                lstButtonBarCurrent.SelectedIndex = sel;
                lstButtonBarCurrent.ScrollIntoView(lstButtonBarCurrent.SelectedItem);
            }
            if(idx != QTButtonBar.BII_SEPARATOR) {
                int i = 0;
                while(i < ButtonPool.Count && ButtonPool[i].Index < idx) ++i;
                ButtonPool.Insert(i, new ButtonEntry(this, idx));
                lstButtonBarPool.SelectedIndex = i;
            }
            else {
                lstButtonBarPool.SelectedIndex = 0;
            }
            lstButtonBarPool.ScrollIntoView(lstButtonBarPool.SelectedItem);
        }

        private void btnBBarUp_Click(object sender, RoutedEventArgs e) {
            int sel = lstButtonBarCurrent.SelectedIndex;
            if(sel <= 0) return;
            CurrentButtons.Move(sel, sel - 1);
            lstButtonBarCurrent.ScrollIntoView(lstButtonBarCurrent.SelectedItem);
        }

        private void btnBBarDown_Click(object sender, RoutedEventArgs e) {
            int sel = lstButtonBarCurrent.SelectedIndex;
            if(sel == -1 || sel == CurrentButtons.Count - 1) return;
            CurrentButtons.Move(sel, sel + 1);
            lstButtonBarCurrent.ScrollIntoView(lstButtonBarCurrent.SelectedItem);
        }

        #endregion

        #region ---------- Plugins ----------

        private void btnPluginOptions_Click(object sender, RoutedEventArgs e) {
            if(lstPluginView.SelectedIndex == -1) return;
            PluginEntry entry = CurrentPlugins[lstPluginView.SelectedIndex];
            Plugin p;
            if(pluginManager.TryGetPlugin(entry.PluginInfo.PluginID, out p) && p.Instance != null) {
                try {
                    p.Instance.OnOption();
                }
                catch(Exception ex) {
                    PluginManager.HandlePluginException(ex, new WindowInteropHelper(this).Handle, 
                            entry.PluginInfo.Name, "Open plugin option.");
                }
            }
        }

        private void btnPluginDisable_Click(object sender, RoutedEventArgs e) {
            if(lstPluginView.SelectedIndex == -1) return;
            PluginEntry entry = CurrentPlugins[lstPluginView.SelectedIndex];
            if(entry.DisableOnClose) {
                entry.DisableOnClose = false;
            }
            else if(entry.EnableOnClose) {
                entry.EnableOnClose = false;
            }
            else if(entry.PluginInfo.Enabled) {
                entry.DisableOnClose = true;
            }
            else {
                entry.EnableOnClose = true;
            }
            lstPluginView.Items.Refresh();
        }
        
        private void btnPluginRemove_Click(object sender, RoutedEventArgs e) {
            if(lstPluginView.SelectedIndex == -1) return;
            PluginEntry entry = CurrentPlugins[lstPluginView.SelectedIndex];
            if(entry.InstallOnClose) {
                entry.PluginAssembly.Dispose();
                CurrentPlugins.RemoveAt(lstPluginView.SelectedIndex);
            }
            else {
                entry.UninstallOnClose = true;
                lstPluginView.Items.Refresh();
            }
        }

        private void btnBrowsePlugin_Click(object sender, RoutedEventArgs e) {
            using(OpenFileDialog ofd = new OpenFileDialog()) {
                ofd.Filter = "Plugin files (*.dll)|*.dll";
                ofd.RestoreDirectory = true;
                ofd.Multiselect = true;

                if(System.Windows.Forms.DialogResult.OK != ofd.ShowDialog()) return;
                bool fFirst = true;
                foreach(string path in ofd.FileNames) {
                    PluginAssembly pa = new PluginAssembly(path);
                    if(!pa.PluginInfosExist) continue;
                    CreatePluginEntry(pa, true);
                    if(!fFirst) continue;
                    fFirst = false;
                    lstPluginView.SelectedItem = CurrentPlugins[CurrentPlugins.Count - 1];
                    lstPluginView.ScrollIntoView(lstPluginView.SelectedItem);
                }
            }
        }

        private bool CreatePluginEntry(PluginAssembly pa, bool fAddedByUser) {
            if(!pa.PluginInfosExist || CurrentPlugins.Any(pe => pe.PluginInfo.Path == pa.Path)) {
                return false;
            }
            foreach(PluginInformation pi in pa.PluginInformations) {
                PluginEntry entry = new PluginEntry(this, pi, pa) { InstallOnClose = fAddedByUser };
                int i = 0;
                while(i < CurrentPlugins.Count && string.Compare(CurrentPlugins[i].Title, entry.Title, true) <= 0) ++i;
                CurrentPlugins.Insert(i, entry);
            }
            return true;
        }

        private List<PluginAssembly> CommitPlugins() {
            List<PluginAssembly> assemblies = new List<PluginAssembly>();

            // Don't dispose the assemblies here.  That will be done by the plugin manager
            // when the plugins are unloaded.
            for(int i = 0; i < CurrentPlugins.Count; ++i) {
                if(CurrentPlugins[i].UninstallOnClose) {
                    CurrentPlugins.RemoveAt(i);
                    --i;
                }
            }

            List<string> enabled = new List<string>();
            foreach(PluginEntry entry in CurrentPlugins) {
                PluginAssembly pa = entry.PluginAssembly;
                if(!assemblies.Contains(pa)) {
                    pa.Enabled = false;
                    assemblies.Add(pa);
                }
                PluginInformation pi = entry.PluginInfo;
                if(entry.DisableOnClose) {
                    pi.Enabled = false;
                    //RemovePluginShortcutKeys(pi.PluginID);
                    // todo
                }
                else if(entry.EnableOnClose || entry.InstallOnClose) {
                    pi.Enabled = true;
                }
                entry.EnableOnClose = entry.DisableOnClose = entry.InstallOnClose = false;

                if(pi.Enabled) {
                    pa.Enabled = true;
                    enabled.Add(pi.PluginID);
                }
            }
            workingConfig.plugin.Enabled = enabled.ToArray();
            lstPluginView.Items.Refresh();
            return assemblies;
        }

        #endregion
        
        #region ---------- ListBox Item Classes ----------

        private class ButtonEntry {
            private OptionsDialog parent;
            public int Index { get; private set; }
            public string Text { get { return parent.ButtonItemsDisplayName[Index]; } }
            public Bitmap LargeImage { get { return getImage(true); } }
            public Bitmap SmallImage { get { return getImage(false); } }
            private Bitmap getImage(bool large) {
                return Index == 0 || Index >= QTButtonBar.BII_WINDOWOPACITY ? null : large
                        ? parent.imageStripLarge[Index - 1]
                        : parent.imageStripSmall[Index - 1];
            }
            public ButtonEntry(OptionsDialog parent, int Index) {
                this.parent = parent;
                this.Index = Index;
            }
        }

        private class PluginEntry {
            private OptionsDialog parent;
            public PluginInformation PluginInfo;
		    public PluginAssembly PluginAssembly;

            public Image Icon { get { return PluginInfo.ImageLarge; } }
            public string Title { get { return PluginInfo.Name + "  " + PluginInfo.Version; } }
            public string Author { get { return "by " + PluginInfo.Author; } }
            public string Desc { get { return PluginInfo.Description; } }
            public bool IsSelected { get; set; }
            public double Opacity { get { return PluginInfo.Enabled ? 1.0 : 0.5; } }
            public bool DisableOnClose { get; set; }
            public bool EnableOnClose { get; set; }
            public bool InstallOnClose { get; set; }
            public bool UninstallOnClose { get; set; }

            private bool cachedHasOptions;
            private bool optionsQueried;

            public bool HasOptions {
                get {
                    // TODO: reset when appropriate.
                    if(optionsQueried) return cachedHasOptions;
                    optionsQueried = true;
                    Plugin p;
                    if(parent.pluginManager.TryGetPlugin(PluginInfo.PluginID, out p)) {
                        try {
                            cachedHasOptions = p.Instance.HasOption;
                            return cachedHasOptions;
                        }
                        catch(Exception ex) {
                            PluginManager.HandlePluginException(ex, new WindowInteropHelper(parent).Handle, PluginInfo.Name,
                                    "Checking if the plugin has options.");
                        }
                    }
                    return false;
                }
            } 

            public PluginEntry(OptionsDialog parent, PluginInformation pluginInfo, PluginAssembly pluginAssembly) {
                this.parent = parent;
                PluginInfo = pluginInfo;
                PluginAssembly = pluginAssembly;
            }
        }

        #endregion
    }
        
    #region ---------- Converters ----------

    // Inverts the value of a boolean
    public class BoolInverterConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value is bool ? !(bool)value : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return value is bool ? !(bool)value : value;
        }
    }

    // Converts between booleans and one using logical and.
    public class LogicalAndMultiConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            return values.All(b => b is bool && (bool)b);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            return new object[] { value };
        }
    }

    // Converts between many booleans and a string by StringJoining them.
    public class BoolJoinMultiConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            return values.StringJoin(",");
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            return ((string)value).Split(',').Select(s => (object)bool.Parse(s)).ToArray();
        }
    }

    // Converts between a boolean and a string by comparing the string to the 
    // passed parameter.
    [ValueConversion(typeof(string), typeof(bool))]
    public class StringEqualityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return (string)parameter == (string)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return (bool)value ? parameter : Binding.DoNothing;
        }
    }

    // Converts Bitmaps to ImageSources.
    [ValueConversion(typeof(Bitmap), typeof(ImageSource))]
    public class BitmapToImageSourceConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value == null || !(value is Bitmap)) return null;
            IntPtr hBitmap = ((Bitmap)value).GetHbitmap();
            try {
                return Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero,
                        Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally {
                PInvoke.DeleteObject(hBitmap);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }

    #endregion

    // Overloaded RadioButton class to work around .NET 3.5's horribly HORRIBLY
    // bugged RadioButton data binding.
    public class RadioButtonEx : RadioButton {

        private bool bIsChanging;

        public RadioButtonEx() {
            Checked += RadioButtonExtended_Checked;
            Unchecked += RadioButtonExtended_Unchecked;
        }

        void RadioButtonExtended_Unchecked(object sender, RoutedEventArgs e) {
            if(!bIsChanging) IsCheckedReal = false;
        }

        void RadioButtonExtended_Checked(object sender, RoutedEventArgs e) {
            if(!bIsChanging) IsCheckedReal = true;
        }

        public bool? IsCheckedReal {
            get { return (bool?)GetValue(IsCheckedRealProperty); }
            set { SetValue(IsCheckedRealProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsCheckedReal.
        // This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsCheckedRealProperty =
                DependencyProperty.Register("IsCheckedReal", typeof(bool?), typeof(RadioButtonEx),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Journal |
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsCheckedRealChanged));

        private static void OnIsCheckedRealChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            RadioButtonEx rbx = ((RadioButtonEx)d);
            rbx.bIsChanging = true; 
            rbx.IsChecked = (bool?)e.NewValue;
            rbx.bIsChanging = false;
        }
    }
}
