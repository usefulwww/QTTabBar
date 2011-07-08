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
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace QTTabBarLib {
    /// <summary>
    /// Interaction logic for OptionsDialog.xaml
    /// </summary>
    public partial class OptionsDialog : IDisposable {

        private static OptionsDialog instance;
        private static Thread instanceThread;

        public static void Open() {
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
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            TabItem[] tabs = {
                tabGeneral,
                tabTabs,
                tabWindow,
                tabTweaks,
                tabMouse,
                tabTooltips,
                tabAppearence,
                tabGroups,
                tabApps,
                tabKeys,
                tabButtonBar,
                tabPlugins,
                tabLanguage,
                tabHelp
            };
            foreach(var tabItem in tabs) {
                //tabItem.Height = 0;
                //tabItem.Width = 2;
            }
            //tabControl1.TabStripPlacement = Dock.Bottom;
        }

        public void Dispose() {
            // TODO
        }
    }
}
