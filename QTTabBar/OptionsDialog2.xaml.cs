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

using System.Windows;
using System.Windows.Controls;

namespace QTTabBarLib {
    /// <summary>
    /// Interaction logic for OptionsDialog2.xaml
    /// </summary>
    public partial class OptionsDialog2 : Window {
        public OptionsDialog2() {
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
    }
}
