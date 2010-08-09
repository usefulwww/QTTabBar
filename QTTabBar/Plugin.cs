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
using QTPlugin;

namespace QTTabBarLib {
    internal sealed class Plugin {
        private bool fBackgroundButtonIsEnabled;
        private bool fBackgroundButtonIsSupported;
        private IPluginClient pluginClient;
        private PluginInformation pluginInfo;

        public Plugin(IPluginClient pluginClient, PluginInformation pluginInfo) {
            this.pluginClient = pluginClient;
            this.pluginInfo = pluginInfo;
            fBackgroundButtonIsSupported = ((pluginInfo.PluginType == PluginType.Background) && ((pluginClient is IBarButton) || (pluginClient is IBarCustomItem))) || ((pluginInfo.PluginType == PluginType.BackgroundMultiple) && (pluginClient is IBarMultipleCustomItems));
        }

        public void Close(EndCode code) {
            if(pluginClient != null) {
                try {
                    pluginClient.Close(code);
                }
                catch(Exception exception) {
                    PluginManager.HandlePluginException(exception, IntPtr.Zero, pluginInfo.Name, "Closing plugin.");
                }
                pluginClient = null;
            }
            pluginInfo = null;
        }

        public bool BackgroundButtonEnabled {
            get {
                return (fBackgroundButtonIsSupported && fBackgroundButtonIsEnabled);
            }
            set {
                if(fBackgroundButtonIsSupported) {
                    fBackgroundButtonIsEnabled = value;
                }
            }
        }

        public bool BackgroundButtonSupported {
            get {
                return fBackgroundButtonIsSupported;
            }
        }

        public IPluginClient Instance {
            get {
                return pluginClient;
            }
        }

        public PluginInformation PluginInformation {
            get {
                return pluginInfo;
            }
        }
    }
}
