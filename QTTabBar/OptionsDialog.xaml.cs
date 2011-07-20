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
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    /// <summary>
    /// Interaction logic for OptionsDialog.xaml
    /// </summary>
    public partial class OptionsDialog : IDisposable {

        private static OptionsDialog instance;
        private static Thread instanceThread;
        public Config workingConfig { get; set; }

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

        private OptionsDialog() {
            workingConfig = QTUtility2.DeepClone(ConfigManager.LoadedConfig);
            InitializeComponent();
        }

        public void Dispose() {
            // TODO
        }

        private void UpdateOptions() {
            ConfigManager.LoadedConfig = QTUtility2.DeepClone(workingConfig);
            QTTabBarClass tabBar = InstanceManager.CurrentTabBar;
            if(tabBar != null) {
                tabBar.Invoke(new Action(tabBar.RefreshOptions));
            }
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

        private void BtnResetAll_Click(object sender, RoutedEventArgs e) {
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
    }

    public class BoolInverterConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value is bool ? !(bool)value : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return value is bool ? !(bool)value : value;
        }
    }

    public class RadioBoolMultiConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            return (parameter as string) == values.StringJoin(",");
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            return (parameter as string).Split(',').Select(s => (object)bool.Parse(s)).ToArray();
        }
    }

    [ValueConversion(typeof(Bitmap), typeof(ImageSource))]
    public class BitmapToImageSourceConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(!(value is Bitmap)) return null;
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
            RadioButtonEx rbx = (RadioButtonEx)d;
            rbx.bIsChanging = true;
            rbx.IsChecked = (bool?)e.NewValue;
            rbx.bIsChanging = false;
        }
    }
}
